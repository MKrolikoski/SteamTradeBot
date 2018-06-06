using Newtonsoft.Json;
using System;

namespace TradeBot.Bitstamp
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class TransactionInfo
    {
        [JsonProperty(PropertyName = "usd")]
        public string usd { get; set; }

        [JsonProperty(PropertyName = "btc")]
        public string btc { get; set; }

        [JsonProperty(PropertyName = "btc_usd")]
        public string btc_usd { get; set; }

        [JsonProperty(PropertyName = "order_id")]
        public string order_id { get; set; }

        [JsonProperty(PropertyName = "fee")]
        public string fee { get; set; }

        [JsonProperty(PropertyName = "type")]
        public int type { get; set; }

        [JsonProperty(PropertyName = "id")]
        public int id { get; set; }

        [JsonProperty(PropertyName = "datetime")]
        public DateTime datetime { get; set; }
    }
}

