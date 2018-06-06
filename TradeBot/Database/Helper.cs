using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeBot.Database
{
    /// <summary>
    /// Utilities to handle databse connection.
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Method get connection string named as method parameter.
        /// </summary>
        /// <param name="name">name of connection string</param>
        /// <returns>connection string</returns>
        public static string CnnVal(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }
    }
}
