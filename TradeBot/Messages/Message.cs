using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;

namespace TradeBot.Messages
{
    /// <summary>
    /// Class that represents messages sended by users on steam chat.
    /// </summary>
    public class Message : EventArgs
    {
        /// <summary>
        /// Type of current message.
        /// </summary>
        public MessageType messageType { get; set; }
        /// <summary>
        /// List of all parameters in message.
        /// </summary>
        public List<string> parameters { get; set; }
        /// <summary>
        /// Steam id of user who sended message.
        /// </summary>
        public SteamID from { get; set; }
        
        /// <summary>
        /// Default constructor of meesage.
        /// </summary>
        /// <param name="messageType">type of message</param>
        /// <param name="parameters">all message parameters</param>
        /// <param name="from">steam id of user who sended message</param>
        public Message(MessageType messageType, List<string> parameters, SteamID from) : base()
        {
            this.messageType = messageType;
            this.parameters = parameters;
            this.from = from;
        }

        /// <summary>
        /// Implementation of Equals method
        /// </summary>
        /// <param name="obj">object to compare</param>
        /// <returns>true if objects are equal, false in other case</returns>
        public override bool Equals(object obj)
        {
            var message = obj as Message;

            if (message == null)
                return false;

            if (message.messageType != this.messageType || !message.parameters.SequenceEqual(this.parameters) || message.from != this.from)
                return false;

            return true;         
        }


    }
}
