// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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

            sharedCommand = createElement(_table.Name, _table.Schema, _comparer);
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsOptionalWithNull(IUpdateEntry entry)
        {
            var principalEntityTypesMap = new Dictionary<IEntityType, bool>();

            var optional = GetPrincipalEntityTypes(entry.EntityType);

            var nullableWithNull = true;

            if (!optional)
            {
                return false;
            }

            foreach (var property in entry.EntityType.GetProperties())
            {
                if (property.IsPrimaryKey())
                {
                    continue;
                }

                if(entry.GetCurrentValue(property) is not null)
                {
                    nullableWithNull = false;
                }
            }

            bool GetPrincipalEntityTypes(IEntityType entityType)
            {
                if (!principalEntityTypesMap.TryGetValue(entityType, out var optional))
                {
                    foreach (var foreignKey in entityType.FindForeignKeys(entityType.FindPrimaryKey()!.Properties))
                    {
                        var principalEntityType = foreignKey.PrincipalEntityType;
                        var innerOptional = GetPrincipalEntityTypes(principalEntityType.GetRootType());

                        optional |= !foreignKey.IsRequiredDependent | innerOptional;
                    }
                }

                return optional;
            }

            return nullableWithNull;
        }

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
}
