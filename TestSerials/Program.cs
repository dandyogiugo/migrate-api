using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSerials
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Serials(6));
            Console.WriteLine(Serials(10));
            Console.WriteLine(Serials(566));
            Console.WriteLine(Serials(7000));
            Console.WriteLine(Serials(45678));
            Console.WriteLine(Serials(2345678));
            Console.WriteLine(Serials(4325));
            Console.ReadKey();
        }

        public static string Serials(int val)
        {
            string final = "";
            if (val <= 9)
            {
                final = "000000" + val;
            }
            else if(val.ToString().Length < 7)
            {
                int loop = 7 - val.ToString().Length;
                string zeros = "";
                for (int i = 0; i < loop; i++)
                {
                    zeros += "0";
                }
                final = zeros + val;
            }
            else
            {
                final = val.ToString();
            }

            return "HO/A/07/T"+final;
        }
    }
}
