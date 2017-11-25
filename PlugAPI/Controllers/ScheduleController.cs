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
        public HttpResponseMessage Post(HttpRequestMessage request)
        {
            var content = request.Content;
            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(content.ReadAsStringAsync().Result);

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

            var overridden = false;
            var scheduleList = JsonConvert.DeserializeObject<List<Schedule>>(data["schedule"]);
            for (int i = 0; i <= 6; i++)
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

            UpdateSchedule(ref msgToSend);



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


        private void UpdateSchedule(ref XDocument msg)
        {
            for (int i=0; i<=6; i++) // for each day of week
            {
                var value = msg.Descendants("Device.System.Power.Schedule." + i + ".List").First().Value;
                if (value.Length > 0)
                {
                    string[] encodedHours = value.Split('-');
                    var scheduleList = new List<Schedule>();
                    foreach(var h in encodedHours)
                    {
                        if (!String.IsNullOrEmpty(h))
                            scheduleList.Add(DecodeHelper.DecodeString(i, h));
                    }

                    msg.Descendants("Device.System.Power.Schedule." + i).First().Value = PreparePowerSchedule(scheduleList);
                }
            }
        }

        private string PreparePowerSchedule(IEnumerable<Schedule> entriesToSchedule)
        {
            int[] minuteSchedule = InitMinuteSchedule(); // an array containing one element for each minute of a day


            foreach (Schedule entry in entriesToSchedule)
            {
                // filling in scheduled minutes for each scheduled entry
                FillMinuteSchedule(entry, ref minuteSchedule);
            }

            //Transfer it to a 360 elements based hex schedule

            char[] hexSchedule = InitHexSchedule();

            int schedulePosition = 0;
            for (int quadrupletStart = 0; quadrupletStart < 1440; quadrupletStart += 4)
            {
                hexSchedule[schedulePosition] = QuadrupletToHex(minuteSchedule, quadrupletStart);
                schedulePosition++;
            }

            return new string(hexSchedule); // string with length 360, each position contains a value between '0' and 'F'
        }

        private static int[] InitMinuteSchedule()
        {
            const int MinutesOfADay = 1440;
            int[] scheduleTemplate = new int[MinutesOfADay];
            for (int i = 0; i < MinutesOfADay; i++)
            {
                scheduleTemplate[i] = 0;
            }
            return scheduleTemplate;
        }

        private static char[] InitHexSchedule()
        {
            char[] scheduleTemplate = new char[360];
            for (int i = 0; i < 360; i++)
            {
                scheduleTemplate[i] = '0';
            }
            return scheduleTemplate;
        }

        static void FillMinuteSchedule(Schedule entry, ref int[] minuteSchedule)
        {
            var hourStart = entry.HourStart.Split(':');
            var hourEnd = entry.HourEnd.Split(':');
            FillMinuteSchedule(int.Parse(hourStart[0]), int.Parse(hourStart[1]),
                              int.Parse(hourEnd[0]), int.Parse(hourEnd[1]), ref minuteSchedule);

        }

        static void FillMinuteSchedule(int fromHour, int fromMinute, int toHour, int toMinute, ref int[] minuteSchedule)
        {
            int begin = fromHour * 60 + fromMinute;
            int end = toHour * 60 + toMinute;
            for (int pos = begin; pos < end; pos++)
            {
                minuteSchedule[pos] = 1;
            }
        }

        private static char QuadrupletToHex(int[] minuteSchedule, int quadrupletBaseAdress)
        {
            int quadrupletSum = 0;
            for (int position = quadrupletBaseAdress; position < (quadrupletBaseAdress + 4); position++)
            {
                quadrupletSum = quadrupletSum * 2 + minuteSchedule[position];
            }
            return quadrupletSum.ToString("X")[0];
        }


        /// <summary>
        /// Testowa metoda 
        /// </summary>
        /// <param name="jsonObject"></param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<Schedule>))]
        [HttpDelete]
        public HttpResponseMessage Delete(HttpRequestMessage request)
        {
            var content = request.Content;
            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(content.ReadAsStringAsync().Result);

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


            var scheduleList = JsonConvert.DeserializeObject<List<Schedule>>(data["schedule"]);
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
