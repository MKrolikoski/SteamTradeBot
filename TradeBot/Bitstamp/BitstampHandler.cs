﻿using RestSharp;
using System;
using System.Globalization;
using System.Net;
using TradeBot.Bot;
using TradeBot.Web;

namespace TradeBot.Bitstamp
{
    /// <summary>
    /// Class that allows user to get informtion from Bitstamp website using theri API.
    /// </summary>
    class BitstampHandler
    {
        private BitstampAccount account;

        /// <summary>
        /// Defualt class constructor. It get create new user using information stored in configuration file.
        /// </summary>
        public BitstampHandler()
        {
            account = new BitstampAccount();
        }


        /// <summary>
        /// Method allows user to check a account balance. It shows result on console output.
        /// </summary>
        public void checkBalance()
        {
            var baseUrl = "https://www.bitstamp.net/api/balance/";
            var client = new RestClient(baseUrl);
            RestRequest request = account.authenticator.authenticate(new RestRequest(Method.POST));          
            var response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Bot.BotCore.log.Info("Balance:\n" + response.Content);
            }
            else
                Bot.BotCore.log.Error("Error while checking balance");
        }


        /// <summary>
        /// Method allows user to get information about eth price.
        /// </summary>
        /// <returns>double value with ETH price</returns>
        public double getEthPriceForOneUsd()
        {
            var baseUrl = "https://www.bitstamp.net/api/v2/ticker/ethusd/";
            var client = new RestClient(baseUrl);
            RestRequest request = new RestRequest(Method.GET);
            var response = client.Execute(request);
            if(response.StatusCode == HttpStatusCode.OK)
            {
                var cultureInfo = (CultureInfo)CultureInfo.InvariantCulture.Clone();
                cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
                cultureInfo.NumberFormat.NumberGroupSeparator = ".";
                return 1/Convert.ToDouble(WebUtils.GetJSONAtribute(response.Content, "vwap"), cultureInfo);
            }
            return -1;
        }

