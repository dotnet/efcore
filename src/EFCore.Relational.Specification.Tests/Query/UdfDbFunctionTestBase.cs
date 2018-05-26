// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.Expressions;
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
            public int QuantitySold { get; set; }
            public DateTime OrderDate { get; set; }
            public Customer Customer { get; set; }
            public Product Product { get; set; }
        }

        public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        protected class UDFSqlContext : PoolableDbContext
        {
            #region DbSets

            public DbSet<Customer> Customers { get; set; }
            public DbSet<Order> Orders { get; set; }
            public DbSet<Product> Products { get; set; }

            #endregion

            #region Function Stubs

            #region Static Functions

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
                    case 5:
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
                    case 5:
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

            [DbFunction(Schema = "dbo")]
            public static string IdentityString(string s)
            {
                throw new NotImplementedException();
            }

            public string SCHEMA_NAME()
            {
                return Execute(() => SCHEMA_NAME());
            }

            public Task<string> SCHEMA_NAME_Async()
            {
                return ExecuteAsync(() => SCHEMA_NAME());
            }

            public int AddValues(int a, int b)
            {
                 return Execute(() => AddValues(a, b));
            }

            public int AddValues(Expression<Func<int>> a, int b)
            {
                  return Execute(() => AddValues(a, b));
            }

            #endregion

            #region Table Functions

            public class OrderByYear
            {
                public int? CustomerId { get; set; }
                public int? Count { get; set; }
                public int? Year { get; set; }
            }

            public IQueryable<OrderByYear> GetCustomerOrderCountByYear(int customerId)
            {
                return CreateQuery(() => GetCustomerOrderCountByYear(customerId));
            }

            public IQueryable<OrderByYear> GetCustomerOrderCountByYear(Expression<Func<int>> customerId)
            {
                return CreateQuery(() => GetCustomerOrderCountByYear(customerId));
            }

            public class TopSellingProduct
            {
                public int? ProductId { get; set; }
                public int? AmountSold { get; set; }
            }

            public IQueryable<TopSellingProduct> GetTopTwoSellingProducts()
            {
                return CreateQuery(() => GetTopTwoSellingProducts());
            }

            public IQueryable<TopSellingProduct> GetTopTwoSellingProductsCustomTranslation()
            {
                return CreateQuery(() => GetTopTwoSellingProductsCustomTranslation());
            }

            #endregion

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

                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(AddValues), new[] { typeof(int), typeof(int) }));

                var methodInfo2 = typeof(UDFSqlContext).GetMethod(nameof(MyCustomLengthInstance));

                modelBuilder.HasDbFunction(methodInfo2)
                    .HasTranslation(args => new SqlFunctionExpression("len", methodInfo2.ReturnType, args));

                //Bootstrap
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(SCHEMA_NAME))).HasName("SCHEMA_NAME").HasSchema("");

                //Table
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetCustomerOrderCountByYear), new[] { typeof(int) }));
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetTopTwoSellingProducts)));
                modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetTopTwoSellingProductsCustomTranslation)))
                    .HasTranslation(args => new SqlFunctionExpression("GetTopTwoSellingProducts", typeof(TopSellingProduct), "dbo", args));
            }
        }

        public abstract class BaseUdfFixture : SharedStoreFixtureBase<DbContext>
        {
            protected override Type ContextType { get; } = typeof(UDFSqlContext);

            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                base.AddOptions(builder);
                return builder.ConfigureWarnings(w => w.Ignore(RelationalEventId.QueryClientEvaluationWarning));
            }

            protected override void Seed(DbContext context)
            {
                context.Database.EnsureCreatedResiliently();

                var product1 = new Product
                {
                    Name = "Product1"
                };
                var product2 = new Product
                {
                    Name = "Product2"
                };
                var product3 = new Product
                {
                    Name = "Product3"
                };
                var product4 = new Product
                {
                    Name = "Product4"
                };
                var product5 = new Product
                {
                    Name = "Product5"
                };

                var order11 = new Order
                {
                    Name = "Order11",
                    QuantitySold = 4,
                    OrderDate = new DateTime(2000, 1, 20),
                    Product = product1
                };
                var order12 = new Order
                {
                    Name = "Order12",
                    QuantitySold = 8,
                    OrderDate = new DateTime(2000, 2, 21),
                    Product = product2
                };
                var order13 = new Order
                {
                    Name = "Order13",
                    QuantitySold = 15,
                    OrderDate = new DateTime(2001, 3, 20),
                    Product = product3
                };
                var order21 = new Order
                {
                    Name = "Order21",
                    QuantitySold = 16,
                    OrderDate = new DateTime(2000, 4, 21),
                    Product = product4
                };
                var order22 = new Order
                {
                    Name = "Order22",
                    QuantitySold = 23,
                    OrderDate = new DateTime(2000, 5, 20),
                    Product = product1
                };
                var order31 = new Order
                {
                    Name = "Order31",
                    QuantitySold = 42,
                    OrderDate = new DateTime(2001, 6, 21),
                    Product = product2
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
                    Orders = new List<Order>
                    {
                        order21,
                        order22
                    }
                };
                var customer3 = new Customer
                {
                    FirstName = "Customer",
                    LastName = "Three",
                    Orders = new List<Order>
                    {
                        order31
                    }
                };
                var customer4 = new Customer
                {
                    FirstName = "Customer",
                    LastName = "Four"
                };

                ((UDFSqlContext)context).Customers.AddRange(customer1, customer2, customer3, customer4);
                ((UDFSqlContext)context).Orders.AddRange(order11, order12, order13, order21, order22, order31);
                ((UDFSqlContext)context).Products.AddRange(product1, product2, product3, product4, product5);
            }
        }

        #endregion

        #region Scalar Tests

        #region Static

       [Fact]
        public virtual void Scalar_Function_Extension_Method_Static()
        {
            using (var context = CreateContext())
            {
                var len = context.Customers.Count(c => UDFSqlContext.IsDateStatic(c.FirstName) == false);

                Assert.Equal(4, len);
            }
        }

        [Fact]
        public virtual void Scalar_Function_With_Translator_Translates_Static()
        {
            using (var context = CreateContext())
            {
                var customerId = 3;

                var len = context.Customers.Where(c => c.Id == customerId)
                    .Select(c => UDFSqlContext.MyCustomLengthStatic(c.LastName)).Single();

                Assert.Equal(5, len);
            }
        }

        [Fact]
        public virtual void Scalar_Function_ClientEval_Method_As_Translateable_Method_Parameter_Static()
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
        public virtual void Scalar_Function_Constant_Parameter_Static()
        {
            using (var context = CreateContext())
            {
                var customerId = 1;

                var custs = context.Customers.Select(c => UDFSqlContext.CustomerOrderCountStatic(customerId)).ToList();

                Assert.Equal(4, custs.Count);
            }
        }

        [Fact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Correlated_Static()
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
            }
        }

        [Fact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Not_Correlated_Static()
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
            }
        }

        [Fact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Parameter_Static()
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
            }
        }

        [Fact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Nested_Static()
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
            }
        }

        [Fact]
        public virtual void Scalar_Function_Where_Correlated_Static()
        {
            using (var context = CreateContext())
            {
                var cust = (from c in context.Customers
                            where UDFSqlContext.IsTopCustomerStatic(c.Id)
                            select c.Id.ToString().ToLower()).ToList();

                Assert.Equal(1, cust.Count);
            }
        }

        [Fact]
        public virtual void Scalar_Function_Where_Not_Correlated_Static()
        {
            using (var context = CreateContext())
            {
                var startDate = new DateTime(2000, 4, 1);

                var custId = (from c in context.Customers
                              where UDFSqlContext.GetCustomerWithMostOrdersAfterDateStatic(startDate) == c.Id
                              select c.Id).SingleOrDefault();

                Assert.Equal(custId, 2);
            }
        }

        [Fact]
        public virtual void Scalar_Function_Where_Parameter_Static()
        {
            using (var context = CreateContext())
            {
                var period = UDFSqlContext.ReportingPeriod.Winter;

                var custId = (from c in context.Customers
                              where c.Id == UDFSqlContext.GetCustomerWithMostOrdersAfterDateStatic(
                                        UDFSqlContext.GetReportingPeriodStartDateStatic(period))
                              select c.Id).SingleOrDefault();

                Assert.Equal(custId, 1);
            }
        }

        [Fact]
        public virtual void Scalar_Function_Where_Nested_Static()
        {
            using (var context = CreateContext())
            {
                var custId = (from c in context.Customers
                              where c.Id == UDFSqlContext.GetCustomerWithMostOrdersAfterDateStatic(
                                        UDFSqlContext.GetReportingPeriodStartDateStatic(
                                            UDFSqlContext.ReportingPeriod.Winter))
                              select c.Id).SingleOrDefault();

                Assert.Equal(custId, 1);
            }
        }

        [Fact]
        public virtual void Scalar_Function_Let_Correlated_Static()
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
            }
        }

        [Fact]
        public virtual void Scalar_Function_Let_Not_Correlated_Static()
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
            }
        }

        [Fact]
        public virtual void Scalar_Function_Let_Not_Parameter_Static()
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
            }
        }

        [Fact]
        public virtual void Scalar_Function_Let_Nested_Static()
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
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_Unwind_Client_Eval_Where_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == UDFSqlContext.AddOneStatic(c.Id)
                               select c.Id).Single();

                Assert.Equal(1, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_Unwind_Client_Eval_OrderBy_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               orderby UDFSqlContext.AddOneStatic(c.Id)
                               select c.Id).ToList();

                Assert.Equal(4, results.Count);
                Assert.True(results.SequenceEqual(Enumerable.Range(1, 4)));
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_Unwind_Client_Eval_Select_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               orderby c.Id
                               select UDFSqlContext.AddOneStatic(c.Id)).ToList();

                Assert.Equal(4, results.Count);
                Assert.True(results.SequenceEqual(Enumerable.Range(2, 4)));
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_Client_BCL_UDF_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == UDFSqlContext.AddOneStatic(Math.Abs(UDFSqlContext.CustomerOrderCountWithClientStatic(c.Id)))
                               select c.Id).Single();

                Assert.Equal(3, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_Client_UDF_BCL_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == UDFSqlContext.AddOneStatic(UDFSqlContext.CustomerOrderCountWithClientStatic(Math.Abs(c.Id)))
                               select c.Id).Single();

                Assert.Equal(3, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_BCL_Client_UDF_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == Math.Abs(UDFSqlContext.AddOneStatic(UDFSqlContext.CustomerOrderCountWithClientStatic(c.Id)))
                               select c.Id).Single();

                Assert.Equal(3, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_BCL_UDF_Client_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 1 == Math.Abs(UDFSqlContext.CustomerOrderCountWithClientStatic(UDFSqlContext.AddOneStatic(c.Id)))
                               select c.Id).Single();

                Assert.Equal(2, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_UDF_BCL_Client_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 1 == UDFSqlContext.CustomerOrderCountWithClientStatic(Math.Abs(UDFSqlContext.AddOneStatic(c.Id)))
                               select c.Id).Single();

                Assert.Equal(2, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_UDF_Client_BCL_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 1 == UDFSqlContext.CustomerOrderCountWithClientStatic(UDFSqlContext.AddOneStatic(Math.Abs(c.Id)))
                               select c.Id).Single();

                Assert.Equal(2, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_Client_BCL_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 3 == UDFSqlContext.AddOneStatic(Math.Abs(c.Id))
                               select c.Id).Single();

                Assert.Equal(2, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_Client_UDF_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == UDFSqlContext.AddOneStatic(UDFSqlContext.CustomerOrderCountWithClientStatic(c.Id))
                               select c.Id).Single();

                Assert.Equal(3, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_BCL_Client_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 3 == Math.Abs(UDFSqlContext.AddOneStatic(c.Id))
                               select c.Id).Single();

                Assert.Equal(2, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_BCL_UDF_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 3 == Math.Abs(UDFSqlContext.CustomerOrderCountStatic(c.Id))
                               select c.Id).Single();

                Assert.Equal(1, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_UDF_Client_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == UDFSqlContext.CustomerOrderCountWithClientStatic(UDFSqlContext.AddOneStatic(c.Id))
                               select c.Id).Single();

                Assert.Equal(1, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_UDF_BCL_Static()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 3 == UDFSqlContext.CustomerOrderCountStatic(Math.Abs(c.Id))
                               select c.Id).Single();

                Assert.Equal(1, results);
            }
        }

        [Fact]
        public virtual void Nullable_navigation_property_access_preserves_schema_for_sql_function()
        {
            using (var context = CreateContext())
            {
                var result = context.Orders
                    .OrderBy(o => o.Id)
                    .Select(o => UDFSqlContext.IdentityString(o.Customer.FirstName))
                    .FirstOrDefault();

                Assert.Equal("Customer", result);
            }
        }

        #endregion

        #region Instance

        [Fact]
        public virtual void Scalar_Function_Non_Static()
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
            }
        }

        [Fact]
        public virtual void Scalar_Function_Extension_Method_Instance()
        {
            using (var context = CreateContext())
            {
                var len = context.Customers.Count(c => context.IsDateInstance(c.FirstName) == false);

                Assert.Equal(4, len);
            }
        }

        [Fact]
        public virtual void Scalar_Function_With_Translator_Translates_Instance()
        {
            using (var context = CreateContext())
            {
                var customerId = 3;

                var len = context.Customers.Where(c => c.Id == customerId)
                    .Select(c => context.MyCustomLengthInstance(c.LastName)).Single();

                Assert.Equal(5, len);
            }
        }

        [Fact]
        public virtual void Scalar_Function_ClientEval_Method_As_Translateable_Method_Parameter_Instance()
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
        public virtual void Scalar_Function_Constant_Parameter_Instance()
        {
            using (var context = CreateContext())
            {
                var customerId = 1;

                var custs = context.Customers.Select(c => context.CustomerOrderCountInstance(customerId)).ToList();

                Assert.Equal(4, custs.Count);
            }
        }

        [Fact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Correlated_Instance()
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
            }
        }

        [Fact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Not_Correlated_Instance()
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
            }
        }

        [Fact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Parameter_Instance()
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
            }
        }

        [Fact]
        public virtual void Scalar_Function_Anonymous_Type_Select_Nested_Instance()
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
            }
        }

        [Fact]
        public virtual void Scalar_Function_Where_Correlated_Instance()
        {
            using (var context = CreateContext())
            {
                var cust = (from c in context.Customers
                            where context.IsTopCustomerInstance(c.Id)
                            select c.Id.ToString().ToLower()).ToList();

                Assert.Equal(1, cust.Count);
            }
        }

        [Fact]
        public virtual void Scalar_Function_Where_Not_Correlated_Instance()
        {
            using (var context = CreateContext())
            {
                var startDate = new DateTime(2000, 4, 1);

                var custId = (from c in context.Customers
                              where context.GetCustomerWithMostOrdersAfterDateInstance(startDate) == c.Id
                              select c.Id).SingleOrDefault();

                Assert.Equal(custId, 2);
            }
        }

        [Fact]
        public virtual void Scalar_Function_Where_Parameter_Instance()
        {
            using (var context = CreateContext())
            {
                var period = UDFSqlContext.ReportingPeriod.Winter;

                var custId = (from c in context.Customers
                              where c.Id == context.GetCustomerWithMostOrdersAfterDateInstance(
                                        context.GetReportingPeriodStartDateInstance(period))
                              select c.Id).SingleOrDefault();

                Assert.Equal(custId, 1);
            }
        }

        [Fact]
        public virtual void Scalar_Function_Where_Nested_Instance()
        {
            using (var context = CreateContext())
            {
                var custId = (from c in context.Customers
                              where c.Id == context.GetCustomerWithMostOrdersAfterDateInstance(
                                        context.GetReportingPeriodStartDateInstance(
                                            UDFSqlContext.ReportingPeriod.Winter))
                              select c.Id).SingleOrDefault();

                Assert.Equal(custId, 1);
            }
        }

        [Fact]
        public virtual void Scalar_Function_Let_Correlated_Instance()
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
            }
        }

        [Fact]
        public virtual void Scalar_Function_Let_Not_Correlated_Instance()
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
            }
        }

        [Fact]
        public virtual void Scalar_Function_Let_Not_Parameter_Instance()
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
            }
        }

        [Fact]
        public virtual void Scalar_Function_Let_Nested_Instance()
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
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_Unwind_Client_Eval_Where_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == context.AddOneInstance(c.Id)
                               select c.Id).Single();

                Assert.Equal(1, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_Unwind_Client_Eval_OrderBy_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               orderby context.AddOneInstance(c.Id)
                               select c.Id).ToList();

                Assert.Equal(4, results.Count);
                Assert.True(results.SequenceEqual(Enumerable.Range(1, 4)));
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_Unwind_Client_Eval_Select_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               orderby c.Id
                               select context.AddOneInstance(c.Id)).ToList();

                Assert.Equal(4, results.Count);
                Assert.True(results.SequenceEqual(Enumerable.Range(2, 4)));
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_Client_BCL_UDF_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == context.AddOneInstance(Math.Abs(context.CustomerOrderCountWithClientInstance(c.Id)))
                               select c.Id).Single();

                Assert.Equal(3, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_Client_UDF_BCL_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == context.AddOneInstance(context.CustomerOrderCountWithClientInstance(Math.Abs(c.Id)))
                               select c.Id).Single();

                Assert.Equal(3, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_BCL_Client_UDF_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == Math.Abs(context.AddOneInstance(context.CustomerOrderCountWithClientInstance(c.Id)))
                               select c.Id).Single();

                Assert.Equal(3, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_BCL_UDF_Client_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 1 == Math.Abs(context.CustomerOrderCountWithClientInstance(context.AddOneInstance(c.Id)))
                               select c.Id).Single();

                Assert.Equal(2, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_UDF_BCL_Client_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 1 == context.CustomerOrderCountWithClientInstance(Math.Abs(context.AddOneInstance(c.Id)))
                               select c.Id).Single();

                Assert.Equal(2, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_UDF_Client_BCL_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 1 == context.CustomerOrderCountWithClientInstance(context.AddOneInstance(Math.Abs(c.Id)))
                               select c.Id).Single();

                Assert.Equal(2, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_Client_BCL_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 3 == context.AddOneInstance(Math.Abs(c.Id))
                               select c.Id).Single();

                Assert.Equal(2, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_Client_UDF_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == context.AddOneInstance(context.CustomerOrderCountWithClientInstance(c.Id))
                               select c.Id).Single();

                Assert.Equal(3, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_BCL_Client_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 3 == Math.Abs(context.AddOneInstance(c.Id))
                               select c.Id).Single();

                Assert.Equal(2, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_BCL_UDF_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 3 == Math.Abs(context.CustomerOrderCountInstance(c.Id))
                               select c.Id).Single();

                Assert.Equal(1, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_UDF_Client_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 2 == context.CustomerOrderCountWithClientInstance(context.AddOneInstance(c.Id))
                               select c.Id).Single();

                Assert.Equal(1, results);
            }
        }

        [Fact]
        public virtual void Scalar_Nested_Function_UDF_BCL_Instance()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where 3 == context.CustomerOrderCountInstance(Math.Abs(c.Id))
                               select c.Id).Single();

                Assert.Equal(1, results);
            }
        }

        #endregion

        #endregion

        #region BootStrap

        [Fact]
        public virtual void BootstrapScalarNoParams()
        {
            using (var context = CreateContext())
            {
                var schame = context.SCHEMA_NAME();

                Assert.Equal("dbo", schame);
            }
        }

        [Fact]
        public virtual async Task BootstrapScalarNoParamsAsync()
        {
            using (var context = CreateContext())
            {
                var schema = await context.SCHEMA_NAME_Async();

                Assert.Equal("dbo", schema);
            }
        }

        [Fact]
        public virtual void BootstrapScalarParams()
        {
            using (var context = CreateContext())
            {
                var value = context.AddValues(1, 2);

                Assert.Equal(3, value);
            }
        }

        [Fact]
        public virtual void BootstrapScalarFuncParams()
        {
            using (var context = CreateContext())
            {
                var value = context.AddValues(() => context.AddValues(1, 2), 2);

                Assert.Equal(5, value);
            }
        }

        [Fact]
        public virtual void BootstrapScalarFuncParamsWithVariable()
        {
            using (var context = CreateContext())
            {
                var x = 5;
                var value = context.AddValues(() => context.AddValues(x, 2), 2);

                Assert.Equal(9, value);
            }
        }

        [Fact]
        public virtual void BootstrapScalarFuncParamsConstant()
        {
            using (var context = CreateContext())
            {
                var value = context.AddValues(() => 1, 2);

                Assert.Equal(3, value);
            }
        }
        #endregion

        #region Table Valued Tests

        [Fact]
        public virtual void TVF_Stand_Alone()
        {
            using (var context = CreateContext())
            {
                var products = (from t in context.GetTopTwoSellingProducts()
                                orderby t.ProductId
                                select t).ToList();

                Assert.Equal(2, products.Count);
                Assert.Equal(1, products[0].ProductId);
                Assert.Equal(27, products[0].AmountSold);
                Assert.Equal(2, products[1].ProductId);
                Assert.Equal(50, products[1].AmountSold);
            }
        }

        [Fact]
        public virtual void TVF_Stand_Alone_With_Translation()
        {
            using (var context = CreateContext())
            {
                var products = (from t in context.GetTopTwoSellingProductsCustomTranslation()
                                orderby t.ProductId
                                select t).ToList();

                Assert.Equal(2, products.Count);
                Assert.Equal(1, products[0].ProductId);
                Assert.Equal(27, products[0].AmountSold);
                Assert.Equal(2, products[1].ProductId);
                Assert.Equal(50, products[1].AmountSold);
            }
        }

        [Fact]
        public virtual void TVF_Stand_Alone_Parameter()
        {
            using (var context = CreateContext())
            {
                var orders = (from c in context.GetCustomerOrderCountByYear(1)
                              orderby c.Count descending
                              select c).ToList();

                Assert.Equal(2, orders.Count);
                Assert.Equal(2, orders[0].Count);
                Assert.Equal(2000, orders[0].Year);
                Assert.Equal(1, orders[1].Count);
                Assert.Equal(2001, orders[1].Year);
            }
        }

        [Fact]
        public virtual void TVF_Stand_Alone_Nested()
        {
            using (var context = CreateContext())
            {
                var orders = (from r in context.GetCustomerOrderCountByYear(() => context.AddValues(-2, 3))
                              orderby r.Count descending
                              select r).ToList();

                Assert.Equal(2, orders.Count);
                Assert.Equal(2, orders[0].Count);
                Assert.Equal(2000, orders[0].Year);
                Assert.Equal(1, orders[1].Count);
                Assert.Equal(2001, orders[1].Year);
            }
        }

        [Fact]
        public virtual void TVF_CrossApply_Correlated_Select_Anonymous()
        {
            using (var context = CreateContext())
            {
                var orders = (from c in context.Customers
                              from r in context.GetCustomerOrderCountByYear(c.Id)
                              orderby c.Id, r.Year
                              select new
                              {
                                  c.Id,
                                  c.LastName,
                                  r.Year,
                                  r.Count
                              }).ToList();

                Assert.Equal(4, orders.Count);
                Assert.Equal(2, orders[0].Count);
                Assert.Equal(1, orders[1].Count);
                Assert.Equal(2, orders[2].Count);
                Assert.Equal(1, orders[3].Count);
                Assert.Equal(2000, orders[0].Year);
                Assert.Equal(2001, orders[1].Year);
                Assert.Equal(2000, orders[2].Year);
                Assert.Equal(2001, orders[3].Year);
                Assert.Equal(1, orders[0].Id);
                Assert.Equal(1, orders[1].Id);
                Assert.Equal(2, orders[2].Id);
                Assert.Equal(3, orders[3].Id);
            }
        }

        [Fact]
        public virtual void TVF_Select_Direct_In_Anonymous()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               select new
                               {
                                   c.Id,
                                   Prods = context.GetTopTwoSellingProducts().ToList(),
                               }).ToList();

                Assert.Equal(4, results.Count);
                Assert.Equal(2, results[0].Prods.Count);
                Assert.Equal(2, results[1].Prods.Count);
                Assert.Equal(2, results[2].Prods.Count);
                Assert.Equal(2, results[3].Prods.Count);
            }
        }

        [Fact]
        public virtual void TVF_Select_Correlated_Direct_In_Anonymous()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               select new
                               {
                                   c.Id,
                                   OrderCountYear = context.GetCustomerOrderCountByYear(c.Id).ToList()
                               }).ToList();

                Assert.Equal(4, results.Count);
                Assert.Equal(1, results[0].Id);
                Assert.Equal(2, results[1].Id);
                Assert.Equal(3, results[2].Id);
                Assert.Equal(4, results[3].Id);
                Assert.Equal(2, results[0].OrderCountYear.Count);
                Assert.Equal(1, results[1].OrderCountYear.Count);
                Assert.Equal(1, results[2].OrderCountYear.Count);
                Assert.Equal(0, results[3].OrderCountYear.Count);
            }
        }

        [Fact]
        public virtual void TVF_Select_Correlated_Direct_With_Function_Query_Parameter_Correlated_In_Anonymous()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               where c.Id == 1
                               select new
                               {
                                   c.Id,
                                   OrderCountYear = context.GetCustomerOrderCountByYear(context.AddValues(c.Id, 1)).ToList()
                               }).ToList();

                Assert.Equal(1, results.Count);
                Assert.Equal(1, results[0].Id);
                Assert.Equal(1, results[0].OrderCountYear.Count);
            }
        }

        [Fact]
        public virtual void TVF_Select_Correlated_Subquery_In_Anonymous()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               select new
                               {
                                   c.Id,
                                   OrderCountYear = context.GetCustomerOrderCountByYear(c.Id).Where(o => o.Year == 2000).ToList()
                               }).ToList();

                Assert.Equal(4, results.Count);
                Assert.Equal(1, results[0].Id);
                Assert.Equal(2, results[1].Id);
                Assert.Equal(3, results[2].Id);
                Assert.Equal(4, results[3].Id);
                Assert.Equal(1, results[0].OrderCountYear.Count);
                Assert.Equal(1, results[1].OrderCountYear.Count);
                Assert.Equal(0, results[2].OrderCountYear.Count);
                Assert.Equal(0, results[3].OrderCountYear.Count);
            }
        }

        [Fact]
        public virtual void TVF_Select_Correlated_Subquery_In_Anonymous_Nested()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               select new
                               {
                                   c.Id,
                                   OrderCountYear = context.GetCustomerOrderCountByYear(1).Where(o => o.Year == 2000).Select(o => new
                                   {
                                       OrderCountYearNested = context.GetCustomerOrderCountByYear(2000).Where(o2 => o.Year == 2001).ToList(),
                                       Prods = context.GetTopTwoSellingProducts().ToList(),
                                   }).ToList()
                               }).ToList();

                Assert.Equal(4, results.Count);
                Assert.Equal(1, results[0].OrderCountYear.Count);
                Assert.Equal(2, results[0].OrderCountYear[0].Prods.Count);
                Assert.Equal(0, results[0].OrderCountYear[0].OrderCountYearNested.Count);
                Assert.Equal(1, results[1].OrderCountYear.Count);
                Assert.Equal(2, results[1].OrderCountYear[0].Prods.Count);
                Assert.Equal(0, results[1].OrderCountYear[0].OrderCountYearNested.Count);
                Assert.Equal(1, results[2].OrderCountYear.Count);
                Assert.Equal(2, results[2].OrderCountYear[0].Prods.Count);
                Assert.Equal(0, results[2].OrderCountYear[0].OrderCountYearNested.Count);
                Assert.Equal(1, results[3].OrderCountYear.Count);
                Assert.Equal(2, results[3].OrderCountYear[0].Prods.Count);
                Assert.Equal(0, results[3].OrderCountYear[0].OrderCountYearNested.Count);
            }
        }

        [Fact]
        public virtual void TVF_Select_NonCorrelated_Subquery_In_Anonymous()
        {
            using (var context = CreateContext())
            {
                var results = (from c in context.Customers
                               select new
                               {
                                   c.Id,
                                   Prods = context.GetTopTwoSellingProducts().Where(p => p.AmountSold == 27).Select(p => p.ProductId).ToList(),
                               }).ToList();

                Assert.Equal(4, results.Count);
                Assert.Equal(1, results[0].Prods.Count);
                Assert.Equal(1, results[1].Prods.Count);
                Assert.Equal(1, results[2].Prods.Count);
                Assert.Equal(1, results[3].Prods.Count);
            }
        }

        [Fact]
        public virtual void TVF_Select_NonCorrelated_Subquery_In_Anonymous_Parameter()
        {
            using (var context = CreateContext())
            {
                var amount = 27;

                var results = (from c in context.Customers
                               select new
                               {
                                   c.Id,
                                   Prods = context.GetTopTwoSellingProducts().Where(p => p.AmountSold == amount).Select(p => p.ProductId).ToList(),
                               }).ToList();

                Assert.Equal(4, results.Count);
                Assert.Equal(1, results[0].Prods.Count);
                Assert.Equal(1, results[1].Prods.Count);
                Assert.Equal(1, results[2].Prods.Count);
                Assert.Equal(1, results[3].Prods.Count);
            }
        }

        [Fact]
        public virtual void TVF_Correlated_Select_In_Anonymous()
        {
            using (var context = CreateContext())
            {
                var cust = (from c in context.Customers
                            orderby c.Id
                            select new
                            {
                                c.Id,
                                c.LastName,
                                Orders = context.GetCustomerOrderCountByYear(c.Id).ToList()
                            }).ToList();

                Assert.Equal(4, cust.Count);
                Assert.Equal(2, cust[0].Orders[0].Count);
                Assert.Equal(2, cust[1].Orders[0].Count);
                Assert.Equal(1, cust[2].Orders[0].Count);
                Assert.Equal(0, cust[3].Orders.Count);
                Assert.Equal("One", cust[0].LastName);
                Assert.Equal("Two", cust[1].LastName);
                Assert.Equal("Three", cust[2].LastName);
                Assert.Equal("Four", cust[3].LastName);
                Assert.Equal(1, cust[0].Id);
                Assert.Equal(2, cust[1].Id);
                Assert.Equal(3, cust[2].Id);
                Assert.Equal(4, cust[3].Id);
            }
        }

        [Fact]
        public virtual void TVF_CrossApply_Correlated_Select_Result()
        {
            using (var context = CreateContext())
            {
                var orders = (from c in context.Customers
                              from r in context.GetCustomerOrderCountByYear(c.Id)
                              orderby r.Count descending, r.Year descending
                              select r).ToList();

                Assert.Equal(4, orders.Count);

                Assert.Equal(4, orders.Count);
                Assert.Equal(2, orders[0].Count);
                Assert.Equal(2, orders[1].Count);
                Assert.Equal(1, orders[2].Count);
                Assert.Equal(1, orders[3].Count);
                Assert.Equal(2000, orders[0].Year);
                Assert.Equal(2000, orders[1].Year);
                Assert.Equal(2001, orders[2].Year);
                Assert.Equal(2001, orders[3].Year);
            }
        }

        [Fact]
        public virtual void TVF_CrossJoin_Not_Correlated()
        {
            using (var context = CreateContext())
            {
                var orders = (from c in context.Customers
                              from r in context.GetCustomerOrderCountByYear(2)
                              where c.Id == 2
                              orderby r.Count
                              select new
                              {
                                  c.Id,
                                  c.LastName,
                                  r.Year,
                                  r.Count
                              }).ToList();

                Assert.Equal(1, orders.Count);

                Assert.Equal(2, orders[0].Count);
                Assert.Equal(2000, orders[0].Year);
            }
        }

        [Fact]
        public virtual void TVF_CrossJoin_Parameter()
        {
            using (var context = CreateContext())
            {
                var custId = 2;

                var orders = (from c in context.Customers
                              from r in context.GetCustomerOrderCountByYear(custId)
                              where c.Id == custId
                              orderby r.Count
                              select new
                              {
                                  c.Id,
                                  c.LastName,
                                  r.Year,
                                  r.Count
                              }).ToList();

                Assert.Equal(1, orders.Count);

                Assert.Equal(2, orders[0].Count);
                Assert.Equal(2000, orders[0].Year);
            }
        }

        [Fact]
        public virtual void TVF_Join()
        {
            using (var context = CreateContext())
            {
                var products = (from p in context.Products
                                join r in context.GetTopTwoSellingProducts() on p.Id equals r.ProductId
                                select new
                                {
                                    p.Id,
                                    p.Name,
                                    r.AmountSold
                                }).OrderBy(p => p.Id).ToList();

                Assert.Equal(2, products.Count);
                Assert.Equal(1, products[0].Id);
                Assert.Equal("Product1", products[0].Name);
                Assert.Equal(27, products[0].AmountSold);
                Assert.Equal(2, products[1].Id);
                Assert.Equal("Product2", products[1].Name);
                Assert.Equal(50, products[1].AmountSold);
            }
        }

        [Fact]
        public virtual void TVF_LeftJoin_Select_Anonymous()
        {
            using (var context = CreateContext())
            {
                var products = (from p in context.Products
                                join r in context.GetTopTwoSellingProducts() on p.Id equals r.ProductId into joinTable
                                from j in joinTable.DefaultIfEmpty()
                                orderby p.Id descending
                                select new
                                {
                                    p.Id,
                                    p.Name,
                                    j.AmountSold
                                }).ToList();

                Assert.Equal(5, products.Count);
                Assert.Equal(5, products[0].Id);
                Assert.Equal(null, products[0].AmountSold);
                Assert.Equal("Product5", products[0].Name);
                Assert.Equal(4, products[1].Id);
                Assert.Equal(null, products[1].AmountSold);
                Assert.Equal("Product4", products[1].Name);
                Assert.Equal(3, products[2].Id);
                Assert.Equal(null, products[2].AmountSold);
                Assert.Equal("Product3", products[2].Name);
                Assert.Equal(2, products[3].Id);
                Assert.Equal(50, products[3].AmountSold);
                Assert.Equal("Product2", products[3].Name);
                Assert.Equal(1, products[4].Id);
                Assert.Equal(27, products[4].AmountSold);
                Assert.Equal("Product1", products[4].Name);
            }
        }

        [Fact]
        public virtual void TVF_LeftJoin_Select_Result()
        {
            using (var context = CreateContext())
            {
                var products = (from p in context.Products
                                join r in context.GetTopTwoSellingProducts() on p.Id equals r.ProductId into joinTable
                                from j in joinTable.DefaultIfEmpty()
                                orderby p.Id descending
                                select j).ToList();

                Assert.Equal(5, products.Count);
                Assert.Equal(null, products[0].ProductId);
                Assert.Equal(null, products[0].AmountSold);
                Assert.Equal(null, products[1].ProductId);
                Assert.Equal(null, products[1].AmountSold);
                Assert.Equal(null, products[2].ProductId);
                Assert.Equal(null, products[2].AmountSold);
                Assert.Equal(2, products[3].ProductId);
                Assert.Equal(50, products[3].AmountSold);
                Assert.Equal(1, products[4].ProductId);
                Assert.Equal(27, products[4].AmountSold);
            }
        }

        [Fact]
        public virtual void TVF_OuterApply_Correlated_Select_TVF()
        {
            using (var context = CreateContext())
            {
                var orders = (from c in context.Customers
                              from r in context.GetCustomerOrderCountByYear(c.Id).DefaultIfEmpty()
                              orderby c.Id, r.Year
                              select r).ToList();

                Assert.Equal(5, orders.Count);

                Assert.Equal(2, orders[0].Count);
                Assert.Equal(1, orders[1].Count);
                Assert.Equal(2, orders[2].Count);
                Assert.Equal(1, orders[3].Count);
                Assert.Null(orders[4].Count);
                Assert.Equal(2000, orders[0].Year);
                Assert.Equal(2001, orders[1].Year);
                Assert.Equal(2000, orders[2].Year);
                Assert.Equal(2001, orders[3].Year);
                Assert.Null(orders[4].Year);
                Assert.Equal(1, orders[0].CustomerId);
                Assert.Equal(1, orders[1].CustomerId);
                Assert.Equal(2, orders[2].CustomerId);
                Assert.Equal(3, orders[3].CustomerId);
                Assert.Null(orders[4].CustomerId);
            }
        }

        [Fact]
        public virtual void TVF_OuterApply_Correlated_Select_DbSet()
        {
            using (var context = CreateContext())
            {
                var custs = (from c in context.Customers
                             from r in context.GetCustomerOrderCountByYear(c.Id).DefaultIfEmpty()
                             orderby c.Id, r.Year
                             select c).ToList();

                Assert.Equal(5, custs.Count);

                Assert.Equal(1, custs[0].Id);
                Assert.Equal(1, custs[1].Id);
                Assert.Equal(2, custs[2].Id);
                Assert.Equal(3, custs[3].Id);
                Assert.Equal(4, custs[4].Id);
                Assert.Equal("One", custs[0].LastName);
                Assert.Equal("One", custs[1].LastName);
                Assert.Equal("Two", custs[2].LastName);
                Assert.Equal("Three", custs[3].LastName);
                Assert.Equal("Four", custs[4].LastName);
            }
        }

        [Fact]
        public virtual void TVF_OuterApply_Correlated_Select_Anonymous()
        {
            using (var context = CreateContext())
            {
                var orders = (from c in context.Customers
                              from r in context.GetCustomerOrderCountByYear(c.Id).DefaultIfEmpty()
                              orderby c.Id, r.Year
                              select new
                              {
                                  c.Id,
                                  c.LastName,
                                  r.Year,
                                  r.Count
                              }).ToList();

                Assert.Equal(5, orders.Count);

                Assert.Equal(1, orders[0].Id);
                Assert.Equal(1, orders[1].Id);
                Assert.Equal(2, orders[2].Id);
                Assert.Equal(3, orders[3].Id);
                Assert.Equal(4, orders[4].Id);
                Assert.Equal("One", orders[0].LastName);
                Assert.Equal("One", orders[1].LastName);
                Assert.Equal("Two", orders[2].LastName);
                Assert.Equal("Three", orders[3].LastName);
                Assert.Equal("Four", orders[4].LastName);
                Assert.Equal(2, orders[0].Count);
                Assert.Equal(1, orders[1].Count);
                Assert.Equal(2, orders[2].Count);
                Assert.Equal(1, orders[3].Count);
                Assert.Null(orders[4].Count);
                Assert.Equal(2000, orders[0].Year);
                Assert.Equal(2001, orders[1].Year);
                Assert.Equal(2000, orders[2].Year);
                Assert.Equal(2001, orders[3].Year);
            }
        }

        [Fact]
        public virtual void TVF_Nested()
        {
            using (var context = CreateContext())
            {
                var custId = 2;

                var orders = (from c in context.Customers
                              from r in context.GetCustomerOrderCountByYear(context.AddValues(1, 1))
                              where c.Id == custId
                              orderby r.Year
                              select new
                              {
                                  c.Id,
                                  c.LastName,
                                  r.Year,
                                  r.Count
                              }).ToList();

                Assert.Equal(1, orders.Count);

                Assert.Equal(2, orders[0].Count);
                Assert.Equal(2000, orders[0].Year);
            }
        }


        [Fact]
        public virtual void TVF_Correlated_Nested_Func_Call()
        {
            var custId = 2;

            using (var context = CreateContext())
            {
                var orders = (from c in context.Customers
                              from r in context.GetCustomerOrderCountByYear(context.AddValues(c.Id, 1))
                              where c.Id == custId
                              select new
                              {
                                  c.Id,
                                  r.Count,
                                  r.Year
                              }).ToList();

                Assert.Equal(1, orders.Count);

                Assert.Equal(1, orders[0].Count);
                Assert.Equal(2001, orders[0].Year);
            }
        }

        #endregion
    }
}
