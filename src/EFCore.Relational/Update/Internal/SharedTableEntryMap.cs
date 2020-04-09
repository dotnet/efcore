// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
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
        private readonly SharedTableEntryValueFactory<TValue> _createElement;
        private readonly IComparer<IUpdateEntry> _comparer;

        private readonly Dictionary<IUpdateEntry, TValue> _entryValueMap
            = new Dictionary<IUpdateEntry, TValue>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SharedTableEntryMap(
            [NotNull] ITable table,
            [NotNull] IUpdateAdapter updateAdapter,
            [NotNull] SharedTableEntryValueFactory<TValue> createElement)
        {
            _table = table;
            _updateAdapter = updateAdapter;
            _createElement = createElement;
            _comparer = new EntryComparer(table);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static Dictionary<(string Name, string Schema), SharedTableEntryMapFactory<TValue>>
            CreateSharedTableEntryMapFactories(
                [NotNull] IModel model,
                [NotNull] IUpdateAdapter updateAdapter)
        {
            var sharedTablesMap = new Dictionary<(string, string), SharedTableEntryMapFactory<TValue>>();
            foreach (var table in model.GetRelationalModel().Tables)
            {
                if (!table.IsSplit)
                {
                    continue;
                }

                var factory = CreateSharedTableEntryMapFactory(table, updateAdapter);

                sharedTablesMap.Add((table.Name, table.Schema), factory);
            }

            return sharedTablesMap;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static SharedTableEntryMapFactory<TValue> CreateSharedTableEntryMapFactory(
            [NotNull] ITable table,
            [NotNull] IUpdateAdapter updateAdapter)
            => createElement
                => new SharedTableEntryMap<TValue>(
                    table,
                    updateAdapter,
                    createElement);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<TValue> Values => _entryValueMap.Values;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual TValue GetOrAddValue([NotNull] IUpdateEntry entry)
        {
            var mainEntry = GetMainEntry(entry);
            if (_entryValueMap.TryGetValue(mainEntry, out var sharedCommand))
            {
                return sharedCommand;
            }

            sharedCommand = _createElement(_table.Name, _table.Schema, _comparer);
            _entryValueMap.Add(mainEntry, sharedCommand);

            return sharedCommand;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsMainEntityType([NotNull] IEntityType entityType) => !_table.GetInternalForeignKeys(entityType).Any();

        private IUpdateEntry GetMainEntry(IUpdateEntry entry)
        {
            var entityType = entry.EntityType.GetRootType();
            var foreignKeys = _table.GetInternalForeignKeys(entityType);
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
        public virtual IReadOnlyList<IUpdateEntry> GetAllEntries([NotNull] IUpdateEntry entry)
        {
            var entries = new List<IUpdateEntry>();
            AddAllDependentsInclusive(GetMainEntry(entry), entries);

            return entries;
        }

        private void AddAllDependentsInclusive(IUpdateEntry entry, List<IUpdateEntry> entries)
        {
            entries.Add(entry);
            var foreignKeys = _table.GetReferencingInternalForeignKeys(entry.EntityType);
            if (!foreignKeys.Any())
            {
                return;
            }

            foreach (var foreignKey in foreignKeys)
            {
                var dependentEntry = _updateAdapter.GetDependents(entry, foreignKey).SingleOrDefault();
                if (dependentEntry != null)
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

            public int Compare(IUpdateEntry x, IUpdateEntry y)
                => !_table.GetInternalForeignKeys(x.EntityType).Any()
                    ? -1
                    : !_table.GetInternalForeignKeys(y.EntityType).Any()
                        ? 1
                        : StringComparer.Ordinal.Compare(x.EntityType.Name, y.EntityType.Name);
        }
    }
}
