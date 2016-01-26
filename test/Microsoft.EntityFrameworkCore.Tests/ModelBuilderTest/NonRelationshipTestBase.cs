// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.Tests
{
    public abstract partial class ModelBuilderTest
    {
        public abstract class NonRelationshipTestBase : ModelBuilderTestBase
        {
            [Fact]
            public virtual void Can_get_entity_builder_for_clr_type()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                var entityBuilder = modelBuilder.Entity<Customer>();

                Assert.NotNull(entityBuilder);
                Assert.Equal(typeof(Customer).FullName, model.FindEntityType(typeof(Customer)).Name);
            }

            [Fact]
            public virtual void Can_set_entity_key_from_clr_property()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Customer>().HasKey(b => b.Id);

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Equal(1, entity.FindPrimaryKey().Properties.Count);
                Assert.Equal(Customer.IdProperty.Name, entity.FindPrimaryKey().Properties.First().Name);
            }

            [Fact]
            public virtual void Can_set_entity_key_from_property_name_when_no_clr_property()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Customer>(b =>
                    {
                        b.Property<int>(Customer.IdProperty.Name + 1);
                        b.Ignore(p => p.Details);
                        b.Ignore(p => p.Orders);
                        b.HasKey(Customer.IdProperty.Name + 1);
                    });

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Equal(1, entity.FindPrimaryKey().Properties.Count);
                Assert.Equal(Customer.IdProperty.Name + 1, entity.FindPrimaryKey().Properties.First().Name);
            }

            [Fact]
            public virtual void Can_set_entity_key_from_clr_property_when_property_ignored()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>(b =>
                    {
                        b.Ignore(Customer.IdProperty.Name);
                        b.HasKey(e => e.Id);
                    });

                var entity = modelBuilder.Model.FindEntityType(typeof(Customer));

                Assert.Equal(1, entity.FindPrimaryKey().Properties.Count);
                Assert.Equal(Customer.IdProperty.Name, entity.FindPrimaryKey().Properties.First().Name);
            }

            [Fact]
            public virtual void Can_set_composite_entity_key_from_clr_properties()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder
                    .Entity<Customer>()
                    .HasKey(e => new { e.Id, e.Name });

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Equal(2, entity.FindPrimaryKey().Properties.Count);
                Assert.Equal(Customer.IdProperty.Name, entity.FindPrimaryKey().Properties.First().Name);
                Assert.Equal(Customer.NameProperty.Name, entity.FindPrimaryKey().Properties.Last().Name);
            }

            [Fact]
            public virtual void Can_set_composite_entity_key_from_property_names_when_mixed_properties()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Ignore<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                modelBuilder.Entity<Customer>(b =>
                    {
                        b.Property<string>(Customer.NameProperty.Name + "Shadow");
                        b.HasKey(Customer.IdProperty.Name, Customer.NameProperty.Name + "Shadow");
                    });

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Equal(2, entity.FindPrimaryKey().Properties.Count);
                Assert.Equal(Customer.IdProperty.Name, entity.FindPrimaryKey().Properties.First().Name);
                Assert.Equal(Customer.NameProperty.Name + "Shadow", entity.FindPrimaryKey().Properties.Last().Name);
            }

            [Fact]
            public virtual void Can_set_entity_key_with_annotations()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                var keyBuilder = modelBuilder
                    .Entity<Customer>()
                    .HasKey(e => new { e.Id, e.Name });

                keyBuilder.HasAnnotation("A1", "V1")
                    .HasAnnotation("A2", "V2");

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Equal(new[] { Customer.IdProperty.Name, Customer.NameProperty.Name }, entity.FindPrimaryKey().Properties.Select(p => p.Name));
                Assert.Equal("V1", keyBuilder.Metadata["A1"]);
                Assert.Equal("V2", keyBuilder.Metadata["A2"]);
            }

            [Fact]
            public virtual void Can_upgrade_candidate_key_to_primary_key()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<Customer>().Property<int>(Customer.IdProperty.Name);
                modelBuilder.Entity<Customer>().HasAlternateKey(b => b.Name);

                var entity = modelBuilder.Model.FindEntityType(typeof(Customer));
                var key = entity.FindKey(entity.FindProperty(Customer.NameProperty));

                modelBuilder.Entity<Customer>().HasKey(b => b.Name);

                Assert.Same(key, entity.GetKeys().Single());
                Assert.Equal(Customer.NameProperty.Name, entity.FindPrimaryKey().Properties.Single().Name);

                var idProperty = (IProperty)entity.FindProperty(Customer.IdProperty);
                Assert.False(idProperty.RequiresValueGenerator);
                Assert.Equal(ValueGenerated.Never, idProperty.ValueGenerated);
            }

            [Fact]
            public virtual void Can_set_alternate_key_from_clr_property()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Customer>().HasAlternateKey(b => b.AlternateKey);

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Equal(1, entity.GetKeys().Count(key => key != entity.FindPrimaryKey()));
                Assert.Equal(Customer.AlternateKeyProperty.Name, entity.GetKeys().First(key => key != entity.FindPrimaryKey()).Properties.First().Name);
            }

            [Fact]
            public virtual void Can_set_alternate_key_from_property_name_when_no_clr_property()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Customer>(b =>
                    {
                        b.Property<int>(Customer.AlternateKeyProperty.Name + 1);
                        b.HasAlternateKey(Customer.AlternateKeyProperty.Name + 1);
                    });

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Equal(1, entity.GetKeys().Count(key => key != entity.FindPrimaryKey()));
                Assert.Equal(Customer.AlternateKeyProperty.Name + 1, entity.GetKeys().First(key => key != entity.FindPrimaryKey()).Properties.First().Name);
            }

            [Fact]
            public virtual void Can_set_alternate_key_from_clr_property_when_property_ignored()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<Customer>(b =>
                    {
                        b.Ignore(Customer.AlternateKeyProperty.Name);
                        b.HasAlternateKey(e => e.AlternateKey);
                    });

                var entity = modelBuilder.Model.FindEntityType(typeof(Customer));

                Assert.Equal(1, entity.GetKeys().Count(key => key != entity.FindPrimaryKey()));
                Assert.Equal(Customer.AlternateKeyProperty.Name, entity.GetKeys().First(key => key != entity.FindPrimaryKey()).Properties.First().Name);
            }

            [Fact]
            public virtual void Setting_alternate_key_makes_properties_required()
            {
                var modelBuilder = CreateModelBuilder();
                var entityBuilder = modelBuilder.Entity<Customer>();

                var entity = modelBuilder.Model.FindEntityType(typeof(Customer));
                var alternateKeyProperty = entity.FindProperty(nameof(Customer.Name));
                Assert.True(alternateKeyProperty.IsNullable);

                entityBuilder.HasAlternateKey(e => e.Name);

                Assert.False(alternateKeyProperty.IsNullable);
            }

            [Fact]
            public virtual void Can_set_entity_annotation()
            {
                var modelBuilder = CreateModelBuilder();

                var entityBuilder = modelBuilder
                    .Entity<Customer>()
                    .HasAnnotation("foo", "bar");

                Assert.Equal("bar", entityBuilder.Metadata["foo"]);
            }

            [Fact]
            public virtual void Can_set_property_annotation()
            {
                var modelBuilder = CreateModelBuilder();

                var propertyBuilder = modelBuilder
                    .Entity<Customer>()
                    .Property(c => c.Name).HasAnnotation("foo", "bar");

                Assert.Equal("bar", propertyBuilder.Metadata["foo"]);
            }

            [Fact]
            public virtual void Can_set_property_annotation_when_no_clr_property()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                var propertyBuilder = modelBuilder
                    .Entity<Customer>()
                    .Property<string>(Customer.NameProperty.Name).HasAnnotation("foo", "bar");

                Assert.Equal("bar", propertyBuilder.Metadata["foo"]);
            }

            [Fact]
            public virtual void Can_add_multiple_properties()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Ignore<CustomerDetails>();

                modelBuilder.Entity<Customer>(b =>
                    {
                        b.Property(e => e.Id);
                        b.Property(e => e.Name);
                        b.Property(e => e.AlternateKey);
                    });

                Assert.Equal(3, modelBuilder.Model.FindEntityType(typeof(Customer)).GetProperties().Count());
            }

            [Fact]
            public virtual void Properties_are_required_by_default_only_if_CLR_type_is_nullable()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(b =>
                    {
                        b.Property(e => e.Up);
                        b.Property(e => e.Down);
                        b.Property<int>("Charm");
                        b.Property<string>("Strange");
                        b.Property<int>("Top");
                        b.Property<string>("Bottom");
                    });

                var entityType = (IEntityType)model.FindEntityType(typeof(Quarks));

                Assert.False(entityType.FindProperty("Up").IsNullable);
                Assert.True(entityType.FindProperty("Down").IsNullable);
                Assert.False(entityType.FindProperty("Charm").IsNullable);
                Assert.True(entityType.FindProperty("Strange").IsNullable);
                Assert.False(entityType.FindProperty("Top").IsNullable);
                Assert.True(entityType.FindProperty("Bottom").IsNullable);
            }

            [Fact]
            public virtual void Properties_can_be_ignored()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                var entityType = (IEntityType)modelBuilder.Entity<Quarks>().Metadata;

                Assert.Equal(3, entityType.GetProperties().Count());

                modelBuilder.Entity<Quarks>(b =>
                    {
                        b.Ignore(e => e.Up);
                        b.Ignore(e => e.Down);
                        b.Ignore("Charm");
                        b.Ignore("Strange");
                        b.Ignore("Top");
                        b.Ignore("Bottom");
                        b.Ignore("Shadow");
                    });

                Assert.Equal(Customer.IdProperty.Name, entityType.GetProperties().Single().Name);
            }

            [Fact]
            public virtual void Can_ignore_a_property_that_is_part_of_explicit_entity_key_throws()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                var entityBuilder = modelBuilder.Entity<Customer>();
                entityBuilder.HasKey(e => e.Id);
                entityBuilder.Ignore(e => e.Id);

                Assert.Null(entityBuilder.Metadata.FindProperty(Customer.IdProperty.Name));
            }

            [Fact]
            public virtual void Ignoring_shadow_properties_when_they_have_been_added_explicitly_throws()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                var entityBuilder = modelBuilder.Entity<Customer>();
                entityBuilder.Property<string>("Shadow");
                entityBuilder.Ignore("Shadow");

                Assert.Null(entityBuilder.Metadata.FindProperty("Shadow"));
            }

            [Fact]
            public virtual void Can_add_shadow_properties_when_they_have_been_ignored()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>(b =>
                    {
                        b.Ignore("Shadow");
                        b.Property<string>("Shadow");
                    });

                Assert.NotNull(modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Shadow"));
            }

            [Fact]
            public virtual void Ignoring_a_navigation_property_removes_discovered_entity_types()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>(b =>
                    {
                        b.Ignore(c => c.Details);
                        b.Ignore(c => c.Orders);
                    });

                modelBuilder.Validate();

                Assert.Equal(1, model.GetEntityTypes().Count());
            }

            [Fact]
            public virtual void Ignoring_a_navigation_property_removes_discovered_relationship()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>(b =>
                    {
                        b.Ignore(c => c.Details);
                        b.Ignore(c => c.Orders);
                    });
                modelBuilder.Entity<CustomerDetails>(b => { b.Ignore(c => c.Customer); });

                modelBuilder.Validate();

                Assert.Equal(0, model.GetEntityTypes().First().GetForeignKeys().Count());
                Assert.Equal(0, model.GetEntityTypes().Last().GetForeignKeys().Count());
                Assert.Equal(2, model.GetEntityTypes().Count());
            }

            [Fact]
            public virtual void Properties_can_be_made_required()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(b =>
                    {
                        b.Property(e => e.Up).IsRequired();
                        b.Property(e => e.Down).IsRequired();
                        b.Property<int>("Charm").IsRequired();
                        b.Property<string>("Strange").IsRequired();
                        b.Property<int>("Top").IsRequired();
                        b.Property<string>("Bottom").IsRequired();
                    });

                var entityType = (IEntityType)model.FindEntityType(typeof(Quarks));

                Assert.False(entityType.FindProperty("Up").IsNullable);
                Assert.False(entityType.FindProperty("Down").IsNullable);
                Assert.False(entityType.FindProperty("Charm").IsNullable);
                Assert.False(entityType.FindProperty("Strange").IsNullable);
                Assert.False(entityType.FindProperty("Top").IsNullable);
                Assert.False(entityType.FindProperty("Bottom").IsNullable);
            }

            [Fact]
            public virtual void Properties_can_be_made_optional()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(b =>
                    {
                        b.Property(e => e.Down).IsRequired(false);
                        b.Property<string>("Strange").IsRequired(false);
                        b.Property<string>("Bottom").IsRequired(false);
                    });

                var entityType = (IEntityType)model.FindEntityType(typeof(Quarks));

                Assert.True(entityType.FindProperty("Down").IsNullable);
                Assert.True(entityType.FindProperty("Strange").IsNullable);
                Assert.True(entityType.FindProperty("Bottom").IsNullable);
            }

            [Fact]
            public virtual void Non_nullable_properties_cannot_be_made_optional()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(b =>
                    {
                        Assert.Equal(
                            CoreStrings.CannotBeNullable("Up", "Quarks", "Int32"),
                            Assert.Throws<InvalidOperationException>(() => b.Property(e => e.Up).IsRequired(false)).Message);

                        Assert.Equal(
                            CoreStrings.CannotBeNullable("Charm", "Quarks", "Int32"),
                            Assert.Throws<InvalidOperationException>(() => b.Property<int>("Charm").IsRequired(false)).Message);

                        Assert.Equal(
                            CoreStrings.CannotBeNullable("Top", "Quarks", "Int32"),
                            Assert.Throws<InvalidOperationException>(() => b.Property<int>("Top").IsRequired(false)).Message);
                    });

                var entityType = (IEntityType)model.FindEntityType(typeof(Quarks));

                Assert.False(entityType.FindProperty("Up").IsNullable);
                Assert.False(entityType.FindProperty("Charm").IsNullable);
                Assert.False(entityType.FindProperty("Top").IsNullable);
            }

            [Fact]
            public virtual void Properties_specified_by_string_are_shadow_properties_unless_already_known_to_be_CLR_properties()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(b =>
                    {
                        b.Property<int>("Up");
                        b.Property<int>("Gluon");
                        b.Property<string>("Down");
                        b.Property<string>("Photon");
                    });

                var entityType = (IEntityType)model.FindEntityType(typeof(Quarks));

                Assert.False(entityType.FindProperty("Up").IsShadowProperty);
                Assert.False(entityType.FindProperty("Down").IsShadowProperty);
                Assert.True(entityType.FindProperty("Gluon").IsShadowProperty);
                Assert.True(entityType.FindProperty("Photon").IsShadowProperty);

                Assert.Equal(-1, entityType.FindProperty("Up").GetShadowIndex());
                Assert.Equal(-1, entityType.FindProperty("Down").GetShadowIndex());
                Assert.Equal(0, entityType.FindProperty("Gluon").GetShadowIndex());
                Assert.Equal(1, entityType.FindProperty("Photon").GetShadowIndex());
            }

            [Fact]
            public virtual void Properties_can_be_made_concurency_tokens()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(b =>
                    {
                        b.Property(e => e.Up).IsConcurrencyToken();
                        b.Property(e => e.Down).IsConcurrencyToken(false);
                        b.Property<int>("Charm").IsConcurrencyToken(true);
                        b.Property<string>("Strange").IsConcurrencyToken(false);
                        b.Property<int>("Top").IsConcurrencyToken();
                        b.Property<string>("Bottom").IsConcurrencyToken(false);
                    });

                var entityType = (IEntityType)model.FindEntityType(typeof(Quarks));

                Assert.False(entityType.FindProperty(Customer.IdProperty.Name).IsConcurrencyToken);
                Assert.True(entityType.FindProperty("Up").IsConcurrencyToken);
                Assert.False(entityType.FindProperty("Down").IsConcurrencyToken);
                Assert.True(entityType.FindProperty("Charm").IsConcurrencyToken);
                Assert.False(entityType.FindProperty("Strange").IsConcurrencyToken);
                Assert.True(entityType.FindProperty("Top").IsConcurrencyToken);
                Assert.False(entityType.FindProperty("Bottom").IsConcurrencyToken);

                Assert.Equal(-1, entityType.FindProperty(Customer.IdProperty.Name).GetOriginalValueIndex());
                Assert.Equal(2, entityType.FindProperty("Up").GetOriginalValueIndex());
                Assert.Equal(-1, entityType.FindProperty("Down").GetOriginalValueIndex());
                Assert.Equal(0, entityType.FindProperty("Charm").GetOriginalValueIndex());
                Assert.Equal(-1, entityType.FindProperty("Strange").GetOriginalValueIndex());
                Assert.Equal(1, entityType.FindProperty("Top").GetOriginalValueIndex());
                Assert.Equal(-1, entityType.FindProperty("Bottom").GetOriginalValueIndex());
            }

            [Fact]
            public virtual void Properties_can_be_set_to_generate_values_on_Add()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(b =>
                    {
                        b.Property(e => e.Id).Metadata.RequiresValueGenerator = false;
                        b.Property(e => e.Up).Metadata.RequiresValueGenerator = true;
                        b.Property(e => e.Down).Metadata.RequiresValueGenerator = true;
                        b.Property<int>("Charm").Metadata.RequiresValueGenerator = true;
                        b.Property<string>("Strange").Metadata.RequiresValueGenerator = false;
                        b.Property<int>("Top").Metadata.RequiresValueGenerator = true;
                        b.Property<string>("Bottom").Metadata.RequiresValueGenerator = false;
                    });

                var entityType = model.FindEntityType(typeof(Quarks));

                Assert.Equal(false, entityType.FindProperty(Customer.IdProperty.Name).RequiresValueGenerator);
                Assert.Equal(true, entityType.FindProperty("Up").RequiresValueGenerator);
                Assert.Equal(true, entityType.FindProperty("Down").RequiresValueGenerator);
                Assert.Equal(true, entityType.FindProperty("Charm").RequiresValueGenerator);
                Assert.Equal(false, entityType.FindProperty("Strange").RequiresValueGenerator);
                Assert.Equal(true, entityType.FindProperty("Top").RequiresValueGenerator);
                Assert.Equal(false, entityType.FindProperty("Bottom").RequiresValueGenerator);
            }

            [Fact]
            public virtual void Properties_can_be_set_to_be_store_computed()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(b =>
                    {
                        b.HasKey(e => e.Id);
                        b.Property(e => e.Up).ValueGeneratedOnAddOrUpdate();
                        b.Property(e => e.Down).ValueGeneratedNever();
                        b.Property<int>("Charm").ValueGeneratedOnAdd();
                        b.Property<string>("Strange").ValueGeneratedNever();
                        b.Property<int>("Top").ValueGeneratedOnAddOrUpdate();
                        b.Property<string>("Bottom").ValueGeneratedNever();
                    });

                var entityType = model.FindEntityType(typeof(Quarks));

                Assert.Equal(ValueGenerated.OnAdd, entityType.FindProperty(Customer.IdProperty.Name).ValueGenerated);
                Assert.Equal(ValueGenerated.OnAddOrUpdate, entityType.FindProperty("Up").ValueGenerated);
                Assert.Equal(ValueGenerated.Never, entityType.FindProperty("Down").ValueGenerated);
                Assert.Equal(ValueGenerated.OnAdd, entityType.FindProperty("Charm").ValueGenerated);
                Assert.Equal(ValueGenerated.Never, entityType.FindProperty("Strange").ValueGenerated);
                Assert.Equal(ValueGenerated.OnAddOrUpdate, entityType.FindProperty("Top").ValueGenerated);
                Assert.Equal(ValueGenerated.Never, entityType.FindProperty("Bottom").ValueGenerated);
            }

            [Fact]
            public virtual void Can_set_max_length_for_properties()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(b =>
                    {
                        b.Property(e => e.Up).HasMaxLength(0);
                        b.Property(e => e.Down).HasMaxLength(100);
                        b.Property<int>("Charm").HasMaxLength(0);
                        b.Property<string>("Strange").HasMaxLength(100);
                        b.Property<int>("Top").HasMaxLength(0);
                        b.Property<string>("Bottom").HasMaxLength(100);
                    });

                var entityType = model.FindEntityType(typeof(Quarks));

                Assert.Null(entityType.FindProperty(Customer.IdProperty.Name).GetMaxLength());
                Assert.Equal(0, entityType.FindProperty("Up").GetMaxLength());
                Assert.Equal(100, entityType.FindProperty("Down").GetMaxLength());
                Assert.Equal(0, entityType.FindProperty("Charm").GetMaxLength());
                Assert.Equal(100, entityType.FindProperty("Strange").GetMaxLength());
                Assert.Equal(0, entityType.FindProperty("Top").GetMaxLength());
                Assert.Equal(100, entityType.FindProperty("Bottom").GetMaxLength());
            }

            [Fact]
            public virtual void PropertyBuilder_methods_can_be_chained()
            {
                CreateModelBuilder()
                    .Entity<Quarks>()
                    .Property(e => e.Up)
                    .IsRequired()
                    .HasAnnotation("A", "V")
                    .IsConcurrencyToken()
                    .ValueGeneratedNever()
                    .ValueGeneratedOnAdd()
                    .ValueGeneratedOnAddOrUpdate()
                    .HasMaxLength(100)
                    .IsRequired();
            }

            [Fact]
            public virtual void Can_add_index()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder
                    .Entity<Customer>()
                    .HasIndex(ix => ix.Name);

                var entityType = model.FindEntityType(typeof(Customer));

                var index = entityType.GetIndexes().Single();
                Assert.Equal(Customer.NameProperty.Name, index.Properties.Single().Name);
            }

            [Fact]
            public virtual void Can_add_index_when_no_clr_property()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder
                    .Entity<Customer>(b =>
                        {
                            b.Property<int>("Index");
                            b.HasIndex("Index");
                        });

                var entityType = model.FindEntityType(typeof(Customer));

                var index = entityType.GetIndexes().Single();
                Assert.Equal("Index", index.Properties.Single().Name);
            }

            [Fact]
            public virtual void Can_add_multiple_indexes()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                var entityBuilder = modelBuilder.Entity<Customer>();
                var firstIndexBuilder = entityBuilder.HasIndex(ix => ix.Id).IsUnique();
                var secondIndexBuilder = entityBuilder.HasIndex(ix => ix.Name).HasAnnotation("A1", "V1");

                var entityType = (IEntityType)model.FindEntityType(typeof(Customer));

                Assert.Equal(2, entityType.GetIndexes().Count());
                Assert.True(firstIndexBuilder.Metadata.IsUnique);
                Assert.False(((IIndex)secondIndexBuilder.Metadata).IsUnique);
                Assert.Equal("V1", secondIndexBuilder.Metadata["A1"]);
            }

            [Fact]
            public virtual void Can_set_primary_key_by_convention_for_user_specified_shadow_property()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                var entityBuilder = modelBuilder.Entity<EntityWithoutId>();

                var entityType = (IEntityType)model.FindEntityType(typeof(EntityWithoutId));

                Assert.Null(entityType.FindPrimaryKey());

                entityBuilder.Property<int>("Id");

                Assert.NotNull(entityType.FindPrimaryKey());
                AssertEqual(new[] { "Id" }, entityType.FindPrimaryKey().Properties.Select(p => p.Name));
            }

            [Fact]
            public virtual void Can_ignore_explicit_interface_implementation()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<EntityBase>().Ignore(e => ((IEntityBase)e).Target);

                Assert.Empty(modelBuilder.Model.FindEntityType(typeof(EntityBase)).GetProperties());
            }

            [Fact]
            public virtual void Throws_for_shadow_key()
            {
                var modelBuilder = CreateModelBuilder();
                var entityType = (EntityType)modelBuilder.Entity<SelfRef>().Metadata;
                var shadowProperty = entityType.AddProperty("ShadowProperty", ConfigurationSource.Convention);
                shadowProperty.IsNullable = false;
                entityType.AddKey(shadowProperty);

                Assert.Equal(
                    CoreStrings.ShadowKey("{'ShadowProperty'}", typeof(SelfRef).FullName, "{'ShadowProperty'}"),
                    Assert.Throws<InvalidOperationException>(() => modelBuilder.Validate()).Message);
            }
        }
    }
}
