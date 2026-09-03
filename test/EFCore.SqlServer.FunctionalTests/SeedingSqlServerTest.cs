// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class SeedingSqlServerTest : SeedingTestBase
{
    protected override TestStore TestStore
        => SqlServerTestStore.Create("SeedingTest");

    protected override SeedingContext CreateContextWithEmptyDatabase(string testId)
        => new SeedingSqlServerContext(testId);

    protected class SeedingSqlServerContext(string testId) : SeedingContext(testId)
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(SqlServerTestStore.CreateConnectionString($"Seeds{TestId}"));
    }
}
