// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class CompositeKeysSplitQuerySqlServerTest : CompositeKeysSplitQueryRelationalTestBase<CompositeKeysQuerySqlServerFixture>
{
    public CompositeKeysSplitQuerySqlServerTest(
        CompositeKeysQuerySqlServerFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Projecting_collections_multi_level(bool async)
    {
        await base.Projecting_collections_multi_level(async);

        AssertSql(
            """
SELECT [c].[Name], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
ORDER BY [c].[Id2], [c].[Id1]
""",
            //
            """
SELECT [c8].[Name], [c].[Id1], [c].[Id2], [c8].[Id1], [c8].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c8] ON [c].[Id1] = [c8].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c8].[OneToMany_Optional_Inverse2Id2]
ORDER BY [c].[Id2], [c].[Id1], [c8].[Id2], [c8].[Id1]
""",
            //
            """
SELECT [c11].[Id1], [c11].[Id2], [c11].[Level2_Optional_Id1], [c11].[Level2_Optional_Id2], [c11].[Level2_Required_Id1], [c11].[Level2_Required_Id2], [c11].[Name], [c11].[OneToMany_Optional_Inverse3Id1], [c11].[OneToMany_Optional_Inverse3Id2], [c11].[OneToMany_Optional_Self_Inverse3Id1], [c11].[OneToMany_Optional_Self_Inverse3Id2], [c11].[OneToMany_Required_Inverse3Id1], [c11].[OneToMany_Required_Inverse3Id2], [c11].[OneToMany_Required_Self_Inverse3Id1], [c11].[OneToMany_Required_Self_Inverse3Id2], [c11].[OneToOne_Optional_PK_Inverse3Id1], [c11].[OneToOne_Optional_PK_Inverse3Id2], [c11].[OneToOne_Optional_Self3Id1], [c11].[OneToOne_Optional_Self3Id2], [c].[Id1], [c].[Id2], [c8].[Id1], [c8].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c8] ON [c].[Id1] = [c8].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c8].[OneToMany_Optional_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c11] ON [c8].[Id1] = [c11].[OneToMany_Required_Inverse3Id1] AND [c8].[Id2] = [c11].[OneToMany_Required_Inverse3Id2]
ORDER BY [c].[Id2], [c].[Id1], [c8].[Id2], [c8].[Id1], [c11].[Id2] DESC
""");
    }

    public override async Task Projecting_multiple_collections_on_multiple_levels_no_explicit_ordering(bool async)
    {
        await base.Projecting_multiple_collections_on_multiple_levels_no_explicit_ordering(async);

        AssertSql(
            """
SELECT [c].[Name], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
ORDER BY [c].[Id1], [c].[Id2]
""",
            //
            """
SELECT [c62].[Name], [c].[Id1], [c].[Id2], [c62].[Id1], [c62].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c62] ON [c].[Id1] = [c62].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c62].[OneToMany_Optional_Inverse2Id2]
ORDER BY [c].[Id1], [c].[Id2], [c62].[Id1], [c62].[Id2]
""",
            //
            """
SELECT [c75].[Name], [c].[Id1], [c].[Id2], [c62].[Id1], [c62].[Id2], [c75].[Id1], [c75].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c62] ON [c].[Id1] = [c62].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c62].[OneToMany_Optional_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c75] ON [c62].[Id1] = [c75].[OneToMany_Required_Inverse3Id1] AND [c62].[Id2] = [c75].[OneToMany_Required_Inverse3Id2]
ORDER BY [c].[Id1], [c].[Id2], [c62].[Id1], [c62].[Id2], [c75].[Id1], [c75].[Id2]
""",
            //
            """
SELECT [c87].[Name], [c].[Id1], [c].[Id2], [c62].[Id1], [c62].[Id2], [c87].[Id1], [c87].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c62] ON [c].[Id1] = [c62].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c62].[OneToMany_Optional_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c87] ON [c62].[Id1] = [c87].[OneToMany_Optional_Inverse3Id1] AND [c62].[Id2] = [c87].[OneToMany_Optional_Inverse3Id2]
ORDER BY [c].[Id1], [c].[Id2], [c62].[Id1], [c62].[Id2], [c87].[Id1], [c87].[Id2]
""",
            //
            """
SELECT [c90].[Id1], [c90].[Id2], [c90].[Level3_Optional_Id1], [c90].[Level3_Optional_Id2], [c90].[Level3_Required_Id1], [c90].[Level3_Required_Id2], [c90].[Name], [c90].[OneToMany_Optional_Inverse4Id1], [c90].[OneToMany_Optional_Inverse4Id2], [c90].[OneToMany_Optional_Self_Inverse4Id1], [c90].[OneToMany_Optional_Self_Inverse4Id2], [c90].[OneToMany_Required_Inverse4Id1], [c90].[OneToMany_Required_Inverse4Id2], [c90].[OneToMany_Required_Self_Inverse4Id1], [c90].[OneToMany_Required_Self_Inverse4Id2], [c90].[OneToOne_Optional_PK_Inverse4Id1], [c90].[OneToOne_Optional_PK_Inverse4Id2], [c90].[OneToOne_Optional_Self4Id1], [c90].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c62].[Id1], [c62].[Id2], [c87].[Id1], [c87].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c62] ON [c].[Id1] = [c62].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c62].[OneToMany_Optional_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c87] ON [c62].[Id1] = [c87].[OneToMany_Optional_Inverse3Id1] AND [c62].[Id2] = [c87].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c90] ON [c87].[Id1] = [c90].[OneToMany_Required_Inverse4Id1] AND [c87].[Id2] = [c90].[OneToMany_Required_Inverse4Id2]
ORDER BY [c].[Id1], [c].[Id2], [c62].[Id1], [c62].[Id2], [c87].[Id1], [c87].[Id2]
""",
            //
            """
SELECT [c92].[Id1], [c92].[Id2], [c92].[Level3_Optional_Id1], [c92].[Level3_Optional_Id2], [c92].[Level3_Required_Id1], [c92].[Level3_Required_Id2], [c92].[Name], [c92].[OneToMany_Optional_Inverse4Id1], [c92].[OneToMany_Optional_Inverse4Id2], [c92].[OneToMany_Optional_Self_Inverse4Id1], [c92].[OneToMany_Optional_Self_Inverse4Id2], [c92].[OneToMany_Required_Inverse4Id1], [c92].[OneToMany_Required_Inverse4Id2], [c92].[OneToMany_Required_Self_Inverse4Id1], [c92].[OneToMany_Required_Self_Inverse4Id2], [c92].[OneToOne_Optional_PK_Inverse4Id1], [c92].[OneToOne_Optional_PK_Inverse4Id2], [c92].[OneToOne_Optional_Self4Id1], [c92].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c62].[Id1], [c62].[Id2], [c87].[Id1], [c87].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c62] ON [c].[Id1] = [c62].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c62].[OneToMany_Optional_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c87] ON [c62].[Id1] = [c87].[OneToMany_Optional_Inverse3Id1] AND [c62].[Id2] = [c87].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c92] ON [c87].[Id1] = [c92].[OneToMany_Optional_Inverse4Id1] AND [c87].[Id2] = [c92].[OneToMany_Optional_Inverse4Id2]
ORDER BY [c].[Id1], [c].[Id2], [c62].[Id1], [c62].[Id2], [c87].[Id1], [c87].[Id2]
""",
            //
            """
SELECT [c124].[Name], [c].[Id1], [c].[Id2], [c124].[Id1], [c124].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c124] ON [c].[Id1] = [c124].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c124].[OneToMany_Required_Inverse2Id2]
ORDER BY [c].[Id1], [c].[Id2], [c124].[Id1], [c124].[Id2]
""",
            //
            """
SELECT [c137].[Name], [c].[Id1], [c].[Id2], [c124].[Id1], [c124].[Id2], [c137].[Id1], [c137].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c124] ON [c].[Id1] = [c124].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c124].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c137] ON [c124].[Id1] = [c137].[OneToMany_Optional_Inverse3Id1] AND [c124].[Id2] = [c137].[OneToMany_Optional_Inverse3Id2]
ORDER BY [c].[Id1], [c].[Id2], [c124].[Id1], [c124].[Id2], [c137].[Id1], [c137].[Id2]
""",
            //
            """
SELECT [c140].[Id1], [c140].[Id2], [c140].[Level3_Optional_Id1], [c140].[Level3_Optional_Id2], [c140].[Level3_Required_Id1], [c140].[Level3_Required_Id2], [c140].[Name], [c140].[OneToMany_Optional_Inverse4Id1], [c140].[OneToMany_Optional_Inverse4Id2], [c140].[OneToMany_Optional_Self_Inverse4Id1], [c140].[OneToMany_Optional_Self_Inverse4Id2], [c140].[OneToMany_Required_Inverse4Id1], [c140].[OneToMany_Required_Inverse4Id2], [c140].[OneToMany_Required_Self_Inverse4Id1], [c140].[OneToMany_Required_Self_Inverse4Id2], [c140].[OneToOne_Optional_PK_Inverse4Id1], [c140].[OneToOne_Optional_PK_Inverse4Id2], [c140].[OneToOne_Optional_Self4Id1], [c140].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c124].[Id1], [c124].[Id2], [c137].[Id1], [c137].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c124] ON [c].[Id1] = [c124].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c124].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c137] ON [c124].[Id1] = [c137].[OneToMany_Optional_Inverse3Id1] AND [c124].[Id2] = [c137].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c140] ON [c137].[Id1] = [c140].[OneToMany_Required_Inverse4Id1] AND [c137].[Id2] = [c140].[OneToMany_Required_Inverse4Id2]
ORDER BY [c].[Id1], [c].[Id2], [c124].[Id1], [c124].[Id2], [c137].[Id1], [c137].[Id2]
""",
            //
            """
SELECT [c142].[Id1], [c142].[Id2], [c142].[Level3_Optional_Id1], [c142].[Level3_Optional_Id2], [c142].[Level3_Required_Id1], [c142].[Level3_Required_Id2], [c142].[Name], [c142].[OneToMany_Optional_Inverse4Id1], [c142].[OneToMany_Optional_Inverse4Id2], [c142].[OneToMany_Optional_Self_Inverse4Id1], [c142].[OneToMany_Optional_Self_Inverse4Id2], [c142].[OneToMany_Required_Inverse4Id1], [c142].[OneToMany_Required_Inverse4Id2], [c142].[OneToMany_Required_Self_Inverse4Id1], [c142].[OneToMany_Required_Self_Inverse4Id2], [c142].[OneToOne_Optional_PK_Inverse4Id1], [c142].[OneToOne_Optional_PK_Inverse4Id2], [c142].[OneToOne_Optional_Self4Id1], [c142].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c124].[Id1], [c124].[Id2], [c137].[Id1], [c137].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c124] ON [c].[Id1] = [c124].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c124].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c137] ON [c124].[Id1] = [c137].[OneToMany_Optional_Inverse3Id1] AND [c124].[Id2] = [c137].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c142] ON [c137].[Id1] = [c142].[OneToMany_Optional_Inverse4Id1] AND [c137].[Id2] = [c142].[OneToMany_Optional_Inverse4Id2]
ORDER BY [c].[Id1], [c].[Id2], [c124].[Id1], [c124].[Id2], [c137].[Id1], [c137].[Id2]
""",
            //
            """
SELECT [c149].[Name], [c].[Id1], [c].[Id2], [c124].[Id1], [c124].[Id2], [c149].[Id1], [c149].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c124] ON [c].[Id1] = [c124].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c124].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c149] ON [c124].[Id1] = [c149].[OneToMany_Optional_Inverse3Id1] AND [c124].[Id2] = [c149].[OneToMany_Optional_Inverse3Id2]
ORDER BY [c].[Id1], [c].[Id2], [c124].[Id1], [c124].[Id2], [c149].[Id1], [c149].[Id2]
""",
            //
            """
SELECT [c152].[Id1], [c152].[Id2], [c152].[Level3_Optional_Id1], [c152].[Level3_Optional_Id2], [c152].[Level3_Required_Id1], [c152].[Level3_Required_Id2], [c152].[Name], [c152].[OneToMany_Optional_Inverse4Id1], [c152].[OneToMany_Optional_Inverse4Id2], [c152].[OneToMany_Optional_Self_Inverse4Id1], [c152].[OneToMany_Optional_Self_Inverse4Id2], [c152].[OneToMany_Required_Inverse4Id1], [c152].[OneToMany_Required_Inverse4Id2], [c152].[OneToMany_Required_Self_Inverse4Id1], [c152].[OneToMany_Required_Self_Inverse4Id2], [c152].[OneToOne_Optional_PK_Inverse4Id1], [c152].[OneToOne_Optional_PK_Inverse4Id2], [c152].[OneToOne_Optional_Self4Id1], [c152].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c124].[Id1], [c124].[Id2], [c149].[Id1], [c149].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c124] ON [c].[Id1] = [c124].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c124].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c149] ON [c124].[Id1] = [c149].[OneToMany_Optional_Inverse3Id1] AND [c124].[Id2] = [c149].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c152] ON [c149].[Id1] = [c152].[OneToMany_Optional_Inverse4Id1] AND [c149].[Id2] = [c152].[OneToMany_Optional_Inverse4Id2]
ORDER BY [c].[Id1], [c].[Id2], [c124].[Id1], [c124].[Id2], [c149].[Id1], [c149].[Id2]
""",
            //
            """
SELECT [c154].[Id1], [c154].[Id2], [c154].[Level3_Optional_Id1], [c154].[Level3_Optional_Id2], [c154].[Level3_Required_Id1], [c154].[Level3_Required_Id2], [c154].[Name], [c154].[OneToMany_Optional_Inverse4Id1], [c154].[OneToMany_Optional_Inverse4Id2], [c154].[OneToMany_Optional_Self_Inverse4Id1], [c154].[OneToMany_Optional_Self_Inverse4Id2], [c154].[OneToMany_Required_Inverse4Id1], [c154].[OneToMany_Required_Inverse4Id2], [c154].[OneToMany_Required_Self_Inverse4Id1], [c154].[OneToMany_Required_Self_Inverse4Id2], [c154].[OneToOne_Optional_PK_Inverse4Id1], [c154].[OneToOne_Optional_PK_Inverse4Id2], [c154].[OneToOne_Optional_Self4Id1], [c154].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c124].[Id1], [c124].[Id2], [c149].[Id1], [c149].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c124] ON [c].[Id1] = [c124].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c124].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c149] ON [c124].[Id1] = [c149].[OneToMany_Optional_Inverse3Id1] AND [c124].[Id2] = [c149].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c154] ON [c149].[Id1] = [c154].[OneToMany_Required_Inverse4Id1] AND [c149].[Id2] = [c154].[OneToMany_Required_Inverse4Id2]
ORDER BY [c].[Id1], [c].[Id2], [c124].[Id1], [c124].[Id2], [c149].[Id1], [c149].[Id2]
""");
    }

    public override async Task Projecting_multiple_collections_on_multiple_levels_some_explicit_ordering(bool async)
    {
        await base.Projecting_multiple_collections_on_multiple_levels_some_explicit_ordering(async);

        AssertSql(
            """
SELECT [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2]
""",
            //
            """
SELECT [c62].[Name], [c].[Id1], [c].[Id2], [c62].[Id1], [c62].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c62] ON [c].[Id1] = [c62].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c62].[OneToMany_Optional_Inverse2Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], [c62].[Id1], [c62].[Id2]
""",
            //
            """
SELECT [c].[Id1], [c].[Id2], [c62].[Id1], [c62].[Id2], [c75].[Id1], [c75].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c62] ON [c].[Id1] = [c62].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c62].[OneToMany_Optional_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c75] ON [c62].[Id1] = [c75].[OneToMany_Required_Inverse3Id1] AND [c62].[Id2] = [c75].[OneToMany_Required_Inverse3Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], [c62].[Id1], [c62].[Id2], [c75].[Id2] DESC, [c75].[Id1] DESC
""",
            //
            """
SELECT [c87].[Name], [c].[Id1], [c].[Id2], [c62].[Id1], [c62].[Id2], [c87].[Id1], [c87].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c62] ON [c].[Id1] = [c62].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c62].[OneToMany_Optional_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c87] ON [c62].[Id1] = [c87].[OneToMany_Optional_Inverse3Id1] AND [c62].[Id2] = [c87].[OneToMany_Optional_Inverse3Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], [c62].[Id1], [c62].[Id2], [c87].[Id1], [c87].[Id2]
""",
            //
            """
SELECT [c90].[Id1], [c90].[Id2], [c90].[Level3_Optional_Id1], [c90].[Level3_Optional_Id2], [c90].[Level3_Required_Id1], [c90].[Level3_Required_Id2], [c90].[Name], [c90].[OneToMany_Optional_Inverse4Id1], [c90].[OneToMany_Optional_Inverse4Id2], [c90].[OneToMany_Optional_Self_Inverse4Id1], [c90].[OneToMany_Optional_Self_Inverse4Id2], [c90].[OneToMany_Required_Inverse4Id1], [c90].[OneToMany_Required_Inverse4Id2], [c90].[OneToMany_Required_Self_Inverse4Id1], [c90].[OneToMany_Required_Self_Inverse4Id2], [c90].[OneToOne_Optional_PK_Inverse4Id1], [c90].[OneToOne_Optional_PK_Inverse4Id2], [c90].[OneToOne_Optional_Self4Id1], [c90].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c62].[Id1], [c62].[Id2], [c87].[Id1], [c87].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c62] ON [c].[Id1] = [c62].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c62].[OneToMany_Optional_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c87] ON [c62].[Id1] = [c87].[OneToMany_Optional_Inverse3Id1] AND [c62].[Id2] = [c87].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c90] ON [c87].[Id1] = [c90].[OneToMany_Required_Inverse4Id1] AND [c87].[Id2] = [c90].[OneToMany_Required_Inverse4Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], [c62].[Id1], [c62].[Id2], [c87].[Id1], [c87].[Id2]
""",
            //
            """
SELECT [c92].[Id1], [c92].[Id2], [c92].[Level3_Optional_Id1], [c92].[Level3_Optional_Id2], [c92].[Level3_Required_Id1], [c92].[Level3_Required_Id2], [c92].[Name], [c92].[OneToMany_Optional_Inverse4Id1], [c92].[OneToMany_Optional_Inverse4Id2], [c92].[OneToMany_Optional_Self_Inverse4Id1], [c92].[OneToMany_Optional_Self_Inverse4Id2], [c92].[OneToMany_Required_Inverse4Id1], [c92].[OneToMany_Required_Inverse4Id2], [c92].[OneToMany_Required_Self_Inverse4Id1], [c92].[OneToMany_Required_Self_Inverse4Id2], [c92].[OneToOne_Optional_PK_Inverse4Id1], [c92].[OneToOne_Optional_PK_Inverse4Id2], [c92].[OneToOne_Optional_Self4Id1], [c92].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c62].[Id1], [c62].[Id2], [c87].[Id1], [c87].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c62] ON [c].[Id1] = [c62].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c62].[OneToMany_Optional_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c87] ON [c62].[Id1] = [c87].[OneToMany_Optional_Inverse3Id1] AND [c62].[Id2] = [c87].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c92] ON [c87].[Id1] = [c92].[OneToMany_Optional_Inverse4Id1] AND [c87].[Id2] = [c92].[OneToMany_Optional_Inverse4Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], [c62].[Id1], [c62].[Id2], [c87].[Id1], [c87].[Id2]
""",
            //
            """
SELECT [c124].[Name], [c].[Id1], [c].[Id2], [c124].[Id1], [c124].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c124] ON [c].[Id1] = [c124].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c124].[OneToMany_Required_Inverse2Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], CAST(LEN([c124].[Name]) AS int), [c124].[Id1], [c124].[Id2]
""",
            //
            """
SELECT [c137].[Name], [c].[Id1], [c].[Id2], [c124].[Id1], [c124].[Id2], [c137].[Id1], [c137].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c124] ON [c].[Id1] = [c124].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c124].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c137] ON [c124].[Id1] = [c137].[OneToMany_Optional_Inverse3Id1] AND [c124].[Id2] = [c137].[OneToMany_Optional_Inverse3Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], CAST(LEN([c124].[Name]) AS int), [c124].[Id1], [c124].[Id2], [c137].[Id1], [c137].[Id2]
""",
            //
            """
SELECT [c140].[Id1], [c140].[Id2], [c140].[Level3_Optional_Id1], [c140].[Level3_Optional_Id2], [c140].[Level3_Required_Id1], [c140].[Level3_Required_Id2], [c140].[Name], [c140].[OneToMany_Optional_Inverse4Id1], [c140].[OneToMany_Optional_Inverse4Id2], [c140].[OneToMany_Optional_Self_Inverse4Id1], [c140].[OneToMany_Optional_Self_Inverse4Id2], [c140].[OneToMany_Required_Inverse4Id1], [c140].[OneToMany_Required_Inverse4Id2], [c140].[OneToMany_Required_Self_Inverse4Id1], [c140].[OneToMany_Required_Self_Inverse4Id2], [c140].[OneToOne_Optional_PK_Inverse4Id1], [c140].[OneToOne_Optional_PK_Inverse4Id2], [c140].[OneToOne_Optional_Self4Id1], [c140].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c124].[Id1], [c124].[Id2], [c137].[Id1], [c137].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c124] ON [c].[Id1] = [c124].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c124].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c137] ON [c124].[Id1] = [c137].[OneToMany_Optional_Inverse3Id1] AND [c124].[Id2] = [c137].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c140] ON [c137].[Id1] = [c140].[OneToMany_Required_Inverse4Id1] AND [c137].[Id2] = [c140].[OneToMany_Required_Inverse4Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], CAST(LEN([c124].[Name]) AS int), [c124].[Id1], [c124].[Id2], [c137].[Id1], [c137].[Id2]
""",
            //
            """
SELECT [c142].[Id1], [c142].[Id2], [c142].[Level3_Optional_Id1], [c142].[Level3_Optional_Id2], [c142].[Level3_Required_Id1], [c142].[Level3_Required_Id2], [c142].[Name], [c142].[OneToMany_Optional_Inverse4Id1], [c142].[OneToMany_Optional_Inverse4Id2], [c142].[OneToMany_Optional_Self_Inverse4Id1], [c142].[OneToMany_Optional_Self_Inverse4Id2], [c142].[OneToMany_Required_Inverse4Id1], [c142].[OneToMany_Required_Inverse4Id2], [c142].[OneToMany_Required_Self_Inverse4Id1], [c142].[OneToMany_Required_Self_Inverse4Id2], [c142].[OneToOne_Optional_PK_Inverse4Id1], [c142].[OneToOne_Optional_PK_Inverse4Id2], [c142].[OneToOne_Optional_Self4Id1], [c142].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c124].[Id1], [c124].[Id2], [c137].[Id1], [c137].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c124] ON [c].[Id1] = [c124].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c124].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c137] ON [c124].[Id1] = [c137].[OneToMany_Optional_Inverse3Id1] AND [c124].[Id2] = [c137].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c142] ON [c137].[Id1] = [c142].[OneToMany_Optional_Inverse4Id1] AND [c137].[Id2] = [c142].[OneToMany_Optional_Inverse4Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], CAST(LEN([c124].[Name]) AS int), [c124].[Id1], [c124].[Id2], [c137].[Id1], [c137].[Id2]
""",
            //
            """
SELECT [c149].[Name], [c].[Id1], [c].[Id2], [c124].[Id1], [c124].[Id2], [c149].[Id1], [c149].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c124] ON [c].[Id1] = [c124].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c124].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c149] ON [c124].[Id1] = [c149].[OneToMany_Optional_Inverse3Id1] AND [c124].[Id2] = [c149].[OneToMany_Optional_Inverse3Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], CAST(LEN([c124].[Name]) AS int), [c124].[Id1], [c124].[Id2], [c149].[Id1], [c149].[Id2]
""",
            //
            """
SELECT [c152].[Id1], [c152].[Id2], [c152].[Level3_Optional_Id1], [c152].[Level3_Optional_Id2], [c152].[Level3_Required_Id1], [c152].[Level3_Required_Id2], [c152].[Name], [c152].[OneToMany_Optional_Inverse4Id1], [c152].[OneToMany_Optional_Inverse4Id2], [c152].[OneToMany_Optional_Self_Inverse4Id1], [c152].[OneToMany_Optional_Self_Inverse4Id2], [c152].[OneToMany_Required_Inverse4Id1], [c152].[OneToMany_Required_Inverse4Id2], [c152].[OneToMany_Required_Self_Inverse4Id1], [c152].[OneToMany_Required_Self_Inverse4Id2], [c152].[OneToOne_Optional_PK_Inverse4Id1], [c152].[OneToOne_Optional_PK_Inverse4Id2], [c152].[OneToOne_Optional_Self4Id1], [c152].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c124].[Id1], [c124].[Id2], [c149].[Id1], [c149].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c124] ON [c].[Id1] = [c124].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c124].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c149] ON [c124].[Id1] = [c149].[OneToMany_Optional_Inverse3Id1] AND [c124].[Id2] = [c149].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c152] ON [c149].[Id1] = [c152].[OneToMany_Optional_Inverse4Id1] AND [c149].[Id2] = [c152].[OneToMany_Optional_Inverse4Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], CAST(LEN([c124].[Name]) AS int), [c124].[Id1], [c124].[Id2], [c149].[Id1], [c149].[Id2]
""",
            //
            """
SELECT [c154].[Id1], [c154].[Id2], [c154].[Level3_Optional_Id1], [c154].[Level3_Optional_Id2], [c154].[Level3_Required_Id1], [c154].[Level3_Required_Id2], [c154].[Name], [c154].[OneToMany_Optional_Inverse4Id1], [c154].[OneToMany_Optional_Inverse4Id2], [c154].[OneToMany_Optional_Self_Inverse4Id1], [c154].[OneToMany_Optional_Self_Inverse4Id2], [c154].[OneToMany_Required_Inverse4Id1], [c154].[OneToMany_Required_Inverse4Id2], [c154].[OneToMany_Required_Self_Inverse4Id1], [c154].[OneToMany_Required_Self_Inverse4Id2], [c154].[OneToOne_Optional_PK_Inverse4Id1], [c154].[OneToOne_Optional_PK_Inverse4Id2], [c154].[OneToOne_Optional_Self4Id1], [c154].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c124].[Id1], [c124].[Id2], [c149].[Id1], [c149].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c124] ON [c].[Id1] = [c124].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c124].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c149] ON [c124].[Id1] = [c149].[OneToMany_Optional_Inverse3Id1] AND [c124].[Id2] = [c149].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c154] ON [c149].[Id1] = [c154].[OneToMany_Required_Inverse4Id1] AND [c149].[Id2] = [c154].[OneToMany_Required_Inverse4Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], CAST(LEN([c124].[Name]) AS int), [c124].[Id1], [c124].[Id2], [c149].[Id1], [c149].[Id2], [c154].[Id1] + CAST([c154].[Id2] AS nvarchar(max)) DESC
""");
    }

    public override async Task Projecting_multiple_collections_same_level_top_level_ordering(bool async)
    {
        await base.Projecting_multiple_collections_same_level_top_level_ordering(async);

        AssertSql(
            """
SELECT [c].[Name], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
ORDER BY [c].[Id2], [c].[Id1]
""",
            //
            """
SELECT [c2].[Id1], [c2].[Id2], [c2].[Date], [c2].[Level1_Optional_Id1], [c2].[Level1_Optional_Id2], [c2].[Level1_Required_Id1], [c2].[Level1_Required_Id2], [c2].[Name], [c2].[OneToMany_Optional_Inverse2Id1], [c2].[OneToMany_Optional_Inverse2Id2], [c2].[OneToMany_Optional_Self_Inverse2Id1], [c2].[OneToMany_Optional_Self_Inverse2Id2], [c2].[OneToMany_Required_Inverse2Id1], [c2].[OneToMany_Required_Inverse2Id2], [c2].[OneToMany_Required_Self_Inverse2Id1], [c2].[OneToMany_Required_Self_Inverse2Id2], [c2].[OneToOne_Optional_PK_Inverse2Id1], [c2].[OneToOne_Optional_PK_Inverse2Id2], [c2].[OneToOne_Optional_Self2Id1], [c2].[OneToOne_Optional_Self2Id2], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c2] ON [c].[Id1] = [c2].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c2].[OneToMany_Optional_Inverse2Id2]
ORDER BY [c].[Id2], [c].[Id1]
""",
            //
            """
SELECT [c4].[Id1], [c4].[Id2], [c4].[Date], [c4].[Level1_Optional_Id1], [c4].[Level1_Optional_Id2], [c4].[Level1_Required_Id1], [c4].[Level1_Required_Id2], [c4].[Name], [c4].[OneToMany_Optional_Inverse2Id1], [c4].[OneToMany_Optional_Inverse2Id2], [c4].[OneToMany_Optional_Self_Inverse2Id1], [c4].[OneToMany_Optional_Self_Inverse2Id2], [c4].[OneToMany_Required_Inverse2Id1], [c4].[OneToMany_Required_Inverse2Id2], [c4].[OneToMany_Required_Self_Inverse2Id1], [c4].[OneToMany_Required_Self_Inverse2Id2], [c4].[OneToOne_Optional_PK_Inverse2Id1], [c4].[OneToOne_Optional_PK_Inverse2Id2], [c4].[OneToOne_Optional_Self2Id1], [c4].[OneToOne_Optional_Self2Id2], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c4] ON [c].[Id1] = [c4].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c4].[OneToMany_Required_Inverse2Id2]
ORDER BY [c].[Id2], [c].[Id1]
""");
    }

    public override async Task Projecting_multiple_collections_same_level_top_level_ordering_using_entire_composite_key(bool async)
    {
        await base.Projecting_multiple_collections_same_level_top_level_ordering_using_entire_composite_key(async);

        AssertSql(
            """
SELECT [c].[Name], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
ORDER BY [c].[Id2], [c].[Id1] DESC
""",
            //
            """
SELECT [c2].[Id1], [c2].[Id2], [c2].[Date], [c2].[Level1_Optional_Id1], [c2].[Level1_Optional_Id2], [c2].[Level1_Required_Id1], [c2].[Level1_Required_Id2], [c2].[Name], [c2].[OneToMany_Optional_Inverse2Id1], [c2].[OneToMany_Optional_Inverse2Id2], [c2].[OneToMany_Optional_Self_Inverse2Id1], [c2].[OneToMany_Optional_Self_Inverse2Id2], [c2].[OneToMany_Required_Inverse2Id1], [c2].[OneToMany_Required_Inverse2Id2], [c2].[OneToMany_Required_Self_Inverse2Id1], [c2].[OneToMany_Required_Self_Inverse2Id2], [c2].[OneToOne_Optional_PK_Inverse2Id1], [c2].[OneToOne_Optional_PK_Inverse2Id2], [c2].[OneToOne_Optional_Self2Id1], [c2].[OneToOne_Optional_Self2Id2], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c2] ON [c].[Id1] = [c2].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c2].[OneToMany_Optional_Inverse2Id2]
ORDER BY [c].[Id2], [c].[Id1] DESC
""",
            //
            """
SELECT [c4].[Id1], [c4].[Id2], [c4].[Date], [c4].[Level1_Optional_Id1], [c4].[Level1_Optional_Id2], [c4].[Level1_Required_Id1], [c4].[Level1_Required_Id2], [c4].[Name], [c4].[OneToMany_Optional_Inverse2Id1], [c4].[OneToMany_Optional_Inverse2Id2], [c4].[OneToMany_Optional_Self_Inverse2Id1], [c4].[OneToMany_Optional_Self_Inverse2Id2], [c4].[OneToMany_Required_Inverse2Id1], [c4].[OneToMany_Required_Inverse2Id2], [c4].[OneToMany_Required_Self_Inverse2Id1], [c4].[OneToMany_Required_Self_Inverse2Id2], [c4].[OneToOne_Optional_PK_Inverse2Id1], [c4].[OneToOne_Optional_PK_Inverse2Id2], [c4].[OneToOne_Optional_Self2Id1], [c4].[OneToOne_Optional_Self2Id2], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c4] ON [c].[Id1] = [c4].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c4].[OneToMany_Required_Inverse2Id2]
ORDER BY [c].[Id2], [c].[Id1] DESC
""");
    }

    public override async Task Projecting_multiple_collections_with_ordering_same_level(bool async)
    {
        await base.Projecting_multiple_collections_with_ordering_same_level(async);

        AssertSql(
            """
SELECT [c].[Name], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
ORDER BY [c].[Id1], [c].[Id2]
""",
            //
            """
SELECT [c2].[Id1], [c2].[Id2], [c2].[Date], [c2].[Level1_Optional_Id1], [c2].[Level1_Optional_Id2], [c2].[Level1_Required_Id1], [c2].[Level1_Required_Id2], [c2].[Name], [c2].[OneToMany_Optional_Inverse2Id1], [c2].[OneToMany_Optional_Inverse2Id2], [c2].[OneToMany_Optional_Self_Inverse2Id1], [c2].[OneToMany_Optional_Self_Inverse2Id2], [c2].[OneToMany_Required_Inverse2Id1], [c2].[OneToMany_Required_Inverse2Id2], [c2].[OneToMany_Required_Self_Inverse2Id1], [c2].[OneToMany_Required_Self_Inverse2Id2], [c2].[OneToOne_Optional_PK_Inverse2Id1], [c2].[OneToOne_Optional_PK_Inverse2Id2], [c2].[OneToOne_Optional_Self2Id1], [c2].[OneToOne_Optional_Self2Id2], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c2] ON [c].[Id1] = [c2].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c2].[OneToMany_Optional_Inverse2Id2]
ORDER BY [c].[Id1], [c].[Id2], [c2].[Id2]
""",
            //
            """
SELECT [c4].[Id1], [c4].[Id2], [c4].[Date], [c4].[Level1_Optional_Id1], [c4].[Level1_Optional_Id2], [c4].[Level1_Required_Id1], [c4].[Level1_Required_Id2], [c4].[Name], [c4].[OneToMany_Optional_Inverse2Id1], [c4].[OneToMany_Optional_Inverse2Id2], [c4].[OneToMany_Optional_Self_Inverse2Id1], [c4].[OneToMany_Optional_Self_Inverse2Id2], [c4].[OneToMany_Required_Inverse2Id1], [c4].[OneToMany_Required_Inverse2Id2], [c4].[OneToMany_Required_Self_Inverse2Id1], [c4].[OneToMany_Required_Self_Inverse2Id2], [c4].[OneToOne_Optional_PK_Inverse2Id1], [c4].[OneToOne_Optional_PK_Inverse2Id2], [c4].[OneToOne_Optional_Self2Id1], [c4].[OneToOne_Optional_Self2Id2], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c4] ON [c].[Id1] = [c4].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c4].[OneToMany_Required_Inverse2Id2]
ORDER BY [c].[Id1], [c].[Id2], [c4].[Name] DESC
""");
    }

    public override async Task Projecting_multiple_collections_with_ordering_same_level_top_level_ordering(bool async)
    {
        await base.Projecting_multiple_collections_with_ordering_same_level_top_level_ordering(async);

        AssertSql(
            """
SELECT [c].[Name], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
ORDER BY [c].[Id2], [c].[Id1]
""",
            //
            """
SELECT [c2].[Id1], [c2].[Id2], [c2].[Date], [c2].[Level1_Optional_Id1], [c2].[Level1_Optional_Id2], [c2].[Level1_Required_Id1], [c2].[Level1_Required_Id2], [c2].[Name], [c2].[OneToMany_Optional_Inverse2Id1], [c2].[OneToMany_Optional_Inverse2Id2], [c2].[OneToMany_Optional_Self_Inverse2Id1], [c2].[OneToMany_Optional_Self_Inverse2Id2], [c2].[OneToMany_Required_Inverse2Id1], [c2].[OneToMany_Required_Inverse2Id2], [c2].[OneToMany_Required_Self_Inverse2Id1], [c2].[OneToMany_Required_Self_Inverse2Id2], [c2].[OneToOne_Optional_PK_Inverse2Id1], [c2].[OneToOne_Optional_PK_Inverse2Id2], [c2].[OneToOne_Optional_Self2Id1], [c2].[OneToOne_Optional_Self2Id2], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c2] ON [c].[Id1] = [c2].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c2].[OneToMany_Optional_Inverse2Id2]
ORDER BY [c].[Id2], [c].[Id1], [c2].[Id2]
""",
            //
            """
SELECT [c4].[Id1], [c4].[Id2], [c4].[Date], [c4].[Level1_Optional_Id1], [c4].[Level1_Optional_Id2], [c4].[Level1_Required_Id1], [c4].[Level1_Required_Id2], [c4].[Name], [c4].[OneToMany_Optional_Inverse2Id1], [c4].[OneToMany_Optional_Inverse2Id2], [c4].[OneToMany_Optional_Self_Inverse2Id1], [c4].[OneToMany_Optional_Self_Inverse2Id2], [c4].[OneToMany_Required_Inverse2Id1], [c4].[OneToMany_Required_Inverse2Id2], [c4].[OneToMany_Required_Self_Inverse2Id1], [c4].[OneToMany_Required_Self_Inverse2Id2], [c4].[OneToOne_Optional_PK_Inverse2Id1], [c4].[OneToOne_Optional_PK_Inverse2Id2], [c4].[OneToOne_Optional_Self2Id1], [c4].[OneToOne_Optional_Self2Id2], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c4] ON [c].[Id1] = [c4].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c4].[OneToMany_Required_Inverse2Id2]
ORDER BY [c].[Id2], [c].[Id1], [c4].[Name] DESC
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
