// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Storage;

public class RelationalTypeMappingSourceTest : RelationalTypeMappingSourceTestBase
{
    [ConditionalFact]
    public void Does_simple_mapping_from_CLR_type()
        => Assert.Equal("default_int_mapping", GetTypeMapping(typeof(int)).StoreType);

    [ConditionalFact]
    public void Does_simple_mapping_from_nullable_CLR_type()
        => Assert.Equal("default_int_mapping", GetTypeMapping(typeof(int?)).StoreType);

    [ConditionalFact]
    public void Does_type_mapping_from_string_with_no_MaxLength()
    {
        var mapping = GetTypeMapping(typeof(string));

        Assert.Equal("just_string(max)", mapping.StoreType);
    }

    [ConditionalFact]
    public void Does_type_mapping_from_string_with_MaxLength()
    {
        var mapping = GetTypeMapping(typeof(string), maxLength: 666);

        Assert.Equal("just_string(666)", mapping.StoreType);
        Assert.Equal(666, mapping.Size);
    }

    [ConditionalFact]
    public void Does_type_mapping_from_string_with_MaxLength_greater_than_unbounded_max()
    {
        var mapping = GetTypeMapping(typeof(string), maxLength: 2020);

        Assert.Equal("just_string(2020)", mapping.StoreType);
        Assert.Equal(2020, mapping.Size);
    }

    [ConditionalFact]
    public void Does_type_mapping_from_btye_array_with_no_MaxLength()
    {
        var mapping = GetTypeMapping(typeof(byte[]));

        Assert.Equal("just_binary(max)", mapping.StoreType);
    }

    [ConditionalFact]
    public void Does_type_mapping_from_btye_array_with_MaxLength()
    {
        var mapping = GetTypeMapping(typeof(byte[]), maxLength: 777);

        Assert.Equal("just_binary(777)", mapping.StoreType);
        Assert.Equal(777, mapping.Size);
    }

    [ConditionalFact]
    public void Does_type_mapping_from_btye_array_greater_than_unbounded_max()
    {
        var mapping = GetTypeMapping(typeof(byte[]), maxLength: 2020);

        Assert.Equal("just_binary(2020)", mapping.StoreType);
    }

    [ConditionalFact]
    public void Does_simple_mapping_from_name()
        => Assert.Equal("int", GetTypeMapping(typeof(int), storeTypeName: "int").StoreType);

    [ConditionalFact]
    public void Does_default_mapping_for_unrecognized_store_type()
        => Assert.Equal("int", GetTypeMapping(typeof(int), storeTypeName: "int").StoreType);

    [ConditionalFact]
    public void Does_type_mapping_from_named_string_with_no_MaxLength()
    {
        var mapping = GetTypeMapping(typeof(string), storeTypeName: "some_string(max)");

        Assert.Equal("some_string(max)", mapping.StoreType);
    }

    [ConditionalFact]
    public void Does_type_mapping_from_named_string_with_MaxLength()
    {
        var mapping = GetTypeMapping(typeof(string), storeTypeName: "some_string(666)");

        Assert.Equal("(666)some_string", mapping.StoreType);
        Assert.Equal(666, mapping.Size);
    }

    [ConditionalFact]
    public void Does_type_mapping_from_named_binary_with_no_MaxLength()
    {
        var mapping = GetTypeMapping(typeof(byte[]), storeTypeName: "some_binary(max)");

        Assert.Equal("some_binary(max)", mapping.StoreType);
    }

