using System;
using System.Collections.Generic;
using TradeBot.Entity;

namespace TradeBot.Database
{
    public class DatabaseHandler : DataAccess
    {
        public event EventHandler<string> TransactionDeletedEvent;

        public DatabaseHandler() : base() { }


        public bool TransactionConfirmed(string steamID)
        {
            return GetUserTransaction(steamID).Confirmed;
        }

        public bool ConfirmTransaction(string steamID)
        {
            Transaction transaction = GetUserTransaction(steamID);
            if (transaction != null)
            {
                transaction.Confirmed = true;
                return UpdateTransaction(transaction);
            }
            return false;
        }

        public bool TradeOfferAccepted(string steamID)
        {
            return GetUserTradeOffer(steamID).Accepted;
        }

        public bool AcceptTradeOffer(string steamID)
        {
            Tradeoffer tradeoffer = GetUserTradeOffer(steamID);
            if (tradeoffer != null)
            {
                tradeoffer.Accepted = true;
                return UpdateTradeOffer(tradeoffer);
            }
            return false;
        }


        public bool DeleteUserTransaction(string steamID)
        {
            return DeleteTransaction(GetUserTransaction(steamID));
        }

        public bool setEthAddress(string steamID, string address)
        {
            User user = GetUser(steamID);
            user.WalletAddress = address;
            return UpdateUser(user);
        }

        public TransactionStage getTransactionStage(string steamID)
        {
            Transaction transaction = GetUserTransaction(steamID);
            if (transaction == null)
                return TransactionStage.WAITING_FOR_TRANSACTION;
            if (!transaction.Confirmed)
                return TransactionStage.WAITING_FOR_CONFIRMATION;
            Tradeoffer tradeoffer = GetUserTradeOffer(steamID);
            if (!tradeoffer.Accepted)
                return TransactionStage.WAITING_FOR_TRADEOFFER;
            if(transaction.Buy)
                return TransactionStage.WAITING_FOR_ETH;
            return TransactionStage.SENDING_ETH;
        }

        public double getTransactionEthValue(string steamID)
        {
            Tradeoffer tradeoffer = GetUserTradeOffer(steamID);
            return tradeoffer.Amount * tradeoffer.CostPerOne;
        }

        public double getTransactionEthValue(Tradeoffer tradeoffer)
        {        
            return tradeoffer.Amount * tradeoffer.CostPerOne;
        }

        public double getTransactionEthValue(Transaction transaction)
        {
            Tradeoffer tradeoffer = GetTradeOffer(transaction);
            return tradeoffer.Amount * tradeoffer.CostPerOne;
        }

        public void DeleteExpiredTransactions()
        {
            List<Transaction> transactions = GetAllTransactions();
            if (transactions.Count == 0)
                return;
            foreach(var transaction in transactions)
            {
                if ((DateTime.Now - transaction.CreationDate).Days > 0)
                {
                    DeleteTransaction(transaction);
                }
            }
        }

        public new bool DeleteTransaction(Transaction transaction)
        {
            string eventArg;
            if (transaction.Sell)
            {
                var ethAmount = getTransactionEthValue(transaction);
                eventArg = "{\"ethAmount\":\"" + ethAmount + "\"}";
            }
            else
            {
                var keysAmount = GetTradeOffer(transaction).Amount;
                eventArg = "{\"keysAmount\":\"" + keysAmount + "\"}";
            }
            if (base.DeleteTransaction(transaction))
            {
                TransactionDeletedEvent(this, eventArg);
                return true;
            }
            return false;
        }

        public int getReservedKeysAmount()
        {
            List<Transaction> transactions = GetAllTransactions();
            int count = 0;
            foreach(var transaction in transactions)
            {
                if(transaction.Buy)
                {
                    Tradeoffer tradeoffer = GetTradeOffer(transaction);
                    count += tradeoffer.Amount;
                }
            }
            return count;
        }

        public double getReservedEthAmount()
        {
            List<Transaction> transactions = GetAllTransactions();
            double count = 0;
            foreach (var transaction in transactions)
            {
                if (transaction.Sell)
                {
                    Tradeoffer tradeoffer = GetTradeOffer(transaction);
                    count += tradeoffer.Amount * tradeoffer.CostPerOne;
                }
            }
            return count;
        }
    }
}
