// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Migrations;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational;
using Microsoft.Data.SqlServer.Utilities;

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

        public async override Task CreateIfNotExistsAsync(IModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = _dataStore.CreateConnection())
            {
                using (var masterConnection = _dataStore.CreateMasterConnection())
                {
                    var operations = new MigrationOperation[]
                    { 
                        new CreateDatabaseOperation(connection.Database)
                    };

                    var masterCommands = _sqlGenerator.Generate(operations, generateIdempotentSql: true);
                    await _statementExecutor.ExecuteAsync(masterConnection, masterCommands, cancellationToken);
                }

                var schemaOperations = _modelDiffer.DiffSource(model);
                var schemaCommands = _sqlGenerator.Generate(schemaOperations, generateIdempotentSql: false);
                await _statementExecutor.ExecuteAsync(connection, schemaCommands, cancellationToken);
            }
        }
    }
}
