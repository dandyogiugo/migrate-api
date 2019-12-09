using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.ViewModels
{
    public class Quote
    {
        public Quote()
        {

        }

        [Required]
        public TravelCategory Region { get; set; }
        [Required]
        public DateTime DepartureDate { get; set; }
        [Required]
        public DateTime ReturnDate { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public List<DateTime> DateOfBirth { get; set; }

    }

    public class RateCategory
    {
        public string type { get; set; }
        public double included_rate { get; set; }
        public double? excluded_rate { get; set; }
    }

    public class TravelRate
    {
        public string _class { get; set; }
        public string days { get; set; }
        public List<RateCategory> category { get; set; }
    }



    public class Package
    {
        public string type { get; set; }
        public List<string> values { get; set; }
        public string name { get; set; }
    }

    public class _Category
    {
        public int region { get; set; }
        public List<Package> package { get; set; }
    }

    public class PackageList
    {
        public List<string> benefits { get; set; }
        public List<_Category> category { get; set; }
    }

    public class plans
    {
        public double premium { get; set; }
        public double exchangeRate { get; set; }
        public Package package { get; set; }
        public int travellers { get; set; }
        public dynamic breakDown { get; set; }
    }
}
