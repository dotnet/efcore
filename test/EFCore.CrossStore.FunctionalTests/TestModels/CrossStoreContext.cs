// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels
{
    public class CrossStoreContext : DbContext
    {
        public CrossStoreContext(DbContextOptions options)
            : base(options)
        {
        }

        public virtual DbSet<SimpleEntity> SimpleEntities { get; set; }

        public static void RemoveAllEntities(CrossStoreContext context)
            => context.SimpleEntities.RemoveRange(context.SimpleEntities);
    }
}
