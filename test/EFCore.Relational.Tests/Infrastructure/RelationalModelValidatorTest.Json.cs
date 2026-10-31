// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

public partial class RelationalModelValidatorTest
{
    [Theory, InlineData(false), InlineData(true)]
    public void Accepts_reader_writer_for_concrete_JSON_collection_type(bool genericBuilder)
    {
        var builder = CreateConventionModelBuilder();
        var owned = builder.Entity<JsonReaderWriterOwner>().OwnsOne(e => e.Value);
        owned.ToJson();
        var collection = owned.PrimitiveCollection<List<int>>("Numbers");
        if (genericBuilder)
        {
            collection.HasJsonValueReaderWriter<IntListReaderWriter>();
        }
        else
        {
            ((PrimitiveCollectionBuilder)collection).HasJsonValueReaderWriter(typeof(IntListReaderWriter));
        }

        var model = Validate(builder);
        var property = model.FindEntityType(typeof(JsonReaderWriterOwner))!.FindNavigation(nameof(JsonReaderWriterOwner.Value))!
            .TargetEntityType.FindProperty("Numbers")!;
        var readerWriter = property.GetTypeMapping().JsonValueReaderWriter!;
        Assert.Equal("[1,2]", readerWriter.ToJsonString(new List<int> { 1, 2 }));
        Assert.Equal(new List<int> { 1, 2 }, Assert.IsType<List<int>>(readerWriter.FromJsonString("[1,2]")));
    }

    [Theory, InlineData(false), InlineData(true)]
    public void Accepts_reader_writer_for_primitive_collection_on_complex_type(bool genericBuilder)
    {
        var builder = CreateConventionModelBuilder();
        var collection = builder.Entity<JsonComplexCollectionReaderWriterOwner>().ComplexProperty(e => e.Value)
            .PrimitiveCollection(e => e.Numbers);
        if (genericBuilder)
        {
            collection.HasJsonValueReaderWriter<IntListReaderWriter>();
        }
        else
        {
            ((ComplexTypePrimitiveCollectionBuilder)collection).HasJsonValueReaderWriter(typeof(IntListReaderWriter));
        }

        var model = Validate(builder);
        var property = model.FindEntityType(typeof(JsonComplexCollectionReaderWriterOwner))!
            .FindComplexProperty(nameof(JsonComplexCollectionReaderWriterOwner.Value))!
            .ComplexType.FindProperty(nameof(JsonIntListValue.Numbers))!;
        var readerWriter = property.GetTypeMapping().JsonValueReaderWriter!;
        Assert.Same(IntListReaderWriter.Instance, readerWriter);
        Assert.Equal("[1,2]", readerWriter.ToJsonString(new List<int> { 1, 2 }));
        Assert.Equal(new List<int> { 1, 2 }, Assert.IsType<List<int>>(readerWriter.FromJsonString("[1,2]")));
    }

    public sealed class IntListReaderWriter : JsonValueReaderWriter<List<int>>
    {
        private static readonly JsonCollectionOfStructsReaderWriter<List<int>, int> ReaderWriter = new(JsonInt32ReaderWriter.Instance);

        public static IntListReaderWriter Instance { get; } = new();

        public override List<int> FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => (List<int>)ReaderWriter.FromJsonTyped(ref manager, existingObject);

        public override void ToJsonTyped(Utf8JsonWriter writer, List<int> value)
            => ReaderWriter.ToJsonTyped(writer, value);

        public override Expression ConstructorExpression
            => Expression.Property(null, typeof(IntListReaderWriter), nameof(Instance));
    }

    [Theory, InlineData(false), InlineData(true)]
    public void Configured_enum_reader_writer_overrides_legacy_convention(bool typeMappingDefault)
    {
        var builder = CreateConventionModelBuilder(
            c =>
            {
                if (typeMappingDefault)
                {
                    c.DefaultTypeMapping<DayOfWeek>()
                        .HasJsonValueReaderWriter<Storage.RelationalTypeMappingSourceTestBase.JsonInt32AsStringReaderWriter>();
                }
            });
        var owned = builder.Entity<JsonReaderWriterOwner>().OwnsOne(e => e.Value);
        owned.ToJson();
        var propertyBuilder = owned.Property<DayOfWeek>("Day");
        if (!typeMappingDefault)
        {
            propertyBuilder
                .HasJsonValueReaderWriter<Storage.RelationalTypeMappingSourceTestBase.JsonInt32AsStringReaderWriter>();
        }

        var model = Validate(builder);
        var property = model.FindEntityType(typeof(JsonReaderWriterOwner))!.FindNavigation(nameof(JsonReaderWriterOwner.Value))!
            .TargetEntityType.FindProperty("Day")!;
        var readerWriter = property.GetTypeMapping().JsonValueReaderWriter!;
        Assert.Equal("\"1\"", readerWriter.ToJsonString(DayOfWeek.Monday));
        Assert.Equal(DayOfWeek.Monday, readerWriter.FromJsonString("\"1\""));
    }

    [Fact]
    public void Member_reader_writer_overrides_type_mapping_default()
    {
        var builder = CreateConventionModelBuilder(
            c => c.DefaultTypeMapping<int>().HasJsonValueReaderWriter<JsonInt32ReaderWriter>());
        var owned = builder.Entity<JsonReaderWriterOwner>().OwnsOne(e => e.Value);
        owned.ToJson();
        owned.Property(e => e.Number)
            .HasJsonValueReaderWriter<Storage.RelationalTypeMappingSourceTestBase.JsonInt32AsStringReaderWriter>();

        var model = Validate(builder);
        var property = model.FindEntityType(typeof(JsonReaderWriterOwner))!.FindNavigation(nameof(JsonReaderWriterOwner.Value))!
            .TargetEntityType.FindProperty(nameof(JsonReaderWriterValue.Number))!;
        Assert.Equal("\"42\"", property.GetTypeMapping().JsonValueReaderWriter!.ToJsonString(42));
        Assert.Equal(42, property.GetTypeMapping().JsonValueReaderWriter!.FromJsonString("\"42\""));
    }

    [Fact]
    public void Accepts_reader_writers_on_JSON_complex_type_and_collection_members()
    {
        var builder = CreateConventionModelBuilder();
        var entity = builder.Entity<JsonComplexReaderWriterOwner>();
        entity.ComplexProperty(e => e.Value).ToJson().Property(e => e.Number)
            .HasConversion<string>().HasJsonValueReaderWriter<JsonStringReaderWriter>();
        entity.ComplexCollection(e => e.Values).ToJson().Property(e => e.Number)
            .HasConversion<string>().HasJsonValueReaderWriter<JsonStringReaderWriter>();

        var model = Validate(builder);
        var entityType = model.FindEntityType(typeof(JsonComplexReaderWriterOwner))!;
        foreach (var complexProperty in entityType.GetComplexProperties())
        {
            var property = complexProperty.ComplexType.FindProperty(nameof(JsonReaderWriterValue.Number))!;
            var readerWriter = property.GetTypeMapping().JsonValueReaderWriter!;
            Assert.Equal("\"42\"", readerWriter.ToJsonString(42));
            Assert.Equal(42, readerWriter.FromJsonString("\"42\""));
        }
    }

    [Fact]
    public void Accepts_reader_writer_on_JSON_collection_element_with_conversion()
    {
        var builder = CreateConventionModelBuilder();
        builder.Entity<JsonCollectionReaderWriterOwner>().PrimitiveCollection(e => e.Numbers).ElementType()
            .HasConversion<string>().HasJsonValueReaderWriter<JsonStringReaderWriter>();

        var model = Validate(builder);
        var property = model.FindEntityType(typeof(JsonCollectionReaderWriterOwner))!
            .FindProperty(nameof(JsonCollectionReaderWriterOwner.Numbers))!;
        var readerWriter = property.GetTypeMapping().JsonValueReaderWriter!;
        Assert.Equal("[\"42\",null]", readerWriter.ToJsonString(new List<int?> { 42, null }));
        Assert.Equal(new List<int?> { 42, null }, Assert.IsAssignableFrom<IEnumerable<int?>>(readerWriter.FromJsonString("[\"42\",null]")));
    }

