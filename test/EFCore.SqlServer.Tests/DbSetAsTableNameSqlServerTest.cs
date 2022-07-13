// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class DbSetAsTableNameSqlServerTest : DbSetAsTableNameTest
{
    protected override string GetTableName<TEntity>(DbContext context)
        => context.Model.FindEntityType(typeof(TEntity)).GetTableName();

    protected override string GetTableName<TEntity>(DbContext context, string entityTypeName)
        => context.Model.FindEntityType(entityTypeName).GetTableName();

    protected override SetsContext CreateContext()
        => new SqlServerSetsContext();

    protected override SetsContext CreateNamedTablesContext()
        => new SqlServerNamedTablesContextContext();

    protected class SqlServerSetsContext : SetsContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(SqlServerFixture.DefaultServiceProvider)
                .UseSqlServer("Database = Dummy");
    }

    protected class SqlServerNamedTablesContextContext : NamedTablesContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(SqlServerFixture.DefaultServiceProvider)
                .UseSqlServer("Database = Dummy");
    }
}
