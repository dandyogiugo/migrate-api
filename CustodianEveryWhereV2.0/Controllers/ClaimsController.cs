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

namespace CustodianEveryWhereV2._0.Controllers
{
    public class ClaimsController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private store<LifeClaims> _claims = null;
        private Utility util = null;
        public ClaimsController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            _claims = new store<LifeClaims>();
        }

        [HttpPost]
        public async Task<claims_response> ProcessLifeClaims(Life_Claims claims)
        {
            try
            {
                log.Info("about to validate request params for ProcessClaims()");
                log.Info(Newtonsoft.Json.JsonConvert.SerializeObject(claims));
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
                var save_claims = new LifeClaims
                {
                    updated_at = DateTime.Now,
                    // burial_certificate = (!string.IsNullOrEmpty(claims.burial_certificate)) ? Convert.FromBase64String(claims.burial_certificate) : null,
                    burial_location = claims.burial_location,
                    cause_of_death = claims.cause_of_death,
                    claimant_name = claims.claimant_name,
                    claimant_relationship = claims.claimant_relationship,
                    claim_amount = (claims.claim_amount.HasValue) ? claims.claim_amount.Value : default(decimal),
                    claim_request_type = claims.claim_request_type,
                    created_at = DateTime.Now,
                    // doctor_report = (!string.IsNullOrEmpty(claims.doctor_report)) ? Convert.FromBase64String(claims.doctor_report) : null,
                    email_address = claims.email_address,
                    // identification_doc = (!string.IsNullOrEmpty(claims.identification_doc)) ? Convert.FromBase64String(claims.identification_doc) : null,
                    // injury_photographs = (!string.IsNullOrEmpty(claims.injury_photographs)) ? Convert.FromBase64String(claims.injury_photographs) : null,
                    last_residential_address = claims.last_residential_address,
                    // mccd = (!string.IsNullOrEmpty(claims.mccd)) ? Convert.FromBase64String(claims.mccd) : null,
                    // medical_bill = (!string.IsNullOrEmpty(claims.medical_bill)) ? Convert.FromBase64String(claims.medical_bill) : null,
                    // passport_photo = (!string.IsNullOrEmpty(claims.passport_photo)) ? Convert.FromBase64String(claims.passport_photo) : null,
                    phone_number = claims.phone_number,
                    //police_report = (!string.IsNullOrEmpty(claims.police_report)) ? Convert.FromBase64String(claims.police_report) : null,
                    policy_holder_name = claims.policy_holder_name,
                    policy_number = claims.policy_number,
                    policy_type = claims.policy_type,
                    //residency_proof = (!string.IsNullOrEmpty(claims.residency_proof)) ? Convert.FromBase64String(claims.residency_proof) : null,
                    status = true
                };
                List<Extension> ext = new List<Extension>();
                if (claims.documents != null && claims.documents.Count > 0)
                {

                    foreach (var item in claims.documents)
                    {
                        if (item.name == "burial_certificate" && !string.IsNullOrEmpty(item.data) && !string.IsNullOrEmpty(item.extension))
                        {
                            save_claims.burial_certificate = Convert.FromBase64String(item.data);
                            var new_ext = new Extension
                            {
                                extension = item.extension,
                                label = item.name
                            };
                            ext.Add(new_ext);
                        }
                        else if (item.name == "mccd" && !string.IsNullOrEmpty(item.data) && !string.IsNullOrEmpty(item.extension))
                        {
                            save_claims.mccd = Convert.FromBase64String(item.data);
                            var new_ext = new Extension
                            {
                                extension = item.extension,
                                label = item.name
                            };
                            ext.Add(new_ext);
                        }
                        else if (item.name == "police_report" && !string.IsNullOrEmpty(item.data) && !string.IsNullOrEmpty(item.extension))
                        {
                            save_claims.police_report = Convert.FromBase64String(item.data);
                            var new_ext = new Extension
                            {
                                extension = item.extension,
                                label = item.name
                            };
                            ext.Add(new_ext);
                        }
                        else if (item.name == "doctor_report" && !string.IsNullOrEmpty(item.data) && !string.IsNullOrEmpty(item.extension))
                        {
                            save_claims.doctor_report = Convert.FromBase64String(item.data);
                            var new_ext = new Extension
                            {
                                extension = item.extension,
                                label = item.name
                            };
                            ext.Add(new_ext);
                        }
                        else if (item.name == "medical_bill" && !string.IsNullOrEmpty(item.data) && !string.IsNullOrEmpty(item.extension))
                        {
                            save_claims.medical_bill = Convert.FromBase64String(item.data);
                            var new_ext = new Extension
                            {
                                extension = item.extension,
                                label = item.name
                            };
                            ext.Add(new_ext);
                        }
                        else if (item.name == "injury_photographs" && !string.IsNullOrEmpty(item.data) && !string.IsNullOrEmpty(item.extension))
                        {
                            save_claims.injury_photographs = Convert.FromBase64String(item.data);
                            var new_ext = new Extension
                            {
                                extension = item.extension,
                                label = item.name
                            };
                            ext.Add(new_ext);
                        }
                        else if (item.name == "passport_photo" && !string.IsNullOrEmpty(item.data) && !string.IsNullOrEmpty(item.extension))
                        {
                            save_claims.passport_photo = Convert.FromBase64String(item.data);
                            var new_ext = new Extension
                            {
                                extension = item.extension,
                                label = item.name
                            };
                            ext.Add(new_ext);
                        }
                        else if (item.name == "identification_doc" && !string.IsNullOrEmpty(item.data) && !string.IsNullOrEmpty(item.extension))
                        {
                            save_claims.identification_doc = Convert.FromBase64String(item.data);
                            var new_ext = new Extension
                            {
                                extension = item.extension,
                                label = item.name
                            };
                            ext.Add(new_ext);
                        }
                        else if (item.name == "residency_proof" && !string.IsNullOrEmpty(item.data) && !string.IsNullOrEmpty(item.extension))
                        {
                            save_claims.residency_proof = Convert.FromBase64String(item.data);
                            var new_ext = new Extension
                            {
                                extension = item.extension,
                                label = item.name
                            };
                            ext.Add(new_ext);
                        }
                    }

                    save_claims.extension = Newtonsoft.Json.JsonConvert.SerializeObject(ext);
                }

                await _claims.Save(save_claims);
                var path = HttpContext.Current.Server.MapPath("~/Cert/claims.html");
                var imagepath = HttpContext.Current.Server.MapPath("~/Images/logo-white.png");
                var template = System.IO.File.ReadAllText(path);

                Task.Factory.StartNew(() =>
                {
                    util.SendMail(claims, true, template, imagepath);
                    util.SendMail(claims, false, template, imagepath);
                });

                return new claims_response
                {
                    status = 200,
                    cliams_number = "Not Yet Availabe",
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
    }
}
