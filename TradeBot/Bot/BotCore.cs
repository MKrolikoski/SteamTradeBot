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
using System.Net;
using SteamTrade.TradeOffer;
using TradeBot.Web;
using log4net;

/// <summary>
/// Implementation of main bot functionallity 
/// </summary>
namespace TradeBot.Bot
{
    public class BotCore
    {
        #region variables_declaration
        public delegate UserHandler UserHandlerCreator(BotCore bot, SteamID id);
        private readonly UserHandlerCreator createHandler;
        private readonly Dictionary<SteamID, UserHandler> userHandlers;

        private bool cookiesAreInvalid = true;
        private Thread tradeOfferThread;
        private Thread expirationCheckThread;
        public static ILog log = LogManager.GetLogger(typeof(BotCore));


        private CallbackManager callbackManager;
        private TradeOfferManager tradeOfferManager;

        private MessageHandler messageHandler;
        private BitstampHandler bitstampHandler;
        private DatabaseHandler databaseHandler;

        private SteamClient steamClient;
        private SteamUser steamUser;
        private SteamFriends steamFriends;
        private SteamID steamID;

        private SteamGuardAccount steamGuardAccount;

        private string authCode;
        private string steamGuardCode;
        private string myUserNonce;
        private string myUniqueId;

        private Inventory steamInventory;

        private int tradeOfferPollingIntervalSecs;
        private int transactionExpirationCheckIntervalMins;

        public int availableKeys;
        public double availableEth;

        private BotConfig config;

        public readonly SteamTrade.SteamWeb steamWeb;
        public bool IsLoggedIn { get; private set; }
        #endregion

        public BotCore(UserHandlerCreator handler)
        {
            #region config_init
            config = new BotConfig();
            if (!File.Exists("config.cfg"))
            {
                log.Info("New Bot config created.");
                config.createNew();
                if (File.Exists("sentry.bin"))
                    File.Delete("sentry.bin");
            }
            config = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("config.cfg"));

            if (config.login.Equals("") || config.password.Equals(""))
            {
                log.Info("Set username and password for Steam account");
                config.working = false;
            } else
            {
                config.working = true;
            }
            #endregion

            #region variables_init

            steamClient = new SteamClient();

            callbackManager = new CallbackManager(steamClient);

            messageHandler = new MessageHandler();
            bitstampHandler = new BitstampHandler();
            databaseHandler = new DatabaseHandler();

            steamUser = steamClient.GetHandler<SteamUser>();

            steamFriends = steamClient.GetHandler<SteamFriends>();

            steamGuardAccount = new SteamGuardAccount();

            userHandlers = new Dictionary<SteamID, UserHandler>();
            createHandler = handler;
            tradeOfferPollingIntervalSecs = 10;
            transactionExpirationCheckIntervalMins = 1;
            steamWeb = new SteamTrade.SteamWeb();
            GetSteamGuardAccountDetails();

            #endregion

            #region events_subscription

            callbackManager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);

            callbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            callbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);

            callbackManager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

            callbackManager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);

            callbackManager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);

            callbackManager.Subscribe<SteamFriends.FriendMsgCallback>(OnMessageReceived);

            callbackManager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendsList);

            callbackManager.Subscribe<SteamFriends.FriendAddedCallback>(OnFriendAdded);

            callbackManager.Subscribe<SteamUser.LoginKeyCallback>(OnLoginKey);

            callbackManager.Subscribe<SteamUser.WebAPIUserNonceCallback>(OnWebApiUserNonce);

            messageHandler.MessageProcessedEvent += OnMessageProcessed;

            databaseHandler.TransactionDeletedEvent += OnTransactionDeleted;

            ServicePointManager.ServerCertificateValidationCallback += steamWeb.ValidateRemoteCertificate;

            #endregion

            #region main_loop

            log.Info("Connecting to Steam...");
            steamClient.Connect();

            //config.working = true;

            //while (config.working)
            //{
            //    callbackManager.RunCallbacks();
            //    if (tradeOfferManager != null)
            //    {
            //        tradeOfferManager.HandleNextPendingTradeOfferUpdate();
            //    }
            //    Thread.Sleep(1);
            //}
            #endregion
        }


        #region steamguard_methods
        public bool AcceptAllTradeConfirmations()
        {
            steamGuardAccount.Session.SteamLogin = steamWeb.Token;
            steamGuardAccount.Session.SteamLoginSecure = steamWeb.TokenSecure;
            try
            {
                foreach (var confirmation in steamGuardAccount.FetchConfirmations())
                {
                    steamGuardAccount.AcceptConfirmation(confirmation);
                }
            }
            catch (SteamGuardAccount.WGTokenInvalidException)
            {
                log.Error("Invalid session when trying to fetch trade confirmations.");
                return false;
            }
            return true;
        }


        bool GetSteamGuardAccountDetails()
        {
            if (File.Exists("steamguard.cfg"))
            {
                steamGuardAccount = Newtonsoft.Json.JsonConvert.DeserializeObject<SteamAuth.SteamGuardAccount>(File.ReadAllText("steamguard.cfg"));
                return true;
            }
            log.Error("steamguard.cfg not found.");
            return false;
        }
        #endregion

        #region transaction_expiration_control  
        protected void SpawnExpirationCheckThread()
        {
            if (expirationCheckThread == null)
            {
                expirationCheckThread = new Thread(DeleteExpiredTransactions);
                expirationCheckThread.Start();
            }
        }

        protected void CancelExpirationCheckThread()
        {
            expirationCheckThread = null;
        }

        protected void DeleteExpiredTransactions()
        {
            while (expirationCheckThread == Thread.CurrentThread)
            {
                databaseHandler.DeleteExpiredTransactions();
                Thread.Sleep(transactionExpirationCheckIntervalMins * 1000 * 60);
            }
        }
        #endregion

        #region tradeoffer_methods  
        protected void SpawnTradeOfferPollingThread()
        {
            if (tradeOfferThread == null)
            {
                tradeOfferThread = new Thread(TradeOfferPollingFunction);
                tradeOfferThread.Start();
            }
        }

        protected void CancelTradeOfferPollingThread()
        {
            tradeOfferThread = null;
        }

        protected void TradeOfferPollingFunction()
        {
            while (tradeOfferThread == Thread.CurrentThread)
            {
                try
                {
                    tradeOfferManager.EnqueueUpdatedOffers();
                }
                catch (Exception e)
                {
                    log.Error("Error while polling trade offers: " + e);
                }

                Thread.Sleep(tradeOfferPollingIntervalSecs * 1000);
            }
        }

        public void TradeOfferRouter(SteamTrade.TradeOffer.TradeOffer offer)
        {
            GetUserHandler(offer.PartnerSteamId).OnTradeOfferUpdated(offer);
        }
        public void SubscribeTradeOffer(TradeOfferManager tradeOfferManager)
        {
            tradeOfferManager.OnTradeOfferUpdated += TradeOfferRouter;
        }
        #endregion

        #region webAPI_authentication
        void UserWebLogOn()
        {
            do
            {
                IsLoggedIn = steamWeb.Authenticate(myUniqueId, steamClient, myUserNonce);

                if (!IsLoggedIn)
                {
                    log.Info("Authentication failed, retrying in 2s...");
                    Thread.Sleep(2000);
                }
            } while (!IsLoggedIn);

            log.Info("User Authenticated!");


            tradeOfferManager = new TradeOfferManager(config.api_key, steamWeb);
            SubscribeTradeOffer(tradeOfferManager);
            cookiesAreInvalid = false;

            SpawnTradeOfferPollingThread();
            SpawnExpirationCheckThread();

        }

        bool CheckCookies()
        {
            if (cookiesAreInvalid)
                return false;

            try
            {
                if (!steamWeb.VerifyCookies())
                {
                    log.Info("Cookies are invalid. Need to re-authenticate.");
                    cookiesAreInvalid = true;
                    steamUser.RequestWebAPIUserNonce();
                    return false;
                }
            }
            catch
            {
                log.Error("Cookie check failed. http://steamcommunity.com is possibly down.");
            }

            return true;
        }
        #endregion

        #region userhandlers
        public UserHandler GetUserHandler(SteamID sid)
        {
            if (!userHandlers.ContainsKey(sid))
                userHandlers[sid] = createHandler(this, sid);
            return userHandlers[sid];
        }

        void RemoveUserHandler(SteamID sid)
        {
            if (userHandlers.ContainsKey(sid))
                userHandlers.Remove(sid);
        }
        #endregion

        #region callback_handlers
        private void OnConnected(SteamClient.ConnectedCallback callback)
        {
            log.Info("Connected to Steam! Logging in...");

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

        private void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            if (authCode != null)
            {
                log.Info("Updating sentryfile...");



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

                log.Info("Sentryfile updated.");
            }
        }

        private void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            bool isSteamGuard = callback.Result == EResult.AccountLogonDenied;
            bool is2FA = callback.Result == EResult.AccountLoginDeniedNeedTwoFactor;

            //SteamGuard enabled -> need to generate code
            if (isSteamGuard || is2FA)
            {
                log.Info("This account is SteamGuard protected!");

                if (is2FA)
                {
                    if (steamGuardAccount.SharedSecret.Equals(""))
                    {
                        log.Error("Shared-secret not set.");
                        Console.WriteLine("Please enter SteamGuard code from you authentication device: ");
                        steamGuardCode = Console.ReadLine();
                    }
                    else
                    {
                        log.Info("Generating SteamGuard code..");
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
                log.Info("Unable to logon to Steam: "+ callback.Result+ "/"+ callback.ExtendedResult);

                config.working = false;


                return;
            }

            log.Info("Successfully logged on!");

            myUserNonce = callback.WebAPIUserNonce;

            config.save();
            steamID = steamClient.SteamID;

            //730 - appID for CS:GO
            steamInventory = new Inventory(steamID, 730);
            updateCurrentKeysAndEthAmount();
            log.Info("Available keys: " + availableKeys);
            log.Info("Available eth: " + availableEth);
        }

        private void OnLoginKey(SteamUser.LoginKeyCallback callback)
        {
                myUniqueId = callback.UniqueID.ToString();
                UserWebLogOn();                     
                GetUserHandler(steamClient.SteamID).OnLoginCompleted();
        }

        private void OnWebApiUserNonce(SteamUser.WebAPIUserNonceCallback callback)
        {
            log.Info("Received new WebAPIUserNonce.");

            if (callback.Result == EResult.OK)
            {
                myUserNonce = callback.Nonce;
                UserWebLogOn();
            }
            else
            {
                log.Error("WebAPIUserNonce Error: " + callback.Result);
            }
        }
       
        private void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
                log.Info("Disconnected from Steam, reconnecting in 5...");

            CancelTradeOfferPollingThread();
            CancelExpirationCheckThread();

            Thread.Sleep(TimeSpan.FromSeconds(5));

            steamClient.Connect();
        }
     

        private void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            log.Info(string.Format("Logged off of Steam: {0}", callback.Result));
            CancelTradeOfferPollingThread();
            CancelExpirationCheckThread();
            config.working = false;
            config.save();
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
                    sendMessage(message.from, "Available commands: \n!help, \n!sell [number_of_keys], \n!buy [number_of_keys], \n!setethaddress [eth_address], \n!info, \n!confirm"); break;
                case MessageType.SELL:
                    if (createTransaction(message.from, Convert.ToInt32(message.parameters[0]), message.messageType))
                    {
                        sendMessage(message.from, "Transaction added succesfully. To confirm transaction type: !confirm");
                    }
                    break;
                case MessageType.BUY:
                    if (createTransaction(message.from, Convert.ToInt32(message.parameters[0]), message.messageType))
                    {
                        sendMessage(message.from, "Transaction added succesfully. To confirm transaction type: !confirm");
                    }
                    break;
                case MessageType.SETETHADDRESS:
                    if (databaseHandler.setEthAddress(message.from.ToString(), message.parameters[0]))
                    {
                        string msg = "Successfully changed ETH address to: " + message.parameters[0];
                        sendMessage(message.from, msg);
                    }
                    else
                    {
                        sendMessage(message.from, "Error while changing ETH address.");
                    }
                    break;
                case MessageType.CONFIRM:
                    confirmTransaction(message.from);
                    break;
                case MessageType.INFO:
                    printInfo(message.from); break;
                case MessageType.BADPARAMS:
                    sendMessage(message.from, "Bad arguments. Type !help for the list of commands and their arguments."); break;
                default:
                    sendMessage(message.from, "Unknown command. Type !help for the list of commands and their arguments."); break;
            }
        }


        private void OnTransactionDeleted(object sender, string e)
        {
            string attribute = WebUtils.GetJSONAtribute(e, "keysAmount");
            if(attribute == null)
            {
                attribute = WebUtils.GetJSONAtribute(e, "ethAmount");
                double ethAmount = Double.Parse(attribute);
                availableEth += ethAmount;
            }
            else
            {
                int keysAmount = int.Parse(attribute);
                availableKeys += keysAmount;
            }
        }



        private void OnAccountInfo(SteamUser.AccountInfoCallback pData)
        {
            changeStatus();
        }

        private void OnFriendsList(SteamFriends.FriendsListCallback callback)
        {
            foreach (var friend in callback.FriendList)
            {
                if (friend.Relationship == EFriendRelationship.RequestRecipient)
                {
                    steamFriends.AddFriend(friend.SteamID);
                }
            }
        }

        private void OnFriendAdded(SteamFriends.FriendAddedCallback callback)
        {
            sendMessage(callback.SteamID, "Welcome. Type !help for the list of commands.");
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
        #endregion


        #region database
        private bool createTransaction(SteamID steamID, int keyAmount, MessageType transactionType)
        {
            try
            {
                User user = databaseHandler.GetUser(steamID.ToString());
                if (user.WalletAddress.Equals(""))
                {
                    sendMessage(steamID, "Please set your ETH address with !setethaddress comand.");
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
                    if (keyAmount > availableKeys)
                    {
                        sendMessage(steamID, "Not enough keys in bot's inventory.\nNumber of available keys: " + availableKeys.ToString());
                        return false;
                    }

                    transaction.Buy = true;
                    costPerOneInUSD = config.buy_price;
                }
                else
                {
                    transaction.Sell = true;
                    costPerOneInUSD = config.sell_price;
                }
                double ethPriceForOneUsd = bitstampHandler.getEthPriceForOneUsd();
                if (ethPriceForOneUsd == -1)
                {
                    sendMessage(steamID, "Error while adding transaction.");
                    return false;
                }

                double costPerOneInETH = costPerOneInUSD * ethPriceForOneUsd;
                double totalEthValue = costPerOneInETH * keyAmount;
                if (transactionType == MessageType.SELL && availableEth < totalEthValue)
                {
                    sendMessage(steamID, "Not enough ETH in bot's wallet.\nNumber of max. number of keys bot can buy: " + Math.Floor(availableEth / costPerOneInETH).ToString());
                    return false;
                }

                databaseHandler.AddTransaction(transaction);
                transaction = databaseHandler.GetUserTransaction(user.SteamID);
                Tradeoffer tradeoffer = new Tradeoffer(transaction.TransactionID, keyAmount, costPerOneInETH, false);
                databaseHandler.AddTradeOffer(tradeoffer);
            }
            catch (Exception e)
            {
                sendMessage(steamID, "Error while adding transaction.");
                return false;
            }
            updateCurrentKeysAndEthAmount();
            return true;
        }

        private void confirmTransaction(SteamID steamID)
        {
            TransactionStage transactionStage = databaseHandler.getTransactionStage(steamID.ToString());
            StringBuilder response = new StringBuilder();
            switch (transactionStage)
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
            sendMessage(steamID, response.ToString());
        }

        public DatabaseHandler GetDatabaseHandler()
        {
            return databaseHandler;
        }
        #endregion

        #region bitstmap
        public bool sendEth(string address, double amount)
        {
            return bitstampHandler.sendEth(address, amount);
        }

        public string getEthAddress()
        {
            return bitstampHandler.getEthAddress();
        }

        public bool checkIfTransfered(DateTime date, double amount)
        {
            return bitstampHandler.checkIfTransfered(date, amount);
        }
        #endregion


        #region utils
        private int getKeysAmountFromInventory()
        {
            int count = 0;
            foreach (var value in steamInventory.Items.Values)
            {
                if (value.Description.Tradable)
                {
                    foreach (var item in value.Items)
                    {
                        if (item.ToCEconAsset(730).InstanceId == 143865972)
                            count++;
                    }
                }
            }
            return count;
        }

        private void updateCurrentKeysAndEthAmount()
        {
            availableKeys = getKeysAmountFromInventory() - databaseHandler.getReservedKeysAmount();
            availableEth = bitstampHandler.getAvailableEth() - databaseHandler.getReservedEthAmount();
            //log.Info("Available keys: " + availableKeys);
            //log.Info("Available eth: " + availableEth);
        }

        public void sendMessage(SteamID to, string message)
        {
            steamFriends.SendChatMessage(to, EChatEntryType.ChatMsg, message);
        }

        public void sendMessage(string to, string message)
        {
            SteamID steamID;
            if(Utils.TrySetSteamID(to, out steamID))
                steamFriends.SendChatMessage(steamID, EChatEntryType.ChatMsg, message);
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
                switch (transactionStage)
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
                if (status != null)
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
            sendMessage(steamID, response.ToString());
        }
        #endregion

        public void stop()
        {
            config.working = false;
        }

        public void start()
        {
            config.working = true;
        }

        public void run()
        {
            while (config.working)
            {
                callbackManager.RunCallbacks();
                if (tradeOfferManager != null)
                {
                    tradeOfferManager.HandleNextPendingTradeOfferUpdate();
                }
                Thread.Sleep(1);
            }
        }
        }
}

