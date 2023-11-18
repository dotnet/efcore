// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

public class InternalPropertyBuilderTest
{
    [ConditionalFact]
    public void Property_added_by_name_is_non_shadow_if_matches_Clr_property()
    {
        var model = new Model();
        var modelBuilder = new InternalModelBuilder(model);
        var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var builder = entityBuilder.Property(Customer.NameProperty.Name, ConfigurationSource.Convention);
        var property = builder.Metadata;

        Assert.Equal(typeof(string), property.ClrType);
        Assert.False(property.IsShadowProperty());
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_ConcurrencyToken()
    {
        var builder = CreateInternalPropertyBuilder();
        var metadata = builder.Metadata;

        Assert.NotNull(builder.IsConcurrencyToken(true, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.IsConcurrencyToken(false, ConfigurationSource.DataAnnotation));

        Assert.False(metadata.IsConcurrencyToken);

        Assert.Null(builder.IsConcurrencyToken(true, ConfigurationSource.Convention));
        Assert.False(metadata.IsConcurrencyToken);
    }

    [ConditionalFact]
    public void Can_only_override_existing_ConcurrencyToken_value_explicitly()
    {
        var metadata = CreateProperty();
        Assert.Null(metadata.GetIsConcurrencyTokenConfigurationSource());
        metadata.IsConcurrencyToken = true;
        var builder = metadata.Builder;

        Assert.Equal(ConfigurationSource.Explicit, metadata.GetIsConcurrencyTokenConfigurationSource());
        Assert.NotNull(builder.IsConcurrencyToken(true, ConfigurationSource.DataAnnotation));
        Assert.Null(builder.IsConcurrencyToken(false, ConfigurationSource.DataAnnotation));

        Assert.True(metadata.IsConcurrencyToken);

        Assert.NotNull(builder.IsConcurrencyToken(false, ConfigurationSource.Explicit));
        Assert.False(metadata.IsConcurrencyToken);
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_ValueGenerated()
    {
        var builder = CreateInternalPropertyBuilder();
        var metadata = builder.Metadata;

        Assert.NotNull(builder.ValueGenerated(ValueGenerated.OnAddOrUpdate, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.ValueGenerated(ValueGenerated.Never, ConfigurationSource.DataAnnotation));

        Assert.Equal(ValueGenerated.Never, metadata.ValueGenerated);

        Assert.Null(builder.ValueGenerated(ValueGenerated.OnAddOrUpdate, ConfigurationSource.Convention));
        Assert.Equal(ValueGenerated.Never, metadata.ValueGenerated);
    }

    [ConditionalFact]
    public void Can_only_override_existing_ValueGenerated_value_explicitly()
    {
        var metadata = CreateProperty();
        Assert.Null(metadata.GetValueGeneratedConfigurationSource());
        metadata.ValueGenerated = ValueGenerated.OnAddOrUpdate;
        var builder = metadata.Builder;

        Assert.Equal(ConfigurationSource.Explicit, metadata.GetValueGeneratedConfigurationSource());
        Assert.NotNull(builder.ValueGenerated(ValueGenerated.OnAddOrUpdate, ConfigurationSource.DataAnnotation));
        Assert.Null(builder.ValueGenerated(ValueGenerated.Never, ConfigurationSource.DataAnnotation));

        Assert.Equal(ValueGenerated.OnAddOrUpdate, metadata.ValueGenerated);

        Assert.NotNull(builder.ValueGenerated(ValueGenerated.Never, ConfigurationSource.Explicit));
        Assert.Equal(ValueGenerated.Never, metadata.ValueGenerated);
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_MaxLength()
    {
        var builder = CreateInternalPropertyBuilder();
        var metadata = builder.Metadata;

        Assert.NotNull(builder.HasMaxLength(1, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.HasMaxLength(2, ConfigurationSource.DataAnnotation));

        Assert.Equal(2, metadata.GetMaxLength().Value);

        Assert.Null(builder.HasMaxLength(1, ConfigurationSource.Convention));
        Assert.Equal(2, metadata.GetMaxLength().Value);
    }

    [ConditionalFact]
    public void Can_only_override_existing_MaxLength_value_explicitly()
    {
        var metadata = CreateProperty();
        metadata.SetMaxLength(1, ConfigurationSource.Explicit);
        var builder = metadata.Builder;

        Assert.NotNull(builder.HasMaxLength(1, ConfigurationSource.DataAnnotation));
        Assert.Null(builder.HasMaxLength(2, ConfigurationSource.DataAnnotation));

        Assert.Equal(1, metadata.GetMaxLength().Value);

        Assert.NotNull(builder.HasMaxLength(2, ConfigurationSource.Explicit));
        Assert.Equal(2, metadata.GetMaxLength().Value);
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_sentinel()
    {
        var builder = CreateInternalPropertyBuilder();
        var metadata = builder.Metadata;

        Assert.NotNull(builder.HasSentinel(1, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.HasSentinel(2, ConfigurationSource.DataAnnotation));

        Assert.Equal("2", metadata.Sentinel);

        Assert.Null(builder.HasSentinel(1, ConfigurationSource.Convention));
        Assert.Equal("2", metadata.Sentinel);
    }

    [ConditionalFact]
    public void Can_only_override_existing_sentinel_value_explicitly()
    {
        var metadata = CreateProperty();
        metadata.SetSentinel(1, ConfigurationSource.Explicit);
        var builder = metadata.Builder;

        Assert.NotNull(builder.HasSentinel("1", ConfigurationSource.DataAnnotation));
        Assert.Null(builder.HasSentinel("2", ConfigurationSource.DataAnnotation));

        Assert.Equal("1", metadata.Sentinel);

        Assert.NotNull(builder.HasSentinel("2", ConfigurationSource.Explicit));
        Assert.Equal("2", metadata.Sentinel);
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_Precision()
    {
        var builder = CreateInternalPropertyBuilder();
        var metadata = builder.Metadata;

        Assert.NotNull(builder.HasPrecision(1, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.HasPrecision(2, ConfigurationSource.DataAnnotation));

        Assert.Equal(2, metadata.GetPrecision().Value);

        Assert.Null(builder.HasPrecision(1, ConfigurationSource.Convention));
        Assert.Equal(2, metadata.GetPrecision().Value);
    }

    [ConditionalFact]
    public void Can_only_override_existing_Precision_value_explicitly()
    {
        var metadata = CreateProperty();
        metadata.SetPrecision(1, ConfigurationSource.Explicit);
        var builder = metadata.Builder;

        Assert.NotNull(builder.HasPrecision(1, ConfigurationSource.DataAnnotation));
        Assert.Null(builder.HasPrecision(2, ConfigurationSource.DataAnnotation));

        Assert.Equal(1, metadata.GetPrecision().Value);

        Assert.NotNull(builder.HasPrecision(2, ConfigurationSource.Explicit));
        Assert.Equal(2, metadata.GetPrecision().Value);
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_Scale()
    {
        var builder = CreateInternalPropertyBuilder();
        var metadata = builder.Metadata;

        Assert.NotNull(builder.HasScale(1, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.HasScale(2, ConfigurationSource.DataAnnotation));

        Assert.Equal(2, metadata.GetScale().Value);

        Assert.Null(builder.HasScale(1, ConfigurationSource.Convention));
        Assert.Equal(2, metadata.GetScale().Value);
    }

    [ConditionalFact]
    public void Can_only_override_existing_Scale_value_explicitly()
    {
        var metadata = CreateProperty();
        metadata.SetScale(1, ConfigurationSource.Explicit);
        var builder = metadata.Builder;

        Assert.NotNull(builder.HasScale(1, ConfigurationSource.DataAnnotation));
        Assert.Null(builder.HasScale(2, ConfigurationSource.DataAnnotation));

        Assert.Equal(1, metadata.GetScale().Value);

        Assert.NotNull(builder.HasScale(2, ConfigurationSource.Explicit));
        Assert.Equal(2, metadata.GetScale().Value);
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_CustomValueGenerator_factory()
    {
        var builder = CreateInternalPropertyBuilder();
        var metadata = builder.Metadata;

        Assert.NotNull(builder.HasValueGenerator((p, e) => new CustomValueGenerator1(), ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.HasValueGenerator((p, e) => new CustomValueGenerator2(), ConfigurationSource.DataAnnotation));

        Assert.IsType<CustomValueGenerator2>(metadata.GetValueGeneratorFactory()(null, null));
        Assert.True(metadata.RequiresValueGenerator());

        Assert.Null(builder.HasValueGenerator((p, e) => new CustomValueGenerator1(), ConfigurationSource.Convention));
        Assert.IsType<CustomValueGenerator2>(metadata.GetValueGeneratorFactory()(null, null));
        Assert.True(metadata.RequiresValueGenerator());

        Assert.Null(builder.HasValueGeneratorFactory(typeof(CustomValueGeneratorFactory), ConfigurationSource.Convention));
        Assert.IsType<CustomValueGenerator2>(metadata.GetValueGeneratorFactory()(null, null));

        Assert.NotNull(builder.HasValueGeneratorFactory(typeof(CustomValueGeneratorFactory), ConfigurationSource.DataAnnotation));
        Assert.IsType<CustomValueGenerator1>(metadata.GetValueGeneratorFactory()(null, null));
        Assert.True(metadata.RequiresValueGenerator());

        Assert.NotNull(builder.HasValueGeneratorFactory(null, ConfigurationSource.DataAnnotation));
        Assert.Null(metadata.GetValueGeneratorFactory());
        Assert.False(metadata.RequiresValueGenerator());
        Assert.Null(metadata[CoreAnnotationNames.ValueGeneratorFactory]);
        Assert.Null(metadata[CoreAnnotationNames.ValueGeneratorFactoryType]);
    }

    [ConditionalFact]
    public void Can_only_override_existing_CustomValueGenerator_factory_explicitly()
    {
        ValueGenerator factory(IReadOnlyProperty p, ITypeBase t)
            => new CustomValueGenerator1();

        var metadata = CreateProperty();
        metadata.SetValueGeneratorFactory(factory, ConfigurationSource.Explicit);
        var builder = metadata.Builder;

        Assert.NotNull(builder.HasValueGenerator(factory, ConfigurationSource.DataAnnotation));
        Assert.Null(builder.HasValueGenerator((p, e) => new CustomValueGenerator2(), ConfigurationSource.DataAnnotation));

        Assert.IsType<CustomValueGenerator1>(metadata.GetValueGeneratorFactory()(null, null));
        Assert.True(metadata.RequiresValueGenerator());

        Assert.NotNull(builder.HasValueGenerator((p, e) => new CustomValueGenerator2(), ConfigurationSource.Explicit));
        Assert.IsType<CustomValueGenerator2>(metadata.GetValueGeneratorFactory()(null, null));
        Assert.True(metadata.RequiresValueGenerator());
    }

    [ConditionalFact]
    public void Can_clear_CustomValueGenerator_factory()
    {
        var metadata = CreateProperty();
        var builder = metadata.Builder;

        Assert.NotNull(builder.HasValueGenerator((p, e) => new CustomValueGenerator1(), ConfigurationSource.DataAnnotation));

        Assert.IsType<CustomValueGenerator1>(metadata.GetValueGeneratorFactory()(null, null));
        Assert.Equal(ValueGenerated.Never, metadata.ValueGenerated);
        Assert.True(metadata.RequiresValueGenerator());

        Assert.Null(
            builder.HasValueGenerator(
                (Func<IReadOnlyProperty, ITypeBase, ValueGenerator>)null, ConfigurationSource.Convention));

        Assert.IsType<CustomValueGenerator1>(metadata.GetValueGeneratorFactory()(null, null));
        Assert.Equal(ValueGenerated.Never, metadata.ValueGenerated);
        Assert.True(metadata.RequiresValueGenerator());

        Assert.NotNull(
            builder.HasValueGenerator(
                (Func<IReadOnlyProperty, ITypeBase, ValueGenerator>)null, ConfigurationSource.Explicit));

        Assert.Null(metadata.GetValueGeneratorFactory());
        Assert.Equal(ValueGenerated.Never, metadata.ValueGenerated);
        Assert.False(metadata.RequiresValueGenerator());
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_CustomValueGenerator_type()
    {
        var builder = CreateInternalPropertyBuilder();
        var metadata = builder.Metadata;

        Assert.NotNull(builder.HasValueGenerator(typeof(CustomValueGenerator1), ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.HasValueGenerator(typeof(CustomValueGenerator2), ConfigurationSource.DataAnnotation));

        Assert.IsType<CustomValueGenerator2>(metadata.GetValueGeneratorFactory()(null, null));
        Assert.True(metadata.RequiresValueGenerator());

        Assert.Null(builder.HasValueGenerator(typeof(CustomValueGenerator1), ConfigurationSource.Convention));
        Assert.IsType<CustomValueGenerator2>(metadata.GetValueGeneratorFactory()(null, null));
        Assert.True(metadata.RequiresValueGenerator());
    }

    [ConditionalFact]
    public void Can_clear_CustomValueGenerator_type()
    {
        var metadata = CreateProperty();
        var builder = metadata.Builder;

        Assert.NotNull(builder.HasValueGenerator(typeof(CustomValueGenerator1), ConfigurationSource.DataAnnotation));

        Assert.IsType<CustomValueGenerator1>(metadata.GetValueGeneratorFactory()(null, null));
        Assert.Equal(ValueGenerated.Never, metadata.ValueGenerated);
        Assert.True(metadata.RequiresValueGenerator());

        Assert.Null(builder.HasValueGenerator((Type)null, ConfigurationSource.Convention));

        Assert.IsType<CustomValueGenerator1>(metadata.GetValueGeneratorFactory()(null, null));
        Assert.Equal(ValueGenerated.Never, metadata.ValueGenerated);
        Assert.True(metadata.RequiresValueGenerator());

        Assert.NotNull(builder.HasValueGenerator((Type)null, ConfigurationSource.Explicit));

        Assert.Null(metadata.GetValueGeneratorFactory());
        Assert.Equal(ValueGenerated.Never, metadata.ValueGenerated);
        Assert.False(metadata.RequiresValueGenerator());
    }

    private class CustomValueGenerator1 : ValueGenerator<string>
    {
        public override string Next(EntityEntry entry)
            => throw new NotImplementedException();

        public override bool GeneratesTemporaryValues
            => false;
    }

    private class CustomValueGenerator2 : ValueGenerator<string>
    {
        public override string Next(EntityEntry entry)
            => throw new NotImplementedException();

        public override bool GeneratesTemporaryValues
            => false;
    }

    private class CustomValueGeneratorFactory : ValueGeneratorFactory
    {
        public override ValueGenerator Create(IProperty property, ITypeBase typeBase)
            => new CustomValueGenerator1();
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_ValueConverter()
    {
        var builder = CreateInternalPropertyBuilder();
        var metadata = builder.Metadata;

        Assert.NotNull(builder.HasConversion(new UTF8StringToBytesConverter(), ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.HasConversion(new CastingConverter<string, string>(), ConfigurationSource.DataAnnotation));

        Assert.IsType<CastingConverter<string, string>>(metadata.GetValueConverter());

        Assert.Null(builder.HasConversion(new UTF8StringToBytesConverter(), ConfigurationSource.Convention));
        Assert.IsType<CastingConverter<string, string>>(metadata.GetValueConverter());

        Assert.Null(builder.HasConverter(typeof(UTF8StringToBytesConverter), ConfigurationSource.Convention));
        Assert.IsType<CastingConverter<string, string>>(metadata.GetValueConverter());

        Assert.NotNull(builder.HasConverter(typeof(UTF8StringToBytesConverter), ConfigurationSource.DataAnnotation));
        Assert.IsType<UTF8StringToBytesConverter>(metadata.GetValueConverter());

        Assert.NotNull(builder.HasConverter(null, ConfigurationSource.DataAnnotation));
        Assert.Null(metadata.GetValueConverter());
        Assert.Null(metadata[CoreAnnotationNames.ValueConverter]);
        Assert.Null(metadata[CoreAnnotationNames.ValueConverterType]);
    }

    private class UTF8StringToBytesConverter : StringToBytesConverter
    {
        public UTF8StringToBytesConverter()
            : base(Encoding.UTF8)
        {
        }
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_ValueComparer()
    {
        var builder = CreateInternalPropertyBuilder();
        var metadata = builder.Metadata;

        Assert.NotNull(builder.HasValueComparer(new CustomValueComparer<string>(), ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.HasValueComparer(new ValueComparer<string>(false), ConfigurationSource.DataAnnotation));

        Assert.IsType<ValueComparer<string>>(metadata.GetValueComparer());

        Assert.Null(builder.HasValueComparer(new CustomValueComparer<string>(), ConfigurationSource.Convention));
        Assert.IsType<ValueComparer<string>>(metadata.GetValueComparer());

        Assert.Null(builder.HasValueComparer(typeof(CustomValueComparer<string>), ConfigurationSource.Convention));
        Assert.IsType<ValueComparer<string>>(metadata.GetValueComparer());

        Assert.NotNull(builder.HasValueComparer(typeof(CustomValueComparer<string>), ConfigurationSource.DataAnnotation));
        Assert.IsType<CustomValueComparer<string>>(metadata.GetValueComparer());

        Assert.NotNull(builder.HasValueComparer((ValueComparer)null, ConfigurationSource.DataAnnotation));
        Assert.Null(metadata.GetValueComparer());
        Assert.Null(metadata[CoreAnnotationNames.ValueComparer]);
        Assert.Null(metadata[CoreAnnotationNames.ValueComparerType]);
    }

    private class CustomValueComparer<T> : ValueComparer<T>
    {
        public CustomValueComparer()
            : base(false)
        {
        }
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_ProviderValueComparer()
    {
        var builder = CreateInternalPropertyBuilder();
        var metadata = builder.Metadata;

        Assert.NotNull(builder.HasProviderValueComparer(new CustomValueComparer<string>(), ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.HasProviderValueComparer(new ValueComparer<string>(false), ConfigurationSource.DataAnnotation));

        Assert.IsType<ValueComparer<string>>(metadata.GetProviderValueComparer());

        Assert.Null(builder.HasProviderValueComparer(new CustomValueComparer<string>(), ConfigurationSource.Convention));
        Assert.IsType<ValueComparer<string>>(metadata.GetProviderValueComparer());

        Assert.Null(builder.HasProviderValueComparer(typeof(CustomValueComparer<string>), ConfigurationSource.Convention));
        Assert.IsType<ValueComparer<string>>(metadata.GetProviderValueComparer());

        Assert.NotNull(builder.HasProviderValueComparer(typeof(CustomValueComparer<string>), ConfigurationSource.DataAnnotation));
        Assert.IsType<CustomValueComparer<string>>(metadata.GetProviderValueComparer());

        Assert.NotNull(builder.HasProviderValueComparer((ValueComparer)null, ConfigurationSource.DataAnnotation));
        Assert.Null(metadata.GetProviderValueComparer());
        Assert.Null(metadata[CoreAnnotationNames.ProviderValueComparer]);
        Assert.Null(metadata[CoreAnnotationNames.ProviderValueComparerType]);
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_IsUnicode()
    {
        var builder = CreateInternalPropertyBuilder();
        var metadata = builder.Metadata;

        Assert.NotNull(builder.IsUnicode(true, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.IsUnicode(false, ConfigurationSource.DataAnnotation));

        Assert.False(metadata.IsUnicode().Value);

        Assert.Null(builder.IsUnicode(true, ConfigurationSource.Convention));
        Assert.False(metadata.IsUnicode().Value);
    }

    [ConditionalFact]
    public void Can_only_override_existing_IsUnicode_value_explicitly()
    {
        var metadata = CreateProperty();
        metadata.SetIsUnicode(true, ConfigurationSource.Explicit);
        var builder = metadata.Builder;

        Assert.NotNull(builder.IsUnicode(true, ConfigurationSource.DataAnnotation));
        Assert.Null(builder.IsUnicode(false, ConfigurationSource.DataAnnotation));

        Assert.True(metadata.IsUnicode().Value);

        Assert.NotNull(builder.IsUnicode(false, ConfigurationSource.Explicit));
        Assert.False(metadata.IsUnicode().Value);
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_Required()
    {
        var builder = CreateInternalPropertyBuilder();
        var metadata = builder.Metadata;

        Assert.NotNull(builder.IsRequired(true, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.IsRequired(false, ConfigurationSource.DataAnnotation));

        Assert.True(metadata.IsNullable);

        Assert.Null(builder.IsRequired(true, ConfigurationSource.Convention));
        Assert.True(metadata.IsNullable);
    }

    [ConditionalFact]
    public void Can_only_override_existing_Required_value_explicitly()
    {
        var metadata = CreateProperty();
        Assert.Null(metadata.GetIsNullableConfigurationSource());
        metadata.IsNullable = false;
        var builder = metadata.Builder;

        Assert.Equal(ConfigurationSource.Explicit, metadata.GetIsNullableConfigurationSource());
        Assert.NotNull(builder.IsRequired(true, ConfigurationSource.DataAnnotation));
        Assert.Null(builder.IsRequired(false, ConfigurationSource.DataAnnotation));

        Assert.False(metadata.IsNullable);

        Assert.NotNull(builder.IsRequired(false, ConfigurationSource.Explicit));
        Assert.True(metadata.IsNullable);
    }

    [ConditionalFact]
    public void Cannot_set_required_to_false_if_nonnullable()
    {
        var modelBuilder = new InternalModelBuilder(new Model());
        var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Convention);
        var builder = entityBuilder.Property(typeof(int), nameof(Customer.Id), ConfigurationSource.Convention);

        Assert.Null(builder.IsRequired(false, ConfigurationSource.DataAnnotation));

        Assert.Equal(
            CoreStrings.CannotBeNullable(nameof(Customer.Id), typeof(Customer).Name, "int"),
            Assert.Throws<InvalidOperationException>(() => builder.IsRequired(false, ConfigurationSource.Explicit)).Message);
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_BeforeSaveBehavior()
    {
        var builder = CreateInternalPropertyBuilder();
        var metadata = builder.Metadata;

        Assert.NotNull(builder.BeforeSave(PropertySaveBehavior.Throw, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.BeforeSave(PropertySaveBehavior.Ignore, ConfigurationSource.DataAnnotation));

        Assert.Equal(PropertySaveBehavior.Ignore, metadata.GetBeforeSaveBehavior());

        Assert.Null(builder.BeforeSave(PropertySaveBehavior.Save, ConfigurationSource.Convention));
        Assert.Equal(PropertySaveBehavior.Ignore, metadata.GetBeforeSaveBehavior());
    }

    [ConditionalFact]
    public void Can_only_override_existing_BeforeSaveBehavior_value_explicitly()
    {
        var metadata = CreateProperty();
        Assert.Null(metadata.GetBeforeSaveBehaviorConfigurationSource());
        metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Throw, ConfigurationSource.Explicit);
        var builder = metadata.Builder;

        Assert.Equal(ConfigurationSource.Explicit, metadata.GetBeforeSaveBehaviorConfigurationSource());
        Assert.NotNull(builder.BeforeSave(PropertySaveBehavior.Throw, ConfigurationSource.DataAnnotation));
        Assert.Null(builder.BeforeSave(PropertySaveBehavior.Ignore, ConfigurationSource.DataAnnotation));

        Assert.Equal(PropertySaveBehavior.Throw, metadata.GetBeforeSaveBehavior());

        Assert.NotNull(builder.BeforeSave(PropertySaveBehavior.Ignore, ConfigurationSource.Explicit));
        Assert.Equal(PropertySaveBehavior.Ignore, metadata.GetBeforeSaveBehavior());
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_AfterSaveBehavior()
    {
        var builder = CreateInternalPropertyBuilder();
        var metadata = builder.Metadata;

        Assert.NotNull(builder.AfterSave(PropertySaveBehavior.Throw, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.AfterSave(PropertySaveBehavior.Ignore, ConfigurationSource.DataAnnotation));

        Assert.Equal(PropertySaveBehavior.Ignore, metadata.GetAfterSaveBehavior());

        Assert.Null(builder.AfterSave(PropertySaveBehavior.Save, ConfigurationSource.Convention));
        Assert.Equal(PropertySaveBehavior.Ignore, metadata.GetAfterSaveBehavior());
    }

    [ConditionalFact]
    public void Can_only_override_existing_AfterSaveBehavior_value_explicitly()
    {
        var metadata = CreateProperty();
        Assert.Null(metadata.GetAfterSaveBehaviorConfigurationSource());
        metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw, ConfigurationSource.Explicit);
        var builder = metadata.Builder;

        Assert.Equal(ConfigurationSource.Explicit, metadata.GetAfterSaveBehaviorConfigurationSource());
        Assert.NotNull(builder.AfterSave(PropertySaveBehavior.Throw, ConfigurationSource.DataAnnotation));
        Assert.Null(builder.AfterSave(PropertySaveBehavior.Ignore, ConfigurationSource.DataAnnotation));

        Assert.Equal(PropertySaveBehavior.Throw, metadata.GetAfterSaveBehavior());

        Assert.NotNull(builder.AfterSave(PropertySaveBehavior.Ignore, ConfigurationSource.Explicit));
        Assert.Equal(PropertySaveBehavior.Ignore, metadata.GetAfterSaveBehavior());
    }

    private InternalPropertyBuilder CreateInternalPropertyBuilder()
    {
        var modelBuilder = (InternalModelBuilder)
            InMemoryTestHelpers.Instance.CreateConventionBuilder().GetInfrastructure();
        var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Convention);
        return entityBuilder.Property(Customer.NameProperty, ConfigurationSource.Convention);
    }

    private Property CreateProperty()
        => CreateInternalPropertyBuilder().Metadata;

    private class Customer
    {
        public static readonly PropertyInfo NameProperty = typeof(Customer).GetProperty("Name");

        public int Id { get; set; }
        public string Name { get; set; }
    }
}
