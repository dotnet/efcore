// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

public class BasicTypesContext(DbContextOptions options) : PoolableDbContext(options)
{
    public DbSet<BasicTypesEntity> BasicTypesEntities { get; set; } = null!;
    public DbSet<NullableBasicTypesEntity> NullableBasicTypesEntities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BasicTypesEntity>().Property(b => b.Id).ValueGeneratedNever();
        modelBuilder.Entity<NullableBasicTypesEntity>().Property(b => b.Id).ValueGeneratedNever();
    }
}
