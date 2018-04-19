using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using SteamKit2;
using Newtonsoft.Json;
using TradeBot.Messages;
using SteamToolkit;
using SteamToolkit.Trading;
using System.Collections.Generic;

namespace TradeBot.Bot
{
    class BotCore
    {
        private CallbackManager callbackManager;

        private EconServiceHandler offerHandler;
        private MarketHandler marketHandler;


        private SteamClient steamClient;
        private SteamUser steamUser;
        private SteamFriends steamFriends;


        private string authCode, twoFactorAuth;

        private SteamID steamID;

        private Inventory steamInventory;


        private BotConfig config;

        private MessageHandler messageHandler;

        private Thread tradeOfferThread;

        public BotCore()
        {
            config = new BotConfig();
            if (!File.Exists("config.cfg"))
            {
                config.createNew();
            }
            config = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("config.cfg"));

            if (config.login.Equals("") || config.password.Equals(""))
            {
                Console.Write("Username: ");
                config.login = Console.ReadLine();

                Console.Write("Password: ");
                config.password = Console.ReadLine();
            }


            steamClient = new SteamClient();

            // create the callback manager which will route callbacks to function calls
            callbackManager = new CallbackManager(steamClient);

            messageHandler = new MessageHandler();

            // get the steamuser handler, which is used for logging on after successfully connecting
            steamUser = steamClient.GetHandler<SteamUser>();

            steamFriends = steamClient.GetHandler<SteamFriends>();

            offerHandler = new EconServiceHandler(config.api_key);
            marketHandler = new MarketHandler();


        // register a few callbacks we're interested in
        // these are registered upon creation to a callback manager, which will then route the callbacks
        // to the functions specified
            callbackManager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            callbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            callbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);


            callbackManager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

