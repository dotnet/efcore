// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public abstract partial class ModelBuilding101TestBase
{
    [ConditionalFact]
    public virtual void BasicManyToManyTest()
        => Model101Test();

    protected class BasicManyToMany
    {
        public class Post
        {
            public int Id { get; set; }
            public List<Tag> Tags { get; } = [];
        }

        public class Tag
        {
            public int Id { get; set; }
            public List<Post> Posts { get; } = [];
        }

        public class Context0 : Context101
        {
            public DbSet<Post> Posts
                => Set<Post>();

            public DbSet<Tag> Tags
                => Set<Tag>();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts);
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity(
                        "PostTag",
                        l => l.HasOne(typeof(Tag)).WithMany().HasForeignKey("TagsId").HasPrincipalKey(nameof(Tag.Id))
                            .OnDelete(DeleteBehavior.Cascade),
                        r => r.HasOne(typeof(Post)).WithMany().HasForeignKey("PostsId").HasPrincipalKey(nameof(Post.Id))
                            .OnDelete(DeleteBehavior.Cascade),
                        j => j.HasKey("PostsId", "TagsId"));
        }
    }

    [ConditionalFact]
    public virtual void UnidirectionalManyToManyTest()
        => Model101Test();

    protected class UnidirectionalManyToMany
    {
        public class Post
        {
            public int Id { get; set; }
            public List<Tag> Tags { get; } = [];
        }

        public class Tag
        {
            public int Id { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Post> Posts
                => Set<Post>();

            public DbSet<Tag> Tags
                => Set<Tag>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany()
                    .UsingEntity(
                        "PostTag",
                        l => l.HasOne(typeof(Tag)).WithMany().HasForeignKey("TagsId").HasPrincipalKey(nameof(Tag.Id))
                            .OnDelete(DeleteBehavior.Cascade),
                        r => r.HasOne(typeof(Post)).WithMany().HasForeignKey("PostId").HasPrincipalKey(nameof(Post.Id))
                            .OnDelete(DeleteBehavior.Cascade),
                        j => j.HasKey("PostId", "TagsId"));
        }
    }

    [ConditionalFact]
    public virtual void ManyToManyNamedJoinTableTest()
        => Model101Test();

    protected class ManyToManyNamedJoinTable
    {
        public class Post
        {
            public int Id { get; set; }
            public List<Tag> Tags { get; } = [];
        }

        public class Tag
        {
            public int Id { get; set; }
            public List<Post> Posts { get; } = [];
        }

        public class Context0 : Context101
        {
            public DbSet<Post> Posts
                => Set<Post>();

            public DbSet<Tag> Tags
                => Set<Tag>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity("PostsToTagsJoinTable");
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity(
                        "PostsToTagsJoinTable",
                        l => l.HasOne(typeof(Tag)).WithMany().HasForeignKey("TagsId").HasPrincipalKey(nameof(Tag.Id)),
                        r => r.HasOne(typeof(Post)).WithMany().HasForeignKey("PostsId").HasPrincipalKey(nameof(Post.Id)),
                        j => j.HasKey("PostsId", "TagsId"));
        }
    }

    [ConditionalFact]
    public virtual void ManyToManyNamedForeignKeyColumnsTest()
        => Model101Test();

    protected class ManyToManyNamedForeignKeyColumns
    {
        public class Post
        {
            public int Id { get; set; }
            public List<Tag> Tags { get; } = [];
        }

        public class Tag
        {
            public int Id { get; set; }
            public List<Post> Posts { get; } = [];
        }

        public class Context0 : Context101
        {
            public DbSet<Post> Posts
                => Set<Post>();

            public DbSet<Tag> Tags
                => Set<Tag>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity(
                        l => l.HasOne(typeof(Tag)).WithMany().HasForeignKey("TagForeignKey"),
                        r => r.HasOne(typeof(Post)).WithMany().HasForeignKey("PostForeignKey"));
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity(
                        "PostTag",
                        l => l.HasOne(typeof(Tag)).WithMany().HasForeignKey("TagForeignKey").HasPrincipalKey(nameof(Tag.Id)),
                        r => r.HasOne(typeof(Post)).WithMany().HasForeignKey("PostForeignKey").HasPrincipalKey(nameof(Post.Id)),
                        j => j.HasKey("PostForeignKey", "TagForeignKey"));
        }
    }

    [ConditionalFact]
    public virtual void ManyToManyWithJoinClassTest()
        => Model101Test();

    protected class ManyToManyWithJoinClass
    {
        public class Post
        {
            public int Id { get; set; }
            public List<Tag> Tags { get; } = [];
        }

        public class Tag
        {
            public int Id { get; set; }
            public List<Post> Posts { get; } = [];
        }

        public class PostTag
        {
            public int PostId { get; set; }
            public int TagId { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Post> Posts
                => Set<Post>();

            public DbSet<Tag> Tags
                => Set<Tag>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity<PostTag>();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity<PostTag>(
                        l => l.HasOne<Tag>().WithMany().HasForeignKey(e => e.TagId),
                        r => r.HasOne<Post>().WithMany().HasForeignKey(e => e.PostId));
        }

        public class Context2 : Context1
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity<PostTag>(
                        l => l.HasOne<Tag>().WithMany().HasForeignKey(e => e.TagId).HasPrincipalKey(e => e.Id),
                        r => r.HasOne<Post>().WithMany().HasForeignKey(e => e.PostId).HasPrincipalKey(e => e.Id),
                        j => j.HasKey(e => new { e.PostId, e.TagId }));
        }
    }

    [ConditionalFact]
    public virtual void ManyToManyWithNavsToJoinClassTest()
        => Model101Test();

    protected class ManyToManyWithNavsToJoinClass
    {
        public class Post
        {
            public int Id { get; set; }
            public List<Tag> Tags { get; } = [];
            public List<PostTag> PostTags { get; } = [];
        }

        public class Tag
        {
            public int Id { get; set; }
            public List<Post> Posts { get; } = [];
            public List<PostTag> PostTags { get; } = [];
        }

        public class PostTag
        {
            public int PostId { get; set; }
            public int TagId { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Post> Posts
                => Set<Post>();

            public DbSet<Tag> Tags
                => Set<Tag>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity<PostTag>();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity<PostTag>(
                        l => l.HasOne<Tag>().WithMany(e => e.PostTags),
                        r => r.HasOne<Post>().WithMany(e => e.PostTags));
        }

        public class Context2 : Context1
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity<PostTag>(
                        l => l.HasOne<Tag>().WithMany(e => e.PostTags).HasForeignKey(e => e.TagId).HasPrincipalKey(e => e.Id),
                        r => r.HasOne<Post>().WithMany(e => e.PostTags).HasForeignKey(e => e.PostId).HasPrincipalKey(e => e.Id),
                        j => j.HasKey(e => new { e.PostId, e.TagId }));
        }
    }

    [ConditionalFact]
    public virtual void ManyToManyWithNavsToAndFromJoinClassTest()
        => Model101Test();

    protected class ManyToManyWithNavsToAndFromJoinClass
    {
        public class Post
        {
            public int Id { get; set; }
            public List<Tag> Tags { get; } = [];
            public List<PostTag> PostTags { get; } = [];
        }

        public class Tag
        {
            public int Id { get; set; }
            public List<Post> Posts { get; } = [];
            public List<PostTag> PostTags { get; } = [];
        }

        public class PostTag
        {
            public int PostId { get; set; }
            public int TagId { get; set; }
            public Post Post { get; set; } = null!;
            public Tag Tag { get; set; } = null!;
        }

        public class Context0 : Context101
        {
            public DbSet<Post> Posts
                => Set<Post>();

            public DbSet<Tag> Tags
                => Set<Tag>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity<PostTag>();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity<PostTag>(
                        l => l.HasOne<Tag>(e => e.Tag).WithMany(e => e.PostTags),
                        r => r.HasOne<Post>(e => e.Post).WithMany(e => e.PostTags));
        }

        public class Context2 : Context1
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity<PostTag>(
                        l => l.HasOne<Tag>(e => e.Tag).WithMany(e => e.PostTags).HasForeignKey(e => e.TagId).HasPrincipalKey(e => e.Id),
                        r => r.HasOne<Post>(e => e.Post).WithMany(e => e.PostTags).HasForeignKey(e => e.PostId).HasPrincipalKey(e => e.Id),
                        j => j.HasKey(e => new { e.PostId, e.TagId }));
        }
    }

    [ConditionalFact]
    public virtual void ManyToManyWithNamedFksAndNavsToAndFromJoinClassTest()
        => Model101Test();

    protected class ManyToManyWithNamedFksAndNavsToAndFromJoinClass
    {
        public class Post
        {
            public int Id { get; set; }
            public List<Tag> Tags { get; } = [];
            public List<PostTag> PostTags { get; } = [];
        }

        public class Tag
        {
            public int Id { get; set; }
            public List<Post> Posts { get; } = [];
            public List<PostTag> PostTags { get; } = [];
        }

        public class PostTag
        {
            public int PostForeignKey { get; set; }
            public int TagForeignKey { get; set; }
            public Post Post { get; set; } = null!;
            public Tag Tag { get; set; } = null!;
        }

        public class Context0 : Context101
        {
            public DbSet<Post> Posts
                => Set<Post>();

            public DbSet<Tag> Tags
                => Set<Tag>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity<PostTag>(
                        l => l.HasOne<Tag>(e => e.Tag).WithMany(e => e.PostTags).HasForeignKey(e => e.TagForeignKey),
                        r => r.HasOne<Post>(e => e.Post).WithMany(e => e.PostTags).HasForeignKey(e => e.PostForeignKey));
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity<PostTag>(
                        l => l.HasOne<Tag>(e => e.Tag).WithMany(e => e.PostTags).HasForeignKey(e => e.TagForeignKey)
                            .HasPrincipalKey(e => e.Id),
                        r => r.HasOne<Post>(e => e.Post).WithMany(e => e.PostTags).HasForeignKey(e => e.PostForeignKey)
                            .HasPrincipalKey(e => e.Id),
                        j => j.HasKey(e => new { e.PostForeignKey, e.TagForeignKey }));
        }
    }

    [ConditionalFact]
    public virtual void ManyToManyAlternateKeysTest()
        => Model101Test();

    protected class ManyToManyAlternateKeys
    {
        public class Post
        {
            public int Id { get; set; }
            public int AlternateKey { get; set; }
            public List<Tag> Tags { get; } = [];
        }

        public class Tag
        {
            public int Id { get; set; }
            public int AlternateKey { get; set; }
            public List<Post> Posts { get; } = [];
        }

        public class Context0 : Context101
        {
            public DbSet<Post> Posts
                => Set<Post>();

            public DbSet<Tag> Tags
                => Set<Tag>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity(
                        l => l.HasOne(typeof(Tag)).WithMany().HasPrincipalKey(nameof(Tag.AlternateKey)),
                        r => r.HasOne(typeof(Post)).WithMany().HasPrincipalKey(nameof(Post.AlternateKey)));
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity(
                        "PostTag",
                        l => l.HasOne(typeof(Tag)).WithMany().HasForeignKey("TagsAlternateKey").HasPrincipalKey(nameof(Tag.AlternateKey)),
                        r => r.HasOne(typeof(Post)).WithMany().HasForeignKey("PostsAlternateKey")
                            .HasPrincipalKey(nameof(Post.AlternateKey)),
                        j => j.HasKey("PostsAlternateKey", "TagsAlternateKey"));
        }
    }

    [ConditionalFact]
    public virtual void ManyToManyWithNavsAndAlternateKeysTest()
        => Model101Test();

    protected class ManyToManyWithNavsAndAlternateKeys
    {
        public class Post
        {
            public int Id { get; set; }
            public int AlternateKey { get; set; }
            public List<Tag> Tags { get; } = [];
            public List<PostTag> PostTags { get; } = [];
        }

        public class Tag
        {
            public int Id { get; set; }
            public int AlternateKey { get; set; }
            public List<Post> Posts { get; } = [];
            public List<PostTag> PostTags { get; } = [];
        }

        public class PostTag
        {
            public int PostId { get; set; }
            public int TagId { get; set; }
            public Post Post { get; set; } = null!;
            public Tag Tag { get; set; } = null!;
        }

        public class Context0 : Context101
        {
            public DbSet<Post> Posts
                => Set<Post>();

            public DbSet<Tag> Tags
                => Set<Tag>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity<PostTag>(
                        l => l.HasOne<Tag>(e => e.Tag).WithMany(e => e.PostTags).HasPrincipalKey(e => e.AlternateKey),
                        r => r.HasOne<Post>(e => e.Post).WithMany(e => e.PostTags).HasPrincipalKey(e => e.AlternateKey));
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity<PostTag>(
                        l => l.HasOne<Tag>(e => e.Tag).WithMany(e => e.PostTags).HasForeignKey(e => e.TagId)
                            .HasPrincipalKey(e => e.AlternateKey),
                        r => r.HasOne<Post>(e => e.Post).WithMany(e => e.PostTags).HasForeignKey(e => e.PostId)
                            .HasPrincipalKey(e => e.AlternateKey),
                        j => j.HasKey(e => new { e.PostId, e.TagId }));
        }
    }

    [ConditionalFact]
    public virtual void ManyToManyWithJoinClassHavingPrimaryKeyTest()
        => Model101Test();

    protected class ManyToManyWithJoinClassHavingPrimaryKey
    {
        public class Post
        {
            public int Id { get; set; }
            public List<Tag> Tags { get; } = [];
        }

        public class Tag
        {
            public int Id { get; set; }
            public List<Post> Posts { get; } = [];
        }

        public class PostTag
        {
            public int Id { get; set; }
            public int PostId { get; set; }
            public int TagId { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Post> Posts
                => Set<Post>();

            public DbSet<Tag> Tags
                => Set<Tag>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity<PostTag>();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity<PostTag>(
                        l => l.HasOne<Tag>().WithMany().HasForeignKey(e => e.TagId).HasPrincipalKey(e => e.Id),
                        r => r.HasOne<Post>().WithMany().HasForeignKey(e => e.PostId).HasPrincipalKey(e => e.Id),
                        j => j.HasKey(e => e.Id));
        }
    }

    [ConditionalFact]
    public virtual void ManyToManyWithPrimaryKeyInJoinEntityTest()
        => Model101Test();

    protected class ManyToManyWithPrimaryKeyInJoinEntity
    {
        public class Post
        {
            public int Id { get; set; }
            public List<Tag> Tags { get; } = [];
        }

        public class Tag
        {
            public int Id { get; set; }
            public List<Post> Posts { get; } = [];
        }

        public class Context0 : Context101
        {
            public DbSet<Post> Posts
                => Set<Post>();

            public DbSet<Tag> Tags
                => Set<Tag>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity(
                        j =>
                        {
                            j.IndexerProperty<int>("Id");
                            j.HasKey("Id");
                        });
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity(
                        "PostTag",
                        l => l.HasOne(typeof(Tag)).WithMany().HasForeignKey("TagsId").HasPrincipalKey(nameof(Tag.Id)),
                        r => r.HasOne(typeof(Post)).WithMany().HasForeignKey("PostsId").HasPrincipalKey(nameof(Post.Id)),
                        j =>
                        {
                            j.IndexerProperty<int>("Id");
                            j.HasKey("Id");
                        });
        }
    }

    [ConditionalFact]
    public virtual void ManyToManyWithPayloadAndNavsToJoinClassTest()
        => Model101Test();

    protected class ManyToManyWithPayloadAndNavsToJoinClass
    {
        public class Post
        {
            public int Id { get; set; }
            public List<Tag> Tags { get; } = [];
            public List<PostTag> PostTags { get; } = [];
        }

        public class Tag
        {
            public int Id { get; set; }
            public List<Post> Posts { get; } = [];
            public List<PostTag> PostTags { get; } = [];
        }

        public class PostTag
        {
            public int PostId { get; set; }
            public int TagId { get; set; }
            public DateTime CreatedOn { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Post> Posts
                => Set<Post>();

            public DbSet<Tag> Tags
                => Set<Tag>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity<PostTag>();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity<PostTag>(
                        l => l.HasOne<Tag>().WithMany(e => e.PostTags).HasForeignKey(e => e.TagId).HasPrincipalKey(e => e.Id),
                        r => r.HasOne<Post>().WithMany(e => e.PostTags).HasForeignKey(e => e.PostId).HasPrincipalKey(e => e.Id),
                        j =>
                        {
                            j.HasKey(e => new { e.PostId, e.TagId });
                        });
        }
    }

    [ConditionalFact]
    public virtual void ManyToManyWithNoCascadeDeleteTest()
        => Model101Test();

    protected class ManyToManyWithNoCascadeDelete
    {
        public class Post
        {
            public int Id { get; set; }
            public List<Tag> Tags { get; } = [];
        }

        public class Tag
        {
            public int Id { get; set; }
            public List<Post> Posts { get; } = [];
        }

        public class Context0 : Context101
        {
            public DbSet<Post> Posts
                => Set<Post>();

            public DbSet<Tag> Tags
                => Set<Tag>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity(
                        l => l.HasOne(typeof(Tag)).WithMany().OnDelete(DeleteBehavior.Restrict),
                        r => r.HasOne(typeof(Post)).WithMany().OnDelete(DeleteBehavior.Restrict));
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity(
                        "PostTag",
                        l => l.HasOne(typeof(Tag)).WithMany().HasForeignKey("TagsId").HasPrincipalKey(nameof(Tag.Id))
                            .OnDelete(DeleteBehavior.Restrict),
                        r => r.HasOne(typeof(Post)).WithMany().HasForeignKey("PostsId").HasPrincipalKey(nameof(Post.Id))
                            .OnDelete(DeleteBehavior.Restrict),
                        j => j.HasKey("PostsId", "TagsId"));
        }
    }

    [ConditionalFact]
    public virtual void SelfReferencingManyToManyTest()
        => Model101Test();

    protected class SelfReferencingManyToMany
    {
        public class Person
        {
            public int Id { get; set; }
            public List<Person> Parents { get; } = [];
            public List<Person> Children { get; } = [];
        }

        public class Context0 : Context101
        {
            public DbSet<Person> People
                => Set<Person>();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Person>()
                    .HasMany(e => e.Children)
                    .WithMany(e => e.Parents);
        }

        public class Context2 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Person>()
                    .HasMany(e => e.Children)
                    .WithMany(e => e.Parents)
                    .UsingEntity(
                        "PersonPerson",
                        l => l.HasOne(typeof(Person)).WithMany().HasForeignKey("ChildrenId").HasPrincipalKey(nameof(Person.Id)),
                        r => r.HasOne(typeof(Person)).WithMany().HasForeignKey("ParentsId").HasPrincipalKey(nameof(Person.Id)),
                        j => j.HasKey("ChildrenId", "ParentsId"));
        }
    }

    [ConditionalFact]
    public virtual void SelfReferencingUnidirectionalManyToManyTest()
        => Model101Test();

    protected class SelfReferencingUnidirectionalManyToMany
    {
        public class Person
        {
            public int Id { get; set; }
            public List<Person> Friends { get; } = [];
        }

        public class Context0 : Context101
        {
            public DbSet<Person> People
                => Set<Person>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Person>()
                    .HasMany(e => e.Friends)
                    .WithMany();
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Person>()
                    .HasMany(e => e.Friends)
                    .WithMany()
                    .UsingEntity(
                        "PersonPerson",
                        l => l.HasOne(typeof(Person)).WithMany().HasForeignKey("FriendsId").HasPrincipalKey(nameof(Person.Id)),
                        r => r.HasOne(typeof(Person)).WithMany().HasForeignKey("PersonId").HasPrincipalKey(nameof(Person.Id)),
                        j => j.HasKey("FriendsId", "PersonId"));
        }
    }

    [ConditionalFact]
    public virtual void ManyToManyWithCustomSharedTypeEntityTypeTest()
        => Model101Test();

    protected class ManyToManyWithCustomSharedTypeEntityType
    {
        public class Post
        {
            public int Id { get; set; }
            public List<Tag> Tags { get; } = [];
            public List<JoinType> PostTags { get; } = [];
        }

        public class Tag
        {
            public int Id { get; set; }
            public List<Post> Posts { get; } = [];
            public List<JoinType> PostTags { get; } = [];
        }

        public class Blog
        {
            public int Id { get; set; }
            public List<Author> Authors { get; } = [];
            public List<JoinType> BlogAuthors { get; } = [];
        }

        public class Author
        {
            public int Id { get; set; }
            public List<Blog> Blogs { get; } = [];
            public List<JoinType> BlogAuthors { get; } = [];
        }

        public class JoinType
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }
            public DateTime CreatedOn { get; set; }
        }

        public class Context0 : Context101
        {
            public DbSet<Post> Posts
                => Set<Post>();

            public DbSet<Tag> Tags
                => Set<Tag>();

            public DbSet<Blog> Blogs
                => Set<Blog>();

            public DbSet<Author> Authors
                => Set<Author>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity<JoinType>(
                        "PostTag",
                        l => l.HasOne<Tag>().WithMany(e => e.PostTags).HasForeignKey(e => e.Id1),
                        r => r.HasOne<Post>().WithMany(e => e.PostTags).HasForeignKey(e => e.Id2));

                modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Authors)
                    .WithMany(e => e.Blogs)
                    .UsingEntity<JoinType>(
                        "BlogAuthor",
                        l => l.HasOne<Author>().WithMany(e => e.BlogAuthors).HasForeignKey(e => e.Id1),
                        r => r.HasOne<Blog>().WithMany(e => e.BlogAuthors).HasForeignKey(e => e.Id2));
            }
        }

        public class Context1 : Context0
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Post>()
                    .HasMany(e => e.Tags)
                    .WithMany(e => e.Posts)
                    .UsingEntity<JoinType>(
                        "PostTag",
                        l => l.HasOne<Tag>().WithMany(e => e.PostTags).HasForeignKey(e => e.Id1).HasPrincipalKey(e => e.Id),
                        r => r.HasOne<Post>().WithMany(e => e.PostTags).HasForeignKey(e => e.Id2).HasPrincipalKey(e => e.Id),
                        j => j.HasKey(e => new { e.Id1, e.Id2 }));

                modelBuilder.Entity<Blog>()
                    .HasMany(e => e.Authors)
                    .WithMany(e => e.Blogs)
                    .UsingEntity<JoinType>(
                        "BlogAuthor",
                        l => l.HasOne<Author>().WithMany(e => e.BlogAuthors).HasForeignKey(e => e.Id1).HasPrincipalKey(e => e.Id),
                        r => r.HasOne<Blog>().WithMany(e => e.BlogAuthors).HasForeignKey(e => e.Id2).HasPrincipalKey(e => e.Id),
                        j => j.HasKey(e => new { e.Id1, e.Id2 }));
            }
        }
    }
}
