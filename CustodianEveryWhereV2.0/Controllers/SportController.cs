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
        private store<MyPreference> _MyPeference = null;
        public SportController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            _MyPeference = new store<MyPreference>();
        }

        public async Task<notification_response> GetAllPreference(string email, string merchant_id, string hash)
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
                var checkhash = await util.ValidateHash2(merchant_id + email, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var checkIfUserIsLoginIn = await auth.FindOneByCriteria(x => x.email.ToLower() == email?.ToLower().Trim());

                if (checkIfUserIsLoginIn == null)
                {
                    return new notification_response
                    {
                        status = 306,
                        message = "Please login with your email to use this feature"
                    };
                }

                List<League> filtered = null;
                var getAllLeagues = await leagues.FindOneByCriteria(x => x.Id == Config.GetID);
                var MyPreference = await _MyPeference.FindOneByCriteria(x => x.Email?.Trim().ToUpper() == email?.Trim().ToUpper());
                var deserialise_preference = Newtonsoft.Json.JsonConvert.DeserializeObject<List<League>>(getAllLeagues.NewsFeed);
                if (MyPreference != null)
                {
                    filtered = new List<League>();
                    var deserialise_mypreference = Newtonsoft.Json.JsonConvert.DeserializeObject<List<League>>(MyPreference.MyPreferenceInJson);
                    foreach (var preference in deserialise_preference)
                    {
                        foreach (var mypreference in deserialise_mypreference)
                        {
                            if (preference.league_id == mypreference.league_id)
                            {
                                preference.is_my_preference = true;
                                break;
                            }
                        }
                        filtered.Add(preference);
                    }
                }

                return new notification_response
                {
                    status = 200,
                    message = "Preference retrieved successfully",
                    data = new
                    {
                        result = filtered ?? deserialise_preference
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

                var check_user_function = await util.CheckForAssignedFunction("AddToMyPreference", preference.merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == preference.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {preference.merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }
                // validate hash
                var checkhash = await util.ValidateHash2(preference.merchant_id + preference.email, config.secret_key, preference.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {preference.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
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

                if (preference.leagues.Count() < 1)
                {
                    return new notification_response
                    {
                        status = 308,
                        message = "Please select your preference"
                    };
                }
                var MyPreference = await _MyPeference.FindOneByCriteria(x => x.Email?.Trim().ToUpper() == preference.email?.Trim().ToUpper());
                if (MyPreference == null)
                {
                    var mypreference = new MyPreference
                    {
                        CreatedAt = DateTime.Now,
                        Email = preference.email,
                        MyPreferenceInJson = Newtonsoft.Json.JsonConvert.SerializeObject(preference.leagues)
                    };
                    await _MyPeference.Save(mypreference);
                    return new notification_response
                    {
                        status = 200,
                        message = "Preference was added successfully"
                    };
                }
                MyPreference.MyPreferenceInJson = Newtonsoft.Json.JsonConvert.SerializeObject(preference.leagues);
                await _MyPeference.Update(MyPreference);
                return new notification_response
                {
                    status = 200,
                    message = "Preference was updated successfully"
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

    }
}
