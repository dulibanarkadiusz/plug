using Newtonsoft.Json;
using PlugAPI.Cors;
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
    [AllowCross]
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
                return Request.CreateErrorResponse(HttpStatusCode.GatewayTimeout, ex);
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
        [HttpPost]
        public HttpResponseMessage Post([FromBody] object jsonObject, bool overridden = true)
        {
            string xxx = jsonObject.ToString();
            XDocument msgToSend = XDocument.Parse(@"<?xml version=""1.0"" encoding=""UTF8""?>
                                                   <SMARTPLUG id=""edimax"">
                                                      <CMD id=""get"">
                                                         <SCHEDULE/>
                                                      </CMD>
                                                   </SMARTPLUG>");

            try
            {
                msgToSend = cm.SendMessage(msgToSend, Config.IPadress, Config.Username, Config.Password);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.GatewayTimeout, ex);
            }
            msgToSend.Descendants("CMD").First().Attribute("id").Value = "setup";

            var scheduleList = JsonConvert.DeserializeObject<List<Schedule>>(jsonObject.ToString());
            for (int i=0; i<=6; i++)
            {
                var list = scheduleList.Where(x => x.DayOfWeek == i).ToList();
                var decodedListStr = DecodeHelper.EncodeSchedule(list);
                if (overridden)
                {
                    msgToSend.Descendants("Device.System.Power.Schedule." + i + ".List").First().Value = decodedListStr;
                }
                else
                {
                    var connector = (msgToSend.Descendants("Device.System.Power.Schedule." + i + ".List").First().Value.ToString().Length > 0) ? "-" : "";
                    msgToSend.Descendants("Device.System.Power.Schedule." + i + ".List").First().Value += connector + decodedListStr;
                }
            }


            XDocument msgReceived;
            try
            {
                msgReceived = cm.SendMessage(msgToSend, Config.IPadress, Config.Username, Config.Password);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.GatewayTimeout, ex);
            }

            return Get();
        }

        /// <summary>
        /// Testowa metoda 
        /// </summary>
        /// <param name="jsonObject"></param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<Schedule>))]
        [HttpPut]
        public HttpResponseMessage Put([FromBody] object jsonObject)
        {
            XDocument msgToSend = XDocument.Parse(@"<?xml version=""1.0"" encoding=""UTF8""?>
                                                   <SMARTPLUG id=""edimax"">
                                                      <CMD id=""get"">
                                                         <SCHEDULE/>
                                                      </CMD>
                                                   </SMARTPLUG>");

            try
            {
                msgToSend = cm.SendMessage(msgToSend, Config.IPadress, Config.Username, Config.Password);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
            msgToSend.Descendants("CMD").First().Attribute("id").Value = "setup";


            var scheduleList = JsonConvert.DeserializeObject<List<Schedule>>(jsonObject.ToString());
            var currentValue = msgToSend.Descendants("Device.System.Power.Schedule." + scheduleList[0].DayOfWeek + ".List").First().Value;

            string decodedVal;
            try
            {
                decodedVal = DecodeHelper.EncodeSchedule(scheduleList);
            }
            catch
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }


            if (currentValue.Contains("-" + decodedVal))
            {
                decodedVal = "-" + decodedVal;
            }
            else if (!currentValue.Contains(decodedVal))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            msgToSend.Descendants("Device.System.Power.Schedule." + scheduleList[0].DayOfWeek + ".List").First().Value = currentValue.Replace(decodedVal, "");
            try
            {
                cm.SendMessage(msgToSend, Config.IPadress, Config.Username, Config.Password);
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.GatewayTimeout, ex);
            }
            return Get();
        }
    }
}
