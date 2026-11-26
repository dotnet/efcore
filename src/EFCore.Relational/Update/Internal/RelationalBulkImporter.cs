// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable CS1591
#pragma warning disable EF1001

using System.Text;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RelationalBulkImporter : IBulkImporter
{
    private const int DefaultBatchSize = 1000;
    private const int DefaultMaxParameters = 1000;
    private const int DefaultValuesBatchSize = 500;

    private readonly IRelationalConnection _connection;
    private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
    private readonly ISqlGenerationHelper _sqlGenerationHelper;
    private readonly IRelationalCommandDiagnosticsLogger _logger;
    private readonly ICurrentDbContext _currentDbContext;

    public RelationalBulkImporter(
        IRelationalConnection connection,
        IRelationalCommandBuilderFactory commandBuilderFactory,
        ISqlGenerationHelper sqlGenerationHelper,
        IRelationalCommandDiagnosticsLogger logger,
        ICurrentDbContext currentDbContext)
    {
        _connection = connection;
        _commandBuilderFactory = commandBuilderFactory;
        _sqlGenerationHelper = sqlGenerationHelper;
        _logger = logger;
        _currentDbContext = currentDbContext;
    }

    protected virtual IRelationalConnection Connection
        => _connection;

    protected virtual ISqlGenerationHelper SqlGenerationHelper
        => _sqlGenerationHelper;

    public virtual void Import<TEntity>(
        IEntityType entityType,
        IEnumerable<TEntity> entities,
        BulkImportOptions? options = null)
        where TEntity : class
    {
        Check.NotNull(entityType);
        Check.NotNull(entities);

        ValidateEntityType(entityType);

        if (CanExecuteNativeImport(entityType, options))
        {
            ExecuteNativeImport(entityType, entities, options);
            return;
        }

        ExecuteBatchInsert(entityType, entities, options);
    }

    public virtual async Task ImportAsync<TEntity>(
        IEntityType entityType,
        IEnumerable<TEntity> entities,
        BulkImportOptions? options = null,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        Check.NotNull(entityType);
        Check.NotNull(entities);

        ValidateEntityType(entityType);

        if (CanExecuteNativeImport(entityType, options))
        {
            await ExecuteNativeImportAsync(entityType, entities, options, cancellationToken).ConfigureAwait(false);
            return;
        }

        await ExecuteBatchInsertAsync(entityType, AsAsyncEnumerable(entities), options, cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task ImportAsync<TEntity>(
        IEntityType entityType,
        IAsyncEnumerable<TEntity> entities,
        BulkImportOptions? options = null,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        Check.NotNull(entityType);
        Check.NotNull(entities);

        ValidateEntityType(entityType);

        if (CanExecuteNativeImport(entityType, options))
        {
            await ExecuteNativeImportAsync(entityType, entities, options, cancellationToken).ConfigureAwait(false);
            return;
        }

        await ExecuteBatchInsertAsync(entityType, entities, options, cancellationToken).ConfigureAwait(false);
    }

    protected virtual void ValidateEntityType(IEntityType entityType)
    {
        if (entityType.GetMappingStrategy() == RelationalAnnotationNames.TptMappingStrategy)
        {
            throw new NotSupportedException(
                $"Bulk import does not currently support Table-per-Type (TPT) inheritance for entity type '{entityType.DisplayName()}'.");
        }

        var tables = entityType.GetTableMappings().Select(m => m.Table.Name).Distinct().ToList();
        if (tables.Count > 1)
        {
            throw new NotSupportedException(
                $"Bulk import does not currently support entity splitting across multiple tables for entity type '{entityType.DisplayName()}'.");
        }

        if (entityType.IsMappedToJson())
        {
            throw new NotSupportedException(
                $"Bulk import does not currently support entities mapped to JSON for entity type '{entityType.DisplayName()}'.");
        }
    }

    protected virtual bool CanExecuteNativeImport(IEntityType entityType, BulkImportOptions? options)
        => false;

    protected virtual void ExecuteNativeImport<TEntity>(
        IEntityType entityType,
        IEnumerable<TEntity> entities,
        BulkImportOptions? options)
        where TEntity : class
        => throw new NotSupportedException();

    protected virtual Task ExecuteNativeImportAsync<TEntity>(
        IEntityType entityType,
        IEnumerable<TEntity> entities,
        BulkImportOptions? options,
        CancellationToken cancellationToken)
        where TEntity : class
        => throw new NotSupportedException();

    protected virtual Task ExecuteNativeImportAsync<TEntity>(
        IEntityType entityType,
        IAsyncEnumerable<TEntity> entities,
        BulkImportOptions? options,
        CancellationToken cancellationToken)
        where TEntity : class
        => throw new NotSupportedException();

    protected virtual (string DestinationTable, StoreObjectIdentifier StoreObject) GetTableAndStoreObject(
        IEntityType entityType,
        BulkImportOptions? options)
    {
        var tableName = entityType.GetTableName();
        if (tableName is null)
        {
            throw new InvalidOperationException($"The entity type '{entityType.DisplayName()}' is not mapped to a table.");
        }

        var schema = entityType.GetSchema();
        var storeObject = StoreObjectIdentifier.Table(tableName, schema);
        var destinationTable = options?.DestinationTableName is not null
            ? (schema is not null
                ? _sqlGenerationHelper.DelimitIdentifier(options.DestinationTableName, schema)
                : _sqlGenerationHelper.DelimitIdentifier(options.DestinationTableName))
            : _sqlGenerationHelper.DelimitIdentifier(tableName, schema);

        return (destinationTable, storeObject);
    }

    protected virtual int? GetEffectiveTimeout(BulkImportOptions? options)
        => options?.Timeout ?? _connection.CommandTimeout;

    protected virtual void ExecuteBatchInsert<TEntity>(
        IEntityType entityType,
        IEnumerable<TEntity> entities,
        BulkImportOptions? options)
        where TEntity : class
    {
        var (delimitedTableName, storeObject) = GetTableAndStoreObject(entityType, options);
        var keepIdentity = options?.KeepIdentity ?? false;

        using var reader = new EntityDataReader<TEntity>(entityType, entities, storeObject, keepIdentity);
        var columns = reader.ColumnMappings;
        if (columns.Count == 0)
        {
            ExecuteDefaultValuesBatch(entities, delimitedTableName, options);
            return;
        }

        var maxBatchSize = options?.BatchSize ?? DefaultBatchSize;
        var maxRowsPerBatch = Math.Max(1, Math.Min(maxBatchSize, DefaultMaxParameters / columns.Count));
        var columnList = string.Join(", ", columns.Select(c => _sqlGenerationHelper.DelimitIdentifier(c.ColumnName)));

        var transactionEnlistManager = _connection as ITransactionEnlistmentManager;
        var shouldManageTransaction = _connection.CurrentTransaction is null
            && transactionEnlistManager?.EnlistedTransaction is null
            && transactionEnlistManager?.CurrentAmbientTransaction is null;
        IDbContextTransaction? transaction = null;

        var previousTimeout = _connection.CommandTimeout;
        var effectiveTimeout = GetEffectiveTimeout(options);
        if (effectiveTimeout.HasValue)
        {
            _connection.CommandTimeout = effectiveTimeout;
        }

        _connection.Open();
        try
        {
            if (shouldManageTransaction)
            {
                transaction = _connection.BeginTransaction();
            }

            var batch = new List<TEntity>(maxRowsPerBatch);
            foreach (var entity in entities)
            {
                batch.Add(entity);
                if (batch.Count >= maxRowsPerBatch)
                {
                    ExecuteSingleBatch(batch, delimitedTableName, columnList, columns);
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                ExecuteSingleBatch(batch, delimitedTableName, columnList, columns);
            }

            transaction?.Commit();
        }
        catch
        {
            transaction?.Rollback();
            throw;
        }
        finally
        {
            if (effectiveTimeout.HasValue)
            {
                _connection.CommandTimeout = previousTimeout;
            }

            transaction?.Dispose();
            _connection.Close();
        }
    }

    protected virtual async Task ExecuteBatchInsertAsync<TEntity>(
        IEntityType entityType,
        IAsyncEnumerable<TEntity> entities,
        BulkImportOptions? options,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var (delimitedTableName, storeObject) = GetTableAndStoreObject(entityType, options);
        var keepIdentity = options?.KeepIdentity ?? false;

        var reader = new EntityDataReader<TEntity>(entityType, entities, storeObject, keepIdentity, cancellationToken: cancellationToken);
        await using var _ = reader.ConfigureAwait(false);
        var columns = reader.ColumnMappings;
        if (columns.Count == 0)
        {
            await ExecuteDefaultValuesBatchAsync(entities, delimitedTableName, options, cancellationToken).ConfigureAwait(false);
            return;
        }

        var maxBatchSize = options?.BatchSize ?? DefaultBatchSize;
        var maxRowsPerBatch = Math.Max(1, Math.Min(maxBatchSize, DefaultMaxParameters / columns.Count));
        var columnList = string.Join(", ", columns.Select(c => _sqlGenerationHelper.DelimitIdentifier(c.ColumnName)));

        var transactionEnlistManager = _connection as ITransactionEnlistmentManager;
        var shouldManageTransaction = _connection.CurrentTransaction is null
            && transactionEnlistManager?.EnlistedTransaction is null
            && transactionEnlistManager?.CurrentAmbientTransaction is null;
        IDbContextTransaction? transaction = null;

        var previousTimeout = _connection.CommandTimeout;
        var effectiveTimeout = GetEffectiveTimeout(options);
        if (effectiveTimeout.HasValue)
        {
            _connection.CommandTimeout = effectiveTimeout;
        }

        await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (shouldManageTransaction)
            {
                transaction = await _connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            }

            var batch = new List<TEntity>(maxRowsPerBatch);

            await foreach (var entity in entities.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                batch.Add(entity);
                if (batch.Count >= maxRowsPerBatch)
                {
                    await ExecuteSingleBatchAsync(batch, delimitedTableName, columnList, columns, cancellationToken).ConfigureAwait(false);
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                await ExecuteSingleBatchAsync(batch, delimitedTableName, columnList, columns, cancellationToken).ConfigureAwait(false);
            }

            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            }

            throw;
        }
        finally
        {
            if (effectiveTimeout.HasValue)
            {
                _connection.CommandTimeout = previousTimeout;
            }

            if (transaction is not null)
            {
                await transaction.DisposeAsync().ConfigureAwait(false);
            }

            await _connection.CloseAsync().ConfigureAwait(false);
        }
    }

    private void ExecuteDefaultValuesBatch<TEntity>(
        IEnumerable<TEntity> entities,
        string delimitedTableName,
        BulkImportOptions? options)
        where TEntity : class
    {
        var transactionEnlistManager = _connection as ITransactionEnlistmentManager;
        var shouldManageTransaction = _connection.CurrentTransaction is null
            && transactionEnlistManager?.EnlistedTransaction is null
            && transactionEnlistManager?.CurrentAmbientTransaction is null;
        IDbContextTransaction? transaction = null;

        var previousTimeout = _connection.CommandTimeout;
        var effectiveTimeout = GetEffectiveTimeout(options);
        if (effectiveTimeout.HasValue)
        {
            _connection.CommandTimeout = effectiveTimeout;
        }

        _connection.Open();
        try
        {
            if (shouldManageTransaction)
            {
                transaction = _connection.BeginTransaction();
            }

            var batchCount = 0;
            var commandBuilder = _commandBuilderFactory.Create();

            foreach (var _ in entities)
            {
                commandBuilder.AppendLine($"INSERT INTO {delimitedTableName} DEFAULT VALUES;");
                batchCount++;
                if (batchCount >= DefaultValuesBatchSize)
                {
                    commandBuilder.Build().ExecuteNonQuery(
                        new RelationalCommandParameterObject(
                            _connection,
                            null,
                            null,
                            _currentDbContext.Context,
                            _logger,
                            CommandSource.ExecuteSqlRaw));
                    commandBuilder = _commandBuilderFactory.Create();
                    batchCount = 0;
                }
            }

            if (batchCount > 0)
            {
                commandBuilder.Build().ExecuteNonQuery(
                    new RelationalCommandParameterObject(
                        _connection,
                        null,
                        null,
                        _currentDbContext.Context,
                        _logger,
                        CommandSource.ExecuteSqlRaw));
            }

            transaction?.Commit();
        }
        catch
        {
            transaction?.Rollback();
            throw;
        }
        finally
        {
            if (effectiveTimeout.HasValue)
            {
                _connection.CommandTimeout = previousTimeout;
            }

            transaction?.Dispose();
            _connection.Close();
        }
    }

    private async Task ExecuteDefaultValuesBatchAsync<TEntity>(
        IAsyncEnumerable<TEntity> entities,
        string delimitedTableName,
        BulkImportOptions? options,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var transactionEnlistManager = _connection as ITransactionEnlistmentManager;
        var shouldManageTransaction = _connection.CurrentTransaction is null
            && transactionEnlistManager?.EnlistedTransaction is null
            && transactionEnlistManager?.CurrentAmbientTransaction is null;
        IDbContextTransaction? transaction = null;

        var previousTimeout = _connection.CommandTimeout;
        var effectiveTimeout = GetEffectiveTimeout(options);
        if (effectiveTimeout.HasValue)
        {
            _connection.CommandTimeout = effectiveTimeout;
        }

        await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (shouldManageTransaction)
            {
                transaction = await _connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            }

            var batchCount = 0;
            var commandBuilder = _commandBuilderFactory.Create();

            await foreach (var _ in entities.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                commandBuilder.AppendLine($"INSERT INTO {delimitedTableName} DEFAULT VALUES;");
                batchCount++;
                if (batchCount >= DefaultValuesBatchSize)
                {
                    await commandBuilder.Build().ExecuteNonQueryAsync(
                        new RelationalCommandParameterObject(
                            _connection,
                            null,
                            null,
                            _currentDbContext.Context,
                            _logger,
                            CommandSource.ExecuteSqlRaw),
                        cancellationToken).ConfigureAwait(false);
                    commandBuilder = _commandBuilderFactory.Create();
                    batchCount = 0;
                }
            }

            if (batchCount > 0)
            {
                await commandBuilder.Build().ExecuteNonQueryAsync(
                    new RelationalCommandParameterObject(
                        _connection,
                        null,
                        null,
                        _currentDbContext.Context,
                        _logger,
                        CommandSource.ExecuteSqlRaw),
                    cancellationToken).ConfigureAwait(false);
            }

            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            }

            throw;
        }
        finally
        {
            if (effectiveTimeout.HasValue)
            {
                _connection.CommandTimeout = previousTimeout;
            }

            if (transaction is not null)
            {
                await transaction.DisposeAsync().ConfigureAwait(false);
            }

            await _connection.CloseAsync().ConfigureAwait(false);
        }
    }

    private void ExecuteSingleBatch<TEntity>(
        List<TEntity> batch,
        string delimitedTableName,
        string columnList,
        IReadOnlyList<EntityDataReader<TEntity>.PropertyColumnMapping> columns)
        where TEntity : class
    {
        var (relationalCommand, parameterValues) = BuildBatchCommand(batch, delimitedTableName, columnList, columns);
        relationalCommand.ExecuteNonQuery(
            new RelationalCommandParameterObject(
                _connection,
                parameterValues,
                null,
                _currentDbContext.Context,
                _logger,
                CommandSource.ExecuteSqlRaw));
    }

    private async Task ExecuteSingleBatchAsync<TEntity>(
        List<TEntity> batch,
        string delimitedTableName,
        string columnList,
        IReadOnlyList<EntityDataReader<TEntity>.PropertyColumnMapping> columns,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var (relationalCommand, parameterValues) = BuildBatchCommand(batch, delimitedTableName, columnList, columns);
        await relationalCommand.ExecuteNonQueryAsync(
            new RelationalCommandParameterObject(
                _connection,
                parameterValues,
                null,
                _currentDbContext.Context,
                _logger,
                CommandSource.ExecuteSqlRaw),
            cancellationToken).ConfigureAwait(false);
    }

    private (IRelationalCommand Command, IReadOnlyDictionary<string, object?> ParameterValues) BuildBatchCommand<TEntity>(
        List<TEntity> batch,
        string delimitedTableName,
        string columnList,
        IReadOnlyList<EntityDataReader<TEntity>.PropertyColumnMapping> columns)
        where TEntity : class
    {
        var commandBuilder = _commandBuilderFactory.Create();
        var parameterValues = new Dictionary<string, object?>();

        var sqlBuilder = new StringBuilder();
        sqlBuilder.Append("INSERT INTO ")
            .Append(delimitedTableName)
            .Append(" (")
            .Append(columnList)
            .Append(") VALUES ");

        var paramIndex = 0;
        for (var i = 0; i < batch.Count; i++)
        {
            if (i > 0)
            {
                sqlBuilder.Append(", ");
            }

            sqlBuilder.Append('(');
            var entity = batch[i];

            for (var c = 0; c < columns.Count; c++)
            {
                if (c > 0)
                {
                    sqlBuilder.Append(", ");
                }

                var paramName = "p" + paramIndex++;
                var placeholder = _sqlGenerationHelper.GenerateParameterNamePlaceholder(paramName);
                sqlBuilder.Append(placeholder);

                var column = columns[c];
                var val = column.ValueAccessor(entity);

                commandBuilder.AddParameter(paramName, placeholder, column.TypeMapping, column.Property.IsNullable);
                parameterValues[paramName] = val;
            }

            sqlBuilder.Append(')');
        }

        sqlBuilder.Append(';');
        commandBuilder.Append(sqlBuilder.ToString());

        return (commandBuilder.Build(), parameterValues);
    }

    private static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            yield return item;
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
