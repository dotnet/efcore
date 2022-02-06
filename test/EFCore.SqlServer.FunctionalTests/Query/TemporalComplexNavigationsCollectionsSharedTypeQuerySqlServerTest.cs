// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query;

public class TemporalComplexNavigationsCollectionsSharedTypeQuerySqlServerTest : ComplexNavigationsCollectionsSharedTypeQueryRelationalTestBase<
    TemporalComplexNavigationsSharedTypeQuerySqlServerFixture>
{
    public TemporalComplexNavigationsCollectionsSharedTypeQuerySqlServerTest(
        TemporalComplexNavigationsSharedTypeQuerySqlServerFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
    {
        var temporalEntityTypes = new List<Type>
            {
                typeof(Level1),
                typeof(Level2),
                typeof(Level3),
                typeof(Level4),
            };

        var rewriter = new TemporalPointInTimeQueryRewriter(Fixture.ChangesDate, temporalEntityTypes);

        return rewriter.Visit(serverQueryExpression);
    }

    public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
    {
        await base.Complex_multi_include_with_order_by_and_paging(async);

        AssertSql(
            @"@__p_0='0'
@__p_1='10'

SELECT [t].[Id], [t].[Date], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t1].[Id], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Id0], [t1].[Id00], [t3].[Id], [t3].[Level2_Optional_Id], [t3].[Level2_Required_Id], [t3].[Level3_Name], [t3].[OneToMany_Optional_Inverse3Id], [t3].[OneToMany_Required_Inverse3Id], [t3].[OneToOne_Optional_PK_Inverse3Id], [t3].[PeriodEnd], [t3].[PeriodStart], [t3].[Id0], [t3].[Id00]
FROM (
    SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    ORDER BY [l].[Name]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Level1_Required_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t2] ON [l2].[Id] = [t2].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t1] ON CASE
    WHEN [t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t0].[PeriodEnd] IS NOT NULL AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
END = [t1].[OneToMany_Optional_Inverse3Id]
LEFT JOIN (
    SELECT [l5].[Id], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Level3_Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t4].[Id] AS [Id0], [t4].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
    INNER JOIN (
        SELECT [l6].[Id], [l7].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7] ON [l6].[Id] = [l7].[Id]
        WHERE [l6].[OneToOne_Required_PK_Date] IS NOT NULL AND [l6].[Level1_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t4] ON [l5].[Id] = [t4].[Id]
    WHERE [l5].[Level2_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t3] ON CASE
    WHEN [t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t0].[PeriodEnd] IS NOT NULL AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
END = [t3].[OneToMany_Required_Inverse3Id]
ORDER BY [t].[Name], [t].[Id], [t0].[Id], [t0].[Id0], [t1].[Id], [t1].[Id0], [t1].[Id00], [t3].[Id], [t3].[Id0]");
    }

    public override async Task Complex_multi_include_with_order_by_and_paging_joins_on_correct_key(bool async)
    {
        await base.Complex_multi_include_with_order_by_and_paging_joins_on_correct_key(async);

        AssertSql(
            @"@__p_0='0'
@__p_1='10'

SELECT [t].[Id], [t].[Date], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t1].[Id], [t1].[Id0], [t2].[Id], [t2].[Level2_Optional_Id], [t2].[Level2_Required_Id], [t2].[Level3_Name], [t2].[OneToMany_Optional_Inverse3Id], [t2].[OneToMany_Required_Inverse3Id], [t2].[OneToOne_Optional_PK_Inverse3Id], [t2].[PeriodEnd], [t2].[PeriodStart], [t2].[Id0], [t2].[Id00], [t1].[OneToOne_Required_PK_Date], [t1].[Level1_Optional_Id], [t1].[Level1_Required_Id], [t1].[Level2_Name], [t1].[OneToMany_Optional_Inverse2Id], [t1].[OneToMany_Required_Inverse2Id], [t1].[OneToOne_Optional_PK_Inverse2Id], [t1].[PeriodEnd], [t1].[PeriodStart], [t4].[Id], [t4].[Level2_Optional_Id], [t4].[Level2_Required_Id], [t4].[Level3_Name], [t4].[OneToMany_Optional_Inverse3Id], [t4].[OneToMany_Required_Inverse3Id], [t4].[OneToOne_Optional_PK_Inverse3Id], [t4].[PeriodEnd], [t4].[PeriodStart], [t4].[Id0], [t4].[Id00]
FROM (
    SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    ORDER BY [l].[Name]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[PeriodEnd], [l2].[PeriodStart], [l3].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3] ON [l2].[Id] = [l3].[Id]
    WHERE [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t1] ON [t].[Id] = [t1].[Level1_Required_Id]
LEFT JOIN (
    SELECT [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id], [l4].[PeriodEnd], [l4].[PeriodStart], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4]
    INNER JOIN (
        SELECT [l5].[Id], [l6].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6] ON [l5].[Id] = [l6].[Id]
        WHERE [l5].[OneToOne_Required_PK_Date] IS NOT NULL AND [l5].[Level1_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t3] ON [l4].[Id] = [t3].[Id]
    WHERE [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t2] ON CASE
    WHEN [t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t0].[PeriodEnd] IS NOT NULL AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
END = [t2].[OneToMany_Optional_Inverse3Id]
LEFT JOIN (
    SELECT [l7].[Id], [l7].[Level2_Optional_Id], [l7].[Level2_Required_Id], [l7].[Level3_Name], [l7].[OneToMany_Optional_Inverse3Id], [l7].[OneToMany_Required_Inverse3Id], [l7].[OneToOne_Optional_PK_Inverse3Id], [l7].[PeriodEnd], [l7].[PeriodStart], [t5].[Id] AS [Id0], [t5].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
    INNER JOIN (
        SELECT [l8].[Id], [l9].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l9] ON [l8].[Id] = [l9].[Id]
        WHERE [l8].[OneToOne_Required_PK_Date] IS NOT NULL AND [l8].[Level1_Required_Id] IS NOT NULL AND [l8].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t5] ON [l7].[Id] = [t5].[Id]
    WHERE [l7].[Level2_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t4] ON CASE
    WHEN [t1].[OneToOne_Required_PK_Date] IS NOT NULL AND [t1].[Level1_Required_Id] IS NOT NULL AND [t1].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t1].[PeriodEnd] IS NOT NULL AND [t1].[PeriodStart] IS NOT NULL THEN [t1].[Id]
END = [t4].[OneToMany_Required_Inverse3Id]
ORDER BY [t].[Name], [t].[Id], [t0].[Id], [t0].[Id0], [t1].[Id], [t1].[Id0], [t2].[Id], [t2].[Id0], [t2].[Id00], [t4].[Id], [t4].[Id0]");
    }

    public override async Task Complex_multi_include_with_order_by_and_paging_joins_on_correct_key2(bool async)
    {
        await base.Complex_multi_include_with_order_by_and_paging_joins_on_correct_key2(async);

        AssertSql(
            @"@__p_0='0'
@__p_1='10'

SELECT [t].[Id], [t].[Date], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t1].[Id], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd], [t1].[PeriodStart], [t0].[Id0], [t1].[Id0], [t1].[Id00], [t3].[Id], [t3].[Level3_Optional_Id], [t3].[Level3_Required_Id], [t3].[Level4_Name], [t3].[OneToMany_Optional_Inverse4Id], [t3].[OneToMany_Required_Inverse4Id], [t3].[OneToOne_Optional_PK_Inverse4Id], [t3].[PeriodEnd], [t3].[PeriodStart], [t3].[Id0], [t3].[Id00], [t3].[Id000]
FROM (
    SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    ORDER BY [l].[Name]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t2] ON [l2].[Id] = [t2].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t1] ON CASE
    WHEN [t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t0].[PeriodEnd] IS NOT NULL AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
