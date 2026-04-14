// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using Microsoft.EntityFrameworkCore.Storage.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

public class CosmosTypeMappingSourceTest
{
    [ConditionalFact]
    public void Can_map_sbyte()
        => Can_map_scalar_by_clr_type<sbyte, JsonSByteReaderWriter>(1, JTokenType.Integer, "1");

    [ConditionalFact]
    public void Can_map_short()
        => Can_map_scalar_by_clr_type<short, JsonInt16ReaderWriter>(1, JTokenType.Integer, "1");

    [ConditionalFact]
    public void Can_map_int()
        => Can_map_scalar_by_clr_type<int, JsonInt32ReaderWriter>(1, JTokenType.Integer, "1");

    [ConditionalFact]
    public void Can_map_long()
        => Can_map_scalar_by_clr_type<long, JsonInt64ReaderWriter>(1, JTokenType.Integer, "1");

    [ConditionalFact]
    public void Can_map_byte()
        => Can_map_scalar_by_clr_type<byte, JsonByteReaderWriter>(1, JTokenType.Integer, "1");

    [ConditionalFact]
    public void Can_map_ushort()
        => Can_map_scalar_by_clr_type<ushort, JsonUInt16ReaderWriter>(1, JTokenType.Integer, "1");

    [ConditionalFact]
    public void Can_map_uint()
        => Can_map_scalar_by_clr_type<uint, JsonUInt32ReaderWriter>(1, JTokenType.Integer, "1");

    [ConditionalFact]
    public void Can_map_ulong()
        => Can_map_scalar_by_clr_type<ulong, JsonUInt64ReaderWriter>(1, JTokenType.Integer, "1");

    [ConditionalFact]
    public void Can_map_char()
        => Can_map_scalar_by_clr_type<char, JsonCharReaderWriter>('A', JTokenType.String, "\"A\"");

    [ConditionalFact]
    public void Can_map_decimal()
        => Can_map_scalar_by_clr_type<decimal, JsonDecimalReaderWriter>(1.33m, JTokenType.Float, "1.33");

    [ConditionalFact]
    public void Can_map_float()
        => Can_map_scalar_by_clr_type<float, JsonFloatReaderWriter>(1.33f, JTokenType.Float, "1.33", "DefaultFloatValueComparer");

    [ConditionalFact]
    public void Can_map_double()
        => Can_map_scalar_by_clr_type<double, JsonDoubleReaderWriter>(1.33, JTokenType.Float, "1.33", "DefaultDoubleValueComparer");

    [ConditionalFact]
    public void Can_map_bool()
        => Can_map_scalar_by_clr_type<bool, JsonBoolReaderWriter>(true, JTokenType.Boolean, "true");

    [ConditionalFact]
    public void Can_map_DateOnly()
        => Can_map_scalar_by_clr_type<DateOnly, JsonDateOnlyReaderWriter>(new DateOnly(2003, 12, 25), JTokenType.String, "\"2003-12-25\"");

    [ConditionalFact]
    public void Can_map_TimeOnly()
        => Can_map_scalar_by_clr_type<TimeOnly, JsonTimeOnlyReaderWriter>(
            new TimeOnly(20, 19, 12, 254), JTokenType.String, "\"20:19:12.254\"");

    [ConditionalFact]
    public void Can_map_DateTime()
        => Can_map_scalar_by_clr_type<DateTime, JsonDateTimeReaderWriter>(
            new DateTime(2003, 12, 25, 20, 19, 12, 254), JTokenType.Date, "\"2003-12-25T20:19:12.254\"");

    [ConditionalFact]
    public void Can_map_DateTimeOffset()
        => Can_map_scalar_by_clr_type<DateTimeOffset, JsonDateTimeOffsetReaderWriter>(
            new DateTimeOffset(2003, 12, 25, 20, 19, 12, 254, new TimeSpan(4, 30, 0)), JTokenType.Date, "\"2003-12-25T20:19:12.254+04:30\"",
            "DefaultDateTimeOffsetValueComparer");

    [ConditionalFact]
    public void Can_map_TimeSpan()
        => Can_map_scalar_by_clr_type<TimeSpan, JsonTimeSpanReaderWriter>(new TimeSpan(2, 3, 4, 5), JTokenType.TimeSpan, "\"2.03:04:05\"");

    [ConditionalFact]
    public void Can_map_string()
        => Can_map_scalar_by_clr_type<string, JsonStringReaderWriter>("Hello", JTokenType.String, "\"Hello\"");

