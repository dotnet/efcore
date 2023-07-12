// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Query;

public class NonSharedPrimitiveCollectionsQuerySqlServerTest : NonSharedPrimitiveCollectionsQueryRelationalTestBase
{
    #region Support for specific element types

    public override async Task Array_of_int()
    {
        await base.Array_of_int();

        AssertSql(
"""
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([t].[SomeArray]) WITH ([value] int '$') AS [s]
    WHERE [s].[value] = 1) = 2
""");
    }

    public override async Task Array_of_long()
    {
        await base.Array_of_long();

        AssertSql(
"""
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([t].[SomeArray]) WITH ([value] bigint '$') AS [s]
    WHERE [s].[value] = CAST(1 AS bigint)) = 2
""");
    }

    public override async Task Array_of_short()
    {
        await base.Array_of_short();

        AssertSql(
"""
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([t].[SomeArray]) WITH ([value] smallint '$') AS [s]
    WHERE [s].[value] = CAST(1 AS smallint)) = 2
""");
    }

    public override async Task Array_of_double()
    {
        await base.Array_of_double();

        AssertSql(
"""
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([t].[SomeArray]) WITH ([value] float '$') AS [s]
    WHERE [s].[value] = 1.0E0) = 2
""");
    }

    public override async Task Array_of_float()
    {
        await base.Array_of_float();

        AssertSql(
"""
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([t].[SomeArray]) WITH ([value] real '$') AS [s]
    WHERE [s].[value] = CAST(1 AS real)) = 2
""");
    }

    public override async Task Array_of_decimal()
    {
        await base.Array_of_decimal();

        AssertSql(
"""
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([t].[SomeArray]) WITH ([value] decimal(18,2) '$') AS [s]
    WHERE [s].[value] = 1.0) = 2
""");
    }

    public override async Task Array_of_DateTime()
    {
        await base.Array_of_DateTime();

        AssertSql(
"""
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([t].[SomeArray]) WITH ([value] datetime2 '$') AS [s]
    WHERE [s].[value] = '2023-01-01T12:30:00.0000000') = 2
""");
    }

    public override async Task Array_of_DateOnly()
    {
        await base.Array_of_DateOnly();

        AssertSql(
"""
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([t].[SomeArray]) WITH ([value] date '$') AS [s]
    WHERE [s].[value] = '2023-01-01') = 2
""");
    }

    public override async Task Array_of_TimeOnly()
    {
        await base.Array_of_TimeOnly();

        AssertSql(
"""
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([t].[SomeArray]) WITH ([value] time '$') AS [s]
    WHERE [s].[value] = '12:30:00') = 2
""");
    }

    public override async Task Array_of_DateTimeOffset()
    {
        await base.Array_of_DateTimeOffset();

        AssertSql(
"""
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([t].[SomeArray]) WITH ([value] datetimeoffset '$') AS [s]
    WHERE [s].[value] = '2023-01-01T12:30:00.0000000+02:00') = 2
""");
    }

    public override async Task Array_of_bool()
    {
        await base.Array_of_bool();

        AssertSql(
"""
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([t].[SomeArray]) WITH ([value] bit '$') AS [s]
    WHERE [s].[value] = CAST(1 AS bit)) = 2
""");
    }

    public override async Task Array_of_Guid()
    {
        await base.Array_of_Guid();

        AssertSql(
"""
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([t].[SomeArray]) WITH ([value] uniqueidentifier '$') AS [s]
    WHERE [s].[value] = 'dc8c903d-d655-4144-a0fd-358099d40ae1') = 2
""");
    }

    // The JSON representation for new[] { 1, 2 } is AQI= (base64), this cannot simply be cast to varbinary(max) (0x0102). See #30727.
    public override Task Array_of_byte_array()
        => AssertTranslationFailed(() => base.Array_of_byte_array());

    public override async Task Array_of_enum()
    {
        await base.Array_of_enum();

        AssertSql(
"""
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([t].[SomeArray]) WITH ([value] int '$') AS [s]
    WHERE [s].[value] = 0) = 2
""");
    }

