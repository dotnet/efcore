// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

namespace Microsoft.EntityFrameworkCore;

public class UpdatesSqlServerTPTTest : UpdatesSqlServerTestBase<UpdatesSqlServerTPTTest.UpdatesSqlServerTPTFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public UpdatesSqlServerTPTTest(UpdatesSqlServerTPTFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
    }

    [ConditionalTheory(Skip = "Issue #29874. Skipped because the database is in a bad state, but the test may or may not fail.")]
    public override Task Can_change_type_of_pk_to_pk_dependent_by_replacing_with_new_dependent(bool async)
        => Task.CompletedTask;

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
INSERT INTO [Categories] ([Name], [PrincipalId])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1);");
    }

    public override void Save_replaced_principal()
    {
        base.Save_replaced_principal();

        AssertSql(
"""
SELECT TOP(2) [c].[Id], [c].[Name], [c].[PrincipalId], CASE
    WHEN [s].[Id] IS NOT NULL THEN N'SpecialCategory'
END AS [Discriminator]
FROM [Categories] AS [c]
LEFT JOIN [SpecialCategory] AS [s] ON [c].[Id] = [s].[Id]
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
SELECT TOP(2) [c].[Id], [c].[Name], [c].[PrincipalId], CASE
    WHEN [s].[Id] IS NOT NULL THEN N'SpecialCategory'
END AS [Discriminator]
FROM [Categories] AS [c]
LEFT JOIN [SpecialCategory] AS [s] ON [c].[Id] = [s].[Id]
""",
            //
"""
@__category_PrincipalId_0='778' (Nullable = true)

SELECT [p].[Id], [p].[Discriminator], [p].[DependentId], [p].[Name], [p].[Price]
FROM [ProductBase] AS [p]
WHERE [p].[Discriminator] = N'Product' AND [p].[DependentId] = @__category_PrincipalId_0
""");
    }

    public class UpdatesSqlServerTPTFixture : UpdatesSqlServerFixtureBase
    {
        protected override string StoreName
            => "UpdateTestTPT";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Category>().UseTptMappingStrategy();
            modelBuilder.Entity<GiftObscurer>().UseTptMappingStrategy();
            modelBuilder.Entity<LiftObscurer>().UseTptMappingStrategy();
        }
    }
}
