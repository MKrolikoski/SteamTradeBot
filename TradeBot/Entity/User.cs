namespace TradeBot.Entity
{
    /// <summary>
    /// Class that represents user class in databse.
    /// </summary>
    public class User
    {
        /// <summary>
        /// User id.
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// String with SteamID.
        /// </summary>
        public string SteamID { get; set; }

        /// <summary>
        /// String with ETH wallet address.
        /// </summary>
        public string WalletAddress { get; set; }

        /// <summary>
        /// Default constructor. It is used by library that handle connection with database
        /// </summary>
        public User()
        {
        }

        /// <summary>
        /// Constructor that allows user to pass all record fields.
        /// </summary>
        /// <param name="SteamID">SteamID</param>
        /// <param name="WalletAddress">ETH wallet address</param>
        public User(string SteamID, string WalletAddress)
        {
            this.SteamID = SteamID;
            this.WalletAddress = WalletAddress;
        }

        override
        public string ToString()
        {
            return UserID + ", " + SteamID + ", " + WalletAddress;
        }
    }
}
