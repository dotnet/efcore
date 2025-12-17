// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class MiscellaneousTranslationsSqliteTest : MiscellaneousTranslationsRelationalTestBase<BasicTypesQuerySqliteFixture>
{
    public MiscellaneousTranslationsSqliteTest(BasicTypesQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Random

    public override async Task Random_on_EF_Functions()
    {
        await base.Random_on_EF_Functions();

        AssertSql(
            """
SELECT COUNT(*)
FROM "BasicTypesEntities" AS "b"
WHERE abs(random() / 9.2233720368547799E+18) >= 0.0 AND abs(random() / 9.2233720368547799E+18) < 1.0
""");
    }

    public override async Task Random_Shared_Next_with_no_args()
    {
        await base.Random_Shared_Next_with_no_args();

        AssertSql();
    }

    public override async Task Random_Shared_Next_with_one_arg()
    {
        await base.Random_Shared_Next_with_one_arg();

        AssertSql();
    }

    public override async Task Random_Shared_Next_with_two_args()
    {
        await base.Random_Shared_Next_with_two_args();

        AssertSql();
    }

    public override async Task Random_new_Next_with_no_args()
    {
        await base.Random_new_Next_with_no_args();

        AssertSql();
    }

    public override async Task Random_new_Next_with_one_arg()
    {
        await base.Random_new_Next_with_one_arg();

        AssertSql();
    }

    public override async Task Random_new_Next_with_two_args()
    {
        await base.Random_new_Next_with_two_args();

        AssertSql();
    }

    #endregion Random

    #region Convert

    public override Task Convert_ToBoolean()
        => AssertTranslationFailed(() => base.Convert_ToBoolean());

    public override Task Convert_ToByte()
        => AssertTranslationFailed(() => base.Convert_ToByte());

    public override Task Convert_ToDecimal()
        => AssertTranslationFailed(() => base.Convert_ToDecimal());

    public override Task Convert_ToDouble()
        => AssertTranslationFailed(() => base.Convert_ToDouble());

    public override Task Convert_ToInt16()
        => AssertTranslationFailed(() => base.Convert_ToInt16());

    public override Task Convert_ToInt32()
        => AssertTranslationFailed(() => base.Convert_ToInt32());

    public override Task Convert_ToInt64()
        => AssertTranslationFailed(() => base.Convert_ToInt64());

    public override Task Convert_ToString()
        => AssertTranslationFailed(() => base.Convert_ToString());

    #endregion Convert

    #region Compare

    public override async Task Int_Compare_to_simple_zero()
    {
        await base.Int_Compare_to_simple_zero();

        AssertSql(
            """
@orderId='8'

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."Int" = @orderId
""",
            //
            """
@orderId='8'

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."Int" <> @orderId
""",
            //
            """
@orderId='8'

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."Int" > @orderId
""",
            //
            """
@orderId='8'

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."Int" <= @orderId
""",
            //
            """
@orderId='8'

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."Int" > @orderId
""",
            //
            """
@orderId='8'

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."Int" <= @orderId
""");
    }

    public override async Task DateTime_Compare_to_simple_zero(bool compareTo)
    {
        await base.DateTime_Compare_to_simple_zero(compareTo);

        AssertSql(
            """
@dateTime='1998-05-04T15:30:10.0000000' (DbType = DateTime)

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."DateTime" = @dateTime
""",
            //
            """
@dateTime='1998-05-04T15:30:10.0000000' (DbType = DateTime)

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."DateTime" <> @dateTime
""",
            //
            """
@dateTime='1998-05-04T15:30:10.0000000' (DbType = DateTime)

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."DateTime" > @dateTime
""",
            //
            """
@dateTime='1998-05-04T15:30:10.0000000' (DbType = DateTime)

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."DateTime" <= @dateTime
""",
            //
            """
@dateTime='1998-05-04T15:30:10.0000000' (DbType = DateTime)

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."DateTime" > @dateTime
""",
            //
            """
@dateTime='1998-05-04T15:30:10.0000000' (DbType = DateTime)

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."DateTime" <= @dateTime
""");
    }

    public override async Task TimeSpan_Compare_to_simple_zero(bool compareTo)
    {
        await base.TimeSpan_Compare_to_simple_zero(compareTo);

        AssertSql(
            """
@timeSpan='01:02:03'

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."TimeSpan" = @timeSpan
""",
            //
            """
@timeSpan='01:02:03'

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."TimeSpan" <> @timeSpan
""",
            //
            """
@timeSpan='01:02:03'

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."TimeSpan" > @timeSpan
""",
            //
            """
@timeSpan='01:02:03'

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."TimeSpan" <= @timeSpan
""",
            //
            """
@timeSpan='01:02:03'

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."TimeSpan" > @timeSpan
""",
            //
            """
@timeSpan='01:02:03'

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."TimeSpan" <= @timeSpan
""");
    }

    #endregion Compare

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
