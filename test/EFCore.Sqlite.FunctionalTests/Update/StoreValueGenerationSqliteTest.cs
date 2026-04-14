// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

// Newer Sqlite versions support the RETURNING clause, so we use those (see StoreValueGenerationLegacySqliteTest for older Sqlite versions)
[SqliteVersionCondition(Min = "3.35.0")]
public class StoreValueGenerationSqliteTest : StoreValueGenerationTestBase<StoreValueGenerationSqliteFixture>
{
    public StoreValueGenerationSqliteTest(StoreValueGenerationSqliteFixture fixture, ITestOutputHelper testOutputHelper)
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

    #region Single operation

    public override async Task Add_with_generated_values(bool async)
    {
        await base.Add_with_generated_values(async);

        AssertSql(
            """
@p0='1000'

INSERT INTO "WithSomeDatabaseGenerated" ("Data2")
VALUES (@p0)
RETURNING "Id", "Data1";
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
""");
    }

    public override async Task Add_with_all_generated_values(bool async)
    {
        await base.Add_with_all_generated_values(async);

        AssertSql(
            """
INSERT INTO "WithAllDatabaseGenerated"
DEFAULT VALUES
RETURNING "Id", "Data1", "Data2";
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
WHERE "Id" = @p1
RETURNING "Data1";
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
WHERE "Id" = @p2
RETURNING 1;
""");
    }

    public override async Task Delete(bool async)
    {
        await base.Delete(async);

        AssertSql(
            """
@p0='1'

DELETE FROM "WithSomeDatabaseGenerated"
WHERE "Id" = @p0
RETURNING 1;
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
VALUES (@p0)
RETURNING "Id", "Data1";
""",
            //
            """
@p0='1001'

INSERT INTO "WithSomeDatabaseGenerated" ("Data2")
VALUES (@p0)
RETURNING "Id", "Data1";
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
""",
            //
            """
@p0='101'
@p1='1001'
@p2='1001'

INSERT INTO "WithNoDatabaseGenerated" ("Id", "Data1", "Data2")
VALUES (@p0, @p1, @p2);
""");
    }

    public override async Task Add_Add_with_same_entity_type_and_all_generated_values(bool async)
    {
        await base.Add_Add_with_same_entity_type_and_all_generated_values(async);

        AssertSql(
            """
INSERT INTO "WithAllDatabaseGenerated"
DEFAULT VALUES
RETURNING "Id", "Data1", "Data2";
""",
            //
            """
INSERT INTO "WithAllDatabaseGenerated"
DEFAULT VALUES
RETURNING "Id", "Data1", "Data2";
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
WHERE "Id" = @p1
RETURNING "Data1";
""",
            //
            """
@p1='2'
@p0='1001'

UPDATE "WithSomeDatabaseGenerated" SET "Data2" = @p0
WHERE "Id" = @p1
RETURNING "Data1";
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
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
@p2='2'
@p0='1001'
@p1='1001'

UPDATE "WithNoDatabaseGenerated" SET "Data1" = @p0, "Data2" = @p1
WHERE "Id" = @p2
RETURNING 1;
""");
    }

    public override async Task Delete_Delete_with_same_entity_type(bool async)
    {
        await base.Delete_Delete_with_same_entity_type(async);

        AssertSql(
            """
@p0='1'

DELETE FROM "WithSomeDatabaseGenerated"
WHERE "Id" = @p0
RETURNING 1;
""",
            //
            """
@p0='2'

DELETE FROM "WithSomeDatabaseGenerated"
WHERE "Id" = @p0
RETURNING 1;
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
VALUES (@p0)
RETURNING "Id", "Data1";
""",
            //
            """
@p0='1001'

INSERT INTO "WithSomeDatabaseGenerated2" ("Data2")
VALUES (@p0)
RETURNING "Id", "Data1";
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
""",
            //
            """
@p0='101'
@p1='1001'
@p2='1001'

INSERT INTO "WithNoDatabaseGenerated2" ("Id", "Data1", "Data2")
VALUES (@p0, @p1, @p2);
""");
    }

    public override async Task Add_Add_with_different_entity_types_and_all_generated_values(bool async)
    {
        await base.Add_Add_with_different_entity_types_and_all_generated_values(async);

        AssertSql(
            """
INSERT INTO "WithAllDatabaseGenerated"
DEFAULT VALUES
RETURNING "Id", "Data1", "Data2";
""",
            //
            """
INSERT INTO "WithAllDatabaseGenerated2"
DEFAULT VALUES
RETURNING "Id", "Data1", "Data2";
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
WHERE "Id" = @p1
RETURNING "Data1";
""",
            //
            """
@p1='2'
@p0='1001'

UPDATE "WithSomeDatabaseGenerated2" SET "Data2" = @p0
WHERE "Id" = @p1
RETURNING "Data1";
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
WHERE "Id" = @p2
RETURNING 1;
""",
            //
            """
@p2='2'
@p0='1001'
@p1='1001'

UPDATE "WithNoDatabaseGenerated2" SET "Data1" = @p0, "Data2" = @p1
WHERE "Id" = @p2
RETURNING 1;
""");
    }

    public override async Task Delete_Delete_with_different_entity_types(bool async)
    {
        await base.Delete_Delete_with_different_entity_types(async);

        AssertSql(
            """
@p0='1'

DELETE FROM "WithSomeDatabaseGenerated"
WHERE "Id" = @p0
RETURNING 1;
""",
            //
            """
@p0='2'

DELETE FROM "WithSomeDatabaseGenerated2"
WHERE "Id" = @p0
RETURNING 1;
""");
    }

    #endregion Same two operations with different entity types
}
