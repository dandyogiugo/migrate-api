﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.ViewModels
{
    public static class GlobalConstant
    {
        public static string merchant_id { get; } = ConfigurationManager.AppSettings["Merchant_ID"];
        public static string password { get; } = ConfigurationManager.AppSettings["Password"];
        public static string Certificate_url { get; } = ConfigurationManager.AppSettings["TRAVEL_CERT_URL"];
        public static string Reciept_url { get; } = ConfigurationManager.AppSettings["RecieptBaseUrl"];
        public static string base_url { get; } = ConfigurationManager.AppSettings["HALOGEN_API"];
        public static string auth_email { get; } = ConfigurationManager.AppSettings["HALOGEN_AUTH_EMAIL"];
        public static string passcode { get; } = ConfigurationManager.AppSettings["HALOGEN_PASSCODE"];
        public static string HalogenDefaultPrice { get; } = ConfigurationManager.AppSettings["HologenDefaultPrice"];
        public static string DiscountPriceHalogen { get; } = ConfigurationManager.AppSettings["DiscountPriceHalogen"];
        public static string LabelHalogen { get; } = ConfigurationManager.AppSettings["LabelHalogen"];
        public static string LoadingPrice { get; } = ConfigurationManager.AppSettings["LoadingPrice"];
        public static string GTBANK { get; } = ConfigurationManager.AppSettings["GTBANK"];
        public static bool IsDemoMode
        {
            get
            {
                string IsDemoMode = ConfigurationManager.AppSettings["IsDemoMode"];
                if (string.IsNullOrEmpty(IsDemoMode))
                {
                    return false;
                }
                else
                {
                    bool mode = Boolean.TryParse(IsDemoMode, out bool result);
                    if (!mode)
                    {
                        return false;
                    }
                    else
                    {
                        if (result)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    }

                }
            }
        }
        public static decimal HardCodedHalogenPrice
        {
            get
            {
                string HologenPrice = ConfigurationManager.AppSettings["HALOGENPRICE"];
                if (!string.IsNullOrEmpty(HologenPrice))
                {
                    var price = Convert.ToDecimal(HologenPrice);
                    return price;
                }
                else
                {
                    //if no configuration found use default
                    return 22500;
                }
            }
        }

        public static decimal DeviceComprehensiveTracker
        {
            get
            {
                const decimal default_value = 4000000;
                var getKey = ConfigurationManager.AppSettings["DEVICECOMPREHENSIVETRACKER"];
                if (!string.IsNullOrEmpty(getKey))
                {
                    var min_value = decimal.TryParse(getKey, out decimal result);
                    if (min_value)
                    {
                        return result;
                    }
                    else
                    {
                        return default_value;
                    }
                }
                else
                {
                    return default_value;
                }
            }
        }

        public static string GetWeatherAPIKey
        {
            get
            {
                string apikey = ConfigurationManager.AppSettings["WEATHER_API_KEY"];
                if (string.IsNullOrEmpty(apikey))
                    return null;
                return apikey;
            }
        }

        public static string WeatherIconBaseURL
        {
            get
            {
                string baseurl = ConfigurationManager.AppSettings["WEATHER_API_ICON_URL"];
                if (string.IsNullOrEmpty(baseurl))
                    return null;
                return baseurl;
            }
        }

        public static string WeatherBaseURL
        {
            get
            {
                string baseurl = ConfigurationManager.AppSettings["WEATHER_API_BASE_URL"];
                if (string.IsNullOrEmpty(baseurl))
                    return null;
                return baseurl;
            }
        }
    }
}
