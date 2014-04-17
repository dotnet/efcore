// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Migrations;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational;
using Microsoft.Data.SqlServer.Utilities;
#if NET45
using System.Data.SqlClient;
#endif

namespace Microsoft.Data.SqlServer
{
    public class SqlServerDataStoreCreator : DataStoreCreator
    {
        private readonly SqlServerDataStore _dataStore;
        private readonly ModelDiffer _modelDiffer;
        private readonly MigrationOperationSqlGenerator _sqlGenerator;
        private readonly SqlStatementExecutor _statementExecutor;

        public SqlServerDataStoreCreator(
            [NotNull] SqlServerDataStore dataStore,
            [NotNull] ModelDiffer modelDiffer,
            [NotNull] MigrationOperationSqlGenerator sqlGenerator,
            [NotNull] SqlStatementExecutor statementExecutor)
        {
            Check.NotNull(dataStore, "dataStore");
            Check.NotNull(modelDiffer, "modelDiffer");
            Check.NotNull(sqlGenerator, "sqlGenerator");
            Check.NotNull(statementExecutor, "statementExecutor");

            _dataStore = dataStore;
            _modelDiffer = modelDiffer;
            _sqlGenerator = sqlGenerator;
            _statementExecutor = statementExecutor;
        }

        public override async Task CreateAsync(IModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = _dataStore.CreateConnection())
            {
                using (var masterConnection = _dataStore.CreateMasterConnection())
                {
                    var operations = new MigrationOperation[]
                        {
                            // TODO Check DbConnection.Database always gives us what we want
                            new CreateDatabaseOperation(connection.Database)
                        };

                    var masterCommands = _sqlGenerator.Generate(operations, generateIdempotentSql: true);
                    await _statementExecutor.ExecuteNonQueryAsync(masterConnection, masterCommands, cancellationToken);

#if NET45
                    // Clear connection pool for the database connection since after the 'create database' call, a previously
                    // invalid connection may now be valid.
                    SqlConnection.ClearPool((SqlConnection)connection);
#endif
                }

                var schemaOperations = _modelDiffer.DiffSource(model);
                var schemaCommands = _sqlGenerator.Generate(schemaOperations, generateIdempotentSql: false);
                await _statementExecutor.ExecuteNonQueryAsync(connection, schemaCommands, cancellationToken);
            }
        }

        public override async Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = _dataStore.CreateConnection())
            {
                try
                {
                    await connection.OpenAsync(cancellationToken);
                    connection.Close();
                    return true;
                }
#if NET45
                catch (SqlException e)
                {
                    // TODO Explore if there are important scenarios where this could give a false negative
                    // Login failed is thrown when database does not exist
                    if (e.Number == 4060)
                    {
                        return false;
                    }

                    throw;
                }
#else
                catch
                {
                    throw;
                }
#endif
            }
        }

        public override async Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            string database;
            using (var connection = _dataStore.CreateConnection())
            {
                // TODO Check DbConnection.Database always gives us what we want
                database = connection.Database;
            }

#if NET45
            // Clear connection pools in case there are active connections that are pooled
            SqlConnection.ClearAllPools();
#endif

            using (var masterConnection = _dataStore.CreateMasterConnection())
            {
                var operations = new MigrationOperation[]
                    {
                        new DropDatabaseOperation(database)
                    };

                var masterCommands = _sqlGenerator.Generate(operations, generateIdempotentSql: true);
                await _statementExecutor.ExecuteNonQueryAsync(masterConnection, masterCommands, cancellationToken);
            }
        }
    }
}
