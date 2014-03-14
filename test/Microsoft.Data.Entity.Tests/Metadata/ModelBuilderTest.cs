// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class ModelBuilderTest
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
        public void Can_get_entity_builder_for_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            var entityBuilder = modelBuilder.Entity<Customer>();

            Assert.NotNull(entityBuilder);
            Assert.Equal("Customer", model.GetEntityType(typeof(Customer)).Name);
        }

        [Fact]
        public void Can_set_entity_key_from_clr_properties()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>().Key(e => e.Id);

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entity.GetKey().Properties.Count());
            Assert.Equal("Id", entity.GetKey().Properties.First().Name);
        }

        [Fact]
        public void Can_set_composite_entity_key_from_clr_properties()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Key(e => new { e.Id, e.Name });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetKey().Properties.Count());
            Assert.Equal("Id", entity.GetKey().Properties.First().Name);
            Assert.Equal("Name", entity.GetKey().Properties.Last().Name);
        }

        [Fact]
        public void Can_set_entity_annotation()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Annotation("foo", "bar");

            Assert.Equal("bar", model.GetEntityType(typeof(Customer))["foo"]);
        }

        [Fact]
        public void Can_set_entity_storage_name()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .StorageName("foo");

            Assert.Equal("foo", model.GetEntityType(typeof(Customer)).StorageName);
        }

        [Fact]
        public void Can_set_property_annotation()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Properties(ps => ps.Property(c => c.Name).Annotation("foo", "bar"));

            Assert.Equal("bar", model.GetEntityType(typeof(Customer)).GetProperty("Name")["foo"]);
        }

        [Fact]
        public void Can_set_property_storage_name()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Properties(ps => ps.Property(c => c.Name).StorageName("foo"));

            Assert.Equal("foo", model.GetEntityType(typeof(Customer)).GetProperty("Name").StorageName);
        }

        [Fact]
        public void Can_add_multiple_properties()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Properties(
                    ps =>
                        {
                            ps.Property(c => c.Id);
                            ps.Property(c => c.Name).Annotation("foo", "bar");
                        });

            Assert.Equal(2, model.GetEntityType(typeof(Customer)).Properties.Count());
        }

        [Fact]
        public void Can_add_foreign_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Order>()
                .ForeignKeys(fks => fks.ForeignKey<Customer>(c => c.CustomerId));

            var entityType = model.GetEntityType(typeof(Order));

            Assert.Equal(1, entityType.ForeignKeys.Count());
        }

        [Fact]
        public void Can_add_multiple_foreign_keys()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Order>()
                .ForeignKeys(
                    fks =>
                        {
                            fks.ForeignKey<Customer>(c => c.CustomerId);
                            fks.ForeignKey<Customer>(c => c.CustomerId).IsUnique();
                        });

            var entityType = model.GetEntityType(typeof(Order));

            Assert.Equal(2, entityType.ForeignKeys.Count());
            Assert.True(entityType.ForeignKeys.Last().IsUnique);
        }
    }
}