            // this callback is triggered when the steam servers wish for the client to store the sentry file
            callbackManager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);

            callbackManager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);

            callbackManager.Subscribe<SteamFriends.FriendMsgCallback>(OnMessageReceived);




            messageHandler.MessageProcessedEvent += OnMessageProcessed;



            Console.WriteLine("Connecting to Steam...");

            // initiate the connection
            steamClient.Connect();

            config.working = true;

            // create our callback handling loop
            while (config.working)
            {
                // in order for the callbacks to get routed, they need to be handled by the manager
                callbackManager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
        }




        private void OnConnected(SteamClient.ConnectedCallback callback)
        {
            Console.WriteLine("Connected to Steam! Logging in...");

            byte[] sentryHash = null;
            if (File.Exists("sentry.bin"))
            {
                // if we have a saved sentry file, read and sha-1 hash it
                byte[] sentryFile = File.ReadAllBytes("sentry.bin");
                sentryHash = CryptoHelper.SHAHash(sentryFile);
            }

            steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = config.login,
                Password = config.password,


                // in this sample, we pass in an additional authcode
                // this value will be null (which is the default) for our first logon attempt
                AuthCode = authCode,

                // if the account is using 2-factor auth, we'll provide the two factor code instead
                // this will also be null on our first logon attempt
                TwoFactorCode = twoFactorAuth,

                // our subsequent logons use the hash of the sentry file as proof of ownership of the file
                // this will also be null for our first (no authcode) and second (authcode only) logon attempts
                SentryFileHash = sentryHash,
            });


        }

        private void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            // after recieving an AccountLogonDenied, we'll be disconnected from steam
            // so after we read an authcode from the user, we need to reconnect to begin the logon flow again

            Console.WriteLine("Disconnected from Steam, reconnecting in 5...");

            CancelTradeOfferPollingThread();

            Thread.Sleep(TimeSpan.FromSeconds(5));

            steamClient.Connect();
        }

        private void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            bool isSteamGuard = callback.Result == EResult.AccountLogonDenied;
            bool is2FA = callback.Result == EResult.AccountLoginDeniedNeedTwoFactor;

            if (isSteamGuard || is2FA)
            {
                Console.WriteLine("This account is SteamGuard protected!");

                if (is2FA)
                {
                    Console.Write("Please enter your 2 factor auth code from your authenticator app: ");
                    twoFactorAuth = Console.ReadLine();
                }
                else
                {
                    Console.Write("Please enter the auth code sent to the email at {0}: ", callback.EmailDomain);
                    authCode = Console.ReadLine();
                }

                return;
            }

            if (callback.Result != EResult.OK)
            {
                Console.WriteLine("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult);

                config.working = false;

                Console.WriteLine("Press any key to exit..");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Successfully logged on!");

            config.save();

            steamID = steamClient.SteamID;

            //730 - appID for CS:GO
            steamInventory = new Inventory(steamID, 730);

            //starts thread that handles tradeoffers
            SpawnTradeOfferPollingThread();




            // at this point, we'd be able to perform actions on Steam
        }

        private void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Console.WriteLine("Logged off of Steam: {0}", callback.Result);
            CancelTradeOfferPollingThread();
            config.working = false;
            config.save();
        }

        private void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            Console.WriteLine("Updating sentryfile...");

            // write out our sentry file
            // ideally we'd want to write to the filename specified in the callback
            // but then this sample would require more code to find the correct sentry file to read during logon
            // for the sake of simplicity, we'll just use "sentry.bin"

            int fileSize;
            byte[] sentryHash;
            using (var fs = File.Open("sentry.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fs.Seek(callback.Offset, SeekOrigin.Begin);
                fs.Write(callback.Data, 0, callback.BytesToWrite);
                fileSize = (int)fs.Length;

                fs.Seek(0, SeekOrigin.Begin);
                using (var sha = SHA1.Create())
                {
                    sentryHash = sha.ComputeHash(fs);
                }
            }

            // inform the steam servers that we're accepting this sentry file
            steamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                JobID = callback.JobID,

                FileName = callback.FileName,

                BytesWritten = callback.BytesToWrite,
                FileSize = fileSize,
                Offset = callback.Offset,

                Result = EResult.OK,
                LastError = 0,

                OneTimePassword = callback.OneTimePassword,

                SentryFileHash = sentryHash,
            });

            Console.WriteLine("Done!");
        }

        private void OnMessageReceived(SteamFriends.FriendMsgCallback callback)
        {
            if (callback.EntryType == EChatEntryType.ChatMsg)
            {
                messageHandler.processMessage(callback.Message, callback.Sender);
            }
        }

        private void OnMessageProcessed(object sender, EventArgs e)
        {
            Message message = (Message)e;
            switch (message.messageType)
            {
                case MessageType.HELP:
                    steamFriends.SendChatMessage(message.from, EChatEntryType.ChatMsg, "Available commands: \n!help, \n!sell, \n!buy, \n!changewalletaddress"); break;
                case MessageType.SELL:
                    steamFriends.SendChatMessage(message.from, EChatEntryType.ChatMsg, "Sell option coming soon.."); break;
                case MessageType.BUY:
                    steamFriends.SendChatMessage(message.from, EChatEntryType.ChatMsg, "Buy option coming soon.."); break;
                case MessageType.CHANGE_WALLET_ADDRESS:
                    steamFriends.SendChatMessage(message.from, EChatEntryType.ChatMsg, "Changewalletaddress option coming soon.."); break;
                default:
                    steamFriends.SendChatMessage(message.from, EChatEntryType.ChatMsg, "Unknown command. Type !help for the list of commands."); break;
            }
        }

        private void OnAccountInfo(SteamUser.AccountInfoCallback pData)
        {
            changeStatus();
        }

        private void changeStatus()
        {
            switch (config.status)
            {
                case BotStatus.ONLINE: steamFriends.SetPersonaState(EPersonaState.Online); break;
                case BotStatus.AWAY: steamFriends.SetPersonaState(EPersonaState.Away); break;
                case BotStatus.BUSY: steamFriends.SetPersonaState(EPersonaState.Busy); break;
                case BotStatus.LOOKINGTOTRADE: steamFriends.SetPersonaState(EPersonaState.LookingToTrade); break;
                default: steamFriends.SetPersonaState(EPersonaState.Offline); break;
            }
        }

        //TradeOffers
        private void SpawnTradeOfferPollingThread()
        {
            if (tradeOfferThread == null)
            {
                tradeOfferThread = new Thread(PollOffers);
                tradeOfferThread.Start();
            }
        }

        protected void CancelTradeOfferPollingThread()
        {
            tradeOfferThread = null;
        }

        private void PollOffers()
        {
            while (tradeOfferThread == Thread.CurrentThread)
            {
                Thread.Sleep(10000);

                //set parameters for data you want to receive
                var recData = new Dictionary<string, string>
                {
                    {"get_received_offers", "1"},
                    {"active_only", "1"},
                    {"time_historical_cutoff", "999999999999"}
                };

                var offers = offerHandler.GetTradeOffers(recData).TradeOffersReceived;

                Console.WriteLine("Number of items in CS:GO equipment: {0}", steamInventory.AssetCount().ToString());

                if (offers == null)
                    continue;

                Console.WriteLine("Pending offers:");
                foreach (CEconTradeOffer cEconTradeOffer in offers)
                {
                    Console.WriteLine("Offer from user: {0}", cEconTradeOffer.AccountIdOther.ToString());
                }
            }

        }
    }
}

