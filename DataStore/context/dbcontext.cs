using DataStore.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.context
{
    public class dbcontext : DbContext
    {
        public dbcontext() : base("name=CustApi")
        {

        }

        public DbSet<ApiConfiguration> ApiConfiguration { get; set; }
        public DbSet<GITInsurance> GITInsurance { get; set; }
        public DbSet<PremiumCalculatorMapping> PremiumCalculatorMapping { get; set; }
        public DbSet<ApiMethods> ApiMethods { get; set; }
        public DbSet<LifeClaims> LifeClaims { get; set; }
        public DbSet<GeneralClaims> GeneralClaims { get; set; }
        public DbSet<NonLifeClaimsDocument> NonLifeClaimsDocument { get; set; }
        public DbSet<TravelInsurance> TravelInsurance { get; set; }
        public DbSet<FlightAndAirPortData> FlightAndAirPortData { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            //modelBuilder.Entity<NonLifeClaims>().HasMany(x => x.NonLifeDocument);
        }
    }
}
