// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

#pragma warning disable RCS1202 // Avoid NullReferenceException.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NorthwindMiscellaneousQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    protected NorthwindMiscellaneousQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    protected NorthwindContext CreateContext()
        => Fixture.CreateContext();

    protected virtual void ClearLog()
    {
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Multiple_context_instances(bool async)
    {
        using var context1 = CreateContext();
        using var context2 = CreateContext();

        var message = async
            ? (await Assert.ThrowsAsync<InvalidOperationException>(
                () => (from c in context1.Customers
                       from o in context2.Orders
                       select c).FirstAsync())).Message
            : Assert.Throws<InvalidOperationException>(
                () => (from c in context1.Customers
                       from o in context2.Orders
                       select c).First()).Message;

        Assert.Equal(CoreStrings.ErrorInvalidQueryable, message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Multiple_context_instances_2(bool async)
    {
        using var context1 = CreateContext();
        using var context2 = CreateContext();

        var message = async
            ? (await Assert.ThrowsAsync<InvalidOperationException>(
                () => (from c in context1.Customers
                       from o in context2.Set<Order>()
                       select c).FirstAsync())).Message
            : Assert.Throws<InvalidOperationException>(
                () => (from c in context1.Customers
                       from o in context2.Set<Order>()
                       select c).First()).Message;

        Assert.Equal(CoreStrings.ErrorInvalidQueryable, message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Multiple_context_instances_set(bool async)
    {
        using var context1 = CreateContext();
        using var context2 = CreateContext();
        var set = context2.Orders;

        var message = async
            ? (await Assert.ThrowsAsync<InvalidOperationException>(
                () => (from c in context1.Customers
                       from o in set
                       select c).FirstAsync())).Message
            : Assert.Throws<InvalidOperationException>(
                () => (from c in context1.Customers
                       from o in set
                       select c).First()).Message;

        Assert.Equal(CoreStrings.ErrorInvalidQueryable, message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Multiple_context_instances_parameter(bool async)
    {
        using var context1 = CreateContext();
        using var context2 = CreateContext();

        var message = async
            ? (await Assert.ThrowsAsync<InvalidOperationException>(
                () => queryAsync(context2))).Message
            : Assert.Throws<InvalidOperationException>(
                () => query(context2)).Message;

        Assert.Equal(CoreStrings.ErrorInvalidQueryable, message);

        Customer query(NorthwindContext c2)
            => (from c in context1.Customers
                from o in c2.Orders
                select c).First();

        Task<Customer> queryAsync(NorthwindContext c2)
            => (from c in context1.Customers
                from o in c2.Orders
                select c).FirstAsync();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Query_when_evaluatable_queryable_method_call_with_repository(bool async)
    {
        using var context = CreateContext();
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var customerRepository = new Repository<Customer>(context);
        var orderRepository = new Repository<Order>(context);

        var query1 = customerRepository.Find()
            .Where(c => orderRepository.Find().Any(o => o.CustomerID == c.CustomerID));

        var results = async ? await query1.ToListAsync() : query1.ToList();

        Assert.Equal(89, results.Count);

        var query2 = from c in customerRepository.Find()
                     where orderRepository.Find().Any(o => o.CustomerID == c.CustomerID)
                     select c;

        results = async ? await query2.ToListAsync() : query2.ToList();

        Assert.Equal(89, results.Count);

        var orderQuery = orderRepository.Find();

        var query3 = customerRepository.Find()
            .Where(c => orderQuery.Any(o => o.CustomerID == c.CustomerID));

        results = async ? await query3.ToListAsync() : query3.ToList();

        Assert.Equal(89, results.Count);

        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
    }

    protected class Repository<T>(NorthwindContext bloggingContext)
        where T : class
    {
        private readonly NorthwindContext _context = bloggingContext;

        public IQueryable<T> Find()
            => _context.Set<T>();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Lifting_when_subquery_nested_order_by_simple(bool async)
        => AssertQuery(
            async,
            ss => from c1_Orders in ss.Set<Order>()
                  join _c1 in
                      (from c1 in
                           (from c in ss.Set<Customer>()
                            orderby c.CustomerID
                            select c)
                           .Take(2)
                       from c2 in ss.Set<Customer>()
                       select EF.Property<string>(c1, "CustomerID"))
                      .Distinct()
                      on EF.Property<string>(c1_Orders, "CustomerID") equals _c1
                  orderby _c1
                  select c1_Orders);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Lifting_when_subquery_nested_order_by_anonymous(bool async)
        => AssertQuery(
            async,
            ss => from c1_Orders in ss.Set<Order>()
                  join _c1 in
                      (from c1 in
                           (from c in ss.Set<Customer>()
                            orderby c.CustomerID
                            select c)
                           .Take(2)
                       from c2 in ss.Set<Customer>()
                       select new { CustomerID = EF.Property<string>(c1, "CustomerID") })
                      .Distinct()
                      on EF.Property<string>(c1_Orders, "CustomerID") equals _c1.CustomerID
                  orderby _c1.CustomerID
                  select c1_Orders);

    private class Context
    {
        public readonly Dictionary<string, object> Arguments = new();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Local_dictionary(bool async)
    {
        var context = new Context();
        context.Arguments.Add("customerId", "ALFKI");

        return AssertSingle(
            async,
            ss => ss.Set<Customer>(),
            predicate: c => c.CustomerID == (string)context.Arguments["customerId"]);
    }

    private static IQueryable<Customer> QueryableArgQuery(NorthwindContext context, IQueryable<string> ids)
        => context.Customers.Where(c => ids.Contains(c.CustomerID));

    [ConditionalFact]
    public virtual void Query_composition_against_ienumerable_set()
    {
        using var context = CreateContext();
        IEnumerable<Order> orders = context.Orders;

        var results
            = orders
                .Where(x => x.OrderDate < new DateTime(1996, 7, 12) && x.OrderDate > new DateTime(1996, 7, 4))
                .OrderBy(x => x.ShippedDate)
                .GroupBy(x => x.ShipName)
                .ToList();

        Assert.Single(results);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Shaper_command_caching_when_parameter_names_different(bool async)
    {
        using var context = CreateContext();
        var variableName = "test";
        var differentVariableName = "test";

        var query1 = context.Set<Customer>()
            .Where(e => e.CustomerID == "ALFKI")
            .Where(e2 => InMemoryCheck.Check(variableName, e2.CustomerID) || true);

        var query2 = context.Set<Customer>()
            .Where(e => e.CustomerID == "ALFKI")
            .Where(e2 => InMemoryCheck.Check(differentVariableName, e2.CustomerID) || true);

        if (async)
        {
            await query1.CountAsync();
            await query2.CountAsync();
        }
        else
        {
            query1.Count();
            query2.Count();
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Can_convert_manually_build_expression_with_default(bool async)
    {
        var parameter = Expression.Parameter(typeof(Customer));
        var defaultExpression =
            Expression.Lambda<Func<Customer, bool>>(
                Expression.NotEqual(
                    Expression.Property(
                        parameter,
                        "City"),
                    Expression.Default(typeof(string))),
                parameter);

        await AssertCount(async, ss => ss.Set<Customer>().Where(defaultExpression));
        await AssertCount(async, ss => ss.Set<Customer>(), predicate: defaultExpression);
    }

    private static class InMemoryCheck
    {
        public static bool Check(string input1, string input2)
            => false;
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_self(bool async)
    {
        return AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
#pragma warning disable CS1718 // Comparison made to same variable
                  where c == c
#pragma warning restore CS1718 // Comparison made to same variable
                  select c.CustomerID);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_local(bool async)
    {
        var local = new Customer { CustomerID = "ANATR" };

        return AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where c == local
                  select c.CustomerID);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_local_composite_key(bool async)
    {
        var local = new OrderDetail { OrderID = 10248, ProductID = 11 };

        return AssertQuery(
            async,
            ss => from od in ss.Set<OrderDetail>()
                  where od.Equals(local)
                  select od);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_local_double_check(bool async)
    {
        var local = new Customer { CustomerID = "ANATR" };

        return AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where c == local && local == c
                  select c.CustomerID);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_with_entity_equality_local_on_both_sources(bool async)
    {
        var local = new Customer { CustomerID = "ANATR" };

        return AssertQuery(
            async,
            ss =>
                (from c1 in ss.Set<Customer>()
                 where c1 == local
                 select c1).Join(
                    from c2 in ss.Set<Customer>()
                    where c2 == local
                    select c2, o => o, i => i, (o, i) => o).Select(e => e.CustomerID));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_local_inline(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where c == new Customer { CustomerID = "ANATR" }
                  select c.CustomerID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_local_inline_composite_key(bool async)
        => AssertQuery(
            async,
            ss => from od in ss.Set<OrderDetail>()
                  where od.Equals(new OrderDetail { OrderID = 10248, ProductID = 11 })
                  select od);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_null(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where c == null
                  select c.CustomerID,
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_null_composite_key(bool async)
        => AssertQuery(
            async,
            ss => from od in ss.Set<OrderDetail>()
                  where od == null
                  select od,
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_not_null(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where c != null
                  select c.CustomerID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_not_null_composite_key(bool async)
        => AssertQuery(
            async,
            ss => from od in ss.Set<OrderDetail>()
                  where od != null
                  select od);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_through_nested_anonymous_type_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Select(x => new { CustomerInfo = new { x.Customer } })
                .Where(x => x.CustomerInfo.Customer != null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_through_DTO_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Select(o => new CustomerWrapper { Customer = o.Customer })
                .Where(x => x.Customer != null));

    private class CustomerWrapper
    {
        public Customer Customer { get; set; }

        public override bool Equals(object obj)
            => obj is CustomerWrapper other && other.Customer.Equals(Customer);

        public override int GetHashCode()
            => Customer.GetHashCode();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_through_subquery(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where c.Orders.FirstOrDefault() != null
                  select c.CustomerID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_through_subquery_composite_key(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderDetails.FirstOrDefault() == new OrderDetail { OrderID = 10248, ProductID = 11 }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_through_include(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>().Include(c => c.Orders)
                  where c == null
                  select c.CustomerID,
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_orderby(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_orderby_descending_composite_key(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().OrderByDescending(o => o),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_orderby_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.Orders.FirstOrDefault()),
            ss => ss.Set<Customer>().OrderBy(c => c.Orders.FirstOrDefault() == null ? (int?)null : c.Orders.FirstOrDefault().OrderID),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_orderby_descending_subquery_composite_key(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().OrderByDescending(o => o.OrderDetails.FirstOrDefault()),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Queryable_simple(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Queryable_simple_anonymous(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new { c }),
            e => e.c.CustomerID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Queryable_simple_anonymous_projection_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Take(91).Select(c => new { c }).Select(a => a.c.City));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Queryable_simple_anonymous_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new { c }).Take(91).Select(a => a.c));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Queryable_reprojection(bool async)
        => AssertTranslationFailedWithDetails(
            () => AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.IsLondon)
                    .Select(c => new Customer { CustomerID = "Foo", City = c.City })),
            CoreStrings.QueryUnableToTranslateMember(nameof(Customer.IsLondon), nameof(Customer)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Queryable_nested_simple(bool async)
        => AssertQuery(
            async,
            ss => from c1 in (from c2 in (from c3 in ss.Set<Customer>() select c3) select c2) select c1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_simple(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(10),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_simple_parameterized(bool async)
    {
        var take = 10;

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(take),
            assertOrder: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_simple_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Select(c => c.City).Take(10),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_subquery_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(2).Select(c => c.City),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Skip(5),
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID, StringComparer.Ordinal).Skip(5),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_no_orderby(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Skip(5),
            elementAsserter: (_, __) =>
            {
                /* non-deterministic */
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_orderby_const(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => true).Skip(5),
            elementAsserter: (_, __) =>
            {
                /* non-deterministic */
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_Skip(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactName).Take(10).Skip(5),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distinct_Skip(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Distinct().OrderBy(c => c.CustomerID).Skip(5),
            ss => ss.Set<Customer>().Distinct().OrderBy(c => c.CustomerID, StringComparer.Ordinal).Skip(5),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactName).Skip(5).Take(10),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_Customers_Orders_Skip_Take(bool async)
        => AssertQuery(
            async,
            ss =>
                (from c in ss.Set<Customer>()
                 join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                 orderby o.OrderID
                 select new { c.ContactName, o.OrderID }).Skip(10).Take(5),
            e => e.ContactName);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_Customers_Orders_Skip_Take_followed_by_constant_projection(bool async)
        => AssertQuery(
            async,
            ss =>
                (from c in ss.Set<Customer>()
                 join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                 orderby o.OrderID
                 select new { c.ContactName, o.OrderID }).Skip(10).Take(5).Select(e => "Foo"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_Customers_Orders_Projection_With_String_Concat_Skip_Take(bool async)
        => AssertQuery(
            async,
            ss =>
                (from c in ss.Set<Customer>()
                 join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                 orderby o.OrderID
                 select new { Contact = c.ContactName + " " + c.ContactTitle, o.OrderID }).Skip(10).Take(5),
            e => e.Contact);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_Customers_Orders_Orders_Skip_Take_Same_Properties(bool async)
        => AssertQuery(
            async,
            ss =>
                (from o in ss.Set<Order>()
                 join ca in ss.Set<Customer>() on o.CustomerID equals ca.CustomerID
                 join cb in ss.Set<Customer>() on o.CustomerID equals cb.CustomerID
                 orderby o.OrderID
                 select new
                 {
                     o.OrderID,
                     CustomerIDA = ca.CustomerID,
                     CustomerIDB = cb.CustomerID,
                     ContactNameA = ca.ContactName,
                     ContactNameB = cb.ContactName
                 }).Skip(10).Take(5),
            e => e.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Ternary_should_not_evaluate_both_sides(bool async)
    {
        Customer customer = null;
        var hasData = customer is not null;

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(
                c => new
                {
                    c.CustomerID,
                    Data1 = hasData ? customer.CustomerID : "none",
                    Data2 = customer != null ? customer.CustomerID : "none",
                    Data3 = !hasData ? "none" : customer.CustomerID
                }));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Ternary_should_not_evaluate_both_sides_with_parameter(bool async)
    {
        DateTime? param = null;

        return AssertQuery(
            async,
            ss => ss.Set<Order>().Select(
                o => new
                {
                    // ReSharper disable SimplifyConditionalTernaryExpression
                    Data1 = param != null ? o.OrderDate == param.Value : true, Data2 = param == null ? true : o.OrderDate == param.Value
                    // ReSharper restore SimplifyConditionalTernaryExpression
                }));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Null_Coalesce_Short_Circuit(bool async)
    {
        List<int> values = null;
        bool? test = false;

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Distinct().Select(c => new { Customer = c, Test = test ?? values.Contains(1) }));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Null_Coalesce_Short_Circuit_with_server_correlated_leftover(bool async)
    {
        List<Customer> values = null;
        bool? test = false;

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new { Result = test ?? values.Select(c2 => c2.CustomerID).Contains(c.CustomerID) }));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distinct_Skip_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Distinct().OrderBy(c => c.ContactName).Skip(5).Take(10),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_Distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactName).Skip(5).Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_Take_Distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactName).Skip(5).Take(10).Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_Take_Any(bool async)
        => AssertAny(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactName).Skip(5).Take(10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_Take_All(bool async)
        => AssertAll(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Skip(4).Take(7),
            predicate: p => p.CustomerID.StartsWith("B"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_All(bool async)
        => AssertAll(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(4),
            predicate: p => p.CustomerID.StartsWith("A"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_Take_Any_with_predicate(bool async)
        => AssertAny(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Skip(5).Take(7),
            predicate: p => p.CustomerID.StartsWith("C"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_Any_with_predicate(bool async)
        => AssertAny(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(5),
            predicate: p => p.CustomerID.StartsWith("B"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_Skip_Distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactName).Take(10).Skip(5).Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Take_Skip_Distinct_Caching(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactName).Take(10).Skip(5).Distinct());

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactName).Take(15).Skip(10).Distinct());
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_Distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().OrderBy(o => o.OrderID).Take(5).Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distinct_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Distinct().OrderBy(o => o.OrderID).Take(5),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distinct_Take_Count(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Order>().Distinct().Take(5));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_Distinct_Count(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Order>().Take(5).Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_Where_Distinct_Count(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Order>().Where(o => o.CustomerID == "FRANK").Take(5).Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Any_simple(bool async)
        => AssertAny(
            async,
            ss => ss.Set<Customer>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_Take_Count(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Order>().OrderBy(o => o.OrderID).Take(5));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_OrderBy_Count(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Order>().Take(5).OrderBy(o => o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Any_predicate(bool async)
        => AssertAny(
            async,
            ss => ss.Set<Customer>(),
            predicate: c => c.ContactName.StartsWith("A"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Any_nested_negated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => !ss.Set<Order>().Any(o => o.CustomerID.StartsWith("A"))),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Any_nested_negated2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => c.City != "London"
                    && !ss.Set<Order>().Any(o => o.CustomerID.StartsWith("ABC"))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Any_nested_negated3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => !ss.Set<Order>().Any(o => o.CustomerID.StartsWith("ABC"))
                    && c.City != "London"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Any_nested(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ss.Set<Order>().Any(o => o.CustomerID.StartsWith("A"))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Any_nested2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City != "London" && ss.Set<Order>().Any(o => o.CustomerID.StartsWith("A"))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Any_nested3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ss.Set<Order>().Any(o => o.CustomerID.StartsWith("A")) && c.City != "London"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Any_with_multiple_conditions_still_uses_exists(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == "London" && c.Orders.Any(o => o.EmployeeID == 1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task All_top_level(bool async)
        => AssertAll(
            async,
            ss => ss.Set<Customer>(),
            predicate: c => c.ContactName.StartsWith("A"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task All_top_level_column(bool async)
        => AssertAll(
            async,
            ss => ss.Set<Customer>(),
            predicate: c => c.ContactName.StartsWith(c.ContactName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task All_top_level_subquery(bool async)
        => AssertSingleResult(
            async,
            syncQuery: ss => ss.Set<Customer>().All(
                c1 => ss.Set<Customer>().Any(c2 => ss.Set<Customer>().Any(c3 => c1.CustomerID == c3.CustomerID))),
            asyncQuery: ss => ss.Set<Customer>().AllAsync(
                c1 => ss.Set<Customer>().Any(c2 => ss.Set<Customer>().Any(c3 => c1.CustomerID == c3.CustomerID)),
                default));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task All_top_level_subquery_ef_property(bool async)
        => AssertSingleResult(
            async,
            syncQuery: ss => ss.Set<Customer>().All(
                c1 => ss.Set<Customer>().Any(
                    c2 => ss.Set<Customer>().Any(c3 => EF.Property<string>(c1, "CustomerID") == c3.CustomerID))),
            asyncQuery: ss => ss.Set<Customer>().AllAsync(
                c1 => ss.Set<Customer>().Any(
                    c2 => ss.Set<Customer>().Any(c3 => EF.Property<string>(c1, "CustomerID") == c3.CustomerID)),
                default));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task All_client(bool async)
        => AssertTranslationFailed(
            () => AssertAll(
                async,
                ss => ss.Set<Customer>(),
                predicate: c => c.IsLondon));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task All_client_and_server_top_level(bool async)
        => AssertTranslationFailed(
            () => AssertAll(
                async,
                ss => ss.Set<Customer>(),
                predicate: c => c.CustomerID != "Foo" && c.IsLondon));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task All_client_or_server_top_level(bool async)
        => AssertTranslationFailed(
            () => AssertAll(
                async,
                ss => ss.Set<Customer>(),
                predicate: c => c.CustomerID != "Foo" || c.IsLondon));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_with_single(bool async)
        => AssertSingle(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_with_single_select_many(bool async)
        => AssertSingle(
            async,
            ss => (from c in ss.Set<Customer>()
                   from o in ss.Set<Order>()
                   orderby c.CustomerID, o.OrderID
                   select new { c, o })
                .Take(1)
                .Cast<object>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Cast_results_to_object(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>().Cast<object>() select c);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task First_client_predicate(bool async)
        => AssertTranslationFailedWithDetails(
            () => AssertFirst(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID),
                predicate: c => c.IsLondon),
            CoreStrings.QueryUnableToTranslateMember(nameof(Customer.IsLondon), nameof(Customer)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_select_many_or(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                from e in ss.Set<Employee>()
                where c.City == "London"
                    || e.City == "London"
                select new { c, e },
            e => (e.c.CustomerID, +e.e.EmployeeID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_select_many_or2(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                from e in ss.Set<Employee>()
                where c.City == "London"
                    || c.City == "Berlin"
                select new { c, e },
            e => (e.c.CustomerID, e.e.EmployeeID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_select_many_or3(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                from e in ss.Set<Employee>()
                where c.City == "London"
                    || c.City == "Berlin"
                    || c.City == "Seattle"
                select new { c, e },
            e => (e.c.CustomerID, e.e.EmployeeID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_select_many_or4(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                from e in ss.Set<Employee>()
                where c.City == "London"
                    || c.City == "Berlin"
                    || c.City == "Seattle"
                    || c.City == "Lisboa"
                select new { c, e },
            e => (e.c.CustomerID, e.e.EmployeeID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_select_many_or_with_parameter(bool async)
    {
        var london = "London";
        var lisboa = "Lisboa";

        return AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                from e in ss.Set<Employee>()
                where c.City == london
                    || c.City == "Berlin"
                    || c.City == "Seattle"
                    || c.City == lisboa
                select new { c, e },
            e => (e.c.CustomerID, e.e.EmployeeID));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_anon(bool async)
        => AssertQuery(
            async,
            ss =>
                from e in ss.Set<Employee>().OrderBy(ee => ee.EmployeeID).Take(3).Select(
                    e => new { e })
                from o in ss.Set<Order>().OrderBy(oo => oo.OrderID).Take(5).Select(
                    o => new { o })
                where e.e.EmployeeID == o.o.EmployeeID
                select new { e, o });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_anon_nested(bool async)
        => AssertQuery(
            async,
            ss =>
                from t in (
                    from e in ss.Set<Employee>().OrderBy(ee => ee.EmployeeID).Take(3).Select(
                        e => new { e }).Where(e => e.e.City == "Seattle")
                    from o in ss.Set<Order>().OrderBy(oo => oo.OrderID).Take(5).Select(
                        o => new { o })
                    select new { e, o })
                from c in ss.Set<Customer>().OrderBy(cc => cc.CustomerID).Take(2).Select(
                    c => new { c })
                select new
                {
                    t.e,
                    t.o,
                    c
                });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_expression(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var firstOrder = ss.Set<Order>().First();
                Expression<Func<Order, bool>> expr = z => z.OrderID == firstOrder.OrderID;
                return ss.Set<Order>().Where(x => ss.Set<Order>().Where(expr).Any());
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_expression_same_parametername(bool async)
        => AssertQuery(
            async,
            ss =>
            {
                var firstOrder = ss.Set<Order>().OrderBy(o => o.OrderID).First();
                Expression<Func<Order, bool>> expr = x => x.OrderID == firstOrder.OrderID;
                return ss.Set<Order>().Where(x => ss.Set<Order>().Where(expr).Where(o => o.CustomerID == x.CustomerID).Any());
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_DTO_distinct_translated_to_server(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .Select(o => new OrderCountDTO())
                .Distinct(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Count, a.Count);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_DTO_constructor_distinct_translated_to_server(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .Select(o => new OrderCountDTO(o.CustomerID))
                .Distinct(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Count, a.Count);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_DTO_constructor_distinct_with_navigation_translated_to_server(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .Select(o => new OrderCountDTO(o.Customer.City))
                .Distinct(),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Count, a.Count);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_DTO_constructor_distinct_with_collection_projection_translated_to_server(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .Select(o => new { A = new OrderCountDTO(o.CustomerID), o.CustomerID })
                .Distinct()
                .Select(e => new { e.A, Orders = ss.Set<Order>().Where(o => o.CustomerID == e.CustomerID).ToList() }),
            elementSorter: e => e.A.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.A.Id, a.A.Id);
                AssertCollection(e.Orders, a.Orders);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task
        Select_DTO_constructor_distinct_with_collection_projection_translated_to_server_with_binding_after_client_eval(bool async)
    {
        using var context = CreateContext();
        var actualQuery = context.Set<Order>()
            .Where(o => o.OrderID < 10300)
            .Select(o => new { A = new OrderCountDTO(o.CustomerID), o.CustomerID })
            .Distinct()
            .Select(e => new { e.A, Orders = context.Set<Order>().Where(o => o.CustomerID == e.CustomerID).ToList() });

        var actual = async
            ? (await actualQuery.ToListAsync()).OrderBy(e => e.A.Id).ToList()
            : actualQuery.ToList().OrderBy(e => e.A.Id).ToList();

        var expected = Fixture.GetExpectedData().Set<Order>()
            .Where(o => o.OrderID < 10300)
            .Select(o => new { A = new OrderCountDTO(o.CustomerID), o.CustomerID })
            .Distinct()
            .Select(e => new { e.A, Orders = Fixture.GetExpectedData().Set<Order>().Where(o => o.CustomerID == e.CustomerID).ToList() })
            .ToList().OrderBy(e => e.A.Id).ToList();

        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].A.Id, actual[i].A.Id);
            Assert.True(expected[i].Orders?.SequenceEqual(actual[i].Orders) ?? true);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_DTO_with_member_init_distinct_translated_to_server(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .Select(o => new OrderCountDTO { Id = o.CustomerID, Count = o.OrderID })
                .Distinct(),
            elementSorter: e => e.Count,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Count, a.Count);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nested_collection_count_using_DTO(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.CustomerID.StartsWith("A"))
                .Select(c => new OrderCountDTO { Id = c.CustomerID, Count = c.Orders.Count }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Count, a.Count);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_DTO_with_member_init_distinct_in_subquery_translated_to_server(bool async)
        => AssertQuery(
            async,
            ss =>
                from o in ss.Set<Order>().Where(o => o.OrderID < 10300)
                    .Select(
                        o => new OrderCountDTO { Id = o.CustomerID, Count = o.OrderID })
                    .Distinct()
                from c in ss.Set<Customer>().Where(c => c.CustomerID == o.Id)
                select c);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_DTO_with_member_init_distinct_in_subquery_translated_to_server_2(bool async)
        => AssertQuery(
            async,
            ss =>
                from o in ss.Set<Order>().Where(o => o.OrderID < 10300)
                    .Select(
                        o => new OrderCountDTO { Id = o.CustomerID, Count = o.OrderID })
                    .Distinct()
                from c in ss.Set<Customer>().Where(c => o.Id == c.CustomerID)
                select c);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_DTO_with_member_init_distinct_in_subquery_used_in_projection_translated_to_server(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A"))
                  from o in ss.Set<Order>()
                      .Where(o => o.OrderID < 10300)
                      .Select(o => new OrderCountDTO { Id = o.CustomerID, Count = o.OrderID })
                      .Distinct()
                  select new { c, o },
            elementSorter: e => (e.c.CustomerID, e.o.Count),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.c.CustomerID, a.c.CustomerID);
                Assert.Equal(e.o.Id, a.o.Id);
                Assert.Equal(e.o.Count, a.o.Count);
            });

    private class OrderCountDTO
    {
        public string Id { get; set; }
        public int Count { get; set; }

        public OrderCountDTO()
        {
        }

        public OrderCountDTO(string id)
        {
            Id = id;
            Count = 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((OrderCountDTO)obj);
        }

        private bool Equals(OrderCountDTO other)
            => string.Equals(Id, other.Id) && Count == other.Count;

        public override int GetHashCode()
            => HashCode.Combine(Id, Count);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_correlated_subquery_filtered_returning_queryable_throws(bool async)
        => AssertInvalidMaterializationType(
            () => AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>()
                    where c.CustomerID.StartsWith("A")
                    orderby c.CustomerID
                    select ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a)),
            typeof(IQueryable<Order>).ShortDisplayName());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_correlated_subquery_ordered_returning_queryable_throws(bool async)
        => AssertInvalidMaterializationType(
            () => AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3)
                    select ss.Set<Order>().OrderBy(o => o.OrderID).ThenBy(o => c.CustomerID).Skip(100).Take(2),
                elementSorter: e => e.Count(),
                elementAsserter: (e, a) => AssertCollection(e, a, ordered: true)),
            typeof(IQueryable<Order>).ShortDisplayName());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_correlated_subquery_ordered_returning_queryable_in_DTO_throws(bool async)
        => AssertInvalidMaterializationType(
            () => AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3)
                    select new QueryableDto
                    {
                        Orders = ss.Set<Order>().OrderBy(o => o.OrderID).ThenBy(o => c.CustomerID).Skip(100).Take(2)
                    }),
            typeof(IQueryable<Order>).ShortDisplayName());

    private class QueryableDto
    {
        public IQueryable<Order> Orders { get; set; }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_correlated_subquery_filtered(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                where c.CustomerID.StartsWith("A")
                orderby c.CustomerID
                select ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).ToList(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_correlated_subquery_ordered(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3)
                select ss.Set<Order>().OrderBy(o => o.OrderID).ThenBy(o => c.CustomerID).Skip(100).Take(2).ToList(),
            elementSorter: e => e.Count(),
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nested_collection_in_anonymous_type_returning_ordered_queryable(bool async)
        => AssertInvalidMaterializationType(
            () => AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>()
                    where c.CustomerID == "ALFKI"
                    select new
                    {
                        CustomerId = c.CustomerID,
                        OrderIds
                            = ss.Set<Order>().Where(
                                    o => o.CustomerID == c.CustomerID
                                        && o.OrderDate.Value.Year == 1997)
                                .Select(o => o.OrderID)
                                .OrderBy(o => o),
                        Customer = c
                    },
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.CustomerId, a.CustomerId);
                    AssertCollection(e.OrderIds, a.OrderIds);
                    AssertEqual(e.Customer, a.Customer);
                }),
            typeof(IOrderedQueryable<int>).ShortDisplayName());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_recursive_trivial_returning_queryable(bool async)
        => AssertInvalidMaterializationType(
            () => AssertQuery(
                async,
                ss => from e1 in ss.Set<Employee>()
                      select (from e2 in ss.Set<Employee>()
                              select (from e3 in ss.Set<Employee>()
                                      orderby e3.EmployeeID
                                      select e3)),
                elementSorter: e => e.Count(),
                elementAsserter: (e, a) => AssertCollection(
                    e,
                    a,
                    elementSorter: ee => ee.Count(),
                    elementAsserter: (ee, aa) => AssertCollection(ee, aa, ordered: true))),
            typeof(IQueryable<IOrderedQueryable<Employee>>).ShortDisplayName());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nested_collection_in_anonymous_type(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                where c.CustomerID == "ALFKI"
                select new
                {
                    CustomerId = c.CustomerID,
                    OrderIds
                        = ss.Set<Order>().Where(
                                o => o.CustomerID == c.CustomerID
                                    && o.OrderDate.Value.Year == 1997)
                            .Select(o => o.OrderID)
                            .OrderBy(o => o)
                            .ToArray(),
                    Customer = c
                },
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.CustomerId, a.CustomerId);
                AssertCollection(e.OrderIds, a.OrderIds);
                AssertEqual(e.Customer, a.Customer);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_recursive_trivial(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>()
                  select (from e2 in ss.Set<Employee>()
                          select (from e3 in ss.Set<Employee>()
                                  orderby e3.EmployeeID
                                  select e3).ToList()).ToList(),
            elementSorter: e => e.Count(),
            elementAsserter: (e, a) => AssertCollection(
                e,
                a,
                elementSorter: ee => ee.Count(),
                elementAsserter: (ee, aa) => AssertCollection(ee, aa, ordered: true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_on_bool(bool async)
        => AssertQuery(
            async,
            ss =>
                from p in ss.Set<Product>()
                where ss.Set<Product>().Select(p2 => p2.ProductName).Contains("Chai")
                select p);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_on_collection(bool async)
        => AssertQuery(
            async,
            ss =>
                ss.Set<Product>().Where(
                    p => ss.Set<OrderDetail>()
                        .Where(o => o.ProductID == p.ProductID)
                        .Select(od => od.Quantity).Contains<short>(5)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_query_composition(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>()
                  where e1.FirstName == ss.Set<Employee>().OrderBy(e => e.EmployeeID).FirstOrDefault().FirstName
                  select e1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_query_composition_is_null(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>().OrderBy(e => e.EmployeeID).Take(3)
                  where ss.Set<Employee>().SingleOrDefault(e2 => e2.EmployeeID == e1.ReportsTo) == null
                  select e1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_query_composition_is_not_null(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>().OrderBy(e => e.EmployeeID).Skip(4).Take(3)
                  where ss.Set<Employee>().SingleOrDefault(e2 => e2.EmployeeID == e1.ReportsTo) != null
                  select e1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_query_composition_entity_equality_one_element_SingleOrDefault(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>()
                  where ss.Set<Employee>().SingleOrDefault(e2 => e2.EmployeeID == e1.ReportsTo) == new Employee()
                  select e1,
            ss => from e1 in ss.Set<Employee>()
                  where ss.Set<Employee>().FirstOrDefault(e2 => e2.EmployeeID == e1.ReportsTo) == new Employee()
                  select e1,
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_query_composition_entity_equality_one_element_Single(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>()
                  where ss.Set<Employee>().Single(e2 => e2.EmployeeID == e1.ReportsTo) == new Employee()
                  select e1,
            ss => from e1 in ss.Set<Employee>()
                  where ss.Set<Employee>().FirstOrDefault(e2 => e2.EmployeeID == e1.ReportsTo) == new Employee()
                  select e1,
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_query_composition_entity_equality_one_element_FirstOrDefault(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>()
                  where ss.Set<Employee>().FirstOrDefault(e2 => e2.EmployeeID == e1.ReportsTo) == new Employee()
                  select e1,
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_query_composition_entity_equality_one_element_First(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>()
                  where ss.Set<Employee>().First(e2 => e2.EmployeeID == e1.ReportsTo) == new Employee()
                  select e1,
            ss => from e1 in ss.Set<Employee>()
                  where ss.Set<Employee>().FirstOrDefault(e2 => e2.EmployeeID == e1.ReportsTo) == new Employee()
                  select e1,
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_query_composition_entity_equality_no_elements_SingleOrDefault(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>()
                  where ss.Set<Employee>().SingleOrDefault(e2 => e2.EmployeeID == 42) == new Employee()
                  select e1,
            ss => from e1 in ss.Set<Employee>()
                  where ss.Set<Employee>().FirstOrDefault(e2 => e2.EmployeeID == 42) == new Employee()
                  select e1,
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_query_composition_entity_equality_no_elements_Single(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>()
                  where ss.Set<Employee>().Single(e2 => e2.EmployeeID == 42) == new Employee()
                  select e1,
            ss => from e1 in ss.Set<Employee>()
                  where ss.Set<Employee>().FirstOrDefault(e2 => e2.EmployeeID == 42) == new Employee()
                  select e1,
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_query_composition_entity_equality_no_elements_FirstOrDefault(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>()
                  where ss.Set<Employee>().FirstOrDefault(e2 => e2.EmployeeID == 42) == new Employee()
                  select e1,
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_query_composition_entity_equality_no_elements_First(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>()
                  where ss.Set<Employee>().First(e2 => e2.EmployeeID == 42) == new Employee()
                  select e1,
            ss => from e1 in ss.Set<Employee>()
                  where ss.Set<Employee>().FirstOrDefault(e2 => e2.EmployeeID == 42) == new Employee()
                  select e1,
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>()
                  where ss.Set<Employee>().OrderBy(e2 => e2.EmployeeID).SingleOrDefault(e2 => e2.EmployeeID != e1.ReportsTo)
                      == new Employee { EmployeeID = 1 }
                  select e1,
            ss => from e1 in ss.Set<Employee>()
                  where ss.Set<Employee>().OrderBy(e2 => e2.EmployeeID).FirstOrDefault(e2 => e2.EmployeeID != e1.ReportsTo).EmployeeID == 1
                  select e1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_query_composition_entity_equality_multiple_elements_Single(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>()
                  where ss.Set<Employee>().Single(e2 => e2.EmployeeID != e1.ReportsTo) == new Employee()
                  select e1,
            ss => from e1 in ss.Set<Employee>()
                  where ss.Set<Employee>().FirstOrDefault(e2 => e2.EmployeeID != e1.ReportsTo) == new Employee()
                  select e1,
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_query_composition_entity_equality_multiple_elements_FirstOrDefault(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>()
                  where ss.Set<Employee>().FirstOrDefault(e2 => e2.EmployeeID != e1.ReportsTo) == new Employee()
                  select e1,
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_query_composition_entity_equality_multiple_elements_First(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>()
                  where ss.Set<Employee>().First(e2 => e2.EmployeeID != e1.ReportsTo) == new Employee()
                  select e1,
            ss => from e1 in ss.Set<Employee>()
                  where ss.Set<Employee>().FirstOrDefault(e2 => e2.EmployeeID != e1.ReportsTo) == new Employee()
                  select e1,
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_query_composition2(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>().Take(3)
                  where e1.FirstName
                      == (from e2 in ss.Set<Employee>().OrderBy(e => e.EmployeeID)
                          select new { Foo = e2 }).First().Foo.FirstName
                  select e1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_query_composition2_FirstOrDefault(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>().Take(3)
                  where e1.FirstName
                      == (from e2 in ss.Set<Employee>().OrderBy(e => e.EmployeeID)
                          select e2).FirstOrDefault().FirstName
                  select e1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_query_composition2_FirstOrDefault_with_anonymous(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>().Take(3)
                  where e1.FirstName
                      == (from e2 in ss.Set<Employee>().OrderBy(e => e.EmployeeID)
                          select new { Foo = e2 }).FirstOrDefault().Foo.FirstName
                  select e1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_query_composition3(bool async)
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => from c1 in ss.Set<Customer>()
                      where c1.City == ss.Set<Customer>().OrderBy(c => c.CustomerID).First(c => c.IsLondon).City
                      select c1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_query_composition4(bool async)
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => from c1 in ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(2)
                      where c1.City
                          == (from c2 in ss.Set<Customer>().OrderBy(c => c.CustomerID)
                              from c3 in ss.Set<Customer>().OrderBy(c => c.IsLondon).ThenBy(c => c.CustomerID)
                              select new { c3 }).First().c3.City
                      select c1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_query_composition5(bool async)
        => AssertTranslationFailedWithDetails(
            () => AssertQuery(
                async,
                ss => from c1 in ss.Set<Customer>()
                      where c1.IsLondon == ss.Set<Customer>().OrderBy(c => c.CustomerID).First().IsLondon
                      select c1),
            CoreStrings.QueryUnableToTranslateMember(nameof(Customer.IsLondon), nameof(Customer)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_query_composition6(bool async)
        => AssertTranslationFailedWithDetails(
            () => AssertQuery(
                async,
                ss => from c1 in ss.Set<Customer>()
                      where c1.IsLondon
                          == ss.Set<Customer>().OrderBy(c => c.CustomerID)
                              .Select(c => new { Foo = c })
                              .First().Foo.IsLondon
                      select c1),
            CoreStrings.QueryUnableToTranslateMember(nameof(Customer.IsLondon), nameof(Customer)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_recursive_trivial(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>()
                  where (from e2 in ss.Set<Employee>()
                         where (from e3 in ss.Set<Employee>()
                                orderby e3.EmployeeID
                                select e3).Any()
                         select e2).Any()
                  orderby e1.EmployeeID
                  select e1,
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_scalar_primitive(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Employee>().Select(e => e.EmployeeID).OrderBy(i => i),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_mixed(bool async)
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => from e1 in ss.Set<Employee>().OrderBy(e => e.EmployeeID).Take(2)
                      from s in new[] { "a", "b" }
                      from c in ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(2)
                      select new
                      {
                          e1,
                          s,
                          c
                      },
                e => (e.e1.EmployeeID, e.c.CustomerID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_simple1(bool async)
        => AssertQuery(
            async,
            ss =>
                from e in ss.Set<Employee>()
                from c in ss.Set<Customer>()
                select new { c, e },
            e => (e.c.CustomerID, e.e.EmployeeID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_simple_subquery(bool async)
        => AssertQuery(
            async,
            ss =>
                from e in ss.Set<Employee>().Take(9)
                from c in ss.Set<Customer>()
                select new { c, e },
            e => (e.c.CustomerID, e.e.EmployeeID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_simple2(bool async)
        => AssertQuery(
            async,
            ss =>
                from e1 in ss.Set<Employee>()
                from c in ss.Set<Customer>()
                from e2 in ss.Set<Employee>()
                select new
                {
                    e1,
                    c,
                    e2.FirstName
                },
            e => (e.e1.EmployeeID, e.c.CustomerID, e.FirstName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_entity_deep(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>()
                  from e2 in ss.Set<Employee>()
                  from e3 in ss.Set<Employee>()
                  from e4 in ss.Set<Employee>()
                  select new
                  {
                      e2,
                      e3,
                      e1,
                      e4
                  },
            e => (e.e2.EmployeeID, e.e3.EmployeeID, e.e1.EmployeeID, e.e4.EmployeeID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_projection1(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>()
                  from e2 in ss.Set<Employee>()
                  select new { e1.City, e2.Country },
            e => (e.City, e.Country));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_projection2(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>()
                  from e2 in ss.Set<Employee>()
                  from e3 in ss.Set<Employee>()
                  select new
                  {
                      e1.City,
                      e2.Country,
                      e3.FirstName
                  },
            e => (e.City, e.Country, e.FirstName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_nested_simple(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                from c1 in
                    (from c2 in (from c3 in ss.Set<Customer>() select c3) select c2)
                orderby c1.CustomerID
                select c1,
            ss => ss.Set<Customer>().SelectMany(
                    c => (from c2 in (from c3 in ss.Set<Customer>() select c3) select c2),
                    (c, c1) => new { c, c1 }).OrderBy(t => t.c1.CustomerID, StringComparer.Ordinal)
                .Select(t => t.c1),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_correlated_simple(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                from e in ss.Set<Employee>()
                where c.City == e.City
                orderby c.CustomerID, e.EmployeeID
                select new { c, e },
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_correlated_subquery_simple(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                from e in ss.Set<Employee>().Where(e => e.City == c.City)
                orderby c.CustomerID, e.EmployeeID
                select new { c, e },
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_correlated_subquery_hard(bool async)
        => AssertQuery(
            async,
            ss =>
                from c1 in
                    (from c2 in ss.Set<Customer>().Take(91) select c2.City).Distinct()
                from e1 in
                    (from e2 in ss.Set<Employee>()
                     where c1 == e2.City
                     select new { e2.City, c1 }).Take(9)
                from e2 in
                    (from e3 in ss.Set<Employee>() where e1.City == e3.City select c1).Take(9)
                select new { c1, e1 },
            e => (e.c1, e.e1.City, e.e1.c1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_cartesian_product_with_ordering(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                from e in ss.Set<Employee>()
                where c.City == e.City
                orderby e.City, c.CustomerID descending
                select new { c, e.City },
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_primitive(bool async)
        => AssertQueryScalar(
            async,
            ss => from e1 in ss.Set<Employee>()
                  from i in ss.Set<Employee>().Select(e2 => e2.EmployeeID)
                  select i);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_primitive_select_subquery(bool async)
        => AssertQueryScalar(
            async,
            ss => from e1 in ss.Set<Employee>()
                  from i in ss.Set<Employee>().Select(e2 => e2.EmployeeID)
                  select ss.Set<Employee>().Any());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_Where_Count(bool async)
        => AssertCount(
            async,
            ss => (from c in ss.Set<Customer>()
                   join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                   where c.CustomerID == "ALFKI"
                   select c));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Join_Any(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => c.CustomerID.StartsWith("A") && c.Orders.Any(o => o.OrderDate == new DateTime(1998, 1, 15))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Join_Exists(bool async)
        // Translate List.Exists. Issue #17762.
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(
                    c => c.CustomerID == "ALFKI" && c.Orders.Exists(o => o.OrderDate == new DateTime(2008, 10, 24)))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Join_Exists_Inequality(bool async)
        // Translate List.Exists. Issue #17762.
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(
                    c => c.CustomerID == "ALFKI" && c.Orders.Exists(o => o.OrderDate != new DateTime(2008, 10, 24)))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Join_Exists_Constant(bool async)
        // Translate List.Exists. Issue #17762.
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" && c.Orders.Exists(o => false))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Join_Not_Exists(bool async)
        // Translate List.Exists. Issue #17762.
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" && !c.Orders.Exists(o => false))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_joins_Where_Order_Any(bool async)
        => AssertAny(
            async,
            ss => ss.Set<Customer>().Join(ss.Set<Order>(), c => c.CustomerID, o => o.CustomerID, (cr, or) => new { cr, or })
                .Join(
                    ss.Set<OrderDetail>(), e => e.or.OrderID, od => od.OrderID, (e, od) => new
                    {
                        e.cr,
                        e.or,
                        od
                    })
                .Where(r => r.cr.City == "London").OrderBy(r => r.cr.CustomerID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_OrderBy_Count(bool async)
        => AssertCount(
            async,
            ss => from c in ss.Set<Customer>()
                  join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                  orderby c.CustomerID
                  select c);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_join_select(bool async)
        => AssertQuery(
            async,
            ss =>
                (from c in ss.Set<Customer>()
                 where c.CustomerID == "ALFKI"
                 join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                 select c));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_orderby_join_select(bool async)
        => AssertQuery(
            async,
            ss =>
                (from c in ss.Set<Customer>()
                 where c.CustomerID != "ALFKI"
                 orderby c.CustomerID
                 join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                 select c));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_join_orderby_join_select(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where c.CustomerID != "ALFKI"
                  join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                  orderby c.CustomerID
                  join od in ss.Set<OrderDetail>() on o.OrderID equals od.OrderID
                  select c);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_select_many(bool async)
        => AssertQuery(
            async,
            ss =>
                (from c in ss.Set<Customer>()
                 where c.CustomerID == "ALFKI"
                 from o in ss.Set<Order>()
                 select c));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_orderby_select_many(bool async)
        => AssertQuery(
            async,
            ss =>
                (from c in ss.Set<Customer>()
                 where c.CustomerID == "ALFKI"
                 orderby c.CustomerID
                 from o in ss.Set<Order>()
                 select c));

    private class Foo
    {
        public string Bar { get; set; }
    }

    protected const uint NonExistentID = uint.MaxValue;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Default_if_empty_top_level(bool async)
        => AssertQuery(
            async,
            ss => from e in ss.Set<Employee>().Where(c => c.EmployeeID == NonExistentID).DefaultIfEmpty()
                  select e);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_with_default_if_empty_on_both_sources(bool async)
        => AssertQuery(
            async,
            ss => (from e in ss.Set<Employee>().Where(c => c.EmployeeID == NonExistentID).DefaultIfEmpty()
                   select e).Join(
                from e in ss.Set<Employee>().Where(c => c.EmployeeID == NonExistentID).DefaultIfEmpty()
                select e, o => o, i => i, (o, i) => o),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Default_if_empty_top_level_followed_by_projecting_constant(bool async)
        => AssertQuery(
            async,
            ss => from e in ss.Set<Employee>().Where(c => c.EmployeeID == NonExistentID).DefaultIfEmpty()
                  select "Foo");

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Default_if_empty_top_level_arg(bool async)
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => from e in ss.Set<Employee>().Where(c => c.EmployeeID == NonExistentID).DefaultIfEmpty(new Employee())
                      select e));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Default_if_empty_top_level_arg_followed_by_projecting_constant(bool async)
        => AssertTranslationFailed(
            () => AssertQueryScalar(
                async,
                ss => from e in ss.Set<Employee>().Where(c => c.EmployeeID == NonExistentID).DefaultIfEmpty(new Employee())
                      select 42));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Default_if_empty_top_level_positive(bool async)
        => AssertQuery(
            async,
            ss => from e in ss.Set<Employee>().Where(c => c.EmployeeID > 0).DefaultIfEmpty()
                  select e);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Default_if_empty_top_level_projection(bool async)
        => AssertQueryScalar(
            async,
            ss => from e in ss.Set<Employee>().Where(e => e.EmployeeID == NonExistentID).Select(e => e.EmployeeID).DefaultIfEmpty()
                  select e);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_customer_orders(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                from o in ss.Set<Order>()
                where c.CustomerID == o.CustomerID
                select new { c.ContactName, o.OrderID },
            e => e.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_Count(bool async)
        => AssertCount(
            async,
            ss => from c in ss.Set<Customer>()
                  from o in ss.Set<Order>()
                  select c.CustomerID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_LongCount(bool async)
        => AssertLongCount(
            async,
            ss => from c in ss.Set<Customer>()
                  from o in ss.Set<Order>()
                  select c.CustomerID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_OrderBy_ThenBy_Any(bool async)
        => AssertAny(
            async,
            ss => from c in ss.Set<Customer>()
                  from o in ss.Set<Order>()
                  orderby c.CustomerID, c.City
                  select c);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID),
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID, StringComparer.Ordinal),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_true(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => true).Select(c => c));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_integer(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => 3).Select(c => c));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_parameter(bool async)
    {
        var param = 5;
        return AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => param).Select(c => c));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_anon(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(
                c => new { c.CustomerID }).OrderBy(a => a.CustomerID),
            ss => ss.Set<Customer>().Select(
                c => new { c.CustomerID }).OrderBy(a => a.CustomerID, StringComparer.Ordinal),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_anon2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(
                c => new { c }).OrderBy(a => a.c.CustomerID),
            ss => ss.Set<Customer>().Select(
                c => new { c }).OrderBy(a => a.c.CustomerID, StringComparer.Ordinal),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_client_mixed(bool async)
        => AssertTranslationFailedWithDetails(
            () => AssertQuery(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.IsLondon).ThenBy(c => c.CompanyName),
                assertOrder: true),
            CoreStrings.QueryUnableToTranslateMember(nameof(Customer.IsLondon), nameof(Customer)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_multiple_queries(bool async)
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      join o in ss.Set<Order>() on new Foo { Bar = c.CustomerID } equals new Foo { Bar = o.CustomerID }
                      orderby c.IsLondon, o.OrderDate
                      select new { c, o }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_shadow(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Employee>().OrderBy(e => EF.Property<string>(e, "Title")).ThenBy(e => e.EmployeeID),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_ThenBy_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == "London")
                .OrderBy(c => c.City)
                .ThenBy(c => c.CustomerID),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_correlated_subquery1(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where c.CustomerID.StartsWith("A")
                  orderby ss.Set<Customer>().Any(c2 => c2.CustomerID == c.CustomerID), c.CustomerID
                  select c,
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_correlated_subquery2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(
                o => o.OrderID <= 10250
                    && ss.Set<Customer>().OrderBy(
                            c => ss.Set<Customer>().Any(
                                c2 => c2.CustomerID == "ALFKI"))
                        .FirstOrDefault().City
                    != "Nowhere"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_Select(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID)
                .Select(c => c.ContactName),
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID, StringComparer.Ordinal)
                .Select(c => c.ContactName),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_multiple(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A"))
                .OrderBy(c => c.CustomerID)
                .OrderBy(c => c.Country)
                .ThenBy(c => c.City)
                .Select(c => c.City),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_ThenBy(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID)
                .ThenBy(c => c.Country)
                .Select(c => c.City),
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID, StringComparer.Ordinal)
                .ThenBy(c => c.Country, StringComparer.Ordinal)
                .Select(c => c.City),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderByDescending(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderByDescending(c => c.CustomerID).Select(c => c.City),
            ss => ss.Set<Customer>().OrderByDescending(c => c.CustomerID, StringComparer.Ordinal).Select(c => c.City),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderByDescending_ThenBy(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderByDescending(c => c.CustomerID)
                .ThenBy(c => c.Country)
                .Select(c => c.City),
            ss => ss.Set<Customer>().OrderByDescending(c => c.CustomerID, StringComparer.Ordinal)
                .ThenBy(c => c.Country, StringComparer.Ordinal)
                .Select(c => c.City),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderByDescending_ThenByDescending(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderByDescending(c => c.CustomerID)
                .ThenByDescending(c => c.Country)
                .Select(c => c.City),
            ss => ss.Set<Customer>().OrderByDescending(c => c.CustomerID, StringComparer.Ordinal)
                .ThenByDescending(c => c.Country, StringComparer.Ordinal)
                .Select(c => c.City),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_ThenBy_Any(bool async)
        => AssertAny(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).ThenBy(c => c.ContactName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_Join(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>().OrderBy(c => c.CustomerID)
                join o in ss.Set<Order>().OrderBy(o => o.OrderID) on c.CustomerID equals o.CustomerID
                select new { c.CustomerID, o.OrderID });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_SelectMany(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>().OrderBy(c => c.CustomerID)
                from o in ss.Set<Order>().OrderBy(o => o.OrderID).Take(3)
                where c.CustomerID == o.CustomerID
                select new { c.ContactName, o.OrderID },
            ss =>
                ss.Set<Customer>().OrderBy(c => c.CustomerID, StringComparer.Ordinal)
                    .SelectMany(
                        _ => ss.Set<Order>().OrderBy(o => o.OrderID).Take(3),
                        (c, o) => new { c, o }).Where(t => t.c.CustomerID == t.o.CustomerID)
                    .Select(
                        t => new { t.c.ContactName, t.o.OrderID }),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Let_any_subquery_anonymous(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                let hasOrders = ss.Set<Order>().Any(o => o.CustomerID == c.CustomerID)
                where c.CustomerID.StartsWith("A")
                orderby c.CustomerID
                select new { c, hasOrders },
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_arithmetic(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Employee>().OrderBy(e => e.EmployeeID - e.EmployeeID).Select(e => e));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_condition_comparison(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>().OrderBy(p => p.UnitsInStock > 0).ThenBy(p => p.ProductID),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_ternary_conditions(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>().OrderBy(p => p.UnitsInStock > 10 ? p.ProductID > 40 : p.ProductID <= 40).ThenBy(p => p.ProductID),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_any(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(p => p.Orders.Any(o => o.OrderID > 11000)).ThenBy(p => p.CustomerID),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_Joined(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                from o in ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID)
                select new { c.ContactName, o.OrderDate },
            e => (e.ContactName, e.OrderDate));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_Joined_DefaultIfEmpty(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                from o in ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                select new { c.ContactName, o },
            e => (e.ContactName, e.o?.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_Joined_Take(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                from o in ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).Take(4)
                select new { c.ContactName, o },
            e => e.o.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_Joined_DefaultIfEmpty2(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                from o in ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                select o);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_Joined_DefaultIfEmpty3(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                from o in ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).Where(o => o.OrderDetails.Any()).DefaultIfEmpty()
                select o);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_many_cross_join_same_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().SelectMany(c => ss.Set<Customer>()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_null_coalesce_operator(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.Region ?? "ZZ").ThenBy(c => c.CustomerID),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_null_coalesce_operator(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Select(
                    c => new
                    {
                        c.CustomerID,
                        c.CompanyName,
                        Region = c.Region ?? "ZZ"
                    })
                .OrderBy(o => o.Region)
                .ThenBy(o => o.CustomerID),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_conditional_operator(bool async)
    {
        return AssertQuery(
            async,
            ss => ss.Set<Customer>()
#pragma warning disable IDE0029 // Use coalesce expression
                .OrderBy(c => c.Region == null ? "ZZ" : c.Region).ThenBy(c => c.CustomerID),
#pragma warning restore IDE0029 // Use coalesce expression
            assertOrder: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_conditional_operator_where_condition_false(bool async)
    {
        var fakeCustomer = new Customer();
        return AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => fakeCustomer.City == "London" ? "ZZ" : c.City)
                .Select(c => c));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_comparison_operator(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.Region == "ASK").Select(c => c));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_null_coalesce_operator(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(
                c => new
                {
                    c.CustomerID,
                    c.CompanyName,
                    Region = c.Region ?? "ZZ"
                }),
            e => e.CustomerID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_coalesce_operator(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => (c.CompanyName ?? c.ContactName) == "The Big Cheese"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_skip_null_coalesce_operator(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.Region ?? "ZZ").Take(10).Skip(5).Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_take_null_coalesce_operator(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Select(
                    c => new
                    {
                        c.CustomerID,
                        c.CompanyName,
                        Region = c.Region ?? "ZZ"
                    })
                .OrderBy(c => c.Region)
                .Take(5),
            e => e.CustomerID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_take_skip_null_coalesce_operator(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Select(
                    c => new
                    {
                        c.CustomerID,
                        c.CompanyName,
                        Region = c.Region ?? "ZZ"
                    })
                .OrderBy(c => c.Region)
                .Take(10)
                .Skip(5),
            e => e.CustomerID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_take_skip_null_coalesce_operator2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Select(
                    c => new
                    {
                        c.CustomerID,
                        c.CompanyName,
                        c.Region
                    })
                .OrderBy(c => c.Region ?? "ZZ")
                .Take(10)
                .Skip(5),
            e => e.CustomerID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_take_skip_null_coalesce_operator3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.Region ?? "ZZ").Take(10).Skip(5));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Property_when_non_shadow(bool async)
        => AssertQueryScalar(
            async,
            ss => from o in ss.Set<Order>()
                  select EF.Property<int>(o, "OrderID"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Property_when_non_shadow(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<Order>()
                  where EF.Property<int>(o, "OrderID") == 10248
                  select o);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Property_when_shadow(bool async)
        => AssertQuery(
            async,
            ss => from e in ss.Set<Employee>()
                  select EF.Property<string>(e, "Title"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Property_when_shadow(bool async)
        => AssertQuery(
            async,
            ss => from e in ss.Set<Employee>()
                  where EF.Property<string>(e, "Title") == "Sales Representative"
                  select e);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Property_when_shadow_unconstrained_generic_method(bool async)
        => AssertQuery(
            async,
            ss => ShadowPropertySelect<Employee, string>(ss.Set<Employee>(), "Title"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Property_when_shadow_unconstrained_generic_method(bool async)
        => AssertQuery(
            async,
            ss => ShadowPropertyWhere(ss.Set<Employee>(), "Title", "Sales Representative"));

    protected IQueryable<TOut> ShadowPropertySelect<TIn, TOut>(IQueryable<TIn> source, object column)
        => source.Select(e => EF.Property<TOut>(e, (string)column));

    protected IQueryable<T> ShadowPropertyWhere<T>(IQueryable<T> source, object column, string value)
        => source.Where(e => EF.Property<string>(e, (string)column) == value);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_Property_shadow_closure(bool async)
    {
        var propertyName = "Title";
        var value = "Sales Representative";

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => EF.Property<string>(e, propertyName) == value));

        propertyName = "FirstName";
        value = "Steven";

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => EF.Property<string>(e, propertyName) == value));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Selected_column_can_coalesce(bool async)
        => AssertQuery(
            async,
            ss => (from c in ss.Set<Customer>()
                   orderby c.Region ?? "ZZ"
                   select c).Select(x => x));

    [ConditionalFact]
    public virtual void Can_cast_CreateQuery_result_to_IQueryable_T_bug_1730()
    {
        using var context = CreateContext();
        IQueryable<Product> products = context.Products;

        // ReSharper disable once RedundantAssignment
        products = (IQueryable<Product>)products.Provider.CreateQuery(products.Expression);
    }

    [ConditionalFact]
    public virtual async Task IQueryable_captured_variable()
    {
        await using var context = CreateContext();

        IQueryable<Order> nestedOrdersQuery = context.Orders;

        _ = await context.Customers.CountAsync(c => nestedOrdersQuery.Count() == 2);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Subquery_Single(bool async)
        => AssertQuery(
            async,
            ss => (from od in ss.Set<OrderDetail>()
                   orderby od.ProductID, od.OrderID
                   select (from o in ss.Set<Order>()
                           where od.OrderID == o.OrderID
                           orderby o.OrderID
                           select o).First()).Take(2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Where_Subquery_Deep_Single(bool async)
        => AssertQuery(
            async,
            ss => (from od in ss.Set<OrderDetail>().Where(od => od.OrderID == 10344)
                   where (
                           from o in ss.Set<Order>()
                           where od.OrderID == o.OrderID
                           select (
                               from c in ss.Set<Customer>()
                               where o.CustomerID == c.CustomerID
                               select c
                           ).Single()
                       ).Single()
                       .City
                       == "Seattle"
                   select od)
                .Take(2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Where_Subquery_Deep_First(bool async)
        => AssertQuery(
            async,
            ss => (from od in ss.Set<OrderDetail>()
                   where (
                           from o in ss.Set<Order>()
                           where od.OrderID == o.OrderID
                           select (
                               from c in ss.Set<Customer>()
                               where o.CustomerID == c.CustomerID
                               select c
                           ).FirstOrDefault()
                       ).FirstOrDefault()
                       .City
                       == "Seattle"
                   select od)
                .Take(2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Where_Subquery_Equality(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<Order>().OrderBy(o => o.OrderID).Take(1)
                  // ReSharper disable once UseMethodAny.0
                  where (from od in ss.Set<OrderDetail>().OrderBy(od => od.OrderID).Take(2)
                         where (from c in ss.Set<Customer>()
                                where c.CustomerID == o.CustomerID
                                orderby c.CustomerID
                                select c).First().Country
                             == (from o2 in ss.Set<Order>()
                                 join c in ss.Set<Customer>() on o2.CustomerID equals c.CustomerID
                                 where o2.OrderID == od.OrderID
                                 orderby o2.OrderID, c.CustomerID
                                 select c).First().Country
                         orderby od.ProductID, od.OrderID
                         select od).Count()
                      > 0
                  orderby o.OrderID
                  select o,
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Throws_on_concurrent_query_list(bool async)
    {
        using var context = CreateContext();
        await context.Database.EnsureCreatedResilientlyAsync();

        using var synchronizationEvent = new ManualResetEventSlim(false);
        using var blockingSemaphore = new SemaphoreSlim(0);
        var blockingTask = Task.Run(
            async () =>
            {
                try
                {
                    await context.Customers.Select(
                        c => Process(c, synchronizationEvent, blockingSemaphore)).ToListAsync();
                }
                finally
                {
                    synchronizationEvent.Set();
                }
            });

        var throwingTask = Task.Run(
            async () =>
            {
                synchronizationEvent.Wait(TimeSpan.FromMinutes(5));
                Assert.Equal(
                    CoreStrings.ConcurrentMethodInvocation,
                    (async
                        ? await Assert.ThrowsAsync<InvalidOperationException>(() => context.Customers.ToListAsync())
                        : Assert.Throws<InvalidOperationException>(() => context.Customers.ToList())).Message);
            });

        await throwingTask;

        blockingSemaphore.Release(1);

        await blockingTask;
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Throws_on_concurrent_query_first(bool async)
    {
        using var context = CreateContext();
        await context.Database.EnsureCreatedResilientlyAsync();

        using var synchronizationEvent = new ManualResetEventSlim(false);
        using var blockingSemaphore = new SemaphoreSlim(0);
        var blockingTask = Task.Run(
            async () =>
            {
                try
                {
                    await context.Customers.Select(
                        c => Process(c, synchronizationEvent, blockingSemaphore)).ToListAsync();
                }
                finally
                {
                    synchronizationEvent.Set();
                }
            });

        var throwingTask = Task.Run(
            async () =>
            {
                synchronizationEvent.Wait(TimeSpan.FromMinutes(5));
                Assert.Equal(
                    CoreStrings.ConcurrentMethodInvocation,
                    (async
                        ? await Assert.ThrowsAsync<InvalidOperationException>(() => context.Customers.FirstAsync())
                        : Assert.Throws<InvalidOperationException>(() => context.Customers.First())).Message);
            });

        await throwingTask;

        blockingSemaphore.Release(1);

        await blockingTask;
    }

    private static Customer Process(Customer c, ManualResetEventSlim e, SemaphoreSlim s)
    {
        e.Set();
        s.Wait(TimeSpan.FromMinutes(5));
        s.Release(1);
        return c;
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTime_parse_is_inlined(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate > DateTime.Parse("1/1/1998 12:00:00 PM")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTime_parse_is_parameterized_when_from_closure(bool async)
    {
        var date = "1/1/1998 12:00:00 PM";

        return AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate > DateTime.Parse(date)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task New_DateTime_is_inlined(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate > new DateTime(1998, 1, 1, 12, 0, 0)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task New_DateTime_is_parameterized_when_from_closure(bool async)
    {
        var year = 1998;
        var month = 1;
        var date = 1;
        var hour = 12;

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate > new DateTime(year, month, date, hour, 0, 0)));

        hour = 11;

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate > new DateTime(year, month, date, hour, 0, 0)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Random_next_is_not_funcletized_1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID < (Random.Shared.Next() - 2147483647)),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Random_next_is_not_funcletized_2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID > Random.Shared.Next(5)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Random_next_is_not_funcletized_3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID > Random.Shared.Next(0, 10)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Random_next_is_not_funcletized_4(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID - 20000 > new Random(15).Next()),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Random_next_is_not_funcletized_5(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID > new Random(15).Next(5)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Random_next_is_not_funcletized_6(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID > new Random(15).Next(0, 10)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Environment_newline_is_funcletized(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.Contains(Environment.NewLine)),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_string_int(bool isAsync)
        => AssertQuery(
            isAsync,
            ss => ss.Set<Order>().Select(o => o.OrderID + o.CustomerID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_int_string(bool isAsync)
        => AssertQuery(
            isAsync,
            ss => ss.Set<Order>().Select(o => o.CustomerID + o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_parameter_string_int(bool isAsync)
    {
        var parameter = "-";
        return AssertQuery(
            isAsync,
            ss => ss.Set<Order>().Select(o => parameter + o.OrderID));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_constant_string_int(bool isAsync)
        => AssertQuery(
            isAsync,
            ss => ss.Set<Order>().Select(o => "-" + o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_concat_with_navigation1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Select(o => o.CustomerID + " " + o.Customer.City));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_concat_with_navigation2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Select(o => o.Customer.City + " " + o.Customer.City));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_bitwise_or(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Select(
                c => new { c.CustomerID, Value = c.CustomerID == "ALFKI" | c.CustomerID == "ANATR" }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_bitwise_or_multiple(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID)
                .Select(
                    c => new { c.CustomerID, Value = c.CustomerID == "ALFKI" | c.CustomerID == "ANATR" | c.CustomerID == "ANTON" }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_bitwise_and(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Select(
                c => new { c.CustomerID, Value = c.CustomerID == "ALFKI" & c.CustomerID == "ANATR" }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_bitwise_and_or(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID)
                .Select(
                    c => new { c.CustomerID, Value = c.CustomerID == "ALFKI" & c.CustomerID == "ANATR" | c.CustomerID == "ANTON" }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bitwise_or_with_logical_or(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" | c.CustomerID == "ANATR" || c.CustomerID == "ANTON"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bitwise_and_with_logical_and(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" & c.CustomerID == "ANATR" && c.CustomerID == "ANTON"),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bitwise_or_with_logical_and(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" | c.CustomerID == "ANATR" && c.Country == "Germany"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bitwise_and_with_logical_or(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI" & c.CustomerID == "ANATR" || c.CustomerID == "ANTON"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bitwise_binary_not(bool async)
    {
        var negatedId = ~10248;

        return AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => ~o.OrderID == negatedId));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bitwise_binary_and(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => (o.OrderID & 10248) == 10248));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_bitwise_binary_or(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => (o.OrderID | 10248) == 10248));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_bitwise_or_with_logical_or(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Select(
                c => new { c.CustomerID, Value = c.CustomerID == "ALFKI" | c.CustomerID == "ANATR" || c.CustomerID == "ANTON" }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_bitwise_and_with_logical_and(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Select(
                c => new { c.CustomerID, Value = c.CustomerID == "ALFKI" & c.CustomerID == "ANATR" && c.CustomerID == "ANTON" }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Handle_materialization_properly_when_more_than_two_query_sources_are_involved(bool async)
        => AssertFirstOrDefault(
            async,
            ss => from c in ss.Set<Customer>().OrderBy(c => c.CustomerID)
                  from o in ss.Set<Order>()
                  from e in ss.Set<Employee>()
                  select new { c });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_extraction_short_circuits_1(bool async)
    {
        DateTime? dateFilter = new DateTime(1996, 7, 15);

        await AssertQuery(
            async,
            ss =>
                ss.Set<Order>().Where(
                    o => (o.OrderID < 10400)
                        && ((dateFilter == null)
                            || (o.OrderDate.HasValue
                                && o.OrderDate.Value.Month == dateFilter.Value.Month
                                && o.OrderDate.Value.Year == dateFilter.Value.Year))));

        dateFilter = null;

        await AssertQuery(
            async,
            ss =>
                ss.Set<Order>().Where(
                    o => (o.OrderID < 10400)
                        && ((dateFilter == null)
                            || (o.OrderDate.HasValue
                                && o.OrderDate.Value.Month == dateFilter.Value.Month
                                && o.OrderDate.Value.Year == dateFilter.Value.Year))));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_extraction_short_circuits_2(bool async)
    {
        DateTime? dateFilter = new DateTime(1996, 7, 15);

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(
                o => (o.OrderID < 10400)
                    && (dateFilter.HasValue)
                    && (o.OrderDate.HasValue
                        && o.OrderDate.Value.Month == dateFilter.Value.Month
                        && o.OrderDate.Value.Year == dateFilter.Value.Year)));

        dateFilter = null;

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(
                o => (o.OrderID < 10400)
                    && (dateFilter.HasValue)
                    && (o.OrderDate.HasValue
                        && o.OrderDate.Value.Month == dateFilter.Value.Month
                        && o.OrderDate.Value.Year == dateFilter.Value.Year)),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_extraction_short_circuits_3(bool async)
    {
        DateTime? dateFilter = new DateTime(1996, 7, 15);

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(
                o => (o.OrderID < 10400)
                    || (dateFilter == null)
                    || (o.OrderDate.HasValue
                        && o.OrderDate.Value.Month == dateFilter.Value.Month
                        && o.OrderDate.Value.Year == dateFilter.Value.Year)));

        dateFilter = null;

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(
                o => (o.OrderID < 10400)
                    || (dateFilter == null)
                    || (o.OrderDate.HasValue
                        && o.OrderDate.Value.Month == dateFilter.Value.Month
                        && o.OrderDate.Value.Year == dateFilter.Value.Year)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_extraction_can_throw_exception_from_user_code(bool async)
    {
        var customer = new Customer();

        return Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => Equals(c.Orders.First(), customer.Orders.First()))));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_extraction_can_throw_exception_from_user_code_2(bool async)
    {
        DateTime? dateFilter = null;

        return Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertQuery(
                async,
                ss => ss.Set<Order>()
                    .Where(
                        o => (o.OrderID < 10400)
                            && o.OrderDate.HasValue
                            && o.OrderDate.Value.Month == dateFilter.Value.Month
                            && o.OrderDate.Value.Year == dateFilter.Value.Year)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_member_pushdown_does_not_change_original_subquery_model(bool async)
        => AssertQuery(
            async,
            ss =>
                ss.Set<Order>().OrderBy(o => o.OrderID)
                    .Take(3)
                    .Select(
                        o => new { OrderId = o.OrderID, ss.Set<Customer>().SingleOrDefault(c => c.CustomerID == o.CustomerID).City })
                    .OrderBy(o => o.City),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_member_pushdown_does_not_change_original_subquery_model2(bool async)
        => AssertQuery(
            async,
            ss =>
                ss.Set<Order>().OrderBy(o => o.OrderID)
                    .Take(3)
                    .Select(
                        o => new
                        {
                            OrderId = o.OrderID,
                            City = EF.Property<string>(
                                ss.Set<Customer>().SingleOrDefault(c => c.CustomerID == o.CustomerID), "City")
                        })
                    .OrderBy(o => o.City),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Query_expression_with_to_string_and_contains(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate != null && o.EmployeeID.Value.ToString().Contains("7"))
                .Select(o => new Order { CustomerID = o.CustomerID }),
            elementSorter: e => e.CustomerID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_expression_other_to_string(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate != null)
                .Select(o => new Order { ShipName = o.OrderDate.Value.ToString() }),
            e => e.ShipName);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_expression_long_to_string(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate != null)
                .Select(o => new Order { ShipName = ((long)o.OrderID).ToString() }),
            e => e.ShipName);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_expression_int_to_string(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate != null)
                .Select(o => new Order { ShipName = o.OrderID.ToString() }),
            e => e.ShipName);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task ToString_with_formatter_is_evaluated_on_the_client(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate != null)
                .Select(o => new Order { ShipName = o.OrderID.ToString("X") }),
            e => e.ShipName);

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate != null)
                .Select(o => new Order { ShipName = o.OrderID.ToString(new CultureInfo("en-US")) }),
            e => e.ShipName);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_expression_date_add_year(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate != null)
                .Select(o => new Order { OrderDate = o.OrderDate.Value.AddYears(1) }),
            e => e.OrderDate);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_expression_datetime_add_month(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderDate != null)
                .Select(o => new Order { OrderDate = o.OrderDate.Value.AddMonths(1) }),
            e => e.OrderDate);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_expression_datetime_add_hour(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate != null)
                .Select(o => new Order { OrderDate = o.OrderDate.Value.AddHours(1) }),
            e => e.OrderDate);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_expression_datetime_add_minute(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate != null)
                .Select(o => new Order { OrderDate = o.OrderDate.Value.AddMinutes(1) }),
            e => e.OrderDate);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_expression_datetime_add_second(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate != null)
                .Select(o => new Order { OrderDate = o.OrderDate.Value.AddSeconds(1) }),
            e => e.OrderDate);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_expression_datetime_add_ticks(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate != null)
                .Select(o => new Order { OrderDate = o.OrderDate.Value.AddTicks(TimeSpan.TicksPerMillisecond) }),
            e => e.OrderDate);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_expression_date_add_milliseconds_above_the_range(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate != null)
                .Select(o => new Order { OrderDate = o.OrderDate.Value.AddMilliseconds(1000000000000) }),
            e => e.OrderDate);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_expression_date_add_milliseconds_below_the_range(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate != null)
                .Select(o => new Order { OrderDate = o.OrderDate.Value.AddMilliseconds(-1000000000000) }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_expression_date_add_milliseconds_large_number_divided(bool async)
    {
        var millisecondsPerDay = 86400000L;
        return AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate != null)
                .Select(
                    o => new Order
                    {
                        OrderDate = o.OrderDate.Value
                            .AddDays(o.OrderDate.Value.Millisecond / millisecondsPerDay)
                            .AddMilliseconds(o.OrderDate.Value.Millisecond % millisecondsPerDay)
                    }),
            e => e.OrderDate);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Add_minutes_on_constant_value(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(c => c.OrderID < 10500)
                .OrderBy(o => o.OrderID)
                .Select(o => new { Test = new DateTime(1900, 1, 1).AddMinutes(o.OrderID % 25) }),
            assertOrder: true,
            elementAsserter: (e, a) => AssertEqual(e.Test, a.Test));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_expression_references_are_updated_correctly_with_subquery(bool async)
    {
        var nextYear = 2017;

        return AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate != null)
                .Select(o => o.OrderDate.Value.Year)
                .Distinct()
                .Where(x => x < nextYear));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DefaultIfEmpty_without_group_join(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == "London").DefaultIfEmpty().Where(d => d != null).Select(d => d.CustomerID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DefaultIfEmpty_in_subquery(bool async)
        => AssertQuery(
            async,
            ss =>
                (from c in ss.Set<Customer>()
                 from o in ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                 where o != null
                 select new { c.CustomerID, o.OrderID }),
            e => (e.CustomerID, e.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DefaultIfEmpty_in_subquery_not_correlated(bool async)
        => AssertQuery(
            async,
            ss =>
                (from c in ss.Set<Customer>()
                 from o in ss.Set<Order>().Where(o => o.OrderID > 15000).DefaultIfEmpty()
                 select new { c.CustomerID, OrderID = o != null ? o.OrderID : (int?)null }),
            e => (e.CustomerID, e.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DefaultIfEmpty_in_subquery_nested(bool async)
        => AssertQuery(
            async,
            ss =>
                (from c in ss.Set<Customer>().Where(c => c.City == "Seattle")
                 from o1 in ss.Set<Order>().Where(o => o.OrderID > 11050).DefaultIfEmpty()
                 from o2 in ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                 where o1 != null && o2 != null
                 orderby o1.OrderID, o2.OrderDate
                 select new
                 {
                     c.CustomerID,
                     o1.OrderID,
                     o2.OrderDate
                 }),
            e => (e.CustomerID, e.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DefaultIfEmpty_in_subquery_nested_filter_order_comparison(bool async)
        => AssertQuery(
            async,
            ss =>
                (from c in ss.Set<Customer>().Where(c => c.City == "Seattle")
                 from o1 in ss.Set<Order>().Where(o => o.OrderID > 11050).DefaultIfEmpty()
                 from o2 in ss.Set<Order>().Where(o => o.OrderID <= c.CustomerID.Length + 10250).DefaultIfEmpty()
                 where o1 != null && o2 != null
                 orderby o1.OrderID, o2.OrderDate
                 select new
                 {
                     c.CustomerID,
                     o1.OrderID,
                     o2.OrderDate
                 }),
            e => (e.CustomerID, e.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_skip_take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactTitle)
                .ThenBy(c => c.ContactName)
                .Skip(5)
                .Take(8),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_skip_skip_take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactTitle)
                .ThenBy(c => c.ContactName)
                .Skip(5)
                .Skip(8)
                .Take(3),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_skip_take_take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactTitle)
                .ThenBy(c => c.ContactName)
                .Skip(5)
                .Take(8)
                .Take(3),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_skip_take_take_take_take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactTitle)
                .ThenBy(c => c.ContactName)
                .Skip(5)
                .Take(15)
                .Take(10)
                .Take(8)
                .Take(5),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_skip_take_skip_take_skip(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactTitle)
                .ThenBy(c => c.ContactName)
                .Skip(5)
                .Take(15)
                .Skip(2)
                .Take(8)
                .Skip(5),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_skip_take_distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactTitle)
                .ThenBy(c => c.ContactName)
                .Skip(5)
                .Take(15)
                .Distinct(),
            assertOrder: false);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_coalesce_take_distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>().OrderBy(p => p.UnitPrice ?? 0)
                .Take(15)
                .Distinct(),
            assertOrder: false);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_coalesce_skip_take_distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>().OrderBy(p => p.UnitPrice ?? 0)
                .Skip(5)
                .Take(15)
                .Distinct(),
            assertOrder: false);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_coalesce_skip_take_distinct_take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>().OrderBy(p => p.UnitPrice ?? 0)
                .Skip(5)
                .Take(15)
                .Distinct()
                .Take(5),
            elementAsserter: (_, __) =>
            {
                /* non-deterministic */
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_skip_take_distinct_orderby_take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactTitle)
                .ThenBy(c => c.ContactName)
                .Skip(5)
                .Take(15)
                .Distinct()
                .OrderBy(c => c.ContactTitle)
                .Take(8),
            assertOrder: false);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task No_orderby_added_for_fully_translated_manually_constructed_LOJ(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Employee>()
                  join e2 in ss.Set<Employee>() on e1.EmployeeID equals e2.ReportsTo into grouping
                  from e2 in grouping.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                  select new { City1 = e1.City, City2 = e2 != null ? e2.City : null },
#pragma warning restore IDE0031 // Use null propagation
            e => (e.City1, e.City2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ(bool async)
        // Translation failed message. Issue #17328.
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss =>
                    from o in ss.Set<Order>()
                    join c in ss.Set<Customer>() on o.CustomerID equals c.CustomerID into grouping
                    from c in ClientDefaultIfEmpty(grouping)
#pragma warning disable IDE0031 // Use null propagation
                    select new { Id1 = o.CustomerID, Id2 = c != null ? c.CustomerID : null },
#pragma warning restore IDE0031 // Use null propagation
                e => (e.Id1, e.Id2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition1(bool async)
        // Translation failed message. Issue #17328.
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss =>
                    from o in ss.Set<Order>()
                    join c in ss.Set<Customer>() on new { o.CustomerID, o.OrderID } equals new { c.CustomerID, OrderID = 10000 } into
                        grouping
                    from c in ClientDefaultIfEmpty(grouping)
#pragma warning disable IDE0031 // Use null propagation
                    select new { Id1 = o.CustomerID, Id2 = c != null ? c.CustomerID : null },
#pragma warning restore IDE0031 // Use null propagation
                e => (e.Id1, e.Id2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2(bool async)
        // Translation failed message. Issue #17328.
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss =>
                    from o in ss.Set<Order>()
                    join c in ss.Set<Customer>() on new { o.OrderID, o.CustomerID } equals new { OrderID = 10000, c.CustomerID } into
                        grouping
                    from c in ClientDefaultIfEmpty(grouping)
#pragma warning disable IDE0031 // Use null propagation
                    select new { Id1 = o.CustomerID, Id2 = c != null ? c.CustomerID : null },
#pragma warning restore IDE0031 // Use null propagation
                e => (e.Id1, e.Id2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ(bool async)
        // Translation failed message. Issue #17328.
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => from e1 in ss.Set<Employee>()
                      join e2 in ss.Set<Employee>() on e1.EmployeeID equals e2.ReportsTo into grouping
                      from e2 in ClientDefaultIfEmpty(grouping)
#pragma warning disable IDE0031 // Use null propagation
                      select new { City1 = e1.City, City2 = e2 != null ? e2.City : null },
#pragma warning restore IDE0031 // Use null propagation
                e => (e.City1, e.City2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Contains_with_DateTime_Date(bool async)
    {
        var dates = new[] { new DateTime(1996, 07, 04), new DateTime(1996, 07, 16) };

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(e => dates.Contains(e.OrderDate.Value.Date)));

        dates = [new DateTime(1996, 07, 04)];

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(e => dates.Contains(e.OrderDate.Value.Date)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_subquery_involving_join_binds_to_correct_table(bool async)
        => AssertQuery(
            async,
            ss =>
                ss.Set<Order>().Where(
                    o => o.OrderID > 11000
                        && ss.Set<OrderDetail>().Where(od => od.Product.ProductName == "Chai")
                            .Select(od => od.OrderID)
                            .Contains(o.OrderID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Anonymous_member_distinct_where(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new { c.CustomerID }).Distinct().Where(n => n.CustomerID == "ALFKI"),
            e => e.CustomerID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Anonymous_member_distinct_orderby(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new { c.CustomerID }).Distinct().OrderBy(n => n.CustomerID),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Anonymous_member_distinct_result(bool async)
        => AssertSingleResult(
            async,
            syncQuery: ss => ss.Set<Customer>().Select(
                c => new { c.CustomerID }).Distinct().Count(n => n.CustomerID.StartsWith("A")),
            asyncQuery: ss => ss.Set<Customer>().Select(
                c => new { c.CustomerID }).Distinct().CountAsync(n => n.CustomerID.StartsWith("A"), default));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Anonymous_complex_distinct_where(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new { A = c.CustomerID + c.City }).Distinct().Where(n => n.A == "ALFKIBerlin"),
            e => e.A);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Anonymous_complex_distinct_orderby(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new { A = c.CustomerID + c.City }).Distinct().OrderBy(n => n.A),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Anonymous_complex_distinct_result(bool async)
        => AssertSingleResult(
            async,
            syncQuery: ss => ss.Set<Customer>().Select(c => new { A = c.CustomerID + c.City }).Distinct()
                .Count(n => n.A.StartsWith("A")),
            asyncQuery: ss
                => ss.Set<Customer>().Select(c => new { A = c.CustomerID + c.City }).Distinct()
                    .CountAsync(n => n.A.StartsWith("A"), default));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Anonymous_complex_orderby(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new { A = c.CustomerID + c.City }).OrderBy(n => n.A),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Anonymous_subquery_orderby(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.Orders.Count > 1)
                .Select(c => new { A = c.Orders.OrderByDescending(o => o.OrderID).FirstOrDefault().OrderDate })
                .OrderBy(n => n.A),
            assertOrder: true);

    protected class DTO<T>
    {
        public T Property { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((DTO<T>)obj);
        }

        private bool Equals(DTO<T> other)
            => EqualityComparer<T>.Default.Equals(Property, other.Property);

        public override int GetHashCode()
            => Property.GetHashCode();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DTO_member_distinct_where(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new DTO<string> { Property = c.CustomerID }).Distinct()
                .Where(n => n.Property == "ALFKI"),
            elementSorter: e => e.Property,
            elementAsserter: (e, a) => Assert.Equal(e.Property, a.Property));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DTO_member_distinct_orderby(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new DTO<string> { Property = c.CustomerID }).Distinct().OrderBy(n => n.Property),
            assertOrder: true,
            elementAsserter: (e, a) => Assert.Equal(e.Property, a.Property));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DTO_member_distinct_result(bool async)
        => AssertSingleResult(
            async,
            syncQuery: ss => ss.Set<Customer>().Select(
                c => new DTO<string> { Property = c.CustomerID }).Distinct().Count(n => n.Property.StartsWith("A")),
            asyncQuery: ss => ss.Set<Customer>().Select(
                c => new DTO<string> { Property = c.CustomerID }).Distinct().CountAsync(n => n.Property.StartsWith("A"), default));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DTO_complex_distinct_where(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new DTO<string> { Property = c.CustomerID + c.City }).Distinct()
                .Where(n => n.Property == "ALFKIBerlin"),
            elementSorter: e => e.Property,
            elementAsserter: (e, a) => Assert.Equal(e.Property, a.Property));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DTO_complex_distinct_orderby(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new DTO<string> { Property = c.CustomerID + c.City }).Distinct()
                .OrderBy(n => n.Property),
            assertOrder: true,
            elementAsserter: (e, a) => Assert.Equal(e.Property, a.Property));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DTO_complex_distinct_result(bool async)
        => AssertSingleResult(
            async,
            syncQuery: ss => ss.Set<Customer>().Select(
                c => new DTO<string> { Property = c.CustomerID + c.City }).Distinct().Count(n => n.Property.StartsWith("A")),
            asyncQuery: ss => ss.Set<Customer>().Select(
                    c => new DTO<string> { Property = c.CustomerID + c.City }).Distinct()
                .CountAsync(n => n.Property.StartsWith("A"), default));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DTO_complex_orderby(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new DTO<string> { Property = c.CustomerID + c.City }).OrderBy(n => n.Property),
            assertOrder: true,
            elementAsserter: (e, a) => Assert.Equal(e.Property, a.Property));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DTO_subquery_orderby(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.Orders.Count > 1)
                .Select(c => new DTO<DateTime?> { Property = c.Orders.OrderByDescending(o => o.OrderID).FirstOrDefault().OrderDate })
                .OrderBy(n => n.Property),
            assertOrder: true,
            elementAsserter: (e, a) => Assert.Equal(e.Property, a.Property));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_with_orderby_skip_preserves_ordering(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Include(c => c.Orders)
                .Where(c => c.CustomerID != "VAFFE" && c.CustomerID != "DRACD")
                .OrderBy(c => c.City)
                .ThenBy(c => c.CustomerID)
                .Skip(40)
                .Take(5),
            assertOrder: true);

    private static IEnumerable<TElement> ClientDefaultIfEmpty<TElement>(IEnumerable<TElement> source)
        => source?.Count() == 0 ? new[] { default(TElement) } : source;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_query_with_repeated_query_model_compiles_correctly(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(outer => outer.CustomerID == "ALFKI")
                .Where(
                    outer =>
                        (from c in ss.Set<Customer>()
                         let customers = ss.Set<Customer>().Select(cc => cc.CustomerID).ToList()
                         where customers.Any()
                         select customers).Any()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_query_with_repeated_nested_query_model_compiles_correctly(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(outer => outer.CustomerID == "ALFKI")
                .Where(
                    outer =>
                        (from c in ss.Set<Customer>()
                         let customers = ss.Set<Customer>().Where(
                                 cc => ss.Set<Customer>().OrderBy(inner => inner.CustomerID).Take(10).Distinct().Any())
                             .Select(cc => cc.CustomerID).ToList()
                         where customers.Any()
                         select customers).Any()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Int16_parameter_can_be_used_for_int_column(bool async)
    {
        const ushort parameter = 10300;

        return AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID == parameter));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_is_null_translated_correctly(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                let lastOrder = c.Orders.OrderByDescending(o => o.OrderID)
                    .Select(o => o.CustomerID)
                    .FirstOrDefault()
                where lastOrder == null
                select c);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_is_not_null_translated_correctly(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                let lastOrder = c.Orders.OrderByDescending(o => o.OrderID)
                    .Select(o => o.CustomerID)
                    .FirstOrDefault()
                where lastOrder != null
                select c);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_take_average(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<Order>().OrderBy(o => o.OrderID).Select(o => o.OrderID).Take(10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_take_count(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Customer>().Take(7));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_orderBy_take_count(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.Country).Take(7));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_take_long_count(bool async)
        => AssertLongCount(
            async,
            ss => ss.Set<Customer>().Take(7));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_orderBy_take_long_count(bool async)
        => AssertLongCount(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.Country).Take(7));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_take_max(bool async)
        => AssertMax(
            async,
            ss => ss.Set<Order>().OrderBy(o => o.OrderID).Select(o => o.OrderID).Take(10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_take_min(bool async)
        => AssertMin(
            async,
            ss => ss.Set<Order>().OrderBy(o => o.OrderID).Select(o => o.OrderID).Take(10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_take_sum(bool async)
        => AssertSum(
            async,
            ss => ss.Set<Order>().OrderBy(o => o.OrderID).Select(o => o.OrderID).Take(10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_skip_average(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<Order>().OrderBy(o => o.OrderID).Select(o => o.OrderID).Skip(10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_skip_count(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Customer>().Skip(7));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_orderBy_skip_count(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.Country).Skip(7));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_skip_long_count(bool async)
        => AssertLongCount(
            async,
            ss => ss.Set<Customer>().Skip(7));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_orderBy_skip_long_count(bool async)
        => AssertLongCount(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.Country).Skip(7));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_skip_max(bool async)
        => AssertMax(
            async,
            ss => ss.Set<Order>().OrderBy(o => o.OrderID).Select(o => o.OrderID).Skip(10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_skip_min(bool async)
        => AssertMin(
            async,
            ss => ss.Set<Order>().OrderBy(o => o.OrderID).Select(o => o.OrderID).Skip(10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_skip_sum(bool async)
        => AssertSum(
            async,
            ss => ss.Set<Order>().OrderBy(o => o.OrderID).Select(o => o.OrderID).Skip(10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_distinct_average(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderID).Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_distinct_count(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Customer>().Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_distinct_long_count(bool async)
        => AssertLongCount(
            async,
            ss => ss.Set<Customer>().Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_distinct_max(bool async)
        => AssertMax(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderID).Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_distinct_min(bool async)
        => AssertMin(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderID).Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_distinct_sum(bool async)
        => AssertSum(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderID).Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Comparing_to_fixed_string_parameter(bool async)
        => AssertQuery(
            async,
            ss => FindLike(ss.Set<Customer>(), "A"));

    private static IQueryable<string> FindLike(IQueryable<Customer> cs, string prefix)
        => from c in cs
           where c.CustomerID.StartsWith(prefix)
           select c.CustomerID;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Comparing_entities_using_Equals(bool async)
        => AssertQuery(
            async,
            ss => from c1 in ss.Set<Customer>()
                  from c2 in ss.Set<Customer>()
                  where c1.CustomerID.StartsWith("ALFKI")
                  where c1.Equals(c2)
                  orderby c1.CustomerID
                  select new { Id1 = c1.CustomerID, Id2 = c2.CustomerID });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Comparing_different_entity_types_using_Equals(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  from o in ss.Set<Order>()
                  where c.CustomerID == "ALFKI" && o.CustomerID == "ALFKI"
                  where c.Equals(o)
                  select c.CustomerID,
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Comparing_entity_to_null_using_Equals(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where c.CustomerID.StartsWith("A")
                  where !Equals(null, c)
                  orderby c.CustomerID
                  select c.CustomerID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Comparing_navigations_using_Equals(bool async)
        => AssertQuery(
            async,
            ss =>
                from o1 in ss.Set<Order>()
                from o2 in ss.Set<Order>()
                where o1.CustomerID.StartsWith("A")
                where o1.Customer.Equals(o2.Customer)
                orderby o1.OrderID, o2.OrderID
                select new { Id1 = o1.OrderID, Id2 = o2.OrderID },
            e => (e.Id1, e.Id2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Comparing_navigations_using_static_Equals(bool async)
        => AssertQuery(
            async,
            ss =>
                from o1 in ss.Set<Order>()
                from o2 in ss.Set<Order>()
                where o1.CustomerID.StartsWith("A")
                where Equals(o1.Customer, o2.Customer)
                orderby o1.OrderID, o2.OrderID
                select new { Id1 = o1.OrderID, Id2 = o2.OrderID },
            e => (e.Id1, e.Id2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Comparing_non_matching_entities_using_Equals(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                from o in ss.Set<Order>()
                where c.CustomerID == "ALFKI"
                where Equals(c, o)
                select new { Id1 = c.CustomerID, Id2 = o.OrderID },
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Comparing_non_matching_collection_navigations_using_Equals(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                from o in ss.Set<Order>()
                where c.CustomerID == "ALFKI"
                where c.Orders.Equals(o.OrderDetails)
                select new { Id1 = c.CustomerID, Id2 = o.OrderID },
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Comparing_collection_navigation_to_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders == null).Select(c => c.CustomerID),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Comparing_collection_navigation_to_null_complex(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>()
                .Where(od => od.OrderID < 10250)
                .Where(od => od.Order.Customer.Orders != null)
                .OrderBy(od => od.OrderID)
                .ThenBy(od => od.ProductID)
                .Select(od => new { od.ProductID, od.OrderID }),
            e => (e.ProductID, e.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Compare_collection_navigation_with_itself(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where c.CustomerID.StartsWith("A")
                  where c.Orders == c.Orders
                  select c.CustomerID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Compare_two_collection_navigations_with_different_query_sources(bool async)
        => AssertQuery(
            async,
            ss =>
                from c1 in ss.Set<Customer>()
                from c2 in ss.Set<Customer>()
                where c1.CustomerID == "ALFKI" && c2.CustomerID == "ALFKI"
                where c1.Orders == c2.Orders
                select new { Id1 = c1.CustomerID, Id2 = c2.CustomerID },
            e => (e.Id1, e.Id2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Compare_two_collection_navigations_using_equals(bool async)
        => AssertQuery(
            async,
            ss =>
                from c1 in ss.Set<Customer>()
                from c2 in ss.Set<Customer>()
                where c1.CustomerID == "ALFKI" && c2.CustomerID == "ALFKI"
                where Equals(c1.Orders, c2.Orders)
                select new { Id1 = c1.CustomerID, Id2 = c2.CustomerID },
            e => (e.Id1, e.Id2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Compare_two_collection_navigations_with_different_property_chains(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                where c.CustomerID == "ALFKI"
                from o in ss.Set<Order>()
                where c.Orders == o.Customer.Orders
                orderby c.CustomerID, o.OrderID
                select new { Id1 = c.CustomerID, Id2 = o.OrderID },
            e => (e.Id1, e.Id2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_ThenBy_same_column_different_direction(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.CustomerID.StartsWith("A"))
                .OrderBy(c => c.CustomerID)
                .ThenByDescending(c => c.CustomerID)
                .Select(c => c.CustomerID),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_OrderBy_same_column_different_direction(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.CustomerID.StartsWith("A"))
                .OrderBy(c => c.CustomerID)
                .OrderByDescending(c => c.CustomerID)
                .Select(c => c.CustomerID),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_nested_query_doesnt_try_binding_to_grandparent_when_parent_returns_complex_result(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI")
                .Select(
                    c => new
                    {
                        c.CustomerID,
                        OuterOrders = c.Orders.Select(
                            o => new { InnerOrder = c.Orders.Count(), Id = c.CustomerID }).ToList()
                    }),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.CustomerID, a.CustomerID);
                Assert.Equal(e.OuterOrders.Count, a.OuterOrders.Count);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_nested_query_properly_binds_to_grandparent_when_parent_returns_scalar_result(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI")
                .Select(c => new { c.CustomerID, OuterOrders = c.Orders.Count(o => c.Orders.Count() > 0) }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_Dto_projection_skip_take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID)
                .Select(c => new { Id = c.CustomerID })
                .Skip(5)
                .Take(10),
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_take_count_works(bool async)
        => AssertCount(
            async,
            ss => (from o in ss.Set<Order>().Where(o => o.OrderID > 690 && o.OrderID < 710)
                   join c in ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI")
                       on o.CustomerID equals c.CustomerID
                   select o)
                .Take(5));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_empty_list_contains(bool async)
    {
        var list = new List<string>();

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => list.Contains(c.CustomerID)).Select(c => c));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_empty_list_does_not_contains(bool async)
    {
        var list = new List<string>();

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => !list.Contains(c.CustomerID)).Select(c => c));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Manual_expression_tree_typed_null_equality(bool async)
    {
        using var context = CreateContext();
        var orderParameter = Expression.Parameter(typeof(Order), "o");
        var orderCustomer = Expression.MakeMemberAccess(
            orderParameter, typeof(Order).GetMember(nameof(Order.Customer))[0]);

        var selector = Expression.Lambda<Func<Order, string>>(
            Expression.Condition(
                Expression.NotEqual(
                    orderCustomer,
                    Expression.Constant(null, typeof(Customer))),
                Expression.MakeMemberAccess(
                    orderCustomer,
                    typeof(Customer).GetMember(nameof(Customer.City))[0]),
                Expression.Constant(null, typeof(string))),
            orderParameter);

        return AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .Select(selector));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Let_subquery_with_multiple_occurrences(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<Order>()
                  let details =
                      from od in o.OrderDetails
                      where od.Quantity < 10
                      select od.Quantity
                  where details.Any()
                  select new { Count = details.Count() });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Let_entity_equality_to_null(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A"))
                  let o = c.Orders.OrderBy(e => e.OrderDate).FirstOrDefault()
                  where o != null
                  select new { c.CustomerID, o.OrderDate });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Let_entity_equality_to_other_entity(bool async)
    {
        return AssertQuery(
            async,
            ss => from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A"))
                  let o = c.Orders.OrderBy(e => e.OrderDate).FirstOrDefault()
                  where o != new Order()
                  select new
                  {
                      c.CustomerID,
#pragma warning disable IDE0031 // Use null propagation
                      A = (o != null ? o.OrderDate : null)
#pragma warning restore IDE0031 // Use null propagation
                  });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_after_client_method(bool async)
        => AssertTranslationFailed(
            () => AssertQueryScalar(
                async,
                ss => ss.Set<Customer>().OrderBy(c => ClientOrderBy(c))
                    .SelectMany(c => c.Orders)
                    .Distinct()
                    .Select(o => o.OrderDate)));

    private static string ClientOrderBy(Customer c)
        => c.CustomerID;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Client_OrderBy_GroupBy_Group_ordering_works(bool async)
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => from o in ss.Set<Order>()
                      orderby ClientEvalSelector(o)
                      group o by o.CustomerID
                      into g
                      orderby g.Key
                      select g.OrderByDescending(x => x.OrderID).ToList(),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a, ordered: true)));

    protected static bool ClientEvalPredicate(Order order)
        => order.OrderID > 10000;

    protected internal uint ClientEvalSelector(Order order)
        => order.EmployeeID % 10 ?? 0;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_navigation_equal_to_null_for_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault().OrderDetails == null),
            ss => ss.Set<Customer>().Where(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault() == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Dependent_to_principal_navigation_equal_to_null_for_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault().Customer == null),
            ss => ss.Set<Customer>().Where(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).FirstOrDefault() == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_navigation_equality_rewrite_for_subquery(bool async)
        // Dependency issues between visitors Issue #20445.
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(
                    c => c.CustomerID.StartsWith("A")
                        && ss.Set<Order>().Where(o => o.OrderID < 10300).OrderBy(o => o.OrderID).FirstOrDefault().OrderDetails
                        == ss.Set<Order>().Where(o => o.OrderID > 10500).OrderBy(o => o.OrderID).FirstOrDefault().OrderDetails)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inner_parameter_in_nested_lambdas_gets_preserved(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.Where(o => c == new Customer { CustomerID = o.CustomerID }).Count() > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Convert_to_nullable_on_nullable_value_is_ignored(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Select(o => new Order { OrderDate = o.OrderDate.Value }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_inside_interpolated_string_is_expanded(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Select(o => $"CustomerCity:{o.Customer.City}"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Client_code_using_instance_method_throws(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => InstanceMethod(c)));

    private string InstanceMethod(Customer c)
        => c.City;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Client_code_using_instance_in_static_method(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => StaticMethod(this, c)));

    private static string StaticMethod(NorthwindMiscellaneousQueryTestBase<TFixture> containingClass, Customer c)
        => c.City;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Client_code_using_instance_in_anonymous_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new { A = this }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Client_code_unknown_method(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => UnknownMethod(c.ContactName) == "foo"),
            assertEmpty: true);

    public static string UnknownMethod(string foo)
        => foo;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Context_based_client_method(bool async)
    {
        using (var context = CreateContext())
        {
            var query = context.Customers.Select(c => context.ClientMethod(c));

            // Memory leak would throw exception. This verifies that we are not leaking.
            var result = async
                ? (await query.ToListAsync())
                : query.ToList();

            Assert.Equal(91, result.Count);
            Assert.Equal(85, result.Count(e => e));
        }

        // re-run using different context to verify that previous context is not in the cache.
        using (var context = CreateContext())
        {
            var query = context.Customers.Select(c => context.ClientMethod(c));

            var result = async
                ? (await query.ToListAsync())
                : query.ToList();

            Assert.Equal(91, result.Count);
            Assert.Equal(85, result.Count(e => e));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_object_type_server_evals(bool async)
    {
        Expression<Func<Order, object>>[] orderingExpressions =
        [
            o => o.OrderID, o => o.OrderDate, o => o.Customer.CustomerID, o => o.Customer.City
        ];

        return AssertQuery(
            async,
            ss => ss.Set<Order>().OrderBy(orderingExpressions[0])
                .ThenBy(orderingExpressions[1])
                .ThenBy(orderingExpressions[2])
                .ThenBy(orderingExpressions[3])
                .Skip(0)
                .Take(20),
            assertOrder: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task AsQueryable_in_query_server_evals(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID)
                .Select(
                    c => c.Orders.AsQueryable()
                        .Where(ValidYear)
                        .OrderBy(o => o.OrderID)
                        .Take(1)
                        .Select(o => new { o.OrderDate }).ToList()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, ordered: true));

    private static Expression<Func<Order, bool>> ValidYear
        => a => a.OrderDate.Value.Year == 1998;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_DefaultIfEmpty_Any(bool async)
        => AssertAny(
            async,
            ss => (from e in ss.Set<Employee>()
                       .Where(e => e.EmployeeID == NonExistentID)
                       .Select(e => e.EmployeeID)
                       .DefaultIfEmpty()
                   select e));

    // Issue#18374
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_skip_collection_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .OrderBy(o => o.OrderID)
                .Select(o => new { Item = o })
                .Skip(5)
                .Select(e => new { e.Item.OrderID, ProductIds = e.Item.OrderDetails.Select(od => od.ProductID).ToList() }),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.OrderID, a.OrderID);
                AssertCollection(e.ProductIds, a.ProductIds, ordered: true, elementAsserter: (ie, ia) => Assert.Equal(ie, ia));
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_take_collection_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .OrderBy(o => o.OrderID)
                .Select(o => new { Item = o })
                .Take(10)
                .Select(e => new { e.Item.OrderID, ProductIds = e.Item.OrderDetails.Select(od => od.ProductID).ToList() }),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.OrderID, a.OrderID);
                AssertCollection(e.ProductIds, a.ProductIds, ordered: true, elementAsserter: (ie, ia) => Assert.Equal(ie, ia));
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_skip_take_collection_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .OrderBy(o => o.OrderID)
                .Select(o => new { Item = o })
                .Skip(5)
                .Take(10)
                .Select(e => new { e.Item.OrderID, ProductIds = e.Item.OrderDetails.Select(od => od.ProductID).ToList() }),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.OrderID, a.OrderID);
                AssertCollection(e.ProductIds, a.ProductIds, ordered: true, elementAsserter: (ie, ia) => Assert.Equal(ie, ia));
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_skip_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .OrderBy(o => o.OrderID)
                .Select(o => new { Item = o })
                .Skip(5)
                .Select(e => new { e.Item.Customer.City }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_take_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .OrderBy(o => o.OrderID)
                .Select(o => new { Item = o })
                .Take(10)
                .Select(e => new { e.Item.Customer.City }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_skip_take_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .OrderBy(o => o.OrderID)
                .Select(o => new { Item = o })
                .Skip(5)
                .Take(10)
                .Select(e => new { e.Item.Customer.City }));

    // Issue#19207
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_projection_skip(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .OrderBy(o => o.OrderID)
                .Select(o => new { Order = o, o.OrderDetails })
                .Skip(5),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Order, a.Order);
                AssertCollection(e.OrderDetails, a.OrderDetails);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_projection_take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .OrderBy(o => o.OrderID)
                .Select(o => new { Order = o, o.OrderDetails })
                .Take(10),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Order, a.Order);
                AssertCollection(e.OrderDetails, a.OrderDetails);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_projection_skip_take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .OrderBy(o => o.OrderID)
                .Select(o => new { Order = o, o.OrderDetails })
                .Skip(5)
                .Take(10),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Order, a.Order);
                AssertCollection(e.OrderDetails, a.OrderDetails);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Anonymous_projection_skip_empty_collection_FirstOrDefault(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.CustomerID == "FISSA")
                .Select(c => new { Customer = c })
                .Skip(0)
                .Select(e => e.Customer.Orders.FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Anonymous_projection_take_empty_collection_FirstOrDefault(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.CustomerID == "FISSA")
                .Select(c => new { Customer = c })
                .Take(1)
                .Select(e => e.Customer.Orders.FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Anonymous_projection_skip_take_empty_collection_FirstOrDefault(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.CustomerID == "FISSA")
                .Select(c => new { Customer = c })
                .Skip(0)
                .Take(1)
                .Select(e => e.Customer.Orders.FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Checked_context_with_arithmetic_does_not_fail(bool isAsync)
    {
        checked
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<OrderDetail>()
                    .Where(w => w.Quantity + 1 == 5 && w.Quantity - 1 == 3 && w.Quantity * 1 == w.Quantity)
                    .OrderBy(o => o.OrderID),
                assertOrder: true,
                elementAsserter: (e, a) => { AssertEqual(e, a); });
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Checked_context_with_case_to_same_nullable_type_does_not_fail(bool isAsync)
        => AssertMax(
            isAsync,
            ss => ss.Set<OrderDetail>(),
            ss => ss.Set<OrderDetail>(),
            detail => detail.Quantity,
            detail => (short?)detail.Quantity
        );

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_with_null_coalesce_client_side(bool async)
    {
        var a = new Customer { CustomerID = "ALFKI" };
        var b = a;

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c == (a ?? b)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_contains_with_list_of_null(bool async)
    {
        var customers = new List<Customer> { null, new() { CustomerID = "ALFKI" } };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => customers.Contains(c)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task MemberInitExpression_NewExpression_is_funcletized_even_when_bindings_are_not_evaluatable(bool async)
    {
        var randomString = "random";
        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A"))
                .Select(c => new Dto(randomString) { CustomerID = c.CustomerID, NestedDto = new Dto(randomString) }),
            elementSorter: e => e.CustomerID,
            elementAsserter: (e, a) => Assert.Equal(e.CustomerID, a.CustomerID));
    }

    private class Dto(string value)
    {
        public string Value { get; } = value;
        public string CustomerID { get; set; }
        public Dto NestedDto { get; set; }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Null_parameter_name_works(bool async)
    {
        using var context = CreateContext();
        var customerDbSet = context.Set<Customer>().AsQueryable();

        var parameter = Expression.Parameter(typeof(Customer));
        var body = Expression.Equal(parameter, Expression.Default(typeof(Customer)));
        var queryExpression = Expression.Call(
            QueryableMethods.Where.MakeGenericMethod(typeof(Customer)),
            customerDbSet.Expression,
            Expression.Quote(Expression.Lambda(body, parameter)));

        var query = ((IAsyncQueryProvider)customerDbSet.Provider).CreateQuery<Customer>(queryExpression);

        var result = async
            ? (await query.ToListAsync())
            : query.ToList();

        Assert.Empty(result);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_include_on_incorrect_property_throws(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(
            async () => await AssertQuery(async, ss => ss.Set<Customer>().Include("OrderDetails")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task EF_Property_include_on_incorrect_property_throws(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(
            async () => await AssertQuery(async, ss => ss.Set<Customer>().Include(c => EF.Property<Customer>(c, "OrderDetails"))));

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public virtual async Task Perform_identity_resolution_reuses_same_instances(bool async, bool useAsTracking)
    {
        using var context = CreateContext();
        var orderIds = context.Customers.Where(c => c.CustomerID == "ALFKI")
            .SelectMany(c => c.Orders).Select(o => o.OrderID).ToList();

        var query = context.Orders.Where(o => orderIds.Contains(o.OrderID))
            .Select(o => o.Customer);

        query = useAsTracking
            ? query.AsTracking(QueryTrackingBehavior.NoTrackingWithIdentityResolution)
            : query.AsNoTrackingWithIdentityResolution();

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Equal(6, result.Count);
        var firstCustomer = result[0];
        Assert.All(result, t => Assert.Same(firstCustomer, t));
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public virtual async Task Perform_identity_resolution_reuses_same_instances_across_joins(bool async, bool useAsTracking)
    {
        using var context = CreateContext();

        var query = (from c in context.Customers.Where(c => c.CustomerID.StartsWith("A"))
                     join o in context.Orders.Where(o => o.OrderID < 10500).Include(o => o.Customer)
                         on c.CustomerID equals o.CustomerID
                     select new { c, o });

        query = useAsTracking
            ? query.AsTracking(QueryTrackingBehavior.NoTrackingWithIdentityResolution)
            : query.AsNoTrackingWithIdentityResolution();

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        var arouts = result.Where(t => t.c.CustomerID == "AROUT").Select(t => t.c)
            .Concat(result.Where(t => t.o.CustomerID == "AROUT").Select(t => t.o.Customer))
            .ToList();

        var firstArout = arouts[0];
        Assert.All(arouts, t => Assert.Same(firstArout, t));
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Using_string_Equals_with_StringComparison_throws_informative_error(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.Equals("ALFKI", StringComparison.InvariantCulture)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Using_static_string_Equals_with_StringComparison_throws_informative_error(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => string.Equals(c.CustomerID, "ALFKI", StringComparison.InvariantCulture)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Single_non_scalar_projection_after_skip_uses_join(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.Orders.OrderBy(o => o.OrderDate).ThenBy(o => o.OrderID).Skip(2).FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_distinct_Select_with_client_bindings(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID < 20000).Select(o => o.OrderDate.Value.Year).Distinct()
                .Select(e => new DTO<int> { Property = ClientMethod(e) }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ToList_over_string(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Select(e => new { Property = e.City.ToList() }),
            assertOrder: true,
            elementAsserter: (e, a) => Assert.True(e.Property.SequenceEqual(a.Property)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ToArray_over_string(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Select(e => new { Property = e.City.ToArray() }),
            assertOrder: true,
            elementAsserter: (e, a) => Assert.True(e.Property.SequenceEqual(a.Property)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task AsEnumerable_over_string(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Select(e => new { Property = e.City.AsEnumerable() }),
            assertOrder: true,
            elementAsserter: (e, a) => Assert.True(e.Property.SequenceEqual(a.Property)));

    private static int ClientMethod(int s)
        => s;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Non_nullable_property_through_optional_navigation(bool async)
        => Assert.Equal(
            "Nullable object must have a value.",
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<Customer>().Select(e => new { e.Region.Length })))).Message);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Max_on_empty_sequence_throws(bool async)
    {
        using var context = CreateContext();
        var query = context.Set<Customer>().Select(e => new { Max = e.Orders.Max(o => o.OrderID) });

        _ = async ? await query.ToListAsync() : query.ToList();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Pending_selector_in_cardinality_reducing_method_is_applied_before_expanding_collection_navigation_member(
        bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.CustomerID.StartsWith("F"))
                .OrderBy(c => c.CustomerID)
                .Select(
                    c => new
                    {
                        Complex = (bool?)c.Orders.OrderBy(e => e.OrderDate).FirstOrDefault().Customer.Orders
                            .Any(e => e.OrderID < 11000)
                    }),
            ss => ss.Set<Customer>()
                .Where(c => c.CustomerID.StartsWith("F"))
                .OrderBy(c => c.CustomerID)
                .Select(
                    c => new
                    {
                        Complex = c.Orders.OrderBy(e => e.OrderDate).FirstOrDefault() != null
                            ? c.Orders.OrderBy(e => e.OrderDate).FirstOrDefault().Customer.Orders.Any(e => e.OrderID < 11000)
                            : (bool?)false
                    }),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distinct_followed_by_ordering_on_condition(bool async)
    {
        var searchTerm = "c";
        return AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.CustomerID != "VAFFE" && c.CustomerID != "DRACD")
                .Select(e => e.City)
                .Distinct()
                .OrderBy(x => x.IndexOf(searchTerm))
                .ThenBy(x => x)
                .Take(5),
            assertOrder: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DefaultIfEmpty_Sum_over_collection_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Select(c => new { c.CustomerID, Sum = c.Orders.Select(o => o.OrderID).DefaultIfEmpty().Sum() }),
            elementSorter: c => c.CustomerID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_on_subquery_with_null_check(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Select(
                    c => new
                    {
                        c.CustomerID,
                        Order = (c.Orders.Any() ? c.Orders.FirstOrDefault() : null) == null
                            ? null
                            : new { c.Orders.OrderBy(o => o.OrderID).FirstOrDefault().OrderDate }
                    }),
            elementSorter: c => c.CustomerID,
            elementAsserter: (e, a) => AssertEqual(e.Order, a.Order));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DefaultIfEmpty_over_empty_collection_followed_by_projecting_constant(bool async)
        => AssertFirstOrDefault(
            async,
            ss => ss.Set<Customer>().Where(c => false).DefaultIfEmpty().Select(c => "520"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FirstOrDefault_with_predicate_nested(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")).OrderBy(c => c.CustomerID).Select(TestDto.Projection),
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")).OrderBy(c => c.CustomerID)
                .Select(
                    x => new TestDto
                    {
                        CustomerID = x.CustomerID,
                        OrderDate = x.Orders
                            .FirstOrDefault(t => t.OrderID == t.OrderID)
                            .MaybeScalar(e => e.OrderDate)
                    }),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.CustomerID, a.CustomerID);
                Assert.Equal(e.OrderDate, a.OrderDate);
            });

    public class TestDto
    {
        public string CustomerID { get; set; }
        public DateTime? OrderDate { get; set; }

        public static readonly Expression<Func<Customer, TestDto>> Projection = x => new TestDto
        {
            CustomerID = x.CustomerID,
            OrderDate = x.Orders
                .FirstOrDefault(t => t.OrderID == t.OrderID)
                .OrderDate
        };
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task First_on_collection_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")).OrderBy(c => c.CustomerID)
                .Select(c => new { c.CustomerID, OrderDate = c.Orders.Any() ? c.Orders.First().OrderDate : default }),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.CustomerID, a.CustomerID);
                Assert.Equal(e.OrderDate, a.OrderDate);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SkipWhile_throws_meaningful_exception(bool async)
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).SkipWhile(c => c.CustomerID != "Foo").Skip(1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Skip_0_Take_0_works_when_parameter(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Skip(0).Take(0),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Skip(1).Take(1));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_0_Take_0_works_when_constant(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                .OrderBy(c => c.CustomerID)
                .Select(e => e.Orders.OrderBy(o => o.OrderID).Skip(0).Take(0).Any()),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_1_Take_0_works_when_constant(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                .OrderBy(c => c.CustomerID)
                .Select(e => e.Orders.OrderBy(o => o.OrderID).Skip(1).Take(0).Any()),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_0_works_when_constant(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                .OrderBy(c => c.CustomerID)
                .Select(e => e.Orders.OrderBy(o => o.OrderID).Take(0).Any()),
            assertOrder: true);

    [ConditionalFact]
    public virtual async Task ToListAsync_can_be_canceled()
    {
        for (var i = 0; i < 10; i++)
        {
            // without fix, this usually throws within 2 or three iterations

            Fixture.ListLoggerFactory.Log.Clear();
            using var context = CreateContext();
            var tokenSource = new CancellationTokenSource();
            var query = context.Employees.AsNoTracking().ToListAsync(tokenSource.Token);
            tokenSource.Cancel();
            List<Employee> result = null;
            Exception exception = null;
            try
            {
                result = await query;
            }
            catch (Exception e)
            {
                exception = e;
            }

            if (exception != null)
            {
                Assert.Null(result);
                Assert.Contains(CoreEventId.QueryCanceled, Fixture.ListLoggerFactory.Log.Select(l => l.Id));
            }
            else
            {
                Assert.Equal(9, result.Count);
            }
        }
    }

    [ConditionalFact]
    public virtual async Task ToListAsync_with_canceled_token()
    {
        using var context = CreateContext();

        await Assert.ThrowsAsync<OperationCanceledException>(() => context.Employees.ToListAsync(new CancellationToken(true)));

        Assert.Contains(CoreEventId.QueryCanceled, Fixture.ListLoggerFactory.Log.Select(l => l.Id));
    }

    [ConditionalFact]
    public virtual async Task Mixed_sync_async_query()
    {
        using var context = CreateContext();

        // Bad test. Issue 17019.
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
            {
                var results
                    = (await context.Customers
                        .Select(
                            c => new { c.CustomerID, Orders = context.Orders.Where(o => o.Customer.CustomerID == c.CustomerID) })
                        .ToListAsync())
                    .Select(
                        x => new
                        {
                            Orders = x.Orders
                                .GroupJoin(
                                    new[] { "ALFKI" }, y => x.CustomerID, y => y, (h, id) => new { h.Customer })
                        })
                    .ToList();

                Assert.Equal(830, results.SelectMany(r => r.Orders).ToList().Count);
            });
    }

    protected virtual async Task Single_Predicate_Cancellation_test(CancellationToken cancellationToken)
    {
        using var ctx = CreateContext();
        var result = await ctx.Customers.SingleAsync(c => c.CustomerID == "ALFKI", cancellationToken);

        Assert.Equal("ALFKI", result.CustomerID);
    }

    [ConditionalFact]
    public virtual async Task Mixed_sync_async_in_query_cache()
    {
        using var context = CreateContext();
        Assert.Equal(91, context.Customers.AsNoTracking().ToList().Count);
        Assert.Equal(91, (await context.Customers.AsNoTracking().ToListAsync()).Count);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Load_should_track_results(bool async)
    {
        using var context = CreateContext();
        if (async)
        {
            await context.Customers.LoadAsync();
        }
        else
        {
            context.Customers.Load();
        }

        Assert.Equal(91, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_with_distinct_without_default_identifiers_projecting_columns(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Select(
                    c => new
                    {
                        Key = c.CustomerID,
                        Subquery = c.Orders
                            .Select(o => new { First = o.OrderID, Second = o.OrderDate })
                            .Distinct().ToList()
                    }),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Key, a.Key);
                AssertCollection(
                    e.Subquery,
                    a.Subquery,
                    elementSorter: ee => ee.First,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.First, aa.First);
                        Assert.Equal(ee.Second, aa.Second);
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_collection_with_distinct_without_default_identifiers_projecting_columns_with_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Select(
                    c => new
                    {
                        Key = c.CustomerID,
                        Subquery = c.Orders
                            .Select(
                                o => new
                                {
                                    First = o.OrderID,
                                    Second = o.OrderDate,
                                    Third = o.Customer.City
                                })
                            .Distinct().ToList()
                    }),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Key, a.Key);
                AssertCollection(
                    e.Subquery,
                    a.Subquery,
                    elementSorter: ee => ee.First,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.First, aa.First);
                        Assert.Equal(ee.Second, aa.Second);
                        Assert.Equal(ee.Third, aa.Third);
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nested_collection_with_distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Where(c => c.CustomerID.StartsWith("A"))
                .Select(
                    c => c.Orders.Any()
                        ? c.Orders.Select(o => o.CustomerID).Distinct().ToArray()
                        : Array.Empty<string>()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_projection_after_DefaultIfEmpty(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == "Seattle").DefaultIfEmpty()
                .OrderBy(c => c.CustomerID)
                .Select(e => new { e.Orders }),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e.Orders, a.Orders));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_constant_zero(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.OrderBy(o => o.OrderID).ElementAtOrDefault(0).OrderDetails == null),
            ss => ss.Set<Customer>().Where(c => c.Orders.OrderBy(o => o.OrderID).ElementAtOrDefault(0) == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_constant_one(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.OrderBy(o => o.OrderID).ElementAtOrDefault(1).OrderDetails == null),
            ss => ss.Set<Customer>().Where(c => c.Orders.OrderBy(o => o.OrderID).ElementAtOrDefault(1) == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_parameter(bool async)
    {
        var prm = 2;

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.OrderBy(o => o.OrderID).ElementAtOrDefault(prm).OrderDetails == null),
            ss => ss.Set<Customer>().Where(c => c.Orders.OrderBy(o => o.OrderID).ElementAtOrDefault(prm) == null));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_with_navigation_inside_inline_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => new[] { 100, c.Orders.Count }.Sum() > 101));

    [ConditionalTheory] // #32234
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_collection_Contains_with_projection_and_ordering(bool async)
    {
        var ids = new[] { 10248, 10249 };

        await AssertQuery(
            async,
            ss => ss.Set<OrderDetail>()
                .Where(e => ids.Contains(e.OrderID))
                .GroupBy(e => e.Quantity)
                .Select(g => new { g.Key, MaxTimestamp = g.Select(e => e.Order.OrderDate).Max() })
                .OrderBy(x => x.MaxTimestamp)
                .Select(x => x));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_over_concatenated_columns_with_different_sizes(bool async)
    {
        var data = new[] { "ALFKI" + "Alfreds Futterkiste", "ANATR" + "Ana Trujillo Emparedados y helados" };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => data.Contains(c.CustomerID + c.CompanyName)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_over_concatenated_column_and_constant(bool async)
    {
        var data = new[] { "ALFKI" + "SomeConstant", "ANATR" + "SomeConstant", "ALFKI" + "X" };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => data.Contains(c.CustomerID + "SomeConstant")));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_over_concatenated_column_and_parameter(bool async)
    {
        var data = new[] { "ALFKI" + "SomeVariable", "ANATR" + "SomeVariable", "ALFKI" + "X" };
        var someVariable = "SomeVariable";

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => data.Contains(c.CustomerID + someVariable)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_over_concatenated_parameter_and_constant(bool async)
    {
        var data = new[] { "ALFKI" + "SomeConstant", "ANATR" + "SomeConstant" };
        var someVariable = "ALFKI";

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => data.Contains(someVariable + "SomeConstant")));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_over_concatenated_columns_both_fixed_length(bool async)
    {
        var data = new[] { "ALFKIALFKI", "ALFKI", "ANATR" + "Ana Trujillo Emparedados y helados", "ANATR" + "ANATR" };

        return AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => data.Contains(o.CustomerID + o.Customer.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Compiler_generated_local_closure_produces_valid_parameter_name(bool async)
        => Run_compiler_generated_local_closure_produces_valid_parameter_name(
            async,
            new MyCustomerDetails { CustomerId = "ALFKI", City = "Berlin" });

    private Task Run_compiler_generated_local_closure_produces_valid_parameter_name(
        bool async,
        MyCustomerDetails details)
    {
        var customerId = details.CustomerId;

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(x => x.CustomerID == customerId && x.City == details.City));
    }

    private class MyCustomerDetails
    {
        public string CustomerId { get; set; }
        public string City { get; set; }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Static_member_access_gets_parameterized_within_larger_evaluatable(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == StaticProperty + "KI"));

    private static string StaticProperty
        => "ALF";
}
