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
        private readonly IReadOnlyDictionary<IEntityType, IReadOnlyList<IEntityType>> _principals;
        private readonly IReadOnlyDictionary<IEntityType, IReadOnlyList<IEntityType>> _dependents;
        private readonly string _name;
        private readonly string _schema;
        private readonly Func<string> _generateParameterName;
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
            [NotNull] IReadOnlyDictionary<IEntityType, IReadOnlyList<IEntityType>> principals,
            [NotNull] IReadOnlyDictionary<IEntityType, IReadOnlyList<IEntityType>> dependents,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] Func<string> generateParameterName,
            bool sensitiveLoggingEnabled)
        {
            _stateManager = stateManager;
            _principals = principals;
            _dependents = dependents;
            _name = name;
            _schema = schema;
            _generateParameterName = generateParameterName;
            _sensitiveLoggingEnabled = sensitiveLoggingEnabled;
            _comparer = new EntryComparer(principals);
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
                _name, _schema, _generateParameterName, _sensitiveLoggingEnabled, _comparer);
            _sharedCommands.Add(mainEntry, sharedCommand);

            return sharedCommand;
        }

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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Validate(bool sensitiveLoggingEnabled)
        {
            foreach (var command in _sharedCommands.Values)
            {
                if ((command.EntityState != EntityState.Added
                     && command.EntityState != EntityState.Deleted)
                    || (command.Entries.Any(e => _principals[e.EntityType].Count == 0)
                        && command.Entries.Any(e => _dependents[e.EntityType].Count == 0)))
                {
                    continue;
                }

                var tableName = (string.IsNullOrEmpty(command.Schema) ? "" : command.Schema + ".") + command.TableName;
                foreach (var entry in command.Entries)
                {
                    foreach (var principalEntityType in _principals[entry.EntityType])
                    {
                        if (!command.Entries.Any(principalEntry => principalEntry != entry
                                                                   && principalEntityType.IsAssignableFrom(principalEntry.EntityType)))
                        {
                            if (sensitiveLoggingEnabled)
                            {
                                throw new InvalidOperationException(RelationalStrings.SharedRowEntryCountMismatchSensitive(
                                    entry.EntityType.DisplayName(),
                                    tableName,
                                    principalEntityType.DisplayName(),
                                    entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey().Properties),
                                    command.EntityState));
                            }

                            throw new InvalidOperationException(RelationalStrings.SharedRowEntryCountMismatch(
                                entry.EntityType.DisplayName(),
                                tableName,
                                principalEntityType.DisplayName(),
                                command.EntityState));
                        }
                    }
                    
                    foreach (var dependentEntityType in _dependents[entry.EntityType])
                    {
                        if (!command.Entries.Any(dependentEntry => dependentEntry != entry
                                                                   && dependentEntityType.IsAssignableFrom(dependentEntry.EntityType)))
                        {
                            if (sensitiveLoggingEnabled)
                            {
                                throw new InvalidOperationException(RelationalStrings.SharedRowEntryCountMismatchSensitive(
                                    entry.EntityType.DisplayName(),
                                    tableName,
                                    dependentEntityType.DisplayName(),
                                    entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey().Properties),
                                    command.EntityState));
                            }

                            throw new InvalidOperationException(RelationalStrings.SharedRowEntryCountMismatch(
                                entry.EntityType.DisplayName(),
                                tableName,
                                dependentEntityType.DisplayName(),
                                command.EntityState));
                        }
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

                if (_principals[y.EntityType].Count == 0)
                {
                    return 1;
                }

                return StringComparer.Ordinal.Compare(x.EntityType.Name, y.EntityType.Name);
            }
        }
    }
}
