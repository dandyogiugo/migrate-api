using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.ViewModels
{

    public class response
    {
        public response()
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
}
