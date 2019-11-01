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
using DataStore.ViewModels;

namespace DapperLayer.Dapper.Core
{
    public class Core<T> where T : class
    {
        public async Task<dynamic> GetAllbyPagination(int limit, string sql)
        {
            IList<int> count = null;
            IList<T> results = null;
            using (var cnn = new SqlConnection(connectionManager.connectionString()))
            {
                var p = new { limit = limit };
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
            obj.recommended = recommended;
            return obj;
        }

        public async Task<IEnumerable<T>> GetNewCustomerDetails(DateTime start_date, DateTime end_date)
        {
            var p = new { start_date = start_date, end_date = end_date };
            using (var cnn = new SqlConnection(connectionManager.connectionString()))
            {
                var result = await cnn.QueryAsync<T>(connectionManager._getNewCustomerSP, p, null, null, CommandType.StoredProcedure);
                return result;
            }
        }


        public async Task<List<T>> GetRenewalRatio(string sql)
        {

            using (var cnn = new SqlConnection(connectionManager.connectionString()))
            {
                var multiple = await cnn.QueryAsync<T>(sql.Trim(), commandTimeout: 60);
                return multiple?.ToList();
            };
        }

        public async Task<NextRenewalResult> GetRenewalNext(string query, string condition = "", string condition_where = "", int offset = 0, int next = 100)
        {
            IList<int> count = null;
            IList<NextRenewal> results = null;
            var p = new
            {
                condition = condition,
                condition_where = condition_where,
                offset = offset,
                next = next
            };
            var result = new NextRenewalResult();
            var new_query = string.Format(query, condition, offset, next, condition_where);
            using (var cnn = new SqlConnection(connectionManager.connectionString()))
            {
                using (var multiple = await cnn.QueryMultipleAsync(new_query))
                {
                    results = multiple.Read<NextRenewal>().ToList();
                    count = multiple.Read<int>().ToList();
                    result.TotalPages = count[0];
                    result.Results = results.ToList();
                }
            };
            return result;
        }
    }
}
