// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class SqliteDatabaseCreator : RelationalDatabaseCreator
    {
        // ReSharper disable once InconsistentNaming
        private const int SQLITE_CANTOPEN = 14;

        private readonly SqliteRelationalConnection _connection;
        private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;

        public SqliteDatabaseCreator(
            [NotNull] SqliteRelationalConnection connection,
            [NotNull] IMigrationsModelDiffer modelDiffer,
            [NotNull] IMigrationsSqlGenerator migrationsSqlGenerator,
            [NotNull] IModel model,
            [NotNull] IRawSqlCommandBuilder rawSqlCommandBuilder)
            : base(model, connection, modelDiffer, migrationsSqlGenerator)
        {
            Check.NotNull(rawSqlCommandBuilder, nameof(rawSqlCommandBuilder));

            _connection = connection;
            _rawSqlCommandBuilder = rawSqlCommandBuilder;
        }

        public override void Create()
        {
            Connection.Open();
            Connection.Close();
        }

        public override bool Exists()
        {
            using (var readOnlyConnection = _connection.CreateReadOnlyConnection())
            {
                try
                {
                    readOnlyConnection.Open();
                }
                catch (SqliteException ex) when (ex.SqliteErrorCode == SQLITE_CANTOPEN)
                {
                    return false;
                }
            }

            return true;
        }

        protected override bool HasTables()
        {
            var count = (long)_rawSqlCommandBuilder
                .Build("SELECT COUNT(*) FROM \"sqlite_master\" WHERE \"type\" = 'table' AND \"rootpage\" IS NOT NULL;")
                .ExecuteScalar(Connection);

            return count != 0;
        }

        public override void Delete()
        {
            string path = null;

            Connection.Open();
            try
            {
                path = Connection.DbConnection.DataSource;
                Connection.Close();
            }
            catch
            {
                // any exceptions here can be ignored
            }

            if (!string.IsNullOrEmpty(path))
            {
                File.Delete(path);
            }
        }
    }
}
