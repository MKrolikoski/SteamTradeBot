﻿using System;
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
using System.Globalization;

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

            callbackManager = new CallbackManager(steamClient);

            messageHandler = new MessageHandler();
            bitstampHandler = new BitstampHandler();
            databaseHandler = new DatabaseHandler();

            steamUser = steamClient.GetHandler<SteamUser>();

            steamFriends = steamClient.GetHandler<SteamFriends>();

            steamGuardAccount = new SteamGuardAccount();

            offerHandler = new EconServiceHandler(config.api_key);
            marketHandler = new MarketHandler();

            callbackManager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);

            callbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            callbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);

            callbackManager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

            callbackManager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);

            callbackManager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);

            callbackManager.Subscribe<SteamFriends.FriendMsgCallback>(OnMessageReceived);

            callbackManager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendsList);

            callbackManager.Subscribe<SteamFriends.FriendAddedCallback>(OnFriendAdded);

            messageHandler.MessageProcessedEvent += OnMessageProcessed;

            Console.WriteLine("Connecting to Steam...");

            steamClient.Connect();

            config.working = true;

            while (config.working)
            {
                callbackManager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
        }



        private void OnConnected(SteamClient.ConnectedCallback callback)
        {
            Console.WriteLine("Connected to Steam! Logging in...");

            byte[] sentryHash = null;
            if (File.Exists("sentry.bin"))
            {
                byte[] sentryFile = File.ReadAllBytes("sentry.bin");
                sentryHash = CryptoHelper.SHAHash(sentryFile);
            }

            steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = config.login,
                Password = config.password,
                AuthCode = authCode,
                TwoFactorCode = steamGuardCode,
                SentryFileHash = sentryHash,
            });


        }

        private void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
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
                    //TODELETE
                    Transaction transasaction = databaseHandler.GetUserTransaction(message.from.ToString());
                    if (transasaction != null)
                        databaseHandler.DeleteTransaction(transasaction);
                    User user = databaseHandler.GetUser(message.from.ToString());
                    if (user == null)
                    {
                        user = new User(message.from.ToString(), "");
                        databaseHandler.AddUser(user);
                    }
                    //
                    steamFriends.SendChatMessage(message.from, EChatEntryType.ChatMsg, "Available commands: \n!help, \n!sell [number_of_keys], \n!buy [number_of_keys], \n!setethaddress [eth_address], \n!info, \n!confirm"); break;
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
                    confirmTransaction(message.from);
                    break;
                case MessageType.INFO:
                    printInfo(message.from); break;
                case MessageType.BADPARAMS:
                    steamFriends.SendChatMessage(message.from, EChatEntryType.ChatMsg, "Bad arguments. Type !help for the list of commands and their arguments."); break;
                default:
                    steamFriends.SendChatMessage(message.from, EChatEntryType.ChatMsg, "Unknown command. Type !help for the list of commands and their arguments."); break;
            }
        }

        private void OnAccountInfo(SteamUser.AccountInfoCallback pData)
        {
            changeStatus();
        }

        private void OnFriendsList(SteamFriends.FriendsListCallback callback)
        {
            foreach(var friend in callback.FriendList)
            {
                if (friend.Relationship == EFriendRelationship.RequestRecipient)
                {
                    steamFriends.AddFriend(friend.SteamID);
                }
            }
        }

        private void OnFriendAdded(SteamFriends.FriendAddedCallback callback)
        {
            steamFriends.SendChatMessage(callback.SteamID, EChatEntryType.ChatMsg, "Welcome. Type !help for the list of commands.");
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
                //Console.WriteLine("Number of items in CS:GO equipment: {0}", steamInventory.AssetCount().ToString());
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
            StringBuilder response = new StringBuilder();
            if (!user.WalletAddress.Equals(""))
            {
                response.Append("Your ETH address: ");
                response.AppendLine(user.WalletAddress);
                TransactionStage transactionStage = databaseHandler.getTransactionStage(user.SteamID);
                string status = null;
                switch(transactionStage)
                {
                    case TransactionStage.WAITING_FOR_TRANSACTION:
                        response.AppendLine("No pending transaction.\nTo add a transaction use !sell or !buy commands.");
                        break;
                    case TransactionStage.WAITING_FOR_CONFIRMATION:
                        status = "Waiting for confirmation.";
                        break;
                    case TransactionStage.WAITING_FOR_TRADEOFFER:
                        status = "Waiting for trade offer.";
                        break;
                    case TransactionStage.WAITING_FOR_ETH:
                        status = "Waiting for ETH transfer.";
                        break;
                    case TransactionStage.SENDING_ETH:
                        status = "Sending ETH to your wallet.";
                        break;
                }
                if(status != null)
                {
                    Transaction transaction = databaseHandler.GetUserTransaction(user.SteamID);
                    response.Append("---Transaction details---\nType: ");
                    if (transaction.Sell)
                        response.AppendLine("SELL");
                    else
                        response.AppendLine("BUY");
                    response.Append("Created: ");
                    response.AppendLine(transaction.CreationDate.ToString("MM/dd/yyyy"));
                    response.Append("Number of keys: ");
                    response.AppendLine("" + databaseHandler.GetUserTradeOffer(user.SteamID).Amount);
                    response.Append("ETH value: ");
                    response.AppendLine("" + databaseHandler.getTransactionEthValue(user.SteamID));
                    response.Append("Status: ");
                    response.AppendLine(status);
                }              
            }
            else
            {
                response.AppendLine("Please set your ETH address with !setethaddress comand.");
            }
            steamFriends.SendChatMessage(steamID, EChatEntryType.ChatMsg, response.ToString());
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

                Transaction transaction = new Transaction(user.UserID, DateTime.Now, false, false, false);
                double costPerOneInUSD;
                if (transactionType == MessageType.BUY)
                {
                    transaction.Buy = true;
                    costPerOneInUSD = config.buy_price;
                }
                else
                {
                    transaction.Sell = true;
                    costPerOneInUSD = config.sell_price;
                }
                double ethPriceForOneUsd = bitstampHandler.getEthPriceForOneUsd();
                double costPerOneInETH = costPerOneInUSD * ethPriceForOneUsd;

                databaseHandler.AddTransaction(transaction);
                transaction = databaseHandler.GetUserTransaction(user.SteamID);
                Tradeoffer tradeoffer = new Tradeoffer(transaction.TransactionID, Convert.ToInt32(parameters[0]), costPerOneInETH, false);
                databaseHandler.AddTradeOffer(tradeoffer);
            }catch(Exception e)
            {
                steamFriends.SendChatMessage(steamID, EChatEntryType.ChatMsg, "Error while adding transaction.");
                return false;
            }
            return true;
        }

        private void confirmTransaction(SteamID steamID)
        {
            TransactionStage transactionStage = databaseHandler.getTransactionStage(steamID.ToString());
            StringBuilder response = new StringBuilder();
            switch(transactionStage)
            {
                case TransactionStage.WAITING_FOR_TRANSACTION:
                    response.AppendLine("No pending transaction.\nTo add a transaction use !sell or !buy commands.");
                    break;
                case TransactionStage.WAITING_FOR_CONFIRMATION:
                    databaseHandler.ConfirmTransaction(steamID.ToString());
                    response.AppendLine("Transaction confirmed.\nPlease send the trade offer.");
                    break;
                default:
                    response.AppendLine("Transaction already confirmed.\nTo see next step please use !info command."); break;
            }
            steamFriends.SendChatMessage(steamID, EChatEntryType.ChatMsg, response.ToString());
        }
    }
}

