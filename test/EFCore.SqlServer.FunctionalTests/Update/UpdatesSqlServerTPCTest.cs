// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

namespace Microsoft.EntityFrameworkCore.Update;

public class UpdatesSqlServerTPCTest(UpdatesSqlServerTPCTest.UpdatesSqlServerTPCFixture fixture, ITestOutputHelper testOutputHelper)
    : UpdatesSqlServerTestBase<UpdatesSqlServerTPCTest.UpdatesSqlServerTPCFixture>(fixture, testOutputHelper)
{
    public override async Task Save_with_shared_foreign_key()
    {
        await base.Save_with_shared_foreign_key();

        AssertContainsSql(
            @"@p0=NULL (Size = 8000) (DbType = Binary)
@p1='ProductWithBytes' (Nullable = false) (Size = 21)
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

    public override async Task Save_replaced_principal()
    {
        await base.Save_replaced_principal();

        AssertSql(
            """
SELECT TOP(2) [u].[Id], [u].[Name], [u].[PrincipalId], [u].[Discriminator]
FROM (
    SELECT [c].[Id], [c].[Name], [c].[PrincipalId], N'Category' AS [Discriminator]
    FROM [Categories] AS [c]
    UNION ALL
    SELECT [s].[Id], [s].[Name], [s].[PrincipalId], N'SpecialCategory' AS [Discriminator]
    FROM [SpecialCategory] AS [s]
) AS [u]
""",
            //
            """
@__category_PrincipalId_0='778' (Nullable = true)

SELECT [p].[Id], [p].[Discriminator], [p].[DependentId], [p].[Name], [p].[Price]
FROM [ProductBase] AS [p]
WHERE [p].[Discriminator] = N'Product' AND [p].[DependentId] = @__category_PrincipalId_0
""",
            //
            """
@p1='1'
@p0='New Category' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [Categories] SET [Name] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT TOP(2) [u].[Id], [u].[Name], [u].[PrincipalId], [u].[Discriminator]
FROM (
    SELECT [c].[Id], [c].[Name], [c].[PrincipalId], N'Category' AS [Discriminator]
    FROM [Categories] AS [c]
    UNION ALL
    SELECT [s].[Id], [s].[Name], [s].[PrincipalId], N'SpecialCategory' AS [Discriminator]
    FROM [SpecialCategory] AS [s]
) AS [u]
""",
            //
            """
@__category_PrincipalId_0='778' (Nullable = true)

SELECT [p].[Id], [p].[Discriminator], [p].[DependentId], [p].[Name], [p].[Price]
FROM [ProductBase] AS [p]
WHERE [p].[Discriminator] = N'Product' AND [p].[DependentId] = @__category_PrincipalId_0
""");
    }

    public class UpdatesSqlServerTPCFixture : UpdatesSqlServerFixtureBase
    {
        protected override string StoreName
            => "UpdateTestTPC";

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(
                w =>
                {
                    w.Log(RelationalEventId.ForeignKeyTpcPrincipalWarning);
                });

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Category>().UseTpcMappingStrategy();
            modelBuilder.Entity<GiftObscurer>().UseTpcMappingStrategy();
            modelBuilder.Entity<LiftObscurer>().UseTpcMappingStrategy();
            modelBuilder.Entity<Baked>().UseTpcMappingStrategy();
        }
    }
}
