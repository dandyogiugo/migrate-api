﻿using CustodianEmailSMSGateway.Email;
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
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace CustodianEveryWhereV2._0.Controllers
{
    public class CarTrackingController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private store<BuyTrackerDevice> track = null;
        private store<TelematicsUsers> telematics_user = null;
        public CarTrackingController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            track = new store<BuyTrackerDevice>();
            telematics_user = new store<TelematicsUsers>();
        }

        [HttpGet]
        public async Task<notification_response> GetCarLastStatus(string imei, string merchant_id, string hash, string email, string password)
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
                var checkhash = await util.ValidateHash2(imei + email + password, config.secret_key, hash);
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
                    var request = await api.GetAsync(GlobalConstant.base_url + $"getLastStatus?email={email}&passcode={passcode}&imei={imei}");
                    if (request.IsSuccessStatusCode)
                    {
                        var response = await request.Content.ReadAsAsync<dynamic>();
                        log.Info($"Raw response from api {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");
                        if (response.response_code == "00")
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
                                message = response.response_message
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
        public async Task<notification_response> GetCarStatusHistory(string imei, string start_date_time, string end_date_time, string merchant_id, string hash, string email, string password, int page = 1)
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
                var checkhash = await util.ValidateHash2(imei + email + password, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                //var base_url = ConfigurationManager.AppSettings["HALOGEN_API"];
                //var auth_email = ConfigurationManager.AppSettings["HALOGEN_AUTH_EMAIL"];
                //var passcode = ConfigurationManager.AppSettings["HALOGEN_PASSCODE"];

                using (var api = new HttpClient())
                {
                    var request = await api.GetAsync(GlobalConstant.base_url + $"getStatusHistory?email={email}&passcode={password}&imei={imei}&start_date_time={start_date_time}&end_date_time={end_date_time}&pageNo={page}&pageSize=10");
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
                                message = response.response_message
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
        public async Task<notification_response> GetCarAddress(string imei, string lng, string lat, string merchant_id, string email, string password, string hash)
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
                var checkhash = await util.ValidateHash2(imei + email + password, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                //var base_url = ConfigurationManager.AppSettings["HALOGEN_API"];
                //var auth_email = ConfigurationManager.AppSettings["HALOGEN_AUTH_EMAIL"];
                //var passcode = ConfigurationManager.AppSettings["HALOGEN_PASSCODE"];

                using (var api = new HttpClient())
                {
                    var request = await api.GetAsync(GlobalConstant.base_url + $"getAddress?email={email}&passcode={password}&latlng={lat},{lng}");
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
                                message = response.response_message
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

        [HttpPost]
        public async Task<notification_response> BuyTracker(BuyTracker tracker)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        status = 406,
                        message = "Some required parameters missing from request payload"
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("BuyTracker", tracker.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == tracker.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {tracker.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                // validate hash
                var checkhash = await util.ValidateHash2(tracker.address + tracker.customer_email, config.secret_key, tracker.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {tracker.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                DateTime install_date;
                var parse_date = DateTime.TryParse(tracker.installation_date_time, out install_date);
                if (!parse_date)
                {
                    return new notification_response
                    {
                        status = 407,
                        message = "Invalid Installation date"
                    };
                }

                using (var apicall = new HttpClient())
                {
                    var request = await apicall.PostAsJsonAsync(GlobalConstant.base_url + "submitRequest", new BuyTrackerPost
                    {
                        address = tracker.address,
                        contact_person = tracker.contact_person,
                        customer_email = tracker.customer_email,
                        customer_name = tracker.customer_name,
                        installation_date_time = install_date,
                        mobile_number = tracker.mobile_number,
                        plate_number = tracker.plate_number,
                        tracker_type_id = tracker.tracker_type_id,
                        user_email = GlobalConstant.auth_email,
                        user_passcode = GlobalConstant.passcode
                    });
                    log.Info($"response from halogen buytracker device is {request.StatusCode}");
                    if (request.IsSuccessStatusCode)
                    {
                        var response = await request.Content.ReadAsAsync<dynamic>();
                        log.Info($"response object from halogen buytracker device is {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");
                        if (response.response_code == "00")
                        {
                            var savenew_request = new BuyTrackerDevice
                            {
                                address = tracker.address,
                                annual_subscription = tracker.annual_subscription,
                                contact_person = tracker.contact_person,
                                customer_email = tracker.customer_email,
                                customer_name = tracker.customer_name,
                                date_created = DateTime.Now,
                                device_description = tracker.device_description,
                                installation_date_time = install_date,
                                mobile_number = tracker.mobile_number,
                                plate_number = tracker.plate_number,
                                price = tracker.price,
                                tracker_type_id = tracker.tracker_type_id,
                                vehicle_year = tracker.vehicle_year,
                                vehicle_make = tracker.vehicle_make,
                                vehicle_model = tracker.vehicle_model
                            };
                            log.Info($"about to save to database");
                            if (await track.Save(savenew_request))
                            {
                                log.Info($"data commited to database");
                                return new notification_response
                                {
                                    status = 200,
                                    message = "Your device has been successfully booked"
                                };
                            }
                            else
                            {
                                log.Info($"something happend while committing data check stack trace");
                                return new notification_response
                                {
                                    status = 203,
                                    message = "Oops!, something went wrong while processing your request"
                                };
                            }
                        }
                        else
                        {
                            return new notification_response
                            {
                                status = 205,
                                message = response.response_message
                            };
                        }
                    }
                    else
                    {
                        return new notification_response
                        {
                            status = 409,
                            message = "Tracker request was not successful"
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
                    message = "oops!, something happend while booking your request"
                };
            }
        }

        [HttpGet]
        public async Task<notification_response> GetCompactibleVehicle(int year, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetCompactibleVehicle", merchant_id);
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
                var checkhash = await util.ValidateHash2(year.ToString(), config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                using (var apicall = new HttpClient())
                {
                    var request = await apicall.GetAsync(GlobalConstant.base_url + $"getCompatibleTrackerTypes?vehicle_year={year}");
                    if (request.IsSuccessStatusCode)
                    {
                        var response = await request.Content.ReadAsAsync<DevicePricesResponse>();
                        if (response.response_code == "00")
                        {
                            log.Info("loaded successfully");
                            var cnt = response.data;
                            if (cnt.Count > 0)
                            {
                                response.data[0].discount = GlobalConstant.DiscountPriceHalogen;
                                response.data[0].actual_price = Convert.ToDecimal(GlobalConstant.HalogenDefaultPrice);
                                response.data[0].label = GlobalConstant.LabelHalogen;
                                response.data[0].price = Convert.ToDecimal(response.data[0].price) + Convert.ToDecimal(GlobalConstant.LoadingPrice);
                                return new notification_response
                                {
                                    status = 200,
                                    message = "operation successful",
                                    data = response.data
                                };
                            }
                            else
                            {
                                return new notification_response
                                {
                                    status = 209,
                                    message = "Vehicle not yet supported",
                                };
                            }
                            //return new notification_response
                            //{
                            //    status = 200,
                            //    message = "operation successful",
                            //    data = response.data
                            //};
                        }
                        else
                        {
                            log.Info("no vehicle found for selected year");
                            return new notification_response
                            {
                                status = 409,
                                message = "Sorry no device for your car model and year"
                            };
                        }
                    }
                    else
                    {
                        log.Info("error loading compactible vehicle with year");
                        return new notification_response
                        {
                            status = 405,
                            message = "oops!, something happend while getting vehicle"
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
                    message = "oops!, something happend while getting vehicle"
                };
            }
        }

        [HttpGet]
        public async Task<notification_response> StopStartCar(string imei, Car state, string email, string password, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("StopStartCar", merchant_id);
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
                var checkhash = await util.ValidateHash2(imei + email + password, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                using (var apicall = new HttpClient())
                {
                    var request = await apicall.GetAsync(GlobalConstant.base_url + $"{((state == Car.Start) ? "startCar" : "stopCar")}?email={email}&passcode={password}&imei={imei}");
                    if (request.IsSuccessStatusCode)
                    {
                        var response = await request.Content.ReadAsAsync<dynamic>();
                        if (response.response_code == "00")
                        {
                            return new notification_response
                            {
                                status = 200,
                                message = ((state == Car.Start) ? "Start" : "Stop") + " command was successful"
                            };
                        }
                        else
                        {
                            return new notification_response
                            {
                                status = 401,
                                message = response.response_message
                            };
                        }
                    }
                    else
                    {
                        return new notification_response
                        {
                            status = 401,
                            message = "Unable to start/stop car at the moment, Please try again"
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
                    message = "Error: Unable to start/stop car at the moment"
                };
            }
        }

        [HttpGet]
        public async Task<notification_response> ListenIn(string imei, string sos_number, string merchant_id, string hash, string email, string password)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("ListenIn", merchant_id);
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
                var checkhash = await util.ValidateHash2(imei + sos_number, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                using (var apicall = new HttpClient())
                {
                    var request = await apicall.GetAsync(GlobalConstant.base_url + $"listenIn?email={email}&passcode={password}&imei={imei}&sos_number={sos_number}");
                    if (request.IsSuccessStatusCode)
                    {
                        var response = await request.Content.ReadAsAsync<dynamic>();
                        if (response.response_code == "00")
                        {
                            return new notification_response
                            {
                                status = 200,
                                message = $"Command was successful, kindly call {sos_number} with your mobile phone and listen in"
                            };
                        }
                        else
                        {
                            return new notification_response
                            {
                                status = 401,
                                message = response.response_message
                            };
                        }
                    }
                    else
                    {
                        return new notification_response
                        {
                            status = 401,
                            message = "Unable to bind number to device, Please try again"
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
                    message = "Error: Unable to bind number to device"
                };
            }
        }

        [HttpPost]
        public async Task<notification_response> SetupUser(SetTeleUser user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Some required fields missing from request",
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("SetupUser", user.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == user.merchant_id);
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {user.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }
                // validate hash
                var checkhash = await util.ValidateHash2(user.Email + user.OTP + user.Newpassword, config.secret_key, user.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {user.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                // check if user exist
                var is_profile_setup = await telematics_user.FindOneByCriteria(x => x.email == user.Email);
                if (is_profile_setup != null)
                {
                    log.Info($"user has been setup already {user.Email}");
                    return new notification_response
                    {
                        status = 402,
                        message = "User profile is already setup"
                    };
                }
                // if user exist at halogen ends
                using (var apicall = new HttpClient())
                {
                    var request = await apicall.GetAsync(GlobalConstant.base_url + $"CheckEmail?email={user.Email}");
                    if (!request.IsSuccessStatusCode)
                    {
                        log.Info($"verifying from halogen {user.Email}");
                        return new notification_response
                        {
                            status = 409,
                            message = "Secondary verification failed"
                        };
                    }
                    var response = await request.Content.ReadAsAsync<dynamic>();
                    if (response.response_code != "00")
                    {
                        log.Info($"verifying from halogen failed {user.Email}");
                        return new notification_response
                        {
                            status = 409,
                            message = response.response_message
                        };
                    }

                    var validate_otp = await util.ValidateOTP(user.OTP, user.Email);
                    if (validate_otp)
                    {
                        var new_setup = new TelematicsUsers
                        {
                            CreatedAt = DateTime.Now,
                            email = user.Email,
                            Gender = user.Gender,
                            IsActive = true,
                            IsFromCustodian = true,
                            LoginLocation = user.LoginLocation,
                            OwnerName = user.OwnerName
                        };
                        await telematics_user.Save(new_setup);
                        return new notification_response
                        {
                            status = 200,
                            message = "User profile setup successfully"
                        };
                    }
                    else
                    {
                        return new notification_response
                        {
                            status = 401,
                            message = "Invalid OTP provided"
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
                    message = "Error: Unable to setup user, Try Again"
                };
            }
        }

        [HttpGet]
        public async Task<notification_response> SendSecureCode(string email, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("SendSecureCode", merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id);
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
                var checkhash = await util.ValidateHash2(email, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                //validate email

                using (var apicall = new HttpClient())
                {
                    var request = await apicall.GetAsync(GlobalConstant.base_url + $"CheckEmail?email={email}");
                    if (!request.IsSuccessStatusCode)
                    {
                        log.Info("Unable to verify email, Try Again");
                        return new notification_response
                        {
                            status = 409,
                            message = "Unable to verify email, Try Again"
                        };
                    }

                    var response = await request.Content.ReadAsAsync<dynamic>();
                    if (response.response_code != "00")
                    {
                        log.Info("Unable to verify email, Try Again");
                        return new notification_response
                        {
                            status = 407,
                            message = "Email address not associated with any tracking device"
                        };
                    }

                    var generate_otp = await util.GenerateOTP(false, email, "TELEMATICS", Platforms.ADAPT);
                    if (string.IsNullOrEmpty(generate_otp))
                    {
                        log.Info("Unable to generate OTP, Try Again");
                        return new notification_response
                        {
                            status = 408,
                            message = "Unable to generate OTP, Try Again"
                        };
                    }

                    // send OTP to email address
                    string messageBody = $"Adpat Telematics authentication code <br/><br/><h2><strong>{generate_otp}</strong></h2>";
                    var template = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Cert/Notification.html"));
                    StringBuilder sb = new StringBuilder(template);
                    sb.Replace("#CONTENT#", messageBody);
                    sb.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                    var imagepath = HttpContext.Current.Server.MapPath("~/Images/logo-white.png");
                    await Task.Factory.StartNew(() =>
                    {
                        new SendEmail().Send_Email(email, "Adapt", sb.ToString(), "Telematics Authentication", true, imagepath, null, null, null);
                    });

                    log.Info($"Otp was sent successfully to {email}");
                    return new notification_response
                    {
                        status = 408,
                        message = $"OTP was sent successfully to {email}"
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
                    message = "Error: Unable to verify user email, Try Again"
                };
            }
        }

        [HttpPost]
        public async Task<notification_response> AuthUser(AuthTeleUser userAuth)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        status = 406,
                        message = "Invalid request: some parameters missing from request",
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("AuthUser", userAuth.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == userAuth.merchant_id);
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {userAuth.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }
                // validate hash
                var checkhash = await util.ValidateHash2(userAuth.email + userAuth.password, config.secret_key, userAuth.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {userAuth.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                //check if user has been setup first
                var is_user_setup = await telematics_user.FindOneByCriteria(x => x.email.ToLower() == userAuth.email.ToLower() && x.IsActive == true);
                if (is_user_setup == null)
                {
                    log.Info("User account has not been mapped. kindly use the setup option");
                    return new notification_response
                    {
                        status = 406,
                        message = "User account has not been mapped. kindly use the setup option",
                    };
                }

                //authenticate from halogen
                using (var apicall = new HttpClient())
                {
                    var request = await apicall.GetAsync(GlobalConstant.base_url + $"getObjects?email={userAuth.email}&passcode={userAuth.password}");
                    if (!request.IsSuccessStatusCode)
                    {
                        log.Info("Host is not reachable, Try Again");
                        return new notification_response
                        {
                            status = 402,
                            message = "Host is not reachable, Try Again",
                        };
                    }

                    //read from  stream
                    var response = await request.Content.ReadAsAsync<dynamic>();
                    if (response.response_code != "00")
                    {
                        log.Info("Authentication failed: Hint(Invalid email or password)");
                        return new notification_response
                        {
                            status = 401,
                            message = "Authentication failed: Hint(Invalid email or password)",
                        };
                    }

                    // successful process
                    return new notification_response
                    {
                        status = 200,
                        message = "Authentication successful",
                        data = response.data
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
                    message = "Error: User authenication failed, Try Again"
                };
            }
        }

    }
}
