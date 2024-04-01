// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

namespace Microsoft.EntityFrameworkCore.Update;

public abstract class UpdatesSqlServerTestBase<TFixture> : UpdatesRelationalTestBase<TFixture>
    where TFixture : UpdatesSqlServerTestBase<TFixture>.UpdatesSqlServerFixtureBase
{
    protected UpdatesSqlServerTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        Fixture.TestSqlLoggerFactory.Clear();
    }

    public override async Task Can_add_and_remove_self_refs()
    {
        await Fixture.ResetIdentity();

        await base.Can_add_and_remove_self_refs();

        AssertSql(
            """
@p0=NULL (Size = 4000)
@p1='1' (Nullable = false) (Size = 4000)
@p2=NULL (DbType = Int32)
@p3=NULL (DbType = Int32)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [Person] ([Country], [Name], [ParentId], [ZipCode])
OUTPUT INSERTED.[PersonId]
VALUES (@p0, @p1, @p2, @p3);
""",
            //
            """
@p4=NULL (Size = 4000)
@p5='2' (Nullable = false) (Size = 4000)
@p6='1' (Nullable = true)
@p7=NULL (DbType = Int32)
@p8=NULL (Size = 4000)
@p9='3' (Nullable = false) (Size = 4000)
@p10='1' (Nullable = true)
@p11=NULL (DbType = Int32)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
MERGE [Person] USING (
VALUES (@p4, @p5, @p6, @p7, 0),
(@p8, @p9, @p10, @p11, 1)) AS i ([Country], [Name], [ParentId], [ZipCode], _Position) ON 1=0
WHEN NOT MATCHED THEN
INSERT ([Country], [Name], [ParentId], [ZipCode])
VALUES (i.[Country], i.[Name], i.[ParentId], i.[ZipCode])
OUTPUT INSERTED.[PersonId], i._Position;
""",
            //
            """
@p12=NULL (Size = 4000)
@p13='4' (Nullable = false) (Size = 4000)
@p14='2' (Nullable = true)
@p15=NULL (DbType = Int32)
@p16=NULL (Size = 4000)
@p17='5' (Nullable = false) (Size = 4000)
@p18='2' (Nullable = true)
@p19=NULL (DbType = Int32)
@p20=NULL (Size = 4000)
@p21='6' (Nullable = false) (Size = 4000)
@p22='3' (Nullable = true)
@p23=NULL (DbType = Int32)
@p24=NULL (Size = 4000)
@p25='7' (Nullable = false) (Size = 4000)
@p26='3' (Nullable = true)
@p27=NULL (DbType = Int32)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
MERGE [Person] USING (
VALUES (@p12, @p13, @p14, @p15, 0),
(@p16, @p17, @p18, @p19, 1),
(@p20, @p21, @p22, @p23, 2),
(@p24, @p25, @p26, @p27, 3)) AS i ([Country], [Name], [ParentId], [ZipCode], _Position) ON 1=0
WHEN NOT MATCHED THEN
INSERT ([Country], [Name], [ParentId], [ZipCode])
VALUES (i.[Country], i.[Name], i.[ParentId], i.[ZipCode])
OUTPUT INSERTED.[PersonId], i._Position;
""",
            //
            """
@p0='4'
@p1='5'
@p2='6'
@p3='2'
@p4='7'
@p5='3'
@p6=NULL (Size = 4000)
@p7='1' (Nullable = false) (Size = 4000)
@p8=NULL (DbType = Int32)
@p9=NULL (DbType = Int32)

SET NOCOUNT ON;
DELETE FROM [Person]
OUTPUT 1
WHERE [PersonId] = @p0;
DELETE FROM [Person]
OUTPUT 1
WHERE [PersonId] = @p1;
DELETE FROM [Person]
OUTPUT 1
WHERE [PersonId] = @p2;
DELETE FROM [Person]
OUTPUT 1
WHERE [PersonId] = @p3;
DELETE FROM [Person]
OUTPUT 1
WHERE [PersonId] = @p4;
DELETE FROM [Person]
OUTPUT 1
WHERE [PersonId] = @p5;
INSERT INTO [Person] ([Country], [Name], [ParentId], [ZipCode])
OUTPUT INSERTED.[PersonId]
VALUES (@p6, @p7, @p8, @p9);
""",
            //
            """
@p10='1'
@p11=NULL (Size = 4000)
@p12='2' (Nullable = false) (Size = 4000)
@p13='8' (Nullable = true)
@p14=NULL (DbType = Int32)
@p15=NULL (Size = 4000)
@p16='3' (Nullable = false) (Size = 4000)
@p17='8' (Nullable = true)
@p18=NULL (DbType = Int32)

SET NOCOUNT ON;
DELETE FROM [Person]
OUTPUT 1
WHERE [PersonId] = @p10;
MERGE [Person] USING (
VALUES (@p11, @p12, @p13, @p14, 0),
(@p15, @p16, @p17, @p18, 1)) AS i ([Country], [Name], [ParentId], [ZipCode], _Position) ON 1=0
WHEN NOT MATCHED THEN
INSERT ([Country], [Name], [ParentId], [ZipCode])
VALUES (i.[Country], i.[Name], i.[ParentId], i.[ZipCode])
OUTPUT INSERTED.[PersonId], i._Position;
""",
            //
            """
@p19=NULL (Size = 4000)
@p20='4' (Nullable = false) (Size = 4000)
@p21='9' (Nullable = true)
@p22=NULL (DbType = Int32)
@p23=NULL (Size = 4000)
@p24='5' (Nullable = false) (Size = 4000)
@p25='9' (Nullable = true)
@p26=NULL (DbType = Int32)
@p27=NULL (Size = 4000)
@p28='6' (Nullable = false) (Size = 4000)
@p29='10' (Nullable = true)
@p30=NULL (DbType = Int32)
@p31=NULL (Size = 4000)
@p32='7' (Nullable = false) (Size = 4000)
@p33='10' (Nullable = true)
@p34=NULL (DbType = Int32)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
MERGE [Person] USING (
VALUES (@p19, @p20, @p21, @p22, 0),
(@p23, @p24, @p25, @p26, 1),
(@p27, @p28, @p29, @p30, 2),
(@p31, @p32, @p33, @p34, 3)) AS i ([Country], [Name], [ParentId], [ZipCode], _Position) ON 1=0
WHEN NOT MATCHED THEN
INSERT ([Country], [Name], [ParentId], [ZipCode])
VALUES (i.[Country], i.[Name], i.[ParentId], i.[ZipCode])
OUTPUT INSERTED.[PersonId], i._Position;
""",
            //
            """
SELECT [p].[PersonId], [p].[Country], [p].[Name], [p].[ParentId], [p].[ZipCode], [p].[Address_City], [p].[Country], [p].[ZipCode], [p0].[PersonId], [p0].[Country], [p0].[Name], [p0].[ParentId], [p0].[ZipCode], [p0].[Address_City], [p0].[Country], [p0].[ZipCode], [p1].[PersonId], [p1].[Country], [p1].[Name], [p1].[ParentId], [p1].[ZipCode], [p1].[Address_City], [p1].[Country], [p1].[ZipCode], [p2].[PersonId], [p2].[Country], [p2].[Name], [p2].[ParentId], [p2].[ZipCode], [p2].[Address_City], [p2].[Country], [p2].[ZipCode]
FROM [Person] AS [p]
LEFT JOIN [Person] AS [p0] ON [p].[ParentId] = [p0].[PersonId]
LEFT JOIN [Person] AS [p1] ON [p0].[ParentId] = [p1].[PersonId]
LEFT JOIN [Person] AS [p2] ON [p1].[ParentId] = [p2].[PersonId]
""");
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

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected void AssertContainsSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, assertOrder: false);

    public abstract class UpdatesSqlServerFixtureBase : UpdatesRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(
                w =>
                {
                    w.Log(SqlServerEventId.DecimalTypeKeyWarning);
                });

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
            => configurationBuilder.Properties<decimal>().HaveColumnType("decimal(18, 2)");

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<ProductBase>()
                .Property(p => p.Id).HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<Product>().HasIndex(p => new { p.Name, p.Price }).HasFilter("Name IS NOT NULL");
        }

        public virtual async Task ResetIdentity()
        {
            var context = CreateContext();
            await context.Database.ExecuteSqlRawAsync(ResetIdentitySql);
            TestSqlLoggerFactory.Clear();
        }

        private const string ResetIdentitySql = @"
-- We can't use TRUNCATE on tables with foreign keys, so we DELETE and reset IDENTITY manually.
-- DBCC CHECKIDENT resets IDENTITY, but behaves differently based on whether whether rows were ever inserted (seed+1) or not (seed).
-- So we insert a dummy row before deleting everything to make sure we get the seed value 1.
INSERT INTO [Person] ([Name]) VALUES ('');
DELETE FROM [Person];
DBCC CHECKIDENT ('[Person]', RESEED, 0);";
    }
}
