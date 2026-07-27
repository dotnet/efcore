// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class TimeOnlyTranslationsSqliteTest : TimeOnlyTranslationsTestBase<BasicTypesQuerySqliteFixture>
{
    public TimeOnlyTranslationsSqliteTest(BasicTypesQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Hour()
    {
        await base.Hour();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%H', "b"."TimeOnly") AS INTEGER) = 15
""");
    }

    public override async Task Minute()
    {
        await base.Minute();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%M', "b"."TimeOnly") AS INTEGER) = 30
""");
    }

    public override async Task Second()
    {
        await base.Second();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%S', "b"."TimeOnly") AS INTEGER) = 10
""");
    }

    public override async Task Millisecond()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Millisecond());

        AssertSql();
    }

    public override async Task Microsecond()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Microsecond());

        AssertSql();
    }

    public override async Task Nanosecond()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Nanosecond());

        AssertSql();
    }

    public override async Task AddHours()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.AddHours());

        AssertSql();
    }

    public override async Task AddMinutes()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.AddMinutes());

        AssertSql();
    }

    public override async Task Add_TimeSpan()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Add_TimeSpan());

        AssertSql();
    }

    public override async Task IsBetween()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.IsBetween());

        AssertSql();
    }

    public override async Task Subtract()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.Subtract());

        AssertSql();
    }

    public override async Task FromDateTime_compared_to_property()
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.FromDateTime_compared_to_property());

        AssertSql();
    }

    public override async Task FromDateTime_compared_to_parameter()
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.FromDateTime_compared_to_parameter());

        AssertSql();
    }

    public override async Task FromDateTime_compared_to_constant()
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.FromDateTime_compared_to_constant());

        AssertSql();
    }

    public override async Task FromTimeSpan_compared_to_property()
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.FromTimeSpan_compared_to_property());

        AssertSql();
    }

    public override async Task FromTimeSpan_compared_to_parameter()
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.FromTimeSpan_compared_to_parameter());

        AssertSql();
    }

    public override async Task Order_by_FromTimeSpan()
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.Order_by_FromTimeSpan());

        AssertSql();
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
