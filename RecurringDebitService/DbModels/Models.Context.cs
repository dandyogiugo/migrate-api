﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RecurringDebitService.DbModels
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class connectionStr : DbContext
    {
        public connectionStr()
            : base("name=connectionStr")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<PaystackRecurringCharge> PaystackRecurringCharges { get; set; }
        public virtual DbSet<PaystackRecurringDump> PaystackRecurringDumps { get; set; }
        public virtual DbSet<AdaptLead> AdaptLeads { get; set; }
    }
}