using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.ViewModels
{
    public static class GlobalConstant
    {
        public static string merchant_id = ConfigurationManager.AppSettings["Merchant_ID"];
        public static string password = ConfigurationManager.AppSettings["Password"];
        public static string Certificate_url = ConfigurationManager.AppSettings["TRAVEL_CERT_URL"];
    }
}
