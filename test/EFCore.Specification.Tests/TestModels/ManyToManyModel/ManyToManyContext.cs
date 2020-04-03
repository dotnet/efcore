// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class ManyToManyContext : PoolableDbContext
    {
        public static readonly string StoreName = "ManyToMany";

        public ManyToManyContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<EntityOne> EntityOnes { get; set; }
        public DbSet<EntityTwo> EntityTwos { get; set; }
        public DbSet<EntityThree> EntityThrees { get; set; }
        public DbSet<EntityCompositeKey> EntityCompositeKeys { get; set; }
        public DbSet<EntityRoot> EntityRoots { get; set; }

        public static void Seed(ManyToManyContext context) => ManyToManyData.Seed(context);
    }
}
