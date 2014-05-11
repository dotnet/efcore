// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Model;
using Xunit;
using Moq;

namespace Microsoft.Data.Entity.Relational.Tests
{
    public class MetadataExtensionsTest
    {
        #region Fixture

        public class Customer
        {
            public int Id { get; set; }
        }

        #endregion

        [Fact]
        public void ToTable_sets_storage_name_on_entity()
        {
            var model = new Metadata.Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>().ToTable("customers");

            Assert.Equal("customers", model.EntityTypes.Single().StorageName);
        }

        [Fact]
        public void ColumnName_sets_storage_name_on_entity_property()
        {
            var model = new Metadata.Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>().Properties(ps => ps.Property(c => c.Id).ColumnName("id"));

            Assert.Equal("id", model.EntityTypes.Single().Properties.Single().StorageName);
        }

        [Fact]
        public void ColumnType_sets_annotation_on_entity_property()
        {
            var model = new Metadata.Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>().Properties(ps => ps.Property(c => c.Id).ColumnType("bigint"));

            Assert.Equal("bigint", model.EntityTypes.Single().Properties.Single()[MetadataExtensions.Annotations.StorageTypeName]);
        }

        [Fact]
        public void GetStoreGeneratedColumns_returns_store_generated_columns()
        {
            var table = new Table("table",
                new[]
                    {
                        new Column("Id", "storetype") { ValueGenerationStrategy = StoreValueGenerationStrategy.Identity },
                        new Column("Name", "_"),
                        new Column("LastUpdate", "_") { ValueGenerationStrategy = StoreValueGenerationStrategy.Computed }
                    });

            Assert.Equal(table.Columns.Where(c => c.Name != "Name"), table.GetStoreGeneratedColumns());
        }

        [Fact]
        public void GetStoreGeneratedColumns_validates_parameter_not_null()
        {
            Assert.Equal("table",
                Assert.Throws<ArgumentNullException>(() => MetadataExtensions.GetStoreGeneratedColumns(null)).ParamName);
        }

        [Fact]
        public void IsClustered_returns_true_by_default()
        {
            Assert.True(new Mock<Key>().Object.IsClustered());
        }
    }
}
