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
    public class UdfDbFunctionOracleTests : IClassFixture<UdfDbFunctionOracleTests.OracleUDFFixture>
    {
        public UdfDbFunctionOracleTests(OracleUDFFixture fixture) => Fixture = fixture;

        private OracleUDFFixture Fixture { get; }

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
            }
        }

        [Fact]
        public void Scalar_Function_Where_Not_Correlated()
        {
            using (var context = CreateContext())
            {
                var startDate = DateTime.Parse("4/1/2000");

                var custId = (from c in context.Customers
                              where UDFSqlContext.GetCustomerWithMostOrdersAfterDate(startDate) == c.Id
                              select c.Id).SingleOrDefault();

                Assert.Equal(custId, 1);
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
            }
        }

        #endregion

        public class OracleUDFFixture : SharedStoreFixtureBase<DbContext>
        {
            protected override string StoreName { get; } = "UDFDbFunctionOracleTests";
            protected override Type ContextType { get; } = typeof(UDFSqlContext);
            protected override ITestStoreFactory TestStoreFactory => OracleTestStoreFactory.Instance;

            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                base.AddOptions(builder);
                return builder.ConfigureWarnings(w => w.Ignore(RelationalEventId.QueryClientEvaluationWarning));
            }

            protected override void Seed(DbContext context)
            {
                context.Database.EnsureCreated();

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION CustomerOrderCount (customerId INTEGER)
RETURN INTEGER IS
  result INTEGER;
BEGIN
  SELECT COUNT(""Id"") 
  INTO result
  FROM ""Orders"" 
  WHERE ""CustomerId"" = customerId;
  RETURN result;
END;");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION StarValue (starCount INTEGER, value NVARCHAR2)
RETURN NVARCHAR2 IS
BEGIN
  RETURN LPAD(value, starCount + LENGTH(value), '*');
END;");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION GetReportingPeriodStartDate (period INTEGER)
RETURN TIMESTAMP IS
BEGIN
	RETURN TO_TIMESTAMP('01/01/1998', 'MM/DD/YYYY');
END;");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION GetCustomerWithMostOrdersAfterDate (searchDate TIMESTAMP)
RETURN INTEGER IS
  result INTEGER;
BEGIN
  SELECT ""CustomerId""
  INTO result
  FROM ""Orders""
  WHERE ""OrderDate"" > searchDate
  GROUP BY ""CustomerId""
  ORDER BY COUNT(""Id"") DESC
  FETCH FIRST 1 ROWS ONLY;
  RETURN result;
END;");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION IsTopCustomer (customerId INTEGER)
RETURN INTEGER IS
BEGIN
  IF (customerId = 1) THEN
    RETURN 1;
  ELSE	
    RETURN 0;
  END IF;
END;");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION IsDate (value NVARCHAR2)
RETURN INTEGER IS
BEGIN
  RETURN 0;
END;");

                context.Database.ExecuteSqlCommand(
                    @"CREATE OR REPLACE FUNCTION Len (value NVARCHAR2)
RETURN INTEGER IS
BEGIN
  RETURN LENGTH(value);
END;");

                var order11 = new Order { Name = "Order11", ItemCount = 4, OrderDate = DateTime.Parse("1/20/2000") };
                var order12 = new Order { Name = "Order12", ItemCount = 8, OrderDate = DateTime.Parse("2/21/2000") };
                var order13 = new Order { Name = "Order13", ItemCount = 15, OrderDate = DateTime.Parse("3/20/2000") };
                var order21 = new Order { Name = "Order21", ItemCount = 16, OrderDate = DateTime.Parse("4/21/2000") };
                var order22 = new Order { Name = "Order22", ItemCount = 23, OrderDate = DateTime.Parse("5/20/2000") };
                var order31 = new Order { Name = "Order31", ItemCount = 42, OrderDate = DateTime.Parse("6/21/2000") };

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
