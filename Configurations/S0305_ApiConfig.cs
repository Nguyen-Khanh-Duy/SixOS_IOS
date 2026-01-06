using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SixOSDatKhamAppMobile.Configurations
{
    public static class S0305_ApiConfig
    {
        public const string BaseUrl = "https://sixos-dkkhamapi.onrender.com/api";
        //public const string BaseUrl = "http://192.168.1.15:5069/api";
        public const int DefaultTimeoutSeconds = 180;

        public static string GetFullUrl(string endpoint)
        {
            return $"{BaseUrl}{endpoint}";
        }
    }
}
