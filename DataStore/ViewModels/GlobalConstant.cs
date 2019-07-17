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
        public static string Reciept_url = ConfigurationManager.AppSettings["RecieptBaseUrl"];
        public static string base_url = ConfigurationManager.AppSettings["HALOGEN_API"];
        public static string auth_email = ConfigurationManager.AppSettings["HALOGEN_AUTH_EMAIL"];
        public static string passcode = ConfigurationManager.AppSettings["HALOGEN_PASSCODE"];
        public static string HalogenDefaultPrice = ConfigurationManager.AppSettings["HologenDefaultPrice"];
        public static string DiscountPriceHalogen = ConfigurationManager.AppSettings["DiscountPriceHalogen"];
        public static string LabelHalogen = ConfigurationManager.AppSettings["LabelHalogen"];
        public static string LoadingPrice = ConfigurationManager.AppSettings["LoadingPrice"];
    }
}
