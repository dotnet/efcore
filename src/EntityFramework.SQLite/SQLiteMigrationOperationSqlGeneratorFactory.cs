// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.SQLite.Utilities;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteMigrationOperationSqlGeneratorFactory : IMigrationOperationSqlGeneratorFactory
    {
        public virtual MigrationOperationSqlGenerator Create(DatabaseModel database)
        {
            Check.NotNull(database, "database");

            return new SQLiteMigrationOperationSqlGenerator(new SQLiteTypeMapper()) { Database = database };
        }
    }
}
