// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Tests;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class SqliteDatabaseCleaner : RelationalDatabaseCleaner
    {
        protected override IInternalDatabaseModelFactory CreateDatabaseModelFactory(ILoggerFactory loggerFactory)
            => new SqliteDatabaseModelFactory(loggerFactory.CreateLogger<SqliteDatabaseModelFactory>());

        protected override bool AcceptForeignKey(ForeignKeyModel foreignKey) => false;

        protected override bool AcceptIndex(IndexModel index) => false;

        protected override string BuildCustomSql(DatabaseModel databaseModel) => "PRAGMA foreign_keys=OFF;";
    }
}
