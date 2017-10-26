using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlugAPI.Models
{
    public class Schedule
    {
        public int DayOfWeek { get; set; }
        public string HourStart { get; set; }
        public string HourEnd { get; set; }
        public bool IsEnabled { get; set; }
    }
}