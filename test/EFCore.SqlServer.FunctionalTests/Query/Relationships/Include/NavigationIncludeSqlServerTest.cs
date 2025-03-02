// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Include;

public class NavigationIncludeSqlServerTest
    : NavigationIncludeRelationalTestBase<NavigationRelationshipsSqlServerFixture>
{
    public NavigationIncludeSqlServerTest(NavigationRelationshipsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Include_trunk_optional(bool async)
    {
        await base.Include_trunk_optional(async);

        AssertSql(
"""
SELECT [r].[Id], [r].[Name], [r].[OptionalReferenceTrunkId], [r].[RequiredReferenceTrunkId], [t].[Id], [t].[CollectionRootId], [t].[Name], [t].[OptionalReferenceBranchId], [t].[RequiredReferenceBranchId]
FROM [RootEntities] AS [r]
LEFT JOIN [TrunkEntities] AS [t] ON [r].[OptionalReferenceTrunkId] = [t].[Id]
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
