// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class SqliteMigrationsSqlGeneratorTest : MigrationsSqlGeneratorTestBase
    {
        [ConditionalFact]
        public virtual void It_lifts_foreign_key_additions()
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "Pie",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            ClrType = typeof(int),
                            Name = "FlavorId",
                            ColumnType = "INT",
                            Table = "Pie"
                        }
                    }
                },
                new AddForeignKeyOperation
                {
                    Table = "Pie",
                    PrincipalTable = "Flavor",
                    Columns = new[] { "FlavorId" },
                    PrincipalColumns = new[] { "Id" }
                });

            AssertSql(
                @"CREATE TABLE ""Pie"" (
    ""FlavorId"" INT NOT NULL,
    FOREIGN KEY (""FlavorId"") REFERENCES ""Flavor"" (""Id"")
);
");
        }

        [ConditionalFact]
        public virtual void DefaultValue_formats_literal_correctly()
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "History",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Event",
                            Table = "History",
                            ClrType = typeof(string),
                            ColumnType = "TEXT",
                            DefaultValue = new DateTime(2015, 4, 12, 17, 5, 0)
                        }
                    }
                });

            AssertSql(
                @"CREATE TABLE ""History"" (
    ""Event"" TEXT NOT NULL DEFAULT '2015-04-12 17:05:00'
);
");
        }

        public override void DefaultValue_with_line_breaks(bool isUnicode)
        {
            base.DefaultValue_with_line_breaks(isUnicode);

            AssertSql(
                @"CREATE TABLE ""TestLineBreaks"" (
    ""TestDefaultValue"" TEXT NOT NULL DEFAULT (CHAR(13) || CHAR(10) || 'Various Line' || CHAR(13) || 'Breaks' || CHAR(10))
);
");
        }

        [ConditionalTheory]
        [InlineData(true, null)]
        [InlineData(false, "PK_Id")]
        public void CreateTableOperation_with_annotations(bool autoincrement, string pkName)
        {
            var addIdColumn = new AddColumnOperation
            {
                Name = "Id",
                Table = "People",
                ClrType = typeof(long),
                ColumnType = "INTEGER",
                IsNullable = false
            };
            if (autoincrement)
            {
                addIdColumn.AddAnnotation(SqliteAnnotationNames.Autoincrement, true);
            }

            Generate(
                new CreateTableOperation
                {
                    Name = "People",
                    Columns =
                    {
                        addIdColumn,
                        new AddColumnOperation
                        {
                            Name = "EmployerId",
                            Table = "People",
                            ClrType = typeof(int),
                            ColumnType = "int",
                            IsNullable = true
                        },
                        new AddColumnOperation
                        {
                            Name = "SSN",
                            Table = "People",
                            ClrType = typeof(string),
                            ColumnType = "char(11)",
                            IsNullable = true
                        }
                    },
                    PrimaryKey = new AddPrimaryKeyOperation { Name = pkName, Columns = new[] { "Id" } },
                    UniqueConstraints = { new AddUniqueConstraintOperation { Columns = new[] { "SSN" } } },
                    ForeignKeys =
                    {
                        new AddForeignKeyOperation
                        {
                            Columns = new[] { "EmployerId" },
                            PrincipalTable = "Companies",
                            PrincipalColumns = new[] { "Id" }
                        }
                    }
                });

            AssertSql(
                $@"CREATE TABLE ""People"" (
    ""Id"" INTEGER NOT NULL{(pkName != null ? $@" CONSTRAINT ""{pkName}""" : "")} PRIMARY KEY{(autoincrement ? " AUTOINCREMENT," : ",")}
    ""EmployerId"" int NULL,
    ""SSN"" char(11) NULL,
    UNIQUE (""SSN""),
    FOREIGN KEY (""EmployerId"") REFERENCES ""Companies"" (""Id"")
);
");
        }

        [ConditionalFact]
        public void CreateSchemaOperation_is_ignored()
        {
            Generate(new EnsureSchemaOperation());

            Assert.Empty(Sql);
        }

        public override void AddColumnOperation_without_column_type()
        {
            base.AddColumnOperation_without_column_type();

            AssertSql(
                @"ALTER TABLE ""People"" ADD ""Alias"" TEXT NOT NULL;
");
        }

        public override void AddColumnOperation_with_maxLength_overridden()
        {
            base.AddColumnOperation_with_maxLength_overridden();

            // See issue #3698
            AssertSql(
                @"ALTER TABLE ""Person"" ADD ""Name"" TEXT NULL;
");
        }

        public override void AddColumnOperation_with_unicode_no_model()
        {
            base.AddColumnOperation_with_unicode_no_model();

            AssertSql(
                @"ALTER TABLE ""Person"" ADD ""Name"" TEXT NULL;
");
        }

        public override void AddColumnOperation_with_fixed_length_no_model()
        {
            base.AddColumnOperation_with_fixed_length_no_model();

            AssertSql(
                @"ALTER TABLE ""Person"" ADD ""Name"" TEXT NULL;
");
        }

        public override void AddColumnOperation_with_maxLength_no_model()
        {
            base.AddColumnOperation_with_maxLength_no_model();

            AssertSql(
                @"ALTER TABLE ""Person"" ADD ""Name"" TEXT NULL;
");
        }

        public override void AddColumnOperation_with_precision_and_scale_overridden()
        {
            base.AddColumnOperation_with_precision_and_scale_overridden();

            AssertSql(
                @"ALTER TABLE ""Person"" ADD ""Pi"" TEXT NOT NULL;
");
        }

        public override void AddColumnOperation_with_precision_and_scale_no_model()
        {
            base.AddColumnOperation_with_precision_and_scale_no_model();

            AssertSql(
                @"ALTER TABLE ""Person"" ADD ""Pi"" TEXT NOT NULL;
");
        }

        public override void AddColumnOperation_with_unicode_overridden()
        {
            base.AddColumnOperation_with_unicode_overridden();

            AssertSql(
                @"ALTER TABLE ""Person"" ADD ""Name"" TEXT NULL;
");
        }

        [ConditionalFact]
        public void DropSchemaOperation_is_ignored()
        {
            Generate(new DropSchemaOperation());

            Assert.Empty(Sql);
        }

        [ConditionalFact]
        public void RestartSequenceOperation_not_supported()
        {
            var ex = Assert.Throws<NotSupportedException>(() => Generate(new RestartSequenceOperation()));
            Assert.Equal(SqliteStrings.SequencesNotSupported, ex.Message);
        }

        public override void AddForeignKeyOperation_without_principal_columns()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.AddForeignKeyOperation_without_principal_columns());
            Assert.Equal(SqliteStrings.InvalidMigrationOperation(nameof(AddForeignKeyOperation)), ex.Message);
        }

        public override void AlterColumnOperation_without_column_type()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.AlterColumnOperation_without_column_type());
            Assert.Equal(SqliteStrings.InvalidMigrationOperation(nameof(AlterColumnOperation)), ex.Message);
        }

        [ConditionalFact]
        public void AlterColumnOperation_computed()
        {
            var ex = Assert.Throws<NotSupportedException>(
                () => Generate(
                    new AlterColumnOperation
                    {
                        Table = "People",
                        Name = "FullName",
                        ClrType = typeof(string),
                        ComputedColumnSql = "FirstName || ' ' || LastName"
                    }));
            Assert.Equal(SqliteStrings.InvalidMigrationOperation(nameof(AlterColumnOperation)), ex.Message);
        }

        [ConditionalFact]
        public virtual void RenameIndexOperations_throws_when_no_model()
        {
            var migrationBuilder = new MigrationBuilder("Sqlite");

            migrationBuilder.RenameIndex(
                table: "Person",
                name: "IX_Person_Name",
                newName: "IX_Person_FullName");

            var ex = Assert.Throws<NotSupportedException>(
                () => Generate(migrationBuilder.Operations.ToArray()));

            Assert.Equal(SqliteStrings.InvalidMigrationOperation("RenameIndexOperation"), ex.Message);
        }

        public override void RenameTableOperation()
        {
            base.RenameTableOperation();

            AssertSql(
                @"ALTER TABLE ""People"" RENAME TO ""Person"";
");
        }

        public override void RenameTableOperation_legacy()
        {
            base.RenameTableOperation_legacy();

            AssertSql(
                @"ALTER TABLE ""People"" RENAME TO ""Person"";
");
        }

        [ConditionalFact]
        public virtual void CreateTableOperation_old_autoincrement_annotation()
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "People",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Id",
                            Table = "People",
                            ClrType = typeof(int),
                            IsNullable = false,
                            ["Autoincrement"] = true
                        }
                    },
                    PrimaryKey = new AddPrimaryKeyOperation { Columns = new[] { "Id" } }
                });

            AssertSql(
                @"CREATE TABLE ""People"" (
    ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
);
");
        }

        public override void SqlOperation()
        {
            base.SqlOperation();

            AssertSql(
                @"-- I <3 DDL
");
        }

        public override void InsertDataOperation_all_args_spatial()
        {
            base.InsertDataOperation_all_args_spatial();

            AssertSql(
                @"INSERT INTO ""People"" (""Id"", ""Full Name"", ""Geometry"")
VALUES (0, NULL, NULL);
INSERT INTO ""People"" (""Id"", ""Full Name"", ""Geometry"")
VALUES (1, 'Daenerys Targaryen', NULL);
INSERT INTO ""People"" (""Id"", ""Full Name"", ""Geometry"")
VALUES (2, 'John Snow', NULL);
INSERT INTO ""People"" (""Id"", ""Full Name"", ""Geometry"")
VALUES (3, 'Arya Stark', NULL);
INSERT INTO ""People"" (""Id"", ""Full Name"", ""Geometry"")
VALUES (4, 'Harry Strickland', NULL);
INSERT INTO ""People"" (""Id"", ""Full Name"", ""Geometry"")
VALUES (5, 'The Imp', NULL);
INSERT INTO ""People"" (""Id"", ""Full Name"", ""Geometry"")
VALUES (6, 'The Kingslayer', NULL);
INSERT INTO ""People"" (""Id"", ""Full Name"", ""Geometry"")
VALUES (7, 'Aemon Targaryen', GeomFromText('GEOMETRYCOLLECTION Z(LINESTRING Z(1.1 2.2 NaN, 2.2 2.2 NaN, 2.2 1.1 NaN, 7.1 7.2 NaN), LINESTRING Z(7.1 7.2 NaN, 20.2 20.2 NaN, 20.2 1.1 NaN, 70.1 70.2 NaN), MULTIPOINT Z((1.1 2.2 NaN), (2.2 2.2 NaN), (2.2 1.1 NaN)), POLYGON Z((1.1 2.2 NaN, 2.2 2.2 NaN, 2.2 1.1 NaN, 1.1 2.2 NaN)), POLYGON Z((10.1 20.2 NaN, 20.2 20.2 NaN, 20.2 10.1 NaN, 10.1 20.2 NaN)), POINT Z(1.1 2.2 3.3), MULTILINESTRING Z((1.1 2.2 NaN, 2.2 2.2 NaN, 2.2 1.1 NaN, 7.1 7.2 NaN), (7.1 7.2 NaN, 20.2 20.2 NaN, 20.2 1.1 NaN, 70.1 70.2 NaN)), MULTIPOLYGON Z(((10.1 20.2 NaN, 20.2 20.2 NaN, 20.2 10.1 NaN, 10.1 20.2 NaN)), ((1.1 2.2 NaN, 2.2 2.2 NaN, 2.2 1.1 NaN, 1.1 2.2 NaN))))', 4326));
");
        }

        protected override string GetGeometryCollectionStoreType()
            => "GEOMETRYCOLLECTION";

        public override void InsertDataOperation_required_args()
        {
            base.InsertDataOperation_required_args();

            AssertSql(
                @"INSERT INTO ""People"" (""First Name"")
VALUES ('John');
");
        }

        public override void InsertDataOperation_required_args_composite()
        {
            base.InsertDataOperation_required_args_composite();

            AssertSql(
                @"INSERT INTO ""People"" (""First Name"", ""Last Name"")
VALUES ('John', 'Snow');
");
        }

        public override void InsertDataOperation_required_args_multiple_rows()
        {
            base.InsertDataOperation_required_args_multiple_rows();

            AssertSql(
                @"INSERT INTO ""People"" (""First Name"")
VALUES ('John');
INSERT INTO ""People"" (""First Name"")
VALUES ('Daenerys');
");
        }

        public override void InsertDataOperation_throws_for_unsupported_column_types()
        {
            // All column types are supported by Sqlite
        }

        public override void DeleteDataOperation_all_args()
        {
            base.DeleteDataOperation_all_args();

            AssertSql(
                @"DELETE FROM ""People""
WHERE ""First Name"" = 'Hodor';
SELECT changes();

DELETE FROM ""People""
WHERE ""First Name"" = 'Daenerys';
SELECT changes();

DELETE FROM ""People""
WHERE ""First Name"" = 'John';
SELECT changes();

DELETE FROM ""People""
WHERE ""First Name"" = 'Arya';
SELECT changes();

DELETE FROM ""People""
WHERE ""First Name"" = 'Harry';
SELECT changes();

");
        }

        public override void DeleteDataOperation_all_args_composite()
        {
            base.DeleteDataOperation_all_args_composite();

            AssertSql(
                @"DELETE FROM ""People""
WHERE ""First Name"" = 'Hodor' AND ""Last Name"" IS NULL;
SELECT changes();

DELETE FROM ""People""
WHERE ""First Name"" = 'Daenerys' AND ""Last Name"" = 'Targaryen';
SELECT changes();

DELETE FROM ""People""
WHERE ""First Name"" = 'John' AND ""Last Name"" = 'Snow';
SELECT changes();

DELETE FROM ""People""
WHERE ""First Name"" = 'Arya' AND ""Last Name"" = 'Stark';
SELECT changes();

DELETE FROM ""People""
WHERE ""First Name"" = 'Harry' AND ""Last Name"" = 'Strickland';
SELECT changes();

");
        }

        public override void DeleteDataOperation_required_args()
        {
            base.DeleteDataOperation_required_args();

            AssertSql(
                @"DELETE FROM ""People""
WHERE ""Last Name"" = 'Snow';
SELECT changes();

");
        }

        public override void DeleteDataOperation_required_args_composite()
        {
            base.DeleteDataOperation_required_args_composite();

            AssertSql(
                @"DELETE FROM ""People""
WHERE ""First Name"" = 'John' AND ""Last Name"" = 'Snow';
SELECT changes();

");
        }

        public override void UpdateDataOperation_all_args()
        {
            base.UpdateDataOperation_all_args();

            AssertSql(
                @"UPDATE ""People"" SET ""Birthplace"" = 'Winterfell', ""House Allegiance"" = 'Stark', ""Culture"" = 'Northmen'
WHERE ""First Name"" = 'Hodor';
SELECT changes();

UPDATE ""People"" SET ""Birthplace"" = 'Dragonstone', ""House Allegiance"" = 'Targaryen', ""Culture"" = 'Valyrian'
WHERE ""First Name"" = 'Daenerys';
SELECT changes();

");
        }

        public override void UpdateDataOperation_all_args_composite()
        {
            base.UpdateDataOperation_all_args_composite();

            AssertSql(
                @"UPDATE ""People"" SET ""House Allegiance"" = 'Stark'
WHERE ""First Name"" = 'Hodor' AND ""Last Name"" IS NULL;
SELECT changes();

UPDATE ""People"" SET ""House Allegiance"" = 'Targaryen'
WHERE ""First Name"" = 'Daenerys' AND ""Last Name"" = 'Targaryen';
SELECT changes();

");
        }

        public override void UpdateDataOperation_all_args_composite_multi()
        {
            base.UpdateDataOperation_all_args_composite_multi();

            AssertSql(
                @"UPDATE ""People"" SET ""Birthplace"" = 'Winterfell', ""House Allegiance"" = 'Stark', ""Culture"" = 'Northmen'
WHERE ""First Name"" = 'Hodor' AND ""Last Name"" IS NULL;
SELECT changes();

UPDATE ""People"" SET ""Birthplace"" = 'Dragonstone', ""House Allegiance"" = 'Targaryen', ""Culture"" = 'Valyrian'
WHERE ""First Name"" = 'Daenerys' AND ""Last Name"" = 'Targaryen';
SELECT changes();

");
        }

        public override void UpdateDataOperation_all_args_multi()
        {
            base.UpdateDataOperation_all_args_multi();

            AssertSql(
                @"UPDATE ""People"" SET ""Birthplace"" = 'Dragonstone', ""House Allegiance"" = 'Targaryen', ""Culture"" = 'Valyrian'
WHERE ""First Name"" = 'Daenerys';
SELECT changes();

");
        }

        public override void UpdateDataOperation_required_args()
        {
            base.UpdateDataOperation_required_args();

            AssertSql(
                @"UPDATE ""People"" SET ""House Allegiance"" = 'Targaryen'
WHERE ""First Name"" = 'Daenerys';
SELECT changes();

");
        }

        public override void UpdateDataOperation_required_args_composite()
        {
            base.UpdateDataOperation_required_args_composite();

            AssertSql(
                @"UPDATE ""People"" SET ""House Allegiance"" = 'Targaryen'
WHERE ""First Name"" = 'Daenerys' AND ""Last Name"" = 'Targaryen';
SELECT changes();

");
        }

        public override void UpdateDataOperation_required_args_composite_multi()
        {
            base.UpdateDataOperation_required_args_composite_multi();

            AssertSql(
                @"UPDATE ""People"" SET ""Birthplace"" = 'Dragonstone', ""House Allegiance"" = 'Targaryen', ""Culture"" = 'Valyrian'
WHERE ""First Name"" = 'Daenerys' AND ""Last Name"" = 'Targaryen';
SELECT changes();

");
        }

        public override void UpdateDataOperation_required_args_multi()
        {
            base.UpdateDataOperation_required_args_multi();

            AssertSql(
                @"UPDATE ""People"" SET ""Birthplace"" = 'Dragonstone', ""House Allegiance"" = 'Targaryen', ""Culture"" = 'Valyrian'
WHERE ""First Name"" = 'Daenerys';
SELECT changes();

");
        }

        public override void UpdateDataOperation_required_args_multiple_rows()
        {
            base.UpdateDataOperation_required_args_multiple_rows();

            AssertSql(
                @"UPDATE ""People"" SET ""House Allegiance"" = 'Stark'
WHERE ""First Name"" = 'Hodor';
SELECT changes();

UPDATE ""People"" SET ""House Allegiance"" = 'Targaryen'
WHERE ""First Name"" = 'Daenerys';
SELECT changes();

");
        }

        [ConditionalFact]
        public virtual void AddPrimaryKey_throws_when_no_model()
        {
            var ex = Assert.Throws<NotSupportedException>(
                () => Generate(
                    new AddPrimaryKeyOperation
                    {
                        Table = "Blogs",
                        Name = "PK_Blogs",
                        Columns = new[] { "Id" }
                    }));

            Assert.Equal(SqliteStrings.InvalidMigrationOperation("AddPrimaryKeyOperation"), ex.Message);
        }

        [ConditionalFact]
        public virtual void AddUniqueConstraint_throws_when_no_model()
        {
            var ex = Assert.Throws<NotSupportedException>(
                () => Generate(
                    new AddUniqueConstraintOperation
                    {
                        Table = "Blogs",
                        Name = "AK_Blogs_Uri",
                        Columns = new[] { "Uri" }
                    }));

            Assert.Equal(SqliteStrings.InvalidMigrationOperation("AddUniqueConstraintOperation"), ex.Message);
        }

        [ConditionalFact]
        public virtual void AddCheckConstraint_throws_when_no_model()
        {
            var ex = Assert.Throws<NotSupportedException>(
                () => Generate(
                    new AddCheckConstraintOperation
                    {
                        Table = "Blogs",
                        Name = "CK_Blogs_Rating",
                        Sql = "Rating BETWEEN 1 AND 5"
                    }));

            Assert.Equal(SqliteStrings.InvalidMigrationOperation("AddCheckConstraintOperation"), ex.Message);
        }

        [ConditionalFact]
        public virtual void AlterTable_mostly_works_when_no_model()
        {
            Generate(
                new AlterTableOperation { Name = "Blogs", Comment = "The Blogs table" });

            Assert.Empty(Sql);
        }

        [ConditionalFact]
        public virtual void DropForeignKey_throws_when_no_model()
        {
            var ex = Assert.Throws<NotSupportedException>(
                () => Generate(
                    new DropForeignKeyOperation { Table = "Posts", Name = "FK_Posts_BlogId" }));

            Assert.Equal(SqliteStrings.InvalidMigrationOperation("DropForeignKeyOperation"), ex.Message);
        }

        [ConditionalFact]
        public virtual void DropPrimaryKey_throws_when_no_model()
        {
            var ex = Assert.Throws<NotSupportedException>(
                () => Generate(
                    new DropPrimaryKeyOperation { Table = "Blogs", Name = "PK_Blogs" }));

            Assert.Equal(SqliteStrings.InvalidMigrationOperation("DropPrimaryKeyOperation"), ex.Message);
        }

        [ConditionalFact]
        public virtual void DropUniqueConstraint_throws_when_no_model()
        {
            var ex = Assert.Throws<NotSupportedException>(
                () => Generate(
                    new DropUniqueConstraintOperation { Table = "Blogs", Name = "AK_Blogs_Uri" }));

            Assert.Equal(SqliteStrings.InvalidMigrationOperation("DropUniqueConstraintOperation"), ex.Message);
        }

        [ConditionalFact]
        public virtual void DropColumn_throws_when_no_model()
        {
            var ex = Assert.Throws<NotSupportedException>(
                () => Generate(
                    new DropColumnOperation { Table = "Posts", Name = "Rating" }));

            Assert.Equal(SqliteStrings.InvalidMigrationOperation("DropColumnOperation"), ex.Message);
        }

        [ConditionalFact]
        public virtual void AddColumnOperation_with_comment_mostly_works_when_no_model()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "Blogs",
                    ClrType = typeof(string),
                    Name = "Summary",
                    Comment = "A short description"
                });

            AssertSql(
                @"ALTER TABLE ""Blogs"" ADD ""Summary"" TEXT NOT NULL;
");
        }

        [ConditionalFact]
        public virtual void DropColumn_defers_subsequent_RenameColumn()
        {
            Generate(
                modelBuilder => modelBuilder.Entity("Blog").Property<string>("Name"),
                migrationBuilder =>
                {
                    migrationBuilder.DropColumn(
                        name: "Name",
                        table: "Blog");
                    migrationBuilder.RenameColumn(
                        name: "Title",
                        table: "Blog",
                        newName: "Name");
                });

            AssertSql(
                @"CREATE TABLE ""ef_temp_Blog"" (
    ""Name"" TEXT NULL
);
GO

INSERT INTO ""ef_temp_Blog"" (""Name"")
SELECT ""Title""
FROM Blog;
GO

PRAGMA foreign_keys = 0;
GO

DROP TABLE ""Blog"";
GO

ALTER TABLE ""ef_temp_Blog"" RENAME TO ""Blog"";
GO

PRAGMA foreign_keys = 1;
");
        }

        [ConditionalFact]
        public virtual void Deferred_RenameColumn_defers_subsequent_AddColumn()
        {
            Generate(
                modelBuilder => modelBuilder.Entity(
                    "Blog", x =>
                    {
                        x.Property<string>("Title");
                        x.Property<string>("Name");
                    }),
                migrationBuilder =>
                {
                    migrationBuilder.DropColumn(
                        name: "Name",
                        table: "Blog");
                    migrationBuilder.RenameColumn(
                        name: "Title",
                        table: "Blog",
                        newName: "Name");
                    migrationBuilder.AddColumn<string>(
                        name: "Title",
                        table: "Blog",
                        nullable: true);
                });

            AssertSql(
                @"CREATE TABLE ""ef_temp_Blog"" (
    ""Name"" TEXT NULL,
    ""Title"" TEXT NULL
);
GO

INSERT INTO ""ef_temp_Blog"" (""Name"")
SELECT ""Title""
FROM Blog;
GO

PRAGMA foreign_keys = 0;
GO

DROP TABLE ""Blog"";
GO

ALTER TABLE ""ef_temp_Blog"" RENAME TO ""Blog"";
GO

PRAGMA foreign_keys = 1;
");
        }

        [ConditionalFact]
        public virtual void Deferred_RenameColumn_defers_subsequent_CreateIndex_unique()
        {
            Generate(
                modelBuilder => modelBuilder.Entity(
                    "Blog", x =>
                    {
                        x.Property<string>("Name");
                        x.HasIndex("Name").IsUnique();
                    }),
                migrationBuilder =>
                {
                    migrationBuilder.DropColumn(
                        name: "Name",
                        table: "Blog");
                    migrationBuilder.RenameColumn(
                        name: "Title",
                        table: "Blog",
                        newName: "Name");
                    migrationBuilder.CreateIndex(
                        name: "IX_Blog_Name",
                        table: "Blog",
                        column: "Name",
                        unique: true);
                });

            AssertSql(
                @"CREATE TABLE ""ef_temp_Blog"" (
    ""Name"" TEXT NULL
);
GO

CREATE UNIQUE INDEX ""IX_Blog_Name"" ON ""ef_temp_Blog"" (""Name"");
GO

INSERT INTO ""ef_temp_Blog"" (""Name"")
SELECT ""Title""
FROM Blog;
GO

PRAGMA foreign_keys = 0;
GO

DROP TABLE ""Blog"";
GO

ALTER TABLE ""ef_temp_Blog"" RENAME TO ""Blog"";
GO

PRAGMA foreign_keys = 1;
");
        }

        [ConditionalFact]
        public virtual void DropColumn_defers_subsequent_AddColumn_required()
        {
            Generate(
                modelBuilder => modelBuilder.Entity(
                    "Blog",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("Name").IsRequired();
                    }),
                migrationBuilder =>
                {
                    migrationBuilder.DropColumn(
                        name: "Name",
                        table: "Blog");
                    migrationBuilder.AddColumn<string>(
                        name: "Name",
                        table: "Blog",
                        nullable: false,
                        defaultValue: "Overridden");
                });

            AssertSql(
                @"CREATE TABLE ""ef_temp_Blog"" (
    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Blog"" PRIMARY KEY AUTOINCREMENT,
    ""Name"" TEXT NOT NULL DEFAULT 'Overridden'
);
GO

INSERT INTO ""ef_temp_Blog"" (""Id"")
SELECT ""Id""
FROM Blog;
GO

PRAGMA foreign_keys = 0;
GO

DROP TABLE ""Blog"";
GO

ALTER TABLE ""ef_temp_Blog"" RENAME TO ""Blog"";
GO

PRAGMA foreign_keys = 1;
");
        }

        [ConditionalFact]
        public virtual void Deferred_AddColumn_defers_subsequent_CreateIndex()
        {
            Generate(
                modelBuilder => modelBuilder.Entity(
                    "Blog",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("Name");
                        x.HasIndex("Name");
                    }),
                migrationBuilder =>
                {
                    migrationBuilder.DropColumn(
                        name: "Name",
                        table: "Blog");
                    migrationBuilder.AddColumn<string>(
                        name: "Name",
                        table: "Blog");
                    migrationBuilder.CreateIndex(
                        name: "IX_Blog_Name",
                        table: "Blog",
                        column: "Name");
                });

            AssertSql(
                @"CREATE TABLE ""ef_temp_Blog"" (
    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Blog"" PRIMARY KEY AUTOINCREMENT,
    ""Name"" TEXT NULL
);
GO

INSERT INTO ""ef_temp_Blog"" (""Id"")
SELECT ""Id""
FROM Blog;
GO

PRAGMA foreign_keys = 0;
GO

DROP TABLE ""Blog"";
GO

ALTER TABLE ""ef_temp_Blog"" RENAME TO ""Blog"";
GO

PRAGMA foreign_keys = 1;
GO

CREATE INDEX ""IX_Blog_Name"" ON ""Blog"" (""Name"");
");
        }

        [ConditionalFact]
        public virtual void RenameTable_preserves_pending_rebuilds()
        {
            Generate(
                modelBuilder => modelBuilder.Entity("Blog").Property<int>("Id"),
                migrationBuilder =>
                {
                    migrationBuilder.DropColumn(
                        name: "Name",
                        table: "Blogs");
                    migrationBuilder.RenameTable(
                        name: "Blogs",
                        newName: "Blog");
                });

            AssertSql(
                @"ALTER TABLE ""Blogs"" RENAME TO ""Blog"";
GO

CREATE TABLE ""ef_temp_Blog"" (
    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Blog"" PRIMARY KEY AUTOINCREMENT
);
GO

INSERT INTO ""ef_temp_Blog"" (""Id"")
SELECT ""Id""
FROM Blog;
GO

PRAGMA foreign_keys = 0;
GO

DROP TABLE ""Blog"";
GO

ALTER TABLE ""ef_temp_Blog"" RENAME TO ""Blog"";
GO

PRAGMA foreign_keys = 1;
");
        }

        public SqliteMigrationsSqlGeneratorTest()
            : base(
                SqliteTestHelpers.Instance,
                new ServiceCollection().AddEntityFrameworkSqliteNetTopologySuite(),
                SqliteTestHelpers.Instance.AddProviderOptions(
                    ((IRelationalDbContextOptionsBuilderInfrastructure)
                        new SqliteDbContextOptionsBuilder(new DbContextOptionsBuilder()).UseNetTopologySuite())
                    .OptionsBuilder).Options)
        {
        }
    }
}
