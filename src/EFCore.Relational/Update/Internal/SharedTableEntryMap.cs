// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SharedTableEntryMap<TValue>
{
    private readonly ITable _table;
    private readonly IUpdateAdapter _updateAdapter;
    private readonly IComparer<IUpdateEntry> _comparer;
    private readonly Dictionary<IUpdateEntry, TValue> _entryValueMap = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SharedTableEntryMap(
        ITable table,
        IUpdateAdapter updateAdapter)
    {
        _table = table;
        _updateAdapter = updateAdapter;
        _comparer = new EntryComparer(table);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<TValue> Values
        => _entryValueMap.Values;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TValue GetOrAddValue(IUpdateEntry entry, SharedTableEntryValueFactory<TValue> createElement)
    {
        var mainEntry = GetMainEntry(entry);
        if (_entryValueMap.TryGetValue(mainEntry, out var sharedCommand))
        {
            return sharedCommand;
        }

        sharedCommand = createElement(_table, _comparer);
        _entryValueMap.Add(mainEntry, sharedCommand);

        return sharedCommand;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsMainEntry(IUpdateEntry entry)
        => !_table.GetRowInternalForeignKeys(entry.EntityType).Any();

    private IUpdateEntry GetMainEntry(IUpdateEntry entry)
    {
        var entityType = entry.EntityType;
        var foreignKeys = _table.GetRowInternalForeignKeys(entityType);
        foreach (var foreignKey in foreignKeys)
        {
            var principalEntry = _updateAdapter.FindPrincipal(entry, foreignKey);
            if (principalEntry != null)
            {
                return GetMainEntry(principalEntry);
            }
        }

        return entry;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<IUpdateEntry> GetAllEntries(IUpdateEntry entry)
    {
        var entries = new List<IUpdateEntry>();
        AddAllDependentsInclusive(GetMainEntry(entry), entries);

        return entries;
    }

    private void AddAllDependentsInclusive(IUpdateEntry entry, List<IUpdateEntry> entries)
    {
        entries.Add(entry);
        var foreignKeys = _table.GetReferencingRowInternalForeignKeys(entry.EntityType);
        if (!foreignKeys.Any())
        {
            return;
        }

        foreach (var foreignKey in foreignKeys)
        {
            var dependentEntries = _updateAdapter.GetDependents(entry, foreignKey);
            foreach (var dependentEntry in dependentEntries)
            {
                AddAllDependentsInclusive(dependentEntry, entries);
            }
        }
    }

    private sealed class EntryComparer : IComparer<IUpdateEntry>
    {
        private readonly ITable _table;

        public EntryComparer(ITable table)
        {
            _table = table;
        }

        public int Compare(IUpdateEntry? x, IUpdateEntry? y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            return !_table.GetRowInternalForeignKeys(x.EntityType).Any()
                ? -1
                : !_table.GetRowInternalForeignKeys(y.EntityType).Any()
                    ? 1
                    : StringComparer.Ordinal.Compare(x.EntityType.Name, y.EntityType.Name);
        }
    }
}
