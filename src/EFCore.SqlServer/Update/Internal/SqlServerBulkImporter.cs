// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable CS1591

using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerBulkImporter : IBulkImporter
{
    private readonly ISqlServerConnection _connection;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerBulkImporter(ISqlServerConnection connection)
    {
        _connection = connection;
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

        var (destinationTable, storeObject) = GetTableAndStoreObject(entityType);
        var keepIdentity = options?.KeepIdentity ?? false;

        var reader = new EntityDataReader<TEntity>(entityType, entities, storeObject, keepIdentity);
        await using (reader.ConfigureAwait(false))
        {
            await ExecuteBulkCopyAsync(reader, reader.ColumnNames, destinationTable, options, cancellationToken).ConfigureAwait(false);
        }
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

        var (destinationTable, storeObject) = GetTableAndStoreObject(entityType);
        var keepIdentity = options?.KeepIdentity ?? false;

        var reader = new EntityDataReader<TEntity>(entityType, entities, storeObject, keepIdentity, cancellationToken);
        await using (reader.ConfigureAwait(false))
        {
            await ExecuteBulkCopyAsync(reader, reader.ColumnNames, destinationTable, options, cancellationToken).ConfigureAwait(false);
        }
    }

    private static (string DestinationTable, StoreObjectIdentifier StoreObject) GetTableAndStoreObject(IEntityType entityType)
    {
        var tableName = entityType.GetTableName();
        if (tableName is null)
        {
            throw new InvalidOperationException($"The entity type '{entityType.DisplayName()}' is not mapped to a table.");
        }

        var schema = entityType.GetSchema();
        var storeObject = StoreObjectIdentifier.Table(tableName, schema);
        var destinationTable = schema is not null ? $"[{schema.Replace("]", "]]")}].[{tableName.Replace("]", "]]")}]" : $"[{tableName.Replace("]", "]]")}]";

        return (destinationTable, storeObject);
    }

    private async Task ExecuteBulkCopyAsync(
        DbDataReader reader,
        IReadOnlyList<string> columnNames,
        string destinationTable,
        BulkImportOptions? options,
        CancellationToken cancellationToken)
    {
        var sqlConnection = (SqlConnection)_connection.DbConnection;
        var transaction = (SqlTransaction?)_connection.CurrentTransaction?.GetDbTransaction();

        var bulkOptions = SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.KeepNulls;
        if (options?.KeepIdentity == true)
        {
            bulkOptions |= SqlBulkCopyOptions.KeepIdentity;
        }

        using var bulkCopy = new SqlBulkCopy(sqlConnection, bulkOptions, transaction);
        bulkCopy.DestinationTableName = destinationTable;

        if (options?.BatchSize.HasValue == true)
        {
            bulkCopy.BatchSize = options.BatchSize.Value;
        }

        if (options?.Timeout.HasValue == true)
        {
            bulkCopy.BulkCopyTimeout = options.Timeout.Value;
        }

        foreach (var col in columnNames)
        {
            bulkCopy.ColumnMappings.Add(col, col);
        }

        await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await bulkCopy.WriteToServerAsync(reader, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await _connection.CloseAsync().ConfigureAwait(false);
        }
    }
}
