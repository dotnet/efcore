// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class WithConstructorsTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : WithConstructorsTestBase<TFixture>.WithConstructorsFixtureBase, new()
    {
        protected WithConstructorsTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        protected DbContext CreateContext() => Fixture.CreateContext();

        protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        {
        }

        [Fact]
        public virtual void Query_and_update_using_constructors_with_property_parameters()
        {
            TestHelpers.ExecuteWithStrategyInTransaction(
                CreateContext, UseTransaction,
                context =>
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

                    context.SaveChanges();
                },
                context =>
                {
                    var blogs = context.Set<Blog>().Include(e => e.Posts).OrderBy(e => e.Title).ToList();

                    Assert.Equal(2, blogs.Count);

                    Assert.Equal("Cats", blogs[0].Title);
                    Assert.Equal("Puppies", blogs[1].Title);

                    var posts = blogs[0].Posts.OrderBy(e => e.Title).ToList();

                    Assert.Equal(1, posts.Count);

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
        }

        [Fact]
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

        [Fact]
        public virtual void Query_with_loader_injected_for_reference()
        {
            using (var context = CreateContext())
            {
                var post = context.Set<LazyPost>().OrderBy(e => e.Id).First();

                Assert.NotNull(post.LazyBlog);
                Assert.Contains(post, post.LazyBlog.LazyPosts);
            }
        }

        [Fact]
        public virtual void Query_with_loader_injected_for_collections()
        {
            using (var context = CreateContext())
            {
                var blog = context.Set<LazyBlog>().Single();

                Assert.Equal(2, blog.LazyPosts.Count());
                Assert.Same(blog, blog.LazyPosts.First().LazyBlog);
                Assert.Same(blog, blog.LazyPosts.Skip(1).First().LazyBlog);
            }
        }

        [Fact]
        public virtual void Query_with_POCO_loader_injected_for_reference()
        {
            using (var context = CreateContext())
            {
                var post = context.Set<LazyPocoPost>().OrderBy(e => e.Id).First();

                Assert.NotNull(post.LazyPocoBlog);
                Assert.Contains(post, post.LazyPocoBlog.LazyPocoPosts);
            }
        }

        [Fact]
        public virtual void Query_with_POCO_loader_injected_for_collections()
        {
            using (var context = CreateContext())
            {
                var blog = context.Set<LazyPocoBlog>().Single();

                Assert.Equal(2, blog.LazyPocoPosts.Count());
                Assert.Same(blog, blog.LazyPocoPosts.First().LazyPocoBlog);
                Assert.Same(blog, blog.LazyPocoPosts.Skip(1).First().LazyPocoBlog);
            }
        }

        protected class Blog
        {
            private int _blogId;

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

        protected class Post
        {
            private int _id;

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
            public TContext Context { get; }
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

            public void AddPost(LazyPost post) => _lazyPosts.Add(post);

            public IEnumerable<LazyPost> LazyPosts => _loader.Load(this, ref _lazyPosts);
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

            public LazyBlog LazyBlog
            {
                get => _loader.Load(this, ref _lazyBlog);
                set => _lazyBlog = value;
            }
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

            public void AddPost(LazyPocoPost post) => _lazyPocoPosts.Add(post);

            public IEnumerable<LazyPocoPost> LazyPocoPosts => _loader.Load(this, ref _lazyPocoPosts);
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

            public LazyPocoBlog LazyPocoBlog
            {
                get => _loader.Load(this, ref _lazyPocoBlog);
                set => _lazyPocoBlog = value;
            }
        }

        public class OtherContext : DbContext
        {
        }

        public class WithConstructorsContext : DbContext
        {
            public WithConstructorsContext(DbContextOptions options)
                : base(options)
            {
            }
        }

        public abstract class WithConstructorsFixtureBase : SharedStoreFixtureBase<WithConstructorsContext>
        {
            protected override string StoreName { get; } = "WithConstructors";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<Blog>(
                    b =>
                    {
                        b.HasKey("_blogId");
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

                modelBuilder.Entity<LazyBlog>();
                modelBuilder.Entity<LazyPocoBlog>();
            }

            protected override void Seed(WithConstructorsContext context)
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

                var lazyBlog = new LazyBlog();
                lazyBlog.AddPost(new LazyPost());
                lazyBlog.AddPost(new LazyPost());

                context.Add(lazyBlog);

                var lazyPocoBlog = new LazyPocoBlog();
                lazyPocoBlog.AddPost(new LazyPocoPost());
                lazyPocoBlog.AddPost(new LazyPocoPost());

                context.Add(lazyPocoBlog);

                context.SaveChanges();
            }
        }
    }
}
