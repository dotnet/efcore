// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;
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
///         infrastructure (<see cref="ClrPropertySetter{TStructuralType, TStructural, TValue}" />). At the typed
///         dispatch site we cast to the concrete generic type and call the typed overload, avoiding boxing.
///     </para>
///     <para>
///         The materializer loop is provider-specific: it reads typed values from the
///         <see cref="DbDataReader" /> (dispatching on CLR type) and passes them to the setters.
///         This code is regular C# and does not need generation.
///     </para>
/// </remarks>
public class RelationalStructuralTypeMaterializer<TStructuralType> : RelationalStructuralTypeMaterializer
{
    private readonly ITypeBase _structuralType;
    private readonly IKey? _primaryKey;
    private readonly KeyColumnInfo[]? _keyColumns;
    private readonly bool _isTracking;

    /// <summary>
    ///     For nullable structural types (table-split optional dependents or optional complex types):
    ///     column indices of required non-PK properties that must all be non-null for the type to exist.
    /// </summary>
    private readonly int[]? _requiredNonPkColumnIndices;

    /// <summary>
    ///     For nullable structural types where all non-principal-shared non-PK properties are nullable:
    ///     column indices where at least one must be non-null.
    /// </summary>
    private readonly int[]? _optionalNonPkColumnIndices;

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
    ///     Typed reader for the discriminator column, applying value conversion (e.g. Int64 → enum).
    ///     Null when there's no discriminator.
    /// </summary>
    private readonly ITypedValueReader<DbDataReader>? _discriminatorReader;

