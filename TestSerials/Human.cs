using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSerials
{
    class Human
    {
    }
    public class Kehinde
    {
        private int Age { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public string Gender { get; set; }

        public string Address(string Location)
        {
            return Location;
        }
    }
}
