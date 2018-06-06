using SteamKit2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using TradeBot.Bitstamp;
using TradeBot.Bot;
using TradeBot.Database;
using TradeBot.Entity;
using TradeBot.GUIForms;
using TradeBot.Web;

namespace TradeBot
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
           // BotCore bot = new BotCore();

            var form = new MainWindowForm();
            ((log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetLoggerRepository()).Root.AddAppender(form);
            Application.Run(form);
            //form.ShowDialog();

            //BotCore bot = new BotCore(UserHandlerCreator);




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
