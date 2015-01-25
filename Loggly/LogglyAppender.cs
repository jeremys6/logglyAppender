using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;

namespace Loggly
{
    public class LogglyAppender : AppenderSkeleton
    {
        #region consts
        private const char Splitter = '|';
        private const string PropertyDefinition = "property{";
        private const string TagConfigurationName = "CastleLog_Component";
        #endregion

        #region Public Instance Properties
        public string Key { get; set; }
        public string Url { get; set; }
        public string Tag { get; set; }
        #endregion

        #region Override implementation of AppenderSkeleton

        override protected void Append(LoggingEvent loggingEvent)
        {
            string enventData = this.RenderLoggingEvent(loggingEvent);
            string layoutPattern = ((PatternLayout)(Layout)).ConversionPattern;

            AppendLoggly(enventData, layoutPattern);
        }

        #endregion Override implementation of AppenderSkeleton

        #region private methods

        private void AppendLoggly(string enventData, string layoutPattern)
        {
            //build the Json Data
            string enventDetails = BuildEventData(enventData, layoutPattern);

            //Send the data
            Task.Factory.StartNew(() => SendLogglyData(enventDetails));
        }

        /// <summary>
        /// Send the data to loggly
        /// </summary>
        /// <param name="tag">The tag for the log</param>
        /// <param name="enventDetails">the json data</param>
        private void SendLogglyData(string enventDetails)
        {
            //Create the Request
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}inputs/{1}/tag/{2}", Url, Key, Tag));
            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "POST";
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(enventDetails);
                streamWriter.Flush();
                streamWriter.Close();
                httpWebRequest.GetResponseAsync(); //Don't care of the response...
            }
        }

        /// <summary>
        /// Build the json string we need to log
        /// </summary>
        /// <param name="enventData">the data logged by the application /user</param>
        /// <param name="layout">the layout defined</param>
        /// <returns>a Json string ready to be sent</returns>
        private string BuildEventData(string enventData, string layout)
        {
            //Get the data logged
            List<string> dataParsed = enventData.Split(Splitter).ToList();

            //Get the layout defined by log4net
            List<string> layoutProperties = layout.Split(Splitter).ToList();

            string parsedData = ParseEventData(layoutProperties, dataParsed);
            return parsedData;
        }

        /// <summary>
        /// Build the Json data for loggly based in the layout properties and the logged data
        /// </summary>
        /// <param name="layoutProperties">all the properties we need to pass to loggely</param>
        /// <param name="dataParsed">the actual data we want to log</param>
        /// <returns>Json string</returns>
        private static string ParseEventData(List<string> layoutProperties, List<string> dataParsed)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            for (int i = 0; i < layoutProperties.Count; i++)
            {
                string properyName = layoutProperties[i].Substring(1); //remove the '%' that start the propertyName

                if (properyName.ToLower().StartsWith(PropertyDefinition)) //in case this is a custom property
                    properyName = properyName.ToLower().Replace(PropertyDefinition, string.Empty).Replace("}", "");

                if (!string.IsNullOrEmpty(dataParsed[i]))
                {
                    if (i > 0) //only the first parameter doesn't need the ',' separator (json)
                        sb.AppendFormat(",");

                    sb.AppendFormat("'{0}':'{1}'", properyName, dataParsed[i]);
                }
            }
            sb.Append("}");
            return sb.ToString();
        }

        #endregion
    }
}
