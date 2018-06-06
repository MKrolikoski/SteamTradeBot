using System;
using System.Collections.Generic;
using System.Linq;
using TradeBot.Entity;
using Dapper;
using System.Data;
using MySql.Data.MySqlClient;
using System.Globalization;

namespace TradeBot.Database
{
    public class DataAccess
    {
        public List<User> GetAllUsers()
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                return connection.Query<User>($"SELECT * FROM Users").ToList();
            }
        }

        public User GetUser(string steamID)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                try
                {
                    return connection.Query<User>($"SELECT * FROM Users WHERE SteamID = '{steamID}'").Single();
                } catch (InvalidOperationException)
                {
                    return null;
                }
            }
        }

        public bool AddUser(User user)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB"))) {
                User userDB = GetUser(user.SteamID);
                if(userDB == null)
                {
                    connection.Query<User>($"INSERT INTO Users(SteamID,WalletAddress) VALUES ('{user.SteamID}','{user.WalletAddress}')");
                    return true;
                }             
                return false;
            }
        }

        public bool UpdateUser(User user)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                User userDB = GetUser(user.SteamID);
                if (userDB != null)
                {
                    connection.Query<Transaction>($"UPDATE Users SET WalletAddress='{user.WalletAddress}' WHERE UserID='{user.UserID}'");
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        public bool DeleteUser(User user)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                User userDB = GetUser(user.SteamID);
                if (userDB != null)
                {
                    connection.Query<User>($"DELETE FROM Users WHERE Users.UserID='{userDB.UserID}' OR Users.SteamID='{user.SteamID}'");
                    DeleteTradeOffer(GetUserTradeOffer(userDB.SteamID));
                    return true;
                }
                return false;
            }
        }

        public Transaction GetTransaction(int TransacionID)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                try
                {
                    return connection.Query<Transaction>($"SELECT * FROM Transactions WHERE Transactions.TransactionID='{TransacionID}'").Single();
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            }
        }

        public List<Transaction> GetAllTransactions()
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                return connection.Query<Transaction>($"SELECT * FROM Transactions").ToList();
            }
        }



        //User is allowed only one transaction at a time, adding new overrides previous one
        //public List<Transaction> GetUserTransactions(string steamID)
        //{
        //    using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
        //    {
        //        User user = GetUser(steamID);
        //        if(user != null)
        //        {
        //            try { 
        //                return connection.Query<Transaction>($"SELECT * FROM Transactions WHERE Transactions.UserID='{user.UserID}'").ToList();
        //            }
        //            catch (InvalidOperationException)
        //            {
        //                return null;
        //            }
        //        }
        //        else
        //        {
        //            return null;
        //        }

        //    }
        //}

        public Transaction GetUserTransaction(string steamID)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                User user = GetUser(steamID);
                if (user != null)
                {
                    try
                    {
                        return connection.Query<Transaction>($"SELECT * FROM Transactions WHERE Transactions.UserID='{user.UserID}'").Single();
                    }
                    catch (InvalidOperationException)
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }

            }
        }

        public bool AddTransaction(Transaction transaction)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                Transaction transactionInDB = GetTransaction(transaction.TransactionID);
                if (transactionInDB == null)
                {
                    connection.Query<Transaction>($"INSERT INTO Transactions(UserID, CreationDate, Sell,Buy,Confirmed) VALUES ('{transaction.UserID}','{transaction.CreationDate.ToString("yyyy-MM-dd HH:mm")}',{transaction.Sell.ToString()},{transaction.Buy.ToString()},{transaction.Confirmed.ToString()})");
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        public bool UpdateTransaction(Transaction transaction)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                Transaction transactionInDB = GetTransaction(transaction.TransactionID);
                if (transactionInDB != null)
                {
                    connection.Query<Transaction>($"UPDATE Transactions SET CreationDate='{transaction.CreationDate.ToString("yyyy-MM-dd")}', Sell={transaction.Sell.ToString()},Buy={transaction.Buy.ToString()},Confirmed={transaction.Confirmed.ToString()} WHERE TransactionID='{transaction.TransactionID}'");
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool DeleteTransaction(Transaction transaction)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                Transaction transactionDB = GetTransaction(transaction.TransactionID);
                if (transactionDB != null)
                {
                    connection.Query<User>($"DELETE FROM Transactions WHERE Transactions.TransactionID='{transactionDB.TransactionID}'");
                    return true;
                }
                return false;
            }
        }

        public List<Tradeoffer> GetAllTradeOffers()
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                return connection.Query<Tradeoffer>($"SELECT * FROM TradeOffers").ToList();
            }
        }

        public Tradeoffer GetTradeOffer(int tradeOfferID)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                try { 
                    return connection.Query<Tradeoffer>($"SELECT * FROM TradeOffers WHERE TradeOffers.TradeOfferID='{tradeOfferID}'").Single();
                }
                    catch (InvalidOperationException)
                {
                    return null;
                }
            }
        }

        public Tradeoffer GetTradeOffer(Transaction transaction)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                try
                {
                    return connection.Query<Tradeoffer>($"SELECT * FROM TradeOffers WHERE TradeOffers.TransactionID='{transaction.TransactionID}'").Single();
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            }
        }


        public Tradeoffer GetUserTradeOffer(string steamID)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                User user = GetUser(steamID);
                if (user != null)
                {
                    try
                    {
                        return connection.Query<Tradeoffer>($"select TradeOfferID, Amount, CostPerOne, Accepted from Users natural join Transactions natural join TradeOffers WHERE Users.UserID='{user.UserID}'").Single();
                    }
                    catch (InvalidOperationException)
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }

            }
        }

        public bool AddTradeOffer(Tradeoffer tradeoffer)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                Tradeoffer tradeOfferInDB = GetTradeOffer(tradeoffer.TradeofferID);
                if (tradeOfferInDB == null)
                {
                    connection.Query<Tradeoffer>($"INSERT INTO TradeOffers(TransactionID, Amount,CostPerOne, Accepted) VALUES ('{tradeoffer.TransactionID}','{tradeoffer.Amount}',{tradeoffer.CostPerOne.ToString(CultureInfo.GetCultureInfo("en-US"))}, {tradeoffer.Accepted.ToString()})");
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool UpdateTradeOffer(Tradeoffer tradeoffer)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                Tradeoffer tradeOfferInDB = GetTradeOffer(tradeoffer.TradeofferID);
                if (tradeOfferInDB != null)
                {
                    connection.Query<Tradeoffer>($"UPDATE TradeOffers SET Amount='{tradeoffer.Amount}',CostPerOne={tradeoffer.CostPerOne.ToString(CultureInfo.GetCultureInfo("en-US"))}, Accepted={tradeoffer.Accepted.ToString()} WHERE TradeOfferID='{tradeoffer.TradeofferID}'");
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool DeleteTradeOffer(Tradeoffer tradeoffer)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                Tradeoffer tradeofferDB = GetTradeOffer(tradeoffer.TradeofferID);
                if (tradeofferDB != null)
                {
                    connection.Query<User>($"DELETE FROM TradeOffers WHERE TradeOffers.TradeOfferID='{tradeofferDB.TradeofferID}'");
                    return true;
                }
                return false;
            }
        }
    }
}
