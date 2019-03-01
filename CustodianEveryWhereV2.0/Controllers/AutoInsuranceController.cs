﻿using DataStore.Models;
using DataStore.repository;
using DataStore.Utilities;
using DataStore.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Serialization;

namespace CustodianEveryWhereV2._0.Controllers
{
    public class AutoInsuranceController : ApiController
    {

        private static Logger log = LogManager.GetCurrentClassLogger();
        private Utility util = null;
        private store<ApiConfiguration> _apiconfig = null;
        private store<AutoInsurance> auto = null;
        public AutoInsuranceController()
        {
            util = new Utility();
            _apiconfig = new store<ApiConfiguration>();
            auto = new store<AutoInsurance>();
        }

        [HttpGet]
        public async Task<res> AutoReg(string regno, string merchant_id)
        {
            try
            {
                if (string.IsNullOrEmpty(regno) || string.IsNullOrEmpty(merchant_id))
                {
                    return new res { message = $"Invalid Registration Number or Merchant Id ({regno})", status = 405 };
                }

                var check_user_function = await util.CheckForAssignedFunction("AutoReg", merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var reg = new AutoReg.VehicleCheckSoapClient();
                var resp = await reg.WS_LicenseInfoByRegNoAsync(regno, "", "", "", "", "", "6492CUS15");
                if (resp.Contains("</LicenseInfo>"))
                {
                    var index = resp.IndexOf("<Response>");
                    var remove = resp.Remove(index, resp.Length - resp.Substring(0, index).Length);
                    XmlSerializer serializer = new XmlSerializer(typeof(LicenseInfo), new XmlRootAttribute("LicenseInfo"));
                    StringReader stringReader = new StringReader(remove);
                    LicenseInfo details = (LicenseInfo)serializer.Deserialize(stringReader);
                    return new res
                    {
                        message = "Registration Number Is Valid",
                        status = 200,
                        data = details
                    };
                }
                else
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(response), new XmlRootAttribute("Response"));
                    StringReader stringReader = new StringReader(resp);
                    response details = (response)serializer.Deserialize(stringReader);
                    return new res
                    {
                        message = details.ResponseMessage,
                        status = 402
                    };
                }
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
        public async Task<res> GetAutoQuote(AutoQuoute quote)
        {
            try
            {
                log.Info($"request from {Newtonsoft.Json.JsonConvert.SerializeObject(quote)}");

                if (!ModelState.IsValid)
                {
                    return new res
                    {
                        status = 409,
                        message = "Some required parameters missing from request"
                    };
                }
                // check if user has access to function
                var check_user_function = await util.CheckForAssignedFunction("GetAutoQuote", quote.merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }

                //check api config
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == quote.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {quote.merchant_id}");
                    return new res
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                // validate hash

                var checkhash = await util.ValidateHash2(quote.vehicle_value + quote.cover_type.ToString() + quote.tracking + quote.excess + quote.srcc + quote.flood, config.secret_key, quote.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {quote.merchant_id}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                using (var api = new CustodianAPI.CustodianEverywhereAPISoapClient())
                {
                    log.Info($"cover type {quote.cover_type.ToString()}");
                    var request = await api.GetMotorQuoteAsync(quote.cover_type.ToString().Replace("_", " "),
                         quote.vehicle_category, quote.vehicle_value.ToString(),
                        !string.IsNullOrEmpty(quote.payment_option) ? quote.payment_option : "",
                        quote.excess, quote.tracking, quote.flood, quote.srcc);
                    log.Info($"Raw quote computed from {quote.merchant_id} is {request}");
                    if (!string.IsNullOrEmpty(request))
                    {
                        log.Info($"quote computed successfully {quote.merchant_id}");
                        decimal amount = 0;
                        var parse_amount = decimal.TryParse(request, out amount);
                        return new res
                        {
                            status = (amount > 0) ? 200 : 407,
                            message = (amount > 0) ? "Quote computed successfully" : "Quote computation error",
                            data = (amount > 0) ? new Dictionary<string, decimal> { { "quote_amount", amount } } : null
                        };
                    }
                    else
                    {
                        return new res { message = "Quote computation not successful", status = 405 };
                    }
                }
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

        public async Task<res> BuyAutoInsurance(Auto Auto)
        {
            try
            {
                log.Info($"request from {Newtonsoft.Json.JsonConvert.SerializeObject(Auto)}");
                if (!ModelState.IsValid)
                {
                    return new res
                    {
                        status = 409,
                        message = "Some required parameters missing from request"
                    };
                }
                // check if user has access to function

                var check_user_function = await util.CheckForAssignedFunction("BuyAutoInsurance", Auto.merchant_id);
                if (!check_user_function)
                {
                    return new res
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }

                //check api config
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == Auto.merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {Auto.merchant_id}");
                    return new res
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                // validate hash
                var checkhash = await util.ValidateHash2(Auto.premium.ToString() + Auto.sum_insured.ToString() + Auto.insurance_type.ToString() + Auto.reference_no, config.secret_key, Auto.hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {Auto.merchant_id}");
                    return new res
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }
                using (var api = new CustodianAPI.CustodianEverywhereAPISoapClient())
                {
                    var request = await api.POSTMotorRecAsync(GlobalConstant.merchant_id, GlobalConstant.password,
                        Auto.customer_name, Auto.address, Auto.phone_number, Auto.email, Auto.engine_number,
                        Auto.insurance_type.ToString().Replace("_", " ").Replace("And", "&"), Auto.premium, Auto.sum_insured
                        , Auto.chassis_number, Auto.registration_number, Auto.vehicle_model,
                        Auto.vehicle_model, Auto.vehicle_color, Auto.vehicle_model, Auto.vehicle_type, Auto.vehicle_year,
                        DateTime.Now, DateTime.Now, DateTime.Now.AddMonths(12), Auto.reference_no, "", "ADAPT", "", "", "");
                    log.Info($"Response from Api {request.Passing_Motor_PostSourceResult}");
                    //HO/V/29/G0000529E|17294
                    if (!string.IsNullOrEmpty(request.Passing_Motor_PostSourceResult) || request.Passing_Motor_PostSourceResult.ToLower() == "success")
                    {

                        var save_new = new AutoInsurance
                        {
                            address = Auto.address,
                            chassis_number = Auto.chassis_number,
                            create_at = DateTime.Now,
                            customer_name = Auto.customer_name,
                            dob = Auto.dob,
                            email = Auto.email,
                            engine_number = Auto.engine_number,
                            extension_type = Auto.extension_type,
                            id_number = Auto.id_number,
                            id_type = Auto.id_type,
                            insurance_type = Auto.insurance_type,
                            occupation = Auto.occupation,
                            phone_number = Auto.phone_number,
                            premium = Auto.premium,
                            reference_no = Auto.reference_no,
                            registration_number = Auto.registration_number,
                            sum_insured = Auto.sum_insured,
                            vehicle_category = Auto.vehicle_category,
                            vehicle_color = Auto.vehicle_color,
                            vehicle_model = Auto.vehicle_model,
                            vehicle_type = Auto.vehicle_type,
                            vehicle_year = Auto.vehicle_year
                        };

                        var cert_code = request.Passing_Motor_PostSourceResult.Replace("**", "|").Split('|')[1];
                        var reciept_base_url = ConfigurationManager.AppSettings["Reciept_Base_Url"];

                        var cert_number = cert_code;
                        var nameurl = $"{await new Utility().GetSerialNumber()}_{DateTime.Now.ToFileTimeUtc().ToString()}_{cert_number}.{Auto.extension_type}";
                        var filepath = $"{ConfigurationManager.AppSettings["DOC_PATH"]}/Documents/Auto/{nameurl}";
                        byte[] content = Convert.FromBase64String(Auto.attachment);
                        File.WriteAllBytes(filepath, content);
                        save_new.attachemt = filepath;
                        save_new.extension_type = Auto.extension_type;

                        await auto.Save(save_new);
                        
                        return new res
                        {
                            status = 200,
                            message = "Transaction was successful",
                            data = new Dictionary<string, string>
                            {
                                {"cert_url",reciept_base_url+$"mUser=CUST_WEB&mCert={cert_code}&mCert2={cert_code}" }
                            }
                        };
                    }
                    else
                    {
                        return new res
                        {
                            status = 308,
                            message = "Transaction was not successful"
                        };
                    }
                }

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
