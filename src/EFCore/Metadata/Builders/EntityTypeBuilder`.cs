// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
/// <typeparam name="TEntity">The entity type being configured.</typeparam>
public class EntityTypeBuilder<[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TEntity> : EntityTypeBuilder
    where TEntity : class
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public EntityTypeBuilder(IMutableEntityType entityType)
        : base(entityType)
    {
    }

    /// <summary>
    ///     Adds or updates an annotation on the entity type. If an annotation with the key specified in
    ///     <paramref name="annotation" /> already exists its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same typeBuilder instance so that multiple configuration calls can be chained.</returns>
    public new virtual EntityTypeBuilder<TEntity> HasAnnotation(string annotation, object? value)
        => (EntityTypeBuilder<TEntity>)base.HasAnnotation(annotation, value);

    /// <summary>
    ///     Sets the base type of this entity type in an inheritance hierarchy.
    /// </summary>
    /// <param name="name">The name of the base type or <see langword="null" /> to indicate no base type.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual EntityTypeBuilder<TEntity> HasBaseType(string? name)
        => new(Builder.HasBaseType(name, ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Sets the base type of this entity type in an inheritance hierarchy.
    /// </summary>
    /// <param name="entityType">The base type or <see langword="null" /> to indicate no base type.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual EntityTypeBuilder<TEntity> HasBaseType(Type? entityType)
        => new(Builder.HasBaseType(entityType, ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Sets the base type of this entity type in an inheritance hierarchy.
    /// </summary>
    /// <typeparam name="TBaseType">The base type or <see langword="null" /> to indicate no base type.</typeparam>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder<TEntity> HasBaseType<TBaseType>()
        => HasBaseType(typeof(TBaseType));

    /// <summary>
    ///     Sets the properties that make up the primary key for this entity type.
    /// </summary>
    /// <param name="keyExpression">
    ///     <para>
    ///         A lambda expression representing the primary key property(s) (<c>blog => blog.Url</c>).
    ///     </para>
    ///     <para>
    ///         If the primary key is made up of multiple properties then specify an anonymous type including the
    ///         properties (<c>post => new { post.Title, post.BlogId }</c>).
    ///     </para>
    /// </param>
    /// <returns>An object that can be used to configure the primary key.</returns>
    public virtual KeyBuilder HasKey(Expression<Func<TEntity, object?>> keyExpression)
        => new KeyBuilder<TEntity>(
            Builder.PrimaryKey(
                Check.NotNull(keyExpression, nameof(keyExpression)).GetMemberAccessList(),
                ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Sets the properties that make up the primary key for this entity type.
    /// </summary>
    /// <param name="propertyNames">The names of the properties that make up the primary key.</param>
    /// <returns>An object that can be used to configure the primary key.</returns>
    public new virtual KeyBuilder<TEntity> HasKey(params string[] propertyNames)
        => new(
            Builder.PrimaryKey(
                Check.NotEmpty(propertyNames, nameof(propertyNames)), ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Creates an alternate key in the model for this entity type if one does not already exist over the specified
    ///     properties. This will force the properties to be read-only. Use <see cref="HasIndex(string[])" /> or
    ///     <see cref="HasIndex(Expression{Func{TEntity, object}})" /> to specify uniqueness
    ///     in the model that does not force properties to be read-only.
    /// </summary>
    /// <param name="keyExpression">
    ///     <para>
    ///         A lambda expression representing the key property(s) (<c>blog => blog.Url</c>).
    ///     </para>
    ///     <para>
    ///         If the key is made up of multiple properties then specify an anonymous type including
    ///         the properties (<c>post => new { post.Title, post.BlogId }</c>).
    ///     </para>
    /// </param>
    /// <returns>An object that can be used to configure the key.</returns>
    public virtual KeyBuilder<TEntity> HasAlternateKey(Expression<Func<TEntity, object?>> keyExpression)
        => new(
            Builder.HasKey(
                Check.NotNull(keyExpression, nameof(keyExpression)).GetMemberAccessList(),
                ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Creates an alternate key in the model for this entity type if one does not already exist over the specified
    ///     properties. This will force the properties to be read-only. Use <see cref="HasIndex(string[])" /> or
    ///     <see cref="HasIndex(Expression{Func{TEntity, object}})" /> to specify uniqueness
    ///     in the model that does not force properties to be read-only.
    /// </summary>
    /// <param name="propertyNames">The names of the properties that make up the key.</param>
    /// <returns>An object that can be used to configure the key.</returns>
    public new virtual KeyBuilder<TEntity> HasAlternateKey(params string[] propertyNames)
        => new(
            Builder.HasKey(
                Check.NotEmpty(propertyNames, nameof(propertyNames)), ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Configures the entity type to have no keys. It will only be usable for queries.
    /// </summary>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual EntityTypeBuilder<TEntity> HasNoKey()
        => (EntityTypeBuilder<TEntity>)base.HasNoKey();

    /// <summary>
    ///     Returns an object that can be used to configure a property of the entity type.
    ///     If the specified property is not already part of the model, it will be added.
    /// </summary>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (
    ///     <c>blog => blog.Url</c>).
    /// </param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual PropertyBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
        => new(
            Builder.Property(
                    Check.NotNull(propertyExpression, nameof(propertyExpression)).GetMemberAccess(), ConfigurationSource.Explicit)!
                .Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a property of the entity type where that property represents
    ///     a collection of primitive values, such as strings or integers.
    /// </summary>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (
    ///     <c>blog => blog.Url</c>).
    /// </param>
    /// <returns>An object that can be used to configure the property.</returns>
    public virtual PrimitiveCollectionBuilder<TProperty> PrimitiveCollection<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression)
        => new(
            Builder.PrimitiveCollection(
                Check.NotNull(propertyExpression, nameof(propertyExpression)).GetMemberAccess(),
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
    public new virtual EntityTypeBuilder<TEntity> ComplexProperty(string propertyName, Action<ComplexPropertyBuilder> buildAction)
        => (EntityTypeBuilder<TEntity>)base.ComplexProperty(propertyName, buildAction);

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
    public new virtual EntityTypeBuilder<TEntity> ComplexProperty<TProperty>(
        string propertyName,
        Action<ComplexPropertyBuilder<TProperty>> buildAction)
        => (EntityTypeBuilder<TEntity>)base.ComplexProperty(propertyName, buildAction);

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
    public new virtual EntityTypeBuilder<TEntity> ComplexProperty<TProperty>(
        string propertyName,
        string complexTypeName,
        Action<ComplexPropertyBuilder<TProperty>> buildAction)
        => (EntityTypeBuilder<TEntity>)base.ComplexProperty(propertyName, complexTypeName, buildAction);

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
    public new virtual EntityTypeBuilder<TEntity> ComplexProperty(
        Type propertyType,
        string propertyName,
        Action<ComplexPropertyBuilder> buildAction)
        => (EntityTypeBuilder<TEntity>)base.ComplexProperty(propertyType, propertyName, buildAction);

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
    public new virtual EntityTypeBuilder<TEntity> ComplexProperty(
        Type propertyType,
        string propertyName,
        string complexTypeName,
        Action<ComplexPropertyBuilder> buildAction)
        => (EntityTypeBuilder<TEntity>)base.ComplexProperty(propertyType, propertyName, complexTypeName, buildAction);

    /// <summary>
    ///     Returns an object that can be used to configure a complex property of the entity type.
    ///     If the specified property is not already part of the model, it will be added.
    /// </summary>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (
    ///     <c>blog => blog.Url</c>).
    /// </param>
    /// <returns>An object that can be used to configure the complex property.</returns>
    public virtual ComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(
        Expression<Func<TEntity, TProperty?>> propertyExpression)
        => new(
            Builder.ComplexProperty(
                    Check.NotNull(propertyExpression, nameof(propertyExpression)).GetMemberAccess(),
                    complexTypeName: null,
                    collection: false,
                    ConfigurationSource.Explicit)!
                .Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a complex property of the entity type.
    ///     If the specified property is not already part of the model, it will be added.
    /// </summary>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (
    ///     <c>blog => blog.Url</c>).
    /// </param>
    /// <param name="complexTypeName">The name of the complex type.</param>
    /// <returns>An object that can be used to configure the complex property.</returns>
    public virtual ComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(
        Expression<Func<TEntity, TProperty?>> propertyExpression,
        string complexTypeName)
        => new(
            Builder.ComplexProperty(
                    Check.NotNull(propertyExpression, nameof(propertyExpression)).GetMemberAccess(),
                    Check.NotEmpty(complexTypeName, nameof(complexTypeName)),
                    collection: false,
                    ConfigurationSource.Explicit)!
                .Metadata);

    /// <summary>
    ///     Configures a complex property of the entity type.
    ///     If the specified property is not already part of the model, it will be added.
    /// </summary>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (
    ///     <c>blog => blog.Url</c>).
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder<TEntity> ComplexProperty<TProperty>(
        Expression<Func<TEntity, TProperty?>> propertyExpression,
        Action<ComplexPropertyBuilder<TProperty>> buildAction)
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(ComplexProperty(propertyExpression));

        return this;
    }

    /// <summary>
    ///     Configures a complex property of the entity type.
    ///     If the specified property is not already part of the model, it will be added.
    /// </summary>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured (
    ///     <c>blog => blog.Url</c>).
    /// </param>
    /// <param name="complexTypeName">The name of the complex type.</param>
    /// <param name="buildAction">An action that performs configuration of the property.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder<TEntity> ComplexProperty<TProperty>(
        Expression<Func<TEntity, TProperty?>> propertyExpression,
        string complexTypeName,
        Action<ComplexPropertyBuilder<TProperty>> buildAction)
    {
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(ComplexProperty(propertyExpression, complexTypeName));

        return this;
    }

    /// <summary>
    ///     Returns an object that can be used to configure an existing navigation property of the entity type.
    ///     It is an error for the navigation property not to exist.
    /// </summary>
    /// <typeparam name="TNavigation">The target entity type.</typeparam>
    /// <param name="navigationExpression">
    ///     A lambda expression representing the navigation property to be configured (
    ///     <c>blog => blog.Posts</c>).
    /// </param>
    /// <returns>An object that can be used to configure the navigation property.</returns>
    public virtual NavigationBuilder<TEntity, TNavigation> Navigation<TNavigation>(
        Expression<Func<TEntity, TNavigation?>> navigationExpression)
        where TNavigation : class
        => new(
            Builder.Navigation(
                Check.NotNull(navigationExpression, nameof(navigationExpression)).GetMemberAccess()));

    /// <summary>
    ///     Returns an object that can be used to configure an existing navigation property of the entity type.
    ///     It is an error for the navigation property not to exist.
    /// </summary>
    /// <typeparam name="TNavigation">The target entity type.</typeparam>
    /// <param name="navigationExpression">
    ///     A lambda expression representing the navigation property to be configured (
    ///     <c>blog => blog.Posts</c>).
    /// </param>
    /// <returns>An object that can be used to configure the navigation property.</returns>
    public virtual NavigationBuilder<TEntity, TNavigation> Navigation<TNavigation>(
        Expression<Func<TEntity, IEnumerable<TNavigation>?>> navigationExpression)
        where TNavigation : class
        => new(
            Builder.Navigation(
                Check.NotNull(navigationExpression, nameof(navigationExpression)).GetMemberAccess()));

    /// <summary>
    ///     Excludes the given property from the entity type. This method is typically used to remove properties
    ///     or navigations from the entity type that were added by convention.
    /// </summary>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be ignored
    ///     (<c>blog => blog.Url</c>).
    /// </param>
    public virtual EntityTypeBuilder<TEntity> Ignore(Expression<Func<TEntity, object?>> propertyExpression)
        => (EntityTypeBuilder<TEntity>)base.Ignore(
            Check.NotNull(propertyExpression, nameof(propertyExpression)).GetMemberAccess().GetSimpleMemberName());

    /// <summary>
    ///     Excludes the given property from the entity type. This method is typically used to remove properties
    ///     or navigations from the entity type that were added by convention.
    /// </summary>
    /// <param name="propertyName">The name of the property to be removed from the entity type.</param>
    public new virtual EntityTypeBuilder<TEntity> Ignore(string propertyName)
        => (EntityTypeBuilder<TEntity>)base.Ignore(propertyName);

    /// <summary>
    ///     Specifies a LINQ predicate expression that will automatically be applied to any queries targeting
    ///     this entity type.
    /// </summary>
    /// <param name="filter">The LINQ predicate expression.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual EntityTypeBuilder<TEntity> HasQueryFilter(LambdaExpression? filter)
        => (EntityTypeBuilder<TEntity>)base.HasQueryFilter(filter);

    /// <summary>
    ///     Specifies a LINQ predicate expression that will automatically be applied to any queries targeting
    ///     this entity type.
    /// </summary>
    /// <param name="filter">The LINQ predicate expression.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual EntityTypeBuilder<TEntity> HasQueryFilter(Expression<Func<TEntity, bool>>? filter)
        => (EntityTypeBuilder<TEntity>)base.HasQueryFilter(filter);

    /// <summary>
    ///     Configures a query used to provide data for a keyless entity type.
    /// </summary>
    /// <param name="query">The query that will provide the underlying data for the keyless entity type.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    [Obsolete("Use InMemoryEntityTypeBuilderExtensions.ToInMemoryQuery")]
    public virtual EntityTypeBuilder<TEntity> ToQuery(Expression<Func<IQueryable<TEntity>>> query)
    {
        Check.NotNull(query, nameof(query));

        Builder.HasDefiningQuery(query, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Configures an unnamed index on the specified properties.
    ///     If there is an existing index on the given list of properties,
    ///     then the existing index will be returned for configuration.
    /// </summary>
    /// <param name="indexExpression">
    ///     <para>
    ///         A lambda expression representing the property(s) to be included in the index
    ///         (<c>blog => blog.Url</c>).
    ///     </para>
    ///     <para>
    ///         If the index is made up of multiple properties then specify an anonymous type including the
    ///         properties (<c>post => new { post.Title, post.BlogId }</c>).
    ///     </para>
    /// </param>
    /// <returns>An object that can be used to configure the index.</returns>
    public virtual IndexBuilder<TEntity> HasIndex(Expression<Func<TEntity, object?>> indexExpression)
        => new(
            Builder.HasIndex(
                Check.NotNull(indexExpression, nameof(indexExpression)).GetMemberAccessList(),
                ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Configures an index on the specified properties with the given name.
    ///     If there is an existing index on the given list of properties and with
    ///     the given name, then the existing index will be returned for configuration.
    /// </summary>
    /// <param name="indexExpression">
    ///     <para>
    ///         A lambda expression representing the property(s) to be included in the index
    ///         (<c>blog => blog.Url</c>).
    ///     </para>
    ///     <para>
    ///         If the index is made up of multiple properties then specify an anonymous type including the
    ///         properties (<c>post => new { post.Title, post.BlogId }</c>).
    ///     </para>
    /// </param>
    /// <param name="name">The name to assign to the index.</param>
    /// <returns>An object that can be used to configure the index.</returns>
    public virtual IndexBuilder<TEntity> HasIndex(
        Expression<Func<TEntity, object?>> indexExpression,
        string name)
        => new(
            Builder.HasIndex(
                Check.NotNull(indexExpression, nameof(indexExpression)).GetMemberAccessList(),
                name,
                ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Configures an unnamed index on the specified properties.
    ///     If there is an existing index on the given list of properties,
    ///     then the existing index will be returned for configuration.
    /// </summary>
    /// <param name="propertyNames">The names of the properties that make up the index.</param>
    /// <returns>An object that can be used to configure the index.</returns>
    public new virtual IndexBuilder<TEntity> HasIndex(params string[] propertyNames)
        => new(
            Builder.HasIndex(
                Check.NotEmpty(propertyNames, nameof(propertyNames)),
                ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Configures an index on the specified properties with the given name.
    ///     If there is an existing index on the given list of properties and with
    ///     the given name, then the existing index will be returned for configuration.
    /// </summary>
    /// <param name="propertyNames">The names of the properties that make up the index.</param>
    /// <param name="name">The name to assign to the index.</param>
    /// <returns>An object that can be used to configure the index.</returns>
    public new virtual IndexBuilder<TEntity> HasIndex(
        string[] propertyNames,
        string name)
        => new(
            Builder.HasIndex(
                Check.NotEmpty(propertyNames, nameof(propertyNames)),
                name,
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
    ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TRelatedEntity">The entity type that this relationship targets.</typeparam>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <returns>An object that can be used to configure the owned type and the relationship.</returns>
    public virtual OwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TRelatedEntity>(string navigationName)
        where TRelatedEntity : class
        => OwnsOneBuilder<TRelatedEntity>(
            new TypeIdentity(typeof(TRelatedEntity), (Model)Metadata.Model),
            new MemberIdentity(Check.NotEmpty(navigationName, nameof(navigationName))));

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
    ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TRelatedEntity">The entity type that this relationship targets.</typeparam>
    /// <param name="ownedTypeName">The name of the entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <returns>An object that can be used to configure the owned type and the relationship.</returns>
    public virtual OwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TRelatedEntity>(
            string ownedTypeName,
            string navigationName)
        where TRelatedEntity : class
        => OwnsOneBuilder<TRelatedEntity>(
            new TypeIdentity(Check.NotEmpty(ownedTypeName, nameof(ownedTypeName)), typeof(TRelatedEntity)),
            new MemberIdentity(Check.NotEmpty(navigationName, nameof(navigationName))));

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
    ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TRelatedEntity">The entity type that this relationship targets.</typeparam>
    /// <param name="navigationExpression">
    ///     A lambda expression representing the reference navigation property on this entity type that represents
    ///     the relationship (<c>customer => customer.Address</c>).
    /// </param>
    /// <returns>An object that can be used to configure the owned type and the relationship.</returns>
    public virtual OwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TRelatedEntity>(
            Expression<Func<TEntity, TRelatedEntity?>> navigationExpression)
        where TRelatedEntity : class
        => OwnsOneBuilder<TRelatedEntity>(
            new TypeIdentity(typeof(TRelatedEntity), (Model)Metadata.Model),
            new MemberIdentity(Check.NotNull(navigationExpression, nameof(navigationExpression)).GetMemberAccess()));

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
    ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TRelatedEntity">The entity type that this relationship targets.</typeparam>
    /// <param name="ownedTypeName">The name of the entity type that this relationship targets.</param>
    /// <param name="navigationExpression">
    ///     A lambda expression representing the reference navigation property on this entity type that represents
    ///     the relationship (<c>customer => customer.Address</c>).
    /// </param>
    /// <returns>An object that can be used to configure the owned type and the relationship.</returns>
    public virtual OwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TRelatedEntity>(
            string ownedTypeName,
            Expression<Func<TEntity, TRelatedEntity?>> navigationExpression)
        where TRelatedEntity : class
        => OwnsOneBuilder<TRelatedEntity>(
            new TypeIdentity(Check.NotEmpty(ownedTypeName, nameof(ownedTypeName)), typeof(TRelatedEntity)),
            new MemberIdentity(Check.NotNull(navigationExpression, nameof(navigationExpression)).GetMemberAccess()));

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
    ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TRelatedEntity">The entity type that this relationship targets.</typeparam>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the owned type and the relationship.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual EntityTypeBuilder<TEntity> OwnsOne
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TRelatedEntity>(
            string navigationName,
            Action<OwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
        where TRelatedEntity : class
    {
        Check.NotEmpty(navigationName, nameof(navigationName));
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(
            OwnsOneBuilder<TRelatedEntity>(
                new TypeIdentity(typeof(TRelatedEntity), (Model)Metadata.Model), new MemberIdentity(navigationName)));
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
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the owned type and the relationship.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public new virtual EntityTypeBuilder<TEntity> OwnsOne(
        string ownedTypeName,
        string navigationName,
        Action<OwnedNavigationBuilder> buildAction)
        => (EntityTypeBuilder<TEntity>)base.OwnsOne(ownedTypeName, navigationName, buildAction);

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
    public new virtual EntityTypeBuilder<TEntity> OwnsOne(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type ownedType,
        string navigationName,
        Action<OwnedNavigationBuilder> buildAction)
        => (EntityTypeBuilder<TEntity>)base.OwnsOne(ownedType, navigationName, buildAction);

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
    public new virtual EntityTypeBuilder<TEntity> OwnsOne(
        string ownedTypeName,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type ownedType,
        string navigationName,
        Action<OwnedNavigationBuilder> buildAction)
        => (EntityTypeBuilder<TEntity>)base.OwnsOne(ownedTypeName, ownedType, navigationName, buildAction);

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
    ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TRelatedEntity">The entity type that this relationship targets.</typeparam>
    /// <param name="ownedTypeName"> The name of the entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the owned type and the relationship.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual EntityTypeBuilder<TEntity> OwnsOne
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TRelatedEntity>(
            string ownedTypeName,
            string navigationName,
            Action<OwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
        where TRelatedEntity : class
    {
        Check.NotEmpty(ownedTypeName, nameof(ownedTypeName));
        Check.NotEmpty(navigationName, nameof(navigationName));
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(
            OwnsOneBuilder<TRelatedEntity>(
                new TypeIdentity(ownedTypeName, typeof(TRelatedEntity)), new MemberIdentity(navigationName)));
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
    ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TRelatedEntity">The entity type that this relationship targets.</typeparam>
    /// <param name="navigationExpression">
    ///     A lambda expression representing the reference navigation property on this entity type that represents
    ///     the relationship (<c>customer => customer.Address</c>).
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the owned type and the relationship.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual EntityTypeBuilder<TEntity> OwnsOne
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TRelatedEntity>(
            Expression<Func<TEntity, TRelatedEntity?>> navigationExpression,
            Action<OwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
        where TRelatedEntity : class
    {
        Check.NotNull(navigationExpression, nameof(navigationExpression));
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(
            OwnsOneBuilder<TRelatedEntity>(
                new TypeIdentity(typeof(TRelatedEntity), (Model)Metadata.Model),
                new MemberIdentity(navigationExpression.GetMemberAccess())));
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
    ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TRelatedEntity">The entity type that this relationship targets.</typeparam>
    /// <param name="ownedTypeName">The name of the entity type that this relationship targets.</param>
    /// <param name="navigationExpression">
    ///     A lambda expression representing the reference navigation property on this entity type that represents
    ///     the relationship (<c>customer => customer.Address</c>).
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the owned type and the relationship.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual EntityTypeBuilder<TEntity> OwnsOne
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TRelatedEntity>(
            string ownedTypeName,
            Expression<Func<TEntity, TRelatedEntity?>> navigationExpression,
            Action<OwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
        where TRelatedEntity : class
    {
        Check.NotEmpty(ownedTypeName, nameof(ownedTypeName));
        Check.NotNull(navigationExpression, nameof(navigationExpression));
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(
            OwnsOneBuilder<TRelatedEntity>(
                new TypeIdentity(ownedTypeName, typeof(TRelatedEntity)), new MemberIdentity(navigationExpression.GetMemberAccess())));
        return this;
    }

    private OwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOneBuilder
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TRelatedEntity>(
            TypeIdentity ownedType,
            MemberIdentity navigation)
        where TRelatedEntity : class
    {
        InternalForeignKeyBuilder relationship;
        using (var batch = Builder.Metadata.Model.DelayConventions())
        {
            relationship = Builder.HasOwnership(ownedType, navigation, ConfigurationSource.Explicit)!;
            relationship.IsUnique(true, ConfigurationSource.Explicit);
            relationship = (InternalForeignKeyBuilder)batch.Run(relationship.Metadata)!.Builder;
        }

        return new OwnedNavigationBuilder<TEntity, TRelatedEntity>(relationship.Metadata);
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
    ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TRelatedEntity">The entity type that this relationship targets.</typeparam>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <returns>An object that can be used to configure the owned type and the relationship.</returns>
    public virtual OwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TRelatedEntity>(
            string navigationName)
        where TRelatedEntity : class
        => OwnsManyBuilder<TRelatedEntity>(
            new TypeIdentity(typeof(TRelatedEntity), (Model)Metadata.Model),
            new MemberIdentity(Check.NotEmpty(navigationName, nameof(navigationName))));

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
    ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TRelatedEntity">The entity type that this relationship targets.</typeparam>
    /// <param name="ownedTypeName">The name of the entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <returns>An object that can be used to configure the owned type and the relationship.</returns>
    public virtual OwnedNavigationBuilder<TEntity, TRelatedEntity>
        OwnsMany<[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TRelatedEntity>(
            string ownedTypeName,
            string navigationName)
        where TRelatedEntity : class
        => OwnsManyBuilder<TRelatedEntity>(
            new TypeIdentity(ownedTypeName, typeof(TRelatedEntity)),
            new MemberIdentity(Check.NotEmpty(navigationName, nameof(navigationName))));

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
    ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TRelatedEntity">The entity type that this relationship targets.</typeparam>
    /// <param name="navigationExpression">
    ///     A lambda expression representing the reference navigation property on this entity type that represents
    ///     the relationship (<c>customer => customer.Address</c>).
    /// </param>
    /// <returns>An object that can be used to configure the owned type and the relationship.</returns>
    public virtual OwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TRelatedEntity>(
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>> navigationExpression)
        where TRelatedEntity : class
        => OwnsManyBuilder<TRelatedEntity>(
            new TypeIdentity(typeof(TRelatedEntity), (Model)Metadata.Model),
            new MemberIdentity(Check.NotNull(navigationExpression, nameof(navigationExpression)).GetMemberAccess()));

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
    ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TRelatedEntity">The entity type that this relationship targets.</typeparam>
    /// <param name="ownedTypeName">The name of the entity type that this relationship targets.</param>
    /// <param name="navigationExpression">
    ///     A lambda expression representing the reference navigation property on this entity type that represents
    ///     the relationship (<c>customer => customer.Address</c>).
    /// </param>
    /// <returns>An object that can be used to configure the owned type and the relationship.</returns>
    public virtual OwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TRelatedEntity>(
            string ownedTypeName,
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>> navigationExpression)
        where TRelatedEntity : class
        => OwnsManyBuilder<TRelatedEntity>(
            new TypeIdentity(ownedTypeName, typeof(TRelatedEntity)),
            new MemberIdentity(Check.NotNull(navigationExpression, nameof(navigationExpression)).GetMemberAccess()));

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
    ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TRelatedEntity">The entity type that this relationship targets.</typeparam>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the owned type and the relationship.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual EntityTypeBuilder<TEntity> OwnsMany
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TRelatedEntity>(
            string navigationName,
            Action<OwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
        where TRelatedEntity : class
    {
        Check.NotEmpty(navigationName, nameof(navigationName));
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(
            OwnsManyBuilder<TRelatedEntity>(
                new TypeIdentity(typeof(TRelatedEntity), (Model)Metadata.Model), new MemberIdentity(navigationName)));
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
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the owned type and the relationship.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public new virtual EntityTypeBuilder<TEntity> OwnsMany(
        string ownedTypeName,
        string navigationName,
        Action<OwnedNavigationBuilder> buildAction)
        => (EntityTypeBuilder<TEntity>)base.OwnsMany(ownedTypeName, navigationName, buildAction);

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
    public new virtual EntityTypeBuilder<TEntity> OwnsMany(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type ownedType,
        string navigationName,
        Action<OwnedNavigationBuilder> buildAction)
        => (EntityTypeBuilder<TEntity>)base.OwnsMany(ownedType, navigationName, buildAction);

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
    public new virtual EntityTypeBuilder<TEntity> OwnsMany(
        string ownedTypeName,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type ownedType,
        string navigationName,
        Action<OwnedNavigationBuilder> buildAction)
        => (EntityTypeBuilder<TEntity>)base.OwnsMany(ownedTypeName, ownedType, navigationName, buildAction);

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
    ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TRelatedEntity">The entity type that this relationship targets.</typeparam>
    /// <param name="ownedTypeName">The name of the entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the owned type and the relationship.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual EntityTypeBuilder<TEntity> OwnsMany
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TRelatedEntity>(
            string ownedTypeName,
            string navigationName,
            Action<OwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
        where TRelatedEntity : class
    {
        Check.NotEmpty(ownedTypeName, nameof(ownedTypeName));
        Check.NotEmpty(navigationName, nameof(navigationName));
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(
            OwnsManyBuilder<TRelatedEntity>(
                new TypeIdentity(ownedTypeName, typeof(TRelatedEntity)), new MemberIdentity(navigationName)));
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
    ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TRelatedEntity">The entity type that this relationship targets.</typeparam>
    /// <param name="navigationExpression">
    ///     A lambda expression representing the reference navigation property on this entity type that represents
    ///     the relationship (<c>customer => customer.Address</c>).
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the owned type and the relationship.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual EntityTypeBuilder<TEntity> OwnsMany
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TRelatedEntity>(
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>> navigationExpression,
            Action<OwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
        where TRelatedEntity : class
    {
        Check.NotNull(navigationExpression, nameof(navigationExpression));
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(
            OwnsManyBuilder<TRelatedEntity>(
                new TypeIdentity(typeof(TRelatedEntity), (Model)Metadata.Model),
                new MemberIdentity(navigationExpression.GetMemberAccess())));
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
    ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TRelatedEntity">The entity type that this relationship targets.</typeparam>
    /// <param name="ownedTypeName">The name of the entity type that this relationship targets.</param>
    /// <param name="navigationExpression">
    ///     A lambda expression representing the reference navigation property on this entity type that represents
    ///     the relationship (<c>customer => customer.Address</c>).
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the owned type and the relationship.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual EntityTypeBuilder<TEntity> OwnsMany
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TRelatedEntity>(
            string ownedTypeName,
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>> navigationExpression,
            Action<OwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
        where TRelatedEntity : class
    {
        Check.NotEmpty(ownedTypeName, nameof(ownedTypeName));
        Check.NotNull(navigationExpression, nameof(navigationExpression));
        Check.NotNull(buildAction, nameof(buildAction));

        buildAction(
            OwnsManyBuilder<TRelatedEntity>(
                new TypeIdentity(ownedTypeName, typeof(TRelatedEntity)), new MemberIdentity(navigationExpression.GetMemberAccess())));
        return this;
    }

    private OwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsManyBuilder
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TRelatedEntity>(
            TypeIdentity ownedType,
            MemberIdentity navigation)
        where TRelatedEntity : class
    {
        InternalForeignKeyBuilder relationship;
        using (var batch = Builder.Metadata.Model.DelayConventions())
        {
            relationship = Builder.HasOwnership(ownedType, navigation, ConfigurationSource.Explicit)!;

            relationship.IsUnique(false, ConfigurationSource.Explicit);
            relationship = (InternalForeignKeyBuilder)batch.Run(relationship.Metadata)!.Builder;
        }

        return new OwnedNavigationBuilder<TEntity, TRelatedEntity>(relationship.Metadata);
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
    ///         <see cref="ReferenceNavigationBuilder{TEntity,TRelatedEntity}.WithMany(string)" />
    ///         or
    ///         <see cref="ReferenceNavigationBuilder{TEntity,TRelatedEntity}.WithOne(string)" />
    ///         to fully configure the relationship. Calling just this method without the chained call will not
    ///         produce a valid relationship.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TRelatedEntity">The entity type that this relationship targets.</typeparam>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship. If
    ///     no property is specified, the relationship will be configured without a navigation property on this
    ///     end.
    /// </param>
    /// <returns>An object that can be used to configure the relationship.</returns>
    public virtual ReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TRelatedEntity>(
            string? navigationName)
        where TRelatedEntity : class
    {
        var relatedEntityType = FindRelatedEntityType(typeof(TRelatedEntity), navigationName);
        var foreignKey = HasOneBuilder(MemberIdentity.Create(navigationName), relatedEntityType);

        return new ReferenceNavigationBuilder<TEntity, TRelatedEntity>(
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
    ///         <see
    ///             cref="ReferenceNavigationBuilder{TEntity,TRelatedEntity}.WithMany(Expression{Func{TRelatedEntity,IEnumerable{TEntity}}})" />
    ///         or
    ///         <see
    ///             cref="ReferenceNavigationBuilder{TEntity,TRelatedEntity}.WithOne(Expression{Func{TRelatedEntity,TEntity}})" />
    ///         to fully configure the relationship. Calling just this method without the chained call will not
    ///         produce a valid relationship.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TRelatedEntity">The entity type that this relationship targets.</typeparam>
    /// <param name="navigationExpression">
    ///     A lambda expression representing the reference navigation property on this entity type that represents
    ///     the relationship (<c>post => post.Blog</c>). If no property is specified, the relationship will be
    ///     configured without a navigation property on this end.
    /// </param>
    /// <returns>An object that can be used to configure the relationship.</returns>
    public virtual ReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TRelatedEntity>(
            Expression<Func<TEntity, TRelatedEntity?>>? navigationExpression = null)
        where TRelatedEntity : class
    {
        var navigationMember = navigationExpression?.GetMemberAccess();
        var relatedEntityType = FindRelatedEntityType(typeof(TRelatedEntity), navigationMember?.GetSimpleMemberName());
        var foreignKey = HasOneBuilder(MemberIdentity.Create(navigationMember), relatedEntityType);

        return new ReferenceNavigationBuilder<TEntity, TRelatedEntity>(
            Builder.Metadata,
            relatedEntityType,
            navigationMember,
            foreignKey);
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
    ///         <see
    ///             cref="CollectionNavigationBuilder{TEntity,TRelatedEntity}.WithOne(Expression{Func{TRelatedEntity,TEntity}})" />
    ///         to fully configure the relationship. Calling just this method without the chained call will not
    ///         produce a valid relationship.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TRelatedEntity">The entity type that this relationship targets.</typeparam>
    /// <param name="navigationName">
    ///     The name of the collection navigation property on this entity type that represents the relationship. If
    ///     no property is specified, the relationship will be configured without a navigation property on this
    ///     end.
    /// </param>
    /// <returns>An object that can be used to configure the relationship.</returns>
    public virtual CollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TRelatedEntity>(string? navigationName)
        where TRelatedEntity : class
    {
        Check.NullButNotEmpty(navigationName, nameof(navigationName));

        var relatedEntityType = FindRelatedEntityType(typeof(TRelatedEntity), navigationName);

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

        return new CollectionNavigationBuilder<TEntity, TRelatedEntity>(
            Builder.Metadata,
            relatedEntityType,
            navigationName is null ? MemberIdentity.None : new MemberIdentity(navigationName),
            relationship?.Metadata,
            skipNavigation);
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
    ///         <see
    ///             cref="CollectionNavigationBuilder{TEntity,TRelatedEntity}.WithOne(Expression{Func{TRelatedEntity,TEntity}})" />
    ///         to fully configure the relationship. Calling just this method without the chained call will not
    ///         produce a valid relationship.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TRelatedEntity">The entity type that this relationship targets.</typeparam>
    /// <param name="navigationExpression">
    ///     A lambda expression representing the collection navigation property on this entity type that represents
    ///     the relationship (<c>blog => blog.Posts</c>). If no property is specified, the relationship will be
    ///     configured without a navigation property on this end.
    /// </param>
    /// <returns>An object that can be used to configure the relationship.</returns>
    public virtual CollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany
        <[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TRelatedEntity>(
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>>? navigationExpression = null)
        where TRelatedEntity : class
    {
        var navigationMember = navigationExpression?.GetMemberAccess();
        var relatedEntityType = FindRelatedEntityType(typeof(TRelatedEntity), navigationMember?.GetSimpleMemberName());

        // Note: delay setting ConfigurationSource of skip navigation (if it exists).
        // We do not yet know whether this will be a HasMany().WithOne() or a
        // HasMany().WithMany(). If the skip navigation was found by convention
        // we want to be able to override it later.
        var skipNavigation = navigationMember != null ? Builder.Metadata.FindSkipNavigation(navigationMember) : null;

        InternalForeignKeyBuilder? relationship = null;
        if (skipNavigation == null)
        {
            relationship = Builder
                .HasRelationship(relatedEntityType, navigationMember, ConfigurationSource.Explicit, targetIsPrincipal: false)!
                .IsUnique(false, ConfigurationSource.Explicit);
        }

        return new CollectionNavigationBuilder<TEntity, TRelatedEntity>(
            Builder.Metadata,
            relatedEntityType,
            navigationMember is null ? MemberIdentity.None : new MemberIdentity(navigationMember),
            relationship?.Metadata,
            skipNavigation);
    }

    /// <summary>
    ///     Configures the <see cref="ChangeTrackingStrategy" /> to be used for this entity type.
    ///     This strategy indicates how the context detects changes to properties for an instance of the entity type.
    /// </summary>
    /// <param name="changeTrackingStrategy">The change tracking strategy to be used.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual EntityTypeBuilder<TEntity> HasChangeTrackingStrategy(ChangeTrackingStrategy changeTrackingStrategy)
        => (EntityTypeBuilder<TEntity>)base.HasChangeTrackingStrategy(changeTrackingStrategy);

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
    public new virtual EntityTypeBuilder<TEntity> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
        => (EntityTypeBuilder<TEntity>)base.UsePropertyAccessMode(propertyAccessMode);

    /// <summary>
    ///     Adds seed data to this entity type. It is used to generate data motion migrations.
    /// </summary>
    /// <param name="data">
    ///     An array of seed data of the same type as the entity.
    /// </param>
    /// <returns>An object that can be used to configure the model data.</returns>
    public virtual DataBuilder<TEntity> HasData(params TEntity[] data)
        => HasData((IEnumerable<object>)data);

    /// <summary>
    ///     Adds seed data to this entity type. It is used to generate data motion migrations.
    /// </summary>
    /// <param name="data">
    ///     A collection of seed data of the same type as the entity.
    /// </param>
    /// <returns>An object that can be used to configure the model data.</returns>
    public virtual DataBuilder<TEntity> HasData(IEnumerable<TEntity> data)
        => HasData((IEnumerable<object>)data);

    /// <summary>
    ///     Adds seed data to this entity type. It is used to generate data motion migrations.
    /// </summary>
    /// <param name="data">
    ///     An array of seed data represented by anonymous types.
    /// </param>
    /// <returns>An object that can be used to configure the model data.</returns>
    public new virtual DataBuilder<TEntity> HasData(params object[] data)
        => HasData((IEnumerable<object>)data);

    /// <summary>
    ///     Adds seed data to this entity type. It is used to generate data motion migrations.
    /// </summary>
    /// <param name="data">
    ///     A collection of seed data represented by anonymous types.
    /// </param>
    /// <returns>An object that can be used to configure the model data.</returns>
    public new virtual DataBuilder<TEntity> HasData(IEnumerable<object> data)
    {
        base.HasData(data);

        return new DataBuilder<TEntity>();
    }

    /// <summary>
    ///     Configures the discriminator property used to identify the entity type in the store.
    /// </summary>
    /// <typeparam name="TDiscriminator">The type of values stored in the discriminator property.</typeparam>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be used as the discriminator (
    ///     <c>blog => blog.Discriminator</c>).
    /// </param>
    /// <returns>A builder that allows the discriminator property to be configured.</returns>
    public virtual DiscriminatorBuilder<TDiscriminator> HasDiscriminator<TDiscriminator>(
        Expression<Func<TEntity, TDiscriminator>> propertyExpression)
    {
        Check.NotNull(propertyExpression, nameof(propertyExpression));

        return new DiscriminatorBuilder<TDiscriminator>(
            Builder.HasDiscriminator(propertyExpression.GetMemberAccess(), ConfigurationSource.Explicit)!);
    }

    /// <summary>
    ///     Configures the entity type as having no discriminator property.
    /// </summary>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual EntityTypeBuilder<TEntity> HasNoDiscriminator()
        => (EntityTypeBuilder<TEntity>)base.HasNoDiscriminator();
}
