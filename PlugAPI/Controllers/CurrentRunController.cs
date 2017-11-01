using Newtonsoft.Json;
using PlugAPI.Cors;
using PlugAPI.Helpers;
using System;
using System.Collections.Generic;
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
    public class CurrentRunController : ApiController
    {
        CommunicationHelper cm = new CommunicationHelper();

        // GET api/State
        [ResponseType(typeof(Models.CurrentRun))]
        public HttpResponseMessage Get()
        {
            XDocument msgToSend = XDocument.Parse(@"<?xml version=""1.0"" encoding=""UTF8""?>
                                     <SMARTPLUG id = ""edimax"">
                                        <CMD id = ""get"">
                                           <NOW_POWER/>
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

            List<Models.CurrentRun> list = new List<Models.CurrentRun>();
            list.Add(new Models.CurrentRun()
            {
                LastToogleTime = DateTime.ParseExact((string)msgReceived.Descendants("Device.System.Power.LastToggleTime").First(),
                    "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture),
                CurrentValue = Double.Parse((string)msgReceived.Descendants("Device.System.Power.NowCurrent").First(), CultureInfo.InvariantCulture),
                PowerValue = Double.Parse((string)msgReceived.Descendants("Device.System.Power.NowPower").First(), CultureInfo.InvariantCulture),
                EnergyValue = new Models.Energy()
                {
                    Day = Double.Parse((string)msgReceived.Descendants("Device.System.Power.NowEnergy.Day").First(), CultureInfo.InvariantCulture),
                    Week = Double.Parse((string)msgReceived.Descendants("Device.System.Power.NowEnergy.Week").First(), CultureInfo.InvariantCulture),
                    Month = Double.Parse((string)msgReceived.Descendants("Device.System.Power.NowEnergy.Month").First(), CultureInfo.InvariantCulture)
                }
            });

            var json = JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.None);
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return response;
        }
    }
}
