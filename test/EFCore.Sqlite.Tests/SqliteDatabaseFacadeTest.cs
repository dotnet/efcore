// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class SqliteDatabaseFacadeTest
    {
        [Fact]
        public void IsSqlite_when_using_SQLite()
        {
            using (var context = new ProviderContext(
                new DbContextOptionsBuilder().UseSqlite("Database=Maltesers").Options))
            {
                Assert.True(SqliteDatabaseFacadeExtensions.IsSqlite(context.Database));
            }
        }

        [Fact]
        public void Not_IsSqlite_when_using_different_provider()
        {
            using (var context = new ProviderContext(
                new DbContextOptionsBuilder().UseInMemoryDatabase("Maltesers").Options))
            {
                Assert.False(SqliteDatabaseFacadeExtensions.IsSqlite(context.Database));
            }
        }

        private class ProviderContext : DbContext
        {
            public ProviderContext(DbContextOptions options)
                : base(options)
            {
            }
        }
    }
}
