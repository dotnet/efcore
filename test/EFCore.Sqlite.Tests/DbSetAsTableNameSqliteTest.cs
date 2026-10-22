// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class DbSetAsTableNameSqliteTest : DbSetAsTableNameTest
{
    protected override string GetTableName<TEntity>(DbContext context)
        => context.Model.FindEntityType(typeof(TEntity))!.GetTableName()!;

    protected override string GetTableName<TEntity>(DbContext context, string entityTypeName)
        => context.Model.FindEntityType(entityTypeName)!.GetTableName()!;

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
                        .BuildServiceProvider(validateScopes: true))
                .UseSqlite("Database = Dummy");
    }

    protected class SqliteNamedTablesContextContext : NamedTablesContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(
                    new ServiceCollection()
                        .AddEntityFrameworkSqlite()
                        .BuildServiceProvider(validateScopes: true))
                .UseSqlite("Database = Dummy");
    }
}
