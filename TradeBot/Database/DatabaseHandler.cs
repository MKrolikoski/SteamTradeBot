using System;
using System.Collections.Generic;
using TradeBot.Entity;

namespace TradeBot.Database
{
    /// <summary>
    /// Class that helps user in handling databse operations.
    /// </summary>
    public class DatabaseHandler : DataAccess
    {
        public event EventHandler<string> TransactionDeletedEvent;

        /// <summary>
        /// Default constructor
        /// </summary>
        public DatabaseHandler() : base() { }

        /// <summary>
        /// Method check if user transaction is confirmed.
        /// </summary>
        /// <param name="steamID">user steam id</param>
        /// <returns>true if transaction is confirmed, false in other case</returns>
        public bool TransactionConfirmed(string steamID)
        {
            return GetUserTransaction(steamID).Confirmed;
        }

        /// <summary>
        /// Method set confirmed status in databse record.
        /// </summary>
        /// <param name="steamID">user steam id</param>
        /// <returns>true if record update ended with success, false in other case</returns>
        public bool ConfirmTransaction(string steamID)
        {
            Transaction transaction = GetUserTransaction(steamID);
            if (transaction != null)
            {
                transaction.Confirmed = true;
                return UpdateTransaction(transaction);
            }
            return false;
        }

        /// <summary>
        /// Method check if user transaction is accepted.
        /// </summary>
        /// <param name="steamID">user steam id</param>
        /// <returns>true if transaction is accepted, false in other case</returns>
        public bool TradeOfferAccepted(string steamID)
        {
            //return GetUserTradeOffer(steamID).Accepted;
            return false;
        }

        /// <summary>
        /// Method set accept status in databse record.
        /// </summary>
        /// <param name="steamID">user steam id</param>
        /// <returns>true if record update ended with success, false in other case</returns>
        public bool AcceptTradeOffer(string steamID)
        {
            Tradeoffer tradeoffer = GetUserTradeOffer(steamID);
            if (tradeoffer != null)
            {
                //tradeoffer.Accepted = true;
                return UpdateTradeOffer(tradeoffer);
            }
            return false;
        }

        /// <summary>
        /// Method delete user transaction in databse.
        /// </summary>
        /// <param name="steamID">user steam id</param>
        /// <returns>true if record delete ended with success, false in other case</returns>
        public bool DeleteUserTransaction(string steamID)
        {
            //return DeleteTransaction(GetUserTransaction(steamID));
            return base.DeleteTransaction(GetUserTransaction(steamID));
        }

        /// <summary>
        /// Method set ehtereum wallet address.
        /// </summary>
        /// <param name="steamID">user steam id</param>
        /// <param name="address">ethereum wallet address</param>
        /// <returns>true if record update ended with success, false in other case</returns>
        public bool setEthAddress(string steamID, string address)
        {
            User user = GetUser(steamID);
            user.WalletAddress = address;
            return UpdateUser(user);
        }

        /// <summary>
        /// Method checks current transaction stage.
        /// </summary>
        /// <param name="steamID">user steam id</param>
        /// <returns>TransactionStage class</returns>
        public TransactionStage getTransactionStage(string steamID)
        {
            Transaction transaction = GetUserTransaction(steamID);
            if (transaction == null)
                return TransactionStage.WAITING_FOR_TRANSACTION;
            if (!transaction.Confirmed)
                return TransactionStage.WAITING_FOR_CONFIRMATION;
            Tradeoffer tradeoffer = GetUserTradeOffer(steamID);
            //if (!tradeoffer.Accepted)
            //    return TransactionStage.WAITING_FOR_TRADEOFFER;
            if(transaction.Buy) 
                return TransactionStage.WAITING_FOR_ETH;
            return TransactionStage.SENDING_MONEY;
        }

        /// <summary>
        /// Method checks value of transaction in ETH
        /// </summary>
        /// <param name="steamID">user steam id</param>
        /// <returns>value of transaction in ETH</returns>
        //public double getTransactionEthValue(string steamID)
        //{
        //    Tradeoffer tradeoffer = GetUserTradeOffer(steamID);
        //    return Math.Round(tradeoffer.Amount * tradeoffer.CostPerOne, 8);
        //}

        /// <summary>
        /// Method checks value of transaction in ETH
        /// </summary>
        /// <param name="tradeoffer">tradeoffer from db</param>
        /// <returns>value of transaction in ETH</returns>
        //public double getTransactionEthValue(Tradeoffer tradeoffer)
        //{
        //    return Math.Round(tradeoffer.Amount * tradeoffer.CostPerOne, 8);
        //}

        /// <summary>
        /// Method checks value of transaction in ETH
        /// </summary>
        /// <param name="transaction">transaction from db</param>
        /// <returns>value of transaction in ETH</returns>
        //public double getTransactionEthValue(Transaction transaction)
        //{
        //    Tradeoffer tradeoffer = GetTradeOffer(transaction);
        //    return Math.Round(tradeoffer.Amount * tradeoffer.CostPerOne,8);
        //}


