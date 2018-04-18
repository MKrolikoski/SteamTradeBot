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
                return connection.Query<User>($"SELECT * FROM Users WHERE SteamID = '{steamID}'").Single();
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
                    connection.Query<User>($"DELETE FROM Users WHERE Users.UserID='{userDB.UserID}'");
                    return true;
                }
                return false;
            }
        }

        public Transaction GetTransaction(string TransacionID)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                return connection.Query<Transaction>($"SELECT * FROM Transactions WHERE Transactions.TransactionID='{TransacionID}'").Single();
            }
        }

        public List<Transaction> GetUserTransaction(string steamID)
        {
            using (IDbConnection connection = new MySqlConnection(Helper.CnnVal("SteamBotDB")))
            {
                User user = GetUser(steamID);
                if(user != null)
                {
                    return connection.Query<Transaction>($"SELECT * FROM Transactions WHERE Transactions.UserID='{user.UserID}'").ToList();
                }
                else
                {
                    return new List<Transaction>();
                }
                
            }
        }

    }
}
