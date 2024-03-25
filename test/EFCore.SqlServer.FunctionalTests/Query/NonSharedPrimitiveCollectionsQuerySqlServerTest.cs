// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

using static Expression;

public class NonSharedPrimitiveCollectionsQuerySqlServerTest : NonSharedPrimitiveCollectionsQueryRelationalTestBase
{
    #region Support for specific element types

    public override async Task Array_of_string()
    {
        await base.Array_of_string();

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([t].[SomeArray]) WITH ([value] nvarchar(max) '$') AS [s]
    WHERE [s].[value] = N'a') = 2
""");
    }

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

    [ConditionalFact]
    public override Task Array_of_byte()
        => base.Array_of_byte();

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

    public override async Task Array_of_DateTime_with_milliseconds()
    {
        await base.Array_of_DateTime_with_milliseconds();

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([t].[SomeArray]) WITH ([value] datetime2 '$') AS [s]
    WHERE [s].[value] = '2023-01-01T12:30:00.1230000') = 2
""");
    }

    public override async Task Array_of_DateTime_with_microseconds()
    {
        await base.Array_of_DateTime_with_microseconds();

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([t].[SomeArray]) WITH ([value] datetime2 '$') AS [s]
    WHERE [s].[value] = '2023-01-01T12:30:00.1234560') = 2
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

    public override async Task Array_of_TimeOnly_with_milliseconds()
    {
        await base.Array_of_TimeOnly_with_milliseconds();

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([t].[SomeArray]) WITH ([value] time '$') AS [s]
    WHERE [s].[value] = '12:30:00.123') = 2
