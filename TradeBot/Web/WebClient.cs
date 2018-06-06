using System;
using System.IO;
using System.Net;

namespace TradeBot.Web
{
    /// <summary>
    /// Class that is used to sending requests.
    /// </summary>
    class WebClient
    {
        private HttpMethod httpMethod { get; set; }
        private string endPoint { get; set; }
        private string data { get; set; }

        /// <summary>
        /// Default class constructor
        /// </summary>
        public WebClient() { }

        /// <summary>
        /// Class constructor that set all fields in class.
        /// </summary>
        /// <param name="httpMethod">type of HTTP method (GET,POST)</param>
        /// <param name="endPoint">endpoint to send request</param>
        /// <param name="data">body of request</param>
        public WebClient(HttpMethod httpMethod, string endPoint, string data)
        {
            this.httpMethod = httpMethod;
            this.endPoint = endPoint;
            this.data = data;
        }

        /// <summary>
        /// Method returns response from endpoint.
        /// </summary>
        /// <returns>string with endpoint response</returns>
        public string getResponse()
        {
            string response = null;

            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(endPoint);
            httpRequest.Method = httpMethod.ToString();

            if (data != null)
            {
                httpRequest.ContentType = "application/json";
                httpRequest.ContentLength = data.Length;
                using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    streamWriter.Write(data);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

            }

            using (HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse())
            {
                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    string errorMessage = "Status code from " + endPoint + " not OK";
                    throw new ApplicationException(errorMessage);
                }

                using (Stream responseStream = httpResponse.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            response = reader.ReadToEnd();
                        }
                    }
                }
            }
                return response;
        }


    }
}
