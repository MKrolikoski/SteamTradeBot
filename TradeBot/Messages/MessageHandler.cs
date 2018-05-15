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
                MessageType messageType;
                switch (command)
                {
                    case "!help": messageType = MessageType.HELP; break;
                    case "!sell": messageType = MessageType.SELL; break;
                    case "!buy": messageType = MessageType.BUY; break;
                    case "!setethaddress": messageType = MessageType.SETETHADDRESS; break;
                    case "!confirm": messageType = MessageType.CONFIRM; break;
                    case "!info": messageType = MessageType.INFO; break;
                    default: parameters = new List<string>();  messageType = MessageType.UNKNOWN; break;
                }
                if (!checkParams(messageType, parameters))
                    messageType = MessageType.BADPARAMS; 
                return new Message(messageType, parameters, from);
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
            }
            return parameters;
        }

        private bool checkParams(MessageType messageType, List<string> parameters)
        {
            switch(messageType)
            {
                case MessageType.BUY:
                case MessageType.SELL:
                    int number;
                    if (parameters.Count == 1 && Int32.TryParse(parameters[0], out number) && Convert.ToInt32(parameters[0]) > 0)
                        return true;
                    break;
                case MessageType.SETETHADDRESS:
                    if (parameters.Count == 1)
                        return true;
                    break;
                default: return true;
            }
            return false;
        }
    }
}
