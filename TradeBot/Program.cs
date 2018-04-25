using System.Collections.Generic;
using TradeBot.Bot;
using TradeBot.Database;
using TradeBot.Entity;
using MySql.Data.MySqlClient;
using System;
using TradeBot.Web;

namespace TradeBot
{
    class Program
    {
        static void Main(string[] args)
        {
            BotCore bot = new BotCore();
            /*
            DataAccess db = new DataAccess();
            User user = new User("steamID1", "walletAddress1");
            User user2 = new User("steamID2", "walletAddress2");
            User user3 = new User("steamID3", "walletAddress3");
            Transaction transaction = new Transaction(2,1,false,false,false);
            Tradeoffer tradeOffer = new Tradeoffer("item1", 1, 1.0);

            db.AddTradeOffer(tradeOffer);
            db.AddUser(user);
            db.AddUser(user2);
            db.AddTransaction(transaction);
            */
        }
    }
}
