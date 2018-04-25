using RestSharp;
using System;
using System.Security.Cryptography;
using System.Text;

namespace TradeBot.Bitstamp
{
    class RequestAuthenticator
    {
        private BitstampConfig config;
        private Int64 nonce { get; set; }
        private string signature { get; set; }

        public RequestAuthenticator(BitstampConfig config)
        {
            this.config = config;
            nonce = generateNonce();
        }

        private Int64 generateNonce()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }


        private string generateSignature()
        {
            string message = string.Format("{0}{1}{2}", nonce.ToString(), config.customer_id, config.api_key);        
            byte[] key = Encoding.ASCII.GetBytes(config.api_secret);
            HMACSHA256 hmac = new HMACSHA256(key);
            byte[] hashValue = hmac.ComputeHash(Encoding.ASCII.GetBytes(message));
            return BitConverter.ToString(hashValue).Replace("-", "").ToUpper();
        }
        public RestRequest authenticate(RestRequest request)
        {
            request.AddParameter("key", config.api_key);
            request.AddParameter("signature", generateSignature());   
            request.AddParameter("nonce", nonce);
            nonce++;
            return request;
        }
    }
}
