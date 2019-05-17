using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.ViewModels
{

    public class req_response
    {
        public req_response()
        {

        }
        public int status { get; set; }
        public string premium { get; set; }
        public string category { get; set; }
        public decimal value_of_goods { get; set; }
        public string message { get; set; }
        public policy_details policy_details { get; set; }
    }

    public class policy_details
    {
        public policy_details()
        {

        }
        public string policy_number { get; set; }
        public string certificate_no { get; set; }
        public string download_link { get; set; }
    }

    public class claims_response
    {
        public claims_response()
        {

        }
        public int status { get; set; }
        public string message { get; set; }
        public string cliams_number { get; set; }
    }

    public class notification_response
    {
        public notification_response()
        {

        }
        public int status { get; set; }
        public string message { get; set; }
        public string type { get; set; }
        public object data { get; set; }
        public string image_base_url { get; set; }
        public string reciept_url { get; set; }
        public decimal sum_insured { get; set; }
    }

    public class LifeClaimStatus
    {
        public LifeClaimStatus()
        {

        }
        public int status { get; set; }
        public string claim_no { get; set; }
        public int code { get; set; }
        public string message { get; set; }
        public string policy_no { get; set; }
        public string claim_status { get; set; }

    }

    public class ClaimsStatus
    {
        public ClaimsStatus()
        {

        }
        public int status { get; set; }
        public string claim_status { get; set; }
        public string message { get; set; }
        public string policy_number { get; set; }
        public string policy_holder_name { get; set; }
    }

    public class ClaimsRequest
    {
        public ClaimsRequest()
        {

        }
        [Required]
        public string claims_number { get; set; }
        [Required]
        public string subsidiary { get; set; }
        [Required]
        public string merchant_id { get; set; }
    }

    public class Policy
    {
        public Policy()
        {

        }

        public int status { get; set; }
        public string message { get; set; }
        public object details { get; set; }
    }

    public class Wallet
    {
        public Wallet()
        {

        }
        public int status { get; set; }
        public string message { get; set; }
        public List<object> trnx_details { get; set; }
        public decimal wallet_balance { get; set; }
    }

    public class claim_details
    {
        public claim_details()
        {

        }

        public string policy_no { get; set; }
        public string policy_type { get; set; }
        public string claim_type { get; set; }
    }

    public class claims_details
    {
        public claims_details()
        {

        }
        public string status { get; set; }
        public string message { get; set; }
        public string claim_no { get; set; }
        public decimal amount { get; set; }
        public string policy_no { get; set; }
        public int code { get; set; }
    }


    public class ClaimsDetails
    {
        public ClaimsDetails()
        {

        }

        public string p_policy_no { get; set; }
        public string p_type { get; set; }
        public string p_claim_type { get; set; }
        public string merchant_id { get; set; }
    }

    public class TravelQuoteResponse
    {
        public TravelQuoteResponse()
        {

        }
        public int status { get; set; }
        public string message { get; set; }
        public string quote_amount { get; set; }
        public string cert_url { get; set; }
        public List<decimal> quote_list { get; set; }
    }


    public class FlightData
    {
        public FlightData()
        {

        }
        public string AirportCode { get; set; }
        public string AirportName { get; set; }
        public string CityCountry { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }

    public class FlightSearch
    {
        public FlightSearch()
        {

        }

        public int status { get; set; }
        public string message { get; set; }
        public List<FlightData> flight_search { get; set; }
    }

    public class policy_data
    {
        public policy_data()
        {

        }
        public int status { get; set; }
        public string message { get; set; }
        public object data { get; set; }
    }

    public class joke
    {
        public joke()
        {

        }
        public string youtube_url { get; set; }
        public string thumbnail_image { get; set; }
        public string credit { get; set; }
        public string title { get; set; }
    }
}
