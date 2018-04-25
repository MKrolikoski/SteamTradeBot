using RestSharp;
using System;
using System.Net;

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


    }
}
