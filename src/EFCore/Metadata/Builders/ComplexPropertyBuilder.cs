// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring an <see cref="IMutableComplexProperty" />.
/// </summary>
/// <remarks>
///     Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///     and it is not designed to be directly constructed in your application code.
/// </remarks>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class ComplexPropertyBuilder :
    IInfrastructure<IConventionComplexPropertyBuilder>,
    IInfrastructure<IConventionComplexTypeBuilder>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public ComplexPropertyBuilder(IMutableComplexProperty complexProperty)
    {
        PropertyBuilder = ((ComplexProperty)complexProperty).Builder;
        TypeBuilder = ((ComplexProperty)complexProperty).ComplexType.Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalComplexPropertyBuilder PropertyBuilder { [DebuggerStepThrough] get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalComplexTypeBuilder TypeBuilder { [DebuggerStepThrough] get; }

    /// <summary>
    ///     Gets the internal builder being used to configure the complex property.
    /// </summary>
    IConventionComplexPropertyBuilder IInfrastructure<IConventionComplexPropertyBuilder>.Instance
        => PropertyBuilder;

    /// <summary>
    ///     Gets the internal builder being used to configure the complex type.
    /// </summary>
    IConventionComplexTypeBuilder IInfrastructure<IConventionComplexTypeBuilder>.Instance
        => TypeBuilder;

    /// <summary>
    ///     The complex property being configured.
    /// </summary>
    public virtual IMutableComplexProperty Metadata
        => PropertyBuilder.Metadata;

    /// <summary>
    ///     Adds or updates an annotation on the complex property. If an annotation with the key specified in
    ///     <paramref name="annotation" /> already exists its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ComplexPropertyBuilder HasPropertyAnnotation(string annotation, object? value)
    {
        Check.NotEmpty(annotation, nameof(annotation));

        PropertyBuilder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Adds or updates an annotation on the complex type. If an annotation with the key specified in
    ///     <paramref name="annotation" /> already exists its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ComplexPropertyBuilder HasTypeAnnotation(string annotation, object? value)
    {
        Check.NotEmpty(annotation, nameof(annotation));

        TypeBuilder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Configures whether this property must have a value assigned or <see langword="null" /> is a valid value.
    ///     A property can only be configured as non-required if it is based on a CLR type that can be
    ///     assigned <see langword="null" />.
    /// </summary>
    /// <param name="required">A value indicating whether the property is required.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ComplexPropertyBuilder IsRequired(bool required = true)
    {
        PropertyBuilder.IsRequired(required, ConfigurationSource.Explicit);

        return this;
    }

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
    public virtual ComplexPropertyBuilder HasField(string fieldName)
    {
        Check.NotEmpty(fieldName, nameof(fieldName));

        PropertyBuilder.HasField(fieldName, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Returns an object that can be used to configure a property of the complex type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new property with this overload the property name must match the
    ///     name of a CLR property or field on the complex type. This overload cannot be used to
    ///     add a new shadow state property.
    /// </remarks>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ComplexTypePropertyBuilder Property(string propertyName)
        => new(
            TypeBuilder.Property(
                Check.NotEmpty(propertyName, nameof(propertyName)),
                ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a property of the complex type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new property, if a property with the same name exists in the entity class
    ///     then it will be added to the model. If no property exists in the entity class, then
    ///     a new shadow state property will be added. A shadow state property is one that does not have a
    ///     corresponding property in the entity class. The current value for the property is stored in
    ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the entity class.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property to be configured.</typeparam>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ComplexTypePropertyBuilder<TProperty> Property<TProperty>(string propertyName)
        => new(
            TypeBuilder.Property(
                typeof(TProperty),
                Check.NotEmpty(propertyName, nameof(propertyName)), ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a property of the complex type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new property, if a property with the same name exists in the entity class
    ///     then it will be added to the model. If no property exists in the entity class, then
    ///     a new shadow state property will be added. A shadow state property is one that does not have a
    ///     corresponding property in the entity class. The current value for the property is stored in
    ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the entity class.
    /// </remarks>
    /// <param name="propertyType">The type of the property to be configured.</param>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ComplexTypePropertyBuilder Property(Type propertyType, string propertyName)
        => new(
            TypeBuilder.Property(
                Check.NotNull(propertyType, nameof(propertyType)),
                Check.NotEmpty(propertyName, nameof(propertyName)), ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a property of the complex type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new property with this overload the property name must match the
    ///     name of a CLR property or field on the complex type. This overload cannot be used to
    ///     add a new shadow state property.
    /// </remarks>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ComplexTypePrimitiveCollectionBuilder PrimitiveCollection(string propertyName)
        => new(
            TypeBuilder.PrimitiveCollection(
                Check.NotEmpty(propertyName, nameof(propertyName)),
                ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a property of the complex type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new property, if a property with the same name exists in the entity class
    ///     then it will be added to the model. If no property exists in the entity class, then
    ///     a new shadow state property will be added. A shadow state property is one that does not have a
    ///     corresponding property in the entity class. The current value for the property is stored in
    ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the entity class.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property to be configured.</typeparam>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ComplexTypePrimitiveCollectionBuilder<TProperty> PrimitiveCollection<TProperty>(string propertyName)
        => new(
            TypeBuilder.PrimitiveCollection(
                typeof(TProperty),
                Check.NotEmpty(propertyName, nameof(propertyName)), ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a property of the complex type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new property, if a property with the same name exists in the entity class
    ///     then it will be added to the model. If no property exists in the entity class, then
    ///     a new shadow state property will be added. A shadow state property is one that does not have a
    ///     corresponding property in the entity class. The current value for the property is stored in
    ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the entity class.
    /// </remarks>
    /// <param name="propertyType">The type of the property to be configured.</param>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ComplexTypePrimitiveCollectionBuilder PrimitiveCollection(Type propertyType, string propertyName)
        => new(
            TypeBuilder.PrimitiveCollection(
                Check.NotNull(propertyType, nameof(propertyType)),
                Check.NotEmpty(propertyName, nameof(propertyName)), ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a property of the complex type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     Indexer properties are stored in the entity using
    ///     <see href="https://docs.microsoft.com/dotnet/csharp/programming-guide/indexers/">an indexer</see>
    ///     supplying the provided property name.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property to be configured.</typeparam>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ComplexTypePropertyBuilder<TProperty> IndexerProperty
        <[DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] TProperty>(string propertyName)
        => new(
            TypeBuilder.IndexerProperty(
                typeof(TProperty),
                Check.NotEmpty(propertyName, nameof(propertyName)), ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a property of the complex type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     Indexer properties are stored in the entity using
    ///     <see href="https://docs.microsoft.com/dotnet/csharp/programming-guide/indexers/">an indexer</see>
    ///     supplying the provided property name.
    /// </remarks>
    /// <param name="propertyType">The type of the property to be configured.</param>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ComplexTypePropertyBuilder IndexerProperty(
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type propertyType,
        string propertyName)
    {
        Check.NotNull(propertyType, nameof(propertyType));

        return new ComplexTypePropertyBuilder(
            TypeBuilder.IndexerProperty(
                propertyType,
                Check.NotEmpty(propertyName, nameof(propertyName)), ConfigurationSource.Explicit)!.Metadata);
    }

    /// <summary>
    ///     Returns an object that can be used to configure a complex property of the complex type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new property with this overload the property name must match the
    ///     name of a CLR property or field on the complex type. This overload cannot be used to
    ///     add a new shadow state complex property.
    /// </remarks>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ComplexPropertyBuilder ComplexProperty(string propertyName)
        => new(
            TypeBuilder.ComplexProperty(
                propertyType: null,
                Check.NotEmpty(propertyName, nameof(propertyName)),
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
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(string propertyName)
        => new(
            TypeBuilder.ComplexProperty(
                typeof(TProperty),
                Check.NotEmpty(propertyName, nameof(propertyName)),
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
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <param name="complexTypeName">The name of the complex type.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(string propertyName, string complexTypeName)
        => new(
            TypeBuilder.ComplexProperty(
                typeof(TProperty),
                Check.NotEmpty(propertyName, nameof(propertyName)),
                Check.NotEmpty(complexTypeName, nameof(complexTypeName)),
                collection: false,
                ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a complex property of the complex type.
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
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ComplexPropertyBuilder ComplexProperty(Type propertyType, string propertyName)
        => new(
            TypeBuilder.ComplexProperty(
                Check.NotNull(propertyType, nameof(propertyType)),
                Check.NotEmpty(propertyName, nameof(propertyName)),
                complexTypeName: null,
                collection: false,
                ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a complex property of the complex type.
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
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ComplexPropertyBuilder ComplexProperty(Type propertyType, string propertyName, string complexTypeName)
        => new(
            TypeBuilder.ComplexProperty(
                Check.NotNull(propertyType, nameof(propertyType)),
                Check.NotEmpty(propertyName, nameof(propertyName)),
                Check.NotEmpty(complexTypeName, nameof(complexTypeName)),
                collection: false,
                ConfigurationSource.Explicit)!.Metadata);

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
    public virtual ComplexPropertyBuilder ComplexProperty(string propertyName, Action<ComplexPropertyBuilder> buildAction)
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(ComplexProperty(propertyName));

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
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <param name="buildAction">An action that performs configuration of the property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ComplexPropertyBuilder ComplexProperty<TProperty>(
        string propertyName,
        Action<ComplexPropertyBuilder<TProperty>> buildAction)
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(ComplexProperty<TProperty>(propertyName));

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
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <param name="complexTypeName">The name of the complex type.</param>
    /// <param name="buildAction">An action that performs configuration of the property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ComplexPropertyBuilder ComplexProperty<TProperty>(
        string propertyName,
        string complexTypeName,
        Action<ComplexPropertyBuilder<TProperty>> buildAction)
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(ComplexProperty<TProperty>(propertyName, complexTypeName));

        return this;
    }

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
    public virtual ComplexPropertyBuilder ComplexProperty(
        Type propertyType,
        string propertyName,
        Action<ComplexPropertyBuilder> buildAction)
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(ComplexProperty(propertyType, propertyName));

        return this;
    }

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
    public virtual ComplexPropertyBuilder ComplexProperty(
        Type propertyType,
        string propertyName,
        string complexTypeName,
        Action<ComplexPropertyBuilder> buildAction)
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(ComplexProperty(propertyType, propertyName, complexTypeName));

        return this;
    }

    /// <summary>
    ///     Excludes the given property from the complex type. This method is typically used to remove properties
    ///     and navigations from the complex type that were added by convention.
    /// </summary>
    /// <param name="propertyName">The name of the property to be removed from the complex type.</param>
    public virtual ComplexPropertyBuilder Ignore(string propertyName)
    {
        Check.NotEmpty(propertyName, nameof(propertyName));

        TypeBuilder.Ignore(propertyName, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Configures the <see cref="ChangeTrackingStrategy" /> to be used for this entity type.
    ///     This strategy indicates how the context detects changes to properties for an instance of the entity type.
    /// </summary>
    /// <param name="changeTrackingStrategy">The change tracking strategy to be used.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual ComplexPropertyBuilder HasChangeTrackingStrategy(ChangeTrackingStrategy changeTrackingStrategy)
    {
        TypeBuilder.HasChangeTrackingStrategy(changeTrackingStrategy, ConfigurationSource.Explicit);

        return this;
    }

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
    public virtual ComplexPropertyBuilder UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
    {
        PropertyBuilder.UsePropertyAccessMode(propertyAccessMode, ConfigurationSource.Explicit);

        return this;
    }

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
    public virtual ComplexPropertyBuilder UseDefaultPropertyAccessMode(PropertyAccessMode propertyAccessMode)
    {
        TypeBuilder.UsePropertyAccessMode(propertyAccessMode, ConfigurationSource.Explicit);

        return this;
    }

    #region Hidden System.Object members

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString()
        => base.ToString();

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once BaseObjectEqualsIsObjectEquals
    public override bool Equals(object? obj)
        => base.Equals(obj);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
    public override int GetHashCode()
        => base.GetHashCode();

    #endregion
}
