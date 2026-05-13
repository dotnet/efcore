// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Microsoft.Data.SqlClient;

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

    private static readonly ConcurrentDictionary<string, bool> MultipleActiveResultSetsEnabledMap = new();

    /// <summary>
    ///     Maps a user-provided connection string to a possibly transformed connection string that will actually be used
    ///     to connect. By default, if the user-provided connection string doesn't set Application Name, we set it to contain
    ///     information about EF Core and the version being used.
    /// </summary>
    private static readonly ConcurrentDictionary<string, string> ConnectionStringMap = new();

    private static string? _defaultApplicationName;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerConnection(RelationalConnectionDependencies dependencies)
        : base(dependencies)
    {
        // If EF was configured with a connection string that doesn't contain Application Name, rewrite it to inject
        // Application Name with EF and versioning info.
        // Note that we don't do anything if EF was configured with a SqlConnection, as that could reset the
        // password because of Persist Security Info=true
        var relationalOptions = RelationalOptionsExtension.Extract(dependencies.ContextOptions);

        if (!string.IsNullOrWhiteSpace(relationalOptions.ConnectionString))
        {
            Check.DebugAssert(relationalOptions.Connection is null);

            // In the base constructor, the connection string is set by assigning directly to _connectionString
            // (possibly first going through ConnectionStringResolver).
            // Here we re-assign the same via the ConnectionString property, which does the rewriting.
            var userProvidedConnectionString = ConnectionString;
            ConnectionString = userProvidedConnectionString;
        }
    }

    /// <inheritdoc />
    public override string? ConnectionString
    {
        get => base.ConnectionString;
        set => base.ConnectionString = value is null ? null : ConnectionStringMap.GetOrAdd(value, ModifyConnectionString);
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
        => new SqlConnection(GetValidatedConnectionString());

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

    /// <summary>
    ///     Modifies the user-provided connection string. By default, adds an Application Name to the connection
    ///     string containing EF Core and versioning information.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    [EntityFrameworkInternal]
    protected virtual string ModifyConnectionString(string userProvidedConnectionString)
    {
        try
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(userProvidedConnectionString);

            // SqlClient sends "Core Microsoft SqlClient Data Provider" as the default Application Name when unset;
            // detect that and overwrite.
            if (connectionStringBuilder.ApplicationName is "Core Microsoft SqlClient Data Provider" or "" or null)
            {
                var efVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

                _defaultApplicationName ??=
                    $"EFCore/{efVersion} ({RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture})";

                connectionStringBuilder.ApplicationName = _defaultApplicationName;

                return connectionStringBuilder.ToString();
            }
        }
        catch
        {
            // If anything goes wrong, simply don't modify the connection string.
            // There are some scenarios where an invalid string is provided and an exception isn't expected at this phase
            // (see e.g. test GetContextInfo_does_not_throw_if_DbConnection_cannot_be_created)
        }

        return userProvidedConnectionString;
    }
}