    /// <summary>
    ///     JSON-mapped complex properties on this entity. Each entry contains the JSON column index,
    ///     the materializer for the complex type, and the setter to assign it on the entity.
    ///     Null when the entity has no JSON complex properties.
    /// </summary>
    private readonly JsonComplexPropertyInfo[]? _jsonComplexProperties;



    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RelationalStructuralTypeMaterializer(
        ITypeBase structuralType,
        IReadOnlyDictionary<IPropertyBase, int> projectionMap,
        bool isTracking,
        bool isNullable = false,
        IInstantiationBindingInterceptor[]? bindingInterceptors = null)
    {
        _structuralType = structuralType;
        var entityType = structuralType as IEntityType;
        var primaryKey = entityType?.FindPrimaryKey();
        _primaryKey = isTracking ? primaryKey : null;
        _isTracking = _primaryKey is not null;

        // Build key column info — needed for tracking (identity resolution) and for nullable entities (null detection)
        if (primaryKey is not null && (isTracking || isNullable))
        {
            _keyColumns = new KeyColumnInfo[primaryKey.Properties.Count];

            for (var i = 0; i < primaryKey.Properties.Count; i++)
            {
                var keyProperty = primaryKey.Properties[i];
                var columnIndex = projectionMap[keyProperty];
                var typeMapping = (RelationalTypeMapping)keyProperty.GetTypeMapping();
                _keyColumns[i] = new KeyColumnInfo(columnIndex, typeMapping.CreateReader(columnIndex), keyProperty);
            }
        }

        // For nullable structural types (table-split optional dependents or optional complex type
        // projections): build existence-check column indices. For entity types, the PK may share columns
        // with the principal (always non-null), so we check non-PK properties exclusive to the dependent.
        // This mirrors GenerateMaterializationCondition in RelationalStructuralTypeShaperExpression.
        if (isNullable)
        {
            IEnumerable<IProperty> requiredCheckProperties;
            IEnumerable<IProperty>? optionalCheckProperties = null;

            if (entityType is not null)
            {
                // The table-optional existence check only applies to root entity types in table-splitting
                // scenarios. For entities with a discriminator property (TPH), derived types (TPT/TPC),
                // TPC entities, keyless entities, and JSON-mapped entities, the discriminator or other
                // mechanisms handle existence detection.
                // This mirrors GenerateMaterializationCondition in RelationalStructuralTypeShaperExpression.
                if (entityType.FindDiscriminatorProperty() is not null
                    || entityType.FindPrimaryKey() is null
                    || entityType.GetRootType() != entityType
                    || entityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy
                    || entityType.IsMappedToJson())
                {
                    requiredCheckProperties = [];
                }
                else
                {
                    var table = entityType.GetViewOrTableMappings()
                        .SingleOrDefault(e => e.IsSplitEntityTypePrincipal ?? true)?.Table
                        ?? entityType.GetDefaultMappings().Single().Table;

                    if (table.IsOptional(entityType))
                    {
                        requiredCheckProperties = entityType.GetProperties()
                            .Where(p => !p.IsNullable && !p.IsPrimaryKey());

                        var nonPrincipalSharedNonPk = entityType.GetNonPrincipalSharedNonPkProperties(table);
                        if (nonPrincipalSharedNonPk.Count != 0
                            && nonPrincipalSharedNonPk.All(p => p.IsNullable))
                        {
                            optionalCheckProperties = nonPrincipalSharedNonPk;
                        }
                    }
                    else
                    {
                        requiredCheckProperties = [];
                    }
                }
            }
            else
            {
                // Nullable complex type (e.g. optional complex property projection)
                requiredCheckProperties = structuralType.GetProperties().Where(p => !p.IsNullable);
                if (!requiredCheckProperties.Any())
                {
                    optionalCheckProperties = structuralType.GetProperties();
                }
            }

            var requiredIndices = requiredCheckProperties
                .Where(p => projectionMap.ContainsKey(p))
                .Select(p => projectionMap[p])
                .ToArray();
            if (requiredIndices.Length > 0)
            {
                _requiredNonPkColumnIndices = requiredIndices;
            }

            if (optionalCheckProperties is not null)
            {
                var optionalIndices = optionalCheckProperties
                    .Where(p => projectionMap.ContainsKey(p))
                    .Select(p => projectionMap[p])
                    .ToArray();
                if (optionalIndices.Length > 0)
                {
                    _optionalNonPkColumnIndices = optionalIndices;
                }
            }
        }

        // Build per-concrete-type materialization info (entity types only)
        var discriminatorProperty = entityType?.FindDiscriminatorProperty();
        if (discriminatorProperty is not null && projectionMap.TryGetValue(discriminatorProperty, out var discIdx))
        {
            // TPH: discriminator is a real property in the projection map
            _discriminatorColumnIndex = discIdx;
            var discriminatorTypeMapping = (RelationalTypeMapping)discriminatorProperty.GetTypeMapping();
            _discriminatorReader = discriminatorTypeMapping.CreateReader(_discriminatorColumnIndex);
        }
        else if (entityType is not null
            && entityType.GetConcreteDerivedTypesInclusive().Count() > 1
            && projectionMap.Count > 0)
        {
            // TPC/TPT: synthetic discriminator column projected after all properties
            _discriminatorColumnIndex = projectionMap.Values.Max() + 1;
        }
        else
        {
            _discriminatorColumnIndex = -1;
        }

        var concreteEntityTypes = entityType?.GetConcreteDerivedTypesInclusive().ToList();
        _concreteTypes = new ConcreteTypeInfo[concreteEntityTypes?.Count ?? 1];

        // Set up discriminator comparison strategy
        if (concreteEntityTypes is { Count: > 1 } && _discriminatorColumnIndex >= 0)
        {
            if (discriminatorProperty is not null)
            {
                var discriminatorComparer = discriminatorProperty.GetKeyValueComparer();
                _discriminatorMap = discriminatorComparer.IsDefault()
                    ? new Dictionary<object, int>(concreteEntityTypes.Count)
                    : new Dictionary<object, int>(concreteEntityTypes.Count, new ValueComparerEqualityComparer(discriminatorComparer));
            }
            else
            {
                // TPC/TPT: synthetic discriminator values are always strings
                _discriminatorMap = new Dictionary<object, int>(concreteEntityTypes.Count);
            }
        }

        for (var i = 0; i < _concreteTypes.Length; i++)
        {
            var concreteType = (ITypeBase?)concreteEntityTypes?[i] ?? structuralType;
            var discriminatorValue = (concreteType as IEntityType)?.GetDiscriminatorValue();

            if (discriminatorValue is not null)
            {
                _discriminatorMap?.Add(discriminatorValue, i);
            }

            // Determine the constructor to use and which properties it consumes.
            // Run binding interceptors (e.g. ProxyBindingInterceptor for lazy loading proxies)
            // to get the final binding, matching what the generated shaper does via ModifyBindings.
            var constructorBinding = concreteType.ConstructorBinding;
            if (constructorBinding is not null && bindingInterceptors is { Length: > 0 })
            {
                var interceptionData = new InstantiationBindingInterceptionData(concreteType);
                foreach (var interceptor in bindingInterceptors)
                {
                    constructorBinding = interceptor.ModifyBinding(interceptionData, constructorBinding);
                }
            }

            ConstructorInvoker? constructorInvoker = null;
            MethodInvoker? factoryMethodInvoker = null;
            object? factoryInstance = null;
            ConstructorParameterReader[]? constructorParameters = null;
            HashSet<IPropertyBase>? consumedProperties = null;

            switch (constructorBinding)
            {
                case ConstructorBinding { Constructor: var constructor, ParameterBindings: var paramBindings }:
                {
                    constructorInvoker = ConstructorInvoker.Create(constructor);

                    if (paramBindings.Count > 0)
                    {
                        constructorParameters = new ConstructorParameterReader[paramBindings.Count];
                        consumedProperties = [];
                        BuildParameterReaders(paramBindings, projectionMap, concreteType,
                            constructorParameters, consumedProperties);
                    }

                    break;
                }

                case FactoryMethodBinding factoryBinding:
                {
                    // Factory method binding (e.g. proxy factory for lazy loading).
                    // Store MethodInvoker + instance; args are resolved the same way as ConstructorBinding.
                    factoryMethodInvoker = MethodInvoker.Create(factoryBinding.FactoryMethod);
                    factoryInstance = factoryBinding.FactoryInstance;
                    var paramBindings = factoryBinding.ParameterBindings;

                    constructorParameters = new ConstructorParameterReader[paramBindings.Count];
                    consumedProperties = [];
                    BuildParameterReaders(paramBindings, projectionMap, concreteType,
                        constructorParameters, consumedProperties);

                    break;
                }

                case null or DefaultValueBinding:
                {
                    // Parameterless constructor (or no binding / value type default)
                    var ctor = concreteType.ClrType.GetConstructor(Type.EmptyTypes);
                    constructorInvoker = ctor is not null ? ConstructorInvoker.Create(ctor) : null;
                    break;
                }

                default:
                    throw new NotImplementedException(
                        $"The non-generated materializer does not yet support instantiation binding type "
                        + $"'{constructorBinding!.GetType().Name}' on '{concreteType.DisplayName()}'.");
            }

            // Build property materializers for this concrete type's properties (declared + inherited),
            // excluding properties consumed by the constructor
            var properties = concreteType.GetProperties().Where(p => !p.IsShadowProperty()).ToList();
            var materializers = new List<PropertyMaterializer>(properties.Count);
            var isComplexType = structuralType is IComplexType;

            foreach (var property in properties)
            {
                if (consumedProperties is not null && consumedProperties.Contains(property))
                {
                    continue;
                }

                if (!projectionMap.TryGetValue(property, out var columnIndex))
                {
                    continue;
                }

                var typeMapping = (RelationalTypeMapping)property.GetTypeMapping();
                // For complex types, use MemberInfo-based setters because IClrPropertySetter targets
                // the containing entity (TStructuralType), not the complex type instance (TStructural).
                var setter = isComplexType ? null : ((IRuntimePropertyBase)property).MaterializationSetter;
                var memberInfo = isComplexType ? property.GetMemberInfo(forMaterialization: true, forSet: true) : null;
                var reader = typeMapping.CreateReader(columnIndex);

                materializers.Add(new PropertyMaterializer(columnIndex, property.IsNullable, setter, memberInfo, reader, property));
            }

            // Build shadow property readers for tracking (FK shadow properties, discriminators, etc.)
            List<ShadowPropertyMaterializer>? shadowMaterializers = null;
            if (isTracking)
            {
                foreach (var property in concreteType.GetProperties().Where(p => p.IsShadowProperty()))
                {
                    if (!projectionMap.TryGetValue(property, out var columnIndex))
                    {
                        continue;
                    }

                    var typeMapping = (RelationalTypeMapping)property.GetTypeMapping();
                    var reader = typeMapping.CreateReader(columnIndex);

                    shadowMaterializers ??= [];
                    shadowMaterializers.Add(new ShadowPropertyMaterializer(
                        columnIndex, property.IsNullable, reader, property.GetShadowIndex()));
                }
            }

            // Build non-JSON table-split complex property materializers for this concrete type.
            List<TableSplitComplexPropertyInfo>? tableSplitComplexProps = null;
            foreach (var complexProperty in concreteType.GetComplexProperties())
            {
                var complexType2 = complexProperty.ComplexType;

                if (complexType2.IsMappedToJson())
                {
                    continue;
                }

                if (!complexType2.GetProperties().Any(p => projectionMap.ContainsKey(p)))
                {
                    continue;
                }

                var materializerType = typeof(RelationalStructuralTypeMaterializer<>).MakeGenericType(complexType2.ClrType);
                var complexMaterializer = (RelationalStructuralTypeMaterializer)Activator.CreateInstance(
                    materializerType, complexType2, projectionMap, isTracking, complexProperty.IsNullable,
                    bindingInterceptors)!;

                var setterMemberInfo = complexProperty.GetMemberInfo(forMaterialization: true, forSet: true);
                IClrPropertySetter? cpSetter = complexType2.ClrType.IsValueType
                    ? null
                    : ((IRuntimePropertyBase)complexProperty).MaterializationSetter;

                tableSplitComplexProps ??= [];
                tableSplitComplexProps.Add(new TableSplitComplexPropertyInfo(
                    complexMaterializer, cpSetter, setterMemberInfo, complexType2.ClrType.IsValueType));
            }

            _concreteTypes[i] = new ConcreteTypeInfo(
                concreteType,
                discriminatorValue,
                constructorInvoker,
                factoryMethodInvoker,
                factoryInstance,
                constructorParameters,
                materializers.ToArray(),
                shadowMaterializers?.ToArray() ?? [],
                tableSplitComplexProps?.ToArray() ?? []);
        }

        // Build JSON complex property info for JSON-mapped complex properties on this entity.
        // This mirrors ProcessTopLevelComplexJsonProperties in the generated shaper.
        List<JsonComplexPropertyInfo>? jsonComplexProps = null;

        foreach (var (property, projectionIndex) in projectionMap)
        {
            if (property is IComplexProperty { ComplexType: var complexType } complexProperty
                && complexType.IsMappedToJson())
            {
                // Look up the JSON column's type mapping — needed to correctly read the column
                // value (e.g. SQLite stores JSON as string, not MemoryStream).
                var jsonColumnName = complexType.GetContainerColumnName()!;
                var jsonColumn = complexType.ContainingEntityType.GetViewOrTableMappings()
                    .Select(m => m.Table.FindColumn(jsonColumnName))
                    .FirstOrDefault(c => c is not null)
                    ?? throw new UnreachableException(
                        $"Could not find JSON container column '{jsonColumnName}' for complex type '{complexType.DisplayName()}'.");

                var jsonColumnTypeMapping = (RelationalTypeMapping)jsonColumn.StoreTypeMapping;
                var jsonStreamReader = RelationalMaterializerFactory.BuildJsonColumnReader(
                    jsonColumnTypeMapping, projectionIndex);

                jsonComplexProps ??= [];
                jsonComplexProps.Add(new JsonComplexPropertyInfo(
                    projectionIndex,
                    jsonStreamReader,
                    RelationalMaterializerFactory.BuildJsonStructuralTypeMaterializer(
                        complexType, isTracking, isNullable || complexProperty.IsNullable),
                    ((IRuntimePropertyBase)complexProperty).MaterializationSetter,
                    complexProperty.IsCollection,
                    complexProperty,
                    complexProperty.DeclaringType.ClrType));
            }
        }

        _jsonComplexProperties = jsonComplexProps?.ToArray();


    }

