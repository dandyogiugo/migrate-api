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
    public class AgentController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        public AgentController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
        }

        [HttpPost]
        public async Task<policy_data> GetPolicyDetails(policydetails pol_detials)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    log.Info($"All request parameters are mandatory for policy search {pol_detials.policy_number}");
                    return new policy_data
                    {
                        status = 203,
                        message = "All request parameters are mandatory"
                    };
                }

                var headerValues = HttpContext.Current.Request.Headers.Get("Authorization");

                if (string.IsNullOrEmpty(headerValues))
                {
                    log.Info($"Authorization denied for policy search {pol_detials.policy_number}");
                    return new policy_data
                    {
                        status = 205,
                        message = "Authorization denied"
                    };
                }

                var validate_headers = await util.ValidateHeaders(headerValues, pol_detials.merchant_id);

                if (!validate_headers)
                {
                    log.Info($"Authorization failed feature for policy search {pol_detials.policy_number}");
                    return new policy_data
                    {
                        status = 407,
                        message = "Authorizatikon failed"
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("GetPolicyDetails", pol_detials.merchant_id);
                if (!check_user_function)
                {
                    log.Info($"Permission denied from accessing this feature for policy search {pol_detials.policy_number}");
                    return new policy_data
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }


                using (var api = new CustodianAPI.CustodianEverywhereAPISoapClient())
                {
                    var request = await api.GetMorePolicyDetailsAsync(GlobalConstant.merchant_id, GlobalConstant.password, pol_detials.subsidiary.ToString(), pol_detials.policy_number);
                    log.Info($"raw api response  {Newtonsoft.Json.JsonConvert.SerializeObject(request)}");
                    if (request == null || request.PolicyNo == "NULL")
                    {

                        log.Info($"Invalid policy number for policy search {pol_detials.policy_number}");
                        return new policy_data
                        {
                            status = 202,
                            message = "Invalid policy number"
                        };
                    }

                    return new policy_data
                    {
                        status = 200,
                        message = "policy number is valid",
                        data = request
                    };

                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new policy_data
                {
                    status = 404,
                    message = "system malfunction"
                };
            }
        }

        [HttpPost]
        public async Task<policy_data> PostTransaction(PostTransaction post)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    log.Info($"All request parameters are mandatory for policy search(PostTransaction) {post.policy_number}");
                    return new policy_data
                    {
                        status = 203,
                        message = "All request parameters are mandatory"
                    };
                }

                var headerValues = HttpContext.Current.Request.Headers.Get("Authorization");

                if (string.IsNullOrEmpty(headerValues))
                {
                    log.Info($"Authorization denied for policy search(PostTransaction)  {post.policy_number}");
                    return new policy_data
                    {
                        status = 205,
                        message = "Authorization denied"
                    };
                }

                var validate_headers = await util.ValidateHeaders(headerValues, post.merchant_id);

                if (!validate_headers)
                {
                    log.Info($"Authorization failed feature for policy search(PostTransaction)  {post.policy_number}");
                    return new policy_data
                    {
                        status = 407,
                        message = "Authorizatikon failed"
                    };
                }


                using (var api = new CustodianAPI.CustodianEverywhereAPISoapClient())
                {
                    var request = await api.SubmitPaymentRecordAsync(GlobalConstant.merchant_id, GlobalConstant.password, post.policy_number, post.subsidiary.ToString(), post.payment_narrtn,
                        DateTime.Now, DateTime.Now, post.reference_no, "",
                        "", "", "", "", "", "", "", "", post.biz_unit, post.premium, 0, "API", "RW");
                    if (string.IsNullOrEmpty(request.Passing_Payment_PostSourceResult) || request.Passing_Payment_PostSourceResult != "1")
                    {
                        log.Info($"Something went wrong while processing your transaction search(PostTransaction)  {post.policy_number}");
                        return new policy_data
                        {
                            status = 409,
                            message = "Something went wrong while processing your transaction"
                        };
                    }
                    //http://41.216.175.114/WebPortal/Receipt.aspx?mUser=CUST_WEB&mCert={}&mCert2={}
                    var url = ConfigurationManager.AppSettings["RecieptBaseUrl"];
                    return new policy_data
                    {
                        status = 200,
                        message = "Transaction was successful",
                        data = new Dictionary<string, string>
                        {
                            {"RecieptUrl",url+$"Receipt.aspx?mUser=CUST_WEB&mCert={post.reference_no}&mCert2={post.reference_no}" }
                        }
                    };
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new policy_data
                {
                    status = 404,
                    message = "system malfunction"
                };
            }
        }

    }
}
