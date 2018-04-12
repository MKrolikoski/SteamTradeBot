using SteamKit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeBot.Messages
{
    class MessageHandler
    {
        public event EventHandler<Message> MessageProcessedEvent;


        public void processMessage(string message, SteamID from)
        {
            Message m = parseMessage(message, from);
            MessageProcessedEvent(this, m);   
        }

        private Message parseMessage(string message, SteamID from)
        {
            try
            {
                if (message.Length <= 1)
                {
                    return new Message(MessageType.UKNOWN, null, from);
                }
                message = message.Trim();
                if (message.Remove(1) != "!")
                {
                    return new Message(MessageType.UKNOWN, null, from);
                }
                string command;
                List<string> parameters;
                if (message.Contains(" "))
                {
                    command = message.Remove(message.IndexOf(" ")).ToLower();
                    parameters = getParams(message.Substring(message.IndexOf(" ")).Trim());
                }
                else
                {
                    command = message;
                    parameters = null;
                }
                switch (command)
                {
                    case "!help": return new Message(MessageType.HELP, parameters, from);
                    case "!sell": return new Message(MessageType.SELL, parameters, from);
                    case "!buy": return new Message(MessageType.BUY, parameters, from);
                    case "!changewalletaddress": return new Message(MessageType.CHANGE_WALLET_ADDRESS, parameters, from);
                    default: return new Message(MessageType.UKNOWN, null, from);
                }
            }catch(Exception e)
            {
                return new Message(MessageType.UKNOWN, null, from);
            }
        }

        private List<string> getParams(string message)
        {
            List<string> parameters = new List<string>();
            string[] p = message.Split(' ');
            foreach (string s in p)
                parameters.Add(s);
            return parameters;
        }
    }
}
