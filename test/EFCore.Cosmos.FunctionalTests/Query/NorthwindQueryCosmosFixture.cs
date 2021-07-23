// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindQueryCosmosFixture<TModelCustomizer> : NorthwindQueryFixtureBase<TModelCustomizer>
        where TModelCustomizer : IModelCustomizer, new()
    {
        protected override ITestStoreFactory TestStoreFactory
            => CosmosNorthwindTestStoreFactory.Instance;

        protected override bool UsePooling
            => false;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

        protected override bool ShouldLogCategory(string logCategory)
            => logCategory == DbLoggerCategory.Query.Name;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder
                .Entity<CustomerQuery>()
                .HasDiscriminator<string>("Discriminator").HasValue("Customer");

            modelBuilder
                .Entity<OrderQuery>()
                .HasDiscriminator<string>("Discriminator").HasValue("Order");

            modelBuilder
                .Entity<ProductQuery>()
                .HasDiscriminator<string>("Discriminator").HasValue("Product");

            modelBuilder
                .Entity<CustomerQueryWithQueryFilter>()
                .HasDiscriminator<string>("Discriminator").HasValue("Customer");
        }
    }
}
