// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.AspNetIdentity;

#nullable disable

public abstract class
    IdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken> : IdentityUserContext<TUser, TKey,
        TUserClaim, TUserLogin, TUserToken>
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>
    where TUserRole : IdentityUserRole<TKey>
    where TUserLogin : IdentityUserLogin<TKey>
    where TRoleClaim : IdentityRoleClaim<TKey>
    where TUserToken : IdentityUserToken<TKey>
{
    protected IdentityDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected IdentityDbContext()
    {
    }

    public virtual DbSet<TUserRole> UserRoles { get; set; }
    public virtual DbSet<TRole> Roles { get; set; }
    public virtual DbSet<TRoleClaim> RoleClaims { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<TUser>(
            b =>
            {
                b.HasMany<TUserRole>().WithOne().HasForeignKey(ur => ur.UserId).IsRequired();
            });

        builder.Entity<TRole>(
            b =>
            {
                b.HasKey(r => r.Id);
                b.HasIndex(r => r.NormalizedName).IsUnique();
                b.Property(r => r.ConcurrencyStamp).IsConcurrencyToken();

                b.Property(u => u.Name).HasMaxLength(256);
                b.Property(u => u.NormalizedName).HasMaxLength(256);

                b.HasMany<TUserRole>().WithOne().HasForeignKey(ur => ur.RoleId).IsRequired();
                b.HasMany<TRoleClaim>().WithOne().HasForeignKey(rc => rc.RoleId).IsRequired();
            });

        builder.Entity<TRoleClaim>(
            b =>
            {
                b.HasKey(rc => rc.Id);
            });

        builder.Entity<TUserRole>(
            b =>
            {
                b.HasKey(
                    r => new { r.UserId, r.RoleId });
            });
    }
}
