using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;

namespace TradeBot.Bot
{
    public class Utils
    {
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
