// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.AspNetIdentity;

namespace Microsoft.EntityFrameworkCore.TestModels.MusicStore;

public class MusicStoreContext(DbContextOptions<MusicStoreContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Album> Albums { get; set; } = null!;
    public DbSet<Artist> Artists { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<Genre> Genres { get; set; } = null!;
    public DbSet<CartItem> CartItems { get; set; } = null!;
    public DbSet<OrderDetail> OrderDetails { get; set; } = null!;
}
