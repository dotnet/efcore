// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class UdfDbFunctionTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : SharedStoreFixtureBase<DbContext>, new()
    {
        protected UdfDbFunctionTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        protected UDFSqlContext CreateContext() => (UDFSqlContext)Fixture.CreateContext();

        #region Model

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

        protected class UDFSqlContext : PoolableDbContext
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

            public static long MyCustomLengthStatic(string s) => throw new Exception();
            public static bool IsDateStatic(string date) => throw new Exception();
            public static int AddOneStatic(int num) => num + 1;
            public static int AddFiveStatic(int number) => number + 5;
            public static int CustomerOrderCountStatic(int customerId) => throw new NotImplementedException();

            public static int CustomerOrderCountWithClientStatic(int customerId) => customerId switch
            {
                1 => 3,
                2 => 2,
                3 => 1,
                4 => 0,
                _ => throw new Exception()
            };

            public static string StarValueStatic(int starCount, int value) => throw new NotImplementedException();
            public static bool IsTopCustomerStatic(int customerId) => throw new NotImplementedException();
            public static int GetCustomerWithMostOrdersAfterDateStatic(DateTime? startDate) => throw new NotImplementedException();
            public static DateTime? GetReportingPeriodStartDateStatic(ReportingPeriod periodId) => throw new NotImplementedException();
            public static string GetSqlFragmentStatic() => throw new NotImplementedException();

            public long MyCustomLengthInstance(string s) => throw new Exception();
            public bool IsDateInstance(string date) => throw new Exception();
            public int AddOneInstance(int num) => num + 1;
            public int AddFiveInstance(int number) => number + 5;
            public int CustomerOrderCountInstance(int customerId) => throw new NotImplementedException();

            public int CustomerOrderCountWithClientInstance(int customerId) => customerId switch
            {
                1 => 3,
                2 => 2,
                3 => 1,
                4 => 0,
                _ => throw new Exception()
            };

            public string StarValueInstance(int starCount, int value) => throw new NotImplementedException();
            public bool IsTopCustomerInstance(int customerId) => throw new NotImplementedException();
            public int GetCustomerWithMostOrdersAfterDateInstance(DateTime? startDate) => throw new NotImplementedException();
            public DateTime? GetReportingPeriodStartDateInstance(ReportingPeriod periodId) => throw new NotImplementedException();
            public string DollarValueInstance(int starCount, string value) => throw new NotImplementedException();

            [DbFunction(Schema = "dbo")]
            public static string IdentityString(string s) => throw new Exception();

            #endregion

            public UDFSqlContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                //Static
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(CustomerOrderCountStatic))).HasName("CustomerOrderCount");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(CustomerOrderCountWithClientStatic)))
                    .HasName("CustomerOrderCount");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(StarValueStatic))).HasName("StarValue");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(IsTopCustomerStatic))).HasName("IsTopCustomer");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetCustomerWithMostOrdersAfterDateStatic)))
                    .HasName("GetCustomerWithMostOrdersAfterDate");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetReportingPeriodStartDateStatic)))
                    .HasName("GetReportingPeriodStartDate");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetSqlFragmentStatic)))
                    .HasTranslation(args => new SqlFragmentExpression("'Two'"));
                var isDateMethodInfo = typeof(UDFSqlContext).GetMethod(nameof(IsDateStatic));
                modelBuilder.HasDbFunction(isDateMethodInfo)
                    .HasTranslation(args => SqlFunctionExpression.Create(
                        "IsDate",
                        args,
                        nullResultAllowed: true,
                        argumentsPropagateNullability: args.Select(a => true).ToList(),
                        isDateMethodInfo.ReturnType,
                        null));

                var methodInfo = typeof(UDFSqlContext).GetMethod(nameof(MyCustomLengthStatic));

                modelBuilder.HasDbFunction(methodInfo)
                    .HasTranslation(args => SqlFunctionExpression.Create(
                        "len",
                        args,
                        nullResultAllowed: true,
                        argumentsPropagateNullability: args.Select(a => true).ToList(),
                        methodInfo.ReturnType,
                        null));

                //Instance
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(CustomerOrderCountInstance)))
                    .HasName("CustomerOrderCount");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(CustomerOrderCountWithClientInstance)))
                    .HasName("CustomerOrderCount");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(StarValueInstance))).HasName("StarValue");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(IsTopCustomerInstance))).HasName("IsTopCustomer");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetCustomerWithMostOrdersAfterDateInstance)))
                    .HasName("GetCustomerWithMostOrdersAfterDate");
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetReportingPeriodStartDateInstance)))
                    .HasName("GetReportingPeriodStartDate");
                var isDateMethodInfo2 = typeof(UDFSqlContext).GetMethod(nameof(IsDateInstance));
                modelBuilder.HasDbFunction(isDateMethodInfo2)
                    .HasTranslation(args => SqlFunctionExpression.Create(
                        "IsDate",
                        args,
                        nullResultAllowed: true,
                        argumentsPropagateNullability: args.Select(a => true).ToList(),
                        isDateMethodInfo2.ReturnType,
                        null));

                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(DollarValueInstance))).HasName("DollarValue");

                var methodInfo2 = typeof(UDFSqlContext).GetMethod(nameof(MyCustomLengthInstance));

                modelBuilder.HasDbFunction(methodInfo2)
                    .HasTranslation(args => SqlFunctionExpression.Create(
                        "len",
                        args,
                        nullResultAllowed: true,
                        argumentsPropagateNullability: args.Select(a => true).ToList(),
                        methodInfo2.ReturnType,
                        null));
            }
        }

        public abstract class UdfFixtureBase : SharedStoreFixtureBase<DbContext>
        {
            protected override Type ContextType { get; } = typeof(UDFSqlContext);

            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

            protected override bool ShouldLogCategory(string logCategory)
                => logCategory == DbLoggerCategory.Query.Name;

            protected override void Seed(DbContext context)
            {
                context.Database.EnsureCreatedResiliently();

                var order11 = new Order
                {
                    Name = "Order11",
                    ItemCount = 4,
                    OrderDate = new DateTime(2000, 1, 20)
                };
                var order12 = new Order
                {
                    Name = "Order12",
                    ItemCount = 8,
                    OrderDate = new DateTime(2000, 2, 21)
                };
                var order13 = new Order
                {
                    Name = "Order13",
                    ItemCount = 15,
                    OrderDate = new DateTime(2000, 3, 20)
                };
                var order21 = new Order
                {
                    Name = "Order21",
                    ItemCount = 16,
                    OrderDate = new DateTime(2000, 4, 21)
                };
                var order22 = new Order
                {
                    Name = "Order22",
                    ItemCount = 23,
                    OrderDate = new DateTime(2000, 5, 20)
                };
                var order31 = new Order
                {
                    Name = "Order31",
                    ItemCount = 42,
                    OrderDate = new DateTime(2000, 6, 21)
                };

                var customer1 = new Customer
                {
                    FirstName = "Customer",
                    LastName = "One",
                    Orders = new List<Order>
                    {
                        order11,
                        order12,
                        order13
                    }
                };
                var customer2 = new Customer
                {
                    FirstName = "Customer",
                    LastName = "Two",
                    Orders = new List<Order> { order21, order22 }
                };
                var customer3 = new Customer
                {
                    FirstName = "Customer",
                    LastName = "Three",
                    Orders = new List<Order> { order31 }
                };

                ((UDFSqlContext)context).Customers.AddRange(customer1, customer2, customer3);
                ((UDFSqlContext)context).Orders.AddRange(order11, order12, order13, order21, order22, order31);
            }
        }

        #endregion

        #region Scalar Tests

        #region Static

        [ConditionalFact]
        public virtual void Scalar_Function_Extension_Method_Static()
        {
            using var context = CreateContext();

            var len = context.Customers.Count(c => UDFSqlContext.IsDateStatic(c.FirstName) == false);

            Assert.Equal(3, len);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_With_Translator_Translates_Static()
        {
            using var context = CreateContext();
            var customerId = 3;

            var len = context.Customers.Where(c => c.Id == customerId)
                .Select(c => UDFSqlContext.MyCustomLengthStatic(c.LastName)).Single();

            Assert.Equal(5, len);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_ClientEval_Method_As_Translateable_Method_Parameter_Static()
        {
            using var context = CreateContext();

            Assert.Throws<NotImplementedException>(
                () => (from c in context.Customers
                       where c.Id == 1
                       select new
                       {
                           c.FirstName, OrderCount = UDFSqlContext.CustomerOrderCountStatic(UDFSqlContext.AddFiveStatic(c.Id - 5))
                       }).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Constant_Parameter_Static()
        {
            using var context = CreateContext();
            var customerId = 1;

            var custs = context.Customers.Select(c => UDFSqlContext.CustomerOrderCountStatic(customerId)).ToList();

            Assert.Equal(3, custs.Count);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Correlated_Static()
        {
            using var context = CreateContext();

            var cust = (from c in context.Customers
                        where c.Id == 1
                        select new { c.LastName, OrderCount = UDFSqlContext.CustomerOrderCountStatic(c.Id) }).Single();

            Assert.Equal("One", cust.LastName);
            Assert.Equal(3, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Not_Correlated_Static()
        {
            using var context = CreateContext();

            var cust = (from c in context.Customers
                        where c.Id == 1
                        select new { c.LastName, OrderCount = UDFSqlContext.CustomerOrderCountStatic(1) }).Single();

            Assert.Equal("One", cust.LastName);
            Assert.Equal(3, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Parameter_Static()
        {
            using var context = CreateContext();
            var customerId = 1;

            var cust = (from c in context.Customers
                        where c.Id == customerId
                        select new { c.LastName, OrderCount = UDFSqlContext.CustomerOrderCountStatic(customerId) }).Single();

            Assert.Equal("One", cust.LastName);
            Assert.Equal(3, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Nested_Static()
        {
            using var context = CreateContext();
            var customerId = 3;
            var starCount = 3;

            var cust = (from c in context.Customers
                        where c.Id == customerId
                        select new
                        {
                            c.LastName,
                            OrderCount = UDFSqlContext.StarValueStatic(
                                starCount, UDFSqlContext.CustomerOrderCountStatic(customerId))
                        }).Single();

            Assert.Equal("Three", cust.LastName);
            Assert.Equal("***1", cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Where_Correlated_Static()
        {
            using var context = CreateContext();

            var cust = (from c in context.Customers
                        where UDFSqlContext.IsTopCustomerStatic(c.Id)
                        select c.Id.ToString().ToLower()).ToList();

            Assert.Single(cust);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Where_Not_Correlated_Static()
        {
            using var context = CreateContext();
            var startDate = new DateTime(2000, 4, 1);

            var custId = (from c in context.Customers
                          where UDFSqlContext.GetCustomerWithMostOrdersAfterDateStatic(startDate) == c.Id
                          select c.Id).SingleOrDefault();

            Assert.Equal(2, custId);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Where_Parameter_Static()
        {
            using var context = CreateContext();
            var period = UDFSqlContext.ReportingPeriod.Winter;

            var custId = (from c in context.Customers
                          where c.Id
                              == UDFSqlContext.GetCustomerWithMostOrdersAfterDateStatic(
                                  UDFSqlContext.GetReportingPeriodStartDateStatic(period))
                          select c.Id).SingleOrDefault();

            Assert.Equal(1, custId);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Where_Nested_Static()
        {
            using var context = CreateContext();

            var custId = (from c in context.Customers
                          where c.Id
                              == UDFSqlContext.GetCustomerWithMostOrdersAfterDateStatic(
                                  UDFSqlContext.GetReportingPeriodStartDateStatic(
                                      UDFSqlContext.ReportingPeriod.Winter))
                          select c.Id).SingleOrDefault();

            Assert.Equal(1, custId);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Let_Correlated_Static()
        {
            using var context = CreateContext();

            var cust = (from c in context.Customers
                        let orderCount = UDFSqlContext.CustomerOrderCountStatic(c.Id)
                        where c.Id == 2
                        select new { c.LastName, OrderCount = orderCount }).Single();

            Assert.Equal("Two", cust.LastName);
            Assert.Equal(2, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Let_Not_Correlated_Static()
        {
            using var context = CreateContext();

            var cust = (from c in context.Customers
                        let orderCount = UDFSqlContext.CustomerOrderCountStatic(2)
                        where c.Id == 2
                        select new { c.LastName, OrderCount = orderCount }).Single();

            Assert.Equal("Two", cust.LastName);
            Assert.Equal(2, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Let_Not_Parameter_Static()
        {
            using var context = CreateContext();
            var customerId = 2;

            var cust = (from c in context.Customers
                        let orderCount = UDFSqlContext.CustomerOrderCountStatic(customerId)
                        where c.Id == customerId
                        select new { c.LastName, OrderCount = orderCount }).Single();

            Assert.Equal("Two", cust.LastName);
            Assert.Equal(2, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Let_Nested_Static()
        {
            using var context = CreateContext();
            var customerId = 1;
            var starCount = 3;

            var cust = (from c in context.Customers
                        let orderCount = UDFSqlContext.StarValueStatic(starCount, UDFSqlContext.CustomerOrderCountStatic(customerId))
                        where c.Id == customerId
                        select new { c.LastName, OrderCount = orderCount }).Single();

            Assert.Equal("One", cust.LastName);
            Assert.Equal("***3", cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Unwind_Client_Eval_Where_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == UDFSqlContext.AddOneStatic(c.Id)
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Unwind_Client_Eval_OrderBy_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       orderby UDFSqlContext.AddOneStatic(c.Id)
                       select c.Id).ToList());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Unwind_Client_Eval_Select_Static()
        {
            using var context = CreateContext();

            var results = (from c in context.Customers
                           orderby c.Id
                           select UDFSqlContext.AddOneStatic(c.Id)).ToList();

            Assert.Equal(3, results.Count);
            Assert.True(results.SequenceEqual(Enumerable.Range(2, 3)));
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Client_BCL_UDF_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == UDFSqlContext.AddOneStatic(Math.Abs(UDFSqlContext.CustomerOrderCountWithClientStatic(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Client_UDF_BCL_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == UDFSqlContext.AddOneStatic(UDFSqlContext.CustomerOrderCountWithClientStatic(Math.Abs(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_BCL_Client_UDF_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == Math.Abs(UDFSqlContext.AddOneStatic(UDFSqlContext.CustomerOrderCountWithClientStatic(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_BCL_UDF_Client_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 1 == Math.Abs(UDFSqlContext.CustomerOrderCountWithClientStatic(UDFSqlContext.AddOneStatic(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_UDF_BCL_Client_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 1 == UDFSqlContext.CustomerOrderCountWithClientStatic(Math.Abs(UDFSqlContext.AddOneStatic(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_UDF_Client_BCL_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 1 == UDFSqlContext.CustomerOrderCountWithClientStatic(UDFSqlContext.AddOneStatic(Math.Abs(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Client_BCL_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 3 == UDFSqlContext.AddOneStatic(Math.Abs(c.Id))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Client_UDF_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == UDFSqlContext.AddOneStatic(UDFSqlContext.CustomerOrderCountWithClientStatic(c.Id))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_BCL_Client_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 3 == Math.Abs(UDFSqlContext.AddOneStatic(c.Id))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_BCL_UDF_Static()
        {
            using var context = CreateContext();

            var results = (from c in context.Customers
                           where 3 == Math.Abs(UDFSqlContext.CustomerOrderCountStatic(c.Id))
                           select c.Id).Single();

            Assert.Equal(1, results);
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_UDF_Client_Static()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == UDFSqlContext.CustomerOrderCountWithClientStatic(UDFSqlContext.AddOneStatic(c.Id))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_UDF_BCL_Static()
        {
            using var context = CreateContext();

            var results = (from c in context.Customers
                           where 3 == UDFSqlContext.CustomerOrderCountStatic(Math.Abs(c.Id))
                           select c.Id).Single();

            Assert.Equal(1, results);
        }

        [ConditionalFact]
        public virtual void Nullable_navigation_property_access_preserves_schema_for_sql_function()
        {
            using var context = CreateContext();

            var result = context.Orders
                .OrderBy(o => o.Id)
                .Select(o => UDFSqlContext.IdentityString(o.Customer.FirstName))
                .FirstOrDefault();

            Assert.Equal("Customer", result);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_SqlFragment_Static()
        {
            using var context = CreateContext();

            var len = context.Customers.Count(c => c.LastName == UDFSqlContext.GetSqlFragmentStatic());

            Assert.Equal(1, len);
        }

        #endregion

        #region Instance

        [ConditionalFact]
        public virtual void Scalar_Function_Non_Static()
        {
            using var context = CreateContext();

            var custName = (from c in context.Customers
                            where c.Id == 1
                            select new { Id = context.StarValueInstance(4, c.Id), LastName = context.DollarValueInstance(2, c.LastName) })
                .Single();

            Assert.Equal("$$One", custName.LastName);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Extension_Method_Instance()
        {
            using var context = CreateContext();

            var len = context.Customers.Count(c => context.IsDateInstance(c.FirstName) == false);

            Assert.Equal(3, len);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_With_Translator_Translates_Instance()
        {
            using var context = CreateContext();
            var customerId = 3;

            var len = context.Customers.Where(c => c.Id == customerId)
                .Select(c => context.MyCustomLengthInstance(c.LastName)).Single();

            Assert.Equal(5, len);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_ClientEval_Method_As_Translateable_Method_Parameter_Instance()
        {
            using var context = CreateContext();

            Assert.Throws<NotImplementedException>(
                () => (from c in context.Customers
                       where c.Id == 1
                       select new { c.FirstName, OrderCount = context.CustomerOrderCountInstance(context.AddFiveInstance(c.Id - 5)) })
                    .Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Constant_Parameter_Instance()
        {
            using var context = CreateContext();
            var customerId = 1;

            var custs = context.Customers.Select(c => context.CustomerOrderCountInstance(customerId)).ToList();

            Assert.Equal(3, custs.Count);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Correlated_Instance()
        {
            using var context = CreateContext();

            var cust = (from c in context.Customers
                        where c.Id == 1
                        select new { c.LastName, OrderCount = context.CustomerOrderCountInstance(c.Id) }).Single();

            Assert.Equal("One", cust.LastName);
            Assert.Equal(3, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Not_Correlated_Instance()
        {
            using var context = CreateContext();

            var cust = (from c in context.Customers
                        where c.Id == 1
                        select new { c.LastName, OrderCount = context.CustomerOrderCountInstance(1) }).Single();

            Assert.Equal("One", cust.LastName);
            Assert.Equal(3, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Parameter_Instance()
        {
            using var context = CreateContext();
            var customerId = 1;

            var cust = (from c in context.Customers
                        where c.Id == customerId
                        select new { c.LastName, OrderCount = context.CustomerOrderCountInstance(customerId) }).Single();

            Assert.Equal("One", cust.LastName);
            Assert.Equal(3, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Nested_Instance()
        {
            using var context = CreateContext();
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
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Where_Correlated_Instance()
        {
            using var context = CreateContext();

            var cust = (from c in context.Customers
                        where context.IsTopCustomerInstance(c.Id)
                        select c.Id.ToString().ToLower()).ToList();

            Assert.Single(cust);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Where_Not_Correlated_Instance()
        {
            using var context = CreateContext();
            var startDate = new DateTime(2000, 4, 1);

            var custId = (from c in context.Customers
                          where context.GetCustomerWithMostOrdersAfterDateInstance(startDate) == c.Id
                          select c.Id).SingleOrDefault();

            Assert.Equal(2, custId);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Where_Parameter_Instance()
        {
            using var context = CreateContext();
            var period = UDFSqlContext.ReportingPeriod.Winter;

            var custId = (from c in context.Customers
                          where c.Id
                              == context.GetCustomerWithMostOrdersAfterDateInstance(
                                  context.GetReportingPeriodStartDateInstance(period))
                          select c.Id).SingleOrDefault();

            Assert.Equal(1, custId);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Where_Nested_Instance()
        {
            using var context = CreateContext();

            var custId = (from c in context.Customers
                          where c.Id
                              == context.GetCustomerWithMostOrdersAfterDateInstance(
                                  context.GetReportingPeriodStartDateInstance(
                                      UDFSqlContext.ReportingPeriod.Winter))
                          select c.Id).SingleOrDefault();

            Assert.Equal(1, custId);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Let_Correlated_Instance()
        {
            using var context = CreateContext();

            var cust = (from c in context.Customers
                        let orderCount = context.CustomerOrderCountInstance(c.Id)
                        where c.Id == 2
                        select new { c.LastName, OrderCount = orderCount }).Single();

            Assert.Equal("Two", cust.LastName);
            Assert.Equal(2, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Let_Not_Correlated_Instance()
        {
            using var context = CreateContext();

            var cust = (from c in context.Customers
                        let orderCount = context.CustomerOrderCountInstance(2)
                        where c.Id == 2
                        select new { c.LastName, OrderCount = orderCount }).Single();

            Assert.Equal("Two", cust.LastName);
            Assert.Equal(2, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Let_Not_Parameter_Instance()
        {
            using var context = CreateContext();
            var customerId = 2;

            var cust = (from c in context.Customers
                        let orderCount = context.CustomerOrderCountInstance(customerId)
                        where c.Id == customerId
                        select new { c.LastName, OrderCount = orderCount }).Single();

            Assert.Equal("Two", cust.LastName);
            Assert.Equal(2, cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Function_Let_Nested_Instance()
        {
            using var context = CreateContext();
            var customerId = 1;
            var starCount = 3;

            var cust = (from c in context.Customers
                        let orderCount = context.StarValueInstance(starCount, context.CustomerOrderCountInstance(customerId))
                        where c.Id == customerId
                        select new { c.LastName, OrderCount = orderCount }).Single();

            Assert.Equal("One", cust.LastName);
            Assert.Equal("***3", cust.OrderCount);
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Unwind_Client_Eval_Where_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == context.AddOneInstance(c.Id)
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Unwind_Client_Eval_OrderBy_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       orderby context.AddOneInstance(c.Id)
                       select c.Id).ToList());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Unwind_Client_Eval_Select_Instance()
        {
            using var context = CreateContext();

            var results = (from c in context.Customers
                           orderby c.Id
                           select context.AddOneInstance(c.Id)).ToList();

            Assert.Equal(3, results.Count);
            Assert.True(results.SequenceEqual(Enumerable.Range(2, 3)));
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Client_BCL_UDF_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == context.AddOneInstance(Math.Abs(context.CustomerOrderCountWithClientInstance(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Client_UDF_BCL_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == context.AddOneInstance(context.CustomerOrderCountWithClientInstance(Math.Abs(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_BCL_Client_UDF_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == Math.Abs(context.AddOneInstance(context.CustomerOrderCountWithClientInstance(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_BCL_UDF_Client_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 1 == Math.Abs(context.CustomerOrderCountWithClientInstance(context.AddOneInstance(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_UDF_BCL_Client_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 1 == context.CustomerOrderCountWithClientInstance(Math.Abs(context.AddOneInstance(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_UDF_Client_BCL_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 1 == context.CustomerOrderCountWithClientInstance(context.AddOneInstance(Math.Abs(c.Id)))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Client_BCL_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 3 == context.AddOneInstance(Math.Abs(c.Id))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_Client_UDF_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == context.AddOneInstance(context.CustomerOrderCountWithClientInstance(c.Id))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_BCL_Client_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 3 == Math.Abs(context.AddOneInstance(c.Id))
                       select c.Id).Single());
        }

        public static Exception AssertThrows<T>(Func<object> testCode)
            where T : Exception, new()
        {
            testCode();

            return new T();
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_BCL_UDF_Instance()
        {
            using var context = CreateContext();
            var results = (from c in context.Customers
                           where 3 == Math.Abs(context.CustomerOrderCountInstance(c.Id))
                           select c.Id).Single();

            Assert.Equal(1, results);
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_UDF_Client_Instance()
        {
            using var context = CreateContext();

            AssertTranslationFailed(
                () => (from c in context.Customers
                       where 2 == context.CustomerOrderCountWithClientInstance(context.AddOneInstance(c.Id))
                       select c.Id).Single());
        }

        [ConditionalFact]
        public virtual void Scalar_Nested_Function_UDF_BCL_Instance()
        {
            using var context = CreateContext();

            var results = (from c in context.Customers
                           where 3 == context.CustomerOrderCountInstance(Math.Abs(c.Id))
                           select c.Id).Single();

            Assert.Equal(1, results);
        }

        #endregion

        #endregion

        private void AssertTranslationFailed(Action testCode)
            => Assert.Contains(
                CoreStrings.TranslationFailed("").Substring(21),
                Assert.Throws<InvalidOperationException>(testCode).Message);
    }
}
