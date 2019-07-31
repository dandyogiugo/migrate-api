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
        //public static string sp_getall { get; } = @"    SELECT  count(*)  from [ABS_MEMO].[dbo].[Product_recommender];
        //                                               SELECT  * FROM ( SELECT ROW_NUMBER() OVER (ORDER BY CustomerID) AS RowNum, 
        //                                               * FROM [ABS_MEMO].[dbo].[Product_recommender]) AS result
        //                                               inner join [ABS_MEMO].[dbo].[Dim_Customers] as Customer
        //                                               ON result.CustomerID = Customer.CustomerID AND  RowNum >= @offset AND RowNum <= @limit ORDER BY RowNum desc;";

        public static string sp_getall_new = @"  SELECT  count(*)  from [ABS_MEMO].[dbo].[Product_recommender];
                                                SELECT  result.CustomerID,Customer.FullName,Customer.Email,Customer.Phone,Customer.Occupation,Customer.Data_source,
                                                 result.product1, result.product2, result.product3, result.product4,prod.product5, 
                                                (SELECT count(*) FROM [ABS_MEMO].[dbo].Dim_Policies AS pol inner join [ABS_MEMO].[dbo].Dim_Product as prod 
                                                on pol.ProdID = prod.ProductID and pol.CustomerID = result.CustomerID) as 'currentProdCount'
                                                FROM (SELECT ROW_NUMBER() OVER (ORDER BY CustomerID) AS RowNum, * FROM [ABS_MEMO].[dbo].[Product_recommender]) AS result
                                                inner join [ABS_MEMO].[dbo].[Dim_Customers] as Customer
                                                ON result.CustomerID = Customer.CustomerID AND  RowNum >= @offset AND RowNum <= @limit ORDER BY RowNum desc;";

        //public static string search_query { get; } = @"SELECT * FROM [ABS_MEMO].[dbo].[Dim_Customers]  as cust
        //                                                inner join [ABS_MEMO].[dbo].[Product_recommender] as prod
        //                                                on cust.CustomerID = prod.CustomerID and cust.FullName like '%' + @search + '%';";

        public static string search_query { get; } = @"SELECT prod.CustomerID,Customer.FullName,Customer.Email,Customer.Phone,Customer.Occupation,Customer.Data_source,
                                                         prod.product1, prod.product2, prod.product3, prod.product4,prod.product5,
                                                         (SELECT count(*) FROM [ABS_MEMO].[dbo].Dim_Policies AS pol inner join [ABS_MEMO].[dbo].Dim_Product as pro 
                                                        on pol.ProdID = pro.ProductID and pol.CustomerID = prod.CustomerID) as 'currentProdCount'
                                                         FROM [ABS_MEMO].[dbo].[Dim_Customers]  as Customer
                                                        inner join [ABS_MEMO].[dbo].[Product_recommender] as prod
                                                        on Customer.CustomerID = prod.CustomerID and Customer.FullName like '%' + @search + '%';";

        public static string recomendation { get; } = @"SELECT * FROM [ABS_MEMO].[dbo].[Product_recommender] where CustomerID = @customer_id;

                                                        SELECT prod.Product_lng_descr FROM [ABS_MEMO].[dbo].Dim_Policies as pol
                                                        inner join [ABS_MEMO].[dbo].Dim_Product as prod
                                                        on pol.ProdID = prod.ProductID and pol.CustomerID = @customer_id;";

    }
}
