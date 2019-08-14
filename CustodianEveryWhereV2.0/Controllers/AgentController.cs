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
    public class AgentController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private store<AgentTransactionLogs> trans_logs = null;
        public AgentController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            trans_logs = new store<AgentTransactionLogs>();
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


                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    var request = api.GetMorePolicyDetails(GlobalConstant.merchant_id, GlobalConstant.password, pol_detials.subsidiary.ToString(), pol_detials.policy_number);
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
                        data = new TransPoseGetPolicyDetails
                        {
                            agenctNameField = request.AgenctName?.Trim(),
                            bizBranchField = request.BizBranch?.Trim(),
                            dOBField = request.DOB,
                            agenctNumField = request.AgenctNum?.Trim(),
                            bizUnitField = request.BizUnit?.Trim(),
                            enddateField = request.Enddate,
                            insAddr1Field = request.InsAddr1?.Trim(),
                            insAddr2Field = request.InsAddr2?.Trim(),
                            insAddr3Field = request.InsAddr3?.Trim(),
                            insLGAField = request.InsLGA?.Trim(),
                            insOccupField = request.InsOccup?.Trim(),
                            insStateField = request.InsState?.Trim(),
                            instPremiumField = request.InstPremium,
                            insuredEmailField = request.InsuredEmail?.Trim(),
                            insuredNameField = request.InsuredName?.Trim(),
                            insuredNumField = request.InsuredNum?.Trim(),
                            insuredOthNameField = request.InsuredOthName?.Trim(),
                            insuredTelNumField = request.InsuredTelNum?.Trim(),
                            mPremiumField = request.mPremium,
                            outPremiumField = request.OutPremium,
                            policyEBusinessField = request.PolicyEBusiness?.Trim(),
                            policyNoField = request.PolicyNo?.Trim(),
                            startdateField = request.Startdate,
                            sumInsField = request.SumIns,
                            telNumField = request.TelNum?.Trim()

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

        [HttpPost]
        public async Task<policy_data> PostTransaction(PostTransaction post)
        {
            try
            {
                log.Info($"Object from page {Newtonsoft.Json.JsonConvert.SerializeObject(post)}");
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

                var new_trans = new AgentTransactionLogs
                {
                    biz_unit = post.biz_unit,
                    createdat = DateTime.Now,
                    description = post.description,
                    policy_number = post.policy_number.ToUpper(),
                    premium = post.premium,
                    reference_no = post.reference_no,
                    status = post.payment_narrtn,
                    subsidiary = ((subsidiary)post.subsidiary).ToString(),
                    email_address = post.email_address,
                    issured_name = post.issured_name,
                    phone_no = post.phone_no

                };

                if (post.payment_narrtn.ToLower() == "failed")
                {
                    await trans_logs.Save(new_trans);
                    log.Info($"Transaction failed dont push to abs");
                    return new policy_data
                    {
                        status = 409,
                        message = post.description
                    };
                }



                using (var api = new CustodianAPI.PolicyServicesSoapClient())
                {
                    var request = await api.SubmitPaymentRecordAsync(GlobalConstant.merchant_id, GlobalConstant.password, post.policy_number, post.subsidiary.ToString(), post.payment_narrtn, DateTime.Now,
                        DateTime.Now, post.reference_no, new_trans.issured_name, "", "", "", new_trans.phone_no, new_trans.email_address, "", "", "", post.biz_unit, post.premium, 0, "MPOS", "RW");
                    log.Info($"raw response from api {request.Passing_Payment_PostSourceResult}");
                    if (string.IsNullOrEmpty(request.Passing_Payment_PostSourceResult) || request.Passing_Payment_PostSourceResult != "1")
                    {
                        log.Info($"Something went wrong while processing your transaction search(PostTransaction)  {post.policy_number}");
                        return new policy_data
                        {
                            status = 409,
                            message = "Something went wrong while processing your transaction"
                        };
                    }
                    await trans_logs.Save(new_trans);
                    //http://41.216.175.114/WebPortal/Receipt.aspx?mUser=CUST_WEB&mCert={}&mCert2={}
                    var url = ConfigurationManager.AppSettings["RecieptBaseUrl"];
                    if (!string.IsNullOrEmpty(post.phone_no))
                    {
                        var phone = post.phone_no.Trim();
                        if (!phone.StartsWith("234"))
                        {
                            phone = "234" + phone.Remove(0, 1);
                        }

                        var sms = new SendSMS();
                        string message = $@"Dear {post.issured_name} We have acknowledged receipt of NGN {post.premium} premium payment.We will apply this to your policy number {post.policy_number.ToUpper()}";
                        await sms.Send_SMS(message, phone);
                    }

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
