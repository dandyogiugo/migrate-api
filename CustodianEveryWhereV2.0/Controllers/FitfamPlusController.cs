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
using System.Net.Http.Formatting;

namespace CustodianEveryWhereV2._0.Controllers
{
    public class FitfamPlusController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private store<DealsTransactionHistory> _deals = null;
        public FitfamPlusController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            _deals = new store<DealsTransactionHistory>();
        }

        [HttpGet]
        public async Task<Wallet> WelletBalance()
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new Wallet
                {
                    status = 404,
                    message = "oops!, something happend while getting your balance"
                };
            }
        }

        /// <summary>
        /// Get deals from Fitfam plus api
        /// </summary>
        /// <param name="merchant_id"></param>
        /// <returns></returns>

        [HttpGet]
        public async Task<notification_response> GetDeals(string merchant_id)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetDeals", merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                        type = DataStore.ViewModels.Type.SMS.ToString()
                    };
                }

                var endpoint = ConfigurationManager.AppSettings["ENDPOINTS"].Split('|');
                Dictionary<string, List<dynamic>> deals = new Dictionary<string, List<dynamic>>();
                List<dynamic> dy_deals = new List<dynamic>();
                using (var api = new HttpClient())
                {
                    foreach (var item in endpoint)
                    {
                        try
                        {
                            var request = await api.GetAsync(item + "deals");
                            if (request.IsSuccessStatusCode)
                            {
                                var response = await request.Content.ReadAsAsync<dynamic>();
                                if (response.status == "success")
                                {
                                    foreach (var subitems in response.deals)
                                    {
                                        dy_deals.Add(subitems);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex.Message);
                            log.Error(ex.StackTrace);
                            log.Error(ex.InnerException);
                        }
                    }
                }

                if (dy_deals != null && dy_deals.Count > 0)
                {
                    var ordered_list = dy_deals.OrderByDescending(x => Convert.ToDecimal(x.discounted_pric)).ToList();
                    deals.Add("deals", ordered_list);
                    return new notification_response
                    {
                        status = 200,
                        message = "deals loaded sucessfully",
                        data = deals
                    };
                }
                else
                {
                    return new notification_response
                    {
                        status = 205,
                        message = "No deal avaliable yet",
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
                    message = "oops!, something happend while getting deals"
                };
            }
        }

        [HttpGet]
        public async Task<notification_response> CheckIn(string phone, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("CheckIn", merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                        type = DataStore.ViewModels.Type.SMS.ToString()
                    };
                }


                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                // validate hash
                var checkhash = await util.ValidateHash2(phone, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var endpoint = ConfigurationManager.AppSettings["ENDPOINTS"].Split('|');
                Dictionary<string, List<dynamic>> deals = new Dictionary<string, List<dynamic>>();
                List<dynamic> dy_deals = new List<dynamic>();
                using (var api = new HttpClient())
                {
                    foreach (var item in endpoint)
                    {
                        try
                        {
                            var request = await api.GetAsync(item + $"customer/info/{phone}");
                            if (request.IsSuccessStatusCode)
                            {
                                var response = await request.Content.ReadAsAsync<dynamic>();
                                if (response.status == "success")
                                {
                                    dy_deals.Add(response.member);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex.Message);
                            log.Error(ex.StackTrace);
                            log.Error(ex.InnerException);
                        }
                    }
                }

                if (dy_deals != null && dy_deals.Count > 0)
                {
                    var ordered_list = dy_deals.OrderByDescending(x => Convert.ToDecimal(x.discounted_pric)).ToList();
                    deals.Add("membership", ordered_list);
                    return new notification_response
                    {
                        status = 200,
                        message = "details loaded sucessfully",
                        data = deals
                    };
                }
                else
                {
                    return new notification_response
                    {
                        status = 205,
                        message = "User does not exist",
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
                    message = "oops!, something happend while searching for details"
                };
            }
        }

        [HttpPost]
        public async Task<notification_response> BuyDeal(deal deal)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Some parameters are missing from request",
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("BuyDeal", deal.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == deal.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {deal.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                // validate hash
                var checkhash = await util.ValidateHash2(deal.email + deal.reference + deal.gym, config.secret_key, deal.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {deal.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                DateTime versary;

                var newdeal = new DealsTransactionHistory
                {
                    address = deal.address,
                    anniversary = (!string.IsNullOrEmpty(deal.anniversary) && DateTime.TryParse(deal.anniversary, out versary)) ? versary : default(DateTime),
                    discounted_percent = deal.discounted_percent,
                    discounted_price = deal.discounted_price,
                    dob = (!string.IsNullOrEmpty(deal.dob) && DateTime.TryParse(deal.dob, out versary)) ? versary : default(DateTime),
                    email = deal.email,
                    end_date = (!string.IsNullOrEmpty(deal.end_date) && DateTime.TryParse(deal.end_date, out versary)) ? versary : default(DateTime),
                    firstname = deal.firstname,
                    gender = deal.gender,
                    gym = deal.gym,
                    lastname = deal.lastname,
                    marital_status = deal.marital_status,
                    membership = deal.membership,
                    mobile = deal.mobile,
                    package_id = deal.package_id,
                    price = deal.price,
                    purchase_date = DateTime.Now,
                    reference = deal.reference,
                    start_date = (!string.IsNullOrEmpty(deal.start_date) && DateTime.TryParse(deal.start_date, out versary)) ? versary : default(DateTime)
                };

                //  int index = 0;
                var geturl = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Cert/config.json"))).FirstOrDefault(x => x.gymname == deal.gym.ToUpper().Trim());
                //for (int i = 0; i <= geturl.Count - 1; ++i)
                //{
                //    if (geturl[i].gymname.ToLower().Trim() == deal.gym.ToLower().Trim())
                //    {
                //        index = i;
                //        break;
                //    }
                //}

                if (geturl != null)
                {
                    using (var _api = new HttpClient())
                    {
                        string url = geturl.url;
                        log.Info($"endpoint to push {url}");
                        var request = await _api.PostAsJsonAsync(url, deal);
                        log.Info($"response code for fitfam {request.StatusCode}");
                        if (request.IsSuccessStatusCode)
                        {

                            var response = await request.Content.ReadAsAsync<dynamic>();
                            log.Info($"response from fitfam {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");
                            if (response != null && response.status != "fail")
                            {
                                await _deals.Save(newdeal);
                                log.Info($"Gym processing to api success");
                                return new notification_response
                                {
                                    status = 200,
                                    message = "Transaction was successful"
                                };
                            }
                            else
                            {
                                log.Info($"Gym processing to api failed");
                                return new notification_response
                                {
                                    status = 405,
                                    message = "Unable to push deal to gym. Try Agin"
                                };
                            }
                        }
                        else
                        {
                            log.Info($"Gym processing to api failed");
                            return new notification_response
                            {
                                status = 402,
                                message = "Unable to push deal to gym. Try Agin"
                            };
                        }
                    }
                }
                else
                {
                    log.Info($"Invalid config settings not config found on json config file");
                    return new notification_response
                    {
                        status = 407,
                        message = "Incorrect gym name"
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
                    message = "oops!, something happend while searching for details"
                };
            }
        }
    }
}
