// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.AspNetIdentity;

namespace Microsoft.EntityFrameworkCore.TestModels.MusicStore;

#nullable disable

public class MusicStoreContext(DbContextOptions<MusicStoreContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Album> Albums { get; set; }
    public DbSet<Artist> Artists { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
}
