// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Migrations
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

        public SqliteMigrationSqlGeneratorTest()
            : base(SqliteTestHelpers.Instance)
        {
        }
    }
}
