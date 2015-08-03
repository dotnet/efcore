// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Migrations.Builders;
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

            for (int i = migrationBuilder.Operations.Count - 1; i >= 0; i--)
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
