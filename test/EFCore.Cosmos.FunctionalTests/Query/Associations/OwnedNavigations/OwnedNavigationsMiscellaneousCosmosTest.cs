// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public class OwnedNavigationsMiscellaneousCosmosTest : OwnedNavigationsMiscellaneousTestBase<OwnedNavigationsCosmosFixture>
{
    public OwnedNavigationsMiscellaneousCosmosTest(OwnedNavigationsCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Where_on_associate_scalar_property()
    {
        await base.Where_on_associate_scalar_property();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["RequiredAssociate"]["Int"] = 8)
""");
    }

    public override async Task Where_on_optional_associate_scalar_property()
    {
        await base.Where_on_optional_associate_scalar_property();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["OptionalAssociate"]["Int"] = 8)
""");
    }

    public override async Task Where_on_nested_associate_scalar_property()
    {
        await base.Where_on_nested_associate_scalar_property();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["RequiredAssociate"]["RequiredNestedAssociate"]["Int"] = 8)
""");
    }

    #region GroupBy

    // TODO: GroupBy, #17313
    public override Task GroupBy_OrderByDescending_First_on_associate_scalar_property()
        => AssertTranslationFailed(base.GroupBy_OrderByDescending_First_on_associate_scalar_property);

    // TODO: GroupBy, #17313
    public override Task GroupBy_MaxBy_on_associate_scalar_property()
        => AssertTranslationFailed(base.GroupBy_MaxBy_on_associate_scalar_property);

    // TODO: GroupBy, #17313
    public override Task GroupBy_Max_on_associate_scalar_property()
        => AssertTranslationFailed(base.GroupBy_Max_on_associate_scalar_property);

    // TODO: GroupBy, #17313
    public override Task GroupBy_Count_with_predicate_on_associate_scalar_property()
        => AssertTranslationFailed(base.GroupBy_Count_with_predicate_on_associate_scalar_property);

    #endregion GroupBy

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
