// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TrimmingTests.TestUtilities;

//await using var ctx = new BlogContext();
//await ctx.Database.EnsureDeletedAsync();
//await ctx.Database.EnsureCreatedAsync();

//ctx.Add(new Blog { Name = "Some Blog Name" });
//await ctx.SaveChangesAsync();

//ctx.ChangeTracker.Clear();

//// Execute any query to make sure the basic query pipeline works
//var blog = await ctx.Blogs.Where(b => b.Name!.StartsWith("Some ")).SingleAsync();
//if (blog.Name != "Some Blog Name")
//{
//    throw new Exception($"Incorrect blog name ({blog.Name})");
//}

return 100;

public class BlogContext : DbContext
{
    public BlogContext()
    {
        Blogs = Set<Blog>();
    }

    private static readonly string ConnectionString;

    public DbSet<Blog> Blogs { get; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(ConnectionString);

    static BlogContext()
    {
        var builder = new SqlConnectionStringBuilder(TestEnvironment.DefaultConnection) { InitialCatalog = "TrimmingTests" };

        ConnectionString = builder.ToString();
    }
}

public class Blog
{
    public int Id { get; set; }
    public string? Name { get; set; }
}
