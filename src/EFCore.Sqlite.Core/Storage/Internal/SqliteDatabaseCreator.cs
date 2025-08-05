// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.Data.Sqlite;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteDatabaseCreator : RelationalDatabaseCreator
{
    // ReSharper disable once InconsistentNaming
    private const int SQLITE_CANTOPEN = 14;

    private readonly ISqliteRelationalConnection _connection;
    private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteDatabaseCreator(
        RelationalDatabaseCreatorDependencies dependencies,
        ISqliteRelationalConnection connection,
        IRawSqlCommandBuilder rawSqlCommandBuilder)
        : base(dependencies)
    {
        _connection = connection;
        _rawSqlCommandBuilder = rawSqlCommandBuilder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void Create()
    {
        Dependencies.Connection.Open();

        _rawSqlCommandBuilder.Build("PRAGMA journal_mode = 'wal';")
            .ExecuteNonQuery(
                new RelationalCommandParameterObject(
                    Dependencies.Connection,
                    null,
                    null,
                    null,
                    Dependencies.CommandLogger, CommandSource.Migrations));

        Dependencies.Connection.Close();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool Exists()
    {
        var connectionOptions = new SqliteConnectionStringBuilder(_connection.ConnectionString);
        if (connectionOptions.DataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase)
            || connectionOptions.Mode == SqliteOpenMode.Memory)
        {
            return true;
        }

        using var readOnlyConnection = _connection.CreateReadOnlyConnection();
        try
        {
            readOnlyConnection.Open(errorsExpected: true);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == SQLITE_CANTOPEN)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool HasTables()
    {
        var count = (long)_rawSqlCommandBuilder
            .Build("SELECT COUNT(*) FROM \"sqlite_master\" WHERE \"type\" = 'table' AND \"rootpage\" IS NOT NULL;")
            .ExecuteScalar(
                new RelationalCommandParameterObject(
                    Dependencies.Connection,
                    null,
                    null,
                    null,
                    Dependencies.CommandLogger, CommandSource.Migrations))!;

        return count != 0;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void Delete()
    {
        string? path = null;

        Dependencies.Connection.Open();
        var dbConnection = Dependencies.Connection.DbConnection;
        try
        {
            path = dbConnection.DataSource;
        }
        catch
        {
            // any exceptions here can be ignored
        }
        finally
        {
            Dependencies.Connection.Close();
        }

        if (!string.IsNullOrEmpty(path))
        {
            SqliteConnection.ClearAllPools();
            File.Delete(path);
        }
        else if (dbConnection.State == ConnectionState.Open)
        {
            dbConnection.Close();
            SqliteConnection.ClearPool(new SqliteConnection(Dependencies.Connection.ConnectionString));
            dbConnection.Open();
        }
    }
}
