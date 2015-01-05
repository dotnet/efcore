// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.Builders;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations
{
    public class MigrationTest
    {
        [Fact]
        public void Get_upgrade_operations()
        {
            var migration = new AMigration();
            var migrationBuilder = new MigrationBuilder();

            migration.Up(migrationBuilder);

            Assert.Equal(2, migrationBuilder.Operations.Count);
            Assert.IsType<CreateTableOperation>(migrationBuilder.Operations[0]);
            Assert.IsType<AddColumnOperation>(migrationBuilder.Operations[1]);
        }

        [Fact]
        public void Get_downgrade_operations()
        {
            var migration = new AMigration();
            var migrationBuilder = new MigrationBuilder();

            migration.Down(migrationBuilder);

            Assert.Equal(2, migrationBuilder.Operations.Count);
            Assert.IsType<DropColumnOperation>(migrationBuilder.Operations[0]);
            Assert.IsType<DropTableOperation>(migrationBuilder.Operations[1]);
        }

        public class AMigration : Migration
        {
            public override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.CreateTable(
                    "dbo.MyTable",
                    c => new
                        {
                            Id = c.Int()
                        })
                    .PrimaryKey("MyPK", t => t.Id);

                migrationBuilder.AddColumn("dbo.MyTable", "Foo", c => c.String());
            }

            public override void Down(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.DropColumn("dbo.MyTable", "Foo");
                migrationBuilder.DropTable("dbo.MyTable");
            }
        }
    }
}
