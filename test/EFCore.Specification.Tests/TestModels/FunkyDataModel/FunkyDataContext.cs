// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.TestModels.FunkyDataModel
{
    public class FunkyDataContext : PoolableDbContext
    {
        public FunkyDataContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<FunkyCustomer> FunkyCustomers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FunkyCustomer>().Property(e => e.Id).ValueGeneratedNever();
        }

        public static void Seed(FunkyDataContext context)
        {
            context.FunkyCustomers.AddRange(FunkyDataData.CreateFunkyCustomers());
            context.SaveChanges();
        }
    }
}
