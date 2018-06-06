using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeBot.Web
{
    public static class WebUtils
    {
        public static String GetJSONAtribute(string JSONString, string attribute)
        {
            try
            {
                JObject jObject = JObject.Parse(JSONString);
                string value = (string)jObject.SelectToken(attribute);
                return value;
            }
            catch (Exception e)
            {
                return "";
            }
        }


        public static Dictionary<string, string> GetJSONAtribute(string JSONString, string[] attributes)
        {
            try
            {
                JObject jObject = JObject.Parse(JSONString);
                Dictionary<string, string> attributesValues = new Dictionary<string, string>();
                foreach (string singleAttribute in attributes)
                {
                    var value = (string)jObject.SelectToken(singleAttribute);
                    if (value == null)
                    {
                        continue;
                    }
                    else
                    {
                        attributesValues.Add(singleAttribute, value);
                    }
                }
                return attributesValues;
            }
            catch (Exception e)
            {
                return new Dictionary<string, string>();
            }
        }

        public static List<T> Deserialize<T>(this string SerializedJSONString)
        {
            var convertedObject = JsonConvert.DeserializeObject<List<T>>(SerializedJSONString);
            return convertedObject;
        }
    }
}
