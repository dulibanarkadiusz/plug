using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace PlugAPI.Helpers
{
    public static class Config
    {
        public static IPAddress IPadress = IPAddress.Parse("192.168.1.104");
        public static string Username = "admin";
        public static string Password = "1234";
    }
}