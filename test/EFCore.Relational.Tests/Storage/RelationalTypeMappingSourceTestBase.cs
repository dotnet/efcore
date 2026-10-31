// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Storage;

public abstract class RelationalTypeMappingSourceTestBase
{
    [Theory, InlineData(false), InlineData(true)]
    public void JSON_reader_writer_defaults_apply_to_model_aware_type_lookup(bool converted)
    {
        var builder = CreateModelBuilder(c =>
        {
            var configuration = c.DefaultTypeMapping<int>();
            if (converted)
            {
                configuration.HasConversion<string>().HasJsonValueReaderWriter<JsonStringReaderWriter>();
            }
            else
            {
                configuration.HasJsonValueReaderWriter<JsonInt32AsStringReaderWriter>();
            }
        });
        var model = builder.FinalizeModel();
        var source = CreateRelationalTypeMappingSource(model);

        var mapping = source.GetMapping(typeof(int), model);
        Assert.Equal(typeof(int), mapping.JsonValueReaderWriter!.ValueType);
        Assert.Equal("\"42\"", mapping.JsonValueReaderWriter!.ToJsonString(42));
        Assert.Equal(42, mapping.JsonValueReaderWriter.FromJsonString("\"42\""));
        if (converted)
        {
            Assert.Equal(typeof(int), mapping.Converter!.ModelClrType);
            Assert.Equal(typeof(string), mapping.Converter.ProviderClrType);
            Assert.Equal("42", mapping.Converter.ConvertToProvider(42));
            Assert.Equal(42, mapping.Converter.ConvertFromProvider("42"));
        }

        Assert.Same(mapping, source.GetMapping(typeof(int?), model));
        Assert.NotSame(mapping, source.GetMapping(typeof(int)));
    }

    [Fact]
    public void JSON_reader_writer_defaults_do_not_leak_between_models()
    {
        var customized = CreateModelBuilder(c =>
            c.DefaultTypeMapping<int>().HasJsonValueReaderWriter<JsonInt32AsStringReaderWriter>()).FinalizeModel();
        var unconfigured = CreateModelBuilder().FinalizeModel();
        var source = CreateRelationalTypeMappingSource(customized);

        Assert.Equal("\"42\"", source.GetMapping(typeof(int), customized).JsonValueReaderWriter!.ToJsonString(42));
        Assert.Equal("42", source.GetMapping(typeof(int), unconfigured).JsonValueReaderWriter!.ToJsonString(42));
        Assert.Equal("\"42\"", source.GetMapping(typeof(int), customized).JsonValueReaderWriter!.ToJsonString(42));
    }

    [Fact]
    public void JSON_reader_writer_defaults_reuse_mappings_for_properties_and_elements()
    {
        var builder = CreateModelBuilder(c =>
            c.DefaultTypeMapping<int>().HasJsonValueReaderWriter<JsonInt32AsStringReaderWriter>());
        builder.Entity("Owner").Property<int>("Id");
        builder.Entity("Owner").PrimitiveCollection<List<int>>("Numbers");
        var model = builder.FinalizeModel();
        var source = CreateRelationalTypeMappingSource(model);
        var property = model.FindEntityType("Owner")!.FindProperty("Id")!;
        var element = model.FindEntityType("Owner")!.FindProperty("Numbers")!.GetElementType()!;

        Assert.Same(source.GetMapping(property), source.GetMapping(property));
        Assert.Same(source.FindMapping(element), source.FindMapping(element));
        Assert.Same(source.GetMapping(property).JsonValueReaderWriter, source.FindMapping(element)!.JsonValueReaderWriter);
        Assert.Equal("\"42\"", source.FindMapping(element)!.JsonValueReaderWriter!.ToJsonString(42));
    }

    public sealed class JsonInt32AsStringReaderWriter : JsonValueReaderWriter<int>
    {
        public override int FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => int.Parse(manager.CurrentReader.GetString()!, CultureInfo.InvariantCulture);