END = [t1].[Level2_Required_Id]
LEFT JOIN (
    SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t4].[Id] AS [Id0], [t4].[Id0] AS [Id00], [t4].[Id00] AS [Id000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
    INNER JOIN (
        SELECT [l6].[Id], [t5].[Id] AS [Id0], [t5].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
        INNER JOIN (
            SELECT [l7].[Id], [l8].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8] ON [l7].[Id] = [l8].[Id]
            WHERE [l7].[OneToOne_Required_PK_Date] IS NOT NULL AND [l7].[Level1_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t5] ON [l6].[Id] = [t5].[Id]
        WHERE [l6].[Level2_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t4] ON [l5].[Id] = [t4].[Id]
    WHERE [l5].[Level3_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse4Id] IS NOT NULL
) AS [t3] ON CASE
    WHEN [t1].[Level2_Required_Id] IS NOT NULL AND [t1].[OneToMany_Required_Inverse3Id] IS NOT NULL AND [t1].[PeriodEnd] IS NOT NULL AND [t1].[PeriodStart] IS NOT NULL THEN [t1].[Id]
END = [t3].[OneToMany_Optional_Inverse4Id]
ORDER BY [t].[Name], [t].[Id], [t0].[Id], [t0].[Id0], [t1].[Id], [t1].[Id0], [t1].[Id00], [t3].[Id], [t3].[Id0], [t3].[Id00]");
    }

    public override async Task Complex_query_with_let_collection_projection_FirstOrDefault(bool async)
    {
        await base.Complex_query_with_let_collection_projection_FirstOrDefault(async);

        AssertSql(
            @"SELECT [l].[Id], [t0].[Id], [t0].[Id0], [t1].[Name], [t1].[Id], [t0].[c]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[c], [t].[Id], [t].[Id0], [t].[OneToOne_Required_PK_Date], [t].[Level1_Required_Id], [t].[OneToMany_Required_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[OneToMany_Optional_Inverse2Id]
    FROM (
        SELECT 1 AS [c], [l0].[Id], [l1].[Id] AS [Id0], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l0].[OneToMany_Optional_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id], [l1].[Id]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l0].[Level2_Name] <> N'Foo' OR [l0].[Level2_Name] IS NULL)
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
OUTER APPLY (
    SELECT [l2].[Name], [l2].[Id]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    WHERE EXISTS (
        SELECT 1
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l2].[Id] = [l3].[OneToMany_Optional_Inverse2Id] AND [l3].[Id] = CASE
            WHEN [t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t0].[Id]
        END)
) AS [t1]
ORDER BY [l].[Id], [t0].[Id], [t0].[Id0]");
    }

    public override async Task Complex_query_with_let_collection_projection_FirstOrDefault_with_ToList_on_inner_and_outer(bool async)
    {
        // Nested collection with ToList. Issue #23303.
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => base.Complex_query_with_let_collection_projection_FirstOrDefault_with_ToList_on_inner_and_outer(async));

        AssertSql(@"SELECT [l].[Id], [t0].[Id], [t0].[Id0], [t1].[Name], [t1].[Id], [t0].[c]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[c], [t].[Id], [t].[Id0], [t].[OneToOne_Required_PK_Date], [t].[Level1_Required_Id], [t].[OneToMany_Required_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[OneToMany_Optional_Inverse2Id]
    FROM (
        SELECT 1 AS [c], [l0].[Id], [l1].[Id] AS [Id0], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l0].[OneToMany_Optional_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id], [l1].[Id]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l0].[Level2_Name] <> N'Foo' OR [l0].[Level2_Name] IS NULL)
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
OUTER APPLY (
    SELECT [l2].[Name], [l2].[Id]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    WHERE EXISTS (
        SELECT 1
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l2].[Id] = [l3].[OneToMany_Optional_Inverse2Id] AND [l3].[Id] = CASE
            WHEN [t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t0].[Id]
        END)
) AS [t1]
ORDER BY [l].[Id], [t0].[Id], [t0].[Id0]");
    }

    public override async Task Filtered_include_after_different_filtered_include_different_level(bool async)
    {
        await base.Filtered_include_after_different_filtered_include_different_level(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t3].[Id], [t3].[OneToOne_Required_PK_Date], [t3].[Level1_Optional_Id], [t3].[Level1_Required_Id], [t3].[Level2_Name], [t3].[OneToMany_Optional_Inverse2Id], [t3].[OneToMany_Required_Inverse2Id], [t3].[OneToOne_Optional_PK_Inverse2Id], [t3].[PeriodEnd], [t3].[PeriodStart], [t3].[Id0], [t3].[Id1], [t3].[Level2_Optional_Id], [t3].[Level2_Required_Id], [t3].[Level3_Name], [t3].[OneToMany_Optional_Inverse3Id], [t3].[OneToMany_Required_Inverse3Id], [t3].[OneToOne_Optional_PK_Inverse3Id], [t3].[PeriodEnd0], [t3].[PeriodStart0], [t3].[Id00], [t3].[Id000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
OUTER APPLY (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t1].[Id] AS [Id1], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd] AS [PeriodEnd0], [t1].[PeriodStart] AS [PeriodStart0], [t1].[Id0] AS [Id00], [t1].[Id00] AS [Id000]
    FROM (
        SELECT TOP(3) [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id] AND ([l0].[Level2_Name] <> N'Foo' OR [l0].[Level2_Name] IS NULL)
        ORDER BY [l0].[Level2_Name]
    ) AS [t]
    LEFT JOIN (
        SELECT [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00]
        FROM (
            SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00], ROW_NUMBER() OVER(PARTITION BY [l2].[OneToMany_Required_Inverse3Id] ORDER BY [l2].[Level3_Name] DESC) AS [row]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
            INNER JOIN (
                SELECT [l3].[Id], [l4].[Id] AS [Id0]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
                INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
                WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
            ) AS [t2] ON [l2].[Id] = [t2].[Id]
            WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL AND ([l2].[Level3_Name] <> N'Bar' OR [l2].[Level3_Name] IS NULL)
        ) AS [t0]
        WHERE 1 < [t0].[row]
    ) AS [t1] ON CASE
        WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t].[Id]
    END = [t1].[OneToMany_Required_Inverse3Id]
) AS [t3]
ORDER BY [l].[Id], [t3].[Level2_Name], [t3].[Id], [t3].[Id0], [t3].[OneToMany_Required_Inverse3Id], [t3].[Level3_Name] DESC, [t3].[Id1], [t3].[Id00]");
    }

    public override async Task Filtered_include_after_different_filtered_include_same_level(bool async)
    {
        await base.Filtered_include_after_different_filtered_include_same_level(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t1].[Id], [t1].[OneToOne_Required_PK_Date], [t1].[Level1_Optional_Id], [t1].[Level1_Required_Id], [t1].[Level2_Name], [t1].[OneToMany_Optional_Inverse2Id], [t1].[OneToMany_Required_Inverse2Id], [t1].[OneToOne_Optional_PK_Inverse2Id], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Level2_Name]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l0].[Level2_Name] <> N'Foo' OR [l0].[Level2_Name] IS NULL)
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [t2].[Id], [t2].[OneToOne_Required_PK_Date], [t2].[Level1_Optional_Id], [t2].[Level1_Required_Id], [t2].[Level2_Name], [t2].[OneToMany_Optional_Inverse2Id], [t2].[OneToMany_Required_Inverse2Id], [t2].[OneToOne_Optional_PK_Inverse2Id], [t2].[PeriodEnd], [t2].[PeriodStart], [t2].[Id0]
    FROM (
        SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[PeriodEnd], [l2].[PeriodStart], [l3].[Id] AS [Id0], ROW_NUMBER() OVER(PARTITION BY [l2].[OneToMany_Required_Inverse2Id] ORDER BY [l2].[Level2_Name] DESC) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3] ON [l2].[Id] = [l3].[Id]
        WHERE [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l2].[Level2_Name] <> N'Bar' OR [l2].[Level2_Name] IS NULL)
    ) AS [t2]
    WHERE 1 < [t2].[row]
) AS [t1] ON [l].[Id] = [t1].[OneToMany_Required_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Level2_Name], [t0].[Id], [t0].[Id0], [t1].[OneToMany_Required_Inverse2Id], [t1].[Level2_Name] DESC, [t1].[Id]");
    }

    public override async Task Filtered_include_after_reference_navigation(bool async)
    {
        await base.Filtered_include_after_reference_navigation(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t1].[Id], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Id0], [t1].[Id00]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00]
    FROM (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00], ROW_NUMBER() OVER(PARTITION BY [l2].[OneToMany_Optional_Inverse3Id] ORDER BY [l2].[Level3_Name]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t2] ON [l2].[Id] = [t2].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL AND ([l2].[Level3_Name] <> N'Foo' OR [l2].[Level3_Name] IS NULL)
    ) AS [t0]
    WHERE 1 < [t0].[row] AND [t0].[row] <= 4
) AS [t1] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t1].[OneToMany_Optional_Inverse3Id], [t1].[Level3_Name], [t1].[Id], [t1].[Id0]");
    }

    public override async Task Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(bool async)
    {
        await base.Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t5].[Id], [t5].[OneToOne_Required_PK_Date], [t5].[Level1_Optional_Id], [t5].[Level1_Required_Id], [t5].[Level2_Name], [t5].[OneToMany_Optional_Inverse2Id], [t5].[OneToMany_Required_Inverse2Id], [t5].[OneToOne_Optional_PK_Inverse2Id], [t5].[PeriodEnd], [t5].[PeriodStart], [t5].[Id0], [t5].[Level2_Optional_Id], [t5].[Level2_Required_Id], [t5].[Level3_Name], [t5].[OneToMany_Optional_Inverse3Id], [t5].[OneToMany_Required_Inverse3Id], [t5].[OneToOne_Optional_PK_Inverse3Id], [t5].[PeriodEnd0], [t5].[PeriodStart0], [t5].[Id00], [t5].[Id01], [t5].[Id000], [t5].[Id1], [t5].[Level3_Optional_Id], [t5].[Level3_Required_Id], [t5].[Level4_Name], [t5].[OneToMany_Optional_Inverse4Id], [t5].[OneToMany_Required_Inverse4Id], [t5].[OneToOne_Optional_PK_Inverse4Id], [t5].[PeriodEnd1], [t5].[PeriodStart1], [t5].[Id02], [t5].[Id001], [t5].[Id0000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
OUTER APPLY (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t0].[Id] AS [Id0], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd] AS [PeriodEnd0], [t0].[PeriodStart] AS [PeriodStart0], [t].[Id0] AS [Id00], [t0].[Id0] AS [Id01], [t0].[Id00] AS [Id000], [t2].[Id] AS [Id1], [t2].[Level3_Optional_Id], [t2].[Level3_Required_Id], [t2].[Level4_Name], [t2].[OneToMany_Optional_Inverse4Id], [t2].[OneToMany_Required_Inverse4Id], [t2].[OneToOne_Optional_PK_Inverse4Id], [t2].[PeriodEnd] AS [PeriodEnd1], [t2].[PeriodStart] AS [PeriodStart1], [t2].[Id0] AS [Id02], [t2].[Id00] AS [Id001], [t2].[Id000] AS [Id0000], [t].[c]
    FROM (
        SELECT TOP(1) [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [c]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id] AND ([l0].[Level2_Name] <> N'Foo' OR [l0].[Level2_Name] IS NULL)
        ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END
    ) AS [t]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t1] ON [l2].[Id] = [t1].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t0] ON CASE
        WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t].[Id]
    END = [t0].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN (
        SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00], [t3].[Id00] AS [Id000]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
        INNER JOIN (
            SELECT [l6].[Id], [t4].[Id] AS [Id0], [t4].[Id0] AS [Id00]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
            INNER JOIN (
                SELECT [l7].[Id], [l8].[Id] AS [Id0]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
                INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8] ON [l7].[Id] = [l8].[Id]
                WHERE [l7].[OneToOne_Required_PK_Date] IS NOT NULL AND [l7].[Level1_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse2Id] IS NOT NULL
            ) AS [t4] ON [l6].[Id] = [t4].[Id]
            WHERE [l6].[Level2_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse3Id] IS NOT NULL
        ) AS [t3] ON [l5].[Id] = [t3].[Id]
        WHERE [l5].[Level3_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse4Id] IS NOT NULL AND [l5].[Id] > 1
    ) AS [t2] ON CASE
        WHEN [t0].[Level2_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse3Id] IS NOT NULL AND [t0].[PeriodEnd] IS NOT NULL AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
    END = [t2].[OneToMany_Optional_Inverse4Id]
) AS [t5]
ORDER BY [l].[Id], [t5].[c], [t5].[Id], [t5].[Id00], [t5].[Id0], [t5].[Id01], [t5].[Id000], [t5].[Id1], [t5].[Id02], [t5].[Id001]");
    }

    public override async Task Filtered_include_and_non_filtered_include_on_same_navigation1(bool async)
    {
        await base.Filtered_include_and_non_filtered_include_on_same_navigation1(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t].[c]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END) AS [row], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [c]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l0].[Level2_Name] <> N'Foo' OR [l0].[Level2_Name] IS NULL)
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[c], [t0].[Id]");
    }

    public override async Task Filtered_include_and_non_filtered_include_on_same_navigation2(bool async)
    {
        await base.Filtered_include_and_non_filtered_include_on_same_navigation2(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t].[c]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END) AS [row], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [c]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l0].[Level2_Name] <> N'Foo' OR [l0].[Level2_Name] IS NULL)
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[c], [t0].[Id]");
    }

    public override async Task Filtered_include_basic_OrderBy_Skip(bool async)
    {
        await base.Filtered_include_basic_OrderBy_Skip(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Level2_Name]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t]
    WHERE 1 < [t].[row]
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Level2_Name], [t0].[Id]");
    }

    public override async Task Filtered_include_basic_OrderBy_Skip_Take(bool async)
    {
        await base.Filtered_include_basic_OrderBy_Skip_Take(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Level2_Name]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t]
    WHERE 1 < [t].[row] AND [t].[row] <= 4
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Level2_Name], [t0].[Id]");
    }

    public override async Task Filtered_include_basic_OrderBy_Take(bool async)
    {
        await base.Filtered_include_basic_OrderBy_Take(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Level2_Name]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Level2_Name], [t0].[Id]");
    }

    public override async Task Filtered_include_basic_Where(bool async)
    {
        await base.Filtered_include_basic_Where(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l0].[Id] > 5
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id]");
    }

    public override async Task Filtered_include_complex_three_level_with_middle_having_filter1(bool async)
    {
        await base.Filtered_include_complex_three_level_with_middle_having_filter1(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t8].[Id], [t8].[OneToOne_Required_PK_Date], [t8].[Level1_Optional_Id], [t8].[Level1_Required_Id], [t8].[Level2_Name], [t8].[OneToMany_Optional_Inverse2Id], [t8].[OneToMany_Required_Inverse2Id], [t8].[OneToOne_Optional_PK_Inverse2Id], [t8].[PeriodEnd], [t8].[PeriodStart], [t8].[Id0], [t8].[Id1], [t8].[Level2_Optional_Id], [t8].[Level2_Required_Id], [t8].[Level3_Name], [t8].[OneToMany_Optional_Inverse3Id], [t8].[OneToMany_Required_Inverse3Id], [t8].[OneToOne_Optional_PK_Inverse3Id], [t8].[PeriodEnd0], [t8].[PeriodStart0], [t8].[Id00], [t8].[Id000], [t8].[Id10], [t8].[Level3_Optional_Id], [t8].[Level3_Required_Id], [t8].[Level4_Name], [t8].[OneToMany_Optional_Inverse4Id], [t8].[OneToMany_Required_Inverse4Id], [t8].[OneToOne_Optional_PK_Inverse4Id], [t8].[PeriodEnd00], [t8].[PeriodStart00], [t8].[Id01], [t8].[Id0000], [t8].[Id00000], [t8].[Id2], [t8].[Level3_Optional_Id0], [t8].[Level3_Required_Id0], [t8].[Level4_Name0], [t8].[OneToMany_Optional_Inverse4Id0], [t8].[OneToMany_Required_Inverse4Id0], [t8].[OneToOne_Optional_PK_Inverse4Id0], [t8].[PeriodEnd1], [t8].[PeriodStart1], [t8].[Id02], [t8].[Id001], [t8].[Id0001]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], [t7].[Id] AS [Id1], [t7].[Level2_Optional_Id], [t7].[Level2_Required_Id], [t7].[Level3_Name], [t7].[OneToMany_Optional_Inverse3Id], [t7].[OneToMany_Required_Inverse3Id], [t7].[OneToOne_Optional_PK_Inverse3Id], [t7].[PeriodEnd] AS [PeriodEnd0], [t7].[PeriodStart] AS [PeriodStart0], [t7].[Id0] AS [Id00], [t7].[Id00] AS [Id000], [t7].[Id1] AS [Id10], [t7].[Level3_Optional_Id], [t7].[Level3_Required_Id], [t7].[Level4_Name], [t7].[OneToMany_Optional_Inverse4Id], [t7].[OneToMany_Required_Inverse4Id], [t7].[OneToOne_Optional_PK_Inverse4Id], [t7].[PeriodEnd0] AS [PeriodEnd00], [t7].[PeriodStart0] AS [PeriodStart00], [t7].[Id01], [t7].[Id000] AS [Id0000], [t7].[Id0000] AS [Id00000], [t7].[Id2], [t7].[Level3_Optional_Id0], [t7].[Level3_Required_Id0], [t7].[Level4_Name0], [t7].[OneToMany_Optional_Inverse4Id0], [t7].[OneToMany_Required_Inverse4Id0], [t7].[OneToOne_Optional_PK_Inverse4Id0], [t7].[PeriodEnd1], [t7].[PeriodStart1], [t7].[Id02], [t7].[Id001], [t7].[Id0001], [t7].[c]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    OUTER APPLY (
        SELECT [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00], [t1].[Id] AS [Id1], [t1].[Level3_Optional_Id], [t1].[Level3_Required_Id], [t1].[Level4_Name], [t1].[OneToMany_Optional_Inverse4Id], [t1].[OneToMany_Required_Inverse4Id], [t1].[OneToOne_Optional_PK_Inverse4Id], [t1].[PeriodEnd] AS [PeriodEnd0], [t1].[PeriodStart] AS [PeriodStart0], [t1].[Id0] AS [Id01], [t1].[Id00] AS [Id000], [t1].[Id000] AS [Id0000], [t4].[Id] AS [Id2], [t4].[Level3_Optional_Id] AS [Level3_Optional_Id0], [t4].[Level3_Required_Id] AS [Level3_Required_Id0], [t4].[Level4_Name] AS [Level4_Name0], [t4].[OneToMany_Optional_Inverse4Id] AS [OneToMany_Optional_Inverse4Id0], [t4].[OneToMany_Required_Inverse4Id] AS [OneToMany_Required_Inverse4Id0], [t4].[OneToOne_Optional_PK_Inverse4Id] AS [OneToOne_Optional_PK_Inverse4Id0], [t4].[PeriodEnd] AS [PeriodEnd1], [t4].[PeriodStart] AS [PeriodStart1], [t4].[Id0] AS [Id02], [t4].[Id00] AS [Id001], [t4].[Id000] AS [Id0001], [t0].[c]
        FROM (
            SELECT TOP(1) [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t].[Id] AS [Id0], [t].[Id0] AS [Id00], CASE
                WHEN [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l2].[Id]
            END AS [c]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
            INNER JOIN (
                SELECT [l3].[Id], [l4].[Id] AS [Id0]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
                INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
                WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
            ) AS [t] ON [l2].[Id] = [t].[Id]
            WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL AND CASE
                WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
            END IS NOT NULL AND (CASE
                WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
            END = [l2].[OneToMany_Optional_Inverse3Id] OR (CASE
                WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
            END IS NULL AND [l2].[OneToMany_Optional_Inverse3Id] IS NULL)) AND ([l2].[Level3_Name] <> N'Foo' OR [l2].[Level3_Name] IS NULL)
            ORDER BY CASE
                WHEN [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l2].[Id]
            END
        ) AS [t0]
        LEFT JOIN (
            SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00], [t2].[Id00] AS [Id000]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
            INNER JOIN (
                SELECT [l6].[Id], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
                INNER JOIN (
                    SELECT [l7].[Id], [l8].[Id] AS [Id0]
                    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
                    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8] ON [l7].[Id] = [l8].[Id]
                    WHERE [l7].[OneToOne_Required_PK_Date] IS NOT NULL AND [l7].[Level1_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse2Id] IS NOT NULL
                ) AS [t3] ON [l6].[Id] = [t3].[Id]
                WHERE [l6].[Level2_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse3Id] IS NOT NULL
            ) AS [t2] ON [l5].[Id] = [t2].[Id]
            WHERE [l5].[Level3_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse4Id] IS NOT NULL
        ) AS [t1] ON CASE
            WHEN [t0].[Level2_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [t0].[Id]
        END = [t1].[OneToMany_Optional_Inverse4Id]
        LEFT JOIN (
            SELECT [l9].[Id], [l9].[Level3_Optional_Id], [l9].[Level3_Required_Id], [l9].[Level4_Name], [l9].[OneToMany_Optional_Inverse4Id], [l9].[OneToMany_Required_Inverse4Id], [l9].[OneToOne_Optional_PK_Inverse4Id], [l9].[PeriodEnd], [l9].[PeriodStart], [t5].[Id] AS [Id0], [t5].[Id0] AS [Id00], [t5].[Id00] AS [Id000]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l9]
            INNER JOIN (
                SELECT [l10].[Id], [t6].[Id] AS [Id0], [t6].[Id0] AS [Id00]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l10]
                INNER JOIN (
                    SELECT [l11].[Id], [l12].[Id] AS [Id0]
                    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l11]
                    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l12] ON [l11].[Id] = [l12].[Id]
                    WHERE [l11].[OneToOne_Required_PK_Date] IS NOT NULL AND [l11].[Level1_Required_Id] IS NOT NULL AND [l11].[OneToMany_Required_Inverse2Id] IS NOT NULL
                ) AS [t6] ON [l10].[Id] = [t6].[Id]
                WHERE [l10].[Level2_Required_Id] IS NOT NULL AND [l10].[OneToMany_Required_Inverse3Id] IS NOT NULL
            ) AS [t5] ON [l9].[Id] = [t5].[Id]
            WHERE [l9].[Level3_Required_Id] IS NOT NULL AND [l9].[OneToMany_Required_Inverse4Id] IS NOT NULL
        ) AS [t4] ON CASE
            WHEN [t0].[Level2_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [t0].[Id]
        END = [t4].[OneToMany_Required_Inverse4Id]
    ) AS [t7]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t8] ON [l].[Id] = [t8].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t8].[Id], [t8].[Id0], [t8].[c], [t8].[Id1], [t8].[Id00], [t8].[Id000], [t8].[Id10], [t8].[Id01], [t8].[Id0000], [t8].[Id00000], [t8].[Id2], [t8].[Id02], [t8].[Id001]");
    }

    public override async Task Filtered_include_complex_three_level_with_middle_having_filter2(bool async)
    {
        await base.Filtered_include_complex_three_level_with_middle_having_filter2(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t8].[Id], [t8].[OneToOne_Required_PK_Date], [t8].[Level1_Optional_Id], [t8].[Level1_Required_Id], [t8].[Level2_Name], [t8].[OneToMany_Optional_Inverse2Id], [t8].[OneToMany_Required_Inverse2Id], [t8].[OneToOne_Optional_PK_Inverse2Id], [t8].[PeriodEnd], [t8].[PeriodStart], [t8].[Id0], [t8].[Id1], [t8].[Level2_Optional_Id], [t8].[Level2_Required_Id], [t8].[Level3_Name], [t8].[OneToMany_Optional_Inverse3Id], [t8].[OneToMany_Required_Inverse3Id], [t8].[OneToOne_Optional_PK_Inverse3Id], [t8].[PeriodEnd0], [t8].[PeriodStart0], [t8].[Id00], [t8].[Id000], [t8].[Id10], [t8].[Level3_Optional_Id], [t8].[Level3_Required_Id], [t8].[Level4_Name], [t8].[OneToMany_Optional_Inverse4Id], [t8].[OneToMany_Required_Inverse4Id], [t8].[OneToOne_Optional_PK_Inverse4Id], [t8].[PeriodEnd00], [t8].[PeriodStart00], [t8].[Id01], [t8].[Id0000], [t8].[Id00000], [t8].[Id2], [t8].[Level3_Optional_Id0], [t8].[Level3_Required_Id0], [t8].[Level4_Name0], [t8].[OneToMany_Optional_Inverse4Id0], [t8].[OneToMany_Required_Inverse4Id0], [t8].[OneToOne_Optional_PK_Inverse4Id0], [t8].[PeriodEnd1], [t8].[PeriodStart1], [t8].[Id02], [t8].[Id001], [t8].[Id0001]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], [t7].[Id] AS [Id1], [t7].[Level2_Optional_Id], [t7].[Level2_Required_Id], [t7].[Level3_Name], [t7].[OneToMany_Optional_Inverse3Id], [t7].[OneToMany_Required_Inverse3Id], [t7].[OneToOne_Optional_PK_Inverse3Id], [t7].[PeriodEnd] AS [PeriodEnd0], [t7].[PeriodStart] AS [PeriodStart0], [t7].[Id0] AS [Id00], [t7].[Id00] AS [Id000], [t7].[Id1] AS [Id10], [t7].[Level3_Optional_Id], [t7].[Level3_Required_Id], [t7].[Level4_Name], [t7].[OneToMany_Optional_Inverse4Id], [t7].[OneToMany_Required_Inverse4Id], [t7].[OneToOne_Optional_PK_Inverse4Id], [t7].[PeriodEnd0] AS [PeriodEnd00], [t7].[PeriodStart0] AS [PeriodStart00], [t7].[Id01], [t7].[Id000] AS [Id0000], [t7].[Id0000] AS [Id00000], [t7].[Id2], [t7].[Level3_Optional_Id0], [t7].[Level3_Required_Id0], [t7].[Level4_Name0], [t7].[OneToMany_Optional_Inverse4Id0], [t7].[OneToMany_Required_Inverse4Id0], [t7].[OneToOne_Optional_PK_Inverse4Id0], [t7].[PeriodEnd1], [t7].[PeriodStart1], [t7].[Id02], [t7].[Id001], [t7].[Id0001], [t7].[c]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    OUTER APPLY (
        SELECT [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00], [t1].[Id] AS [Id1], [t1].[Level3_Optional_Id], [t1].[Level3_Required_Id], [t1].[Level4_Name], [t1].[OneToMany_Optional_Inverse4Id], [t1].[OneToMany_Required_Inverse4Id], [t1].[OneToOne_Optional_PK_Inverse4Id], [t1].[PeriodEnd] AS [PeriodEnd0], [t1].[PeriodStart] AS [PeriodStart0], [t1].[Id0] AS [Id01], [t1].[Id00] AS [Id000], [t1].[Id000] AS [Id0000], [t4].[Id] AS [Id2], [t4].[Level3_Optional_Id] AS [Level3_Optional_Id0], [t4].[Level3_Required_Id] AS [Level3_Required_Id0], [t4].[Level4_Name] AS [Level4_Name0], [t4].[OneToMany_Optional_Inverse4Id] AS [OneToMany_Optional_Inverse4Id0], [t4].[OneToMany_Required_Inverse4Id] AS [OneToMany_Required_Inverse4Id0], [t4].[OneToOne_Optional_PK_Inverse4Id] AS [OneToOne_Optional_PK_Inverse4Id0], [t4].[PeriodEnd] AS [PeriodEnd1], [t4].[PeriodStart] AS [PeriodStart1], [t4].[Id0] AS [Id02], [t4].[Id00] AS [Id001], [t4].[Id000] AS [Id0001], [t0].[c]
        FROM (
            SELECT TOP(1) [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t].[Id] AS [Id0], [t].[Id0] AS [Id00], CASE
                WHEN [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l2].[Id]
            END AS [c]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
            INNER JOIN (
                SELECT [l3].[Id], [l4].[Id] AS [Id0]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
                INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
                WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
            ) AS [t] ON [l2].[Id] = [t].[Id]
            WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL AND CASE
                WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
            END IS NOT NULL AND (CASE
                WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
            END = [l2].[OneToMany_Optional_Inverse3Id] OR (CASE
                WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
            END IS NULL AND [l2].[OneToMany_Optional_Inverse3Id] IS NULL)) AND ([l2].[Level3_Name] <> N'Foo' OR [l2].[Level3_Name] IS NULL)
            ORDER BY CASE
                WHEN [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l2].[Id]
            END
        ) AS [t0]
        LEFT JOIN (
            SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00], [t2].[Id00] AS [Id000]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
            INNER JOIN (
                SELECT [l6].[Id], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
                INNER JOIN (
                    SELECT [l7].[Id], [l8].[Id] AS [Id0]
                    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
                    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8] ON [l7].[Id] = [l8].[Id]
                    WHERE [l7].[OneToOne_Required_PK_Date] IS NOT NULL AND [l7].[Level1_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse2Id] IS NOT NULL
                ) AS [t3] ON [l6].[Id] = [t3].[Id]
                WHERE [l6].[Level2_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse3Id] IS NOT NULL
            ) AS [t2] ON [l5].[Id] = [t2].[Id]
            WHERE [l5].[Level3_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse4Id] IS NOT NULL
        ) AS [t1] ON CASE
            WHEN [t0].[Level2_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [t0].[Id]
        END = [t1].[OneToMany_Optional_Inverse4Id]
        LEFT JOIN (
            SELECT [l9].[Id], [l9].[Level3_Optional_Id], [l9].[Level3_Required_Id], [l9].[Level4_Name], [l9].[OneToMany_Optional_Inverse4Id], [l9].[OneToMany_Required_Inverse4Id], [l9].[OneToOne_Optional_PK_Inverse4Id], [l9].[PeriodEnd], [l9].[PeriodStart], [t5].[Id] AS [Id0], [t5].[Id0] AS [Id00], [t5].[Id00] AS [Id000]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l9]
            INNER JOIN (
                SELECT [l10].[Id], [t6].[Id] AS [Id0], [t6].[Id0] AS [Id00]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l10]
                INNER JOIN (
                    SELECT [l11].[Id], [l12].[Id] AS [Id0]
                    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l11]
                    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l12] ON [l11].[Id] = [l12].[Id]
                    WHERE [l11].[OneToOne_Required_PK_Date] IS NOT NULL AND [l11].[Level1_Required_Id] IS NOT NULL AND [l11].[OneToMany_Required_Inverse2Id] IS NOT NULL
                ) AS [t6] ON [l10].[Id] = [t6].[Id]
                WHERE [l10].[Level2_Required_Id] IS NOT NULL AND [l10].[OneToMany_Required_Inverse3Id] IS NOT NULL
            ) AS [t5] ON [l9].[Id] = [t5].[Id]
            WHERE [l9].[Level3_Required_Id] IS NOT NULL AND [l9].[OneToMany_Required_Inverse4Id] IS NOT NULL
        ) AS [t4] ON CASE
            WHEN [t0].[Level2_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [t0].[Id]
        END = [t4].[OneToMany_Required_Inverse4Id]
    ) AS [t7]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t8] ON [l].[Id] = [t8].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t8].[Id], [t8].[Id0], [t8].[c], [t8].[Id1], [t8].[Id00], [t8].[Id000], [t8].[Id10], [t8].[Id01], [t8].[Id0000], [t8].[Id00000], [t8].[Id2], [t8].[Id02], [t8].[Id001]");
    }

    public override async Task Filtered_include_context_accessed_inside_filter(bool async)
    {
        await base.Filtered_include_context_accessed_inside_filter(async);

        AssertSql(
            @"SELECT COUNT(*)
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]",
            //
            @"@__p_0='True'

SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t].[c]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END) AS [row], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [c]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND @__p_0 = CAST(1 AS bit)
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[c], [t0].[Id]");
    }

    public override async Task Filtered_include_context_accessed_inside_filter_correlated(bool async)
    {
        await base.Filtered_include_context_accessed_inside_filter_correlated(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t].[c]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END) AS [row], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [c]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND (
            SELECT COUNT(*)
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
            WHERE [l2].[Id] <> [l0].[Id]) > 1
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[c], [t0].[Id]");
    }

    // TODO: remove later!!!!!
    public override Task Filtered_include_is_considered_loaded(bool async)
    {
        return base.Filtered_include_is_considered_loaded(async);
    }

    public override Task Filtered_include_calling_methods_directly_on_parameter_throws(bool async)
    {
        return base.Filtered_include_calling_methods_directly_on_parameter_throws(async);
    }

    public override Task Filtered_include_different_filter_set_on_same_navigation_twice(bool async)
    {
        return base.Filtered_include_different_filter_set_on_same_navigation_twice(async);
    }

    public override Task Filtered_include_different_filter_set_on_same_navigation_twice_multi_level(bool async)
    {
        return base.Filtered_include_different_filter_set_on_same_navigation_twice_multi_level(async);
    }

    public override async Task Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only(bool async)
    {
        await base.Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t4].[Id], [t4].[OneToOne_Required_PK_Date], [t4].[Level1_Optional_Id], [t4].[Level1_Required_Id], [t4].[Level2_Name], [t4].[OneToMany_Optional_Inverse2Id], [t4].[OneToMany_Required_Inverse2Id], [t4].[OneToOne_Optional_PK_Inverse2Id], [t4].[PeriodEnd], [t4].[PeriodStart], [t4].[Id0], [t4].[Id1], [t4].[Id00], [t4].[Id000], [t4].[Id2], [t4].[Level2_Optional_Id], [t4].[Level2_Required_Id], [t4].[Level3_Name], [t4].[OneToMany_Optional_Inverse3Id], [t4].[OneToMany_Required_Inverse3Id], [t4].[OneToOne_Optional_PK_Inverse3Id], [t4].[PeriodEnd0], [t4].[PeriodStart0], [t4].[Id01], [t4].[Id001], [t4].[Level2_Optional_Id0], [t4].[Level2_Required_Id0], [t4].[Level3_Name0], [t4].[OneToMany_Optional_Inverse3Id0], [t4].[OneToMany_Required_Inverse3Id0], [t4].[OneToOne_Optional_PK_Inverse3Id0], [t4].[PeriodEnd1], [t4].[PeriodStart1]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
