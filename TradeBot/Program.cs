using SteamKit2;
using System;
using System.Collections.Generic;
using System.Globalization;
using TradeBot.Bitstamp;
using TradeBot.Bot;
using TradeBot.Database;
using TradeBot.Entity;
using TradeBot.Web;

namespace TradeBot
{
    class Program
    {
        static void Main(string[] args)
        {
            BotCore bot = new BotCore(UserHandlerCreator);


            //BitstampHandler bh = new BitstampHandler();
            //Console.WriteLine(bh.getAvailableEth());
            //Console.ReadKey();


            //DatabaseHandler db = new DatabaseHandler();
            //Transaction tr = db.GetUserTransaction("steamID");
            //Console.WriteLine("Cofnirmed: {0}", tr.Confirmed);
            //Console.ReadKey();


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
        public static UserHandler UserHandlerCreator(BotCore bot, SteamID sid)
        {
            Type controlClass = typeof(TradeOfferUserHandler);

            if (controlClass == null)
                throw new ArgumentException("Configured control class type was null.", "bot");

            return (UserHandler)Activator.CreateInstance(
                    controlClass, new object[] { bot, sid });
        }
    }
}
