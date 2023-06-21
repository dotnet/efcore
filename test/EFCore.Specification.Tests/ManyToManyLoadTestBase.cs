// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore;

public abstract partial class ManyToManyLoadTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : ManyToManyLoadTestBase<TFixture>.ManyToManyLoadFixtureBase
{
    protected ManyToManyLoadTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    public virtual async Task Load_collection(EntityState state, QueryTrackingBehavior queryTrackingBehavior, bool async)
    {
        using var context = Fixture.CreateContext();

        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;

        var left = context.Set<EntityOne>().Find(3)!;

        ClearLog();

        var collectionEntry = context.Entry(left).Collection(e => e.TwoSkip);

        SetState(context, left, state, queryTrackingBehavior);

        Assert.False(collectionEntry.IsLoaded);

        if (ExpectLazyLoading
            && state == EntityState.Detached
            && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
        {
            Assert.Null(left.TwoSkip);
        }
        else
        {
            if (ExpectLazyLoading)
            {
                Assert.Equal(7, left.TwoSkip.Count);
            }
            else
            {
                Assert.Null(left.TwoSkip);
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
            foreach (var entityTwo in left.TwoSkip!)
            {
                Assert.False(context.Entry(entityTwo).Collection(e => e.OneSkip).IsLoaded);
            }

            RecordLog();

            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(7, left.TwoSkip.Count);
            foreach (var right in left.TwoSkip)
            {
                Assert.Contains(left, right.OneSkip);
            }
        }

        Assert.Equal(state == EntityState.Detached ? 0 : 1 + 7 + 7, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Detached, true)]
    [InlineData(EntityState.Detached, false)]
    public virtual async Task Load_collection_using_Query(EntityState state, bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<EntityOne>().Find(3)!;

        ClearLog();

        var collectionEntry = context.Entry(left).Collection(e => e.TwoSkipShared);

        SetState(context, left, state);

        Assert.False(collectionEntry.IsLoaded);

        var children = async
            ? await collectionEntry.Query().ToListAsync()
            : collectionEntry.Query().ToList();

        Assert.False(collectionEntry.IsLoaded);
        foreach (var entityTwo in left.TwoSkipShared)
        {
            Assert.False(context.Entry(entityTwo).Collection(e => e.OneSkipShared).IsLoaded);
        }

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 1 + 3 + 3, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Detached)
        {
            Assert.Equal(3, left.TwoSkipShared.Count);
            foreach (var right in left.TwoSkipShared)
            {
                Assert.Contains(left, right.OneSkipShared);
            }

            Assert.Equal(children, left.TwoSkipShared.ToList());
        }
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
                    b.TwoSkip = new ObservableCollection<EntityTwo> { new() { Id = 7777 } };
                    b.TwoSkipShared = new ObservableCollection<EntityTwo> { new() { Id = 7778 } };
                    b.SelfSkipPayloadLeft = new ObservableCollection<EntityOne> { new() { Id = 7779 } };
                    b.SelfSkipPayloadRight = new ObservableCollection<EntityOne> { new() { Id = 7780 } };
                    b.BranchSkip = new ObservableCollection<EntityBranch> { new() { Id = 7781 } };
                    b.ThreeSkipPayloadFull = new ObservableCollection<EntityThree> { new() { Id = 7782 } };
                    b.ThreeSkipPayloadFullShared = new ObservableCollection<EntityThree> { new() { Id = 7783 } };
                })
            : new EntityOne
            {
                Id = 7776,
                TwoSkip = new List<EntityTwo> { new() { Id = 7777 } },
                TwoSkipShared = new List<EntityTwo> { new() { Id = 7778 } },
                SelfSkipPayloadLeft = new List<EntityOne> { new() { Id = 7779 } },
                SelfSkipPayloadRight = new List<EntityOne> { new() { Id = 7780 } },
                BranchSkip = new List<EntityBranch> { new() { Id = 7781 } },
                ThreeSkipPayloadFull = new List<EntityThree> { new() { Id = 7782 } },
                ThreeSkipPayloadFullShared = new List<EntityThree> { new() { Id = 7783 } }
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
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Detached, true)]
    [InlineData(EntityState.Detached, false)]
    public virtual async Task Load_collection_already_loaded(EntityState state, bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFull).Single(e => e.Id == 3);

        ClearLog();

        var collectionEntry = context.Entry(left).Collection(e => e.ThreeSkipPayloadFull);

        foreach (var two in left.ThreeSkipPayloadFull)
        {
            SetState(context, two, state);
        }

        SetState(context, left, state);

        Assert.True(collectionEntry.IsLoaded);

        if (ExpectLazyLoading)
        {
            Assert.Equal(4, left.ThreeSkipPayloadFull.Count);
        }
        else
        {
            Assert.Equal(4, left.ThreeSkipPayloadFull.Count);
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
        foreach (var entityTwo in left.ThreeSkipPayloadFull)
        {
            Assert.False(context.Entry(entityTwo).Collection(e => e.OneSkipPayloadFull).IsLoaded);
        }

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(4, left.ThreeSkipPayloadFull.Count);
        foreach (var right in left.ThreeSkipPayloadFull)
        {
            Assert.Contains(left, right.OneSkipPayloadFull);
        }

        Assert.Equal(state == EntityState.Detached ? 0 : 1 + 4 + 4, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Detached, true)]
    [InlineData(EntityState.Detached, false)]
    public virtual async Task Load_collection_using_Query_already_loaded(EntityState state, bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<EntityOne>().Include(e => e.TwoSkip).Single(e => e.Id == 3);

        ClearLog();

        var collectionEntry = context.Entry(left).Collection(e => e.TwoSkip);

        foreach (var two in left.TwoSkip)
        {
            SetState(context, two, state);
        }

        SetState(context, left, state);

        Assert.True(collectionEntry.IsLoaded);

        var children = async
            ? await collectionEntry.Query().ToListAsync()
            : collectionEntry.Query().ToList();

        Assert.True(collectionEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;
        foreach (var entityTwo in left.TwoSkip)
        {
            Assert.False(context.Entry(entityTwo).Collection(e => e.OneSkip).IsLoaded);
        }

        Assert.Equal(7, left.TwoSkip.Count);
        foreach (var right in left.TwoSkip)
        {
            Assert.Contains(left, right.OneSkip);
        }

        if (state == EntityState.Detached)
        {
            Assert.NotEqual(children, left.TwoSkip.ToList());
        }
        else
        {
            Assert.Equal(children, left.TwoSkip.ToList());
        }

        Assert.Equal(state == EntityState.Detached ? 0 : 1 + 7 + 7, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, false, true)]
    [InlineData(EntityState.Unchanged, false, false)]
    [InlineData(EntityState.Added, false, true)]
    [InlineData(EntityState.Added, false, false)]
    [InlineData(EntityState.Modified, false, true)]
    [InlineData(EntityState.Modified, false, false)]
    [InlineData(EntityState.Deleted, false, true)]
    [InlineData(EntityState.Deleted, false, false)]
    [InlineData(EntityState.Detached, false, true)]
    [InlineData(EntityState.Detached, false, false)]
    [InlineData(EntityState.Unchanged, true, true)]
    [InlineData(EntityState.Unchanged, true, false)]
    [InlineData(EntityState.Added, true, true)]
    [InlineData(EntityState.Added, true, false)]
    [InlineData(EntityState.Modified, true, true)]
    [InlineData(EntityState.Modified, true, false)]
    [InlineData(EntityState.Deleted, true, true)]
    [InlineData(EntityState.Deleted, true, false)]
    [InlineData(EntityState.Detached, true, true)]
    [InlineData(EntityState.Detached, true, false)]
    public virtual async Task Load_collection_partially_loaded(EntityState state, bool forceIdentityResolution, bool async)
    {
        using var context = Fixture.CreateContext();

        context.ChangeTracker.LazyLoadingEnabled = false;

        var left = context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFull.OrderBy(e => e.Id).Take(1)).Single(e => e.Id == 3);

        ClearLog();

        var collectionEntry = context.Entry(left).Collection(e => e.ThreeSkipPayloadFull);

        foreach (var three in left.ThreeSkipPayloadFull)
        {
            SetState(context, three, state);
        }

        SetState(context, left, state);

        collectionEntry.IsLoaded = false;

        context.ChangeTracker.LazyLoadingEnabled = true;

        if (ExpectLazyLoading)
        {
            if (state == EntityState.Detached) // Explicitly detached
            {
                Assert.Equal(1, left.ThreeSkipPayloadFull.Count);
                Assert.False(collectionEntry.IsLoaded);
                Assert.Empty(context.ChangeTracker.Entries());
            }
            else
            {
                Assert.Equal(4, left.ThreeSkipPayloadFull.Count);
                Assert.True(collectionEntry.IsLoaded);

                context.ChangeTracker.LazyLoadingEnabled = false;
                foreach (var right in left.ThreeSkipPayloadFull)
                {
                    Assert.Contains(left, right.OneSkipPayloadFull);
                }

                Assert.Equal(1 + 4 + 4, context.ChangeTracker.Entries().Count());
            }
        }
        else
        {
            if (async)
            {
                await collectionEntry.LoadAsync(forceIdentityResolution ? LoadOptions.ForceIdentityResolution : LoadOptions.None);
            }
            else
            {
                collectionEntry.Load(forceIdentityResolution ? LoadOptions.ForceIdentityResolution : LoadOptions.None);
            }

            Assert.True(collectionEntry.IsLoaded);

            foreach (var entityTwo in left.ThreeSkipPayloadFull)
            {
                Assert.False(context.Entry(entityTwo).Collection(e => e.OneSkipPayloadFull).IsLoaded);
            }

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached && !forceIdentityResolution ? 5 : 4, left.ThreeSkipPayloadFull.Count);
            foreach (var right in left.ThreeSkipPayloadFull)
            {
                Assert.Contains(left, right.OneSkipPayloadFull);
            }

            Assert.Equal(state == EntityState.Detached ? 0 : 1 + 4 + 4, context.ChangeTracker.Entries().Count());
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, false, true)]
    [InlineData(EntityState.Unchanged, false, false)]
    [InlineData(EntityState.Added, false, true)]
    [InlineData(EntityState.Added, false, false)]
    [InlineData(EntityState.Modified, false, true)]
    [InlineData(EntityState.Modified, false, false)]
    [InlineData(EntityState.Deleted, false, true)]
    [InlineData(EntityState.Deleted, false, false)]
    [InlineData(EntityState.Detached, false, true)]
    [InlineData(EntityState.Detached, false, false)]
    [InlineData(EntityState.Unchanged, true, true)]
    [InlineData(EntityState.Unchanged, true, false)]
    [InlineData(EntityState.Added, true, true)]
    [InlineData(EntityState.Added, true, false)]
    [InlineData(EntityState.Modified, true, true)]
    [InlineData(EntityState.Modified, true, false)]
    [InlineData(EntityState.Deleted, true, true)]
    [InlineData(EntityState.Deleted, true, false)]
    [InlineData(EntityState.Detached, true, true)]
    [InlineData(EntityState.Detached, true, false)]
    public virtual async Task Load_collection_partially_loaded_no_explicit_join(EntityState state, bool forceIdentityResolution, bool async)
    {
        using var context = Fixture.CreateContext();

        context.ChangeTracker.LazyLoadingEnabled = false;

        var left = context.Set<EntityOne>().Include(e => e.TwoSkip.OrderBy(e => e.Id).Take(1)).Single(e => e.Id == 3);

        ClearLog();

        var collectionEntry = context.Entry(left).Collection(e => e.TwoSkip);

        foreach (var three in left.TwoSkip)
        {
            SetState(context, three, state);
        }

        SetState(context, left, state);

        collectionEntry.IsLoaded = false;

        context.ChangeTracker.LazyLoadingEnabled = true;

        if (ExpectLazyLoading)
        {
            if (state == EntityState.Detached) // Explicitly detached
            {
                Assert.Equal(1, left.TwoSkip.Count);
                Assert.False(collectionEntry.IsLoaded);
                Assert.Empty(context.ChangeTracker.Entries());
            }
            else
            {
                Assert.Equal(7, left.TwoSkip.Count);
                Assert.True(collectionEntry.IsLoaded);

                context.ChangeTracker.LazyLoadingEnabled = false;
                foreach (var right in left.TwoSkip)
                {
                    Assert.Contains(left, right.OneSkip);
                }

                Assert.Equal(1 + 7 + 7, context.ChangeTracker.Entries().Count());
            }
        }
        else
        {
            if (async)
            {
                await collectionEntry.LoadAsync(forceIdentityResolution ? LoadOptions.ForceIdentityResolution : LoadOptions.None);
            }
            else
            {
                collectionEntry.Load(forceIdentityResolution ? LoadOptions.ForceIdentityResolution : LoadOptions.None);
            }

            Assert.True(collectionEntry.IsLoaded);

            foreach (var entityTwo in left.TwoSkip)
            {
                Assert.False(context.Entry(entityTwo).Collection(e => e.OneSkip).IsLoaded);
            }

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached && !forceIdentityResolution ? 8 : 7, left.TwoSkip.Count);
            foreach (var right in left.TwoSkip)
            {
                Assert.Contains(left, right.OneSkip);
            }

            Assert.Equal(state == EntityState.Detached ? 0 : 1 + 7 + 7, context.ChangeTracker.Entries().Count());
        }
    }

    [ConditionalTheory]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Load_collection_partially_loaded_no_tracking(QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = Fixture.CreateContext();

        context.ChangeTracker.LazyLoadingEnabled = false;
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;

        var left = context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFull.OrderBy(e => e.Id).Take(1)).Single(e => e.Id == 3);

        ClearLog();

        var collectionEntry = context.Entry(left).Collection(e => e.ThreeSkipPayloadFull);
        collectionEntry.IsLoaded = false;

        context.ChangeTracker.LazyLoadingEnabled = true;

        if (ExpectLazyLoading)
        {
            Assert.Equal(queryTrackingBehavior == QueryTrackingBehavior.NoTracking ? 5 : 4, left.ThreeSkipPayloadFull.Count);
        }
        else
        {
            Assert.Single(left.ThreeSkipPayloadFull);
            collectionEntry.Load(
                queryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution
                    ? LoadOptions.ForceIdentityResolution
                    : LoadOptions.None);
        }

        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.True(collectionEntry.IsLoaded);

        foreach (var entityTwo in left.ThreeSkipPayloadFull)
        {
            Assert.False(context.Entry(entityTwo).Collection(e => e.OneSkipPayloadFull).IsLoaded);
        }

        RecordLog();

        Assert.Equal(queryTrackingBehavior == QueryTrackingBehavior.NoTracking ? 5 : 4, left.ThreeSkipPayloadFull.Count);
        foreach (var right in left.ThreeSkipPayloadFull)
        {
            Assert.Contains(left, right.OneSkipPayloadFull);
        }

        Assert.Empty(context.ChangeTracker.Entries());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Detached, true)]
    [InlineData(EntityState.Detached, false)]
    public virtual async Task Load_collection_untyped(EntityState state, bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<EntityOne>().Find(3)!;

        ClearLog();

        var navigationEntry = context.Entry(left).Navigation("TwoSkip");

        SetState(context, left, state);

        Assert.False(navigationEntry.IsLoaded);

        if (ExpectLazyLoading && state == EntityState.Detached)
        {
            Assert.Null(left.TwoSkip);
        }
        else
        {
            if (ExpectLazyLoading)
            {
                Assert.Equal(7, left.TwoSkip.Count);
            }
            else
            {
                Assert.Null(left.TwoSkip);
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
            foreach (var entityTwo in left.TwoSkip!)
            {
                Assert.False(context.Entry((object)entityTwo).Collection("OneSkip").IsLoaded);
            }

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(7, left.TwoSkip.Count);
            foreach (var right in left.TwoSkip)
            {
                Assert.Contains(left, right.OneSkip);
            }
        }

        Assert.Equal(state == EntityState.Detached ? 0 : 1 + 7 + 7, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Detached, true)]
    [InlineData(EntityState.Detached, false)]
    public virtual async Task Load_collection_using_Query_untyped(EntityState state, bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<EntityOne>().Find(3)!;

        ClearLog();

        var collectionEntry = context.Entry(left).Navigation("TwoSkipShared");

        SetState(context, left, state);

        Assert.False(collectionEntry.IsLoaded);

        var children = async
            ? await collectionEntry.Query().ToListAsync<object>()
            : collectionEntry.Query().ToList<object>();

        Assert.False(collectionEntry.IsLoaded);
        foreach (var entityTwo in left.TwoSkipShared)
        {
            Assert.False(context.Entry((object)entityTwo).Collection("OneSkipShared").IsLoaded);
        }

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 1 + 3 + 3, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Detached)
        {
            Assert.Equal(3, left.TwoSkipShared.Count);
            foreach (var right in left.TwoSkipShared)
            {
                Assert.Contains(left, right.OneSkipShared);
            }

            Assert.Equal(children, left.TwoSkipShared.ToList());
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Detached, true)]
    [InlineData(EntityState.Detached, false)]
    public virtual async Task Load_collection_not_found_untyped(EntityState state, bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Attach(
            ExpectLazyLoading
                ? context.CreateProxy<EntityOne>(b => b.Id = 999)
                : new EntityOne { Id = 999 }).Entity;

        ClearLog();

        var navigationEntry = context.Entry(left).Navigation("TwoSkip");

        SetState(context, left, state);

        Assert.False(navigationEntry.IsLoaded);

        if (ExpectLazyLoading && state == EntityState.Detached)
        {
            Assert.Null(left.TwoSkip);
        }
        else
        {
            if (ExpectLazyLoading)
            {
                Assert.Equal(0, left.TwoSkip.Count);
            }
            else
            {
                Assert.Null(left.TwoSkip);
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

            Assert.Empty(left.TwoSkip!);
        }

        Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Detached, true)]
    [InlineData(EntityState.Detached, false)]
    public virtual async Task Load_collection_using_Query_not_found_untyped(EntityState state, bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Attach(
            ExpectLazyLoading
                ? context.CreateProxy<EntityOne>(b => b.Id = 999)
                : new EntityOne { Id = 999 }).Entity;

        ClearLog();

        var navigationEntry = context.Entry(left).Navigation("TwoSkip");

        SetState(context, left, state);

        Assert.False(navigationEntry.IsLoaded);

        var children = async
            ? await navigationEntry.Query().ToListAsync<object>()
            : navigationEntry.Query().ToList<object>();

        Assert.False(navigationEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Empty(children);
        Assert.Empty(left.TwoSkip);

        Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Added, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Added, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Detached, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Detached, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Unchanged, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Added, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Added, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Modified, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Modified, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Deleted, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Deleted, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Detached, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Detached, false, CascadeTiming.OnSaveChanges)]
    public virtual async Task Load_collection_already_loaded_untyped(EntityState state, bool async, CascadeTiming deleteOrphansTiming)
    {
        using var context = Fixture.CreateContext();

        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

        var left = context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFull).Single(e => e.Id == 3);

        ClearLog();

        var navigationEntry = context.Entry(left).Navigation("ThreeSkipPayloadFull");

        foreach (var two in left.ThreeSkipPayloadFull)
        {
            SetState(context, two, state);
        }

        SetState(context, left, state);

        Assert.True(navigationEntry.IsLoaded);

        if (ExpectLazyLoading)
        {
            Assert.Equal(4, left.ThreeSkipPayloadFull.Count);
        }
        else
        {
            Assert.Equal(4, left.ThreeSkipPayloadFull.Count);
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
        foreach (var entityTwo in left.ThreeSkipPayloadFull)
        {
            Assert.False(context.Entry((object)entityTwo).Collection("OneSkipPayloadFull").IsLoaded);
        }

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(4, left.ThreeSkipPayloadFull.Count);
        foreach (var right in left.ThreeSkipPayloadFull)
        {
            Assert.Contains(left, right.OneSkipPayloadFull);
        }

        Assert.Equal(state == EntityState.Detached ? 0 : 1 + 4 + 4, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Added, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Added, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Detached, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Detached, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Unchanged, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Added, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Added, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Modified, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Modified, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Deleted, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Deleted, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Detached, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Detached, false, CascadeTiming.OnSaveChanges)]
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

        foreach (var two in left.TwoSkip)
        {
            SetState(context, two, state);
        }

        SetState(context, left, state);

        Assert.True(navigationEntry.IsLoaded);

        // Issue #16429
        var children = async
            ? await navigationEntry.Query().ToListAsync<object>()
            : navigationEntry.Query().ToList<object>();

        Assert.True(navigationEntry.IsLoaded);
        foreach (var entityTwo in left.TwoSkip)
        {
            Assert.False(context.Entry((object)entityTwo).Collection("OneSkip").IsLoaded);
        }

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(7, left.TwoSkip.Count);
        foreach (var right in left.TwoSkip)
        {
            Assert.Contains(left, right.OneSkip);
        }

        if (state == EntityState.Detached)
        {
            Assert.NotEqual(children, left.TwoSkip.ToList());
        }
        else
        {
            Assert.Equal(children, left.TwoSkip.ToList());
        }

        Assert.Equal(state == EntityState.Detached ? 0 : 1 + 7 + 7, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Detached, true)]
    [InlineData(EntityState.Detached, false)]
    public virtual async Task Load_collection_composite_key(EntityState state, bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<EntityCompositeKey>().Find(7, "7_2", new DateTime(2007, 2, 1))!;

        ClearLog();

        var collectionEntry = context.Entry(left).Collection(e => e.ThreeSkipFull);

        SetState(context, left, state);

        Assert.False(collectionEntry.IsLoaded);

        if (ExpectLazyLoading && state == EntityState.Detached)
        {
            Assert.Null(left.ThreeSkipFull);
        }
        else
        {
            if (ExpectLazyLoading)
            {
                Assert.Equal(2, left.ThreeSkipFull.Count);
            }
            else
            {
                Assert.Null(left.ThreeSkipFull);
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
            foreach (var entityTwo in left.ThreeSkipFull!)
            {
                Assert.False(context.Entry(entityTwo).Collection(e => e.CompositeKeySkipFull).IsLoaded);
            }

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, left.ThreeSkipFull.Count);
            foreach (var right in left.ThreeSkipFull)
            {
                Assert.Contains(left, right.CompositeKeySkipFull);
            }
        }

        Assert.Equal(state == EntityState.Detached ? 0 : 1 + 2 + 2, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Detached, true)]
    [InlineData(EntityState.Detached, false)]
    public virtual async Task Load_collection_using_Query_composite_key(EntityState state, bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<EntityCompositeKey>().Find(7, "7_2", new DateTime(2007, 2, 1))!;

        ClearLog();

        var collectionEntry = context.Entry(left).Collection(e => e.ThreeSkipFull);

        SetState(context, left, state);

        Assert.False(collectionEntry.IsLoaded);

        var children = async
            ? await collectionEntry.Query().ToListAsync()
            : collectionEntry.Query().ToList();

        Assert.False(collectionEntry.IsLoaded);
        foreach (var entityTwo in left.ThreeSkipFull)
        {
            Assert.False(context.Entry(entityTwo).Collection(e => e.CompositeKeySkipFull).IsLoaded);
        }

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 1 + 2 + 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Detached)
        {
            Assert.Equal(2, left.ThreeSkipFull.Count);
            foreach (var right in left.ThreeSkipFull)
            {
                Assert.Contains(left, right.CompositeKeySkipFull);
            }

            Assert.Equal(children, left.ThreeSkipFull.ToList());
        }
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

        if (async)
        {
            await collectionEntry.LoadAsync();
        }
        else
        {
            collectionEntry.Load();
        }
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

        var query = collectionEntry.Query();
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Load_collection_using_Query_with_Include(bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<EntityOne>().Find(3)!;

        ClearLog();

        var collectionEntry = context.Entry(left).Collection(e => e.TwoSkipShared);

        Assert.False(collectionEntry.IsLoaded);

        var children = async
            ? await collectionEntry.Query().Include(e => e.ThreeSkipFull).ToListAsync()
            : collectionEntry.Query().Include(e => e.ThreeSkipFull).ToList();

        Assert.False(collectionEntry.IsLoaded);
        foreach (var entityTwo in left.TwoSkipShared)
        {
            Assert.False(context.Entry(entityTwo).Collection(e => e.OneSkipShared).IsLoaded);
            Assert.True(context.Entry(entityTwo).Collection(e => e.ThreeSkipFull).IsLoaded);

            foreach (var entityThree in entityTwo.ThreeSkipFull)
            {
                Assert.False(context.Entry(entityThree).Collection(e => e.TwoSkipFull).IsLoaded);
            }
        }

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(3, left.TwoSkipShared.Count);
        foreach (var right in left.TwoSkipShared)
        {
            Assert.Contains(left, right.OneSkipShared);
            foreach (var three in right.ThreeSkipFull)
            {
                Assert.Contains(right, three.TwoSkipFull);
            }
        }

        Assert.Equal(children, left.TwoSkipShared.ToList());

        Assert.Equal(21, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Load_collection_using_Query_with_Include_for_inverse(bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<EntityOne>().Find(3)!;

        ClearLog();

        var collectionEntry = context.Entry(left).Collection(e => e.TwoSkipShared);

        Assert.False(collectionEntry.IsLoaded);

        var queryable = collectionEntry.Query().Include(e => e.OneSkipShared);
        var children = async
            ? await queryable.ToListAsync()
            : queryable.ToList();

        Assert.False(collectionEntry.IsLoaded);
        foreach (var entityTwo in left.TwoSkipShared)
        {
            Assert.True(context.Entry(entityTwo).Collection(e => e.OneSkipShared).IsLoaded);
        }

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(3, left.TwoSkipShared.Count);
        foreach (var right in left.TwoSkipShared)
        {
            Assert.Contains(left, right.OneSkipShared);
        }

        Assert.Equal(children, left.TwoSkipShared.ToList());
        Assert.Equal(7, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Load_collection_using_Query_with_Include_for_same_collection(bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<EntityOne>().Find(3)!;

        ClearLog();

        var collectionEntry = context.Entry(left).Collection(e => e.TwoSkipShared);

        Assert.False(collectionEntry.IsLoaded);

        var queryable = collectionEntry.Query().Include(e => e.OneSkipShared).ThenInclude(e => e.TwoSkipShared);
        var children = async
            ? await queryable.ToListAsync()
            : queryable.ToList();

        Assert.True(collectionEntry.IsLoaded);
        foreach (var entityTwo in left.TwoSkipShared)
        {
            Assert.True(context.Entry(entityTwo).Collection(e => e.OneSkipShared).IsLoaded);
        }

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(3, left.TwoSkipShared.Count);
        foreach (var right in left.TwoSkipShared)
        {
            Assert.Contains(left, right.OneSkipShared);
        }

        Assert.Equal(children, left.TwoSkipShared.ToList());
        Assert.Equal(7, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Load_collection_using_Query_with_filtered_Include(bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<EntityOne>().Find(3)!;

        ClearLog();

        var collectionEntry = context.Entry(left).Collection(e => e.TwoSkipShared);

        Assert.False(collectionEntry.IsLoaded);

        var children = async
            ? await collectionEntry.Query().Include(e => e.ThreeSkipFull.Where(e => e.Id == 13 || e.Id == 11)).ToListAsync()
            : collectionEntry.Query().Include(e => e.ThreeSkipFull.Where(e => e.Id == 13 || e.Id == 11)).ToList();

        Assert.False(collectionEntry.IsLoaded);
        foreach (var entityTwo in left.TwoSkipShared)
        {
            Assert.False(context.Entry(entityTwo).Collection(e => e.OneSkipShared).IsLoaded);
            Assert.True(context.Entry(entityTwo).Collection(e => e.ThreeSkipFull).IsLoaded);

            foreach (var entityThree in entityTwo.ThreeSkipFull)
            {
                Assert.False(context.Entry(entityThree).Collection(e => e.TwoSkipFull).IsLoaded);
            }
        }

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(3, left.TwoSkipShared.Count);
        foreach (var right in left.TwoSkipShared)
        {
            Assert.Contains(left, right.OneSkipShared);
            foreach (var three in right.ThreeSkipFull)
            {
                Assert.True(three.Id is 11 or 13);
                Assert.Contains(right, three.TwoSkipFull);
            }
        }

        Assert.Equal(children, left.TwoSkipShared.ToList());

        Assert.Equal(9, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Load_collection_using_Query_with_filtered_Include_and_projection(bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<EntityOne>().Find(3)!;

        ClearLog();

        var collectionEntry = context.Entry(left).Collection(e => e.TwoSkipShared);

        Assert.False(collectionEntry.IsLoaded);

        var queryable = collectionEntry
            .Query()
            .Include(e => e.ThreeSkipFull.Where(e => e.Id == 13 || e.Id == 11))
            .OrderBy(e => e.Id)
            .Select(
                e => new
                {
                    e.Id,
                    e.Name,
                    Count1 = e.OneSkipShared.Count,
                    Count3 = e.ThreeSkipFull.Count
                });

        var projected = async
            ? await queryable.ToListAsync()
            : queryable.ToList();

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;
        Assert.False(collectionEntry.IsLoaded);
        Assert.Empty(left.TwoSkipShared);
        Assert.Single(context.ChangeTracker.Entries());

        Assert.Equal(3, projected.Count);

        Assert.Equal(10, projected[0].Id);
        Assert.Equal("EntityTwo 10", projected[0].Name);
        Assert.Equal(3, projected[0].Count1);
        Assert.Equal(1, projected[0].Count3);

        Assert.Equal(11, projected[1].Id);
        Assert.Equal("EntityTwo 11", projected[1].Name);
        Assert.Equal(2, projected[1].Count1);
        Assert.Equal(4, projected[1].Count3);

        Assert.Equal(16, projected[2].Id);
        Assert.Equal("EntityTwo 16", projected[2].Name);
        Assert.Equal(3, projected[2].Count1);
        Assert.Equal(2, projected[2].Count3);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Load_collection_using_Query_with_join(bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<EntityOne>().Find(3)!;

        ClearLog();

        var collectionEntry = context.Entry(left).Collection(e => e.TwoSkipShared);

        Assert.False(collectionEntry.IsLoaded);

        var queryable = from t in collectionEntry.Query()
                        join s in context.Set<EntityOne>().SelectMany(e => e.TwoSkipShared)
                            on t.Id equals s.Id
                        select new { t, s };

        var projected = async
            ? await queryable.ToListAsync()
            : queryable.ToList();

        Assert.False(collectionEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(7, context.ChangeTracker.Entries().Count());
        Assert.Equal(8, projected.Count);

        foreach (var pair in projected)
        {
            Assert.Same(pair.s, pair.t);
        }
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Query_with_Include_marks_only_left_as_loaded(bool async)
    {
        using var context = Fixture.CreateContext();

        var queryable = context.EntityOnes.Include(e => e.TwoSkip);
        var left = async
            ? await queryable.SingleAsync(e => e.Id == 1)
            : queryable.Single(e => e.Id == 1);

        Assert.True(context.Entry(left).Collection(e => e.TwoSkip).IsLoaded);

        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(20, left.TwoSkip.Count);
        foreach (var right in left.TwoSkip)
        {
            Assert.False(context.Entry(right).Collection(e => e.OneSkip).IsLoaded);
            Assert.Same(left, right.OneSkip.Single());
        }
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Query_with_filtered_Include_marks_only_left_as_loaded(bool async)
    {
        using var context = Fixture.CreateContext();

        var queryable = context.EntityOnes.Include(e => e.TwoSkip.Where(e => e.Id == 1 || e.Id == 2));
        var left = async
            ? await queryable.SingleAsync(e => e.Id == 1)
            : queryable.Single(e => e.Id == 1);

        Assert.True(context.Entry(left).Collection(e => e.TwoSkip).IsLoaded);

        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(2, left.TwoSkip.Count);
        foreach (var right in left.TwoSkip)
        {
            Assert.False(context.Entry(right).Collection(e => e.OneSkip).IsLoaded);
            Assert.Same(left, right.OneSkip.Single());
        }
    }

    private static void SetState(
        DbContext context,
        object entity,
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.TrackAll)
    {
        if (state != (queryTrackingBehavior == QueryTrackingBehavior.TrackAll ? EntityState.Unchanged : EntityState.Detached))
        {
            context.Entry(entity).State = state;
        }
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
        protected override string StoreName
            => "ManyToManyLoadTest";
    }
}