    [ConditionalFact]
    public void Can_map_nullable_sbyte()
        => Can_map_scalar_by_clr_type<sbyte?, JsonSByteReaderWriter>(1, JTokenType.Integer, "1");

    [ConditionalFact]
    public void Can_map_nullable_short()
        => Can_map_scalar_by_clr_type<short?, JsonInt16ReaderWriter>(1, JTokenType.Integer, "1");

    [ConditionalFact]
    public void Can_map_nullable_int()
        => Can_map_scalar_by_clr_type<int?, JsonInt32ReaderWriter>(1, JTokenType.Integer, "1");

    [ConditionalFact]
    public void Can_map_nullable_long()
        => Can_map_scalar_by_clr_type<long?, JsonInt64ReaderWriter>(1, JTokenType.Integer, "1");

    [ConditionalFact]
    public void Can_map_nullable_byte()
        => Can_map_scalar_by_clr_type<byte?, JsonByteReaderWriter>(1, JTokenType.Integer, "1");

    [ConditionalFact]
    public void Can_map_nullable_ushort()
        => Can_map_scalar_by_clr_type<ushort?, JsonUInt16ReaderWriter>(1, JTokenType.Integer, "1");

    [ConditionalFact]
    public void Can_map_nullable_uint()
        => Can_map_scalar_by_clr_type<uint?, JsonUInt32ReaderWriter>(1, JTokenType.Integer, "1");

    [ConditionalFact]
    public void Can_map_nullable_ulong()
        => Can_map_scalar_by_clr_type<ulong?, JsonUInt64ReaderWriter>(1, JTokenType.Integer, "1");

    [ConditionalFact]
    public void Can_map_nullable_char()
        => Can_map_scalar_by_clr_type<char?, JsonCharReaderWriter>('A', JTokenType.String, "\"A\"");

    [ConditionalFact]
    public void Can_map_nullable_decimal()
        => Can_map_scalar_by_clr_type<decimal?, JsonDecimalReaderWriter>(1.33m, JTokenType.Float, "1.33");

    [ConditionalFact]
    public void Can_map_nullable_float()
        => Can_map_scalar_by_clr_type<float?, JsonFloatReaderWriter>(1.33f, JTokenType.Float, "1.33", "DefaultFloatValueComparer");

    [ConditionalFact]
    public void Can_map_nullable_double()
        => Can_map_scalar_by_clr_type<double?, JsonDoubleReaderWriter>(1.33, JTokenType.Float, "1.33", "DefaultDoubleValueComparer");

    [ConditionalFact]
    public void Can_map_nullable_bool()
        => Can_map_scalar_by_clr_type<bool?, JsonBoolReaderWriter>(true, JTokenType.Boolean, "true");

    [ConditionalFact]
    public void Can_map_nullable_DateOnly()
        => Can_map_scalar_by_clr_type<DateOnly?, JsonDateOnlyReaderWriter>(new DateOnly(2003, 12, 25), JTokenType.String, "\"2003-12-25\"");

    [ConditionalFact]
    public void Can_map_nullable_TimeOnly()
        => Can_map_scalar_by_clr_type<TimeOnly?, JsonTimeOnlyReaderWriter>(
            new TimeOnly(20, 19, 12, 254), JTokenType.String, "\"20:19:12.254\"");

    [ConditionalFact]
    public void Can_map_nullable_DateTime()
        => Can_map_scalar_by_clr_type<DateTime?, JsonDateTimeReaderWriter>(
            new DateTime(2003, 12, 25, 20, 19, 12, 254), JTokenType.Date, "\"2003-12-25T20:19:12.254\"");

    [ConditionalFact]
    public void Can_map_nullable_DateTimeOffset()
        => Can_map_scalar_by_clr_type<DateTimeOffset?, JsonDateTimeOffsetReaderWriter>(
            new DateTimeOffset(2003, 12, 25, 20, 19, 12, 254, new TimeSpan(4, 30, 0)), JTokenType.Date, "\"2003-12-25T20:19:12.254+04:30\"",
            "DefaultDateTimeOffsetValueComparer");

    [ConditionalFact]
    public void Can_map_nullable_TimeSpan()
        => Can_map_scalar_by_clr_type<TimeSpan?, JsonTimeSpanReaderWriter>(new TimeSpan(2, 3, 4, 5), JTokenType.TimeSpan, "\"2.03:04:05\"");

    [ConditionalFact]
    public void Can_map_nullable_string()
        => Can_map_scalar_by_clr_type<string?, JsonStringReaderWriter>("Hello", JTokenType.String, "\"Hello\"");

