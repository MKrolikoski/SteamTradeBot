using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TradeBot.Bot
{
    /// <summary>
    /// Class that represents a bot configuration file.
    /// </summary>
    public class BotConfig
    {
        /// <summary>
        /// Bot working status (true,false).
        /// </summary>
        [JsonProperty("working")]
        public bool working { get; set; }

        /// <summary>
        /// Bot status (ONLINE, OFFLINE, AWAY, BUSY, LOOKINGTOTRADE).
        /// </summary>
        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public BotStatus status { get; set; }

        /// <summary>
        /// Steam login.
        /// </summary>
        [JsonProperty("login")]
        public string login { get; set; }

        /// <summary>
        /// Password to steam account.
        /// </summary>
        [JsonProperty("password")]
        public string password { get; set; }

        /// <summary>
        /// Secret key connected with Bitstamp account.
        /// </summary>
        [JsonProperty("shared_secret")]
        public string shared_secret { get; set; }

        /// <summary>
        /// API key connected with Bitstamp account
        /// </summary>
        [JsonProperty("api_key")]
        public string api_key { get; set; }

        /// <summary>
        /// Buying coefficient.
        /// </summary>
        [JsonProperty("buy_price")]
        public double buy_price { get; set; }

        /// <summary>
        /// Selling coefficient.
        /// </summary>
        [JsonProperty("sell_price")]
        public double sell_price { get; set; }
        
        /// <summary>
        /// Charged transaction fee.
        /// </summary>
        [JsonProperty("transaction_toll")]
        public double transaction_toll { get; set; }

        /// <summary>
        /// Save current confiugration to file.
        /// </summary>
        public void save()
        {
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("config.cfg", output);
        }

        /// <summary>
        /// Save current configuration to file in specific place.
        /// </summary>
        /// <param name="path">path to save file</param>
        public void exportTo(string path)
        {
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(path, output);
        }

        /// <summary>
        /// Create default configuration file
        /// </summary>
        public void createNew()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\r\n");
            sb.Append("  \"working\": false,\r\n");
            sb.Append("  \"status\": \"ONLINE\",\r\n");
            sb.Append("  \"login\": \"\",\r\n");
            sb.Append("  \"password\": \"\",\r\n");
            sb.Append("  \"shared_secret\": \"\",\r\n");
            sb.Append("  \"api_key\": \"\",\r\n");
            sb.Append("  \"buy_price\": 1.7,\r\n");
            sb.Append("  \"sell_price\": 1.6,\r\n");
            sb.Append("  \"transaction_toll\": 0.1\r\n");
            sb.Append("}\r\n");
            File.WriteAllText("config.cfg", sb.ToString());
        }
    }
}
