using DataStore.Models;
using DataStore.repository;
using DataStore.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CustodianEveryWhereV2._0.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class InterstateController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        public InterstateController()
        {
            _apiconfig = new store<ApiConfiguration>();
        }

        //[HttpPost]
        //public async Task<res> Onboarding(InterState interState)
        //{
        //    try
        //    {

        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error(ex.Message);
        //        log.Error(ex.StackTrace);
        //        log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
        //        return new res { message = "System error, Try Again", status = 404 };
        //    }
        //}

        [HttpGet]
        public async Task<res> GetFormSelections()
        {
            try
            {
                var title = new List<string>() { "Alh", "Chief", "Dr", "Engr", "Miss", "Mr", "Prof" };
                var gender = new List<string>() { "M", "F" };
                var idtypes = new List<dynamic>()
                {
                    new
                    {
                        desc = "Driver’s licence",
                        value = "DL"
                    },
                    new
                    {
                        desc = "International passport",
                        value = "IP"
                    },
                    new
                    {
                        desc = "National ID card",
                        value = "NID"
                    },
                    new
                    {
                        desc = "Voter's card",
                        value = "VC"
                    }
                };
                var relationship = new List<dynamic>()
                {
                    new
                    {
                        desc = "Brother",
                        value = "BRO"
                    },
                    new
                    {
                        desc = "Daughter",
                        value = "DAU"
                    },
                    new
                    {
                        desc = "Father",
                        value = "FA"
                    },
                     new
                    {
                        desc = "Husband",
                        value = "HUS"
                    },
                     new
                    {
                        desc = "Mother",
                        value = "MO"
                    },
                     new
                    {
                        desc = "Sister",
                        value = "SIS"
                    },
                     new
                    {
                        desc = "Son",
                        value = "SON"
                    },
                     new
                    {
                        desc = "Wife",
                        value = "WIF"
                    },
                     new
                    {
                        desc = "Other",
                        value = "OTH"
                    },
                };
                var country_file = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/TravelCategoryJSON/Country.json"));
                var country_list = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(country_file);
                var country_phone_code = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/TravelCategoryJSON/countryPhoneCode.json"));
                var country_phone_code_list = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(country_phone_code);
                return new res
                {
                    status = 200,
                    message = "data fetch successully",
                    data = new
                    {
                        title,
                        gender,
                        idtypes,
                        relationship,
                        country_list,
                        country_phone_code_list
                    }
                };
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = 404 };
            }
        }
    }
}
