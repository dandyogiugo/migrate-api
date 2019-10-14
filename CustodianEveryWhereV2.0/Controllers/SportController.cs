using CustodianEmailSMSGateway.SMS;
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

namespace CustodianEveryWhereV2._0.Controllers
{
    public class SportController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private store<News> leagues = null;
        private store<AdaptLeads> auth = null;
        public SportController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
        }

        public async Task<notification_response> GetAllPreference(string merchant_id, string hash)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetAllPreference", merchant_id);
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
                var checkhash = await util.ValidateHash2(merchant_id, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var getAllLeagues = await leagues.GetAll();
                return new notification_response
                {
                    status = 200,
                    message = "Preference retrieved successfully",
                    data = new
                    {
                        result = getAllLeagues
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
                    message = "oops!, something happend while getting preference"
                };
            }
        }

        [HttpPost]
        public async Task<notification_response> AddToMyPreference(LeagueObject preference)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    string errorMessage = "";
                    foreach (var item in ModelState?.Values)
                    {
                        foreach (var error in item?.Errors)
                        {
                            errorMessage += error.ErrorMessage + ", ";
                        }
                    }
                    return new notification_response
                    {
                        status = 402,
                        message = errorMessage
                    };
                }

                var checkIfUserIsLoginIn = await auth.FindOneByCriteria(x => x.email.ToLower() == preference.email?.ToLower().Trim());

                if (checkIfUserIsLoginIn == null)
                {
                    return new notification_response
                    {
                        status = 306,
                        message = "Please login with your email to use this feature"
                    };
                }




                return null;


            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new notification_response
                {
                    status = 404,
                    message = "oops!, something happend while getting preference"
                };
            }
        }

    }
}
