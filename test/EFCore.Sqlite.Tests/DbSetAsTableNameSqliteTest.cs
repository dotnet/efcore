// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore
{
    public class DbSetAsTableNameSqliteTest : DbSetAsTableNameTest
    {
        protected override string GetTableName<TEntity>(DbContext context)
            => context.Model.FindEntityType(typeof(TEntity)).Relational().TableName;

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
