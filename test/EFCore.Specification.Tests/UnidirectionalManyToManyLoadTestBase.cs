// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract partial class ManyToManyLoadTestBase<TFixture>
{
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
    public virtual async Task Load_collection_unidirectional(EntityState state, QueryTrackingBehavior queryTrackingBehavior, bool async)
    {
        using var context = Fixture.CreateContext();

        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;

        var left = context.Set<UnidirectionalEntityOne>().Find(3);

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
        foreach (var entityTwo in left.TwoSkip)
        {
            Assert.False(context.Entry(entityTwo).Collection("UnidirectionalEntityOne1").IsLoaded);
        }

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(7, left.TwoSkip.Count);
        foreach (var right in left.TwoSkip)
        {
            Assert.Contains(left, ((IEnumerable<object>)context.Entry(right).Collection("UnidirectionalEntityOne1").CurrentValue)!);
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
    public virtual async Task Load_collection_using_Query_unidirectional(EntityState state, bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<UnidirectionalEntityOne>().Find(3);

        ClearLog();

        var collectionEntry = context.Entry(left).Collection(e => e.TwoSkipShared);

        context.Entry(left).State = state;

        Assert.False(collectionEntry.IsLoaded);

        var children = async
            ? await collectionEntry.Query().ToListAsync()
            : collectionEntry.Query().ToList();

        Assert.False(collectionEntry.IsLoaded);
        foreach (var entityTwo in left.TwoSkipShared)
        {
            Assert.False(context.Entry(entityTwo).Collection("UnidirectionalEntityOne").IsLoaded);
        }

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(3, left.TwoSkipShared.Count);
        foreach (var right in left.TwoSkipShared)
        {
            Assert.Contains(left, context.Entry(right).Collection("UnidirectionalEntityOne").CurrentValue!.Cast<object>());
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
    public virtual void Attached_collections_are_not_marked_as_loaded_unidirectional(EntityState state, bool lazy)
    {
        using var context = Fixture.CreateContext();

        context.ChangeTracker.LazyLoadingEnabled = false;

        var left = ExpectLazyLoading
            ? context.CreateProxy<UnidirectionalEntityOne>(
                b =>
                {
                    b.Id = 7776;
                    b.TwoSkip = new ObservableCollection<UnidirectionalEntityTwo> { new() { Id = 7777 } };
                    b.TwoSkipShared = new ObservableCollection<UnidirectionalEntityTwo> { new() { Id = 7778 } };
                    b.SelfSkipPayloadLeft = new ObservableCollection<UnidirectionalEntityOne> { new() { Id = 7779 } };
                    b.BranchSkip = new ObservableCollection<UnidirectionalEntityBranch> { new() { Id = 7781 } };
                    b.ThreeSkipPayloadFullShared = new ObservableCollection<UnidirectionalEntityThree> { new() { Id = 7783 } };
                })
            : new UnidirectionalEntityOne
            {
                Id = 7776,
                TwoSkip = new List<UnidirectionalEntityTwo> { new() { Id = 7777 } },
                TwoSkipShared = new List<UnidirectionalEntityTwo> { new() { Id = 7778 } },
                SelfSkipPayloadLeft = new List<UnidirectionalEntityOne> { new() { Id = 7779 } },
                BranchSkip = new List<UnidirectionalEntityBranch> { new() { Id = 7781 } },
                ThreeSkipPayloadFullShared = new List<UnidirectionalEntityThree> { new() { Id = 7783 } }
            };

        var entityThreeCollection = context.Entry(left).Collection<UnidirectionalEntityThree>("UnidirectionalEntityThree");
        entityThreeCollection.CurrentValue = ExpectLazyLoading
            ? new ObservableCollection<UnidirectionalEntityThree>()
            : new List<UnidirectionalEntityThree>();
        ((ICollection<UnidirectionalEntityThree>)entityThreeCollection.CurrentValue!).Add(new UnidirectionalEntityThree { Id = 7782 });

        var entityOneCollection = context.Entry(left).Collection<UnidirectionalEntityOne>("UnidirectionalEntityOne");
        entityOneCollection.CurrentValue = ExpectLazyLoading
            ? new ObservableCollection<UnidirectionalEntityOne>()
            : new List<UnidirectionalEntityOne>();
        ((ICollection<UnidirectionalEntityOne>)entityOneCollection.CurrentValue!).Add(new UnidirectionalEntityOne { Id = 7780 });

        context.Attach(left);

        if (state != EntityState.Unchanged)
        {
            foreach (var child in left.TwoSkip.Cast<object>()
                         .Concat(left.TwoSkipShared)
                         .Concat(left.SelfSkipPayloadLeft)
                         .Concat(entityOneCollection.CurrentValue!)
                         .Concat(left.BranchSkip)
                         .Concat(entityThreeCollection.CurrentValue!)
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
        Assert.False(entityOneCollection.IsLoaded);
        Assert.False(context.Entry(left).Collection(e => e.BranchSkip).IsLoaded);
        Assert.False(entityThreeCollection.IsLoaded);
        Assert.False(context.Entry(left).Collection(e => e.ThreeSkipPayloadFullShared).IsLoaded);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_collection_already_loaded_unidirectional(EntityState state, bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<UnidirectionalEntityOne>().Include("UnidirectionalEntityThree").Single(e => e.Id == 3);

        ClearLog();

        var collectionEntry = context.Entry(left).Collection<UnidirectionalEntityThree>("UnidirectionalEntityThree");

        context.Entry(left).State = state;

        Assert.True(collectionEntry.IsLoaded);

        if (ExpectLazyLoading)
        {
            Assert.Equal(4, collectionEntry.CurrentValue!.Count());
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
        foreach (var entityTwo in collectionEntry.CurrentValue!)
        {
            Assert.False(context.Entry(entityTwo).Collection("UnidirectionalEntityOne").IsLoaded);
        }

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(4, collectionEntry.CurrentValue!.Count());
        foreach (var right in collectionEntry.CurrentValue!)
        {
            Assert.Contains(left, context.Entry(right).Collection<UnidirectionalEntityOne>("UnidirectionalEntityOne").CurrentValue!);
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
    public virtual async Task Load_collection_using_Query_already_loaded_unidirectional(EntityState state, bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<UnidirectionalEntityOne>().Include(e => e.TwoSkip).Single(e => e.Id == 3);

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
        foreach (var entityTwo in left.TwoSkip)
        {
            Assert.False(context.Entry(entityTwo).Collection("UnidirectionalEntityOne1").IsLoaded);
        }

        Assert.Equal(7, left.TwoSkip.Count);
        foreach (var right in left.TwoSkip)
        {
            Assert.Contains(left, ((IEnumerable<object>)context.Entry(right).Navigation("UnidirectionalEntityOne1").CurrentValue)!);
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
    public virtual async Task Load_collection_untyped_unidirectional(EntityState state, bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<UnidirectionalEntityOne>().Find(3);

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
        foreach (var entityTwo in left.TwoSkip)
        {
            Assert.False(context.Entry((object)entityTwo).Collection("UnidirectionalEntityOne1").IsLoaded);
        }

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(7, left.TwoSkip.Count);
        foreach (var right in left.TwoSkip)
        {
            Assert.Contains(left, ((IEnumerable<object>)context.Entry(right).Member("UnidirectionalEntityOne1").CurrentValue)!);
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
    public virtual async Task Load_collection_using_Query_untyped_unidirectional(EntityState state, bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<UnidirectionalEntityOne>().Find(3);

        ClearLog();

        var collectionEntry = context.Entry(left).Navigation("TwoSkipShared");

        context.Entry(left).State = state;

        Assert.False(collectionEntry.IsLoaded);

        var children = async
            ? await collectionEntry.Query().ToListAsync<object>()
            : collectionEntry.Query().ToList<object>();

        Assert.False(collectionEntry.IsLoaded);
        foreach (var entityTwo in left.TwoSkipShared)
        {
            Assert.False(context.Entry((object)entityTwo).Collection("UnidirectionalEntityOne").IsLoaded);
        }

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(3, left.TwoSkipShared.Count);
        foreach (var right in left.TwoSkipShared)
        {
            Assert.Contains(left, context.Entry(right).Collection("UnidirectionalEntityOne").CurrentValue!.Cast<object>());
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
    public virtual async Task Load_collection_not_found_untyped_unidirectional(EntityState state, bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Attach(
            ExpectLazyLoading
                ? context.CreateProxy<UnidirectionalEntityOne>(b => b.Id = 999)
                : new UnidirectionalEntityOne { Id = 999 }).Entity;

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
    public virtual async Task Load_collection_using_Query_not_found_untyped_unidirectional(EntityState state, bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Attach(
            ExpectLazyLoading
                ? context.CreateProxy<UnidirectionalEntityOne>(b => b.Id = 999)
                : new UnidirectionalEntityOne { Id = 999 }).Entity;

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
    public virtual async Task Load_collection_already_loaded_untyped_unidirectional(
        EntityState state,
        bool async,
        CascadeTiming deleteOrphansTiming)
    {
        using var context = Fixture.CreateContext();

        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

        var left = context.Set<UnidirectionalEntityOne>().Include("UnidirectionalEntityThree").Single(e => e.Id == 3);

        ClearLog();

        var navigationEntry = context.Entry(left).Collection<UnidirectionalEntityThree>("UnidirectionalEntityThree");

        context.Entry(left).State = state;

        Assert.True(navigationEntry.IsLoaded);

        if (ExpectLazyLoading)
        {
            Assert.Equal(4, navigationEntry.CurrentValue!.Count());
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
        foreach (var entityTwo in navigationEntry.CurrentValue!)
        {
            Assert.False(context.Entry((object)entityTwo).Collection("UnidirectionalEntityOne").IsLoaded);
        }

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(4, navigationEntry.CurrentValue!.Count());
        foreach (var right in navigationEntry.CurrentValue!)
        {
            Assert.Contains(
                left, context.Entry((object)right).Collection("UnidirectionalEntityOne")
                    .CurrentValue!.Cast<UnidirectionalEntityOne>());
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
    public virtual async Task Load_collection_using_Query_already_loaded_untyped_unidirectional(
        EntityState state,
        bool async,
        CascadeTiming deleteOrphansTiming)
    {
        using var context = Fixture.CreateContext();

        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

        var left = context.Set<UnidirectionalEntityOne>().Include(e => e.TwoSkip).Single(e => e.Id == 3);

        ClearLog();

        var navigationEntry = context.Entry(left).Navigation("TwoSkip");

        context.Entry(left).State = state;

        Assert.True(navigationEntry.IsLoaded);

        // Issue #16429
        var children = async
            ? await navigationEntry.Query().ToListAsync<object>()
            : navigationEntry.Query().ToList<object>();

        Assert.True(navigationEntry.IsLoaded);
        foreach (var entityTwo in left.TwoSkip)
        {
            Assert.False(context.Entry((object)entityTwo).Collection("UnidirectionalEntityOne1").IsLoaded);
        }

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(7, left.TwoSkip.Count);
        foreach (var right in left.TwoSkip)
        {
            Assert.Contains(left, ((IEnumerable<object>)context.Entry(right).Collection("UnidirectionalEntityOne1").CurrentValue)!);
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
    public virtual async Task Load_collection_composite_key_unidirectional(EntityState state, bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<UnidirectionalEntityCompositeKey>().Find(7, "7_2", new DateTime(2007, 2, 1));

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
        foreach (var entityTwo in left.ThreeSkipFull)
        {
            Assert.False(context.Entry(entityTwo).Collection("UnidirectionalEntityCompositeKey").IsLoaded);
        }

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(2, left.ThreeSkipFull.Count);
        foreach (var right in left.ThreeSkipFull)
        {
            Assert.Contains(left, context.Entry(right).Collection("UnidirectionalEntityCompositeKey").CurrentValue!.Cast<object>());
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
    public virtual async Task Load_collection_using_Query_composite_key_unidirectional(EntityState state, bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<UnidirectionalEntityCompositeKey>().Find(7, "7_2", new DateTime(2007, 2, 1));

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
    public virtual async Task Load_collection_for_detached_throws_unidirectional(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<UnidirectionalEntityOne>().AsTracking(queryTrackingBehavior).Single(e => e.Id == 3);

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
    public virtual void Query_collection_for_detached_throws_unidirectional(QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<UnidirectionalEntityOne>().AsTracking(queryTrackingBehavior).Single(e => e.Id == 3);

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
    public virtual async Task Load_collection_using_Query_with_Include_unidirectional(bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<UnidirectionalEntityOne>().Find(3);

        ClearLog();

        var collectionEntry = context.Entry(left).Collection(e => e.TwoSkipShared);

        Assert.False(collectionEntry.IsLoaded);

        var children = async
            ? await collectionEntry.Query().Include("UnidirectionalEntityThree").ToListAsync()
            : collectionEntry.Query().Include("UnidirectionalEntityThree").ToList();

        Assert.False(collectionEntry.IsLoaded);
        foreach (var entityTwo in left.TwoSkipShared)
        {
            var threeNav = context.Entry(entityTwo).Collection<UnidirectionalEntityThree>("UnidirectionalEntityThree");
            Assert.True(threeNav.IsLoaded);
            foreach (var entityThree in threeNav.CurrentValue!)
            {
                Assert.False(context.Entry(entityThree).Collection(e => e.TwoSkipFull).IsLoaded);
            }
        }

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(3, left.TwoSkipShared.Count);
        foreach (var right in left.TwoSkipShared)
        {
            foreach (var three in context.Entry(right).Collection<UnidirectionalEntityThree>("UnidirectionalEntityThree").CurrentValue!)
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
    public virtual async Task Load_collection_using_Query_with_Include_for_inverse_unidirectional(bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<UnidirectionalEntityOne>().Find(3);

        ClearLog();

        var collectionEntry = context.Entry(left).Collection(e => e.TwoSkipShared);

        Assert.False(collectionEntry.IsLoaded);

        var queryable = collectionEntry.Query().Include("UnidirectionalEntityOne");
        var children = async
            ? await queryable.ToListAsync()
            : queryable.ToList();

        Assert.False(collectionEntry.IsLoaded);
        foreach (var entityTwo in left.TwoSkipShared)
        {
            Assert.True(context.Entry(entityTwo).Collection("UnidirectionalEntityOne").IsLoaded);
        }

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(3, left.TwoSkipShared.Count);
        foreach (var right in left.TwoSkipShared)
        {
            Assert.Contains(
                left,
                context.Entry(right).Collection("UnidirectionalEntityOne").CurrentValue!.Cast<object>());
        }

        Assert.Equal(children, left.TwoSkipShared.ToList());
        Assert.Equal(7, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Load_collection_using_Query_with_filtered_Include_unidirectional(bool async)
    {
        using var context = Fixture.CreateContext();

        var left = await context.Set<UnidirectionalEntityOne>().FindAsync(3);

        ClearLog();

        var collectionEntry = context.Entry(left).Collection(e => e.TwoSkipShared);

        Assert.False(collectionEntry.IsLoaded);

        var children = async
            ? await collectionEntry.Query()
                .Include(
                    e => EF.Property<ICollection<UnidirectionalEntityThree>>(e, "UnidirectionalEntityThree")
                        .Where(e => e.Id == 13 || e.Id == 11)).ToListAsync()
            : collectionEntry.Query()
                .Include(
                    e => EF.Property<ICollection<UnidirectionalEntityThree>>(e, "UnidirectionalEntityThree")
                        .Where(e => e.Id == 13 || e.Id == 11)).ToList();

        Assert.False(collectionEntry.IsLoaded);
        foreach (var entityTwo in left.TwoSkipShared)
        {
            Assert.False(context.Entry(entityTwo).Collection("UnidirectionalEntityOne").IsLoaded);
            Assert.True(context.Entry(entityTwo).Collection("UnidirectionalEntityThree").IsLoaded);

            foreach (var entityThree in
                     context.Entry(entityTwo).Collection<UnidirectionalEntityThree>("UnidirectionalEntityThree").CurrentValue!)
            {
                Assert.False(context.Entry(entityThree).Collection(e => e.TwoSkipFull).IsLoaded);
            }
        }

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(3, left.TwoSkipShared.Count);
        foreach (var right in left.TwoSkipShared)
        {
            Assert.Contains(left, context.Entry(right).Collection("UnidirectionalEntityOne").CurrentValue!.Cast<object>());
            foreach (var three in context.Entry(right).Collection<UnidirectionalEntityThree>("UnidirectionalEntityThree").CurrentValue!)
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
    public virtual async Task Load_collection_using_Query_with_filtered_Include_and_projection_unidirectional(bool async)
    {
        using var context = Fixture.CreateContext();

        var left = await context.Set<UnidirectionalEntityOne>().FindAsync(3);

        ClearLog();

        var collectionEntry = context.Entry(left).Collection(e => e.TwoSkipShared);

        Assert.False(collectionEntry.IsLoaded);

        var queryable = collectionEntry
            .Query()
            .Include(
                e => EF.Property<ICollection<UnidirectionalEntityThree>>(e, "UnidirectionalEntityThree")
                    .Where(e => e.Id == 13 || e.Id == 11))
            .OrderBy(e => e.Id)
            .Select(
                e => new
                {
                    e.Id, e.Name,
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

        Assert.Equal(11, projected[1].Id);
        Assert.Equal("EntityTwo 11", projected[1].Name);

        Assert.Equal(16, projected[2].Id);
        Assert.Equal("EntityTwo 16", projected[2].Name);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Load_collection_using_Query_with_join_unidirectional(bool async)
    {
        using var context = Fixture.CreateContext();

        var left = context.Set<UnidirectionalEntityOne>().Find(3);

        ClearLog();

        var collectionEntry = context.Entry(left).Collection(e => e.TwoSkipShared);

        Assert.False(collectionEntry.IsLoaded);

        var queryable = from t in collectionEntry.Query()
                        join s in context.Set<UnidirectionalEntityOne>().SelectMany(e => e.TwoSkipShared)
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
    public virtual async Task Query_with_Include_marks_only_left_as_loaded_unidirectional(bool async)
    {
        using var context = Fixture.CreateContext();

        var queryable = context.UnidirectionalEntityOnes.Include(e => e.TwoSkip);
        var left = async
            ? await queryable.SingleAsync(e => e.Id == 1)
            : queryable.Single(e => e.Id == 1);

        Assert.True(context.Entry(left).Collection(e => e.TwoSkip).IsLoaded);

        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(20, left.TwoSkip.Count);
        foreach (var right in left.TwoSkip)
        {
            Assert.False(context.Entry(right).Navigation("UnidirectionalEntityOne1").IsLoaded);
            Assert.Same(left, context.Entry(right).Collection("UnidirectionalEntityOne1").CurrentValue!.Cast<object>().Single());
        }
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Query_with_filtered_Include_marks_only_left_as_loaded_unidirectional(bool async)
    {
        using var context = Fixture.CreateContext();

        var queryable = context.UnidirectionalEntityOnes.Include(e => e.TwoSkip.Where(e => e.Id == 1 || e.Id == 2));
        var left = async
            ? await queryable.SingleAsync(e => e.Id == 1)
            : queryable.Single(e => e.Id == 1);

        Assert.True(context.Entry(left).Collection(e => e.TwoSkip).IsLoaded);

        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(2, left.TwoSkip.Count);
        foreach (var right in left.TwoSkip)
        {
            Assert.False(context.Entry(right).Collection<UnidirectionalEntityOne>("UnidirectionalEntityOne1").IsLoaded);
            Assert.Same(left, context.Entry(right).Collection("UnidirectionalEntityOne1").CurrentValue!.Cast<object>().Single());
        }
    }
}
