// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class UdfDbFunctionTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : SharedStoreFixtureBase<DbContext>, new()
{
    protected UdfDbFunctionTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

    protected UDFSqlContext CreateContext()
        => (UDFSqlContext)Fixture.CreateContext();

    #region Model

    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public List<Order> Orders { get; set; }
        public List<Address> Addresses { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime OrderDate { get; set; }

        public int CustomerId { get; set; }

        public Customer Customer { get; set; }
        public List<LineItem> Items { get; set; }
    }

    public class LineItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        public Order Order { get; set; }
        public Product Product { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Address
    {
        public int Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
    }

    public class OrderByYear
    {
        public int? CustomerId { get; set; }
        public int? Count { get; set; }
        public int? Year { get; set; }
    }

    public class MultProductOrders
    {
        public int OrderId { get; set; }

        public Customer Customer { get; set; }
        public int CustomerId { get; set; }

        public DateTime OrderDate { get; set; }
    }

    public class TopSellingProduct
    {
        public Product Product { get; set; }
        public int? ProductId { get; set; }

        public int? AmountSold { get; set; }
    }

    public class CustomerData
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    protected class UDFSqlContext(DbContextOptions options) : PoolableDbContext(options)
    {
        #region DbSets

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Address> Addresses { get; set; }

        #endregion

        #region Function Stubs

        public enum ReportingPeriod
        {
            Winter = 0,
            Spring,
            Summer,
            Fall
        }

        [DbFunction("len", IsBuiltIn = true)]
        public static long MyCustomLengthStatic(string s)
            => throw new Exception();

        public static bool IsDateStatic(string date)
            => throw new Exception();

        public static int AddOneStatic(int num)
            => num + 1;

        public static int AddFiveStatic(int number)
            => number + 5;

        public static int CustomerOrderCountStatic(int customerId)
            => throw new NotImplementedException();

        public static int CustomerOrderCountWithClientStatic(int customerId)
            => customerId switch
            {
                1 => 3,
                2 => 2,
                3 => 1,
                4 => 0,
                _ => throw new Exception()
            };

        public static string StarValueStatic(int starCount, int value)
            => throw new NotImplementedException();

        public static bool IsTopCustomerStatic(int customerId)
            => throw new NotImplementedException();

        public static int GetCustomerWithMostOrdersAfterDateStatic(DateTime? startDate)
            => throw new NotImplementedException();

        public static DateTime? GetReportingPeriodStartDateStatic(ReportingPeriod periodId)
            => throw new NotImplementedException();

        public static string GetSqlFragmentStatic()
            => throw new NotImplementedException();

        public static bool IsABC(string name)
            => throw new NotImplementedException();

        public static bool IsOrIsNotABC(string name)
            => throw new NotImplementedException();

        public long MyCustomLengthInstance(string s)
            => throw new Exception();

        public bool IsDateInstance(string date)
            => throw new Exception();

        public int AddOneInstance(int num)
            => num + 1;

        public int AddFiveInstance(int number)
            => number + 5;

        public int CustomerOrderCountInstance(int customerId)
            => throw new NotImplementedException();

        public int CustomerOrderCountWithClientInstance(int customerId)
            => customerId switch
            {
                1 => 3,
                2 => 2,
                3 => 1,
                4 => 0,
                _ => throw new Exception()
            };

        public string StarValueInstance(int starCount, int value)
            => throw new NotImplementedException();

        public bool IsTopCustomerInstance(int customerId)
            => throw new NotImplementedException();

        public int GetCustomerWithMostOrdersAfterDateInstance(DateTime? startDate)
            => throw new NotImplementedException();

        public DateTime? GetReportingPeriodStartDateInstance(ReportingPeriod periodId)
            => throw new NotImplementedException();

        public string DollarValueInstance(int starCount, string value)
            => throw new NotImplementedException();

        [DbFunction(Schema = "dbo")]
        public static string IdentityString(string s)
            => throw new Exception();

        public static string IdentityStringPropagateNull(string s)
            => throw new Exception();

        [DbFunction(IsNullable = false)]
        public static string IdentityStringNonNullable(string s)
            => throw new Exception();

        public static string IdentityStringNonNullableFluent(string s)
            => throw new Exception();

        public static int? NullableValueReturnType()
            => throw new NotImplementedException();

        public string StringLength(string s)
            => throw new Exception();

        public int AddValues(int a, int b)
            => throw new NotImplementedException();

        public int AddValues(Expression<Func<int>> a, int b)
            => throw new NotImplementedException();

        #region Queryable Functions

        public IQueryable<OrderByYear> GetCustomerOrderCountByYear(int customerId)
            => FromExpression(() => GetCustomerOrderCountByYear(customerId));

        public IQueryable<OrderByYear> GetCustomerOrderCountByYearOnlyFrom2000(int customerId, bool onlyFrom2000)
            => FromExpression(() => GetCustomerOrderCountByYearOnlyFrom2000(customerId, onlyFrom2000));

        public IQueryable<TopSellingProduct> GetTopTwoSellingProducts()
            => FromExpression(() => GetTopTwoSellingProducts());

        public IQueryable<TopSellingProduct> GetTopSellingProductsForCustomer(int customerId)
            => FromExpression(() => GetTopSellingProductsForCustomer(customerId));

        public IQueryable<MultProductOrders> GetOrdersWithMultipleProducts(int customerId)
            => FromExpression(() => GetOrdersWithMultipleProducts(customerId));

        public IQueryable<CustomerData> GetCustomerData(int customerId)
            => FromExpression(() => GetCustomerData(customerId));

        #endregion

        #endregion

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
            modelBuilder.HasDbFunction(isDateMethodInfo).HasName("IsDate").IsBuiltIn();

            modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(AddValues), [typeof(int), typeof(int)]));

            modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(IdentityStringPropagateNull), [typeof(string)]))
                .HasParameter("s").PropagatesNullability();

            modelBuilder.HasDbFunction(
                    typeof(UDFSqlContext).GetMethod(nameof(IdentityStringNonNullableFluent), [typeof(string)]))
                .IsNullable(false);

            var abc = new[] { "A", "B", "C" };
            modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(IsABC), [typeof(string)]))
                .HasTranslation(
                    args => new InExpression(
                        args.First(),
                        new[]
                        {
                            new SqlConstantExpression(abc[0], typeMapping: null),
                            new SqlConstantExpression(abc[1], typeMapping: null),
                            new SqlConstantExpression(abc[2], typeMapping: null)
                        }, // args.First().TypeMapping)
                        typeMapping: null));

            var trueFalse = new[] { true, false };
            modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(IsOrIsNotABC), [typeof(string)]))
                .HasTranslation(
                    args => new InExpression(
                        new InExpression(
                            args.First(),
                            new[]
                            {
                                new SqlConstantExpression(abc[0], args.First().TypeMapping),
                                new SqlConstantExpression(abc[1], args.First().TypeMapping),
                                new SqlConstantExpression(abc[2], args.First().TypeMapping)
                            },
                            typeMapping: null),
                        new[]
                        {
                            new SqlConstantExpression(trueFalse[0], typeMapping: null),
                            new SqlConstantExpression(trueFalse[1], typeMapping: null)
                        },
                        typeMapping: null));

            modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(NullableValueReturnType), []))
                .HasTranslation(
                    _ => new SqlFunctionExpression(
                        "foo",
                        nullable: true,
                        typeof(int?),
                        typeMapping: null));

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
            modelBuilder.HasDbFunction(isDateMethodInfo2).HasName("IsDate").IsBuiltIn();

            modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(DollarValueInstance))).HasName("DollarValue");

            var methodInfo2 = typeof(UDFSqlContext).GetMethod(nameof(MyCustomLengthInstance));

            modelBuilder.HasDbFunction(methodInfo2).HasName("len").IsBuiltIn();

            modelBuilder.Entity<MultProductOrders>().ToTable("MultProductOrders").HasKey(mpo => mpo.OrderId);

            modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(StringLength), [typeof(string)]))
                .HasParameter("s").PropagatesNullability();

            //Table
            modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetCustomerOrderCountByYear), [typeof(int)]));
            modelBuilder.HasDbFunction(
                typeof(UDFSqlContext).GetMethod(nameof(GetCustomerOrderCountByYearOnlyFrom2000), [typeof(int), typeof(bool)]));
            modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetTopTwoSellingProducts)));
            modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetTopSellingProductsForCustomer)));
            modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetOrdersWithMultipleProducts)));
            modelBuilder.HasDbFunction(typeof(UDFSqlContext).GetMethod(nameof(GetCustomerData)));

            modelBuilder.Entity<OrderByYear>().HasNoKey();
            modelBuilder.Entity<TopSellingProduct>().HasNoKey().ToFunction("GetTopTwoSellingProducts");
            modelBuilder.Entity<CustomerData>().ToView("Customers");
        }
    }

    public abstract class UdfFixtureBase : SharedStoreFixtureBase<DbContext>, ITestSqlLoggerFactory
    {
        protected override Type ContextType { get; } = typeof(UDFSqlContext);

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override bool ShouldLogCategory(string logCategory)
            => logCategory == DbLoggerCategory.Query.Name;

        protected override async Task SeedAsync(DbContext context)
        {
            await context.Database.EnsureCreatedResilientlyAsync();

            var product1 = new Product { Name = "Product1" };
            var product2 = new Product { Name = "Product2" };
            var product3 = new Product { Name = "Product3" };
            var product4 = new Product { Name = "Product4" };
            var product5 = new Product { Name = "Product5" };

            var order11 = new Order
            {
                Name = "Order11",
                OrderDate = new DateTime(2000, 1, 20),
                Items = [new() { Quantity = 5, Product = product1 }, new() { Quantity = 15, Product = product3 }]
            };

            var order12 = new Order
            {
                Name = "Order12",
                OrderDate = new DateTime(2000, 2, 21),
                Items =
                [
                    new() { Quantity = 1, Product = product1 },
                    new() { Quantity = 6, Product = product2 },
                    new() { Quantity = 200, Product = product3 }
                ]
            };

            var order13 = new Order
            {
                Name = "Order13",
                OrderDate = new DateTime(2001, 3, 20),
                Items = [new() { Quantity = 50, Product = product4 }]
            };

            var order21 = new Order
            {
                Name = "Order21",
                OrderDate = new DateTime(2000, 4, 21),
                Items =
                [
                    new() { Quantity = 1, Product = product1 },
                    new() { Quantity = 34, Product = product4 },
                    new() { Quantity = 100, Product = product5 }
                ]
            };

            var order22 = new Order
            {
                Name = "Order22",
                OrderDate = new DateTime(2000, 5, 20),
                Items = [new() { Quantity = 34, Product = product3 }, new() { Quantity = 100, Product = product4 }]
            };

            var order31 = new Order
            {
                Name = "Order31",
                OrderDate = new DateTime(2001, 6, 21),
                Items = [new() { Quantity = 5, Product = product5 }]
            };

            var address11 = new Address
            {
                Street = "1600 Pennsylvania Avenue",
                City = "Washington",
                State = "DC"
            };
            var address12 = new Address
            {
                Street = "742 Evergreen Terrace",
                City = "SpringField",
                State = ""
            };
            var address21 = new Address
            {
                Street = "Apartment 5A, 129 West 81st Street",
                City = "New York",
                State = "NY"
            };
            var address31 = new Address
            {
                Street = "425 Grove Street, Apt 20",
                City = "New York",
                State = "NY"
            };
            var address32 = new Address
            {
                Street = "342 GravelPit Terrace",
                City = "BedRock",
                State = ""
            };
            var address41 = new Address
            {
                Street = "4222 Clinton Way",
                City = "Los Angles",
                State = "CA"
            };
            var address42 = new Address
            {
                Street = "1060 West Addison Street",
                City = "Chicago",
                State = "IL"
            };
            var address43 = new Address
            {
                Street = "112 ½ Beacon Street",
                City = "Boston",
                State = "MA"
            };

            var customer1 = new Customer
            {
                FirstName = "Customer",
                LastName = "One",
                Orders =
                [
                    order11,
                    order12,
                    order13
                ],
                Addresses = [address11, address12]
            };

            var customer2 = new Customer
            {
                FirstName = "Customer",
                LastName = "Two",
                Orders = [order21, order22],
                Addresses = [address21]
            };

            var customer3 = new Customer
            {
                FirstName = "Customer",
                LastName = "Three",
                Orders = [order31],
                Addresses = [address31, address32]
            };

            var customer4 = new Customer
            {
                FirstName = "Customer",
                LastName = "Four",
                Addresses =
                [
                    address41,
                    address42,
                    address43
                ]
            };

            ((UDFSqlContext)context).Products.AddRange(product1, product2, product3, product4, product5);
            ((UDFSqlContext)context).Addresses.AddRange(
                address11, address12, address21, address31, address32, address41, address42, address43);
            ((UDFSqlContext)context).Customers.AddRange(customer1, customer2, customer3, customer4);
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

        Assert.Equal(4, len);
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
                   select new { c.FirstName, OrderCount = UDFSqlContext.CustomerOrderCountStatic(UDFSqlContext.AddFiveStatic(c.Id - 5)) })
                .Single());
    }

    [ConditionalFact]
    public virtual void Scalar_Function_Constant_Parameter_Static()
    {
        using var context = CreateContext();
        var customerId = 1;

        var custs = context.Customers.Select(c => UDFSqlContext.CustomerOrderCountStatic(customerId)).ToList();

        Assert.Equal(4, custs.Count);
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

        Assert.Equal(4, results.Count);
        Assert.True(results.SequenceEqual(Enumerable.Range(2, 4)));
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
    public virtual void Compare_function_without_null_propagation_to_null()
    {
        using var context = CreateContext();

        var result = context.Customers
            .OrderBy(c => c.Id)
            .Where(c => UDFSqlContext.IdentityString(c.FirstName) != null)
            .ToList();

        Assert.Equal(4, result.Count);
    }

    [ConditionalFact]
    public virtual void Compare_function_with_null_propagation_to_null()
    {
        using var context = CreateContext();

        var result = context.Customers
            .OrderBy(c => c.Id)
            .Where(c => UDFSqlContext.IdentityStringPropagateNull(c.FirstName) != null)
            .ToList();

        Assert.Equal(4, result.Count);
    }

    [ConditionalFact]
    public virtual void Compare_non_nullable_function_to_null_gets_optimized()
    {
        using var context = CreateContext();

        var result = context.Customers
            .OrderBy(c => c.Id)
            .Where(
                c => UDFSqlContext.IdentityStringNonNullable(c.FirstName) != null
                    && UDFSqlContext.IdentityStringNonNullableFluent(c.FirstName) != null)
            .ToList();

        Assert.Equal(4, result.Count);
    }

    [ConditionalFact]
    public virtual void Compare_functions_returning_int_that_take_nullable_param_which_propagates_null()
    {
        using var context = CreateContext();

        var result = context.Customers
            .OrderBy(c => c.Id)
            .Where(c => context.StringLength(c.FirstName) != context.StringLength(c.LastName))
            .ToList();

        Assert.Equal(4, result.Count);
    }

    [ConditionalFact]
    public virtual void Scalar_Function_SqlFragment_Static()
    {
        using var context = CreateContext();

        var len = context.Customers.Count(c => c.LastName == UDFSqlContext.GetSqlFragmentStatic());

        Assert.Equal(1, len);
    }

    [ConditionalFact]
    public virtual void Scalar_Function_with_InExpression_translation()
    {
        using var context = CreateContext();
        var query = context.Customers.Where(c => UDFSqlContext.IsABC(c.FirstName.Substring(0, 1))).ToList();

        Assert.Equal(4, query.Count);
    }

    [ConditionalFact]
    public virtual void Scalar_Function_with_nested_InExpression_translation()
    {
        using var context = CreateContext();
        var query = context.Customers.Where(c => UDFSqlContext.IsOrIsNotABC(c.FirstName.Substring(0, 1))).ToList();

        Assert.Equal(4, query.Count);
    }

#if RELEASE
    [ConditionalFact]
    public virtual void Scalar_Function_with_nullable_value_return_type_throws()
    {
        using var context = CreateContext();

        var exception = Assert.Throws<InvalidOperationException>(
            () => context.Customers.Where(c => c.Id == UDFSqlContext.NullableValueReturnType()).ToList());

        Assert.Equal(
            RelationalStrings.DbFunctionNullableValueReturnType(
                context.Model.FindDbFunction(typeof(UDFSqlContext).GetMethod(nameof(UDFSqlContext.NullableValueReturnType)))!.ModelName,
                "int?"),
            exception.Message);
    }
#endif

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

        Assert.Equal(4, len);
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

        Assert.Equal(4, custs.Count);
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
                        c.LastName, OrderCount = context.StarValueInstance(starCount, context.CustomerOrderCountInstance(customerId))
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

        Assert.Equal(4, results.Count);
        Assert.True(results.SequenceEqual(Enumerable.Range(2, 4)));
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

    #region TableValuedFunction

    [ConditionalFact]
    public virtual void QF_Anonymous_Collection_No_PK_Throws()
    {
        using (var context = CreateContext())
        {
            var query = from c in context.Customers
                        select new
                        {
                            c.Id,
                            products = context.GetTopSellingProductsForCustomer(c.Id).ToList(),
                            orders = context.Orders.Where(o => o.CustomerId == c.Id).ToList()
                        };

            Assert.Equal(
                RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin,
                Assert.Throws<InvalidOperationException>(() => query.ToList()).Message);
        }
    }

    [ConditionalFact]
    public virtual void QF_Anonymous_Collection_No_IQueryable_In_Projection_Throws()
    {
        using (var context = CreateContext())
        {
            var query = (from c in context.Customers
                         select new { c.Id, orders = context.GetCustomerOrderCountByYear(c.Id) });

            //Assert.Contains(
            //    RelationalStrings.DbFunctionCantProjectIQueryable(),
            //    Assert.Throws<InvalidOperationException>(() => query.ToList()).Message);
        }
    }

    [ConditionalFact]
    public virtual void QF_Stand_Alone()
    {
        using (var context = CreateContext())
        {
            var products = (from t in context.GetTopTwoSellingProducts()
                            orderby t.ProductId
                            select t).ToList();

            Assert.Equal(2, products.Count);
            Assert.Equal(3, products[0].ProductId);
            Assert.Equal(249, products[0].AmountSold);
            Assert.Equal(4, products[1].ProductId);
            Assert.Equal(184, products[1].AmountSold);
        }
    }

    [ConditionalFact]
    public virtual void QF_Stand_Alone_Parameter()
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

    [ConditionalFact]
    public virtual void QF_CrossApply_Correlated_Select_QF_Type()
    {
        using (var context = CreateContext())
        {
            var orders = (from c in context.Customers
                          from r in context.GetCustomerOrderCountByYear(c.Id)
                          orderby r.Year
                          select r
                ).ToList();

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

    [ConditionalFact]
    public virtual void QF_CrossApply_Correlated_Select_Anonymous()
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

    [ConditionalFact]
    public virtual void QF_Select_Direct_In_Anonymous()
    {
        using (var context = CreateContext())
        {
            var message = Assert.Throws<InvalidOperationException>(
                () => (from c in context.Customers
                       select new
                       {
                           c.Id, Prods = context.GetTopTwoSellingProducts().ToList(),
                       }).ToList()).Message;

            Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin, message);
        }
    }

    [ConditionalFact(Skip = "issue #26078")]
    public virtual void QF_Select_Direct_In_Anonymous_distinct()
    {
        using (var context = CreateContext())
        {
            var query = (from c in context.Customers
                         select new
                         {
                             c.Id, Prods = context.GetTopTwoSellingProducts().Distinct().ToList(),
                         }).ToList();
        }
    }

    [ConditionalFact]
    public virtual void QF_Select_Correlated_Direct_With_Function_Query_Parameter_Correlated_In_Anonymous()
    {
        using (var context = CreateContext())
        {
            var cust = (from c in context.Customers
                        where c.Id == 1
                        select new { c.Id, Orders = context.GetOrdersWithMultipleProducts(context.AddValues(c.Id, 1)).ToList() })
                .ToList();

            Assert.Single(cust);

            Assert.Equal(1, cust[0].Id);
            Assert.Equal(4, cust[0].Orders[0].OrderId);
            Assert.Equal(5, cust[0].Orders[1].OrderId);
            Assert.Equal(new DateTime(2000, 4, 21), cust[0].Orders[0].OrderDate);
            Assert.Equal(new DateTime(2000, 5, 20), cust[0].Orders[1].OrderDate);
        }
    }

    [ConditionalFact]
    public virtual void QF_Select_Correlated_Subquery_In_Anonymous()
    {
        using (var context = CreateContext())
        {
            var results = (from c in context.Customers
                           select new
                           {
                               c.Id,
                               OrderCountYear = context.GetOrdersWithMultipleProducts(c.Id).Where(o => o.OrderDate.Day == 21)
                                   .ToList()
                           }).ToList();

            Assert.Equal(4, results.Count);
            Assert.Equal(1, results[0].Id);
            Assert.Equal(2, results[1].Id);
            Assert.Equal(3, results[2].Id);
            Assert.Equal(4, results[3].Id);
            Assert.Single(results[0].OrderCountYear);
            Assert.Single(results[1].OrderCountYear);
            Assert.Empty(results[2].OrderCountYear);
            Assert.Empty(results[3].OrderCountYear);
        }
    }

    [ConditionalFact]
    public virtual void QF_Select_Correlated_Subquery_In_Anonymous_Nested_With_QF()
    {
        using (var context = CreateContext())
        {
            var results = (from o in context.Orders
                           join osub in (from c in context.Customers
                                         from a in context.GetOrdersWithMultipleProducts(c.Id)
                                         select a.OrderId
                               ) on o.Id equals osub
                           select new { o.CustomerId, o.OrderDate }).ToList();

            Assert.Equal(4, results.Count);

            Assert.Equal(1, results[0].CustomerId);
            Assert.Equal(new DateTime(2000, 1, 20), results[0].OrderDate);

            Assert.Equal(1, results[1].CustomerId);
            Assert.Equal(new DateTime(2000, 2, 21), results[1].OrderDate);

            Assert.Equal(2, results[2].CustomerId);
            Assert.Equal(new DateTime(2000, 4, 21), results[2].OrderDate);

            Assert.Equal(2, results[3].CustomerId);
            Assert.Equal(new DateTime(2000, 5, 20), results[3].OrderDate);
        }
    }

    [ConditionalFact]
    public virtual void QF_Select_Correlated_Subquery_In_Anonymous_Nested()
    {
        using (var context = CreateContext())
        {
            var message = Assert.Throws<InvalidOperationException>(
                () => (from c in context.Customers
                       select new
                       {
                           c.Id,
                           OrderCountYear = context.GetOrdersWithMultipleProducts(c.Id).Where(o => o.OrderDate.Day == 21).Select(
                               o => new
                               {
                                   OrderCountYearNested = context.GetOrdersWithMultipleProducts(o.CustomerId).ToList(),
                                   Prods = context.GetTopTwoSellingProducts().ToList(),
                               }).ToList()
                       }).ToList()).Message;

            Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin, message);
        }
    }

    [ConditionalFact]
    public virtual void QF_Select_Correlated_Subquery_In_Anonymous_MultipleCollections()
    {
        using (var context = CreateContext())
        {
            var message = Assert.Throws<InvalidOperationException>(
                () => (from c in context.Customers
                       select new
                       {
                           c.Id,
                           Addresses = c.Addresses.Where(a => a.State == "NY").ToList(),
                           Prods = context.GetTopTwoSellingProducts().Where(p => p.AmountSold == 249).Select(p => p.ProductId).ToList()
                       }).ToList()).Message;

            Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin, message);
        }
    }

    [ConditionalFact]
    public virtual void QF_Select_NonCorrelated_Subquery_In_Anonymous()
    {
        using (var context = CreateContext())
        {
            var message = Assert.Throws<InvalidOperationException>(
                () => (from c in context.Customers
                       select new
                       {
                           c.Id, Prods = context.GetTopTwoSellingProducts().Select(p => p.ProductId).ToList(),
                       }).ToList()).Message;

            Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin, message);
        }
    }

    [ConditionalFact]
    public virtual void QF_Select_NonCorrelated_Subquery_In_Anonymous_Parameter()
    {
        using (var context = CreateContext())
        {
            var amount = 27;
            var message = Assert.Throws<InvalidOperationException>(
                () => (from c in context.Customers
                       select new
                       {
                           c.Id,
                           Prods = context.GetTopTwoSellingProducts().Where(p => p.AmountSold == amount).Select(p => p.ProductId)
                               .ToList(),
                       }).ToList()).Message;

            Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin, message);
        }
    }

    [ConditionalFact]
    public virtual void QF_Correlated_Select_In_Anonymous()
    {
        using (var context = CreateContext())
        {
            var cust = (from c in context.Customers
                        orderby c.Id
                        select new
                        {
                            c.Id,
                            c.LastName,
                            Orders = context.GetOrdersWithMultipleProducts(c.Id).ToList()
                        }).ToList();

            Assert.Equal(4, cust.Count);

            Assert.Equal(1, cust[0].Id);
            Assert.Equal(2, cust[0].Orders.Count);
            Assert.Equal(1, cust[0].Orders[0].OrderId);
            Assert.Equal(2, cust[0].Orders[1].OrderId);
            Assert.Equal(new DateTime(2000, 1, 20), cust[0].Orders[0].OrderDate);
            Assert.Equal(new DateTime(2000, 2, 21), cust[0].Orders[1].OrderDate);

            Assert.Equal(2, cust[1].Id);
            Assert.Equal(2, cust[1].Orders.Count);
            Assert.Equal(4, cust[1].Orders[0].OrderId);
            Assert.Equal(5, cust[1].Orders[1].OrderId);
            Assert.Equal(new DateTime(2000, 4, 21), cust[1].Orders[0].OrderDate);
            Assert.Equal(new DateTime(2000, 5, 20), cust[1].Orders[1].OrderDate);

            Assert.Equal(3, cust[2].Id);
            Assert.Empty(cust[2].Orders);

            Assert.Equal(4, cust[3].Id);
            Assert.Empty(cust[3].Orders);
        }
    }

    [ConditionalFact]
    public virtual void QF_CrossApply_Correlated_Select_Result()
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

    [ConditionalFact]
    public virtual void QF_CrossJoin_Not_Correlated()
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

            Assert.Single(orders);

            Assert.Equal(2, orders[0].Count);
            Assert.Equal(2000, orders[0].Year);
        }
    }

    [ConditionalFact]
    public virtual void QF_CrossJoin_Parameter()
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

            Assert.Single(orders);

            Assert.Equal(2, orders[0].Count);
            Assert.Equal(2000, orders[0].Year);
        }
    }

    [ConditionalFact]
    public virtual void QF_Join()
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
            Assert.Equal(3, products[0].Id);
            Assert.Equal("Product3", products[0].Name);
            Assert.Equal(249, products[0].AmountSold);
            Assert.Equal(4, products[1].Id);
            Assert.Equal("Product4", products[1].Name);
            Assert.Equal(184, products[1].AmountSold);
        }
    }

    [ConditionalFact]
    public virtual void QF_LeftJoin_Select_Anonymous()
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
            Assert.Equal("Product5", products[0].Name);
            Assert.Null(products[0].AmountSold);

            Assert.Equal(4, products[1].Id);
            Assert.Equal("Product4", products[1].Name);
            Assert.Equal(184, products[1].AmountSold);

            Assert.Equal(3, products[2].Id);
            Assert.Equal("Product3", products[2].Name);
            Assert.Equal(249, products[2].AmountSold);

            Assert.Equal(2, products[3].Id);
            Assert.Equal("Product2", products[3].Name);
            Assert.Null(products[3].AmountSold);

            Assert.Equal(1, products[4].Id);
            Assert.Equal("Product1", products[4].Name);
            Assert.Null(products[4].AmountSold);
        }
    }

    [ConditionalFact]
    public virtual void QF_LeftJoin_Select_Result()
    {
        using (var context = CreateContext())
        {
            var products = (from p in context.Products
                            join r in context.GetTopTwoSellingProducts() on p.Id equals r.ProductId into joinTable
                            from j in joinTable.DefaultIfEmpty()
                            orderby p.Id descending
                            select j).ToList();

            Assert.Equal(5, products.Count);
            Assert.Null(products[0]);
            Assert.Equal(4, products[1].ProductId);
            Assert.Equal(184, products[1].AmountSold);
            Assert.Equal(3, products[2].ProductId);
            Assert.Equal(249, products[2].AmountSold);
            Assert.Null(products[3]);
            Assert.Null(products[4]);
        }
    }

    [ConditionalFact]
    public virtual void QF_OuterApply_Correlated_Select_QF()
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
            Assert.Null(orders[4]);
            Assert.Equal(2000, orders[0].Year);
            Assert.Equal(2001, orders[1].Year);
            Assert.Equal(2000, orders[2].Year);
            Assert.Equal(2001, orders[3].Year);
            Assert.Null(orders[4]);
            Assert.Equal(1, orders[0].CustomerId);
            Assert.Equal(1, orders[1].CustomerId);
            Assert.Equal(2, orders[2].CustomerId);
            Assert.Equal(3, orders[3].CustomerId);
            Assert.Null(orders[4]);
        }
    }

    [ConditionalFact]
    public virtual void QF_OuterApply_Correlated_Select_Entity()
    {
        using (var context = CreateContext())
        {
            var custs = (from c in context.Customers
                         from r in context.GetCustomerOrderCountByYear(c.Id).DefaultIfEmpty()
                         where r.Year == 2000
                         orderby c.Id, r.Year
                         select c).ToList();

            Assert.Equal(2, custs.Count);

            Assert.Equal(1, custs[0].Id);
            Assert.Equal(2, custs[1].Id);
            Assert.Equal("One", custs[0].LastName);
            Assert.Equal("Two", custs[1].LastName);
        }
    }

    [ConditionalFact]
    public virtual void QF_OuterApply_Correlated_Select_Anonymous()
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

    [ConditionalFact]
    public virtual void QF_Nested()
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

            Assert.Single(orders);

            Assert.Equal(2, orders[0].Count);
            Assert.Equal(2000, orders[0].Year);
        }
    }

    [ConditionalFact]
    public virtual void QF_Correlated_Nested_Func_Call()
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

            Assert.Single(orders);

            Assert.Equal(1, orders[0].Count);
            Assert.Equal(2001, orders[0].Year);
        }
    }

    [ConditionalFact]
    public virtual void QF_Correlated_Func_Call_With_Navigation()
    {
        using (var context = CreateContext())
        {
            var cust = (from c in context.Customers
                        orderby c.Id
                        select new
                        {
                            c.Id,
                            Orders = context.GetOrdersWithMultipleProducts(c.Id).Select(
                                mpo => new
                                {
                                    //how to I setup the PK/FK combo properly for this?  Is it even possible?
                                    //OrderName = mpo.Order.Name,
                                    CustomerName = mpo.Customer.LastName
                                }).ToList()
                        }).ToList();

            Assert.Equal(4, cust.Count);
            Assert.Equal(2, cust[0].Orders.Count);
            Assert.Equal("One", cust[0].Orders[0].CustomerName);
            Assert.Equal(2, cust[1].Orders.Count);
            Assert.Equal("Two", cust[1].Orders[0].CustomerName);
        }
    }

    [ConditionalFact]
    public virtual void DbSet_mapped_to_function()
    {
        using (var context = CreateContext())
        {
            var products = (from t in context.Set<TopSellingProduct>()
                            orderby t.ProductId
                            select t).ToList();

            Assert.Equal(2, products.Count);
            Assert.Equal(3, products[0].ProductId);
            Assert.Equal(249, products[0].AmountSold);
            Assert.Equal(4, products[1].ProductId);
            Assert.Equal(184, products[1].AmountSold);
        }
    }

    [ConditionalFact]
    public virtual void TVF_with_navigation_in_projection_groupby_aggregate()
    {
        using (var context = CreateContext())
        {
            var query = context.Orders
                .Where(c => !context.Set<TopSellingProduct>().Select(x => x.ProductId).Contains(25))
                .Select(x => new { x.Customer.FirstName, x.Customer.LastName })
                .GroupBy(x => new { x.LastName })
                .Select(x => new { x.Key.LastName, SumOfLengths = x.Sum(xx => xx.FirstName.Length) })
                .ToList();

            Assert.Equal(3, query.Count);
            var orderedResult = query.OrderBy(x => x.LastName).ToList();
            Assert.Equal("One", orderedResult[0].LastName);
            Assert.Equal(24, orderedResult[0].SumOfLengths);
            Assert.Equal("Three", orderedResult[1].LastName);
            Assert.Equal(8, orderedResult[1].SumOfLengths);
            Assert.Equal("Two", orderedResult[2].LastName);
            Assert.Equal(16, orderedResult[2].SumOfLengths);
        }
    }

    [ConditionalFact]
    public virtual void TVF_with_argument_being_a_subquery_with_navigation_in_projection_groupby_aggregate()
    {
        using (var context = CreateContext())
        {
            var query = context.Orders
                .Where(
                    c => !context.GetOrdersWithMultipleProducts(context.Customers.OrderBy(x => x.Id).FirstOrDefault().Id)
                        .Select(x => x.CustomerId).Contains(25))
                .Select(x => new { x.Customer.FirstName, x.Customer.LastName })
                .GroupBy(x => new { x.LastName })
                .Select(x => new { x.Key.LastName, SumOfLengths = x.Sum(xx => xx.FirstName.Length) })
                .ToList();

            Assert.Equal(3, query.Count);
            var orderedResult = query.OrderBy(x => x.LastName).ToList();
            Assert.Equal("One", orderedResult[0].LastName);
            Assert.Equal(24, orderedResult[0].SumOfLengths);
            Assert.Equal("Three", orderedResult[1].LastName);
            Assert.Equal(8, orderedResult[1].SumOfLengths);
            Assert.Equal("Two", orderedResult[2].LastName);
            Assert.Equal(16, orderedResult[2].SumOfLengths);
        }
    }

    [ConditionalFact]
    public virtual void TVF_backing_entity_type_mapped_to_view()
    {
        using (var context = CreateContext())
        {
            var customers = (from t in context.Set<CustomerData>()
                             orderby t.FirstName
                             select t).ToList();

            Assert.Equal(4, customers.Count);
        }
    }

    [ConditionalFact]
    public virtual void Udf_with_argument_being_comparison_to_null_parameter()
    {
        using (var context = CreateContext())
        {
            var prm = default(string);
            var query = (from c in context.Customers
                         from r in context.GetCustomerOrderCountByYearOnlyFrom2000(c.Id, c.LastName != prm)
                         orderby r.Year
                         select r
                ).ToList();

            Assert.Equal(4, query.Count);
            Assert.Equal(1, query[0].CustomerId);
            Assert.Equal(2, query[0].Count);
            Assert.Equal(2, query[1].CustomerId);
            Assert.Equal(2, query[1].Count);
            Assert.Equal(3, query[2].CustomerId);
            Assert.Equal(2, query[2].Count);
            Assert.Equal(4, query[3].CustomerId);
            Assert.Equal(2, query[3].Count);
            Assert.True(query.All(x => x.Year == 2000));
        }
    }

    [ConditionalFact]
    public virtual void Udf_with_argument_being_comparison_of_nullable_columns()
    {
        using (var context = CreateContext())
        {
            var expected = (from a in context.Addresses.ToList()
                            from r in context.Orders.ToList()
                                .Where(x => x.CustomerId == 1 && (a.City != a.State || x.OrderDate.Year == 2000))
                                .GroupBy(x => new { x.CustomerId, x.OrderDate.Year })
                                .Select(
                                    x => new OrderByYear
                                    {
                                        CustomerId = x.Key.CustomerId,
                                        Year = x.Key.Year,
                                        Count = x.Count()
                                    })
                            orderby a.Id, r.Year
                            select r
                ).ToList();

            ClearLog();

            var query = (from a in context.Addresses
                         from r in context.GetCustomerOrderCountByYearOnlyFrom2000(1, a.City == a.State)
                         orderby a.Id, r.Year
                         select r
                ).ToList();

            Assert.Equal(expected.Count, query.Count);
            for (var i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i].CustomerId, query[i].CustomerId);
                Assert.Equal(expected[i].Year, query[i].Year);
                Assert.Equal(expected[i].Count, query[i].Count);
            }
        }
    }

    #endregion

    protected virtual void ClearLog()
    {
    }

    private void AssertTranslationFailed(Action testCode)
        => Assert.Contains(
            CoreStrings.TranslationFailed("")[48..],
            Assert.Throws<InvalidOperationException>(testCode).Message);
}