    [Fact]
    public void Rejects_reader_writer_with_incompatible_JSON_collection_element_store_type()
    {
        var builder = CreateConventionModelBuilder();
        var property = builder.Entity<JsonCollectionReaderWriterOwner>().PrimitiveCollection(e => e.Numbers);
        property.ElementType().HasJsonValueReaderWriter<JsonStringReaderWriter>();

        VerifyError(
            CoreStrings.JsonValueReaderWriterPropertyMismatch(
                property.Metadata.DeclaringType.DisplayName(), property.Metadata.Name + "[]", nameof(JsonStringReaderWriter),
                "string", "int"),
            builder);
    }

    [Fact]
    public void Rejects_reader_writer_on_non_JSON_member()
    {
        var builder = CreateConventionModelBuilder();
        var property = builder.Entity<JsonReaderWriterOwner>().Property(e => e.Id)
            .HasJsonValueReaderWriter<JsonInt32ReaderWriter>().Metadata;
        builder.Entity<JsonReaderWriterOwner>().Ignore(e => e.Value);

        VerifyError(
            CoreStrings.JsonValueReaderWriterOnNonJsonProperty(
                property.DeclaringType.DisplayName(), property.Name, nameof(JsonInt32ReaderWriter)),
            builder);
    }

    [Fact]
    public void Rejects_reader_writer_on_non_JSON_member_with_string_conversion()
    {
        var builder = CreateConventionModelBuilder();
        var property = builder.Entity<JsonReaderWriterOwner>().Property(e => e.Id)
            .HasConversion<string>()
            .HasJsonValueReaderWriter<JsonStringReaderWriter>().Metadata;
        builder.Entity<JsonReaderWriterOwner>().Ignore(e => e.Value);

        VerifyError(
            CoreStrings.JsonValueReaderWriterOnNonJsonProperty(
                property.DeclaringType.DisplayName(), property.Name, nameof(JsonStringReaderWriter)),
            builder);
    }

    [Theory, InlineData(false), InlineData(true)]
    public void Accepts_reader_writer_for_converted_JSON_member_regardless_of_configuration_order(bool configureReaderWriterFirst)
    {
        var builder = CreateConventionModelBuilder();
        var propertyBuilder = builder.Entity<JsonReaderWriterOwner>().OwnsOne(e => e.Value).Property(e => e.Number);
        if (configureReaderWriterFirst)
        {
            propertyBuilder.HasJsonValueReaderWriter<JsonStringReaderWriter>();
        }

        propertyBuilder.HasConversion<string>();
        builder.Entity<JsonReaderWriterOwner>().OwnsOne(e => e.Value).ToJson();
        if (!configureReaderWriterFirst)
        {
            propertyBuilder.HasJsonValueReaderWriter<JsonStringReaderWriter>();
        }

        var model = Validate(builder);
        var property = model.FindEntityType(typeof(JsonReaderWriterOwner))!.FindNavigation(nameof(JsonReaderWriterOwner.Value))!
            .TargetEntityType.FindProperty(nameof(JsonReaderWriterValue.Number))!;
        var readerWriter = property.GetTypeMapping().JsonValueReaderWriter!;
        Assert.Equal("\"42\"", readerWriter.ToJsonString(42));
        Assert.Equal(42, readerWriter.FromJsonString("\"42\""));
    }

    [Theory, InlineData(false), InlineData(true)]
    public void Rejects_reader_writer_for_model_type_instead_of_JSON_store_type(bool configureReaderWriterFirst)
    {
        var builder = CreateConventionModelBuilder();
        var property = builder.Entity<JsonReaderWriterOwner>().OwnsOne(e => e.Value).Property(e => e.Number);
        if (configureReaderWriterFirst)
        {
            property.HasJsonValueReaderWriter<JsonInt32ReaderWriter>();
        }

        property.HasConversion<string>();
        builder.Entity<JsonReaderWriterOwner>().OwnsOne(e => e.Value).ToJson();
        if (!configureReaderWriterFirst)
        {
            property.HasJsonValueReaderWriter<JsonInt32ReaderWriter>();
        }

        VerifyError(
            CoreStrings.JsonValueReaderWriterPropertyMismatch(
                property.Metadata.DeclaringType.DisplayName(), property.Metadata.Name, nameof(JsonInt32ReaderWriter), "int", "string"),
            builder);
    }

    [Fact]
    public void Does_not_validate_type_mapping_default_as_member_configuration()
    {
        var builder = CreateConventionModelBuilder(
            c => c.DefaultTypeMapping<int>().HasJsonValueReaderWriter<JsonInt32ReaderWriter>());
        builder.Entity<JsonReaderWriterOwner>().Ignore(e => e.Value);

        var model = Validate(builder);
        var property = model.FindEntityType(typeof(JsonReaderWriterOwner))!.FindProperty(nameof(JsonReaderWriterOwner.Id))!;
        Assert.Null(property.GetJsonValueReaderWriter());
        Assert.Same(JsonInt32ReaderWriter.Instance, property.GetTypeMapping().JsonValueReaderWriter);
    }

    [Fact]
    public void Clearing_member_reader_writer_restores_default_without_JSON_location_error()
    {
        var builder = CreateConventionModelBuilder(
            c => c.DefaultTypeMapping<int>().HasJsonValueReaderWriter<JsonInt32ReaderWriter>());
        builder.Entity<JsonReaderWriterOwner>().Ignore(e => e.Value);
        builder.Entity<JsonReaderWriterOwner>().Property(e => e.Id)
            .HasJsonValueReaderWriter<JsonStringReaderWriter>()
            .HasJsonValueReaderWriter(null);

        var model = Validate(builder);
        Assert.Same(
            JsonInt32ReaderWriter.Instance,
            model.FindEntityType(typeof(JsonReaderWriterOwner))!.FindProperty(nameof(JsonReaderWriterOwner.Id))!
                .GetTypeMapping().JsonValueReaderWriter);
    }

    private class JsonReaderWriterOwner
    {
        public int Id { get; set; }
        public JsonReaderWriterValue Value { get; set; } = null!;
    }

    private class JsonReaderWriterValue
    {
        public int? Number { get; set; }
    }

    private class JsonComplexReaderWriterOwner
    {
        public int Id { get; set; }
        public JsonReaderWriterValue Value { get; set; } = null!;
        public List<JsonReaderWriterValue> Values { get; set; } = [];
    }

    private class JsonCollectionReaderWriterOwner
    {
        public int Id { get; set; }
        public List<int?> Numbers { get; set; } = [];
    }

    private class JsonComplexCollectionReaderWriterOwner
    {
        public int Id { get; set; }
        public JsonIntListValue Value { get; set; } = null!;
    }

    private class JsonIntListValue
    {
        public List<int> Numbers { get; set; } = [];
    }

