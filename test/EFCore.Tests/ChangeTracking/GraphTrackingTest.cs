// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

public class GraphTrackingTest
{
    [ConditionalFact]
    public void Can_add_aggregate()
    {
        using var context = new AggregateContext();
        var comments0 = new[] { new Comment(), new Comment() };
        var comments1 = new[] { new Comment(), new Comment() };
        var posts = new[] { new Post { Comments = comments0.ToList() }, new Post { Comments = comments1.ToList() } };
        var blog = new Blog { Posts = posts.ToList() };

        context.Add(blog);

        Assert.Equal(EntityState.Added, context.Entry(blog).State);
        Assert.Equal(EntityState.Added, context.Entry(posts[0]).State);
        Assert.Equal(EntityState.Added, context.Entry(posts[1]).State);
        Assert.Equal(EntityState.Added, context.Entry(comments0[0]).State);
        Assert.Equal(EntityState.Added, context.Entry(comments0[1]).State);
        Assert.Equal(EntityState.Added, context.Entry(comments1[0]).State);
        Assert.Equal(EntityState.Added, context.Entry(comments1[1]).State);
    }

    [ConditionalFact]
    public void Can_add_one_to_one_aggregate()
    {
        using var context = new AggregateContext();
        var statistics = new BlogCategoryStatistics();
        var category = new BlogCategory { Statistics = statistics };

        context.Add(category);

        Assert.Equal(EntityState.Added, context.Entry(category).State);
        Assert.Equal(EntityState.Added, context.Entry(category.Statistics).State);
    }

    [ConditionalFact]
    public void Can_attach_aggregate()
    {
        using var context = new AggregateContext();
        var comments0 = new[] { new Comment { Id = 33, PostId = 55 }, new Comment { Id = 34, PostId = 55 } };
        var comments1 = new[] { new Comment { Id = 44, PostId = 56 }, new Comment { Id = 45, PostId = 56 } };
        var posts = new[]
        {
            new Post
            {
                Id = 55,
                BlogId = 66,
                Comments = comments0.ToList()
            },
            new Post
            {
                Id = 56,
                BlogId = 66,
                Comments = comments1.ToList()
            }
        };
        var blog = new Blog { Id = 66, Posts = posts.ToList() };

        context.Attach(blog);

        Assert.Equal(EntityState.Unchanged, context.Entry(blog).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(posts[0]).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(posts[1]).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(comments0[0]).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(comments0[1]).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(comments1[0]).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(comments1[1]).State);
    }

