using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.ViewModels
{
    public class requests
    {
        public requests()
        {

        }
    }

    public class GetQuote
    {
        public GetQuote()
        {

        }
        [Required]
        public string category { get; set; }
        [Required]
        public decimal value_of_goods { get; set; }
        [Required]
        public string hash { get; set; }
        [Required]
        public string merchant_id { get; set; }
    }

    public class CoverPeriod
    {
        public CoverPeriod()
        {

        }
        [Required]
        public DateTime start_date { get; set; }
        //[Required]
        //[DataType(DataType.DateTime)]
        //public DateTime end_date { get; set; }
    }

    public class Movement
    {
        public Movement()
        {

        }
        [Required]

        public string from { get; set; }
        [Required]

        public string to { get; set; }
    }

    public class BuyGITInsurance
    {
        public BuyGITInsurance()
        {

        }
        [Required]
        public string vehicle_registration_no { get; set; }
        [Required]
        public string insured_name { get; set; }
        [Required]
        public int premium { get; set; }
        [Required]
        public string category { get; set; }
        [Required]
        public int value_of_goods { get; set; }
        [Required]
        public string address { get; set; }
        [Required]
        public string email_address { get; set; }
        [Required]
        public string phone_number { get; set; }
        [Required]
        public string goods_description { get; set; }
        [Required]
        public CoverPeriod cover_period { get; set; }
        [Required]
        public Movement movement { get; set; }
        [Required]
        public string merchant_id { get; set; }
        [Required]
        public string hash { get; set; }
    }

    public class GenerateCert
    {
        public GenerateCert()
        {

        }
        public string policy_no { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string from_date { get; set; }
        public string to_date { get; set; }
        public string vehicle_reg_no { get; set; }
        public string interest { get; set; }
        public string value_of_goods { get; set; }
        public string from_location { get; set; }
        public string to_location { get; set; }
        public string premium { get; set; }
        public string serial_number { get; set; }
        public string email_address { get; set; }
        public string cert_path { get; set; }
    }
    public class Life_Claims
    {
        public Life_Claims()
        {

        }
        [Required]
        public string policy_number { get; set; }
        [Required]
        public string policy_holder_name { get; set; }
        [Required]
        public string policy_type { get; set; }
        [Required]
        public string email_address { get; set; }
        [Required]
        public string phone_number { get; set; }
        [Required]
        public string claim_request_type { get; set; }
        public decimal? claim_amount { get; set; }
        public string cause_of_death { get; set; }
        public string last_residential_address { get; set; }
        public string burial_location { get; set; }
        public string claimant_name { get; set; }
        public string claimant_relationship { get; set; }
        public bool status { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public List<documents> documents { get; set; }
        [Required]
        public string merchant_id { get; set; }
        public string claim_number { get; set; }
        public string division { get; set; }
        public string branch { get; set; }

    }

    public class documents
    {
        public documents()
        {

        }
        public string name { get; set; }
        public string extension { get; set; }
        public string data { get; set; }
    }

    public class Extension
    {
        public Extension()
        {

        }

        public string label { get; set; }
        public string extension { get; set; }
    }

    public class Email
    {
        public Email()
        {

        }

        [DataType(DataType.EmailAddress)]
        [Required]
        public string ToAddress { get; set; }
        [DataType(DataType.Html)]
        [Required]
        public string htmlBody { get; set; }
        [DataType(DataType.Html)]
        public string ExtraHtmlBody { get; set; }
        public List<string> CC { get; set; }
        public List<string> Bcc { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Merchant_Id { get; set; }
        public  List<string> CCUnit { get; set; }
    }


    public class SMS
    {
        public SMS()
        {

        }

        [DataType(DataType.PhoneNumber)]
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        [MaxLength(160)]
        public string Message { get; set; }
        public string Merchant_Id { get; set; }
    }


    public class BuyOneOffGITInsurance
    {
        public BuyOneOffGITInsurance()
        {

        }
        [Required]
        public string vehicle_registration_no { get; set; }
        [Required]
        public string insured_name { get; set; }
        [Required]
        public decimal sum_insured { get; set; }
        [Required]
        public string address { get; set; }
        [Required]
        public string email_address { get; set; }
        [Required]
        public string phone_number { get; set; }
        [Required]
        public string merchant_id { get; set; }
        [Required]
        public string hash { get; set; }
    }

    public class DivisionEmail
    {
        public DivisionEmail()
        {

        }
        public string Key { get; set; }
        public string Code { get; set; }
        public string Email { get; set; }
    }

}


