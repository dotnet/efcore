// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

public class SeedingInMemoryTest : SeedingTestBase
{
    protected override TestStore TestStore
        => InMemoryTestStore.Create("SeedingTest");

    protected override SeedingContext CreateContextWithEmptyDatabase(string testId)
        => new SeedingInMemoryContext(testId);

    protected class SeedingInMemoryContext : SeedingContext
    {
        public SeedingInMemoryContext(string testId)
            : base(testId)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase($"Seeds{TestId}");
    }
}
