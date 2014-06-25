// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Update
{
    public class CommandBatchPreparer
    {
        private readonly ModificationCommandBatchFactory _modificationCommandBatchFactory;
        private readonly ParameterNameGeneratorFactory _parameterNameGeneratorFactory;
        private readonly GraphFactory _graphFactory;
        private readonly ModificationCommandComparer _modificationCommandComparer;
        
        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected CommandBatchPreparer()
        {
        }

        public CommandBatchPreparer(
            [NotNull] ModificationCommandBatchFactory modificationCommandBatchFactory,
            [NotNull] ParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] GraphFactory graphFactory,
            [NotNull] ModificationCommandComparer modificationCommandComparer)
        {
            Check.NotNull(modificationCommandBatchFactory, "modificationCommandBatchFactory");
            Check.NotNull(parameterNameGeneratorFactory, "parameterNameGeneratorFactory");
            Check.NotNull(graphFactory, "graphFactory");
            Check.NotNull(modificationCommandComparer, "modificationCommandComparer");

            _modificationCommandBatchFactory = modificationCommandBatchFactory;
            _parameterNameGeneratorFactory = parameterNameGeneratorFactory;
            _graphFactory = graphFactory;
            _modificationCommandComparer = modificationCommandComparer;
        }

        public virtual IEnumerable<ModificationCommandBatch> BatchCommands([NotNull] IReadOnlyList<StateEntry> stateEntries)
        {
            Check.NotNull(stateEntries, "stateEntries");

            var modificationCommandGraph = _graphFactory.Create<ModificationCommand>();
            var commands = CreateModificationCommands(stateEntries);

            PopulateModificationCommandGraph(modificationCommandGraph, commands);
            var sortedCommandSets = modificationCommandGraph.TopologicalSort();

            foreach (var independentCommandSet in sortedCommandSets)
            {
                independentCommandSet.Sort(_modificationCommandComparer);
            }

            // TODO: Note that the code below appears to do batching, but it doesn't really do it because
            // it always creates a new batch for each insert, update, or delete operation.
            return sortedCommandSets.SelectMany(mc => mc).Select(mc =>
                {
                    var batch = _modificationCommandBatchFactory.Create();
                    _modificationCommandBatchFactory.AddCommand(batch, mc);
                    return batch;
                });
        }

        protected virtual IEnumerable<ModificationCommand> CreateModificationCommands([NotNull] IReadOnlyList<StateEntry> stateEntries)
        {
            var parameterNameGenerator = _parameterNameGeneratorFactory.Create();
            // TODO: Handle multiple state entries that update the same row
            return stateEntries.Select(e => new ModificationCommand(e.EntityType.StorageName, parameterNameGenerator).AddStateEntry(e));
        }

        // To avoid violating store constraints the modification commands must be sorted
        // according to these rules:
        //
        // 1. Commands adding rows or modifying the candidate key values (when supported) must precede
        //     commands modifying or adding rows that will be referencing the former
        // 2. Commands deleting rows or modifying the foreign key values must precede
        //     commands deleting rows or modifying the candidate key values (when supported) of rows
        //     that are currently being referenced by the former
        protected virtual void PopulateModificationCommandGraph(
            [NotNull] Graph<ModificationCommand> modificationCommandGraph,
            [NotNull] IEnumerable<ModificationCommand> commands)
        {
            Check.NotNull(modificationCommandGraph, "modificationCommandGraph");
            Check.NotNull(commands, "commands");

            modificationCommandGraph.AddVertices(commands);

            // The predecessors map allows to populate the graph in linear time
            var predecessorsMap = CreateKeyValuePredecessorMap(modificationCommandGraph);
            AddForeignKeyEdges(modificationCommandGraph, predecessorsMap);
        }

        // Builds a map from foreign key values to list of modification commands, with an entry for every command
        // that may need to precede some other command involving that foreign key value.
        private Dictionary<KeyValue, List<ModificationCommand>> CreateKeyValuePredecessorMap(Graph<ModificationCommand> commandGraph)
        {
            var predecessorsMap = new Dictionary<KeyValue, List<ModificationCommand>>(new KeyValueComparer());
            foreach (var command in commandGraph.Vertices)
            {
                if (command.EntityState == EntityState.Modified
                    || command.EntityState == EntityState.Added)
                {
                    foreach (var stateEntry in command.StateEntries)
                    {
                        // TODO: Perf: Consider only adding foreign keys defined on entity types involved in a modification
                        foreach (var foreignKey in stateEntry.EntityType.GetReferencingForeignKeys())
                        {
                            var candidateKeyValueColumnModifications =
                                command.ColumnModifications.Where(cm =>
                                    foreignKey.ReferencedProperties.Contains(cm.Property)
                                    && (cm.IsWrite || cm.IsRead));

                            if (command.EntityState == EntityState.Modified
                                && candidateKeyValueColumnModifications.Any(cm => cm.IsWrite))
                            {
                                throw new InvalidOperationException(Strings.FormatPrincipalKeyModified());
                            }

                            if (command.EntityState == EntityState.Added
                                || candidateKeyValueColumnModifications.Any())
                            {
                                var candidateKeyValue = CreatePrincipalKeyValue(stateEntry, foreignKey);

                                List<ModificationCommand> predecessorCommands;
                                if (!predecessorsMap.TryGetValue(candidateKeyValue, out predecessorCommands))
                                {
                                    predecessorCommands = new List<ModificationCommand>();
                                    predecessorsMap.Add(candidateKeyValue, predecessorCommands);
                                }
                                predecessorCommands.Add(command);
                            }
                        }
                    }
                }

                if (command.EntityState == EntityState.Modified
                    || command.EntityState == EntityState.Deleted)
                {
                    foreach (var stateEntry in command.StateEntries)
                    {
                        foreach (var foreignKey in stateEntry.EntityType.ForeignKeys)
                        {
                            var foreignKeyValueColumnModifications =
                                command.ColumnModifications.Where(cm =>
                                    foreignKey.Properties.Contains(cm.Property)
                                    && (cm.IsWrite || cm.IsRead));

                            if (command.EntityState == EntityState.Deleted
                                || foreignKeyValueColumnModifications.Any())
                            {
                                var foreignKeyValue = CreateDependentKeyValue(stateEntry.OriginalValues, foreignKey);

                                List<ModificationCommand> predecessorCommands;
                                if (!predecessorsMap.TryGetValue(foreignKeyValue, out predecessorCommands))
                                {
                                    predecessorCommands = new List<ModificationCommand>();
                                    predecessorsMap.Add(foreignKeyValue, predecessorCommands);
                                }
                                predecessorCommands.Add(command);
                            }
                        }
                    }
                }
            }
            return predecessorsMap;
        }

        private void AddForeignKeyEdges(
            Graph<ModificationCommand> commandGraph,
            Dictionary<KeyValue, List<ModificationCommand>> predecessorsMap)
        {
            foreach (var command in commandGraph.Vertices)
            {
                if (command.EntityState == EntityState.Modified
                    || command.EntityState == EntityState.Added)
                {
                    foreach (var stateEntry in command.StateEntries)
                    {
                        foreach (var foreignKey in stateEntry.EntityType.ForeignKeys)
                        {
                            var foreignKeyValue = CreateDependentKeyValue(stateEntry, foreignKey);

                            List<ModificationCommand> predecessorCommands;
                            if (predecessorsMap.TryGetValue(foreignKeyValue, out predecessorCommands))
                            {
                                foreach (var predecessor in predecessorCommands)
                                {
                                    if (predecessor != command)
                                    {
                                        commandGraph.AddEdge(predecessor, command);
                                    }
                                }
                            }
                        }
                    }
                }

                // TODO: also examine modified entities here when principal key modification is supported
                if (command.EntityState == EntityState.Deleted)
                {
                    foreach (var stateEntry in command.StateEntries)
                    {
                        foreach (var foreignKey in stateEntry.EntityType.GetReferencingForeignKeys())
                        {
                            var candidateKeyValue = CreatePrincipalKeyValue(stateEntry.OriginalValues, foreignKey);

                            List<ModificationCommand> predecessorCommands;
                            if (predecessorsMap.TryGetValue(candidateKeyValue, out predecessorCommands))
                            {
                                foreach (var predecessor in predecessorCommands)
                                {
                                    if (predecessor != command)
                                    {
                                        commandGraph.AddEdge(predecessor, command);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private KeyValue CreatePrincipalKeyValue(IPropertyBagEntry propertyBagEntry, IForeignKey foreignKey)
        {
            var key = propertyBagEntry.GetPrincipalKeyValue(foreignKey);
            return new KeyValue(foreignKey, key, propertyBagEntry.GetType());
        }

        private KeyValue CreateDependentKeyValue(IPropertyBagEntry propertyBagEntry, IForeignKey foreignKey)
        {
            var key = propertyBagEntry.GetDependentKeyValue(foreignKey);
            return new KeyValue(foreignKey, key, propertyBagEntry.GetType());
        }

        private struct KeyValue
        {
            public KeyValue(
                [NotNull] IForeignKey foreignKey, EntityKey keyValue, [NotNull] Type propertyBagEntryType)
            {
                ForeignKey = foreignKey;
                Key = keyValue;
                PropertyBagEntryType = propertyBagEntryType;
            }

            internal readonly IForeignKey ForeignKey;

            internal readonly EntityKey Key;

            internal readonly Type PropertyBagEntryType;
        }

        private class KeyValueComparer : IEqualityComparer<KeyValue>
        {
            public bool Equals(KeyValue x, KeyValue y)
            {
                return x.PropertyBagEntryType == y.PropertyBagEntryType
                       && x.ForeignKey == y.ForeignKey
                       && x.Key.Equals(y.Key);
            }

            public int GetHashCode(KeyValue obj)
            {
                return (((obj.PropertyBagEntryType.GetHashCode() * 397)
                         ^ obj.ForeignKey.GetHashCode()) * 397)
                       ^ obj.Key.GetHashCode();
            }
        }
    }
}
