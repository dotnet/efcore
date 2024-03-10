// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

// ReSharper disable FormatStringProblem
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToConstant.Local
// ReSharper disable AccessToDisposedClosure
namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NorthwindSqlQueryTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : NorthwindQueryRelationalFixture<NoopModelCustomizer>, new()
{
    protected NorthwindSqlQueryTestBase(TFixture fixture)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
    }

    protected TFixture Fixture { get; }

    public static IEnumerable<object[]> IsAsyncData = new object[][] { [false], [true] };

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQueryRaw_over_int(bool async)
    {
        using var context = CreateContext();
        var query = context.Database.SqlQueryRaw<int>(NormalizeDelimitersInRawString(@"SELECT [ProductID] FROM [Products]"));

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Equal(77, result.Count);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQuery_composed_Contains(bool async)
    {
        using var context = CreateContext();
        var query = context.Set<Order>()
            .Where(
                e => context.Database
                    .SqlQuery<int>(NormalizeDelimitersInInterpolatedString(@$"SELECT [ProductID] AS [Value] FROM [Products]"))
                    .Contains(e.OrderID));

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Equal(0, result.Count);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQuery_composed_Join(bool async)
    {
        using var context = CreateContext();
        var query = from o in context.Set<Order>()
                    join p in context.Database.SqlQuery<int>(
                            NormalizeDelimitersInInterpolatedString(@$"SELECT [ProductID] AS [Value] FROM [Products]"))
                        on o.OrderID equals p
                    select new { o, p };

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Equal(0, result.Count);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SqlQuery_over_int_with_parameter(bool async)
    {
        using var context = CreateContext();
        var value = 10;
        var query = context.Database.SqlQuery<int>(
            NormalizeDelimitersInInterpolatedString(@$"SELECT [ProductID] FROM [Products] WHERE [ProductID] = {value}"));

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Equal(10, result.Single());
    }

    protected string NormalizeDelimitersInRawString(string sql)
        => Fixture.TestStore.NormalizeDelimitersInRawString(sql);

    protected FormattableString NormalizeDelimitersInInterpolatedString(FormattableString sql)
        => Fixture.TestStore.NormalizeDelimitersInInterpolatedString(sql);

    protected abstract DbParameter CreateDbParameter(string name, object value);

    protected NorthwindContext CreateContext()
        => Fixture.CreateContext();
}
