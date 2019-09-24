using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpSellingAndCrossSelling.Config
{
    public class RequestModel
    {
        public RequestModel()
        {

        }
        public string Gender { get; set; }
        public string Age { get; set; }
        public string Occupation { get; set; }
        public string Premium { get; set; }
    }


    public class DbModels
    {
        public DbModels()
        {

        }

        public string Occupation { get; set; }
        public DateTime Date_of_Birth { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string currentProds { get; set; }
    }
}
