using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace UpSellingAndCrossSelling.Config
{
    public static class Apiconfig
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Read configuration props from Config folder filename  API_SETTINGS.json
        /// </summary>
        public static List<JsonConfigSettings> _configSettings
        {
            get
            {
                try
                {
                    _log.Info("about to read from API_SETTINGS.json");
                    var configSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<List<JsonConfigSettings>>(System.IO.File.ReadAllText
                    (HttpContext.Current.Server.MapPath("~/Config/API_SETTINGS.json"))).Where(x => x.IsActive == true).ToList();
                    _log.Info("Read was successful from  API_SETTINGS.json");
                    return configSettings;
                }
                catch (Exception ex)
                {
                    _log.Info("Reading file failed from API_SETTINGS.json");
                    _log.Error(ex.Message);
                    _log.Error(ex.StackTrace);
                    _log.Error(ex.InnerException);
                    return null;
                }
            }
        }

        /// <summary>
        /// Email template
        /// </summary>
        public static string _emailTemplate
        {
            get
            {
                try
                {
                    _log.Info("about to read email template EMAIL_TEMPLATE.html");
                    var emailTemplate = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Config/EMAIL_TEMPLATE.html"));
                    _log.Info("Read was successful from  EMAIL_TEMPLATE.html");
                    return emailTemplate;
                }
                catch (Exception ex)
                {
                    _log.Info("Reading file failed from EMAIL_TEMPLATE.html");
                    _log.Error(ex.Message);
                    _log.Error(ex.StackTrace);
                    _log.Error(ex.InnerException);
                    return null;
                }
            }
        }
    }
}
