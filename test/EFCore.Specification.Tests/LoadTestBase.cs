// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract partial class LoadTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : LoadTestBase<TFixture>.LoadFixtureBase
{
    protected LoadTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Added, true)]
    public virtual void Attached_references_to_principal_are_marked_as_loaded(EntityState state, bool lazy)
    {
        using var context = CreateContext(lazy);
        var parent = new Parent
        {
            Id = 707,
            AlternateId = "Root",
            SinglePkToPk = new SinglePkToPk { Id = 707 },
            Single = new Single { Id = 21 },
            RequiredSingle = new RequiredSingle { Id = 21 },
            SingleAk = new SingleAk { Id = 42 },
            SingleShadowFk = new SingleShadowFk { Id = 62 },
            SingleCompositeKey = new SingleCompositeKey { Id = 62 }
        };

        context.Attach(parent);

        if (state != EntityState.Unchanged)
        {
            context.ChangeTracker.LazyLoadingEnabled = false;

            context.Entry(parent.SinglePkToPk).State = state;
            context.Entry(parent.Single).State = state;
            context.Entry(parent.SingleAk).State = state;
            context.Entry(parent.SingleShadowFk).State = state;
            context.Entry(parent.SingleCompositeKey).State = state;
            context.Entry(parent).State = state;

            context.ChangeTracker.LazyLoadingEnabled = true;
        }

        Assert.True(context.Entry(parent).Reference(e => e.SinglePkToPk).IsLoaded);
        Assert.True(context.Entry(parent).Reference(e => e.Single).IsLoaded);
        Assert.True(context.Entry(parent).Reference(e => e.SingleAk).IsLoaded);
        Assert.True(context.Entry(parent).Reference(e => e.SingleShadowFk).IsLoaded);
        Assert.True(context.Entry(parent).Reference(e => e.SingleCompositeKey).IsLoaded);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Added, true)]
    public virtual void Attached_references_to_dependents_are_marked_as_loaded(EntityState state, bool lazy)
    {
        using var context = CreateContext(lazy);
        var parent = new Parent
        {
            Id = 707,
            AlternateId = "Root",
            SinglePkToPk = new SinglePkToPk { Id = 707 },
            Single = new Single { Id = 21 },
            RequiredSingle = new RequiredSingle { Id = 21 },
            SingleAk = new SingleAk { Id = 42 },
            SingleShadowFk = new SingleShadowFk { Id = 62 },
            SingleCompositeKey = new SingleCompositeKey { Id = 62 }
        };

        context.Attach(parent);

        if (state != EntityState.Unchanged)
        {
            context.ChangeTracker.LazyLoadingEnabled = false;

            context.Entry(parent.SinglePkToPk).State = state;
            context.Entry(parent.Single).State = state;
            context.Entry(parent.RequiredSingle).State = state;
            context.Entry(parent.SingleAk).State = state;
            context.Entry(parent.SingleShadowFk).State = state;
            context.Entry(parent.SingleCompositeKey).State = state;
            context.Entry(parent).State = state;

            context.ChangeTracker.LazyLoadingEnabled = true;
        }

        Assert.True(context.Entry(parent.SinglePkToPk).Reference(e => e.Parent).IsLoaded);
        Assert.True(context.Entry(parent.Single).Reference(e => e.Parent).IsLoaded);
        Assert.True(context.Entry(parent.RequiredSingle).Reference(e => e.Parent).IsLoaded);
        Assert.True(context.Entry(parent.SingleAk).Reference(e => e.Parent).IsLoaded);
        Assert.True(context.Entry(parent.SingleShadowFk).Reference(e => e.Parent).IsLoaded);
        Assert.True(context.Entry(parent.SingleCompositeKey).Reference(e => e.Parent).IsLoaded);
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
        using var context = CreateContext(lazy);
        var parent = new Parent
        {
            Id = 707,
            AlternateId = "Root",
            Children = new List<Child> { new() { Id = 11 }, new() { Id = 12 } },
            ChildrenAk = new List<ChildAk> { new() { Id = 31 }, new() { Id = 32 } },
            ChildrenShadowFk = new List<ChildShadowFk> { new() { Id = 51 }, new() { Id = 52 } },
            ChildrenCompositeKey = new List<ChildCompositeKey> { new() { Id = 51 }, new() { Id = 52 } }
        };

        context.Attach(parent);

        if (state != EntityState.Unchanged)
        {
            context.ChangeTracker.LazyLoadingEnabled = false;

            foreach (var child in parent.Children.Cast<object>()
                         .Concat(parent.ChildrenAk)
                         .Concat(parent.ChildrenShadowFk)
                         .Concat(parent.ChildrenCompositeKey))
            {
                context.Entry(child).State = state;
            }

            context.Entry(parent).State = state;

            context.ChangeTracker.LazyLoadingEnabled = true;
        }

        Assert.False(context.Entry(parent).Collection(e => e.Children).IsLoaded);
        Assert.False(context.Entry(parent).Collection(e => e.ChildrenAk).IsLoaded);
        Assert.False(context.Entry(parent).Collection(e => e.ChildrenShadowFk).IsLoaded);
        Assert.False(context.Entry(parent).Collection(e => e.ChildrenCompositeKey).IsLoaded);
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
        using var context = CreateContext();
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;

        var parent = context.Set<Parent>().Single();

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior);

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

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

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(2, parent.Children.Count());
        Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));

        Assert.Equal(state == EntityState.Detached ? 0 : 3, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_many_to_one_reference_to_principal(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Set<Child>().Single(e => e.Id == 12);

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        context.Entry(child).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Deleted)
        {
            Assert.Same(child, child.Parent.Children.Single());
        }

        if (state != EntityState.Detached)
        {
            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            if (state == EntityState.Deleted)
            {
                Assert.Null(child.Parent);
                Assert.Null(parent.Children);
            }
            else
            {
                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.Children.Single());
            }
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
    public virtual async Task Load_one_to_one_reference_to_principal(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<Single>().Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Deleted)
        {
            Assert.Same(single, single.Parent.Single);
        }

        if (state != EntityState.Detached)
        {
            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            if (state == EntityState.Deleted)
            {
                Assert.Null(single.Parent);
                Assert.Null(parent.Single);
            }
            else
            {
                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.Single);
            }
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
    public virtual async Task Load_one_to_one_reference_to_principal_when_NoTracking_behavior(EntityState state, bool async)
    {
        using var context = CreateContext();
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var single = context.Set<Single>().Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Deleted)
        {
            Assert.Same(single, single.Parent.Single);
        }

        if (state != EntityState.Detached)
        {
            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            if (state == EntityState.Deleted)
            {
                Assert.Null(single.Parent);
                Assert.Null(parent.Single);
            }
            else
            {
                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.Single);
            }
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
    public virtual async Task Load_one_to_one_reference_to_dependent(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        context.Entry(parent).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        Assert.Same(parent, parent.Single.Parent);

        if (state != EntityState.Detached)
        {
            var single = context.ChangeTracker.Entries<Single>().Single().Entity;

            Assert.Same(single, parent.Single);
            Assert.Same(parent, single.Parent);
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
    public virtual async Task Load_one_to_one_PK_to_PK_reference_to_principal(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<SinglePkToPk>().Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Detached)
        {
            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            if (state == EntityState.Deleted)
            {
                Assert.Null(single.Parent);
                Assert.Null(parent.SinglePkToPk);
            }
            else
            {
                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.SinglePkToPk);
            }
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
    public virtual async Task Load_one_to_one_PK_to_PK_reference_to_dependent(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.SinglePkToPk);

        context.Entry(parent).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        if (state != EntityState.Detached)
        {
            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var single = context.ChangeTracker.Entries<SinglePkToPk>().Single().Entity;

            Assert.Same(single, parent.SinglePkToPk);
            Assert.Same(parent, single.Parent);
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
    public virtual async Task Load_collection_using_Query(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        context.Entry(parent).State = state;

        Assert.False(collectionEntry.IsLoaded);

        var children = async
            ? await collectionEntry.Query().ToListAsync()
            : collectionEntry.Query().ToList();

        Assert.False(collectionEntry.IsLoaded);

        RecordLog();

        Assert.Equal(2, children.Count);

        if (state == EntityState.Detached)
        {
            Assert.Empty(parent.Children);
            Assert.All(children, c => Assert.Null(c.Parent));
            Assert.Empty(context.ChangeTracker.Entries());
        }
        else
        {
            Assert.Equal(2, parent.Children.Count());
            Assert.All(children.Select(e => e.Parent), c => Assert.Same(parent, c));
            Assert.All(children, p => Assert.Contains(p, parent.Children));
            Assert.Equal(3, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_many_to_one_reference_to_principal_using_Query(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Set<Child>().Single(e => e.Id == 12);

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        context.Entry(child).State = state;

        Assert.False(referenceEntry.IsLoaded);

        var parent = async
            ? await referenceEntry.Query().SingleAsync()
            : referenceEntry.Query().Single();

        RecordLog();

        Assert.NotNull(parent);

        if (state != EntityState.Detached)
        {
            if (state == EntityState.Deleted)
            {
                Assert.False(referenceEntry.IsLoaded);
                Assert.Null(child.Parent);
                Assert.Null(parent.Children);
            }
            else
            {
                Assert.True(referenceEntry.IsLoaded);
                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.Children.Single());
            }

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_one_to_one_reference_to_principal_using_Query(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<Single>().Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.False(referenceEntry.IsLoaded);

        var parent = async
            ? await referenceEntry.Query().SingleAsync()
            : referenceEntry.Query().Single();

        RecordLog();

        Assert.NotNull(parent);

        if (state != EntityState.Detached)
        {
            if (state == EntityState.Deleted)
            {
                Assert.False(referenceEntry.IsLoaded);
                Assert.Null(single.Parent);
                Assert.Null(parent.Single);
            }
            else
            {
                Assert.True(referenceEntry.IsLoaded);
                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.Single);
            }

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_one_to_one_reference_to_dependent_using_Query(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        context.Entry(parent).State = state;

        Assert.False(referenceEntry.IsLoaded);

        var single = async
            ? await referenceEntry.Query().SingleAsync()
            : referenceEntry.Query().Single();

        Assert.NotNull(single);

        if (state != EntityState.Detached)
        {
            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Same(single, parent.Single);
            Assert.Same(parent, single.Parent);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_one_to_one_PK_to_PK_reference_to_principal_using_Query(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<SinglePkToPk>().Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.False(referenceEntry.IsLoaded);

        var parent = async
            ? await referenceEntry.Query().SingleAsync()
            : referenceEntry.Query().Single();

        RecordLog();

        Assert.NotNull(parent);

        if (state != EntityState.Detached)
        {
            if (state == EntityState.Deleted)
            {
                Assert.False(referenceEntry.IsLoaded);
                Assert.Null(single.Parent);
                Assert.Null(parent.SinglePkToPk);
            }
            else
            {
                Assert.True(referenceEntry.IsLoaded);
                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.SinglePkToPk);
            }

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_one_to_one_PK_to_PK_reference_to_dependent_using_Query(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.SinglePkToPk);

        context.Entry(parent).State = state;

        Assert.False(referenceEntry.IsLoaded);

        var single = async
            ? await referenceEntry.Query().SingleAsync()
            : referenceEntry.Query().Single();

        Assert.NotNull(single);

        if (state != EntityState.Detached)
        {
            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Same(single, parent.SinglePkToPk);
            Assert.Same(parent, single.Parent);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_many_to_one_reference_to_principal_null_FK(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(new Child { Id = 767, ParentId = null }).Entity;

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        context.Entry(child).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
        Assert.Null(child.Parent);
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
    public virtual async Task Load_one_to_one_reference_to_principal_null_FK(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(new Single { Id = 767, ParentId = null }).Entity;

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());

        Assert.Null(single.Parent);
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
    public virtual async Task Load_many_to_one_reference_to_principal_using_Query_null_FK(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(new Child { Id = 767, ParentId = null }).Entity;

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        context.Entry(child).State = state;

        Assert.False(referenceEntry.IsLoaded);

        var parent = async
            ? await referenceEntry.Query().SingleOrDefaultAsync()
            : referenceEntry.Query().SingleOrDefault();

        Assert.False(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Null(parent);
        Assert.Null(child.Parent);

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
    public virtual async Task Load_one_to_one_reference_to_principal_using_Query_null_FK(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(new Single { Id = 767, ParentId = null }).Entity;

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.False(referenceEntry.IsLoaded);

        var parent = async
            ? await referenceEntry.Query().SingleOrDefaultAsync()
            : referenceEntry.Query().SingleOrDefault();

        Assert.False(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Null(parent);
        Assert.Null(single.Parent);

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
    public virtual async Task Load_collection_not_found(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Attach(new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

        ClearLog();

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        context.Entry(parent).State = state;

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

        RecordLog();

        Assert.Empty(parent.Children);
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
    public virtual async Task Load_many_to_one_reference_to_principal_not_found(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(new Child { Id = 767, ParentId = 787 }).Entity;

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        context.Entry(child).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
        Assert.Null(child.Parent);
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
    public virtual async Task Load_one_to_one_reference_to_principal_not_found(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(new Single { Id = 767, ParentId = 787 }).Entity;

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());

        Assert.Null(single.Parent);
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
    public virtual async Task Load_one_to_one_reference_to_dependent_not_found(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Attach(new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        context.Entry(parent).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());

        Assert.Null(parent.Single);
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
    public virtual async Task Load_collection_using_Query_not_found(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Attach(new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

        ClearLog();

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        context.Entry(parent).State = state;

        Assert.False(collectionEntry.IsLoaded);

        var children = async
            ? await collectionEntry.Query().ToListAsync()
            : collectionEntry.Query().ToList();

        Assert.False(collectionEntry.IsLoaded);

        RecordLog();

        Assert.Empty(children);
        Assert.Empty(parent.Children);

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
    public virtual async Task Load_many_to_one_reference_to_principal_using_Query_not_found(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(new Child { Id = 767, ParentId = 787 }).Entity;

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        context.Entry(child).State = state;

        Assert.False(referenceEntry.IsLoaded);

        var parent = async
            ? await referenceEntry.Query().SingleOrDefaultAsync()
            : referenceEntry.Query().SingleOrDefault();

        Assert.False(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Null(parent);
        Assert.Null(child.Parent);

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
    public virtual async Task Load_one_to_one_reference_to_principal_using_Query_not_found(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(new Single { Id = 767, ParentId = 787 }).Entity;

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.False(referenceEntry.IsLoaded);

        var parent = async
            ? await referenceEntry.Query().SingleOrDefaultAsync()
            : referenceEntry.Query().SingleOrDefault();

        Assert.False(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Null(parent);
        Assert.Null(single.Parent);

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
    public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_not_found(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Attach(new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        context.Entry(parent).State = state;

        Assert.False(referenceEntry.IsLoaded);

        var single = async
            ? await referenceEntry.Query().SingleOrDefaultAsync()
            : referenceEntry.Query().SingleOrDefault();

        Assert.False(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Null(single);
        Assert.Null(parent.Single);

        Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Detached, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Detached, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Unchanged, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Modified, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Modified, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Deleted, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Deleted, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Detached, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Detached, false, CascadeTiming.OnSaveChanges)]
    public virtual async Task Load_collection_already_loaded(EntityState state, bool async, CascadeTiming deleteOrphansTiming)
    {
        using var context = CreateContext();
        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

        var parent = context.Set<Parent>().Include(e => e.Children).Single();

        ClearLog();

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        foreach (var child in parent.Children)
        {
            context.Entry(child).State = state;
        }

        context.Entry(parent).State = state;

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

        RecordLog();

        Assert.Equal(2, parent.Children.Count());
        Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));

        Assert.Equal(state == EntityState.Detached ? 0 : 3, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_many_to_one_reference_to_principal_already_loaded(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Set<Child>().Include(e => e.Parent).Single(e => e.Id == 12);

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        context.Entry(child.Parent).State = state;
        context.Entry(child).State = state;

        Assert.Equal(state != EntityState.Deleted, referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Detached && state != EntityState.Deleted)
        {
            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, child.Parent);
            Assert.Same(child, parent.Children.Single());
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Detached, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Detached, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Unchanged, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Modified, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Modified, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Deleted, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Deleted, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Detached, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Detached, false, CascadeTiming.OnSaveChanges)]
    public virtual async Task Load_one_to_one_reference_to_principal_already_loaded(
        EntityState state,
        bool async,
        CascadeTiming deleteOrphansTiming)
    {
        using var context = CreateContext();
        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

        var single = context.Set<Single>().Include(e => e.Parent).Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single.Parent).State = state;
        context.Entry(single).State = state;

        Assert.Equal(state != EntityState.Deleted, referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Detached && state != EntityState.Deleted)
        {
            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.Single);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Detached, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Detached, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Unchanged, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Modified, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Modified, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Deleted, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Deleted, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Detached, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Detached, false, CascadeTiming.OnSaveChanges)]
    public virtual async Task Load_one_to_one_reference_to_dependent_already_loaded(
        EntityState state,
        bool async,
        CascadeTiming deleteOrphansTiming)
    {
        using var context = CreateContext();
        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

        var parent = context.Set<Parent>().Include(e => e.Single).Single();

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        context.Entry(parent.Single).State = state;
        context.Entry(parent).State = state;

        Assert.True(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Detached)
        {
            var single = context.ChangeTracker.Entries<Single>().Single().Entity;

            Assert.Same(single, parent.Single);
            Assert.Same(parent, single.Parent);
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
    public virtual async Task Load_one_to_one_PK_to_PK_reference_to_principal_already_loaded(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<SinglePkToPk>().Include(e => e.Parent).Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single.Parent).State = state;
        context.Entry(single).State = state;

        Assert.True(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Detached)
        {
            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.SinglePkToPk);
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
    public virtual async Task Load_one_to_one_PK_to_PK_reference_to_dependent_already_loaded(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Include(e => e.SinglePkToPk).Single();

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.SinglePkToPk);

        context.Entry(parent.SinglePkToPk).State = state;
        context.Entry(parent).State = state;

        Assert.True(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Detached)
        {
            var single = context.ChangeTracker.Entries<SinglePkToPk>().Single().Entity;

            Assert.Same(single, parent.SinglePkToPk);
            Assert.Same(parent, single.Parent);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Detached, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Detached, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Unchanged, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Modified, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Modified, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Deleted, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Deleted, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Detached, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Detached, false, CascadeTiming.OnSaveChanges)]
    public virtual async Task Load_collection_using_Query_already_loaded(
        EntityState state,
        bool async,
        CascadeTiming deleteOrphansTiming)
    {
        using var context = CreateContext();
        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;
        context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;

        var parent = context.Set<Parent>().Include(e => e.Children).Single();

        ClearLog();

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        foreach (var child in parent.Children)
        {
            context.Entry(child).State = state;
        }

        context.Entry(parent).State = state;

        Assert.True(collectionEntry.IsLoaded);

        var children = async
            ? await collectionEntry.Query().ToListAsync()
            : collectionEntry.Query().ToList();

        Assert.True(collectionEntry.IsLoaded);

        RecordLog();

        Assert.Equal(2, children.Count);

        if (state == EntityState.Detached)
        {
            Assert.Equal(2, parent.Children.Count());
            Assert.All(children, c => Assert.Null(c.Parent));
            Assert.Empty(context.ChangeTracker.Entries());
        }
        else
        {
            Assert.Equal(2, parent.Children.Count());
            Assert.All(children.Select(e => e.Parent), c => Assert.Same(parent, c));
            Assert.All(children, p => Assert.Contains(p, parent.Children));
            Assert.Equal(3, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_many_to_one_reference_to_principal_using_Query_already_loaded(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Set<Child>().Include(e => e.Parent).Single(e => e.Id == 12);

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        context.Entry(child.Parent).State = state;
        context.Entry(child).State = state;

        if (state != EntityState.Deleted) // FK is null
        {
            Assert.True(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleAsync()
                : referenceEntry.Query().Single();

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(parent);

            if (state != EntityState.Detached)
            {
                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.Children.Single());
            }

            Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_one_to_one_reference_to_principal_using_Query_already_loaded(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<Single>().Include(e => e.Parent).Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single.Parent).State = state;
        context.Entry(single).State = state;

        if (state != EntityState.Deleted) // FK is null
        {
            Assert.True(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleAsync()
                : referenceEntry.Query().Single();

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(parent);

            if (state != EntityState.Detached)
            {
                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.Single);
            }
        }

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Detached, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Detached, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Unchanged, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Modified, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Modified, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Deleted, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Deleted, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Detached, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Detached, false, CascadeTiming.OnSaveChanges)]
    public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_already_loaded(
        EntityState state,
        bool async,
        CascadeTiming deleteOrphansTiming)
    {
        using var context = CreateContext();
        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;
        context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;

        var parent = context.Set<Parent>().Include(e => e.Single).Single();

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        context.Entry(parent.Single).State = state;
        context.Entry(parent).State = state;

        Assert.True(referenceEntry.IsLoaded);

        var single = async
            ? await referenceEntry.Query().SingleAsync()
            : referenceEntry.Query().Single();

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        Assert.NotNull(single);

        if (state != EntityState.Detached)
        {
            Assert.Same(single, parent.Single);
            Assert.Same(parent, single.Parent);
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
    public virtual async Task Load_one_to_one_PK_to_PK_reference_to_principal_using_Query_already_loaded(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<SinglePkToPk>().Include(e => e.Parent).Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single.Parent).State = state;
        context.Entry(single).State = state;

        Assert.True(referenceEntry.IsLoaded);

        var parent = async
            ? await referenceEntry.Query().SingleAsync()
            : referenceEntry.Query().Single();

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        Assert.NotNull(parent);

        if (state != EntityState.Detached)
        {
            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.SinglePkToPk);
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
    public virtual async Task Load_one_to_one_PK_to_PK_reference_to_dependent_using_Query_already_loaded(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Include(e => e.SinglePkToPk).Single();

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.SinglePkToPk);

        context.Entry(parent.SinglePkToPk).State = state;
        context.Entry(parent).State = state;

        Assert.True(referenceEntry.IsLoaded);

        var single = async
            ? await referenceEntry.Query().SingleAsync()
            : referenceEntry.Query().Single();

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(single);

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Detached)
        {
            Assert.Same(single, parent.SinglePkToPk);
            Assert.Same(parent, single.Parent);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Detached, true)]
    [InlineData(EntityState.Detached, false)]
    public virtual async Task Load_collection_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var navigationEntry = context.Entry(parent).Navigation("Children");

        context.Entry(parent).State = state;

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

        Assert.Equal(2, parent.Children.Count());
        Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));

        Assert.Equal(state == EntityState.Detached ? 0 : 3, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_many_to_one_reference_to_principal_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Set<Child>().Single(e => e.Id == 12);

        ClearLog();

        var navigationEntry = context.Entry(child).Navigation("Parent");

        context.Entry(child).State = state;

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

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Deleted)
        {
            Assert.Same(child, child.Parent.Children.Single());
        }

        if (state != EntityState.Detached)
        {
            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            if (state == EntityState.Deleted)
            {
                Assert.Null(child.Parent);
                Assert.Null(parent.Children);
            }
            else
            {
                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.Children.Single());
            }
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
    public virtual async Task Load_one_to_one_reference_to_principal_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<Single>().Single();

        ClearLog();

        var navigationEntry = context.Entry(single).Navigation("Parent");

        context.Entry(single).State = state;

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

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Detached)
        {
            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            if (state == EntityState.Deleted)
            {
                Assert.Null(single.Parent);
                Assert.Null(parent.Single);
            }
            else
            {
                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.Single);
            }
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
    public virtual async Task Load_one_to_one_reference_to_dependent_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var navigationEntry = context.Entry(parent).Navigation("Single");

        context.Entry(parent).State = state;

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

        if (state != EntityState.Detached)
        {
            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var single = context.ChangeTracker.Entries<Single>().Single().Entity;

            Assert.Same(single, parent.Single);
            Assert.Same(parent, single.Parent);
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
    public virtual async Task Load_collection_using_Query_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var navigationEntry = context.Entry(parent).Navigation("Children");

        context.Entry(parent).State = state;

        Assert.False(navigationEntry.IsLoaded);

        // Issue #16429
        var children = async
            ? await navigationEntry.Query().ToListAsync<object>()
            : navigationEntry.Query().ToList<object>();

        Assert.False(navigationEntry.IsLoaded);

        RecordLog();

        Assert.Equal(2, children.Count);

        if (state == EntityState.Detached)
        {
            Assert.Empty(parent.Children);
            Assert.All(children, c => Assert.Null(((Child)c).Parent));
            Assert.Empty(context.ChangeTracker.Entries());
        }
        else
        {
            Assert.Equal(2, parent.Children.Count());
            Assert.All(children.Select(e => ((Child)e).Parent), c => Assert.Same(parent, c));
            Assert.All(children, p => Assert.Contains(p, parent.Children));
            Assert.Equal(3, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_many_to_one_reference_to_principal_using_Query_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Set<Child>().Single(e => e.Id == 12);

        ClearLog();

        var navigationEntry = context.Entry(child).Navigation("Parent");

        context.Entry(child).State = state;

        Assert.False(navigationEntry.IsLoaded);

        // Issue #16429
        var parent = async
            ? (await navigationEntry.Query().ToListAsync<object>()).Single()
            : navigationEntry.Query().ToList<object>().Single();

        RecordLog();

        Assert.NotNull(parent);

        if (state != EntityState.Detached)
        {
            if (state == EntityState.Deleted)
            {
                Assert.False(navigationEntry.IsLoaded);
                Assert.Null(child.Parent);
                Assert.Null(((Parent)parent).Children);
            }
            else
            {
                Assert.True(navigationEntry.IsLoaded);
                Assert.Same(parent, child.Parent);
                Assert.Same(child, ((Parent)parent).Children.Single());
            }

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_one_to_one_reference_to_principal_using_Query_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<Single>().Single();

        ClearLog();

        var navigationEntry = context.Entry(single).Navigation("Parent");

        context.Entry(single).State = state;

        Assert.False(navigationEntry.IsLoaded);

        // Issue #16429
        var parent = async
            ? (await navigationEntry.Query().ToListAsync<object>()).Single()
            : navigationEntry.Query().ToList<object>().Single();

        RecordLog();

        Assert.NotNull(parent);

        if (state != EntityState.Detached)
        {
            if (state == EntityState.Deleted)
            {
                Assert.False(navigationEntry.IsLoaded);
                Assert.Null(single.Parent);
                Assert.Null(((Parent)parent).Single);
            }
            else
            {
                Assert.True(navigationEntry.IsLoaded);
                Assert.Same(parent, single.Parent);
                Assert.Same(single, ((Parent)parent).Single);
            }

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var navigationEntry = context.Entry(parent).Navigation("Single");

        context.Entry(parent).State = state;

        Assert.False(navigationEntry.IsLoaded);

        // Issue #16429
        var single = async
            ? (await navigationEntry.Query().ToListAsync<object>()).Single()
            : navigationEntry.Query().ToList<object>().Single();

        Assert.NotNull(single);

        if (state != EntityState.Detached)
        {
            Assert.True(navigationEntry.IsLoaded);

            RecordLog();

            Assert.Same(single, parent.Single);
            Assert.Same(parent, ((Single)single).Parent);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
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
        using var context = CreateContext();
        var parent = context.Attach(new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

        ClearLog();

        var navigationEntry = context.Entry(parent).Navigation("Children");

        context.Entry(parent).State = state;

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

        Assert.Empty(parent.Children);
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
    public virtual async Task Load_many_to_one_reference_to_principal_not_found_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(new Child { Id = 767, ParentId = 787 }).Entity;

        ClearLog();

        var navigationEntry = context.Entry(child).Navigation("Parent");

        context.Entry(child).State = state;

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

        Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
        Assert.Null(child.Parent);
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
    public virtual async Task Load_one_to_one_reference_to_principal_not_found_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(new Single { Id = 767, ParentId = 787 }).Entity;

        ClearLog();

        var navigationEntry = context.Entry(single).Navigation("Parent");

        context.Entry(single).State = state;

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

        Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());

        Assert.Null(single.Parent);
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
    public virtual async Task Load_one_to_one_reference_to_dependent_not_found_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Attach(new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

        ClearLog();

        var navigationEntry = context.Entry(parent).Navigation("Single");

        context.Entry(parent).State = state;

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

        Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());

        Assert.Null(parent.Single);
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
        using var context = CreateContext();
        var parent = context.Attach(new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

        ClearLog();

        var navigationEntry = context.Entry(parent).Navigation("Children");

        context.Entry(parent).State = state;

        Assert.False(navigationEntry.IsLoaded);

        // Issue #16429
        var children = async
            ? await navigationEntry.Query().ToListAsync<object>()
            : navigationEntry.Query().ToList<object>();

        Assert.False(navigationEntry.IsLoaded);

        RecordLog();

        Assert.Empty(children);
        Assert.Empty(parent.Children);

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
    public virtual async Task Load_many_to_one_reference_to_principal_using_Query_not_found_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(new Child { Id = 767, ParentId = 787 }).Entity;

        ClearLog();

        var navigationEntry = context.Entry(child).Navigation("Parent");

        context.Entry(child).State = state;

        Assert.False(navigationEntry.IsLoaded);

        // Issue #16429
        var parent = async
            ? (await navigationEntry.Query().ToListAsync<object>()).SingleOrDefault()
            : navigationEntry.Query().ToList<object>().SingleOrDefault();

        Assert.False(navigationEntry.IsLoaded);

        RecordLog();

        Assert.Null(parent);
        Assert.Null(child.Parent);

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
    public virtual async Task Load_one_to_one_reference_to_principal_using_Query_not_found_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(new Single { Id = 767, ParentId = 787 }).Entity;

        ClearLog();

        var navigationEntry = context.Entry(single).Navigation("Parent");

        context.Entry(single).State = state;

        Assert.False(navigationEntry.IsLoaded);

        // Issue #16429
        var parent = async
            ? (await navigationEntry.Query().ToListAsync<object>()).SingleOrDefault()
            : navigationEntry.Query().ToList<object>().SingleOrDefault();

        Assert.False(navigationEntry.IsLoaded);

        RecordLog();

        Assert.Null(parent);
        Assert.Null(single.Parent);

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
    public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_not_found_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Attach(new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

        ClearLog();

        var navigationEntry = context.Entry(parent).Navigation("Single");

        context.Entry(parent).State = state;

        Assert.False(navigationEntry.IsLoaded);

        // Issue #16429
        var single = async
            ? (await navigationEntry.Query().ToListAsync<object>()).SingleOrDefault()
            : navigationEntry.Query().ToList<object>().SingleOrDefault();

        Assert.False(navigationEntry.IsLoaded);

        RecordLog();

        Assert.Null(single);
        Assert.Null(parent.Single);

        Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Detached, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Detached, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Unchanged, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Modified, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Modified, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Deleted, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Deleted, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Detached, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Detached, false, CascadeTiming.OnSaveChanges)]
    public virtual async Task Load_collection_already_loaded_untyped(EntityState state, bool async, CascadeTiming deleteOrphansTiming)
    {
        using var context = CreateContext();
        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

        var parent = context.Set<Parent>().Include(e => e.Children).Single();

        ClearLog();

        var navigationEntry = context.Entry(parent).Navigation("Children");

        foreach (var child in parent.Children)
        {
            context.Entry(child).State = state;
        }

        context.Entry(parent).State = state;

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

        RecordLog();

        Assert.Equal(2, parent.Children.Count());
        Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));

        Assert.Equal(state == EntityState.Detached ? 0 : 3, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_many_to_one_reference_to_principal_already_loaded_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Set<Child>().Include(e => e.Parent).Single(e => e.Id == 12);

        ClearLog();

        var navigationEntry = context.Entry(child).Navigation("Parent");

        context.Entry(child.Parent).State = state;
        context.Entry(child).State = state;

        Assert.Equal(state != EntityState.Deleted, navigationEntry.IsLoaded);

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

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Detached && state != EntityState.Deleted)
        {
            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, child.Parent);
            Assert.Same(child, parent.Children.Single());
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
    public virtual async Task Load_one_to_one_reference_to_principal_already_loaded_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<Single>().Include(e => e.Parent).Single();

        ClearLog();

        var navigationEntry = context.Entry(single).Navigation("Parent");

        context.Entry(single.Parent).State = state;
        context.Entry(single).State = state;

        Assert.Equal(state != EntityState.Deleted, navigationEntry.IsLoaded);

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

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Detached && state != EntityState.Deleted)
        {
            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.Single);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Detached, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Detached, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Unchanged, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Modified, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Modified, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Deleted, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Deleted, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Detached, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Detached, false, CascadeTiming.OnSaveChanges)]
    public virtual async Task Load_one_to_one_reference_to_dependent_already_loaded_untyped(
        EntityState state,
        bool async,
        CascadeTiming deleteOrphansTiming)
    {
        using var context = CreateContext();
        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

        var parent = context.Set<Parent>().Include(e => e.Single).Single();

        ClearLog();

        var navigationEntry = context.Entry(parent).Navigation("Single");

        context.Entry(parent.Single).State = state;
        context.Entry(parent).State = state;

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

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Detached)
        {
            var single = context.ChangeTracker.Entries<Single>().Single().Entity;

            Assert.Same(single, parent.Single);
            Assert.Same(parent, single.Parent);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Detached, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Detached, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Unchanged, false, CascadeTiming.OnSaveChanges)]
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
        using var context = CreateContext();
        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;
        context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;

        var parent = context.Set<Parent>().Include(e => e.Children).Single();

        ClearLog();

        var navigationEntry = context.Entry(parent).Navigation("Children");

        foreach (var child in parent.Children)
        {
            context.Entry(child).State = state;
        }

        context.Entry(parent).State = state;

        Assert.True(navigationEntry.IsLoaded);

        // Issue #16429
        var children = async
            ? await navigationEntry.Query().ToListAsync<object>()
            : navigationEntry.Query().ToList<object>();

        Assert.True(navigationEntry.IsLoaded);

        RecordLog();

        Assert.Equal(2, children.Count);

        if (state == EntityState.Detached)
        {
            Assert.Equal(2, parent.Children.Count());
            Assert.All(children, c => Assert.Null(((Child)c).Parent));
            Assert.Empty(context.ChangeTracker.Entries());
        }
        else
        {
            Assert.Equal(2, parent.Children.Count());
            Assert.All(children.Select(e => ((Child)e).Parent), c => Assert.Same(parent, c));
            Assert.All(children, p => Assert.Contains(p, parent.Children));
            Assert.Equal(3, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_many_to_one_reference_to_principal_using_Query_already_loaded_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Set<Child>().Include(e => e.Parent).Single(e => e.Id == 12);

        ClearLog();

        var navigationEntry = context.Entry(child).Navigation("Parent");

        context.Entry(child.Parent).State = state;
        context.Entry(child).State = state;

        if (state != EntityState.Deleted) // FK is null
        {
            Assert.True(navigationEntry.IsLoaded);

            // Issue #16429
            var parent = async
                ? (await navigationEntry.Query().ToListAsync<object>()).Single()
                : navigationEntry.Query().ToList<object>().Single();

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(parent);

            if (state != EntityState.Detached)
            {
                Assert.Same(parent, child.Parent);
                Assert.Same(child, ((Parent)parent).Children.Single());
            }

            Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_one_to_one_reference_to_principal_using_Query_already_loaded_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<Single>().Include(e => e.Parent).Single();

        ClearLog();

        var navigationEntry = context.Entry(single).Navigation("Parent");

        context.Entry(single.Parent).State = state;
        context.Entry(single).State = state;

        if (state != EntityState.Deleted) // FK is null
        {
            Assert.True(navigationEntry.IsLoaded);

            // Issue #16429
            var parent = async
                ? (await navigationEntry.Query().ToListAsync<object>()).Single()
                : navigationEntry.Query().ToList<object>().Single();

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(parent);

            if (state != EntityState.Detached)
            {
                Assert.Same(parent, single.Parent);
                Assert.Same(single, ((Parent)parent).Single);
            }
        }

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Detached, true, CascadeTiming.Immediate)]
    [InlineData(EntityState.Detached, false, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Unchanged, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Modified, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Modified, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Deleted, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Deleted, false, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Detached, true, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Detached, false, CascadeTiming.OnSaveChanges)]
    public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_already_loaded_untyped(
        EntityState state,
        bool async,
        CascadeTiming deleteOrphansTiming)
    {
        using var context = CreateContext();
        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;
        context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;

        var parent = context.Set<Parent>().Include(e => e.Single).Single();

        ClearLog();

        var navigationEntry = context.Entry(parent).Navigation("Single");

        context.Entry(parent.Single).State = state;
        context.Entry(parent).State = state;

        Assert.True(navigationEntry.IsLoaded);

        // Issue #16429
        var single = async
            ? (await navigationEntry.Query().ToListAsync<object>()).Single()
            : navigationEntry.Query().ToList<object>().Single();

        Assert.True(navigationEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(single);

        if (state == EntityState.Detached)
        {
            Assert.NotSame(single, parent.Single);
            Assert.Null(((Single)single).Parent);
            Assert.Empty(context.ChangeTracker.Entries());
        }
        else
        {
            Assert.Same(single, parent.Single);
            Assert.Same(parent, ((Single)single).Parent);
            Assert.Equal(2, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_collection_alternate_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var collectionEntry = context.Entry(parent).Collection(e => e.ChildrenAk);

        context.Entry(parent).State = state;

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

        RecordLog();

        Assert.Equal(2, parent.ChildrenAk.Count());
        Assert.All(parent.ChildrenAk.Select(e => e.Parent), c => Assert.Same(parent, c));

        Assert.Equal(state == EntityState.Detached ? 0 : 3, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_many_to_one_reference_to_principal_alternate_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Set<ChildAk>().Single(e => e.Id == 32);

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        context.Entry(child).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Detached)
        {
            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            if (state == EntityState.Deleted)
            {
                Assert.Null(child.Parent);
                Assert.Null(parent.ChildrenAk);
            }
            else
            {
                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.ChildrenAk.Single());
            }
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
    public virtual async Task Load_one_to_one_reference_to_principal_alternate_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<SingleAk>().Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Detached)
        {
            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            if (state == EntityState.Deleted)
            {
                Assert.Null(single.Parent);
                Assert.Null(parent.SingleAk);
            }
            else
            {
                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.SingleAk);
            }
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
    public virtual async Task Load_one_to_one_reference_to_dependent_alternate_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.SingleAk);

        context.Entry(parent).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        if (state != EntityState.Detached)
        {
            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var single = context.ChangeTracker.Entries<SingleAk>().Single().Entity;

            Assert.Same(single, parent.SingleAk);
            Assert.Same(parent, single.Parent);
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
    public virtual async Task Load_collection_using_Query_alternate_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var collectionEntry = context.Entry(parent).Collection(e => e.ChildrenAk);

        context.Entry(parent).State = state;

        Assert.False(collectionEntry.IsLoaded);

        var children = async
            ? await collectionEntry.Query().ToListAsync()
            : collectionEntry.Query().ToList();

        Assert.False(collectionEntry.IsLoaded);

        RecordLog();

        Assert.Equal(2, children.Count);

        if (state == EntityState.Detached)
        {
            Assert.Null(parent.Children);
            Assert.All(children, c => Assert.Null(c.Parent));
            Assert.Equal(0, context.ChangeTracker.Entries().Count());
        }
        else
        {
            Assert.Equal(2, parent.ChildrenAk.Count());
            Assert.All(children.Select(e => e.Parent), c => Assert.Same(parent, c));
            Assert.All(children, p => Assert.Contains(p, parent.ChildrenAk));
            Assert.Equal(3, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_many_to_one_reference_to_principal_using_Query_alternate_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Set<ChildAk>().Single(e => e.Id == 32);

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        context.Entry(child).State = state;

        Assert.False(referenceEntry.IsLoaded);

        var parent = async
            ? await referenceEntry.Query().SingleAsync()
            : referenceEntry.Query().Single();

        RecordLog();

        Assert.NotNull(parent);

        if (state != EntityState.Detached)
        {
            if (state == EntityState.Deleted)
            {
                Assert.False(referenceEntry.IsLoaded);
                Assert.Null(child.Parent);
                Assert.Null(parent.ChildrenAk);
            }
            else
            {
                Assert.True(referenceEntry.IsLoaded);
                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.ChildrenAk.Single());
            }

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_one_to_one_reference_to_principal_using_Query_alternate_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<SingleAk>().Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.False(referenceEntry.IsLoaded);

        var parent = async
            ? await referenceEntry.Query().SingleAsync()
            : referenceEntry.Query().Single();

        RecordLog();

        Assert.NotNull(parent);

        if (state != EntityState.Detached)
        {
            if (state == EntityState.Deleted)
            {
                Assert.False(referenceEntry.IsLoaded);
                Assert.Null(single.Parent);
                Assert.Null(parent.SingleAk);
            }
            else
            {
                Assert.True(referenceEntry.IsLoaded);
                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.SingleAk);
            }

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_alternate_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.SingleAk);

        context.Entry(parent).State = state;

        Assert.False(referenceEntry.IsLoaded);

        var single = async
            ? await referenceEntry.Query().SingleAsync()
            : referenceEntry.Query().Single();

        Assert.NotNull(single);

        if (state != EntityState.Detached)
        {
            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Same(single, parent.SingleAk);
            Assert.Same(parent, single.Parent);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_many_to_one_reference_to_principal_null_FK_alternate_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(new ChildAk { Id = 767, ParentId = null }).Entity;

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        context.Entry(child).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
        Assert.Null(child.Parent);
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
    public virtual async Task Load_one_to_one_reference_to_principal_null_FK_alternate_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(new SingleAk { Id = 767, ParentId = null }).Entity;

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());

        Assert.Null(single.Parent);
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
    public virtual async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_alternate_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(new ChildAk { Id = 767, ParentId = null }).Entity;

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        context.Entry(child).State = state;

        Assert.False(referenceEntry.IsLoaded);

        var parent = async
            ? await referenceEntry.Query().SingleOrDefaultAsync()
            : referenceEntry.Query().SingleOrDefault();

        Assert.False(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Null(parent);
        Assert.Null(child.Parent);

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
    public virtual async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_alternate_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(new SingleAk { Id = 767, ParentId = null }).Entity;

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.False(referenceEntry.IsLoaded);

        var parent = async
            ? await referenceEntry.Query().SingleOrDefaultAsync()
            : referenceEntry.Query().SingleOrDefault();

        Assert.False(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Null(parent);
        Assert.Null(single.Parent);

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
    public virtual async Task Load_collection_shadow_fk(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var collectionEntry = context.Entry(parent).Collection(e => e.ChildrenShadowFk);

        context.Entry(parent).State = state;

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

        RecordLog();

        Assert.Equal(2, parent.ChildrenShadowFk.Count());
        Assert.All(parent.ChildrenShadowFk.Select(e => e.Parent), c => Assert.Same(parent, c));

        Assert.Equal(state == EntityState.Detached ? 0 : 3, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_many_to_one_reference_to_principal_shadow_fk(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Set<ChildShadowFk>().Single(e => e.Id == 52);

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        context.Entry(child).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (state == EntityState.Detached)
        {
            if (async)
            {
                Assert.Equal(
                    CoreStrings.CannotLoadDetachedShadow("Parent", "ChildShadowFk"),
                    (await Assert.ThrowsAsync<InvalidOperationException>(() => referenceEntry.LoadAsync())).Message);
            }
            else
            {
                Assert.Equal(
                    CoreStrings.CannotLoadDetachedShadow("Parent", "ChildShadowFk"),
                    Assert.Throws<InvalidOperationException>(() => referenceEntry.Load()).Message);
            }
        }
        else
        {
            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            if (state == EntityState.Deleted)
            {
                Assert.Null(child.Parent);
                Assert.Null(parent.ChildrenShadowFk);
            }
            else
            {
                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.ChildrenShadowFk.Single());
            }
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
    public virtual async Task Load_one_to_one_reference_to_principal_shadow_fk(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<SingleShadowFk>().Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (state == EntityState.Detached)
        {
            Assert.Equal(
                CoreStrings.CannotLoadDetachedShadow("Parent", "SingleShadowFk"),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    async () =>
                    {
                        if (async)
                        {
                            await referenceEntry.LoadAsync();
                        }
                        else
                        {
                            referenceEntry.Load();
                        }
                    })).Message);
        }
        else
        {
            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            if (state == EntityState.Deleted)
            {
                Assert.Null(single.Parent);
                Assert.Null(parent.SingleShadowFk);
            }
            else
            {
                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.SingleShadowFk);
            }
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
    public virtual async Task Load_one_to_one_reference_to_dependent_shadow_fk(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.SingleShadowFk);

        context.Entry(parent).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        if (state != EntityState.Detached)
        {
            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var single = context.ChangeTracker.Entries<SingleShadowFk>().Single().Entity;

            Assert.Same(single, parent.SingleShadowFk);
            Assert.Same(parent, single.Parent);
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
    public virtual async Task Load_collection_using_Query_shadow_fk(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var collectionEntry = context.Entry(parent).Collection(e => e.ChildrenShadowFk);

        context.Entry(parent).State = state;

        Assert.False(collectionEntry.IsLoaded);

        var children = async
            ? await collectionEntry.Query().ToListAsync()
            : collectionEntry.Query().ToList();

        Assert.False(collectionEntry.IsLoaded);

        RecordLog();

        Assert.Equal(2, children.Count);

        if (state == EntityState.Detached)
        {
            Assert.Empty(parent.ChildrenShadowFk);
            Assert.All(children, c => Assert.Null(c.Parent));
            Assert.Empty(context.ChangeTracker.Entries());
        }
        else
        {
            Assert.Equal(2, parent.ChildrenShadowFk.Count());
            Assert.All(children.Select(e => e.Parent), c => Assert.Same(parent, c));
            Assert.All(children, p => Assert.Contains(p, parent.ChildrenShadowFk));
            Assert.Equal(3, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_many_to_one_reference_to_principal_using_Query_shadow_fk(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Set<ChildShadowFk>().Single(e => e.Id == 52);

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        context.Entry(child).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (state == EntityState.Detached)
        {
            Assert.Equal(
                CoreStrings.CannotLoadDetachedShadow("Parent", "ChildShadowFk"),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    async () =>
                    {
                        _ = async
                            ? await referenceEntry.Query().SingleOrDefaultAsync()
                            : referenceEntry.Query().SingleOrDefault();
                    })).Message);
        }
        else
        {
            var parent = async
                ? await referenceEntry.Query().SingleAsync()
                : referenceEntry.Query().Single();

            Assert.Equal(state != EntityState.Deleted, referenceEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(parent);

            if (state == EntityState.Deleted)
            {
                Assert.Null(child.Parent);
                Assert.Null(parent.ChildrenShadowFk);
            }
            else
            {
                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.ChildrenShadowFk.Single());
            }

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_one_to_one_reference_to_principal_using_Query_shadow_fk(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<SingleShadowFk>().Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (state == EntityState.Detached)
        {
            Assert.Equal(
                CoreStrings.CannotLoadDetachedShadow("Parent", "SingleShadowFk"),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    async () =>
                    {
                        _ = async
                            ? await referenceEntry.Query().SingleAsync()
                            : referenceEntry.Query().Single();
                    })).Message);
        }
        else
        {
            var parent = async
                ? await referenceEntry.Query().SingleAsync()
                : referenceEntry.Query().Single();

            Assert.Equal(state != EntityState.Deleted, referenceEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(parent);

            if (state == EntityState.Deleted)
            {
                Assert.Null(single.Parent);
                Assert.Null(parent.SingleShadowFk);
            }
            else
            {
                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.SingleShadowFk);
            }

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_shadow_fk(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.SingleShadowFk);

        context.Entry(parent).State = state;

        Assert.False(referenceEntry.IsLoaded);

        var single = async
            ? await referenceEntry.Query().SingleAsync()
            : referenceEntry.Query().Single();

        Assert.NotNull(single);

        if (state != EntityState.Detached)
        {
            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Same(single, parent.SingleShadowFk);
            Assert.Same(parent, single.Parent);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_many_to_one_reference_to_principal_null_FK_shadow_fk(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(new ChildShadowFk { Id = 767 }).Entity;
        context.Entry(child).Property("ParentId").CurrentValue = null;

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        context.Entry(child).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (state == EntityState.Detached)
        {
            if (async)
            {
                Assert.Equal(
                    CoreStrings.CannotLoadDetachedShadow("Parent", "ChildShadowFk"),
                    (await Assert.ThrowsAsync<InvalidOperationException>(() => referenceEntry.LoadAsync())).Message);
            }
            else
            {
                Assert.Equal(
                    CoreStrings.CannotLoadDetachedShadow("Parent", "ChildShadowFk"),
                    Assert.Throws<InvalidOperationException>(() => referenceEntry.Load()).Message);
            }
        }
        else
        {
            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
            Assert.Null(child.Parent);
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
    public virtual async Task Load_one_to_one_reference_to_principal_null_FK_shadow_fk(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(new SingleShadowFk { Id = 767 }).Entity;
        context.Entry(single).Property("ParentId").CurrentValue = null;

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (state == EntityState.Detached)
        {
            Assert.Equal(
                CoreStrings.CannotLoadDetachedShadow("Parent", "SingleShadowFk"),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    async () =>
                    {
                        if (async)
                        {
                            await referenceEntry.LoadAsync();
                        }
                        else
                        {
                            referenceEntry.Load();
                        }
                    })).Message);
        }
        else
        {
            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());

            Assert.Null(single.Parent);
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
    public virtual async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(new ChildShadowFk { Id = 767 }).Entity;
        context.Entry(child).Property("ParentId").CurrentValue = null;

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        context.Entry(child).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (state == EntityState.Detached)
        {
            Assert.Equal(
                CoreStrings.CannotLoadDetachedShadow("Parent", "ChildShadowFk"),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    async () =>
                    {
                        _ = async
                            ? await referenceEntry.Query().SingleOrDefaultAsync()
                            : referenceEntry.Query().SingleOrDefault();
                    })).Message);
        }
        else
        {
            var parent = async
                ? await referenceEntry.Query().SingleOrDefaultAsync()
                : referenceEntry.Query().SingleOrDefault();

            Assert.False(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Null(parent);
            Assert.Null(child.Parent);

            Assert.Single(context.ChangeTracker.Entries());
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
    public virtual async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(new SingleShadowFk { Id = 767 }).Entity;
        context.Entry(single).Property("ParentId").CurrentValue = null;

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (state == EntityState.Detached)
        {
            Assert.Equal(
                CoreStrings.CannotLoadDetachedShadow("Parent", "SingleShadowFk"),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    async () =>
                    {
                        _ = async
                            ? await referenceEntry.Query().SingleOrDefaultAsync()
                            : referenceEntry.Query().SingleOrDefault();
                    })).Message);
        }
        else
        {
            var parent = async
                ? await referenceEntry.Query().SingleOrDefaultAsync()
                : referenceEntry.Query().SingleOrDefault();

            Assert.False(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Null(parent);
            Assert.Null(single.Parent);

            Assert.Single(context.ChangeTracker.Entries());
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
    public virtual async Task Load_collection_composite_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var collectionEntry = context.Entry(parent).Collection(e => e.ChildrenCompositeKey);

        context.Entry(parent).State = state;

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

        RecordLog();

        Assert.Equal(2, parent.ChildrenCompositeKey.Count());
        Assert.All(parent.ChildrenCompositeKey.Select(e => e.Parent), c => Assert.Same(parent, c));

        Assert.Equal(state == EntityState.Detached ? 0 : 3, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_many_to_one_reference_to_principal_composite_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Set<ChildCompositeKey>().Single(e => e.Id == 52);

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        context.Entry(child).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Detached)
        {
            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            if (state == EntityState.Deleted)
            {
                Assert.Null(child.Parent);
                Assert.Null(parent.ChildrenCompositeKey);
            }
            else
            {
                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.ChildrenCompositeKey.Single());
            }
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
    public virtual async Task Load_one_to_one_reference_to_principal_composite_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<SingleCompositeKey>().Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Detached)
        {
            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            if (state == EntityState.Deleted)
            {
                Assert.Null(single.Parent);
                Assert.Null(parent.SingleCompositeKey);
            }
            else
            {
                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.SingleCompositeKey);
            }
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
    public virtual async Task Load_one_to_one_reference_to_dependent_composite_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.SingleCompositeKey);

        context.Entry(parent).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        if (state != EntityState.Detached)
        {
            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var single = context.ChangeTracker.Entries<SingleCompositeKey>().Single().Entity;

            Assert.Same(single, parent.SingleCompositeKey);
            Assert.Same(parent, single.Parent);
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
    public virtual async Task Load_collection_using_Query_composite_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var collectionEntry = context.Entry(parent).Collection(e => e.ChildrenCompositeKey);

        context.Entry(parent).State = state;

        Assert.False(collectionEntry.IsLoaded);

        var children = async
            ? await collectionEntry.Query().ToListAsync()
            : collectionEntry.Query().ToList();

        Assert.False(collectionEntry.IsLoaded);

        RecordLog();

        Assert.Equal(2, children.Count);

        if (state == EntityState.Detached)
        {
            Assert.Empty(parent.ChildrenCompositeKey);
            Assert.All(children, c => Assert.Null(c.Parent));
            Assert.Empty(context.ChangeTracker.Entries());
        }
        else
        {
            Assert.Equal(2, parent.ChildrenCompositeKey.Count());
            Assert.All(children.Select(e => e.Parent), c => Assert.Same(parent, c));
            Assert.All(children, p => Assert.Contains(p, parent.ChildrenCompositeKey));
            Assert.Equal(3, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_many_to_one_reference_to_principal_using_Query_composite_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Set<ChildCompositeKey>().Single(e => e.Id == 52);

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        context.Entry(child).State = state;

        Assert.False(referenceEntry.IsLoaded);

        var parent = async
            ? await referenceEntry.Query().SingleAsync()
            : referenceEntry.Query().Single();

        RecordLog();

        Assert.NotNull(parent);

        if (state != EntityState.Detached)
        {
            if (state == EntityState.Deleted)
            {
                Assert.False(referenceEntry.IsLoaded);
                Assert.Null(child.Parent);
                Assert.Null(parent.ChildrenCompositeKey);
            }
            else
            {
                Assert.True(referenceEntry.IsLoaded);
                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.ChildrenCompositeKey.Single());
            }

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_one_to_one_reference_to_principal_using_Query_composite_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<SingleCompositeKey>().Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.False(referenceEntry.IsLoaded);

        var parent = async
            ? await referenceEntry.Query().SingleAsync()
            : referenceEntry.Query().Single();

        RecordLog();

        Assert.NotNull(parent);

        if (state != EntityState.Detached)
        {
            if (state == EntityState.Deleted)
            {
                Assert.False(referenceEntry.IsLoaded);
                Assert.Null(single.Parent);
                Assert.Null(parent.SingleCompositeKey);
            }
            else
            {
                Assert.True(referenceEntry.IsLoaded);
                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.SingleCompositeKey);
            }

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_composite_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.SingleCompositeKey);

        context.Entry(parent).State = state;

        Assert.False(referenceEntry.IsLoaded);

        var single = async
            ? await referenceEntry.Query().SingleAsync()
            : referenceEntry.Query().Single();

        Assert.NotNull(single);

        if (state != EntityState.Detached)
        {
            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Same(single, parent.SingleCompositeKey);
            Assert.Same(parent, single.Parent);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
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
    public virtual async Task Load_many_to_one_reference_to_principal_null_FK_composite_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(new ChildCompositeKey { Id = 767, ParentId = 567 }).Entity;

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        context.Entry(child).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
        Assert.Null(child.Parent);
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
    public virtual async Task Load_one_to_one_reference_to_principal_null_FK_composite_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(new SingleCompositeKey { Id = 767, ParentAlternateId = "Boot" }).Entity;

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.False(referenceEntry.IsLoaded);

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());

        Assert.Null(single.Parent);
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
    public virtual async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_composite_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(new ChildCompositeKey { Id = 767, ParentAlternateId = "Boot" }).Entity;

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        context.Entry(child).State = state;

        Assert.False(referenceEntry.IsLoaded);

        var parent = async
            ? await referenceEntry.Query().SingleOrDefaultAsync()
            : referenceEntry.Query().SingleOrDefault();

        Assert.False(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Null(parent);
        Assert.Null(child.Parent);

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
    public virtual async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_composite_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(new SingleCompositeKey { Id = 767, ParentId = 567 }).Entity;

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.False(referenceEntry.IsLoaded);

        var parent = async
            ? await referenceEntry.Query().SingleOrDefaultAsync()
            : referenceEntry.Query().SingleOrDefault();

        Assert.False(referenceEntry.IsLoaded);

        RecordLog();

        Assert.Null(parent);
        Assert.Null(single.Parent);

        Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public virtual void Can_change_IsLoaded_flag_for_collection()
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Single();

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        Assert.False(collectionEntry.IsLoaded);

        collectionEntry.IsLoaded = true;

        Assert.True(collectionEntry.IsLoaded);

        collectionEntry.Load();

        Assert.Empty(parent.Children);
        Assert.Single(context.ChangeTracker.Entries());

        Assert.True(collectionEntry.IsLoaded);

        collectionEntry.IsLoaded = false;

        Assert.False(collectionEntry.IsLoaded);

        collectionEntry.Load();

        Assert.Equal(2, parent.Children.Count());
        Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));
        Assert.Equal(3, context.ChangeTracker.Entries().Count());

        Assert.True(collectionEntry.IsLoaded);
    }

    [ConditionalFact]
    public virtual void Can_change_IsLoaded_flag_for_reference_only_if_null()
    {
        using var context = CreateContext();
        var child = context.Set<Child>().Single(e => e.Id == 12);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        referenceEntry.IsLoaded = true;

        Assert.True(referenceEntry.IsLoaded);

        referenceEntry.Load();

        Assert.True(referenceEntry.IsLoaded);

        Assert.Single(context.ChangeTracker.Entries());

        referenceEntry.IsLoaded = true;

        referenceEntry.IsLoaded = false;

        referenceEntry.Load();

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

        Assert.True(referenceEntry.IsLoaded);

        Assert.Equal(
            CoreStrings.ReferenceMustBeLoaded("Parent", typeof(Child).Name),
            Assert.Throws<InvalidOperationException>(() => referenceEntry.IsLoaded = false).Message);
    }

    [ConditionalFact] // Issue #27497
    public virtual void Fixup_reference_after_FK_change_without_DetectChanges()
    {
        using var context = CreateContext();

        var child = context.Attach(new Child { Id = 274, ParentId = 707 }).Entity;
        var newParent = context.Attach(new Parent { Id = 497 }).Entity;

        child.Parent = newParent;

        var oldParent = context.Set<Parent>().Single(e => e.Id == 707);

        Assert.Same(newParent, child.Parent);
        Assert.Equal(497, child.ParentId);
    }

    [ConditionalFact] // Issue #27497
    public virtual void Fixup_one_to_one_reference_after_FK_change_without_DetectChanges()
    {
        using var context = CreateContext();

        var child = context.Attach(new Single { Id = 274, ParentId = 707 }).Entity;
        var newParent = context.Attach(new Parent { Id = 497 }).Entity;

        child.Parent = newParent;

        var oldParent = context.Set<Parent>().Single(e => e.Id == 707);

        Assert.Same(newParent, child.Parent);
        Assert.Equal(497, child.ParentId);
    }

    [ConditionalFact]
    public virtual void Setting_navigation_to_null_is_detected_by_local_DetectChanges() // Issue #26937
    {
        using var context = CreateContext();
        context.ChangeTracker.AutoDetectChangesEnabled = false;

        var child = context.Attach(new RequiredSingle { Id = 274 }).Entity;
        var newParent = new Parent { Id = 497 };
        child.Parent = newParent;

        var childEntry = context.Entry(child);
        childEntry.DetectChanges();

        Assert.Same(newParent, child.Parent);
        Assert.Equal(newParent.Id, child.ParentId);
        Assert.Equal(EntityState.Modified, childEntry.State);

        child.Parent = null;
        childEntry.DetectChanges();

        Assert.Null(child.Parent);
        Assert.Equal(EntityState.Deleted, childEntry.State);
    }

    private static void SetState(
        DbContext context,
        object entity,
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool isAttached = false)
    {
        if (isAttached && state == EntityState.Detached
            || state != (queryTrackingBehavior == QueryTrackingBehavior.TrackAll ? EntityState.Unchanged : EntityState.Detached))
        {
            context.Entry(entity).State = state;
        }
    }

    protected class Parent
    {
        private IEnumerable<Child> _children;
        private SinglePkToPk _singlePkToPk;
        private Single _single;
        private RequiredSingle _requiredSingle;
        private IEnumerable<ChildAk> _childrenAk;
        private SingleAk _singleAk;
        private IEnumerable<ChildShadowFk> _childrenShadowFk;
        private SingleShadowFk _singleShadowFk;
        private IEnumerable<ChildCompositeKey> _childrenCompositeKey;
        private SingleCompositeKey _singleCompositeKey;

        public ILazyLoader Loader { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string AlternateId { get; set; }

        public IEnumerable<Child> Children
        {
            get => Loader.Load(this, ref _children);
            set => _children = value;
        }

        public async Task<IEnumerable<Child>> LazyLoadChildren(bool async)
        {
            if (async)
            {
                await Loader.LoadAsync(this, default, nameof(Children));
                return _children;
            }

            return Children;
        }

        public SinglePkToPk SinglePkToPk
        {
            get => Loader.Load(this, ref _singlePkToPk);
            set => _singlePkToPk = value;
        }

        public Single Single
        {
            get => Loader.Load(this, ref _single);
            set => _single = value;
        }

        public async Task<Single> LazyLoadSingle(bool async)
        {
            if (async)
            {
                await Loader.LoadAsync(this, default, nameof(Single));
                return _single;
            }

            return Single;
        }

        public RequiredSingle RequiredSingle
        {
            get => Loader.Load(this, ref _requiredSingle);
            set => _requiredSingle = value;
        }

        public IEnumerable<ChildAk> ChildrenAk
        {
            get => Loader.Load(this, ref _childrenAk);
            set => _childrenAk = value;
        }

        public SingleAk SingleAk
        {
            get => Loader.Load(this, ref _singleAk);
            set => _singleAk = value;
        }

        public IEnumerable<ChildShadowFk> ChildrenShadowFk
        {
            get => Loader.Load(this, ref _childrenShadowFk);
            set => _childrenShadowFk = value;
        }

        public SingleShadowFk SingleShadowFk
        {
            get => Loader.Load(this, ref _singleShadowFk);
            set => _singleShadowFk = value;
        }

        public IEnumerable<ChildCompositeKey> ChildrenCompositeKey
        {
            get => Loader.Load(this, ref _childrenCompositeKey);
            set => _childrenCompositeKey = value;
        }

        public SingleCompositeKey SingleCompositeKey
        {
            get => Loader.Load(this, ref _singleCompositeKey);
            set => _singleCompositeKey = value;
        }
    }

    protected class Child
    {
        private Parent _parent;

        private ILazyLoader Loader { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int? ParentId { get; set; }

        public Parent Parent
        {
            get => Loader.Load(this, ref _parent);
            set => _parent = value;
        }

        public async Task<Parent> LazyLoadParent(bool async)
        {
            if (async)
            {
                await Loader.LoadAsync(this, default, nameof(Parent));
                return _parent;
            }

            return Parent;
        }
    }

    protected class SinglePkToPk
    {
        private Parent _parent;

        private ILazyLoader Loader { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public Parent Parent
        {
            get => Loader.Load(this, ref _parent);
            set => _parent = value;
        }
    }

    protected class Single
    {
        private Parent _parent;

        private ILazyLoader Loader { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int? ParentId { get; set; }

        public Parent Parent
        {
            get => Loader.Load(this, ref _parent);
            set => _parent = value;
        }

        public async Task<Parent> LazyLoadParent(bool async)
        {
            if (async)
            {
                await Loader.LoadAsync(this, default, nameof(Parent));
                return _parent;
            }

            return Parent;
        }
    }

    protected class RequiredSingle
    {
        private Parent _parent;

        private ILazyLoader Loader { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int ParentId { get; set; }

        public Parent Parent
        {
            get => Loader.Load(this, ref _parent);
            set => _parent = value;
        }
    }

    protected class ChildAk
    {
        private Parent _parent;

        private ILazyLoader Loader { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string ParentId { get; set; }

        public Parent Parent
        {
            get => Loader.Load(this, ref _parent);
            set => _parent = value;
        }
    }

    protected class SingleAk
    {
        private Parent _parent;

        private ILazyLoader Loader { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string ParentId { get; set; }

        public Parent Parent
        {
            get => Loader.Load(this, ref _parent);
            set => _parent = value;
        }
    }

    protected class ChildShadowFk
    {
        private Parent _parent;

        private ILazyLoader Loader { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public Parent Parent
        {
            get => Loader.Load(this, ref _parent);
            set => _parent = value;
        }
    }

    protected class SingleShadowFk
    {
        private Parent _parent;

        private ILazyLoader Loader { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public Parent Parent
        {
            get => Loader.Load(this, ref _parent);
            set => _parent = value;
        }
    }

    protected class ChildCompositeKey
    {
        private Parent _parent;

        private ILazyLoader Loader { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int? ParentId { get; set; }
        public string ParentAlternateId { get; set; }

        public Parent Parent
        {
            get => Loader.Load(this, ref _parent);
            set => _parent = value;
        }
    }

    protected class SingleCompositeKey
    {
        private Parent _parent;

        private ILazyLoader Loader { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int? ParentId { get; set; }
        public string ParentAlternateId { get; set; }

        public Parent Parent
        {
            get => Loader.Load(this, ref _parent);
            set => _parent = value;
        }
    }

    protected abstract class RootClass
    {
        protected RootClass(Action<object, string> lazyLoader)
        {
            LazyLoader = lazyLoader;
        }

        protected RootClass()
        {
        }

        public int Id { get; set; }

        protected Action<object, string> LazyLoader { get; }
    }

    protected class Deposit : RootClass
    {
        private Deposit(Action<object, string> lazyLoader)
            : base(lazyLoader)
        {
        }

        public Deposit()
        {
        }
    }

    protected abstract class Product : RootClass
    {
        protected Product(Action<object, string> lazyLoader)
            : base(lazyLoader)
        {
        }

        protected Product()
        {
        }

        public int? DepositID { get; set; }

        private Deposit _deposit;

        public Deposit Deposit
        {
            get => LazyLoader.Load(this, ref _deposit);
            set => _deposit = value;
        }
    }

    protected class SimpleProduct : Product
    {
        private SimpleProduct(Action<object, string> lazyLoader)
            : base(lazyLoader)
        {
        }

        public SimpleProduct()
        {
        }
    }

    protected class OptionalChildView
    {
        private readonly Action<object, string> _loader;
        private RootClass _root;

        public OptionalChildView()
        {
        }

        public OptionalChildView(Action<object, string> lazyLoader)
        {
            _loader = lazyLoader;
        }

        public int? RootId { get; set; }

        public RootClass Root
        {
            get => _loader.Load(this, ref _root);
            set => _root = value;
        }
    }

    protected class RequiredChildView
    {
        private readonly Action<object, string> _loader;
        private RootClass _root;

        public RequiredChildView()
        {
        }

        public RequiredChildView(Action<object, string> lazyLoader)
        {
            _loader = lazyLoader;
        }

        public int RootId { get; set; }

        public RootClass Root
        {
            get => _loader.Load(this, ref _root);
            set => _root = value;
        }
    }

    protected class ParentFullLoaderByConstructor
    {
        private readonly ILazyLoader _loader;
        private IEnumerable<ChildFullLoaderByConstructor> _children;
        private SingleFullLoaderByConstructor _single;

        public ParentFullLoaderByConstructor()
        {
        }

        private ParentFullLoaderByConstructor(ILazyLoader loader)
        {
            _loader = loader;
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public IEnumerable<ChildFullLoaderByConstructor> Children
        {
            get => _loader.Load(this, ref _children);
            set => _children = value;
        }

        public async Task<IEnumerable<ChildFullLoaderByConstructor>> LazyLoadChildren(bool async)
        {
            if (async)
            {
                await _loader.LoadAsync(this, default, nameof(Children));
                return _children;
            }

            return Children;
        }

        public SingleFullLoaderByConstructor Single
        {
            get => _loader.Load(this, ref _single);
            set => _single = value;
        }

        public async Task<SingleFullLoaderByConstructor> LazyLoadSingle(bool async)
        {
            if (async)
            {
                await _loader.LoadAsync(this, default, nameof(Single));
                return _single;
            }

            return Single;
        }
    }

    protected class ChildFullLoaderByConstructor
    {
        private readonly ILazyLoader _loader;
        private ParentFullLoaderByConstructor _parent;

        public ChildFullLoaderByConstructor()
        {
        }

        public ChildFullLoaderByConstructor(ILazyLoader loader)
        {
            _loader = loader;
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int? ParentId { get; set; }

        public ParentFullLoaderByConstructor Parent
        {
            get => _loader.Load(this, ref _parent);
            set => _parent = value;
        }

        public async Task<ParentFullLoaderByConstructor> LazyLoadParent(bool async)
        {
            if (async)
            {
                await _loader.LoadAsync(this, default, nameof(Parent));
                return _parent;
            }

            return Parent;
        }
    }

    protected class SingleFullLoaderByConstructor
    {
        private readonly ILazyLoader _loader;
        private ParentFullLoaderByConstructor _parent;

        public SingleFullLoaderByConstructor()
        {
        }

        public SingleFullLoaderByConstructor(ILazyLoader loader)
        {
            _loader = loader;
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int? ParentId { get; set; }

        public ParentFullLoaderByConstructor Parent
        {
            get => _loader.Load(this, ref _parent);
            set => _parent = value;
        }

        public async Task<ParentFullLoaderByConstructor> LazyLoadParent(bool async)
        {
            if (async)
            {
                await _loader.LoadAsync(this, default, nameof(Parent));
                return _parent;
            }

            return Parent;
        }
    }

    protected class ParentDelegateLoaderByConstructor
    {
        private readonly Action<object, string> _loader;
        private IEnumerable<ChildDelegateLoaderByConstructor> _children;
        private SingleDelegateLoaderByConstructor _single;

        public ParentDelegateLoaderByConstructor()
        {
        }

        private ParentDelegateLoaderByConstructor(Action<object, string> lazyLoader)
        {
            _loader = lazyLoader;
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public IEnumerable<ChildDelegateLoaderByConstructor> Children
        {
            get => _loader.Load(this, ref _children);
            set => _children = value;
        }

        public SingleDelegateLoaderByConstructor Single
        {
            get => _single ?? _loader.Load(this, ref _single);
            set => _single = value;
        }
    }

    protected class ChildDelegateLoaderByConstructor
    {
        private readonly Action<object, string> _loader;
        private ParentDelegateLoaderByConstructor _parent;
        private int? _parentId;

        public ChildDelegateLoaderByConstructor()
        {
        }

        private ChildDelegateLoaderByConstructor(Action<object, string> lazyLoader)
        {
            _loader = lazyLoader;
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int? ParentId
        {
            get => _parentId;
            set
            {
                if (_parentId != value)
                {
                    _parentId = value;
                    _parent = null;
                }
            }
        }

        public ParentDelegateLoaderByConstructor Parent
        {
            get => _parent ?? _loader.Load(this, ref _parent);
            set => _parent = value;
        }
    }

    protected class SingleDelegateLoaderByConstructor
    {
        private readonly Action<object, string> _loader;
        private ParentDelegateLoaderByConstructor _parent;
        private int? _parentId;

        public SingleDelegateLoaderByConstructor()
        {
        }

        private SingleDelegateLoaderByConstructor(Action<object, string> lazyLoader)
        {
            _loader = lazyLoader;
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int? ParentId
        {
            get => _parentId;
            set
            {
                if (_parentId != value)
                {
                    _parentId = value;
                    _parent = null;
                }
            }
        }

        public ParentDelegateLoaderByConstructor Parent
        {
            get => _parent ?? _loader.Load(this, ref _parent);
            set => _parent = value;
        }
    }

    protected class ParentDelegateLoaderByProperty
    {
        private IEnumerable<ChildDelegateLoaderByProperty> _children;
        private SingleDelegateLoaderByProperty _single;

        private Action<object, string> LazyLoader { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public IEnumerable<ChildDelegateLoaderByProperty> Children
        {
            get => LazyLoader.Load(this, ref _children);
            set => _children = value;
        }

        public SingleDelegateLoaderByProperty Single
        {
            get => _single ?? LazyLoader.Load(this, ref _single);
            set => _single = value;
        }
    }

    protected class ChildDelegateLoaderByProperty
    {
        private ParentDelegateLoaderByProperty _parent;
        private int? _parentId;

        private Action<object, string> LazyLoader { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int? ParentId
        {
            get => _parentId;
            set
            {
                if (_parentId != value)
                {
                    _parentId = value;
                    _parent = null;
                }
            }
        }

        public ParentDelegateLoaderByProperty Parent
        {
            get => _parent ?? LazyLoader.Load(this, ref _parent);
            set => _parent = value;
        }
    }

    protected class SingleDelegateLoaderByProperty
    {
        private ParentDelegateLoaderByProperty _parent;
        private int? _parentId;

        private Action<object, string> LazyLoader { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int? ParentId
        {
            get => _parentId;
            set
            {
                if (_parentId != value)
                {
                    _parentId = value;
                    _parent = null;
                }
            }
        }

        public ParentDelegateLoaderByProperty Parent
        {
            get => _parent ?? LazyLoader.Load(this, ref _parent);
            set => _parent = value;
        }
    }

    protected class ParentDelegateLoaderWithStateByProperty
    {
        private IEnumerable<ChildDelegateLoaderWithStateByProperty> _children;
        private SingleDelegateLoaderWithStateByProperty _single;

        private object LazyLoaderState { get; set; }
        private Action<object, string> LazyLoader { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public IEnumerable<ChildDelegateLoaderWithStateByProperty> Children
        {
            get => LazyLoader.Load(this, ref _children);
            set => _children = value;
        }

        public SingleDelegateLoaderWithStateByProperty Single
        {
            get => _single ?? LazyLoader.Load(this, ref _single);
            set => _single = value;
        }
    }

    protected class ChildDelegateLoaderWithStateByProperty
    {
        private ParentDelegateLoaderWithStateByProperty _parent;
        private int? _parentId;

        private object LazyLoaderState { get; set; }
        private Action<object, string> LazyLoader { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int? ParentId
        {
            get => _parentId;
            set
            {
                if (_parentId != value)
                {
                    _parentId = value;
                    _parent = null;
                }
            }
        }

        public ParentDelegateLoaderWithStateByProperty Parent
        {
            get => _parent ?? LazyLoader.Load(this, ref _parent);
            set => _parent = value;
        }
    }

    protected class SingleDelegateLoaderWithStateByProperty
    {
        private ParentDelegateLoaderWithStateByProperty _parent;
        private int? _parentId;

        private object LazyLoaderState { get; set; }
        private Action<object, string> LazyLoader { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int? ParentId
        {
            get => _parentId;
            set
            {
                if (_parentId != value)
                {
                    _parentId = value;
                    _parent = null;
                }
            }
        }

        public ParentDelegateLoaderWithStateByProperty Parent
        {
            get => _parent ?? LazyLoader.Load(this, ref _parent);
            set => _parent = value;
        }
    }

    protected DbContext CreateContext(bool lazyLoadingEnabled = false, bool noTracking = false)
    {
        var context = Fixture.CreateContext();
        context.ChangeTracker.LazyLoadingEnabled = lazyLoadingEnabled;

        context.ChangeTracker.QueryTrackingBehavior = noTracking
            ? QueryTrackingBehavior.NoTracking
            : QueryTrackingBehavior.TrackAll;

        return context;
    }

    protected virtual void ClearLog()
    {
    }

    protected virtual void RecordLog()
    {
    }

    protected class ChangeDetectorProxy(
        IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> logger,
        ILoggingOptions loggingOptions) : ChangeDetector(logger, loggingOptions)
    {
        public bool DetectChangesCalled { get; set; }

        public override void DetectChanges(IStateManager stateManager)
        {
            DetectChangesCalled = true;

            base.DetectChanges(stateManager);
        }
    }

    public abstract class LoadFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName
            => "LoadTest";

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection.AddScoped<IChangeDetector, ChangeDetectorProxy>());

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<SingleShadowFk>()
                .Property<int?>("ParentId");

            modelBuilder.Entity<Parent>(
                b =>
                {
                    b.Property(e => e.AlternateId).ValueGeneratedOnAdd();

                    b.HasMany<Child>(nameof(Parent.Children))
                        .WithOne(nameof(Child.Parent))
                        .HasForeignKey(e => e.ParentId);

                    b.HasOne<SinglePkToPk>(nameof(Parent.SinglePkToPk))
                        .WithOne(nameof(SinglePkToPk.Parent))
                        .HasForeignKey<SinglePkToPk>(e => e.Id)
                        .IsRequired();

                    b.HasOne<Single>(nameof(Parent.Single))
                        .WithOne(e => e.Parent)
                        .HasForeignKey<Single>(e => e.ParentId);

                    b.HasOne<RequiredSingle>(nameof(Parent.RequiredSingle))
                        .WithOne(e => e.Parent)
                        .HasForeignKey<RequiredSingle>(e => e.ParentId);

                    b.HasMany<ChildAk>(nameof(Parent.ChildrenAk))
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(e => e.AlternateId)
                        .HasForeignKey(e => e.ParentId);

                    b.HasOne<SingleAk>(nameof(Parent.SingleAk))
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey<Parent>(e => e.AlternateId)
                        .HasForeignKey<SingleAk>(e => e.ParentId);

                    b.HasMany(e => e.ChildrenShadowFk)
                        .WithOne(nameof(ChildShadowFk.Parent))
                        .HasPrincipalKey(e => e.Id)
                        .HasForeignKey("ParentId");

                    b.HasOne<SingleShadowFk>(nameof(Parent.SingleShadowFk))
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey<Parent>(e => e.Id)
                        .HasForeignKey<SingleShadowFk>("ParentId");

                    b.HasMany(e => e.ChildrenCompositeKey)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(
                            e => new { e.AlternateId, e.Id })
                        .HasForeignKey(
                            e => new { e.ParentAlternateId, e.ParentId });

                    b.HasOne<SingleCompositeKey>(nameof(Parent.SingleCompositeKey))
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey<Parent>(
                            e => new { e.AlternateId, e.Id })
                        .HasForeignKey<SingleCompositeKey>(
                            e => new { e.ParentAlternateId, e.ParentId });
                });

            modelBuilder.Entity<ParentFullLoaderByConstructor>(
                b =>
                {
                    b.HasMany<ChildFullLoaderByConstructor>(nameof(ParentFullLoaderByConstructor.Children))
                        .WithOne(nameof(ChildFullLoaderByConstructor.Parent))
                        .HasForeignKey(e => e.ParentId);

                    b.HasOne<SingleFullLoaderByConstructor>(nameof(ParentFullLoaderByConstructor.Single))
                        .WithOne(e => e.Parent)
                        .HasForeignKey<SingleFullLoaderByConstructor>(e => e.ParentId);
                });

            modelBuilder.Entity<ParentDelegateLoaderByConstructor>(
                b =>
                {
                    b.HasMany<ChildDelegateLoaderByConstructor>(nameof(ParentDelegateLoaderByConstructor.Children))
                        .WithOne(nameof(ChildDelegateLoaderByConstructor.Parent))
                        .HasForeignKey(e => e.ParentId);

                    b.HasOne<SingleDelegateLoaderByConstructor>(nameof(ParentDelegateLoaderByConstructor.Single))
                        .WithOne(e => e.Parent)
                        .HasForeignKey<SingleDelegateLoaderByConstructor>(e => e.ParentId);
                });

            modelBuilder.Entity<ParentDelegateLoaderByProperty>(
                b =>
                {
                    b.HasMany<ChildDelegateLoaderByProperty>(nameof(ParentDelegateLoaderByProperty.Children))
                        .WithOne(nameof(ChildDelegateLoaderByProperty.Parent))
                        .HasForeignKey(e => e.ParentId);

                    b.HasOne<SingleDelegateLoaderByProperty>(nameof(ParentDelegateLoaderByProperty.Single))
                        .WithOne(e => e.Parent)
                        .HasForeignKey<SingleDelegateLoaderByProperty>(e => e.ParentId);
                });

            modelBuilder.Entity<ChildDelegateLoaderWithStateByProperty>(
                b =>
                {
                    var serviceProperty = (ServiceProperty)b.Metadata.AddServiceProperty(
                        typeof(ChildDelegateLoaderWithStateByProperty).GetAnyProperty("LazyLoaderState")!,
                        typeof(ILazyLoader));

                    serviceProperty.SetParameterBinding(
                        new DependencyInjectionParameterBinding(typeof(object), typeof(ILazyLoader), serviceProperty),
                        ConfigurationSource.Explicit);
                });

            modelBuilder.Entity<SingleDelegateLoaderWithStateByProperty>(
                b =>
                {
                    var serviceProperty = (ServiceProperty)b.Metadata.AddServiceProperty(
                        typeof(SingleDelegateLoaderWithStateByProperty).GetAnyProperty("LazyLoaderState")!,
                        typeof(ILazyLoader));

                    serviceProperty.SetParameterBinding(
                        new DependencyInjectionParameterBinding(typeof(object), typeof(ILazyLoader), serviceProperty),
                        ConfigurationSource.Explicit);
                });

            modelBuilder.Entity<ParentDelegateLoaderWithStateByProperty>(
                b =>
                {
                    var serviceProperty = (ServiceProperty)b.Metadata.AddServiceProperty(
                        typeof(ParentDelegateLoaderWithStateByProperty).GetAnyProperty("LazyLoaderState")!,
                        typeof(ILazyLoader));

                    serviceProperty.SetParameterBinding(
                        new DependencyInjectionParameterBinding(typeof(object), typeof(ILazyLoader), serviceProperty),
                        ConfigurationSource.Explicit);

                    b.HasMany<ChildDelegateLoaderWithStateByProperty>(nameof(ParentDelegateLoaderWithStateByProperty.Children))
                        .WithOne(nameof(ChildDelegateLoaderWithStateByProperty.Parent))
                        .HasForeignKey(e => e.ParentId);

                    b.HasOne<SingleDelegateLoaderWithStateByProperty>(nameof(ParentDelegateLoaderWithStateByProperty.Single))
                        .WithOne(e => e.Parent)
                        .HasForeignKey<SingleDelegateLoaderWithStateByProperty>(e => e.ParentId);
                });

            modelBuilder.Entity<RootClass>();
            modelBuilder.Entity<Product>();
            modelBuilder.Entity<Deposit>();
            modelBuilder.Entity<SimpleProduct>();

            modelBuilder.Entity<OptionalChildView>().HasNoKey();
            modelBuilder.Entity<RequiredChildView>().HasNoKey();
        }

        protected override Task SeedAsync(PoolableDbContext context)
        {
            context.Add(
                new Parent
                {
                    Id = 707,
                    AlternateId = "Root",
                    Children = new List<Child> { new() { Id = 11 }, new() { Id = 12 } },
                    SinglePkToPk = new SinglePkToPk { Id = 707 },
                    Single = new Single { Id = 21 },
                    RequiredSingle = new RequiredSingle { Id = 21 },
                    ChildrenAk = new List<ChildAk> { new() { Id = 31 }, new() { Id = 32 } },
                    SingleAk = new SingleAk { Id = 42 },
                    ChildrenShadowFk = new List<ChildShadowFk> { new() { Id = 51 }, new() { Id = 52 } },
                    SingleShadowFk = new SingleShadowFk { Id = 62 },
                    ChildrenCompositeKey = new List<ChildCompositeKey> { new() { Id = 51 }, new() { Id = 52 } },
                    SingleCompositeKey = new SingleCompositeKey { Id = 62 }
                });

            context.Add(
                new ParentFullLoaderByConstructor
                {
                    Id = 707,
                    Children = new List<ChildFullLoaderByConstructor> { new() { Id = 11 }, new() { Id = 12 } },
                    Single = new SingleFullLoaderByConstructor { Id = 21 }
                });

            context.Add(
                new ParentDelegateLoaderByConstructor
                {
                    Id = 707,
                    Children = new List<ChildDelegateLoaderByConstructor> { new() { Id = 11 }, new() { Id = 12 } },
                    Single = new SingleDelegateLoaderByConstructor { Id = 21 }
                });

            context.Add(
                new ParentDelegateLoaderByProperty
                {
                    Id = 707,
                    Children = new List<ChildDelegateLoaderByProperty> { new() { Id = 11 }, new() { Id = 12 } },
                    Single = new SingleDelegateLoaderByProperty { Id = 21 }
                });

            context.Add(
                new ParentDelegateLoaderWithStateByProperty
                {
                    Id = 707,
                    Children = new List<ChildDelegateLoaderWithStateByProperty> { new() { Id = 11 }, new() { Id = 12 } },
                    Single = new SingleDelegateLoaderWithStateByProperty { Id = 21 }
                });

            context.Add(
                new SimpleProduct { Deposit = new Deposit() });

            return context.SaveChangesAsync();
        }
    }
}
