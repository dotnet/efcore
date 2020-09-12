// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Utilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class ManyToManyLoadTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : ManyToManyLoadTestBase<TFixture>.ManyToManyLoadFixtureBase
    {
        protected ManyToManyLoadTestBase(TFixture fixture)
            => Fixture = fixture;

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
        [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
        [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
        [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
        [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
        [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
        [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
        [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
        [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
        [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
        [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
        [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
        [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
        [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
        [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
        [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
        [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
        [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
        public virtual async Task Load_collection(EntityState state, QueryTrackingBehavior queryTrackingBehavior, bool async)
        {
            using var context = Fixture.CreateContext();

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var left = context.Set<EntityOne>().Find(3);

            ClearLog();

            var collectionEntry = context.Entry(left).Collection(e => e.TwoSkip);

            context.Entry(left).State = state;

            Assert.False(collectionEntry.IsLoaded);

            if (ExpectLazyLoading)
            {
                Assert.Equal(7, left.TwoSkip.Count);
            }
            else
            {
                if (async)
                {
                    await collectionEntry.LoadAsync();
                }
                else
                {
                    collectionEntry.Load();
                }
            }

            Assert.True(collectionEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(7, left.TwoSkip.Count);
            foreach (var right in left.TwoSkip)
            {
                Assert.Contains(left, right.OneSkip);
            }

            Assert.Equal(1 + 7 + 7, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_collection_using_Query(EntityState state, bool async)
        {
            using var context = Fixture.CreateContext();

            var left = context.Set<EntityOne>().Find(3);

            ClearLog();

            var collectionEntry = context.Entry(left).Collection(e => e.TwoSkipShared);

            context.Entry(left).State = state;

            Assert.False(collectionEntry.IsLoaded);

            var children = async
                ? await collectionEntry.Query().ToListAsync()
                : collectionEntry.Query().ToList();

            Assert.False(collectionEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(3, left.TwoSkipShared.Count);
            foreach (var right in left.TwoSkipShared)
            {
                Assert.Contains(left, right.OneSkipShared);
            }

            Assert.Equal(children, left.TwoSkipShared.ToList());

            Assert.Equal(1 + 3 + 3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Added, false)]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Added, true)]
        public virtual void Attached_collections_are_not_marked_as_loaded(EntityState state, bool lazy)
        {
            using var context = Fixture.CreateContext();

            context.ChangeTracker.LazyLoadingEnabled = false;

            var left = ExpectLazyLoading
                ? context.CreateProxy<EntityOne>(
                    b =>
                    {
                        b.Id = 7776;
                        b.TwoSkip = new ObservableCollection<EntityTwo> { new EntityTwo { Id = 7777 } };
                        b.TwoSkipShared = new ObservableCollection<EntityTwo> { new EntityTwo { Id = 7778 } };
                        b.SelfSkipPayloadLeft = new ObservableCollection<EntityOne> { new EntityOne { Id = 7779 } };
                        b.SelfSkipPayloadRight = new ObservableCollection<EntityOne> { new EntityOne { Id = 7780 } };
                        b.BranchSkip = new ObservableCollection<EntityBranch> { new EntityBranch { Id = 7781 } };
                        b.ThreeSkipPayloadFull = new ObservableCollection<EntityThree> { new EntityThree { Id = 7782 } };
                        b.ThreeSkipPayloadFullShared = new ObservableCollection<EntityThree> { new EntityThree { Id = 7783 } };
                    })
                : new EntityOne
                {
                    Id = 7776,
                    TwoSkip = new List<EntityTwo> { new EntityTwo { Id = 7777 } },
                    TwoSkipShared = new List<EntityTwo> { new EntityTwo { Id = 7778 } },
                    SelfSkipPayloadLeft = new List<EntityOne> { new EntityOne { Id = 7779 } },
                    SelfSkipPayloadRight = new List<EntityOne> { new EntityOne { Id = 7780 } },
                    BranchSkip = new List<EntityBranch> { new EntityBranch { Id = 7781 } },
                    ThreeSkipPayloadFull = new List<EntityThree> { new EntityThree { Id = 7782 } },
                    ThreeSkipPayloadFullShared = new List<EntityThree> { new EntityThree { Id = 7783 } }
                };

            context.Attach(left);

            if (state != EntityState.Unchanged)
            {
                foreach (var child in left.TwoSkip.Cast<object>()
                    .Concat(left.TwoSkipShared)
                    .Concat(left.SelfSkipPayloadLeft)
                    .Concat(left.SelfSkipPayloadRight)
                    .Concat(left.BranchSkip)
                    .Concat(left.ThreeSkipPayloadFull)
                    .Concat(left.TwoSkipShared)
                    .Concat(left.ThreeSkipPayloadFullShared))
                {
                    context.Entry(child).State = state;
                }

                context.Entry(left).State = state;
            }

            context.ChangeTracker.LazyLoadingEnabled = true;

            Assert.False(context.Entry(left).Collection(e => e.TwoSkip).IsLoaded);
            Assert.False(context.Entry(left).Collection(e => e.TwoSkipShared).IsLoaded);
            Assert.False(context.Entry(left).Collection(e => e.SelfSkipPayloadLeft).IsLoaded);
            Assert.False(context.Entry(left).Collection(e => e.SelfSkipPayloadRight).IsLoaded);
            Assert.False(context.Entry(left).Collection(e => e.BranchSkip).IsLoaded);
            Assert.False(context.Entry(left).Collection(e => e.ThreeSkipPayloadFull).IsLoaded);
            Assert.False(context.Entry(left).Collection(e => e.ThreeSkipPayloadFullShared).IsLoaded);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_collection_already_loaded(EntityState state, bool async)
        {
            using var context = Fixture.CreateContext();

            var left = context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFull).Single(e => e.Id == 3);

            ClearLog();

            var collectionEntry = context.Entry(left).Collection(e => e.ThreeSkipPayloadFull);

            context.Entry(left).State = state;

            Assert.True(collectionEntry.IsLoaded);

            if (ExpectLazyLoading)
            {
                Assert.Equal(4, left.ThreeSkipPayloadFull.Count);
            }
            else
            {
                if (async)
                {
                    await collectionEntry.LoadAsync();
                }
                else
                {
                    collectionEntry.Load();
                }
            }

            Assert.True(collectionEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(4, left.ThreeSkipPayloadFull.Count);
            foreach (var right in left.ThreeSkipPayloadFull)
            {
                Assert.Contains(left, right.OneSkipPayloadFull);
            }

            Assert.Equal(1 + 4 + 4, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_collection_using_Query_already_loaded(EntityState state, bool async)
        {
            using var context = Fixture.CreateContext();

            var left = context.Set<EntityOne>().Include(e => e.TwoSkip).Single(e => e.Id == 3);

            ClearLog();

            var collectionEntry = context.Entry(left).Collection(e => e.TwoSkip);

            context.Entry(left).State = state;

            Assert.True(collectionEntry.IsLoaded);

            var children = async
                ? await collectionEntry.Query().ToListAsync()
                : collectionEntry.Query().ToList();

            Assert.True(collectionEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(7, left.TwoSkip.Count);
            foreach (var right in left.TwoSkip)
            {
                Assert.Contains(left, right.OneSkip);
            }

            Assert.Equal(children, left.TwoSkip.ToList());

            Assert.Equal(1 + 7 + 7, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_collection_untyped(EntityState state, bool async)
        {
            using var context = Fixture.CreateContext();

            var left = context.Set<EntityOne>().Find(3);

            ClearLog();

            var navigationEntry = context.Entry(left).Navigation("TwoSkip");

            context.Entry(left).State = state;

            Assert.False(navigationEntry.IsLoaded);

            if (ExpectLazyLoading)
            {
                Assert.Equal(7, left.TwoSkip.Count);
            }
            else
            {
                if (async)
                {
                    await navigationEntry.LoadAsync();
                }
                else
                {
                    navigationEntry.Load();
                }
            }

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(7, left.TwoSkip.Count);
            foreach (var right in left.TwoSkip)
            {
                Assert.Contains(left, right.OneSkip);
            }

            Assert.Equal(1 + 7 + 7, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_collection_using_Query_untyped(EntityState state, bool async)
        {
            using var context = Fixture.CreateContext();

            var left = context.Set<EntityOne>().Find(3);

            ClearLog();

            var collectionEntry = context.Entry(left).Navigation("TwoSkipShared");

            context.Entry(left).State = state;

            Assert.False(collectionEntry.IsLoaded);

            var children = async
                ? await collectionEntry.Query().ToListAsync<object>()
                : collectionEntry.Query().ToList<object>();

            Assert.False(collectionEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(3, left.TwoSkipShared.Count);
            foreach (var right in left.TwoSkipShared)
            {
                Assert.Contains(left, right.OneSkipShared);
            }

            Assert.Equal(children, left.TwoSkipShared.ToList());

            Assert.Equal(1 + 3 + 3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_collection_not_found_untyped(EntityState state, bool async)
        {
            using var context = Fixture.CreateContext();

            var left = context.Attach(
                ExpectLazyLoading
                    ? context.CreateProxy<EntityOne>(b => b.Id = 999)
                    : new EntityOne { Id = 999 }).Entity;

            ClearLog();

            var navigationEntry = context.Entry(left).Navigation("TwoSkip");

            context.Entry(left).State = state;

            Assert.False(navigationEntry.IsLoaded);

            if (ExpectLazyLoading)
            {
                Assert.Equal(0, left.TwoSkip.Count);
            }
            else
            {
                if (async)
                {
                    await navigationEntry.LoadAsync();
                }
                else
                {
                    navigationEntry.Load();
                }
            }

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Empty(left.TwoSkip);
            Assert.Single(context.ChangeTracker.Entries());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_collection_using_Query_not_found_untyped(EntityState state, bool async)
        {
            using var context = Fixture.CreateContext();

            var left = context.Attach(
                ExpectLazyLoading
                    ? context.CreateProxy<EntityOne>(b => b.Id = 999)
                    : new EntityOne { Id = 999 }).Entity;

            ClearLog();

            var navigationEntry = context.Entry(left).Navigation("TwoSkip");

            context.Entry(left).State = state;

            Assert.False(navigationEntry.IsLoaded);

            var children = async
                ? await navigationEntry.Query().ToListAsync<object>()
                : navigationEntry.Query().ToList<object>();

            Assert.False(navigationEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Empty(children);
            Assert.Empty(left.TwoSkip);

            Assert.Single(context.ChangeTracker.Entries());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Unchanged, false, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, false, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, false, CascadeTiming.OnSaveChanges)]
        public virtual async Task Load_collection_already_loaded_untyped(EntityState state, bool async, CascadeTiming deleteOrphansTiming)
        {
            using var context = Fixture.CreateContext();

            context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

            var left = context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFull).Single(e => e.Id == 3);

            ClearLog();

            var navigationEntry = context.Entry(left).Navigation("ThreeSkipPayloadFull");

            context.Entry(left).State = state;

            Assert.True(navigationEntry.IsLoaded);

            if (ExpectLazyLoading)
            {
                Assert.Equal(4, left.ThreeSkipPayloadFull.Count);
            }
            else
            {
                if (async)
                {
                    await navigationEntry.LoadAsync();
                }
                else
                {
                    navigationEntry.Load();
                }
            }

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(4, left.ThreeSkipPayloadFull.Count);
            foreach (var right in left.ThreeSkipPayloadFull)
            {
                Assert.Contains(left, right.OneSkipPayloadFull);
            }

            Assert.Equal(1 + 4 + 4, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Unchanged, false, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, false, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, false, CascadeTiming.OnSaveChanges)]
        public virtual async Task Load_collection_using_Query_already_loaded_untyped(
            EntityState state,
            bool async,
            CascadeTiming deleteOrphansTiming)
        {
            using var context = Fixture.CreateContext();

            context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

            var left = context.Set<EntityOne>().Include(e => e.TwoSkip).Single(e => e.Id == 3);

            ClearLog();

            var navigationEntry = context.Entry(left).Navigation("TwoSkip");

            context.Entry(left).State = state;

            Assert.True(navigationEntry.IsLoaded);

            // Issue #16429
            var children = async
                ? await navigationEntry.Query().ToListAsync<object>()
                : navigationEntry.Query().ToList<object>();

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(7, left.TwoSkip.Count);
            foreach (var right in left.TwoSkip)
            {
                Assert.Contains(left, right.OneSkip);
            }

            Assert.Equal(children, left.TwoSkip.ToList());

            Assert.Equal(1 + 7 + 7, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_collection_composite_key(EntityState state, bool async)
        {
            using var context = Fixture.CreateContext();

            var left = context.Set<EntityCompositeKey>().Find(7, "7_2", new DateTime(2007, 2, 1));

            ClearLog();

            var collectionEntry = context.Entry(left).Collection(e => e.ThreeSkipFull);

            context.Entry(left).State = state;

            Assert.False(collectionEntry.IsLoaded);

            if (ExpectLazyLoading)
            {
                Assert.Equal(2, left.ThreeSkipFull.Count);
            }
            else
            {
                if (async)
                {
                    await collectionEntry.LoadAsync();
                }
                else
                {
                    collectionEntry.Load();
                }
            }

            Assert.True(collectionEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, left.ThreeSkipFull.Count);
            foreach (var right in left.ThreeSkipFull)
            {
                Assert.Contains(left, right.CompositeKeySkipFull);
            }

            Assert.Equal(1 + 2 + 2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_collection_using_Query_composite_key(EntityState state, bool async)
        {
            using var context = Fixture.CreateContext();

            var left = context.Set<EntityCompositeKey>().Find(7, "7_2", new DateTime(2007, 2, 1));

            ClearLog();

            var collectionEntry = context.Entry(left).Collection(e => e.ThreeSkipFull);

            context.Entry(left).State = state;

            Assert.False(collectionEntry.IsLoaded);

            var children = async
                ? await collectionEntry.Query().ToListAsync()
                : collectionEntry.Query().ToList();

            Assert.False(collectionEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, left.ThreeSkipFull.Count);
            foreach (var right in left.ThreeSkipFull)
            {
                Assert.Contains(left, right.CompositeKeySkipFull);
            }

            Assert.Equal(children, left.ThreeSkipFull.ToList());

            Assert.Equal(1 + 2 + 2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(true, QueryTrackingBehavior.NoTracking)]
        [InlineData(true, QueryTrackingBehavior.TrackAll)]
        [InlineData(true, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
        [InlineData(false, QueryTrackingBehavior.NoTracking)]
        [InlineData(false, QueryTrackingBehavior.TrackAll)]
        [InlineData(false, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
        public virtual async Task Load_collection_for_detached_throws(bool async, QueryTrackingBehavior queryTrackingBehavior)
        {
            using var context = Fixture.CreateContext();

            var left = context.Set<EntityOne>().AsTracking(queryTrackingBehavior).Single(e => e.Id == 3);

            var collectionEntry = context.Entry(left).Collection(e => e.TwoSkip);

            if (queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                context.Entry(left).State = EntityState.Detached;
            }

            Assert.Equal(
                CoreStrings.CannotLoadDetached(nameof(left.TwoSkip), nameof(EntityOne)),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    async () =>
                    {
                        if (async)
                        {
                            await collectionEntry.LoadAsync();
                        }
                        else
                        {
                            collectionEntry.Load();
                        }
                    })).Message);
        }

        [ConditionalTheory]
        [InlineData(QueryTrackingBehavior.NoTracking)]
        [InlineData(QueryTrackingBehavior.TrackAll)]
        [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
        public virtual void Query_collection_for_detached_throws(QueryTrackingBehavior queryTrackingBehavior)
        {
            using var context = Fixture.CreateContext();

            var left = context.Set<EntityOne>().AsTracking(queryTrackingBehavior).Single(e => e.Id == 3);

            var collectionEntry = context.Entry(left).Collection(e => e.TwoSkip);

            if (queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                context.Entry(left).State = EntityState.Detached;
            }

            Assert.Equal(
                CoreStrings.CannotLoadDetached(nameof(left.TwoSkip), nameof(EntityOne)),
                Assert.Throws<InvalidOperationException>(() => collectionEntry.Query()).Message);
        }

        protected virtual void ClearLog()
        {
        }

        protected virtual void RecordLog()
        {
        }

        protected TFixture Fixture { get; }

        protected virtual bool ExpectLazyLoading
            => false;

        public abstract class ManyToManyLoadFixtureBase : ManyToManyQueryFixtureBase
        {
            protected override string StoreName { get; } = "ManyToManyLoadTest";
        }
    }
}
