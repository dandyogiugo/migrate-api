using DataStore.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class AutoInsurance
    {
        public AutoInsurance()
        {

        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(200)]
        public string customer_name { get; set; }
        [MaxLength(400)]
        public string address { get; set; }
        [MaxLength(20)]
        public string phone_number { get; set; }
        public string email { get; set; }
        [MaxLength(100)]
        public string engine_number { get; set; }
        public TypeOfCover insurance_type { get; set; }
        public decimal premium { get; set; }
        public decimal sum_insured { get; set; }
        public string chassis_number { get; set; }
        [MaxLength(200)]
        public string registration_number { get; set; }
        [MaxLength(200)]
        public string vehicle_model { get; set; }
        [MaxLength(200)]
        public string vehicle_category { get; set; }
        [MaxLength(200)]
        public string vehicle_color { get; set; }
        [MaxLength(200)]
        public string vehicle_type { get; set; }
        [MaxLength(200)]
        public string vehicle_year { get; set; }
        [MaxLength(200)]
        public string reference_no { get; set; }
        [MaxLength(200)]
        public string id_type { get; set; }
        public string occupation { get; set; }
        [MaxLength(200)]
        public string id_number { get; set; }
        public DateTime create_at { get; set; }
        public DateTime dob { get; set; }
        public string attachemt { get; set; }
        [MaxLength(200)]
        public string extension_type { get; set; }
        
    }
}
