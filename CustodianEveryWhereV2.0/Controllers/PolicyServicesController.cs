using CustodianEmailSMSGateway.Email;
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
                var validate = await policyService.FindOneByCriteria(x => x.deviceimei == policy.imei || x.email == policy.email && x.is_setup_completed == true);
                if (validate != null)
                {
                    return new res
                    {
                        status = 201,
                        message = "User has already been setup"
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

                var email = lookup.FirstOrDefault(x => util.IsValid(x.email?.Trim()) == true);
                var phone = util.numberin234(lookup.FirstOrDefault(x => util.isValidPhone(x.phone?.Trim()) == true)?.phone);

                if (email == null && string.IsNullOrEmpty(phone))
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
                if (email != null)
                {
                    string test_email = "oscardybabaphd@gmail.com";
                    //email.email
                    BackgroundJob.Schedule(() => new SendEmail().Send_Email(test_email,
                          "Adapt-PolicyServices Authentication",
                          sb.ToString(), "PolicyServices Authentication",
                          true, imagepath, cc, null, null)
                      , TimeSpan.FromMilliseconds(20));

                }

                if (!string.IsNullOrEmpty(phone))
                {
                    phone = "2348069314541";
                    await new SendSMS().Send_SMS($"Adapt OTP: {generate_otp}", phone);
                }

                //var build = lookup.Select(x => new
                //{
                //    PolicyNo = x.policyno?.Trim(),
                //    StartDate = x.startdate.ToShortDateString(),
                //    EndDate = x.endtdate.ToShortTimeString(),
                //    Source = (x.datasource == "ABS") ? "General" : "Life",
                //    ProductName = x.productdesc,
                //    ProductType = x.productsubdesc,
                //    PolicyNumber = x.policyno?.Trim(),
                //    Status = x.status?.Trim().ToUpper()
                //}).ToList();

                dynamic pol = new ExpandoObject();

                var obj = lookup.First();
                //pol.FullName = obj.fullname;
                //pol.CustomerId = obj.customerid;
                //pol.Phone = obj.phone;
                //pol.Email = obj.email;
                //pol.PolicyList = build;

                //save record and set status inactive
                var check_setup_has_started = await policyService.FindOneByCriteria(x => x.customerid == obj.customerid.ToString().Trim() && x.email == policy.email.ToLower().Trim());
                if (check_setup_has_started == null)
                {
                    var savePolicy = new PolicyServicesDetails
                    {
                        createdat = DateTime.UtcNow,
                        customerid = obj.customerid.ToString().Trim(),
                        deviceimei = policy.imei,
                        devicename = policy.devicename,
                        email = policy.email.ToLower(),
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
                                customer_id = obj.customerid
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
                            customer_id = obj.customerid
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

                var checkhash = await util.ValidateHash2(validate.token + validate.email + validate.customerid, config.secret_key, validate.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {validate.merchant_id}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                var validateOTP = await util.ValidateOTP(validate.email.ToLower(), "POLICYSERVICE");
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

                var check_setup = await policyService.FindOneByCriteria(x => x.customerid == customer_id.Trim());
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
                    EndDate = x.endtdate.ToShortTimeString(),
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

        //[HttpGet]
        //public async Task<res> ResetPIN(string merchant_id, string newpin, string customer_id, string hash)
        //{
        //    try
        //    {

        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error(ex.Message);
        //        log.Error(ex.StackTrace);
        //        log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
        //        return new res { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
        //    }
        //}

        //[HttpGet]
        //public async Task<res> GetPolicyDetails(string merchant_id, string pin, string policynumber, string hash)
        //{
        //    try
        //    {

        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error(ex.Message);
        //        log.Error(ex.StackTrace);
        //        log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
        //        return new res { message = "System error, Try Again", status = (int)HttpStatusCode.NotFound };
        //    }
        //}
    }
}
