// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

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
        public void ToTable_sets_table_name_on_entity()
        {
            var model = new Metadata.Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>().ToTable("customers");

            Assert.Equal("customers", model.EntityTypes.Single().TableName());
            Assert.True(string.IsNullOrEmpty(model.EntityTypes.Single().Schema()));

            modelBuilder.Entity("Customer").ToTable("CUSTOMERS");

            Assert.Equal("CUSTOMERS", model.EntityTypes.Single().TableName());
            Assert.True(string.IsNullOrEmpty(model.EntityTypes.Single().Schema()));

            modelBuilder.Entity<Customer>().ToTable("my.table");

            Assert.Equal("my.table", model.EntityTypes.Single().TableName());
            Assert.True(string.IsNullOrEmpty(model.EntityTypes.Single().Schema()));

            modelBuilder.Entity<Customer>().ToTable("my.table", "my.schema");

            Assert.Equal("my.table", model.EntityTypes.Single().TableName());
            Assert.Equal("my.schema", model.EntityTypes.Single().Schema());
        }

        [Fact]
        public void ColumnName_sets_storage_name_on_entity_property()
        {
            var model = new Metadata.Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>().Property(c => c.Id).ColumnName("id");

            Assert.Equal("id", model.EntityTypes.Single().Properties.Single().ColumnName());

            modelBuilder.Entity<Customer>().Property<int>("Id").ColumnName("ID");

            Assert.Equal("ID", model.EntityTypes.Single().Properties.Single().ColumnName());
        }

        [Fact]
        public void ColumnType_sets_annotation_on_entity_property()
        {
            var model = new Metadata.Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>().Property(c => c.Id).ColumnType("bigint");

            Assert.Equal("bigint", model.EntityTypes.Single().Properties.Single()[MetadataExtensions.Annotations.StorageTypeName]);

            modelBuilder.Entity<Customer>().Property<int>("Id").ColumnType("BIGINT");

            Assert.Equal("BIGINT", model.EntityTypes.Single().Properties.Single()[MetadataExtensions.Annotations.StorageTypeName]);
        }

        [Fact]
        public void IsClustered_returns_true_by_default()
        {
            Assert.True(new Mock<Key>().Object.IsClustered());
        }
    }
}
