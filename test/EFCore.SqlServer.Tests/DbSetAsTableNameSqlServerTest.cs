// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore
{
    public class DbSetAsTableNameSqlServerTest : DbSetAsTableNameTest
    {
        protected override string GetTableName<TEntity>(DbContext context)
            => context.Model.FindEntityType(typeof(TEntity)).SqlServer().TableName;

        protected override SetsContext CreateContext() => new SqlServerSetsContext();

        protected override SetsContext CreateNamedTablesContext() => new SqlServerNamedTablesContextContext();

        protected class SqlServerSetsContext : SetsContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlServer("Database = Dummy");
        }

        protected class SqlServerNamedTablesContextContext : NamedTablesContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlServer("Database = Dummy");
        }
    }
}
