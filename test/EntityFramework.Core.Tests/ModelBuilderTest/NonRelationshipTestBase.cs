// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Microsoft.Data.Entity.Tests
{
    public abstract partial class ModelBuilderTest
    {
        public abstract class NonRelationshipTestBase : ModelBuilderTestBase
        {
            [Fact]
            public virtual void Can_get_entity_builder_for_clr_type()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

                var entityBuilder = modelBuilder.Entity<Customer>();

                Assert.NotNull(entityBuilder);
                Assert.Equal(typeof(Customer).FullName, model.GetEntityType(typeof(Customer)).Name);
            }

            [Fact]
            public virtual void Cannot_get_entity_builder_for_ignored_clr_type()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<Customer>();

                Assert.Equal(Strings.EntityIgnoredExplicitly(typeof(Customer).FullName),
                    Assert.Throws<InvalidOperationException>(() =>
                        modelBuilder.Entity<Customer>()).Message);
            }

            [Fact]
            public virtual void Can_set_entity_key_from_clr_property()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

                modelBuilder.Entity<Customer>().Key(b => b.Id);

                var entity = model.GetEntityType(typeof(Customer));

                Assert.Equal(1, entity.GetPrimaryKey().Properties.Count);
                Assert.Equal(Customer.IdProperty.Name, entity.GetPrimaryKey().Properties.First().Name);
            }

            [Fact]
            public virtual void Can_set_entity_key_from_property_name_when_no_clr_property()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

                modelBuilder.Entity<Customer>(b =>
                    {
                        b.Property<int>(Customer.IdProperty.Name + 1);
                        b.Ignore(p => p.Details);
                        b.Ignore(p => p.Orders);
                        b.Key(Customer.IdProperty.Name + 1);
                    });

                var entity = model.GetEntityType(typeof(Customer));

                Assert.Equal(1, entity.GetPrimaryKey().Properties.Count);
                Assert.Equal(Customer.IdProperty.Name + 1, entity.GetPrimaryKey().Properties.First().Name);
            }

            [Fact]
            public virtual void Setting_entity_key_from_clr_property_when_property_ignored_throws()
            {
                var modelBuilder = CreateModelBuilder();

                Assert.Equal(Strings.PropertyIgnoredExplicitly(Customer.IdProperty.Name, typeof(Customer).FullName),
                    Assert.Throws<InvalidOperationException>(() =>
                        modelBuilder.Entity<Customer>(b =>
                            {
                                b.Ignore(Customer.IdProperty.Name);
                                b.Key(e => e.Id);
                            })).Message);
            }

            [Fact]
            public virtual void Can_set_composite_entity_key_from_clr_properties()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

                modelBuilder
                    .Entity<Customer>()
                    .Key(e => new { e.Id, e.Name });

                var entity = model.GetEntityType(typeof(Customer));

                Assert.Equal(2, entity.GetPrimaryKey().Properties.Count);
                Assert.Equal(Customer.IdProperty.Name, entity.GetPrimaryKey().Properties.First().Name);
                Assert.Equal(Customer.NameProperty.Name, entity.GetPrimaryKey().Properties.Last().Name);
            }

            [Fact]
            public virtual void Can_set_composite_entity_key_from_property_names_when_mixed_properties()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Ignore<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                modelBuilder.Entity<Customer>(b =>
                    {
                        b.Property<string>(Customer.NameProperty.Name + "Shadow");
                        b.Key(Customer.IdProperty.Name, Customer.NameProperty.Name + "Shadow");
                    });

                var entity = model.GetEntityType(typeof(Customer));

                Assert.Equal(2, entity.GetPrimaryKey().Properties.Count);
                Assert.Equal(Customer.IdProperty.Name, entity.GetPrimaryKey().Properties.First().Name);
                Assert.Equal(Customer.NameProperty.Name + "Shadow", entity.GetPrimaryKey().Properties.Last().Name);
            }

            [Fact]
            public virtual void Can_set_entity_key_with_annotations()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

                var keyBuilder = modelBuilder
                    .Entity<Customer>()
                    .Key(e => new { e.Id, e.Name });

                keyBuilder.Annotation("A1", "V1")
                    .Annotation("A2", "V2");

                var entity = model.GetEntityType(typeof(Customer));

                Assert.Equal(new[] { Customer.IdProperty.Name, Customer.NameProperty.Name }, entity.GetPrimaryKey().Properties.Select(p => p.Name));
                Assert.Equal("V1", keyBuilder.Metadata["A1"]);
                Assert.Equal("V2", keyBuilder.Metadata["A2"]);
            }

            [Fact]
            public virtual void Can_upgrade_candidate_key_to_primary_key()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>().Property<int>(Customer.IdProperty.Name);

                var entity = model.GetEntityType(typeof(Customer));
                var key = entity.AddKey(entity.GetOrAddProperty(Customer.NameProperty));

                modelBuilder.Entity<Customer>().Key(b => b.Name);

                Assert.Same(key, entity.GetKeys().Single());
                Assert.Equal(Customer.NameProperty.Name, entity.GetPrimaryKey().Properties.Single().Name);

                var idProperty = (IProperty)entity.GetProperty(Customer.IdProperty);
                Assert.False(idProperty.RequiresValueGenerator);
                Assert.Equal(ValueGenerated.Never, idProperty.ValueGenerated);
            }

            [Fact]
            public virtual void Can_set_alternate_key_from_clr_property()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

                modelBuilder.Entity<Customer>().AlternateKey(b => b.AlternateKey);

                var entity = model.GetEntityType(typeof(Customer));

                Assert.Equal(1, entity.GetKeys().Count(key => key != entity.GetPrimaryKey()));
                Assert.Equal(Customer.AlternateKeyProperty.Name, entity.GetKeys().First(key => key != entity.GetPrimaryKey()).Properties.First().Name);
            }

            [Fact]
            public virtual void Can_set_alternate_key_from_property_name_when_no_clr_property()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

                modelBuilder.Entity<Customer>(b =>
                    {
                        b.Property<int>(Customer.AlternateKeyProperty.Name + 1);
                        b.AlternateKey(Customer.AlternateKeyProperty.Name + 1);
                    });

                var entity = model.GetEntityType(typeof(Customer));

                Assert.Equal(1, entity.GetKeys().Count(key => key != entity.GetPrimaryKey()));
                Assert.Equal(Customer.AlternateKeyProperty.Name + 1, entity.GetKeys().First(key => key != entity.GetPrimaryKey()).Properties.First().Name);
            }

            [Fact]
            public virtual void Setting_alternate_key_from_clr_property_when_property_ignored_throws()
            {
                var modelBuilder = CreateModelBuilder();

                Assert.Equal(Strings.PropertyIgnoredExplicitly(Customer.AlternateKeyProperty.Name, typeof(Customer).FullName),
                    Assert.Throws<InvalidOperationException>(() =>
                        modelBuilder.Entity<Customer>(b =>
                            {
                                b.Ignore(Customer.AlternateKeyProperty.Name);
                                b.AlternateKey(e => e.AlternateKey);
                            })).Message);
            }

            [Fact]
            public virtual void Can_set_entity_annotation()
            {
                var modelBuilder = CreateModelBuilder();

                var entityBuilder = modelBuilder
                    .Entity<Customer>()
                    .Annotation("foo", "bar");

                Assert.Equal("bar", entityBuilder.Metadata["foo"]);
            }

            [Fact]
            public virtual void Can_set_property_annotation()
            {
                var modelBuilder = CreateModelBuilder();

                var propertyBuilder = modelBuilder
                    .Entity<Customer>()
                    .Property(c => c.Name).Annotation("foo", "bar");

                Assert.Equal("bar", propertyBuilder.Metadata["foo"]);
            }

            [Fact]
            public virtual void Can_set_property_annotation_when_no_clr_property()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

                var propertyBuilder = modelBuilder
                    .Entity<Customer>()
                    .Property<string>(Customer.NameProperty.Name).Annotation("foo", "bar");

                Assert.Equal("bar", propertyBuilder.Metadata["foo"]);
            }

            [Fact]
            public virtual void Can_add_multiple_properties()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

                modelBuilder.Entity<Customer>(b =>
                    {
                        b.Property(e => e.Id);
                        b.Property(e => e.Name);
                        b.Property(e => e.AlternateKey);
                    });

                Assert.Equal(3, model.GetEntityType(typeof(Customer)).PropertyCount);
            }

            [Fact]
            public virtual void Properties_are_required_by_default_only_if_CLR_type_is_nullable()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

                modelBuilder.Entity<Quarks>(b =>
                    {
                        b.Property(e => e.Up);
                        b.Property(e => e.Down);
                        b.Property<int>("Charm");
                        b.Property<string>("Strange");
                        b.Property<int>("Top");
                        b.Property<string>("Bottom");
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
            public virtual void Properties_can_be_ignored()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

                var entityType = (IEntityType)modelBuilder.Entity<Quarks>().Metadata;

                Assert.Equal(7, entityType.GetProperties().Count());

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
            public virtual void Can_ignore_a_property_that_is_part_of_explicit_entity_key()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

                Assert.NotNull(modelBuilder.Entity<Customer>(b =>
                    {
                        b.Key(e => e.Id);
                        b.Ignore(e => e.Id);
                    }));
            }

            [Fact]
            public virtual void Can_ignore_shadow_properties_when_they_have_been_added_explicitly()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

                Assert.NotNull(modelBuilder.Entity<Customer>(b =>
                    {
                        b.Property<string>("Shadow");
                        b.Ignore("Shadow");
                    }));
            }

            [Fact]
            public virtual void Adding_shadow_properties_when_they_have_been_ignored_throws()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

                Assert.Equal(Strings.PropertyIgnoredExplicitly("Shadow", typeof(Customer).FullName),
                    Assert.Throws<InvalidOperationException>(() =>
                        modelBuilder.Entity<Customer>(b =>
                            {
                                b.Ignore("Shadow");
                                b.Property<string>("Shadow");
                            })).Message);
            }

            [Fact]
            public virtual void Ignoring_a_navigation_property_removes_discovered_entity_types()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>(b =>
                    {
                        b.Ignore(c => c.Details);
                        b.Ignore(c => c.Orders);
                    });

                modelBuilder.Validate();

                Assert.Equal(1, model.EntityTypes.Count);
            }

            [Fact]
            public virtual void Ignoring_a_navigation_property_removes_discovered_relationship()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Customer>(b =>
                    {
                        b.Ignore(c => c.Details);
                        b.Ignore(c => c.Orders);
                    });
                modelBuilder.Entity<CustomerDetails>(b => { b.Ignore(c => c.Customer); });

                modelBuilder.Validate();

                Assert.Equal(0, model.EntityTypes[0].GetForeignKeys().Count());
                Assert.Equal(0, model.EntityTypes[1].GetForeignKeys().Count());
                Assert.Equal(2, model.EntityTypes.Count);
            }

            [Fact]
            public virtual void Properties_can_be_made_required()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

                modelBuilder.Entity<Quarks>(b =>
                    {
                        b.Property(e => e.Up).Required();
                        b.Property(e => e.Down).Required();
                        b.Property<int>("Charm").Required();
                        b.Property<string>("Strange").Required();
                        b.Property<int>("Top").Required();
                        b.Property<string>("Bottom").Required();
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
            public virtual void Properties_can_be_made_optional()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

                modelBuilder.Entity<Quarks>(b =>
                    {
                        b.Property(e => e.Down).Required(false);
                        b.Property<string>("Strange").Required(false);
                        b.Property<string>("Bottom").Required(false);
                    });

                var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

                Assert.True(entityType.GetProperty("Down").IsNullable);
                Assert.True(entityType.GetProperty("Strange").IsNullable);
                Assert.True(entityType.GetProperty("Bottom").IsNullable);
            }

            [Fact]
            public virtual void Non_nullable_properties_cannot_be_made_optional()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

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
                            Assert.Throws<InvalidOperationException>(() => b.Property<int>("Top").Required(false)).Message);
                    });

                var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

                Assert.False(entityType.GetProperty("Up").IsNullable);
                Assert.False(entityType.GetProperty("Charm").IsNullable);
                Assert.False(entityType.GetProperty("Top").IsNullable);
            }

            [Fact]
            public virtual void Properties_specified_by_string_are_shadow_properties_unless_already_known_to_be_CLR_properties()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

                modelBuilder.Entity<Quarks>(b =>
                    {
                        b.Property<int>("Charm");
                        b.Property<int>("Top");
                        b.Property<string>("Gluon");
                        b.Property<string>("Photon");
                    });

                var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

                Assert.False(entityType.GetProperty("Up").IsShadowProperty);
                Assert.False(entityType.GetProperty("Charm").IsShadowProperty);
                Assert.False(entityType.GetProperty("Top").IsShadowProperty);
                Assert.True(entityType.GetProperty("Gluon").IsShadowProperty);
                Assert.True(entityType.GetProperty("Photon").IsShadowProperty);

                Assert.Equal(-1, entityType.GetProperty("Up").GetShadowIndex());
                Assert.Equal(-1, entityType.GetProperty("Charm").GetShadowIndex());
                Assert.Equal(-1, entityType.GetProperty("Top").GetShadowIndex());
                Assert.Equal(0, entityType.GetProperty("Gluon").GetShadowIndex());
                Assert.Equal(1, entityType.GetProperty("Photon").GetShadowIndex());
            }

            [Fact]
            public virtual void Properties_can_be_made_concurency_tokens()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

                modelBuilder.Entity<Quarks>(b =>
                    {
                        b.Property(e => e.Up).ConcurrencyToken();
                        b.Property(e => e.Down).ConcurrencyToken(false);
                        b.Property<int>("Charm").ConcurrencyToken(true);
                        b.Property<string>("Strange").ConcurrencyToken(false);
                        b.Property<int>("Top").ConcurrencyToken();
                        b.Property<string>("Bottom").ConcurrencyToken(false);
                    });

                var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

                Assert.False(entityType.GetProperty(Customer.IdProperty.Name).IsConcurrencyToken);
                Assert.True(entityType.GetProperty("Up").IsConcurrencyToken);
                Assert.False(entityType.GetProperty("Down").IsConcurrencyToken);
                Assert.True(entityType.GetProperty("Charm").IsConcurrencyToken);
                Assert.False(entityType.GetProperty("Strange").IsConcurrencyToken);
                Assert.True(entityType.GetProperty("Top").IsConcurrencyToken);
                Assert.False(entityType.GetProperty("Bottom").IsConcurrencyToken);

                Assert.Equal(-1, entityType.GetProperty(Customer.IdProperty.Name).GetOriginalValueIndex());
                Assert.Equal(2, entityType.GetProperty("Up").GetOriginalValueIndex());
                Assert.Equal(-1, entityType.GetProperty("Down").GetOriginalValueIndex());
                Assert.Equal(0, entityType.GetProperty("Charm").GetOriginalValueIndex());
                Assert.Equal(-1, entityType.GetProperty("Strange").GetOriginalValueIndex());
                Assert.Equal(1, entityType.GetProperty("Top").GetOriginalValueIndex());
                Assert.Equal(-1, entityType.GetProperty("Bottom").GetOriginalValueIndex());
            }

            [Fact]
            public virtual void Properties_can_be_set_to_generate_values_on_Add()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

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

                var entityType = model.GetEntityType(typeof(Quarks));

                Assert.Equal(false, entityType.GetProperty(Customer.IdProperty.Name).RequiresValueGenerator);
                Assert.Equal(true, entityType.GetProperty("Up").RequiresValueGenerator);
                Assert.Equal(true, entityType.GetProperty("Down").RequiresValueGenerator);
                Assert.Equal(true, entityType.GetProperty("Charm").RequiresValueGenerator);
                Assert.Equal(false, entityType.GetProperty("Strange").RequiresValueGenerator);
                Assert.Equal(true, entityType.GetProperty("Top").RequiresValueGenerator);
                Assert.Equal(false, entityType.GetProperty("Bottom").RequiresValueGenerator);
            }

            [Fact]
            public virtual void Properties_can_be_set_to_be_store_computed()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

                modelBuilder.Entity<Quarks>(b =>
                    {
                        b.Key(e => e.Id);
                        b.Property(e => e.Up).ValueGeneratedOnAddOrUpdate();
                        b.Property(e => e.Down).ValueGeneratedNever();
                        b.Property<int>("Charm").ValueGeneratedOnAdd();
                        b.Property<string>("Strange").ValueGeneratedNever();
                        b.Property<int>("Top").ValueGeneratedOnAddOrUpdate();
                        b.Property<string>("Bottom").ValueGeneratedNever();
                    });

                var entityType = model.GetEntityType(typeof(Quarks));

                Assert.Equal(ValueGenerated.OnAdd, entityType.GetProperty(Customer.IdProperty.Name).ValueGenerated);
                Assert.Equal(ValueGenerated.OnAddOrUpdate, entityType.GetProperty("Up").ValueGenerated);
                Assert.Equal(ValueGenerated.Never, entityType.GetProperty("Down").ValueGenerated);
                Assert.Equal(ValueGenerated.OnAdd, entityType.GetProperty("Charm").ValueGenerated);
                Assert.Equal(ValueGenerated.Never, entityType.GetProperty("Strange").ValueGenerated);
                Assert.Equal(ValueGenerated.OnAddOrUpdate, entityType.GetProperty("Top").ValueGenerated);
                Assert.Equal(ValueGenerated.Never, entityType.GetProperty("Bottom").ValueGenerated);
            }

            [Fact]
            public virtual void Can_set_max_length_for_properties()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

                modelBuilder.Entity<Quarks>(b =>
                    {
                        b.Property(e => e.Up).MaxLength(0);
                        b.Property(e => e.Down).MaxLength(100);
                        b.Property<int>("Charm").MaxLength(0);
                        b.Property<string>("Strange").MaxLength(100);
                        b.Property<int>("Top").MaxLength(0);
                        b.Property<string>("Bottom").MaxLength(100);
                    });

                var entityType = model.GetEntityType(typeof(Quarks));

                Assert.Null(entityType.GetProperty(Customer.IdProperty.Name).GetMaxLength());
                Assert.Equal(0, entityType.GetProperty("Up").GetMaxLength());
                Assert.Equal(100, entityType.GetProperty("Down").GetMaxLength());
                Assert.Equal(0, entityType.GetProperty("Charm").GetMaxLength());
                Assert.Equal(100, entityType.GetProperty("Strange").GetMaxLength());
                Assert.Equal(0, entityType.GetProperty("Top").GetMaxLength());
                Assert.Equal(100, entityType.GetProperty("Bottom").GetMaxLength());
            }

            [Fact]
            public virtual void PropertyBuilder_methods_can_be_chained()
            {
                CreateModelBuilder()
                    .Entity<Quarks>()
                    .Property(e => e.Up)
                    .Required()
                    .Annotation("A", "V")
                    .ConcurrencyToken()
                    .ValueGeneratedNever()
                    .ValueGeneratedOnAdd()
                    .ValueGeneratedOnAddOrUpdate()
                    .MaxLength(100)
                    .Required();
            }

            [Fact]
            public virtual void Can_add_index()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

                modelBuilder
                    .Entity<Customer>()
                    .Index(ix => ix.Name);

                var entityType = model.GetEntityType(typeof(Customer));

                var index = entityType.Indexes.Single();
                Assert.Equal(Customer.NameProperty.Name, index.Properties.Single().Name);
            }

            [Fact]
            public virtual void Can_add_index_when_no_clr_property()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

                modelBuilder
                    .Entity<Customer>(b =>
                        {
                            b.Property<int>("Index");
                            b.Index("Index");
                        });

                var entityType = model.GetEntityType(typeof(Customer));

                var index = entityType.Indexes.Single();
                Assert.Equal("Index", index.Properties.Single().Name);
            }

            [Fact]
            public virtual void Can_add_multiple_indexes()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);

                var entityBuilder = modelBuilder.Entity<Customer>();
                var firstIndexBuilder = entityBuilder.Index(ix => ix.Id).Unique();
                var secondIndexBuilder = entityBuilder.Index(ix => ix.Name).Annotation("A1", "V1");

                var entityType = (IEntityType)model.GetEntityType(typeof(Customer));

                Assert.Equal(2, entityType.GetIndexes().Count());
                Assert.True(firstIndexBuilder.Metadata.IsUnique);
                Assert.False(((IIndex)secondIndexBuilder.Metadata).IsUnique);
                Assert.Equal("V1", secondIndexBuilder.Metadata["A1"]);
            }
        }
    }
}
