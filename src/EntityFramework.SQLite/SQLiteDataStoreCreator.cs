// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.SQLite.Utilities;
using Microsoft.Data.SQLite;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteDataStoreCreator : RelationalDataStoreCreator
    {
        private const int SQLITE_CANTOPEN = 14;

        private readonly SQLiteConnection _connection;
        private readonly SqlStatementExecutor _executor;
        private readonly SQLiteMigrationOperationSqlGeneratorFactory _generatorFactory;
        private readonly SQLiteModelDiffer _modelDiffer;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected SQLiteDataStoreCreator()
        {
        }

        public SQLiteDataStoreCreator(
            [NotNull] SQLiteConnection connection,
            [NotNull] SqlStatementExecutor executor,
            [NotNull] SQLiteMigrationOperationSqlGeneratorFactory generatorFactory,
            [NotNull] SQLiteModelDiffer modelDiffer)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(executor, "executor");
            Check.NotNull(generatorFactory, "generatorFactory");
            Check.NotNull(modelDiffer, "modelDiffer");

            _connection = connection;
            _executor = executor;
            _generatorFactory = generatorFactory;
            _modelDiffer = modelDiffer;
        }

        public override void Create()
        {
            using (var connection = _connection.CreateConnectionReadWriteCreate())
            {
                connection.Open();
            }
        }

        public override Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Create();

            return Task.FromResult(0);
        }

        public override void CreateTables(IModel model)
        {
            Check.NotNull(model, "model");

            var generator = _generatorFactory.Create(model);
            var operations = _modelDiffer.CreateSchema(model);
            var statements = generator.Generate(operations);

            // TODO: Delete database on error
            using (var connection = _connection.CreateConnectionReadWrite())
            {
                _executor.ExecuteNonQuery(connection, null, statements);
            }
        }

        public override Task CreateTablesAsync(IModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            CreateTables(model);

            return Task.FromResult(0);
        }

        public override bool Exists()
        {
            try
            {
                using (var connection = _connection.CreateConnectionReadOnly())
                {
                    connection.Open();
                    connection.Close();
                }

                return true;
            }
            catch (SQLiteException ex)
            {
                if (ex.ErrorCode != SQLITE_CANTOPEN)
                {
                    throw;
                }
            }

            return false;
        }

        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(Exists());
        }

        public override bool HasTables()
        {
            return (long)_executor.ExecuteScalar(_connection.DbConnection, _connection.DbTransaction, CreateHasTablesCommand()) != 0;
        }

        public override async Task<bool> HasTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return (long)(await _executor
                .ExecuteScalarAsync(_connection.DbConnection, _connection.DbTransaction, CreateHasTablesCommand(), cancellationToken)
                .WithCurrentCulture()) != 0;
        }

        private SqlStatement CreateHasTablesCommand()
        {
            return new SqlStatement("SELECT count(*) FROM sqlite_master WHERE type = 'table' AND rootpage IS NOT NULL");
        }

        public override void Delete()
        {
            string filename = null;
            using (var connection = _connection.CreateConnectionReadOnly())
            {
                connection.Open();
                filename = connection.DataSource;
            }

            if (filename != null)
            {
                SQLiteEngine.DeleteDatabase(filename);
            }
        }

        public override Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Delete();

            return Task.FromResult(0);
        }
    }
}