        public override void ToJsonTyped(Utf8JsonWriter writer, int value)
            => writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));

        public override Expression ConstructorExpression
            => Expression.New(typeof(JsonInt32AsStringReaderWriter));
    }

    protected IMutableEntityType CreateEntityType<TEntity>()
    {
        var builder = CreateModelBuilder();

        builder.Entity<MyType>().Property(e => e.Id).HasColumnType("money");
        builder.Entity<MyRelatedType1>().Property(e => e.Id).HasMaxLength(200).IsFixedLength();
        builder.Entity<MyRelatedType1>().Property(e => e.Relationship2Id).HasColumnType("dec(6,1)");
        builder.Entity<MyRelatedType2>().Property(e => e.Id).HasMaxLength(100).IsFixedLength();
        builder.Entity<MyRelatedType2>().Property(e => e.Relationship2Id).HasMaxLength(787);
        builder.Entity<MyRelatedType3>().Property(e => e.Id).IsUnicode(false);
        builder.Entity<MyRelatedType3>().Property(e => e.Relationship2Id).HasMaxLength(767);
        builder.Entity<MyRelatedType4>().Property(e => e.Relationship2Id).IsUnicode();
        builder.Entity<MyPrecisionType>().Property(e => e.PrecisionOnly).HasPrecision(16);
        builder.Entity<MyPrecisionType>().Property(e => e.PrecisionAndScale).HasPrecision(18, 7);
        builder.Entity<MyTypeWithIndexAttribute>();
        builder.Entity<MyTypeWithIndexAttributeOnCollection>();

        return builder.Model.FindEntityType(typeof(TEntity))!;
    }

    protected IModel CreateModel()
        => CreateEntityType<MyType>().Model.FinalizeModel();

    protected RelationalTypeMapping GetTypeMapping(
        Type propertyType,
        bool? nullable = null,
        int? maxLength = null,
        int? precision = null,
        int? scale = null,
        Type? providerType = null,
        bool? unicode = null,
        bool? fixedLength = null,
        string? storeTypeName = null,
        bool useConfiguration = false)
    {
        if (useConfiguration)
        {
            var model = CreateModelBuilder(c =>
            {
                var scalarBuilder = c.DefaultTypeMapping(propertyType);

                if (maxLength.HasValue)
                {
                    scalarBuilder.HasMaxLength(maxLength.Value);
                }

                if (precision.HasValue)
                {
                    if (scale.HasValue)
                    {
                        scalarBuilder.HasPrecision(precision.Value, scale.Value);
                    }
                    else
                    {
                        scalarBuilder.HasPrecision(precision.Value);
                    }
                }

                if (providerType != null)
                {
                    scalarBuilder.HasConversion(providerType);
                }

                if (unicode.HasValue)
                {
                    scalarBuilder.IsUnicode(unicode.Value);
                }

                if (fixedLength.HasValue)
                {
                    scalarBuilder.IsFixedLength(fixedLength.Value);
                }

                if (storeTypeName != null)
                {
                    scalarBuilder.HasColumnType(storeTypeName);
                }
            }).FinalizeModel();

            return CreateRelationalTypeMappingSource(model).GetMapping(propertyType, model);
        }
        else
        {
            var modelBuilder = CreateModelBuilder();
            var entityType = modelBuilder.Entity<MyType>();
            entityType.Property(e => e.Id);
            var property = entityType.Property(propertyType, "MyProp").Metadata;

            if (nullable.HasValue)
            {
                property.IsNullable = nullable.Value;
            }

            if (maxLength.HasValue)
            {
                property.SetMaxLength(maxLength);
            }

            if (precision.HasValue)
            {
                property.SetPrecision(precision);
            }

            if (scale.HasValue)
            {
                property.SetScale(scale);
            }

            if (providerType != null)
            {
                property.SetProviderClrType(providerType);
            }

            if (unicode.HasValue)
            {
                property.SetIsUnicode(unicode);
            }

            if (fixedLength.HasValue)
            {
                property.SetIsFixedLength(fixedLength);
            }

            if (storeTypeName != null)
            {
                property.SetColumnType(storeTypeName);
            }

            var model = modelBuilder.Model.FinalizeModel();
            return CreateRelationalTypeMappingSource(model).GetMapping(model.FindEntityType(typeof(MyType))!.FindProperty(property.Name)!);
        }
    }

    protected abstract ModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configureConventions = null);
    protected abstract IRelationalTypeMappingSource CreateRelationalTypeMappingSource(IModel model);

    protected class MyType
    {
        public decimal Id { get; set; }
    }

    protected class MyPrecisionType
    {
        public decimal Id { get; set; }
        public decimal PrecisionOnly { get; set; }
        public decimal PrecisionAndScale { get; set; }
    }

    protected class MyRelatedType1
    {
        public string Id { get; set; } = null!;

        public decimal Relationship1Id { get; set; }
        public MyType Relationship1 { get; set; } = null!;

        public decimal Relationship2Id { get; set; }
        public MyType Relationship2 { get; set; } = null!;
    }

    protected class MyRelatedType2
    {
        public byte[] Id { get; set; } = null!;

        public string Relationship1Id { get; set; } = null!;
        public MyRelatedType1 Relationship1 { get; set; } = null!;

        public string Relationship2Id { get; set; } = null!;
        public MyRelatedType1 Relationship2 { get; set; } = null!;
    }

    protected class MyRelatedType3
    {
        public string Id { get; set; } = null!;

        public byte[] Relationship1Id { get; set; } = null!;
        public MyRelatedType2 Relationship1 { get; set; } = null!;

        public byte[] Relationship2Id { get; set; } = null!;
        public MyRelatedType2 Relationship2 { get; set; } = null!;
    }

    protected class MyRelatedType4
    {
        public string Id { get; set; } = null!;

        public string Relationship1Id { get; set; } = null!;
        public MyRelatedType3 Relationship1 { get; set; } = null!;

        public string Relationship2Id { get; set; } = null!;
        public MyRelatedType3 Relationship2 { get; set; } = null!;
    }

    [Index(nameof(Name))]
    protected class MyTypeWithIndexAttribute
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    [Index(nameof(Ints))]
    protected class MyTypeWithIndexAttributeOnCollection
    {
        public int Id { get; set; }
        public IEnumerable<int> Ints { get; set; } = null!;
    }
}
