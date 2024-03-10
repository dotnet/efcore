// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

// ReSharper disable FormatStringProblem
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToConstant.Local
// ReSharper disable AccessToDisposedClosure
namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class FromSqlQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : NorthwindQueryRelationalFixture<NoopModelCustomizer>, new()
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly string _eol = Environment.NewLine;

    protected FromSqlQueryTestBase(TFixture fixture)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Bad_data_error_handling_invalid_cast_key(bool async)
    {
        using var context = CreateContext();
        var query = context.Set<Product>().FromSqlRaw(
            NormalizeDelimitersInRawString(
                @"SELECT [ProductName] AS [ProductID], [ProductID] AS [ProductName], [SupplierID], [UnitPrice], [UnitsInStock], [Discontinued]
                      FROM [Products]"));

        Assert.Equal(
            CoreStrings.ErrorMaterializingPropertyInvalidCast("Product", "ProductID", typeof(int), typeof(string)),
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToList())).Message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Bad_data_error_handling_invalid_cast(bool async)
    {
        using var context = CreateContext();
        var query = context.Set<Product>().FromSqlRaw(
            NormalizeDelimitersInRawString(
                @"SELECT [ProductID], [SupplierID] AS [UnitPrice], [ProductName], [SupplierID], [UnitsInStock], [Discontinued]
                      FROM [Products]"));

        Assert.Equal(
            CoreStrings.ErrorMaterializingPropertyInvalidCast("Product", "UnitPrice", typeof(decimal?), typeof(int)),
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToList())).Message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Bad_data_error_handling_invalid_cast_projection(bool async)
    {
        using var context = CreateContext();
        var query = context.Set<Product>().FromSqlRaw(
                NormalizeDelimitersInRawString(
                    @"SELECT [ProductID], [SupplierID] AS [UnitPrice], [ProductName], [UnitsInStock], [Discontinued]
                      FROM [Products]"))
            .Select(p => p.UnitPrice);

        Assert.Equal(
            RelationalStrings.ErrorMaterializingValueInvalidCast(typeof(decimal?), typeof(int)),
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToList())).Message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Bad_data_error_handling_invalid_cast_no_tracking(bool async)
    {
        using var context = CreateContext();
        var query = context.Set<Product>()
            .FromSqlRaw(
                NormalizeDelimitersInRawString(
                    @"SELECT [ProductName] AS [ProductID], [ProductID] AS [ProductName], [SupplierID], [UnitPrice], [UnitsInStock], [Discontinued]
                    FROM [Products]")).AsNoTracking();

        Assert.Equal(
            CoreStrings.ErrorMaterializingPropertyInvalidCast("Product", "ProductID", typeof(int), typeof(string)),
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToList())).Message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Bad_data_error_handling_null(bool async)
    {
        using var context = CreateContext();
        var query = context.Set<Product>().FromSqlRaw(
            NormalizeDelimitersInRawString(
                @"SELECT [ProductID], [ProductName], [SupplierID], [UnitPrice], [UnitsInStock], NULL AS [Discontinued]
                FROM [Products]"));

        Assert.Equal(
            RelationalStrings.ErrorMaterializingPropertyNullReference("Product", "Discontinued", typeof(bool)),
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToList())).Message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Bad_data_error_handling_null_projection(bool async)
    {
        using var context = CreateContext();
        var query = context.Set<Product>()
            .FromSqlRaw(
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
        var query = context.Set<Product>()
            .FromSqlRaw(
                NormalizeDelimitersInRawString(
                    @"SELECT [ProductID], [ProductName], [SupplierID], [UnitPrice], [UnitsInStock], NULL AS [Discontinued]
                          FROM [Products]")).AsNoTracking();

        Assert.Equal(
            RelationalStrings.ErrorMaterializingPropertyNullReference("Product", "Discontinued", typeof(bool)),
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToList())).Message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_queryable_simple(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>())
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [ContactName] LIKE '%z%'")),
            ss => ss.Set<Customer>().Where(x => x.ContactName.Contains("z")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_queryable_simple_columns_out_of_order(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(
                NormalizeDelimitersInRawString(
                    "SELECT [Region], [PostalCode], [Phone], [Fax], [CustomerID], [Country], [ContactTitle], [ContactName], [CompanyName], [City], [Address] FROM [Customers]")),
            ss => ss.Set<Customer>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_queryable_simple_columns_out_of_order_and_extra_columns(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(
                NormalizeDelimitersInRawString(
                    "SELECT [Region], [PostalCode], [PostalCode] AS [Foo], [Phone], [Fax], [CustomerID], [Country], [ContactTitle], [ContactName], [CompanyName], [City], [Address] FROM [Customers]")),
            ss => ss.Set<Customer>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSqlRaw_queryable_simple_columns_out_of_order_and_not_enough_columns_throws(bool async)
    {
        using var context = CreateContext();
        var query = context.Set<Customer>().FromSqlRaw(
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
    public virtual Task FromSqlRaw_queryable_composed(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                .Where(c => c.ContactName.Contains("z")),
            ss => ss.Set<Customer>().Where(c => c.ContactName.Contains("z")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_queryable_composed_after_removing_whitespaces(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        _eol + "    " + _eol + _eol + _eol + "SELECT" + _eol + "* FROM [Customers]"))
                .Where(c => c.ContactName.Contains("z")),
            ss => ss.Set<Customer>().Where(c => c.ContactName.Contains("z")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSqlRaw_queryable_composed_compiled(bool async)
    {
        if (async)
        {
            var query = EF.CompileAsyncQuery(
                (NorthwindContext context) => context.Set<Customer>()
                    .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
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
                (NorthwindContext context) => context.Set<Customer>()
                    .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
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
    public virtual async Task FromSqlRaw_queryable_composed_compiled_with_parameter(bool async)
    {
        if (async)
        {
            var query = EF.CompileAsyncQuery(
                (NorthwindContext context) => context.Set<Customer>()
                    .FromSqlRaw(
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
                (NorthwindContext context) => context.Set<Customer>()
                    .FromSqlRaw(
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
    public virtual async Task FromSqlRaw_queryable_composed_compiled_with_DbParameter(bool async)
    {
        if (async)
        {
            var query = EF.CompileAsyncQuery(
                (NorthwindContext context) => context.Set<Customer>()
                    .FromSqlRaw(
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
                (NorthwindContext context) => context.Set<Customer>()
                    .FromSqlRaw(
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
    public virtual async Task FromSqlRaw_queryable_composed_compiled_with_nameless_DbParameter(bool async)
    {
        if (async)
        {
            var query = EF.CompileAsyncQuery(
                (NorthwindContext context) => context.Set<Customer>()
                    .FromSqlRaw(
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
                (NorthwindContext context) => context.Set<Customer>()
                    .FromSqlRaw(
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
    public virtual Task FromSqlRaw_composed_contains(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where ((DbSet<Order>)ss.Set<Order>()).FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Orders]"))
                      .Select(o => o.CustomerID)
                      .Contains(c.CustomerID)
                  select c,
            ss => from c in ss.Set<Customer>()
                  where ss.Set<Order>()
                      .Select(o => o.CustomerID)
                      .Contains(c.CustomerID)
                  select c);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_composed_contains2(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where
                      c.CustomerID == "ALFKI"
                      && ((DbSet<Order>)ss.Set<Order>()).FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Orders]"))
                      .Select(o => o.CustomerID)
                      .Contains(c.CustomerID)
                  select c,
            ss => from c in ss.Set<Customer>()
                  where c.CustomerID == "ALFKI" && ss.Set<Order>().Select(o => o.CustomerID).Contains(c.CustomerID)
                  select c);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_queryable_multiple_composed(bool async)
        => AssertQuery(
            async,
            ss => from c in ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                  from o in ((DbSet<Order>)ss.Set<Order>()).FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Orders]"))
                  where c.CustomerID == o.CustomerID
                  select new { c, o },
            ss => from c in ss.Set<Customer>()
                  from o in ss.Set<Order>()
                  where c.CustomerID == o.CustomerID
                  select new { c, o });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_queryable_multiple_composed_with_closure_parameters(bool async)
    {
        var startDate = new DateTime(1997, 1, 1);
        var endDate = new DateTime(1998, 1, 1);

        return AssertQuery(
            async,
            ss => from c in ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                  from o in ((DbSet<Order>)ss.Set<Order>()).FromSqlRaw(
                      NormalizeDelimitersInRawString("SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {0} AND {1}"),
                      startDate,
                      endDate)
                  where c.CustomerID == o.CustomerID
                  select new { c, o },
            ss => from c in ss.Set<Customer>()
                  from o in ss.Set<Order>().Where(x => x.OrderDate >= startDate && x.OrderDate <= endDate)
                  where c.CustomerID == o.CustomerID
                  select new { c, o });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSqlRaw_queryable_multiple_composed_with_parameters_and_closure_parameters(bool async)
    {
        var city = "London";
        var startDate = new DateTime(1997, 1, 1);
        var endDate = new DateTime(1998, 1, 1);

        await AssertQuery(
            async,
            ss => from c in ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(
                      NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), city)
                  from o in ((DbSet<Order>)ss.Set<Order>()).FromSqlRaw(
                      NormalizeDelimitersInRawString("SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {0} AND {1}"),
                      startDate,
                      endDate)
                  where c.CustomerID == o.CustomerID
                  select new { c, o },
            ss => from c in ss.Set<Customer>().Where(x => x.City == city)
                  from o in ss.Set<Order>().Where(x => x.OrderDate >= startDate && x.OrderDate <= endDate)
                  where c.CustomerID == o.CustomerID
                  select new { c, o });

        city = "Berlin";
        startDate = new DateTime(1998, 4, 1);
        endDate = new DateTime(1998, 5, 1);

        await AssertQuery(
            async,
            ss => from c in ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(
                      NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), city)
                  from o in ((DbSet<Order>)ss.Set<Order>()).FromSqlRaw(
                      NormalizeDelimitersInRawString("SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {0} AND {1}"),
                      startDate,
                      endDate)
                  where c.CustomerID == o.CustomerID
                  select new { c, o },
            ss => from c in ss.Set<Customer>().Where(x => x.City == city)
                  from o in ss.Set<Order>().Where(x => x.OrderDate >= startDate && x.OrderDate <= endDate)
                  where c.CustomerID == o.CustomerID
                  select new { c, o });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_queryable_multiple_line_query(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(
                NormalizeDelimitersInRawString(
                    @"SELECT *
FROM [Customers]
WHERE [City] = 'London'")),
            ss => ss.Set<Customer>().Where(x => x.City == "London"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_queryable_composed_multiple_line_query(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        @"SELECT *
FROM [Customers]"))
                .Where(c => c.City == "London"),
            ss => ss.Set<Customer>().Where(x => x.City == "London"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_queryable_with_parameters(bool async)
    {
        var city = "London";
        var contactTitle = "Sales Representative";

        return AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(
                NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0} AND [ContactTitle] = {1}"), city,
                contactTitle),
            ss => ss.Set<Customer>().Where(x => x.City == city && x.ContactTitle == contactTitle));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_queryable_with_parameters_inline(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(
                NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0} AND [ContactTitle] = {1}"), "London",
                "Sales Representative"),
            ss => ss.Set<Customer>().Where(x => x.City == "London" && x.ContactTitle == "Sales Representative"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlInterpolated_queryable_with_parameters_interpolated(bool async)
    {
        var city = "London";
        var contactTitle = "Sales Representative";

        return AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>()).FromSqlInterpolated(
                NormalizeDelimitersInInterpolatedString(
                    $"SELECT * FROM [Customers] WHERE [City] = {city} AND [ContactTitle] = {contactTitle}")),
            ss => ss.Set<Customer>().Where(x => x.City == city && x.ContactTitle == contactTitle));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSql_queryable_with_parameters_interpolated(bool async)
    {
        var city = "London";
        var contactTitle = "Sales Representative";

        return AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>()).FromSql(
                NormalizeDelimitersInInterpolatedString(
                    $"SELECT * FROM [Customers] WHERE [City] = {city} AND [ContactTitle] = {contactTitle}")),
            ss => ss.Set<Customer>().Where(x => x.City == city && x.ContactTitle == contactTitle));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlInterpolated_queryable_with_parameters_inline_interpolated(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>()).FromSqlInterpolated(
                NormalizeDelimitersInInterpolatedString(
                    $"SELECT * FROM [Customers] WHERE [City] = {"London"} AND [ContactTitle] = {"Sales Representative"}")),
            ss => ss.Set<Customer>().Where(x => x.City == "London" && x.ContactTitle == "Sales Representative"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSql_queryable_with_parameters_inline_interpolated(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>()).FromSql(
                NormalizeDelimitersInInterpolatedString(
                    $"SELECT * FROM [Customers] WHERE [City] = {"London"} AND [ContactTitle] = {"Sales Representative"}")),
            ss => ss.Set<Customer>().Where(x => x.City == "London" && x.ContactTitle == "Sales Representative"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSqlInterpolated_queryable_multiple_composed_with_parameters_and_closure_parameters_interpolated(
        bool async)
    {
        var city = "London";
        var startDate = new DateTime(1997, 1, 1);
        var endDate = new DateTime(1998, 1, 1);

        await AssertQuery(
            async,
            ss => from c in ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(
                      NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), city)
                  from o in ((DbSet<Order>)ss.Set<Order>()).FromSqlInterpolated(
                      NormalizeDelimitersInInterpolatedString(
                          $"SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {startDate} AND {endDate}"))
                  where c.CustomerID == o.CustomerID
                  select new { c, o },
            ss => from c in ss.Set<Customer>().Where(x => x.City == city)
                  from o in ss.Set<Order>().Where(x => x.OrderDate >= startDate && x.OrderDate <= endDate)
                  where c.CustomerID == o.CustomerID
                  select new { c, o });

        city = "Berlin";
        startDate = new DateTime(1998, 4, 1);
        endDate = new DateTime(1998, 5, 1);

        await AssertQuery(
            async,
            ss => from c in ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(
                      NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), city)
                  from o in ((DbSet<Order>)ss.Set<Order>()).FromSqlInterpolated(
                      NormalizeDelimitersInInterpolatedString(
                          $"SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {startDate} AND {endDate}"))
                  where c.CustomerID == o.CustomerID
                  select new { c, o },
            ss => from c in ss.Set<Customer>().Where(x => x.City == city)
                  from o in ss.Set<Order>().Where(x => x.OrderDate >= startDate && x.OrderDate <= endDate)
                  where c.CustomerID == o.CustomerID
                  select new { c, o });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSql_queryable_multiple_composed_with_parameters_and_closure_parameters_interpolated(
        bool async)
    {
        var city = "London";
        var startDate = new DateTime(1997, 1, 1);
        var endDate = new DateTime(1998, 1, 1);

        await AssertQuery(
            async,
            ss => from c in ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(
                      NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), city)
                  from o in ((DbSet<Order>)ss.Set<Order>()).FromSql(
                      NormalizeDelimitersInInterpolatedString(
                          $"SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {startDate} AND {endDate}"))
                  where c.CustomerID == o.CustomerID
                  select new { c, o },
            ss => from c in ss.Set<Customer>().Where(x => x.City == city)
                  from o in ss.Set<Order>().Where(x => x.OrderDate >= startDate && x.OrderDate <= endDate)
                  where c.CustomerID == o.CustomerID
                  select new { c, o });

        city = "Berlin";
        startDate = new DateTime(1998, 4, 1);
        endDate = new DateTime(1998, 5, 1);

        await AssertQuery(
            async,
            ss => from c in ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(
                      NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), city)
                  from o in ((DbSet<Order>)ss.Set<Order>()).FromSql(
                      NormalizeDelimitersInInterpolatedString(
                          $"SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN {startDate} AND {endDate}"))
                  where c.CustomerID == o.CustomerID
                  select new { c, o },
            ss => from c in ss.Set<Customer>().Where(x => x.City == city)
                  from o in ss.Set<Order>().Where(x => x.OrderDate >= startDate && x.OrderDate <= endDate)
                  where c.CustomerID == o.CustomerID
                  select new { c, o });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_queryable_with_null_parameter(bool async)
    {
        uint? reportsTo = null;

        return AssertQuery(
            async,
            ss => ((DbSet<Employee>)ss.Set<Employee>()).FromSqlRaw(
                NormalizeDelimitersInRawString(
                    // ReSharper disable once ExpressionIsAlwaysNull
                    "SELECT * FROM [Employees] WHERE [ReportsTo] = {0} OR ([ReportsTo] IS NULL AND {0} IS NULL)"), reportsTo),
            ss => ss.Set<Employee>().Where(x => x.ReportsTo == reportsTo));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task<string> FromSqlRaw_queryable_with_parameters_and_closure(bool async)
    {
        var city = "London";
        var contactTitle = "Sales Representative";

        using var context = CreateContext();
        var query = context.Set<Customer>().FromSqlRaw(
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
    public virtual async Task FromSqlRaw_queryable_simple_cache_key_includes_query_string(bool async)
    {
        await AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>())
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = 'London'")),
            ss => ss.Set<Customer>().Where(x => x.City == "London"));

        await AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>())
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = 'Seattle'")),
            ss => ss.Set<Customer>().Where(x => x.City == "Seattle"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSqlRaw_queryable_with_parameters_cache_key_includes_parameters(bool async)
    {
        var city = "London";
        var contactTitle = "Sales Representative";
        var sql = "SELECT * FROM [Customers] WHERE [City] = {0} AND [ContactTitle] = {1}";

        await AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(NormalizeDelimitersInRawString(sql), city, contactTitle),
            ss => ss.Set<Customer>().Where(x => x.City == city && x.ContactTitle == contactTitle));

        city = "Madrid";
        contactTitle = "Accounting Manager";

        await AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(NormalizeDelimitersInRawString(sql), city, contactTitle),
            ss => ss.Set<Customer>().Where(x => x.City == city && x.ContactTitle == contactTitle));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_queryable_simple_as_no_tracking_not_composed(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>())
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                .AsNoTracking(),
            ss => ss.Set<Customer>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSqlRaw_queryable_simple_projection_composed(bool async)
    {
        using var context = CreateContext();
        var boolMapping = (RelationalTypeMapping)context.GetService<ITypeMappingSource>().FindMapping(typeof(bool));
        var boolLiteral = boolMapping.GenerateSqlLiteral(true);

        await AssertQuery(
            async,
            ss => ((DbSet<Product>)ss.Set<Product>()).FromSqlRaw(
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
    public virtual Task FromSqlRaw_queryable_simple_include(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>())
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                .Include(c => c.Orders),
            ss => ss.Set<Customer>(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(x => x.Orders)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_queryable_simple_composed_include(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>())
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                .Include(c => c.Orders)
                .Where(c => c.City == "London"),
            ss => ss.Set<Customer>().Where(c => c.City == "London"),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(x => x.Orders)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSqlRaw_annotations_do_not_affect_successive_calls(bool async)
    {
        using var context = CreateContext();
        var query = context.Customers
            .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [ContactName] LIKE '%z%'"));

        var actual = async
            ? await query.ToArrayAsync()
            : query.ToArray();

        Assert.Equal(14, actual.Length);

        query = context.Customers;
        actual = async
            ? await query.ToArrayAsync()
            : query.ToArray();

        Assert.Equal(91, actual.Length);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_composed_with_nullable_predicate(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>())
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
                .Where(c => c.ContactName == c.CompanyName),
            ss => ss.Set<Customer>().Where(c => c.ContactName == c.CompanyName),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSqlRaw_with_dbParameter(bool async)
    {
        var parameter = CreateDbParameter("@city", "London");

        await AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(
                NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = @city"), parameter),
            ss => ss.Set<Customer>().Where(x => x.City == "London"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSqlRaw_with_dbParameter_without_name_prefix(bool async)
    {
        var parameter = CreateDbParameter("city", "London");

        await AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(
                NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = @city"), parameter),
            ss => ss.Set<Customer>().Where(x => x.City == "London"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSqlRaw_with_dbParameter_mixed(bool async)
    {
        var city = "London";
        var title = "Sales Representative";

        var titleParameter = CreateDbParameter("@title", title);

        await AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(
                NormalizeDelimitersInRawString(
                    "SELECT * FROM [Customers] WHERE [City] = {0} AND [ContactTitle] = @title"), city, titleParameter),
            ss => ss.Set<Customer>().Where(x => x.City == city && x.ContactTitle == title));

        var cityParameter = CreateDbParameter("@city", city);

        await AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(
                NormalizeDelimitersInRawString(
                    "SELECT * FROM [Customers] WHERE [City] = @city AND [ContactTitle] = {1}"), cityParameter, title),
            ss => ss.Set<Customer>().Where(x => x.City == city && x.ContactTitle == title));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Include_does_not_close_user_opened_connection_for_empty_result(bool async)
    {
        Fixture.TestStore.CloseConnection();
        using (var context = CreateContext())
        {
            var connection = context.Database.GetDbConnection();

            Assert.Equal(ConnectionState.Closed, connection.State);

            context.Database.OpenConnection();

            Assert.Equal(ConnectionState.Open, connection.State);

            var query = context.Customers
                .Include(v => v.Orders)
                .Where(v => v.CustomerID == "MAMRFC");

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.False(query.Any());
            Assert.Equal(ConnectionState.Open, connection.State);

            context.Database.CloseConnection();

            Assert.Equal(ConnectionState.Closed, connection.State);
        }

        Fixture.TestStore.OpenConnection();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSqlRaw_with_db_parameters_called_multiple_times(bool async)
    {
        using var context = CreateContext();
        var parameter = CreateDbParameter("@id", "ALFKI");

        var query = context.Customers.FromSqlRaw(
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
    public virtual Task FromSqlRaw_with_SelectMany_and_include(bool async)
        => AssertQuery(
            async,
            ss => from c1 in ((DbSet<Customer>)ss.Set<Customer>())
                      .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = 'ALFKI'"))
                  from c2 in ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(
                          NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = 'AROUT'"))
                      .Include(c => c.Orders)
                  select new { c1, c2 },
            ss => from c1 in ss.Set<Customer>().Where(x => x.CustomerID == "ALFKI")
                  from c2 in ss.Set<Customer>().Where(x => x.CustomerID == "AROUT")
                  select new { c1, c2 },
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.c1, a.c1);
                AssertInclude(e.c2, a.c2, new ExpectedInclude<Customer>(x => x.Orders));
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_with_join_and_include(bool async)
        => AssertQuery(
            async,
            ss => from c in ((DbSet<Customer>)ss.Set<Customer>())
                      .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = 'ALFKI'"))
                  join o in ((DbSet<Order>)ss.Set<Order>()).FromSqlRaw(
                          NormalizeDelimitersInRawString("SELECT * FROM [Orders] WHERE [OrderID] <> 1"))
                      .Include(o => o.OrderDetails)
                      on c.CustomerID equals o.CustomerID
                  select new { c, o },
            ss => from c in ss.Set<Customer>().Where(x => x.CustomerID == "ALFKI")
                  join o in ss.Set<Order>().Where(x => x.OrderID != 1)
                      on c.CustomerID equals o.CustomerID
                  select new { c, o },
            elementSorter: e => (e.c.CustomerID, e.o.OrderID),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.c, a.c);
                AssertInclude(e.o, a.o, new ExpectedInclude<Order>(x => x.OrderDetails));
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Include_closed_connection_opened_by_it_when_buffering(bool async)
    {
        Fixture.TestStore.CloseConnection();
        using var context = CreateContext();
        var connection = context.Database.GetDbConnection();

        Assert.Equal(ConnectionState.Closed, connection.State);

        var query = context.Customers
            .Include(v => v.Orders)
            .Where(v => v.CustomerID == "ALFKI");

        var actual = async
            ? await query.ToArrayAsync()
            : query.ToArray();

        Assert.NotEmpty(query);
        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSqlInterpolated_with_inlined_db_parameter(bool async)
    {
        var parameter = CreateDbParameter("@somename", "ALFKI");

        await AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>())
                .FromSqlInterpolated(
                    NormalizeDelimitersInInterpolatedString($"SELECT * FROM [Customers] WHERE [CustomerID] = {parameter}")),
            ss => ss.Set<Customer>().Where(x => x.CustomerID == "ALFKI"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSql_with_inlined_db_parameter(bool async)
    {
        var parameter = CreateDbParameter("@somename", "ALFKI");

        await AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>())
                .FromSql(
                    NormalizeDelimitersInInterpolatedString($"SELECT * FROM [Customers] WHERE [CustomerID] = {parameter}")),
            ss => ss.Set<Customer>().Where(x => x.CustomerID == "ALFKI"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSqlInterpolated_with_inlined_db_parameter_without_name_prefix(bool async)
    {
        var parameter = CreateDbParameter("somename", "ALFKI");

        await AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>())
                .FromSqlInterpolated(
                    NormalizeDelimitersInInterpolatedString($"SELECT * FROM [Customers] WHERE [CustomerID] = {parameter}")),
            ss => ss.Set<Customer>().Where(x => x.CustomerID == "ALFKI"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSql_with_inlined_db_parameter_without_name_prefix(bool async)
    {
        var parameter = CreateDbParameter("somename", "ALFKI");

        await AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>())
                .FromSql(
                    NormalizeDelimitersInInterpolatedString($"SELECT * FROM [Customers] WHERE [CustomerID] = {parameter}")),
            ss => ss.Set<Customer>().Where(x => x.CustomerID == "ALFKI"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSqlInterpolated_parameterization_issue_12213(bool async)
    {
        using var context = CreateContext();
        var min = 10300;
        var max = 10400;

        var query1 = context.Orders
            .FromSqlInterpolated(NormalizeDelimitersInInterpolatedString($"SELECT * FROM [Orders] WHERE [OrderID] >= {min}"))
            .Select(i => i.OrderID);

        var actual1 = async
            ? await query1.ToArrayAsync()
            : query1.ToArray();

        var query2 = context.Orders
            .Where(o => o.OrderID <= max && query1.Contains(o.OrderID))
            .Select(o => o.OrderID);

        var actual2 = async
            ? await query2.ToArrayAsync()
            : query2.ToArray();

        var query3 = context.Orders
            .Where(
                o => o.OrderID <= max
                    && context.Orders
                        .FromSqlInterpolated(
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
    public virtual async Task FromSqlRaw_does_not_parameterize_interpolated_string(bool async)
    {
        var tableName = "Orders";
        var max = 10250;

        await AssertQuery(
            async,
            ss => ((DbSet<Order>)ss.Set<Order>()).FromSqlRaw(
                NormalizeDelimitersInRawString($"SELECT * FROM [{tableName}] WHERE [OrderID] < {{0}}"), max),
            ss => ss.Set<Order>().Where(x => x.OrderID < max));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_through_fromsql(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<Order>)ss.Set<Order>())
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Orders]"))
                .Where(o => o.Customer == new Customer { CustomerID = "VINET" }),
            ss => ss.Set<Order>().Where(o => o.Customer == new Customer { CustomerID = "VINET" }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_with_set_operation(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>())
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = 'London'"))
                .Concat(
                    ((DbSet<Customer>)ss.Set<Customer>())
                    .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = 'Berlin'"))),
            ss => ss.Set<Customer>().Where(x => x.City == "London")
                .Concat(ss.Set<Customer>().Where(x => x.City == "Berlin")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Keyless_entity_with_all_nulls(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<OrderQuery>)ss.Set<OrderQuery>())
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT NULL AS [CustomerID] FROM [Customers] WHERE [City] = 'Berlin'"))
                .IgnoreQueryFilters(),
            ss => ss.Set<Customer>().Where(x => x.City == "Berlin").Select(x => new OrderQuery(null)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSql_used_twice_without_parameters(bool async)
    {
        await AssertAny(
            async,
            ss => ((DbSet<OrderQuery>)ss.Set<OrderQuery>())
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT 'ALFKI' AS [CustomerID]"))
                .IgnoreQueryFilters(),
            ss => ss.Set<Customer>().Where(x => x.CustomerID == "ALFKI").Select(x => new OrderQuery(x.CustomerID)));

        await AssertAny(
            async,
            ss => ((DbSet<OrderQuery>)ss.Set<OrderQuery>())
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT 'ALFKI' AS [CustomerID]"))
                .IgnoreQueryFilters(),
            ss => ss.Set<Customer>().Where(x => x.CustomerID == "ALFKI").Select(x => new OrderQuery(x.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSql_used_twice_with_parameters(bool async)
    {
        await AssertAny(
            async,
            ss => ((DbSet<OrderQuery>)ss.Set<OrderQuery>())
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT {0} AS [CustomerID]"), "ALFKI")
                .IgnoreQueryFilters(),
            ss => ss.Set<Customer>().Where(x => x.CustomerID == "ALFKI").Select(x => new OrderQuery(x.CustomerID)));

        await AssertAny(
            async,
            ss => ((DbSet<OrderQuery>)ss.Set<OrderQuery>())
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT {0} AS [CustomerID]"), "ALFKI")
                .IgnoreQueryFilters(),
            ss => ss.Set<Customer>().Where(x => x.CustomerID == "ALFKI").Select(x => new OrderQuery(x.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSql_Count_used_twice_without_parameters(bool async)
    {
        await AssertCount(
            async,
            ss => ((DbSet<OrderQuery>)ss.Set<OrderQuery>())
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT 'ALFKI' AS [CustomerID]"))
                .IgnoreQueryFilters(),
            ss => ss.Set<Customer>().Where(x => x.CustomerID == "ALFKI").Select(x => new OrderQuery(x.CustomerID)));

        await AssertCount(
            async,
            ss => ((DbSet<OrderQuery>)ss.Set<OrderQuery>())
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT 'ALFKI' AS [CustomerID]"))
                .IgnoreQueryFilters(),
            ss => ss.Set<Customer>().Where(x => x.CustomerID == "ALFKI").Select(x => new OrderQuery(x.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSql_Count_used_twice_with_parameters(bool async)
    {
        await AssertCount(
            async,
            ss => ((DbSet<OrderQuery>)ss.Set<OrderQuery>())
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT {0} AS [CustomerID]"), "ALFKI")
                .IgnoreQueryFilters(),
            ss => ss.Set<Customer>().Where(x => x.CustomerID == "ALFKI").Select(x => new OrderQuery(x.CustomerID)));

        await AssertCount(
            async,
            ss => ((DbSet<OrderQuery>)ss.Set<OrderQuery>())
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT {0} AS [CustomerID]"), "ALFKI")
                .IgnoreQueryFilters(),
            ss => ss.Set<Customer>().Where(x => x.CustomerID == "ALFKI").Select(x => new OrderQuery(x.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Line_endings_after_Select(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>())
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT" + Environment.NewLine + "* FROM [Customers]"))
                .Where(e => e.City == "Seattle"),
            ss => ss.Set<Customer>().Where(x => x.City == "Seattle"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSql_with_db_parameter_in_split_query(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>())
                .FromSqlRaw(
                    NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = {0}"),
                    CreateDbParameter("customerID", "ALFKI"))
                .Include(e => e.Orders)
                .ThenInclude(o => o.OrderDetails)
                .AsSplitQuery(),
            ss => ss.Set<Customer>().Where(x => x.CustomerID == "ALFKI"),
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<Customer>(x => x.Orders),
                new ExpectedInclude<Order>(x => x.OrderDetails, "Orders")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_queryable_simple_projection_not_composed(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>())
                .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers]"))
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
    public virtual Task FromSqlRaw_in_subquery_with_dbParameter(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(
                o => ((DbSet<Customer>)ss.Set<Customer>()).FromSqlRaw(
                        NormalizeDelimitersInRawString(@"SELECT * FROM [Customers] WHERE [City] = @city"),
                        // ReSharper disable once FormatStringProblem
                        CreateDbParameter("@city", "London"))
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)),
            ss => ss.Set<Order>().Where(
                o => ss.Set<Customer>().Where(x => x.City == "London")
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_in_subquery_with_positional_dbParameter_without_name(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(
                o => ((DbSet<Customer>)ss.Set<Customer>())
                    .FromSqlRaw(
                        NormalizeDelimitersInRawString(@"SELECT * FROM [Customers] WHERE [City] = {0}"),
                        // ReSharper disable once FormatStringProblem
                        CreateDbParameter(null, "London"))
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)),
            ss => ss.Set<Order>().Where(
                o => ss.Set<Customer>().Where(x => x.City == "London")
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_in_subquery_with_positional_dbParameter_with_name(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(
                o => ((DbSet<Customer>)ss.Set<Customer>())
                    .FromSqlRaw(
                        NormalizeDelimitersInRawString(@"SELECT * FROM [Customers] WHERE [City] = {0}"),
                        // ReSharper disable once FormatStringProblem
                        CreateDbParameter("@city", "London"))
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)),
            ss => ss.Set<Order>().Where(
                o => ss.Set<Customer>().Where(x => x.City == "London")
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task FromSqlRaw_with_dbParameter_mixed_in_subquery(bool async)
    {
        const string city = "London";
        const string title = "Sales Representative";

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(
                o => ((DbSet<Customer>)ss.Set<Customer>())
                    .FromSqlRaw(
                        NormalizeDelimitersInRawString(@"SELECT * FROM [Customers] WHERE [City] = {0} AND [ContactTitle] = @title"),
                        city,
                        // ReSharper disable once FormatStringProblem
                        CreateDbParameter("@title", title))
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)),
            ss => ss.Set<Order>().Where(
                o => ss.Set<Customer>().Where(x => x.City == city && x.ContactTitle == title)
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)));

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(
                o => ((DbSet<Customer>)ss.Set<Customer>())
                    .FromSqlRaw(
                        NormalizeDelimitersInRawString(@"SELECT * FROM [Customers] WHERE [City] = @city AND [ContactTitle] = {1}"),
                        // ReSharper disable once FormatStringProblem
                        CreateDbParameter("@city", city),
                        title)
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)),
            ss => ss.Set<Order>().Where(
                o => ss.Set<Customer>().Where(x => x.City == city && x.ContactTitle == title)
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromSqlRaw_composed_with_common_table_expression(bool async)
        => AssertQuery(
            async,
            ss => ((DbSet<Customer>)ss.Set<Customer>())
                .FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        @"WITH [Customers2] AS (
    SELECT * FROM [Customers]
)
SELECT * FROM [Customers2]"))
                .Where(c => c.ContactName.Contains("z")),
            ss => ss.Set<Customer>().Where(c => c.ContactName.Contains("z")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Multiple_occurrences_of_FromSql_with_db_parameter_adds_parameter_only_once(bool async)
    {
        using var context = CreateContext();
        var city = "Seattle";
        var fromSqlQuery = context.Customers.FromSqlRaw(
            NormalizeDelimitersInRawString(@"SELECT * FROM [Customers] WHERE [City] = {0}"),
            CreateDbParameter("city", city));

        var query = fromSqlQuery.Intersect(fromSqlQuery);

        var actual = async
            ? await query.ToArrayAsync()
            : query.ToArray();

        Assert.Single(actual);
    }

    protected string NormalizeDelimitersInRawString(string sql)
        => Fixture.TestStore.NormalizeDelimitersInRawString(sql);

    protected FormattableString NormalizeDelimitersInInterpolatedString(FormattableString sql)
        => Fixture.TestStore.NormalizeDelimitersInInterpolatedString(sql);

    protected abstract DbParameter CreateDbParameter(string name, object value);

    protected NorthwindContext CreateContext()
        => Fixture.CreateContext();
}
