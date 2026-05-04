// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace Microsoft.EntityFrameworkCore.Query.Internal;

#pragma warning disable EF1001 // Internal EF Core API usage.

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
/// <remarks>
///     <para>
///         A non-generated materializer that reads entity instances from a <see cref="DbDataReader" />.
///         Supports tracking, no-tracking, and TPH inheritance.
///     </para>
///     <para>
///         Per-property setters are reused from the model's existing <see cref="IClrPropertySetter" />
///         infrastructure (<see cref="ClrPropertySetter{TEntity, TStructural, TValue}" />). At the typed
///         dispatch site we cast to the concrete generic type and call the typed overload, avoiding boxing.
///     </para>
///     <para>
///         The materializer loop is provider-specific: it reads typed values from the
///         <see cref="DbDataReader" /> (dispatching on CLR type) and passes them to the setters.
///         This code is regular C# and does not need generation.
///     </para>
/// </remarks>
public class RelationalEntityMaterializer<TEntity> : RelationalEntityMaterializer
    where TEntity : class, new()
{
    private readonly IEntityType _entityType;
    private readonly IKey? _primaryKey;
    private readonly KeyColumnInfo[]? _keyColumns;
    private readonly bool _isTracking;
    private readonly bool _isNullable;

    /// <summary>
    ///     The concrete type infos for this entity type hierarchy. For non-TPH entities, this contains
    ///     a single entry. For TPH, it contains one entry per concrete type in the hierarchy.
    /// </summary>
    private readonly ConcreteTypeInfo[] _concreteTypes;

    /// <summary>
    ///     Dictionary for O(1) discriminator value → concrete type index lookup.
    ///     Null when there's no discriminator (single concrete type).
    /// </summary>
    private readonly Dictionary<object, int>? _discriminatorMap;

    /// <summary>
    ///     The column index of the discriminator in the DbDataReader, or -1 if there is no discriminator.
    /// </summary>
    private readonly int _discriminatorColumnIndex;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RelationalEntityMaterializer(
        IEntityType entityType,
        IReadOnlyDictionary<IPropertyBase, int> projectionMap,
        bool isTracking,
        bool isNullable = false)
    {
        _entityType = entityType;
        var primaryKey = entityType.FindPrimaryKey();
        _primaryKey = isTracking ? primaryKey : null;
        _isTracking = _primaryKey is not null;
        _isNullable = isNullable;

        // Build key column info — needed for tracking (identity resolution) and for nullable entities (null detection)
        if (primaryKey is not null && (isTracking || isNullable))
        {
            _keyColumns = new KeyColumnInfo[primaryKey.Properties.Count];

            for (var i = 0; i < primaryKey.Properties.Count; i++)
            {
                var keyProperty = primaryKey.Properties[i];
                _keyColumns[i] = new KeyColumnInfo(projectionMap[keyProperty], keyProperty.ClrType);
            }
        }

        // Build per-concrete-type materialization info
        var discriminatorProperty = entityType.FindDiscriminatorProperty();
        _discriminatorColumnIndex = discriminatorProperty is not null && projectionMap.TryGetValue(discriminatorProperty, out var discIdx)
            ? discIdx
            : -1;

        var concreteEntityTypes = entityType.GetConcreteDerivedTypesInclusive().ToList();
        _concreteTypes = new ConcreteTypeInfo[concreteEntityTypes.Count];

        // Set up discriminator comparison strategy
        if (discriminatorProperty is not null && concreteEntityTypes.Count > 1)
        {
            var discriminatorComparer = discriminatorProperty.GetKeyValueComparer();

            // TODO: Possible optimization: for the common case of small hierarchies (<10), just do an array with a linear scan rather
            // than a dictionary
            _discriminatorMap = discriminatorComparer.IsDefault()
                ? new Dictionary<object, int>(concreteEntityTypes.Count)
                : new Dictionary<object, int>(concreteEntityTypes.Count, new ValueComparerEqualityComparer(discriminatorComparer));
        }

        for (var i = 0; i < concreteEntityTypes.Count; i++)
        {
            var concreteType = concreteEntityTypes[i];
            var discriminatorValue = concreteType.GetDiscriminatorValue();

            if (discriminatorValue is not null)
            {
                _discriminatorMap?.Add(discriminatorValue, i);
            }

            // Build property materializers for this concrete type's properties (declared + inherited)
            var properties = concreteType.GetProperties().Where(p => !p.IsShadowProperty()).ToList();
            var materializers = new List<PropertyMaterializer>(properties.Count);

            foreach (var property in properties)
            {
                if (!projectionMap.TryGetValue(property, out var columnIndex))
                {
                    continue;
                }

                var typeMapping = (RelationalTypeMapping)property.GetTypeMapping();
                var setter = ((IRuntimePropertyBase)property).MaterializationSetter;
                var reader = typeMapping.CreateReader(columnIndex);

                materializers.Add(new PropertyMaterializer(columnIndex, property.IsNullable, setter, reader));
            }

            _concreteTypes[i] = new ConcreteTypeInfo(
                concreteType,
                discriminatorValue,
                materializers.ToArray());
        }
    }

    /// <inheritdoc />
    public override IEntityType EntityType => _entityType;

    /// <inheritdoc />
    public override object? Materialize(
        QueryContext queryContext,
        DbDataReader dataReader,
        ResultContext resultContext,
        SingleQueryResultCoordinator resultCoordinator)
        => MaterializeTyped(queryContext, dataReader, resultContext, resultCoordinator);

    /// <inheritdoc />
    protected override Delegate GetMaterializeDelegateCore()
        => (Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, TEntity?>)MaterializeTyped;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TEntity? MaterializeTyped(
        QueryContext queryContext,
        DbDataReader dataReader,
        ResultContext resultContext,
        SingleQueryResultCoordinator resultCoordinator)
    {

        TEntity entity;

        if (CollectionIncludes is not null)
        {
            // Collection include protocol: this method is called multiple times per parent entity.
            // On first call (resultContext.Values == null), we materialize the parent and initialize collections.
            // On subsequent calls, we populate collection elements.
            // When the parent changes or rows end, we set ResultReady = true and return the parent.

            if (resultContext.Values is null)
            {
                // First call for this parent entity
                entity = MaterializeEntity(queryContext, dataReader)!;
                if (entity is null)
                {
                    return null;
                }

                // Store entity reference for subsequent calls (using Values as a marker + storage)
                resultContext.Values = [entity];

                // Reset per-include result contexts: when the outer entity changes, the per-include
                // contexts must be cleared so included materializers start fresh on the new entity.
                if (ReferenceIncludes is not null)
                {
                    for (var i = 0; i < ReferenceIncludes.Count; i++)
                    {
                        ReferenceIncludes[i].ResultContext.Values = null;
                    }
                }

                // Process reference includes ONCE during initialization. After flattening, reference
                // include materializers no longer have collection includes, so they complete in a single
                // call without affecting ResultReady.
                ProcessReferenceIncludes(queryContext, dataReader, resultCoordinator, entity);

                // Initialize collection includes. Direct collections use the outer entity as parent;
                // flattened collections (from reference includes) obtain their parent from the provider
                // (which reads the entity from the reference include's ResultContext, populated above).
                for (var i = 0; i < CollectionIncludes.Count; i++)
                {
                    var ci = CollectionIncludes[i];
                    var parentEntity = ci.ParentEntityProvider is not null
                        ? ci.ParentEntityProvider()
                        : (object)entity;
                    InitializeIncludeCollection(queryContext, dataReader, resultCoordinator, _isTracking, parentEntity, ci);
                }
            }
            else
            {
                entity = (TEntity)resultContext.Values[0];
            }

            // Populate ALL collection elements from the current row. Both direct and flattened
            // collections are processed at the same level, using the parent entity stored in the
            // SingleQueryCollectionContext during initialization.
            for (var i = 0; i < CollectionIncludes.Count; i++)
            {
                PopulateIncludeCollection(queryContext, dataReader, resultCoordinator, CollectionIncludes[i]);
            }

            // If ResultReady is true, return the entity; otherwise return default (MoveNext will call again)
            return resultCoordinator.ResultReady ? entity : default;
        }

        // Reference includes only (no collection includes). After flattening, reference include
        // materializers never have collection includes, so they complete in a single call.
        if (ReferenceIncludes is not null)
        {
            if (resultContext.Values is null)
            {
                entity = MaterializeEntity(queryContext, dataReader)!;
                if (entity is null)
                {
                    return null;
                }

                for (var i = 0; i < ReferenceIncludes.Count; i++)
                {
                    ReferenceIncludes[i].ResultContext.Values = null;
                }

                ProcessReferenceIncludes(queryContext, dataReader, resultCoordinator, entity);

                resultContext.Values = [entity];

                return entity;
            }

            return (TEntity?)resultContext.Values[0];
        }

        // No includes at all — cache the entity in resultContext.Values on first call and use the cache on
        // subsequent calls. This is necessary when this materializer is used as a reference include inside an
        // outer materializer that has collection includes: the outer materializer calls us on every row to drive
        // its collection population, and we must not re-read from the reader after the first call.
        if (resultContext.Values is null)
        {
            entity = MaterializeEntity(queryContext, dataReader)!;
            if (entity is not null)
            {
                resultContext.Values = [entity];
            }

            return entity;
        }

        return (TEntity?)resultContext.Values[0];
    }

    /// <summary>
    ///     Materializes the entity itself (identity resolution, instantiation, property population, tracking).
    /// </summary>
    private TEntity? MaterializeEntity(
        QueryContext queryContext,
        DbDataReader dataReader)
    {
        // Check for null keys — needed for tracking (identity resolution) and nullable entities (LEFT JOIN)
        if (_keyColumns is not null)
        {
            var hasNullKey = false;
            object[]? keyValues = _isTracking ? new object[_keyColumns.Length] : null;

            for (var i = 0; i < _keyColumns.Length; i++)
            {
                ref readonly var keyCol = ref _keyColumns[i];

                if (dataReader.IsDBNull(keyCol.ColumnIndex))
                {
                    hasNullKey = true;
                    break;
                }

                if (keyValues is not null)
                {
                    keyValues[i] = dataReader.GetFieldValue<object>(keyCol.ColumnIndex);

                    if (keyValues[i].GetType() != keyCol.ClrType)
                    {
                        keyValues[i] = Convert.ChangeType(keyValues[i], keyCol.ClrType);
                    }
                }
            }

            if (hasNullKey)
            {
                return null;
            }

            if (_isTracking)
            {
                var entry = queryContext.TryGetEntry(_primaryKey!, keyValues!, throwOnNullKey: true, out _);
                if (entry is not null)
                {
                    return (TEntity)entry.Entity;
                }
            }
        }

        var (entity, typeInfo) = InstantiateEntity(dataReader);

        PopulateProperties(dataReader, entity, typeInfo.PropertyMaterializers);

        if (_isTracking)
        {
            queryContext.StartTracking(typeInfo.EntityType, entity, Snapshot.Empty);
        }

        return entity;
    }

    /// <summary>
    ///     Processes reference includes for the given entity.
    /// </summary>
    private void ProcessReferenceIncludes(
        QueryContext queryContext,
        DbDataReader dataReader,
        SingleQueryResultCoordinator resultCoordinator,
        TEntity entity)
    {
        if (ReferenceIncludes is null)
        {
            return;
        }

        for (var i = 0; i < ReferenceIncludes.Count; i++)
        {
            var include = ReferenceIncludes[i];

            // TPH guard: the navigation may be declared on a derived type that this entity isn't
            if (!include.Navigation.DeclaringEntityType.ClrType.IsInstanceOfType(entity))
            {
                continue;
            }

            // Each include has its own ResultContext so its collection-include protocol state
            // does not collide with the outer materializer's context (which holds a different entity type).
            var relatedEntity = include.Materializer.Materialize(queryContext, dataReader, include.ResultContext, resultCoordinator);

            if (_isTracking && !include.IsKeylessEntityType)
            {
                // Tracking query with a trackable declaring type: the state manager handles fixup
                // automatically when both entities are tracked. We only need to explicitly mark the
                // navigation as loaded when the related entity is null (LEFT JOIN with no match),
                // since the state manager won't see it.
                if (relatedEntity is null)
                {
                    queryContext.SetNavigationIsLoaded(entity, include.Navigation);
                }
            }
            else
            {
                // Non-tracking query, or the declaring entity type is keyless (can't be tracked):
                // perform fixup manually by setting the navigation property directly.
                include.Navigation.SetIsLoadedWhenNoTracking(entity);

                if (relatedEntity is not null)
                {
                    include.NavigationSetter.SetClrValue(entity, relatedEntity);

                    if (include.InverseNavigation is { IsCollection: false } inverse)
                    {
                        inverse.SetIsLoadedWhenNoTracking(relatedEntity);
                        include.InverseNavigationSetter?.SetClrValue(relatedEntity, entity);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Initializes a collection include for the given parent entity.
    ///     Corresponds to InitializeIncludeCollection in the generated shaper's ClientMethods.
    /// </summary>
    private static void InitializeIncludeCollection(
        QueryContext queryContext,
        DbDataReader dataReader,
        SingleQueryResultCoordinator resultCoordinator,
        bool isTracking,
        object? parentEntity,
        CollectionIncludeInfo includeInfo)
    {
        object? collection = null;

        if (parentEntity is not null
            && includeInfo.Navigation.DeclaringEntityType.ClrType.IsInstanceOfType(parentEntity))
        {
            if (isTracking && !includeInfo.IsKeylessEntityType)
            {
                queryContext.SetNavigationIsLoaded(parentEntity, includeInfo.Navigation);
            }
            else
            {
                includeInfo.Navigation.SetIsLoadedWhenNoTracking(parentEntity);
            }

            collection = includeInfo.CollectionAccessor.GetOrCreate(parentEntity, forMaterialization: true);
        }

        var parentKey = includeInfo.ParentIdentifier(queryContext, dataReader);
        var outerKey = includeInfo.OuterIdentifier(queryContext, dataReader);

        resultCoordinator.SetSingleQueryCollectionContext(
            includeInfo.CollectionId,
            new SingleQueryCollectionContext(parentEntity, collection, parentKey, outerKey));
    }

    /// <summary>
    ///     Populates a collection include from the current DbDataReader row.
    ///     Corresponds to PopulateIncludeCollection in the generated shaper's ClientMethods.
    /// </summary>
    private void PopulateIncludeCollection(
        QueryContext queryContext,
        DbDataReader dataReader,
        SingleQueryResultCoordinator resultCoordinator,
        CollectionIncludeInfo ci)
    {
        var collectionContext = resultCoordinator.Collections[ci.CollectionId]!;
        var parent = collectionContext.Parent;

        // TPH guard: check the parent entity stored in the collection context (which may be
        // the outer entity for direct collections or a reference entity for flattened collections).
        if (parent is null || !ci.Navigation.DeclaringEntityType.ClrType.IsInstanceOfType(parent))
        {
            return;
        }

        if (resultCoordinator.HasNext == false)
        {
            // Outer enumerator has ended — materialize last pending element
            GenerateCurrentElementIfPending();
            return;
        }

        if (!CompareIdentifiers(
                ci.OuterIdentifierValueComparers,
                ci.OuterIdentifier(queryContext, dataReader),
                collectionContext.OuterIdentifier))
        {
            // Outer changed — collection has ended. Materialize last element.
            GenerateCurrentElementIfPending();

            // If parent also changed then this row is pointing to element of next collection
            if (!CompareIdentifiers(
                    ci.ParentIdentifierValueComparers,
                    ci.ParentIdentifier(queryContext, dataReader),
                    collectionContext.ParentIdentifier))
            {
                resultCoordinator.HasNext = true;
            }

            return;
        }

        var innerKey = ci.SelfIdentifier(queryContext, dataReader);
        if (innerKey.All(e => e == null))
        {
            // No correlated element (null FK)
            return;
        }

        if (collectionContext.SelfIdentifier is not null)
        {
            if (CompareIdentifiers(ci.SelfIdentifierValueComparers, innerKey, collectionContext.SelfIdentifier))
            {
                // Repeated row for current element (nested includes may need processing)
                if (collectionContext.ResultContext.Values is not null)
                {
                    ProcessCurrentElementRow();
                }

                resultCoordinator.ResultReady = false;
                return;
            }

            // New element — materialize the previous one first
            GenerateCurrentElementIfPending();
            resultCoordinator.HasNext = null;
            collectionContext.UpdateSelfIdentifier(innerKey);
        }
        else
        {
            // First element in collection
            collectionContext.UpdateSelfIdentifier(innerKey);
        }

        ProcessCurrentElementRow();
        resultCoordinator.ResultReady = false;

        void ProcessCurrentElementRow()
        {
            var previousResultReady = resultCoordinator.ResultReady;
            resultCoordinator.ResultReady = true;

            var relatedEntity = ci.InnerMaterializer.Materialize(
                queryContext, dataReader, collectionContext.ResultContext, resultCoordinator);

            if (resultCoordinator.ResultReady)
            {
                collectionContext.ResultContext.Values = null;
                if (!_isTracking || ci.IsKeylessEntityType)
                {
                    ci.CollectionAccessor.Add(parent, relatedEntity!, forMaterialization: false);
                    if (ci.InverseNavigation is { IsCollection: false })
                    {
                        ci.InverseNavigationSetter?.SetClrValue(relatedEntity!, parent);
                        ci.InverseNavigation.SetIsLoadedWhenNoTracking(relatedEntity!);
                    }
                }
            }

            resultCoordinator.ResultReady &= previousResultReady;
        }

        void GenerateCurrentElementIfPending()
        {
            if (collectionContext.ResultContext.Values is not null)
            {
                resultCoordinator.HasNext = false;
                ProcessCurrentElementRow();
            }

            collectionContext.UpdateSelfIdentifier(null);
        }
    }

    private static bool CompareIdentifiers(
        IReadOnlyList<Func<object, object, bool>> valueComparers,
        object[] left,
        object[] right)
    {
        for (var i = 0; i < left.Length; i++)
        {
            if (!valueComparers[i](left[i], right[i]))
            {
                return false;
            }
        }

        return true;
    }

    private (TEntity Entity, ConcreteTypeInfo TypeInfo) InstantiateEntity(DbDataReader dataReader)
    {
        return _concreteTypes is [var entityType]
            ? (new TEntity(), entityType)
            : InstantiateInheritanceEntity(dataReader);

        (TEntity Entity, ConcreteTypeInfo TypeInfo) InstantiateInheritanceEntity(DbDataReader dataReader)
        {
            if (_discriminatorColumnIndex < 0)
            {
                throw new InvalidOperationException(
                    $"Multiple concrete types ({_concreteTypes.Length}) but no discriminator column.");
            }

            var discriminatorValue = dataReader.GetValue(_discriminatorColumnIndex);

            if (_discriminatorMap!.TryGetValue(discriminatorValue, out var index))
            {
                ref readonly var typeInfo = ref _concreteTypes[index];
                return ((TEntity)Activator.CreateInstance(typeInfo.EntityType.ClrType)!, typeInfo);
            }

            throw new InvalidOperationException(
                $"Unable to materialize entity of type '{typeof(TEntity).Name}'. "
                + $"No concrete type found for discriminator value '{discriminatorValue}'.");
        }
    }

    /// <summary>
    ///     Reads and sets all mapped properties on <paramref name="entity" /> from the current row.
    ///     Each <see cref="PropertyMaterializer.Reader" /> is produced by
    ///     <see cref="RelationalTypeMapping.CreateReader" /> and is either a
    ///     <see cref="RelationalTypedValueReader" /> (no converter) or a
    ///     <see cref="ConvertingTypedValueReader{TModel,TProvider,TState}" /> wrapping it (with converter).
    ///     The <see cref="DbDataReader" /> is passed as state per-call, so all reader instances are
    ///     immutable and safe to share across concurrent executions.
    /// </summary>
    private static void PopulateProperties(DbDataReader dataReader, TEntity entity, PropertyMaterializer[] materializers)
    {
        for (var i = 0; i < materializers.Length; i++)
        {
            ref readonly var pm = ref materializers[i];

            if (pm.IsNullable && dataReader.IsDBNull(pm.ColumnIndex))
            {
                continue;
            }

            pm.Setter.SetClrValue(entity, pm.Reader, dataReader);
        }
    }

    private readonly struct ConcreteTypeInfo(
        IEntityType entityType,
        object? discriminatorValue,
        PropertyMaterializer[] propertyMaterializers)
    {
        public IEntityType EntityType { get; } = entityType;
        public object? DiscriminatorValue { get; } = discriminatorValue;
        public PropertyMaterializer[] PropertyMaterializers { get; } = propertyMaterializers;
    }

    private readonly struct PropertyMaterializer(
        int columnIndex,
        bool isNullable,
        IClrPropertySetter setter,
        ITypedValueReader<DbDataReader> reader)
    {
        public int ColumnIndex { get; } = columnIndex;
        public bool IsNullable { get; } = isNullable;
        public IClrPropertySetter Setter { get; } = setter;

        /// <summary>
        ///     The reader for this property, produced by <see cref="RelationalTypeMapping.CreateReader" />.
        ///     Immutable; the <see cref="DbDataReader" /> is passed as state per-call.
        /// </summary>
        public ITypedValueReader<DbDataReader> Reader { get; } = reader;
    }

    private readonly struct KeyColumnInfo(int columnIndex, Type clrType)
    {
        public int ColumnIndex { get; } = columnIndex;
        public Type ClrType { get; } = clrType;
    }

    private sealed class ValueComparerEqualityComparer(ValueComparer comparer) : IEqualityComparer<object>
    {
        public new bool Equals(object? x, object? y) => comparer.Equals(x, y);
        public int GetHashCode(object obj) => comparer.GetHashCode(obj);
    }
}
