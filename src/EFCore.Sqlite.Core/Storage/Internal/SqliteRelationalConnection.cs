// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteRelationalConnection : RelationalConnection, ISqliteRelationalConnection
{
    private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
    private readonly IDiagnosticsLogger<DbLoggerCategory.Infrastructure> _logger;
    private readonly bool _loadSpatialite;
    private readonly int? _commandTimeout;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteRelationalConnection(
        RelationalConnectionDependencies dependencies,
        IRawSqlCommandBuilder rawSqlCommandBuilder,
        IDiagnosticsLogger<DbLoggerCategory.Infrastructure> logger)
        : base(dependencies)
    {
        _rawSqlCommandBuilder = rawSqlCommandBuilder;
        _logger = logger;

        var optionsExtension = dependencies.ContextOptions.Extensions.OfType<SqliteOptionsExtension>().FirstOrDefault();
        if (optionsExtension != null)
        {
            _loadSpatialite = optionsExtension.LoadSpatialite;

            var relationalOptions = RelationalOptionsExtension.Extract(dependencies.ContextOptions);
            _commandTimeout = relationalOptions.CommandTimeout;

            if (relationalOptions.Connection != null)
            {
                InitializeDbConnection(relationalOptions.Connection);
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override DbConnection CreateDbConnection()
    {
        var connection = new SqliteConnection(GetValidatedConnectionString());
        InitializeDbConnection(connection);

        return connection;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ISqliteRelationalConnection CreateReadOnlyConnection()
    {
        var connectionStringBuilder = new SqliteConnectionStringBuilder(GetValidatedConnectionString())
        {
            Mode = SqliteOpenMode.ReadOnly, Pooling = false
        };

        var contextOptions = new DbContextOptionsBuilder().UseSqlite(connectionStringBuilder.ToString()).Options;

        return new SqliteRelationalConnection(Dependencies with { ContextOptions = contextOptions }, _rawSqlCommandBuilder, _logger);
    }

    private void InitializeDbConnection(DbConnection connection)
    {
        if (_loadSpatialite)
        {
            SpatialiteLoader.Load(connection);
        }

        if (connection is SqliteConnection sqliteConnection)
        {
            if (_commandTimeout.HasValue)
            {
                sqliteConnection.DefaultTimeout = _commandTimeout.Value;
            }

            sqliteConnection.CreateFunction<string, string, bool?>(
                "regexp",
                (pattern, input) =>
                {
                    if (input == null
                        || pattern == null)
                    {
                        return null;
                    }

                    return Regex.IsMatch(input, pattern);
                },
                isDeterministic: true);

            sqliteConnection.CreateFunction(
                "ef_mod",
                (decimal? dividend, decimal? divisor) => dividend % divisor,
                isDeterministic: true);

            sqliteConnection.CreateFunction(
                name: "ef_add",
                (decimal? left, decimal? right) => left + right,
                isDeterministic: true);

            sqliteConnection.CreateFunction(
                name: "ef_divide",
                (decimal? dividend, decimal? divisor) => dividend / divisor,
                isDeterministic: true);

            sqliteConnection.CreateFunction(
                name: "ef_compare",
                (decimal? left, decimal? right) => left.HasValue && right.HasValue
                    ? decimal.Compare(left.Value, right.Value)
                    : default(int?),
                isDeterministic: true);

            sqliteConnection.CreateFunction(
                name: "ef_multiply",
                (decimal? left, decimal? right) => left * right,
                isDeterministic: true);

            sqliteConnection.CreateFunction(
                name: "ef_negate",
                (decimal? m) => -m,
                isDeterministic: true);
        }
        else
        {
            _logger.UnexpectedConnectionTypeWarning(connection.GetType());
        }
    }
}
