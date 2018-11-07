// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SharedTableEntryMap<TValue>
    {
        private readonly IStateManager _stateManager;
        private readonly IReadOnlyDictionary<IEntityType, IReadOnlyList<IEntityType>> _principals;
        private readonly IReadOnlyDictionary<IEntityType, IReadOnlyList<IEntityType>> _dependents;
        private readonly string _name;
        private readonly string _schema;
        private readonly SharedTableEntryValueFactory<TValue> _createElement;
        private readonly IComparer<IUpdateEntry> _comparer;

        private readonly Dictionary<InternalEntityEntry, TValue> _entryValueMap
            = new Dictionary<InternalEntityEntry, TValue>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SharedTableEntryMap(
            [NotNull] IStateManager stateManager,
            [NotNull] IReadOnlyDictionary<IEntityType, IReadOnlyList<IEntityType>> principals,
            [NotNull] IReadOnlyDictionary<IEntityType, IReadOnlyList<IEntityType>> dependents,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] SharedTableEntryValueFactory<TValue> createElement)
        {
            _stateManager = stateManager;
            _principals = principals;
            _dependents = dependents;
            _name = name;
            _schema = schema;
            _createElement = createElement;
            _comparer = new EntryComparer(principals);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Dictionary<(string Schema, string Name), SharedTableEntryMapFactory<TValue>>
            CreateSharedTableEntryMapFactories([NotNull] IModel model, [NotNull] IStateManager stateManager)
        {
            var tables = new Dictionary<(string Schema, string TableName), List<IEntityType>>();
            foreach (var entityType in model.GetEntityTypes().Where(et => !et.IsQueryType))
            {
                var fullName = (entityType.Relational().Schema, entityType.Relational().TableName);
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

                var factory = CreateSharedTableEntryMapFactory(tableMapping.Value, stateManager, tableMapping.Key.TableName, tableMapping.Key.Schema);

                sharedTablesMap.Add(tableMapping.Key, factory);
            }

            return sharedTablesMap;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static SharedTableEntryMapFactory<TValue> CreateSharedTableEntryMapFactory(
            [NotNull] IReadOnlyList<IEntityType> entityTypes,
            [NotNull] IStateManager stateManager,
            [NotNull] string tableName,
            [NotNull] string schema)
        {
            var principals = new Dictionary<IEntityType, IReadOnlyList<IEntityType>>(entityTypes.Count);
            var dependents = new Dictionary<IEntityType, IReadOnlyList<IEntityType>>(entityTypes.Count);
            foreach (var entityType in entityTypes)
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
                stateManager,
                principals,
                dependents,
                tableName,
                schema,
                createElement);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<TValue> Values => _entryValueMap.Values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TValue GetOrAddValue([NotNull] IUpdateEntry entry)
        {
            var mainEntry = GetMainEntry((InternalEntityEntry)entry);
            if (_entryValueMap.TryGetValue(mainEntry, out var sharedCommand))
            {
                return sharedCommand;
            }

            sharedCommand = _createElement(_name, _schema, _comparer);
            _entryValueMap.Add(mainEntry, sharedCommand);

            return sharedCommand;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<IEntityType> GetPrincipals([NotNull] IEntityType entityType) => _principals[entityType];

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<IEntityType> GetDependents([NotNull] IEntityType entityType) => _dependents[entityType];

        private InternalEntityEntry GetMainEntry(InternalEntityEntry entry)
        {
            var entityType = entry.EntityType.RootType();
            if (_principals[entityType].Count == 0)
            {
                return entry;
            }

            foreach (var foreignKey in entityType.FindForeignKeys(entityType.FindPrimaryKey().Properties))
            {
                if (foreignKey.PrincipalKey.IsPrimaryKey()
                    && _principals.ContainsKey(foreignKey.PrincipalEntityType))
                {
                    var principal = _stateManager.GetPrincipal(entry, foreignKey);
                    if (principal != null)
                    {
                        return GetMainEntry(principal);
                    }
                }
            }

            return entry;
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
