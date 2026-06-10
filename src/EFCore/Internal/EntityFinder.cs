// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class EntityFinder<TEntity> : IEntityFinder<TEntity>
    where TEntity : class
{
    private readonly IStateManager _stateManager;
    private readonly IDbSetSource _setSource;
    private readonly IDbSetCache _setCache;
    private readonly IEntityType _entityType;
    private readonly IKey _primaryKey;
    private readonly Type _primaryKeyType;
    private readonly int _primaryKeyPropertiesCount;
    private readonly IQueryable<TEntity> _queryRoot;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public EntityFinder(
        IStateManager stateManager,
        IDbSetSource setSource,
        IDbSetCache setCache,
        IEntityType entityType)
    {
        _stateManager = stateManager;
        _setSource = setSource;
        _setCache = setCache;
        _entityType = entityType;
        _primaryKey = entityType.FindPrimaryKey()!;
        _primaryKeyPropertiesCount = _primaryKey.Properties.Count;
        _primaryKeyType = _primaryKeyPropertiesCount == 1 ? _primaryKey.Properties[0].ClrType : typeof(IReadOnlyList<object?>);
        _queryRoot = (IQueryable<TEntity>)BuildQueryRoot(entityType);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TEntity? Find(object?[]? keyValues)
    {
        if (keyValues == null
            || keyValues.Any(v => v == null))
        {
            return default;
        }

        var (processedKeyValues, _) = ValidateKeyPropertiesAndExtractCancellationToken(keyValues!, async: false, default);

        return FindTracked(processedKeyValues)
            ?? _queryRoot.FirstOrDefault(BuildLambda(_primaryKey.Properties, new ValueBuffer(processedKeyValues)));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    object? IEntityFinder.Find(object?[]? keyValues)
        => Find(keyValues);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityEntry? FindEntry<TKey>(TKey keyValue)
    {
        if (_primaryKeyPropertiesCount != 1)
        {
            throw new ArgumentException(
                CoreStrings.FindValueCountMismatch(typeof(TEntity).ShortDisplayName(), _primaryKeyPropertiesCount, 1));
        }

        if (typeof(TKey) != _primaryKeyType)
        {
            throw new ArgumentException(
                CoreStrings.WrongGenericPropertyType(
                    _primaryKey.Properties[0].Name,
                    _primaryKey.Properties[0].DeclaringType.DisplayName(),
                    _primaryKeyType.ShortDisplayName(),
                    typeof(TKey).ShortDisplayName()));
        }

        return _stateManager.TryGetEntryTyped(_primaryKey, keyValue);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityEntry? FindEntry<TProperty>(IProperty property, TProperty propertyValue)
    {
        ValidateProperty(property, propertyValue);

        if (TryFindByKey(property, propertyValue, out var entry))
        {
            return entry;
        }

        if (TryGetByForeignKey(property, propertyValue, out var entries))
        {
            return entries!.FirstOrDefault();
        }

        var comparer = (ValueComparer<TProperty>)property.GetValueComparer();

        foreach (var candidate in _stateManager.GetEntries(_primaryKey))
        {
            if (comparer.Equals(candidate.GetCurrentValue<TProperty>(property), propertyValue))
            {
                return candidate;
            }
        }

        return null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<InternalEntityEntry> GetEntries<TProperty>(IProperty property, TProperty propertyValue)
    {
        ValidateProperty(property, propertyValue);

        if (TryFindByKey(property, propertyValue, out var entry))
        {
            return entry != null
                ? new[] { entry }
                : Enumerable.Empty<InternalEntityEntry>();
        }

        if (TryGetByForeignKey(property, propertyValue, out var entries))
        {
            return entries!;
        }

        var comparer = (ValueComparer<TProperty>)property.GetValueComparer();

        return _stateManager.GetEntries(_primaryKey).Where(e => comparer.Equals(e.GetCurrentValue<TProperty>(property), propertyValue));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityEntry? FindEntry(IEnumerable<object?> keyValues)
    {
        ValidateProperties(_primaryKey.Properties, keyValues, out _, out var valuesList);

        if (valuesList.Any(v => v == null))
        {
            return null;
        }

        return _stateManager.TryGetEntry(_primaryKey, valuesList);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalEntityEntry? FindEntry(IEnumerable<IProperty> properties, IEnumerable<object?> propertyValues)
    {
        ValidateProperties(properties, propertyValues, out var propertiesList, out var valuesList);

        if (TryFindByKey(propertiesList, valuesList, out var entry))
        {
            return entry;
        }

        if (TryGetByForeignKey(propertiesList, valuesList, out var entries))
        {
            return entries!.FirstOrDefault();
        }

        return GetEntries(propertiesList, valuesList).FirstOrDefault();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<InternalEntityEntry> GetEntries(IEnumerable<IProperty> properties, IEnumerable<object?> propertyValues)
    {
        ValidateProperties(properties, propertyValues, out var propertiesList, out var valuesList);

        if (TryFindByKey(propertiesList, valuesList, out var entry))
        {
            return entry != null
                ? new[] { entry }
                : Enumerable.Empty<InternalEntityEntry>();
        }

        if (TryGetByForeignKey(propertiesList, valuesList, out var entries))
        {
            return entries!;
        }

        return GetEntriesByScan(propertiesList, valuesList);
    }

    private IEnumerable<InternalEntityEntry> GetEntriesByScan(IReadOnlyList<IProperty> propertiesList, IReadOnlyList<object?> valuesList)
    {
        var comparers = propertiesList.Select(p => p.GetValueComparer()).ToList();

        foreach (var entry in _stateManager.GetEntries(_primaryKey))
        {
            for (var i = 0; i < comparers.Count; i++)
            {
                if (!comparers[i].Equals(entry[propertiesList[i]], valuesList[i]))
                {
                    goto next;
                }
            }

            yield return entry;
            next: ;
        }
    }

    private static void ValidateProperties(
        IEnumerable<IProperty> properties,
        IEnumerable<object?> propertyValues,
        out IReadOnlyList<IProperty> propertiesList,
        out IReadOnlyList<object?> valuesList)
    {
        propertiesList = (properties as IReadOnlyList<IProperty>) ?? properties.ToList();
        valuesList = (propertyValues as IReadOnlyList<object?>) ?? propertyValues.ToList();

        if (propertiesList.Count != valuesList.Count)
        {
            throw new ArgumentException(CoreStrings.FindWrongCount(valuesList.Count, propertiesList.Count));
        }

        for (var i = 0; i < propertiesList.Count; i++)
        {
            var value = valuesList[i];
            if (value != null
                && !propertiesList[i].ClrType.UnwrapNullableType().IsInstanceOfType(value))
            {
                throw new ArgumentException(
                    CoreStrings.FindWrongType(
                        value.GetType().ShortDisplayName(), propertiesList[i].Name, propertiesList[i].ClrType.ShortDisplayName()));
            }
        }
    }

    private static void ValidateProperty<TProperty>(IProperty property, TProperty value)
    {
        if (value != null
            && !property.ClrType.UnwrapNullableType().IsInstanceOfType(value))
        {
            throw new ArgumentException(
                CoreStrings.FindWrongType(
                    value.GetType().ShortDisplayName(), property.Name, property.ClrType.ShortDisplayName()));
        }
    }

    private bool TryFindByKey<TProperty>(IProperty property, TProperty propertyValue, out InternalEntityEntry? entry)
    {
        var key = _entityType.FindKey(property);
        if (key != null)
        {
            entry = _stateManager.TryGetEntryTyped(key, propertyValue);
            return true;
        }

        entry = null;
        return false;
    }

    private bool TryGetByForeignKey<TProperty>(IProperty property, TProperty propertyValue, out IEnumerable<InternalEntityEntry>? entries)
    {
        var foreignKeys = _entityType.FindForeignKeys(property).ToList();
        if (foreignKeys.Count == 0
            || propertyValue == null)
        {
            entries = null;
            return false;
        }

        var keyValues = new object[] { propertyValue! };
        entries = _stateManager.GetDependents(keyValues, foreignKeys[0]).Cast<InternalEntityEntry>();
        if (foreignKeys.Count == 1)
        {
            return true;
        }

        for (var i = 1; i < foreignKeys.Count; i++)
        {
            entries = entries.Concat(_stateManager.GetDependents(keyValues, foreignKeys[i]).Cast<InternalEntityEntry>());
        }

        entries = entries.Distinct();
        return true;
    }

    private bool TryFindByKey(IReadOnlyList<IProperty> properties, IReadOnlyList<object?> propertyValues, out InternalEntityEntry? entry)
    {
        var key = _entityType.FindKey(properties);
        if (key != null)
        {
            entry = _stateManager.TryGetEntry(key, propertyValues);
            return true;
        }

        entry = null;
        return false;
    }

    private bool TryGetByForeignKey(
        IReadOnlyList<IProperty> properties,
        IReadOnlyList<object?> propertyValues,
        out IEnumerable<InternalEntityEntry>? entries)
    {
        var foreignKeys = _entityType.FindForeignKeys(properties).ToList();
        if (foreignKeys.Count == 0
            || propertyValues.Any(v => v == null))
        {
            entries = null;
            return false;
        }

        entries = _stateManager.GetDependents(propertyValues!, foreignKeys[0]).Cast<InternalEntityEntry>();
        if (foreignKeys.Count == 1)
        {
            return true;
        }

        for (var i = 1; i < foreignKeys.Count; i++)
        {
            entries = entries.Concat(_stateManager.GetDependents(propertyValues!, foreignKeys[i]).Cast<InternalEntityEntry>());
        }

        entries = entries.Distinct();
        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ValueTask<TEntity?> FindAsync(object?[]? keyValues, CancellationToken cancellationToken = default)
    {
        if (keyValues == null
            || keyValues.Any(v => v == null))
        {
            return default;
        }

        var (processedKeyValues, ct) = ValidateKeyPropertiesAndExtractCancellationToken(keyValues!, async: true, cancellationToken);

        var tracked = FindTracked(processedKeyValues);
        return tracked != null
            ? new ValueTask<TEntity?>(tracked)
            : new ValueTask<TEntity?>(
                _queryRoot.FirstOrDefaultAsync(BuildLambda(_primaryKey.Properties, new ValueBuffer(processedKeyValues)), ct));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    ValueTask<object?> IEntityFinder.FindAsync(object?[]? keyValues, CancellationToken cancellationToken)
    {
        if (keyValues == null
            || keyValues.Any(v => v == null))
        {
            return default;
        }

        var (processedKeyValues, ct) = ValidateKeyPropertiesAndExtractCancellationToken(keyValues!, async: true, cancellationToken);

        var tracked = FindTracked(processedKeyValues);
        return tracked != null
            ? new ValueTask<object?>(tracked)
            : new ValueTask<object?>(
                _queryRoot.FirstOrDefaultAsync(
                    BuildObjectLambda(_primaryKey.Properties, new ValueBuffer(processedKeyValues)), ct));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Load(INavigation navigation, InternalEntityEntry entry, LoadOptions options)
    {
        var keyValues = GetLoadValues(navigation, entry);
        // Short-circuit for any null key values for perf and because of #6129
        if (keyValues != null)
        {
            var queryable = Query(navigation, keyValues, entry, options);
            if (entry.EntityState == EntityState.Detached)
            {
                var inverse = navigation.Inverse;
                var stateManager = GetOrCreateStateManagerAndStartTrackingIfNeeded(navigation, entry, options);
                try
                {
                    if (navigation.IsCollection)
                    {
                        foreach (var loaded in queryable)
                        {
                            Fixup(stateManager, entry.Entity, navigation, inverse, options, loaded);
                        }
                    }
                    else
                    {
                        Fixup(stateManager, entry.Entity, navigation, inverse, options, queryable.FirstOrDefault());
                    }
                }
                finally
                {
                    if (stateManager != _stateManager)
                    {
                        stateManager.Clear(resetting: false);
                    }
                }
            }
            else
            {
                if (navigation.IsCollection)
                {
                    queryable.Load();
                }
                else
                {
                    _ = queryable.FirstOrDefault();
                }
            }
        }

        entry.SetIsLoaded(navigation);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual async Task LoadAsync(
        INavigation navigation,
        InternalEntityEntry entry,
        LoadOptions options,
        CancellationToken cancellationToken = default)
    {
        var keyValues = GetLoadValues(navigation, entry);
        // Short-circuit for any null key values for perf and because of #6129
        if (keyValues != null)
        {
            var queryable = Query(navigation, keyValues, entry, options);
            if (entry.EntityState == EntityState.Detached)
            {
                var inverse = navigation.Inverse;
                var stateManager = GetOrCreateStateManagerAndStartTrackingIfNeeded(navigation, entry, options);
                try
                {
                    if (navigation.IsCollection)
                    {
                        await foreach (var loaded in queryable.AsAsyncEnumerable().WithCancellation(cancellationToken)
                                           .ConfigureAwait(false))
                        {
                            Fixup(stateManager, entry.Entity, navigation, inverse, options, loaded);
                        }
                    }
                    else
                    {
                        Fixup(
                            stateManager, entry.Entity, navigation, inverse, options,
                            await queryable.FirstOrDefaultAsync(cancellationToken: cancellationToken).ConfigureAwait(false));
                    }
                }
                finally
                {
                    if (stateManager != _stateManager)
                    {
                        stateManager.Clear(resetting: false);
                    }
                }
            }
            else
            {
                if (navigation.IsCollection)
                {
                    await queryable.LoadAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    _ = await queryable.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        entry.SetIsLoaded(navigation);
    }

    private static void Fixup(
        IStateManager stateManager,
        object entity,
        INavigation beingLoaded,
        INavigation? inverse,
        LoadOptions options,
        object? loaded)
    {
        SetValue(stateManager.GetOrCreateEntry(entity), beingLoaded, loaded, options);

        if (inverse != null && loaded != null)
        {
            SetValue(stateManager.GetOrCreateEntry(loaded), inverse, entity, options);
        }

        static void SetValue(InternalEntityEntry entry, INavigation navigation, object? value, LoadOptions options)
        {
            var stateManager = entry.StateManager;
            if (navigation.IsCollection)
            {
                if (value != null
                    && ((options & LoadOptions.ForceIdentityResolution) == 0 || !TryGetTracked(stateManager, value, out _)))
                {
                    entry.AddToCollection(navigation, value, forMaterialization: true);
                }
            }
            else
            {
                if (value != null
                    && (options & LoadOptions.ForceIdentityResolution) != 0
                    && TryGetTracked(stateManager, value, out var existing))
                {
                    value = existing;
                }

                entry.SetProperty(navigation, value, isMaterialization: true, setModified: false);
            }

            static bool TryGetTracked(IStateManager stateManager, object value, out object? tracked)
            {
                var relatedEntry = stateManager.GetOrCreateEntry(value);
                var key = relatedEntry.EntityType.FindPrimaryKey();
                if (key == null)
                {
                    tracked = null;
                    return false;
                }

                tracked = stateManager.TryGetExistingEntry(relatedEntry.Entity, key)?.Entity;
                return tracked != null;
            }
        }
    }

    private IStateManager GetOrCreateStateManagerAndStartTrackingIfNeeded(
        INavigation loading,
        InternalEntityEntry entry,
        LoadOptions options)
    {
        if ((options & LoadOptions.ForceIdentityResolution) == 0)
        {
            return _stateManager;
        }

        var stateManager = new StateManager(_stateManager.Dependencies);
        StartTracking(stateManager, entry, loading);
        return stateManager;
    }

    private static void StartTracking(StateManager stateManager, InternalEntityEntry entry, INavigation navigation)
    {
        Track(entry.Entity);

        var navigationValue = entry[navigation];
        if (navigationValue != null)
        {
            if (navigation.IsCollection)
            {
                foreach (var related in (IEnumerable)navigationValue)
                {
                    Track(related);
                }
            }
            else
            {
                Track(navigationValue);
            }
        }

        void Track(object entity)
            => stateManager.StartTracking(stateManager.GetOrCreateEntry(entity)).MarkUnchangedFromQuery();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IQueryable<TEntity> Query(INavigation navigation, InternalEntityEntry entry)
    {
        var keyValues = GetLoadValues(navigation, entry);
        // Short-circuit for any null key values for perf and because of #6129
        if (keyValues == null)
        {
            // Creates an empty Queryable that works with Async. Has to be an EF query because it
            // could be used in a composition.
            return _queryRoot.Where(e => false);
        }

        return Query(navigation, keyValues, entry, LoadOptions.None);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object[]? GetDatabaseValues(InternalEntityEntry entry)
        => GetDatabaseValuesQuery(entry)?.FirstOrDefault();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<object[]?> GetDatabaseValuesAsync(
        InternalEntityEntry entry,
        CancellationToken cancellationToken = default)
        => GetDatabaseValuesQuery(entry)?.FirstOrDefaultAsync(cancellationToken) ?? Task.FromResult((object[]?)null);

    private IQueryable<object[]>? GetDatabaseValuesQuery(InternalEntityEntry entry)
    {
        var entityType = entry.EntityType;
        var properties = entityType.FindPrimaryKey()!.Properties;

        var keyValues = new object[properties.Count];
        for (var i = 0; i < keyValues.Length; i++)
        {
            var keyValue = entry[properties[i]];
            if (keyValue == null)
            {
                return null;
            }

            keyValues[i] = keyValue;
        }

        return _queryRoot.AsNoTracking().IgnoreQueryFilters()
            .Where(BuildObjectLambda(properties, new ValueBuffer(keyValues)))
            .Select(BuildProjection(entityType));
    }

    private IQueryable<TEntity> Query(INavigation navigation, object[] keyValues, InternalEntityEntry entry, LoadOptions options)
    {
        var queryable = _queryRoot.Where(BuildLambda(GetLoadProperties(navigation), new ValueBuffer(keyValues)));
        return entry.EntityState == EntityState.Detached
            ? (options & LoadOptions.ForceIdentityResolution) != 0
                ? queryable.AsNoTrackingWithIdentityResolution()
                : queryable.AsNoTracking()
            : queryable.AsTracking();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    IQueryable IEntityFinder.Query(INavigation navigation, InternalEntityEntry entry)
        => Query(navigation, entry);

    private static object[]? GetLoadValues(INavigation navigation, InternalEntityEntry entry)
    {
        var properties = navigation.IsOnDependent
            ? navigation.ForeignKey.Properties
            : navigation.ForeignKey.PrincipalKey.Properties;

        var values = new object[properties.Count];
        var detached = entry.EntityState == EntityState.Detached;

        for (var i = 0; i < values.Length; i++)
        {
            var property = properties[i];
            if (property.IsShadowProperty() && (detached || (entry.EntityState != EntityState.Added && entry.IsUnknown(property))))
            {
                throw new InvalidOperationException(
                    CoreStrings.CannotLoadDetachedShadow(navigation.Name, entry.EntityType.DisplayName()));
            }

            var value = entry[property];
            if (value == null)
            {
                return null;
            }

            values[i] = value;
        }

        return values;
    }

    private static IReadOnlyList<IProperty> GetLoadProperties(INavigation navigation)
        => navigation.IsOnDependent
            ? navigation.ForeignKey.PrincipalKey.Properties
            : navigation.ForeignKey.Properties;

    private (object[] KeyValues, CancellationToken CancellationToken) ValidateKeyPropertiesAndExtractCancellationToken(
        object[] keyValues,
        bool async,
        CancellationToken cancellationToken)
    {
        if (_primaryKeyPropertiesCount != keyValues.Length)
        {
            if (async
                && _primaryKeyPropertiesCount == keyValues.Length - 1
                && keyValues[_primaryKeyPropertiesCount] is CancellationToken ct)
            {
                var newValues = new object[_primaryKeyPropertiesCount];
                Array.Copy(keyValues, newValues, _primaryKeyPropertiesCount);
                return (newValues, ct);
            }

            if (_primaryKeyPropertiesCount == 1)
            {
                throw new ArgumentException(
                    CoreStrings.FindNotCompositeKey(typeof(TEntity).ShortDisplayName(), keyValues.Length));
            }

            throw new ArgumentException(
                CoreStrings.FindValueCountMismatch(typeof(TEntity).ShortDisplayName(), _primaryKeyPropertiesCount, keyValues.Length));
        }

        return (keyValues, cancellationToken);
    }

    private TEntity? FindTracked(object[] keyValues)
    {
        var keyProperties = _primaryKey.Properties;
        for (var i = 0; i < keyValues.Length; i++)
        {
            var valueType = keyValues[i].GetType();
            var propertyType = keyProperties[i].ClrType;
            if (!propertyType.UnwrapNullableType().IsAssignableFrom(valueType))
            {
                throw new ArgumentException(
                    CoreStrings.FindValueTypeMismatch(
                        i, typeof(TEntity).ShortDisplayName(), valueType.ShortDisplayName(), propertyType.ShortDisplayName()));
            }
        }

        return _stateManager.TryGetEntry(_primaryKey, keyValues)?.Entity as TEntity;
    }

    private static Expression<Func<TEntity, bool>> BuildLambda(IReadOnlyList<IProperty> keyProperties, ValueBuffer keyValues)
    {
        var entityParameter = Expression.Parameter(typeof(TEntity), "e");

        return Expression.Lambda<Func<TEntity, bool>>(
            ExpressionExtensions.BuildPredicate(keyProperties, keyValues, entityParameter), entityParameter);
    }

    private static Expression<Func<object, bool>> BuildObjectLambda(IReadOnlyList<IProperty> keyProperties, ValueBuffer keyValues)
    {
        var entityParameter = Expression.Parameter(typeof(object), "e");

        return Expression.Lambda<Func<object, bool>>(
            ExpressionExtensions.BuildPredicate(keyProperties, keyValues, entityParameter), entityParameter);
    }

    private IQueryable BuildQueryRoot(IEntityType entityType)
        => entityType.FindOwnership() is IForeignKey ownership
            ? BuildQueryRoot(ownership.PrincipalEntityType, entityType, ownership.PrincipalToDependent!.Name)
            : entityType.HasSharedClrType
                ? (IQueryable)_setCache.GetOrAddSet(_setSource, entityType.Name, entityType.ClrType)
                : (IQueryable)_setCache.GetOrAddSet(_setSource, entityType.ClrType);

    private IQueryable BuildQueryRoot(IEntityType ownerEntityType, IEntityType entityType, string navigationName)
    {
        var queryRoot = BuildQueryRoot(ownerEntityType);
        var collectionNavigation = ownerEntityType.FindNavigation(navigationName)!.IsCollection;

        return (IQueryable)(collectionNavigation ? SelectManyMethod : SelectMethod)
            .MakeGenericMethod(ownerEntityType.ClrType, entityType.ClrType)
            .Invoke(null, [queryRoot, navigationName])!;
    }

    private static readonly MethodInfo SelectMethod
        = typeof(EntityFinder<TEntity>).GetTypeInfo().GetDeclaredMethods(nameof(Select)).Single(mi => mi.IsGenericMethodDefinition);

    private static IQueryable<TResult> Select<TSource, TResult>(
        IQueryable<TSource> source,
        string propertyName)
        where TResult : class
        where TSource : class
    {
        var parameter = Expression.Parameter(typeof(TSource), "e");
        return source.Select(
            Expression.Lambda<Func<TSource, TResult>>(
                Expression.MakeMemberAccess(parameter, typeof(TSource).GetAnyProperty(propertyName)!),
                parameter));
    }

    private static readonly MethodInfo SelectManyMethod
        = typeof(EntityFinder<TEntity>).GetTypeInfo().GetDeclaredMethods(nameof(SelectMany)).Single(mi => mi.IsGenericMethodDefinition);

    private static IQueryable<TResult> SelectMany<TSource, TResult>(
        IQueryable<TSource> source,
        string propertyName)
        where TResult : class
        where TSource : class
    {
        var parameter = Expression.Parameter(typeof(TSource), "e");
        return source.SelectMany(
            Expression.Lambda<Func<TSource, IEnumerable<TResult>>>(
                Expression.MakeMemberAccess(parameter, typeof(TSource).GetAnyProperty(propertyName)!),
                parameter));
    }

    private static Expression<Func<object, object[]>> BuildProjection(IEntityType entityType)
    {
        var entityParameter = Expression.Parameter(typeof(object), "e");

        var projections = new List<Expression>();
        foreach (var property in entityType.GetFlattenedProperties())
        {
            var path = new List<IPropertyBase> { property };
            while (path[^1].DeclaringType is IComplexType complexType)
            {
                path.Add(complexType.ComplexProperty);
            }

            Expression instanceExpression = entityParameter;
            for (var i = path.Count - 1; i >= 0; i--)
            {
                instanceExpression = Expression.Call(
                    EF.PropertyMethod.MakeGenericMethod(path[i].ClrType),
                    instanceExpression,
                    Expression.Constant(path[i].Name, typeof(string)));

                if (i != 0 && instanceExpression.Type.IsValueType)
                {
                    instanceExpression = Expression.Convert(instanceExpression, typeof(object));
                }
            }

            projections.Add(
                Expression.Convert(
                    Expression.Convert(
                        instanceExpression,
                        property.ClrType),
                    typeof(object)));
        }

        return Expression.Lambda<Func<object, object[]>>(
            Expression.NewArrayInit(typeof(object), projections),
            entityParameter);
    }
}
