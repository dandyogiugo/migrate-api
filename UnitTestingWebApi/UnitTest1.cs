using System;
using CustodianEveryWhereV2._0.Controllers;
using DataStore.Utilities;
using DataStore.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestingWebApi
{
    [TestClass]
    public class UnitTest1
    {
        private Utility util = null;
        private string merchant_id = "CUST_00003";
        private string secret_key = "Test_vkMqq$%ww";
        public UnitTest1()
        {
            util = new Utility();
        }
        [TestMethod]
        public void GetLifeQuote()
        {
            // string secret_key = "Test_vkMqq$%ww";
            var quote = new LifeQuoteObject
            {
                amount = 10000,
                date_of_birth = "1989-04-24",
                frequency = Frequency.Single,
                merchant_id = merchant_id,
                policy_type = PolicyType.TermAssurance,
                terms = 1
            };
            var hash = util.MD_5(quote.date_of_birth + quote.amount + quote.frequency.ToString() + secret_key).GetAwaiter().GetResult();
            quote.hash = hash;
            var controller = new LifeController().GetQuote(quote).GetAwaiter().GetResult();
            Console.WriteLine($"Raw Object from API {Newtonsoft.Json.JsonConvert.SerializeObject(controller)}");
            Assert.AreEqual(controller.status, 200);
        }

        [TestMethod]
        public void GetHomeShieldQuote()
        {
            var hash = util.MD_5(2.ToString() + secret_key).GetAwaiter().GetResult();
            var controller = new HomeShieldController().GetHomeShieldQuote(merchant_id, 2, hash).GetAwaiter().GetResult();
            Console.WriteLine($"Raw Object from API {Newtonsoft.Json.JsonConvert.SerializeObject(controller)}");
            Assert.AreEqual(controller.status, 200);
          
        }
    }
}
