// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

namespace Microsoft.EntityFrameworkCore;

public class UpdatesSqlServerTest : UpdatesRelationalTestBase<UpdatesSqlServerFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public UpdatesSqlServerTest(UpdatesSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        Fixture.TestSqlLoggerFactory.Clear();
    }

    [ConditionalFact]
    public virtual void Save_with_shared_foreign_key()
    {
        ExecuteWithStrategyInTransaction(
            context =>
            {
                context.AddRange(
                    new ProductWithBytes { ProductCategories = new List<ProductCategory> { new() { CategoryId = 77 } } },
                    new Category { Id = 77, PrincipalId = 777 });

                context.SaveChanges();
            },
            context =>
            {
                var product = context.Set<ProductBase>()
                    .Include(p => ((ProductWithBytes)p).ProductCategories)
                    .Include(p => ((Product)p).ProductCategories)
                    .OfType<ProductWithBytes>()
                    .Single();
                var productCategory = product.ProductCategories.Single();
                Assert.Equal(productCategory.CategoryId, context.Set<ProductCategory>().Single().CategoryId);
                Assert.Equal(productCategory.CategoryId, context.Set<Category>().Single(c => c.PrincipalId == 777).Id);
            });

        AssertContainsSql(
            @"@p0='77'
@p1=NULL (Size = 4000)
@p2='777'
@p3=NULL (Size = 8000) (DbType = Binary)
@p4='ProductWithBytes' (Nullable = false) (Size = 4000)
@p5=NULL (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [Categories] ([Id], [Name], [PrincipalId])
VALUES (@p0, @p1, @p2);
INSERT INTO [ProductBase] ([Bytes], [Discriminator], [ProductWithBytes_Name])
OUTPUT INSERTED.[Id]
VALUES (@p3, @p4, @p5);");
    }

    [ConditionalFact]
    public override void Can_add_and_remove_self_refs()
    {
        base.Can_add_and_remove_self_refs();

        AssertContainsSql(
                @"@p0='1' (Size = 4000)
@p1=NULL (DbType = Int32)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [Person] ([Name], [ParentId])
OUTPUT INSERTED.[PersonId]
VALUES (@p0, @p1);",
                //
                @"@p2='2' (Size = 4000)
@p3='1' (Nullable = true)
@p4='3' (Size = 4000)
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
                @"@p6='4' (Size = 4000)
@p7='2' (Nullable = true)
@p8='5' (Size = 4000)
@p9='2' (Nullable = true)
@p10='6' (Size = 4000)
@p11='3' (Nullable = true)
@p12='7' (Size = 4000)
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

        AssertContainsSql(
            @"@p1='78'
@p0='New Category' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [Categories] SET [Name] = @p0
WHERE [Id] = @p1;
SELECT @@ROWCOUNT;");
    }

    public override void Identifiers_are_generated_correctly()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(
            typeof(
                LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly
            ));
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
            ));

        Assert.Equal(
            "LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkin~1",
            entityType2.GetTableName());
        Assert.Equal(
            "PK_LoginDetails",
            entityType2.GetKeys().Single().GetName());
        Assert.Equal(
            "ExtraPropertyWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCo~",
            entityType2.GetProperties().ElementAt(1).GetColumnBaseName());
        Assert.Equal(
            "ExtraPropertyWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingC~1",
            entityType2.GetProperties().ElementAt(2).GetColumnBaseName());
        Assert.Equal(
            "IX_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWor~1",
            entityType2.GetIndexes().Single().GetDatabaseName());
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected void AssertContainsSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, assertOrder: false);
}
