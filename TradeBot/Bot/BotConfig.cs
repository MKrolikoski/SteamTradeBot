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
    public class BotConfig
    {
        [JsonProperty("working")]
        public bool working { get; set; }

        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public BotStatus status { get; set; }

        [JsonProperty("login")]
        public string login { get; set; }

        [JsonProperty("password")]
        public string password { get; set; }

        [JsonProperty("shared_secret")]
        public string shared_secret { get; set; }

        [JsonProperty("api_key")]
        public string api_key { get; set; }

        [JsonProperty("transaction_toll")]
        public float transaction_toll { get; set; }


        public void save()
        {
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("config.cfg", output);
        }

        public void exportTo(string path)
        {
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(path, output);
        }

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
            sb.Append("  \"transaction_toll\": 0.1\r\n");
            sb.Append("}\r\n");
            File.WriteAllText("config.cfg", sb.ToString());
        }
    }
}
