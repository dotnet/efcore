// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels;

public class CrossStoreContext : DbContext
{
    public CrossStoreContext()
    {
    }

    public CrossStoreContext(DbContextOptions options)
        : base(options)
    {
    }

    public virtual DbSet<SimpleEntity> SimpleEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<SimpleEntity>(
            eb =>
            {
                eb.ToTable("RelationalSimpleEntity");
                eb.Property(typeof(string), SimpleEntity.ShadowPropertyName);
                eb.HasKey(e => e.Id);
                eb.Property(e => e.Id).UseIdentityColumn();
            });

    public static void RemoveAllEntities(CrossStoreContext context)
        => context.SimpleEntities.RemoveRange(context.SimpleEntities);
}
