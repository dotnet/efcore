// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

#nullable enable

namespace Microsoft.EntityFrameworkCore
{
    public class MigrationsSqliteTest : MigrationsTestBase<MigrationsSqliteTest.MigrationsSqliteFixture>
    {
        public MigrationsSqliteTest(MigrationsSqliteFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Create_table()
        {
            await base.Create_table();

            AssertSql(
                @"CREATE TABLE ""People"" (
    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_People"" PRIMARY KEY AUTOINCREMENT,
    ""Name"" TEXT NULL
);");
        }

        public override async Task Create_table_all_settings()
        {
            await base.Create_table_all_settings();

            AssertSql(
                @"CREATE TABLE ""People"" (
    -- Table comment

    ""CustomId"" INTEGER NOT NULL CONSTRAINT ""PK_People"" PRIMARY KEY,

    -- Employer ID comment
    ""EmployerId"" INTEGER NOT NULL,

    ""SSN"" TEXT COLLATE NOCASE NOT NULL,
    CONSTRAINT ""AK_People_SSN"" UNIQUE (""SSN""),
    CONSTRAINT ""CK_EmployerId"" CHECK (""EmployerId"" > 0),
    CONSTRAINT ""FK_People_Employers_EmployerId"" FOREIGN KEY (""EmployerId"") REFERENCES ""Employers"" (""Id"") ON DELETE RESTRICT
);");
        }

        public override async Task Create_table_with_comments()
        {
            await base.Create_table_with_comments();

            AssertSql(
                @"CREATE TABLE ""People"" (
    -- Table comment

    ""Id"" INTEGER NOT NULL,

    -- Column comment
    ""Name"" TEXT NULL
);");
        }

        public override async Task Create_table_with_multiline_comments()
        {
            await base.Create_table_with_multiline_comments();

            AssertSql(
                @"CREATE TABLE ""People"" (
    -- This is a multi-line
    -- table comment.
    -- More information can
    -- be found in the docs.

    ""Id"" INTEGER NOT NULL,

    -- This is a multi-line
    -- column comment.
    -- More information can
    -- be found in the docs.
    ""Name"" TEXT NULL
);");
        }

        public override async Task Create_table_with_computed_column(bool? stored)
        {
            await base.Create_table_with_computed_column(stored);

            var computedColumnTypeSql = stored == true ? " STORED" : "";

            AssertSql(
                $@"CREATE TABLE ""People"" (
    ""Id"" INTEGER NOT NULL,
    ""Sum"" AS (""X"" + ""Y""){computedColumnTypeSql},
    ""X"" INTEGER NOT NULL,
    ""Y"" INTEGER NOT NULL
);");
        }

        public override async Task Alter_table_add_comment()
        {
            await base.Alter_table_add_comment();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    -- Table comment

    ""Id"" INTEGER NOT NULL
);",
                @"INSERT INTO ""ef_temp_People"" (""Id"")
SELECT ""Id""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Alter_table_add_comment_non_default_schema()
        {
            await base.Alter_table_add_comment_non_default_schema();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    -- Table comment

    ""Id"" INTEGER NOT NULL
);",
                @"INSERT INTO ""ef_temp_People"" (""Id"")
SELECT ""Id""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Alter_table_change_comment()
        {
            await base.Alter_table_change_comment();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    -- Table comment2

    ""Id"" INTEGER NOT NULL
);",
                @"INSERT INTO ""ef_temp_People"" (""Id"")
SELECT ""Id""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Alter_table_remove_comment()
        {
            await base.Alter_table_remove_comment();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    ""Id"" INTEGER NOT NULL
);",
                @"INSERT INTO ""ef_temp_People"" (""Id"")
SELECT ""Id""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Rename_table()
        {
            await base.Rename_table();

            AssertSql(
                @"ALTER TABLE ""People"" RENAME TO ""Persons"";");
        }

        public override async Task Rename_table_with_primary_key()
        {
            await base.Rename_table_with_primary_key();

            AssertSql(
                @"ALTER TABLE ""People"" RENAME TO ""Persons"";",
                //
                @"CREATE TABLE ""ef_temp_Persons"" (
    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Persons"" PRIMARY KEY
);",
                //
                @"INSERT INTO ""ef_temp_Persons"" (""Id"")
SELECT ""Id""
FROM Persons;",
                //
                @"PRAGMA foreign_keys = 0;",
                //
                @"DROP TABLE ""Persons"";",
                //
                @"ALTER TABLE ""ef_temp_Persons"" RENAME TO ""Persons"";",
                //
                @"PRAGMA foreign_keys = 1;");
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
                @"CREATE TABLE ""People"" (
    ""Id"" INTEGER NOT NULL
);");
        }

