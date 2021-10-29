// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Utilities;
using Xunit;

// ReSharper disable FormatStringProblem
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToConstant.Local
// ReSharper disable AccessToDisposedClosure
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class FromSqlQueryTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryRelationalFixture<NoopModelCustomizer>, new()
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly string _eol = Environment.NewLine;

        protected FromSqlQueryTestBase(TFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        protected TFixture Fixture { get; }

        public static IEnumerable<object[]> IsAsyncData = new[] { new object[] { false }, new object[] { true } };

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Bad_data_error_handling_invalid_cast_key(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Product>().FromSqlRaw(
                NormalizeDelimitersInRawString(
                    @"SELECT [ProductName] AS [ProductID], [ProductID] AS [ProductName], [SupplierID], [UnitPrice], [UnitsInStock], [Discontinued]
                      FROM [Products]"));

            Assert.Equal(
                CoreStrings.ErrorMaterializingPropertyInvalidCast("Product", "ProductID", typeof(int), typeof(string)),
                (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToList())).Message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Bad_data_error_handling_invalid_cast(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Product>().FromSqlRaw(
                NormalizeDelimitersInRawString(
                    @"SELECT [ProductID], [SupplierID] AS [UnitPrice], [ProductName], [SupplierID], [UnitsInStock], [Discontinued]
                      FROM [Products]"));

            Assert.Equal(
                CoreStrings.ErrorMaterializingPropertyInvalidCast("Product", "UnitPrice", typeof(decimal?), typeof(int)),
                (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToList())).Message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Bad_data_error_handling_invalid_cast_projection(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Product>().FromSqlRaw(
                NormalizeDelimitersInRawString(
                    @"SELECT [ProductID], [SupplierID] AS [UnitPrice], [ProductName], [UnitsInStock], [Discontinued]
                      FROM [Products]"))
                .Select(p => p.UnitPrice);

            Assert.Equal(
                RelationalStrings.ErrorMaterializingValueInvalidCast(typeof(decimal?), typeof(int)),
                (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToList())).Message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Bad_data_error_handling_invalid_cast_no_tracking(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Product>()
                .FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        @"SELECT [ProductName] AS [ProductID], [ProductID] AS [ProductName], [SupplierID], [UnitPrice], [UnitsInStock], [Discontinued]
                    FROM [Products]")).AsNoTracking();

            Assert.Equal(
                CoreStrings.ErrorMaterializingPropertyInvalidCast("Product", "ProductID", typeof(int), typeof(string)),
                (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToList())).Message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Bad_data_error_handling_null(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Product>().FromSqlRaw(
                NormalizeDelimitersInRawString(
                    @"SELECT [ProductID], [ProductName], [SupplierID], [UnitPrice], [UnitsInStock], NULL AS [Discontinued]
                FROM [Products]"));

            Assert.Equal(
                RelationalStrings.ErrorMaterializingPropertyNullReference("Product", "Discontinued", typeof(bool)),
                (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToList())).Message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Bad_data_error_handling_null_projection(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Product>()
                .FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        @"SELECT [ProductID], [ProductName], [SupplierID], [UnitPrice], [UnitsInStock], NULL AS [Discontinued]
                          FROM [Products]"))
                .Select(p => p.Discontinued);

            Assert.Equal(
                RelationalStrings.ErrorMaterializingValueNullReference(typeof(bool)),
                (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToList())).Message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Bad_data_error_handling_null_no_tracking(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Product>()
                .FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        @"SELECT [ProductID], [ProductName], [SupplierID], [UnitPrice], [UnitsInStock], NULL AS [Discontinued]
                          FROM [Products]")).AsNoTracking();

            Assert.Equal(
                RelationalStrings.ErrorMaterializingPropertyNullReference("Product", "Discontinued", typeof(bool)),
                (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToList())).Message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_simple(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Customer>()
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [ContactName] LIKE '%z%'"));

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(14, actual.Length);
            Assert.Equal(14, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_simple_columns_out_of_order(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        "SELECT [Region], [PostalCode], [Phone], [Fax], [CustomerID], [Country], [ContactTitle], [ContactName], [CompanyName], [City], [Address] FROM [Customers]"));

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(91, actual.Length);
            Assert.Equal(91, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_simple_columns_out_of_order_and_extra_columns(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        "SELECT [Region], [PostalCode], [PostalCode] AS [Foo], [Phone], [Fax], [CustomerID], [Country], [ContactTitle], [ContactName], [CompanyName], [City], [Address] FROM [Customers]"));

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(91, actual.Length);
            Assert.Equal(91, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_simple_columns_out_of_order_and_not_enough_columns_throws(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlRaw(
                NormalizeDelimitersInRawString(
                    "SELECT [PostalCode], [Phone], [Fax], [CustomerID], [Country], [ContactTitle], [ContactName], [CompanyName], [City], [Address] FROM [Customers]"));

            Assert.Equal(
                RelationalStrings.FromSqlMissingColumn("Region"),
                (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToList())).Message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task<string> FromSqlRaw_queryable_composed(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                .Where(c => c.ContactName.Contains("z"));

            var queryString = query.ToQueryString();

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(14, actual.Length);

            return queryString;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_composed_after_removing_whitespaces(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        _eol + "    " + _eol + _eol + _eol + "SELECT" + _eol + "* FROM [Customers]"))
                .Where(c => c.ContactName.Contains("z"));

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(14, actual.Length);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_composed_compiled(bool async)
        {
            if (async)
            {
                var query = EF.CompileAsyncQuery(
                    (NorthwindContext context) => context.Set<Customer>()
                        .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                        .Where(c => c.ContactName.Contains("z")));

                using (var context = CreateContext())
                {
                    var actual = await query(context).ToListAsync();

                    Assert.Equal(14, actual.Count);
                }
            }
            else
            {
                var query = EF.CompileQuery(
                    (NorthwindContext context) => context.Set<Customer>()
                        .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                        .Where(c => c.ContactName.Contains("z")));

                using (var context = CreateContext())
                {
                    var actual = query(context).ToArray();

                    Assert.Equal(14, actual.Length);
                }
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_composed_compiled_with_parameter(bool async)
        {
            if (async)
            {
                var query = EF.CompileAsyncQuery(
                    (NorthwindContext context) => context.Set<Customer>()
                        .FromSqlRaw(
                            NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = {0}"), "CONSH")
                        .Where(c => c.ContactName.Contains("z")));

                using (var context = CreateContext())
                {
                    var actual = await query(context).ToListAsync();

                    Assert.Single(actual);
                }
            }
            else
            {
                var query = EF.CompileQuery(
                    (NorthwindContext context) => context.Set<Customer>()
                        .FromSqlRaw(
                            NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = {0}"), "CONSH")
                        .Where(c => c.ContactName.Contains("z")));

                using (var context = CreateContext())
                {
                    var actual = query(context).ToArray();

                    Assert.Single(actual);
                }
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_composed_compiled_with_DbParameter(bool async)
        {
            if (async)
            {
                var query = EF.CompileAsyncQuery(
                    (NorthwindContext context) => context.Set<Customer>()
                        .FromSqlRaw(
                            NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = @customer"),
                            CreateDbParameter("customer", "CONSH"))
                        .Where(c => c.ContactName.Contains("z")));

                using (var context = CreateContext())
                {
                    var actual = await query(context).ToListAsync();

                    Assert.Single(actual);
                }
            }
            else
            {
                var query = EF.CompileQuery(
                    (NorthwindContext context) => context.Set<Customer>()
                        .FromSqlRaw(
                            NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = @customer"),
                            CreateDbParameter("customer", "CONSH"))
                        .Where(c => c.ContactName.Contains("z")));

                using (var context = CreateContext())
                {
                    var actual = query(context).ToArray();

                    Assert.Single(actual);
                }
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_composed_compiled_with_nameless_DbParameter(bool async)
        {
            if (async)
            {
                var query = EF.CompileAsyncQuery(
                    (NorthwindContext context) => context.Set<Customer>()
                        .FromSqlRaw(
                            NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = {0}"),
                            CreateDbParameter(null, "CONSH"))
                        .Where(c => c.ContactName.Contains("z")));

                using (var context = CreateContext())
                {
                    var actual = await query(context).ToListAsync();

                    Assert.Single(actual);
                }
            }
            else
            {
                var query = EF.CompileQuery(
                    (NorthwindContext context) => context.Set<Customer>()
                        .FromSqlRaw(
                            NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = {0}"),
                            CreateDbParameter(null, "CONSH"))
                        .Where(c => c.ContactName.Contains("z")));

                using (var context = CreateContext())
                {
                    var actual = query(context).ToArray();

                    Assert.Single(actual);
                }
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_composed_contains(bool async)
        {
            using var context = CreateContext();
            var query = from c in context.Set<Customer>()
                        where context.Orders.FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Orders]"))
                             .Select(o => o.CustomerID)
                             .Contains(c.CustomerID)
                        select c;

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(89, actual.Length);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_composed_contains2(bool async)
        {
            using var context = CreateContext();
            var query = from c in context.Set<Customer>()
                        where
                            c.CustomerID == "ALFKI"
                            && context.Orders.FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Orders]"))
                                .Select(o => o.CustomerID)
                                .Contains(c.CustomerID)
                        select c;

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Single(actual);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_multiple_composed(bool async)
        {
            using var context = CreateContext();
            var query = from c in context.Set<Customer>().FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                        from o in context.Set<Order>().FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Orders]"))
                        where c.CustomerID == o.CustomerID
                        select new { c, o };

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(830, actual.Length);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_multiple_composed_with_closure_parameters(bool async)
        {
            var startDate = new DateTime(1997, 1, 1);
            var endDate = new DateTime(1998, 1, 1);

            using var context = CreateContext();
            var query = from c in context.Set<Customer>().FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                        from o in context.Set<Order>().FromSqlRaw(
                            NormalizeDelimitersInRawString("SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {0} AND {1}"),
                            startDate,
                            endDate)
                        where c.CustomerID == o.CustomerID
                        select new { c, o };

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(411, actual.Length);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_multiple_composed_with_parameters_and_closure_parameters(bool async)
        {
            var city = "London";
            var startDate = new DateTime(1997, 1, 1);
            var endDate = new DateTime(1998, 1, 1);

            using var context = CreateContext();
            var query = from c in context.Set<Customer>().FromSqlRaw(
                            NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), city)
                        from o in context.Set<Order>().FromSqlRaw(
                            NormalizeDelimitersInRawString("SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {0} AND {1}"),
                            startDate,
                            endDate)
                        where c.CustomerID == o.CustomerID
                        select new { c, o };

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(25, actual.Length);

            city = "Berlin";
            startDate = new DateTime(1998, 4, 1);
            endDate = new DateTime(1998, 5, 1);

            query = (from c in context.Set<Customer>().FromSqlRaw(
                        NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), city)
                     from o in context.Set<Order>().FromSqlRaw(
                         NormalizeDelimitersInRawString("SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {0} AND {1}"),
                         startDate,
                         endDate)
                     where c.CustomerID == o.CustomerID
                     select new { c, o });

            actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Single(actual);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_multiple_line_query(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        @"SELECT *
FROM [Customers]
WHERE [City] = 'London'"));

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(6, actual.Length);
            Assert.True(actual.All(c => c.City == "London"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_composed_multiple_line_query(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        @"SELECT *
FROM [Customers]"))
                .Where(c => c.City == "London");

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(6, actual.Length);
            Assert.True(actual.All(c => c.City == "London"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_with_parameters(bool async)
        {
            var city = "London";
            var contactTitle = "Sales Representative";

            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlRaw(
                    NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0} AND [ContactTitle] = {1}"), city,
                    contactTitle);

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(3, actual.Length);
            Assert.True(actual.All(c => c.City == "London"));
            Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_with_parameters_inline(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlRaw(
                    NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0} AND [ContactTitle] = {1}"), "London",
                    "Sales Representative");

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(3, actual.Length);
            Assert.True(actual.All(c => c.City == "London"));
            Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlInterpolated_queryable_with_parameters_interpolated(bool async)
        {
            var city = "London";
            var contactTitle = "Sales Representative";

            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlInterpolated(
                    NormalizeDelimitersInInterpolatedString(
                        $"SELECT * FROM [Customers] WHERE [City] = {city} AND [ContactTitle] = {contactTitle}"));

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(3, actual.Length);
            Assert.True(actual.All(c => c.City == "London"));
            Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlInterpolated_queryable_with_parameters_inline_interpolated(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlInterpolated(
                    NormalizeDelimitersInInterpolatedString(
                        $"SELECT * FROM [Customers] WHERE [City] = {"London"} AND [ContactTitle] = {"Sales Representative"}"));

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(3, actual.Length);
            Assert.True(actual.All(c => c.City == "London"));
            Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlInterpolated_queryable_multiple_composed_with_parameters_and_closure_parameters_interpolated(bool async)
        {
            var city = "London";
            var startDate = new DateTime(1997, 1, 1);
            var endDate = new DateTime(1998, 1, 1);

            using var context = CreateContext();
            var query
                = from c in context.Set<Customer>().FromSqlRaw(
                       NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), city)
                  from o in context.Set<Order>().FromSqlInterpolated(
                      NormalizeDelimitersInInterpolatedString(
                          $"SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {startDate} AND {endDate}"))
                  where c.CustomerID == o.CustomerID
                  select new { c, o };

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(25, actual.Length);

            city = "Berlin";
            startDate = new DateTime(1998, 4, 1);
            endDate = new DateTime(1998, 5, 1);

            query
                = (from c in context.Set<Customer>().FromSqlRaw(
                       NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), city)
                   from o in context.Set<Order>().FromSqlInterpolated(
                       NormalizeDelimitersInInterpolatedString(
                           $"SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {startDate} AND {endDate}"))
                   where c.CustomerID == o.CustomerID
                   select new { c, o });

            actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Single(actual);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_with_null_parameter(bool async)
        {
            uint? reportsTo = null;

            using var context = CreateContext();
            var query = context.Set<Employee>().FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        // ReSharper disable once ExpressionIsAlwaysNull
                        "SELECT * FROM [Employees] WHERE [ReportsTo] = {0} OR ([ReportsTo] IS NULL AND {0} IS NULL)"), reportsTo);

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Single(actual);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task<string> FromSqlRaw_queryable_with_parameters_and_closure(bool async)
        {
            var city = "London";
            var contactTitle = "Sales Representative";

            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlRaw(
                    NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), city)
                .Where(c => c.ContactTitle == contactTitle);
            var queryString = query.ToQueryString();

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(3, actual.Length);
            Assert.True(actual.All(c => c.City == "London"));
            Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));

            return queryString;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_simple_cache_key_includes_query_string(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Customer>()
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = 'London'"));

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(6, actual.Length);
            Assert.True(actual.All(c => c.City == "London"));

            query = context.Set<Customer>()
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = 'Seattle'"));

            actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Single(actual);
            Assert.True(actual.All(c => c.City == "Seattle"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_with_parameters_cache_key_includes_parameters(bool async)
        {
            var city = "London";
            var contactTitle = "Sales Representative";
            var sql = "SELECT * FROM [Customers] WHERE [City] = {0} AND [ContactTitle] = {1}";

            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlRaw(NormalizeDelimitersInRawString(sql), city, contactTitle);

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(3, actual.Length);
            Assert.True(actual.All(c => c.City == "London"));
            Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));

            city = "Madrid";
            contactTitle = "Accounting Manager";

            query = context.Set<Customer>().FromSqlRaw(NormalizeDelimitersInRawString(sql), city, contactTitle);

            actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(2, actual.Length);
            Assert.True(actual.All(c => c.City == "Madrid"));
            Assert.True(actual.All(c => c.ContactTitle == "Accounting Manager"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_simple_as_no_tracking_not_composed(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                .AsNoTracking();

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(91, actual.Length);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_simple_projection_composed(bool async)
        {
            using var context = CreateContext();
            var boolMapping = (RelationalTypeMapping)context.GetService<ITypeMappingSource>().FindMapping(typeof(bool));
            var query = context.Set<Product>().FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        @"SELECT *
FROM [Products]
WHERE [Discontinued] <> "
                        + boolMapping.GenerateSqlLiteral(true)
                        + @"
AND (([UnitsInStock] + [UnitsOnOrder]) < [ReorderLevel])"))
                .Select(p => p.ProductName);

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(2, actual.Length);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_simple_include(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                .Include(c => c.Orders);

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(830, actual.SelectMany(c => c.Orders).Count());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_simple_composed_include(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                .Include(c => c.Orders)
                .Where(c => c.City == "London");

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(46, actual.SelectMany(c => c.Orders).Count());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_annotations_do_not_affect_successive_calls(bool async)
        {
            using var context = CreateContext();
            var query = context.Customers
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [ContactName] LIKE '%z%'"));

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(14, actual.Length);

            query = context.Customers;
            actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(91, actual.Length);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_composed_with_nullable_predicate(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                .Where(c => c.ContactName == c.CompanyName);

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Empty(actual);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_with_dbParameter(bool async)
        {
            using var context = CreateContext();
            var parameter = CreateDbParameter("@city", "London");

            var query = context.Customers.FromSqlRaw(
                    NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = @city"), parameter);

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(6, actual.Length);
            Assert.True(actual.All(c => c.City == "London"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_with_dbParameter_without_name_prefix(bool async)
        {
            using var context = CreateContext();
            var parameter = CreateDbParameter("city", "London");

            var query = context.Customers.FromSqlRaw(
                    NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = @city"), parameter);

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(6, actual.Length);
            Assert.True(actual.All(c => c.City == "London"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_with_dbParameter_mixed(bool async)
        {
            using var context = CreateContext();
            var city = "London";
            var title = "Sales Representative";

            var titleParameter = CreateDbParameter("@title", title);

            var query = context.Customers.FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        "SELECT * FROM [Customers] WHERE [City] = {0} AND [ContactTitle] = @title"), city, titleParameter);

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(3, actual.Length);
            Assert.True(actual.All(c => c.City == "London"));
            Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));

            var cityParameter = CreateDbParameter("@city", city);

            query = context.Customers.FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        "SELECT * FROM [Customers] WHERE [City] = @city AND [ContactTitle] = {1}"), cityParameter, title);

            actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(3, actual.Length);
            Assert.True(actual.All(c => c.City == "London"));
            Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_does_not_close_user_opened_connection_for_empty_result(bool async)
        {
            Fixture.TestStore.CloseConnection();
            using (var context = CreateContext())
            {
                var connection = context.Database.GetDbConnection();

                Assert.Equal(ConnectionState.Closed, connection.State);

                context.Database.OpenConnection();

                Assert.Equal(ConnectionState.Open, connection.State);

                var query = context.Customers
                    .Include(v => v.Orders)
                    .Where(v => v.CustomerID == "MAMRFC");

                var actual = async
                    ? await query.ToArrayAsync()
                    : query.ToArray();

                Assert.Empty(query);
                Assert.Equal(ConnectionState.Open, connection.State);

                context.Database.CloseConnection();

                Assert.Equal(ConnectionState.Closed, connection.State);
            }

            Fixture.TestStore.OpenConnection();
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_with_db_parameters_called_multiple_times(bool async)
        {
            using var context = CreateContext();
            var parameter = CreateDbParameter("@id", "ALFKI");

            var query = context.Customers.FromSqlRaw(
                NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = @id"), parameter);

            // ReSharper disable PossibleMultipleEnumeration
            var result1 = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Single(result1);

            var result2 = async
                ? await query.ToArrayAsync()
                : query.ToArray();
            // ReSharper restore PossibleMultipleEnumeration

            Assert.Single(result2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_with_SelectMany_and_include(bool async)
        {
            using var context = CreateContext();
            var query = from c1 in context.Set<Customer>()
                            .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = 'ALFKI'"))
                        from c2 in context.Set<Customer>().FromSqlRaw(
                                NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = 'AROUT'"))
                            .Include(c => c.Orders)
                        select new { c1, c2 };

            var result = async
                ? await query.ToArrayAsync()
                : query.ToArray();
            Assert.Single(result);

            var customers1 = result.Select(r => r.c1);
            var customers2 = result.Select(r => r.c2);
            foreach (var customer1 in customers1)
            {
                Assert.Null(customer1.Orders);
            }

            foreach (var customer2 in customers2)
            {
                Assert.NotNull(customer2.Orders);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_with_join_and_include(bool async)
        {
            using var context = CreateContext();
            var query = from c in context.Set<Customer>()
                            .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = 'ALFKI'"))
                        join o in context.Set<Order>().FromSqlRaw(
                                    NormalizeDelimitersInRawString("SELECT * FROM [Orders] WHERE [OrderID] <> 1"))
                                .Include(o => o.OrderDetails)
                            on c.CustomerID equals o.CustomerID
                        select new { c, o };

            var result = async
                ? await query.ToListAsync()
                : query.ToList();

            Assert.Equal(6, result.Count);

            var orders = result.Select(r => r.o);
            foreach (var order in orders)
            {
                Assert.NotNull(order.OrderDetails);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Include_closed_connection_opened_by_it_when_buffering(bool async)
        {
            Fixture.TestStore.CloseConnection();
            using var context = CreateContext();
            var connection = context.Database.GetDbConnection();

            Assert.Equal(ConnectionState.Closed, connection.State);

            var query = context.Customers
                .Include(v => v.Orders)
                .Where(v => v.CustomerID == "ALFKI");

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.NotEmpty(query);
            Assert.Equal(ConnectionState.Closed, connection.State);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlInterpolated_with_inlined_db_parameter(bool async)
        {
            using var context = CreateContext();
            var parameter = CreateDbParameter("@somename", "ALFKI");

            var query = context.Customers
                .FromSqlInterpolated(
                    NormalizeDelimitersInInterpolatedString($"SELECT * FROM [Customers] WHERE [CustomerID] = {parameter}"));

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Single(actual);
            Assert.True(actual.All(c => c.City == "Berlin"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlInterpolated_with_inlined_db_parameter_without_name_prefix(bool async)
        {
            using var context = CreateContext();
            var parameter = CreateDbParameter("somename", "ALFKI");

            var query = context.Customers
                .FromSqlInterpolated(
                    NormalizeDelimitersInInterpolatedString($"SELECT * FROM [Customers] WHERE [CustomerID] = {parameter}"));

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Single(actual);
            Assert.True(actual.All(c => c.City == "Berlin"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlInterpolated_parameterization_issue_12213(bool async)
        {
            using var context = CreateContext();
            var min = 10300;
            var max = 10400;

            var query1 = context.Orders
                .FromSqlInterpolated(NormalizeDelimitersInInterpolatedString($"SELECT * FROM [Orders] WHERE [OrderID] >= {min}"))
                .Select(i => i.OrderID);

            var actual1 = async
                ? await query1.ToArrayAsync()
                : query1.ToArray();

            var query2 = context.Orders
                .Where(o => o.OrderID <= max && query1.Contains(o.OrderID))
                .Select(o => o.OrderID);

            var actual2 = async
                ? await query2.ToArrayAsync()
                : query2.ToArray();

            var query3 = context.Orders
                .Where(
                    o => o.OrderID <= max
                        && context.Orders
                            .FromSqlInterpolated(
                                NormalizeDelimitersInInterpolatedString($"SELECT * FROM [Orders] WHERE [OrderID] >= {min}"))
                            .Select(i => i.OrderID)
                            .Contains(o.OrderID))
                .Select(o => o.OrderID);

            var actual3 = async
                ? await query3.ToArrayAsync()
                : query3.ToArray();
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_does_not_parameterize_interpolated_string(bool async)
        {
            using var context = CreateContext();
            var tableName = "Orders";
            var max = 10250;
            var query = context.Orders.FromSqlRaw(
                    NormalizeDelimitersInRawString($"SELECT * FROM [{tableName}] WHERE [OrderID] < {{0}}"), max);

            var actual = async
                ? await query.ToListAsync()
                : query.ToList();

            Assert.Equal(2, actual.Count);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Entity_equality_through_fromsql(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Order>()
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Orders]"))
                .Where(o => o.Customer == new Customer { CustomerID = "VINET" });

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(5, actual.Length);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_with_set_operation(bool async)
        {
            using var context = CreateContext();

            var query = context.Set<Customer>()
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = 'London'"))
                .Concat(
                    context.Set<Customer>()
                        .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = 'Berlin'")));

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(7, actual.Length);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Keyless_entity_with_all_nulls(bool async)
        {
            using var context = CreateContext();

            var query = context.Set<OrderQuery>()
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT NULL AS [CustomerID] FROM [Customers] WHERE [City] = 'Berlin'"))
                .IgnoreQueryFilters();

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.NotNull(Assert.Single(actual));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSql_used_twice_without_parameters(bool async)
        {
            using var context = CreateContext();

            var query = context.Set<OrderQuery>()
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT 'ALFKI' AS [CustomerID]"))
                .IgnoreQueryFilters();

            var result1 = async
                ? await query.AnyAsync()
                : query.Any();

            Assert.Equal(
                RelationalStrings.QueryFromSqlInsideExists,
                async
                ? (await Assert.ThrowsAsync<InvalidOperationException>(() => query.AnyAsync())).Message
                : Assert.Throws<InvalidOperationException>(() => query.Any()).Message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSql_used_twice_with_parameters(bool async)
        {
            using var context = CreateContext();

            var query = context.Set<OrderQuery>()
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT {0} AS [CustomerID]"), "ALFKI")
                .IgnoreQueryFilters();

            var result1 = async
                ? await query.AnyAsync()
                : query.Any();

            Assert.Equal(
                RelationalStrings.QueryFromSqlInsideExists,
                async
                ? (await Assert.ThrowsAsync<InvalidOperationException>(() => query.AnyAsync())).Message
                : Assert.Throws<InvalidOperationException>(() => query.Any()).Message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSql_Count_used_twice_without_parameters(bool async)
        {
            using var context = CreateContext();

            var query = context.Set<OrderQuery>()
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT 'ALFKI' AS [CustomerID]"))
                .IgnoreQueryFilters();

            var result1 = async
                ? await query.CountAsync() > 0
                : query.Count() > 0;

            var result2 = async
                ? await query.CountAsync() > 0
                : query.Count() > 0;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSql_Count_used_twice_with_parameters(bool async)
        {
            using var context = CreateContext();

            var query = context.Set<OrderQuery>()
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT {0} AS [CustomerID]"), "ALFKI")
                .IgnoreQueryFilters();

            var result1 = async
                ? await query.CountAsync() > 0
                : query.Count() > 0;

            var result2 = async
                ? await query.CountAsync() > 0
                : query.Count() > 0;
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Line_endings_after_Select(bool async)
        {
            using var context = CreateContext();

            var query = context.Set<Customer>()
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT" + Environment.NewLine + "* FROM [Customers]"))
                .Where(e => e.City == "Seattle");

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.NotNull(actual);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSql_with_db_parameter_in_split_query(bool async)
        {
            using var context = CreateContext();

            var query = context.Set<Customer>()
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = {0}"),
                    CreateDbParameter("customerID", "ALFKI"))
                .Include(e => e.Orders)
                .ThenInclude(o => o.OrderDetails)
                .AsSplitQuery();

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            var customer = Assert.Single(actual);
            Assert.Equal(6, customer.Orders.Count);
            Assert.Equal(12, customer.Orders.SelectMany(e => e.OrderDetails).Count());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_queryable_simple_projection_not_composed(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                .Select(
                    c => new { c.CustomerID, c.City })
                .AsNoTracking();

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(91, actual.Length);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_in_subquery_with_dbParameter(bool async)
        {
            using var context = CreateContext();
            var query = context.Orders.Where(
                    o =>
                        context.Customers
                            .FromSqlRaw(
                                NormalizeDelimitersInRawString(@"SELECT * FROM [Customers] WHERE [City] = @city"),
                                // ReSharper disable once FormatStringProblem
                                CreateDbParameter("@city", "London"))
                            .Select(c => c.CustomerID)
                            .Contains(o.CustomerID));

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(46, actual.Length);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_in_subquery_with_positional_dbParameter_without_name(bool async)
        {
            using var context = CreateContext();
            var query = context.Orders.Where(
                    o =>
                        context.Customers
                            .FromSqlRaw(
                                NormalizeDelimitersInRawString(@"SELECT * FROM [Customers] WHERE [City] = {0}"),
                                // ReSharper disable once FormatStringProblem
                                CreateDbParameter(null, "London"))
                            .Select(c => c.CustomerID)
                            .Contains(o.CustomerID));

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(46, actual.Length);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_in_subquery_with_positional_dbParameter_with_name(bool async)
        {
            using var context = CreateContext();
            var query = context.Orders.Where(
                    o =>
                        context.Customers
                            .FromSqlRaw(
                                NormalizeDelimitersInRawString(@"SELECT * FROM [Customers] WHERE [City] = {0}"),
                                // ReSharper disable once FormatStringProblem
                                CreateDbParameter("@city", "London"))
                            .Select(c => c.CustomerID)
                            .Contains(o.CustomerID));

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(46, actual.Length);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_with_dbParameter_mixed_in_subquery(bool async)
        {
            using var context = CreateContext();
            const string city = "London";
            const string title = "Sales Representative";

            var query = context.Orders.Where(
                    o =>
                        context.Customers
                            .FromSqlRaw(
                                NormalizeDelimitersInRawString(@"SELECT * FROM [Customers] WHERE [City] = {0} AND [ContactTitle] = @title"),
                                city,
                                // ReSharper disable once FormatStringProblem
                                CreateDbParameter("@title", title))
                            .Select(c => c.CustomerID)
                            .Contains(o.CustomerID));

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(26, actual.Length);

            query = context.Orders.Where(
                    o =>
                        context.Customers
                            .FromSqlRaw(
                                NormalizeDelimitersInRawString(@"SELECT * FROM [Customers] WHERE [City] = @city AND [ContactTitle] = {1}"),
                                // ReSharper disable once FormatStringProblem
                                CreateDbParameter("@city", city),
                                title)
                            .Select(c => c.CustomerID)
                            .Contains(o.CustomerID));

            actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(26, actual.Length);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task FromSqlRaw_composed_with_common_table_expression(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Customer>()
                .FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        @"WITH [Customers2] AS (
    SELECT * FROM [Customers]
)
SELECT * FROM [Customers2]"))
                .Where(c => c.ContactName.Contains("z"));

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(14, actual.Length);
            Assert.Equal(14, context.ChangeTracker.Entries().Count());
        }

        protected string NormalizeDelimitersInRawString(string sql)
            => Fixture.TestStore.NormalizeDelimitersInRawString(sql);

        protected FormattableString NormalizeDelimitersInInterpolatedString(FormattableString sql)
            => Fixture.TestStore.NormalizeDelimitersInInterpolatedString(sql);

        protected abstract DbParameter CreateDbParameter(string name, object value);

        protected NorthwindContext CreateContext()
            => Fixture.CreateContext();
    }
}