    private void Can_map_scalar_by_clr_type<T, TReader>(T value, JTokenType tokenType, string jsonValue, string? comparerType = null)
    {
        var unwrappedType = UnwrapNullableType(typeof(T));
        comparerType ??= $"DefaultValueComparer<{unwrappedType.ShortDisplayName()}>";
        var mapping = (CosmosTypeMapping)GetTypeMapping(typeof(T));

        Assert.Null(mapping.ElementTypeMapping);
        Assert.Same(unwrappedType, mapping.ClrType);
        //Assert.Null(mapping.Converter);
        Assert.Same(mapping.GetType(), mapping.Clone().GetType());
        Assert.Equal(comparerType, mapping.Comparer.GetType().ShortDisplayName());
        Assert.Equal(comparerType, mapping.KeyComparer.GetType().ShortDisplayName());
        //Assert.Equal(comparerType, mapping.ProviderValueComparer.GetType().ShortDisplayName());
        Assert.IsType<TReader>(mapping.JsonValueReaderWriter);
        Assert.Equal(jsonValue, mapping.GenerateConstant(value));
        var token = mapping.GenerateJToken(value)!;
        Assert.Equal(tokenType, token.Type);

        if (token.Type != JTokenType.String)
        {
            Assert.Equal(value, token.Value<T>());
        }
    }

    [ConditionalFact]
    public void Can_map_byte_array()
    {
        var value = new byte[] { 1, 2, 3, 4, 5 };
        var mapping = (CosmosTypeMapping)GetTypeMapping(typeof(byte[]));

        Assert.Null(mapping.ElementTypeMapping);
        Assert.Same(typeof(byte[]), mapping.ClrType);
        Assert.IsType<BytesToStringConverter>(mapping.Converter);
        Assert.Same(mapping.GetType(), mapping.Clone().GetType());
        Assert.Equal("ValueComparer<byte[]>", mapping.Comparer.GetType().ShortDisplayName());
        Assert.Equal("ValueComparer<byte[]>", mapping.KeyComparer.GetType().ShortDisplayName());
        Assert.Equal("DefaultValueComparer<string>", mapping.ProviderValueComparer.GetType().ShortDisplayName());
        Assert.IsType<JsonConvertedValueReaderWriter<byte[], string>>(mapping.JsonValueReaderWriter);
        Assert.Equal("\"AQIDBAU=\"", mapping.GenerateConstant(value));
        var token = mapping.GenerateJToken(value)!;
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("AQIDBAU=", token.Value<string>());
    }

    [ConditionalFact]
    public void Can_map_sbyte_array()
        => Can_map_collection_by_clr_type<sbyte[], sbyte, JsonCollectionOfStructsReaderWriter<sbyte[], sbyte>>(
            [1, -2, 3, -4, 5], "[1,-2,3,-4,5]", "ListOfValueTypesComparer<sbyte[], sbyte>", "ValueComparer<sbyte[]>",
            "ValueComparer<sbyte[]>");

    [ConditionalFact]
    public void Can_map_short_array()
        => Can_map_collection_by_clr_type<short[], short, JsonCollectionOfStructsReaderWriter<short[], short>>(
            [1, -2, 3, -4, 5], "[1,-2,3,-4,5]", "ListOfValueTypesComparer<short[], short>", "ValueComparer<short[]>",
            "ValueComparer<short[]>");

    [ConditionalFact]
    public void Can_map_int_array()
        => Can_map_collection_by_clr_type<int[], int, JsonCollectionOfStructsReaderWriter<int[], int>>(
            [1, -2, 3, -4, 5], "[1,-2,3,-4,5]", "ListOfValueTypesComparer<int[], int>", "ValueComparer<int[]>", "ValueComparer<int[]>");

    [ConditionalFact]
    public void Can_map_long_array()
        => Can_map_collection_by_clr_type<long[], long, JsonCollectionOfStructsReaderWriter<long[], long>>(
            [1, -2, 3, -4, 5], "[1,-2,3,-4,5]", "ListOfValueTypesComparer<long[], long>", "ValueComparer<long[]>", "ValueComparer<long[]>");

    [ConditionalFact]
    public void Can_map_ushort_array()
        => Can_map_collection_by_clr_type<ushort[], ushort, JsonCollectionOfStructsReaderWriter<ushort[], ushort>>(
            [1, 2, 3, 4, 5], "[1,2,3,4,5]", "ListOfValueTypesComparer<ushort[], ushort>", "ValueComparer<ushort[]>",
            "ValueComparer<ushort[]>");

