// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class GuidTranslationsCosmosTest : GuidTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public GuidTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper) : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task New_with_constant(bool async)
    {
        await base.New_with_constant(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Guid"] = "df36f493-463f-4123-83f9-6b135deeb7ba")
""");
    }

    public override async Task New_with_parameter(bool async)
    {
        await base.New_with_parameter(async);

        AssertSql(
            """
@p=?

SELECT VALUE c
FROM root c
WHERE (c["Guid"] = @p)
""");
    }

    public override async Task ToString_projection(bool async)
    {
        await base.ToString_projection(async);

        AssertSql(
            """
SELECT VALUE c["Guid"]
FROM root c
""");
    }

    public override async Task NewGuid(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.NewGuid(async));

        AssertSql();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
