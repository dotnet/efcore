// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

public class StoreValueGenerationIdentityWithoutOutputSqlServerTest : StoreValueGenerationWithoutOutputSqlServerTestBase<
    StoreValueGenerationIdentityWithoutOutputSqlServerTest.StoreValueGenerationIdentityWithWithoutOutputSqlServerFixture>
{
    public StoreValueGenerationIdentityWithoutOutputSqlServerTest(
        StoreValueGenerationIdentityWithWithoutOutputSqlServerFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override bool ShouldCreateImplicitTransaction(
        EntityState firstOperationType,
        EntityState? secondOperationType,
        GeneratedValues generatedValues,
        bool withSameEntityType)
    {
        // We have triggers, so any insert/update retrieving a database-generated value must be enclosed in a transaction
        // (e.g. we use INSERT/UPDATE+SELECT or INSERT ... OUTPUT INTO+SELECT)
        if (generatedValues is GeneratedValues.Some or GeneratedValues.All
            && firstOperationType is EntityState.Added or EntityState.Modified)
        {
            return true;
        }

        if (secondOperationType is null)
        {
            return false;
        }

        // For multiple operations, we specifically optimize multiple insertions of the same entity type with a single MERGE.
        return !(firstOperationType is EntityState.Added && secondOperationType is EntityState.Added && withSameEntityType);
    }

    #region Single operation

    public override async Task Add_with_generated_values(bool async)
    {
        await base.Add_with_generated_values(async);

        AssertSql(
            """
@p0='1000'

SET NOCOUNT ON;
INSERT INTO [WithSomeDatabaseGenerated] ([Data2])
VALUES (@p0);
SELECT [Id], [Data1]
FROM [WithSomeDatabaseGenerated]
WHERE @@ROWCOUNT = 1 AND [Id] = scope_identity();
""");
    }

    public override async Task Add_with_no_generated_values(bool async)
    {
        await base.Add_with_no_generated_values(async);

        AssertSql(
            """
@p0='100'
@p1='1000'
@p2='1000'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [WithNoDatabaseGenerated] ([Id], [Data1], [Data2])
VALUES (@p0, @p1, @p2);
""");
    }

    public override async Task Add_with_all_generated_values(bool async)
    {
        await base.Add_with_all_generated_values(async);

        AssertSql(
            """
SET NOCOUNT ON;
INSERT INTO [WithAllDatabaseGenerated]
DEFAULT VALUES;
SELECT [Id], [Data1], [Data2]
FROM [WithAllDatabaseGenerated]
WHERE @@ROWCOUNT = 1 AND [Id] = scope_identity();
""");
    }

    public override async Task Modify_with_generated_values(bool async)
    {
        await base.Modify_with_generated_values(async);

        AssertSql(
            """
@p1='1'
@p0='1000'

SET NOCOUNT ON;
UPDATE [WithSomeDatabaseGenerated] SET [Data2] = @p0
WHERE [Id] = @p1;
SELECT [Data1]
FROM [WithSomeDatabaseGenerated]
WHERE @@ROWCOUNT = 1 AND [Id] = @p1;
""");
    }

    public override async Task Modify_with_no_generated_values(bool async)
    {
        await base.Modify_with_no_generated_values(async);

        AssertSql(
            """
@p2='1'
@p0='1000'
@p1='1000'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [WithNoDatabaseGenerated] SET [Data1] = @p0, [Data2] = @p1
WHERE [Id] = @p2;
SELECT @@ROWCOUNT;
""");
    }

    public override async Task Delete(bool async)
    {
        await base.Delete(async);

        AssertSql(
            """
@p0='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
DELETE FROM [WithSomeDatabaseGenerated]
WHERE [Id] = @p0;
SELECT @@ROWCOUNT;
""");
    }

    #endregion Single operation

    #region Same two operations with same entity type

    public override async Task Add_Add_with_same_entity_type_and_generated_values(bool async)
    {
        await base.Add_Add_with_same_entity_type_and_generated_values(async);

        AssertSql(
            """
@p0='1000'
@p1='1001'

SET NOCOUNT ON;
INSERT INTO [WithSomeDatabaseGenerated] ([Data2])
VALUES (@p0);
SELECT [Id], [Data1]
FROM [WithSomeDatabaseGenerated]
WHERE @@ROWCOUNT = 1 AND [Id] = scope_identity();

INSERT INTO [WithSomeDatabaseGenerated] ([Data2])
VALUES (@p1);
SELECT [Id], [Data1]
FROM [WithSomeDatabaseGenerated]
WHERE @@ROWCOUNT = 1 AND [Id] = scope_identity();
""");
    }

    public override async Task Add_Add_with_same_entity_type_and_no_generated_values(bool async)
    {
        await base.Add_Add_with_same_entity_type_and_no_generated_values(async);

        AssertSql(
            """
@p0='100'
@p1='1000'
@p2='1000'
@p3='101'
@p4='1001'
@p5='1001'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [WithNoDatabaseGenerated] ([Id], [Data1], [Data2])
VALUES (@p0, @p1, @p2),
(@p3, @p4, @p5);
""");
    }

    public override async Task Add_Add_with_same_entity_type_and_all_generated_values(bool async)
    {
        await base.Add_Add_with_same_entity_type_and_all_generated_values(async);

        AssertSql(
            """
SET NOCOUNT ON;
INSERT INTO [WithAllDatabaseGenerated]
DEFAULT VALUES;
SELECT [Id], [Data1], [Data2]
FROM [WithAllDatabaseGenerated]
WHERE @@ROWCOUNT = 1 AND [Id] = scope_identity();

INSERT INTO [WithAllDatabaseGenerated]
DEFAULT VALUES;
SELECT [Id], [Data1], [Data2]
FROM [WithAllDatabaseGenerated]
WHERE @@ROWCOUNT = 1 AND [Id] = scope_identity();
""");
    }

    public override async Task Modify_Modify_with_same_entity_type_and_generated_values(bool async)
    {
        await base.Modify_Modify_with_same_entity_type_and_generated_values(async);

        AssertSql(
            """
@p1='1'
@p0='1000'
@p3='2'
@p2='1001'

SET NOCOUNT ON;
UPDATE [WithSomeDatabaseGenerated] SET [Data2] = @p0
WHERE [Id] = @p1;
SELECT [Data1]
FROM [WithSomeDatabaseGenerated]
WHERE @@ROWCOUNT = 1 AND [Id] = @p1;

UPDATE [WithSomeDatabaseGenerated] SET [Data2] = @p2
WHERE [Id] = @p3;
SELECT [Data1]
FROM [WithSomeDatabaseGenerated]
WHERE @@ROWCOUNT = 1 AND [Id] = @p3;
""");
    }

    public override async Task Modify_Modify_with_same_entity_type_and_no_generated_values(bool async)
    {
        await base.Modify_Modify_with_same_entity_type_and_no_generated_values(async);

        AssertSql(
            """
@p2='1'
@p0='1000'
@p1='1000'
@p5='2'
@p3='1001'
@p4='1001'

SET NOCOUNT ON;
UPDATE [WithNoDatabaseGenerated] SET [Data1] = @p0, [Data2] = @p1
WHERE [Id] = @p2;
SELECT @@ROWCOUNT;

UPDATE [WithNoDatabaseGenerated] SET [Data1] = @p3, [Data2] = @p4
WHERE [Id] = @p5;
SELECT @@ROWCOUNT;
""");
    }

    public override async Task Delete_Delete_with_same_entity_type(bool async)
    {
        await base.Delete_Delete_with_same_entity_type(async);

        AssertSql(
            """
@p0='1'
@p1='2'

SET NOCOUNT ON;
DELETE FROM [WithSomeDatabaseGenerated]
WHERE [Id] = @p0;
SELECT @@ROWCOUNT;

DELETE FROM [WithSomeDatabaseGenerated]
WHERE [Id] = @p1;
SELECT @@ROWCOUNT;
""");
    }

    #endregion Same two operations with same entity type

    #region Same two operations with different entity types

    public override async Task Add_Add_with_different_entity_types_and_generated_values(bool async)
    {
        await base.Add_Add_with_different_entity_types_and_generated_values(async);

        AssertSql(
            """
@p0='1000'
@p1='1001'

SET NOCOUNT ON;
INSERT INTO [WithSomeDatabaseGenerated] ([Data2])
VALUES (@p0);
SELECT [Id], [Data1]
FROM [WithSomeDatabaseGenerated]
WHERE @@ROWCOUNT = 1 AND [Id] = scope_identity();

INSERT INTO [WithSomeDatabaseGenerated2] ([Data2])
VALUES (@p1);
SELECT [Id], [Data1]
FROM [WithSomeDatabaseGenerated2]
WHERE @@ROWCOUNT = 1 AND [Id] = scope_identity();
""");
    }

    public override async Task Add_Add_with_different_entity_types_and_no_generated_values(bool async)
    {
        await base.Add_Add_with_different_entity_types_and_no_generated_values(async);

        AssertSql(
            """
@p0='100'
@p1='1000'
@p2='1000'
@p3='101'
@p4='1001'
@p5='1001'

SET NOCOUNT ON;
INSERT INTO [WithNoDatabaseGenerated] ([Id], [Data1], [Data2])
VALUES (@p0, @p1, @p2);
INSERT INTO [WithNoDatabaseGenerated2] ([Id], [Data1], [Data2])
VALUES (@p3, @p4, @p5);
""");
    }

    public override async Task Add_Add_with_different_entity_types_and_all_generated_values(bool async)
    {
        await base.Add_Add_with_different_entity_types_and_all_generated_values(async);

        AssertSql(
            """
SET NOCOUNT ON;
INSERT INTO [WithAllDatabaseGenerated]
DEFAULT VALUES;
SELECT [Id], [Data1], [Data2]
FROM [WithAllDatabaseGenerated]
WHERE @@ROWCOUNT = 1 AND [Id] = scope_identity();

INSERT INTO [WithAllDatabaseGenerated2]
DEFAULT VALUES;
SELECT [Id], [Data1], [Data2]
FROM [WithAllDatabaseGenerated2]
WHERE @@ROWCOUNT = 1 AND [Id] = scope_identity();
""");
    }

    public override async Task Modify_Modify_with_different_entity_types_and_generated_values(bool async)
    {
        await base.Modify_Modify_with_different_entity_types_and_generated_values(async);

        AssertSql(
            """
@p1='1'
@p0='1000'
@p3='2'
@p2='1001'

SET NOCOUNT ON;
UPDATE [WithSomeDatabaseGenerated] SET [Data2] = @p0
WHERE [Id] = @p1;
SELECT [Data1]
FROM [WithSomeDatabaseGenerated]
WHERE @@ROWCOUNT = 1 AND [Id] = @p1;

UPDATE [WithSomeDatabaseGenerated2] SET [Data2] = @p2
WHERE [Id] = @p3;
SELECT [Data1]
FROM [WithSomeDatabaseGenerated2]
WHERE @@ROWCOUNT = 1 AND [Id] = @p3;
""");
    }

    public override async Task Modify_Modify_with_different_entity_types_and_no_generated_values(bool async)
    {
        await base.Modify_Modify_with_different_entity_types_and_no_generated_values(async);

        AssertSql(
            """
@p2='1'
@p0='1000'
@p1='1000'
@p5='2'
@p3='1001'
@p4='1001'

SET NOCOUNT ON;
UPDATE [WithNoDatabaseGenerated] SET [Data1] = @p0, [Data2] = @p1
WHERE [Id] = @p2;
SELECT @@ROWCOUNT;

UPDATE [WithNoDatabaseGenerated2] SET [Data1] = @p3, [Data2] = @p4
WHERE [Id] = @p5;
SELECT @@ROWCOUNT;
""");
    }

    public override async Task Delete_Delete_with_different_entity_types(bool async)
    {
        await base.Delete_Delete_with_different_entity_types(async);

        AssertSql(
            """
@p0='1'
@p1='2'

SET NOCOUNT ON;
DELETE FROM [WithSomeDatabaseGenerated]
WHERE [Id] = @p0;
SELECT @@ROWCOUNT;

DELETE FROM [WithSomeDatabaseGenerated2]
WHERE [Id] = @p1;
SELECT @@ROWCOUNT;
""");
    }

    #endregion Same two operations with different entity types

    public override async Task Three_Add_use_batched_inserts(bool async)
    {
        await base.Three_Add_use_batched_inserts(async);

        AssertSql(
            """
@p0='0'
@p1='0'
@p2='0'

SET NOCOUNT ON;
INSERT INTO [WithSomeDatabaseGenerated] ([Data2])
VALUES (@p0);
SELECT [Id], [Data1]
FROM [WithSomeDatabaseGenerated]
WHERE @@ROWCOUNT = 1 AND [Id] = scope_identity();

INSERT INTO [WithSomeDatabaseGenerated] ([Data2])
VALUES (@p1);
SELECT [Id], [Data1]
FROM [WithSomeDatabaseGenerated]
WHERE @@ROWCOUNT = 1 AND [Id] = scope_identity();

INSERT INTO [WithSomeDatabaseGenerated] ([Data2])
VALUES (@p2);
SELECT [Id], [Data1]
FROM [WithSomeDatabaseGenerated]
WHERE @@ROWCOUNT = 1 AND [Id] = scope_identity();
""");
    }

    protected override async Task Test(
        EntityState firstOperationType,
        EntityState? secondOperationType,
        GeneratedValues generatedValues,
        bool async,
        bool withSameEntityType = true)
    {
        await base.Test(firstOperationType, secondOperationType, generatedValues, async, withSameEntityType);

        if (!ShouldCreateImplicitTransaction(firstOperationType, secondOperationType, generatedValues, withSameEntityType))
        {
            Assert.Contains("SET IMPLICIT_TRANSACTIONS OFF", Fixture.TestSqlLoggerFactory.SqlStatements[0]);
        }
    }

    public class StoreValueGenerationIdentityWithWithoutOutputSqlServerFixture : StoreValueGenerationWithoutOutputSqlServerFixture
    {
        protected override string StoreName
            => "StoreValueGenerationIdentityWithTriggerTest";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
