// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Xunit.Sdk;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class CosmosModelBuilderGenericTest : ModelBuilderTest
{
    public class CosmosGenericNonRelationship(CosmosModelBuilderFixture fixture) : NonRelationshipTestBase(fixture), IClassFixture<CosmosModelBuilderFixture>
    {
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        public override void Element_types_can_have_custom_type_value_converter_type_set()
            => Assert.Equal(
                CosmosStrings.ElementWithValueConverter("int[]", "CollectionQuarks", "Charm", "int"),
                Assert.Throws<InvalidOperationException>(base.Element_types_can_have_custom_type_value_converter_type_set).Message);

        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        public override void Element_types_can_have_non_generic_value_converter_set()
            => Assert.Equal(
                CosmosStrings.ElementWithValueConverter("int[]", "CollectionQuarks", "Charm", "int"),
                Assert.Throws<InvalidOperationException>(base.Element_types_can_have_non_generic_value_converter_set).Message);

        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        public override void Element_types_can_have_provider_type_set()
            => Assert.Equal(
                CosmosStrings.ElementWithValueConverter("List<int>", "CollectionQuarks", "Charm", "int"),
                Assert.Throws<InvalidOperationException>(base.Element_types_can_have_provider_type_set).Message);

        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        public override void Primitive_collections_can_have_value_converter_set()
            => Assert.Equal(
                CosmosStrings.ElementWithValueConverter("List<int>", "CollectionQuarks", "Charm", "int"),
                Assert.Throws<InvalidOperationException>(base.Primitive_collections_can_have_value_converter_set).Message);

        public override void Can_add_contained_indexes()
            => Assert.Equal(
                CosmosStrings.IndexesExist(nameof(Customer), "Id"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_add_contained_indexes).Message);

        public override void Can_add_index()
            => Assert.Equal(
                CosmosStrings.IndexesExist(nameof(Customer), "Name"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_add_index).Message);

        public override void Can_add_index_when_no_clr_property()
            => Assert.Equal(
                CosmosStrings.IndexesExist(nameof(Customer), "Index"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_add_index_when_no_clr_property).Message);

        public override void Can_add_multiple_indexes()
            => Assert.Equal(
                CosmosStrings.IndexesExist(nameof(Customer), "Id"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_add_multiple_indexes).Message);

        public override void Can_set_composite_index_on_an_entity_with_fields()
            => Assert.Equal(
                CosmosStrings.IndexesExist(nameof(EntityWithFields), "TenantId,CompanyId"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_set_composite_index_on_an_entity_with_fields).Message);

        public override void Can_set_index_on_an_entity_with_fields()
            => Assert.Equal(
                CosmosStrings.IndexesExist(nameof(EntityWithFields), "CompanyId"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_set_index_on_an_entity_with_fields).Message);

        public override void Properties_can_set_row_version()
            => Assert.Equal(
                CosmosStrings.NonETagConcurrencyToken(nameof(Quarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Properties_can_set_row_version).Message);

        public override void Properties_can_be_made_concurrency_tokens()
            => Assert.Equal(
                CosmosStrings.NonETagConcurrencyToken(nameof(Quarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Properties_can_be_made_concurrency_tokens).Message);

        public override void Primitive_collections_can_be_made_concurrency_tokens()
            => Assert.Equal(
                CosmosStrings.NonETagConcurrencyToken(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Primitive_collections_can_be_made_concurrency_tokens).Message);

        public override void Properties_can_have_custom_type_value_converter_type_set()
            => Properties_can_have_custom_type_value_converter_type_set<string>();

        public override void Properties_can_have_non_generic_value_converter_set()
            => Properties_can_have_non_generic_value_converter_set<string>();

        public override void Properties_can_have_provider_type_set()
            => Properties_can_have_provider_type_set<string>();

        public override void Properties_can_be_set_to_generate_values_on_Add()
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
            Assert.Equal(ValueGenerated.Never, entityType.FindProperty(Customer.IdProperty.Name)!.ValueGenerated);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, entityType.FindProperty("Up")!.ValueGenerated);
            Assert.Equal(ValueGenerated.Never, entityType.FindProperty("Down")!.ValueGenerated);
            Assert.Equal(ValueGenerated.OnUpdateSometimes, entityType.FindProperty("Charm")!.ValueGenerated);
            Assert.Equal(ValueGenerated.Never, entityType.FindProperty("Strange")!.ValueGenerated);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, entityType.FindProperty("Top")!.ValueGenerated);
            Assert.Equal(ValueGenerated.OnUpdate, entityType.FindProperty("Bottom")!.ValueGenerated);
        }

        [ConditionalFact]
        public virtual void Partition_key_is_added_to_the_keys()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders)
                .HasPartitionKey(b => b.AlternateKey)
                .Property(b => b.AlternateKey).HasConversion<string>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(
                new[] { nameof(Customer.Id), nameof(Customer.AlternateKey) },
                entity.FindPrimaryKey()!.Properties.Select(p => p.Name));

            Assert.Equal(1, entity.GetKeys().Count());
        }

        [ConditionalFact]
        public virtual void Hierarchical_partition_key_is_added_to_the_keys()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders)
                .HasPartitionKey(b => new { b.Title, b.Name });

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(
                new[] { nameof(Customer.Title), nameof(Customer.Name) },
                entity.GetPartitionKeyProperties().Select(p => p.Name));

            Assert.Equal(1, entity.GetKeys().Count());
        }

        [ConditionalFact]
        public virtual void Three_level_hierarchical_partition_key_is_added_to_the_keys()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders)
                .HasPartitionKey(b => new { b.Title, b.Name, b.AlternateKey })
                .Property(b => b.AlternateKey).HasConversion<string>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(
                new[] { nameof(Customer.Title), nameof(Customer.Name), nameof(Customer.AlternateKey) },
                entity.GetPartitionKeyProperties().Select(p => p.Name));

            Assert.Equal(1, entity.GetKeys().Count());
        }

        [ConditionalFact]
        public virtual void Partition_key_is_added_to_the_alternate_key_if_primary_key_contains_id()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>(
                b =>
                {
                    b.HasAnnotation(CosmosAnnotationNames.HasShadowId, true);
                    b.HasKey(CosmosJsonIdConvention.DefaultIdPropertyName);

                    b.Ignore(b => b.Details)
                        .Ignore(b => b.Orders)
                        .HasPartitionKey(b => b.AlternateKey)
                        .Property(b => b.AlternateKey).HasConversion<string>();
                });

            var model = modelBuilder.FinalizeModel();
            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(
                new[] { CosmosJsonIdConvention.DefaultIdPropertyName },
                entity.FindPrimaryKey()!.Properties.Select(p => p.Name));

            Assert.Equal(1, entity.GetKeys().Count());
        }

        [ConditionalFact]
        public virtual void Hierarchical_partition_key_is_added_to_the_alternate_key_if_primary_key_contains_id()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().AlwaysHasShadowId();
            modelBuilder.Entity<Customer>().HasKey(CosmosJsonIdConvention.DefaultIdPropertyName);

            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders)
                .HasPartitionKey(
                    b => new
                    {
                        b.AlternateKey,
                        b.Name,
                        b.Title
                    })
                .Property(b => b.AlternateKey).HasConversion<string>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(
                new[] { nameof(Customer.AlternateKey), nameof(Customer.Name), nameof(Customer.Title) },
                entity.GetPartitionKeyProperties().Select(p => p.Name));
            Assert.Equal(
                new[] { CosmosJsonIdConvention.DefaultIdPropertyName },
                entity.FindPrimaryKey()!.Properties.Select(p => p.Name));
        }

        [ConditionalFact]
        public virtual void No_id_property_created_if_another_property_mapped_to_id()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>()
                .Property(c => c.Name)
                .ToJsonProperty(CosmosJsonIdConvention.IdPropertyJsonName);
            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders);

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Null(entity.FindProperty(CosmosJsonIdConvention.DefaultIdPropertyName));

            Assert.Equal(1, entity.GetKeys().Count());
        }

        [ConditionalFact]
        public virtual void No_id_property_created_if_another_property_mapped_to_id_in_pk()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>()
                .Property(c => c.Name)
                .ToJsonProperty(CosmosJsonIdConvention.IdPropertyJsonName);
            modelBuilder.Entity<Customer>()
                .Ignore(c => c.Details)
                .Ignore(c => c.Orders)
                .HasKey(c => c.Name);

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Null(entity.FindProperty(CosmosJsonIdConvention.DefaultIdPropertyName));
            Assert.DoesNotContain(entity.GetKeys(), k => k != entity.FindPrimaryKey());

            var idProperty = entity.GetDeclaredProperties()
                .Single(p => p.GetJsonPropertyName() == CosmosJsonIdConvention.IdPropertyJsonName);
            Assert.Single(idProperty.GetContainingKeys());
            Assert.Null(idProperty.GetValueGeneratorFactory());

            Assert.Equal(1, entity.GetKeys().Count());
        }

        [ConditionalFact]
        public virtual void No_alternate_key_is_created_if_primary_key_contains_id()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().AlwaysHasShadowId();
            modelBuilder.Entity<Customer>().HasKey(CosmosJsonIdConvention.DefaultIdPropertyName);

            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders);

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(
                new[] { CosmosJsonIdConvention.DefaultIdPropertyName },
                entity.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.DoesNotContain(entity.GetKeys(), k => k != entity.FindPrimaryKey());

            Assert.Equal(1, entity.GetKeys().Count());
        }

        [ConditionalFact]
        public virtual void No_alternate_key_is_created_if_primary_key_contains_id_and_partition_key()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().AlwaysHasShadowId();
            modelBuilder.Entity<Customer>().HasKey(nameof(Customer.AlternateKey), CosmosJsonIdConvention.DefaultIdPropertyName);

            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders)
                .HasPartitionKey(b => b.AlternateKey)
                .Property(b => b.AlternateKey).HasConversion<string>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(
                new[] { nameof(Customer.AlternateKey), CosmosJsonIdConvention.DefaultIdPropertyName },
                entity.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.DoesNotContain(entity.GetKeys(), k => k != entity.FindPrimaryKey());
        }

        [ConditionalFact]
        public virtual void No_alternate_key_is_created_if_primary_key_contains_id_and_hierarchical_partition_key()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().AlwaysHasShadowId();

            modelBuilder.Entity<Customer>().HasKey(
                nameof(Customer.AlternateKey),
                nameof(Customer.Name),
                nameof(Customer.Title),
                CosmosJsonIdConvention.DefaultIdPropertyName);

            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders)
                .HasPartitionKey(
                    b => new
                    {
                        b.AlternateKey,
                        b.Name,
                        b.Title
                    })
                .Property(b => b.AlternateKey).HasConversion<string>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(
                new[] { nameof(Customer.AlternateKey), nameof(Customer.Name), nameof(Customer.Title) },
                entity.GetPartitionKeyProperties().Select(p => p.Name));

            Assert.Equal(
                new[]
                {
                    nameof(Customer.AlternateKey),
                    nameof(Customer.Name),
                    nameof(Customer.Title),
                    CosmosJsonIdConvention.DefaultIdPropertyName
                },
                entity.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.DoesNotContain(entity.GetKeys(), k => k != entity.FindPrimaryKey());
        }

        [ConditionalFact]
        public virtual void No_alternate_key_is_created_if_primary_key_contains_id_and_hierarchical_partition_key_in_different_order()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().AlwaysHasShadowId();

            modelBuilder.Entity<Customer>().HasKey(
                nameof(Customer.Title),
                nameof(Customer.Name),
                nameof(Customer.AlternateKey),
                CosmosJsonIdConvention.DefaultIdPropertyName);

            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders)
                .HasPartitionKey(
                    b => new
                    {
                        b.AlternateKey,
                        b.Name,
                        b.Title
                    })
                .Property(b => b.AlternateKey).HasConversion<string>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(
                new[] { nameof(Customer.AlternateKey), nameof(Customer.Name), nameof(Customer.Title) },
                entity.GetPartitionKeyProperties().Select(p => p.Name));

            Assert.Equal(
                new[]
                {
                    nameof(Customer.Title),
                    nameof(Customer.Name),
                    nameof(Customer.AlternateKey),
                    CosmosJsonIdConvention.DefaultIdPropertyName
                },
                entity.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.DoesNotContain(entity.GetKeys(), k => k != entity.FindPrimaryKey());
        }

        [ConditionalFact]
        public virtual void Hierarchical_partition_key_is_added_to_the_alternate_key_if_primary_key_contains_part_of_partition_key()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().AlwaysHasShadowId();

            modelBuilder.Entity<Customer>().HasKey(
                nameof(Customer.Title),
                nameof(Customer.AlternateKey),
                CosmosJsonIdConvention.DefaultIdPropertyName);

            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders)
                .HasPartitionKey(
                    b => new
                    {
                        b.Title,
                        b.AlternateKey,
                        b.Name
                    })
                .Property(b => b.AlternateKey).HasConversion<string>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(
                new[] { nameof(Customer.Title), nameof(Customer.AlternateKey), nameof(Customer.Name) },
                entity.GetPartitionKeyProperties().Select(p => p.Name));

            Assert.Equal(
                new[]
                {
                    nameof(Customer.Title),
                    nameof(Customer.AlternateKey),
                    CosmosJsonIdConvention.DefaultIdPropertyName
                },
                entity.FindPrimaryKey()!.Properties.Select(p => p.Name));
        }

        [ConditionalFact]
        public virtual void No_alternate_key_is_created_if_id_is_partition_key()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().HasKey(nameof(Customer.AlternateKey));
            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders)
                .HasPartitionKey(b => b.AlternateKey)
                .Property(b => b.AlternateKey).HasConversion<string>().ToJsonProperty("id");

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(
                new[] { nameof(Customer.AlternateKey) },
                entity.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.DoesNotContain(entity.GetKeys(), k => k != entity.FindPrimaryKey());
        }

        [ConditionalFact]
        public virtual void No_alternate_key_is_created_if_id_is_hierarchical_partition_key()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().HasKey(e => new { e.Name, e.AlternateKey, e.Title });
            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders)
                .HasPartitionKey(b => new { b.Name, b.AlternateKey, b.Title })
                .Property(b => b.AlternateKey).HasConversion<string>().ToJsonProperty("id");

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(
                new[] { nameof(Customer.Name), nameof(Customer.AlternateKey), nameof(Customer.Title) },
                entity.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.DoesNotContain(entity.GetKeys(), k => k != entity.FindPrimaryKey());
        }

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class CosmosGenericComplexType(CosmosModelBuilderFixture fixture) : ComplexTypeTestBase(fixture), IClassFixture<CosmosModelBuilderFixture>
    {
        public override void Properties_can_have_custom_type_value_converter_type_set()
            => Properties_can_have_custom_type_value_converter_type_set<string>();

        public override void Properties_can_have_non_generic_value_converter_set()
            => Properties_can_have_non_generic_value_converter_set<string>();

        public override void Properties_can_have_provider_type_set()
            => Properties_can_have_provider_type_set<string>();

        public override void Can_set_complex_property_annotation()
        {
            var modelBuilder = CreateModelBuilder();

            var complexPropertyBuilder = modelBuilder
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(e => e.Customer)
                .HasTypeAnnotation("foo", "bar")
                .HasPropertyAnnotation("foo2", "bar2")
                .Ignore(c => c.Details)
                .Ignore(c => c.Orders);

            var model = modelBuilder.FinalizeModel();
            var complexProperty = model.FindEntityType(typeof(ComplexProperties))!.GetComplexProperties().Single();

            Assert.Equal("bar", complexProperty.ComplexType["foo"]);
            Assert.Equal("bar2", complexProperty["foo2"]);
            Assert.Equal(typeof(Customer).Name, complexProperty.Name);
            Assert.Equal(
                @"Customer (Customer) Required
  ComplexType: ComplexProperties.Customer#Customer
    Properties: "
                + @"
      AlternateKey (Guid) Required
      Id (int) Required
      Name (string)
      Notes (List<string>) Element type: string Required
      Title (string) Required", complexProperty.ToDebugString(), ignoreLineEndingDifferences: true);
        }

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class CosmosGenericInheritance(CosmosModelBuilderFixture fixture) : InheritanceTestBase(fixture), IClassFixture<CosmosModelBuilderFixture>
    {
        public override void Base_type_can_be_discovered_after_creating_foreign_keys_on_derived()
        {
            var mb = CreateModelBuilder();
            mb.Entity<AL>();
            mb.Entity<L>();

            var mutableEntityTypes = mb.Model.GetEntityTypes().Where(e => e.ClrType == typeof(Q)).ToList();

            Assert.Equal(2, mutableEntityTypes.Count);

            foreach (var mutableEntityType in mutableEntityTypes)
            {
                var mutableProperty = mutableEntityType.FindProperty(nameof(Q.ID))!;

                Assert.Equal(ValueGenerated.Never, mutableProperty.ValueGenerated);
            }
        }

        public override void Relationships_on_derived_types_are_discovered_first_if_base_is_one_sided()
            // Base discovered as owned
            => Assert.Throws<NullReferenceException>(
                base.Relationships_on_derived_types_are_discovered_first_if_base_is_one_sided);

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class CosmosGenericOneToMany(CosmosModelBuilderFixture fixture) : OneToManyTestBase(fixture), IClassFixture<CosmosModelBuilderFixture>
    {
        public override void Creates_overlapping_foreign_keys_with_different_nullability()
            => Assert.Equal(
                CosmosStrings.IndexesExist(nameof(Product), "Id,OrderId"),
                Assert.Throws<InvalidOperationException>(
                    base.Creates_overlapping_foreign_keys_with_different_nullability).Message);

        public override void Navigation_to_shared_type_is_not_discovered_by_convention()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<CollectionNavigationToSharedType>();

            var model = modelBuilder.FinalizeModel();

            var principal = model.FindEntityType(typeof(CollectionNavigationToSharedType))!;
            var owned = principal.FindNavigation(nameof(CollectionNavigationToSharedType.Navigation))!.TargetEntityType;
            Assert.True(owned.IsOwned());
            Assert.True(owned.HasSharedClrType);
            Assert.Equal(
                "CollectionNavigationToSharedType.Navigation#Dictionary<string, object>",
                owned.DisplayName());
        }

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class CosmosGenericManyToOne(CosmosModelBuilderFixture fixture) : ManyToOneTestBase(fixture), IClassFixture<CosmosModelBuilderFixture>
    {
        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class CosmosGenericOneToOne(CosmosModelBuilderFixture fixture) : OneToOneTestBase(fixture), IClassFixture<CosmosModelBuilderFixture>
    {
        public override void Navigation_to_shared_type_is_not_discovered_by_convention()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<ReferenceNavigationToSharedType>();

            var model = modelBuilder.FinalizeModel();

            var principal = model.FindEntityType(typeof(ReferenceNavigationToSharedType))!;
            var owned = principal.FindNavigation(nameof(ReferenceNavigationToSharedType.Navigation))!.TargetEntityType;
            Assert.True(owned.IsOwned());
            Assert.True(owned.HasSharedClrType);
            Assert.Equal(
                "ReferenceNavigationToSharedType.Navigation#Dictionary<string, object>",
                owned.DisplayName());
        }

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class CosmosGenericManyToMany(CosmosModelBuilderFixture fixture) : ManyToManyTestBase(fixture), IClassFixture<CosmosModelBuilderFixture>
    {
        [ConditionalFact]
        public virtual void Can_use_shared_type_as_join_entity_with_partition_keys()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<OneToManyNavPrincipal>();
            modelBuilder.Ignore<OneToOneNavPrincipal>();

            modelBuilder.Entity<ManyToManyNavPrincipal>(
                mb =>
                {
                    mb.Property<string>("PartitionId");
                    mb.HasPartitionKey("PartitionId");
                });

            modelBuilder.Entity<NavDependent>(
                mb =>
                {
                    mb.Property<string>("PartitionId");
                    mb.HasPartitionKey("PartitionId");
                });

            modelBuilder.Entity<ManyToManyNavPrincipal>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals)
                .UsingEntity<Dictionary<string, object>>(
                    "JoinType",
                    e => e.HasOne<NavDependent>().WithMany().HasAnnotation("Right", "Foo"),
                    e => e.HasOne<ManyToManyNavPrincipal>().WithMany().HasAnnotation("Left", "Bar"));

            modelBuilder.Entity<ManyToManyNavPrincipal>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals)
                .UsingEntity<Dictionary<string, object>>(
                    "JoinType",
                    e => e.HasOne<NavDependent>().WithMany().HasForeignKey("DependentId", "PartitionId"),
                    e => e.HasOne<ManyToManyNavPrincipal>().WithMany().HasForeignKey("PrincipalId", "PartitionId"),
                    e => e.HasPartitionKey("PartitionId"));

            var model = modelBuilder.FinalizeModel();

            var joinType = model.FindEntityType("JoinType")!;
            Assert.NotNull(joinType);
            Assert.Collection(joinType.GetForeignKeys(),
                fk => Assert.Equal("Foo", fk["Right"]),
                fk => Assert.Equal("Bar", fk["Left"]));
            Assert.Equal(3, joinType.FindPrimaryKey()!.Properties.Count);
            Assert.Equal(6, joinType.GetProperties().Count());
            Assert.Equal("DbContext", joinType.GetContainer());
            Assert.Equal(["PartitionId"], joinType.GetPartitionKeyPropertyNames());
            Assert.Equal("PartitionId", joinType.FindPrimaryKey()!.Properties.Last().Name);

#pragma warning disable CS0618 // Type or member is obsolete
            Assert.Equal("PartitionId", joinType.GetPartitionKeyPropertyName());
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [ConditionalFact]
        public virtual void Can_use_shared_type_as_join_entity_with_hierarchical_partition_keys()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<OneToManyNavPrincipal>();
            modelBuilder.Ignore<OneToOneNavPrincipal>();

            modelBuilder.Entity<ManyToManyNavPrincipal>(
                mb =>
                {
                    mb.Property<string>("PartitionId1");
                    mb.Property<string>("PartitionId2");
                    mb.Property<string>("PartitionId3");
                    mb.HasPartitionKey("PartitionId1", "PartitionId2", "PartitionId3");
                });

            modelBuilder.Entity<NavDependent>(
                mb =>
                {
                    mb.Property<string>("PartitionId1");
                    mb.Property<string>("PartitionId2");
                    mb.Property<string>("PartitionId3");
                    mb.HasPartitionKey("PartitionId1", "PartitionId2", "PartitionId3");
                });

            modelBuilder.Entity<ManyToManyNavPrincipal>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals)
                .UsingEntity<Dictionary<string, object>>(
                    "JoinType",
                    e => e.HasOne<NavDependent>().WithMany().HasAnnotation("Right", "Foo"),
                    e => e.HasOne<ManyToManyNavPrincipal>().WithMany().HasAnnotation("Left", "Bar"));

            modelBuilder.Entity<ManyToManyNavPrincipal>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals)
                .UsingEntity<Dictionary<string, object>>(
                    "JoinType",
                    e => e.HasOne<NavDependent>().WithMany().HasForeignKey("DependentId", "PartitionId1", "PartitionId2", "PartitionId3"),
                    e => e.HasOne<ManyToManyNavPrincipal>().WithMany().HasForeignKey("PrincipalId", "PartitionId1", "PartitionId2", "PartitionId3"),
                    e => e.HasPartitionKey("PartitionId1", "PartitionId2", "PartitionId3"));

            var model = modelBuilder.FinalizeModel();

            var joinType = model.FindEntityType("JoinType")!;
            Assert.NotNull(joinType);
            Assert.Collection(joinType.GetForeignKeys(),
                fk => Assert.Equal("Foo", fk["Right"]),
                fk => Assert.Equal("Bar", fk["Left"]));

            Assert.Equal(
                new[] { "PartitionId1", "PartitionId2", "PartitionId3" },
                joinType.GetPartitionKeyProperties().Select(p => p.Name));

            Assert.Equal(
                new[] { "DependentId", "PrincipalId", "PartitionId1", "PartitionId2", "PartitionId3" },
                joinType.FindPrimaryKey()!.Properties.Select(p => p.Name));
        }

        [ConditionalFact]
        public virtual void Can_use_implicit_join_entity_with_partition_keys()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<OneToManyNavPrincipal>();
            modelBuilder.Ignore<OneToOneNavPrincipal>();

            modelBuilder.Entity<ManyToManyNavPrincipal>(
                mb =>
                {
                    mb.Ignore(e => e.Dependents);
                    mb.Property<string>("PartitionId");
                    mb.HasPartitionKey("PartitionId");
                });

            modelBuilder.Entity<NavDependent>(
                mb =>
                {
                    mb.Property<string>("PartitionId");
                    mb.HasPartitionKey("PartitionId");
                });

            modelBuilder.Entity<ManyToManyNavPrincipal>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals);

            var model = modelBuilder.FinalizeModel();

            var joinType = model.FindEntityType("ManyToManyNavPrincipalNavDependent");
            Assert.NotNull(joinType);
            Assert.Equal(2, joinType.GetForeignKeys().Count());
            Assert.Equal(3, joinType.FindPrimaryKey()!.Properties.Count);
            Assert.Equal(6, joinType.GetProperties().Count());
            Assert.Equal("DbContext", joinType.GetContainer());
            Assert.Equal(["PartitionId"], joinType.GetPartitionKeyPropertyNames());
            Assert.Equal("PartitionId", joinType.FindPrimaryKey()!.Properties.Last().Name);