        /// <summary>
        /// Method for deleting expired transactions
        /// </summary>
        public void DeleteExpiredTransactions(Dictionary<SteamTrade.TradeOffer.TradeOffer, bool> pendingSteamOffers)
        {
            List<User> users = GetAllUsers();
            List<User> usersWithExpiredTransactions = new List<User>();
            var steamOffers = pendingSteamOffers.Keys;
            foreach (var user in users)
            {
                Transaction transaction = GetUserTransaction(user.SteamID);
                if (transaction == null || transaction.Confirmed)
                    continue;
                var timeNow = DateTime.Now;
                //var hourMinutes = transaction.UpdateTime.Split(':');
                int updateHour, updateMinute;
                //Int32.TryParse(hourMinutes[0], out updateHour);
                //Int32.TryParse(hourMinutes[1], out updateMinute);
                //nie zrobiona zmiana miesiąca (np. 31.07.2018 23:59 i 01.08.2018 00:01) i roku
                if (timeNow.Year == transaction.CreationDate.Year && timeNow.Month == transaction.CreationDate.Month && timeNow.Day == transaction.CreationDate.Day)
                {
                    Bot.BotCore.log.Info("Same day");

                    //if (timeNow.Hour == updateHour && timeNow.Minute - updateMinute <= 5)
                    //{
                    //    Bot.BotCore.log.Info("Same hour");
                    //    continue;

                    //}
                    //else if (timeNow.Hour - updateHour == 1 && (timeNow.Minute - updateMinute >= -59 || timeNow.Minute - updateMinute <= -55))
                    //    continue;
                }
                else if(timeNow.Year == transaction.CreationDate.Year && timeNow.Month == transaction.CreationDate.Month && timeNow.Day - transaction.CreationDate.Day == 1)
                {
                    Bot.BotCore.log.Info("Different day");

                    //if (timeNow.Hour == 0 && updateHour == 23 && (timeNow.Minute - updateMinute >= -59 || timeNow.Minute - updateMinute <= -55))
                    //    continue;
                }
                Bot.BotCore.log.Info("Deleting");

                usersWithExpiredTransactions.Add(user);
                DeleteTransaction(transaction);
            }
            foreach(var steamOffer in steamOffers)
            {
                foreach(var user in usersWithExpiredTransactions)
                {
                    if(steamOffer.PartnerSteamId.ToString() == user.SteamID)
                    {
                        pendingSteamOffers[steamOffer] = false;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Method for deleting transactions if owner doesn't have an active offer
        /// </summary>
        public void DeleteInactiveTransactions(Dictionary<SteamTrade.TradeOffer.TradeOffer, bool> pendingSteamOffers)
        {
            //Bot.BotCore.log.Info("Deleting inactive");

            List<string> activeUsers = new List<string>();
            foreach(var offer in pendingSteamOffers)
            {
                if (offer.Value == true)
                    activeUsers.Add(offer.Key.PartnerSteamId.ToString());
            }
            List<User> users = GetAllUsers();
            foreach(var user in users)
            {
                if (activeUsers.Contains(user.SteamID))
                    continue;
                else
                {
                    Transaction transaction = GetUserTransaction(user.SteamID);
                    if (transaction != null)
                        DeleteTransaction(transaction);
                }
            }           
        }

        /// <summary>
        /// Method for deleting specified transaction 
        /// </summary>
        /// <param name="transaction">transaction to be deleted</param>
        /// <returns>true if transaction was successfully deleted</returns>
        public new bool DeleteTransaction(Transaction transaction)
        {
            if(transaction != null)
            {
                //string eventArg;
                //if (transaction.Sell)
                //{
                //    var ethAmount = getTransactionEthValue(transaction);
                //    eventArg = "{\"ethAmount\":\"" + ethAmount + "\"}";
                //}
                //else
                //{
                //    var keysAmount = GetTradeOffer(transaction).Amount;
                //    eventArg = "{\"keysAmount\":\"" + keysAmount + "\"}";
                //}
                if (base.DeleteTransaction(transaction))
                {
                    //TransactionDeletedEvent(this, eventArg);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Method for getting amount of reserved keys in db transactions
        /// </summary>
        /// <returns>amount of keys</returns>
        public int getReservedKeysAmount()
        {
            List<Transaction> transactions = GetAllTransactions();
            int count = 0;
            foreach(var transaction in transactions)
            {
                if(transaction.Buy)
                {
                    Tradeoffer tradeoffer = GetTradeOffer(transaction);
                    count += tradeoffer.Amount;
                }
            }
            return count;
        }

        /// <summary>
        /// Method for getting amount of reserved eth in db transactions
        /// </summary>
        /// <returns>amount of eth</returns>
        public double getReservedEthAmount()
        {
            List<Transaction> transactions = GetAllTransactions();
            double count = 0;
            foreach (var transaction in transactions)
            {
                if (transaction.Sell)
                {
                    Tradeoffer tradeoffer = GetTradeOffer(transaction);
                    //count += tradeoffer.Amount * tradeoffer.CostPerOne;
                }
            }
            return count;
        }
    }
}
