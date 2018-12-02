using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeBot.Bitstamp
{
    /// <summary>
    /// Class represents a Bitstamp account configuration. It store information about authentication data and information about account.
    /// </summary>
    class BitstampConfig
    {
        /// <summary>
        /// Bitstamp api key that user can get from his Bitstamp account.
        /// </summary>
        [JsonProperty("api_key")]
        public string api_key { get; set; }

        /// <summary>
        /// Bitstamp api secret key that user can get from his Bitstamp account.
        /// </summary>
        [JsonProperty("api_secret")]
        public string api_secret { get; set; }

        /// <summary>
        /// Bitstamp account id.
        /// </summary>
        [JsonProperty("customer_id")]
        public string customer_id { get; set; }

        /// <summary>
        /// Ehereum wallet address from Bistamp account.
        /// </summary>
        [JsonProperty("eth_address")]
        public string eth_address { get; set; }

        /// <summary>
        /// Bitcoin wallet address from Bistamp account.
        /// </summary>
        [JsonProperty("btc_address")]
        public string btc_address { get; set; }


        /// <summary>
        /// Method allows to save configuration to file. Default filename is "bitstamp_config.cfg".
        /// </summary>
        public void save()
        {
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("bitstamp_config.cfg", output);
        }

        /// <summary>
        /// Method allows to save configuration to a file in the location specified by the user.
        /// </summary>
        /// <param name="path">Path to save file</param>
        public void exportTo(string path)
        {
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(path, output);
        }

        /// <summary>
        /// Method create new config file based on information stored in class fields.
        /// </summary>
        public void createNew()
        {
            getInfoFromUser();
            StringBuilder sb = new StringBuilder();
            sb.Append("{\r\n");
            sb.Append("  \"api_key\": \""+api_key+"\",\r\n");
            sb.Append("  \"api_secret\": \"" + api_secret + "\",\r\n");
            sb.Append("  \"customer_id\": \"" + customer_id + "\",\r\n");
            sb.Append("  \"eth_address\": \"" + eth_address + "\",\r\n");
            sb.Append("  \"btc_address\": \"" + btc_address + "\"\r\n");
            sb.Append("}\r\n");
            File.WriteAllText("bitstamp_config.cfg", sb.ToString());
        }

        /// <summary>
        /// Method get information about account from user.
        /// </summary>
        private void getInfoFromUser()
        {
            Console.Write("Bitstamp API key: ");
            api_key = Console.ReadLine();
            Console.Write("API secret: ");
            api_secret = Console.ReadLine();
            Console.Write("CustomerID: ");
            customer_id = Console.ReadLine();
            Console.Write("ETH address: ");
            eth_address = Console.ReadLine();
            Console.Write("BTC address: ");
            btc_address = Console.ReadLine();
        }
    }
}
