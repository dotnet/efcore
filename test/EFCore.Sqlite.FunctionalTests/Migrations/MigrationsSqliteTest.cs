// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Scaffolding.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations;

public class MigrationsSqliteTest : MigrationsTestBase<MigrationsSqliteTest.MigrationsSqliteFixture>
{
    public MigrationsSqliteTest(MigrationsSqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Create_table()
    {
        await base.Create_table();

        AssertSql(
            """
CREATE TABLE "People" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NULL
);
""");
    }

    public override async Task Create_table_all_settings()
    {
        await base.Create_table_all_settings();

        AssertSql(
            """
CREATE TABLE "People" (
    -- Table comment

    "CustomId" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT,

    -- Employer ID comment
    "EmployerId" INTEGER NOT NULL,

    "SSN" TEXT COLLATE NOCASE NOT NULL,
    CONSTRAINT "AK_People_SSN" UNIQUE ("SSN"),
    CONSTRAINT "CK_People_EmployerId" CHECK ("EmployerId" > 0),
    CONSTRAINT "FK_People_Employers_EmployerId" FOREIGN KEY ("EmployerId") REFERENCES "Employers" ("Id") ON DELETE CASCADE
);
""",
            //
            """
CREATE INDEX "IX_People_EmployerId" ON "People" ("EmployerId");
""");
    }

    public override async Task Create_table_with_comments()
    {
        await base.Create_table_with_comments();

        AssertSql(
            """
CREATE TABLE "People" (
    -- Table comment

    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT,

    -- Column comment
    "Name" TEXT NULL
);
""");
    }

    public override async Task Create_table_with_multiline_comments()
    {
        await base.Create_table_with_multiline_comments();

        AssertSql(
            """
CREATE TABLE "People" (
    -- This is a multi-line
    -- table comment.
    -- More information can
    -- be found in the docs.

    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT,

    -- This is a multi-line
    -- column comment.
    -- More information can
    -- be found in the docs.
    "Name" TEXT NULL
);
""");
    }

    public override async Task Create_table_with_computed_column(bool? stored)
    {
        await base.Create_table_with_computed_column(stored);

        var computedColumnTypeSql = stored == true ? " STORED" : "";

        AssertSql(
            $"""
CREATE TABLE "People" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT,
    "Sum" AS ("X" + "Y"){computedColumnTypeSql},
    "X" INTEGER NOT NULL,
    "Y" INTEGER NOT NULL
);
""");
    }

    public override async Task Create_table_with_json_column()
    {
        await base.Create_table_with_json_column();

        AssertSql(
            """
CREATE TABLE "Entity" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Entity" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NULL,
    "OwnedCollection" TEXT NULL,
    "OwnedReference" TEXT NULL,
    "OwnedRequiredReference" TEXT NOT NULL
);
""");
    }

    public override async Task Create_table_with_json_column_explicit_json_column_names()
    {
        await base.Create_table_with_json_column_explicit_json_column_names();

        AssertSql(
            """
CREATE TABLE "Entity" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Entity" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NULL,
    "json_collection" TEXT NULL,
    "json_reference" TEXT NULL
);
""");
    }

    public override async Task Alter_table_add_comment()
    {
        await base.Alter_table_add_comment();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    -- Table comment

    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id")
SELECT "Id"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Alter_table_add_comment_non_default_schema()
    {
        await base.Alter_table_add_comment_non_default_schema();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    -- Table comment

    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id")
SELECT "Id"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Alter_table_change_comment()
    {
        await base.Alter_table_change_comment();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    -- Table comment2

    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id")
SELECT "Id"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Alter_table_remove_comment()
    {
        await base.Alter_table_remove_comment();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id")
SELECT "Id"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Rename_table()
    {
        await base.Rename_table();

        AssertSql(
            """
ALTER TABLE "People" RENAME TO "Persons";
""",
            //
            """
CREATE TABLE "ef_temp_Persons" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Persons" PRIMARY KEY AUTOINCREMENT
);
""",
            //
            """
INSERT INTO "ef_temp_Persons" ("Id")
SELECT "Id"
FROM "Persons";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "Persons";
""",
            //
            """
ALTER TABLE "ef_temp_Persons" RENAME TO "Persons";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Rename_table_with_primary_key()
    {
        await base.Rename_table_with_primary_key();

        AssertSql(
            """
ALTER TABLE "People" RENAME TO "Persons";
""",
            //
            """
CREATE TABLE "ef_temp_Persons" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Persons" PRIMARY KEY AUTOINCREMENT
);
""",
            //
            """
INSERT INTO "ef_temp_Persons" ("Id")
SELECT "Id"
FROM "Persons";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "Persons";
""",
            //
            """
ALTER TABLE "ef_temp_Persons" RENAME TO "Persons";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Rename_table_with_json_column()
    {
        await base.Rename_table_with_json_column();

        AssertSql(
            """
ALTER TABLE "Entities" RENAME TO "NewEntities";
""",
            //
            """
CREATE TABLE "ef_temp_NewEntities" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_NewEntities" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NULL,
    "OwnedCollection" TEXT NULL,
    "OwnedReference" TEXT NULL
);
""",
            //
            """
INSERT INTO "ef_temp_NewEntities" ("Id", "Name", "OwnedCollection", "OwnedReference")
SELECT "Id", "Name", "OwnedCollection", "OwnedReference"
FROM "NewEntities";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "NewEntities";
""",
            //
            """
ALTER TABLE "ef_temp_NewEntities" RENAME TO "NewEntities";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    // SQLite does not support schemas.
    public override async Task Move_table()
    {
        await base.Move_table();

        AssertSql();
    }

    // SQLite does not support schemas
    public override async Task Create_schema()
    {
        await base.Create_schema();

        AssertSql(
            """
CREATE TABLE "People" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT
);
""");
    }

    public override async Task Add_column_with_defaultValue_datetime()
    {
        await base.Add_column_with_defaultValue_datetime();

        AssertSql(
            """
ALTER TABLE "People" ADD "Birthday" TEXT NOT NULL DEFAULT '2015-04-12 17:05:00';
""");
    }

    public override async Task Add_column_with_defaultValueSql()
    {
        await base.Add_column_with_defaultValueSql();

        AssertSql(
            """
ALTER TABLE "People" ADD "Sum" INTEGER NOT NULL DEFAULT (1 + 2);
""");
    }

    public override async Task Add_json_columns_to_existing_table()
    {
        await base.Add_json_columns_to_existing_table();

        AssertSql(
            """
ALTER TABLE "Entity" ADD "OwnedCollection" TEXT NULL;
""",
            //
            """
ALTER TABLE "Entity" ADD "OwnedReference" TEXT NULL;
""",
            //
            """
ALTER TABLE "Entity" ADD "OwnedRequiredReference" TEXT NOT NULL DEFAULT '{}';
""");
    }

    public override async Task Add_column_with_computedSql(bool? stored)
    {
        await base.Add_column_with_computedSql(stored);

        var storedSql = stored == true ? " STORED" : "";

        AssertSql(
            $"""
ALTER TABLE "People" ADD "Sum" AS ("X" + "Y"){storedSql};
""");
    }

    public override async Task Add_column_with_max_length()
    {
        await base.Add_column_with_max_length();

        // See issue #3698
        AssertSql(
            """
ALTER TABLE "People" ADD "Name" TEXT NULL;
""");
    }

    public override async Task Add_column_with_comment()
    {
        await base.Add_column_with_comment();

        AssertSql(
            """
ALTER TABLE "People" ADD "FullName" TEXT NULL;
""",
            //
            """
CREATE TABLE "ef_temp_People" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT,

    -- My comment
    "FullName" TEXT NULL
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id", "FullName")
SELECT "Id", "FullName"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Add_column_with_collation()
    {
        await base.Add_column_with_collation();

        AssertSql(
            """
ALTER TABLE "People" ADD "Name" TEXT COLLATE NOCASE NULL;
""");
    }

    public override async Task Add_column_computed_with_collation(bool stored)
    {
        await base.Add_column_computed_with_collation(stored);

        AssertSql(
            stored
                ? """ALTER TABLE "People" ADD "Name" AS ('hello') STORED COLLATE NOCASE;"""
                : """ALTER TABLE "People" ADD "Name" AS ('hello') COLLATE NOCASE;""");
    }

    public override async Task Add_column_with_check_constraint()
    {
        await base.Add_column_with_check_constraint();

        AssertSql(
            """
ALTER TABLE "People" ADD "DriverLicense" INTEGER NOT NULL DEFAULT 0;
""",
            //
            """
CREATE TABLE "ef_temp_People" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT,
    "DriverLicense" INTEGER NOT NULL,
    CONSTRAINT "CK_People_Foo" CHECK ("DriverLicense" > 0)
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id", "DriverLicense")
SELECT "Id", "DriverLicense"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Alter_column_make_required()
    {
        await base.Alter_column_make_required();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT,
    "SomeColumn" TEXT NOT NULL
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id", "SomeColumn")
SELECT "Id", IFNULL("SomeColumn", '')
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Alter_column_make_required_with_index()
    {
        await base.Alter_column_make_required_with_index();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT,
    "SomeColumn" TEXT NOT NULL
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id", "SomeColumn")
SELECT "Id", IFNULL("SomeColumn", '')
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""",
            //
            """
CREATE INDEX "IX_People_SomeColumn" ON "People" ("SomeColumn");
""");
    }

    public override async Task Alter_column_make_required_with_composite_index()
    {
        await base.Alter_column_make_required_with_composite_index();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT,
    "FirstName" TEXT NOT NULL,
    "LastName" TEXT NULL
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id", "FirstName", "LastName")
SELECT "Id", IFNULL("FirstName", ''), "LastName"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""",
            //
            """
CREATE INDEX "IX_People_FirstName_LastName" ON "People" ("FirstName", "LastName");
""");
    }

    public override async Task Alter_column_make_computed(bool? stored)
    {
        await base.Alter_column_make_computed(stored);

        var storedSql = stored == true ? " STORED" : "";

        AssertSql(
            $"""
CREATE TABLE "ef_temp_People" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT,
    "Sum" AS ("X" + "Y"){storedSql},
    "X" INTEGER NOT NULL,
    "Y" INTEGER NOT NULL
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id", "X", "Y")
SELECT "Id", "X", "Y"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Alter_column_change_computed()
    {
        await base.Alter_column_change_computed();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT,
    "Sum" AS ("X" - "Y"),
    "X" INTEGER NOT NULL,
    "Y" INTEGER NOT NULL
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id", "X", "Y")
SELECT "Id", "X", "Y"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Alter_column_change_computed_recreates_indexes()
    {
        await base.Alter_column_change_computed_recreates_indexes();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT,
    "Sum" AS ("X" - "Y"),
    "X" INTEGER NOT NULL,
    "Y" INTEGER NOT NULL
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id", "X", "Y")
SELECT "Id", "X", "Y"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""",
            //
            """
CREATE INDEX "IX_People_Sum" ON "People" ("Sum");
""");
    }

    public override async Task Alter_column_change_computed_type()
    {
        await base.Alter_column_change_computed_type();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT,
    "Sum" AS ("X" + "Y") STORED,
    "X" INTEGER NOT NULL,
    "Y" INTEGER NOT NULL
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id", "X", "Y")
SELECT "Id", "X", "Y"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Alter_column_make_non_computed()
    {
        await base.Alter_column_make_non_computed();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT,
    "Sum" INTEGER NOT NULL,
    "X" INTEGER NOT NULL,
    "Y" INTEGER NOT NULL
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id", "Sum", "X", "Y")
SELECT "Id", "Sum", "X", "Y"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Alter_column_add_comment()
    {
        await base.Alter_column_add_comment();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    -- Some comment
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id")
SELECT "Id"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Alter_computed_column_add_comment()
    {
        await base.Alter_computed_column_add_comment();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT,

    -- Some comment
    "SomeColumn" AS (42)
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id")
SELECT "Id"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Alter_column_change_comment()
    {
        await base.Alter_column_change_comment();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    -- Some comment2
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id")
SELECT "Id"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Alter_column_remove_comment()
    {
        await base.Alter_column_remove_comment();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id")
SELECT "Id"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Alter_column_set_collation()
    {
        await base.Alter_column_set_collation();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "Name" TEXT COLLATE NOCASE NULL
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Name")
SELECT "Name"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Alter_column_reset_collation()
    {
        await base.Alter_column_reset_collation();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "Name" TEXT NULL
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Name")
SELECT "Name"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Convert_json_entities_to_regular_owned()
    {
        await base.Convert_json_entities_to_regular_owned();

AssertSql(
            """
ALTER TABLE "Entity" RENAME COLUMN "OwnedReference" TO "OwnedReference_Date";
""",
                //
                """
ALTER TABLE "Entity" ADD "OwnedReference_NestedReference_Number" INTEGER NULL;
""",
                //
                """
CREATE TABLE "Entity_NestedCollection" (
    "OwnedEntityId" INTEGER NOT NULL,
    "Id" INTEGER NOT NULL,
    "Number2" INTEGER NOT NULL,
    CONSTRAINT "PK_Entity_NestedCollection" PRIMARY KEY ("OwnedEntityId", "Id"),
    CONSTRAINT "FK_Entity_NestedCollection_Entity_OwnedEntityId" FOREIGN KEY ("OwnedEntityId") REFERENCES "Entity" ("Id") ON DELETE CASCADE
);
""",
                //
                """
CREATE TABLE "Entity_OwnedCollection" (
    "EntityId" INTEGER NOT NULL,
    "Id" INTEGER NOT NULL,
    "Date2" TEXT NOT NULL,
    "NestedReference2_Number3" INTEGER NULL,
    CONSTRAINT "PK_Entity_OwnedCollection" PRIMARY KEY ("EntityId", "Id"),
    CONSTRAINT "FK_Entity_OwnedCollection_Entity_EntityId" FOREIGN KEY ("EntityId") REFERENCES "Entity" ("Id") ON DELETE CASCADE
);
""",
                //
                """
CREATE TABLE "Entity_OwnedCollection_NestedCollection2" (
    "Owned2EntityId" INTEGER NOT NULL,
    "Owned2Id" INTEGER NOT NULL,
    "Id" INTEGER NOT NULL,
    "Number4" INTEGER NOT NULL,
    CONSTRAINT "PK_Entity_OwnedCollection_NestedCollection2" PRIMARY KEY ("Owned2EntityId", "Owned2Id", "Id"),
    CONSTRAINT "FK_Entity_OwnedCollection_NestedCollection2_Entity_OwnedCollection_Owned2EntityId_Owned2Id" FOREIGN KEY ("Owned2EntityId", "Owned2Id") REFERENCES "Entity_OwnedCollection" ("EntityId", "Id") ON DELETE CASCADE
);
""",
                //
                """
CREATE TABLE "ef_temp_Entity" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Entity" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NULL,
    "OwnedReference_Date" TEXT NULL,
    "OwnedReference_NestedReference_Number" INTEGER NULL
);
""",
                //
                """
INSERT INTO "ef_temp_Entity" ("Id", "Name", "OwnedReference_Date", "OwnedReference_NestedReference_Number")
SELECT "Id", "Name", "OwnedReference_Date", "OwnedReference_NestedReference_Number"
FROM "Entity";
""",
                //
                """
PRAGMA foreign_keys = 0;
""",
                //
                """
DROP TABLE "Entity";
""",
                //
                """
ALTER TABLE "ef_temp_Entity" RENAME TO "Entity";
""",
                //
                """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Convert_regular_owned_entities_to_json()
    {
        await base.Convert_regular_owned_entities_to_json();

        AssertSql(
            """
DROP TABLE "Entity_NestedCollection";
""",
            //
            """
DROP TABLE "Entity_OwnedCollection_NestedCollection2";
""",
            //
            """
DROP TABLE "Entity_OwnedCollection";
""",
            //
            """
ALTER TABLE "Entity" RENAME COLUMN "OwnedReference_Date" TO "OwnedReference";
""",
            //
            """
ALTER TABLE "Entity" ADD "OwnedCollection" TEXT NULL;
""",
            //
            """
CREATE TABLE "ef_temp_Entity" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Entity" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NULL,
    "OwnedCollection" TEXT NULL,
    "OwnedReference" TEXT NULL
);
""",
            //
            """
INSERT INTO "ef_temp_Entity" ("Id", "Name", "OwnedCollection", "OwnedReference")
SELECT "Id", "Name", "OwnedCollection", "OwnedReference"
FROM "Entity";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "Entity";
""",
            //
            """
ALTER TABLE "ef_temp_Entity" RENAME TO "Entity";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Convert_string_column_to_a_json_column_containing_reference()
    {
        await base.Convert_string_column_to_a_json_column_containing_reference();

        AssertSql();
    }

    public override async Task Convert_string_column_to_a_json_column_containing_required_reference()
    {
        await base.Convert_string_column_to_a_json_column_containing_required_reference();

        AssertSql(
            """
CREATE TABLE "ef_temp_Entity" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Entity" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL
);
""",
            //
            """
INSERT INTO "ef_temp_Entity" ("Id", "Name")
SELECT "Id", IFNULL("Name", '{}')
FROM "Entity";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "Entity";
""",
            //
            """
ALTER TABLE "ef_temp_Entity" RENAME TO "Entity";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Convert_string_column_to_a_json_column_containing_collection()
    {
        await base.Convert_string_column_to_a_json_column_containing_collection();

        AssertSql();
    }

    public override async Task Drop_column()
    {
        await base.Drop_column();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id")
SELECT "Id"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Drop_column_primary_key()
    {
        await base.Drop_column_primary_key();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "SomeColumn" INTEGER NOT NULL
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("SomeColumn")
SELECT "SomeColumn"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Drop_json_columns_from_existing_table()
    {
        await base.Drop_json_columns_from_existing_table();

        AssertSql(
            """
CREATE TABLE "ef_temp_Entity" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Entity" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NULL
);
""",
            //
            """
INSERT INTO "ef_temp_Entity" ("Id", "Name")
SELECT "Id", "Name"
FROM "Entity";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "Entity";
""",
            //
            """
ALTER TABLE "ef_temp_Entity" RENAME TO "Entity";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Rename_column()
    {
        await base.Rename_column();

        AssertSql(
            """
ALTER TABLE "People" RENAME COLUMN "SomeColumn" TO "SomeOtherColumn";
""");
    }

    public override async Task Rename_json_column()
    {
        await base.Rename_json_column();

        AssertSql(
            """
ALTER TABLE "Entity" RENAME COLUMN "json_reference" TO "new_json_reference";
""",
            //
            """
ALTER TABLE "Entity" RENAME COLUMN "json_collection" TO "new_json_collection";
""");
    }

    public override async Task Create_index_with_filter()
    {
        await base.Create_index_with_filter();

        AssertSql(
            """
CREATE INDEX "IX_People_Name" ON "People" ("Name") WHERE "Name" IS NOT NULL;
""");
    }

    public override async Task Create_unique_index_with_filter()
    {
        await base.Create_unique_index_with_filter();

        AssertSql(
            """
CREATE UNIQUE INDEX "IX_People_Name" ON "People" ("Name") WHERE "Name" IS NOT NULL AND "Name" <> '';
""");
    }

    public override async Task Rename_index()
    {
        await base.Rename_index();

        AssertSql(
            """
DROP INDEX "Foo";
""",
            //
            """
CREATE INDEX "foo" ON "People" ("FirstName");
""");
    }

    public override async Task Add_primary_key_int()
    {
        await base.Add_primary_key_int();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "SomeField" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("SomeField")
SELECT "SomeField"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Add_primary_key_string()
    {
        await base.Add_primary_key_string();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "SomeField" TEXT NOT NULL CONSTRAINT "PK_People" PRIMARY KEY
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("SomeField")
SELECT "SomeField"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Add_primary_key_with_name()
    {
        await base.Add_primary_key_with_name();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "SomeField" TEXT NOT NULL CONSTRAINT "PK_Foo" PRIMARY KEY
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("SomeField")
SELECT IFNULL("SomeField", '')
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Add_primary_key_composite_with_name()
    {
        await base.Add_primary_key_composite_with_name();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "SomeField1" INTEGER NOT NULL,
    "SomeField2" INTEGER NOT NULL,
    CONSTRAINT "PK_Foo" PRIMARY KEY ("SomeField1", "SomeField2")
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("SomeField1", "SomeField2")
SELECT "SomeField1", "SomeField2"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Drop_primary_key_int()
    {
        await base.Drop_primary_key_int();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "SomeField" INTEGER NOT NULL
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("SomeField")
SELECT "SomeField"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Drop_primary_key_string()
    {
        await base.Drop_primary_key_string();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "SomeField" TEXT NOT NULL
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("SomeField")
SELECT "SomeField"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Add_foreign_key()
    {
        await base.Add_foreign_key();

        AssertSql(
            """
CREATE INDEX "IX_Orders_CustomerId" ON "Orders" ("CustomerId");
""",
            //
            """
CREATE TABLE "ef_temp_Orders" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Orders" PRIMARY KEY AUTOINCREMENT,
    "CustomerId" INTEGER NOT NULL,
    CONSTRAINT "FK_Orders_Customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE CASCADE
);
""",
            //
            """
INSERT INTO "ef_temp_Orders" ("Id", "CustomerId")
SELECT "Id", "CustomerId"
FROM "Orders";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "Orders";
""",
            //
            """
ALTER TABLE "ef_temp_Orders" RENAME TO "Orders";
""",
            //
            """
PRAGMA foreign_keys = 1;
""",
            //
            """
CREATE INDEX "IX_Orders_CustomerId" ON "Orders" ("CustomerId");
""");
    }

    public override async Task Add_foreign_key_with_name()
    {
        await base.Add_foreign_key_with_name();

        AssertSql(
            """
CREATE INDEX "IX_Orders_CustomerId" ON "Orders" ("CustomerId");
""",
            //
            """
CREATE TABLE "ef_temp_Orders" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Orders" PRIMARY KEY AUTOINCREMENT,
    "CustomerId" INTEGER NOT NULL,
    CONSTRAINT "FK_Foo" FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE CASCADE
);
""",
            //
            """
INSERT INTO "ef_temp_Orders" ("Id", "CustomerId")
SELECT "Id", "CustomerId"
FROM "Orders";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "Orders";
""",
            //
            """
ALTER TABLE "ef_temp_Orders" RENAME TO "Orders";
""",
            //
            """
PRAGMA foreign_keys = 1;
""",
            //
            """
CREATE INDEX "IX_Orders_CustomerId" ON "Orders" ("CustomerId");
""");
    }

    public override async Task Drop_foreign_key()
    {
        await base.Drop_foreign_key();

        AssertSql(
            """
DROP INDEX "IX_Orders_CustomerId";
""",
            //
            """
CREATE TABLE "ef_temp_Orders" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Orders" PRIMARY KEY AUTOINCREMENT,
    "CustomerId" INTEGER NOT NULL
);
""",
            //
            """
INSERT INTO "ef_temp_Orders" ("Id", "CustomerId")
SELECT "Id", "CustomerId"
FROM "Orders";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "Orders";
""",
            //
            """
ALTER TABLE "ef_temp_Orders" RENAME TO "Orders";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Add_unique_constraint()
    {
        await base.Add_unique_constraint();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT,
    "AlternateKeyColumn" INTEGER NOT NULL,
    CONSTRAINT "AK_People_AlternateKeyColumn" UNIQUE ("AlternateKeyColumn")
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id", "AlternateKeyColumn")
SELECT "Id", "AlternateKeyColumn"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Add_unique_constraint_composite_with_name()
    {
        await base.Add_unique_constraint_composite_with_name();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT,
    "AlternateKeyColumn1" INTEGER NOT NULL,
    "AlternateKeyColumn2" INTEGER NOT NULL,
    CONSTRAINT "AK_Foo" UNIQUE ("AlternateKeyColumn1", "AlternateKeyColumn2")
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id", "AlternateKeyColumn1", "AlternateKeyColumn2")
SELECT "Id", "AlternateKeyColumn1", "AlternateKeyColumn2"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Drop_unique_constraint()
    {
        await base.Drop_unique_constraint();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT,
    "AlternateKeyColumn" INTEGER NOT NULL
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id", "AlternateKeyColumn")
SELECT "Id", "AlternateKeyColumn"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Add_check_constraint_with_name()
    {
        await base.Add_check_constraint_with_name();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT,
    "DriverLicense" INTEGER NOT NULL,
    CONSTRAINT "CK_People_Foo" CHECK ("DriverLicense" > 0)
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id", "DriverLicense")
SELECT "Id", "DriverLicense"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Alter_check_constraint()
    {
        await base.Alter_check_constraint();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT,
    "DriverLicense" INTEGER NOT NULL,
    CONSTRAINT "CK_People_Foo" CHECK ("DriverLicense" > 1)
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id", "DriverLicense")
SELECT "Id", "DriverLicense"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    public override async Task Drop_check_constraint()
    {
        await base.Drop_check_constraint();

        AssertSql(
            """
CREATE TABLE "ef_temp_People" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_People" PRIMARY KEY AUTOINCREMENT,
    "DriverLicense" INTEGER NOT NULL
);
""",
            //
            """
INSERT INTO "ef_temp_People" ("Id", "DriverLicense")
SELECT "Id", "DriverLicense"
FROM "People";
""",
            //
            """
PRAGMA foreign_keys = 0;
""",
            //
            """
DROP TABLE "People";
""",
            //
            """
ALTER TABLE "ef_temp_People" RENAME TO "People";
""",
            //
            """
PRAGMA foreign_keys = 1;
""");
    }

    [ConditionalFact]
    public virtual async Task ValueGeneratedOnAdd_on_properties()
    {
        await Test(
            builder => { },
            builder => { },
            builder => builder.Entity(
                "Person", e =>
                {
                    e.Property<int>("Id").ValueGeneratedOnAdd();
                    e.Property<string>("Name");
                    e.Property<int>("Age").HasDefaultValue(18);
                    e.HasKey("Id");
                }),
            model =>
            {
                var personTable = Assert.Single(model.Tables);
                Assert.Equal(ValueGenerated.OnAdd, personTable.Columns.Single(c => c.Name == "Id").ValueGenerated);
                Assert.Null(personTable.Columns.Single(c => c.Name == "Age").ValueGenerated);
                Assert.NotNull(personTable.Columns.Single(c => c.Name == "Age").DefaultValueSql);
            });

        AssertSql(
            """
CREATE TABLE "Person" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Person" PRIMARY KEY AUTOINCREMENT,
    "Age" INTEGER NOT NULL DEFAULT 18,
    "Name" TEXT NULL
);
""");
    }

    public override async Task Create_table_with_complex_type_with_required_properties_on_derived_entity_in_TPH()
    {
        await base.Create_table_with_complex_type_with_required_properties_on_derived_entity_in_TPH();

        AssertSql(
"""
CREATE TABLE "Contacts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Contacts" PRIMARY KEY AUTOINCREMENT,
    "Discriminator" TEXT NOT NULL,
    "Name" TEXT NULL,
    "Number" INTEGER NULL,
    "MyComplex_Prop" TEXT NULL,
    "MyComplex_MyNestedComplex_Bar" TEXT NULL,
    "MyComplex_MyNestedComplex_Foo" INTEGER NULL
);
""");
    }

    public override Task Create_sequence()
        => AssertNotSupportedAsync(base.Create_sequence, SqliteStrings.SequencesNotSupported);

    public override Task Create_sequence_long()
        => AssertNotSupportedAsync(base.Create_sequence_long, SqliteStrings.SequencesNotSupported);

    public override Task Create_sequence_short()
        => AssertNotSupportedAsync(base.Create_sequence_short, SqliteStrings.SequencesNotSupported);

    public override Task Create_sequence_all_settings()
        => AssertNotSupportedAsync(base.Create_sequence_all_settings, SqliteStrings.SequencesNotSupported);

    public override Task Alter_sequence_all_settings()
        => AssertNotSupportedAsync(base.Alter_sequence_all_settings, SqliteStrings.SequencesNotSupported);

    public override Task Alter_sequence_increment_by()
        => AssertNotSupportedAsync(base.Alter_sequence_increment_by, SqliteStrings.SequencesNotSupported);

    public override Task Alter_sequence_cache_to_default_cache()
        => AssertNotSupportedAsync(base.Alter_sequence_cache_to_default_cache, SqliteStrings.SequencesNotSupported);

    public override Task Alter_sequence_cache_to_nocache()
        => AssertNotSupportedAsync(base.Alter_sequence_cache_to_nocache, SqliteStrings.SequencesNotSupported);

    public override Task Alter_sequence_default_cache_to_cache()
        => AssertNotSupportedAsync(base.Alter_sequence_default_cache_to_cache, SqliteStrings.SequencesNotSupported);

    public override Task Alter_sequence_default_cache_to_nocache()
        => AssertNotSupportedAsync(base.Alter_sequence_default_cache_to_nocache, SqliteStrings.SequencesNotSupported);

    public override Task Alter_sequence_nocache_to_cache()
        => AssertNotSupportedAsync(base.Alter_sequence_nocache_to_cache, SqliteStrings.SequencesNotSupported);

    public override Task Alter_sequence_nocache_to_default_cache()
        => AssertNotSupportedAsync(base.Alter_sequence_nocache_to_default_cache, SqliteStrings.SequencesNotSupported);

    public override Task Create_sequence_cache()
        => AssertNotSupportedAsync(base.Create_sequence_cache, SqliteStrings.SequencesNotSupported);

    public override Task Create_sequence_default_cache()
        => AssertNotSupportedAsync(base.Create_sequence_default_cache, SqliteStrings.SequencesNotSupported);

    public override Task Create_sequence_nocache()
        => AssertNotSupportedAsync(base.Create_sequence_nocache, SqliteStrings.SequencesNotSupported);

    public override Task Alter_sequence_restart_with()
        => AssertNotSupportedAsync(base.Alter_sequence_restart_with, SqliteStrings.SequencesNotSupported);

    public override Task Drop_sequence()
        => AssertNotSupportedAsync(base.Drop_sequence, SqliteStrings.SequencesNotSupported);

    public override Task Rename_sequence()
        => AssertNotSupportedAsync(base.Rename_sequence, SqliteStrings.SequencesNotSupported);

    public override Task Move_sequence()
        => AssertNotSupportedAsync(base.Move_sequence, SqliteStrings.SequencesNotSupported);

    [ConditionalFact]
    public override async Task Add_required_primitve_collection_to_existing_table()
    {
        await base.Add_required_primitve_collection_to_existing_table();

        AssertSql(
"""
ALTER TABLE "Customers" ADD "Numbers" TEXT NOT NULL DEFAULT '[]';
""");
    }

    [ConditionalFact]
    public override async Task Add_required_primitve_collection_with_custom_default_value_to_existing_table()
    {
        await base.Add_required_primitve_collection_with_custom_default_value_to_existing_table();

        AssertSql(
"""
ALTER TABLE "Customers" ADD "Numbers" TEXT NOT NULL DEFAULT '[1,2,3]';
""");
    }

    [ConditionalFact]
    public override async Task Add_required_primitve_collection_with_custom_default_value_sql_to_existing_table()
    {
        await base.Add_required_primitve_collection_with_custom_default_value_sql_to_existing_table_core("'[3, 2, 1]'");

        AssertSql(
"""
ALTER TABLE "Customers" ADD "Numbers" TEXT NOT NULL DEFAULT ('[3, 2, 1]');
""");
    }

    [ConditionalFact(Skip = "issue #33038")]
    public override async Task Add_required_primitve_collection_with_custom_converter_to_existing_table()
    {
        await base.Add_required_primitve_collection_with_custom_converter_to_existing_table();

        AssertSql(
"""
ALTER TABLE [Customers] ADD [Numbers] nvarchar(max) NOT NULL DEFAULT N'nothing';
""");
    }

    [ConditionalFact]
    public override async Task Add_required_primitve_collection_with_custom_converter_and_custom_default_value_to_existing_table()
    {
        await base.Add_required_primitve_collection_with_custom_converter_and_custom_default_value_to_existing_table();

        AssertSql(
"""
ALTER TABLE "Customers" ADD "Numbers" TEXT NOT NULL DEFAULT 'some numbers';
""");
    }

    [ConditionalFact]
    public override async Task Add_required_primitive_collection_to_existing_table()
    {
        await base.Add_required_primitive_collection_to_existing_table();

        AssertSql(
"""
ALTER TABLE "Customers" ADD "Numbers" TEXT NOT NULL DEFAULT '[]';
""");
    }

    [ConditionalFact]
    public override async Task Add_required_primitive_collection_with_custom_default_value_to_existing_table()
    {
        await base.Add_required_primitive_collection_with_custom_default_value_to_existing_table();

        AssertSql(
"""
ALTER TABLE "Customers" ADD "Numbers" TEXT NOT NULL DEFAULT '[1,2,3]';
""");
    }

    [ConditionalFact]
    public override async Task Add_required_primitive_collection_with_custom_default_value_sql_to_existing_table()
    {
        await base.Add_required_primitive_collection_with_custom_default_value_sql_to_existing_table_core("'[3, 2, 1]'");

        AssertSql(
"""
ALTER TABLE "Customers" ADD "Numbers" TEXT NOT NULL DEFAULT ('[3, 2, 1]');
""");
    }

    [ConditionalFact(Skip = "issue #33038")]
    public override async Task Add_required_primitive_collection_with_custom_converter_to_existing_table()
    {
        await base.Add_required_primitive_collection_with_custom_converter_to_existing_table();

        AssertSql(
"""
ALTER TABLE [Customers] ADD [Numbers] nvarchar(max) NOT NULL DEFAULT N'nothing';
""");
    }

    [ConditionalFact]
    public override async Task Add_required_primitive_collection_with_custom_converter_and_custom_default_value_to_existing_table()
    {
        await base.Add_required_primitive_collection_with_custom_converter_and_custom_default_value_to_existing_table();

        AssertSql(
"""
ALTER TABLE "Customers" ADD "Numbers" TEXT NOT NULL DEFAULT 'some numbers';
""");
    }

    [ConditionalFact]
    public override async Task Add_optional_primitive_collection_to_existing_table()
    {
        await base.Add_optional_primitive_collection_to_existing_table();

        AssertSql(
"""
ALTER TABLE "Customers" ADD "Numbers" TEXT NULL;
""");
    }

    [ConditionalFact]
    public override async Task Create_table_with_required_primitive_collection()
    {
        await base.Create_table_with_required_primitive_collection();

        AssertSql(
"""
CREATE TABLE "Customers" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Customers" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NULL,
    "Numbers" TEXT NOT NULL
);
""");
    }

    [ConditionalFact]
    public override async Task Create_table_with_optional_primitive_collection()
    {
        await base.Create_table_with_optional_primitive_collection();

        AssertSql(
"""
CREATE TABLE "Customers" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Customers" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NULL,
    "Numbers" TEXT NULL
);
""");
    }

    // SQLite does not support schemas
    protected override bool AssertSchemaNames
        => false;

    // Reverse-engineering of comments isn't supported in Sqlite
    protected override bool AssertComments
        => false;

    // Reverse engineering of computed columns isn't fully supported on SQLite
    protected override bool AssertComputedColumns
        => false;

    // Our current version Sqlite doesn't seem to support scaffolding collations
    protected override bool AssertCollations
        => false;

    // Reverse engineering of index filters isn't supported in SQLite
    protected override bool AssertIndexFilters
        => false;

    // Reverse engineering of constraint names isn't supported in SQLite
    protected override bool AssertConstraintNames
        => false;

    protected override string NonDefaultCollation
        => "NOCASE";

    protected virtual async Task AssertNotSupportedAsync(Func<Task> action, string? message = null)
    {
        var ex = await Assert.ThrowsAsync<NotSupportedException>(action);
        if (message != null)
        {
            Assert.Equal(message, ex.Message);
        }
    }

    public class MigrationsSqliteFixture : MigrationsFixtureBase
    {
        protected override string StoreName
            => nameof(MigrationsSqliteTest);

        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        public override RelationalTestHelpers TestHelpers
            => SqliteTestHelpers.Instance;

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection)
                .AddScoped<IDatabaseModelFactory, SqliteDatabaseModelFactory>();
    }
}
