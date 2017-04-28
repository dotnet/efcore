// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ModificationCommandIdentityMap
    {
        private readonly IStateManager _stateManager;
        private readonly IReadOnlyDictionary<IEntityType, int> _rootTypesOrder;
        private readonly string _name;
        private readonly string _schema;
        private readonly Func<string> _generateParameterName;
        private readonly IRelationalAnnotationProvider _annotationProvider;
        private readonly bool _sensitiveLoggingEnabled;
        private readonly IComparer<IUpdateEntry> _comparer;

        private readonly Dictionary<InternalEntityEntry, ModificationCommand> _sharedCommands
            = new Dictionary<InternalEntityEntry, ModificationCommand>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ModificationCommandIdentityMap(
            [NotNull] IStateManager stateManager,
            [NotNull] IReadOnlyDictionary<IEntityType, int> rootTypesOrder,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] Func<string> generateParameterName,
            [NotNull] IRelationalAnnotationProvider annotationProvider,
            bool sensitiveLoggingEnabled)
        {
            _stateManager = stateManager;
            _rootTypesOrder = rootTypesOrder;
            _name = name;
            _schema = schema;
            _generateParameterName = generateParameterName;
            _annotationProvider = annotationProvider;
            _sensitiveLoggingEnabled = sensitiveLoggingEnabled;
            _comparer = new EntryComparer(rootTypesOrder);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ModificationCommand GetOrAddCommand([NotNull] IUpdateEntry entry)
        {
            var mainEntry = GetMainEntry((InternalEntityEntry)entry);
            if (_sharedCommands.TryGetValue(mainEntry, out var sharedCommand))
            {
                return sharedCommand;
            }

            sharedCommand = new ModificationCommand(
                _name, _schema, _generateParameterName, _annotationProvider, _sensitiveLoggingEnabled, _comparer);
            _sharedCommands.Add(mainEntry, sharedCommand);

            return sharedCommand;
        }

        private InternalEntityEntry GetMainEntry(InternalEntityEntry entry)
        {
            var entityType = entry.EntityType.RootType();
            if (_rootTypesOrder[entityType] == 0)
            {
                return entry;
            }

            foreach (var foreignKey in entityType.FindForeignKeys(entityType.FindPrimaryKey().Properties))
            {
                if (foreignKey.PrincipalKey.IsPrimaryKey()
                    && _rootTypesOrder.ContainsKey(foreignKey.PrincipalEntityType))
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Validate(bool sensitiveLoggingEnabled)
        {
            foreach (var command in _sharedCommands.Values)
            {
                if ((command.EntityState == EntityState.Added
                     || command.EntityState == EntityState.Deleted)
                    && command.Entries.Count != _rootTypesOrder.Count)
                {
                    var tableName = (string.IsNullOrEmpty(command.Schema) ? "" : command.Schema + ".") + command.TableName;

                    var missingEntityTypes = new HashSet<IEntityType>(_rootTypesOrder.Keys);
                    foreach (var entry in command.Entries)
                    {
                        missingEntityTypes.Remove(entry.EntityType.RootType());
                    }

                    var missingEntityTypesString = "{" + string.Join(", ", missingEntityTypes.Select(p => "'" + p.DisplayName() + "'")) + "}";

                    if (sensitiveLoggingEnabled)
                    {
                        throw new InvalidOperationException(RelationalStrings.SharedRowEntryCountMismatchSensitive(
                            _rootTypesOrder.Count,
                            tableName,
                            command.Entries.Count,
                            command.Entries[0].BuildCurrentValuesString(command.Entries[0].EntityType.FindPrimaryKey().Properties),
                            command.EntityState,
                            missingEntityTypesString));
                    }

                    throw new InvalidOperationException(RelationalStrings.SharedRowEntryCountMismatch(
                        _rootTypesOrder.Count, tableName, command.Entries.Count, command.EntityState, missingEntityTypesString));
                }
            }
        }

        private class EntryComparer : IComparer<IUpdateEntry>
        {
            private readonly IReadOnlyDictionary<IEntityType, int> _rootTypesOrder;

            public EntryComparer(IReadOnlyDictionary<IEntityType, int> rootTypesOrder)
            {
                _rootTypesOrder = rootTypesOrder;
            }

            public int Compare(IUpdateEntry x, IUpdateEntry y)
                => _rootTypesOrder[x.EntityType.RootType()] - _rootTypesOrder[y.EntityType.RootType()];
        }
    }
}
