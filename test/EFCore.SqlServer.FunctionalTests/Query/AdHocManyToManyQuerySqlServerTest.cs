// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class AdHocManyToManyQuerySqlServerTest : AdHocManyToManyQueryRelationalTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    public override async Task SelectMany_with_collection_selector_having_subquery()
    {
        await base.SelectMany_with_collection_selector_having_subquery();

        AssertSql(
            """
SELECT [u].[Id] AS [UserId], [s].[Id] AS [OrgId]
FROM [Users] AS [u]
CROSS JOIN (
    SELECT [o1].[Id]
    FROM (
        SELECT 1 AS empty
    ) AS [e]
    LEFT JOIN (
        SELECT [o].[Id]
        FROM [Organisations] AS [o]
        WHERE EXISTS (
            SELECT 1
            FROM [OrganisationUser] AS [o0]
            WHERE [o].[Id] = [o0].[OrganisationId])
    ) AS [o1] ON 1 = 1
) AS [s]
""");
    }

    public override async Task Many_to_many_load_works_when_join_entity_has_custom_key(bool async)
    {
        await base.Many_to_many_load_works_when_join_entity_has_custom_key(async);

        AssertSql(
            """
@__p_0='1'

SELECT TOP(1) [m].[Id]
FROM [ManyM_DB] AS [m]
WHERE [m].[Id] = @__p_0
""",
            //
            """
@__p_0='1'

SELECT [s].[Id], [m].[Id], [s].[Id0], [s0].[Id], [s0].[ManyM_Id], [s0].[ManyN_Id], [s0].[Id0]
FROM [ManyM_DB] AS [m]
INNER JOIN (
    SELECT [m1].[Id], [m0].[Id] AS [Id0], [m0].[ManyM_Id]
    FROM [ManyMN_DB] AS [m0]
    LEFT JOIN [ManyN_DB] AS [m1] ON [m0].[ManyN_Id] = [m1].[Id]
) AS [s] ON [m].[Id] = [s].[ManyM_Id]
LEFT JOIN (
    SELECT [m2].[Id], [m2].[ManyM_Id], [m2].[ManyN_Id], [m3].[Id] AS [Id0]
    FROM [ManyMN_DB] AS [m2]
    INNER JOIN [ManyM_DB] AS [m3] ON [m2].[ManyM_Id] = [m3].[Id]
    WHERE [m3].[Id] = @__p_0
) AS [s0] ON [s].[Id] = [s0].[ManyN_Id]
WHERE [m].[Id] = @__p_0
ORDER BY [m].[Id], [s].[Id0], [s].[Id], [s0].[Id]
""",
            //
            """
@__p_0='1'

SELECT TOP(1) [m].[Id]
FROM [ManyN_DB] AS [m]
WHERE [m].[Id] = @__p_0
""",
            //
            """
@__p_0='1'

SELECT [s].[Id], [m].[Id], [s].[Id0], [s0].[Id], [s0].[ManyM_Id], [s0].[ManyN_Id], [s0].[Id0]
FROM [ManyN_DB] AS [m]
INNER JOIN (
    SELECT [m1].[Id], [m0].[Id] AS [Id0], [m0].[ManyN_Id]
    FROM [ManyMN_DB] AS [m0]
    INNER JOIN [ManyM_DB] AS [m1] ON [m0].[ManyM_Id] = [m1].[Id]
) AS [s] ON [m].[Id] = [s].[ManyN_Id]
LEFT JOIN (
    SELECT [m2].[Id], [m2].[ManyM_Id], [m2].[ManyN_Id], [m3].[Id] AS [Id0]
    FROM [ManyMN_DB] AS [m2]
    INNER JOIN [ManyN_DB] AS [m3] ON [m2].[ManyN_Id] = [m3].[Id]
    WHERE [m3].[Id] = @__p_0
) AS [s0] ON [s].[Id] = [s0].[ManyM_Id]
WHERE [m].[Id] = @__p_0
ORDER BY [m].[Id], [s].[Id0], [s].[Id], [s0].[Id]
""");
    }
}
