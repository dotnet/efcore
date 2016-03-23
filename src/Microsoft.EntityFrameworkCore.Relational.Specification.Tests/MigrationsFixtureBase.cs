// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class MigrationsFixtureBase
    {
        public static string ActiveProvider { get; set; }

        public abstract MigrationsContext CreateContext();

        public abstract EmptyMigrationsContext CreateEmptyContext();

        public class EmptyMigrationsContext : DbContext
        {
            public EmptyMigrationsContext(DbContextOptions options)
                : base(options)
            {
            }
        }

        public class MigrationsContext : DbContext
        {
            public MigrationsContext(DbContextOptions options)
                : base(options)
            {
            }
        }

        [DbContext(typeof(MigrationsContext))]
        [Migration("00000000000001_Migration1")]
        private class Migration1 : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
                MigrationsFixtureBase.ActiveProvider = migrationBuilder.ActiveProvider;

                migrationBuilder
                    .CreateTable(
                        name: "Table1",
                        columns: x => new
                        {
                            Id = x.Column<int>()
                        })
                    .PrimaryKey(
                        name: "PK_Table1",
                        columns: x => x.Id);
            }

            protected override void Down(MigrationBuilder migrationBuilder)
                => migrationBuilder.DropTable("Table1");
        }

        [DbContext(typeof(MigrationsContext))]
        [Migration("00000000000002_Migration2")]
        private class Migration2 : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
                => migrationBuilder.RenameTable(
                    name: "Table1",
                    newName: "Table2");

            protected override void Down(MigrationBuilder migrationBuilder)
                => migrationBuilder.RenameTable(
                    name: "Table2",
                    newName: "Table1");
        }

        [DbContext(typeof(MigrationsContext))]
        [Migration("00000000000003_Migration3")]
        private class Migration3 : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
                if (ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
                {
                    migrationBuilder.Sql("CREATE DATABASE TransactionSuppressed", suppressTransaction: true);
                    migrationBuilder.Sql("DROP DATABASE TransactionSuppressed", suppressTransaction: true);
                }
            }

            protected override void Down(MigrationBuilder migrationBuilder)
            {
            }
        }
    }
}
