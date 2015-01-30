// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class BasicModelBuilderTest
    {
        [Fact]
        public void Can_get_entity_builder_for_clr_type()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            var entityBuilder = modelBuilder.Entity<Customer>();

            Assert.NotNull(entityBuilder);
            Assert.Equal(typeof(Customer).FullName, model.GetEntityType(typeof(Customer)).Name);
        }

        [Fact]
        public void Can_get_entity_builder_for_clr_type_non_generic()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            var entityBuilder = modelBuilder.Entity(typeof(Customer));

            Assert.NotNull(entityBuilder);
            Assert.Equal(typeof(Customer).FullName, model.GetEntityType(typeof(Customer)).Name);
        }

        [Fact]
        public void Can_get_entity_builder_for_entity_type_name()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            var entityBuilder = modelBuilder.Entity(typeof(Customer).FullName);

            Assert.NotNull(entityBuilder);
            Assert.NotNull(model.TryGetEntityType(typeof(Customer).FullName));
        }

        [Fact]
        public void Can_set_entity_key_from_clr_property()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<Customer>().Key(e => e.Id);

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entity.GetPrimaryKey().Properties.Count());
            Assert.Equal("Id", entity.GetPrimaryKey().Properties.First().Name);
        }

        [Fact]
        public void Can_set_entity_key_from_CLR_property_non_generic()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity(typeof(Customer), b =>
                {
                    b.Property<int>("Id");
                    b.Key("Id");
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entity.GetPrimaryKey().Properties.Count());
            Assert.Equal("Id", entity.GetPrimaryKey().Properties.First().Name);
        }

        [Fact]
        public void Can_set_entity_key_from_property_name()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<Customer>(b =>
                {
                    b.Property(e => e.Id);
                    b.Key("Id");
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entity.GetPrimaryKey().Properties.Count());
            Assert.Equal("Id", entity.GetPrimaryKey().Properties.First().Name);
        }

        [Fact]
        public void Can_set_entity_key_from_property_name_when_no_clr_property()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<Customer>(b =>
                {
                    b.Property<int>("Id");
                    b.Key("Id");
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entity.GetPrimaryKey().Properties.Count());
            Assert.Equal("Id", entity.GetPrimaryKey().Properties.First().Name);
        }

        [Fact]
        public void Can_set_entity_key_from_property_name_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity(typeof(Customer).FullName, b =>
                {
                    b.Property<int>("Id");
                    b.Key("Id");
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entity.GetPrimaryKey().Properties.Count());
            Assert.Equal("Id", entity.GetPrimaryKey().Properties.First().Name);
        }

        [Fact]
        public void Can_set_composite_entity_key_from_clr_properties()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Key(e => new { e.Id, e.Name });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetPrimaryKey().Properties.Count());
            Assert.Equal("Id", entity.GetPrimaryKey().Properties.First().Name);
            Assert.Equal("Name", entity.GetPrimaryKey().Properties.Last().Name);
        }

        [Fact]
        public void Can_set_composite_entity_key_from_property_names()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<Customer>(b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.Name);
                    b.Key("Id", "Name");
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetPrimaryKey().Properties.Count());
            Assert.Equal("Id", entity.GetPrimaryKey().Properties.First().Name);
            Assert.Equal("Name", entity.GetPrimaryKey().Properties.Last().Name);
        }

        [Fact]
        public void Can_set_composite_entity_key_from_property_names_when_mixed_properties()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<Customer>(b =>
                {
                    b.Property(e => e.Id);
                    b.Property<string>("Name");
                    b.Key("Id", "Name");
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetPrimaryKey().Properties.Count());
            Assert.Equal("Id", entity.GetPrimaryKey().Properties.First().Name);
            Assert.Equal("Name", entity.GetPrimaryKey().Properties.Last().Name);
        }

        [Fact]
        public void Can_set_composite_entity_key_from_property_names_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity(typeof(Customer).FullName, ps =>
                {
                    ps.Property<int>("Id");
                    ps.Property<string>("Name");
                    ps.Key("Id", "Name");
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetPrimaryKey().Properties.Count());
            Assert.Equal("Id", entity.GetPrimaryKey().Properties.First().Name);
            Assert.Equal("Name", entity.GetPrimaryKey().Properties.Last().Name);
        }

        [Fact]
        public void Can_set_entity_key_with_annotations()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder
                .Entity<Customer>(
                    b => b.Key(e => new { e.Id, e.Name })
                        .Annotation("A1", "V1")
                        .Annotation("A2", "V2"));

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetPrimaryKey().Properties.Count());
            Assert.Equal(new[] { "Id", "Name" }, entity.GetPrimaryKey().Properties.Select(p => p.Name));
            Assert.Equal("V1", entity.GetPrimaryKey()["A1"]);
            Assert.Equal("V2", entity.GetPrimaryKey()["A2"]);
        }

        [Fact]
        public void Can_set_entity_key_with_annotations_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity(typeof(Customer).FullName, b =>
                {
                    b.Property<int>("Id");
                    b.Property<string>("Name");
                    b.Key("Id", "Name")
                        .Annotation("A1", "V1")
                        .Annotation("A2", "V2");
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetPrimaryKey().Properties.Count());
            Assert.Equal(new[] { "Id", "Name" }, entity.GetPrimaryKey().Properties.Select(p => p.Name));
            Assert.Equal("V1", entity.GetPrimaryKey()["A1"]);
            Assert.Equal("V2", entity.GetPrimaryKey()["A2"]);
        }

        [Fact]
        public void Can_set_entity_annotation()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Annotation("foo", "bar");

            Assert.Equal("bar", model.GetEntityType(typeof(Customer))["foo"]);
        }

        [Fact]
        public void Can_set_entity_annotation_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder
                .Entity(typeof(Customer).FullName)
                .Annotation("foo", "bar");

            Assert.Equal("bar", model.GetEntityType(typeof(Customer))["foo"]);
        }

        [Fact]
        public void Can_set_property_annotation()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Property(c => c.Name).Annotation("foo", "bar");

            Assert.Equal("bar", model.GetEntityType(typeof(Customer)).GetProperty("Name")["foo"]);
        }

        [Fact]
        public void Can_set_property_annotation_when_no_clr_property()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Property<string>("Name").Annotation("foo", "bar");

            Assert.Equal("bar", model.GetEntityType(typeof(Customer)).GetProperty("Name")["foo"]);
        }

        [Fact]
        public void Can_set_property_annotation_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder
                .Entity(typeof(Customer).FullName)
                .Property<string>("Name").Annotation("foo", "bar");

            Assert.Equal("bar", model.GetEntityType(typeof(Customer)).GetProperty("Name")["foo"]);
        }

        [Fact]
        public void Can_add_multiple_properties()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

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
            var modelBuilder = new BasicModelBuilder(model);

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
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity(typeof(Customer).FullName, b =>
                {
                    b.Property<int>("Id");
                    b.Property<string>("Name").Annotation("foo", "bar");
                });

            Assert.Equal(2, model.GetEntityType(typeof(Customer)).Properties.Count());
        }

        [Fact]
        public void Properties_are_required_by_default_only_if_CLR_type_is_nullable()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Up);
                    b.Property(e => e.Down);
                    b.Property<int>("Charm");
                    b.Property<string>("Strange");
                    b.Property(typeof(int), "Top");
                    b.Property(typeof(string), "Bottom");
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

            Assert.False(entityType.GetProperty("Up").IsNullable);
            Assert.True(entityType.GetProperty("Down").IsNullable);
            Assert.False(entityType.GetProperty("Charm").IsNullable);
            Assert.True(entityType.GetProperty("Strange").IsNullable);
            Assert.False(entityType.GetProperty("Top").IsNullable);
            Assert.True(entityType.GetProperty("Bottom").IsNullable);
        }

        [Fact]
        public void Properties_can_be_made_required()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Up).Required();
                    b.Property(e => e.Down).Required();
                    b.Property<int>("Charm").Required();
                    b.Property<string>("Strange").Required();
                    b.Property(typeof(int), "Top").Required();
                    b.Property(typeof(string), "Bottom").Required();
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

            Assert.False(entityType.GetProperty("Up").IsNullable);
            Assert.False(entityType.GetProperty("Down").IsNullable);
            Assert.False(entityType.GetProperty("Charm").IsNullable);
            Assert.False(entityType.GetProperty("Strange").IsNullable);
            Assert.False(entityType.GetProperty("Top").IsNullable);
            Assert.False(entityType.GetProperty("Bottom").IsNullable);
        }

        [Fact]
        public void Properties_can_be_made_optional()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Down).Required(false);
                    b.Property<string>("Strange").Required(false);
                    b.Property(typeof(string), "Bottom").Required(false);
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

            Assert.True(entityType.GetProperty("Down").IsNullable);
            Assert.True(entityType.GetProperty("Strange").IsNullable);
            Assert.True(entityType.GetProperty("Bottom").IsNullable);
        }

        [Fact]
        public void Non_nullable_properties_cannot_be_made_optional()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    Assert.Equal(
                        Strings.CannotBeNullable("Up", "Quarks", "Int32"),
                        Assert.Throws<InvalidOperationException>(() => b.Property(e => e.Up).Required(false)).Message);

                    Assert.Equal(
                        Strings.CannotBeNullable("Charm", "Quarks", "Int32"),
                        Assert.Throws<InvalidOperationException>(() => b.Property<int>("Charm").Required(false)).Message);

                    Assert.Equal(
                        Strings.CannotBeNullable("Top", "Quarks", "Int32"),
                        Assert.Throws<InvalidOperationException>(() => b.Property(typeof(int), "Top").Required(false)).Message);
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

            Assert.False(entityType.GetProperty("Up").IsNullable);
            Assert.False(entityType.GetProperty("Charm").IsNullable);
            Assert.False(entityType.GetProperty("Top").IsNullable);
        }

        [Fact]
        public void Properties_specified_by_string_are_shadow_properties_unless_already_defined_as_CLR_properties()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Up);
                    b.Property<int>("Charm");
                    b.Property(typeof(int), "Top");
                    b.Property<int>("Up");
                    b.Property<string>("Gluon");
                    b.Property(typeof(string), "Photon");
                });

            var entityType = model.GetEntityType(typeof(Quarks));

            Assert.False(entityType.GetProperty("Up").IsShadowProperty);
            Assert.True(entityType.GetProperty("Charm").IsShadowProperty);
            Assert.True(entityType.GetProperty("Top").IsShadowProperty);
            Assert.True(entityType.GetProperty("Gluon").IsShadowProperty);
            Assert.True(entityType.GetProperty("Photon").IsShadowProperty);

            Assert.Equal(-1, entityType.GetProperty("Up").ShadowIndex);
            Assert.Equal(0, entityType.GetProperty("Charm").ShadowIndex);
            Assert.Equal(3, entityType.GetProperty("Top").ShadowIndex);
            Assert.Equal(1, entityType.GetProperty("Gluon").ShadowIndex);
            Assert.Equal(2, entityType.GetProperty("Photon").ShadowIndex);
        }

        [Fact]
        public void Properties_can_be_made_shadow_properties_or_vice_versa()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Up).Shadow();
                    b.Property<int>("Charm").Shadow(false);
                    b.Property(typeof(int), "Top").Shadow(false);
                    b.Property<string>("Gluon").Shadow();
                    b.Property(typeof(string), "Photon").Shadow();
                });

            var entityType = model.GetEntityType(typeof(Quarks));

            Assert.True(entityType.GetProperty("Up").IsShadowProperty);
            Assert.False(entityType.GetProperty("Charm").IsShadowProperty);
            Assert.False(entityType.GetProperty("Top").IsShadowProperty);
            Assert.True(entityType.GetProperty("Gluon").IsShadowProperty);
            Assert.True(entityType.GetProperty("Photon").IsShadowProperty);

            Assert.Equal(2, entityType.GetProperty("Up").ShadowIndex);
            Assert.Equal(-1, entityType.GetProperty("Charm").ShadowIndex);
            Assert.Equal(-1, entityType.GetProperty("Top").ShadowIndex);
            Assert.Equal(0, entityType.GetProperty("Gluon").ShadowIndex);
            Assert.Equal(1, entityType.GetProperty("Photon").ShadowIndex);
        }

        [Fact]
        public void Properties_can_be_made_concurency_tokens()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.Up).ConcurrencyToken();
                    b.Property(e => e.Down).ConcurrencyToken(false);
                    b.Property<int>("Charm").ConcurrencyToken(true);
                    b.Property<string>("Strange").ConcurrencyToken(false);
                    b.Property(typeof(int), "Top").ConcurrencyToken();
                    b.Property(typeof(string), "Bottom").ConcurrencyToken(false);
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

            Assert.False(entityType.GetProperty("Id").IsConcurrencyToken);
            Assert.True(entityType.GetProperty("Up").IsConcurrencyToken);
            Assert.False(entityType.GetProperty("Down").IsConcurrencyToken);
            Assert.True(entityType.GetProperty("Charm").IsConcurrencyToken);
            Assert.False(entityType.GetProperty("Strange").IsConcurrencyToken);
            Assert.True(entityType.GetProperty("Top").IsConcurrencyToken);
            Assert.False(entityType.GetProperty("Bottom").IsConcurrencyToken);

            Assert.Equal(-1, entityType.GetProperty("Id").OriginalValueIndex);
            Assert.Equal(2, entityType.GetProperty("Up").OriginalValueIndex);
            Assert.Equal(-1, entityType.GetProperty("Down").OriginalValueIndex);
            Assert.Equal(0, entityType.GetProperty("Charm").OriginalValueIndex);
            Assert.Equal(-1, entityType.GetProperty("Strange").OriginalValueIndex);
            Assert.Equal(1, entityType.GetProperty("Top").OriginalValueIndex);
            Assert.Equal(-1, entityType.GetProperty("Bottom").OriginalValueIndex);
        }

        [Fact]
        public void Properties_can_be_set_to_generate_values_on_Add()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Id).GenerateValueOnAdd(false);
                    b.Property(e => e.Up).GenerateValueOnAdd();
                    b.Property(e => e.Down).GenerateValueOnAdd(true);
                    b.Property<int>("Charm").GenerateValueOnAdd();
                    b.Property<string>("Strange").GenerateValueOnAdd(false);
                    b.Property(typeof(int), "Top").GenerateValueOnAdd();
                    b.Property(typeof(string), "Bottom").GenerateValueOnAdd(false);
                });

            var entityType = model.GetEntityType(typeof(Quarks));

            Assert.Equal(false, entityType.GetProperty("Id").GenerateValueOnAdd);
            Assert.Equal(true, entityType.GetProperty("Up").GenerateValueOnAdd);
            Assert.Equal(true, entityType.GetProperty("Down").GenerateValueOnAdd);
            Assert.Equal(true, entityType.GetProperty("Charm").GenerateValueOnAdd);
            Assert.Equal(false, entityType.GetProperty("Strange").GenerateValueOnAdd);
            Assert.Equal(true, entityType.GetProperty("Top").GenerateValueOnAdd);
            Assert.Equal(false, entityType.GetProperty("Bottom").GenerateValueOnAdd);
        }

        [Fact]
        public void Properties_can_be_set_to_be_store_computed()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.Up).StoreComputed();
                    b.Property(e => e.Down).StoreComputed(false);
                    b.Property<int>("Charm").StoreComputed();
                    b.Property<string>("Strange").StoreComputed(false);
                    b.Property(typeof(int), "Top").StoreComputed();
                    b.Property(typeof(string), "Bottom").StoreComputed(false);
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

            Assert.False(entityType.GetProperty("Id").IsStoreComputed);
            Assert.True(entityType.GetProperty("Up").IsStoreComputed);
            Assert.False(entityType.GetProperty("Down").IsStoreComputed);
            Assert.True(entityType.GetProperty("Charm").IsStoreComputed);
            Assert.False(entityType.GetProperty("Strange").IsStoreComputed);
            Assert.True(entityType.GetProperty("Top").IsStoreComputed);
            Assert.False(entityType.GetProperty("Bottom").IsStoreComputed);
        }

        [Fact]
        public void Properties_can_be_set_to_use_store_default_values()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.Up).UseStoreDefault();
                    b.Property(e => e.Down).UseStoreDefault(false);
                    b.Property<int>("Charm").UseStoreDefault();
                    b.Property<string>("Strange").UseStoreDefault(false);
                    b.Property(typeof(int), "Top").UseStoreDefault();
                    b.Property(typeof(string), "Bottom").UseStoreDefault(false);
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

            Assert.False(entityType.GetProperty("Id").UseStoreDefault);
            Assert.True(entityType.GetProperty("Up").UseStoreDefault);
            Assert.False(entityType.GetProperty("Down").UseStoreDefault);
            Assert.True(entityType.GetProperty("Charm").UseStoreDefault);
            Assert.False(entityType.GetProperty("Strange").UseStoreDefault);
            Assert.True(entityType.GetProperty("Top").UseStoreDefault);
            Assert.False(entityType.GetProperty("Bottom").UseStoreDefault);
        }

        [Fact]
        public void Can_set_max_length_for_properties()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Id);
                    b.Property(e => e.Up).MaxLength(0);
                    b.Property(e => e.Down).MaxLength(100);
                    b.Property<int>("Charm").MaxLength(0);
                    b.Property<string>("Strange").MaxLength(100);
                    b.Property(typeof(int), "Top").MaxLength(0);
                    b.Property(typeof(string), "Bottom").MaxLength(100);
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

            Assert.Equal(0, entityType.GetProperty("Id").MaxLength);
            Assert.Equal(0, entityType.GetProperty("Up").MaxLength);
            Assert.Equal(100, entityType.GetProperty("Down").MaxLength);
            Assert.Equal(0, entityType.GetProperty("Charm").MaxLength);
            Assert.Equal(100, entityType.GetProperty("Strange").MaxLength);
            Assert.Equal(0, entityType.GetProperty("Top").MaxLength);
            Assert.Equal(100, entityType.GetProperty("Bottom").MaxLength);
        }

        [Fact]
        public void PropertyBuilder_methods_can_be_chained()
        {
            new BasicModelBuilder()
                .Entity<Quarks>()
                .Property(e => e.Up)
                .Required()
                .Annotation("A", "V")
                .ConcurrencyToken()
                .Shadow()
                .StoreComputed()
                .GenerateValueOnAdd()
                .UseStoreDefault()
                .MaxLength(100)
                .Required();
        }

        [Fact]
        public void Can_add_foreign_key()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);
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
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Key(c => c.Id);

            modelBuilder.Entity<Order>(b =>
                {
                    b.Property<int>("CustomerId");
                    b.ForeignKey(typeof(Customer).FullName, new[] { "CustomerId" });
                });

            var entityType = model.GetEntityType(typeof(Order));

            Assert.Equal(1, entityType.ForeignKeys.Count());
        }

        [Fact]
        public void Can_add_foreign_key_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Key(c => c.Id);

            modelBuilder.Entity(typeof(Order).FullName, b =>
                {
                    b.Property<int>("CustomerId");
                    b.ForeignKey(typeof(Customer).FullName, "CustomerId");
                });

            var entityType = model.GetEntityType(typeof(Order));

            Assert.Equal(1, entityType.ForeignKeys.Count());
        }

        [Fact]
        public void Can_add_foreign_key_when_no_clr_type_on_both_ends()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity(typeof(Customer).FullName, b =>
                {
                    b.Property<int>("Id");
                    b.Key(new[] { "Id" });
                });

            modelBuilder.Entity(typeof(Order).FullName, b =>
                {
                    b.Property<int>("CustomerId");
                    b.ForeignKey(typeof(Customer).FullName, "CustomerId");
                });

            var entityType = model.GetEntityType(typeof(Order));

            Assert.Equal(1, entityType.ForeignKeys.Count());
        }

        [Fact]
        public void Can_add_multiple_foreign_keys()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<Customer>().Key(c => c.Id);

            modelBuilder.Entity<Order>(b =>
                {
                    b.ForeignKey<Customer>(c => c.CustomerId);
                    b.ForeignKey<Customer>(c => c.AnotherCustomerId).IsUnique();
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Order));

            Assert.Equal(2, entityType.ForeignKeys.Count());
            Assert.Equal(1, entityType.ForeignKeys.Count(fk => fk.IsUnique));
        }

        [Fact]
        public void Can_add_multiple_foreign_keys_when_mixed_properties()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<Customer>().Key(c => c.Id);

            modelBuilder.Entity<Order>(b =>
                {
                    b.Property<int>("CustomerId");
                    b.Property<int>("AnotherCustomerId");
                    b.ForeignKey<Customer>(c => c.CustomerId);
                    b.ForeignKey(typeof(Customer).FullName, "AnotherCustomerId").IsUnique();
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Order));

            Assert.Equal(2, entityType.ForeignKeys.Count());
            Assert.Equal(1, entityType.ForeignKeys.Count(fk => fk.IsUnique));
        }

        [Fact]
        public void Can_add_multiple_foreign_keys_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<Customer>().Key(c => c.Id);

            modelBuilder
                .Entity(typeof(Order).FullName, b =>
                    {
                        b.Property<int>("CustomerId");
                        b.Property<int>("AnotherCustomerId");
                        b.ForeignKey(typeof(Customer).FullName, "CustomerId");
                        b.ForeignKey(typeof(Customer).FullName, "AnotherCustomerId").IsUnique();
                    });

            var entityType = (IEntityType)model.GetEntityType(typeof(Order));

            Assert.Equal(2, entityType.ForeignKeys.Count());
            Assert.Equal(1, entityType.ForeignKeys.Count(fk => fk.IsUnique));
        }

        [Fact]
        public void Can_add_multiple_foreign_keys_when_no_clr_type_on_both_ends()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity(typeof(Customer).FullName, b =>
                {
                    b.Property<int>("Id");
                    b.Key("Id");
                });

            modelBuilder.Entity(typeof(Order).FullName, b =>
                {
                    b.Property<int>("CustomerId");
                    b.Property<int>("AnotherCustomerId");
                    b.ForeignKey(typeof(Customer).FullName, "CustomerId");
                    b.ForeignKey(typeof(Customer).FullName, "AnotherCustomerId").IsUnique();
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Order));

            Assert.Equal(2, entityType.ForeignKeys.Count());
            Assert.Equal(1, entityType.ForeignKeys.Count(fk => fk.IsUnique));
        }

        [Fact]
        public void Can_add_index()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

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
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity(typeof(Customer).FullName, b =>
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
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<Customer>(b =>
                {
                    b.Index(ix => ix.Id).IsUnique();
                    b.Index(ix => ix.Name).Annotation("A1", "V1");
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entityType.Indexes.Count());
            Assert.True(entityType.Indexes.First().IsUnique);
            Assert.False(entityType.Indexes.Last().IsUnique);
            Assert.Equal("V1", entityType.Indexes.Last()["A1"]);
        }

        [Fact]
        public void Can_add_multiple_indexes_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity(typeof(Customer).FullName, b =>
                {
                    b.Property<int>("Id");
                    b.Property<string>("Name");
                    b.Index("Id").IsUnique();
                    b.Index("Name").Annotation("A1", "V1");
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entityType.Indexes.Count());
            Assert.True(entityType.Indexes.First().IsUnique);
            Assert.False(entityType.Indexes.Last().IsUnique);
            Assert.Equal("V1", entityType.Indexes.Last()["A1"]);
        }

        [Fact]
        public void Can_convert_to_convention_builder()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            Assert.Same(model, new ModelBuilder(modelBuilder.Model).Model);
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
            public int AnotherCustomerId { get; set; }
            public Customer Customer { get; set; }

            public OrderDetails Details { get; set; }
        }

        private class OrderDetails
        {
            public int Id { get; set; }

            public int OrderId { get; set; }
            public Order Order { get; set; }
        }

        // INotify interfaces not really implemented; just marking the classes to test metadata construction
        private class Quarks : INotifyPropertyChanging, INotifyPropertyChanged
        {
            public int Id { get; set; }

            public int Up { get; set; }
            public string Down { get; set; }
            public int Charm { get; set; }
            public string Strange { get; set; }
            public int Top { get; set; }
            public string Bottom { get; set; }

#pragma warning disable 67
            public event PropertyChangingEventHandler PropertyChanging;
            public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
        }
    }
}
