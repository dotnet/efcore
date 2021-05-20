// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class OwnedEntityQuerySqlServerTest : OwnedEntityQueryRelationalTestBase
    {
        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

        public override async Task Multiple_single_result_in_projection_containing_owned_types(bool async)
        {
            await base.Multiple_single_result_in_projection_containing_owned_types(async);

            AssertSql(
                @"SELECT [e].[Id], [t0].[Id], [t0].[Entity20277Id], [t0].[Owned_IsDeleted], [t0].[Owned_Value], [t0].[Type], [t0].[c], [t1].[Id], [t1].[Entity20277Id], [t1].[Owned_IsDeleted], [t1].[Owned_Value], [t1].[Type], [t1].[c]
FROM [Entities] AS [e]
LEFT JOIN (
    SELECT [t].[Id], [t].[Entity20277Id], [t].[Owned_IsDeleted], [t].[Owned_Value], [t].[Type], [t].[c]
    FROM (
        SELECT [c].[Id], [c].[Entity20277Id], [c].[Owned_IsDeleted], [c].[Owned_Value], [c].[Type], 1 AS [c], ROW_NUMBER() OVER(PARTITION BY [c].[Entity20277Id] ORDER BY [c].[Entity20277Id], [c].[Id]) AS [row]
        FROM [Child20277] AS [c]
        WHERE [c].[Type] = 1
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [e].[Id] = [t0].[Entity20277Id]
LEFT JOIN (
    SELECT [t2].[Id], [t2].[Entity20277Id], [t2].[Owned_IsDeleted], [t2].[Owned_Value], [t2].[Type], [t2].[c]
    FROM (
        SELECT [c0].[Id], [c0].[Entity20277Id], [c0].[Owned_IsDeleted], [c0].[Owned_Value], [c0].[Type], 1 AS [c], ROW_NUMBER() OVER(PARTITION BY [c0].[Entity20277Id] ORDER BY [c0].[Entity20277Id], [c0].[Id]) AS [row]
        FROM [Child20277] AS [c0]
        WHERE [c0].[Type] = 2
    ) AS [t2]
    WHERE [t2].[row] <= 1
) AS [t1] ON [e].[Id] = [t1].[Entity20277Id]");
        }

        public override async Task Multiple_owned_reference_mapped_to_own_table_containing_owned_collection_in_split_query(bool async)
        {
            await base.Multiple_owned_reference_mapped_to_own_table_containing_owned_collection_in_split_query(async);

            AssertSql(
                @"SELECT TOP(2) [r].[Id], [m].[Id], [m].[Enabled], [m].[RootId], [m0].[Id], [m0].[RootId]
FROM [Root24777] AS [r]
LEFT JOIN [MiddleB24777] AS [m] ON [r].[Id] = [m].[RootId]
LEFT JOIN [ModdleA24777] AS [m0] ON [r].[Id] = [m0].[RootId]
WHERE [r].[Id] = 3
ORDER BY [r].[Id], [m].[Id], [m0].[Id]",
                //
                @"SELECT [l].[ModdleAId], [l].[UnitThreshold], [t].[Id], [t].[Id0], [t].[Id1]
FROM (
    SELECT TOP(1) [r].[Id], [m].[Id] AS [Id0], [m0].[Id] AS [Id1]
    FROM [Root24777] AS [r]
    LEFT JOIN [MiddleB24777] AS [m] ON [r].[Id] = [m].[RootId]
    LEFT JOIN [ModdleA24777] AS [m0] ON [r].[Id] = [m0].[RootId]
    WHERE [r].[Id] = 3
) AS [t]
INNER JOIN [Leaf24777] AS [l] ON [t].[Id1] = [l].[ModdleAId]
ORDER BY [t].[Id], [t].[Id0], [t].[Id1]");
        }
    }
}
