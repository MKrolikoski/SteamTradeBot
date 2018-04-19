using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;

namespace TradeBot.Messages
{
    public class Message : EventArgs
    {
        public MessageType messageType { get; set; }
        public List<string> parameters { get; set; }
        public SteamID from { get; set; }
        
        public Message(MessageType messageType, List<string> parameters, SteamID from) : base()
        {
            this.messageType = messageType;
            this.parameters = parameters;
            this.from = from;
        }

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
