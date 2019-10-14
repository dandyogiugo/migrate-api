using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        public List<string> CCUnit { get; set; }
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

    public class Enquiry
    {
        public Enquiry()
        {

        }

        [Required]
        public string policy_no { get; set; }
        [Required]
        public string merchant_id { get; set; }
    }

    public class TravelQuote
    {
        public TravelQuote()
        {

        }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? date_of_birth { get; set; }
        [Required]
        public Zones zone { get; set; }
        [Required]
        public string destination { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime departure_date { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime return_date { get; set; }
        [Required]
        public string merchant_id { get; set; }

        [DataType(DataType.Date)]
        public List<DateTime> multiple_dob { get; set; }
    }

    public class BuyTravelInsurance
    {
        public BuyTravelInsurance()
        {

        }

        public string title { get; set; }

        public string surname { get; set; }

        public string firstname { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? date_of_birth { get; set; }

        public string gender { get; set; }
        [Required]
        public string nationality { get; set; }

        public string passport_number { get; set; }
        [Required]
        public string occupation { get; set; }
        [Required]
        public string phone_number { get; set; }
        [Required]
        public string address { get; set; }
        [Required]
        public Zones zone { get; set; }
        [Required]
        public string destination { get; set; }
        [Required]
        public string purpose_of_trip { get; set; }


        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime departure_date { get; set; }


        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime return_date { get; set; }


        [Required]
        public decimal premium { get; set; }
        [Required]
        public string transaction_ref { get; set; }
        public string multiple_destination { get; set; }
        [Required]
        public string merchant_id { get; set; }
        [Required]
        public string hash { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [MaxLength(10)]
        public string extension { get; set; }
        public string attachment { get; set; }
        public List<Passenger> Passenger { get; set; }

    }

    public class Passenger
    {
        public string surname { get; set; }
        [MaxLength(50)]
        public string firstname { get; set; }
        [MaxLength(10)]
        public string title { get; set; }
        public DateTime date_of_birth { get; set; }
        [MaxLength(8)]
        public string gender { get; set; }
        [MaxLength(20)]
        public string passport_number { get; set; }
        [MaxLength(50)]
        public string occupation { get; set; }
        [MaxLength(8)]
        public string extension { get; set; }
        public decimal premium { get; set; }
        [MaxLength(400)]
        public string attachment { get; set; }
    }

    public class policydetails
    {
        public policydetails()
        {

        }

        [Required]
        public string policy_number { get; set; }
        [Required]
        public string merchant_id { get; set; }
        [Required]
        public subsidiary subsidiary { get; set; }
        public string checksum { get; set; }

    }

    public class PostTransaction
    {
        public PostTransaction()
        {

        }

        [Required]
        public string policy_number { get; set; }
        [Required]
        public subsidiary subsidiary { get; set; }
        public string payment_narrtn { get; set; }
        [Required]
        public string reference_no { get; set; }
        [Required]
        public string biz_unit { get; set; }
        [Required]
        public decimal premium { get; set; }
        [Required]
        public string merchant_id { get; set; }
        public string description { get; set; }
        public string issured_name { get; set; }
        public string phone_no { get; set; }
        public string email_address { get; set; }
        public string checksum { get; set; }
        public string status { get; set; }
    }


    public class res
    {
        public res()
        {

        }
        public string message { get; set; }
        public int status { get; set; }
        public object data { get; set; }
    }


    public class LicenseInfo
    {
        public LicenseInfo()
        {

        }
        public string ChasisNo { get; set; }
        public string EngineNo { get; set; }
        public string Color { get; set; }
        public string Model { get; set; }
        public string Name { get; set; }
        public string RegistrationNo { get; set; }
        public string VehicleStatus { get; set; }
    }

    public class response
    {
        public response()
        {

        }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public string DV_url { get; set; }
    }


    public class AutoQuoute
    {
        public AutoQuoute()
        {

        }

        [Required]
        public TypeOfCover cover_type { get; set; }
        public string vehicle_category { get; set; }
        [Required]
        public decimal vehicle_value { get; set; }
        public string payment_option { get; set; }
        public string excess { get; set; }
        public string tracking { get; set; }
        public string flood { get; set; }
        public string srcc { get; set; }
        [Required]
        public string merchant_id { get; set; }
        [Required]
        public string hash { get; set; }
    }

    public class Auto
    {
        public Auto()
        {

        }

        [Required]
        public string customer_name { get; set; }
        [Required]
        public string address { get; set; }
        [Required]
        public string phone_number { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public string engine_number { get; set; }
        [Required]
        public TypeOfCover insurance_type { get; set; }
        [Required]
        public decimal premium { get; set; }
        [Required]
        public decimal sum_insured { get; set; }
        [Required]
        public string chassis_number { get; set; }
        [Required]
        public string registration_number { get; set; }
        public string vehicle_model { get; set; }
        public string vehicle_category { get; set; }
        public string vehicle_color { get; set; }
        public string vehicle_type { get; set; }
        public string vehicle_year { get; set; }
        [Required]
        public string hash { get; set; }
        [Required]
        public string merchant_id { get; set; }
        [Required]
        public string reference_no { get; set; }
        [Required]
        public string id_type { get; set; }
        [Required]
        public string occupation { get; set; }
        [Required]
        public string id_number { get; set; }
        public DateTime dob { get; set; }
        [Required]
        public string attachment { get; set; }
        [Required]
        public string extension_type { get; set; }
        public string payment_option { get; set; }
        public string excess { get; set; }
        public string tracking { get; set; }
        public string flood { get; set; }
        public string srcc { get; set; }
        public DateTime? start_date { get; set; }
    }

    public class user_otp
    {
        public user_otp()
        {

        }
        [Required]
        public Platforms platform { get; set; }
        [Required]
        public string mobile { get; set; }
        [Required]
        public string fullname { get; set; }
        [Required]
        public string hash { get; set; }
        [Required]
        public string merchant_id { get; set; }
    }

    public class Rate
    {
        public Rate()
        {

        }

        public string customer_name { get; set; }

    }


    public class deal
    {
        [Required]
        public string firstname { get; set; }
        [Required]
        public string lastname { get; set; }
        [Required]
        public string dob { get; set; }
        [Required]
        public string gender { get; set; }
        [Required]
        public string email { get; set; }
        public string address { get; set; }
        [Required]
        public string mobile { get; set; }
        public string marital_status { get; set; }
        public int membership { get; set; }
        [Required]
        public decimal discounted_price { get; set; }
        [Required]
        public decimal discounted_percent { get; set; }
        public string start_date { get; set; }
        [Required]
        public decimal price { get; set; }
        [Required]
        public string gym { get; set; }
        [Required]
        public int package_id { get; set; }
        [Required]
        public string merchant_id { get; set; }
        [Required]
        public string hash { get; set; }
        [Required]
        public string reference { get; set; }
        public string description { get; set; }
        public string period { get; set; }
        public int duration { get; set; }
    }

    public class _MealPlan
    {
        public _MealPlan()
        {

        }
        public MealPlanCategory target { get; set; }
        public Preference preference { get; set; }
        public string ageRange { get; set; }
        public GivenBirth givenBirth { get; set; }
        public MaritalStatus maritalStatus { get; set; }
        public Gender gender { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }
        public string merchant_id { get; set; }
        public string hash { get; set; }


    }

    public class temp
    {
        public temp()
        {

        }

        public string food { get; set; }
        public string quantity { get; set; }
        public string time { get; set; }
        public string youtubeurl { get; set; }
        public string image_path { get; set; }
    }


    public class UserAuthDetails
    {
        public UserAuthDetails()
        {

        }

        [DataType(DataType.EmailAddress)]
        [MaxLength(100)]
        [Required]
        public string email { get; set; }
        [MaxLength(100)]
        [Required]
        public string fullname { get; set; }
        [MaxLength(200)]
        [Required]
        public string UUID { get; set; }
        [Required]
        public string merchant_id { get; set; }
        [Required]
        public string hash { get; set; }
    }


    public class Pinned_News
    {
        public Pinned_News()
        {

        }

        public string merchant_id { get; set; }
        public string hash { get; set; }
        public dynamic jsonbase64string { get; set; }
        public string email { get; set; }
    }

    public class NewList
    {
        public NewList()
        {

        }
        public int Id { get; set; }
        public dynamic news { get; set; }
    }


    public class BuyTracker
    {
        public int tracker_type_id { get; set; }
        [Required]
        public string customer_name { get; set; }
        [Required]
        public string customer_email { get; set; }
        [Required]
        public string plate_number { get; set; }
        [Required]
        public string installation_date_time { get; set; }
        [Required]
        public string address { get; set; }
        [Required]
        public string mobile_number { get; set; }
        public string contact_person { get; set; }
        [Required]
        public string hash { get; set; }
        [Required]
        public string merchant_id { get; set; }
        public string device_description { get; set; }
        public string vehicle_year { get; set; }
        public decimal price { get; set; }
        public decimal annual_subscription { get; set; }
        public string vehicle_make { get; set; }
        public string vehicle_model { get; set; }
        [Required]
        public string reference { get; set; }
    }


    public class BuyTrackerPost
    {
        public string user_email { get; set; }
        public string user_passcode { get; set; }
        public int tracker_type_id { get; set; }
        public string customer_name { get; set; }
        public string customer_email { get; set; }
        public string plate_number { get; set; }
        public DateTime installation_date_time { get; set; }
        public string address { get; set; }
        public string mobile_number { get; set; }
        public string contact_person { get; set; }
        public string vehicle_year { get; set; }
        public string vehicle_make { get; set; }
        public string vehicle_model { get; set; }

    }

    public class DevicePriceDetails
    {
        public string id { get; set; }
        public string description { get; set; }
        public string features { get; set; }
        public string in_stock { get; set; }
        public string minimum_vehicle_year { get; set; }
        public string maximum_vehicle_year { get; set; }
        public decimal price { get; set; }
        public decimal annual_subscription { get; set; }
        public string active { get; set; }
        public string discount { get; set; }
        public decimal actual_price { get; set; }
        public string label { get; set; }
    }

    public class DevicePricesResponse
    {
        public string response_code { get; set; }
        public string response_message { get; set; }
        public List<DevicePriceDetails> data { get; set; }
        public bool can_buy_comprehensive { get; set; }
    }

    public class SetTeleUser
    {
        public SetTeleUser()
        {

        }

        [Required]
        public string OwnerName { get; set; }
        [Required]
        public Gender Gender { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Newpassword { get; set; }
        [Required]
        public string OTP { get; set; }
        public string LoginLocation { get; set; }
        public string merchant_id { get; set; }
        public string hash { get; set; }
    }

    public class AuthTeleUser
    {
        public AuthTeleUser()
        {

        }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string password { get; set; }
        [Required]
        public string merchant_id { get; set; }
        [Required]
        public string hash { get; set; }
    }

    public class TelemaricResetPassword
    {
        public TelemaricResetPassword()
        {

        }

        [Required]
        public string email { get; set; }
        [Required]
        public string password { get; set; }
        [Required]
        public string merchant_id { get; set; }
        [Required]
        public string hash { get; set; }
        [Required]
        public string OTP { get; set; }
    }



    public class SafetyRequest
    {
        public SafetyRequest()
        {

        }
        [Required]
        public string hash { get; set; }
        [Required]
        public string merchant_id { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public DateTime CustomerDOB { get; set; }
        [Required]
        public string CustomerName { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Occupation { get; set; }
        [Required]
        public decimal Premium { get; set; }
        [Required]
        public int NoOfUnit { get; set; }
        [Required]
        public string Reference { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string BeneficiaryName { get; set; }
        [Required]
        public Gender BeneficiarySex { get; set; }
        [Required]
        public DateTime BeneficiaryDOB { get; set; }
        [Required]
        public string BeneficiaryRelatn { get; set; }
        [Required]
        public string ImageBase64 { get; set; }
        [Required]
        public string ImageFormat { get; set; }
        [Required]
        public string IdentificationType { get; set; }

        [Required]
        public string IdentificationNumber { get; set; }
    }

    public class League
    {
        public int league_id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string country { get; set; }
        public string country_code { get; set; }
        public int season { get; set; }
        public DateTime season_start { get; set; }
        public DateTime season_end { get; set; }
        public string logo { get; set; }
        public string flag { get; set; }
        public bool standings { get; set; }
        public bool is_current { get; set; }
    }

    public class LeagueObject
    {
        [Required]
        public string merchant_id { get; set; }
        [Required]
        public string hash { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string email { get; set; }
        public List<League> leagues { get; set; }
    }

    public class GymLogin
    {
        public GymLogin()
        {

        }
        [Required]
        public string merchant_id { get; set; }
        [Required]
        public string hash { get; set; }
        [Required]
        public string username { get; set; }
        [Required]
        public string password { get; set; }
        [Required]
        public int gym_id { get; set; }
    }


    public class MarkAttendance
    {
        public MarkAttendance()
        {

        }

        [Required]
        public string merchant_id { get; set; }
        [Required]
        public string hash { get; set; }
        [Required]
        public int user_id { get; set; }
        [Required]
        public int gym_id { get; set; }
    }
}


