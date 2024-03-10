// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

// ReSharper disable FormatStringProblem
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToConstant.Local
// ReSharper disable AccessToDisposedClosure
namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class SqlQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : NorthwindQueryRelationalFixture<NoopModelCustomizer>, new()
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly string _eol = Environment.NewLine;

    protected SqlQueryTestBase(TFixture fixture)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Bad_data_error_handling_invalid_cast_key(bool async)
    {
        using var context = CreateContext();
        var query = context.Database.SqlQueryRaw<UnmappedProduct>(
            NormalizeDelimitersInRawString(
                @"SELECT [ProductName] AS [ProductID], [ProductID] AS [ProductName], [SupplierID], [UnitPrice], [UnitsInStock], [Discontinued]
                      FROM [Products]"));

        Assert.Equal(
            CoreStrings.ErrorMaterializingPropertyInvalidCast("UnmappedProduct", "ProductID", typeof(int), typeof(string)),
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToList())).Message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Bad_data_error_handling_invalid_cast(bool async)
    {
        using var context = CreateContext();
        var query = context.Database.SqlQueryRaw<UnmappedProduct>(
            NormalizeDelimitersInRawString(
                @"SELECT [ProductID], [ProductName] AS [UnitPrice], [ProductName], [SupplierID], [UnitsInStock], [Discontinued]
                      FROM [Products]"));

        Assert.Equal(
            CoreStrings.ErrorMaterializingPropertyInvalidCast("UnmappedProduct", "UnitPrice", typeof(decimal?), typeof(string)),
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToList())).Message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Bad_data_error_handling_invalid_cast_projection(bool async)
    {
        using var context = CreateContext();
        var query = context.Database.SqlQueryRaw<UnmappedProduct>(
                NormalizeDelimitersInRawString(
                    @"SELECT [ProductID], [ProductName] AS [UnitPrice], [ProductName], [UnitsInStock], [Discontinued]
                      FROM [Products]"))
            .Select(p => p.UnitPrice);

        Assert.Equal(
            RelationalStrings.ErrorMaterializingValueInvalidCast(typeof(decimal?), typeof(string)),
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToList())).Message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Bad_data_error_handling_invalid_cast_no_tracking(bool async)
    {
        using var context = CreateContext();
        var query = context.Database.SqlQueryRaw<UnmappedProduct>(
            NormalizeDelimitersInRawString(
                @"SELECT [ProductName] AS [ProductID], [ProductID] AS [ProductName], [SupplierID], [UnitPrice], [UnitsInStock], [Discontinued]
                    FROM [Products]")).AsNoTracking();

        Assert.Equal(
            CoreStrings.ErrorMaterializingPropertyInvalidCast("UnmappedProduct", "ProductID", typeof(int), typeof(string)),
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToList())).Message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Bad_data_error_handling_null(bool async)
    {
        using var context = CreateContext();
        var query = context.Database.SqlQueryRaw<UnmappedProduct>(
            NormalizeDelimitersInRawString(
                @"SELECT [ProductID], [ProductName], [SupplierID], [UnitPrice], [UnitsInStock], NULL AS [Discontinued]
                FROM [Products]"));

        Assert.Equal(
            RelationalStrings.ErrorMaterializingPropertyNullReference("UnmappedProduct", "Discontinued", typeof(bool)),
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToList())).Message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Bad_data_error_handling_null_projection(bool async)
    {
        using var context = CreateContext();
        var query = context.Database.SqlQueryRaw<UnmappedProduct>(
                NormalizeDelimitersInRawString(
                    @"SELECT [ProductID], [ProductName], [SupplierID], [UnitPrice], [UnitsInStock], NULL AS [Discontinued]
                          FROM [Products]"))
            .Select(p => p.Discontinued);

        Assert.Equal(
            RelationalStrings.ErrorMaterializingValueNullReference(typeof(bool)),
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToList())).Message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Bad_data_error_handling_null_no_tracking(bool async)
    {
        using var context = CreateContext();
        var query = context.Database.SqlQueryRaw<UnmappedProduct>(
            NormalizeDelimitersInRawString(
                @"SELECT [ProductID], [ProductName], [SupplierID], [UnitPrice], [UnitsInStock], NULL AS [Discontinued]
                          FROM [Products]")).AsNoTracking();

        Assert.Equal(
            RelationalStrings.ErrorMaterializingPropertyNullReference("UnmappedProduct", "Discontinued", typeof(bool)),
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToList())).Message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQueryRaw_queryable_simple(bool async)
        => AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedCustomer>
                (NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [ContactName] LIKE '%z%'")),
            ss => ss.Set<Customer>().Where(x => x.ContactName.Contains("z")).Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQueryRaw_queryable_simple_mapped_type(bool async)
        => AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<CustomerQuery>
                (NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [ContactName] LIKE '%z%'")),
            ss => ss.Set<CustomerQuery>().Where(x => x.ContactName.Contains("z")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQueryRaw_queryable_simple_columns_out_of_order(bool async)
        => AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedCustomer>(
                NormalizeDelimitersInRawString(
                    "SELECT [Region], [PostalCode], [Phone], [Fax], [CustomerID], [Country], [ContactTitle], [ContactName], [CompanyName], [City], [Address] FROM [Customers]")),
            ss => ss.Set<Customer>().Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQueryRaw_queryable_simple_columns_out_of_order_and_extra_columns(bool async)
        => AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedCustomer>(
                NormalizeDelimitersInRawString(
                    "SELECT [Region], [PostalCode], [PostalCode] AS [Foo], [Phone], [Fax], [CustomerID], [Country], [ContactTitle], [ContactName], [CompanyName], [City], [Address] FROM [Customers]")),
            ss => ss.Set<Customer>().Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQueryRaw_queryable_simple_columns_out_of_order_and_not_enough_columns_throws(bool async)
    {
        using var context = CreateContext();
        var query = context.Database.SqlQueryRaw<UnmappedCustomer>(
            NormalizeDelimitersInRawString(
                "SELECT [PostalCode], [Phone], [Fax], [CustomerID], [Country], [ContactTitle], [ContactName], [CompanyName], [City], [Address] FROM [Customers]"));

        Assert.Equal(
            RelationalStrings.FromSqlMissingColumn("Region"),
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToList())).Message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQueryRaw_queryable_composed(bool async)
        => AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedCustomer>(
                    NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                .Where(c => c.ContactName.Contains("z")),
            ss => ss.Set<Customer>().Where(c => c.ContactName.Contains("z")).Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQueryRaw_queryable_composed_after_removing_whitespaces(bool async)
        => AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedCustomer>(
                    NormalizeDelimitersInRawString(
                        _eol + "    " + _eol + _eol + _eol + "SELECT" + _eol + "* FROM [Customers]"))
                .Where(c => c.ContactName.Contains("z")),
            ss => ss.Set<Customer>().Where(c => c.ContactName.Contains("z")).Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQueryRaw_queryable_composed_compiled(bool async)
    {
        if (async)
        {
            var query = EF.CompileAsyncQuery(
                (NorthwindContext context) => context.Database.SqlQueryRaw<UnmappedCustomer>(
                        NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                    .Where(c => c.ContactName.Contains("z")));

            using (var context = CreateContext())
            {
                var actual = await query(context).ToListAsync();

                Assert.Equal(14, actual.Count);
            }
        }
        else
        {
            var query = EF.CompileQuery(
                (NorthwindContext context) => context.Database.SqlQueryRaw<UnmappedCustomer>(
                        NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                    .Where(c => c.ContactName.Contains("z")));

            using (var context = CreateContext())
            {
                var actual = query(context).ToArray();

                Assert.Equal(14, actual.Length);
            }
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQueryRaw_queryable_composed_compiled_with_parameter(bool async)
    {
        if (async)
        {
            var query = EF.CompileAsyncQuery(
                (NorthwindContext context) => context.Database.SqlQueryRaw<UnmappedCustomer>(
                        NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = {0}"), "CONSH")
                    .Where(c => c.ContactName.Contains("z")));

            using (var context = CreateContext())
            {
                var actual = await query(context).ToListAsync();

                Assert.Single(actual);
            }
        }
        else
        {
            var query = EF.CompileQuery(
                (NorthwindContext context) => context.Database.SqlQueryRaw<UnmappedCustomer>(
                        NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = {0}"), "CONSH")
                    .Where(c => c.ContactName.Contains("z")));

            using (var context = CreateContext())
            {
                var actual = query(context).ToArray();

                Assert.Single(actual);
            }
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQueryRaw_queryable_composed_compiled_with_DbParameter(bool async)
    {
        if (async)
        {
            var query = EF.CompileAsyncQuery(
                (NorthwindContext context) => context.Database.SqlQueryRaw<UnmappedCustomer>(
                        NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = @customer"),
                        CreateDbParameter("customer", "CONSH"))
                    .Where(c => c.ContactName.Contains("z")));

            using (var context = CreateContext())
            {
                var actual = await query(context).ToListAsync();

                Assert.Single(actual);
            }
        }
        else
        {
            var query = EF.CompileQuery(
                (NorthwindContext context) => context.Database.SqlQueryRaw<UnmappedCustomer>(
                        NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = @customer"),
                        CreateDbParameter("customer", "CONSH"))
                    .Where(c => c.ContactName.Contains("z")));

            using (var context = CreateContext())
            {
                var actual = query(context).ToArray();

                Assert.Single(actual);
            }
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQueryRaw_queryable_composed_compiled_with_nameless_DbParameter(bool async)
    {
        if (async)
        {
            var query = EF.CompileAsyncQuery(
                (NorthwindContext context) => context.Database.SqlQueryRaw<UnmappedCustomer>(
                        NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = {0}"),
                        CreateDbParameter(null, "CONSH"))
                    .Where(c => c.ContactName.Contains("z")));

            using (var context = CreateContext())
            {
                var actual = await query(context).ToListAsync();

                Assert.Single(actual);
            }
        }
        else
        {
            var query = EF.CompileQuery(
                (NorthwindContext context) => context.Database.SqlQueryRaw<UnmappedCustomer>(
                        NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = {0}"),
                        CreateDbParameter(null, "CONSH"))
                    .Where(c => c.ContactName.Contains("z")));

            using (var context = CreateContext())
            {
                var actual = query(context).ToArray();

                Assert.Single(actual);
            }
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQueryRaw_composed_contains(bool async)
    {
        var context = Fixture.CreateContext();
        return AssertQuery(
            async,
            ss => from c in context.Database.SqlQueryRaw<UnmappedCustomer>(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                  where context.Database.SqlQueryRaw<UnmappedOrder>(NormalizeDelimitersInRawString("SELECT * FROM [Orders]"))
                      .Select(o => o.CustomerID)
                      .Contains(c.CustomerID)
                  select c,
            ss => from c in ss.Set<Customer>()
                  where ss.Set<Order>()
                      .Select(o => o.CustomerID)
                      .Contains(c.CustomerID)
                  select UnmappedCustomer.FromCustomer(c),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQueryRaw_queryable_multiple_composed(bool async)
    {
        var context = Fixture.CreateContext();

        return AssertQuery(
            async,
            _ => from c in context.Database
                     .SqlQueryRaw<UnmappedCustomer>(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                 from o in context.Database
                     .SqlQueryRaw<UnmappedOrder>(NormalizeDelimitersInRawString("SELECT * FROM [Orders]"))
                 where c.CustomerID == o.CustomerID
                 select new { c, o },
            ss => from c in ss.Set<Customer>()
                  from o in ss.Set<Order>()
                  where c.CustomerID == o.CustomerID
                  select new { c = UnmappedCustomer.FromCustomer(c), o = UnmappedOrder.FromOrder(o) },
            elementSorter: e => (e.c.CustomerID, e.o.OrderID),
            elementAsserter: (l, r) =>
            {
                AssertUnmappedCustomers(l.c, r.c);
                AssertUnmappedOrders(l.o, r.o);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQueryRaw_queryable_multiple_composed_with_closure_parameters(bool async)
    {
        var startDate = new DateTime(1997, 1, 1);
        var endDate = new DateTime(1998, 1, 1);
        var context = Fixture.CreateContext();

        return AssertQuery(
            async,
            _ => from c in context.Database.SqlQueryRaw<UnmappedCustomer>(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                 from o in context.Database.SqlQueryRaw<UnmappedOrder>(
                     NormalizeDelimitersInRawString("SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {0} AND {1}"),
                     startDate,
                     endDate)
                 where c.CustomerID == o.CustomerID
                 select new { c, o },
            ss => from c in ss.Set<Customer>()
                  from o in ss.Set<Order>().Where(x => x.OrderDate >= startDate && x.OrderDate <= endDate)
                  where c.CustomerID == o.CustomerID
                  select new { c = UnmappedCustomer.FromCustomer(c), o = UnmappedOrder.FromOrder(o) },
            elementSorter: e => (e.c.CustomerID, e.o.OrderID),
            elementAsserter: (l, r) =>
            {
                AssertUnmappedCustomers(l.c, r.c);
                AssertUnmappedOrders(l.o, r.o);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQueryRaw_queryable_multiple_composed_with_parameters_and_closure_parameters(bool async)
    {
        var city = "London";
        var startDate = new DateTime(1997, 1, 1);
        var endDate = new DateTime(1998, 1, 1);
        var context = Fixture.CreateContext();

        await AssertQuery(
            async,
            _ => from c in context.Database.SqlQueryRaw<UnmappedCustomer>(
                     NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), city)
                 from o in context.Database.SqlQueryRaw<UnmappedOrder>(
                     NormalizeDelimitersInRawString("SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {0} AND {1}"),
                     startDate,
                     endDate)
                 where c.CustomerID == o.CustomerID
                 select new { c, o },
            ss => from c in ss.Set<Customer>().Where(x => x.City == city)
                  from o in ss.Set<Order>().Where(x => x.OrderDate >= startDate && x.OrderDate <= endDate)
                  where c.CustomerID == o.CustomerID
                  select new { c = UnmappedCustomer.FromCustomer(c), o = UnmappedOrder.FromOrder(o) },
            elementSorter: e => (e.c.CustomerID, e.o.OrderID),
            elementAsserter: (l, r) =>
            {
                AssertUnmappedCustomers(l.c, r.c);
                AssertUnmappedOrders(l.o, r.o);
            });

        city = "Berlin";
        startDate = new DateTime(1998, 4, 1);
        endDate = new DateTime(1998, 5, 1);

        await AssertQuery(
            async,
            _ => from c in context.Database.SqlQueryRaw<UnmappedCustomer>(
                     NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), city)
                 from o in context.Database.SqlQueryRaw<UnmappedOrder>(
                     NormalizeDelimitersInRawString("SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {0} AND {1}"),
                     startDate,
                     endDate)
                 where c.CustomerID == o.CustomerID
                 select new { c, o },
            ss => from c in ss.Set<Customer>().Where(x => x.City == city)
                  from o in ss.Set<Order>().Where(x => x.OrderDate >= startDate && x.OrderDate <= endDate)
                  where c.CustomerID == o.CustomerID
                  select new { c = UnmappedCustomer.FromCustomer(c), o = UnmappedOrder.FromOrder(o) },
            elementSorter: e => (e.c.CustomerID, e.o.OrderID),
            elementAsserter: (l, r) =>
            {
                AssertUnmappedCustomers(l.c, r.c);
                AssertUnmappedOrders(l.o, r.o);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQueryRaw_queryable_multiple_line_query(bool async)
        => AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedCustomer>(
                NormalizeDelimitersInRawString(
                    @"SELECT *
FROM [Customers]
WHERE [City] = 'London'")),
            ss => ss.Set<Customer>().Where(x => x.City == "London").Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQueryRaw_queryable_composed_multiple_line_query(bool async)
        => AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedCustomer>(
                    NormalizeDelimitersInRawString(
                        @"SELECT *
FROM [Customers]"))
                .Where(c => c.City == "London"),
            ss => ss.Set<Customer>().Where(x => x.City == "London").Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQueryRaw_queryable_with_parameters(bool async)
    {
        var city = "London";
        var contactTitle = "Sales Representative";

        return AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedCustomer>(
                NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0} AND [ContactTitle] = {1}"), city,
                contactTitle),
            ss => ss.Set<Customer>().Where(x => x.City == city && x.ContactTitle == contactTitle)
                .Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQueryRaw_queryable_with_parameters_inline(bool async)
        => AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedCustomer>(
                NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0} AND [ContactTitle] = {1}"), "London",
                "Sales Representative"),
            ss => ss.Set<Customer>().Where(x => x.City == "London" && x.ContactTitle == "Sales Representative")
                .Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQuery_queryable_with_parameters_interpolated(bool async)
    {
        var city = "London";
        var contactTitle = "Sales Representative";

        return AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQuery<UnmappedCustomer>(
                NormalizeDelimitersInInterpolatedString(
                    $"SELECT * FROM [Customers] WHERE [City] = {city} AND [ContactTitle] = {contactTitle}")),
            ss => ss.Set<Customer>().Where(x => x.City == city && x.ContactTitle == contactTitle)
                .Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQuery_queryable_with_parameters_inline_interpolated(bool async)
        => AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQuery<UnmappedCustomer>(
                NormalizeDelimitersInInterpolatedString(
                    $"SELECT * FROM [Customers] WHERE [City] = {"London"} AND [ContactTitle] = {"Sales Representative"}")),
            ss => ss.Set<Customer>().Where(x => x.City == "London" && x.ContactTitle == "Sales Representative")
                .Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQuery_queryable_multiple_composed_with_parameters_and_closure_parameters_interpolated(
        bool async)
    {
        var city = "London";
        var startDate = new DateTime(1997, 1, 1);
        var endDate = new DateTime(1998, 1, 1);
        var context = Fixture.CreateContext();

        await AssertQuery(
            async,
            _ => from c in context.Database.SqlQueryRaw<UnmappedCustomer>(
                     NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), city)
                 from o in context.Database.SqlQuery<UnmappedOrder>(
                     NormalizeDelimitersInInterpolatedString(
                         $"SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {startDate} AND {endDate}"))
                 where c.CustomerID == o.CustomerID
                 select new { c, o },
            ss => from c in ss.Set<Customer>().Where(x => x.City == city)
                  from o in ss.Set<Order>().Where(x => x.OrderDate >= startDate && x.OrderDate <= endDate)
                  where c.CustomerID == o.CustomerID
                  select new { c = UnmappedCustomer.FromCustomer(c), o = UnmappedOrder.FromOrder(o) },
            elementSorter: e => (e.c.CustomerID, e.o.OrderID),
            elementAsserter: (l, r) =>
            {
                AssertUnmappedCustomers(l.c, r.c);
                AssertUnmappedOrders(l.o, r.o);
            });

        city = "Berlin";
        startDate = new DateTime(1998, 4, 1);
        endDate = new DateTime(1998, 5, 1);

        await AssertQuery(
            async,
            _ => from c in context.Database.SqlQueryRaw<UnmappedCustomer>(
                     NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), city)
                 from o in context.Database.SqlQuery<UnmappedOrder>(
                     NormalizeDelimitersInInterpolatedString(
                         $"SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {startDate} AND {endDate}"))
                 where c.CustomerID == o.CustomerID
                 select new { c, o },
            ss => from c in ss.Set<Customer>().Where(x => x.City == city)
                  from o in ss.Set<Order>().Where(x => x.OrderDate >= startDate && x.OrderDate <= endDate)
                  where c.CustomerID == o.CustomerID
                  select new { c = UnmappedCustomer.FromCustomer(c), o = UnmappedOrder.FromOrder(o) },
            elementSorter: e => (e.c.CustomerID, e.o.OrderID),
            elementAsserter: (l, r) =>
            {
                AssertUnmappedCustomers(l.c, r.c);
                AssertUnmappedOrders(l.o, r.o);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQueryRaw_queryable_with_null_parameter(bool async)
    {
        uint? reportsTo = null;

        return AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedEmployee>(
                NormalizeDelimitersInRawString(
                    // ReSharper disable once ExpressionIsAlwaysNull
                    "SELECT * FROM [Employees] WHERE [ReportsTo] = {0} OR ([ReportsTo] IS NULL AND {0} IS NULL)"), reportsTo),
            ss => ss.Set<Employee>().Where(x => x.ReportsTo == reportsTo).Select(e => UnmappedEmployee.FromEmployee(e)),
            elementSorter: e => e.EmployeeID,
            elementAsserter: AssertUnmappedEmployees);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task<string> SqlQueryRaw_queryable_with_parameters_and_closure(bool async)
    {
        var city = "London";
        var contactTitle = "Sales Representative";

        using var context = CreateContext();
        var query = context.Database.SqlQueryRaw<UnmappedCustomer>(
                NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), city)
            .Where(c => c.ContactTitle == contactTitle);
        var queryString = query.ToQueryString();

        var actual = async
            ? await query.ToArrayAsync()
            : query.ToArray();

        Assert.Equal(3, actual.Length);
        Assert.True(actual.All(c => c.City == "London"));
        Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));

        return queryString;
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQueryRaw_queryable_simple_cache_key_includes_query_string(bool async)
    {
        await AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedCustomer>(
                NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = 'London'")),
            ss => ss.Set<Customer>().Where(x => x.City == "London").Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);

        await AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedCustomer>(
                NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = 'Seattle'")),
            ss => ss.Set<Customer>().Where(x => x.City == "Seattle").Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQueryRaw_queryable_with_parameters_cache_key_includes_parameters(bool async)
    {
        var city = "London";
        var contactTitle = "Sales Representative";
        var sql = "SELECT * FROM [Customers] WHERE [City] = {0} AND [ContactTitle] = {1}";

        await AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedCustomer>(NormalizeDelimitersInRawString(sql), city, contactTitle),
            ss => ss.Set<Customer>().Where(x => x.City == city && x.ContactTitle == contactTitle)
                .Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);

        city = "Madrid";
        contactTitle = "Accounting Manager";

        await AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedCustomer>(NormalizeDelimitersInRawString(sql), city, contactTitle),
            ss => ss.Set<Customer>().Where(x => x.City == city && x.ContactTitle == contactTitle)
                .Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQueryRaw_queryable_simple_as_no_tracking_not_composed(bool async)
        => AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedCustomer>(
                    NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                .AsNoTracking(),
            ss => ss.Set<Customer>().Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQueryRaw_queryable_simple_projection_composed(bool async)
    {
        using var context = CreateContext();
        var boolMapping = (RelationalTypeMapping)context.GetService<ITypeMappingSource>().FindMapping(typeof(bool));
        var boolLiteral = boolMapping.GenerateSqlLiteral(true);

        await AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedProduct>(
                    NormalizeDelimitersInRawString(
                        @"SELECT *
FROM [Products]
WHERE [Discontinued] <> "
                        + boolLiteral
                        + @"
AND (([UnitsInStock] + [UnitsOnOrder]) < [ReorderLevel])"))
                .Select(p => p.ProductName),
            ss => ss.Set<Product>()
                .Where(x => x.Discontinued != true && (x.UnitsInStock + x.UnitsOnOrder) < x.ReorderLevel)
                .Select(x => x.ProductName));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQueryRaw_annotations_do_not_affect_successive_calls(bool async)
    {
        using var context = CreateContext();
        var query = context.Database.SqlQueryRaw<UnmappedCustomer>(
            NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [ContactName] LIKE '%z%'"));

        var actual = async
            ? await query.ToArrayAsync()
            : query.ToArray();

        Assert.Equal(14, actual.Length);

        query = context.Database.SqlQueryRaw<UnmappedCustomer>(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"));
        actual = async
            ? await query.ToArrayAsync()
            : query.ToArray();

        Assert.Equal(91, actual.Length);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQueryRaw_composed_with_predicate(bool async)
        => AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedCustomer>(
                    NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                .Where(c => c.ContactName.Substring(0, 1) == c.CompanyName.Substring(0, 1)),
            ss => ss.Set<Customer>().Where(c => c.ContactName.Substring(0, 1) == c.CompanyName.Substring(0, 1)).Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQueryRaw_composed_with_empty_predicate(bool async)
        => AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedCustomer>(
                    NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                .Where(c => c.ContactName == c.CompanyName),
            ss => ss.Set<Customer>().Where(c => c.ContactName == c.CompanyName).Select(e => UnmappedCustomer.FromCustomer(e)),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQueryRaw_with_dbParameter(bool async)
    {
        var parameter = CreateDbParameter("@city", "London");

        await AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedCustomer>(
                NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = @city"), parameter),
            ss => ss.Set<Customer>().Where(x => x.City == "London").Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQueryRaw_with_dbParameter_without_name_prefix(bool async)
    {
        var parameter = CreateDbParameter("city", "London");

        await AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedCustomer>(
                NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = @city"), parameter),
            ss => ss.Set<Customer>().Where(x => x.City == "London").Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQueryRaw_with_dbParameter_mixed(bool async)
    {
        var city = "London";
        var title = "Sales Representative";

        var titleParameter = CreateDbParameter("@title", title);

        await AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedCustomer>(
                NormalizeDelimitersInRawString(
                    "SELECT * FROM [Customers] WHERE [City] = {0} AND [ContactTitle] = @title"), city, titleParameter),
            ss => ss.Set<Customer>().Where(x => x.City == city && x.ContactTitle == title).Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);

        var cityParameter = CreateDbParameter("@city", city);

        await AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedCustomer>(
                NormalizeDelimitersInRawString(
                    "SELECT * FROM [Customers] WHERE [City] = @city AND [ContactTitle] = {1}"), cityParameter, title),
            ss => ss.Set<Customer>().Where(x => x.City == city && x.ContactTitle == title).Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQueryRaw_with_db_parameters_called_multiple_times(bool async)
    {
        using var context = CreateContext();
        var parameter = CreateDbParameter("@id", "ALFKI");

        var query = context.Database.SqlQueryRaw<UnmappedCustomer>(
            NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = @id"), parameter);

        // ReSharper disable PossibleMultipleEnumeration
        var result1 = async
            ? await query.ToArrayAsync()
            : query.ToArray();

        Assert.Single(result1);

        var result2 = async
            ? await query.ToArrayAsync()
            : query.ToArray();
        // ReSharper restore PossibleMultipleEnumeration

        Assert.Single(result2);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQuery_with_inlined_db_parameter(bool async)
    {
        var parameter = CreateDbParameter("@somename", "ALFKI");

        await AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQuery<UnmappedCustomer>(
                NormalizeDelimitersInInterpolatedString($"SELECT * FROM [Customers] WHERE [CustomerID] = {parameter}")),
            ss => ss.Set<Customer>().Where(x => x.CustomerID == "ALFKI").Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQuery_with_inlined_db_parameter_without_name_prefix(bool async)
    {
        var parameter = CreateDbParameter("somename", "ALFKI");

        await AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQuery<UnmappedCustomer>(
                NormalizeDelimitersInInterpolatedString($"SELECT * FROM [Customers] WHERE [CustomerID] = {parameter}")),
            ss => ss.Set<Customer>().Where(x => x.CustomerID == "ALFKI").Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQuery_parameterization_issue_12213(bool async)
    {
        using var context = CreateContext();
        var min = 10300;
        var max = 10400;

        var query1 = context.Database.SqlQuery<UnmappedOrder>(
                NormalizeDelimitersInInterpolatedString($"SELECT * FROM [Orders] WHERE [OrderID] >= {min}"))
            .Select(i => i.OrderID);

        var actual1 = async
            ? await query1.ToArrayAsync()
            : query1.ToArray();

        var query2 = context.Database.SqlQueryRaw<UnmappedOrder>(NormalizeDelimitersInRawString("SELECT * FROM [Orders]"))
            .Where(o => o.OrderID <= max && query1.Contains(o.OrderID))
            .Select(o => o.OrderID);

        var actual2 = async
            ? await query2.ToArrayAsync()
            : query2.ToArray();

        var query3 = context.Database.SqlQueryRaw<UnmappedOrder>(NormalizeDelimitersInRawString("SELECT * FROM [Orders]"))
            .Where(
                o => o.OrderID <= max
                    && context.Database.SqlQuery<UnmappedOrder>(
                            NormalizeDelimitersInInterpolatedString($"SELECT * FROM [Orders] WHERE [OrderID] >= {min}"))
                        .Select(i => i.OrderID)
                        .Contains(o.OrderID))
            .Select(o => o.OrderID);

        var actual3 = async
            ? await query3.ToArrayAsync()
            : query3.ToArray();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQueryRaw_does_not_parameterize_interpolated_string(bool async)
    {
        var tableName = "Orders";
        var max = 10250;

        await AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedOrder>(
                NormalizeDelimitersInRawString($"SELECT * FROM [{tableName}] WHERE [OrderID] < {{0}}"), max),
            ss => ss.Set<Order>().Where(x => x.OrderID < max).Select(e => UnmappedOrder.FromOrder(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedOrders);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQueryRaw_with_set_operation(bool async)
    {
        var context = Fixture.CreateContext();

        return AssertQuery(
            async,
            _ => context.Database.SqlQueryRaw<UnmappedCustomer>(
                    NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = 'London'"))
                .Concat(
                    context.Database.SqlQueryRaw<UnmappedCustomer>(
                        NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = 'Berlin'"))),
            ss => ss.Set<Customer>().Where(x => x.City == "London")
                .Concat(ss.Set<Customer>().Where(x => x.City == "Berlin")).Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Line_endings_after_Select(bool async)
        => AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedCustomer>(
                    NormalizeDelimitersInRawString("SELECT" + Environment.NewLine + "* FROM [Customers]"))
                .Where(e => e.City == "Seattle"),
            ss => ss.Set<Customer>().Where(x => x.City == "Seattle").Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQueryRaw_queryable_simple_projection_not_composed(bool async)
        => AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedCustomer>(
                    NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                .Select(c => new { c.CustomerID, c.City })
                .AsNoTracking(),
            ss => ss.Set<Customer>().Select(c => new { c.CustomerID, c.City }),
            elementSorter: e => e.CustomerID,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.CustomerID, a.CustomerID);
                Assert.Equal(e.City, a.City);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQueryRaw_in_subquery_with_dbParameter(bool async)
    {
        var context = Fixture.CreateContext();

        return AssertQuery(
            async,
            _ => context.Database.SqlQueryRaw<UnmappedOrder>(
                NormalizeDelimitersInRawString("SELECT * FROM [Orders]")).Where(
                o => context.Database.SqlQueryRaw<UnmappedCustomer>(
                        NormalizeDelimitersInRawString(@"SELECT * FROM [Customers] WHERE [City] = @city"),
                        // ReSharper disable once FormatStringProblem
                        CreateDbParameter("@city", "London"))
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)),
            ss => ss.Set<Order>().Select(e => UnmappedOrder.FromOrder(e)).Where(
                o => ss.Set<Customer>().Select(e => UnmappedCustomer.FromCustomer(e)).Where(x => x.City == "London")
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)),
            elementSorter: e => e.OrderID,
            elementAsserter: AssertUnmappedOrders);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQueryRaw_in_subquery_with_positional_dbParameter_without_name(bool async)
    {
        var context = Fixture.CreateContext();

        return AssertQuery(
            async,
            _ => context.Database.SqlQueryRaw<UnmappedOrder>(
                NormalizeDelimitersInRawString("SELECT * FROM [Orders]")).Where(
                o => context.Database.SqlQueryRaw<UnmappedCustomer>(
                        NormalizeDelimitersInRawString(@"SELECT * FROM [Customers] WHERE [City] = {0}"),
                        // ReSharper disable once FormatStringProblem
                        CreateDbParameter(null, "London"))
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)),
            ss => ss.Set<Order>().Select(e => UnmappedOrder.FromOrder(e)).Where(
                o => ss.Set<Customer>().Select(e => UnmappedCustomer.FromCustomer(e)).Where(x => x.City == "London")
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)),
            elementSorter: e => e.OrderID,
            elementAsserter: AssertUnmappedOrders);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQueryRaw_in_subquery_with_positional_dbParameter_with_name(bool async)
    {
        var context = Fixture.CreateContext();

        return AssertQuery(
            async,
            _ => context.Database.SqlQueryRaw<UnmappedOrder>(
                NormalizeDelimitersInRawString("SELECT * FROM [Orders]")).Where(
                o => context.Database.SqlQueryRaw<UnmappedCustomer>(
                        NormalizeDelimitersInRawString(@"SELECT * FROM [Customers] WHERE [City] = {0}"),
                        // ReSharper disable once FormatStringProblem
                        CreateDbParameter("@city", "London"))
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)),
            ss => ss.Set<Order>().Select(e => UnmappedOrder.FromOrder(e)).Where(
                o => ss.Set<Customer>().Select(e => UnmappedCustomer.FromCustomer(e)).Where(x => x.City == "London")
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)),
            elementSorter: e => e.OrderID,
            elementAsserter: AssertUnmappedOrders);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQueryRaw_with_dbParameter_mixed_in_subquery(bool async)
    {
        const string city = "London";
        const string title = "Sales Representative";
        var context = Fixture.CreateContext();

        await AssertQuery(
            async,
            _ => context.Database.SqlQueryRaw<UnmappedOrder>(
                NormalizeDelimitersInRawString("SELECT * FROM [Orders]")).Where(
                o => context.Database.SqlQueryRaw<UnmappedCustomer>(
                        NormalizeDelimitersInRawString(@"SELECT * FROM [Customers] WHERE [City] = {0} AND [ContactTitle] = @title"),
                        city,
                        // ReSharper disable once FormatStringProblem
                        CreateDbParameter("@title", title))
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)),
            ss => ss.Set<Order>().Select(e => UnmappedOrder.FromOrder(e)).Where(
                o => ss.Set<Customer>().Select(e => UnmappedCustomer.FromCustomer(e)).Where(x => x.City == city && x.ContactTitle == title)
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)),
            elementSorter: e => e.OrderID,
            elementAsserter: AssertUnmappedOrders);

        await AssertQuery(
            async,
            _ => context.Database.SqlQueryRaw<UnmappedOrder>(
                NormalizeDelimitersInRawString("SELECT * FROM [Orders]")).Where(
                o => context.Database.SqlQueryRaw<UnmappedCustomer>(
                        NormalizeDelimitersInRawString(@"SELECT * FROM [Customers] WHERE [City] = @city AND [ContactTitle] = {1}"),
                        // ReSharper disable once FormatStringProblem
                        CreateDbParameter("@city", city),
                        title)
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)),
            ss => ss.Set<Order>().Select(e => UnmappedOrder.FromOrder(e)).Where(
                o => ss.Set<Customer>().Select(e => UnmappedCustomer.FromCustomer(e)).Where(x => x.City == city && x.ContactTitle == title)
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)),
            elementSorter: e => e.OrderID,
            elementAsserter: AssertUnmappedOrders);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SqlQueryRaw_composed_with_common_table_expression(bool async)
        => AssertQuery(
            async,
            _ => Fixture.CreateContext().Database.SqlQueryRaw<UnmappedCustomer>(
                    NormalizeDelimitersInRawString(
                        """
WITH [Customers2] AS (
    SELECT * FROM [Customers]
)
SELECT * FROM [Customers2]
"""))
                .Where(c => c.ContactName.Contains("z")),
            ss => ss.Set<Customer>().Where(c => c.ContactName.Contains("z")).Select(e => UnmappedCustomer.FromCustomer(e)),
            elementSorter: e => e.CustomerID,
            elementAsserter: AssertUnmappedCustomers);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Multiple_occurrences_of_SqlQuery_with_db_parameter_adds_parameter_only_once(bool async)
    {
        using var context = CreateContext();
        var city = "Seattle";
        var qqlQuery = context.Database.SqlQueryRaw<UnmappedCustomer>(
            NormalizeDelimitersInRawString(@"SELECT * FROM [Customers] WHERE [City] = {0}"),
            CreateDbParameter("city", city));

        var query = qqlQuery.Intersect(qqlQuery);

        var actual = async
            ? await query.ToArrayAsync()
            : query.ToArray();

        Assert.Single(actual);
    }

    [ConditionalFact]
    public virtual void Ad_hoc_type_with_reference_navigation_throws()
    {
        using var context = CreateContext();

        Assert.Equal(
            CoreStrings.NavigationNotAddedAdHoc("Post", "Blog", "Blog"),
            Assert.Throws<InvalidOperationException>(
                () => context.Database.SqlQueryRaw<Post>(NormalizeDelimitersInRawString(@"SELECT * FROM [Posts]"))).Message);
    }

    [ConditionalFact] // Issue #30056
    public virtual void Ad_hoc_type_with_collection_navigation_throws()
    {
        using var context = CreateContext();

        Assert.Equal(
            CoreStrings.NavigationNotAddedAdHoc("Blog", "Posts", "List<Post>"),
            Assert.Throws<InvalidOperationException>(
                () => context.Database.SqlQueryRaw<Blog>(NormalizeDelimitersInRawString(@"SELECT * FROM [Blogs]"))).Message);
    }

    [ConditionalFact]
    public virtual void Ad_hoc_type_with_unmapped_property_throws()
    {
        using var context = CreateContext();

        Assert.Equal(
            CoreStrings.PropertyNotAddedAdHoc("Person", "Contact", "ContactInfo"),
            Assert.Throws<InvalidOperationException>(
                () => context.Database.SqlQueryRaw<Person>(NormalizeDelimitersInRawString(@"SELECT * FROM [People]"))).Message);
    }

    protected class Blog
    {
        public int Id { get; set; }
        public List<Post> Posts { get; set; }
    }

    protected class Post
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }

    protected class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ContactInfo Contact { get; set; }
    }

    protected readonly struct ContactInfo
    {
        public string Address { get; init; }
        public string Phone { get; init; }
    }

    private static void AssertUnmappedCustomers(UnmappedCustomer l, UnmappedCustomer r)
    {
        Assert.Equal(l.CustomerID, r.CustomerID);
        Assert.Equal(l.CompanyName, r.CompanyName);
        Assert.Equal(l.ContactName, r.ContactName);
        Assert.Equal(l.ContactTitle, r.ContactTitle);
        Assert.Equal(l.City, r.City);
        Assert.Equal(l.Region, r.Region);
        Assert.Equal(l.Zip, r.Zip);
        Assert.Equal(l.Country, r.Country);
        Assert.Equal(l.Phone, r.Phone);
        Assert.Equal(l.Fax, r.Fax);
    }

    private static void AssertUnmappedOrders(UnmappedOrder l, UnmappedOrder r)
    {
        Assert.Equal(l.OrderID, r.OrderID);
        Assert.Equal(l.CustomerID, r.CustomerID);
        Assert.Equal(l.EmployeeID, r.EmployeeID);
        Assert.Equal(l.OrderDate, r.OrderDate);
        Assert.Equal(l.RequiredDate, r.RequiredDate);
        Assert.Equal(l.ShippedDate, r.ShippedDate);
        Assert.Equal(l.ShipVia, r.ShipVia);
        Assert.Equal(l.Freight, r.Freight);
        Assert.Equal(l.ShipName, r.ShipName);
        Assert.Equal(l.ShipAddress, r.ShipAddress);
        Assert.Equal(l.ShipRegion, r.ShipRegion);
        Assert.Equal(l.ShipPostalCode, r.ShipPostalCode);
        Assert.Equal(l.ShipCountry, r.ShipCountry);
    }

    private static void AssertUnmappedEmployees(UnmappedEmployee l, UnmappedEmployee r)
    {
        Assert.Equal(l.EmployeeID, r.EmployeeID);
        Assert.Equal(l.LastName, r.LastName);
        Assert.Equal(l.FirstName, r.FirstName);
        Assert.Equal(l.Title, r.Title);
        Assert.Equal(l.TitleOfCourtesy, r.TitleOfCourtesy);
        Assert.Equal(l.BirthDate, r.BirthDate);
        Assert.Equal(l.HireDate, r.HireDate);
        Assert.Equal(l.Address, r.Address);
        Assert.Equal(l.City, r.City);
        Assert.Equal(l.Region, r.Region);
        Assert.Equal(l.PostalCode, r.PostalCode);
        Assert.Equal(l.Country, r.Country);
        Assert.Equal(l.HomePhone, r.HomePhone);
        Assert.Equal(l.Extension, r.Extension);
        Assert.Equal(l.Photo, r.Photo);
        Assert.Equal(l.Notes, r.Notes);
        Assert.Equal(l.ReportsTo, r.ReportsTo);
        Assert.Equal(l.PhotoPath, r.PhotoPath);
    }

    protected string NormalizeDelimitersInRawString(string sql)
        => Fixture.TestStore.NormalizeDelimitersInRawString(sql);

    protected FormattableString NormalizeDelimitersInInterpolatedString(FormattableString sql)
        => Fixture.TestStore.NormalizeDelimitersInInterpolatedString(sql);

    protected abstract DbParameter CreateDbParameter(string name, object value);

    protected NorthwindContext CreateContext()
        => Fixture.CreateContext();
}
