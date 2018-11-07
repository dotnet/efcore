// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class NotificationEntitiesTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NotificationEntitiesTestBase<TFixture>.NotificationEntitiesFixtureBase, new()
    {
        protected NotificationEntitiesTestBase(TFixture fixture) => Fixture = fixture;

        protected virtual TFixture Fixture { get; }

        [Fact] // Issue #4020
        public virtual void Include_brings_entities_referenced_from_already_tracked_notification_entities_as_Unchanged()
        {
            using (var context = CreateContext())
            {
                var postA = context.Set<Post>().Single(e => e.Id == 1);
                var postB = context.Set<Post>().Where(e => e.Id == 1).Include(e => e.Blog).ToArray().Single();

                Assert.Same(postA, postB);

                Assert.Equal(EntityState.Unchanged, context.Entry(postA).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(postA.Blog).State);
            }
        }

        [Fact] // Issue #4020
        public virtual void Include_brings_collections_referenced_from_already_tracked_notification_entities_as_Unchanged()
        {
            using (var context = CreateContext())
            {
                var blogA = context.Set<Blog>().Single(e => e.Id == 1);
                var blogB = context.Set<Blog>().Where(e => e.Id == 1).Include(e => e.Posts).ToArray().Single();

                Assert.Same(blogA, blogB);

                Assert.Equal(EntityState.Unchanged, context.Entry(blogA).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(blogA.Posts.First()).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(blogA.Posts.Skip(1).First()).State);
            }
        }

        protected class Blog : NotificationEntity
        {
            private int _id;
            private ICollection<Post> _posts;

            public int Id
            {
                get => _id;
                set => SetWithNotify(value, ref _id);
            }

            public ICollection<Post> Posts
            {
                get => _posts;
                set => SetWithNotify(value, ref _posts);
            }
        }

        protected class Post : NotificationEntity
        {
            private int _id;
            private int _postId;
            private Blog _blog;

            public int Id
            {
                get => _id;
                set => SetWithNotify(value, ref _id);
            }

            public int PostId
            {
                get => _postId;
                set => SetWithNotify(value, ref _postId);
            }

            public Blog Blog
            {
                get => _blog;
                set => SetWithNotify(value, ref _blog);
            }
        }

        protected class NotificationEntity : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

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

        protected DbContext CreateContext() => Fixture.CreateContext();

        public abstract class NotificationEntitiesFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
        {
            protected override string StoreName { get; } = "NotificationEntities";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<Blog>().Property(e => e.Id).ValueGeneratedNever();
                modelBuilder.Entity<Post>().Property(e => e.Id).ValueGeneratedNever();
            }

            protected override void Seed(PoolableDbContext context)
            {
                context.Add(
                    new Blog
                    {
                        Id = 1,
                        Posts = new List<Post>
                        {
                            new Post
                            {
                                Id = 1
                            },
                            new Post
                            {
                                Id = 2
                            }
                        }
                    });

                context.SaveChanges();
            }
        }
    }
}
