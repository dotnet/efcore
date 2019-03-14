// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryLoggingSqlServerTest : IClassFixture<IncludeSqlServerFixture>
    {
        private static readonly string _eol = Environment.NewLine;

        public QueryLoggingSqlServerTest(IncludeSqlServerFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        protected IncludeSqlServerFixture Fixture { get; }

        [Fact]
        public virtual void Queryable_simple()
        {
            using (var context = CreateContext())
            {
                var customers
                    = context.Set<Customer>()
                        .ToList();

                Assert.NotNull(customers);
                Assert.StartsWith(
                    "Compiling query model: " + _eol +
                    "'from Customer <generated>_0 in DbSet<Customer>",
                    Fixture.TestSqlLoggerFactory.Log[0].Message);
                Assert.StartsWith(
                    "Optimized query model: " + _eol +
                    "'from Customer <generated>_0 in DbSet<Customer>",
                    Fixture.TestSqlLoggerFactory.Log[1].Message);
            }
        }

        [Fact]
        public virtual void Queryable_with_parameter_outputs_parameter_value_logging_warning()
        {
            using (var context = CreateContext())
            {
                context.GetInfrastructure().GetRequiredService<IDiagnosticsLogger<DbLoggerCategory.Query>>()
                    .Options.IsSensitiveDataLoggingWarned = false;
                // ReSharper disable once ConvertToConstant.Local
                var city = "Redmond";

                var customers
                    = context.Customers
                        .Where(c => c.City == city)
                        .ToList();

                Assert.NotNull(customers);
                Assert.Contains(
                    CoreResources.LogSensitiveDataLoggingEnabled(new TestLogger<SqlServerLoggingDefinitions>()).GenerateMessage(), Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
            }
        }

        //[Fact]
        //public virtual void Query_with_ignored_include_should_log_warning()
        //{
        //    using (var context = CreateContext())
        //    {
        //        var customers
        //            = context.Customers
        //                .Include(c => c.Orders)
        //                .Select(c => c.CustomerID)
        //                .ToList();

        //        Assert.NotNull(customers);
        //        Assert.Contains(
        //            CoreResources.LogIgnoredInclude(new TestLogger<SqlServerLoggingDefinitions>()).GenerateMessage("[c].Orders"), Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
        //    }
        //}

        //[Fact]
        //public virtual void Include_navigation()
        //{
        //    using (var context = CreateContext())
        //    {
        //        var customers
        //            = context.Set<Customer>()
        //                .Include(c => c.Orders)
        //                .ToList();

        //        Assert.NotNull(customers);

        //        Assert.Equal(
        //            "Compiling query model: " + _eol +
        //            "'(from Customer c in DbSet<Customer>" + _eol +
        //            @"select [c]).Include(""Orders"")'"
        //            ,
        //            Fixture.TestSqlLoggerFactory.Log[0].Message);
        //        Assert.Equal(
        //            "Including navigation: '[c].Orders'"
        //            ,
        //            Fixture.TestSqlLoggerFactory.Log[1].Message);
        //        Assert.StartsWith(
        //            "Optimized query model: " + _eol +
        //            "'from Customer c in DbSet<Customer>" + _eol +
        //            @"order by EF.Property(?[c]?, ""CustomerID"") asc" + _eol +
        //            "select Customer _Include("
        //            ,
        //            Fixture.TestSqlLoggerFactory.Log[2].Message);
        //    }
        //}

        [Fact(Skip = "Issue #14935. Cannot eval 'Concat({from Order o in value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[Microsoft.EntityFrameworkCore.TestModels.Northwind.Order]) where ([o].CustomerID == \"ALFKI\") select [o]})'")]
        public virtual void Concat_Include_collection_ignored()
        {
            using (var context = CreateContext())
            {
                var orders = context.Orders
                    .Where(o => o.OrderID < 10250)
                    .Concat(context.Orders.Where(o => o.CustomerID == "ALFKI"))
                    .Include(o => o.OrderDetails)
                    .ToList();

                Assert.NotNull(orders);
                Assert.Contains(
                    CoreResources.LogIgnoredInclude(new TestLogger<SqlServerLoggingDefinitions>()).GenerateMessage("[o].OrderDetails"),
                    Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
            }
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'Union({from Order o in value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[Microsoft.EntityFrameworkCore.TestModels.Northwind.Order]) where ([o].CustomerID == \"ALFKI\") select [o]})'")]
        public virtual void Union_Include_collection_ignored()
        {
            using (var context = CreateContext())
            {
                var orders = context.Orders
                    .Where(o => o.OrderID < 10250)
                    .Union(context.Orders.Where(o => o.CustomerID == "ALFKI"))
                    .Include(o => o.OrderDetails)
                    .ToList();

                Assert.NotNull(orders);
                Assert.Contains(
                    CoreResources.LogIgnoredInclude(new TestLogger<SqlServerLoggingDefinitions>()).GenerateMessage("[o].OrderDetails"),
                    Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
            }
        }

        [Fact(Skip = "Issue #14935. Cannot eval 'GroupBy([o].OrderID, [o])'")]
        public virtual void GroupBy_Include_collection_ignored()
        {
            using (var context = CreateContext())
            {
                var orders = context.Orders
                    .GroupBy(o => o.OrderID)
                    .Select(g => g.OrderBy(o => o.OrderID).FirstOrDefault())
                    .Include(o => o.OrderDetails)
                    .ToList();

                Assert.NotNull(orders);
                Assert.Contains(
                    CoreResources.LogIgnoredInclude(new TestLogger<SqlServerLoggingDefinitions>()).GenerateMessage(
                        "{from Order o in [g] orderby [o].OrderID asc select [o] => FirstOrDefault()}.OrderDetails"),
                    Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
            }
        }

        [Fact]
        public void SelectExpression_does_not_use_an_old_logger()
        {
            DbContextOptions CreateOptions(ListLoggerFactory listLoggerFactory)
            {
                var optionsBuilder = new DbContextOptionsBuilder();
                Fixture.TestStore.AddProviderOptions(optionsBuilder);
                optionsBuilder.UseLoggerFactory(listLoggerFactory);
                return optionsBuilder.Options;
            }

            var loggerFactory1 = new ListLoggerFactory();

            using (var context = new NorthwindRelationalContext(CreateOptions(loggerFactory1)))
            {
                var _ = context.Customers.ToList();
            }

            Assert.Equal(1, loggerFactory1.Log.Count(e => e.Id == RelationalEventId.CommandExecuted));

            var loggerFactory2 = new ListLoggerFactory();

            using (var context = new NorthwindRelationalContext(CreateOptions(loggerFactory2)))
            {
                var _ = context.Customers.ToList();
            }

            Assert.Equal(1, loggerFactory1.Log.Count(e => e.Id == RelationalEventId.CommandExecuted));
            Assert.Equal(1, loggerFactory2.Log.Count(e => e.Id == RelationalEventId.CommandExecuted));
        }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();
    }
}
