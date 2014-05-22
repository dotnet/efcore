// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.SqlServer.Utilities;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerDataStoreCreator : DataStoreCreator
    {
        private readonly SqlServerConnection _connection;
        private readonly ModelDiffer _modelDiffer;
        private readonly SqlServerMigrationOperationSqlGenerator _sqlGenerator;
        private readonly SqlStatementExecutor _statementExecutor;

        public SqlServerDataStoreCreator(
            [NotNull] SqlServerConnection connection,
            [NotNull] ModelDiffer modelDiffer,
            [NotNull] SqlServerMigrationOperationSqlGenerator sqlGenerator,
            [NotNull] SqlStatementExecutor statementExecutor)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(modelDiffer, "modelDiffer");
            Check.NotNull(sqlGenerator, "sqlGenerator");
            Check.NotNull(statementExecutor, "statementExecutor");

            _connection = connection;
            _modelDiffer = modelDiffer;
            _sqlGenerator = sqlGenerator;
            _statementExecutor = statementExecutor;
        }

        public override void Create()
        {
            using (var masterConnection = _connection.CreateMasterConnection())
            {
                _statementExecutor.ExecuteNonQuery(masterConnection, CreateCreateOperations());
                ClearPool();
            }
        }

        public override async Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var masterConnection = _connection.CreateMasterConnection())
            {
                await _statementExecutor.ExecuteNonQueryAsync(masterConnection, CreateCreateOperations(), cancellationToken);
                ClearPool();
            }
        }

        public override void CreateTables(IModel model)
        {
            Check.NotNull(model, "model");

            _statementExecutor.ExecuteNonQuery(_connection.DbConnection, CreateSchemaCommands(model));
        }

        public override async Task CreateTablesAsync(IModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(model, "model");

            await _statementExecutor.ExecuteNonQueryAsync(_connection.DbConnection, CreateSchemaCommands(model), cancellationToken);
        }

        public override bool HasTables()
        {
            return (int)_statementExecutor.ExecuteScalar(_connection.DbConnection, CreateHasTablesCommand()) != 0;
        }

        public override async Task<bool> HasTablesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return (int)(await _statementExecutor.ExecuteScalarAsync(_connection.DbConnection, CreateHasTablesCommand(), cancellationToken)) != 0;
        }

        private IEnumerable<SqlStatement> CreateSchemaCommands(IModel model)
        {
            return _sqlGenerator.Generate(_modelDiffer.DiffSource(model), generateIdempotentSql: false);
        }

        private SqlStatement CreateHasTablesCommand()
        {
            return new SqlStatement("IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES) SELECT 1 ELSE SELECT 0");
        }

        private IEnumerable<SqlStatement> CreateCreateOperations()
        {
            // TODO Check DbConnection.Database always gives us what we want
            var databaseName = _connection.DbConnection.Database;

            var operations = new MigrationOperation[]
                {
                    new CreateDatabaseOperation(databaseName)
                };

            var masterCommands = _sqlGenerator.Generate(operations, generateIdempotentSql: false);
            return masterCommands;
        }

        public override bool Exists()
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
                    if (IsDoesNotExist(e))
                    {
                        return false;
                    }

                    if (!RetryOnNoProcessOnEndOfPipe(e, ref retryCount))
                    {
                        throw;
                    }
                }
            }
        }

        public override async Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
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
                    if (IsDoesNotExist(e))
                    {
                        return false;
                    }

                    if (!RetryOnNoProcessOnEndOfPipe(e, ref retryCount))
                    {
                        throw;
                    }
                }
            }
        }

        private static bool IsDoesNotExist(SqlException exception)
        {
            // TODO Explore if there are important scenarios where this could give a false negative
            // Login failed is thrown when database does not exist
            return exception.Number == 4060;
        }

        private bool RetryOnNoProcessOnEndOfPipe(SqlException exception, ref int retryCount)
        {
            // This is to handle the case where Open throws:
            //   System.Data.SqlClient.SqlException : A connection was successfully established with the
            //   server, but then an error occurred during the login process. (provider: Named Pipes
            //   Provider, error: 0 - No process is on the other end of the pipe.)
            // It appears that this happens when the database has just been created but has not yet finished
            // opening or is auto-closing when using the AUTO_CLOSE option. The workaround is to flush the pool
            // for the connection and then retry the Open call.
            if (exception.Number == 233
                && retryCount++ < 3)
            {
                ClearPool();
                return true;
            }
            return false;
        }

        public override void Delete()
        {
            ClearAllPools();

            using (var masterConnection = _connection.CreateMasterConnection())
            {
                _statementExecutor.ExecuteNonQuery(masterConnection, CreateDropCommands());
            }
        }

        public override async Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            ClearAllPools();

            using (var masterConnection = _connection.CreateMasterConnection())
            {
                await _statementExecutor.ExecuteNonQueryAsync(masterConnection, CreateDropCommands(), cancellationToken);
            }
        }

        private IEnumerable<SqlStatement> CreateDropCommands()
        {
            var operations = new MigrationOperation[]
                {
                    // TODO Check DbConnection.Database always gives us what we want
                    new DropDatabaseOperation(_connection.DbConnection.Database)
                };

            var masterCommands = _sqlGenerator.Generate(operations, generateIdempotentSql: false);
            return masterCommands;
        }

        private static void ClearAllPools()
        {
            // Clear connection pools in case there are active connections that are pooled
            SqlConnection.ClearAllPools();
        }

        private void ClearPool()
        {
            // Clear connection pool for the database connection since after the 'create database' call, a previously
            // invalid connection may now be valid.
            SqlConnection.ClearPool((SqlConnection)_connection.DbConnection);
        }
    }
}
