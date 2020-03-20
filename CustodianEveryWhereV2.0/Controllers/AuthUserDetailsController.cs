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
using System.Threading.Tasks;
using System.Web.Http;

namespace CustodianEveryWhereV2._0.Controllers
{
    public class AuthUserDetailsController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private store<AdaptLeads> auth_user = null;
        public AuthUserDetailsController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            auth_user = new store<AdaptLeads>();
        }

        [HttpPost]
        public async Task<notification_response> AuthUser(UserAuthDetails auth_deatils)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    log.Info($"All request parameters are mandatory for email {auth_deatils.email}");
                    return new notification_response
                    {
                        status = 203,
                        message = "All request parameters are mandatory"
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("AuthUser", auth_deatils.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == auth_deatils.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {auth_deatils.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }


                // validate hash
                var checkhash = await util.ValidateHash2(auth_deatils.UUID + auth_deatils.email, config.secret_key, auth_deatils.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {auth_deatils.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var check_for_existence = await auth_user.FindOneByCriteria(x => x.email.ToLower() == auth_deatils.email.ToLower());
               
                bool dontpass = false;
                if (check_for_existence != null && (string.IsNullOrEmpty(check_for_existence.app_version) || string.IsNullOrEmpty(check_for_existence.fcm_token) 
                    || string.IsNullOrEmpty(check_for_existence.platform)))
                {
                    check_for_existence.platform = auth_deatils.platform;
                    check_for_existence.fcm_token = auth_deatils.fcm_token;
                    check_for_existence.app_version = auth_deatils.app_version;
                    check_for_existence.updatedAt = DateTime.Now;
                    await auth_user.Update(check_for_existence);
                    dontpass = true;
                }
                
                if (!dontpass && check_for_existence != null && (!auth_deatils.fcm_token.Trim().Equals(check_for_existence.fcm_token)
                    || auth_deatils.UUID.Trim().Equals(check_for_existence.UUID)))
                {
                    check_for_existence.platform = auth_deatils.platform;
                    check_for_existence.fcm_token = auth_deatils.fcm_token;
                    check_for_existence.app_version = auth_deatils.app_version;
                    check_for_existence.updatedAt = DateTime.Now;
                    await auth_user.Update(check_for_existence);
                }

                if (check_for_existence != null && !string.IsNullOrEmpty(check_for_existence.app_version) && !string.IsNullOrEmpty(check_for_existence.fcm_token) && !string.IsNullOrEmpty(check_for_existence.platform))
                {
                    log.Info($"User profile already exist {auth_deatils.email}");
                    return new notification_response
                    {
                        status = 200,
                        message = "User profile is active",
                        data = check_for_existence
                    };
                }

                if (check_for_existence == null)
                {
                    var new_user = new AdaptLeads
                    {
                        created_at = DateTime.Now,
                        email = auth_deatils.email.ToLower(),
                        fullname = auth_deatils.fullname,
                        UUID = auth_deatils.UUID,
                        app_version = auth_deatils.app_version,
                        fcm_token = auth_deatils.fcm_token,
                        platform = auth_deatils.platform,
                        createdAt = DateTime.Now
                    };

                    await auth_user.Save(new_user);
                }



                log.Info($"User profile hash been saved {auth_deatils.email}");
                return new notification_response
                {
                    status = 200,
                    message = $"User profile has been created for '{auth_deatils.email}'",
                    data = auth_deatils
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
                    message = "oops!, something happend while saving authentication details"
                };
            }
        }
    }
}
