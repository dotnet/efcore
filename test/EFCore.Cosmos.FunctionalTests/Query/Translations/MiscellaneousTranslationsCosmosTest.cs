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

    #region Guid

    public override Task Guid_new_with_constant(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Guid_new_with_constant(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Guid"] = "df36f493-463f-4123-83f9-6b135deeb7ba")
""");
            });

    public override Task Guid_new_with_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Guid_new_with_parameter(a);

                AssertSql(
                    """
@p=?

SELECT VALUE c
FROM root c
WHERE (c["Guid"] = @p)
""");
            });

    public override Task Guid_ToString_projection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Guid_ToString_projection(a);

                AssertSql(
                    """
SELECT VALUE c["Guid"]
FROM root c
""");
            });

    public override async Task Guid_NewGuid(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Guid_NewGuid(async));

        AssertSql();
    }

    #endregion Guid

    #region Byte array

    public override Task Byte_array_Length(bool async)
        => AssertTranslationFailed(() => base.Byte_array_Length(async));

    public override Task Byte_array_array_index(bool async)
        => AssertTranslationFailed(() => base.Byte_array_array_index(async));

    public override Task Byte_array_First(bool async)
        => AssertTranslationFailed(() => base.Byte_array_First(async));

    #endregion Byte array

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
