// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#nullable disable

public abstract class NorthwindBulkUpdatesTestBase<TFixture> : BulkUpdatesTestBase<TFixture>
    where TFixture : NorthwindBulkUpdatesFixture<NoopModelCustomizer>, new()
{
    protected NorthwindBulkUpdatesTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_TagWith(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).TagWith("MyDelete"),
            rowsAffectedCount: 140);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300),
            rowsAffectedCount: 140);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Delete_Where_parameter(bool async)
    {
        int? quantity = 1;

        await AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.Quantity == quantity),
            rowsAffectedCount: 17);

        quantity = null;

        await AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.Quantity == quantity),
            rowsAffectedCount: 0);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_OrderBy(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).OrderBy(e => e.OrderID),
            rowsAffectedCount: 140);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_OrderBy_Skip(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).OrderBy(e => e.OrderID).Skip(100),
            rowsAffectedCount: 40);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_OrderBy_Take(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).OrderBy(e => e.OrderID).Take(100),
            rowsAffectedCount: 100);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_OrderBy_Skip_Take(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).OrderBy(e => e.OrderID).Skip(100).Take(100),
            rowsAffectedCount: 40);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_Skip(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).Skip(100),
            rowsAffectedCount: 40);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_Take(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).Take(100),
            rowsAffectedCount: 100);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_Skip_Take(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).Skip(100).Take(100),
            rowsAffectedCount: 40);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_predicate_with_GroupBy_aggregate(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>()
                .Where(
                    e => e.OrderID
                        < ss.Set<Order>()
                            .GroupBy(o => o.CustomerID)
                            .Where(g => g.Count() > 11)
                            .Select(g => g.First()).First().OrderID),
            rowsAffectedCount: 284);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_predicate_with_GroupBy_aggregate_2(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>()
                .Where(
                    e => ss.Set<Order>()
                        .GroupBy(o => o.CustomerID)
                        .Where(g => g.Count() > 9)
                        .Select(g => g.First()).Contains(e.Order)),
            rowsAffectedCount: 109);

    [ConditionalTheory(Skip = "Issue#28525")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_GroupBy_Where_Select(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>()
                .GroupBy(od => od.OrderID)
                .Where(g => g.Count() > 5)
                .Select(g => g.First()),
            rowsAffectedCount: 284);

    [ConditionalTheory(Skip = "Issue#26753")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_GroupBy_Where_Select_2(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>()
                .Where(
                    od => od
                        == ss.Set<OrderDetail>().GroupBy(od => od.OrderID).Where(g => g.Count() > 5).Select(g => g.First())
                            .FirstOrDefault()),
            rowsAffectedCount: 284);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_Skip_Take_Skip_Take_causing_subquery(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).Skip(100).Take(100).Skip(20).Take(5),
            rowsAffectedCount: 5);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_Distinct(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).Distinct(),
            rowsAffectedCount: 140);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_SelectMany(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<Order>().Where(e => e.OrderID < 10250).SelectMany(e => e.OrderDetails),
            rowsAffectedCount: 5);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_SelectMany_subquery(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<Order>().Where(e => e.OrderID < 10250).SelectMany(e => e.OrderDetails.Where(i => i.ProductID > 0)),
            rowsAffectedCount: 5);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_using_navigation(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.Order.OrderDate.Value.Year == 2000),
            rowsAffectedCount: 0);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_using_navigation_2(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.Order.Customer.CustomerID.StartsWith("F")),
            rowsAffectedCount: 164);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Union(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID < 10250)
                .Union(ss.Set<OrderDetail>().Where(od => od.OrderID > 11250)),
            rowsAffectedCount: 5);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Concat(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID < 10250)
                .Concat(ss.Set<OrderDetail>().Where(od => od.OrderID > 11250)),
            rowsAffectedCount: 5);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Intersect(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID < 10250)
                .Intersect(ss.Set<OrderDetail>().Where(od => od.OrderID > 11250)),
            rowsAffectedCount: 0);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Except(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID < 10250)
                .Except(ss.Set<OrderDetail>().Where(od => od.OrderID > 11250)),
            rowsAffectedCount: 5);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_non_entity_projection(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteDeleteOnNonEntityType,
            () => AssertDelete(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID < 10250).Select(e => e.ProductID),
                rowsAffectedCount: 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_non_entity_projection_2(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteDeleteOnNonEntityType,
            () => AssertDelete(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID < 10250)
                    .Select(e => new OrderDetail { OrderID = e.OrderID, ProductID = e.ProductID }),
                rowsAffectedCount: 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_non_entity_projection_3(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteDeleteOnNonEntityType,
            () => AssertDelete(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID < 10250)
                    .Select(e => new { OrderDetail = e, e.ProductID }),
                rowsAffectedCount: 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_FromSql_converted_to_subquery(bool async)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            () => Fixture.CreateContext(),
            (facade, transaction) => Fixture.UseTransaction(facade, transaction),
            async context =>
            {
                var queryable = context.Set<OrderDetail>().FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        @"SELECT [OrderID], [ProductID], [UnitPrice], [Quantity], [Discount]
FROM [Order Details]
WHERE [OrderID] < 10300"));

                if (async)
                {
                    await queryable.ExecuteDeleteAsync();
                }
                else
                {
                    queryable.ExecuteDelete();
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_optional_navigation_predicate(bool async)
        => AssertDelete(
            async,
            ss => from od in ss.Set<OrderDetail>()
                  where od.Order.Customer.City.StartsWith("Se")
                  select od,
            rowsAffectedCount: 66);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_with_join(bool async)
        => AssertDelete(
            async,
            ss => from od in ss.Set<OrderDetail>()
                  join o in ss.Set<Order>().Where(o => o.OrderID < 10300).OrderBy(e => e.OrderID).Skip(0).Take(100)
                      on od.OrderID equals o.OrderID
                  select od,
            rowsAffectedCount: 140);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_with_left_join(bool async)
        => AssertDelete(
            async,
            ss => from od in ss.Set<OrderDetail>().Where(e => e.OrderID < 10276)
                  join o in ss.Set<Order>().Where(o => o.OrderID < 10300).OrderBy(e => e.OrderID).Skip(0).Take(100)
                      on od.OrderID equals o.OrderID into grouping
                  from o in grouping.DefaultIfEmpty()
                  select od,
            rowsAffectedCount: 74);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_with_cross_join(bool async)
        => AssertDelete(
            async,
            ss => from od in ss.Set<OrderDetail>().Where(e => e.OrderID < 10276)
                  from o in ss.Set<Order>().Where(o => o.OrderID < 10300).OrderBy(e => e.OrderID).Skip(0).Take(100)
                  select od,
            rowsAffectedCount: 74);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_with_cross_apply(bool async)
        => AssertDelete(
            async,
            ss => from od in ss.Set<OrderDetail>().Where(e => e.OrderID < 10276)
                  from o in ss.Set<Order>().Where(o => o.OrderID < od.OrderID).OrderBy(e => e.OrderID).Skip(0).Take(100)
                  select od,
            rowsAffectedCount: 71);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_with_outer_apply(bool async)
        => AssertDelete(
            async,
            ss => from od in ss.Set<OrderDetail>().Where(e => e.OrderID < 10276)
                  from o in ss.Set<Order>().Where(o => o.OrderID < od.OrderID).OrderBy(e => e.OrderID).Skip(0).Take(100).DefaultIfEmpty()
                  select od,
            rowsAffectedCount: 74);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_set_constant_TagWith(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")).TagWith("MyUpdate"),
            e => e,
            s => s.SetProperty(c => c.ContactName, "Updated"),
            rowsAffectedCount: 8,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")),
            e => e,
            s => s.SetProperty(c => c.ContactName, "Updated"),
            rowsAffectedCount: 8,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Update_Where_parameter_set_constant(bool async)
    {
        var customer = "ALFKI";
        await AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == customer),
            e => e,
            s => s.SetProperty(c => c.ContactName, "Updated"),
            rowsAffectedCount: 1,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

        customer = null;
        await AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == customer),
            e => e,
            s => s.SetProperty(c => c.ContactName, "Updated"),
            rowsAffectedCount: 0,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_set_parameter(bool async)
    {
        var value = "Abc";
        return AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")),
            e => e,
            s => s.SetProperty(c => c.ContactName, value),
            rowsAffectedCount: 8,
            (b, a) => Assert.All(a, c => Assert.Equal("Abc", c.ContactName)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_set_parameter_from_closure_array(bool async)
    {
        var array = new[] { "Abc", "Def" };
        return AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")),
            e => e,
            s => s.SetProperty(c => c.ContactName, array[0]),
            rowsAffectedCount: 8,
            (b, a) => Assert.All(a, c => Assert.Equal("Abc", c.ContactName)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_set_parameter_from_inline_list(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")),
            e => e,
            s => s.SetProperty(c => c.ContactName, new List<string> { "Abc", "Def" }[0]),
            rowsAffectedCount: 8,
            (b, a) => Assert.All(a, c => Assert.Equal("Abc", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_set_parameter_from_multilevel_property_access(bool async)
    {
        var container = new Container { Containee = new Containee { Property = "Abc" } };

        return AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")),
            e => e,
            s => s.SetProperty(c => c.ContactName, container.Containee.Property),
            rowsAffectedCount: 8,
            (b, a) => Assert.All(a, c => Assert.Equal("Abc", c.ContactName)));
    }

    private class Container
    {
        public Containee Containee { get; set; }
    }

    private class Containee
    {
        public string Property { get; set; }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_Skip_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")).Skip(4),
            e => e,
            s => s.SetProperty(c => c.ContactName, "Updated"),
            rowsAffectedCount: 4,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_Take_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")).Take(4),
            e => e,
            s => s.SetProperty(c => c.ContactName, "Updated"),
            rowsAffectedCount: 4,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_Skip_Take_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")).Skip(2).Take(4),
            e => e,
            s => s.SetProperty(c => c.ContactName, "Updated"),
            rowsAffectedCount: 4,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_OrderBy_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")).OrderBy(c => c.City),
            e => e,
            s => s.SetProperty(c => c.ContactName, "Updated"),
            rowsAffectedCount: 8,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_OrderBy_Skip_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")).OrderBy(c => c.City).Skip(4),
            e => e,
            s => s.SetProperty(c => c.ContactName, "Updated"),
            rowsAffectedCount: 4,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_OrderBy_Take_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")).OrderBy(c => c.City).Take(4),
            e => e,
            s => s.SetProperty(c => c.ContactName, "Updated"),
            rowsAffectedCount: 4,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_OrderBy_Skip_Take_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")).OrderBy(c => c.City).Skip(2).Take(4),
            e => e,
            s => s.SetProperty(c => c.ContactName, "Updated"),
            rowsAffectedCount: 4,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_OrderBy_Skip_Take_Skip_Take_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")).OrderBy(c => c.City).Skip(2).Take(6).Skip(2).Take(2),
            e => e,
            s => s.SetProperty(c => c.ContactName, "Updated"),
            rowsAffectedCount: 2,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_GroupBy_aggregate_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>()
                .Where(
                    c => c.CustomerID
                        == ss.Set<Order>()
                            .GroupBy(e => e.CustomerID).Where(g => g.Count() > 11).Select(e => e.Key).FirstOrDefault()),
            e => e,
            s => s.SetProperty(c => c.ContactName, "Updated"),
            rowsAffectedCount: 1,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_GroupBy_First_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>()
                .Where(
                    c => c.CustomerID
                        == ss.Set<Order>()
                            .GroupBy(e => e.CustomerID).Where(g => g.Count() > 11).Select(e => e.First().CustomerID).FirstOrDefault()),
            e => e,
            s => s.SetProperty(c => c.ContactName, "Updated"),
            rowsAffectedCount: 1,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory(Skip = "Issue#26753")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_GroupBy_First_set_constant_2(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>()
                .Where(
                    c => c
                        == ss.Set<Order>()
                            .GroupBy(e => e.CustomerID).Where(g => g.Count() > 11).Select(e => e.First().Customer).FirstOrDefault()),
            e => e,
            s => s.SetProperty(c => c.ContactName, "Updated"),
            rowsAffectedCount: 1,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_GroupBy_First_set_constant_3(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>()
                .Where(
                    c => ss.Set<Order>()
                        .GroupBy(e => e.CustomerID).Where(g => g.Count() > 11).Select(e => e.First().Customer).Contains(c)),
            e => e,
            s => s.SetProperty(c => c.ContactName, "Updated"),
            rowsAffectedCount: 24,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_Distinct_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")).Distinct(),
            e => e,
            s => s.SetProperty(c => c.ContactName, "Updated"),
            rowsAffectedCount: 8,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_using_navigation_set_null(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Order>().Where(o => o.Customer.City == "Seattle"),
            e => e,
            s => s.SetProperty(c => c.OrderDate, (DateTime?)null),
            rowsAffectedCount: 14,
            (b, a) => Assert.All(a, c => Assert.Null(c.OrderDate)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_using_navigation_2_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.Order.Customer.City == "Seattle"),
            e => e,
            s => s.SetProperty(c => c.Quantity, 1),
            rowsAffectedCount: 40,
            (b, a) => Assert.All(a, c => Assert.Equal(1, c.Quantity)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_SelectMany_set_null(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")).SelectMany(c => c.Orders),
            e => e,
            s => s.SetProperty(c => c.OrderDate, (DateTime?)null),
            rowsAffectedCount: 63,
            (b, a) => Assert.All(a, c => Assert.Null(c.OrderDate)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_set_property_plus_constant(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")),
            e => e,
            s => s.SetProperty(c => c.ContactName, c => c.ContactName + "Abc"),
            rowsAffectedCount: 8,
            (b, a) => b.Zip(a).ForEach(e => Assert.Equal(e.First.ContactName + "Abc", e.Second.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_set_property_plus_parameter(bool async)
    {
        var value = "Abc";
        return AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")),
            e => e,
            s => s.SetProperty(c => c.ContactName, c => c.ContactName + value),
            rowsAffectedCount: 8,
            (b, a) => b.Zip(a).ForEach(e => Assert.Equal(e.First.ContactName + "Abc", e.Second.ContactName)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_set_property_plus_property(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")),
            e => e,
            s => s.SetProperty(c => c.ContactName, c => c.ContactName + c.CustomerID),
            rowsAffectedCount: 8,
            (b, a) => b.Zip(a).ForEach(e => Assert.Equal(e.First.ContactName + e.First.CustomerID, e.Second.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_set_constant_using_ef_property(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")),
            e => e,
            s => s.SetProperty(c => EF.Property<string>(c, "ContactName"), "Updated"),
            rowsAffectedCount: 8,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_set_null(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")),
            e => e,
            s => s.SetProperty(c => c.ContactName, (string)null),
            rowsAffectedCount: 8,
            (b, a) => Assert.All(a, c => Assert.Null(c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_without_property_to_set_throws(bool async)
        => AssertTranslationFailed(
            RelationalStrings.NoSetPropertyInvocation,
            () => AssertUpdate(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID < 10250),
                e => e,
                s => s,
                rowsAffectedCount: 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_with_invalid_lambda_throws(bool async)
        => AssertTranslationFailed(
            RelationalStrings.InvalidArgumentToExecuteUpdate,
            () => AssertUpdate(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID < 10250),
                e => e,
                s => s.Maybe(e => e),
                rowsAffectedCount: 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_multiple_set(bool async)
    {
        var value = "Abc";
        return AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")),
            e => e,
            s => s.SetProperty(c => c.ContactName, c => value).SetProperty(c => c.City, "Seattle"),
            rowsAffectedCount: 8,
            (b, a) => Assert.All(
                a, c =>
                {
                    Assert.Equal("Abc", c.ContactName);
                    Assert.Equal("Seattle", c.City);
                }));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_with_invalid_lambda_in_set_property_throws(bool async)
        => AssertTranslationFailed(
            RelationalStrings.InvalidPropertyInSetProperty(
                new ExpressionPrinter().PrintExpression((OrderDetail e) => e.MaybeScalar(e => e.OrderID))),
            () => AssertUpdate(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID < 10250),
                e => e,
                s => s.SetProperty(e => e.MaybeScalar(e => e.OrderID), 10300),
                rowsAffectedCount: 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_multiple_tables_throws(bool async)
        => AssertTranslationFailed(
            RelationalStrings.MultipleTablesInExecuteUpdate("c => c.Customer.ContactName", "c => c.e.OrderDate"),
            () => AssertUpdate(
                async,
                ss => ss.Set<Order>()
                    .Where(o => o.CustomerID.StartsWith("F"))
                    .Select(e => new { e, e.Customer }),
                e => e.Customer,
                s => s
                    .SetProperty(c => c.Customer.ContactName, "Name")
                    .SetProperty(c => c.e.OrderDate, new DateTime(2020, 1, 1)),
                rowsAffectedCount: 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_unmapped_property_throws(bool async)
        => AssertTranslationFailed(
            RelationalStrings.InvalidPropertyInSetProperty("c => c.IsLondon"),
            () => AssertUpdate(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")),
                e => e,
                s => s.SetProperty(c => c.IsLondon, true),
                rowsAffectedCount: 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Union_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                .Union(ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A"))),
            e => e,
            s => s.SetProperty(c => c.ContactName, "Updated"),
            rowsAffectedCount: 12,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Concat_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                .Concat(ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A"))),
            e => e,
            s => s.SetProperty(c => c.ContactName, "Updated"),
            rowsAffectedCount: 12,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Except_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                .Except(ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A"))),
            e => e,
            s => s.SetProperty(c => c.ContactName, "Updated"),
            rowsAffectedCount: 8,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Intersect_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                .Intersect(ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A"))),
            e => e,
            s => s.SetProperty(c => c.ContactName, "Updated"),
            rowsAffectedCount: 0,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_with_join_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                  join o in ss.Set<Order>().Where(o => o.OrderID < 10300)
                      on c.CustomerID equals o.CustomerID
                  select new { c, o },
            e => e.c,
            s => s.SetProperty(c => c.c.ContactName, "Updated"),
            rowsAffectedCount: 2,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_with_left_join_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                  join o in ss.Set<Order>().Where(o => o.OrderID < 10300)
                      on c.CustomerID equals o.CustomerID into grouping
                  from o in grouping.DefaultIfEmpty()
                  select new { c, o },
            e => e.c,
            s => s.SetProperty(c => c.c.ContactName, "Updated"),
            rowsAffectedCount: 8,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_with_cross_join_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                  from o in ss.Set<Order>().Where(o => o.OrderID < 10300)
                  select new { c, o },
            e => e.c,
            s => s.SetProperty(c => c.c.ContactName, "Updated"),
            rowsAffectedCount: 8,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_with_cross_apply_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                  from o in ss.Set<Order>().Where(o => o.OrderID < 10300 && o.OrderDate.Value.Year < c.ContactName.Length)
                  select new { c, o },
            e => e.c,
            s => s.SetProperty(c => c.c.ContactName, "Updated"),
            rowsAffectedCount: 0,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_with_outer_apply_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                  from o in ss.Set<Order>().Where(o => o.OrderID < 10300 && o.OrderDate.Value.Year < c.ContactName.Length).DefaultIfEmpty()
                  select new { c, o },
            e => e.c,
            s => s.SetProperty(c => c.c.ContactName, "Updated"),
            rowsAffectedCount: 8,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_with_cross_join_left_join_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                  from c2 in ss.Set<Customer>().Where(c => c.City.StartsWith("S"))
                  join o in ss.Set<Order>().Where(o => o.OrderID < 10300)
                      on c.CustomerID equals o.CustomerID into grouping
                  from o in grouping.DefaultIfEmpty()
                  select new
                  {
                      c,
                      c2,
                      o
                  },
            e => e.c,
            s => s.SetProperty(c => c.c.ContactName, "Updated"),
            rowsAffectedCount: 8,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_with_cross_join_cross_apply_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                  from c2 in ss.Set<Customer>().Where(c => c.City.StartsWith("S"))
                  from o in ss.Set<Order>().Where(o => o.OrderID < 10300 && o.OrderDate.Value.Year < c.ContactName.Length)
                  select new { c, o },
            e => e.c,
            s => s.SetProperty(c => c.c.ContactName, "Updated"),
            rowsAffectedCount: 0,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_with_cross_join_outer_apply_set_constant(bool async)
        => AssertUpdate(
            async,
            ss => from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                  from c2 in ss.Set<Customer>().Where(c => c.City.StartsWith("S"))
                  from o in ss.Set<Order>().Where(o => o.OrderID < 10300 && o.OrderDate.Value.Year < c.ContactName.Length).DefaultIfEmpty()
                  select new
                  {
                      c,
                      c2,
                      o
                  },
            e => e.c,
            s => s.SetProperty(c => c.c.ContactName, "Updated"),
            rowsAffectedCount: 8,
            (b, a) => Assert.All(a, c => Assert.Equal("Updated", c.ContactName)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_FromSql_set_constant(bool async)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            () => Fixture.CreateContext(),
            (facade, transaction) => Fixture.UseTransaction(facade, transaction),
            async context =>
            {
                var queryable = context.Set<Customer>().FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        @"SELECT [Region], [PostalCode], [Phone], [Fax], [CustomerID], [Country], [ContactTitle], [ContactName], [CompanyName], [City], [Address]
FROM [Customers]
WHERE [CustomerID] LIKE 'A%'"));

                if (async)
                {
                    await queryable.ExecuteUpdateAsync(s => s.SetProperty(c => c.ContactName, "Updated"));
                }
                else
                {
                    queryable.ExecuteUpdate(s => s.SetProperty(c => c.ContactName, "Updated"));
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_SelectMany_subquery_set_null(bool async)
        => AssertUpdate(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                .SelectMany(c => c.Orders.Where(o => o.OrderDate.Value.Year == 1997)),
            e => e,
            s => s.SetProperty(c => c.OrderDate, (DateTime?)null),
            rowsAffectedCount: 35,
            (b, a) => Assert.All(a, c => Assert.Null(c.OrderDate)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_Join_set_property_from_joined_single_result_table(bool async)
        => AssertUpdate(
            async,
            ss => from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                  select new { c, LastOrder = c.Orders.OrderByDescending(o => o.OrderDate).FirstOrDefault() },
            e => e.c,
            s => s.SetProperty(c => c.c.City, c => c.LastOrder.OrderDate.Value.Year.ToString()),
            rowsAffectedCount: 8,
            (b, a) => Assert.All(
                a, c =>
                {
                    if (c.CustomerID == "FISSA")
                    {
                        Assert.Null(c.City);
                    }
                    else
                    {
                        Assert.NotNull(c.City);
                    }
                }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_Join_set_property_from_joined_table(bool async)
        => AssertUpdate(
            async,
            ss => from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                  from c2 in ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI")
                  select new { c, c2 },
            e => e.c,
            s => s.SetProperty(c => c.c.City, c => c.c2.City),
            rowsAffectedCount: 8,
            (b, a) => Assert.All(a, c => Assert.NotNull(c.City)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_Where_Join_set_property_from_joined_single_result_scalar(bool async)
        => AssertUpdate(
            async,
            ss => from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                  select new { c, LastOrderDate = c.Orders.OrderByDescending(o => o.OrderDate).FirstOrDefault().OrderDate.Value.Year },
            e => e.c,
            s => s.SetProperty(c => c.c.City, c => c.LastOrderDate.ToString()),
            rowsAffectedCount: 8,
            (b, a) => Assert.All(
                a, c =>
                {
                    if (c.CustomerID == "FISSA")
                    {
                        Assert.Null(c.City);
                    }
                    else
                    {
                        Assert.NotNull(c.City);
                    }
                }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Update_with_two_inner_joins(bool async)
        => AssertUpdate(
            async,
            ss => ss
                .Set<OrderDetail>()
                .Where(od => od.Product.Discontinued && od.Order.OrderDate > new DateTime(1990, 1, 1)),
            e => e,
            s => s.SetProperty(od => od.Quantity, 1),
            rowsAffectedCount: 228,
            (b, a) => Assert.All(a, od => Assert.Equal(1, od.Quantity)));

    protected string NormalizeDelimitersInRawString(string sql)
        => Fixture.TestStore.NormalizeDelimitersInRawString(sql);

    protected virtual void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
