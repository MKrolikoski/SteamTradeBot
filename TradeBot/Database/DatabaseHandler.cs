using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeBot.Entity;

namespace TradeBot.Database
{
    /// <summary>
    /// Class that helps user in handling databse operations.
    /// </summary>
    public class DatabaseHandler : DataAccess
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public DatabaseHandler() : base() { }

        /// <summary>
        /// Method check if user transaction is confirmed.
        /// </summary>
        /// <param name="steamID">user steam id</param>
        /// <returns>true if transaction is confirmed, false in other case</returns>
        public bool TransactionConfirmed(string steamID)
        {
            return GetUserTransaction(steamID).Confirmed;
        }

        /// <summary>
        /// Method set confirmed status in databse record.
        /// </summary>
        /// <param name="steamID">user steam id</param>
        /// <returns>true if record update ended with success, false in other case</returns>
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

        /// <summary>
        /// Method check if user transaction is accepted.
        /// </summary>
        /// <param name="steamID">user steam id</param>
        /// <returns>true if transaction is accepted, false in other case</returns>
        public bool TradeOfferAccepted(string steamID)
        {
            return GetUserTradeOffer(steamID).Accepted;
        }

        /// <summary>
        /// Method set accept status in databse record.
        /// </summary>
        /// <param name="steamID">user steam id</param>
        /// <returns>true if record update ended with success, false in other case</returns>
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

        /// <summary>
        /// Method delete user transaction in databse.
        /// </summary>
        /// <param name="steamID">user steam id</param>
        /// <returns>true if record delete ended with success, false in other case</returns>
        public bool DeleteUserTransaction(string steamID)
        {
            return DeleteTransaction(GetUserTransaction(steamID));
        }

        /// <summary>
        /// Method set ehtereum wallet address.
        /// </summary>
        /// <param name="steamID">user steam id</param>
        /// <param name="address">ethereum wallet address</param>
        /// <returns>true if record update ended with success, false in other case</returns>
        public bool setEthAddress(string steamID, string address)
        {
            User user = GetUser(steamID);
            user.WalletAddress = address;
            return UpdateUser(user);
        }

        /// <summary>
        /// Method checks current transaction stage.
        /// </summary>
        /// <param name="steamID">user steam id</param>
        /// <returns>TransactionStage class</returns>
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

        /// <summary>
        /// Method checks value of transaction in ETH
        /// </summary>
        /// <param name="steamID">user steam id</param>
        /// <returns>value of transaction in ETH</returns>
        public double getTransactionEthValue(string steamID)
        {
            Tradeoffer tradeoffer = GetUserTradeOffer(steamID);
            return tradeoffer.Amount * tradeoffer.CostPerOne;
        }




    }
}
