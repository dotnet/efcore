// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.CompositeKeysModel;

public class CompositeKeysContext(DbContextOptions options) : PoolableDbContext(options)
{
    public DbSet<CompositeOne> CompositeOnes { get; set; } = null!;
    public DbSet<CompositeTwo> CompositeTwos { get; set; } = null!;
    public DbSet<CompositeThree> CompositeThrees { get; set; } = null!;
    public DbSet<CompositeFour> CompositeFours { get; set; } = null!;
}
