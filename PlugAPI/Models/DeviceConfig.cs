using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlugAPI.Models
{
    public class DeviceConfig
    {
        public string Name { get; set; }
        public string Model { get; set; }
        public string MAC { get; set; }
        public bool IsMailingOn { get; set; }
        public bool IsScheduleOn { get; set; }
    }
}