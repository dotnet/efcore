// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

public class InternalSkipNavigationBuilderTest
{
    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_SkipNavigation()
    {
        var builder = CreateInternalSkipNavigationBuilder();
        IConventionSkipNavigation skipNavigation = builder.Metadata;

        ((SkipNavigation)skipNavigation).SetConfigurationSource(ConfigurationSource.DataAnnotation);

        var productEntity = skipNavigation.TargetEntityType.Builder;
        Assert.Null(productEntity.HasRelationship(skipNavigation.DeclaringEntityType, null, nameof(Order.Products)));

        Assert.NotNull(
            productEntity.HasRelationship(
                skipNavigation.DeclaringEntityType, null, nameof(Order.Products), fromDataAnnotation: true));

        Assert.False(skipNavigation.IsInModel);
        Assert.Empty(skipNavigation.DeclaringEntityType.GetSkipNavigations());
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_HasField()
    {
        var builder = CreateInternalSkipNavigationBuilder();
        var metadata = builder.Metadata;

        Assert.NotNull(metadata.FieldInfo);
        Assert.Equal(ConfigurationSource.Convention, metadata.GetFieldInfoConfigurationSource());

        Assert.True(builder.CanSetField(Order.ProductsField, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.HasField(Order.ProductsField, ConfigurationSource.DataAnnotation));

        Assert.Equal(Order.ProductsField, metadata.FieldInfo);
        Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetFieldInfoConfigurationSource());

        Assert.True(builder.CanSetField(Order.ProductsField, ConfigurationSource.Convention));
        Assert.False(builder.CanSetField(Order.OtherProductsField, ConfigurationSource.Convention));
        Assert.NotNull(builder.HasField(Order.ProductsField, ConfigurationSource.Convention));
        Assert.Null(builder.HasField(Order.OtherProductsField, ConfigurationSource.Convention));

        Assert.Equal(Order.ProductsField, metadata.FieldInfo);
        Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetFieldInfoConfigurationSource());

        Assert.True(builder.CanSetField(Order.OtherProductsField, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.HasField(Order.OtherProductsField, ConfigurationSource.DataAnnotation));

        Assert.Equal(Order.OtherProductsField, metadata.FieldInfo);
        Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetFieldInfoConfigurationSource());

        Assert.True(builder.CanSetField((string)null, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.HasField((string)null, ConfigurationSource.DataAnnotation));

        Assert.Null(metadata.FieldInfo);
        Assert.Null(metadata.GetFieldInfoConfigurationSource());
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_HasField_string()
    {
        var builder = CreateInternalSkipNavigationBuilder();
        var metadata = builder.Metadata;

        Assert.NotNull(metadata.FieldInfo);
        Assert.Equal(ConfigurationSource.Convention, metadata.GetFieldInfoConfigurationSource());

        Assert.True(builder.CanSetField("_products", ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.HasField("_products", ConfigurationSource.DataAnnotation));

        Assert.Equal("_products", metadata.FieldInfo?.Name);
        Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetFieldInfoConfigurationSource());

        Assert.True(builder.CanSetField("_products", ConfigurationSource.Convention));
        Assert.False(builder.CanSetField("_otherProducts", ConfigurationSource.Convention));
        Assert.NotNull(builder.HasField("_products", ConfigurationSource.Convention));
        Assert.Null(builder.HasField("_otherProducts", ConfigurationSource.Convention));

        Assert.Equal("_products", metadata.FieldInfo?.Name);
        Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetFieldInfoConfigurationSource());

        Assert.True(builder.CanSetField("_otherProducts", ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.HasField("_otherProducts", ConfigurationSource.DataAnnotation));

        Assert.Equal("_otherProducts", metadata.FieldInfo?.Name);
        Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetFieldInfoConfigurationSource());

        Assert.True(builder.CanSetField((string)null, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.HasField((string)null, ConfigurationSource.DataAnnotation));

        Assert.Null(metadata.FieldInfo);
        Assert.Null(metadata.GetFieldInfoConfigurationSource());
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_PropertyAccessMode()
    {
        var builder = CreateInternalSkipNavigationBuilder();
        IConventionSkipNavigation metadata = builder.Metadata;

        Assert.Equal(PropertyAccessMode.PreferField, metadata.GetPropertyAccessMode());
        Assert.Null(metadata.GetPropertyAccessModeConfigurationSource());

        Assert.True(builder.CanSetPropertyAccessMode(PropertyAccessMode.PreferProperty, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.UsePropertyAccessMode(PropertyAccessMode.PreferProperty, ConfigurationSource.DataAnnotation));

        Assert.Equal(PropertyAccessMode.PreferProperty, metadata.GetPropertyAccessMode());
        Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetPropertyAccessModeConfigurationSource());

        Assert.True(builder.CanSetPropertyAccessMode(PropertyAccessMode.PreferProperty, ConfigurationSource.Convention));
        Assert.False(
            builder.CanSetPropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction, ConfigurationSource.Convention));
        Assert.NotNull(builder.UsePropertyAccessMode(PropertyAccessMode.PreferProperty, ConfigurationSource.Convention));
        Assert.Null(builder.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction, ConfigurationSource.Convention));

        Assert.Equal(PropertyAccessMode.PreferProperty, metadata.GetPropertyAccessMode());
        Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetPropertyAccessModeConfigurationSource());

        Assert.True(
            builder.CanSetPropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction, ConfigurationSource.DataAnnotation));
        Assert.NotNull(
            builder.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction, ConfigurationSource.DataAnnotation));

        Assert.Equal(PropertyAccessMode.PreferFieldDuringConstruction, metadata.GetPropertyAccessMode());
        Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetPropertyAccessModeConfigurationSource());

        Assert.True(builder.CanSetPropertyAccessMode(null, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.UsePropertyAccessMode(null, ConfigurationSource.DataAnnotation));

        Assert.Equal(PropertyAccessMode.PreferField, metadata.GetPropertyAccessMode());
        Assert.Null(metadata.GetPropertyAccessModeConfigurationSource());
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_ForeignKey()
    {
        var builder = CreateInternalSkipNavigationBuilder();
        IConventionSkipNavigation metadata = builder.Metadata;

        var originalFK = metadata.ForeignKey;
        Assert.NotNull(originalFK);
        Assert.Equal(ConfigurationSource.Convention, metadata.GetForeignKeyConfigurationSource());

        var orderProductEntity = metadata.DeclaringEntityType.Model.Builder.Entity(typeof(OrderProduct));
        var fk = (ForeignKey)orderProductEntity
            .HasRelationship(metadata.DeclaringEntityType, nameof(OrderProduct.Order))
            .IsUnique(false)
            .Metadata;

        Assert.NotSame(fk, metadata.ForeignKey);
        Assert.Same(originalFK, metadata.ForeignKey);
        Assert.Equal(ConfigurationSource.Convention, metadata.GetForeignKeyConfigurationSource());
        Assert.NotNull(metadata.Inverse.ForeignKey);

        Assert.True(builder.CanSetForeignKey(fk, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.HasForeignKey(fk, ConfigurationSource.DataAnnotation));

        Assert.Equal(fk, metadata.ForeignKey);
        Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetForeignKeyConfigurationSource());
        Assert.Null(metadata.Inverse.ForeignKey);

        Assert.True(builder.CanSetForeignKey(fk, ConfigurationSource.Convention));
        Assert.False(builder.CanSetForeignKey(null, ConfigurationSource.Convention));
        Assert.NotNull(builder.HasForeignKey(fk, ConfigurationSource.Convention));
        Assert.Null(builder.HasForeignKey(null, ConfigurationSource.Convention));

        Assert.Equal(fk, metadata.ForeignKey);
        Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetForeignKeyConfigurationSource());

        Assert.True(builder.CanSetForeignKey(null, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.HasForeignKey(null, ConfigurationSource.DataAnnotation));

        Assert.Null(metadata.ForeignKey);
        Assert.Null(metadata.GetForeignKeyConfigurationSource());
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_Inverse()
    {
        var builder = CreateInternalSkipNavigationBuilder();
        IConventionSkipNavigation metadata = builder.Metadata;

        // the skip navigation is pointing to the automatically-generated
        // join entity type and so is its inverse
        var inverse = (SkipNavigation)metadata.TargetEntityType.Builder.HasSkipNavigation(
                Product.OrdersProperty,
                metadata.DeclaringEntityType)
            .Metadata;

        Assert.NotNull(metadata.Inverse);
        Assert.Equal(ConfigurationSource.Convention, metadata.GetInverseConfigurationSource());
        Assert.NotNull(inverse.Inverse);
        Assert.Equal(ConfigurationSource.Convention, inverse.GetInverseConfigurationSource());

        // now explicitly assign the skip navigation's Inverse
        Assert.True(builder.CanSetInverse(inverse, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.HasInverse(inverse, ConfigurationSource.DataAnnotation));

        Assert.Equal(inverse, metadata.Inverse);
        Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetInverseConfigurationSource());
        Assert.Equal(metadata, inverse.Inverse);
        Assert.Equal(ConfigurationSource.DataAnnotation, inverse.GetInverseConfigurationSource());

        Assert.True(builder.CanSetInverse(inverse, ConfigurationSource.Convention));
        Assert.False(builder.CanSetInverse(null, ConfigurationSource.Convention));
        Assert.NotNull(builder.HasInverse(inverse, ConfigurationSource.Convention));
        Assert.Null(builder.HasInverse(null, ConfigurationSource.Convention));

        Assert.Equal(inverse, metadata.Inverse);
        Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetInverseConfigurationSource());
        Assert.Equal(metadata, inverse.Inverse);
        Assert.Equal(ConfigurationSource.DataAnnotation, inverse.GetInverseConfigurationSource());

        Assert.True(builder.CanSetInverse(null, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.HasInverse(null, ConfigurationSource.DataAnnotation));

        Assert.Null(metadata.Inverse);
        Assert.Null(metadata.GetInverseConfigurationSource());
        Assert.Null(inverse.Inverse);
        Assert.Null(inverse.GetInverseConfigurationSource());
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_IsEagerLoaded()
    {
        var builder = CreateInternalSkipNavigationBuilder();
        IConventionSkipNavigation metadata = builder.Metadata;

        Assert.False(metadata.IsEagerLoaded);
        Assert.Null(metadata.GetIsEagerLoadedConfigurationSource());

        Assert.True(builder.CanSetAutoInclude(autoInclude: true, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.AutoInclude(autoInclude: true, ConfigurationSource.DataAnnotation));

        Assert.True(metadata.IsEagerLoaded);
        Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetIsEagerLoadedConfigurationSource());

        Assert.True(builder.CanSetAutoInclude(autoInclude: true, ConfigurationSource.Convention));
        Assert.False(builder.CanSetAutoInclude(autoInclude: false, ConfigurationSource.Convention));
        Assert.NotNull(builder.AutoInclude(autoInclude: true, ConfigurationSource.Convention));
        Assert.Null(builder.AutoInclude(autoInclude: false, ConfigurationSource.Convention));

        Assert.True(metadata.IsEagerLoaded);
        Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetIsEagerLoadedConfigurationSource());

        Assert.True(builder.CanSetAutoInclude(autoInclude: false, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.AutoInclude(autoInclude: false, ConfigurationSource.DataAnnotation));

        Assert.False(metadata.IsEagerLoaded);
        Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetIsEagerLoadedConfigurationSource());

        Assert.True(builder.CanSetAutoInclude(null, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.AutoInclude(null, ConfigurationSource.DataAnnotation));

        Assert.False(metadata.IsEagerLoaded);
        Assert.Null(metadata.GetIsEagerLoadedConfigurationSource());
    }

    [ConditionalFact]
    public void Can_only_override_lower_or_equal_source_LazyLoadingEnabled()
    {
        var builder = CreateInternalSkipNavigationBuilder();
        IConventionSkipNavigation metadata = builder.Metadata;

        Assert.True(metadata.LazyLoadingEnabled);
        Assert.Null(metadata.GetLazyLoadingEnabledConfigurationSource());

        Assert.True(builder.CanSetLazyLoadingEnabled(lazyLoadingEnabled: false, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.EnableLazyLoading(lazyLoadingEnabled: false, ConfigurationSource.DataAnnotation));

        Assert.False(metadata.LazyLoadingEnabled);
        Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetLazyLoadingEnabledConfigurationSource());

        Assert.True(builder.CanSetLazyLoadingEnabled(lazyLoadingEnabled: false, ConfigurationSource.Convention));
        Assert.False(builder.CanSetLazyLoadingEnabled(lazyLoadingEnabled: true, ConfigurationSource.Convention));
        Assert.NotNull(builder.EnableLazyLoading(lazyLoadingEnabled: false, ConfigurationSource.Convention));
        Assert.Null(builder.EnableLazyLoading(lazyLoadingEnabled: true, ConfigurationSource.Convention));

        Assert.False(metadata.LazyLoadingEnabled);
        Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetLazyLoadingEnabledConfigurationSource());

        Assert.True(builder.CanSetLazyLoadingEnabled(lazyLoadingEnabled: true, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.EnableLazyLoading(lazyLoadingEnabled: true, ConfigurationSource.DataAnnotation));

        Assert.True(metadata.LazyLoadingEnabled);
        Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetLazyLoadingEnabledConfigurationSource());

        Assert.True(builder.CanSetLazyLoadingEnabled(null, ConfigurationSource.DataAnnotation));
        Assert.NotNull(builder.EnableLazyLoading(null, ConfigurationSource.DataAnnotation));

        Assert.True(metadata.LazyLoadingEnabled);
        Assert.Null(metadata.GetLazyLoadingEnabledConfigurationSource());
    }

    private InternalSkipNavigationBuilder CreateInternalSkipNavigationBuilder()
    {
        var modelBuilder = (InternalModelBuilder)
            InMemoryTestHelpers.Instance.CreateConventionBuilder().GetInfrastructure();

        return modelBuilder.Entity(typeof(Order), ConfigurationSource.Convention)
            .HasSkipNavigation(
                MemberIdentity.Create(Order.ProductsProperty),
                modelBuilder.Entity(typeof(Product), ConfigurationSource.Convention).Metadata,
                ConfigurationSource.Convention);
    }

    protected class Order
    {
        public static readonly PropertyInfo ProductsProperty = typeof(Order).GetProperty(nameof(Products));

        public static readonly FieldInfo ProductsField = typeof(Order)
            .GetField(nameof(_products), BindingFlags.Instance | BindingFlags.NonPublic);

        public static readonly FieldInfo OtherProductsField = typeof(Order)
            .GetField(nameof(_otherProducts), BindingFlags.Instance | BindingFlags.NonPublic);

        public int OrderId { get; set; }

        private ICollection<Product> _products;
        private readonly ICollection<Product> _otherProducts = new List<Product>();
        public ICollection<Product> Products { get => _products; set => _products = value; }
    }

    private class OrderProduct
    {
        public static readonly PropertyInfo OrderIdProperty = typeof(OrderProduct).GetProperty(nameof(OrderId));
        public static readonly PropertyInfo ProductIdProperty = typeof(OrderProduct).GetProperty(nameof(ProductId));

        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }

    protected class Product
    {
        public static readonly PropertyInfo OrdersProperty = typeof(Product).GetProperty(nameof(Orders));

        public int Id { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
