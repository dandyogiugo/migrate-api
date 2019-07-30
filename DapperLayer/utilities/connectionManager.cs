using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DapperLayer.utilities
{
    public static class connectionManager
    {
        public static string connectionString()
        {
            var cnn = ConfigurationManager.ConnectionStrings["Dapper"].ConnectionString;
            return cnn;
        }
        public static string sp_getall { get;} = @"    SELECT  count(*)  from [ABS_MEMO].[dbo].[Product_recommender];
                                                       SELECT  * FROM ( SELECT ROW_NUMBER() OVER (ORDER BY CustomerID) AS RowNum, 
                                                       * FROM [ABS_MEMO].[dbo].[Product_recommender]) AS result
                                                       inner join [ABS_MEMO].[dbo].[Dim_Customers] as Customer
                                                       ON result.CustomerID = Customer.CustomerID AND  RowNum >= @offset AND RowNum <= @limit ORDER BY RowNum desc;";

    }
}
