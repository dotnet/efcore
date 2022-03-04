// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.SqlServer.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerModificationCommandBatch : AffectedCountModificationCommandBatch
{
    private const int DefaultNetworkPacketSizeBytes = 4096;
    private const int MaxScriptLength = 65536 * DefaultNetworkPacketSizeBytes / 2;
    private const int MaxParameterCount = 2100;
    private const int MaxRowCount = 1000;
    private int _parameterCount = 1; // Implicit parameter for the command text
    private readonly int _maxBatchSize;
    private readonly List<IReadOnlyModificationCommand> _bulkInsertCommands = new();
    private int _commandsLeftToLengthCheck = 50;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerModificationCommandBatch(
        ModificationCommandBatchFactoryDependencies dependencies,
        int? maxBatchSize)
        : base(dependencies)
    {
        if (maxBatchSize.HasValue
            && maxBatchSize.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxBatchSize), RelationalStrings.InvalidMaxBatchSize(maxBatchSize.Value));
        }

        _maxBatchSize = Math.Min(maxBatchSize ?? 42, MaxRowCount);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected new virtual ISqlServerUpdateSqlGenerator UpdateSqlGenerator
        => (ISqlServerUpdateSqlGenerator)base.UpdateSqlGenerator;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool CanAddCommand(IReadOnlyModificationCommand modificationCommand)
    {
        if (ModificationCommands.Count >= _maxBatchSize)
        {
            return false;
        }

        var additionalParameterCount = CountParameters(modificationCommand);

        if (_parameterCount + additionalParameterCount >= MaxParameterCount)
        {
            return false;
        }

        _parameterCount += additionalParameterCount;
        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool IsCommandTextValid()
    {
        if (--_commandsLeftToLengthCheck < 0)
        {
            UpdateCachedCommandText();
            var commandTextLength = CachedCommandText.Length;
            if (commandTextLength >= MaxScriptLength)
            {
                return false;
            }

            var averageCommandLength = commandTextLength / ModificationCommands.Count;
            var expectedAdditionalCommandCapacity = (MaxScriptLength - commandTextLength) / averageCommandLength;
            _commandsLeftToLengthCheck = Math.Max(1, expectedAdditionalCommandCapacity / 4);
        }

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override int GetParameterCount()
        => _parameterCount;

    private static int CountParameters(IReadOnlyModificationCommand modificationCommand)
    {
        var parameterCount = 0;
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var columnIndex = 0; columnIndex < modificationCommand.ColumnModifications.Count; columnIndex++)
        {
            var columnModification = modificationCommand.ColumnModifications[columnIndex];
            if (columnModification.UseCurrentValueParameter)
            {
                parameterCount++;
            }

            if (columnModification.UseOriginalValueParameter)
            {
                parameterCount++;
            }
        }

        return parameterCount;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void ResetCommandText()
    {
        base.ResetCommandText();

        _bulkInsertCommands.Clear();
    }

    private void AppendBulkInsertCommandText(int lastIndex)
    {
        if (_bulkInsertCommands.Count == 0)
        {
            return;
        }

        var wasCachedCommandTextEmpty = IsCachedCommandTextEmpty;

        var resultSetMapping = UpdateSqlGenerator.AppendBulkInsertOperation(
            CachedCommandText, _bulkInsertCommands, lastIndex - _bulkInsertCommands.Count, out var requiresTransaction);

        SetRequiresTransaction(!wasCachedCommandTextEmpty || requiresTransaction);

        for (var i = lastIndex - _bulkInsertCommands.Count; i < lastIndex; i++)
        {
            CommandResultSet[i] = resultSetMapping;
        }

        if (resultSetMapping != ResultSetMapping.NoResultSet)
        {
            CommandResultSet[lastIndex - 1] = ResultSetMapping.LastInResultSet;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void UpdateCachedCommandText()
    {
        base.UpdateCachedCommandText();

        AppendBulkInsertCommandText(ModificationCommands.Count);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void UpdateCachedCommandText(int commandPosition)
    {
        var newModificationCommand = ModificationCommands[commandPosition];

        if (newModificationCommand.EntityState == EntityState.Added)
        {
            if (_bulkInsertCommands.Count > 0
                && !CanBeInsertedInSameStatement(_bulkInsertCommands[0], newModificationCommand))
            {
                // The new Add command cannot be added to the pending bulk insert commands (e.g. different table).
                // Write out the pending commands before starting a new pending chain.
                AppendBulkInsertCommandText(commandPosition);
                _bulkInsertCommands.Clear();
            }

            _bulkInsertCommands.Add(newModificationCommand);

            LastCachedCommandIndex = commandPosition;
        }
        else
        {
            // If we have any pending bulk insert commands, write them out before the next non-Add command
            if (_bulkInsertCommands.Count > 0)
            {
                // Note that we don't care about the transactionality of the bulk insert SQL, since there's the additional non-Add
                // command coming right afterwards, and so a transaction is required in any case.
                AppendBulkInsertCommandText(commandPosition);
                _bulkInsertCommands.Clear();
            }

            base.UpdateCachedCommandText(commandPosition);
        }
    }

    private static bool CanBeInsertedInSameStatement(
        IReadOnlyModificationCommand firstCommand,
        IReadOnlyModificationCommand secondCommand)
        => string.Equals(firstCommand.TableName, secondCommand.TableName, StringComparison.Ordinal)
            && string.Equals(firstCommand.Schema, secondCommand.Schema, StringComparison.Ordinal)
            && firstCommand.ColumnModifications.Where(o => o.IsWrite).Select(o => o.ColumnName).SequenceEqual(
                secondCommand.ColumnModifications.Where(o => o.IsWrite).Select(o => o.ColumnName))
            && firstCommand.ColumnModifications.Where(o => o.IsRead).Select(o => o.ColumnName).SequenceEqual(
                secondCommand.ColumnModifications.Where(o => o.IsRead).Select(o => o.ColumnName));
}
