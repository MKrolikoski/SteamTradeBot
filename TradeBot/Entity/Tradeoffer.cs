using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeBot.Entity
{
    /// <summary>
    /// Class that represent user trade offer in databse.
    /// </summary>
    public class Tradeoffer
    {
        /// <summary>
        /// Offer id.
        /// </summary>
        public int TradeofferID { get; set; }

        /// <summary>
        /// Transaction id. It is foreign key to table Transactions.
        /// </summary>
        public int TransactionID { get; set; }

        /// <summary>
        /// String with steam offer ID
        /// </summary>
        public string SteamOfferID { get; set; }

        /// <summary>
        /// app id
        /// </summary>
        public int AppId { get; set; }

        /// <summary>
        /// context id
        /// </summary>
        public int ContextId { get; set; }

        /// <summary>
        /// Asset Id
        /// </summary>
        public long AssetId { get; set; }

        /// <summary>
        /// Number of keys that user offers.
        /// </summary>
        public int Amount { get; set; }


        /// <summary>
        /// Default constructor. It is used by library that handle connection with database
        /// </summary>
        public Tradeoffer() { }

        /// <summary>
        /// Constructor that allows user to pass all fields.
        /// </summary>
        /// <param name="TransactionID"> transaction id</param>
        /// <param name="SteamOfferID"> steam offer id</param>
        /// <param name="AppId">cost in USD</param>
        /// <param name="ContextId">total value of an offer</param>
        /// <param name="AssetId">information about offer status</param>
        /// <param name="Amount"> number of keys</param>
        public Tradeoffer(int TransactionID, string SteamOfferID, int AppId, int ContextId, long AssetId, int Amount)
        {
            this.TransactionID = TransactionID;
            this.SteamOfferID = SteamOfferID;
            this.AppId = AppId;
            this.ContextId = ContextId;
            this.AssetId = AssetId;
            this.Amount = Amount;
        }
    }
}
