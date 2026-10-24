// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable CS1591

using System.Text;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
    private readonly IRelationalConnection _connection;
    private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
    private readonly ISqlGenerationHelper _sqlGenerationHelper;
    private readonly IRelationalCommandDiagnosticsLogger _logger;
    private readonly ICurrentDbContext _currentDbContext;

    public RelationalBulkImporter(
        IRelationalConnection connection,
        IRawSqlCommandBuilder rawSqlCommandBuilder,
        ISqlGenerationHelper sqlGenerationHelper,
        IRelationalCommandDiagnosticsLogger logger,
        ICurrentDbContext currentDbContext)
    {
        _connection = connection;
        _rawSqlCommandBuilder = rawSqlCommandBuilder;
        _sqlGenerationHelper = sqlGenerationHelper;
        _logger = logger;
        _currentDbContext = currentDbContext;
    }

    public virtual void Import<TEntity>(
        IEntityType entityType,
        IEnumerable<TEntity> entities,
        BulkImportOptions? options = null)
        where TEntity : class
        => ImportAsync(entityType, entities, options, CancellationToken.None).GetAwaiter().GetResult();

    public virtual async Task ImportAsync<TEntity>(
        IEntityType entityType,
        IEnumerable<TEntity> entities,
        BulkImportOptions? options = null,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        Check.NotNull(entityType);
        Check.NotNull(entities);

        await ImportCoreAsync(entityType, AsAsyncEnumerable(entities), options, cancellationToken).ConfigureAwait(false);
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

        await ImportCoreAsync(entityType, entities, options, cancellationToken).ConfigureAwait(false);
    }

    private static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            yield return item;
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    private readonly record struct PropertyColumn(
        IProperty Property,
        string ColumnName,
        string DelimitedColumnName,
        IClrPropertyGetter Getter,
        ValueConverter? Converter);

    private async Task ImportCoreAsync<TEntity>(
        IEntityType entityType,
        IAsyncEnumerable<TEntity> entities,
        BulkImportOptions? options,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var tableName = entityType.GetTableName();
        if (tableName is null)
        {
            throw new InvalidOperationException($"The entity type '{entityType.DisplayName()}' is not mapped to a table.");
        }

        var schema = entityType.GetSchema();
        var storeObject = StoreObjectIdentifier.Table(tableName, schema);
        var delimitedTableName = _sqlGenerationHelper.DelimitIdentifier(tableName, schema);

        var keepIdentity = options?.KeepIdentity ?? false;
        var columns = new List<PropertyColumn>();

        foreach (var property in entityType.GetProperties())
        {
            var columnName = property.GetColumnName(storeObject);
            if (columnName is null)
            {
                continue;
            }

            if (property.IsPrimaryKey()
                && property.ValueGenerated != ValueGenerated.Never
                && !keepIdentity)
            {
                continue;
            }

            var getter = property.GetGetter();
            var converter = property.GetValueConverter() ?? property.FindTypeMapping()?.Converter;
            columns.Add(new PropertyColumn(
                property,
                columnName,
                _sqlGenerationHelper.DelimitIdentifier(columnName),
                getter,
                converter));
        }

        if (columns.Count == 0)
        {
            return;
        }

        var maxBatchSize = options?.BatchSize ?? 1000;
        var maxRowsPerBatch = Math.Max(1, Math.Min(maxBatchSize, 1000 / columns.Count));

        var columnList = string.Join(", ", columns.Select(c => c.DelimitedColumnName));

        var shouldManageTransaction = _connection.CurrentTransaction is null;
        IDbContextTransaction? transaction = null;

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
                    await ExecuteBatchAsync(batch, delimitedTableName, columnList, columns, cancellationToken).ConfigureAwait(false);
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                await ExecuteBatchAsync(batch, delimitedTableName, columnList, columns, cancellationToken).ConfigureAwait(false);
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
            if (transaction is not null)
            {
                await transaction.DisposeAsync().ConfigureAwait(false);
            }

            await _connection.CloseAsync().ConfigureAwait(false);
        }
    }

    private async Task ExecuteBatchAsync<TEntity>(
        List<TEntity> batch,
        string delimitedTableName,
        string columnList,
        List<PropertyColumn> columns,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var sqlBuilder = new StringBuilder();
        sqlBuilder.Append("INSERT INTO ").Append(delimitedTableName).Append(" (").Append(columnList).Append(") VALUES ");

        var parameters = new List<object?>(batch.Count * columns.Count);
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

                var paramName = "@p" + paramIndex++;
                sqlBuilder.Append(paramName);

                var column = columns[c];
                var val = column.Getter.GetClrValue(entity);
                if (val is not null && column.Converter is not null)
                {
                    val = column.Converter.ConvertToProvider(val);
                }

                parameters.Add(val);
            }

            sqlBuilder.Append(')');
        }

        sqlBuilder.Append(';');

        var rawSqlCommand = _rawSqlCommandBuilder.Build(sqlBuilder.ToString(), parameters);
        await rawSqlCommand.RelationalCommand.ExecuteNonQueryAsync(
            new RelationalCommandParameterObject(
                _connection,
                rawSqlCommand.ParameterValues,
                null,
                _currentDbContext.Context,
                _logger,
                CommandSource.ExecuteSqlRaw),
            cancellationToken).ConfigureAwait(false);
    }
}
