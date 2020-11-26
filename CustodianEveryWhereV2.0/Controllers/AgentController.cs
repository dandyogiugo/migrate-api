using CustodianEmailSMSGateway.SMS;
using DataStore.ExtensionMethods;
using DataStore.Models;
using DataStore.repository;
using DataStore.Utilities;
using DataStore.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CustodianEveryWhereV2._0.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
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
                if (!ModelState.IsValid && !pol_detials.use_vehicle_reg_only)
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
                        message = "Authorisation failed"
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
                var merchantConfig = await _apiconfig.FindOneByCriteria(x => x.merchant_id == pol_detials.merchant_id);
                //check if request is from GT Bank and apply sha512
                if (merchantConfig != null && merchantConfig.merchant_name == GlobalConstant.GTBANK)
                {
                    if (string.IsNullOrEmpty(pol_detials.checksum))
                    {
                        log.Info($"Checksum is required {pol_detials.policy_number}");
                        return new policy_data
                        {
                            status = 405,
                            message = "Checksum is required for this merchant"
                        };
                    }

                    var computedhash = await util.Sha512(merchantConfig.merchant_id + pol_detials.policy_number + merchantConfig.secret_key);
                    if (!await util.ValidateGTBankUsers(pol_detials.checksum, computedhash))
                    {
                        log.Info($"Invalid hash for {pol_detials.policy_number}");
                        return new policy_data
                        {
                            status = 403,
                            message = "Invalid checksum"
                        };
                    }
                }

                if (!pol_detials.use_vehicle_reg_only)
                {
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

                        if (pol_detials.vehicle_regs != null && pol_detials.vehicle_regs.Count() > 0)
                        {
                            var motoRequest = api.GetMotorPolicyDetails(GlobalConstant.merchant_id, GlobalConstant.password, pol_detials.policy_number);
                            if (motoRequest.Length == 0)
                            {
                                return new policy_data
                                {
                                    status = 202,
                                    message = $"Unable to fetch vehicles for {pol_detials.policy_number}"
                                };
                            }
                            var filter = motoRequest.Where(x => pol_detials.vehicle_regs.Any(y => y.ToUpper().RemoveWhiteSpaces() == x.mVehReg?.Trim()?.ToUpper()?.RemoveWhiteSpaces())).ToList();
                            var transposed = filter.Select(x => new
                            {
                                RegNumber = x.mVehReg?.Trim(),
                                ChasisNumber = x.mChasisNum?.Trim(),
                                EngineNumber = x.mENGINENUM?.Trim(),
                                ExpiryDate = Convert.ToDateTime(x.mEnddate?.Trim()).ToString("dd-MMM-yyyy"),
                                StartDate = Convert.ToDateTime(x.mStartdate?.Trim()).ToString("dd-MMM-yyyy"),
                                Color = x.mVEHCOLOR?.Trim(),
                                Make = x.mVEHMAKE?.Trim(),
                                Premium = Convert.ToDouble(x.mVEHPREMIUM?.Trim()),
                                Value = Convert.ToDouble(x.mVEHVALUE?.Trim()),
                                InsuredName = x.mInsuredname?.Trim(),
                                Status = x.Status?.Trim(),
                                EngineCapacity = x.mHPCAPACITY?.Trim()

                            }).ToList();

                            return new policy_data
                            {
                                status = 200,
                                message = "policy number is valid",
                                data = new TransPoseGetPolicyDetails
                                {
                                    agenctNameField = request.AgenctName?.Trim(),
                                    bizBranchField = request.BizBranch?.Trim(),
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
                                    insuredEmailField = (request.InsuredEmail?.Trim() == null || string.IsNullOrEmpty(request.InsuredEmail?.Trim()) || request.InsuredEmail?.Trim() == "NULL") ? $"{Guid.NewGuid().ToString().Split('-')[0]}@gmail.com" : request.InsuredEmail?.Trim(),
                                    insuredNameField = request.InsuredName?.Trim(),
                                    insuredNumField = request.InsuredNum?.Trim(),
                                    insuredOthNameField = request.InsuredOthName?.Trim(),
                                    insuredTelNumField = request.InsuredTelNum?.Trim(),
                                    policyEBusinessField = request.PolicyEBusiness?.Trim(),
                                    policyNoField = request.PolicyNo?.Trim(),
                                    startdateField = request.Startdate,
                                    sumInsField = request.SumIns,
                                    telNumField = request.TelNum?.Trim()
                                },
                                hash = new
                                {
                                    checksum = await util.Sha512(merchantConfig.merchant_id + request.SumIns + request.PolicyNo?.Trim() + merchantConfig.secret_key),
                                    message = $"checksum created on {DateTime.Now}"
                                },
                                vehiclelist = transposed

                            };
                        }
                        else
                        {
                            #region - commented out
                            //dynamic transposed = null;
                            //if (pol_detials.subsidiary == subsidiary.General)
                            //{
                            //    var motoRequest = api.GetMotorPolicyDetails(GlobalConstant.merchant_id, GlobalConstant.password, pol_detials.policy_number);
                            //    if (motoRequest.Length == 0)
                            //    {
                            //        return new policy_data
                            //        {
                            //            status = 202,
                            //            message = $"Unable to fetch vehicles for {pol_detials.policy_number}"
                            //        };
                            //    }

                            //    transposed = motoRequest.Select(x => new
                            //    {
                            //        RegNumber = x.mVehReg?.Trim(),
                            //        ChasisNumber = x.mChasisNum?.Trim(),
                            //        EngineNumber = x.mENGINENUM?.Trim(),
                            //        ExpiryDate = Convert.ToDateTime(x.mEnddate?.Trim()).ToString("dd-MMM-yyyy"),
                            //        StartDate = Convert.ToDateTime(x.mStartdate?.Trim()).ToString("dd-MMM-yyyy"),
                            //        Color = x.mVEHCOLOR?.Trim(),
                            //        Make = x.mVEHMAKE?.Trim(),
                            //        Premium = Convert.ToDouble(x.mVEHPREMIUM?.Trim()),
                            //        Value = Convert.ToDouble(x.mVEHVALUE?.Trim()),
                            //        InsuredName = x.mInsuredname?.Trim(),
                            //        Status = x.Status?.Trim(),
                            //        EngineCapacity = x.mHPCAPACITY?.Trim()
                            //    }).ToList();
                            //}

                            #endregion

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
                                    insuredEmailField = (request.InsuredEmail?.Trim() == null || string.IsNullOrEmpty(request.InsuredEmail?.Trim()) || request.InsuredEmail?.Trim() == "NULL") ? $"{Guid.NewGuid().ToString().Split('-')[0]}@gmail.com" : request.InsuredEmail?.Trim(),
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
                                },
                                hash = new
                                {
                                    checksum = await util.Sha512(merchantConfig.merchant_id + request.SumIns + request.PolicyNo?.Trim() + merchantConfig.secret_key),
                                    message = $"checksum created on {DateTime.Now}"
                                },

                            };
                        }
                    }
                }
                else
                {
                    if (pol_detials.vehicle_regs.Count() > 1)
                    {
                        return new policy_data
                        {
                            status = 402,
                            message = "Only one vehicle reg is expected when 'use_vehicle_reg_only' is set to true"
                        };
                    }
                    using (var api = new CustodianAPI.PolicyServicesSoapClient())
                    {
                        var motoRequest = api.GetMotorDetailsByRegNum(GlobalConstant.merchant_id, GlobalConstant.password, pol_detials.vehicle_regs[0]);
                        if (motoRequest.Status != "200")
                        {
                            return new policy_data
                            {
                                status = 402,
                                message = $"Unable to fetech vehicle information for Reg '{pol_detials.vehicle_regs[0]}'"
                            };
                        }
                        var request2 = api.GetMorePolicyDetails(GlobalConstant.merchant_id, GlobalConstant.password, pol_detials.subsidiary.ToString(), motoRequest.mPolicyNumber?.Trim());
                        log.Info($"raw api response business unit  {Newtonsoft.Json.JsonConvert.SerializeObject(request2)}");
                        var transposed = new
                        {
                            RegNumber = motoRequest.mVehReg?.Trim(),
                            ChasisNumber = motoRequest.mChasisNum?.Trim(),
                            EngineNumber = motoRequest.mENGINENUM?.Trim(),
                            ExpiryDate = Convert.ToDateTime(motoRequest.mEnddate?.Trim()).ToString("dd-MMM-yyyy"),
                            StartDate = Convert.ToDateTime(motoRequest.mStartdate?.Trim()).ToString("dd-MMM-yyyy"),
                            Color = motoRequest.mVEHCOLOR?.Trim(),
                            Make = motoRequest.mVEHMAKE?.Trim(),
                            Premium = Convert.ToDouble(motoRequest.mVEHPREMIUM?.Trim()),
                            Value = Convert.ToDouble(motoRequest.mVEHVALUE?.Trim()),
                            InsuredName = motoRequest.mInsuredname?.Trim(),
                            Status = motoRequest.Status?.Trim(),
                            EngineCapacity = motoRequest.mHPCAPACITY?.Trim(),
                            PolicyNumber = motoRequest.mPolicyNumber?.Trim(),
                            BusinessUnit = request2?.BizUnit?.Trim(),
                            Email = request2?.InsuredEmail?.Trim(),
                            PhoneNumber = request2?.InsuredNum?.Trim()
                        };

                        return new policy_data
                        {
                            status = 200,
                            message = "Vehicle reg is valid",
                            data = transposed
                        };
                    }
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

                var merchantConfig = await _apiconfig.FindOneByCriteria(x => x.merchant_id == post.merchant_id);
                //check if request is from GT Bank and apply sha512

                string channelName = "PAYSTACK";
                channelName = (merchantConfig.merchant_name.ToLower().Contains("agent")) ? "MPOS" : "PAYSTACK";
                if (merchantConfig != null && merchantConfig.merchant_name == GlobalConstant.GTBANK)
                {
                    if (string.IsNullOrEmpty(post.checksum))
                    {
                        log.Info($"Checksum is required {post.policy_number}");
                        return new policy_data
                        {
                            status = 405,
                            message = "Checksum is required for this merchant"
                        };
                    }
                    var computedhash = await util.Sha512(merchantConfig.merchant_id + post.policy_number + post.premium + post.reference_no + merchantConfig.secret_key);
                    if (!await util.ValidateGTBankUsers(post.checksum, computedhash))
                    {
                        log.Info($"Invalid hash for {post.policy_number}");
                        return new policy_data
                        {
                            status = 403,
                            message = "Invalid checksum"
                        };
                    }
                    channelName = merchantConfig.merchant_name.ToUpper();
                }

                if (!merchantConfig.merchant_name.ToLower().Contains("adapt"))
                {
                    channelName = merchantConfig.merchant_name.ToUpper();
                }

                var validate_headers = await util.ValidateHeaders(headerValues, post.merchant_id);

                if (!validate_headers)
                {
                    log.Info($"Authorization failed feature for policy search(PostTransaction)  {post.policy_number}");
                    return new policy_data
                    {
                        status = 407,
                        message = "Authorisation failed"
                    };
                }
                //NumberFormatInfo setPrecision = new NumberFormatInfo();
                //setPrecision.NumberDecimalDigits = 1;
                var new_trans = new AgentTransactionLogs
                {
                    biz_unit = post.biz_unit,
                    createdat = DateTime.Now,
                    description = post.description,
                    policy_number = post.policy_number.ToUpper(),
                    premium = post.premium,
                    reference_no = post.reference_no,
                    status = !string.IsNullOrEmpty(post.payment_narrtn) ? post.payment_narrtn : post.status,
                    subsidiary = ((subsidiary)post.subsidiary).ToString(),
                    email_address = post.email_address,
                    issured_name = post.issured_name,
                    phone_no = post.phone_no,
                    merchant_id = post.merchant_id,
                    vehicle_reg_no = post.vehicle_reg_no ?? ""
                };

                if (post.payment_narrtn?.ToLower() == "failed" || post.status?.ToLower() == "failed")
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
                    var request = await api.SubmitPaymentRecordAsync(GlobalConstant.merchant_id, GlobalConstant.password, post.policy_number,
                        post.subsidiary.ToString(), post.payment_narrtn ?? new_trans.description, DateTime.Now,
                        DateTime.Now, post.reference_no, new_trans.issured_name, "", "", "", new_trans.phone_no, new_trans.email_address,
                        "", "", "", post.biz_unit, post.premium, 0, channelName, "RW", "", new_trans.vehicle_reg_no ?? "");
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
                        if (!GlobalConstant.IsDemoMode)
                        {
                            await sms.Send_SMS(message, phone);
                        }
                    }

                    return new policy_data
                    {
                        status = 200,
                        message = "Transaction was successful",
                        data = new Dictionary<string, string>
                        {
                            {"RecieptUrl",url+$"Receipt.aspx?mUser=CUST_WEB&mCert={post.reference_no}&mCert2={post.reference_no}" },
                            {"message",$"Notification of payment sent to customer mobile number ({(GlobalConstant.IsDemoMode?"Message are not sent in demo mode":"")})" }
                        },
                        hash = new
                        {
                            checksum = await util.Sha512(merchantConfig.merchant_id + post.reference_no + post.policy_number + post.premium + merchantConfig.secret_key),
                            message = $"checksum created on {DateTime.Now}"
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

        [HttpGet]
        public async Task<policy_data> GetTransactionWithReferenceNumber(string reference_no, string merchant_id, string checksum)
        {
            try
            {
                var headerValues = HttpContext.Current.Request.Headers.Get("Authorization");

                if (string.IsNullOrEmpty(headerValues))
                {
                    log.Info($"Authorization denied for policy GetTransaction()  {reference_no}");
                    return new policy_data
                    {
                        status = 205,
                        message = "Authorization denied"
                    };
                }

                var merchantConfig = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id);
                //check if request is from GT Bank and apply sha512
                if (merchantConfig != null && merchantConfig.merchant_name == GlobalConstant.GTBANK)
                {
                    if (string.IsNullOrEmpty(checksum))
                    {
                        log.Info($"Checksum is required {reference_no}");
                        return new policy_data
                        {
                            status = 405,
                            message = "Checksum is required for this merchant"
                        };
                    }
                    var computedhash = await util.Sha512(merchant_id + reference_no + merchantConfig.secret_key);
                    if (!await util.ValidateGTBankUsers(checksum, computedhash))
                    {
                        log.Info($"Invalid hash for {reference_no}");
                        return new policy_data
                        {
                            status = 403,
                            message = "Invalid checksum"
                        };
                    }
                }

                var validate_headers = await util.ValidateHeaders(headerValues, merchant_id);

                if (!validate_headers)
                {
                    log.Info($"Authorization failed feature for policy GetTransactionWithReferenceNumber(PostTransaction)  {reference_no}");
                    return new policy_data
                    {
                        status = 407,
                        message = "Authorizatikon failed"
                    };
                }

                var getTransaction = await trans_logs.FindOneByCriteria(x => x.reference_no?.Trim().ToLower() == reference_no?.Trim().ToLower() && x.merchant_id?.ToLower() == merchant_id?.Trim().ToLower());
                if (getTransaction == null)
                {
                    log.Info($"Transaction not found with reference {reference_no}");
                    return new policy_data
                    {
                        status = 303,
                        message = "Transaction not found",
                        data = new
                        {
                            message = $"Transaction not found with reference number '({reference_no})'"
                        }
                    };
                }
                else
                {
                    return new policy_data
                    {
                        status = 200,
                        message = "Transaction found",
                        data = new
                        {
                            merchantName = merchantConfig.merchant_name,
                            policyNumber = getTransaction.policy_number,
                            subsidiary = getTransaction.subsidiary,
                            businessUnit = getTransaction.biz_unit,
                            referenceNumber = getTransaction.reference_no,
                            status = getTransaction.status,
                            description = getTransaction.description,
                            amountPaid = getTransaction.premium,
                            transactionDateTime = getTransaction.createdat,
                            insuredName = getTransaction.issured_name,
                            emailAddress = getTransaction.email_address
                        },
                        hash = new
                        {
                            checksum = await util.Sha512(merchantConfig.merchant_id + getTransaction.reference_no + getTransaction.policy_number + merchantConfig.secret_key),
                            message = $"checksum created on {DateTime.Now}"
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
