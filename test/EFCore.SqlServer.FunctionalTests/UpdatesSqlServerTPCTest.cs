// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using static Microsoft.EntityFrameworkCore.UpdatesSqlServerTPCTest;

#nullable enable

namespace Microsoft.EntityFrameworkCore;

public class UpdatesSqlServerTPCTest : UpdatesRelationalTestBase<UpdatesSqlServerTPCTest.UpdatesSqlServerTPTFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public UpdatesSqlServerTPCTest(UpdatesSqlServerTPTFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        Fixture.TestSqlLoggerFactory.Clear();
    }

    [ConditionalFact]
    public override void Save_with_shared_foreign_key()
    {
        base.Save_with_shared_foreign_key();

        AssertContainsSql(
            @"@p0=NULL (Size = 8000) (DbType = Binary)
@p1='ProductWithBytes' (Nullable = false) (Size = 4000)
@p2=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [ProductBase] ([Bytes], [Discriminator], [ProductWithBytes_Name])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2);",
            @"@p0=NULL (Size = 4000)
@p1='777'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [SpecialCategory] ([Name], [PrincipalId])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1);");
    }

    [ConditionalFact]
    public override void Can_add_and_remove_self_refs()
    {
        base.Can_add_and_remove_self_refs();

        AssertContainsSql(
                @"@p0='1' (Nullable = false) (Size = 4000)
@p1=NULL (DbType = Int32)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [Person] ([Name], [ParentId])
OUTPUT INSERTED.[PersonId]
VALUES (@p0, @p1);",
                //
                @"@p2='2' (Nullable = false) (Size = 4000)
@p3='1' (Nullable = true)
@p4='3' (Nullable = false) (Size = 4000)
@p5='1' (Nullable = true)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
MERGE [Person] USING (
VALUES (@p2, @p3, 0),
(@p4, @p5, 1)) AS i ([Name], [ParentId], _Position) ON 1=0
WHEN NOT MATCHED THEN
INSERT ([Name], [ParentId])
VALUES (i.[Name], i.[ParentId])
OUTPUT INSERTED.[PersonId], i._Position;",
                //
                @"@p6='4' (Nullable = false) (Size = 4000)
@p7='2' (Nullable = true)
@p8='5' (Nullable = false) (Size = 4000)
@p9='2' (Nullable = true)
@p10='6' (Nullable = false) (Size = 4000)
@p11='3' (Nullable = true)
@p12='7' (Nullable = false) (Size = 4000)
@p13='3' (Nullable = true)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
MERGE [Person] USING (
VALUES (@p6, @p7, 0),
(@p8, @p9, 1),
(@p10, @p11, 2),
(@p12, @p13, 3)) AS i ([Name], [ParentId], _Position) ON 1=0
WHEN NOT MATCHED THEN
INSERT ([Name], [ParentId])
VALUES (i.[Name], i.[ParentId])
OUTPUT INSERTED.[PersonId], i._Position;");
    }

    public override void Save_replaced_principal()
    {
        base.Save_replaced_principal();

        AssertSql(
            @"SELECT TOP(2) [t].[Id], [t].[Name], [t].[PrincipalId], [t].[Discriminator]
FROM (
    SELECT [c].[Id], [c].[Name], [c].[PrincipalId], N'Category' AS [Discriminator]
    FROM [Categories] AS [c]
    UNION ALL
    SELECT [s].[Id], [s].[Name], [s].[PrincipalId], N'SpecialCategory' AS [Discriminator]
    FROM [SpecialCategory] AS [s]
) AS [t]",
            //
            @"@__category_PrincipalId_0='778' (Nullable = true)

SELECT [p].[Id], [p].[Discriminator], [p].[DependentId], [p].[Name], [p].[Price]
FROM [ProductBase] AS [p]
WHERE [p].[Discriminator] = N'Product' AND [p].[DependentId] = @__category_PrincipalId_0",
            //
            @"@p1='1'
@p0='New Category' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [Categories] SET [Name] = @p0
OUTPUT 1
WHERE [Id] = @p1;",
            //
            @"SELECT TOP(2) [t].[Id], [t].[Name], [t].[PrincipalId], [t].[Discriminator]
FROM (
    SELECT [c].[Id], [c].[Name], [c].[PrincipalId], N'Category' AS [Discriminator]
    FROM [Categories] AS [c]
    UNION ALL
    SELECT [s].[Id], [s].[Name], [s].[PrincipalId], N'SpecialCategory' AS [Discriminator]
    FROM [SpecialCategory] AS [s]
) AS [t]",
            //
            @"@__category_PrincipalId_0='778' (Nullable = true)

SELECT [p].[Id], [p].[Discriminator], [p].[DependentId], [p].[Name], [p].[Price]
FROM [ProductBase] AS [p]
WHERE [p].[Discriminator] = N'Product' AND [p].[DependentId] = @__category_PrincipalId_0");
    }

    public override void Identifiers_are_generated_correctly()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(
            typeof(
                LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly
            ))!;
        Assert.Equal(
            "LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorking~",
            entityType.GetTableName());
        Assert.Equal(
            "PK_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWork~",
            entityType.GetKeys().Single().GetName());
        Assert.Equal(
            "FK_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWork~",
            entityType.GetForeignKeys().Single().GetConstraintName());
        Assert.Equal(
            "IX_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWork~",
            entityType.GetIndexes().Single().GetDatabaseName());

        var entityType2 = context.Model.FindEntityType(
            typeof(
                LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectlyDetails
            ))!;

        Assert.Equal(
            "LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkin~1",
            entityType2.GetTableName());
        Assert.Equal(
            "PK_LoginDetails",
            entityType2.GetKeys().Single().GetName());
        Assert.Equal(
            "ExtraPropertyWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCo~",
            entityType2.GetProperties().ElementAt(1).GetColumnName(StoreObjectIdentifier.Table(entityType2.GetTableName()!)));
        Assert.Equal(
            "ExtraPropertyWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingC~1",
            entityType2.GetProperties().ElementAt(2).GetColumnName(StoreObjectIdentifier.Table(entityType2.GetTableName()!)));
        Assert.Equal(
            "IX_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWork~",
            entityType2.GetIndexes().Single().GetDatabaseName());
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected void AssertContainsSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, assertOrder: false);

    public class UpdatesSqlServerTPTFixture : UpdatesRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override string StoreName
            => "UpdateTestTPC";

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(
                w =>
                {
                    w.Log(SqlServerEventId.DecimalTypeKeyWarning);
                    w.Log(RelationalEventId.ForeignKeyTpcPrincipalWarning);
                });

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>().HaveColumnType("decimal(18, 2)");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<ProductBase>()
                .Property(p => p.Id).HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<Category>()
                .UseTpcMappingStrategy();
        }
    }
}
