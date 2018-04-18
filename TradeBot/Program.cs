using System.Collections.Generic;
using TradeBot.Bot;
using TradeBot.Database;
using TradeBot.Entity;
using MySql.Data.MySqlClient;
using System;

namespace TradeBot
{
    class Program
    {
        static void Main(string[] args)
        {
            BotCore bot = new BotCore();
            /*
            DataAccess db = new DataAccess();

            List<User> users = db.GetAllUsers();

            foreach(User s in users)
            {
                Console.WriteLine(s);
            }

            User user = db.GetUser("testSteamID1");

            Console.WriteLine(user);

            Console.WriteLine(db.AddUser(user));
            Console.WriteLine(db.DeleteUser(user));
            */
        }
    }
}
