using SteamKit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeBot.Messages
{
    /// <summary>
    /// Class that helps user in handling message operations.
    /// </summary>
    public class MessageHandler
    {
        /// <summary>
        /// Event message handler.
        /// </summary>
        public event EventHandler<Message> MessageProcessedEvent;

        /// <summary>
        /// Method proccess new message.
        /// </summary>
        /// <param name="message">user message</param>
        /// <param name="from">steam id of user who sended message</param>
        public void processMessage(string message, SteamID from)
        {
            Message m = parseMessage(message, from);
            MessageProcessedEvent(this, m);   
        }

        /// <summary>
        /// Method parse input string and check if it is correct.
        /// </summary>
        /// <param name="message">string with message that user sended</param>
        /// <param name="from">steam id of user who sended message</param>
        /// <returns>Message class. If input string can not be parsed method returns message with MessageType.UNKNOWN</returns>
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

        /// <summary>
        /// Method returns message parameters.
        /// </summary>
        /// <param name="message">message class</param>
        /// <returns>List of all method parameters</returns>
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

        /// <summary>
        /// Method checks parameters and return if they are correct.
        /// </summary>
        /// <param name="messageType">type of message</param>
        /// <param name="parameters">list of parameters</param>
        /// <returns>true if parameters are correct, false in other case</returns>
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
