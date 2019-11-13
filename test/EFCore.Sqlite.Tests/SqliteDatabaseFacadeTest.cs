// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class SqliteDatabaseFacadeTest
    {
        [ConditionalFact]
        public void IsSqlite_when_using_SQLite()
        {
            using var context = new ProviderContext(
                new DbContextOptionsBuilder()
                    .UseInternalServiceProvider(
                        new ServiceCollection()
                            .AddEntityFrameworkSqlite()
                            .BuildServiceProvider())
                    .UseSqlite("Database=Maltesers").Options);
            Assert.True(context.Database.IsSqlite());
        }

        [ConditionalFact]
        public void Not_IsSqlite_when_using_different_provider()
        {
            using var context = new ProviderContext(
                new DbContextOptionsBuilder()
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("Maltesers").Options);
            Assert.False(context.Database.IsSqlite());
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