    /// <inheritdoc />
    public override ITypeBase StructuralType => _structuralType;

    /// <inheritdoc />
    public override object? Materialize(
        QueryContext queryContext,
        DbDataReader dataReader,
        ResultContext resultContext,
        SingleQueryResultCoordinator resultCoordinator)
        // For value types, MaterializeTyped returns default(TStructuralType) when the entity doesn't
        // exist (nullable check), which boxes to a non-null zeroed struct. We must check existence
        // before boxing to correctly return null.
        => typeof(TStructuralType).IsValueType && !CheckNullableExists(dataReader)
            ? null
            : MaterializeTyped(queryContext, dataReader, resultContext, resultCoordinator);

    /// <inheritdoc />
    protected override Delegate GetMaterializeDelegateCore()
        => (Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, TStructuralType?>)MaterializeTyped;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TStructuralType? MaterializeTyped(
        QueryContext queryContext,
        DbDataReader dataReader,
        ResultContext resultContext,
        SingleQueryResultCoordinator resultCoordinator)
    {
        TStructuralType instance;

        if (HasCollectionIncludesInHierarchy)
        {
            Check.DebugAssert(_structuralType is IEntityType, "Collection includes are only supported for entity types.");

            // Collection include protocol: this method is called multiple times per parent entity.
            // On first call (resultContext.Values == null), we materialize the parent and initialize collections.
            // On subsequent calls, we populate collection elements.
            // When the parent changes or rows end, we set ResultReady = true and return the parent.
            //
            // HasCollectionIncludesInHierarchy is true when this materializer has direct collection includes OR
            // when any reference include materializer (transitively) has collection includes. In the latter
            // case, those reference materializers participate in the multi-call protocol themselves, and
            // we re-drive them on each subsequent row (see the else branch below).

            if (resultContext.Values is null)
            {
                // First call for this parent entity
                instance = MaterializeStructuralType(queryContext, dataReader, out var fromIdentityMap1)!;
                if (instance is null)
                {
                    return default;
                }

                // Store entity reference for subsequent calls (using Values as a marker + storage)
                resultContext.Values = [instance];

                // Process reference includes ONCE during initialization. Reference materializers that
                // have their own collection includes (HasCollectionIncludesInHierarchy) will initialize and
                // populate the first collection element themselves. Subsequent rows are driven by
                // the re-call loop in the else branch below.
                ProcessReferenceIncludes(queryContext, dataReader, resultCoordinator, instance);
                ProcessJsonIncludes(queryContext, dataReader, instance, fromIdentityMap1);

                // Initialize direct collection includes.
                for (var i = 0; i < CollectionIncludes.Count; i++)
                {
                    var ci = CollectionIncludes[i];
                    var parentEntity = ci.ParentEntityProvider is not null
                        ? ci.ParentEntityProvider()
                        : (object)instance;
                    InitializeIncludeCollection(queryContext, dataReader, resultCoordinator, _isTracking, parentEntity, ci);
                }
            }
            else
            {
                instance = (TStructuralType)resultContext.Values[0];

                // Re-drive reference includes that have collection includes in their hierarchy on each subsequent row.
                // Reference includes without collection includes in their hierarchy completed during initialization and
                // do not need to be called again.
                for (var i = 0; i < ReferenceIncludes.Count; i++)
                {
                    var refInclude = ReferenceIncludes[i];
                    if (refInclude.Materializer.HasCollectionIncludesInHierarchy && refInclude.ResultContext.Values is not null)
                    {
                        refInclude.Materializer.Materialize(
                            queryContext, dataReader, refInclude.ResultContext, resultCoordinator);
                    }
                }
            }

            // Populate direct collection elements from the current row.
            for (var i = 0; i < CollectionIncludes.Count; i++)
            {
                PopulateIncludeCollection(queryContext, dataReader, resultCoordinator, CollectionIncludes[i]);
            }

            // If ResultReady is true, return the entity; otherwise return default (MoveNext will call again)
            return resultCoordinator.ResultReady ? instance : default;
        }

        // Reference and/or JSON includes (no collection includes). These complete in a single call.
        if (ReferenceIncludes.Count > 0 || JsonIncludes.Count > 0)
        {
            Check.DebugAssert(_structuralType is IEntityType, "Reference/JSON includes are only supported for entity types.");

            if (resultContext.Values is null)
            {
                instance = MaterializeStructuralType(queryContext, dataReader, out var fromIdentityMap)!;
                if (instance is null)
                {
                    return default;
                }

                ProcessReferenceIncludes(queryContext, dataReader, resultCoordinator, instance);

                ProcessJsonIncludes(queryContext, dataReader, instance, fromIdentityMap);

                resultContext.Values = [instance];

                return instance;
            }

            return (TStructuralType?)resultContext.Values[0];
        }

        // No includes at all — cache the entity in resultContext.Values on first call and use the cache on
        // subsequent calls. This is necessary when this materializer is used as a reference include inside an
        // outer materializer that has collection includes: the outer materializer calls us on every row to drive
        // its collection population, and we must not re-read from the reader after the first call.
        if (resultContext.Values is null)
        {
            instance = MaterializeStructuralType(queryContext, dataReader, out _)!;
            if (instance is not null)
            {
                resultContext.Values = [instance];
            }

            return instance;
        }

        return (TStructuralType?)resultContext.Values[0];
    }

