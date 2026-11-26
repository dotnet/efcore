// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class FullTextTableQueryTest
{
    private class Post
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
        public bool IsDeleted { get; set; }
    }

    private class BlogContext : DbContext
    {
        public DbSet<Post> Posts => Set<Post>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer("Server=localhost;Database=Test;Trusted_Connection=True;TrustServerCertificate=True");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Post>().HasQueryFilter(p => !p.IsDeleted);
    }

    [Fact]
    public void FreeTextTable_with_query_filter_does_not_generate_filter_with_undeclared_alias()
    {
        using var ctx = new BlogContext();
        var hits = ctx.Posts.FreeTextTable<Post, int>("search");
        var sql = hits.ToQueryString();

        Assert.Contains("FREETEXTTABLE([Posts], *, @p)", sql);
        Assert.DoesNotContain("WHERE", sql);
    }

    [Fact]
    public void ContainsTable_with_query_filter_does_not_generate_filter_with_undeclared_alias()
    {
        using var ctx = new BlogContext();
        var hits = ctx.Posts.ContainsTable<Post, int>("search");
        var sql = hits.ToQueryString();

        Assert.Contains("CONTAINSTABLE([Posts], *, @p)", sql);
        Assert.DoesNotContain("WHERE", sql);
    }
}