OUTER APPLY (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t0].[Id] AS [Id1], [t0].[Id0] AS [Id00], [t0].[Id00] AS [Id000], [t2].[Id] AS [Id2], [t2].[Level2_Optional_Id], [t2].[Level2_Required_Id], [t2].[Level3_Name], [t2].[OneToMany_Optional_Inverse3Id], [t2].[OneToMany_Required_Inverse3Id], [t2].[OneToOne_Optional_PK_Inverse3Id], [t2].[PeriodEnd] AS [PeriodEnd0], [t2].[PeriodStart] AS [PeriodStart0], [t2].[Id0] AS [Id01], [t2].[Id00] AS [Id001], [t0].[Level2_Optional_Id] AS [Level2_Optional_Id0], [t0].[Level2_Required_Id] AS [Level2_Required_Id0], [t0].[Level3_Name] AS [Level3_Name0], [t0].[OneToMany_Optional_Inverse3Id] AS [OneToMany_Optional_Inverse3Id0], [t0].[OneToMany_Required_Inverse3Id] AS [OneToMany_Required_Inverse3Id0], [t0].[OneToOne_Optional_PK_Inverse3Id] AS [OneToOne_Optional_PK_Inverse3Id0], [t0].[PeriodEnd] AS [PeriodEnd1], [t0].[PeriodStart] AS [PeriodStart1], [t].[c]
    FROM (
        SELECT TOP(2) [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [c]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id] AND ([l0].[Level2_Name] <> N'Foo' OR [l0].[Level2_Name] IS NULL)
        ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END
    ) AS [t]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t1] ON [l2].[Id] = [t1].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t0] ON CASE
        WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t].[Id]
    END = [t0].[Level2_Required_Id]
    LEFT JOIN (
        SELECT [l5].[Id], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Level3_Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
        INNER JOIN (
            SELECT [l6].[Id], [l7].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7] ON [l6].[Id] = [l7].[Id]
            WHERE [l6].[OneToOne_Required_PK_Date] IS NOT NULL AND [l6].[Level1_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t3] ON [l5].[Id] = [t3].[Id]
        WHERE [l5].[Level2_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t2] ON CASE
        WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t].[Id]
    END = [t2].[OneToMany_Optional_Inverse3Id]
) AS [t4]
ORDER BY [l].[Id], [t4].[c], [t4].[Id], [t4].[Id0], [t4].[Id1], [t4].[Id00], [t4].[Id000], [t4].[Id2], [t4].[Id01]");
    }

    public override async Task Filtered_include_on_ThenInclude(bool async)
    {
        await base.Filtered_include_on_ThenInclude(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t1].[Id], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Id0], [t1].[Id00]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00]
    FROM (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00], ROW_NUMBER() OVER(PARTITION BY [l2].[OneToMany_Optional_Inverse3Id] ORDER BY [l2].[Level3_Name]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t2] ON [l2].[Id] = [t2].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL AND ([l2].[Level3_Name] <> N'Foo' OR [l2].[Level3_Name] IS NULL)
    ) AS [t0]
    WHERE 1 < [t0].[row] AND [t0].[row] <= 4
) AS [t1] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t1].[OneToMany_Optional_Inverse3Id], [t1].[Level3_Name], [t1].[Id], [t1].[Id0]");
    }

    public override async Task Filtered_include_Skip_without_OrderBy(bool async)
    {
        await base.Filtered_include_Skip_without_OrderBy(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id], [l1].[Id]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t]
    WHERE 1 < [t].[row]
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[Id]");
    }

    public override async Task Filtered_include_Take_without_OrderBy(bool async)
    {
        await base.Filtered_include_Take_without_OrderBy(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id], [l1].[Id]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[Id]");
    }

    public override async Task Filtered_include_Take_with_another_Take_on_top_level(bool async)
    {
        await base.Filtered_include_Take_with_another_Take_on_top_level(async);

        AssertSql(
            @"@__p_0='5'

SELECT [t].[Id], [t].[Date], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t2].[Id], [t2].[OneToOne_Required_PK_Date], [t2].[Level1_Optional_Id], [t2].[Level1_Required_Id], [t2].[Level2_Name], [t2].[OneToMany_Optional_Inverse2Id], [t2].[OneToMany_Required_Inverse2Id], [t2].[OneToOne_Optional_PK_Inverse2Id], [t2].[PeriodEnd], [t2].[PeriodStart], [t2].[Id0], [t2].[Level2_Optional_Id], [t2].[Level2_Required_Id], [t2].[Level3_Name], [t2].[OneToMany_Optional_Inverse3Id], [t2].[OneToMany_Required_Inverse3Id], [t2].[OneToOne_Optional_PK_Inverse3Id], [t2].[PeriodEnd0], [t2].[PeriodStart0], [t2].[Id00], [t2].[Id01], [t2].[Id000]
FROM (
    SELECT TOP(@__p_0) [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    ORDER BY [l].[Id]
) AS [t]
OUTER APPLY (
    SELECT [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd] AS [PeriodEnd0], [t1].[PeriodStart] AS [PeriodStart0], [t0].[Id0] AS [Id00], [t1].[Id0] AS [Id01], [t1].[Id00] AS [Id000]
    FROM (
        SELECT TOP(4) [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
        ORDER BY [l0].[Level2_Name] DESC
    ) AS [t0]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t3] ON [l2].[Id] = [t3].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t1] ON CASE
        WHEN [t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t0].[Id]
    END = [t1].[Level2_Optional_Id]
) AS [t2]
ORDER BY [t].[Id], [t2].[Level2_Name] DESC, [t2].[Id], [t2].[Id00], [t2].[Id0], [t2].[Id01]");
    }

    public override async Task Filtered_include_ThenInclude_OrderBy(bool async)
    {
        await base.Filtered_include_ThenInclude_OrderBy(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t1].[Id], [t1].[OneToOne_Required_PK_Date], [t1].[Level1_Optional_Id], [t1].[Level1_Required_Id], [t1].[Level2_Name], [t1].[OneToMany_Optional_Inverse2Id], [t1].[OneToMany_Required_Inverse2Id], [t1].[OneToOne_Optional_PK_Inverse2Id], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Id0], [t1].[Id1], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd0], [t1].[PeriodStart0], [t1].[Id00], [t1].[Id000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], [t0].[Id] AS [Id1], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd] AS [PeriodEnd0], [t0].[PeriodStart] AS [PeriodStart0], [t0].[Id0] AS [Id00], [t0].[Id00] AS [Id000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t].[Id] AS [Id0], [t].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t] ON [l2].[Id] = [t].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t0] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [t0].[OneToMany_Optional_Inverse3Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t1] ON [l].[Id] = [t1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t1].[Level2_Name], [t1].[Id], [t1].[Id0], [t1].[Level3_Name] DESC, [t1].[Id1], [t1].[Id00]");
    }

    public override async Task Filtered_include_variable_used_inside_filter(bool async)
    {
        await base.Filtered_include_variable_used_inside_filter(async);

        AssertSql(
            @"@__prm_0='Foo' (Size = 4000)

SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t].[c]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END) AS [row], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [c]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l0].[Level2_Name] <> @__prm_0 OR [l0].[Level2_Name] IS NULL)
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[c], [t0].[Id]");
    }

    public override async Task Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_FirstOrDefault_on_top_level(bool async)
    {
        await base.Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_FirstOrDefault_on_top_level(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[Date], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t2].[Id], [t2].[OneToOne_Required_PK_Date], [t2].[Level1_Optional_Id], [t2].[Level1_Required_Id], [t2].[Level2_Name], [t2].[OneToMany_Optional_Inverse2Id], [t2].[OneToMany_Required_Inverse2Id], [t2].[OneToOne_Optional_PK_Inverse2Id], [t2].[PeriodEnd], [t2].[PeriodStart], [t2].[Id0], [t2].[Level2_Optional_Id], [t2].[Level2_Required_Id], [t2].[Level3_Name], [t2].[OneToMany_Optional_Inverse3Id], [t2].[OneToMany_Required_Inverse3Id], [t2].[OneToOne_Optional_PK_Inverse3Id], [t2].[PeriodEnd0], [t2].[PeriodStart0], [t2].[Id00], [t2].[Id01], [t2].[Id000]
FROM (
    SELECT TOP(1) [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    ORDER BY [l].[Id]
) AS [t]
OUTER APPLY (
    SELECT [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd] AS [PeriodEnd0], [t1].[PeriodStart] AS [PeriodStart0], [t0].[Id0] AS [Id00], [t1].[Id0] AS [Id01], [t1].[Id00] AS [Id000]
    FROM (
        SELECT TOP(40) [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
    ) AS [t0]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t3] ON [l2].[Id] = [t3].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t1] ON CASE
        WHEN [t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t0].[Id]
    END = [t1].[Level2_Optional_Id]
) AS [t2]
ORDER BY [t].[Id], [t2].[Id], [t2].[Id00], [t2].[Id0], [t2].[Id01]");
    }

    public override async Task Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_unordered_Take_on_top_level(bool async)
    {
        await base.Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_unordered_Take_on_top_level(async);

        AssertSql(
            @"@__p_0='30'

SELECT [t].[Id], [t].[Date], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t2].[Id], [t2].[OneToOne_Required_PK_Date], [t2].[Level1_Optional_Id], [t2].[Level1_Required_Id], [t2].[Level2_Name], [t2].[OneToMany_Optional_Inverse2Id], [t2].[OneToMany_Required_Inverse2Id], [t2].[OneToOne_Optional_PK_Inverse2Id], [t2].[PeriodEnd], [t2].[PeriodStart], [t2].[Id0], [t2].[Level2_Optional_Id], [t2].[Level2_Required_Id], [t2].[Level3_Name], [t2].[OneToMany_Optional_Inverse3Id], [t2].[OneToMany_Required_Inverse3Id], [t2].[OneToOne_Optional_PK_Inverse3Id], [t2].[PeriodEnd0], [t2].[PeriodStart0], [t2].[Id00], [t2].[Id01], [t2].[Id000]
FROM (
    SELECT TOP(@__p_0) [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    ORDER BY [l].[Id]
) AS [t]
OUTER APPLY (
    SELECT [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd] AS [PeriodEnd0], [t1].[PeriodStart] AS [PeriodStart0], [t0].[Id0] AS [Id00], [t1].[Id0] AS [Id01], [t1].[Id00] AS [Id000]
    FROM (
        SELECT TOP(40) [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
    ) AS [t0]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t3] ON [l2].[Id] = [t3].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t1] ON CASE
        WHEN [t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t0].[Id]
    END = [t1].[Level2_Optional_Id]
) AS [t2]
ORDER BY [t].[Id], [t2].[Id], [t2].[Id00], [t2].[Id0], [t2].[Id01]");
    }

    public override async Task Filtered_ThenInclude_OrderBy(bool async)
    {
        await base.Filtered_ThenInclude_OrderBy(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t1].[Id], [t1].[OneToOne_Required_PK_Date], [t1].[Level1_Optional_Id], [t1].[Level1_Required_Id], [t1].[Level2_Name], [t1].[OneToMany_Optional_Inverse2Id], [t1].[OneToMany_Required_Inverse2Id], [t1].[OneToOne_Optional_PK_Inverse2Id], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Id0], [t1].[Id1], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd0], [t1].[PeriodStart0], [t1].[Id00], [t1].[Id000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], [t0].[Id] AS [Id1], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd] AS [PeriodEnd0], [t0].[PeriodStart] AS [PeriodStart0], [t0].[Id0] AS [Id00], [t0].[Id00] AS [Id000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t].[Id] AS [Id0], [t].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t] ON [l2].[Id] = [t].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t0] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [t0].[OneToMany_Optional_Inverse3Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t1] ON [l].[Id] = [t1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t1].[Id], [t1].[Id0], [t1].[Level3_Name], [t1].[Id1], [t1].[Id00]");
    }

    public override async Task FirstOrDefault_with_predicate_on_correlated_collection_in_projection(bool async)
    {
        await base.FirstOrDefault_with_predicate_on_correlated_collection_in_projection(async);

        AssertSql(
            @"SELECT [l].[Id], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END ORDER BY [l0].[Id], [l1].[Id]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id] AND [l].[Id] = CASE
    WHEN [t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t0].[Id]
END");
    }

    public override async Task Filtered_include_OrderBy(bool async)
    {
        await base.Filtered_include_OrderBy(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Level2_Name], [t].[Id]");
    }

    public override async Task Filtered_include_same_filter_set_on_same_navigation_twice(bool async)
    {
        await base.Filtered_include_same_filter_set_on_same_navigation_twice(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t].[c]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END DESC) AS [row], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [c]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l0].[Level2_Name] <> N'Foo' OR [l0].[Level2_Name] IS NULL)
    ) AS [t]
    WHERE [t].[row] <= 2
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[c] DESC, [t0].[Id]");
    }

    public override async Task Filtered_include_same_filter_set_on_same_navigation_twice_followed_by_ThenIncludes(bool async)
    {
        await base.Filtered_include_same_filter_set_on_same_navigation_twice_followed_by_ThenIncludes(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t4].[Id], [t4].[OneToOne_Required_PK_Date], [t4].[Level1_Optional_Id], [t4].[Level1_Required_Id], [t4].[Level2_Name], [t4].[OneToMany_Optional_Inverse2Id], [t4].[OneToMany_Required_Inverse2Id], [t4].[OneToOne_Optional_PK_Inverse2Id], [t4].[PeriodEnd], [t4].[PeriodStart], [t4].[Id0], [t4].[Id1], [t4].[Id00], [t4].[Id000], [t4].[Id2], [t4].[Level2_Optional_Id], [t4].[Level2_Required_Id], [t4].[Level3_Name], [t4].[OneToMany_Optional_Inverse3Id], [t4].[OneToMany_Required_Inverse3Id], [t4].[OneToOne_Optional_PK_Inverse3Id], [t4].[PeriodEnd0], [t4].[PeriodStart0], [t4].[Id01], [t4].[Id001], [t4].[Level2_Optional_Id0], [t4].[Level2_Required_Id0], [t4].[Level3_Name0], [t4].[OneToMany_Optional_Inverse3Id0], [t4].[OneToMany_Required_Inverse3Id0], [t4].[OneToOne_Optional_PK_Inverse3Id0], [t4].[PeriodEnd1], [t4].[PeriodStart1]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
OUTER APPLY (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t0].[Id] AS [Id1], [t0].[Id0] AS [Id00], [t0].[Id00] AS [Id000], [t2].[Id] AS [Id2], [t2].[Level2_Optional_Id], [t2].[Level2_Required_Id], [t2].[Level3_Name], [t2].[OneToMany_Optional_Inverse3Id], [t2].[OneToMany_Required_Inverse3Id], [t2].[OneToOne_Optional_PK_Inverse3Id], [t2].[PeriodEnd] AS [PeriodEnd0], [t2].[PeriodStart] AS [PeriodStart0], [t2].[Id0] AS [Id01], [t2].[Id00] AS [Id001], [t0].[Level2_Optional_Id] AS [Level2_Optional_Id0], [t0].[Level2_Required_Id] AS [Level2_Required_Id0], [t0].[Level3_Name] AS [Level3_Name0], [t0].[OneToMany_Optional_Inverse3Id] AS [OneToMany_Optional_Inverse3Id0], [t0].[OneToMany_Required_Inverse3Id] AS [OneToMany_Required_Inverse3Id0], [t0].[OneToOne_Optional_PK_Inverse3Id] AS [OneToOne_Optional_PK_Inverse3Id0], [t0].[PeriodEnd] AS [PeriodEnd1], [t0].[PeriodStart] AS [PeriodStart1], [t].[c]
    FROM (
        SELECT TOP(2) [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [c]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id] AND ([l0].[Level2_Name] <> N'Foo' OR [l0].[Level2_Name] IS NULL)
        ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END
    ) AS [t]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t1] ON [l2].[Id] = [t1].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t0] ON CASE
        WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t].[Id]
    END = [t0].[Level2_Required_Id]
    LEFT JOIN (
        SELECT [l5].[Id], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Level3_Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
        INNER JOIN (
            SELECT [l6].[Id], [l7].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7] ON [l6].[Id] = [l7].[Id]
            WHERE [l6].[OneToOne_Required_PK_Date] IS NOT NULL AND [l6].[Level1_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t3] ON [l5].[Id] = [t3].[Id]
        WHERE [l5].[Level2_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t2] ON CASE
        WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t].[Id]
    END = [t2].[OneToMany_Optional_Inverse3Id]
) AS [t4]
ORDER BY [l].[Id], [t4].[c], [t4].[Id], [t4].[Id0], [t4].[Id1], [t4].[Id00], [t4].[Id000], [t4].[Id2], [t4].[Id01]");
    }

    public override async Task Filtered_include_Skip_Take_with_another_Skip_Take_on_top_level(bool async)
    {
        await base.Filtered_include_Skip_Take_with_another_Skip_Take_on_top_level(async);

        AssertSql(
            @"@__p_0='10'
@__p_1='5'

SELECT [t].[Id], [t].[Date], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t2].[Id], [t2].[OneToOne_Required_PK_Date], [t2].[Level1_Optional_Id], [t2].[Level1_Required_Id], [t2].[Level2_Name], [t2].[OneToMany_Optional_Inverse2Id], [t2].[OneToMany_Required_Inverse2Id], [t2].[OneToOne_Optional_PK_Inverse2Id], [t2].[PeriodEnd], [t2].[PeriodStart], [t2].[Id0], [t2].[Level2_Optional_Id], [t2].[Level2_Required_Id], [t2].[Level3_Name], [t2].[OneToMany_Optional_Inverse3Id], [t2].[OneToMany_Required_Inverse3Id], [t2].[OneToOne_Optional_PK_Inverse3Id], [t2].[PeriodEnd0], [t2].[PeriodStart0], [t2].[Id00], [t2].[Id01], [t2].[Id000]
FROM (
    SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    ORDER BY [l].[Id] DESC
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
OUTER APPLY (
    SELECT [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd] AS [PeriodEnd0], [t1].[PeriodStart] AS [PeriodStart0], [t0].[Id0] AS [Id00], [t1].[Id0] AS [Id01], [t1].[Id00] AS [Id000]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
        ORDER BY [l0].[Level2_Name] DESC
        OFFSET 2 ROWS FETCH NEXT 4 ROWS ONLY
    ) AS [t0]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t3] ON [l2].[Id] = [t3].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t1] ON CASE
        WHEN [t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t0].[Id]
    END = [t1].[Level2_Optional_Id]
) AS [t2]
ORDER BY [t].[Id] DESC, [t2].[Level2_Name] DESC, [t2].[Id], [t2].[Id00], [t2].[Id0], [t2].[Id01]");
    }

    public override async Task Include_after_SelectMany(bool async)
    {
        await base.Include_after_SelectMany(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [l].[Id], [t].[Id0], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToMany_Required_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0]");
    }

    public override async Task Include_and_ThenInclude_collections_followed_by_projecting_the_first_collection(bool async)
    {
        await base.Include_and_ThenInclude_collections_followed_by_projecting_the_first_collection(async);

        AssertSql(
            @"SELECT [l].[Id], [t1].[Id], [t1].[OneToOne_Required_PK_Date], [t1].[Level1_Optional_Id], [t1].[Level1_Required_Id], [t1].[Level2_Name], [t1].[OneToMany_Optional_Inverse2Id], [t1].[OneToMany_Required_Inverse2Id], [t1].[OneToOne_Optional_PK_Inverse2Id], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Id0], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd0], [t1].[PeriodStart0], [t1].[Id1], [t1].[Id00], [t1].[Id000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [t0].[Id] AS [Id0], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd] AS [PeriodEnd0], [t0].[PeriodStart] AS [PeriodStart0], [l1].[Id] AS [Id1], [t0].[Id0] AS [Id00], [t0].[Id00] AS [Id000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t].[Id] AS [Id0], [t].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t] ON [l2].[Id] = [t].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t0] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [t0].[OneToOne_Optional_PK_Inverse3Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t1] ON [l].[Id] = [t1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t1].[Id], [t1].[Id1], [t1].[Id0], [t1].[Id00]");
    }

    public override async Task Include_collection(bool async)
    {
        await base.Include_collection(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id]");
    }

    public override async Task Include_collection_and_another_navigation_chain_followed_by_projecting_the_first_collection(bool async)
    {
        await base.Include_collection_and_another_navigation_chain_followed_by_projecting_the_first_collection(async);

        AssertSql(
            @"SELECT [l].[Id], [t4].[Id], [t4].[OneToOne_Required_PK_Date], [t4].[Level1_Optional_Id], [t4].[Level1_Required_Id], [t4].[Level2_Name], [t4].[OneToMany_Optional_Inverse2Id], [t4].[OneToMany_Required_Inverse2Id], [t4].[OneToOne_Optional_PK_Inverse2Id], [t4].[PeriodEnd], [t4].[PeriodStart], [t4].[Id0], [t4].[Level2_Optional_Id], [t4].[Level2_Required_Id], [t4].[Level3_Name], [t4].[OneToMany_Optional_Inverse3Id], [t4].[OneToMany_Required_Inverse3Id], [t4].[OneToOne_Optional_PK_Inverse3Id], [t4].[PeriodEnd0], [t4].[PeriodStart0], [t4].[Id1], [t4].[Level3_Optional_Id], [t4].[Level3_Required_Id], [t4].[Level4_Name], [t4].[OneToMany_Optional_Inverse4Id], [t4].[OneToMany_Required_Inverse4Id], [t4].[OneToOne_Optional_PK_Inverse4Id], [t4].[PeriodEnd1], [t4].[PeriodStart1], [t4].[Id2], [t4].[Id00], [t4].[Id000], [t4].[Id01], [t4].[Id001], [t4].[Id0000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [t0].[Id] AS [Id0], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd] AS [PeriodEnd0], [t0].[PeriodStart] AS [PeriodStart0], [t1].[Id] AS [Id1], [t1].[Level3_Optional_Id], [t1].[Level3_Required_Id], [t1].[Level4_Name], [t1].[OneToMany_Optional_Inverse4Id], [t1].[OneToMany_Required_Inverse4Id], [t1].[OneToOne_Optional_PK_Inverse4Id], [t1].[PeriodEnd] AS [PeriodEnd1], [t1].[PeriodStart] AS [PeriodStart1], [l1].[Id] AS [Id2], [t0].[Id0] AS [Id00], [t0].[Id00] AS [Id000], [t1].[Id0] AS [Id01], [t1].[Id00] AS [Id001], [t1].[Id000] AS [Id0000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t].[Id] AS [Id0], [t].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t] ON [l2].[Id] = [t].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t0] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [t0].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN (
        SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00], [t2].[Id00] AS [Id000]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
        INNER JOIN (
            SELECT [l6].[Id], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
            INNER JOIN (
                SELECT [l7].[Id], [l8].[Id] AS [Id0]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
                INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8] ON [l7].[Id] = [l8].[Id]
                WHERE [l7].[OneToOne_Required_PK_Date] IS NOT NULL AND [l7].[Level1_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse2Id] IS NOT NULL
            ) AS [t3] ON [l6].[Id] = [t3].[Id]
            WHERE [l6].[Level2_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse3Id] IS NOT NULL
        ) AS [t2] ON [l5].[Id] = [t2].[Id]
        WHERE [l5].[Level3_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [t1] ON CASE
        WHEN [t0].[Level2_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse3Id] IS NOT NULL AND [t0].[PeriodEnd] IS NOT NULL AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
    END = [t1].[Level3_Optional_Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t4] ON [l].[Id] = [t4].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t4].[Id], [t4].[Id2], [t4].[Id0], [t4].[Id00], [t4].[Id000], [t4].[Id1], [t4].[Id01], [t4].[Id001]");
    }

    public override async Task Include_collection_followed_by_complex_includes_and_projecting_the_included_collection(bool async)
    {
        await base.Include_collection_followed_by_complex_includes_and_projecting_the_included_collection(async);

        AssertSql(
            @"SELECT [l].[Id], [t9].[Id], [t9].[OneToOne_Required_PK_Date], [t9].[Level1_Optional_Id], [t9].[Level1_Required_Id], [t9].[Level2_Name], [t9].[OneToMany_Optional_Inverse2Id], [t9].[OneToMany_Required_Inverse2Id], [t9].[OneToOne_Optional_PK_Inverse2Id], [t9].[PeriodEnd], [t9].[PeriodStart], [t9].[Id0], [t9].[Level2_Optional_Id], [t9].[Level2_Required_Id], [t9].[Level3_Name], [t9].[OneToMany_Optional_Inverse3Id], [t9].[OneToMany_Required_Inverse3Id], [t9].[OneToOne_Optional_PK_Inverse3Id], [t9].[PeriodEnd0], [t9].[PeriodStart0], [t9].[Id1], [t9].[Level3_Optional_Id], [t9].[Level3_Required_Id], [t9].[Level4_Name], [t9].[OneToMany_Optional_Inverse4Id], [t9].[OneToMany_Required_Inverse4Id], [t9].[OneToOne_Optional_PK_Inverse4Id], [t9].[PeriodEnd1], [t9].[PeriodStart1], [t9].[Id2], [t9].[Level2_Optional_Id0], [t9].[Level2_Required_Id0], [t9].[Level3_Name0], [t9].[OneToMany_Optional_Inverse3Id0], [t9].[OneToMany_Required_Inverse3Id0], [t9].[OneToOne_Optional_PK_Inverse3Id0], [t9].[PeriodEnd2], [t9].[PeriodStart2], [t9].[Id3], [t9].[Id00], [t9].[Id000], [t9].[Id01], [t9].[Id001], [t9].[Id0000], [t9].[Id02], [t9].[Id002], [t9].[Id4], [t9].[Level3_Optional_Id0], [t9].[Level3_Required_Id0], [t9].[Level4_Name0], [t9].[OneToMany_Optional_Inverse4Id0], [t9].[OneToMany_Required_Inverse4Id0], [t9].[OneToOne_Optional_PK_Inverse4Id0], [t9].[PeriodEnd3], [t9].[PeriodStart3], [t9].[Id03], [t9].[Id003], [t9].[Id0001]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [t0].[Id] AS [Id0], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd] AS [PeriodEnd0], [t0].[PeriodStart] AS [PeriodStart0], [t1].[Id] AS [Id1], [t1].[Level3_Optional_Id], [t1].[Level3_Required_Id], [t1].[Level4_Name], [t1].[OneToMany_Optional_Inverse4Id], [t1].[OneToMany_Required_Inverse4Id], [t1].[OneToOne_Optional_PK_Inverse4Id], [t1].[PeriodEnd] AS [PeriodEnd1], [t1].[PeriodStart] AS [PeriodStart1], [t4].[Id] AS [Id2], [t4].[Level2_Optional_Id] AS [Level2_Optional_Id0], [t4].[Level2_Required_Id] AS [Level2_Required_Id0], [t4].[Level3_Name] AS [Level3_Name0], [t4].[OneToMany_Optional_Inverse3Id] AS [OneToMany_Optional_Inverse3Id0], [t4].[OneToMany_Required_Inverse3Id] AS [OneToMany_Required_Inverse3Id0], [t4].[OneToOne_Optional_PK_Inverse3Id] AS [OneToOne_Optional_PK_Inverse3Id0], [t4].[PeriodEnd] AS [PeriodEnd2], [t4].[PeriodStart] AS [PeriodStart2], [l1].[Id] AS [Id3], [t0].[Id0] AS [Id00], [t0].[Id00] AS [Id000], [t1].[Id0] AS [Id01], [t1].[Id00] AS [Id001], [t1].[Id000] AS [Id0000], [t4].[Id0] AS [Id02], [t4].[Id00] AS [Id002], [t6].[Id] AS [Id4], [t6].[Level3_Optional_Id] AS [Level3_Optional_Id0], [t6].[Level3_Required_Id] AS [Level3_Required_Id0], [t6].[Level4_Name] AS [Level4_Name0], [t6].[OneToMany_Optional_Inverse4Id] AS [OneToMany_Optional_Inverse4Id0], [t6].[OneToMany_Required_Inverse4Id] AS [OneToMany_Required_Inverse4Id0], [t6].[OneToOne_Optional_PK_Inverse4Id] AS [OneToOne_Optional_PK_Inverse4Id0], [t6].[PeriodEnd] AS [PeriodEnd3], [t6].[PeriodStart] AS [PeriodStart3], [t6].[Id0] AS [Id03], [t6].[Id00] AS [Id003], [t6].[Id000] AS [Id0001]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t].[Id] AS [Id0], [t].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t] ON [l2].[Id] = [t].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t0] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [t0].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN (
        SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00], [t2].[Id00] AS [Id000]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
        INNER JOIN (
            SELECT [l6].[Id], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
            INNER JOIN (
                SELECT [l7].[Id], [l8].[Id] AS [Id0]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
                INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8] ON [l7].[Id] = [l8].[Id]
                WHERE [l7].[OneToOne_Required_PK_Date] IS NOT NULL AND [l7].[Level1_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse2Id] IS NOT NULL
            ) AS [t3] ON [l6].[Id] = [t3].[Id]
            WHERE [l6].[Level2_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse3Id] IS NOT NULL
        ) AS [t2] ON [l5].[Id] = [t2].[Id]
        WHERE [l5].[Level3_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [t1] ON CASE
        WHEN [t0].[Level2_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse3Id] IS NOT NULL AND [t0].[PeriodEnd] IS NOT NULL AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
    END = [t1].[Level3_Optional_Id]
    LEFT JOIN (
        SELECT [l9].[Id], [l9].[Level2_Optional_Id], [l9].[Level2_Required_Id], [l9].[Level3_Name], [l9].[OneToMany_Optional_Inverse3Id], [l9].[OneToMany_Required_Inverse3Id], [l9].[OneToOne_Optional_PK_Inverse3Id], [l9].[PeriodEnd], [l9].[PeriodStart], [t5].[Id] AS [Id0], [t5].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l9]
        INNER JOIN (
            SELECT [l10].[Id], [l11].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l10]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l11] ON [l10].[Id] = [l11].[Id]
            WHERE [l10].[OneToOne_Required_PK_Date] IS NOT NULL AND [l10].[Level1_Required_Id] IS NOT NULL AND [l10].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t5] ON [l9].[Id] = [t5].[Id]
        WHERE [l9].[Level2_Required_Id] IS NOT NULL AND [l9].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t4] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [t4].[Level2_Optional_Id]
    LEFT JOIN (
        SELECT [l12].[Id], [l12].[Level3_Optional_Id], [l12].[Level3_Required_Id], [l12].[Level4_Name], [l12].[OneToMany_Optional_Inverse4Id], [l12].[OneToMany_Required_Inverse4Id], [l12].[OneToOne_Optional_PK_Inverse4Id], [l12].[PeriodEnd], [l12].[PeriodStart], [t7].[Id] AS [Id0], [t7].[Id0] AS [Id00], [t7].[Id00] AS [Id000]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l12]
        INNER JOIN (
            SELECT [l13].[Id], [t8].[Id] AS [Id0], [t8].[Id0] AS [Id00]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l13]
            INNER JOIN (
                SELECT [l14].[Id], [l15].[Id] AS [Id0]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l14]
                INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l15] ON [l14].[Id] = [l15].[Id]
                WHERE [l14].[OneToOne_Required_PK_Date] IS NOT NULL AND [l14].[Level1_Required_Id] IS NOT NULL AND [l14].[OneToMany_Required_Inverse2Id] IS NOT NULL
            ) AS [t8] ON [l13].[Id] = [t8].[Id]
            WHERE [l13].[Level2_Required_Id] IS NOT NULL AND [l13].[OneToMany_Required_Inverse3Id] IS NOT NULL
        ) AS [t7] ON [l12].[Id] = [t7].[Id]
        WHERE [l12].[Level3_Required_Id] IS NOT NULL AND [l12].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [t6] ON CASE
        WHEN [t4].[Level2_Required_Id] IS NOT NULL AND [t4].[OneToMany_Required_Inverse3Id] IS NOT NULL AND [t4].[PeriodEnd] IS NOT NULL AND [t4].[PeriodStart] IS NOT NULL THEN [t4].[Id]
    END = [t6].[OneToMany_Optional_Inverse4Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t9] ON [l].[Id] = [t9].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t9].[Id], [t9].[Id3], [t9].[Id0], [t9].[Id00], [t9].[Id000], [t9].[Id1], [t9].[Id01], [t9].[Id001], [t9].[Id0000], [t9].[Id2], [t9].[Id02], [t9].[Id002], [t9].[Id4], [t9].[Id03], [t9].[Id003]");
    }

    public override async Task Include_collection_followed_by_include_reference(bool async)
    {
        await base.Include_collection_followed_by_include_reference(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t1].[Id], [t1].[OneToOne_Required_PK_Date], [t1].[Level1_Optional_Id], [t1].[Level1_Required_Id], [t1].[Level2_Name], [t1].[OneToMany_Optional_Inverse2Id], [t1].[OneToMany_Required_Inverse2Id], [t1].[OneToOne_Optional_PK_Inverse2Id], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Id0], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd0], [t1].[PeriodStart0], [t1].[Id1], [t1].[Id00], [t1].[Id000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [t0].[Id] AS [Id0], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd] AS [PeriodEnd0], [t0].[PeriodStart] AS [PeriodStart0], [l1].[Id] AS [Id1], [t0].[Id0] AS [Id00], [t0].[Id00] AS [Id000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t].[Id] AS [Id0], [t].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t] ON [l2].[Id] = [t].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t0] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [t0].[OneToOne_Optional_PK_Inverse3Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t1] ON [l].[Id] = [t1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t1].[Id], [t1].[Id1], [t1].[Id0], [t1].[Id00]");
    }

    public override async Task Include_collection_followed_by_projecting_the_included_collection(bool async)
    {
        await base.Include_collection_followed_by_projecting_the_included_collection(async);

        AssertSql(
            @"SELECT [l].[Id], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id]");
    }

    public override async Task Include_collection_multiple(bool async)
    {
        await base.Include_collection_multiple(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t9].[Id], [t9].[OneToOne_Required_PK_Date], [t9].[Level1_Optional_Id], [t9].[Level1_Required_Id], [t9].[Level2_Name], [t9].[OneToMany_Optional_Inverse2Id], [t9].[OneToMany_Required_Inverse2Id], [t9].[OneToOne_Optional_PK_Inverse2Id], [t9].[PeriodEnd], [t9].[PeriodStart], [t9].[Id0], [t9].[Level2_Optional_Id], [t9].[Level2_Required_Id], [t9].[Level3_Name], [t9].[OneToMany_Optional_Inverse3Id], [t9].[OneToMany_Required_Inverse3Id], [t9].[OneToOne_Optional_PK_Inverse3Id], [t9].[PeriodEnd0], [t9].[PeriodStart0], [t9].[Id1], [t9].[Level3_Optional_Id], [t9].[Level3_Required_Id], [t9].[Level4_Name], [t9].[OneToMany_Optional_Inverse4Id], [t9].[OneToMany_Required_Inverse4Id], [t9].[OneToOne_Optional_PK_Inverse4Id], [t9].[PeriodEnd1], [t9].[PeriodStart1], [t9].[Id2], [t9].[Level2_Optional_Id0], [t9].[Level2_Required_Id0], [t9].[Level3_Name0], [t9].[OneToMany_Optional_Inverse3Id0], [t9].[OneToMany_Required_Inverse3Id0], [t9].[OneToOne_Optional_PK_Inverse3Id0], [t9].[PeriodEnd2], [t9].[PeriodStart2], [t9].[Id3], [t9].[Id00], [t9].[Id000], [t9].[Id01], [t9].[Id001], [t9].[Id0000], [t9].[Id02], [t9].[Id002], [t9].[Id4], [t9].[Level3_Optional_Id0], [t9].[Level3_Required_Id0], [t9].[Level4_Name0], [t9].[OneToMany_Optional_Inverse4Id0], [t9].[OneToMany_Required_Inverse4Id0], [t9].[OneToOne_Optional_PK_Inverse4Id0], [t9].[PeriodEnd3], [t9].[PeriodStart3], [t9].[Id03], [t9].[Id003], [t9].[Id0001]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [t0].[Id] AS [Id0], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd] AS [PeriodEnd0], [t0].[PeriodStart] AS [PeriodStart0], [t1].[Id] AS [Id1], [t1].[Level3_Optional_Id], [t1].[Level3_Required_Id], [t1].[Level4_Name], [t1].[OneToMany_Optional_Inverse4Id], [t1].[OneToMany_Required_Inverse4Id], [t1].[OneToOne_Optional_PK_Inverse4Id], [t1].[PeriodEnd] AS [PeriodEnd1], [t1].[PeriodStart] AS [PeriodStart1], [t4].[Id] AS [Id2], [t4].[Level2_Optional_Id] AS [Level2_Optional_Id0], [t4].[Level2_Required_Id] AS [Level2_Required_Id0], [t4].[Level3_Name] AS [Level3_Name0], [t4].[OneToMany_Optional_Inverse3Id] AS [OneToMany_Optional_Inverse3Id0], [t4].[OneToMany_Required_Inverse3Id] AS [OneToMany_Required_Inverse3Id0], [t4].[OneToOne_Optional_PK_Inverse3Id] AS [OneToOne_Optional_PK_Inverse3Id0], [t4].[PeriodEnd] AS [PeriodEnd2], [t4].[PeriodStart] AS [PeriodStart2], [l1].[Id] AS [Id3], [t0].[Id0] AS [Id00], [t0].[Id00] AS [Id000], [t1].[Id0] AS [Id01], [t1].[Id00] AS [Id001], [t1].[Id000] AS [Id0000], [t4].[Id0] AS [Id02], [t4].[Id00] AS [Id002], [t6].[Id] AS [Id4], [t6].[Level3_Optional_Id] AS [Level3_Optional_Id0], [t6].[Level3_Required_Id] AS [Level3_Required_Id0], [t6].[Level4_Name] AS [Level4_Name0], [t6].[OneToMany_Optional_Inverse4Id] AS [OneToMany_Optional_Inverse4Id0], [t6].[OneToMany_Required_Inverse4Id] AS [OneToMany_Required_Inverse4Id0], [t6].[OneToOne_Optional_PK_Inverse4Id] AS [OneToOne_Optional_PK_Inverse4Id0], [t6].[PeriodEnd] AS [PeriodEnd3], [t6].[PeriodStart] AS [PeriodStart3], [t6].[Id0] AS [Id03], [t6].[Id00] AS [Id003], [t6].[Id000] AS [Id0001]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t].[Id] AS [Id0], [t].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t] ON [l2].[Id] = [t].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t0] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [t0].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN (
        SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00], [t2].[Id00] AS [Id000]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
        INNER JOIN (
            SELECT [l6].[Id], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
            INNER JOIN (
                SELECT [l7].[Id], [l8].[Id] AS [Id0]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
                INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8] ON [l7].[Id] = [l8].[Id]
                WHERE [l7].[OneToOne_Required_PK_Date] IS NOT NULL AND [l7].[Level1_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse2Id] IS NOT NULL
            ) AS [t3] ON [l6].[Id] = [t3].[Id]
            WHERE [l6].[Level2_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse3Id] IS NOT NULL
        ) AS [t2] ON [l5].[Id] = [t2].[Id]
        WHERE [l5].[Level3_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [t1] ON CASE
        WHEN [t0].[Level2_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse3Id] IS NOT NULL AND [t0].[PeriodEnd] IS NOT NULL AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
    END = [t1].[Level3_Optional_Id]
    LEFT JOIN (
        SELECT [l9].[Id], [l9].[Level2_Optional_Id], [l9].[Level2_Required_Id], [l9].[Level3_Name], [l9].[OneToMany_Optional_Inverse3Id], [l9].[OneToMany_Required_Inverse3Id], [l9].[OneToOne_Optional_PK_Inverse3Id], [l9].[PeriodEnd], [l9].[PeriodStart], [t5].[Id] AS [Id0], [t5].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l9]
        INNER JOIN (
            SELECT [l10].[Id], [l11].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l10]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l11] ON [l10].[Id] = [l11].[Id]
            WHERE [l10].[OneToOne_Required_PK_Date] IS NOT NULL AND [l10].[Level1_Required_Id] IS NOT NULL AND [l10].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t5] ON [l9].[Id] = [t5].[Id]
        WHERE [l9].[Level2_Required_Id] IS NOT NULL AND [l9].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t4] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [t4].[Level2_Optional_Id]
    LEFT JOIN (
        SELECT [l12].[Id], [l12].[Level3_Optional_Id], [l12].[Level3_Required_Id], [l12].[Level4_Name], [l12].[OneToMany_Optional_Inverse4Id], [l12].[OneToMany_Required_Inverse4Id], [l12].[OneToOne_Optional_PK_Inverse4Id], [l12].[PeriodEnd], [l12].[PeriodStart], [t7].[Id] AS [Id0], [t7].[Id0] AS [Id00], [t7].[Id00] AS [Id000]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l12]
        INNER JOIN (
            SELECT [l13].[Id], [t8].[Id] AS [Id0], [t8].[Id0] AS [Id00]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l13]
            INNER JOIN (
                SELECT [l14].[Id], [l15].[Id] AS [Id0]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l14]
                INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l15] ON [l14].[Id] = [l15].[Id]
                WHERE [l14].[OneToOne_Required_PK_Date] IS NOT NULL AND [l14].[Level1_Required_Id] IS NOT NULL AND [l14].[OneToMany_Required_Inverse2Id] IS NOT NULL
            ) AS [t8] ON [l13].[Id] = [t8].[Id]
            WHERE [l13].[Level2_Required_Id] IS NOT NULL AND [l13].[OneToMany_Required_Inverse3Id] IS NOT NULL
        ) AS [t7] ON [l12].[Id] = [t7].[Id]
        WHERE [l12].[Level3_Required_Id] IS NOT NULL AND [l12].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [t6] ON CASE
        WHEN [t4].[Level2_Required_Id] IS NOT NULL AND [t4].[OneToMany_Required_Inverse3Id] IS NOT NULL AND [t4].[PeriodEnd] IS NOT NULL AND [t4].[PeriodStart] IS NOT NULL THEN [t4].[Id]
    END = [t6].[OneToMany_Optional_Inverse4Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t9] ON [l].[Id] = [t9].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t9].[Id], [t9].[Id3], [t9].[Id0], [t9].[Id00], [t9].[Id000], [t9].[Id1], [t9].[Id01], [t9].[Id001], [t9].[Id0000], [t9].[Id2], [t9].[Id02], [t9].[Id002], [t9].[Id4], [t9].[Id03], [t9].[Id003]");
    }

    public override async Task Include_collection_multiple_with_filter(bool async)
    {
        await base.Include_collection_multiple_with_filter(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t4].[Id], [t4].[OneToOne_Required_PK_Date], [t4].[Level1_Optional_Id], [t4].[Level1_Required_Id], [t4].[Level2_Name], [t4].[OneToMany_Optional_Inverse2Id], [t4].[OneToMany_Required_Inverse2Id], [t4].[OneToOne_Optional_PK_Inverse2Id], [t4].[PeriodEnd], [t4].[PeriodStart], [t4].[Id0], [t4].[Level2_Optional_Id], [t4].[Level2_Required_Id], [t4].[Level3_Name], [t4].[OneToMany_Optional_Inverse3Id], [t4].[OneToMany_Required_Inverse3Id], [t4].[OneToOne_Optional_PK_Inverse3Id], [t4].[PeriodEnd0], [t4].[PeriodStart0], [t4].[Id1], [t4].[Level3_Optional_Id], [t4].[Level3_Required_Id], [t4].[Level4_Name], [t4].[OneToMany_Optional_Inverse4Id], [t4].[OneToMany_Required_Inverse4Id], [t4].[OneToOne_Optional_PK_Inverse4Id], [t4].[PeriodEnd1], [t4].[PeriodStart1], [t4].[Id2], [t4].[Id00], [t4].[Id000], [t4].[Id01], [t4].[Id001], [t4].[Id0000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l5].[Id], [l5].[OneToOne_Required_PK_Date], [l5].[Level1_Optional_Id], [l5].[Level1_Required_Id], [l5].[Level2_Name], [l5].[OneToMany_Optional_Inverse2Id], [l5].[OneToMany_Required_Inverse2Id], [l5].[OneToOne_Optional_PK_Inverse2Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd] AS [PeriodEnd0], [t1].[PeriodStart] AS [PeriodStart0], [t2].[Id] AS [Id1], [t2].[Level3_Optional_Id], [t2].[Level3_Required_Id], [t2].[Level4_Name], [t2].[OneToMany_Optional_Inverse4Id], [t2].[OneToMany_Required_Inverse4Id], [t2].[OneToOne_Optional_PK_Inverse4Id], [t2].[PeriodEnd] AS [PeriodEnd1], [t2].[PeriodStart] AS [PeriodStart1], [l6].[Id] AS [Id2], [t1].[Id0] AS [Id00], [t1].[Id00] AS [Id000], [t2].[Id0] AS [Id01], [t2].[Id00] AS [Id001], [t2].[Id000] AS [Id0000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6] ON [l5].[Id] = [l6].[Id]
    LEFT JOIN (
        SELECT [l7].[Id], [l7].[Level2_Optional_Id], [l7].[Level2_Required_Id], [l7].[Level3_Name], [l7].[OneToMany_Optional_Inverse3Id], [l7].[OneToMany_Required_Inverse3Id], [l7].[OneToOne_Optional_PK_Inverse3Id], [l7].[PeriodEnd], [l7].[PeriodStart], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
        INNER JOIN (
            SELECT [l8].[Id], [l9].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l9] ON [l8].[Id] = [l9].[Id]
            WHERE [l8].[OneToOne_Required_PK_Date] IS NOT NULL AND [l8].[Level1_Required_Id] IS NOT NULL AND [l8].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t3] ON [l7].[Id] = [t3].[Id]
        WHERE [l7].[Level2_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t1] ON CASE
        WHEN [l5].[OneToOne_Required_PK_Date] IS NOT NULL AND [l5].[Level1_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l5].[Id]
    END = [t1].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN (
        SELECT [l10].[Id], [l10].[Level3_Optional_Id], [l10].[Level3_Required_Id], [l10].[Level4_Name], [l10].[OneToMany_Optional_Inverse4Id], [l10].[OneToMany_Required_Inverse4Id], [l10].[OneToOne_Optional_PK_Inverse4Id], [l10].[PeriodEnd], [l10].[PeriodStart], [t5].[Id] AS [Id0], [t5].[Id0] AS [Id00], [t5].[Id00] AS [Id000]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l10]
        INNER JOIN (
            SELECT [l11].[Id], [t6].[Id] AS [Id0], [t6].[Id0] AS [Id00]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l11]
            INNER JOIN (
                SELECT [l12].[Id], [l13].[Id] AS [Id0]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l12]
                INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l13] ON [l12].[Id] = [l13].[Id]
                WHERE [l12].[OneToOne_Required_PK_Date] IS NOT NULL AND [l12].[Level1_Required_Id] IS NOT NULL AND [l12].[OneToMany_Required_Inverse2Id] IS NOT NULL
            ) AS [t6] ON [l11].[Id] = [t6].[Id]
            WHERE [l11].[Level2_Required_Id] IS NOT NULL AND [l11].[OneToMany_Required_Inverse3Id] IS NOT NULL
        ) AS [t5] ON [l10].[Id] = [t5].[Id]
        WHERE [l10].[Level3_Required_Id] IS NOT NULL AND [l10].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [t2] ON CASE
        WHEN [t1].[Level2_Required_Id] IS NOT NULL AND [t1].[OneToMany_Required_Inverse3Id] IS NOT NULL AND [t1].[PeriodEnd] IS NOT NULL AND [t1].[PeriodStart] IS NOT NULL THEN [t1].[Id]
    END = [t2].[Level3_Optional_Id]
    WHERE [l5].[OneToOne_Required_PK_Date] IS NOT NULL AND [l5].[Level1_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t4] ON [l].[Id] = [t4].[OneToMany_Optional_Inverse2Id]
