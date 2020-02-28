using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CustodianEveryWhereV2._0.Controllers
{
    public class AnnuityController : ApiController
    {
        public AnnuityController()
        {

        }
        [HttpGet]
        public async Task<dynamic> GetAnnuity(DateTime dob, double amount)
        {
            try
            {

            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
