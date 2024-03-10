// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore;

public abstract partial class ModelBuilding101TestBase
{
    [ConditionalFact]
    public virtual void OneToOneRequiredNrtTest()
        => Model101Test();

    protected class OneToOneRequiredNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public BlogHeader? Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public int BlogId { get; set; }
            public Blog Blog { get; set; } = null!;
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne(e => e.Blog)
                    .HasForeignKey<BlogHeader>(e => e.BlogId)
                    .IsRequired();
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne(e => e.Header)
                    .HasForeignKey<BlogHeader>(e => e.BlogId)
                    .IsRequired();
        }

        public class ContextAnnotated0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            public class Blog
            {
                public int Id { get; set; }

                [InverseProperty("Blog")]
                public BlogHeader? Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [ForeignKey("Blog")]
                [Required]
                public int BlogId { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("BlogId")]
                [Required]
                public Blog Blog { get; set; } = null!;
            }
        }
    }

    [ConditionalFact]
    public virtual void OneToOneOptionalNrtTest()
        => Model101Test();

    protected class OneToOneOptionalNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public BlogHeader? Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public int? BlogId { get; set; }
            public Blog? Blog { get; set; }
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne(e => e.Blog)
                    .HasForeignKey<BlogHeader>(e => e.BlogId)
                    .IsRequired(false);
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne(e => e.Header)
                    .HasForeignKey<BlogHeader>(e => e.BlogId)
                    .IsRequired(false);
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }

                [InverseProperty("Blog")]
                public BlogHeader? Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [ForeignKey("Blog")]
                public int? BlogId { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("BlogId")]
                public Blog? Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneRequiredPkToPkNrtTest()
        => Model101Test();

    protected class OneToOneRequiredPkToPkNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public BlogHeader? Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public Blog Blog { get; set; } = null!;
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne(e => e.Blog)
                    .HasForeignKey<BlogHeader>();
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne(e => e.Blog)
                    .HasForeignKey<BlogHeader>(e => e.Id)
                    .IsRequired();
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne(e => e.Header)
                    .HasForeignKey<BlogHeader>(e => e.Id)
                    .IsRequired();
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }

                public BlogHeader? Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [ForeignKey("Id")]
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }

        public class ContextAnnotated1 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }

                [InverseProperty("Blog")]
                public BlogHeader? Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("Id")]
                [Required]
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneRequiredWithShadowFkNrtTest()
        => Model101Test();

    protected class OneToOneRequiredWithShadowFkNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public BlogHeader? Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public Blog Blog { get; set; } = null!;
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne(e => e.Blog)
                    .HasForeignKey<BlogHeader>("BlogId");
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne(e => e.Blog)
                    .HasForeignKey<BlogHeader>("BlogId")
                    .IsRequired();
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne(e => e.Header)
                    .HasForeignKey<BlogHeader>("BlogId")
                    .IsRequired();
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
                public BlogHeader? Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [ForeignKey("BlogId")]
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }

        public class ContextAnnotated1 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }

                [InverseProperty("Blog")]
                public BlogHeader? Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("BlogId")]
                [Required]
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneOptionalWithShadowFkNrtTest()
        => Model101Test();

    protected class OneToOneOptionalWithShadowFkNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public BlogHeader? Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public Blog? Blog { get; set; }
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne(e => e.Blog)
                    .HasForeignKey<BlogHeader>("BlogId");
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne(e => e.Blog)
                    .HasForeignKey<BlogHeader>("BlogId")
                    .IsRequired(false);
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne(e => e.Header)
                    .HasForeignKey<BlogHeader>("BlogId")
                    .IsRequired(false);
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
                public BlogHeader? Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [ForeignKey("BlogId")]
                public Blog? Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }

        public class ContextAnnotated1 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }

                [InverseProperty("Blog")]
                public BlogHeader? Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("BlogId")]
                public Blog? Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneRequiredNoNavigationToPrincipalNrtTest()
        => Model101Test();

    protected class OneToOneRequiredNoNavigationToPrincipalNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public BlogHeader? Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public int BlogId { get; set; }
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne()
                    .HasForeignKey<BlogHeader>(e => e.BlogId)
                    .IsRequired();
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne<Blog>()
                    .WithOne(e => e.Header)
                    .HasForeignKey<BlogHeader>(e => e.BlogId)
                    .IsRequired();
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
                public BlogHeader? Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [Required]
                public int BlogId { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneOptionalNoNavigationToPrincipalNrtTest()
        => Model101Test();

    protected class OneToOneOptionalNoNavigationToPrincipalNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public BlogHeader? Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public int? BlogId { get; set; }
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne()
                    .HasForeignKey<BlogHeader>(e => e.BlogId)
                    .IsRequired(false);
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne<Blog>()
                    .WithOne(e => e.Header)
                    .HasForeignKey<BlogHeader>(e => e.BlogId)
                    .IsRequired(false);
        }
    }

    [ConditionalFact]
    public virtual void OneToOneRequiredWithShadowFkAndNoNavigationToPrincipalNrtTest()
        => Model101Test();

    protected class OneToOneRequiredWithShadowFkAndNoNavigationToPrincipalNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public BlogHeader? Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne()
                    .HasForeignKey<BlogHeader>("BlogId")
                    .IsRequired();
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne()
                    .HasForeignKey<BlogHeader>("BlogId")
                    .IsRequired();
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne<Blog>()
                    .WithOne(e => e.Header)
                    .HasForeignKey<BlogHeader>("BlogId")
                    .IsRequired();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneOptionalWithShadowFkAndNoNavigationToPrincipalNrtTest()
        => Model101Test();

    protected class OneToOneOptionalWithShadowFkAndNoNavigationToPrincipalNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public BlogHeader? Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne()
                    .HasForeignKey<BlogHeader>("BlogId");
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne()
                    .HasForeignKey<BlogHeader>("BlogId")
                    .IsRequired(false);
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne<Blog>()
                    .WithOne(e => e.Header)
                    .HasForeignKey<BlogHeader>("BlogId")
                    .IsRequired(false);
        }
    }

    [ConditionalFact]
    public virtual void OneToOneRequiredNoNavigationToDependentsNrtTest()
        => Model101Test();

    protected class OneToOneRequiredNoNavigationToDependentsNrt
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public int BlogId { get; set; }
            public Blog Blog { get; set; } = null!;
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne();
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne<BlogHeader>()
                    .WithOne(e => e.Blog)
                    .HasForeignKey<BlogHeader>(e => e.BlogId)
                    .IsRequired();
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne()
                    .HasForeignKey<BlogHeader>(e => e.BlogId)
                    .IsRequired();
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
            }

            public class BlogHeader
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

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneOptionalNoNavigationToDependentsNrtTest()
        => Model101Test();

    protected class OneToOneOptionalNoNavigationToDependentsNrt
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public int? BlogId { get; set; }
            public Blog? Blog { get; set; }
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne();
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne<BlogHeader>()
                    .WithOne(e => e.Blog)
                    .HasForeignKey<BlogHeader>(e => e.BlogId)
                    .IsRequired(false);
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne()
                    .HasForeignKey<BlogHeader>(e => e.BlogId)
                    .IsRequired(false);
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [ForeignKey("Blog")]
                public int? BlogId { get; set; }

                [ForeignKey("BlogId")]
                public Blog? Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneRequiredWithShadowFkAndNoNavigationToDependentsNrtTest()
        => Model101Test();

    protected class OneToOneRequiredWithShadowFkAndNoNavigationToDependentsNrt
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public Blog Blog { get; set; } = null!;
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne()
                    .HasForeignKey<BlogHeader>("BlogId");
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne<BlogHeader>()
                    .WithOne(e => e.Blog)
                    .HasForeignKey<BlogHeader>("BlogId")
                    .IsRequired();
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne()
                    .HasForeignKey<BlogHeader>("BlogId")
                    .IsRequired();
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [ForeignKey("BlogId")]
                [Required]
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneOptionalWithShadowFkAndNoNavigationToDependentsNrtTest()
        => Model101Test();

    protected class OneToOneOptionalWithShadowFkAndNoNavigationToDependentsNrt
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public Blog? Blog { get; set; }
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne()
                    .HasForeignKey<BlogHeader>("BlogId");
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne<BlogHeader>()
                    .WithOne(e => e.Blog)
                    .HasForeignKey<BlogHeader>("BlogId")
                    .IsRequired(false);
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne()
                    .HasForeignKey<BlogHeader>("BlogId")
                    .IsRequired(false);
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [ForeignKey("BlogId")]
                public Blog? Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneRequiredNoNavigationsNrtTest()
        => Model101Test();

    protected class OneToOneRequiredNoNavigationsNrt
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public int BlogId { get; set; }
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne<BlogHeader>()
                    .WithOne();
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne<BlogHeader>()
                    .WithOne()
                    .HasForeignKey<BlogHeader>(e => e.BlogId)
                    .IsRequired();
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne<Blog>()
                    .WithOne()
                    .HasForeignKey<BlogHeader>(e => e.BlogId)
                    .IsRequired();
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [Required]
                public int BlogId { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne<BlogHeader>()
                    .WithOne();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneOptionalNoNavigationsNrtTest()
        => Model101Test();

    protected class OneToOneOptionalNoNavigationsNrt
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public int? BlogId { get; set; }
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne<BlogHeader>()
                    .WithOne();
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne<BlogHeader>()
                    .WithOne()
                    .HasForeignKey<BlogHeader>(e => e.BlogId)
                    .IsRequired(false);
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne<Blog>()
                    .WithOne()
                    .HasForeignKey<BlogHeader>(e => e.BlogId)
                    .IsRequired(false);
        }
    }

    [ConditionalFact]
    public virtual void OneToOneRequiredWithShadowFkAndNoNavigationsNrtTest()
        => Model101Test();

    protected class OneToOneRequiredWithShadowFkAndNoNavigationsNrt
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne<BlogHeader>()
                    .WithOne()
                    .HasForeignKey<BlogHeader>("BlogId")
                    .IsRequired();
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne<BlogHeader>()
                    .WithOne()
                    .HasForeignKey<BlogHeader>("BlogId")
                    .IsRequired();
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne<Blog>()
                    .WithOne()
                    .HasForeignKey<BlogHeader>("BlogId")
                    .IsRequired();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneOptionalWithShadowFkAndNoNavigationsNrtTest()
        => Model101Test();

    protected class OneToOneOptionalWithShadowFkAndNoNavigationsNrt
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne<BlogHeader>()
                    .WithOne()
                    .HasForeignKey<BlogHeader>("BlogId");
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne<BlogHeader>()
                    .WithOne()
                    .HasForeignKey<BlogHeader>("BlogId")
                    .IsRequired(false);
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne<Blog>()
                    .WithOne()
                    .HasForeignKey<BlogHeader>("BlogId")
                    .IsRequired(false);
        }
    }

    [ConditionalFact]
    public virtual void OneToOneRequiredWithAlternateKeyNrtTest()
        => Assert.Throws<EqualException>(() => Model101Test()); // Issue #30346

    protected class OneToOneRequiredWithAlternateKeyNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public int AlternateId { get; set; }
            public BlogHeader? Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public int BlogId { get; set; }
            public Blog Blog { get; set; } = null!;
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey<Blog>(e => e.AlternateId);
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey<Blog>(e => e.AlternateId)
                    .HasForeignKey<BlogHeader>(e => e.BlogId)
                    .IsRequired();
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne(e => e.Header)
                    .HasPrincipalKey<Blog>(e => e.AlternateId)
                    .HasForeignKey<BlogHeader>(e => e.BlogId)
                    .IsRequired();
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
                public int AlternateId { get; set; }

                [InverseProperty("Blog")]
                public BlogHeader? Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [ForeignKey("Blog")]
                [Required]
                public int BlogId { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("BlogId")]
                [Required]
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey<Blog>(e => e.AlternateId);
        }
    }

    [ConditionalFact]
    public virtual void OneToOneOptionalWithAlternateKeyNrtTest()
        => Assert.Throws<EqualException>(() => Model101Test()); // Issue #30346

    protected class OneToOneOptionalWithAlternateKeyNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public int AlternateId { get; set; }
            public BlogHeader? Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public int? BlogId { get; set; }
            public Blog? Blog { get; set; }
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey<Blog>(e => e.AlternateId);
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey<Blog>(e => e.AlternateId)
                    .HasForeignKey<BlogHeader>(e => e.BlogId)
                    .IsRequired(false);
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne(e => e.Header)
                    .HasPrincipalKey<Blog>(e => e.AlternateId)
                    .HasForeignKey<BlogHeader>(e => e.BlogId)
                    .IsRequired(false);
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
                public int AlternateId { get; set; }

                [InverseProperty("Blog")]
                public BlogHeader? Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [ForeignKey("Blog")]
                public int? BlogId { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("BlogId")]
                public Blog? Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey<Blog>(e => e.AlternateId);
        }
    }

    [ConditionalFact]
    public virtual void OneToOneRequiredWithShadowFkWithAlternateKeyNrtTest()
        => Assert.Throws<EqualException>(() => Model101Test()); // Issue #30346

    protected class OneToOneRequiredWithShadowFkWithAlternateKeyNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public int AlternateId { get; set; }
            public BlogHeader? Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public Blog Blog { get; set; } = null!;
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey<Blog>(e => e.AlternateId);
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey<Blog>(e => e.AlternateId)
                    .HasForeignKey<BlogHeader>("BlogAlternateId")
                    .IsRequired();
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne(e => e.Header)
                    .HasPrincipalKey<Blog>(e => e.AlternateId)
                    .HasForeignKey<BlogHeader>("BlogAlternateId")
                    .IsRequired();
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
                public int AlternateId { get; set; }

                [InverseProperty("Blog")]
                public BlogHeader? Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("BlogId")]
                [Required]
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey<Blog>(e => e.AlternateId);
        }
    }

    [ConditionalFact]
    public virtual void OneToOneOptionalWithShadowFkWithAlternateKeyNrtTest()
        => Assert.Throws<EqualException>(() => Model101Test()); // Issue #30346

    protected class OneToOneOptionalWithShadowFkWithAlternateKeyNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public int AlternateId { get; set; }
            public BlogHeader? Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public Blog? Blog { get; set; }
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey<Blog>(e => e.AlternateId);
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey<Blog>(e => e.AlternateId)
                    .HasForeignKey<BlogHeader>("BlogAlternateId")
                    .IsRequired(false);
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne(e => e.Header)
                    .HasPrincipalKey<Blog>(e => e.AlternateId)
                    .HasForeignKey<BlogHeader>("BlogAlternateId")
                    .IsRequired(false);
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
                public int AlternateId { get; set; }

                [InverseProperty("Blog")]
                public BlogHeader? Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("BlogId")]
                public Blog? Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne(e => e.Blog)
                    .HasPrincipalKey<Blog>(e => e.AlternateId);
        }
    }

    [ConditionalFact]
    public virtual void OneToOneRequiredWithCompositeKeyNrtTest()
        => Model101Test();

    protected class OneToOneRequiredWithCompositeKeyNrt
    {
        public class Blog
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }
            public BlogHeader? Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public int BlogId1 { get; set; }
            public int BlogId2 { get; set; }
            public Blog Blog { get; set; } = null!;
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasKey(e => new { e.Id1, e.Id2 });
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>(
                    nestedBuilder =>
                    {
                        nestedBuilder.HasKey(e => new { e.Id1, e.Id2 });

                        nestedBuilder.HasOne(e => e.Header)
                            .WithOne(e => e.Blog)
                            .HasPrincipalKey<Blog>(e => new { e.Id1, e.Id2 })
                            .HasForeignKey<BlogHeader>(e => new { e.BlogId1, e.BlogId2 })
                            .IsRequired();
                    });
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog>()
                    .HasKey(e => new { e.Id1, e.Id2 });

                modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne(e => e.Header)
                    .HasPrincipalKey<Blog>(e => new { e.Id1, e.Id2 })
                    .HasForeignKey<BlogHeader>(e => new { e.BlogId1, e.BlogId2 })
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
                public BlogHeader? Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }
                public int BlogId1 { get; set; }
                public int BlogId2 { get; set; }

                [ForeignKey("BlogId1, BlogId2")]
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }

        public class ContextAnnotated1 : Context101
        {
            [PrimaryKey("Id1", "Id2")]
            public class Blog
            {
                public int Id1 { get; set; }
                public int Id2 { get; set; }

                [InverseProperty("Blog")]
                public BlogHeader? Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [Required]
                public int BlogId1 { get; set; }

                [Required]
                public int BlogId2 { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("BlogId1, BlogId2")]
                [Required]
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneOptionalWithCompositeKeyNrtTest()
        => Model101Test();

    protected class OneToOneOptionalWithCompositeKeyNrt
    {
        public class Blog
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }
            public BlogHeader? Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public int BlogId1 { get; set; }
            public int? BlogId2 { get; set; }
            public Blog? Blog { get; set; }
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasKey(e => new { e.Id1, e.Id2 });
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>(
                    b =>
                    {
                        b.HasKey(e => new { e.Id1, e.Id2 });

                        b.HasOne(e => e.Header)
                            .WithOne(e => e.Blog)
                            .HasPrincipalKey<Blog>(e => new { e.Id1, e.Id2 })
                            .HasForeignKey<BlogHeader>(e => new { e.BlogId1, e.BlogId2 })
                            .IsRequired(false);
                    });
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog>()
                    .HasKey(e => new { e.Id1, e.Id2 });

                modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne(e => e.Header)
                    .HasPrincipalKey<Blog>(e => new { e.Id1, e.Id2 })
                    .HasForeignKey<BlogHeader>(e => new { e.BlogId1, e.BlogId2 })
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
                public BlogHeader? Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }
                public int BlogId1 { get; set; }
                public int? BlogId2 { get; set; }

                [ForeignKey("BlogId1, BlogId2")]
                public Blog? Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }

        public class ContextAnnotated1 : Context101
        {
            [PrimaryKey("Id1", "Id2")]
            public class Blog
            {
                public int Id1 { get; set; }
                public int Id2 { get; set; }

                [InverseProperty("Blog")]
                public BlogHeader? Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [Required]
                public int BlogId1 { get; set; }

                public int? BlogId2 { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("BlogId1, BlogId2")]
                public Blog? Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneOneToOneRequiredWithShadowFkWithCompositeKeyNrtTest()
        => Model101Test();

    protected class OneToOneOneToOneRequiredWithShadowFkWithCompositeKeyNrt
    {
        public class Blog
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }
            public BlogHeader? Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public Blog Blog { get; set; } = null!;
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>(
                    b =>
                    {
                        b.HasKey(e => new { e.Id1, e.Id2 });

                        b.HasOne(e => e.Header)
                            .WithOne(e => e.Blog)
                            .HasPrincipalKey<Blog>(e => new { e.Id1, e.Id2 });
                    });
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>(
                    b =>
                    {
                        b.HasKey(e => new { e.Id1, e.Id2 });

                        b.HasOne(e => e.Header)
                            .WithOne(e => e.Blog)
                            .HasPrincipalKey<Blog>(e => new { e.Id1, e.Id2 })
                            .HasForeignKey<BlogHeader>("BlogId1", "BlogId2")
                            .IsRequired();
                    });
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog>()
                    .HasKey(e => new { e.Id1, e.Id2 });

                modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne(e => e.Header)
                    .HasPrincipalKey<Blog>(e => new { e.Id1, e.Id2 })
                    .HasForeignKey<BlogHeader>("BlogId1", "BlogId2")
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
                public BlogHeader? Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [ForeignKey("BlogId1, BlogId2")]
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }

        public class ContextAnnotated1 : Context101
        {
            [PrimaryKey("Id1", "Id2")]
            public class Blog
            {
                public int Id1 { get; set; }
                public int Id2 { get; set; }

                [InverseProperty("Blog")]
                public BlogHeader? Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("BlogId1, BlogId2")]
                [Required]
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneOptionalWithShadowFkWithCompositeKeyNrtTest()
        => Model101Test();

    protected class OneToOneOptionalWithShadowFkWithCompositeKeyNrt
    {
        public class Blog
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }
            public BlogHeader? Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public Blog? Blog { get; set; }
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>(
                    b =>
                    {
                        b.HasKey(e => new { e.Id1, e.Id2 });

                        b.HasOne(e => e.Header)
                            .WithOne(e => e.Blog)
                            .HasPrincipalKey<Blog>(e => new { e.Id1, e.Id2 });
                    });
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>(
                    b =>
                    {
                        b.HasKey(e => new { e.Id1, e.Id2 });

                        b.HasOne(e => e.Header)
                            .WithOne(e => e.Blog)
                            .HasPrincipalKey<Blog>(e => new { e.Id1, e.Id2 })
                            .HasForeignKey<BlogHeader>("BlogId1", "BlogId2")
                            .IsRequired(false);
                    });
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog>()
                    .HasKey(e => new { e.Id1, e.Id2 });

                modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne(e => e.Header)
                    .HasPrincipalKey<Blog>(e => new { e.Id1, e.Id2 })
                    .HasForeignKey<BlogHeader>("BlogId1", "BlogId2")
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
                public BlogHeader? Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [ForeignKey("BlogId1, BlogId2")]
                public Blog? Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }

        public class ContextAnnotated1 : Context101
        {
            [PrimaryKey("Id1", "Id2")]
            public class Blog
            {
                public int Id1 { get; set; }
                public int Id2 { get; set; }

                [InverseProperty("Blog")]
                public BlogHeader? Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("BlogId1, BlogId2")]
                public Blog? Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneSelfReferencingNrtTest()
        => Assert.Throws<EqualException>(() => Model101Test()); // Issue #30355

    protected class OneToOneSelfReferencingNrt
    {
        public class Person
        {
            public int Id { get; set; }

            public int? HusbandId { get; set; }
            public Person? Husband { get; set; }
            public Person? Wife { get; set; }
        }

        public class PersonContext0 : Context101
        {
            public DbSet<Person> People
                => Set<Person>();
        }

        public class PersonContext1 : PersonContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Person>()
                    .HasOne(e => e.Husband)
                    .WithOne(e => e.Wife);
        }

        public class PersonContext2 : PersonContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Person>()
                    .HasOne(e => e.Husband)
                    .WithOne(e => e.Wife)
                    .HasForeignKey<Person>(e => e.HusbandId)
                    .IsRequired(false);
        }

        public class ContextAnnotated0 : Context101
        {
            public class Person
            {
                public int Id { get; set; }

                [ForeignKey("Husband")]
                public int? HusbandId { get; set; }

                [InverseProperty("Wife")]
                [ForeignKey("HusbandId")]
                public Person? Husband { get; set; }

                [InverseProperty("Husband")]
                public Person? Wife { get; set; }
            }

            public DbSet<Person> People
                => Set<Person>();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneRequiredWithoutCascadeDeleteNrtTest()
        => Model101Test();

    protected class OneToOneRequiredWithoutCascadeDeleteNrt
    {
        public class Blog
        {
            public int Id { get; set; }
            public BlogHeader? Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public int BlogId { get; set; }
            public Blog Blog { get; set; } = null!;
        }

        public class BlogContext0 : Context101
        {
            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne(e => e.Blog)
                    .OnDelete(DeleteBehavior.Restrict);
        }

        public class BlogContext1 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog>()
                    .HasOne(e => e.Header)
                    .WithOne(e => e.Blog)
                    .HasForeignKey<BlogHeader>(e => e.BlogId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
        }

        public class BlogContext2 : BlogContext0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<BlogHeader>()
                    .HasOne(e => e.Blog)
                    .WithOne(e => e.Header)
                    .HasForeignKey<BlogHeader>(e => e.BlogId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
        }

        public class ContextAnnotated0 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
                public BlogHeader? Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }
                public int BlogId { get; set; }

                [DeleteBehavior(DeleteBehavior.Restrict)]
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }

        public class ContextAnnotated1 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }

                [InverseProperty("Blog")]
                public BlogHeader? Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [ForeignKey("Blog")]
                [Required]
                public int BlogId { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("BlogId")]
                [Required]
                [DeleteBehavior(DeleteBehavior.Restrict)]
                public Blog Blog { get; set; } = null!;
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }
    }
}
