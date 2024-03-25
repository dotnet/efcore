// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0052 // Remove unread private members
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class WithConstructorsTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : WithConstructorsTestBase<TFixture>.WithConstructorsFixtureBase, new()
{
    protected WithConstructorsTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

    protected DbContext CreateContext()
        => Fixture.CreateContext();

    protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
    {
    }

    [ConditionalFact]
    public virtual Task Query_and_update_using_constructors_with_property_parameters()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext, UseTransaction, async context =>
            {
                var blog = context.Set<Blog>().Include(e => e.Posts).Single();

                Assert.Equal("Puppies", blog.Title);

                var posts = blog.Posts.OrderBy(e => e.Title).ToList();

                Assert.Equal(2, posts.Count);

                Assert.StartsWith("Baxter", posts[0].Title);
                Assert.StartsWith("He", posts[0].Content);

                Assert.StartsWith("Golden", posts[1].Title);
                Assert.StartsWith("Smaller", posts[1].Content);

                posts[0].Content += " He is just trying to make a living.";

                blog.AddPost(new Post("Olive has a TPLO", "Yes she does."));

                var newBlog = context.Add(new Blog("Cats", 100)).Entity;
                newBlog.AddPost(new Post("Baxter is a cat.", "With dog friends."));

                await context.SaveChangesAsync();
            }, async context =>
            {
                var blogs = await context.Set<Blog>().Include(e => e.Posts).OrderBy(e => e.Title).ToListAsync();

                Assert.Equal(2, blogs.Count);

                Assert.Equal("Cats", blogs[0].Title);
                Assert.Equal("Puppies", blogs[1].Title);

                var posts = blogs[0].Posts.OrderBy(e => e.Title).ToList();

                Assert.Single(posts);

                Assert.StartsWith("Baxter", posts[0].Title);
                Assert.StartsWith("With dog", posts[0].Content);

                posts = blogs[1].Posts.OrderBy(e => e.Title).ToList();

                Assert.Equal(3, posts.Count);

                Assert.StartsWith("Baxter", posts[0].Title);
                Assert.EndsWith("living.", posts[0].Content);

                Assert.StartsWith("Golden", posts[1].Title);
                Assert.StartsWith("Smaller", posts[1].Content);

                Assert.StartsWith("Olive", posts[2].Title);
                Assert.StartsWith("Yes", posts[2].Content);
            });

    [ConditionalFact]
    public virtual void Query_with_keyless_type()
    {
        using var context = CreateContext();
        var blogs = context.Set<BlogQuery>().ToList();

        Assert.Single(blogs);
        Assert.Equal("Puppies", blogs[0].Title);
    }

    [ConditionalFact]
    public virtual void Query_with_context_injected()
    {
        using (var context = CreateContext())
        {
            Assert.Same(context, context.Set<HasContext<DbContext>>().Single().Context);
            Assert.Same(context, context.Set<HasContext<WithConstructorsContext>>().Single().Context);
            Assert.Null(context.Set<HasContext<OtherContext>>().Single().Context);
        }

        // Ensure new context instance is injected on repeated uses
        using (var context = CreateContext())
        {
            Assert.Same(context, context.Set<HasContext<DbContext>>().Single().Context);
            Assert.Same(context, context.Set<HasContext<WithConstructorsContext>>().Single().Context);
            Assert.Null(context.Set<HasContext<OtherContext>>().Single().Context);
        }
    }

    [ConditionalFact]
    public virtual void Query_with_context_injected_into_property()
    {
        using (var context = CreateContext())
        {
            Assert.Same(context, context.Set<HasContextProperty<DbContext>>().Single().Context);
            Assert.Same(context, context.Set<HasContextProperty<WithConstructorsContext>>().Single().Context);
            Assert.Null(context.Set<HasContextProperty<OtherContext>>().Single().Context);
        }

        // Ensure new context instance is injected on repeated uses
        using (var context = CreateContext())
        {
            Assert.Same(context, context.Set<HasContextProperty<DbContext>>().Single().Context);
            Assert.Same(context, context.Set<HasContextProperty<WithConstructorsContext>>().Single().Context);
            Assert.Null(context.Set<HasContextProperty<OtherContext>>().Single().Context);
        }
    }

    [ConditionalFact]
    public virtual void Query_with_context_injected_into_constructor_with_property()
    {
        HasContextPc<DbContext> entityWithBase;
        HasContextPc<WithConstructorsContext> entityWithDerived;
        HasContextPc<OtherContext> entityWithOther;

        using (var context = CreateContext())
        {
            entityWithBase = context.Set<HasContextPc<DbContext>>().Single();
            Assert.Same(context, entityWithBase.GetContext());
            Assert.False(entityWithBase.SetterCalled);

            entityWithDerived = context.Set<HasContextPc<WithConstructorsContext>>().Single();
            Assert.Same(context, entityWithDerived.GetContext());
            Assert.False(entityWithDerived.SetterCalled);

            entityWithOther = context.Set<HasContextPc<OtherContext>>().Single();
            Assert.Null(entityWithOther.GetContext());
            Assert.False(entityWithOther.SetterCalled);

            context.Entry(entityWithBase).State = EntityState.Detached;
            context.Entry(entityWithDerived).State = EntityState.Detached;
            context.Entry(entityWithOther).State = EntityState.Detached;

            Assert.Null(entityWithBase.GetContext());
            Assert.True(entityWithBase.SetterCalled);
            Assert.Null(entityWithDerived.GetContext());
            Assert.True(entityWithDerived.SetterCalled);
            Assert.Null(entityWithOther.GetContext());
            Assert.False(entityWithOther.SetterCalled); // Because value didn't changed
        }

        using (var context = CreateContext())
        {
            context.Attach(entityWithBase);
            context.Attach(entityWithDerived);
            context.Attach(entityWithOther);

            Assert.Same(context, entityWithBase.GetContext());
            Assert.True(entityWithBase.SetterCalled);
            Assert.Same(context, entityWithDerived.GetContext());
            Assert.True(entityWithDerived.SetterCalled);
            Assert.Null(entityWithOther.GetContext());
            Assert.False(entityWithOther.SetterCalled); // Because value didn't changed
        }
    }

    [ConditionalFact]
    public virtual void Attaching_entity_sets_context()
    {
        int id1, id2, id3;
        using (var context = CreateContext())
        {
            id1 = context.Set<HasContextProperty<DbContext>>().Single().Id;
            id2 = context.Set<HasContextProperty<WithConstructorsContext>>().Single().Id;
            id3 = context.Set<HasContextProperty<OtherContext>>().Single().Id;
        }

        using (var context = CreateContext())
        {
            var entityWithBase = new HasContextProperty<DbContext> { Id = id1 };
            var entityWithDerived = new HasContextProperty<WithConstructorsContext> { Id = id2 };
            var entityWithOther = new HasContextProperty<OtherContext> { Id = id3 };

            context.Attach(entityWithBase);
            context.Attach(entityWithDerived);
            context.Attach(entityWithOther);

            Assert.Same(context, entityWithBase.Context);
            Assert.Same(context, entityWithDerived.Context);
            Assert.Null(entityWithOther.Context);
        }
    }

    [ConditionalFact]
    public virtual void Query_with_EntityType_injected()
    {
        using var context = CreateContext();
        Assert.Same(
            context.Model.FindEntityType(typeof(HasEntityType)),
            context.Set<HasEntityType>().Single().GetEntityType());
    }

    [ConditionalFact]
    public virtual void Query_with_EntityType_injected_into_property()
    {
        using var context = CreateContext();
        Assert.Same(
            context.Model.FindEntityType(typeof(HasEntityTypeProperty)),
            context.Set<HasEntityTypeProperty>().Single().EntityType);
    }

    [ConditionalFact]
    public virtual void Query_with_EntityType_injected_into_constructor_with_property()
    {
        HasEntityTypePc entity;

        using (var context = CreateContext())
        {
            entity = context.Set<HasEntityTypePc>().Single();
            Assert.Same(context.Model.FindEntityType(typeof(HasEntityTypePc)), entity.GetEntityType());
            Assert.False(entity.SetterCalled);

            context.Entry(entity).State = EntityState.Detached;

            Assert.Null(entity.GetEntityType());
            Assert.True(entity.SetterCalled);
        }

        using (var context = CreateContext())
        {
            context.Attach(entity);

            Assert.True(entity.SetterCalled);
            Assert.Same(context.Model.FindEntityType(typeof(HasEntityTypePc)), entity.GetEntityType());
        }
    }

    [ConditionalFact]
    public virtual void Attaching_entity_sets_EntityType()
    {
        int id;
        using (var context = CreateContext())
        {
            id = context.Set<HasEntityTypeProperty>().Single().Id;
        }

        using (var context = CreateContext())
        {
            var entity = new HasEntityTypeProperty { Id = id };

            context.Attach(entity);

            Assert.Same(context.Model.FindEntityType(typeof(HasEntityTypeProperty)), entity.EntityType);
        }
    }

    [ConditionalFact]
    public virtual void Query_with_StateManager_injected()
    {
        using var context = CreateContext();
        Assert.Same(
            context.GetService<IStateManager>(),
            context.Set<HasStateManager>().Single().GetStateManager());
    }

    [ConditionalFact]
    public virtual void Query_with_StateManager_injected_into_property()
    {
        using var context = CreateContext();
        Assert.Same(
            context.GetService<IStateManager>(),
            context.Set<HasStateManagerProperty>().Single().StateManager);
    }

    [ConditionalFact]
    public virtual void Query_with_StateManager_injected_into_constructor_with_property()
    {
        HasStateManagerPc entity;

        using (var context = CreateContext())
        {
            entity = context.Set<HasStateManagerPc>().Single();
            Assert.Same(context.GetService<IStateManager>(), entity.GetStateManager());
            Assert.False(entity.SetterCalled);

            context.Entry(entity).State = EntityState.Detached;

            Assert.Null(entity.GetStateManager());
            Assert.True(entity.SetterCalled);
        }

        using (var context = CreateContext())
        {
            context.Attach(entity);

            Assert.True(entity.SetterCalled);
            Assert.Same(context.GetService<IStateManager>(), entity.GetStateManager());
        }
    }

    [ConditionalFact]
    public virtual void Attaching_entity_sets_StateManager()
    {
        int id;
        using (var context = CreateContext())
        {
            id = context.Set<HasStateManagerProperty>().Single().Id;
        }

        using (var context = CreateContext())
        {
            var entity = new HasStateManagerProperty { Id = id };

            context.Attach(entity);

            Assert.Same(context.GetService<IStateManager>(), entity.StateManager);
        }
    }

    [ConditionalFact]
    public virtual void Query_with_loader_injected_for_reference()
    {
        using var context = CreateContext();
        var post = context.Set<LazyPost>().OrderBy(e => e.Id).First();

        Assert.NotNull(post.LazyBlog);
        Assert.Contains(post, post.LazyBlog.LazyPosts);
    }

    [ConditionalFact]
    public virtual void Query_with_loader_injected_for_collections()
    {
        using var context = CreateContext();
        var blog = context.Set<LazyBlog>().Single();

        Assert.Equal(2, blog.LazyPosts.Count());
        Assert.Same(blog, blog.LazyPosts.First().LazyBlog);
        Assert.Same(blog, blog.LazyPosts.Skip(1).First().LazyBlog);
    }

    [ConditionalFact]
    public virtual async Task Query_with_loader_injected_for_reference_async()
    {
        using var context = CreateContext();
        var post = await context.Set<LazyAsyncPost>().OrderBy(e => e.Id).FirstAsync();

        var loaded = await post.LoadBlogAsync();

        Assert.NotNull(loaded);
        Assert.Same(loaded, post.LazyAsyncBlog);
        Assert.Contains(post, post.LazyAsyncBlog.LazyAsyncPosts);
    }

    [ConditionalFact]
    public virtual async Task Query_with_loader_injected_for_collections_async()
    {
        using var context = CreateContext();
        var blog = await context.Set<LazyAsyncBlog>().SingleAsync();

        var loaded = await blog.LoadPostsAsync();

        Assert.Same(loaded, blog.LazyAsyncPosts);
        Assert.Equal(2, blog.LazyAsyncPosts.Count());
        Assert.Same(blog, blog.LazyAsyncPosts.First().LazyAsyncBlog);
        Assert.Same(blog, blog.LazyAsyncPosts.Skip(1).First().LazyAsyncBlog);
    }

    [ConditionalFact]
    public virtual void Query_with_POCO_loader_injected_for_reference()
    {
        using var context = CreateContext();
        var post = context.Set<LazyPocoPost>().OrderBy(e => e.Id).First();

        Assert.NotNull(post.LazyPocoBlog);
        Assert.Contains(post, post.LazyPocoBlog.LazyPocoPosts);
    }

    [ConditionalFact]
    public virtual void Query_with_POCO_loader_injected_for_collections()
    {
        using var context = CreateContext();
        var blog = context.Set<LazyPocoBlog>().Single();

        Assert.Equal(2, blog.LazyPocoPosts.Count());
        Assert.Same(blog, blog.LazyPocoPosts.First().LazyPocoBlog);
        Assert.Same(blog, blog.LazyPocoPosts.Skip(1).First().LazyPocoBlog);
    }

    [ConditionalFact]
    public virtual async Task Query_with_loader_delegate_injected_for_reference_async()
    {
        using var context = CreateContext();
        var post = await context.Set<LazyAsyncPocoPost>().OrderBy(e => e.Id).FirstAsync();

        var loaded = await post.LoadBlogAsync();

        Assert.NotNull(loaded);
        Assert.Same(loaded, post.LazyAsyncPocoBlog);
        Assert.Contains(post, post.LazyAsyncPocoBlog.LazyAsyncPocoPosts);
    }

    [ConditionalFact]
    public virtual async Task Query_with_loader_delegate_injected_for_collections_async()
    {
        using var context = CreateContext();
        var blog = await context.Set<LazyAsyncPocoBlog>().SingleAsync();

        var loaded = await blog.LoadPostsAsync();

        Assert.Same(loaded, blog.LazyAsyncPocoPosts);
        Assert.Equal(2, blog.LazyAsyncPocoPosts.Count());
        Assert.Same(blog, blog.LazyAsyncPocoPosts.First().LazyAsyncPocoBlog);
        Assert.Same(blog, blog.LazyAsyncPocoPosts.Skip(1).First().LazyAsyncPocoBlog);
    }

    [ConditionalFact]
    public virtual void Query_with_loader_injected_into_property_for_reference()
    {
        using var context = CreateContext();
        var post = context.Set<LazyPropertyPost>().OrderBy(e => e.Id).First();

        Assert.NotNull(post.LazyPropertyBlog);
        Assert.Contains(post, post.LazyPropertyBlog.LazyPropertyPosts);
    }

    [ConditionalFact]
    public virtual void Query_with_loader_injected_into_property_for_collections()
    {
        using var context = CreateContext();
        var blog = context.Set<LazyPropertyBlog>().Single();

        Assert.Equal(2, blog.LazyPropertyPosts.Count());
        Assert.Same(blog, blog.LazyPropertyPosts.First().LazyPropertyBlog);
        Assert.Same(blog, blog.LazyPropertyPosts.Skip(1).First().LazyPropertyBlog);
    }

    [ConditionalFact]
    public virtual void Attaching_entity_sets_lazy_loader()
    {
        int id, fk;
        using (var context = CreateContext())
        {
            var post = context.Set<LazyPropertyPost>().OrderBy(e => e.Id).First();
            id = post.Id;
            fk = post.LazyPropertyBlogId;
        }

        using (var context = CreateContext())
        {
            var post = new LazyPropertyPost { Id = id, LazyPropertyBlogId = fk };
            Assert.Null(post.GetLoader());

            context.Attach(post);

            Assert.NotNull(post.GetLoader());

            Assert.NotNull(post.LazyPropertyBlog);
            Assert.Contains(post, post.LazyPropertyBlog.LazyPropertyPosts);
        }
    }

    [ConditionalFact]
    public virtual void Detaching_entity_resets_lazy_loader_so_it_can_be_reattached()
    {
        LazyPropertyPost post;
        using (var context = CreateContext())
        {
            post = context.Set<LazyPropertyPost>().OrderBy(e => e.Id).First();
            Assert.NotNull(post.GetLoader());
            context.Entry(post).State = EntityState.Detached;
        }

        Assert.NotNull(post.GetLoader());
        Assert.Null(post.LazyPropertyBlog);

        using (var context = CreateContext())
        {
            context.Attach(post);
            Assert.NotNull(post.GetLoader());
            Assert.NotNull(post.LazyPropertyBlog);
            Assert.Contains(post, post.LazyPropertyBlog.LazyPropertyPosts);
        }
    }

    [ConditionalFact]
    public virtual void Query_with_loader_injected_into_field_for_reference()
    {
        using var context = CreateContext();
        var post = context.Set<LazyFieldPost>().OrderBy(e => e.Id).First();

        Assert.NotNull(post.LazyFieldBlog);
        Assert.Contains(post, post.LazyFieldBlog.LazyFieldPosts);
    }

    [ConditionalFact]
    public virtual void Query_with_loader_injected_into_field_for_collections()
    {
        using var context = CreateContext();
        var blog = context.Set<LazyFieldBlog>().Single();

        Assert.Equal(2, blog.LazyFieldPosts.Count());
        Assert.Same(blog, blog.LazyFieldPosts.First().LazyFieldBlog);
        Assert.Same(blog, blog.LazyFieldPosts.Skip(1).First().LazyFieldBlog);
    }

    [ConditionalFact]
    public virtual void Attaching_entity_sets_lazy_loader_field()
    {
        int id, fk;
        using (var context = CreateContext())
        {
            var post = context.Set<LazyFieldPost>().OrderBy(e => e.Id).First();
            id = post.Id;
            fk = post.LazyFieldBlogId;
        }

        using (var context = CreateContext())
        {
            var post = new LazyFieldPost { Id = id, LazyFieldBlogId = fk };
            Assert.Null(post.GetLoader());

            context.Attach(post);

            Assert.NotNull(post.GetLoader());

            Assert.NotNull(post.LazyFieldBlog);
            Assert.Contains(post, post.LazyFieldBlog.LazyFieldPosts);
        }
    }

    [ConditionalFact]
    public virtual void Detaching_entity_resets_lazy_loader_field_so_it_can_be_reattached()
    {
        LazyFieldPost post;
        using (var context = CreateContext())
        {
            post = context.Set<LazyFieldPost>().OrderBy(e => e.Id).First();
            Assert.NotNull(post.GetLoader());
            context.Entry(post).State = EntityState.Detached;
        }

        Assert.NotNull(post.GetLoader());
        Assert.Null(post.LazyFieldBlog);

        using (var context = CreateContext())
        {
            context.Attach(post);
            Assert.NotNull(post.GetLoader());
            Assert.NotNull(post.LazyFieldBlog);
            Assert.Contains(post, post.LazyFieldBlog.LazyFieldPosts);
        }
    }

    [ConditionalFact]
    public virtual void Attaching_entity_sets_lazy_loader_delegate()
    {
        int id, fk;
        using (var context = CreateContext())
        {
            var post = context.Set<LazyPcsPost>().OrderBy(e => e.Id).First();
            id = post.Id;
            fk = post.LazyPcsBlogId;
        }

        using (var context = CreateContext())
        {
            var post = new LazyPcsPost { Id = id, LazyPcsBlogId = fk };
            Assert.Null(post.GetLoader());

            context.Attach(post);

            Assert.NotNull(post.GetLoader());

            Assert.NotNull(post.LazyPcsBlog);
            Assert.Contains(post, post.LazyPcsBlog.LazyPcsPosts);
        }
    }

    [ConditionalFact]
    public virtual void Detaching_entity_resets_lazy_loader_delegate_so_it_can_be_reattached()
    {
        LazyPcsPost post;
        using (var context = CreateContext())
        {
            post = context.Set<LazyPcsPost>().OrderBy(e => e.Id).First();

            Assert.NotNull(post.GetLoader());

            context.Entry(post).State = EntityState.Detached;

            Assert.Null(post.GetLoader());
        }

        Assert.Null(post.LazyPcsBlog);

        using (var context = CreateContext())
        {
            context.Attach(post);

            Assert.NotNull(post.GetLoader());

            Assert.NotNull(post.LazyPcsBlog);
            Assert.Contains(post, post.LazyPcsBlog.LazyPcsPosts);
        }
    }

    [ConditionalFact]
    public virtual void Query_with_loader_delegate_injected_into_property_for_reference()
    {
        using var context = CreateContext();
        var post = context.Set<LazyPsPost>().OrderBy(e => e.Id).First();

        Assert.NotNull(post.LazyPsBlog);
        Assert.Contains(post, post.LazyPsBlog.LazyPsPosts);
    }

    [ConditionalFact]
    public virtual void Query_with_loader_delgate_injected_into_property_for_collections()
    {
        using var context = CreateContext();
        var blog = context.Set<LazyPsBlog>().Single();

        Assert.Equal(2, blog.LazyPsPosts.Count());
        Assert.Same(blog, blog.LazyPsPosts.First().LazyPsBlog);
        Assert.Same(blog, blog.LazyPsPosts.Skip(1).First().LazyPsBlog);
    }

    [ConditionalFact]
    public virtual async Task Query_with_loader_delegate_injected_into_property_for_reference_async()
    {
        using var context = CreateContext();
        var post = await context.Set<LazyAsyncPsPost>().OrderBy(e => e.Id).FirstAsync();

        var loaded = await post.LoadBlogAsync();

        Assert.NotNull(loaded);
        Assert.Same(loaded, post.LazyAsyncPsBlog);
        Assert.Contains(post, post.LazyAsyncPsBlog.LazyAsyncPsPosts);
    }

    [ConditionalFact]
    public virtual async Task Query_with_loader_delegate_injected_into_property_for_collections_async()
    {
        using var context = CreateContext();
        var blog = await context.Set<LazyAsyncPsBlog>().SingleAsync();

        var loaded = await blog.LoadPostsAsync();

        Assert.Same(loaded, blog.LazyAsyncPsPosts);
        Assert.Equal(2, blog.LazyAsyncPsPosts.Count());
        Assert.Same(blog, blog.LazyAsyncPsPosts.First().LazyAsyncPsBlog);
        Assert.Same(blog, blog.LazyAsyncPsPosts.Skip(1).First().LazyAsyncPsBlog);
    }

    [ConditionalFact]
    public virtual void Query_with_loader_injected_into_property_via_constructor_for_reference()
    {
        using var context = CreateContext();
        var post = context.Set<LazyPcPost>().OrderBy(e => e.Id).First();

        Assert.False(post.LoaderSetterCalled);

        Assert.NotNull(post.LazyPcBlog);
        Assert.Contains(post, post.LazyPcBlog.LazyPcPosts);
    }

    [ConditionalFact]
    public virtual void Query_with_loader_injected_into_property_via_constructor_for_collections()
    {
        using var context = CreateContext();
        var blog = context.Set<LazyPcBlog>().Single();

        Assert.False(blog.LoaderSetterCalled);

        Assert.Equal(2, blog.LazyPcPosts.Count());
        Assert.Same(blog, blog.LazyPcPosts.First().LazyPcBlog);
        Assert.Same(blog, blog.LazyPcPosts.Skip(1).First().LazyPcBlog);
    }

    [ConditionalFact]
    public virtual void Query_with_loader_delegate_injected_into_property_via_constructor_for_reference()
    {
        using var context = CreateContext();
        var post = context.Set<LazyPcsPost>().OrderBy(e => e.Id).First();

        Assert.False(post.LoaderSetterCalled);

        Assert.NotNull(post.LazyPcsBlog);
        Assert.Contains(post, post.LazyPcsBlog.LazyPcsPosts);
    }

    [ConditionalFact]
    public virtual void Query_with_loader_delegate_injected_into_property_via_constructor_for_collections()
    {
        using var context = CreateContext();
        var blog = context.Set<LazyPcsBlog>().Single();

        Assert.False(blog.LoaderSetterCalled);

        Assert.Equal(2, blog.LazyPcsPosts.Count());
        Assert.Same(blog, blog.LazyPcsPosts.First().LazyPcsBlog);
        Assert.Same(blog, blog.LazyPcsPosts.Skip(1).First().LazyPcsBlog);
    }

    [ConditionalFact]
    public virtual async Task Add_immutable_record()
    {
        var title = "xyzzy";
        int blogId;
        using (var context = CreateContext())
        {
            var immutableBlog = new BlogAsImmutableRecord(title);

            await context.AddAsync(immutableBlog);
            await context.SaveChangesAsync();

            Assert.NotEqual(0, immutableBlog.BlogId);
            blogId = immutableBlog.BlogId;
        }

        using (var context = CreateContext())
        {
            Assert.Equal(title, context.Set<BlogAsImmutableRecord>().Single(e => e.BlogId == blogId).Title);
        }
    }

    [PrimaryKey(nameof(_blogId))]
    protected class Blog
    {
        private readonly int _blogId;

        private Blog(
            int blogId,
            string title,
            int? monthlyRevenue)
        {
            _blogId = blogId;
            Title = title;
            MonthlyRevenue = monthlyRevenue;
        }

        public Blog(
            string title,
            int? monthlyRevenue = null)
            : this(0, title, monthlyRevenue)
        {
        }

        public string Title { get; }
        public int? MonthlyRevenue { get; set; }

        public IEnumerable<Post> Posts { get; } = new List<Post>();

        public void AddPost(Post post)
            => ((List<Post>)Posts).Add(post);
    }

    protected class BlogQuery(
        string title,
        int? monthlyRevenue)
    {
        public string Title { get; } = title;
        public int? MonthlyRevenue { get; set; } = monthlyRevenue;
    }

    protected class Post
    {
        private readonly int _id;

        private Post(
            int id,
            string title,
            string content)
        {
            _id = id;
            Title = title;
            Content = content;
        }

        public Post(
            string title,
            string content,
            Blog blog = null)
            : this(0, title, content)
        {
            Blog = blog;
        }

        public string Title { get; }
        public string Content { get; set; }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public Blog Blog { get; private set; }
    }

    protected class HasContext<TContext>
        where TContext : DbContext
    {
        public HasContext()
        {
        }

        private HasContext(TContext context, int id)
        {
            Context = context;
            Id = id;
        }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public int Id { get; private set; }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public bool Filler { get; private set; }

        public TContext Context { get; }
    }

    protected class HasContextProperty<TContext>
        where TContext : DbContext
    {
        public int Id { get; set; }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public bool Filler { get; private set; }

        public TContext Context { get; private set; }
    }

    protected class HasContextPc<TContext>
        where TContext : DbContext
    {
        private TContext _context;
        private bool _setterCalled;

        public HasContextPc()
        {
        }

        private HasContextPc(TContext context, int id)
        {
            _context = context;
            Id = id;
        }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public int Id { get; private set; }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public bool Filler { get; private set; }

        private TContext Context
        {
            get => _context;
            set
            {
                _setterCalled = true;
                _context = value;
            }
        }

        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public bool SetterCalled
            => _setterCalled;

        public TContext GetContext()
            => Context;
    }

    protected class HasEntityType
    {
        private readonly IEntityType _entityType;

        public HasEntityType()
        {
        }

        private HasEntityType(IEntityType entityType)
        {
            _entityType = entityType;
        }

        public int Id { get; set; }

        public bool Filler { get; set; }

        public IEntityType GetEntityType()
            => _entityType;
    }

    protected class HasEntityTypeProperty
    {
        public int Id { get; set; }

        public bool Filler { get; set; }

        public IEntityType EntityType { get; set; }
    }

    protected class HasEntityTypePc
    {
        private IEntityType _entityType;
        private bool _setterCalled;

        public HasEntityTypePc()
        {
        }

        private HasEntityTypePc(IEntityType entityType)
        {
            _entityType = entityType;
        }

        public int Id { get; set; }

        public bool Filler { get; set; }

        private IEntityType EntityType
        {
            get => _entityType;
            set
            {
                _setterCalled = true;
                _entityType = value;
            }
        }

        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public bool SetterCalled
            => _setterCalled;

        public IEntityType GetEntityType()
            => EntityType;
    }

    protected class HasStateManager
    {
        private readonly IStateManager _stateManager;

        public HasStateManager()
        {
        }

        private HasStateManager(IStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public int Id { get; set; }

        public bool Filler { get; set; }

        public IStateManager GetStateManager()
            => _stateManager;
    }

    protected class HasStateManagerProperty
    {
        public int Id { get; set; }

        public bool Filler { get; set; }

        public IStateManager StateManager { get; set; }
    }

    protected class HasStateManagerPc
    {
        private IStateManager _stateManager;
        private bool _setterCalled;
        // ReSharper disable once ConvertToAutoProperty

        public HasStateManagerPc()
        {
        }

        private HasStateManagerPc(IStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public int Id { get; set; }

        public bool Filler { get; set; }

        private IStateManager StateManager
        {
            get => _stateManager;
            set
            {
                _setterCalled = true;
                _stateManager = value;
            }
        }

        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public bool SetterCalled
            => _setterCalled;

        public IStateManager GetStateManager()
            => StateManager;
    }

    protected class LazyBlog
    {
        private readonly ILazyLoader _loader;
        private ICollection<LazyPost> _lazyPosts = new List<LazyPost>();

        public LazyBlog()
        {
        }

        private LazyBlog(ILazyLoader loader)
        {
            _loader = loader;
        }

        public int Id { get; set; }

        public bool Filler { get; set; }

        public void AddPost(LazyPost post)
            => _lazyPosts.Add(post);

        public IEnumerable<LazyPost> LazyPosts
            => _loader.Load(this, ref _lazyPosts);
    }

    protected class LazyPost
    {
        private readonly ILazyLoader _loader;
        private LazyBlog _lazyBlog;

        public LazyPost()
        {
        }

        private LazyPost(ILazyLoader loader)
        {
            _loader = loader;
        }

        public int Id { get; set; }

        public bool Filler { get; set; }

        public LazyBlog LazyBlog
        {
            get => _loader.Load(this, ref _lazyBlog);
            set => _lazyBlog = value;
        }
    }

    protected class LazyPropertyBlog
    {
        private ICollection<LazyPropertyPost> _lazyPropertyPosts = new List<LazyPropertyPost>();

        private ILazyLoader Loader { get; set; }

        public int Id { get; set; }

        public bool Filler { get; set; }

        public void AddPost(LazyPropertyPost post)
            => _lazyPropertyPosts.Add(post);

        public IEnumerable<LazyPropertyPost> LazyPropertyPosts
            => Loader.Load(this, ref _lazyPropertyPosts);
    }

    protected class LazyPropertyPost
    {
        private LazyPropertyBlog _lazyPropertyBlog;

        private ILazyLoader Loader { get; set; }

        public int Id { get; set; }
        public bool Filler { get; set; }
        public int LazyPropertyBlogId { get; set; }

        public LazyPropertyBlog LazyPropertyBlog
        {
            get => Loader.Load(this, ref _lazyPropertyBlog);
            set => _lazyPropertyBlog = value;
        }

        public ILazyLoader GetLoader()
            => Loader;
    }

    protected class LazyFieldBlog
    {
        private ICollection<LazyFieldPost> _lazyFieldPosts = new List<LazyFieldPost>();

#pragma warning disable 649
#pragma warning disable IDE0044 // Add readonly modifier
        private ILazyLoader _loader;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore 649

        public int Id { get; set; }

        public bool Filler { get; set; }

        public void AddPost(LazyFieldPost post)
            => _lazyFieldPosts.Add(post);

        public IEnumerable<LazyFieldPost> LazyFieldPosts
            => _loader.Load(this, ref _lazyFieldPosts);
    }

    protected class LazyFieldPost
    {
        private LazyFieldBlog _lazyFieldBlog;

#pragma warning disable 649
#pragma warning disable IDE0044 // Add readonly modifier
        private ILazyLoader _loader;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore 649

        public int Id { get; set; }
        public bool Filler { get; set; }
        public int LazyFieldBlogId { get; set; }

        public LazyFieldBlog LazyFieldBlog
        {
            get => _loader.Load(this, ref _lazyFieldBlog);
            set => _lazyFieldBlog = value;
        }

        public ILazyLoader GetLoader()
            => _loader;
    }

    protected class LazyPsBlog
    {
        private ICollection<LazyPsPost> _lazyPsPosts = new List<LazyPsPost>();

        private Action<object, string> LazyLoader { get; set; }

        public int Id { get; set; }

        public bool Filler { get; set; }

        public void AddPost(LazyPsPost post)
            => _lazyPsPosts.Add(post);

        public IEnumerable<LazyPsPost> LazyPsPosts
            => LazyLoader.Load(this, ref _lazyPsPosts);
    }

    protected class LazyPsPost
    {
        private LazyPsBlog _lazyPsBlog;

        private Action<object, string> LazyLoader { get; set; }

        public int Id { get; set; }

        public bool Filler { get; set; }

        public LazyPsBlog LazyPsBlog
        {
            get => LazyLoader.Load(this, ref _lazyPsBlog);
            set => _lazyPsBlog = value;
        }
    }

    protected class LazyAsyncPsBlog
    {
        private readonly ICollection<LazyAsyncPsPost> _lazyAsyncPsPosts = new List<LazyAsyncPsPost>();

        private Func<object, CancellationToken, string, Task> LazyLoader { get; set; }

        public int Id { get; set; }

        public bool Filler { get; set; }

        public void AddPost(LazyAsyncPsPost post)
            => _lazyAsyncPsPosts.Add(post);

        public async Task<IEnumerable<LazyAsyncPsPost>> LoadPostsAsync(CancellationToken cancellationToken = default)
        {
            await LazyLoader(this, cancellationToken, nameof(LazyAsyncPsPosts));

            return LazyAsyncPsPosts;
        }

        public IEnumerable<LazyAsyncPsPost> LazyAsyncPsPosts
            => _lazyAsyncPsPosts;
    }

    protected class LazyAsyncPsPost
    {
        private Func<object, CancellationToken, string, Task> LazyLoader { get; set; }

        public int Id { get; set; }
        public bool Filler { get; set; }

        public async Task<LazyAsyncPsBlog> LoadBlogAsync(CancellationToken cancellationToken = default)
        {
            await LazyLoader(this, cancellationToken, nameof(LazyAsyncPsBlog));

            return LazyAsyncPsBlog;
        }

        public LazyAsyncPsBlog LazyAsyncPsBlog { get; set; }
    }

    protected class LazyPcBlog
    {
        private ICollection<LazyPcPost> _lazyPcPosts = new List<LazyPcPost>();
        private ILazyLoader _loader;

        public LazyPcBlog()
        {
        }

        private LazyPcBlog(ILazyLoader loader)
        {
            _loader = loader;
        }

        private ILazyLoader Loader
        {
            get => _loader;
            set
            {
                LoaderSetterCalled = true;

                _loader = value;
            }
        }

        [NotMapped]
        public bool LoaderSetterCalled { get; set; }

        public int Id { get; set; }

        public bool Filler { get; set; }

        public void AddPost(LazyPcPost post)
            => _lazyPcPosts.Add(post);

        public IEnumerable<LazyPcPost> LazyPcPosts
            => Loader.Load(this, ref _lazyPcPosts);
    }

    protected class LazyPcPost
    {
        private LazyPcBlog _lazyPcBlog;
        private ILazyLoader _loader;

        public LazyPcPost()
        {
        }

        private LazyPcPost(ILazyLoader loader)
        {
            _loader = loader;
        }

        private ILazyLoader Loader
        {
            get => _loader;
            set
            {
                LoaderSetterCalled = true;

                _loader = value;
            }
        }

        [NotMapped]
        public bool LoaderSetterCalled { get; set; }

        public int Id { get; set; }

        public bool Filler { get; set; }

        public LazyPcBlog LazyPcBlog
        {
            get => Loader.Load(this, ref _lazyPcBlog);
            set => _lazyPcBlog = value;
        }
    }

    protected class LazyPcsBlog
    {
        private ICollection<LazyPcsPost> _lazyPcsPosts = new List<LazyPcsPost>();
        private Action<object, string> _loader;

        public LazyPcsBlog()
        {
        }

        private LazyPcsBlog(Action<object, string> lazyLoader)
        {
            _loader = lazyLoader;
        }

        private Action<object, string> LazyLoader
        {
            get => _loader;
            set
            {
                LoaderSetterCalled = true;

                _loader = value;
            }
        }

        [NotMapped]
        public bool LoaderSetterCalled { get; set; }

        public int Id { get; set; }

        public bool Filler { get; set; }

        public void AddPost(LazyPcsPost post)
            => _lazyPcsPosts.Add(post);

        public IEnumerable<LazyPcsPost> LazyPcsPosts
            => LazyLoader.Load(this, ref _lazyPcsPosts);
    }

    protected class LazyPcsPost
    {
        private LazyPcsBlog _lazyPcsBlog;
        private Action<object, string> _loader;

        public LazyPcsPost()
        {
        }

        private LazyPcsPost(Action<object, string> lazyLoader)
        {
            _loader = lazyLoader;
        }

        private Action<object, string> LazyLoader
        {
            get => _loader;
            set
            {
                LoaderSetterCalled = true;

                _loader = value;
            }
        }

        [NotMapped]
        public bool LoaderSetterCalled { get; set; }

        public int Id { get; set; }
        public bool Filler { get; set; }
        public int LazyPcsBlogId { get; set; }

        public LazyPcsBlog LazyPcsBlog
        {
            get => LazyLoader.Load(this, ref _lazyPcsBlog);
            set => _lazyPcsBlog = value;
        }

        public Action<object, string> GetLoader()
            => _loader;
    }

    protected class LazyPocoBlog
    {
        private readonly Action<object, string> _loader;
        private ICollection<LazyPocoPost> _lazyPocoPosts = new List<LazyPocoPost>();

        public LazyPocoBlog()
        {
        }

        private LazyPocoBlog(Action<object, string> lazyLoader)
        {
            _loader = lazyLoader;
        }

        public int Id { get; set; }

        public bool Filler { get; set; }

        public void AddPost(LazyPocoPost post)
            => _lazyPocoPosts.Add(post);

        public IEnumerable<LazyPocoPost> LazyPocoPosts
            => _loader.Load(this, ref _lazyPocoPosts);
    }

    protected class LazyPocoPost
    {
        private readonly Action<object, string> _loader;
        private LazyPocoBlog _lazyPocoBlog;

        public LazyPocoPost()
        {
        }

        private LazyPocoPost(Action<object, string> lazyLoader)
        {
            _loader = lazyLoader;
        }

        public int Id { get; set; }

        public bool Filler { get; set; }

        public LazyPocoBlog LazyPocoBlog
        {
            get => _loader.Load(this, ref _lazyPocoBlog);
            set => _lazyPocoBlog = value;
        }
    }

    protected class LazyAsyncPocoBlog
    {
        private readonly Func<object, CancellationToken, string, Task> _loader;
        private readonly ICollection<LazyAsyncPocoPost> _lazyAsyncPocoPosts = new List<LazyAsyncPocoPost>();

        public LazyAsyncPocoBlog()
        {
        }

        private LazyAsyncPocoBlog(Func<object, CancellationToken, string, Task> lazyLoader)
        {
            _loader = lazyLoader;
        }

        public int Id { get; set; }

        public bool Filler { get; set; }

        public void AddPost(LazyAsyncPocoPost post)
            => _lazyAsyncPocoPosts.Add(post);

        public async Task<IEnumerable<LazyAsyncPocoPost>> LoadPostsAsync(CancellationToken cancellationToken = default)
        {
            await _loader(this, cancellationToken, nameof(LazyAsyncPocoPosts));

            return LazyAsyncPocoPosts;
        }

        public IEnumerable<LazyAsyncPocoPost> LazyAsyncPocoPosts
            => _lazyAsyncPocoPosts;
    }

    protected class LazyAsyncPocoPost
    {
        private readonly Func<object, CancellationToken, string, Task> _loader;

        public LazyAsyncPocoPost()
        {
        }

        private LazyAsyncPocoPost(Func<object, CancellationToken, string, Task> lazyLoader)
        {
            _loader = lazyLoader;
        }

        public int Id { get; set; }

        public bool Filler { get; set; }

        public async Task<LazyAsyncPocoBlog> LoadBlogAsync(CancellationToken cancellationToken = default)
        {
            await _loader(this, cancellationToken, nameof(LazyAsyncPocoBlog));

            return LazyAsyncPocoBlog;
        }

        public LazyAsyncPocoBlog LazyAsyncPocoBlog { get; set; }
    }

    protected class LazyAsyncBlog
    {
        private readonly ILazyLoader _loader;
        private readonly ICollection<LazyAsyncPost> _lazyAsyncPosts = new List<LazyAsyncPost>();

        public LazyAsyncBlog()
        {
        }

        private LazyAsyncBlog(ILazyLoader loader)
        {
            _loader = loader;
        }

        public int Id { get; set; }

        public bool Filler { get; set; }

        public void AddPost(LazyAsyncPost post)
            => _lazyAsyncPosts.Add(post);

        public async Task<IEnumerable<LazyAsyncPost>> LoadPostsAsync(CancellationToken cancellationToken = default)
        {
            await _loader.LoadAsync(this, cancellationToken, nameof(LazyAsyncPosts));

            return LazyAsyncPosts;
        }

        public IEnumerable<LazyAsyncPost> LazyAsyncPosts
            => _lazyAsyncPosts;
    }

    protected class LazyAsyncPost
    {
        private readonly ILazyLoader _loader;

        public LazyAsyncPost()
        {
        }

        private LazyAsyncPost(ILazyLoader loader)
        {
            _loader = loader;
        }

        public int Id { get; set; }

        public bool Filler { get; set; }

        public async Task<LazyAsyncBlog> LoadBlogAsync(CancellationToken cancellationToken = default)
        {
            await _loader.LoadAsync(this, cancellationToken, nameof(LazyAsyncBlog));

            return LazyAsyncBlog;
        }

        public LazyAsyncBlog LazyAsyncBlog { get; set; }
    }

    protected record BlogAsImmutableRecord
    {
        public BlogAsImmutableRecord(
            string title,
            int? monthlyRevenue = null)
            : this(0, title, monthlyRevenue)
        {
        }

        private BlogAsImmutableRecord(
            int blogId,
            string title,
            int? monthlyRevenue)
        {
            BlogId = blogId;
            Title = title;
            MonthlyRevenue = monthlyRevenue;
        }

        [Key]
        public int BlogId { get; init; }

        public string Title { get; init; }
        public int? MonthlyRevenue { get; init; }
    }

    public class OtherContext : DbContext;

    public class WithConstructorsContext(DbContextOptions options) : PoolableDbContext(options);

    public abstract class WithConstructorsFixtureBase : SharedStoreFixtureBase<WithConstructorsContext>
    {
        protected override string StoreName
            => "WithConstructors";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<Blog>(
                b =>
                {
                    b.Property(e => e.Title);
                });

            modelBuilder.Entity<BlogQuery>(
                b =>
                {
                    b.HasNoKey();
                    b.Property(e => e.Title);
                });

            modelBuilder.Entity<Post>(
                b =>
                {
                    b.HasKey("_id");
                    b.Property(e => e.Title);
                });

            modelBuilder.Entity<HasContext<DbContext>>();
            modelBuilder.Entity<HasContext<WithConstructorsContext>>();
            modelBuilder.Entity<HasContext<OtherContext>>();

            modelBuilder.Entity<HasContextProperty<DbContext>>();
            modelBuilder.Entity<HasContextProperty<WithConstructorsContext>>();
            modelBuilder.Entity<HasContextProperty<OtherContext>>();

            modelBuilder.Entity<HasContextPc<DbContext>>();
            modelBuilder.Entity<HasContextPc<WithConstructorsContext>>();
            modelBuilder.Entity<HasContextPc<OtherContext>>();

            modelBuilder.Entity<HasEntityType>();
            modelBuilder.Entity<HasEntityTypeProperty>();
            modelBuilder.Entity<HasEntityTypePc>();

            modelBuilder.Entity<HasStateManager>();
            modelBuilder.Entity<HasStateManagerProperty>();
            modelBuilder.Entity<HasStateManagerPc>();

            modelBuilder.Entity<LazyBlog>();
            modelBuilder.Entity<LazyPocoBlog>();

            modelBuilder.Entity<LazyAsyncBlog>();
            modelBuilder.Entity<LazyAsyncPocoBlog>();

            modelBuilder.Entity<LazyPropertyBlog>();
            modelBuilder.Entity<LazyPcBlog>();
            modelBuilder.Entity<LazyPsBlog>();
            modelBuilder.Entity<LazyAsyncPsBlog>();
            modelBuilder.Entity<LazyPcsBlog>();
            modelBuilder.Entity<BlogAsImmutableRecord>();
            modelBuilder.Entity<LazyFieldBlog>();
            modelBuilder.Entity<LazyFieldPost>();
        }

        protected override Task SeedAsync(WithConstructorsContext context)
        {
            var blog = new Blog("Puppies");

            var post1 = new Post(
                "Golden Toasters Rock",
                "Smaller than the Black Library Dog, and more chewy.",
                blog);

            var post2 = new Post(
                "Baxter is not a dog",
                "He is a cat. Who eats dog food. And wags his tail.",
                blog);

            context.AddRange(blog, post1, post2);

            context.AddRange(
                new HasContext<DbContext>(),
                new HasContext<WithConstructorsContext>(),
                new HasContext<OtherContext>());

            context.AddRange(
                new HasContextProperty<DbContext>(),
                new HasContextProperty<WithConstructorsContext>(),
                new HasContextProperty<OtherContext>());

            context.AddRange(
                new HasContextPc<DbContext>(),
                new HasContextPc<WithConstructorsContext>(),
                new HasContextPc<OtherContext>());

            context.AddRange(
                new HasEntityType(),
                new HasEntityTypeProperty(),
                new HasEntityTypePc());

            context.AddRange(
                new HasStateManager(),
                new HasStateManagerProperty(),
                new HasStateManagerPc());

            var lazyBlog = new LazyBlog();
            lazyBlog.AddPost(new LazyPost());
            lazyBlog.AddPost(new LazyPost());

            context.Add(lazyBlog);

            var lazyAsyncBlog = new LazyAsyncBlog();
            lazyAsyncBlog.AddPost(new LazyAsyncPost());
            lazyAsyncBlog.AddPost(new LazyAsyncPost());

            context.Add(lazyAsyncBlog);

            var lazyPocoBlog = new LazyPocoBlog();
            lazyPocoBlog.AddPost(new LazyPocoPost());
            lazyPocoBlog.AddPost(new LazyPocoPost());

            context.Add(lazyPocoBlog);

            var lazyAsyncPocoBlog = new LazyAsyncPocoBlog();
            lazyAsyncPocoBlog.AddPost(new LazyAsyncPocoPost());
            lazyAsyncPocoBlog.AddPost(new LazyAsyncPocoPost());

            context.Add(lazyAsyncPocoBlog);

            var lazyPropertyBlog = new LazyPropertyBlog();
            lazyPropertyBlog.AddPost(new LazyPropertyPost());
            lazyPropertyBlog.AddPost(new LazyPropertyPost());

            context.Add(lazyPropertyBlog);

            var lazyFieldBlog = new LazyFieldBlog();
            lazyFieldBlog.AddPost(new LazyFieldPost());
            lazyFieldBlog.AddPost(new LazyFieldPost());

            context.Add(lazyFieldBlog);

            var lazyPsBlog = new LazyPsBlog();
            lazyPsBlog.AddPost(new LazyPsPost());
            lazyPsBlog.AddPost(new LazyPsPost());

            context.Add(lazyPsBlog);

            var lazyAsyncPsBlog = new LazyAsyncPsBlog();
            lazyAsyncPsBlog.AddPost(new LazyAsyncPsPost());
            lazyAsyncPsBlog.AddPost(new LazyAsyncPsPost());

            context.Add(lazyAsyncPsBlog);

            var lazyPcBlog = new LazyPcBlog();
            lazyPcBlog.AddPost(new LazyPcPost());
            lazyPcBlog.AddPost(new LazyPcPost());

            context.Add(lazyPcBlog);

            var lazyPcsBlog = new LazyPcsBlog();
            lazyPcsBlog.AddPost(new LazyPcsPost());
            lazyPcsBlog.AddPost(new LazyPcsPost());

            context.Add(lazyPcsBlog);

            return context.SaveChangesAsync();
        }
    }
}