WHERE (
    SELECT COUNT(*)
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t].[Id] AS [Id0], [t].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l3].[PeriodEnd], [l3].[PeriodStart], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t] ON [l2].[Id] = [t].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t0] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [t0].[OneToOne_Optional_PK_Inverse3Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id] AND ([t0].[Level3_Name] <> N'Foo' OR [t0].[Level3_Name] IS NULL)) > 0
ORDER BY [l].[Id], [t4].[Id], [t4].[Id2], [t4].[Id0], [t4].[Id00], [t4].[Id000], [t4].[Id1], [t4].[Id01], [t4].[Id001]");
    }

    public override async Task Include_collection_ThenInclude_reference_followed_by_projection_into_anonmous_type(bool async)
    {
        await base.Include_collection_ThenInclude_reference_followed_by_projection_into_anonmous_type(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t1].[Id], [t1].[OneToOne_Required_PK_Date], [t1].[Level1_Optional_Id], [t1].[Level1_Required_Id], [t1].[Level2_Name], [t1].[OneToMany_Optional_Inverse2Id], [t1].[OneToMany_Required_Inverse2Id], [t1].[OneToOne_Optional_PK_Inverse2Id], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Id0], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd0], [t1].[PeriodStart0], [t1].[Id1], [t1].[Id00], [t1].[Id000], [t2].[Id], [t2].[OneToOne_Required_PK_Date], [t2].[Level1_Optional_Id], [t2].[Level1_Required_Id], [t2].[Level2_Name], [t2].[OneToMany_Optional_Inverse2Id], [t2].[OneToMany_Required_Inverse2Id], [t2].[OneToOne_Optional_PK_Inverse2Id], [t2].[PeriodEnd], [t2].[PeriodStart], [t2].[Id0], [t2].[Level2_Optional_Id], [t2].[Level2_Required_Id], [t2].[Level3_Name], [t2].[OneToMany_Optional_Inverse3Id], [t2].[OneToMany_Required_Inverse3Id], [t2].[OneToOne_Optional_PK_Inverse3Id], [t2].[PeriodEnd0], [t2].[PeriodStart0], [t2].[Id1], [t2].[Id00], [t2].[Id000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [t0].[Id] AS [Id0], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd] AS [PeriodEnd0], [t0].[PeriodStart] AS [PeriodStart0], [l1].[Id] AS [Id1], [t0].[Id0] AS [Id00], [t0].[Id00] AS [Id000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t].[Id] AS [Id0], [t].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t] ON [l2].[Id] = [t].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t0] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [t0].[OneToOne_Optional_PK_Inverse3Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t1] ON [l].[Id] = [t1].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l5].[Id], [l5].[OneToOne_Required_PK_Date], [l5].[Level1_Optional_Id], [l5].[Level1_Required_Id], [l5].[Level2_Name], [l5].[OneToMany_Optional_Inverse2Id], [l5].[OneToMany_Required_Inverse2Id], [l5].[OneToOne_Optional_PK_Inverse2Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t3].[Id] AS [Id0], [t3].[Level2_Optional_Id], [t3].[Level2_Required_Id], [t3].[Level3_Name], [t3].[OneToMany_Optional_Inverse3Id], [t3].[OneToMany_Required_Inverse3Id], [t3].[OneToOne_Optional_PK_Inverse3Id], [t3].[PeriodEnd] AS [PeriodEnd0], [t3].[PeriodStart] AS [PeriodStart0], [l6].[Id] AS [Id1], [t3].[Id0] AS [Id00], [t3].[Id00] AS [Id000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6] ON [l5].[Id] = [l6].[Id]
    LEFT JOIN (
        SELECT [l7].[Id], [l7].[Level2_Optional_Id], [l7].[Level2_Required_Id], [l7].[Level3_Name], [l7].[OneToMany_Optional_Inverse3Id], [l7].[OneToMany_Required_Inverse3Id], [l7].[OneToOne_Optional_PK_Inverse3Id], [l7].[PeriodEnd], [l7].[PeriodStart], [t4].[Id] AS [Id0], [t4].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
        INNER JOIN (
            SELECT [l8].[Id], [l9].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l9] ON [l8].[Id] = [l9].[Id]
            WHERE [l8].[OneToOne_Required_PK_Date] IS NOT NULL AND [l8].[Level1_Required_Id] IS NOT NULL AND [l8].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t4] ON [l7].[Id] = [t4].[Id]
        WHERE [l7].[Level2_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t3] ON CASE
        WHEN [l5].[OneToOne_Required_PK_Date] IS NOT NULL AND [l5].[Level1_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l5].[Id]
    END = [t3].[OneToOne_Optional_PK_Inverse3Id]
    WHERE [l5].[OneToOne_Required_PK_Date] IS NOT NULL AND [l5].[Level1_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t2] ON [l].[Id] = [t2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t1].[Id], [t1].[Id1], [t1].[Id0], [t1].[Id00], [t1].[Id000], [t2].[Id], [t2].[Id1], [t2].[Id0], [t2].[Id00]");
    }

    public override async Task Include_collection_ThenInclude_two_references(bool async)
    {
        await base.Include_collection_ThenInclude_two_references(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t4].[Id], [t4].[OneToOne_Required_PK_Date], [t4].[Level1_Optional_Id], [t4].[Level1_Required_Id], [t4].[Level2_Name], [t4].[OneToMany_Optional_Inverse2Id], [t4].[OneToMany_Required_Inverse2Id], [t4].[OneToOne_Optional_PK_Inverse2Id], [t4].[PeriodEnd], [t4].[PeriodStart], [t4].[Id0], [t4].[Level2_Optional_Id], [t4].[Level2_Required_Id], [t4].[Level3_Name], [t4].[OneToMany_Optional_Inverse3Id], [t4].[OneToMany_Required_Inverse3Id], [t4].[OneToOne_Optional_PK_Inverse3Id], [t4].[PeriodEnd0], [t4].[PeriodStart0], [t4].[Id1], [t4].[Level3_Optional_Id], [t4].[Level3_Required_Id], [t4].[Level4_Name], [t4].[OneToMany_Optional_Inverse4Id], [t4].[OneToMany_Required_Inverse4Id], [t4].[OneToOne_Optional_PK_Inverse4Id], [t4].[PeriodEnd1], [t4].[PeriodStart1], [t4].[Id2], [t4].[Id00], [t4].[Id000], [t4].[Id01], [t4].[Id001], [t4].[Id0000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [t0].[Id] AS [Id0], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd] AS [PeriodEnd0], [t0].[PeriodStart] AS [PeriodStart0], [t1].[Id] AS [Id1], [t1].[Level3_Optional_Id], [t1].[Level3_Required_Id], [t1].[Level4_Name], [t1].[OneToMany_Optional_Inverse4Id], [t1].[OneToMany_Required_Inverse4Id], [t1].[OneToOne_Optional_PK_Inverse4Id], [t1].[PeriodEnd] AS [PeriodEnd1], [t1].[PeriodStart] AS [PeriodStart1], [l1].[Id] AS [Id2], [t0].[Id0] AS [Id00], [t0].[Id00] AS [Id000], [t1].[Id0] AS [Id01], [t1].[Id00] AS [Id001], [t1].[Id000] AS [Id0000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t].[Id] AS [Id0], [t].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t] ON [l2].[Id] = [t].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t0] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [t0].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN (
        SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00], [t2].[Id00] AS [Id000]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
        INNER JOIN (
            SELECT [l6].[Id], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
            INNER JOIN (
                SELECT [l7].[Id], [l8].[Id] AS [Id0]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
                INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8] ON [l7].[Id] = [l8].[Id]
                WHERE [l7].[OneToOne_Required_PK_Date] IS NOT NULL AND [l7].[Level1_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse2Id] IS NOT NULL
            ) AS [t3] ON [l6].[Id] = [t3].[Id]
            WHERE [l6].[Level2_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse3Id] IS NOT NULL
        ) AS [t2] ON [l5].[Id] = [t2].[Id]
        WHERE [l5].[Level3_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [t1] ON CASE
        WHEN [t0].[Level2_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse3Id] IS NOT NULL AND [t0].[PeriodEnd] IS NOT NULL AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
    END = [t1].[Level3_Optional_Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t4] ON [l].[Id] = [t4].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t4].[Id], [t4].[Id2], [t4].[Id0], [t4].[Id00], [t4].[Id000], [t4].[Id1], [t4].[Id01], [t4].[Id001]");
    }

    public override async Task Include_collection_then_reference(bool async)
    {
        await base.Include_collection_then_reference(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t1].[Id], [t1].[OneToOne_Required_PK_Date], [t1].[Level1_Optional_Id], [t1].[Level1_Required_Id], [t1].[Level2_Name], [t1].[OneToMany_Optional_Inverse2Id], [t1].[OneToMany_Required_Inverse2Id], [t1].[OneToOne_Optional_PK_Inverse2Id], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Id0], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd0], [t1].[PeriodStart0], [t1].[Id1], [t1].[Id00], [t1].[Id000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [t0].[Id] AS [Id0], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd] AS [PeriodEnd0], [t0].[PeriodStart] AS [PeriodStart0], [l1].[Id] AS [Id1], [t0].[Id0] AS [Id00], [t0].[Id00] AS [Id000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t].[Id] AS [Id0], [t].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t] ON [l2].[Id] = [t].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t0] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [t0].[Level2_Optional_Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t1] ON [l].[Id] = [t1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t1].[Id], [t1].[Id1], [t1].[Id0], [t1].[Id00]");
    }

    public override async Task Include_collection_with_conditional_order_by(bool async)
    {
        await base.Include_collection_with_conditional_order_by(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY CASE
    WHEN [l].[Name] IS NOT NULL AND ([l].[Name] LIKE N'%03') THEN 1
    ELSE 2
END, [l].[Id], [t].[Id]");
    }

    public override async Task Include_collection_with_groupby_in_subquery(bool async)
    {
        await base.Include_collection_with_groupby_in_subquery(async);

        AssertSql(
            @"SELECT [t0].[Id], [t0].[Date], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart], [t].[Name], [t2].[Id], [t2].[OneToOne_Required_PK_Date], [t2].[Level1_Optional_Id], [t2].[Level1_Required_Id], [t2].[Level2_Name], [t2].[OneToMany_Optional_Inverse2Id], [t2].[OneToMany_Required_Inverse2Id], [t2].[OneToOne_Optional_PK_Inverse2Id], [t2].[PeriodEnd], [t2].[PeriodStart], [t2].[Id0]
FROM (
    SELECT [l].[Name]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    GROUP BY [l].[Name]
) AS [t]
LEFT JOIN (
    SELECT [t1].[Id], [t1].[Date], [t1].[Name], [t1].[PeriodEnd], [t1].[PeriodStart]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name], [l0].[PeriodEnd], [l0].[PeriodStart], ROW_NUMBER() OVER(PARTITION BY [l0].[Name] ORDER BY [l0].[Id]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    ) AS [t1]
    WHERE [t1].[row] <= 1
) AS [t0] ON [t].[Name] = [t0].[Name]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[PeriodEnd], [l1].[PeriodStart], [l2].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2] ON [l1].[Id] = [l2].[Id]
    WHERE [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t2] ON [t0].[Id] = [t2].[OneToMany_Optional_Inverse2Id]
ORDER BY [t].[Name], [t0].[Id], [t2].[Id]");
    }

    public override async Task Include_collection_with_groupby_in_subquery_and_filter_after_groupby(bool async)
    {
        await base.Include_collection_with_groupby_in_subquery_and_filter_after_groupby(async);

        AssertSql(
            @"SELECT [t0].[Id], [t0].[Date], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart], [t].[Name], [t2].[Id], [t2].[OneToOne_Required_PK_Date], [t2].[Level1_Optional_Id], [t2].[Level1_Required_Id], [t2].[Level2_Name], [t2].[OneToMany_Optional_Inverse2Id], [t2].[OneToMany_Required_Inverse2Id], [t2].[OneToOne_Optional_PK_Inverse2Id], [t2].[PeriodEnd], [t2].[PeriodStart], [t2].[Id0]
FROM (
    SELECT [l].[Name]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    GROUP BY [l].[Name]
    HAVING [l].[Name] <> N'Foo' OR [l].[Name] IS NULL
) AS [t]
LEFT JOIN (
    SELECT [t1].[Id], [t1].[Date], [t1].[Name], [t1].[PeriodEnd], [t1].[PeriodStart]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name], [l0].[PeriodEnd], [l0].[PeriodStart], ROW_NUMBER() OVER(PARTITION BY [l0].[Name] ORDER BY [l0].[Id]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    ) AS [t1]
    WHERE [t1].[row] <= 1
) AS [t0] ON [t].[Name] = [t0].[Name]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[PeriodEnd], [l1].[PeriodStart], [l2].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2] ON [l1].[Id] = [l2].[Id]
    WHERE [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t2] ON [t0].[Id] = [t2].[OneToMany_Optional_Inverse2Id]
