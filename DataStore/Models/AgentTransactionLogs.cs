using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Models
{
    public class AgentTransactionLogs
    {
        public AgentTransactionLogs()
        {

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [MaxLength(50)]
        public string policy_number { get; set; }
        [MaxLength(20)]
        public string subsidiary { get; set; }
        [MaxLength(100)]
        public string biz_unit { get; set; }
        [MaxLength(100)]
        public string reference_no { get; set; }
        [MaxLength(10)]
        public string status { get; set; }
        [MaxLength(50)]
        public string description { get; set; }
        public decimal premium { get; set; }
        public DateTime createdat { get; set; }
    }
}
