using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class NorthwindMiscellaneousQueryTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    protected virtual void ClearLog()
    {
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_navigation_equal_to_null_for_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault()!.OrderDetails == null),
            ss => ss.Set<Customer>().Where(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault() == null));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Dependent_to_principal_navigation_equal_to_null_for_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault()!.Customer == null),
            ss => ss.Set<Customer>().Where(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).FirstOrDefault() == null));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Inner_parameter_in_nested_lambdas_gets_preserved(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.Where(o => c == new Customer { CustomerID = o.CustomerID! }).Count() > 0));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_inside_interpolated_string_is_expanded(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Select(o => $"CustomerCity:{o.Customer!.City}"));
}