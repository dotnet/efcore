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
        public UdfDbFunctionSqlServerTests(SqlServerUDFFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        private SqlServerUDFFixture Fixture { get; }

        protected UDFSqlContext CreateContext() => (UDFSqlContext)Fixture.CreateContext();

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

            public static long MyCustomLengthStatic(string s)
            {
                throw new Exception();
            }

            public static bool IsDateStatic(string date)
            {
                throw new Exception();
            }

            public static int AddOneStatic(int num)
            {
                return num + 1;
            }

            public static int AddFiveStatic(int number)
            {
                return number + 5;
            }

            public static int CustomerOrderCountStatic(int customerId)
            {
                throw new NotImplementedException();
            }

            public static int CustomerOrderCountWithClientStatic(int customerId)
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

            public static string StarValueStatic(int starCount, int value)
            {
                throw new NotImplementedException();
            }

            public static bool IsTopCustomerStatic(int customerId)
            {
                throw new NotImplementedException();
            }

            public static int GetCustomerWithMostOrdersAfterDateStatic(DateTime? startDate)
            {
                throw new NotImplementedException();
            }

            public static DateTime? GetReportingPeriodStartDateStatic(ReportingPeriod periodId)
            {
                throw new NotImplementedException();
            }

            public long MyCustomLengthInstance(string s)
            {
                throw new Exception();
            }

            public bool IsDateInstance(string date)
            {
                throw new Exception();
            }

            public int AddOneInstance(int num)
            {
                return num + 1;
            }

            public int AddFiveInstance(int number)
            {
                return number + 5;
            }

            public int CustomerOrderCountInstance(int customerId)
            {
                throw new NotImplementedException();
            }

            public int CustomerOrderCountWithClientInstance(int customerId)
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

            public string StarValueInstance(int starCount, int value)
            {
                throw new NotImplementedException();
            }

            public bool IsTopCustomerInstance(int customerId)
            {
                throw new NotImplementedException();
            }

            public int GetCustomerWithMostOrdersAfterDateInstance(DateTime? startDate)
            {
                throw new NotImplementedException();
            }

            public DateTime? GetReportingPeriodStartDateInstance(ReportingPeriod periodId)
            {
                throw new NotImplementedException();
            }

            public string DollarValueInstance(int starCount, string value)
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
                //Static
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(CustomerOrderCountStatic))).HasName("CustomerOrderCount");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(CustomerOrderCountWithClientStatic))).HasName("CustomerOrderCount");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(StarValueStatic))).HasName("StarValue");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(IsTopCustomerStatic))).HasName("IsTopCustomer");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetCustomerWithMostOrdersAfterDateStatic))).HasName("GetCustomerWithMostOrdersAfterDate");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetReportingPeriodStartDateStatic))).HasName("GetReportingPeriodStartDate");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(IsDateStatic))).HasSchema("").HasName("IsDate");

                var methodInfo = typeof(UDFSqlContext).GetMethod(nameof(MyCustomLengthStatic));

                modelBuilder.HasDbFunction(methodInfo)
                    .HasTranslation(args => new SqlFunctionExpression("len", methodInfo.ReturnType, args));

                //Instance
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(CustomerOrderCountInstance))).HasName("CustomerOrderCount");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(CustomerOrderCountWithClientInstance))).HasName("CustomerOrderCount");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(StarValueInstance))).HasName("StarValue");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(IsTopCustomerInstance))).HasName("IsTopCustomer");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetCustomerWithMostOrdersAfterDateInstance))).HasName("GetCustomerWithMostOrdersAfterDate");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetReportingPeriodStartDateInstance))).HasName("GetReportingPeriodStartDate");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(IsDateInstance))).HasSchema("").HasName("IsDate");

                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(DollarValueInstance))).HasName("DollarValue");

                var methodInfo2 = typeof(UDFSqlContext).GetMethod(nameof(MyCustomLengthInstance));

                modelBuilder.HasDbFunction(methodInfo2)
                    .HasTranslation(args => new SqlFunctionExpression("len", methodInfo2.ReturnType, args));
            }
        }

        #region Scalar Tests

        #region Static

        [Fact]
        private void Scalar_Function_Extension_Method_Static()
        {
            using (var context = CreateContext())
            {
                var len = context.Customers.Count(c => UDFSqlContext.IsDateStatic(c.FirstName) == false);

                Assert.Equal(3, len);

                AssertSql(
                    @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE CASE
    WHEN IsDate([c].[FirstName]) = 1
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = 0");
            }
        }

        [Fact]
        private void Scalar_Function_With_Translator_Translates_Static()
        {
            using (var context = CreateContext())
            {
                var customerId = 3;

                var len = context.Customers.Where(c => c.Id == customerId)
                    .Select(c => UDFSqlContext.MyCustomLengthStatic(c.LastName)).Single();

                Assert.Equal(5, len);

                AssertSql(
                    @"@__customerId_0='3'

SELECT TOP(2) len([c].[LastName])
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0");
            }
        }

        [Fact]
        public void Scalar_Function_ClientEval_Method_As_Translateable_Method_Parameter_Static()
        {
            using (var context = CreateContext())
            {
                Assert.Throws<NotImplementedException>(
                    () => (from c in context.Customers
                           where c.Id == 1
                           select new
                           {
                               c.FirstName,
                               OrderCount = UDFSqlContext.CustomerOrderCountStatic(UDFSqlContext.AddFiveStatic(c.Id - 5))
                           }).Single());
            }
        }

        [Fact]
        public void Scalar_Function_Constant_Parameter_Static()
        {
            using (var context = CreateContext())
            {
                var customerId = 1;

                var custs = context.Customers.Select(c => UDFSqlContext.CustomerOrderCountStatic(customerId)).ToList();

                Assert.Equal(3, custs.Count);

                AssertSql(
                    @"@__customerId_0='1'

SELECT [dbo].CustomerOrderCount(@__customerId_0)
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Function_Anonymous_Type_Select_Correlated_Static()
        {
            using (var context = CreateContext())
            {
                var cust = (from c in context.Customers
                            where c.Id == 1
                            select new
                            {
                                c.LastName,
                                OrderCount = UDFSqlContext.CustomerOrderCountStatic(c.Id)
                            }).Single();

                Assert.Equal("One", cust.LastName);
                Assert.Equal(3, cust.OrderCount);

                AssertSql(
                    @"SELECT TOP(2) [c].[LastName], [dbo].CustomerOrderCount([c].[Id]) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 1");
            }
        }

        [Fact]
        public void Scalar_Function_Anonymous_Type_Select_Not_Correlated_Static()
        {
            using (var context = CreateContext())
            {
                var cust = (from c in context.Customers
                            where c.Id == 1
                            select new
                            {
                                c.LastName,
                                OrderCount = UDFSqlContext.CustomerOrderCountStatic(1)
                            }).Single();

                Assert.Equal("One", cust.LastName);
                Assert.Equal(3, cust.OrderCount);

                AssertSql(
                    @"SELECT TOP(2) [c].[LastName], [dbo].CustomerOrderCount(1) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 1");
            }
        }

        [Fact]
        public void Scalar_Function_Anonymous_Type_Select_Parameter_Static()
        {
            using (var context = CreateContext())
            {
                var customerId = 1;

                var cust = (from c in context.Customers
                            where c.Id == customerId
                            select new
                            {
                                c.LastName,
                                OrderCount = UDFSqlContext.CustomerOrderCountStatic(customerId)
                            }).Single();

                Assert.Equal("One", cust.LastName);
                Assert.Equal(3, cust.OrderCount);

                AssertSql(
                    @"@__customerId_1='1'
@__customerId_0='1'

SELECT TOP(2) [c].[LastName], [dbo].CustomerOrderCount(@__customerId_1) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0");
            }
        }

        [Fact]
        public void Scalar_Function_Anonymous_Type_Select_Nested_Static()
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
                                OrderCount = UDFSqlContext.StarValueStatic(starCount, UDFSqlContext.CustomerOrderCountStatic(customerId))
                            }).Single();

                Assert.Equal("Three", cust.LastName);
                Assert.Equal("***1", cust.OrderCount);

                AssertSql(
                    @"@__starCount_1='3'
@__customerId_2='3'
@__customerId_0='3'

SELECT TOP(2) [c].[LastName], [dbo].StarValue(@__starCount_1, [dbo].CustomerOrderCount(@__customerId_2)) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0");
            }
        }

        [Fact]
        public void Scalar_Function_Where_Correlated_Static()
        {
            using (var context = CreateContext())
            {
                var cust = (from c in context.Customers
                            where UDFSqlContext.IsTopCustomerStatic(c.Id)
                            select c.Id.ToString().ToLower()).ToList();

                Assert.Equal(1, cust.Count);

                AssertSql(
                    @"SELECT LOWER(CONVERT(VARCHAR(11), [c].[Id]))
FROM [Customers] AS [c]
WHERE [dbo].IsTopCustomer([c].[Id]) = 1");
            }
        }

        [Fact]
        public void Scalar_Function_Where_Not_Correlated_Static()
        {
            using (var context = CreateContext())
            {
                var startDate = new DateTime(2000, 4, 1);

                var custId = (from c in context.Customers
                              where UDFSqlContext.GetCustomerWithMostOrdersAfterDateStatic(startDate) == c.Id
                              select c.Id).SingleOrDefault();

                Assert.Equal(custId, 2);

                AssertSql(
                    @"@__startDate_0='2000-04-01T00:00:00'

SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [dbo].GetCustomerWithMostOrdersAfterDate(@__startDate_0) = [c].[Id]");
            }
        }

        [Fact]
        public void Scalar_Function_Where_Parameter_Static()
        {
            using (var context = CreateContext())
            {
                var period = UDFSqlContext.ReportingPeriod.Winter;

                var custId = (from c in context.Customers
                              where c.Id == UDFSqlContext.GetCustomerWithMostOrdersAfterDateStatic(
                                        UDFSqlContext.GetReportingPeriodStartDateStatic(period))
                              select c.Id).SingleOrDefault();

                Assert.Equal(custId, 1);

                AssertSql(
                    @"@__period_0='0'

SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [c].[Id] = [dbo].GetCustomerWithMostOrdersAfterDate([dbo].GetReportingPeriodStartDate(@__period_0))");
            }
        }

        [Fact]
        public void Scalar_Function_Where_Nested_Static()
        {
            using (var context = CreateContext())
            {
                var custId = (from c in context.Customers
                              where c.Id == UDFSqlContext.GetCustomerWithMostOrdersAfterDateStatic(
                                        UDFSqlContext.GetReportingPeriodStartDateStatic(
                                            UDFSqlContext.ReportingPeriod.Winter))
                              select c.Id).SingleOrDefault();

                Assert.Equal(custId, 1);

                AssertSql(
                    @"SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [c].[Id] = [dbo].GetCustomerWithMostOrdersAfterDate([dbo].GetReportingPeriodStartDate(0))");
            }
        }

        [Fact]
        public void Scalar_Function_Let_Correlated_Static()
        {
            using (var context = CreateContext())
            {
                var cust = (from c in context.Customers
                            let orderCount = UDFSqlContext.CustomerOrderCountStatic(c.Id)
                            where c.Id == 2
                            select new
                            {
                                c.LastName,
                                OrderCount = orderCount
                            }).Single();

                Assert.Equal("Two", cust.LastName);
                Assert.Equal(2, cust.OrderCount);

                AssertSql(
                    @"SELECT TOP(2) [c].[LastName], [dbo].CustomerOrderCount([c].[Id]) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 2");
            }
        }

        [Fact]
        public void Scalar_Function_Let_Not_Correlated_Static()
        {
            using (var context = CreateContext())
            {
                var cust = (from c in context.Customers
                            let orderCount = UDFSqlContext.CustomerOrderCountStatic(2)
                            where c.Id == 2
                            select new
                            {
                                c.LastName,
                                OrderCount = orderCount
                            }).Single();

                Assert.Equal("Two", cust.LastName);
                Assert.Equal(2, cust.OrderCount);

                AssertSql(
                    @"SELECT TOP(2) [c].[LastName], [dbo].CustomerOrderCount(2) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 2");
            }
        }

        [Fact]
        public void Scalar_Function_Let_Not_Parameter_Static()
        {
            var customerId = 2;

            using (var context = CreateContext())
            {
                var cust = (from c in context.Customers
                            let orderCount = UDFSqlContext.CustomerOrderCountStatic(customerId)
                            where c.Id == customerId
                            select new
                            {
                                c.LastName,
                                OrderCount = orderCount
                            }).Single();

                Assert.Equal("Two", cust.LastName);
                Assert.Equal(2, cust.OrderCount);

                AssertSql(
                    @"@__customerId_0='2'
@__customerId_1='2'

SELECT TOP(2) [c].[LastName], [dbo].CustomerOrderCount(@__customerId_0) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_1");
            }
        }

        [Fact]
        public void Scalar_Function_Let_Nested_Static()
        {
            using (var context = CreateContext())
            {
                var customerId = 1;
                var starCount = 3;

                var cust = (from c in context.Customers
                            let orderCount = UDFSqlContext.StarValueStatic(starCount, UDFSqlContext.CustomerOrderCountStatic(customerId))
                            where c.Id == customerId
                            select new
                            {
                                c.LastName,
                                OrderCount = orderCount
                            }).Single();

                Assert.Equal("One", cust.LastName);
                Assert.Equal("***3", cust.OrderCount);

                AssertSql(
                    @"@__starCount_0='3'
@__customerId_1='1'
@__customerId_2='1'

SELECT TOP(2) [c].[LastName], [dbo].StarValue(@__starCount_0, [dbo].CustomerOrderCount(@__customerId_1)) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_2");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Unwind_Client_Eval_Where_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == UDFSqlContext.AddOneStatic(c.Id)
                               select c.Id).Single();

                Assert.Equal(1, results);
                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested__Function_Unwind_Client_Eval_OrderBy_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               orderby UDFSqlContext.AddOneStatic(c.Id)
                               select c.Id).ToList();

                Assert.Equal(3, results.Count);
                Assert.True(results.SequenceEqual(Enumerable.Range(1, 3)));

                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Unwind_Client_Eval_Select_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               orderby c.Id
                               select UDFSqlContext.AddOneStatic(c.Id)).ToList();

                Assert.Equal(3, results.Count);
                Assert.True(results.SequenceEqual(Enumerable.Range(2, 3)));

                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]
ORDER BY [c].[Id]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Client_BCL_UDF_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == UDFSqlContext.AddOneStatic(Math.Abs(UDFSqlContext.CustomerOrderCountWithClientStatic(c.Id)))
                               select c.Id).Single();

                Assert.Equal(3, results);
                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Client_UDF_BCL_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == UDFSqlContext.AddOneStatic(UDFSqlContext.CustomerOrderCountWithClientStatic(Math.Abs(c.Id)))
                               select c.Id).Single();

                Assert.Equal(3, results);
                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_BCL_Client_UDF_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == Math.Abs(UDFSqlContext.AddOneStatic(UDFSqlContext.CustomerOrderCountWithClientStatic(c.Id)))
                               select c.Id).Single();

                Assert.Equal(3, results);
                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_BCL_UDF_Client_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 1 == Math.Abs(UDFSqlContext.CustomerOrderCountWithClientStatic(UDFSqlContext.AddOneStatic(c.Id)))
                               select c.Id).Single();

                Assert.Equal(2, results);
                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_UDF_BCL_Client_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 1 == UDFSqlContext.CustomerOrderCountWithClientStatic(Math.Abs(UDFSqlContext.AddOneStatic(c.Id)))
                               select c.Id).Single();

                Assert.Equal(2, results);
                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_UDF_Client_BCL_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 1 == UDFSqlContext.CustomerOrderCountWithClientStatic(UDFSqlContext.AddOneStatic(Math.Abs(c.Id)))
                               select c.Id).Single();

                Assert.Equal(2, results);
                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Client_BCL_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 3 == UDFSqlContext.AddOneStatic(Math.Abs(c.Id))
                               select c.Id).Single();

                Assert.Equal(2, results);
                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Client_UDF_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == UDFSqlContext.AddOneStatic(UDFSqlContext.CustomerOrderCountWithClientStatic(c.Id))
                               select c.Id).Single();

                Assert.Equal(3, results);
                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_BCL_Client_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 3 == Math.Abs(UDFSqlContext.AddOneStatic(c.Id))
                               select c.Id).Single();

                Assert.Equal(2, results);
                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_BCL_UDF_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 3 == Math.Abs(UDFSqlContext.CustomerOrderCountStatic(c.Id))
                               select c.Id).Single();

                Assert.Equal(1, results);
                AssertSql(
                    @"SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE 3 = ABS([dbo].CustomerOrderCount([c].[Id]))");
            }
        }


        [Fact]
        public void Scalar_Nested_Function_UDF_Client_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == UDFSqlContext.CustomerOrderCountWithClientStatic(UDFSqlContext.AddOneStatic(c.Id))
                               select c.Id).Single();

                Assert.Equal(1, results);
                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_UDF_BCL_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 3 == UDFSqlContext.CustomerOrderCountStatic(Math.Abs(c.Id))
                               select c.Id).Single();

                Assert.Equal(1, results);
                AssertSql(
                    @"SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE 3 = [dbo].CustomerOrderCount(ABS([c].[Id]))");
            }
        }

        #endregion

        #region Instance

        [Fact]
        public void Scalar_Function_Non_Static()
        {
            using (var context = CreateContext())
            {
                var custName = (from c in context.Customers
                                where c.Id == 1
                                select new
                                {
                                    Id = context.StarValueInstance(4, c.Id),
                                    LastName = context.DollarValueInstance(2, c.LastName)
                                }).Single();

                Assert.Equal(custName.LastName, "$$One");

                AssertSql(
                    @"SELECT TOP(2) [dbo].StarValue(4, [c].[Id]) AS [Id], [dbo].DollarValue(2, [c].[LastName]) AS [LastName]
FROM [Customers] AS [c]
WHERE [c].[Id] = 1");
            }
        }


        [Fact]
        private void Scalar_Function_Extension_Method_Instance()
        {
            using (var context = CreateContext())
            {
                var len = context.Customers.Count(c => context.IsDateInstance(c.FirstName) == false);

                Assert.Equal(3, len);

                AssertSql(
                    @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE CASE
    WHEN IsDate([c].[FirstName]) = 1
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END = 0");
            }
        }

        [Fact]
        private void Scalar_Function_With_Translator_Translates_Instance()
        {
            using (var context = CreateContext())
            {
                var customerId = 3;

                var len = context.Customers.Where(c => c.Id == customerId)
                    .Select(c => context.MyCustomLengthInstance(c.LastName)).Single();

                Assert.Equal(5, len);

                AssertSql(
                    @"@__customerId_0='3'

SELECT TOP(2) len([c].[LastName])
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0");
            }
        }

        [Fact]
        public void Scalar_Function_ClientEval_Method_As_Translateable_Method_Parameter_Instance()
        {
            using (var context = CreateContext())
            {
                Assert.Throws<NotImplementedException>(
                    () => (from c in context.Customers
                           where c.Id == 1
                           select new
                           {
                               c.FirstName,
                               OrderCount = context.CustomerOrderCountInstance(context.AddFiveInstance(c.Id - 5))
                           }).Single());
            }
        }

        [Fact]
        public void Scalar_Function_Constant_Parameter_Instance()
        {
            using (var context = CreateContext())
            {
                var customerId = 1;

                var custs = context.Customers.Select(c => context.CustomerOrderCountInstance(customerId)).ToList();

                Assert.Equal(3, custs.Count);

                AssertSql(
                    @"@__customerId_1='1'

SELECT [dbo].CustomerOrderCount(@__customerId_1)
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Function_Anonymous_Type_Select_Correlated_Instance()
        {
            using (var context = CreateContext())
            {
                var cust = (from c in context.Customers
                            where c.Id == 1
                            select new
                            {
                                c.LastName,
                                OrderCount = context.CustomerOrderCountInstance(c.Id)
                            }).Single();

                Assert.Equal("One", cust.LastName);
                Assert.Equal(3, cust.OrderCount);

                AssertSql(
                    @"SELECT TOP(2) [c].[LastName], [dbo].CustomerOrderCount([c].[Id]) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 1");
            }
        }

        [Fact]
        public void Scalar_Function_Anonymous_Type_Select_Not_Correlated_Instance()
        {
            using (var context = CreateContext())
            {
                var cust = (from c in context.Customers
                            where c.Id == 1
                            select new
                            {
                                c.LastName,
                                OrderCount = context.CustomerOrderCountInstance(1)
                            }).Single();

                Assert.Equal("One", cust.LastName);
                Assert.Equal(3, cust.OrderCount);

                AssertSql(
                    @"SELECT TOP(2) [c].[LastName], [dbo].CustomerOrderCount(1) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 1");
            }
        }

        [Fact]
        public void Scalar_Function_Anonymous_Type_Select_Parameter_Instance()
        {
            using (var context = CreateContext())
            {
                var customerId = 1;

                var cust = (from c in context.Customers
                            where c.Id == customerId
                            select new
                            {
                                c.LastName,
                                OrderCount = context.CustomerOrderCountInstance(customerId)
                            }).Single();

                Assert.Equal("One", cust.LastName);
                Assert.Equal(3, cust.OrderCount);

                AssertSql(
                    @"@__customerId_2='1'
@__customerId_0='1'

SELECT TOP(2) [c].[LastName], [dbo].CustomerOrderCount(@__customerId_2) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0");
            }
        }

        [Fact]
        public void Scalar_Function_Anonymous_Type_Select_Nested_Instance()
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
                                OrderCount = context.StarValueInstance(starCount, context.CustomerOrderCountInstance(customerId))
                            }).Single();

                Assert.Equal("Three", cust.LastName);
                Assert.Equal("***1", cust.OrderCount);

                AssertSql(
                    @"@__starCount_2='3'
@__customerId_4='3'
@__customerId_0='3'

SELECT TOP(2) [c].[LastName], [dbo].StarValue(@__starCount_2, [dbo].CustomerOrderCount(@__customerId_4)) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_0");
            }
        }

        [Fact]
        public void Scalar_Function_Where_Correlated_Instance()
        {
            using (var context = CreateContext())
            {
                var cust = (from c in context.Customers
                            where context.IsTopCustomerInstance(c.Id)
                            select c.Id.ToString().ToLower()).ToList();

                Assert.Equal(1, cust.Count);

                AssertSql(
                    @"SELECT LOWER(CONVERT(VARCHAR(11), [c].[Id]))
FROM [Customers] AS [c]
WHERE [dbo].IsTopCustomer([c].[Id]) = 1");
            }
        }

        [Fact]
        public void Scalar_Function_Where_Not_Correlated_Instance()
        {
            using (var context = CreateContext())
            {
                var startDate = DateTime.Parse("4/1/2000");

                var custId = (from c in context.Customers
                              where context.GetCustomerWithMostOrdersAfterDateInstance(startDate) == c.Id
                              select c.Id).SingleOrDefault();

                Assert.Equal(custId, 2);

                AssertSql(
                    @"@__startDate_1='2000-04-01T00:00:00'

SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [dbo].GetCustomerWithMostOrdersAfterDate(@__startDate_1) = [c].[Id]");
            }
        }

        [Fact]
        public void Scalar_Function_Where_Parameter_Instance()
        {
            using (var context = CreateContext())
            {
                var period = UDFSqlContext.ReportingPeriod.Winter;

                var custId = (from c in context.Customers
                              where c.Id == context.GetCustomerWithMostOrdersAfterDateInstance(
                                        context.GetReportingPeriodStartDateInstance(period))
                              select c.Id).SingleOrDefault();

                Assert.Equal(custId, 1);

                AssertSql(
                    @"@__period_2='0'

SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [c].[Id] = [dbo].GetCustomerWithMostOrdersAfterDate([dbo].GetReportingPeriodStartDate(@__period_2))");
            }
        }

        [Fact]
        public void Scalar_Function_Where_Nested_Instance()
        {
            using (var context = CreateContext())
            {
                var custId = (from c in context.Customers
                              where c.Id == context.GetCustomerWithMostOrdersAfterDateInstance(
                                        context.GetReportingPeriodStartDateInstance(
                                            UDFSqlContext.ReportingPeriod.Winter))
                              select c.Id).SingleOrDefault();

                Assert.Equal(custId, 1);

                AssertSql(
                    @"SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE [c].[Id] = [dbo].GetCustomerWithMostOrdersAfterDate([dbo].GetReportingPeriodStartDate(0))");
            }
        }

        [Fact]
        public void Scalar_Function_Let_Correlated_Instance()
        {
            using (var context = CreateContext())
            {
                var cust = (from c in context.Customers
                            let orderCount = context.CustomerOrderCountInstance(c.Id)
                            where c.Id == 2
                            select new
                            {
                                c.LastName,
                                OrderCount = orderCount
                            }).Single();

                Assert.Equal("Two", cust.LastName);
                Assert.Equal(2, cust.OrderCount);

                AssertSql(
                    @"SELECT TOP(2) [c].[LastName], [dbo].CustomerOrderCount([c].[Id]) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 2");
            }
        }

        [Fact]
        public void Scalar_Function_Let_Not_Correlated_Instance()
        {
            using (var context = CreateContext())
            {
                var cust = (from c in context.Customers
                            let orderCount = context.CustomerOrderCountInstance(2)
                            where c.Id == 2
                            select new
                            {
                                c.LastName,
                                OrderCount = orderCount
                            }).Single();

                Assert.Equal("Two", cust.LastName);
                Assert.Equal(2, cust.OrderCount);

                AssertSql(
                    @"SELECT TOP(2) [c].[LastName], [dbo].CustomerOrderCount(2) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = 2");
            }
        }

        [Fact]
        public void Scalar_Function_Let_Not_Parameter_Instance()
        {
            var customerId = 2;

            using (var context = CreateContext())
            {
                var cust = (from c in context.Customers
                            let orderCount = context.CustomerOrderCountInstance(customerId)
                            where c.Id == customerId
                            select new
                            {
                                c.LastName,
                                OrderCount = orderCount
                            }).Single();

                Assert.Equal("Two", cust.LastName);
                Assert.Equal(2, cust.OrderCount);

                AssertSql(
                    @"@__8__locals1_customerId_1='2'
@__8__locals1_customerId_2='2'

SELECT TOP(2) [c].[LastName], [dbo].CustomerOrderCount(@__8__locals1_customerId_1) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__8__locals1_customerId_2");
            }
        }

        [Fact]
        public void Scalar_Function_Let_Nested_Instance()
        {
            using (var context = CreateContext())
            {
                var customerId = 1;
                var starCount = 3;

                var cust = (from c in context.Customers
                            let orderCount = context.StarValueInstance(starCount, context.CustomerOrderCountInstance(customerId))
                            where c.Id == customerId
                            select new
                            {
                                c.LastName,
                                OrderCount = orderCount
                            }).Single();

                Assert.Equal("One", cust.LastName);
                Assert.Equal("***3", cust.OrderCount);

                AssertSql(
                    @"@__starCount_1='3'
@__customerId_3='1'
@__customerId_4='1'

SELECT TOP(2) [c].[LastName], [dbo].StarValue(@__starCount_1, [dbo].CustomerOrderCount(@__customerId_3)) AS [OrderCount]
FROM [Customers] AS [c]
WHERE [c].[Id] = @__customerId_4");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Unwind_Client_Eval_Where_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == context.AddOneInstance(c.Id)
                               select c.Id).Single();

                Assert.Equal(1, results);
                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested__Function_Unwind_Client_Eval_OrderBy_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               orderby context.AddOneInstance(c.Id)
                               select c.Id).ToList();

                Assert.Equal(3, results.Count);
                Assert.True(results.SequenceEqual(Enumerable.Range(1, 3)));

                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Unwind_Client_Eval_Select_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               orderby c.Id
                               select context.AddOneInstance(c.Id)).ToList();

                Assert.Equal(3, results.Count);
                Assert.True(results.SequenceEqual(Enumerable.Range(2, 3)));

                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]
ORDER BY [c].[Id]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Client_BCL_UDF_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == context.AddOneInstance(Math.Abs(context.CustomerOrderCountWithClientInstance(c.Id)))
                               select c.Id).Single();

                Assert.Equal(3, results);
                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Client_UDF_BCL_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == context.AddOneInstance(context.CustomerOrderCountWithClientInstance(Math.Abs(c.Id)))
                               select c.Id).Single();

                Assert.Equal(3, results);
                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_BCL_Client_UDF_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == Math.Abs(context.AddOneInstance(context.CustomerOrderCountWithClientInstance(c.Id)))
                               select c.Id).Single();

                Assert.Equal(3, results);
                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_BCL_UDF_Client_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 1 == Math.Abs(context.CustomerOrderCountWithClientInstance(context.AddOneInstance(c.Id)))
                               select c.Id).Single();

                Assert.Equal(2, results);
                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_UDF_BCL_Client_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 1 == context.CustomerOrderCountWithClientInstance(Math.Abs(context.AddOneInstance(c.Id)))
                               select c.Id).Single();

                Assert.Equal(2, results);
                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_UDF_Client_BCL_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 1 == context.CustomerOrderCountWithClientInstance(context.AddOneInstance(Math.Abs(c.Id)))
                               select c.Id).Single();

                Assert.Equal(2, results);
                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Client_BCL_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 3 == context.AddOneInstance(Math.Abs(c.Id))
                               select c.Id).Single();

                Assert.Equal(2, results);
                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_Client_UDF_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == context.AddOneInstance(context.CustomerOrderCountWithClientInstance(c.Id))
                               select c.Id).Single();

                Assert.Equal(3, results);
                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_BCL_Client_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 3 == Math.Abs(context.AddOneInstance(c.Id))
                               select c.Id).Single();

                Assert.Equal(2, results);
                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_BCL_UDF_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 3 == Math.Abs(context.CustomerOrderCountInstance(c.Id))
                               select c.Id).Single();

                Assert.Equal(1, results);
                AssertSql(
                    @"SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE 3 = ABS([dbo].CustomerOrderCount([c].[Id]))");
            }
        }


        [Fact]
        public void Scalar_Nested_Function_UDF_Client_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == context.CustomerOrderCountWithClientInstance(context.AddOneInstance(c.Id))
                               select c.Id).Single();

                Assert.Equal(1, results);
                AssertSql(
                    @"SELECT [c].[Id]
FROM [Customers] AS [c]");
            }
        }

        [Fact]
        public void Scalar_Nested_Function_UDF_BCL_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 3 == context.CustomerOrderCountInstance(Math.Abs(c.Id))
                               select c.Id).Single();

                Assert.Equal(1, results);
                AssertSql(
                    @"SELECT TOP(2) [c].[Id]
FROM [Customers] AS [c]
WHERE 3 = [dbo].CustomerOrderCount(ABS([c].[Id]))");
            }
        }

        #endregion

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

                context.Database.ExecuteSqlCommand(@"create function[dbo].[DollarValue] (@starCount int, @value nvarchar(max))
                                                    returns nvarchar(max)
                                                        as
                                                        begin
                                                    return replicate('$', @starCount) + @value
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

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
