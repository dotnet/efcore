// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if !EXCLUDE_ON_MAC

using Google.Protobuf.WellKnownTypes;
using ProtoTest;

namespace Microsoft.EntityFrameworkCore;

public abstract class GrpcTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : GrpcTestBase<TFixture>.GrpcFixtureBase
{
    protected GrpcTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

    protected List<EntityTypeMapping> ExpectedMappings
        => new()
        {
            new EntityTypeMapping
            {
                Name = "PostTag",
                TableName = "PostTag",
                PrimaryKey =
                    "Key: PostTag (Dictionary<string, object>).PostsInTagDataPostId, PostTag (Dictionary<string, object>).TagsInPostDataTagId PK",
                Properties =
                {
                    "Property: PostTag (Dictionary<string, object>).PostsInTagDataPostId (no field, int) Indexer Required PK FK AfterSave:Throw",
                    "Property: PostTag (Dictionary<string, object>).TagsInPostDataTagId (no field, int) Indexer Required PK FK Index AfterSave:Throw",
                },
                Indexes = { "{'TagsInPostDataTagId'} ", },
                FKs =
                {
                    "ForeignKey: PostTag (Dictionary<string, object>) {'PostsInTagDataPostId'} -> Post {'PostId'} Required Cascade",
                    "ForeignKey: PostTag (Dictionary<string, object>) {'TagsInPostDataTagId'} -> Tag {'TagId'} Required Cascade",
                },
            },
            new EntityTypeMapping
            {
                Name = "ProtoTest.Author",
                TableName = "Author",
                PrimaryKey = "Key: Author.AuthorId PK",
                Properties =
                {
                    "Property: Author.AuthorId (authorId_, int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: Author.DateCreated (dateCreated_, Timestamp)",
                    "Property: Author.Name (name_, string)",
                },
            },
            new EntityTypeMapping
            {
                Name = "ProtoTest.Post",
                TableName = "Post",
                PrimaryKey = "Key: Post.PostId PK",
                Properties =
                {
                    "Property: Post.PostId (postId_, int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: Post.AuthorId (authorId_, int) Required FK Index",
                    "Property: Post.DateCreated (dateCreated_, Timestamp)",
                    "Property: Post.PostStat (postStat_, PostStatus) Required",
                    "Property: Post.Title (title_, string)",
                },
                Indexes = { "{'AuthorId'} ", },
                FKs = { "ForeignKey: Post {'AuthorId'} -> Author {'AuthorId'} Required Cascade ToPrincipal: PostAuthor", },
                Navigations = { "Navigation: Post.PostAuthor (postAuthor_, Author) ToPrincipal Author", },
                SkipNavigations =
                {
                    "SkipNavigation: Post.TagsInPostData (tagsInPostData_, RepeatedField<Tag>) CollectionTag Inverse: PostsInTagData",
                },
            },
            new EntityTypeMapping
            {
                Name = "ProtoTest.Tag",
                TableName = "Tag",
                PrimaryKey = "Key: Tag.TagId PK",
                Properties =
                {
                    "Property: Tag.TagId (tagId_, int) Required PK AfterSave:Throw ValueGenerated.OnAdd",
                    "Property: Tag.Name (name_, string)",
                },
                SkipNavigations =
                {
                    "SkipNavigation: Tag.PostsInTagData (postsInTagData_, RepeatedField<Post>) CollectionPost Inverse: TagsInPostData",
                },
            },
        };

    [ConditionalFact]
    public void Can_build_Grpc_model()
    {
        using var context = Fixture.CreateContext();

        var entityTypeMappings = context.Model.GetEntityTypes().Select(e => new EntityTypeMapping(e)).ToList();
        EntityTypeMapping.AssertEqual(ExpectedMappings, entityTypeMappings);
    }

    [ConditionalFact]
    public void Can_query_Grpc_model()
    {
        using var context = Fixture.CreateContext();

        var post = context.Set<Post>().Include(e => e.PostAuthor).Include(e => e.TagsInPostData).Single();

        Assert.Equal("Arthur's post", post.Title);
        Assert.Equal(new DateTime(2021, 9, 3, 12, 10, 0, DateTimeKind.Utc), post.DateCreated.ToDateTime());
        Assert.Equal(PostStatus.Published, post.PostStat);
        Assert.Equal("Arthur", post.PostAuthor.Name);
        Assert.Equal(new DateTime(1973, 9, 3, 12, 10, 0, DateTimeKind.Utc), post.PostAuthor.DateCreated.ToDateTime());

        Assert.Equal(2, post.TagsInPostData.Count);
        Assert.Contains("Puppies", post.TagsInPostData.Select(e => e.Name).ToList());
        Assert.Contains("Kittens", post.TagsInPostData.Select(e => e.Name).ToList());
        Assert.Same(post, post.TagsInPostData.First().PostsInTagData.First());
        Assert.Same(post, post.TagsInPostData.Skip(1).First().PostsInTagData.First());
    }

    public class GrpcContext(DbContextOptions options) : PoolableDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var timeStampConverter = new ValueConverter<Timestamp, DateTime>(
                v => v.ToDateTime(),
                v => new DateTime(v.Ticks, DateTimeKind.Utc).ToTimestamp());

            modelBuilder.Entity<Author>().Property(e => e.DateCreated).HasConversion(timeStampConverter);
            modelBuilder.Entity<Post>().Property(e => e.DateCreated).HasConversion(timeStampConverter);
            modelBuilder.Entity<Tag>();
        }
    }

    public abstract class GrpcFixtureBase : SharedStoreFixtureBase<GrpcContext>
    {
        protected override string StoreName
            => "GrpcTest";

        protected override Task SeedAsync(GrpcContext context)
        {
            var post = new Post
            {
                DateCreated = Timestamp.FromDateTime(new DateTime(2021, 9, 3, 12, 10, 0, DateTimeKind.Utc)),
                Title = "Arthur's post",
                PostAuthor = new Author
                {
                    DateCreated = Timestamp.FromDateTime(new DateTime(1973, 9, 3, 12, 10, 0, DateTimeKind.Utc)), Name = "Arthur"
                },
                PostStat = PostStatus.Published,
                TagsInPostData = { new Tag { Name = "Kittens" }, new Tag { Name = "Puppies" } }
            };

            context.Add(post);

            return context.SaveChangesAsync();
        }
    }
}

#endif
