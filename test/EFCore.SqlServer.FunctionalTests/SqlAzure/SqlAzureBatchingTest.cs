// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.SqlAzure.Model;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.SqlAzure
{
    [SqlServerCondition(SqlServerCondition.IsSqlAzure)]
    public class SqlAzureBatchingTest : IClassFixture<BatchingSqlAzureFixture>
    {
        public SqlAzureBatchingTest(BatchingSqlAzureFixture fixture, ITestOutputHelper output)
        {
            Fixture = fixture;
        }

        public BatchingSqlAzureFixture Fixture { get; }

        [ConditionalTheory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public void AddWithBatchSize(int batchSize)
        {
            using var context = Fixture.CreateContext(batchSize);
            context.Database.CreateExecutionStrategy().Execute(
                context, contextScoped =>
                {
                    using (contextScoped.Database.BeginTransaction())
                    {
                        for (var i = 0; i < batchSize; i++)
                        {
                            var uuid = Guid.NewGuid().ToString();
                            contextScoped.Products.Add(
                                new Product
                                {
                                    Name = uuid,
                                    ProductNumber = uuid.Substring(0, 25),
                                    Weight = 1000,
                                    SellStartDate = DateTime.Now
                                });
                        }

                        Assert.Equal(batchSize, contextScoped.SaveChanges());
                    }
                });
        }
    }
}
