// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

public class StoreValueGenerationWithoutReturningSqliteTest : StoreValueGenerationTestBase
    <StoreValueGenerationWithoutReturningSqliteTest.StoreValueGenerationWithoutReturningSqliteFixture>
{
    public StoreValueGenerationWithoutReturningSqliteTest(
        StoreValueGenerationWithoutReturningSqliteFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
        fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // We don't currently batch in Sqlite (the perf impact is likely to be minimal, no networking)
    protected override int ShouldExecuteInNumberOfCommands(
        EntityState firstOperationType,
        EntityState? secondOperationType,
        GeneratedValues generatedValues,
        bool withDatabaseGenerated)
        => secondOperationType is null ? 1 : 2;

    protected override bool ShouldCreateImplicitTransaction(
        EntityState firstOperationType,
        EntityState? secondOperationType,
        GeneratedValues generatedValues,
        bool withSameEntityType)
        => secondOperationType is not null
            || (generatedValues is GeneratedValues.Some or GeneratedValues.All
                && firstOperationType is EntityState.Added or EntityState.Modified);

    #region Single operation

    public override async Task Add_with_generated_values(bool async)
    {
        await base.Add_with_generated_values(async);

        AssertSql(
            """
@p0='1000'

INSERT INTO "WithSomeDatabaseGenerated" ("Data2")
VALUES (@p0);
SELECT "Id", "Data1"
FROM "WithSomeDatabaseGenerated"
WHERE changes() = 1 AND "rowid" = last_insert_rowid();
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

INSERT INTO "WithNoDatabaseGenerated" ("Id", "Data1", "Data2")
VALUES (@p0, @p1, @p2);
SELECT changes();
""");
    }

    public override async Task Add_with_all_generated_values(bool async)
    {
        await base.Add_with_all_generated_values(async);

        AssertSql(
            """
INSERT INTO "WithAllDatabaseGenerated"
DEFAULT VALUES;
SELECT "Id", "Data1", "Data2"
FROM "WithAllDatabaseGenerated"
WHERE changes() = 1 AND "rowid" = last_insert_rowid();
""");
    }

    public override async Task Modify_with_generated_values(bool async)
    {
        await base.Modify_with_generated_values(async);

        AssertSql(
            """
@p1='1'
@p0='1000'

UPDATE "WithSomeDatabaseGenerated" SET "Data2" = @p0
WHERE "Id" = @p1;
SELECT "Data1"
FROM "WithSomeDatabaseGenerated"
WHERE changes() = 1 AND "Id" = @p1;
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

UPDATE "WithNoDatabaseGenerated" SET "Data1" = @p0, "Data2" = @p1
WHERE "Id" = @p2;
SELECT changes();
""");
    }

    public override async Task Delete(bool async)
    {
        await base.Delete(async);

        AssertSql(
            """
@p0='1'

DELETE FROM "WithSomeDatabaseGenerated"
WHERE "Id" = @p0;
SELECT changes();
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

INSERT INTO "WithSomeDatabaseGenerated" ("Data2")
VALUES (@p0);
SELECT "Id", "Data1"
FROM "WithSomeDatabaseGenerated"
WHERE changes() = 1 AND "rowid" = last_insert_rowid();
""",
            //
            """
@p0='1001'

INSERT INTO "WithSomeDatabaseGenerated" ("Data2")
VALUES (@p0);
SELECT "Id", "Data1"
FROM "WithSomeDatabaseGenerated"
WHERE changes() = 1 AND "rowid" = last_insert_rowid();
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

INSERT INTO "WithNoDatabaseGenerated" ("Id", "Data1", "Data2")
VALUES (@p0, @p1, @p2);
SELECT changes();
""",
            //
            """
@p0='101'
@p1='1001'
@p2='1001'

INSERT INTO "WithNoDatabaseGenerated" ("Id", "Data1", "Data2")
VALUES (@p0, @p1, @p2);
SELECT changes();
""");
    }

    public override async Task Add_Add_with_same_entity_type_and_all_generated_values(bool async)
    {
        await base.Add_Add_with_same_entity_type_and_all_generated_values(async);

        AssertSql(
            """
INSERT INTO "WithAllDatabaseGenerated"
DEFAULT VALUES;
SELECT "Id", "Data1", "Data2"
FROM "WithAllDatabaseGenerated"
WHERE changes() = 1 AND "rowid" = last_insert_rowid();
""",
            //
            """
INSERT INTO "WithAllDatabaseGenerated"
DEFAULT VALUES;
SELECT "Id", "Data1", "Data2"
FROM "WithAllDatabaseGenerated"
WHERE changes() = 1 AND "rowid" = last_insert_rowid();
""");
    }

    public override async Task Modify_Modify_with_same_entity_type_and_generated_values(bool async)
    {
        await base.Modify_Modify_with_same_entity_type_and_generated_values(async);

        AssertSql(
            """
@p1='1'
@p0='1000'

UPDATE "WithSomeDatabaseGenerated" SET "Data2" = @p0
WHERE "Id" = @p1;
SELECT "Data1"
FROM "WithSomeDatabaseGenerated"
WHERE changes() = 1 AND "Id" = @p1;
""",
            //
            """
@p1='2'
@p0='1001'

UPDATE "WithSomeDatabaseGenerated" SET "Data2" = @p0
WHERE "Id" = @p1;
SELECT "Data1"
FROM "WithSomeDatabaseGenerated"
WHERE changes() = 1 AND "Id" = @p1;
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

UPDATE "WithNoDatabaseGenerated" SET "Data1" = @p0, "Data2" = @p1
WHERE "Id" = @p2;
SELECT changes();
""",
            //
            """
@p2='2'
@p0='1001'
@p1='1001'

UPDATE "WithNoDatabaseGenerated" SET "Data1" = @p0, "Data2" = @p1
WHERE "Id" = @p2;
SELECT changes();
""");
    }

    public override async Task Delete_Delete_with_same_entity_type(bool async)
    {
        await base.Delete_Delete_with_same_entity_type(async);

        AssertSql(
            """
@p0='1'

DELETE FROM "WithSomeDatabaseGenerated"
WHERE "Id" = @p0;
SELECT changes();
""",
            //
            """
@p0='2'

DELETE FROM "WithSomeDatabaseGenerated"
WHERE "Id" = @p0;
SELECT changes();
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

INSERT INTO "WithSomeDatabaseGenerated" ("Data2")
VALUES (@p0);
SELECT "Id", "Data1"
FROM "WithSomeDatabaseGenerated"
WHERE changes() = 1 AND "rowid" = last_insert_rowid();
""",
            //
            """
@p0='1001'

INSERT INTO "WithSomeDatabaseGenerated2" ("Data2")
VALUES (@p0);
SELECT "Id", "Data1"
FROM "WithSomeDatabaseGenerated2"
WHERE changes() = 1 AND "rowid" = last_insert_rowid();
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

INSERT INTO "WithNoDatabaseGenerated" ("Id", "Data1", "Data2")
VALUES (@p0, @p1, @p2);
SELECT changes();
""",
            //
            """
@p0='101'
@p1='1001'
@p2='1001'

INSERT INTO "WithNoDatabaseGenerated2" ("Id", "Data1", "Data2")
VALUES (@p0, @p1, @p2);
SELECT changes();
""");
    }

    public override async Task Add_Add_with_different_entity_types_and_all_generated_values(bool async)
    {
        await base.Add_Add_with_different_entity_types_and_all_generated_values(async);

        AssertSql(
            """
INSERT INTO "WithAllDatabaseGenerated"
DEFAULT VALUES;
SELECT "Id", "Data1", "Data2"
FROM "WithAllDatabaseGenerated"
WHERE changes() = 1 AND "rowid" = last_insert_rowid();
""",
            //
            """
INSERT INTO "WithAllDatabaseGenerated2"
DEFAULT VALUES;
SELECT "Id", "Data1", "Data2"
FROM "WithAllDatabaseGenerated2"
WHERE changes() = 1 AND "rowid" = last_insert_rowid();
""");
    }

    public override async Task Modify_Modify_with_different_entity_types_and_generated_values(bool async)
    {
        await base.Modify_Modify_with_different_entity_types_and_generated_values(async);

        AssertSql(
            """
@p1='1'
@p0='1000'

UPDATE "WithSomeDatabaseGenerated" SET "Data2" = @p0
WHERE "Id" = @p1;
SELECT "Data1"
FROM "WithSomeDatabaseGenerated"
WHERE changes() = 1 AND "Id" = @p1;
""",
            //
            """
@p1='2'
@p0='1001'

UPDATE "WithSomeDatabaseGenerated2" SET "Data2" = @p0
WHERE "Id" = @p1;
SELECT "Data1"
FROM "WithSomeDatabaseGenerated2"
WHERE changes() = 1 AND "Id" = @p1;
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

UPDATE "WithNoDatabaseGenerated" SET "Data1" = @p0, "Data2" = @p1
WHERE "Id" = @p2;
SELECT changes();
""",
            //
            """
@p2='2'
@p0='1001'
@p1='1001'

UPDATE "WithNoDatabaseGenerated2" SET "Data1" = @p0, "Data2" = @p1
WHERE "Id" = @p2;
SELECT changes();
""");
    }

    public override async Task Delete_Delete_with_different_entity_types(bool async)
    {
        await base.Delete_Delete_with_different_entity_types(async);

        AssertSql(
            """
@p0='1'

DELETE FROM "WithSomeDatabaseGenerated"
WHERE "Id" = @p0;
SELECT changes();
""",
            //
            """
@p0='2'

DELETE FROM "WithSomeDatabaseGenerated2"
WHERE "Id" = @p0;
SELECT changes();
""");
    }

    #endregion Same two operations with different entity types

    public class StoreValueGenerationWithoutReturningSqliteFixture : StoreValueGenerationSqliteFixture
    {
        protected override string StoreName
            => "StoreValueGenerationWithoutReturningTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                modelBuilder.Entity(entity.Name).ToTable(b => b.UseSqlReturningClause(false));
            }
        }
    }
}
