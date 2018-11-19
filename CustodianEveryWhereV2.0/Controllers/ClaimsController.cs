using DataStore.Models;
using DataStore.repository;
using DataStore.Utilities;
using DataStore.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                    status = true
                };
                if (claims.documents != null && claims.documents.Count() > 0)
                {
                    List<LifeDocument> docs = new List<LifeDocument>();
                    foreach (var item in claims.documents)
                    {
                        var newdocs = new LifeDocument
                        {
                            data = Convert.FromBase64String(item.data),
                            extension = item.extension,
                            name = item.name
                        };

                        docs.Add(newdocs);
                    }

                    save_claims.LifeDocument = docs;
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

        public async Task<claims_response> ProcessNonLifeClaims(ViewModelNonLife claims)
        {
            try
            {
                log.Info("about to validate request params for ProcessNonLifeClaims()");
                // log.Info(Newtonsoft.Json.JsonConvert.SerializeObject(claims));
                if (!ModelState.IsValid)
                {
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

                };

                if (claims.documents != null && claims.documents.Count() > 0)
                {
                    List<NonLifeClaimsDocument> docs = new List<NonLifeClaimsDocument>();
                    foreach (var item in claims.documents)
                    {
                        var newdocs = new NonLifeClaimsDocument
                        {
                            data = Convert.FromBase64String(item.data),
                            extension = item.extension,
                            name = item.name
                        };

                        docs.Add(newdocs);
                    }

                    newClaims.NonLifeDocument = docs;
                }

                await __nonlifeclaim.Save(newClaims);
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
                log.Error(ex.InnerException);
                return new claims_response
                {
                    status = 404,
                    message = "system malfunction"
                };
            }
        }
    }
}
