// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class DependentsMap<TKey> : IDependentsMap
    where TKey : notnull
{
    private readonly IForeignKey _foreignKey;
    private readonly IPrincipalKeyValueFactory<TKey> _principalKeyValueFactory;
    private readonly IDependentKeyValueFactory<TKey> _dependentKeyValueFactory;
    private readonly Dictionary<TKey, HashSet<IUpdateEntry>> _map;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public DependentsMap(
        IForeignKey foreignKey,
        IPrincipalKeyValueFactory<TKey> principalKeyValueFactory,
        IDependentKeyValueFactory<TKey> dependentKeyValueFactory)
    {
        _foreignKey = foreignKey;
        _principalKeyValueFactory = principalKeyValueFactory;
        _dependentKeyValueFactory = dependentKeyValueFactory;
        _map = new Dictionary<TKey, HashSet<IUpdateEntry>>(principalKeyValueFactory.EqualityComparer);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Add(IUpdateEntry entry)
    {
        if (_foreignKey.DeclaringEntityType.IsAssignableFrom(entry.EntityType)
            && TryCreateFromCurrentValues(entry, out var key))
        {
            if (!_map.TryGetValue(key, out var dependents))
            {
                dependents = [];
                _map[key] = dependents;
            }

            dependents.Add(entry);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Remove(IUpdateEntry entry)
    {
        if (_foreignKey.DeclaringEntityType.IsAssignableFrom(entry.EntityType)
            && TryCreateFromCurrentValues(entry, out var key))
        {
            if (_map.TryGetValue(key, out var dependents))
            {
                dependents.Remove(entry);
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Update(IUpdateEntry entry)
    {
        if (_foreignKey.DeclaringEntityType.IsAssignableFrom(entry.EntityType))
        {
            if (_dependentKeyValueFactory.TryCreateFromRelationshipSnapshot(entry, out var key)
                && _map.TryGetValue(key, out var dependents))
            {
                dependents.Remove(entry);
            }

            if (TryCreateFromCurrentValues(entry, out key))
            {
                if (!_map.TryGetValue(key, out dependents))
                {
                    dependents = [];
                    _map[key] = dependents;
                }

                dependents.Add(entry);
            }
        }
    }

    private bool TryCreateFromCurrentValues(IUpdateEntry entry, [NotNullWhen(true)] out TKey? key)
    {
        // TODO: Move into delegate
        foreach (var property in _foreignKey.Properties)
        {
            if (entry.IsConceptualNull(property))
            {
                key = default;
                return false;
            }
        }

        return _dependentKeyValueFactory.TryCreateFromCurrentValues(entry, out key);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<IUpdateEntry> GetDependents(IUpdateEntry principalEntry)
        => _map.TryGetValue(_principalKeyValueFactory.CreateFromCurrentValues(principalEntry)!, out var dependents)
            ? dependents
            : Enumerable.Empty<IUpdateEntry>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<IUpdateEntry> GetDependents(IReadOnlyList<object?> keyValues)
        => _map.TryGetValue((TKey)_principalKeyValueFactory.CreateFromKeyValues(keyValues)!, out var dependents)
            ? dependents
            : Enumerable.Empty<IUpdateEntry>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<IUpdateEntry> GetDependentsUsingRelationshipSnapshot(IUpdateEntry principalEntry)
        => _map.TryGetValue(_principalKeyValueFactory.CreateFromRelationshipSnapshot(principalEntry), out var dependents)
            ? dependents
            : Enumerable.Empty<IUpdateEntry>();
}
