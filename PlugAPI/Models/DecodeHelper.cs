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

        public static string EncodeHour(Hour hour)
        {
           string hourString = hour.ToString();

            return string.Empty;
        }

        public static string EncodeSchedule(List<Schedule> scheduleList)
        {
            var schedulestr = string.Empty;
            foreach(var s in scheduleList)
            {
                string startEncode = s.HourStart;
            }

            return string.Empty;
        }
    }
}