        public override async Task Add_column_with_defaultValue_datetime()
        {
            await base.Add_column_with_defaultValue_datetime();

            AssertSql(
                @"ALTER TABLE ""People"" ADD ""Birthday"" TEXT NOT NULL DEFAULT '2015-04-12 17:05:00';");
        }

        public override async Task Add_column_with_defaultValueSql()
        {
            await base.Add_column_with_defaultValueSql();

            AssertSql(
                @"ALTER TABLE ""People"" ADD ""Sum"" INTEGER NOT NULL DEFAULT (1 + 2);");
        }

        public override async Task Add_column_with_computedSql(bool? stored)
        {
            await base.Add_column_with_computedSql(stored);

            var storedSql = stored == true ? " STORED" : "";

            AssertSql(
                $@"ALTER TABLE ""People"" ADD ""Sum"" AS (""X"" + ""Y""){storedSql};");
        }

        public override async Task Add_column_with_max_length()
        {
            await base.Add_column_with_max_length();

            // See issue #3698
            AssertSql(
                @"ALTER TABLE ""People"" ADD ""Name"" TEXT NULL;");
        }

        public override async Task Add_column_with_comment()
        {
            await base.Add_column_with_comment();

            AssertSql(
                @"ALTER TABLE ""People"" ADD ""FullName"" TEXT NULL;",
                //
                @"CREATE TABLE ""ef_temp_People"" (
    -- My comment
    ""FullName"" TEXT NULL,

    ""Id"" INTEGER NOT NULL
);",
                //
                @"INSERT INTO ""ef_temp_People"" (""FullName"", ""Id"")
SELECT ""FullName"", ""Id""
FROM People;",
                //
                @"PRAGMA foreign_keys = 0;",
                //
                @"DROP TABLE ""People"";",
                //
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                //
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Add_column_with_collation()
        {
            await base.Add_column_with_collation();

            AssertSql(
                @"ALTER TABLE ""People"" ADD ""Name"" TEXT COLLATE NOCASE NULL;");
        }

        public override async Task Add_column_computed_with_collation()
        {
            await base.Add_column_computed_with_collation();

            AssertSql(
                @"ALTER TABLE ""People"" ADD ""Name"" AS ('hello') COLLATE NOCASE;");
        }

        public override async Task Add_column_with_check_constraint()
        {
            await base.Add_column_with_check_constraint();

            AssertSql(
                @"ALTER TABLE ""People"" ADD ""DriverLicense"" INTEGER NOT NULL DEFAULT 0;",
                @"CREATE TABLE ""ef_temp_People"" (
    ""DriverLicense"" INTEGER NOT NULL,
    ""Id"" INTEGER NOT NULL,
    CONSTRAINT ""CK_Foo"" CHECK (""DriverLicense"" > 0)
);",
                @"INSERT INTO ""ef_temp_People"" (""DriverLicense"", ""Id"")
SELECT ""DriverLicense"", ""Id""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Alter_column_make_required()
        {
            await base.Alter_column_make_required();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    ""Id"" INTEGER NOT NULL,
    ""SomeColumn"" TEXT NOT NULL
);",
                @"INSERT INTO ""ef_temp_People"" (""Id"", ""SomeColumn"")
SELECT ""Id"", IFNULL(""SomeColumn"", '')
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Alter_column_make_required_with_index()
        {
            await base.Alter_column_make_required_with_index();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    ""Id"" INTEGER NOT NULL,
    ""SomeColumn"" TEXT NOT NULL
);",
                @"INSERT INTO ""ef_temp_People"" (""Id"", ""SomeColumn"")
SELECT ""Id"", IFNULL(""SomeColumn"", '')
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;",
                @"CREATE INDEX ""IX_People_SomeColumn"" ON ""People"" (""SomeColumn"");");
        }

