using RestSharp;
using System;
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
                Console.WriteLine("Error respoms");
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


    }
}