ORDER BY [t].[Name], [t0].[Id], [t2].[Id]");
    }

    public override async Task Include_collection_with_groupby_in_subquery_and_filter_before_groupby(bool async)
    {
        await base.Include_collection_with_groupby_in_subquery_and_filter_before_groupby(async);

        AssertSql(
            @"SELECT [t0].[Id], [t0].[Date], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart], [t].[Name], [t2].[Id], [t2].[OneToOne_Required_PK_Date], [t2].[Level1_Optional_Id], [t2].[Level1_Required_Id], [t2].[Level2_Name], [t2].[OneToMany_Optional_Inverse2Id], [t2].[OneToMany_Required_Inverse2Id], [t2].[OneToOne_Optional_PK_Inverse2Id], [t2].[PeriodEnd], [t2].[PeriodStart], [t2].[Id0]
FROM (
    SELECT [l].[Name]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    WHERE [l].[Id] > 3
    GROUP BY [l].[Name]
) AS [t]
LEFT JOIN (
    SELECT [t1].[Id], [t1].[Date], [t1].[Name], [t1].[PeriodEnd], [t1].[PeriodStart]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name], [l0].[PeriodEnd], [l0].[PeriodStart], ROW_NUMBER() OVER(PARTITION BY [l0].[Name] ORDER BY [l0].[Id]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        WHERE [l0].[Id] > 3
    ) AS [t1]
    WHERE [t1].[row] <= 1
) AS [t0] ON [t].[Name] = [t0].[Name]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[PeriodEnd], [l1].[PeriodStart], [l2].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2] ON [l1].[Id] = [l2].[Id]
    WHERE [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t2] ON [t0].[Id] = [t2].[OneToMany_Optional_Inverse2Id]
ORDER BY [t].[Name], [t0].[Id], [t2].[Id]");
    }

    public override async Task Include_nested_with_optional_navigation(bool async)
    {
        await base.Include_nested_with_optional_navigation(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t3].[Id], [t3].[Level2_Optional_Id], [t3].[Level2_Required_Id], [t3].[Level3_Name], [t3].[OneToMany_Optional_Inverse3Id], [t3].[OneToMany_Required_Inverse3Id], [t3].[OneToOne_Optional_PK_Inverse3Id], [t3].[PeriodEnd], [t3].[PeriodStart], [t3].[Id0], [t3].[Level3_Optional_Id], [t3].[Level3_Required_Id], [t3].[Level4_Name], [t3].[OneToMany_Optional_Inverse4Id], [t3].[OneToMany_Required_Inverse4Id], [t3].[OneToOne_Optional_PK_Inverse4Id], [t3].[PeriodEnd0], [t3].[PeriodStart0], [t3].[Id1], [t3].[Id00], [t3].[Id01], [t3].[Id000], [t3].[Id0000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Level3_Optional_Id], [t1].[Level3_Required_Id], [t1].[Level4_Name], [t1].[OneToMany_Optional_Inverse4Id], [t1].[OneToMany_Required_Inverse4Id], [t1].[OneToOne_Optional_PK_Inverse4Id], [t1].[PeriodEnd] AS [PeriodEnd0], [t1].[PeriodStart] AS [PeriodStart0], [t0].[Id] AS [Id1], [t0].[Id0] AS [Id00], [t1].[Id0] AS [Id01], [t1].[Id00] AS [Id000], [t1].[Id000] AS [Id0000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t0] ON [l2].[Id] = [t0].[Id]
    LEFT JOIN (
        SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00], [t2].[Id00] AS [Id000]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
        INNER JOIN (
            SELECT [l6].[Id], [t4].[Id] AS [Id0], [t4].[Id0] AS [Id00]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
            INNER JOIN (
                SELECT [l7].[Id], [l8].[Id] AS [Id0]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
                INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8] ON [l7].[Id] = [l8].[Id]
                WHERE [l7].[OneToOne_Required_PK_Date] IS NOT NULL AND [l7].[Level1_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse2Id] IS NOT NULL
            ) AS [t4] ON [l6].[Id] = [t4].[Id]
            WHERE [l6].[Level2_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse3Id] IS NOT NULL
        ) AS [t2] ON [l5].[Id] = [t2].[Id]
        WHERE [l5].[Level3_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [t1] ON CASE
        WHEN [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l2].[Id]
    END = [t1].[Level3_Required_Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t3] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t3].[OneToMany_Required_Inverse3Id]
WHERE [t].[Level2_Name] <> N'L2 09' OR [t].[Level2_Name] IS NULL
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t3].[Id], [t3].[Id1], [t3].[Id00], [t3].[Id0], [t3].[Id01], [t3].[Id000]");
    }

    public override async Task Include_partially_added_before_Where_and_then_build_upon(bool async)
    {
        await base.Include_partially_added_before_Where_and_then_build_upon(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t].[Id], [t].[Id0], [t0].[Id0], [t3].[Id], [t3].[Level2_Optional_Id], [t3].[Level2_Required_Id], [t3].[Level3_Name], [t3].[OneToMany_Optional_Inverse3Id], [t3].[OneToMany_Required_Inverse3Id], [t3].[OneToOne_Optional_PK_Inverse3Id], [t3].[PeriodEnd], [t3].[PeriodStart], [t3].[Id0], [t3].[Level3_Optional_Id], [t3].[Level3_Required_Id], [t3].[Level4_Name], [t3].[OneToMany_Optional_Inverse4Id], [t3].[OneToMany_Required_Inverse4Id], [t3].[OneToOne_Optional_PK_Inverse4Id], [t3].[PeriodEnd0], [t3].[PeriodStart0], [t3].[Id1], [t3].[Id00], [t3].[Id01], [t3].[Id000], [t3].[Id0000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToOne_Optional_PK_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[PeriodEnd], [l2].[PeriodStart], [l3].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3] ON [l2].[Id] = [l3].[Id]
    WHERE [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t0] ON [l].[Id] = [t0].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id], [l4].[PeriodEnd], [l4].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Level3_Optional_Id], [t2].[Level3_Required_Id], [t2].[Level4_Name], [t2].[OneToMany_Optional_Inverse4Id], [t2].[OneToMany_Required_Inverse4Id], [t2].[OneToOne_Optional_PK_Inverse4Id], [t2].[PeriodEnd] AS [PeriodEnd0], [t2].[PeriodStart] AS [PeriodStart0], [t1].[Id] AS [Id1], [t1].[Id0] AS [Id00], [t2].[Id0] AS [Id01], [t2].[Id00] AS [Id000], [t2].[Id000] AS [Id0000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4]
    INNER JOIN (
        SELECT [l5].[Id], [l6].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6] ON [l5].[Id] = [l6].[Id]
        WHERE [l5].[OneToOne_Required_PK_Date] IS NOT NULL AND [l5].[Level1_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l4].[Id] = [t1].[Id]
    LEFT JOIN (
        SELECT [l7].[Id], [l7].[Level3_Optional_Id], [l7].[Level3_Required_Id], [l7].[Level4_Name], [l7].[OneToMany_Optional_Inverse4Id], [l7].[OneToMany_Required_Inverse4Id], [l7].[OneToOne_Optional_PK_Inverse4Id], [l7].[PeriodEnd], [l7].[PeriodStart], [t4].[Id] AS [Id0], [t4].[Id0] AS [Id00], [t4].[Id00] AS [Id000]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
        INNER JOIN (
            SELECT [l8].[Id], [t5].[Id] AS [Id0], [t5].[Id0] AS [Id00]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8]
            INNER JOIN (
                SELECT [l9].[Id], [l10].[Id] AS [Id0]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l9]
                INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l10] ON [l9].[Id] = [l10].[Id]
                WHERE [l9].[OneToOne_Required_PK_Date] IS NOT NULL AND [l9].[Level1_Required_Id] IS NOT NULL AND [l9].[OneToMany_Required_Inverse2Id] IS NOT NULL
            ) AS [t5] ON [l8].[Id] = [t5].[Id]
            WHERE [l8].[Level2_Required_Id] IS NOT NULL AND [l8].[OneToMany_Required_Inverse3Id] IS NOT NULL
        ) AS [t4] ON [l7].[Id] = [t4].[Id]
        WHERE [l7].[Level3_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [t2] ON CASE
        WHEN [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l4].[Id]
    END = [t2].[Level3_Optional_Id]
    WHERE [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t3] ON CASE
    WHEN [t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t0].[PeriodEnd] IS NOT NULL AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
END = [t3].[OneToMany_Optional_Inverse3Id]
WHERE CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END < 3 OR CASE
    WHEN [t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t0].[PeriodEnd] IS NOT NULL AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
END > 8
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0], [t3].[Id], [t3].[Id1], [t3].[Id00], [t3].[Id0], [t3].[Id01], [t3].[Id000]");
    }

    public override async Task Include_partially_added_before_Where_and_then_build_upon_with_filtered_include(bool async)
    {
        await base.Include_partially_added_before_Where_and_then_build_upon_with_filtered_include(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t].[Id], [t].[Id0], [t0].[Id0], [t1].[Id], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Id0], [t1].[Id00], [t4].[Id], [t4].[Level2_Optional_Id], [t4].[Level2_Required_Id], [t4].[Level3_Name], [t4].[OneToMany_Optional_Inverse3Id], [t4].[OneToMany_Required_Inverse3Id], [t4].[OneToOne_Optional_PK_Inverse3Id], [t4].[PeriodEnd], [t4].[PeriodStart], [t4].[Id0], [t4].[Level3_Optional_Id], [t4].[Level3_Required_Id], [t4].[Level4_Name], [t4].[OneToMany_Optional_Inverse4Id], [t4].[OneToMany_Required_Inverse4Id], [t4].[OneToOne_Optional_PK_Inverse4Id], [t4].[PeriodEnd0], [t4].[PeriodStart0], [t4].[Id1], [t4].[Id00], [t4].[Id01], [t4].[Id000], [t4].[Id0000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToOne_Optional_PK_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[PeriodEnd], [l2].[PeriodStart], [l3].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3] ON [l2].[Id] = [l3].[Id]
    WHERE [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t0] ON [l].[Id] = [t0].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [t2].[Id], [t2].[Level2_Optional_Id], [t2].[Level2_Required_Id], [t2].[Level3_Name], [t2].[OneToMany_Optional_Inverse3Id], [t2].[OneToMany_Required_Inverse3Id], [t2].[OneToOne_Optional_PK_Inverse3Id], [t2].[PeriodEnd], [t2].[PeriodStart], [t2].[Id0], [t2].[Id00], [t2].[c]
    FROM (
        SELECT [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id], [l4].[PeriodEnd], [l4].[PeriodStart], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00], ROW_NUMBER() OVER(PARTITION BY [l4].[OneToMany_Optional_Inverse3Id] ORDER BY CASE
            WHEN [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l4].[Id]
        END) AS [row], CASE
            WHEN [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l4].[Id]
        END AS [c]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4]
        INNER JOIN (
            SELECT [l5].[Id], [l6].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6] ON [l5].[Id] = [l6].[Id]
            WHERE [l5].[OneToOne_Required_PK_Date] IS NOT NULL AND [l5].[Level1_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t3] ON [l4].[Id] = [t3].[Id]
        WHERE [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t2]
    WHERE [t2].[row] <= 3
) AS [t1] ON CASE
    WHEN [t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t0].[PeriodEnd] IS NOT NULL AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
END = [t1].[OneToMany_Optional_Inverse3Id]
LEFT JOIN (
    SELECT [l7].[Id], [l7].[Level2_Optional_Id], [l7].[Level2_Required_Id], [l7].[Level3_Name], [l7].[OneToMany_Optional_Inverse3Id], [l7].[OneToMany_Required_Inverse3Id], [l7].[OneToOne_Optional_PK_Inverse3Id], [l7].[PeriodEnd], [l7].[PeriodStart], [t6].[Id] AS [Id0], [t6].[Level3_Optional_Id], [t6].[Level3_Required_Id], [t6].[Level4_Name], [t6].[OneToMany_Optional_Inverse4Id], [t6].[OneToMany_Required_Inverse4Id], [t6].[OneToOne_Optional_PK_Inverse4Id], [t6].[PeriodEnd] AS [PeriodEnd0], [t6].[PeriodStart] AS [PeriodStart0], [t5].[Id] AS [Id1], [t5].[Id0] AS [Id00], [t6].[Id0] AS [Id01], [t6].[Id00] AS [Id000], [t6].[Id000] AS [Id0000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
    INNER JOIN (
        SELECT [l8].[Id], [l9].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l9] ON [l8].[Id] = [l9].[Id]
        WHERE [l8].[OneToOne_Required_PK_Date] IS NOT NULL AND [l8].[Level1_Required_Id] IS NOT NULL AND [l8].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t5] ON [l7].[Id] = [t5].[Id]
    LEFT JOIN (
        SELECT [l10].[Id], [l10].[Level3_Optional_Id], [l10].[Level3_Required_Id], [l10].[Level4_Name], [l10].[OneToMany_Optional_Inverse4Id], [l10].[OneToMany_Required_Inverse4Id], [l10].[OneToOne_Optional_PK_Inverse4Id], [l10].[PeriodEnd], [l10].[PeriodStart], [t7].[Id] AS [Id0], [t7].[Id0] AS [Id00], [t7].[Id00] AS [Id000]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l10]
        INNER JOIN (
            SELECT [l11].[Id], [t8].[Id] AS [Id0], [t8].[Id0] AS [Id00]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l11]
            INNER JOIN (
                SELECT [l12].[Id], [l13].[Id] AS [Id0]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l12]
                INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l13] ON [l12].[Id] = [l13].[Id]
                WHERE [l12].[OneToOne_Required_PK_Date] IS NOT NULL AND [l12].[Level1_Required_Id] IS NOT NULL AND [l12].[OneToMany_Required_Inverse2Id] IS NOT NULL
            ) AS [t8] ON [l11].[Id] = [t8].[Id]
            WHERE [l11].[Level2_Required_Id] IS NOT NULL AND [l11].[OneToMany_Required_Inverse3Id] IS NOT NULL
        ) AS [t7] ON [l10].[Id] = [t7].[Id]
        WHERE [l10].[Level3_Required_Id] IS NOT NULL AND [l10].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [t6] ON CASE
        WHEN [l7].[Level2_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l7].[Id]
    END = [t6].[Level3_Optional_Id]
    WHERE [l7].[Level2_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t4] ON CASE
    WHEN [t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t0].[PeriodEnd] IS NOT NULL AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
END = [t4].[OneToMany_Required_Inverse3Id]
WHERE CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END < 3 OR CASE
    WHEN [t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t0].[PeriodEnd] IS NOT NULL AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
