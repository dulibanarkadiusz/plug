using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlugAPI.Models
{
    public class Hour
    {
        int HourPart { get; set; }
        int MinutesPart { get; set; }

        public Hour(int hour, int minutes)
        {
            HourPart = hour;
            MinutesPart = minutes;
        }

        public override string ToString()
        {
            string minutesString = MinutesPart + "";
            if (minutesString.Length == 1)
            {
                minutesString = "0" + minutesString;
            }

            return HourPart + ":" + minutesString;
        }
    }
}