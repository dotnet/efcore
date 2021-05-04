// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.TestModels.CompositeKeysModel
{
    public class CompositeKeysContext : PoolableDbContext
    {
        public CompositeKeysContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<CompositeOne> CompositeOnes { get; set; }
        public DbSet<CompositeTwo> CompositeTwos { get; set; }
        public DbSet<CompositeThree> CompositeThrees { get; set; }
        public DbSet<CompositeFour> CompositeFours { get; set; }
    }
}
