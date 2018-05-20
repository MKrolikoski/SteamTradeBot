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
        /// Number of keys that user offers.
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// Cost in USD for each key.
        /// </summary>
        public double CostPerOne { get; set; }

        /// <summary>
        /// Field store information about offer acceptance.
        /// </summary>
        public bool Accepted { get; set; }

        /// <summary>
        /// Default constructor. It is used by library that handle connection with database
        /// </summary>
        public Tradeoffer() { }

        /// <summary>
        /// Constructor that allows user to pass all fields.
        /// </summary>
        /// <param name="TransactionID"> transaction id</param>
        /// <param name="Amount"> number of keys</param>
        /// <param name="CostPerOne">cost in USD</param>
        /// <param name="Accepted">information about offer status</param>
        public Tradeoffer(int TransactionID, int Amount, double CostPerOne, bool Accepted)
        {
            this.TransactionID = TransactionID;
            this.Amount = Amount;
            this.CostPerOne = CostPerOne;
            this.Accepted = Accepted;
        }
    }
}
