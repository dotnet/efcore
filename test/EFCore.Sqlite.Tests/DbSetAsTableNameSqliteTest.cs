// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Relational.Tests;

namespace Microsoft.EntityFrameworkCore.Sqlite.Tests
{
    public class DbSetAsTableNameSqliteTest : DbSetAsTableNameTest
    {
        protected override string GetTableName<TEntity>(DbContext context)
            => context.Model.FindEntityType(typeof(TEntity)).Sqlite().TableName;

        protected override SetsContext CreateContext() => new SqliteSetsContext();

        protected override SetsContext CreateNamedTablesContext() => new SqliteNamedTablesContextContext();

        protected class SqliteSetsContext : SetsContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlite("Database = Dummy");
        }

        protected class SqliteNamedTablesContextContext : NamedTablesContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlite("Database = Dummy");
        }
    }
}
