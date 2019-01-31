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
    public class FitfamPlusController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        public FitfamPlusController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
        }

        [HttpGet]
        public async Task<Wallet> WelletBalance()
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new Wallet
                {
                    status = 404,
                    message = "oops!, something happend while getting your balance"
                };
            }
        }
    }
}
