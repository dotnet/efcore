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
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Update
{
    public class CommandBatchPreparer : ICommandBatchPreparer
    {
        private readonly IModificationCommandBatchFactory _modificationCommandBatchFactory;
        private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;
        private readonly IComparer<ModificationCommand> _modificationCommandComparer;
        private readonly IRelationalValueBufferFactoryFactory _valueBufferFactoryFactory;
        private readonly IRelationalMetadataExtensionProvider _metadataExtensionProvider;

        public CommandBatchPreparer(
            [NotNull] IModificationCommandBatchFactory modificationCommandBatchFactory,
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] IComparer<ModificationCommand> modificationCommandComparer,
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory,
            [NotNull] IRelationalMetadataExtensionProvider metadataExtensions)
        {
            Check.NotNull(modificationCommandBatchFactory, nameof(modificationCommandBatchFactory));
            Check.NotNull(parameterNameGeneratorFactory, nameof(parameterNameGeneratorFactory));
            Check.NotNull(modificationCommandComparer, nameof(modificationCommandComparer));
            Check.NotNull(valueBufferFactoryFactory, nameof(valueBufferFactoryFactory));
            Check.NotNull(metadataExtensions, nameof(metadataExtensions));

            _modificationCommandBatchFactory = modificationCommandBatchFactory;
            _parameterNameGeneratorFactory = parameterNameGeneratorFactory;
            _modificationCommandComparer = modificationCommandComparer;
            _valueBufferFactoryFactory = valueBufferFactoryFactory;
            _metadataExtensionProvider = metadataExtensions;
        }

        public virtual IEnumerable<ModificationCommandBatch> BatchCommands(IReadOnlyList<InternalEntityEntry> entries, IDbContextOptions options)
        {
            Check.NotNull(entries, nameof(entries));

            var commands = CreateModificationCommands(entries);
            var sortedCommandSets = TopologicalSort(commands);

            // TODO: Enable batching of dependent commands by passing through the dependency graph
            foreach (var independentCommandSet in sortedCommandSets)
            {
                independentCommandSet.Sort(_modificationCommandComparer);

                var batch = _modificationCommandBatchFactory.Create(options, _metadataExtensionProvider);
                foreach (var modificationCommand in independentCommandSet)
                {
                    if (!_modificationCommandBatchFactory.AddCommand(batch, modificationCommand))
                    {
                        yield return batch;
                        batch = _modificationCommandBatchFactory.Create(options, _metadataExtensionProvider);
                        _modificationCommandBatchFactory.AddCommand(batch, modificationCommand);
                    }
                }

                yield return batch;
            }
        }

        protected virtual IEnumerable<ModificationCommand> CreateModificationCommands([NotNull] IReadOnlyList<InternalEntityEntry> entries)
        {
            var parameterNameGenerator = _parameterNameGeneratorFactory.Create();
            // TODO: Handle multiple state entries that update the same row
            return entries.Select(
                e => new ModificationCommand(
                    _metadataExtensionProvider.For(e.EntityType).TableName,
                    _metadataExtensionProvider.For(e.EntityType).Schema,
                    parameterNameGenerator,
                    _metadataExtensionProvider.For,
                    _valueBufferFactoryFactory)
                    .AddEntry(e));
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
            Check.NotNull(commands, nameof(commands));

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
        private Dictionary<KeyValue, List<ModificationCommand>> CreateKeyValuePredecessorMap(Graph<ModificationCommand> commandGraph)
        {
            var predecessorsMap = new Dictionary<KeyValue, List<ModificationCommand>>(new KeyValueComparer());
            foreach (var command in commandGraph.Vertices)
            {
                if (command.EntityState == EntityState.Modified
                    || command.EntityState == EntityState.Added)
                {
                    foreach (var entry in command.Entries)
                    {
                        // TODO: Perf: Consider only adding foreign keys defined on entity types involved in a modification
                        foreach (var foreignKey in entry.EntityType.FindReferencingForeignKeys())
                        {
                            var candidateKeyValueColumnModifications =
                                command.ColumnModifications.Where(cm =>
                                    foreignKey.PrincipalKey.Properties.Contains(cm.Property)
                                    && (cm.IsWrite || cm.IsRead)).ToList();

                            if (command.EntityState == EntityState.Added
                                || candidateKeyValueColumnModifications.Count != 0)
                            {
                                var principalKeyValue = CreatePrincipalKeyValue(entry, foreignKey, ValueType.Current);

                                if (principalKeyValue.Key != EntityKey.InvalidEntityKey)
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
                                var dependentKeyValue = CreateDependentKeyValue(entry.OriginalValues, foreignKey, ValueType.Original);

                                if (dependentKeyValue.Key != EntityKey.InvalidEntityKey)
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
            Dictionary<KeyValue, List<ModificationCommand>> predecessorsMap)
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

                            if (dependentKeyValue.Key != EntityKey.InvalidEntityKey)
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
                        foreach (var foreignKey in entry.EntityType.FindReferencingForeignKeys())
                        {
                            var principalKeyValue = CreatePrincipalKeyValue(entry.OriginalValues, foreignKey, ValueType.Original);

                            if (principalKeyValue.Key != EntityKey.InvalidEntityKey)
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

        private KeyValue CreatePrincipalKeyValue(IPropertyAccessor propertyAccessor, IForeignKey foreignKey, ValueType valueType)
            => new KeyValue(foreignKey, propertyAccessor.GetPrincipalKeyValue(foreignKey), valueType);

        private KeyValue CreateDependentKeyValue(IPropertyAccessor propertyAccessor, IForeignKey foreignKey, ValueType valueType)
            => new KeyValue(foreignKey, propertyAccessor.GetDependentKeyValue(foreignKey), valueType);

        private enum ValueType
        {
            Original,
            Current
        }

        private struct KeyValue
        {
            public KeyValue([NotNull] IForeignKey foreignKey, EntityKey keyValue, ValueType valueType)
            {
                ForeignKey = foreignKey;
                Key = keyValue;
                ValueType = valueType;
            }

            internal readonly IForeignKey ForeignKey;

            internal readonly EntityKey Key;

            internal readonly ValueType ValueType;
        }

        private class KeyValueComparer : IEqualityComparer<KeyValue>
        {
            public bool Equals(KeyValue x, KeyValue y)
                => x.ValueType == y.ValueType
                   && x.ForeignKey == y.ForeignKey
                   && x.Key.Equals(y.Key);

            public int GetHashCode(KeyValue obj)
                => (((obj.ValueType.GetHashCode() * 397)
                     ^ obj.ForeignKey.GetHashCode()) * 397)
                   ^ obj.Key.GetHashCode();
        }
    }
}
