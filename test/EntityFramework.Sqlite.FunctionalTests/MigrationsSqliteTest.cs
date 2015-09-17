// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Operations;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class MigrationsSqliteTest : MigrationsTestBase<MigrationsSqliteFixture>
    {
        public MigrationsSqliteTest(MigrationsSqliteFixture fixture)
            : base(fixture)
        {
        }

        public override void Can_generate_up_scripts()
        {
            base.Can_generate_up_scripts();

            Assert.Equal(
                @"CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
    ""MigrationId"" TEXT NOT NULL CONSTRAINT ""PK_HistoryRow"" PRIMARY KEY,
    ""ProductVersion"" TEXT NOT NULL
);

CREATE TABLE ""Table1"" (
    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Table1"" PRIMARY KEY
);

INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
VALUES ('00000000000001_Migration1', '7.0.0-test');

ALTER TABLE ""Table1"" RENAME TO ""Table2"";

INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
VALUES ('00000000000002_Migration2', '7.0.0-test');

",
                Sql);
        }

        public override void Can_generate_idempotent_up_scripts()
        {
            Assert.Throws<NotSupportedException>(() => base.Can_generate_idempotent_up_scripts());
        }

        public override void Can_generate_down_scripts()
        {
            base.Can_generate_down_scripts();

            Assert.Equal(
                @"ALTER TABLE ""Table2"" RENAME TO ""Table1"";

DELETE FROM ""__EFMigrationsHistory""
WHERE ""MigrationId"" = '00000000000002_Migration2';

DROP TABLE ""Table1"";

DELETE FROM ""__EFMigrationsHistory""
WHERE ""MigrationId"" = '00000000000001_Migration1';

",
                Sql);
        }

        public override void Can_generate_idempotent_down_scripts()
        {
            Assert.Throws<NotSupportedException>(() => base.Can_generate_idempotent_down_scripts());
        }

        public override void Can_get_active_provider()
        {
            base.Can_get_active_provider();

            Assert.Equal("EntityFramework.Sqlite", ActiveProvider);
        }

        protected override void AssertFirstMigration(DbConnection connection)
        {
            var sql = GetDatabaseSchemaAsync(connection);
            Assert.Equal(
                @"
CreatedTable
    Id INTEGER NOT NULL
    ColumnWithDefaultToDrop INTEGER NULL DEFAULT 0
    ColumnWithDefaultToAlter INTEGER NULL DEFAULT 1
",
                sql);
        }

        protected override void BuildSecondMigration(MigrationBuilder migrationBuilder)
        {
            base.BuildSecondMigration(migrationBuilder);

            for (var i = migrationBuilder.Operations.Count - 1; i >= 0; i--)
            {
                var operation = migrationBuilder.Operations[i];
                if (operation is AlterColumnOperation
                    || operation is DropColumnOperation)
                {
                    migrationBuilder.Operations.RemoveAt(i);
                }
            }
        }

        protected override void AssertSecondMigration(DbConnection connection)
        {
            var sql = GetDatabaseSchemaAsync(connection);
            Assert.Equal(
                @"
CreatedTable
    Id INTEGER NOT NULL
    ColumnWithDefaultToDrop INTEGER NULL DEFAULT 0
    ColumnWithDefaultToAlter INTEGER NULL DEFAULT 1
",
                sql);
        }

        private string GetDatabaseSchemaAsync(DbConnection connection)
        {
            var builder = new IndentedStringBuilder();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT name
                FROM sqlite_master
                WHERE type = 'table'
                ORDER BY name;";

            var tables = new List<string>();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    tables.Add(reader.GetString(0));
                }
            }

            var first = true;
            foreach (var table in tables)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.DecrementIndent();
                }

                builder
                    .AppendLine()
                    .AppendLine(table)
                    .IncrementIndent();

                command.CommandText = "PRAGMA table_info(" + table + ");";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        builder
                            .Append(reader[1]) // Name
                            .Append(" ")
                            .Append(reader[2]) // Type
                            .Append(" ")
                            .Append(reader.GetBoolean(3) ? "NOT NULL" : "NULL");

                        if (!reader.IsDBNull(4))
                        {
                            builder
                                .Append(" DEFAULT ")
                                .Append(reader[4]);
                        }

                        builder.AppendLine();
                    }
                }
            }

            return builder.ToString();
        }
    }
}
