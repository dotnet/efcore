// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;
using Xunit;

namespace Microsoft.Data.Entity.SQLite.Tests
{
    public class SQLiteMigrationOperationSqlGeneratorTest
    {
        [Fact]
        public void Generate_with_create_database_not_supported()
        {
            var operation = new CreateDatabaseOperation("Bronies");

            Assert.Throws<NotSupportedException>(() => Generate(operation));
        }

        [Fact]
        public void Generate_with_drop_database_not_supported()
        {
            var operation = new DropDatabaseOperation("Bronies");

            Assert.Throws<NotSupportedException>(() => Generate(operation));
        }

        [Fact]
        public void Generate_with_create_sequence_not_supported()
        {
            var operation = new CreateSequenceOperation(new Sequence("EpisodeSequence"));

            Assert.Throws<NotSupportedException>(() => Generate(operation));
        }

        [Fact]
        public void Generate_with_drop_sequence_is_noop()
        {
            var operation = new DropSequenceOperation("EpisodeSequence");

            var sql = Generate(operation);

            Assert.Empty(sql);
        }

        [Fact]
        public void Generate_with_create_table_generates_fks()
        {
            var pegasusId = new Column("Id", typeof(long));
            new Table("Pegasus", new[] { pegasusId });
            var friend1Id = new Column("Friend1Id", typeof(long));
            var friend2Id = new Column("Friend2Id", typeof(long));
            var friendship = new Table("Friendship", new[] { friend1Id, friend2Id })
                {
                    PrimaryKey = new PrimaryKey("PegasusPK", new[] { friend1Id, friend2Id })
                };
            friendship.AddForeignKey(new ForeignKey("FriendshipFK1", new[] { friend1Id }, new[] { pegasusId }));
            friendship.AddForeignKey(new ForeignKey("FriendshipFK2", new[] { friend2Id }, new[] { pegasusId }));
            var operation = new CreateTableOperation(friendship);

            var sql = Generate(operation);

            Assert.Equal(
                @"CREATE TABLE ""Friendship"" (
    ""Friend1Id"" INTEGER,
    ""Friend2Id"" INTEGER,
    CONSTRAINT ""PegasusPK"" PRIMARY KEY (""Friend1Id"", ""Friend2Id""),
    CONSTRAINT ""FriendshipFK1"" FOREIGN KEY (""Friend1Id"") REFERENCES ""Pegasus"" (""Id""),
    CONSTRAINT ""FriendshipFK2"" FOREIGN KEY (""Friend2Id"") REFERENCES ""Pegasus"" (""Id"")
)",
                sql);
        }

        [Fact]
        public void Generate_with_rename_table_works()
        {
            var operation = new RenameTableOperation("my.Pegasus", "Pony");

            var sql = Generate(operation);

            Assert.Equal("ALTER TABLE \"my.Pegasus\" RENAME TO \"my.Pony\"", sql);
        }

        [Fact]
        public void Generate_with_move_table_works()
        {
            var operation = new MoveTableOperation("my.Pony", "bro");

            var sql = Generate(operation);

            Assert.Equal("ALTER TABLE \"my.Pony\" RENAME TO \"bro.Pony\"", sql);
        }

        [Fact]
        public void Generate_with_drop_index_works()
        {
            var operation = new DropIndexOperation("Pony", "Ponydex");

            var sql = Generate(operation);

            Assert.Equal("DROP INDEX \"Ponydex\"", sql);
        }

        [Fact]
        public void GenerateLiteral_with_byte_array_works()
        {
            var bros = new byte[] { 0xB2, 0x05 };

            var sql = CreateGenerator().GenerateLiteral(bros);

            Assert.Equal("X'B205'", sql);
        }

        [Fact]
        public void DelimitIdentifier_with_schema_qualified_name_works_when_no_schema()
        {
            var name = new SchemaQualifiedName("Pony");

            var sql = CreateGenerator().DelimitIdentifier(name);

            Assert.Equal("\"Pony\"", sql);
        }

        [Fact]
        public void DelimitIdentifier_with_schema_qualified_name_concatenates_parts_when_schema()
        {
            var name = new SchemaQualifiedName("Pony", "my");

            var sql = CreateGenerator().DelimitIdentifier(name);

            Assert.Equal("\"my.Pony\"", sql);
        }

        [Fact]
        public void Generate_uses_preprocessor()
        {
            var database = new DatabaseModel();
            var column = new Column("Id", typeof(string)) { IsNullable = false };
            var newColumn = new Column("Id", typeof(int)) { IsNullable = false };
            var table
                = new Table("A", new[] { column })
                    {
                        PrimaryKey = new PrimaryKey("PK", new[] { column })
                    };

            database.AddTable(table);

            var operations
                = new MigrationOperation[]
                    {
                        new AlterColumnOperation(table.Name, newColumn, isDestructiveChange: true)
                    };

            var stringBuilder = new StringBuilder();
            foreach (var statement in CreateGenerator(database).Generate(operations))
            {
                stringBuilder.AppendLine(statement.Sql);
            }

            Assert.Equal(
                @"DROP TABLE ""A""
CREATE TABLE ""A"" (
    ""Id"" INT NOT NULL,
    CONSTRAINT ""PK"" PRIMARY KEY (""Id"")
)
",
                stringBuilder.ToString());
        }

        private static string Generate(MigrationOperation operation, DatabaseModel database = null)
        {
            return CreateGenerator().Generate(operation).Sql;
        }

        private static SQLiteMigrationOperationSqlGenerator CreateGenerator(DatabaseModel database = null)
        {
            return new SQLiteMigrationOperationSqlGenerator(new SQLiteTypeMapper())
                {
                    Database = database ?? new DatabaseModel(),
                    DatabaseModelModifier = new DatabaseModelModifier()
                };
        }
    }
}
