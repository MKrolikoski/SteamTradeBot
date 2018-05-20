using RestSharp;
using System;
using System.Globalization;
using System.Net;
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
                Console.WriteLine(response.Content);
            }
            else
                Console.WriteLine("Error response");
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


    }
}
