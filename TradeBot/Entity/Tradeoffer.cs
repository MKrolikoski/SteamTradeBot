using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeBot.Entity
{
    public class Tradeoffer
    {
        public int TradeofferID { get; set; }

        public int TransactionID { get; set; }

        public int Amount { get; set; }

        public double CostPerOne { get; set; }

        public Tradeoffer() { }

        public Tradeoffer(int TransactionID, int Amount, double CostPerOne)
        {
            this.TransactionID = TransactionID;
            this.Amount = Amount;
            this.CostPerOne = CostPerOne;
        }
    }
}