        public override async Task Alter_column_make_required_with_composite_index()
        {
            await base.Alter_column_make_required_with_composite_index();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    ""FirstName"" TEXT NOT NULL,
    ""Id"" INTEGER NOT NULL,
    ""LastName"" TEXT NULL
);",
                @"INSERT INTO ""ef_temp_People"" (""FirstName"", ""Id"", ""LastName"")
SELECT IFNULL(""FirstName"", ''), ""Id"", ""LastName""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;",
                @"CREATE INDEX ""IX_People_FirstName_LastName"" ON ""People"" (""FirstName"", ""LastName"");");
        }

        public override async Task Alter_column_make_computed(bool? stored)
        {
            await base.Alter_column_make_computed(stored);

            var storedSql = stored == true ? " STORED" : "";

            AssertSql(
                $@"CREATE TABLE ""ef_temp_People"" (
    ""Id"" INTEGER NOT NULL,
    ""Sum"" AS (""X"" + ""Y""){storedSql},
    ""X"" INTEGER NOT NULL,
    ""Y"" INTEGER NOT NULL
);",
                @"INSERT INTO ""ef_temp_People"" (""Id"", ""X"", ""Y"")
SELECT ""Id"", ""X"", ""Y""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Alter_column_change_computed()
        {
            await base.Alter_column_change_computed();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    ""Id"" INTEGER NOT NULL,
    ""Sum"" AS (""X"" - ""Y""),
    ""X"" INTEGER NOT NULL,
    ""Y"" INTEGER NOT NULL
);",
                @"INSERT INTO ""ef_temp_People"" (""Id"", ""X"", ""Y"")
SELECT ""Id"", ""X"", ""Y""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Alter_column_change_computed_type()
        {
            await base.Alter_column_change_computed_type();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    ""Id"" INTEGER NOT NULL,
    ""Sum"" AS (""X"" + ""Y"") STORED,
    ""X"" INTEGER NOT NULL,
    ""Y"" INTEGER NOT NULL
);",
                @"INSERT INTO ""ef_temp_People"" (""Id"", ""X"", ""Y"")
SELECT ""Id"", ""X"", ""Y""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Alter_column_add_comment()
        {
            await base.Alter_column_add_comment();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    -- Some comment
    ""Id"" INTEGER NOT NULL
);",
                @"INSERT INTO ""ef_temp_People"" (""Id"")
SELECT ""Id""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Alter_column_change_comment()
        {
            await base.Alter_column_change_comment();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    -- Some comment2
    ""Id"" INTEGER NOT NULL
);",
                @"INSERT INTO ""ef_temp_People"" (""Id"")
SELECT ""Id""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Alter_column_remove_comment()
        {
            await base.Alter_column_remove_comment();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    ""Id"" INTEGER NOT NULL
);",
                @"INSERT INTO ""ef_temp_People"" (""Id"")
SELECT ""Id""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Alter_column_set_collation()
        {
            await base.Alter_column_set_collation();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    ""Name"" TEXT COLLATE NOCASE NULL
);",
                @"INSERT INTO ""ef_temp_People"" (""Name"")
SELECT ""Name""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Alter_column_reset_collation()
        {
            await base.Alter_column_reset_collation();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    ""Name"" TEXT NULL
);",
                @"INSERT INTO ""ef_temp_People"" (""Name"")
SELECT ""Name""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Drop_column()
        {
            await base.Drop_column();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    ""Id"" INTEGER NOT NULL
);",
                @"INSERT INTO ""ef_temp_People"" (""Id"")
SELECT ""Id""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Drop_column_primary_key()
        {
            await base.Drop_column_primary_key();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    ""SomeColumn"" INTEGER NOT NULL
);",
                @"INSERT INTO ""ef_temp_People"" (""SomeColumn"")
SELECT ""SomeColumn""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Rename_column()
        {
            await base.Rename_column();

            AssertSql(
                @"ALTER TABLE ""People"" RENAME COLUMN ""SomeColumn"" TO ""SomeOtherColumn"";");
        }

        public override async Task Create_index_with_filter()
        {
            await base.Create_index_with_filter();

            AssertSql(
                @"CREATE INDEX ""IX_People_Name"" ON ""People"" (""Name"") WHERE ""Name"" IS NOT NULL;");
        }

        public override async Task Create_unique_index_with_filter()
        {
            await base.Create_unique_index_with_filter();

            AssertSql(
                @"CREATE UNIQUE INDEX ""IX_People_Name"" ON ""People"" (""Name"") WHERE ""Name"" IS NOT NULL AND ""Name"" <> '';");
        }

        public override async Task Rename_index()
        {
            await base.Rename_index();

            AssertSql(
                @"DROP INDEX ""Foo"";",
                //
                @"CREATE INDEX ""foo"" ON ""People"" (""FirstName"");");
        }

        public override async Task Add_primary_key()
        {
            await base.Add_primary_key();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    ""SomeField"" INTEGER NOT NULL CONSTRAINT ""PK_People"" PRIMARY KEY
);",
                @"INSERT INTO ""ef_temp_People"" (""SomeField"")
SELECT ""SomeField""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Add_primary_key_with_name()
        {
            await base.Add_primary_key_with_name();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    ""SomeField"" INTEGER NOT NULL CONSTRAINT ""PK_Foo"" PRIMARY KEY
);",
                @"INSERT INTO ""ef_temp_People"" (""SomeField"")
SELECT ""SomeField""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Add_primary_key_composite_with_name()
        {
            await base.Add_primary_key_composite_with_name();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    ""SomeField1"" INTEGER NOT NULL,
    ""SomeField2"" INTEGER NOT NULL,
    CONSTRAINT ""PK_Foo"" PRIMARY KEY (""SomeField1"", ""SomeField2"")
);",
                @"INSERT INTO ""ef_temp_People"" (""SomeField1"", ""SomeField2"")
SELECT ""SomeField1"", ""SomeField2""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Drop_primary_key()
        {
            await base.Drop_primary_key();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    ""SomeField"" INTEGER NOT NULL
);",
                @"INSERT INTO ""ef_temp_People"" (""SomeField"")
SELECT ""SomeField""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Add_foreign_key()
        {
            await base.Add_foreign_key();

            AssertSql(
                @"CREATE TABLE ""ef_temp_Orders"" (
    ""CustomerId"" INTEGER NOT NULL,
    ""Id"" INTEGER NOT NULL,
    CONSTRAINT ""FK_Orders_Customers_CustomerId"" FOREIGN KEY (""CustomerId"") REFERENCES ""Customers"" (""Id"") ON DELETE RESTRICT
);",
                @"INSERT INTO ""ef_temp_Orders"" (""CustomerId"", ""Id"")
SELECT ""CustomerId"", ""Id""
FROM Orders;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""Orders"";",
                @"ALTER TABLE ""ef_temp_Orders"" RENAME TO ""Orders"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Add_foreign_key_with_name()
        {
            await base.Add_foreign_key_with_name();

            AssertSql(
                @"CREATE TABLE ""ef_temp_Orders"" (
    ""CustomerId"" INTEGER NOT NULL,
    ""Id"" INTEGER NOT NULL,
    CONSTRAINT ""FK_Foo"" FOREIGN KEY (""CustomerId"") REFERENCES ""Customers"" (""Id"") ON DELETE RESTRICT
);",
                @"INSERT INTO ""ef_temp_Orders"" (""CustomerId"", ""Id"")
SELECT ""CustomerId"", ""Id""
FROM Orders;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""Orders"";",
                @"ALTER TABLE ""ef_temp_Orders"" RENAME TO ""Orders"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Drop_foreign_key()
        {
            await base.Drop_foreign_key();

            AssertSql(
                @"CREATE TABLE ""ef_temp_Orders"" (
    ""CustomerId"" INTEGER NOT NULL,
    ""Id"" INTEGER NOT NULL
);",
                @"INSERT INTO ""ef_temp_Orders"" (""CustomerId"", ""Id"")
SELECT ""CustomerId"", ""Id""
FROM Orders;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""Orders"";",
                @"ALTER TABLE ""ef_temp_Orders"" RENAME TO ""Orders"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Add_unique_constraint()
        {
            await base.Add_unique_constraint();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    ""AlternateKeyColumn"" INTEGER NOT NULL,
    ""Id"" INTEGER NOT NULL,
    CONSTRAINT ""AK_People_AlternateKeyColumn"" UNIQUE (""AlternateKeyColumn"")
);",
                @"INSERT INTO ""ef_temp_People"" (""AlternateKeyColumn"", ""Id"")
SELECT ""AlternateKeyColumn"", ""Id""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Add_unique_constraint_composite_with_name()
        {
            await base.Add_unique_constraint_composite_with_name();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    ""AlternateKeyColumn1"" INTEGER NOT NULL,
    ""AlternateKeyColumn2"" INTEGER NOT NULL,
    ""Id"" INTEGER NOT NULL,
    CONSTRAINT ""AK_Foo"" UNIQUE (""AlternateKeyColumn1"", ""AlternateKeyColumn2"")
);",
                @"INSERT INTO ""ef_temp_People"" (""AlternateKeyColumn1"", ""AlternateKeyColumn2"", ""Id"")
SELECT ""AlternateKeyColumn1"", ""AlternateKeyColumn2"", ""Id""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Drop_unique_constraint()
        {
            await base.Drop_unique_constraint();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    ""AlternateKeyColumn"" INTEGER NOT NULL,
    ""Id"" INTEGER NOT NULL
);",
                @"INSERT INTO ""ef_temp_People"" (""AlternateKeyColumn"", ""Id"")
SELECT ""AlternateKeyColumn"", ""Id""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Add_check_constraint_with_name()
        {
            await base.Add_check_constraint_with_name();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    ""DriverLicense"" INTEGER NOT NULL,
    ""Id"" INTEGER NOT NULL,
    CONSTRAINT ""CK_Foo"" CHECK (""DriverLicense"" > 0)
);",
                @"INSERT INTO ""ef_temp_People"" (""DriverLicense"", ""Id"")
SELECT ""DriverLicense"", ""Id""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Alter_check_constraint()
        {
            await base.Alter_check_constraint();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    ""DriverLicense"" INTEGER NOT NULL,
    ""Id"" INTEGER NOT NULL,
    CONSTRAINT ""CK_Foo"" CHECK (""DriverLicense"" > 1)
);",
                @"INSERT INTO ""ef_temp_People"" (""DriverLicense"", ""Id"")
SELECT ""DriverLicense"", ""Id""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
        }

        public override async Task Drop_check_constraint()
        {
            await base.Drop_check_constraint();

            AssertSql(
                @"CREATE TABLE ""ef_temp_People"" (
    ""DriverLicense"" INTEGER NOT NULL,
    ""Id"" INTEGER NOT NULL
);",
                @"INSERT INTO ""ef_temp_People"" (""DriverLicense"", ""Id"")
SELECT ""DriverLicense"", ""Id""
FROM People;",
                @"PRAGMA foreign_keys = 0;",
                @"DROP TABLE ""People"";",
                @"ALTER TABLE ""ef_temp_People"" RENAME TO ""People"";",
                @"PRAGMA foreign_keys = 1;");
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
                @"CREATE TABLE ""Person"" (
    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Person"" PRIMARY KEY AUTOINCREMENT,
    ""Age"" INTEGER NOT NULL DEFAULT 18,
    ""Name"" TEXT NULL
);");
        }

        public override Task Create_sequence()
            => AssertNotSupportedAsync(base.Create_sequence, SqliteStrings.SequencesNotSupported);

        public override Task Create_sequence_all_settings()
            => AssertNotSupportedAsync(base.Create_sequence, SqliteStrings.SequencesNotSupported);

        public override Task Alter_sequence_all_settings()
            => AssertNotSupportedAsync(base.Create_sequence, SqliteStrings.SequencesNotSupported);

        public override Task Alter_sequence_increment_by()
            => AssertNotSupportedAsync(base.Create_sequence, SqliteStrings.SequencesNotSupported);

        public override Task Drop_sequence()
            => AssertNotSupportedAsync(base.Create_sequence, SqliteStrings.SequencesNotSupported);

        public override Task Rename_sequence()
            => AssertNotSupportedAsync(base.Create_sequence, SqliteStrings.SequencesNotSupported);

        public override Task Move_sequence()
            => AssertNotSupportedAsync(base.Create_sequence, SqliteStrings.SequencesNotSupported);

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
            protected override string StoreName { get; } = nameof(MigrationsSqliteTest);

            protected override ITestStoreFactory TestStoreFactory
                => SqliteTestStoreFactory.Instance;

            public override TestHelpers TestHelpers
                => SqliteTestHelpers.Instance;

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection)
                    .AddScoped<IDatabaseModelFactory, SqliteDatabaseModelFactory>();
        }
    }
}
