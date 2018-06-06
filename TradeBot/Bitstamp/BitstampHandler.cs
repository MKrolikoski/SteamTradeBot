using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using TradeBot.Web;

namespace TradeBot.Bitstamp
{
    class BitstampHandler
    {
        private BitstampAccount account;

        public BitstampHandler()
        {
            account = new BitstampAccount();
        }

        public void checkBalance()
        {
            var baseUrl = "https://www.bitstamp.net/api/balance/";
            var client = new RestClient(baseUrl);
            RestRequest request = account.authenticator.authenticate(new RestRequest(Method.POST));          
            var response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine(response.Content);
            }
            else
                Console.WriteLine("Error response");
        }

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
                //Console.WriteLine(response.Content);
                return true;
            }
            else
            {
                //Console.WriteLine(response.Content);
                return false;
            }
        }

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

        public bool checkIfTransfered(DateTime date, double amount)
        {
            string transactions = getUserTransactions();
            var data = transactions.Deserialize<TransactionInfo>();
            foreach(var item in data)
            {
                if(item.type == 0 && (date - (item.datetime.AddHours(2))).Days == 0 && amount == double.Parse(item.btc, CultureInfo.GetCultureInfo("en-US")))
                {
                    return true;
                }
            }
            return false;
        }



        public string getEthAddress()
        {
            return account.getEthAddress();
        }


    }
}
