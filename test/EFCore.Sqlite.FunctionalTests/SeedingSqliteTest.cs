// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class SeedingSqliteTest : SeedingTestBase
{
    protected override TestStore TestStore
        => SqliteTestStore.Create("SeedingTest");

    protected override SeedingContext CreateContextWithEmptyDatabase(string testId)
        => new SeedingSqliteContext(testId);

    protected class SeedingSqliteContext(string testId) : SeedingContext(testId)
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite(($"Data Source = Seeds{TestId}.db"));
    }
}
