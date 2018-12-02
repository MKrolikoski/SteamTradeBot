using SteamKit2;
using SteamToolkit.Trading;
using System;
using static SteamTrade.TradeOffer.TradeOffer.TradeStatusUser;

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

        public static RgInventoryItem createRgInventoryItemFromAsset(TradeAsset asset)
        {
            RgInventoryItem item = new RgInventoryItem();
            item.ClassId = asset.ClassId;
            item.Id = asset.AssetId;
            item.InstanceId = asset.InstanceId;
            return item;
        }

        public static string getAssetMarketName(TradeAsset asset, string apiKey)
        {
            return createRgInventoryItemFromAsset(asset).ToCEconAsset((uint)asset.AppId).GetMarketHashName(apiKey);
        }
    }
}
