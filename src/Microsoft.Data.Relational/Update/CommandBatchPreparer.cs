// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Relational.Update
{
    internal class CommandBatchPreparer
    {
        public IEnumerable<ModificationCommandBatch> BatchCommands([NotNull] IEnumerable<StateEntry> stateEntries)
        {
            foreach (var stateEntry in stateEntries)
            {
                var tableName = stateEntry.EntityType.StorageName;

                switch (stateEntry.EntityState)
                {
                    case EntityState.Added:
                        yield return new ModificationCommandBatch(
                            new[] { new ModificationCommand(tableName, GetColumnValues(stateEntry, true), null) });
                        break;
                    case EntityState.Modified:
                        yield return new ModificationCommandBatch(
                            new [] { new ModificationCommand(tableName, GetColumnValues(stateEntry, false), GetWhereClauses(stateEntry))});
                        break;
                    case EntityState.Deleted:
                        yield return new ModificationCommandBatch(
                            new [] { new ModificationCommand(tableName, null, GetWhereClauses(stateEntry))});
                        break;
                }
            }
        }

        private static IEnumerable<KeyValuePair<string, Object>> GetColumnValues([NotNull] StateEntry stateEntry, bool includeKeys)
        {
            var entityType = stateEntry.EntityType;

            return entityType
                    .Properties
                    .Where(p => 
                            p.ValueGenerationStrategy != ValueGenerationStrategy.StoreComputed && 
                            p.ValueGenerationStrategy != ValueGenerationStrategy.StoreIdentity &&
                            (includeKeys || !entityType.Key.Contains(p)))
                    .Select(p => new KeyValuePair<string, object>(p.StorageName, p.GetValue(stateEntry.Entity)));
        }

        private static IEnumerable<KeyValuePair<string, object>> GetWhereClauses([NotNull] StateEntry stateEntry)
        {
            return 
                stateEntry
                    .EntityType
                    .Key
                    .Select(k => new KeyValuePair<string, object>(k.Name, k.GetValue(stateEntry.Entity)));
        }
    }
}
