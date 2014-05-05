// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Model;
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
    }
}
