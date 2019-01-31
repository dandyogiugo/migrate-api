using DataStore.Models;
using DataStore.repository;
using DataStore.Utilities;
using DataStore.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace CustodianEveryWhereV2._0.Controllers
{
    public class ClaimsController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private store<LifeClaims> _claims = null;
        private Utility util = null;
        private store<GeneralClaims> __nonlifeclaim = null;
        public ClaimsController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            _claims = new store<LifeClaims>();
            __nonlifeclaim = new store<GeneralClaims>();
        }

        [HttpPost]
        public async Task<claims_response> ProcessLifeClaims(Life_Claims claims)
        {
            try
            {
                log.Info("about to validate request params for ProcessClaims()");
                //log.Info(Newtonsoft.Json.JsonConvert.SerializeObject(claims));
                if (!ModelState.IsValid)
                {
                    return new claims_response
                    {
                        status = 401,
                        message = "Missing parameters from request"
                    };
                }
                var check_user_function = await util.CheckForAssignedFunction("ProcessLifeClaims", claims.merchant_id);
                if (!check_user_function)
                {
                    return new claims_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }

                //  if (claims.claim_request_type =)
                var claim_code = await util.ClaimCode(claims.claim_request_type);
                string claim_no = string.Empty;
                if (claim_code != "PROCEED")
                {
                    var submit_claim_to_turnquest = await util.SubmitLifeClaims(claims.policy_number.Trim(), claim_code);
                    if (submit_claim_to_turnquest == null)
                    {
                        return new claims_response
                        {
                            status = 204,
                            message = "Claim request could not be submitted"
                        };
                    }
                    else if (submit_claim_to_turnquest.code != 200)
                    {
                        return new claims_response
                        {
                            status = 208,
                            message = submit_claim_to_turnquest.message
                        };

                    }
                    else
                    {
                        claim_no = submit_claim_to_turnquest.claim_no;
                    }
                }

                var save_claims = new LifeClaims
                {
                    updated_at = DateTime.Now,
                    burial_location = claims.burial_location,
                    cause_of_death = claims.cause_of_death,
                    claimant_name = claims.claimant_name,
                    claimant_relationship = claims.claimant_relationship,
                    claim_amount = (claims.claim_amount.HasValue) ? claims.claim_amount.Value : default(decimal),
                    claim_request_type = claims.claim_request_type,
                    created_at = DateTime.Now,
                    email_address = claims.email_address,
                    last_residential_address = claims.last_residential_address,
                    phone_number = claims.phone_number,
                    policy_holder_name = claims.policy_holder_name,
                    policy_number = claims.policy_number,
                    policy_type = claims.policy_type,
                    status = true,
                    Claim_No = claim_no,
                    division = claims.division,
                    BranchCode = claims.branch
                };

                if (claims.documents != null && claims.documents.Count() > 0)
                {
                    List<LifeDocument> docs = new List<LifeDocument>();
                    foreach (var item in claims.documents)
                    {
                        var nameurl = $"{await new Utility().GetSerialNumber()}_{DateTime.Now.ToFileTimeUtc().ToString()}_{save_claims.policy_number.Replace('/', '-')}_{item.name}.{item.extension}";
                        var filepath = $"{ConfigurationManager.AppSettings["DOC_PATH"]}/Documents/Life/{nameurl}";
                        var newdocs = new LifeDocument
                        {
                            path = nameurl,
                            extension = item.extension,
                            name = item.name
                        };
                        docs.Add(newdocs);
                        byte[] content = Convert.FromBase64String(item.data);
                        File.WriteAllBytes(filepath, content);
                    }
                    save_claims.LifeDocument = docs;
                }
                await _claims.Save(save_claims);
                var path = HttpContext.Current.Server.MapPath("~/Cert/claims.html");
                var imagepath = HttpContext.Current.Server.MapPath("~/Images/logo-white.png");
                var template = System.IO.File.ReadAllText(path);
                // claims
                claims.claim_number = claim_no;

                Task.Factory.StartNew(() =>
                {
                    util.SendMail(claims, true, template, imagepath);
                    util.SendMail(claims, false, template, imagepath);
                });

                return new claims_response
                {
                    status = 200,
                    cliams_number = claim_no,
                    message = "Claims was submitted successfully. Thank you"
                };
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                return new claims_response
                {
                    status = 404,
                    message = "system malfunction"
                };
            }
        }

        [HttpPost]
        public async Task<claims_response> ProcessNonLifeClaims(ViewModelNonLife claims)
        {
            try
            {
                log.Info("about to validate request params for ProcessNonLifeClaims()");
                log.Info(Newtonsoft.Json.JsonConvert.SerializeObject(claims));
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
                    return new claims_response
                    {
                        status = 401,
                        message = "Missing parameters from request"
                    };
                }

                var check_user_function = await util.CheckForAssignedFunction("ProcessNonLifeClaims", claims.merchant_id);
                if (!check_user_function)
                {
                    return new claims_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var newClaims = new GeneralClaims
                {
                    claim_amount = claims.claim_amount,
                    claim_request_type = claims.claim_request_type,
                    damage_type = claims.damage_type,
                    data_source = claims.data_source,
                    describe_permanent_disability = claims.describe_permanent_disability,
                    doctor_name = claims.doctor_name,
                    email_address = claims.email_address,
                    fire_service_informed = claims.fire_service_informed,
                    fire_service_report_available = claims.fire_service_report_available,
                    fire_service_station_address = claims.fire_service_station_address,
                    hospital_address = claims.hospital_address,
                    hospital_name = claims.hospital_name,
                    incident_date_time = (claims.incident_date_time.HasValue) ? Convert.ToDateTime(claims.incident_date_time.Value) : default(Nullable<DateTime>),
                    incident_description = claims.incident_description,
                    injury_sustained_description = claims.injury_sustained_description,
                    inspection_location = claims.inspection_location,
                    is_insured_employee = claims.is_insured_employee,
                    last_occupied_date = (claims.last_occupied_date.HasValue) ? Convert.ToDateTime(claims.last_occupied_date.Value) : default(Nullable<DateTime>),
                    list_of_damaged_items = claims.list_of_damaged_items,
                    list_of_items_lost = claims.list_of_items_lost,
                    list_of_stolen_items = claims.list_of_stolen_items,
                    mode_of_conveyance = claims.mode_of_conveyance,
                    permanent_disability = claims.permanent_disability,
                    phone_number = claims.phone_number,
                    police_informed = claims.police_informed,
                    police_report_date_time = (claims.police_report_date_time.HasValue) ? Convert.ToDateTime(claims.police_report_date_time.Value) : default(Nullable<DateTime>),
                    police_station_address = claims.police_station_address,
                    policy_holder_name = claims.policy_holder_name,
                    policy_number = claims.policy_number,
                    policy_type = claims.policy_type,
                    premise_occupied = claims.premise_occupied,
                    taken_to_hospital = claims.taken_to_hospital,
                    thirdparty_involved = claims.thirdparty_involved,
                    third_party_info = claims.third_party_info,
                    vehicle_details = claims.vehicle_details,
                    vehicle_reg_number = claims.vehicle_reg_number,
                    witness_available = claims.witness_available,
                    witness_contact_info = claims.witness_contact_info,
                    witness_name = claims.witness_name,
                    datecreated = DateTime.Now,
                    division = claims.division,
                    BranchCode = claims.branch
                };
                //var merchant_id = ConfigurationManager.AppSettings["Merchant_ID"];
                //var password = ConfigurationManager.AppSettings["Password"];
                string claim_number = "";
                using (var api = new CustodianAPI.CustodianEverywhereAPISoapClient())
                {
                    var submite_claim = await api.SubmitClaimRegisterAsync(GlobalConstant.merchant_id, GlobalConstant.password, newClaims.policy_holder_name, "", newClaims.email_address,
                        newClaims.phone_number, newClaims.policy_number, newClaims.incident_description,
                        newClaims.incident_date_time.Value, newClaims.vehicle_reg_number, newClaims.claim_amount.ToString());
                    log.Info($"Response from Claims api {Newtonsoft.Json.JsonConvert.SerializeObject(submite_claim.Generating_Claims_NumberResult)}");
                    if (submite_claim.Generating_Claims_NumberResult.RegStatusCode != "200")
                    {
                        return new claims_response
                        {
                            status = 409,
                            message = submite_claim.Generating_Claims_NumberResult.RegStatus
                        };
                    }
                    claim_number = submite_claim.Generating_Claims_NumberResult.RegStatus;
                }


                if (claims.documents != null && claims.documents.Count() > 0)
                {

                    List<NonLifeClaimsDocument> docs = new List<NonLifeClaimsDocument>();
                    foreach (var item in claims.documents)
                    {
                        var nameurl = $"{await new Utility().GetSerialNumber()}_{DateTime.Now.ToFileTimeUtc().ToString()}_{newClaims.policy_number.Replace('/', '-')}_{item.name}.{item.extension}";
                        var filepath = $"{ConfigurationManager.AppSettings["DOC_PATH"]}/Documents/General/{nameurl}";
                        var newdocs = new NonLifeClaimsDocument
                        {
                            path = nameurl,
                            extension = item.extension,
                            name = item.name
                        };

                        docs.Add(newdocs);
                        byte[] content = Convert.FromBase64String(item.data);
                        File.WriteAllBytes(filepath, content);
                    }

                    newClaims.NonLifeDocument = docs;
                }
                claims.claims_number = claim_number;
                newClaims.claims_number = claim_number;
                await __nonlifeclaim.Save(newClaims);
                var path = HttpContext.Current.Server.MapPath("~/Cert/claims.html");
                var imagepath = HttpContext.Current.Server.MapPath("~/Images/logo-white.png");
                var template = System.IO.File.ReadAllText(path);
                string email_division = string.Empty;
                var divisionn_obj = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DivisionEmail>>(System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Cert/json.json")));
                if (claims.division.ToUpper() == "BRANCH")
                {
                    email_division = divisionn_obj.FirstOrDefault(x => x.Key.ToUpper() == claims.branch.ToUpper().Trim()).Email;
                }
                else
                {
                    email_division = divisionn_obj.FirstOrDefault(x => x.Code.ToUpper() == claims.division.ToUpper().Trim()).Email;
                }

                Task.Factory.StartNew(() =>
                {
                    util.SendMail(claims, true, template, imagepath, email_division);
                    util.SendMail(claims, false, template, imagepath);
                });

                return new claims_response
                {
                    status = 200,
                    cliams_number = claim_number,
                    message = "Claims was submitted successfully. Thank you"
                };
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new claims_response
                {
                    status = 404,
                    message = "system malfunction"
                };
            }
        }

        [HttpPost]
        public async Task<ClaimsStatus> ClaimsStatus(ClaimsRequest claims)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ClaimsStatus
                    {
                        status = 204,
                        message = "Some parameters missing from request"
                    };
                }
                //check permision
                var check_user_function = await util.CheckForAssignedFunction("ClaimsStatus", claims.merchant_id);
                if (!check_user_function)
                {
                    return new ClaimsStatus
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }

                if (claims.subsidiary.ToLower().Trim() != "general")
                {
                    var get_status_turnquest = await util.CheckClaimStatus(claims.claims_number);
                    if (get_status_turnquest == null)
                    {
                        log.Info($"Could not retrieve claim status {claims.claims_number}");
                        return new ClaimsStatus
                        {
                            status = 206,
                            message = "Claim number is not valid"
                        };
                    }
                    else if (get_status_turnquest.status != 200)
                    {
                        log.Info($"Could not retrieve claim status {claims.claims_number}");
                        return new ClaimsStatus
                        {
                            status = 206,
                            message = "Claim number is not valid"
                        };
                    }
                    else
                    {
                        return new ClaimsStatus
                        {
                            status = 200,
                            message = "Claim status is avaliable",
                            claim_status = get_status_turnquest.claim_status,
                            policy_number = get_status_turnquest.policy_no,

                        };
                    }
                }
                else
                {
                    using (var api = new CustodianAPI.CustodianEverywhereAPISoapClient())
                    {
                        var response = await api.GetClaimStatusAsync(claims.claims_number);
                        log.Info($"response from ABS {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");
                        if (response.ClPolicyNo == "NULL")
                        {
                            log.Info($"Claim number is not valid {claims.claims_number}");
                            return new ClaimsStatus
                            {
                                status = 206,
                                message = "Claim number is not valid"
                            };
                        }
                        else
                        {
                            return new ClaimsStatus
                            {
                                status = 200,
                                message = "Claim status is avaliable",
                                claim_status = response.ClaimStatus,
                                policy_number = response.ClPolicyNo,
                                policy_holder_name = response.InsuredName
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new ClaimsStatus
                {
                    status = 404,
                    message = "system malfunction"
                };
            }
        }

        [HttpPost]
        public async Task<claims_details> GetLifeClaimsDetails(ClaimsDetails details)
        {
            try
            {
                // var properties = details.GetType().GetProperties();
                // log.Info(Newtonsoft.Json.JsonConvert.SerializeObject(details));
                //  log.Info("merchant id: " + (string)properties["merchant_id"]);
                var check_user_function = await util.CheckForAssignedFunction("GetLifeClaimsDetails", details.merchant_id);
                if (!check_user_function)
                {
                    return new claims_details
                    {
                        code = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }

                var getdetails = await util.GetLifeClaimsDetails(details);
                if (getdetails != null && getdetails.code == 200)
                {
                    //return new claims_details
                    //{
                    //    code = 200,
                    //    message = getdetails.message,
                    //    amount = getdetails.amount,
                    //    claim_no = getdetails.claim_no,
                    //    policy_no = details.p_policy_no
                    //};
                    return getdetails;

                }
                else
                {
                    return new claims_details
                    {
                        code = 404,
                        message = "Unable to retrieve claims details"
                    };
                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new claims_details
                {
                    code = 404,
                    message = "system malfunction"
                };
            }
        }
    }
}
