// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.Metadata
{
    public class ModelBuilderTest
    {
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
        public void Can_get_entity_builder_for_entity_type_name()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            var entityBuilder = modelBuilder.Entity("Customer");

            Assert.NotNull(entityBuilder);
            Assert.NotNull(model.TryGetEntityType("Customer"));
        }

        [Fact]
        public void Can_set_entity_key_from_clr_property()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>().Key(e => e.Id);

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entity.GetKey().Properties.Count());
            Assert.Equal("Id", entity.GetKey().Properties.First().Name);
        }

        [Fact]
        public void Can_set_entity_key_from_property_name()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>(b =>
                {
                    b.Property(e => e.Id);
                    b.Key("Id");
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entity.GetKey().Properties.Count());
            Assert.Equal("Id", entity.GetKey().Properties.First().Name);
        }

        [Fact]
        public void Can_set_entity_key_from_property_name_when_no_clr_property()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>(b =>
                {
                    b.Property<int>("Id");
                    b.Key("Id");
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entity.GetKey().Properties.Count());
            Assert.Equal("Id", entity.GetKey().Properties.First().Name);
        }

        [Fact]
        public void Can_set_entity_key_from_property_name_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity("Customer", b =>
                {
                    b.Property<int>("Id");
                    b.Key("Id");
                });

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
        public void Can_set_composite_entity_key_from_property_names()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>(b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.Name);
                    b.Key("Id", "Name");
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetKey().Properties.Count());
            Assert.Equal("Id", entity.GetKey().Properties.First().Name);
            Assert.Equal("Name", entity.GetKey().Properties.Last().Name);
        }

        [Fact]
        public void Can_set_composite_entity_key_from_property_names_when_mixed_properties()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>(b =>
                {
                    b.Property(e => e.Id);
                    b.Property<string>("Name");
                    b.Key("Id", "Name");
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetKey().Properties.Count());
            Assert.Equal("Id", entity.GetKey().Properties.First().Name);
            Assert.Equal("Name", entity.GetKey().Properties.Last().Name);
        }

        [Fact]
        public void Can_set_composite_entity_key_from_property_names_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity("Customer", ps =>
                {
                    ps.Property<int>("Id");
                    ps.Property<string>("Name");
                    ps.Key("Id", "Name");
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetKey().Properties.Count());
            Assert.Equal("Id", entity.GetKey().Properties.First().Name);
            Assert.Equal("Name", entity.GetKey().Properties.Last().Name);
        }

        [Fact]
        public void Can_set_entity_key_with_annotations()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>(
                    b => b.Key(e => new { e.Id, e.Name })
                        .Annotation("A1", "V1")
                        .Annotation("A2", "V2"));

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetKey().Properties.Count());
            Assert.Equal(new[] { "Id", "Name" }, entity.GetKey().Properties.Select(p => p.Name));
            Assert.Equal("V1", entity.GetKey()["A1"]);
            Assert.Equal("V2", entity.GetKey()["A2"]);
        }

        [Fact]
        public void Can_set_entity_key_with_annotations_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity("Customer", b =>
                {
                    b.Property<int>("Id");
                    b.Property<string>("Name");
                    b.Key("Id", "Name")
                        .Annotation("A1", "V1")
                        .Annotation("A2", "V2");
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetKey().Properties.Count());
            Assert.Equal(new[] { "Id", "Name" }, entity.GetKey().Properties.Select(p => p.Name));
            Assert.Equal("V1", entity.GetKey()["A1"]);
            Assert.Equal("V2", entity.GetKey()["A2"]);
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
        public void Can_set_entity_annotation_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity("Customer")
                .Annotation("foo", "bar");

            Assert.Equal("bar", model.GetEntityType(typeof(Customer))["foo"]);
        }

        [Fact]
        public void Can_set_property_annotation()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Property(c => c.Name).Annotation("foo", "bar");

            Assert.Equal("bar", model.GetEntityType(typeof(Customer)).GetProperty("Name")["foo"]);
        }

        [Fact]
        public void Can_set_property_annotation_when_no_clr_property()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Property<string>("Name").Annotation("foo", "bar");

            Assert.Equal("bar", model.GetEntityType(typeof(Customer)).GetProperty("Name")["foo"]);
        }

        [Fact]
        public void Can_set_property_annotation_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity("Customer")
                .Property<string>("Name").Annotation("foo", "bar");

            Assert.Equal("bar", model.GetEntityType(typeof(Customer)).GetProperty("Name")["foo"]);
        }

        [Fact]
        public void Can_set_use_of_store_sequence()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Property(c => c.Name).UseStoreSequence();

            var property = model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal(ValueGenerationOnAdd.Server, property.ValueGenerationOnAdd);
            Assert.Equal(ValueGenerationOnSave.None, property.ValueGenerationOnSave);
        }

        [Fact]
        public void Can_set_use_of_store_sequence_with_name_and_block_size()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Property(c => c.Name).UseStoreSequence("UltrasonicTarsier", 17);

            var property = model.GetEntityType(typeof(Customer)).GetProperty("Name");

            Assert.Equal(ValueGenerationOnAdd.Server, property.ValueGenerationOnAdd);
            Assert.Equal(ValueGenerationOnSave.None, property.ValueGenerationOnSave);
            Assert.Equal("UltrasonicTarsier", property["StoreSequenceName"]);
            Assert.Equal("17", property["StoreSequenceBlockSize"]);
        }

        [Fact]
        public void Can_add_multiple_properties()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>(b =>
                {
                    b.Property(c => c.Id);
                    b.Property(c => c.Name).Annotation("foo", "bar");
                });

            Assert.Equal(2, model.GetEntityType(typeof(Customer)).Properties.Count());
        }

        [Fact]
        public void Can_add_multiple_mixed_properties()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>(
                b =>
                    {
                        b.Property(c => c.Id);
                        b.Property<string>("Name").Annotation("foo", "bar");
                    });

            Assert.Equal(2, model.GetEntityType(typeof(Customer)).Properties.Count());
        }

        [Fact]
        public void Can_add_multiple_properties_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity("Customer", b =>
                {
                    b.Property<int>("Id");
                    b.Property<string>("Name").Annotation("foo", "bar");
                });

            Assert.Equal(2, model.GetEntityType(typeof(Customer)).Properties.Count());
        }

        [Fact]
        public void Can_add_foreign_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder
                .Entity<Order>()
                .ForeignKeys(fks => fks.ForeignKey<Customer>(c => c.CustomerId));

            var entityType = model.GetEntityType(typeof(Order));

            Assert.Equal(1, entityType.ForeignKeys.Count());
        }

        [Fact]
        public void Can_add_foreign_key_when_no_clr_property()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Key(c => c.Id);

            modelBuilder.Entity<Order>(b =>
                {
                    b.Property<int>("CustomerId");
                    b.ForeignKeys(fks => fks.ForeignKey("Customer", new[] { "CustomerId" }));
                });

            var entityType = model.GetEntityType(typeof(Order));

            Assert.Equal(1, entityType.ForeignKeys.Count());
        }

        [Fact]
        public void Can_add_foreign_key_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Key(c => c.Id);

            modelBuilder.Entity("Order", b =>
                {
                    b.Property<int>("CustomerId");
                    b.ForeignKeys(fks => fks.ForeignKey("Customer", "CustomerId"));
                });

            var entityType = model.GetEntityType(typeof(Order));

            Assert.Equal(1, entityType.ForeignKeys.Count());
        }

        [Fact]
        public void Can_add_foreign_key_when_no_clr_type_on_both_ends()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity("Customer", b =>
                {
                    b.Property<int>("Id");
                    b.Key(new[] { "Id" });
                });

            modelBuilder.Entity("Order", b =>
                {
                    b.Property<int>("CustomerId");
                    b.ForeignKeys(fks => fks.ForeignKey("Customer", "CustomerId"));
                });

            var entityType = model.GetEntityType(typeof(Order));

            Assert.Equal(1, entityType.ForeignKeys.Count());
        }

        [Fact]
        public void Can_add_multiple_foreign_keys()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>().Key(c => c.Id);

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

        [Fact]
        public void Can_add_multiple_foreign_keys_when_mixed_properties()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>().Key(c => c.Id);

            modelBuilder.Entity<Order>(b =>
                {
                    b.Property<int>("CustomerId");
                    b.ForeignKeys(
                        fks =>
                            {
                                fks.ForeignKey<Customer>(c => c.CustomerId);
                                fks.ForeignKey("Customer", "CustomerId").IsUnique();
                            });
                });

            var entityType = model.GetEntityType(typeof(Order));

            Assert.Equal(2, entityType.ForeignKeys.Count());
            Assert.True(entityType.ForeignKeys.Last().IsUnique);
        }

        [Fact]
        public void Can_add_multiple_foreign_keys_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>().Key(c => c.Id);

            modelBuilder
                .Entity("Order", b =>
                    {
                        b.Property<int>("CustomerId");
                        b.ForeignKeys(
                            fks =>
                                {
                                    fks.ForeignKey("Customer", "CustomerId");
                                    fks.ForeignKey("Customer", "CustomerId").IsUnique();
                                });
                    });

            var entityType = model.GetEntityType(typeof(Order));

            Assert.Equal(2, entityType.ForeignKeys.Count());
            Assert.True(entityType.ForeignKeys.Last().IsUnique);
        }

        [Fact]
        public void Can_add_multiple_foreign_keys_when_no_clr_type_on_both_ends()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity("Customer", b =>
                {
                    b.Property<int>("Id");
                    b.Key("Id");
                });

            modelBuilder.Entity("Order", b =>
                {
                    b.Property<int>("CustomerId");
                    b.ForeignKeys(
                        fks =>
                            {
                                fks.ForeignKey("Customer", "CustomerId");
                                fks.ForeignKey("Customer", "CustomerId").IsUnique();
                            });
                });

            var entityType = model.GetEntityType(typeof(Order));

            Assert.Equal(2, entityType.ForeignKeys.Count());
            Assert.True(entityType.ForeignKeys.Last().IsUnique);
        }

        [Fact]
        public void Can_add_index()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Indexes(ixs => ixs.Index(ix => ix.Name));

            var entityType = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entityType.Indexes.Count());
        }

        [Fact]
        public void Can_add_index_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity("Customer", b =>
                {
                    b.Property<string>("Name");
                    b.Indexes(ixs => ixs.Index("Name"));
                });

            var entityType = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entityType.Indexes.Count());
        }

        [Fact]
        public void Can_add_multiple_indexes()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Indexes(ixs =>
                    {
                        ixs.Index(ix => ix.Id).IsUnique();
                        ixs.Index(ix => ix.Name).Annotation("A1", "V1");
                    });

            var entityType = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entityType.Indexes.Count());
            Assert.True(entityType.Indexes.First().IsUnique);
            Assert.False(entityType.Indexes.Last().IsUnique);
            Assert.Equal("V1", entityType.Indexes.Last()["A1"]);
        }

        [Fact]
        public void Can_add_multiple_indexes_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity("Customer", ps =>
                {
                    ps.Property<int>("Id");
                    ps.Property<string>("Name");
                    ps.Indexes(ixs =>
                        {
                            ixs.Index("Id").IsUnique();
                            ixs.Index("Name").Annotation("A1", "V1");
                        });
                });

            var entityType = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entityType.Indexes.Count());
            Assert.True(entityType.Indexes.First().IsUnique);
            Assert.False(entityType.Indexes.Last().IsUnique);
            Assert.Equal("V1", entityType.Indexes.Last()["A1"]);
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class Order
        {
            public int OrderId { get; set; }

            public int CustomerId { get; set; }
            public Customer Customer { get; set; }
        }
    }
}
