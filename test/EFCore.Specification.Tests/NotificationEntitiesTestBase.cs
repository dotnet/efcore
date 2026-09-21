// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public abstract class NotificationEntitiesTestBase<TFixture>(TFixture fixture) : IClassFixture<TFixture>
    where TFixture : NotificationEntitiesTestBase<TFixture>.NotificationEntitiesFixtureBase, new()
{
    protected virtual TFixture Fixture { get; } = fixture;

    [Fact] // Issue #4020
    public virtual void Include_brings_entities_referenced_from_already_tracked_notification_entities_as_Unchanged()
    {
        using var context = CreateContext();
        var postA = context.Set<Post>().Single(e => e.Id == 1);
        var postB = context.Set<Post>().Where(e => e.Id == 1).Include(e => e.Blog).ToArray().Single();

        Assert.Same(postA, postB);

        Assert.Equal(EntityState.Unchanged, context.Entry(postA).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(postA.Blog).State);
    }

    [Fact] // Issue #4020
    public virtual void Include_brings_collections_referenced_from_already_tracked_notification_entities_as_Unchanged()
    {
        using var context = CreateContext();
        var blogA = context.Set<Blog>().Single(e => e.Id == 1);
        var blogB = context.Set<Blog>().Where(e => e.Id == 1).Include(e => e.Posts).ToArray().Single();

        Assert.Same(blogA, blogB);

        Assert.Equal(EntityState.Unchanged, context.Entry(blogA).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(blogA.Posts.First()).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(blogA.Posts.Skip(1).First()).State);
    }

    protected class Blog : NotificationEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public ICollection<Post> Posts
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;
    }

    protected class Post : NotificationEntity
    {
        public int Id
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public int PostId
        {
            get;
            set => SetWithNotify(value, ref field);
        }

        public Blog Blog
        {
            get;
            set => SetWithNotify(value, ref field);
        } = null!;
    }

    protected class NotificationEntity : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected void SetWithNotify<T>(T value, ref T field, [CallerMemberName] string propertyName = "")
        {
            if (!StructuralComparisons.StructuralEqualityComparer.Equals(field, value))
            {
                field = value;
                NotifyChanged(propertyName);
            }
        }
    }

    protected DbContext CreateContext()
        => Fixture.CreateContext();

    public abstract class NotificationEntitiesFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName
            => "NotificationEntities";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<Blog>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<Post>().Property(e => e.Id).ValueGeneratedNever();
        }

        protected override Task SeedAsync(PoolableDbContext context)
        {
            context.Add(
                new Blog { Id = 1, Posts = [new() { Id = 1 }, new() { Id = 2 }] });

            return context.SaveChangesAsync();
        }
    }
}
