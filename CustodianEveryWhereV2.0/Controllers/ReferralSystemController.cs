using DapperLayer.Dapper.Core;
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
using System.Web.Http.Cors;

namespace CustodianEveryWhereV2._0.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ReferralSystemController : ApiController
    {
        /// <summary>
        /// 
        /// </summary>
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private store<ReferralCodeLookUp> _referrals = null;
        private Utility util = null;
        private Core<dynamic> dapper_core = null;
        public ReferralSystemController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            dapper_core = new Core<dynamic>();
            _referrals = new store<ReferralCodeLookUp>();
        }
        /// <summary>
        /// `
        /// </summary>
        /// <param name="code"></param>
        /// <param name="merchant_id"></param>
        /// <returns></returns>

        [HttpGet]
        public async Task<dynamic> ValidateReferralCode(string code, string merchant_id)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("ValidateReferralCode", merchant_id);
                if (!check_user_function)
                {
                    return new
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }
                var validate = await dapper_core.ValidateReferralCode(code);
                if (validate == null)
                {
                    return new
                    {
                        status = 308,
                        message = $"Invalid referral code. '{code}'"
                    };
                }

                if (validate.Agnt_Status != "ACTIVE")
                {
                    return new
                    {
                        status = 302,
                        message = $"Referral code not active. '{code}'"
                    };
                }

                // [AgntRefID]
                //,[Agnt_Name]
                //,[Agnt_Status]
                //,[Agnt_Num]
                //,[Data_Source]
                return new
                {
                    status = 200,
                    message = "Referral code is valid",
                    data = new
                    {
                        agent_name = validate.Agnt_Name,
                        agent_status = validate.Agnt_Status,
                        agent_referral_code = validate.AgntRefID,
                        agent_core_system_referral_code = validate.Agnt_Num
                    }
                };

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException?.ToString());
                return new
                {
                    status = 400,
                    message = "System error: please try again"
                };

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="referrals"></param>
        /// <returns></returns>

        [HttpPost]
        public async Task<dynamic> UpdatePaymentRecord(Referrals referrals)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("UpdatePaymentRecord", referrals.merchant_id);
                if (!check_user_function)
                {
                    return new
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == referrals.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {referrals.merchant_id}");
                    return new
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }
                var checkhash = await util.ValidateHash2(referrals.AgentCode + referrals.Amount + referrals.CustomerName + referrals.TransactionRef, config.secret_key, referrals.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {referrals.merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                //re-validate referral code again
                #region revalidate agent code ensure things are fine
                var validate = await dapper_core.ValidateReferralCode(referrals.AgentCode);
                if (validate == null)
                {
                    return new
                    {
                        status = 308,
                        message = $"Invalid referral code. '{referrals.AgentCode}'"
                    };
                }
                if (validate.Agnt_Status != "ACTIVE")
                {
                    return new
                    {
                        status = 302,
                        message = $"Referral code not active. '{referrals.AgentCode}'"
                    };
                }
                #endregion

                var add_new = new ReferralCodeLookUp
                {
                    AgentCode = referrals.AgentCode,
                    Amount = referrals.Amount,
                    CustomerName = referrals.CustomerName,
                    ProductName = referrals.ProductName,
                    TransactionRef = referrals.TransactionRef
                };

                var verifyTranxRef = await _referrals.FindOneByCriteria(x => x.AgentCode == referrals.AgentCode && x.TransactionRef == referrals.TransactionRef);
                if (verifyTranxRef != null)
                {
                    return new
                    {
                        status = 309,
                        message = "Duplicate reference"
                    };
                }

                if (!await _referrals.Save(add_new))
                {
                    return new
                    {
                        status = 307,
                        message = "Referral was not mapped"
                    };
                }

                return new
                {
                    status = 200,
                    message = "Transaction mapped to referral successfully"
                };

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException?.ToString());
                return new
                {
                    status = 400,
                    message = "System error: please try again"
                };
            }
        }
    }
}