    [Fact]
    public void Throw_when_non_json_entity_has_column_type_set()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.OwnsOne(
                x => x.OwnedReference, bb =>
                {
                    bb.HasColumnType("nvarchar(2000)");
                    bb.Ignore(x => x.NestedCollection);
                    bb.Ignore(x => x.NestedReference);
                });
            b.Ignore(x => x.OwnedCollection);
        });

        VerifyError(
            RelationalStrings.ContainerTypeOnNonContainer(nameof(ValidatorJsonOwnedRoot)),
            modelBuilder);
    }

    [Fact]
    public void Throw_when_non_root_json_entity_has_column_type_set()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.OwnsOne(
                x => x.OwnedReference, bb =>
                {
                    bb.ToJson();
                    bb.Ignore(x => x.NestedCollection);
                    bb.OwnsOne(x => x.NestedReference, bbb => bbb.HasColumnType("nvarchar(2000)"));
                });
            b.Ignore(x => x.OwnedCollection);
        });

        VerifyError(
            RelationalStrings.ContainerTypeOnNestedOwnedEntityType(nameof(ValidatorJsonOwnedBranch)),
            modelBuilder);
    }

    [Fact]
    public void Throw_when_non_json_entity_is_the_owner_of_json_entity_ref_ref()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.OwnsOne(
                x => x.OwnedReference, bb =>
                {
                    bb.Ignore(x => x.NestedCollection);
                    bb.OwnsOne(x => x.NestedReference, bbb => bbb.ToJson("reference_reference"));
                });
            b.Ignore(x => x.OwnedCollection);
        });

        VerifyError(
            RelationalStrings.JsonEntityOwnedByNonJsonOwnedType(
                nameof(ValidatorJsonOwnedRoot), nameof(ValidatorJsonEntityBasic)),
            modelBuilder);
    }

    [Fact]
    public void Throw_when_non_json_entity_is_the_owner_of_json_entity_ref_col()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.OwnsOne(
                x => x.OwnedReference, bb =>
                {
                    bb.OwnsMany(x => x.NestedCollection, bbb => bbb.ToJson("reference_collection"));
                    bb.Ignore(x => x.NestedReference);
                });
            b.Ignore(x => x.OwnedCollection);
        });

        VerifyError(
            RelationalStrings.JsonEntityOwnedByNonJsonOwnedType(
                nameof(ValidatorJsonOwnedRoot), nameof(ValidatorJsonEntityBasic)),
            modelBuilder);
    }

    [Fact]
    public void Throw_when_non_json_entity_is_the_owner_of_json_entity_col_ref()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.OwnsMany(
                x => x.OwnedCollection, bb =>
                {
                    bb.Ignore(x => x.NestedCollection);
                    bb.OwnsOne(x => x.NestedReference, bbb => bbb.ToJson("collection_reference"));
                });
            b.Ignore(x => x.OwnedReference);
        });

        VerifyError(
            RelationalStrings.JsonEntityOwnedByNonJsonOwnedType(
                nameof(ValidatorJsonOwnedRoot), nameof(ValidatorJsonOwnedRoot)),
            modelBuilder);
    }

    [Fact]
    public void Throw_when_non_json_entity_is_the_owner_of_json_entity_col_col()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.OwnsMany(
                x => x.OwnedCollection, bb =>
                {
                    bb.Ignore(x => x.NestedReference);
                    bb.OwnsMany(x => x.NestedCollection, bbb => bbb.ToJson("collection_collection"));
                });
            b.Ignore(x => x.OwnedReference);
        });

        VerifyError(
            RelationalStrings.JsonEntityOwnedByNonJsonOwnedType(
                nameof(ValidatorJsonOwnedRoot), nameof(ValidatorJsonOwnedRoot)),
            modelBuilder);
    }

    [Fact]
    public void Throw_when_json_entity_references_another_non_json_entity_via_reference()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityReferencedEntity>();
        modelBuilder.Entity<ValidatorJsonEntityJsonReferencingRegularEntity>(b => b.OwnsOne(
            x => x.Owned, bb =>
            {
                bb.ToJson("reference");
                bb.HasOne(x => x.Reference).WithOne().HasForeignKey<ValidatorJsonOwnedReferencingRegularEntity>(x => x.Fk);
            }));

        VerifyError(
            RelationalStrings.JsonEntityReferencingRegularEntity(nameof(ValidatorJsonOwnedReferencingRegularEntity)),
            modelBuilder);
    }

    [Fact]
    public void Throw_when_json_entity_is_the_owner_of_json_entity_with_different_column_name()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.OwnsOne(
                x => x.OwnedReference, bb =>
                {
                    bb.ToJson();
                    bb.Ignore(x => x.NestedCollection);
                    bb.OwnsOne(x => x.NestedReference, bbb => bbb.ToJson());
                });
            b.Ignore(x => x.OwnedCollection);
        });

        VerifyError(
            RelationalStrings.JsonEntityMappedToDifferentColumnThanOwner(
                nameof(ValidatorJsonOwnedBranch), nameof(ValidatorJsonOwnedRoot.NestedReference),
                nameof(ValidatorJsonOwnedRoot), nameof(ValidatorJsonEntityBasic.OwnedReference)),
            modelBuilder);
    }

    [Fact] // #32513
    public void Throw_when_json_collection_is_the_owner_of_json_collection_with_different_column_name()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.OwnsMany(
                x => x.OwnedCollection, bb =>
                {
                    bb.ToJson();
                    bb.Ignore(x => x.NestedReference);
                    bb.OwnsMany(x => x.NestedCollection).ToJson();
                });
            b.Ignore(x => x.OwnedReference);
        });

        VerifyError(
            RelationalStrings.JsonEntityMappedToDifferentColumnThanOwner(
                nameof(ValidatorJsonOwnedBranch), nameof(ValidatorJsonOwnedRoot.NestedCollection),
                nameof(ValidatorJsonOwnedRoot), nameof(ValidatorJsonEntityBasic.OwnedCollection)),
            modelBuilder);
    }

    [Fact]
    public void Tpt_not_supported_for_owner_of_json_entity_on_base()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>(b =>
        {
            b.ToTable("Table1");
            b.OwnsOne(
                x => x.ReferenceOnBase, bb => bb.ToJson("reference"));
        });

        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(b =>
        {
            b.HasBaseType<ValidatorJsonEntityInheritanceBase>();
            b.ToTable("Table2");
            b.Ignore(x => x.ReferenceOnDerived);
            b.Ignore(x => x.CollectionOnDerived);
        });

        VerifyError(
            RelationalStrings.JsonEntityWithNonTphInheritanceOnOwner(nameof(ValidatorJsonEntityInheritanceBase)),
            modelBuilder);
    }

    [Fact]
    public void Tpt_not_supported_for_owner_of_json_entity_on_derived()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>(b =>
        {
            b.ToTable("Table1");
            b.Ignore(x => x.ReferenceOnBase);
        });

        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(b =>
        {
            b.ToTable("Table2");
            b.OwnsOne(x => x.ReferenceOnDerived, bb => bb.ToJson("reference"));
            b.Ignore(x => x.CollectionOnDerived);
        });

        VerifyError(
            RelationalStrings.JsonEntityWithNonTphInheritanceOnOwner(nameof(ValidatorJsonEntityInheritanceBase)),
            modelBuilder);
    }

    [Fact]
    public void Tpt_not_supported_for_owner_of_json_entity_mapping_strategy_explicitly_defined()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>(b =>
        {
            b.UseTptMappingStrategy();
            b.OwnsOne(
                x => x.ReferenceOnBase, bb => bb.ToJson("reference"));
        });

        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(b =>
        {
            b.HasBaseType<ValidatorJsonEntityInheritanceBase>();
            b.Ignore(x => x.ReferenceOnDerived);
            b.Ignore(x => x.CollectionOnDerived);
        });

        VerifyError(
            RelationalStrings.JsonEntityWithNonTphInheritanceOnOwner(nameof(ValidatorJsonEntityInheritanceBase)),
            modelBuilder);
    }

    [Fact]
    public void Tpt_not_supported_for_owner_of_json_entity_same_table_names_different_schemas()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>(b =>
        {
            b.ToTable("Table", "mySchema1");
            b.Ignore(x => x.ReferenceOnBase);
        });

        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(b =>
        {
            b.ToTable("Table", "mySchema2");
            b.OwnsOne(x => x.ReferenceOnDerived, bb => bb.ToJson("reference"));
            b.Ignore(x => x.CollectionOnDerived);
        });

        VerifyError(
            RelationalStrings.JsonEntityWithNonTphInheritanceOnOwner(nameof(ValidatorJsonEntityInheritanceBase)),
            modelBuilder);
    }

    [Fact]
    public void Tpc_not_supported_for_owner_of_json_entity()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>().UseTpcMappingStrategy();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceAbstract>();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>(b => b.Ignore(x => x.ReferenceOnBase));

        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(b =>
        {
            b.OwnsOne(x => x.ReferenceOnDerived, bb => bb.ToJson("reference"));
            b.Ignore(x => x.CollectionOnDerived);
        });

        VerifyError(
            RelationalStrings.JsonEntityWithNonTphInheritanceOnOwner(nameof(ValidatorJsonEntityInheritanceBase)),
            modelBuilder);
    }

    [Fact]
    public void Json_entity_not_mapped_to_table_or_a_view_is_not_supported()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.ToTable((string?)null);
            b.OwnsOne(
                x => x.OwnedReference, bb =>
                {
                    bb.ToJson("reference");
                    bb.Ignore(x => x.NestedReference);
                    bb.Ignore(x => x.NestedCollection);
                });
            b.Ignore(x => x.OwnedCollection);
        });

        VerifyError(
            RelationalStrings.JsonEntityWithOwnerNotMappedToTableOrView(nameof(ValidatorJsonEntityBasic)),
            modelBuilder);
    }

    [Fact]
    public void Json_multiple_json_entities_mapped_to_the_same_column()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntitySideBySide>(b =>
        {
            b.OwnsOne(x => x.Reference1, bb => bb.ToJson("json"));
            b.OwnsOne(x => x.Reference2, bb => bb.ToJson("json"));
            b.Ignore(x => x.Collection1);
            b.Ignore(x => x.Collection2);
        });

        VerifyError(
            RelationalStrings.JsonEntityMultipleRootsMappedToTheSameJsonColumn(
                "json", nameof(ValidatorJsonEntitySideBySide)),
            modelBuilder);
    }

    [Fact]
    public void Json_entity_with_default_value_on_a_property()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.Ignore(x => x.OwnedCollection);
            b.OwnsOne(
                x => x.OwnedReference, bb =>
                {
                    bb.Ignore(x => x.NestedReference);
                    bb.Ignore(x => x.NestedCollection);
                    bb.ToJson("json");
                    bb.Property(x => x.Name).HasDefaultValue("myDefault");
                });
        });

        VerifyError(RelationalStrings.JsonEntityWithDefaultValueSetOnItsProperty("ValidatorJsonOwnedRoot", "Name"), modelBuilder);
    }

    [Fact]
    public void Json_entity_with_table_splitting_throws()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.ToTable("SharedTable");
            b.OwnsOne(
                x => x.OwnedReference, bb =>
                {
                    bb.ToJson("json");
                    bb.OwnsOne(x => x.NestedReference);
                    bb.OwnsMany(x => x.NestedCollection);
                });
            b.Ignore(x => x.OwnedCollection);
        });

        modelBuilder.Entity<ValidatorJsonEntityTableSplitting>(b =>
        {
            b.ToTable("SharedTable");
            b.HasOne(x => x.Link).WithOne().HasForeignKey<ValidatorJsonEntityBasic>(x => x.Id);
        });

        VerifyError(
            RelationalStrings.JsonEntityWithTableSplittingIsNotSupported,
            modelBuilder);
    }

    [Fact]
    public void Json_entity_with_explicit_ordinal_key_on_collection_throws()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityExplicitOrdinal>(b => b.OwnsMany(
            x => x.OwnedCollection, bb =>
            {
                bb.ToJson("json");
                bb.HasKey(x => x.Ordinal);
            }));

        VerifyError(RelationalStrings.JsonEntityWithExplicitlyConfiguredKey("ValidatorJsonOwnedExplicitOrdinal", "Ordinal"), modelBuilder);
    }

    [Fact]
    public void Json_entity_with_key_having_json_property_name_configured_explicitly_throws()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityExplicitOrdinal>(b => b.OwnsMany(
            x => x.OwnedCollection, bb =>
            {
                bb.ToJson("json");
                bb.HasKey(x => x.Ordinal);
                bb.Property(x => x.Ordinal).HasJsonPropertyName("Foo");
            }));

        VerifyError(
            RelationalStrings.JsonEntityWithExplicitlyConfiguredJsonPropertyNameOnKey(
                nameof(ValidatorJsonOwnedExplicitOrdinal.Ordinal), nameof(ValidatorJsonOwnedExplicitOrdinal)),
            modelBuilder);
    }

    [Fact]
    public void Json_entity_with_multiple_properties_mapped_to_same_json_name()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.OwnsOne(
                x => x.OwnedReference, bb =>
                {
                    bb.Property(x => x.Name).HasJsonPropertyName("Foo");
                    bb.Property(x => x.Number).HasJsonPropertyName("Foo");
                    bb.ToJson("reference");
                    bb.Ignore(x => x.NestedReference);
                    bb.Ignore(x => x.NestedCollection);
                });
            b.Ignore(x => x.OwnedCollection);
        });

        VerifyError(
            RelationalStrings.JsonObjectWithMultiplePropertiesMappedToSameJsonProperty(
                "Name",
                "Number",
                "ValidatorJsonOwnedRoot",
                "Foo"),
            modelBuilder);
    }

    [Fact]
    public void Json_entity_with_property_and_navigation_mapped_to_same_json_name()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.OwnsOne(
                x => x.OwnedReference, bb =>
                {
                    bb.Property(x => x.Name);
                    bb.Property(x => x.Number);
                    bb.ToJson("reference");
                    bb.OwnsOne(x => x.NestedReference, bbb => bbb.HasJsonPropertyName("Name"));
                    bb.Ignore(x => x.NestedCollection);
                });
            b.Ignore(x => x.OwnedCollection);
        });

        VerifyError(
            RelationalStrings.JsonObjectWithMultiplePropertiesMappedToSameJsonProperty(
                "Name",
                "NestedReference",
                "ValidatorJsonOwnedRoot",
                "Name"),
            modelBuilder);
    }

    [Fact]
    public void Json_on_base_and_derived_mapped_to_same_column_throws()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>().OwnsOne(x => x.ReferenceOnBase, b => b.ToJson("jsonColumn"));
        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(b =>
        {
            b.HasBaseType<ValidatorJsonEntityInheritanceBase>();
            b.OwnsOne(x => x.ReferenceOnDerived, bb => bb.ToJson("jsonColumn"));
            b.Ignore(x => x.CollectionOnDerived);
        });

        VerifyError(
            RelationalStrings.JsonEntityMultipleRootsMappedToTheSameJsonColumn(
                "jsonColumn", nameof(ValidatorJsonEntityInheritanceBase)),
            modelBuilder);
    }

    [Fact]
    public void Json_entity_mapped_to_different_view_than_its_root_aggregate()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.ToView("MyView");
            b.OwnsOne(
                x => x.OwnedReference, bb =>
                {
                    bb.ToJson();
                    bb.ToView("MyOtherView");
                    bb.Ignore(x => x.NestedReference);
                    bb.Ignore(x => x.NestedCollection);
                });
            b.Ignore(x => x.OwnedCollection);
        });

        VerifyError(
            RelationalStrings.JsonEntityMappedToDifferentTableOrViewThanOwner(
                nameof(ValidatorJsonOwnedRoot), "MyOtherView", nameof(ValidatorJsonEntityBasic), "MyView"),
            modelBuilder);
    }

    [Fact]
    public void Json_entity_mapped_to_different_view_than_its_parent()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.ToView("MyView");
            b.OwnsOne(
                x => x.OwnedReference, bb =>
                {
                    bb.ToJson();
                    bb.ToView("MyView");
                    bb.OwnsMany(x => x.NestedCollection, bbb => bbb.ToView("MyOtherView"));
                    bb.Ignore(x => x.NestedReference);
                });
            b.Ignore(x => x.OwnedCollection);
        });

        VerifyError(
            RelationalStrings.JsonEntityMappedToDifferentTableOrViewThanOwner(
                nameof(ValidatorJsonOwnedBranch), "MyOtherView", nameof(ValidatorJsonOwnedRoot), "MyView"),
            modelBuilder);
    }

    [Fact]
    public void Json_entity_mapped_to_different_table_than_its_parent()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.ToTable("MyTable");
            b.OwnsOne(
                x => x.OwnedReference, bb =>
                {
                    bb.ToJson();
                    bb.ToTable("MyJsonTable");
                    bb.Ignore(x => x.NestedCollection);
                    bb.Ignore(x => x.NestedReference);
                });
            b.Ignore(x => x.OwnedCollection);
        });

        VerifyError(
            RelationalStrings.JsonEntityMappedToDifferentTableOrViewThanOwner(
                nameof(ValidatorJsonOwnedRoot), "MyJsonTable", nameof(ValidatorJsonEntityBasic), "MyTable"),
            modelBuilder);
    }

    [Fact]
    public void Json_entity_mapped_to_a_view_but_its_parent_is_mapped_to_a_table()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.ToTable("MyTable");
            b.OwnsOne(
                x => x.OwnedReference, bb =>
                {
                    bb.ToJson();
                    bb.ToView("MyJsonView");
                    bb.Ignore(x => x.NestedCollection);
                    bb.Ignore(x => x.NestedReference);
                });
            b.Ignore(x => x.OwnedCollection);
        });

        VerifyError(
            RelationalStrings.JsonEntityMappedToDifferentTableOrViewThanOwner(
                nameof(ValidatorJsonOwnedRoot), "MyJsonView", nameof(ValidatorJsonEntityBasic), "MyTable"),
            modelBuilder);
    }

    [Fact]
    public void Json_entity_mapped_to_a_table_but_its_parent_is_mapped_to_a_view()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.ToView("MyView");
            b.OwnsOne(
                x => x.OwnedReference, bb =>
                {
                    bb.ToJson();
                    bb.ToTable("MyJsonTable");
                    bb.Ignore(x => x.NestedCollection);
                    bb.Ignore(x => x.NestedReference);
                });
            b.Ignore(x => x.OwnedCollection);
        });

        VerifyError(
            RelationalStrings.JsonEntityMappedToDifferentTableOrViewThanOwner(
                nameof(ValidatorJsonOwnedRoot), "MyJsonTable", nameof(ValidatorJsonEntityBasic), "MyView"),
            modelBuilder);
    }

    [Fact]
    public void SeedData_on_json_entity_throws_meaningful_exception()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.HasData(new { Id = 1, OwnedReference = new { Name = "foo", Number = 1 } });
            b.OwnsOne(
                x => x.OwnedReference, bb =>
                {
                    bb.ToJson();
                    bb.Ignore(x => x.NestedCollection);
                    bb.Ignore(x => x.NestedReference);
                });
            b.Ignore(x => x.OwnedCollection);
        });

        VerifyError(
            RelationalStrings.HasDataNotSupportedForEntitiesMappedToJson(nameof(ValidatorJsonEntityBasic)),
            modelBuilder);
    }

    [Fact]
    public void SeedData_on_entity_with_json_navigation_throws_meaningful_exception()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.HasData(new { Id = 1 });
            b.OwnsMany(
                x => x.OwnedCollection, bb =>
                {
                    bb.ToJson();
                    bb.HasData(
                        new { Name = "foo", Number = 1 },
                        new { Name = "bar", Number = 2 });
                    bb.Ignore(x => x.NestedCollection);
                    bb.Ignore(x => x.NestedReference);
                });
            b.Ignore(x => x.OwnedReference);
        });

        VerifyError(
            RelationalStrings.HasDataNotSupportedForEntitiesMappedToJson(nameof(ValidatorJsonOwnedRoot)),
            modelBuilder);
    }

    [Fact]
    public void Throws_when_complex_property_has_both_json_column_and_json_property_name()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorComplexEntity>()
            .ComplexProperty(
                e => e.ComplexProp, b =>
                {
                    b.ToJson("complex_data");
                    b.HasJsonPropertyName("ComplexData");
                });

        VerifyError(
            RelationalStrings.ComplexPropertyBothJsonColumnAndJsonPropertyName(
                "ValidatorComplexEntity.ComplexProp", "complex_data", "ComplexData"),
            modelBuilder);
    }

    [Fact]
    public void Throws_when_has_json_property_name_on_non_json_mapped_complex_property()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorComplexEntity>()
            .ComplexProperty(
                e => e.ComplexProp, b => b.HasJsonPropertyName("ComplexData"));

        VerifyError(
            RelationalStrings.ComplexPropertyJsonPropertyNameWithoutJsonMapping(
                "ValidatorComplexEntity.ComplexProp"),
            modelBuilder);
    }

    [Fact]
    public void Throws_when_nested_complex_property_mapped_to_json_with_table_sharing()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorComplexEntity>()
            .ComplexProperty(
                e => e.ComplexProp, outerBuilder => outerBuilder.ComplexProperty(
                    x => x.NestedComplex, innerBuilder => innerBuilder.ToJson("inner_json")));

        VerifyError(
            RelationalStrings.NestedComplexPropertyJsonWithTableSharing(
                "ValidatorComplexEntity.ComplexProp#ValidatorComplexType.NestedComplex",
                "ValidatorComplexEntity.ComplexProp#ValidatorComplexType"),
            modelBuilder);
    }

    [Fact]
    public void Throws_when_properties_in_complex_type_have_same_json_property_name()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorComplexEntity>()
            .ComplexProperty(
                e => e.ComplexProp, b =>
                {
                    b.ToJson("complex_data");
                    b.Property(x => x.Name).HasJsonPropertyName("nested");
                    b.Property(x => x.Number).HasJsonPropertyName("nested");
                });

        VerifyError(
            RelationalStrings.JsonObjectWithMultiplePropertiesMappedToSameJsonProperty(
                "Name",
                "Number",
                "ValidatorComplexEntity.ComplexProp#ValidatorComplexType",
                "nested"),
            modelBuilder);
    }

    [Fact]
    public void Throws_when_nested_json_entities_have_same_json_property_name()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.ComplexProperty(
                x => x.OwnedReference, bb =>
                {
                    bb.ToJson("reference");
                    bb.ComplexProperty(x => x.NestedReference, bbb => bbb.HasJsonPropertyName("nested"));
                    bb.ComplexCollection(x => x.NestedCollection, bbb => bbb.HasJsonPropertyName("nested"));
                });
            b.Ignore(x => x.OwnedCollection);
        });

        VerifyError(
            RelationalStrings.JsonObjectWithMultiplePropertiesMappedToSameJsonProperty(
                "NestedCollection",
                "NestedReference",
                "ValidatorJsonEntityBasic.OwnedReference#ValidatorJsonOwnedRoot",
                "nested"),
            modelBuilder);
    }

    [Fact]
    public void Throws_when_owned_entity_and_complex_property_mapped_to_same_json_column()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.OwnsOne(
                x => x.OwnedReference, bb =>
                {
                    bb.ToJson("json_data");
                    bb.Ignore(x => x.NestedReference);
                    bb.Ignore(x => x.NestedCollection);
                });
            b.ComplexCollection(
                x => x.OwnedCollection, cb =>
                {
                    cb.ToJson("json_data");
                    cb.Ignore(x => x.NestedReference);
                    cb.Ignore(x => x.NestedCollection);
                });
        });

        VerifyError(
            RelationalStrings.JsonEntityMultipleRootsMappedToTheSameJsonColumn(
                "json_data", nameof(ValidatorJsonEntityBasic)),
            modelBuilder);
    }

    [Fact]
    public void Throws_when_nested_complex_property_in_json_complex_type_is_mapped_to_json_column()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.Ignore(x => x.OwnedCollection);
            b.ComplexProperty(
                x => x.OwnedReference, cb =>
                {
                    cb.ToJson("json_data");
                    cb.ComplexProperty(x => x.NestedReference, nb => nb.ToJson("nested_json_data"));
                    cb.Ignore(x => x.NestedCollection);
                });
        });

        VerifyError(
            RelationalStrings.NestedComplexPropertyJsonColumn(
                "ValidatorJsonEntityBasic.OwnedReference#ValidatorJsonOwnedRoot.NestedReference",
                "nested_json_data",
                "ValidatorJsonEntityBasic.OwnedReference#ValidatorJsonOwnedRoot",
                "json_data"),
            modelBuilder);
    }

    [Fact]
    public void Throws_when_complex_properties_on_base_and_derived_types_are_mapped_to_same_json_column()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>(
            b => b.ComplexProperty(x => x.ReferenceOnBase, cb => cb.ToJson("json_data")));
        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(b =>
        {
            b.ComplexProperty(x => x.ReferenceOnDerived, cb => cb.ToJson("json_data"));
            b.Ignore(x => x.CollectionOnDerived);
        });

        VerifyError(
            RelationalStrings.ComplexPropertiesSharedJsonColumn(
                "ValidatorJsonEntityInheritanceBase.ReferenceOnBase",
                "ValidatorJsonEntityInheritanceDerived.ReferenceOnDerived",
                "json_data",
                nameof(ValidatorJsonEntityInheritanceBase)),
            modelBuilder);
    }

    [Fact]
    public void Passes_for_complex_properties_on_sibling_types_mapped_to_same_json_column()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>().Ignore(x => x.ReferenceOnBase);
        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(b =>
        {
            b.ComplexProperty(x => x.ReferenceOnDerived, cb => cb.ToJson("json_data"));
            b.Ignore(x => x.CollectionOnDerived);
        });
        modelBuilder.Entity<ValidatorJsonEntityInheritanceSibling>(
            b => b.ComplexProperty(x => x.ReferenceOnSibling, cb => cb.ToJson("json_data")));

        Validate(modelBuilder);
    }

    [Fact]
    public void Throws_when_complex_collection_and_complex_property_on_sibling_types_are_mapped_to_same_json_column()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>().Ignore(x => x.ReferenceOnBase);
        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(b =>
        {
            b.Ignore(x => x.ReferenceOnDerived);
            b.ComplexCollection(x => x.CollectionOnDerived, cb => cb.ToJson("json_data"));
        });
        modelBuilder.Entity<ValidatorJsonEntityInheritanceSibling>(
            b => b.ComplexProperty(x => x.ReferenceOnSibling, cb => cb.ToJson("json_data")));

        VerifyError(
            RelationalStrings.ComplexCollectionSharedJsonColumn(
                "ValidatorJsonEntityInheritanceDerived.CollectionOnDerived",
                "ValidatorJsonEntityInheritanceSibling.ReferenceOnSibling",
                "json_data",
                nameof(ValidatorJsonEntityInheritanceBase)),
            modelBuilder);
    }

    [Fact]
    public void Throws_when_complex_properties_on_principal_and_dependent_sharing_table_are_mapped_to_same_json_column()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonTableSplittingPrincipal>(b =>
        {
            b.ToTable("Shared");
            b.HasOne(x => x.Dependent).WithOne().HasForeignKey<ValidatorJsonTableSplittingDependent>(x => x.Id);
            b.ComplexProperty(x => x.Data, cb => cb.ToJson("json_data"));
        });
        modelBuilder.Entity<ValidatorJsonTableSplittingDependent>(b =>
        {
            b.ToTable("Shared");
            b.ComplexProperty(x => x.Data, cb => cb.ToJson("json_data"));
        });

        VerifyError(
            RelationalStrings.ComplexPropertiesSharedJsonColumn(
                "ValidatorJsonTableSplittingDependent.Data",
                "ValidatorJsonTableSplittingPrincipal.Data",
                "json_data",
                "Shared"),
            modelBuilder);
    }

    [Fact]
    public void Does_not_warn_for_optional_dependent_identified_by_required_json_complex_property()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonTableSplittingPrincipal>(b =>
        {
            b.ToTable("Shared");
            b.HasOne(x => x.Dependent).WithOne().HasForeignKey<ValidatorJsonTableSplittingDependent>(x => x.Id);
            b.ComplexProperty(x => x.Data, cb => cb.ToJson("principal_data"));
        });
        modelBuilder.Entity<ValidatorJsonTableSplittingDependent>(b =>
        {
            b.ToTable("Shared");
            b.ComplexProperty(x => x.Data, cb => cb.ToJson("dependent_data"));
        });

        var definition =
            RelationalResources.LogOptionalDependentWithoutIdentifyingProperty(new TestLogger<TestRelationalLoggingDefinitions>());
        VerifyLogDoesNotContain(definition.GenerateMessage(nameof(ValidatorJsonTableSplittingDependent)), modelBuilder);
    }

    [Fact]
    public void Does_not_warn_for_optional_dependent_identified_by_required_json_complex_collection()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonTableSplittingPrincipal>(b =>
        {
            b.ToTable("Shared");
            b.Ignore(x => x.Dependent);
            b.HasOne<ValidatorJsonTableSplittingCollectionDependent>().WithOne()
                .HasForeignKey<ValidatorJsonTableSplittingCollectionDependent>(x => x.Id);
            b.ComplexProperty(x => x.Data, cb => cb.ToJson("principal_data"));
        });
        modelBuilder.Entity<ValidatorJsonTableSplittingCollectionDependent>(b =>
        {
            b.ToTable("Shared");
            b.ComplexCollection(x => x.Data, cb => cb.ToJson("dependent_data"));
        });

        var definition =
            RelationalResources.LogOptionalDependentWithoutIdentifyingProperty(new TestLogger<TestRelationalLoggingDefinitions>());
        VerifyLogDoesNotContain(definition.GenerateMessage(nameof(ValidatorJsonTableSplittingCollectionDependent)), modelBuilder);
    }

    [Fact]
    public void Throws_when_complex_properties_on_sibling_type_and_dependent_of_other_sibling_are_mapped_to_same_json_column()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>().Ignore(x => x.ReferenceOnBase);
        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(b =>
        {
            b.Ignore(x => x.ReferenceOnDerived);
            b.Ignore(x => x.CollectionOnDerived);
            b.HasOne<ValidatorJsonTableSplittingDependent>().WithOne().HasForeignKey<ValidatorJsonTableSplittingDependent>(x => x.Id);
        });
        modelBuilder.Entity<ValidatorJsonEntityInheritanceSibling>(
            b => b.ComplexProperty(x => x.ReferenceOnSibling, cb => cb.ToJson("json_data")));
        modelBuilder.Entity<ValidatorJsonTableSplittingDependent>(b =>
        {
            b.ToTable(nameof(ValidatorJsonEntityInheritanceBase));
            b.ComplexProperty(x => x.Data, cb => cb.ToJson("json_data"));
        });

        // Querying the dependent doesn't filter by the principal's discriminator, so it would read the sibling's JSON
        VerifyError(
            RelationalStrings.ComplexPropertiesSharedJsonColumn(
                "ValidatorJsonEntityInheritanceSibling.ReferenceOnSibling",
                "ValidatorJsonTableSplittingDependent.Data",
                "json_data",
                nameof(ValidatorJsonEntityInheritanceBase)),
            modelBuilder);
    }

    [Fact]
    public void Throws_when_complex_properties_on_derived_type_and_dependent_of_base_type_are_mapped_to_same_json_column()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>(b =>
        {
            b.Ignore(x => x.ReferenceOnBase);
            b.HasOne<ValidatorJsonTableSplittingDependent>().WithOne().HasForeignKey<ValidatorJsonTableSplittingDependent>(x => x.Id);
        });
        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(b =>
        {
            b.Ignore(x => x.ReferenceOnDerived);
            b.Ignore(x => x.CollectionOnDerived);
        });
        modelBuilder.Entity<ValidatorJsonEntityInheritanceSibling>(
            b => b.ComplexProperty(x => x.ReferenceOnSibling, cb => cb.ToJson("json_data")));
        modelBuilder.Entity<ValidatorJsonTableSplittingDependent>(b =>
        {
            b.ToTable(nameof(ValidatorJsonEntityInheritanceBase));
            b.ComplexProperty(x => x.Data, cb => cb.ToJson("json_data"));
        });

        VerifyError(
            RelationalStrings.ComplexPropertiesSharedJsonColumn(
                "ValidatorJsonEntityInheritanceSibling.ReferenceOnSibling",
                "ValidatorJsonTableSplittingDependent.Data",
                "json_data",
                nameof(ValidatorJsonEntityInheritanceBase)),
            modelBuilder);
    }

    [Fact]
    public void Throws_when_complex_properties_on_derived_principal_and_dependent_sharing_table_are_mapped_to_same_json_column()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>().Ignore(x => x.ReferenceOnBase);
        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(b =>
        {
            b.ComplexProperty(x => x.ReferenceOnDerived, cb => cb.ToJson("json_data"));
            b.Ignore(x => x.CollectionOnDerived);
            b.HasOne<ValidatorJsonTableSplittingDependent>().WithOne().HasForeignKey<ValidatorJsonTableSplittingDependent>(x => x.Id);
        });
        modelBuilder.Entity<ValidatorJsonEntityInheritanceSibling>().Ignore(x => x.ReferenceOnSibling);
        modelBuilder.Entity<ValidatorJsonTableSplittingDependent>(b =>
        {
            b.ToTable(nameof(ValidatorJsonEntityInheritanceBase));
            b.ComplexProperty(x => x.Data, cb => cb.ToJson("json_data"));
        });

        VerifyError(
            RelationalStrings.ComplexPropertiesSharedJsonColumn(
                "ValidatorJsonEntityInheritanceDerived.ReferenceOnDerived",
                "ValidatorJsonTableSplittingDependent.Data",
                "json_data",
                nameof(ValidatorJsonEntityInheritanceBase)),
            modelBuilder);
    }

    [Fact]
    public void Passes_for_complex_properties_on_sibling_dependent_types_mapped_to_same_json_column()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonTableSplittingPrincipal>(b =>
        {
            b.ToTable("Shared");
            b.HasOne(x => x.Dependent).WithOne().HasForeignKey<ValidatorJsonTableSplittingDependent>(x => x.Id);
            b.ComplexProperty(x => x.Data, cb => cb.ToJson("principal_data"));
        });
        modelBuilder.Entity<ValidatorJsonTableSplittingDependent>(b =>
        {
            b.ToTable("Shared");
            b.ComplexProperty(x => x.Data, cb => cb.ToJson("dependent_data"));
        });
        modelBuilder.Entity<ValidatorJsonTableSplittingDependentA>(b => b.ComplexProperty(x => x.Extra, cb => cb.ToJson("extra")));
        modelBuilder.Entity<ValidatorJsonTableSplittingDependentB>(b => b.ComplexProperty(x => x.Extra, cb => cb.ToJson("extra")));

        Validate(modelBuilder);
    }

    [Fact]
    public void Throws_when_owned_entity_and_complex_property_on_base_and_derived_types_are_mapped_to_same_json_column()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>(
            b => b.OwnsOne(x => x.ReferenceOnBase, ob => ob.ToJson("json_data")));
        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(b =>
        {
            b.ComplexProperty(x => x.ReferenceOnDerived, cb => cb.ToJson("json_data"));
            b.Ignore(x => x.CollectionOnDerived);
        });

        VerifyError(
            RelationalStrings.JsonEntityMultipleRootsMappedToTheSameJsonColumn(
                "json_data", nameof(ValidatorJsonEntityInheritanceBase)),
            modelBuilder);
    }

    [Fact]
    public void Throws_when_owned_entity_and_complex_property_on_sibling_types_are_mapped_to_same_json_column()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>().Ignore(x => x.ReferenceOnBase);
        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(b =>
        {
            b.OwnsOne(x => x.ReferenceOnDerived, ob => ob.ToJson("json_data"));
            b.Ignore(x => x.CollectionOnDerived);
        });
        modelBuilder.Entity<ValidatorJsonEntityInheritanceSibling>(
            b => b.ComplexProperty(x => x.ReferenceOnSibling, cb => cb.ToJson("json_data")));

        VerifyError(
            RelationalStrings.JsonEntityMultipleRootsMappedToTheSameJsonColumn(
                "json_data", nameof(ValidatorJsonEntityInheritanceBase)),
            modelBuilder);
    }

    [Fact]
    public void Throws_when_owned_entities_on_sibling_types_are_mapped_to_same_json_column()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>().Ignore(x => x.ReferenceOnBase);
        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(b =>
        {
            b.OwnsOne(x => x.ReferenceOnDerived, ob => ob.ToJson("json_data"));
            b.Ignore(x => x.CollectionOnDerived);
        });
        modelBuilder.Entity<ValidatorJsonEntityInheritanceSibling>(
            b => b.OwnsOne(x => x.ReferenceOnSibling, ob => ob.ToJson("json_data")));

        VerifyError(
            RelationalStrings.JsonEntityMultipleRootsMappedToTheSameJsonColumn(
                "json_data", nameof(ValidatorJsonEntityInheritanceBase)),
            modelBuilder);
    }

    [Fact]
    public void Passes_for_complex_properties_on_base_and_derived_types_mapped_to_same_json_column_with_TPT()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>(b =>
        {
            b.UseTptMappingStrategy();
            b.ComplexProperty(x => x.ReferenceOnBase, cb => cb.ToJson("json_data"));
        });
        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(b =>
        {
            b.ComplexProperty(x => x.ReferenceOnDerived, cb => cb.ToJson("json_data"));
            b.Ignore(x => x.CollectionOnDerived);
        });

        Validate(modelBuilder);
    }

    [Fact]
    public void Throws_when_complex_properties_on_base_and_derived_types_are_mapped_to_same_json_column_with_TPC()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>(b =>
        {
            b.UseTpcMappingStrategy();
            b.ComplexProperty(x => x.ReferenceOnBase, cb => cb.ToJson("json_data"));
        });
        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(b =>
        {
            b.ComplexProperty(x => x.ReferenceOnDerived, cb => cb.ToJson("json_data"));
            b.Ignore(x => x.CollectionOnDerived);
        });

        VerifyError(
            RelationalStrings.ComplexPropertiesSharedJsonColumn(
                "ValidatorJsonEntityInheritanceBase.ReferenceOnBase",
                "ValidatorJsonEntityInheritanceDerived.ReferenceOnDerived",
                "json_data",
                nameof(ValidatorJsonEntityInheritanceDerived)),
            modelBuilder);
    }

    [Fact]
    public void Passes_for_json_complex_property_on_base_type_with_owned_json_entity_in_same_table()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>(
            b => b.ComplexProperty(x => x.ReferenceOnBase, cb => cb.ToJson("complex_json")));
        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(b =>
        {
            b.OwnsOne(x => x.ReferenceOnDerived, ob => ob.ToJson("owned_json"));
            b.Ignore(x => x.CollectionOnDerived);
        });

        Validate(modelBuilder);
    }

    [Fact]
    public void Passes_for_complex_properties_on_sibling_types_mapped_to_same_json_column_with_owned_json_entity_in_same_table()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityInheritanceBase>(
            b => b.OwnsOne(x => x.ReferenceOnBase, ob => ob.ToJson("owned_json")));
        modelBuilder.Entity<ValidatorJsonEntityInheritanceDerived>(b =>
        {
            b.ComplexProperty(x => x.ReferenceOnDerived, cb => cb.ToJson("json_data"));
            b.Ignore(x => x.CollectionOnDerived);
        });
        modelBuilder.Entity<ValidatorJsonEntityInheritanceSibling>(
            b => b.ComplexProperty(x => x.ReferenceOnSibling, cb => cb.ToJson("json_data")));

        Validate(modelBuilder);
    }

    [Fact]
    public void Throws_when_owned_entity_property_has_both_column_name_and_json_property_name()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.OwnsOne(
                x => x.OwnedReference, bb =>
                {
                    bb.ToJson("reference");
                    bb.Property(x => x.Name).HasColumnName("column_name").HasJsonPropertyName("json_name");
                    bb.Ignore(x => x.NestedReference);
                    bb.Ignore(x => x.NestedCollection);
                });
            b.Ignore(x => x.OwnedCollection);
        });

        VerifyError(
            RelationalStrings.PropertyBothColumnNameAndJsonPropertyName(
                "ValidatorJsonOwnedRoot.Name",
                "column_name",
                "json_name"),
            modelBuilder);
    }

    [Fact]
    public void Throws_when_complex_property_scalar_has_both_column_name_and_json_property_name()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorComplexEntity>()
            .ComplexProperty(
                e => e.ComplexProp, b =>
                {
                    b.ToJson("complex_data");
                    b.Property(x => x.Name).HasColumnName("column_name").HasJsonPropertyName("json_name");
                });

        VerifyError(
            RelationalStrings.PropertyBothColumnNameAndJsonPropertyName(
                "ValidatorComplexEntity.ComplexProp#ValidatorComplexType.Name",
                "column_name",
                "json_name"),
            modelBuilder);
    }

    protected class ValidatorComplexEntity
    {
        public int Id { get; set; }
        public ValidatorComplexType ComplexProp { get; set; } = null!;
    }

    protected class ValidatorComplexType
    {
        public string Name { get; set; } = null!;
        public int Number { get; set; }
        public ValidatorNestedComplexType NestedComplex { get; set; } = null!;
    }

    protected class ValidatorNestedComplexType
    {
        public string Value { get; set; } = null!;
        public int Count { get; set; }
    }

    protected class ValidatorJsonEntityBasic
    {
        public int Id { get; set; }
        public ValidatorJsonOwnedRoot OwnedReference { get; set; } = null!;
        public List<ValidatorJsonOwnedRoot> OwnedCollection { get; set; } = null!;
    }

    protected abstract class ValidatorJsonEntityInheritanceAbstract : ValidatorJsonEntityInheritanceBase
    {
        public Guid Guid { get; set; }
    }

    protected class ValidatorJsonEntityInheritanceBase
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ValidatorJsonOwnedBranch ReferenceOnBase { get; set; } = null!;
    }

    protected class ValidatorJsonEntityInheritanceDerived : ValidatorJsonEntityInheritanceAbstract
    {
        public bool Switch { get; set; }

        public ValidatorJsonOwnedBranch ReferenceOnDerived { get; set; } = null!;

        public List<ValidatorJsonOwnedBranch> CollectionOnDerived { get; set; } = null!;
    }

    protected class ValidatorJsonEntityInheritanceSibling : ValidatorJsonEntityInheritanceBase
    {
        public ValidatorJsonOwnedBranch ReferenceOnSibling { get; set; } = null!;
    }

    protected class ValidatorJsonTableSplittingPrincipal
    {
        public int Id { get; set; }
        public ValidatorJsonOwnedBranch Data { get; set; } = null!;
        public ValidatorJsonTableSplittingDependent? Dependent { get; set; }
    }

    protected class ValidatorJsonTableSplittingDependent
    {
        public int Id { get; set; }
        public ValidatorJsonOwnedBranch Data { get; set; } = null!;
    }

    protected class ValidatorJsonTableSplittingCollectionDependent
    {
        public int Id { get; set; }
        public List<ValidatorJsonOwnedBranch> Data { get; set; } = null!;
    }

    protected class ValidatorJsonTableSplittingDependentA : ValidatorJsonTableSplittingDependent
    {
        public ValidatorJsonOwnedBranch Extra { get; set; } = null!;
    }

    protected class ValidatorJsonTableSplittingDependentB : ValidatorJsonTableSplittingDependent
    {
        public ValidatorJsonOwnedBranch Extra { get; set; } = null!;
    }

    protected class ValidatorJsonOwnedRoot
    {
        public string Name { get; set; } = null!;
        public int Number { get; }

        public ValidatorJsonOwnedBranch NestedReference { get; } = null!;
        public List<ValidatorJsonOwnedBranch> NestedCollection { get; } = null!;
    }

    protected class ValidatorJsonOwnedBranch
    {
        public double Number { get; set; }
    }

    protected class ValidatorJsonEntityExplicitOrdinal
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public List<ValidatorJsonOwnedExplicitOrdinal> OwnedCollection { get; set; } = null!;
    }

    protected class ValidatorJsonOwnedExplicitOrdinal
    {
        public int Ordinal { get; set; }
        public DateTime Date { get; set; }
    }

    protected class ValidatorJsonEntityJsonReferencingRegularEntity
    {
        public int Id { get; set; }
        public ValidatorJsonOwnedReferencingRegularEntity Owned { get; set; } = null!;
    }

    protected class ValidatorJsonOwnedReferencingRegularEntity
    {
        public string Foo { get; set; } = null!;

        public int? Fk { get; }
        public ValidatorJsonEntityReferencedEntity Reference { get; } = null!;
    }

    protected class ValidatorJsonEntityReferencedEntity
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
    }

    protected class ValidatorJsonEntitySideBySide
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ValidatorJsonOwnedBranch Reference1 { get; set; } = null!;
        public ValidatorJsonOwnedBranch Reference2 { get; set; } = null!;
        public List<ValidatorJsonOwnedBranch> Collection1 { get; set; } = null!;
        public List<ValidatorJsonOwnedBranch> Collection2 { get; set; } = null!;
    }

    protected class ValidatorJsonEntityTableSplitting
    {
        public int Id { get; set; }
        public ValidatorJsonEntityBasic Link { get; set; } = null!;
    }

    [Fact]
    public void Throw_when_concurrency_token_configured_on_json_mapped_owned_entity()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<ValidatorJsonEntityBasic>(b =>
        {
            b.OwnsOne(
                x => x.OwnedReference, bb =>
                {
                    bb.ToJson();
                    bb.Property(x => x.Name).IsConcurrencyToken();
                    bb.Ignore(x => x.NestedCollection);
                    bb.Ignore(x => x.NestedReference);
                });
            b.Ignore(x => x.OwnedCollection);
        });

        VerifyError(
            RelationalStrings.ConcurrencyTokenOnJsonMappedProperty("Name", nameof(ValidatorJsonOwnedRoot)),
            modelBuilder);
    }

    [Fact]
    public void Throw_when_concurrency_token_configured_on_json_mapped_complex_property()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<ValidatorJsonEntityBasic>(b => b.ComplexProperty(
            x => x.OwnedReference, cb =>
            {
                cb.ToJson();
                cb.Property(x => x.Name).IsConcurrencyToken();
                cb.Ignore(x => x.NestedCollection);
                cb.Ignore(x => x.NestedReference);
            }));

        VerifyError(
            RelationalStrings.ConcurrencyTokenOnJsonMappedProperty(
                "Name", "ValidatorJsonEntityBasic.OwnedReference#ValidatorJsonOwnedRoot"),
            modelBuilder);
    }
}
