using Newtonsoft.Json;
using PlugAPI.Cors;
using PlugAPI.Helpers;
using System;
using System.Collections.Generic;
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
    public class StateController : ApiController
    {
        CommunicationHelper cm = new CommunicationHelper();

        // GET api/State
        [ResponseType(typeof(Models.State))]
        public HttpResponseMessage Get()
        {
            XDocument msgToSend = XDocument.Parse(@"<?xml version=""1.0"" encoding=""UTF8""?>
                                     <SMARTPLUG id=""edimax"">
	                                    <CMD id=""get"">
		                                   <Device.System.Power.State/>
	                                    </CMD>
                                     </SMARTPLUG>");

            XDocument msgReceived;
            try
            {
                msgReceived = cm.SendMessage(msgToSend, Config.IPadress, Config.Username, Config.Password);
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.GatewayTimeout, ex);
            }

            var value = (string)msgReceived.Root.Value;
            List<Models.State> list = new List<Models.State>();
            list.Add(new Models.State() { IsOn = (value == "ON") });

            var json = JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.None);
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return response;
        }

        // POST api/State 
        [ResponseType(typeof(Models.State))]
        public HttpResponseMessage Post(bool isOn)
        {
            var state = (isOn) ? "ON" : "OFF";
            XDocument msgToSend = XDocument.Parse(@"<?xml version=""1.0"" encoding=""utf-8""?>
                                             <SMARTPLUG id=""edimax"">
                                                <CMD id=""setup"">
                                                   <Device.System.Power.State>"+state+@"</Device.System.Power.State>
                                                </CMD>
                                             </SMARTPLUG>");

            try
            {
                cm.SendMessage(msgToSend, Config.IPadress, Config.Username, Config.Password);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.GatewayTimeout, ex);
            }

            return Get();
        }
    }
}
