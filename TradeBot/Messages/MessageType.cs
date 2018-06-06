using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeBot.Messages
{
    /// <summary>
    /// Class that represents types of messages.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// status when user send sell request
        /// </summary>
        SELL,
        /// <summary>
        /// status when user send buy request
        /// </summary>
        BUY,
        /// <summary>
        /// user need help information
        /// </summary>
        HELP,
        /// <summary>
        /// user wants to set new wallet address
        /// </summary>
        SETETHADDRESS,
        /// <summary>
        /// user confirm transaction
        /// </summary>
        CONFIRM,
        /// <summary>
        /// user wants information about his account
        /// </summary>
        INFO,
        /// <summary>
        /// 
        /// </summary>
        BADPARAMS,
        /// <summary>
        /// status when message cannot be parsed
        /// </summary>
        UNKNOWN
    }
}
