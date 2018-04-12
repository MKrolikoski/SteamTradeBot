using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;

namespace TradeBot.Messages
{
    class Message : EventArgs
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
        


    }
}
