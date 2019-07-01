// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
namespace Microsoft.EntityFrameworkCore
{
    public class SeedingInMemoryTest : SeedingTestBase
    {
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
}
