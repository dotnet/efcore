// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Operations;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class SqlServerDatabaseCreator : RelationalDatabaseCreator
    {
        private readonly ISqlServerConnection _connection;
        private readonly IMigrationsSqlGenerator _migrationsSqlGenerator;

        public SqlServerDatabaseCreator(
            [NotNull] ISqlServerConnection connection,
            [NotNull] IMigrationsModelDiffer modelDiffer,
            [NotNull] IMigrationsSqlGenerator migrationsSqlGenerator,
            [NotNull] ISqlStatementExecutor statementExecutor,
            [NotNull] IModel model)
            : base(model, connection, modelDiffer, migrationsSqlGenerator, statementExecutor)
        {
            _connection = connection;
            _migrationsSqlGenerator = migrationsSqlGenerator;
        }

        public override void Create()
        {
            using (var masterConnection = _connection.CreateMasterConnection())
            {
                SqlStatementExecutor.ExecuteNonQuery(masterConnection, CreateCreateOperations());
                ClearPool();
            }

            Exists(retryOnNotExists: true);
        }

        public override async Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var masterConnection = _connection.CreateMasterConnection())
            {
                await SqlStatementExecutor
                    .ExecuteNonQueryAsync(masterConnection, CreateCreateOperations(), cancellationToken);
                ClearPool();
            }

            await ExistsAsync(retryOnNotExists: true, cancellationToken: cancellationToken);
        }

        protected override bool HasTables()
            => (int)SqlStatementExecutor.ExecuteScalar(_connection, CreateHasTablesCommand()) != 0;

        protected override async Task<bool> HasTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
            => (int)(await SqlStatementExecutor
                .ExecuteScalarAsync(_connection, CreateHasTablesCommand(), cancellationToken)) != 0;

        private string CreateHasTablesCommand()
            => "IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE') SELECT 1 ELSE SELECT 0";

        private IEnumerable<RelationalCommand> CreateCreateOperations()
            => _migrationsSqlGenerator.Generate(new[] { new SqlServerCreateDatabaseOperation { Name = _connection.DbConnection.Database } });

        public override bool Exists()
            => Exists(retryOnNotExists: false);

        private bool Exists(bool retryOnNotExists)
        {
            var retryCount = 0;
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

                    if (!RetryOnExistsFailure(e, ref retryCount))
                    {
                        throw;
                    }
                }
            }
        }

        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
            => ExistsAsync(retryOnNotExists: false, cancellationToken: cancellationToken);

        private async Task<bool> ExistsAsync(bool retryOnNotExists, CancellationToken cancellationToken)
        {
            var retryCount = 0;
            while (true)
            {
                try
                {
                    await _connection.OpenAsync(cancellationToken);
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

                    if (!RetryOnExistsFailure(e, ref retryCount))
                    {
                        throw;
                    }
                }
            }
        }

        // Login failed is thrown when database does not exist (See Issue #776)
        private static bool IsDoesNotExist(SqlException exception) => exception.Number == 4060;

        // See Issue #985
        private bool RetryOnExistsFailure(SqlException exception, ref int retryCount)
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
            //   attempting to consume the pre-login handshake acknowledgement.  This could be because the pre-login
            //   handshake failed or the server was unable to respond back in time.
            // And (Number 4060):
            //   System.Data.SqlClient.SqlException: Cannot open database "X" requested by the login. The
            //   login failed.
            if ((exception.Number == 233 || exception.Number == -2 || exception.Number == 4060)
                && ++retryCount < 30)
            {
                ClearPool();
                Thread.Sleep(100);
                return true;
            }
            return false;
        }

        public override void Delete()
        {
            ClearAllPools();

            using (var masterConnection = _connection.CreateMasterConnection())
            {
                SqlStatementExecutor
                    .ExecuteNonQuery(masterConnection, CreateDropCommands());
            }
        }

        public override async Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            ClearAllPools();

            using (var masterConnection = _connection.CreateMasterConnection())
            {
                await SqlStatementExecutor.ExecuteNonQueryAsync(masterConnection, CreateDropCommands(), cancellationToken);
            }
        }

        private IEnumerable<RelationalCommand> CreateDropCommands()
        {
            var operations = new MigrationOperation[]
            {
                // TODO Check DbConnection.Database always gives us what we want
                // Issue #775
                new SqlServerDropDatabaseOperation { Name = _connection.DbConnection.Database }
            };

            var masterCommands = _migrationsSqlGenerator.Generate(operations);
            return masterCommands;
        }

        // Clear connection pools in case there are active connections that are pooled
        private static void ClearAllPools() => SqlConnection.ClearAllPools();

        // Clear connection pool for the database connection since after the 'create database' call, a previously
        // invalid connection may now be valid.
        private void ClearPool() => SqlConnection.ClearPool((SqlConnection)_connection.DbConnection);
    }
}
