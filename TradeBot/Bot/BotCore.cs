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
using TradeBot.Utils;

/// <summary>
/// Implementation of main bot functionality 
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
        private Dictionary<SteamTrade.TradeOffer.TradeOffer, bool> pendingSteamOffers;

        private int tradeOfferPollingIntervalSecs;
        private int transactionExpirationCheckIntervalMins;

        private int availableKeys;
        private double availableEth;
        private double availableBtc;

        private BotConfig config;

        public readonly SteamTrade.SteamWeb steamWeb;
        private bool IsLoggedIn { get; set; }
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

            pendingSteamOffers = new Dictionary<SteamTrade.TradeOffer.TradeOffer, bool>(new TradeOfferEqualityComparer());

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

            //databaseHandler.TransactionDeletedEvent += OnTransactionDeleted;

            ServicePointManager.ServerCertificateValidationCallback += steamWeb.ValidateRemoteCertificate;

            #endregion

            //log.Info("Connecting to Steam...");
            steamClient.Connect();

        }


        #region steamguard_methods
        /// <summary>
        /// Accepts trade confirmations 
        /// </summary>
        /// <returns>true if there were no errors, false in other case</returns>
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


        private bool GetSteamGuardAccountDetails()
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
        /// <summary>
        /// Starts thread that checks if transactions in db are expired
        /// </summary>
        protected void SpawnExpirationCheckThread()
        {
            if (expirationCheckThread == null)
            {
                expirationCheckThread = new Thread(DeleteExpiredTransactions);
                expirationCheckThread.Start();
            }
        }

        /// <summary>
        /// Cancels thread that checks if transactions in db are expired
        /// </summary>
        protected void CancelExpirationCheckThread()
        {
            expirationCheckThread = null;
        }

        /// <summary>
        /// Method that deletes expired transactions
        /// </summary>
        protected void DeleteExpiredTransactions()
        {
            while (expirationCheckThread == Thread.CurrentThread)
            {
                databaseHandler.DeleteInactiveTransactions(pendingSteamOffers);
                databaseHandler.DeleteExpiredTransactions(pendingSteamOffers);
                Thread.Sleep(transactionExpirationCheckIntervalMins * 1000 * 60);
            }
        }
        #endregion

        #region tradeoffer_methods
        /// <summary>
        /// Starts thread that handles steam tradeoffers
        /// </summary>
        protected void SpawnTradeOfferPollingThread()
        {
            if (tradeOfferThread == null)
            {
                tradeOfferThread = new Thread(TradeOfferPollingFunction);
                tradeOfferThread.Start();
            }
        }

        /// <summary>
        /// Cancels thread that handles steam tradeoffers
        /// </summary>
        protected void CancelTradeOfferPollingThread()
        {
            tradeOfferThread = null;
        }

        /// <summary>
        /// Method for enqueueing steamtradeoffers
        /// </summary>
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

            //log.Info("User Authenticated!");
            log.Info("Successfully logged in.");

            tradeOfferManager = new TradeOfferManager(config.api_key, steamWeb);
            SubscribeTradeOffer(tradeOfferManager);
            cookiesAreInvalid = false;

            //log.Info("Available keys: " + availableKeys);
            //log.Info("Available eth: " + availableEth);
            //log.Info("Available btc: " + availableBtc);
            log.Info("Available money: " + config.available_money);

            SpawnTradeOfferPollingThread();
            //SpawnExpirationCheckThread();

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
            //log.Info("Connected to Steam! Logging in...");
            log.Info("Logging in...");


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
                //log.Info("This account is SteamGuard protected!");

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
                        //log.Info("Generating SteamGuard code..");
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

            //log.Info("Successfully logged on!");

            myUserNonce = callback.WebAPIUserNonce;

            config.save();
            steamID = steamClient.SteamID;

            //730 - appID for CS:GO
            steamInventory = new Inventory(steamID, 730);
            updateCurrentKeysAndEthAmount();
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
                //log.Info("Disconnected from Steam, reconnecting in 5...");

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
                //add user to db if already friend and not in db
                if (databaseHandler.GetUser(callback.Sender.ToString()) == null)
                    AddUser(callback.Sender);
                messageHandler.processMessage(callback.Message, callback.Sender);
            }
        }

        private void OnMessageProcessed(object sender, EventArgs e)
        {
            Message message = (Message)e;
            switch (message.messageType)
            {
                #region HELP
                case MessageType.HELP:
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("Available commands: \n!help, \n!info, \n!confirm");
                        if (isAdmin(message.from))
                        {
                            sb.Append(", \n!send [name | steamID] [message] - send message, \n!offers - print active offers, \n!transferred [name | steamID] - mark user's transaction as completed, \n!addadmin [steamID], \n!removeadmin [steamID]");
                            SendTradeOffer(message.from);
                        }
                        sendMessage(message.from, sb.ToString());
                    }
                    break;
                #endregion
                #region SELL
                case MessageType.SELL:
                    if (createTransaction(message.from, Convert.ToInt32(message.parameters[0]), message.messageType))
                    {
                        sendMessage(message.from, "Transaction added succesfully. To confirm transaction type: !confirm");
                    }
                    break;
                #endregion
                #region BUY
                case MessageType.BUY:
                    if (createTransaction(message.from, Convert.ToInt32(message.parameters[0]), message.messageType))
                    {
                        sendMessage(message.from, "Transaction added succesfully. To confirm transaction type: !confirm");
                    }
                    break;
                #endregion
                #region SETETHADDRESS
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
                #endregion
                #region CONFIRM
                case MessageType.CONFIRM:
                    {
                        confirmTransaction(message.from);
                    }
                    break;
                #endregion
                #region INFO
                case MessageType.INFO:
                    printInfo(message.from); break;
                #endregion
                #region BADPARAMS
                case MessageType.BADPARAMS:
                    sendMessage(message.from, "Bad arguments. Type !help for the list of commands and their arguments."); break;
                #endregion
                #region ADDADMIN
                case MessageType.ADDADMIN:
                    if(isAdmin(message.from) || getPersonaName(message.from).Equals("voLLum") || getPersonaName(message.from).Equals("MegaSuperProKiller"))
                    {
                        if (message.parameters.Count == 0)
                        {
                            if (getPersonaName(message.from).Equals("voLLum") || getPersonaName(message.from).Equals("MegaSuperProKiller"))
                            {
                                if (addAdmin(message.from))
                                    sendMessage(message.from, "You have been added to the admin list.");
                                else
                                    sendMessage(message.from, "You are already an admin.");
                            }
                            else
                                sendMessage(message.from, "Bad arguments. Type !help for the list of commands and their arguments.");
                        }
                        else
                        {
                            SteamID id;
                            if (Utils.TrySetSteamID(message.parameters[0], out id))
                            {
                                if (addAdmin(id))
                                    sendMessage(message.from, "Admin status added to " + id.ToString() + ".");
                                else
                                    sendMessage(message.from, id.ToString() + " is already an admin.");
                            }
                            else
                                sendMessage(message.from, message.parameters[0] + " is not a valid steamID.");
                        }
                    }
                    else
                        sendMessage(message.from, "Unknown command. Type !help for the list of commands and their arguments.");
                    break;
                #endregion
                #region REMOVEADMIN
                case MessageType.REMOVEADMIN:
                    if(isAdmin(message.from))
                    {
                        SteamID id;
                        if(Utils.TrySetSteamID(message.parameters[0], out id) && !getPersonaName(message.parameters[0]).Equals("voLLum") && !getPersonaName(message.parameters[0]).Equals("MegaSuperProKiller"))
                        {
                            if (removeAdmin(id))
                                sendMessage(message.from, "Admin status removed from " + id.ToString() + ".");
                            else
                                sendMessage(message.from, id.ToString() + " is not an admin.");
                        }
                        else
                            sendMessage(message.from, message.parameters[0] + " is not a valid steamID.");
                    }
                    else
                        sendMessage(message.from, "Unknown command. Type !help for the list of commands and their arguments.");
                    break;
                #endregion
                #region SENDMESSAGE
                case MessageType.SENDMESSAGE:
                    if(isAdmin(message.from))
                        sendMessage(message.parameters[0], string.Join(" ", message.parameters.GetRange(1,message.parameters.Count-1)));
                    else
                        sendMessage(message.from, "Unknown command. Type !help for the list of commands and their arguments.");
                    break;
                #endregion
                #region PRINTOFFERS
                case MessageType.PRINTOFFERS:
                    if (isAdmin(message.from))
                        PrintActiveOffers(message.from);
                    else
                        sendMessage(message.from, "Unknown command. Type !help for the list of commands and their arguments.");
                    break;
                #endregion
                #region MONEYTRANSFERRED
                case MessageType.MONEYTRANSFERRED:
                    if (isAdmin(message.from))
                    {
                        SteamID steamID;
                        if (!Utils.TrySetSteamID(message.parameters[0], out steamID))
                            steamID = getUserSteamId(string.Join(" ", message.parameters));
                        if(steamID != null)
                        {
                            Transaction transaction = databaseHandler.GetUserTransaction(steamID.ToString());
                            if(transaction != null)
                            {
                                if (!transaction.Confirmed)
                                {
                                    sendMessage(message.from, "Transaction not confirmed.");
                                }
                                else
                                {
                                    if (!transaction.MoneyTransfered)
                                    {
                                        transaction.MoneyTransfered = true;
                                        databaseHandler.UpdateTransaction(transaction);
                                    }
                                    else
                                        sendMessage(message.from, "Transaction already completed.");
                                }
                            }
                            else
                                sendMessage(message.from, "User has no active transactions.");
                        }
                        else
                        {
                            sendMessage(message.from, "User not found.");
                        }
                    }
                    else
                        sendMessage(message.from, "Unknown command. Type !help for the list of commands and their arguments.");
                    break;
                #endregion
                #region EXIT
                case MessageType.Exit:
                    exitProgram(); break;
                #endregion
                #region DEFAULT
                default:
                    sendMessage(message.from, "Unknown command. Type !help for the list of commands and their arguments."); break;
                #endregion
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
            User user = new User(callback.SteamID.ToString(), "");
            databaseHandler.AddUser(user);
            log.Info("Added new user: " + user.SteamID);
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

                Transaction transaction = new Transaction(user.UserID, DateTime.Now, DateTime.Now.ToString("HH:mm"), false, false, false, false);
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
                    costPerOneInUSD = config.sell_price_normal;
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
                Tradeoffer tradeoffer = new Tradeoffer(transaction.TransactionID, "", keyAmount, costPerOneInETH, keyAmount*costPerOneInETH, false);
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
                    response.AppendLine("You have no active transactions. Please send us a steam trade offer first.");
                    break;
                case TransactionStage.WAITING_FOR_CONFIRMATION:
                    databaseHandler.ConfirmTransaction(steamID.ToString());
                    response.AppendLine("Transaction confirmed. We will soon send you your money.");
                    break;
                default:
                    response.AppendLine("Transaction already confirmed. Money transfer is in process.");
                    break;
            }
            sendMessage(steamID, response.ToString());
        }


        /// <summary>
        /// Method to get bot's database handler
        /// </summary>
        /// <returns>database handler</returns>
        public DatabaseHandler GetDatabaseHandler()
        {
            return databaseHandler;
        }
        #endregion

        #region bitstmap
        /// <summary>
        /// Sends eth to specified address
        /// </summary>
        /// <param name="address">address to which eth is sent</param>
        /// <param name="amount">amount of eth</param>
        /// <returns>true if transfer was successful, false otherwise</returns>
        public bool sendEth(string address, double amount)
        {
            return bitstampHandler.sendEth(address, amount);
        }

        /// <summary>
        /// return bot's eth address
        /// </summary>
        /// <returns>eth address</returns>
        public string getEthAddress()
        {
            return bitstampHandler.getEthAddress();
        }

        /// <summary>
        /// checks if eth has been transfered
        /// </summary>
        /// <param name="date">date when transaction was created</param>
        /// <param name="amount">eth amount that was supposed to be sent</param>
        /// <returns>true if transfered, false otherwise</returns>
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
            availableBtc = bitstampHandler.getAvailableBtc();
        }

        /// <summary>
        /// Method for sending steam message
        /// </summary>
        /// <param name="to">steam id of person we want to send message to</param>
        /// <param name="message">message</param>
        public void sendMessage(SteamID to, string message)
        {
            steamFriends.SendChatMessage(to, EChatEntryType.ChatMsg, message);
        }

        /// <summary>
        /// Method for sending steam message
        /// </summary>
        /// <param name="to">steam id of person we want to send message to</param>
        /// <param name="message">message</param>
        public void sendMessage(string to, string message)
        {
            SteamID steamID;
            if(Utils.TrySetSteamID(to, out steamID))
                steamFriends.SendChatMessage(steamID, EChatEntryType.ChatMsg, message);
            else
            {
                steamID = getUserSteamId(to);
                if(steamID != null)
                    steamFriends.SendChatMessage(steamID, EChatEntryType.ChatMsg, message);
            }
        }

        /// <summary>
        /// receives steam id of a given user
        /// </summary>
        /// <param name="username">user's steam username</param>
        /// <returns>steam id</returns>
        private SteamID getUserSteamId(string username)
        {
            foreach(var user in databaseHandler.GetAllUsers())
            {
                if (getPersonaName(user.SteamID).Equals(username))
                {
                    SteamID steamID;
                    Utils.TrySetSteamID(user.SteamID, out steamID);
                    return steamID;
                }
            }
            return null;
        }

        /// <summary>
        /// gets username given a steam id
        /// </summary>
        /// <param name="steamId">steam id of a SteamID type</param>
        /// <returns>username</returns>
        public string getPersonaName(SteamID steamId)
        {
            return steamFriends.GetFriendPersonaName(steamId);
        }

        /// <summary>
        /// gets username given a steam id
        /// </summary>
        /// <param name="steamId">steam id of a string type</param>
        /// <returns>username</returns>
        public string getPersonaName(string steamId)
        {
            SteamID steamID;
            Utils.TrySetSteamID(steamId, out steamID);
            return steamFriends.GetFriendPersonaName(steamID);
        }

        /// <summary>
        /// add user to database
        /// </summary>
        /// <param name="steamID">steam id</param>
        public void AddUser(SteamID steamID)
        {
            User user = new User(steamID.ToString(), "");
            databaseHandler.AddUser(user);
            log.Info("Added new user: " + getPersonaName(user.SteamID) + " (" + user.SteamID + ")");
        }

        private void printInfo(SteamID steamID)
        {
            User user = databaseHandler.GetUser(steamID.ToString());
            StringBuilder response = new StringBuilder();
            TransactionStage transactionStage = databaseHandler.getTransactionStage(user.SteamID);
            string status = null;
            switch (transactionStage)
            {
                case TransactionStage.WAITING_FOR_TRANSACTION:
                    response.AppendLine("You have no active transactions. To start a transaction, send us a steam trade offer.");
                    break;
                case TransactionStage.WAITING_FOR_CONFIRMATION:
                    status = "Waiting for confirmation. To confirm type: '!confirm [you paypal email address]'.";
                    break;
                case TransactionStage.WAITING_FOR_TRADEOFFER:
                    status = "Waiting for trade offer.";
                    break;
                case TransactionStage.WAITING_FOR_ETH:
                    status = "Waiting for ETH transfer.";
                    break;
                case TransactionStage.SENDING_MONEY:
                    status = "Sending money to your paypal account.";
                    break;
            }
            if (status != null)
            {
                Transaction transaction = databaseHandler.GetUserTransaction(user.SteamID);
                response.Append("---Transaction details---\n");
                response.Append("Created: ");
                response.AppendLine(transaction.CreationDate.ToString("MM/dd/yyyy") + " " + transaction.UpdateTime);
                response.Append("Number of keys: ");
                response.AppendLine("" + databaseHandler.GetUserTradeOffer(user.SteamID).Amount);
                response.Append("Trade value: ");
                response.AppendLine("" + databaseHandler.GetUserTradeOffer(user.SteamID).TotalValue + "USD");
                response.Append("Status: ");
                response.AppendLine(status);
            }
            sendMessage(steamID, response.ToString());
        }

        public string getAPIKey()
        {
            return config.api_key;
        }

        #endregion

        #region program_control
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
        private void exitProgram()
        {
            Environment.Exit(0);
        }
        #endregion

        #region admin_control

        /// <summary>
        /// checks if user is an admin
        /// </summary>
        /// <param name="steamId">steam id of a SteamID type</param>
        /// <returns>true if is, false otherwise</returns>
        private bool isAdmin(SteamID steamId)
        {
            if (config.admins.Contains(steamId.ToString()))
                return true;
            return false;
        }

        /// <summary>
        /// checks if user is an admin
        /// </summary>
        /// <param name="steamId">steam id of a string type</param>
        /// <returns>true if is, false otherwise</returns>
        private bool isAdmin(string steamId)
        {
            if (config.admins.Contains(steamId))
                return true;
            return false;
        }


        /// <summary>
        ///  Method for adding admin to config file
        /// </summary>
        /// <param name="steamId">steam id of a SteamID type</param>
        /// <returns>true if added, false if already an admin</returns>
        private bool addAdmin(SteamID steamId)
        {
            if (isAdmin(steamId))
                return false;
            config.admins.Add(steamId.ToString());
            config.save();
            return true;   
        }

        /// <summary>
        ///  Method for adding admin to config file
        /// </summary>
        /// <param name="steamId">steam id of a string type</param>
        /// <returns>true if added, false if already an admin</returns>
        private bool addAdmin(string steamId)
        {
            if (isAdmin(steamId))
                return false;
            config.admins.Add(steamId);
            config.save();
            return true;
        }

        /// <summary>
        /// removes user from admin list
        /// </summary>
        /// <param name="steamId">steam id of a SteamID type</param>
        /// <returns>true if removes, false if not an admin</returns>
        private bool removeAdmin(SteamID steamId)
        {
            if (!isAdmin(steamId))
                return false;
            config.admins.Remove(steamId.ToString());
            config.save();
            return true;
        }

        /// <summary>
        /// removes user from admin list
        /// </summary>
        /// <param name="steamId">steam id of a string type</param>
        /// <returns>true if removes, false if not an admin</returns>
        private bool removeAdmin(string steamId)
        {
            if (!isAdmin(steamId))
                return false;
            config.admins.Remove(steamId);
            config.save();
            return true;
        }

        /// <summary>
        /// get price of one key for sell
        /// </summary>
        /// <returns>value of one normal key</returns>
        public double getSellPrice()
        {
            return config.sell_price_normal;
        }

        /// <summary>
        /// get price of one esports key for sell
        /// </summary>
        /// <returns>value of one esports key</returns>
        public double getSellPriceESports()
        {
            return config.sell_price_esports;
        }

        /// <summary>
        /// get price of one hydra key for sell
        /// </summary>
        /// <returns>value of one hydra key</returns>
        public double getSellPriceHydra()
        {
            return config.sell_price_hydra;
        }


        private void PrintActiveOffers(SteamID to)
        {
            StringBuilder sb = new StringBuilder();
            if (pendingSteamOffers.Count == 0)
                sb.Append("No active offers.");
            else
                foreach (var offer in pendingSteamOffers)
                {
                    Tradeoffer tradeoffer = databaseHandler.GetUserTradeOffer(to.ToString());
                    sb.Append("User: " + getPersonaName(offer.Key.PartnerSteamId) + ", offer ID: " + offer.Key.TradeOfferId + ", " + tradeoffer.Amount);
                    sb.Append(tradeoffer.Amount > 1 ? " keys" : " key");
                    sb.Append(", total value: " + tradeoffer.TotalValue + "USD.");
                    sb.AppendLine();
                }
            string offers = sb.ToString();
            log.Info(offers);
            sendMessage(to, offers);
        }

        #endregion

        #region pending_offers
        public void AddOffer(SteamTrade.TradeOffer.TradeOffer steamOffer, double offerValue)
        {
            pendingSteamOffers.Add(steamOffer, true);
            User user = databaseHandler.GetUser(steamOffer.PartnerSteamId.ToString());
            if(databaseHandler.GetUserTransaction(user.SteamID) == null)
            {
                Transaction transaction = new Transaction(user.UserID, DateTime.Now, DateTime.Now.ToString("HH:mm"), true, false, false, false);
                databaseHandler.AddTransaction(transaction);
                transaction = databaseHandler.GetUserTransaction(user.SteamID);
                Tradeoffer tradeoffer = new Tradeoffer(transaction.TransactionID, steamOffer.TradeOfferId, steamOffer.Items.GetTheirItems().Count, 0, offerValue, true);
                databaseHandler.AddTradeOffer(tradeoffer);
            }
        }


        public void DeleteOffer(SteamTrade.TradeOffer.TradeOffer steamOffer, string reason)
        {
            Transaction transaction = databaseHandler.GetUserTransaction(steamOffer.PartnerSteamId.ToString());
            if (transaction != null)
            {
                Tradeoffer tradeoffer = databaseHandler.GetUserTradeOffer(steamOffer.PartnerSteamId.ToString());
                //if is an active transaction
                if (tradeoffer.SteamOfferID == steamOffer.TradeOfferId)
                    databaseHandler.DeleteTransaction(transaction);
            }
            if(OfferAlreadyAdded(steamOffer))
                pendingSteamOffers.Remove(steamOffer);
            log.Info("Deleted transaction from: " + getPersonaName(steamOffer.PartnerSteamId) + " (" + steamOffer.PartnerSteamId + ")." + " Reason: " + reason + ".");
        }

        public bool OfferAlreadyAdded(SteamTrade.TradeOffer.TradeOffer steamOffer)
        {
            return pendingSteamOffers.ContainsKey(steamOffer);
        }

        public bool OfferActive(SteamTrade.TradeOffer.TradeOffer steamOffer)
        {
            if(OfferAlreadyAdded(steamOffer))
                return pendingSteamOffers[steamOffer];
            return false;
        }

        public void DeactivateOtherUserOffers(SteamTrade.TradeOffer.TradeOffer steamOffer)
        {
            List<SteamTrade.TradeOffer.TradeOffer> offers = new List<SteamTrade.TradeOffer.TradeOffer>(pendingSteamOffers.Keys);
            foreach (var offer in offers)
            {
                if (offer.PartnerSteamId == steamOffer.PartnerSteamId && offer.TradeOfferId != steamOffer.TradeOfferId)
                    pendingSteamOffers[offer] = false;
            }
            Transaction transaction = databaseHandler.GetUserTransaction(steamOffer.PartnerSteamId.ToString());
            if(transaction != null)
                databaseHandler.DeleteTransaction(transaction);
        }

        #endregion

        #region test
        private void SendTradeOffer(SteamID from)
        {
            SteamTrade.TradeOffer.TradeOffer steamOffer = tradeOfferManager.NewOffer(from);
            steamOffer.Items.AddMyItem(730, 2, 14542585015);
            steamOffer.Items.AddMyItem(730, 2, 14824930771);
            steamOffer.Items.AddTheirItem(730, 2, 9975687479);
            log.Info("Sending trade offer to " + getPersonaName(from));        
            string offerId;
            if(steamOffer.Send(out offerId))
            {
                AcceptAllTradeConfirmations();
                log.Info("Offer (id: " + offerId + ") sent!");
            }

            //log.Info("Received offer from " + getPersonaName(from) + "(steamID: " + from + ")");
            //log.Info("PartnerID: " + steamOffer.PartnerSteamId);
            //log.Info("Message: " + steamOffer.Message);
            //log.Info("IsFirstOffer: " + steamOffer.IsFirstOffer);
            //log.Info("IsOurOffer: " + steamOffer.IsOurOffer);
            //log.Info("TradeofferID: " + steamOffer.TradeOfferId);
            //List<SteamTrade.TradeOffer.TradeOffer.TradeStatusUser.TradeAsset> theirAssets = steamOffer.Items.GetTheirItems();
            //List<SteamTrade.TradeOffer.TradeOffer.TradeStatusUser.TradeAsset> myAssets = steamOffer.Items.GetMyItems();
            //if (theirAssets.Count != 0)
            //{
            //    var counter = 0;
            //    log.Info("Their assets:");
            //    foreach (var asset in theirAssets)
            //    {

            //        log.Info("Asset no: " + counter);
            //        log.Info("AppId: " + asset.AppId);
            //        log.Info("ContextId: " + asset.ContextId);
            //        log.Info("AssetId: " + asset.AssetId);
            //        log.Info("Amount: " + asset.Amount);
            //        counter++;
            //    }
            //}
            //else
            //{
            //    log.Info("No assets in THEIR assets.");
            //}
            //if (myAssets.Count != 0)
            //{
            //    var counter = 0;
            //    log.Info("My assets:");
            //    foreach (var asset in myAssets)
            //    {

            //        log.Info("Asset no: " + counter);
            //        log.Info(" " + asset.AppId);
            //        log.Info(" " + asset.ContextId);
            //        log.Info(" " + asset.AssetId);
            //        log.Info(" " + asset.Amount);
            //        counter++;
            //    }
            //}
            //else
            //{
            //    log.Info("No assets in MY assets.");
            //}

        }
        #endregion
    }
}

