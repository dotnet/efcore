// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class UdfDbFunctionSqlServerTests : IClassFixture<UdfDbFunctionSqlServerTests.SqlServerUDFFixture>
    {
        public UdfDbFunctionSqlServerTests(SqlServerUDFFixture fixture) => Fixture = fixture;

        private SqlServerUDFFixture Fixture { get; }

        protected UDFSqlContext CreateContext() => (UDFSqlContext)Fixture.CreateContext();

        private string Sql => Fixture.TestSqlLoggerFactory.SqlStatements.Last();

        public class Customer
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public List<Order> Orders { get; set; }
        }

        public class Order
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int ItemCount { get; set; }
            public DateTime OrderDate { get; set; }
            public Customer Customer { get; set; }
        }

        protected class UDFSqlContext : DbContext
        {
            #region DbSets

            public DbSet<Customer> Customers { get; set; }
            public DbSet<Order> Orders { get; set; }

            #endregion

            #region Function Stubs

            public enum ReportingPeriod
            {
                Winter = 0,
                Spring,
                Summer,
                Fall
            }

            public static long MyCustomLength(string s)
            {
                throw new Exception();
            }

            public static bool IsDate(string date)
            {
                throw new Exception();
            }

            public static int AddOne(int num)
            {
                return num + 1;
            }

            public static int AddFive(int number)
            {
                return number + 5;
            }

            public static int CustomerOrderCount(int customerId)
            {
                throw new NotImplementedException();
            }

            public static int CustomerOrderCountWithClient(int customerId)
            {
                switch (customerId)
                {
                    case 1:
                        return 3;
                    case 2:
                        return 2;
                    case 3:
                        return 1;
                    case 4:
                        return 0;
                    default:
                        throw new Exception();
                }
            }

            public static string StarValue(int starCount, int value)
            {
                throw new NotImplementedException();
            }

            public static bool IsTopCustomer(int customerId)
            {
                throw new NotImplementedException();
            }

            public static int GetCustomerWithMostOrdersAfterDate(DateTime? startDate)
            {
                throw new NotImplementedException();
            }

            public static DateTime? GetReportingPeriodStartDate(ReportingPeriod periodId)
            {
                throw new NotImplementedException();
            }

            #endregion

            public UDFSqlContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(CustomerOrderCount)));
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(CustomerOrderCountWithClient))).HasName("CustomerOrderCount");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(StarValue)));
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(IsTopCustomer)));
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetCustomerWithMostOrdersAfterDate)));
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetReportingPeriodStartDate)));
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(IsDate))).HasSchema("");

                var methodInfo = typeof(UDFSqlContext).GetMethod(nameof(MyCustomLength));

                modelBuilder.HasDbFunction(methodInfo)
                    .HasTranslation(args => new SqlFunctionExpression("len", methodInfo.ReturnType, args));
            }
        }

        #region Scalar Tests

        [Fact]
        private void Scalar_Function_Extension_Method()
        {
            using (var context = CreateContext())
            {
                var len = context.Customers.Count(c => UDFSqlContext.IsDate(c.FirstName) == false);

                Assert.Equal(3, len);

                Assert.Equal(
                    @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE CASE
    WHEN IsDate([c].[FirstName]) = 1
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = 0",
                Sql,
                ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        private void Scalar_Function_With_Translator_Translates()
        {
            using (var context = CreateContext())
            {
                var customerId = 3;

                var len = context.Customers.Where(c => c.Id == customerId)
                    .Select(c => UDFSqlContext.MyCustomLength(c.LastName)).Single();

                Assert.Equal(5, len);

                Assert.Equal(
                    @"@__customerId_0='3'

SELECT TOP(2) len([c].[LastName])
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Function_ClientEval_Method_As_Translateable_Method_Parameter()
        {
            using (var context = CreateContext())
            {
                Assert.Throws<NotImplementedException>(
                    () => (from c in context.Customers
                           where c.Id == 1
                           select new
                           {
                               c.FirstName,
                               OrderCount = UDFSqlContext.CustomerOrderCount(UDFSqlContext.AddFive(c.Id - 5))
                           }).Single());
            }
        }

        [Fact]
        public void Scalar_Function_Constant_Parameter()
        {
            using (var context = CreateContext())
            {
                var customerId = 1;

                var custs = context.Customers.Select(c => UDFSqlContext.CustomerOrderCount(customerId)).ToList();

                Assert.Equal(3, custs.Count);

                Assert.Equal(
                    @"@__customerId_0='1'

SELECT [dbo].CustomerOrderCount(@__customerId_0)
FROM [Customers] AS [c]",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Function_Anonymous_Type_Select_Correlated()
        {
            using (var context = CreateContext())
            {
                var cust = (from c in context.Customers
                            where c.Id == 1
                            select new
                            {
                                c.LastName,
                                OrderCount = UDFSqlContext.CustomerOrderCount(c.Id)
                            }).Single();

                Assert.Equal("One", cust.LastName);
                Assert.Equal(3, cust.OrderCount);

                Assert.Equal(
                    @"SELECT TOP(2) [c].[LastName], [dbo].CustomerOrderCount([c].[Id]) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 1",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Function_Anonymous_Type_Select_Not_Correlated()
        {
            using (var context = CreateContext())
            {
                var cust = (from c in context.Customers
                            where c.Id == 1
                            select new
                            {
                                c.LastName,
                                OrderCount = UDFSqlContext.CustomerOrderCount(1)
                            }).Single();

                Assert.Equal("One", cust.LastName);
                Assert.Equal(3, cust.OrderCount);

                Assert.Equal(
                    @"SELECT TOP(2) [c].[LastName], [dbo].CustomerOrderCount(1) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 1",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Function_Anonymous_Type_Select_Parameter()
        {
            using (var context = CreateContext())
            {
                var customerId = 1;

                var cust = (from c in context.Customers
                            where c.Id == customerId
                            select new
                            {
                                c.LastName,
                                OrderCount = UDFSqlContext.CustomerOrderCount(customerId)
                            }).Single();

                Assert.Equal("One", cust.LastName);
                Assert.Equal(3, cust.OrderCount);

                Assert.Equal(
                    @"@__customerId_1='1'
@__customerId_0='1'

SELECT TOP(2) [c].[LastName], [dbo].CustomerOrderCount(@__customerId_1) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Function_Anonymous_Type_Select_Nested()
        {
            using (var context = CreateContext())
            {
                var customerId = 3;
                var starCount = 3;

                var cust = (from c in context.Customers
                            where c.Id == customerId
                            select new
                            {
                                c.LastName,
                                OrderCount = UDFSqlContext.StarValue(starCount, UDFSqlContext.CustomerOrderCount(customerId))
                            }).Single();

                Assert.Equal("Three", cust.LastName);
                Assert.Equal("***1", cust.OrderCount);

                Assert.Equal(
                    @"@__starCount_1='3'
@__customerId_2='3'
@__customerId_0='3'

SELECT TOP(2) [c].[LastName], [dbo].StarValue(@__starCount_1, [dbo].CustomerOrderCount(@__customerId_2)) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Function_Where_Correlated()
        {
            using (var context = CreateContext())
            {
                var cust = (from c in context.Customers
                            where UDFSqlContext.IsTopCustomer(c.Id)
                            select c.Id.ToString().ToLower()).ToList();

                Assert.Equal(1, cust.Count);

                Assert.Equal(
                    @"SELECT LOWER(CONVERT(VARCHAR(11), [c].[Id]))
FROM [Customers] AS [c]
WHERE [dbo].IsTopCustomer([c].[Id]) = 1",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Function_Where_Not_Correlated()
        {
            using (var context = CreateContext())
            {
                var startDate = new DateTime(2000, 4, 1);

                var custId = (from c in context.Customers
                              where UDFSqlContext.GetCustomerWithMostOrdersAfterDate(startDate) == c.Id
                              select c.Id).SingleOrDefault();

                Assert.Equal(custId, 2);

                Assert.Equal(
                    @"@__startDate_0='04/01/2000 00:00:00'

SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [dbo].GetCustomerWithMostOrdersAfterDate(@__startDate_0) = [c].[Id]",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Function_Where_Parameter()
        {
            using (var context = CreateContext())
            {
                var period = UDFSqlContext.ReportingPeriod.Winter;

                var custId = (from c in context.Customers
                              where c.Id == UDFSqlContext.GetCustomerWithMostOrdersAfterDate(
                                        UDFSqlContext.GetReportingPeriodStartDate(period))
                              select c.Id).SingleOrDefault();

                Assert.Equal(custId, 1);

                Assert.Equal(
                    @"@__period_0='Winter'

SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [c].[Id] = [dbo].GetCustomerWithMostOrdersAfterDate([dbo].GetReportingPeriodStartDate(@__period_0))",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Function_Where_Nested()
        {
            using (var context = CreateContext())
            {
                var custId = (from c in context.Customers
                              where c.Id == UDFSqlContext.GetCustomerWithMostOrdersAfterDate(
                                        UDFSqlContext.GetReportingPeriodStartDate(
                                            UDFSqlContext.ReportingPeriod.Winter))
                              select c.Id).SingleOrDefault();

                Assert.Equal(custId, 1);

                Assert.Equal(
                    @"SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [c].[Id] = [dbo].GetCustomerWithMostOrdersAfterDate([dbo].GetReportingPeriodStartDate(0))",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Function_Let_Correlated()
        {
            using (var context = CreateContext())
            {
                var cust = (from c in context.Customers
                            let orderCount = UDFSqlContext.CustomerOrderCount(c.Id)
                            where c.Id == 2
                            select new
                            {
                                c.LastName,
                                OrderCount = orderCount
                            }).Single();

                Assert.Equal("Two", cust.LastName);
                Assert.Equal(2, cust.OrderCount);

                Assert.Equal(
                    @"SELECT TOP(2) [c].[LastName], [dbo].CustomerOrderCount([c].[Id]) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 2",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Function_Let_Not_Correlated()
        {
            using (var context = CreateContext())
            {
                var cust = (from c in context.Customers
                            let orderCount = UDFSqlContext.CustomerOrderCount(2)
                            where c.Id == 2
                            select new
                            {
                                c.LastName,
                                OrderCount = orderCount
                            }).Single();

                Assert.Equal("Two", cust.LastName);
                Assert.Equal(2, cust.OrderCount);

                Assert.Equal(
                    @"SELECT TOP(2) [c].[LastName], [dbo].CustomerOrderCount(2) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 2",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Function_Let_Not_Parameter()
        {
            var customerId = 2;

            using (var context = CreateContext())
            {
                var cust = (from c in context.Customers
                            let orderCount = UDFSqlContext.CustomerOrderCount(customerId)
                            where c.Id == customerId
                            select new
                            {
                                c.LastName,
                                OrderCount = orderCount
                            }).Single();

                Assert.Equal("Two", cust.LastName);
                Assert.Equal(2, cust.OrderCount);

                Assert.Equal(
                    @"@__customerId_0='2'
@__customerId_1='2'

SELECT TOP(2) [c].[LastName], [dbo].CustomerOrderCount(@__customerId_0) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_1",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Function_Let_Nested()
        {
            using (var context = CreateContext())
            {
                var customerId = 1;
                var starCount = 3;

                var cust = (from c in context.Customers
                            let orderCount = UDFSqlContext.StarValue(starCount, UDFSqlContext.CustomerOrderCount(customerId))
                            where c.Id == customerId
                            select new
                            {
                                c.LastName,
                                OrderCount = orderCount
                            }).Single();

                Assert.Equal("One", cust.LastName);
                Assert.Equal("***3", cust.OrderCount);

                Assert.Equal(
                    @"@__starCount_0='3'
@__customerId_1='1'
@__customerId_2='1'

SELECT TOP(2) [c].[LastName], [dbo].StarValue(@__starCount_0, [dbo].CustomerOrderCount(@__customerId_1)) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_2",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Unwind_Client_Eval_Where()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == UDFSqlContext.AddOne(c.Id)
                               select c.Id).Single();

                Assert.Equal(1, results);
                Assert.Equal(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Nested__Function_Unwind_Client_Eval_OrderBy()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               orderby UDFSqlContext.AddOne(c.Id)
                               select c.Id).ToList();

                Assert.Equal(3, results.Count);
                Assert.True(results.SequenceEqual(Enumerable.Range(1, 3)));

                Assert.Equal(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Unwind_Client_Eval_Select()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               orderby c.Id
                               select UDFSqlContext.AddOne(c.Id)).ToList();

                Assert.Equal(3, results.Count);
                Assert.True(results.SequenceEqual(Enumerable.Range(2, 3)));

                Assert.Equal(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]
ORDER BY [c].[Id]",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Client_BCL_UDF()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == UDFSqlContext.AddOne(Math.Abs(UDFSqlContext.CustomerOrderCountWithClient(c.Id)))
                               select c.Id).Single();

                Assert.Equal(3, results);
                Assert.Equal(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Client_UDF_BCL()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == UDFSqlContext.AddOne(UDFSqlContext.CustomerOrderCountWithClient(Math.Abs(c.Id)))
                               select c.Id).Single();

                Assert.Equal(3, results);
                Assert.Equal(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_BCL_Client_UDF()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == Math.Abs(UDFSqlContext.AddOne(UDFSqlContext.CustomerOrderCountWithClient(c.Id)))
                               select c.Id).Single();

                Assert.Equal(3, results);
                Assert.Equal(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_BCL_UDF_Client()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 1 == Math.Abs(UDFSqlContext.CustomerOrderCountWithClient(UDFSqlContext.AddOne(c.Id)))
                               select c.Id).Single();

                Assert.Equal(2, results);
                Assert.Equal(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_UDF_BCL_Client()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 1 == UDFSqlContext.CustomerOrderCountWithClient(Math.Abs(UDFSqlContext.AddOne(c.Id)))
                               select c.Id).Single();

                Assert.Equal(2, results);
                Assert.Equal(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_UDF_Client_BCL()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 1 == UDFSqlContext.CustomerOrderCountWithClient(UDFSqlContext.AddOne(Math.Abs(c.Id)))
                               select c.Id).Single();

                Assert.Equal(2, results);
                Assert.Equal(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Client_BCL()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 3 == UDFSqlContext.AddOne(Math.Abs(c.Id))
                               select c.Id).Single();

                Assert.Equal(2, results);
                Assert.Equal(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Client_UDF()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == UDFSqlContext.AddOne(UDFSqlContext.CustomerOrderCountWithClient(c.Id))
                               select c.Id).Single();

                Assert.Equal(3, results);
                Assert.Equal(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_BCL_Client()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 3 == Math.Abs(UDFSqlContext.AddOne(c.Id))
                               select c.Id).Single();

                Assert.Equal(2, results);
                Assert.Equal(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_BCL_UDF()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 3 == Math.Abs(UDFSqlContext.CustomerOrderCount(c.Id))
                               select c.Id).Single();

                Assert.Equal(1, results);
                Assert.Equal(
                    @"SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE 3 = ABS([dbo].CustomerOrderCount([c].[Id]))",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }


        [Fact]
        public void Scalar_Nested_Function_UDF_Client()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == UDFSqlContext.CustomerOrderCountWithClient(UDFSqlContext.AddOne(c.Id))
                               select c.Id).Single();

                Assert.Equal(1, results);
                Assert.Equal(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Scalar_Nested_Function_UDF_BCL()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 3 == UDFSqlContext.CustomerOrderCount(Math.Abs(c.Id))
                               select c.Id).Single();

                Assert.Equal(1, results);
                Assert.Equal(
                    @"SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE 3 = [dbo].CustomerOrderCount(ABS([c].[Id]))",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        #endregion

        public class SqlServerUDFFixture : SharedStoreFixtureBase<DbContext>
        {
            protected override string StoreName { get; } = "UDFDbFunctionSqlServerTests";
            protected override Type ContextType { get; } = typeof(UDFSqlContext);
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                base.AddOptions(builder);
                return builder.ConfigureWarnings(w => w.Ignore(RelationalEventId.QueryClientEvaluationWarning));
            }

            protected override void Seed(DbContext context)
            {
                context.Database.EnsureCreated();

                context.Database.ExecuteSqlCommand(@"create function [dbo].[CustomerOrderCount] (@customerId int)
                                                    returns int
                                                    as
                                                    begin
	                                                    return (select count(id) from orders where customerId = @customerId);
                                                    end");

                context.Database.ExecuteSqlCommand(@"create function[dbo].[StarValue] (@starCount int, @value nvarchar(max))
                                                    returns nvarchar(max)
                                                        as
                                                        begin
                                                    return replicate('*', @starCount) + @value
                                                    end");

                context.Database.ExecuteSqlCommand(@"create function [dbo].[GetReportingPeriodStartDate] (@period int)
                                                    returns DateTime
                                                    as
                                                    begin
	                                                    return '1998-01-01'
                                                    end");

                context.Database.ExecuteSqlCommand(@"create function [dbo].[GetCustomerWithMostOrdersAfterDate] (@searchDate Date)
                                                    returns int
                                                    as
                                                    begin
	                                                    return (select top 1 customerId
			                                                    from orders
			                                                    where orderDate > @searchDate
			                                                    group by CustomerId
			                                                    order by count(id) desc)
                                                    end");

                context.Database.ExecuteSqlCommand(@"create function [dbo].[IsTopCustomer] (@customerId int)
                                                    returns bit
                                                    as
                                                    begin
	                                                    if(@customerId = 1)
		                                                    return 1
		
	                                                    return 0
                                                    end");

                var order11 = new Order { Name = "Order11", ItemCount = 4, OrderDate = new DateTime(2000, 1, 20) };
                var order12 = new Order { Name = "Order12", ItemCount = 8, OrderDate = new DateTime(2000, 2, 21) };
                var order13 = new Order { Name = "Order13", ItemCount = 15, OrderDate = new DateTime(2000, 3, 20) };
                var order21 = new Order { Name = "Order21", ItemCount = 16, OrderDate = new DateTime(2000, 4, 21) };
                var order22 = new Order { Name = "Order22", ItemCount = 23, OrderDate = new DateTime(2000, 5, 20) };
                var order31 = new Order { Name = "Order31", ItemCount = 42, OrderDate = new DateTime(2000, 6, 21) };

                var customer1 = new Customer { FirstName = "Customer", LastName = "One", Orders = new List<Order> { order11, order12, order13 } };
                var customer2 = new Customer { FirstName = "Customer", LastName = "Two", Orders = new List<Order> { order21, order22 } };
                var customer3 = new Customer { FirstName = "Customer", LastName = "Three", Orders = new List<Order> { order31 } };

                ((UDFSqlContext)context).Customers.AddRange(customer1, customer2, customer3);
                ((UDFSqlContext)context).Orders.AddRange(order11, order12, order13, order21, order22, order31);
                context.SaveChanges();
            }
        }
    }
}
