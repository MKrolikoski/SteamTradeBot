using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamTrade.TradeOffer;

namespace TradeBot.Utils
{
    class TradeOfferEqualityComparer : IEqualityComparer<TradeOffer>
    {
        public bool Equals(TradeOffer x, TradeOffer y)
        {
            return (x.TradeOfferId == y.TradeOfferId);
        }

        public int GetHashCode(TradeOffer obj)
        {
            string combined = obj.PartnerSteamId.ToString() + "|" + obj.TradeOfferId;
            return (combined.GetHashCode());
        }
    }
}
