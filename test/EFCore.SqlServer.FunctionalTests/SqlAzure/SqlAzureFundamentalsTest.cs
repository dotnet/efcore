// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.SqlAzure.Model;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.SqlAzure
{
    [SqlServerCondition(SqlServerCondition.IsSqlAzure)]
    public class SqlAzureFundamentalsTest : IClassFixture<SqlAzureFixture>
    {
        public SqlAzureFundamentalsTest(SqlAzureFixture fixture)
            => Fixture = fixture;

        public SqlAzureFixture Fixture { get; }

        [ConditionalFact]
        public void CanExecuteQuery()
        {
            using var context = CreateContext();
            Assert.NotEqual(0, context.Addresses.Count());
        }

        [ConditionalFact]
        public void CanAdd()
        {
            using var context = CreateContext();
            context.Database.CreateExecutionStrategy().Execute(
                context, contextScoped =>
                {
                    using (contextScoped.Database.BeginTransaction())
                    {
                        contextScoped.Add(
                            new Product
                            {
                                Name = "Blue Cloud",
                                ProductNumber = "xxxxxxxxxxx",
                                Weight = 0.01m,
                                SellStartDate = DateTime.Now
                            });
                        Assert.Equal(1, contextScoped.SaveChanges());
                    }
                });
        }

        [ConditionalFact]
        public void CanUpdate()
        {
            using var context = CreateContext();
            context.Database.CreateExecutionStrategy().Execute(
                context, contextScoped =>
                {
                    using (contextScoped.Database.BeginTransaction())
                    {
                        var product = new Product { ProductID = 999 };
                        contextScoped.Products.Attach(product);
                        Assert.Equal(0, contextScoped.SaveChanges());

                        product.Color = "Blue";

                        Assert.Equal(1, contextScoped.SaveChanges());
                    }
                });
        }

        [ConditionalFact]
        public void IncludeQuery()
        {
            using var context = CreateContext();
            var order = context.SalesOrders
                .OrderBy(s => s.SalesOrderID)
                .Include(s => s.Customer)
                .First();

            Assert.NotNull(order.Customer);
        }

        protected AdventureWorksContext CreateContext()
            => Fixture.CreateContext();
    }
}
