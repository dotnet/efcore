// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract partial class ModelBuilding101TestBase
{
    [ConditionalFact]
    public virtual void OneToManyRequiredTest()
        => Model101Test();

    protected class OneToManyRequired
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
            public Blog Blog { get; set; }
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
                [Required]
                public int BlogId { get; set; }

                [InverseProperty("Posts")]
                [ForeignKey("BlogId")]
                [Required]
                public Blog Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyOptionalTest()
        => Model101Test();

    protected class OneToManyOptional
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
            public Blog Blog { get; set; }
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
                public Blog Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyRequiredWithShadowFkTest()
        => Model101Test();

    protected class OneToManyRequiredWithShadowFk
    {
        public class Blog
        {
            public int Id { get; set; }
            public ICollection<Post> Posts { get; } = new List<Post>();
        }

        public class Post
        {
            public int Id { get; set; }
            public Blog Blog { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .Property<int>("BlogId");
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Posts)
                    .WithOne(e => e.Blog)
                    .IsRequired();
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

                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }

                [Required]
                public Blog Blog { get; set; }
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

                [InverseProperty("Posts")]
                [ForeignKey("BlogId")]
                [Required]
                public Blog Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyOptionalWithShadowFkTest()
        => Model101Test();

    protected class OneToManyOptionalWithShadowFk
    {
        public class Blog
        {
            public int Id { get; set; }
            public ICollection<Post> Posts { get; } = new List<Post>();
        }

        public class Post
        {
            public int Id { get; set; }
            public Blog Blog { get; set; }
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
                public Blog Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyRequiredNoNavigationToPrincipalTest()
        => Model101Test();

    protected class OneToManyRequiredNoNavigationToPrincipal
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

                [ForeignKey("BlogId")]
                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }
                public int BlogId { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyOptionalNoNavigationToPrincipalTest()
        => Model101Test();

    protected class OneToManyOptionalNoNavigationToPrincipal
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

                [ForeignKey("BlogId")]
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
    public virtual void OneToManyRequiredWithShadowFkAndNoNavigationToPrincipalTest()
        => Model101Test();

    protected class OneToManyRequiredWithShadowFkAndNoNavigationToPrincipal
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

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }

                [ForeignKey("BlogId")]
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

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .Property<int>("BlogId");
        }
    }

    [ConditionalFact]
    public virtual void OneToManyOptionalWithShadowFkAndNoNavigationToPrincipalTest()
        => Model101Test();

    protected class OneToManyOptionalWithShadowFkAndNoNavigationToPrincipal
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

                [ForeignKey("BlogId")]
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
    public virtual void OneToManyRequiredNoNavigationToDependentsTest()
        => Model101Test();

    protected class OneToManyRequiredNoNavigationToDependents
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class Post
        {
            public int Id { get; set; }
            public int BlogId { get; set; }
            public Blog Blog { get; set; }
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
                public int BlogId { get; set; }

                [ForeignKey("BlogId")]
                public Blog Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyOptionalNoNavigationToDependentsTest()
        => Model101Test();

    protected class OneToManyOptionalNoNavigationToDependents
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class Post
        {
            public int Id { get; set; }
            public int? BlogId { get; set; }
            public Blog Blog { get; set; }
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
                public Blog Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyRequiredWithShadowFkAndNoNavigationToDependentsTest()
        => Model101Test();

    protected class OneToManyRequiredWithShadowFkAndNoNavigationToDependents
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class Post
        {
            public int Id { get; set; }
            public Blog Blog { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .Property<int>("BlogId");
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasMany<Post>()
                    .WithOne(e => e.Blog)
                    .IsRequired();
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

                [Required]
                public Blog Blog { get; set; }
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
            }

            public class Post
            {
                public int Id { get; set; }

                [Required]
                [ForeignKey("BlogId")]
                public Blog Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyOptionalWithShadowFkAndNoNavigationToDependentsTest()
        => Model101Test();

    protected class OneToManyOptionalWithShadowFkAndNoNavigationToDependents
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class Post
        {
            public int Id { get; set; }
            public Blog Blog { get; set; }
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
                public Blog Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyRequiredNoNavigationsTest()
        => Model101Test();

    protected class OneToManyRequiredNoNavigations
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
    }

    [ConditionalFact]
    public virtual void OneToManyOptionalNoNavigationsTest()
        => Model101Test();

    protected class OneToManyOptionalNoNavigations
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
    }

    [ConditionalFact]
    public virtual void OneToManyRequiredWithShadowFkAndNoNavigationsTest()
        => Model101Test();

    protected class OneToManyRequiredWithShadowFkAndNoNavigations
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
    public virtual void OneToManyOptionalWithShadowFkAndNoNavigationsTest()
        => Model101Test();

    protected class OneToManyOptionalWithShadowFkAndNoNavigations
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
    public virtual void OneToManyRequiredWithAlternateKeyTest()
        => Model101Test();

    protected class OneToManyRequiredWithAlternateKey
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
            public Blog Blog { get; set; }
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
                public int BlogId { get; set; }

                [ForeignKey("BlogId")]
                [Required]
                [InverseProperty("Posts")]
                public Blog Blog { get; set; }
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
    public virtual void OneToManyOptionalWithAlternateKeyTest()
        => Assert.Throws<EqualException>(() => Model101Test()); // Issue #30346

    protected class OneToManyOptionalWithAlternateKey
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
            public Blog Blog { get; set; }
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
                public Blog Blog { get; set; }
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
    public virtual void OneToManyRequiredWithShadowFkWithAlternateKeyTest()
        => Model101Test();

    protected class OneToManyRequiredWithShadowFkWithAlternateKey
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
            public Blog Blog { get; set; }
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
                    .HasPrincipalKey(e => e.AlternateId)
                    .IsRequired();
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
                public ICollection<Post> Posts { get; } = new List<Post>();
            }

            public class Post
            {
                public int Id { get; set; }

                [Required]
                public Blog Blog { get; set; }
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

        public class ContextAnnotated1 : Context101
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

                [Required]
                [ForeignKey("BlogAlternateId")]
                [InverseProperty("Posts")]
                public Blog Blog { get; set; }
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
    public virtual void OneToManyOptionalWithShadowFkWithAlternateKeyTest()
        => Assert.Throws<EqualException>(() => Model101Test()); // Issue #30346

    protected class OneToManyOptionalWithShadowFkWithAlternateKey
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
            public Blog Blog { get; set; }
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

                [ForeignKey("BlogAlternateId")]
                [InverseProperty("Posts")]
                public Blog Blog { get; set; }
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
    public virtual void OneToManyRequiredWithCompositeKeyTest()
        => Model101Test();

    protected class OneToManyRequiredWithCompositeKey
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
            public Blog Blog { get; set; }
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
                            .WithOne(e => e.Blog)
                            .IsRequired();
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
                public Blog Blog { get; set; }
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

                [ForeignKey("BlogId1, BlogId2")]
                [InverseProperty("Posts")]
                [Required]
                public Blog Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyOptionalWithCompositeKeyTest()
        => Model101Test();

    protected class OneToManyOptionalWithCompositeKey
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
            public Blog Blog { get; set; }
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
                public Blog Blog { get; set; }
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
                public Blog Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyRequiredWithShadowFkWithCompositeKeyTest()
        => Model101Test();

    protected class OneToManyRequiredWithShadowFkWithCompositeKey
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
            public Blog Blog { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog>()
                    .HasKey(e => new { e.Id1, e.Id2 });

                modelBuilder.Entity<Post>(
                    b =>
                    {
                        b.Property<int>("BlogId1");
                        b.Property<int>("BlogId2");
                    });
            }
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>(
                    b =>
                    {
                        b.HasKey(e => new { e.Id1, e.Id2 });

                        b.HasMany(e => e.Posts)
                            .WithOne(e => e.Blog)
                            .IsRequired();
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

                [Required]
                public Blog Blog { get; set; }
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

                [Required]
                [ForeignKey("BlogId1, BlogId2")]
                [InverseProperty("Posts")]
                public Blog Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyOptionalWithShadowFkWithCompositeKeyTest()
        => Model101Test();

    protected class OneToManyOptionalWithShadowFkWithCompositeKey
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
            public Blog Blog { get; set; }
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
                public Blog Blog { get; set; }
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
                public Blog Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManySelfReferencingTest()
        => Model101Test();

    protected class OneToManySelfReferencing
    {
        public class Employee
        {
            public int Id { get; set; }

            public int? ManagerId { get; set; }
            public Employee Manager { get; set; }
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
                public Employee Manager { get; set; }

                [InverseProperty("Manager")]
                public ICollection<Employee> Reports { get; } = new List<Employee>();
            }

            public DbSet<Employee> Employees
                => Set<Employee>();
        }
    }

    [ConditionalFact]
    public virtual void OneToManyRequiredWithoutCascadeDeleteTest()
        => Model101Test();

    protected class OneToManyRequiredWithoutCascadeDelete
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
            public Blog Blog { get; set; }
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
                public Blog Blog { get; set; }
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
                public int BlogId { get; set; }

                [DeleteBehavior(DeleteBehavior.Restrict)]
                [InverseProperty("Posts")]
                [Required]
                [ForeignKey("BlogId")]
                public Blog Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Post> Posts
                => Set<Post>();
        }
    }
}
