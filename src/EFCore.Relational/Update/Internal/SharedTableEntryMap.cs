// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
        private readonly IUpdateAdapter _updateAdapter;
        private readonly IReadOnlyDictionary<IEntityType, IReadOnlyList<IEntityType>> _principals;
        private readonly IReadOnlyDictionary<IEntityType, IReadOnlyList<IEntityType>> _dependents;
        private readonly string _name;
        private readonly string _schema;
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
            [NotNull] IUpdateAdapter updateAdapter,
            [NotNull] IReadOnlyDictionary<IEntityType, IReadOnlyList<IEntityType>> principals,
            [NotNull] IReadOnlyDictionary<IEntityType, IReadOnlyList<IEntityType>> dependents,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] SharedTableEntryValueFactory<TValue> createElement)
        {
            _updateAdapter = updateAdapter;
            _principals = principals;
            _dependents = dependents;
            _name = name;
            _schema = schema;
            _createElement = createElement;
            _comparer = new EntryComparer(principals);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static Dictionary<(string Schema, string Name), SharedTableEntryMapFactory<TValue>>
            CreateSharedTableEntryMapFactories(
                [NotNull] IModel model,
                [NotNull] IUpdateAdapter updateAdapter)
        {
            var tables = new Dictionary<(string Schema, string TableName), List<IEntityType>>();
            foreach (var entityType in model.GetEntityTypes().Where(et => et.FindPrimaryKey() != null))
            {
                var fullName = (entityType.GetSchema(), entityType.GetTableName());
                if (!tables.TryGetValue(fullName, out var mappedEntityTypes))
                {
                    mappedEntityTypes = new List<IEntityType>();
                    tables.Add(fullName, mappedEntityTypes);
                }

                mappedEntityTypes.Add(entityType);
            }

            var sharedTablesMap = new Dictionary<(string Schema, string Name), SharedTableEntryMapFactory<TValue>>();
            foreach (var tableMapping in tables)
            {
                if (tableMapping.Value.Count <= 1)
                {
                    continue;
                }

                var factory = CreateSharedTableEntryMapFactory(tableMapping.Value, updateAdapter, tableMapping.Key.TableName, tableMapping.Key.Schema);

                sharedTablesMap.Add(tableMapping.Key, factory);
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
            [NotNull] IReadOnlyList<IEntityType> entityTypes,
            [NotNull] IUpdateAdapter updateAdapter,
            [NotNull] string tableName,
            [NotNull] string schema)
        {
            var principals = new Dictionary<IEntityType, IReadOnlyList<IEntityType>>(entityTypes.Count);
            var dependents = new Dictionary<IEntityType, IReadOnlyList<IEntityType>>(entityTypes.Count);
            foreach (var entityType in entityTypes.Where(t => t.FindPrimaryKey() != null))
            {
                var principalList = new List<IEntityType>();
                foreach (var foreignKey in entityType.FindForeignKeys(entityType.FindPrimaryKey().Properties))
                {
                    if (foreignKey.PrincipalKey.IsPrimaryKey()
                        && entityTypes.Contains(foreignKey.PrincipalEntityType)
                        && !foreignKey.IsIntraHierarchical())
                    {
                        principalList.Add(foreignKey.PrincipalEntityType);
                    }
                }

                principals[entityType] = principalList;

                var dependentList = new List<IEntityType>();
                foreach (var referencingForeignKey in entityType.FindPrimaryKey().GetReferencingForeignKeys())
                {
                    if (referencingForeignKey.PrincipalEntityType.IsAssignableFrom(entityType)
                        && entityTypes.Contains(referencingForeignKey.DeclaringEntityType)
                        && !referencingForeignKey.IsIntraHierarchical()
                        && PropertyListComparer.Instance.Compare(
                            referencingForeignKey.DeclaringEntityType.FindPrimaryKey().Properties, referencingForeignKey.Properties) == 0)
                    {
                        dependentList.Add(referencingForeignKey.DeclaringEntityType);
                    }
                }

                dependents[entityType] = dependentList;
            }

            return createElement => new SharedTableEntryMap<TValue>(
                updateAdapter,
                principals,
                dependents,
                tableName,
                schema,
                createElement);
        }

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

            sharedCommand = _createElement(_name, _schema, _comparer);
            _entryValueMap.Add(mainEntry, sharedCommand);

            return sharedCommand;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<IEntityType> GetPrincipals([NotNull] IEntityType entityType) => _principals[entityType];

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<IEntityType> GetDependents([NotNull] IEntityType entityType) => _dependents[entityType];

        private IUpdateEntry GetMainEntry(IUpdateEntry entry)
        {
            var entityType = entry.EntityType.GetRootType();
            if (_principals[entityType].Count == 0)
            {
                return entry;
            }

            foreach (var foreignKey in entityType.FindForeignKeys(entityType.FindPrimaryKey().Properties))
            {
                if (foreignKey.PrincipalKey.IsPrimaryKey()
                    && _principals.ContainsKey(foreignKey.PrincipalEntityType))
                {
                    var principalEntry = _updateAdapter.FindPrincipal(entry, foreignKey);
                    if (principalEntry != null)
                    {
                        return GetMainEntry(principalEntry);
                    }
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
            foreach (var foreignKey in entry.EntityType.GetReferencingForeignKeys())
            {
                if (foreignKey.PrincipalKey.IsPrimaryKey()
                    && foreignKey.IsUnique
                    && _dependents.ContainsKey(foreignKey.DeclaringEntityType))
                {
                    var dependentEntry = _updateAdapter.GetDependents(entry, foreignKey).SingleOrDefault();
                    if (dependentEntry != null)
                    {
                        AddAllDependentsInclusive(dependentEntry, entries);
                    }
                }
            }
        }

        private class EntryComparer : IComparer<IUpdateEntry>
        {
            private readonly IReadOnlyDictionary<IEntityType, IReadOnlyList<IEntityType>> _principals;

            public EntryComparer(IReadOnlyDictionary<IEntityType, IReadOnlyList<IEntityType>> principals)
            {
                _principals = principals;
            }

            public int Compare(IUpdateEntry x, IUpdateEntry y)
            {
                if (_principals[x.EntityType].Count == 0)
                {
                    return -1;
                }

                return _principals[y.EntityType].Count == 0 ? 1 : StringComparer.Ordinal.Compare(x.EntityType.Name, y.EntityType.Name);
            }
        }
    }
}
