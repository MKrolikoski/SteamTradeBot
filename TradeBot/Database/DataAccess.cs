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
    /// <summary>
    /// Class that is used to connect and make changes to the databse.
    /// </summary>
    public class DataAccess
    {
        /// <summary>
        /// Method connect to databse and returns list of all users.
        /// </summary>
        /// <returns> List of all Users</returns>
        public List<User> GetAllUsers()
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                return connection.Query<User>($"SELECT * FROM Users").ToList();
            }
        }

        /// <summary>
        /// Method connect to databse and retur user with steam id same as method parameter.
        /// <param name="steamID">string with user steam id</param> 
        /// </summary>
        /// <returns>User class if user exist or null if not</returns>
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

        /// <summary>
        /// Method connect to databse and add new user passed as argument.
        /// </summary>
        /// <param name="user">user class to add</param> 
        /// <returns>true if operation ended with success, false in other case</returns>
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

        /// <summary>
        /// Method connect to databse and update user passed as argument.
        /// </summary>
        /// <param name="user">user class to update</param> 
        /// <returns>>true if operation ended with success, false in other case</returns>
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

        /// <summary>
        /// Method connect to databse and delete user passed as argument.
        /// </summary>
        /// <param name="user">user class</param> 
        /// <returns>>true if operation ended with success, false in other case</returns>
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

        /// <summary>
        /// Method connect to databse and get transaction with id same as in parameter.
        /// <param name="TransacionID">string with transaction id</param>
        /// </summary>
        /// <returns>Transaction class if it exist in databese, null in other case</returns>
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

        /// <summary>
        /// get all transactions
        /// </summary>
        /// <returns>list of transactions</returns>
        public List<Transaction> GetAllTransactions()
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                return connection.Query<Transaction>($"SELECT * FROM Transactions").ToList();
            }
        }





        /// <summary>
        /// Method connect to databse and returns trasanction that is connected to user with steamID same as in parameter.
        /// <param name="steamID">string with user steamID</param>
        /// </summary>
        /// <returns>Transaction class if it exist in database, null in other case</returns>
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

        /// <summary>
        /// Method connect to databse and add new trasanction passed as argument.
        /// </summary>
        /// <param name="transaction">transaction class to add</param>
        /// <returns>>true if operation ended with success, false in other case</returns>
        public bool AddTransaction(Transaction transaction)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                Transaction transactionInDB = GetTransaction(transaction.TransactionID);
                if (transactionInDB == null)
                {
                    connection.Query<Transaction>($"INSERT INTO Transactions(UserID, CreationDate, Sell,Buy,Confirmed, MoneyTransfered, UpdateTime) VALUES ('{transaction.UserID}','{transaction.CreationDate.ToString("yyyy-MM-dd HH:mm")}',{transaction.Sell.ToString()},{transaction.Buy.ToString()},{transaction.Confirmed.ToString()},{transaction.MoneyTransfered.ToString()}, '{transaction.UpdateTime}')");
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        /// <summary>
        /// Method connect to databse and update transaction passed as argument.
        /// </summary>
        /// <param name="transaction">transaction class to update</param>
        /// <returns>>true if operation ended with success, false in other case</returns>
        public bool UpdateTransaction(Transaction transaction)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                Transaction transactionInDB = GetTransaction(transaction.TransactionID);
                if (transactionInDB != null)
                {
                    connection.Query<Transaction>($"UPDATE Transactions SET CreationDate='{transaction.CreationDate.ToString("yyyy-MM-dd")}', Sell={transaction.Sell.ToString()},Buy={transaction.Buy.ToString()},Confirmed={transaction.Confirmed.ToString()},MoneyTransfered={transaction.MoneyTransfered.ToString()},UpdateTime='{transaction.UpdateTime}' WHERE TransactionID='{transaction.TransactionID}'");
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Method connect to databse and delete transaction passed as argument.
        /// </summary>
        /// <param name="transaction">transaction class to delete</param>
        /// <returns>>true if operation ended with success, false in other case</returns>
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

        /// <summary>
        /// get all trade offers
        /// </summary>
        /// <returns>list of trade offers</returns>
        public List<Tradeoffer> GetAllTradeOffers()
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                return connection.Query<Tradeoffer>($"SELECT * FROM TradeOffers").ToList();
            }
        }

        /// <summary>
        /// Method connect to databse and returns trade offer with id same as in parameter.
        /// </summary>
        /// <param name="tradeOfferID">trade offer id</param>
        /// <returns>Tradeoffer class if it exist in databse, null in other case</returns>
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
        /// <summary>
        /// Method connect to databse and returns user transactions who has steam id same as in parameter.
        /// </summary>
        /// <param name="steamID">string with user steam id</param>  
        /// <returns>Tradeoffer class if it exist in databse, null in other case</returns>
        public Tradeoffer GetUserTradeOffer(string steamID)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                User user = GetUser(steamID);
                if (user != null)
                {
                    try
                    {
                        return connection.Query<Tradeoffer>($"select TradeOfferID, Amount, CostPerOne, Accepted, SteamOfferID, TotalValue from Users natural join Transactions natural join TradeOffers WHERE Users.UserID='{user.UserID}'").Single();
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

        /// <summary>
        /// Method connect to databse and add new trade offer passed as argument.
        /// </summary>
        /// <param name="tradeoffer">Tradeoofer class to add</param>  
        /// <returns>>true if operation ended with success, false in other case</returns>
        public bool AddTradeOffer(Tradeoffer tradeoffer)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                Tradeoffer tradeOfferInDB = GetTradeOffer(tradeoffer.TradeofferID);
                if (tradeOfferInDB == null)
                {
                    connection.Query<Tradeoffer>($"INSERT INTO TradeOffers(TransactionID, SteamOfferID, Amount,CostPerOne, Accepted, TotalValue) VALUES ('{tradeoffer.TransactionID}','{tradeoffer.SteamOfferID}','{tradeoffer.Amount}',{tradeoffer.CostPerOne.ToString(CultureInfo.GetCultureInfo("en-US"))}, {tradeoffer.Accepted.ToString()}, {tradeoffer.TotalValue.ToString(CultureInfo.GetCultureInfo("en-US"))})");
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Method connect to databse and update trade offer passed as argument.
        /// </summary>
        /// <param name="tradeoffer">Tradeoofer class to update</param>  
        /// <returns>>true if operation ended with success, false in other case</returns>
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

        /// <summary>
        /// Method connect to databse and delete trade offer passed as argument.
        /// </summary>
        /// <param name="tradeoffer">Tradeoofer class to delete</param>  
        /// <returns>>true if operation ended with success, false in other case</returns>
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
