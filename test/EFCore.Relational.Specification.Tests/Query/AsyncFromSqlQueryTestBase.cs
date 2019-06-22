// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable AccessToDisposedClosure
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class AsyncFromSqlQueryTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryRelationalFixture<NoopModelCustomizer>, new()
    {
        protected AsyncFromSqlQueryTestBase(TFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        protected TFixture Fixture { get; }

        [ConditionalFact]
        public virtual async Task FromSqlRaw_queryable_simple()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>()
                    .FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [ContactName] LIKE '%z%'"))
                    .ToArrayAsync();

                Assert.Equal(14, actual.Length);
                Assert.Equal(14, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual async Task FromSqlRaw_queryable_simple_columns_out_of_order()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>().FromSqlRaw(
                        NormalizeDelimetersInRawString(
                            "SELECT [Region], [PostalCode], [Phone], [Fax], [CustomerID], [Country], [ContactTitle], [ContactName], [CompanyName], [City], [Address] FROM [Customers]"))
                    .ToArrayAsync();

                Assert.Equal(91, actual.Length);
                Assert.Equal(91, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual async Task FromSqlRaw_queryable_simple_columns_out_of_order_and_extra_columns()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>().FromSqlRaw(
                        NormalizeDelimetersInRawString(
                            "SELECT [Region], [PostalCode], [PostalCode] AS [Foo], [Phone], [Fax], [CustomerID], [Country], [ContactTitle], [ContactName], [CompanyName], [City], [Address] FROM [Customers]"))
                    .ToArrayAsync();

                Assert.Equal(91, actual.Length);
                Assert.Equal(91, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual async Task FromSqlRaw_queryable_composed()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers]"))
                    .Where(c => c.ContactName.Contains("z"))
                    .ToArrayAsync();

                Assert.Equal(14, actual.Length);
            }
        }

        [ConditionalFact]
        public virtual async Task FromSqlRaw_queryable_multiple_composed()
        {
            using (var context = CreateContext())
            {
                var actual
                    = await (from c in context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers]"))
                             from o in context.Set<Order>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Orders]"))
                             where c.CustomerID == o.CustomerID
                             select new
                             {
                                 c,
                                 o
                             })
                        .ToArrayAsync();

                Assert.Equal(830, actual.Length);
            }
        }

        [ConditionalFact]
        public virtual async Task FromSqlRaw_queryable_multiple_composed_with_closure_parameters()
        {
            var startDate = new DateTime(1997, 1, 1);
            var endDate = new DateTime(1998, 1, 1);

            using (var context = CreateContext())
            {
                var actual
                    = await (from c in context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers]"))
                             from o in context.Set<Order>().FromSqlRaw(
                                 NormalizeDelimetersInRawString("SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {0} AND {1}"), startDate, endDate)
                             where c.CustomerID == o.CustomerID
                             select new
                             {
                                 c,
                                 o
                             })
                        .ToArrayAsync();

                Assert.Equal(411, actual.Length);
            }
        }

        [ConditionalFact]
        public virtual async Task FromSqlRaw_queryable_multiple_composed_with_parameters_and_closure_parameters()
        {
            var city = "London";
            var startDate = new DateTime(1997, 1, 1);
            var endDate = new DateTime(1998, 1, 1);

            using (var context = CreateContext())
            {
                var actual
                    = await (from c in context.Set<Customer>().FromSqlRaw(
                                 NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), city)
                             from o in context.Set<Order>().FromSqlRaw(
                                 NormalizeDelimetersInRawString("SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {0} AND {1}"), startDate, endDate)
                             where c.CustomerID == o.CustomerID
                             select new
                             {
                                 c,
                                 o
                             })
                        .ToArrayAsync();

                Assert.Equal(25, actual.Length);
            }
        }

        [ConditionalFact]
        public virtual async Task FromSqlRaw_queryable_multiple_line_query()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>().FromSqlRaw(
                        NormalizeDelimetersInRawString(
                            @"SELECT *
FROM [Customers]
WHERE [City] = 'London'"))
                    .ToArrayAsync();

                Assert.Equal(6, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
            }
        }

        [ConditionalFact]
        public virtual async Task FromSqlRaw_queryable_composed_multiple_line_query()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>().FromSqlRaw(
                        NormalizeDelimetersInRawString(
                            @"SELECT *
FROM [Customers]"))
                    .Where(c => c.City == "London")
                    .ToArrayAsync();

                Assert.Equal(6, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
            }
        }

        [ConditionalFact]
        public virtual async Task FromSqlRaw_queryable_with_parameters()
        {
            var city = "London";
            var contactTitle = "Sales Representative";

            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>().FromSqlRaw(
                        NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [City] = {0} AND [ContactTitle] = {1}"), city, contactTitle)
                    .ToArrayAsync();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));
            }
        }

        [ConditionalFact]
        public virtual async Task FromSqlRaw_queryable_with_parameters_and_closure()
        {
            var city = "London";
            var contactTitle = "Sales Representative";

            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>().FromSqlRaw(
                        NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), city)
                    .Where(c => c.ContactTitle == contactTitle)
                    .ToArrayAsync();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));
            }
        }

        [ConditionalFact]
        public virtual async Task FromSqlRaw_queryable_simple_cache_key_includes_query_string()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [City] = 'London'"))
                    .ToArrayAsync();

                Assert.Equal(6, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));

                actual = await context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [City] = 'Seattle'"))
                    .ToArrayAsync();

                Assert.Equal(1, actual.Length);
                Assert.True(actual.All(c => c.City == "Seattle"));
            }
        }

        [ConditionalFact]
        public virtual async Task FromSqlRaw_queryable_with_parameters_cache_key_includes_parameters()
        {
            var city = "London";
            var contactTitle = "Sales Representative";
            var sql = "SELECT * FROM [Customers] WHERE [City] = {0} AND [ContactTitle] = {1}";

            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString(sql), city, contactTitle)
                    .ToArrayAsync();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));

                city = "Madrid";
                contactTitle = "Accounting Manager";

                actual = await context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString(sql), city, contactTitle)
                    .ToArrayAsync();

                Assert.Equal(2, actual.Length);
                Assert.True(actual.All(c => c.City == "Madrid"));
                Assert.True(actual.All(c => c.ContactTitle == "Accounting Manager"));
            }
        }

        [ConditionalFact]
        public virtual async Task FromSqlRaw_queryable_simple_as_no_tracking_not_composed()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers]"))
                    .AsNoTracking()
                    .ToArrayAsync();

                Assert.Equal(91, actual.Length);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual async Task FromSqlRaw_queryable_simple_projection_not_composed()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers]"))
                    .Select(
                        c => new
                        {
                            c.CustomerID,
                            c.City
                        })
                    .AsNoTracking()
                    .ToArrayAsync();

                Assert.Equal(91, actual.Length);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual async Task FromSqlRaw_queryable_simple_include()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers]"))
                    .Include(c => c.Orders)
                    .ToArrayAsync();

                Assert.Equal(830, actual.SelectMany(c => c.Orders).Count());
            }
        }

        [ConditionalFact]
        public virtual async Task FromSqlRaw_queryable_simple_composed_include()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers]"))
                    .Include(c => c.Orders)
                    .Where(c => c.City == "London")
                    .ToArrayAsync();

                Assert.Equal(46, actual.SelectMany(c => c.Orders).Count());
            }
        }

        [ConditionalFact]
        public virtual async Task FromSqlRaw_annotations_do_not_affect_successive_calls()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Customers
                    .FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [ContactName] LIKE '%z%'"))
                    .ToArrayAsync();

                Assert.Equal(14, actual.Length);

                actual = await context.Customers
                    .ToArrayAsync();

                Assert.Equal(91, actual.Length);
            }
        }

        [ConditionalFact]
        public virtual async Task FromSqlRaw_composed_with_nullable_predicate()
        {
            using (var context = CreateContext())
            {
                var actual = await context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers]"))
                    .Where(c => c.ContactName == c.CompanyName)
                    .ToArrayAsync();

                Assert.Equal(0, actual.Length);
            }
        }

        [ConditionalFact]
        public virtual async Task Include_does_not_close_user_opened_connection_for_empty_result()
        {
            Fixture.TestStore.CloseConnection();
            using (var context = CreateContext())
            {
                var connection = context.Database.GetDbConnection();

                Assert.Equal(ConnectionState.Closed, connection.State);

                context.Database.OpenConnection();

                Assert.Equal(ConnectionState.Open, connection.State);

                var query = await context.Customers
                    .Include(v => v.Orders)
                    .Where(v => v.CustomerID == "MAMRFC")
                    .ToListAsync();

                Assert.Empty(query);
                Assert.Equal(ConnectionState.Open, connection.State);

                await context.Database.CloseConnectionAsync();

                Assert.Equal(ConnectionState.Closed, connection.State);
            }

            Fixture.TestStore.OpenConnection();
        }

        [ConditionalFact]
        public virtual async Task Include_closed_connection_opened_by_it_when_buffering()
        {
            Fixture.TestStore.CloseConnection();
            using (var context = CreateContext())
            {
                var connection = context.Database.GetDbConnection();

                Assert.Equal(ConnectionState.Closed, connection.State);

                var query = await context.Customers
                    .Include(v => v.Orders)
                    .Where(v => v.CustomerID == "ALFKI")
                    .ToListAsync();

                Assert.NotEmpty(query);
                Assert.Equal(ConnectionState.Closed, connection.State);
            }
        }

        private string NormalizeDelimetersInRawString(string sql)
            => Fixture.TestStore.NormalizeDelimetersInRawString(sql);

        private FormattableString NormalizeDelimetersInInterpolatedString(FormattableString sql)
            => Fixture.TestStore.NormalizeDelimetersInInterpolatedString(sql);

        protected NorthwindContext CreateContext() => Fixture.CreateContext();
    }
}
