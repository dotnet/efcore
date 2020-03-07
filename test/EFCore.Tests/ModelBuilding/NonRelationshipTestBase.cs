// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public abstract partial class ModelBuilderTest
    {
        public abstract class NonRelationshipTestBase : ModelBuilderTestBase
        {
            [ConditionalFact]
            public void Can_set_model_annotation()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder = modelBuilder.HasAnnotation("Fus", "Ro");

                Assert.NotNull(modelBuilder);
                Assert.Equal("Ro", model.FindAnnotation("Fus").Value);
            }

            [ConditionalFact]
            public void Model_is_readonly_after_Finalize()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.FinalizeModel();

                Assert.ThrowsAny<Exception>(() => modelBuilder.HasAnnotation("Fus", "Ro"));
            }

            [ConditionalFact]
            public virtual void Can_get_entity_builder_for_clr_type()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                var entityBuilder = modelBuilder.Entity<Customer>();

                Assert.NotNull(entityBuilder);
                Assert.Equal(typeof(Customer).FullName, model.FindEntityType(typeof(Customer)).Name);
            }

            [ConditionalFact]
            public virtual void Can_set_entity_key_from_clr_property()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Customer>().HasKey(b => b.Id);

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Equal(1, entity.FindPrimaryKey().Properties.Count);
                Assert.Equal(Customer.IdProperty.Name, entity.FindPrimaryKey().Properties.First().Name);
            }

            [ConditionalFact]
            public virtual void Entity_key_on_shadow_property_is_discovered_by_convention()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<Order>().Property<int>("Id");

                var entity = modelBuilder.Model.FindEntityType(typeof(Order));

                modelBuilder.FinalizeModel();
                Assert.Equal("Id", entity.FindPrimaryKey().Properties.Single().Name);
            }

            [ConditionalFact]
            public virtual void Entity_key_on_secondary_property_is_discovered_by_convention_when_first_ignored()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<SelfRef>()
                    .Ignore(s => s.SelfRef1)
                    .Ignore(s => s.SelfRef2)
                    .Ignore(s => s.Id);

                modelBuilder.FinalizeModel();
                var entity = modelBuilder.Model.FindEntityType(typeof(SelfRef));
                Assert.Equal(nameof(SelfRef.SelfRefId), entity.FindPrimaryKey().Properties.Single().Name);
            }

            [ConditionalFact]
            public virtual void Can_set_entity_key_from_property_name_when_no_clr_property()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Customer>(
                    b =>
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

            [ConditionalFact]
            public virtual void Can_set_entity_key_from_clr_property_when_property_ignored()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>(
                    b =>
                    {
                        b.Ignore(Customer.IdProperty.Name);
                        b.HasKey(e => e.Id);
                    });

                var entity = modelBuilder.Model.FindEntityType(typeof(Customer));

                Assert.Equal(1, entity.FindPrimaryKey().Properties.Count);
                Assert.Equal(Customer.IdProperty.Name, entity.FindPrimaryKey().Properties.First().Name);
            }

            [ConditionalFact]
            public virtual void Can_set_composite_entity_key_from_clr_properties()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder
                    .Entity<Customer>()
                    .HasKey(
                        e => new { e.Id, e.Name });

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Equal(2, entity.FindPrimaryKey().Properties.Count);
                Assert.Equal(Customer.IdProperty.Name, entity.FindPrimaryKey().Properties.First().Name);
                Assert.Equal(Customer.NameProperty.Name, entity.FindPrimaryKey().Properties.Last().Name);
            }

            [ConditionalFact]
            public virtual void Can_set_composite_entity_key_from_property_names_when_mixed_properties()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Ignore<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                modelBuilder.Entity<Customer>(
                    b =>
                    {
                        b.Property<string>(Customer.NameProperty.Name + "Shadow");
                        b.HasKey(Customer.IdProperty.Name, Customer.NameProperty.Name + "Shadow");
                    });

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Equal(2, entity.FindPrimaryKey().Properties.Count);
                Assert.Equal(Customer.IdProperty.Name, entity.FindPrimaryKey().Properties.First().Name);
                Assert.Equal(Customer.NameProperty.Name + "Shadow", entity.FindPrimaryKey().Properties.Last().Name);
            }

            [ConditionalFact]
            public virtual void Can_set_entity_key_with_annotations()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                var keyBuilder = modelBuilder
                    .Entity<Customer>()
                    .HasKey(
                        e => new { e.Id, e.Name });

                keyBuilder.HasAnnotation("A1", "V1")
                    .HasAnnotation("A2", "V2");

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Equal(
                    new[] { Customer.IdProperty.Name, Customer.NameProperty.Name }, entity.FindPrimaryKey().Properties.Select(p => p.Name));
                Assert.Equal("V1", keyBuilder.Metadata["A1"]);
                Assert.Equal("V2", keyBuilder.Metadata["A2"]);
            }

            [ConditionalFact]
            public virtual void Can_upgrade_candidate_key_to_primary_key()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<Customer>().Property<int>(Customer.IdProperty.Name);
                modelBuilder.Entity<Customer>().HasAlternateKey(b => b.Name);
                modelBuilder.Ignore<OrderDetails>();
                modelBuilder.Ignore<CustomerDetails>();
                modelBuilder.Ignore<Order>();

                var entity = modelBuilder.Model.FindEntityType(typeof(Customer));
                var key = entity.FindKey(entity.FindProperty(Customer.NameProperty));

                modelBuilder.Entity<Customer>().HasKey(b => b.Name);

                modelBuilder.FinalizeModel();

                Assert.Same(key, entity.GetKeys().Single());

                var nameProperty = entity.FindPrimaryKey().Properties.Single();
                Assert.Equal(Customer.NameProperty.Name, nameProperty.Name);
                Assert.False(nameProperty.RequiresValueGenerator());
                Assert.Equal(ValueGenerated.Never, nameProperty.ValueGenerated);

                var idProperty = (IProperty)entity.FindProperty(Customer.IdProperty);
                Assert.Equal(ValueGenerated.Never, idProperty.ValueGenerated);
            }

            [ConditionalFact]
            public virtual void Can_set_alternate_key_from_clr_property()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Customer>().HasAlternateKey(b => b.AlternateKey);

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Equal(1, entity.GetKeys().Count(key => key != entity.FindPrimaryKey()));
                Assert.Equal(
                    Customer.AlternateKeyProperty.Name,
                    entity.GetKeys().First(key => key != entity.FindPrimaryKey()).Properties.First().Name);
            }

            [ConditionalFact]
            public virtual void Can_set_alternate_key_from_property_name_when_no_clr_property()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Customer>(
                    b =>
                    {
                        b.Property<int>(Customer.AlternateKeyProperty.Name + 1);
                        b.HasAlternateKey(Customer.AlternateKeyProperty.Name + 1);
                    });

                var entity = model.FindEntityType(typeof(Customer));

                Assert.Equal(1, entity.GetKeys().Count(key => key != entity.FindPrimaryKey()));
                Assert.Equal(
                    Customer.AlternateKeyProperty.Name + 1,
                    entity.GetKeys().First(key => key != entity.FindPrimaryKey()).Properties.First().Name);
            }

            [ConditionalFact]
            public virtual void Can_set_alternate_key_from_clr_property_when_property_ignored()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<Customer>(
                    b =>
                    {
                        b.Ignore(Customer.AlternateKeyProperty.Name);
                        b.HasAlternateKey(e => e.AlternateKey);
                    });

                var entity = modelBuilder.Model.FindEntityType(typeof(Customer));

                Assert.Equal(1, entity.GetKeys().Count(key => key != entity.FindPrimaryKey()));
                Assert.Equal(
                    Customer.AlternateKeyProperty.Name,
                    entity.GetKeys().First(key => key != entity.FindPrimaryKey()).Properties.First().Name);
            }

            [ConditionalFact]
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

            [ConditionalFact]
            public virtual void Can_set_entity_annotation()
            {
                var modelBuilder = CreateModelBuilder();

                var entityBuilder = modelBuilder
                    .Entity<Customer>()
                    .HasAnnotation("foo", "bar");

                Assert.Equal("bar", entityBuilder.Metadata["foo"]);
            }

            [ConditionalFact]
            public virtual void Can_set_property_annotation()
            {
                var modelBuilder = CreateModelBuilder();

                var propertyBuilder = modelBuilder
                    .Entity<Customer>()
                    .Property(c => c.Name).HasAnnotation("foo", "bar");

                Assert.Equal("bar", propertyBuilder.Metadata["foo"]);
            }

            [ConditionalFact]
            public virtual void Can_set_property_annotation_when_no_clr_property()
            {
                var modelBuilder = CreateModelBuilder();

                var propertyBuilder = modelBuilder
                    .Entity<Customer>()
                    .Property<string>(Customer.NameProperty.Name).HasAnnotation("foo", "bar");

                Assert.Equal("bar", propertyBuilder.Metadata["foo"]);
            }

            [ConditionalFact]
            public virtual void Can_add_multiple_properties()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Ignore<CustomerDetails>();

                modelBuilder.Entity<Customer>(
                    b =>
                    {
                        b.Property(e => e.Id);
                        b.Property(e => e.Name);
                        b.Property(e => e.AlternateKey);
                    });

                Assert.Equal(3, modelBuilder.Model.FindEntityType(typeof(Customer)).GetProperties().Count());
            }

            [ConditionalFact]
            public virtual void Properties_are_required_by_default_only_if_CLR_type_is_nullable()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(
                    b =>
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

            [ConditionalFact]
            public virtual void Properties_can_be_ignored()
            {
                var modelBuilder = CreateModelBuilder();

                var entityType = (IEntityType)modelBuilder.Entity<Quarks>().Metadata;

                Assert.Equal(3, entityType.GetProperties().Count());

                modelBuilder.Entity<Quarks>(
                    b =>
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

            [ConditionalFact]
            public virtual void Can_ignore_a_property_that_is_part_of_explicit_entity_key()
            {
                var modelBuilder = CreateModelBuilder();

                var entityBuilder = modelBuilder.Entity<Customer>();
                entityBuilder.HasKey(e => e.Id);
                entityBuilder.Ignore(e => e.Id);

                Assert.Null(entityBuilder.Metadata.FindProperty(Customer.IdProperty.Name));
            }

            [ConditionalFact]
            public virtual void Can_ignore_shadow_properties_when_they_have_been_added_explicitly()
            {
                var modelBuilder = CreateModelBuilder();

                var entityBuilder = modelBuilder.Entity<Customer>();
                entityBuilder.Property<string>("Shadow");
                entityBuilder.Ignore("Shadow");

                Assert.Null(entityBuilder.Metadata.FindProperty("Shadow"));
            }

            [ConditionalFact]
            public virtual void Can_add_shadow_properties_when_they_have_been_ignored()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Customer>(
                    b =>
                    {
                        b.Ignore("Shadow");
                        b.Property<string>("Shadow");
                    });

                Assert.NotNull(modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Shadow"));
            }

            [ConditionalFact]
            public virtual void Can_override_navigations_as_properties()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>();

                var customer = model.FindEntityType(typeof(Customer));
                Assert.NotNull(customer.FindNavigation(nameof(Customer.Orders)));

                modelBuilder.Entity<Customer>().Property(c => c.Orders);

                Assert.Null(customer.FindNavigation(nameof(Customer.Orders)));
                Assert.NotNull(customer.FindProperty(nameof(Customer.Orders)));
            }

            [ConditionalFact]
            public virtual void Ignoring_a_navigation_property_removes_discovered_entity_types()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>(
                    b =>
                    {
                        b.Ignore(c => c.Details);
                        b.Ignore(c => c.Orders);
                    });

                modelBuilder.FinalizeModel();

                Assert.Single(model.GetEntityTypes());
            }

            [ConditionalFact]
            public virtual void Ignoring_a_navigation_property_removes_discovered_relationship()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Customer>(
                    b =>
                    {
                        b.Ignore(c => c.Details);
                        b.Ignore(c => c.Orders);
                    });
                modelBuilder.Entity<CustomerDetails>(b => b.Ignore(c => c.Customer));

                modelBuilder.FinalizeModel();

                Assert.Empty(model.GetEntityTypes().First().GetForeignKeys());
                Assert.Empty(model.GetEntityTypes().Last().GetForeignKeys());
                Assert.Equal(2, model.GetEntityTypes().Count());
            }

            [ConditionalFact]
            public virtual void Properties_can_be_made_required()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(
                    b =>
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

            [ConditionalFact]
            public virtual void Properties_can_be_made_optional()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(
                    b =>
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

            [ConditionalFact]
            public virtual void Key_properties_cannot_be_made_optional()
            {
                Assert.Equal(
                    CoreStrings.KeyPropertyCannotBeNullable(nameof(Quarks.Down), nameof(Quarks), "{'" + nameof(Quarks.Down) + "'}"),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            CreateModelBuilder().Entity<Quarks>(
                                b =>
                                {
                                    b.HasAlternateKey(
                                        e => new { e.Down });
                                    b.Property(e => e.Down).IsRequired(false);
                                })).Message);
            }

            [ConditionalFact]
            public virtual void Non_nullable_properties_cannot_be_made_optional()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(
                    b =>
                    {
                        Assert.Equal(
                            CoreStrings.CannotBeNullable("Up", "Quarks", "int"),
                            Assert.Throws<InvalidOperationException>(() => b.Property(e => e.Up).IsRequired(false)).Message);

                        Assert.Equal(
                            CoreStrings.CannotBeNullable("Charm", "Quarks", "int"),
                            Assert.Throws<InvalidOperationException>(() => b.Property<int>("Charm").IsRequired(false)).Message);

                        Assert.Equal(
                            CoreStrings.CannotBeNullable("Top", "Quarks", "int"),
                            Assert.Throws<InvalidOperationException>(() => b.Property<int>("Top").IsRequired(false)).Message);
                    });

                var entityType = (IEntityType)model.FindEntityType(typeof(Quarks));

                Assert.False(entityType.FindProperty("Up").IsNullable);
                Assert.False(entityType.FindProperty("Charm").IsNullable);
                Assert.False(entityType.FindProperty("Top").IsNullable);
            }

            [ConditionalFact]
            public virtual void Properties_specified_by_string_are_shadow_properties_unless_already_known_to_be_CLR_properties()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(
                    b =>
                    {
                        b.Property<int>("Up");
                        b.Property<int>("Gluon");
                        b.Property<string>("Down");
                        b.Property<string>("Photon");
                    });

                var entityType = (IEntityType)model.FindEntityType(typeof(Quarks));

                modelBuilder.FinalizeModel();

                Assert.False(entityType.FindProperty("Up").IsShadowProperty());
                Assert.False(entityType.FindProperty("Down").IsShadowProperty());
                Assert.True(entityType.FindProperty("Gluon").IsShadowProperty());
                Assert.True(entityType.FindProperty("Photon").IsShadowProperty());

                Assert.Equal(-1, entityType.FindProperty("Up").GetShadowIndex());
                Assert.Equal(-1, entityType.FindProperty("Down").GetShadowIndex());
                Assert.Equal(0, entityType.FindProperty("Gluon").GetShadowIndex());
                Assert.Equal(1, entityType.FindProperty("Photon").GetShadowIndex());
            }

            [ConditionalFact]
            public virtual void Properties_can_be_made_concurrency_tokens()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(
                    b =>
                    {
                        b.Property(e => e.Up).IsConcurrencyToken();
                        b.Property(e => e.Down).IsConcurrencyToken(false);
                        b.Property<int>("Charm").IsConcurrencyToken();
                        b.Property<string>("Strange").IsConcurrencyToken(false);
                        b.Property<int>("Top").IsConcurrencyToken();
                        b.Property<string>("Bottom").IsConcurrencyToken(false);
                        b.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
                    });

                var entityType = (IEntityType)model.FindEntityType(typeof(Quarks));

                modelBuilder.FinalizeModel();

                Assert.False(entityType.FindProperty(Customer.IdProperty.Name).IsConcurrencyToken);
                Assert.True(entityType.FindProperty("Up").IsConcurrencyToken);
                Assert.False(entityType.FindProperty("Down").IsConcurrencyToken);
                Assert.True(entityType.FindProperty("Charm").IsConcurrencyToken);
                Assert.False(entityType.FindProperty("Strange").IsConcurrencyToken);
                Assert.True(entityType.FindProperty("Top").IsConcurrencyToken);
                Assert.False(entityType.FindProperty("Bottom").IsConcurrencyToken);

                Assert.Equal(0, entityType.FindProperty(Customer.IdProperty.Name).GetOriginalValueIndex());
                Assert.Equal(3, entityType.FindProperty("Up").GetOriginalValueIndex());
                Assert.Equal(-1, entityType.FindProperty("Down").GetOriginalValueIndex());
                Assert.Equal(1, entityType.FindProperty("Charm").GetOriginalValueIndex());
                Assert.Equal(-1, entityType.FindProperty("Strange").GetOriginalValueIndex());
                Assert.Equal(2, entityType.FindProperty("Top").GetOriginalValueIndex());
                Assert.Equal(-1, entityType.FindProperty("Bottom").GetOriginalValueIndex());

                Assert.Equal(ChangeTrackingStrategy.ChangingAndChangedNotifications, entityType.GetChangeTrackingStrategy());
            }

            [ConditionalFact]
            public virtual void Properties_can_have_access_mode_set()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(
                    b =>
                    {
                        b.Property(e => e.Up);
                        b.Property(e => e.Down).UsePropertyAccessMode(PropertyAccessMode.Field);
                        b.Property<int>("Charm").UsePropertyAccessMode(PropertyAccessMode.Property);
                        b.Property<string>("Strange").UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);
                    });

                var entityType = (IEntityType)model.FindEntityType(typeof(Quarks));

                Assert.Equal(PropertyAccessMode.PreferField, entityType.FindProperty("Up").GetPropertyAccessMode());
                Assert.Equal(PropertyAccessMode.Field, entityType.FindProperty("Down").GetPropertyAccessMode());
                Assert.Equal(PropertyAccessMode.Property, entityType.FindProperty("Charm").GetPropertyAccessMode());
                Assert.Equal(PropertyAccessMode.FieldDuringConstruction, entityType.FindProperty("Strange").GetPropertyAccessMode());
            }

            [ConditionalFact]
            public virtual void Access_mode_can_be_overridden_at_entity_and_property_levels()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Field);

                modelBuilder.Entity<Hob>().Property(e => e.Id1);

                modelBuilder.Entity<Quarks>(
                    b =>
                    {
                        b.UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);
                        b.Property(e => e.Up).UsePropertyAccessMode(PropertyAccessMode.Property);
                    });

                Assert.Equal(PropertyAccessMode.Field, model.GetPropertyAccessMode());

                var hobsType = (IEntityType)model.FindEntityType(typeof(Hob));
                Assert.Equal(PropertyAccessMode.Field, hobsType.GetPropertyAccessMode());
                Assert.Equal(PropertyAccessMode.Field, hobsType.FindProperty("Id1").GetPropertyAccessMode());

                var quarksType = (IEntityType)model.FindEntityType(typeof(Quarks));
                Assert.Equal(PropertyAccessMode.FieldDuringConstruction, quarksType.GetPropertyAccessMode());
                Assert.Equal(PropertyAccessMode.FieldDuringConstruction, quarksType.FindProperty("Down").GetPropertyAccessMode());
                Assert.Equal(PropertyAccessMode.Property, quarksType.FindProperty("Up").GetPropertyAccessMode());
            }

            [ConditionalFact]
            public virtual void Properties_can_have_store_type_set()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(
                    b =>
                    {
                        b.Property(e => e.Up);
                        b.Property(e => e.Down).HasConversion<byte[]>();
                        b.Property<int>("Charm").HasConversion(typeof(long));
                        b.Property<string>("Strange").HasConversion<byte[]>();
                        b.Property<string>("Strange").HasConversion((Type)null);
                    });

                var entityType = (IEntityType)model.FindEntityType(typeof(Quarks));

                Assert.Null(entityType.FindProperty("Up").GetProviderClrType());
                Assert.Same(typeof(byte[]), entityType.FindProperty("Down").GetProviderClrType());
                Assert.Same(typeof(long), entityType.FindProperty("Charm").GetProviderClrType());
                Assert.Null(entityType.FindProperty("Strange").GetProviderClrType());
            }

            [ConditionalFact]
            public virtual void Properties_can_have_value_converter_set_non_generic()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                ValueConverter stringConverter = new StringToBytesConverter(Encoding.UTF8);
                ValueConverter intConverter = new CastingConverter<int, long>();

                modelBuilder.Entity<Quarks>(
                    b =>
                    {
                        b.Property(e => e.Up);
                        b.Property(e => e.Down).HasConversion(stringConverter);
                        b.Property<int>("Charm").HasConversion(intConverter);
                        b.Property<string>("Strange").HasConversion(stringConverter);
                        b.Property<string>("Strange").HasConversion((ValueConverter)null);
                    });

                var entityType = (IEntityType)model.FindEntityType(typeof(Quarks));

                Assert.Null(entityType.FindProperty("Up").GetValueConverter());
                Assert.Same(stringConverter, entityType.FindProperty("Down").GetValueConverter());
                Assert.Same(intConverter, entityType.FindProperty("Charm").GetValueConverter());
                Assert.Null(entityType.FindProperty("Strange").GetValueConverter());
            }

            [ConditionalFact]
            public virtual void Properties_can_have_value_converter_set_generic()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                var stringConverter = new StringToBytesConverter(Encoding.UTF8);
                var intConverter = new CastingConverter<int, long>();

                modelBuilder.Entity<Quarks>(
                    b =>
                    {
                        b.Property(e => e.Up);
                        b.Property(e => e.Down).HasConversion(stringConverter);
                        b.Property<int>("Charm").HasConversion(intConverter);
                        b.Property<string>("Strange").HasConversion(stringConverter);
                        b.Property<string>("Strange").HasConversion((ValueConverter)null);
                    });

                var entityType = (IEntityType)model.FindEntityType(typeof(Quarks));

                Assert.Null(entityType.FindProperty("Up").GetValueConverter());
                Assert.Same(stringConverter, entityType.FindProperty("Down").GetValueConverter());
                Assert.Same(intConverter, entityType.FindProperty("Charm").GetValueConverter());
                Assert.Null(entityType.FindProperty("Strange").GetValueConverter());
            }

            [ConditionalFact]
            public virtual void Properties_can_have_value_converter_set_inline()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(
                    b =>
                    {
                        b.Property(e => e.Up);
                        b.Property(e => e.Down).HasConversion(v => v.ToCharArray(), v => new string(v));
                        b.Property<int>("Charm").HasConversion(v => (long)v, v => (int)v);
                    });

                var entityType = (IEntityType)model.FindEntityType(typeof(Quarks));

                Assert.Null(entityType.FindProperty("Up").GetValueConverter());
                Assert.NotNull(entityType.FindProperty("Down").GetValueConverter());
                Assert.NotNull(entityType.FindProperty("Charm").GetValueConverter());
            }

            [ConditionalFact]
            public virtual void IEnumerable_properties_with_value_converter_set_are_not_discovered_as_navigations()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<DynamicProperty>(
                    b =>
                    {
                        b.Property(e => e.ExpandoObject).HasConversion(
                            v => (string)((IDictionary<string, object>)v)["Value"], v => DeserializeExpandoObject(v));

                        var comparer = new ValueComparer<ExpandoObject>(
                            (v1, v2) => v1.SequenceEqual(v2),
                            v => v.GetHashCode());

                        b.Property(e => e.ExpandoObject).Metadata.SetValueComparer(comparer);
                    });

                modelBuilder.FinalizeModel();

                var entityType = (IEntityType)model.GetEntityTypes().Single();
                Assert.NotNull(entityType.FindProperty(nameof(DynamicProperty.ExpandoObject)).GetValueConverter());
            }

            private static ExpandoObject DeserializeExpandoObject(string value)
            {
                dynamic obj = new ExpandoObject();
                obj.Value = value;

                return obj;
            }

            [ConditionalFact]
            public virtual void Value_converter_type_is_checked()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(
                    b =>
                    {
                        Assert.Equal(
                            CoreStrings.ConverterPropertyMismatch("string", "Quarks", "Up", "int"),
                            Assert.Throws<InvalidOperationException>(
                                () => b.Property(e => e.Up).HasConversion(
                                    new StringToBytesConverter(Encoding.UTF8))).Message);
                    });

                var entityType = (IEntityType)model.FindEntityType(typeof(Quarks));
                Assert.Null(entityType.FindProperty("Up").GetValueConverter());
            }

            [ConditionalFact]
            public virtual void Properties_can_have_field_set()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(
                    b =>
                    {
                        b.Property<int>("Up").HasField("_forUp");
                        b.Property(e => e.Down).HasField("_forDown");
                        b.Property<int?>("_forWierd").HasField("_forWierd");
                    });

                var entityType = (IEntityType)model.FindEntityType(typeof(Quarks));

                Assert.Equal("_forUp", entityType.FindProperty("Up").GetFieldName());
                Assert.Equal("_forDown", entityType.FindProperty("Down").GetFieldName());
                Assert.Equal("_forWierd", entityType.FindProperty("_forWierd").GetFieldName());
            }

            [ConditionalFact]
            public virtual void HasField_throws_if_field_is_not_found()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Quarks>(
                    b =>
                    {
                        Assert.Equal(
                            CoreStrings.MissingBackingField("_notFound", nameof(Quarks.Down), nameof(Quarks)),
                            Assert.Throws<InvalidOperationException>(() => b.Property(e => e.Down).HasField("_notFound")).Message);
                    });
            }

            [ConditionalFact]
            public virtual void HasField_throws_if_field_is_wrong_type()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Quarks>(
                    b =>
                    {
                        Assert.Equal(
                            CoreStrings.BadBackingFieldType("_forUp", "int", nameof(Quarks), nameof(Quarks.Down), "string"),
                            Assert.Throws<InvalidOperationException>(() => b.Property(e => e.Down).HasField("_forUp")).Message);
                    });
            }

            [ConditionalFact]
            public virtual void Properties_can_be_set_to_generate_values_on_Add()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Quarks>(
                    b =>
                    {
                        b.HasKey(e => e.Id);
                        b.Property(e => e.Up).ValueGeneratedOnAddOrUpdate();
                        b.Property(e => e.Down).ValueGeneratedNever();
                        b.Property<int>("Charm").ValueGeneratedOnAdd();
                        b.Property<string>("Strange").ValueGeneratedNever();
                        b.Property<int>("Top").ValueGeneratedOnAddOrUpdate();
                        b.Property<string>("Bottom").ValueGeneratedOnUpdate();
                    });

                var entityType = modelBuilder.Model.FindEntityType(typeof(Quarks));
                Assert.Equal(ValueGenerated.OnAdd, entityType.FindProperty(Customer.IdProperty.Name).ValueGenerated);
                Assert.Equal(ValueGenerated.OnAddOrUpdate, entityType.FindProperty("Up").ValueGenerated);
                Assert.Equal(ValueGenerated.Never, entityType.FindProperty("Down").ValueGenerated);
                Assert.Equal(ValueGenerated.OnAdd, entityType.FindProperty("Charm").ValueGenerated);
                Assert.Equal(ValueGenerated.Never, entityType.FindProperty("Strange").ValueGenerated);
                Assert.Equal(ValueGenerated.OnAddOrUpdate, entityType.FindProperty("Top").ValueGenerated);
                Assert.Equal(ValueGenerated.OnUpdate, entityType.FindProperty("Bottom").ValueGenerated);
            }

            [ConditionalFact]
            public virtual void Properties_can_set_row_version()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(
                    b =>
                    {
                        b.HasKey(e => e.Id);
                        b.Property(e => e.Up).IsRowVersion();
                        b.Property(e => e.Down).ValueGeneratedNever();
                        b.Property<int>("Charm").IsRowVersion();
                    });

                modelBuilder.FinalizeModel();

                var entityType = model.FindEntityType(typeof(Quarks));

                Assert.Equal(ValueGenerated.OnAddOrUpdate, entityType.FindProperty("Up").ValueGenerated);
                Assert.Equal(ValueGenerated.Never, entityType.FindProperty("Down").ValueGenerated);
                Assert.Equal(ValueGenerated.OnAddOrUpdate, entityType.FindProperty("Charm").ValueGenerated);

                Assert.True(entityType.FindProperty("Up").IsConcurrencyToken);
                Assert.False(entityType.FindProperty("Down").IsConcurrencyToken);
                Assert.True(entityType.FindProperty("Charm").IsConcurrencyToken);
            }

            [ConditionalFact]
            public virtual void Can_set_max_length_for_properties()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(
                    b =>
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

            [ConditionalFact]
            public virtual void Can_set_custom_value_generator_for_properties()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(
                    b =>
                    {
                        b.Property(e => e.Up).HasValueGenerator<CustomValueGenerator>();
                        b.Property(e => e.Down).HasValueGenerator(typeof(CustomValueGenerator));
                        b.Property<int>("Charm").HasValueGenerator((_, __) => new CustomValueGenerator());
                        b.Property<string>("Strange").HasValueGenerator<CustomValueGenerator>();
                        b.Property<int>("Top").HasValueGenerator(typeof(CustomValueGenerator));
                        b.Property<string>("Bottom").HasValueGenerator((_, __) => new CustomValueGenerator());
                    });

                modelBuilder.FinalizeModel();

                var entityType = model.FindEntityType(typeof(Quarks));

                Assert.Null(entityType.FindProperty(Customer.IdProperty.Name).GetValueGeneratorFactory());
                Assert.IsType<CustomValueGenerator>(entityType.FindProperty("Up").GetValueGeneratorFactory()(null, null));
                Assert.IsType<CustomValueGenerator>(entityType.FindProperty("Down").GetValueGeneratorFactory()(null, null));
                Assert.IsType<CustomValueGenerator>(entityType.FindProperty("Charm").GetValueGeneratorFactory()(null, null));
                Assert.IsType<CustomValueGenerator>(entityType.FindProperty("Strange").GetValueGeneratorFactory()(null, null));
                Assert.IsType<CustomValueGenerator>(entityType.FindProperty("Top").GetValueGeneratorFactory()(null, null));
                Assert.IsType<CustomValueGenerator>(entityType.FindProperty("Bottom").GetValueGeneratorFactory()(null, null));
            }

            private class CustomValueGenerator : ValueGenerator<int>
            {
                public override int Next(EntityEntry entry)
                {
                    throw new NotImplementedException();
                }

                public override bool GeneratesTemporaryValues => false;
            }

            [ConditionalFact]
            public virtual void Throws_for_bad_value_generator_type()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<Quarks>(
                    b =>
                    {
                        Assert.Equal(
                            CoreStrings.BadValueGeneratorType(nameof(Random), nameof(ValueGenerator)),
                            Assert.Throws<ArgumentException>(() => b.Property(e => e.Down).HasValueGenerator(typeof(Random))).Message);
                    });
            }

            [ConditionalFact]
            public virtual void Throws_for_value_generator_that_cannot_be_constructed()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(
                    b =>
                    {
                        b.Property(e => e.Up).HasValueGenerator<BadCustomValueGenerator1>();
                        b.Property(e => e.Down).HasValueGenerator<BadCustomValueGenerator2>();
                    });

                var entityType = model.FindEntityType(typeof(Quarks));

                Assert.Equal(
                    CoreStrings.CannotCreateValueGenerator(nameof(BadCustomValueGenerator1)),
                    Assert.Throws<InvalidOperationException>(
                        () => entityType.FindProperty("Up").GetValueGeneratorFactory()(null, null)).Message);

                Assert.Equal(
                    CoreStrings.CannotCreateValueGenerator(nameof(BadCustomValueGenerator2)),
                    Assert.Throws<InvalidOperationException>(
                        () => entityType.FindProperty("Down").GetValueGeneratorFactory()(null, null)).Message);
            }

            private class BadCustomValueGenerator1 : CustomValueGenerator
            {
                public BadCustomValueGenerator1(string foo)
                {
                }
            }

            private abstract class BadCustomValueGenerator2 : CustomValueGenerator
            {
            }

            [ConditionalFact]
            public virtual void Throws_for_collection_of_string()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<StringCollectionEntity>();

                Assert.Equal(
                    CoreStrings.PropertyNotAdded(
                        nameof(StringCollectionEntity), nameof(StringCollectionEntity.Property), "ICollection<string>"),
                    Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
            }

            protected class StringCollectionEntity
            {
                public ICollection<string> Property { get; set; }
            }

            [ConditionalFact]
            public virtual void Can_set_unicode_for_properties()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder.Entity<Quarks>(
                    b =>
                    {
                        b.Property(e => e.Up).IsUnicode();
                        b.Property(e => e.Down).IsUnicode(false);
                        b.Property<int>("Charm").IsUnicode();
                        b.Property<string>("Strange").IsUnicode(false);
                        b.Property<int>("Top").IsUnicode();
                        b.Property<string>("Bottom").IsUnicode(false);
                    });

                var entityType = model.FindEntityType(typeof(Quarks));

                Assert.Null(entityType.FindProperty(Customer.IdProperty.Name).IsUnicode());
                Assert.True(entityType.FindProperty("Up").IsUnicode());
                Assert.False(entityType.FindProperty("Down").IsUnicode());
                Assert.True(entityType.FindProperty("Charm").IsUnicode());
                Assert.False(entityType.FindProperty("Strange").IsUnicode());
                Assert.True(entityType.FindProperty("Top").IsUnicode());
                Assert.False(entityType.FindProperty("Bottom").IsUnicode());
            }

            [ConditionalFact]
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
                    .ValueGeneratedOnUpdate()
                    .IsUnicode()
                    .HasMaxLength(100)
                    .HasValueGenerator<CustomValueGenerator>()
                    .HasValueGenerator(typeof(CustomValueGenerator))
                    .HasValueGenerator((_, __) => null)
                    .IsRequired();
            }

            [ConditionalFact]
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

            [ConditionalFact]
            public virtual void Can_add_index_when_no_clr_property()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                modelBuilder
                    .Entity<Customer>(
                        b =>
                        {
                            b.Property<int>("Index");
                            b.HasIndex("Index");
                        });

                var entityType = model.FindEntityType(typeof(Customer));

                var index = entityType.GetIndexes().Single();
                Assert.Equal("Index", index.Properties.Single().Name);
            }

            [ConditionalFact]
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

            [ConditionalFact]
            public virtual void Can_add_contained_indexes()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;

                var entityBuilder = modelBuilder.Entity<Customer>();
                var firstIndexBuilder = entityBuilder.HasIndex(
                    ix => new { ix.Id, ix.AlternateKey }).IsUnique();
                var secondIndexBuilder = entityBuilder.HasIndex(
                    ix => new { ix.Id });

                var entityType = (IEntityType)model.FindEntityType(typeof(Customer));

                Assert.Equal(2, entityType.GetIndexes().Count());
                Assert.True(firstIndexBuilder.Metadata.IsUnique);
                Assert.False(secondIndexBuilder.Metadata.IsUnique);
            }

            [ConditionalFact]
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

            [ConditionalFact]
            public virtual void Can_ignore_explicit_interface_implementation()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<EntityBase>().Ignore(e => ((IEntityBase)e).Target);

                Assert.Empty(modelBuilder.Model.FindEntityType(typeof(EntityBase)).GetProperties());
            }

            [ConditionalFact]
            public virtual void Can_add_seed_data_objects()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Beta>(
                    c =>
                    {
                        c.HasData(
                            new Beta { Id = -1 });
                        var customers = new List<Beta> { new Beta { Id = -2 } };
                        c.HasData(customers);
                    });

                modelBuilder.FinalizeModel();

                var customer = model.FindEntityType(typeof(Beta));
                var data = customer.GetSeedData();
                Assert.Equal(2, data.Count());
                Assert.Equal(-1, data.First()[nameof(Beta.Id)]);
                Assert.Equal(-2, data.Last()[nameof(Beta.Id)]);
            }

            [ConditionalFact]
            public virtual void Can_add_seed_data_anonymous_objects()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Beta>(
                    c =>
                    {
                        c.HasData(
                            new { Id = -1 });
                        var customers = new List<object> { new { Id = -2 } };
                        c.HasData(customers);
                    });

                modelBuilder.FinalizeModel();

                var customer = model.FindEntityType(typeof(Beta));
                var data = customer.GetSeedData();
                Assert.Equal(2, data.Count());
                Assert.Equal(-1, data.First().Values.Single());
                Assert.Equal(-2, data.Last().Values.Single());
            }

            [ConditionalFact]
            public virtual void Private_property_is_not_discovered_by_convention()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<Alpha>();
                modelBuilder.Entity<Gamma>();

                modelBuilder.FinalizeModel();

                Assert.Single(modelBuilder.Model.FindEntityType(typeof(Gamma)).GetProperties());
            }
        }
    }
}
