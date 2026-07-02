// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class MiscellaneousTranslationsCosmosTest : MiscellaneousTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public MiscellaneousTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
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
SELECT VALUE COUNT(1)
FROM root c
WHERE ((RAND() >= 0.0) AND (RAND() < 1.0))
""");
    }

    public override async Task Random_Shared_Next_with_no_args()
    {
        await AssertTranslationFailed(() => base.Random_Shared_Next_with_no_args());

        AssertSql();
    }

    public override async Task Random_Shared_Next_with_one_arg()
    {
        await AssertTranslationFailed(() => base.Random_Shared_Next_with_one_arg());

        AssertSql();
    }

    public override async Task Random_Shared_Next_with_two_args()
    {
        await AssertTranslationFailed(() => base.Random_Shared_Next_with_two_args());

        AssertSql();
    }

    public override async Task Random_new_Next_with_no_args()
    {
        await AssertTranslationFailed(() => base.Random_new_Next_with_no_args());

        AssertSql();
    }

    public override async Task Random_new_Next_with_one_arg()
    {
        await AssertTranslationFailed(() => base.Random_new_Next_with_one_arg());

        AssertSql();
    }

    public override async Task Random_new_Next_with_two_args()
    {
        await AssertTranslationFailed(() => base.Random_new_Next_with_two_args());

        AssertSql();
    }

    #endregion Random

    #region Convert

    public override async Task Convert_ToBoolean()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToBoolean());

        AssertSql();
    }

    public override async Task Convert_ToByte()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToByte());

        AssertSql();
    }

    public override async Task Convert_ToDecimal()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToDecimal());

        AssertSql();
    }

    public override async Task Convert_ToDouble()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToDouble());

        AssertSql();
    }

    public override async Task Convert_ToInt16()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToInt16());

        AssertSql();
    }

    public override async Task Convert_ToInt32()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToInt32());

        AssertSql();
    }

    public override async Task Convert_ToInt64()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToInt64());

        AssertSql();
    }

    public override async Task Convert_ToString()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToString());

        AssertSql();
    }

    #endregion Convert

    #region Compare

    public override async Task Int_Compare_to_simple_zero()
    {
        await base.Int_Compare_to_simple_zero();

        AssertSql(
            """
@orderId='8'

SELECT VALUE c
FROM root c
WHERE (IIF((c["Int"] = @orderId), 0, IIF((c["Int"] > @orderId), 1, IIF((c["Int"] < @orderId), -1, null))) = 0)
""",
            //
            """
@orderId='8'

SELECT VALUE c
FROM root c
WHERE (0 != IIF((c["Int"] = @orderId), 0, IIF((c["Int"] > @orderId), 1, IIF((c["Int"] < @orderId), -1, null))))
""",
            //
            """
@orderId='8'

SELECT VALUE c
FROM root c
WHERE (IIF((c["Int"] = @orderId), 0, IIF((c["Int"] > @orderId), 1, IIF((c["Int"] < @orderId), -1, null))) > 0)
""",
            //
            """
@orderId='8'

SELECT VALUE c
FROM root c
WHERE (0 >= IIF((c["Int"] = @orderId), 0, IIF((c["Int"] > @orderId), 1, IIF((c["Int"] < @orderId), -1, null))))
""",
            //
            """
@orderId='8'

SELECT VALUE c
FROM root c
WHERE (0 < IIF((c["Int"] = @orderId), 0, IIF((c["Int"] > @orderId), 1, IIF((c["Int"] < @orderId), -1, null))))
""",
            //
            """
@orderId='8'

SELECT VALUE c
FROM root c
WHERE (IIF((c["Int"] = @orderId), 0, IIF((c["Int"] > @orderId), 1, IIF((c["Int"] < @orderId), -1, null))) <= 0)
""");
    }

    public override async Task DateTime_Compare_to_simple_zero(bool compareTo)
    {
        await base.DateTime_Compare_to_simple_zero(compareTo);

        AssertSql(
            """
@dateTime='1998-05-04T15:30:10'

