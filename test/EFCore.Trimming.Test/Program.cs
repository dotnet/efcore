// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

await using var ctx = new BlogContext();
await ctx.Database.EnsureDeletedAsync();
await ctx.Database.EnsureCreatedAsync();

// Execute any query to make sure the basic query pipeline works
_ = ctx.Blogs.Where(b => b.Name.StartsWith("foo")).ToList();

public class BlogContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(TestEnvironment.DefaultConnection);
}

public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; }
}