#pragma warning disable CS0618 // Type or member is obsolete
            Assert.Equal("PartitionId", joinType.GetPartitionKeyPropertyName());
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [ConditionalFact]
        public virtual void Can_use_implicit_join_entity_with_hierarchical_partition_keys()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<OneToManyNavPrincipal>();
            modelBuilder.Ignore<OneToOneNavPrincipal>();

            modelBuilder.Entity<ManyToManyNavPrincipal>(
                mb =>
                {
                    mb.Ignore(e => e.Dependents);
                    mb.Property<string>("PartitionId1");
                    mb.Property<string>("PartitionId2");
                    mb.Property<string>("PartitionId3");
                    mb.HasPartitionKey("PartitionId1", "PartitionId2", "PartitionId3");
                });

            modelBuilder.Entity<NavDependent>(
                mb =>
                {
                    mb.Property<string>("PartitionId1");
                    mb.Property<string>("PartitionId2");
                    mb.Property<string>("PartitionId3");
                    mb.HasPartitionKey("PartitionId1", "PartitionId2", "PartitionId3");
                });

            modelBuilder.Entity<ManyToManyNavPrincipal>()
                .HasMany(e => e.Dependents)
                .WithMany(e => e.ManyToManyPrincipals);

            var model = modelBuilder.FinalizeModel();

            var joinType = model.FindEntityType("ManyToManyNavPrincipalNavDependent");
            Assert.NotNull(joinType);

            Assert.Equal(
                new[] { "PartitionId1", "PartitionId2", "PartitionId3" },
                joinType.GetPartitionKeyProperties().Select(p => p.Name));

            Assert.Equal(
                new[] { "Id", "Id1", "PartitionId1", "PartitionId2", "PartitionId3" },
                joinType.FindPrimaryKey()!.Properties.Select(p => p.Name));

            Assert.Equal(2, joinType.GetForeignKeys().Count());
            Assert.Equal(5, joinType.FindPrimaryKey()!.Properties.Count);
            Assert.Equal(8, joinType.GetProperties().Count());
            Assert.Equal("DbContext", joinType.GetContainer());
        }

        [ConditionalFact]
        public virtual void Can_use_implicit_join_entity_with_partition_keys_changed()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Ignore<OneToManyNavPrincipal>();
            modelBuilder.Ignore<OneToOneNavPrincipal>();

            modelBuilder.Entity<ManyToManyNavPrincipal>(
                mb =>
                {
                    mb.Property<string>("PartitionId");
                    mb.HasPartitionKey("PartitionId");
                });

            modelBuilder.Entity<NavDependent>(
                mb =>
                {
                    mb.Property<string>("PartitionId");
                    mb.HasPartitionKey("PartitionId");
                });

            modelBuilder.Entity<ManyToManyNavPrincipal>(
                mb =>
                {
                    mb.Property<string>("Partition2Id");
                    mb.HasPartitionKey("Partition2Id");
                });

            modelBuilder.Entity<NavDependent>(
                mb =>
                {
                    mb.Property<string>("Partition2Id");
                    mb.HasPartitionKey("Partition2Id");
                });

            var model = modelBuilder.FinalizeModel();

            var joinType = model.FindEntityType("ManyToManyNavPrincipalNavDependent");
            Assert.NotNull(joinType);
            Assert.Equal(2, joinType.GetForeignKeys().Count());
            Assert.Equal(3, joinType.FindPrimaryKey()!.Properties.Count);
            Assert.Equal(6, joinType.GetProperties().Count());
            Assert.Equal("DbContext", joinType.GetContainer());
            Assert.Equal(["Partition2Id"], joinType.GetPartitionKeyPropertyNames());
            Assert.Equal("Partition2Id", joinType.FindPrimaryKey()!.Properties.Last().Name);