    [ConditionalFact]
    public void Can_map_uint_array()
        => Can_map_collection_by_clr_type<uint[], uint, JsonCollectionOfStructsReaderWriter<uint[], uint>>(
            [1, 2, 3, 4, 5], "[1,2,3,4,5]", "ListOfValueTypesComparer<uint[], uint>", "ValueComparer<uint[]>", "ValueComparer<uint[]>");

    [ConditionalFact]
    public void Can_map_ulong_array()
        => Can_map_collection_by_clr_type<ulong[], ulong, JsonCollectionOfStructsReaderWriter<ulong[], ulong>>(
            [1, 2, 3, 4, 5], "[1,2,3,4,5]", "ListOfValueTypesComparer<ulong[], ulong>", "ValueComparer<ulong[]>", "ValueComparer<ulong[]>");

    [ConditionalFact]
    public void Can_map_sbyte_list()
        => Can_map_collection_by_clr_type<List<sbyte>, sbyte, JsonCollectionOfStructsReaderWriter<List<sbyte>, sbyte>>(
            [1, -2, 3, -4, 5], "[1,-2,3,-4,5]", "ListOfValueTypesComparer<List<sbyte>, sbyte>", "ValueComparer<List<sbyte>>",
            "ValueComparer<List<sbyte>>");

    [ConditionalFact]
    public void Can_map_short_list()
        => Can_map_collection_by_clr_type<List<short>, short, JsonCollectionOfStructsReaderWriter<List<short>, short>>(
            [1, -2, 3, -4, 5], "[1,-2,3,-4,5]", "ListOfValueTypesComparer<List<short>, short>", "ValueComparer<List<short>>",
            "ValueComparer<List<short>>");

    [ConditionalFact]
    public void Can_map_int_list()
        => Can_map_collection_by_clr_type<List<int>, int, JsonCollectionOfStructsReaderWriter<List<int>, int>>(
            [1, -2, 3, -4, 5], "[1,-2,3,-4,5]", "ListOfValueTypesComparer<List<int>, int>", "ValueComparer<List<int>>",
            "ValueComparer<List<int>>");

    [ConditionalFact]
    public void Can_map_long_list()
        => Can_map_collection_by_clr_type<List<long>, long, JsonCollectionOfStructsReaderWriter<List<long>, long>>(
            [1, -2, 3, -4, 5], "[1,-2,3,-4,5]", "ListOfValueTypesComparer<List<long>, long>", "ValueComparer<List<long>>",
            "ValueComparer<List<long>>");

    [ConditionalFact]
    public void Can_map_byte_list()
        => Can_map_collection_by_clr_type<List<byte>, byte, JsonCollectionOfStructsReaderWriter<List<byte>, byte>>(
            [1, 2, 3, 4, 5], "[1,2,3,4,5]", "ListOfValueTypesComparer<List<byte>, byte>", "ValueComparer<List<byte>>",
            "ValueComparer<List<byte>>");

    [ConditionalFact]
    public void Can_map_ushort_list()
        => Can_map_collection_by_clr_type<List<ushort>, ushort, JsonCollectionOfStructsReaderWriter<List<ushort>, ushort>>(
            [1, 2, 3, 4, 5], "[1,2,3,4,5]", "ListOfValueTypesComparer<List<ushort>, ushort>", "ValueComparer<List<ushort>>",
            "ValueComparer<List<ushort>>");

    [ConditionalFact]
    public void Can_map_uint_list()
        => Can_map_collection_by_clr_type<List<uint>, uint, JsonCollectionOfStructsReaderWriter<List<uint>, uint>>(
            [1, 2, 3, 4, 5], "[1,2,3,4,5]", "ListOfValueTypesComparer<List<uint>, uint>", "ValueComparer<List<uint>>",
            "ValueComparer<List<uint>>");

    [ConditionalFact]
    public void Can_map_ulong_list()
        => Can_map_collection_by_clr_type<List<ulong>, ulong, JsonCollectionOfStructsReaderWriter<List<ulong>, ulong>>(
            [1, 2, 3, 4, 5], "[1,2,3,4,5]", "ListOfValueTypesComparer<List<ulong>, ulong>", "ValueComparer<List<ulong>>",
            "ValueComparer<List<ulong>>");

