// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public abstract partial class ModelBuilderTest
{
    public abstract class NonRelationshipTestBase(ModelBuilderFixtureBase fixture) : ModelBuilderTestBase(fixture)
    {
        [ConditionalFact]
        public void Can_set_model_annotation()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder = modelBuilder.HasAnnotation("Fus", "Ro");

            Assert.NotNull(modelBuilder);

            var model = modelBuilder.FinalizeModel();
            Assert.Equal("Ro", model.FindAnnotation("Fus")!.Value);
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
            Assert.Equal(typeof(Customer).FullName, model.FindEntityType(typeof(Customer))!.Name);
        }

        [ConditionalFact]
        public virtual void Can_set_entity_key_from_clr_property()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<Customer>().HasKey(b => b.Id);

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(1, entity.FindPrimaryKey()!.Properties.Count);
            Assert.Equal(Customer.IdProperty.Name, entity.FindPrimaryKey()!.Properties.First().Name);
        }

        [ConditionalFact]
        public virtual void Entity_key_on_shadow_property_is_discovered_by_convention()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<Order>().Property<int>("Id");
            modelBuilder.Entity<Customer>();
            modelBuilder.Ignore<Product>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Order))!;
            Assert.Equal("Id", entity.FindPrimaryKey()!.Properties.Single().Name);
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
            var entity = modelBuilder.Model.FindEntityType(typeof(SelfRef))!;
            Assert.Equal(nameof(SelfRef.SelfRefId), entity.FindPrimaryKey()!.Properties.Single().Name);
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

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(1, entity.FindPrimaryKey()!.Properties.Count);
            Assert.Equal(Customer.IdProperty.Name + 1, entity.FindPrimaryKey()!.Properties.First().Name);
        }

        [ConditionalFact]
        public virtual void Can_set_entity_key_from_clr_property_when_property_ignored_on_keyless()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>(
                b =>
                {
                    b.HasNoKey();
                    b.Ignore(Customer.IdProperty.Name);
                    b.HasKey(e => e.Id);
                });

            var entity = modelBuilder.Model.FindEntityType(typeof(Customer))!;

            Assert.Equal(1, entity.FindPrimaryKey()!.Properties.Count);
            Assert.Equal(Customer.IdProperty.Name, entity.FindPrimaryKey()!.Properties.First().Name);
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

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(2, entity.FindPrimaryKey()!.Properties.Count);
            Assert.Equal(Customer.IdProperty.Name, entity.FindPrimaryKey()!.Properties.First().Name);
            Assert.Equal(Customer.NameProperty.Name, entity.FindPrimaryKey()!.Properties.Last().Name);
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

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(2, entity.FindPrimaryKey()!.Properties.Count);
            Assert.Equal(Customer.IdProperty.Name, entity.FindPrimaryKey()!.Properties.First().Name);
            Assert.Equal(Customer.NameProperty.Name + "Shadow", entity.FindPrimaryKey()!.Properties.Last().Name);
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

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(
                new[] { Customer.IdProperty.Name, Customer.NameProperty.Name }, entity.FindPrimaryKey()!.Properties.Select(p => p.Name));
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

            var entity = modelBuilder.Model.FindEntityType(typeof(Customer))!;
            var key = entity.FindKey(entity.FindProperty(Customer.NameProperty)!);

            modelBuilder.Entity<Customer>().HasKey(b => b.Name);

            modelBuilder.FinalizeModel();

            var nameProperty = entity.FindPrimaryKey()!.Properties.Single();
            Assert.Equal(Customer.NameProperty.Name, nameProperty.Name);
            Assert.False(nameProperty.RequiresValueGenerator());
            Assert.Equal(ValueGenerated.Never, nameProperty.ValueGenerated);

            var idProperty = (IReadOnlyProperty)entity.FindProperty(Customer.IdProperty)!;
            Assert.Equal(ValueGenerated.Never, idProperty.ValueGenerated);
        }

        [ConditionalFact]
        public virtual void Can_set_alternate_key_from_clr_property()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<Customer>().HasAlternateKey(b => b.AlternateKey);

            var entity = model.FindEntityType(typeof(Customer))!;

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

            var entity = model.FindEntityType(typeof(Customer))!;

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

            var entity = modelBuilder.Model.FindEntityType(typeof(Customer))!;

            Assert.Equal(
                Customer.AlternateKeyProperty.Name,
                entity.GetKeys().First(key => key != entity.FindPrimaryKey()).Properties.First().Name);
        }

        [ConditionalFact]
        public virtual void Setting_alternate_key_makes_properties_required()
        {
            var modelBuilder = CreateModelBuilder();
            var entityBuilder = modelBuilder.Entity<Customer>();

            var entity = modelBuilder.Model.FindEntityType(typeof(Customer))!;
            var alternateKeyProperty = entity.FindProperty(nameof(Customer.Name))!;
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

            modelBuilder.Ignore<Product>();
            modelBuilder
                .Entity<Customer>()
                .Property(c => c.Name).HasAnnotation("foo", "bar");

            var property = modelBuilder.FinalizeModel().FindEntityType(typeof(Customer))!.FindProperty(nameof(Customer.Name))!;

            Assert.Equal("bar", property["foo"]);
        }

        [ConditionalFact]
        public virtual void Can_set_property_annotation_when_no_clr_property()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Product>();
            modelBuilder
                .Entity<Customer>()
                .Property<string>(Customer.NameProperty.Name).HasAnnotation("foo", "bar");

            var property = modelBuilder.FinalizeModel().FindEntityType(typeof(Customer))!.FindProperty(nameof(Customer.Name))!;

            Assert.Equal("bar", property["foo"]);
        }

        [ConditionalFact]
        public virtual void Can_set_property_annotation_by_type()
        {
            var modelBuilder = CreateModelBuilder(c => c.Properties<string>().HaveAnnotation("foo", "bar"));

            modelBuilder.Ignore<Product>();
            var propertyBuilder = modelBuilder
                .Entity<Customer>()
                .Property(c => c.Name);

            var property = modelBuilder.FinalizeModel().FindEntityType(typeof(Customer))!.FindProperty(nameof(Customer.Name))!;

            Assert.Equal("bar", property["foo"]);
        }

        [ConditionalFact]
        public virtual void Properties_are_required_by_default_only_if_CLR_type_is_nullable()
        {
            var modelBuilder = CreateModelBuilder();

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

            var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(Quarks))!;

            Assert.False(entityType.FindProperty("Up")!.IsNullable);
            Assert.True(entityType.FindProperty("Down")!.IsNullable);
            Assert.False(entityType.FindProperty("Charm")!.IsNullable);
            Assert.True(entityType.FindProperty("Strange")!.IsNullable);
            Assert.False(entityType.FindProperty("Top")!.IsNullable);
            Assert.True(entityType.FindProperty("Bottom")!.IsNullable);
        }

        [ConditionalFact]
        public virtual void Properties_can_be_ignored()
        {
            var modelBuilder = CreateModelBuilder();

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

            var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(Quarks))!;
            Assert.Contains(nameof(Quarks.Id), entityType.GetProperties().Select(p => p.Name));
            Assert.DoesNotContain(nameof(Quarks.Up), entityType.GetProperties().Select(p => p.Name));
            Assert.DoesNotContain(nameof(Quarks.Down), entityType.GetProperties().Select(p => p.Name));
        }

        [ConditionalFact]
        public virtual void Properties_can_be_ignored_by_type()
        {
            var modelBuilder = CreateModelBuilder(c => c.IgnoreAny<Guid>());

            modelBuilder.Ignore<Product>();
            modelBuilder.Entity<Customer>();

            var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(Customer))!;
            Assert.Null(entityType.FindProperty(nameof(Customer.AlternateKey)));
        }

        [ConditionalFact]
        public virtual void Conventions_can_be_added()
        {
            var modelBuilder = CreateModelBuilder(c => c.Conventions.Add(s => new TestConvention()));

            var model = modelBuilder.FinalizeModel();

            Assert.Equal("bar", model["foo"]);
        }

        [ConditionalFact]
        public virtual void Conventions_can_be_removed()
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                {
                    c.Conventions.Add(s => new TestConvention());
                    c.Conventions.Remove(typeof(TestConvention));
                });

            var model = modelBuilder.FinalizeModel();

            Assert.Null(model["foo"]);
        }

        [ConditionalFact]
        public virtual void Conventions_can_be_removed_by_generic_method()
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                {
                    c.Conventions.Add(s => new TestConvention());
                    c.Conventions.Remove<TestConvention>();
                });

            var model = modelBuilder.FinalizeModel();

            Assert.Null(model["foo"]);
        }

        [ConditionalFact]
        public virtual void Conventions_can_be_replaced()
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                    c.Conventions.Replace<DbSetFindingConvention>(
                        s => new TestDbSetFindingConvention(s.GetService<ProviderConventionSetBuilderDependencies>()!)));

            var model = modelBuilder.FinalizeModel();

            Assert.Equal("bar", model["foo"]);
        }

        protected class TestConvention : IModelInitializedConvention
        {
            public void ProcessModelInitialized(
                IConventionModelBuilder modelBuilder,
                IConventionContext<IConventionModelBuilder> context)
                => modelBuilder.HasAnnotation("foo", "bar");
        }

        protected class TestDbSetFindingConvention(ProviderConventionSetBuilderDependencies dependencies) : DbSetFindingConvention(dependencies)
        {
            public override void ProcessModelInitialized(
                IConventionModelBuilder modelBuilder,
                IConventionContext<IConventionModelBuilder> context)
                => modelBuilder.HasAnnotation("foo", "bar");
        }

        [ConditionalFact]
        public virtual void Int32_cannot_be_ignored()
            => Assert.Equal(
                CoreStrings.UnconfigurableType("int?", "Ignored", "Property", "int"),
                Assert.Throws<InvalidOperationException>(() => CreateModelBuilder(c => c.IgnoreAny<int>())).Message);

        [ConditionalFact]
        public virtual void Object_cannot_be_ignored()
            => Assert.Equal(
                CoreStrings.UnconfigurableType("string", "Ignored", "Property", "object"),
                Assert.Throws<InvalidOperationException>(() => CreateModelBuilder(c => c.IgnoreAny<object>())).Message);

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

            modelBuilder.Ignore<Product>();
            modelBuilder.Entity<Customer>(
                b =>
                {
                    b.Ignore("Shadow");
                    b.Property<string>("Shadow");
                });

            var model = modelBuilder.FinalizeModel();

            Assert.NotNull(model.FindEntityType(typeof(Customer))!.FindProperty("Shadow"));
        }

        [ConditionalFact]
        public virtual void Can_override_navigations_as_properties()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();

            var customer = model.FindEntityType(typeof(Customer))!;
            Assert.NotNull(customer.FindNavigation(nameof(Customer.Orders)));

            modelBuilder.Entity<Customer>().Property(c => c.Orders);

            Assert.Null(customer.FindNavigation(nameof(Customer.Orders)));
            Assert.NotNull(customer.FindProperty(nameof(Customer.Orders)));
        }

        [ConditionalFact]
        public virtual void Ignoring_a_navigation_property_removes_discovered_entity_types()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>(
                b =>
                {
                    b.Ignore(c => c.Details);
                    b.Ignore(c => c.Orders);
                });

            var model = modelBuilder.FinalizeModel();

            Assert.Single(model.GetEntityTypes());
        }

        [ConditionalFact]
        public virtual void Ignoring_a_navigation_property_removes_discovered_relationship()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>(
                b =>
                {
                    b.Ignore(c => c.Details);
                    b.Ignore(c => c.Orders);
                });
            modelBuilder.Entity<CustomerDetails>(b => b.Ignore(c => c.Customer));

            var model = modelBuilder.FinalizeModel();

            Assert.Empty(model.GetEntityTypes().First().GetForeignKeys());
            Assert.Empty(model.GetEntityTypes().Last().GetForeignKeys());
            Assert.Equal(2, model.GetEntityTypes().Count());
        }

        [ConditionalFact]
        public virtual void Ignoring_a_base_type_removes_relationships()
        {
            var modelBuilder = CreateModelBuilder(c => c.IgnoreAny<INotifyPropertyChanged>());

            modelBuilder.Entity<Customer>();

            var model = modelBuilder.FinalizeModel();

            Assert.Empty(model.GetEntityTypes().Single().GetForeignKeys());
        }

        [ConditionalFact]
        public virtual void Properties_can_be_made_required()
        {
            var modelBuilder = CreateModelBuilder();

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

            var model = modelBuilder.FinalizeModel();
            var entityType = (IReadOnlyEntityType)model.FindEntityType(typeof(Quarks))!;

            Assert.False(entityType.FindProperty("Up")!.IsNullable);
            Assert.False(entityType.FindProperty("Down")!.IsNullable);
            Assert.False(entityType.FindProperty("Charm")!.IsNullable);
            Assert.False(entityType.FindProperty("Strange")!.IsNullable);
            Assert.False(entityType.FindProperty("Top")!.IsNullable);
            Assert.False(entityType.FindProperty("Bottom")!.IsNullable);
        }

        [ConditionalFact]
        public virtual void Properties_can_be_made_optional()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property(e => e.Down).IsRequired(false);
                    b.Property<string>("Strange").IsRequired(false);
                    b.Property<string>("Bottom").IsRequired(false);
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = (IReadOnlyEntityType)model.FindEntityType(typeof(Quarks))!;

            Assert.True(entityType.FindProperty("Down")!.IsNullable);
            Assert.True(entityType.FindProperty("Strange")!.IsNullable);
            Assert.True(entityType.FindProperty("Bottom")!.IsNullable);
        }

        [ConditionalFact]
        public virtual void Key_properties_cannot_be_made_optional()
            => Assert.Equal(
                CoreStrings.KeyPropertyCannotBeNullable(nameof(Quarks.Down), nameof(Quarks), "{'" + nameof(Quarks.Down) + "'}"),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        CreateModelBuilder().Entity<Quarks>(
                            b =>
                            {
                                b.HasAlternateKey(e => new { e.Down });
                                b.Property(e => e.Down).IsRequired(false);
                            })).Message);

        [ConditionalFact]
        public virtual void Non_nullable_properties_cannot_be_made_optional()
        {
            var modelBuilder = CreateModelBuilder();

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

            var model = modelBuilder.FinalizeModel();
            var entityType = (IReadOnlyEntityType)model.FindEntityType(typeof(Quarks))!;

            Assert.False(entityType.FindProperty("Up")!.IsNullable);
            Assert.False(entityType.FindProperty("Charm")!.IsNullable);
            Assert.False(entityType.FindProperty("Top")!.IsNullable);
        }

        [ConditionalFact]
        public virtual void Properties_specified_by_string_are_shadow_properties_unless_already_known_to_be_CLR_properties()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property<int>("Up");
                    b.Property<int>("Gluon");
                    b.Property<string>("Down");
                    b.Property<string>("Photon");
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(Quarks))!;

            Assert.False(entityType.FindProperty("Up")!.IsShadowProperty());
            Assert.False(entityType.FindProperty("Down")!.IsShadowProperty());
            Assert.True(entityType.FindProperty("Gluon")!.IsShadowProperty());
            Assert.True(entityType.FindProperty("Photon")!.IsShadowProperty());

            Assert.Equal(-1, entityType.FindProperty("Up")!.GetShadowIndex());
            Assert.Equal(-1, entityType.FindProperty("Down")!.GetShadowIndex());
            Assert.NotEqual(-1, entityType.FindProperty("Gluon")!.GetShadowIndex());
            Assert.NotEqual(-1, entityType.FindProperty("Photon")!.GetShadowIndex());
            Assert.NotEqual(entityType.FindProperty("Gluon")!.GetShadowIndex(), entityType.FindProperty("Photon")!.GetShadowIndex());
        }

        [ConditionalFact]
        public virtual void Properties_can_be_made_concurrency_tokens()
        {
            var modelBuilder = CreateModelBuilder();

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

            var model = modelBuilder.FinalizeModel();
            var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(Quarks))!;

            Assert.False(entityType.FindProperty(Customer.IdProperty.Name)!.IsConcurrencyToken);
            Assert.True(entityType.FindProperty("Up")!.IsConcurrencyToken);
            Assert.False(entityType.FindProperty("Down")!.IsConcurrencyToken);
            Assert.True(entityType.FindProperty("Charm")!.IsConcurrencyToken);
            Assert.False(entityType.FindProperty("Strange")!.IsConcurrencyToken);
            Assert.True(entityType.FindProperty("Top")!.IsConcurrencyToken);
            Assert.False(entityType.FindProperty("Bottom")!.IsConcurrencyToken);

            Assert.Equal(0, entityType.FindProperty(Customer.IdProperty.Name)!.GetOriginalValueIndex());
            Assert.Equal(3, entityType.FindProperty("Up")!.GetOriginalValueIndex());
            Assert.Equal(-1, entityType.FindProperty("Down")!.GetOriginalValueIndex());
            Assert.Equal(1, entityType.FindProperty("Charm")!.GetOriginalValueIndex());
            Assert.Equal(-1, entityType.FindProperty("Strange")!.GetOriginalValueIndex());
            Assert.Equal(2, entityType.FindProperty("Top")!.GetOriginalValueIndex());
            Assert.Equal(-1, entityType.FindProperty("Bottom")!.GetOriginalValueIndex());

            Assert.Equal(ChangeTrackingStrategy.ChangingAndChangedNotifications, entityType.GetChangeTrackingStrategy());
        }

        [ConditionalFact]
        public virtual void Properties_can_have_access_mode_set()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property(e => e.Up);
                    b.Property(e => e.Down).HasField("_forDown").UsePropertyAccessMode(PropertyAccessMode.Field);
                    b.Property<int>("Charm").UsePropertyAccessMode(PropertyAccessMode.Property);
                    b.Property<string>("Strange").UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = (IReadOnlyEntityType)model.FindEntityType(typeof(Quarks))!;

            Assert.Equal(PropertyAccessMode.PreferField, entityType.FindProperty("Up")!.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.Field, entityType.FindProperty("Down")!.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.Property, entityType.FindProperty("Charm")!.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.FieldDuringConstruction, entityType.FindProperty("Strange")!.GetPropertyAccessMode());
        }

        [ConditionalFact]
        public virtual void Access_mode_can_be_overridden_at_entity_and_property_levels()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Field);

            modelBuilder.Entity<Hob>(
                b =>
                {
                    b.HasKey(e => e.Id1);
                });
            modelBuilder.Ignore<Nob>();

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);
                    b.Property(e => e.Up).UsePropertyAccessMode(PropertyAccessMode.Property);
                    b.Property(e => e.Down).HasField("_forDown");
                });

            var model = modelBuilder.FinalizeModel();
            Assert.Equal(PropertyAccessMode.Field, model.GetPropertyAccessMode());

            var hobsType = (IReadOnlyEntityType)model.FindEntityType(typeof(Hob))!;
            Assert.Equal(PropertyAccessMode.Field, hobsType.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.Field, hobsType.FindProperty("Id1")!.GetPropertyAccessMode());

            var quarksType = (IReadOnlyEntityType)model.FindEntityType(typeof(Quarks))!;
            Assert.Equal(PropertyAccessMode.FieldDuringConstruction, quarksType.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.FieldDuringConstruction, quarksType.FindProperty("Down")!.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.Property, quarksType.FindProperty("Up")!.GetPropertyAccessMode());
        }

        [ConditionalFact]
        public virtual void Properties_can_have_provider_type_set()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property(e => e.Up);
                    b.Property(e => e.Down).HasConversion<byte[]>();
                    b.Property<int>("Charm").HasConversion<long, CustomValueComparer<int>>();
                    b.Property<string>("Strange").HasConversion<byte[]>(
                        new CustomValueComparer<string>(), new CustomValueComparer<byte[]>());
                    b.Property<string>("Strange").HasConversion(null);
                    b.Property<string>("Top").HasConversion<string>(new CustomValueComparer<string>());
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = (IReadOnlyEntityType)model.FindEntityType(typeof(Quarks))!;

            var up = entityType.FindProperty("Up")!;
            Assert.Null(up.GetProviderClrType());
            Assert.True(up.GetValueComparer()?.IsDefault());

            var down = entityType.FindProperty("Down")!;
            Assert.Same(typeof(byte[]), down.GetProviderClrType());
            Assert.True(down.GetValueComparer()?.IsDefault());
            Assert.IsType<ValueComparer<byte[]>>(down.GetProviderValueComparer());

            var charm = entityType.FindProperty("Charm")!;
            Assert.Same(typeof(long), charm.GetProviderClrType());
            Assert.IsType<CustomValueComparer<int>>(charm.GetValueComparer());
            Assert.True(charm.GetProviderValueComparer()?.IsDefault());

            var strange = entityType.FindProperty("Strange")!;
            Assert.Null(strange.GetProviderClrType());
            Assert.True(strange.GetValueComparer()?.IsDefault());
            Assert.True(strange.GetProviderValueComparer()?.IsDefault());

            var top = entityType.FindProperty("Top")!;
            Assert.Same(typeof(string), top.GetProviderClrType());
            Assert.IsType<CustomValueComparer<string>>(top.GetValueComparer());
            Assert.IsType<CustomValueComparer<string>>(top.GetProviderValueComparer());
        }

        [ConditionalFact]
        public virtual void Properties_can_have_provider_type_set_for_type()
        {
            var modelBuilder = CreateModelBuilder(c => c.Properties<string>().HaveConversion<byte[]>());

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property(e => e.Up);
                    b.Property(e => e.Down);
                    b.Property<int>("Charm");
                    b.Property<string>("Strange");
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = (IReadOnlyEntityType)model.FindEntityType(typeof(Quarks))!;

            Assert.Null(entityType.FindProperty("Up")!.GetProviderClrType());
            Assert.Same(typeof(byte[]), entityType.FindProperty("Down")!.GetProviderClrType());
            Assert.Null(entityType.FindProperty("Charm")!.GetProviderClrType());
            Assert.Same(typeof(byte[]), entityType.FindProperty("Strange")!.GetProviderClrType());
        }

        [ConditionalFact]
        public virtual void Properties_can_have_non_generic_value_converter_set()
        {
            var modelBuilder = CreateModelBuilder();

            ValueConverter stringConverter = new StringToBytesConverter(Encoding.UTF8);
            ValueConverter intConverter = new CastingConverter<int, long>();

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property(e => e.Up);
                    b.Property(e => e.Down).HasConversion(stringConverter);
                    b.Property<int>("Charm").HasConversion(intConverter, null, new CustomValueComparer<long>());
                    b.Property<string>("Strange").HasConversion(stringConverter);
                    b.Property<string>("Strange").HasConversion(null);
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = (IReadOnlyEntityType)model.FindEntityType(typeof(Quarks))!;

            Assert.Null(entityType.FindProperty("Up")!.GetValueConverter());

            var down = entityType.FindProperty("Down")!;
            Assert.Same(stringConverter, down.GetValueConverter());
            Assert.True(down.GetValueComparer()?.IsDefault());
            Assert.IsType<ValueComparer<byte[]>>(down.GetProviderValueComparer());

            var charm = entityType.FindProperty("Charm")!;
            Assert.Same(intConverter, charm.GetValueConverter());
            Assert.True(charm.GetValueComparer()?.IsDefault());
            Assert.IsType<CustomValueComparer<long>>(charm.GetProviderValueComparer());

            Assert.Null(entityType.FindProperty("Strange")!.GetValueConverter());
        }

        [ConditionalFact]
        public virtual void Properties_can_have_custom_type_value_converter_type_set()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property(e => e.Up).HasConversion<int, CustomValueComparer<int>>();
                    b.Property(e => e.Down)
                        .HasConversion<UTF8StringToBytesConverter, CustomValueComparer<string>, CustomValueComparer<byte[]>>();
                    b.Property<int>("Charm").HasConversion<CastingConverter<int, long>, CustomValueComparer<int>>();
                    b.Property<string>("Strange").HasConversion<UTF8StringToBytesConverter, CustomValueComparer<string>>();
                    b.Property<string>("Strange").HasConversion(null, null);
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = (IReadOnlyEntityType)model.FindEntityType(typeof(Quarks))!;

            var up = entityType.FindProperty("Up")!;
            Assert.Equal(typeof(int), up.GetProviderClrType());
            Assert.Null(up.GetValueConverter());
            Assert.IsType<CustomValueComparer<int>>(up.GetValueComparer());
            Assert.IsType<CustomValueComparer<int>>(up.GetProviderValueComparer());

            var down = entityType.FindProperty("Down")!;
            Assert.IsType<UTF8StringToBytesConverter>(down.GetValueConverter());
            Assert.IsType<CustomValueComparer<string>>(down.GetValueComparer());
            Assert.IsType<CustomValueComparer<byte[]>>(down.GetProviderValueComparer());

            var charm = entityType.FindProperty("Charm")!;
            Assert.IsType<CastingConverter<int, long>>(charm.GetValueConverter());
            Assert.IsType<CustomValueComparer<int>>(charm.GetValueComparer());
            Assert.True(charm.GetProviderValueComparer()?.IsDefault());

            var strange = entityType.FindProperty("Strange")!;
            Assert.Null(strange.GetValueConverter());
            Assert.True(strange.GetValueComparer()?.IsDefault());
            Assert.True(strange.GetProviderValueComparer()?.IsDefault());
        }

        private class UTF8StringToBytesConverter : StringToBytesConverter
        {
            public UTF8StringToBytesConverter()
                : base(Encoding.UTF8)
            {
            }
        }

        private class CustomValueComparer<T> : ValueComparer<T>
        {
            public CustomValueComparer()
                : base(false)
            {
            }
        }

        [ConditionalFact]
        public virtual void Properties_can_have_value_converter_set_inline()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property(e => e.Up);
                    b.Property(e => e.Down).HasConversion(v => int.Parse(v!), v => v.ToString());
                    b.Property<int>("Charm").HasConversion(v => (long)v, v => (int)v, new CustomValueComparer<int>());
                    b.Property<float>("Strange").HasConversion(
                        v => (double)v, v => (float)v, new CustomValueComparer<float>(), new CustomValueComparer<double>());
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(Quarks))!;

            var up = entityType.FindProperty("Up")!;
            Assert.Null(up.GetProviderClrType());
            Assert.Null(up.GetValueConverter());
            Assert.True(up.GetValueComparer()?.IsDefault());
            Assert.True(up.GetProviderValueComparer()?.IsDefault());

            var down = entityType.FindProperty("Down")!;
            Assert.IsType<ValueConverter<string, int>>(down.GetValueConverter());
            Assert.True(down.GetValueComparer()?.IsDefault());
            Assert.True(down.GetProviderValueComparer()?.IsDefault());

            var charm = entityType.FindProperty("Charm")!;
            Assert.IsType<ValueConverter<int, long>>(charm.GetValueConverter());
            Assert.IsType<CustomValueComparer<int>>(charm.GetValueComparer());
            Assert.True(charm.GetProviderValueComparer()?.IsDefault());

            var strange = entityType.FindProperty("Strange")!;
            Assert.IsType<ValueConverter<float, double>>(strange.GetValueConverter());
            Assert.IsType<CustomValueComparer<float>>(strange.GetValueComparer());
            Assert.IsType<CustomValueComparer<double>>(strange.GetProviderValueComparer());
        }

        [ConditionalFact]
        public virtual void Properties_can_have_value_converter_set()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property(e => e.Up);
                    b.Property(e => e.Down).HasConversion(
                        new ValueConverter<string, int>(v => int.Parse(v), v => v.ToString())!);
                    b.Property<int>("Charm").HasConversion(
                        new ValueConverter<int, long>(v => v, v => (int)v), new CustomValueComparer<int>());
                    b.Property<float>("Strange").HasConversion(
                        new ValueConverter<float, double>(v => v, v => (float)v), new CustomValueComparer<float>(),
                        new CustomValueComparer<double>());
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(Quarks))!;

            var up = entityType.FindProperty("Up")!;
            Assert.Null(up.GetProviderClrType());
            Assert.Null(up.GetValueConverter());
            Assert.True(up.GetValueComparer()?.IsDefault());
            Assert.True(up.GetProviderValueComparer()?.IsDefault());

            var down = entityType.FindProperty("Down")!;
            Assert.IsType<ValueConverter<string, int>>(down.GetValueConverter());
            Assert.True(down.GetValueComparer()?.IsDefault());
            Assert.True(down.GetProviderValueComparer()?.IsDefault());

            var charm = entityType.FindProperty("Charm")!;
            Assert.IsType<ValueConverter<int, long>>(charm.GetValueConverter());
            Assert.IsType<CustomValueComparer<int>>(charm.GetValueComparer());
            Assert.True(charm.GetProviderValueComparer()?.IsDefault());

            var strange = entityType.FindProperty("Strange")!;
            Assert.IsType<ValueConverter<float, double>>(strange.GetValueConverter());
            Assert.IsType<CustomValueComparer<float>>(strange.GetValueComparer());
            Assert.IsType<CustomValueComparer<double>>(strange.GetProviderValueComparer());
        }

        [ConditionalFact]
        public virtual void IEnumerable_properties_with_value_converter_set_are_not_discovered_as_navigations()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<DynamicProperty>(
                b =>
                {
                    b.Property(e => e.ExpandoObject).HasConversion(
                        v => (string)((IDictionary<string, object>)v!)["Value"], v => DeserializeExpandoObject(v));

                    var comparer = new ValueComparer<ExpandoObject>(
                        (v1, v2) => v1!.SequenceEqual(v2!),
                        v => v.GetHashCode());

                    b.Property(e => e.ExpandoObject).Metadata.SetValueComparer(comparer);
                });

            var model = modelBuilder.FinalizeModel();

            var entityType = (IReadOnlyEntityType)model.GetEntityTypes().Single();
            Assert.NotNull(entityType.FindProperty(nameof(DynamicProperty.ExpandoObject))!.GetValueConverter());
            Assert.NotNull(entityType.FindProperty(nameof(DynamicProperty.ExpandoObject))!.GetValueComparer());
        }

        private static ExpandoObject DeserializeExpandoObject(string value)
        {
            dynamic obj = new ExpandoObject();
            obj.Value = value;

            return obj;
        }

        private class ExpandoObjectConverter : ValueConverter<ExpandoObject, string>
        {
            public ExpandoObjectConverter()
                : base(v => (string)((IDictionary<string, object>)v!)["Value"], v => DeserializeExpandoObject(v))
            {
            }
        }

        private class ExpandoObjectComparer : ValueComparer<ExpandoObject>
        {
            public ExpandoObjectComparer()
                : base((v1, v2) => v1!.SequenceEqual(v2!), v => v.GetHashCode())
            {
            }
        }

        [ConditionalFact]
        public virtual void Properties_can_have_value_converter_configured_by_type()
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                {
                    c.Properties(typeof(IWrapped<>)).AreUnicode(false);
                    c.Properties<WrappedStringBase>().HaveMaxLength(20);
                    c.Properties<WrappedString>().HaveConversion(typeof(WrappedStringToStringConverter));
                });

            modelBuilder.Entity<WrappedStringEntity>();

            var model = modelBuilder.FinalizeModel();

            var entityType = (IReadOnlyEntityType)model.GetEntityTypes().Single();
            var wrappedProperty = entityType.FindProperty(nameof(WrappedStringEntity.WrappedString))!;
            Assert.False(wrappedProperty.IsUnicode());
            Assert.Equal(20, wrappedProperty.GetMaxLength());
            Assert.IsType<WrappedStringToStringConverter>(wrappedProperty.GetValueConverter());
            Assert.IsType<ValueComparer<WrappedString>>(wrappedProperty.GetValueComparer());
        }

        [ConditionalFact]
        public virtual void Value_converter_configured_on_non_nullable_type_is_applied()
        {
            var modelBuilder = CreateModelBuilder(
                c => c.Properties<int>().HaveConversion<NumberToStringConverter<int>, CustomValueComparer<int>>());

            modelBuilder.Entity<Quarks>(
                b => b.Property<int?>("Wierd"));

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(Quarks))!;

            var id = entityType.FindProperty("Id")!;
            Assert.IsType<NumberToStringConverter<int>>(id.GetValueConverter());
            Assert.IsType<CustomValueComparer<int>>(id.GetValueComparer());

            var wierd = entityType.FindProperty("Wierd")!;
            Assert.IsType<NumberToStringConverter<int>>(wierd.GetValueConverter());
            Assert.IsType<ValueComparer<int?>>(wierd.GetValueComparer());
        }

        [ConditionalFact]
        public virtual void Value_converter_configured_on_nullable_type_overrides_non_nullable()
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                {
                    c.Properties<int?>().HaveConversion<NumberToStringConverter<int?>, CustomValueComparer<int?>>();
                    c.Properties<int>()
                        .HaveConversion<NumberToStringConverter<int>, CustomValueComparer<int>, CustomValueComparer<string>>();
                });

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property<int?>("Wierd");
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(Quarks))!;

            var id = entityType.FindProperty("Id")!;
            Assert.IsType<NumberToStringConverter<int>>(id.GetValueConverter());
            Assert.IsType<CustomValueComparer<int>>(id.GetValueComparer());
            Assert.IsType<CustomValueComparer<string>>(id.GetProviderValueComparer());

            var wierd = entityType.FindProperty("Wierd")!;
            Assert.IsType<NumberToStringConverter<int?>>(wierd.GetValueConverter());
            Assert.IsType<CustomValueComparer<int?>>(wierd.GetValueComparer());
            Assert.IsType<CustomValueComparer<string>>(wierd.GetProviderValueComparer());
        }

        [ConditionalFact]
        public virtual void Value_converter_configured_on_base_type_is_not_applied()
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                {
                    c.Properties<WrappedStringBase>().HaveConversion(typeof(WrappedStringToStringConverter));
                });

            modelBuilder.Entity<WrappedStringEntity>();

            Assert.Equal(
                CoreStrings.PropertyNotMapped(
                    nameof(WrappedString),
                    nameof(WrappedStringEntity),
                    nameof(WrappedStringEntity.WrappedString)),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
        }

        private class WrappedStringToStringConverter : ValueConverter<WrappedString, string>
        {
            public WrappedStringToStringConverter()
                : base(v => v.Value!, v => new WrappedString { Value = v })
            {
            }
        }

        [ConditionalFact]
        public virtual void Throws_for_conflicting_base_configurations_by_type()
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                {
                    c.Properties<WrappedString>();
                    c.IgnoreAny<IWrapped<string>>();
                });

            Assert.Equal(
                CoreStrings.TypeConfigurationConflict(
                    nameof(WrappedString), "Property",
                    "IWrapped<string>", "Ignored"),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.Entity<WrappedStringEntity>()).Message);
        }

        [ConditionalFact]
        public virtual void Value_converter_type_is_checked()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    Assert.Equal(
                        CoreStrings.ConverterPropertyMismatch("string", "Quarks", "Up", "int"),
                        Assert.Throws<InvalidOperationException>(
                            () => b.Property(e => e.Up).HasConversion(
                                new StringToBytesConverter(Encoding.UTF8))).Message);
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(Quarks))!;
            Assert.Null(entityType.FindProperty("Up")!.GetValueConverter());
        }

        [ConditionalFact]
        public virtual void Properties_can_have_field_set()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property<int>("Up").HasField("_forUp");
                    b.Property(e => e.Down).HasField("_forDown");
                    b.Property<int?>("_forWierd").HasField("_forWierd");
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(Quarks))!;

            Assert.Equal("_forUp", entityType.FindProperty("Up")!.GetFieldName());
            Assert.Equal("_forDown", entityType.FindProperty("Down")!.GetFieldName());
            Assert.Equal("_forWierd", entityType.FindProperty("_forWierd")!.GetFieldName());
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
                    b.Property<int>("Charm").Metadata.ValueGenerated = ValueGenerated.OnUpdateSometimes;
                    b.Property<string>("Strange").ValueGeneratedNever();
                    b.Property<int>("Top").ValueGeneratedOnAddOrUpdate();
                    b.Property<string>("Bottom").ValueGeneratedOnUpdate();
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(Quarks))!;
            Assert.Equal(ValueGenerated.OnAdd, entityType.FindProperty(Customer.IdProperty.Name)!.ValueGenerated);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, entityType.FindProperty("Up")!.ValueGenerated);
            Assert.Equal(ValueGenerated.Never, entityType.FindProperty("Down")!.ValueGenerated);
            Assert.Equal(ValueGenerated.OnUpdateSometimes, entityType.FindProperty("Charm")!.ValueGenerated);
            Assert.Equal(ValueGenerated.Never, entityType.FindProperty("Strange")!.ValueGenerated);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, entityType.FindProperty("Top")!.ValueGenerated);
            Assert.Equal(ValueGenerated.OnUpdate, entityType.FindProperty("Bottom")!.ValueGenerated);
        }

        [ConditionalFact]
        public virtual void Properties_can_set_row_version()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.HasKey(e => e.Id);
                    b.Property(e => e.Up).IsRowVersion();
                    b.Property(e => e.Down).ValueGeneratedNever();
                    b.Property<int>("Charm").IsRowVersion();
                });

            var model = modelBuilder.FinalizeModel();

            var entityType = model.FindEntityType(typeof(Quarks))!;

            Assert.Equal(ValueGenerated.OnAddOrUpdate, entityType.FindProperty("Up")!.ValueGenerated);
            Assert.Equal(ValueGenerated.Never, entityType.FindProperty("Down")!.ValueGenerated);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, entityType.FindProperty("Charm")!.ValueGenerated);

            Assert.True(entityType.FindProperty("Up")!.IsConcurrencyToken);
            Assert.False(entityType.FindProperty("Down")!.IsConcurrencyToken);
            Assert.True(entityType.FindProperty("Charm")!.IsConcurrencyToken);
        }

        [ConditionalFact]
        public virtual void Can_set_max_length_for_properties()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property(e => e.Up).HasMaxLength(0);
                    b.Property(e => e.Down).HasMaxLength(100);
                    b.Property<int>("Charm").HasMaxLength(0);
                    b.Property<string>("Strange").HasMaxLength(-1);
                    b.Property<int>("Top").HasMaxLength(0);
                    b.Property<string>("Bottom").HasMaxLength(100);
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(Quarks))!;

            Assert.Null(entityType.FindProperty(Customer.IdProperty.Name)!.GetMaxLength());
            Assert.Equal(0, entityType.FindProperty("Up")!.GetMaxLength());
            Assert.Equal(100, entityType.FindProperty("Down")!.GetMaxLength());
            Assert.Equal(0, entityType.FindProperty("Charm")!.GetMaxLength());
            Assert.Equal(-1, entityType.FindProperty("Strange")!.GetMaxLength());
            Assert.Equal(0, entityType.FindProperty("Top")!.GetMaxLength());
            Assert.Equal(100, entityType.FindProperty("Bottom")!.GetMaxLength());
        }

        [ConditionalFact]
        public virtual void Can_set_max_length_for_property_type()
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                {
                    c.Properties<int>().HaveMaxLength(0);
                    c.Properties<string>().HaveMaxLength(100);
                });

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property<int>("Charm");
                    b.Property<string>("Strange");
                    b.Property<int>("Top");
                    b.Property<string>("Bottom");
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(Quarks))!;

            Assert.Equal(0, entityType.FindProperty(Customer.IdProperty.Name)!.GetMaxLength());
            Assert.Equal(0, entityType.FindProperty("Up")!.GetMaxLength());
            Assert.Equal(100, entityType.FindProperty("Down")!.GetMaxLength());
            Assert.Equal(0, entityType.FindProperty("Charm")!.GetMaxLength());
            Assert.Equal(100, entityType.FindProperty("Strange")!.GetMaxLength());
            Assert.Equal(0, entityType.FindProperty("Top")!.GetMaxLength());
            Assert.Equal(100, entityType.FindProperty("Bottom")!.GetMaxLength());
        }

        [ConditionalFact]
        public virtual void Can_set_sentinel_for_properties()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property(e => e.Up).HasSentinel(1);
                    b.Property(e => e.Down).HasSentinel("100");
                    b.Property<int>("Charm").HasSentinel((sbyte)-1);
                    b.Property<string>("Strange").HasSentinel("");
                    b.Property<int>("Top").HasSentinel(77);
                    b.Property<string>("Bottom").HasSentinel(null);
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(Quarks))!;

            Assert.Equal(0, entityType.FindProperty(Customer.IdProperty.Name)!.Sentinel);
            Assert.Equal(1, entityType.FindProperty("Up")!.Sentinel);
            Assert.Equal("100", entityType.FindProperty("Down")!.Sentinel);
            Assert.Equal(-1, entityType.FindProperty("Charm")!.Sentinel);
            Assert.Equal("", entityType.FindProperty("Strange")!.Sentinel);
            Assert.Equal(77, entityType.FindProperty("Top")!.Sentinel);
            Assert.Null(entityType.FindProperty("Bottom")!.Sentinel);
        }

        [ConditionalFact]
        public virtual void Setting_sentinel_throws_for_null_on_nonnullable()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property<int>("Top").Metadata.Sentinel = 77D;
                    b.Property<int>("Charm").Metadata.Sentinel = EnumerablePartitionerOptions.NoBuffering;
                    Assert.Equal(CoreStrings.IncompatibleSentinelValue("null", nameof(Quarks), nameof(Quarks.Up), "int"),
                        Assert.Throws<InvalidOperationException>(() => b.Property(e => e.Up).Metadata.Sentinel = null).Message);
                });
        }

        [ConditionalFact]
        public virtual void Setting_sentinel_throws_for_noncompatible()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    Assert.Equal(CoreStrings.IncompatibleSentinelValue("System.Byte[]", nameof(Quarks), nameof(Quarks.Up), "int"),
                        Assert.Throws<InvalidOperationException>(() => b.Property(e => e.Up).Metadata.Sentinel = Array.Empty<byte>()).Message);
                });
        }

        [ConditionalFact]
        public virtual void Can_set_sentinel_for_property_type()
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                {
                    c.Properties<int>().HaveSentinel(-1);
                    c.Properties<string>().HaveSentinel("100");
                });

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property<int>("Charm");
                    b.Property<string>("Strange");
                    b.Property<int>("Top");
                    b.Property<string>("Bottom");
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(Quarks))!;

            Assert.Equal(-1, entityType.FindProperty(Customer.IdProperty.Name)!.Sentinel);
            Assert.Equal(-1, entityType.FindProperty("Up")!.Sentinel);
            Assert.Equal("100", entityType.FindProperty("Down")!.Sentinel);
            Assert.Equal(-1, entityType.FindProperty("Charm")!.Sentinel);
            Assert.Equal("100", entityType.FindProperty("Strange")!.Sentinel);
            Assert.Equal(-1, entityType.FindProperty("Top")!.Sentinel);
            Assert.Equal("100", entityType.FindProperty("Bottom")!.Sentinel);
        }

        [ConditionalFact]
        public virtual void Can_set_unbounded_max_length_for_property_type()
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                {
                    c.Properties<int>().HaveMaxLength(0);
                    c.Properties<string>().HaveMaxLength(-1);
                });

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property<int>("Charm");
                    b.Property<string>("Strange");
                    b.Property<int>("Top");
                    b.Property<string>("Bottom");
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(Quarks))!;

            Assert.Equal(0, entityType.FindProperty(Customer.IdProperty.Name)!.GetMaxLength());
            Assert.Equal(0, entityType.FindProperty("Up")!.GetMaxLength());
            Assert.Equal(-1, entityType.FindProperty("Down")!.GetMaxLength());
            Assert.Equal(0, entityType.FindProperty("Charm")!.GetMaxLength());
            Assert.Equal(-1, entityType.FindProperty("Strange")!.GetMaxLength());
            Assert.Equal(0, entityType.FindProperty("Top")!.GetMaxLength());
            Assert.Equal(-1, entityType.FindProperty("Bottom")!.GetMaxLength());
        }

        [ConditionalFact]
        public virtual void Can_set_precision_and_scale_for_properties()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property(e => e.Up).HasPrecision(1, 0);
                    b.Property(e => e.Down).HasPrecision(100, 10);
                    b.Property<int>("Charm").HasPrecision(1, 0);
                    b.Property<string>("Strange").HasPrecision(100, 10);
                    b.Property<int>("Top").HasPrecision(1, 0);
                    b.Property<string>("Bottom").HasPrecision(100, 10);
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(Quarks))!;

            Assert.Null(entityType.FindProperty(Customer.IdProperty.Name)!.GetPrecision());
            Assert.Null(entityType.FindProperty(Customer.IdProperty.Name)!.GetScale());
            Assert.Equal(1, entityType.FindProperty("Up")!.GetPrecision());
            Assert.Equal(0, entityType.FindProperty("Up")!.GetScale());
            Assert.Equal(100, entityType.FindProperty("Down")!.GetPrecision());
            Assert.Equal(10, entityType.FindProperty("Down")!.GetScale());
            Assert.Equal(1, entityType.FindProperty("Charm")!.GetPrecision());
            Assert.Equal(0, entityType.FindProperty("Charm")!.GetScale());
            Assert.Equal(100, entityType.FindProperty("Strange")!.GetPrecision());
            Assert.Equal(10, entityType.FindProperty("Strange")!.GetScale());
            Assert.Equal(1, entityType.FindProperty("Top")!.GetPrecision());
            Assert.Equal(0, entityType.FindProperty("Top")!.GetScale());
            Assert.Equal(100, entityType.FindProperty("Bottom")!.GetPrecision());
            Assert.Equal(10, entityType.FindProperty("Bottom")!.GetScale());
        }

        [ConditionalFact]
        public virtual void Can_set_precision_and_scale_for_property_type()
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                {
                    c.Properties<int>().HavePrecision(1, 0);
                    c.Properties<string>().HavePrecision(100, 10);
                });

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property<int>("Charm");
                    b.Property<string>("Strange");
                    b.Property<int>("Top");
                    b.Property<string>("Bottom");
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(Quarks))!;

            Assert.Equal(1, entityType.FindProperty(Customer.IdProperty.Name)!.GetPrecision());
            Assert.Equal(0, entityType.FindProperty(Customer.IdProperty.Name)!.GetScale());
            Assert.Equal(1, entityType.FindProperty("Up")!.GetPrecision());
            Assert.Equal(0, entityType.FindProperty("Up")!.GetScale());
            Assert.Equal(100, entityType.FindProperty("Down")!.GetPrecision());
            Assert.Equal(10, entityType.FindProperty("Down")!.GetScale());
            Assert.Equal(1, entityType.FindProperty("Charm")!.GetPrecision());
            Assert.Equal(0, entityType.FindProperty("Charm")!.GetScale());
            Assert.Equal(100, entityType.FindProperty("Strange")!.GetPrecision());
            Assert.Equal(10, entityType.FindProperty("Strange")!.GetScale());
            Assert.Equal(1, entityType.FindProperty("Top")!.GetPrecision());
            Assert.Equal(0, entityType.FindProperty("Top")!.GetScale());
            Assert.Equal(100, entityType.FindProperty("Bottom")!.GetPrecision());
            Assert.Equal(10, entityType.FindProperty("Bottom")!.GetScale());
        }

        [ConditionalFact]
        public virtual void Can_set_custom_value_generator_for_properties()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property(e => e.Up).HasValueGenerator<CustomValueGenerator>();
                    b.Property(e => e.Down).HasValueGenerator(typeof(CustomValueGenerator));
                    b.Property<int>("Charm").HasValueGenerator((_, __) => new CustomValueGenerator());
                    b.Property<string>("Strange").HasValueGenerator<CustomValueGenerator>();
                    b.Property<int>("Top").HasValueGeneratorFactory(typeof(CustomValueGeneratorFactory));
                    b.Property<string>("Bottom").HasValueGeneratorFactory<CustomValueGeneratorFactory>();
                });

            var model = modelBuilder.FinalizeModel();

            var entityType = model.FindEntityType(typeof(Quarks))!;

            Assert.Null(entityType.FindProperty(Customer.IdProperty.Name)!.GetValueGeneratorFactory());
            Assert.IsType<CustomValueGenerator>(entityType.FindProperty("Up")!.GetValueGeneratorFactory()!(null!, null!));
            Assert.IsType<CustomValueGenerator>(entityType.FindProperty("Down")!.GetValueGeneratorFactory()!(null!, null!));
            Assert.IsType<CustomValueGenerator>(entityType.FindProperty("Charm")!.GetValueGeneratorFactory()!(null!, null!));
            Assert.IsType<CustomValueGenerator>(entityType.FindProperty("Strange")!.GetValueGeneratorFactory()!(null!, null!));
            Assert.IsType<CustomValueGenerator>(entityType.FindProperty("Top")!.GetValueGeneratorFactory()!(null!, null!));
            Assert.IsType<CustomValueGenerator>(entityType.FindProperty("Bottom")!.GetValueGeneratorFactory()!(null!, null!));
        }

        private class CustomValueGenerator : ValueGenerator<int>
        {
            public override int Next(EntityEntry entry)
                => throw new NotImplementedException();

            public override bool GeneratesTemporaryValues
                => false;
        }

        private class CustomValueGeneratorFactory : ValueGeneratorFactory
        {
            public override ValueGenerator Create(IProperty property, ITypeBase entityType)
                => new CustomValueGenerator();
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

            var entityType = model.FindEntityType(typeof(Quarks))!;

            Assert.Equal(
                CoreStrings.CannotCreateValueGenerator(nameof(BadCustomValueGenerator1), "HasValueGenerator"),
                Assert.Throws<InvalidOperationException>(
                    () => entityType.FindProperty("Up")!.GetValueGeneratorFactory()!(null!, null!)).Message);

            Assert.Equal(
                CoreStrings.CannotCreateValueGenerator(nameof(BadCustomValueGenerator2), "HasValueGenerator"),
                Assert.Throws<InvalidOperationException>(
                    () => entityType.FindProperty("Down")!.GetValueGeneratorFactory()!(null!, null!)).Message);
        }

