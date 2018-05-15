using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeBot.Messages
{
    public enum MessageType
    {
        SELL,
        BUY,
        HELP,
        SETETHADDRESS,
        CONFIRM,
        INFO,
        BADPARAMS,
        UNKNOWN
    }
}