    [ConditionalFact]
    public void Key_with_store_type_is_picked_up_by_FK()
    {
        var model = CreateModel();
        var mapper = CreateRelationalTypeMappingSource(model);

        Assert.Equal(
            "money",
            GetMapping(mapper, model.FindEntityType(typeof(MyType)).FindProperty("Id")).StoreType);

        Assert.Equal(
            "money",
            GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType1)).FindProperty("Relationship1Id")).StoreType);
    }

    [ConditionalFact]
    public void Does_default_type_mapping_from_decimal()
    {
        var model = CreateModel();
        var mapper = CreateRelationalTypeMappingSource(model);

        Assert.Equal(
            "default_decimal_mapping",
            GetMapping(mapper, model.FindEntityType(typeof(MyPrecisionType)).FindProperty("Id")).StoreType);
    }

    [ConditionalFact]
    public void Does_type_mapping_from_decimal_with_precision_only()
    {
        var model = CreateModel();
        var mapper = CreateRelationalTypeMappingSource(model);

        Assert.Equal(
            "decimal_mapping(16)",
            GetMapping(mapper, model.FindEntityType(typeof(MyPrecisionType)).FindProperty("PrecisionOnly")).StoreType);
    }

    [ConditionalFact]
    public void Does_type_mapping_from_decimal_with_precision_and_scale()
    {
        var model = CreateModel();
        var mapper = CreateRelationalTypeMappingSource(model);

        Assert.Equal(
            "decimal_mapping(18,7)",
            GetMapping(mapper, model.FindEntityType(typeof(MyPrecisionType)).FindProperty("PrecisionAndScale")).StoreType);
    }

    [ConditionalFact]
    public void Does_type_mapping_from_string_with_configuration()
    {
        var mapping = GetTypeMapping(
            typeof(string),
            maxLength: 666,
            precision: 66,
            scale: 6,
            unicode: false,
            fixedLength: true,
            useConfiguration: true);

        Assert.Equal("ansi_string_fixed(666)", mapping.StoreType);
        Assert.Equal("ansi_string_fixed", mapping.StoreTypeNameBase);
        Assert.Equal(666, mapping.Size);
        Assert.Null(mapping.Precision);
        Assert.Null(mapping.Scale);
        Assert.False(mapping.IsUnicode);
        Assert.True(mapping.IsFixedLength);
    }

    [ConditionalFact]
    public void Does_type_mapping_from_string_type_with_configuration()
    {
        var mapping = GetTypeMapping(
            typeof(string),
            storeTypeName: "ansi_string_fixed(666)",
            useConfiguration: true);

        Assert.Equal("ansi_string_fixed(666)", mapping.StoreType);
        Assert.Equal("ansi_string_fixed", mapping.StoreTypeNameBase);
        Assert.Equal(666, mapping.Size);
        Assert.Null(mapping.Precision);
        Assert.Null(mapping.Scale);
        Assert.False(mapping.IsUnicode);
    }

    [ConditionalFact]
    public void Does_type_mapping_from_decimal_with_configuration()
    {
        var mapping = GetTypeMapping(
            typeof(decimal),
            maxLength: 666,
            precision: 66,
            scale: 6,
            unicode: false,
            fixedLength: true,
            useConfiguration: true);

        Assert.Equal("decimal_mapping(66,6)", mapping.StoreType);
        Assert.Equal("decimal_mapping", mapping.StoreTypeNameBase);
        Assert.Null(mapping.Size);
        Assert.Equal(66, mapping.Precision);
        Assert.Equal(6, mapping.Scale);
        Assert.False(mapping.IsUnicode);
        Assert.False(mapping.IsFixedLength);
    }

    [ConditionalFact]
    public void Does_type_mapping_from_decimal_type_with_configuration()
    {
        var mapping = GetTypeMapping(
            typeof(decimal),
            storeTypeName: "decimal_mapping(66,6)",
            useConfiguration: true);

        Assert.Equal("decimal_mapping(66,6)", mapping.StoreType);
        Assert.Equal("decimal_mapping", mapping.StoreTypeNameBase);
        Assert.Null(mapping.Size);
        Assert.Equal(66, mapping.Precision);
        Assert.Equal(6, mapping.Scale);
        Assert.False(mapping.IsUnicode);
        Assert.False(mapping.IsFixedLength);
    }

    [ConditionalFact]
    public void StoreTypeNameBase_is_trimmed()
    {
        var mapping = GetTypeMapping(
            typeof(string),
            storeTypeName: "ansi_string_fixed (666)",
            useConfiguration: true);

        Assert.Equal("ansi_string_fixed (666)", mapping.StoreType);
        Assert.Equal("ansi_string_fixed", mapping.StoreTypeNameBase);
        Assert.Equal(666, mapping.Size);
        Assert.False(mapping.IsUnicode);
        Assert.False(mapping.IsFixedLength);
    }

    protected override IRelationalTypeMappingSource CreateRelationalTypeMappingSource(IModel model)
        => new TestRelationalTypeMappingSource(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());

    public RelationalTypeMapping GetMapping(Type type)
        => CreateRelationalTypeMappingSource(CreateModel()).FindMapping(type);

    public RelationalTypeMapping GetMapping(IProperty property)
        => CreateRelationalTypeMappingSource(CreateModel()).FindMapping(property);

    [ConditionalFact]
    public void String_key_with_max_fixed_length_is_picked_up_by_FK()
    {
        var model = CreateModel();
        var mapper = CreateRelationalTypeMappingSource(model);

        Assert.Equal(
            "just_string_fixed(200)",
            GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType1)).FindProperty("Id")).StoreType);

        Assert.Equal(
            "just_string_fixed(200)",
            GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType2)).FindProperty("Relationship1Id")).StoreType);
    }

    [ConditionalFact]
    public void Binary_key_with_max_fixed_length_is_picked_up_by_FK()
    {
        var model = CreateModel();
        var mapper = CreateRelationalTypeMappingSource(model);

        Assert.Equal(
            "just_binary_fixed(100)",
            GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType2)).FindProperty("Id")).StoreType);

        Assert.Equal(
            "just_binary_fixed(100)",
            GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType3)).FindProperty("Relationship1Id")).StoreType);
    }

    [ConditionalFact]
    public void String_key_with_unicode_is_picked_up_by_FK()
    {
        var model = CreateModel();
        var mapper = CreateRelationalTypeMappingSource(model);

        Assert.Equal(
            "ansi_string(900)",
            GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType3)).FindProperty("Id")).StoreType);

        Assert.Equal(
            "ansi_string(900)",
            GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType4)).FindProperty("Relationship1Id")).StoreType);
    }

    [ConditionalFact]
    public void Key_store_type_is_preferred_if_specified()
    {
        var model = CreateModel();
        var mapper = CreateRelationalTypeMappingSource(model);

        Assert.Equal(
            "money",
            GetMapping(mapper, model.FindEntityType(typeof(MyType)).FindProperty("Id")).StoreType);

        Assert.Equal(
            "decimal_mapping(6,1)",
            GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType1)).FindProperty("Relationship2Id")).StoreType);
    }

    [ConditionalFact]
    public void String_FK_max_length_is_preferred_if_specified()
    {
        var model = CreateModel();
        var mapper = CreateRelationalTypeMappingSource(model);

        Assert.Equal(
            "just_string_fixed(200)",
            GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType1)).FindProperty("Id")).StoreType);

        Assert.Equal(
            "just_string_fixed(787)",
            GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType2)).FindProperty("Relationship2Id")).StoreType);
    }

    [ConditionalFact]
    public void Binary_FK_max_length_is_preferred_if_specified()
    {
        var model = CreateModel();
        var mapper = CreateRelationalTypeMappingSource(model);

        Assert.Equal(
            "just_binary_fixed(100)",
            GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType2)).FindProperty("Id")).StoreType);

        Assert.Equal(
            "just_binary_fixed(767)",
            GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType3)).FindProperty("Relationship2Id")).StoreType);
    }

    [ConditionalFact]
    public void String_FK_unicode_is_preferred_if_specified()
    {
        var model = CreateModel();
        var mapper = CreateRelationalTypeMappingSource(model);

        Assert.Equal(
            "ansi_string(900)",
            GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType3)).FindProperty("Id")).StoreType);

        Assert.Equal(
            "just_string(450)",
            GetMapping(mapper, model.FindEntityType(typeof(MyRelatedType4)).FindProperty("Relationship2Id")).StoreType);
    }

    public static RelationalTypeMapping GetMapping(
        IRelationalTypeMappingSource typeMappingSource,
        IProperty property)
        => typeMappingSource.FindMapping(property);

    protected override ModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder> configureConventions = null)
        => FakeRelationalTestHelpers.Instance.CreateConventionBuilder(configureConventions: configureConventions);
}
