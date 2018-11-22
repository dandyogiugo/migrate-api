using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSerials
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (OracleConnection cn = new OracleConnection("Data Source=TESTDB;User id=TQ_LMS; Password=TQ_LMS; enlist=false; pooling=false"))
                {
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = cn;
                    cn.Open();
                    cmd.CommandText = "cust_max_mgt.create_claim";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("p_policy_no", OracleDbType.Varchar2).Value = "170200001709";
                    cmd.Parameters.Add("p_type_code", OracleDbType.Varchar2).Value = "DTH";
                    cmd.Parameters.Add("v_data", OracleDbType.Varchar2, 300).Direction = ParameterDirection.Output;
                    cmd.ExecuteNonQuery();
                    string resposne = cmd.Parameters["v_data"].Value.ToString();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public static string Serials(int val)
        {
            string final = "";
            if (val <= 9)
            {
                final = "000000" + val;
            }
            else if (val.ToString().Length < 7)
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

            return "HO/A/07/T" + final;
        }
    }
}
