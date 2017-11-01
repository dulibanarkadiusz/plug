using Newtonsoft.Json;
using PlugAPI.Cors;
using PlugAPI.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml.Linq;

namespace PlugAPI.Controllers
{
    [AllowCross]
    public class DeviceConfigController : ApiController
    {
        CommunicationHelper cm = new CommunicationHelper();

        // GET api/State
        [ResponseType(typeof(Models.DeviceConfig))]
        public HttpResponseMessage Get()
        {
            XDocument msgToSend = XDocument.Parse(@"<?xml version=""1.0"" encoding=""UTF8""?>
                                     <SMARTPLUG id=""edimax"">
                                        <CMD id=""get"">
                                           <SYSTEM_INFO/>
                                        </CMD>
                                     </SMARTPLUG>");

            XDocument msgReceived;
            try
            {
                msgReceived = cm.SendMessage(msgToSend, Config.IPadress, Config.Username, Config.Password);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }

            List<Models.DeviceConfig> list = new List<Models.DeviceConfig>();
            list.Add(new Models.DeviceConfig()
            {
                Name = (string)msgReceived.Descendants("Device.System.Name").First(),
                Model = (string)msgReceived.Descendants("Run.Model").First(),
                MAC = (string)msgReceived.Descendants("Run.LAN.Client.MAC.Address").First(),
                IsMailingOn = ((string)msgReceived.Descendants("Device.System.Power.Schedule.Support").First() == "1"),
                IsScheduleOn = ((string)msgReceived.Descendants("Device.System.SMTP.Support").First() == "1")
            });

            var json = JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.None);
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return response;
        }

        [HttpPost]
        public HttpResponseMessage ChangeName(string name)
        {
            XDocument msgToSend = XDocument.Parse(@"<?xml version=""1.0"" encoding=""UTF8""?>
                                                         <SMARTPLUG id=""edimax"">
	                                                     <CMD id=""setup"">
                                                             <SYSTEM_INFO>
                                                                <Device.System.Name>"+name+@"</Device.System.Name>
                                                             </SYSTEM_INFO>
	                                                     </CMD>
                                                       </SMARTPLUG>");

            try
            {
                cm.SendMessage(msgToSend, Config.IPadress, Config.Username, Config.Password);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }

            return Get();
        }
    }
}
