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
        protected override TestStore TestStore => SqlServerTestStore.Create("sqlServerTestStore");

        protected override SeedingContext CreateContextWithEmptyDatabase(string testId)
            => new SeedingSqlServerContext(testId);

        protected override KeylessSeedingContext CreateKeylessContextWithEmptyDatabase(string testId)
            => new KeylessSeedingSqlServerContext(testId);

        protected class SeedingSqlServerContext : SeedingContext
        {
            public SeedingSqlServerContext(string testId)
                : base(testId)
            {
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlServer(SqlServerTestStore.CreateConnectionString($"Seeds{TestId}"));
        }
        protected class KeylessSeedingSqlServerContext : KeylessSeedingContext
        {
            public KeylessSeedingSqlServerContext(string testId)
                : base(testId)
            {
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlServer(SqlServerTestStore.CreateConnectionString($"KeylessSeeds{TestId}"));
        }
    }
}
