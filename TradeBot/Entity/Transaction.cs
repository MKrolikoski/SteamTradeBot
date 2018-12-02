using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeBot.Entity
{
    /// <summary>
    /// Class represents transactions in database.
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// Transaction id.
        /// </summary>
        public int TransactionID { get; set; }

        /// <summary>
        /// User id. It is id in databse, not SteamID
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// Date with information when record was created.
        /// </summary>
        public DateTime CreationDate;

        /// <summary>
        /// Field can be 0 or 1. If it is 1, it means that this is sell offer.
        /// </summary>
        public bool Sell { get; set; }

        /// <summary>
        /// Field can be 0 or 1. If it is 1, it means that this is buy offer.
        /// </summary>
        public bool Buy { get; set; }

        /// <summary>
        /// Field store information about trasaction status.
        /// </summary>
        public bool Confirmed { get; set; }

        /// <summary>
        /// Field store information about money transfer status
        /// </summary>
        public bool MoneyTransfered { get; set; }

        /// <summary>
        /// hour and minute of creation
        /// </summary>
        public string UpdateTime { get; set; }

        /// <summary>
        /// Default constructor. It is used by library that handle connection with database
        /// </summary>
        public Transaction() { }

        /// <summary>
        /// Constructor that allows user to pass all record fields.
        /// </summary>
        /// <param name="UserID">user id</param>
        /// <param name="CreationDate">date with information when record was created</param>
        /// <param name="UpdateTime">creation time</param>
        /// <param name="Sell">0 or 1</param>
        /// <param name="Buy">0 or 1</param>
        /// <param name="Confirmed">transaction status</param>
        /// <param name="MoneyTransfered">money transfer status</param>
        public Transaction(int UserID, DateTime CreationDate, string UpdateTime, bool Sell, bool Buy, bool Confirmed, bool MoneyTransfered)
        {
            this.UserID = UserID;
            this.CreationDate = CreationDate;
            this.UpdateTime = UpdateTime;
            this.Sell = Sell;
            this.Buy = Buy;
            this.Confirmed = Confirmed;
            this.MoneyTransfered = MoneyTransfered;
        }
    }
}
