// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class FieldsOnlyLoadTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : FieldsOnlyLoadTestBase<TFixture>.FieldsOnlyLoadFixtureBase
{
    protected FieldsOnlyLoadTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Added)]
    public virtual void Attached_references_to_principal_are_marked_as_loaded(EntityState state)
    {
        using var context = CreateContext();
        var parent = new Parent
        {
            Id = 707,
            AlternateId = "Root",
            SinglePkToPk = new SinglePkToPk { Id = 707 },
            Single = new Single { Id = 21 },
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
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Added)]
    public virtual void Attached_references_to_dependents_are_marked_as_loaded(EntityState state)
    {
        using var context = CreateContext();
        var parent = new Parent
        {
            Id = 707,
            AlternateId = "Root",
            SinglePkToPk = new SinglePkToPk { Id = 707 },
            Single = new Single { Id = 21 },
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

        Assert.True(context.Entry(parent.SinglePkToPk).Reference(e => e.Parent).IsLoaded);
        Assert.True(context.Entry(parent.Single).Reference(e => e.Parent).IsLoaded);
        Assert.True(context.Entry(parent.SingleAk).Reference(e => e.Parent).IsLoaded);
        Assert.True(context.Entry(parent.SingleShadowFk).Reference(e => e.Parent).IsLoaded);
        Assert.True(context.Entry(parent.SingleCompositeKey).Reference(e => e.Parent).IsLoaded);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Added)]
    public virtual void Attached_collections_are_not_marked_as_loaded(EntityState state)
    {
        using var context = CreateContext();
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
        using var context = CreateContext();

        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var parent = context.Set<Parent>().Single();

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
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(2, parent.Children.Count());
        Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

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

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

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

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

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

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

        var single = context.ChangeTracker.Entries<Single>().Single().Entity;

        Assert.Same(single, parent.Single);
        Assert.Same(parent, single.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

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

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

        var single = context.ChangeTracker.Entries<SinglePkToPk>().Single().Entity;

        Assert.Same(single, parent.SinglePkToPk);
        Assert.Same(parent, single.Parent);
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
        Assert.Equal(2, parent.Children.Count());
        Assert.All(children.Select(e => e.Parent), c => Assert.Same(parent, c));
        Assert.All(children, p => Assert.Contains(p, parent.Children));

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(state != EntityState.Deleted, referenceEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(parent);

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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(state != EntityState.Deleted, referenceEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(parent);

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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(single);
        Assert.Same(single, parent.Single);
        Assert.Same(parent, single.Parent);

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(state != EntityState.Deleted, referenceEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(parent);

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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(single);
        Assert.Same(single, parent.SinglePkToPk);
        Assert.Same(parent, single.Parent);

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_many_to_one_reference_to_principal_null_FK(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(
            new Child { Id = 767, ParentId = null }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());
        Assert.Null(child.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_reference_to_principal_null_FK(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(
            new Single { Id = 767, ParentId = null }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());

        Assert.Null(single.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_many_to_one_reference_to_principal_using_Query_null_FK(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(
            new Child { Id = 767, ParentId = null }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_reference_to_principal_using_Query_null_FK(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(
            new Single { Id = 767, ParentId = null }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_collection_not_found(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Attach(
            new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

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
        Assert.Single(context.ChangeTracker.Entries());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_many_to_one_reference_to_principal_not_found(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(
            new Child { Id = 767, ParentId = 787 }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());
        Assert.Null(child.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_reference_to_principal_not_found(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(
            new Single { Id = 767, ParentId = 787 }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());

        Assert.Null(single.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_reference_to_dependent_not_found(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Attach(
            new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());

        Assert.Null(parent.Single);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_collection_using_Query_not_found(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Attach(
            new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_many_to_one_reference_to_principal_using_Query_not_found(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(
            new Child { Id = 767, ParentId = 787 }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_reference_to_principal_using_Query_not_found(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(
            new Single { Id = 767, ParentId = 787 }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_not_found(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Attach(
            new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

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
    public virtual async Task Load_collection_already_loaded(EntityState state, bool async, CascadeTiming deleteOrphansTiming)
    {
        using var context = CreateContext();
        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

        var parent = context.Set<Parent>().Include(e => e.Children).Single();

        ClearLog();

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

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

        if (state == EntityState.Deleted
            && deleteOrphansTiming != CascadeTiming.Never)
        {
            Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Null(c));
        }
        else
        {
            Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));
        }

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_many_to_one_reference_to_principal_already_loaded(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Set<Child>().Include(e => e.Parent).Single(e => e.Id == 12);

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        context.Entry(child).State = state;

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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

        var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

        Assert.Same(parent, child.Parent);
        Assert.Same(child, parent.Children.Single());
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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

        var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

        Assert.Same(parent, single.Parent);
        Assert.Same(single, parent.Single);
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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

        var single = context.ChangeTracker.Entries<Single>().Single().Entity;

        Assert.Same(single, parent.Single);

        if (state == EntityState.Deleted
            && deleteOrphansTiming != CascadeTiming.Never)
        {
            Assert.Null(single.Parent);
        }
        else
        {
            Assert.Same(parent, single.Parent);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_PK_to_PK_reference_to_principal_already_loaded(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<SinglePkToPk>().Include(e => e.Parent).Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

        var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

        Assert.Same(parent, single.Parent);
        Assert.Same(single, parent.SinglePkToPk);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_PK_to_PK_reference_to_dependent_already_loaded(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Include(e => e.SinglePkToPk).Single();

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.SinglePkToPk);

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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

        var single = context.ChangeTracker.Entries<SinglePkToPk>().Single().Entity;

        Assert.Same(single, parent.SinglePkToPk);
        Assert.Same(parent, single.Parent);
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

        context.Entry(parent).State = state;

        Assert.True(collectionEntry.IsLoaded);

        var children = async
            ? await collectionEntry.Query().ToListAsync()
            : collectionEntry.Query().ToList();

        Assert.True(collectionEntry.IsLoaded);

        RecordLog();

        Assert.Equal(2, children.Count);
        Assert.Equal(2, parent.Children.Count());
        Assert.All(children.Select(e => e.Parent), c => Assert.Same(parent, c));
        Assert.All(children, p => Assert.Contains(p, parent.Children));
        Assert.Equal(3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_many_to_one_reference_to_principal_using_Query_already_loaded(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Set<Child>().Include(e => e.Parent).Single(e => e.Id == 12);

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        context.Entry(child).State = state;

        Assert.True(referenceEntry.IsLoaded);

        var parent = async
            ? await referenceEntry.Query().SingleAsync()
            : referenceEntry.Query().Single();

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(parent);
        Assert.Same(parent, child.Parent);
        Assert.Same(child, parent.Children.Single());

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_reference_to_principal_using_Query_already_loaded(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<Single>().Include(e => e.Parent).Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.True(referenceEntry.IsLoaded);

        var parent = async
            ? await referenceEntry.Query().SingleAsync()
            : referenceEntry.Query().Single();

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(parent);
        Assert.Same(parent, single.Parent);
        Assert.Same(single, parent.Single);

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
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

        context.Entry(parent).State = state;

        Assert.True(referenceEntry.IsLoaded);

        var single = async
            ? await referenceEntry.Query().SingleAsync()
            : referenceEntry.Query().Single();

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(single);
        Assert.Same(single, parent.Single);
        Assert.Same(parent, single.Parent);
        Assert.Equal(2, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_PK_to_PK_reference_to_principal_using_Query_already_loaded(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<SinglePkToPk>().Include(e => e.Parent).Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.True(referenceEntry.IsLoaded);

        var parent = async
            ? await referenceEntry.Query().SingleAsync()
            : referenceEntry.Query().Single();

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(parent);
        Assert.Same(parent, single.Parent);
        Assert.Same(single, parent.SinglePkToPk);

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_PK_to_PK_reference_to_dependent_using_Query_already_loaded(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Set<Parent>().Include(e => e.SinglePkToPk).Single();

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.SinglePkToPk);

        context.Entry(parent).State = state;

        Assert.True(referenceEntry.IsLoaded);

        var single = async
            ? await referenceEntry.Query().SingleAsync()
            : referenceEntry.Query().Single();

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(single);
        Assert.Same(single, parent.SinglePkToPk);
        Assert.Same(parent, single.Parent);

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
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

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

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

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

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

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

        var single = context.ChangeTracker.Entries<Single>().Single().Entity;

        Assert.Same(single, parent.Single);
        Assert.Same(parent, single.Parent);
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
        Assert.Equal(2, parent.Children.Count());
        Assert.All(children.Select(e => ((Child)e).Parent), c => Assert.Same(parent, c));
        Assert.All(children, p => Assert.Contains(p, parent.Children));

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(state != EntityState.Deleted, navigationEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(parent);

        if (state == EntityState.Deleted)
        {
            Assert.Null(child.Parent);
            Assert.Null(((Parent)parent).Children);
        }
        else
        {
            Assert.Same(parent, child.Parent);
            Assert.Same(child, ((Parent)parent).Children.Single());
        }

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(state != EntityState.Deleted, navigationEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(parent);

        if (state == EntityState.Deleted)
        {
            Assert.Null(single.Parent);
            Assert.Null(((Parent)parent).Single);
        }
        else
        {
            Assert.Same(parent, single.Parent);
            Assert.Same(single, ((Parent)parent).Single);
        }

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.True(navigationEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(single);
        Assert.Same(single, parent.Single);
        Assert.Same(parent, ((Single)single).Parent);

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
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
        using var context = CreateContext();
        var parent = context.Attach(
            new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

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
        Assert.Single(context.ChangeTracker.Entries());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_many_to_one_reference_to_principal_not_found_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(
            new Child { Id = 767, ParentId = 787 }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());
        Assert.Null(child.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_reference_to_principal_not_found_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(
            new Single { Id = 767, ParentId = 787 }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());

        Assert.Null(single.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_reference_to_dependent_not_found_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Attach(
            new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());

        Assert.Null(parent.Single);
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
        using var context = CreateContext();
        var parent = context.Attach(
            new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_many_to_one_reference_to_principal_using_Query_not_found_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(
            new Child { Id = 767, ParentId = 787 }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_reference_to_principal_using_Query_not_found_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(
            new Single { Id = 767, ParentId = 787 }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_not_found_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var parent = context.Attach(
            new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

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
        using var context = CreateContext();
        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

        var parent = context.Set<Parent>().Include(e => e.Children).Single();

        ClearLog();

        var navigationEntry = context.Entry(parent).Navigation("Children");

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

        if (state == EntityState.Deleted
            && deleteOrphansTiming != CascadeTiming.Never)
        {
            Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Null(c));
        }
        else
        {
            Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));
        }

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_many_to_one_reference_to_principal_already_loaded_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Set<Child>().Include(e => e.Parent).Single(e => e.Id == 12);

        ClearLog();

        var navigationEntry = context.Entry(child).Navigation("Parent");

        context.Entry(child).State = state;

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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

        var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

        Assert.Same(parent, child.Parent);
        Assert.Same(child, parent.Children.Single());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_reference_to_principal_already_loaded_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<Single>().Include(e => e.Parent).Single();

        ClearLog();

        var navigationEntry = context.Entry(single).Navigation("Parent");

        context.Entry(single).State = state;

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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

        var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

        Assert.Same(parent, single.Parent);
        Assert.Same(single, parent.Single);
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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

        var single = context.ChangeTracker.Entries<Single>().Single().Entity;

        Assert.Same(single, parent.Single);

        if (state == EntityState.Deleted
            && deleteOrphansTiming != CascadeTiming.Never)
        {
            Assert.Null(single.Parent);
        }
        else
        {
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
        using var context = CreateContext();
        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;
        context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;

        var parent = context.Set<Parent>().Include(e => e.Children).Single();

        ClearLog();

        var navigationEntry = context.Entry(parent).Navigation("Children");

        context.Entry(parent).State = state;

        Assert.True(navigationEntry.IsLoaded);

        // Issue #16429
        var children = async
            ? await navigationEntry.Query().ToListAsync<object>()
            : navigationEntry.Query().ToList<object>();

        Assert.True(navigationEntry.IsLoaded);

        RecordLog();

        Assert.Equal(2, children.Count);
        Assert.Equal(2, parent.Children.Count());
        Assert.All(children.Select(e => ((Child)e).Parent), c => Assert.Same(parent, c));
        Assert.All(children, p => Assert.Contains(p, parent.Children));
        Assert.Equal(3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_many_to_one_reference_to_principal_using_Query_already_loaded_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Set<Child>().Include(e => e.Parent).Single(e => e.Id == 12);

        ClearLog();

        var navigationEntry = context.Entry(child).Navigation("Parent");

        context.Entry(child).State = state;

        Assert.True(navigationEntry.IsLoaded);

        // Issue #16429
        var parent = async
            ? (await navigationEntry.Query().ToListAsync<object>()).Single()
            : navigationEntry.Query().ToList<object>().Single();

        Assert.True(navigationEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(parent);
        Assert.Same(parent, child.Parent);
        Assert.Same(child, ((Parent)parent).Children.Single());

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_reference_to_principal_using_Query_already_loaded_untyped(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<Single>().Include(e => e.Parent).Single();

        ClearLog();

        var navigationEntry = context.Entry(single).Navigation("Parent");

        context.Entry(single).State = state;

        Assert.True(navigationEntry.IsLoaded);

        // Issue #16429
        var parent = async
            ? (await navigationEntry.Query().ToListAsync<object>()).Single()
            : navigationEntry.Query().ToList<object>().Single();

        Assert.True(navigationEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(parent);
        Assert.Same(parent, single.Parent);
        Assert.Same(single, ((Parent)parent).Single);

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
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

        context.Entry(parent).State = state;

        Assert.True(navigationEntry.IsLoaded);

        // Issue #16429
        var single = async
            ? (await navigationEntry.Query().ToListAsync<object>()).Single()
            : navigationEntry.Query().ToList<object>().Single();

        Assert.True(navigationEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(single);
        Assert.Same(single, parent.Single);

        Assert.Same(parent, ((Single)single).Parent);

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

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

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

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

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

        var single = context.ChangeTracker.Entries<SingleAk>().Single().Entity;

        Assert.Same(single, parent.SingleAk);
        Assert.Same(parent, single.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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
        Assert.Equal(2, parent.ChildrenAk.Count());
        Assert.All(children.Select(e => e.Parent), c => Assert.Same(parent, c));
        Assert.All(children, p => Assert.Contains(p, parent.ChildrenAk));

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(state != EntityState.Deleted, referenceEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(parent);

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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(state != EntityState.Deleted, referenceEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(parent);

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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(single);
        Assert.Same(single, parent.SingleAk);
        Assert.Same(parent, single.Parent);

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_many_to_one_reference_to_principal_null_FK_alternate_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(
            new ChildAk { Id = 767, ParentId = null }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());
        Assert.Null(child.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_reference_to_principal_null_FK_alternate_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(
            new SingleAk { Id = 767, ParentId = null }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());

        Assert.Null(single.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_alternate_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(
            new ChildAk { Id = 767, ParentId = null }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_alternate_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(
            new SingleAk { Id = 767, ParentId = null }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_many_to_one_reference_to_principal_shadow_fk(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Set<ChildShadowFk>().Single(e => e.Id == 52);

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

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_reference_to_principal_shadow_fk(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<SingleShadowFk>().Single();

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

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

        var single = context.ChangeTracker.Entries<SingleShadowFk>().Single().Entity;

        Assert.Same(single, parent.SingleShadowFk);
        Assert.Same(parent, single.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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
        Assert.Equal(2, parent.ChildrenShadowFk.Count());
        Assert.All(children.Select(e => e.Parent), c => Assert.Same(parent, c));
        Assert.All(children, p => Assert.Contains(p, parent.ChildrenShadowFk));

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_many_to_one_reference_to_principal_using_Query_shadow_fk(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Set<ChildShadowFk>().Single(e => e.Id == 52);

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        context.Entry(child).State = state;

        Assert.False(referenceEntry.IsLoaded);

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

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_reference_to_principal_using_Query_shadow_fk(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Set<SingleShadowFk>().Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        context.Entry(single).State = state;

        Assert.False(referenceEntry.IsLoaded);

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

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(single);
        Assert.Same(single, parent.SingleShadowFk);
        Assert.Same(parent, single.Parent);

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_many_to_one_reference_to_principal_null_FK_shadow_fk(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(new ChildShadowFk { Id = 767 }).Entity;
        context.Entry(child).Property("ParentId").CurrentValue = null;

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

        Assert.Single(context.ChangeTracker.Entries());
        Assert.Null(child.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_reference_to_principal_null_FK_shadow_fk(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(new SingleShadowFk { Id = 767 }).Entity;
        context.Entry(single).Property("ParentId").CurrentValue = null;

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

        Assert.Single(context.ChangeTracker.Entries());

        Assert.Null(single.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(new ChildShadowFk { Id = 767 }).Entity;
        context.Entry(child).Property("ParentId").CurrentValue = null;

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

        Assert.Single(context.ChangeTracker.Entries());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(new SingleShadowFk { Id = 767 }).Entity;
        context.Entry(single).Property("ParentId").CurrentValue = null;

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

        Assert.Single(context.ChangeTracker.Entries());
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

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

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

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

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

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

        var single = context.ChangeTracker.Entries<SingleCompositeKey>().Single().Entity;

        Assert.Same(single, parent.SingleCompositeKey);
        Assert.Same(parent, single.Parent);
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
        Assert.Equal(2, parent.ChildrenCompositeKey.Count());
        Assert.All(children.Select(e => e.Parent), c => Assert.Same(parent, c));
        Assert.All(children, p => Assert.Contains(p, parent.ChildrenCompositeKey));

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(state != EntityState.Deleted, referenceEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(parent);

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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.Equal(state != EntityState.Deleted, referenceEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(parent);

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

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
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

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();

        Assert.NotNull(single);
        Assert.Same(single, parent.SingleCompositeKey);
        Assert.Same(parent, single.Parent);

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_many_to_one_reference_to_principal_null_FK_composite_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(
            new ChildCompositeKey { Id = 767, ParentId = 567 }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());
        Assert.Null(child.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_reference_to_principal_null_FK_composite_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(
            new SingleCompositeKey { Id = 767, ParentAlternateId = "Boot" }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());

        Assert.Null(single.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_composite_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var child = context.Attach(
            new ChildCompositeKey { Id = 767, ParentAlternateId = "Boot" }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    public virtual async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_composite_key(EntityState state, bool async)
    {
        using var context = CreateContext();
        var single = context.Attach(
            new SingleCompositeKey { Id = 767, ParentId = 567 }).Entity;

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

        Assert.Single(context.ChangeTracker.Entries());
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

    [ConditionalTheory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public virtual async Task Load_collection_for_detached_throws(bool async, bool noTracking)
    {
        using var context = CreateContext(noTracking: noTracking);
        var parent = context.Set<Parent>().Single();

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        if (!noTracking)
        {
            context.Entry(parent).State = EntityState.Detached;
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
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public virtual async Task Load_collection_using_string_for_detached_throws(bool async, bool noTracking)
    {
        using var context = CreateContext(noTracking: noTracking);
        var parent = context.Set<Parent>().Single();

        var collectionEntry = context.Entry(parent).Collection(nameof(Parent.Children));

        if (!noTracking)
        {
            context.Entry(parent).State = EntityState.Detached;
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
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public virtual async Task Load_collection_with_navigation_for_detached_throws(bool async, bool noTracking)
    {
        using var context = CreateContext(noTracking: noTracking);
        var parent = context.Set<Parent>().Single();

        var collectionEntry = context.Entry(parent).Navigation(nameof(Parent.Children));

        if (!noTracking)
        {
            context.Entry(parent).State = EntityState.Detached;
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
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public virtual async Task Load_reference_to_principal_for_detached_throws(bool async, bool noTracking)
    {
        using var context = CreateContext(noTracking: noTracking);
        var child = context.Set<Child>().Single(e => e.Id == 12);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        if (!noTracking)
        {
            context.Entry(child).State = EntityState.Detached;
        }

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }
    }

    [ConditionalTheory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public virtual async Task Load_reference_with_navigation_to_principal_for_detached_throws(bool async, bool noTracking)
    {
        using var context = CreateContext(noTracking: noTracking);
        var child = context.Set<Child>().Single(e => e.Id == 12);

        var referenceEntry = context.Entry(child).Navigation(nameof(Child.Parent));

        if (!noTracking)
        {
            context.Entry(child).State = EntityState.Detached;
        }

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }
    }

    [ConditionalTheory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public virtual async Task Load_reference_using_string_to_principal_for_detached_throws(bool async, bool noTracking)
    {
        using var context = CreateContext(noTracking: noTracking);
        var child = context.Set<Child>().Single(e => e.Id == 12);

        var referenceEntry = context.Entry(child).Reference(nameof(Child.Parent));

        if (!noTracking)
        {
            context.Entry(child).State = EntityState.Detached;
        }

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }
    }

    [ConditionalTheory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public virtual async Task Load_reference_to_dependent_for_detached_throws(bool async, bool noTracking)
    {
        using var context = CreateContext(noTracking: noTracking);
        var parent = context.Set<Parent>().Single();

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        if (!noTracking)
        {
            context.Entry(parent).State = EntityState.Detached;
        }

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }
    }

    [ConditionalTheory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public virtual async Task Load_reference_to_dependent_with_navigation_for_detached_throws(bool async, bool noTracking)
    {
        using var context = CreateContext(noTracking: noTracking);
        var parent = context.Set<Parent>().Single();

        var referenceEntry = context.Entry(parent).Navigation(nameof(Parent.Single));

        if (!noTracking)
        {
            context.Entry(parent).State = EntityState.Detached;
        }

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }
    }

    [ConditionalTheory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public virtual async Task Load_reference_to_dependent_using_string_for_detached_throws(bool async, bool noTracking)
    {
        using var context = CreateContext(noTracking: noTracking);
        var parent = context.Set<Parent>().Single();

        var referenceEntry = context.Entry(parent).Reference(nameof(Parent.Single));

        if (!noTracking)
        {
            context.Entry(parent).State = EntityState.Detached;
        }

        if (async)
        {
            await referenceEntry.LoadAsync();
        }
        else
        {
            referenceEntry.Load();
        }
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual void Query_collection_for_detached_throws(bool noTracking)
    {
        using var context = CreateContext(noTracking: noTracking);
        var parent = context.Set<Parent>().Single();

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        if (!noTracking)
        {
            context.Entry(parent).State = EntityState.Detached;
        }

        var query = collectionEntry.Query();
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual void Query_collection_using_string_for_detached_throws(bool noTracking)
    {
        using var context = CreateContext(noTracking: noTracking);
        var parent = context.Set<Parent>().Single();

        var collectionEntry = context.Entry(parent).Collection(nameof(Parent.Children));

        if (!noTracking)
        {
            context.Entry(parent).State = EntityState.Detached;
        }

        var query = collectionEntry.Query();
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual void Query_collection_with_navigation_for_detached_throws(bool noTracking)
    {
        using var context = CreateContext(noTracking: noTracking);
        var parent = context.Set<Parent>().Single();

        var collectionEntry = context.Entry(parent).Navigation(nameof(Parent.Children));

        if (!noTracking)
        {
            context.Entry(parent).State = EntityState.Detached;
        }

        var query = collectionEntry.Query();
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual void Query_reference_to_principal_for_detached_throws(bool noTracking)
    {
        using var context = CreateContext(noTracking: noTracking);
        var child = context.Set<Child>().Single(e => e.Id == 12);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        if (!noTracking)
        {
            context.Entry(child).State = EntityState.Detached;
        }

        var query = referenceEntry.Query();
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual void Query_reference_with_navigation_to_principal_for_detached_throws(bool noTracking)
    {
        using var context = CreateContext(noTracking: noTracking);
        var child = context.Set<Child>().Single(e => e.Id == 12);

        var referenceEntry = context.Entry(child).Navigation(nameof(Child.Parent));

        if (!noTracking)
        {
            context.Entry(child).State = EntityState.Detached;
        }

        var query = referenceEntry.Query();
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual void Query_reference_using_string_to_principal_for_detached_throws(bool noTracking)
    {
        using var context = CreateContext(noTracking: noTracking);
        var child = context.Set<Child>().Single(e => e.Id == 12);

        var referenceEntry = context.Entry(child).Reference(nameof(Child.Parent));

        if (!noTracking)
        {
            context.Entry(child).State = EntityState.Detached;
        }

        var query = referenceEntry.Query();
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual void Query_reference_to_dependent_for_detached_throws(bool noTracking)
    {
        using var context = CreateContext(noTracking: noTracking);
        var parent = context.Set<Parent>().Single();

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        if (!noTracking)
        {
            context.Entry(parent).State = EntityState.Detached;
        }

        var query = referenceEntry.Query();
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual void Query_reference_to_dependent_with_navigation_for_detached_throws(bool noTracking)
    {
        using var context = CreateContext(noTracking: noTracking);
        var parent = context.Set<Parent>().Single();

        var referenceEntry = context.Entry(parent).Navigation(nameof(Parent.Single));

        if (!noTracking)
        {
            context.Entry(parent).State = EntityState.Detached;
        }

        var query = referenceEntry.Query();
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual void Query_reference_to_dependent_using_string_for_detached_throws(bool noTracking)
    {
        using var context = CreateContext(noTracking: noTracking);
        var parent = context.Set<Parent>().Single();

        var referenceEntry = context.Entry(parent).Reference(nameof(Parent.Single));

        if (!noTracking)
        {
            context.Entry(parent).State = EntityState.Detached;
        }

        var query = referenceEntry.Query();
    }

    protected class Parent
    {
        public int Id;
        public string AlternateId;
        public IEnumerable<Child> Children;
        public SinglePkToPk SinglePkToPk;
        public Single Single;
        public IEnumerable<ChildAk> ChildrenAk;
        public SingleAk SingleAk;
        public IEnumerable<ChildShadowFk> ChildrenShadowFk;
        public SingleShadowFk SingleShadowFk;
        public IEnumerable<ChildCompositeKey> ChildrenCompositeKey;
        public SingleCompositeKey SingleCompositeKey;
    }

    protected class Child
    {
        public int Id;
        public int? ParentId;
        public Parent Parent;
    }

    protected class SinglePkToPk
    {
        public int Id;
        public Parent Parent;
    }

    protected class Single
    {
        public int Id;
        public int? ParentId;
        public Parent Parent;
    }

    protected class ChildAk
    {
        public int Id;
        public string ParentId;
        public Parent Parent;
    }

    protected class SingleAk
    {
        public int Id;
        public string ParentId;
        public Parent Parent;
    }

    protected class ChildShadowFk
    {
        public int Id;
        public Parent Parent;
    }

    protected class SingleShadowFk
    {
        public int Id;
        public Parent Parent;
    }

    protected class ChildCompositeKey
    {
        public int Id;
        public int? ParentId;
        public string ParentAlternateId;
        public Parent Parent;
    }

    protected class SingleCompositeKey
    {
        public int Id;
        public int? ParentId;
        public string ParentAlternateId;
        public Parent Parent;
    }

    protected DbContext CreateContext(bool noTracking = false)
    {
        var context = Fixture.CreateContext();
        context.ChangeTracker.LazyLoadingEnabled = false;

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

    public abstract class FieldsOnlyLoadFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName
            => "FieldsOnlyLoadTest";

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection.AddScoped<IChangeDetector, ChangeDetectorProxy>());

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<SingleShadowFk>()
                .Property<int?>("ParentId");

            modelBuilder.Entity<Parent>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
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

            modelBuilder.Entity<SingleShadowFk>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                });

            modelBuilder.Entity<ChildShadowFk>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                });

            modelBuilder.Entity<SingleCompositeKey>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                });

            modelBuilder.Entity<ChildCompositeKey>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                });

            modelBuilder.Entity<SingleAk>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                });

            modelBuilder.Entity<ChildAk>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                });

            modelBuilder.Entity<Single>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                });

            modelBuilder.Entity<SinglePkToPk>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                });

            modelBuilder.Entity<Child>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                });
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
                    ChildrenAk = new List<ChildAk> { new() { Id = 31 }, new() { Id = 32 } },
                    SingleAk = new SingleAk { Id = 42 },
                    ChildrenShadowFk = new List<ChildShadowFk> { new() { Id = 51 }, new() { Id = 52 } },
                    SingleShadowFk = new SingleShadowFk { Id = 62 },
                    ChildrenCompositeKey = new List<ChildCompositeKey> { new() { Id = 51 }, new() { Id = 52 } },
                    SingleCompositeKey = new SingleCompositeKey { Id = 62 }
                });

            // context.Add(
            //     new SimpleProduct { Deposit = new Deposit() });

            return context.SaveChangesAsync();
        }
    }
}
