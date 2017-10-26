using Newtonsoft.Json;
using PlugAPI.Helpers;
using PlugAPI.Models;
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
    public class ScheduleController : ApiController
    {
        CommunicationHelper cm = new CommunicationHelper();

        // GET api/State
        [ResponseType(typeof(IEnumerable<Schedule>))]
        public HttpResponseMessage Get()
        {
            XDocument msgToSend = XDocument.Parse(@"<?xml version=""1.0"" encoding=""UTF8""?>
                                                   <SMARTPLUG id=""edimax"">
                                                      <CMD id=""get"">
                                                         <SCHEDULE/>
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

            List<string> decodedSchedule = new List<string>();
            decodedSchedule.Add((string)msgReceived.Descendants("Device.System.Power.Schedule.0.List").First());
            decodedSchedule.Add((string)msgReceived.Descendants("Device.System.Power.Schedule.1.List").First());
            decodedSchedule.Add((string)msgReceived.Descendants("Device.System.Power.Schedule.2.List").First());
            decodedSchedule.Add((string)msgReceived.Descendants("Device.System.Power.Schedule.3.List").First());
            decodedSchedule.Add((string)msgReceived.Descendants("Device.System.Power.Schedule.4.List").First());
            decodedSchedule.Add((string)msgReceived.Descendants("Device.System.Power.Schedule.5.List").First());
            decodedSchedule.Add((string)msgReceived.Descendants("Device.System.Power.Schedule.6.List").First());

            List<Schedule> scheduleList = new List<Schedule>();
            for (int i = 0; i < decodedSchedule.Count; i++)
            {
                string[] entries = decodedSchedule[i].Split('-');
                foreach (string entry in entries)
                {
                    if (!String.IsNullOrEmpty(entry))
                        scheduleList.Add(DecodeHelper.DecodeString(i, entry));
                }
            }

            var json = JsonConvert.SerializeObject(scheduleList, Newtonsoft.Json.Formatting.None);
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return response;
        }

        [ResponseType(typeof(IEnumerable<Schedule>))]
        public HttpResponseMessage Post(string jsonObject)
        {
            XDocument msgToSend = XDocument.Parse(@"<?xml version=""1.0"" encoding=""UTF8""?>
                                                   <SMARTPLUG id=""edimax"">
                                                      <CMD id=""setup"">
                                                         <SCHEDULE/>
                                                      </CMD>
                                                   </SMARTPLUG>");

            var scheduleList = JsonConvert.DeserializeObject<List<Schedule>>(jsonObject);

            XDocument msgReceived;

            scheduleList.Where(x => x.DayOfWeek == 0).ToList();

            msgToSend.Descendants("Device.System.Power.Schedule.0.List").First().Value = "";
            msgToSend.Descendants("Device.System.Power.Schedule.1.List").First().Value = "";
            msgToSend.Descendants("Device.System.Power.Schedule.2.List").First().Value = "";
            msgToSend.Descendants("Device.System.Power.Schedule.3.List").First().Value = "";
            msgToSend.Descendants("Device.System.Power.Schedule.4.List").First().Value = "";
            msgToSend.Descendants("Device.System.Power.Schedule.5.List").First().Value = "";
            msgToSend.Descendants("Device.System.Power.Schedule.6.List").First().Value = "";

            /*foreach (XElement xElement in scheduleNode.Elements())
           {
               if (xElement.Name.ToString().Contains("Device.System.Power.Schedule."))
               {
                   if (xElement.Name.ToString().Contains(".List"))
                   {
                       xElement.Value = PreparePowerScheduleList(entriesToSchedule);
                   }
                   else
                   {
                       xElement.Value = PreparePowerSchedule(entriesToSchedule);
                   }
               }
           }
           */


            return Get();
        }
    }
}
