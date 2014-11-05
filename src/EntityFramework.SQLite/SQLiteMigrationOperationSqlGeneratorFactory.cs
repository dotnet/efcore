// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.SQLite.Utilities;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteMigrationOperationSqlGeneratorFactory : IMigrationOperationSqlGeneratorFactory
    {
        public virtual SQLiteMigrationOperationSqlGenerator Create()
        {
            return Create(new DatabaseModel());
        }

        public virtual SQLiteMigrationOperationSqlGenerator Create([NotNull] DatabaseModel database)
        {
            Check.NotNull(database, "database");

            return
                new SQLiteMigrationOperationSqlGenerator(new SQLiteTypeMapper())
                    {
                        Database = database,
                    };
        }

        MigrationOperationSqlGenerator IMigrationOperationSqlGeneratorFactory.Create()
        {
            return Create();
        }

        MigrationOperationSqlGenerator IMigrationOperationSqlGeneratorFactory.Create(DatabaseModel database)
        {
            return Create(database);
        }
    }
}
