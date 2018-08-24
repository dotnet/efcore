// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsyncGroupByQuerySqlServerTest : AsyncGroupByQueryTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        internal sealed class Order
        {
            public int OrderID { get; set; }
            public string CustomerID { get; set; }
            public decimal? Freight { get; set; }
        }

        internal sealed class OrderContext : DbContext
        {
            public OrderContext(DbContextOptions options) : base(Create(options))
            {
            }

            private static DbContextOptions Create(DbContextOptions options)
            {
                var optionsBuilder = new DbContextOptionsBuilder<OrderContext>();
                optionsBuilder.UseSqlServer(GetConnectionString(options));
                return optionsBuilder.Options;
            }
            private static string GetConnectionString(DbContextOptions options)
            {
                foreach (var extension in options.Extensions)
                    if (extension is RelationalOptionsExtension relationalOptionsExtension)
                        return relationalOptionsExtension.ConnectionString ?? relationalOptionsExtension.Connection.ConnectionString;

                throw new InvalidOperationException("cannot find connection string");
            }
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Order>().ToTable("Orders").HasKey(i => i.OrderID);
            }

            public DbSet<Order> Orders { get; set; }
        }

        // ReSharper disable once UnusedParameter.Local
        public AsyncGroupByQuerySqlServerTest(NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        [ConditionalFact]
        public async Task GroupBy_Select_sum_async()
        {
            using (var context = new OrderContext(base.Fixture.CreateOptions()))
            {
                var query = await context.Orders.GroupBy(o => o.CustomerID).Select(g => g.Sum(o => o.Freight)).ToListAsync();
                Assert.Equal(89, query.Count);
            }
        }
    }
}
