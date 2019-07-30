using DataStore.Models;
using DataStore.repository;
using Newtonsoft.Json;
using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WindowsService1.NewsAPIJob;
using DapperLayer.Dapper.Core;
using DapperLayer.utilities;

namespace TestSerials
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var test = new Core<dynamic>();
                var l = test.GetAllbyPagination(0, 20, connectionManager.sp_getall).GetAwaiter().GetResult();

                //var str = "2 3 1 4";
                //var arr = str.Split(' ').Select(x => Convert.ToInt32(x)).ToList();
                //List<int> newlist = new List<int>();
                //for (int i = 0; i < arr.Count(); i++)
                //{
                //    for (int k = 1 + i; k < arr.Count(); k++)
                //    {
                //        newlist.Add(arr[i] * arr[k]);
                //    }
                //    //  arr.RemoveAt(i + 1);
                //}

                //int min = newlist.Min();

                //int T = Convert.ToInt32(Console.ReadLine());
                //string cases = Console.ReadLine();
                //string[] all = cases.Split(' ');

                //if (T >= 1 && T <= 10)
                //{
                //    int count_test = all.Length;
                //    for (int i = 0; i < count_test; ++i)
                //    {
                //        int item = Convert.ToInt32(all[0]);
                //        for (int k = 1; k <= item; ++k)
                //        {
                //            if (k % 3 == 0)
                //            {
                //                Console.WriteLine("Fizz");
                //            }
                //            else if (k % 5 == 0)
                //            {
                //                Console.WriteLine("Buzz");
                //            }
                //            else if ((k % 3 == 0) && (k % 5 == 0))
                //            {
                //                Console.WriteLine("FizzBuzz");
                //            }
                //            else
                //            {
                //                Console.WriteLine("{0}", k);
                //            }
                //        }
                //    }
                //}




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
                //string username = "Aladdin";
                //string password = "openSesame";

                //byte[] concatenated = System.Text.ASCIIEncoding.ASCII.GetBytes(username + ":" + password);
                //string header = System.Convert.ToBase64String(concatenated);
                //HttpClient client = new HttpClient();
                //client.BaseAddress = new Uri("https://{base_url}/");
                //client.DefaultRequestHeaders.Accept.Clear();
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "QWxhZGRpbjpvcGVuIHNlc2FtZQ==");
                //var request = new MultipartFormDataContent();
                //request.Add(new StringContent("Jane Doe <jane.doe@mail.custodianplc.com.ng>"), "from");
                //request.Add(new StringContent("john.smith@somedomain.com"), "to");
                //request.Add(new StringContent("Mail subject text"), "subject");
                //request.Add(new StringContent("Rich HTML message body."), "text");
                //var response = client.PostAsync("email/1/send", request).Result;
                //if (response.IsSuccessStatusCode)
                //{
                //    var responseContent = response.Content;
                //    string responseString = responseContent.ReadAsStringAsync().Result;
                //    Console.WriteLine(responseString);
                //}

                //var payload = System.IO.File.ReadAllText(@"C:\Users\OItaba\Desktop\MF'B\payload.txt");
                //using (var api = new TravelApi.wsLowFarePlusSoapClient())
                //{

                //    var result = api.wmLowFarePlusXml(payload);
                //    XmlDocument doc = new XmlDocument();
                //    doc.LoadXml(result);

                //    string jsonText = JsonConvert.SerializeXmlNode(doc).Replace("@","_");
                //    var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonText);
                //    var test = obj.OTA_AirLowFareSearchPlusRS._Version.ToString();

                //}
                //var mealPlan = new store<MealPlan>();
                //List<MealPlan> plan = mealPlan.FindMany(x => x.target == "WeightGain" && x.preference == "Poultry").GetAwaiter().GetResult();
                //var group_plan = plan.GroupBy(x => x.daysOfWeek);
                //var final = new List<object>();
                //foreach (var item in group_plan)
                //{
                //    var dic = new Dictionary<string, Dictionary<string, List<temp>>>();
                //    var list_meal = new List<temp>();
                //    var meal = item.GroupBy(x => x.mealType);
                //    var day = new Dictionary<string, List<temp>>();
                //    foreach (var subitem in meal)
                //    {
                //        day.Add(subitem.First().mealType, subitem.Select(x => new temp
                //        {
                //            food = x.food,
                //            quantity = x.quantity,
                //            time = x.time,
                //            youtubeurl = x.youTubeUrl
                //        }).ToList());
                //    }

                //    dic.Add(item.First().daysOfWeek, day);
                //    final.Add(dic);
                //}
                //var net = Newtonsoft.Json.JsonConvert.SerializeObject(final);

                //int A = 10;
                //int B = 20;
                //decimal test = Convert.ToDecimal(98)/ Convert.ToDecimal(10);
                //decimal b = Math.Ceiling(test);
                //double b = Math.Floor(Math.Sqrt(B));
                //List<int> arry = new List<int>();
                //if(A >= 2 && B <= 1000000000)
                //{
                //    for (double i = a; i <= b; ++i)
                //    {
                //        double sqr = Math.Pow(i, Convert.ToDouble(2));
                //        if (sqr >= A && sqr <= B)
                //        {
                //            //double sqr = Math.Sqrt(sqrt);
                //           int count =  Cal(sqr, 0);
                //            arry.Add(count);
                //        }
                //    }

                //    return arry.Min.Max();
                //}

                // int value = 8;
                //List<int> binary = Convert.ToString(161, 2).ToCharArray().Select(x => Convert.ToInt32(x)).ToList();
                //int count = 0;
                //List<int> index = new List<int>();
                //int i = 0;
                //foreach (var item in binary)
                //{
                //    if(item % 2 != 0)
                //    {
                //        count++;
                //        index.Add(i + 1);
                //    }
                //    ++i;
                //}
                //index.Insert(0, count);
                //NewsProcessor.GetNews();

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public static int Cal(double sqrt, int count)
        {
            double sqr = Math.Sqrt(sqrt);
            if (sqr - Math.Floor(sqr) == 0)
            {
                ++count;
                return Cal(sqr, count);
            }
            return count;

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

    public class temp
    {
        public temp()
        {

        }
        public string food { get; set; }
        public string quantity { get; set; }
        public string time { get; set; }
        public string youtubeurl { get; set; }
    }
}
