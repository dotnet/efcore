// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

// ReSharper disable ConvertToAutoProperty
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class FieldMappingTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : FieldMappingTestBase<TFixture>.FieldMappingFixtureBase, new()
{
    protected FieldMappingTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

    protected static AsyncLocal<bool> _isSeeding = new();

    protected interface IUser2;

    protected class User2 : IUser2
    {
        public int Id { get; set; }
    }

    protected class LoginSession
    {
        private object _id;
        private IUser2 _user;
        private object _users;

        public int Id
        {
            get => (int)_id;
            set => _id = value;
        }

        public virtual User2 User
        {
            get => (User2)_user;
            set => _user = value;
        }

        public virtual ICollection<User2> Users
        {
            get => (ICollection<User2>)_users;
            set => _users = value;
        }
    }

    [ConditionalFact]
    public virtual void Field_mapping_with_conversion_does_not_throw()
    {
        using var context = CreateContext();
        var session = context.Set<LoginSession>().Include(e => e.User).Include(e => e.Users).Single();

        var entry = context.Entry(session);

        Assert.Same(session.User, entry.Reference(e => e.User).CurrentValue);
        Assert.Same(session.Users.Single(), entry.Collection(e => e.Users).CurrentValue.Single());
        Assert.Equal(session.Id, entry.Property(e => e.Id).CurrentValue);
        Assert.Equal(session.Id, entry.Property(e => e.Id).OriginalValue);

        var newUser = new User2();
        var newUsers = new List<User2> { new() };

        entry.Reference(e => e.User).CurrentValue = newUser;
        entry.Collection(e => e.Users).CurrentValue = newUsers;

        Assert.Same(newUser, session.User);
        Assert.Same(newUsers, session.Users);

        var newSession = new LoginSession { Id = 77 };
        var newEntry = context.Entry(newSession);
        newEntry.State = EntityState.Added;

        Assert.Equal(77, newEntry.Property(e => e.Id).CurrentValue);
        newEntry.Property(e => e.Id).CurrentValue = 78;
        Assert.Equal(78, newSession.Id);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Simple_query_auto_props(bool tracking)
    {
        using var context = CreateContext();
        AssertBlogs(context.Set<BlogAuto>().AsTracking(tracking).ToList());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Include_collection_auto_props(bool tracking)
    {
        using var context = CreateContext();
        AssertGraph(context.Set<BlogAuto>().Include(e => e.Posts).AsTracking(tracking).ToList());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Include_reference_auto_props(bool tracking)
    {
        using var context = CreateContext();
        AssertGraph(context.Set<PostAuto>().Include(e => e.Blog).AsTracking(tracking).ToList(), tracking);
    }

    [ConditionalFact]
    public virtual void Load_collection_auto_props()
        => Load_collection<BlogAuto>("Posts");

    [ConditionalFact]
    public virtual void Load_reference_auto_props()
        => Load_reference<PostAuto>("Blog");

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Query_with_conditional_constant_auto_props(bool tracking)
        => Query_with_conditional_constant<PostAuto>("BlogId", tracking);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Query_with_conditional_param_auto_props(bool tracking)
        => Query_with_conditional_param<PostAuto>("Title", tracking);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Projection_auto_props(bool tracking)
        => Projection<PostAuto>("Id", "Title", tracking);

    [ConditionalFact]
    public virtual Task Update_auto_props()
        => UpdateAsync<BlogAuto>("Posts");

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Simple_query_hiding_props(bool tracking)
    {
        using var context = CreateContext();
        AssertBlogs(context.Set<BlogHiding>().AsTracking(tracking).ToList());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Include_collection_hiding_props(bool tracking)
    {
        using var context = CreateContext();
        AssertGraph(context.Set<BlogHiding>().Include(e => e.Posts).AsTracking(tracking).ToList());
    }

    [ConditionalFact]
    public virtual void Can_define_a_backing_field_for_a_navigation_and_query_and_update_it()
    {
        using (var context = CreateContext())
        {
            var principal = context.Set<OneToOneFieldNavPrincipal>().OrderBy(e => e.Id).First();
            var dependent1 = new NavDependent
            {
                Id = 1,
                Name = "FirstName",
                OneToOneFieldNavPrincipal = principal
            };
            context.Set<NavDependent>().Add(dependent1);
            context.SaveChanges();

            var dependentName =
                context.Set<OneToOneFieldNavPrincipal>().OrderBy(e => e.Id).Select(p => p.Dependent.Name).First();

            Assert.Equal("FirstName", dependentName);

            // use the backing field directly
            var dependent2 = new NavDependent
            {
                Id = 2,
                Name = "SecondName",
                OneToOneFieldNavPrincipal = principal
            };
            principal._unconventionalDependent = dependent2;
            context.SaveChanges();

            dependentName =
                context.Set<OneToOneFieldNavPrincipal>().OrderBy(e => e.Id).Select(p => p.Dependent.Name).First();

            Assert.Equal("SecondName", dependentName);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Include_reference_hiding_props(bool tracking)
    {
        using var context = CreateContext();
        AssertGraph(context.Set<PostHiding>().Include(e => e.Blog).AsTracking(tracking).ToList(), tracking);
    }

    [ConditionalFact]
    public virtual void Load_collection_hiding_props()
        => Load_collection<BlogHiding>("Posts");

    [ConditionalFact]
    public virtual void Load_reference_hiding_props()
        => Load_reference<PostHiding>("Blog");

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Query_with_conditional_constant_hiding_props(bool tracking)
        => Query_with_conditional_constant<PostHiding>("BlogId", tracking);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Query_with_conditional_param_hiding_props(bool tracking)
        => Query_with_conditional_param<PostHiding>("Title", tracking);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Projection_hiding_props(bool tracking)
        => Projection<PostHiding>("Id", "Title", tracking);

    [ConditionalFact]
    public virtual Task Update_hiding_props()
        => UpdateAsync<BlogHiding>("Posts");

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Simple_query_full_props(bool tracking)
    {
        using var context = CreateContext();
        AssertBlogs(context.Set<BlogFull>().AsTracking(tracking).ToList());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Include_collection_full_props(bool tracking)
    {
        using var context = CreateContext();
        AssertGraph(context.Set<BlogFull>().Include(e => e.Posts).AsTracking(tracking).ToList());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Include_reference_full_props(bool tracking)
    {
        using var context = CreateContext();
        AssertGraph(context.Set<PostFull>().Include(e => e.Blog).AsTracking(tracking).ToList(), tracking);
    }

    [ConditionalFact]
    public virtual void Load_collection_full_props()
        => Load_collection<BlogFull>("Posts");

    [ConditionalFact]
    public virtual void Load_reference_full_props()
        => Load_reference<PostFull>("Blog");

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Query_with_conditional_constant_full_props(bool tracking)
        => Query_with_conditional_constant<PostFull>("BlogId", tracking);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Query_with_conditional_param_full_props(bool tracking)
        => Query_with_conditional_param<PostFull>("Title", tracking);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Projection_full_props(bool tracking)
        => Projection<PostFull>("Id", "Title", tracking);

    [ConditionalFact]
    public virtual Task Update_full_props()
        => UpdateAsync<BlogFull>("Posts");

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Simple_query_full_props_with_named_fields(bool tracking)
    {
        using var context = CreateContext();
        AssertBlogs(context.Set<BlogFullExplicit>().AsTracking(tracking).ToList());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Include_collection_full_props_with_named_fields(bool tracking)
    {
        using var context = CreateContext();
        AssertGraph(context.Set<BlogFullExplicit>().Include(e => e.Posts).AsTracking(tracking).ToList());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Include_reference_full_props_with_named_fields(bool tracking)
    {
        using var context = CreateContext();
        AssertGraph(context.Set<PostFullExplicit>().Include(e => e.Blog).AsTracking(tracking).ToList(), tracking);
    }

    [ConditionalFact]
    public virtual void Load_collection_full_props_with_named_fields()
        => Load_collection<BlogFullExplicit>("Posts");

    [ConditionalFact]
    public virtual void Load_reference_full_props_with_named_fields()
        => Load_reference<PostFullExplicit>("Blog");

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Query_with_conditional_constant_full_props_with_named_fields(bool tracking)
        => Query_with_conditional_constant<PostFullExplicit>("BlogId", tracking);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Query_with_conditional_param_full_props_with_named_fields(bool tracking)
        => Query_with_conditional_param<PostFullExplicit>("Title", tracking);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Projection_full_props_with_named_fields(bool tracking)
        => Projection<PostFullExplicit>("Id", "Title", tracking);

    [ConditionalFact]
    public virtual Task Update_full_props_with_named_fields()
        => UpdateAsync<BlogFullExplicit>("Posts");

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Simple_query_read_only_props(bool tracking)
    {
        using var context = CreateContext();
        AssertBlogs(context.Set<BlogReadOnly>().AsTracking(tracking).ToList());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Include_collection_read_only_props(bool tracking)
    {
        using var context = CreateContext();
        AssertGraph(context.Set<BlogReadOnly>().Include(e => e.Posts).AsTracking(tracking).ToList());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Include_reference_read_only_props(bool tracking)
    {
        using var context = CreateContext();
        AssertGraph(context.Set<PostReadOnly>().Include(e => e.Blog).AsTracking(tracking).ToList(), tracking);
    }

    [ConditionalFact]
    public virtual void Load_collection_read_only_props()
        => Load_collection<BlogReadOnly>("Posts");

    [ConditionalFact]
    public virtual void Load_reference_read_only_props()
        => Load_reference<PostReadOnly>("Blog");

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Query_with_conditional_constant_read_only_props(bool tracking)
        => Query_with_conditional_constant<PostReadOnly>("BlogId", tracking);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Query_with_conditional_param_read_only_props(bool tracking)
        => Query_with_conditional_param<PostReadOnly>("Title", tracking);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Projection_read_only_props(bool tracking)
        => Projection<PostReadOnly>("Id", "Title", tracking);

    [ConditionalFact]
    public virtual Task Update_read_only_props()
        => UpdateAsync<BlogReadOnly>("Posts");

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Simple_query_props_with_IReadOnlyCollection(bool tracking)
    {
        using var context = CreateContext();
        AssertBlogs(context.Set<BlogWithReadOnlyCollection>().AsTracking(tracking).ToList());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Include_collection_props_with_IReadOnlyCollection(bool tracking)
    {
        using var context = CreateContext();
        AssertGraph(context.Set<BlogWithReadOnlyCollection>().Include(e => e.Posts).AsTracking(tracking).ToList());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Include_reference_props_with_IReadOnlyCollection(bool tracking)
    {
        using var context = CreateContext();
        AssertGraph(context.Set<PostWithReadOnlyCollection>().Include(e => e.Blog).AsTracking(tracking).ToList(), tracking);
    }

    [ConditionalFact]
    public virtual void Load_collection_props_with_IReadOnlyCollection()
        => Load_collection<BlogWithReadOnlyCollection>("Posts");

    [ConditionalFact]
    public virtual void Load_reference_props_with_IReadOnlyCollection()
        => Load_reference<PostWithReadOnlyCollection>("Blog");

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Query_with_conditional_constant_props_with_IReadOnlyCollection(bool tracking)
        => Query_with_conditional_constant<PostWithReadOnlyCollection>("BlogId", tracking);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Query_with_conditional_param_props_with_IReadOnlyCollection(bool tracking)
        => Query_with_conditional_param<PostWithReadOnlyCollection>("Title", tracking);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Projection_props_with_IReadOnlyCollection(bool tracking)
        => Projection<PostWithReadOnlyCollection>("Id", "Title", tracking);

    [ConditionalFact]
    public virtual Task Update_props_with_IReadOnlyCollection()
        => UpdateAsync<BlogWithReadOnlyCollection>("Posts");

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Simple_query_read_only_props_with_named_fields(bool tracking)
    {
        using var context = CreateContext();
        AssertBlogs(context.Set<BlogReadOnlyExplicit>().AsTracking(tracking).ToList());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Include_collection_read_only_props_with_named_fields(bool tracking)
    {
        using var context = CreateContext();
        AssertGraph(context.Set<BlogReadOnlyExplicit>().Include(e => e.Posts).AsTracking(tracking).ToList());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Include_reference_read_only_props_with_named_fields(bool tracking)
    {
        using var context = CreateContext();
        AssertGraph(context.Set<PostReadOnlyExplicit>().Include(e => e.Blog).AsTracking(tracking).ToList(), tracking);
    }

    [ConditionalFact]
    public virtual void Load_collection_read_only_props_with_named_fields()
        => Load_collection<BlogReadOnlyExplicit>("Posts");

    [ConditionalFact]
    public virtual void Load_reference_read_only_props_with_named_fields()
        => Load_reference<PostReadOnlyExplicit>("Blog");

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Query_with_conditional_constant_read_only_props_with_named_fields(bool tracking)
        => Query_with_conditional_constant<PostReadOnlyExplicit>("BlogId", tracking);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Query_with_conditional_param_read_only_props_with_named_fields(bool tracking)
        => Query_with_conditional_param<PostReadOnlyExplicit>("Title", tracking);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Projection_read_only_props_with_named_fields(bool tracking)
        => Projection<PostReadOnlyExplicit>("Id", "Title", tracking);

    [ConditionalFact]
    public virtual Task Update_read_only_props_with_named_fields()
        => UpdateAsync<BlogReadOnlyExplicit>("Posts");

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Simple_query_write_only_props(bool tracking)
    {
        using var context = CreateContext();
        AssertBlogs(context.Set<BlogWriteOnly>().AsTracking(tracking).ToList());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Include_collection_write_only_props(bool tracking)
    {
        using var context = CreateContext();
        AssertGraph(context.Set<BlogWriteOnly>().Include("Posts").AsTracking(tracking).ToList());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Include_reference_write_only_props(bool tracking)
    {
        using var context = CreateContext();
        AssertGraph(context.Set<PostWriteOnly>().Include("Blog").AsTracking(tracking).ToList(), tracking);
    }

    [ConditionalFact]
    public virtual void Load_collection_write_only_props()
        => Load_collection<BlogWriteOnly>("Posts");

    [ConditionalFact]
    public virtual void Load_reference_write_only_props()
        => Load_reference<PostWriteOnly>("Blog");

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Query_with_conditional_constant_write_only_props(bool tracking)
        => Query_with_conditional_constant<PostWriteOnly>("BlogId", tracking);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Query_with_conditional_param_write_only_props(bool tracking)
        => Query_with_conditional_param<PostWriteOnly>("Title", tracking);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Projection_write_only_props(bool tracking)
        => Projection<PostWriteOnly>("Id", "Title", tracking);

    [ConditionalFact]
    public virtual Task Update_write_only_props()
        => UpdateAsync<BlogWriteOnly>("Posts");

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Simple_query_write_only_props_with_named_fields(bool tracking)
    {
        using var context = CreateContext();
        AssertBlogs(context.Set<BlogWriteOnlyExplicit>().AsTracking(tracking).ToList());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Include_collection_write_only_props_with_named_fields(bool tracking)
    {
        using var context = CreateContext();
        AssertGraph(context.Set<BlogWriteOnlyExplicit>().Include("Posts").AsTracking(tracking).ToList());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Include_reference_write_only_props_with_named_fields(bool tracking)
    {
        using var context = CreateContext();
        AssertGraph(context.Set<PostWriteOnlyExplicit>().Include("Blog").AsTracking(tracking).ToList(), tracking);
    }

    [ConditionalFact]
    public virtual void Load_collection_write_only_props_with_named_fields()
        => Load_collection<BlogWriteOnlyExplicit>("Posts");

    [ConditionalFact]
    public virtual void Load_reference_write_only_props_with_named_fields()
        => Load_reference<PostWriteOnlyExplicit>("Blog");

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Query_with_conditional_constant_write_only_props_with_named_fields(bool tracking)
        => Query_with_conditional_constant<PostWriteOnlyExplicit>("BlogId", tracking);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Query_with_conditional_param_write_only_props_with_named_fields(bool tracking)
        => Query_with_conditional_param<PostWriteOnlyExplicit>("Title", tracking);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Projection_write_only_props_with_named_fields(bool tracking)
        => Projection<PostWriteOnlyExplicit>("Id", "Title", tracking);

    [ConditionalFact]
    public virtual Task Update_write_only_props_with_named_fields()
        => UpdateAsync<BlogWriteOnlyExplicit>("Posts");

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Simple_query_fields_only(bool tracking)
    {
        using var context = CreateContext();
        AssertBlogs(context.Set<BlogFields>().AsTracking(tracking).ToList());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Include_collection_fields_only(bool tracking)
    {
        using var context = CreateContext();
        AssertGraph(context.Set<BlogFields>().Include(e => e.Posts).AsTracking(tracking).ToList());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Include_reference_fields_only(bool tracking)
    {
        using var context = CreateContext();
        AssertGraph(context.Set<PostFields>().Include(e => e.Blog).AsTracking(tracking).ToList(), tracking);
    }

    [ConditionalFact]
    public virtual void Load_collection_fields_only()
        => Load_collection<BlogFields>("Posts");

    [ConditionalFact]
    public virtual void Load_reference_fields_only()
        => Load_reference<PostFields>("Blog");

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Query_with_conditional_constant_fields_only(bool tracking)
        => Query_with_conditional_constant<PostFields>("_blogId", tracking);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Query_with_conditional_param_fields_only(bool tracking)
        => Query_with_conditional_param<PostFields>("_title", tracking);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Projection_fields_only(bool tracking)
        => Projection<PostFields>("_id", "_title", tracking);

    [ConditionalFact]
    public virtual Task Update_fields_only()
        => UpdateAsync<BlogFields>("Posts");

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Simple_query_fields_only_for_navs_too(bool tracking)
    {
        using var context = CreateContext();
        AssertBlogs(context.Set<BlogNavFields>().AsTracking(tracking).ToList());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Include_collection_fields_only_for_navs_too(bool tracking)
    {
        using var context = CreateContext();
        AssertGraph(context.Set<BlogNavFields>().Include("_posts").AsTracking(tracking).ToList());
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Include_reference_fields_only_only_for_navs_too(bool tracking)
    {
        using var context = CreateContext();
        AssertGraph(context.Set<PostNavFields>().Include("_blog").AsTracking(tracking).ToList(), tracking);
    }

    [ConditionalFact]
    public virtual void Load_collection_fields_only_only_for_navs_too()
        => Load_collection<BlogNavFields>("_posts");

    [ConditionalFact]
    public virtual void Load_reference_fields_only_only_for_navs_too()
        => Load_reference<PostNavFields>("_blog");

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Query_with_conditional_constant_fields_only_only_for_navs_too(bool tracking)
        => Query_with_conditional_constant<PostNavFields>("_blogId", tracking);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Query_with_conditional_param_fields_only_only_for_navs_too(bool tracking)
        => Query_with_conditional_param<PostNavFields>("_title", tracking);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Projection_fields_only_only_for_navs_too(bool tracking)
        => Projection<PostNavFields>("_id", "_title", tracking);

    [ConditionalFact]
    public virtual Task Update_fields_only_only_for_navs_too()
        => UpdateAsync<BlogNavFields>("_posts");

    protected virtual void Load_collection<TBlog>(string navigation)
        where TBlog : class, IBlogAccessor, new()
    {
        using var context = CreateContext();
        var blogs = context.Set<TBlog>().ToList();

        foreach (var blog in blogs)
        {
            context.Entry(blog).Collection(navigation).Load();
        }

        AssertGraph(blogs);
    }

    protected virtual void Load_reference<TPost>(string navigation)
        where TPost : class, IPostAccessor, new()
    {
        using var context = CreateContext();
        var posts = context.Set<TPost>().ToList();

        foreach (var post in posts)
        {
            context.Entry(post).Reference(navigation).Load();
        }

        AssertGraph(posts, true);
    }

    protected virtual void Query_with_conditional_constant<TPost>(string property, bool tracking)
        where TPost : class, IPostAccessor, new()
    {
        using var context = CreateContext();
        var posts = context.Set<TPost>().Where(p => EF.Property<int>(p, property) == 10).AsTracking(tracking).ToList();

        Assert.Equal(2, posts.Count);

        var post1 = posts.Single(e => e.AccessId == 10);
        Assert.Equal("Post10", post1.AccessTitle);
        Assert.Equal(10, post1.AccessBlogId);

        var post2 = posts.Single(e => e.AccessId == 11);
        Assert.Equal("Post11", post2.AccessTitle);
        Assert.Equal(10, post2.AccessBlogId);
    }

    protected virtual void Query_with_conditional_param<TPost>(string property, bool tracking)
        where TPost : class, IPostAccessor, new()
    {
        var postTitle = "Post11";
        using var context = CreateContext();
        var posts = context.Set<TPost>().Where(p => EF.Property<string>(p, property) == postTitle).AsTracking(tracking).ToList();

        Assert.Single(posts);

        var post = posts.Single(e => e.AccessId == 11);
        Assert.Equal("Post11", post.AccessTitle);
        Assert.Equal(10, post.AccessBlogId);
    }

    protected virtual void Projection<TPost>(string property1, string property2, bool tracking)
        where TPost : class, IPostAccessor, new()
    {
        using var context = CreateContext();
        var posts = context.Set<TPost>().Select(
                p => new { Prop1 = EF.Property<int>(p, property1), Prop2 = EF.Property<string>(p, property2) }).AsTracking(tracking)
            .ToList();

        Assert.Equal(4, posts.Count);

        Assert.Equal("Post10", posts.Single(e => e.Prop1 == 10).Prop2);
        Assert.Equal("Post11", posts.Single(e => e.Prop1 == 11).Prop2);
        Assert.Equal("Post20", posts.Single(e => e.Prop1 == 20).Prop2);
        Assert.Equal("Post21", posts.Single(e => e.Prop1 == 21).Prop2);
    }

    protected virtual Task UpdateAsync<TBlog>(string navigation)
        where TBlog : class, IBlogAccessor, new()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext, UseTransaction, async context =>
            {
                var blogs = await context.Set<TBlog>().ToListAsync();

                foreach (var blog in blogs)
                {
                    await context.Entry(blog).Collection(navigation).LoadAsync();

                    blog.AccessTitle += "Updated";

                    foreach (var post in blog.AccessPosts)
                    {
                        post.AccessTitle += "Updated";
                    }
                }

                AssertGraph(blogs, "Updated");

                await context.SaveChangesAsync();

                AssertGraph(blogs, "Updated");
            }, async context =>
            {
                var blogs = context.Set<TBlog>().ToList();

                foreach (var blog in blogs)
                {
                    await context.Entry(blog).Collection(navigation).LoadAsync();
                }

                AssertGraph(blogs, "Updated");
            });

    protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
    {
    }

    protected void AssertBlogs(IEnumerable<IBlogAccessor> blogs)
    {
        Assert.Equal(2, blogs.Count());
        Assert.Equal("Blog10", blogs.Single(e => e.AccessId == 10).AccessTitle);
        Assert.Equal("Blog20", blogs.Single(e => e.AccessId == 20).AccessTitle);
    }

    protected void AssertGraph(IEnumerable<IBlogAccessor> blogs, string updated = "")
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

    private static void AssertPost(IPostAccessor post, int postId, IBlogAccessor blog, string updated = "")
    {
        Assert.Equal("Post" + postId + updated, post.AccessTitle);
        Assert.Same(blog, post.AccessBlog);
        Assert.Equal(blog.AccessId, post.AccessBlogId);
    }

    protected void AssertGraph(IEnumerable<IPostAccessor> posts, bool tracking)
    {
        Assert.Equal(4, posts.Count());

        AssertBlogs(posts, tracking, 10, 11, "Blog10");
        AssertBlogs(posts, tracking, 20, 21, "Blog20");
    }

    private static void AssertBlogs(IEnumerable<IPostAccessor> posts, bool tracking, int post1Id, int post2Id, string blogName)
    {
        var blog1a = posts.Single(e => e.AccessId == post1Id).AccessBlog;
        var blog1b = posts.Single(e => e.AccessId == post2Id).AccessBlog;

        if (tracking)
        {
            Assert.Same(blog1a, blog1b);
            Assert.Equal(blogName, blog1a.AccessTitle);
            Assert.Equal(2, blog1a.AccessPosts.Count());
        }
        else
        {
            // Because no identity resolution for no-tracking
            Assert.NotSame(blog1a, blog1b);
            Assert.Equal(blogName, blog1a.AccessTitle);
            Assert.Equal(blogName, blog1b.AccessTitle);
            Assert.Single(blog1a.AccessPosts);
            Assert.Single(blog1b.AccessPosts);
        }

        AssertPost(posts.Single(e => e.AccessId == post1Id), post1Id, blog1a);
        AssertPost(posts.Single(e => e.AccessId == post2Id), post2Id, blog1b);
    }

    protected class BlogAuto : IBlogAccessor
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Title { get; set; }
        public IEnumerable<PostAuto> Posts { get; set; }

        int IBlogAccessor.AccessId
        {
            get => Id;
            set => Id = value;
        }

        string IBlogAccessor.AccessTitle
        {
            get => Title;
            set => Title = value;
        }

        IEnumerable<IPostAccessor> IBlogAccessor.AccessPosts
        {
            get => Posts;
            set => Posts = (IEnumerable<PostAuto>)value;
        }
    }

    protected class PostAuto : IPostAccessor
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Title { get; set; }

        public int BlogId { get; set; }
        public BlogAuto Blog { get; set; }

        int IPostAccessor.AccessId
        {
            get => Id;
            set => Id = value;
        }

        string IPostAccessor.AccessTitle
        {
            get => Title;
            set => Title = value;
        }

        int IPostAccessor.AccessBlogId
        {
            get => BlogId;
            set => BlogId = value;
        }

        IBlogAccessor IPostAccessor.AccessBlog
        {
            get => Blog;
            set => Blog = (BlogAuto)value;
        }
    }

    protected class BlogFull : IBlogAccessor
    {
        private int _id;
        private string _title;
#pragma warning disable 649
        private List<PostFull> _posts;
#pragma warning restore 649

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id
        {
            get => _id;
            set => _id = value;
        }

        public string Title
        {
            get => _title;
            set => _title = value;
        }

        public IEnumerable<PostFull> Posts
        {
            get => _posts;
            set
            {
                if (!_isSeeding.Value)
                {
                    throw new InvalidOperationException();
                }

                _posts = (List<PostFull>)value;
            }
        }

        int IBlogAccessor.AccessId
        {
            get => Id;
            set => Id = value;
        }

        string IBlogAccessor.AccessTitle
        {
            get => Title;
            set => Title = value;
        }

        IEnumerable<IPostAccessor> IBlogAccessor.AccessPosts
        {
            get => Posts;
            set => _posts = (List<PostFull>)value;
        }
    }

    protected class PostFull : IPostAccessor
    {
        private int _id;
        private string _title;
        private int _blogId;
#pragma warning disable 649
        private BlogFull _blog;
#pragma warning restore 649

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id
        {
            get => _id;
            set => _id = value;
        }

        public string Title
        {
            get => _title;
            set => _title = value;
        }

        public int BlogId
        {
            get => _blogId;
            set => _blogId = value;
        }

        public BlogFull Blog
        {
            get => _blog;
            set
            {
                if (!_isSeeding.Value)
                {
                    throw new InvalidOperationException();
                }

                _blog = value;
            }
        }

        int IPostAccessor.AccessId
        {
            get => Id;
            set => Id = value;
        }

        string IPostAccessor.AccessTitle
        {
            get => Title;
            set => Title = value;
        }

        int IPostAccessor.AccessBlogId
        {
            get => BlogId;
            set => BlogId = value;
        }

        IBlogAccessor IPostAccessor.AccessBlog
        {
            get => Blog;
            set => _blog = (BlogFull)value;
        }
    }

    protected class BlogNavFields : IBlogAccessor
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        private int _id;

        private string _title;
        private IList<PostNavFields> _posts;

        int IBlogAccessor.AccessId
        {
            get => _id;
            set => _id = value;
        }

        string IBlogAccessor.AccessTitle
        {
            get => _title;
            set => _title = value;
        }

        IEnumerable<IPostAccessor> IBlogAccessor.AccessPosts
        {
            get => _posts;
            set => _posts = (IList<PostNavFields>)value;
        }
    }

    protected class PostNavFields : IPostAccessor
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        private int _id;

        private string _title;
        private int _blogId;

        private BlogNavFields _blog;

        int IPostAccessor.AccessId
        {
            get => _id;
            set => _id = value;
        }

        string IPostAccessor.AccessTitle
        {
            get => _title;
            set => _title = value;
        }

        int IPostAccessor.AccessBlogId
        {
            get => _blogId;
            set => _blogId = value;
        }

        IBlogAccessor IPostAccessor.AccessBlog
        {
            get => _blog;
            set => _blog = (BlogNavFields)value;
        }
    }

    protected class BlogFullExplicit : IBlogAccessor
    {
        private int _myid;
        private string _mytitle;
        private IList<PostFullExplicit> _myposts;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id
        {
            get => _myid;
            set => _myid = value;
        }

        public string Title
        {
            get => _mytitle;
            set => _mytitle = value;
        }

        public IEnumerable<PostFullExplicit> Posts
        {
            get => _myposts;
            set => _myposts = (IList<PostFullExplicit>)value;
        }

        int IBlogAccessor.AccessId
        {
            get => Id;
            set => Id = value;
        }

        string IBlogAccessor.AccessTitle
        {
            get => Title;
            set => Title = value;
        }

        IEnumerable<IPostAccessor> IBlogAccessor.AccessPosts
        {
            get => Posts;
            set => Posts = (IEnumerable<PostFullExplicit>)value;
        }
    }

    protected class PostFullExplicit : IPostAccessor
    {
        private int _myid;
        private string _mytitle;
        private int _myblogId;
        private BlogFullExplicit _myblog;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id
        {
            get => _myid;
            set => _myid = value;
        }

        public string Title
        {
            get => _mytitle;
            set => _mytitle = value;
        }

        public int BlogId
        {
            get => _myblogId;
            set => _myblogId = value;
        }

        public BlogFullExplicit Blog
        {
            get => _myblog;
            set => _myblog = value;
        }

        int IPostAccessor.AccessId
        {
            get => Id;
            set => Id = value;
        }

        string IPostAccessor.AccessTitle
        {
            get => Title;
            set => Title = value;
        }

        int IPostAccessor.AccessBlogId
        {
            get => BlogId;
            set => BlogId = value;
        }

        IBlogAccessor IPostAccessor.AccessBlog
        {
            get => Blog;
            set => Blog = (BlogFullExplicit)value;
        }
    }

    protected class BlogReadOnly : IBlogAccessor
    {
        private int _id;
        private string _title;
        private ObservableCollection<PostReadOnly> _posts;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id
            => _id;

        public string Title
            => _title;

        public IEnumerable<PostReadOnly> Posts
            => _posts;

        int IBlogAccessor.AccessId
        {
            get => Id;
            set => _id = value;
        }

        string IBlogAccessor.AccessTitle
        {
            get => Title;
            set => _title = value;
        }

        IEnumerable<IPostAccessor> IBlogAccessor.AccessPosts
        {
            get => Posts;
            set => _posts = (ObservableCollection<PostReadOnly>)value;
        }
    }

    protected class PostReadOnly : IPostAccessor
    {
        private int _id;
        private string _title;
        private int _blogId;
        private BlogReadOnly _blog;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id
            => _id;

        public string Title
            => _title;

        public int BlogId
            => _blogId;

        public BlogReadOnly Blog
            => _blog;

        int IPostAccessor.AccessId
        {
            get => Id;
            set => _id = value;
        }

        string IPostAccessor.AccessTitle
        {
            get => Title;
            set => _title = value;
        }

        int IPostAccessor.AccessBlogId
        {
            get => BlogId;
            set => _blogId = value;
        }

        IBlogAccessor IPostAccessor.AccessBlog
        {
            get => Blog;
            set => _blog = (BlogReadOnly)value;
        }
    }

    protected class BlogWithReadOnlyCollection : IBlogAccessor
    {
        private int _id;
        private string _title;
        private ICollection<PostWithReadOnlyCollection> _posts;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id
            => _id;

        public string Title
            => _title;

        public IReadOnlyCollection<PostWithReadOnlyCollection> Posts
            => (IReadOnlyCollection<PostWithReadOnlyCollection>)_posts;

        int IBlogAccessor.AccessId
        {
            get => Id;
            set => _id = value;
        }

        string IBlogAccessor.AccessTitle
        {
            get => Title;
            set => _title = value;
        }

        IEnumerable<IPostAccessor> IBlogAccessor.AccessPosts
        {
            get => Posts;
            set => _posts = (ICollection<PostWithReadOnlyCollection>)value;
        }
    }

    protected class PostWithReadOnlyCollection : IPostAccessor
    {
        private int _id;
        private string _title;
        private int _blogId;
        private BlogWithReadOnlyCollection _blog;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id
            => _id;

        public string Title
            => _title;

        public int BlogId
            => _blogId;

        public BlogWithReadOnlyCollection Blog
            => _blog;

        int IPostAccessor.AccessId
        {
            get => Id;
            set => _id = value;
        }

        string IPostAccessor.AccessTitle
        {
            get => Title;
            set => _title = value;
        }

        int IPostAccessor.AccessBlogId
        {
            get => BlogId;
            set => _blogId = value;
        }

        IBlogAccessor IPostAccessor.AccessBlog
        {
            get => Blog;
            set => _blog = (BlogWithReadOnlyCollection)value;
        }
    }

    protected class BlogReadOnlyExplicit : IBlogAccessor
    {
        private int _myid;
        private string _mytitle;
        private Collection<PostReadOnlyExplicit> _myposts;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id
            => _myid;

        public string Title
            => _mytitle;

        public IEnumerable<PostReadOnlyExplicit> Posts
            => _myposts;

        int IBlogAccessor.AccessId
        {
            get => Id;
            set => _myid = value;
        }

        string IBlogAccessor.AccessTitle
        {
            get => Title;
            set => _mytitle = value;
        }

        IEnumerable<IPostAccessor> IBlogAccessor.AccessPosts
        {
            get => Posts;
            set => _myposts = (Collection<PostReadOnlyExplicit>)value;
        }
    }

    protected class PostReadOnlyExplicit : IPostAccessor
    {
        private int _myid;
        private string _mytitle;
        private int _myblogId;
        private BlogReadOnlyExplicit _myblog;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id
            => _myid;

        public string Title
            => _mytitle;

        public int BlogId
            => _myblogId;

        public BlogReadOnlyExplicit Blog
            => _myblog;

        int IPostAccessor.AccessId
        {
            get => Id;
            set => _myid = value;
        }

        string IPostAccessor.AccessTitle
        {
            get => Title;
            set => _mytitle = value;
        }

        int IPostAccessor.AccessBlogId
        {
            get => BlogId;
            set => _myblogId = value;
        }

        IBlogAccessor IPostAccessor.AccessBlog
        {
            get => Blog;
            set => _myblog = (BlogReadOnlyExplicit)value;
        }
    }

    protected class BlogWriteOnly : IBlogAccessor
    {
        private int _id;
        private string _title;
        private IEnumerable<PostWriteOnly> _posts;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id
        {
            set => _id = value;
        }

        public string Title
        {
            set => _title = value;
        }

        public IEnumerable<PostWriteOnly> Posts
        {
            set => _posts = value;
        }

        int IBlogAccessor.AccessId
        {
            get => _id;
            set => Id = value;
        }

        string IBlogAccessor.AccessTitle
        {
            get => _title;
            set => Title = value;
        }

        IEnumerable<IPostAccessor> IBlogAccessor.AccessPosts
        {
            get => _posts;
            set => Posts = (IEnumerable<PostWriteOnly>)value;
        }
    }

    [PrimaryKey(nameof(Id))]
    protected class PostWriteOnly : IPostAccessor
    {
        private int _id;
        private string _title;
        private int _blogId;
        private BlogWriteOnly _blog;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id
        {
            set => _id = value;
        }

        public string Title
        {
            set => _title = value;
        }

        public int BlogId
        {
            set => _blogId = value;
        }

        public BlogWriteOnly Blog
        {
            set => _blog = value;
        }

        int IPostAccessor.AccessId
        {
            get => _id;
            set => Id = value;
        }

        string IPostAccessor.AccessTitle
        {
            get => _title;
            set => Title = value;
        }

        int IPostAccessor.AccessBlogId
        {
            get => _blogId;
            set => BlogId = value;
        }

        IBlogAccessor IPostAccessor.AccessBlog
        {
            get => _blog;
            set => Blog = (BlogWriteOnly)value;
        }
    }

    protected class BlogWriteOnlyExplicit : IBlogAccessor
    {
        private int _myid;
        private string _mytitle;
        private ICollection<PostWriteOnlyExplicit> _myposts;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id
        {
            set => _myid = value;
        }

        public string Title
        {
            set => _mytitle = value;
        }

        public IEnumerable<PostWriteOnlyExplicit> Posts
        {
            set => _myposts = (ICollection<PostWriteOnlyExplicit>)value;
        }

        int IBlogAccessor.AccessId
        {
            get => _myid;
            set => Id = value;
        }

        string IBlogAccessor.AccessTitle
        {
            get => _mytitle;
            set => Title = value;
        }

        IEnumerable<IPostAccessor> IBlogAccessor.AccessPosts
        {
            get => _myposts;
            set => Posts = (IEnumerable<PostWriteOnlyExplicit>)value;
        }
    }

    protected class PostWriteOnlyExplicit : IPostAccessor
    {
        private int _myid;
        private string _mytitle;
        private int _myblogId;
        private BlogWriteOnlyExplicit _myblog;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id
        {
            set => _myid = value;
        }

        public string Title
        {
            set => _mytitle = value;
        }

        public int BlogId
        {
            set => _myblogId = value;
        }

        public BlogWriteOnlyExplicit Blog
        {
            set => _myblog = value;
        }

        int IPostAccessor.AccessId
        {
            get => _myid;
            set => Id = value;
        }

        string IPostAccessor.AccessTitle
        {
            get => _mytitle;
            set => Title = value;
        }

        int IPostAccessor.AccessBlogId
        {
            get => _myblogId;
            set => BlogId = value;
        }

        IBlogAccessor IPostAccessor.AccessBlog
        {
            get => _myblog;
            set => Blog = (BlogWriteOnlyExplicit)value;
        }
    }

    protected class BlogFields : IBlogAccessor
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        private int _id;

        private string _title;

        public IEnumerable<PostFields> Posts { get; set; }

        int IBlogAccessor.AccessId
        {
            get => _id;
            set => _id = value;
        }

        string IBlogAccessor.AccessTitle
        {
            get => _title;
            set => _title = value;
        }

        IEnumerable<IPostAccessor> IBlogAccessor.AccessPosts
        {
            get => Posts;
            set => Posts = (IEnumerable<PostFields>)value;
        }
    }

    protected class PostFields : IPostAccessor
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        private int _id;

        private string _title;
        private int _blogId;

        public BlogFields Blog { get; set; }

        int IPostAccessor.AccessId
        {
            get => _id;
            set => _id = value;
        }

        string IPostAccessor.AccessTitle
        {
            get => _title;
            set => _title = value;
        }

        int IPostAccessor.AccessBlogId
        {
            get => _blogId;
            set => _blogId = value;
        }

        IBlogAccessor IPostAccessor.AccessBlog
        {
            get => Blog;
            set => Blog = (BlogFields)value;
        }
    }

    protected interface IBlogAccessor
    {
        int AccessId { get; set; }
        string AccessTitle { get; set; }
        IEnumerable<IPostAccessor> AccessPosts { get; set; }
    }

    protected interface IPostAccessor
    {
        int AccessId { get; set; }
        string AccessTitle { get; set; }
        int AccessBlogId { get; set; }
        IBlogAccessor AccessBlog { get; set; }
    }

    protected static TBlog CreateBlogAndPosts<TBlog, TPost>(
        ICollection<TPost> posts)
        where TBlog : IBlogAccessor, new()
        where TPost : IPostAccessor, new()
    {
        posts.Add(
            new TPost { AccessId = 10, AccessTitle = "Post10" });

        posts.Add(
            new TPost { AccessId = 11, AccessTitle = "Post11" });

        return new TBlog
        {
            AccessId = 10,
            AccessTitle = "Blog10",
            AccessPosts = (IEnumerable<IPostAccessor>)posts
        };
    }

    protected static IList<TPost> CreatePostsAndBlog<TBlog, TPost>()
        where TBlog : IBlogAccessor, new()
        where TPost : IPostAccessor, new()
    {
        var blog = new TBlog { AccessId = 20, AccessTitle = "Blog20" };

        return new List<TPost>
        {
            new()
            {
                AccessId = 20,
                AccessTitle = "Post20",
                AccessBlog = blog
            },
            new()
            {
                AccessId = 21,
                AccessTitle = "Post21",
                AccessBlog = blog
            }
        };
    }

    protected class BlogHidingBase
    {
        public object Id { get; set; } = 0;

        public object Title { get; set; }
        public object Posts { get; set; }
    }

    protected class BlogHiding : BlogHidingBase, IBlogAccessor
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public new int Id
        {
            get => base.Id is int value ? value : default;
            set => base.Id = value;
        }

        public new string Title
        {
            get => base.Title is string value ? value : default;
            set => base.Title = value;
        }

        public new IEnumerable<PostHiding> Posts
        {
            get => base.Posts is IEnumerable<PostHiding> value ? value : default;
            set => base.Posts = value;
        }

        int IBlogAccessor.AccessId
        {
            get => Id;
            set => Id = value;
        }

        string IBlogAccessor.AccessTitle
        {
            get => Title;
            set => Title = value;
        }

        IEnumerable<IPostAccessor> IBlogAccessor.AccessPosts
        {
            get => Posts;
            set => Posts = (IEnumerable<PostHiding>)value;
        }
    }

    protected class PostHidingBase
    {
        public object Id { get; set; } = 0;

        public object Title { get; set; }

        public object BlogId { get; set; } = 0;
        public object Blog { get; set; }
    }

    protected class PostHiding : PostHidingBase, IPostAccessor
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public new int Id
        {
            get => base.Id is int value ? value : default;
            set => base.Id = value;
        }

        public new string Title
        {
            get => base.Title is string value ? value : default;
            set => base.Title = value;
        }

        public new int BlogId
        {
            get => base.BlogId is int value ? value : default;
            set => base.BlogId = value;
        }

        public new BlogHiding Blog
        {
            get => base.Blog is BlogHiding value ? value : default;
            set => base.Blog = value;
        }

        int IPostAccessor.AccessId
        {
            get => Id;
            set => Id = value;
        }

        string IPostAccessor.AccessTitle
        {
            get => Title;
            set => Title = value;
        }

        int IPostAccessor.AccessBlogId
        {
            get => BlogId;
            set => BlogId = value;
        }

        IBlogAccessor IPostAccessor.AccessBlog
        {
            get => Blog;
            set => Blog = (BlogHiding)value;
        }
    }

    protected class OneToOneFieldNavPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Name { get; set; }

        public NavDependent _unconventionalDependent; // won't be picked up by convention

        public NavDependent Dependent
        {
            get => throw new NotImplementedException("Invalid attempt to access Dependent getter");
            set => throw new NotImplementedException("Invalid attempt to access Dependent setter");
        }
    }

    protected class NavDependent
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Name { get; set; }

        public OneToOneFieldNavPrincipal OneToOneFieldNavPrincipal { get; set; }
    }

    protected DbContext CreateContext()
        => Fixture.CreateContext();

    public abstract class FieldMappingFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName
            => "FieldMapping";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<PostHiding>();
            modelBuilder.Entity<BlogHiding>();

            modelBuilder.Entity<PostAuto>();
            modelBuilder.Entity<BlogAuto>();

            modelBuilder.Entity<PostFull>();
            modelBuilder.Entity<BlogFull>();

            modelBuilder.Entity<PostFullExplicit>(
                b =>
                {
                    b.Property(e => e.Id).HasField("_myid");
                    b.Property(e => e.Title).HasField("_mytitle");
                    b.Property(e => e.BlogId).HasField("_myblogId");
                });

            modelBuilder.Entity<BlogFullExplicit>(
                b =>
                {
                    b.Property(e => e.Id).HasField("_myid");
                    b.Property(e => e.Title).HasField("_mytitle");
                    b.HasMany(e => e.Posts).WithOne(e => e.Blog).HasForeignKey(e => e.BlogId);
                });

            modelBuilder.Entity<PostFullExplicit>().Metadata.FindNavigation("Blog").SetField("_myblog");
            modelBuilder.Entity<BlogFullExplicit>().Metadata.FindNavigation("Posts").SetField("_myposts");

            modelBuilder.Entity<LoginSession>().UsePropertyAccessMode(PropertyAccessMode.Field);

            modelBuilder.Entity<OneToOneFieldNavPrincipal>()
                .HasOne(e => e.Dependent)
                .WithOne(e => e.OneToOneFieldNavPrincipal)
                .HasForeignKey<NavDependent>();

            modelBuilder.Entity<OneToOneFieldNavPrincipal>()
                .Navigation(e => e.Dependent)
                .HasField("_unconventionalDependent")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            modelBuilder.Entity<NavDependent>();

            if (modelBuilder.Model.GetPropertyAccessMode() != PropertyAccessMode.Property)
            {
                modelBuilder.Entity<PostReadOnly>(
                    b =>
                    {
                        b.HasKey(e => e.Id);
                        b.Property(e => e.Title);
                        b.Property(e => e.BlogId);
                    });

                modelBuilder.Entity<BlogReadOnly>(
                    b =>
                    {
                        b.HasKey(e => e.Id);
                        b.Property(e => e.Title);
                        b.HasMany(e => e.Posts).WithOne(e => e.Blog).HasForeignKey(e => e.BlogId);
                    });

                modelBuilder.Entity<PostWithReadOnlyCollection>(
                    b =>
                    {
                        b.HasKey(e => e.Id);
                        b.Property(e => e.Title);
                        b.Property(e => e.BlogId);
                    });

                modelBuilder.Entity<BlogWithReadOnlyCollection>(
                    b =>
                    {
                        b.HasKey(e => e.Id);
                        b.Property(e => e.Title);
                        b.HasMany(e => e.Posts).WithOne(e => e.Blog).HasForeignKey(e => e.BlogId);
                    });

                modelBuilder.Entity<PostReadOnlyExplicit>(
                    b =>
                    {
                        b.HasKey(e => e.Id);
                        b.Property(e => e.Id).HasField("_myid");
                        b.Property(e => e.Title).HasField("_mytitle");
                        b.Property(e => e.BlogId).HasField("_myblogId");
                    });

                modelBuilder.Entity<BlogReadOnlyExplicit>(
                    b =>
                    {
                        b.HasKey(e => e.Id);
                        b.Property(e => e.Id).HasField("_myid");
                        b.Property(e => e.Title).HasField("_mytitle");
                        b.HasMany(e => e.Posts).WithOne(e => e.Blog).HasForeignKey(e => e.BlogId);
                    });

                modelBuilder.Entity<PostReadOnlyExplicit>().Metadata.FindNavigation("Blog").SetField("_myblog");
                modelBuilder.Entity<BlogReadOnlyExplicit>().Metadata.FindNavigation("Posts").SetField("_myposts");

                modelBuilder.Entity<PostWriteOnly>(
                    b =>
                    {
                        b.Property("Title");
                        b.Property("BlogId");
                    });

                modelBuilder.Entity<BlogWriteOnly>(
                    b =>
                    {
                        b.HasKey("Id");
                        b.Property("Title");
                        b.HasMany(typeof(PostWriteOnly).DisplayName(), "Posts").WithOne("Blog").HasForeignKey("BlogId");
                    });

                modelBuilder.Entity<PostWriteOnlyExplicit>(
                    b =>
                    {
                        b.HasKey("Id");
                        b.Property("Id").HasField("_myid");
                        b.Property("Title").HasField("_mytitle");
                        b.Property("BlogId").HasField("_myblogId");
                    });

                modelBuilder.Entity<BlogWriteOnlyExplicit>(
                    b =>
                    {
                        b.HasKey("Id");
                        b.Property("Id").HasField("_myid");
                        b.Property("Title").HasField("_mytitle");
                        b.HasMany(typeof(PostWriteOnlyExplicit).DisplayName(), "Posts").WithOne("Blog").HasForeignKey("BlogId");
                    });

                modelBuilder.Entity<PostWriteOnlyExplicit>().Metadata.FindNavigation("Blog").SetField("_myblog");
                modelBuilder.Entity<BlogWriteOnlyExplicit>().Metadata.FindNavigation("Posts").SetField("_myposts");

                modelBuilder.Entity<PostFields>(
                    b =>
                    {
                        b.Property("_id");
                        b.HasKey("_id");
                        b.Property("_title");
                        b.Property("_blogId");
                    });

                modelBuilder.Entity<BlogFields>(
                    b =>
                    {
                        b.Property("_id");
                        b.HasKey("_id");
                        b.Property("_title");
                        b.HasMany(e => e.Posts).WithOne(e => e.Blog).HasForeignKey("_blogId");
                    });

                modelBuilder.Entity<PostNavFields>(
                    b =>
                    {
                        b.Property("_id");
                        b.HasKey("_id");
                        b.Property("_title");
                        b.Property("_blogId");
                    });

                modelBuilder.Entity<BlogNavFields>(
                    b =>
                    {
                        b.Property("_id");
                        b.HasKey("_id");
                        b.Property("_title");
                        b.HasMany(typeof(PostNavFields), "_posts").WithOne("_blog").HasForeignKey("_blogId");
                    });
            }
        }

        protected override async Task SeedAsync(PoolableDbContext context)
        {
            _isSeeding.Value = true;
            try
            {
                context.Add(CreateBlogAndPosts<BlogAuto, PostAuto>(new List<PostAuto>()));
                context.AddRange(CreatePostsAndBlog<BlogAuto, PostAuto>());

                context.Add(CreateBlogAndPosts<BlogHiding, PostHiding>(new List<PostHiding>()));
                context.AddRange(CreatePostsAndBlog<BlogHiding, PostHiding>());

                context.Add(CreateBlogAndPosts<BlogFull, PostFull>(new List<PostFull>()));
                context.AddRange(CreatePostsAndBlog<BlogFull, PostFull>());

                context.Add(CreateBlogAndPosts<BlogFullExplicit, PostFullExplicit>(new List<PostFullExplicit>()));
                context.AddRange(CreatePostsAndBlog<BlogFullExplicit, PostFullExplicit>());

                if (context.GetService<IDesignTimeModel>().Model.GetPropertyAccessMode() != PropertyAccessMode.Property)
                {
                    context.Add(CreateBlogAndPosts<BlogReadOnly, PostReadOnly>(new ObservableCollection<PostReadOnly>()));
                    context.AddRange(CreatePostsAndBlog<BlogReadOnly, PostReadOnly>());

                    context.Add(
                        CreateBlogAndPosts<BlogWithReadOnlyCollection, PostWithReadOnlyCollection>(
                            new ObservableCollection<PostWithReadOnlyCollection>()));
                    context.AddRange(CreatePostsAndBlog<BlogWithReadOnlyCollection, PostWithReadOnlyCollection>());

                    context.Add(CreateBlogAndPosts<BlogReadOnlyExplicit, PostReadOnlyExplicit>(new Collection<PostReadOnlyExplicit>()));
                    context.AddRange(CreatePostsAndBlog<BlogReadOnlyExplicit, PostReadOnlyExplicit>());

                    context.Add(CreateBlogAndPosts<BlogWriteOnly, PostWriteOnly>(new List<PostWriteOnly>()));
                    context.AddRange(CreatePostsAndBlog<BlogWriteOnly, PostWriteOnly>());

                    context.Add(CreateBlogAndPosts<BlogWriteOnlyExplicit, PostWriteOnlyExplicit>(new HashSet<PostWriteOnlyExplicit>()));
                    context.AddRange(CreatePostsAndBlog<BlogWriteOnlyExplicit, PostWriteOnlyExplicit>());

                    context.Add(CreateBlogAndPosts<BlogFields, PostFields>(new List<PostFields>()));
                    context.AddRange(CreatePostsAndBlog<BlogFields, PostFields>());

                    context.Add(CreateBlogAndPosts<BlogNavFields, PostNavFields>(new List<PostNavFields>()));
                    context.AddRange(CreatePostsAndBlog<BlogNavFields, PostNavFields>());
                }

                context.Add(
                    new LoginSession { User = new User2(), Users = new List<User2> { new() } });

                context.Add(new OneToOneFieldNavPrincipal { Id = 1, Name = "OneToOneFieldNavPrincipal1" });

                await context.SaveChangesAsync();
            }
            finally
            {
                _isSeeding.Value = true;
            }
        }
    }
}
