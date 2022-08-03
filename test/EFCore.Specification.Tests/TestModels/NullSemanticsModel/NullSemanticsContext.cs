// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.NullSemanticsModel;

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
