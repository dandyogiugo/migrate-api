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

        public static string sp_getall_new = @"SELECT count(DISTINCT(Customerid)) from [ABS_MEMO].[dbo].[Recommended_products];

                                            SELECT  result.CustomerID,(Customer.FirstName +' '+ Customer.LastName) as 'FullName',Customer.Email,Customer.Phone,Customer.Occupation,Customer.Data_source,
                                            (SELECT count(*) FROM [ABS_MEMO].[dbo].Dim_Policies AS pol inner join [ABS_MEMO].[dbo].Dim_Product as prod 
                                            on pol.ProdID = prod.ProductID and pol.CustomerID = result.CustomerID and 
                                            pol.Bus_Category = 'Individual' and pol.Bus_Source = 'Non_Broker') as 'currentProdCount',
                                            (SELECT count(*) FROM [ABS_MEMO].[dbo].[Recommended_products] where CustomerID = result.CustomerID) as 'NoOfProductRecommended',
                                            (SELECT SUM(Scored_probability)/Count(*) FROM [ABS_MEMO].[dbo].[Recommended_products] where CustomerID = result.CustomerID) as 'AvgProb'
                                            FROM (SELECT DISTINCT TOP 30 CustomerID FROM [ABS_MEMO].[dbo].[Recommended_products]
                                            where CustomerID NOT IN (SELECT DISTINCT TOP {0} CustomerID FROM [ABS_MEMO].[dbo].[Recommended_products])) AS  result
                                            inner join [ABS_MEMO].[dbo].[Dim_Customers] as Customer
                                            ON result.CustomerID = Customer.CustomerID 
                                            order by 'AvgProb' desc";

        //public static string search_query { get; } = @"SELECT * FROM [ABS_MEMO].[dbo].[Dim_Customers]  as cust
        //                                                inner join [ABS_MEMO].[dbo].[Product_recommender] as prod
        //                                                on cust.CustomerID = prod.CustomerID and cust.FullName like '%' + @search + '%';";

        public static string search_query { get; } = @"SELECT DISTINCT prod.CustomerID,(Customer.FirstName +' '+ Customer.LastName) as 'FullName',Customer.Email,Customer.Phone,Customer.Occupation,Customer.Data_source,
                                                        (SELECT count(*) FROM [ABS_MEMO].[dbo].Dim_Policies AS pol inner join [ABS_MEMO].[dbo].Dim_Product as pro 
                                                        on pol.ProdID = pro.ProductID and pol.CustomerID = prod.CustomerID and pol.Bus_Category = 'Individual' 
                                                        and pol.Bus_Source = 'Non_Broker') as 'currentProdCount',
                                                        (SELECT count(*) FROM [ABS_MEMO].[dbo].[Recommended_products] where CustomerID = prod.CustomerID) as 'NoOfProductRecommended',
                                                        (SELECT SUM(Scored_probability)/Count(*) FROM [ABS_MEMO].[dbo].[Recommended_products] where CustomerID = prod.CustomerID) as 'AvgProb'
                                                        FROM [ABS_MEMO].[dbo].[Dim_Customers]  as Customer
                                                        inner join [ABS_MEMO].[dbo].[Recommended_products] as prod
                                                        on Customer.CustomerID = prod.CustomerID and Customer.FullName  like '%' + @search + '%' order by 'AvgProb' desc
;";

        public static string recomendation { get; } = @"SELECT Product_recommended,Scored_probability FROM [ABS_MEMO].[dbo].[Recommended_products] where CustomerID = @customer_id order by Scored_probability desc;
                                                        SELECT prod.Product_lng_descr,pol.Policy_no FROM [ABS_MEMO].[dbo].Dim_Policies as pol
                                                        inner join [ABS_MEMO].[dbo].Dim_Product as prod
                                                        on pol.ProdID = prod.ProductID and pol.CustomerID = @customer_id and
                                                        pol.Bus_Category = 'Individual' and pol.Bus_Source = 'Non_Broker';";

    }
}
