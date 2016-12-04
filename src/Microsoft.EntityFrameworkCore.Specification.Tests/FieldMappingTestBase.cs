// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class FieldMappingTestBase<TTestStore, TFixture> : IClassFixture<TFixture>, IDisposable
        where TTestStore : TestStore
        where TFixture : FieldMappingTestBase<TTestStore, TFixture>.FieldMappingFixtureBase, new()
    {
        protected FieldMappingTestBase(TFixture fixture)
        {
            Fixture = fixture;
            TestStore = Fixture.CreateTestStore();
        }

        [Fact]
        public virtual void Include_collection_auto_props()
        {
            using (var context = CreateContext())
            {
                AssertGraph(context.Set<BlogAuto>().Include(e => e.Posts).ToList());
            }
        }

        [Fact]
        public virtual void Include_reference_auto_props()
        {
            using (var context = CreateContext())
            {
                AssertGraph(context.Set<PostAuto>().Include(e => e.Blog).ToList());
            }
        }

        [Fact]
        public virtual void Load_collection_auto_props()
            => Load_collection<BlogAuto>("Posts");

        [Fact]
        public virtual void Load_reference_auto_props()
            => Load_reference<PostAuto>("Blog");

        [Fact]
        public virtual void Query_with_conditional_constant_auto_props()
            => Query_with_conditional_constant<PostAuto>("BlogId");

        [Fact]
        public virtual void Query_with_conditional_param_auto_props()
            => Query_with_conditional_param<PostAuto>("Title");

        [Fact]
        public virtual void Projection_auto_props()
            => Projection<PostAuto>("Id", "Title");

        [Fact]
        public virtual void Update_auto_props()
            => Update<BlogAuto>("Posts");

        [Fact]
        public virtual void Include_collection_full_props()
        {
            using (var context = CreateContext())
            {
                AssertGraph(context.Set<BlogFull>().Include(e => e.Posts).ToList());
            }
        }

        [Fact]
        public virtual void Include_reference_full_props()
        {
            using (var context = CreateContext())
            {
                AssertGraph(context.Set<PostFull>().Include(e => e.Blog).ToList());
            }
        }

        [Fact]
        public virtual void Load_collection_full_props()
            => Load_collection<BlogFull>("Posts");

        [Fact]
        public virtual void Load_reference_full_props()
            => Load_reference<PostFull>("Blog");

        [Fact]
        public virtual void Query_with_conditional_constant_full_props()
            => Query_with_conditional_constant<PostFull>("BlogId");

        [Fact]
        public virtual void Query_with_conditional_param_full_props()
            => Query_with_conditional_param<PostFull>("Title");

        [Fact]
        public virtual void Projection_full_props()
            => Projection<PostFull>("Id", "Title");

        [Fact]
        public virtual void Update_full_props()
            => Update<BlogFull>("Posts");

        [Fact]
        public virtual void Include_collection_full_props_with_named_fields()
        {
            using (var context = CreateContext())
            {
                AssertGraph(context.Set<BlogFullExplicit>().Include(e => e.Posts).ToList());
            }
        }

        [Fact]
        public virtual void Include_reference_full_props_with_named_fields()
        {
            using (var context = CreateContext())
            {
                AssertGraph(context.Set<PostFullExplicit>().Include(e => e.Blog).ToList());
            }
        }

        [Fact]
        public virtual void Load_collection_full_props_with_named_fields()
            => Load_collection<BlogFullExplicit>("Posts");

        [Fact]
        public virtual void Load_reference_full_props_with_named_fields()
            => Load_reference<PostFullExplicit>("Blog");

        [Fact]
        public virtual void Query_with_conditional_constant_full_props_with_named_fields()
            => Query_with_conditional_constant<PostFullExplicit>("BlogId");

        [Fact]
        public virtual void Query_with_conditional_param_full_props_with_named_fields()
            => Query_with_conditional_param<PostFullExplicit>("Title");

        [Fact]
        public virtual void Projection_full_props_with_named_fields()
            => Projection<PostFullExplicit>("Id", "Title");

        [Fact]
        public virtual void Update_full_props_with_named_fields()
            => Update<BlogFullExplicit>("Posts");

        [Fact]
        public virtual void Include_collection_read_only_props()
        {
            using (var context = CreateContext())
            {
                AssertGraph(context.Set<BlogReadOnly>().Include(e => e.Posts).ToList());
            }
        }

        [Fact]
        public virtual void Include_reference_read_only_props()
        {
            using (var context = CreateContext())
            {
                AssertGraph(context.Set<PostReadOnly>().Include(e => e.Blog).ToList());
            }
        }

        [Fact]
        public virtual void Load_collection_read_only_props()
            => Load_collection<BlogReadOnly>("Posts");

        [Fact]
        public virtual void Load_reference_read_only_props()
            => Load_reference<PostReadOnly>("Blog");

        [Fact]
        public virtual void Query_with_conditional_constant_read_only_props()
            => Query_with_conditional_constant<PostReadOnly>("BlogId");

        [Fact]
        public virtual void Query_with_conditional_param_read_only_props()
            => Query_with_conditional_param<PostReadOnly>("Title");

        [Fact]
        public virtual void Projection_read_only_props()
            => Projection<PostReadOnly>("Id", "Title");

        [Fact]
        public virtual void Update_read_only_props()
            => Update<BlogReadOnly>("Posts");

        [Fact]
        public virtual void Include_collection_read_only_props_with_named_fields()
        {
            using (var context = CreateContext())
            {
                AssertGraph(context.Set<BlogReadOnlyExplicit>().Include(e => e.Posts).ToList());
            }
        }

        [Fact]
        public virtual void Include_reference_read_only_props_with_named_fields()
        {
            using (var context = CreateContext())
            {
                AssertGraph(context.Set<PostReadOnlyExplicit>().Include(e => e.Blog).ToList());
            }
        }

        [Fact]
        public virtual void Load_collection_read_only_props_with_named_fields()
            => Load_collection<BlogReadOnlyExplicit>("Posts");

        [Fact]
        public virtual void Load_reference_read_only_props_with_named_fields()
            => Load_reference<PostReadOnlyExplicit>("Blog");

        [Fact]
        public virtual void Query_with_conditional_constant_read_only_props_with_named_fields()
            => Query_with_conditional_constant<PostReadOnlyExplicit>("BlogId");

        [Fact]
        public virtual void Query_with_conditional_param_read_only_props_with_named_fields()
            => Query_with_conditional_param<PostReadOnlyExplicit>("Title");

        [Fact]
        public virtual void Projection_read_only_props_with_named_fields()
            => Projection<PostReadOnlyExplicit>("Id", "Title");

        [Fact]
        public virtual void Update_read_only_props_with_named_fields()
            => Update<BlogReadOnlyExplicit>("Posts");

        [Fact]
        public virtual void Include_collection_write_only_props()
        {
            using (var context = CreateContext())
            {
                AssertGraph(context.Set<BlogWriteOnly>().Include("Posts").ToList());
            }
        }

        [Fact]
        public virtual void Include_reference_write_only_props()
        {
            using (var context = CreateContext())
            {
                AssertGraph(context.Set<PostWriteOnly>().Include("Blog").ToList());
            }
        }

        [Fact]
        public virtual void Load_collection_write_only_props()
            => Load_collection<BlogWriteOnly>("Posts");

        [Fact]
        public virtual void Load_reference_write_only_props()
            => Load_reference<PostWriteOnly>("Blog");

        [Fact]
        public virtual void Query_with_conditional_constant_write_only_props()
            => Query_with_conditional_constant<PostWriteOnly>("BlogId");

        [Fact]
        public virtual void Query_with_conditional_param_write_only_props()
            => Query_with_conditional_param<PostWriteOnly>("Title");

        [Fact]
        public virtual void Projection_write_only_props()
            => Projection<PostWriteOnly>("Id", "Title");

        [Fact]
        public virtual void Update_write_only_props()
            => Update<BlogWriteOnly>("Posts");

        [Fact]
        public virtual void Include_collection_write_only_props_with_named_fields()
        {
            using (var context = CreateContext())
            {
                AssertGraph(context.Set<BlogWriteOnlyExplicit>().Include("Posts").ToList());
            }
        }

        [Fact]
        public virtual void Include_reference_write_only_props_with_named_fields()
        {
            using (var context = CreateContext())
            {
                AssertGraph(context.Set<PostWriteOnlyExplicit>().Include("Blog").ToList());
            }
        }

        [Fact]
        public virtual void Load_collection_write_only_props_with_named_fields()
            => Load_collection<BlogWriteOnlyExplicit>("Posts");

        [Fact]
        public virtual void Load_reference_write_only_props_with_named_fields()
            => Load_reference<PostWriteOnlyExplicit>("Blog");

        [Fact]
        public virtual void Query_with_conditional_constant_write_only_props_with_named_fields()
            => Query_with_conditional_constant<PostWriteOnlyExplicit>("BlogId");

        [Fact]
        public virtual void Query_with_conditional_param_write_only_props_with_named_fields()
            => Query_with_conditional_param<PostWriteOnlyExplicit>("Title");

        [Fact]
        public virtual void Projection_write_only_props_with_named_fields()
            => Projection<PostWriteOnlyExplicit>("Id", "Title");

        [Fact]
        public virtual void Update_write_only_props_with_named_fields()
            => Update<BlogWriteOnlyExplicit>("Posts");

        [Fact]
        public virtual void Include_collection_fields_only()
        {
            using (var context = CreateContext())
            {
                AssertGraph(context.Set<BlogFields>().Include(e => e.Posts).ToList());
            }
        }

        [Fact]
        public virtual void Include_reference_fields_only()
        {
            using (var context = CreateContext())
            {
                AssertGraph(context.Set<PostFields>().Include(e => e.Blog).ToList());
            }
        }

        [Fact]
        public virtual void Load_collection_fields_only()
            => Load_collection<BlogFields>("Posts");

        [Fact]
        public virtual void Load_reference_fields_only()
            => Load_reference<PostFields>("Blog");

        [Fact]
        public virtual void Query_with_conditional_constant_fields_only()
            => Query_with_conditional_constant<PostFields>("_blogId");

        [Fact]
        public virtual void Query_with_conditional_param_fields_only()
            => Query_with_conditional_param<PostFields>("_title");

        [Fact]
        public virtual void Projection_fields_only()
            => Projection<PostFields>("_id", "_title");

        [Fact]
        public virtual void Update_fields_only()
            => Update<BlogFields>("Posts");

        protected virtual void Load_collection<TBlog>(string navigation)
            where TBlog : class, IBlogAccesor, new()
        {
            using (var context = CreateContext())
            {
                var blogs = context.Set<TBlog>().ToList();

                foreach (var blog in blogs)
                {
                    context.Entry(blog).Collection(navigation).Load();
                }

                AssertGraph(blogs);
            }
        }

        protected virtual void Load_reference<TPost>(string navigation)
            where TPost : class, IPostAccesor, new()
        {
            using (var context = CreateContext())
            {
                var posts = context.Set<TPost>().ToList();

                foreach (var post in posts)
                {
                    context.Entry(post).Reference(navigation).Load();
                }

                AssertGraph(posts);
            }
        }

        protected virtual void Query_with_conditional_constant<TPost>(string property)
            where TPost : class, IPostAccesor, new()
        {
            using (var context = CreateContext())
            {
                var posts = context.Set<TPost>().Where(p => EF.Property<int>(p, property) == 10).ToList();

                Assert.Equal(2, posts.Count);

                var post1 = posts.Single(e => e.AccessId == 10);
                Assert.Equal("Post10", post1.AccessTitle);
                Assert.Equal(10, post1.AccessBlogId);

                var post2 = posts.Single(e => e.AccessId == 11);
                Assert.Equal("Post11", post2.AccessTitle);
                Assert.Equal(10, post2.AccessBlogId);
            }
        }

        protected virtual void Query_with_conditional_param<TPost>(string property)
            where TPost : class, IPostAccesor, new()
        {
            var postTitle = "Post11";
            using (var context = CreateContext())
            {
                var posts = context.Set<TPost>().Where(p => EF.Property<string>(p, property) == postTitle).ToList();

                Assert.Equal(1, posts.Count);

                var post = posts.Single(e => e.AccessId == 11);
                Assert.Equal("Post11", post.AccessTitle);
                Assert.Equal(10, post.AccessBlogId);
            }
        }

        protected virtual void Projection<TPost>(string property1, string property2)
            where TPost : class, IPostAccesor, new()
        {
            using (var context = CreateContext())
            {
                var posts = context.Set<TPost>().Select(
                    p => new
                    {
                        Prop1 = EF.Property<int>(p, property1),
                        Prop2 = EF.Property<string>(p, property2)
                    }).ToList();

                Assert.Equal(4, posts.Count);

                Assert.Equal("Post10", posts.Single(e => e.Prop1 == 10).Prop2);
                Assert.Equal("Post11", posts.Single(e => e.Prop1 == 11).Prop2);
                Assert.Equal("Post20", posts.Single(e => e.Prop1 == 20).Prop2);
                Assert.Equal("Post21", posts.Single(e => e.Prop1 == 21).Prop2);
            }
        }

        protected virtual void Update<TBlog>(string navigation)
            where TBlog : class, IBlogAccesor, new()
        {
            DbContextHelpers.ExecuteWithStrategyInTransaction(CreateContext, UseTransaction,
                context =>
                    {
                        var blogs = context.Set<TBlog>().ToList();

                        foreach (var blog in blogs)
                        {
                            context.Entry(blog).Collection(navigation).Load();

                            blog.AccessTitle += "Updated";

                            foreach (var post in blog.AccessPosts)
                            {
                                post.AccessTitle += "Updated";
                            }
                        }

                        AssertGraph(blogs, "Updated");

                        context.SaveChanges();

                        AssertGraph(blogs, "Updated");
                    },
                context =>
                    {
                        var blogs = context.Set<TBlog>().ToList();

                        foreach (var blog in blogs)
                        {
                            context.Entry(blog).Collection(navigation).Load();
                        }

                        AssertGraph(blogs, "Updated");
                    });
        }

        protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        {
        }

        protected void AssertGraph(IEnumerable<IBlogAccesor> blogs, string updated = "")
        {
            Assert.Equal(2, blogs.Count());

            var blog1 = blogs.Single(e => e.AccessId == 10);
            Assert.Equal("Blog10" + updated, blog1.AccessTitle);
            Assert.Equal(2, blog1.AccessPosts.Count());

            AssertPost(blog1.AccessPosts.Single(e => e.AccessId == 10), 10, blog1, updated);
            AssertPost(blog1.AccessPosts.Single(e => e.AccessId == 11), 11, blog1, updated);

            var blog2 = blogs.Single(e => e.AccessId == 20);
            Assert.Equal("Blog20" + updated, blog2.AccessTitle);
            Assert.Equal(2, blog2.AccessPosts.Count());

            AssertPost(blog2.AccessPosts.Single(e => e.AccessId == 20), 20, blog2, updated);
            AssertPost(blog2.AccessPosts.Single(e => e.AccessId == 21), 21, blog2, updated);
        }

        private static void AssertPost(IPostAccesor post, int postId, IBlogAccesor blog1, string updated = "")
        {
            Assert.Equal("Post" + postId + updated, post.AccessTitle);
            Assert.Same(blog1, post.AccessBlog);
            Assert.Equal(blog1.AccessId, post.AccessBlogId);
        }

        protected void AssertGraph(IEnumerable<IPostAccesor> posts)
        {
            Assert.Equal(4, posts.Count());

            var blog1 = posts.Select(e => e.AccessBlog).First(e => e.AccessId == 10);
            Assert.Equal("Blog10", blog1.AccessTitle);
            Assert.Equal(2, blog1.AccessPosts.Count());

            var blog2 = posts.Select(e => e.AccessBlog).First(e => e.AccessId == 20);
            Assert.Equal("Blog20", blog2.AccessTitle);
            Assert.Equal(2, blog1.AccessPosts.Count());

            AssertPost(posts.Single(e => e.AccessId == 10), 10, blog1);
            AssertPost(posts.Single(e => e.AccessId == 11), 11, blog1);
            AssertPost(posts.Single(e => e.AccessId == 20), 20, blog2);
            AssertPost(posts.Single(e => e.AccessId == 21), 21, blog2);
        }

        protected class BlogAuto : IBlogAccesor
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string Title { get; set; }
            public IEnumerable<PostAuto> Posts { get; set; }

            int IBlogAccesor.AccessId
            {
                get { return Id; }
                set { Id = value; }
            }

            string IBlogAccesor.AccessTitle
            {
                get { return Title; }
                set { Title = value; }
            }

            IEnumerable<IPostAccesor> IBlogAccesor.AccessPosts
            {
                get { return Posts; }
                set { Posts = (IEnumerable<PostAuto>)value; }
            }
        }

        protected class PostAuto : IPostAccesor
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string Title { get; set; }

            public int BlogId { get; set; }
            public BlogAuto Blog { get; set; }

            int IPostAccesor.AccessId
            {
                get { return Id; }
                set { Id = value; }
            }

            string IPostAccesor.AccessTitle
            {
                get { return Title; }
                set { Title = value; }
            }

            int IPostAccesor.AccessBlogId
            {
                get { return BlogId; }
                set { BlogId = value; }
            }

            IBlogAccesor IPostAccesor.AccessBlog
            {
                get { return Blog; }
                set { Blog = (BlogAuto)value; }
            }
        }

        protected class BlogFull : IBlogAccesor
        {
            private int _id;
            private string _title;
            private ICollection<PostFull> _posts;

            // ReSharper disable once ConvertToAutoProperty
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id
            {
                get { return _id; }
                set { _id = value; }
            }

            // ReSharper disable once ConvertToAutoProperty
            public string Title
            {
                get { return _title; }
                set { _title = value; }
            }

            // ReSharper disable once ConvertToAutoProperty
            public IEnumerable<PostFull> Posts
            {
                get { return _posts; }
                set { _posts = (ICollection<PostFull>)value; }
            }

            int IBlogAccesor.AccessId
            {
                get { return Id; }
                set { Id = value; }
            }

            string IBlogAccesor.AccessTitle
            {
                get { return Title; }
                set { Title = value; }
            }

            IEnumerable<IPostAccesor> IBlogAccesor.AccessPosts
            {
                get { return Posts; }
                set { Posts = (IEnumerable<PostFull>)value; }
            }
        }

        protected class PostFull : IPostAccesor
        {
            private int _id;
            private string _title;
            private int _blogId;
            private BlogFull _blog;

            // ReSharper disable once ConvertToAutoProperty
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id
            {
                get { return _id; }
                set { _id = value; }
            }

            // ReSharper disable once ConvertToAutoProperty
            public string Title
            {
                get { return _title; }
                set { _title = value; }
            }

            // ReSharper disable once ConvertToAutoProperty
            public int BlogId
            {
                get { return _blogId; }
                set { _blogId = value; }
            }

            // ReSharper disable once ConvertToAutoProperty
            public BlogFull Blog
            {
                get { return _blog; }
                set { _blog = value; }
            }

            int IPostAccesor.AccessId
            {
                get { return Id; }
                set { Id = value; }
            }

            string IPostAccesor.AccessTitle
            {
                get { return Title; }
                set { Title = value; }
            }

            int IPostAccesor.AccessBlogId
            {
                get { return BlogId; }
                set { BlogId = value; }
            }

            IBlogAccesor IPostAccesor.AccessBlog
            {
                get { return Blog; }
                set { Blog = (BlogFull)value; }
            }
        }

        protected class BlogFullExplicit : IBlogAccesor
        {
            private int _myid;
            private string _mytitle;
            private ICollection<PostFullExplicit> _myposts;

            // ReSharper disable once ConvertToAutoProperty
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id
            {
                get { return _myid; }
                set { _myid = value; }
            }

            // ReSharper disable once ConvertToAutoProperty
            public string Title
            {
                get { return _mytitle; }
                set { _mytitle = value; }
            }

            // ReSharper disable once ConvertToAutoProperty
            public IEnumerable<PostFullExplicit> Posts
            {
                get { return _myposts; }
                set { _myposts = (ICollection<PostFullExplicit>)value; }
            }

            int IBlogAccesor.AccessId
            {
                get { return Id; }
                set { Id = value; }
            }

            string IBlogAccesor.AccessTitle
            {
                get { return Title; }
                set { Title = value; }
            }

            IEnumerable<IPostAccesor> IBlogAccesor.AccessPosts
            {
                get { return Posts; }
                set { Posts = (IEnumerable<PostFullExplicit>)value; }
            }
        }

        protected class PostFullExplicit : IPostAccesor
        {
            private int _myid;
            private string _mytitle;
            private int _myblogId;
            private BlogFullExplicit _myblog;

            // ReSharper disable once ConvertToAutoProperty
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id
            {
                get { return _myid; }
                set { _myid = value; }
            }

            // ReSharper disable once ConvertToAutoProperty
            public string Title
            {
                get { return _mytitle; }
                set { _mytitle = value; }
            }

            // ReSharper disable once ConvertToAutoProperty
            public int BlogId
            {
                get { return _myblogId; }
                set { _myblogId = value; }
            }

            // ReSharper disable once ConvertToAutoProperty
            public BlogFullExplicit Blog
            {
                get { return _myblog; }
                set { _myblog = value; }
            }

            int IPostAccesor.AccessId
            {
                get { return Id; }
                set { Id = value; }
            }

            string IPostAccesor.AccessTitle
            {
                get { return Title; }
                set { Title = value; }
            }

            int IPostAccesor.AccessBlogId
            {
                get { return BlogId; }
                set { BlogId = value; }
            }

            IBlogAccesor IPostAccesor.AccessBlog
            {
                get { return Blog; }
                set { Blog = (BlogFullExplicit)value; }
            }
        }

        protected class BlogReadOnly : IBlogAccesor
        {
            private int _id;
            private string _title;
            private ICollection<PostReadOnly> _posts;

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id => _id;

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            public string Title => _title;

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            public IEnumerable<PostReadOnly> Posts => _posts;

            int IBlogAccesor.AccessId
            {
                get { return Id; }
                set { _id = value; }
            }

            string IBlogAccesor.AccessTitle
            {
                get { return Title; }
                set { _title = value; }
            }

            IEnumerable<IPostAccesor> IBlogAccesor.AccessPosts
            {
                get { return Posts; }
                set { _posts = (ICollection<PostReadOnly>)value; }
            }
        }

        protected class PostReadOnly : IPostAccesor
        {
            private int _id;
            private string _title;
            private int _blogId;
            private BlogReadOnly _blog;

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id => _id;

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            public string Title => _title;

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            public int BlogId => _blogId;

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            public BlogReadOnly Blog => _blog;

            int IPostAccesor.AccessId
            {
                get { return Id; }
                set { _id = value; }
            }

            string IPostAccesor.AccessTitle
            {
                get { return Title; }
                set { _title = value; }
            }

            int IPostAccesor.AccessBlogId
            {
                get { return BlogId; }
                set { _blogId = value; }
            }

            IBlogAccesor IPostAccesor.AccessBlog
            {
                get { return Blog; }
                set { _blog = (BlogReadOnly)value; }
            }
        }

        protected class BlogReadOnlyExplicit : IBlogAccesor
        {
            private int _myid;
            private string _mytitle;
            private ICollection<PostReadOnlyExplicit> _myposts;

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id => _myid;

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            public string Title => _mytitle;

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            public IEnumerable<PostReadOnlyExplicit> Posts => _myposts;

            int IBlogAccesor.AccessId
            {
                get { return Id; }
                set { _myid = value; }
            }

            string IBlogAccesor.AccessTitle
            {
                get { return Title; }
                set { _mytitle = value; }
            }

            IEnumerable<IPostAccesor> IBlogAccesor.AccessPosts
            {
                get { return Posts; }
                set { _myposts = (ICollection<PostReadOnlyExplicit>)value; }
            }
        }

        protected class PostReadOnlyExplicit : IPostAccesor
        {
            private int _myid;
            private string _mytitle;
            private int _myblogId;
            private BlogReadOnlyExplicit _myblog;

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id => _myid;

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            public string Title => _mytitle;

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            public int BlogId => _myblogId;

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            public BlogReadOnlyExplicit Blog => _myblog;

            int IPostAccesor.AccessId
            {
                get { return Id; }
                set { _myid = value; }
            }

            string IPostAccesor.AccessTitle
            {
                get { return Title; }
                set { _mytitle = value; }
            }

            int IPostAccesor.AccessBlogId
            {
                get { return BlogId; }
                set { _myblogId = value; }
            }

            IBlogAccesor IPostAccesor.AccessBlog
            {
                get { return Blog; }
                set { _myblog = (BlogReadOnlyExplicit)value; }
            }
        }

        protected class BlogWriteOnly : IBlogAccesor
        {
            private int _id;
            private string _title;
            private ICollection<PostWriteOnly> _posts;

            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id
            {
                set { _id = value; }
            }

            public string Title
            {
                set { _title = value; }
            }

            public IEnumerable<PostWriteOnly> Posts
            {
                set { _posts = (ICollection<PostWriteOnly>)value; }
            }

            int IBlogAccesor.AccessId
            {
                get { return _id; }
                set { Id = value; }
            }

            string IBlogAccesor.AccessTitle
            {
                get { return _title; }
                set { Title = value; }
            }

            IEnumerable<IPostAccesor> IBlogAccesor.AccessPosts
            {
                get { return _posts; }
                set { Posts = (IEnumerable<PostWriteOnly>)value; }
            }
        }

        protected class PostWriteOnly : IPostAccesor
        {
            private int _id;
            private string _title;
            private int _blogId;
            private BlogWriteOnly _blog;

            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id
            {
                set { _id = value; }
            }

            public string Title
            {
                set { _title = value; }
            }

            public int BlogId
            {
                set { _blogId = value; }
            }

            public BlogWriteOnly Blog
            {
                set { _blog = value; }
            }

            int IPostAccesor.AccessId
            {
                get { return _id; }
                set { Id = value; }
            }

            string IPostAccesor.AccessTitle
            {
                get { return _title; }
                set { Title = value; }
            }

            int IPostAccesor.AccessBlogId
            {
                get { return _blogId; }
                set { BlogId = value; }
            }

            IBlogAccesor IPostAccesor.AccessBlog
            {
                get { return _blog; }
                set { Blog = (BlogWriteOnly)value; }
            }
        }

        protected class BlogWriteOnlyExplicit : IBlogAccesor
        {
            private int _myid;
            private string _mytitle;
            private ICollection<PostWriteOnlyExplicit> _myposts;

            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id
            {
                set { _myid = value; }
            }

            public string Title
            {
                set { _mytitle = value; }
            }

            public IEnumerable<PostWriteOnlyExplicit> Posts
            {
                set { _myposts = (ICollection<PostWriteOnlyExplicit>)value; }
            }

            int IBlogAccesor.AccessId
            {
                get { return _myid; }
                set { Id = value; }
            }

            string IBlogAccesor.AccessTitle
            {
                get { return _mytitle; }
                set { Title = value; }
            }

            IEnumerable<IPostAccesor> IBlogAccesor.AccessPosts
            {
                get { return _myposts; }
                set { Posts = (IEnumerable<PostWriteOnlyExplicit>)value; }
            }
        }

        protected class PostWriteOnlyExplicit : IPostAccesor
        {
            private int _myid;
            private string _mytitle;
            private int _myblogId;
            private BlogWriteOnlyExplicit _myblog;

            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id
            {
                set { _myid = value; }
            }

            public string Title
            {
                set { _mytitle = value; }
            }

            public int BlogId
            {
                set { _myblogId = value; }
            }

            public BlogWriteOnlyExplicit Blog
            {
                set { _myblog = value; }
            }

            int IPostAccesor.AccessId
            {
                get { return _myid; }
                set { Id = value; }
            }

            string IPostAccesor.AccessTitle
            {
                get { return _mytitle; }
                set { Title = value; }
            }

            int IPostAccesor.AccessBlogId
            {
                get { return _myblogId; }
                set { BlogId = value; }
            }

            IBlogAccesor IPostAccesor.AccessBlog
            {
                get { return _myblog; }
                set { Blog = (BlogWriteOnlyExplicit)value; }
            }
        }

        protected class BlogFields : IBlogAccesor
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            private int _id;

            private string _title;

            public IEnumerable<PostFields> Posts { get; set; }

            int IBlogAccesor.AccessId
            {
                get { return _id; }
                set { _id = value; }
            }

            // ReSharper disable once ConvertToAutoProperty
            string IBlogAccesor.AccessTitle
            {
                get { return _title; }
                set { _title = value; }
            }

            IEnumerable<IPostAccesor> IBlogAccesor.AccessPosts
            {
                get { return Posts; }
                set { Posts = (IEnumerable<PostFields>)value; }
            }
        }

        protected class PostFields : IPostAccesor
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            private int _id;

            private string _title;
            private int _blogId;

            public BlogFields Blog { get; set; }

            int IPostAccesor.AccessId
            {
                get { return _id; }
                set { _id = value; }
            }

            // ReSharper disable once ConvertToAutoProperty
            string IPostAccesor.AccessTitle
            {
                get { return _title; }
                set { _title = value; }
            }

            // ReSharper disable once ConvertToAutoProperty
            int IPostAccesor.AccessBlogId
            {
                get { return _blogId; }
                set { _blogId = value; }
            }

            IBlogAccesor IPostAccesor.AccessBlog
            {
                get { return Blog; }
                set { Blog = (BlogFields)value; }
            }
        }

        protected interface IBlogAccesor
        {
            int AccessId { get; set; }
            string AccessTitle { get; set; }
            IEnumerable<IPostAccesor> AccessPosts { get; set; }
        }

        protected interface IPostAccesor
        {
            int AccessId { get; set; }
            string AccessTitle { get; set; }
            int AccessBlogId { get; set; }
            IBlogAccesor AccessBlog { get; set; }
        }

        protected static TBlog CreateBlogAndPosts<TBlog, TPost>()
            where TBlog : IBlogAccesor, new()
            where TPost : IPostAccesor, new()
            => new TBlog
            {
                AccessId = 10,
                AccessTitle = "Blog10",
                AccessPosts = (IEnumerable<IPostAccesor>)new List<TPost>
                {
                    new TPost { AccessId = 10, AccessTitle = "Post10" },
                    new TPost { AccessId = 11, AccessTitle = "Post11" }
                }
            };

        protected static IList<TPost> CreatePostsAndBlog<TBlog, TPost>()
            where TBlog : IBlogAccesor, new()
            where TPost : IPostAccesor, new()
        {
            var blog = new TBlog
            {
                AccessId = 20,
                AccessTitle = "Blog20"
            };

            return new List<TPost>
            {
                new TPost { AccessId = 20, AccessTitle = "Post20", AccessBlog = blog },
                new TPost { AccessId = 21, AccessTitle = "Post21", AccessBlog = blog }
            };
        }

        protected class FieldMappingContext : DbContext
        {
            public FieldMappingContext(DbContextOptions options)
                : base(options)
            {
            }
        }

        protected FieldMappingContext CreateContext()
            => (FieldMappingContext)Fixture.CreateContext(TestStore);

        public void Dispose() => TestStore.Dispose();

        protected TFixture Fixture { get; }

        protected TTestStore TestStore { get; }

        public abstract class FieldMappingFixtureBase
        {
            public abstract TTestStore CreateTestStore();

            public abstract DbContext CreateContext(TTestStore testStore);

            protected virtual void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<PostAuto>();
                modelBuilder.Entity<BlogAuto>();

                modelBuilder.Entity<PostFull>();
                modelBuilder.Entity<BlogFull>();

                modelBuilder.Entity<PostFullExplicit>(b =>
                    {
                        b.Property(e => e.Id).HasField("_myid");
                        b.Property(e => e.Title).HasField("_mytitle");
                        b.Property(e => e.BlogId).HasField("_myblogId");
                        ;
                    });

                modelBuilder.Entity<BlogFullExplicit>(b =>
                    {
                        b.Property(e => e.Id).HasField("_myid");
                        b.Property(e => e.Title).HasField("_mytitle");
                        b.HasMany(e => e.Posts).WithOne(e => e.Blog).HasForeignKey(e => e.BlogId);
                    });

                modelBuilder.Entity<PostFullExplicit>().Metadata.FindNavigation("Blog").SetField("_myblog");
                modelBuilder.Entity<BlogFullExplicit>().Metadata.FindNavigation("Posts").SetField("_myposts");

                if (modelBuilder.Model.GetPropertyAccessMode() != PropertyAccessMode.Property)
                {
                    modelBuilder.Entity<PostReadOnly>(b =>
                        {
                            b.HasKey(e => e.Id);
                            b.Property(e => e.Title);
                            b.Property(e => e.BlogId);
                        });

                    modelBuilder.Entity<BlogReadOnly>(b =>
                        {
                            b.HasKey(e => e.Id);
                            b.Property(e => e.Title);
                            b.HasMany(e => e.Posts).WithOne(e => e.Blog).HasForeignKey(e => e.BlogId);
                        });

                    modelBuilder.Entity<PostReadOnlyExplicit>(b =>
                        {
                            b.HasKey(e => e.Id);
                            b.Property(e => e.Id).HasField("_myid");
                            b.Property(e => e.Title).HasField("_mytitle");
                            b.Property(e => e.BlogId).HasField("_myblogId");
                            ;
                        });

                    modelBuilder.Entity<BlogReadOnlyExplicit>(b =>
                        {
                            b.HasKey(e => e.Id);
                            b.Property(e => e.Id).HasField("_myid");
                            b.Property(e => e.Title).HasField("_mytitle");
                            b.HasMany(e => e.Posts).WithOne(e => e.Blog).HasForeignKey(e => e.BlogId);
                        });

                    modelBuilder.Entity<PostReadOnlyExplicit>().Metadata.FindNavigation("Blog").SetField("_myblog");
                    modelBuilder.Entity<BlogReadOnlyExplicit>().Metadata.FindNavigation("Posts").SetField("_myposts");

                    modelBuilder.Entity<PostWriteOnly>(b =>
                        {
                            b.HasKey("Id");
                            b.Property("Title");
                            b.Property("BlogId");
                        });

                    modelBuilder.Entity<BlogWriteOnly>(b =>
                        {
                            b.HasKey("Id");
                            b.Property("Title");
                            b.HasMany(typeof(PostWriteOnly).DisplayName(), "Posts").WithOne("Blog").HasForeignKey("BlogId");
                        });

                    modelBuilder.Entity<PostWriteOnlyExplicit>(b =>
                        {
                            b.HasKey("Id");
                            b.Property("Id").HasField("_myid");
                            b.Property("Title").HasField("_mytitle");
                            b.Property("BlogId").HasField("_myblogId");
                        });

                    modelBuilder.Entity<BlogWriteOnlyExplicit>(b =>
                        {
                            b.HasKey("Id");
                            b.Property("Id").HasField("_myid");
                            b.Property("Title").HasField("_mytitle");
                            b.HasMany(typeof(PostWriteOnlyExplicit).DisplayName(), "Posts").WithOne("Blog").HasForeignKey("BlogId");
                        });

                    modelBuilder.Entity<PostWriteOnlyExplicit>().Metadata.FindNavigation("Blog").SetField("_myblog");
                    modelBuilder.Entity<BlogWriteOnlyExplicit>().Metadata.FindNavigation("Posts").SetField("_myposts");

                    modelBuilder.Entity<PostFields>(b =>
                        {
                            b.Property("_id");
                            b.HasKey("_id");
                            b.Property("_title");
                            b.Property("_blogId");
                        });

                    modelBuilder.Entity<BlogFields>(b =>
                        {
                            b.Property("_id");
                            b.HasKey("_id");
                            b.Property("_title");
                            b.HasMany(e => e.Posts).WithOne(e => e.Blog).HasForeignKey("_blogId");
                        });
                }
            }

            protected virtual void Seed(DbContext context)
            {
                context.Add(CreateBlogAndPosts<BlogAuto, PostAuto>());
                context.AddRange(CreatePostsAndBlog<BlogAuto, PostAuto>());

                context.Add(CreateBlogAndPosts<BlogFull, PostFull>());
                context.AddRange(CreatePostsAndBlog<BlogFull, PostFull>());

                context.Add(CreateBlogAndPosts<BlogFullExplicit, PostFullExplicit>());
                context.AddRange(CreatePostsAndBlog<BlogFullExplicit, PostFullExplicit>());

                if (context.Model.GetPropertyAccessMode() != PropertyAccessMode.Property)
                {
                    context.Add(CreateBlogAndPosts<BlogReadOnly, PostReadOnly>());
                    context.AddRange(CreatePostsAndBlog<BlogReadOnly, PostReadOnly>());

                    context.Add(CreateBlogAndPosts<BlogReadOnlyExplicit, PostReadOnlyExplicit>());
                    context.AddRange(CreatePostsAndBlog<BlogReadOnlyExplicit, PostReadOnlyExplicit>());

                    context.Add(CreateBlogAndPosts<BlogWriteOnly, PostWriteOnly>());
                    context.AddRange(CreatePostsAndBlog<BlogWriteOnly, PostWriteOnly>());

                    context.Add(CreateBlogAndPosts<BlogWriteOnlyExplicit, PostWriteOnlyExplicit>());
                    context.AddRange(CreatePostsAndBlog<BlogWriteOnlyExplicit, PostWriteOnlyExplicit>());

                    context.Add(CreateBlogAndPosts<BlogFields, PostFields>());
                    context.AddRange(CreatePostsAndBlog<BlogFields, PostFields>());
                }

                context.SaveChanges();
            }
        }
    }
}
