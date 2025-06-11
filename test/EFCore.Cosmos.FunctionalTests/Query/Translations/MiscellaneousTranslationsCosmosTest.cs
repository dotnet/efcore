// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class MiscellaneousTranslationsCosmosTest : MiscellaneousTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public MiscellaneousTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper) : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Random

    public override Task Random_on_EF_Functions(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Random_on_EF_Functions(a);

                AssertSql(
                    """
SELECT VALUE COUNT(1)
FROM root c
WHERE ((RAND() >= 0.0) AND (RAND() < 1.0))
""");
            });

    public override async Task Random_Shared_Next_with_no_args(bool async)
    {
        await AssertTranslationFailed(() => base.Random_Shared_Next_with_no_args(async));

        AssertSql();
    }

    public override async Task Random_Shared_Next_with_one_arg(bool async)
    {
        await AssertTranslationFailed(() => base.Random_Shared_Next_with_one_arg(async));

        AssertSql();
    }

    public override async Task Random_Shared_Next_with_two_args(bool async)
    {
        await AssertTranslationFailed(() => base.Random_Shared_Next_with_two_args(async));

        AssertSql();
    }

    public override async Task Random_new_Next_with_no_args(bool async)
    {
        await AssertTranslationFailed(() => base.Random_new_Next_with_no_args(async));

        AssertSql();
    }

    public override async Task Random_new_Next_with_one_arg(bool async)
    {
        await AssertTranslationFailed(() => base.Random_new_Next_with_one_arg(async));

        AssertSql();
    }

    public override async Task Random_new_Next_with_two_args(bool async)
    {
        await AssertTranslationFailed(() => base.Random_new_Next_with_two_args(async));

        AssertSql();
    }

    #endregion Random

    #region Convert

    public override async Task Convert_ToBoolean(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToBoolean(async));

        AssertSql();
    }

    public override async Task Convert_ToByte(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToByte(async));

        AssertSql();
    }

    public override async Task Convert_ToDecimal(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToDecimal(async));

        AssertSql();
    }

    public override async Task Convert_ToDouble(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToDouble(async));

        AssertSql();
    }

    public override async Task Convert_ToInt16(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToInt16(async));

        AssertSql();
    }

    public override async Task Convert_ToInt32(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToInt32(async));

        AssertSql();
    }

    public override async Task Convert_ToInt64(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToInt64(async));

        AssertSql();
    }

    public override async Task Convert_ToString(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Convert_ToString(async));

        AssertSql();
    }

    #endregion Convert

    #region Compare

    public override async Task Int_Compare_to_simple_zero(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Int_Compare_to_simple_zero(async));

        AssertSql();
    }

    public override async Task DateTime_Compare_to_simple_zero(bool async, bool compareTo)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.DateTime_Compare_to_simple_zero(async, compareTo));

        AssertSql();
    }

    public override async Task TimeSpan_Compare_to_simple_zero(bool async, bool compareTo)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.TimeSpan_Compare_to_simple_zero(async, compareTo));

        AssertSql();
    }

    #endregion Compare

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
