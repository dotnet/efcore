// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Xunit;
#if NETCOREAPP2_0

#endif
namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class QueryLoggingSqlServerTest : IClassFixture<NorthwindQuerySqlServerFixture>
    {
        private const string FileLineEnding = @"
";

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
                    @"    Compiling query model: 
'from Customer <generated>_0 in DbSet<Customer>
select [<generated>_0]'
    Optimized query model: 
'from Customer <generated>_0 in DbSet<Customer>",
                    _fixture.TestSqlLoggerFactory.Log.Replace(Environment.NewLine, FileLineEnding));
            }
        }

        [Fact]
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
                Assert.Contains(CoreStrings.SensitiveDataLoggingEnabled, _fixture.TestSqlLoggerFactory.Log);
            }
        }

        [Fact]
        public virtual void Query_with_ignored_include_should_log_warning1()
        {
            using (var context = CreateContext())
            {
                var customers
                    = context.Customers
                        .Include(c => c.Orders)
                        .Select(c => c.CustomerID)
                        .ToList();

                Assert.NotNull(customers);
                Assert.Contains(CoreStrings.LogIgnoredInclude("[c].Orders"), _fixture.TestSqlLoggerFactory.Log);
            }
        }

        [Fact]
        public virtual void Query_with_ignored_include_should_log_warning2()
        {
            using (var context = CreateContext())
            {
                var results
                    = context.Customers
                        .Select(c => new Tuple<Customer, int>( c, 5 ))
                        .Include(t => t.Item1.Orders)
                        .ToList();

                Assert.NotNull(results);
                Assert.True(results.All(t => t.Item1.Orders == null));
                Assert.Contains(CoreStrings.LogIgnoredInclude("new Tuple`2(Item1 = [c], Item2 = 5).Item1.Orders"), _fixture.TestSqlLoggerFactory.Log);
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
                Assert.StartsWith(
                    @"    Compiling query model: 
'(from Customer c in DbSet<Customer>
select [c]).Include(""Orders"")'
    Including navigation: '[c].Orders'
    Optimized query model: 
'from Customer c in DbSet<Customer>"
                    ,
                    _fixture.TestSqlLoggerFactory.Log.Replace(Environment.NewLine, FileLineEnding));
            }
        }

        private readonly NorthwindQuerySqlServerFixture _fixture;

        public QueryLoggingSqlServerTest(NorthwindQuerySqlServerFixture fixture)
        {
            _fixture = fixture;
            _fixture.TestSqlLoggerFactory.Clear();
        }

        protected NorthwindContext CreateContext() => _fixture.CreateContext();
    }
}
