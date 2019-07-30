using DapperLayer.Dapper.Core;
using DapperLayer.utilities;
using DataStore.Models;
using DataStore.repository;
using DataStore.Utilities;
using DataStore.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CustodianEveryWhereV2._0.Controllers
{
    public class CRMController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private Utility util = null;
        private store<ApiConfiguration> _apiconfig = null;
        private Core<dynamic> dapper_core = null;
        public CRMController()
        {
            util = new Utility();
            _apiconfig = new store<ApiConfiguration>();
            dapper_core = new Core<dynamic>();
        }

        [HttpGet]
        public async Task<dynamic> GetPrediction(string merchant_id, int page = 1)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetPrediction", merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id);
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                int pagesize = 30;
                int skip = (page == 1) ? 0 : pagesize * (page - 1);
                int limit = skip + pagesize;
                var result = await dapper_core.GetAllbyPagination(skip, limit, connectionManager.sp_getall);
                if (result == null)
                {
                    return new
                    {
                        status = 401,
                        message = "No predicted result found"
                    };
                }
                decimal total = Convert.ToDecimal(result.count) / Convert.ToDecimal(pagesize);
                int totalpage = (int)Math.Ceiling(total);
                var props = new
                {
                    pageSize = pagesize,
                    totalPages = totalpage,
                    statusCode = 200,
                    navigation = $"{page} of {totalpage}"
                };
                List<dynamic> mylist = new List<dynamic>();
                foreach (var item in result.results)
                {
                    mylist.Add(new
                    {
                        CustomerID = item.CustomerID,
                        FullName = item.FullName,
                        Email = item.Email,
                        Phone = item.Phone,
                        Occupation = item.Occupation,
                        Products = Newtonsoft.Json.JsonConvert.DeserializeObject($"['{item.product1}','{item.product2}','{item.product3}','{item.product4}']")
                    });
                }
                return new { pageProps = props, dataSets = mylist };
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new notification_response
                {
                    status = 404,
                    message = "Error: while getting predicted product"
                };
            }
        }
    }
}
