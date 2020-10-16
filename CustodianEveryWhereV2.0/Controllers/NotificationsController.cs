using CustodianEmailSMSGateway.Email;
using CustodianEmailSMSGateway.SMS;
using DataStore.Models;
using DataStore.repository;
using DataStore.Utilities;
using DataStore.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CustodianEveryWhereV2._0.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class NotificationsController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private store<Token> _otp = null;
        public NotificationsController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            _otp = new store<Token>();
        }

        [HttpPost]
        public async Task<notification_response> SendEmail(Email email)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        message = "Some required parameters are missing from request. please check Api docs for all required params",
                        status = 203,
                        type = DataStore.ViewModels.Type.EMAIL.ToString()
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("SendEmail", email.Merchant_Id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                        type = DataStore.ViewModels.Type.EMAIL.ToString()
                    };
                }

                var template = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Cert/Notification.html"));
                StringBuilder sb = new StringBuilder(template);
                sb.Replace("#CONTENT#", email.htmlBody);
                sb.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                var imagepath = HttpContext.Current.Server.MapPath("~/Images/logo-white.png");
                Task.Factory.StartNew(() =>
                {
                    new SendEmail().Send_Email(email.ToAddress, email.Subject, sb.ToString(), email.Title, true, imagepath, email.CC, email.Bcc, null);
                });

                if (!string.IsNullOrEmpty(email.ExtraHtmlBody) && email.CCUnit != null && email.CCUnit.Count() > 0)
                {
                    var template2 = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Cert/Notification.html"));
                    StringBuilder sb2 = new StringBuilder(template);
                    sb2.Replace("#CONTENT#", email.ExtraHtmlBody);
                    sb2.Replace("#TIMESTAMP#", string.Format("{0:F}", DateTime.Now));
                    int i = 0;
                    List<string> newCC = new List<string>();
                    foreach (var item in email.CCUnit)
                    {
                        if (i == 0)
                        {
                            ++i;
                            continue;

                        }
                        else
                        {
                            newCC.Add(item);
                        }
                        ++i;
                    }

                    Task.Factory.StartNew(() =>
                    {
                        new SendEmail().Send_Email(email.CCUnit[0], email.Subject, sb2.ToString(), email.Title, true, imagepath, newCC, null, null);
                    });
                }

                return new notification_response
                {
                    status = 200,
                    message = "Email was sent successfully",
                    type = DataStore.ViewModels.Type.EMAIL.ToString()
                };

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new notification_response
                {
                    message = "Error occured while sending email",
                    status = 203,
                    type = DataStore.ViewModels.Type.EMAIL.ToString()
                };
            }
        }

        [HttpPost]
        public async Task<notification_response> SendSMS(SMS sms)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        message = "Some required parameters are missing from request. please check Api docs for all required params",
                        status = 203,
                        type = DataStore.ViewModels.Type.SMS.ToString()
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("SendSMS", sms.Merchant_Id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                        type = DataStore.ViewModels.Type.SMS.ToString()
                    };
                }

                await new SendSMS().Send_SMS(sms.Message, sms.PhoneNumber);

                return new notification_response
                {
                    status = 200,
                    message = "Sms was sent successfully",
                    type = DataStore.ViewModels.Type.SMS.ToString()
                };

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                return new notification_response
                {
                    message = "Error occured while sending email",
                    status = 203,
                    type = DataStore.ViewModels.Type.SMS.ToString()
                };
            }
        }

        [HttpPost]
        public async Task<notification_response> SendOTP(user_otp send)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new notification_response
                    {
                        status = 408,
                        message = "Some parameters missing",
                        type = DataStore.ViewModels.Type.OTP.ToString()
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("SendOTP", send.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                        type = DataStore.ViewModels.Type.OTP.ToString()
                    };
                }

                //check api config
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == send.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {send.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                // validate hash
                var checkhash = await util.ValidateHash2(send.mobile + send.fullname, config.secret_key, send.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {send.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                log.Info($"about to send otp to number {send.mobile}");
                var old_otp = await _otp.FindOneByCriteria(x => x.is_used == false && x.is_valid == true && x.mobile_number == send.mobile);
                if (old_otp != null)
                {
                    log.Info($"deactivating previous un-used otp for mobile: {send.mobile}");
                    old_otp.is_used = true;
                    old_otp.is_valid = false;
                    await _otp.Update(old_otp);
                }
                log.Info($"creating new opt for user: {send.mobile}");
                var new_otp = new Token
                {
                    datecreated = DateTime.Now,
                    fullname = send.fullname,
                    is_used = false,
                    is_valid = true,
                    mobile_number = send.mobile,
                    platform = send.platform,
                    otp = Security.Transactions.UID.Codes.TransactionCodes.GenTransactionCodes(6)
                };
                await _otp.Save(new_otp);
                log.Info($"otp was saved successfully: {send.mobile}");
                var sms = new SendSMS();
                string body = $"Authentication code: {new_otp.otp}";
                string number = string.Empty;
                if (new_otp.mobile_number.StartsWith("0"))
                {
                    number = "234" + new_otp.mobile_number.Remove(0, 1);
                }
                else
                {
                    number = new_otp.mobile_number;
                }
                var response = await sms.Send_SMS(body, number);
                if (response == "SUCCESS")
                {
                    log.Info($"otp was sent successfully to mobile: {send.mobile}");
                    return new notification_response
                    {
                        status = 200,
                        message = "OTP sent successfully",
                        type = DataStore.ViewModels.Type.OTP.ToString()
                    };

                }
                else
                {
                    log.Info($"error sending otp to : {send.mobile}");
                    return new notification_response
                    {
                        status = 207,
                        message = "Oops! we couldn't send OTP to the provided number",
                        type = DataStore.ViewModels.Type.OTP.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                return new notification_response
                {
                    message = "Error generating token",
                    status = 404,
                };
            }
        }

        [HttpGet]
        public async Task<notification_response> ValidateOTP(string token, string mobile, string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("ValidateOTP", merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                        type = DataStore.ViewModels.Type.SMS.ToString()
                    };
                }

                //check api config
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
                var checkhash = await util.ValidateHash2(token + mobile, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                if (token.Length < 6 || token.Length > 6)
                {
                    log.Info($"you have provided an invalid otp {mobile}");
                    return new notification_response
                    {
                        status = 304,
                        message = "you have provided an invalid OTP"
                    };
                }

                var validate = await _otp.FindOneByCriteria(x => x.is_used == false && x.is_valid == true && x.mobile_number == mobile && x.otp == token);
                if (validate != null)
                {
                    log.Info($"you have provided an valid otp {mobile}");
                    return new notification_response
                    {
                        status = 200,
                        message = "you have provided a valid OTP"
                    };
                }
                else
                {
                    log.Info($"you have provided an invalid otp {mobile}");
                    return new notification_response
                    {
                        status = 201,
                        message = "you have provided an invalid OTP"
                    };
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                return new notification_response
                {
                    message = "Error validating token",
                    status = 203,
                };
            }
        }
    }
}