END > 8
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0], [t1].[OneToMany_Optional_Inverse3Id], [t1].[c], [t1].[Id], [t1].[Id0], [t1].[Id00], [t4].[Id], [t4].[Id1], [t4].[Id00], [t4].[Id0], [t4].[Id01], [t4].[Id000]");
    }

    public override async Task Include_reference_and_collection_order_by(bool async)
    {
        await base.Include_reference_and_collection_order_by(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Name], [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0]");
    }

    public override async Task Include_reference_collection_order_by_reference_navigation(bool async)
    {
        await base.Include_reference_collection_order_by_reference_navigation(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END, [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0]");
    }

    public override async Task Include_reference_followed_by_include_collection(bool async)
    {
        await base.Include_reference_followed_by_include_collection(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0]");
    }

    public override async Task Include_reference_ThenInclude_collection_order_by(bool async)
    {
        await base.Include_reference_ThenInclude_collection_order_by(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Name], [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0]");
    }

    public override async Task Include_ThenInclude_ThenInclude_followed_by_two_nested_selects(bool async)
    {
        await base.Include_ThenInclude_ThenInclude_followed_by_two_nested_selects(async);

        AssertSql(
            @"SELECT [l].[Id], [t4].[Id], [t4].[Level2_Optional_Id], [t4].[Level2_Required_Id], [t4].[Level3_Name], [t4].[OneToMany_Optional_Inverse3Id], [t4].[OneToMany_Required_Inverse3Id], [t4].[OneToOne_Optional_PK_Inverse3Id], [t4].[PeriodEnd], [t4].[PeriodStart], [t4].[Id0], [t4].[Level3_Optional_Id], [t4].[Level3_Required_Id], [t4].[Level4_Name], [t4].[OneToMany_Optional_Inverse4Id], [t4].[OneToMany_Required_Inverse4Id], [t4].[OneToOne_Optional_PK_Inverse4Id], [t4].[PeriodEnd0], [t4].[PeriodStart0], [t4].[Id1], [t4].[Id2], [t4].[Id00], [t4].[Id000], [t4].[Id01], [t4].[Id001], [t4].[Id0000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Level3_Optional_Id], [t1].[Level3_Required_Id], [t1].[Level4_Name], [t1].[OneToMany_Optional_Inverse4Id], [t1].[OneToMany_Required_Inverse4Id], [t1].[OneToOne_Optional_PK_Inverse4Id], [t1].[PeriodEnd] AS [PeriodEnd0], [t1].[PeriodStart] AS [PeriodStart0], [l0].[Id] AS [Id1], [l1].[Id] AS [Id2], [t0].[Id0] AS [Id00], [t0].[Id00] AS [Id000], [t1].[Id0] AS [Id01], [t1].[Id00] AS [Id001], [t1].[Id000] AS [Id0000], [l0].[OneToMany_Optional_Inverse2Id]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t].[Id] AS [Id0], [t].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t] ON [l2].[Id] = [t].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t0] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [t0].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN (
        SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00], [t2].[Id00] AS [Id000]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
        INNER JOIN (
            SELECT [l6].[Id], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
            INNER JOIN (
                SELECT [l7].[Id], [l8].[Id] AS [Id0]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
                INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8] ON [l7].[Id] = [l8].[Id]
                WHERE [l7].[OneToOne_Required_PK_Date] IS NOT NULL AND [l7].[Level1_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse2Id] IS NOT NULL
            ) AS [t3] ON [l6].[Id] = [t3].[Id]
            WHERE [l6].[Level2_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse3Id] IS NOT NULL
        ) AS [t2] ON [l5].[Id] = [t2].[Id]
        WHERE [l5].[Level3_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [t1] ON CASE
        WHEN [t0].[Level2_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse3Id] IS NOT NULL AND [t0].[PeriodEnd] IS NOT NULL AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
    END = [t1].[Level3_Optional_Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t4] ON [l].[Id] = [t4].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t4].[Id1], [t4].[Id2], [t4].[Id], [t4].[Id00], [t4].[Id000], [t4].[Id0], [t4].[Id01], [t4].[Id001]");
    }

    public override async Task Including_reference_navigation_and_projecting_collection_navigation(bool async)
    {
        await base.Including_reference_navigation_and_projecting_collection_navigation(async);

        AssertSql(
            @"SELECT [l].[Id], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t].[Id0], [t0].[Id0], [t0].[Id00], [t2].[Id], [t2].[OneToOne_Required_PK_Date], [t2].[Level1_Optional_Id], [t2].[Level1_Required_Id], [t2].[Level2_Name], [t2].[OneToMany_Optional_Inverse2Id], [t2].[OneToMany_Required_Inverse2Id], [t2].[OneToOne_Optional_PK_Inverse2Id], [t2].[PeriodEnd], [t2].[PeriodStart], [t2].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Level1_Required_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t0].[Level2_Optional_Id]
LEFT JOIN (
    SELECT [l5].[Id], [l5].[OneToOne_Required_PK_Date], [l5].[Level1_Optional_Id], [l5].[Level1_Required_Id], [l5].[Level2_Name], [l5].[OneToMany_Optional_Inverse2Id], [l5].[OneToMany_Required_Inverse2Id], [l5].[OneToOne_Optional_PK_Inverse2Id], [l5].[PeriodEnd], [l5].[PeriodStart], [l6].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6] ON [l5].[Id] = [l6].[Id]
    WHERE [l5].[OneToOne_Required_PK_Date] IS NOT NULL AND [l5].[Level1_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t2] ON [l].[Id] = [t2].[OneToMany_Required_Inverse2Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0], [t0].[Id00], [t2].[Id]");
    }

    public override async Task LeftJoin_with_Any_on_outer_source_and_projecting_collection_from_inner(bool async)
    {
        await base.LeftJoin_with_Any_on_outer_source_and_projecting_collection_from_inner(async);

        AssertSql(
            @"SELECT CASE
    WHEN [t0].[OneToOne_Required_PK_Date] IS NULL OR [t0].[Level1_Required_Id] IS NULL OR [t0].[OneToMany_Required_Inverse2Id] IS NULL OR CASE
        WHEN [t0].[PeriodEnd0] IS NOT NULL AND [t0].[PeriodStart0] IS NOT NULL THEN [t0].[PeriodEnd0]
    END IS NULL OR CASE
        WHEN [t0].[PeriodEnd0] IS NOT NULL AND [t0].[PeriodStart0] IS NOT NULL THEN [t0].[PeriodStart0]
    END IS NULL THEN 0
    WHEN [t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t0].[PeriodEnd0] IS NOT NULL AND [t0].[PeriodStart0] IS NOT NULL THEN [t0].[Id0]
END, [l].[Id], [t0].[Id], [t0].[Id0], [t0].[Id00], [t1].[Id], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Id0], [t1].[Id00]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [t].[Id] AS [Id0], [t].[OneToOne_Required_PK_Date], [t].[Level1_Required_Id], [t].[OneToMany_Required_Inverse2Id], [t].[PeriodEnd] AS [PeriodEnd0], [t].[PeriodStart] AS [PeriodStart0], [t].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Required_Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[PeriodEnd], [l1].[PeriodStart], [l2].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2] ON [l1].[Id] = [l2].[Id]
        WHERE [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t] ON [l0].[Id] = CASE
        WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t].[Id]
    END
    WHERE [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND CASE
        WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[PeriodEnd]
    END IS NOT NULL AND CASE
        WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[PeriodStart]
    END IS NOT NULL
) AS [t0] ON [l].[Id] = [t0].[Level1_Required_Id]
LEFT JOIN (
    SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[PeriodEnd], [l3].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
    INNER JOIN (
        SELECT [l4].[Id], [l5].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5] ON [l4].[Id] = [l5].[Id]
        WHERE [l4].[OneToOne_Required_PK_Date] IS NOT NULL AND [l4].[Level1_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t2] ON [l3].[Id] = [t2].[Id]
    WHERE [l3].[Level2_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t1] ON CASE
    WHEN [t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t0].[PeriodEnd0] IS NOT NULL AND [t0].[PeriodStart0] IS NOT NULL THEN [t0].[Id0]
END = [t1].[OneToMany_Required_Inverse3Id]
WHERE [l].[Name] IN (N'L1 01', N'L1 02')
ORDER BY [l].[Id], [t0].[Id], [t0].[Id0], [t0].[Id00], [t1].[Id], [t1].[Id0]");
    }

    public override async Task Lift_projection_mapping_when_pushing_down_subquery(bool async)
    {
        await base.Lift_projection_mapping_when_pushing_down_subquery(async);

        AssertSql(
            @"@__p_0='25'

SELECT [t].[Id], [t0].[Id0], [t0].[Id1], [t2].[Id], [t2].[Id0], [t2].[Id1], [t0].[Id], [t0].[c]
FROM (
    SELECT TOP(@__p_0) [l].[Id]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
) AS [t]
LEFT JOIN (
    SELECT [t1].[Id], [t1].[c], [t1].[Id0], [t1].[Id1], [t1].[OneToMany_Required_Inverse2Id]
    FROM (
        SELECT CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [Id], 1 AS [c], [l0].[Id] AS [Id0], [l1].[Id] AS [Id1], [l0].[OneToMany_Required_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Required_Inverse2Id] ORDER BY [l0].[Id], [l1].[Id]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1]
    WHERE [t1].[row] <= 1
) AS [t0] ON [t].[Id] = [t0].[OneToMany_Required_Inverse2Id]
LEFT JOIN (
    SELECT CASE
        WHEN [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l2].[Id]
    END AS [Id], [l2].[Id] AS [Id0], [l3].[Id] AS [Id1], [l2].[OneToMany_Required_Inverse2Id]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3] ON [l2].[Id] = [l3].[Id]
    WHERE [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t2] ON [t].[Id] = [t2].[OneToMany_Required_Inverse2Id]
ORDER BY [t].[Id], [t0].[Id0], [t0].[Id1], [t2].[Id0]");
    }

    public override async Task Multiple_complex_includes(bool async)
    {
        await base.Multiple_complex_includes(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00], [t2].[Id], [t2].[OneToOne_Required_PK_Date], [t2].[Level1_Optional_Id], [t2].[Level1_Required_Id], [t2].[Level2_Name], [t2].[OneToMany_Optional_Inverse2Id], [t2].[OneToMany_Required_Inverse2Id], [t2].[OneToOne_Optional_PK_Inverse2Id], [t2].[PeriodEnd], [t2].[PeriodStart], [t2].[Id0], [t2].[Level2_Optional_Id], [t2].[Level2_Required_Id], [t2].[Level3_Name], [t2].[OneToMany_Optional_Inverse3Id], [t2].[OneToMany_Required_Inverse3Id], [t2].[OneToOne_Optional_PK_Inverse3Id], [t2].[PeriodEnd0], [t2].[PeriodStart0], [t2].[Id1], [t2].[Id00], [t2].[Id000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToMany_Optional_Inverse3Id]
LEFT JOIN (
    SELECT [l5].[Id], [l5].[OneToOne_Required_PK_Date], [l5].[Level1_Optional_Id], [l5].[Level1_Required_Id], [l5].[Level2_Name], [l5].[OneToMany_Optional_Inverse2Id], [l5].[OneToMany_Required_Inverse2Id], [l5].[OneToOne_Optional_PK_Inverse2Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t3].[Id] AS [Id0], [t3].[Level2_Optional_Id], [t3].[Level2_Required_Id], [t3].[Level3_Name], [t3].[OneToMany_Optional_Inverse3Id], [t3].[OneToMany_Required_Inverse3Id], [t3].[OneToOne_Optional_PK_Inverse3Id], [t3].[PeriodEnd] AS [PeriodEnd0], [t3].[PeriodStart] AS [PeriodStart0], [l6].[Id] AS [Id1], [t3].[Id0] AS [Id00], [t3].[Id00] AS [Id000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6] ON [l5].[Id] = [l6].[Id]
    LEFT JOIN (
        SELECT [l7].[Id], [l7].[Level2_Optional_Id], [l7].[Level2_Required_Id], [l7].[Level3_Name], [l7].[OneToMany_Optional_Inverse3Id], [l7].[OneToMany_Required_Inverse3Id], [l7].[OneToOne_Optional_PK_Inverse3Id], [l7].[PeriodEnd], [l7].[PeriodStart], [t4].[Id] AS [Id0], [t4].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
        INNER JOIN (
            SELECT [l8].[Id], [l9].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l9] ON [l8].[Id] = [l9].[Id]
            WHERE [l8].[OneToOne_Required_PK_Date] IS NOT NULL AND [l8].[Level1_Required_Id] IS NOT NULL AND [l8].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t4] ON [l7].[Id] = [t4].[Id]
        WHERE [l7].[Level2_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t3] ON CASE
        WHEN [l5].[OneToOne_Required_PK_Date] IS NOT NULL AND [l5].[Level1_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l5].[Id]
    END = [t3].[Level2_Optional_Id]
    WHERE [l5].[OneToOne_Required_PK_Date] IS NOT NULL AND [l5].[Level1_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t2] ON [l].[Id] = [t2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0], [t0].[Id00], [t2].[Id], [t2].[Id1], [t2].[Id0], [t2].[Id00]");
    }

    public override async Task Multiple_complex_include_select(bool async)
    {
        await base.Multiple_complex_include_select(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00], [t2].[Id], [t2].[OneToOne_Required_PK_Date], [t2].[Level1_Optional_Id], [t2].[Level1_Required_Id], [t2].[Level2_Name], [t2].[OneToMany_Optional_Inverse2Id], [t2].[OneToMany_Required_Inverse2Id], [t2].[OneToOne_Optional_PK_Inverse2Id], [t2].[PeriodEnd], [t2].[PeriodStart], [t2].[Id0], [t2].[Level2_Optional_Id], [t2].[Level2_Required_Id], [t2].[Level3_Name], [t2].[OneToMany_Optional_Inverse3Id], [t2].[OneToMany_Required_Inverse3Id], [t2].[OneToOne_Optional_PK_Inverse3Id], [t2].[PeriodEnd0], [t2].[PeriodStart0], [t2].[Id1], [t2].[Id00], [t2].[Id000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToMany_Optional_Inverse3Id]
LEFT JOIN (
    SELECT [l5].[Id], [l5].[OneToOne_Required_PK_Date], [l5].[Level1_Optional_Id], [l5].[Level1_Required_Id], [l5].[Level2_Name], [l5].[OneToMany_Optional_Inverse2Id], [l5].[OneToMany_Required_Inverse2Id], [l5].[OneToOne_Optional_PK_Inverse2Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t3].[Id] AS [Id0], [t3].[Level2_Optional_Id], [t3].[Level2_Required_Id], [t3].[Level3_Name], [t3].[OneToMany_Optional_Inverse3Id], [t3].[OneToMany_Required_Inverse3Id], [t3].[OneToOne_Optional_PK_Inverse3Id], [t3].[PeriodEnd] AS [PeriodEnd0], [t3].[PeriodStart] AS [PeriodStart0], [l6].[Id] AS [Id1], [t3].[Id0] AS [Id00], [t3].[Id00] AS [Id000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6] ON [l5].[Id] = [l6].[Id]
    LEFT JOIN (
        SELECT [l7].[Id], [l7].[Level2_Optional_Id], [l7].[Level2_Required_Id], [l7].[Level3_Name], [l7].[OneToMany_Optional_Inverse3Id], [l7].[OneToMany_Required_Inverse3Id], [l7].[OneToOne_Optional_PK_Inverse3Id], [l7].[PeriodEnd], [l7].[PeriodStart], [t4].[Id] AS [Id0], [t4].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
        INNER JOIN (
            SELECT [l8].[Id], [l9].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l9] ON [l8].[Id] = [l9].[Id]
            WHERE [l8].[OneToOne_Required_PK_Date] IS NOT NULL AND [l8].[Level1_Required_Id] IS NOT NULL AND [l8].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t4] ON [l7].[Id] = [t4].[Id]
        WHERE [l7].[Level2_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t3] ON CASE
        WHEN [l5].[OneToOne_Required_PK_Date] IS NOT NULL AND [l5].[Level1_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l5].[Id]
    END = [t3].[Level2_Optional_Id]
    WHERE [l5].[OneToOne_Required_PK_Date] IS NOT NULL AND [l5].[Level1_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t2] ON [l].[Id] = [t2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0], [t0].[Id00], [t2].[Id], [t2].[Id1], [t2].[Id0], [t2].[Id00]");
    }

    public override async Task Multiple_include_with_multiple_optional_navigations(bool async)
    {
        await base.Multiple_include_with_multiple_optional_navigations(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t0].[Id], [t0].[Id0], [t0].[Id00], [t2].[Id], [t2].[Id0], [t2].[Id00], [t4].[Id], [t4].[Id0], [t5].[Id], [t5].[Id0], [t5].[Id00], [t7].[Id], [t7].[Level2_Optional_Id], [t7].[Level2_Required_Id], [t7].[Level3_Name], [t7].[OneToMany_Optional_Inverse3Id], [t7].[OneToMany_Required_Inverse3Id], [t7].[OneToOne_Optional_PK_Inverse3Id], [t7].[PeriodEnd], [t7].[PeriodStart], [t7].[Id0], [t7].[Id00], [t2].[Level2_Optional_Id], [t2].[Level2_Required_Id], [t2].[Level3_Name], [t2].[OneToMany_Optional_Inverse3Id], [t2].[OneToMany_Required_Inverse3Id], [t2].[OneToOne_Optional_PK_Inverse3Id], [t2].[PeriodEnd], [t2].[PeriodStart], [t4].[OneToOne_Required_PK_Date], [t4].[Level1_Optional_Id], [t4].[Level1_Required_Id], [t4].[Level2_Name], [t4].[OneToMany_Optional_Inverse2Id], [t4].[OneToMany_Required_Inverse2Id], [t4].[OneToOne_Optional_PK_Inverse2Id], [t4].[PeriodEnd], [t4].[PeriodStart], [t5].[Level2_Optional_Id], [t5].[Level2_Required_Id], [t5].[Level3_Name], [t5].[OneToMany_Optional_Inverse3Id], [t5].[OneToMany_Required_Inverse3Id], [t5].[OneToOne_Optional_PK_Inverse3Id], [t5].[PeriodEnd], [t5].[PeriodStart]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Level1_Required_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level3_Name], [l2].[OneToOne_Optional_PK_Inverse3Id], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToOne_Optional_PK_Inverse3Id]
LEFT JOIN (
    SELECT [l5].[Id], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Level3_Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
    INNER JOIN (
        SELECT [l6].[Id], [l7].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7] ON [l6].[Id] = [l7].[Id]
        WHERE [l6].[OneToOne_Required_PK_Date] IS NOT NULL AND [l6].[Level1_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t3] ON [l5].[Id] = [t3].[Id]
    WHERE [l5].[Level2_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t2] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t2].[Level2_Optional_Id]
LEFT JOIN (
    SELECT [l8].[Id], [l8].[OneToOne_Required_PK_Date], [l8].[Level1_Optional_Id], [l8].[Level1_Required_Id], [l8].[Level2_Name], [l8].[OneToMany_Optional_Inverse2Id], [l8].[OneToMany_Required_Inverse2Id], [l8].[OneToOne_Optional_PK_Inverse2Id], [l8].[PeriodEnd], [l8].[PeriodStart], [l9].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l9] ON [l8].[Id] = [l9].[Id]
    WHERE [l8].[OneToOne_Required_PK_Date] IS NOT NULL AND [l8].[Level1_Required_Id] IS NOT NULL AND [l8].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t4] ON [l].[Id] = [t4].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l10].[Id], [l10].[Level2_Optional_Id], [l10].[Level2_Required_Id], [l10].[Level3_Name], [l10].[OneToMany_Optional_Inverse3Id], [l10].[OneToMany_Required_Inverse3Id], [l10].[OneToOne_Optional_PK_Inverse3Id], [l10].[PeriodEnd], [l10].[PeriodStart], [t6].[Id] AS [Id0], [t6].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l10]
    INNER JOIN (
        SELECT [l11].[Id], [l12].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l11]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l12] ON [l11].[Id] = [l12].[Id]
        WHERE [l11].[OneToOne_Required_PK_Date] IS NOT NULL AND [l11].[Level1_Required_Id] IS NOT NULL AND [l11].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t6] ON [l10].[Id] = [t6].[Id]
    WHERE [l10].[Level2_Required_Id] IS NOT NULL AND [l10].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t5] ON CASE
    WHEN [t4].[OneToOne_Required_PK_Date] IS NOT NULL AND [t4].[Level1_Required_Id] IS NOT NULL AND [t4].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t4].[PeriodEnd] IS NOT NULL AND [t4].[PeriodStart] IS NOT NULL THEN [t4].[Id]
END = [t5].[Level2_Optional_Id]
LEFT JOIN (
    SELECT [l13].[Id], [l13].[Level2_Optional_Id], [l13].[Level2_Required_Id], [l13].[Level3_Name], [l13].[OneToMany_Optional_Inverse3Id], [l13].[OneToMany_Required_Inverse3Id], [l13].[OneToOne_Optional_PK_Inverse3Id], [l13].[PeriodEnd], [l13].[PeriodStart], [t8].[Id] AS [Id0], [t8].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l13]
    INNER JOIN (
        SELECT [l14].[Id], [l15].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l14]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l15] ON [l14].[Id] = [l15].[Id]
        WHERE [l14].[OneToOne_Required_PK_Date] IS NOT NULL AND [l14].[Level1_Required_Id] IS NOT NULL AND [l14].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t8] ON [l13].[Id] = [t8].[Id]
    WHERE [l13].[Level2_Required_Id] IS NOT NULL AND [l13].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t7] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t7].[OneToMany_Optional_Inverse3Id]
