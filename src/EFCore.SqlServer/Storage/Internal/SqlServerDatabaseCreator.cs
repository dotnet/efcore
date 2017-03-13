// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerDatabaseCreator : RelationalDatabaseCreator
    {
        private readonly ISqlServerConnection _connection;
        private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;

        public SqlServerDatabaseCreator(
            [NotNull] RelationalDatabaseCreatorDependencies dependencies,
            [NotNull] ISqlServerConnection connection,
            [NotNull] IRawSqlCommandBuilder rawSqlCommandBuilder)
            : base(dependencies)
        {
            _connection = connection;
            _rawSqlCommandBuilder = rawSqlCommandBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(500);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TimeSpan RetryTimeout { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void Create()
        {
            using (var masterConnection = _connection.CreateMasterConnection())
            {
                Dependencies.MigrationCommandExecutor
                    .ExecuteNonQuery(CreateCreateOperations(), masterConnection);

                ClearPool();
            }

            Exists(retryOnNotExists: true);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override async Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var masterConnection = _connection.CreateMasterConnection())
            {
                await Dependencies.MigrationCommandExecutor
                    .ExecuteNonQueryAsync(CreateCreateOperations(), masterConnection, cancellationToken);

                ClearPool();
            }

            await ExistsAsync(retryOnNotExists: true, cancellationToken: cancellationToken);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool HasTables()
            => Dependencies.ExecutionStrategyFactory.Create().Execute(
                connection => (int)CreateHasTablesCommand().ExecuteScalar(connection) != 0,
                _connection);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Task<bool> HasTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
            => Dependencies.ExecutionStrategyFactory.Create().ExecuteAsync(
                async (connection, ct) => (int)await CreateHasTablesCommand().ExecuteScalarAsync(connection, cancellationToken: ct) != 0,
                _connection,
                cancellationToken);

        private IRelationalCommand CreateHasTablesCommand()
            => _rawSqlCommandBuilder
                .Build("IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE') SELECT 1 ELSE SELECT 0");

        private IReadOnlyList<MigrationCommand> CreateCreateOperations()
        {
            var builder = new SqlConnectionStringBuilder(_connection.DbConnection.ConnectionString);
            return Dependencies.MigrationsSqlGenerator.Generate(new[] { new SqlServerCreateDatabaseOperation { Name = builder.InitialCatalog, FileName = builder.AttachDBFilename } });
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool Exists()
            => Exists(retryOnNotExists: false);

        private bool Exists(bool retryOnNotExists)
            => Dependencies.ExecutionStrategyFactory.Create().Execute(
                giveUp =>
                    {
                        while (true)
                        {
                            try
                            {
                                _connection.Open();
                                _connection.Close();
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
                        }
                    }, DateTime.UtcNow + RetryTimeout);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
            => ExistsAsync(retryOnNotExists: false, cancellationToken: cancellationToken);

        private Task<bool> ExistsAsync(bool retryOnNotExists, CancellationToken cancellationToken)
            => Dependencies.ExecutionStrategyFactory.Create().ExecuteAsync(
                async (giveUp, ct) =>
                    {
                        while (true)
                        {
                            try
                            {
                                await _connection.OpenAsync(ct);

                                _connection.Close();
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

                                await Task.Delay(RetryDelay, ct);
                            }
                        }
                    }, DateTime.UtcNow + RetryTimeout, cancellationToken);

        // Login failed is thrown when database does not exist (See Issue #776)
        // Unable to attach database file is thrown when file does not exist (See Issue #2810)
        // Unable to open the physical file is thrown when file does not exist (See Issue #2810)
        private static bool IsDoesNotExist(SqlException exception) =>
            exception.Number == 4060 || exception.Number == 1832 || exception.Number == 5120;

        // See Issue #985
        private bool RetryOnExistsFailure(SqlException exception)
        {
            // This is to handle the case where Open throws (Number 233):
            //   System.Data.SqlClient.SqlException: A connection was successfully established with the
            //   server, but then an error occurred during the login process. (provider: Named Pipes
            //   Provider, error: 0 - No process is on the other end of the pipe.)
            // It appears that this happens when the database has just been created but has not yet finished
            // opening or is auto-closing when using the AUTO_CLOSE option. The workaround is to flush the pool
            // for the connection and then retry the Open call.
            // Also handling (Number -2):
            //   System.Data.SqlClient.SqlException: Connection Timeout Expired.  The timeout period elapsed while
            //   attempting to consume the pre-login handshake acknowledgment.  This could be because the pre-login
            //   handshake failed or the server was unable to respond back in time.
            // And (Number 4060):
            //   System.Data.SqlClient.SqlException: Cannot open database "X" requested by the login. The
            //   login failed.
            // And (Number 1832)
            //   System.Data.SqlClient.SqlException: Unable to Attach database file as database xxxxxxx.
            // And (Number 5120)
            //   System.Data.SqlClient.SqlException: Unable to open the physical file xxxxxxx.
            if ((exception.Number == 233
                 || exception.Number == -2
                 || exception.Number == 4060
                 || exception.Number == 1832
                 || exception.Number == 5120))
            {
                ClearPool();
                return true;
            }
            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void Delete()
        {
            ClearAllPools();

            using (var masterConnection = _connection.CreateMasterConnection())
            {
                Dependencies.MigrationCommandExecutor
                    .ExecuteNonQuery(CreateDropCommands(), masterConnection);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override async Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            ClearAllPools();

            using (var masterConnection = _connection.CreateMasterConnection())
            {
                await Dependencies.MigrationCommandExecutor
                    .ExecuteNonQueryAsync(CreateDropCommands(), masterConnection, cancellationToken);
            }
        }

        private IReadOnlyList<MigrationCommand> CreateDropCommands()
        {
            var operations = new MigrationOperation[]
            {
                // TODO Check DbConnection.Database always gives us what we want
                // Issue #775
                new SqlServerDropDatabaseOperation { Name = _connection.DbConnection.Database }
            };

            var masterCommands = Dependencies.MigrationsSqlGenerator.Generate(operations);
            return masterCommands;
        }

        // Clear connection pools in case there are active connections that are pooled
        private static void ClearAllPools() => SqlConnection.ClearAllPools();

        // Clear connection pool for the database connection since after the 'create database' call, a previously
        // invalid connection may now be valid.
        private void ClearPool() => SqlConnection.ClearPool((SqlConnection)_connection.DbConnection);
    }
}
