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
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CustodianEveryWhereV2._0.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class InterstateController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private store<InterStateStocks> interstate = null;
        private store<TempSaveData> _tempSaveData = null;
        public InterstateController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            interstate = new store<InterStateStocks>();
            _tempSaveData = new store<TempSaveData>();
        }

        [HttpPost]
        public async Task<res> OnboardingInterstate(InterState interState)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    //log.Error(Newtonsoft.Json.JsonConvert.SerializeObject(ModelState.Values));
                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            log.Error(error.ErrorMessage);
                        }
                    }
                    return new res
                    {
                        status = 401,
                        message = "Missing parameters from request"
                    };
                }

                var emailExist = await interstate.FindOneByCriteria(x => x.email.ToLower() == interState.email.ToLower());
                if (emailExist != null)
                {
                    return new res
                    {
                        status = 403,
                        message = $"User with email '{emailExist.email.ToLower()}' already exist"
                    };
                }
                var check_user_function = await util.CheckForAssignedFunction("OnboardingInterstate", interState.merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == interState.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {interState.merchant_id}");
                    return new res
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(interState.email, config.secret_key, interState.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {interState.merchant_id}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                var updateTempRecord = await _tempSaveData.FindOneByCriteria(x => x.email.ToLower() == interState.email.ToLower());
                if (updateTempRecord != null)
                {
                    updateTempRecord.isCompleted = true;
                    updateTempRecord.updatedAt = DateTime.Now;
                    await _tempSaveData.Update(updateTempRecord);
                }

                var data = new InterStateStocks
                {
                    createdAt = DateTime.Now,
                    data = Newtonsoft.Json.JsonConvert.SerializeObject(interState),
                    email = interState.email,
                    pushStatus = false
                };
                // call inter state api here
                if (!await interstate.Save(data))
                {
                    return new res
                    {
                        status = 409,
                        message = "Operation failed"
                    };
                }

                return new res
                {
                    status = 200,
                    message = "Onboarding successful"
                };

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = 404 };
            }
        }

        [HttpPost]
        public async Task<res> TempSaveData(InterState interState)
        {
            try
            {

                if (string.IsNullOrEmpty(interState.email))
                {
                    return new res
                    {
                        status = 405,
                        message = "Email is required to save"
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("TempSaveData", interState.merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == interState.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {interState.merchant_id}");
                    return new res
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(interState.email, config.secret_key, interState.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {interState.merchant_id}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var emailExist = await _tempSaveData.FindOneByCriteria(x => x.email.ToLower() == interState.email.ToLower());
                if (emailExist != null)
                {
                    emailExist.updatedAt = DateTime.Now;
                    emailExist.email = interState.email?.Trim().ToLower();
                    emailExist.data = Newtonsoft.Json.JsonConvert.SerializeObject(interState);
                    emailExist.updatedAt = DateTime.Now;
                    if (!await _tempSaveData.Update(emailExist))
                    {
                        return new res
                        {
                            status = 209,
                            message = "Update failed"
                        };
                    }
                    return new res
                    {
                        status = 200,
                        message = "Record was updated successfully"
                    };

                }
                var data = new TempSaveData
                {
                    createdAt = DateTime.Now,
                    data = Newtonsoft.Json.JsonConvert.SerializeObject(interState),
                    email = interState.email,
                    isCompleted = false
                };
                if (!await _tempSaveData.Save(data))
                {
                    return new res
                    {
                        status = 409,
                        message = "Operation failed"
                    };
                }

                return new res
                {
                    status = 200,
                    message = "Onboarding successful"
                };

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = 404 };
            }
        }

        [HttpGet]
        public async Task<res> GetTempData(string email, string merchant_id, string hash)
        {
            try
            {

                var check_user_function = await util.CheckForAssignedFunction("GetTempData", merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new res
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                var checkhash = await util.ValidateHash2(email, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                var result = await _tempSaveData.FindOneByCriteria(x => x.isCompleted == false && x.email.ToLower() == email.ToLower());
                if (result == null)
                {
                    return new res
                    {
                        status = 406,
                        message = $"No record found wiht emai '{email}'"
                    };
                }

                return new res
                {
                    status = 200,
                    message = "Operation successful",
                    data = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(result.data)
                };
            }
            catch (Exception ex)
            {

                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = 404 };
            }
        }

        [HttpGet]
        public async Task<res> GetFormSelections()
        {
            try
            {
                var title = new List<string>() { "Alh", "Chief", "Dr", "Engr", "Miss", "Mr", "Prof" };
                var gender = new List<string>() { "M", "F" };
                var idtypes = new List<dynamic>()
                {
                    new
                    {
                        desc = "Driver’s licence",
                        value = "DL"
                    },
                    new
                    {
                        desc = "International passport",
                        value = "IP"
                    },
                    new
                    {
                        desc = "National ID card",
                        value = "NID"
                    },
                    new
                    {
                        desc = "Voter's card",
                        value = "VC"
                    }
                };
                var relationship = new List<dynamic>()
                {
                    new
                    {
                        desc = "Brother",
                        value = "BRO"
                    },
                    new
                    {
                        desc = "Daughter",
                        value = "DAU"
                    },
                    new
                    {
                        desc = "Father",
                        value = "FA"
                    },
                     new
                    {
                        desc = "Husband",
                        value = "HUS"
                    },
                     new
                    {
                        desc = "Mother",
                        value = "MO"
                    },
                     new
                    {
                        desc = "Sister",
                        value = "SIS"
                    },
                     new
                    {
                        desc = "Son",
                        value = "SON"
                    },
                     new
                    {
                        desc = "Wife",
                        value = "WIF"
                    },
                     new
                    {
                        desc = "Other",
                        value = "OTH"
                    },
                };
                var bankCods = new List<dynamic>()
                {
                   new
                   {
                        Name = "ACCESS BANK",
                        Code = "044"
                   },
                   new
                   {
                        Name = "ACCESS BANK (DIAMOND)",
                        Code = "063"
                   },
                   new
                   {
                        Name = "ECOBANK NIGERIA",
                        Code = "050"
                   },
                   new
                   {
                        Name = "ENTERPRISE BANK",
                        Code = "084"
                   },
                   new
                   {
                        Name = "FIDELITY BANK",
                        Code = "070"
                   },
                   new
                   {
                        Name = "FIRST BANK",
                        Code = "011"
                   },
                   new
                   {
                        Name = "FIRST CITY MONUMENT BANK",
                        Code = "214"
                   },
                   new
                   {
                        Name = "GUARANTY TRUST BANK",
                        Code = "058"
                   },
                   new
                   {
                        Name = "HERITAGE BANK",
                        Code = "030"
                   },
                   new
                   {
                        Name = "JAIZ BANK",
                        Code = "301"
                   },
                   new
                   {
                        Name = "KEY STONE BANK",
                        Code = "082"
                   },
                   new
                   {
                        Name = "MAINSTREET BANK",
                        Code = "014"
                   },
                   new
                   {
                        Name = "SKYE BANK PLC (POLARIS)",
                        Code = "076"
                   },
                    new
                   {
                        Name = "STANBIC IBTC BANK",
                        Code = "221"
                   },
                     new
                   {
                        Name = "STANDARD CHARTERED BANK",
                        Code = "068"
                   },
                       new
                   {
                        Name = "STERLING BANK",
                        Code = "232"
                   },
                           new
                   {
                        Name = "UNION BANK OF NIGERIA",
                        Code = "032"
                   },
                    new {
                        Name = "UNITED BANK FOR AFRICA",
                        Code = "033"
                   },
                     new {
                        Name = "UNITY BANK",
                        Code = "215"
                   },
                     new {
                        Name = "WEMA BANK",
                        Code = "035"
                   },
                     new {
                        Name = "ZENITH BANK",
                        Code = "057"
                   }
                };
                var country_file = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/TravelCategoryJSON/Country.json"));
                var country_list = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(country_file);
                var country_phone_code = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/TravelCategoryJSON/countryPhoneCode.json"));
                var country_phone_code_list = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(country_phone_code);
                return new res
                {
                    status = 200,
                    message = "data fetch successully",
                    data = new
                    {
                        title,
                        gender,
                        idtypes,
                        relationship,
                        country_list,
                        country_phone_code_list,
                        bankCods
                    }
                };
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new res { message = "System error, Try Again", status = 404 };
            }
        }
    }
}
