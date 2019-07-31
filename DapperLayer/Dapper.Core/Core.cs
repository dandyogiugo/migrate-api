using DapperLayer.utilities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using System.Dynamic;

namespace DapperLayer.Dapper.Core
{
    public class Core<T> where T : class
    {
        public async Task<dynamic> GetAllbyPagination(int offset, int limit, string sql)
        {
            IList<int> count = null;
            IList<T> results = null;
            using (var cnn = new SqlConnection(connectionManager.connectionString()))
            {
                var p = new { offset = offset, limit = limit };
                using (var multiple = await cnn.QueryMultipleAsync(sql.Trim(), p))
                {
                    count = multiple.Read<int>().ToList();
                    results = multiple.Read<T>().ToList();
                }
            };
            dynamic ret = new ExpandoObject();
            ret.results = results;
            ret.count = count[0];
            return ret;
        }

        public async Task<dynamic> SearchPager(int page, int skip, string terms, string sql)
        {
            IList<T> results = null;
            dynamic obj = new ExpandoObject();
            using (var cnn = new SqlConnection(connectionManager.connectionString()))
            {
                var p = new { search = terms };
                var result = await cnn.QueryAsync<T>(sql.Trim(), p);
                obj.count = result.Count();
                results = result.Skip(skip).Take(page).ToList();
            };

            obj.results = results;
            return obj;
        }

        public async Task<dynamic> GetPredictionByCustomerID(int customer_id, string sql)
        {
            IList<T> current = null;
            IList<T> recommended = null;
            dynamic obj = new ExpandoObject();
            using (var cnn = new SqlConnection(connectionManager.connectionString()))
            {
                var p = new { customer_id = customer_id };
                using (var multiple = await cnn.QueryMultipleAsync(sql.Trim(), p))
                {
                    recommended = multiple.Read<T>().ToList();
                    current = multiple.Read<T>().ToList();
                   
                }
            };

            obj.current = current;
            obj.recommended = recommended[0];
            return obj;
        }
    }
}