WHERE [t0].[Level3_Name] <> N'Foo' OR [t0].[Level3_Name] IS NULL
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0], [t0].[Id00], [t2].[Id], [t2].[Id0], [t2].[Id00], [t4].[Id], [t4].[Id0], [t5].[Id], [t5].[Id0], [t5].[Id00], [t7].[Id], [t7].[Id0]");
    }

    public override async Task Multiple_optional_navigation_with_Include(bool async)
    {
        await base.Multiple_optional_navigation_with_Include(async);

        AssertSql(
            @"SELECT [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [l].[Id], [t].[Id], [t].[Id0], [t0].[Id0], [t0].[Id00], [t2].[Id], [t2].[Level3_Optional_Id], [t2].[Level3_Required_Id], [t2].[Level4_Name], [t2].[OneToMany_Optional_Inverse4Id], [t2].[OneToMany_Required_Inverse4Id], [t2].[OneToOne_Optional_PK_Inverse4Id], [t2].[PeriodEnd], [t2].[PeriodStart], [t2].[Id0], [t2].[Id00], [t2].[Id000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToOne_Optional_PK_Inverse3Id]
LEFT JOIN (
    SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00], [t3].[Id00] AS [Id000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
    INNER JOIN (
        SELECT [l6].[Id], [t4].[Id] AS [Id0], [t4].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
        INNER JOIN (
            SELECT [l7].[Id], [l8].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8] ON [l7].[Id] = [l8].[Id]
            WHERE [l7].[OneToOne_Required_PK_Date] IS NOT NULL AND [l7].[Level1_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t4] ON [l6].[Id] = [t4].[Id]
        WHERE [l6].[Level2_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t3] ON [l5].[Id] = [t3].[Id]
    WHERE [l5].[Level3_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse4Id] IS NOT NULL
) AS [t2] ON CASE
    WHEN [t0].[Level2_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse3Id] IS NOT NULL AND [t0].[PeriodEnd] IS NOT NULL AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
END = [t2].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0], [t0].[Id00], [t2].[Id], [t2].[Id0], [t2].[Id00]");
    }

    public override async Task Multiple_optional_navigation_with_string_based_Include(bool async)
    {
        await base.Multiple_optional_navigation_with_string_based_Include(async);

        AssertSql(
            @"SELECT [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [l].[Id], [t].[Id], [t].[Id0], [t0].[Id0], [t0].[Id00], [t2].[Id], [t2].[Level3_Optional_Id], [t2].[Level3_Required_Id], [t2].[Level4_Name], [t2].[OneToMany_Optional_Inverse4Id], [t2].[OneToMany_Required_Inverse4Id], [t2].[OneToOne_Optional_PK_Inverse4Id], [t2].[PeriodEnd], [t2].[PeriodStart], [t2].[Id0], [t2].[Id00], [t2].[Id000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToOne_Optional_PK_Inverse3Id]
LEFT JOIN (
    SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00], [t3].[Id00] AS [Id000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
    INNER JOIN (
        SELECT [l6].[Id], [t4].[Id] AS [Id0], [t4].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
        INNER JOIN (
            SELECT [l7].[Id], [l8].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8] ON [l7].[Id] = [l8].[Id]
            WHERE [l7].[OneToOne_Required_PK_Date] IS NOT NULL AND [l7].[Level1_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t4] ON [l6].[Id] = [t4].[Id]
        WHERE [l6].[Level2_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t3] ON [l5].[Id] = [t3].[Id]
    WHERE [l5].[Level3_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse4Id] IS NOT NULL
) AS [t2] ON CASE
    WHEN [t0].[Level2_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse3Id] IS NOT NULL AND [t0].[PeriodEnd] IS NOT NULL AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
END = [t2].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0], [t0].[Id00], [t2].[Id], [t2].[Id0], [t2].[Id00]");
    }

    public override async Task Multiple_SelectMany_navigation_property_followed_by_select_collection_navigation(bool async)
    {
        await base.Multiple_SelectMany_navigation_property_followed_by_select_collection_navigation(async);

        AssertSql(
            @"SELECT CASE
    WHEN [t0].[Level2_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [t0].[Id]
END, [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0], [t0].[Id00], [t2].[Id], [t2].[Level3_Optional_Id], [t2].[Level3_Required_Id], [t2].[Level4_Name], [t2].[OneToMany_Optional_Inverse4Id], [t2].[OneToMany_Required_Inverse4Id], [t2].[OneToOne_Optional_PK_Inverse4Id], [t2].[PeriodEnd], [t2].[PeriodStart], [t2].[Id0], [t2].[Id00], [t2].[Id000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Required_Id], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
INNER JOIN (
    SELECT [l2].[Id], [l2].[Level2_Required_Id], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToMany_Optional_Inverse3Id]
LEFT JOIN (
    SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00], [t3].[Id00] AS [Id000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
    INNER JOIN (
        SELECT [l6].[Id], [t4].[Id] AS [Id0], [t4].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
        INNER JOIN (
            SELECT [l7].[Id], [l8].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8] ON [l7].[Id] = [l8].[Id]
            WHERE [l7].[OneToOne_Required_PK_Date] IS NOT NULL AND [l7].[Level1_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t4] ON [l6].[Id] = [t4].[Id]
        WHERE [l6].[Level2_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t3] ON [l5].[Id] = [t3].[Id]
    WHERE [l5].[Level3_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse4Id] IS NOT NULL
) AS [t2] ON CASE
    WHEN [t0].[Level2_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [t0].[Id]
END = [t2].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0], [t0].[Id00], [t2].[Id], [t2].[Id0], [t2].[Id00]");
    }

    public override async Task Multiple_SelectMany_with_Include(bool async)
    {
        await base.Multiple_SelectMany_with_Include(async);

        AssertSql(
            @"SELECT [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t2].[Id], [t2].[Level3_Optional_Id], [t2].[Level3_Required_Id], [t2].[Level4_Name], [t2].[OneToMany_Optional_Inverse4Id], [t2].[OneToMany_Required_Inverse4Id], [t2].[OneToOne_Optional_PK_Inverse4Id], [t2].[PeriodEnd], [t2].[PeriodStart], [l].[Id], [t].[Id], [t].[Id0], [t0].[Id0], [t0].[Id00], [t2].[Id0], [t2].[Id00], [t2].[Id000], [t5].[Id], [t5].[Level3_Optional_Id], [t5].[Level3_Required_Id], [t5].[Level4_Name], [t5].[OneToMany_Optional_Inverse4Id], [t5].[OneToMany_Required_Inverse4Id], [t5].[OneToOne_Optional_PK_Inverse4Id], [t5].[PeriodEnd], [t5].[PeriodStart], [t5].[Id0], [t5].[Id00], [t5].[Id000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Required_Id], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
INNER JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToMany_Optional_Inverse3Id]
LEFT JOIN (
    SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00], [t3].[Id00] AS [Id000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
    INNER JOIN (
        SELECT [l6].[Id], [t4].[Id] AS [Id0], [t4].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
        INNER JOIN (
            SELECT [l7].[Id], [l8].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8] ON [l7].[Id] = [l8].[Id]
            WHERE [l7].[OneToOne_Required_PK_Date] IS NOT NULL AND [l7].[Level1_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t4] ON [l6].[Id] = [t4].[Id]
        WHERE [l6].[Level2_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t3] ON [l5].[Id] = [t3].[Id]
    WHERE [l5].[Level3_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse4Id] IS NOT NULL
) AS [t2] ON CASE
    WHEN [t0].[Level2_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [t0].[Id]
END = [t2].[Level3_Required_Id]
LEFT JOIN (
    SELECT [l9].[Id], [l9].[Level3_Optional_Id], [l9].[Level3_Required_Id], [l9].[Level4_Name], [l9].[OneToMany_Optional_Inverse4Id], [l9].[OneToMany_Required_Inverse4Id], [l9].[OneToOne_Optional_PK_Inverse4Id], [l9].[PeriodEnd], [l9].[PeriodStart], [t6].[Id] AS [Id0], [t6].[Id0] AS [Id00], [t6].[Id00] AS [Id000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l9]
    INNER JOIN (
        SELECT [l10].[Id], [t7].[Id] AS [Id0], [t7].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l10]
        INNER JOIN (
            SELECT [l11].[Id], [l12].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l11]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l12] ON [l11].[Id] = [l12].[Id]
            WHERE [l11].[OneToOne_Required_PK_Date] IS NOT NULL AND [l11].[Level1_Required_Id] IS NOT NULL AND [l11].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t7] ON [l10].[Id] = [t7].[Id]
        WHERE [l10].[Level2_Required_Id] IS NOT NULL AND [l10].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t6] ON [l9].[Id] = [t6].[Id]
    WHERE [l9].[Level3_Required_Id] IS NOT NULL AND [l9].[OneToMany_Required_Inverse4Id] IS NOT NULL
) AS [t5] ON CASE
    WHEN [t0].[Level2_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [t0].[Id]
END = [t5].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0], [t0].[Id00], [t2].[Id], [t2].[Id0], [t2].[Id00], [t2].[Id000], [t5].[Id], [t5].[Id0], [t5].[Id00]");
    }

    public override async Task Multi_level_include_correct_PK_is_chosen_as_the_join_predicate_for_queries_that_join_same_table_multiple_times(bool async)
    {
        await base.Multi_level_include_correct_PK_is_chosen_as_the_join_predicate_for_queries_that_join_same_table_multiple_times(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t4].[Id], [t4].[OneToOne_Required_PK_Date], [t4].[Level1_Optional_Id], [t4].[Level1_Required_Id], [t4].[Level2_Name], [t4].[OneToMany_Optional_Inverse2Id], [t4].[OneToMany_Required_Inverse2Id], [t4].[OneToOne_Optional_PK_Inverse2Id], [t4].[PeriodEnd], [t4].[PeriodStart], [t4].[Id0], [t4].[Id1], [t4].[Level2_Optional_Id], [t4].[Level2_Required_Id], [t4].[Level3_Name], [t4].[OneToMany_Optional_Inverse3Id], [t4].[OneToMany_Required_Inverse3Id], [t4].[OneToOne_Optional_PK_Inverse3Id], [t4].[PeriodEnd0], [t4].[PeriodStart0], [t4].[Id00], [t4].[OneToOne_Required_PK_Date0], [t4].[Level1_Optional_Id0], [t4].[Level1_Required_Id0], [t4].[Level2_Name0], [t4].[OneToMany_Optional_Inverse2Id0], [t4].[OneToMany_Required_Inverse2Id0], [t4].[OneToOne_Optional_PK_Inverse2Id0], [t4].[PeriodEnd00], [t4].[PeriodStart00], [t4].[Id10], [t4].[Id000], [t4].[Id01], [t4].[Id2], [t4].[Level2_Optional_Id0], [t4].[Level2_Required_Id0], [t4].[Level3_Name0], [t4].[OneToMany_Optional_Inverse3Id0], [t4].[OneToMany_Required_Inverse3Id0], [t4].[OneToOne_Optional_PK_Inverse3Id0], [t4].[PeriodEnd1], [t4].[PeriodStart1], [t4].[Id02], [t4].[Id0000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], [t3].[Id] AS [Id1], [t3].[Level2_Optional_Id], [t3].[Level2_Required_Id], [t3].[Level3_Name], [t3].[OneToMany_Optional_Inverse3Id], [t3].[OneToMany_Required_Inverse3Id], [t3].[OneToOne_Optional_PK_Inverse3Id], [t3].[PeriodEnd] AS [PeriodEnd0], [t3].[PeriodStart] AS [PeriodStart0], [t3].[Id0] AS [Id00], [t3].[OneToOne_Required_PK_Date] AS [OneToOne_Required_PK_Date0], [t3].[Level1_Optional_Id] AS [Level1_Optional_Id0], [t3].[Level1_Required_Id] AS [Level1_Required_Id0], [t3].[Level2_Name] AS [Level2_Name0], [t3].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id0], [t3].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id0], [t3].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id0], [t3].[PeriodEnd0] AS [PeriodEnd00], [t3].[PeriodStart0] AS [PeriodStart00], [t3].[Id1] AS [Id10], [t3].[Id00] AS [Id000], [t3].[Id01], [t3].[Id2], [t3].[Level2_Optional_Id0], [t3].[Level2_Required_Id0], [t3].[Level3_Name0], [t3].[OneToMany_Optional_Inverse3Id0], [t3].[OneToMany_Required_Inverse3Id0], [t3].[OneToOne_Optional_PK_Inverse3Id0], [t3].[PeriodEnd1], [t3].[PeriodStart1], [t3].[Id02], [t3].[Id000] AS [Id0000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t0].[Id] AS [Id0], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd] AS [PeriodEnd0], [t0].[PeriodStart] AS [PeriodStart0], [t].[Id] AS [Id1], [t].[Id0] AS [Id00], [t0].[Id0] AS [Id01], [t1].[Id] AS [Id2], [t1].[Level2_Optional_Id] AS [Level2_Optional_Id0], [t1].[Level2_Required_Id] AS [Level2_Required_Id0], [t1].[Level3_Name] AS [Level3_Name0], [t1].[OneToMany_Optional_Inverse3Id] AS [OneToMany_Optional_Inverse3Id0], [t1].[OneToMany_Required_Inverse3Id] AS [OneToMany_Required_Inverse3Id0], [t1].[OneToOne_Optional_PK_Inverse3Id] AS [OneToOne_Optional_PK_Inverse3Id0], [t1].[PeriodEnd] AS [PeriodEnd1], [t1].[PeriodStart] AS [PeriodStart1], [t1].[Id0] AS [Id02], [t1].[Id00] AS [Id000]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t] ON [l2].[Id] = [t].[Id]
        INNER JOIN (
            SELECT [l5].[Id], [l5].[OneToOne_Required_PK_Date], [l5].[Level1_Optional_Id], [l5].[Level1_Required_Id], [l5].[Level2_Name], [l5].[OneToMany_Optional_Inverse2Id], [l5].[OneToMany_Required_Inverse2Id], [l5].[OneToOne_Optional_PK_Inverse2Id], [l5].[PeriodEnd], [l5].[PeriodStart], [l6].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6] ON [l5].[Id] = [l6].[Id]
            WHERE [l5].[OneToOne_Required_PK_Date] IS NOT NULL AND [l5].[Level1_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t0] ON [l2].[OneToMany_Required_Inverse3Id] = CASE
            WHEN [t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t0].[Id]
        END
        LEFT JOIN (
            SELECT [l7].[Id], [l7].[Level2_Optional_Id], [l7].[Level2_Required_Id], [l7].[Level3_Name], [l7].[OneToMany_Optional_Inverse3Id], [l7].[OneToMany_Required_Inverse3Id], [l7].[OneToOne_Optional_PK_Inverse3Id], [l7].[PeriodEnd], [l7].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
            INNER JOIN (
                SELECT [l8].[Id], [l9].[Id] AS [Id0]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8]
                INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l9] ON [l8].[Id] = [l9].[Id]
                WHERE [l8].[OneToOne_Required_PK_Date] IS NOT NULL AND [l8].[Level1_Required_Id] IS NOT NULL AND [l8].[OneToMany_Required_Inverse2Id] IS NOT NULL
            ) AS [t2] ON [l7].[Id] = [t2].[Id]
            WHERE [l7].[Level2_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse3Id] IS NOT NULL
        ) AS [t1] ON CASE
            WHEN [t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t0].[Id]
        END = [t1].[OneToMany_Optional_Inverse3Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t3] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [t3].[OneToMany_Optional_Inverse3Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t4] ON [l].[Id] = [t4].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t4].[Id], [t4].[Id0], [t4].[Id1], [t4].[Id10], [t4].[Id000], [t4].[Id00], [t4].[Id01], [t4].[Id2], [t4].[Id02]");
    }

    public override async Task Multi_level_include_one_to_many_optional_and_one_to_many_optional_produces_valid_sql(bool async)
    {
        await base.Multi_level_include_one_to_many_optional_and_one_to_many_optional_produces_valid_sql(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t1].[Id], [t1].[OneToOne_Required_PK_Date], [t1].[Level1_Optional_Id], [t1].[Level1_Required_Id], [t1].[Level2_Name], [t1].[OneToMany_Optional_Inverse2Id], [t1].[OneToMany_Required_Inverse2Id], [t1].[OneToOne_Optional_PK_Inverse2Id], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Id0], [t1].[Id1], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd0], [t1].[PeriodStart0], [t1].[Id00], [t1].[Id000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], [t0].[Id] AS [Id1], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd] AS [PeriodEnd0], [t0].[PeriodStart] AS [PeriodStart0], [t0].[Id0] AS [Id00], [t0].[Id00] AS [Id000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t].[Id] AS [Id0], [t].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t] ON [l2].[Id] = [t].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t0] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [t0].[OneToMany_Optional_Inverse3Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t1] ON [l].[Id] = [t1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t1].[Id], [t1].[Id0], [t1].[Id1], [t1].[Id00]");
    }

    public override async Task Null_check_in_anonymous_type_projection_should_not_be_removed(bool async)
    {
        await base.Null_check_in_anonymous_type_projection_should_not_be_removed(async);

        AssertSql(
            @"SELECT [l].[Id], [t1].[c], [t1].[Level3_Name], [t1].[Id], [t1].[Id0], [t1].[Id1], [t1].[Id00], [t1].[Id000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT CASE
        WHEN [t0].[Level2_Required_Id] IS NULL OR [t0].[OneToMany_Required_Inverse3Id] IS NULL OR CASE
            WHEN [t0].[PeriodEnd] IS NOT NULL AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[PeriodEnd]
        END IS NULL OR CASE
            WHEN [t0].[PeriodEnd] IS NOT NULL AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[PeriodStart]
        END IS NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [c], [t0].[Level3_Name], [l0].[Id], [l1].[Id] AS [Id0], [t0].[Id] AS [Id1], [t0].[Id0] AS [Id00], [t0].[Id00] AS [Id000], [l0].[OneToMany_Optional_Inverse2Id]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Required_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t].[Id] AS [Id0], [t].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t] ON [l2].[Id] = [t].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t0] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [t0].[Level2_Required_Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t1] ON [l].[Id] = [t1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t1].[Id], [t1].[Id0], [t1].[Id1], [t1].[Id00]");
    }

    public override async Task Null_check_in_Dto_projection_should_not_be_removed(bool async)
    {
        await base.Null_check_in_Dto_projection_should_not_be_removed(async);

        AssertSql(
            @"SELECT [l].[Id], [t1].[c], [t1].[Level3_Name], [t1].[Id], [t1].[Id0], [t1].[Id1], [t1].[Id00], [t1].[Id000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT CASE
        WHEN [t0].[Level2_Required_Id] IS NULL OR [t0].[OneToMany_Required_Inverse3Id] IS NULL OR CASE
            WHEN [t0].[PeriodEnd] IS NOT NULL AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[PeriodEnd]
        END IS NULL OR CASE
            WHEN [t0].[PeriodEnd] IS NOT NULL AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[PeriodStart]
        END IS NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [c], [t0].[Level3_Name], [l0].[Id], [l1].[Id] AS [Id0], [t0].[Id] AS [Id1], [t0].[Id0] AS [Id00], [t0].[Id00] AS [Id000], [l0].[OneToMany_Optional_Inverse2Id]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Required_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t].[Id] AS [Id0], [t].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t] ON [l2].[Id] = [t].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t0] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [t0].[Level2_Required_Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t1] ON [l].[Id] = [t1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t1].[Id], [t1].[Id0], [t1].[Id1], [t1].[Id00]");
    }

    public override async Task Optional_navigation_with_Include_and_order(bool async)
    {
        await base.Optional_navigation_with_Include_and_order(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [l].[Id], [t].[Id0], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY [t].[Level2_Name], [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0]");
    }

    public override async Task Optional_navigation_with_Include_ThenInclude(bool async)
    {
        await base.Optional_navigation_with_Include_ThenInclude(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [l].[Id], [t].[Id0], [t3].[Id], [t3].[Level2_Optional_Id], [t3].[Level2_Required_Id], [t3].[Level3_Name], [t3].[OneToMany_Optional_Inverse3Id], [t3].[OneToMany_Required_Inverse3Id], [t3].[OneToOne_Optional_PK_Inverse3Id], [t3].[PeriodEnd], [t3].[PeriodStart], [t3].[Id0], [t3].[Level3_Optional_Id], [t3].[Level3_Required_Id], [t3].[Level4_Name], [t3].[OneToMany_Optional_Inverse4Id], [t3].[OneToMany_Required_Inverse4Id], [t3].[OneToOne_Optional_PK_Inverse4Id], [t3].[PeriodEnd0], [t3].[PeriodStart0], [t3].[Id1], [t3].[Id00], [t3].[Id01], [t3].[Id000], [t3].[Id0000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Level3_Optional_Id], [t1].[Level3_Required_Id], [t1].[Level4_Name], [t1].[OneToMany_Optional_Inverse4Id], [t1].[OneToMany_Required_Inverse4Id], [t1].[OneToOne_Optional_PK_Inverse4Id], [t1].[PeriodEnd] AS [PeriodEnd0], [t1].[PeriodStart] AS [PeriodStart0], [t0].[Id] AS [Id1], [t0].[Id0] AS [Id00], [t1].[Id0] AS [Id01], [t1].[Id00] AS [Id000], [t1].[Id000] AS [Id0000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t0] ON [l2].[Id] = [t0].[Id]
    LEFT JOIN (
        SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00], [t2].[Id00] AS [Id000]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
        INNER JOIN (
            SELECT [l6].[Id], [t4].[Id] AS [Id0], [t4].[Id0] AS [Id00]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
            INNER JOIN (
                SELECT [l7].[Id], [l8].[Id] AS [Id0]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
                INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8] ON [l7].[Id] = [l8].[Id]
                WHERE [l7].[OneToOne_Required_PK_Date] IS NOT NULL AND [l7].[Level1_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse2Id] IS NOT NULL
            ) AS [t4] ON [l6].[Id] = [t4].[Id]
            WHERE [l6].[Level2_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse3Id] IS NOT NULL
        ) AS [t2] ON [l5].[Id] = [t2].[Id]
        WHERE [l5].[Level3_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [t1] ON CASE
        WHEN [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l2].[Id]
    END = [t1].[Level3_Optional_Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t3] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t3].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t3].[Id], [t3].[Id1], [t3].[Id00], [t3].[Id0], [t3].[Id01], [t3].[Id000]");
    }

    public override async Task Optional_navigation_with_order_by_and_Include(bool async)
    {
        await base.Optional_navigation_with_order_by_and_Include(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [l].[Id], [t].[Id0], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY [t].[Level2_Name], [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0]");
    }

    public override async Task Orderby_SelectMany_with_Include1(bool async)
    {
        await base.Orderby_SelectMany_with_Include1(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [l].[Id], [t].[Id0], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0]");
    }

    public override async Task Projecting_collection_with_FirstOrDefault(bool async)
    {
        await base.Projecting_collection_with_FirstOrDefault(async);

        AssertSql(
            @"SELECT [t].[Id], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0]
FROM (
    SELECT TOP(1) [l].[Id]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    WHERE [l].[Id] = 1
) AS [t]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [t].[Id], [t0].[Id]");
    }

    public override async Task Project_collection_and_include(bool async)
    {
        await base.Project_collection_and_include(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[PeriodEnd], [l2].[PeriodStart], [l3].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3] ON [l2].[Id] = [l3].[Id]
    WHERE [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id]");
    }

    public override async Task Project_collection_and_root_entity(bool async)
    {
        await base.Project_collection_and_root_entity(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id]");
    }

    public override async Task Project_collection_navigation(bool async)
    {
        await base.Project_collection_navigation(async);

        AssertSql(
            @"SELECT [l].[Id], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id]");
    }

    public override async Task Project_collection_navigation_composed(bool async)
    {
        await base.Project_collection_navigation_composed(async);

        AssertSql(
            @"SELECT [l].[Id], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l0].[Level2_Name] <> N'Foo' OR [l0].[Level2_Name] IS NULL)
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
WHERE [l].[Id] < 3
ORDER BY [l].[Id], [t].[Id]");
    }

    public override async Task Project_collection_navigation_nested(bool async)
    {
        await base.Project_collection_navigation_nested(async);

        AssertSql(
            @"SELECT [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0]");
    }

    public override async Task Project_collection_navigation_nested_anonymous(bool async)
    {
        await base.Project_collection_navigation_nested_anonymous(async);

        AssertSql(
            @"SELECT [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0]");
    }

    public override async Task Project_collection_navigation_nested_with_take(bool async)
    {
        await base.Project_collection_navigation_nested_with_take(async);

        AssertSql(
            @"SELECT [l].[Id], [t].[Id], [t].[Id0], [t1].[Id], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Id0], [t1].[Id00]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00]
    FROM (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00], ROW_NUMBER() OVER(PARTITION BY [l2].[OneToMany_Optional_Inverse3Id] ORDER BY [l2].[Id], [t2].[Id], [t2].[Id0]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t2] ON [l2].[Id] = [t2].[Id]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t0]
    WHERE [t0].[row] <= 50
) AS [t1] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t1].[Id], [t1].[Id0]");
    }

    public override async Task Project_collection_navigation_using_ef_property(bool async)
    {
        await base.Project_collection_navigation_using_ef_property(async);

        AssertSql(
            @"SELECT [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0]");
    }

    public override async Task Project_navigation_and_collection(bool async)
    {
        await base.Project_navigation_and_collection(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [l].[Id], [t].[Id0], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[PeriodEnd] IS NOT NULL AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0]");
    }

    public override async Task SelectMany_navigation_property_followed_by_select_collection_navigation(bool async)
    {
        await base.SelectMany_navigation_property_followed_by_select_collection_navigation(async);

        AssertSql(
            @"SELECT CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t].[Id]
END, [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Required_Id], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0]");
    }

    public override async Task SelectMany_navigation_property_with_include_and_followed_by_select_collection_navigation(bool async)
    {
        await base.SelectMany_navigation_property_with_include_and_followed_by_select_collection_navigation(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [l].[Id], [t].[Id0], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00], [t2].[Id], [t2].[Level2_Optional_Id], [t2].[Level2_Required_Id], [t2].[Level3_Name], [t2].[OneToMany_Optional_Inverse3Id], [t2].[OneToMany_Required_Inverse3Id], [t2].[OneToOne_Optional_PK_Inverse3Id], [t2].[PeriodEnd], [t2].[PeriodStart], [t2].[Id0], [t2].[Id00]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToMany_Required_Inverse3Id]
LEFT JOIN (
    SELECT [l5].[Id], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Level3_Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
    INNER JOIN (
        SELECT [l6].[Id], [l7].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7] ON [l6].[Id] = [l7].[Id]
        WHERE [l6].[OneToOne_Required_PK_Date] IS NOT NULL AND [l6].[Level1_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t3] ON [l5].[Id] = [t3].[Id]
    WHERE [l5].[Level2_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t2] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t].[Id]
END = [t2].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0], [t0].[Id00], [t2].[Id], [t2].[Id0]");
    }

    public override async Task SelectMany_with_Include1(bool async)
    {
        await base.SelectMany_with_Include1(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [l].[Id], [t].[Id0], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0]");
    }

    public override async Task SelectMany_with_Include2(bool async)
    {
        await base.SelectMany_with_Include2(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t].[Id]
END = [t0].[Level2_Required_Id]");
    }

    public override async Task SelectMany_with_Include_and_order_by(bool async)
    {
        await base.SelectMany_with_Include_and_order_by(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [l].[Id], [t].[Id0], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY [t].[Level2_Name], [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0]");
    }

    public override async Task SelectMany_with_Include_ThenInclude(bool async)
    {
        await base.SelectMany_with_Include_ThenInclude(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [l].[Id], [t].[Id0], [t0].[Id0], [t0].[Id00], [t2].[Id], [t2].[Level3_Optional_Id], [t2].[Level3_Required_Id], [t2].[Level4_Name], [t2].[OneToMany_Optional_Inverse4Id], [t2].[OneToMany_Required_Inverse4Id], [t2].[OneToOne_Optional_PK_Inverse4Id], [t2].[PeriodEnd], [t2].[PeriodStart], [t2].[Id0], [t2].[Id00], [t2].[Id000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t].[Id]
END = [t0].[Level2_Required_Id]
LEFT JOIN (
    SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00], [t3].[Id00] AS [Id000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
    INNER JOIN (
        SELECT [l6].[Id], [t4].[Id] AS [Id0], [t4].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
        INNER JOIN (
            SELECT [l7].[Id], [l8].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8] ON [l7].[Id] = [l8].[Id]
            WHERE [l7].[OneToOne_Required_PK_Date] IS NOT NULL AND [l7].[Level1_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t4] ON [l6].[Id] = [t4].[Id]
        WHERE [l6].[Level2_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t3] ON [l5].[Id] = [t3].[Id]
    WHERE [l5].[Level3_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse4Id] IS NOT NULL
) AS [t2] ON CASE
    WHEN [t0].[Level2_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse3Id] IS NOT NULL AND [t0].[PeriodEnd] IS NOT NULL AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
END = [t2].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0], [t0].[Id00], [t2].[Id], [t2].[Id0], [t2].[Id00]");
    }

    public override async Task SelectMany_with_navigation_and_Distinct(bool async)
    {
        await base.SelectMany_with_navigation_and_Distinct(async);

        AssertSql(
            @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t].[Id], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
INNER JOIN (
    SELECT DISTINCT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[PeriodEnd], [l2].[PeriodStart], [l3].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3] ON [l2].[Id] = [l3].[Id]
    WHERE [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
WHERE [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL AND CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t].[PeriodEnd]
END IS NOT NULL AND CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t].[PeriodStart]
END IS NOT NULL
ORDER BY [l].[Id], [t].[Id], [t0].[Id]");
    }

    public override async Task SelectMany_with_order_by_and_Include(bool async)
    {
        await base.SelectMany_with_order_by_and_Include(async);

        AssertSql(
            @"SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [l].[Id], [t].[Id0], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON CASE
    WHEN [t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t].[Id]
END = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY [t].[Level2_Name], [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0]");
    }

    public override async Task Select_nav_prop_collection_one_to_many_required(bool async)
    {
        await base.Select_nav_prop_collection_one_to_many_required(async);

        AssertSql(
            @"SELECT [l].[Id], [t].[c], [t].[Id], [t].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END AS [c], [l0].[Id], [l1].[Id] AS [Id0], [l0].[OneToMany_Required_Inverse2Id]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToMany_Required_Inverse2Id]
ORDER BY [l].[Id], [t].[Id]");
    }

    public override async Task Select_subquery_single_nested_subquery(bool async)
    {
        await base.Select_subquery_single_nested_subquery(async);

        AssertSql(
            @"SELECT [l].[Id], [t0].[Id], [t0].[Id0], [t1].[Id], [t1].[Id0], [t1].[Id1], [t1].[Id00], [t0].[c]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[c], [t].[Id], [t].[Id0], [t].[OneToOne_Required_PK_Date], [t].[Level1_Required_Id], [t].[OneToMany_Required_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[OneToMany_Optional_Inverse2Id]
    FROM (
        SELECT 1 AS [c], [l0].[Id], [l1].[Id] AS [Id0], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l0].[OneToMany_Optional_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT CASE
        WHEN [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l2].[Id]
    END AS [Id], [l2].[Id] AS [Id0], [t2].[Id] AS [Id1], [t2].[Id0] AS [Id00], [l2].[OneToMany_Optional_Inverse3Id]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t2] ON [l2].[Id] = [t2].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t1] ON CASE
    WHEN [t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t0].[Id]
END = [t1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [t0].[Id], [t0].[Id0], [t1].[Id], [t1].[Id0], [t1].[Id1]");
    }

    public override async Task Select_subquery_single_nested_subquery2(bool async)
    {
        await base.Select_subquery_single_nested_subquery2(async);

        AssertSql(
            @"SELECT [l].[Id], [t5].[Id], [t5].[Id0], [t5].[Id1], [t5].[Id00], [t5].[Id000], [t5].[Id2], [t5].[Id01], [t5].[Id10], [t5].[Id001], [t5].[Id0000], [t5].[c]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l1].[Id] AS [Id0], [t1].[Id] AS [Id1], [t1].[Id0] AS [Id00], [t1].[Id00] AS [Id000], [t2].[Id] AS [Id2], [t2].[Id0] AS [Id01], [t2].[Id1] AS [Id10], [t2].[Id00] AS [Id001], [t2].[Id000] AS [Id0000], [t1].[c], CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END AS [c0], [l0].[OneToMany_Optional_Inverse2Id]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    LEFT JOIN (
        SELECT [t0].[c], [t0].[Id], [t0].[Id0], [t0].[Id00], [t0].[Level2_Required_Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[OneToMany_Optional_Inverse3Id]
        FROM (
            SELECT 1 AS [c], [l2].[Id], [t].[Id] AS [Id0], [t].[Id0] AS [Id00], [l2].[Level2_Required_Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [l2].[OneToMany_Optional_Inverse3Id], ROW_NUMBER() OVER(PARTITION BY [l2].[OneToMany_Optional_Inverse3Id] ORDER BY CASE
                WHEN [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l2].[Id]
            END) AS [row]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
            INNER JOIN (
                SELECT [l3].[Id], [l4].[Id] AS [Id0]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
                INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
                WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
            ) AS [t] ON [l2].[Id] = [t].[Id]
            WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
        ) AS [t0]
        WHERE [t0].[row] <= 1
    ) AS [t1] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [t1].[OneToMany_Optional_Inverse3Id]
    LEFT JOIN (
        SELECT CASE
            WHEN [l5].[Level3_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse4Id] IS NOT NULL THEN [l5].[Id]
        END AS [Id], [l5].[Id] AS [Id0], [t3].[Id] AS [Id1], [t3].[Id0] AS [Id00], [t3].[Id00] AS [Id000], [l5].[OneToMany_Optional_Inverse4Id]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
        INNER JOIN (
            SELECT [l6].[Id], [t4].[Id] AS [Id0], [t4].[Id0] AS [Id00]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
            INNER JOIN (
                SELECT [l7].[Id], [l8].[Id] AS [Id0]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
                INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8] ON [l7].[Id] = [l8].[Id]
                WHERE [l7].[OneToOne_Required_PK_Date] IS NOT NULL AND [l7].[Level1_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse2Id] IS NOT NULL
            ) AS [t4] ON [l6].[Id] = [t4].[Id]
            WHERE [l6].[Level2_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse3Id] IS NOT NULL
        ) AS [t3] ON [l5].[Id] = [t3].[Id]
        WHERE [l5].[Level3_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [t2] ON CASE
        WHEN [t1].[Level2_Required_Id] IS NOT NULL AND [t1].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [t1].[Id]
    END = [t2].[OneToMany_Optional_Inverse4Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t5] ON [l].[Id] = [t5].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t5].[c0], [t5].[Id], [t5].[Id0], [t5].[Id1], [t5].[Id00], [t5].[Id000], [t5].[Id2], [t5].[Id01], [t5].[Id10], [t5].[Id001]");
    }

    public override async Task Skip_on_grouping_element(bool async)
    {
        await base.Skip_on_grouping_element(async);

        AssertSql(
            @"SELECT [t].[Date], [t0].[Id], [t0].[Date], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart]
FROM (
    SELECT [l].[Date]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    GROUP BY [l].[Date]
) AS [t]
LEFT JOIN (
    SELECT [t1].[Id], [t1].[Date], [t1].[Name], [t1].[PeriodEnd], [t1].[PeriodStart]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name], [l0].[PeriodEnd], [l0].[PeriodStart], ROW_NUMBER() OVER(PARTITION BY [l0].[Date] ORDER BY [l0].[Name]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    ) AS [t1]
    WHERE 1 < [t1].[row]
) AS [t0] ON [t].[Date] = [t0].[Date]
ORDER BY [t].[Date], [t0].[Date], [t0].[Name]");
    }

    public override async Task Skip_Take_Distinct_on_grouping_element(bool async)
    {
        await base.Skip_Take_Distinct_on_grouping_element(async);

        AssertSql(
            @"SELECT [t].[Date], [t0].[Id], [t0].[Date], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart]
FROM (
    SELECT [l].[Date]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    GROUP BY [l].[Date]
) AS [t]
OUTER APPLY (
    SELECT DISTINCT [t1].[Id], [t1].[Date], [t1].[Name], [t1].[PeriodEnd], [t1].[PeriodStart]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name], [l0].[PeriodEnd], [l0].[PeriodStart]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        WHERE [t].[Date] = [l0].[Date]
        ORDER BY [l0].[Name]
        OFFSET 1 ROWS FETCH NEXT 5 ROWS ONLY
    ) AS [t1]
) AS [t0]
ORDER BY [t].[Date]");
    }

    public override async Task Skip_Take_on_grouping_element(bool async)
    {
        await base.Skip_Take_on_grouping_element(async);

        AssertSql(
            @"SELECT [t].[Date], [t0].[Id], [t0].[Date], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart]
FROM (
    SELECT [l].[Date]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    GROUP BY [l].[Date]
) AS [t]
LEFT JOIN (
    SELECT [t1].[Id], [t1].[Date], [t1].[Name], [t1].[PeriodEnd], [t1].[PeriodStart]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name], [l0].[PeriodEnd], [l0].[PeriodStart], ROW_NUMBER() OVER(PARTITION BY [l0].[Date] ORDER BY [l0].[Name]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    ) AS [t1]
    WHERE 1 < [t1].[row] AND [t1].[row] <= 6
) AS [t0] ON [t].[Date] = [t0].[Date]
ORDER BY [t].[Date], [t0].[Date], [t0].[Name]");
    }

    public override async Task Skip_Take_on_grouping_element_inside_collection_projection(bool async)
    {
        await base.Skip_Take_on_grouping_element_inside_collection_projection(async);

        AssertSql(
            @"SELECT [l].[Id], [t2].[Date], [t2].[Id], [t2].[Date0], [t2].[Name], [t2].[PeriodEnd], [t2].[PeriodStart]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
OUTER APPLY (
    SELECT [t].[Date], [t0].[Id], [t0].[Date] AS [Date0], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart]
    FROM (
        SELECT [l0].[Date]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        WHERE [l0].[Name] = [l].[Name] OR ([l0].[Name] IS NULL AND [l].[Name] IS NULL)
        GROUP BY [l0].[Date]
    ) AS [t]
    LEFT JOIN (
        SELECT [t1].[Id], [t1].[Date], [t1].[Name], [t1].[PeriodEnd], [t1].[PeriodStart]
        FROM (
            SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[PeriodEnd], [l1].[PeriodStart], ROW_NUMBER() OVER(PARTITION BY [l1].[Date] ORDER BY [l1].[Name]) AS [row]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1]
            WHERE [l1].[Name] = [l].[Name] OR ([l1].[Name] IS NULL AND [l].[Name] IS NULL)
        ) AS [t1]
        WHERE 1 < [t1].[row] AND [t1].[row] <= 6
    ) AS [t0] ON [t].[Date] = [t0].[Date]
) AS [t2]
ORDER BY [l].[Id], [t2].[Date], [t2].[Date0], [t2].[Name]");
    }

    public override async Task Skip_Take_on_grouping_element_into_non_entity(bool async)
    {
        await base.Skip_Take_on_grouping_element_into_non_entity(async);

        AssertSql(
            @"SELECT [t].[Date], [t0].[Name], [t0].[Id]
FROM (
    SELECT [l].[Date]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    GROUP BY [l].[Date]
) AS [t]
LEFT JOIN (
    SELECT [t1].[Name], [t1].[Id], [t1].[Date]
    FROM (
        SELECT [l0].[Name], [l0].[Id], [l0].[Date], ROW_NUMBER() OVER(PARTITION BY [l0].[Date] ORDER BY [l0].[Name]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    ) AS [t1]
    WHERE 1 < [t1].[row] AND [t1].[row] <= 6
) AS [t0] ON [t].[Date] = [t0].[Date]
ORDER BY [t].[Date], [t0].[Date], [t0].[Name]");
    }

    public override async Task Skip_Take_on_grouping_element_with_collection_include(bool async)
    {
        await base.Skip_Take_on_grouping_element_with_collection_include(async);

        AssertSql(
            @"SELECT [t].[Date], [t1].[Id], [t1].[Date], [t1].[Name], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Id0], [t1].[OneToOne_Required_PK_Date], [t1].[Level1_Optional_Id], [t1].[Level1_Required_Id], [t1].[Level2_Name], [t1].[OneToMany_Optional_Inverse2Id], [t1].[OneToMany_Required_Inverse2Id], [t1].[OneToOne_Optional_PK_Inverse2Id], [t1].[PeriodEnd0], [t1].[PeriodStart0], [t1].[Id00]
FROM (
    SELECT [l].[Date]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    GROUP BY [l].[Date]
) AS [t]
OUTER APPLY (
    SELECT [t0].[Id], [t0].[Date], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart], [t2].[Id] AS [Id0], [t2].[OneToOne_Required_PK_Date], [t2].[Level1_Optional_Id], [t2].[Level1_Required_Id], [t2].[Level2_Name], [t2].[OneToMany_Optional_Inverse2Id], [t2].[OneToMany_Required_Inverse2Id], [t2].[OneToOne_Optional_PK_Inverse2Id], [t2].[PeriodEnd] AS [PeriodEnd0], [t2].[PeriodStart] AS [PeriodStart0], [t2].[Id0] AS [Id00]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name], [l0].[PeriodEnd], [l0].[PeriodStart]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        WHERE [t].[Date] = [l0].[Date]
        ORDER BY [l0].[Name]
        OFFSET 1 ROWS FETCH NEXT 5 ROWS ONLY
    ) AS [t0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[PeriodEnd], [l1].[PeriodStart], [l2].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2] ON [l1].[Id] = [l2].[Id]
        WHERE [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t2] ON [t0].[Id] = [t2].[OneToMany_Optional_Inverse2Id]
) AS [t1]
ORDER BY [t].[Date], [t1].[Name], [t1].[Id], [t1].[Id0]");
    }

    public override async Task Skip_Take_on_grouping_element_with_reference_include(bool async)
    {
        await base.Skip_Take_on_grouping_element_with_reference_include(async);

        AssertSql(
            @"SELECT [t].[Date], [t1].[Id], [t1].[Date], [t1].[Name], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Id0], [t1].[OneToOne_Required_PK_Date], [t1].[Level1_Optional_Id], [t1].[Level1_Required_Id], [t1].[Level2_Name], [t1].[OneToMany_Optional_Inverse2Id], [t1].[OneToMany_Required_Inverse2Id], [t1].[OneToOne_Optional_PK_Inverse2Id], [t1].[PeriodEnd0], [t1].[PeriodStart0], [t1].[Id00]
FROM (
    SELECT [l].[Date]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    GROUP BY [l].[Date]
) AS [t]
OUTER APPLY (
    SELECT [t0].[Id], [t0].[Date], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart], [t2].[Id] AS [Id0], [t2].[OneToOne_Required_PK_Date], [t2].[Level1_Optional_Id], [t2].[Level1_Required_Id], [t2].[Level2_Name], [t2].[OneToMany_Optional_Inverse2Id], [t2].[OneToMany_Required_Inverse2Id], [t2].[OneToOne_Optional_PK_Inverse2Id], [t2].[PeriodEnd] AS [PeriodEnd0], [t2].[PeriodStart] AS [PeriodStart0], [t2].[Id0] AS [Id00]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name], [l0].[PeriodEnd], [l0].[PeriodStart]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        WHERE [t].[Date] = [l0].[Date]
        ORDER BY [l0].[Name]
        OFFSET 1 ROWS FETCH NEXT 5 ROWS ONLY
    ) AS [t0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[PeriodEnd], [l1].[PeriodStart], [l2].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2] ON [l1].[Id] = [l2].[Id]
        WHERE [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t2] ON [t0].[Id] = [t2].[Level1_Optional_Id]
) AS [t1]
ORDER BY [t].[Date], [t1].[Name], [t1].[Id], [t1].[Id0]");
    }

    public override async Task Skip_Take_Select_collection_Skip_Take(bool async)
    {
        await base.Skip_Take_Select_collection_Skip_Take(async);

        AssertSql(
            @"@__p_0='1'

SELECT [t].[Id], [t].[Name], [t0].[Id], [t0].[Name], [t0].[Level1Id], [t0].[Level2Id], [t0].[Id0], [t0].[Date], [t0].[Name0], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id1], [t0].[Id00]
FROM (
    SELECT [l].[Id], [l].[Name]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    ORDER BY [l].[Id]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY
) AS [t]
OUTER APPLY (
    SELECT CASE
        WHEN [t1].[OneToOne_Required_PK_Date] IS NOT NULL AND [t1].[Level1_Required_Id] IS NOT NULL AND [t1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t1].[Id]
    END AS [Id], [t1].[Level2_Name] AS [Name], [t1].[OneToMany_Required_Inverse2Id] AS [Level1Id], [t1].[Level1_Required_Id] AS [Level2Id], [l1].[Id] AS [Id0], [l1].[Date], [l1].[Name] AS [Name0], [l1].[PeriodEnd], [l1].[PeriodStart], [t1].[Id] AS [Id1], [t1].[Id0] AS [Id00], [t1].[c]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Required_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l2].[Id] AS [Id0], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [c]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2] ON [l0].[Id] = [l2].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[Id] = [l0].[OneToMany_Required_Inverse2Id]
        ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END
        OFFSET 1 ROWS FETCH NEXT 3 ROWS ONLY
    ) AS [t1]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [t1].[Level1_Required_Id] = [l1].[Id]
) AS [t0]
ORDER BY [t].[Id], [t0].[c], [t0].[Id1], [t0].[Id00]");
    }

    public override async Task Skip_Take_ToList_on_grouping_element(bool async)
    {
        await base.Skip_Take_ToList_on_grouping_element(async);

        AssertSql(
            @"SELECT [t].[Date], [t0].[Id], [t0].[Date], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart]
FROM (
    SELECT [l].[Date]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    GROUP BY [l].[Date]
) AS [t]
LEFT JOIN (
    SELECT [t1].[Id], [t1].[Date], [t1].[Name], [t1].[PeriodEnd], [t1].[PeriodStart]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name], [l0].[PeriodEnd], [l0].[PeriodStart], ROW_NUMBER() OVER(PARTITION BY [l0].[Date] ORDER BY [l0].[Name]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    ) AS [t1]
    WHERE 1 < [t1].[row] AND [t1].[row] <= 6
) AS [t0] ON [t].[Date] = [t0].[Date]
ORDER BY [t].[Date], [t0].[Date], [t0].[Name]");
    }

    public override async Task Take_on_correlated_collection_in_projection(bool async)
    {
        await base.Take_on_correlated_collection_in_projection(async);

        AssertSql(
            @"SELECT [l].[Id], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id], [l1].[Id]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t]
    WHERE [t].[row] <= 50
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[Id]");
    }

    public override async Task Take_on_grouping_element(bool async)
    {
        await base.Take_on_grouping_element(async);

        AssertSql(
            @"SELECT [t].[Date], [t0].[Id], [t0].[Date], [t0].[Name], [t0].[PeriodEnd], [t0].[PeriodStart]
FROM (
    SELECT [l].[Date]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    GROUP BY [l].[Date]
) AS [t]
LEFT JOIN (
    SELECT [t1].[Id], [t1].[Date], [t1].[Name], [t1].[PeriodEnd], [t1].[PeriodStart]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name], [l0].[PeriodEnd], [l0].[PeriodStart], ROW_NUMBER() OVER(PARTITION BY [l0].[Date] ORDER BY [l0].[Name] DESC) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    ) AS [t1]
    WHERE [t1].[row] <= 10
) AS [t0] ON [t].[Date] = [t0].[Date]
ORDER BY [t].[Date], [t0].[Date], [t0].[Name] DESC");
    }

    public override async Task Take_Select_collection_Take(bool async)
    {
        await base.Take_Select_collection_Take(async);

        AssertSql(
            @"@__p_0='1'

SELECT [t].[Id], [t].[Name], [t0].[Id], [t0].[Name], [t0].[Level1Id], [t0].[Level2Id], [t0].[Id0], [t0].[Date], [t0].[Name0], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id1], [t0].[Id00]
FROM (
    SELECT TOP(@__p_0) [l].[Id], [l].[Name]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    ORDER BY [l].[Id]
) AS [t]
OUTER APPLY (
    SELECT CASE
        WHEN [t1].[OneToOne_Required_PK_Date] IS NOT NULL AND [t1].[Level1_Required_Id] IS NOT NULL AND [t1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [t1].[Id]
    END AS [Id], [t1].[Level2_Name] AS [Name], [t1].[OneToMany_Required_Inverse2Id] AS [Level1Id], [t1].[Level1_Required_Id] AS [Level2Id], [l1].[Id] AS [Id0], [l1].[Date], [l1].[Name] AS [Name0], [l1].[PeriodEnd], [l1].[PeriodStart], [t1].[Id] AS [Id1], [t1].[Id0] AS [Id00], [t1].[c]
    FROM (
        SELECT TOP(3) [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Required_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l2].[Id] AS [Id0], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [c]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2] ON [l0].[Id] = [l2].[Id]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [t].[Id] = [l0].[OneToMany_Required_Inverse2Id]
        ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END
    ) AS [t1]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [t1].[Level1_Required_Id] = [l1].[Id]
) AS [t0]
ORDER BY [t].[Id], [t0].[c], [t0].[Id1], [t0].[Id00]");
    }

    public override async Task SelectMany_with_predicate_and_DefaultIfEmpty_projecting_root_collection_element_and_another_collection(bool async)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            base.SelectMany_with_predicate_and_DefaultIfEmpty_projecting_root_collection_element_and_another_collection(async));

        Assert.Equal(
            CoreStrings.ExpressionParameterizationExceptionSensitive(
                "Convert(value(Microsoft.EntityFrameworkCore.Query.ComplexNavigationsCollectionsQueryTestBase`1+<>c__DisplayClass140_0[Microsoft.EntityFrameworkCore.Query.TemporalComplexNavigationsSharedTypeQuerySqlServerFixture]).ss.Set(), DbSet`1).TemporalAsOf(1/1/2010 12:00:00 AM)"),
            exception.Message);
        Assert.True(exception.InnerException is InvalidCastException);
    }

    [ConditionalTheory(Skip = "issue #26922")]
    public override async Task Filtered_include_include_parameter_used_inside_filter_throws(bool async)
    {
        await base.Filtered_include_include_parameter_used_inside_filter_throws(async);

        AssertSql("");
    }

    [ConditionalTheory(Skip = "issue #26922")]
    public override async Task Filtered_include_outer_parameter_used_inside_filter(bool async)
    {
        await base.Filtered_include_outer_parameter_used_inside_filter(async);

        AssertSql("");
    }

    [ConditionalTheory(Skip = "issue #26922")]
    public override async Task Include_inside_subquery(bool async)
    {
        await base.Include_inside_subquery(async);

        AssertSql("");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
