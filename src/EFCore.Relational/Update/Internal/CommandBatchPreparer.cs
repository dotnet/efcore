// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Update.Internal;

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
            ?? 1;
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
    public virtual IEnumerable<(ModificationCommandBatch Batch, bool HasMore)> BatchCommands(
        IList<IUpdateEntry> entries,
        IUpdateAdapter updateAdapter)
    {
        var parameterNameGenerator = Dependencies.ParameterNameGeneratorFactory.Create();
        var commands = CreateModificationCommands(entries, updateAdapter, parameterNameGenerator.GenerateNext);
        var sortedCommandSets = TopologicalSort(commands);

        for (var commandSetIndex = 0; commandSetIndex < sortedCommandSets.Count; commandSetIndex++)
        {
            var independentCommandSet = sortedCommandSets[commandSetIndex];

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

                if (!batch.TryAddCommand(modificationCommand))
                {
                    if (batch.ModificationCommands.Count == 1
                        || batch.ModificationCommands.Count >= _minBatchSize)
                    {
                        if (batch.ModificationCommands.Count > 1)
                        {
                            Dependencies.UpdateLogger.BatchReadyForExecution(
                                batch.ModificationCommands.SelectMany(c => c.Entries), batch.ModificationCommands.Count);
                        }

                        batch.Complete();

                        yield return (batch, true);
                    }
                    else
                    {
                        Dependencies.UpdateLogger.BatchSmallerThanMinBatchSize(
                            batch.ModificationCommands.SelectMany(c => c.Entries), batch.ModificationCommands.Count, _minBatchSize);

                        foreach (var command in batch.ModificationCommands)
                        {
                            batch = StartNewBatch(parameterNameGenerator, command);
                            batch.Complete();

                            yield return (batch, true);
                        }
                    }

                    batch = StartNewBatch(parameterNameGenerator, modificationCommand);
                }
            }

            var hasMoreCommandSets = commandSetIndex < sortedCommandSets.Count - 1;

            if (batch.ModificationCommands.Count == 1
                || batch.ModificationCommands.Count >= _minBatchSize)
            {
                if (batch.ModificationCommands.Count > 1)
                {
                    Dependencies.UpdateLogger.BatchReadyForExecution(
                        batch.ModificationCommands.SelectMany(c => c.Entries), batch.ModificationCommands.Count);
                }

                batch.Complete();

                yield return (batch, hasMoreCommandSets);
            }
            else
            {
                Dependencies.UpdateLogger.BatchSmallerThanMinBatchSize(
                    batch.ModificationCommands.SelectMany(c => c.Entries), batch.ModificationCommands.Count, _minBatchSize);

                for (var commandIndex = 0; commandIndex < batch.ModificationCommands.Count; commandIndex++)
                {
                    var singleCommandBatch = StartNewBatch(parameterNameGenerator, batch.ModificationCommands[commandIndex]);
                    singleCommandBatch.Complete();

                    yield return (singleCommandBatch, hasMoreCommandSets || commandIndex < batch.ModificationCommands.Count - 1);
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
        batch.TryAddCommand(modificationCommand);
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

            var mappings = entry.EntityType.GetTableMappings();
            IModificationCommand? firstCommands = null;
            foreach (var mapping in mappings)
            {
                var table = mapping.Table;

                IModificationCommand command;
                var isMainEntry = true;
                if (table.IsShared)
                {
                    sharedTablesCommandsMap ??= new Dictionary<(string, string?), SharedTableEntryMap<IModificationCommand>>();

                    var tableKey = (table.Name, table.Schema);
                    if (!sharedTablesCommandsMap.TryGetValue(tableKey, out var sharedCommandsMap))
                    {
                        sharedCommandsMap = new SharedTableEntryMap<IModificationCommand>(table, updateAdapter);
                        sharedTablesCommandsMap.Add(tableKey, sharedCommandsMap);
                    }

                    command = sharedCommandsMap.GetOrAddValue(
                        entry,
                        (t, comparer) => Dependencies.ModificationCommandFactory.CreateModificationCommand(
                            new ModificationCommandParameters(
                                t, _sensitiveLoggingEnabled, comparer, generateParameterName, Dependencies.UpdateLogger)));
                    isMainEntry = sharedCommandsMap.IsMainEntry(entry);
                }
                else
                {
                    command = Dependencies.ModificationCommandFactory.CreateModificationCommand(
                        new ModificationCommandParameters(
                            table, _sensitiveLoggingEnabled, comparer: null, generateParameterName,
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

    private static void AddUnchangedSharingEntries(
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

        AddForeignKeyEdges(_modificationCommandGraph);

        AddUniqueValueEdges(_modificationCommandGraph);

        AddSameTableEdges(_modificationCommandGraph);

        return _modificationCommandGraph.BatchingTopologicalSort(static (_, _, edges) => edges.All(e => e is ITable), FormatCycle);
    }

    private string FormatCycle(
        IReadOnlyList<Tuple<IReadOnlyModificationCommand, IReadOnlyModificationCommand, IEnumerable<IAnnotatable>>> data)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < data.Count; i++)
        {
            var (command1, command2, annotatables) = data[i];
            Format(command1, builder);

            switch (annotatables.First())
            {
                case IForeignKeyConstraint foreignKey:
                    Format(foreignKey, command1, command2, builder);
                    break;
                case IUniqueConstraint key:
                    Format(key, command1, command2, builder);
                    break;
                case ITableIndex index:
                    Format(index, command1, command2, builder);
                    break;
                default:
                    builder.AppendLine(" <-");
                    break;
            }

            if (i == data.Count - 1)
            {
                Format(command2, builder);
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
        IForeignKeyConstraint foreignKey,
        IReadOnlyModificationCommand source,
        IReadOnlyModificationCommand target,
        StringBuilder builder)
    {
        var reverseDependency = source.Table != foreignKey.Table;
        if (reverseDependency)
        {
            builder.AppendLine(" <-");
        }
        else
        {
            builder.Append(' ');
        }

        builder.Append("ForeignKey { ");

        var rowForeignKeyValueFactory = ((ForeignKeyConstraint)foreignKey).GetRowForeignKeyValueFactory();
        var dependentCommand = reverseDependency ? target : source;
        var values = rowForeignKeyValueFactory.CreateDependentKeyValue(dependentCommand, fromOriginalValues: !reverseDependency)!;
        FormatValues(values, foreignKey.Columns, dependentCommand, builder);

        builder.Append(" } ");

        if (!reverseDependency)
        {
            builder.AppendLine("<-");
        }
    }

    private void Format(IUniqueConstraint key, IReadOnlyModificationCommand source, IReadOnlyModificationCommand target, StringBuilder builder)
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

        builder.Append("Key { ");
        var rowForeignKeyValueFactory = ((UniqueConstraint)key).GetRowKeyValueFactory();
        var dependentCommand = reverseDependency ? target : source;
        var values = rowForeignKeyValueFactory.CreateKeyValue(dependentCommand, fromOriginalValues: !reverseDependency)!;
        FormatValues(values, key.Columns, dependentCommand, builder);

        builder.Append(" } ");

        if (!reverseDependency)
        {
            builder.AppendLine("<-");
        }
    }

    private void Format(ITableIndex index, IReadOnlyModificationCommand source, IReadOnlyModificationCommand target, StringBuilder builder)
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

        builder.Append("Index { ");

        var rowForeignKeyValueFactory = ((TableIndex)index).GetRowIndexValueFactory();
        var dependentCommand = reverseDependency ? target : source;
        var values = rowForeignKeyValueFactory.CreateValue(dependentCommand, fromOriginalValues: !reverseDependency)!;
        FormatValues(values, index.Columns, dependentCommand, builder);

        builder.Append(" } ");

        if (!reverseDependency)
        {
            builder.AppendLine("<-");
        }
    }

    private void FormatValues(object[] values, IReadOnlyList<IColumn> columns, IReadOnlyModificationCommand dependentCommand, StringBuilder builder)
    {
        for (var i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            builder.Append('\'');
            builder.Append(column.Name);
            builder.Append('\'');
            if (_sensitiveLoggingEnabled)
            {
                builder.Append(": ");
                builder.Append(values[i]);
            }

            if (i != columns.Count - 1)
            {
                builder.Append(", ");
            }
        }
    }

    private void AddForeignKeyEdges(
        Multigraph<IReadOnlyModificationCommand, IAnnotatable> commandGraph)
    {
        var predecessorsMap = new Dictionary<object, List<IReadOnlyModificationCommand>>();
        var originalPredecessorsMap = new Dictionary<object, List<IReadOnlyModificationCommand>>();
        foreach (var command in commandGraph.Vertices)
        {
            if (command.EntityState is EntityState.Modified or EntityState.Added)
            {
                foreach (var foreignKey in command.Table!.ReferencingForeignKeyConstraints)
                {
                    if (!IsModified(foreignKey.PrincipalUniqueConstraint.Columns, command))
                    {
                        continue;
                    }

                    var principalKeyValue = ((ForeignKeyConstraint)foreignKey).GetRowForeignKeyValueFactory()
                        .CreatePrincipalValueIndex(command);
                    Check.DebugAssert(principalKeyValue != null, "null principalKeyValue");

                    if (!predecessorsMap.TryGetValue(principalKeyValue, out var predecessorCommands))
                    {
                        predecessorCommands = new List<IReadOnlyModificationCommand>();
                        predecessorsMap.Add(principalKeyValue, predecessorCommands);
                    }

                    predecessorCommands.Add(command);
                }
            }

            if (command.EntityState is EntityState.Modified or EntityState.Deleted)
            {
                foreach (var foreignKey in command.Table!.ForeignKeyConstraints)
                {
                    if (!IsModified(foreignKey.Columns, command))
                    {
                        continue;
                    }

                    var dependentKeyValue = ((ForeignKeyConstraint)foreignKey).GetRowForeignKeyValueFactory()
                        .CreateDependentValueIndex(command, fromOriginalValues: true);
                    if (dependentKeyValue != null)
                    {
                        if (!originalPredecessorsMap.TryGetValue(dependentKeyValue, out var predecessorCommands))
                        {
                            predecessorCommands = new();
                            originalPredecessorsMap.Add(dependentKeyValue, predecessorCommands);
                        }

                        predecessorCommands.Add(command);
                    }
                }
            }
        }

        foreach (var command in commandGraph.Vertices)
        {
            if (command.EntityState is EntityState.Modified or EntityState.Added)
            {
                foreach (var foreignKey in command.Table!.ForeignKeyConstraints)
                {
                    if (!IsModified(foreignKey.Columns, command))
                    {
                        continue;
                    }

                    var dependentKeyValue = ((ForeignKeyConstraint)foreignKey).GetRowForeignKeyValueFactory()
                        .CreateDependentValueIndex(command);
                    if (dependentKeyValue != null)
                    {
                        AddMatchingPredecessorEdge(
                            predecessorsMap, dependentKeyValue, commandGraph, command, foreignKey);
                    }
                }
            }

            if (command.EntityState is EntityState.Modified or EntityState.Deleted)
            {
                foreach (var foreignKey in command.Table!.ReferencingForeignKeyConstraints)
                {
                    if (!IsModified(foreignKey.PrincipalUniqueConstraint.Columns, command))
                    {
                        continue;
                    }

                    var principalKeyValue = ((ForeignKeyConstraint)foreignKey).GetRowForeignKeyValueFactory()
                        .CreatePrincipalValueIndex(command, fromOriginalValues: true);
                    Check.DebugAssert(principalKeyValue != null, "null principalKeyValue");
                    AddMatchingPredecessorEdge(
                        originalPredecessorsMap, principalKeyValue, commandGraph, command, foreignKey);
                }
            }
        }
    }

    private static bool IsModified(IReadOnlyList<IColumn> columns, IReadOnlyModificationCommand command)
    {
        if (command.EntityState != EntityState.Modified)
        {
            return true;
        }

        foreach (var column in columns)
        {
            object? originalValue = null;
            object? currentValue = null;
            RelationalTypeMapping? typeMapping = null;
            foreach (var entry in command.Entries)
            {
                var columnMapping = column.FindColumnMapping(entry.EntityType);
                var property = columnMapping?.Property;
                if (property != null
                    && ((property.GetAfterSaveBehavior() == PropertySaveBehavior.Save)
                         || (!property.IsPrimaryKey() && entry.EntityState != EntityState.Modified)))
                {
                    switch (entry.EntityState)
                    {
                        case EntityState.Added:
                            currentValue = entry.GetCurrentProviderValue(property);
                            break;
                        case EntityState.Deleted:
                        case EntityState.Unchanged:
                            originalValue ??= entry.GetOriginalProviderValue(property);
                            break;
                        case EntityState.Modified:
                            if (entry.IsModified(property))
                            {
                                return true;
                            }

                            originalValue ??= entry.GetOriginalProviderValue(property);
                            break;
                    }

                    typeMapping = columnMapping!.TypeMapping;
                }
            }

            if (typeMapping != null
                && !typeMapping.ProviderComparer.Equals(originalValue, currentValue))
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
        Dictionary<object, List<IReadOnlyModificationCommand>>? indexPredecessorsMap = null;
        var keyPredecessorsMap = new Dictionary<object, List<IReadOnlyModificationCommand>>();
        foreach (var command in commandGraph.Vertices)
        {
            if (command.EntityState is EntityState.Added)
            {
                continue;
            }

            foreach (var index in command.Table!.Indexes)
            {
                if (!index.IsUnique
                    || !IsModified(index.Columns, command))
                {
                    continue;
                }

                var indexValue = ((TableIndex)index).GetRowIndexValueFactory()
                    .CreateValueIndex(command, fromOriginalValues: true);
                if (indexValue != null)
                {
                    indexPredecessorsMap ??= new();
                    if (!indexPredecessorsMap.TryGetValue(indexValue, out var predecessorCommands))
                    {
                        predecessorCommands = new();
                        indexPredecessorsMap.Add(indexValue, predecessorCommands);
                    }

                    predecessorCommands.Add(command);
                }
            }

            if (command.EntityState is not EntityState.Deleted)
            {
                continue;
            }

            foreach (var key in command.Table.UniqueConstraints)
            {
                var keyValue = ((UniqueConstraint)key).GetRowKeyValueFactory()
                    .CreateValueIndex(command, fromOriginalValues: true);
                Check.DebugAssert(keyValue != null, "null keyValue");
                if (!keyPredecessorsMap.TryGetValue((key, keyValue), out var predecessorCommands))
                {
                    predecessorCommands = new List<IReadOnlyModificationCommand>();
                    keyPredecessorsMap.Add((key, keyValue), predecessorCommands);
                }

                predecessorCommands.Add(command);
            }
        }

        if (indexPredecessorsMap != null)
        {
            foreach (var command in commandGraph.Vertices)
            {
                if (command.EntityState is EntityState.Deleted)
                {
                    continue;
                }

                foreach (var index in command.Table!.Indexes)
                {
                    if (!index.IsUnique
                        || !IsModified(index.Columns, command))
                    {
                        continue;
                    }

                    var indexValue = ((TableIndex)index).GetRowIndexValueFactory()
                        .CreateValueIndex(command);
                    if (indexValue != null)
                    {
                        AddMatchingPredecessorEdge(
                            indexPredecessorsMap, indexValue, commandGraph, command, index);
                    }
                }
            }
        }

        if (keyPredecessorsMap != null)
        {
            foreach (var command in commandGraph.Vertices)
            {
                if (command.EntityState is not EntityState.Added)
                {
                    continue;
                }

                foreach (var key in command.Table!.UniqueConstraints)
                {
                    var keyValue = ((UniqueConstraint)key).GetRowKeyValueFactory()
                        .CreateValueIndex(command, fromOriginalValues: true);
                    Check.DebugAssert(keyValue != null, "null keyValue");

                    AddMatchingPredecessorEdge(
                        keyPredecessorsMap, keyValue, commandGraph, command, key);
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
                            modificationCommandGraph.AddEdge(deleted, lastDelete, deleted.Table!);
                        }

                        deletedDictionary[table] = (deletedCommands.List, true);
                    }

                    modificationCommandGraph.AddEdge(lastDelete, command, command.Table!);
                }
            }
        }
    }
}
