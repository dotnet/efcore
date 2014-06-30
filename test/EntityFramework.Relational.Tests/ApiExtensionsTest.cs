// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Microsoft.Data.Entity.Relational
{
    public class ApiExtensionsTest
    {
        #region Fixture

        public class Customer
        {
            public static PropertyInfo NameProperty = typeof(Customer).GetProperty("Name");

            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Order
        {
            public int OrderId { get; set; }

            public int CustomerId { get; set; }
            public Customer Customer { get; set; }
        }

        #endregion

        [Fact]
        public void Can_set_entity_table_name()
        {
            var model = new Microsoft.Data.Entity.Metadata.Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>().TableName("foo");

            Assert.Equal("foo", model.GetEntityType(typeof(Customer)).TableName());
        }

        [Fact]
        public void Can_set_entity_table_name_with_dot()
        {
            var model = new Microsoft.Data.Entity.Metadata.Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>().TableName("my.table");

            Assert.Equal("my.table", model.GetEntityType(typeof(Customer)).TableName());
        }

        [Fact]
        public void Can_set_entity_table_name_and_schema()
        {
            var model = new Microsoft.Data.Entity.Metadata.Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>().TableName("foo", "schema");

            Assert.Equal("foo", model.GetEntityType(typeof(Customer)).TableName());
            Assert.Equal("schema", model.GetEntityType(typeof(Customer)).Schema());
        }

        [Fact]
        public void Can_set_entity_table_name_when_no_clr_type()
        {
            var model = new Microsoft.Data.Entity.Metadata.Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity("Customer")
                .TableName("foo");

            Assert.Equal("foo", model.GetEntityType(typeof(Customer)).TableName());
        }

        [Fact]
        public void Can_set_entity_table_name_and_schema_when_no_clr_type()
        {
            var model = new Microsoft.Data.Entity.Metadata.Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity("Customer")
                .TableName("foo", "schema");

            Assert.Equal("foo", model.GetEntityType(typeof(Customer)).TableName());
            Assert.Equal("schema", model.GetEntityType(typeof(Customer)).Schema());
        }

        [Fact]
        public void Can_set_property_column_name()
        {
            var model = new Microsoft.Data.Entity.Metadata.Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Properties(ps => ps.Property(c => c.Name).ColumnName("foo"));

            Assert.Equal("foo", model.GetEntityType(typeof(Customer)).GetProperty("Name").ColumnName());
        }

        [Fact]
        public void Can_set_property_column_name_when_no_clr_property()
        {
            var model = new Microsoft.Data.Entity.Metadata.Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Properties(ps => ps.Property<string>("Name").ColumnName("foo"));

            Assert.Equal("foo", model.GetEntityType(typeof(Customer)).GetProperty("Name").ColumnName());
        }

        [Fact]
        public void Can_set_property_column_name_when_no_clr_type()
        {
            var model = new Microsoft.Data.Entity.Metadata.Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity("Customer")
                .Properties(ps => ps.Property<string>("Name").ColumnName("foo"));

            Assert.Equal("foo", model.GetEntityType(typeof(Customer)).GetProperty("Name").ColumnName());
        }

        [Fact]
        public void Can_set_foreign_key_name()
        {
            var model = new Microsoft.Data.Entity.Metadata.Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity("Customer")
                .Properties(ps => ps.Property<int>("Id"))
                .Key("Id");

            modelBuilder
                .Entity("Order")
                .Properties(ps => ps.Property<int>("CustomerId"))
                .ForeignKeys(fks => fks.ForeignKey("Customer", "CustomerId").KeyName("FK_Foo"));

            Assert.Equal("FK_Foo", model.GetEntityType(typeof(Order)).ForeignKeys.Single().KeyName());
        }

        [Fact]
        public void Entity_table_name_defaults_to_name()
        {
            var entityType = new EntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.TableName());
        }

        [Fact]
        public void Entity_table_name_can_be_different_from_name()
        {
            var entityType = new EntityType(typeof(Customer));
            entityType.SetTableName("CustomerTable");

            Assert.Equal("CustomerTable", entityType.TableName());
        }

        [Fact]
        public void Property_column_name_defaults_to_name()
        {
            var model = new Microsoft.Data.Entity.Metadata.Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity("Customer")
                .Properties(ps => ps.Property<string>("Name"));

            Assert.Equal("Name", model.GetEntityType(typeof(Customer)).GetProperty("Name").ColumnName());
        }

        [Fact]
        public void Property_column_name_can_be_different_from_name()
        {
            var model = new Microsoft.Data.Entity.Metadata.Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity("Customer")
                .Properties(ps => ps.Property<string>("Name"));

            model.GetEntityType(typeof(Customer)).GetProperty("Name").SetColumnName("CustomerName");

            Assert.Equal("CustomerName", model.GetEntityType(typeof(Customer)).GetProperty("Name").ColumnName());
        }
    }
}
