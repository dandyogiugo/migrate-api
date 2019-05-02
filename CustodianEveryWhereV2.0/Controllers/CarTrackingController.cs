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
using System.Web.Http;

namespace CustodianEveryWhereV2._0.Controllers
{
    public class CarTrackingController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        public CarTrackingController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
        }

        [HttpGet]
        public async Task<notification_response> GetCarLastStatus(string imei, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetCarLastStatus", merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
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
                var checkhash = await util.ValidateHash2(imei, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var base_url = ConfigurationManager.AppSettings["HALOGEN_API"];
                var auth_email = ConfigurationManager.AppSettings["HALOGEN_AUTH_EMAIL"];
                var passcode = ConfigurationManager.AppSettings["HALOGEN_PASSCODE"];

                using (var api = new HttpClient())
                {
                    var request = await api.GetAsync(base_url + $"getLastStatus?email={auth_email}&passcode={passcode}&imei={imei}");
                    if (request.IsSuccessStatusCode)
                    {
                        var response = await request.Content.ReadAsAsync<Dictionary<string, object>>();
                        log.Info($"Raw response from api {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");
                        if (response["response_code"].ToString() == "00")
                        {
                            log.Info($"status imei for user imei {imei}");
                            return new notification_response
                            {
                                status = 200,
                                message = "operation successful",
                                data = response
                            };
                        }
                        else
                        {
                            log.Info($"unable to get status imei for user imei {imei}");
                            return new notification_response
                            {
                                status = 206,
                                message = "Unable to retieve vehicle last status"
                            };
                        }
                    }
                    else
                    {
                        log.Info($"unable to get status imei for user imei {imei}");
                        return new notification_response
                        {
                            status = 205,
                            message = "Unable to get vehicle last status"
                        };
                    }
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

        [HttpGet]
        public async Task<notification_response> GetCarStatusHistory(string imei, string start_date_time, string end_date_time, string merchant_id, string hash)
        {
            try
            {
                log.Info(start_date_time + " " + end_date_time);
                var check_user_function = await util.CheckForAssignedFunction("GetCarStatusHistory", merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
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
                var checkhash = await util.ValidateHash2(imei, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var base_url = ConfigurationManager.AppSettings["HALOGEN_API"];
                var auth_email = ConfigurationManager.AppSettings["HALOGEN_AUTH_EMAIL"];
                var passcode = ConfigurationManager.AppSettings["HALOGEN_PASSCODE"];

                using (var api = new HttpClient())
                {
                    var request = await api.GetAsync(base_url + $"getStatusHistory?email={auth_email}&passcode={passcode}&imei={imei}&start_date_time={start_date_time}&end_date_time={end_date_time}");
                    if (request.IsSuccessStatusCode)
                    {
                        var response = await request.Content.ReadAsAsync<dynamic>();
                        log.Info($"Raw response from api {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");
                        if (response.response_code == "00")
                        {
                            log.Info($"unable to get status imei for user imei {imei}");
                            return new notification_response
                            {
                                status = 200,
                                message = "operation successful",
                                data = response
                            };
                        }
                        else
                        {
                            log.Info($"unable to get status imei for user imei {imei}");
                            return new notification_response
                            {
                                status = 206,
                                message = "Unable to retieve vehicle last status"
                            };
                        }
                    }
                    else
                    {
                        log.Info($"unable to get status imei for user imei {imei}");
                        return new notification_response
                        {
                            status = 205,
                            message = "Unable to get vehicle last status"
                        };
                    }
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

        [HttpGet]
        public async Task<notification_response> GetCarAddress(string imei, string lng, string lat, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetCarAddress", merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
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
                var checkhash = await util.ValidateHash2(imei, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var base_url = ConfigurationManager.AppSettings["HALOGEN_API"];
                var auth_email = ConfigurationManager.AppSettings["HALOGEN_AUTH_EMAIL"];
                var passcode = ConfigurationManager.AppSettings["HALOGEN_PASSCODE"];

                using (var api = new HttpClient())
                {
                    var request = await api.GetAsync(base_url + $"getAddress?email={auth_email}&passcode={passcode}&latlng={lat},{lng}");
                    if (request.IsSuccessStatusCode)
                    {
                        var response = await request.Content.ReadAsAsync<dynamic>();
                        log.Info($"Raw response from api {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");
                        if (response.response_code == "00")
                        {
                            log.Info($"unable to get status imei for user imei {imei}");
                            return new notification_response
                            {
                                status = 200,
                                message = "operation successful",
                                data = response
                            };
                        }
                        else
                        {
                            log.Info($"unable to get status imei for user imei {imei}");
                            return new notification_response
                            {
                                status = 206,
                                message = "Unable to retieve vehicle last status"
                            };
                        }
                    }
                    else
                    {
                        log.Info($"unable to get status imei for user imei {imei}");
                        return new notification_response
                        {
                            status = 205,
                            message = "Unable to get vehicle last status"
                        };
                    }
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
