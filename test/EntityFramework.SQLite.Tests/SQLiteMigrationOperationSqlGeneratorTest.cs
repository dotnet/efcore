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

            Assert.Equal(
                Strings.FormatMigrationOperationNotSupported(typeof(SQLiteMigrationOperationSqlGenerator), operation.GetType()),
                Assert.Throws<NotSupportedException>(() => Generate(operation)).Message);
        }

        [Fact]
        public void Generate_with_drop_database_not_supported()
        {
            var operation = new DropDatabaseOperation("Bronies");

            Assert.Equal(
                Strings.FormatMigrationOperationNotSupported(typeof(SQLiteMigrationOperationSqlGenerator), operation.GetType()),
                Assert.Throws<NotSupportedException>(() => Generate(operation)).Message);
        }

        [Fact]
        public void Generate_with_create_sequence_not_supported()
        {
            var operation = new CreateSequenceOperation(new Sequence("EpisodeSequence", "bigint", 0, 1));

            Assert.Equal(
                Strings.FormatMigrationOperationNotSupported(typeof(SQLiteMigrationOperationSqlGenerator), operation.GetType()),
                Assert.Throws<NotSupportedException>(() => Generate(operation)).Message);
        }

        [Fact]
        public void Generate_with_drop_sequence_is_not_supported()
        {
            var operation = new DropSequenceOperation("EpisodeSequence");

            Assert.Equal(
                Strings.FormatMigrationOperationNotSupported(typeof(SQLiteMigrationOperationSqlGenerator), operation.GetType()),
                Assert.Throws<NotSupportedException>(() => Generate(operation)).Message);
        }

        [Fact]
        public void Generate_with_move_sequence_is_not_supported()
        {
            var operation = new RenameSequenceOperation("EpisodeSequence", "RenamedSchema");

            Assert.Equal(
                Strings.FormatMigrationOperationNotSupported(typeof(SQLiteMigrationOperationSqlGenerator), operation.GetType()),
                Assert.Throws<NotSupportedException>(() => Generate(operation)).Message);
        }

        [Fact]
        public void Generate_with_alter_sequence_is_not_supported()
        {
            var operation = new AlterSequenceOperation("EpisodeSequence", 7);

            Assert.Equal(
                Strings.FormatMigrationOperationNotSupported(typeof(SQLiteMigrationOperationSqlGenerator), operation.GetType()),
                Assert.Throws<NotSupportedException>(() => Generate(operation)).Message);
        }

        [Fact]
        public void Generate_with_create_table_with_unique_constraints()
        {
            var c0 = new Column("Id", typeof(long));
            var c1 = new Column("C1", typeof(int));
            var c2 = new Column("C2", typeof(string));
            var table = new Table("T", new[] { c0, c1, c2 })
                {
                    PrimaryKey = new PrimaryKey("PK", new[] { c0 })
                };

            table.AddUniqueConstraint(new UniqueConstraint("UC0", new[] { c0, c1 }));
            table.AddUniqueConstraint(new UniqueConstraint("UC1", new[] { c2 }));

            var operation = new CreateTableOperation(table);
            var sql = Generate(operation);

            Assert.Equal(
                @"CREATE TABLE ""T"" (
    ""Id"" INTEGER,
    ""C1"" INT,
    ""C2"" CHAR,
    CONSTRAINT ""PK"" PRIMARY KEY (""Id""),
    UNIQUE (""Id"", ""C1""),
    UNIQUE (""C2"")
)",
                sql);
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

        private static string Generate(MigrationOperation operation, DatabaseModel database = null)
        {
            return CreateGenerator().Generate(operation).Sql;
        }

        private static SQLiteMigrationOperationSqlGenerator CreateGenerator(DatabaseModel database = null)
        {
            return new SQLiteMigrationOperationSqlGenerator(new SQLiteTypeMapper())
                {
                    Database = database ?? new DatabaseModel(),
                };
        }
    }
}
