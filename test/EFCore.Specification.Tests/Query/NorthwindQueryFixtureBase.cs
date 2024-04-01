// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NorthwindQueryFixtureBase<TModelCustomizer> : SharedStoreFixtureBase<NorthwindContext>, IFilteredQueryFixtureBase
    where TModelCustomizer : ITestModelCustomizer, new()
{
    public Func<DbContext> GetContextCreator()
        => CreateContext;

    private readonly Dictionary<(bool, string, string), ISetSource> _expectedDataCache = new();

    public virtual ISetSource GetExpectedData()
        => NorthwindData.Instance;

    public virtual ISetSource GetFilteredExpectedData(DbContext context)
    {
        var applyFilters = typeof(TModelCustomizer) == typeof(NorthwindQueryFiltersCustomizer);
        var tenantPrefix = applyFilters ? ((NorthwindContext)context).TenantPrefix : null;
        var searchTerm = applyFilters ? ((NorthwindContext)context).SearchTerm : null;

        if (_expectedDataCache.TryGetValue((applyFilters, tenantPrefix, searchTerm), out var cachedResult))
        {
            return cachedResult;
        }

        var expectedData = new NorthwindData();
        if (applyFilters)
        {
            var customers = expectedData.Customers.Where(c => c.CompanyName.StartsWith(tenantPrefix)).ToArray();
            var customerQueriesWithQueryFilter = expectedData.CustomerQueriesWithQueryFilter
                .Where(cq => cq.CompanyName.StartsWith(searchTerm)).ToArray();
            var employees = expectedData.Employees.Where(e => e.Address.StartsWith("A")).ToArray();
            var products = expectedData.Products.Where(p => p.Discontinued).ToArray();
            var orders = expectedData.Orders.Where(o => o.Customer.CompanyName.StartsWith(tenantPrefix)).ToArray();
            var orderDetails = expectedData.OrderDetails
                .Where(od => od.Order.Customer.CompanyName.StartsWith(tenantPrefix) && od.Quantity > 50).ToArray();

            foreach (var product in products)
            {
                product.OrderDetails = product.OrderDetails.Where(od => od.Quantity > 50).ToList();
            }

            foreach (var order in orders)
            {
                order.OrderDetails = order.OrderDetails.Where(od => od.Quantity > 50).ToList();
            }

            foreach (var orderDetail in orderDetails)
            {
                orderDetail.Order = orderDetail.Order.Customer.CompanyName.StartsWith(tenantPrefix) ? orderDetail.Order : null;
                orderDetail.Product = orderDetail.Product.Discontinued ? orderDetail.Product : null;
            }

            expectedData = new NorthwindData(
                customers,
                expectedData.CustomerQueries,
                customerQueriesWithQueryFilter,
                employees,
                products,
                expectedData.ProductQueries,
                orders,
                orderDetails);
        }

        _expectedDataCache[(applyFilters, tenantPrefix, searchTerm)] = expectedData;

        return expectedData;
    }

    public IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object, object>>
    {
        { typeof(Customer), e => ((Customer)e)?.CustomerID },
        { typeof(CustomerQuery), e => ((CustomerQuery)e)?.CompanyName },
        { typeof(CustomerQueryWithQueryFilter), e => ((CustomerQueryWithQueryFilter)e)?.CompanyName },
        { typeof(Order), e => ((Order)e)?.OrderID },
        { typeof(OrderQuery), e => ((OrderQuery)e)?.CustomerID },
        { typeof(Employee), e => ((Employee)e)?.EmployeeID },
        { typeof(Product), e => ((Product)e)?.ProductID },
        { typeof(ProductQuery), e => ((ProductQuery)e)?.ProductID },
        { typeof(ProductView), e => ((ProductView)e)?.ProductID },
        { typeof(OrderDetail), e => (((OrderDetail)e)?.OrderID.ToString(), ((OrderDetail)e)?.ProductID.ToString()) }
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object, object>>
    {
        {
            typeof(Customer), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (Customer)e;
                    var aa = (Customer)a;

                    Assert.Equal(ee.CustomerID, aa.CustomerID);
                    Assert.Equal(ee.Address, aa.Address);
                    Assert.Equal(ee.CompanyName, aa.CompanyName);
                    Assert.Equal(ee.ContactName, aa.ContactName);
                    Assert.Equal(ee.ContactTitle, aa.ContactTitle);
                    Assert.Equal(ee.Country, aa.Country);
                    Assert.Equal(ee.Fax, aa.Fax);
                    Assert.Equal(ee.Phone, aa.Phone);
                    Assert.Equal(ee.PostalCode, aa.PostalCode);
                }
            }
        },
        {
            typeof(CustomerQuery), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (CustomerQuery)e;
                    var aa = (CustomerQuery)a;

                    Assert.Equal(ee.CompanyName, aa.CompanyName);
                    Assert.Equal(ee.Address, aa.Address);
                    Assert.Equal(ee.City, aa.City);
                    Assert.Equal(ee.ContactName, aa.ContactName);
                    Assert.Equal(ee.ContactTitle, aa.ContactTitle);
                }
            }
        },
        {
            typeof(CustomerQueryWithQueryFilter), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (CustomerQueryWithQueryFilter)e;
                    var aa = (CustomerQueryWithQueryFilter)a;

                    Assert.Equal(ee.CompanyName, aa.CompanyName);
                    Assert.Equal(ee.SearchTerm, aa.SearchTerm);
                    Assert.Equal(ee.OrderCount, aa.OrderCount);
                }
            }
        },
        {
            typeof(Order), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (Order)e;
                    var aa = (Order)a;

                    Assert.Equal(ee.OrderID, aa.OrderID);
                    Assert.Equal(ee.CustomerID, aa.CustomerID);
                    Assert.Equal(ee.EmployeeID, aa.EmployeeID);
                    Assert.Equal(ee.OrderDate, aa.OrderDate);
                }
            }
        },
        {
            typeof(OrderQuery), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (OrderQuery)e;
                    var aa = (OrderQuery)a;

                    Assert.Equal(ee.CustomerID, aa.CustomerID);
                }
            }
        },
        {
            typeof(Employee), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (Employee)e;
                    var aa = (Employee)a;

                    Assert.Equal(ee.EmployeeID, aa.EmployeeID);
                    Assert.Equal(ee.Title, aa.Title);
                    Assert.Equal(ee.City, aa.City);
                    Assert.Equal(ee.Country, aa.Country);
                    Assert.Equal(ee.FirstName, aa.FirstName);
                }
            }
        },
        {
            typeof(Product), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (Product)e;
                    var aa = (Product)a;

                    Assert.Equal(ee.ProductID, aa.ProductID);
                    Assert.Equal(ee.ProductName, aa.ProductName);
                    Assert.Equal(ee.SupplierID, aa.SupplierID);
                    Assert.Equal(ee.UnitPrice, aa.UnitPrice);
                    Assert.Equal(ee.UnitsInStock, aa.UnitsInStock);
                }
            }
        },
        {
            typeof(ProductQuery), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (ProductQuery)e;
                    var aa = (ProductQuery)a;

                    Assert.Equal(ee.ProductID, aa.ProductID);
                    Assert.Equal(ee.CategoryName, aa.CategoryName);
                    Assert.Equal(ee.ProductName, aa.ProductName);
                }
            }
        },
        {
            typeof(ProductView), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (ProductView)e;
                    var aa = (ProductView)a;

                    Assert.Equal(ee.ProductID, aa.ProductID);
                    Assert.Equal(ee.CategoryName, aa.CategoryName);
                    Assert.Equal(ee.ProductName, aa.ProductName);
                }
            }
        },
        {
            typeof(OrderDetail), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (OrderDetail)e;
                    var aa = (OrderDetail)a;

                    Assert.Equal(ee.OrderID, aa.OrderID);
                    Assert.Equal(ee.ProductID, aa.ProductID);
                    Assert.Equal(ee.Quantity, aa.Quantity);
                    Assert.Equal(ee.UnitPrice, aa.UnitPrice);
                    Assert.Equal(ee.Discount, aa.Discount);
                }
            }
        },
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    protected override string StoreName
        => "Northwind";

    protected override bool UsePooling
        => typeof(TModelCustomizer) == typeof(NoopModelCustomizer);

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        => new TModelCustomizer().ConfigureConventions(configurationBuilder);

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        => new TModelCustomizer().Customize(modelBuilder, context);

    protected override Task SeedAsync(NorthwindContext context)
        => NorthwindData.SeedAsync(context);

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(
            c => c
                .Log(CoreEventId.RowLimitingOperationWithoutOrderByWarning)
                .Log(CoreEventId.FirstWithoutOrderByAndFilterWarning)
                .Log(CoreEventId.DistinctAfterOrderByWithoutRowLimitingOperatorWarning)
                .Log(CoreEventId.PossibleUnintendedCollectionNavigationNullComparisonWarning)
                .Log(CoreEventId.PossibleUnintendedReferenceComparisonWarning));
}
