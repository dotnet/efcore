// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.StoreValueGenerationModel;

namespace Microsoft.EntityFrameworkCore.Update;

public class StoreValueGenerationSequenceSqlServerTest : StoreValueGenerationTestBase<
    StoreValueGenerationSequenceSqlServerTest.StoreValueGenerationSequenceSqlServerFixture>
{
    public StoreValueGenerationSequenceSqlServerTest(
        StoreValueGenerationSequenceSqlServerFixture fixture,
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
        // For multiple operations, we specifically optimize multiple insertions of the same entity type with a single command (e.g. MERGE)
        // (as long as there are writable columns)
        if (firstOperationType is EntityState.Added
            && secondOperationType is EntityState.Added
            && withSameEntityType
            && generatedValues != GeneratedValues.All)
        {
            return false;
        }

        // Other single operations should never be in a transaction (always executed in a single SQL command)
        return secondOperationType is not null;
    }

    #region Single operation

    public override async Task Add_with_generated_values(bool async)
    {
        await base.Add_with_generated_values(async);

        AssertSql(
            """
@p0='1000'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [WithSomeDatabaseGenerated] ([Data2])
OUTPUT INSERTED.[Id], INSERTED.[Data1]
VALUES (@p0);
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
SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [WithAllDatabaseGenerated]
OUTPUT INSERTED.[Id], INSERTED.[Data1], INSERTED.[Data2]
DEFAULT VALUES;
""");
    }

    public override async Task Modify_with_generated_values(bool async)
    {
        await base.Modify_with_generated_values(async);

        AssertSql(
            """
@p1='5'
@p0='1000'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [WithSomeDatabaseGenerated] SET [Data2] = @p0
OUTPUT INSERTED.[Data1]
WHERE [Id] = @p1;
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
OUTPUT 1
WHERE [Id] = @p2;
""");
    }

    public override async Task Delete(bool async)
    {
        await base.Delete(async);

        AssertSql(
            """
@p0='5'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
DELETE FROM [WithSomeDatabaseGenerated]
OUTPUT 1
WHERE [Id] = @p0;
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

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
MERGE [WithSomeDatabaseGenerated] USING (
VALUES (@p0, 0),
(@p1, 1)) AS i ([Data2], _Position) ON 1=0
WHEN NOT MATCHED THEN
INSERT ([Data2])
VALUES (i.[Data2])
OUTPUT INSERTED.[Id], INSERTED.[Data1], i._Position;
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
DECLARE @inserted0 TABLE ([Id] int);
INSERT INTO [WithAllDatabaseGenerated] ([Id])
OUTPUT INSERTED.[Id]
INTO @inserted0
VALUES (DEFAULT),
(DEFAULT);
SELECT [t].[Id], [t].[Data1], [t].[Data2] FROM [WithAllDatabaseGenerated] t
INNER JOIN @inserted0 i ON ([t].[Id] = [i].[Id]);
""");
    }

    public override async Task Modify_Modify_with_same_entity_type_and_generated_values(bool async)
    {
        await base.Modify_Modify_with_same_entity_type_and_generated_values(async);

        AssertSql(
            """
@p1='5'
@p0='1000'
@p3='6'
@p2='1001'

SET NOCOUNT ON;
UPDATE [WithSomeDatabaseGenerated] SET [Data2] = @p0
OUTPUT INSERTED.[Data1]
WHERE [Id] = @p1;
UPDATE [WithSomeDatabaseGenerated] SET [Data2] = @p2
OUTPUT INSERTED.[Data1]
WHERE [Id] = @p3;
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
OUTPUT 1
WHERE [Id] = @p2;
UPDATE [WithNoDatabaseGenerated] SET [Data1] = @p3, [Data2] = @p4
OUTPUT 1
WHERE [Id] = @p5;
""");
    }

    public override async Task Delete_Delete_with_same_entity_type(bool async)
    {
        await base.Delete_Delete_with_same_entity_type(async);

        AssertSql(
            """
@p0='5'
@p1='6'

SET NOCOUNT ON;
DELETE FROM [WithSomeDatabaseGenerated]
OUTPUT 1
WHERE [Id] = @p0;
DELETE FROM [WithSomeDatabaseGenerated]
OUTPUT 1
WHERE [Id] = @p1;
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
OUTPUT INSERTED.[Id], INSERTED.[Data1]
VALUES (@p0);
INSERT INTO [WithSomeDatabaseGenerated2] ([Data2])
OUTPUT INSERTED.[Id], INSERTED.[Data1]
VALUES (@p1);
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
OUTPUT INSERTED.[Id], INSERTED.[Data1], INSERTED.[Data2]
DEFAULT VALUES;
INSERT INTO [WithAllDatabaseGenerated2]
OUTPUT INSERTED.[Id], INSERTED.[Data1], INSERTED.[Data2]
DEFAULT VALUES;
""");
    }

    public override async Task Modify_Modify_with_different_entity_types_and_generated_values(bool async)
    {
        await base.Modify_Modify_with_different_entity_types_and_generated_values(async);

        AssertSql(
            """
@p1='5'
@p0='1000'
@p3='8'
@p2='1001'

SET NOCOUNT ON;
UPDATE [WithSomeDatabaseGenerated] SET [Data2] = @p0
OUTPUT INSERTED.[Data1]
WHERE [Id] = @p1;
UPDATE [WithSomeDatabaseGenerated2] SET [Data2] = @p2
OUTPUT INSERTED.[Data1]
WHERE [Id] = @p3;
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
OUTPUT 1
WHERE [Id] = @p2;
UPDATE [WithNoDatabaseGenerated2] SET [Data1] = @p3, [Data2] = @p4
OUTPUT 1
WHERE [Id] = @p5;
""");
    }

    public override async Task Delete_Delete_with_different_entity_types(bool async)
    {
        await base.Delete_Delete_with_different_entity_types(async);

        AssertSql(
            """
@p0='5'
@p1='8'

SET NOCOUNT ON;
DELETE FROM [WithSomeDatabaseGenerated]
OUTPUT 1
WHERE [Id] = @p0;
DELETE FROM [WithSomeDatabaseGenerated2]
OUTPUT 1
WHERE [Id] = @p1;
""");
    }

    #endregion Same two operations with different entity types

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

    public class StoreValueGenerationSequenceSqlServerFixture : StoreValueGenerationSqlServerFixtureBase
    {
        protected override string StoreName
            => "StoreValueGenerationSequenceTest";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.HasSequence<int>("Ids");

            foreach (var name in new[]
                     {
                         nameof(StoreValueGenerationContext.WithSomeDatabaseGenerated),
                         nameof(StoreValueGenerationContext.WithSomeDatabaseGenerated2),
                         nameof(StoreValueGenerationContext.WithAllDatabaseGenerated),
                         nameof(StoreValueGenerationContext.WithAllDatabaseGenerated2)
                     })
            {
                modelBuilder
                    .SharedTypeEntity<StoreValueGenerationData>(name)
                    .Property(w => w.Id)
                    .HasDefaultValueSql("NEXT VALUE FOR [Ids]");
            }
        }
    }
}
