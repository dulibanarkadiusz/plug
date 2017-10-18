using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;

namespace PlugAPI.Helpers
{
    public class CommunicationHelper
    {
        private const int _Port = 10000;
        private const string _PageName = "smartplug.cgi";

        public XDocument SendMessage(XDocument xmlMessageToSend, IPAddress ipAddress, string username, string password)
        {
            XDocument xmlResponse = null;
            string targetUri = string.Format("http://{0}:{1}/{2}", ipAddress.ToString(), _Port, _PageName);
            System.Net.ServicePointManager.Expect100Continue = false;
            byte[] xmlMessageAsByteArray = System.Text.Encoding.UTF8.GetBytes(xmlMessageToSend.ToString());

            WebRequest req = WebRequest.Create(targetUri);
            //((HttpWebRequest)req).UserAgent = ".NET Framework Example Client";

            req.Method = "POST";
            req.Credentials = new NetworkCredential(username, password);
            req.ContentLength = xmlMessageAsByteArray.Length;
            //req.ContentType = "application/x-www-form-urlencoded";
            req.ContentType = "text/xml";

            Stream requestStream = req.GetRequestStream();
            requestStream.Write(xmlMessageAsByteArray, 0, xmlMessageAsByteArray.Length);
            requestStream.Close();

            using (WebResponse response = req.GetResponse())
            {
                if (((HttpWebResponse)response).StatusDescription.ToUpper() != "OK")
                {
                    throw new Exception(string.Format("SmartPlug at: {0} returned: {1} instead of \"OK\"!", ipAddress, ((HttpWebResponse)response).StatusDescription));
                }
                using (StreamReader responseReader = new StreamReader(response.GetResponseStream()))
                {
                    xmlResponse = XDocument.Parse(responseReader.ReadToEnd());
                }
            }
            return xmlResponse;
        }
    }
}