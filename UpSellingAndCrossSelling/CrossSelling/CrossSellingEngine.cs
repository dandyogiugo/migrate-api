using DapperLayer.Dapper.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
        public async void Engine()
        {
            using (var client = new HttpClient())
            {
                var getRecords = await this.GetCustomerRecords();
                if (getRecords != null && getRecords.Count() > 0)
                {
                    foreach (var record in getRecords)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(record.currentProds))
                            {
                                var currentProducts = record.currentProds.Split(',').Where(x => !string.IsNullOrEmpty(x)).ToList();
                                if (Apiconfig._configSettings != null)
                                {
                                    var postParams = this.SetPostParams(new RequestModel
                                    {
                                        Age = (DateTime.Now.Year - record.Date_of_Birth.Year).ToString(),
                                        Gender = record.Gender,
                                        Occupation = record.Occupation,
                                        //Premium = record.
                                    });
                                    var myProduct = Apiconfig._configSettings.Where(x => currentProducts.Any(y => y.Trim().Equals(x.ProductName.Trim())));
                                    foreach (var item in myProduct)
                                    {
                                        client.BaseAddress = new Uri(item.EndPoint);
                                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", item.Key);
                                        var response = await client.PostAsJsonAsync<object>("", postParams);
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

            }
        }
        public object SetPostParams(RequestModel model)
        {
            var scoreRequest = new
            {
                Inputs = new Dictionary<string, List<Dictionary<string, string>>>() {
                        {
                            "input1",
                            new List<Dictionary<string, string>>(){new Dictionary<string, string>(){
                                            {
                                                "gender", model.Gender
                                            },
                                            {
                                                "Occupation", model.Occupation
                                            },
                                            {
                                                "Age", model.Age
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
                DateTime start_date = DateTime.Now;
                DateTime end_date = start_date.AddDays(-300);
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
    }
}
