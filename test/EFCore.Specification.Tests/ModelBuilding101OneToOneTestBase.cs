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
    public virtual void OneToOneRequiredTest()
        => Model101Test();

    protected class OneToOneRequired
    {
        public class Blog
        {
            public int Id { get; set; }
            public BlogHeader Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public int BlogId { get; set; }
            public Blog Blog { get; set; }
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
                public BlogHeader Header { get; set; }
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
                public Blog Blog { get; set; }
            }
        }
    }

    [ConditionalFact]
    public virtual void OneToOneOptionalTest()
        => Model101Test();

    protected class OneToOneOptional
    {
        public class Blog
        {
            public int Id { get; set; }
            public BlogHeader Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public int? BlogId { get; set; }
            public Blog Blog { get; set; }
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
                public BlogHeader Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [ForeignKey("Blog")]
                public int? BlogId { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("BlogId")]
                public Blog Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneRequiredPkToPkTest()
        => Model101Test();

    protected class OneToOneRequiredPkToPk
    {
        public class Blog
        {
            public int Id { get; set; }
            public BlogHeader Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public Blog Blog { get; set; }
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
                    .HasForeignKey<BlogHeader>()
                    .IsRequired();
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

                public BlogHeader Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [ForeignKey("Id")]
                [Required]
                public Blog Blog { get; set; }
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
                public BlogHeader Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("Id")]
                [Required]
                public Blog Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneRequiredWithShadowFkTest()
        => Model101Test();

    protected class OneToOneRequiredWithShadowFk
    {
        public class Blog
        {
            public int Id { get; set; }
            public BlogHeader Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public Blog Blog { get; set; }
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
                    .HasForeignKey<BlogHeader>("BlogId")
                    .IsRequired();
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
                public BlogHeader Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [ForeignKey("BlogId")]
                [Required]
                public Blog Blog { get; set; }
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
                public BlogHeader Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("BlogId")]
                [Required]
                public Blog Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneOptionalWithShadowFkTest()
        => Model101Test();

    protected class OneToOneOptionalWithShadowFk
    {
        public class Blog
        {
            public int Id { get; set; }
            public BlogHeader Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public Blog Blog { get; set; }
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
                public BlogHeader Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [ForeignKey("BlogId")]
                public Blog Blog { get; set; }
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
                public BlogHeader Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("BlogId")]
                public Blog Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneRequiredNoNavigationToPrincipalTest()
        => Model101Test();

    protected class OneToOneRequiredNoNavigationToPrincipal
    {
        public class Blog
        {
            public int Id { get; set; }
            public BlogHeader Header { get; set; }
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
                public BlogHeader Header { get; set; }
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
    public virtual void OneToOneOptionalNoNavigationToPrincipalTest()
        => Model101Test();

    protected class OneToOneOptionalNoNavigationToPrincipal
    {
        public class Blog
        {
            public int Id { get; set; }
            public BlogHeader Header { get; set; }
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
    public virtual void OneToOneRequiredWithShadowFkAndNoNavigationToPrincipalTest()
        => Model101Test();

    protected class OneToOneRequiredWithShadowFkAndNoNavigationToPrincipal
    {
        public class Blog
        {
            public int Id { get; set; }
            public BlogHeader Header { get; set; }
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
    public virtual void OneToOneOptionalWithShadowFkAndNoNavigationToPrincipalTest()
        => Model101Test();

    protected class OneToOneOptionalWithShadowFkAndNoNavigationToPrincipal
    {
        public class Blog
        {
            public int Id { get; set; }
            public BlogHeader Header { get; set; }
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
    public virtual void OneToOneRequiredNoNavigationToDependentsTest()
        => Model101Test();

    protected class OneToOneRequiredNoNavigationToDependents
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public int BlogId { get; set; }
            public Blog Blog { get; set; }
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
                public Blog Blog { get; set; }
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
    public virtual void OneToOneOptionalNoNavigationToDependentsTest()
        => Model101Test();

    protected class OneToOneOptionalNoNavigationToDependents
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public int? BlogId { get; set; }
            public Blog Blog { get; set; }
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
                public Blog Blog { get; set; }
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
    public virtual void OneToOneRequiredWithShadowFkAndNoNavigationToDependentsTest()
        => Model101Test();

    protected class OneToOneRequiredWithShadowFkAndNoNavigationToDependents
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public Blog Blog { get; set; }
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
                    .HasForeignKey<BlogHeader>("BlogId")
                    .IsRequired();
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
                public Blog Blog { get; set; }
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
    public virtual void OneToOneOptionalWithShadowFkAndNoNavigationToDependentsTest()
        => Model101Test();

    protected class OneToOneOptionalWithShadowFkAndNoNavigationToDependents
    {
        public class Blog
        {
            public int Id { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public Blog Blog { get; set; }
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
                public Blog Blog { get; set; }
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
    public virtual void OneToOneRequiredNoNavigationsTest()
        => Model101Test();

    protected class OneToOneRequiredNoNavigations
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
    public virtual void OneToOneOptionalNoNavigationsTest()
        => Model101Test();

    protected class OneToOneOptionalNoNavigations
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
    public virtual void OneToOneRequiredWithShadowFkAndNoNavigationsTest()
        => Model101Test();

    protected class OneToOneRequiredWithShadowFkAndNoNavigations
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
    public virtual void OneToOneOptionalWithShadowFkAndNoNavigationsTest()
        => Model101Test();

    protected class OneToOneOptionalWithShadowFkAndNoNavigations
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
    public virtual void OneToOneRequiredWithAlternateKeyTest()
        => Assert.Throws<EqualException>(() => Model101Test()); // Issue #30346

    protected class OneToOneRequiredWithAlternateKey
    {
        public class Blog
        {
            public int Id { get; set; }
            public int AlternateId { get; set; }
            public BlogHeader Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public int BlogId { get; set; }
            public Blog Blog { get; set; }
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
                public BlogHeader Header { get; set; }
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
                public Blog Blog { get; set; }
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
    public virtual void OneToOneOptionalWithAlternateKeyTest()
        => Assert.Throws<EqualException>(() => Model101Test()); // Issue #30346

    protected class OneToOneOptionalWithAlternateKey
    {
        public class Blog
        {
            public int Id { get; set; }
            public int AlternateId { get; set; }
            public BlogHeader Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public int? BlogId { get; set; }
            public Blog Blog { get; set; }
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
                public BlogHeader Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [ForeignKey("Blog")]
                public int? BlogId { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("BlogId")]
                public Blog Blog { get; set; }
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
    public virtual void OneToOneRequiredWithShadowFkWithAlternateKeyTest()
        => Assert.Throws<EqualException>(() => Model101Test()); // Issue #30346

    protected class OneToOneRequiredWithShadowFkWithAlternateKey
    {
        public class Blog
        {
            public int Id { get; set; }
            public int AlternateId { get; set; }
            public BlogHeader Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public Blog Blog { get; set; }
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
                    .HasPrincipalKey<Blog>(e => e.AlternateId)
                    .IsRequired();
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
                public BlogHeader Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [Required]
                public Blog Blog { get; set; }
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

        public class ContextAnnotated1 : Context101
        {
            public class Blog
            {
                public int Id { get; set; }
                public int AlternateId { get; set; }

                [InverseProperty("Blog")]
                public BlogHeader Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("BlogId")]
                [Required]
                public Blog Blog { get; set; }
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
    public virtual void OneToOneOptionalWithShadowFkWithAlternateKeyTest()
        => Assert.Throws<EqualException>(() => Model101Test()); // Issue #30346

    protected class OneToOneOptionalWithShadowFkWithAlternateKey
    {
        public class Blog
        {
            public int Id { get; set; }
            public int AlternateId { get; set; }
            public BlogHeader Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public Blog Blog { get; set; }
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
                public BlogHeader Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("BlogId")]
                public Blog Blog { get; set; }
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
    public virtual void OneToOneRequiredWithCompositeKeyTest()
        => Model101Test();

    protected class OneToOneRequiredWithCompositeKey
    {
        public class Blog
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }
            public BlogHeader Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public int BlogId1 { get; set; }
            public int BlogId2 { get; set; }
            public Blog Blog { get; set; }
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
                public BlogHeader Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }
                public int BlogId1 { get; set; }
                public int BlogId2 { get; set; }

                [ForeignKey("BlogId1, BlogId2")]
                public Blog Blog { get; set; }
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
                public BlogHeader Header { get; set; }
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
                public Blog Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneOptionalWithCompositeKeyTest()
        => Model101Test();

    protected class OneToOneOptionalWithCompositeKey
    {
        public class Blog
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }
            public BlogHeader Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public int BlogId1 { get; set; }
            public int? BlogId2 { get; set; }
            public Blog Blog { get; set; }
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
                public BlogHeader Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }
                public int BlogId1 { get; set; }
                public int? BlogId2 { get; set; }

                [ForeignKey("BlogId1, BlogId2")]
                public Blog Blog { get; set; }
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
                public BlogHeader Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [Required]
                public int BlogId1 { get; set; }

                public int? BlogId2 { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("BlogId1, BlogId2")]
                public Blog Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneOneToOneRequiredWithShadowFkWithCompositeKeyTest()
        => Model101Test();

    protected class OneToOneOneToOneRequiredWithShadowFkWithCompositeKey
    {
        public class Blog
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }
            public BlogHeader Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public Blog Blog { get; set; }
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
                            .HasPrincipalKey<Blog>(e => new { e.Id1, e.Id2 })
                            .IsRequired();
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
                public BlogHeader Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [ForeignKey("BlogId1, BlogId2")]
                [Required]
                public Blog Blog { get; set; }
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
                public BlogHeader Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("BlogId1, BlogId2")]
                [Required]
                public Blog Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneOptionalWithShadowFkWithCompositeKeyTest()
        => Model101Test();

    protected class OneToOneOptionalWithShadowFkWithCompositeKey
    {
        public class Blog
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }
            public BlogHeader Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public Blog Blog { get; set; }
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
                public BlogHeader Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [ForeignKey("BlogId1, BlogId2")]
                public Blog Blog { get; set; }
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
                public BlogHeader Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }

                [InverseProperty("Header")]
                [ForeignKey("BlogId1, BlogId2")]
                public Blog Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneSelfReferencingTest()
        => Assert.Throws<EqualException>(() => Model101Test()); // Issue #30355

    protected class OneToOneSelfReferencing
    {
        public class Person
        {
            public int Id { get; set; }

            public int? HusbandId { get; set; }
            public Person Husband { get; set; }
            public Person Wife { get; set; }
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
                public Person Husband { get; set; }

                [InverseProperty("Husband")]
                public Person Wife { get; set; }
            }

            public DbSet<Person> People
                => Set<Person>();
        }
    }

    [ConditionalFact]
    public virtual void OneToOneRequiredWithoutCascadeDeleteTest()
        => Model101Test();

    protected class OneToOneRequiredWithoutCascadeDelete
    {
        public class Blog
        {
            public int Id { get; set; }
            public BlogHeader Header { get; set; }
        }

        public class BlogHeader
        {
            public int Id { get; set; }
            public int BlogId { get; set; }
            public Blog Blog { get; set; }
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
                public BlogHeader Header { get; set; }
            }

            public class BlogHeader
            {
                public int Id { get; set; }
                public int BlogId { get; set; }

                [DeleteBehavior(DeleteBehavior.Restrict)]
                public Blog Blog { get; set; }
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
                public BlogHeader Header { get; set; }
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
                public Blog Blog { get; set; }
            }

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<BlogHeader> BlogHeaders
                => Set<BlogHeader>();
        }
    }
}
