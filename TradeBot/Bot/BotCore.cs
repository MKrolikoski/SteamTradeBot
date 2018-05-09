using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using SteamKit2;
using Newtonsoft.Json;
using TradeBot.Messages;
using SteamToolkit.Trading;
using System.Collections.Generic;
using TradeBot.Bitstamp;
using SteamAuth;
using TradeBot.Database;
using TradeBot.Entity;
using System.Text;

namespace TradeBot.Bot
{
    class BotCore
    {
        private CallbackManager callbackManager;

        private EconServiceHandler offerHandler;
        private MarketHandler marketHandler;
        private MessageHandler messageHandler;
        private BitstampHandler bitstampHandler;
        private DatabaseHandler databaseHandler;

        private SteamClient steamClient;
        private SteamUser steamUser;
        private SteamFriends steamFriends;
        private SteamID steamID;

        #region steamguard
        private SteamGuardAccount steamGuardAccount;
        #endregion

        private string authCode, steamGuardCode;

        private Inventory steamInventory;
        private BotConfig config;

        private Thread tradeOfferThread;

        public BotCore()
        {
            config = new BotConfig();
            if (!File.Exists("config.cfg"))
            {
                config.createNew();
                if(File.Exists("sentry.bin"))
                    File.Delete("sentry.bin");
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
            bitstampHandler = new BitstampHandler();
            databaseHandler = new DatabaseHandler();

            // get the steamuser handler, which is used for logging on after successfully connecting
            steamUser = steamClient.GetHandler<SteamUser>();

            steamFriends = steamClient.GetHandler<SteamFriends>();

            steamGuardAccount = new SteamGuardAccount();

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
                TwoFactorCode = steamGuardCode,

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

            //SteamGuard enabled -> need to generate code
            if (isSteamGuard || is2FA)
            {
                Console.WriteLine("This account is SteamGuard protected!");

                if (is2FA)
                {
                    if(config.shared_secret.Equals(""))
                    {
                        Console.Write("Please enter SteamGuard code from you authentication device: ");
                        steamGuardCode = Console.ReadLine();
                    }
                    else
                    {
                        Console.WriteLine("Generating SteamGuard code..");
                        steamGuardAccount.SharedSecret = config.shared_secret;
                        steamGuardCode = steamGuardAccount.GenerateSteamGuardCode();
                    }
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
            if (authCode != null)
            {
                Console.WriteLine("Updating sentryfile...");

                // write out our sentry file
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
                    steamFriends.SendChatMessage(message.from, EChatEntryType.ChatMsg, "Available commands: \n!help, \n!sell, \n!buy, \n!setethaddress, \n!info, \n!confirm"); break;
                case MessageType.SELL:
                    if (createTransaction(message.from, message.parameters, message.messageType))
                        steamFriends.SendChatMessage(message.from, EChatEntryType.ChatMsg, "Transaction added succesfully.");
                    break;
                case MessageType.BUY:
                    if(createTransaction(message.from, message.parameters, message.messageType))
                        steamFriends.SendChatMessage(message.from, EChatEntryType.ChatMsg, "Transaction added succesfully.");
                    break;
                case MessageType.SETETHADDRESS:
                    if(databaseHandler.setEthAddress(message.from.ToString(), message.parameters[0]))
                    {
                        string msg = "Successfully changed ETH address to: " + message.parameters[0];
                        steamFriends.SendChatMessage(message.from, EChatEntryType.ChatMsg, msg);
                    }
                    else
                    {
                        steamFriends.SendChatMessage(message.from, EChatEntryType.ChatMsg, "Error while changing ETH address.");
                    }
                    break;
                case MessageType.CONFIRM:
                    //add method to handle transaction confirmation
                    if(databaseHandler.ConfirmTransaction(message.from.ToString()))
                    {
                        steamFriends.SendChatMessage(message.from, EChatEntryType.ChatMsg, "Transaction confirmed.");
                    }
                    else
                    {
                        steamFriends.SendChatMessage(message.from, EChatEntryType.ChatMsg, "No transaction to confirm.\nTo add a transaction use !sell or !buy commands.");
                    }
                    break;
                case MessageType.INFO:
                    printInfo(message.from); break;
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

        private void printInfo(SteamID steamID)
        {
            User user = databaseHandler.GetUser(steamID.ToString());
            if(!user.WalletAddress.Equals(""))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Your ETH address: ");
                sb.Append(user.WalletAddress);
                sb.AppendLine();
                Transaction transaction = databaseHandler.GetUserTransaction(steamID.ToString());
                if (transaction != null)
                {
                    Tradeoffer offer = databaseHandler.GetUserTradeOffer(steamID.ToString());
                    sb.Append("CURRENT TRANSACTION\nType: ");
                    if (transaction.Sell)
                        sb.Append("SELL");
                    else
                        sb.Append("BUY");
                    sb.AppendLine();
                    sb.Append("Created: ");
                    sb.Append(transaction.CreationDate.ToString("MM/dd/yyyy"));
                    sb.AppendLine();
                    sb.Append("Status: ");
                    if (transaction.Confirmed)
                        sb.Append("Status: CONFIRMED");
                    else
                        sb.Append("NOT CONFIRMED");
                    sb.AppendLine();
                    sb.Append("Number of keys: ");
                    sb.Append(offer.Amount);
                    sb.AppendLine();
                    sb.Append("ETH value: ");
                    //TODO
                    sb.Append("TODO");
                    sb.AppendLine();

                    steamFriends.SendChatMessage(steamID, EChatEntryType.ChatMsg, sb.ToString());
                }
                else
                {
                    steamFriends.SendChatMessage(steamID, EChatEntryType.ChatMsg, "No pending transaction.\nTo add a transaction use !sell or !buy commands.");
                }
            }
            else
            {
                steamFriends.SendChatMessage(steamID, EChatEntryType.ChatMsg, "Please set your ETH address with !setethaddress comand.");
            }
        }

        private bool createTransaction(SteamID steamID, List<string> parameters, MessageType transactionType)
        {
            try
            {
                User user = databaseHandler.GetUser(steamID.ToString());
                if (user.WalletAddress.Equals(""))
                {
                    steamFriends.SendChatMessage(steamID, EChatEntryType.ChatMsg, "Please set your ETH address with !setethaddress comand.");
                    return false;
                }
                //delete previous transaction if exists
                if (databaseHandler.GetUserTransaction(user.SteamID) != null)
                {
                    databaseHandler.DeleteUserTransaction(user.SteamID);
                }
                //TODO      
                double costPerOne = 1.0;
                //
                Transaction transaction = new Transaction(user.UserID, DateTime.Now, false, false, false);
                if (transactionType == MessageType.BUY)
                    transaction.Buy = true;
                else
                    transaction.Sell = true;
                databaseHandler.AddTransaction(transaction);
                transaction = databaseHandler.GetUserTransaction(user.SteamID);

                Tradeoffer tradeoffer = new Tradeoffer(transaction.TransactionID, Convert.ToInt32(parameters[0]), costPerOne);
                databaseHandler.AddTradeOffer(tradeoffer);
            }catch(Exception e)
            {
                steamFriends.SendChatMessage(steamID, EChatEntryType.ChatMsg, "Error while adding transaction.");
                return false;
            }
            return true;
        }
    }
}

