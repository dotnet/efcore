// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable CS1591
#pragma warning disable EF1001

using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerBulkImporter : RelationalBulkImporter
{
    private readonly ISqlServerConnection _sqlServerConnection;

    public SqlServerBulkImporter(
        ISqlServerConnection connection,
        IRelationalCommandBuilderFactory commandBuilderFactory,
        ISqlGenerationHelper sqlGenerationHelper,
        IRelationalCommandDiagnosticsLogger logger,
        ICurrentDbContext currentDbContext)
        : base(connection, commandBuilderFactory, sqlGenerationHelper, logger, currentDbContext)
    {
        _sqlServerConnection = connection;
    }

    protected override bool CanExecuteNativeImport(IEntityType entityType, BulkImportOptions? options)
    {
        if (_sqlServerConnection.DbConnection is not SqlConnection)
        {
            return false;
        }

        var (_, storeObject) = GetTableAndStoreObject(entityType, options);
        return entityType.GetFlattenedProperties().Any(p =>
            p.GetColumnName(storeObject) is not null
            && p.GetComputedColumnSql() is null
            && p.GetBeforeSaveBehavior() != PropertySaveBehavior.Ignore
            && p.ValueGenerated != ValueGenerated.OnAddOrUpdate
            && (options?.KeepIdentity == true || p.GetValueGenerationStrategy(storeObject) != SqlServerValueGenerationStrategy.IdentityColumn));
    }

    protected override void ExecuteNativeImport<TEntity>(
        IEntityType entityType,
        IEnumerable<TEntity> entities,
        BulkImportOptions? options)
    {
        var (destinationTable, storeObject) = GetTableAndStoreObject(entityType, options);
        var keepIdentity = options?.KeepIdentity ?? false;

        using var reader = new EntityDataReader<TEntity>(
            entityType,
            entities,
            storeObject,
            keepIdentity,
            p => p.GetValueGenerationStrategy(storeObject) == SqlServerValueGenerationStrategy.IdentityColumn);

        ExecuteBulkCopy(reader, reader.ColumnNames, destinationTable, options);
    }

    protected override async Task ExecuteNativeImportAsync<TEntity>(
        IEntityType entityType,
        IEnumerable<TEntity> entities,
        BulkImportOptions? options,
        CancellationToken cancellationToken)
    {
        var (destinationTable, storeObject) = GetTableAndStoreObject(entityType, options);
        var keepIdentity = options?.KeepIdentity ?? false;

        var reader = new EntityDataReader<TEntity>(
            entityType,
            entities,
            storeObject,
            keepIdentity,
            p => p.GetValueGenerationStrategy(storeObject) == SqlServerValueGenerationStrategy.IdentityColumn);

        await using (reader.ConfigureAwait(false))
        {
            await ExecuteBulkCopyAsync(reader, reader.ColumnNames, destinationTable, options, cancellationToken).ConfigureAwait(false);
        }
    }

    protected override async Task ExecuteNativeImportAsync<TEntity>(
        IEntityType entityType,
        IAsyncEnumerable<TEntity> entities,
        BulkImportOptions? options,
        CancellationToken cancellationToken)
    {
        var (destinationTable, storeObject) = GetTableAndStoreObject(entityType, options);
        var keepIdentity = options?.KeepIdentity ?? false;

        var reader = new EntityDataReader<TEntity>(
            entityType,
            entities,
            storeObject,
            keepIdentity,
            p => p.GetValueGenerationStrategy(storeObject) == SqlServerValueGenerationStrategy.IdentityColumn,
            cancellationToken);

        await using (reader.ConfigureAwait(false))
        {
            await ExecuteBulkCopyAsync(reader, reader.ColumnNames, destinationTable, options, cancellationToken).ConfigureAwait(false);
        }
    }

    private SqlBulkCopy CreateBulkCopy(
        SqlConnection sqlConnection,
        IReadOnlyList<string> columnNames,
        string destinationTable,
        BulkImportOptions? options)
    {
        var transaction = (SqlTransaction?)_sqlServerConnection.CurrentTransaction?.GetDbTransaction();

        var bulkOptions = SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.KeepNulls;
        if (options?.KeepIdentity == true)
        {
            bulkOptions |= SqlBulkCopyOptions.KeepIdentity;
        }
        if (options?.TableLock == true)
        {
            bulkOptions |= SqlBulkCopyOptions.TableLock;
        }
        if (options?.FireTriggers == true)
        {
            bulkOptions |= SqlBulkCopyOptions.FireTriggers;
        }

        var bulkCopy = new SqlBulkCopy(sqlConnection, bulkOptions, transaction)
        {
            DestinationTableName = destinationTable
        };

        if (options?.BatchSize.HasValue == true)
        {
            bulkCopy.BatchSize = options.BatchSize.Value;
        }

        var effectiveTimeout = GetEffectiveTimeout(options);
        if (effectiveTimeout.HasValue)
        {
            bulkCopy.BulkCopyTimeout = effectiveTimeout.Value;
        }

        foreach (var col in columnNames)
        {
            bulkCopy.ColumnMappings.Add(col, col);
        }

        return bulkCopy;
    }

    private void ExecuteBulkCopy(
        DbDataReader reader,
        IReadOnlyList<string> columnNames,
        string destinationTable,
        BulkImportOptions? options)
    {
        var sqlConnection = (SqlConnection)_sqlServerConnection.DbConnection;
        using var bulkCopy = CreateBulkCopy(sqlConnection, columnNames, destinationTable, options);

        _sqlServerConnection.Open();
        try
        {
            bulkCopy.WriteToServer(reader);
        }
        finally
        {
            _sqlServerConnection.Close();
        }
    }

    private async Task ExecuteBulkCopyAsync(
        DbDataReader reader,
        IReadOnlyList<string> columnNames,
        string destinationTable,
        BulkImportOptions? options,
        CancellationToken cancellationToken)
    {
        var sqlConnection = (SqlConnection)_sqlServerConnection.DbConnection;
        using var bulkCopy = CreateBulkCopy(sqlConnection, columnNames, destinationTable, options);

        await _sqlServerConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await bulkCopy.WriteToServerAsync(reader, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await _sqlServerConnection.CloseAsync().ConfigureAwait(false);
        }
    }
}
