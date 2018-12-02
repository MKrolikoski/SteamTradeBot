using Newtonsoft.Json;
using System.IO;

namespace TradeBot.Bitstamp
{
    /// <summary>
    /// Class that represents a Bitstamp Account. Information about account are stored in bitstamp_config.cfg file.
    /// </summary>
    class BitstampAccount
    {
        private BitstampConfig config { get; set; }
        /// <summary>
        /// BitStamp account authenticator
        /// </summary>
        public RequestAuthenticator authenticator { get; set; }

        /// <summary>
        /// Defualt class constructor. It reads data from "btistamp_config.cfg" file. If it don't exist the file will be generated.
        /// </summary>
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

        /// <summary>
        /// Returns Ethereum wallet address.
        /// </summary>
        /// <returns>Ethereum wallet address</returns>
        public string getEthAddress()
        {
            return config.eth_address;
        }

        public string getBtcAddress()
        {
            return config.btc_address;
        }
    }
}
