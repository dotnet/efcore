// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.CompositeKeysModel;

#nullable disable

public class CompositeKeysContext(DbContextOptions options) : PoolableDbContext(options)
{
    public DbSet<CompositeOne> CompositeOnes { get; set; }
    public DbSet<CompositeTwo> CompositeTwos { get; set; }
    public DbSet<CompositeThree> CompositeThrees { get; set; }
    public DbSet<CompositeFour> CompositeFours { get; set; }
}
