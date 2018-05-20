using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeBot.Web
{
    /// <summary>
    /// WebClient necessary utilities.
    /// </summary>
    public static class WebUtils
    {
        /// <summary>
        /// Method returns value of atribute from passed string.
        /// </summary>
        /// <param name="JSONString">string with JSON content</param>
        /// <param name="attribute">name of the attribute</param>
        /// <returns>string with atribute value, empty string if it does not exist.</returns>
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

        /// <summary>
        /// Method returns dictionary with pairs of attribute and attribute value.
        /// </summary>
        /// <param name="JSONString">string with JSON content</param>
        /// <param name="attributes">name of attributes that should be putted into the dictionary</param>
        /// <returns>Dictionary with attributes and their values</returns>
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
    }
}
