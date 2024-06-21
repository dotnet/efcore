// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class NonSharedPrimitiveCollectionsQueryCosmosTest : NonSharedPrimitiveCollectionsQueryTestBase
{
    #region Support for specific element types

    public override async Task Array_of_string()
    {
        await base.Array_of_string();

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "TestEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN c["SomeArray"]
    WHERE (i = "a")) = 2))
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_int()
    {
        await base.Array_of_int();

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "TestEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN c["SomeArray"]
    WHERE (i = 1)) = 2))
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_long()
    {
        await base.Array_of_long();

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "TestEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN c["SomeArray"]
    WHERE (i = 1)) = 2))
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_short()
    {
        await base.Array_of_short();

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "TestEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN c["SomeArray"]
    WHERE (i = 1)) = 2))
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_byte()
    {
        // TODO
        await Assert.ThrowsAsync<InvalidOperationException>(() => base.Array_of_byte());

        AssertSql();
    }

    public override async Task Array_of_double()
    {
        await base.Array_of_double();

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "TestEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN c["SomeArray"]
    WHERE (i = 1.0)) = 2))
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_float()
    {
        await base.Array_of_float();

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "TestEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN c["SomeArray"]
    WHERE (i = 1.0)) = 2))
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_decimal()
    {
        await base.Array_of_decimal();

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "TestEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN c["SomeArray"]
    WHERE (i = 1.0)) = 2))
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_DateTime()
    {
        await base.Array_of_DateTime();

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "TestEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN c["SomeArray"]
    WHERE (i = "2023-01-01T12:30:00")) = 2))
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_DateTime_with_milliseconds()
    {
        await base.Array_of_DateTime_with_milliseconds();

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "TestEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN c["SomeArray"]
    WHERE (i = "2023-01-01T12:30:00.123")) = 2))
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_DateTime_with_microseconds()
    {
        await base.Array_of_DateTime_with_microseconds();

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "TestEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN c["SomeArray"]
    WHERE (i = "2023-01-01T12:30:00.123456")) = 2))
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_DateOnly()
    {
        await base.Array_of_DateOnly();

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "TestEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN c["SomeArray"]
    WHERE (i = "2023-01-01")) = 2))
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_TimeOnly()
    {
        await base.Array_of_TimeOnly();

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "TestEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN c["SomeArray"]
    WHERE (i = "12:30:00")) = 2))
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_TimeOnly_with_milliseconds()
    {
        await base.Array_of_TimeOnly_with_milliseconds();

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "TestEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN c["SomeArray"]
    WHERE (i = "12:30:00.123")) = 2))
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_TimeOnly_with_microseconds()
    {
        await base.Array_of_TimeOnly_with_microseconds();

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "TestEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN c["SomeArray"]
    WHERE (i = "12:30:00.123456")) = 2))
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_DateTimeOffset()
    {
        await base.Array_of_DateTimeOffset();

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "TestEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN c["SomeArray"]
    WHERE (i = "2023-01-01T12:30:00+02:00")) = 2))
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_bool()
    {
        await base.Array_of_bool();

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "TestEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN c["SomeArray"]
    WHERE (i = true)) = 2))
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_Guid()
    {
        await base.Array_of_Guid();

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "TestEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN c["SomeArray"]
    WHERE (i = "dc8c903d-d655-4144-a0fd-358099d40ae1")) = 2))
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_byte_array()
    {
        // TODO
        await Assert.ThrowsAsync<InvalidOperationException>(() => base.Array_of_byte_array());

        AssertSql("""
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "TestEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN c["SomeArray"]
    WHERE (i = "AQI=")) = 2))
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Array_of_enum()
    {
        await base.Array_of_enum();

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "TestEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN c["SomeArray"]
    WHERE (i = 0)) = 2))
OFFSET 0 LIMIT 2
""");
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
@__ints_0='1,2,3'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "TestEntity") AND (c["Ints"] = @__ints_0))
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
        // TODO
        await Assert.ThrowsAsync<InvalidOperationException>(() => base.Constant_with_inferred_value_converter());

        AssertSql();
    }

    public override async Task Inline_collection_in_query_filter()
    {
        await base.Inline_collection_in_query_filter();

        AssertSql(
            """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "TestEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN (SELECT VALUE [1, 2, 3])
    WHERE (i > c["Id"])) = 1))
OFFSET 0 LIMIT 2
""");
    }

    public override async Task Project_collection_from_entity_type_with_owned()
    {
        await base.Project_collection_from_entity_type_with_owned();

        AssertSql(
            """
SELECT c["Ints"]
FROM root c
WHERE (c["Discriminator"] = "TestEntityWithOwned")
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
