// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class FromSqlSprocQueryTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : NorthwindQueryRelationalFixture<NoopModelCustomizer>, new()
{
    protected FromSqlSprocQueryTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_stored_procedure(bool async)
    {
        using var context = CreateContext();
        var query = context
            .Set<MostExpensiveProduct>()
            .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters());

        var actual = async
            ? await query.ToArrayAsync()
            : query.ToArray();

        Assert.Equal(10, actual.Length);

        Assert.Contains(
            actual, mep =>
                mep.TenMostExpensiveProducts == "Côte de Blaye"
                && mep.UnitPrice == 263.50m);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_stored_procedure_with_tag(bool async)
    {
        using var context = CreateContext();
        var query = context
            .Set<MostExpensiveProduct>()
            .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters())
            .TagWith("Stored Procedure");

        var actual = async
            ? await query.ToArrayAsync()
            : query.ToArray();

        Assert.Equal(10, actual.Length);

        Assert.Contains(
            actual, mep =>
                mep.TenMostExpensiveProducts == "Côte de Blaye"
                && mep.UnitPrice == 263.50m);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_stored_procedure_with_tags(bool async)
    {
        using var context = CreateContext();
        var query = context
            .Set<MostExpensiveProduct>()
            .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters())
            .TagWith("One")
            .TagWith("Two")
            .TagWith("Three");

        var actual = async
            ? await query.ToArrayAsync()
            : query.ToArray();

        Assert.Equal(10, actual.Length);

        Assert.Contains(
            actual, mep =>
                mep.TenMostExpensiveProducts == "Côte de Blaye"
                && mep.UnitPrice == 263.50m);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_stored_procedure_with_caller_info_tag(bool async)
    {
        using var context = CreateContext();
        var query = context
            .Set<MostExpensiveProduct>()
            .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters())
            .TagWithCallSite("SampleFileName", 13);

        var queryResult = async
            ? await query.ToArrayAsync()
            : query.ToArray();

        var actual = query.ToQueryString().Split(Environment.NewLine).First();

        Assert.Equal("-- File: SampleFileName:13", actual);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_stored_procedure_with_caller_info_tag_and_other_tags(bool async)
    {
        using var context = CreateContext();
        var query = context
            .Set<MostExpensiveProduct>()
            .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters())
            .TagWith("Before")
            .TagWithCallSite("SampleFileName", 13)
            .TagWith("After");

        var queryResult = async
            ? await query.ToArrayAsync()
            : query.ToArray();

        var tags = query.ToQueryString().Split(Environment.NewLine).ToList();

        Assert.Equal("-- Before", tags[0]);
        Assert.Equal("-- File: SampleFileName:13", tags[1]);
        Assert.Equal("-- After", tags[2]);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_stored_procedure_projection(bool async)
    {
        using var context = CreateContext();
        var query = context
            .Set<MostExpensiveProduct>()
            .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters())
            .Select(mep => mep.TenMostExpensiveProducts);

        Assert.Equal(
            RelationalStrings.FromSqlNonComposable,
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToArrayAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToArray())).Message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_stored_procedure_re_projection(bool async)
    {
        using var context = CreateContext();
        var query = context
            .Set<MostExpensiveProduct>()
            .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters())
            .Select(
                mep =>
                    new MostExpensiveProduct { TenMostExpensiveProducts = "Foo", UnitPrice = mep.UnitPrice });

        Assert.Equal(
            RelationalStrings.FromSqlNonComposable,
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToArrayAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToArray())).Message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_stored_procedure_re_projection_on_client(bool async)
    {
        using var context = CreateContext();
        var query = context
            .Set<MostExpensiveProduct>()
            .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters());

        var actual = (async ? await query.ToListAsync() : query.ToList())
            .Select(
                mep =>
                    new MostExpensiveProduct { TenMostExpensiveProducts = "Foo", UnitPrice = mep.UnitPrice }).ToArray();

        Assert.Equal(10, actual.Length);
        Assert.True(actual.All(mep => mep.TenMostExpensiveProducts == "Foo"));
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_stored_procedure_with_parameter(bool async)
    {
        using var context = CreateContext();
        var query = context
            .Set<CustomerOrderHistory>()
            .FromSqlRaw(CustomerOrderHistorySproc, GetCustomerOrderHistorySprocParameters());

        var actual = async
            ? query.ToArray()
            : await query.ToArrayAsync();

        Assert.Equal(11, actual.Length);

        Assert.Contains(
            actual, coh =>
                coh.ProductName == "Aniseed Syrup"
                && coh.Total == 6);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_stored_procedure_composed(bool async)
    {
        using var context = CreateContext();
        var query = context
            .Set<MostExpensiveProduct>()
            .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters())
            .Where(mep => mep.TenMostExpensiveProducts.Contains("C"))
            .OrderBy(mep => mep.UnitPrice);

        Assert.Equal(
            RelationalStrings.FromSqlNonComposable,
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToArrayAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToArray())).Message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_stored_procedure_composed_on_client(bool async)
    {
        using var context = CreateContext();
        var query = context
            .Set<MostExpensiveProduct>()
            .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters());

        var actual = (async
                ? await query.ToListAsync()
                : query.ToList())
            .Where(mep => mep.TenMostExpensiveProducts.Contains("C"))
            .OrderBy(mep => mep.UnitPrice)
            .ToArray();

        Assert.Equal(4, actual.Length);
        Assert.Equal(46.00m, actual.First().UnitPrice);
        Assert.Equal(263.50m, actual.Last().UnitPrice);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_stored_procedure_with_parameter_composed(bool async)
    {
        using var context = CreateContext();

        var query = context
            .Set<CustomerOrderHistory>()
            .FromSqlRaw(CustomerOrderHistorySproc, GetCustomerOrderHistorySprocParameters())
            .Where(coh => coh.ProductName.Contains("C"))
            .OrderBy(coh => coh.Total);

        Assert.Equal(
            RelationalStrings.FromSqlNonComposable,
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToArrayAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToArray())).Message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_stored_procedure_with_parameter_composed_on_client(bool async)
    {
        using var context = CreateContext();
        var query = context
            .Set<CustomerOrderHistory>()
            .FromSqlRaw(CustomerOrderHistorySproc, GetCustomerOrderHistorySprocParameters());

        var actual = (async
                ? await query.ToListAsync()
                : query.ToList())
            .Where(coh => coh.ProductName.Contains("C"))
            .OrderBy(coh => coh.Total)
            .ToArray();

        Assert.Equal(2, actual.Length);
        Assert.Equal(15, actual.First().Total);
        Assert.Equal(21, actual.Last().Total);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_stored_procedure_take(bool async)
    {
        using var context = CreateContext();
        var query = context
            .Set<MostExpensiveProduct>()
            .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters())
            .OrderByDescending(mep => mep.UnitPrice)
            .Take(2);

        Assert.Equal(
            RelationalStrings.FromSqlNonComposable,
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToArrayAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToArray())).Message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_stored_procedure_take_on_client(bool async)
    {
        using var context = CreateContext();
        var query = context
            .Set<MostExpensiveProduct>()
            .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters());

        var actual = (async
                ? await query.ToListAsync()
                : query.ToList())
            .OrderByDescending(mep => mep.UnitPrice)
            .Take(2)
            .ToArray();

        Assert.Equal(2, actual.Length);
        Assert.Equal(263.50m, actual.First().UnitPrice);
        Assert.Equal(123.79m, actual.Last().UnitPrice);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_stored_procedure_min(bool async)
    {
        using var context = CreateContext();
        var query = context.Set<MostExpensiveProduct>()
            .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters());

        Assert.Equal(
            RelationalStrings.FromSqlNonComposable,
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.MinAsync(mep => mep.UnitPrice))
                : Assert.Throws<InvalidOperationException>(() => query.Min(mep => mep.UnitPrice))).Message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_stored_procedure_min_on_client(bool async)
    {
        using var context = CreateContext();
        var query = context.Set<MostExpensiveProduct>()
            .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters());

        Assert.Equal(
            45.60m,
            (async
                ? await query.ToListAsync()
                : query.ToList())
            .Min(mep => mep.UnitPrice));
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_stored_procedure_with_include_throws(bool async)
    {
        using var context = CreateContext();
        var query = context.Set<Product>()
            .FromSqlRaw("SelectStoredProcedure")
            .Include(p => p.OrderDetails);

        Assert.Equal(
            RelationalStrings.FromSqlNonComposable,
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToArrayAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToArray())).Message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_with_multiple_stored_procedures(bool async)
    {
        using var context = CreateContext();
        var query = from a in context.Set<MostExpensiveProduct>()
                        .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters())
                    from b in context.Set<MostExpensiveProduct>()
                        .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters())
                    where a.TenMostExpensiveProducts == b.TenMostExpensiveProducts
                    select new { a, b };

        Assert.Equal(
            RelationalStrings.FromSqlNonComposable,
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToArrayAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToArray())).Message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_with_multiple_stored_procedures_on_client(bool async)
    {
        using var context = CreateContext();
        var query1 = context.Set<MostExpensiveProduct>()
            .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters());

        var query2 = context.Set<MostExpensiveProduct>()
            .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters());

        var results1 = async ? await query1.ToListAsync() : query1.ToList();
        var results2 = (async ? await query2.ToListAsync() : query2.ToList());

        var actual = (from a in results1
                      from b in results2
                      where a.TenMostExpensiveProducts == b.TenMostExpensiveProducts
                      select new { a, b }).ToArray();

        Assert.Equal(10, actual.Length);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_stored_procedure_and_select(bool async)
    {
        using var context = CreateContext();
        var query = from mep in context.Set<MostExpensiveProduct>()
                        .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters())
                    from p in context.Set<Product>()
                        .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Products]"))
                    where mep.TenMostExpensiveProducts == p.ProductName
                    select new { mep, p };

        Assert.Equal(
            RelationalStrings.FromSqlNonComposable,
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToArrayAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToArray())).Message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_stored_procedure_and_select_on_client(bool async)
    {
        using var context = CreateContext();
        var query1 = context.Set<MostExpensiveProduct>()
            .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters());
        var query2 = context.Set<Product>()
            .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Products]"));

        var results1 = async ? await query1.ToListAsync() : query1.ToList();
        var results2 = async ? await query2.ToListAsync() : query2.ToList();

        var actual = (from mep in results1
                      from p in results2
                      where mep.TenMostExpensiveProducts == p.ProductName
                      select new { mep, p }).ToArray();

        Assert.Equal(10, actual.Length);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_select_and_stored_procedure(bool async)
    {
        using var context = CreateContext();
        var query = from p in context.Set<Product>().FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Products]"))
                    from mep in context.Set<MostExpensiveProduct>()
                        .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters())
                    where mep.TenMostExpensiveProducts == p.ProductName
                    select new { mep, p };

        Assert.Equal(
            RelationalStrings.FromSqlNonComposable,
            (async
                ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToArrayAsync())
                : Assert.Throws<InvalidOperationException>(() => query.ToArray())).Message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task From_sql_queryable_select_and_stored_procedure_on_client(bool async)
    {
        using var context = CreateContext();

        var query1 = context.Set<Product>()
            .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Products]"));
        var query2 = context.Set<MostExpensiveProduct>()
            .FromSqlRaw(TenMostExpensiveProductsSproc, GetTenMostExpensiveProductsParameters());

        var results1 = async ? await query1.ToListAsync() : query1.ToList();
        var results2 = async ? await query2.ToListAsync() : query2.ToList();

        var actual = (from p in results1
                      from mep in results2
                      where mep.TenMostExpensiveProducts == p.ProductName
                      select new { mep, p }).ToArray();

        Assert.Equal(10, actual.Length);
    }

    private string NormalizeDelimitersInRawString(string sql)
        => Fixture.TestStore.NormalizeDelimitersInRawString(sql);

    protected virtual object[] GetTenMostExpensiveProductsParameters()
        => [];

    protected virtual object[] GetCustomerOrderHistorySprocParameters()
        => ["ALFKI"];

    protected NorthwindContext CreateContext()
        => Fixture.CreateContext();

    protected abstract string TenMostExpensiveProductsSproc { get; }

    protected abstract string CustomerOrderHistorySproc { get; }
}
