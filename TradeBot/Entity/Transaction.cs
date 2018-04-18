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

        public int TradeofferID { get; set; }

        public bool Sell { get; set; }

        public bool Buy { get; set; }

        public bool Completed { get; set; }
    }
}
