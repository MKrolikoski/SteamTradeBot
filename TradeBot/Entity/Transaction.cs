using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeBot.Entity
{
    public class Transaction
    {
        public int TransactionID { get; set; }

        public int UserID { get; set; }

        public DateTime CreationDate;

        public bool Sell { get; set; }

        public bool Buy { get; set; }

        public bool Confirmed { get; set; }

        public Transaction() { }

        public Transaction(int UserID, DateTime CreationDate, bool Sell, bool Buy, bool Confirmed)
        {
            this.UserID = UserID;
            this.CreationDate = CreationDate;
            this.Sell = Sell;
            this.Buy = Buy;
            this.Confirmed = Confirmed;
        }
    }
}
