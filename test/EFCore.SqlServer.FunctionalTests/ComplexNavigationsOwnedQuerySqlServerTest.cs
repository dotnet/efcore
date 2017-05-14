// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class ComplexNavigationsOwnedQuerySqlServerTest
        : ComplexNavigationsOwnedQueryTestBase<SqlServerTestStore, ComplexNavigationsOwnedQuerySqlServerFixture>
    {
        public ComplexNavigationsOwnedQuerySqlServerTest(
            ComplexNavigationsOwnedQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        // TODO: Assert SQL

        [Fact]
        public override void Simple_owned_level1()
        {
            base.Simple_owned_level1();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[OneToOne_Required_PK_Level1_Optional_Id], [l1].[OneToOne_Required_PK_Level1_Required_Id], [l1].[OneToOne_Required_PK_Name], [l1].[OneToOne_Required_PK_OneToOne_Optional_PK_InverseId]
FROM [Level1] AS [l1]");
        }

        [Fact]
        public override void Simple_owned_level1_level2()
        {
            base.Simple_owned_level1_level2();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[OneToOne_Required_PK_Level1_Optional_Id], [l1].[OneToOne_Required_PK_Level1_Required_Id], [l1].[OneToOne_Required_PK_Name], [l1].[OneToOne_Required_PK_OneToOne_Optional_PK_InverseId], [l1].[Id], [l1].[OneToOne_Required_PK_OneToOne_Required_PK_Level2_Optional_Id], [l1].[OneToOne_Required_PK_OneToOne_Required_PK_Level2_Required_Id], [l1].[OneToOne_Required_PK_OneToOne_Required_PK_Name], [l1].[OneToOne_Required_PK_OneToOne_Required_PK_OneToOne_Optional_PK_InverseId]
FROM [Level1] AS [l1]");
        }

        [Fact]
        public override void Simple_owned_level1_level2_level3()
        {
            base.Simple_owned_level1_level2_level3();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[OneToOne_Required_PK_Level1_Optional_Id], [l1].[OneToOne_Required_PK_Level1_Required_Id], [l1].[OneToOne_Required_PK_Name], [l1].[OneToOne_Required_PK_OneToOne_Optional_PK_InverseId], [l1].[Id], [l1].[OneToOne_Required_PK_OneToOne_Required_PK_Level2_Optional_Id], [l1].[OneToOne_Required_PK_OneToOne_Required_PK_Level2_Required_Id], [l1].[OneToOne_Required_PK_OneToOne_Required_PK_Name], [l1].[OneToOne_Required_PK_OneToOne_Required_PK_OneToOne_Optional_PK_InverseId], [l1].[Id], [l1].[OneToOne_Required_PK_OneToOne_Required_PK_OneToOne_Required_PK_Level3_Optional_Id], [l1].[OneToOne_Required_PK_OneToOne_Required_PK_OneToOne_Required_PK_Level3_Required_Id], [l1].[OneToOne_Required_PK_OneToOne_Required_PK_OneToOne_Required_PK_Name], [l1].[OneToOne_Required_PK_OneToOne_Required_PK_OneToOne_Required_PK_OneToOne_Optional_PK_InverseId]
FROM [Level1] AS [l1]");
        }

        [Fact]
        public override void Level4_Include()
        {
            base.Level4_Include();

            AssertSql(
                @"SELECT [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id], [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToOne_Required_PK_Date], [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToOne_Required_PK_Level1_Optional_Id], [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToOne_Required_PK_Level1_Required_Id], [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToOne_Required_PK_Name], [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToOne_Required_PK_OneToOne_Optional_PK_InverseId], [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Id], [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToOne_Required_PK_OneToOne_Required_PK_Level2_Optional_Id], [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToOne_Required_PK_OneToOne_Required_PK_Level2_Required_Id], [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToOne_Required_PK_OneToOne_Required_PK_Name], [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToOne_Required_PK_OneToOne_Required_PK_OneToOne_Optional_PK_InverseId]
FROM [Level1] AS [l1]
LEFT JOIN [Level1] AS [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse] ON [l1].[OneToOne_Required_PK_OneToOne_Required_PK_OneToOne_Required_PK_Level3_Required_Id] = [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse].[Id]
LEFT JOIN [Level1] AS [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse] ON [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse].[OneToOne_Required_PK_OneToOne_Required_PK_Level2_Required_Id] = [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id]
LEFT JOIN [Level1] AS [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK] ON [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id] = [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[OneToOne_Required_PK_OneToOne_Required_PK_Level2_Optional_Id]
WHERE ([l1].[Id] IS NOT NULL AND [l1].[Id] IS NOT NULL) AND [l1].[Id] IS NOT NULL");
        }

        [ConditionalFact(Skip = "issue #4311")]
        public override void Nested_group_join_with_take()
        {
            base.Nested_group_join_with_take();
        }

        [ConditionalFact(Skip = "issue #8492")]
        public override void Explicit_GroupJoin_in_subquery_with_unrelated_projection2()
        {
            base.Explicit_GroupJoin_in_subquery_with_unrelated_projection2();
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
