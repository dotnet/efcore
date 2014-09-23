// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Migrations.Model;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests.Infrastructure
{
    public class MigrationMetadataTest
    {
        [Fact]
        public void Create_migration_metadata()
        {
            var targetModel = new Metadata.Model();
            var upgradeOperations = new MigrationOperation[0];
            var downgradeOperations = new MigrationOperation[0];

            var migration
                = new MigrationMetadata("000000000000001_Name")
                    {
                        TargetModel = targetModel,
                        UpgradeOperations = upgradeOperations,
                        DowngradeOperations = downgradeOperations,
                    };

            Assert.Equal("000000000000001_Name", migration.MigrationId);
            Assert.Equal(MigrationMetadata.CurrentProductVersion, migration.ProductVersion);
            Assert.Same(targetModel, migration.TargetModel);
            Assert.Same(upgradeOperations, migration.UpgradeOperations);
            Assert.Same(downgradeOperations, migration.DowngradeOperations);            
        }

        [Fact]
        public void Create_migration_metadata_with_product_version()
        {
            var targetModel = new Metadata.Model();
            var upgradeOperations = new MigrationOperation[0];
            var downgradeOperations = new MigrationOperation[0];

            var migration
                = new MigrationMetadata("000000000000001_Name", "1.2.3.4")
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
    }
}
