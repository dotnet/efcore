// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class TemporalCompiledQuerySqlServerTest(TemporalCompiledQuerySqlServerFixture fixture)
    : IClassFixture<TemporalCompiledQuerySqlServerFixture>
{
    private static readonly DateTime PointInTime = new(2020, 1, 1);
    private static readonly DateTime LaterPointInTime = new(2021, 1, 1);

    // A per-invocation argument used to throw ArgumentException out of ExpressionTreeFuncletizer,
    // complaining that IQueryable<City> did not fit a DbSet<City> parameter. It now reports why.

    [Fact]
    public async Task TemporalAsOf_with_query_parameter_reports_a_guided_error()
    {
        using var context = fixture.CreateContext();

        var compiled = EF.CompileAsyncQuery(
            (TemporalCompiledQueryContext c, DateTime asOf) => c.Customers.TemporalAsOf(asOf).Select(x => x.Name));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await compiled(context, PointInTime).ToListAsync());

        Assert.Equal(
            SqlServerStrings.TemporalOperatorRequiresConstantArgumentInCompiledQuery(
                nameof(SqlServerDbSetExtensions.TemporalAsOf)),
            exception.Message);
    }

    [Theory]
    [InlineData("FromTo")]
    [InlineData("Between")]
    [InlineData("ContainedIn")]
    public async Task Temporal_range_operators_with_query_parameters_report_a_guided_error(string op)
    {
        using var context = fixture.CreateContext();

        Func<TemporalCompiledQueryContext, DateTime, DateTime, IAsyncEnumerable<string?>> compiled = op switch
        {
            "FromTo" => EF.CompileAsyncQuery(
                (TemporalCompiledQueryContext c, DateTime a, DateTime b) => c.Customers.TemporalFromTo(a, b).Select(x => x.Name)),
            "Between" => EF.CompileAsyncQuery(
                (TemporalCompiledQueryContext c, DateTime a, DateTime b) => c.Customers.TemporalBetween(a, b).Select(x => x.Name)),
            "ContainedIn" => EF.CompileAsyncQuery(
                (TemporalCompiledQueryContext c, DateTime a, DateTime b) => c.Customers.TemporalContainedIn(a, b).Select(x => x.Name)),
            _ => throw new ArgumentOutOfRangeException(nameof(op)),
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await compiled(context, PointInTime, LaterPointInTime).ToListAsync());

        Assert.Contains("compiled query", exception.Message);
    }

    [Fact]
    public void TemporalAsOf_with_query_parameter_reports_a_guided_error_for_sync_CompileQuery()
    {
        using var context = fixture.CreateContext();

        var compiled = EF.CompileQuery(
            (TemporalCompiledQueryContext c, DateTime asOf) => c.Customers.TemporalAsOf(asOf).Select(x => x.Name));

        var exception = Assert.Throws<InvalidOperationException>(() => compiled(context, PointInTime).ToList());

        Assert.Contains("compiled query", exception.Message);
    }

    // The supported shapes must keep working: these evaluate to a temporal query root before
    // translation, so they never reach the guard above.

    [Fact]
    public async Task TemporalAsOf_with_a_constant_still_works()
    {
        using var context = fixture.CreateContext();

        var compiled = EF.CompileAsyncQuery(
            (TemporalCompiledQueryContext c) => c.Customers.TemporalAsOf(new DateTime(2020, 1, 1)).Select(x => x.Name));

        _ = await compiled(context).ToListAsync();
    }

    [Fact]
    public async Task TemporalAsOf_with_a_captured_variable_still_works()
    {
        using var context = fixture.CreateContext();

        var captured = PointInTime;
        var compiled = EF.CompileAsyncQuery(
            (TemporalCompiledQueryContext c) => c.Customers.TemporalAsOf(captured).Select(x => x.Name));

        _ = await compiled(context).ToListAsync();
    }

    [Fact]
    public async Task TemporalAll_still_works()
    {
        using var context = fixture.CreateContext();

        var compiled = EF.CompileAsyncQuery(
            (TemporalCompiledQueryContext c) => c.Customers.TemporalAll().Select(x => x.Name));

        _ = await compiled(context).ToListAsync();
    }

    [Fact]
    public async Task Non_temporal_compiled_query_still_works()
    {
        using var context = fixture.CreateContext();

        var compiled = EF.CompileAsyncQuery((TemporalCompiledQueryContext c) => c.Customers.Select(x => x.Name));

        _ = await compiled(context).ToListAsync();
    }
}
