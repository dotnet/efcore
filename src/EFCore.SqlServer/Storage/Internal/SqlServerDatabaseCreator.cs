// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class SqlServerDatabaseCreator : RelationalDatabaseCreator
    {
        private readonly ISqlServerConnection _connection;
        private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerDatabaseCreator(
            RelationalDatabaseCreatorDependencies relationalDependencies,
            ISqlServerConnection connection,
            IRawSqlCommandBuilder rawSqlCommandBuilder)
            : base(relationalDependencies)
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
        public virtual TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(500);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual TimeSpan RetryTimeout { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void Create()
        {
            using (var masterConnection = _connection.CreateMasterConnection())
            {
                RelationalDependencies.MigrationCommandExecutor
                    .ExecuteNonQuery(CreateCreateOperations(), masterConnection);

                ClearPool();
            }

            Exists(retryOnNotExists: true);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override async Task CreateAsync(CancellationToken cancellationToken = default)
        {
            using (var masterConnection = _connection.CreateMasterConnection())
            {
                await RelationalDependencies.MigrationCommandExecutor
                    .ExecuteNonQueryAsync(CreateCreateOperations(), masterConnection, cancellationToken)
                    .ConfigureAwait(false);

                ClearPool();
            }

            await ExistsAsync(retryOnNotExists: true, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool HasTables()
            => RelationalDependencies.ExecutionStrategyFactory
                .Create()
                .Execute(
                    _connection,
                    connection => (int)CreateHasTablesCommand()
                            .ExecuteScalar(
                                new RelationalCommandParameterObject(
                                    connection,
                                    null,
                                    null,
                                    RelationalDependencies.CurrentContext.Context,
                                    RelationalDependencies.CommandLogger, CommandSource.Migrations))!
                        != 0);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Task<bool> HasTablesAsync(CancellationToken cancellationToken = default)
            => RelationalDependencies.ExecutionStrategyFactory.Create().ExecuteAsync(
                _connection,
                async (connection, ct) => (int)(await CreateHasTablesCommand()
                        .ExecuteScalarAsync(
                            new RelationalCommandParameterObject(
                                connection,
                                null,
                                null,
                                RelationalDependencies.CurrentContext.Context,
                                RelationalDependencies.CommandLogger, CommandSource.Migrations),
                            cancellationToken: ct)
                        .ConfigureAwait(false))!
                    != 0, cancellationToken);

        private IRelationalCommand CreateHasTablesCommand()
            => _rawSqlCommandBuilder
                .Build(
                    @"
IF EXISTS
    (SELECT *
     FROM [sys].[objects] o
     WHERE [o].[type] = 'U'
     AND [o].[is_ms_shipped] = 0
     AND NOT EXISTS (SELECT *
         FROM [sys].[extended_properties] AS [ep]
         WHERE [ep].[major_id] = [o].[object_id]
             AND [ep].[minor_id] = 0
             AND [ep].[class] = 1
             AND [ep].[name] = N'microsoft_database_tools_support'
    )
)
SELECT 1 ELSE SELECT 0");

        private IReadOnlyList<MigrationCommand> CreateCreateOperations()
        {
            var builder = new SqlConnectionStringBuilder(_connection.DbConnection.ConnectionString);
            return RelationalDependencies.MigrationsSqlGenerator.Generate(
                new[]
                {
                    new SqlServerCreateDatabaseOperation
                    {
                        Name = builder.InitialCatalog,
                        FileName = builder.AttachDBFilename,
                        Collation = RelationalDependencies.CurrentContext.Context.GetService<IDesignTimeModel>()
                            .Model.GetRelationalModel().Collation
                    }
                },
                null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool Exists()
            => Exists(retryOnNotExists: false);

        private bool Exists(bool retryOnNotExists)
            => RelationalDependencies.ExecutionStrategyFactory.Create().Execute(
                DateTime.UtcNow + RetryTimeout, giveUp =>
                {
                    while (true)
                    {
                        var opened = false;
                        try
                        {
                            using var _ = new TransactionScope(TransactionScopeOption.Suppress);
                            _connection.Open(errorsExpected: true);
                            opened = true;

                            _rawSqlCommandBuilder
                                .Build("SELECT 1")
                                .ExecuteNonQuery(
                                    new RelationalCommandParameterObject(
                                        _connection,
                                        null,
                                        null,
                                        RelationalDependencies.CurrentContext.Context,
                                        RelationalDependencies.CommandLogger, CommandSource.Migrations));

                            return true;
                        }
                        catch (SqlException e)
                        {
                            if (!retryOnNotExists
                                && IsDoesNotExist(e))
                            {
                                return false;
                            }

                            if (DateTime.UtcNow > giveUp
                                || !RetryOnExistsFailure(e))
                            {
                                throw;
                            }

                            Thread.Sleep(RetryDelay);
                        }
                        finally
                        {
                            if (opened)
                            {
                                _connection.Close();
                            }
                        }
                    }
                });

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
            => ExistsAsync(retryOnNotExists: false, cancellationToken: cancellationToken);

        private Task<bool> ExistsAsync(bool retryOnNotExists, CancellationToken cancellationToken)
            => RelationalDependencies.ExecutionStrategyFactory.Create().ExecuteAsync(
                DateTime.UtcNow + RetryTimeout, async (giveUp, ct) =>
                {
                    while (true)
                    {
                        var opened = false;

                        try
                        {
                            using var _ = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);
                            await _connection.OpenAsync(ct, errorsExpected: true).ConfigureAwait(false);
                            opened = true;

                            await _rawSqlCommandBuilder
                                .Build("SELECT 1")
                                .ExecuteNonQueryAsync(
                                    new RelationalCommandParameterObject(
                                        _connection,
                                        null,
                                        null,
                                        RelationalDependencies.CurrentContext.Context,
                                        RelationalDependencies.CommandLogger, CommandSource.Migrations),
                                    ct)
                                .ConfigureAwait(false);

                            return true;
                        }
                        catch (SqlException e)
                        {
                            if (!retryOnNotExists
                                && IsDoesNotExist(e))
                            {
                                return false;
                            }

                            if (DateTime.UtcNow > giveUp
                                || !RetryOnExistsFailure(e))
                            {
                                throw;
                            }

                            await Task.Delay(RetryDelay, ct).ConfigureAwait(false);
                        }
                        finally
                        {
                            if (opened)
                            {
                                await _connection.CloseAsync().ConfigureAwait(false);
                            }
                        }
                    }
                }, cancellationToken);

        // Login failed is thrown when database does not exist (See Issue #776)
        // Unable to attach database file is thrown when file does not exist (See Issue #2810)
        // Unable to open the physical file is thrown when file does not exist (See Issue #2810)
        private static bool IsDoesNotExist(SqlException exception)
            => exception.Number == 4060 || exception.Number == 1832 || exception.Number == 5120;

        // See Issue #985
        private bool RetryOnExistsFailure(SqlException exception)
        {
            // This is to handle the case where Open throws (Number 233):
            //   Microsoft.Data.SqlClient.SqlException: A connection was successfully established with the
            //   server, but then an error occurred during the login process. (provider: Named Pipes
            //   Provider, error: 0 - No process is on the other end of the pipe.)
            // It appears that this happens when the database has just been created but has not yet finished
            // opening or is auto-closing when using the AUTO_CLOSE option. The workaround is to flush the pool
            // for the connection and then retry the Open call.
            // Also handling (Number -2):
            //   Microsoft.Data.SqlClient.SqlException: Connection Timeout Expired.  The timeout period elapsed while
            //   attempting to consume the pre-login handshake acknowledgment.  This could be because the pre-login
            //   handshake failed or the server was unable to respond back in time.
            // And (Number 4060):
            //   Microsoft.Data.SqlClient.SqlException: Cannot open database "X" requested by the login. The
            //   login failed.
            // And (Number 1832)
            //   Microsoft.Data.SqlClient.SqlException: Unable to Attach database file as database xxxxxxx.
            // And (Number 5120)
            //   Microsoft.Data.SqlClient.SqlException: Unable to open the physical file xxxxxxx.
            if (exception.Number == 233
                || exception.Number == -2
                || exception.Number == 4060
                || exception.Number == 1832
                || exception.Number == 5120)
            {
                ClearPool();
                return true;
            }

            return false;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void Delete()
        {
            ClearAllPools();

            using var masterConnection = _connection.CreateMasterConnection();
            RelationalDependencies.MigrationCommandExecutor
                .ExecuteNonQuery(CreateDropCommands(), masterConnection);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override async Task DeleteAsync(CancellationToken cancellationToken = default)
        {
            ClearAllPools();

            using var masterConnection = _connection.CreateMasterConnection();
            await RelationalDependencies.MigrationCommandExecutor
                .ExecuteNonQueryAsync(CreateDropCommands(), masterConnection, cancellationToken)
                .ConfigureAwait(false);
        }

        private IReadOnlyList<MigrationCommand> CreateDropCommands()
        {
            var databaseName = _connection.DbConnection.Database;
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new InvalidOperationException(SqlServerStrings.NoInitialCatalog);
            }

            var operations = new MigrationOperation[] { new SqlServerDropDatabaseOperation { Name = databaseName } };

            return RelationalDependencies.MigrationsSqlGenerator.Generate(operations, null);
        }

        // Clear connection pools in case there are active connections that are pooled
        private static void ClearAllPools()
            => SqlConnection.ClearAllPools();

        // Clear connection pool for the database connection since after the 'create database' call, a previously
        // invalid connection may now be valid.
        private void ClearPool()
            => SqlConnection.ClearPool((SqlConnection)_connection.DbConnection);
    }
}