    private void Can_map_collection_by_clr_type<TCollection, TElement, TReader>(
        TCollection value,
        string jsonValue,
        string comparerType,
        string keyComparerType,
        string providerComparerType)
    {
        var unwrappedType = UnwrapNullableType(typeof(TCollection));
        var mapping = (CosmosTypeMapping)GetTypeMapping(typeof(TCollection));

        Assert.Same(typeof(TElement), mapping.ElementTypeMapping!.ClrType);
        Assert.Same(unwrappedType, mapping.ClrType);
        Assert.Null(mapping.Converter);
        Assert.Same(mapping.GetType(), mapping.Clone().GetType());
        Assert.Equal(comparerType, mapping.Comparer.GetType().ShortDisplayName());
        Assert.Equal(keyComparerType, mapping.KeyComparer.GetType().ShortDisplayName());
        Assert.Equal(providerComparerType, mapping.ProviderValueComparer.GetType().ShortDisplayName());
        Assert.IsType<TReader>(mapping.JsonValueReaderWriter);
        Assert.Equal(jsonValue, mapping.GenerateConstant(value));
        var token = mapping.GenerateJToken(value)!;
        Assert.Equal(JTokenType.Array, token.Type);
        Assert.Equal((IEnumerable<TElement?>)value!, ((JArray)token).Values<TElement?>().ToList());
    }

    [ConditionalFact]
    public void Can_map_GUIDs()
    {
        var value = new Guid("39E5DEBB-8826-4996-B68D-F9C05E687A86");
        var mapping = (CosmosTypeMapping)GetTypeMapping(typeof(Guid));

        Assert.Null(mapping.ElementTypeMapping);
        Assert.Same(typeof(Guid), mapping.ClrType);
        Assert.IsType<GuidToStringConverter>(mapping.Converter);
        Assert.Same(mapping.GetType(), mapping.Clone().GetType());
        Assert.Equal("DefaultValueComparer<Guid>", mapping.Comparer.GetType().ShortDisplayName());
        Assert.Equal("DefaultValueComparer<Guid>", mapping.KeyComparer.GetType().ShortDisplayName());
        Assert.Equal("DefaultValueComparer<string>", mapping.ProviderValueComparer.GetType().ShortDisplayName());
        Assert.IsType<JsonConvertedValueReaderWriter<Guid, string>>(mapping.JsonValueReaderWriter);

        Assert.Equal("\"39e5debb-8826-4996-b68d-f9c05e687a86\"", mapping.GenerateConstant(value));
        var token = mapping.GenerateJToken(value)!;
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("39e5debb-8826-4996-b68d-f9c05e687a86", token.Value<string>());
    }

    [ConditionalFact]
    public void Does_not_map_Memory_types_without_converter()
    {
        Assert.Null(GetTypeMapping(typeof(Memory<float>)));
        Assert.Null(GetTypeMapping(typeof(Memory<float>?)));
    }

    private static Type UnwrapNullableType(Type type)
        => Nullable.GetUnderlyingType(type) ?? type;

    [ConditionalFact]
    public void Plugins_can_override_builtin_mappings()
    {
        var typeMappingSource = new CosmosTypeMappingSource(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>() with
            {
                Plugins = new[] { new FakeTypeMappingSourcePlugin() }
            });

        Assert.Same(typeof(Random), typeMappingSource.FindMapping(typeof(int))!.ClrType);
    }

    private class FakeTypeMappingSourcePlugin : ITypeMappingSourcePlugin
    {
        public CoreTypeMapping FindMapping(in TypeMappingInfo mappingInfo)
            => new CosmosTypeMapping(typeof(Random));
    }

    protected ITypeMappingSource CreateTypeMappingSource(IModel model)
    {
        var typeMappingSource = new CosmosTypeMappingSource(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>());

        model.ModelDependencies = new RuntimeModelDependencies(typeMappingSource, null!, null!);

        return typeMappingSource;
    }

    protected ModelBuilder CreateModelBuilder()
        => CosmosTestHelpers.Instance.CreateConventionBuilder();

    protected IMutableEntityType CreateEntityType<TEntity>()
    {
        var builder = CreateModelBuilder();

        builder.Entity<MyType>().Property(e => e.Id);

        return builder.Model.FindEntityType(typeof(TEntity))!;
    }

    protected IModel CreateModel()
        => CreateEntityType<MyType>().Model.FinalizeModel();

    protected CoreTypeMapping GetTypeMapping(Type propertyType, bool? nullable = null)
    {
        var modelBuilder = CreateModelBuilder();
        var entityType = modelBuilder.Entity<MyType>();
        entityType.Property(e => e.Id);
        var property = entityType.Property(propertyType, "MyProp").Metadata;

        if (nullable.HasValue)
        {
            property.IsNullable = nullable.Value;
        }

        var model = modelBuilder.Model.FinalizeModel();

        return CreateTypeMappingSource(model).FindMapping(model.FindEntityType(typeof(MyType))!.FindProperty(property.Name)!)!;
    }

    private class MyType
    {
        public Guid Id { get; set; }
    }
}
