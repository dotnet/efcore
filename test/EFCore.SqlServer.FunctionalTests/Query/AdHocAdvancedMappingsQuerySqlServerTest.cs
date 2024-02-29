// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocAdvancedMappingsQuerySqlServerTest : AdHocAdvancedMappingsQueryRelationalTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    public override async Task Two_similar_complex_properties_projected_with_split_query1()
    {
        await base.Two_similar_complex_properties_projected_with_split_query1();

        AssertSql(
"""
SELECT [o].[Id]
FROM [Offers] AS [o]
ORDER BY [o].[Id]
""",
                //
                """
SELECT [t].[Id], [t].[NestedId], [t].[OfferId], [t].[payment_brutto], [t].[payment_netto], [t].[Id0], [t].[payment_brutto0], [t].[payment_netto0], [o].[Id]
FROM [Offers] AS [o]
INNER JOIN (
    SELECT [v].[Id], [v].[NestedId], [v].[OfferId], [v].[payment_brutto], [v].[payment_netto], [n].[Id] AS [Id0], [n].[payment_brutto] AS [payment_brutto0], [n].[payment_netto] AS [payment_netto0]
    FROM [Variation] AS [v]
    LEFT JOIN [NestedEntity] AS [n] ON [v].[NestedId] = [n].[Id]
) AS [t] ON [o].[Id] = [t].[OfferId]
ORDER BY [o].[Id]
""");
    }

    public override async Task Two_similar_complex_properties_projected_with_split_query2()
    {
        await base.Two_similar_complex_properties_projected_with_split_query2();

        AssertSql(
"""
SELECT TOP(2) [o].[Id]
FROM [Offers] AS [o]
WHERE [o].[Id] = 1
ORDER BY [o].[Id]
""",
                //
                """
SELECT [t0].[Id], [t0].[NestedId], [t0].[OfferId], [t0].[payment_brutto], [t0].[payment_netto], [t0].[Id0], [t0].[payment_brutto0], [t0].[payment_netto0], [t].[Id]
FROM (
    SELECT TOP(1) [o].[Id]
    FROM [Offers] AS [o]
    WHERE [o].[Id] = 1
) AS [t]
INNER JOIN (
    SELECT [v].[Id], [v].[NestedId], [v].[OfferId], [v].[payment_brutto], [v].[payment_netto], [n].[Id] AS [Id0], [n].[payment_brutto] AS [payment_brutto0], [n].[payment_netto] AS [payment_netto0]
    FROM [Variation] AS [v]
    LEFT JOIN [NestedEntity] AS [n] ON [v].[NestedId] = [n].[Id]
) AS [t0] ON [t].[Id] = [t0].[OfferId]
ORDER BY [t].[Id]
""");
    }

    public override async Task Projecting_one_of_two_similar_complex_types_picks_the_correct_one()
    {
        await base.Projecting_one_of_two_similar_complex_types_picks_the_correct_one();

        AssertSql(
"""
@__p_0='10'

SELECT [a].[Id], [t].[Info_Created0] AS [Created]
FROM (
    SELECT TOP(@__p_0) [c].[Id], [b].[AId], [b].[Info_Created] AS [Info_Created0]
    FROM [Cs] AS [c]
    INNER JOIN [Bs] AS [b] ON [c].[BId] = [b].[Id]
    WHERE [b].[AId] = 1
    ORDER BY [c].[Id]
) AS [t]
LEFT JOIN [As] AS [a] ON [t].[AId] = [a].[Id]
ORDER BY [t].[Id]
""");
    }
}
