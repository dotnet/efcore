// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.SaveChangesScenariosModel;

namespace Microsoft.EntityFrameworkCore.Update;

#nullable enable

public class SaveChangesScenariosSequenceSqlServerTest : SaveChangesScenariosTestBase<
    SaveChangesScenariosSequenceSqlServerTest.SaveChangesScenariosSequenceSqlServerFixture>
{
    public SaveChangesScenariosSequenceSqlServerTest(
        SaveChangesScenariosSequenceSqlServerFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        // Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override int ShouldExecuteInNumberOfCommands(
        EntityState firstOperationType,
        EntityState? secondOperationType,
        GeneratedValues generatedValues,
        bool withDatabaseGenerated)
        => secondOperationType is null ? 1 : 2;

    #region Single operation

    public override async Task Add_with_generated_values(bool async)
    {
        await base.Add_with_generated_values(async);

        AssertSql(
            @"@p0='1000'

SET NOCOUNT ON;
DECLARE @inserted0 TABLE ([Id] int);
INSERT INTO [WithSomeDatabaseGenerated] ([Data2])
OUTPUT INSERTED.[Id]
INTO @inserted0
VALUES (@p0);
SELECT [t].[Id], [t].[Data1] FROM [WithSomeDatabaseGenerated] t
INNER JOIN @inserted0 i ON ([t].[Id] = [i].[Id]);");
    }

    public override async Task Add_with_no_generated_values(bool async)
    {
        await base.Add_with_no_generated_values(async);

        AssertSql(
            @"@p0='100'
@p1='1000'
@p2='1000'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [WithNoDatabaseGenerated] ([Id], [Data1], [Data2])
VALUES (@p0, @p1, @p2);");
    }

    public override async Task Add_with_all_generated_values(bool async)
    {
        await base.Add_with_all_generated_values(async);

        AssertSql(
            @"SET NOCOUNT ON;
DECLARE @inserted0 TABLE ([Id] int);
INSERT INTO [WithAllDatabaseGenerated]
OUTPUT INSERTED.[Id]
INTO @inserted0
DEFAULT VALUES;
SELECT [t].[Id], [t].[Data1], [t].[Data2] FROM [WithAllDatabaseGenerated] t
INNER JOIN @inserted0 i ON ([t].[Id] = [i].[Id]);");
    }

    public override async Task Modify_with_generated_values(bool async)
    {
        await base.Modify_with_generated_values(async);

        AssertSql(
            @"@p1='5'
@p0='1000'

SET NOCOUNT ON;
UPDATE [WithSomeDatabaseGenerated] SET [Data2] = @p0
WHERE [Id] = @p1;
SELECT [Data1]
FROM [WithSomeDatabaseGenerated]
WHERE @@ROWCOUNT = 1 AND [Id] = @p1;");
    }

    public override async Task Modify_with_no_generated_values(bool async)
    {
        await base.Modify_with_no_generated_values(async);

        AssertSql(
            @"@p2='1'
@p0='1000'
@p1='1000'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [WithNoDatabaseGenerated] SET [Data1] = @p0, [Data2] = @p1
WHERE [Id] = @p2;
SELECT @@ROWCOUNT;");
    }

    public override async Task Delete(bool async)
    {
        await base.Delete(async);

        AssertSql(
            @"@p0='5'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
DELETE FROM [WithSomeDatabaseGenerated]
WHERE [Id] = @p0;
SELECT @@ROWCOUNT;");
    }

    #endregion Single operation

    #region Two operations with same entity type

    public override async Task Add_Add_with_same_entity_type_and_generated_values(bool async)
    {
        await base.Add_Add_with_same_entity_type_and_generated_values(async);

        AssertSql(
            @"@p0='1000'

SET NOCOUNT ON;
DECLARE @inserted0 TABLE ([Id] int);
INSERT INTO [WithSomeDatabaseGenerated] ([Data2])
OUTPUT INSERTED.[Id]
INTO @inserted0
VALUES (@p0);
SELECT [t].[Id], [t].[Data1] FROM [WithSomeDatabaseGenerated] t
INNER JOIN @inserted0 i ON ([t].[Id] = [i].[Id]);",
            //
            @"@p0='1001'

SET NOCOUNT ON;
DECLARE @inserted0 TABLE ([Id] int);
INSERT INTO [WithSomeDatabaseGenerated] ([Data2])
OUTPUT INSERTED.[Id]
INTO @inserted0
VALUES (@p0);
SELECT [t].[Id], [t].[Data1] FROM [WithSomeDatabaseGenerated] t
INNER JOIN @inserted0 i ON ([t].[Id] = [i].[Id]);");
    }

    public override async Task Add_Add_with_same_entity_type_and_no_generated_values(bool async)
    {
        await base.Add_Add_with_same_entity_type_and_no_generated_values(async);

        AssertSql(
            @"@p0='100'
@p1='1000'
@p2='1000'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [WithNoDatabaseGenerated] ([Id], [Data1], [Data2])
VALUES (@p0, @p1, @p2);",
            //
            @"@p0='101'
@p1='1001'
@p2='1001'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [WithNoDatabaseGenerated] ([Id], [Data1], [Data2])
VALUES (@p0, @p1, @p2);");
    }

    public override async Task Add_Add_with_same_entity_type_and_all_generated_values(bool async)
    {
        await base.Add_Add_with_same_entity_type_and_all_generated_values(async);

        AssertSql(
            @"SET NOCOUNT ON;
DECLARE @inserted0 TABLE ([Id] int);
INSERT INTO [WithAllDatabaseGenerated]
OUTPUT INSERTED.[Id]
INTO @inserted0
DEFAULT VALUES;
SELECT [t].[Id], [t].[Data1], [t].[Data2] FROM [WithAllDatabaseGenerated] t
INNER JOIN @inserted0 i ON ([t].[Id] = [i].[Id]);",
            //
            @"SET NOCOUNT ON;
DECLARE @inserted0 TABLE ([Id] int);
INSERT INTO [WithAllDatabaseGenerated]
OUTPUT INSERTED.[Id]
INTO @inserted0
DEFAULT VALUES;
SELECT [t].[Id], [t].[Data1], [t].[Data2] FROM [WithAllDatabaseGenerated] t
INNER JOIN @inserted0 i ON ([t].[Id] = [i].[Id]);");
    }

    public override async Task Modify_Modify_with_same_entity_type_and_generated_values(bool async)
    {
        await base.Modify_Modify_with_same_entity_type_and_generated_values(async);

        AssertSql(
            @"@p1='5'
@p0='1000'

SET NOCOUNT ON;
UPDATE [WithSomeDatabaseGenerated] SET [Data2] = @p0
WHERE [Id] = @p1;
SELECT [Data1]
FROM [WithSomeDatabaseGenerated]
WHERE @@ROWCOUNT = 1 AND [Id] = @p1;",
            //
            @"@p1='6'
@p0='1001'

SET NOCOUNT ON;
UPDATE [WithSomeDatabaseGenerated] SET [Data2] = @p0
WHERE [Id] = @p1;
SELECT [Data1]
FROM [WithSomeDatabaseGenerated]
WHERE @@ROWCOUNT = 1 AND [Id] = @p1;");
    }

    public override async Task Modify_Modify_with_same_entity_type_and_no_generated_values(bool async)
    {
        await base.Modify_Modify_with_same_entity_type_and_no_generated_values(async);

        AssertSql(
            @"@p2='1'
@p0='1000'
@p1='1000'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [WithNoDatabaseGenerated] SET [Data1] = @p0, [Data2] = @p1
WHERE [Id] = @p2;
SELECT @@ROWCOUNT;",
            //
            @"@p2='2'
@p0='1001'
@p1='1001'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [WithNoDatabaseGenerated] SET [Data1] = @p0, [Data2] = @p1
WHERE [Id] = @p2;
SELECT @@ROWCOUNT;");
    }

    public override async Task Delete_Delete_with_same_entity_type(bool async)
    {
        await base.Delete_Delete_with_same_entity_type(async);

        AssertSql(
            @"@p0='5'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
DELETE FROM [WithSomeDatabaseGenerated]
WHERE [Id] = @p0;
SELECT @@ROWCOUNT;",
            //
            @"@p0='6'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
DELETE FROM [WithSomeDatabaseGenerated]
WHERE [Id] = @p0;
SELECT @@ROWCOUNT;");
    }

    #endregion Two operations with same entity type

    #region Two operations with different entity types

    public override async Task Add_Add_with_different_entity_types_and_generated_values(bool async)
    {
        await base.Add_Add_with_different_entity_types_and_generated_values(async);

        AssertSql(
            @"@p0='1000'

SET NOCOUNT ON;
DECLARE @inserted0 TABLE ([Id] int);
INSERT INTO [WithSomeDatabaseGenerated] ([Data2])
OUTPUT INSERTED.[Id]
INTO @inserted0
VALUES (@p0);
SELECT [t].[Id], [t].[Data1] FROM [WithSomeDatabaseGenerated] t
INNER JOIN @inserted0 i ON ([t].[Id] = [i].[Id]);",
            //
            @"@p0='1001'

SET NOCOUNT ON;
DECLARE @inserted0 TABLE ([Id] int);
INSERT INTO [WithSomeDatabaseGenerated2] ([Data2])
OUTPUT INSERTED.[Id]
INTO @inserted0
VALUES (@p0);
SELECT [t].[Id], [t].[Data1] FROM [WithSomeDatabaseGenerated2] t
INNER JOIN @inserted0 i ON ([t].[Id] = [i].[Id]);");
    }

    public override async Task Add_Add_with_different_entity_types_and_no_generated_values(bool async)
    {
        await base.Add_Add_with_different_entity_types_and_no_generated_values(async);

        AssertSql(
            @"@p0='100'
@p1='1000'
@p2='1000'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [WithNoDatabaseGenerated] ([Id], [Data1], [Data2])
VALUES (@p0, @p1, @p2);",
            //
            @"@p0='101'
@p1='1001'
@p2='1001'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [WithNoDatabaseGenerated2] ([Id], [Data1], [Data2])
VALUES (@p0, @p1, @p2);");
    }

    public override async Task Add_Add_with_different_entity_types_and_all_generated_values(bool async)
    {
        await base.Add_Add_with_different_entity_types_and_all_generated_values(async);

        AssertSql(
            @"SET NOCOUNT ON;
DECLARE @inserted0 TABLE ([Id] int);
INSERT INTO [WithAllDatabaseGenerated]
OUTPUT INSERTED.[Id]
INTO @inserted0
DEFAULT VALUES;
SELECT [t].[Id], [t].[Data1], [t].[Data2] FROM [WithAllDatabaseGenerated] t
INNER JOIN @inserted0 i ON ([t].[Id] = [i].[Id]);",
            //
            @"SET NOCOUNT ON;
DECLARE @inserted0 TABLE ([Id] int);
INSERT INTO [WithAllDatabaseGenerated2]
OUTPUT INSERTED.[Id]
INTO @inserted0
DEFAULT VALUES;
SELECT [t].[Id], [t].[Data1], [t].[Data2] FROM [WithAllDatabaseGenerated2] t
INNER JOIN @inserted0 i ON ([t].[Id] = [i].[Id]);");
    }

    public override async Task Modify_Modify_with_different_entity_types_and_generated_values(bool async)
    {
        await base.Modify_Modify_with_different_entity_types_and_generated_values(async);

        AssertSql(
            @"@p1='5'
@p0='1000'

SET NOCOUNT ON;
UPDATE [WithSomeDatabaseGenerated] SET [Data2] = @p0
WHERE [Id] = @p1;
SELECT [Data1]
FROM [WithSomeDatabaseGenerated]
WHERE @@ROWCOUNT = 1 AND [Id] = @p1;",
            //
            @"@p1='8'
@p0='1001'

SET NOCOUNT ON;
UPDATE [WithSomeDatabaseGenerated2] SET [Data2] = @p0
WHERE [Id] = @p1;
SELECT [Data1]
FROM [WithSomeDatabaseGenerated2]
WHERE @@ROWCOUNT = 1 AND [Id] = @p1;");
    }

    public override async Task Modify_Modify_with_different_entity_types_and_no_generated_values(bool async)
    {
        await base.Modify_Modify_with_different_entity_types_and_no_generated_values(async);
AssertSql(
    @"@p2='1'
@p0='1000'
@p1='1000'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [WithNoDatabaseGenerated] SET [Data1] = @p0, [Data2] = @p1
WHERE [Id] = @p2;
SELECT @@ROWCOUNT;",
    //
    @"@p2='2'
@p0='1001'
@p1='1001'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [WithNoDatabaseGenerated2] SET [Data1] = @p0, [Data2] = @p1
WHERE [Id] = @p2;
SELECT @@ROWCOUNT;");
    }

    public override async Task Delete_Delete_with_different_entity_types(bool async)
    {
        await base.Delete_Delete_with_different_entity_types(async);

        AssertSql(
            @"@p0='5'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
DELETE FROM [WithSomeDatabaseGenerated]
WHERE [Id] = @p0;
SELECT @@ROWCOUNT;",
            //
            @"@p0='8'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
DELETE FROM [WithSomeDatabaseGenerated2]
WHERE [Id] = @p0;
SELECT @@ROWCOUNT;");
    }

    #endregion Two operations with different entity types

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

    public class SaveChangesScenariosSequenceSqlServerFixture : SaveChangesScenariosFixtureBase
    {
        protected override string StoreName { get; } = "SaveChangesScenariosSequenceTest";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.HasSequence<int>("Ids");

            foreach (var name in new[]
                     {
                         nameof(SaveChangesScenariosContext.WithSomeDatabaseGenerated),
                         nameof(SaveChangesScenariosContext.WithSomeDatabaseGenerated2),
                         nameof(SaveChangesScenariosContext.WithAllDatabaseGenerated),
                         nameof(SaveChangesScenariosContext.WithAllDatabaseGenerated2)
                     })
            {
                modelBuilder
                    .SharedTypeEntity<SaveChangesData>(name)
                    .Property(w => w.Id)
                    .HasDefaultValueSql("NEXT VALUE FOR [Ids]");
            }
        }
    }
}
