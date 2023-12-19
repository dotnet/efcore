// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
public class EntityTypeBuilder : IInfrastructure<IConventionEntityTypeBuilder>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public EntityTypeBuilder(IMutableEntityType entityType)
    {
        Builder = ((EntityType)entityType).Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalEntityTypeBuilder Builder { [DebuggerStepThrough] get; }

    /// <summary>
    ///     Gets the internal builder being used to configure the entity type.
    /// </summary>
    IConventionEntityTypeBuilder IInfrastructure<IConventionEntityTypeBuilder>.Instance
        => Builder;

    /// <summary>
    ///     The entity type being configured.
    /// </summary>
    public virtual IMutableEntityType Metadata
        => Builder.Metadata;

    /// <summary>
    ///     Adds or updates an annotation on the entity type. If an annotation with the key specified in
    ///     <paramref name="annotation" /> already exists its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder HasAnnotation(string annotation, object? value)
    {
        Check.NotEmpty(annotation, nameof(annotation));

        Builder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Sets the base type of this entity type in an inheritance hierarchy.
    /// </summary>
    /// <param name="name">The name of the base type or <see langword="null" /> to indicate no base type.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder HasBaseType(string? name)
        => new(Builder.HasBaseType(name, ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Sets the base type of this entity type in an inheritance hierarchy.
    /// </summary>
    /// <param name="entityType">The base type or <see langword="null" /> to indicate no base type.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder HasBaseType(Type? entityType)
        => new(Builder.HasBaseType(entityType, ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Sets the properties that make up the primary key for this entity type.
    /// </summary>
    /// <param name="propertyNames">The names of the properties that make up the primary key.</param>
    /// <returns>An object that can be used to configure the primary key.</returns>
    public virtual KeyBuilder HasKey(params string[] propertyNames)
        => new(Builder.PrimaryKey(Check.NotEmpty(propertyNames, nameof(propertyNames)), ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Creates an alternate key in the model for this entity type if one does not already exist over the specified
    ///     properties. This will force the properties to be read-only. Use <see cref="O:HasIndex" /> to specify uniqueness
    ///     in the model that does not force properties to be read-only.
    /// </summary>
    /// <param name="propertyNames">The names of the properties that make up the key.</param>
    /// <returns>An object that can be used to configure the key.</returns>
    public virtual KeyBuilder HasAlternateKey(params string[] propertyNames)
        => new(Builder.HasKey(Check.NotEmpty(propertyNames, nameof(propertyNames)), ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Configures the entity type to have no keys. It will only be usable for queries.
    /// </summary>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder HasNoKey()
    {
        Builder.HasNoKey(ConfigurationSource.Explicit);
        return this;
    }

    /// <summary>
    ///     Returns an object that can be used to configure a property of the entity type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new property with this overload the property name must match the
    ///     name of a CLR property or field on the entity type. This overload cannot be used to
    ///     add a new shadow state property.
    /// </remarks>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual PropertyBuilder Property(string propertyName)
        => new(
            Builder.Property(
                Check.NotEmpty(propertyName, nameof(propertyName)),
                ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a property of the entity type.
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
    public virtual PropertyBuilder<TProperty> Property<TProperty>(string propertyName)
        => new(
            Builder.Property(
                typeof(TProperty),
                Check.NotEmpty(propertyName, nameof(propertyName)), ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a property of the entity type.
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
    public virtual PropertyBuilder Property(Type propertyType, string propertyName)
        => new(
            Builder.Property(
                Check.NotNull(propertyType, nameof(propertyType)),
                Check.NotEmpty(propertyName, nameof(propertyName)), ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a property of the entity type where that property represents
    ///     a collection of primitive values, such as strings or integers.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new property with this overload the property name must match the
    ///     name of a CLR property or field on the entity type. This overload cannot be used to
    ///     add a new shadow state property.
    /// </remarks>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual PrimitiveCollectionBuilder PrimitiveCollection(string propertyName)
        => new(
            Builder.PrimitiveCollection(
                Check.NotEmpty(propertyName, nameof(propertyName)), ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a property of the entity type where that property represents
    ///     a collection of primitive values, such as strings or integers.
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
    public virtual PrimitiveCollectionBuilder<TProperty> PrimitiveCollection<TProperty>(string propertyName)
        => new(
            Builder.PrimitiveCollection(
                typeof(TProperty),
                Check.NotEmpty(propertyName, nameof(propertyName)), ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a property of the entity type where that property represents
    ///     a collection of primitive values, such as strings or integers.
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
    public virtual PrimitiveCollectionBuilder PrimitiveCollection(Type propertyType, string propertyName)
        => new(
            Builder.PrimitiveCollection(
                Check.NotNull(propertyType, nameof(propertyType)),
                Check.NotEmpty(propertyName, nameof(propertyName)), ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a property of the entity type.
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
    public virtual PropertyBuilder<TProperty> IndexerProperty
        <[DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] TProperty>(string propertyName)
        => new(
            Builder.IndexerProperty(
                typeof(TProperty),
                Check.NotEmpty(propertyName, nameof(propertyName)), ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a property of the entity type.
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
    public virtual PropertyBuilder IndexerProperty(
        [DynamicallyAccessedMembers(IProperty.DynamicallyAccessedMemberTypes)] Type propertyType,
        string propertyName)
    {
        Check.NotNull(propertyType, nameof(propertyType));

        return new PropertyBuilder(
            Builder.IndexerProperty(
                propertyType,
                Check.NotEmpty(propertyName, nameof(propertyName)), ConfigurationSource.Explicit)!.Metadata);
    }

    /// <summary>
    ///     Returns an object that can be used to configure a complex property of the entity type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new property with this overload the property name must match the
    ///     name of a CLR property or field on the entity type. This overload cannot be used to
    ///     add a new shadow state complex property.
    /// </remarks>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ComplexPropertyBuilder ComplexProperty(string propertyName)
        => new(
            Builder.ComplexProperty(
                propertyType: null,
                Check.NotEmpty(propertyName, nameof(propertyName)),
                complexTypeName: null,
                collection: false,
                ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a complex property of the entity type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new property, if a property with the same name exists in the entity class
    ///     then it will be added to the model. If no property exists in the entity class, then
    ///     a new shadow state complex property will be added. A shadow state property is one that does not have a
    ///     corresponding property in the entity class. The current value for the property is stored in
    ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the entity class.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property to be configured.</typeparam>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(string propertyName)
        => new(
            Builder.ComplexProperty(
                typeof(TProperty),
                Check.NotEmpty(propertyName, nameof(propertyName)),
                complexTypeName: null,
                collection: false,
                ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a complex property of the entity type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new property, if a property with the same name exists in the entity class
    ///     then it will be added to the model. If no property exists in the entity class, then
    ///     a new shadow state complex property will be added. A shadow state property is one that does not have a
    ///     corresponding property in the entity class. The current value for the property is stored in
    ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the entity class.
    /// </remarks>
    /// <typeparam name="TProperty">The type of the property to be configured.</typeparam>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <param name="complexTypeName">The name of the complex type.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(
        string propertyName,
        string complexTypeName)
        => new(
            Builder.ComplexProperty(
                typeof(TProperty),
                Check.NotEmpty(propertyName, nameof(propertyName)),
                Check.NotEmpty(complexTypeName, nameof(complexTypeName)),
                collection: false,
                ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Configures a complex property of the entity type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new complex property, if a property with the same name exists in the entity class
    ///     then it will be added to the model. If no property exists in the entity class, then
    ///     a new shadow state complex property will be added. A shadow state property is one that does not have a
    ///     corresponding property in the entity class. The current value for the property is stored in
    ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the entity class.
    /// </remarks>
    /// <param name="propertyType">The type of the property to be configured.</param>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ComplexPropertyBuilder ComplexProperty(Type propertyType, string propertyName)
        => new(
            Builder.ComplexProperty(
                Check.NotNull(propertyType, nameof(propertyType)),
                Check.NotEmpty(propertyName, nameof(propertyName)),
                complexTypeName: null,
                collection: false,
                ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Configures a complex property of the entity type.
    ///     If no property with the given name exists, then a new property will be added.
    /// </summary>
    /// <remarks>
    ///     When adding a new complex property, if a property with the same name exists in the entity class
    ///     then it will be added to the model. If no property exists in the entity class, then
    ///     a new shadow state complex property will be added. A shadow state property is one that does not have a
    ///     corresponding property in the entity class. The current value for the property is stored in
    ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the entity class.
    /// </remarks>
    /// <param name="propertyType">The type of the property to be configured.</param>
    /// <param name="propertyName">The name of the property to be configured.</param>
    /// <param name="complexTypeName">The name of the complex type.</param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual ComplexPropertyBuilder ComplexProperty(
        Type propertyType,
        string propertyName,
        string complexTypeName)
        => new(
            Builder.ComplexProperty(
                Check.NotNull(propertyType, nameof(propertyType)),
                Check.NotEmpty(propertyName, nameof(propertyName)),
                Check.NotEmpty(complexTypeName, nameof(complexTypeName)),
                collection: false,
                ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Configures a complex property of the entity type.
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
    public virtual EntityTypeBuilder ComplexProperty(string propertyName, Action<ComplexPropertyBuilder> buildAction)
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(ComplexProperty(propertyName));

        return this;
    }

    /// <summary>
    ///     Configures a complex property of the entity type.
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
    public virtual EntityTypeBuilder ComplexProperty<TProperty>(
        string propertyName,
        Action<ComplexPropertyBuilder<TProperty>> buildAction)
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(ComplexProperty<TProperty>(propertyName));

        return this;
    }

    /// <summary>
    ///     Configures a complex property of the entity type.
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
    public virtual EntityTypeBuilder ComplexProperty<TProperty>(
        string propertyName,
        string complexTypeName,
        Action<ComplexPropertyBuilder<TProperty>> buildAction)
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(ComplexProperty<TProperty>(propertyName, complexTypeName));

        return this;
    }

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
    /// <param name="buildAction">An action that performs configuration of the property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder ComplexProperty(Type propertyType, string propertyName, Action<ComplexPropertyBuilder> buildAction)
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(ComplexProperty(propertyType, propertyName));

        return this;
    }

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
    /// <param name="buildAction">An action that performs configuration of the property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder ComplexProperty(
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
    ///     Returns an object that can be used to configure an existing navigation property of the entity type.
    ///     It is an error for the navigation property not to exist.
    /// </summary>
    /// <param name="navigationName">The name of the navigation property to be configured.</param>
    /// <returns>An object that can be used to configure the navigation property.</returns>
    public virtual NavigationBuilder Navigation(string navigationName)
        => new(Builder.Navigation(Check.NotEmpty(navigationName, nameof(navigationName))));

    /// <summary>
    ///     Excludes the given property from the entity type. This method is typically used to remove properties
    ///     and navigations from the entity type that were added by convention.
    /// </summary>
    /// <param name="propertyName">The name of the property to be removed from the entity type.</param>
    public virtual EntityTypeBuilder Ignore(string propertyName)
    {
        Check.NotEmpty(propertyName, nameof(propertyName));

        Builder.Ignore(propertyName, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Specifies a LINQ predicate expression that will automatically be applied to any queries targeting
    ///     this entity type.
    /// </summary>
    /// <param name="filter">The LINQ predicate expression.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder HasQueryFilter(LambdaExpression? filter)
    {
        Builder.HasQueryFilter(filter, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Configures an unnamed index on the specified properties.
    ///     If there is an existing unnamed index on the given
    ///     list of properties, then the existing index will be
    ///     returned for configuration.
    /// </summary>
    /// <param name="propertyNames">The names of the properties that make up the index.</param>
    /// <returns>An object that can be used to configure the index.</returns>
    public virtual IndexBuilder HasIndex(params string[] propertyNames)
        => new(Builder.HasIndex(Check.NotEmpty(propertyNames, nameof(propertyNames)), ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Configures an index on the specified properties and with the given name.
    ///     If there is an existing index on the given list of properties and with
    ///     the given name, then the existing index will be returned for configuration.
    /// </summary>
    /// <param name="propertyNames">The names of the properties that make up the index.</param>
    /// <param name="name">The name to assign to the index.</param>
    /// <returns>An object that can be used to configure the index.</returns>
    public virtual IndexBuilder HasIndex(
        string[] propertyNames,
        string name)
        => new(
            Builder.HasIndex(
                Check.NotEmpty(propertyNames, nameof(propertyNames)),
                Check.NotEmpty(name, nameof(name)),
                ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The target entity type for each ownership relationship is treated as a different entity type
    ///         even if the navigation is of the same type. Configuration of the target entity type
    ///         isn't applied to the target entity type of other ownership relationships.
    ///     </para>
    ///     <para>
    ///         Most operations on an owned entity require accessing it through the owner entity using the corresponding navigation.
    ///     </para>
    ///     <para>
    ///         After calling this method, you should chain a call to
    ///         <see cref="OwnedNavigationBuilder.WithOwner" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <param name="ownedTypeName">The name of the entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <returns>An object that can be used to configure the owned type and the relationship.</returns>
    public virtual OwnedNavigationBuilder OwnsOne(
        string ownedTypeName,
        string navigationName)
        => OwnsOneBuilder(
            new TypeIdentity(Check.NotEmpty(ownedTypeName, nameof(ownedTypeName))),
            Check.NotEmpty(navigationName, nameof(navigationName)));

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The target entity type for each ownership relationship is treated as a different entity type
    ///         even if the navigation is of the same type. Configuration of the target entity type
    ///         isn't applied to the target entity type of other ownership relationships.
    ///     </para>
    ///     <para>
    ///         Most operations on an owned entity require accessing it through the owner entity using the corresponding navigation.
    ///     </para>
    ///     <para>
    ///         After calling this method, you should chain a call to
    ///         <see cref="OwnedNavigationBuilder.WithOwner" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <param name="ownedTypeName">The name of the entity type that this relationship targets.</param>
    /// <param name="ownedType">The CLR type of the entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <returns>An object that can be used to configure the owned type and the relationship.</returns>
    public virtual OwnedNavigationBuilder OwnsOne(
        string ownedTypeName,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type ownedType,
        string navigationName)
        => OwnsOneBuilder(
            new TypeIdentity(Check.NotEmpty(ownedTypeName, nameof(ownedTypeName)), ownedType),
            Check.NotEmpty(navigationName, nameof(navigationName)));

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The target entity type for each ownership relationship is treated as a different entity type
    ///         even if the navigation is of the same type. Configuration of the target entity type
    ///         isn't applied to the target entity type of other ownership relationships.
    ///     </para>
    ///     <para>
    ///         Most operations on an owned entity require accessing it through the owner entity using the corresponding navigation.
    ///     </para>
    ///     <para>
    ///         After calling this method, you should chain a call to
    ///         <see cref="OwnedNavigationBuilder.WithOwner" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <param name="ownedType">The entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <returns>An object that can be used to configure the owned type and the relationship.</returns>
    public virtual OwnedNavigationBuilder OwnsOne(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type ownedType,
        string navigationName)
    {
        Check.NotNull(ownedType, nameof(ownedType));

        return OwnsOneBuilder(
            new TypeIdentity(ownedType, (Model)Metadata.Model),
            Check.NotEmpty(navigationName, nameof(navigationName)));
    }

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The target entity type for each ownership relationship is treated as a different entity type
    ///         even if the navigation is of the same type. Configuration of the target entity type
    ///         isn't applied to the target entity type of other ownership relationships.
    ///     </para>
    ///     <para>
    ///         Most operations on an owned entity require accessing it through the owner entity using the corresponding navigation.
    ///     </para>
    ///     <para>
    ///         After calling this method, you should chain a call to
    ///         <see cref="OwnedNavigationBuilder.WithOwner" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <param name="ownedTypeName">The name of the entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the owned type and the relationship.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual EntityTypeBuilder OwnsOne(
        string ownedTypeName,
        string navigationName,
        Action<OwnedNavigationBuilder> buildAction)
    {
        Check.NotEmpty(ownedTypeName, nameof(ownedTypeName));
        Check.NotEmpty(navigationName, nameof(navigationName));
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(OwnsOneBuilder(new TypeIdentity(ownedTypeName), navigationName));
        return this;
    }

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The target entity type for each ownership relationship is treated as a different entity type
    ///         even if the navigation is of the same type. Configuration of the target entity type
    ///         isn't applied to the target entity type of other ownership relationships.
    ///     </para>
    ///     <para>
    ///         Most operations on an owned entity require accessing it through the owner entity using the corresponding navigation.
    ///     </para>
    ///     <para>
    ///         After calling this method, you should chain a call to
    ///         <see cref="OwnedNavigationBuilder.WithOwner" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <param name="ownedTypeName">The name of the entity type that this relationship targets.</param>
    /// <param name="ownedType">The CLR type of the entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the owned type and the relationship.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual EntityTypeBuilder OwnsOne(
        string ownedTypeName,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type ownedType,
        string navigationName,
        Action<OwnedNavigationBuilder> buildAction)
    {
        Check.NotEmpty(ownedTypeName, nameof(ownedTypeName));
        Check.NotNull(ownedType, nameof(ownedType));
        Check.NotEmpty(navigationName, nameof(navigationName));
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(OwnsOneBuilder(new TypeIdentity(ownedTypeName, ownedType), navigationName));
        return this;
    }

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The target entity type for each ownership relationship is treated as a different entity type
    ///         even if the navigation is of the same type. Configuration of the target entity type
    ///         isn't applied to the target entity type of other ownership relationships.
    ///     </para>
    ///     <para>
    ///         Most operations on an owned entity require accessing it through the owner entity using the corresponding navigation.
    ///     </para>
    ///     <para>
    ///         After calling this method, you should chain a call to
    ///         <see cref="OwnedNavigationBuilder.WithOwner" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <param name="ownedType">The entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the owned type and the relationship.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual EntityTypeBuilder OwnsOne(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type ownedType,
        string navigationName,
        Action<OwnedNavigationBuilder> buildAction)
    {
        Check.NotNull(ownedType, nameof(ownedType));
        Check.NotEmpty(navigationName, nameof(navigationName));
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(OwnsOneBuilder(new TypeIdentity(ownedType, (Model)Metadata.Model), navigationName));
        return this;
    }

    private OwnedNavigationBuilder OwnsOneBuilder(in TypeIdentity ownedType, string navigationName)
    {
        IMutableForeignKey foreignKey;
        using (var batch = Builder.Metadata.Model.DelayConventions())
        {
            var navigationMember = new MemberIdentity(navigationName);
            var relationship = Builder.HasOwnership(ownedType, navigationMember, ConfigurationSource.Explicit)!;
            relationship.IsUnique(true, ConfigurationSource.Explicit);
            foreignKey = (IMutableForeignKey)batch.Run(relationship.Metadata)!;
        }

        return new OwnedNavigationBuilder(foreignKey);
    }

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The target entity type for each ownership relationship is treated as a different entity type
    ///         even if the navigation is of the same type. Configuration of the target entity type
    ///         isn't applied to the target entity type of other ownership relationships.
    ///     </para>
    ///     <para>
    ///         Most operations on an owned entity require accessing it through the owner entity using the corresponding navigation.
    ///     </para>
    ///     <para>
    ///         After calling this method, you should chain a call to
    ///         <see cref="OwnedNavigationBuilder.WithOwner" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <param name="ownedTypeName">The name of the entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <returns>An object that can be used to configure the owned type and the relationship.</returns>
    public virtual OwnedNavigationBuilder OwnsMany(
        string ownedTypeName,
        string navigationName)
        => OwnsManyBuilder(
            new TypeIdentity(Check.NotEmpty(ownedTypeName, nameof(ownedTypeName))),
            Check.NotEmpty(navigationName, nameof(navigationName)));

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The target entity type for each ownership relationship is treated as a different entity type
    ///         even if the navigation is of the same type. Configuration of the target entity type
    ///         isn't applied to the target entity type of other ownership relationships.
    ///     </para>
    ///     <para>
    ///         Most operations on an owned entity require accessing it through the owner entity using the corresponding navigation.
    ///     </para>
    ///     <para>
    ///         After calling this method, you should chain a call to
    ///         <see cref="OwnedNavigationBuilder.WithOwner" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <param name="ownedTypeName">The name of the entity type that this relationship targets.</param>
    /// <param name="ownedType">The CLR type of the entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <returns>An object that can be used to configure the owned type and the relationship.</returns>
    public virtual OwnedNavigationBuilder OwnsMany(
        string ownedTypeName,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type ownedType,
        string navigationName)
    {
        Check.NotNull(ownedType, nameof(ownedType));

        return OwnsManyBuilder(
            new TypeIdentity(Check.NotEmpty(ownedTypeName, nameof(ownedTypeName)), ownedType),
            Check.NotEmpty(navigationName, nameof(navigationName)));
    }

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The target entity type for each ownership relationship is treated as a different entity type
    ///         even if the navigation is of the same type. Configuration of the target entity type
    ///         isn't applied to the target entity type of other ownership relationships.
    ///     </para>
    ///     <para>
    ///         Most operations on an owned entity require accessing it through the owner entity using the corresponding navigation.
    ///     </para>
    ///     <para>
    ///         After calling this method, you should chain a call to
    ///         <see cref="OwnedNavigationBuilder.WithOwner" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <param name="ownedType">The entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <returns>An object that can be used to configure the owned type and the relationship.</returns>
    public virtual OwnedNavigationBuilder OwnsMany(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type ownedType,
        string navigationName)
    {
        Check.NotNull(ownedType, nameof(ownedType));

        return OwnsManyBuilder(
            new TypeIdentity(ownedType, (Model)Metadata.Model),
            Check.NotEmpty(navigationName, nameof(navigationName)));
    }

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The target entity type for each ownership relationship is treated as a different entity type
    ///         even if the navigation is of the same type. Configuration of the target entity type
    ///         isn't applied to the target entity type of other ownership relationships.
    ///     </para>
    ///     <para>
    ///         Most operations on an owned entity require accessing it through the owner entity using the corresponding navigation.
    ///     </para>
    ///     <para>
    ///         After calling this method, you should chain a call to
    ///         <see cref="OwnedNavigationBuilder.WithOwner" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <param name="ownedTypeName">The name of the entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the owned type and the relationship.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual EntityTypeBuilder OwnsMany(
        string ownedTypeName,
        string navigationName,
        Action<OwnedNavigationBuilder> buildAction)
    {
        Check.NotEmpty(ownedTypeName, nameof(ownedTypeName));
        Check.NotEmpty(navigationName, nameof(navigationName));
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(OwnsManyBuilder(new TypeIdentity(ownedTypeName), navigationName));
        return this;
    }

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The target entity type for each ownership relationship is treated as a different entity type
    ///         even if the navigation is of the same type. Configuration of the target entity type
    ///         isn't applied to the target entity type of other ownership relationships.
    ///     </para>
    ///     <para>
    ///         Most operations on an owned entity require accessing it through the owner entity using the corresponding navigation.
    ///     </para>
    ///     <para>
    ///         After calling this method, you should chain a call to
    ///         <see cref="OwnedNavigationBuilder.WithOwner" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <param name="ownedTypeName">The name of the entity type that this relationship targets.</param>
    /// <param name="ownedType">The CLR type of the entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the owned type and the relationship.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual EntityTypeBuilder OwnsMany(
        string ownedTypeName,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type ownedType,
        string navigationName,
        Action<OwnedNavigationBuilder> buildAction)
    {
        Check.NotEmpty(ownedTypeName, nameof(ownedTypeName));
        Check.NotNull(ownedType, nameof(ownedType));
        Check.NotEmpty(navigationName, nameof(navigationName));
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(OwnsManyBuilder(new TypeIdentity(ownedTypeName, ownedType), navigationName));
        return this;
    }

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The target entity type for each ownership relationship is treated as a different entity type
    ///         even if the navigation is of the same type. Configuration of the target entity type
    ///         isn't applied to the target entity type of other ownership relationships.
    ///     </para>
    ///     <para>
    ///         Most operations on an owned entity require accessing it through the owner entity using the corresponding navigation.
    ///     </para>
    ///     <para>
    ///         After calling this method, you should chain a call to
    ///         <see cref="OwnedNavigationBuilder.WithOwner" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <param name="ownedType">The entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the owned type and the relationship.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual EntityTypeBuilder OwnsMany(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type ownedType,
        string navigationName,
        Action<OwnedNavigationBuilder> buildAction)
    {
        Check.NotNull(ownedType, nameof(ownedType));
        Check.NotEmpty(navigationName, nameof(navigationName));
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(OwnsManyBuilder(new TypeIdentity(ownedType, (Model)Metadata.Model), navigationName));
        return this;
    }

    private OwnedNavigationBuilder OwnsManyBuilder(in TypeIdentity ownedType, string navigationName)
    {
        IMutableForeignKey foreignKey;
        using (var batch = Builder.Metadata.Model.DelayConventions())
        {
            var navigationMember = new MemberIdentity(navigationName);
            var relationship = Builder.HasOwnership(ownedType, navigationMember, ConfigurationSource.Explicit)!;
            relationship.IsUnique(false, ConfigurationSource.Explicit);
            foreignKey = (IMutableForeignKey)batch.Run(relationship.Metadata)!;
        }

        return new OwnedNavigationBuilder(foreignKey);
    }

    /// <summary>
    ///     Configures a relationship where this entity type has a reference that points
    ///     to a single instance of the other type in the relationship.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that calling this method with no parameters will explicitly configure this side
    ///         of the relationship to use no navigation property, even if such a property exists on the
    ///         entity type. If the navigation property is to be used, then it must be specified.
    ///     </para>
    ///     <para>
    ///         After calling this method, you should chain a call to
    ///         <see cref="ReferenceNavigationBuilder.WithMany" />
    ///         or <see cref="ReferenceNavigationBuilder.WithOne" /> to fully configure
    ///         the relationship. Calling just this method without the chained call will not
    ///         produce a valid relationship.
    ///     </para>
    /// </remarks>
    /// <param name="relatedTypeName">The name of the entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship. If
    ///     no property is specified, the relationship will be configured without a navigation property on this
    ///     end.
    /// </param>
    /// <returns>An object that can be used to configure the relationship.</returns>
    public virtual ReferenceNavigationBuilder HasOne(
        string relatedTypeName,
        string? navigationName)
    {
        Check.NotEmpty(relatedTypeName, nameof(relatedTypeName));
        Check.NullButNotEmpty(navigationName, nameof(navigationName));

        var relatedEntityType = FindRelatedEntityType(relatedTypeName, navigationName);
        var foreignKey = HasOneBuilder(MemberIdentity.Create(navigationName), relatedEntityType);

        return new ReferenceNavigationBuilder(
            Builder.Metadata,
            relatedEntityType,
            navigationName,
            foreignKey);
    }

    /// <summary>
    ///     Configures a relationship where this entity type has a reference that points
    ///     to a single instance of the other type in the relationship.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that calling this method with no parameters will explicitly configure this side
    ///         of the relationship to use no navigation property, even if such a property exists on the
    ///         entity type. If the navigation property is to be used, then it must be specified.
    ///     </para>
    ///     <para>
    ///         After calling this method, you should chain a call to
    ///         <see cref="ReferenceNavigationBuilder.WithMany" />
    ///         or <see cref="ReferenceNavigationBuilder.WithOne" /> to fully configure
    ///         the relationship. Calling just this method without the chained call will not
    ///         produce a valid relationship.
    ///     </para>
    /// </remarks>
    /// <param name="relatedType">The entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship. If
    ///     no property is specified, the relationship will be configured without a navigation property on this
    ///     end.
    /// </param>
    /// <returns>An object that can be used to configure the relationship.</returns>
    public virtual ReferenceNavigationBuilder HasOne(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type relatedType,
        string? navigationName = null)
    {
        Check.NotNull(relatedType, nameof(relatedType));
        Check.NullButNotEmpty(navigationName, nameof(navigationName));

        var relatedEntityType = FindRelatedEntityType(relatedType, navigationName);
        var foreignKey = HasOneBuilder(MemberIdentity.Create(navigationName), relatedEntityType);

        return new ReferenceNavigationBuilder(
            Builder.Metadata,
            relatedEntityType,
            navigationName,
            foreignKey);
    }

    /// <summary>
    ///     Configures a relationship where this entity type has a reference that points
    ///     to a single instance of the other type in the relationship.
    /// </summary>
    /// <remarks>
    ///     After calling this method, you should chain a call to
    ///     <see cref="ReferenceNavigationBuilder.WithMany" />
    ///     or <see cref="ReferenceNavigationBuilder.WithOne" /> to fully configure
    ///     the relationship. Calling just this method without the chained call will not
    ///     produce a valid relationship.
    /// </remarks>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents
    ///     the relationship. The navigation must be a CLR property on the entity type.
    /// </param>
    /// <returns>An object that can be used to configure the relationship.</returns>
    [RequiresUnreferencedCode("Use an overload that accepts a type")]
    public virtual ReferenceNavigationBuilder HasOne(string? navigationName)
    {
        Check.NotEmpty(navigationName, nameof(navigationName));

        return Metadata.ClrType == Model.DefaultPropertyBagType
            ? HasOne(navigationName, null) // Path only used by pre 3.0 snapshots
            : HasOne(Metadata.GetNavigationMemberInfo(navigationName).GetMemberType(), navigationName);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual ForeignKey HasOneBuilder(
        MemberIdentity navigationId,
        EntityType relatedEntityType)
    {
        if (Metadata[CoreAnnotationNames.SkipNavigationBeingConfigured] is SkipNavigation skipNavigation
            && skipNavigation.DeclaringEntityType == relatedEntityType
            && skipNavigation.ForeignKey?.DeclaringEntityType == Builder.Metadata)
        {
            return navigationId.MemberInfo != null
                ? skipNavigation.ForeignKey.Builder.HasNavigation(navigationId.MemberInfo, pointsToPrincipal: true, ConfigurationSource.Explicit)
                    !.Metadata
                : skipNavigation.ForeignKey.Builder.HasNavigation(navigationId.Name, pointsToPrincipal: true, ConfigurationSource.Explicit)
                    !.Metadata;
        }

        ForeignKey foreignKey;
        if (navigationId.MemberInfo != null)
        {
            foreignKey = Builder.HasRelationship(
                relatedEntityType, navigationId.MemberInfo, ConfigurationSource.Explicit,
                targetIsPrincipal: Builder.Metadata == relatedEntityType ? true : null)!.Metadata;
        }
        else
        {
            foreignKey = Builder.HasRelationship(
                relatedEntityType, navigationId.Name, ConfigurationSource.Explicit,
                targetIsPrincipal: Builder.Metadata == relatedEntityType ? true : null)!.Metadata;
        }

        return foreignKey;
    }

    /// <summary>
    ///     Configures a relationship where this entity type has a collection that contains
    ///     instances of the other type in the relationship.
    /// </summary>
    /// <remarks>
    ///     After calling this method, you should chain a call to
    ///     <see cref="CollectionNavigationBuilder.WithOne" />
    ///     to fully configure the relationship. Calling just this method without the chained call will not
    ///     produce a valid relationship.
    /// </remarks>
    /// <param name="relatedTypeName">The name of the entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the collection navigation property on this entity type that represents the relationship. If
    ///     no property is specified, the relationship will be configured without a navigation property on this
    ///     end.
    /// </param>
    /// <returns>An object that can be used to configure the relationship.</returns>
    public virtual CollectionNavigationBuilder HasMany(
        string relatedTypeName,
        string? navigationName)
    {
        Check.NotEmpty(relatedTypeName, nameof(relatedTypeName));
        Check.NullButNotEmpty(navigationName, nameof(navigationName));

        return HasMany(
            navigationName,
            FindRelatedEntityType(relatedTypeName, navigationName));
    }

    /// <summary>
    ///     Configures a relationship where this entity type has a collection that contains
    ///     instances of the other type in the relationship.
    /// </summary>
    /// <remarks>
    ///     After calling this method, you should chain a call to
    ///     <see cref="CollectionNavigationBuilder.WithOne" />
    ///     to fully configure the relationship. Calling just this method without the chained call will not
    ///     produce a valid relationship.
    /// </remarks>
    /// <param name="navigationName">
    ///     The name of the collection navigation property on this entity type that represents the relationship.
    ///     The navigation must be a CLR property on the entity type.
    /// </param>
    /// <returns>An object that can be used to configure the relationship.</returns>
    [RequiresUnreferencedCode("Use the generic overload instead")]
    public virtual CollectionNavigationBuilder HasMany(string navigationName)
    {
        Check.NotEmpty(navigationName, nameof(navigationName));

        var memberType = Metadata.GetNavigationMemberInfo(navigationName).GetMemberType();
        var elementType = memberType.TryGetElementType(typeof(IEnumerable<>));

        if (elementType == null)
        {
            throw new InvalidOperationException(
                CoreStrings.NavigationCollectionWrongClrType(
                    navigationName,
                    Metadata.DisplayName(),
                    memberType.ShortDisplayName(),
                    "T"));
        }

        return HasMany(elementType, navigationName);
    }

    /// <summary>
    ///     Configures a relationship where this entity type has a collection that contains
    ///     instances of the other type in the relationship.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that calling this method with no parameters will explicitly configure this side
    ///         of the relationship to use no navigation property, even if such a property exists on the
    ///         entity type. If the navigation property is to be used, then it must be specified.
    ///     </para>
    ///     <para>
    ///         After calling this method, you should chain a call to
    ///         <see cref="CollectionNavigationBuilder.WithOne" />
    ///         to fully configure the relationship. Calling just this method without the chained call will not
    ///         produce a valid relationship.
    ///     </para>
    /// </remarks>
    /// <param name="relatedType">The entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the collection navigation property on this entity type that represents the relationship. If
    ///     no property is specified, the relationship will be configured without a navigation property on this
    ///     end.
    /// </param>
    /// <returns>An object that can be used to configure the relationship.</returns>
    public virtual CollectionNavigationBuilder HasMany(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type relatedType,
        string? navigationName = null)
    {
        Check.NotNull(relatedType, nameof(relatedType));
        Check.NullButNotEmpty(navigationName, nameof(navigationName));

        return HasMany(
            navigationName,
            FindRelatedEntityType(relatedType, navigationName));
    }

    private CollectionNavigationBuilder HasMany(
        string? navigationName,
        EntityType relatedEntityType)
    {
        // Note: delay setting ConfigurationSource of skip navigation (if it exists).
        // We do not yet know whether this will be a HasMany().WithOne() or a
        // HasMany().WithMany(). If the skip navigation was found by convention
        // we want to be able to override it later.
        var skipNavigation = navigationName != null ? Builder.Metadata.FindSkipNavigation(navigationName) : null;

        InternalForeignKeyBuilder? relationship = null;
        if (skipNavigation == null)
        {
            relationship = Builder
                .HasRelationship(relatedEntityType, navigationName, ConfigurationSource.Explicit, targetIsPrincipal: false)!
                .IsUnique(false, ConfigurationSource.Explicit);
        }

        return new CollectionNavigationBuilder(
            Builder.Metadata,
            relatedEntityType,
            new MemberIdentity(navigationName!),
            relationship?.Metadata,
            skipNavigation);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual EntityType FindRelatedEntityType(string relatedTypeName, string? navigationName)
        => (navigationName == null
                ? null
                : Builder.ModelBuilder.Metadata.FindEntityType(relatedTypeName, navigationName, Builder.Metadata))
            ?? Builder.ModelBuilder.Entity(relatedTypeName, ConfigurationSource.Explicit, shouldBeOwned: false)!.Metadata;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual EntityType FindRelatedEntityType(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type relatedType,
        string? navigationName)
        => (navigationName == null || !Builder.ModelBuilder.Metadata.IsShared(relatedType)
                ? null
                : Builder.ModelBuilder.Metadata.FindEntityType(relatedType, navigationName, Builder.Metadata))
            ?? Builder.ModelBuilder.Entity(relatedType, ConfigurationSource.Explicit, shouldBeOwned: false)!.Metadata;

    /// <summary>
    ///     Configures the <see cref="ChangeTrackingStrategy" /> to be used for this entity type.
    ///     This strategy indicates how the context detects changes to properties for an instance of the entity type.
    /// </summary>
    /// <param name="changeTrackingStrategy">The change tracking strategy to be used.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder HasChangeTrackingStrategy(ChangeTrackingStrategy changeTrackingStrategy)
    {
        Builder.HasChangeTrackingStrategy(changeTrackingStrategy, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Sets the <see cref="PropertyAccessMode" /> to use for all properties of this entity type.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, the backing field, if one is found by convention or has been specified, is used when
    ///         new objects are constructed, typically when entities are queried from the database.
    ///         Properties are used for all other accesses.  Calling this method will change that behavior
    ///         for all properties of this entity type as described in the <see cref="PropertyAccessMode" /> enum.
    ///     </para>
    ///     <para>
    ///         Calling this method overrides for all properties of this entity type any access mode that was
    ///         set on the model.
    ///     </para>
    /// </remarks>
    /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" /> to use for properties of this entity type.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
    {
        Builder.UsePropertyAccessMode(propertyAccessMode, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Configures this entity to have seed data. It is used to generate data motion migrations.
    /// </summary>
    /// <param name="data">
    ///     An array of seed data represented by anonymous types.
    /// </param>
    /// <returns>An object that can be used to configure the model data.</returns>
    public virtual DataBuilder HasData(params object[] data)
        => HasData((IEnumerable<object>)data);

    /// <summary>
    ///     Configures this entity to have seed data. It is used to generate data motion migrations.
    /// </summary>
    /// <param name="data">
    ///     A collection of seed data represented by anonymous types.
    /// </param>
    /// <returns>An object that can be used to configure the model data.</returns>
    public virtual DataBuilder HasData(IEnumerable<object> data)
    {
        Check.NotNull(data, nameof(data));

        Builder.HasData(data, ConfigurationSource.Explicit);

        return new DataBuilder();
    }

    /// <summary>
    ///     Configures the discriminator property used to identify the entity type in the store.
    /// </summary>
    /// <returns>A builder that allows the discriminator property to be configured.</returns>
    public virtual DiscriminatorBuilder HasDiscriminator()
        => Builder.HasDiscriminator(ConfigurationSource.Explicit)!;

    /// <summary>
    ///     Configures the discriminator property used to identify the entity type in the store.
    /// </summary>
    /// <param name="name">The name of the discriminator property.</param>
    /// <param name="type">The type of values stored in the discriminator property.</param>
    /// <returns>A builder that allows the discriminator property to be configured.</returns>
    public virtual DiscriminatorBuilder HasDiscriminator(
        string name,
        Type type)
    {
        Check.NotEmpty(name, nameof(name));
        Check.NotNull(type, nameof(type));

        return Builder.HasDiscriminator(name, type, ConfigurationSource.Explicit)!;
    }

    /// <summary>
    ///     Configures the discriminator property used to identify the entity type in the store.
    /// </summary>
    /// <typeparam name="TDiscriminator">The type of values stored in the discriminator property.</typeparam>
    /// <param name="name">The name of the discriminator property.</param>
    /// <returns>A builder that allows the discriminator property to be configured.</returns>
    public virtual DiscriminatorBuilder<TDiscriminator> HasDiscriminator<TDiscriminator>(string name)
    {
        Check.NotEmpty(name, nameof(name));

        return new DiscriminatorBuilder<TDiscriminator>(
            Builder.HasDiscriminator(name, typeof(TDiscriminator), ConfigurationSource.Explicit)!);
    }

    /// <summary>
    ///     Configures the entity type as having no discriminator property.
    /// </summary>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder HasNoDiscriminator()
    {
        Builder.HasNoDiscriminator(ConfigurationSource.Explicit);
        return this;
    }

    /// <summary>
    ///     Configures a trigger for the entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="modelName">The name of the trigger.</param>
    /// <returns>A builder that can be used to configure the trigger.</returns>
    public static TriggerBuilder HasTrigger(IMutableEntityType entityType, string modelName)
        => new(((EntityType)entityType).Builder.HasTrigger(modelName, ConfigurationSource.Explicit)!.Metadata);

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
