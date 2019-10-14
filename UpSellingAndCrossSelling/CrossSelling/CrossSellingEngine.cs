using CustodianEmailSMSGateway.Email;
using DapperLayer.Dapper.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UpSellingAndCrossSelling.Config;

namespace UpSellingAndCrossSelling.CrossSelling
{
    public class CrossSellingEngine
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();
        private Core<DbModels> _getRecords = null;
        public CrossSellingEngine()
        {
            _getRecords = new Core<DbModels>();
        }
        public  void EngineProcessor()
        {
            List<RecommendationList> peopleToRecommend = new List<RecommendationList>();

            var getRecords =  GetCustomerRecords().GetAwaiter().GetResult();
            getRecords = getRecords?.Take(5).ToList();
            if (getRecords != null && getRecords.Count() > 0)
            {
                foreach (var record in getRecords)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(record.currentProds))
                        {
                            var currentProducts = record.currentProds.Split(',').Where(x => !string.IsNullOrEmpty(x?.Trim())).ToList();
                         
                            if (Apiconfig._configSettings != null)
                            {
                                var postParams = SetPostParams(new RequestModel
                                {
                                    Age = record.Date_of_Birth,
                                    Gender = record.Gender,
                                    Occupation = record.Occupation,
                                    Premium = record.Premium.ToString()
                                });
                                var myProduct = Apiconfig._configSettings.Where(x => !currentProducts.Any(y => y.ToUpper().Trim() == x.ProductName.ToUpper().Trim())).ToList();
                                var reco = new RecommendationList();
                                reco.Phone = record.PhoneNumber;
                                reco.Email = record.Email;
                                reco.CustomerName = record.CustomerName;
                                List<string> recommendedProd = new List<string>();
                                Console.WriteLine($"====================Start=========================");
                                Console.WriteLine($"Call for :{Newtonsoft.Json.JsonConvert.SerializeObject(myProduct.Select(x => new { Prod = x.ProductName }).ToArray())}");
                                Console.WriteLine($"Current Product {Newtonsoft.Json.JsonConvert.SerializeObject(currentProducts)}");
                                foreach (var item in myProduct)
                                {

                                    try
                                    {
                                        using (var client = new HttpClient())
                                        {
                                            client.BaseAddress = new Uri(item.EndPoint);
                                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", item.Key);
                                            var request =  client.PostAsJsonAsync<object>("", postParams).GetAwaiter().GetResult();
                                            if (request.IsSuccessStatusCode)
                                            {
                                                var response =  request.Content.ReadAsAsync<Dictionary<object, Dictionary<string, List<object>>>>().GetAwaiter().GetResult();
                                                var res = (dynamic)response["Results"]["output1"][0];
                                                int value = (int)res["Scored Labels"];
                                                Console.WriteLine($"Prod:{item.ProductName}: IsRecommended: {value}");
                                                decimal probability = (decimal)res["Scored Probabilities"];
                                                if (value == 1)
                                                {
                                                    recommendedProd.Add(item.ProductName);
                                                }
                                                else
                                                {
                                                    _log.Info($"No recommendation for customer with name {record.CustomerName}  probability{probability}");
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _log.Info($"Process crashed for customer  {record.CustomerName}");
                                        _log.Error(ex.Message);
                                        _log.Error(ex.StackTrace);
                                        _log.Error(ex.InnerException);
                                        Console.WriteLine(ex.Message + "Stack====>" + ex.StackTrace);
                                    }
                                }
                                Console.WriteLine($"======================End=======================");
                                if (recommendedProd.Count() > 0)
                                {
                                    reco.ProductList = recommendedProd;
                                    peopleToRecommend.Add(reco);

                                }
                            }
                            else
                            {
                                _log.Info("Configuration settings is null from API_SETTINGS.json");
                            }
                        }
                        else
                        {
                            _log.Info("current product for use is not null");
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Info("Reading file failed from API_SETTINGS.json");
                        _log.Error(ex.Message);
                        _log.Error(ex.StackTrace);
                        _log.Error(ex.InnerException);
                    }
                }
            }
            else
            {
                _log.Info("No new customer from MDM database");
            }



            if (peopleToRecommend.Count() > 0)
            {
                SendMailAndSMS(peopleToRecommend);
            }
        }
        public object SetPostParams(RequestModel model)
        {
            if (model == null)
                return null;
            string Age = "";
            string Occupation = "";
            string Gender = "";
            if (model.Age != null)
            {
                Age = (DateTime.Now.Year - model.Age.Year).ToString();
            }

            if (!string.IsNullOrEmpty(model.Occupation) && model.Occupation.Length >= 4)
            {
                Occupation = model.Occupation.Trim();
            }

            if (!string.IsNullOrEmpty(model.Gender) && model.Gender.Length == 1)
            {
                Gender = model.Gender.Trim();
            }
            var scoreRequest = new
            {
                Inputs = new Dictionary<string, List<Dictionary<string, string>>>() {
                        {
                            "input1",
                            new List<Dictionary<string, string>>(){new Dictionary<string, string>(){
                                            {
                                                "gender", Gender
                                            },
                                            {
                                                "Occupation", Occupation
                                            },
                                            {
                                                "Age", Age
                                            },
                                            {
                                                "Premium", model.Premium
                                            },
                                }
                            }
                        },
                    },
                GlobalParameters = new Dictionary<string, string>()
                {
                }
            };

            return (object)scoreRequest;
        }
        public async Task<List<DbModels>> GetCustomerRecords()
        {
            try
            {
                DateTime end_date = DateTime.Now;
                DateTime start_date = DateTime.Now.AddDays(-600);
                var getRecords = await _getRecords.GetNewCustomerDetails(start_date, end_date);
                return getRecords?.ToList();
            }
            catch (Exception ex)
            {
                _log.Info("about to get records from GetCustomerRecords");
                _log.Error(ex.Message);
                _log.Error(ex.StackTrace);
                _log.Error(ex.InnerException);
                return null;
            }
        }

        public async void SendMailAndSMS(List<RecommendationList> ItemList)
        {
            try
            {
                if (ItemList.Count() > 0)
                {
                    foreach (var item in ItemList)
                    {
                        StringBuilder template = new StringBuilder(Apiconfig._emailTemplate);
                        template.Replace("#CUSTOMERNAME#", item.CustomerName);
                        string html = "";
                        foreach (var prod in item.ProductList)
                        {
                            var getProductDetails = Apiconfig._configSettings.FirstOrDefault(x => x.ProductName == prod);
                            html += $@"<li>
                                        <a href ='{getProductDetails.HyperLink}'><strong>{getProductDetails.ProductName}</strong><p style='font-size: 10pt;'>{getProductDetails.ProductDescription}</p></a>
                                      </li>";
                        }
                        template.Replace("#PRODUCTS#", html);
                        string Body = template.ToString();
                        item.Email = "oscardybabaphd@gmail.com";
                        new SendEmail().Send_Email(item.Email, "Recommended Products", Body, "Product Recommendation");
                        //Send SMS here
                    }

                }
                else
                {
                    _log.Info($"Nothing to send at this datetime {DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                _log.Info($"About to start sending mail and sms to customer for recommended product");
                _log.Error(ex.Message);
                _log.Error(ex.StackTrace);
                _log.Error(ex.InnerException);
            }
        }
    }
}
