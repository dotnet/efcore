// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.Builders;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.Infrastructure
{
    public class MigrationInfoTest
    {
        [Fact]
        public void Create_migration_info()
        {
            var targetModel = new Entity.Metadata.Model();
            var upgradeOperations = new MigrationOperation[0];
            var downgradeOperations = new MigrationOperation[0];

            var migration
                = new MigrationInfo("000000000000001_Name")
                    {
                        TargetModel = targetModel,
                        UpgradeOperations = upgradeOperations,
                        DowngradeOperations = downgradeOperations,
                    };

            Assert.Equal("000000000000001_Name", migration.MigrationId);
            Assert.Equal(MigrationInfo.CurrentProductVersion, migration.ProductVersion);
            Assert.Same(targetModel, migration.TargetModel);
            Assert.Same(upgradeOperations, migration.UpgradeOperations);
            Assert.Same(downgradeOperations, migration.DowngradeOperations);
        }

        [Fact]
        public void Create_migration_info_with_product_version()
        {
            var targetModel = new Entity.Metadata.Model();
            var upgradeOperations = new MigrationOperation[0];
            var downgradeOperations = new MigrationOperation[0];

            var migration
                = new MigrationInfo("000000000000001_Name", "1.2.3.4")
                    {
                        TargetModel = targetModel,
                        UpgradeOperations = upgradeOperations,
                        DowngradeOperations = downgradeOperations,
                    };

            Assert.Equal("000000000000001_Name", migration.MigrationId);
            Assert.Equal("1.2.3.4", migration.ProductVersion);
            Assert.Same(targetModel, migration.TargetModel);
            Assert.Same(upgradeOperations, migration.UpgradeOperations);
            Assert.Same(downgradeOperations, migration.DowngradeOperations);
        }

        [Fact]
        public void Create_migration_info_from_migration()
        {
            var migration = new MigrationInfo(new TestMigration());

            Assert.Equal("000000000000001_Name", migration.MigrationId);
            Assert.Equal("1.2.3.4", migration.ProductVersion);
            Assert.Equal("AModel", ((Entity.Metadata.Model)migration.TargetModel).StorageName);
            Assert.Equal(new[] { "UpSql1", "UpSql2" }, migration.UpgradeOperations.Select(o => ((SqlOperation)o).Sql));
            Assert.Equal(new[] { "DownSql1", "DownSql2" }, migration.DowngradeOperations.Select(o => ((SqlOperation)o).Sql));
        }

        [Fact]
        public void Create_migration_info_from_migration_without_metadatata_throws()
        {
            Assert.Equal(
                Strings.MissingMigrationMetadata(typeof(TestMigrationNoMetadata)),
                Assert.Throws<InvalidCastException>(() => new MigrationInfo(new TestMigrationNoMetadata())).Message);
        }

        private class TestMigration : Migration, IMigrationMetadata
        {
            public override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.Sql("UpSql1");
                migrationBuilder.Sql("UpSql2");
            }

            public override void Down(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.Sql("DownSql1");
                migrationBuilder.Sql("DownSql2");
            }

            public string MigrationId
            {
                get { return "000000000000001_Name"; }
            }

            public string ProductVersion
            {
                get { return "1.2.3.4"; }
            }

            public IModel TargetModel
            {
                get { return new Entity.Metadata.Model { StorageName = "AModel" }; }
            }
        }

        private class TestMigrationNoMetadata : Migration
        {
            public override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.Sql("UpSql");
            }

            public override void Down(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.Sql("DownSql");
            }
        }
    }
}
