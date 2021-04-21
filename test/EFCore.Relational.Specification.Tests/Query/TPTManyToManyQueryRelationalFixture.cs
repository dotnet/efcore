// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class TPTManyToManyQueryRelationalFixture : ManyToManyQueryRelationalFixture
    {
        protected override string StoreName { get; } = "TPTManyToManyQueryTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<EntityRoot>().ToTable("Roots");
            modelBuilder.Entity<EntityBranch>().ToTable("Branches");
            modelBuilder.Entity<EntityLeaf>().ToTable("Leaves");
        }
    }
}
