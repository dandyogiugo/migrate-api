﻿using CustodianEmailSMSGateway.Email;
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

namespace CustodianEveryWhereV2._0.Controllers
{
    public class NotificationsController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        public NotificationsController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
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
                await Task.Factory.StartNew(() =>
                {
                    new SendEmail().Send_Email(email.ToAddress, email.Subject, sb.ToString(), email.Title, true, imagepath, email.CC, email.Bcc, null);
                });

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
    }
}
