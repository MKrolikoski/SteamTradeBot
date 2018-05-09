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




    }
}
