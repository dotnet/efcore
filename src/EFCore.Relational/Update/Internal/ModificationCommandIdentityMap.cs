// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
    public class ModificationCommandIdentityMap<TKey> : IModificationCommandIdentityMap
    {
        private readonly Dictionary<TKey, ModificationCommand> _sharedCommands;
        private readonly int _entityTypesCount;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ModificationCommandIdentityMap([NotNull] IReadOnlyList<IEntityType> entityTypes)
        {
            _entityTypesCount = entityTypes.Count;
            _sharedCommands = new Dictionary<TKey, ModificationCommand>(
                entityTypes[0].FindPrimaryKey().GetPrincipalKeyValueFactory<TKey>().EqualityComparer);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Add(IUpdateEntry entry, ModificationCommand command)
        {
            var keyValue = entry.EntityType.FindPrimaryKey().GetPrincipalKeyValueFactory<TKey>()
                .CreateFromCurrentValues((InternalEntityEntry)entry);
            _sharedCommands.Add(keyValue, command);
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
                    && command.Entries.Count != _entityTypesCount)
                {
                    var tableName = (string.IsNullOrEmpty(command.Schema) ? "" : command.Schema + ".") + command.TableName;

                    if (sensitiveLoggingEnabled)
                    {
                        throw new InvalidOperationException(RelationalStrings.SharedRowEntryCountMismatchSensitive(
                            _entityTypesCount,
                            tableName,
                            command.Entries.Count,
                            command.Entries[0].BuildCurrentValuesString(command.Entries[0].EntityType.FindPrimaryKey().Properties),
                            command.EntityState));
                    }

                    throw new InvalidOperationException(RelationalStrings.SharedRowEntryCountMismatch(
                        _entityTypesCount, tableName, command.Entries.Count, command.EntityState));
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual  ModificationCommand TryGetCommand(IUpdateEntry entry)
        {
            var keyValue = entry.EntityType.FindPrimaryKey().GetPrincipalKeyValueFactory<TKey>()
                .CreateFromCurrentValues((InternalEntityEntry)entry);
            if (_sharedCommands.TryGetValue(keyValue, out var sharedCommand))
            {
                return sharedCommand;
            }

            return null;
        }
    }
}
