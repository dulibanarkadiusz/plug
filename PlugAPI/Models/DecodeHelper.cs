using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlugAPI.Models
{
    public static class DecodeHelper
    {
        private const string _Template = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static Schedule DecodeString(int dayOfWeekPos, string hash)
        {
            Hour hourStart = new Hour(_Template.IndexOf(hash[0]), _Template.IndexOf(hash[1]));
            Hour hourEnd = new Hour(_Template.IndexOf(hash[2]), _Template.IndexOf(hash[3]));

            return new Schedule() { DayOfWeek = dayOfWeekPos, HourStart = hourStart.ToString(), HourEnd = hourEnd.ToString(), IsEnabled = (hash[4]=='1') };
        }

        public static string EncodeHour(string hour)
        {
            var d = DateTime.Parse(hour);
            hour = d.ToString("HH:mm");
            var parts = hour.Split(':');

            return _Template[int.Parse(parts[0])] + "" + _Template[int.Parse(parts[1])];
        }

        public static string EncodeSchedule(List<Schedule> scheduleList)
        { 
            if (scheduleList.Count == 0)
                return string.Empty;

            var schedulestr = string.Empty;
            string startEncode = string.Empty, endEncode = string.Empty;
            string output = "";
            foreach (var s in scheduleList)
            {
                startEncode = EncodeHour(s.HourStart);
                endEncode = EncodeHour(s.HourEnd);
                output += String.Format("{0}{1}1-", startEncode, endEncode);
            }

            output = output.Substring(0, output.Length - 1);

            return output;
        }
    }
}