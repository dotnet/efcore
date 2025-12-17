// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations.Operators;

public class MiscellaneousOperatorTranslationsSqlServerTest : MiscellaneousOperatorTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public MiscellaneousOperatorTranslationsSqlServerTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Conditional()
    {
        await base.Conditional();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (((c["Int"] = 8) ? c["String"] : "Foo") = "Seattle")
""");
    }

    public override async Task Coalesce()
    {
        await base.Coalesce();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (((c["String"] != null) ? c["String"] : "Unknown") = "Seattle")
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
