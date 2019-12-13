using DapperLayer.Dapper.Core;
using DataStore.Models;
using DataStore.repository;
using DataStore.Utilities;
using DataStore.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
    public class TravelQuoteComputationController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private Core<dynamic> dapper_core = null;
        public TravelQuoteComputationController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            dapper_core = new Core<dynamic>();
        }
        /// <summary>
        /// Get Travel quote
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
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
                        int count = 0;
                        dynamic premiumForIndividual;
                        List<dynamic> premiumBreakDown = new List<dynamic>();
                        foreach (var prem in premium)
                        {
                            computedPremium += (1.32 * prem) * exchnageRate;
                            premiumForIndividual = new
                            {
                                Id = count,
                                premium = (1.32 * prem) * exchnageRate,
                                dateOfBirth = quote.DateOfBirth[count]
                            };
                            count++;
                            premiumBreakDown.Add(premiumForIndividual);
                        }

                        log.Info($"Rate used: => {Newtonsoft.Json.JsonConvert.SerializeObject(rate)}");
                        var section = myPackage.FirstOrDefault(x => x.type == rate.type);
                        var plan = new plans
                        {
                            premium = computedPremium,
                            exchangeRate = exchnageRate,
                            travellers = _age.Count(),
                            package = section,
                            breakDown = premiumBreakDown
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

        public async Task<notification_response> GetDetailsByPassportNumber(string passportNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(passportNumber))
                {
                    return new notification_response
                    {
                        status = 404,
                        message = "Oops!, passport number cannot be empty"
                    };
                }

                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    var request = api.GetPassportDetails("Custodian", "Custodian@123", passportNumber);
                    if (request.StatusCode != "200")
                    {
                        return new notification_response
                        {
                            status = 302,
                            message = "passport details not found"
                        };
                    }

                    return new notification_response
                    {
                        status = 200,
                        message = "details retrieved successfully",
                        data = request
                    };
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new notification_response
                {
                    status = 404,
                    message = "oops!, something happend while getting customer details"
                };
            }
        }

        [HttpPost]
        public async Task<notification_response> BuyTravelInsurance(BuyTravel travel)
        {
            List<string> writtenFiles = new List<string>();
            try
            {
                log.Info("raw request object: " + Newtonsoft.Json.JsonConvert.SerializeObject(travel));
                var check_user_function = await util.CheckForAssignedFunction("BuyTravelInsurance", travel.merchant_id);
                if (!check_user_function)
                {
                    log.Info($"Permission denied from accessing this feature");
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == travel.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }
                var checkhash = await util.ValidateHash2(travel.details.Sum(x => x.premium) + travel.zone.ToString() + string.Join(",", travel.destination), config.secret_key, travel.hash);
                // This is for testing purpose remove before going to production
                checkhash = true;
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                //using (var api = new CustodianAPI.PolicyServicesSoapClient())
                //{
                #region
                //var request = await api.POSTTravelRecAsync(GlobalConstant.merchant_id,
                //    GlobalConstant.password, travel.title, travel.surname, travel.firstname, travel.date_of_birth.Value, travel.gender, travel.nationality,
                //    "Int'l PassPort", travel.passport_number, travel.occupation, travel.phone_number,
                //    travel.Email, travel.address, travel.zone.ToString().Replace("_", " "), travel.destination, travel.date_of_birth.Value, travel.return_date.Subtract(travel.departure_date).ToString(),
                //    travel.purpose_of_trip, travel.departure_date, travel.return_date, travel.premium, "", "", travel.transaction_ref, "API", "", "", "", travel.multiple_destination, "");
                //log.Info("RAW Response from API" + request.Passing_Travel_PostSourceResult);
                #endregion
                var group_reference = Guid.NewGuid().ToString();
                var details = travel.details.Select(x => new TravelInsurance
                {
                    premium = x.premium,
                    address = x.address,
                    date_of_birth = x.date_of_birth,
                    depature_date = travel.departure_date,
                    destination = travel.destination[0],
                    Email = x.Email,
                    firstname = x.firstname,
                    gender = x.gender,
                    Image_extension_type = x.extension,
                    merchant_id = travel.merchant_id,
                    multiple_destination = string.Join(",", travel.destination),
                    merchant_name = "",
                    nationality = x.nationality,
                    occupation = x.occupation,
                    passport_number = x.passport_number,
                    phone_number = x.phone_number,
                    purpose_of_trip = x.purpose_of_trip,
                    return_date = travel.return_date,
                    surname = x.surname,
                    title = "",
                    transaction_ref = travel.transaction_ref,
                    zone = travel.zone.ToString(),
                    file_path = $"{new Utility().GetSerialNumber().GetAwaiter().GetResult()}_{DateTime.Now.ToFileTimeUtc().ToString()}_{Guid.NewGuid().ToString()}.{x.extension}",
                    createdat = DateTime.Now,
                    group_count = travel.details.Count(),
                    group_reference = group_reference,
                }).ToList();
                var save = await dapper_core.BulkInsert(details);
                if (!save)
                {
                    if (dapper_core.TransactionState != null &&
                            dapper_core.TransactionState.Connection.State == System.Data.ConnectionState.Open)
                    {
                        dapper_core.TransactionState.Rollback();
                        dapper_core.TransactionState.Dispose();
                    }

                    return new notification_response
                    {
                        status = 301,
                        message = "Oops! something happened while processing information"
                    };
                }

                int i = 0;

                foreach (var item in details)
                {
                    var filepath = $"{ConfigurationManager.AppSettings["DOC_PATH"]}/Documents/Travel/{item.file_path}";
                    byte[] content = Convert.FromBase64String(travel.details[i].attachment);
                    File.WriteAllBytes(filepath, content);
                    writtenFiles.Add(filepath);
                    ++i;
                }

                if (dapper_core.TransactionState != null &&
                dapper_core.TransactionState.Connection.State == System.Data.ConnectionState.Open)
                {
                    dapper_core.TransactionState.Commit();
                    dapper_core.TransactionState.Dispose();
                }
                //http://192.168.10.74/webportal/travelcert.aspx?muser=ebusiness&mcert=0002474&mcert2=0002474
                return new notification_response
                {
                    status = 200,
                    message = "Transaction was successful",
                    data = new
                    {
                        cert_url = ""// GlobalConstant.Certificate_url + string.Format("muser=ebusiness&mcert={0}&mcert2={1}", cert_number, cert_number)
                    }
                };
            }
            catch (Exception ex)
            {
                if (dapper_core.TransactionState != null &&
                    dapper_core.TransactionState.Connection.State == System.Data.ConnectionState.Open)
                {
                    dapper_core.TransactionState.Rollback();
                    dapper_core.TransactionState.Dispose();
                }
                //rollback written files
                if (writtenFiles.Count() > 0)
                {
                    foreach (var path in writtenFiles)
                    {
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }
                }
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new notification_response
                {
                    status = 404,
                    message = "System malfunction, try again",

                };
            }
        }
    }
}
