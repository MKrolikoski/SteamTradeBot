using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeBot.Entity;

namespace TradeBot.Database
{
    public class DatabaseHandler : DataAccess
    {
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




    }
}
