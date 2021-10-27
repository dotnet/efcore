// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CommandBatchPreparer : ICommandBatchPreparer
    {
        private readonly int _minBatchSize;
        private readonly bool _sensitiveLoggingEnabled;
        private readonly Multigraph<IReadOnlyModificationCommand, IAnnotatable> _modificationCommandGraph = new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CommandBatchPreparer(CommandBatchPreparerDependencies dependencies)
        {
            _minBatchSize =
                dependencies.Options.Extensions.OfType<RelationalOptionsExtension>().FirstOrDefault()?.MinBatchSize
                ?? 4;
            Dependencies = dependencies;

            if (dependencies.LoggingOptions.IsSensitiveDataLoggingEnabled)
            {
                _sensitiveLoggingEnabled = true;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual CommandBatchPreparerDependencies Dependencies { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ModificationCommandBatch> BatchCommands(
            IList<IUpdateEntry> entries,
            IUpdateAdapter updateAdapter)
        {
            var parameterNameGenerator = Dependencies.ParameterNameGeneratorFactory.Create();
            var commands = CreateModificationCommands(entries, updateAdapter, parameterNameGenerator.GenerateNext);
            var sortedCommandSets = TopologicalSort(commands);

            foreach (var independentCommandSet in sortedCommandSets)
            {
                independentCommandSet.Sort(Dependencies.ModificationCommandComparer);

                var batch = Dependencies.ModificationCommandBatchFactory.Create();
                foreach (var modificationCommand in independentCommandSet)
                {
                    (modificationCommand as ModificationCommand)?.AssertColumnsNotInitialized();
                    if (modificationCommand.EntityState == EntityState.Modified
                        && !modificationCommand.ColumnModifications.Any(m => m.IsWrite))
                    {
                        continue;
                    }

                    if (!batch.AddCommand(modificationCommand))
                    {
                        if (batch.ModificationCommands.Count == 1
                            || batch.ModificationCommands.Count >= _minBatchSize)
                        {
                            if (batch.ModificationCommands.Count > 1)
                            {
                                Dependencies.UpdateLogger.BatchReadyForExecution(
                                    batch.ModificationCommands.SelectMany(c => c.Entries), batch.ModificationCommands.Count);
                            }

                            yield return batch;
                        }
                        else
                        {
                            Dependencies.UpdateLogger.BatchSmallerThanMinBatchSize(
                                batch.ModificationCommands.SelectMany(c => c.Entries), batch.ModificationCommands.Count, _minBatchSize);

                            foreach (var command in batch.ModificationCommands)
                            {
                                yield return StartNewBatch(parameterNameGenerator, command);
                            }
                        }

                        batch = StartNewBatch(parameterNameGenerator, modificationCommand);
                    }
                }

                if (batch.ModificationCommands.Count == 1
                    || batch.ModificationCommands.Count >= _minBatchSize)
                {
                    if (batch.ModificationCommands.Count > 1)
                    {
                        Dependencies.UpdateLogger.BatchReadyForExecution(
                            batch.ModificationCommands.SelectMany(c => c.Entries), batch.ModificationCommands.Count);
                    }

                    yield return batch;
                }
                else
                {
                    Dependencies.UpdateLogger.BatchSmallerThanMinBatchSize(
                        batch.ModificationCommands.SelectMany(c => c.Entries), batch.ModificationCommands.Count, _minBatchSize);

                    foreach (var command in batch.ModificationCommands)
                    {
                        yield return StartNewBatch(parameterNameGenerator, command);
                    }
                }
            }
        }

        private ModificationCommandBatch StartNewBatch(
            ParameterNameGenerator parameterNameGenerator,
            IReadOnlyModificationCommand modificationCommand)
        {
            parameterNameGenerator.Reset();
            var batch = Dependencies.ModificationCommandBatchFactory.Create();
            batch.AddCommand(modificationCommand);
            return batch;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEnumerable<IReadOnlyModificationCommand> CreateModificationCommands(
            IList<IUpdateEntry> entries,
            IUpdateAdapter updateAdapter,
            Func<string> generateParameterName)
        {
            var commands = new List<IModificationCommand>();
            Dictionary<(string Name, string? Schema), SharedTableEntryMap<IModificationCommand>>? sharedTablesCommandsMap =
                null;
            foreach (var entry in entries)
            {
                if (entry.SharedIdentityEntry != null
                    && entry.EntityState == EntityState.Deleted)
                {
                    continue;
                }

                var mappings = (IReadOnlyCollection<ITableMapping>)entry.EntityType.GetTableMappings();
                var mappingCount = mappings.Count;
                IModificationCommand? firstCommands = null;
                foreach (var mapping in mappings)
                {
                    var table = mapping.Table;

                    IModificationCommand command;
                    var isMainEntry = true;
                    if (table.IsShared)
                    {
                        if (sharedTablesCommandsMap == null)
                        {
                            sharedTablesCommandsMap = new Dictionary<(string, string?), SharedTableEntryMap<IModificationCommand>>();
                        }

                        var tableKey = (table.Name, table.Schema);
                        if (!sharedTablesCommandsMap.TryGetValue(tableKey, out var sharedCommandsMap))
                        {
                            sharedCommandsMap = new SharedTableEntryMap<IModificationCommand>(table, updateAdapter);
                            sharedTablesCommandsMap.Add(tableKey, sharedCommandsMap);
                        }

                        command = sharedCommandsMap.GetOrAddValue(
                            entry,
                            (n, s, comparer) => Dependencies.ModificationCommandFactory.CreateModificationCommand(
                                new ModificationCommandParameters(
                                    n, s, _sensitiveLoggingEnabled, comparer, generateParameterName, Dependencies.UpdateLogger)));
                        isMainEntry = sharedCommandsMap.IsMainEntry(entry);
                    }
                    else
                    {
                        command = Dependencies.ModificationCommandFactory.CreateModificationCommand(
                            new ModificationCommandParameters(
                                table.Name, table.Schema, _sensitiveLoggingEnabled, comparer: null, generateParameterName,
                                Dependencies.UpdateLogger));
                    }

                    command.AddEntry(entry, isMainEntry);
                    commands.Add(command);

                    if (firstCommands == null)
                    {
                        Check.DebugAssert(firstCommands == null, "firstCommand == null");
                        firstCommands = command;
                    }
                }

                if (firstCommands == null)
                {
                    throw new InvalidOperationException(RelationalStrings.ReadonlyEntitySaved(entry.EntityType.DisplayName()));
                }
            }

            if (sharedTablesCommandsMap != null)
            {
                AddUnchangedSharingEntries(sharedTablesCommandsMap.Values, entries);
            }

            return commands;
        }

        private void AddUnchangedSharingEntries(
            IEnumerable<SharedTableEntryMap<IModificationCommand>> sharedTablesCommands,
            IList<IUpdateEntry> entries)
        {
            foreach (var sharedCommandsMap in sharedTablesCommands)
            {
                foreach (var command in sharedCommandsMap.Values)
                {
                    if (command.EntityState != EntityState.Modified)
                    {
                        continue;
                    }

                    foreach (var entry in sharedCommandsMap.GetAllEntries(command.Entries[0]))
                    {
                        if (entry.EntityState != EntityState.Unchanged)
                        {
                            continue;
                        }

                        entry.EntityState = EntityState.Modified;

                        command.AddEntry(entry, sharedCommandsMap.IsMainEntry(entry));
                        entries.Add(entry);
                    }
                }
            }
        }

        // To avoid violating store constraints the modification commands must be sorted
        // according to these rules:
        //
        // 1. Commands adding rows or modifying the candidate key values (when supported) must precede
        //     commands adding or modifying rows that will be referencing the former
        // 2. Commands deleting rows or modifying the foreign key values must precede
        //     commands deleting rows or modifying the candidate key values (when supported) of rows
        //     that are currently being referenced by the former
        // 3. Commands deleting rows or modifying unique constraint values must precede
        //     commands adding or modifying unique constraint values to the same values
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IReadOnlyList<List<IReadOnlyModificationCommand>> TopologicalSort(
            IEnumerable<IReadOnlyModificationCommand> commands)
        {
            _modificationCommandGraph.Clear();
            _modificationCommandGraph.AddVertices(commands);

            // The predecessors map allows to populate the graph in linear time
            var predecessorsMap = CreateKeyValuePredecessorMap(_modificationCommandGraph);
            AddForeignKeyEdges(_modificationCommandGraph, predecessorsMap);

            AddUniqueValueEdges(_modificationCommandGraph);

            AddSameTableEdges(_modificationCommandGraph);

            return _modificationCommandGraph.BatchingTopologicalSort(static (_, _, edges) => edges.All(e => e is IEntityType), FormatCycle);
        }

        private string FormatCycle(
            IReadOnlyList<Tuple<IReadOnlyModificationCommand, IReadOnlyModificationCommand, IEnumerable<IAnnotatable>>> data)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < data.Count; i++)
            {
                var edge = data[i];
                Format(edge.Item1, builder);

                switch (edge.Item3.First())
                {
                    case IForeignKey foreignKey:
                        Format(foreignKey, edge.Item1, edge.Item2, builder);
                        break;
                    case IIndex index:
                        Format(index, edge.Item1, edge.Item2, builder);
                        break;
                }

                if (i == data.Count - 1)
                {
                    Format(edge.Item2, builder);
                }
            }

            if (!_sensitiveLoggingEnabled)
            {
                builder.Append(CoreStrings.SensitiveDataDisabled);
            }

            return builder.ToString();
        }

        private void Format(IReadOnlyModificationCommand command, StringBuilder builder)
        {
            var entry = command.Entries.First();
            var entityType = entry.EntityType;
            builder.Append(entityType.DisplayName());
            if (_sensitiveLoggingEnabled)
            {
                builder.Append(" { ");
                var properties = entityType.FindPrimaryKey()!.Properties;
                for (var i = 0; i < properties.Count; i++)
                {
                    var keyProperty = properties[i];
                    builder.Append('\'');
                    builder.Append(keyProperty.Name);
                    builder.Append("': ");
                    builder.Append(entry.GetCurrentValue(keyProperty));

                    if (i != properties.Count - 1)
                    {
                        builder.Append(", ");
                    }
                }

                builder.Append(" } ");
            }
            else
            {
                builder.Append(' ');
            }

            builder.Append('[');
            builder.Append(entry.EntityState);
            builder.Append(']');
        }

        private void Format(
            IForeignKey foreignKey,
            IReadOnlyModificationCommand source,
            IReadOnlyModificationCommand target,
            StringBuilder builder)
        {
            var reverseDependency = !source.Entries.Any(e => foreignKey.DeclaringEntityType.IsAssignableFrom(e.EntityType));
            if (reverseDependency)
            {
                builder.AppendLine(" <-");
            }
            else
            {
                builder.Append(' ');
            }

            if (foreignKey.DependentToPrincipal != null
                || foreignKey.PrincipalToDependent != null)
            {
                if (!reverseDependency
                    && foreignKey.DependentToPrincipal != null)
                {
                    builder.Append(foreignKey.DependentToPrincipal.Name);
                    builder.Append(' ');
                }

                if (foreignKey.PrincipalToDependent != null)
                {
                    builder.Append(foreignKey.PrincipalToDependent.Name);
                    builder.Append(' ');
                }

                if (reverseDependency
                    && foreignKey.DependentToPrincipal != null)
                {
                    builder.Append(foreignKey.DependentToPrincipal.Name);
                    builder.Append(' ');
                }
            }
            else
            {
                builder.Append("ForeignKey ");
            }

            var dependentCommand = reverseDependency ? target : source;
            var dependentEntry = dependentCommand.Entries.First(e => foreignKey.DeclaringEntityType.IsAssignableFrom(e.EntityType));
            builder.Append("{ ");
            for (var i = 0; i < foreignKey.Properties.Count; i++)
            {
                var property = foreignKey.Properties[i];
                builder.Append('\'');
                builder.Append(property.Name);
                builder.Append('\'');
                if (_sensitiveLoggingEnabled)
                {
                    builder.Append(": ");
                    builder.Append(dependentEntry.GetCurrentValue(property));
                }

                if (i != foreignKey.Properties.Count - 1)
                {
                    builder.Append(", ");
                }
            }

            builder.Append(" } ");

            if (!reverseDependency)
            {
                builder.AppendLine("<-");
            }
        }

        private void Format(IIndex index, IReadOnlyModificationCommand source, IReadOnlyModificationCommand target, StringBuilder builder)
        {
            var reverseDependency = source.EntityState != EntityState.Deleted;
            if (reverseDependency)
            {
                builder.AppendLine(" <-");
            }
            else
            {
                builder.Append(' ');
            }

            builder.Append("Index ");

            var dependentCommand = reverseDependency ? target : source;
            var dependentEntry = dependentCommand.Entries.First(e => index.DeclaringEntityType.IsAssignableFrom(e.EntityType));
            builder.Append("{ ");
            for (var i = 0; i < index.Properties.Count; i++)
            {
                var property = index.Properties[i];
                builder.Append('\'');
                builder.Append(property.Name);
                builder.Append('\'');
                if (_sensitiveLoggingEnabled)
                {
                    builder.Append(": ");
                    builder.Append(dependentEntry.GetCurrentValue(property));
                }

                if (i != index.Properties.Count - 1)
                {
                    builder.Append(", ");
                }
            }

            builder.Append(" } ");

            if (!reverseDependency)
            {
                builder.AppendLine("<-");
            }
        }

        // Builds a map from foreign key values to list of modification commands, with an entry for every command
        // that may need to precede some other command involving that foreign key value.
        private Dictionary<IKeyValueIndex, List<IReadOnlyModificationCommand>> CreateKeyValuePredecessorMap(
            Multigraph<IReadOnlyModificationCommand, IAnnotatable> commandGraph)
        {
            var predecessorsMap = new Dictionary<IKeyValueIndex, List<IReadOnlyModificationCommand>>();
            foreach (var command in commandGraph.Vertices)
            {
                if (command.EntityState == EntityState.Modified
                    || command.EntityState == EntityState.Added)
                {
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var i = 0; i < command.Entries.Count; i++)
                    {
                        var entry = command.Entries[i];
                        foreach (var foreignKey in entry.EntityType.GetReferencingForeignKeys())
                        {
                            if (!IsMapped(foreignKey, command, principal: true)
                                || !IsModified(foreignKey.PrincipalKey.Properties, entry))
                            {
                                continue;
                            }

                            var principalKeyValue = Dependencies.KeyValueIndexFactorySource
                                .GetKeyValueIndexFactory(foreignKey.PrincipalKey)
                                .CreatePrincipalKeyValue(entry, foreignKey);

                            if (principalKeyValue != null)
                            {
                                if (!predecessorsMap.TryGetValue(principalKeyValue, out var predecessorCommands))
                                {
                                    predecessorCommands = new List<IReadOnlyModificationCommand>();
                                    predecessorsMap.Add(principalKeyValue, predecessorCommands);
                                }

                                predecessorCommands.Add(command);
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
                            if (!IsMapped(foreignKey, command, principal: false)
                                || !IsModified(foreignKey.Properties, entry))
                            {
                                continue;
                            }

                            var dependentKeyValue = Dependencies.KeyValueIndexFactorySource
                                .GetKeyValueIndexFactory(foreignKey.PrincipalKey)
                                .CreateDependentKeyValueFromOriginalValues(entry, foreignKey);

                            if (dependentKeyValue != null)
                            {
                                if (!predecessorsMap.TryGetValue(dependentKeyValue, out var predecessorCommands))
                                {
                                    predecessorCommands = new List<IReadOnlyModificationCommand>();
                                    predecessorsMap.Add(dependentKeyValue, predecessorCommands);
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
            Multigraph<IReadOnlyModificationCommand, IAnnotatable> commandGraph,
            Dictionary<IKeyValueIndex, List<IReadOnlyModificationCommand>> predecessorsMap)
        {
            foreach (var command in commandGraph.Vertices)
            {
                switch (command.EntityState)
                {
                    case EntityState.Modified:
                    case EntityState.Added:
                        // ReSharper disable once ForCanBeConvertedToForeach
                        for (var entryIndex = 0; entryIndex < command.Entries.Count; entryIndex++)
                        {
                            var entry = command.Entries[entryIndex];
                            foreach (var foreignKey in entry.EntityType.GetForeignKeys())
                            {
                                if (!IsMapped(foreignKey, command, principal: false)
                                    || !IsModified(foreignKey.Properties, entry))
                                {
                                    continue;
                                }

                                var dependentKeyValue = Dependencies.KeyValueIndexFactorySource
                                    .GetKeyValueIndexFactory(foreignKey.PrincipalKey)
                                    .CreateDependentKeyValue(entry, foreignKey);
                                if (dependentKeyValue == null)
                                {
                                    continue;
                                }

                                AddMatchingPredecessorEdge(
                                    predecessorsMap, dependentKeyValue, commandGraph, command, foreignKey);
                            }
                        }

                        break;
                    case EntityState.Deleted:
                        // ReSharper disable once ForCanBeConvertedToForeach
                        for (var entryIndex = 0; entryIndex < command.Entries.Count; entryIndex++)
                        {
                            var entry = command.Entries[entryIndex];
                            foreach (var foreignKey in entry.EntityType.GetReferencingForeignKeys())
                            {
                                if (!IsMapped(foreignKey, command, principal: true))
                                {
                                    continue;
                                }

                                var principalKeyValue = Dependencies.KeyValueIndexFactorySource
                                    .GetKeyValueIndexFactory(foreignKey.PrincipalKey)
                                    .CreatePrincipalKeyValueFromOriginalValues(entry, foreignKey);
                                if (principalKeyValue != null)
                                {
                                    AddMatchingPredecessorEdge(
                                        predecessorsMap, principalKeyValue, commandGraph, command, foreignKey);
                                }
                            }
                        }

                        break;
                }
            }
        }

        private static bool IsMapped(IForeignKey foreignKey, IReadOnlyModificationCommand command, bool principal)
        {
            foreach (var constraint in foreignKey.GetMappedConstraints())
            {
                if (principal)
                {
                    if (constraint.PrincipalTable.Name == command.TableName
                        && constraint.PrincipalTable.Schema == command.Schema)
                    {
                        return true;
                    }
                }
                else
                {
                    if (constraint.Table.Name == command.TableName
                        && constraint.Table.Schema == command.Schema)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsModified(IReadOnlyList<IProperty> properties, IUpdateEntry entry)
        {
            if (entry.EntityState != EntityState.Modified)
            {
                return true;
            }

            foreach (var property in properties)
            {
                if (entry.IsModified(property))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AddMatchingPredecessorEdge<T>(
            Dictionary<T, List<IReadOnlyModificationCommand>> predecessorsMap,
            T keyValue,
            Multigraph<IReadOnlyModificationCommand, IAnnotatable> commandGraph,
            IReadOnlyModificationCommand command,
            IAnnotatable edge)
            where T : notnull
        {
            if (predecessorsMap.TryGetValue(keyValue, out var predecessorCommands))
            {
                foreach (var predecessor in predecessorCommands)
                {
                    if (predecessor != command)
                    {
                        commandGraph.AddEdge(predecessor, command, edge);
                    }
                }
            }
        }

        private void AddUniqueValueEdges(Multigraph<IReadOnlyModificationCommand, IAnnotatable> commandGraph)
        {
            Dictionary<IIndex, Dictionary<object[], IReadOnlyModificationCommand>>? indexPredecessorsMap = null;
            var keyPredecessorsMap = new Dictionary<(IKey, IKeyValueIndex), List<IReadOnlyModificationCommand>>();
            foreach (var command in commandGraph.Vertices)
            {
                if (command.EntityState != EntityState.Modified
                    && command.EntityState != EntityState.Deleted)
                {
                    continue;
                }

                for (var entryIndex = 0; entryIndex < command.Entries.Count; entryIndex++)
                {
                    var entry = command.Entries[entryIndex];
                    foreach (var index in entry.EntityType.GetIndexes())
                    {
                        if (!index.IsUnique
                            || !index.GetMappedTableIndexes().Any()
                            || !IsModified(index.Properties, entry))
                        {
                            continue;
                        }

                        var valueFactory = index.GetNullableValueFactory<object[]>();
                        if (valueFactory.TryCreateFromOriginalValues(entry, out var indexValue))
                        {
                            indexPredecessorsMap ??= new Dictionary<IIndex, Dictionary<object[], IReadOnlyModificationCommand>>();
                            if (!indexPredecessorsMap.TryGetValue(index, out var predecessorCommands))
                            {
                                predecessorCommands = new Dictionary<object[], IReadOnlyModificationCommand>(valueFactory.EqualityComparer);
                                indexPredecessorsMap.Add(index, predecessorCommands);
                            }

                            if (!predecessorCommands.ContainsKey(indexValue))
                            {
                                predecessorCommands.Add(indexValue, command);
                            }
                        }
                    }

                    if (command.EntityState != EntityState.Deleted)
                    {
                        continue;
                    }

                    foreach (var key in entry.EntityType.GetKeys())
                    {
                        if (!key.GetMappedConstraints().Any())
                        {
                            continue;
                        }

                        var principalKeyValue = Dependencies.KeyValueIndexFactorySource
                            .GetKeyValueIndexFactory(key)
                            .CreatePrincipalKeyValue(entry, null);

                        if (principalKeyValue != null)
                        {
                            if (!keyPredecessorsMap.TryGetValue((key, principalKeyValue), out var predecessorCommands))
                            {
                                predecessorCommands = new List<IReadOnlyModificationCommand>();
                                keyPredecessorsMap.Add((key, principalKeyValue), predecessorCommands);
                            }

                            predecessorCommands.Add(command);
                        }
                    }
                }
            }

            if (indexPredecessorsMap != null)
            {
                foreach (var command in commandGraph.Vertices)
                {
                    if (command.EntityState == EntityState.Deleted)
                    {
                        continue;
                    }

                    foreach (var entry in command.Entries)
                    {
                        foreach (var index in entry.EntityType.GetIndexes())
                        {
                            if (!index.IsUnique
                                || !index.GetMappedTableIndexes().Any()
                                || !IsModified(index.Properties, entry))
                            {
                                continue;
                            }

                            var valueFactory = index.GetNullableValueFactory<object[]>();
                            if (valueFactory.TryCreateFromCurrentValues(entry, out var indexValue)
                                && indexPredecessorsMap.TryGetValue(index, out var predecessorCommands)
                                && predecessorCommands.TryGetValue(indexValue, out var predecessor)
                                && predecessor != command)
                            {
                                commandGraph.AddEdge(predecessor, command, index);
                            }
                        }
                    }
                }
            }

            if (keyPredecessorsMap != null)
            {
                foreach (var command in commandGraph.Vertices)
                {
                    if (command.EntityState != EntityState.Added)
                    {
                        continue;
                    }

                    foreach (var entry in command.Entries)
                    {
                        foreach (var key in entry.EntityType.GetKeys())
                        {
                            if (!key.GetMappedConstraints().Any())
                            {
                                continue;
                            }

                            var principalKeyValue = Dependencies.KeyValueIndexFactorySource
                                .GetKeyValueIndexFactory(key)
                                .CreatePrincipalKeyValue(entry, null);

                            if (principalKeyValue != null)
                            {
                                AddMatchingPredecessorEdge(
                                    keyPredecessorsMap, (key, principalKeyValue), commandGraph, command, key);
                            }
                        }
                    }
                }
            }
        }

        private static void AddSameTableEdges(Multigraph<IReadOnlyModificationCommand, IAnnotatable> modificationCommandGraph)
        {
            var deletedDictionary = new Dictionary<(string, string?), (List<IReadOnlyModificationCommand> List, bool EdgesAdded)>();

            foreach (var command in modificationCommandGraph.Vertices)
            {
                if (command.EntityState == EntityState.Deleted)
                {
                    var table = (command.TableName, command.Schema);
                    if (!deletedDictionary.TryGetValue(table, out var deletedCommands))
                    {
                        deletedCommands = (new List<IReadOnlyModificationCommand>(), false);
                        deletedDictionary.Add(table, deletedCommands);
                    }

                    deletedCommands.List.Add(command);
                }
            }

            foreach (var command in modificationCommandGraph.Vertices)
            {
                if (command.EntityState == EntityState.Added)
                {
                    var table = (command.TableName, command.Schema);
                    if (deletedDictionary.TryGetValue(table, out var deletedCommands))
                    {
                        var lastDelete = deletedCommands.List[^1];
                        if (!deletedCommands.EdgesAdded)
                        {
                            for (var i = 0; i < deletedCommands.List.Count - 1; i++)
                            {
                                var deleted = deletedCommands.List[i];
                                modificationCommandGraph.AddEdge(deleted, lastDelete, deleted.Entries[0].EntityType);
                            }

                            deletedDictionary[table] = (deletedCommands.List, true);
                        }

                        modificationCommandGraph.AddEdge(lastDelete, command, command.Entries[0].EntityType);
                    }
                }
            }
        }
    }
}
