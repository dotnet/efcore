// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class InMemoryDatabaseFacadeTest
    {
        [Fact]
        public void IsInMemory_when_using_in_memory()
        {
            using (var context = new ProviderContext())
            {
                Assert.True(context.Database.IsInMemory());
            }
        }

        private class ProviderContext : DbContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseInMemoryDatabase("Maltesers");
        }
    }
}
