using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

#pragma warning disable 169

namespace Samples
{
    public class Program
    {
        private static void Main()
        {
            SetupDatabase();

            using (var db = new BloggingContext("Diego"))
            {
                var blogs = db.Blogs
                    .Include(b => b.Posts)
                    .ToList();

                foreach (var blog in blogs)
                {
                    Console.WriteLine(
                        $"{blog.Url.PadRight(33)} [Tenant: {db.Entry(blog).Property("TenantId").CurrentValue}]");

                    foreach (var post in blog.Posts)
                    {
                        Console.WriteLine($" - {post.Title.PadRight(30)} [IsDeleted: {post.IsDeleted}]");
                    }

                    Console.WriteLine();
                }

                #region IgnoreFilters
                blogs = db.Blogs
                    .Include(b => b.Posts)
                    .IgnoreQueryFilters()
                    .ToList();
                #endregion

                foreach (var blog in blogs)
                {
                    Console.WriteLine(
                        $"{blog.Url.PadRight(33)} [Tenant: {db.Entry(blog).Property("TenantId").CurrentValue}]");

                    foreach (var post in blog.Posts)
                    {
                        Console.WriteLine($" - {post.Title.PadRight(30)} [IsDeleted: {post.IsDeleted}]");
                    }

                    Console.WriteLine();
                }
            }
        }

        private static void SetupDatabase()
        {
            using (var db = new BloggingContext("diego"))
            {
                if (db.Database.EnsureCreated())
                {
                    db.Blogs.Add(
                        new Blog
                        {
                            Url = "http://sample.com/blogs/fish",
                            Posts = new List<Post>
                            {
                                new Post { Title = "Fish care 101" },
                                new Post { Title = "Caring for tropical fish" },
                                new Post { Title = "Types of ornamental fish" }
                            }
                        });

                    db.Blogs.Add(
                        new Blog
                        {
                            Url = "http://sample.com/blogs/cats",
                            Posts = new List<Post>
                            {
                                new Post { Title = "Cat care 101" },
                                new Post { Title = "Caring for tropical cats" },
                                new Post { Title = "Types of ornamental cats" }
                            }
                        });

                    db.SaveChanges();

                    using (var andrewDb = new BloggingContext("andrew"))
                    {
                        andrewDb.Blogs.Add(
                            new Blog
                            {
                                Url = "http://sample.com/blogs/catfish",
                                Posts = new List<Post>
                                {
                                    new Post { Title = "Catfish care 101" },
                                    new Post { Title = "History of the catfish name" }
                                }
                            });

                        andrewDb.SaveChanges();
                    }

                    db.Posts
                        .Where(
                            p => p.Title == "Caring for tropical fish"
                                 || p.Title == "Cat care 101")
                        .ToList()
                        .ForEach(p => db.Posts.Remove(p));

                    db.SaveChanges();
                }
            }
        }
    }

    public class BloggingContext : DbContext
    {
        private static readonly ILoggerFactory _loggerFactory
            = new LoggerFactory().AddConsole((s, l) => l == LogLevel.Information && !s.EndsWith("Connection"));

        private readonly string _tenantId;

        public BloggingContext(string tenant)
        {
            _tenantId = tenant;
        }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlServer(
                    @"Server=(localdb)\mssqllocaldb;Database=Demo.QueryFilters;Trusted_Connection=True;ConnectRetryCount=0;")
                .UseLoggerFactory(_loggerFactory);
        }

        #region Configuration
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>().Property<string>("TenantId").HasField("_tenantId");

            // Configure entity filters
            modelBuilder.Entity<Blog>().HasQueryFilter(b => EF.Property<string>(b, "TenantId") == _tenantId);
            modelBuilder.Entity<Post>().HasQueryFilter(p => !p.IsDeleted);
        }
        #endregion

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();

            foreach (var item in ChangeTracker.Entries().Where(
                e =>
                    e.State == EntityState.Added && e.Metadata.GetProperties().Any(p => p.Name == "TenantId")))
            {
                item.CurrentValues["TenantId"] = _tenantId;
            }

            foreach (var item in ChangeTracker.Entries<Post>().Where(e => e.State == EntityState.Deleted))
            {
                item.State = EntityState.Modified;
                item.CurrentValues["IsDeleted"] = true;
            }

            return base.SaveChanges();
        }
    }

    #region Entities
    public class Blog
    {
        private string _tenantId;

        public int BlogId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        public List<Post> Posts { get; set; }
    }

    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsDeleted { get; set; }

        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }
    #endregion
}
