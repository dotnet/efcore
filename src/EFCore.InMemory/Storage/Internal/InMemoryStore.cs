// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Internal;
using Microsoft.EntityFrameworkCore.InMemory.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InMemoryStore : IInMemoryStore
{
    private readonly IInMemoryTableFactory _tableFactory;

    private readonly object _lock = new();

    private Dictionary<string, IInMemoryTable>? _tables;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InMemoryStore(IInMemoryTableFactory tableFactory)
    {
        _tableFactory = tableFactory;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InMemoryIntegerValueGenerator<TProperty> GetIntegerValueGenerator<TProperty>(
        IProperty property)
    {
        var entityType = property.DeclaringType.ContainingEntityType;
        lock (_lock)
        {
            return EnsureTable(entityType).GetIntegerValueGenerator<TProperty>(
                property,
                entityType.GetDerivedTypesInclusive().Select(EnsureTable).ToArray());
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool EnsureCreated(
        IUpdateAdapterFactory updateAdapterFactory,
        IModel designModel,
        IDiagnosticsLogger<DbLoggerCategory.Update> updateLogger)
    {
        lock (_lock)
        {
            var valuesSeeded = _tables == null;
            if (valuesSeeded)
            {
                // ReSharper disable once AssignmentIsFullyDiscarded
                _tables = CreateTables();

                var updateAdapter = updateAdapterFactory.CreateStandalone();
                var entries = new List<IUpdateEntry>();
                foreach (var entityType in designModel.GetEntityTypes())
                {
                    IEntityType? targetEntityType = null;
                    foreach (var targetSeed in entityType.GetSeedData())
                    {
                        targetEntityType ??= updateAdapter.Model.FindEntityType(entityType.Name)!;
                        var entry = updateAdapter.CreateEntry(targetSeed, targetEntityType);
                        entry.EntityState = EntityState.Added;
                        entries.Add(entry);
                    }
                }

                ExecuteTransaction(entries, updateLogger);
            }

            return valuesSeeded;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool Clear()
    {
        lock (_lock)
        {
            if (_tables == null)
            {
                return false;
            }

            _tables = null;

            return true;
        }
    }

    private static Dictionary<string, IInMemoryTable> CreateTables()
        => new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<InMemoryTableSnapshot> GetTables(IEntityType entityType)
    {
        var data = new List<InMemoryTableSnapshot>();
        lock (_lock)
        {
            if (_tables != null)
            {
                foreach (var et in entityType.GetDerivedTypesInclusive().Where(et => !et.IsAbstract()))
                {
                    if (_tables.TryGetValue(et.Name, out var table))
                    {
                        data.Add(new InMemoryTableSnapshot(et, table.SnapshotRows()));
                    }
                }
            }
        }

        return data;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int ExecuteTransaction(
        IList<IUpdateEntry> entries,
        IDiagnosticsLogger<DbLoggerCategory.Update> updateLogger)
    {
        var rowsAffected = 0;

        lock (_lock)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var entityType = entry.EntityType;

                Check.DebugAssert(!entityType.IsAbstract(), "entityType is abstract");

                var table = EnsureTable(entityType);

                if (entry.SharedIdentityEntry != null)
                {
                    if (entry.EntityState == EntityState.Deleted)
                    {
                        continue;
                    }

                    table.Delete(entry.SharedIdentityEntry, updateLogger);
                }

                switch (entry.EntityState)
                {
                    case EntityState.Added:
                        table.Create(entry, updateLogger);
                        break;
                    case EntityState.Deleted:
                        table.Delete(entry, updateLogger);
                        break;
                    case EntityState.Modified:
                        table.Update(entry, updateLogger);
                        break;
                }

                rowsAffected++;
            }
        }

        updateLogger.ChangesSaved(entries, rowsAffected);

        return rowsAffected;
    }

    // Must be called from inside the lock
    private IInMemoryTable EnsureTable(IEntityType entityType)
    {
        _tables ??= CreateTables();

        IInMemoryTable? baseTable = null;

        var entityTypes = entityType.GetAllBaseTypesInclusive();
        foreach (var currentEntityType in entityTypes)
        {
            var key = currentEntityType.Name;
            if (!_tables.TryGetValue(key, out var table))
            {
                _tables.Add(key, table = _tableFactory.Create(currentEntityType, baseTable));
            }

            baseTable = table;
        }

        return _tables[entityType.Name];
    }
}
