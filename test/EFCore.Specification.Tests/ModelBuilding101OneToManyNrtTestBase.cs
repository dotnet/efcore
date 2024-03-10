// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore;

public abstract partial class ModelBuilding101TestBase
{
    [ConditionalFact]
    public virtual void OneToManyRequiredNrtTest()
        => Model101Test();

    protected class OneToManyRequiredNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public ICollection<Post> Posts { get; } = new List<Post>();
        }

        public class Post
        {
            public int Id { get; set; }
            public int BlogId { get; set; }
            public Blog Blog { get; set; } = null!;
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog);
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog)
                    .HasForeignKey(e => e.BlogId)
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired();
        }

        public class Context3 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasOne(e => e.Blog)
                    .WithMany(e => e.Posts)
                    .HasForeignKey(e => e.BlogId)
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired();
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }

                [InverseProperty("Blog")]
                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }

                [ForeignKey("Blog")]
                public int BlogId { get; set; }

                [InverseProperty("Posts")]
                [Required]
                [ForeignKey("BlogId")]
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyOptionalNrtTest()
        => Model101Test();

    protected class OneToManyOptionalNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public ICollection<Post> Posts { get; } = new List<Post>();
        }

        public class Post
        {
            public int Id { get; set; }
            public int? BlogId { get; set; }
            public Blog? Blog { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog);
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog)
                    .HasForeignKey(e => e.BlogId)
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired(false);
        }

        public class Context3 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasOne(e => e.Blog)
                    .WithMany(e => e.Posts)
                    .HasForeignKey(e => e.BlogId)
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired(false);
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }

                [InverseProperty("Blog")]
                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }

                [ForeignKey("Blog")]
                public int? BlogId { get; set; }

                [InverseProperty("Posts")]
                [ForeignKey("BlogId")]
                public Blog? Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyRequiredWithShadowFkNrtTest()
        => Model101Test();

    protected class OneToManyRequiredWithShadowFkNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public ICollection<Post> Posts { get; } = new List<Post>();
        }

        public class Post
        {
            public int Id { get; set; }
            public Blog Blog { get; set; } = null!;
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog);
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog)
                    .HasForeignKey("BlogId")
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired();
        }

        public class Context3 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasOne(e => e.Blog)
                    .WithMany(e => e.Posts)
                    .HasForeignKey("BlogId")
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired();
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }

                [InverseProperty("Blog")]
                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }

                [InverseProperty("Posts")]
                [ForeignKey("BlogId")]
                [Required]
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyOptionalWithShadowFkNrtTest()
        => Model101Test();

    protected class OneToManyOptionalWithShadowFkNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public ICollection<Post> Posts { get; } = new List<Post>();
        }

        public class Post
        {
            public int Id { get; set; }
            public Blog? Blog { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog);
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog)
                    .HasForeignKey("BlogId")
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired(false);
        }

        public class Context3 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasOne(e => e.Blog)
                    .WithMany(e => e.Posts)
                    .HasForeignKey("BlogId")
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired(false);
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }

                [InverseProperty("Blog")]
                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }

                [InverseProperty("Posts")]
                [ForeignKey("BlogId")]
                public Blog? Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyRequiredNoNavigationToPrincipalNrtTest()
        => Model101Test();

    protected class OneToManyRequiredNoNavigationToPrincipalNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public ICollection<Post> Posts { get; } = new List<Post>();
        }

        public class Post
        {
            public int Id { get; set; }
            public int BlogId { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne();
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne()
                    .HasForeignKey(e => e.BlogId)
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired();
        }

        public class Context3 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasOne<Blog>()
                    .WithMany(e => e.Posts)
                    .HasForeignKey(e => e.BlogId)
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired();
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }

                [Required]
                public int BlogId { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyOptionalNoNavigationToPrincipalNrtTest()
        => Model101Test();

    protected class OneToManyOptionalNoNavigationToPrincipalNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public ICollection<Post> Posts { get; } = new List<Post>();
        }

        public class Post
        {
            public int Id { get; set; }
            public int? BlogId { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne();
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne()
                    .HasForeignKey(e => e.BlogId)
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired(false);
        }

        public class Context3 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasOne<Blog>()
                    .WithMany(e => e.Posts)
                    .HasForeignKey(e => e.BlogId)
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired(false);
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }
                public int? BlogId { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyRequiredWithShadowFkAndNoNavigationToPrincipalNrtTest()
        => Model101Test();

    protected class OneToManyRequiredWithShadowFkAndNoNavigationToPrincipalNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public ICollection<Post> Posts { get; } = new List<Post>();
        }

        public class Post
        {
            public int Id { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne()
                    .IsRequired();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne()
                    .HasForeignKey("BlogId")
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired();
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasOne<Blog>()
                    .WithMany(e => e.Posts)
                    .HasForeignKey("BlogId")
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyOptionalWithShadowFkAndNoNavigationToPrincipalNrtTest()
        => Model101Test();

    protected class OneToManyOptionalWithShadowFkAndNoNavigationToPrincipalNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public ICollection<Post> Posts { get; } = new List<Post>();
        }

        public class Post
        {
            public int Id { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne();
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne()
                    .HasForeignKey("BlogId")
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired(false);
        }

        public class Context3 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasOne<Blog>()
                    .WithMany(e => e.Posts)
                    .HasForeignKey("BlogId")
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired(false);
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyRequiredNoNavigationToDependentsNrtTest()
        => Model101Test();

    protected class OneToManyRequiredNoNavigationToDependentsNrt
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class Post
        {
            public int Id { get; set; }
            public int BlogId { get; set; }
            public Blog Blog { get; set; } = null!;
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany<Post>()
                    .WithOne(e => e.Blog);
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany<Post>()
                    .WithOne(e => e.Blog)
                    .HasForeignKey(e => e.BlogId)
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired();
        }

        public class Context3 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasOne(e => e.Blog)
                    .WithMany()
                    .HasForeignKey(e => e.BlogId)
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired();
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
            }

            public class Post
            {
                public int Id { get; set; }

                [ForeignKey("Blog")]
                [Required]
                public int BlogId { get; set; }

                [ForeignKey("BlogId")]
                [Required]
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyOptionalNoNavigationToDependentsNrtTest()
        => Model101Test();

    protected class OneToManyOptionalNoNavigationToDependentsNrt
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class Post
        {
            public int Id { get; set; }
            public int? BlogId { get; set; }
            public Blog? Blog { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany<Post>()
                    .WithOne(e => e.Blog);
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany<Post>()
                    .WithOne(e => e.Blog)
                    .HasForeignKey(e => e.BlogId)
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired(false);
        }

        public class Context3 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasOne(e => e.Blog)
                    .WithMany()
                    .HasForeignKey(e => e.BlogId)
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired(false);
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
            }

            public class Post
            {
                public int Id { get; set; }

                [ForeignKey("Blog")]
                public int? BlogId { get; set; }

                [ForeignKey("BlogId")]
                public Blog? Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyRequiredWithShadowFkAndNoNavigationToDependentsNrtTest()
        => Model101Test();

    protected class OneToManyRequiredWithShadowFkAndNoNavigationToDependentsNrt
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class Post
        {
            public int Id { get; set; }
            public Blog Blog { get; set; } = null!;
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany<Post>()
                    .WithOne(e => e.Blog);
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany<Post>()
                    .WithOne(e => e.Blog)
                    .HasForeignKey("BlogId")
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired();
        }

        public class Context3 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasOne(e => e.Blog)
                    .WithMany()
                    .HasForeignKey("BlogId")
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired();
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
            }

            public class Post
            {
                public int Id { get; set; }

                [ForeignKey("BlogId")]
                [Required]
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyOptionalWithShadowFkAndNoNavigationToDependentsNrtTest()
        => Model101Test();

    protected class OneToManyOptionalWithShadowFkAndNoNavigationToDependentsNrt
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class Post
        {
            public int Id { get; set; }
            public Blog? Blog { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany<Post>()
                    .WithOne(e => e.Blog);
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany<Post>()
                    .WithOne(e => e.Blog)
                    .HasForeignKey("BlogId")
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired(false);
        }

        public class Context3 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasOne(e => e.Blog)
                    .WithMany()
                    .HasForeignKey("BlogId")
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired(false);
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
            }

            public class Post
            {
                public int Id { get; set; }

                [ForeignKey("BlogId")]
                public Blog? Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyRequiredNoNavigationsNrtTest()
        => Model101Test();

    protected class OneToManyRequiredNoNavigationsNrt
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class Post
        {
            public int Id { get; set; }
            public int BlogId { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany<Post>()
                    .WithOne();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany<Post>()
                    .WithOne()
                    .HasForeignKey(e => e.BlogId)
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired();
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasOne<Blog>()
                    .WithMany()
                    .HasForeignKey(e => e.BlogId)
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired();
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
            }

            public class Post
            {
                public int Id { get; set; }

                [Required]
                public int BlogId { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany<Post>()
                    .WithOne();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyOptionalNoNavigationsNrtTest()
        => Model101Test();

    protected class OneToManyOptionalNoNavigationsNrt
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class Post
        {
            public int Id { get; set; }
            public int? BlogId { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany<Post>()
                    .WithOne();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany<Post>()
                    .WithOne()
                    .HasForeignKey(e => e.BlogId)
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired(false);
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasOne<Blog>()
                    .WithMany()
                    .HasForeignKey(e => e.BlogId)
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired(false);
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
            }

            public class Post
            {
                public int Id { get; set; }
                public int? BlogId { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany<Post>()
                    .WithOne();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyRequiredWithShadowFkAndNoNavigationsNrtTest()
        => Model101Test();

    protected class OneToManyRequiredWithShadowFkAndNoNavigationsNrt
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class Post
        {
            public int Id { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany<Post>()
                    .WithOne()
                    .IsRequired();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany<Post>()
                    .WithOne()
                    .HasForeignKey("BlogId")
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired();
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasOne<Blog>()
                    .WithMany()
                    .HasForeignKey("BlogId")
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyOptionalWithShadowFkAndNoNavigationsNrtTest()
        => Model101Test();

    protected class OneToManyOptionalWithShadowFkAndNoNavigationsNrt
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class Post
        {
            public int Id { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany<Post>()
                    .WithOne();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany<Post>()
                    .WithOne()
                    .HasForeignKey("BlogId")
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired(false);
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasOne<Blog>()
                    .WithMany()
                    .HasForeignKey("BlogId")
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired(false);
        }
    }

    [ConditionalFact]
    public virtual void OneToManyRequiredWithAlternateKeyNrtTest()
        => Model101Test();

    protected class OneToManyRequiredWithAlternateKeyNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public int AlternateId { get; set; }
            public ICollection<Post> Posts { get; } = new List<Post>();
        }

        public class Post
        {
            public int Id { get; set; }
            public int BlogId { get; set; }
            public Blog Blog { get; set; } = null!;
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey(e => e.AlternateId);
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey(e => e.AlternateId)
                    .HasForeignKey(e => e.BlogId)
                    .IsRequired();
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasOne(e => e.Blog)
                    .WithMany(e => e.Posts)
                    .HasPrincipalKey(e => e.AlternateId)
                    .HasForeignKey(e => e.BlogId)
                    .IsRequired();
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
                public int AlternateId { get; set; }

                [InverseProperty("Blog")]
                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }

                [ForeignKey("Blog")]
                [Required]
                public int BlogId { get; set; }

                [InverseProperty("Posts")]
                [ForeignKey("BlogId")]
                [Required]
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey(e => e.AlternateId);
        }
    }

    [ConditionalFact]
    public virtual void OneToManyOptionalWithAlternateKeyNrtTest()
        => Assert.Throws<EqualException>(() => Model101Test()); // Issue #30346

    protected class OneToManyOptionalWithAlternateKeyNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public int AlternateId { get; set; }
            public ICollection<Post> Posts { get; } = new List<Post>();
        }

        public class Post
        {
            public int Id { get; set; }
            public int? BlogId { get; set; }
            public Blog? Blog { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey(e => e.AlternateId);
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey(e => e.AlternateId)
                    .HasForeignKey(e => e.BlogId)
                    .IsRequired(false);
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasOne(e => e.Blog)
                    .WithMany(e => e.Posts)
                    .HasPrincipalKey(e => e.AlternateId)
                    .HasForeignKey(e => e.BlogId)
                    .IsRequired(false);
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
                public int AlternateId { get; set; }

                [InverseProperty("Blog")]
                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }

                [ForeignKey("Blog")]
                public int? BlogId { get; set; }

                [InverseProperty("Posts")]
                [ForeignKey("BlogId")]
                public Blog? Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey(e => e.AlternateId);
        }
    }

    [ConditionalFact]
    public virtual void OneToManyRequiredWithShadowFkWithAlternateKeyNrtTest()
        => Assert.Throws<EqualException>(() => Model101Test()); // Issue #30344

    protected class OneToManyRequiredWithShadowFkWithAlternateKeyNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public int AlternateId { get; set; }
            public ICollection<Post> Posts { get; } = new List<Post>();
        }

        public class Post
        {
            public int Id { get; set; }
            public Blog Blog { get; set; } = null!;
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey(e => e.AlternateId);
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey(e => e.AlternateId)
                    .HasForeignKey("BlogAlternateId")
                    .IsRequired();
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasOne(e => e.Blog)
                    .WithMany(e => e.Posts)
                    .HasPrincipalKey(e => e.AlternateId)
                    .HasForeignKey("BlogAlternateId")
                    .IsRequired();
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
                public int AlternateId { get; set; }

                [InverseProperty("Blog")]
                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }

                [InverseProperty("Posts")]
                [ForeignKey("BlogId")]
                [Required]
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey(e => e.AlternateId);
        }
    }

    [ConditionalFact]
    public virtual void OneToManyOptionalWithShadowFkWithAlternateKeyNrtTest()
        => Assert.Throws<EqualException>(() => Model101Test()); // Issue #30346

    protected class OneToManyOptionalWithShadowFkWithAlternateKeyNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public int AlternateId { get; set; }
            public ICollection<Post> Posts { get; } = new List<Post>();
        }

        public class Post
        {
            public int Id { get; set; }
            public Blog? Blog { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey(e => e.AlternateId);
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey(e => e.AlternateId)
                    .HasForeignKey("BlogAlternateId")
                    .IsRequired(false);
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasOne(e => e.Blog)
                    .WithMany(e => e.Posts)
                    .HasPrincipalKey(e => e.AlternateId)
                    .HasForeignKey("BlogAlternateId")
                    .IsRequired(false);
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
                public int AlternateId { get; set; }

                [InverseProperty("Blog")]
                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }

                [InverseProperty("Posts")]
                [ForeignKey("BlogId")]
                public Blog? Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey(e => e.AlternateId);
        }
    }

    [ConditionalFact]
    public virtual void OneToManyRequiredWithCompositeKeyNrtTest()
        => Model101Test();

    protected class OneToManyRequiredWithCompositeKeyNrt
    {
        public class Blog
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }
            public ICollection<Post> Posts { get; } = new List<Post>();
        }

        public class Post
        {
            public int Id { get; set; }
            public int BlogId1 { get; set; }
            public int BlogId2 { get; set; }
            public Blog Blog { get; set; } = null!;
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasKey(e => new { e.Id1, e.Id2 });
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>(
                    nestedBuilder =>
                    {
                        nestedBuilder.HasKey(e => new { e.Id1, e.Id2 });

                        nestedBuilder.HasMany(e => e.Posts)
                            .WithOne(e => e.Blog);
                    });
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>(
                    nestedBuilder =>
                    {
                        nestedBuilder.HasKey(e => new { e.Id1, e.Id2 });

                        nestedBuilder.HasMany(e => e.Posts)
                            .WithOne(e => e.Blog)
                            .HasPrincipalKey(e => new { e.Id1, e.Id2 })
                            .HasForeignKey(e => new { e.BlogId1, e.BlogId2 })
                            .IsRequired();
                    });
        }

        public class Context3 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog>()
                    .HasKey(e => new { e.Id1, e.Id2 });

                modelBuilder.Entity<Post>()
                    .HasOne(e => e.Blog)
                    .WithMany(e => e.Posts)
                    .HasPrincipalKey(e => new { e.Id1, e.Id2 })
                    .HasForeignKey(e => new { e.BlogId1, e.BlogId2 })
                    .IsRequired();
            }
        }

        public class ContextAnnotated0 : Context101
        {
            [PrimaryKey("Id1", "Id2")]
            public class Blog
            {
                public int Id1 { get; set; }
                public int Id2 { get; set; }
                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }
                public int BlogId1 { get; set; }
                public int BlogId2 { get; set; }
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }

        public class ContextAnnotated1 : Context101
        {
            [PrimaryKey("Id1", "Id2")]
            public class Blog
            {
                public int Id1 { get; set; }
                public int Id2 { get; set; }

                [InverseProperty("Blog")]
                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }
                public int BlogId1 { get; set; }
                public int BlogId2 { get; set; }

                [InverseProperty("Posts")]
                [ForeignKey("BlogId1, BlogId2")]
                [Required]
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyOptionalWithCompositeKeyNrtTest()
        => Model101Test();

    protected class OneToManyOptionalWithCompositeKeyNrt
    {
        public class Blog
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }
            public ICollection<Post> Posts { get; } = new List<Post>();
        }

        public class Post
        {
            public int Id { get; set; }
            public int BlogId1 { get; set; }
            public int? BlogId2 { get; set; }
            public Blog? Blog { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasKey(e => new { e.Id1, e.Id2 });
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>(
                    b =>
                    {
                        b.HasKey(e => new { e.Id1, e.Id2 });

                        b.HasMany(e => e.Posts)
                            .WithOne(e => e.Blog);
                    });
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>(
                    b =>
                    {
                        b.HasKey(e => new { e.Id1, e.Id2 });

                        b.HasMany(e => e.Posts)
                            .WithOne(e => e.Blog)
                            .HasPrincipalKey(e => new { e.Id1, e.Id2 })
                            .HasForeignKey(e => new { e.BlogId1, e.BlogId2 })
                            .IsRequired(false);
                    });
        }

        public class Context3 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog>()
                    .HasKey(e => new { e.Id1, e.Id2 });

                modelBuilder.Entity<Post>()
                    .HasOne(e => e.Blog)
                    .WithMany(e => e.Posts)
                    .HasPrincipalKey(e => new { e.Id1, e.Id2 })
                    .HasForeignKey(e => new { e.BlogId1, e.BlogId2 })
                    .IsRequired(false);
            }
        }

        public class ContextAnnotated0 : Context101
        {
            [PrimaryKey("Id1", "Id2")]
            public class Blog
            {
                public int Id1 { get; set; }
                public int Id2 { get; set; }
                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }
                public int BlogId1 { get; set; }
                public int? BlogId2 { get; set; }
                public Blog? Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }

        public class ContextAnnotated1 : Context101
        {
            [PrimaryKey("Id1", "Id2")]
            public class Blog
            {
                public int Id1 { get; set; }
                public int Id2 { get; set; }

                [InverseProperty("Blog")]
                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }
                public int BlogId1 { get; set; }
                public int? BlogId2 { get; set; }

                [InverseProperty("Posts")]
                [ForeignKey("BlogId1, BlogId2")]
                public Blog? Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyRequiredWithShadowFkWithCompositeKeyNrtTest()
        => Model101Test();

    protected class OneToManyRequiredWithShadowFkWithCompositeKeyNrt
    {
        public class Blog
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }
            public ICollection<Post> Posts { get; } = new List<Post>();
        }

        public class Post
        {
            public int Id { get; set; }
            public Blog Blog { get; set; } = null!;
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasKey(e => new { e.Id1, e.Id2 });
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>(
                    b =>
                    {
                        b.HasKey(e => new { e.Id1, e.Id2 });

                        b.HasMany(e => e.Posts)
                            .WithOne(e => e.Blog);
                    });
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>(
                    b =>
                    {
                        b.HasKey(e => new { e.Id1, e.Id2 });

                        b.HasMany(e => e.Posts)
                            .WithOne(e => e.Blog)
                            .HasPrincipalKey(e => new { e.Id1, e.Id2 })
                            .HasForeignKey("BlogId1", "BlogId2")
                            .IsRequired();
                    });
        }

        public class Context3 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog>()
                    .HasKey(e => new { e.Id1, e.Id2 });

                modelBuilder.Entity<Post>()
                    .HasOne(e => e.Blog)
                    .WithMany(e => e.Posts)
                    .HasPrincipalKey(e => new { e.Id1, e.Id2 })
                    .HasForeignKey("BlogId1", "BlogId2")
                    .IsRequired();
            }
        }

        public class ContextAnnotated0 : Context101
        {
            [PrimaryKey("Id1", "Id2")]
            public class Blog
            {
                public int Id1 { get; set; }
                public int Id2 { get; set; }
                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }

        public class ContextAnnotated1 : Context101
        {
            [PrimaryKey("Id1", "Id2")]
            public class Blog
            {
                public int Id1 { get; set; }
                public int Id2 { get; set; }

                [InverseProperty("Blog")]
                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }

                [InverseProperty("Posts")]
                [ForeignKey("BlogId1, BlogId2")]
                [Required]
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyOptionalWithShadowFkWithCompositeKeyNrtTest()
        => Model101Test();

    protected class OneToManyOptionalWithShadowFkWithCompositeKeyNrt
    {
        public class Blog
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }
            public ICollection<Post> Posts { get; } = new List<Post>();
        }

        public class Post
        {
            public int Id { get; set; }
            public Blog? Blog { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasKey(e => new { e.Id1, e.Id2 });
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>(
                    b =>
                    {
                        b.HasKey(e => new { e.Id1, e.Id2 });

                        b.HasMany(e => e.Posts)
                            .WithOne(e => e.Blog);
                    });
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>(
                    b =>
                    {
                        b.HasKey(e => new { e.Id1, e.Id2 });

                        b.HasMany(e => e.Posts)
                            .WithOne(e => e.Blog)
                            .HasPrincipalKey(e => new { e.Id1, e.Id2 })
                            .HasForeignKey("BlogId1", "BlogId2")
                            .IsRequired(false);
                    });
        }

        public class Context3 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog>()
                    .HasKey(e => new { e.Id1, e.Id2 });

                modelBuilder.Entity<Post>()
                    .HasOne(e => e.Blog)
                    .WithMany(e => e.Posts)
                    .HasPrincipalKey(e => new { e.Id1, e.Id2 })
                    .HasForeignKey("BlogId1", "BlogId2")
                    .IsRequired(false);
            }
        }

        public class ContextAnnotated0 : Context101
        {
            [PrimaryKey("Id1", "Id2")]
            public class Blog
            {
                public int Id1 { get; set; }
                public int Id2 { get; set; }
                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }
                public Blog? Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }

        public class ContextAnnotated1 : Context101
        {
            [PrimaryKey("Id1", "Id2")]
            public class Blog
            {
                public int Id1 { get; set; }
                public int Id2 { get; set; }

                [InverseProperty("Blog")]
                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }

                [InverseProperty("Posts")]
                [ForeignKey("BlogId1, BlogId2")]
                public Blog? Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManySelfReferencingNrtTest()
        => Model101Test();

    protected class OneToManySelfReferencingNrt
    {
        public class Employee
        {
            public int Id { get; set; }

            public int? ManagerId { get; set; }
            public Employee? Manager { get; set; }
            public ICollection<Employee> Reports { get; } = new List<Employee>();
        }

        public class Context0 : Context101
        {
            public DbSet<Employee> Employees
                => Set<Employee>();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Employee>()
                    .HasOne(e => e.Manager)
                    .WithMany(e => e.Reports);
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Employee>()
                    .HasOne(e => e.Manager)
                    .WithMany(e => e.Reports)
                    .HasForeignKey(e => e.ManagerId)
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired(false);
        }

        public class ContextAnnotated0 : Context101
        {
            public class Employee
            {
                public int Id { get; set; }

                [ForeignKey("Manager")]
                public int? ManagerId { get; set; }

                [InverseProperty("Reports")]
                [ForeignKey("ManagerId")]
                public Employee? Manager { get; set; }

                [InverseProperty("Manager")]
                public ICollection<Employee> Reports { get; } = new List<Employee>();
            }

            public DbSet<Employee> Employees
                => Set<Employee>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyRequiredWithoutCascadeDeleteNrtTest()
        => Model101Test();

    protected class OneToManyRequiredWithoutCascadeDeleteNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public ICollection<Post> Posts { get; } = new List<Post>();
        }

        public class Post
        {
            public int Id { get; set; }
            public int BlogId { get; set; }
            public Blog Blog { get; set; } = null!;
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog)
                    .OnDelete(DeleteBehavior.Restrict);
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog)
                    .HasForeignKey(e => e.BlogId)
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasOne(e => e.Blog)
                    .WithMany(e => e.Posts)
                    .HasForeignKey(e => e.BlogId)
                    .HasPrincipalKey(e => e.Id)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }
                public int BlogId { get; set; }

                [DeleteBehavior(DeleteBehavior.Restrict)]
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }

        public class ContextAnnotated1 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }

                [InverseProperty("Blog")]
                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }

                [ForeignKey("Blog")]
                [Required]
                public int BlogId { get; set; }

                [InverseProperty("Posts")]
                [ForeignKey("BlogId")]
                [Required]
                [DeleteBehavior(DeleteBehavior.Restrict)]
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }
}
