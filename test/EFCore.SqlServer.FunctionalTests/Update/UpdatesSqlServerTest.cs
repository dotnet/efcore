// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

public class UpdatesSqlServerTest(UpdatesSqlServerTest.UpdatesSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : UpdatesSqlServerTestBase<UpdatesSqlServerTest.UpdatesSqlServerFixture>(fixture, testOutputHelper)
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
            @"@p0='SpecialCategory' (Nullable = false) (Size = 21)
@p1=NULL (Size = 4000)
@p2='777'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [Categories] ([Discriminator], [Name], [PrincipalId])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2);");
    }

    public override async Task Save_replaced_principal()
    {
        await base.Save_replaced_principal();

        AssertSql(
            """
SELECT TOP(2) [c].[Id], [c].[Discriminator], [c].[Name], [c].[PrincipalId]
FROM [Categories] AS [c]
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
SELECT TOP(2) [c].[Id], [c].[Discriminator], [c].[Name], [c].[PrincipalId]
FROM [Categories] AS [c]
""",
            //
            """
@__category_PrincipalId_0='778' (Nullable = true)

SELECT [p].[Id], [p].[Discriminator], [p].[DependentId], [p].[Name], [p].[Price]
FROM [ProductBase] AS [p]
WHERE [p].[Discriminator] = N'Product' AND [p].[DependentId] = @__category_PrincipalId_0
""");
    }

    public class UpdatesSqlServerFixture : UpdatesSqlServerFixtureBase
    {
        protected override string StoreName
            => "UpdateTest";
    }
}
