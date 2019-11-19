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
using System.Dynamic;
using System.Reflection;
using UpSellingAndCrossSelling.CrossSelling;
using DataStore.ViewModels;
using System.Text.RegularExpressions;

namespace TestSerials
{
    class Program
    {
        static void Main(string[] args)
        {
            MatchCollection
            try
            {

                //Core<NextRenewal> dapper_core = new Core<NextRenewal>();
                //var condition = new helpers();
                //var query = condition.QueryResolver(new RenewalRatio
                //{
                //    merchant_id = "Test",
                //    is_MD = true,
                //    subsidary = subsidiary.Life

                //});
                //var result = dapper_core.GetRenewalRatio(string.Format(connectionManager.NexRenewal, query)).GetAwaiter().GetResult();
                //var grouped_item = new helpers().Grouper2(result);
                //var test = Newtonsoft.Json.JsonConvert.SerializeObject(grouped_item);
                //Console.WriteLine(test);
                //NumberFormatInfo setPrecision = new NumberFormatInfo();
                //setPrecision.NumberDecimalDigits = 1;
                //var premium = 5000.00;
                //var format = string.Format("{0:1}", premium);
                //var test = format;
                //GetAllLeague.GetLeague();
                //Console.ReadKey();
                //var t = new CrossSellingEngine();
                // t.EngineProcessor();
                #region
                //var a = new { name = "oscar  ", age = 30, sex = 'M', amount = 345.66, isActive = true };
                //dynamic expando = new ExpandoObject();
                //expando.obj = a;
                //Console.WriteLine(((string)expando.obj.name).Trim());
                //if (expando.obj.amount is decimal)
                //{
                //    Console.WriteLine(true);
                //}
                //else
                //{
                //    Console.WriteLine(false);
                //}
                //Console.ReadKey();
                //int[] arr = { 5, 4, 8, 2, 6, 7, 1, 3, 7 };
                //var test = arr.ToList().OrderBy(x=>x);

                //int sum = 0;
                //for (var i = 0; i < arr.Length; ++i)
                //{
                //    sum += arr[i];
                //}
                //var d = (arr.Length * (arr.Length + 1)) / 2;
                //var dif = d - sum;
                //var dup = arr[dif];
                //var dp = dup;

                //List<int> h = new List<int>() { 4, 5, 67, 8 };
                //h.Min();
                // var test = new Core<dynamic>();
                // var l = test.GetPredictionByCustomerID(100049357, connectionManager.recomendation).GetAwaiter().GetResult();

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
                #endregion

                temp myObj = new temp();

                //var test =  myObj.MapToObject(typeof(Viewtemp));
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<string> popularNFeatures(int numFeatures,
                                        int topFeatures,
                                        List<string> possibleFeatures,
                                        int numFeatureRequests,
                                        List<string> featureRequests)
        {
            // WRITE YOUR CODE HERE
            if (topFeatures > numFeatures) return possibleFeatures;
            Dictionary<string, int> count = new Dictionary<string, int>();
            
            foreach (string pfeatures in possibleFeatures)
            {
                foreach (var item in featureRequests)
                {
                    if (item.Contains(pfeatures))
                    {
                        if (count.ContainsKey(pfeatures))
                        {
                            count[pfeatures] += 1;
                        }
                        else
                        {
                            count.Add(pfeatures, 1);
                        }
                    }
                }

            }
          var final =  count.OrderByDescending(x => x.Value).ToList();
            List<string> f = new List<string>();
            for(int i = 0;i< topFeatures;++i)
            {
                f.Add(final[i].Key);
            }
            return f;
        }
    }

    static string timeConversion(string s)
    {
        List<int[]> sbdjh = new List<int[]>();
        Regex
        /*
         * Write your code here.
         */
        var part = s.Split(':');
        int h = Convert.ToInt32(part[0]);

        if (s.Contains("AM"))
        {

            if (h == 12)
            {
                return $"00:{part[1]}:{part[2].Substring(0, 1)}";
            }
            else
            {
                return $"{h}:{part[1]}:{part[2].Substring(0, 1)}";

            }


        }
        else
        {

            int f = h + 12;
            if (f == 24)
            {
                return $"00:{part[1]}:{part[2].Substring(0, 1)}";
            }
            else
            {
                return $"{f}:{part[1]}:{part[2].Substring(0, 1)}";
            }
        }

    }

    public int get(List<List<int>> arr)
    {
        var M = arr;
        int i, j;
        int count = 0;
        //no of rows in M[,] 
        int R = arr.Count();
        //no of columns in M[,] 
        int C = arr[0].Count();
        int[,] S = new int[R, C];

        int max_of_s, max_i, max_j;

        /* Set first column of S[,]*/
        for (i = 0; i < R; i++)
        {
            S[i, 0] = M[i][0];
        }


        /* Set first row of S[][]*/
        for (j = 0; j < C; j++)
        {
            S[0, j] = M[0][j];
        }

        /* Construct other entries of S[,]*/
        for (i = 1; i < R; i++)
        {
            for (j = 1; j < C; j++)
            {
                if (M[i][j] == 1)
                    S[i, j] = Math.Min(S[i, j - 1],
                            Math.Min(S[i - 1, j], S[i - 1, j - 1])) + 1;
                else
                    S[i, j] = 0;
            }
        }

        max_of_s = S[0, 0]; max_i = 0; max_j = 0;
        for (i = 0; i < R; i++)
        {
            for (j = 0; j < C; j++)
            {
                if (max_of_s < S[i, j])
                {
                    max_of_s = S[i, j];
                    max_i = i;
                    max_j = j;
                }
            }
        }

        for (i = max_i; i > max_i - max_of_s; i--)
        {
            for (j = max_j; j > max_j - max_of_s; j--)
            {
                count += count;
            }

        }
        return count;
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
    public static int kk(int[] A)
    {
        List<int> diff = new List<int>();
        for (int i = 0; i < A.Length; ++i)
        {
            int fpvalue = A[i];
            for (int j = i + 1; j < A.Length; ++j)
            {
                int spvalue = A[j];
                int first;
                int second;
                if (fpvalue > spvalue)
                {
                    first = spvalue + 1;
                    second = fpvalue;
                }
                else
                {
                    first = fpvalue + 1;
                    second = spvalue;
                }
                for (int k = first; k < second; ++k)
                {
                    for (int m = 0; m < A.Length; ++m)
                    {
                        if (A[m] == k)
                        {
                            break;
                        }
                    }


                }
                int abs = Math.Abs(fpvalue - spvalue);
                diff.Add(abs);

            }
        }
        if (diff.Count > 0)
        {
            var min = diff.Min();
            if (min <= 100000000)
            {
                return min;
            }
            return -1;
        }
        else
        {
            return -2;
        }
    }

    (string, object, int) LookupName(long id) // tuple return type
    {
        var first = "";
        var last = "";
        var middle = 60;
        return (first, middle, Convert.ToInt32(last)); // tuple literal
    }

    public static void Closest(int[] arr1, int[] arr2)
    {
        for (int i = 0; i < arr1.Length; i++)
        {
            for (int j = 0; j < arr2.Length; j++)
            {
                int sum = arr1[i] + arr2[j];
                if (sum > 22 && sum <= 25)
                {
                    Console.WriteLine($"({arr1[i]},{arr2[j]}) Sum = {sum}");
                }
                else
                {
                    continue;
                }
            }
        }
    }

    public void PostMe(string firstDate, string lastDate, string weekDay)
    {
        var _firstdate = Convert.ToDateTime(firstDate);
        var _lastdate = Convert.ToDateTime(lastDate);
        var _DaysOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), weekDay);
        List<data> allStocks = new List<data>();
        if (_DaysOfWeek != DayOfWeek.Saturday && _DaysOfWeek != DayOfWeek.Sunday)
        {
            int count = 0;
            while (_firstdate <= _lastdate)
            {
                DateTime query;
                if (count == 0)
                {
                    query = _firstdate;
                }
                else
                {
                    query = _firstdate.AddDays(count);
                    if (query.DayOfWeek != _DaysOfWeek)
                    {
                        ++count;
                        continue;
                    }
                }
                using (var api = new HttpClient())
                {
                    var request = api.GetAsync($"https://jsonmock.hackerrank.com/api/stocks/?date={query.ToString("d-MMMM-yyyy")}").GetAwaiter().GetResult();
                    if (request.IsSuccessStatusCode)
                    {
                        var response = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        var stocks = JsonConvert.DeserializeObject<stock>(response);
                        foreach (var item in stocks.data)
                        {
                            Console.WriteLine($"{item.date.ToString("d-MMMM-yyyy")} {item.open} {item.close}");
                        }

                    }
                }

                ++count;
            }
        }


    }

