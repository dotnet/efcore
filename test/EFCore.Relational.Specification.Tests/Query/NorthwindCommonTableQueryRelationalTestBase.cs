// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

// Queries capturing an IQueryable variable, which is translated to a common table expression.
public abstract class NorthwindCommonTableQueryRelationalTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_query_in_Any(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var londonCustomers = ss.Set<Customer>().Where(c => c.City == "London");
                return ss.Set<Order>().Where(o => londonCustomers.Any(c => c.CustomerID == o.CustomerID));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_query_referenced_twice(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var londonCustomers = ss.Set<Customer>().Where(c => c.City == "London");
                return ss.Set<Order>().Where(o => londonCustomers.Select(c => c.CustomerID).Contains(o.CustomerID)
                    && londonCustomers.Count() > 1);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_query_referenced_twice_with_different_columns(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var londonCustomers = ss.Set<Customer>().Where(c => c.City == "London");
                return ss.Set<Order>().Where(o => londonCustomers.Select(c => c.CustomerID).Contains(o.CustomerID)
                    && londonCustomers.Any(c => c.ContactTitle == "Sales Representative"));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_query_with_parameter(bool async)
    {
        var city = "London";

        return AssertQuery(
            async,
            ss =>
            {
                var cityCustomers = ss.Set<Customer>().Where(c => c.City == city);
                return ss.Set<Order>().Where(o => cityCustomers.Any(c => c.CustomerID == o.CustomerID));
            });
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_query_named_like_table(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var customers = ss.Set<Customer>().Where(c => c.City == "London");
                return ss.Set<Order>().Where(o => customers.Any(c => c.CustomerID == o.CustomerID));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_queries_with_names_differing_in_case(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var query = ss.Set<Customer>().Where(c => c.City == "London");
                var Query = ss.Set<Customer>().Where(c => c.ContactTitle == "Owner");
                return ss.Set<Order>().Where(o => query.Any(c => c.CustomerID == o.CustomerID)
                    || Query.Any(c => c.CustomerID == o.CustomerID));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_query_with_Take(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var latestOrders = ss.Set<Order>().OrderByDescending(o => o.OrderID).Take(5);
                return ss.Set<Customer>().Where(c => latestOrders.Any(o => o.CustomerID == c.CustomerID));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_query_referencing_embedded_query(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var londonCustomers = ss.Set<Customer>().Where(c => c.City == "London");
                var lateLondonOrders = ss.Set<Order>()
                    .Where(o => o.OrderID > 11000 && londonCustomers.Any(c => c.CustomerID == o.CustomerID));
                return ss.Set<Customer>()
                    .Where(c => c.Country == "UK" && lateLondonOrders.Any(o => o.CustomerID == c.CustomerID));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_query_compared_to_navigation(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var londonCustomers = ss.Set<Customer>().Where(c => c.City == "London");
                return ss.Set<Order>().Where(o => londonCustomers.Contains(o.Customer));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_query_with_projection(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var customers = ss.Set<Customer>().Select(c => new { c.CustomerID, Upper = c.City!.ToUpper() });
                return ss.Set<Order>().Where(o => customers.Any(x => x.CustomerID == o.CustomerID && x.Upper == "LONDON"));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_query_in_collection_projection(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var lateOrders = ss.Set<Order>().Where(o => o.OrderID > 11000);
                return ss.Set<Customer>().Where(c => c.City == "London").Select(c => new
                {
                    c.CustomerID, Orders = lateOrders.Where(o => o.CustomerID == c.CustomerID).Select(o => o.OrderID).ToList()
                });
            },
            elementSorter: e => e.CustomerID,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.CustomerID, a.CustomerID);
                AssertCollection(e.Orders, a.Orders);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_query_in_SelectMany(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var lateOrders = ss.Set<Order>().Where(o => o.OrderID > 11000);
                return ss.Set<Customer>().Where(c => c.City == "London")
                    .SelectMany(c => lateOrders.Where(o => o.CustomerID == c.CustomerID), (c, o) => new { c.CustomerID, o.OrderID });
            },
            elementSorter: e => e.OrderID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_query_in_Join_over_required_foreign_key(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var lateOrders = ss.Set<Order>().Where(o => o.OrderID > 11070);
                return ss.Set<Product>().Where(p => ss.Set<OrderDetail>()
                    .Join(lateOrders, od => od.OrderID, o => o.OrderID, (od, o) => od.ProductID)
                    .Contains(p.ProductID));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_query_in_both_sides_of_set_operation(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var londonCustomers = ss.Set<Customer>().Where(c => c.City == "London");
                return ss.Set<Order>().Where(o => o.OrderID < 10300 && londonCustomers.Any(c => c.CustomerID == o.CustomerID))
                    .Concat(ss.Set<Order>().Where(o => o.OrderID > 11000 && londonCustomers.Any(c => c.CustomerID == o.CustomerID)));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_DbSet_is_not_a_common_table(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var customers = ss.Set<Customer>();
                return ss.Set<Order>().Where(o => customers.Any(c => c.CustomerID == o.CustomerID && c.City == "London"));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_grouping_query_in_Any(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var ordersByCustomer = ss.Set<Order>().GroupBy(o => o.CustomerID);
                return ss.Set<Customer>().Where(c => ordersByCustomer.Any(g => g.Key == c.CustomerID && g.Count() > 20));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_grouping_query_in_projection(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var ordersByCustomer = ss.Set<Order>().GroupBy(o => o.CustomerID);
                return ss.Set<Customer>().Select(
                    c => new { c.CustomerID, Count = ordersByCustomer.Where(g => g.Key == c.CustomerID).Select(g => g.Count()).FirstOrDefault() });
            },
            elementSorter: e => e.CustomerID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_in_memory_query_is_not_a_common_table(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var ids = new[] { "ALFKI", "ANATR" }.AsQueryable();
                return ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_query_with_entity_in_dto_is_not_a_common_table(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var lateOrders = ss.Set<Order>().Where(o => o.OrderID > 11000).Select(o => new OrderDto { Order = o });
                return ss.Set<Customer>().Where(c => lateOrders.Any(d => d.Order.Customer!.City == c.City));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_query_with_entity_in_anonymous_type_is_not_a_common_table(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var lateOrders = ss.Set<Order>().Where(o => o.OrderID > 11050).Select(o => new { o.OrderID, o.Customer });
                return ss.Set<Customer>().Where(
                    c => lateOrders.Any(x => x.Customer!.CustomerID == c.CustomerID && x.Customer.Orders.Count() > 10));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_query_with_entity_from_subquery(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var lateOrders = ss.Set<Order>().Where(o => o.OrderID > 11000)
                    .Select(o => new { o.OrderID, Detail = o.OrderDetails.OrderBy(d => d.ProductID).FirstOrDefault() });
                return ss.Set<Customer>().Where(c => lateOrders.Any(x => x.Detail!.Product.ProductName == "Chai" && x.OrderID > c.Orders.Count));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_query_with_collection_navigation(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var londonCustomers = ss.Set<Customer>().Where(c => c.City == "London").Select(c => new { c.CustomerID, c.Orders });
                return ss.Set<Customer>().Where(c => londonCustomers.Any(x => x.CustomerID == c.CustomerID && x.Orders.Count() > 5));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_ordered_query_with_ThenBy(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var lateOrders = ss.Set<Order>().Where(o => o.OrderID > 11000).OrderBy(o => o.CustomerID);
                return ss.Set<Customer>().Where(c => c.City == "London")
                    .Select(c => lateOrders.ThenBy(o => o.OrderID).Where(o => o.CustomerID == c.CustomerID).Select(o => o.OrderID).FirstOrDefault());
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Embedded_query_in_split_collection_projection(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var lateOrders = ss.Set<Order>().Where(o => o.OrderID > 11000);
                return ss.Set<Customer>().Where(c => lateOrders.Any(o => o.CustomerID == c.CustomerID)).Select(c => new
                {
                    c.CustomerID, Orders = lateOrders.Where(o => o.CustomerID == c.CustomerID).Select(o => o.OrderID).ToList()
                }).AsSplitQuery();
            },
            elementSorter: e => e.CustomerID,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.CustomerID, a.CustomerID);
                AssertCollection(e.Orders, a.Orders);
            });

    [Fact]
    public virtual async Task Embedded_query_named_like_table_in_raw_sql()
    {
        using var context = Fixture.CreateContext();

        // The raw SQL reads the real Orders table, which a common table named "orders" must not shadow.
        var orders = context.Set<Customer>().Where(c => c.City == "London");
        var result = await context.Set<Customer>()
            .FromSqlRaw(
                NormalizeDelimitersInRawString(
                    "SELECT * FROM [Customers] WHERE [CustomerID] IN (SELECT [CustomerID] FROM [Orders] WHERE [OrderID] < 10300)"))
            .Where(c => orders.Any(o => o.CustomerID == c.CustomerID))
            .Select(c => c.CustomerID)
            .OrderBy(id => id)
            .ToListAsync();

        var expected = context.Set<Order>()
            .Where(o => o.OrderID < 10300 && o.Customer!.City == "London")
            .Select(o => o.CustomerID!)
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        Assert.NotEmpty(expected);
        Assert.Equal(expected, result);
    }

    private string NormalizeDelimitersInRawString(string sql)
        => ((RelationalTestStore)Fixture.TestStore).NormalizeDelimitersInRawString(sql);

    public class OrderDto
    {
        public Order Order { get; set; } = null!;
    }
}