#pragma warning disable CS0618 // Type or member is obsolete
            Assert.Equal("Partition2Id", joinType.GetPartitionKeyPropertyName());
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public override void Join_type_is_automatically_configured_by_convention()
            // Cosmos many-to-many. Issue #23523.
            => Assert.Equal(
                CoreStrings.NavigationNotAdded(
                    nameof(ImplicitManyToManyA), nameof(ImplicitManyToManyA.Bs), "List<ImplicitManyToManyB>"),
                Assert.Throws<InvalidOperationException>(
                    base.Join_type_is_automatically_configured_by_convention).Message);

        public override void ForeignKeyAttribute_configures_the_properties()
            // Cosmos many-to-many. Issue #23523.
            => Assert.Equal(
                CoreStrings.NavigationNotAdded(
                    nameof(CategoryWithAttribute), nameof(CategoryWithAttribute.Products), "ICollection<ProductWithAttribute>"),
                Assert.Throws<InvalidOperationException>(
                    base.ForeignKeyAttribute_configures_the_properties).Message);

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class CosmosGenericOwnedTypes(CosmosModelBuilderFixture fixture) : OwnedTypesTestBase(fixture), IClassFixture<CosmosModelBuilderFixture>
    {
        public override void Deriving_from_owned_type_throws()
            // On Cosmos the base type starts as owned
            => Assert.Contains(
                "No exception was thrown",
                Assert.Throws<ThrowsException>(base.Deriving_from_owned_type_throws).Message);

        public override void Configuring_base_type_as_owned_throws()
            // On Cosmos the base type starts as owned
            => Assert.Contains(
                "No exception was thrown",
                Assert.Throws<ThrowsException>(base.Deriving_from_owned_type_throws).Message);

        public override void Can_configure_one_to_many_owned_type_with_fields()
            => Assert.Equal(
                CosmosStrings.IndexesExist(nameof(OneToManyOwnedWithField), "OneToManyOwnerId"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_configure_one_to_many_owned_type_with_fields).Message);

        public override void Can_configure_one_to_one_owned_type_with_fields()
            => Assert.Equal(
                CosmosStrings.IndexesExist(nameof(OneToOneOwnedWithField), "OneToOneOwnerId"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_configure_one_to_one_owned_type_with_fields).Message);

        public override void Can_configure_owned_type()
            => Assert.Equal(
                CosmosStrings.IndexesExist(nameof(CustomerDetails), "CustomerId"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_configure_owned_type).Message);

        public override void Can_configure_owned_type_collection()
            => Assert.Equal(
                CosmosStrings.IndexesExist(nameof(Order), "CustomerId"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_configure_owned_type_collection).Message);

        public override void Can_configure_owned_type_collection_using_nested_closure()
            => Assert.Equal(
                CosmosStrings.IndexesExist(nameof(Order), "AnotherCustomerId"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_configure_owned_type_collection_using_nested_closure).Message);

        public override void Can_configure_chained_ownerships()
            => Assert.Equal(
                CosmosStrings.IndexesExist("Book.Label#BookLabel.AnotherBookLabel#AnotherBookLabel.SpecialBookLabel#SpecialBookLabel", "BookId"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_configure_chained_ownerships).Message);

        public override void Shared_type_entity_types_with_FK_to_another_entity_works()
            => Assert.Equal(
                CosmosStrings.IndexesExist("BillingOwner.Bill1#BillingDetail", "Country"),
                Assert.Throws<InvalidOperationException>(
                    base.Shared_type_entity_types_with_FK_to_another_entity_works).Message);

        [ConditionalFact]
        public virtual void Reference_type_is_discovered_as_owned()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<OneToOneOwnerWithField>(
                e =>
                {
                    e.Property(p => p.Id);
                    e.Property(p => p.AlternateKey);
                    e.Property(p => p.Description);
                    e.HasKey(p => p.Id);
                });

            var model = modelBuilder.FinalizeModel();

            var owner = model.FindEntityType(typeof(OneToOneOwnerWithField))!;
            Assert.Equal(typeof(OneToOneOwnerWithField).FullName, owner.Name);
            var ownership = owner.FindNavigation(nameof(OneToOneOwnerWithField.OwnedDependent))!.ForeignKey;
            Assert.True(ownership.IsOwnership);
            Assert.Equal(nameof(OneToOneOwnerWithField.OwnedDependent), ownership.PrincipalToDependent!.Name);
            Assert.Equal(nameof(OneToOneOwnedWithField.OneToOneOwner), ownership.DependentToPrincipal!.Name);
            Assert.Equal(nameof(OneToOneOwnerWithField.Id), ownership.PrincipalKey.Properties.Single().Name);
            var owned = ownership.DeclaringEntityType;
            Assert.Single(owned.GetForeignKeys());
            Assert.NotNull(model.FindEntityType(typeof(OneToOneOwnedWithField)));
            Assert.Equal(1, model.GetEntityTypes().Count(e => e.ClrType == typeof(OneToOneOwnedWithField)));
        }

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class CosmosModelBuilderFixture : ModelBuilderFixtureBase
    {
        public override TestHelpers TestHelpers => CosmosTestHelpers.Instance;
        public override bool ForeignKeysHaveIndexes
            => false;
    }
}
