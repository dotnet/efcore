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
    private readonly bool _detailedErrorsEnabled;
    private readonly Multigraph<IReadOnlyModificationCommand, CommandDependency> _modificationCommandGraph;

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

        _modificationCommandGraph =
            new Multigraph<IReadOnlyModificationCommand, CommandDependency>(dependencies.ModificationCommandComparer);
        Dependencies = dependencies;

        if (dependencies.LoggingOptions.IsSensitiveDataLoggingEnabled)
        {
            _sensitiveLoggingEnabled = true;
        }

        if (dependencies.LoggingOptions.DetailedErrorsEnabled)
        {
            _detailedErrorsEnabled = true;
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
        var commandSets = TopologicalSort(commands);

        for (var commandSetIndex = 0; commandSetIndex < commandSets.Count; commandSetIndex++)
        {
            var batches = CreateCommandBatches(
                commandSets[commandSetIndex],
                commandSetIndex < commandSets.Count - 1,
                assertColumnModification: true,
                parameterNameGenerator);

            foreach (var batch in batches)
            {
                yield return batch;
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<ModificationCommandBatch> CreateCommandBatches(
        IEnumerable<IReadOnlyModificationCommand> commandSet,
        bool moreCommandSets)
        => CreateCommandBatches(commandSet, moreCommandSets, assertColumnModification: false);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private IEnumerable<ModificationCommandBatch> CreateCommandBatches(
        IEnumerable<IReadOnlyModificationCommand> commandSet,
        bool moreCommandSets,
        bool assertColumnModification,
        ParameterNameGenerator? parameterNameGenerator = null)
    {
        var batch = Dependencies.ModificationCommandBatchFactory.Create();

        foreach (var modificationCommand in commandSet)
        {
#if DEBUG
            if (assertColumnModification)
            {
                (modificationCommand as ModificationCommand)?.AssertColumnsNotInitialized();
            }
#endif

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

                    batch.Complete(moreBatchesExpected: true);

                    yield return batch;
                }
                else
                {
                    Dependencies.UpdateLogger.BatchSmallerThanMinBatchSize(
                        batch.ModificationCommands.SelectMany(c => c.Entries), batch.ModificationCommands.Count, _minBatchSize);

                    foreach (var command in batch.ModificationCommands)
                    {
                        batch = StartNewBatch(parameterNameGenerator, command);
                        batch.Complete(moreBatchesExpected: true);

                        yield return batch;
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

            batch.Complete(moreBatchesExpected: moreCommandSets);

            yield return batch;
        }
        else
        {
            Dependencies.UpdateLogger.BatchSmallerThanMinBatchSize(
                batch.ModificationCommands.SelectMany(c => c.Entries), batch.ModificationCommands.Count, _minBatchSize);

            for (var commandIndex = 0; commandIndex < batch.ModificationCommands.Count; commandIndex++)
            {
                var singleCommandBatch = StartNewBatch(parameterNameGenerator, batch.ModificationCommands[commandIndex]);
                singleCommandBatch.Complete(
                    moreBatchesExpected: moreCommandSets || commandIndex < batch.ModificationCommands.Count - 1);

                yield return singleCommandBatch;
            }
        }

        ModificationCommandBatch StartNewBatch(
            ParameterNameGenerator? parameterNameGenerator,
            IReadOnlyModificationCommand modificationCommand)
        {
            parameterNameGenerator?.Reset();
            var batch = Dependencies.ModificationCommandBatchFactory.Create();
            batch.TryAddCommand(modificationCommand);
            return batch;
        }
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
        Dictionary<(string Name, string? Schema), SharedTableEntryMap<IModificationCommand>>? sharedTablesCommandsMap = null;
        foreach (var entry in entries)
        {
            if (entry is { SharedIdentityEntry: not null, EntityState: EntityState.Deleted })
            {
                continue;
            }

            using var sharedIdentityTableMappings =
                entry.SharedIdentityEntry != null
                    && entry.SharedIdentityEntry.EntityState == EntityState.Deleted
                    ? entry.SharedIdentityEntry.EntityType.GetTableMappings().GetEnumerator()
                    : null;

            var foundMapping = false;
            foreach (var tableMapping in entry.EntityType.GetTableMappings())
            {
                if (sharedIdentityTableMappings != null
                    && sharedIdentityTableMappings.MoveNext()
                    && sharedIdentityTableMappings.Current.Table != tableMapping.Table)
                {
                    ProcessEntry(entry.SharedIdentityEntry!, sharedIdentityTableMappings.Current, commands, updateAdapter, generateParameterName, ref sharedTablesCommandsMap);
                }

                ProcessEntry(entry, tableMapping, commands, updateAdapter, generateParameterName, ref sharedTablesCommandsMap);

                foundMapping = true;
            }

            while (sharedIdentityTableMappings != null
                    && sharedIdentityTableMappings.MoveNext())
            {
                ProcessEntry(entry.SharedIdentityEntry!, sharedIdentityTableMappings.Current, commands, updateAdapter, generateParameterName, ref sharedTablesCommandsMap);
            }

            if (!foundMapping)
            {
                throw new InvalidOperationException(RelationalStrings.ReadonlyEntitySaved(entry.EntityType.DisplayName()));
            }
        }

        if (sharedTablesCommandsMap != null)
        {
            AddUnchangedSharingEntries(sharedTablesCommandsMap.Values, entries);
        }

        return commands;

        void ProcessEntry(
            IUpdateEntry entry,
            ITableMapping tableMapping,
            List<IModificationCommand> commands,
            IUpdateAdapter updateAdapter,
            Func<string> generateParameterName,
            ref Dictionary<(string Name, string? Schema), SharedTableEntryMap<IModificationCommand>>? sharedTablesCommandsMap)
        {
            var sprocMapping = entry.EntityState switch
            {
                EntityState.Added => tableMapping.InsertStoredProcedureMapping,
                EntityState.Modified => tableMapping.UpdateStoredProcedureMapping,
                EntityState.Deleted => tableMapping.DeleteStoredProcedureMapping,

                _ => throw new ArgumentOutOfRangeException("Unexpected entry.EntityState: " + entry.EntityState)
            };

            var table = tableMapping.Table;

            IModificationCommand command;
            var isMainEntry = true;
            if (table.IsShared)
            {
                Check.DebugAssert(sprocMapping is null, "Shared table with sproc mapping");

                sharedTablesCommandsMap ??= new Dictionary<(string Name, string? Schema), SharedTableEntryMap<IModificationCommand>>();

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
                            t, _sensitiveLoggingEnabled, _detailedErrorsEnabled, comparer, generateParameterName,
                            Dependencies.UpdateLogger)));
                isMainEntry = sharedCommandsMap.IsMainEntry(entry);
            }
            else
            {
                command = Dependencies.ModificationCommandFactory.CreateModificationCommand(
                    new ModificationCommandParameters(
                        table, sprocMapping?.StoreStoredProcedure, _sensitiveLoggingEnabled, _detailedErrorsEnabled,
                        comparer: null, generateParameterName, Dependencies.UpdateLogger));
            }

            command.AddEntry(entry, isMainEntry);
            commands.Add(command);
        }
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

                    if (entry.EntityType.IsMappedToJson())
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
    public virtual IReadOnlyList<List<IReadOnlyModificationCommand>> TopologicalSort(
        IEnumerable<IReadOnlyModificationCommand> commands)
    {
        _modificationCommandGraph.Clear();
        _modificationCommandGraph.AddVertices(commands);

        AddForeignKeyEdges();

        AddUniqueValueEdges();

        AddSameTableEdges();

        return _modificationCommandGraph.BatchingTopologicalSort(
            static (_, _, edges) => edges.All(e => e.Breakable),
            FormatCycle);
    }

    private string FormatCycle(
        IReadOnlyList<Tuple<IReadOnlyModificationCommand, IReadOnlyModificationCommand, IEnumerable<CommandDependency>>> data)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < data.Count; i++)
        {
            var (command1, command2, edges) = data[i];
            Format(command1, builder);

            switch (edges.First().Metadata)
            {
                case IForeignKey foreignKey:
                    Format(foreignKey, command1, command2, builder);
                    break;
                case IForeignKeyConstraint foreignKeyConstraint:
                    Format(foreignKeyConstraint, command1, command2, builder);
                    break;
                case IKey key:
                    Format(key, command1, command2, builder);
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

        builder.Append("ForeignKey ");

        var dependentCommand = reverseDependency ? target : source;
        var dependentEntry = dependentCommand.Entries.First(e => foreignKey.DeclaringEntityType.IsAssignableFrom(e.EntityType));
        builder.Append(dependentEntry.BuildCurrentValuesString(foreignKey.Properties))
            .Append(' ');

        if (!reverseDependency)
        {
            builder.AppendLine("<-");
        }
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

        builder.Append("ForeignKeyConstraint { ");

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

    private void Format(IKey key, IReadOnlyModificationCommand source, IReadOnlyModificationCommand target, StringBuilder builder)
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

        builder.Append("Key ");
        var dependentCommand = reverseDependency ? target : source;
        var dependentEntry = dependentCommand.Entries.First(e => key.DeclaringEntityType.IsAssignableFrom(e.EntityType));
        builder.Append(
            reverseDependency
                ? dependentEntry.BuildCurrentValuesString(key.Properties)
                : dependentEntry.BuildOriginalValuesString(key.Properties));

        builder.Append(' ');

        if (!reverseDependency)
        {
            builder.AppendLine("<-");
        }
    }

    private void Format(
        IUniqueConstraint constraint,
        IReadOnlyModificationCommand source,
        IReadOnlyModificationCommand target,
        StringBuilder builder)
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

        builder.Append("UniqueConstraint { ");
        var rowForeignKeyValueFactory = ((UniqueConstraint)constraint).GetRowKeyValueFactory();
        var dependentCommand = reverseDependency ? target : source;
        var values = rowForeignKeyValueFactory.CreateKeyValue(dependentCommand, fromOriginalValues: !reverseDependency)!;
        FormatValues(values, constraint.Columns, dependentCommand, builder);

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
        var indexValue = rowForeignKeyValueFactory.CreateIndexValue(dependentCommand, fromOriginalValues: !reverseDependency)!;
        FormatValues(indexValue.Value!, index.Columns, dependentCommand, builder);

        builder.Append(" } ");

        if (!reverseDependency)
        {
            builder.AppendLine("<-");
        }
    }

    private void FormatValues(
        object?[] values,
        IReadOnlyList<IColumn> columns,
        IReadOnlyModificationCommand dependentCommand,
        StringBuilder builder)
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
                var value = values[i];
                if (value != null)
                {
                    builder.Append(values[i]);
                }
                else
                {
                    builder.Append("NULL");
                }
            }

            if (i != columns.Count - 1)
            {
                builder.Append(", ");
            }
        }
    }

    private void AddForeignKeyEdges()
    {
        var predecessorsMap = new Dictionary<object, List<IReadOnlyModificationCommand>>();
        var originalPredecessorsMap = new Dictionary<object, List<IReadOnlyModificationCommand>>();
        foreach (var command in _modificationCommandGraph.Vertices)
        {
            if (command.EntityState is EntityState.Modified or EntityState.Added)
            {
                if (command.Table != null)
                {
                    foreach (var foreignKey in command.Table.ReferencingForeignKeyConstraints)
                    {
                        if (!IsModified(foreignKey.PrincipalUniqueConstraint.Columns, command))
                        {
                            continue;
                        }

                        var principalKeyValue = ((ForeignKeyConstraint)foreignKey).GetRowForeignKeyValueFactory()
                            .CreatePrincipalEquatableKeyValue(command);
                        Check.DebugAssert(principalKeyValue != null, "null principalKeyValue");

                        if (!predecessorsMap.TryGetValue(principalKeyValue, out var predecessorCommands))
                        {
                            predecessorCommands = [];
                            predecessorsMap.Add(principalKeyValue, predecessorCommands);
                        }

                        predecessorCommands.Add(command);
                    }
                }

                for (var i = 0; i < command.Entries.Count; i++)
                {
                    var entry = command.Entries[i];
                    foreach (var foreignKey in entry.EntityType.GetReferencingForeignKeys())
                    {
                        if (!CanCreateDependency(foreignKey, command, principal: true)
                            || !IsModified(foreignKey.PrincipalKey.Properties, entry)
                            || (command.Table != null
                                && !IsStoreGenerated(entry, foreignKey.PrincipalKey)))
                        {
                            continue;
                        }

                        var principalKeyValue = foreignKey.GetDependentKeyValueFactory()
                            .CreatePrincipalEquatableKey(entry);
                        Check.DebugAssert(principalKeyValue != null, "null principalKeyValue");

                        if (!predecessorsMap.TryGetValue(principalKeyValue, out var predecessorCommands))
                        {
                            predecessorCommands = [];
                            predecessorsMap.Add(principalKeyValue, predecessorCommands);
                        }

                        predecessorCommands.Add(command);
                    }
                }
            }

            if (command.EntityState is EntityState.Modified or EntityState.Deleted)
            {
                if (command.Table != null)
                {
                    foreach (var foreignKey in command.Table!.ForeignKeyConstraints)
                    {
                        if (!IsModified(foreignKey.Columns, command))
                        {
                            continue;
                        }

                        var dependentKeyValue = ((ForeignKeyConstraint)foreignKey).GetRowForeignKeyValueFactory()
                            .CreateDependentEquatableKeyValue(command, fromOriginalValues: true);
                        if (dependentKeyValue != null)
                        {
                            if (!originalPredecessorsMap.TryGetValue(dependentKeyValue, out var predecessorCommands))
                            {
                                predecessorCommands = [];
                                originalPredecessorsMap.Add(dependentKeyValue, predecessorCommands);
                            }

                            predecessorCommands.Add(command);
                        }
                    }
                }
                else
                {
                    foreach (var entry in command.Entries)
                    {
                        foreach (var foreignKey in entry.EntityType.GetForeignKeys())
                        {
                            if (!CanCreateDependency(foreignKey, command, principal: false)
                                || !IsModified(foreignKey.Properties, entry))
                            {
                                continue;
                            }

                            var dependentKeyValue = foreignKey.GetDependentKeyValueFactory()
                                ?.CreateDependentEquatableKey(entry, fromOriginalValues: true);

                            if (dependentKeyValue != null)
                            {
                                if (!originalPredecessorsMap.TryGetValue(dependentKeyValue, out var predecessorCommands))
                                {
                                    predecessorCommands = [];
                                    originalPredecessorsMap.Add(dependentKeyValue, predecessorCommands);
                                }

                                predecessorCommands.Add(command);
                            }
                        }
                    }
                }
            }
        }

        foreach (var command in _modificationCommandGraph.Vertices)
        {
            if (command.EntityState is EntityState.Modified or EntityState.Added)
            {
                if (command.Table != null)
                {
                    foreach (var foreignKey in command.Table.ForeignKeyConstraints)
                    {
                        if (!IsModified(foreignKey.Columns, command))
                        {
                            continue;
                        }

                        var dependentKeyValue = ((ForeignKeyConstraint)foreignKey).GetRowForeignKeyValueFactory()
                            .CreateDependentEquatableKeyValue(command);
                        if (dependentKeyValue is null)
                        {
                            continue;
                        }

                        AddMatchingPredecessorEdge(
                            predecessorsMap, dependentKeyValue, command, foreignKey, checkStoreGenerated: true);
                    }
                }

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var entryIndex = 0; entryIndex < command.Entries.Count; entryIndex++)
                {
                    var entry = command.Entries[entryIndex];
                    foreach (var foreignKey in entry.EntityType.GetForeignKeys())
                    {
                        if (!CanCreateDependency(foreignKey, command, principal: false)
                            || !IsModified(foreignKey.Properties, entry))
                        {
                            continue;
                        }

                        var dependentKeyValue = foreignKey.GetDependentKeyValueFactory()
                            ?.CreateDependentEquatableKey(entry);
                        if (dependentKeyValue == null)
                        {
                            continue;
                        }

                        AddMatchingPredecessorEdge(
                            predecessorsMap, dependentKeyValue, command, foreignKey, checkStoreGenerated: true);
                    }
                }
            }

            if (command.EntityState is EntityState.Modified or EntityState.Deleted)
            {
                if (command.Table != null)
                {
                    foreach (var foreignKey in command.Table.ReferencingForeignKeyConstraints)
                    {
                        if (!IsModified(foreignKey.PrincipalUniqueConstraint.Columns, command))
                        {
                            continue;
                        }

                        var principalKeyValue = ((ForeignKeyConstraint)foreignKey).GetRowForeignKeyValueFactory()
                            .CreatePrincipalEquatableKeyValue(command, fromOriginalValues: true);
                        Check.DebugAssert(principalKeyValue != null, "null principalKeyValue");
                        AddMatchingPredecessorEdge(
                            originalPredecessorsMap, principalKeyValue, command, foreignKey);
                    }
                }
                else
                {
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var entryIndex = 0; entryIndex < command.Entries.Count; entryIndex++)
                    {
                        var entry = command.Entries[entryIndex];
                        foreach (var foreignKey in entry.EntityType.GetReferencingForeignKeys())
                        {
                            if (!CanCreateDependency(foreignKey, command, principal: true))
                            {
                                continue;
                            }

                            var principalKeyValue = foreignKey.GetDependentKeyValueFactory()
                                .CreatePrincipalEquatableKey(entry, fromOriginalValues: true);
                            Check.DebugAssert(principalKeyValue != null, "null principalKeyValue");
                            AddMatchingPredecessorEdge(
                                originalPredecessorsMap, principalKeyValue, command, foreignKey);
                        }
                    }
                }
            }
        }
    }

    private static bool IsStoreGenerated(IUpdateEntry entry, IKey key)
    {
        var keyProperties = key.Properties;

        // ReSharper disable once ForCanBeConvertedToForeach
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = 0; i < keyProperties.Count; i++)
        {
            var keyProperty = keyProperties[i];

            if (entry.IsStoreGenerated(keyProperty))
            {
                return true;
            }
        }

        return false;
    }

    private static bool CanCreateDependency(IForeignKey foreignKey, IReadOnlyModificationCommand command, bool principal)
    {
        if (command.Table != null)
        {
            if (foreignKey.IsRowInternal(StoreObjectIdentifier.Table(command.TableName, command.Schema))
                || (foreignKey.PrincipalEntityType.IsAssignableFrom(foreignKey.DeclaringEntityType)
                    && foreignKey.PrincipalKey.Properties.SequenceEqual(foreignKey.Properties)))
            {
                // Row internal or TPT linking FK
                return false;
            }

            if (foreignKey.GetMappedConstraints().Any(c => (principal ? c.PrincipalTable : c.Table) == command.Table))
            {
                // Handled elsewhere
                return false;
            }

            var properties = principal ? foreignKey.PrincipalKey.Properties : foreignKey.Properties;
            foreach (var property in properties)
            {
                if (command.Table.FindColumn(property) == null)
                {
                    return false;
                }
            }

            return true;
        }

        if (command.StoreStoredProcedure != null)
        {
            if (command.StoreStoredProcedure.StoredProcedures.Any(sp => foreignKey.IsRowInternal(sp.GetStoreIdentifier())))
            {
                return false;
            }

            var properties = principal ? foreignKey.PrincipalKey.Properties : foreignKey.Properties;
            foreach (var property in properties)
            {
                if (command.StoreStoredProcedure.FindResultColumn(property) == null
                    && command.StoreStoredProcedure.FindParameter(property) == null)
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    private static bool CanCreateDependency(IKey key, IReadOnlyModificationCommand command)
    {
        if (command.Table != null)
        {
            if (key.GetMappedConstraints().Any(c => c.Table == command.Table))
            {
                // Handled elsewhere
                return false;
            }

            foreach (var property in key.Properties)
            {
                if (command.Table.FindColumn(property) == null)
                {
                    return false;
                }
            }

            return true;
        }

        if (command.StoreStoredProcedure != null)
        {
            foreach (var property in key.Properties)
            {
                if (command.StoreStoredProcedure.FindResultColumn(property) == null
                    && command.StoreStoredProcedure.FindParameter(property) == null)
                {
                    return false;
                }
            }

            return true;
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

    private static bool IsModified(IReadOnlyList<IColumn> columns, IReadOnlyModificationCommand command)
    {
        if (command.EntityState != EntityState.Modified)
        {
            return true;
        }

        for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
        {
            var column = columns[columnIndex];
            object? originalValue = null;
            object? currentValue = null;
            ValueComparer? providerValueComparer = null;
            for (var entryIndex = 0; entryIndex < command.Entries.Count; entryIndex++)
            {
                var entry = command.Entries[entryIndex];
                var columnMapping = column.FindColumnMapping(entry.EntityType);
                var property = columnMapping?.Property;
                if (property != null
                    && (property.GetAfterSaveBehavior() == PropertySaveBehavior.Save
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
                            currentValue ??= entry.GetCurrentProviderValue(property);
                            break;
                    }

                    providerValueComparer = property.GetProviderValueComparer();
                }
            }

            if (providerValueComparer != null
                && !providerValueComparer.Equals(originalValue, currentValue))
            {
                return true;
            }
        }

        return false;
    }

    private void AddMatchingPredecessorEdge<T>(
        Dictionary<T, List<IReadOnlyModificationCommand>> predecessorsMap,
        T keyValue,
        IReadOnlyModificationCommand command,
        IForeignKey foreignKey,
        bool checkStoreGenerated = false)
        where T : notnull
    {
        if (predecessorsMap.TryGetValue(keyValue, out var predecessorCommands))
        {
            foreach (var predecessor in predecessorCommands)
            {
                if (predecessor != command)
                {
                    // If we're adding/inserting a dependent where the principal key is being database-generated, then
                    // the dependency edge represents a batching boundary: fetch the principal database-generated
                    // property from the database in separate batch, in order to populate the dependent foreign key
                    // property in the next.
                    var requiresBatchingBoundary = false;

                    if (checkStoreGenerated)
                    {
                        for (var j = 0; j < predecessor.Entries.Count; j++)
                        {
                            var entry = predecessor.Entries[j];
                            if (IsStoreGenerated(entry, foreignKey.PrincipalKey))
                            {
                                requiresBatchingBoundary = true;
                                goto AfterLoop;
                            }
                        }
                    }

                    AfterLoop:
                    _modificationCommandGraph.AddEdge(predecessor, command, new CommandDependency(foreignKey), requiresBatchingBoundary);
                }
            }
        }
    }

    private void AddMatchingPredecessorEdge<T>(
        Dictionary<T, List<IReadOnlyModificationCommand>> predecessorsMap,
        T keyValue,
        IReadOnlyModificationCommand command,
        IForeignKeyConstraint foreignKey,
        bool checkStoreGenerated = false)
        where T : notnull
    {
        if (predecessorsMap.TryGetValue(keyValue, out var predecessorCommands))
        {
            foreach (var predecessor in predecessorCommands)
            {
                if (predecessor != command)
                {
                    // If we're adding/inserting a dependent where the principal key is being database-generated, then
                    // the dependency edge represents a batching boundary: fetch the principal database-generated
                    // property from the database in separate batch, in order to populate the dependent foreign key
                    // property in the next.
                    var requiresBatchingBoundary = false;

                    if (checkStoreGenerated)
                    {
                        for (var j = 0; j < predecessor.Entries.Count; j++)
                        {
                            var entry = predecessor.Entries[j];

                            foreach (var key in foreignKey.PrincipalUniqueConstraint.MappedKeys)
                            {
                                if (key.DeclaringEntityType.IsAssignableFrom(entry.EntityType)
                                    && IsStoreGenerated(entry, key))
                                {
                                    requiresBatchingBoundary = true;
                                    goto AfterLoop;
                                }
                            }
                        }
                    }

                    AfterLoop:
                    _modificationCommandGraph.AddEdge(predecessor, command, new CommandDependency(foreignKey), requiresBatchingBoundary);
                }
            }
        }
    }

    private void AddMatchingPredecessorEdge<T>(
        Dictionary<T, List<IReadOnlyModificationCommand>> predecessorsMap,
        T keyValue,
        IReadOnlyModificationCommand command,
        CommandDependency edge)
        where T : notnull
    {
        if (predecessorsMap.TryGetValue(keyValue, out var predecessorCommands))
        {
            foreach (var predecessor in predecessorCommands)
            {
                if (predecessor != command)
                {
                    _modificationCommandGraph.AddEdge(predecessor, command, edge);
                }
            }
        }
    }

    private void AddUniqueValueEdges()
    {
        Dictionary<object, List<IReadOnlyModificationCommand>>? indexPredecessorsMap = null;
        var keyPredecessorsMap = new Dictionary<object, List<IReadOnlyModificationCommand>>();
        foreach (var command in _modificationCommandGraph.Vertices)
        {
            if (command.EntityState is EntityState.Added)
            {
                continue;
            }

            if (command.Table != null)
            {
                foreach (var index in command.Table.Indexes)
                {
                    if (!index.IsUnique
                        || !IsModified(index.Columns, command))
                    {
                        continue;
                    }

                    var (value, _) = ((TableIndex)index).GetRowIndexValueFactory()
                        .CreateEquatableIndexValue(command, fromOriginalValues: true);
                    if (value != null)
                    {
                        indexPredecessorsMap ??= new Dictionary<object, List<IReadOnlyModificationCommand>>();
                        if (!indexPredecessorsMap.TryGetValue(value, out var predecessorCommands))
                        {
                            predecessorCommands = [];
                            indexPredecessorsMap.Add(value, predecessorCommands);
                        }

                        predecessorCommands.Add(command);
                    }
                }
            }

            if (command.EntityState is not EntityState.Deleted)
            {
                continue;
            }

            if (command.Table != null)
            {
                foreach (var key in command.Table.UniqueConstraints)
                {
                    var keyValue = ((UniqueConstraint)key).GetRowKeyValueFactory()
                        .CreateEquatableKeyValue(command, fromOriginalValues: true);
                    Check.DebugAssert(keyValue != null, "null keyValue");
                    if (!keyPredecessorsMap.TryGetValue((key, keyValue), out var predecessorCommands))
                    {
                        predecessorCommands = [];
                        keyPredecessorsMap.Add((key, keyValue), predecessorCommands);
                    }

                    predecessorCommands.Add(command);
                }
            }
            else
            {
                for (var entryIndex = 0; entryIndex < command.Entries.Count; entryIndex++)
                {
                    var entry = command.Entries[entryIndex];
                    foreach (var key in entry.EntityType.GetKeys())
                    {
                        if (!CanCreateDependency(key, command))
                        {
                            continue;
                        }

                        var keyValue = key.GetPrincipalKeyValueFactory()
                            .CreateEquatableKey(entry, fromOriginalValues: true);
                        Check.DebugAssert(keyValue != null, "null keyValue");
                        if (!keyPredecessorsMap.TryGetValue((key, keyValue), out var predecessorCommands))
                        {
                            predecessorCommands = [];
                            keyPredecessorsMap.Add((key, keyValue), predecessorCommands);
                        }

                        predecessorCommands.Add(command);
                    }
                }
            }
        }

        if (indexPredecessorsMap != null)
        {
            foreach (var command in _modificationCommandGraph.Vertices)
            {
                if (command.EntityState is EntityState.Deleted
                    || command.Table == null)
                {
                    continue;
                }

                foreach (var index in command.Table.Indexes)
                {
                    if (!index.IsUnique
                        || !IsModified(index.Columns, command))
                    {
                        continue;
                    }

                    var (value, hasNullValue) = ((TableIndex)index).GetRowIndexValueFactory()
                        .CreateEquatableIndexValue(command);
                    if (value != null)
                    {
                        AddMatchingPredecessorEdge(
                            indexPredecessorsMap, value, command,
                            new CommandDependency(index, breakable: index.Filter != null || hasNullValue));
                    }
                }
            }
        }

        if (keyPredecessorsMap != null)
        {
            foreach (var command in _modificationCommandGraph.Vertices)
            {
                if (command.EntityState is not EntityState.Added)
                {
                    continue;
                }

                if (command.Table != null)
                {
                    foreach (var key in command.Table.UniqueConstraints)
                    {
                        var keyValue = ((UniqueConstraint)key).GetRowKeyValueFactory()
                            .CreateEquatableKeyValue(command, fromOriginalValues: true);
                        Check.DebugAssert(keyValue != null, "null keyValue");

                        AddMatchingPredecessorEdge(keyPredecessorsMap, keyValue, command, new CommandDependency(key));
                    }
                }
                else
                {
                    for (var entryIndex = 0; entryIndex < command.Entries.Count; entryIndex++)
                    {
                        var entry = command.Entries[entryIndex];
                        foreach (var key in entry.EntityType.GetKeys())
                        {
                            if (!CanCreateDependency(key, command))
                            {
                                continue;
                            }

                            var keyValue = key.GetPrincipalKeyValueFactory()
                                .CreateEquatableKey(entry, fromOriginalValues: true);
                            Check.DebugAssert(keyValue != null, "null keyValue");

                            AddMatchingPredecessorEdge(keyPredecessorsMap, keyValue, command, new CommandDependency(key));
                        }
                    }
                }
            }
        }
    }

    private void AddSameTableEdges()
    {
        var deletedDictionary = new Dictionary<(string, string?), (List<IReadOnlyModificationCommand> List, bool EdgesAdded)>();

        foreach (var command in _modificationCommandGraph.Vertices)
        {
            if (command.EntityState == EntityState.Deleted)
            {
                var table = (command.TableName, command.Schema);
                if (!deletedDictionary.TryGetValue(table, out var deletedCommands))
                {
                    deletedCommands = ([], false);
                    deletedDictionary.Add(table, deletedCommands);
                }

                deletedCommands.List.Add(command);
            }
        }

        foreach (var command in _modificationCommandGraph.Vertices)
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
                            _modificationCommandGraph.AddEdge(deleted, lastDelete, new CommandDependency(deleted.Table!, breakable: true));
                        }

                        deletedDictionary[table] = (deletedCommands.List, true);
                    }

                    _modificationCommandGraph.AddEdge(lastDelete, command, new CommandDependency(command.Table!, breakable: true));
                }
            }
        }
    }

    /// <inheritdoc/>
    void IResettableService.ResetState() => _modificationCommandGraph.Clear();

    /// <inheritdoc/>
    Task IResettableService.ResetStateAsync(CancellationToken cancellationToken)
    {
        ((IResettableService)this).ResetState();

        return Task.CompletedTask;
    }

    private sealed record class CommandDependency
    {
        public CommandDependency(IAnnotatable metadata, bool breakable = false)
        {
            Metadata = metadata;
            Breakable = breakable;
        }

        public IAnnotatable Metadata { get; }
        public bool Breakable { get; }
    }
}
