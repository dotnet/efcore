// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring an <see cref="IMutableEntityType" />.
/// </summary>
/// <remarks>
///     Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///     and it is not designed to be directly constructed in your application code.
/// </remarks>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
/// <typeparam name="TComplex">The complex type being configured.</typeparam>
public class ComplexCollectionBuilder<[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TComplex>
    : ComplexCollectionBuilder
    where TComplex : notnull
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public ComplexCollectionBuilder(IMutableComplexProperty complexProperty)
        : base(complexProperty)
    {
    }


    /// <summary>
    ///     Adds or updates an annotation on the complex property. If an annotation with the key specified in
    ///     <paramref name="annotation" /> already exists its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same typeBuilder instance so that multiple configuration calls can be chained.</returns>
    public new virtual ComplexCollectionBuilder<TComplex> HasPropertyAnnotation(string annotation, object? value)
        => (ComplexCollectionBuilder<TComplex>)base.HasPropertyAnnotation(annotation, value);

    /// <summary>
    ///     Adds or updates an annotation on the complex type. If an annotation with the key specified in
    ///     <paramref name="annotation" /> already exists its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same typeBuilder instance so that multiple configuration calls can be chained.</returns>
    public new virtual ComplexCollectionBuilder<TComplex> HasTypeAnnotation(string annotation, object? value)
        => (ComplexCollectionBuilder<TComplex>)base.HasTypeAnnotation(annotation, value);

    /// <summary>
    ///     Returns an object that can be used to configure a property of the complex type.
    ///     If the specified property is not already part of the model, it will be added.
    /// </summary>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (
    ///     <c>blog => blog.Url</c>).
    /// </param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ComplexTypePropertyBuilder<TProperty> Property<TProperty>(Expression<Func<TComplex, TProperty>> propertyExpression)
        => new(
            TypeBuilder.Property(
                    Check.NotNull(propertyExpression, nameof(propertyExpression)).GetMemberAccess(), ConfigurationSource.Explicit)!
                .Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a property of the complex type where that property represents
    ///     a collection of primitive values, such as strings or integers.
    ///     If the specified property is not already part of the model, it will be added.
    /// </summary>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (
    ///     <c>blog => blog.Url</c>).
    /// </param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ComplexTypePrimitiveCollectionBuilder<TProperty> PrimitiveCollection<TProperty>(
        Expression<Func<TComplex, TProperty>> propertyExpression)
        => new(
            TypeBuilder.PrimitiveCollection(
                    Check.NotNull(propertyExpression, nameof(propertyExpression)).GetMemberAccess(), ConfigurationSource.Explicit)!
                .Metadata);

    /// <summary>
    ///     Configures a complex property of the complex type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new property with this overload the property name must match the
    ///     name of a CLR property or field on the complex type. This overload cannot be used to
    ///     add a new shadow state complex property.
    /// </remarks>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <param name="buildAction">An action that performs configuration of the property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual ComplexCollectionBuilder<TComplex> ComplexProperty(string propertyName, Action<ComplexPropertyBuilder> buildAction)
        => (ComplexCollectionBuilder<TComplex>)base.ComplexProperty(propertyName, buildAction);

    /// <summary>
    ///     Configures a complex property of the complex type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new property, if a property with the same name exists in the complex class
    ///     then it will be added to the model. If no property exists in the complex class, then
    ///     a new shadow state complex property will be added. A shadow state property is one that does not have a
    ///     corresponding property in the complex class. The current value for the property is stored in
    ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the complex class.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property to be configured.</typeparam>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <param name="buildAction">An action that performs configuration of the property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual ComplexCollectionBuilder<TComplex> ComplexProperty<TProperty>(
        string propertyName,
        Action<ComplexPropertyBuilder<TProperty>> buildAction)
        where TProperty : notnull
        => (ComplexCollectionBuilder<TComplex>)base.ComplexProperty(propertyName, buildAction);

    /// <summary>
    ///     Configures a complex property of the complex type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new property, if a property with the same name exists in the complex class
    ///     then it will be added to the model. If no property exists in the complex class, then
    ///     a new shadow state complex property will be added. A shadow state property is one that does not have a
    ///     corresponding property in the complex class. The current value for the property is stored in
    ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the complex class.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property to be configured.</typeparam>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <param name="complexTypeName">The name of the complex type.</param>
    /// <param name="buildAction">An action that performs configuration of the property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual ComplexCollectionBuilder<TComplex> ComplexProperty<TProperty>(
        string propertyName,
        string complexTypeName,
        Action<ComplexPropertyBuilder<TProperty>> buildAction)
        where TProperty : notnull
        => (ComplexCollectionBuilder<TComplex>)base.ComplexProperty(propertyName, complexTypeName, buildAction);

    /// <summary>
    ///     Configures a complex property of the complex type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new complex property, if a property with the same name exists in the complex class
    ///     then it will be added to the model. If no property exists in the complex class, then
    ///     a new shadow state complex property will be added. A shadow state property is one that does not have a
    ///     corresponding property in the complex class. The current value for the property is stored in
    ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the complex class.
    /// </remarks>
    /// <param name="propertyType">The type of the property to be configured.</param>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <param name="buildAction">An action that performs configuration of the property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual ComplexCollectionBuilder<TComplex> ComplexProperty(
        Type propertyType,
        string propertyName,
        Action<ComplexPropertyBuilder> buildAction)
        => (ComplexCollectionBuilder<TComplex>)base.ComplexProperty(propertyType, propertyName, buildAction);

    /// <summary>
    ///     Configures a complex property of the complex type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new complex property, if a property with the same name exists in the complex class
    ///     then it will be added to the model. If no property exists in the complex class, then
    ///     a new shadow state complex property will be added. A shadow state property is one that does not have a
    ///     corresponding property in the complex class. The current value for the property is stored in
    ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the complex class.
    /// </remarks>
    /// <param name="propertyType">The type of the property to be configured.</param>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <param name="complexTypeName">The name of the complex type.</param>
    /// <param name="buildAction">An action that performs configuration of the property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual ComplexCollectionBuilder<TComplex> ComplexProperty(
        Type propertyType,
        string propertyName,
        string complexTypeName,
        Action<ComplexPropertyBuilder> buildAction)
        => (ComplexCollectionBuilder<TComplex>)base.ComplexProperty(propertyType, propertyName, complexTypeName, buildAction);

    /// <summary>
    ///     Returns an object that can be used to configure a complex property of the complex type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new property, if a property with the same name exists in the complex class
    ///     then it will be added to the model. If no property exists in the complex class, then
    ///     a new shadow state complex property will be added. A shadow state property is one that does not have a
    ///     corresponding property in the complex class. The current value for the property is stored in
    ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the complex class.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property to be configured.</typeparam>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (
    ///     <c>blog => blog.Url</c>).
    /// </param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(
        Expression<Func<TComplex, TProperty?>> propertyExpression)
        where TProperty : notnull
        => new(
            TypeBuilder.ComplexProperty(
                Check.NotNull(propertyExpression, nameof(propertyExpression)).GetMemberAccess(),
                complexTypeName: null,
                collection: false,
                ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a complex property of the complex type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new property, if a property with the same name exists in the complex class
    ///     then it will be added to the model. If no property exists in the complex class, then
    ///     a new shadow state complex property will be added. A shadow state property is one that does not have a
    ///     corresponding property in the complex class. The current value for the property is stored in
    ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the complex class.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property to be configured.</typeparam>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (
    ///     <c>blog => blog.Url</c>).
    /// </param>
    /// <param name="complexTypeName">The name of the complex type.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(
        Expression<Func<TComplex, TProperty?>> propertyExpression,
        string complexTypeName)
        where TProperty : notnull
        => new(
            TypeBuilder.ComplexProperty(
                Check.NotNull(propertyExpression, nameof(propertyExpression)).GetMemberAccess(),
                Check.NotEmpty(complexTypeName, nameof(complexTypeName)),
                collection: false,
                ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Configures a complex property of the complex type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new property, if a property with the same name exists in the complex class
    ///     then it will be added to the model. If no property exists in the complex class, then
    ///     a new shadow state complex property will be added. A shadow state property is one that does not have a
    ///     corresponding property in the complex class. The current value for the property is stored in
    ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the complex class.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property to be configured.</typeparam>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (
    ///     <c>blog => blog.Url</c>).
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ComplexCollectionBuilder<TComplex> ComplexProperty<TProperty>(
        Expression<Func<TComplex, TProperty?>> propertyExpression,
        Action<ComplexPropertyBuilder<TProperty>> buildAction)
        where TProperty : notnull
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(ComplexProperty(propertyExpression));

        return this;
    }

    /// <summary>
    ///     Configures a complex property of the complex type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new property, if a property with the same name exists in the complex class
    ///     then it will be added to the model. If no property exists in the complex class, then
    ///     a new shadow state complex property will be added. A shadow state property is one that does not have a
    ///     corresponding property in the complex class. The current value for the property is stored in
    ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the complex class.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property to be configured.</typeparam>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (
    ///     <c>blog => blog.Url</c>).
    /// </param>
    /// <param name="complexTypeName">The name of the complex type.</param>
    /// <param name="buildAction">An action that performs configuration of the property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ComplexCollectionBuilder<TComplex> ComplexProperty<TProperty>(
        Expression<Func<TComplex, TProperty?>> propertyExpression,
        string complexTypeName,
        Action<ComplexPropertyBuilder<TProperty>> buildAction)
        where TProperty : notnull
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(ComplexProperty(propertyExpression, complexTypeName));

        return this;
    }

    /// <summary>
    ///     Configures a complex collection of the complex type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new property with this overload the property name must match the
    ///     name of a CLR property or field on the complex type. This overload cannot be used to
    ///     add a new shadow state complex property.
    /// </remarks>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <param name="buildAction">An action that performs configuration of the property.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public new virtual ComplexCollectionBuilder<TComplex> ComplexCollection(string propertyName, Action<ComplexCollectionBuilder> buildAction)
        => (ComplexCollectionBuilder<TComplex>)base.ComplexCollection(propertyName, buildAction);

    /// <summary>
    ///     Configures a complex collection of the complex type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new property, if a property with the same name exists in the complex class
    ///     then it will be added to the model. If no property exists in the complex class, then
    ///     a new shadow state complex property will be added. A shadow state property is one that does not have a
    ///     corresponding property in the complex class. The current value for the property is stored in
    ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the complex class.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property to be configured.</typeparam>
    /// <typeparam name="TElement">The element type.</typeparam>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <param name="buildAction">An action that performs configuration of the property.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public new virtual ComplexCollectionBuilder<TComplex> ComplexCollection<TProperty, TElement>(
        string propertyName, Action<ComplexCollectionBuilder<TElement>> buildAction)
        where TProperty : IEnumerable<TElement>
        where TElement : notnull
        => (ComplexCollectionBuilder<TComplex>)base.ComplexCollection<TProperty, TElement>(propertyName, buildAction);

    /// <summary>
    ///     Configures a complex collection of the complex type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new property, if a property with the same name exists in the complex class
    ///     then it will be added to the model. If no property exists in the complex class, then
    ///     a new shadow state complex property will be added. A shadow state property is one that does not have a
    ///     corresponding property in the complex class. The current value for the property is stored in
    ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the complex class.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property to be configured.</typeparam>
    /// <typeparam name="TElement">The element type.</typeparam>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <param name="complexTypeName">The name of the complex type.</param>
    /// <param name="buildAction">An action that performs configuration of the property.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public new virtual ComplexCollectionBuilder<TComplex> ComplexCollection<TProperty, TElement>(
        string propertyName,
        string complexTypeName,
        Action<ComplexCollectionBuilder<TElement>> buildAction)
        where TProperty : IEnumerable<TElement>
        where TElement : notnull
        => (ComplexCollectionBuilder<TComplex>)base.ComplexCollection<TProperty, TElement>(propertyName, complexTypeName, buildAction);

    /// <summary>
    ///     Configures a complex collection of the complex type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new complex property, if a property with the same name exists in the complex class
    ///     then it will be added to the model. If no property exists in the complex class, then
    ///     a new shadow state complex property will be added. A shadow state property is one that does not have a
    ///     corresponding property in the complex class. The current value for the property is stored in
    ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the complex class.
    /// </remarks>
    /// <param name="propertyType">The type of the property to be configured.</param>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <param name="buildAction">An action that performs configuration of the property.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public new virtual ComplexCollectionBuilder<TComplex> ComplexCollection(Type propertyType, string propertyName, Action<ComplexCollectionBuilder> buildAction)
        => (ComplexCollectionBuilder<TComplex>)base.ComplexCollection(propertyType, propertyName, buildAction);

    /// <summary>
    ///     Configures a complex collection of the complex type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new complex property, if a property with the same name exists in the complex class
    ///     then it will be added to the model. If no property exists in the complex class, then
    ///     a new shadow state complex property will be added. A shadow state property is one that does not have a
    ///     corresponding property in the complex class. The current value for the property is stored in
    ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the complex class.
    /// </remarks>
    /// <param name="propertyType">The type of the property to be configured.</param>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <param name="complexTypeName">The name of the complex type.</param>
    /// <param name="buildAction">An action that performs configuration of the property.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public new virtual ComplexCollectionBuilder<TComplex> ComplexCollection(
        Type propertyType, string propertyName, string complexTypeName, Action<ComplexCollectionBuilder> buildAction)
        => (ComplexCollectionBuilder<TComplex>)base.ComplexCollection(propertyType, complexTypeName, propertyName, buildAction);

    /// <summary>
    ///     Returns an object that can be used to configure a complex collection property of the complex type.
    ///     If the specified property is not already part of the model, it will be added.
    /// </summary>
    /// <typeparam name="TElement">The element type.</typeparam>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (
    ///     <c>blog => blog.Url</c>).
    /// </param>
    /// <returns>An object that can be used to configure the complex collection property.</returns>
    public virtual ComplexCollectionBuilder<TElement> ComplexCollection<TElement>(
        Expression<Func<TComplex, IEnumerable<TElement?>?>> propertyExpression)
        where TElement : notnull
        => new(
            TypeBuilder.ComplexProperty(
                Check.NotNull(propertyExpression, nameof(propertyExpression)).GetMemberAccess(),
                complexTypeName: null,
                collection: true,
                ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a complex collection property of the complex type.
    ///     If the specified property is not already part of the model, it will be added.
    /// </summary>
    /// <typeparam name="TElement">The element type.</typeparam>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (
    ///     <c>blog => blog.Url</c>).
    /// </param>
    /// <param name="complexTypeName">The name of the complex type.</param>
    /// <returns>An object that can be used to configure the complex collection property.</returns>
    public virtual ComplexCollectionBuilder<TElement> ComplexCollection<TElement>(
        Expression<Func<TComplex, IEnumerable<TElement?>?>> propertyExpression,
        string complexTypeName)
        where TElement : notnull
        => new(
            TypeBuilder.ComplexProperty(
                Check.NotNull(propertyExpression, nameof(propertyExpression)).GetMemberAccess(),
                Check.NotEmpty(complexTypeName, nameof(complexTypeName)),
                collection: true,
                ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Configures a complex collection property of the complex type.
    ///     If the specified property is not already part of the model, it will be added.
    /// </summary>
    /// <typeparam name="TElement">The element type.</typeparam>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (
    ///     <c>blog => blog.Url</c>).
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the property.</param>
    /// <returns>An object that can be used to configure the complex collection property.</returns>
    public virtual ComplexCollectionBuilder<TComplex> ComplexCollection<TElement>(
        Expression<Func<TComplex, IEnumerable<TElement?>?>> propertyExpression,
        Action<ComplexCollectionBuilder<TElement>> buildAction)
        where TElement : notnull
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(ComplexCollection(propertyExpression));

        return this;
    }

    /// <summary>
    ///     Configures a complex collection property of the complex type.
    ///     If the specified property is not already part of the model, it will be added.
    /// </summary>
    /// <typeparam name="TElement">The element type.</typeparam>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (
    ///     <c>blog => blog.Url</c>).
    /// </param>
    /// <param name="complexTypeName">The name of the complex type.</param>
    /// <param name="buildAction">An action that performs configuration of the property.</param>
    /// <returns>An object that can be used to configure the complex collection property.</returns>
    public virtual ComplexCollectionBuilder<TComplex> ComplexCollection<TElement>(
        Expression<Func<TComplex, IEnumerable<TElement?>?>> propertyExpression,
        string complexTypeName,
        Action<ComplexCollectionBuilder<TElement>> buildAction)
        where TElement : notnull
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(ComplexCollection(propertyExpression, complexTypeName));

        return this;
    }

    /// <summary>
    ///     Excludes the given property from the entity type. This method is typically used to remove properties
    ///     or navigations from the entity type that were added by convention.
    /// </summary>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be ignored
    ///     (<c>blog => blog.Url</c>).
    /// </param>
    public virtual ComplexCollectionBuilder<TComplex> Ignore(Expression<Func<TComplex, object?>> propertyExpression)
        => (ComplexCollectionBuilder<TComplex>)base.Ignore(
            Check.NotNull(propertyExpression, nameof(propertyExpression)).GetMemberAccess().GetSimpleMemberName());

    /// <summary>
    ///     Excludes the given property from the entity type. This method is typically used to remove properties
    ///     or navigations from the entity type that were added by convention.
    /// </summary>
    /// <param name="propertyName">The name of the property to be removed from the entity type.</param>
    public new virtual ComplexCollectionBuilder<TComplex> Ignore(string propertyName)
        => (ComplexCollectionBuilder<TComplex>)base.Ignore(propertyName);

    /// <summary>
    ///     Sets the backing field to use for this property.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Backing fields are normally found by convention.
    ///         This method is useful for setting backing fields explicitly in cases where the
    ///         correct field is not found by convention.
    ///     </para>
    ///     <para>
    ///         By default, the backing field, if one is found or has been specified, is used when
    ///         new objects are constructed, typically when entities are queried from the database.
    ///         Properties are used for all other accesses. This can be changed by calling
    ///         <see cref="UsePropertyAccessMode" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-backing-fields">Backing fields</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="fieldName">The field name.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual ComplexCollectionBuilder<TComplex> HasField(string fieldName)
        => (ComplexCollectionBuilder<TComplex>)base.HasField(fieldName);

    /// <summary>
    ///     Configures the <see cref="ChangeTrackingStrategy" /> to be used for this complex type.
    ///     This strategy indicates how the context detects changes to properties for an instance of the complex type.
    /// </summary>
    /// <param name="changeTrackingStrategy">The change tracking strategy to be used.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual ComplexCollectionBuilder<TComplex> HasChangeTrackingStrategy(ChangeTrackingStrategy changeTrackingStrategy)
        => (ComplexCollectionBuilder<TComplex>)base.HasChangeTrackingStrategy(changeTrackingStrategy);

    /// <summary>
    ///     Sets the <see cref="PropertyAccessMode" /> to use for this property.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, the backing field, if one is found by convention or has been specified, is used when
    ///         new objects are constructed, typically when entities are queried from the database.
    ///         Properties are used for all other accesses. Calling this method will change that behavior
    ///         for this property as described in the <see cref="PropertyAccessMode" /> enum.
    ///     </para>
    ///     <para>
    ///         Calling this method overrides for this property any access mode that was set on the
    ///         entity type or model.
    ///     </para>
    /// </remarks>
    /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" /> to use for this property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual ComplexCollectionBuilder<TComplex> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
        => (ComplexCollectionBuilder<TComplex>)base.UsePropertyAccessMode(propertyAccessMode);

    /// <summary>
    ///     Sets the <see cref="PropertyAccessMode" /> to use for all properties of this complex type.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, the backing field, if one is found by convention or has been specified, is used when
    ///         new objects are constructed, typically when entities are queried from the database.
    ///         Properties are used for all other accesses.  Calling this method will change that behavior
    ///         for all properties of this complex type as described in the <see cref="PropertyAccessMode" /> enum.
    ///     </para>
    ///     <para>
    ///         Calling this method overrides for all properties of this complex type any access mode that was
    ///         set on the model.
    ///     </para>
    /// </remarks>
    /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" /> to use for properties of this complex type.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual ComplexCollectionBuilder<TComplex> UseDefaultPropertyAccessMode(PropertyAccessMode propertyAccessMode)
        => (ComplexCollectionBuilder<TComplex>)base.UseDefaultPropertyAccessMode(propertyAccessMode);
}
