// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore
{
    public class SeedingSqliteTest : SeedingTestBase
    {
        protected override SeedingContext CreateContextWithEmptyDatabase(string testId)
        {
            var context = new SeedingSqliteContext(testId);

            context.Database.EnsureClean();

            return context;
        }

        protected class SeedingSqliteContext : SeedingContext
        {
            public SeedingSqliteContext(string testId)
                : base(testId)
            {
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlite(($"Data Source = Seeds{TestId}.db"));
        }
    }
}
