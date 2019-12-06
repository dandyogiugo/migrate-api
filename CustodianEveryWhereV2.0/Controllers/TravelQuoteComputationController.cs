using DataStore.Models;
using DataStore.repository;
using DataStore.Utilities;
using DataStore.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace CustodianEveryWhereV2._0.Controllers
{
    public class TravelQuoteComputationController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        public TravelQuoteComputationController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
        }

        [HttpPost]
        public async Task<notification_response> GetQuote(Quote quote)
        {

            // add authentication
            try
            {
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        status = 300,
                        message = "some required parameters missing from request"
                    };
                }

                if (quote.DateOfBirth.Count() < 1)
                {
                    return new notification_response
                    {
                        status = 302,
                        message = "date of birth is required"
                    };
                }
                List<plans> plans = new List<plans>();
                List<string> benefits = null;
                List<int> _age = new List<int>();
                foreach (var item in quote.DateOfBirth)
                {
                    int age = DateTime.Now.Year - item.Year;
                    if (age > 76)
                    {
                        return new notification_response
                        {
                            status = 302,
                            message = "Traveller age is greater than 76 years"
                        };
                    }
                    _age.Add(age);
                }

                double exchnageRate = Convert.ToDouble(ConfigurationManager.AppSettings["TRAVEL_EXCHANGE_RATE"]);
                int numbersOfDays = (int)quote.ReturnDate.Subtract(quote.DepartureDate).TotalDays;
                var rateList = await util.GetTravelRate(numbersOfDays, quote.Region);
                var myPackage = util.GetPackageDetails(quote.Region, out benefits);
                //basepremium = 1.32x where x is the base premium

                foreach (var rate in rateList)
                {
                    List<double> premium = null;
                    double computedPremium = 0;
                    bool proceed = false;
                    if (quote.Region == TravelCategory.WorldWide2 && rate.excluded_rate.HasValue)
                    {
                        log.Info($"Premium base: {rate.included_rate}");
                        premium = await util.GetDiscountByAge(_age, rate.included_rate);
                        proceed = true;
                    }
                    else if (quote.Region != TravelCategory.WorldWide2)
                    {
                        log.Info($"Premium base: {rate.excluded_rate ?? rate.included_rate}");
                        premium = await util.GetDiscountByAge(_age, rate.excluded_rate ?? rate.included_rate);
                        proceed = true;
                    }
                    if (proceed)
                    {
                        log.Info($"Premium: {premium}");
                        foreach (var prem in premium)
                        {
                            computedPremium += (1.32 * prem) * exchnageRate;
                        }
                        log.Info($"Rate used: => {Newtonsoft.Json.JsonConvert.SerializeObject(rate)}");
                        var section = myPackage.FirstOrDefault(x => x.type == rate.type);
                        var plan = new plans
                        {
                            premium = computedPremium,
                            exchangeRate = exchnageRate,
                            travellers = _age.Count(),
                            package = section,
                           
                        };
                        plans.Add(plan);
                    }
                }


                if (plans.Count() == 0)
                {
                    return new notification_response
                    {
                        status = 206,
                        message = "Premium computation was skiped. Operation aborted",
                    };
                }

                return new notification_response
                {
                    status = 200,
                    message = "Premium computed successfully",

                    data = new
                    {
                        details = plans,
                        benefits = benefits
                    }
                };
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new notification_response
                {
                    status = 404,
                    message = "oops!, something happend while searching for details"
                };
            }
        }

        #region
        //[HttpGet]
        //public async Task<notification_response> GetTravelRegion()
        //{
        //    try
        //    {
        //        List<dynamic> regions = new List<dynamic>();
        //        regions.Add(new
        //        {
        //            name = "World Wide",
        //            Id = 1
        //        });
        //        regions.Add(new
        //        {
        //            name = "Schengen",
        //            Id = 2
        //        });
        //        regions.Add(new
        //        {
        //            name = "East & Asia",
        //            Id = 3
        //        });
        //        regions.Add(new
        //        {
        //            name = "Africa",
        //            Id = 4
        //        });

        //        return new notification_response
        //        {
        //            status = 200,
        //            message = "successful",
        //            data = regions
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error(ex.Message);
        //        log.Error(ex.StackTrace);
        //        log.Error(ex.InnerException);
        //        return new notification_response
        //        {
        //            status = 404,
        //            message = "oops!, something happend while searching for region"
        //        };
        //    }
        //}
        #endregion

        [HttpGet]
        public async Task<notification_response> GetCountry()
        {
            try
            {
                var country_file = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/TravelCategoryJSON/Country.json"));
                var country_list = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(country_file);
                return new notification_response
                {
                    status = 200,
                    message = "successful",
                    data = country_list
                };
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new notification_response
                {
                    status = 404,
                    message = "oops!, something happend while searching for region"
                };
            }
        }
    }
}
