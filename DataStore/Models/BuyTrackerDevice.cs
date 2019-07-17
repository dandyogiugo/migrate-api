using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class BuyTrackerDevice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int tracker_type_id { get; set; }
        public string customer_name { get; set; }
        public string customer_email { get; set; }
        public string plate_number { get; set; }
        public DateTime installation_date_time { get; set; }
        public string address { get; set; }
        public string mobile_number { get; set; }
        public string contact_person { get; set; }
        public DateTime date_created { get; set; }
        public string device_description { get; set; }
        public string vehicle_year { get; set; }
        public decimal price { get; set; }
        public decimal annual_subscription { get; set; }
        public string vehicle_make { get; set; }
        public string vehicle_model { get; set; }
    }
}
