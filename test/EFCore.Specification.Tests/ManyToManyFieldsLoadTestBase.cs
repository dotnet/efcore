// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class ManyToManyFieldsLoadTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : ManyToManyFieldsLoadTestBase<TFixture>.ManyToManyFieldsLoadFixtureBase
{
    protected ManyToManyFieldsLoadTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

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

        if (async)
        {
            await collectionEntry.LoadAsync();
        }
        else
        {
            collectionEntry.Load();
        }

        Assert.True(collectionEntry.IsLoaded);
        foreach (var entityTwo in left.TwoSkip)
        {
            Assert.False(context.Entry(entityTwo).Collection(e => e.OneSkip).IsLoaded);
        }

        RecordLog();

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
        foreach (var entityTwo in left.TwoSkipShared)
        {
            Assert.False(context.Entry(entityTwo).Collection(e => e.OneSkipShared).IsLoaded);
        }

        RecordLog();

        Assert.Equal(3, left.TwoSkipShared.Count);
        foreach (var right in left.TwoSkipShared)
        {
            Assert.Contains(left, right.OneSkipShared);
        }

        Assert.Equal(children, left.TwoSkipShared.ToList());

        Assert.Equal(1 + 3 + 3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Added)]
    public virtual void Attached_collections_are_not_marked_as_loaded(EntityState state)
    {
        using var context = Fixture.CreateContext();

        var left = new EntityOne
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

        if (async)
        {
            await collectionEntry.LoadAsync();
        }
        else
        {
            collectionEntry.Load();
        }

        Assert.True(collectionEntry.IsLoaded);
        foreach (var entityTwo in left.ThreeSkipPayloadFull)
        {
            Assert.False(context.Entry(entityTwo).Collection(e => e.OneSkipPayloadFull).IsLoaded);
        }

        RecordLog();

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
        foreach (var entityTwo in left.TwoSkip)
        {
            Assert.False(context.Entry(entityTwo).Collection(e => e.OneSkip).IsLoaded);
        }

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

        if (async)
        {
            await navigationEntry.LoadAsync();
        }
        else
        {
            navigationEntry.Load();
        }

        Assert.True(navigationEntry.IsLoaded);
        foreach (var entityTwo in left.TwoSkip)
        {
            Assert.False(context.Entry((object)entityTwo).Collection("OneSkip").IsLoaded);
        }

        RecordLog();

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
        foreach (var entityTwo in left.TwoSkipShared)
        {
            Assert.False(context.Entry((object)entityTwo).Collection("OneSkipShared").IsLoaded);
        }

        RecordLog();

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

        var left = context.Attach(new EntityOne { Id = 999 }).Entity;

        ClearLog();

        var navigationEntry = context.Entry(left).Navigation("TwoSkip");

        context.Entry(left).State = state;

        Assert.False(navigationEntry.IsLoaded);

        if (async)
        {
            await navigationEntry.LoadAsync();
        }
        else
        {
            navigationEntry.Load();
        }

        Assert.True(navigationEntry.IsLoaded);

        RecordLog();

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

        var left = context.Attach(new EntityOne { Id = 999 }).Entity;

        ClearLog();

        var navigationEntry = context.Entry(left).Navigation("TwoSkip");

        context.Entry(left).State = state;

        Assert.False(navigationEntry.IsLoaded);

        var children = async
            ? await navigationEntry.Query().ToListAsync<object>()
            : navigationEntry.Query().ToList<object>();

        Assert.False(navigationEntry.IsLoaded);

        RecordLog();

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

        if (async)
        {
            await navigationEntry.LoadAsync();
        }
        else
        {
            navigationEntry.Load();
        }

        Assert.True(navigationEntry.IsLoaded);
        foreach (var entityTwo in left.ThreeSkipPayloadFull)
        {
            Assert.False(context.Entry((object)entityTwo).Collection("OneSkipPayloadFull").IsLoaded);
        }

        RecordLog();

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
        foreach (var entityTwo in left.TwoSkip)
        {
            Assert.False(context.Entry((object)entityTwo).Collection("OneSkip").IsLoaded);
        }

        RecordLog();

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

        if (async)
        {
            await collectionEntry.LoadAsync();
        }
        else
        {
            collectionEntry.Load();
        }

        Assert.True(collectionEntry.IsLoaded);
        foreach (var entityTwo in left.ThreeSkipFull)
        {
            Assert.False(context.Entry(entityTwo).Collection(e => e.CompositeKeySkipFull).IsLoaded);
        }

        RecordLog();

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
        foreach (var entityTwo in left.ThreeSkipFull)
        {
            Assert.False(context.Entry(entityTwo).Collection(e => e.CompositeKeySkipFull).IsLoaded);
        }

        RecordLog();

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

        var left = context.Set<EntityOne>().Find(3);

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

        var left = context.Set<EntityOne>().Find(3);

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

        var left = context.Set<EntityOne>().Find(3);

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

        var left = context.Set<EntityOne>().Find(3);

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

        var left = context.Set<EntityOne>().Find(3);

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

        var left = context.Set<EntityOne>().Find(3);

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

        Assert.Equal(2, left.TwoSkip.Count);
        foreach (var right in left.TwoSkip)
        {
            Assert.False(context.Entry(right).Collection(e => e.OneSkip).IsLoaded);
            Assert.Same(left, right.OneSkip.Single());
        }
    }

    protected virtual void ClearLog()
    {
    }

    protected virtual void RecordLog()
    {
    }

    protected TFixture Fixture { get; }

    public abstract class ManyToManyFieldsLoadFixtureBase : ManyToManyFieldsQueryFixtureBase
    {
        protected override string StoreName
            => "ManyToManyFieldsLoadTest";

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(
                    c => c
                        .Log(CoreEventId.ShadowForeignKeyPropertyCreated))
                .EnableDetailedErrors();
    }
}
