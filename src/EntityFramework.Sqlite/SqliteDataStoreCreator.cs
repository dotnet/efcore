// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Sqlite.Migrations;
using Microsoft.Data.Entity.Utilities;

#if NET45 || DNX451 || DNXCORE50
using System.IO;
#else
using System;
using System.Reflection;
#endif


namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteDataStoreCreator : RelationalDataStoreCreator, ISqliteDataStoreCreator
    {
        private const int SQLITE_CANTOPEN = 14;

        private readonly ISqliteConnection _connection;
        private readonly IModelDiffer _modelDiffer;
        private readonly ISqliteMigrationSqlGenerator _migrationSqlGenerator;
        private readonly ISqlStatementExecutor _executor;

        public SqliteDataStoreCreator(
            [NotNull] ISqliteConnection connection,
            [NotNull] IModelDiffer modelDiffer,
            [NotNull] ISqliteMigrationSqlGenerator migrationSqlGenerator,
            [NotNull] ISqlStatementExecutor sqlStatementExecutor)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(modelDiffer, nameof(modelDiffer));
            Check.NotNull(migrationSqlGenerator, nameof(migrationSqlGenerator));
            Check.NotNull(sqlStatementExecutor, nameof(sqlStatementExecutor));

            _connection = connection;
            _modelDiffer = modelDiffer;
            _migrationSqlGenerator = migrationSqlGenerator;
            _executor = sqlStatementExecutor;
        }

        public override void Create()
        {
            _connection.Open();
            _connection.Close();
        }

        public override void CreateTables(IModel model)
        {
            Check.NotNull(model, nameof(model));

            var operations = _modelDiffer.GetDifferences(null, model);
            var statements = _migrationSqlGenerator.Generate(operations, model);
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

            if (path != null)
            {
#if NET45 || DNX451 || DNXCORE50
                File.Delete(path);
#else
                // TODO: Remove with netcore451
                Type.GetType("System.IO.File").GetRuntimeMethod("Delete", new[] { typeof(string) }).Invoke(null, new[] { path });
#endif
            }
        }
    }
}