#pragma warning disable CS9113 // Parameter 'foo' is unread
        private class BadCustomValueGenerator1(string foo) : CustomValueGenerator
#pragma warning restore CS9113
        {
        }

        private abstract class BadCustomValueGenerator2 : CustomValueGenerator;

        protected class StringCollectionEntity
        {
            public ICollection<string>? Property { get; set; }
        }

        [ConditionalFact]
        public virtual void Object_cannot_be_configured_as_property()
            => Assert.Equal(
                CoreStrings.UnconfigurableType("Dictionary<string, object>", "Property", "SharedTypeEntityType", "object"),
                Assert.Throws<InvalidOperationException>(() => CreateModelBuilder(c => c.Properties<object>())).Message);

        [ConditionalFact]
        public virtual void Property_bag_cannot_be_configured_as_property()
        {
            Assert.Equal(
                CoreStrings.UnconfigurableType(
                    "Dictionary<string, object>", "Property", "SharedTypeEntityType", "Dictionary<string, object>"),
                Assert.Throws<InvalidOperationException>(() => CreateModelBuilder(c => c.Properties<Dictionary<string, object>>()))
                    .Message);

            Assert.Equal(
                CoreStrings.UnconfigurableType(
                    "Dictionary<string, object>", "Property", "SharedTypeEntityType", "IDictionary<string, object>"),
                Assert.Throws<InvalidOperationException>(() => CreateModelBuilder(c => c.Properties<IDictionary<string, object>>()))
                    .Message);
        }

        [ConditionalFact]
        protected virtual void Mapping_ignores_ignored_array()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<OneDee>().Ignore(e => e.One);

            var model = modelBuilder.FinalizeModel();

            Assert.Null(model.FindEntityType(typeof(OneDee))!.FindProperty("One"));
        }

        [ConditionalFact]
        protected virtual void Mapping_ignores_ignored_two_dimensional_array()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<TwoDee>().Ignore(e => e.Two);

            var model = modelBuilder.FinalizeModel();

            Assert.Null(model.FindEntityType(typeof(TwoDee))!.FindProperty("Two"));
        }

        [ConditionalFact]
        protected virtual void Mapping_throws_for_non_ignored_three_dimensional_array()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<ThreeDee>();

            Assert.Equal(
                CoreStrings.PropertyNotAdded(
                    typeof(ThreeDee).ShortDisplayName(), "Three", typeof(int[,,]).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(modelBuilder.FinalizeModel).Message);
        }

        [ConditionalFact]
        protected virtual void Mapping_ignores_ignored_three_dimensional_array()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<ThreeDee>().Ignore(e => e.Three);

            var model = modelBuilder.FinalizeModel();

            Assert.Null(model.FindEntityType(typeof(ThreeDee))!.FindProperty("Three"));
        }

        [ConditionalFact]
        public virtual void Private_property_is_not_discovered_by_convention()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Alpha>();
            modelBuilder.Entity<Gamma>();

            var model = modelBuilder.FinalizeModel();

            Assert.Empty(
                model.FindEntityType(typeof(Gamma))!.GetProperties()
                    .Where(p => p.Name == "PrivateProperty"));
        }

        [ConditionalFact]
        protected virtual void Throws_for_int_keyed_dictionary()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<IntDict>();

            Assert.Equal(
                CoreStrings.NavigationNotAdded(
                    nameof(IntDict), nameof(IntDict.Notes), typeof(Dictionary<int, string>).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.FinalizeModel()).Message);
        }

        protected class IntDict
        {
            public int Id { get; set; }
            public Dictionary<int, string>? Notes { get; set; }
        }

        [ConditionalFact]
        public virtual void Can_set_unicode_for_properties()
        {
            var modelBuilder = CreateModelBuilder();

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

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(Quarks))!;

            Assert.Null(entityType.FindProperty(Customer.IdProperty.Name)!.IsUnicode());
            Assert.True(entityType.FindProperty("Up")!.IsUnicode());
            Assert.False(entityType.FindProperty("Down")!.IsUnicode());
            Assert.True(entityType.FindProperty("Charm")!.IsUnicode());
            Assert.False(entityType.FindProperty("Strange")!.IsUnicode());
            Assert.True(entityType.FindProperty("Top")!.IsUnicode());
            Assert.False(entityType.FindProperty("Bottom")!.IsUnicode());
        }

        [ConditionalFact]
        public virtual void Can_set_unicode_for_property_type()
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                {
                    c.Properties<int>().AreUnicode();
                    c.Properties<string>().AreUnicode(false);
                });

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property<int>("Charm");
                    b.Property<string>("Strange");
                    b.Property<int>("Top");
                    b.Property<string>("Bottom");
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(Quarks))!;

            Assert.True(entityType.FindProperty(Customer.IdProperty.Name)!.IsUnicode());
            Assert.True(entityType.FindProperty("Up")!.IsUnicode());
            Assert.False(entityType.FindProperty("Down")!.IsUnicode());
            Assert.True(entityType.FindProperty("Charm")!.IsUnicode());
            Assert.False(entityType.FindProperty("Strange")!.IsUnicode());
            Assert.True(entityType.FindProperty("Top")!.IsUnicode());
            Assert.False(entityType.FindProperty("Bottom")!.IsUnicode());
        }

        [ConditionalFact]
        public virtual void PropertyBuilder_methods_can_be_chained()
            => CreateModelBuilder()
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
                .HasSentinel(0)
                .HasPrecision(10, 1)
                .HasValueGenerator<CustomValueGenerator>()
                .HasValueGenerator(typeof(CustomValueGenerator))
                .HasValueGeneratorFactory<CustomValueGeneratorFactory>()
                .HasValueGeneratorFactory(typeof(CustomValueGeneratorFactory))
                .HasValueGenerator((_, __) => null!)
                .IsRequired();

        [ConditionalFact]
        public virtual void Can_add_index()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Product>();
            modelBuilder
                .Entity<Customer>()
                .HasIndex(ix => ix.Name);

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(Customer))!;

            var index = entityType.GetIndexes().Single();
            Assert.Equal(Customer.NameProperty.Name, index.Properties.Single().Name);
        }

        [ConditionalFact]
        public virtual void Can_add_index_when_no_clr_property()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Product>();
            modelBuilder
                .Entity<Customer>(
                    b =>
                    {
                        b.Property<int>("Index");
                        b.HasIndex("Index");
                    });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(Customer))!;

            var index = entityType.GetIndexes().Single();
            Assert.Equal("Index", index.Properties.Single().Name);
        }

        [ConditionalFact]
        public virtual void Can_add_multiple_indexes()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Product>();
            var entityBuilder = modelBuilder.Entity<Customer>();
            entityBuilder.HasIndex(ix => ix.Id).IsUnique();
            entityBuilder.HasIndex(ix => ix.Name).HasAnnotation("A1", "V1");
            entityBuilder.HasIndex(ix => ix.Id, "Named");
            entityBuilder.HasIndex(ix => ix.Id, "Descending").IsDescending();

            var model = modelBuilder.FinalizeModel();
            AssertEqual(modelBuilder.Model, model);

            var entityType = model.FindEntityType(typeof(Customer))!;
            var idProperty = entityType.FindProperty(nameof(Customer.Id))!;
            var nameProperty = entityType.FindProperty(nameof(Customer.Name))!;

            Assert.Equal(4, entityType.GetIndexes().Count());
            var firstIndex = entityType.FindIndex(idProperty)!;
            Assert.True(firstIndex.IsUnique);
            var secondIndex = entityType.FindIndex(nameProperty)!;
            Assert.False(secondIndex.IsUnique);
            Assert.Equal("V1", secondIndex["A1"]);
            var namedIndex = entityType.FindIndex("Named")!;
            Assert.False(namedIndex.IsUnique);
            var descendingIndex = entityType.FindIndex("Descending")!;
            Assert.Equal([], descendingIndex.IsDescending);
        }

        [ConditionalFact]
        public virtual void Can_add_contained_indexes()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Product>();
            var entityBuilder = modelBuilder.Entity<Customer>();
            var firstIndexBuilder = entityBuilder.HasIndex(
                ix => new { ix.Id, ix.AlternateKey }).IsUnique();
            var secondIndexBuilder = entityBuilder.HasIndex(
                ix => new { ix.Id });

            var model = modelBuilder.FinalizeModel();
            var entityType = (IReadOnlyEntityType)model.FindEntityType(typeof(Customer))!;

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

            var entityType = (IReadOnlyEntityType)model.FindEntityType(typeof(EntityWithoutId))!;

            Assert.Null(entityType.FindPrimaryKey());

            entityBuilder.Property<int>("Id");

            Assert.NotNull(entityType.FindPrimaryKey());
            Assert.Equal(new[] { "Id" }, entityType.FindPrimaryKey()!.Properties.Select(p => p.Name));
        }

        [ConditionalFact]
        public virtual void Can_ignore_explicit_interface_implementation_property()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<EntityBase>().HasNoKey().Ignore(e => ((IEntityBase)e).Target);

            Assert.DoesNotContain(
                nameof(IEntityBase.Target),
                modelBuilder.Model.FindEntityType(typeof(EntityBase))!.GetProperties().Select(p => p.Name));

            modelBuilder.Entity<EntityBase>().Property(e => ((IEntityBase)e).Target);

            Assert.Contains(
                nameof(IEntityBase.Target),
                modelBuilder.Model.FindEntityType(typeof(EntityBase))!.GetProperties().Select(p => p.Name));
        }

        [ConditionalFact]
        public virtual void Can_set_key_on_an_entity_with_fields()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<EntityWithFields>().HasKey(e => e.Id);

            var model = modelBuilder.FinalizeModel();
            var entity = model.FindEntityType(typeof(EntityWithFields))!;
            var primaryKey = entity.FindPrimaryKey();
            Assert.NotNull(primaryKey);
            var property = Assert.Single(primaryKey.Properties);
            Assert.Equal(nameof(EntityWithFields.Id), property.Name);
            Assert.Null(property.PropertyInfo);
            Assert.NotNull(property.FieldInfo);
        }

        [ConditionalFact]
        public virtual void Can_set_composite_key_on_an_entity_with_fields()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<EntityWithFields>().HasKey(e => new { e.TenantId, e.CompanyId });

            var model = modelBuilder.FinalizeModel();
            var entity = model.FindEntityType(typeof(EntityWithFields))!;
            var primaryKeyProperties = entity.FindPrimaryKey()!.Properties;
            Assert.Equal(2, primaryKeyProperties.Count);
            var first = primaryKeyProperties[0];
            var second = primaryKeyProperties[1];
            Assert.Equal(nameof(EntityWithFields.TenantId), first.Name);
            Assert.Null(first.PropertyInfo);
            Assert.NotNull(first.FieldInfo);
            Assert.Equal(nameof(EntityWithFields.CompanyId), second.Name);
            Assert.Null(second.PropertyInfo);
            Assert.NotNull(second.FieldInfo);
        }

        [ConditionalFact]
        public virtual void Can_set_alternate_key_on_an_entity_with_fields()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<EntityWithFields>().HasAlternateKey(e => e.CompanyId);

            var entity = modelBuilder.Model.FindEntityType(typeof(EntityWithFields))!;
            var properties = entity.GetProperties().Where(p => !p.IsShadowProperty());
            Assert.Single(properties);
            var property = properties.Single();
            Assert.Equal(nameof(EntityWithFields.CompanyId), property.Name);
            Assert.Null(property.PropertyInfo);
            Assert.NotNull(property.FieldInfo);
            var keys = entity.GetKeys().Where(k => k.Properties.Any(p => p.Name.Contains("Id")));
            var key = Assert.Single(keys);
            Assert.Equal(properties, key.Properties);
        }

        [ConditionalFact]
        public virtual void Can_set_composite_alternate_key_on_an_entity_with_fields()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<EntityWithFields>().HasAlternateKey(e => new { e.TenantId, e.CompanyId });

            var keys = modelBuilder.Model.FindEntityType(typeof(EntityWithFields))!.GetKeys()
                .Where(k => k.Properties.Any(p => p.Name.Contains("Id")));
            Assert.Single(keys);
            var properties = keys.Single().Properties;
            Assert.Equal(2, properties.Count);
            var first = properties[0];
            var second = properties[1];
            Assert.Equal(nameof(EntityWithFields.TenantId), first.Name);
            Assert.Null(first.PropertyInfo);
            Assert.NotNull(first.FieldInfo);
            Assert.Equal(nameof(EntityWithFields.CompanyId), second.Name);
            Assert.Null(second.PropertyInfo);
            Assert.NotNull(second.FieldInfo);
        }

        [ConditionalFact]
        public virtual void Can_call_Property_on_an_entity_with_fields()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<EntityWithFields>().Property(e => e.Id);

            var model = modelBuilder.FinalizeModel();
            var properties = model.FindEntityType(typeof(EntityWithFields))!.GetProperties().Where(p => !p.IsShadowProperty());
            var property = Assert.Single(properties);
            Assert.Equal(nameof(EntityWithFields.Id), property.Name);
            Assert.Null(property.PropertyInfo);
            Assert.NotNull(property.FieldInfo);
        }

        [ConditionalFact]
        public virtual void Can_set_index_on_an_entity_with_fields()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<EntityWithFields>().HasNoKey().HasIndex(e => e.CompanyId);

            var model = modelBuilder.FinalizeModel();
            var indexes = model.FindEntityType(typeof(EntityWithFields))!.GetIndexes();
            var index = Assert.Single(indexes);
            var property = Assert.Single(index.Properties);
            Assert.Null(property.PropertyInfo);
            Assert.NotNull(property.FieldInfo);
        }

        [ConditionalFact]
        public virtual void Can_set_composite_index_on_an_entity_with_fields()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<EntityWithFields>().HasNoKey().HasIndex(e => new { e.TenantId, e.CompanyId });

            var model = modelBuilder.FinalizeModel();
            var indexes = model.FindEntityType(typeof(EntityWithFields))!.GetIndexes();
            var index = Assert.Single(indexes);
            Assert.Equal(2, index.Properties.Count);
            var properties = index.Properties;
            var first = properties[0];
            var second = properties[1];
            Assert.Equal(nameof(EntityWithFields.TenantId), first.Name);
            Assert.Null(first.PropertyInfo);
            Assert.NotNull(first.FieldInfo);
            Assert.Equal(nameof(EntityWithFields.CompanyId), second.Name);
            Assert.Null(second.PropertyInfo);
            Assert.NotNull(second.FieldInfo);
        }

        [ConditionalFact]
        public virtual void Can_ignore_a_field_on_an_entity_with_fields()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<EntityWithFields>()
                .Ignore(e => e.CompanyId)
                .HasKey(e => e.Id);

            var model = modelBuilder.FinalizeModel();
            var entity = model.FindEntityType(typeof(EntityWithFields))!;
            var property = Assert.Single(entity.GetProperties().Where(p => !p.IsShadowProperty()));
            Assert.Equal(nameof(EntityWithFields.Id), property.Name);
        }

        [ConditionalFact]
        public virtual void Can_ignore_a_field_on_a_keyless_entity_with_fields()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<KeylessEntityWithFields>()
                .HasNoKey()
                .Ignore(e => e.FirstName)
                .Property(e => e.LastName);

            var model = modelBuilder.FinalizeModel();
            var entity = model.FindEntityType(typeof(KeylessEntityWithFields))!;
            var property = Assert.Single(entity.GetProperties().Where(p => !p.IsShadowProperty()));
            Assert.Equal(nameof(KeylessEntityWithFields.LastName), property.Name);
        }

        [ConditionalFact]
        public virtual void Can_add_seed_data_objects()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Ignore<Theta>();
            modelBuilder.Entity<Beta>(
                c =>
                {
                    c.HasData(
                        new Beta { Id = -1, Name = " -1" });
                    var customers = new List<Beta> { new() { Id = -2 } };
                    c.HasData(customers);
                });

            var finalModel = modelBuilder.FinalizeModel();

            var customer = finalModel.FindEntityType(typeof(Beta))!;
            var data = customer.GetSeedData();
            Assert.Equal(2, data.Count());
            Assert.Equal(-1, data.First()[nameof(Beta.Id)]);
            Assert.Equal(" -1", data.First()[nameof(Beta.Name)]);
            Assert.Equal(-2, data.Last()[nameof(Beta.Id)]);

            var _ = finalModel.ToDebugString();
        }

        [ConditionalFact]
        public virtual void Can_add_seed_data_anonymous_objects()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Ignore<Theta>();
            modelBuilder.Entity<Beta>(
                c =>
                {
                    c.HasData(
                        new { Id = -1 });
                    var customers = new List<object> { new { Id = -2 } };
                    c.HasData(customers);
                });

            var model = modelBuilder.FinalizeModel();

            var customer = model.FindEntityType(typeof(Beta))!;
            var data = customer.GetSeedData();
            Assert.Equal(2, data.Count());
            Assert.Equal(-1, data.First().Values.Single());
            Assert.Equal(-2, data.Last().Values.Single());
        }

        [ConditionalFact]
        public virtual void Can_add_seed_data_objects_indexed_property()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<IndexedClass>(
                b =>
                {
                    b.IndexerProperty<int>("Required");
                    b.IndexerProperty<string>("Optional");
                    var d = new IndexedClass { Id = -1 };
                    d["Required"] = 2;
                    b.HasData(d);
                });

            var model = modelBuilder.FinalizeModel();

            var entityType = model.FindEntityType(typeof(IndexedClass))!;
            var data = Assert.Single(entityType.GetSeedData());
            Assert.Equal(-1, data["Id"]);
            Assert.Equal(2, data["Required"]);
            Assert.Null(data["Optional"]);
        }

        [ConditionalFact]
        public virtual void Can_add_seed_data_anonymous_objects_indexed_property()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<IndexedClass>(
                b =>
                {
                    b.IndexerProperty<int>("Required");
                    b.IndexerProperty<string>("Optional");
                    b.HasData(new { Id = -1, Required = 2 });
                });

            var model = modelBuilder.FinalizeModel();

            var entityType = model.FindEntityType(typeof(IndexedClass))!;
            var data = Assert.Single(entityType.GetSeedData());
            Assert.Equal(-1, data["Id"]);
            Assert.Equal(2, data["Required"]);
            Assert.False(data.ContainsKey("Optional"));
        }

        [ConditionalFact]
        public virtual void Can_add_seed_data_objects_indexed_property_dictionary()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<IndexedClassByDictionary>(
                b =>
                {
                    b.IndexerProperty<int>("Required");
                    b.IndexerProperty<string>("Optional");
                    var d = new IndexedClassByDictionary { Id = -1 };
                    d["Required"] = 2;
                    b.HasData(d);
                });

            var model = modelBuilder.FinalizeModel();

            var entityType = model.FindEntityType(typeof(IndexedClassByDictionary))!;
            var data = Assert.Single(entityType.GetSeedData());
            Assert.Equal(-1, data["Id"]);
            Assert.Equal(2, data["Required"]);
            Assert.Null(data["Optional"]);
        }

        [ConditionalFact]
        public virtual void Can_add_seed_data_anonymous_objects_indexed_property_dictionary()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<IndexedClassByDictionary>(
                b =>
                {
                    b.IndexerProperty<int>("Required");
                    b.IndexerProperty<string>("Optional");
                    b.HasData(new { Id = -1, Required = 2 });
                });

            var model = modelBuilder.FinalizeModel();

            var entityType = model.FindEntityType(typeof(IndexedClassByDictionary))!;
            var data = Assert.Single(entityType.GetSeedData());
            Assert.Equal(-1, data["Id"]);
            Assert.Equal(2, data["Required"]);
            Assert.False(data.ContainsKey("Optional"));
        }

        [ConditionalFact] //Issue#12617
        [UseCulture("de-DE")]
        public virtual void EntityType_name_is_stored_culture_invariantly()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Entityß>();
            modelBuilder.Entity<Entityss>();

            var model = modelBuilder.FinalizeModel();

            Assert.Equal(2, model.GetEntityTypes().Count());
            Assert.Equal(2, model.FindEntityType(typeof(Entityss))!.GetNavigations().Count());
        }

        protected class Entityß
        {
            public int Id { get; set; }
        }

        protected class Entityss
        {
            public int Id { get; set; }
            public Entityß? Navigationß { get; set; }
            public Entityß? Navigationss { get; set; }
        }

        [ConditionalFact]
        public virtual void Can_add_shared_type_entity_type()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.SharedTypeEntity<Dictionary<string, object>>(
                "Shared1", b =>
                {
                    b.IndexerProperty<int>("Key");
                    b.Property<int>("Keys");
                    b.Property<byte[]>("Values");
                    b.Property<string>("Count");
                    b.HasKey("Key");
                });

            modelBuilder.SharedTypeEntity<Dictionary<string, object>>("Shared2", b => b.IndexerProperty<int>("Id"));

            Assert.Equal(
                CoreStrings.ClashingSharedType(typeof(Dictionary<string, object>).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.Entity<Dictionary<string, object>>()).Message);

            var model = modelBuilder.FinalizeModel();
            Assert.Equal(2, model.GetEntityTypes().Count());

            var shared1 = model.FindEntityType("Shared1");
            Assert.NotNull(shared1);
            Assert.True(shared1.HasSharedClrType);
            Assert.Null(shared1.FindProperty("Id"));
            Assert.Equal(typeof(int), shared1.FindProperty("Keys")!.ClrType);
            Assert.Equal(typeof(byte[]), shared1.FindProperty("Values")!.ClrType);
            Assert.Equal(typeof(string), shared1.FindProperty("Count")!.ClrType);

            var shared2 = model.FindEntityType("Shared2");
            Assert.NotNull(shared2);
            Assert.True(shared2.HasSharedClrType);
            Assert.NotNull(shared2.FindProperty("Id"));

            var indexer = shared1.FindIndexerPropertyInfo()!;
            Assert.True(model.IsIndexerMethod(indexer.GetMethod!));
            Assert.True(model.IsIndexerMethod(indexer.SetMethod!));
            Assert.Same(indexer, shared2.FindIndexerPropertyInfo());
        }

        [ConditionalFact]
        public virtual void Cannot_add_shared_type_when_non_shared_exists()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>();

            Assert.Equal(
                CoreStrings.ClashingNonSharedType("Shared1", nameof(Customer)),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.SharedTypeEntity<Customer>("Shared1")).Message);
        }

        [ConditionalFact]
        public virtual void Can_set_primitive_collection_annotation()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Product>();
            modelBuilder
                .Entity<Customer>()
                .PrimitiveCollection(c => c.Notes).HasAnnotation("foo", "bar");

            var property = modelBuilder.FinalizeModel().FindEntityType(typeof(Customer))!.FindProperty(nameof(Customer.Notes))!;

            Assert.Equal("bar", property["foo"]);
        }

        [ConditionalFact]
        public virtual void Can_set_primitive_collection_annotation_when_no_clr_property()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Product>();
            modelBuilder
                .Entity<Customer>()
                .PrimitiveCollection<List<string>>(nameof(Customer.Notes)).HasAnnotation("foo", "bar");

            var property = modelBuilder.FinalizeModel().FindEntityType(typeof(Customer))!.FindProperty(nameof(Customer.Notes))!;

            Assert.Equal("bar", property["foo"]);
        }

        [ConditionalFact]
        public virtual void Can_set_primitive_collection_annotation_by_type()
        {
            var modelBuilder = CreateModelBuilder(c => c.Properties<string>().HaveAnnotation("foo", "bar"));

            modelBuilder.Ignore<Product>();
            var propertyBuilder = modelBuilder
                .Entity<Customer>()
                .PrimitiveCollection(c => c.Notes);

            var property = modelBuilder.FinalizeModel().FindEntityType(typeof(Customer))!.FindProperty(nameof(Customer.Name))!;

            Assert.Equal("bar", property["foo"]);
        }

        [ConditionalFact]
        public virtual void Primitive_collections_are_required_by_default_only_if_CLR_type_is_nullable()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection(e => e.Up);
                    b.PrimitiveCollection(e => e.Down);
                    b.PrimitiveCollection<List<int>>("Charm");
                    b.PrimitiveCollection<List<string>?>("Strange");
                });

            var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(CollectionQuarks))!;

            Assert.False(entityType.FindProperty("Up")!.IsNullable);
            Assert.True(entityType.FindProperty("Down")!.IsNullable);
            Assert.True(entityType.FindProperty("Charm")!.IsNullable); // Because we can't detect the non-nullable reference type
            Assert.True(entityType.FindProperty("Strange")!.IsNullable);
        }

        [ConditionalFact]
        public virtual void Primitive_collections_can_be_ignored()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.Ignore(e => e.Up);
                    b.Ignore(e => e.Down);
                    b.Ignore("Charm");
                    b.Ignore("Strange");
                });

            var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(CollectionQuarks))!;
            Assert.Contains(nameof(CollectionQuarks.Id), entityType.GetProperties().Select(p => p.Name));
            Assert.DoesNotContain(nameof(CollectionQuarks.Up), entityType.GetProperties().Select(p => p.Name));
            Assert.DoesNotContain(nameof(CollectionQuarks.Down), entityType.GetProperties().Select(p => p.Name));
        }

        [ConditionalFact]
        public virtual void Can_override_navigations_as_primitive_collections()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;
            modelBuilder.Entity<Customer>();

            var customer = model.FindEntityType(typeof(Customer))!;
            Assert.NotNull(customer.FindNavigation(nameof(Customer.Orders)));

            modelBuilder.Entity<Customer>().PrimitiveCollection(c => c.Orders);

            Assert.Null(customer.FindNavigation(nameof(Customer.Orders)));
            var property = customer.FindProperty(nameof(Customer.Orders));
            Assert.NotNull(property);
            Assert.NotNull(property.GetElementType());
        }

        [ConditionalFact]
        public virtual void Primitive_collections_can_be_made_required()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection(e => e.Up).IsRequired();
                    b.PrimitiveCollection(e => e.Down).IsRequired();
                    b.PrimitiveCollection<List<int>>("Charm").IsRequired();
                    b.PrimitiveCollection<List<string>?>("Strange").IsRequired();
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = (IReadOnlyEntityType)model.FindEntityType(typeof(CollectionQuarks))!;

            Assert.False(entityType.FindProperty("Up")!.IsNullable);
            Assert.False(entityType.FindProperty("Down")!.IsNullable);
            Assert.False(entityType.FindProperty("Charm")!.IsNullable);
            Assert.False(entityType.FindProperty("Strange")!.IsNullable);
        }

        [ConditionalFact]
        public virtual void Primitive_collections_can_be_made_optional()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection(e => e.Up).IsRequired(false);
                    b.PrimitiveCollection(e => e.Down).IsRequired(false);
                    b.PrimitiveCollection<List<int>>("Charm").IsRequired(false);
                    b.PrimitiveCollection<List<string>?>("Strange").IsRequired(false);
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = (IReadOnlyEntityType)model.FindEntityType(typeof(CollectionQuarks))!;

            Assert.True(entityType.FindProperty("Up")!.IsNullable);
            Assert.True(entityType.FindProperty("Down")!.IsNullable);
            Assert.True(entityType.FindProperty("Charm")!.IsNullable);
            Assert.True(entityType.FindProperty("Strange")!.IsNullable);
        }

        [ConditionalFact]
        public virtual void PrimitiveCollection_Key_properties_cannot_be_made_optional()
            => Assert.Equal(
                CoreStrings.KeyPropertyCannotBeNullable(
                    nameof(CollectionQuarks.Down), nameof(CollectionQuarks), "{'" + nameof(CollectionQuarks.Down) + "'}"),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        CreateModelBuilder().Entity<CollectionQuarks>(
                            b =>
                            {
                                b.HasAlternateKey(e => new { e.Down });
                                b.PrimitiveCollection(e => e.Down).IsRequired(false);
                            })).Message);

        [ConditionalFact]
        public virtual void Primitive_collections_specified_by_string_are_shadow_properties_unless_already_known_to_be_CLR_properties()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection<ObservableCollection<int>>("Up");
                    b.PrimitiveCollection<ObservableCollection<string>?>("Down");
                    b.PrimitiveCollection<ObservableCollection<int>>("Charm");
                    b.PrimitiveCollection<ObservableCollection<string>?>("Strange");
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(CollectionQuarks))!;

            Assert.False(entityType.FindProperty("Up")!.IsShadowProperty());
            Assert.False(entityType.FindProperty("Down")!.IsShadowProperty());
            Assert.True(entityType.FindProperty("Charm")!.IsShadowProperty());
            Assert.True(entityType.FindProperty("Strange")!.IsShadowProperty());

            Assert.Equal(-1, entityType.FindProperty("Up")!.GetShadowIndex());
            Assert.Equal(-1, entityType.FindProperty("Down")!.GetShadowIndex());
            Assert.NotEqual(-1, entityType.FindProperty("Charm")!.GetShadowIndex());
            Assert.NotEqual(-1, entityType.FindProperty("Strange")!.GetShadowIndex());
        }

        [ConditionalFact]
        public virtual void Primitive_collections_can_be_made_concurrency_tokens()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection(e => e.Up).IsConcurrencyToken();
                    b.PrimitiveCollection(e => e.Down).IsConcurrencyToken(false);
                    b.PrimitiveCollection<List<int>>("Charm").IsConcurrencyToken();
                    b.PrimitiveCollection<List<string>?>("Strange").IsConcurrencyToken(false);
                    b.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(CollectionQuarks))!;

            Assert.True(entityType.FindProperty("Up")!.IsConcurrencyToken);
            Assert.False(entityType.FindProperty("Down")!.IsConcurrencyToken);
            Assert.True(entityType.FindProperty("Charm")!.IsConcurrencyToken);
            Assert.False(entityType.FindProperty("Strange")!.IsConcurrencyToken);

            Assert.Equal(ChangeTrackingStrategy.ChangingAndChangedNotifications, entityType.GetChangeTrackingStrategy());
        }

        [ConditionalFact]
        public virtual void Primitive_collections_can_have_access_mode_set()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection(e => e.Up);
                    b.PrimitiveCollection(e => e.Down).HasField("_forDown").UsePropertyAccessMode(PropertyAccessMode.Field);
                    b.PrimitiveCollection<ObservableCollection<int>>("Charm").UsePropertyAccessMode(PropertyAccessMode.Property);
                    b.PrimitiveCollection<ObservableCollection<string>?>("Strange")
                        .UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = (IReadOnlyEntityType)model.FindEntityType(typeof(CollectionQuarks))!;

            Assert.Equal(PropertyAccessMode.PreferField, entityType.FindProperty("Up")!.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.Field, entityType.FindProperty("Down")!.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.Property, entityType.FindProperty("Charm")!.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.FieldDuringConstruction, entityType.FindProperty("Strange")!.GetPropertyAccessMode());
        }

        [ConditionalFact]
        public virtual void Access_mode_can_be_overridden_at_entity_and_primitive_collection_levels()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Field);

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);
                    b.PrimitiveCollection(e => e.Up).UsePropertyAccessMode(PropertyAccessMode.Property);
                    b.PrimitiveCollection(e => e.Down).HasField("_forDown");
                });

            var model = modelBuilder.FinalizeModel();
            Assert.Equal(PropertyAccessMode.Field, model.GetPropertyAccessMode());

            var collectionQuarksType = (IReadOnlyEntityType)model.FindEntityType(typeof(CollectionQuarks))!;
            Assert.Equal(PropertyAccessMode.FieldDuringConstruction, collectionQuarksType.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.FieldDuringConstruction, collectionQuarksType.FindProperty("Down")!.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.Property, collectionQuarksType.FindProperty("Up")!.GetPropertyAccessMode());
        }

        [ConditionalFact]
        public virtual void Primitive_collections_can_have_field_set()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection<ObservableCollection<int>>("Up").HasField("_forUp");
                    b.PrimitiveCollection(e => e.Down).HasField("_forDown");
                    b.PrimitiveCollection<ObservableCollection<string>?>("_forWierd").HasField("_forWierd");
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(CollectionQuarks))!;

            Assert.Equal("_forUp", entityType.FindProperty("Up")!.GetFieldName());
            Assert.Equal("_forDown", entityType.FindProperty("Down")!.GetFieldName());
            Assert.Equal("_forWierd", entityType.FindProperty("_forWierd")!.GetFieldName());
        }

        [ConditionalFact]
        public virtual void HasField_for_primitive_collection_throws_if_field_is_not_found()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    Assert.Equal(
                        CoreStrings.MissingBackingField("_notFound", nameof(CollectionQuarks.Down), nameof(CollectionQuarks)),
                        Assert.Throws<InvalidOperationException>(() => b.PrimitiveCollection(e => e.Down).HasField("_notFound")).Message);
                });
        }

        [ConditionalFact]
        public virtual void HasField_for_primitive_collection_throws_if_field_is_wrong_type()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    Assert.Equal(
                        CoreStrings.BadBackingFieldType(
                            "_forUp", "ObservableCollection<int>", nameof(CollectionQuarks), nameof(CollectionQuarks.Down),
                            "ObservableCollection<string>"),
                        Assert.Throws<InvalidOperationException>(() => b.PrimitiveCollection(e => e.Down).HasField("_forUp")).Message);
                });
        }

        [ConditionalFact]
        public virtual void Primitive_collections_can_be_set_to_generate_values_on_Add()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.HasKey(e => e.Id);
                    b.PrimitiveCollection(e => e.Up).ValueGeneratedOnAddOrUpdate();
                    b.PrimitiveCollection(e => e.Down).ValueGeneratedNever();
                    b.PrimitiveCollection<List<int>>("Charm").Metadata.ValueGenerated = ValueGenerated.OnUpdateSometimes;
                    b.PrimitiveCollection<List<string>>("Strange").ValueGeneratedNever();
                    b.PrimitiveCollection<List<int>>("Top").ValueGeneratedOnAddOrUpdate();
                    b.PrimitiveCollection<List<string>>("Bottom").ValueGeneratedOnUpdate();
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(CollectionQuarks))!;
            Assert.Equal(ValueGenerated.OnAddOrUpdate, entityType.FindProperty("Up")!.ValueGenerated);
            Assert.Equal(ValueGenerated.Never, entityType.FindProperty("Down")!.ValueGenerated);
            Assert.Equal(ValueGenerated.OnUpdateSometimes, entityType.FindProperty("Charm")!.ValueGenerated);
            Assert.Equal(ValueGenerated.Never, entityType.FindProperty("Strange")!.ValueGenerated);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, entityType.FindProperty("Top")!.ValueGenerated);
            Assert.Equal(ValueGenerated.OnUpdate, entityType.FindProperty("Bottom")!.ValueGenerated);
        }

        [ConditionalFact]
        public virtual void Can_set_max_length_for_primitive_collections()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection(e => e.Up).HasMaxLength(0);
                    b.PrimitiveCollection(e => e.Down).HasMaxLength(100);
                    b.PrimitiveCollection<List<int>>("Charm").HasMaxLength(0);
                    b.PrimitiveCollection<List<string>>("Strange").HasMaxLength(-1);
                    b.PrimitiveCollection<int[]>("Top").HasMaxLength(0);
                    b.PrimitiveCollection<string[]>("Bottom").HasMaxLength(100);
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(CollectionQuarks))!;

            Assert.Null(entityType.FindProperty(nameof(CollectionQuarks.Id))!.GetMaxLength());
            Assert.Equal(0, entityType.FindProperty("Up")!.GetMaxLength());
            Assert.Equal(100, entityType.FindProperty("Down")!.GetMaxLength());
            Assert.Equal(0, entityType.FindProperty("Charm")!.GetMaxLength());
            Assert.Equal(-1, entityType.FindProperty("Strange")!.GetMaxLength());
            Assert.Equal(0, entityType.FindProperty("Top")!.GetMaxLength());
            Assert.Equal(100, entityType.FindProperty("Bottom")!.GetMaxLength());
        }

        [ConditionalFact]
        public virtual void Can_set_sentinel_for_primitive_collections()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection(e => e.Up).HasSentinel(null);
                    b.PrimitiveCollection(e => e.Down).HasSentinel(new ObservableCollection<string>());
                    b.PrimitiveCollection<int[]>("Charm").HasSentinel([]);
                    b.PrimitiveCollection<List<string>>("Strange").HasSentinel(new List<string> { });
                    b.PrimitiveCollection<int[]>("Top").HasSentinel([77]);
                    b.PrimitiveCollection<List<string>>("Bottom").HasSentinel(new List<string> { "" });
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(CollectionQuarks))!;

            Assert.Equal(0, entityType.FindProperty(nameof(CollectionQuarks.Id))!.Sentinel);
            Assert.Null(entityType.FindProperty("Up")!.Sentinel);
            Assert.Equal(new ObservableCollection<string>(), entityType.FindProperty("Down")!.Sentinel);
            Assert.Equal(Array.Empty<int>(), entityType.FindProperty("Charm")!.Sentinel);
            Assert.Equal(new List<string> { }, entityType.FindProperty("Strange")!.Sentinel);
            Assert.Equal(new int[] { 77 }, entityType.FindProperty("Top")!.Sentinel);
            Assert.Equal(new List<string> { "" }, entityType.FindProperty("Bottom")!.Sentinel);
        }

        [ConditionalFact]
        public virtual void Can_set_custom_value_generator_for_primitive_collections()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection(e => e.Up).HasValueGenerator<CustomValueGenerator>();
                    b.PrimitiveCollection(e => e.Down).HasValueGenerator(typeof(CustomValueGenerator));
                    b.PrimitiveCollection<List<string>>("Strange").HasValueGenerator<CustomValueGenerator>();
                    b.PrimitiveCollection<int[]>("Top").HasValueGeneratorFactory(typeof(CustomValueGeneratorFactory));
                    b.PrimitiveCollection<List<string>>("Bottom").HasValueGeneratorFactory<CustomValueGeneratorFactory>();
                });

            var model = modelBuilder.FinalizeModel();

            var entityType = model.FindEntityType(typeof(CollectionQuarks))!;

            Assert.Null(entityType.FindProperty(nameof(CollectionQuarks.Id))!.GetValueGeneratorFactory());
            Assert.IsType<CustomValueGenerator>(entityType.FindProperty("Up")!.GetValueGeneratorFactory()!(null!, null!));
            Assert.IsType<CustomValueGenerator>(entityType.FindProperty("Down")!.GetValueGeneratorFactory()!(null!, null!));
            Assert.IsType<CustomValueGenerator>(entityType.FindProperty("Strange")!.GetValueGeneratorFactory()!(null!, null!));
            Assert.IsType<CustomValueGenerator>(entityType.FindProperty("Top")!.GetValueGeneratorFactory()!(null!, null!));
            Assert.IsType<CustomValueGenerator>(entityType.FindProperty("Bottom")!.GetValueGeneratorFactory()!(null!, null!));
        }

        [ConditionalFact]
        public virtual void Throws_for_bad_value_generator_type_for_primitive_collection()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    Assert.Equal(
                        CoreStrings.BadValueGeneratorType(nameof(Random), nameof(ValueGenerator)),
                        Assert.Throws<ArgumentException>(() => b.PrimitiveCollection(e => e.Down).HasValueGenerator(typeof(Random)))
                            .Message);
                });
        }

        [ConditionalFact]
        public virtual void Throws_for_primitive_collection_for_value_generator_that_cannot_be_constructed()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection(e => e.Up).HasValueGenerator<BadCustomValueGenerator1>();
                    b.PrimitiveCollection(e => e.Down).HasValueGenerator<BadCustomValueGenerator2>();
                });

            var entityType = model.FindEntityType(typeof(CollectionQuarks))!;

            Assert.Equal(
                CoreStrings.CannotCreateValueGenerator(nameof(BadCustomValueGenerator1), "HasValueGenerator"),
                Assert.Throws<InvalidOperationException>(
                    () => entityType.FindProperty("Up")!.GetValueGeneratorFactory()!(null!, null!)).Message);

            Assert.Equal(
                CoreStrings.CannotCreateValueGenerator(nameof(BadCustomValueGenerator2), "HasValueGenerator"),
                Assert.Throws<InvalidOperationException>(
                    () => entityType.FindProperty("Down")!.GetValueGeneratorFactory()!(null!, null!)).Message);
        }

        [ConditionalFact]
        protected virtual void Mapping_for_primitive_collection_ignores_ignored_array()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<OneDee>().Ignore(e => e.One);

            var model = modelBuilder.FinalizeModel();

            Assert.Null(model.FindEntityType(typeof(OneDee))!.FindProperty("One"));
        }

        [ConditionalFact]
        public virtual void Private_primitive_collection_is_not_discovered_by_convention()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Alpha>();
            modelBuilder.Entity<Gamma>();

            var model = modelBuilder.FinalizeModel();

            Assert.Empty(
                model.FindEntityType(typeof(Gamma))!.GetProperties()
                    .Where(p => p.Name == "PrivateCollection"));
        }

        [ConditionalFact]
        public virtual void Can_set_unicode_for_primitive_collections()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection(e => e.Up).IsUnicode();
                    b.PrimitiveCollection(e => e.Down).IsUnicode(false);
                    b.PrimitiveCollection<int[]>("Charm").IsUnicode();
                    b.PrimitiveCollection<List<string>>("Strange").IsUnicode(false);
                    b.PrimitiveCollection<int[]?>("Top").IsUnicode();
                    b.PrimitiveCollection<List<string>?>("Bottom").IsUnicode(false);
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(CollectionQuarks))!;

            Assert.Null(entityType.FindProperty(nameof(CollectionQuarks.Id))!.IsUnicode());
            Assert.True(entityType.FindProperty("Up")!.IsUnicode());
            Assert.False(entityType.FindProperty("Down")!.IsUnicode());
            Assert.True(entityType.FindProperty("Charm")!.IsUnicode());
            Assert.False(entityType.FindProperty("Strange")!.IsUnicode());
            Assert.True(entityType.FindProperty("Top")!.IsUnicode());
            Assert.False(entityType.FindProperty("Bottom")!.IsUnicode());
        }

        [ConditionalFact]
        public virtual void PrimitiveCollectionBuilder_methods_can_be_chained()
            => CreateModelBuilder()
                .Entity<CollectionQuarks>()
                .PrimitiveCollection(e => e.Up)
                .ElementType(t => t
                    .HasAnnotation("B", "C")
                    .HasConversion(typeof(long))
                    .HasConversion(new CastingConverter<int, long>())
                    .HasConversion(typeof(long), typeof(CustomValueComparer<int>))
                    .HasConversion(typeof(long), new CustomValueComparer<int>())
                    .HasConversion(new CastingConverter<int, long>())
                    .HasConversion(new CastingConverter<int, long>(), new CustomValueComparer<int>())
                    .HasConversion<long>()
                    .HasConversion<long>(new CustomValueComparer<int>())
                    .HasConversion<long, CustomValueComparer<int>>()
                    .HasMaxLength(2)
                    .HasPrecision(1)
                    .HasPrecision(1, 2)
                    .IsRequired()
                    .IsUnicode())
                .IsRequired()
                .IsRequired(false)
                .HasAnnotation("A", "V")
                .IsConcurrencyToken()
                .ValueGeneratedNever()
                .ValueGeneratedOnAdd()
                .ValueGeneratedOnAddOrUpdate()
                .ValueGeneratedOnUpdate()
                .IsUnicode()
                .HasMaxLength(100)
                .HasSentinel(null)
                .HasValueGenerator<CustomValueGenerator>()
                .HasValueGenerator(typeof(CustomValueGenerator))
                .HasValueGeneratorFactory<CustomValueGeneratorFactory>()
                .HasValueGeneratorFactory(typeof(CustomValueGeneratorFactory));

        [ConditionalFact]
        public virtual void Can_set_primary_key_by_convention_for_user_specified_shadow_primitive_collection()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            var entityBuilder = modelBuilder.Entity<EntityWithoutId>();

            var entityType = (IReadOnlyEntityType)model.FindEntityType(typeof(EntityWithoutId))!;

            Assert.Null(entityType.FindPrimaryKey());

            entityBuilder.PrimitiveCollection<List<int>>("Id");

            Assert.NotNull(entityType.FindPrimaryKey());
            Assert.Equal(new[] { "Id" }, entityType.FindPrimaryKey()!.Properties.Select(p => p.Name));
        }

        [ConditionalFact]
        public virtual void Can_set_key_for_primitive_collection_on_an_entity_with_fields()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<EntityWithFields>().HasKey(e => e.Id);

            var model = modelBuilder.FinalizeModel();
            var entity = model.FindEntityType(typeof(EntityWithFields))!;
            var primaryKey = entity.FindPrimaryKey();
            Assert.NotNull(primaryKey);
            var property = Assert.Single(primaryKey.Properties);
            Assert.Equal(nameof(EntityWithFields.Id), property.Name);
            Assert.Null(property.PropertyInfo);
            Assert.NotNull(property.FieldInfo);
        }

        [ConditionalFact]
        public virtual void Can_set_composite_key_for_primitive_collection_on_an_entity_with_fields()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<EntityWithFields>(
                b =>
                {
                    b.PrimitiveCollection(e => e.CollectionCompanyId); // Issue #31417
                    b.PrimitiveCollection(e => e.CollectionTenantId);
                    b.HasKey(e => new { e.CollectionCompanyId, e.CollectionTenantId });
                });

            var model = modelBuilder.FinalizeModel();
            var entity = model.FindEntityType(typeof(EntityWithFields))!;
            var primaryKeyProperties = entity.FindPrimaryKey()!.Properties;
            Assert.Equal(2, primaryKeyProperties.Count);
            var first = primaryKeyProperties[0];
            var second = primaryKeyProperties[1];
            Assert.Equal(nameof(EntityWithFields.CollectionCompanyId), first.Name);
            Assert.Null(first.PropertyInfo);
            Assert.NotNull(first.FieldInfo);
            Assert.NotNull(first.GetElementType());
            Assert.Equal(nameof(EntityWithFields.CollectionTenantId), second.Name);
            Assert.Null(second.PropertyInfo);
            Assert.NotNull(second.FieldInfo);
            Assert.NotNull(second.GetElementType());
        }

        [ConditionalFact]
        public virtual void Can_set_alternate_key_for_primitive_collection_on_an_entity_with_fields()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<EntityWithFields>(
                b =>
                {
                    b.PrimitiveCollection(e => e.CollectionCompanyId);
                    b.HasAlternateKey(e => e.CollectionCompanyId);
                    b.HasAlternateKey(e => e.CollectionId);
                    b.HasKey(e => e.Id);
                });

            var model = modelBuilder.FinalizeModel();
            AssertEqual(modelBuilder.Model, model);

            var entity = model.FindEntityType(typeof(EntityWithFields))!;
            var property = entity.FindProperty(nameof(EntityWithFields.CollectionCompanyId))!;
            Assert.Null(property.PropertyInfo);
            Assert.NotNull(property.FieldInfo);
            Assert.NotNull(property.GetElementType());
            var keys = entity.GetKeys();
            Assert.Equal(3, keys.Count());
            Assert.Single(keys.Where(k => k.Properties.All(p => p == property)));
        }

        [ConditionalFact]
        public virtual void Can_call_PrimitiveCollection_on_an_entity_with_fields()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<EntityWithFields>().PrimitiveCollection(e => e.CollectionId);
            modelBuilder.Entity<EntityWithFields>().HasKey(e => e.CollectionId);

            var model = modelBuilder.FinalizeModel();
            var properties = model.FindEntityType(typeof(EntityWithFields))!.GetProperties();
            var property = Assert.Single(properties);
            Assert.Equal(nameof(EntityWithFields.CollectionId), property.Name);
            Assert.Null(property.PropertyInfo);
            Assert.NotNull(property.FieldInfo);
            Assert.NotNull(property.GetElementType());
        }

        [ConditionalFact]
        public virtual void Can_set_element_type_annotation()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<Product>();
            modelBuilder
                .Entity<Customer>()
                .PrimitiveCollection(c => c.Notes)
                .ElementType()
                .HasAnnotation("foo", "bar");

            var elementType = modelBuilder.FinalizeModel()
                .FindEntityType(typeof(Customer))!
                .FindProperty(nameof(Customer.Notes))!
                .GetElementType()!;

            Assert.Equal("bar", elementType["foo"]);
        }

        [ConditionalFact]
        public virtual void Element_types_are_nullable_by_default_if_the_type_is_nullable()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection(e => e.Up);
                    b.PrimitiveCollection(e => e.Down);
                    b.PrimitiveCollection<List<int?>>("Charm");
                    b.PrimitiveCollection<List<string?>>("Strange");
                    b.PrimitiveCollection<List<string>>("Stranger"); // Still optional since no NRT metadata available
                });

            var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(CollectionQuarks))!;

            Assert.False(entityType.FindProperty(nameof(CollectionQuarks.Up))!.GetElementType()!.IsNullable);
            Assert.False(entityType.FindProperty(nameof(CollectionQuarks.Down))!.GetElementType()!.IsNullable);
            Assert.True(entityType.FindProperty("Charm")!.GetElementType()!.IsNullable);
            Assert.True(entityType.FindProperty("Strange")!.GetElementType()!.IsNullable);
            Assert.True(entityType.FindProperty("Stranger")!.GetElementType()!.IsNullable);
        }

        [ConditionalFact]
        public virtual void Element_types_can_be_made_required()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection(e => e.Up).ElementType().IsRequired();
                    b.PrimitiveCollection(e => e.Down).ElementType().IsRequired(false);
                    b.PrimitiveCollection<List<int?>>("Charm").ElementType().IsRequired();
                    ;
                    b.PrimitiveCollection<List<string?>>("Strange").ElementType().IsRequired();
                    ;
                    b.PrimitiveCollection<List<string>>("Stranger").ElementType().IsRequired();
                    ; // Still optional since no NRT metadata available
                });

            var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(CollectionQuarks))!;

            Assert.False(entityType.FindProperty(nameof(CollectionQuarks.Up))!.GetElementType()!.IsNullable);
            Assert.True(entityType.FindProperty(nameof(CollectionQuarks.Down))!.GetElementType()!.IsNullable);
            Assert.False(entityType.FindProperty("Charm")!.GetElementType()!.IsNullable);
            Assert.False(entityType.FindProperty("Strange")!.GetElementType()!.IsNullable);
            Assert.False(entityType.FindProperty("Stranger")!.GetElementType()!.IsNullable);
        }

        [ConditionalFact]
        public virtual void Element_types_have_no_max_length_by_default()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection<List<int?>>("Charm");
                    b.PrimitiveCollection<List<string?>>("Strange");
                    b.PrimitiveCollection<List<string>>("Stranger");
                });

            var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(CollectionQuarks))!;

            Assert.Null(entityType.FindProperty(nameof(CollectionQuarks.Up))!.GetElementType()!.GetMaxLength());
            Assert.Null(entityType.FindProperty(nameof(CollectionQuarks.Down))!.GetElementType()!.GetMaxLength());
            Assert.Null(entityType.FindProperty("Charm")!.GetElementType()!.GetMaxLength());
            Assert.Null(entityType.FindProperty("Strange")!.GetElementType()!.GetMaxLength());
            Assert.Null(entityType.FindProperty("Stranger")!.GetElementType()!.GetMaxLength());
        }

        [ConditionalFact]
        public virtual void Element_types_can_have_max_length()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection(e => e.Down).ElementType().HasMaxLength(-1);
                    b.PrimitiveCollection<List<int?>>("Charm");
                    b.PrimitiveCollection<List<string?>>("Strange").ElementType().HasMaxLength(512);
                    b.PrimitiveCollection<List<string>>("Stranger").ElementType().HasMaxLength(int.MaxValue);
                });

            var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(CollectionQuarks))!;

            Assert.Null(entityType.FindProperty(nameof(CollectionQuarks.Up))!.GetElementType()!.GetMaxLength());
            Assert.Equal(-1, entityType.FindProperty(nameof(CollectionQuarks.Down))!.GetElementType()!.GetMaxLength());
            Assert.Null(entityType.FindProperty("Charm")!.GetElementType()!.GetMaxLength());
            Assert.Equal(512, entityType.FindProperty("Strange")!.GetElementType()!.GetMaxLength());
            Assert.Equal(int.MaxValue, entityType.FindProperty("Stranger")!.GetElementType()!.GetMaxLength());
        }

        [ConditionalFact]
        public virtual void Element_types_have_default_precision_and_scale()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection<List<decimal?>>("Charm");
                    b.PrimitiveCollection<List<DateTime?>>("Strange");
                    b.PrimitiveCollection<List<decimal>>("Stranger");
                });

            var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(CollectionQuarks))!;

            var property = entityType.FindProperty(nameof(CollectionQuarks.Up))!;
            Assert.Null(property.GetElementType()!.GetPrecision());
            Assert.Null(property.GetElementType()!.GetScale());
            property = entityType.FindProperty(nameof(CollectionQuarks.Down))!;
            Assert.Null(property.GetElementType()!.GetPrecision());
            Assert.Null(property.GetElementType()!.GetScale());
            property = entityType.FindProperty("Charm")!;
            Assert.Null(property.GetElementType()!.GetPrecision());
            Assert.Null(property.GetElementType()!.GetScale());
            property = entityType.FindProperty("Strange")!;
            Assert.Null(property.GetElementType()!.GetPrecision());
            Assert.Null(property.GetElementType()!.GetScale());
            property = entityType.FindProperty("Stranger")!;
            Assert.Null(property.GetElementType()!.GetPrecision());
            Assert.Null(property.GetElementType()!.GetScale());
        }

        [ConditionalFact]
        public virtual void Element_types_can_have_precision_and_scale()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection<List<decimal?>>("Charm").ElementType(b => b.HasPrecision(5, 6));
                    b.PrimitiveCollection<List<DateTime?>>("Strange").ElementType(b => b.HasPrecision(12));
                    b.PrimitiveCollection<List<decimal>>("Stranger");
                });

            var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(CollectionQuarks))!;

            var elementType = entityType.FindProperty(nameof(CollectionQuarks.Up))!.GetElementType()!;
            Assert.Null(elementType.GetPrecision());
            Assert.Null(elementType.GetScale());

            elementType = entityType.FindProperty(nameof(CollectionQuarks.Down))!.GetElementType()!;
            Assert.Null(elementType.GetPrecision());
            Assert.Null(elementType.GetScale());

            elementType = entityType.FindProperty("Charm")!.GetElementType()!;
            Assert.Equal(5, elementType.GetPrecision());
            Assert.Equal(6, elementType.GetScale());

            elementType = entityType.FindProperty("Strange")!.GetElementType()!;
            Assert.Equal(12, elementType.GetPrecision());
            Assert.Null(elementType.GetScale());

            elementType = entityType.FindProperty("Stranger")!.GetElementType()!;
            Assert.Null(elementType.GetPrecision());
            Assert.Null(elementType.GetScale());
        }

        [ConditionalFact]
        public virtual void Element_types_have_default_unicode()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection<List<decimal?>>("Charm");
                    b.PrimitiveCollection<List<DateTime?>>("Strange");
                    b.PrimitiveCollection<List<decimal>>("Stranger");
                });

            var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(CollectionQuarks))!;

            Assert.Null(entityType.FindProperty(nameof(CollectionQuarks.Up))!.GetElementType()!.IsUnicode());
            Assert.Null(entityType.FindProperty(nameof(CollectionQuarks.Down))!.GetElementType()!.IsUnicode());
            Assert.Null(entityType.FindProperty("Charm")!.GetElementType()!.IsUnicode());
            Assert.Null(entityType.FindProperty("Strange")!.GetElementType()!.IsUnicode());
            Assert.Null(entityType.FindProperty("Stranger")!.GetElementType()!.IsUnicode());
        }

        [ConditionalFact]
        public virtual void Element_types_can_have_unicode_set()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection(e => e.Down).ElementType().IsUnicode(false);
                    b.PrimitiveCollection<List<int?>>("Charm");
                    b.PrimitiveCollection<List<string?>>("Strange").ElementType().IsUnicode();
                    b.PrimitiveCollection<List<string>>("Stranger").ElementType().IsUnicode(false);
                });

            var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(CollectionQuarks))!;

            Assert.Null(entityType.FindProperty(nameof(CollectionQuarks.Up))!.GetElementType()!.IsUnicode());
            Assert.False(entityType.FindProperty(nameof(CollectionQuarks.Down))!.GetElementType()!.IsUnicode());
            Assert.Null(entityType.FindProperty("Charm")!.GetElementType()!.IsUnicode());
            Assert.True(entityType.FindProperty("Strange")!.GetElementType()!.IsUnicode());
            Assert.False(entityType.FindProperty("Stranger")!.GetElementType()!.IsUnicode());
        }

        [ConditionalFact]
        public virtual void Conversion_on_base_property_prevents_primitive_collection()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<DerivedCollectionQuarks>();
            modelBuilder.Entity<CollectionQuarks>(b =>
            {
                b.Property(c => c.Down).HasConversion(gs => string.Join(',', gs!),
                    s => new ObservableCollection<string>(s.Split(',', StringSplitOptions.RemoveEmptyEntries)));
            });

            var model = modelBuilder.FinalizeModel();

            var property = model.FindEntityType(typeof(CollectionQuarks))!.FindProperty(nameof(CollectionQuarks.Down))!;
            Assert.False(property.IsPrimitiveCollection);
            Assert.NotNull(property.GetValueConverter());
        }

        [ConditionalFact]
        public virtual void Conversion_on_base_property_prevents_primitive_collection_when_base_first()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder.Entity<CollectionQuarks>(b =>
            {
                b.Property(c => c.Down).HasConversion(gs => string.Join(',', gs!),
                    s => new ObservableCollection<string>(s.Split(',', StringSplitOptions.RemoveEmptyEntries)));
            });

            var property = (IProperty)modelBuilder.Model.FindEntityType(typeof(CollectionQuarks))!.FindProperty(nameof(CollectionQuarks.Down))!;
            Assert.False(property.IsPrimitiveCollection);

            modelBuilder.Entity<DerivedCollectionQuarks>();

            var model = modelBuilder.FinalizeModel();
            property = model.FindEntityType(typeof(CollectionQuarks))!.FindProperty(nameof(CollectionQuarks.Down))!;
            Assert.False(property.IsPrimitiveCollection);
            Assert.NotNull(property.GetValueConverter());
        }

        [ConditionalFact]
        public virtual void Element_types_can_have_provider_type_set()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection(e => e.Up);
                    b.PrimitiveCollection(e => e.Down).ElementType().HasConversion<byte[]>();
                    b.PrimitiveCollection<List<int>>("Charm").ElementType().HasConversion<long, CustomValueComparer<int>>();
                    b.PrimitiveCollection<string[]>("Strange").ElementType().HasConversion((ValueConverter?)null);
                    b.PrimitiveCollection<IList<byte[]>>("Top").ElementType().HasConversion<string>(new CustomValueComparer<byte[]>());
                });

            var entityType = modelBuilder.FinalizeModel().FindEntityType(typeof(CollectionQuarks))!;

            var up = entityType.FindProperty("Up")!.GetElementType()!;
            Assert.Null(up.GetProviderClrType());
            Assert.True(up.GetValueComparer()?.IsDefault());

            var down = entityType.FindProperty("Down")!.GetElementType()!;
            Assert.Same(typeof(byte[]), down.GetProviderClrType());
            Assert.True(down.GetValueComparer()?.IsDefault());

            var charm = entityType.FindProperty("Charm")!.GetElementType()!;
            Assert.Same(typeof(long), charm.GetProviderClrType());
            Assert.IsType<CustomValueComparer<int>>(charm.GetValueComparer());

            var strange = entityType.FindProperty("Strange")!.GetElementType()!;
            Assert.Null(strange.GetProviderClrType());
            Assert.True(strange.GetValueComparer()?.IsDefault());

            var top = entityType.FindProperty("Top")!.GetElementType()!;
            Assert.Same(typeof(string), top.GetProviderClrType());
            Assert.IsType<CustomValueComparer<byte[]>>(top.GetValueComparer());
        }

        [ConditionalFact]
        public virtual void Element_types_can_have_non_generic_value_converter_set()
        {
            var modelBuilder = CreateModelBuilder();

            ValueConverter stringConverter = new StringToBytesConverter(Encoding.UTF8);
            ValueConverter intConverter = new CastingConverter<int, long>();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection(e => e.Up);
                    b.PrimitiveCollection(e => e.Down).ElementType().HasConversion(stringConverter);
                    b.PrimitiveCollection<int[]>("Charm").ElementType().HasConversion(intConverter, null);
                    b.PrimitiveCollection<List<string>>("Strange").ElementType().HasConversion(stringConverter);
                    b.PrimitiveCollection<List<string>>("Strange").ElementType().HasConversion((ValueConverter?)null);
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = (IReadOnlyEntityType)model.FindEntityType(typeof(CollectionQuarks))!;

            Assert.Null(entityType.FindProperty("Up")!.GetElementType()!.GetValueConverter());

            var down = entityType.FindProperty("Down")!.GetElementType()!;
            Assert.Same(stringConverter, down.GetValueConverter());
            Assert.True(down.GetValueComparer()?.IsDefault());

            var charm = entityType.FindProperty("Charm")!.GetElementType()!;
            Assert.Same(intConverter, charm.GetValueConverter());
            Assert.True(charm.GetValueComparer()?.IsDefault());

            Assert.Null(entityType.FindProperty("Strange")!.GetElementType()!.GetValueConverter());
        }

        [ConditionalFact]
        public virtual void Element_types_can_have_custom_type_value_converter_type_set()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection(e => e.Up).ElementType().HasConversion<int, CustomValueComparer<int>>();
                    b.PrimitiveCollection(e => e.Down).ElementType()
                        .HasConversion<UTF8StringToBytesConverter, CustomValueComparer<string>>();
                    b.PrimitiveCollection<int[]>("Charm").ElementType()
                        .HasConversion<CastingConverter<int, long>, CustomValueComparer<int>>();
                    b.PrimitiveCollection<IList<string>>("Strange").ElementType()
                        .HasConversion<UTF8StringToBytesConverter, CustomValueComparer<string>>();
                    b.PrimitiveCollection<List<string>>("Strange").ElementType().HasConversion((ValueConverter?)null);
                    b.PrimitiveCollection<string[]>("Top").ElementType().HasConversion<string>(new CustomValueComparer<string>());
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = (IReadOnlyEntityType)model.FindEntityType(typeof(CollectionQuarks))!;

            var up = entityType.FindProperty("Up")!.GetElementType()!;
            Assert.Equal(typeof(int), up.GetProviderClrType());
            Assert.Null(up.GetValueConverter());
            Assert.IsType<CustomValueComparer<int>>(up.GetValueComparer());

            var down = entityType.FindProperty("Down")!.GetElementType()!;
            Assert.IsType<UTF8StringToBytesConverter>(down.GetValueConverter());
            Assert.IsType<CustomValueComparer<string>>(down.GetValueComparer());

            var charm = entityType.FindProperty("Charm")!.GetElementType()!;
            Assert.IsType<CastingConverter<int, long>>(charm.GetValueConverter());
            Assert.IsType<CustomValueComparer<int>>(charm.GetValueComparer());

            var strange = entityType.FindProperty("Strange")!.GetElementType()!;
            Assert.Null(strange.GetValueConverter());
            Assert.True(strange.GetValueComparer()?.IsDefault());

            var top = entityType.FindProperty("Top")!.GetElementType()!;
            Assert.Null(top.GetValueConverter());
            Assert.IsType<CustomValueComparer<string>>(top.GetValueComparer());
        }

        [ConditionalFact]
        public virtual void Primitive_collections_can_have_value_converter_set()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    b.PrimitiveCollection(e => e.Up);
                    b.PrimitiveCollection(e => e.Down).ElementType().HasConversion(
                        new ValueConverter<string, int>(v => int.Parse(v), v => v.ToString())!);
                    b.PrimitiveCollection<List<int>>("Charm").ElementType().HasConversion(
                        new ValueConverter<int, long>(v => v, v => (int)v), new CustomValueComparer<int>());
                    b.PrimitiveCollection<float[]>("Strange").ElementType().HasConversion(
                        new ValueConverter<float, double>(v => v, v => (float)v), new CustomValueComparer<float>());
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(CollectionQuarks))!;

            var up = entityType.FindProperty("Up")!.GetElementType()!;
            Assert.Null(up.GetProviderClrType());
            Assert.Null(up.GetValueConverter());
            Assert.True(up.GetValueComparer()?.IsDefault());

            var down = entityType.FindProperty("Down")!.GetElementType()!;
            Assert.IsType<ValueConverter<string, int>>(down.GetValueConverter());
            Assert.True(down.GetValueComparer()?.IsDefault());

            var charm = entityType.FindProperty("Charm")!.GetElementType()!;
            Assert.IsType<ValueConverter<int, long>>(charm.GetValueConverter());
            Assert.IsType<CustomValueComparer<int>>(charm.GetValueComparer());

            var strange = entityType.FindProperty("Strange")!.GetElementType()!;
            Assert.IsType<ValueConverter<float, double>>(strange.GetValueConverter());
            Assert.IsType<CustomValueComparer<float>>(strange.GetValueComparer());
        }

        [ConditionalFact]
        public virtual void Value_converter_type_on_primitive_collection_is_checked()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionQuarks>(
                b =>
                {
                    Assert.Equal(
                        CoreStrings.ConverterPropertyMismatchElement("string", "CollectionQuarks", "Up", "int"),
                        Assert.Throws<InvalidOperationException>(
                            () => b.PrimitiveCollection(e => e.Up).ElementType().HasConversion(
                                new StringToBytesConverter(Encoding.UTF8))).Message);
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = model.FindEntityType(typeof(CollectionQuarks))!;
            Assert.Null(entityType.FindProperty("Up")!.GetElementType()!.GetValueConverter());
        }
    }
}
