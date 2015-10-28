// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Update.Internal
{
    public class CommandBatchPreparer : ICommandBatchPreparer
    {
        private readonly IModificationCommandBatchFactory _modificationCommandBatchFactory;
        private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;
        private readonly IComparer<ModificationCommand> _modificationCommandComparer;
        private readonly IRelationalAnnotationProvider _annotationProvider;

        public CommandBatchPreparer(
            [NotNull] IModificationCommandBatchFactory modificationCommandBatchFactory,
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] IComparer<ModificationCommand> modificationCommandComparer,
            [NotNull] IRelationalAnnotationProvider annotations)
        {
            _modificationCommandBatchFactory = modificationCommandBatchFactory;
            _parameterNameGeneratorFactory = parameterNameGeneratorFactory;
            _modificationCommandComparer = modificationCommandComparer;
            _annotationProvider = annotations;
        }

        public virtual IEnumerable<ModificationCommandBatch> BatchCommands(IReadOnlyList<IUpdateEntry> entries)
        {
            var commands = CreateModificationCommands(entries);
            var sortedCommandSets = TopologicalSort(commands);

            // TODO: Enable batching of dependent commands by passing through the dependency graph
            foreach (var independentCommandSet in sortedCommandSets)
            {
                independentCommandSet.Sort(_modificationCommandComparer);

                var batch = _modificationCommandBatchFactory.Create();
                foreach (var modificationCommand in independentCommandSet)
                {
                    if (!batch.AddCommand(modificationCommand))
                    {
                        yield return batch;
                        batch = _modificationCommandBatchFactory.Create();
                        batch.AddCommand(modificationCommand);
                    }
                }

                yield return batch;
            }
        }

        protected virtual IEnumerable<ModificationCommand> CreateModificationCommands([NotNull] IReadOnlyList<IUpdateEntry> entries)
        {
            var parameterNameGenerator = _parameterNameGeneratorFactory.Create();
            // TODO: Handle multiple state entries that update the same row
            return entries.Select(
                e =>
                {
                    var command = new ModificationCommand(
                        _annotationProvider.For(e.EntityType).TableName,
                        _annotationProvider.For(e.EntityType).Schema,
                        parameterNameGenerator,
                        _annotationProvider.For);

                    command.AddEntry(e);
                    return command;
                });
        }

        // To avoid violating store constraints the modification commands must be sorted
        // according to these rules:
        //
        // 1. Commands adding rows or modifying the candidate key values (when supported) must precede
        //     commands modifying or adding rows that will be referencing the former
        // 2. Commands deleting rows or modifying the foreign key values must precede
        //     commands deleting rows or modifying the candidate key values (when supported) of rows
        //     that are currently being referenced by the former
        protected virtual IReadOnlyList<List<ModificationCommand>> TopologicalSort([NotNull] IEnumerable<ModificationCommand> commands)
        {
            var modificationCommandGraph = new Multigraph<ModificationCommand, IForeignKey>();
            modificationCommandGraph.AddVertices(commands);

            // The predecessors map allows to populate the graph in linear time
            var predecessorsMap = CreateKeyValuePredecessorMap(modificationCommandGraph);
            AddForeignKeyEdges(modificationCommandGraph, predecessorsMap);

            var sortedCommands
                = modificationCommandGraph.BatchingTopologicalSort(data => { return string.Join(", ", data.Select(d => d.Item3.First())); });

            return sortedCommands;
        }

        // Builds a map from foreign key values to list of modification commands, with an entry for every command
        // that may need to precede some other command involving that foreign key value.
        private Dictionary<KeyValueIndex, List<ModificationCommand>> CreateKeyValuePredecessorMap(Graph<ModificationCommand> commandGraph)
        {
            var predecessorsMap = new Dictionary<KeyValueIndex, List<ModificationCommand>>(new KeyValueIndexComparer());
            foreach (var command in commandGraph.Vertices)
            {
                if (command.EntityState == EntityState.Modified
                    || command.EntityState == EntityState.Added)
                {
                    foreach (var entry in command.Entries)
                    {
                        // TODO: Perf: Consider only adding foreign keys defined on entity types involved in a modification
                        foreach (var foreignKey in entry.EntityType.GetReferencingForeignKeys())
                        {
                            var candidateKeyValueColumnModifications =
                                command.ColumnModifications.Where(cm =>
                                    foreignKey.PrincipalKey.Properties.Contains(cm.Property)
                                    && (cm.IsWrite || cm.IsRead)).ToList();

                            if (command.EntityState == EntityState.Added
                                || candidateKeyValueColumnModifications.Count != 0)
                            {
                                var principalKeyValue = CreatePrincipalKeyValue(entry, foreignKey, ValueType.Current);

                                if (!principalKeyValue.KeyValue.IsInvalid)
                                {
                                    List<ModificationCommand> predecessorCommands;
                                    if (!predecessorsMap.TryGetValue(principalKeyValue, out predecessorCommands))
                                    {
                                        predecessorCommands = new List<ModificationCommand>();
                                        predecessorsMap.Add(principalKeyValue, predecessorCommands);
                                    }
                                    predecessorCommands.Add(command);
                                }
                            }
                        }
                    }
                }

                if (command.EntityState == EntityState.Modified
                    || command.EntityState == EntityState.Deleted)
                {
                    foreach (var entry in command.Entries)
                    {
                        foreach (var foreignKey in entry.EntityType.GetForeignKeys())
                        {
                            var currentForeignKey = foreignKey;
                            var foreignKeyValueColumnModifications =
                                command.ColumnModifications.Where(cm =>
                                    currentForeignKey.Properties.Contains(cm.Property)
                                    && (cm.IsWrite || cm.IsRead));

                            if (command.EntityState == EntityState.Deleted
                                || foreignKeyValueColumnModifications.Any())
                            {
                                var dependentKeyValue = CreateDependentKeyValue(entry, foreignKey, ValueType.Original);

                                if (!dependentKeyValue.KeyValue.IsInvalid)
                                {
                                    List<ModificationCommand> predecessorCommands;
                                    if (!predecessorsMap.TryGetValue(dependentKeyValue, out predecessorCommands))
                                    {
                                        predecessorCommands = new List<ModificationCommand>();
                                        predecessorsMap.Add(dependentKeyValue, predecessorCommands);
                                    }
                                    predecessorCommands.Add(command);
                                }
                            }
                        }
                    }
                }
            }
            return predecessorsMap;
        }

        private void AddForeignKeyEdges(
            Multigraph<ModificationCommand, IForeignKey> commandGraph,
            Dictionary<KeyValueIndex, List<ModificationCommand>> predecessorsMap)
        {
            foreach (var command in commandGraph.Vertices)
            {
                if (command.EntityState == EntityState.Modified
                    || command.EntityState == EntityState.Added)
                {
                    foreach (var entry in command.Entries)
                    {
                        foreach (var foreignKey in entry.EntityType.GetForeignKeys())
                        {
                            var dependentKeyValue = CreateDependentKeyValue(entry, foreignKey, ValueType.Current);

                            if (!dependentKeyValue.KeyValue.IsInvalid)
                            {
                                List<ModificationCommand> predecessorCommands;
                                if (predecessorsMap.TryGetValue(dependentKeyValue, out predecessorCommands))
                                {
                                    foreach (var predecessor in predecessorCommands)
                                    {
                                        if (predecessor != command)
                                        {
                                            commandGraph.AddEdge(predecessor, command, foreignKey);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // TODO: also examine modified entities here when principal key modification is supported
                if (command.EntityState == EntityState.Deleted)
                {
                    foreach (var entry in command.Entries)
                    {
                        foreach (var foreignKey in entry.EntityType.GetReferencingForeignKeys())
                        {
                            var principalKeyValue = CreatePrincipalKeyValue(entry, foreignKey, ValueType.Original);

                            if (!principalKeyValue.KeyValue.IsInvalid)
                            {
                                List<ModificationCommand> predecessorCommands;
                                if (predecessorsMap.TryGetValue(principalKeyValue, out predecessorCommands))
                                {
                                    foreach (var predecessor in predecessorCommands)
                                    {
                                        if (predecessor != command)
                                        {
                                            commandGraph.AddEdge(predecessor, command, foreignKey);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private KeyValueIndex CreatePrincipalKeyValue(IUpdateEntry entry, IForeignKey foreignKey, ValueType valueType)
            => new KeyValueIndex(foreignKey, entry.GetPrincipalKeyValue(foreignKey, valueType == ValueType.Original), valueType);

        private KeyValueIndex CreateDependentKeyValue(IUpdateEntry entry, IForeignKey foreignKey, ValueType valueType)
            => new KeyValueIndex(foreignKey, entry.GetDependentKeyValue(foreignKey, valueType == ValueType.Original), valueType);

        private enum ValueType
        {
            Original,
            Current
        }

        private struct KeyValueIndex
        {
            public KeyValueIndex([NotNull] IForeignKey foreignKey, IKeyValue keyValueValue, ValueType valueType)
            {
                ForeignKey = foreignKey;
                KeyValue = keyValueValue;
                ValueType = valueType;
            }

            internal readonly IForeignKey ForeignKey;

            internal readonly IKeyValue KeyValue;

            internal readonly ValueType ValueType;
        }

        private class KeyValueIndexComparer : IEqualityComparer<KeyValueIndex>
        {
            public bool Equals(KeyValueIndex x, KeyValueIndex y)
                => x.ValueType == y.ValueType
                   && x.ForeignKey == y.ForeignKey
                   && x.KeyValue.Equals(y.KeyValue);

            public int GetHashCode(KeyValueIndex obj)
                => (((obj.ValueType.GetHashCode() * 397)
                     ^ obj.ForeignKey.GetHashCode()) * 397)
                   ^ obj.KeyValue.GetHashCode();
        }
    }
}
