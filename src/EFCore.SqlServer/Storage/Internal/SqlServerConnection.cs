// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerConnection : RelationalConnection, ISqlServerConnection
{
    // Compensate for slow SQL Server database creation
    private const int DefaultMasterConnectionCommandTimeout = 60;
    private const string NetSqlClientDefaultApplicationName = ".NET SqlClient Data Provider";
    private const string CoreSqlClientDefaultApplicationName = "Core Microsoft SqlClient Data Provider";

    private static readonly ConcurrentDictionary<string, bool> MultipleActiveResultSetsEnabledMap = new();
    private static readonly string DefaultApplicationName = "EFCore/" + ProductInfo.GetVersion();

    static SqlServerConnection()
    {
        // Enable SqlClient's native UserAgent TDS Feature Extension, which sends driver version and
        // environment information to the server during login.
        AppContext.SetSwitch("Switch.Microsoft.Data.SqlClient.EnableUserAgent", true);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerConnection(RelationalConnectionDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void OpenDbConnection(bool errorsExpected)
    {
        if (errorsExpected
            && DbConnection is SqlConnection sqlConnection)
        {
            sqlConnection.Open(SqlConnectionOverrides.OpenWithoutRetry);
        }
        else
        {
            DbConnection.Open();
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Task OpenDbConnectionAsync(bool errorsExpected, CancellationToken cancellationToken)
    {
        if (errorsExpected
            && DbConnection is SqlConnection sqlConnection)
        {
            return sqlConnection.OpenAsync(SqlConnectionOverrides.OpenWithoutRetry, cancellationToken);
        }

        return DbConnection.OpenAsync(cancellationToken);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override DbConnection CreateDbConnection()
    {
        var connectionString = GetValidatedConnectionString();

        try
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

            // SqlClient assigns one of these defaults when Application Name isn't set by the caller.
            if (connectionStringBuilder.ApplicationName is NetSqlClientDefaultApplicationName or CoreSqlClientDefaultApplicationName or "" or null)
            {
                connectionStringBuilder.ApplicationName = DefaultApplicationName;
                connectionString = connectionStringBuilder.ConnectionString;
            }
        }
        catch
        {
            // If anything goes wrong, simply don't modify the connection string.
            // There are some scenarios where an invalid string is provided and an exception isn't expected at this phase
            // (see e.g. test GetContextInfo_does_not_throw_if_DbConnection_cannot_be_created)
        }

        return new SqlConnection(connectionString);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ISqlServerConnection CreateMasterConnection()
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder(GetValidatedConnectionString()) { InitialCatalog = "master" };
        connectionStringBuilder.Remove("AttachDBFilename");

        var contextOptions = new DbContextOptionsBuilder()
            .UseSqlServer(
                connectionStringBuilder.ConnectionString,
                b => b.CommandTimeout(CommandTimeout ?? DefaultMasterConnectionCommandTimeout))
            .Options;

        return new SqlServerConnection(Dependencies with { ContextOptions = contextOptions });
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsMultipleActiveResultSetsEnabled
    {
        get
        {
            var connectionString = ConnectionString;

            return connectionString != null
                && MultipleActiveResultSetsEnabledMap.GetOrAdd(
                    connectionString, cs => new SqlConnectionStringBuilder(cs).MultipleActiveResultSets);
        }
    }

    /// <summary>
    ///     Indicates whether the store connection supports ambient transactions
    /// </summary>
    protected override bool SupportsAmbientTransactions
        => true;
}
