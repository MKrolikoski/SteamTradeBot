using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeBot.Bitstamp
{
    class BitstampConfig
    {
        [JsonProperty("api_key")]
        public string api_key { get; set; }

        [JsonProperty("api_secret")]
        public string api_secret { get; set; }

        [JsonProperty("customer_id")]
        public string customer_id { get; set; }

        //on bitstamp: account -> deposit -> eth
        [JsonProperty("eth_address")]
        public string eth_address { get; set; }


        public void save()
        {
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("bitstamp_config.cfg", output);
        }

        public void exportTo(string path)
        {
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(path, output);
        }

        public void createNew()
        {
            getInfoFromUser();
            StringBuilder sb = new StringBuilder();
            sb.Append("{\r\n");
            sb.Append("  \"api_key\": \""+api_key+"\",\r\n");
            sb.Append("  \"api_secret\": \"" + api_secret + "\",\r\n");
            sb.Append("  \"customer_id\": \"" + customer_id + "\",\r\n");
            sb.Append("  \"eth_address\": \"" + eth_address + "\"\r\n");
            sb.Append("}\r\n");
            File.WriteAllText("bitstamp_config.cfg", sb.ToString());
        }

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

        }
    }
}
