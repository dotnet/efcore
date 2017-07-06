// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
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

        [Fact]
        public override void Simple_owned_level1()
        {
            base.Simple_owned_level1();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToOne_Optional_PK_InverseId]
FROM [Level1] AS [l1]");
        }
        
        [Fact]
        public override void Simple_owned_level1_convention()
        {
            base.Simple_owned_level1_convention();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name]
FROM [Level1] AS [l]");
        }

        [Fact]
        public override void Simple_owned_level1_level2()
        {
            base.Simple_owned_level1_level2();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToOne_Optional_PK_InverseId], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[Level3_OneToOne_Optional_PK_InverseId]
FROM [Level1] AS [l1]");
        }

        [Fact]
        public override void Simple_owned_level1_level2_level3()
        {
            base.Simple_owned_level1_level2_level3();

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToOne_Optional_PK_InverseId], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[Level3_OneToOne_Optional_PK_InverseId], [l1].[Id], [l1].[Level3_Optional_Id], [l1].[Level3_Required_Id], [l1].[Level4_Name], [l1].[Level4_OneToOne_Optional_PK_InverseId]
FROM [Level1] AS [l1]");
        }

        [Fact]
        public override void Level4_Include()
        {
            base.Level4_Include();

            AssertSql(
                @"SELECT [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id], [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToOne_Required_PK_Date], [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Level1_Optional_Id], [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Level1_Required_Id], [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Level2_Name], [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[OneToOne_Optional_PK_InverseId], [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Id], [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Level2_Optional_Id], [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Level2_Required_Id], [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Level3_Name], [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Level3_OneToOne_Optional_PK_InverseId]
FROM [Level1] AS [l1]
LEFT JOIN [Level1] AS [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse] ON [l1].[Level3_Required_Id] = [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse].[Id]
LEFT JOIN [Level1] AS [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse] ON [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse].[Level2_Required_Id] = [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id]
LEFT JOIN [Level1] AS [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK] ON [l1.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse].[Id] = [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_FK_Inverse.OneToOne_Required_FK_Inverse.OneToOne_Optional_FK].[Level2_Optional_Id]
WHERE ([l1].[Id] IS NOT NULL AND [l1].[Id] IS NOT NULL) AND [l1].[Id] IS NOT NULL");
        }

        [ConditionalFact(Skip = "issue #4311")]
        public override void Nested_group_join_with_take()
        {
            base.Nested_group_join_with_take();
        }

        [ConditionalFact]
        public override void Explicit_GroupJoin_in_subquery_with_unrelated_projection2()
        {
            base.Explicit_GroupJoin_in_subquery_with_unrelated_projection2();

            AssertSql(
                @"SELECT [t1].[Id]
FROM (
    SELECT DISTINCT [l1].*
    FROM [Level1] AS [l1]
    LEFT JOIN (
        SELECT [t].*
        FROM [Level1] AS [t]
        WHERE [t].[Id] IS NOT NULL
    ) AS [t0] ON [l1].[Id] = [t0].[Level1_Optional_Id]
    WHERE ([t0].[Level2_Name] <> N'Foo') OR [t0].[Level2_Name] IS NULL
) AS [t1]");
        }

        [ConditionalFact]
        public override void Result_operator_nav_prop_reference_optional_via_DefaultIfEmpty()
        {
            base.Result_operator_nav_prop_reference_optional_via_DefaultIfEmpty();

            AssertSql(
                @"SELECT SUM(CASE
    WHEN [t0].[Id] IS NULL
    THEN 0 ELSE [t0].[Level1_Required_Id]
END)
FROM [Level1] AS [l1]
LEFT JOIN (
    SELECT [t].*
    FROM [Level1] AS [t]
    WHERE [t].[Id] IS NOT NULL
) AS [t0] ON [l1].[Id] = [t0].[Level1_Optional_Id]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
