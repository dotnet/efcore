// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Migrations.Builders;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Migrations.Model;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests
{
    public class MigrationTest
    {
        [Fact]
        public void Get_upgrade_operations()
        {
            var migration = (IMigrationMetadata)new AMigration();

            Assert.Equal(2, migration.UpgradeOperations.Count);
            Assert.IsType<CreateTableOperation>(migration.UpgradeOperations[0]);
            Assert.IsType<AddColumnOperation>(migration.UpgradeOperations[1]);
        }

        [Fact]
        public void Get_downgrade_operations()
        {
            var migration = (IMigrationMetadata)new AMigration();

            Assert.Equal(2, migration.DowngradeOperations.Count);
            Assert.IsType<DropColumnOperation>(migration.DowngradeOperations[0]);
            Assert.IsType<DropTableOperation>(migration.DowngradeOperations[1]);
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
