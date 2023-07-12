// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

public class SkipNavigationTest
{
    [ConditionalFact]
    public void Throws_when_model_is_readonly()
    {
        var model = CreateModel();
        var firstEntity = model.AddEntityType(typeof(Order));
        var firstIdProperty = firstEntity.AddProperty(Order.IdProperty);
        var firstKey = firstEntity.AddKey(firstIdProperty);
        var secondEntity = model.AddEntityType(typeof(Product));
        var joinEntityBuilder = model.AddEntityType(typeof(OrderProduct));
        var orderIdProperty = joinEntityBuilder.AddProperty(OrderProduct.OrderIdProperty);

        var navigation = firstEntity.AddSkipNavigation(nameof(Order.Products), null, null, secondEntity, true, false);

        model.FinalizeModel();

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(
                () => firstEntity.AddSkipNavigation(nameof(Order.Products), null, null, secondEntity, true, false)).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => firstEntity.RemoveSkipNavigation(navigation)).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => navigation.SetInverse(null)).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => navigation.SetForeignKey(null)).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => navigation.SetField(null)).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => navigation.SetIsEagerLoaded(null)).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => navigation.SetLazyLoadingEnabled(null)).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => navigation.SetPropertyAccessMode(null)).Message);
    }

    [ConditionalFact]
    public void Gets_expected_default_values()
    {
        var model = (IConventionModel)CreateModel();
        var firstEntity = model.AddEntityType(typeof(Order));
        var firstIdProperty = firstEntity.AddProperty(Order.IdProperty);
        var firstKey = firstEntity.AddKey(firstIdProperty);
        var secondEntity = model.AddEntityType(typeof(Product));
        var joinEntityBuilder = model.AddEntityType(typeof(OrderProduct));
        var orderIdProperty = joinEntityBuilder.AddProperty(OrderProduct.OrderIdProperty);
        var firstFk = joinEntityBuilder
            .AddForeignKey(new[] { orderIdProperty }, firstKey, firstEntity);

        var navigation = firstEntity.AddSkipNavigation(nameof(Order.Products), null, null, secondEntity, true, false);
        navigation.SetForeignKey(firstFk);

        Assert.True(navigation.IsCollection);
        Assert.False(navigation.IsOnDependent);
        Assert.False(navigation.IsEagerLoaded);
        Assert.Null(navigation.Inverse);
        Assert.Equal(firstFk, navigation.ForeignKey);
        Assert.Equal(nameof(Order.Products), navigation.Name);
        Assert.Null(navigation.FieldInfo);
        Assert.NotNull(navigation.PropertyInfo);
        Assert.Equal(ConfigurationSource.Convention, navigation.GetForeignKeyConfigurationSource());
        Assert.Null(navigation.GetInverseConfigurationSource());
        Assert.Equal(ConfigurationSource.Convention, navigation.GetConfigurationSource());
        Assert.Equal(PropertyAccessMode.PreferField, navigation.GetPropertyAccessMode());
        Assert.Null(navigation.GetPropertyAccessModeConfigurationSource());

        Assert.Same(navigation, firstEntity.FindDeclaredSkipNavigation(navigation.Name));
        Assert.Same(navigation, firstEntity.FindSkipNavigation(navigation.Name));
        Assert.Same(navigation, firstEntity.FindSkipNavigation(navigation.GetIdentifyingMemberInfo()));
        Assert.Same(navigation, firstEntity.GetDeclaredSkipNavigations().Single());
    }

    [ConditionalFact]
    public void Can_set_foreign_key()
    {
        var model = (IConventionModel)CreateModel();
        var firstEntity = model.AddEntityType(typeof(Order));
        var firstIdProperty = firstEntity.AddProperty(Order.IdProperty);
        var firstKey = firstEntity.AddKey(firstIdProperty);
        var secondEntity = model.AddEntityType(typeof(Product));
        var joinEntityBuilder = model.AddEntityType(typeof(OrderProduct));
        var orderIdProperty = joinEntityBuilder.AddProperty(OrderProduct.OrderIdProperty);
        var firstFk = joinEntityBuilder
            .AddForeignKey(new[] { orderIdProperty }, firstKey, firstEntity);

        var navigation = firstEntity.AddSkipNavigation(nameof(Order.Products), null, null, secondEntity, true, false);

        Assert.Null(navigation.ForeignKey);
        Assert.Null(navigation.GetForeignKeyConfigurationSource());

        navigation.SetForeignKey(firstFk, fromDataAnnotation: true);

        Assert.Same(firstFk, navigation.ForeignKey);
        Assert.Equal(ConfigurationSource.DataAnnotation, navigation.GetForeignKeyConfigurationSource());

        navigation.SetForeignKey(null);

        Assert.Null(navigation.ForeignKey);
        Assert.Null(navigation.GetForeignKeyConfigurationSource());
    }

    [ConditionalFact]
    public void Setting_foreign_key_to_skip_navigation_with_wrong_dependent_throws()
    {
        var model = CreateModel();
        var orderEntity = model.AddEntityType(typeof(Order));
        var orderIdProperty = orderEntity.AddProperty(Order.IdProperty);
        var orderKey = orderEntity.AddKey(orderIdProperty);
        var productEntity = model.AddEntityType(typeof(Product));
        var orderProductEntity = model.AddEntityType(typeof(OrderProduct));
        var orderProductFkProperty = orderProductEntity.AddProperty(nameof(OrderProduct.OrderId), typeof(int));
        var orderProductForeignKey = orderProductEntity.AddForeignKey(orderProductFkProperty, orderKey, orderEntity);

        var navigation = orderEntity.AddSkipNavigation(nameof(Order.Products), null, null, productEntity, true, true);

        Assert.Equal(
            CoreStrings.SkipNavigationForeignKeyWrongDependentType(
                "{'" + nameof(OrderProduct.OrderId) + "'}", nameof(Order), nameof(Order.Products), nameof(OrderProduct)),
            Assert.Throws<InvalidOperationException>(() => navigation.SetForeignKey(orderProductForeignKey)).Message);
    }

    [ConditionalFact]
    public void Setting_foreign_key_to_skip_navigation_with_wrong_principal_throws()
    {
        var model = CreateModel();
        var orderEntity = model.AddEntityType(typeof(Order));
        var orderIdProperty = orderEntity.AddProperty(Order.IdProperty);
        var orderKey = orderEntity.AddKey(orderIdProperty);
        var productEntity = model.AddEntityType(typeof(Product));
        var orderProductEntity = model.AddEntityType(typeof(OrderProduct));
        var orderProductFkProperty = orderProductEntity.AddProperty(nameof(OrderProduct.OrderId), typeof(int));
        var orderProductForeignKey = orderProductEntity.AddForeignKey(orderProductFkProperty, orderKey, orderEntity);

        var navigation = orderProductEntity.AddSkipNavigation(
            nameof(OrderProduct.Order), null, null, orderEntity, false, false);

        Assert.Equal(
            CoreStrings.SkipNavigationForeignKeyWrongPrincipalType(
                "{'" + nameof(OrderProduct.OrderId) + "'}", nameof(OrderProduct), nameof(OrderProduct.Order), nameof(Order)),
            Assert.Throws<InvalidOperationException>(() => navigation.SetForeignKey(orderProductForeignKey)).Message);
    }

    [ConditionalFact]
    public void Setting_foreign_key_with_wrong_inverse_throws()
    {
        var model = CreateModel();
        var orderEntity = model.AddEntityType(typeof(Order));
        var orderIdProperty = orderEntity.AddProperty(Order.IdProperty);
        var orderKey = orderEntity.AddKey(orderIdProperty);
        var productEntity = model.AddEntityType(typeof(Product));
        var productIdProperty = productEntity.AddProperty(Product.IdProperty);
        var productKey = productEntity.AddKey(productIdProperty);
        var orderProductEntity = model.AddEntityType(typeof(OrderProduct));
        var orderProductFkProperty = orderProductEntity.AddProperty(OrderProduct.OrderIdProperty);
        var orderProductForeignKey = orderProductEntity
            .AddForeignKey(new[] { orderProductFkProperty }, orderKey, orderEntity);
        var productFkProperty = productEntity.AddProperty("Fk", typeof(int));
        var productOrderForeignKey = productEntity
            .AddForeignKey(new[] { productFkProperty }, productKey, productEntity);

        var productsNavigation = orderEntity.AddSkipNavigation(nameof(Order.Products), null, null, productEntity, true, false);

        var ordersNavigation = productEntity.AddSkipNavigation(nameof(Product.Orders), null, null, orderEntity, true, false);
        ordersNavigation.SetForeignKey(productOrderForeignKey);

        productsNavigation.SetInverse(ordersNavigation);

        Assert.Equal(
            CoreStrings.SkipInverseMismatchedForeignKey(
                "{'" + orderProductFkProperty.Name + "'}",
                nameof(Order.Products), nameof(OrderProduct),
                nameof(Product.Orders), nameof(Product)),
            Assert.Throws<InvalidOperationException>(() => productsNavigation.SetForeignKey(orderProductForeignKey)).Message);
    }

    [ConditionalFact]
    public void Can_set_inverse()
    {
        var model = CreateModel();
        var orderEntity = model.AddEntityType(typeof(Order));
        var orderIdProperty = orderEntity.AddProperty(Order.IdProperty);
        var orderKey = orderEntity.AddKey(orderIdProperty);
        var productEntity = model.AddEntityType(typeof(Product));
        var productIdProperty = productEntity.AddProperty(Product.IdProperty);
        var productKey = productEntity.AddKey(productIdProperty);
        var orderProductEntity = model.AddEntityType(typeof(OrderProduct));
        var orderProductFkProperty = orderProductEntity.AddProperty(OrderProduct.OrderIdProperty);
        var orderProductForeignKey = orderProductEntity
            .AddForeignKey(new[] { orderProductFkProperty }, orderKey, orderEntity);
        var productOrderFkProperty = orderProductEntity.AddProperty(OrderProduct.ProductIdProperty);
        var productOrderForeignKey = orderProductEntity
            .AddForeignKey(new[] { productOrderFkProperty }, productKey, productEntity);

        var productsNavigation = orderEntity.AddSkipNavigation(nameof(Order.Products), null, null, productEntity, true, false);
        productsNavigation.SetForeignKey(orderProductForeignKey);

        var ordersNavigation = productEntity.AddSkipNavigation(nameof(Product.Orders), null, null, orderEntity, true, false);
        ordersNavigation.SetForeignKey(productOrderForeignKey);

        productsNavigation.SetInverse(ordersNavigation);
        ordersNavigation.SetInverse(productsNavigation);

        Assert.Same(ordersNavigation, productsNavigation.Inverse);
        Assert.Same(productsNavigation, ordersNavigation.Inverse);
        Assert.Equal(ConfigurationSource.Explicit, ((IConventionSkipNavigation)productsNavigation).GetConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, ((IConventionSkipNavigation)ordersNavigation).GetConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, ((IConventionSkipNavigation)productsNavigation).GetInverseConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, ((IConventionSkipNavigation)ordersNavigation).GetInverseConfigurationSource());

        Assert.Equal(
            CoreStrings.SkipNavigationInUseBySkipNavigation(
                nameof(Order), nameof(Order.Products), nameof(Product), nameof(Product.Orders)),
            Assert.Throws<InvalidOperationException>(() => orderEntity.RemoveSkipNavigation(productsNavigation)).Message);

        productsNavigation.SetInverse(null);
        ordersNavigation.SetInverse(null);

        Assert.Null(((IConventionSkipNavigation)productsNavigation).GetInverseConfigurationSource());
        Assert.Null(((IConventionSkipNavigation)ordersNavigation).GetInverseConfigurationSource());
    }

    [ConditionalFact]
    public void Setting_inverse_targetting_wrong_type_throws()
    {
        var model = CreateModel();
        var orderEntity = model.AddEntityType(typeof(Order));
        var orderIdProperty = orderEntity.AddProperty(Order.IdProperty);
        var orderKey = orderEntity.AddKey(orderIdProperty);
        var productEntity = model.AddEntityType(typeof(Product));
        var productIdProperty = productEntity.AddProperty(Product.IdProperty);
        var productKey = productEntity.AddKey(productIdProperty);
        var orderProductEntity = model.AddEntityType(typeof(OrderProduct));
        var orderProductFkProperty = orderProductEntity.AddProperty(OrderProduct.OrderIdProperty);
        var orderProductForeignKey = orderProductEntity
            .AddForeignKey(new[] { orderProductFkProperty }, orderKey, orderEntity);
        var productOrderFkProperty = orderProductEntity.AddProperty(OrderProduct.ProductIdProperty);
        var productOrderForeignKey = orderProductEntity
            .AddForeignKey(new[] { productOrderFkProperty }, productKey, productEntity);

        var productsNavigation = orderEntity.AddSkipNavigation(nameof(Order.Products), null, null, productEntity, true, false);
        productsNavigation.SetForeignKey(orderProductForeignKey);

        var ordersNavigation = orderProductEntity.AddSkipNavigation(nameof(OrderProduct.Product), null, null, productEntity, false, true);
        ordersNavigation.SetForeignKey(productOrderForeignKey);

        Assert.Equal(
            CoreStrings.SkipNavigationWrongInverse(
                nameof(OrderProduct.Product), nameof(OrderProduct), nameof(Order.Products), nameof(Product)),
            Assert.Throws<InvalidOperationException>(() => productsNavigation.SetInverse(ordersNavigation)).Message);
    }

    [ConditionalFact]
    public void Setting_inverse_with_wrong_join_type_throws()
    {
        var model = CreateModel();
        var orderEntity = model.AddEntityType(typeof(Order));
        var orderIdProperty = orderEntity.AddProperty(Order.IdProperty);
        var orderKey = orderEntity.AddKey(orderIdProperty);
        var productEntity = model.AddEntityType(typeof(Product));
        var productIdProperty = productEntity.AddProperty(Product.IdProperty);
        var productKey = productEntity.AddKey(productIdProperty);
        var orderProductEntity = model.AddEntityType(typeof(OrderProduct));
        var orderProductFkProperty = orderProductEntity.AddProperty(OrderProduct.OrderIdProperty);
        var orderProductForeignKey = orderProductEntity
            .AddForeignKey(new[] { orderProductFkProperty }, orderKey, orderEntity);
        var productFkProperty = productEntity.AddProperty("Fk", typeof(int));
        var productOrderForeignKey = productEntity
            .AddForeignKey(new[] { productFkProperty }, productKey, productEntity);

        var productsNavigation = orderEntity.AddSkipNavigation(nameof(Order.Products), null, null, productEntity, true, false);
        productsNavigation.SetForeignKey(orderProductForeignKey);

        var ordersNavigation = productEntity.AddSkipNavigation(nameof(Product.Orders), null, null, orderEntity, true, false);
        ordersNavigation.SetForeignKey(productOrderForeignKey);

        Assert.Equal(
            CoreStrings.SkipInverseMismatchedJoinType(
                nameof(Product.Orders), nameof(Product), nameof(Order.Products), nameof(OrderProduct)),
            Assert.Throws<InvalidOperationException>(() => productsNavigation.SetInverse(ordersNavigation)).Message);
    }

    private static IMutableModel CreateModel()
        => new Model();

    private class Order
    {
        public static readonly PropertyInfo IdProperty = typeof(Order).GetProperty(nameof(Id));

        public int Id { get; set; }

        public virtual ICollection<Product> Products { get; set; }
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

    private class Product
    {
        public static readonly PropertyInfo IdProperty = typeof(Product).GetProperty(nameof(Id));

        public int Id { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
