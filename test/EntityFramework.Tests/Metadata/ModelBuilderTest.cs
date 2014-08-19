// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
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
        public void Can_get_entity_builder_for_clr_type_non_generic()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            var entityBuilder = modelBuilder.Entity(typeof(Customer));

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
        public void Can_set_entity_key_from_CLR_property_non_generic()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity(typeof(Customer), b =>
                {
                    b.Property<int>("Id");
                    b.Key("Id");
                });

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

            modelBuilder.Entity<Customer>();

            Assert.Equal(3, model.GetEntityType(typeof(Customer)).Properties.Count());
        }

        [Fact]
        public void Can_add_multiple_mixed_properties()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>();

            Assert.Equal(3, model.GetEntityType(typeof(Customer)).Properties.Count());
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
                .ForeignKey<Customer>(c => c.CustomerId);

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
                    b.ForeignKey("Customer", new[] { "CustomerId" });
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
                    b.ForeignKey("Customer", "CustomerId");
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
                    b.ForeignKey("Customer", "CustomerId");
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

            modelBuilder.Entity<Order>(b =>
                {
                    b.ForeignKey<Customer>(c => c.CustomerId);
                    b.ForeignKey<Customer>(c => c.CustomerId).IsUnique();
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
                    b.ForeignKey<Customer>(c => c.CustomerId);
                    b.ForeignKey("Customer", "CustomerId").IsUnique();
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
                        b.ForeignKey("Customer", "CustomerId");
                        b.ForeignKey("Customer", "CustomerId").IsUnique();
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
                    b.ForeignKey("Customer", "CustomerId");
                    b.ForeignKey("Customer", "CustomerId").IsUnique();
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
                .Index(ix => ix.Name);

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
                    b.Index("Name");
                });

            var entityType = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entityType.Indexes.Count());
        }

        [Fact]
        public void Can_add_multiple_indexes()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>(b =>
                {
                    b.Index(ix => ix.Id).IsUnique();
                    b.Index(ix => ix.Name).Annotation("A1", "V1");
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

            modelBuilder.Entity("Customer", b =>
                {
                    b.Property<int>("Id");
                    b.Property<string>("Name");
                    b.Index("Id").IsUnique();
                    b.Index("Name").Annotation("A1", "V1");
                });

            var entityType = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entityType.Indexes.Count());
            Assert.True(entityType.Indexes.First().IsUnique);
            Assert.False(entityType.Indexes.Last().IsUnique);
            Assert.Equal("V1", entityType.Indexes.Last()["A1"]);
        }

        [Fact]
        public void OneToMany_finds_existing_navs_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder
                .Entity<Order>()
                .ForeignKey<Customer>(c => c.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navToPrincipal = new Navigation(fk, "Customer", true);
            dependentType.AddNavigation(navToPrincipal);
            var navToDependent = new Navigation(fk, "Orders", false);
            principalType.AddNavigation(navToDependent);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Customer>().OneToMany(e => e.Orders, e => e.Customer);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Same(navToPrincipal, dependentType.Navigations.Single());
            Assert.Same(navToDependent, principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_finds_existing_nav_to_principal_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder
                .Entity<Order>()
                .ForeignKey<Customer>(c => c.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navigation = new Navigation(fk, "Customer", true);
            dependentType.AddNavigation(navigation);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Customer>().OneToMany(e => e.Orders, e => e.Customer);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Same(navigation, dependentType.Navigations.Single());
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_finds_existing_nav_to_dependent_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder
                .Entity<Order>()
                .ForeignKey<Customer>(c => c.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navigation = new Navigation(fk, "Orders", false);
            principalType.AddNavigation(navigation);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Customer>().OneToMany(e => e.Orders, e => e.Customer);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Same(navigation, principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_uses_existing_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder
                .Entity<Order>()
                .ForeignKey<Customer>(c => c.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Customer>().OneToMany(e => e.Orders, e => e.Customer);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_creates_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Customer>().OneToMany(e => e.Orders, e => e.Customer);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Customer>().OneToMany(e => e.Orders);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_from_other_end_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            // Passing null as the first arg is not super-compelling, but it is consistent
            modelBuilder.Entity<Customer>().OneToMany<Order>(null, e => e.Customer);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_can_create_relationship_with_no_navigations()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Customer>().OneToMany<Order>();

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_uses_specified_FK_even_if_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .ForeignKey(e => e.CustomerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_uses_existing_FK_not_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder
                .Entity<Pickle>()
                .ForeignKey<BigMak>(c => c.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<BigMak>()
                .OneToMany(e => e.Pickles, e => e.BigMak)
                .ForeignKey(e => e.BurgerId);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_creates_FK_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<BigMak>()
                .OneToMany(e => e.Pickles, e => e.BigMak)
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<BigMak>()
                .OneToMany(e => e.Pickles)
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_from_other_end_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<BigMak>()
                .OneToMany<Pickle>(null, e => e.BigMak)
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_can_create_relationship_with_no_navigations_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<BigMak>()
                .OneToMany<Pickle>()
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_creates_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<BigMak>().OneToMany(e => e.Pickles, e => e.BigMak);

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount + 1, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_nav_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<BigMak>().OneToMany(e => e.Pickles);

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount + 1, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_from_other_end_nav_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<BigMak>().OneToMany<Pickle>(null, e => e.BigMak);

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount + 1, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_can_create_relationship_with_no_navigations_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<BigMak>().OneToMany<Pickle>();

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount + 1, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_matches_shadow_FK_property_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property<int>("BigMakId", true);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            var fkProperty = dependentType.GetProperty("BigMakId");

            modelBuilder.Entity<BigMak>().OneToMany(e => e.Pickles, e => e.BigMak);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_new_FK_when_uniqueness_does_not_match()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder
                .Entity<Pickle>()
                .ForeignKey<BigMak>(c => c.BurgerId)
                .IsUnique();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<BigMak>()
                .OneToMany(e => e.Pickles, e => e.BigMak)
                .ForeignKey(e => e.BurgerId);

            Assert.Equal(2, dependentType.ForeignKeys.Count);
            var newFk = dependentType.ForeignKeys.Single(k => k != fk);

            Assert.True(fk.IsUnique);
            Assert.False(newFk.IsUnique);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(newFk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(newFk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_can_use_explicitly_specified_PK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("Id");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .ReferencedKey(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_can_use_non_PK_principal()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .ReferencedKey(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_can_have_both_convention_properties_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("Id");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .ForeignKey(e => e.CustomerId)
                .ReferencedKey(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_can_have_both_convention_properties_specified_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("Id");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .ReferencedKey(e => e.Id)
                .ForeignKey(e => e.CustomerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_can_have_FK_by_convention_specified_with_explicit_principal_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .ForeignKey(e => e.CustomerId)
                .ReferencedKey(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_can_have_FK_by_convention_specified_with_explicit_principal_key_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .ReferencedKey(e => e.AlternateKey)
                .ForeignKey(e => e.CustomerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_can_have_principal_key_by_convention_specified_with_explicit_PK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<BigMak>()
                .OneToMany(e => e.Pickles, e => e.BigMak)
                .ForeignKey(e => e.BurgerId)
                .ReferencedKey(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_can_have_principal_key_by_convention_specified_with_explicit_PK_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<BigMak>()
                .OneToMany(e => e.Pickles, e => e.BigMak)
                .ReferencedKey(e => e.AlternateKey)
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_finds_existing_navs_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder
                .Entity<Order>()
                .ForeignKey<Customer>(c => c.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navToPrincipal = new Navigation(fk, "Customer", true);
            dependentType.AddNavigation(navToPrincipal);
            var navToDependent = new Navigation(fk, "Orders", false);
            principalType.AddNavigation(navToDependent);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Order>().ManyToOne(e => e.Customer, e => e.Orders);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Same(navToPrincipal, dependentType.Navigations.Single());
            Assert.Same(navToDependent, principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_finds_existing_nav_to_principal_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder
                .Entity<Order>()
                .ForeignKey<Customer>(c => c.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navigation = new Navigation(fk, "Customer", true);
            dependentType.AddNavigation(navigation);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Order>().ManyToOne(e => e.Customer, e => e.Orders);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Same(navigation, dependentType.Navigations.Single());
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_finds_existing_nav_to_dependent_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder
                .Entity<Order>()
                .ForeignKey<Customer>(c => c.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navigation = new Navigation(fk, "Orders", false);
            principalType.AddNavigation(navigation);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Order>().ManyToOne(e => e.Customer, e => e.Orders);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Same(navigation, principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_uses_existing_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder
                .Entity<Order>()
                .ForeignKey<Customer>(c => c.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Order>().ManyToOne(e => e.Customer, e => e.Orders);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_creates_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Order>().ManyToOne(e => e.Customer, e => e.Orders);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            // Passing null as the first arg is not super-compelling, but it is consistent
            modelBuilder.Entity<Order>().ManyToOne<Customer>(null, e => e.Orders);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_from_other_end_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Order>().ManyToOne(e => e.Customer);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_can_create_relationship_with_no_navigations()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Order>().ManyToOne<Customer>();

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_uses_specified_FK_even_if_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .ForeignKey(e => e.CustomerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_uses_existing_FK_not_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder
                .Entity<Pickle>()
                .ForeignKey<BigMak>(c => c.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Pickle>()
                .ManyToOne(e => e.BigMak, e => e.Pickles)
                .ForeignKey(e => e.BurgerId);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_creates_FK_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Pickle>()
                .ManyToOne(e => e.BigMak, e => e.Pickles)
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Pickle>()
                .ManyToOne<BigMak>(null, e => e.Pickles)
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_from_other_end_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Pickle>()
                .ManyToOne(e => e.BigMak)
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_can_create_relationship_with_no_navigations_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Pickle>()
                .ManyToOne<BigMak>()
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_creates_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Pickle>().ManyToOne(e => e.BigMak, e => e.Pickles);

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount + 1, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_nav_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Pickle>().ManyToOne<BigMak>(null, e => e.Pickles);

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount + 1, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_from_other_end_nav_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Pickle>().ManyToOne(e => e.BigMak);

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount + 1, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_can_create_relationship_with_no_navigations_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Pickle>().ManyToOne<BigMak>();

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount + 1, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_matches_shadow_FK_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Pickle>().Property<int>("BigMakId", true);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            var fkProperty = dependentType.GetProperty("BigMakId");

            modelBuilder.Entity<Pickle>().ManyToOne(e => e.BigMak, e => e.Pickles);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_creates_new_FK_if_uniqueness_does_not_match()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder
                .Entity<Pickle>()
                .ForeignKey<BigMak>(c => c.BurgerId)
                .IsUnique();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Pickle>()
                .ManyToOne(e => e.BigMak, e => e.Pickles)
                .ForeignKey(e => e.BurgerId);

            Assert.Equal(2, dependentType.ForeignKeys.Count);
            var newFk = dependentType.ForeignKeys.Single(k => k != fk);

            Assert.True(fk.IsUnique);
            Assert.False(newFk.IsUnique);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(newFk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(newFk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_can_use_explicitly_specified_PK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("Id");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .ReferencedKey(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_can_use_non_PK_principal()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .ReferencedKey(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_can_have_both_convention_properties_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("Id");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .ForeignKey(e => e.CustomerId)
                .ReferencedKey(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_can_have_both_convention_properties_specified_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("Id");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .ReferencedKey(e => e.Id)
                .ForeignKey(e => e.CustomerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_can_have_FK_by_convention_specified_with_explicit_principal_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .ForeignKey(e => e.CustomerId)
                .ReferencedKey(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_can_have_FK_by_convention_specified_with_explicit_principal_key_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Order>().Property(e => e.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .ReferencedKey(e => e.AlternateKey)
                .ForeignKey(e => e.CustomerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_can_have_principal_key_by_convention_specified_with_explicit_PK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Pickle>()
                .ManyToOne(e => e.BigMak, e => e.Pickles)
                .ForeignKey(e => e.BurgerId)
                .ReferencedKey(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_can_have_principal_key_by_convention_specified_with_explicit_PK_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Pickle>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Pickle>()
                .ManyToOne(e => e.BigMak, e => e.Pickles)
                .ReferencedKey(e => e.AlternateKey)
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_finds_existing_navs_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder
                .Entity<CustomerDetails>()
                .ForeignKey<Customer>(c => c.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var navToPrincipal = new Navigation(fk, "Customer", true);
            dependentType.AddNavigation(navToPrincipal);
            var navToDependent = new Navigation(fk, "Details", false);
            principalType.AddNavigation(navToDependent);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Customer>().OneToOne(e => e.Details, e => e.Customer);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Same(navToPrincipal, dependentType.Navigations.Single());
            Assert.Same(navToDependent, principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_finds_existing_nav_to_principal_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder
                .Entity<CustomerDetails>()
                .ForeignKey<Customer>(c => c.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var navigation = new Navigation(fk, "Customer", true);
            dependentType.AddNavigation(navigation);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Customer>().OneToOne(e => e.Details, e => e.Customer);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Same(navigation, dependentType.Navigations.Single());
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_finds_existing_nav_to_dependent_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder
                .Entity<CustomerDetails>()
                .ForeignKey<Customer>(c => c.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var navigation = new Navigation(fk, "Details", false);
            principalType.AddNavigation(navigation);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Customer>().OneToOne(e => e.Details, e => e.Customer);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Same(navigation, principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_uses_existing_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);
            modelBuilder
                .Entity<CustomerDetails>()
                .ForeignKey<Customer>(c => c.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Customer>().OneToOne(e => e.Details, e => e.Customer);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_creates_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Customer>().OneToOne(e => e.Details, e => e.Customer);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_uses_existing_FK_when_not_PK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Order>().Key(c => c.OrderId);
            modelBuilder.Entity<OrderDetails>().Key(e => e.Id);
            modelBuilder
                .Entity<OrderDetails>()
                .ForeignKey<Order>(c => c.Id);

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Order>().OneToOne(e => e.Details, e => e.Order);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_creates_new_FK_when_not_PK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Order>().Key(c => c.OrderId);
            modelBuilder.Entity<OrderDetails>(b =>
                {
                    b.Key(e => e.Id);
                    b.Property(e => e.OrderId);
                });

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Order>().OneToOne(e => e.Details, e => e.Order);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Customer>().OneToOne(e => e.Details);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_from_other_end_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            // Passing null as the first arg is not super-compelling, but it is consistent
            modelBuilder.Entity<Customer>().OneToOne<CustomerDetails>(null, e => e.Customer);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_can_create_relationship_with_no_navigations()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder.Entity<Customer>().OneToOne<CustomerDetails>();

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_uses_specified_FK_even_if_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Order>().Key(c => c.OrderId);
            modelBuilder.Entity<OrderDetails>(b =>
                {
                    b.Key(e => e.Id);
                    b.Property(e => e.OrderId);
                });

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Order>()
                .OneToOne(e => e.Details, e => e.Order)
                .ForeignKey<OrderDetails>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_uses_specified_FK_even_if_PK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Customer>()
                .OneToOne(e => e.Details, e => e.Customer)
                .ForeignKey<CustomerDetails>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_uses_existing_FK_not_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder
                .Entity<Bun>()
                .ForeignKey<BigMak>(c => c.BurgerId);

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<BigMak>()
                .OneToOne(e => e.Bun, e => e.BigMak)
                .ForeignKey<Bun>(e => e.BurgerId);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_creates_FK_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Bun>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<BigMak>()
                .OneToOne(e => e.Bun, e => e.BigMak)
                .ForeignKey<Bun>(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Bun>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<BigMak>()
                .OneToOne(e => e.Bun)
                .ForeignKey<Bun>(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_from_other_end_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Bun>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<BigMak>()
                .OneToOne<Bun>(null, e => e.BigMak)
                .ForeignKey<Bun>(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_can_create_relationship_with_no_navigations_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder.Entity<Bun>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<BigMak>()
                .OneToOne<Bun>()
                .ForeignKey<Bun>(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_makes_new_FK_when_uniqueness_does_not_match()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>().Key(c => c.Id);
            modelBuilder
                .Entity<Bun>()
                .ForeignKey<BigMak>(c => c.BurgerId);

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<BigMak>()
                .OneToOne(e => e.Bun, e => e.BigMak)
                .ForeignKey<Bun>(e => e.BurgerId);

            Assert.Equal(2, dependentType.ForeignKeys.Count);
            var newFk = dependentType.ForeignKeys.Single(k => k != fk);

            Assert.False(fk.IsUnique);
            Assert.True(newFk.IsUnique);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(newFk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(newFk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_principal_and_dependent_can_be_flipped_with_existing_FK_still_used()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Order>().Key(c => c.OrderId);
            modelBuilder.Entity<OrderDetails>().Key(e => e.Id);
            modelBuilder
                .Entity<OrderDetails>()
                .ForeignKey<Order>(c => c.Id);

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<OrderDetails>()
                .OneToOne(e => e.Order, e => e.Details)
                .ForeignKey<OrderDetails>(e => e.Id);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_principal_and_dependent_can_be_flipped_with_FK_still_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Order>().Key(c => c.OrderId);
            modelBuilder.Entity<OrderDetails>(b =>
                {
                    b.Key(e => e.Id);
                    b.Property(e => e.OrderId);
                });

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<OrderDetails>()
                .OneToOne(e => e.Order, e => e.Details)
                .ForeignKey<OrderDetails>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void Unidirectional_from_other_end_OneToOne_principal_and_dependent_can_be_flipped()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<CustomerDetails>()
                .OneToOne<Customer>(e => e.Customer)
                .ForeignKey<CustomerDetails>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void Unidirectional_OneToOne_principal_and_dependent_can_be_flipped()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<CustomerDetails>()
                .OneToOne<Customer>(null, e => e.Details)
                .ForeignKey<CustomerDetails>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void No_navigation_OneToOne_principal_and_dependent_can_be_flipped()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<CustomerDetails>()
                .OneToOne<Customer>()
                .ForeignKey<CustomerDetails>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_principal_and_dependent_can_be_flipped_with_PK_FK_still_used()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<CustomerDetails>()
                .OneToOne(e => e.Customer, e => e.Details)
                .ForeignKey<CustomerDetails>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_can_have_PK_explicitly_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");
            var principalProperty = principalType.GetProperty("Id");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Customer>()
                .OneToOne(e => e.Details, e => e.Customer)
                .ReferencedKey<Customer>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_can_use_alternate_principal_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Customer>()
                .OneToOne(e => e.Details, e => e.Customer)
                .ReferencedKey<Customer>(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_can_have_both_convention_keys_specified_explicitly()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Order>().Key(c => c.OrderId);
            modelBuilder.Entity<OrderDetails>(b =>
                {
                    b.Key(e => e.Id);
                    b.Property(e => e.OrderId);
                });

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");
            var principalProperty = principalType.GetProperty("OrderId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Order>()
                .OneToOne(e => e.Details, e => e.Order)
                .ForeignKey<OrderDetails>(e => e.OrderId)
                .ReferencedKey<Order>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_can_have_both_convention_keys_specified_explicitly_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Order>().Key(c => c.OrderId);
            modelBuilder.Entity<OrderDetails>(b =>
                {
                    b.Key(e => e.Id);
                    b.Property(e => e.OrderId);
                });

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");
            var principalProperty = principalType.GetProperty("OrderId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Order>()
                .OneToOne(e => e.Details, e => e.Order)
                .ReferencedKey<Order>(e => e.OrderId)
                .ForeignKey<OrderDetails>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_can_have_both_keys_specified_explicitly()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Bun>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<BigMak>()
                .OneToOne(e => e.Bun, e => e.BigMak)
                .ForeignKey<Bun>(e => e.BurgerId)
                .ReferencedKey<BigMak>(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_can_have_both_keys_specified_explicitly_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(e => e.AlternateKey);
                });
            modelBuilder.Entity<Bun>().Property(e => e.BurgerId);

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<BigMak>()
                .OneToOne(e => e.Bun, e => e.BigMak)
                .ReferencedKey<BigMak>(e => e.AlternateKey)
                .ForeignKey<Bun>(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_principal_and_dependent_can_be_flipped_using_principal_with_existing_FK_still_used()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Order>().Key(c => c.OrderId);
            modelBuilder.Entity<OrderDetails>().Key(e => e.Id);
            modelBuilder
                .Entity<OrderDetails>()
                .ForeignKey<Order>(c => c.Id);

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<OrderDetails>()
                .OneToOne(e => e.Order, e => e.Details)
                .ReferencedKey<Order>(e => e.OrderId);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_principal_and_dependent_can_be_flipped_using_principal_with_FK_still_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Order>().Key(c => c.OrderId);
            modelBuilder.Entity<OrderDetails>(b =>
                {
                    b.Key(e => e.Id);
                    b.Property(e => e.OrderId);
                });

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<OrderDetails>()
                .OneToOne(e => e.Order, e => e.Details)
                .ReferencedKey<Order>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_principal_and_dependent_can_be_flipped_in_both_ways()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Order>().Key(c => c.OrderId);
            modelBuilder.Entity<OrderDetails>(b =>
                {
                    b.Key(e => e.Id);
                    b.Property(e => e.OrderId);
                });

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<OrderDetails>()
                .OneToOne(e => e.Order, e => e.Details)
                .ForeignKey<OrderDetails>(e => e.OrderId)
                .ReferencedKey<Order>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_principal_and_dependent_can_be_flipped_in_both_ways_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Order>().Key(c => c.OrderId);
            modelBuilder.Entity<OrderDetails>(b =>
                {
                    b.Key(e => e.Id);
                    b.Property(e => e.OrderId);
                });

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<OrderDetails>()
                .OneToOne(e => e.Order, e => e.Details)
                .ReferencedKey<Order>(e => e.OrderId)
                .ForeignKey<OrderDetails>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void Unidirectional_from_other_end_OneToOne_principal_and_dependent_can_be_flipped_using_principal()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<CustomerDetails>()
                .OneToOne<Customer>(e => e.Customer)
                .ReferencedKey<Customer>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void Unidirectional_OneToOne_principal_and_dependent_can_be_flipped_using_principal()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<CustomerDetails>()
                .OneToOne<Customer>(null, e => e.Details)
                .ReferencedKey<Customer>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void No_navigation_OneToOne_principal_and_dependent_can_be_flipped_using_principal()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>().Key(c => c.Id);
            modelBuilder.Entity<CustomerDetails>().Key(e => e.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("Id");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<CustomerDetails>()
                .OneToOne<Customer>()
                .ReferencedKey<Customer>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        private class BigMak
        {
            public int Id { get; set; }
            public int AlternateKey { get; set; }

            public IEnumerable<Pickle> Pickles { get; set; }

            public Bun Bun { get; set; }
        }

        private class Pickle
        {
            public int Id { get; set; }

            public int BurgerId { get; set; }
            public BigMak BigMak { get; set; }
        }

        private class Bun
        {
            public int Id { get; set; }

            public int BurgerId { get; set; }
            public BigMak BigMak { get; set; }
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_uses_existing_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder
                .Entity<Tomato>()
                .ForeignKey<Whoopper>(c => new { c.BurgerId1, c.BurgerId2 });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Whoopper>()
                .OneToMany(e => e.Tomatoes, e => e.Whoopper)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_creates_composite_FK_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Whoopper>()
                .OneToMany(e => e.Tomatoes, e => e.Whoopper)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_can_use_alternate_composite_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>(b =>
                {
                    b.Key(c => new { c.Id1, c.Id2 });
                    b.Property(e => e.AlternateKey1);
                    b.Property(e => e.AlternateKey2);
                });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Whoopper>()
                .OneToMany(e => e.Tomatoes, e => e.Whoopper)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 })
                .ReferencedKey(e => new { e.AlternateKey1, e.AlternateKey2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_can_use_alternate_composite_key_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>(b =>
                {
                    b.Key(c => new { c.Id1, c.Id2 });
                    b.Property(e => e.AlternateKey1);
                    b.Property(e => e.AlternateKey2);
                });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Whoopper>()
                .OneToMany(e => e.Tomatoes, e => e.Whoopper)
                .ReferencedKey(e => new { e.AlternateKey1, e.AlternateKey2 })
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Whoopper>()
                .OneToMany(e => e.Tomatoes)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_from_other_end_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Whoopper>()
                .OneToMany<Tomato>(null, e => e.Whoopper)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToMany_can_create_relationship_with_no_navigations_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Whoopper>()
                .OneToMany<Tomato>()
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_uses_existing_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder
                .Entity<Tomato>()
                .ForeignKey<Whoopper>(c => new { c.BurgerId1, c.BurgerId2 });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Tomato>()
                .ManyToOne(e => e.Whoopper, e => e.Tomatoes)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_creates_composite_FK_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Tomato>()
                .ManyToOne(e => e.Whoopper, e => e.Tomatoes)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_can_use_alternate_composite_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>(b =>
                {
                    b.Key(c => new { c.Id1, c.Id2 });
                    b.Property(e => e.AlternateKey1);
                    b.Property(e => e.AlternateKey2);
                });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Tomato>()
                .ManyToOne(e => e.Whoopper, e => e.Tomatoes)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 })
                .ReferencedKey(e => new { e.AlternateKey1, e.AlternateKey2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_can_use_alternate_composite_key_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>(b =>
                {
                    b.Key(c => new { c.Id1, c.Id2 });
                    b.Property(e => e.AlternateKey1);
                    b.Property(e => e.AlternateKey2);
                });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Tomato>()
                .ManyToOne(e => e.Whoopper, e => e.Tomatoes)
                .ReferencedKey(e => new { e.AlternateKey1, e.AlternateKey2 })
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Tomato>()
                .ManyToOne<Whoopper>(null, e => e.Tomatoes)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_from_other_end_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Tomato>()
                .ManyToOne(e => e.Whoopper)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void ManyToOne_can_create_relationship_with_no_navigations_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Tomato>()
                .ManyToOne<Whoopper>()
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_uses_existing_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder
                .Entity<ToastedBun>()
                .ForeignKey<Whoopper>(c => new { c.BurgerId1, c.BurgerId2 });

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Whoopper>()
                .OneToOne(e => e.ToastedBun, e => e.Whoopper)
                .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_creates_composite_FK_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<ToastedBun>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Whoopper>()
                .OneToOne(e => e.ToastedBun, e => e.Whoopper)
                .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_can_use_alternate_composite_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>(b =>
                {
                    b.Key(c => new { c.Id1, c.Id2 });
                    b.Property(e => e.AlternateKey1);
                    b.Property(e => e.AlternateKey2);
                });
            modelBuilder.Entity<ToastedBun>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Whoopper>()
                .OneToOne(e => e.ToastedBun, e => e.Whoopper)
                .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 })
                .ReferencedKey<Whoopper>(e => new { e.AlternateKey1, e.AlternateKey2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_can_use_alternate_composite_key_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>(b =>
                {
                    b.Key(c => new { c.Id1, c.Id2 });
                    b.Property(e => e.AlternateKey1);
                    b.Property(e => e.AlternateKey2);
                });
            modelBuilder.Entity<ToastedBun>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Whoopper>()
                .OneToOne(e => e.ToastedBun, e => e.Whoopper)
                .ReferencedKey<Whoopper>(e => new { e.AlternateKey1, e.AlternateKey2 })
                .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_uses_composite_PK_for_FK_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Moostard>().Key(c => new { c.Id1, c.Id2 });

            var dependentType = model.GetEntityType(typeof(Moostard));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("Id1");
            var fkProperty2 = dependentType.GetProperty("Id2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Whoopper>()
                .OneToOne(e => e.Moostard, e => e.Whoopper);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Moostard", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_can_be_flipped_and_composite_PK_is_still_used_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Moostard>().Key(c => new { c.Id1, c.Id2 });

            var dependentType = model.GetEntityType(typeof(Moostard));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("Id1");
            var fkProperty2 = dependentType.GetProperty("Id2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Moostard>()
                .OneToOne(e => e.Whoopper, e => e.Moostard)
                .ForeignKey<Moostard>(e => new { e.Id1, e.Id2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Moostard", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_can_be_flipped_using_principal_and_composite_PK_is_still_used_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Moostard>().Key(c => new { c.Id1, c.Id2 });

            var dependentType = model.GetEntityType(typeof(Moostard));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("Id1");
            var fkProperty2 = dependentType.GetProperty("Id2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Moostard>()
                .OneToOne(e => e.Whoopper, e => e.Moostard)
                .ReferencedKey<Whoopper>(e => new { e.Id1, e.Id2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Moostard", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<ToastedBun>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Whoopper>()
                .OneToOne(e => e.ToastedBun)
                .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_from_other_end_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<ToastedBun>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Whoopper>()
                .OneToOne<ToastedBun>(null, e => e.Whoopper)
                .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void OneToOne_can_create_relationship_with_no_navigations_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<ToastedBun>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;

            modelBuilder
                .Entity<Whoopper>()
                .OneToOne<ToastedBun>()
                .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
        }

        [Fact]
        public void Can_convert_to_non_convention_builder()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            Assert.Same(model, ((BasicModelBuilder)modelBuilder).Model);
        }

        private class Whoopper
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }
            public int AlternateKey1 { get; set; }
            public int AlternateKey2 { get; set; }

            public IEnumerable<Tomato> Tomatoes { get; set; }

            public ToastedBun ToastedBun { get; set; }

            public Moostard Moostard { get; set; }
        }

        private class Tomato
        {
            public int Id { get; set; }

            public int BurgerId1 { get; set; }
            public int BurgerId2 { get; set; }
            public Whoopper Whoopper { get; set; }
        }

        private class ToastedBun
        {
            public int Id { get; set; }

            public int BurgerId1 { get; set; }
            public int BurgerId2 { get; set; }
            public Whoopper Whoopper { get; set; }
        }

        private class Moostard
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }

            public Whoopper Whoopper { get; set; }
        }

        private class Customer
        {
            public int Id { get; set; }
            public int AlternateKey { get; set; }
            public string Name { get; set; }

            public IEnumerable<Order> Orders { get; set; }

            public CustomerDetails Details { get; set; }
        }

        private class CustomerDetails
        {
            public int Id { get; set; }

            public Customer Customer { get; set; }
        }

        private class Order
        {
            public int OrderId { get; set; }

            public int CustomerId { get; set; }
            public Customer Customer { get; set; }

            public OrderDetails Details { get; set; }
        }

        private class OrderDetails
        {
            public int Id { get; set; }

            public int OrderId { get; set; }
            public Order Order { get; set; }
        }
    }
}
