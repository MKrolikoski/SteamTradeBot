using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeBot.Entity;
using Dapper;
using System.Data;
using MySql.Data.MySqlClient;

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


        public bool DeleteUser(User user)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                User userDB = GetUser(user.SteamID);
                if (userDB != null)
                {
                    connection.Query<User>($"DELETE FROM Users WHERE Users.UserID='{userDB.UserID}' OR Users.SteamID='{user.SteamID}'");
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

        public List<Transaction> GetUserTransactions(string steamID)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                User user = GetUser(steamID);
                if(user != null)
                {
                    try { 
                        return connection.Query<Transaction>($"SELECT * FROM Transactions WHERE Transactions.UserID='{user.UserID}'").ToList();
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
                    connection.Query<Transaction>($"INSERT INTO Transactions(UserID,TradeOfferID,Sell,Buy,Completed) VALUES ('{transaction.UserID}','{transaction.TradeofferID}','{transaction.Sell}','{transaction.Buy}','{transaction.Completed}')");
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
                if (transactionInDB == null)
                {
                    connection.Query<Transaction>($"UPDATE Transactions SET Sell='{transaction.Sell}',Buy='{transaction.Buy}',Completed='{transaction.Completed}' WHERE TransactionID='{transaction.TransactionID}'");
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        public Tradeoffer GetTradeOffer(int tradeOfferID)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                try { 
                    return connection.Query<Tradeoffer>($"SELECT * FROM TradeOffer WHERE TradeOffer.TradeOfferID='{tradeOfferID}'").Single();
                }
                    catch (InvalidOperationException)
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
                    connection.Query<Tradeoffer>($"INSERT INTO TradeOffer(ItemID,Amount,CostPerOne) VALUES ('{tradeoffer.ItemID}','{tradeoffer.Amount}','{tradeoffer.CostPerOne}')");
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
                if (tradeOfferInDB == null)
                {
                    connection.Query<Tradeoffer>($"UPDATE TradeOffer SET ItemID='{tradeoffer.ItemID}',Amount='{tradeoffer.Amount}',CostPerOne='{tradeoffer.CostPerOne}' WHERE TradeOfferID='{tradeoffer.TradeofferID}'");
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

    }
}
