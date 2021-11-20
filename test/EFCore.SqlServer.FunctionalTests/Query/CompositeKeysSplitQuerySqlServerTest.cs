// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class CompositeKeysSplitQuerySqlServerTest : CompositeKeysSplitQueryRelationalTestBase<CompositeKeysQuerySqlServerFixture>
    {
        public CompositeKeysSplitQuerySqlServerTest(
            CompositeKeysQuerySqlServerFixture fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override bool CanExecuteQueryString
            => true;

        public override async Task Projecting_collections_multi_level(bool async)
        {
            await base.Projecting_collections_multi_level(async);

            AssertSql(
                @"SELECT [c].[Name], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
ORDER BY [c].[Id2], [c].[Id1]",
                //
                @"SELECT [c0].[Name], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Optional_Inverse2Id2]
ORDER BY [c].[Id2], [c].[Id1], [c0].[Id2], [c0].[Id1]",
                //
                @"SELECT [c1].[Id1], [c1].[Id2], [c1].[Level2_Optional_Id1], [c1].[Level2_Optional_Id2], [c1].[Level2_Required_Id1], [c1].[Level2_Required_Id2], [c1].[Name], [c1].[OneToMany_Optional_Inverse3Id1], [c1].[OneToMany_Optional_Inverse3Id2], [c1].[OneToMany_Optional_Self_Inverse3Id1], [c1].[OneToMany_Optional_Self_Inverse3Id2], [c1].[OneToMany_Required_Inverse3Id1], [c1].[OneToMany_Required_Inverse3Id2], [c1].[OneToMany_Required_Self_Inverse3Id1], [c1].[OneToMany_Required_Self_Inverse3Id2], [c1].[OneToOne_Optional_PK_Inverse3Id1], [c1].[OneToOne_Optional_PK_Inverse3Id2], [c1].[OneToOne_Optional_Self3Id1], [c1].[OneToOne_Optional_Self3Id2], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Optional_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c1] ON [c0].[Id1] = [c1].[OneToMany_Required_Inverse3Id1] AND [c0].[Id2] = [c1].[OneToMany_Required_Inverse3Id2]
ORDER BY [c].[Id2], [c].[Id1], [c0].[Id2], [c0].[Id1], [c1].[Id2] DESC");
        }

        public override async Task Projecting_multiple_collections_on_multiple_levels_no_explicit_ordering(bool async)
        {
            await base.Projecting_multiple_collections_on_multiple_levels_no_explicit_ordering(async);

            AssertSql(
                @"SELECT [c].[Name], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
ORDER BY [c].[Id1], [c].[Id2]",
                //
                @"SELECT [c0].[Name], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Optional_Inverse2Id2]
ORDER BY [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2]",
                //
                @"SELECT [c1].[Name], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Optional_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c1] ON [c0].[Id1] = [c1].[OneToMany_Required_Inverse3Id1] AND [c0].[Id2] = [c1].[OneToMany_Required_Inverse3Id2]
ORDER BY [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]",
                //
                @"SELECT [c1].[Name], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Optional_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c1] ON [c0].[Id1] = [c1].[OneToMany_Optional_Inverse3Id1] AND [c0].[Id2] = [c1].[OneToMany_Optional_Inverse3Id2]
ORDER BY [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]",
                //
                @"SELECT [c2].[Id1], [c2].[Id2], [c2].[Level3_Optional_Id1], [c2].[Level3_Optional_Id2], [c2].[Level3_Required_Id1], [c2].[Level3_Required_Id2], [c2].[Name], [c2].[OneToMany_Optional_Inverse4Id1], [c2].[OneToMany_Optional_Inverse4Id2], [c2].[OneToMany_Optional_Self_Inverse4Id1], [c2].[OneToMany_Optional_Self_Inverse4Id2], [c2].[OneToMany_Required_Inverse4Id1], [c2].[OneToMany_Required_Inverse4Id2], [c2].[OneToMany_Required_Self_Inverse4Id1], [c2].[OneToMany_Required_Self_Inverse4Id2], [c2].[OneToOne_Optional_PK_Inverse4Id1], [c2].[OneToOne_Optional_PK_Inverse4Id2], [c2].[OneToOne_Optional_Self4Id1], [c2].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Optional_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c1] ON [c0].[Id1] = [c1].[OneToMany_Optional_Inverse3Id1] AND [c0].[Id2] = [c1].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c2] ON [c1].[Id1] = [c2].[OneToMany_Required_Inverse4Id1] AND [c1].[Id2] = [c2].[OneToMany_Required_Inverse4Id2]
ORDER BY [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]",
                //
                @"SELECT [c2].[Id1], [c2].[Id2], [c2].[Level3_Optional_Id1], [c2].[Level3_Optional_Id2], [c2].[Level3_Required_Id1], [c2].[Level3_Required_Id2], [c2].[Name], [c2].[OneToMany_Optional_Inverse4Id1], [c2].[OneToMany_Optional_Inverse4Id2], [c2].[OneToMany_Optional_Self_Inverse4Id1], [c2].[OneToMany_Optional_Self_Inverse4Id2], [c2].[OneToMany_Required_Inverse4Id1], [c2].[OneToMany_Required_Inverse4Id2], [c2].[OneToMany_Required_Self_Inverse4Id1], [c2].[OneToMany_Required_Self_Inverse4Id2], [c2].[OneToOne_Optional_PK_Inverse4Id1], [c2].[OneToOne_Optional_PK_Inverse4Id2], [c2].[OneToOne_Optional_Self4Id1], [c2].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Optional_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c1] ON [c0].[Id1] = [c1].[OneToMany_Optional_Inverse3Id1] AND [c0].[Id2] = [c1].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c2] ON [c1].[Id1] = [c2].[OneToMany_Optional_Inverse4Id1] AND [c1].[Id2] = [c2].[OneToMany_Optional_Inverse4Id2]
ORDER BY [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]",
                //
                @"SELECT [c0].[Name], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Required_Inverse2Id2]
ORDER BY [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2]",
                //
                @"SELECT [c1].[Name], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c1] ON [c0].[Id1] = [c1].[OneToMany_Optional_Inverse3Id1] AND [c0].[Id2] = [c1].[OneToMany_Optional_Inverse3Id2]
ORDER BY [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]",
                //
                @"SELECT [c2].[Id1], [c2].[Id2], [c2].[Level3_Optional_Id1], [c2].[Level3_Optional_Id2], [c2].[Level3_Required_Id1], [c2].[Level3_Required_Id2], [c2].[Name], [c2].[OneToMany_Optional_Inverse4Id1], [c2].[OneToMany_Optional_Inverse4Id2], [c2].[OneToMany_Optional_Self_Inverse4Id1], [c2].[OneToMany_Optional_Self_Inverse4Id2], [c2].[OneToMany_Required_Inverse4Id1], [c2].[OneToMany_Required_Inverse4Id2], [c2].[OneToMany_Required_Self_Inverse4Id1], [c2].[OneToMany_Required_Self_Inverse4Id2], [c2].[OneToOne_Optional_PK_Inverse4Id1], [c2].[OneToOne_Optional_PK_Inverse4Id2], [c2].[OneToOne_Optional_Self4Id1], [c2].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c1] ON [c0].[Id1] = [c1].[OneToMany_Optional_Inverse3Id1] AND [c0].[Id2] = [c1].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c2] ON [c1].[Id1] = [c2].[OneToMany_Required_Inverse4Id1] AND [c1].[Id2] = [c2].[OneToMany_Required_Inverse4Id2]
ORDER BY [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]",
                //
                @"SELECT [c2].[Id1], [c2].[Id2], [c2].[Level3_Optional_Id1], [c2].[Level3_Optional_Id2], [c2].[Level3_Required_Id1], [c2].[Level3_Required_Id2], [c2].[Name], [c2].[OneToMany_Optional_Inverse4Id1], [c2].[OneToMany_Optional_Inverse4Id2], [c2].[OneToMany_Optional_Self_Inverse4Id1], [c2].[OneToMany_Optional_Self_Inverse4Id2], [c2].[OneToMany_Required_Inverse4Id1], [c2].[OneToMany_Required_Inverse4Id2], [c2].[OneToMany_Required_Self_Inverse4Id1], [c2].[OneToMany_Required_Self_Inverse4Id2], [c2].[OneToOne_Optional_PK_Inverse4Id1], [c2].[OneToOne_Optional_PK_Inverse4Id2], [c2].[OneToOne_Optional_Self4Id1], [c2].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c1] ON [c0].[Id1] = [c1].[OneToMany_Optional_Inverse3Id1] AND [c0].[Id2] = [c1].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c2] ON [c1].[Id1] = [c2].[OneToMany_Optional_Inverse4Id1] AND [c1].[Id2] = [c2].[OneToMany_Optional_Inverse4Id2]
ORDER BY [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]",
                //
                @"SELECT [c1].[Name], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c1] ON [c0].[Id1] = [c1].[OneToMany_Optional_Inverse3Id1] AND [c0].[Id2] = [c1].[OneToMany_Optional_Inverse3Id2]
ORDER BY [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]",
                //
                @"SELECT [c2].[Id1], [c2].[Id2], [c2].[Level3_Optional_Id1], [c2].[Level3_Optional_Id2], [c2].[Level3_Required_Id1], [c2].[Level3_Required_Id2], [c2].[Name], [c2].[OneToMany_Optional_Inverse4Id1], [c2].[OneToMany_Optional_Inverse4Id2], [c2].[OneToMany_Optional_Self_Inverse4Id1], [c2].[OneToMany_Optional_Self_Inverse4Id2], [c2].[OneToMany_Required_Inverse4Id1], [c2].[OneToMany_Required_Inverse4Id2], [c2].[OneToMany_Required_Self_Inverse4Id1], [c2].[OneToMany_Required_Self_Inverse4Id2], [c2].[OneToOne_Optional_PK_Inverse4Id1], [c2].[OneToOne_Optional_PK_Inverse4Id2], [c2].[OneToOne_Optional_Self4Id1], [c2].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c1] ON [c0].[Id1] = [c1].[OneToMany_Optional_Inverse3Id1] AND [c0].[Id2] = [c1].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c2] ON [c1].[Id1] = [c2].[OneToMany_Optional_Inverse4Id1] AND [c1].[Id2] = [c2].[OneToMany_Optional_Inverse4Id2]
ORDER BY [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]",
                //
                @"SELECT [c2].[Id1], [c2].[Id2], [c2].[Level3_Optional_Id1], [c2].[Level3_Optional_Id2], [c2].[Level3_Required_Id1], [c2].[Level3_Required_Id2], [c2].[Name], [c2].[OneToMany_Optional_Inverse4Id1], [c2].[OneToMany_Optional_Inverse4Id2], [c2].[OneToMany_Optional_Self_Inverse4Id1], [c2].[OneToMany_Optional_Self_Inverse4Id2], [c2].[OneToMany_Required_Inverse4Id1], [c2].[OneToMany_Required_Inverse4Id2], [c2].[OneToMany_Required_Self_Inverse4Id1], [c2].[OneToMany_Required_Self_Inverse4Id2], [c2].[OneToOne_Optional_PK_Inverse4Id1], [c2].[OneToOne_Optional_PK_Inverse4Id2], [c2].[OneToOne_Optional_Self4Id1], [c2].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c1] ON [c0].[Id1] = [c1].[OneToMany_Optional_Inverse3Id1] AND [c0].[Id2] = [c1].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c2] ON [c1].[Id1] = [c2].[OneToMany_Required_Inverse4Id1] AND [c1].[Id2] = [c2].[OneToMany_Required_Inverse4Id2]
ORDER BY [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]");
        }

        public override async Task Projecting_multiple_collections_on_multiple_levels_some_explicit_ordering(bool async)
        {
            await base.Projecting_multiple_collections_on_multiple_levels_some_explicit_ordering(async);

            AssertSql(
                @"SELECT [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2]",
                //
                @"SELECT [c0].[Name], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Optional_Inverse2Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2]",
                //
                @"SELECT [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Optional_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c1] ON [c0].[Id1] = [c1].[OneToMany_Required_Inverse3Id1] AND [c0].[Id2] = [c1].[OneToMany_Required_Inverse3Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id2] DESC, [c1].[Id1] DESC",
                //
                @"SELECT [c1].[Name], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Optional_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c1] ON [c0].[Id1] = [c1].[OneToMany_Optional_Inverse3Id1] AND [c0].[Id2] = [c1].[OneToMany_Optional_Inverse3Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]",
                //
                @"SELECT [c2].[Id1], [c2].[Id2], [c2].[Level3_Optional_Id1], [c2].[Level3_Optional_Id2], [c2].[Level3_Required_Id1], [c2].[Level3_Required_Id2], [c2].[Name], [c2].[OneToMany_Optional_Inverse4Id1], [c2].[OneToMany_Optional_Inverse4Id2], [c2].[OneToMany_Optional_Self_Inverse4Id1], [c2].[OneToMany_Optional_Self_Inverse4Id2], [c2].[OneToMany_Required_Inverse4Id1], [c2].[OneToMany_Required_Inverse4Id2], [c2].[OneToMany_Required_Self_Inverse4Id1], [c2].[OneToMany_Required_Self_Inverse4Id2], [c2].[OneToOne_Optional_PK_Inverse4Id1], [c2].[OneToOne_Optional_PK_Inverse4Id2], [c2].[OneToOne_Optional_Self4Id1], [c2].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Optional_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c1] ON [c0].[Id1] = [c1].[OneToMany_Optional_Inverse3Id1] AND [c0].[Id2] = [c1].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c2] ON [c1].[Id1] = [c2].[OneToMany_Required_Inverse4Id1] AND [c1].[Id2] = [c2].[OneToMany_Required_Inverse4Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]",
                //
                @"SELECT [c2].[Id1], [c2].[Id2], [c2].[Level3_Optional_Id1], [c2].[Level3_Optional_Id2], [c2].[Level3_Required_Id1], [c2].[Level3_Required_Id2], [c2].[Name], [c2].[OneToMany_Optional_Inverse4Id1], [c2].[OneToMany_Optional_Inverse4Id2], [c2].[OneToMany_Optional_Self_Inverse4Id1], [c2].[OneToMany_Optional_Self_Inverse4Id2], [c2].[OneToMany_Required_Inverse4Id1], [c2].[OneToMany_Required_Inverse4Id2], [c2].[OneToMany_Required_Self_Inverse4Id1], [c2].[OneToMany_Required_Self_Inverse4Id2], [c2].[OneToOne_Optional_PK_Inverse4Id1], [c2].[OneToOne_Optional_PK_Inverse4Id2], [c2].[OneToOne_Optional_Self4Id1], [c2].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Optional_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c1] ON [c0].[Id1] = [c1].[OneToMany_Optional_Inverse3Id1] AND [c0].[Id2] = [c1].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c2] ON [c1].[Id1] = [c2].[OneToMany_Optional_Inverse4Id1] AND [c1].[Id2] = [c2].[OneToMany_Optional_Inverse4Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]",
                //
                @"SELECT [c0].[Name], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Required_Inverse2Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], CAST(LEN([c0].[Name]) AS int), [c0].[Id1], [c0].[Id2]",
                //
                @"SELECT [c1].[Name], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c1] ON [c0].[Id1] = [c1].[OneToMany_Optional_Inverse3Id1] AND [c0].[Id2] = [c1].[OneToMany_Optional_Inverse3Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], CAST(LEN([c0].[Name]) AS int), [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]",
                //
                @"SELECT [c2].[Id1], [c2].[Id2], [c2].[Level3_Optional_Id1], [c2].[Level3_Optional_Id2], [c2].[Level3_Required_Id1], [c2].[Level3_Required_Id2], [c2].[Name], [c2].[OneToMany_Optional_Inverse4Id1], [c2].[OneToMany_Optional_Inverse4Id2], [c2].[OneToMany_Optional_Self_Inverse4Id1], [c2].[OneToMany_Optional_Self_Inverse4Id2], [c2].[OneToMany_Required_Inverse4Id1], [c2].[OneToMany_Required_Inverse4Id2], [c2].[OneToMany_Required_Self_Inverse4Id1], [c2].[OneToMany_Required_Self_Inverse4Id2], [c2].[OneToOne_Optional_PK_Inverse4Id1], [c2].[OneToOne_Optional_PK_Inverse4Id2], [c2].[OneToOne_Optional_Self4Id1], [c2].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c1] ON [c0].[Id1] = [c1].[OneToMany_Optional_Inverse3Id1] AND [c0].[Id2] = [c1].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c2] ON [c1].[Id1] = [c2].[OneToMany_Required_Inverse4Id1] AND [c1].[Id2] = [c2].[OneToMany_Required_Inverse4Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], CAST(LEN([c0].[Name]) AS int), [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]",
                //
                @"SELECT [c2].[Id1], [c2].[Id2], [c2].[Level3_Optional_Id1], [c2].[Level3_Optional_Id2], [c2].[Level3_Required_Id1], [c2].[Level3_Required_Id2], [c2].[Name], [c2].[OneToMany_Optional_Inverse4Id1], [c2].[OneToMany_Optional_Inverse4Id2], [c2].[OneToMany_Optional_Self_Inverse4Id1], [c2].[OneToMany_Optional_Self_Inverse4Id2], [c2].[OneToMany_Required_Inverse4Id1], [c2].[OneToMany_Required_Inverse4Id2], [c2].[OneToMany_Required_Self_Inverse4Id1], [c2].[OneToMany_Required_Self_Inverse4Id2], [c2].[OneToOne_Optional_PK_Inverse4Id1], [c2].[OneToOne_Optional_PK_Inverse4Id2], [c2].[OneToOne_Optional_Self4Id1], [c2].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c1] ON [c0].[Id1] = [c1].[OneToMany_Optional_Inverse3Id1] AND [c0].[Id2] = [c1].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c2] ON [c1].[Id1] = [c2].[OneToMany_Optional_Inverse4Id1] AND [c1].[Id2] = [c2].[OneToMany_Optional_Inverse4Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], CAST(LEN([c0].[Name]) AS int), [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]",
                //
                @"SELECT [c1].[Name], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c1] ON [c0].[Id1] = [c1].[OneToMany_Optional_Inverse3Id1] AND [c0].[Id2] = [c1].[OneToMany_Optional_Inverse3Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], CAST(LEN([c0].[Name]) AS int), [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]",
                //
                @"SELECT [c2].[Id1], [c2].[Id2], [c2].[Level3_Optional_Id1], [c2].[Level3_Optional_Id2], [c2].[Level3_Required_Id1], [c2].[Level3_Required_Id2], [c2].[Name], [c2].[OneToMany_Optional_Inverse4Id1], [c2].[OneToMany_Optional_Inverse4Id2], [c2].[OneToMany_Optional_Self_Inverse4Id1], [c2].[OneToMany_Optional_Self_Inverse4Id2], [c2].[OneToMany_Required_Inverse4Id1], [c2].[OneToMany_Required_Inverse4Id2], [c2].[OneToMany_Required_Self_Inverse4Id1], [c2].[OneToMany_Required_Self_Inverse4Id2], [c2].[OneToOne_Optional_PK_Inverse4Id1], [c2].[OneToOne_Optional_PK_Inverse4Id2], [c2].[OneToOne_Optional_Self4Id1], [c2].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c1] ON [c0].[Id1] = [c1].[OneToMany_Optional_Inverse3Id1] AND [c0].[Id2] = [c1].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c2] ON [c1].[Id1] = [c2].[OneToMany_Optional_Inverse4Id1] AND [c1].[Id2] = [c2].[OneToMany_Optional_Inverse4Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], CAST(LEN([c0].[Name]) AS int), [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]",
                //
                @"SELECT [c2].[Id1], [c2].[Id2], [c2].[Level3_Optional_Id1], [c2].[Level3_Optional_Id2], [c2].[Level3_Required_Id1], [c2].[Level3_Required_Id2], [c2].[Name], [c2].[OneToMany_Optional_Inverse4Id1], [c2].[OneToMany_Optional_Inverse4Id2], [c2].[OneToMany_Optional_Self_Inverse4Id1], [c2].[OneToMany_Optional_Self_Inverse4Id2], [c2].[OneToMany_Required_Inverse4Id1], [c2].[OneToMany_Required_Inverse4Id2], [c2].[OneToMany_Required_Self_Inverse4Id1], [c2].[OneToMany_Required_Self_Inverse4Id2], [c2].[OneToOne_Optional_PK_Inverse4Id1], [c2].[OneToOne_Optional_PK_Inverse4Id2], [c2].[OneToOne_Optional_Self4Id1], [c2].[OneToOne_Optional_Self4Id2], [c].[Id1], [c].[Id2], [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Required_Inverse2Id2]
INNER JOIN [CompositeThrees] AS [c1] ON [c0].[Id1] = [c1].[OneToMany_Optional_Inverse3Id1] AND [c0].[Id2] = [c1].[OneToMany_Optional_Inverse3Id2]
INNER JOIN [CompositeFours] AS [c2] ON [c1].[Id1] = [c2].[OneToMany_Required_Inverse4Id1] AND [c1].[Id2] = [c2].[OneToMany_Required_Inverse4Id2]
ORDER BY [c].[Name], [c].[Id1], [c].[Id2], CAST(LEN([c0].[Name]) AS int), [c0].[Id1], [c0].[Id2], [c1].[Id1], [c1].[Id2], [c2].[Id1] + CAST([c2].[Id2] AS nvarchar(450)) DESC");
        }

        public override async Task Projecting_multiple_collections_same_level_top_level_ordering(bool async)
        {
            await base.Projecting_multiple_collections_same_level_top_level_ordering(async);

            AssertSql(
                @"SELECT [c].[Name], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
ORDER BY [c].[Id2], [c].[Id1]",
                //
                @"SELECT [c0].[Id1], [c0].[Id2], [c0].[Date], [c0].[Level1_Optional_Id1], [c0].[Level1_Optional_Id2], [c0].[Level1_Required_Id1], [c0].[Level1_Required_Id2], [c0].[Name], [c0].[OneToMany_Optional_Inverse2Id1], [c0].[OneToMany_Optional_Inverse2Id2], [c0].[OneToMany_Optional_Self_Inverse2Id1], [c0].[OneToMany_Optional_Self_Inverse2Id2], [c0].[OneToMany_Required_Inverse2Id1], [c0].[OneToMany_Required_Inverse2Id2], [c0].[OneToMany_Required_Self_Inverse2Id1], [c0].[OneToMany_Required_Self_Inverse2Id2], [c0].[OneToOne_Optional_PK_Inverse2Id1], [c0].[OneToOne_Optional_PK_Inverse2Id2], [c0].[OneToOne_Optional_Self2Id1], [c0].[OneToOne_Optional_Self2Id2], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Optional_Inverse2Id2]
ORDER BY [c].[Id2], [c].[Id1]",
                //
                @"SELECT [c0].[Id1], [c0].[Id2], [c0].[Date], [c0].[Level1_Optional_Id1], [c0].[Level1_Optional_Id2], [c0].[Level1_Required_Id1], [c0].[Level1_Required_Id2], [c0].[Name], [c0].[OneToMany_Optional_Inverse2Id1], [c0].[OneToMany_Optional_Inverse2Id2], [c0].[OneToMany_Optional_Self_Inverse2Id1], [c0].[OneToMany_Optional_Self_Inverse2Id2], [c0].[OneToMany_Required_Inverse2Id1], [c0].[OneToMany_Required_Inverse2Id2], [c0].[OneToMany_Required_Self_Inverse2Id1], [c0].[OneToMany_Required_Self_Inverse2Id2], [c0].[OneToOne_Optional_PK_Inverse2Id1], [c0].[OneToOne_Optional_PK_Inverse2Id2], [c0].[OneToOne_Optional_Self2Id1], [c0].[OneToOne_Optional_Self2Id2], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Required_Inverse2Id2]
ORDER BY [c].[Id2], [c].[Id1]");
        }

        public override async Task Projecting_multiple_collections_same_level_top_level_ordering_using_entire_composite_key(bool async)
        {
            await base.Projecting_multiple_collections_same_level_top_level_ordering_using_entire_composite_key(async);

            AssertSql(
                @"SELECT [c].[Name], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
ORDER BY [c].[Id2], [c].[Id1] DESC",
                //
                @"SELECT [c0].[Id1], [c0].[Id2], [c0].[Date], [c0].[Level1_Optional_Id1], [c0].[Level1_Optional_Id2], [c0].[Level1_Required_Id1], [c0].[Level1_Required_Id2], [c0].[Name], [c0].[OneToMany_Optional_Inverse2Id1], [c0].[OneToMany_Optional_Inverse2Id2], [c0].[OneToMany_Optional_Self_Inverse2Id1], [c0].[OneToMany_Optional_Self_Inverse2Id2], [c0].[OneToMany_Required_Inverse2Id1], [c0].[OneToMany_Required_Inverse2Id2], [c0].[OneToMany_Required_Self_Inverse2Id1], [c0].[OneToMany_Required_Self_Inverse2Id2], [c0].[OneToOne_Optional_PK_Inverse2Id1], [c0].[OneToOne_Optional_PK_Inverse2Id2], [c0].[OneToOne_Optional_Self2Id1], [c0].[OneToOne_Optional_Self2Id2], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Optional_Inverse2Id2]
ORDER BY [c].[Id2], [c].[Id1] DESC",
                //
                @"SELECT [c0].[Id1], [c0].[Id2], [c0].[Date], [c0].[Level1_Optional_Id1], [c0].[Level1_Optional_Id2], [c0].[Level1_Required_Id1], [c0].[Level1_Required_Id2], [c0].[Name], [c0].[OneToMany_Optional_Inverse2Id1], [c0].[OneToMany_Optional_Inverse2Id2], [c0].[OneToMany_Optional_Self_Inverse2Id1], [c0].[OneToMany_Optional_Self_Inverse2Id2], [c0].[OneToMany_Required_Inverse2Id1], [c0].[OneToMany_Required_Inverse2Id2], [c0].[OneToMany_Required_Self_Inverse2Id1], [c0].[OneToMany_Required_Self_Inverse2Id2], [c0].[OneToOne_Optional_PK_Inverse2Id1], [c0].[OneToOne_Optional_PK_Inverse2Id2], [c0].[OneToOne_Optional_Self2Id1], [c0].[OneToOne_Optional_Self2Id2], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Required_Inverse2Id2]
ORDER BY [c].[Id2], [c].[Id1] DESC");
        }

        public override async Task Projecting_multiple_collections_with_ordering_same_level(bool async)
        {
            await base.Projecting_multiple_collections_with_ordering_same_level(async);

            AssertSql(
                @"SELECT [c].[Name], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
ORDER BY [c].[Id1], [c].[Id2]",
                //
                @"SELECT [c0].[Id1], [c0].[Id2], [c0].[Date], [c0].[Level1_Optional_Id1], [c0].[Level1_Optional_Id2], [c0].[Level1_Required_Id1], [c0].[Level1_Required_Id2], [c0].[Name], [c0].[OneToMany_Optional_Inverse2Id1], [c0].[OneToMany_Optional_Inverse2Id2], [c0].[OneToMany_Optional_Self_Inverse2Id1], [c0].[OneToMany_Optional_Self_Inverse2Id2], [c0].[OneToMany_Required_Inverse2Id1], [c0].[OneToMany_Required_Inverse2Id2], [c0].[OneToMany_Required_Self_Inverse2Id1], [c0].[OneToMany_Required_Self_Inverse2Id2], [c0].[OneToOne_Optional_PK_Inverse2Id1], [c0].[OneToOne_Optional_PK_Inverse2Id2], [c0].[OneToOne_Optional_Self2Id1], [c0].[OneToOne_Optional_Self2Id2], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Optional_Inverse2Id2]
ORDER BY [c].[Id1], [c].[Id2], [c0].[Id2]",
                //
                @"SELECT [c0].[Id1], [c0].[Id2], [c0].[Date], [c0].[Level1_Optional_Id1], [c0].[Level1_Optional_Id2], [c0].[Level1_Required_Id1], [c0].[Level1_Required_Id2], [c0].[Name], [c0].[OneToMany_Optional_Inverse2Id1], [c0].[OneToMany_Optional_Inverse2Id2], [c0].[OneToMany_Optional_Self_Inverse2Id1], [c0].[OneToMany_Optional_Self_Inverse2Id2], [c0].[OneToMany_Required_Inverse2Id1], [c0].[OneToMany_Required_Inverse2Id2], [c0].[OneToMany_Required_Self_Inverse2Id1], [c0].[OneToMany_Required_Self_Inverse2Id2], [c0].[OneToOne_Optional_PK_Inverse2Id1], [c0].[OneToOne_Optional_PK_Inverse2Id2], [c0].[OneToOne_Optional_Self2Id1], [c0].[OneToOne_Optional_Self2Id2], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Required_Inverse2Id2]
ORDER BY [c].[Id1], [c].[Id2], [c0].[Name] DESC");
        }

        public override async Task Projecting_multiple_collections_with_ordering_same_level_top_level_ordering(bool async)
        {
            await base.Projecting_multiple_collections_with_ordering_same_level_top_level_ordering(async);

            AssertSql(
                @"SELECT [c].[Name], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
ORDER BY [c].[Id2], [c].[Id1]",
                //
                @"SELECT [c0].[Id1], [c0].[Id2], [c0].[Date], [c0].[Level1_Optional_Id1], [c0].[Level1_Optional_Id2], [c0].[Level1_Required_Id1], [c0].[Level1_Required_Id2], [c0].[Name], [c0].[OneToMany_Optional_Inverse2Id1], [c0].[OneToMany_Optional_Inverse2Id2], [c0].[OneToMany_Optional_Self_Inverse2Id1], [c0].[OneToMany_Optional_Self_Inverse2Id2], [c0].[OneToMany_Required_Inverse2Id1], [c0].[OneToMany_Required_Inverse2Id2], [c0].[OneToMany_Required_Self_Inverse2Id1], [c0].[OneToMany_Required_Self_Inverse2Id2], [c0].[OneToOne_Optional_PK_Inverse2Id1], [c0].[OneToOne_Optional_PK_Inverse2Id2], [c0].[OneToOne_Optional_Self2Id1], [c0].[OneToOne_Optional_Self2Id2], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Optional_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Optional_Inverse2Id2]
ORDER BY [c].[Id2], [c].[Id1], [c0].[Id2]",
                //
                @"SELECT [c0].[Id1], [c0].[Id2], [c0].[Date], [c0].[Level1_Optional_Id1], [c0].[Level1_Optional_Id2], [c0].[Level1_Required_Id1], [c0].[Level1_Required_Id2], [c0].[Name], [c0].[OneToMany_Optional_Inverse2Id1], [c0].[OneToMany_Optional_Inverse2Id2], [c0].[OneToMany_Optional_Self_Inverse2Id1], [c0].[OneToMany_Optional_Self_Inverse2Id2], [c0].[OneToMany_Required_Inverse2Id1], [c0].[OneToMany_Required_Inverse2Id2], [c0].[OneToMany_Required_Self_Inverse2Id1], [c0].[OneToMany_Required_Self_Inverse2Id2], [c0].[OneToOne_Optional_PK_Inverse2Id1], [c0].[OneToOne_Optional_PK_Inverse2Id2], [c0].[OneToOne_Optional_Self2Id1], [c0].[OneToOne_Optional_Self2Id2], [c].[Id1], [c].[Id2]
FROM [CompositeOnes] AS [c]
INNER JOIN [CompositeTwos] AS [c0] ON [c].[Id1] = [c0].[OneToMany_Required_Inverse2Id1] AND [c].[Id2] = [c0].[OneToMany_Required_Inverse2Id2]
ORDER BY [c].[Id2], [c].[Id1], [c0].[Name] DESC");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
