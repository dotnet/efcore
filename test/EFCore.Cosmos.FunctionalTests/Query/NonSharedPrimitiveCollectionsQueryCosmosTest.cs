// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class NonSharedPrimitiveCollectionsQueryCosmosTest(NonSharedFixture fixture) : NonSharedPrimitiveCollectionsQueryTestBase(fixture)
{
    #region Support for specific element types

    public override async Task Array_of_string()
    {
        await base.Array_of_string();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM s IN c["SomeArray"]
    WHERE (s = "a")) = 2)
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_int()
    {
        await base.Array_of_int();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM s IN c["SomeArray"]
    WHERE (s = 1)) = 2)
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_long()
    {
        await base.Array_of_long();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM s IN c["SomeArray"]
    WHERE (s = 1)) = 2)
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_short()
    {
        await base.Array_of_short();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM s IN c["SomeArray"]
    WHERE (s = 1)) = 2)
OFFSET 0 LIMIT 2
""");
    }

    // byte[] gets mapped to base64, which isn't queryable as a regular primitive collection.
    [ConditionalFact]
    public override Task Array_of_byte()
        => AssertTranslationFailed(() => TestArray((byte)1, (byte)2));

    public override async Task Array_of_double()
    {
        await base.Array_of_double();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM s IN c["SomeArray"]
    WHERE (s = 1.0)) = 2)
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_float()
    {
        await base.Array_of_float();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM s IN c["SomeArray"]
    WHERE (s = 1.0)) = 2)
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_decimal()
    {
        await base.Array_of_decimal();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM s IN c["SomeArray"]
    WHERE (s = 1.0)) = 2)
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_DateTime()
    {
        await base.Array_of_DateTime();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM s IN c["SomeArray"]
    WHERE (s = "2023-01-01T12:30:00")) = 2)
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_DateTime_with_milliseconds()
    {
        await base.Array_of_DateTime_with_milliseconds();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM s IN c["SomeArray"]
    WHERE (s = "2023-01-01T12:30:00.123")) = 2)
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_DateTime_with_microseconds()
    {
        await base.Array_of_DateTime_with_microseconds();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM s IN c["SomeArray"]
    WHERE (s = "2023-01-01T12:30:00.123456")) = 2)
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_DateOnly()
    {
        await base.Array_of_DateOnly();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM s IN c["SomeArray"]
    WHERE (s = "2023-01-01")) = 2)
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_TimeOnly()
    {
        await base.Array_of_TimeOnly();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM s IN c["SomeArray"]
    WHERE (s = "12:30:00")) = 2)
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_TimeOnly_with_milliseconds()
    {
        await base.Array_of_TimeOnly_with_milliseconds();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM s IN c["SomeArray"]
    WHERE (s = "12:30:00.123")) = 2)
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_TimeOnly_with_microseconds()
    {
        await base.Array_of_TimeOnly_with_microseconds();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM s IN c["SomeArray"]
    WHERE (s = "12:30:00.123456")) = 2)
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_DateTimeOffset()
    {
        await base.Array_of_DateTimeOffset();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM s IN c["SomeArray"]
    WHERE (s = "2023-01-01T12:30:00+02:00")) = 2)
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_bool()
    {
        await base.Array_of_bool();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM s IN c["SomeArray"]
    WHERE (s = true)) = 2)
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_Guid()
    {
        Assert.Equal(
            CosmosStrings.ElementWithValueConverter("Guid[]", "TestEntity", "SomeArray", "Guid"),
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Array_of_Guid())).Message);

        AssertSql();
    }

    public override async Task Array_of_byte_array()
    {
        // TODO: primitive collection over value-converted element, #34153
        Assert.Equal(
            CosmosStrings.ElementWithValueConverter("byte[][]", "TestEntity", "SomeArray", "byte[]"),
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Array_of_byte_array())).Message);

        AssertSql();
    }

    public override async Task Array_of_enum()
    {
        Assert.Equal(
            CosmosStrings.ElementWithValueConverter("MyEnum[]", "TestEntity", "SomeArray", "MyEnum"),
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Array_of_enum())).Message);

        AssertSql();
    }

    [ConditionalFact]
    public override Task Multidimensional_array_is_not_supported()
        => base.Multidimensional_array_is_not_supported();

    #endregion Support for specific element types

    public override async Task Column_with_custom_converter()
    {
        await base.Column_with_custom_converter();

        AssertSql(
            """
@ints='1,2,3'

SELECT VALUE c
FROM root c
WHERE (c["Ints"] = @ints)
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Parameter_with_inferred_value_converter()
    {
        await base.Parameter_with_inferred_value_converter();

        AssertSql();
    }

    public override async Task Constant_with_inferred_value_converter()
    {
        // TODO: advanced type mapping inference for inline scalar collection, #34026
        await AssertTranslationFailed(() => base.Constant_with_inferred_value_converter());

        AssertSql();
    }

    public override async Task Inline_collection_in_query_filter()
    {
        await base.Inline_collection_in_query_filter();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM a IN (SELECT VALUE [1, 2, 3])
    WHERE (a > c["Id"])) = 1)
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Project_collection_from_entity_type_with_owned()
    {
        await base.Project_collection_from_entity_type_with_owned();

        AssertSql(
            """
SELECT VALUE c["Ints"]
FROM root c
WHERE (c["$type"] = "TestEntityWithOwned")
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(
            w => w.Ignore(CosmosEventId.NoPartitionKeyDefined));

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void ClearLog()
        => TestSqlLoggerFactory.Clear();

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);
}