SELECT VALUE c
FROM root c
WHERE (IIF((c["DateTime"] = @dateTime), 0, IIF((c["DateTime"] > @dateTime), 1, IIF((c["DateTime"] < @dateTime), -1, null))) = 0)
""",
            //
            """
@dateTime='1998-05-04T15:30:10'

SELECT VALUE c
FROM root c
WHERE (0 != IIF((c["DateTime"] = @dateTime), 0, IIF((c["DateTime"] > @dateTime), 1, IIF((c["DateTime"] < @dateTime), -1, null))))
""",
            //
            """
@dateTime='1998-05-04T15:30:10'

SELECT VALUE c
FROM root c
WHERE (IIF((c["DateTime"] = @dateTime), 0, IIF((c["DateTime"] > @dateTime), 1, IIF((c["DateTime"] < @dateTime), -1, null))) > 0)
""",
            //
            """
@dateTime='1998-05-04T15:30:10'

SELECT VALUE c
FROM root c
WHERE (0 >= IIF((c["DateTime"] = @dateTime), 0, IIF((c["DateTime"] > @dateTime), 1, IIF((c["DateTime"] < @dateTime), -1, null))))
""",
            //
            """
@dateTime='1998-05-04T15:30:10'

SELECT VALUE c
FROM root c
WHERE (0 < IIF((c["DateTime"] = @dateTime), 0, IIF((c["DateTime"] > @dateTime), 1, IIF((c["DateTime"] < @dateTime), -1, null))))
""",
            //
            """
@dateTime='1998-05-04T15:30:10'

SELECT VALUE c
FROM root c
WHERE (IIF((c["DateTime"] = @dateTime), 0, IIF((c["DateTime"] > @dateTime), 1, IIF((c["DateTime"] < @dateTime), -1, null))) <= 0)
""");
    }

    public override async Task TimeSpan_Compare_to_simple_zero(bool compareTo)
    {
        await base.TimeSpan_Compare_to_simple_zero(compareTo);

        AssertSql(
            """
    @timeSpan='01:02:03'
    
    SELECT VALUE c
    FROM root c
    WHERE (IIF((c["TimeSpan"] = @timeSpan), 0, IIF((c["TimeSpan"] > @timeSpan), 1, IIF((c["TimeSpan"] < @timeSpan), -1, null))) = 0)
    """,
            //
            """
    @timeSpan='01:02:03'
    
    SELECT VALUE c
    FROM root c
    WHERE (0 != IIF((c["TimeSpan"] = @timeSpan), 0, IIF((c["TimeSpan"] > @timeSpan), 1, IIF((c["TimeSpan"] < @timeSpan), -1, null))))
    """,
            //
            """
    @timeSpan='01:02:03'
    
    SELECT VALUE c
    FROM root c
    WHERE (IIF((c["TimeSpan"] = @timeSpan), 0, IIF((c["TimeSpan"] > @timeSpan), 1, IIF((c["TimeSpan"] < @timeSpan), -1, null))) > 0)
    """,
            //
            """
    @timeSpan='01:02:03'
    
    SELECT VALUE c
    FROM root c
    WHERE (0 >= IIF((c["TimeSpan"] = @timeSpan), 0, IIF((c["TimeSpan"] > @timeSpan), 1, IIF((c["TimeSpan"] < @timeSpan), -1, null))))
    """,
            //
            """
    @timeSpan='01:02:03'
    
    SELECT VALUE c
    FROM root c
    WHERE (0 < IIF((c["TimeSpan"] = @timeSpan), 0, IIF((c["TimeSpan"] > @timeSpan), 1, IIF((c["TimeSpan"] < @timeSpan), -1, null))))
    """,
            //
            """
    @timeSpan='01:02:03'
    
    SELECT VALUE c
    FROM root c
    WHERE (IIF((c["TimeSpan"] = @timeSpan), 0, IIF((c["TimeSpan"] > @timeSpan), 1, IIF((c["TimeSpan"] < @timeSpan), -1, null))) <= 0)
    """);
    }

    #endregion Compare

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