        /// <summary>
        /// Method returns a number with available ETH on Bitstamp account.
        /// </summary>
        /// <returns>double value with ETH available on account</returns>
        public double getAvailableEth()
        {
            var baseUrl = "https://www.bitstamp.net/api/v2/balance/ethusd/";
            var client = new RestClient(baseUrl);
            RestRequest request = account.authenticator.authenticate(new RestRequest(Method.POST));
            var response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var cultureInfo = (CultureInfo)CultureInfo.InvariantCulture.Clone();
                cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
                cultureInfo.NumberFormat.NumberGroupSeparator = ".";
                return Convert.ToDouble(WebUtils.GetJSONAtribute(response.Content, "eth_available"), cultureInfo);
            }
            return -1;
        }

        /// <summary>
        /// Method returns a number with available Btc on Bitstamp account.
        /// </summary>
        /// <returns>double value with Btc available on account</returns>
        public double getAvailableBtc()
        {
            var baseUrl = "https://www.bitstamp.net/api/v2/balance/btcusd/";
            var client = new RestClient(baseUrl);
            RestRequest request = account.authenticator.authenticate(new RestRequest(Method.POST));
            var response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var cultureInfo = (CultureInfo)CultureInfo.InvariantCulture.Clone();
                cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
                cultureInfo.NumberFormat.NumberGroupSeparator = ".";
                return Convert.ToDouble(WebUtils.GetJSONAtribute(response.Content, "btc_available"), cultureInfo);
            }
            return -1;
        }

        /// <summary>
        /// Sends eth to specified address
        /// </summary>
        /// <param name="to">address to which eth is sent</param>
        /// <param name="amount">amount of eth</param>
        /// <returns>true if transfer was successful, false otherwise</returns>
        public bool sendEth(string to, double amount)
        {
            
            var baseUrl = "https://www.bitstamp.net/api/v2/eth_withdrawal/";
            var client = new RestClient(baseUrl);
            RestRequest request = account.authenticator.authenticate(new RestRequest(Method.POST));
            request.AddParameter("address", to);
            request.AddParameter("amount", amount.ToString(CultureInfo.GetCultureInfo("en-US")));
            var response = client.Execute(request);
            if (WebUtils.GetJSONAtribute(response.Content, "status") != "error")
            {
                BotCore.log.Info("Sent " + amount + "Eth to: "+ to + ".");
                return true;
            }
            else
            {
                BotCore.log.Error("Eth transfer to: " + to + " failed.");
                BotCore.log.Error("Reason: " + response.Content);
                return false;
            }
        }

        /// <summary>
        /// Sends btc to specified address
        /// </summary>
        /// <param name="to">address to which btc is sent</param>
        /// <param name="amount">amount of btc</param>
        /// <returns>true if transfer was successful, false otherwise</returns>
        public bool sendBtc(string to, double amount)
        {

            var baseUrl = "https://www.bitstamp.net/api/bitcoin_withdrawal/";
            var client = new RestClient(baseUrl);
            RestRequest request = account.authenticator.authenticate(new RestRequest(Method.POST));
            request.AddParameter("address", to);
            request.AddParameter("amount", amount.ToString(CultureInfo.GetCultureInfo("en-US")));
            request.AddParameter("instant", 0);
            var response = client.Execute(request);
            if (WebUtils.GetJSONAtribute(response.Content, "status") != "error")
            {
                BotCore.log.Info("Sent " + amount + "Btc to: " + to + ".");
                return true;
            }
            else
            {
                BotCore.log.Error("Btc transfer to: " + to + " failed.");
                BotCore.log.Error("Reason: " + response.Content);
                return false;
            }
        }

        /// <summary>
        /// gets user transactions from bitstamp
        /// </summary>
        /// <returns>json response</returns>
        public string getUserTransactions()
        {
            var baseUrl = "https://www.bitstamp.net/api/user_transactions/";
            var client = new RestClient(baseUrl);
            RestRequest request = account.authenticator.authenticate(new RestRequest(Method.POST));
            var response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
                return response.Content;
            return "Error";
        }

        /// <summary>
        /// checks if eth has been transfered
        /// </summary>
        /// <param name="date">date when transaction was created</param>
        /// <param name="amount">eth amount that was supposed to be sent</param>
        /// <returns>true if eth has been transfered, false otherwise</returns>
        public bool checkIfTransfered(DateTime date, double amount)
        {
            string transactions = getUserTransactions();
            var data = transactions.Deserialize<TransactionInfo>();
            foreach(var item in data)
            {
                if(item.type == 0 && (date - (item.datetime.AddHours(2))).Days == 0 && amount == double.Parse(item.btc, CultureInfo.GetCultureInfo("en-US")))
                {
                    BotCore.log.Info("Received " + amount + "Eth.");
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// gets eth address
        /// </summary>
        /// <returns>eth address</returns>
        public string getEthAddress()
        {
            return account.getEthAddress();
        }

        /// <summary>
        /// gets btc address
        /// </summary>
        /// <returns>btc address</returns>
        public string getBtcAddress()
        {
            return account.getBtcAddress();
        }

        //public void getBtcDepositAddress()
        //{
        //    var baseUrl = "https://www.bitstamp.net/api/bitcoin_deposit_address/";
        //    var client = new RestClient(baseUrl);
        //    RestRequest request = account.authenticator.authenticate(new RestRequest(Method.POST));
        //    var response = client.Execute(request);
        //    if (response.StatusCode == HttpStatusCode.OK)
        //    {
        //        Bot.BotCore.log.Info("Btc deposit address:\n" + response.Content);
        //    }
        //    else
        //        Bot.BotCore.log.Error("Error while checking btc deposit address");
        //}

        //public void getEthDepositAddress()
        //{
        //    var baseUrl = "https://www.bitstamp.net/api/v2/eth_address/";
        //    var client = new RestClient(baseUrl);
        //    RestRequest request = account.authenticator.authenticate(new RestRequest(Method.POST));
        //    var response = client.Execute(request);
        //    if (response.StatusCode == HttpStatusCode.OK)
        //    {
        //        Bot.BotCore.log.Info("Eth deposit address:\n" + response.Content);
        //    }
        //    else
        //        Bot.BotCore.log.Error("Error while checking eth deposit address");
        //}


    }
}
