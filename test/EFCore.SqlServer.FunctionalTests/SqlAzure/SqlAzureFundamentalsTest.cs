// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.SqlAzure.Model;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.SqlAzure
{
    [SqlServerCondition(SqlServerCondition.IsSqlAzure)]
    public class SqlAzureFundamentalsTest : IClassFixture<SqlAzureFixture>
    {
        [ConditionalFact]
        public void CanExecuteQuery()
        {
            using (var context = _fixture.CreateContext())
            {
                Assert.NotEqual(0, context.Addresses.Count());
            }
        }

        [ConditionalFact]
        public void CanAdd()
        {
            using (var context = _fixture.CreateContext())
            {
                context.Database.CreateExecutionStrategy().Execute(contextScoped =>
                    {
                        using (contextScoped.Database.BeginTransaction())
                        {
                            contextScoped.Add(new Product
                            {
                                Name = "Blue Cloud",
                                ProductNumber = "xxxxxxxxxxx",
                                Weight = 0.01m,
                                SellStartDate = DateTime.Now
                            });
                            Assert.Equal(1, contextScoped.SaveChanges());
                        }
                    }, context);
            }
        }

        [ConditionalFact]
        public void CanUpdate()
        {
            using (var context = _fixture.CreateContext())
            {
                context.Database.CreateExecutionStrategy().Execute(contextScoped =>
                    {
                        using (contextScoped.Database.BeginTransaction())
                        {
                            var product = new Product { ProductID = 999 };
                            contextScoped.Products.Attach(product);
                            Assert.Equal(0, contextScoped.SaveChanges());

                            product.Color = "Blue";

                            Assert.Equal(1, contextScoped.SaveChanges());
                        }
                    }, context);
            }
        }

        [ConditionalFact]
        public void IncludeQuery()
        {
            using (var context = _fixture.CreateContext())
            {
                var order = context.SalesOrders
                    .Include(s => s.Customer)
                    .First();

                Assert.NotNull(order.Customer);
            }
        }

        private readonly SqlAzureFixture _fixture;

        public SqlAzureFundamentalsTest(SqlAzureFixture fixture)
        {
            _fixture = fixture;
        }
    }
}
