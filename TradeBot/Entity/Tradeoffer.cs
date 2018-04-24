﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeBot.Entity
{
    public class Tradeoffer
    {
        public int TradeofferID { get; set; }

        public string ItemID { get; set; }

        public int Amount { get; set; }

        public double CostPerOne { get; set; }

        public Tradeoffer() { }

        public Tradeoffer(string ItemID, int Amount, double CostPerOne)
        {
            this.ItemID = ItemID;
            this.Amount = Amount;
            this.CostPerOne = CostPerOne;
        }
    }
}
