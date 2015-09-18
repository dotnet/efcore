// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;

namespace Microsoft.Data.Entity.Storage
{
    public class SqliteDatabaseCreator : RelationalDatabaseCreator
    {
        public SqliteDatabaseCreator(
            [NotNull] IModel model,
            [NotNull] IRelationalConnection connection,
            [NotNull] IMigrationsModelDiffer modelDiffer,
            [NotNull] IMigrationsSqlGenerator migrationsSqlGenerator,
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory)
            : base(model, connection, modelDiffer, migrationsSqlGenerator, commandBuilderFactory)
        {
        }

        public override void Create()
        {
            Connection.Open();
            Connection.Close();
        }

        public override bool Exists() => true;

        protected override bool EvaluateHasTablesResult(object result)
            => (long)result != 0;

        protected override string GetHasTablesSql()
            => "SELECT COUNT(*) FROM \"sqlite_master\" WHERE \"type\" = 'table' AND \"rootpage\" IS NOT NULL;";

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
