// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.TestModels.NullSemanticsModel
{
    public class NullSemanticsContext : PoolableDbContext
    {
        public NullSemanticsContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<NullSemanticsEntity1> Entities1 { get; set; }
        public DbSet<NullSemanticsEntity2> Entities2 { get; set; }

        public static void Seed(NullSemanticsContext context)
        {
            var entities1 = NullSemanticsData.CreateEntities1();
            var entities2 = NullSemanticsData.CreateEntities2();

            context.Entities1.AddRange(entities1);
            context.Entities2.AddRange(entities2);
            context.SaveChanges();
        }
    }
}
