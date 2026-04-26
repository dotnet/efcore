// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

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
public class RelationalEntityMaterializer<TEntity>
    where TEntity : class, new()
{
    private readonly IKey? _primaryKey;
    private readonly KeyColumnInfo[]? _keyColumns;
    private readonly bool _isTracking;

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
        bool isTracking)
    {
        // Note that isTracking may be true but with a non-trackable projected type (e.g. keyless entity type).
        _primaryKey = isTracking ? entityType.FindPrimaryKey() : null;
        _isTracking = _primaryKey is not null;

        if (_primaryKey is not null)
        {
            _keyColumns = new KeyColumnInfo[_primaryKey.Properties.Count];

            for (var i = 0; i < _primaryKey.Properties.Count; i++)
            {
                var keyProperty = _primaryKey.Properties[i];
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

                materializers.Add(new PropertyMaterializer(columnIndex, property.IsNullable, property.ClrType, typeMapping, setter));
            }

            _concreteTypes[i] = new ConcreteTypeInfo(
                concreteType,
                discriminatorValue,
                materializers.ToArray());
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TEntity? Materialize(
        QueryContext queryContext,
        DbDataReader dataReader,
        ResultContext resultContext,
        SingleQueryResultCoordinator resultCoordinator)
    {
        if (_isTracking)
        {
            // Identity resolution: check if we already have this entity tracked
            var keyValues = new object[_keyColumns!.Length];
            var hasNullKey = false;

            for (var i = 0; i < _keyColumns.Length; i++)
            {
                ref readonly var keyCol = ref _keyColumns[i];

                if (dataReader.IsDBNull(keyCol.ColumnIndex))
                {
                    hasNullKey = true;
                    break;
                }

                keyValues[i] = dataReader.GetFieldValue<object>(keyCol.ColumnIndex);

                if (keyValues[i].GetType() != keyCol.ClrType)
                {
                    keyValues[i] = Convert.ChangeType(keyValues[i], keyCol.ClrType);
                }
            }

            if (hasNullKey)
            {
                return null;
            }

            var entry = queryContext.TryGetEntry(_primaryKey!, keyValues, throwOnNullKey: true, out _);
            if (entry is not null)
            {
                return (TEntity)entry.Entity;
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
    ///     For the non-inheritance case (single concrete type = TEntity), uses the non-boxing
    ///     <see cref="RelationalTypeMapping.ReadAndSet{TEntity}" /> which downcasts the setter to
    ///     <c>ClrPropertySetter&lt;TEntity, TEntity, TValue&gt;</c>.
    ///     For TPH inheritance, falls back to the boxing <see cref="RelationalTypeMapping.ReadAndSetBoxed" />
    ///     because the setter's entity type parameter is the derived type and cannot be downcast to TEntity.
    ///     To eliminate boxing for TPH, <see cref="IClrPropertySetter" /> would need a typed method that
    ///     doesn't require knowing the entity type at compile time.
    /// </summary>
    private void PopulateProperties(DbDataReader dataReader, TEntity entity, PropertyMaterializer[] materializers)
    {
        if (_concreteTypes.Length == 1)
        {
            for (var i = 0; i < materializers.Length; i++)
            {
                ref readonly var pm = ref materializers[i];

                if (pm.IsNullable && dataReader.IsDBNull(pm.ColumnIndex))
                {
                    continue;
                }

                pm.TypeMapping.ReadAndSet(dataReader, pm.ColumnIndex, entity, pm.Setter, pm.PropertyClrType);
            }
        }
        else
        {
            for (var i = 0; i < materializers.Length; i++)
            {
                ref readonly var pm = ref materializers[i];

                if (pm.IsNullable && dataReader.IsDBNull(pm.ColumnIndex))
                {
                    continue;
                }

                pm.TypeMapping.ReadAndSetBoxed(dataReader, pm.ColumnIndex, entity, pm.Setter);
            }
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
        Type propertyClrType,
        RelationalTypeMapping typeMapping,
        IClrPropertySetter setter)
    {
        public int ColumnIndex { get; } = columnIndex;
        public bool IsNullable { get; } = isNullable;
        public Type PropertyClrType { get; } = propertyClrType;
        public RelationalTypeMapping TypeMapping { get; } = typeMapping;
        public IClrPropertySetter Setter { get; } = setter;
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
