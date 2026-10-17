// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.NullSemanticsModel;

public class NullSemanticsContext(DbContextOptions options) : PoolableDbContext(options)
{
    public DbSet<NullSemanticsEntity1> Entities1 { get; set; } = null!;
    public DbSet<NullSemanticsEntity2> Entities2 { get; set; } = null!;

    public static Task SeedAsync(NullSemanticsContext context)
    {
        var entities1 = NullSemanticsData.CreateEntities1();
        var entities2 = NullSemanticsData.CreateEntities2();

        context.Entities1.AddRange(entities1);
        context.Entities2.AddRange(entities2);
        return context.SaveChangesAsync();
    }
}
