using RestSharp;
using System;
using System.Security.Cryptography;
using System.Text;

namespace TradeBot.Bitstamp
{
    /// <summary>
    /// Class that is used to authenticate Bitstamp user using their API.
    /// </summary>
    class RequestAuthenticator
    {
        private BitstampConfig config;
        private Int64 nonce { get; set; }
        private string signature { get; set; }

        /// <summary>
        /// Default constructor. As parameter it get a BistampConfig with information about Bitstamp account.
        /// </summary>
        /// <param name="config">instance of BitstampConfig with information about user account</param>
        public RequestAuthenticator(BitstampConfig config)
        {
            this.config = config;
            nonce = generateNonce();
        }

        /// <summary>
        /// Method generate nonce that is used to authenticate user.Nonce is based on current time.
        /// </summary>
        /// <returns>nonce value</returns>
        private Int64 generateNonce()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }

        /// <summary>
        /// Method generate signature that is send to Bitstamp API.
        /// </summary>
        /// <returns>string with generated signature. It contains api key, api secret key, user id and nonce.</returns>
        private string generateSignature()
        {
            string message = string.Format("{0}{1}{2}", nonce.ToString(), config.customer_id, config.api_key);        
            byte[] key = Encoding.ASCII.GetBytes(config.api_secret);
            HMACSHA256 hmac = new HMACSHA256(key);
            byte[] hashValue = hmac.ComputeHash(Encoding.ASCII.GetBytes(message));
            return BitConverter.ToString(hashValue).Replace("-", "").ToUpper();
        }

        /// <summary>
        /// Method that allows to authenticate user Bistamp account.
        /// </summary>
        /// <param name="request">JSON REST request with information that are necessary to authenticate account.</param>
        /// <returns>JSON with server response</returns>
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
