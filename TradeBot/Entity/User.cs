using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeBot.Entity
{
    public class User
    {
        public int UserID { get; set; }

        public string SteamID { get; set; }

        public string WalletAddress { get; set; }

        public User()
        {
        }

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
