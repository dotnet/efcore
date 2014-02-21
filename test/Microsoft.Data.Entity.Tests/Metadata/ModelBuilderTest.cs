// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Xunit;

namespace Microsoft.Data.Entity.Metadata
{
    public class ModelBuilderTest
    {
        #region Fixture

        public class Customer
        {
            public static PropertyInfo NameProperty
                = typeof(Customer).GetProperty("Name");

            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion

        [Fact]
        public void Can_get_entity_builder_for_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            var entityBuilder = modelBuilder.Entity<Customer>();

            Assert.NotNull(entityBuilder);
            Assert.Equal("Customer", model.Entity(typeof(Customer)).Name);
        }

        [Fact]
        public void Can_set_entity_key_from_clr_properties()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>().Key(e => e.Id);

            var entity = model.Entity(typeof(Customer));

            Assert.Equal(1, entity.Key.Count());
            Assert.Equal("Id", entity.Key.First().Name);
        }

        [Fact]
        public void Can_set_composite_entity_key_from_clr_properties()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Key(e => new { e.Id, e.Name });

            var entity = model.Entity(typeof(Customer));

            Assert.Equal(2, entity.Key.Count());
            Assert.Equal("Id", entity.Key.First().Name);
            Assert.Equal("Name", entity.Key.Last().Name);
        }

        [Fact]
        public void Can_set_entity_annotation()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Annotation("foo", "bar");

            Assert.Equal("bar", model.Entity(typeof(Customer))["foo"]);
        }

        [Fact]
        public void Can_set_entity_storage_name()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .StorageName("foo");

            Assert.Equal("foo", model.Entity(typeof(Customer)).StorageName);
        }

        [Fact]
        public void Can_set_property_annotation()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Properties(ps => ps.Property(c => c.Name).Annotation("foo", "bar"));

            Assert.Equal(
                "bar",
                model.Entity(typeof(Customer)).Property("Name")["foo"]);
        }

        [Fact]
        public void Can_set_property_storage_name()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Properties(ps => ps.Property(c => c.Name).StorageName("foo"));

            Assert.Equal(
                "foo",
                model.Entity(typeof(Customer)).Property("Name").StorageName);
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

            Assert.Equal(2, model.Entity(typeof(Customer)).Properties.Count());
        }
    }
}