    [ConditionalFact]
    public void Can_attach_one_to_one_aggregate()
    {
        using var context = new AggregateContext();
        var statistics = new BlogCategoryStatistics { Id = 11, BlogCategoryId = 22 };
        var category = new BlogCategory { Id = 22, Statistics = statistics };

        context.Attach(category);

        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(category.Statistics).State);
    }

    [ConditionalFact]
    public void Attaching_aggregate_with_no_key_set_adds_it_instead()
    {
        using var context = new AggregateContext();
        var comments0 = new[] { new Comment(), new Comment() };
        var comments1 = new[] { new Comment(), new Comment() };
        var posts = new[] { new Post { Comments = comments0.ToList() }, new Post { Comments = comments1.ToList() } };
        var blog = new Blog { Posts = posts.ToList() };

        context.Attach(blog);

        Assert.Equal(EntityState.Added, context.Entry(blog).State);
        Assert.Equal(EntityState.Added, context.Entry(posts[0]).State);
        Assert.Equal(EntityState.Added, context.Entry(posts[1]).State);
        Assert.Equal(EntityState.Added, context.Entry(comments0[0]).State);
        Assert.Equal(EntityState.Added, context.Entry(comments0[1]).State);
        Assert.Equal(EntityState.Added, context.Entry(comments1[0]).State);
        Assert.Equal(EntityState.Added, context.Entry(comments1[1]).State);
    }

    [ConditionalFact]
    public void Attaching_one_to_one_aggregate_with_no_key_set_adds_it_instead()
    {
        using var context = new AggregateContext();
        var statistics = new BlogCategoryStatistics();
        var category = new BlogCategory { Statistics = statistics };

        context.Attach(category);

        Assert.Equal(EntityState.Added, context.Entry(category).State);
        Assert.Equal(EntityState.Added, context.Entry(category.Statistics).State);
    }

    [ConditionalFact]
    public void Dependents_with_no_key_set_are_added()
    {
        using var context = new AggregateContext();
        var comments0 = new[] { new Comment { Id = 33, PostId = 55 }, new Comment { Id = 34, PostId = 55 } };
        var comments1 = new[] { new Comment { PostId = 56 }, new Comment { PostId = 56 } };
        var posts = new[]
        {
            new Post
            {
                Id = 55,
                BlogId = 66,
                Comments = comments0.ToList()
            },
            new Post { BlogId = 66, Comments = comments1.ToList() }
        };
        var blog = new Blog { Id = 66, Posts = posts.ToList() };

        context.Attach(blog);

        Assert.Equal(EntityState.Unchanged, context.Entry(blog).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(posts[0]).State);
        Assert.Equal(EntityState.Added, context.Entry(posts[1]).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(comments0[0]).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(comments0[1]).State);
        Assert.Equal(EntityState.Added, context.Entry(comments1[0]).State);
        Assert.Equal(EntityState.Added, context.Entry(comments1[1]).State);
    }

    [ConditionalFact]
    public void One_to_one_dependents_with_no_key_set_are_added()
    {
        using var context = new AggregateContext();
        var statistics = new BlogCategoryStatistics { BlogCategoryId = 22 };
        var category = new BlogCategory { Id = 22, Statistics = statistics };

        context.Attach(category);

        Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
        Assert.Equal(EntityState.Added, context.Entry(category.Statistics).State);
    }

    [ConditionalFact]
    public void Can_add_aggregate_with_linked_aggregate_also_added()
    {
        using var context = new AggregateContext();
        var reminders = new[] { new Reminder { Id = 11 }, new Reminder { Id = 12 } };
        var author = new Author { Id = 22, Reminders = reminders.ToList() };

        var comments0 = new[] { new Comment { Id = 33, Author = author }, new Comment { Id = 34, Author = author } };
        var comments1 = new[] { new Comment { Id = 44, Author = author }, new Comment { Id = 45, Author = author } };
        var posts = new[]
        {
            new Post
            {
                Id = 55,
                Author = author,
                Comments = comments0.ToList()
            },
            new Post
            {
                Id = 56,
                Author = author,
                Comments = comments1.ToList()
            }
        };
        var blog = new Blog
        {
            Id = 66,
            Author = author,
            Posts = posts.ToList()
        };

        context.Add(blog);

        Assert.Equal(EntityState.Added, context.Entry(blog).State);
        Assert.Equal(EntityState.Added, context.Entry(posts[0]).State);
        Assert.Equal(EntityState.Added, context.Entry(posts[1]).State);
        Assert.Equal(EntityState.Added, context.Entry(comments0[0]).State);
        Assert.Equal(EntityState.Added, context.Entry(comments0[1]).State);
        Assert.Equal(EntityState.Added, context.Entry(comments1[0]).State);
        Assert.Equal(EntityState.Added, context.Entry(comments1[1]).State);
        Assert.Equal(EntityState.Added, context.Entry(author).State);
        Assert.Equal(EntityState.Added, context.Entry(reminders[0]).State);
        Assert.Equal(EntityState.Added, context.Entry(reminders[1]).State);
    }

    [ConditionalFact]
    public void Can_add_aggregate_with_other_linked_aggregate_also_attached()
    {
        using var context = new AggregateContext();
        var reminders = new[] { new Reminder { Id = 11 }, new Reminder { Id = 12 } };
        var author = new Author { Id = 22, Reminders = reminders.ToList() };

        var comments0 = new[] { new Comment { Id = 33, Author = author }, new Comment { Id = 34, Author = author } };
        var comments1 = new[] { new Comment { Id = 44, Author = author }, new Comment { Id = 45, Author = author } };
        var posts = new[]
        {
            new Post
            {
                Id = 55,
                Author = author,
                Comments = comments0.ToList()
            },
            new Post
            {
                Id = 56,
                Author = author,
                Comments = comments1.ToList()
            }
        };
        var blog = new Blog
        {
            Id = 66,
            Author = author,
            Posts = posts.ToList()
        };

        author.Comments = comments0.Concat(comments1).ToList();
        comments0[0].Post = posts[0];
        posts[0].Blog = blog;

        context.Add(author);

        Assert.Equal(EntityState.Added, context.Entry(blog).State);
        Assert.Equal(EntityState.Added, context.Entry(posts[0]).State);
        Assert.Equal(EntityState.Added, context.Entry(posts[1]).State);
        Assert.Equal(EntityState.Added, context.Entry(comments0[0]).State);
        Assert.Equal(EntityState.Added, context.Entry(comments0[1]).State);
        Assert.Equal(EntityState.Added, context.Entry(comments1[0]).State);
        Assert.Equal(EntityState.Added, context.Entry(comments1[1]).State);
        Assert.Equal(EntityState.Added, context.Entry(author).State);
        Assert.Equal(EntityState.Added, context.Entry(reminders[0]).State);
        Assert.Equal(EntityState.Added, context.Entry(reminders[1]).State);
    }

    [ConditionalFact]
    public void Can_attach_aggregate_with_linked_aggregate_also_attached()
    {
        using var context = new AggregateContext();
        var reminders = new[] { new Reminder { Id = 11, AuthorId = 22 }, new Reminder { Id = 12, AuthorId = 22 } };
        var author = new Author { Id = 22, Reminders = reminders.ToList() };

        var comments0 = new[]
        {
            new Comment
            {
                Id = 33,
                AuthorId = 22,
                PostId = 55,
                Author = author
            },
            new Comment
            {
                Id = 34,
                AuthorId = 22,
                PostId = 55,
                Author = author
            }
        };

        var comments1 = new[]
        {
            new Comment
            {
                Id = 44,
                AuthorId = 22,
                PostId = 56,
                Author = author
            },
            new Comment
            {
                Id = 45,
                AuthorId = 22,
                PostId = 56,
                Author = author
            }
        };

        var posts = new[]
        {
            new Post
            {
                Id = 55,
                AuthorId = 22,
                BlogId = 66,
                Author = author,
                Comments = comments0.ToList()
            },
            new Post
            {
                Id = 56,
                AuthorId = 22,
                BlogId = 66,
                Author = author,
                Comments = comments1.ToList()
            }
        };

        var blog = new Blog
        {
            Id = 66,
            AuthorId = 22,
            Author = author,
            Posts = posts.ToList()
        };

        context.Attach(blog);

        Assert.Equal(EntityState.Unchanged, context.Entry(blog).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(posts[0]).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(posts[1]).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(comments0[0]).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(comments0[1]).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(comments1[0]).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(comments1[1]).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(author).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(reminders[0]).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(reminders[1]).State);
    }

    [ConditionalFact]
    public void Can_add_two_aggregates_linked_down_the_tree()
    {
        using var context = new AggregateContext();
        var reminders = new[] { new Reminder { Id = 11 }, new Reminder { Id = 12 } };
        var author = new Author { Id = 22, Reminders = reminders.ToList() };

        var comments0 = new[] { new Comment { Id = 33, Author = author }, new Comment { Id = 34, Author = author } };
        var comments1 = new[] { new Comment { Id = 44, Author = author }, new Comment { Id = 45, Author = author } };
        var posts = new[]
        {
            new Post
            {
                Id = 55,
                Author = author,
                Comments = comments0.ToList()
            },
            new Post
            {
                Id = 56,
                Author = author,
                Comments = comments1.ToList()
            }
        };
        var blog = new Blog
        {
            Id = 66,
            Author = author,
            Posts = posts.ToList()
        };

        context.AddRange(blog, author);

        Assert.Equal(EntityState.Added, context.Entry(blog).State);
        Assert.Equal(EntityState.Added, context.Entry(posts[0]).State);
        Assert.Equal(EntityState.Added, context.Entry(posts[1]).State);
        Assert.Equal(EntityState.Added, context.Entry(comments0[0]).State);
        Assert.Equal(EntityState.Added, context.Entry(comments0[1]).State);
        Assert.Equal(EntityState.Added, context.Entry(comments1[0]).State);
        Assert.Equal(EntityState.Added, context.Entry(comments1[1]).State);
        Assert.Equal(EntityState.Added, context.Entry(author).State);
        Assert.Equal(EntityState.Added, context.Entry(reminders[0]).State);
        Assert.Equal(EntityState.Added, context.Entry(reminders[1]).State);
    }

    private class AggregateContext : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(nameof(AggregateContext))
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider);

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Author> Authors { get; set; }
    }

    private class BlogCategoryStatistics
    {
        public int Id { get; set; }

        public int? BlogCategoryId { get; set; }
        public BlogCategory BlogCategory { get; set; }
    }

    private class BlogCategory
    {
        public int Id { get; set; }

        public BlogCategoryStatistics Statistics { get; set; }

        public ICollection<Blog> Blogs { get; set; }
    }

    private class Blog
    {
        public int Id { get; set; }

        public int? BlogCategoryId { get; set; }
        public BlogCategory BlogCategory { get; set; }

        public int? AuthorId { get; set; }
        public Author Author { get; set; }

        public ICollection<Post> Posts { get; set; }
    }

    private class Post
    {
        public int Id { get; set; }

        public int? AuthorId { get; set; }
        public Author Author { get; set; }

        public int BlogId { get; set; }
        public Blog Blog { get; set; }

        public ICollection<Comment> Comments { get; set; }
    }

    private class Comment
    {
        public int Id { get; set; }

        public int? AuthorId { get; set; }
        public Author Author { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; }
    }

    private class Author
    {
        public int Id { get; set; }

        public ICollection<Blog> Blogs { get; set; }
        public ICollection<Post> Posts { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Reminder> Reminders { get; set; }
    }

    private class Reminder
    {
        public int Id { get; set; }

        public int AuthorId { get; set; }
        public Author Author { get; set; }
    }
}
