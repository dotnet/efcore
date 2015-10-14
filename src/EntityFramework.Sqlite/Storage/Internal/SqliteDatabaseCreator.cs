// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class SqliteDatabaseCreator : RelationalDatabaseCreator
    {
        private readonly ISqlCommandBuilder _sqlCommandBuilder;

        public SqliteDatabaseCreator(
            [NotNull] IRelationalConnection connection,
            [NotNull] IMigrationsModelDiffer modelDiffer,
            [NotNull] IMigrationsSqlGenerator migrationsSqlGenerator,
            [NotNull] IModel model,
            [NotNull] ISqlCommandBuilder sqlCommandBuilder)
            : base(model, connection, modelDiffer, migrationsSqlGenerator)
        {
            Check.NotNull(sqlCommandBuilder, nameof(sqlCommandBuilder));

            _sqlCommandBuilder = sqlCommandBuilder;
        }

        public override void Create()
        {
            Connection.Open();
            Connection.Close();
        }

        public override bool Exists() => true;

        protected override bool HasTables()
        {
            var count = (long)_sqlCommandBuilder
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
            }

            if (!string.IsNullOrEmpty(path))
            {
                File.Delete(path);
            }
        }
    }
}
