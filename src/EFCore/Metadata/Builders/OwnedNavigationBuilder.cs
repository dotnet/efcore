// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a navigation to an owned entity type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class OwnedNavigationBuilder : IInfrastructure<IConventionEntityTypeBuilder>
{
    private InternalForeignKeyBuilder _builder;
    private EntityType _dependentEntityType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public OwnedNavigationBuilder(IMutableForeignKey ownership)
    {
        PrincipalEntityType = (EntityType)ownership.PrincipalEntityType;
        _dependentEntityType = (EntityType)ownership.DeclaringEntityType;
        _builder = ((ForeignKey)ownership).Builder;
    }

    /// <summary>
    ///     Gets the principal entity type used to configure this relationship.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual EntityType PrincipalEntityType { get; }

    /// <summary>
    ///     Gets the dependent entity type used to configure this relationship.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual EntityType DependentEntityType
        => _dependentEntityType.IsInModel
            ? _dependentEntityType
            : _dependentEntityType = Builder.Metadata.DeclaringEntityType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalForeignKeyBuilder Builder
    {
        get
        {
            if (!_builder.Metadata.IsInModel
                && PrincipalEntityType.IsInModel)
            {
                _builder = PrincipalEntityType.FindNavigation(_builder.Metadata.PrincipalToDependent!.Name)?.ForeignKey.Builder!;
            }

            return _builder;
        }

        private set => _builder = value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual T UpdateBuilder<T>(Func<T> configure)
    {
        IConventionForeignKey? foreignKey = _builder.Metadata;
        var result = DependentEntityType.Model.Track(configure, ref foreignKey);
        if (foreignKey != null)
        {
            _builder = ((ForeignKey)foreignKey).Builder;
        }

        return result;
    }

    /// <summary>
    ///     Gets the internal builder being used to configure the owned entity type.
    /// </summary>
    IConventionEntityTypeBuilder IInfrastructure<IConventionEntityTypeBuilder>.Instance
        => DependentEntityType.Builder;

    /// <summary>
    ///     The foreign key that represents this ownership.
    /// </summary>
    public virtual IMutableForeignKey Metadata
        => Builder.Metadata;

    /// <summary>
    ///     The owned entity type being configured.
    /// </summary>
    public virtual IMutableEntityType OwnedEntityType
        => DependentEntityType;

    /// <summary>
    ///     Adds or updates an annotation on the owned entity type. If an annotation with the key specified in
    ///     <paramref name="annotation" /> already exists its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual OwnedNavigationBuilder HasAnnotation(string annotation, object? value)
    {
        Check.NotEmpty(annotation, nameof(annotation));

        DependentEntityType.Builder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Sets the properties that make up the primary key for this owned entity type.
    /// </summary>
    /// <param name="propertyNames">The names of the properties that make up the primary key.</param>
    /// <returns>An object that can be used to configure the primary key.</returns>
    public virtual KeyBuilder HasKey(params string[] propertyNames)
        => new(
            DependentEntityType.Builder.PrimaryKey(
                Check.NotEmpty(propertyNames, nameof(propertyNames)), ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a property of the owned entity type.
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
        => UpdateBuilder(
            () => new PropertyBuilder(
                DependentEntityType.Builder.Property(
                    Check.NotEmpty(propertyName, nameof(propertyName)),
                    ConfigurationSource.Explicit)!.Metadata));

    /// <summary>
    ///     Returns an object that can be used to configure a property of the owned entity type.
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
        => UpdateBuilder(
            () => new PropertyBuilder<TProperty>(
                DependentEntityType.Builder.Property(
                    typeof(TProperty),
                    Check.NotEmpty(propertyName, nameof(propertyName)), ConfigurationSource.Explicit)!.Metadata));

    /// <summary>
    ///     Returns an object that can be used to configure a property of the owned entity type.
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
            DependentEntityType.Builder.Property(
                Check.NotNull(propertyType, nameof(propertyType)),
                Check.NotEmpty(propertyName, nameof(propertyName)), ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Returns an object that can be used to configure a property of the owned type where that property represents
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
        => UpdateBuilder(
            () => new PrimitiveCollectionBuilder(
                DependentEntityType.Builder.PrimitiveCollection(
                    Check.NotEmpty(propertyName, nameof(propertyName)),
                    ConfigurationSource.Explicit)!.Metadata));

    /// <summary>
    ///     Returns an object that can be used to configure a property of the owned type where that property represents
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
        => UpdateBuilder(
            () => new PrimitiveCollectionBuilder<TProperty>(
                DependentEntityType.Builder.PrimitiveCollection(
                    typeof(TProperty),
                    Check.NotEmpty(propertyName, nameof(propertyName)), ConfigurationSource.Explicit)!.Metadata));

    /// <summary>
    ///     Returns an object that can be used to configure a property of the owned type where that property represents
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
            DependentEntityType.Builder.PrimitiveCollection(
                Check.NotNull(propertyType, nameof(propertyType)), Check.NotEmpty(propertyName, nameof(propertyName)),
                ConfigurationSource.Explicit)!.Metadata);

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
            DependentEntityType.Builder.IndexerProperty(
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
            DependentEntityType.Builder.IndexerProperty(
                propertyType,
                Check.NotEmpty(propertyName, nameof(propertyName)), ConfigurationSource.Explicit)!.Metadata);
    }

    /// <summary>
    ///     Returns an object that can be used to configure an existing navigation property
    ///     from the owned type to its owner. It is an error for the navigation property
    ///     not to exist.
    /// </summary>
    /// <param name="navigationName">The name of the navigation property to be configured.</param>
    /// <returns>An object that can be used to configure the navigation property.</returns>
    public virtual NavigationBuilder Navigation(string navigationName)
        => new(
            DependentEntityType.Builder.Navigation(
                Check.NotEmpty(navigationName, nameof(navigationName))));

    /// <summary>
    ///     Excludes the given property from the entity type. This method is typically used to remove properties
    ///     or navigations from the owned entity type that were added by convention.
    /// </summary>
    /// <param name="propertyName">The name of the property to be removed from the entity type.</param>
    public virtual OwnedNavigationBuilder Ignore(string propertyName)
    {
        Check.NotEmpty(propertyName, nameof(propertyName));

        DependentEntityType.Builder.Ignore(propertyName, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Configures an index on the specified properties. If there is an existing index on the given
    ///     set of properties, then the existing index will be returned for configuration.
    /// </summary>
    /// <param name="propertyNames">The names of the properties that make up the index.</param>
    /// <returns>An object that can be used to configure the index.</returns>
    public virtual IndexBuilder HasIndex(params string[] propertyNames)
        => new(
            DependentEntityType.Builder.HasIndex(
                Check.NotEmpty(propertyNames, nameof(propertyNames)), ConfigurationSource.Explicit)!.Metadata);

    /// <summary>
    ///     Configures the relationship to the owner.
    /// </summary>
    /// <remarks>
    ///     Note that calling this method with no parameters will explicitly configure this side
    ///     of the relationship to use no navigation property, even if such a property exists on the
    ///     entity type. If the navigation property is to be used, then it must be specified.
    /// </remarks>
    /// <param name="ownerReference">
    ///     The name of the reference navigation property pointing to the owner.
    ///     If null or not specified, there is no navigation property pointing to the owner.
    /// </param>
    /// <returns>An object that can be used to configure the relationship.</returns>
    public virtual OwnershipBuilder WithOwner(
        string? ownerReference = null)
    {
        Check.NullButNotEmpty(ownerReference, nameof(ownerReference));

        return new OwnershipBuilder(
            PrincipalEntityType,
            DependentEntityType,
            Builder.HasNavigation(
                ownerReference,
                pointsToPrincipal: true,
                ConfigurationSource.Explicit)!.Metadata);
    }

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    ///     The target entity key value is always propagated from the entity it belongs to.
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
    ///         <see cref="WithOwner" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <param name="ownedTypeName">The name of the entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <returns>An object that can be used to configure the relationship.</returns>
    public virtual OwnedNavigationBuilder OwnsOne(
        string ownedTypeName,
        string navigationName)
        => OwnsOneBuilder(
            new TypeIdentity(Check.NotEmpty(ownedTypeName, nameof(ownedTypeName))),
            Check.NotEmpty(navigationName, nameof(navigationName)));

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    ///     The target entity key value is always propagated from the entity it belongs to.
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
    ///         <see cref="WithOwner" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <param name="ownedTypeName">The name of the entity type that this relationship targets.</param>
    /// <param name="ownedType">The CLR type of the entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <returns>An object that can be used to configure the relationship.</returns>
    public virtual OwnedNavigationBuilder OwnsOne(
        string ownedTypeName,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type ownedType,
        string navigationName)
    {
        Check.NotNull(ownedType, nameof(ownedType));

        return OwnsOneBuilder(
            new TypeIdentity(Check.NotEmpty(ownedTypeName, nameof(ownedTypeName)), ownedType),
            Check.NotEmpty(navigationName, nameof(navigationName)));
    }

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    ///     The target entity key value is always propagated from the entity it belongs to.
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
    ///         <see cref="WithOwner" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <param name="ownedType">The entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <returns>An object that can be used to configure the relationship.</returns>
    public virtual OwnedNavigationBuilder OwnsOne(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type ownedType,
        string navigationName)
    {
        Check.NotNull(ownedType, nameof(ownedType));

        return OwnsOneBuilder(
            new TypeIdentity(ownedType, DependentEntityType.Model),
            Check.NotEmpty(navigationName, nameof(navigationName)));
    }

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    ///     The target entity key value is always propagated from the entity it belongs to.
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
    ///         <see cref="WithOwner" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <param name="ownedTypeName">The name of the entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the relationship.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual OwnedNavigationBuilder OwnsOne(
        string ownedTypeName,
        string navigationName,
        Action<OwnedNavigationBuilder> buildAction)
    {
        Check.NotEmpty(ownedTypeName, nameof(ownedTypeName));
        Check.NotEmpty(navigationName, nameof(navigationName));
        Check.NotNull(buildAction, nameof(buildAction));

        using (DependentEntityType.Model.DelayConventions())
        {
            buildAction(OwnsOneBuilder(new TypeIdentity(ownedTypeName), navigationName));
            return this;
        }
    }

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    ///     The target entity key value is always propagated from the entity it belongs to.
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
    ///         <see cref="WithOwner" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <param name="ownedTypeName">The name of the entity type that this relationship targets.</param>
    /// <param name="ownedType">The CLR type of the entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the relationship.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual OwnedNavigationBuilder OwnsOne(
        string ownedTypeName,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type ownedType,
        string navigationName,
        Action<OwnedNavigationBuilder> buildAction)
    {
        Check.NotEmpty(ownedTypeName, nameof(ownedTypeName));
        Check.NotNull(ownedType, nameof(ownedType));
        Check.NotEmpty(navigationName, nameof(navigationName));
        Check.NotNull(buildAction, nameof(buildAction));

        using (DependentEntityType.Model.DelayConventions())
        {
            buildAction(OwnsOneBuilder(new TypeIdentity(ownedTypeName, ownedType), navigationName));
            return this;
        }
    }

    /// <summary>
    ///     Configures a relationship where the target entity is owned by (or part of) this entity.
    ///     The target entity key value is always propagated from the entity it belongs to.
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
    ///         <see cref="WithOwner" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <param name="ownedType">The entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the relationship.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual OwnedNavigationBuilder OwnsOne(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type ownedType,
        string navigationName,
        Action<OwnedNavigationBuilder> buildAction)
    {
        Check.NotNull(ownedType, nameof(ownedType));
        Check.NotEmpty(navigationName, nameof(navigationName));
        Check.NotNull(buildAction, nameof(buildAction));

        using (DependentEntityType.Model.DelayConventions())
        {
            buildAction(OwnsOneBuilder(new TypeIdentity(ownedType, DependentEntityType.Model), navigationName));
            return this;
        }
    }

    private OwnedNavigationBuilder OwnsOneBuilder(in TypeIdentity ownedType, string navigationName)
    {
        IMutableForeignKey foreignKey;
        using (var batch = DependentEntityType.Model.DelayConventions())
        {
            var navigationMember = MemberIdentity.Create(navigationName);
            var relationship = DependentEntityType.Builder.HasOwnership(ownedType, navigationMember, ConfigurationSource.Explicit)!;
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
    ///         <see cref="WithOwner" /> to fully configure the relationship.
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
    ///         <see cref="WithOwner" /> to fully configure the relationship.
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
    ///         <see cref="WithOwner" /> to fully configure the relationship.
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
            new TypeIdentity(ownedType, DependentEntityType.Model),
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
    ///         <see cref="WithOwner" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <param name="ownedTypeName">The name of the entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the owned type and the relationship.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual OwnedNavigationBuilder OwnsMany(
        string ownedTypeName,
        string navigationName,
        Action<OwnedNavigationBuilder> buildAction)
    {
        Check.NotEmpty(ownedTypeName, nameof(ownedTypeName));
        Check.NotEmpty(navigationName, nameof(navigationName));
        Check.NotNull(buildAction, nameof(buildAction));

        using (DependentEntityType.Model.DelayConventions())
        {
            buildAction(OwnsManyBuilder(new TypeIdentity(ownedTypeName), navigationName));
            return this;
        }
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
    ///         <see cref="WithOwner" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <param name="ownedTypeName">The name of the entity type that this relationship targets.</param>
    /// <param name="ownedType">The CLR type of the entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the owned type and the relationship.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual OwnedNavigationBuilder OwnsMany(
        string ownedTypeName,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type ownedType,
        string navigationName,
        Action<OwnedNavigationBuilder> buildAction)
    {
        Check.NotEmpty(ownedTypeName, nameof(ownedTypeName));
        Check.NotNull(ownedType, nameof(ownedType));
        Check.NotEmpty(navigationName, nameof(navigationName));
        Check.NotNull(buildAction, nameof(buildAction));

        using (DependentEntityType.Model.DelayConventions())
        {
            buildAction(OwnsManyBuilder(new TypeIdentity(ownedTypeName, ownedType), navigationName));
            return this;
        }
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
    ///         <see cref="WithOwner" /> to fully configure the relationship.
    ///     </para>
    /// </remarks>
    /// <param name="ownedType">The entity type that this relationship targets.</param>
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <param name="buildAction">An action that performs configuration of the owned type and the relationship.</param>
    /// <returns>An object that can be used to configure the entity type.</returns>
    public virtual OwnedNavigationBuilder OwnsMany(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type ownedType,
        string navigationName,
        Action<OwnedNavigationBuilder> buildAction)
    {
        Check.NotNull(ownedType, nameof(ownedType));
        Check.NotEmpty(navigationName, nameof(navigationName));
        Check.NotNull(buildAction, nameof(buildAction));

        using (DependentEntityType.Model.DelayConventions())
        {
            buildAction(OwnsManyBuilder(new TypeIdentity(ownedType, DependentEntityType.Model), navigationName));
            return this;
        }
    }

    private OwnedNavigationBuilder OwnsManyBuilder(in TypeIdentity ownedType, string navigationName)
    {
        IMutableForeignKey foreignKey;
        using (var batch = DependentEntityType.Model.DelayConventions())
        {
            var navigationMember = MemberIdentity.Create(navigationName);
            var relationship = DependentEntityType.Builder.HasOwnership(ownedType, navigationMember, ConfigurationSource.Explicit)!;
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
            DependentEntityType,
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
    /// <param name="navigationName">
    ///     The name of the reference navigation property on this entity type that represents the relationship.
    /// </param>
    /// <returns>An object that can be used to configure the relationship.</returns>
    [RequiresUnreferencedCode("Use an overload that accepts a type")]
    public virtual ReferenceNavigationBuilder HasOne(string navigationName)
    {
        Check.NotEmpty(navigationName, nameof(navigationName));

        return DependentEntityType.ClrType == Model.DefaultPropertyBagType
            ? HasOne(navigationName, null) // Path only used by pre 3.0 snapshots
            : HasOne(DependentEntityType.GetNavigationMemberInfo(navigationName).GetMemberType(), navigationName);
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
            DependentEntityType,
            relatedEntityType,
            navigationName,
            foreignKey);
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
        ForeignKey foreignKey;
        if (navigationId.MemberInfo != null)
        {
            foreignKey = DependentEntityType.Builder.HasRelationship(
                relatedEntityType, navigationId.MemberInfo, ConfigurationSource.Explicit,
                targetIsPrincipal: DependentEntityType == relatedEntityType ? true : null)!.Metadata;
        }
        else
        {
            foreignKey = DependentEntityType.Builder.HasRelationship(
                relatedEntityType, navigationId.Name, ConfigurationSource.Explicit,
                targetIsPrincipal: DependentEntityType == relatedEntityType ? true : null)!.Metadata;
        }

        return foreignKey;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual EntityType FindRelatedEntityType(string relatedTypeName, string? navigationName)
    {
        EntityType? relatedEntityType = null;
        var model = DependentEntityType.Model;
        if (navigationName != null)
        {
            relatedEntityType = model.FindEntityType(relatedTypeName, navigationName, DependentEntityType);
        }
        else if (DependentEntityType.Name == relatedTypeName)
        {
            return DependentEntityType;
        }

        if (relatedEntityType == null
            && ((IReadOnlyModel)model).GetProductVersion()?.StartsWith("2.", StringComparison.Ordinal) == true)
        {
            var owner = DependentEntityType.FindOwnership()!.PrincipalEntityType;
            if (owner.Name == relatedTypeName
                || owner.ShortName() == relatedTypeName)
            {
                relatedEntityType = owner;
            }
        }

        return relatedEntityType ?? Builder.ModelBuilder.Entity(relatedTypeName, ConfigurationSource.Explicit)!.Metadata;
    }

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
    {
        var relatedEntityType = (EntityType?)DependentEntityType.FindInOwnershipPath(relatedType);
        if (relatedEntityType != null)
        {
            return relatedEntityType;
        }

        if (Builder.ModelBuilder.Metadata.IsShared(relatedType))
        {
            if (PrincipalEntityType.HasSharedClrType
                && PrincipalEntityType.ClrType == relatedType)
            {
                return PrincipalEntityType;
            }

            if (navigationName != null)
            {
                relatedEntityType = Builder.ModelBuilder.Metadata.FindEntityType(relatedType, navigationName, DependentEntityType);
            }
        }

        return relatedEntityType
            ?? DependentEntityType.Builder.ModelBuilder.Entity(
                relatedType, ConfigurationSource.Explicit, shouldBeOwned: false)!.Metadata;
    }

    /// <summary>
    ///     Configures the <see cref="ChangeTrackingStrategy" /> to be used for this entity type.
    ///     This strategy indicates how the context detects changes to properties for an instance of the entity type.
    /// </summary>
    /// <param name="changeTrackingStrategy">The change tracking strategy to be used.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual OwnedNavigationBuilder HasChangeTrackingStrategy(ChangeTrackingStrategy changeTrackingStrategy)
    {
        DependentEntityType.Builder.HasChangeTrackingStrategy(changeTrackingStrategy, ConfigurationSource.Explicit);

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
    public virtual OwnedNavigationBuilder UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
    {
        DependentEntityType.Builder.UsePropertyAccessMode(propertyAccessMode, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Adds seed data to this entity type. It is used to generate data motion migrations.
    /// </summary>
    /// <param name="data">
    ///     An array of seed data represented by anonymous types.
    /// </param>
    /// <returns>An object that can be used to configure the model data.</returns>
    public virtual DataBuilder HasData(params object[] data)
    {
        Check.NotNull(data, nameof(data));

        DependentEntityType.Builder.HasData(data, ConfigurationSource.Explicit);

        return new DataBuilder();
    }

    /// <summary>
    ///     Adds seed data to this entity type. It is used to generate data motion migrations.
    /// </summary>
    /// <param name="data">
    ///     A collection of seed data represented by anonymous types.
    /// </param>
    /// <returns>An object that can be used to configure the model data.</returns>
    public virtual DataBuilder HasData(IEnumerable<object> data)
    {
        Check.NotNull(data, nameof(data));

        DependentEntityType.Builder.HasData(data, ConfigurationSource.Explicit);

        return new DataBuilder();
    }
}
