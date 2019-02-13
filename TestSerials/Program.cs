using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
                // using (OracleConnection cn = new OracleConnection("Data Source=TESTDB;User id=TQ_LMS; Password=TQ_LMS; enlist=false; pooling=false"))
                //{
                //OracleCommand cmd = new OracleCommand();
                //cmd.Connection = cn;
                //cn.Open();
                //cmd.CommandText = "cust_max_mgt.create_claim";
                //cmd.CommandType = CommandType.StoredProcedure;
                //cmd.Parameters.Add("p_policy_no", OracleDbType.Varchar2).Value = "170200001709";
                //cmd.Parameters.Add("p_type_code", OracleDbType.Varchar2).Value = "DTH";
                //cmd.Parameters.Add("v_data", OracleDbType.Varchar2, 300).Direction = ParameterDirection.Output;
                //cmd.ExecuteNonQuery();
                //string resposne = cmd.Parameters["v_data"].Value.ToString();

                //OracleCommand cmd = new OracleCommand();
                //cmd.Connection = cn;
                //cn.Open();
                //cmd.CommandText = "cust_max_mgt.get_claim_policy_info";
                //cmd.CommandType = CommandType.StoredProcedure;
                //cmd.Parameters.Add("p_policy_no", OracleDbType.Varchar2).Value = "P/2/FP/000035";
                //cmd.Parameters.Add("p_type", OracleDbType.Varchar2).Value = "CLAIMS";
                //cmd.Parameters.Add("v_data", OracleDbType.Varchar2, 300).Direction = ParameterDirection.Output;
                //cmd.Parameters.Add("p_claim_type", OracleDbType.Varchar2).Value = "DTH";
                //cmd.ExecuteNonQuery();
                //string response = cmd.Parameters["v_data"].Value.ToString();
                //}
                string username = "Aladdin";
                string password = "openSesame";

                byte[] concatenated = System.Text.ASCIIEncoding.ASCII.GetBytes(username + ":" + password);
                string header = System.Convert.ToBase64String(concatenated);


                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://{base_url}/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "QWxhZGRpbjpvcGVuIHNlc2FtZQ==");
                var request = new MultipartFormDataContent();
                request.Add(new StringContent("Jane Doe <jane.doe@mail.custodianplc.com.ng>"), "from");
                request.Add(new StringContent("john.smith@somedomain.com"), "to");
                request.Add(new StringContent("Mail subject text"), "subject");
                request.Add(new StringContent("Rich HTML message body."), "text");
                var response = client.PostAsync("email/1/send", request).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content;
                    string responseString = responseContent.ReadAsStringAsync().Result;
                    Console.WriteLine(responseString);
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
