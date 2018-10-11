// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryLoggingOracleTest : IClassFixture<IncludeOracleFixture>
    {
        private static readonly string _eol = Environment.NewLine;

        public QueryLoggingOracleTest(IncludeOracleFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        protected IncludeOracleFixture Fixture { get; }

        [Fact]
        public virtual void Queryable_simple()
        {
            using (var context = CreateContext())
            {
                var customers
                    = context.Set<Customer>()
                        .ToList();

                Assert.NotNull(customers);
                Assert.Contains(
                    "Compiling query model: " + _eol +
                    "'from Customer <generated>_0 in DbSet<Customer>" + _eol +
                    "select [<generated>_0]'" + _eol +
                    "Optimized query model: " + _eol +
                    "'from Customer <generated>_0 in DbSet<Customer>",
                    string.Join(_eol, Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message)));
            }
        }

        [Fact(Skip = "Issue #13029")]
        public virtual void Queryable_with_parameter_outputs_parameter_value_logging_warning()
        {
            using (var context = CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var city = "Redmond";

                var customers
                    = context.Customers
                        .Where(c => c.City == city)
                        .ToList();

                Assert.NotNull(customers);
                Assert.Contains(CoreStrings.LogSensitiveDataLoggingEnabled.GenerateMessage(), Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
            }
        }

        [Fact]
        public virtual void Query_with_ignored_include_should_log_warning()
        {
            using (var context = CreateContext())
            {
                var customers
                    = context.Customers
                        .Include(c => c.Orders)
                        .Select(c => c.CustomerID)
                        .ToList();

                Assert.NotNull(customers);
                Assert.Contains(CoreStrings.LogIgnoredInclude.GenerateMessage("[c].Orders"), Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message));
            }
        }

        [Fact]
        public virtual void Include_navigation()
        {
            using (var context = CreateContext())
            {
                var customers
                    = context.Set<Customer>()
                        .Include(c => c.Orders)
                        .ToList();

                Assert.NotNull(customers);
                Assert.Contains(
                    "Compiling query model: " + _eol +
                    "'(from Customer c in DbSet<Customer>" + _eol +
                    @"select [c]).Include(""Orders"")'" + _eol +
                    "Including navigation: '[c].Orders'" + _eol +
                    "Optimized query model: " + _eol +
                    "'from Customer c in DbSet<Customer>"
                    ,
                    string.Join(_eol, Fixture.TestSqlLoggerFactory.Log.Select(l => l.Message)));
            }
        }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();
    }
}
