using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;

namespace TradeBot.Bot
{
    /// <summary>
    /// Bot necessary utilities.
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// Method checks if it is possible to set SteamID to SteamID class.
        /// </summary>
        /// <param name="input">steamID to check</param>
        /// <param name="steamID"> SteamID class</param>
        /// <returns>return true if steamID is correct or false in other case</returns>
        public static bool TrySetSteamID(string input, out SteamID steamID)
        {
            steamID = new SteamID();

            if (steamID.SetFromString(input, EUniverse.Public)
            || steamID.SetFromSteam3String(input))
            {
                return true;
            }

            if (ulong.TryParse(input, out var numericInput))
            {
                steamID.SetFromUInt64(numericInput);

                return true;
            }

            return false;
        }
    }
}