""");
    }

    public override async Task Array_of_TimeOnly_with_microseconds()
    {
        await base.Array_of_TimeOnly_with_microseconds();

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([t].[SomeArray]) WITH ([value] time '$') AS [s]
    WHERE [s].[value] = '12:30:00.123456') = 2
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

    public override async Task Array_of_byte_array()
    {
        await base.Array_of_byte_array();

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([t].[SomeArray]) WITH ([value] varbinary(max) '$') AS [s]
    WHERE [s].[value] = 0x0102) = 2
""");
    }

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

    [ConditionalFact]
    public override Task Array_of_array_is_not_supported()
        => base.Array_of_array_is_not_supported();

    [ConditionalFact]
    public override Task Multidimensional_array_is_not_supported()
        => base.Multidimensional_array_is_not_supported();

    #endregion Support for specific element types

    #region Specific element types in ordered context

    // When we don't need to preserve the collection's ordering (e.g. when Contains/Count is composed on top of it), we use OPENJSON with
    // WITH, which handles all conversions out of JSON well.
    // However, OPENJSON with WITH doesn't support preserving the ordering, so when that's needed we switch to OPENJSON without WITH, at
    // which point we need to manually convert JSON values into their relational counterparts (this isn't always possible, e.g. varbinary
    // which is base64 in JSON).
    // The regular element type tests above test in unordered context, so we repeat them here but with an order-preserving context.

    [ConditionalFact]
    public virtual async Task Ordered_array_of_string()
    {
        await TestOrderedArray("a", "b");

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [s].[value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = N'a') = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_int()
    {
        await TestOrderedArray(1, 2);

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS int) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = 1) = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_long()
    {
        await TestOrderedArray(1L, 2L);

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS bigint) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = CAST(1 AS bigint)) = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_short()
    {
        await TestOrderedArray((short)1, (short)2);

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS smallint) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = CAST(1 AS smallint)) = 2
""");
    }

    // On relational databases, byte[] gets mapped to a special binary data type, which isn't queryable as a regular primitive collection.
    [ConditionalFact]
    public virtual async Task Ordered_array_of_byte()
        => await AssertTranslationFailed(() => TestOrderedArray((byte)1, (byte)2));

    [ConditionalFact]
    public virtual async Task Ordered_array_of_double()
    {
        await TestOrderedArray(1d, 2d);

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS float) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = 1.0E0) = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_float()
    {
        await TestOrderedArray(1f, 2f);

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS real) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = CAST(1 AS real)) = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_decimal()
    {
        await TestOrderedArray(1m, 2m);

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS decimal(18,2)) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = 1.0) = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_DateTime()
    {
        await TestOrderedArray(new DateTime(2023, 1, 1, 12, 30, 0), new DateTime(2023, 1, 2, 12, 30, 0));

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS datetime2) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = '2023-01-01T12:30:00.0000000') = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_DateOnly()
    {
        await TestOrderedArray(new DateOnly(2023, 1, 1), new DateOnly(2023, 1, 2));

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS date) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = '2023-01-01') = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_TimeOnly()
    {
        await TestOrderedArray(new TimeOnly(12, 30, 0), new TimeOnly(12, 30, 1));

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS time) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = '12:30:00') = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_DateTimeOffset()
    {
        await TestOrderedArray(
            new DateTimeOffset(2023, 1, 1, 12, 30, 0, TimeSpan.FromHours(2)),
            new DateTimeOffset(2023, 1, 2, 12, 30, 0, TimeSpan.FromHours(2)));

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS datetimeoffset) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = '2023-01-01T12:30:00.0000000+02:00') = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_bool()
    {
        await TestOrderedArray(true, false);

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS bit) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = CAST(1 AS bit)) = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_guid()
    {
        await TestOrderedArray(
            new Guid("dc8c903d-d655-4144-a0fd-358099d40ae1"),
            new Guid("008719a5-1999-4798-9cf3-92a78ffa94a2"));

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS uniqueidentifier) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = 'dc8c903d-d655-4144-a0fd-358099d40ae1') = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_byte_array()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => TestOrderedArray([1, 2], new byte[] { 3, 4 }));

        Assert.Equal(SqlServerStrings.QueryingOrderedBinaryJsonCollectionsNotSupported, exception.Message);
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_enum()
    {
        await TestOrderedArray(MyEnum.Label1, MyEnum.Label2);

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS int) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = 0) = 2
""");
    }

    private enum MyEnum { Label1, Label2 }

    private async Task TestOrderedArray<TElement>(
        TElement value1,
        TElement value2,
        Action<ModelBuilder> onModelCreating = null)
    {
        var arrayClrType = typeof(TElement).MakeArrayType();

        var contextFactory = await InitializeAsync<TestContext>(
            onModelCreating: onModelCreating ?? (mb => mb.Entity<TestEntity>().Property(arrayClrType, "SomeArray")),
            seed: context =>
            {
                var instance1 = new TestEntity { Id = 1 };
                context.Add(instance1);
                var array1 = new TElement[3];
                array1.SetValue(value1, 0); // We have an extra copy of the first value which we'll Skip, to preserve the ordering
                array1.SetValue(value1, 1);
                array1.SetValue(value1, 2);
                context.Entry(instance1).Property("SomeArray").CurrentValue = array1;

                var instance2 = new TestEntity { Id = 2 };
                context.Add(instance2);
                var array2 = new TElement[3];
                array2.SetValue(value1, 0);
                array2.SetValue(value1, 1);
                array2.SetValue(value2, 2);
                context.Entry(instance2).Property("SomeArray").CurrentValue = array2;

                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var entityParam = Parameter(typeof(TestEntity), "m");
        var efPropertyCall = Call(
            typeof(EF).GetMethod(nameof(EF.Property), BindingFlags.Public | BindingFlags.Static)!.MakeGenericMethod(arrayClrType),
            entityParam,
            Constant("SomeArray"));

        var elementParam = Parameter(typeof(TElement), "a");
        var predicate = Lambda<Func<TestEntity, bool>>(
            Equal(
                Call(
                    CountWithPredicateMethod.MakeGenericMethod(typeof(TElement)),
                    Call(
                        SkipMethod.MakeGenericMethod(typeof(TElement)),
                        efPropertyCall,
                        Constant(1)),
                    Lambda(Equal(elementParam, Constant(value1)), elementParam)),
                Constant(2)),
            entityParam);

        // context.Set<TestEntity>().SingleAsync(m => EF.Property<int[]>(m, "SomeArray").Skip(1).Count(a => a == <value1>) == 2)
        var result = await context.Set<TestEntity>().SingleAsync(predicate);
        Assert.Equal(1, result.Id);
    }

    private static readonly MethodInfo CountWithPredicateMethod
        = typeof(Enumerable).GetRuntimeMethods().Single(m => m.Name == nameof(Enumerable.Count) && m.GetParameters().Length == 2);

    private static readonly MethodInfo SkipMethod
        = typeof(Enumerable).GetRuntimeMethods().Single(m => m.Name == nameof(Enumerable.Skip) && m.GetParameters().Length == 2);

    #endregion

    [ConditionalFact]
    public override Task Column_with_custom_converter()
        => base.Column_with_custom_converter();

    public override async Task Parameter_with_inferred_value_converter()
    {
        await base.Parameter_with_inferred_value_converter();

        AssertSql("");
    }

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

    [ConditionalFact]
    public virtual async Task Ordered_collection_with_split_query()
    {
        var contextFactory = await InitializeAsync<Context32976>(
            onModelCreating: mb => mb.Entity<Context32976.Principal>(),
            seed: context =>
            {
                context.Add(new Context32976.Principal { Ints = [2, 3, 4] });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        _ = await context.Set<Context32976.Principal>()
            .Where(p => p.Ints.Skip(1).Contains(3))
            .Include(p => p.Dependents)
            .AsSplitQuery()
            .SingleAsync();
    }

    public class Context32976(DbContextOptions options) : DbContext(options)
    {
        public class Principal
        {
            public int Id { get; set; }
            public List<int> Ints { get; set; }
            public List<Dependent> Dependents { get; set; }
        }

        public class Dependent
        {
            public int Id { get; set; }
            public Principal Principal { get; set; }
        }
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;
}