    public List<data> Pagenated(int pageNumber, DateTime _firstdate, DateTime _lastdate, DayOfWeek _DaysOfWeek)
    {
        using (var api = new HttpClient())
        {
            var request = api.GetAsync($"https://jsonmock.hackerrank.com/api/stocks?Page={pageNumber}").GetAwaiter().GetResult();
            if (request.IsSuccessStatusCode)
            {
                var response = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var stocks = JsonConvert.DeserializeObject<stock>(response);
                var filteredStock = stocks.data.Where(x => x.date >= _firstdate && x.date <= _lastdate && x.date.DayOfWeek == _DaysOfWeek).ToList();
                return filteredStock;
            }
            else
            {
                return null;
            }
        }
    }

    public class responseObject
    {
        public DateTime date { get; set; }
        public decimal open { get; set; }
        public decimal close { get; set; }
        public decimal high { get; set; }
        public decimal low { get; set; }
    }
    public class temp
    {
        public temp()
        {

        }
        public string food { get; set; }
        public tp quantity { get; set; }
        public double time { get; set; }
        public decimal youtubeurl { get; set; }
    }

    public class Viewtemp
    {
        public Viewtemp()
        {

        }
        public string food { get; set; }
        public tp quantity { get; set; }
        public double time { get; set; }
        public decimal youtubeurl { get; set; }
    }

    public class tp
    {
        public tp()
        {

        }
        public int Id { get; set; }
        public DateTime MyDate { get; set; }
    }

    public static class SimpleObjectMapper
    {

    }

    public class data
    {
        public DateTime date { get; set; }
        public decimal open { get; set; }
        public decimal close { get; set; }
        public decimal high { get; set; }
        public decimal low { get; set; }
    }

    public class stock
    {
        public int page { get; set; }
        public int per_page { get; set; }
        public int total { get; set; }
        public int total_pages { get; set; }
        public List<data> data { get; set; }
    }
}
}
