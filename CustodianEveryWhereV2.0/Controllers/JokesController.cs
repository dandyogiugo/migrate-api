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
    public class JokesController : ApiController
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private store<ApiConfiguration> _apiconfig = null;
        private Utility util = null;
        private store<JokesList> jokelist = null;
        public JokesController()
        {
            _apiconfig = new store<ApiConfiguration>();
            util = new Utility();
            jokelist = new store<JokesList>();
        }

        [HttpGet]
        public async Task<notification_response> GetJokes(string merchant_id, string hash, int page = 1)
        {
            try
            {
                var check_user_function = await util.CheckForAssignedFunction("GetJokes", merchant_id);
                if (!check_user_function)
                {
                    return new notification_response
                    {
                        status = 401,
                        message = "Permission denied from accessing this feature",
                    };
                }
                var config = await _apiconfig.FindOneByCriteria(x => x.merchant_id == merchant_id.Trim());
                if (config == null)
                {
                    log.Info($"Invalid merchant Id {merchant_id}");
                    return new notification_response
                    {
                        status = 402,
                        message = "Invalid merchant Id"
                    };
                }

                // validate hash
                var checkhash = await util.ValidateHash2(merchant_id, config.secret_key, hash);
                if (!checkhash)
                {
                    log.Info($"Hash missmatched from request {merchant_id}");
                    return new notification_response
                    {
                        status = 405,
                        message = "Data mismatched"
                    };
                }

                if (page <= 0)
                {
                    return new notification_response
                    {
                        status = 203,
                        message = "page index start at 1"
                    };
                }
                var getjokes = await jokelist.GetAll();
                int pagesize = 10;
                int skip = (page == 1) ? 0 : pagesize * (page - 1);
                decimal total = Convert.ToDecimal(getjokes.Count) / Convert.ToDecimal(pagesize);
                int totalpage = (int)Math.Ceiling(total);
                var sortjoke = getjokes.OrderByDescending(x => x.Id).Skip(skip).Take(pagesize).Select(y => new joke
                {
                    youtube_url = y.youtube_link,
                    credit = y.credit,
                    thumbnail_image = string.Format("https://img.youtube.com/vi/{0}/0.jpg", y.youtube_link.Split('=')[1].Trim()),
                    title = "No title yet"
                }).ToList();

                return new notification_response
                {
                    status = 200,
                    data = new Dictionary<string, object>() {
                        { "page",page },
                        { "navigation", $"{page} of {totalpage}" },
                        { "total_pages",totalpage },
                        { "jokelist", sortjoke}
                    }
                };

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                log.Error(ex.StackTrace);
                log.Error(ex.InnerException);
                return new notification_response
                {
                    status = 404,
                    message = "oops!, something happend while searching for jokes"
                };
            }
        }
    }
}
