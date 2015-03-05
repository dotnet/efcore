// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.SqlServer.FunctionalTests.TestModels;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class ConnectionStringTest
    {
        [Fact]
        public async Task Can_use_actual_connection_string_in_OnConfiguring()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddEntityFramework()
                .AddSqlServer();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            using (await SqlServerNorthwindContext.GetSharedStoreAsync())
            {
                using (var context = new NorthwindContext(serviceProvider, SqlServerNorthwindContext.ConnectionString))
                {
                    Assert.Equal(91, await context.Customers.CountAsync());
                }
            }
        }

        private class NorthwindContext : DbContext
        {
            private readonly string _nameOrConnectionString;

            public NorthwindContext(IServiceProvider serviceProvider, string nameOrConnectionString)
                : base(serviceProvider)
            {
                _nameOrConnectionString = nameOrConnectionString;
            }

            public DbSet<Customer> Customers { get; set; }

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseSqlServer(_nameOrConnectionString);
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                ConfigureModel(modelBuilder);
            }
        }

        private class Customer
        {
            public string CustomerID { get; set; }
            public string CompanyName { get; set; }
            public string Fax { get; set; }
        }

        private static void ConfigureModel(ModelBuilder builder)
        {
            builder.Entity<Customer>(b =>
                {
                    b.Key(c => c.CustomerID);
                    b.ForSqlServer().Table("Customers");
                });
        }
    }
}
