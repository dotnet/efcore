﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

[SqlServerCondition(SqlServerCondition.SupportsTemporalTablesCascadeDelete)]
public class TemporalOwnedQuerySqlServerTest : OwnedQueryRelationalTestBase<
    TemporalOwnedQuerySqlServerTest.TemporalOwnedQuerySqlServerFixture>
{
    public TemporalOwnedQuerySqlServerTest(TemporalOwnedQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override bool CanExecuteQueryString
        => true;

    protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
    {
        serverQueryExpression = base.RewriteServerQueryExpression(serverQueryExpression);

        var temporalEntityTypes = new List<Type>
        {
            typeof(OwnedPerson),
            typeof(Branch),
            typeof(LeafA),
            typeof(LeafB),
            typeof(Barton),
            typeof(Star),
            typeof(Planet),
            typeof(Moon),
        };

        var rewriter = new TemporalPointInTimeQueryRewriter(Fixture.ChangesDate, temporalEntityTypes);

        return rewriter.Visit(serverQueryExpression);
    }

    public override async Task Query_with_owned_entity_equality_operator(bool async)
    {
        await base.Query_with_owned_entity_equality_operator(async);

        AssertSql(
"""
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [t].[Id], [t0].[ClientId], [t0].[Id], [t0].[OrderDate], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[OrderClientId], [t0].[OrderId], [t0].[Id0], [t0].[Detail], [t0].[PeriodEnd0], [t0].[PeriodStart0], [o].[PersonAddress_AddressLine], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
CROSS JOIN (
    SELECT [o0].[Id]
    FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    WHERE [o0].[Discriminator] = N'LeafB'
) AS [t]
LEFT JOIN (
    SELECT [o1].[ClientId], [o1].[Id], [o1].[OrderDate], [o1].[PeriodEnd], [o1].[PeriodStart], [o2].[OrderClientId], [o2].[OrderId], [o2].[Id] AS [Id0], [o2].[Detail], [o2].[PeriodEnd] AS [PeriodEnd0], [o2].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o2] ON [o1].[ClientId] = [o2].[OrderClientId] AND [o1].[Id] = [o2].[OrderId]
) AS [t0] ON [o].[Id] = [t0].[ClientId]
WHERE 0 = 1
ORDER BY [o].[Id], [t].[Id], [t0].[ClientId], [t0].[Id], [t0].[OrderClientId], [t0].[OrderId]
""");
    }

    public override async Task Query_for_base_type_loads_all_owned_navs(bool async)
    {
        await base.Query_for_base_type_loads_all_owned_navs(async);

        AssertSql(
"""
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [t].[ClientId], [t].[Id], [t].[OrderDate], [t].[PeriodEnd], [t].[PeriodStart], [t].[OrderClientId], [t].[OrderId], [t].[Id0], [t].[Detail], [t].[PeriodEnd0], [t].[PeriodStart0], [o].[PersonAddress_AddressLine], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [t] ON [o].[Id] = [t].[ClientId]
ORDER BY [o].[Id], [t].[ClientId], [t].[Id], [t].[OrderClientId], [t].[OrderId]
""");
    }

    public override async Task No_ignored_include_warning_when_implicit_load(bool async)
    {
        await base.No_ignored_include_warning_when_implicit_load(async);

        AssertSql(
"""
SELECT COUNT(*)
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
""");
    }

    public override async Task Query_for_branch_type_loads_all_owned_navs(bool async)
    {
        await base.Query_for_branch_type_loads_all_owned_navs(async);

        AssertSql(
"""
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [t].[ClientId], [t].[Id], [t].[OrderDate], [t].[PeriodEnd], [t].[PeriodStart], [t].[OrderClientId], [t].[OrderId], [t].[Id0], [t].[Detail], [t].[PeriodEnd0], [t].[PeriodStart0], [o].[PersonAddress_AddressLine], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [t] ON [o].[Id] = [t].[ClientId]
WHERE [o].[Discriminator] IN (N'Branch', N'LeafA')
ORDER BY [o].[Id], [t].[ClientId], [t].[Id], [t].[OrderClientId], [t].[OrderId]
""");
    }

    public override async Task Query_for_branch_type_loads_all_owned_navs_tracking(bool async)
    {
        await base.Query_for_branch_type_loads_all_owned_navs_tracking(async);

        AssertSql(
"""
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [t].[ClientId], [t].[Id], [t].[OrderDate], [t].[PeriodEnd], [t].[PeriodStart], [t].[OrderClientId], [t].[OrderId], [t].[Id0], [t].[Detail], [t].[PeriodEnd0], [t].[PeriodStart0], [o].[PersonAddress_AddressLine], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [t] ON [o].[Id] = [t].[ClientId]
WHERE [o].[Discriminator] IN (N'Branch', N'LeafA')
ORDER BY [o].[Id], [t].[ClientId], [t].[Id], [t].[OrderClientId], [t].[OrderId]
""");
    }

    public override async Task Query_for_leaf_type_loads_all_owned_navs(bool async)
    {
        await base.Query_for_leaf_type_loads_all_owned_navs(async);

        AssertSql(
"""
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [t].[ClientId], [t].[Id], [t].[OrderDate], [t].[PeriodEnd], [t].[PeriodStart], [t].[OrderClientId], [t].[OrderId], [t].[Id0], [t].[Detail], [t].[PeriodEnd0], [t].[PeriodStart0], [o].[PersonAddress_AddressLine], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [t] ON [o].[Id] = [t].[ClientId]
WHERE [o].[Discriminator] = N'LeafA'
ORDER BY [o].[Id], [t].[ClientId], [t].[Id], [t].[OrderClientId], [t].[OrderId]
""");
    }

    public override async Task Query_when_subquery(bool async)
    {
        await base.Query_when_subquery(async);

        AssertSql(
"""
@__p_0='5'

SELECT [t0].[Id], [t0].[Discriminator], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart], [t1].[ClientId], [t1].[Id], [t1].[OrderDate], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[OrderClientId], [t1].[OrderId], [t1].[Id0], [t1].[Detail], [t1].[PeriodEnd0], [t1].[PeriodStart0], [t0].[PersonAddress_AddressLine], [t0].[PeriodEnd0], [t0].[PeriodStart0], [t0].[PersonAddress_PlaceType], [t0].[PersonAddress_ZipCode], [t0].[PersonAddress_Country_Name], [t0].[PersonAddress_Country_PlanetId], [t0].[BranchAddress_BranchName], [t0].[BranchAddress_PlaceType], [t0].[BranchAddress_Country_Name], [t0].[BranchAddress_Country_PlanetId], [t0].[LeafBAddress_LeafBType], [t0].[LeafBAddress_PlaceType], [t0].[LeafBAddress_Country_Name], [t0].[LeafBAddress_Country_PlanetId], [t0].[LeafAAddress_LeafType], [t0].[LeafAAddress_PlaceType], [t0].[LeafAAddress_Country_Name], [t0].[LeafAAddress_Country_PlanetId]
FROM (
    SELECT TOP(@__p_0) [t].[Id], [t].[Discriminator], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t].[PersonAddress_AddressLine], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[PersonAddress_PlaceType], [t].[PersonAddress_ZipCode], [t].[PersonAddress_Country_Name], [t].[PersonAddress_Country_PlanetId], [t].[BranchAddress_BranchName], [t].[BranchAddress_PlaceType], [t].[BranchAddress_Country_Name], [t].[BranchAddress_Country_PlanetId], [t].[LeafBAddress_LeafBType], [t].[LeafBAddress_PlaceType], [t].[LeafBAddress_Country_Name], [t].[LeafBAddress_Country_PlanetId], [t].[LeafAAddress_LeafType], [t].[LeafAAddress_PlaceType], [t].[LeafAAddress_Country_Name], [t].[LeafAAddress_Country_PlanetId]
    FROM (
        SELECT DISTINCT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_AddressLine], [o].[PeriodEnd] AS [PeriodEnd0], [o].[PeriodStart] AS [PeriodStart0], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
        FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
    ) AS [t]
    ORDER BY [t].[Id]
) AS [t0]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [t1] ON [t0].[Id] = [t1].[ClientId]
ORDER BY [t0].[Id], [t1].[ClientId], [t1].[Id], [t1].[OrderClientId], [t1].[OrderId]
""");
    }

    public override async Task Navigation_rewrite_on_owned_reference_projecting_scalar(bool async)
    {
        await base.Navigation_rewrite_on_owned_reference_projecting_scalar(async);

        AssertSql(
"""
SELECT [o].[PersonAddress_Country_Name]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
WHERE [o].[PersonAddress_Country_Name] = N'USA'
""");
    }

    public override async Task Navigation_rewrite_on_owned_reference_projecting_entity(bool async)
    {
        await base.Navigation_rewrite_on_owned_reference_projecting_entity(async);

        AssertSql(
"""
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [t].[ClientId], [t].[Id], [t].[OrderDate], [t].[PeriodEnd], [t].[PeriodStart], [t].[OrderClientId], [t].[OrderId], [t].[Id0], [t].[Detail], [t].[PeriodEnd0], [t].[PeriodStart0], [o].[PersonAddress_AddressLine], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [t] ON [o].[Id] = [t].[ClientId]
WHERE [o].[PersonAddress_Country_Name] = N'USA'
ORDER BY [o].[Id], [t].[ClientId], [t].[Id], [t].[OrderClientId], [t].[OrderId]
""");
    }

    public override async Task Navigation_rewrite_on_owned_collection(bool async)
    {
        await base.Navigation_rewrite_on_owned_collection(async);

        AssertSql(
"""
SELECT [o].[Id], [t].[ClientId], [t].[Id], [t].[OrderDate], [t].[PeriodEnd], [t].[PeriodStart], [t].[OrderClientId], [t].[OrderId], [t].[Id0], [t].[Detail], [t].[PeriodEnd0], [t].[PeriodStart0]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN (
    SELECT [o1].[ClientId], [o1].[Id], [o1].[OrderDate], [o1].[PeriodEnd], [o1].[PeriodStart], [o2].[OrderClientId], [o2].[OrderId], [o2].[Id] AS [Id0], [o2].[Detail], [o2].[PeriodEnd] AS [PeriodEnd0], [o2].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o2] ON [o1].[ClientId] = [o2].[OrderClientId] AND [o1].[Id] = [o2].[OrderId]
) AS [t] ON [o].[Id] = [t].[ClientId]
WHERE (
    SELECT COUNT(*)
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    WHERE [o].[Id] = [o0].[ClientId]) > 0
ORDER BY [o].[Id], [t].[ClientId], [t].[Id], [t].[OrderClientId], [t].[OrderId]
""");
    }

    public override async Task Navigation_rewrite_on_owned_collection_with_composition(bool async)
    {
        await base.Navigation_rewrite_on_owned_collection_with_composition(async);

        AssertSql(
"""
SELECT COALESCE((
    SELECT TOP(1) CASE
        WHEN [o0].[Id] <> 42 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    WHERE [o].[Id] = [o0].[ClientId]
    ORDER BY [o0].[Id]), CAST(0 AS bit))
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
ORDER BY [o].[Id]
""");
    }

    public override async Task Navigation_rewrite_on_owned_collection_with_composition_complex(bool async)
    {
        await base.Navigation_rewrite_on_owned_collection_with_composition_complex(async);

        AssertSql(
"""
SELECT (
    SELECT TOP(1) [o1].[PersonAddress_Country_Name]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[Id]
    WHERE [o].[Id] = [o0].[ClientId]
    ORDER BY [o0].[Id])
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
""");
    }

    public override async Task SelectMany_on_owned_collection(bool async)
    {
        await base.SelectMany_on_owned_collection(async);

        AssertSql(
"""
SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o].[Id], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id], [o1].[Detail], [o1].[PeriodEnd], [o1].[PeriodStart]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
INNER JOIN [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0] ON [o].[Id] = [o0].[ClientId]
LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id], [o1].[OrderClientId], [o1].[OrderId]
""");
    }

    public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity(bool async)
    {
        await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Name], [p].[PeriodEnd], [p].[PeriodStart], [p].[StarId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
""");
    }

    public override async Task Filter_owned_entity_chained_with_regular_entity_followed_by_projecting_owned_collection(bool async)
    {
        await base.Filter_owned_entity_chained_with_regular_entity_followed_by_projecting_owned_collection(async);

        AssertSql(
"""
SELECT [o].[Id], [p].[Id], [t].[ClientId], [t].[Id], [t].[OrderDate], [t].[PeriodEnd], [t].[PeriodStart], [t].[OrderClientId], [t].[OrderId], [t].[Id0], [t].[Detail], [t].[PeriodEnd0], [t].[PeriodStart0]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [t] ON [o].[Id] = [t].[ClientId]
WHERE [p].[Id] <> 42 OR [p].[Id] IS NULL
ORDER BY [o].[Id], [p].[Id], [t].[ClientId], [t].[Id], [t].[OrderClientId], [t].[OrderId]
""");
    }

    public override async Task Project_multiple_owned_navigations(bool async)
    {
        await base.Project_multiple_owned_navigations(async);

        AssertSql(
"""
SELECT [o].[Id], [p].[Id], [t].[ClientId], [t].[Id], [t].[OrderDate], [t].[PeriodEnd], [t].[PeriodStart], [t].[OrderClientId], [t].[OrderId], [t].[Id0], [t].[Detail], [t].[PeriodEnd0], [t].[PeriodStart0], [o].[PersonAddress_AddressLine], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [p].[Name], [p].[PeriodEnd], [p].[PeriodStart], [p].[StarId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [t] ON [o].[Id] = [t].[ClientId]
ORDER BY [o].[Id], [p].[Id], [t].[ClientId], [t].[Id], [t].[OrderClientId], [t].[OrderId]
""");
    }

    public override async Task Project_multiple_owned_navigations_with_expansion_on_owned_collections(bool async)
    {
        await base.Project_multiple_owned_navigations_with_expansion_on_owned_collections(async);

        AssertSql(
"""
SELECT (
    SELECT COUNT(*)
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[Id]
    LEFT JOIN [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p0] ON [o1].[PersonAddress_Country_PlanetId] = [p0].[Id]
    LEFT JOIN [Star] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [p0].[StarId] = [s].[Id]
    WHERE [o].[Id] = [o0].[ClientId] AND ([s].[Id] <> 42 OR [s].[Id] IS NULL)) AS [Count], [p].[Id], [p].[Name], [p].[PeriodEnd], [p].[PeriodStart], [p].[StarId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
ORDER BY [o].[Id]
""");
    }

    public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_filter(bool async)
    {
        await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_filter(async);

        AssertSql(
"""
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [p].[Id], [t].[ClientId], [t].[Id], [t].[OrderDate], [t].[PeriodEnd], [t].[PeriodStart], [t].[OrderClientId], [t].[OrderId], [t].[Id0], [t].[Detail], [t].[PeriodEnd0], [t].[PeriodStart0], [o].[PersonAddress_AddressLine], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [t] ON [o].[Id] = [t].[ClientId]
WHERE [p].[Id] <> 7 OR [p].[Id] IS NULL
ORDER BY [o].[Id], [p].[Id], [t].[ClientId], [t].[Id], [t].[OrderClientId], [t].[OrderId]
""");
    }

    public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property(bool async)
    {
        await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property(async);

        AssertSql(
"""
SELECT [p].[Id]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
""");
    }

    public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection(bool async)
    {
        await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection(async);

        AssertSql(
"""
SELECT [o].[Id], [p].[Id], [m].[Id], [m].[Diameter], [m].[PeriodEnd], [m].[PeriodStart], [m].[PlanetId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Moon] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m] ON [p].[Id] = [m].[PlanetId]
ORDER BY [o].[Id], [p].[Id]
""");
    }

    public override async Task SelectMany_on_owned_reference_followed_by_regular_entity_and_collection(bool async)
    {
        await base.SelectMany_on_owned_reference_followed_by_regular_entity_and_collection(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[Diameter], [m].[PeriodEnd], [m].[PeriodStart], [m].[PlanetId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
INNER JOIN [Moon] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m] ON [p].[Id] = [m].[PlanetId]
""");
    }

    public override async Task SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection(bool async)
    {
        await base.SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection(async);

        AssertSql(
"""
SELECT [e].[Id], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[StarId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Star] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [p].[StarId] = [s].[Id]
INNER JOIN [Element] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e] ON [s].[Id] = [e].[StarId]
""");
    }

    public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference(bool async)
    {
        await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference(async);

        AssertSql(
"""
SELECT [s].[Id], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [o].[Id], [p].[Id], [e].[Id], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[StarId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Star] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [p].[StarId] = [s].[Id]
LEFT JOIN [Element] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e] ON [s].[Id] = [e].[StarId]
ORDER BY [o].[Id], [p].[Id], [s].[Id]
""");
    }

    public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar(
        bool async)
    {
        await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar(async);

        AssertSql(
"""
SELECT [s].[Name]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Star] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [p].[StarId] = [s].[Id]
""");
    }

    public override async Task
        Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection(bool async)
    {
        await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection(
            async);

        AssertSql(
"""
SELECT [s].[Id], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [o].[Id], [p].[Id], [e].[Id], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[StarId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Star] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [p].[StarId] = [s].[Id]
LEFT JOIN [Element] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e] ON [s].[Id] = [e].[StarId]
WHERE [s].[Name] = N'Sol'
ORDER BY [o].[Id], [p].[Id], [s].[Id]
""");
    }

    public override async Task Query_with_OfType_eagerly_loads_correct_owned_navigations(bool async)
    {
        await base.Query_with_OfType_eagerly_loads_correct_owned_navigations(async);

        AssertSql(
"""
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [t].[ClientId], [t].[Id], [t].[OrderDate], [t].[PeriodEnd], [t].[PeriodStart], [t].[OrderClientId], [t].[OrderId], [t].[Id0], [t].[Detail], [t].[PeriodEnd0], [t].[PeriodStart0], [o].[PersonAddress_AddressLine], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [t] ON [o].[Id] = [t].[ClientId]
WHERE [o].[Discriminator] = N'LeafA'
ORDER BY [o].[Id], [t].[ClientId], [t].[Id], [t].[OrderClientId], [t].[OrderId]
""");
    }

    public override async Task Unmapped_property_projection_loads_owned_navigations(bool async)
    {
        await base.Unmapped_property_projection_loads_owned_navigations(async);

        AssertSql(
"""
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [t].[ClientId], [t].[Id], [t].[OrderDate], [t].[PeriodEnd], [t].[PeriodStart], [t].[OrderClientId], [t].[OrderId], [t].[Id0], [t].[Detail], [t].[PeriodEnd0], [t].[PeriodStart0], [o].[PersonAddress_AddressLine], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [t] ON [o].[Id] = [t].[ClientId]
WHERE [o].[Id] = 1
ORDER BY [o].[Id], [t].[ClientId], [t].[Id], [t].[OrderClientId], [t].[OrderId]
""");
    }

    public override async Task Client_method_skip_loads_owned_navigations(bool async)
    {
        await base.Client_method_skip_loads_owned_navigations(async);

        AssertSql(
"""
@__p_0='1'

SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t0].[ClientId], [t0].[Id], [t0].[OrderDate], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[OrderClientId], [t0].[OrderId], [t0].[Id0], [t0].[Detail], [t0].[PeriodEnd0], [t0].[PeriodStart0], [t].[PersonAddress_AddressLine], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[PersonAddress_PlaceType], [t].[PersonAddress_ZipCode], [t].[PersonAddress_Country_Name], [t].[PersonAddress_Country_PlanetId], [t].[BranchAddress_BranchName], [t].[BranchAddress_PlaceType], [t].[BranchAddress_Country_Name], [t].[BranchAddress_Country_PlanetId], [t].[LeafBAddress_LeafBType], [t].[LeafBAddress_PlaceType], [t].[LeafBAddress_Country_Name], [t].[LeafBAddress_Country_PlanetId], [t].[LeafAAddress_LeafType], [t].[LeafAAddress_PlaceType], [t].[LeafAAddress_Country_Name], [t].[LeafAAddress_Country_PlanetId]
FROM (
    SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_AddressLine], [o].[PeriodEnd] AS [PeriodEnd0], [o].[PeriodStart] AS [PeriodStart0], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
    ORDER BY [o].[Id]
    OFFSET @__p_0 ROWS
) AS [t]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [t0] ON [t].[Id] = [t0].[ClientId]
ORDER BY [t].[Id], [t0].[ClientId], [t0].[Id], [t0].[OrderClientId], [t0].[OrderId]
""");
    }

    public override async Task Client_method_take_loads_owned_navigations(bool async)
    {
        await base.Client_method_take_loads_owned_navigations(async);

        AssertSql(
"""
@__p_0='2'

SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t0].[ClientId], [t0].[Id], [t0].[OrderDate], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[OrderClientId], [t0].[OrderId], [t0].[Id0], [t0].[Detail], [t0].[PeriodEnd0], [t0].[PeriodStart0], [t].[PersonAddress_AddressLine], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[PersonAddress_PlaceType], [t].[PersonAddress_ZipCode], [t].[PersonAddress_Country_Name], [t].[PersonAddress_Country_PlanetId], [t].[BranchAddress_BranchName], [t].[BranchAddress_PlaceType], [t].[BranchAddress_Country_Name], [t].[BranchAddress_Country_PlanetId], [t].[LeafBAddress_LeafBType], [t].[LeafBAddress_PlaceType], [t].[LeafBAddress_Country_Name], [t].[LeafBAddress_Country_PlanetId], [t].[LeafAAddress_LeafType], [t].[LeafAAddress_PlaceType], [t].[LeafAAddress_Country_Name], [t].[LeafAAddress_Country_PlanetId]
FROM (
    SELECT TOP(@__p_0) [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_AddressLine], [o].[PeriodEnd] AS [PeriodEnd0], [o].[PeriodStart] AS [PeriodStart0], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
    ORDER BY [o].[Id]
) AS [t]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [t0] ON [t].[Id] = [t0].[ClientId]
ORDER BY [t].[Id], [t0].[ClientId], [t0].[Id], [t0].[OrderClientId], [t0].[OrderId]
""");
    }

    public override async Task Client_method_skip_take_loads_owned_navigations(bool async)
    {
        await base.Client_method_skip_take_loads_owned_navigations(async);

        AssertSql(
"""
@__p_0='1'
@__p_1='2'

SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t0].[ClientId], [t0].[Id], [t0].[OrderDate], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[OrderClientId], [t0].[OrderId], [t0].[Id0], [t0].[Detail], [t0].[PeriodEnd0], [t0].[PeriodStart0], [t].[PersonAddress_AddressLine], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[PersonAddress_PlaceType], [t].[PersonAddress_ZipCode], [t].[PersonAddress_Country_Name], [t].[PersonAddress_Country_PlanetId], [t].[BranchAddress_BranchName], [t].[BranchAddress_PlaceType], [t].[BranchAddress_Country_Name], [t].[BranchAddress_Country_PlanetId], [t].[LeafBAddress_LeafBType], [t].[LeafBAddress_PlaceType], [t].[LeafBAddress_Country_Name], [t].[LeafBAddress_Country_PlanetId], [t].[LeafAAddress_LeafType], [t].[LeafAAddress_PlaceType], [t].[LeafAAddress_Country_Name], [t].[LeafAAddress_Country_PlanetId]
FROM (
    SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_AddressLine], [o].[PeriodEnd] AS [PeriodEnd0], [o].[PeriodStart] AS [PeriodStart0], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
    ORDER BY [o].[Id]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [t0] ON [t].[Id] = [t0].[ClientId]
ORDER BY [t].[Id], [t0].[ClientId], [t0].[Id], [t0].[OrderClientId], [t0].[OrderId]
""");
    }

    public override async Task Client_method_skip_loads_owned_navigations_variation_2(bool async)
    {
        await base.Client_method_skip_loads_owned_navigations_variation_2(async);

        AssertSql(
"""
@__p_0='1'

SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t0].[ClientId], [t0].[Id], [t0].[OrderDate], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[OrderClientId], [t0].[OrderId], [t0].[Id0], [t0].[Detail], [t0].[PeriodEnd0], [t0].[PeriodStart0], [t].[PersonAddress_AddressLine], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[PersonAddress_PlaceType], [t].[PersonAddress_ZipCode], [t].[PersonAddress_Country_Name], [t].[PersonAddress_Country_PlanetId], [t].[BranchAddress_BranchName], [t].[BranchAddress_PlaceType], [t].[BranchAddress_Country_Name], [t].[BranchAddress_Country_PlanetId], [t].[LeafBAddress_LeafBType], [t].[LeafBAddress_PlaceType], [t].[LeafBAddress_Country_Name], [t].[LeafBAddress_Country_PlanetId], [t].[LeafAAddress_LeafType], [t].[LeafAAddress_PlaceType], [t].[LeafAAddress_Country_Name], [t].[LeafAAddress_Country_PlanetId]
FROM (
    SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_AddressLine], [o].[PeriodEnd] AS [PeriodEnd0], [o].[PeriodStart] AS [PeriodStart0], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
    ORDER BY [o].[Id]
    OFFSET @__p_0 ROWS
) AS [t]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [t0] ON [t].[Id] = [t0].[ClientId]
ORDER BY [t].[Id], [t0].[ClientId], [t0].[Id], [t0].[OrderClientId], [t0].[OrderId]
""");
    }

    public override async Task Client_method_take_loads_owned_navigations_variation_2(bool async)
    {
        await base.Client_method_take_loads_owned_navigations_variation_2(async);

        AssertSql(
"""
@__p_0='2'

SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t0].[ClientId], [t0].[Id], [t0].[OrderDate], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[OrderClientId], [t0].[OrderId], [t0].[Id0], [t0].[Detail], [t0].[PeriodEnd0], [t0].[PeriodStart0], [t].[PersonAddress_AddressLine], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[PersonAddress_PlaceType], [t].[PersonAddress_ZipCode], [t].[PersonAddress_Country_Name], [t].[PersonAddress_Country_PlanetId], [t].[BranchAddress_BranchName], [t].[BranchAddress_PlaceType], [t].[BranchAddress_Country_Name], [t].[BranchAddress_Country_PlanetId], [t].[LeafBAddress_LeafBType], [t].[LeafBAddress_PlaceType], [t].[LeafBAddress_Country_Name], [t].[LeafBAddress_Country_PlanetId], [t].[LeafAAddress_LeafType], [t].[LeafAAddress_PlaceType], [t].[LeafAAddress_Country_Name], [t].[LeafAAddress_Country_PlanetId]
FROM (
    SELECT TOP(@__p_0) [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_AddressLine], [o].[PeriodEnd] AS [PeriodEnd0], [o].[PeriodStart] AS [PeriodStart0], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
    ORDER BY [o].[Id]
) AS [t]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [t0] ON [t].[Id] = [t0].[ClientId]
ORDER BY [t].[Id], [t0].[ClientId], [t0].[Id], [t0].[OrderClientId], [t0].[OrderId]
""");
    }

    public override async Task Client_method_skip_take_loads_owned_navigations_variation_2(bool async)
    {
        await base.Client_method_skip_take_loads_owned_navigations_variation_2(async);

        AssertSql(
"""
@__p_0='1'
@__p_1='2'

SELECT [t].[Id], [t].[Discriminator], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t0].[ClientId], [t0].[Id], [t0].[OrderDate], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[OrderClientId], [t0].[OrderId], [t0].[Id0], [t0].[Detail], [t0].[PeriodEnd0], [t0].[PeriodStart0], [t].[PersonAddress_AddressLine], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[PersonAddress_PlaceType], [t].[PersonAddress_ZipCode], [t].[PersonAddress_Country_Name], [t].[PersonAddress_Country_PlanetId], [t].[BranchAddress_BranchName], [t].[BranchAddress_PlaceType], [t].[BranchAddress_Country_Name], [t].[BranchAddress_Country_PlanetId], [t].[LeafBAddress_LeafBType], [t].[LeafBAddress_PlaceType], [t].[LeafBAddress_Country_Name], [t].[LeafBAddress_Country_PlanetId], [t].[LeafAAddress_LeafType], [t].[LeafAAddress_PlaceType], [t].[LeafAAddress_Country_Name], [t].[LeafAAddress_Country_PlanetId]
FROM (
    SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_AddressLine], [o].[PeriodEnd] AS [PeriodEnd0], [o].[PeriodStart] AS [PeriodStart0], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
    ORDER BY [o].[Id]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [t0] ON [t].[Id] = [t0].[ClientId]
ORDER BY [t].[Id], [t0].[ClientId], [t0].[Id], [t0].[OrderClientId], [t0].[OrderId]
""");
    }

    public override async Task Where_owned_collection_navigation_ToList_Count(bool async)
    {
        await base.Where_owned_collection_navigation_ToList_Count(async);

        AssertSql(
"""
SELECT [o].[Id], [o0].[ClientId], [o0].[Id], [o2].[OrderClientId], [o2].[OrderId], [o2].[Id], [o2].[Detail], [o2].[PeriodEnd], [o2].[PeriodStart]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
INNER JOIN [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0] ON [o].[Id] = [o0].[ClientId]
LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o2] ON [o0].[ClientId] = [o2].[OrderClientId] AND [o0].[Id] = [o2].[OrderId]
WHERE (
    SELECT COUNT(*)
    FROM [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1]
    WHERE [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]) = 0
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id], [o2].[OrderClientId], [o2].[OrderId]
""");
    }

    public override async Task Where_collection_navigation_ToArray_Count(bool async)
    {
        await base.Where_collection_navigation_ToArray_Count(async);

        AssertSql(
"""
SELECT [o].[Id], [o0].[ClientId], [o0].[Id], [o2].[OrderClientId], [o2].[OrderId], [o2].[Id], [o2].[Detail], [o2].[PeriodEnd], [o2].[PeriodStart]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
INNER JOIN [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0] ON [o].[Id] = [o0].[ClientId]
LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o2] ON [o0].[ClientId] = [o2].[OrderClientId] AND [o0].[Id] = [o2].[OrderId]
WHERE (
    SELECT COUNT(*)
    FROM [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1]
    WHERE [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]) = 0
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id], [o2].[OrderClientId], [o2].[OrderId]
""");
    }

    public override async Task Where_collection_navigation_AsEnumerable_Count(bool async)
    {
        await base.Where_collection_navigation_AsEnumerable_Count(async);

        AssertSql(
"""
SELECT [o].[Id], [o0].[ClientId], [o0].[Id], [o2].[OrderClientId], [o2].[OrderId], [o2].[Id], [o2].[Detail], [o2].[PeriodEnd], [o2].[PeriodStart]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
INNER JOIN [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0] ON [o].[Id] = [o0].[ClientId]
LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o2] ON [o0].[ClientId] = [o2].[OrderClientId] AND [o0].[Id] = [o2].[OrderId]
WHERE (
    SELECT COUNT(*)
    FROM [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1]
    WHERE [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]) = 0
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id], [o2].[OrderClientId], [o2].[OrderId]
""");
    }

    public override async Task Where_collection_navigation_ToList_Count_member(bool async)
    {
        await base.Where_collection_navigation_ToList_Count_member(async);

        AssertSql(
"""
SELECT [o].[Id], [o0].[ClientId], [o0].[Id], [o2].[OrderClientId], [o2].[OrderId], [o2].[Id], [o2].[Detail], [o2].[PeriodEnd], [o2].[PeriodStart]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
INNER JOIN [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0] ON [o].[Id] = [o0].[ClientId]
LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o2] ON [o0].[ClientId] = [o2].[OrderClientId] AND [o0].[Id] = [o2].[OrderId]
WHERE (
    SELECT COUNT(*)
    FROM [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1]
    WHERE [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]) = 0
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id], [o2].[OrderClientId], [o2].[OrderId]
""");
    }

    public override async Task Where_collection_navigation_ToArray_Length_member(bool async)
    {
        await base.Where_collection_navigation_ToArray_Length_member(async);

        AssertSql(
"""
SELECT [o].[Id], [o0].[ClientId], [o0].[Id], [o2].[OrderClientId], [o2].[OrderId], [o2].[Id], [o2].[Detail], [o2].[PeriodEnd], [o2].[PeriodStart]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
INNER JOIN [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0] ON [o].[Id] = [o0].[ClientId]
LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o2] ON [o0].[ClientId] = [o2].[OrderClientId] AND [o0].[Id] = [o2].[OrderId]
WHERE (
    SELECT COUNT(*)
    FROM [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1]
    WHERE [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]) = 0
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id], [o2].[OrderClientId], [o2].[OrderId]
""");
    }

    public override async Task Can_query_on_indexer_properties(bool async)
    {
        await base.Can_query_on_indexer_properties(async);

        AssertSql(
"""
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [t].[ClientId], [t].[Id], [t].[OrderDate], [t].[PeriodEnd], [t].[PeriodStart], [t].[OrderClientId], [t].[OrderId], [t].[Id0], [t].[Detail], [t].[PeriodEnd0], [t].[PeriodStart0], [o].[PersonAddress_AddressLine], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [t] ON [o].[Id] = [t].[ClientId]
WHERE [o].[Name] = N'Mona Cy'
ORDER BY [o].[Id], [t].[ClientId], [t].[Id], [t].[OrderClientId], [t].[OrderId]
""");
    }

    public override async Task Can_query_on_owned_indexer_properties(bool async)
    {
        await base.Can_query_on_owned_indexer_properties(async);

        AssertSql(
"""
SELECT [o].[Name]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
WHERE [o].[PersonAddress_ZipCode] = 38654
""");
    }

    public override async Task Can_query_on_indexer_property_when_property_name_from_closure(bool async)
    {
        await base.Can_query_on_indexer_property_when_property_name_from_closure(async);

        AssertSql(
"""
SELECT [o].[Name]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
WHERE [o].[Name] = N'Mona Cy'
""");
    }

    public override async Task Can_project_indexer_properties(bool async)
    {
        await base.Can_project_indexer_properties(async);

        AssertSql(
"""
SELECT [o].[Name]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
""");
    }

    public override async Task Can_project_owned_indexer_properties(bool async)
    {
        await base.Can_project_owned_indexer_properties(async);

        AssertSql(
"""
SELECT [o].[PersonAddress_AddressLine]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
""");
    }

    public override async Task Can_project_indexer_properties_converted(bool async)
    {
        await base.Can_project_indexer_properties_converted(async);

        AssertSql(
"""
SELECT [o].[Name]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
""");
    }

    public override async Task Can_project_owned_indexer_properties_converted(bool async)
    {
        await base.Can_project_owned_indexer_properties_converted(async);

        AssertSql(
"""
SELECT [o].[PersonAddress_AddressLine]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
""");
    }

    public override async Task Can_OrderBy_indexer_properties(bool async)
    {
        await base.Can_OrderBy_indexer_properties(async);

        AssertSql(
"""
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [t].[ClientId], [t].[Id], [t].[OrderDate], [t].[PeriodEnd], [t].[PeriodStart], [t].[OrderClientId], [t].[OrderId], [t].[Id0], [t].[Detail], [t].[PeriodEnd0], [t].[PeriodStart0], [o].[PersonAddress_AddressLine], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [t] ON [o].[Id] = [t].[ClientId]
ORDER BY [o].[Name], [o].[Id], [t].[ClientId], [t].[Id], [t].[OrderClientId], [t].[OrderId]
""");
    }

    public override async Task Can_OrderBy_indexer_properties_converted(bool async)
    {
        await base.Can_OrderBy_indexer_properties_converted(async);

        AssertSql(
"""
SELECT [o].[Name]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
ORDER BY [o].[Name], [o].[Id]
""");
    }

    public override async Task Can_OrderBy_owned_indexer_properties(bool async)
    {
        await base.Can_OrderBy_owned_indexer_properties(async);

        AssertSql(
"""
SELECT [o].[Name]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
ORDER BY [o].[PersonAddress_ZipCode], [o].[Id]
""");
    }

    public override async Task Can_OrderBy_owened_indexer_properties_converted(bool async)
    {
        await base.Can_OrderBy_owened_indexer_properties_converted(async);

        AssertSql(
"""
SELECT [o].[Name]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
ORDER BY [o].[PersonAddress_ZipCode], [o].[Id]
""");
    }

    public override async Task Can_group_by_indexer_property(bool isAsync)
    {
        await base.Can_group_by_indexer_property(isAsync);

        AssertSql(
"""
SELECT COUNT(*)
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
GROUP BY [o].[Name]
""");
    }

    public override async Task Can_group_by_converted_indexer_property(bool isAsync)
    {
        await base.Can_group_by_converted_indexer_property(isAsync);

        AssertSql(
"""
SELECT COUNT(*)
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
GROUP BY [o].[Name]
""");
    }

    public override async Task Can_group_by_owned_indexer_property(bool isAsync)
    {
        await base.Can_group_by_owned_indexer_property(isAsync);

        AssertSql(
"""
SELECT COUNT(*)
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
GROUP BY [o].[PersonAddress_ZipCode]
""");
    }

    public override async Task Can_group_by_converted_owned_indexer_property(bool isAsync)
    {
        await base.Can_group_by_converted_owned_indexer_property(isAsync);

        AssertSql(
"""
SELECT COUNT(*)
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
GROUP BY [o].[PersonAddress_ZipCode]
""");
    }

    public override async Task Can_join_on_indexer_property_on_query(bool isAsync)
    {
        await base.Can_join_on_indexer_property_on_query(isAsync);

        AssertSql(
"""
SELECT [o].[Id], [o0].[PersonAddress_Country_Name] AS [Name]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
INNER JOIN [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0] ON [o].[PersonAddress_ZipCode] = [o0].[PersonAddress_ZipCode]
""");
    }

    public override async Task Projecting_indexer_property_ignores_include(bool isAsync)
    {
        await base.Projecting_indexer_property_ignores_include(isAsync);

        AssertSql(
"""
SELECT [o].[PersonAddress_ZipCode] AS [Nation]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
""");
    }

    public override async Task Projecting_indexer_property_ignores_include_converted(bool isAsync)
    {
        await base.Projecting_indexer_property_ignores_include_converted(isAsync);

        AssertSql(
"""
SELECT [o].[PersonAddress_ZipCode] AS [Nation]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
""");
    }

    public override async Task Indexer_property_is_pushdown_into_subquery(bool isAsync)
    {
        await base.Indexer_property_is_pushdown_into_subquery(isAsync);

        AssertSql(
"""
SELECT [o].[Name]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
WHERE (
    SELECT TOP(1) [o0].[Name]
    FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    WHERE [o0].[Id] = [o].[Id]) = N'Mona Cy'
""");
    }

    public override async Task Can_query_indexer_property_on_owned_collection(bool isAsync)
    {
        await base.Can_query_indexer_property_on_owned_collection(isAsync);

        AssertSql(
"""
SELECT [o].[Name]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
WHERE (
    SELECT COUNT(*)
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    WHERE [o].[Id] = [o0].[ClientId] AND DATEPART(year, [o0].[OrderDate]) = 2018) = 1
""");
    }

    public override async Task Query_for_base_type_loads_all_owned_navs_split(bool async)
    {
        await base.Query_for_base_type_loads_all_owned_navs_split(async);

        AssertSql(
"""
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_AddressLine], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
ORDER BY [o].[Id]
""",
            //
"""
SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o].[Id]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
INNER JOIN [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0] ON [o].[Id] = [o0].[ClientId]
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]
""",
            //
"""
SELECT [o1].[OrderClientId], [o1].[OrderId], [o1].[Id], [o1].[Detail], [o1].[PeriodEnd], [o1].[PeriodStart], [o].[Id], [o0].[ClientId], [o0].[Id]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
INNER JOIN [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0] ON [o].[Id] = [o0].[ClientId]
INNER JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]
""");
    }

    public override async Task Query_for_branch_type_loads_all_owned_navs_split(bool async)
    {
        await base.Query_for_branch_type_loads_all_owned_navs_split(async);

        AssertSql(
"""
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_AddressLine], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
WHERE [o].[Discriminator] IN (N'Branch', N'LeafA')
ORDER BY [o].[Id]
""",
            //
"""
SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o].[Id]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
INNER JOIN [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE [o].[Discriminator] IN (N'Branch', N'LeafA')
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]
""",
            //
"""
SELECT [o1].[OrderClientId], [o1].[OrderId], [o1].[Id], [o1].[Detail], [o1].[PeriodEnd], [o1].[PeriodStart], [o].[Id], [o0].[ClientId], [o0].[Id]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
INNER JOIN [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0] ON [o].[Id] = [o0].[ClientId]
INNER JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
WHERE [o].[Discriminator] IN (N'Branch', N'LeafA')
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]
""");
    }

    public override async Task Query_when_subquery_split(bool async)
    {
        await base.Query_when_subquery_split(async);

        AssertSql(
"""
@__p_0='5'

SELECT TOP(@__p_0) [t].[Id], [t].[Discriminator], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t].[PersonAddress_AddressLine], [t].[PeriodEnd0], [t].[PeriodStart0], [t].[PersonAddress_PlaceType], [t].[PersonAddress_ZipCode], [t].[PersonAddress_Country_Name], [t].[PersonAddress_Country_PlanetId], [t].[BranchAddress_BranchName], [t].[BranchAddress_PlaceType], [t].[BranchAddress_Country_Name], [t].[BranchAddress_Country_PlanetId], [t].[LeafBAddress_LeafBType], [t].[LeafBAddress_PlaceType], [t].[LeafBAddress_Country_Name], [t].[LeafBAddress_Country_PlanetId], [t].[LeafAAddress_LeafType], [t].[LeafAAddress_PlaceType], [t].[LeafAAddress_Country_Name], [t].[LeafAAddress_Country_PlanetId]
FROM (
    SELECT DISTINCT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_AddressLine], [o].[PeriodEnd] AS [PeriodEnd0], [o].[PeriodStart] AS [PeriodStart0], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
) AS [t]
ORDER BY [t].[Id]
""",
            //
"""
@__p_0='5'

SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [t0].[Id]
FROM (
    SELECT TOP(@__p_0) [t].[Id]
    FROM (
        SELECT DISTINCT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_AddressLine], [o].[PeriodEnd] AS [PeriodEnd0], [o].[PeriodStart] AS [PeriodStart0], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
        FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
    ) AS [t]
    ORDER BY [t].[Id]
) AS [t0]
INNER JOIN [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0] ON [t0].[Id] = [o0].[ClientId]
ORDER BY [t0].[Id], [o0].[ClientId], [o0].[Id]
""",
            //
"""
@__p_0='5'

SELECT [o1].[OrderClientId], [o1].[OrderId], [o1].[Id], [o1].[Detail], [o1].[PeriodEnd], [o1].[PeriodStart], [t0].[Id], [o0].[ClientId], [o0].[Id]
FROM (
    SELECT TOP(@__p_0) [t].[Id]
    FROM (
        SELECT DISTINCT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_AddressLine], [o].[PeriodEnd] AS [PeriodEnd0], [o].[PeriodStart] AS [PeriodStart0], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
        FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
    ) AS [t]
    ORDER BY [t].[Id]
) AS [t0]
INNER JOIN [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0] ON [t0].[Id] = [o0].[ClientId]
INNER JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
ORDER BY [t0].[Id], [o0].[ClientId], [o0].[Id]
""");
    }

    public override async Task Project_multiple_owned_navigations_split(bool async)
    {
        await base.Project_multiple_owned_navigations_split(async);

        AssertSql(
"""
SELECT [o].[Id], [p].[Id], [o].[PersonAddress_AddressLine], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [p].[Name], [p].[PeriodEnd], [p].[PeriodStart], [p].[StarId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
ORDER BY [o].[Id], [p].[Id]
""",
            //
"""
SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o].[Id], [p].[Id]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
INNER JOIN [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0] ON [o].[Id] = [o0].[ClientId]
ORDER BY [o].[Id], [p].[Id], [o0].[ClientId], [o0].[Id]
""",
            //
"""
SELECT [o1].[OrderClientId], [o1].[OrderId], [o1].[Id], [o1].[Detail], [o1].[PeriodEnd], [o1].[PeriodStart], [o].[Id], [p].[Id], [o0].[ClientId], [o0].[Id]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
INNER JOIN [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0] ON [o].[Id] = [o0].[ClientId]
INNER JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
ORDER BY [o].[Id], [p].[Id], [o0].[ClientId], [o0].[Id]
""");
    }

    public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_split(bool async)
    {
        await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_split(async);

        AssertSql(
"""
SELECT [o].[Id], [p].[Id]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
ORDER BY [o].[Id], [p].[Id]
""",
            //
"""
SELECT [m].[Id], [m].[Diameter], [m].[PeriodEnd], [m].[PeriodStart], [m].[PlanetId], [o].[Id], [p].[Id]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
INNER JOIN [Moon] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [m] ON [p].[Id] = [m].[PlanetId]
ORDER BY [o].[Id], [p].[Id]
""");
    }

    public override async Task Query_with_OfType_eagerly_loads_correct_owned_navigations_split(bool async)
    {
        await base.Query_with_OfType_eagerly_loads_correct_owned_navigations_split(async);

        AssertSql(
"""
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_AddressLine], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
WHERE [o].[Discriminator] = N'LeafA'
ORDER BY [o].[Id]
""",
            //
"""
SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o].[Id]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
INNER JOIN [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE [o].[Discriminator] = N'LeafA'
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]
""",
            //
"""
SELECT [o1].[OrderClientId], [o1].[OrderId], [o1].[Id], [o1].[Detail], [o1].[PeriodEnd], [o1].[PeriodStart], [o].[Id], [o0].[ClientId], [o0].[Id]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
INNER JOIN [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0] ON [o].[Id] = [o0].[ClientId]
INNER JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
WHERE [o].[Discriminator] = N'LeafA'
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]
""");
    }

    public override async Task Unmapped_property_projection_loads_owned_navigations_split(bool async)
    {
        await base.Unmapped_property_projection_loads_owned_navigations_split(async);

        AssertSql(
"""
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_AddressLine], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
WHERE [o].[Id] = 1
ORDER BY [o].[Id]
""",
            //
"""
SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o].[Id]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
INNER JOIN [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE [o].[Id] = 1
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]
""",
            //
"""
SELECT [o1].[OrderClientId], [o1].[OrderId], [o1].[Id], [o1].[Detail], [o1].[PeriodEnd], [o1].[PeriodStart], [o].[Id], [o0].[ClientId], [o0].[Id]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
INNER JOIN [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0] ON [o].[Id] = [o0].[ClientId]
INNER JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
WHERE [o].[Id] = 1
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]
""");
    }

    public override async Task Can_query_on_indexer_properties_split(bool async)
    {
        await base.Can_query_on_indexer_properties_split(async);

        AssertSql(
"""
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_AddressLine], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
WHERE [o].[Name] = N'Mona Cy'
ORDER BY [o].[Id]
""",
            //
"""
SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o].[Id]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
INNER JOIN [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0] ON [o].[Id] = [o0].[ClientId]
WHERE [o].[Name] = N'Mona Cy'
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]
""",
            //
"""
SELECT [o1].[OrderClientId], [o1].[OrderId], [o1].[Id], [o1].[Detail], [o1].[PeriodEnd], [o1].[PeriodStart], [o].[Id], [o0].[ClientId], [o0].[Id]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
INNER JOIN [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0] ON [o].[Id] = [o0].[ClientId]
INNER JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
WHERE [o].[Name] = N'Mona Cy'
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id]
""");
    }

    public override async Task GroupBy_with_multiple_aggregates_on_owned_navigation_properties(bool async)
    {
        await base.GroupBy_with_multiple_aggregates_on_owned_navigation_properties(async);

        AssertSql(
"""
SELECT (
    SELECT AVG(CAST([s].[Id] AS float))
    FROM (
        SELECT [o0].[Id], [o0].[Discriminator], [o0].[Name], [o0].[PeriodEnd], [o0].[PeriodStart], 1 AS [Key], [o0].[PersonAddress_AddressLine], [o0].[PeriodEnd] AS [PeriodEnd0], [o0].[PeriodStart] AS [PeriodStart0], [o0].[PersonAddress_PlaceType], [o0].[PersonAddress_ZipCode], [o0].[PersonAddress_Country_Name], [o0].[PersonAddress_Country_PlanetId]
        FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    ) AS [t0]
    LEFT JOIN [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p] ON [t0].[PersonAddress_Country_PlanetId] = [p].[Id]
    LEFT JOIN [Star] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s] ON [p].[StarId] = [s].[Id]
    WHERE [t].[Key] = [t0].[Key]) AS [p1], (
    SELECT COALESCE(SUM([s0].[Id]), 0)
    FROM (
        SELECT [o1].[Id], [o1].[Discriminator], [o1].[Name], [o1].[PeriodEnd], [o1].[PeriodStart], 1 AS [Key], [o1].[PersonAddress_AddressLine], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0], [o1].[PersonAddress_PlaceType], [o1].[PersonAddress_ZipCode], [o1].[PersonAddress_Country_Name], [o1].[PersonAddress_Country_PlanetId]
        FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1]
    ) AS [t1]
    LEFT JOIN [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p0] ON [t1].[PersonAddress_Country_PlanetId] = [p0].[Id]
    LEFT JOIN [Star] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s0] ON [p0].[StarId] = [s0].[Id]
    WHERE [t].[Key] = [t1].[Key]) AS [p2], (
    SELECT MAX(CAST(LEN([s1].[Name]) AS int))
    FROM (
        SELECT [o2].[Id], [o2].[Discriminator], [o2].[Name], [o2].[PeriodEnd], [o2].[PeriodStart], 1 AS [Key], [o2].[PersonAddress_AddressLine], [o2].[PeriodEnd] AS [PeriodEnd0], [o2].[PeriodStart] AS [PeriodStart0], [o2].[PersonAddress_PlaceType], [o2].[PersonAddress_ZipCode], [o2].[PersonAddress_Country_Name], [o2].[PersonAddress_Country_PlanetId]
        FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o2]
    ) AS [t2]
    LEFT JOIN [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p1] ON [t2].[PersonAddress_Country_PlanetId] = [p1].[Id]
    LEFT JOIN [Star] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s1] ON [p1].[StarId] = [s1].[Id]
    WHERE [t].[Key] = [t2].[Key]) AS [p3]
FROM (
    SELECT 1 AS [Key]
    FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
) AS [t]
GROUP BY [t].[Key]
""");
    }

    public override async Task Ordering_by_identifying_projection(bool async)
    {
        await base.Ordering_by_identifying_projection(async);

        AssertSql(
"""
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [t].[ClientId], [t].[Id], [t].[OrderDate], [t].[PeriodEnd], [t].[PeriodStart], [t].[OrderClientId], [t].[OrderId], [t].[Id0], [t].[Detail], [t].[PeriodEnd0], [t].[PeriodStart0], [o].[PersonAddress_AddressLine], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [t] ON [o].[Id] = [t].[ClientId]
ORDER BY [o].[PersonAddress_PlaceType], [o].[Id], [t].[ClientId], [t].[Id], [t].[OrderClientId], [t].[OrderId]
""");
    }

    public override async Task Using_from_sql_on_owner_generates_join_with_table_for_owned_shared_dependents(bool async)
    {
        await base.Using_from_sql_on_owner_generates_join_with_table_for_owned_shared_dependents(async);

        AssertSql(
"""
SELECT [m].[Id], [m].[Discriminator], [m].[Name], [m].[PeriodEnd], [m].[PeriodStart], [o].[Id], [o0].[Id], [o1].[Id], [o2].[Id], [t].[ClientId], [t].[Id], [t].[OrderDate], [t].[PeriodEnd], [t].[PeriodStart], [t].[OrderClientId], [t].[OrderId], [t].[Id0], [t].[Detail], [t].[PeriodEnd0], [t].[PeriodStart0], [o].[PersonAddress_AddressLine], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o0].[BranchAddress_BranchName], [o0].[PeriodEnd], [o0].[PeriodStart], [o0].[BranchAddress_PlaceType], [o0].[BranchAddress_Country_Name], [o0].[BranchAddress_Country_PlanetId], [o1].[LeafBAddress_LeafBType], [o1].[PeriodEnd], [o1].[PeriodStart], [o1].[LeafBAddress_PlaceType], [o1].[LeafBAddress_Country_Name], [o1].[LeafBAddress_Country_PlanetId], [o2].[LeafAAddress_LeafType], [o2].[PeriodEnd], [o2].[PeriodStart], [o2].[LeafAAddress_PlaceType], [o2].[LeafAAddress_Country_Name], [o2].[LeafAAddress_Country_PlanetId]
FROM (
    SELECT * FROM "OwnedPerson"
) AS [m]
LEFT JOIN [OwnedPerson] AS [o] ON [m].[Id] = [o].[Id]
LEFT JOIN [OwnedPerson] AS [o0] ON [m].[Id] = [o0].[Id]
LEFT JOIN [OwnedPerson] AS [o1] ON [m].[Id] = [o1].[Id]
LEFT JOIN [OwnedPerson] AS [o2] ON [m].[Id] = [o2].[Id]
LEFT JOIN (
    SELECT [o3].[ClientId], [o3].[Id], [o3].[OrderDate], [o3].[PeriodEnd], [o3].[PeriodStart], [o4].[OrderClientId], [o4].[OrderId], [o4].[Id] AS [Id0], [o4].[Detail], [o4].[PeriodEnd] AS [PeriodEnd0], [o4].[PeriodStart] AS [PeriodStart0]
    FROM [Order] AS [o3]
    LEFT JOIN [OrderDetail] AS [o4] ON [o3].[ClientId] = [o4].[OrderClientId] AND [o3].[Id] = [o4].[OrderId]
) AS [t] ON [m].[Id] = [t].[ClientId]
ORDER BY [m].[Id], [o].[Id], [o0].[Id], [o1].[Id], [o2].[Id], [t].[ClientId], [t].[Id], [t].[OrderClientId], [t].[OrderId]
""");
    }

    public override async Task Projecting_collection_correlated_with_keyless_entity_after_navigation_works_using_parent_identifiers(
        bool async)
    {
        await base.Projecting_collection_correlated_with_keyless_entity_after_navigation_works_using_parent_identifiers(async);

        AssertSql(
"""
SELECT [b].[Throned_Value], [f].[Id], [b].[Id], [p].[Id], [p].[Name], [p].[PeriodEnd], [p].[PeriodStart], [p].[StarId]
FROM [Fink] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN [Barton] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [b] ON [f].[BartonId] = [b].[Id]
LEFT JOIN [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p] ON [b].[Throned_Value] <> [p].[Id] OR [b].[Throned_Value] IS NULL
ORDER BY [f].[Id], [b].[Id]
""");
    }

    public override async Task Filter_on_indexer_using_closure(bool async)
    {
        await base.Filter_on_indexer_using_closure(async);

        AssertSql(
"""
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [t].[ClientId], [t].[Id], [t].[OrderDate], [t].[PeriodEnd], [t].[PeriodStart], [t].[OrderClientId], [t].[OrderId], [t].[Id0], [t].[Detail], [t].[PeriodEnd0], [t].[PeriodStart0], [o].[PersonAddress_AddressLine], [o].[PeriodEnd], [o].[PeriodStart], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [t] ON [o].[Id] = [t].[ClientId]
WHERE [o].[PersonAddress_ZipCode] = 38654
ORDER BY [o].[Id], [t].[ClientId], [t].[Id], [t].[OrderClientId], [t].[OrderId]
""");
    }

    public override async Task Query_loads_reference_nav_automatically_in_projection(bool async)
    {
        await base.Query_loads_reference_nav_automatically_in_projection(async);

        AssertSql(
"""
SELECT TOP(2) [b].[Id], [b].[PeriodEnd], [b].[PeriodStart], [b].[Simple], [b].[Throned_Property], [b].[Throned_Value]
FROM [Fink] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN [Barton] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [b] ON [f].[BartonId] = [b].[Id]
""");
    }

    public override async Task Simple_query_entity_with_owned_collection(bool async)
    {
        await base.Simple_query_entity_with_owned_collection(async);

        AssertSql(
"""
SELECT [s].[Id], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [e].[Id], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[StarId]
FROM [Star] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
LEFT JOIN [Element] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e] ON [s].[Id] = [e].[StarId]
ORDER BY [s].[Id]
""");
    }

    public override async Task Left_join_on_entity_with_owned_navigations(bool async)
    {
        await base.Left_join_on_entity_with_owned_navigations(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Name], [p].[PeriodEnd], [p].[PeriodStart], [p].[StarId], [o].[Id], [o].[Discriminator], [o].[Name], [o].[PeriodEnd], [o].[PeriodStart], [t].[ClientId], [t].[Id], [t].[OrderDate], [t].[PeriodEnd], [t].[PeriodStart], [t].[OrderClientId], [t].[OrderId], [t].[Id0], [t].[Detail], [t].[PeriodEnd0], [t].[PeriodStart0], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId], [t0].[ClientId], [t0].[Id], [t0].[OrderDate], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[OrderClientId], [t0].[OrderId], [t0].[Id0], [t0].[Detail], [t0].[PeriodEnd0], [t0].[PeriodStart0]
FROM [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p]
LEFT JOIN [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o] ON [p].[Id] = [o].[Id]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [t] ON [o].[Id] = [t].[ClientId]
LEFT JOIN (
    SELECT [o2].[ClientId], [o2].[Id], [o2].[OrderDate], [o2].[PeriodEnd], [o2].[PeriodStart], [o3].[OrderClientId], [o3].[OrderId], [o3].[Id] AS [Id0], [o3].[Detail], [o3].[PeriodEnd] AS [PeriodEnd0], [o3].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o2]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o3] ON [o2].[ClientId] = [o3].[OrderClientId] AND [o2].[Id] = [o3].[OrderId]
) AS [t0] ON [o].[Id] = [t0].[ClientId]
ORDER BY [p].[Id], [o].[Id], [t].[ClientId], [t].[Id], [t].[OrderClientId], [t].[OrderId], [t].[Id0], [t0].[ClientId], [t0].[Id], [t0].[OrderClientId], [t0].[OrderId]
""");
    }

    public override async Task Left_join_on_entity_with_owned_navigations_complex(bool async)
    {
        await base.Left_join_on_entity_with_owned_navigations_complex(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Name], [p].[PeriodEnd], [p].[PeriodStart], [p].[StarId], [t].[Id], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t].[StarId], [t].[Id0], [t].[Discriminator], [t].[Name0], [t].[PeriodEnd0], [t].[PeriodStart0], [t0].[ClientId], [t0].[Id], [t0].[OrderDate], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[OrderClientId], [t0].[OrderId], [t0].[Id0], [t0].[Detail], [t0].[PeriodEnd0], [t0].[PeriodStart0], [t].[PersonAddress_AddressLine], [t].[PersonAddress_PlaceType], [t].[PersonAddress_ZipCode], [t].[PersonAddress_Country_Name], [t].[PersonAddress_Country_PlanetId], [t].[BranchAddress_BranchName], [t].[BranchAddress_PlaceType], [t].[BranchAddress_Country_Name], [t].[BranchAddress_Country_PlanetId], [t].[LeafBAddress_LeafBType], [t].[LeafBAddress_PlaceType], [t].[LeafBAddress_Country_Name], [t].[LeafBAddress_Country_PlanetId], [t].[LeafAAddress_LeafType], [t].[LeafAAddress_PlaceType], [t].[LeafAAddress_Country_Name], [t].[LeafAAddress_Country_PlanetId]
FROM [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p]
LEFT JOIN (
    SELECT DISTINCT [p0].[Id], [p0].[Name], [p0].[PeriodEnd], [p0].[PeriodStart], [p0].[StarId], [o].[Id] AS [Id0], [o].[Discriminator], [o].[Name] AS [Name0], [o].[PeriodEnd] AS [PeriodEnd0], [o].[PeriodStart] AS [PeriodStart0], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
    FROM [Planet] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [p0]
    LEFT JOIN [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o] ON [p0].[Id] = [o].[Id]
) AS [t] ON [p].[Id] = [t].[Id0]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o0].[PeriodEnd], [o0].[PeriodStart], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail], [o1].[PeriodEnd] AS [PeriodEnd0], [o1].[PeriodStart] AS [PeriodStart0]
    FROM [Order] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    LEFT JOIN [OrderDetail] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [t0] ON [t].[Id0] = [t0].[ClientId]
ORDER BY [p].[Id], [t].[Id], [t].[Id0], [t0].[ClientId], [t0].[Id], [t0].[OrderClientId], [t0].[OrderId]
""");
    }

    public override async Task GroupBy_aggregate_on_owned_navigation_in_aggregate_selector(bool async)
    {
        await base.GroupBy_aggregate_on_owned_navigation_in_aggregate_selector(async);

        AssertSql(
"""
SELECT [o].[Id] AS [Key], (
    SELECT COALESCE(SUM([o0].[PersonAddress_Country_PlanetId]), 0)
    FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o0]
    WHERE [o].[Id] = [o0].[Id]) AS [Sum]
FROM [OwnedPerson] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [o]
GROUP BY [o].[Id]
""");
    }

    // not AssertQuery so original (non-temporal) query gets executed, but data is modified
    // so results don't match expectations
    public override Task Owned_entity_without_owner_does_not_throw_for_identity_resolution(bool async, bool useAsTracking)
        => Task.CompletedTask;

    public override Task Preserve_includes_when_applying_skip_take_after_anonymous_type_select(bool async)
        => Task.CompletedTask;

    public override Task Query_on_collection_entry_works_for_owned_collection(bool async)
        => Task.CompletedTask;

    public override Task NoTracking_Include_with_cycles_does_not_throw_when_performing_identity_resolution(
        bool async,
        bool useAsTracking)
        => Task.CompletedTask;

    public override Task Filter_on_indexer_using_function_argument(bool async)
        => Task.CompletedTask;

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class TemporalOwnedQuerySqlServerFixture : RelationalOwnedQueryFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override string StoreName
            => "TemporalOwnedQueryTest";

        public DateTime ChangesDate { get; private set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<OwnedPerson>(
                eb =>
                {
                    eb.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                                ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
                            }));
                    eb.IndexerProperty<string>("Name");
                    var ownedPerson = new OwnedPerson { Id = 1 };
                    ownedPerson["Name"] = "Mona Cy";
                    eb.HasData(ownedPerson);

                    eb.OwnsOne(
                        p => p.PersonAddress, ab =>
                        {
                            ab.ToTable(
                                tb => tb.IsTemporal(
                                    ttb =>
                                    {
                                        ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                                        ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
                                    }));
                            ab.IndexerProperty<string>("AddressLine");
                            ab.IndexerProperty(typeof(int), "ZipCode");
                            ab.HasData(
                                new
                                {
                                    OwnedPersonId = 1,
                                    PlaceType = "Land",
                                    AddressLine = "804 S. Lakeshore Road",
                                    ZipCode = 38654
                                },
                                new
                                {
                                    OwnedPersonId = 2,
                                    PlaceType = "Land",
                                    AddressLine = "7 Church Dr.",
                                    ZipCode = 28655
                                },
                                new
                                {
                                    OwnedPersonId = 3,
                                    PlaceType = "Land",
                                    AddressLine = "72 Hickory Rd.",
                                    ZipCode = 07728
                                },
                                new
                                {
                                    OwnedPersonId = 4,
                                    PlaceType = "Land",
                                    AddressLine = "28 Strawberry St.",
                                    ZipCode = 19053
                                });

                            ab.OwnsOne(
                                a => a.Country, cb =>
                                {
                                    cb.ToTable(
                                        tb => tb.IsTemporal(
                                            ttb =>
                                            {
                                                ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                                                ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
                                            }));
                                    cb.HasData(
                                        new
                                        {
                                            OwnedAddressOwnedPersonId = 1,
                                            PlanetId = 1,
                                            Name = "USA"
                                        },
                                        new
                                        {
                                            OwnedAddressOwnedPersonId = 2,
                                            PlanetId = 1,
                                            Name = "USA"
                                        },
                                        new
                                        {
                                            OwnedAddressOwnedPersonId = 3,
                                            PlanetId = 1,
                                            Name = "USA"
                                        },
                                        new
                                        {
                                            OwnedAddressOwnedPersonId = 4,
                                            PlanetId = 1,
                                            Name = "USA"
                                        });

                                    cb.HasOne(cc => cc.Planet).WithMany().HasForeignKey(ee => ee.PlanetId)
                                        .OnDelete(DeleteBehavior.Restrict);
                                });
                        });

                    eb.OwnsMany(
                        p => p.Orders, ob =>
                        {
                            ob.ToTable(
                                tb => tb.IsTemporal(
                                    ttb =>
                                    {
                                        ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                                        ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
                                    }));
                            ob.IndexerProperty<DateTime>("OrderDate");
                            ob.HasData(
                                new
                                {
                                    Id = -10,
                                    ClientId = 1,
                                    OrderDate = Convert.ToDateTime("2018-07-11 10:01:41")
                                },
                                new
                                {
                                    Id = -11,
                                    ClientId = 1,
                                    OrderDate = Convert.ToDateTime("2015-03-03 04:37:59")
                                },
                                new
                                {
                                    Id = -20,
                                    ClientId = 2,
                                    OrderDate = Convert.ToDateTime("2015-05-25 20:35:48")
                                },
                                new
                                {
                                    Id = -30,
                                    ClientId = 3,
                                    OrderDate = Convert.ToDateTime("2014-11-10 04:32:42")
                                },
                                new
                                {
                                    Id = -40,
                                    ClientId = 4,
                                    OrderDate = Convert.ToDateTime("2016-04-25 19:23:56")
                                }
                            );

                            ob.OwnsMany(
                                e => e.Details, odb =>
                                {
                                    odb.ToTable(
                                        tb => tb.IsTemporal(
                                            ttb =>
                                            {
                                                ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                                                ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
                                            }));
                                    odb.HasData(
                                        new
                                        {
                                            Id = -100,
                                            OrderId = -10,
                                            OrderClientId = 1,
                                            Detail = "Discounted Order"
                                        },
                                        new
                                        {
                                            Id = -101,
                                            OrderId = -10,
                                            OrderClientId = 1,
                                            Detail = "Full Price Order"
                                        },
                                        new
                                        {
                                            Id = -200,
                                            OrderId = -20,
                                            OrderClientId = 2,
                                            Detail = "Internal Order"
                                        },
                                        new
                                        {
                                            Id = -300,
                                            OrderId = -30,
                                            OrderClientId = 3,
                                            Detail = "Bulk Order"
                                        });
                                });
                        });
                });

            modelBuilder.Entity<Branch>(
                eb =>
                {
                    eb.HasData(new { Id = 2, Name = "Antigonus Mitul" });

                    eb.OwnsOne(
                        p => p.BranchAddress, ab =>
                        {
                            ab.ToTable(
                                tb => tb.IsTemporal(
                                    ttb =>
                                    {
                                        ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                                        ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
                                    }));
                            ab.IndexerProperty<string>("BranchName").IsRequired();
                            ab.HasData(
                                new
                                {
                                    BranchId = 2,
                                    PlaceType = "Land",
                                    BranchName = "BranchA"
                                },
                                new
                                {
                                    BranchId = 3,
                                    PlaceType = "Land",
                                    BranchName = "BranchB"
                                });

                            ab.OwnsOne(
                                a => a.Country, cb =>
                                {
                                    cb.ToTable(
                                        tb => tb.IsTemporal(
                                            ttb =>
                                            {
                                                ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                                                ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
                                            }));
                                    cb.HasData(
                                        new
                                        {
                                            OwnedAddressBranchId = 2,
                                            PlanetId = 1,
                                            Name = "Canada"
                                        },
                                        new
                                        {
                                            OwnedAddressBranchId = 3,
                                            PlanetId = 1,
                                            Name = "Canada"
                                        });
                                });
                        });
                });

            modelBuilder.Entity<LeafA>(
                eb =>
                {
                    var leafA = new LeafA { Id = 3 };
                    leafA["Name"] = "Madalena Morana";
                    eb.HasData(leafA);

                    eb.OwnsOne(
                        p => p.LeafAAddress, ab =>
                        {
                            ab.ToTable(
                                tb => tb.IsTemporal(
                                    ttb =>
                                    {
                                        ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                                        ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
                                    }));
                            ab.IndexerProperty<int>("LeafType");

                            ab.HasData(
                                new
                                {
                                    LeafAId = 3,
                                    PlaceType = "Land",
                                    LeafType = 1
                                });

                            ab.OwnsOne(
                                a => a.Country, cb =>
                                {
                                    cb.ToTable(
                                        tb => tb.IsTemporal(
                                            ttb =>
                                            {
                                                ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                                                ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
                                            }));
                                    cb.HasOne(c => c.Planet).WithMany().HasForeignKey(c => c.PlanetId)
                                        .OnDelete(DeleteBehavior.Restrict);

                                    cb.HasData(
                                        new
                                        {
                                            OwnedAddressLeafAId = 3,
                                            PlanetId = 1,
                                            Name = "Mexico"
                                        });
                                });
                        });
                });

            modelBuilder.Entity<LeafB>(
                eb =>
                {
                    var leafB = new LeafB { Id = 4 };
                    leafB["Name"] = "Vanda Waldemar";
                    eb.HasData(leafB);

                    eb.OwnsOne(
                        p => p.LeafBAddress, ab =>
                        {
                            ab.ToTable(
                                tb => tb.IsTemporal(
                                    ttb =>
                                    {
                                        ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                                        ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
                                    }));
                            ab.IndexerProperty<string>("LeafBType").IsRequired();
                            ab.HasData(
                                new
                                {
                                    LeafBId = 4,
                                    PlaceType = "Land",
                                    LeafBType = "Green"
                                });

                            ab.OwnsOne(
                                a => a.Country, cb =>
                                {
                                    cb.ToTable(
                                        tb => tb.IsTemporal(
                                            ttb =>
                                            {
                                                ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                                                ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
                                            }));
                                    cb.HasOne(c => c.Planet).WithMany().HasForeignKey(c => c.PlanetId)
                                        .OnDelete(DeleteBehavior.Restrict);

                                    cb.HasData(
                                        new
                                        {
                                            OwnedAddressLeafBId = 4,
                                            PlanetId = 1,
                                            Name = "Panama"
                                        });
                                });
                        });
                });

            modelBuilder.Entity<Planet>(
                pb =>
                {
                    pb.ToTable(tb => tb.IsTemporal());
                    pb.HasData(
                        new Planet
                        {
                            Id = 1,
                            StarId = 1,
                            Name = "Earth"
                        });
                });

            modelBuilder.Entity<Moon>(
                mb =>
                {
                    mb.ToTable(tb => tb.IsTemporal());
                    mb.HasData(
                        new Moon
                        {
                            Id = 1,
                            PlanetId = 1,
                            Diameter = 3474
                        });
                });

            modelBuilder.Entity<Star>(
                sb =>
                {
                    sb.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                                ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
                            }));
                    sb.HasData(new Star { Id = 1, Name = "Sol" });
                    sb.OwnsMany(
                        s => s.Composition, ob =>
                        {
                            ob.ToTable(
                                tb => tb.IsTemporal(
                                    ttb =>
                                    {
                                        ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                                        ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
                                    }));
                            ob.HasKey(e => e.Id);
                            ob.HasData(
                                new
                                {
                                    Id = "H",
                                    Name = "Hydrogen",
                                    StarId = 1
                                },
                                new
                                {
                                    Id = "He",
                                    Name = "Helium",
                                    StarId = 1
                                });
                        });
                });

            modelBuilder.Entity<Barton>(
                b =>
                {
                    b.ToTable(
                        tb => tb.IsTemporal(
                            ttb =>
                            {
                                ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                                ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
                            }));
                    b.OwnsOne(
                        e => e.Throned, b =>
                        {
                            b.ToTable(
                                tb => tb.IsTemporal(
                                    ttb =>
                                    {
                                        ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                                        ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
                                    }));
                            b.HasData(
                                new
                                {
                                    BartonId = 1,
                                    Property = "Property",
                                    Value = 42
                                });
                        });
                    b.HasData(
                        new Barton { Id = 1, Simple = "Simple" },
                        new Barton { Id = 2, Simple = "Not" });
                });

            modelBuilder.Entity<Fink>()
                .ToTable(tb => tb.IsTemporal())
                .HasData(new { Id = 1, BartonId = 1 });

            modelBuilder.Entity<Balloon>();
            modelBuilder.Entity<HydrogenBalloon>().OwnsOne(e => e.Gas);
            modelBuilder.Entity<HeliumBalloon>().OwnsOne(e => e.Gas);
        }

        protected override void Seed(PoolableDbContext context)
        {
            base.Seed(context);

            ChangesDate = new DateTime(2010, 1, 1);

            var ownedPeople = context.Set<OwnedPerson>().AsTracking().ToList();
            foreach (var ownedPerson in ownedPeople)
            {
                ownedPerson["Name"] = "Modified" + ownedPerson["Name"];
                var orders = ownedPerson.Orders;
                foreach (var order in orders)
                {
                    order["OrderDate"] = new DateTime(1000, 1, 1);
                    var details = order.Details;
                    foreach (var detail in details)
                    {
                        detail.Detail = "Modified" + detail.Detail;
                    }
                }
            }

            var stars = context.Set<Star>().AsTracking().ToList();
            foreach (var star in stars)
            {
                star.Name = "Modified" + star.Name;
                if (star.Composition.Any())
                {
                    foreach (var comp in star.Composition)
                    {
                        comp.Name = "Modified" + comp.Name;
                    }
                }
            }

            var planets = context.Set<Planet>().AsTracking().ToList();
            foreach (var planet in planets)
            {
                planet.Name = "Modified" + planet.Name;
            }

            var moons = context.Set<Moon>().AsTracking().ToList();
            foreach (var moon in moons)
            {
                moon.Diameter += 1000;
            }

            var finks = context.Set<Fink>().AsTracking().ToList();
            context.Set<Fink>().RemoveRange(finks);

            var bartons = context.Set<Barton>().Include(x => x.Throned).AsTracking().ToList();
            foreach (var barton in bartons)
            {
                barton.Simple = "Modified" + barton.Simple;
                if (barton.Throned != null)
                {
                    barton.Throned.Property = "Modified" + barton.Throned.Property;
                }
            }

            context.SaveChanges();

            var tableNames = new List<string>
            {
                nameof(Barton),
                nameof(Element),
                nameof(Fink),
                nameof(Moon),
                nameof(Order),
                nameof(OrderDetail),
                nameof(OwnedPerson),
                nameof(Planet),
                nameof(Star),
            };

            foreach (var tableName in tableNames)
            {
                context.Database.ExecuteSqlRaw($"ALTER TABLE [{tableName}] SET (SYSTEM_VERSIONING = OFF)");
                context.Database.ExecuteSqlRaw($"ALTER TABLE [{tableName}] DROP PERIOD FOR SYSTEM_TIME");

                context.Database.ExecuteSqlRaw($"UPDATE [{tableName + "History"}] SET PeriodStart = '2000-01-01T01:00:00.0000000Z'");
                context.Database.ExecuteSqlRaw($"UPDATE [{tableName + "History"}] SET PeriodEnd = '2020-07-01T07:00:00.0000000Z'");

                context.Database.ExecuteSqlRaw($"ALTER TABLE [{tableName}] ADD PERIOD FOR SYSTEM_TIME ([PeriodStart], [PeriodEnd])");
                context.Database.ExecuteSqlRaw(
                    $"ALTER TABLE [{tableName}] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[{tableName + "History"}]))");
            }
        }
    }
}
