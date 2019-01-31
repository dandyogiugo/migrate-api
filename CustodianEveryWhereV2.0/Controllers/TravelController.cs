using DataStore.Models;
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

namespace CustodianEveryWhereV2._0.Controllers
{
    public class TravelController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private store<TravelInsurance> _buyinsurannce = null;
        public TravelController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            _buyinsurannce = new store<TravelInsurance>();
        }
        /// <summary>
        /// Get quote from custodian every api
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<TravelQuoteResponse> GetTravelQuote(TravelQuote quote)
        {
            try
            {
                // check if parameters are correct
                if (!ModelState.IsValid)
                {
                    return new TravelQuoteResponse
                    {
                        status = 402,
                        message = "Some parameters missing from request"
                    };
                }
                // check if method is assigned to user merchant_id
                var check_user_function = await util.CheckForAssignedFunction("GetTravelQuote", quote.merchant_id);
                if (!check_user_function)
                {
                    return new TravelQuoteResponse
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }
                using (var api = new CustodianAPI.CustodianEverywhereAPISoapClient())
                {
                    var quote_amount = await api.GetTravelQuoteAsync(quote.date_of_birth, quote.departure_date, quote.return_date, quote.zone.ToString().Replace("_", " "));
                    if (!string.IsNullOrEmpty(quote_amount))
                    {
                        decimal amount;
                        var check_if_amount_is_number = decimal.TryParse(quote_amount, out amount);
                        if (check_if_amount_is_number)
                        {
                            return new TravelQuoteResponse
                            {
                                status = 200,
                                message = "Quote computed successfully",
                                quote_amount = quote_amount
                            };
                        }
                        else
                        {
                            return new TravelQuoteResponse
                            {
                                status = 403,
                                message = "Something happend while computing your quote",

                            };
                        }

                    }
                    else
                    {
                        return new TravelQuoteResponse
                        {
                            status = 405,
                            message = "qoute computation was not successful",

                        };
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new TravelQuoteResponse
                {
                    status = 404,
                    message = "System malfunction, try again",

                };
            }
        }
        /// <summary>
        /// Buy Travel Insurance
        /// </summary>
        /// <param name="travel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TravelQuoteResponse> BuyTravelInsurance(BuyTravelInsurance travel)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("BuyTravelInsurance", travel.merchant_id);
                if (!check_user_function)
                {
                    return new TravelQuoteResponse
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature"
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == travel.merchant_id.Trim());
                if (config == null)
                {
                    return new TravelQuoteResponse
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }
                var checkhash = await util.ValidateHash2(travel.premium.ToString() + travel.zone.ToString() + travel.destination, config.secret_key, travel.hash);

                using (var api = new CustodianAPI.CustodianEverywhereAPISoapClient())
                {
                    var request = await api.POSTTravelRecAsync(GlobalConstant.merchant_id,
                        GlobalConstant.password, travel.title, travel.surname, travel.firstname, travel.date_of_birth, travel.gender, travel.nationality,
                        "Int'l PassPort", travel.passport_number, travel.occupation, travel.phone_number,
                        travel.Email, travel.address, travel.zone.ToString().Replace("_", " "), travel.destination, travel.date_of_birth, travel.return_date.Subtract(travel.depature_date).ToString(),
                        travel.purpose_of_trip, travel.depature_date, travel.return_date, travel.premium, "", "", travel.transaction_ref, "API", "", "", "", travel.multiple_destination);
                    if (!string.IsNullOrEmpty(request.Passing_Travel_PostSourceResult))
                    {
                        var save_data = new TravelInsurance
                        {
                            address = travel.address,
                            date_of_birth = travel.date_of_birth,
                            depature_date = travel.depature_date,
                            destination = travel.destination,
                            Email = travel.Email,
                            firstname = travel.firstname,
                            gender = travel.gender,
                            merchant_id = travel.merchant_id,
                            merchant_name = config.merchant_name,
                            multiple_destination = travel.multiple_destination,
                            nationality = travel.nationality,
                            occupation = travel.occupation,
                            passport_number = travel.passport_number,
                            phone_number = travel.phone_number,
                            premium = travel.premium,
                            purpose_of_trip = travel.purpose_of_trip,
                            return_date = travel.return_date,
                            surname = travel.surname,
                            title = travel.title,
                            transaction_ref = travel.transaction_ref,
                            zone = travel.zone.ToString().Replace("_", " ")
                        };



                        var cert_number = request.Passing_Travel_PostSourceResult.Trim().Remove(0, request.Passing_Travel_PostSourceResult.Trim().Length - 7);
                        var nameurl = $"{await new Utility().GetSerialNumber()}_{DateTime.Now.ToFileTimeUtc().ToString()}_{cert_number}.{travel.extension}";
                        var filepath = $"{ConfigurationManager.AppSettings["DOC_PATH"]}/Documents/Travel/{nameurl}";
                        byte[] content = Convert.FromBase64String(travel.attachment);
                        File.WriteAllBytes(filepath, content);


                        save_data.file_path = filepath;
                        save_data.Image_extension_type = travel.extension;


                        await _buyinsurannce.Save(save_data);
                        //http://192.168.10.74/webportal/travelcert.aspx?muser=ebusiness&mcert=0002474&mcert2=0002474
                        return new TravelQuoteResponse
                        {
                            status = 200,
                            message = "Transaction was successful",
                            cert_url = GlobalConstant.Certificate_url + string.Format("muser=ebusiness&mcert={0}&mcert2={1}", cert_number, cert_number)
                        };
                        //muser=ebusiness&mcert={0}&mcert2={1}
                    }
                    else
                    {
                        return new TravelQuoteResponse
                        {
                            status = 403,
                            message = "Transaction processing failed",

                        };
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error((ex.InnerException != null) ? ex.InnerException.ToString() : "");
                return new TravelQuoteResponse
                {
                    status = 404,
                    message = "System malfunction, try again",

                };
            }
        }
    }
}
