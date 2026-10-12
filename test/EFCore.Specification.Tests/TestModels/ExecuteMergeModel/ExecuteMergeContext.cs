// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ExecuteMergeModel;

public class ExecuteMergeContext(DbContextOptions options) : PoolableDbContext(options)
{
    public DbSet<Blog> Blogs { get; set; } = null!;
    public DbSet<DailyVisit> Visits { get; set; } = null!;
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Gadget> Gadgets { get; set; } = null!;
    public DbSet<Keyless> Keyless { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>(
            b =>
            {
                b.Property(e => e.Id).ValueGeneratedNever();
                b.HasIndex(e => e.Name).IsUnique();
            });
        modelBuilder.Entity<DailyVisit>(b => b.HasKey(e => new { e.UserId, e.Day }));
        modelBuilder.Entity<Account>(
            b =>
            {
                b.Property(e => e.Id).ValueGeneratedNever();
                b.Ignore(e => e.Details);
            });
        modelBuilder.Entity<Gadget>(b => b.Property(e => e.Id).ValueGeneratedNever());
        modelBuilder.Entity<Keyless>(b => b.HasNoKey());
    }
}
