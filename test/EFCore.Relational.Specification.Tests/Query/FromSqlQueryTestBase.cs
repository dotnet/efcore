// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
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

        [ConditionalFact(Skip = "#15751")]
        public virtual void Bad_data_error_handling_invalid_cast_key()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.ErrorMaterializingPropertyInvalidCast("Product", "ProductID", typeof(int), typeof(string)),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            context.Set<Product>().FromSqlRaw(
                                    NormalizeDelimetersInRawString(
                                        @"SELECT [ProductID] AS [ProductName], [ProductName] AS [ProductID], [SupplierID], [UnitPrice], [UnitsInStock], [Discontinued]
                               FROM [Products]"))
                                .ToList()).Message);
            }
        }

        [ConditionalFact(Skip = "#15751")]
        public virtual void Bad_data_error_handling_invalid_cast()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.ErrorMaterializingPropertyInvalidCast("Product", "UnitPrice", typeof(decimal?), typeof(int)),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            context.Set<Product>().FromSqlRaw(
                                    NormalizeDelimetersInRawString(
                                        @"SELECT [ProductID], [SupplierID] AS [UnitPrice], [ProductName], [SupplierID], [UnitsInStock], [Discontinued]
                               FROM [Products]"))
                                .ToList()).Message);
            }
        }

        [ConditionalFact(Skip = "#15751")]
        public virtual void Bad_data_error_handling_invalid_cast_projection()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.ErrorMaterializingPropertyInvalidCast("Product", "UnitPrice", typeof(decimal?), typeof(int)),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            context.Set<Product>().FromSqlRaw(
                                    NormalizeDelimetersInRawString(
                                        @"SELECT [ProductID], [SupplierID] AS [UnitPrice], [ProductName], [UnitsInStock], [Discontinued]
                               FROM [Products]"))
                                .Select(p => p.UnitPrice)
                                .ToList()).Message);
            }
        }

        [ConditionalFact(Skip = "#15751")]
        public virtual void Bad_data_error_handling_invalid_cast_no_tracking()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.ErrorMaterializingPropertyInvalidCast("Product", "ProductID", typeof(int), typeof(string)),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            context.Set<Product>()
                                .FromSqlRaw(
                                    NormalizeDelimetersInRawString(
                                        @"SELECT [ProductID] AS [ProductName], [ProductName] AS [ProductID], [SupplierID], [UnitPrice], [UnitsInStock], [Discontinued]
                               FROM [Products]")).AsNoTracking()
                                .ToList()).Message);
            }
        }

        [ConditionalFact(Skip = "#15751")]
        public virtual void Bad_data_error_handling_null()
        {
            using (var context = CreateContext())
            {
                context.Set<Product>().FromSqlRaw(
                        NormalizeDelimetersInRawString(
                            @"SELECT [ProductID], [ProductName], [SupplierID], [UnitPrice], [UnitsInStock], NULL AS [Discontinued]
                               FROM [Products]"))
                    .ToList();
                Assert.Equal(
                    CoreStrings.ErrorMaterializingPropertyNullReference("Product", "Discontinued", typeof(bool)),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            context.Set<Product>().FromSqlRaw(
                                    NormalizeDelimetersInRawString(
                                        @"SELECT [ProductID], [ProductName], [SupplierID], [UnitPrice], [UnitsInStock], NULL AS [Discontinued]
                               FROM [Products]"))
                                .ToList()).Message);
            }
        }

        [ConditionalFact(Skip = "#15751")]
        public virtual void Bad_data_error_handling_null_projection()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.ErrorMaterializingPropertyNullReference("Product", "Discontinued", typeof(bool)),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            context.Set<Product>().FromSqlRaw(
                                    NormalizeDelimetersInRawString(
                                        @"SELECT [ProductID], [ProductName], [SupplierID], [UnitPrice], [UnitsInStock], NULL AS [Discontinued]
                               FROM [Products]"))
                                .Select(p => p.Discontinued)
                                .ToList()).Message);
            }
        }

        [ConditionalFact(Skip = "#15751")]
        public virtual void Bad_data_error_handling_null_no_tracking()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.ErrorMaterializingPropertyNullReference("Product", "Discontinued", typeof(bool)),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            context.Set<Product>()
                                .FromSqlRaw(
                                    NormalizeDelimetersInRawString(
                                        @"SELECT [ProductID], [ProductName], [SupplierID], [UnitPrice], [UnitsInStock], NULL AS [Discontinued]
                               FROM [Products]")).AsNoTracking()
                                .ToList()).Message);
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_simple()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>()
                    .FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [ContactName] LIKE '%z%'"))
                    .ToArray();

                Assert.Equal(14, actual.Length);
                Assert.Equal(14, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_simple_columns_out_of_order()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>().FromSqlRaw(
                        NormalizeDelimetersInRawString(
                            "SELECT [Region], [PostalCode], [Phone], [Fax], [CustomerID], [Country], [ContactTitle], [ContactName], [CompanyName], [City], [Address] FROM [Customers]"))
                    .ToArray();

                Assert.Equal(91, actual.Length);
                Assert.Equal(91, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_simple_columns_out_of_order_and_extra_columns()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>().FromSqlRaw(
                        NormalizeDelimetersInRawString(
                            "SELECT [Region], [PostalCode], [PostalCode] AS [Foo], [Phone], [Fax], [CustomerID], [Country], [ContactTitle], [ContactName], [CompanyName], [City], [Address] FROM [Customers]"))
                    .ToArray();

                Assert.Equal(91, actual.Length);
                Assert.Equal(91, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact(Skip = "#15751")]
        public virtual void FromSqlRaw_queryable_simple_columns_out_of_order_and_not_enough_columns_throws()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    RelationalStrings.FromSqlMissingColumn("Region"),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Set<Customer>().FromSqlRaw(
                                NormalizeDelimetersInRawString(
                                    "SELECT [PostalCode], [Phone], [Fax], [CustomerID], [Country], [ContactTitle], [ContactName], [CompanyName], [City], [Address] FROM [Customers]"))
                            .ToArray()
                    ).Message);
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_composed()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers]"))
                    .Where(c => c.ContactName.Contains("z"))
                    .ToArray();

                Assert.Equal(14, actual.Length);
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_composed_after_removing_whitespaces()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>().FromSqlRaw(
                        NormalizeDelimetersInRawString(
                            _eol +
                            "    " + _eol +
                            _eol +
                            _eol +
                            "SELECT" + _eol +
                            "* FROM [Customers]"))
                    .Where(c => c.ContactName.Contains("z"))
                    .ToArray();

                Assert.Equal(14, actual.Length);
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_composed_compiled()
        {
            var query = EF.CompileQuery(
                (NorthwindContext context) => context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers]"))
                    .Where(c => c.ContactName.Contains("z")));

            using (var context = CreateContext())
            {
                var actual = query(context).ToArray();

                Assert.Equal(14, actual.Length);
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_composed_compiled_with_parameter()
        {
            var query = EF.CompileQuery(
                (NorthwindContext context) => context.Set<Customer>()
                    .FromSqlRaw(
                        NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = {0}"), "CONSH")
                    .Where(c => c.ContactName.Contains("z")));

            using (var context = CreateContext())
            {
                var actual = query(context).ToArray();

                Assert.Equal(1, actual.Length);
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_composed_compiled_with_DbParameter()
        {
            var query = EF.CompileQuery(
                (NorthwindContext context) => context.Set<Customer>()
                    .FromSqlRaw(
                        NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = @customer"),
                        CreateDbParameter("customer", "CONSH"))
                    .Where(c => c.ContactName.Contains("z")));

            using (var context = CreateContext())
            {
                var actual = query(context).ToArray();

                Assert.Equal(1, actual.Length);
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_composed_compiled_with_nameless_DbParameter()
        {
            var query = EF.CompileQuery(
                (NorthwindContext context) => context.Set<Customer>()
                    .FromSqlRaw(
                        NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = {0}"),
                        CreateDbParameter(null, "CONSH"))
                    .Where(c => c.ContactName.Contains("z")));

            using (var context = CreateContext())
            {
                var actual = query(context).ToArray();

                Assert.Equal(1, actual.Length);
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_composed_contains()
        {
            using (var context = CreateContext())
            {
                var actual
                    = (from c in context.Set<Customer>()
                       where context.Orders.FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Orders]"))
                           .Select(o => o.CustomerID)
                           .Contains(c.CustomerID)
                       select c)
                    .ToArray();

                Assert.Equal(89, actual.Length);
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_composed_contains2()
        {
            using (var context = CreateContext())
            {
                var actual
                    = (from c in context.Set<Customer>()
                       where
                           c.CustomerID == "ALFKI"
                           && context.Orders.FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Orders]"))
                               .Select(o => o.CustomerID)
                               .Contains(c.CustomerID)
                       select c)
                    .ToArray();

                Assert.Equal(1, actual.Length);
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_multiple_composed()
        {
            using (var context = CreateContext())
            {
                var actual
                    = (from c in context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers]"))
                       from o in context.Set<Order>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Orders]"))
                       where c.CustomerID == o.CustomerID
                       select new
                       {
                           c,
                           o
                       })
                    .ToArray();

                Assert.Equal(830, actual.Length);
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_multiple_composed_with_closure_parameters()
        {
            var startDate = new DateTime(1997, 1, 1);
            var endDate = new DateTime(1998, 1, 1);

            using (var context = CreateContext())
            {
                var actual
                    = (from c in context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers]"))
                       from o in context.Set<Order>().FromSqlRaw(
                           NormalizeDelimetersInRawString("SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {0} AND {1}"),
                           startDate,
                           endDate)
                       where c.CustomerID == o.CustomerID
                       select new
                       {
                           c,
                           o
                       })
                    .ToArray();

                Assert.Equal(411, actual.Length);
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_multiple_composed_with_parameters_and_closure_parameters()
        {
            var city = "London";
            var startDate = new DateTime(1997, 1, 1);
            var endDate = new DateTime(1998, 1, 1);

            using (var context = CreateContext())
            {
                var actual
                    = (from c in context.Set<Customer>().FromSqlRaw(
                           NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), city)
                       from o in context.Set<Order>().FromSqlRaw(
                           NormalizeDelimetersInRawString("SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {0} AND {1}"),
                           startDate,
                           endDate)
                       where c.CustomerID == o.CustomerID
                       select new
                       {
                           c,
                           o
                       })
                    .ToArray();

                Assert.Equal(25, actual.Length);

                city = "Berlin";
                startDate = new DateTime(1998, 4, 1);
                endDate = new DateTime(1998, 5, 1);

                actual
                    = (from c in context.Set<Customer>().FromSqlRaw(
                           NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), city)
                       from o in context.Set<Order>().FromSqlRaw(
                           NormalizeDelimetersInRawString("SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {0} AND {1}"),
                           startDate,
                           endDate)
                       where c.CustomerID == o.CustomerID
                       select new
                       {
                           c,
                           o
                       })
                    .ToArray();

                Assert.Equal(1, actual.Length);
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_multiple_line_query()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>().FromSqlRaw(
                        NormalizeDelimetersInRawString(
                            @"SELECT *
FROM [Customers]
WHERE [City] = 'London'"))
                    .ToArray();

                Assert.Equal(6, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_composed_multiple_line_query()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>().FromSqlRaw(
                        NormalizeDelimetersInRawString(
                            @"SELECT *
FROM [Customers]"))
                    .Where(c => c.City == "London")
                    .ToArray();

                Assert.Equal(6, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_with_parameters()
        {
            var city = "London";
            var contactTitle = "Sales Representative";

            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>().FromSqlRaw(
                        NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [City] = {0} AND [ContactTitle] = {1}"), city, contactTitle)
                    .ToArray();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_with_parameters_inline()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>().FromSqlRaw(
                        NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [City] = {0} AND [ContactTitle] = {1}"), "London",
                        "Sales Representative")
                    .ToArray();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));
            }
        }

        [ConditionalFact]
        public virtual void FromSqlInterpolated_queryable_with_parameters_interpolated()
        {
            var city = "London";
            var contactTitle = "Sales Representative";

            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>().FromSqlInterpolated(
                        NormalizeDelimetersInInterpolatedString(
                            $"SELECT * FROM [Customers] WHERE [City] = {city} AND [ContactTitle] = {contactTitle}"))
                    .ToArray();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));
            }
        }

        [ConditionalFact]
        public virtual void FromSqlInterpolated_queryable_with_parameters_inline_interpolated()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>().FromSqlInterpolated(
                        NormalizeDelimetersInInterpolatedString(
                            $"SELECT * FROM [Customers] WHERE [City] = {"London"} AND [ContactTitle] = {"Sales Representative"}"))
                    .ToArray();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));
            }
        }

        [ConditionalFact]
        public virtual void FromSqlInterpolated_queryable_multiple_composed_with_parameters_and_closure_parameters_interpolated()
        {
            var city = "London";
            var startDate = new DateTime(1997, 1, 1);
            var endDate = new DateTime(1998, 1, 1);

            using (var context = CreateContext())
            {
                var actual
                    = (from c in context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), city)
                       from o in context.Set<Order>().FromSqlInterpolated(
                           NormalizeDelimetersInInterpolatedString($"SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {startDate} AND {endDate}"))
                       where c.CustomerID == o.CustomerID
                       select new
                       {
                           c,
                           o
                       })
                    .ToArray();

                Assert.Equal(25, actual.Length);

                city = "Berlin";
                startDate = new DateTime(1998, 4, 1);
                endDate = new DateTime(1998, 5, 1);

                actual
                    = (from c in context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), city)
                       from o in context.Set<Order>().FromSqlInterpolated(
                           NormalizeDelimetersInInterpolatedString($"SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {startDate} AND {endDate}"))
                       where c.CustomerID == o.CustomerID
                       select new
                       {
                           c,
                           o
                       })
                    .ToArray();

                Assert.Equal(1, actual.Length);
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_with_null_parameter()
        {
            uint? reportsTo = null;

            using (var context = CreateContext())
            {
                var actual = context.Set<Employee>().FromSqlRaw(
                        NormalizeDelimetersInRawString(
                            // ReSharper disable once ExpressionIsAlwaysNull
                            "SELECT * FROM [Employees] WHERE [ReportsTo] = {0} OR ([ReportsTo] IS NULL AND {0} IS NULL)"), reportsTo)
                    .ToArray();

                Assert.Equal(1, actual.Length);
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_with_parameters_and_closure()
        {
            var city = "London";
            var contactTitle = "Sales Representative";

            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), city)
                    .Where(c => c.ContactTitle == contactTitle)
                    .ToArray();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_simple_cache_key_includes_query_string()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [City] = 'London'"))
                    .ToArray();

                Assert.Equal(6, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));

                actual = context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [City] = 'Seattle'"))
                    .ToArray();

                Assert.Equal(1, actual.Length);
                Assert.True(actual.All(c => c.City == "Seattle"));
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_with_parameters_cache_key_includes_parameters()
        {
            var city = "London";
            var contactTitle = "Sales Representative";
            var sql = "SELECT * FROM [Customers] WHERE [City] = {0} AND [ContactTitle] = {1}";

            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString(sql), city, contactTitle)
                    .ToArray();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));

                city = "Madrid";
                contactTitle = "Accounting Manager";

                actual = context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString(sql), city, contactTitle)
                    .ToArray();

                Assert.Equal(2, actual.Length);
                Assert.True(actual.All(c => c.City == "Madrid"));
                Assert.True(actual.All(c => c.ContactTitle == "Accounting Manager"));
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_simple_as_no_tracking_not_composed()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers]"))
                    .AsNoTracking()
                    .ToArray();

                Assert.Equal(91, actual.Length);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_simple_projection_composed()
        {
            using (var context = CreateContext())
            {
                var boolMapping = (RelationalTypeMapping)context.GetService<ITypeMappingSource>().FindMapping(typeof(bool));
                var actual = context.Set<Product>().FromSqlRaw(
                        NormalizeDelimetersInRawString(
                            @"SELECT *
FROM [Products]
WHERE [Discontinued] <> " + boolMapping.GenerateSqlLiteral(true) + @"
AND (([UnitsInStock] + [UnitsOnOrder]) < [ReorderLevel])"))
                    .Select(p => p.ProductName)
                    .ToArray();

                Assert.Equal(2, actual.Length);
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_simple_include()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers]"))
                    .Include(c => c.Orders)
                    .ToArray();

                Assert.Equal(830, actual.SelectMany(c => c.Orders).Count());
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_queryable_simple_composed_include()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers]"))
                    .Include(c => c.Orders)
                    .Where(c => c.City == "London")
                    .ToArray();

                Assert.Equal(46, actual.SelectMany(c => c.Orders).Count());
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_annotations_do_not_affect_successive_calls()
        {
            using (var context = CreateContext())
            {
                var actual = context.Customers.FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [ContactName] LIKE '%z%'"))
                    .ToArray();

                Assert.Equal(14, actual.Length);

                actual = context.Customers
                    .ToArray();

                Assert.Equal(91, actual.Length);
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_composed_with_nullable_predicate()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Customer>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers]"))
                    .Where(c => c.ContactName == c.CompanyName)
                    .ToArray();

                Assert.Equal(0, actual.Length);
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_with_dbParameter()
        {
            using (var context = CreateContext())
            {
                var parameter = CreateDbParameter("@city", "London");

                var actual = context.Customers.FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [City] = @city"), parameter)
                    .ToArray();

                Assert.Equal(6, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_with_dbParameter_without_name_prefix()
        {
            using (var context = CreateContext())
            {
                var parameter = CreateDbParameter("city", "London");

                var actual = context.Customers.FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [City] = @city"), parameter)
                    .ToArray();

                Assert.Equal(6, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_with_dbParameter_mixed()
        {
            using (var context = CreateContext())
            {
                var city = "London";
                var title = "Sales Representative";

                var titleParameter = CreateDbParameter("@title", title);

                var actual = context.Customers.FromSqlRaw(
                        NormalizeDelimetersInRawString(
                            "SELECT * FROM [Customers] WHERE [City] = {0} AND [ContactTitle] = @title"), city, titleParameter)
                    .ToArray();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));

                var cityParameter = CreateDbParameter("@city", city);

                actual = context.Customers.FromSqlRaw(
                        NormalizeDelimetersInRawString(
                            "SELECT * FROM [Customers] WHERE [City] = @city AND [ContactTitle] = {1}"), cityParameter, title)
                    .ToArray();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));
            }
        }

        [ConditionalFact]
        public virtual void Include_does_not_close_user_opened_connection_for_empty_result()
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
                    .Where(v => v.CustomerID == "MAMRFC")
                    .ToList();

                Assert.Empty(query);
                Assert.Equal(ConnectionState.Open, connection.State);

                context.Database.CloseConnection();

                Assert.Equal(ConnectionState.Closed, connection.State);
            }

            Fixture.TestStore.OpenConnection();
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_with_db_parameters_called_multiple_times()
        {
            using (var context = CreateContext())
            {
                var parameter = CreateDbParameter("@id", "ALFKI");

                var query = context.Customers.FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = @id"), parameter);

                // ReSharper disable PossibleMultipleEnumeration
                var result1 = query.ToList();

                Assert.Equal(1, result1.Count);

                var result2 = query.ToList();
                // ReSharper restore PossibleMultipleEnumeration

                Assert.Equal(1, result2.Count);
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_with_SelectMany_and_include()
        {
            using (var context = CreateContext())
            {
                var query = from c1 in context.Set<Customer>()
                                .FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = 'ALFKI'"))
                            from c2 in context.Set<Customer>().FromSqlRaw(
                                    NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = 'AROUT'"))
                                .Include(c => c.Orders)
                            select new
                            {
                                c1,
                                c2
                            };

                var result = query.ToList();
                Assert.Equal(1, result.Count);

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
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_with_join_and_include()
        {
            using (var context = CreateContext())
            {
                var query = from c in context.Set<Customer>()
                                .FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = 'ALFKI'"))
                            join o in context.Set<Order>().FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Orders] WHERE [OrderID] <> 1"))
                                    .Include(o => o.OrderDetails)
                                on c.CustomerID equals o.CustomerID
                            select new
                            {
                                c,
                                o
                            };

                var result = query.ToList();

                Assert.Equal(6, result.Count);

                var orders = result.Select(r => r.o);
                foreach (var order in orders)
                {
                    Assert.NotNull(order.OrderDetails);
                }
            }
        }

        [ConditionalFact]
        public virtual void Include_closed_connection_opened_by_it_when_buffering()
        {
            Fixture.TestStore.CloseConnection();
            using (var context = CreateContext())
            {
                var connection = context.Database.GetDbConnection();

                Assert.Equal(ConnectionState.Closed, connection.State);

                var query = context.Customers
                    .Include(v => v.Orders)
                    .Where(v => v.CustomerID == "ALFKI")
                    .ToList();

                Assert.NotEmpty(query);
                Assert.Equal(ConnectionState.Closed, connection.State);
            }
        }

        [ConditionalFact]
        public virtual void FromSqlInterpolated_with_inlined_db_parameter()
        {
            using (var context = CreateContext())
            {
                var parameter = CreateDbParameter("@somename", "ALFKI");

                var actual = context.Customers
                    .FromSqlInterpolated(NormalizeDelimetersInInterpolatedString($"SELECT * FROM [Customers] WHERE [CustomerID] = {parameter}"))
                    .ToList();

                Assert.Equal(1, actual.Count);
                Assert.True(actual.All(c => c.City == "Berlin"));
            }
        }

        [ConditionalFact]
        public virtual void FromSqlInterpolated_with_inlined_db_parameter_without_name_prefix()
        {
            using (var context = CreateContext())
            {
                var parameter = CreateDbParameter("somename", "ALFKI");

                var actual = context.Customers
                    .FromSqlInterpolated(NormalizeDelimetersInInterpolatedString($"SELECT * FROM [Customers] WHERE [CustomerID] = {parameter}"))
                    .ToList();

                Assert.Equal(1, actual.Count);
                Assert.True(actual.All(c => c.City == "Berlin"));
            }
        }

        [ConditionalFact]
        public virtual void FromSqlInterpolated_parameterization_issue_12213()
        {
            using (var context = CreateContext())
            {
                var min = 10300;
                var max = 10400;

                var query1 = context.Orders
                    .FromSqlInterpolated(NormalizeDelimetersInInterpolatedString($"SELECT * FROM [Orders] WHERE [OrderID] >= {min}"))
                    .Select(i => i.OrderID);
                query1.ToList();

                var query2 = context.Orders
                    .Where(o => o.OrderID <= max && query1.Contains(o.OrderID))
                    .Select(o => o.OrderID);
                query2.ToList();

                var query3 = context.Orders
                    .Where(
                        o => o.OrderID <= max
                             && context.Orders
                                 .FromSqlInterpolated(NormalizeDelimetersInInterpolatedString($"SELECT * FROM [Orders] WHERE [OrderID] >= {min}"))
                                 .Select(i => i.OrderID)
                                 .Contains(o.OrderID))
                    .Select(o => o.OrderID);
                query3.ToList();
            }
        }

        [ConditionalFact]
        public virtual void FromSqlRaw_does_not_parameterize_interpolated_string()
        {
            using (var context = CreateContext())
            {
                var tableName = "Orders";
                var max = 10250;
                var query = context.Orders.FromSqlRaw(
                        NormalizeDelimetersInRawString($"SELECT * FROM [{tableName}] WHERE [OrderID] < {{0}}"), max)
                    .ToList();

                Assert.Equal(2, query.Count);
            }
        }

        [ConditionalFact]
        public virtual void Entity_equality_through_fromsql()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Order>()
                    .FromSqlRaw(NormalizeDelimetersInRawString("SELECT * FROM [Orders]"))
                    .Where(o => o.Customer == new Customer
                     {
                         CustomerID = "VINET"
                     })
                    .ToArray();

                Assert.Equal(5, actual.Length);
            }
        }

        protected string NormalizeDelimetersInRawString(string sql)
            => Fixture.TestStore.NormalizeDelimetersInRawString(sql);

        protected FormattableString NormalizeDelimetersInInterpolatedString(FormattableString sql)
            => Fixture.TestStore.NormalizeDelimetersInInterpolatedString(sql);

        protected abstract DbParameter CreateDbParameter(string name, object value);

        protected NorthwindContext CreateContext() => Fixture.CreateContext();
    }
}