    /// <summary>
    ///     Checks whether a nullable structural type (table-split optional dependent or optional complex
    ///     type) exists in the current row by examining non-PK columns. Returns true if the type exists
    ///     or if no nullable check is needed. Returns false if all required columns are NULL.
    /// </summary>
    private bool CheckNullableExists(DbDataReader dataReader)
    {
        if (_requiredNonPkColumnIndices is not null)
        {
            for (var i = 0; i < _requiredNonPkColumnIndices.Length; i++)
            {
                if (dataReader.IsDBNull(_requiredNonPkColumnIndices[i]))
                {
                    return false;
                }
            }
        }

        if (_optionalNonPkColumnIndices is not null)
        {
            var anyNonNull = false;
            for (var i = 0; i < _optionalNonPkColumnIndices.Length; i++)
            {
                if (!dataReader.IsDBNull(_optionalNonPkColumnIndices[i]))
                {
                    anyNonNull = true;
                    break;
                }
            }

            if (!anyNonNull)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Materializes the entity itself (identity resolution, instantiation, property population, tracking).
    /// </summary>
    private TStructuralType? MaterializeStructuralType(
        QueryContext queryContext,
        DbDataReader dataReader,
        out bool fromIdentityMap)
    {
        fromIdentityMap = false;

        // Check for null keys — needed for tracking (identity resolution) and nullable entities (LEFT JOIN)
        if (_keyColumns is not null)
        {
            var hasNullKey = false;
            var keyValues = _isTracking ? new object[_keyColumns.Length] : null;

            for (var i = 0; i < _keyColumns.Length; i++)
            {
                ref readonly var keyCol = ref _keyColumns[i];

                if (dataReader.IsDBNull(keyCol.ColumnIndex))
                {
                    hasNullKey = true;
                    break;
                }

                try
                {
                    keyValues?[i] = keyCol.Reader.Read<object>(dataReader);
                }
                catch (Exception e)
                {
                    ThrowReadValueException(e, dataReader,
                        new PropertyMaterializer(keyCol.ColumnIndex, false, null, null, keyCol.Reader, keyCol.Property));
                }
            }

            if (hasNullKey)
            {
                return default;
            }

            if (_isTracking)
            {
                var entry = queryContext.TryGetEntry(_primaryKey!, keyValues!, throwOnNullKey: true, out _);
                if (entry is not null)
                {
                    fromIdentityMap = true;
                    JoinEntityMaterializer?.Materialize(queryContext, dataReader, new ResultContext(), null!);
                    return (TStructuralType)entry.Entity;
                }
            }
        }

        // For table-split optional dependents: check non-PK columns to determine if the dependent exists.
        // The PK may share columns with the principal (always non-null), so we check columns exclusive
        // to the dependent. This mirrors the MaterializationCondition in the generated shaper.
        if (!CheckNullableExists(dataReader))
        {
            return default;
        }

        var (instance, typeInfo) = InstantiateStructuralType(queryContext, dataReader);

        // InstantiateStructuralType returns default when the discriminator column is NULL
        // (e.g. TPT table-split dependent from a LEFT JOIN that doesn't exist).
        if (instance is null)
        {
            return default;
        }

        // For value types (complex types), box once and work with the boxed reference so
        // MemberInfo.SetValue mutations are visible. Unbox after all properties are set.
        object boxedInstance = instance!;
        PopulateProperties(dataReader, boxedInstance, typeInfo.PropertyMaterializers);
        PopulateTableSplitComplexProperties(dataReader, boxedInstance, typeInfo.TableSplitComplexProperties);
        PopulateJsonComplexProperties(queryContext, dataReader, boxedInstance);

        if (typeof(TStructuralType).IsValueType)
        {
            instance = (TStructuralType)boxedInstance;
        }

        if (_isTracking)
        {
            var shadowSnapshot = typeInfo.ShadowPropertyMaterializers.Length > 0
                ? BuildShadowSnapshot(dataReader, typeInfo)
                : Snapshot.Empty;

            queryContext.StartTracking((IEntityType)typeInfo.StructuralType, instance!, shadowSnapshot);

            // For many-to-many skip navigations: materialize the join entity so it gets tracked.
            // FetchJoinEntity is only inserted for tracking queries, so JoinEntityMaterializer
            // is only non-null here.
            JoinEntityMaterializer?.Materialize(queryContext, dataReader, new ResultContext(), null!);
        }

        return instance;
    }

    /// <summary>
    ///     Processes reference includes for the given entity.
    /// </summary>
    private void ProcessReferenceIncludes(
        QueryContext queryContext,
        DbDataReader dataReader,
        SingleQueryResultCoordinator resultCoordinator,
        TStructuralType entity)
    {
        if (ReferenceIncludes.Count == 0)
        {
            return;
        }

        // When the outer entity changes, each reference include must start with a fresh per-include context.
        for (var i = 0; i < ReferenceIncludes.Count; i++)
        {
            ReferenceIncludes[i].ResultContext.Values = null;
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

            // If the reference materializer has collection includes in its hierarchy, it participates in the
            // multi-call protocol and may return null when ResultReady is false. In that case,
            // the entity was already stored in its ResultContext on the first call — use it.
            relatedEntity ??= include.ResultContext.Values?[0];

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
    ///     Processes JSON includes for the given entity.
    ///     Each JSON include reads a JSON column from the DbDataReader, parses it, and sets
    ///     the navigation property on the entity with the materialized result.
    /// </summary>
    /// <remarks>
    ///     This mirrors the generated shaper's <c>IncludeJsonEntityReference</c> and
    ///     <c>IncludeJsonEntityCollection</c> runtime methods.
    /// </remarks>
    private void ProcessJsonIncludes(
        QueryContext queryContext,
        DbDataReader dataReader,
        TStructuralType entity,
        bool entityAlreadyTracked = false)
    {
        if (JsonIncludes.Count == 0)
        {
            return;
        }

        for (var i = 0; i < JsonIncludes.Count; i++)
        {
            var include = JsonIncludes[i];
            var jsonMaterializer = (RelationalJsonStructuralTypeMaterializer)include.Materializer;

            // TPH guard
            if (!include.Navigation.DeclaringEntityType.ClrType.IsInstanceOfType(entity))
            {
                continue;
            }

            var jsonReaderData = RelationalMaterializerFactory.ReadJsonColumn(
                dataReader, include.ProjectionInfo.JsonColumnIndex, include.JsonStreamReader, queryContext);
            if (jsonReaderData is null)
            {
                continue;
            }

            var keyValues = RelationalMaterializerFactory.ExtractJsonKeyValues(
                dataReader, include.ProjectionInfo, include.Navigation.TargetEntityType,
                include.IsCollection);

            if (include.IsCollection)
            {
                // Collection include: create the collection, populate elements, fixup
                var collectionAccessor = include.Navigation.GetCollectionAccessor()!;
                var collection = collectionAccessor.GetOrCreate(entity!, forMaterialization: true);

                object[]? childKeyValues = null;
                if (keyValues is not null)
                {
                    childKeyValues = new object[keyValues.Length + 1];
                    Array.Copy(keyValues, childKeyValues, keyValues.Length);
                }

                var manager = new Utf8JsonReaderManager(jsonReaderData, queryContext.QueryLogger);
                var tokenType = manager.CurrentReader.TokenType;

                if (tokenType is JsonTokenType.Null or not JsonTokenType.StartArray)
                {
                    continue;
                }

                var index = 0;

                for (tokenType = manager.MoveNext(); tokenType != JsonTokenType.EndArray; tokenType = manager.MoveNext())
                {
                    childKeyValues?[^1] = ++index;

                    if (tokenType != JsonTokenType.StartObject)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.JsonReaderInvalidTokenType(tokenType.ToString()));
                    }

                    manager.CaptureState();
                    var element = jsonMaterializer.Materialize(
                        queryContext, jsonReaderData, childKeyValues, out var collSnapshot);

                    if (element is not null && !entityAlreadyTracked)
                    {
                        collectionAccessor.AddStandalone(collection, element);

                        // Complete deferred tracking for shadow-PK entities after navigation fixup.
                        if (collSnapshot is not null)
                        {
                            queryContext.StartTracking(
                                include.Navigation.TargetEntityType, element, collSnapshot);
                        }
                    }

                    manager = new Utf8JsonReaderManager(jsonReaderData, queryContext.QueryLogger);
                    if (manager.CurrentReader.TokenType != JsonTokenType.EndObject)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.JsonReaderInvalidTokenType(manager.CurrentReader.TokenType.ToString()));
                    }
                }

                manager.CaptureState();
            }
            else
            {
                // Reference include: materialize single entity, fixup
                var related = jsonMaterializer.Materialize(
                    queryContext, jsonReaderData, keyValues, out var refSnapshot);

                // For tracking queries where the entity is already tracked, skip fixup —
                // the navigation was already set on first materialization.
                if (related is not null && !entityAlreadyTracked)
                {
                    var navSetter = ((IRuntimePropertyBase)include.Navigation).GetSetter();
                    navSetter.SetClrValue(entity!, related);

                    if (!_isTracking)
                    {
                        ((INavigation)include.Navigation).SetIsLoadedWhenNoTracking(entity!);
                    }

                    if (include.InverseNavigation is { IsCollection: false })
                    {
                        include.InverseNavigationSetter?.SetClrValue(related, entity!);
                        if (!_isTracking)
                        {
                            include.InverseNavigation.SetIsLoadedWhenNoTracking(related);
                        }
                    }

                    // Complete deferred tracking for shadow-PK entities after navigation fixup.
                    if (refSnapshot is not null)
                    {
                        queryContext.StartTracking(
                            include.Navigation.TargetEntityType, related, refSnapshot);
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

        if (resultCoordinator.RowState.IsCurrentResultReaderExhausted)
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
                resultCoordinator.RowState.MarkRowForNextResult();
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

                resultCoordinator.RowState.MarkResultPending();
                return;
            }

            // New element — materialize the previous one first
            GenerateCurrentElementIfPending();
            resultCoordinator.RowState.MarkCurrentRowConsumed();
            collectionContext.UpdateSelfIdentifier(innerKey);
        }
        else
        {
            // First element in collection
            collectionContext.UpdateSelfIdentifier(innerKey);
        }

        ProcessCurrentElementRow();
        resultCoordinator.RowState.MarkResultPending();

        void ProcessCurrentElementRow()
        {
            var previousResultReady = resultCoordinator.ResultReady;
            resultCoordinator.RowState.MarkResultReady();

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

            if (!previousResultReady)
            {
                resultCoordinator.RowState.MarkResultPending();
            }
        }

        void GenerateCurrentElementIfPending()
        {
            if (collectionContext.ResultContext.Values is not null)
            {
                resultCoordinator.RowState.MarkNoMoreRowsForCurrentResult();
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

    /// <summary>
    ///     Resolves parameter bindings into <see cref="ConstructorParameterReader" /> entries.
    ///     Handles <see cref="PropertyParameterBinding" />, <see cref="ContextParameterBinding" />,
    ///     <see cref="EntityTypeParameterBinding" />, <see cref="DependencyInjectionParameterBinding" />,
    ///     and <see cref="ObjectArrayParameterBinding" />.
    /// </summary>
    private static void BuildParameterReaders(
        IReadOnlyList<ParameterBinding> paramBindings,
        IReadOnlyDictionary<IPropertyBase, int> projectionMap,
        ITypeBase concreteType,
        ConstructorParameterReader[] constructorParameters,
        HashSet<IPropertyBase> consumedProperties)
    {
        for (var p = 0; p < paramBindings.Count; p++)
        {
            constructorParameters[p] = ResolveBinding(paramBindings[p]);
        }

        ConstructorParameterReader ResolveBinding(ParameterBinding binding)
        {
            switch (binding)
            {
                case PropertyParameterBinding { ConsumedProperties: [IProperty property] }:
                    if (!projectionMap.TryGetValue(property, out var columnIndex))
                    {
                        throw new InvalidOperationException(
                            $"Constructor parameter for property '{property.Name}' on "
                            + $"'{concreteType.DisplayName()}' is not in the projection map.");
                    }

                    var typeMapping = (RelationalTypeMapping)property.GetTypeMapping();
                    consumedProperties.Add(property);
                    return new ConstructorParameterReader(
                        columnIndex, property.IsNullable, typeMapping.CreateReader(columnIndex));

                case ContextParameterBinding:
                    return new ConstructorParameterReader(static qc => qc.Context);

                case EntityTypeParameterBinding:
                    var capturedType = concreteType;
                    return new ConstructorParameterReader(_ => capturedType);

                case DependencyInjectionParameterBinding diBinding:
                    var serviceType = diBinding.ServiceType;
                    return new ConstructorParameterReader(
                        qc => ((IInfrastructure<IServiceProvider>)qc.Context).Instance.GetService(serviceType));

                case ObjectArrayParameterBinding arrayBinding:
                    // Recursively resolve inner bindings; packed into object[] at materialization time
                    var innerReaders = new ConstructorParameterReader[arrayBinding.Bindings.Count];
                    for (var j = 0; j < arrayBinding.Bindings.Count; j++)
                    {
                        innerReaders[j] = ResolveBinding(arrayBinding.Bindings[j]);
                    }

                    return new ConstructorParameterReader(innerReaders);

                default:
                    throw new NotImplementedException(
                        $"The non-generated materializer does not yet support constructor parameter "
                        + $"binding type '{binding.GetType().Name}' on '{concreteType.DisplayName()}'.");
            }
        }
    }

    private (TStructuralType Instance, ConcreteTypeInfo TypeInfo) InstantiateStructuralType(
        QueryContext queryContext, DbDataReader dataReader)
    {
        return _concreteTypes is [var singleType]
            ? (Instantiate(singleType, queryContext, dataReader), singleType)
            : InstantiateInheritanceEntity(dataReader);

        static TStructuralType Instantiate(in ConcreteTypeInfo typeInfo, QueryContext queryContext, DbDataReader dataReader)
        {
            if (typeInfo.ConstructorParameters is { } ctorParams)
            {
                var args = new object?[ctorParams.Length];
                for (var i = 0; i < ctorParams.Length; i++)
                {
                    args[i] = ResolveParameterValue(ref ctorParams[i], queryContext, dataReader);
                }

                var instance = typeInfo.FactoryMethodInvoker is null
                    ? (TStructuralType)typeInfo.ConstructorInvoker!.Invoke(args.AsSpan())
                    : (TStructuralType)typeInfo.FactoryMethodInvoker.Invoke(typeInfo.FactoryInstance, args.AsSpan())!;

                // After entity creation, attach injectable services (e.g. ILazyLoader).
                // This mirrors AddAttachServiceExpressions in the generated shaper.
                for (var i = 0; i < args.Length; i++)
                {
                    if (args[i] is IInjectableService injectableService)
                    {
                        injectableService.Injected(
                            queryContext.Context, instance!, null, typeInfo.StructuralType);
                    }
                }

                return instance;
            }

            return (TStructuralType)(typeInfo.ConstructorInvoker?.Invoke([])
                ?? Activator.CreateInstance(typeInfo.StructuralType.ClrType))!;
        }

        static object? ResolveParameterValue(
            ref ConstructorParameterReader param, QueryContext queryContext, DbDataReader dataReader)
        {
            if (param.InnerReaders is { } innerReaders)
            {
                // ObjectArrayParameterBinding: pack inner values into object[]
                var innerArgs = new object?[innerReaders.Length];
                for (var j = 0; j < innerReaders.Length; j++)
                {
                    innerArgs[j] = ResolveParameterValue(ref innerReaders[j], queryContext, dataReader);
                }

                return innerArgs;
            }

            if (param.ServiceResolver is not null)
            {
                return param.ServiceResolver(queryContext);
            }

            return param.IsNullable && dataReader.IsDBNull(param.ColumnIndex)
                ? null
                : param.Reader!.Read<object>(dataReader);
        }

        (TStructuralType Instance, ConcreteTypeInfo TypeInfo) InstantiateInheritanceEntity(DbDataReader dataReader)
        {
            Check.DebugAssert(
                _discriminatorColumnIndex >= 0,
                "Multiple concrete types but no discriminator column.");

            if (!dataReader.IsDBNull(_discriminatorColumnIndex))
            {
                var discriminatorValue = _discriminatorReader is not null
                    ? _discriminatorReader.Read<object>(dataReader)
                    : (object)dataReader.GetString(_discriminatorColumnIndex);

                if (_discriminatorMap!.TryGetValue(discriminatorValue, out var index))
                {
                    ref readonly var typeInfo = ref _concreteTypes[index];
                    return (Instantiate(typeInfo, queryContext, dataReader), typeInfo);
                }

                throw new InvalidOperationException(
                    $"Unable to materialize entity of type '{typeof(TStructuralType).Name}'. "
                    + $"No concrete type found for discriminator value '{discriminatorValue}'.");
            }

            // NULL discriminator means the base type. This only happens with TPT, where the synthetic
            // CASE expression checks which derived-type LEFT JOIN matched; when none matched the CASE
            // has no ELSE clause and produces NULL, indicating the base type. (TPC always has a non-null
            // constant discriminator per UNION ALL branch, so this path is not reached for TPC.)
            // Use the first concrete type entry, which is the base type.
            ref readonly var baseTypeInfo = ref _concreteTypes[0];
            return (Instantiate(baseTypeInfo, queryContext, dataReader), baseTypeInfo);
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
    private void PopulateJsonComplexProperties(
        QueryContext queryContext,
        DbDataReader dataReader,
        object entity)
    {
        if (_jsonComplexProperties is null)
        {
            return;
        }

        for (var i = 0; i < _jsonComplexProperties.Length; i++)
        {
            ref readonly var jsonProp = ref _jsonComplexProperties[i];

            // TPH guard: the complex property may be declared on a derived type
            if (!jsonProp.DeclaringClrType.IsInstanceOfType(entity))
            {
                continue;
            }

            var jsonReaderData = RelationalMaterializerFactory.ReadJsonColumn(
                dataReader, jsonProp.JsonColumnIndex, jsonProp.JsonStreamReader, queryContext);

            if (jsonReaderData is not null)
            {
                var value = jsonProp.IsCollection
                    ? RelationalJsonStructuralTypeMaterializer.MaterializeCollection(
                        queryContext, jsonReaderData, keyValues: null, jsonProp.Materializer, jsonProp.ComplexProperty)
                    : jsonProp.Materializer.Materialize(queryContext, jsonReaderData, keyValues: null, out _);

                if (value is not null)
                {
                    jsonProp.Setter.SetClrValue(entity, value);
                }
            }
        }
    }

    /// <summary>
    ///     Materializes and sets non-JSON table-split complex properties on the entity.
    ///     Delegates to a nested <see cref="RelationalStructuralTypeMaterializer" /> for each complex type.
    /// </summary>
    private static void PopulateTableSplitComplexProperties(
        DbDataReader dataReader, object entity, TableSplitComplexPropertyInfo[] complexProperties)
    {
        for (var i = 0; i < complexProperties.Length; i++)
        {
            ref readonly var cp = ref complexProperties[i];

            var result = cp.Materializer.Materialize(queryContext: null!, dataReader, new ResultContext(), null!);
            if (result is null)
            {
                continue;
            }

            if (cp.IsValueType || cp.Setter is null)
            {
                if (cp.SetterMemberInfo is FieldInfo fieldInfo)
                {
                    fieldInfo.SetValue(entity, result);
                }
                else
                {
                    ((PropertyInfo)cp.SetterMemberInfo).SetValue(entity, result);
                }
            }
            else
            {
                cp.Setter.SetClrValue(entity, result);
            }
        }
    }

    private static void PopulateProperties(DbDataReader dataReader, object entity, PropertyMaterializer[] materializers)
    {
        for (var i = 0; i < materializers.Length; i++)
        {
            ref readonly var pm = ref materializers[i];

            if (pm.IsNullable && dataReader.IsDBNull(pm.ColumnIndex))
            {
                continue;
            }

            try
            {
                if (pm.Setter is not null)
                {
                    pm.Setter.SetClrValue(entity, pm.Reader, dataReader);
                }
                else
                {
                    // Complex type path: use MemberInfo-based setting (handles boxed value types correctly)
                    // TODO: Consider what to do here
                    var value = pm.Reader.Read<object>(dataReader);
                    if (pm.MemberInfo is FieldInfo fieldInfo)
                    {
                        fieldInfo.SetValue(entity, value);
                    }
                    else
                    {
                        ((PropertyInfo)pm.MemberInfo!).SetValue(entity, value);
                    }
                }
            }
            catch (Exception e)
            {
                ThrowReadValueException(e, dataReader, pm);
            }
        }
    }

    private static void ThrowReadValueException(Exception exception, DbDataReader dataReader, in PropertyMaterializer pm)
    {
        var value = dataReader.GetFieldValue<object>(pm.ColumnIndex);

        var expectedType = pm.Property?.ClrType ?? typeof(object);
        var actualType = value?.GetType();

        string message;
        if (pm.Property is not null)
        {
            var entityType = pm.Property.DeclaringType.DisplayName();
            var propertyName = pm.Property.Name;

            message = exception is NullReferenceException || Equals(value, DBNull.Value)
                ? RelationalStrings.ErrorMaterializingPropertyNullReference(entityType, propertyName, expectedType)
                : exception is InvalidCastException
                    ? CoreStrings.ErrorMaterializingPropertyInvalidCast(entityType, propertyName, expectedType, actualType)
                    : RelationalStrings.ErrorMaterializingProperty(entityType, propertyName);
        }
        else
        {
            message = exception is NullReferenceException || Equals(value, DBNull.Value)
                ? RelationalStrings.ErrorMaterializingValueNullReference(expectedType)
                : exception is InvalidCastException
                    ? RelationalStrings.ErrorMaterializingValueInvalidCast(expectedType, actualType)
                    : RelationalStrings.ErrorMaterializingValue;
        }

        throw new InvalidOperationException(message, exception);
    }

    /// <summary>
    ///     Builds an <see cref="ISnapshot" /> containing shadow property values read from the
    ///     <see cref="DbDataReader" />. Shadow properties include FK columns and discriminators
    ///     that have no CLR property on the entity. These values are needed by the change tracker
    ///     for relationship fixup (e.g. populating collection navigations via FK matching).
    /// </summary>
    private ISnapshot BuildShadowSnapshot(DbDataReader dataReader, in ConcreteTypeInfo typeInfo)
    {
        var shadowMaterializers = typeInfo.ShadowPropertyMaterializers;
        var snapshot = ((IRuntimeTypeBase)typeInfo.StructuralType).EmptyShadowValuesFactory();
        for (var i = 0; i < shadowMaterializers.Length; i++)
        {
            ref readonly var sm = ref shadowMaterializers[i];

            if (sm.IsNullable && dataReader.IsDBNull(sm.ColumnIndex))
            {
                continue;
            }

            snapshot[sm.ShadowIndex] = sm.Reader.Read<object>(dataReader);
        }

        return snapshot;
    }

    private readonly struct ConcreteTypeInfo(
        ITypeBase structuralType,
        object? discriminatorValue,
        ConstructorInvoker? constructorInvoker,
        MethodInvoker? factoryMethodInvoker,
        object? factoryInstance,
        ConstructorParameterReader[]? constructorParameters,
        PropertyMaterializer[] propertyMaterializers,
        ShadowPropertyMaterializer[] shadowPropertyMaterializers,
        TableSplitComplexPropertyInfo[] tableSplitComplexProperties)
    {
        public ITypeBase StructuralType { get; } = structuralType;
        public object? DiscriminatorValue { get; } = discriminatorValue;
        public ConstructorInvoker? ConstructorInvoker { get; } = constructorInvoker;
        public MethodInvoker? FactoryMethodInvoker { get; } = factoryMethodInvoker;
        public object? FactoryInstance { get; } = factoryInstance;
        public ConstructorParameterReader[]? ConstructorParameters { get; } = constructorParameters;
        public PropertyMaterializer[] PropertyMaterializers { get; } = propertyMaterializers;
        public ShadowPropertyMaterializer[] ShadowPropertyMaterializers { get; } = shadowPropertyMaterializers;
        public TableSplitComplexPropertyInfo[] TableSplitComplexProperties { get; } = tableSplitComplexProperties;
    }

    private readonly struct PropertyMaterializer(
        int columnIndex,
        bool isNullable,
        IClrPropertySetter? setter,
        MemberInfo? memberInfo,
        ITypedValueReader<DbDataReader> reader,
        IPropertyBase? property)
    {
        public int ColumnIndex { get; } = columnIndex;
        public bool IsNullable { get; } = isNullable;
        public IClrPropertySetter? Setter { get; } = setter;
        public MemberInfo? MemberInfo { get; } = memberInfo;
        public IPropertyBase? Property { get; } = property;

        /// <summary>
        ///     The reader for this property, produced by <see cref="RelationalTypeMapping.CreateReader" />.
        ///     Immutable; the <see cref="DbDataReader" /> is passed as state per-call.
        /// </summary>
        public ITypedValueReader<DbDataReader> Reader { get; } = reader;
    }

    private readonly struct ShadowPropertyMaterializer(
        int columnIndex,
        bool isNullable,
        ITypedValueReader<DbDataReader> reader,
        int shadowIndex)
    {
        public int ColumnIndex { get; } = columnIndex;
        public bool IsNullable { get; } = isNullable;
        public ITypedValueReader<DbDataReader> Reader { get; } = reader;
        public int ShadowIndex { get; } = shadowIndex;
    }

    private readonly struct ConstructorParameterReader
    {
        public int ColumnIndex { get; }
        public bool IsNullable { get; }
        public ITypedValueReader<DbDataReader>? Reader { get; }

        /// <summary>
        ///     For service/context/entity-type bindings: a delegate that resolves the value at materialization time.
        ///     When non-null, <see cref="Reader"/> is unused.
        /// </summary>
        public Func<QueryContext, object?>? ServiceResolver { get; }

        /// <summary>
        ///     For <see cref="ObjectArrayParameterBinding"/>: inner readers whose resolved values
        ///     are packed into an <c>object[]</c> at materialization time.
        /// </summary>
        public ConstructorParameterReader[]? InnerReaders { get; }

        /// <summary>Creates a reader for a property-bound constructor parameter.</summary>
        public ConstructorParameterReader(int columnIndex, bool isNullable, ITypedValueReader<DbDataReader> reader)
        {
            ColumnIndex = columnIndex;
            IsNullable = isNullable;
            Reader = reader;
        }

        /// <summary>Creates a reader for a service/context/entity-type constructor parameter.</summary>
        public ConstructorParameterReader(Func<QueryContext, object?> serviceResolver)
            => ServiceResolver = serviceResolver;

        /// <summary>Creates a reader for an object array parameter (packs inner bindings into object[]).</summary>
        public ConstructorParameterReader(ConstructorParameterReader[] innerReaders)
            => InnerReaders = innerReaders;
    }

    private readonly struct KeyColumnInfo(int columnIndex, ITypedValueReader<DbDataReader> reader, IProperty property)
    {
        public int ColumnIndex { get; } = columnIndex;
        public ITypedValueReader<DbDataReader> Reader { get; } = reader;
        public IProperty Property { get; } = property;
    }

    private readonly struct JsonComplexPropertyInfo(
        int jsonColumnIndex,
        Func<DbDataReader, MemoryStream?> jsonStreamReader,
        RelationalJsonStructuralTypeMaterializer materializer,
        IClrPropertySetter setter,
        bool isCollection,
        IPropertyBase complexProperty,
        Type declaringClrType)
    {
        public int JsonColumnIndex { get; } = jsonColumnIndex;
        public Func<DbDataReader, MemoryStream?> JsonStreamReader { get; } = jsonStreamReader;
        public RelationalJsonStructuralTypeMaterializer Materializer { get; } = materializer;
        public IClrPropertySetter Setter { get; } = setter;
        public bool IsCollection { get; } = isCollection;
        public Type DeclaringClrType { get; } = declaringClrType;
        public IPropertyBase ComplexProperty { get; } = complexProperty;
    }

    private readonly struct TableSplitComplexPropertyInfo(
        RelationalStructuralTypeMaterializer materializer,
        IClrPropertySetter? setter,
        MemberInfo setterMemberInfo,
        bool isValueType)
    {
        public RelationalStructuralTypeMaterializer Materializer { get; } = materializer;
        public IClrPropertySetter? Setter { get; } = setter;
        public MemberInfo SetterMemberInfo { get; } = setterMemberInfo;
        public bool IsValueType { get; } = isValueType;
    }

    private sealed class ValueComparerEqualityComparer(ValueComparer comparer) : IEqualityComparer<object>
    {
        public new bool Equals(object? x, object? y) => comparer.Equals(x, y);
        public int GetHashCode(object obj) => comparer.GetHashCode(obj);
    }
}
