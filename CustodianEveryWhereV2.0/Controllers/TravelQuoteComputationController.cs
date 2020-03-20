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
        private store<TravelInsurance> _travel = null;
        private Utility util = null;
        private Core<dynamic> dapper_core = null;
        public TravelQuoteComputationController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            dapper_core = new Core<dynamic>();
            _travel = new store<TravelInsurance>();
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

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == quote.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
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
                        _breakDown premiumForIndividual;
                        double loadedPremium = 0;
                        List<_breakDown> premiumBreakDown = new List<_breakDown>();
                        double loadedtotal = 0;
                        foreach (var prem in premium)
                        {
                            var roundedIndividualPremium = await util.RoundValueToNearst100((1.32 * prem) * exchnageRate);
                            computedPremium += roundedIndividualPremium;

                            if (quote.LoadingRate.HasValue && quote.LoadingRate.Value > 0 && !quote.IsFlatLoading)
                            {
                                double loading = (quote.LoadingRate.Value / 100 * ((1.32 * prem) * exchnageRate));
                                loadedPremium = await util.RoundValueToNearst100(loading + ((1.32 * prem) * exchnageRate));
                                loadedtotal += loadedPremium;
                            }
                            else if (quote.LoadingRate.HasValue && quote.LoadingRate.Value > 0 && quote.IsFlatLoading)
                            {
                                loadedPremium = await util.RoundValueToNearst100(quote.LoadingRate.Value + ((1.32 * prem) * exchnageRate));
                                loadedtotal += loadedPremium;
                            }

                            //roundedpremium += roundedIndividualPremium;
                            premiumForIndividual = new _breakDown
                            {
                                Id = count,
                                premium = roundedIndividualPremium,
                                dateOfBirth = quote.DateOfBirth[count],
                                loadedpremium = loadedPremium
                            };
                            count++;
                            premiumBreakDown.Add(premiumForIndividual);
                        }

                        log.Info($"Rate used: => {Newtonsoft.Json.JsonConvert.SerializeObject(rate)}");
                        var section = myPackage.FirstOrDefault(x => x.type == rate.type);
                        //double loadedtotal = 0;
                        //if (quote.LoadingRate.HasValue && quote.LoadingRate.Value > 0)
                        //{
                        //    var loading = ((quote.LoadingRate.Value / 100) * computedPremium);
                        //    loadedtotal = loading + computedPremium;
                        //}
                        var plan = new plans
                        {
                            premium = computedPremium,
                            loadedpremium = loadedtotal,
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
        [HttpGet]
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
            var group_reference = Guid.NewGuid().ToString() + "_" + DateTime.Now.Ticks;
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
                //checkhash = true;
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                if (!string.IsNullOrEmpty(travel.referrenceKey))
                {
                    var deleted = await dapper_core.DeleteRecord(travel.referrenceKey);
                    log.Info($"Previous record was deleted successfully with referrence number {travel.referrenceKey}");
                }

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
                    IsGroupLeader = x.isgroupleader,
                    IsGroup = travel.isGroup,
                    status = "PENDING",
                    type = travel.type,
                    referalCode = travel.referalCode
                }).ToList();
                // add response
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

                //image directory
                string dir = $"{ConfigurationManager.AppSettings["DOC_PATH"]}/Documents/Travel/{group_reference}";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                int i = 0;
                foreach (var item in details)
                {
                    var filepath = $"{ConfigurationManager.AppSettings["DOC_PATH"]}/Documents/Travel/{group_reference}/{item.file_path}";
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
                //return new notification_response
                //{
                //    status = 200,
                //    message = "Transaction was successful",
                //    data = new
                //    {
                //        cert_url = (!string.IsNullOrEmpty(cert1) && !string.IsNullOrEmpty(cert2)) ? $"http://192.168.10.74/webportal/travelcert.aspx?muser=ebusiness&mcert={cert1}&mcert2={cert2}" : ""// GlobalConstant.Certificate_url + string.Format("muser=ebusiness&mcert={0}&mcert2={1}", cert_number, cert_number)
                //    }
                //};

                return new notification_response
                {
                    status = 200,
                    message = "Referrence key generated successfully",
                    data = new
                    {
                        referrenceKey = group_reference
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
                string dir = $"{ConfigurationManager.AppSettings["DOC_PATH"]}/Documents/Travel/{group_reference}";
                if (Directory.Exists(dir))
                {
                    //foreach (var path in writtenFiles)
                    //{
                    //    if (File.Exists(path))
                    //    {
                    //        File.Delete(path);
                    //    }
                    //}
                    Directory.Delete(dir, true);
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

        [HttpGet]
        public async Task<notification_response> ConfirmTransaction(string referrenceNo, string referrenceKey, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("ConfirmTransaction", merchant_id);
                if (!check_user_function)
                {
                    log.Info($"Permission denied from accessing this feature");
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }
                var checkhash = await util.ValidateHash2(referrenceKey + referrenceNo, config.secret_key, hash);
                // This is for testing purpose remove before going to production
                //checkhash = true;
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var travelList = await _travel.FindMany(x => x.group_reference == referrenceKey && x.status == "PENDING");
                if (travelList.Count() == 0)
                {
                    log.Info($"Return invalid referrence key");
                    return new notification_response
                    {
                        status = 405,
                        message = "Invalid referrence key"
                    };
                }
                log.Info($"post to abs data {Newtonsoft.Json.JsonConvert.SerializeObject(travelList)}");
                string cert1 = "", cert2 = "";
                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    var travelABS = travelList.Select(x => new CustodianAPI.TravelInsuranceArray
                    {
                        Address = x.address,
                        Branch = "",
                        BrokerID = x.referalCode ?? "",
                        ChildU18 = "",
                        CommRate = "",
                        DateOfBirth = x.date_of_birth,
                        DepartureDate = x.depature_date,
                        Email = x.Email,
                        FirstName = x.firstname,
                        Gender = x.gender,
                        GroupCount = x.group_count.ToString(),
                        IdentificationNo = x.passport_number,
                        IsLeading = (x.IsGroupLeader) ? "Y" : "N",
                        LastName = x.surname,
                        Nationality = x.nationality,
                        IdentificationType = "International Passport",
                        PhoneNumber = x.phone_number,
                        MerchantID = GlobalConstant.merchant_id,
                        Mpassword = GlobalConstant.password,
                        Occupation = x.occupation,
                        PackageType = x.zone,
                        ReturnDate = x.return_date,
                        Purposeoftrip = x.purpose_of_trip,
                        TotalCost = (x.IsGroupLeader && x.IsGroup) ? travelList.Sum(y => y.premium).ToString() : x.premium.ToString(),
                        OtherCountry = x.multiple_destination,
                        TravelDestination = x.destination,
                        ReferenceNo = referrenceNo,
                        PeriodofInsurance = ((int)(x.return_date.Subtract(x.depature_date).TotalDays)).ToString(),
                        PostSource = "API",
                        TravelerDOB = x.date_of_birth,
                        TravelType = x.type,
                        m_CltAddress = x.address,
                        PremRate = "",
                        TitleName = "",
                        IsGroup = (x.IsGroup) ? "Y" : "N"
                    }).ToArray();
                    CustodianAPI.mTravelresponse request;
                    log.Info($"post to abs data {Newtonsoft.Json.JsonConvert.SerializeObject(travelABS)}");
                    try
                    {
                        request = api.POSTMultipleTravelRec(travelABS);
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.Message);
                        log.Error(ex.StackTrace);
                        log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                        throw;
                    }
                    log.Info($"raw response from apiv1.0 {Newtonsoft.Json.JsonConvert.SerializeObject(request)}");

                    if (request != null && request.RespCode == "200")
                    {
                        cert1 = request.RespData[0].Certificate;
                        cert2 = request.RespData[request.RespData.Count() - 1].Certificate;
                        List<TravelInsurance> _updatedDetails = new List<TravelInsurance>();
                        int j = 0;
                        foreach (var item in travelList)
                        {
                            item.policy_number = request.RespData[j].PolicyNo;
                            item.certificate_number = request.RespData[j].Certificate;
                            item.transaction_ref = referrenceNo;
                            item.Id = item.Id;
                            item.status = "COMPLETED";
                            _updatedDetails.Add(item);
                            ++j;
                        }

                        var update = await dapper_core.BulkUpdate(_updatedDetails);
                        if (update)
                        {
                            if (dapper_core.TransactionState != null &&
                    dapper_core.TransactionState.Connection.State == System.Data.ConnectionState.Open)
                            {
                                dapper_core.TransactionState.Commit();
                                dapper_core.TransactionState.Dispose();
                            }
                            bool result = false;
                            Boolean.TryParse(ConfigurationManager.AppSettings["IsDemoMode"], out result);

                            #region -- This section can only be executed on Demo mode (Test evnvironment)
                            if (result)
                            {
                                Task.Run(() =>
                                 {
                                     try
                                     {
                                         using (var api2 = new CustodianAPI.PolicyServicesSoapClient())
                                         {
                                             foreach (var item in travelList)
                                             {
                                                 string country = (item.nationality.ToLower().Contains("nigeria")) ? "NIGERIAN NIGERIA" : item.nationality;
                                                 var response = api2.PostTravel2Raga(item.depature_date, item.return_date,
                                                     item.firstname, item.surname, item.type, item.passport_number, item.date_of_birth, country,
                                                     "NIGERIA", item.Email, item.destination);
                                                 log.Info($"pushing to raga {item.passport_number} response from Raga {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");
                                             }
                                         }
                                     }
                                     catch (Exception ex)
                                     {

                                         log.Error(ex.Message);
                                         log.Error(ex.InnerException);
                                         log.Error(ex.StackTrace);
                                     }
                                 });
                            }
                            #endregion
                            // string _cert_url = ConfigurationManager.AppSettings["TRAVEL_CERT_URL"];
                            return new notification_response
                            {
                                status = 200,
                                message = "Transaction was successful",
                                data = new
                                {
                                    cert_url = (!string.IsNullOrEmpty(cert1) && !string.IsNullOrEmpty(cert2)) ? $"{GlobalConstant.Certificate_url}muser=ebusiness&mcert={cert1}&mcert2={cert2}" : ""// GlobalConstant.Certificate_url + string.Format("muser=ebusiness&mcert={0}&mcert2={1}", cert_number, cert_number)
                                }
                            };

                        }
                        else
                        {

                            if (dapper_core.TransactionState != null &&
                    dapper_core.TransactionState.Connection.State == System.Data.ConnectionState.Open)
                            {
                                dapper_core.TransactionState.Rollback();
                                dapper_core.TransactionState.Dispose();
                            }

                            return new notification_response
                            {
                                status = 202,
                                message = "Unable to push transaction due to system failure",
                            };
                        }

                    }
                    else
                    {
                        return new notification_response
                        {
                            status = 201,
                            message = "Unable to push transaction due to technical failure",
                        };
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                if (dapper_core.TransactionState != null &&
                   dapper_core.TransactionState.Connection.State == System.Data.ConnectionState.Open)
                {
                    dapper_core.TransactionState.Rollback();
                    dapper_core.TransactionState.Dispose();
                }
                return new notification_response
                {
                    status = 404,
                    message = "System malfunction, try again",

                };
            }
        }

    }
}
