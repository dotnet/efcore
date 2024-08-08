// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.DatepartModel;

public class ExpeditionContext(DbContextOptions options) : PoolableDbContext(options)
{
    public virtual DbSet<Expedition> Expeditions { get; set; } = null!;

    public static async Task SeedAsync(ExpeditionContext context)
    {
        context.Expeditions.AddRange(ExpeditionData.CreateExpeditions());
        await context.SaveChangesAsync();
    }
}
