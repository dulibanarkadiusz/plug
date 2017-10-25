using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlugAPI.Models
{

    public class CurrentRun
    {
        public DateTime LastToogleTime;
        public double CurrentValue;
        public double PowerValue;
        public Energy EnergyValue;
    }
}