    [ConditionalFact] // #30630
    public override async Task Array_of_geometry_is_not_supported()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => InitializeAsync<TestContext>(
                onConfiguring: options => options.UseSqlServer(o => o.UseNetTopologySuite()),
                addServices: s => s.AddEntityFrameworkSqlServerNetTopologySuite(),
                onModelCreating: mb => mb.Entity<TestEntity>().Property<Point[]>("Points")));

        Assert.Equal(CoreStrings.PropertyNotMapped("Point[]", "TestEntity", "Points"), exception.Message);
    }

    #endregion Support for specific element types

    #region Type mapping inference

    public override async Task Constant_with_inferred_value_converter()
    {
        await base.Constant_with_inferred_value_converter();

        AssertSql(
"""
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[PropertyWithValueConverter]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (VALUES (CAST(1 AS int)), (8)) AS [v]([Value])
    WHERE [v].[Value] = [t].[PropertyWithValueConverter]) = 1
""");
    }

    public override async Task Inline_collection_in_query_filter()
    {
        await base.Inline_collection_in_query_filter();

        AssertSql(
"""
SELECT TOP(2) [t].[Id], [t].[Ints]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (VALUES (CAST(1 AS int)), (2), (3)) AS [v]([Value])
    WHERE [v].[Value] > [t].[Id]) = 1
""");
    }

    public override async Task Column_collection_inside_json_owned_entity()
    {
        await base.Column_collection_inside_json_owned_entity();

        AssertSql(
"""
SELECT TOP(2) [t].[Id], [t].[Owned]
FROM [TestOwner] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON(JSON_VALUE([t].[Owned], '$.Strings')) AS [s]) = 2
""",
            //
"""
SELECT TOP(2) [t].[Id], [t].[Owned]
FROM [TestOwner] AS [t]
WHERE JSON_VALUE(JSON_VALUE([t].[Owned], '$.Strings'), '$[1]') = N'bar'
""");
    }

    [ConditionalFact]
    public virtual async Task Same_parameter_with_different_type_mappings()
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onModelCreating: mb => mb.Entity<TestEntity>(
                b =>
                {
                    b.Property(typeof(DateTime), "DateTime").HasColumnType("datetime");
                    b.Property(typeof(DateTime), "DateTime2").HasColumnType("datetime2");
                }));

        await using var context = contextFactory.CreateContext();

        var dateTimes = new[] { new DateTime(2020, 1, 1, 12, 30, 00), new DateTime(2020, 1, 2, 12, 30, 00) };

        _ = await context.Set<TestEntity>()
            .Where(
                m =>
                    dateTimes.Contains(EF.Property<DateTime>(m, "DateTime"))
                    && dateTimes.Contains(EF.Property<DateTime>(m, "DateTime2")))
            .ToArrayAsync();

        AssertSql(
"""
@__dateTimes_0='["2020-01-01T12:30:00","2020-01-02T12:30:00"]' (Size = 4000)
@__dateTimes_0_1='["2020-01-01T12:30:00","2020-01-02T12:30:00"]' (Size = 4000)

SELECT [t].[Id], [t].[DateTime], [t].[DateTime2], [t].[Ints]
FROM [TestEntity] AS [t]
WHERE [t].[DateTime] IN (
    SELECT [d].[value]
    FROM OPENJSON(@__dateTimes_0) WITH ([value] datetime '$') AS [d]
) AND [t].[DateTime2] IN (
    SELECT [d0].[value]
    FROM OPENJSON(@__dateTimes_0_1) WITH ([value] datetime2 '$') AS [d0]
)
""");
    }

    [ConditionalFact]
    public virtual async Task Same_collection_with_default_type_mapping_and_uninferrable_context()
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onModelCreating: mb => mb.Entity<TestEntity>(b => b.Property(typeof(DateTime), "DateTime")));

        await using var context = contextFactory.CreateContext();

        var dateTimes = new DateTime?[] { new DateTime(2020, 1, 1, 12, 30, 00), new DateTime(2020, 1, 2, 12, 30, 00), null };

        _ = await context.Set<TestEntity>()
            .Where(m => dateTimes.Any(d => d == EF.Property<DateTime>(m, "DateTime") && d != null))
            .ToArrayAsync();

        AssertSql(
"""
@__dateTimes_0='["2020-01-01T12:30:00","2020-01-02T12:30:00",null]' (Size = 4000)

SELECT [t].[Id], [t].[DateTime], [t].[Ints]
FROM [TestEntity] AS [t]
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON(@__dateTimes_0) WITH ([value] datetime2 '$') AS [d]
    WHERE [d].[value] = [t].[DateTime] AND [d].[value] IS NOT NULL)
""");
    }

    [ConditionalFact]
    public virtual async Task Same_collection_with_non_default_type_mapping_and_uninferrable_context()
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onModelCreating: mb => mb.Entity<TestEntity>(
                b => b.Property(typeof(DateTime), "DateTime").HasColumnType("datetime")));

        await using var context = contextFactory.CreateContext();

        var dateTimes = new DateTime?[] { new DateTime(2020, 1, 1, 12, 30, 00), new DateTime(2020, 1, 2, 12, 30, 00), null };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Set<TestEntity>()
                .Where(
                    m => dateTimes.Any(d => d == EF.Property<DateTime>(m, "DateTime") && d != null))
                .ToArrayAsync());
        Assert.Equal(RelationalStrings.ConflictingTypeMappingsInferredForColumn("value"), exception.Message);
    }

    [ConditionalFact]
    public virtual async Task Same_collection_with_conflicting_type_mappings_not_supported()
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onModelCreating: mb => mb.Entity<TestEntity>(
                b =>
                {
                    b.Property(typeof(DateTime), "DateTime").HasColumnType("datetime");
                    b.Property(typeof(DateTime), "DateTime2").HasColumnType("datetime2");
                }));

        await using var context = contextFactory.CreateContext();

        var dateTimes = new[] { new DateTime(2020, 1, 1, 12, 30, 00), new DateTime(2020, 1, 2, 12, 30, 00) };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Set<TestEntity>()
                .Where(
                    m => dateTimes
                        .Any(d => d == EF.Property<DateTime>(m, "DateTime") && d == EF.Property<DateTime>(m, "DateTime2")))
                .ToArrayAsync());
        Assert.Equal(RelationalStrings.ConflictingTypeMappingsInferredForColumn("value"), exception.Message);
    }

    #endregion Type mapping inference

    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;
}
