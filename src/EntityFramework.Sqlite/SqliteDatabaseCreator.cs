// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteDatabaseCreator : RelationalDatabaseCreator
    {
        private readonly IRelationalConnection _connection;
        private readonly IMigrationsModelDiffer _modelDiffer;
        private readonly IMigrationsSqlGenerator _migrationsSqlGenerator;
        private readonly ISqlStatementExecutor _executor;

        public SqliteDatabaseCreator(
            [NotNull] IRelationalConnection connection,
            [NotNull] IMigrationsModelDiffer modelDiffer,
            [NotNull] IMigrationsSqlGenerator migrationsSqlGenerator,
            [NotNull] ISqlStatementExecutor sqlStatementExecutor,
            [NotNull] IModel model)
            : base(model)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(modelDiffer, nameof(modelDiffer));
            Check.NotNull(migrationsSqlGenerator, nameof(migrationsSqlGenerator));
            Check.NotNull(sqlStatementExecutor, nameof(sqlStatementExecutor));

            _connection = connection;
            _modelDiffer = modelDiffer;
            _migrationsSqlGenerator = migrationsSqlGenerator;
            _executor = sqlStatementExecutor;
        }

        public override void Create()
        {
            _connection.Open();
            _connection.Close();
        }

        public override void CreateTables()
        {
            var operations = _modelDiffer.GetDifferences(null, Model);
            var statements = _migrationsSqlGenerator.Generate(operations, Model);
            _executor.ExecuteNonQuery(_connection, null, statements);
        }

        public override bool Exists() => true;

        public override bool HasTables()
        {
            var count = (long)_executor.ExecuteScalar(
                _connection,
                null,
                "SELECT COUNT(*) FROM \"sqlite_master\" WHERE \"type\" = 'table' AND \"rootpage\" IS NOT NULL;");

            return count != 0;
        }

        public override void Delete()
        {
            string path = null;

            _connection.Open();
            try
            {
                path = _connection.DbConnection.DataSource;
                _connection.Close();
            }
            catch
            {
            }

            if (!string.IsNullOrEmpty(path))
            {
                File.Delete(path);
            }
        }
    }
}
