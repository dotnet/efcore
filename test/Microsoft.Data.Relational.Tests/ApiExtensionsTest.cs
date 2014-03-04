// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Relational.Tests
{
    public class ApiExtensionsTest
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
            var model = new Entity.Metadata.Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>().ToTable("customers");

            Assert.Equal("customers", model.EntityTypes.Single().StorageName);
        }

        [Fact]
        public void ColumnName_sets_storage_name_on_entity_property()
        {
            var model = new Entity.Metadata.Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>().Properties(ps => ps.Property(c => c.Id).ColumnName("id"));

            Assert.Equal("id", model.EntityTypes.Single().Properties.Single().StorageName);
        }

        [Fact]
        public void ColumnType_sets_annotation_on_entity_property()
        {
            var model = new Entity.Metadata.Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>().Properties(ps => ps.Property(c => c.Id).ColumnType("bigint"));

            Assert.Equal("bigint", model.EntityTypes.Single().Properties.Single()[ApiExtensions.Annotations.StorageTypeName]);
        }
    }
}
