﻿using CustodianEmailSMSGateway.Email;
using CustodianEmailSMSGateway.SMS;
using DapperLayer.Dapper.Core;
using DataStore.Models;
using DataStore.repository;
using DataStore.Utilities;
using DataStore.ViewModels;
using Hangfire;
using NLog;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CustodianEveryWhereV2._0.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class PolicyServicesController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private Utility util = null;
        private store<ApiConfiguration> _apiconfig = null;
        private store<PolicyServicesDetails> policyService = null;
        private Core<policyInfo> _policyinfo = null;
        public PolicyServicesController()
        {
            util = new Utility();
            _apiconfig = new store<ApiConfiguration>();
            policyService = new store<PolicyServicesDetails>();
            _policyinfo = new Core<policyInfo>();
        }

        [HttpPost]
        public async Task<res> Setup(setup policy)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new res { message = "provide poilcy number", status = (int)HttpStatusCode.ExpectationFailed };
                }

                var check_user_function = await util.CheckForAssignedFunction("PolicyServicesSetup", policy.merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = (int)HttpStatusCode.Unauthorized,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == policy.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {policy.merchant_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(policy.policynumber, config.secret_key, policy.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {policy.merchant_id}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                var validate = await policyService.FindOneByCriteria(x => x.policynumber == policy.policynumber && x.is_setup_completed == true);
                if (validate != null)
                {
                    return new res
                    {
                        status = 201,
                        message = "User has already been setup",
                        data = new
                        {
                            customer_id = validate.customerid,
                            email = validate.email,
                            phonenumber = validate.phonenumber
                        }
                    };
                }

                var lookup = await _policyinfo.GetPolicyServices(policy.policynumber);
                if (lookup == null || lookup.Count() <= 0)
                {
                    return new res
                    {
                        status = 207,
                        message = "Policy not found (Please contact custodain care centre)"
                    };
                }

                var email = lookup.FirstOrDefault(x => util.IsValid(x.email?.Trim()) == true)?.email;
                var phone = util.numberin234(lookup.FirstOrDefault(x => util.isValidPhone(x.phone?.Trim()) == true)?.phone);

                if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(phone))
                {
                    return new res
                    {
                        status = (int)HttpStatusCode.BadRequest,
                        message = "No valid phonenumber or email attached to provided policy (Please contact custodain care centre)"
                    };
                }

                var generate_otp = await util.GenerateOTP(false, policy.email.ToLower(), "POLICYSERVICE", Platforms.ADAPT);
                string messageBody = $"Adapt Policy Services authentication code <br/><br/><h2><strong>{generate_otp}</strong></h2>";
                var template = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Cert/Adapt.html"));
                StringBuilder sb = new StringBuilder(template);
                sb.Replace("#CONTENT#", messageBody);
                sb.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                var imagepath = HttpContext.Current.Server.MapPath("~/Images/adapt_logo.png");
                List<string> cc = new List<string>();
                cc.Add("technology@custodianplc.com.ng");
                //use handfire
                if (!string.IsNullOrEmpty(email))
                {
                    string test_email = "oscardybabaphd@gmail.com";
                    //email.email
                    var test = Config.isDemo ? "Test" : null;
                    new SendEmail().Send_Email(test_email,
                               $"Adapt-PolicyServices Authentication {test}",
                               sb.ToString(), $"PolicyServices Authentication {test}",
                               true, imagepath, cc, null, null);
                }

                if (!Config.isDemo)
                {
                    if (!string.IsNullOrEmpty(phone))
                    {
                        //phone = "2348069314541";
                        await new SendSMS().Send_SMS($"Adapt OTP: {generate_otp}", phone);
                    }
                }
                dynamic pol = new ExpandoObject();
                var obj = lookup.First();

                //save record and set status inactive
                var check_setup_has_started = await policyService.FindOneByCriteria(x => x.customerid == obj.customerid.ToString().Trim());
                if (check_setup_has_started == null)
                {
                    var savePolicy = new PolicyServicesDetails
                    {
                        createdat = DateTime.UtcNow,
                        customerid = obj.customerid.ToString().Trim(),
                        deviceimei = policy.imei,
                        devicename = policy.devicename,
                        email = obj.email?.ToLower(),
                        is_setup_completed = false,
                        phonenumber = obj.phone,
                        policynumber = obj.policyno?.Trim().ToUpper(),
                        os = policy.os,

                    };

                    if (await policyService.Save(savePolicy))
                    {
                        return new res
                        {
                            message = $"OTP has been sent to email {obj.email} and phone {obj.phone} attached to your policy",
                            status = (int)HttpStatusCode.OK,
                            data = new
                            {
                                customer_id = obj.customerid,
                                email = obj.email?.Trim(),
                                phonenumber = obj.phone?.Trim()
                            }
                        };
                    }
                    else
                    {
                        return new res
                        {
                            message = "Something happend while processing your information",
                            status = (int)HttpStatusCode.InternalServerError,
                        };
                    }
                }
                else
                {
                    return new res
                    {

                        message = $"OTP has been sent to email {obj.email} and phone { obj.phone} attached to your policy",
                        status = (int)HttpStatusCode.OK,
                        data = new
                        {
                            customer_id = check_setup_has_started.customerid,
                            email = check_setup_has_started.email,
                            phonenumber = check_setup_has_started.phonenumber
                        }
                    };
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
            }
        }

        [HttpPost]
        public async Task<res> ValidateOTP(ValidatePolicy validate)
        {
            try
            {

                var check_user_function = await util.CheckForAssignedFunction("PolicyServicesSetupValidateOTP", validate.merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = (int)HttpStatusCode.Unauthorized,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == validate.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {validate.merchant_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }
                var getRecord = await policyService.FindOneByCriteria(x => x.customerid == validate.customerid);
                if (getRecord != null && getRecord.is_setup_completed)
                {
                    log.Info($"valid merchant Id {validate.merchant_id} setup completed");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "User has finish setting up profile kindly login to access your policy"
                    };
                }

                var checkhash = await util.ValidateHash2(validate.otp + validate.customerid, config.secret_key, validate.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {validate.merchant_id}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                var validateOTP = await util.ValidateOTP(validate.otp, validate.email.ToLower());
                if (!validateOTP)
                {
                    log.Info($"invalid OTP for {validate.merchant_id} email {validate.email}");
                    return new res
                    {
                        status = 405,
                        message = "Invalid OTP"
                    };
                }
                else
                {
                    if (getRecord == null)
                    {
                        return new res
                        {
                            status = 407,
                            message = "Invalid customer id"
                        };
                    }
                    getRecord.updatedat = DateTime.UtcNow;
                    getRecord.is_setup_completed = true;
                    getRecord.pin = util.Sha256(validate.pin);
                    await policyService.Update(getRecord);
                    return new res
                    {
                        status = 200,
                        message = "OTP Validated successfully"
                    };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
            }
        }

        [HttpGet]
        public async Task<res> GetPolicies(string merchant_id, string pin, string customer_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetPoliciesServices", merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = (int)HttpStatusCode.Unauthorized,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(customer_id + pin, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                var _pin = util.Sha256(pin);
                var check_setup = await policyService.FindOneByCriteria(x => x.customerid == customer_id.Trim() && x.pin == _pin);

                if (check_setup == null)
                {
                    return new res
                    {
                        status = 405,
                        message = "Invalid customer id"
                    };
                }

                var lookup = await _policyinfo.GetPolicyServices(check_setup.policynumber);
                if (lookup == null || lookup.Count() <= 0)
                {
                    return new res
                    {
                        status = 207,
                        message = "Policy not found (Please contact custodain care centre)"
                    };
                }
                var build = lookup.Select(x => new
                {
                    PolicyNo = x.policyno?.Trim(),
                    StartDate = x.startdate.ToShortDateString(),
                    EndDate = x.enddate.ToShortDateString(),
                    Source = (x.datasource == "ABS") ? "General" : "Life",
                    ProductName = x.productdesc,
                    ProductType = x.productsubdesc,
                    PolicyNumber = x.policyno?.Trim(),
                    Status = x.status?.Trim().ToUpper()
                }).ToList();

                dynamic pol = new ExpandoObject();
                var obj = lookup.First();
                pol.FullName = obj.fullname;
                pol.CustomerId = obj.customerid;
                pol.Phone = obj.phone;
                pol.Email = obj.email;
                pol.PolicyList = build;

                log.Info($"Policies fetched succesully for {obj.email}");
                return new res
                {
                    status = 200,
                    message = "Policies fetched succesully",
                    data = pol
                };

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
            }
        }

        [HttpGet]
        public async Task<res> ResetPIN(string merchant_id, string newpin, string otp, string customer_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("ResetPINPoliciesServices", merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = (int)HttpStatusCode.Unauthorized,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(customer_id + newpin + otp, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                

                var getCustomer = await policyService.FindOneByCriteria(x => x.customerid == customer_id);


                if (getCustomer == null)
                {
                    log.Info($"Invalid customer Id {customer_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }

                var validateOTP = await util.ValidateOTP(otp, getCustomer.email.ToLower());
                if (!validateOTP)
                {
                    log.Info($"Invalid customer Id {customer_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid OTP"
                    };
                }
                var _pin = util.Sha256(newpin);
                getCustomer.pin = _pin;
                getCustomer.updatedat = DateTime.UtcNow;
                if (await policyService.Update(getCustomer))
                {
                    log.Info($"Invalid customer Id {customer_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.OK,
                        message = "Pin update successfully"
                    };
                }
                else
                {
                    log.Info($"Invalid customer Id {customer_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.NotAcceptable,
                        message = "Updated was not successful"
                    };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
            }
        }

        [HttpGet]
        public async Task<res> GetPolicyDetails(string merchant_id, string source, string pin, string policynumber, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetPolicyDetailsPoliciesServices", merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = (int)HttpStatusCode.Unauthorized,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(policynumber + pin, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var encryptPin = util.Sha256(pin);
                var checkuser = await policyService.FindOneByCriteria(x => x.pin == encryptPin && x.policynumber == policynumber);
                if (checkuser == null)
                {
                    log.Info($"Pin authentication failed {policynumber}");
                    return new res
                    {
                        status = 409,
                        message = $"Invalid PIN for '{policynumber}'"
                    };
                }

                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    var request = api.GetMorePolicyDetails(GlobalConstant.merchant_id, GlobalConstant.password, source, policynumber);
                    if (request == null)
                    {
                        log.Info($"Unable to fetch policy with policynumber {policynumber}");
                        return new res
                        {
                            status = 409,
                            message = $"Unable to fetch policy with policynumber '{policynumber}'"
                        };
                    }


                    return new res
                    {
                        status = 200,
                        message = "Dfetch was successful",
                        data = request
                    };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
            }
        }

        [HttpGet]
        public async Task<res> GenerateOTP(string customer_id, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GenerateOTPPoliciesServices", merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = (int)HttpStatusCode.Unauthorized,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new res
                    {
                        status = (int)HttpStatusCode.Forbidden,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(customer_id, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {customer_id}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                var checkuser = await policyService.FindOneByCriteria(x => x.customerid == customer_id);
                if (checkuser == null)
                {
                    log.Info($"Invalid customer id {customer_id}");
                    return new res
                    {
                        status = 409,
                        message = $"Customer id is not valid"
                    };
                }

                var generate_otp = await util.GenerateOTP(false, checkuser.email?.ToLower(), "POLICYSERVICE", Platforms.ADAPT);
                string messageBody = $"Adapt Policy Services authentication code <br/><br/><h2><strong>{generate_otp}</strong></h2>";
                var template = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Cert/Adapt.html"));
                StringBuilder sb = new StringBuilder(template);
                sb.Replace("#CONTENT#", messageBody);
                sb.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                var imagepath = HttpContext.Current.Server.MapPath("~/Images/adapt_logo.png");
                List<string> bcc = new List<string>();
                bcc.Add("technology@custodianplc.com.ng");
                //use handfire
                if (!string.IsNullOrEmpty(checkuser.email))
                {
                    string test_email = "oscardybabaphd@gmail.com";
                    //email.email
                    var test = Config.isDemo ? "Test" : null;
                    new SendEmail().Send_Email(test_email,
                          $"Adapt-PolicyServices PIN Reset {test}",
                          sb.ToString(), $"Adapt-PolicyServices PIN Reset {test}",
                          true, imagepath, null, bcc, null);
                }

                if (!Config.isDemo)
                {
                    if (!string.IsNullOrEmpty(checkuser.phonenumber))
                    {
                        //phone = "2348069314541";
                        await new SendSMS().Send_SMS($"Adapt OTP: {generate_otp}", checkuser.phonenumber);
                    }
                }
                return new res
                {

                    message = $"OTP has been sent to email {checkuser.email} and phone { checkuser.phonenumber} attached to your policy",
                    status = (int)HttpStatusCode.OK,
                };
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
            }
        }
    }
}
