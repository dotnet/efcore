// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.SqlServer.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerMigrationOperationSqlGeneratorFactory : IMigrationOperationSqlGeneratorFactory
    {
        public virtual SqlServerMigrationOperationSqlGenerator Create()
        {
            return Create(new DatabaseModel());
        }

        public virtual SqlServerMigrationOperationSqlGenerator Create([NotNull] DatabaseModel database)
        {
            Check.NotNull(database, "database");

            return
                new SqlServerMigrationOperationSqlGenerator(new SqlServerTypeMapper())
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
