using DataStore.ViewModels;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperLayer.utilities
{
    public class helpers
    {
        public helpers()
        {

        }
        public string QueryResolver(RenewalRatio renewalRatio)
        {
            //Supper Admin role
            string condition = "";
            //MD role
            if (renewalRatio.is_MD && renewalRatio.subsidary.HasValue)
            {
                string Company = (renewalRatio.subsidary.Value == subsidiary.General) ? "ABS" : "Turnquest";
                condition = $" and Company = '{Company}'";
            }
            //User role
            if (renewalRatio.user_division != null && renewalRatio.user_division.Count() > 0)
            {
                if (renewalRatio.user_division.Count() == 1)
                {
                    condition = $" and unitid in ({renewalRatio.user_division[0]})";
                }
                else
                {
                    string ids = string.Join(",", renewalRatio.user_division);
                    condition = $" and unitid in ({ids})";
                }
            }
            return condition;
        }

        public List<dynamic> Grouper(List<RenewRatio> renewRatios)
        {
            try
            {

                List<IGrouping<string, RenewRatio>> group_by_company = renewRatios.GroupBy(x => x.Company.Trim()).ToList();
                List<dynamic> main_object = new List<dynamic>();
                foreach (var _item in group_by_company)
                {
                    List<dynamic> groups = new List<dynamic>();
                    dynamic main = new ExpandoObject();
                    main.Source = _item.Key;
                    main.count = _item.Sum(x => x.Count);
                    List<IGrouping<string, RenewRatio>> group_by_unit_id = _item.GroupBy(x => x.Status.Trim()).ToList();
                    foreach (var item in group_by_unit_id)
                    {
                        dynamic obj = new ExpandoObject();
                        List<dynamic> details = new List<dynamic>();
                        obj.total = item.Sum(x => x.Count);
                        var sub_groups = item.GroupBy(x => x.Unit_lng_descr.Trim());
                        obj.category = sub_groups.First().First().Status;
                        List<dynamic> sub_group = new List<dynamic>();
                        foreach (var sub_item in sub_groups)
                        {
                            sub_group.Add(new
                            {
                                division = sub_item.First().Unit_lng_descr,
                                division_id = sub_item.First().unit_id,
                                sub_total = sub_item.Sum(x => x.Count),
                                data = GetSubGroups(sub_item)
                            });

                            details.Add(sub_group);
                        }
                        obj.details = details;
                        groups.Add(obj);
                    }
                    main.data = groups;
                    main_object.Add(main);


                }

                return main_object;

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<dynamic> GetSubGroups(IGrouping<string, RenewRatio> sub_item)
        {
            List<dynamic> sub_sub = new List<dynamic>();
            foreach (var sub_sub_item in sub_item)
            {
                sub_sub.Add(new
                {
                    product = sub_sub_item.Product,
                    Count = sub_sub_item.Count
                });
            }
            return sub_sub;

        }


        public List<dynamic> Grouper2(List<NextRenewal> renewRatios)
        {
            try
            {
                List<IGrouping<string, NextRenewal>> group_by_company = renewRatios.GroupBy(x => x.Company.Trim()).ToList();
                List<dynamic> main_object = new List<dynamic>();
                foreach (var _item in group_by_company)
                {
                    List<dynamic> groups = new List<dynamic>();
                    dynamic main = new ExpandoObject();
                    main.Source = _item.Key;
                    main.TotalPremium = _item.Sum(x => x.Premium);
                    main.Count = _item.Count();
                    List<IGrouping<string, NextRenewal>> group_by_unit_id = _item.GroupBy(x => x.Unit_lng_descr.Trim()).ToList();
                    foreach (var item in group_by_unit_id)
                    {
                        dynamic obj = new ExpandoObject();
                        List<dynamic> details = new List<dynamic>();
                        obj.TotalPremium = item.Sum(x => x.Premium);
                        obj.Unit = item.First().Unit_lng_descr;
                        obj.Count = item.Count();
                        List<dynamic> sub_group = new List<dynamic>();
                        foreach (var sub_item in item)
                        {
                            sub_group.Add(new
                            {
                                policy_number = sub_item.pol_no,
                                end_date = sub_item.EndDate.Date,
                                days_after = sub_item.DaysAfter,
                                product = sub_item.Product_lng_descr,
                                premium = sub_item.Premium
                            });

                            details.Add(sub_group);
                        }
                        obj.details = details;
                        groups.Add(obj);
                    }
                    main.data = groups;
                    main_object.Add(main);
                }
               
                return main_object;

            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
