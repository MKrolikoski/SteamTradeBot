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
        [JsonProperty("sell_price_normal")]
        public double sell_price_normal { get; set; }

        /// <summary>
        /// Esports key price
        /// </summary>
        [JsonProperty("sell_price_esports")]
        public double sell_price_esports { get; set; }

        /// <summary>
        /// Hydra key price
        /// </summary>
        [JsonProperty("sell_price_hydra")]
        public double sell_price_hydra { get; set; }

        /// <summary>
        /// available money
        /// </summary>
        [JsonProperty("available_money")]
        public double available_money { get; set; }

        /// <summary>
        /// List of admins
        /// </summary>
        [JsonProperty("admins")]
        public List<string> admins { get; set; }


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
            sb.Append("  \"api_key\": \"\",\r\n");
            sb.Append("  \"buy_price\": 1.7,\r\n");
            sb.Append("  \"sell_price_normal\": 1.6,\r\n");
            sb.Append("  \"sell_price_esports\": 1.2,\r\n");
            sb.Append("  \"sell_price_hydra\": 0.8,\r\n");
            sb.Append("  \"available_money\": 20.0,\r\n");
            sb.Append("  \"admins\": []\r\n");
            sb.Append("}\r\n");
            File.WriteAllText("config.cfg", sb.ToString());
        }
    }
}
