// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class SqliteMigrationSqlGeneratorTest : MigrationSqlGeneratorTestBase
    {
        [Fact]
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

            Assert.Equal(
                @"CREATE TABLE ""Pie"" (
    ""FlavorId"" INT NOT NULL,
    FOREIGN KEY (""FlavorId"") REFERENCES ""Flavor"" (""Id"")
);
",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        [Fact]
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

            Assert.Equal(
                @"CREATE TABLE ""History"" (
    ""Event"" TEXT NOT NULL DEFAULT '2015-04-12 17:05:00'
);
",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        [Theory]
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

            Assert.Equal(
                "CREATE TABLE \"People\" (" + EOL +
                "    \"Id\" INTEGER NOT NULL" +
                (pkName != null ? $" CONSTRAINT \"{pkName}\"" : "")
                + " PRIMARY KEY" +
                (autoincrement ? " AUTOINCREMENT," : ",") + EOL +
                "    \"EmployerId\" int NULL," + EOL +
                "    \"SSN\" char(11) NULL," + EOL +
                "    UNIQUE (\"SSN\")," + EOL +
                "    FOREIGN KEY (\"EmployerId\") REFERENCES \"Companies\" (\"Id\")" + EOL +
                ");" + EOL,
                Sql);
        }

        [Fact]
        public void CreateSchemaOperation_is_ignored()
        {
            Generate(new EnsureSchemaOperation());

            Assert.Empty(Sql);
        }

        public override void AddColumnOperation_with_defaultValue()
        {
            base.AddColumnOperation_with_defaultValue();

            Assert.Equal(
                @"ALTER TABLE ""People"" ADD ""Name"" varchar(30) NOT NULL DEFAULT 'John Doe';" + EOL,
                Sql);
        }

        public override void AddColumnOperation_without_column_type()
        {
            base.AddColumnOperation_without_column_type();

            Assert.Equal(
                "ALTER TABLE \"People\" ADD \"Alias\" TEXT NOT NULL;" + EOL,
                Sql);
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

            Assert.Equal(
                @"ALTER TABLE ""People"" ADD ""Age"" int NULL DEFAULT (10);" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_maxLength()
        {
            base.AddColumnOperation_with_maxLength();

            // See issue #3698
            Assert.Equal(
                "ALTER TABLE \"Person\" ADD \"Name\" TEXT NULL;" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_maxLength_overridden()
        {
            base.AddColumnOperation_with_maxLength_overridden();

            // See issue #3698
            Assert.Equal(
                "ALTER TABLE \"Person\" ADD \"Name\" TEXT NULL;" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_maxLength_on_derived()
        {
            base.AddColumnOperation_with_maxLength_on_derived();

            // See issue #3698
            Assert.Equal(
                "ALTER TABLE \"Person\" ADD \"Name\" TEXT NULL;" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_shared_column()
        {
            base.AddColumnOperation_with_shared_column();

            Assert.Equal(
                "ALTER TABLE \"Base\" ADD \"Foo\" TEXT NULL;" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_computed_column_SQL()
        {
            base.AddColumnOperation_with_computed_column_SQL();

            Assert.Equal(
                "ALTER TABLE \"People\" ADD \"Birthday\" date NULL;" + EOL,
                Sql);
        }

        [Fact]
        public void DropSchemaOperation_is_ignored()
        {
            Generate(new DropSchemaOperation());

            Assert.Empty(Sql);
        }

        [Fact]
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

        [Fact]
        public virtual void RenameColumnOperation()
        {
            Generate(
                new RenameColumnOperation
                {
                    Table = "People",
                    Name = "Name",
                    NewName = "FullName"
                });

            Assert.Equal(
                @"ALTER TABLE ""People"" RENAME COLUMN ""Name"" TO ""FullName"";" + EOL,
                Sql);
        }

        [Fact]
        public virtual void RenameIndexOperation()
        {
            Generate(
                modelBuilder => modelBuilder.Entity(
                    "Person",
                    x =>
                    {
                        x.Property<string>("FullName");
                        x.HasIndex("FullName").IsUnique().HasFilter(@"""Id"" > 2");
                    }),
                new RenameIndexOperation
                {
                    Table = "Person",
                    Name = "IX_Person_Name",
                    NewName = "IX_Person_FullName"
                });

            Assert.Equal(
                @"DROP INDEX ""IX_Person_Name"";" + EOL +
                @"CREATE UNIQUE INDEX ""IX_Person_FullName"" ON ""Person"" (""FullName"") WHERE ""Id"" > 2;" + EOL,
                Sql);
        }

        [Fact]
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

            Assert.Equal(
                "ALTER TABLE \"People\" RENAME TO \"Person\";" + EOL,
                Sql);
        }

        public override void RenameTableOperation()
        {
            base.RenameTableOperation();

            Assert.Equal(
                "ALTER TABLE \"People\" RENAME TO \"Person\";" + EOL,
                Sql);
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

            Assert.Equal(
                "DROP INDEX \"IX_People_Name\";" + EOL,
                Sql);
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

        [Fact]
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

            Assert.Equal(
                "CREATE TABLE \"People\" (" + EOL +
                "    \"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT" + EOL +
                ");" + EOL,
                Sql);
        }

        public SqliteMigrationSqlGeneratorTest()
            : base(SqliteTestHelpers.Instance)
        {
        }
    }
}
