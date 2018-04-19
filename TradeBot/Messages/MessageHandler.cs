using SteamKit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeBot.Messages
{
    public class MessageHandler
    {
        public event EventHandler<Message> MessageProcessedEvent;



        public void processMessage(string message, SteamID from)
        {
            Message m = parseMessage(message, from);
            MessageProcessedEvent(this, m);   
        }

        public Message parseMessage(string message, SteamID from)
        {
            try
            {
                string command;
                List<string> parameters = new List<string>();
                if (message.Length <= 1)
                {
                    return new Message(MessageType.UNKNOWN, parameters, from);
                }
                message = message.Trim();
                if (message.Remove(1) != "!")
                {
                    return new Message(MessageType.UNKNOWN, parameters, from);
                }
                if (message.Contains(" "))
                {
                    command = message.Remove(message.IndexOf(" ")).ToLower();
                    parameters = getParams(message.Substring(message.IndexOf(" ")).Trim());
                }
                else
                {
                    command = message.ToLower();
                }
                switch (command)
                {
                    case "!help": return new Message(MessageType.HELP, parameters, from);
                    case "!sell": return new Message(MessageType.SELL, parameters, from);
                    case "!buy": return new Message(MessageType.BUY, parameters, from);
                    case "!changewalletaddress": return new Message(MessageType.CHANGE_WALLET_ADDRESS, parameters, from);
                    default: parameters = new List<string>();  return new Message(MessageType.UNKNOWN, parameters, from);
                }
            }catch(Exception e)
            {
                return new Message(MessageType.UNKNOWN, null, from);
            }
        }

        public List<string> getParams(string message)
        {
            List<string> parameters = new List<string>();
            string[] p = message.Split(' ');
            foreach (string s in p)
            { 
                parameters.Add(s);
                Console.WriteLine("---{0}---", s);
            }
            return parameters;
        }
    }
}
