// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public class OwnedNavigationsPrimitiveCollectionCosmosTest : OwnedNavigationsPrimitiveCollectionTestBase<OwnedNavigationsCosmosFixture>
{
    public OwnedNavigationsPrimitiveCollectionCosmosTest(OwnedNavigationsCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Count()
    {
        await base.Count();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(c["RequiredAssociate"]["Ints"]) = 3)
""");
    }

    public override async Task Index()
    {
        await base.Index();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["RequiredAssociate"]["Ints"][0] = 1)
""");
    }

    public override async Task Contains()
    {
        await base.Contains();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(c["RequiredAssociate"]["Ints"], 3)
""");
    }

    public override async Task Any_predicate()
    {
        await base.Any_predicate();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(c["RequiredAssociate"]["Ints"], 2)
""");
    }

    public override async Task Nested_Count()
    {
        await base.Nested_Count();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(c["RequiredAssociate"]["RequiredNestedAssociate"]["Ints"]) = 3)
""");
    }

    public override async Task Select_Sum()
    {
        await base.Select_Sum();

        AssertSql(
            """
SELECT VALUE (
    SELECT VALUE SUM(i0)
    FROM i0 IN c["RequiredAssociate"]["Ints"])
FROM root c
WHERE ((
    SELECT VALUE SUM(i)
    FROM i IN c["RequiredAssociate"]["Ints"]) >= 6)
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
