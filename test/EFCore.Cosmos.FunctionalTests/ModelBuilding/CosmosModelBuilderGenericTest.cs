// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Xunit.Sdk;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class CosmosModelBuilderGenericTest : ModelBuilderTest
{
    public class CosmosGenericNonRelationship(CosmosModelBuilderFixture fixture) : NonRelationshipTestBase(fixture), IClassFixture<CosmosModelBuilderFixture>
    {
        public override void Can_set_composite_key_for_primitive_collection_on_an_entity_with_fields()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(EntityWithFields), "CollectionCompanyId"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_set_composite_key_for_primitive_collection_on_an_entity_with_fields).Message);

        public override void Can_set_alternate_key_for_primitive_collection_on_an_entity_with_fields()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(EntityWithFields), "CollectionCompanyId"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_set_alternate_key_for_primitive_collection_on_an_entity_with_fields).Message);

        public override void Can_call_PrimitiveCollection_on_an_entity_with_fields()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(EntityWithFields), "CollectionId"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_call_PrimitiveCollection_on_an_entity_with_fields).Message);

        public override void Access_mode_can_be_overridden_at_entity_and_primitive_collection_levels()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Down"),
                Assert.Throws<InvalidOperationException>(
                    base.Access_mode_can_be_overridden_at_entity_and_primitive_collection_levels).Message);

        public override void Can_set_custom_value_generator_for_primitive_collections()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Bottom"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_set_custom_value_generator_for_primitive_collections).Message);

        public override void Can_set_element_type_annotation()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(Customer), "Notes"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_set_element_type_annotation).Message);

        public override void Can_set_max_length_for_primitive_collections()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Bottom"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_set_max_length_for_primitive_collections).Message);

        public override void Can_set_primitive_collection_annotation()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(Customer), "Notes"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_set_primitive_collection_annotation).Message);

        public override void Can_set_primitive_collection_annotation_by_type()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(Customer), "Notes"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_set_primitive_collection_annotation_by_type).Message);

        public override void Can_set_primitive_collection_annotation_when_no_clr_property()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(Customer), "Notes"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_set_primitive_collection_annotation_when_no_clr_property).Message);

        public override void Can_set_sentinel_for_primitive_collections()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Bottom"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_set_sentinel_for_primitive_collections).Message);

        public override void Can_set_unicode_for_primitive_collections()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Bottom"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_set_unicode_for_primitive_collections).Message);

        public override void Element_types_are_nullable_by_default_if_the_type_is_nullable()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Element_types_are_nullable_by_default_if_the_type_is_nullable).Message);

        public override void Element_types_can_be_made_required()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Element_types_can_be_made_required).Message);

        public override void Element_types_can_have_custom_type_value_converter_type_set()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Element_types_can_have_custom_type_value_converter_type_set).Message);

        public override void Element_types_can_have_max_length()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Element_types_can_have_max_length).Message);

        public override void Element_types_can_have_non_generic_value_converter_set()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Element_types_can_have_non_generic_value_converter_set).Message);

        public override void Element_types_can_have_precision_and_scale()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Element_types_can_have_precision_and_scale).Message);

        public override void Element_types_can_have_provider_type_set()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Element_types_can_have_provider_type_set).Message);

        public override void Element_types_can_have_unicode_set()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Element_types_can_have_unicode_set).Message);

        public override void Element_types_have_default_precision_and_scale()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Element_types_have_default_precision_and_scale).Message);

        public override void Element_types_have_default_unicode()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Element_types_have_default_unicode).Message);

        public override void Element_types_have_no_max_length_by_default()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Element_types_have_no_max_length_by_default).Message);

        public override void Primitive_collections_are_required_by_default_only_if_CLR_type_is_nullable()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Primitive_collections_are_required_by_default_only_if_CLR_type_is_nullable).Message);

        public override void Primitive_collections_can_be_made_optional()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Primitive_collections_can_be_made_optional).Message);

        public override void Primitive_collections_can_be_made_required()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Primitive_collections_can_be_made_required).Message);

        public override void Primitive_collections_can_be_set_to_generate_values_on_Add()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Bottom"),
                Assert.Throws<InvalidOperationException>(
                    base.Primitive_collections_can_be_set_to_generate_values_on_Add).Message);

        public override void Primitive_collections_can_have_access_mode_set()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Primitive_collections_can_have_access_mode_set).Message);

        public override void Primitive_collections_can_have_field_set()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Down"),
                Assert.Throws<InvalidOperationException>(
                    base.Primitive_collections_can_have_field_set).Message);

        public override void Primitive_collections_can_have_value_converter_set()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Primitive_collections_can_have_value_converter_set).Message);

        public override void Primitive_collections_specified_by_string_are_shadow_properties_unless_already_known_to_be_CLR_properties()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Primitive_collections_specified_by_string_are_shadow_properties_unless_already_known_to_be_CLR_properties).Message);

        public override void Value_converter_type_on_primitive_collection_is_checked()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Up"),
                Assert.Throws<InvalidOperationException>(
                    base.Value_converter_type_on_primitive_collection_is_checked).Message);

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

        public override void Properties_can_have_provider_type_set_for_type()
        {
            var modelBuilder = CreateModelBuilder(c => c.Properties<string>().HaveConversion<byte[]>());

            modelBuilder.Entity<Quarks>(
                b =>
                {
                    b.Property(e => e.Up);
                    b.Property(e => e.Down);
                    b.Property<int>("Charm");
                    b.Property<string>("Strange");
                    b.Property<string>("__id").HasConversion(null);
                });

            var model = modelBuilder.FinalizeModel();
            var entityType = (IReadOnlyEntityType)model.FindEntityType(typeof(Quarks))!;

            Assert.Null(entityType.FindProperty("Up")!.GetProviderClrType());
            Assert.Same(typeof(byte[]), entityType.FindProperty("Down")!.GetProviderClrType());
            Assert.Null(entityType.FindProperty("Charm")!.GetProviderClrType());
            Assert.Same(typeof(byte[]), entityType.FindProperty("Strange")!.GetProviderClrType());
        }

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
            Assert.Equal(
                new[] { StoreKeyConvention.DefaultIdPropertyName, nameof(Customer.AlternateKey) },
                entity.GetKeys().First(k => k != entity.FindPrimaryKey()).Properties.Select(p => p.Name));

            var idProperty = entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName)!;
            Assert.Single(idProperty.GetContainingKeys());
            Assert.NotNull(idProperty.GetValueGeneratorFactory());
        }

        [ConditionalFact]
        public virtual void Partition_key_is_added_to_the_alternate_key_if_primary_key_contains_id()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().HasKey(StoreKeyConvention.DefaultIdPropertyName);
            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders)
                .HasPartitionKey(b => b.AlternateKey)
                .Property(b => b.AlternateKey).HasConversion<string>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(
                new[] { StoreKeyConvention.DefaultIdPropertyName },
                entity.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.Equal(
                new[] { StoreKeyConvention.DefaultIdPropertyName, nameof(Customer.AlternateKey) },
                entity.GetKeys().First(k => k != entity.FindPrimaryKey()).Properties.Select(p => p.Name));
        }

        [ConditionalFact]
        public virtual void No_id_property_created_if_another_property_mapped_to_id()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>()
                .Property(c => c.Name)
                .ToJsonProperty(StoreKeyConvention.IdPropertyJsonName);
            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders);

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Null(entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName));
            Assert.Single(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));

            var idProperty = entity.GetDeclaredProperties()
                .Single(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName);
            Assert.Single(idProperty.GetContainingKeys());
            Assert.NotNull(idProperty.GetValueGeneratorFactory());
        }

        [ConditionalFact]
        public virtual void No_id_property_created_if_another_property_mapped_to_id_in_pk()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>()
                .Property(c => c.Name)
                .ToJsonProperty(StoreKeyConvention.IdPropertyJsonName);
            modelBuilder.Entity<Customer>()
                .Ignore(c => c.Details)
                .Ignore(c => c.Orders)
                .HasKey(c => c.Name);

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Null(entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName));
            Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));

            var idProperty = entity.GetDeclaredProperties()
                .Single(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName);
            Assert.Single(idProperty.GetContainingKeys());
            Assert.Null(idProperty.GetValueGeneratorFactory());
        }

        [ConditionalFact]
        public virtual void No_alternate_key_is_created_if_primary_key_contains_id()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().HasKey(StoreKeyConvention.DefaultIdPropertyName);
            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders);

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(
                new[] { StoreKeyConvention.DefaultIdPropertyName },
                entity.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));

            var idProperty = entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName)!;
            Assert.Single(idProperty.GetContainingKeys());
            Assert.Null(idProperty.GetValueGeneratorFactory());
        }

        [ConditionalFact]
        public virtual void No_alternate_key_is_created_if_primary_key_contains_id_and_partition_key()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().HasKey(nameof(Customer.AlternateKey), StoreKeyConvention.DefaultIdPropertyName);
            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders)
                .HasPartitionKey(b => b.AlternateKey)
                .Property(b => b.AlternateKey).HasConversion<string>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(
                new[] { nameof(Customer.AlternateKey), StoreKeyConvention.DefaultIdPropertyName },
                entity.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));
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
            Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));
        }

        public override void Primitive_collections_can_be_made_concurrency_tokens()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Primitive_collections_can_be_made_concurrency_tokens).Message);

        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new GenericTestModelBuilder(Fixture, configure);
    }

    public class CosmosGenericComplexType(CosmosModelBuilderFixture fixture) : ComplexTypeTestBase(fixture), IClassFixture<CosmosModelBuilderFixture>
    {
        public override void Access_mode_can_be_overridden_at_entity_and_property_levels()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Down"),
                Assert.Throws<InvalidOperationException>(
                    base.Access_mode_can_be_overridden_at_entity_and_property_levels).Message);

        public override void Can_add_shadow_primitive_collections_when_they_have_been_ignored()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(Customer), "Shadow"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_add_shadow_primitive_collections_when_they_have_been_ignored).Message);

        public override void Can_call_PrimitiveCollection_on_a_field()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(EntityWithFields), "CollectionId"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_call_PrimitiveCollection_on_a_field).Message);

        public override void Can_set_custom_value_generator_for_primitive_collections()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Bottom"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_set_custom_value_generator_for_primitive_collections).Message);

        public override void Can_set_max_length_for_primitive_collections()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Bottom"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_set_max_length_for_primitive_collections).Message);

        public override void Can_set_primitive_collection_annotation_when_no_clr_property()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(Customer), "Ints"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_set_primitive_collection_annotation_when_no_clr_property).Message);

        public override void Can_set_sentinel_for_primitive_collections()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Bottom"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_set_sentinel_for_primitive_collections).Message);

        public override void Can_set_unicode_for_primitive_collections()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Bottom"),
                Assert.Throws<InvalidOperationException>(
                    base.Can_set_unicode_for_primitive_collections).Message);

        public override void Primitive_collections_are_required_by_default_only_if_CLR_type_is_nullable()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Primitive_collections_are_required_by_default_only_if_CLR_type_is_nullable).Message);

        public override void Primitive_collections_can_be_made_concurrency_tokens()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Primitive_collections_can_be_made_concurrency_tokens).Message);

        public override void Primitive_collections_can_be_made_optional()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Primitive_collections_can_be_made_optional).Message);

        public override void Primitive_collections_can_be_made_required()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Primitive_collections_can_be_made_required).Message);

        public override void Primitive_collections_can_be_set_to_generate_values_on_Add()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Bottom"),
                Assert.Throws<InvalidOperationException>(
                    base.Primitive_collections_can_be_set_to_generate_values_on_Add).Message);

        public override void Primitive_collections_can_have_field_set()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Down"),
                Assert.Throws<InvalidOperationException>(
                    base.Primitive_collections_can_have_field_set).Message);

        public override void Primitive_collections_specified_by_string_are_shadow_properties_unless_already_known_to_be_CLR_properties()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Charm"),
                Assert.Throws<InvalidOperationException>(
                    base.Primitive_collections_specified_by_string_are_shadow_properties_unless_already_known_to_be_CLR_properties).Message);

        public override void Properties_can_have_access_mode_set()
            => Assert.Equal(
                CosmosStrings.PrimitiveCollectionsNotSupported(nameof(CollectionQuarks), "Down"),
                Assert.Throws<InvalidOperationException>(
                    base.Properties_can_have_access_mode_set).Message);

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
      Notes (List<string>)", complexProperty.ToDebugString(), ignoreLineEndingDifferences: true);
        }

        public override void Properties_can_have_provider_type_set_for_type()
        {
            var modelBuilder = CreateModelBuilder(c => c.Properties<string>().HaveConversion<byte[]>());

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>(
                    b =>
                    {
                        b.Property<string>("__id").HasConversion(null);
                        b.ComplexProperty(
                            e => e.Quarks,
                            b =>
                            {
                                b.Property(e => e.Up);
                                b.Property(e => e.Down);
                                b.Property<int>("Charm");
                                b.Property<string>("Strange");
                            });
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties))!.GetComplexProperties().Single().ComplexType;

            Assert.Null(complexType.FindProperty("Up")!.GetProviderClrType());
            Assert.Same(typeof(byte[]), complexType.FindProperty("Down")!.GetProviderClrType());
            Assert.Null(complexType.FindProperty("Charm")!.GetProviderClrType());
            Assert.Same(typeof(byte[]), complexType.FindProperty("Strange")!.GetProviderClrType());
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
            Assert.Equal(
                new[] { StoreKeyConvention.DefaultIdPropertyName, nameof(Customer.AlternateKey) },
                entity.GetKeys().First(k => k != entity.FindPrimaryKey()).Properties.Select(p => p.Name));

            var idProperty = entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName)!;
            Assert.Single(idProperty.GetContainingKeys());
            Assert.NotNull(idProperty.GetValueGeneratorFactory());
        }

        [ConditionalFact]
        public virtual void Partition_key_is_added_to_the_alternate_key_if_primary_key_contains_id()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().HasKey(StoreKeyConvention.DefaultIdPropertyName);
            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders)
                .HasPartitionKey(b => b.AlternateKey)
                .Property(b => b.AlternateKey).HasConversion<string>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(
                new[] { StoreKeyConvention.DefaultIdPropertyName },
                entity.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.Equal(
                new[] { StoreKeyConvention.DefaultIdPropertyName, nameof(Customer.AlternateKey) },
                entity.GetKeys().First(k => k != entity.FindPrimaryKey()).Properties.Select(p => p.Name));
        }

        [ConditionalFact]
        public virtual void No_id_property_created_if_another_property_mapped_to_id()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>()
                .Property(c => c.Name)
                .ToJsonProperty(StoreKeyConvention.IdPropertyJsonName);
            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders);

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Null(entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName));
            Assert.Single(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));

            var idProperty = entity.GetDeclaredProperties()
                .Single(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName);
            Assert.Single(idProperty.GetContainingKeys());
            Assert.NotNull(idProperty.GetValueGeneratorFactory());
        }

        [ConditionalFact]
        public virtual void No_id_property_created_if_another_property_mapped_to_id_in_pk()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>()
                .Property(c => c.Name)
                .ToJsonProperty(StoreKeyConvention.IdPropertyJsonName);
            modelBuilder.Entity<Customer>()
                .Ignore(c => c.Details)
                .Ignore(c => c.Orders)
                .HasKey(c => c.Name);

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Null(entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName));
            Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));

            var idProperty = entity.GetDeclaredProperties()
                .Single(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName);
            Assert.Single(idProperty.GetContainingKeys());
            Assert.Null(idProperty.GetValueGeneratorFactory());
        }

        [ConditionalFact]
        public virtual void No_alternate_key_is_created_if_primary_key_contains_id()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().HasKey(StoreKeyConvention.DefaultIdPropertyName);
            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders);

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(
                new[] { StoreKeyConvention.DefaultIdPropertyName },
                entity.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));

            var idProperty = entity.FindProperty(StoreKeyConvention.DefaultIdPropertyName)!;
            Assert.Single(idProperty.GetContainingKeys());
            Assert.Null(idProperty.GetValueGeneratorFactory());
        }

        [ConditionalFact]
        public virtual void No_alternate_key_is_created_if_primary_key_contains_id_and_partition_key()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.Entity<Customer>().HasKey(nameof(Customer.AlternateKey), StoreKeyConvention.DefaultIdPropertyName);
            modelBuilder.Entity<Customer>()
                .Ignore(b => b.Details)
                .Ignore(b => b.Orders)
                .HasPartitionKey(b => b.AlternateKey)
                .Property(b => b.AlternateKey).HasConversion<string>();

            var model = modelBuilder.FinalizeModel();

            var entity = model.FindEntityType(typeof(Customer))!;

            Assert.Equal(
                new[] { nameof(Customer.AlternateKey), StoreKeyConvention.DefaultIdPropertyName },
                entity.FindPrimaryKey()!.Properties.Select(p => p.Name));
            Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));
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
            Assert.Empty(entity.GetKeys().Where(k => k != entity.FindPrimaryKey()));
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
            Assert.Equal("PartitionId", joinType.GetPartitionKeyPropertyName());
            Assert.Equal("PartitionId", joinType.FindPrimaryKey()!.Properties.Last().Name);
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
            Assert.Equal("PartitionId", joinType.GetPartitionKeyPropertyName());
            Assert.Equal("PartitionId", joinType.FindPrimaryKey()!.Properties.Last().Name);
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
            Assert.Equal("Partition2Id", joinType.GetPartitionKeyPropertyName());
            Assert.Equal("Partition2Id", joinType.FindPrimaryKey()!.Properties.Last().Name);
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
    }
}
