using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeBot.Bot
{
    /// <summary>
    /// Class that represents possible bot statuses.
    /// Possible statues are: ONLINE, OFFLINE, AWAY, BUSY, LOOKINGTOTRADE.
    /// </summary>
    public enum BotStatus
    {
        /// <summary>
        /// bot has online status on steam
        /// </summary>
        ONLINE,
        /// <summary>
        /// bot has offline status on steam
        /// </summary>
        OFFLINE,
        /// <summary>
        /// bot has away status on steam
        /// </summary>
        AWAY,
        /// <summary>
        /// bot has busy status on steam
        /// </summary>
        BUSY,
        /// <summary>
        /// bot has looking to trade status on steam
        /// </summary>
        LOOKINGTOTRADE
    }
}
