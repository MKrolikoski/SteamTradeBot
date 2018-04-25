using System;
using System.IO;
using System.Net;

namespace TradeBot.Web
{
    class WebClient
    {
        private HttpMethod httpMethod { get; set; }
        private string endPoint { get; set; }
        private string data { get; set; }

        public WebClient() { }
        public WebClient(HttpMethod httpMethod, string endPoint, string data)
        {
            this.httpMethod = httpMethod;
            this.endPoint = endPoint;
            this.data = data;
        }

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
