using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(CustodianEveryWhereV2._0.Startup))]

namespace CustodianEveryWhereV2._0
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
         
            ConfigureAuth(app);
        }
    }
}
