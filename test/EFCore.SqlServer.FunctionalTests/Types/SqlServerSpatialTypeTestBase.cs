// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types;

using static System.Linq.Expressions.Expression;

public abstract class SqlServerSpatialTypeTestBase<T, TFixture>(TFixture fixture, ITestOutputHelper testOutputHelper)
    : RelationalTypeTestBase<T, TFixture>(fixture, testOutputHelper)
    where T : NetTopologySuite.Geometries.Geometry
    where TFixture : SqlServerSpatialTypeTestBase<T, TFixture>.SqlServerSpatialTypeFixture
{
    private static readonly MethodInfo EqualsTopologicallyMethod =
        typeof(NetTopologySuite.Geometries.Geometry).GetMethod(
            nameof(NetTopologySuite.Geometries.Geometry.EqualsTopologically),
            [typeof(NetTopologySuite.Geometries.Geometry)])!;

    // SQL Server doesn't support the equality operator on geometry, override to use EqualsTopologically
    public override async Task Equality_in_query_with_parameter()
    {
        await using var context = Fixture.CreateContext();

        Fixture.TestSqlLoggerFactory.Clear();

        var result = await context.Set<TypeEntity<T>>().Where(e => e.Value.EqualsTopologically(Fixture.Value)).SingleAsync();

        Assert.Equal(Fixture.Value, result.Value, Fixture.Comparer);
    }

    // SQL Server doesn't support the equality operator on geometry, override to use EqualsTopologically
    public override async Task Equality_in_query_with_constant()
    {
        await using var context = Fixture.CreateContext();

        var entityParameter = Parameter(typeof(TypeEntity<T>), "e");
        var predicate =
            Lambda<Func<TypeEntity<T>, bool>>(
                Call(
                    Property(entityParameter, nameof(TypeEntity<>.Value)),
                    EqualsTopologicallyMethod,
                    Constant(Fixture.Value)),
                entityParameter);

        var result = await context.Set<TypeEntity<T>>().Where(predicate).SingleAsync();

        Assert.Equal(Fixture.Value, result.Value, Fixture.Comparer);
    }

    // SQL Server doesn't support the equality operator on geometry, override to use EqualsTopologically
    public override async Task Query_property_within_json()
    {
        await using var context = Fixture.CreateContext();

        Fixture.TestSqlLoggerFactory.Clear();

        var result = await context.Set<JsonTypeEntity<T>>().Where(e => e.JsonContainer.Value.EqualsTopologically(Fixture.Value)).SingleAsync();

        Assert.Equal(Fixture.Value, result.JsonContainer.Value, Fixture.Comparer);
    }

    public abstract class SqlServerSpatialTypeFixture : SqlServerTypeFixture<T>
    {
        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).UseSqlServer(o => o.UseNetTopologySuite());
    }
}





