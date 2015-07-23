// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Migrations.Sql;
using Microsoft.Data.Entity.Sqlite.Metadata;
using Microsoft.Data.Entity.Update;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.Migrations
{
    public class SqliteMigrationSqlGeneratorTest : MigrationSqlGeneratorTestBase
    {
        protected override IMigrationSqlGenerator SqlGenerator => new SqliteMigrationSqlGenerator(new SqliteUpdateSqlGenerator());

        [Fact]
        public virtual void DefaultValue_formats_literal_correctly()
        {
            Generate(new CreateTableOperation
            {
                Name = "History",
                Columns =
                {
                    new AddColumnOperation
                    {
                        Name = "Event",
                        Type = "TEXT",
                        DefaultValue = new DateTime(2015, 4, 12, 17, 5, 0)
                    }
                }
            });

            Assert.Equal(@"CREATE TABLE ""History"" (
    ""Event"" TEXT NOT NULL DEFAULT '2015-04-12 17:05:00'
);" + EOL, Sql);
        }

        [Theory]
        [InlineData(true, null)]
        [InlineData(false, "PK_Id")]
        public void CreateTableOperation_with_annotations(bool autoincrement, string pkName)
        {
            var addIdColumn = new AddColumnOperation
            {
                Name = "Id",
                Type = "INTEGER",
                IsNullable = false,
            };
            if (autoincrement)
            {
                addIdColumn.AddAnnotation(SqliteAnnotationNames.Prefix + SqliteAnnotationNames.Autoincrement, true);
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
                            Type = "int",
                            IsNullable = true
                        },
                         new AddColumnOperation
                        {
                            Name = "SSN",
                            Type = "char(11)",
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
                            ReferencedTable = "Companies"
                        }
                     }
                 });

            Assert.Equal(
                "CREATE TABLE \"People\" (" + EOL +
                "    \"Id\" INTEGER NOT NULL" + 
                (pkName != null ? $" CONSTRAINT \"{pkName}\"":"") 
                + " PRIMARY KEY" +
                (autoincrement ? " AUTOINCREMENT," : ",") + EOL +
                "    \"EmployerId\" int," + EOL +
                "    \"SSN\" char(11)," + EOL +
                "    UNIQUE (\"SSN\")," + EOL +
                "    FOREIGN KEY (\"EmployerId\") REFERENCES \"Companies\"" + EOL +
                ");" + EOL,
                Sql);
        }

        [Fact]
        public void CreateSchemaOperation_not_supported()
        {
            var ex = Assert.Throws<NotSupportedException>(() => Generate(new CreateSchemaOperation()));
            Assert.Equal(Strings.SchemasNotSupported, ex.Message);
        }

        public override void AddColumnOperation_with_defaultValue()
        {
            base.AddColumnOperation_with_defaultValue();

            Assert.Equal(
                @"ALTER TABLE ""People"" ADD ""Name"" varchar(30) NOT NULL DEFAULT 'John Doe';" + EOL,
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
                    Type = "int",
                    IsNullable = true,
                    DefaultValueSql = "10"
                });

            Assert.Equal(
                @"ALTER TABLE ""People"" ADD ""Age"" int DEFAULT (10);" + EOL,
                Sql);
        }

        [Fact]
        public override void AddColumnOperation_with_computed_column_SQL()
        {
            base.AddColumnOperation_with_computed_column_SQL();

            Assert.Equal(
                "ALTER TABLE \"People\" ADD \"Birthday\" date;" + EOL,
                Sql);
        }

        [Fact]
        public void DropSchemaOperation_not_supported()
        {
            var ex = Assert.Throws<NotSupportedException>(() => Generate(new DropSchemaOperation()));
            Assert.Equal(Strings.SchemasNotSupported, ex.Message);
        }

        [Fact]
        public void RestartSequenceOperation_not_supported()
        {
            var ex = Assert.Throws<NotSupportedException>(() => Generate(new RestartSequenceOperation()));
            Assert.Equal(Strings.SequencesNotSupported, ex.Message);
        }

        public override void AddForeignKeyOperation_with_name()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.AddForeignKeyOperation_with_name());
            Assert.Equal(Strings.InvalidMigrationOperation, ex.Message);
        }

        public override void AddForeignKeyOperation_without_name()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.AddForeignKeyOperation_without_name());
            Assert.Equal(Strings.InvalidMigrationOperation, ex.Message);
        }

        public override void AddPrimaryKeyOperation_with_name()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.AddPrimaryKeyOperation_with_name());
            Assert.Equal(Strings.InvalidMigrationOperation, ex.Message);
            ;
        }

        public override void AddPrimaryKeyOperation_without_name()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.AddPrimaryKeyOperation_without_name());
            Assert.Equal(Strings.InvalidMigrationOperation, ex.Message);
            ;
        }

        public override void AddUniqueConstraintOperation_with_name()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.AddUniqueConstraintOperation_with_name());
            Assert.Equal(Strings.InvalidMigrationOperation, ex.Message);
        }

        public override void AddUniqueConstraintOperation_without_name()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.AddUniqueConstraintOperation_without_name());
            Assert.Equal(Strings.InvalidMigrationOperation, ex.Message);
        }

        public override void AlterColumnOperation()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.AlterColumnOperation());
            Assert.Equal(Strings.InvalidMigrationOperation, ex.Message);
        }

        public override void AlterSequenceOperation_with_minValue_and_maxValue()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.AlterSequenceOperation_with_minValue_and_maxValue());
            Assert.Equal(Strings.SequencesNotSupported, ex.Message);
        }

        public override void AlterSequenceOperation_without_minValue_and_maxValue()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.AlterSequenceOperation_without_minValue_and_maxValue());
            Assert.Equal(Strings.SequencesNotSupported, ex.Message);
        }

        public override void RenameTableOperation_within_schema()
        {
            base.RenameTableOperation_within_schema();

            Assert.Equal(
                "ALTER TABLE \"People\" RENAME TO \"Personas\";" + EOL,
                Sql);
        }

        public override void CreateSequenceOperation_with_minValue_and_maxValue()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.CreateSequenceOperation_with_minValue_and_maxValue());
            Assert.Equal(Strings.SequencesNotSupported, ex.Message);
        }

        public override void CreateSequenceOperation_without_minValue_and_maxValue()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.CreateSequenceOperation_without_minValue_and_maxValue());
            Assert.Equal(Strings.SequencesNotSupported, ex.Message);
        }

        public override void DropColumnOperation()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.DropColumnOperation());
            Assert.Equal(Strings.InvalidMigrationOperation, ex.Message);
        }

        public override void DropForeignKeyOperation()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.DropForeignKeyOperation());
            Assert.Equal(Strings.InvalidMigrationOperation, ex.Message);
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
            Assert.Equal(Strings.InvalidMigrationOperation, ex.Message);
        }

        public override void DropSequenceOperation()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.DropSequenceOperation());
            Assert.Equal(Strings.SequencesNotSupported, ex.Message);
        }

        public override void DropUniqueConstraintOperation()
        {
            var ex = Assert.Throws<NotSupportedException>(() => base.DropUniqueConstraintOperation());
            Assert.Equal(Strings.InvalidMigrationOperation, ex.Message);
        }
    }
}
