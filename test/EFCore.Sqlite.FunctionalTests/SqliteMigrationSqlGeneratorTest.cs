// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class SqliteMigrationSqlGeneratorTest : MigrationSqlGeneratorTestBase
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
                            ColumnType = "INT"
                        }
                    }
                }, new AddForeignKeyOperation
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

        [ConditionalTheory]
        [InlineData(true, null)]
        [InlineData(false, "PK_Id")]
        public void CreateTableOperation_with_annotations(bool autoincrement, string pkName)
        {
            var addIdColumn = new AddColumnOperation
            {
                Name = "Id",
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
                            ClrType = typeof(int),
                            ColumnType = "int",
                            IsNullable = true
                        },
                        new AddColumnOperation
                        {
                            Name = "SSN",
                            ClrType = typeof(string),
                            ColumnType = "char(11)",
                            IsNullable = true
                        }
                    },
                    PrimaryKey = new AddPrimaryKeyOperation
                    {
                        Name = pkName,
                        Columns = new[] { "Id" }
                    },
                    UniqueConstraints =
                    {
                        new AddUniqueConstraintOperation
                        {
                            Columns = new[] { "SSN" }
                        }
                    },
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

        public override void AddColumnOperation_with_defaultValue()
        {
            base.AddColumnOperation_with_defaultValue();

            AssertSql(@"ALTER TABLE ""People"" ADD ""Name"" varchar(30) NOT NULL DEFAULT 'John Doe';
");
        }

        public override void AddColumnOperation_without_column_type()
        {
            base.AddColumnOperation_without_column_type();

            AssertSql(@"ALTER TABLE ""People"" ADD ""Alias"" TEXT NOT NULL;
");
        }

        public override void AddColumnOperation_with_defaultValueSql()
        {
            // Override base test because CURRENT_TIMESTAMP is not valid for AddColumn
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "Age",
                    ClrType = typeof(int),
                    ColumnType = "int",
                    IsNullable = true,
                    DefaultValueSql = "10"
                });

            AssertSql(@"ALTER TABLE ""People"" ADD ""Age"" int NULL DEFAULT (10);
");
        }

        public override void AddColumnOperation_with_maxLength()
        {
            base.AddColumnOperation_with_maxLength();

            // See issue #3698
            AssertSql(@"ALTER TABLE ""Person"" ADD ""Name"" TEXT NULL;
");
        }

        public override void AddColumnOperation_with_maxLength_overridden()
        {
            base.AddColumnOperation_with_maxLength_overridden();

            // See issue #3698
            AssertSql(@"ALTER TABLE ""Person"" ADD ""Name"" TEXT NULL;
");
        }

        public override void AddColumnOperation_with_maxLength_on_derived()
        {
            base.AddColumnOperation_with_maxLength_on_derived();

            // See issue #3698
            AssertSql(@"ALTER TABLE ""Person"" ADD ""Name"" TEXT NULL;
");
        }

        public override void AddColumnOperation_with_shared_column()
        {
            base.AddColumnOperation_with_shared_column();

            AssertSql(@"ALTER TABLE ""Base"" ADD ""Foo"" TEXT NULL;
");
        }

        [ConditionalFact]
        public void AddColumnOperation_with_computed_column_SQL()
        {
            var ex = Assert.Throws<NotSupportedException>(
                () => Generate(
                    new AddColumnOperation
                    {
                        Table = "People",
                        Name = "Birthday",
                        ClrType = typeof(DateTime),
                        ColumnType = "TEXT",
                        IsNullable = true,
                        ComputedColumnSql = "CURRENT_TIMESTAMP"
                    }));
            Assert.Equal(SqliteStrings.ComputedColumnsNotSupported, ex.Message);
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

        public override void AddForeignKeyOperation_with_name()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.AddForeignKeyOperation_with_name());
            Assert.Equal(SqliteStrings.InvalidMigrationOperation(nameof(AddForeignKeyOperation)), ex.Message);
        }

        public override void AddForeignKeyOperation_without_name()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.AddForeignKeyOperation_without_name());
            Assert.Equal(SqliteStrings.InvalidMigrationOperation(nameof(AddForeignKeyOperation)), ex.Message);
        }

        public override void AddForeignKeyOperation_without_principal_columns()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.AddForeignKeyOperation_without_name());
            Assert.Equal(SqliteStrings.InvalidMigrationOperation(nameof(AddForeignKeyOperation)), ex.Message);
        }

        public override void AddPrimaryKeyOperation_with_name()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.AddPrimaryKeyOperation_with_name());
            Assert.Equal(SqliteStrings.InvalidMigrationOperation(nameof(AddPrimaryKeyOperation)), ex.Message);
        }

        public override void AddPrimaryKeyOperation_without_name()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.AddPrimaryKeyOperation_without_name());
            Assert.Equal(SqliteStrings.InvalidMigrationOperation(nameof(AddPrimaryKeyOperation)), ex.Message);
        }

        public override void AddUniqueConstraintOperation_with_name()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.AddUniqueConstraintOperation_with_name());
            Assert.Equal(SqliteStrings.InvalidMigrationOperation(nameof(AddUniqueConstraintOperation)), ex.Message);
        }

        public override void AddUniqueConstraintOperation_without_name()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.AddUniqueConstraintOperation_without_name());
            Assert.Equal(SqliteStrings.InvalidMigrationOperation(nameof(AddUniqueConstraintOperation)), ex.Message);
        }

        public override void CreateCheckConstraintOperation_with_name()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.CreateCheckConstraintOperation_with_name());
            Assert.Equal(SqliteStrings.InvalidMigrationOperation(nameof(CreateCheckConstraintOperation)), ex.Message);
        }

        public override void AlterColumnOperation()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.AlterColumnOperation());
            Assert.Equal(SqliteStrings.InvalidMigrationOperation(nameof(AlterColumnOperation)), ex.Message);
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

        public override void AlterSequenceOperation_with_minValue_and_maxValue()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.AlterSequenceOperation_with_minValue_and_maxValue());
            Assert.Equal(SqliteStrings.SequencesNotSupported, ex.Message);
        }

        public override void AlterSequenceOperation_without_minValue_and_maxValue()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.AlterSequenceOperation_without_minValue_and_maxValue());
            Assert.Equal(SqliteStrings.SequencesNotSupported, ex.Message);
        }

        [ConditionalFact]
        public virtual void RenameColumnOperation()
        {
            Generate(
                new RenameColumnOperation
                {
                    Table = "People",
                    Name = "Name",
                    NewName = "FullName"
                });

            AssertSql(@"ALTER TABLE ""People"" RENAME COLUMN ""Name"" TO ""FullName"";
");
        }

        [ConditionalFact]
        public virtual void RenameIndexOperation()
        {
            Generate(
                modelBuilder => modelBuilder.Entity(
                    "Person",
                    x =>
                    {
                        x.Property<string>("FullName");
                        x.HasKey("FullName");
                        x.HasIndex("FullName").IsUnique().HasFilter(@"""Id"" > 2");
                    }),
                new RenameIndexOperation
                {
                    Table = "Person",
                    Name = "IX_Person_Name",
                    NewName = "IX_Person_FullName"
                });

            AssertSql(
                @"DROP INDEX ""IX_Person_Name"";
CREATE UNIQUE INDEX ""IX_Person_FullName"" ON ""Person"" (""FullName"") WHERE ""Id"" > 2;
");
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

        public override void RenameTableOperation_legacy()
        {
            base.RenameTableOperation_legacy();

            AssertSql(@"ALTER TABLE ""People"" RENAME TO ""Person"";
");
        }

        public override void RenameTableOperation()
        {
            base.RenameTableOperation();

            AssertSql(@"ALTER TABLE ""People"" RENAME TO ""Person"";
");
        }

        public override void CreateSequenceOperation_with_minValue_and_maxValue()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.CreateSequenceOperation_with_minValue_and_maxValue());
            Assert.Equal(SqliteStrings.SequencesNotSupported, ex.Message);
        }

        public override void CreateSequenceOperation_with_minValue_and_maxValue_not_long()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.CreateSequenceOperation_with_minValue_and_maxValue_not_long());
            Assert.Equal(SqliteStrings.SequencesNotSupported, ex.Message);
        }

        public override void CreateSequenceOperation_without_minValue_and_maxValue()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.CreateSequenceOperation_without_minValue_and_maxValue());
            Assert.Equal(SqliteStrings.SequencesNotSupported, ex.Message);
        }

        public override void DropColumnOperation()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.DropColumnOperation());
            Assert.Equal(SqliteStrings.InvalidMigrationOperation(nameof(DropColumnOperation)), ex.Message);
        }

        public override void DropForeignKeyOperation()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.DropForeignKeyOperation());
            Assert.Equal(SqliteStrings.InvalidMigrationOperation(nameof(DropForeignKeyOperation)), ex.Message);
        }

        public override void DropIndexOperation()
        {
            base.DropIndexOperation();

            AssertSql(@"DROP INDEX ""IX_People_Name"";
");
        }

        public override void DropPrimaryKeyOperation()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.DropPrimaryKeyOperation());
            Assert.Equal(SqliteStrings.InvalidMigrationOperation(nameof(DropPrimaryKeyOperation)), ex.Message);
        }

        public override void DropSequenceOperation()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.DropSequenceOperation());
            Assert.Equal(SqliteStrings.SequencesNotSupported, ex.Message);
        }

        public override void DropUniqueConstraintOperation()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.DropUniqueConstraintOperation());
            Assert.Equal(SqliteStrings.InvalidMigrationOperation(nameof(DropUniqueConstraintOperation)), ex.Message);
        }

        public override void DropCheckConstraintOperation()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.DropCheckConstraintOperation());
            Assert.Equal(SqliteStrings.InvalidMigrationOperation(nameof(DropCheckConstraintOperation)), ex.Message);
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
                    PrimaryKey = new AddPrimaryKeyOperation
                    {
                        Columns = new[] { "Id" }
                    }
                });

            AssertSql(
                @"CREATE TABLE ""People"" (
    ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
);
");
        }

        [ConditionalFact]
        public virtual void CreateTableOperation_has_comment()
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
                            Comment = "The ID"
                        },
                        new AddColumnOperation
                        {
                            Name = "UncommentedColumn1",
                            Table = "People",
                            ClrType = typeof(string),
                            IsNullable = false
                        },
                        new AddColumnOperation
                        {
                            Name = "UncommentedColumn2",
                            Table = "People",
                            ClrType = typeof(string),
                            IsNullable = false
                        },
                        new AddColumnOperation
                        {
                            Name = "Name",
                            Table = "People",
                            ClrType = typeof(string),
                            IsNullable = false,
                            Comment = "The Name"
                        }
                    }
                });

            AssertSql(
                @"CREATE TABLE ""People"" (
    -- The ID
    ""Id"" INTEGER NOT NULL,

    ""UncommentedColumn1"" TEXT NOT NULL,

    ""UncommentedColumn2"" TEXT NOT NULL,

    -- The Name
    ""Name"" TEXT NOT NULL
);
");
        }

        [ConditionalFact]
        public virtual void CreateTableOperation_has_multi_line_comment()
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
                            Comment = @"This is a multi-line
comment.
More information can
be found in the docs."

                        },
                    }
                });

            AssertSql(
                @"CREATE TABLE ""People"" (
    -- This is a multi-line
    -- comment.
    -- More information can
    -- be found in the docs.
    ""Id"" INTEGER NOT NULL
);
");
        }

        [ConditionalFact]
        public virtual void CreateTableOperation_has_multi_line_table_comment()
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "People",
                    Comment = @"Table level comment
that continues onto another line",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Id",
                            Table = "People",
                            ClrType = typeof(int),
                            IsNullable = false,
                            Comment = "My Comment"
                        },
                    }
                });

            AssertSql(
                @"CREATE TABLE ""People"" (
    -- Table level comment
    -- that continues onto another line

    -- My Comment
    ""Id"" INTEGER NOT NULL
);
");
        }

        public SqliteMigrationSqlGeneratorTest()
            : base(SqliteTestHelpers.Instance)
        {
        }
    }
}
