using Newtonsoft.Json;
using System.IO;

namespace TradeBot.Bitstamp
{
    class BitstampAccount
    {
        private BitstampConfig config { get; set; }
        public RequestAuthenticator authenticator { get; set; }

        public BitstampAccount()
        {
            config = new BitstampConfig();
            if (!File.Exists("bitstamp_config.cfg"))
            {
                config.createNew();
            }
            config = JsonConvert.DeserializeObject<BitstampConfig>(File.ReadAllText("bitstamp_config.cfg"));
            authenticator = new RequestAuthenticator(config);
        }

        public string getEthAddress()
        {
            return config.eth_address;
        }
    }
}
