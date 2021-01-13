// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class DbSetAsTableNameSqliteTest : DbSetAsTableNameTest
    {
        protected override string GetTableName<TEntity>(DbContext context)
            => context.Model.FindEntityType(typeof(TEntity)).GetTableName();

        protected override string GetTableName<TEntity>(DbContext context, string entityTypeName)
            => context.Model.FindEntityType(entityTypeName).GetTableName();

        protected override SetsContext CreateContext()
            => new SqliteSetsContext();

        protected override SetsContext CreateNamedTablesContext()
            => new SqliteNamedTablesContextContext();

        protected class SqliteSetsContext : SetsContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(
                        new ServiceCollection()
                            .AddEntityFrameworkSqlite()
                            .BuildServiceProvider())
                    .UseSqlite("Database = Dummy");
        }

        protected class SqliteNamedTablesContextContext : NamedTablesContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(
                        new ServiceCollection()
                            .AddEntityFrameworkSqlite()
                            .BuildServiceProvider())
                    .UseSqlite("Database = Dummy");
        }
    }
}
