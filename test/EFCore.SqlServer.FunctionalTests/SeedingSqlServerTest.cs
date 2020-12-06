// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class SeedingSqlServerTest : SeedingTestBase
    {
        protected override SeedingContext CreateContextWithEmptyDatabase(string testId)
        {
            var context = new SeedingSqlServerContext(testId);

            context.Database.EnsureClean();

            return context;
        }

        protected override KeylessSeedingContext CreateKeylessContextWithEmptyDatabase(string testId)
        {
            var context = new KeylessSeedingInMemoryContext(testId);

            context.Database.EnsureClean();

            return context;
        }

        protected class SeedingSqlServerContext : SeedingContext
        {
            public SeedingSqlServerContext(string testId)
                : base(testId)
            {
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlServer(SqlServerTestStore.CreateConnectionString($"Seeds{TestId}"));
        }
        protected class KeylessSeedingInMemoryContext : KeylessSeedingContext
        {
            public KeylessSeedingInMemoryContext(string testId)
                : base(testId)
            {
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlServer(SqlServerTestStore.CreateConnectionString($"Seeds{TestId}"));
        }

    }
}
