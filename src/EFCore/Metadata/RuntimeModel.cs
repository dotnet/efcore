// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Metadata about the shape of entities, the relationships between them, and how they map to
///     the database. A model is typically created by overriding the
///     <see cref="DbContext.OnModelCreating(ModelBuilder)" /> method on a derived
///     <see cref="DbContext" />.
/// </summary>
/// <remarks>
///     <para>
///         This is a light-weight implementation that is constructed from a built model and is not meant to be used at design-time.
///     </para>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
///         examples.
///     </para>
/// </remarks>
public class RuntimeModel : RuntimeAnnotatableBase, IRuntimeModel
{
    private bool _skipDetectChanges;
    private Guid _modelId;
    private readonly Dictionary<string, RuntimeEntityType> _entityTypes;
    private readonly Dictionary<Type, List<RuntimeEntityType>> _sharedTypes = new();
    private readonly Dictionary<Type, RuntimeTypeMappingConfiguration> _typeConfigurations;

    private readonly ConcurrentDictionary<Type, PropertyInfo?> _indexerPropertyInfoMap = new();
    private readonly ConcurrentDictionary<Type, string> _clrTypeNameMap = new();
    private readonly ConcurrentDictionary<Type, RuntimeEntityType> _adHocEntityTypes = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    [Obsolete("Use a constructor with parameters")]
    public RuntimeModel()
        : base()
    {
        _entityTypes = new(StringComparer.Ordinal);
        _typeConfigurations = new();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public RuntimeModel(
        bool skipDetectChanges,
        Guid modelId,
        int entityTypeCount,
        int typeConfigurationCount = 0)
    {
        _skipDetectChanges = skipDetectChanges;
        _modelId = modelId;
        _entityTypes = new(entityTypeCount, StringComparer.Ordinal);
        _typeConfigurations = new(typeConfigurationCount);
    }

    /// <summary>
    ///     Sets a value indicating whether <see cref="ChangeTracker.DetectChanges" /> should be called.
    /// </summary>
    [Obsolete("This is set in the constructor now")]
    public virtual void SetSkipDetectChanges(bool skipDetectChanges)
        => _skipDetectChanges = skipDetectChanges;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    [Obsolete("This is set in the constructor now")]
    public virtual Guid ModelId { get => _modelId; set => _modelId = value; }

    /// <summary>
    ///     Adds an entity type with a defining navigation to the model.
    /// </summary>
    /// <param name="name">The name of the entity type to be added.</param>
    /// <param name="type">The CLR class that is used to represent instances of this type.</param>
    /// <param name="sharedClrType">Whether this entity type can share its ClrType with other entities.</param>
    /// <param name="baseType">The base type of this entity type.</param>
    /// <param name="discriminatorProperty">The name of the property that will be used for storing a discriminator value.</param>
    /// <param name="changeTrackingStrategy">The change tracking strategy for this entity type.</param>
    /// <param name="indexerPropertyInfo">The <see cref="PropertyInfo" /> for the indexer on the associated CLR type if one exists.</param>
    /// <param name="propertyBag">
    ///     A value indicating whether this entity type has an indexer which is able to contain arbitrary properties
    ///     and a method that can be used to determine whether a given indexer property contains a value.
    /// </param>
    /// <param name="discriminatorValue">The discriminator value for this entity type.</param>
    /// <param name="derivedTypesCount">The expected number of directly derived entity types.</param>
    /// <param name="propertyCount">The expected number of declared properties for this entity type.</param>
    /// <param name="complexPropertyCount">The expected number of declared complex properties for this entity type.</param>
    /// <param name="navigationCount">The expected number of declared navigations for this entity type.</param>
    /// <param name="skipNavigationCount">The expected number of declared skip navigation for this entity type.</param>
    /// <param name="servicePropertyCount">The expected number of declared service properties for this entity type.</param>
    /// <param name="foreignKeyCount">The expected number of declared foreign keys for this entity type.</param>
    /// <param name="unnamedIndexCount">The expected number of declared unnamed indexes for this entity type.</param>
    /// <param name="namedIndexCount">The expected number of declared named indexes for this entity type.</param>
    /// <param name="keyCount">The expected number of declared keys for this entity type.</param>
    /// <param name="triggerCount">The expected number of declared triggers for this entity type.</param>
    /// <returns>The new entity type.</returns>
    public virtual RuntimeEntityType AddEntityType(
        string name,
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type,
        RuntimeEntityType? baseType = null,
        bool sharedClrType = false,
        string? discriminatorProperty = null,
        ChangeTrackingStrategy changeTrackingStrategy = ChangeTrackingStrategy.Snapshot,
        PropertyInfo? indexerPropertyInfo = null,
        bool propertyBag = false,
        object? discriminatorValue = null,
        int derivedTypesCount = 0,
        int propertyCount = 0,
        int complexPropertyCount = 0,
        int navigationCount = 0,
        int skipNavigationCount = 0,
        int servicePropertyCount = 0,
        int foreignKeyCount = 0,
        int unnamedIndexCount = 0,
        int namedIndexCount = 0,
        int keyCount = 0,
        int triggerCount = 0)
    {
        var entityType = new RuntimeEntityType(
            name,
            type,
            sharedClrType,
            this,
            baseType,
            discriminatorProperty,
            changeTrackingStrategy,
            indexerPropertyInfo,
            propertyBag,
            discriminatorValue,
            derivedTypesCount: derivedTypesCount,
            propertyCount: propertyCount,
            complexPropertyCount: complexPropertyCount,
            foreignKeyCount: foreignKeyCount,
            navigationCount: navigationCount,
            skipNavigationCount: skipNavigationCount,
            servicePropertyCount: servicePropertyCount,
            unnamedIndexCount: unnamedIndexCount,
            namedIndexCount: namedIndexCount,
            keyCount: keyCount,
            triggerCount: triggerCount);

        if (sharedClrType)
        {
            if (_sharedTypes.TryGetValue(type, out var existingTypes))
            {
                existingTypes.Add(entityType);
            }
            else
            {
                var types = new List<RuntimeEntityType> { entityType };
                _sharedTypes.Add(type, types);
            }
        }

        _entityTypes.Add(name, entityType);

        return entityType;
    }

    /// <summary>
    ///     Adds an ad-hoc entity type to the model.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    public virtual RuntimeEntityType GetOrAddAdHocEntityType(RuntimeEntityType entityType)
    {
        entityType.Reparent(this);
        entityType.AddRuntimeAnnotation(CoreAnnotationNames.AdHocModel, true);
        return _adHocEntityTypes.GetOrAdd(((IReadOnlyTypeBase)entityType).ClrType, entityType);
    }

    /// <summary>
    ///     Gets all ad-hoc entity types defined in the model.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <returns>All entity types defined in the model.</returns>
    [DebuggerStepThrough]
    public virtual IEnumerable<IReadOnlyEntityType> GetAdHocEntityTypes()
        => _adHocEntityTypes.Values;

    /// <summary>
    ///     Gets the entity type with the given name. Returns <see langword="null" /> if no entity type with the given name is found
    ///     or the given CLR type is being used by shared type entity type
    ///     or the entity type has a defining navigation.
    /// </summary>
    /// <param name="name">The name of the entity type to find.</param>
    /// <returns>The entity type, or <see langword="null" /> if none is found.</returns>
    public virtual RuntimeEntityType? FindEntityType(string name)
        => _entityTypes.TryGetValue(name, out var entityType)
            ? entityType
            : null;

    /// <summary>
    ///     Gets the entity type with the given name. Returns <see langword="null" /> if no entity type with the given name has been
    ///     mapped as an ad-hoc type.
    /// </summary>
    /// <param name="clrType">The CLR type of the entity type to find.</param>
    /// <returns>The entity type, or <see langword="null" /> if none is found.</returns>
    public virtual RuntimeEntityType? FindAdHocEntityType(Type clrType)
        => _adHocEntityTypes.TryGetValue(clrType, out var entityType)
            ? entityType
            : null;

    private RuntimeEntityType? FindEntityType(Type type)
        => FindEntityType(GetDisplayName(type));

    private RuntimeEntityType? FindEntityType(
        string name,
        string definingNavigationName,
        IReadOnlyEntityType definingEntityType)
        => FindEntityType(definingEntityType.GetOwnedName(name, definingNavigationName));

    private IEnumerable<RuntimeEntityType> FindEntityTypes(Type type)
    {
        var entityType = FindEntityType(GetDisplayName(type));
        var result = entityType == null
            ? Enumerable.Empty<RuntimeEntityType>()
            : new[] { entityType };

        return _sharedTypes.TryGetValue(type, out var sharedTypes)
            ? result.Concat(sharedTypes.OrderBy(n => n.Name, StringComparer.Ordinal))
            : result;
    }

    /// <summary>
    ///     Gets all entity types defined in the model.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
    /// </remarks>
    /// <returns>All entity types defined in the model.</returns>
    private IEnumerable<RuntimeEntityType> GetEntityTypes()
        => _entityTypes.Values.OrderBy(e => e.Name, StringComparer.Ordinal);

    /// <summary>
    ///     Adds configuration for a scalar type.
    /// </summary>
    /// <param name="clrType">The type of value the property will hold.</param>
    /// <param name="maxLength">The maximum length of data that is allowed in this property type.</param>
    /// <param name="unicode">A value indicating whether or not the property can persist Unicode characters.</param>
    /// <param name="precision">The precision of data that is allowed in this property type.</param>
    /// <param name="scale">The scale of data that is allowed in this property type.</param>
    /// <param name="providerPropertyType">
    ///     The type that the property value will be converted to before being sent to the database provider.
    /// </param>
    /// <param name="valueConverter">The custom <see cref="ValueConverter" /> for this type.</param>
    /// <returns>The newly created property.</returns>
    public virtual RuntimeTypeMappingConfiguration AddTypeMappingConfiguration(
        Type clrType,
        int? maxLength = null,
        bool? unicode = null,
        int? precision = null,
        int? scale = null,
        Type? providerPropertyType = null,
        ValueConverter? valueConverter = null)
    {
        var typeConfiguration = new RuntimeTypeMappingConfiguration(
            clrType,
            maxLength,
            unicode,
            precision,
            scale,
            providerPropertyType,
            valueConverter);

        _typeConfigurations.Add(clrType, typeConfiguration);

        return typeConfiguration;
    }

    private string GetDisplayName(Type type)
        => _clrTypeNameMap.GetOrAdd(type, t => t.DisplayName());

    private PropertyInfo? FindIndexerPropertyInfo([DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type)
        => _indexerPropertyInfoMap.GetOrAdd(type, type.FindIndexerProperty());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual IModel FinalizeModel()
        => this;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    object? IRuntimeModel.RelationalModel
        => ((IAnnotatable)this).FindRuntimeAnnotationValue("Relational:RelationalModel");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DebugView DebugView
        => new(
            () => ((IReadOnlyModel)this).ToDebugString(),
            () => ((IReadOnlyModel)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    bool IRuntimeModel.SkipDetectChanges
    {
        [DebuggerStepThrough]
        get => _skipDetectChanges;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    PropertyAccessMode IReadOnlyModel.GetPropertyAccessMode()
        => throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);

    /// <inheritdoc />
    [DebuggerStepThrough]
    ChangeTrackingStrategy IReadOnlyModel.GetChangeTrackingStrategy()
        => throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IModel.IsIndexerMethod(MethodInfo methodInfo)
        => !methodInfo.IsStatic
            && methodInfo is { IsSpecialName: true, DeclaringType: not null }
            && FindIndexerPropertyInfo(methodInfo.DeclaringType) is PropertyInfo indexerProperty
            && (methodInfo == indexerProperty.GetMethod || methodInfo == indexerProperty.SetMethod);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyEntityType? IReadOnlyModel.FindEntityType(string name)
        => FindEntityType(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEntityType? IModel.FindEntityType(string name)
        => FindEntityType(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyEntityType? IReadOnlyModel.FindEntityType(Type type)
        => FindEntityType(type);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEntityType? IModel.FindEntityType([DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type)
        => FindEntityType(type);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyEntityType? IReadOnlyModel.FindEntityType(
        string name,
        string definingNavigationName,
        IReadOnlyEntityType definingEntityType)
        => FindEntityType(name, definingNavigationName, (RuntimeEntityType)definingEntityType);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEntityType? IModel.FindEntityType(
        string name,
        string definingNavigationName,
        IEntityType definingEntityType)
        => FindEntityType(name, definingNavigationName, (RuntimeEntityType)definingEntityType);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IReadOnlyEntityType? IReadOnlyModel.FindEntityType(
        Type type,
        string definingNavigationName,
        IReadOnlyEntityType definingEntityType)
        => FindEntityType(type.ShortDisplayName(), definingNavigationName, definingEntityType);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyEntityType> IReadOnlyModel.GetEntityTypes()
        => GetEntityTypes();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IEntityType> IModel.GetEntityTypes()
        => GetEntityTypes();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IReadOnlyEntityType> IReadOnlyModel.FindEntityTypes(Type type)
        => FindEntityTypes(type);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IEntityType> IModel.FindEntityTypes(Type type)
        => FindEntityTypes(type);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IReadOnlyModel.IsShared([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
        => _sharedTypes.ContainsKey(type);

    /// <inheritdoc />
    IEnumerable<ITypeMappingConfiguration> IModel.GetTypeMappingConfigurations()
        => _typeConfigurations.Values;

    /// <inheritdoc />
    ITypeMappingConfiguration? IModel.FindTypeMappingConfiguration(Type propertyType)
        => _typeConfigurations.Count == 0
            ? null
            : _typeConfigurations.GetValueOrDefault(propertyType);

    /// <inheritdoc />
    Guid IReadOnlyModel.ModelId
        => _modelId;
}
