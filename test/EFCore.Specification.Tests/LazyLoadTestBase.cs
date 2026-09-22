// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract partial class LoadTestBase<TFixture>
{
    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_collection(EntityState state, QueryTrackingBehavior queryTrackingBehavior, bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<Parent>().Single();

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior);

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        Assert.False(collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (!LazyLoadingEnabled || (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll))
        {
            Assert.Null(await parent.LazyLoadChildren(async)); // Explicitly detached
        }
        else
        {
            Assert.NotNull(parent.Children);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(collectionEntry.IsLoaded);

            Assert.All(parent.Children.Select(e => e.Parent), p => Assert.Same(parent, p));

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, parent.Children.Count());

            Assert.Equal(state == EntityState.Detached ? 0 : 3, context.ChangeTracker.Entries().Count());
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_many_to_one_reference_to_principal(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.Set<Child>().Single(e => e.Id == 12);

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(await child.LazyLoadParent(async)); // Explicitly detached
            }
            else
            {
                if (state == EntityState.Deleted)
                {
                    Assert.Null(await child.LazyLoadParent(async));
                }
                else
                {
                    Assert.NotNull(await child.LazyLoadParent(async));
                }

                Assert.False(changeDetector.DetectChangesCalled);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

                if (state != EntityState.Deleted)
                {
                    Assert.Same(child, child.Parent!.Children.Single());
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
        }
        else
        {
            Assert.Null(await child.LazyLoadParent(async));
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_one_to_one_reference_to_principal(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Set<Single>().Single();

        ClearLog();

        SetState(context, single, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(await single.LazyLoadParent(async)); // Explicitly detached
            }
            else
            {
                if (state == EntityState.Deleted)
                {
                    Assert.Null(await single.LazyLoadParent(async));
                }
                else
                {
                    Assert.NotNull(await single.LazyLoadParent(async));
                }

                Assert.False(changeDetector.DetectChangesCalled);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

                if (state != EntityState.Deleted)
                {
                    Assert.Same(single, single.Parent!.Single);
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
        }
        else
        {
            Assert.Null(await single.LazyLoadParent(async));
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_one_to_one_reference_to_dependent(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<Parent>().Single();

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(await parent.LazyLoadSingle(async)); // Explicitly detached
            }
            else
            {
                Assert.NotNull(await parent.LazyLoadSingle(async));

                Assert.False(changeDetector.DetectChangesCalled);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

                if (state != EntityState.Deleted)
                {
                    Assert.Same(parent, parent.Single.Parent);
                }

                if (state != EntityState.Detached)
                {
                    var single = context.ChangeTracker.Entries<Single>().Single().Entity;

                    Assert.Same(single, parent.Single);
                    Assert.Same(parent, single.Parent);
                }
            }
        }
        else
        {
            Assert.Null(await parent.LazyLoadSingle(async));
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_PK_to_PK_reference_to_principal(EntityState state, QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Set<SinglePkToPk>().Single();

        ClearLog();

        SetState(context, single, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(single.Parent); // Explicitly detached
            }
            else
            {
                if (state == EntityState.Deleted)
                {
                    Assert.Null(single.Parent);
                }
                else
                {
                    Assert.NotNull(single.Parent);
                }

                Assert.False(changeDetector.DetectChangesCalled);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

                if (state != EntityState.Deleted)
                {
                    Assert.Same(single, single.Parent!.SinglePkToPk);
                }

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
        }
        else
        {
            Assert.Null(single.Parent);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_PK_to_PK_reference_to_dependent(EntityState state, QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<Parent>().Single();

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(parent).Reference(e => e.SinglePkToPk);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(parent.SinglePkToPk); // Explicitly detached
            }
            else
            {
                Assert.NotNull(parent.SinglePkToPk);

                Assert.False(changeDetector.DetectChangesCalled);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

                Assert.Same(parent, parent.SinglePkToPk.Parent);

                if (state != EntityState.Detached)
                {
                    var single = context.ChangeTracker.Entries<SinglePkToPk>().Single().Entity;

                    Assert.Same(single, parent.SinglePkToPk);
                    Assert.Same(parent, single.Parent);
                }
            }
        }
        else
        {
            Assert.Null(parent.SinglePkToPk);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_many_to_one_reference_to_principal_null_FK(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.Attach(new Child { Id = 767, ParentId = null }).Entity;

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            Assert.Null(await child.LazyLoadParent(async));

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
            Assert.Null(child.Parent);
        }
        else
        {
            Assert.Null(await child.LazyLoadParent(async));
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_one_to_one_reference_to_principal_null_FK(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Attach(new Single { Id = 767, ParentId = null }).Entity;

        ClearLog();

        SetState(context, single, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            Assert.Null(await single.LazyLoadParent(async));

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());

            Assert.Null(single.Parent);
        }
        else
        {
            Assert.Null(await single.LazyLoadParent(async));
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_collection_not_found(EntityState state, QueryTrackingBehavior queryTrackingBehavior, bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Attach(new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior, isAttached: true);

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        Assert.False(collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached)
            {
                Assert.Null(await parent.LazyLoadChildren(async)); // Explicitly detached
            }
            else
            {
                Assert.Empty(await parent.LazyLoadChildren(async));
                Assert.False(changeDetector.DetectChangesCalled);
                Assert.True(collectionEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Single(context.ChangeTracker.Entries());
            }
        }
        else
        {
            Assert.Null(await parent.LazyLoadChildren(async));
            Assert.False(collectionEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_many_to_one_reference_to_principal_not_found(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.Attach(new Child { Id = 767, ParentId = 787 }).Entity;

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            Assert.Null(await child.LazyLoadParent(async));

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
            Assert.Null(child.Parent);
        }
        else
        {
            Assert.Null(await child.LazyLoadParent(async));
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_one_to_one_reference_to_principal_not_found(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Attach(new Single { Id = 767, ParentId = 787 }).Entity;

        ClearLog();

        SetState(context, single, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            Assert.Null(await single.LazyLoadParent(async));

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());

            Assert.Null(single.Parent);
        }
        else
        {
            Assert.Null(await single.LazyLoadParent(async));
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_one_to_one_reference_to_dependent_not_found(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Attach(new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            Assert.Null(await parent.LazyLoadSingle(async));

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Null(parent.Single);

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
        }
        else
        {
            Assert.Null(await parent.LazyLoadSingle(async));
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_collection_already_loaded(
        EntityState state,
        CascadeTiming deleteOrphansTiming,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<Parent>().Include(e => e.Children).Single();

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior);

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        Assert.True(collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.NotNull(await parent.LazyLoadChildren(async));

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.True(collectionEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(2, parent.Children.Count());

        if (queryTrackingBehavior == QueryTrackingBehavior.TrackAll
            && state == EntityState.Deleted
            && deleteOrphansTiming != CascadeTiming.Never)
        {
            Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Null(c));
        }
        else
        {
            Assert.All(parent.Children.Select(e => e.Parent), p => Assert.Same(parent, p));
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_collection_already_partially_loaded(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;

        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        context.ChangeTracker.LazyLoadingEnabled = false;

        var child = context.Set<Child>().OrderBy(e => e.Id).First();
        var parent = context.Set<Parent>().Single();
        if (parent.Children == null)
        {
            parent.Children = new List<Child> { child };
            child.Parent = parent;
        }

        context.ChangeTracker.LazyLoadingEnabled = true;

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior);
        SetState(context, parent, state, queryTrackingBehavior);

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        Assert.False(collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.NotNull(await parent.LazyLoadChildren(async));

        Assert.False(changeDetector.DetectChangesCalled);

        RecordLog();

        if (!LazyLoadingEnabled || (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll))
        {
            Assert.False(collectionEntry.IsLoaded); // Explicitly detached
            Assert.Equal(1, parent.Children.Count());

            Assert.All(parent.Children.Select(e => e.Parent), p => Assert.Same(parent, p));
        }
        else
        {
            Assert.True(collectionEntry.IsLoaded);

            context.ChangeTracker.LazyLoadingEnabled = false;

            // Note that when detached there is no identity resolution, so loading results in duplicates
            Assert.Equal(
                state == EntityState.Detached && queryTrackingBehavior != QueryTrackingBehavior.NoTrackingWithIdentityResolution
                    ? 3
                    : 2, parent.Children.Count());

            Assert.All(parent.Children.Select(e => e.Parent), p => Assert.Same(parent, p));
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_many_to_one_reference_to_principal_already_loaded(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.Set<Child>().Include(e => e.Parent).Single(e => e.Id == 12);

        ClearLog();

        SetState(context, child.Parent, state, queryTrackingBehavior);
        SetState(context, child, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        if (state == EntityState.Deleted && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
        {
            Assert.False(referenceEntry.IsLoaded);
            Assert.Null(await child.LazyLoadParent(async));
        }
        else
        {
            Assert.True(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.NotNull(await child.LazyLoadParent(async));

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

            Assert.Same(child, child.Parent.Children.Single());

            if (state != EntityState.Detached)
            {
                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.Children.Single());
            }
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_one_to_one_reference_to_principal_already_loaded(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Set<Single>().Include(e => e.Parent).Single();

        ClearLog();

        SetState(context, single.Parent, state, queryTrackingBehavior);
        SetState(context, single, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        if (state == EntityState.Deleted && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
        {
            Assert.False(referenceEntry.IsLoaded);
            Assert.Null(await single.LazyLoadParent(async));
        }
        else
        {
            Assert.True(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.NotNull(await single.LazyLoadParent(async));

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

            Assert.Same(single, single.Parent.Single);

            if (state != EntityState.Detached)
            {
                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.Single);
            }
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_one_to_one_reference_to_dependent_already_loaded(
        EntityState state,
        CascadeTiming deleteOrphansTiming,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<Parent>().Include(e => e.Single).Single();

        ClearLog();

        SetState(context, parent.Single, state, queryTrackingBehavior);
        SetState(context, parent, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        Assert.True(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.NotNull(await parent.LazyLoadSingle(async));

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state == EntityState.Deleted
            && deleteOrphansTiming != CascadeTiming.Never)
        {
            Assert.Same(parent, parent.Single.Parent);
        }

        if (state != EntityState.Detached)
        {
            var single = context.ChangeTracker.Entries<Single>().Single().Entity;

            Assert.Same(single, parent.Single);
            Assert.Same(parent, single.Parent);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_PK_to_PK_reference_to_principal_already_loaded(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Set<SinglePkToPk>().Include(e => e.Parent).Single();

        ClearLog();

        SetState(context, single.Parent, state, queryTrackingBehavior);
        SetState(context, single, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        Assert.True(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.NotNull(single.Parent);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        Assert.Same(single, single.Parent.SinglePkToPk);

        if (state != EntityState.Detached)
        {
            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.SinglePkToPk);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_PK_to_PK_reference_to_dependent_already_loaded(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<Parent>().Include(e => e.SinglePkToPk).Single();

        ClearLog();

        SetState(context, parent.SinglePkToPk, state, queryTrackingBehavior);
        SetState(context, parent, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(parent).Reference(e => e.SinglePkToPk);

        Assert.True(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.NotNull(parent.SinglePkToPk);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        Assert.Same(parent, parent.SinglePkToPk.Parent);

        if (state != EntityState.Detached)
        {
            var single = context.ChangeTracker.Entries<SinglePkToPk>().Single().Entity;

            Assert.Same(single, parent.SinglePkToPk);
            Assert.Same(parent, single.Parent);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_alternate_key(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var child = context.Set<ChildAk>().Single(e => e.Id == 32);

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(child.Parent); // Explicitly detached
            }
            else
            {
                if (state == EntityState.Deleted)
                {
                    Assert.Null(child.Parent);
                }
                else
                {
                    Assert.NotNull(child.Parent);
                }

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

                if (state != EntityState.Deleted)
                {
                    Assert.Same(child, child.Parent!.ChildrenAk.Single());
                }

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
        }
        else
        {
            Assert.Null(child.Parent);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_alternate_key(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var single = context.Set<SingleAk>().Single();

        ClearLog();

        SetState(context, single, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(single.Parent); // Explicitly detached
            }
            else
            {
                if (state == EntityState.Deleted)
                {
                    Assert.Null(single.Parent);
                }
                else
                {
                    Assert.NotNull(single.Parent);
                }

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

                if (state != EntityState.Deleted)
                {
                    Assert.Same(single, single.Parent!.SingleAk);
                }

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
        }
        else
        {
            Assert.Null(single.Parent);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_dependent_alternate_key(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var parent = context.Set<Parent>().Single();

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(parent).Reference(e => e.SingleAk);

        Assert.False(referenceEntry.IsLoaded);

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(parent.SingleAk); // Explicitly detached
            }
            else
            {
                Assert.NotNull(parent.SingleAk);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

                Assert.Same(parent, parent.SingleAk.Parent);

                if (state != EntityState.Detached)
                {
                    var single = context.ChangeTracker.Entries<SingleAk>().Single().Entity;

                    Assert.Same(single, parent.SingleAk);
                    Assert.Same(parent, single.Parent);
                }
            }
        }
        else
        {
            Assert.Null(parent.SingleAk);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_null_FK_alternate_key(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var child = context.Attach(new ChildAk { Id = 767, ParentId = null }).Entity;

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        if (LazyLoadingEnabled)
        {
            Assert.Null(child.Parent);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
            Assert.Null(child.Parent);
        }
        else
        {
            Assert.Null(child.Parent);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_null_FK_alternate_key(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var single = context.Attach(new SingleAk { Id = 767, ParentId = null }).Entity;

        ClearLog();

        SetState(context, single, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        if (LazyLoadingEnabled)
        {
            Assert.Null(single.Parent);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());

            Assert.Null(single.Parent);
        }
        else
        {
            Assert.Null(single.Parent);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_collection_shadow_fk(EntityState state, QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var parent = context.Set<Parent>().Single();

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior);

        var collectionEntry = context.Entry(parent).Collection(e => e.ChildrenShadowFk);

        Assert.False(collectionEntry.IsLoaded);

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(parent.ChildrenShadowFk); // Explicitly detached
            }
            else
            {
                Assert.NotNull(parent.ChildrenShadowFk);

                Assert.True(collectionEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(2, parent.ChildrenShadowFk.Count());
                Assert.All(parent.ChildrenShadowFk.Select(e => e.Parent), p => Assert.Same(parent, p));
            }

            Assert.Equal(state == EntityState.Detached ? 0 : 3, context.ChangeTracker.Entries().Count());
        }
        else
        {
            Assert.Null(parent.ChildrenShadowFk);
            Assert.False(collectionEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_shadow_fk(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var child = context.Set<ChildShadowFk>().Single(e => e.Id == 52);

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior);

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll
                || state == EntityState.Added && queryTrackingBehavior != QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(child.Parent); // Explicitly detached
            }
            else if (state == EntityState.Detached || queryTrackingBehavior != QueryTrackingBehavior.TrackAll)
            {
                Assert.Equal(
                    CoreStrings.CannotLoadDetachedShadow("Parent", "ChildShadowFk"),
                    Assert.Throws<InvalidOperationException>(() => child.Parent).Message);
            }
            else
            {
                var referenceEntry = context.Entry(child).Reference(e => e.Parent);

                Assert.False(referenceEntry.IsLoaded);

                if (state == EntityState.Deleted)
                {
                    Assert.Null(child.Parent);
                }
                else
                {
                    Assert.NotNull(child.Parent);
                }

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                if (state != EntityState.Deleted)
                {
                    Assert.Same(child, child.Parent!.ChildrenShadowFk.Single());
                }

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
        else
        {
            Assert.Null(child.Parent);
            Assert.False(context.Entry(child).Reference(e => e.Parent).IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_shadow_fk(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var single = context.Set<SingleShadowFk>().Single();

        ClearLog();

        SetState(context, single, state, queryTrackingBehavior);

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll
                || state == EntityState.Added && queryTrackingBehavior != QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(single.Parent); // Explicitly detached
            }
            else if (state == EntityState.Detached || queryTrackingBehavior != QueryTrackingBehavior.TrackAll)
            {
                Assert.Equal(
                    CoreStrings.CannotLoadDetachedShadow("Parent", "SingleShadowFk"),
                    Assert.Throws<InvalidOperationException>(() => single.Parent).Message);
            }
            else
            {
                var referenceEntry = context.Entry(single).Reference(e => e.Parent);

                Assert.False(referenceEntry.IsLoaded);

                if (state == EntityState.Deleted)
                {
                    Assert.Null(single.Parent);
                }
                else
                {
                    Assert.NotNull(single.Parent);
                }

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                if (state != EntityState.Deleted)
                {
                    Assert.Same(single, single.Parent!.SingleShadowFk);
                }

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
        else
        {
            Assert.Null(single.Parent);
            Assert.False(context.Entry(single).Reference(e => e.Parent).IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_dependent_shadow_fk(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var parent = context.Set<Parent>().Single();

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(parent).Reference(e => e.SingleShadowFk);

        Assert.False(referenceEntry.IsLoaded);

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(parent.SingleShadowFk); // Explicitly detached
            }
            else
            {
                Assert.NotNull(parent.SingleShadowFk);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

                Assert.Same(parent, parent.SingleShadowFk.Parent);

                if (state != EntityState.Detached)
                {
                    var single = context.ChangeTracker.Entries<SingleShadowFk>().Single().Entity;

                    Assert.Same(single, parent.SingleShadowFk);
                    Assert.Same(parent, single.Parent);
                }
            }
        }
        else
        {
            Assert.Null(parent.SingleShadowFk);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_null_FK_shadow_fk(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var child = context.Attach(new ChildShadowFk { Id = 767 }).Entity;

        if (queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
        {
            context.Entry(child).Property("ParentId").CurrentValue = null;
        }

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior, isAttached: true);

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached)
            {
                Assert.Null(child.Parent); // Explicitly detached
            }
            else if (queryTrackingBehavior != QueryTrackingBehavior.TrackAll
                     && state != EntityState.Added)
            {
                Assert.Equal(
                    CoreStrings.CannotLoadDetachedShadow("Parent", "ChildShadowFk"),
                    Assert.Throws<InvalidOperationException>(() => child.Parent).Message);
            }
            else
            {
                var referenceEntry = context.Entry(child).Reference(e => e.Parent);

                Assert.False(referenceEntry.IsLoaded);

                Assert.Null(child.Parent);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Single(context.ChangeTracker.Entries());
                Assert.Null(child.Parent);
            }
        }
        else
        {
            Assert.Null(child.Parent);
            Assert.False(context.Entry(child).Reference(e => e.Parent).IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_null_FK_shadow_fk(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var single = context.Attach(new SingleShadowFk { Id = 767 }).Entity;

        if (queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
        {
            context.Entry(single).Property("ParentId").CurrentValue = null;
        }

        ClearLog();

        SetState(context, single, state, queryTrackingBehavior, isAttached: true);

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached)
            {
                Assert.Null(single.Parent);
            }
            else if (queryTrackingBehavior != QueryTrackingBehavior.TrackAll
                     && state != EntityState.Added)
            {
                Assert.Equal(
                    CoreStrings.CannotLoadDetachedShadow("Parent", "SingleShadowFk"),
                    Assert.Throws<InvalidOperationException>(() => single.Parent).Message);
            }
            else
            {
                var referenceEntry = context.Entry(single).Reference(e => e.Parent);

                Assert.False(referenceEntry.IsLoaded);

                Assert.Null(single.Parent);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Single(context.ChangeTracker.Entries());

                Assert.Null(single.Parent);
            }
        }
        else
        {
            Assert.Null(single.Parent);
            Assert.False(context.Entry(single).Reference(e => e.Parent).IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_collection_composite_key(EntityState state, QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var parent = context.Set<Parent>().Single();

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior);

        var collectionEntry = context.Entry(parent).Collection(e => e.ChildrenCompositeKey);

        Assert.False(collectionEntry.IsLoaded);

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(parent.ChildrenCompositeKey); // Explicitly detached
            }
            else
            {
                Assert.NotNull(parent.ChildrenCompositeKey);

                Assert.True(collectionEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(2, parent.ChildrenCompositeKey.Count());
                Assert.All(parent.ChildrenCompositeKey.Select(e => e.Parent), p => Assert.Same(parent, p));
            }

            Assert.Equal(state == EntityState.Detached ? 0 : 3, context.ChangeTracker.Entries().Count());
        }
        else
        {
            Assert.Null(parent.ChildrenCompositeKey);
            Assert.False(collectionEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_composite_key(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var child = context.Set<ChildCompositeKey>().Single(e => e.Id == 52);

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(child.Parent); // Explicitly detached
            }
            else
            {
                if (state == EntityState.Deleted)
                {
                    Assert.Null(child.Parent);
                }
                else
                {
                    Assert.NotNull(child.Parent);
                }

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

                if (state != EntityState.Deleted)
                {
                    Assert.Same(child, child.Parent!.ChildrenCompositeKey.Single());
                }

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
        }
        else
        {
            Assert.Null(child.Parent);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_composite_key(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var single = context.Set<SingleCompositeKey>().Single();

        ClearLog();

        SetState(context, single, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(single.Parent); // Explicitly detached
            }
            else
            {
                if (state == EntityState.Deleted)
                {
                    Assert.Null(single.Parent);
                }
                else
                {
                    Assert.NotNull(single.Parent);
                }

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

                if (state != EntityState.Deleted)
                {
                    Assert.Same(single, single.Parent!.SingleCompositeKey);
                }

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
        }
        else
        {
            Assert.Null(single.Parent);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_dependent_composite_key(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var parent = context.Set<Parent>().Single();

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(parent).Reference(e => e.SingleCompositeKey);

        Assert.False(referenceEntry.IsLoaded);

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(parent.SingleCompositeKey); // Explicitly detached
            }
            else
            {
                Assert.NotNull(parent.SingleCompositeKey);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

                Assert.Same(parent, parent.SingleCompositeKey.Parent);

                if (state != EntityState.Detached)
                {
                    var single = context.ChangeTracker.Entries<SingleCompositeKey>().Single().Entity;

                    Assert.Same(single, parent.SingleCompositeKey);
                    Assert.Same(parent, single.Parent);
                }
            }
        }
        else
        {
            Assert.Null(parent.SingleCompositeKey);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_null_FK_composite_key(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var child = context.Attach(new ChildCompositeKey { Id = 767, ParentId = 567 }).Entity;

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        if (LazyLoadingEnabled)
        {
            Assert.Null(child.Parent);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
            Assert.Null(child.Parent);
        }
        else
        {
            Assert.Null(child.Parent);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_null_FK_composite_key(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var single = context.Attach(new SingleCompositeKey { Id = 767, ParentAlternateId = "Boot" }).Entity;

        ClearLog();

        SetState(context, single, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        if (LazyLoadingEnabled)
        {
            Assert.Null(single.Parent);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());

            Assert.Null(single.Parent);
        }
        else
        {
            Assert.Null(single.Parent);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_collection_full_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<ParentFullLoaderByConstructor>().Single();

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior);

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        Assert.False(collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(await parent.LazyLoadChildren(async)); // Explicitly detached
            }
            else
            {
                Assert.NotNull(await parent.LazyLoadChildren(async));

                Assert.False(changeDetector.DetectChangesCalled);

                Assert.True(collectionEntry.IsLoaded);

                Assert.All(parent.Children.Select(e => e.Parent), p => Assert.Same(parent, p));

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(2, parent.Children.Count());
            }

            Assert.Equal(state == EntityState.Detached ? 0 : 3, context.ChangeTracker.Entries().Count());
        }
        else
        {
            Assert.Null(await parent.LazyLoadChildren(async));
            Assert.False(collectionEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_many_to_one_reference_to_principal_full_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.Set<ChildFullLoaderByConstructor>().Single(e => e.Id == 12);

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(await child.LazyLoadParent(async)); // Explicitly detached
            }
            else
            {
                if (state == EntityState.Deleted)
                {
                    Assert.Null(await child.LazyLoadParent(async));
                }
                else
                {
                    Assert.NotNull(await child.LazyLoadParent(async));
                }

                Assert.False(changeDetector.DetectChangesCalled);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

                if (state != EntityState.Deleted)
                {
                    Assert.Same(child, child.Parent!.Children.Single());
                }

                if (state != EntityState.Detached)
                {
                    var parent = context.ChangeTracker.Entries<ParentFullLoaderByConstructor>().Single().Entity;

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
        }
        else
        {
            Assert.Null(await child.LazyLoadParent(async));
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_one_to_one_reference_to_principal_full_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Set<SingleFullLoaderByConstructor>().Single();

        ClearLog();

        SetState(context, single, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(await single.LazyLoadParent(async)); // Explicitly detached
            }
            else
            {
                if (state == EntityState.Deleted)
                {
                    Assert.Null(await single.LazyLoadParent(async));
                }
                else
                {
                    Assert.NotNull(await single.LazyLoadParent(async));
                }

                Assert.False(changeDetector.DetectChangesCalled);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

                if (state != EntityState.Deleted)
                {
                    Assert.Same(single, single.Parent!.Single);
                }

                if (state != EntityState.Detached)
                {
                    var parent = context.ChangeTracker.Entries<ParentFullLoaderByConstructor>().Single().Entity;

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
        }
        else
        {
            Assert.Null(await single.LazyLoadParent(async));
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_one_to_one_reference_to_dependent_full_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<ParentFullLoaderByConstructor>().Single();

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(await parent.LazyLoadSingle(async)); // Explicitly detached
            }
            else
            {
                Assert.NotNull(await parent.LazyLoadSingle(async));

                Assert.False(changeDetector.DetectChangesCalled);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

                if (state != EntityState.Deleted)
                {
                    Assert.Same(parent, parent.Single.Parent);
                }

                if (state != EntityState.Detached)
                {
                    var single = context.ChangeTracker.Entries<SingleFullLoaderByConstructor>().Single().Entity;

                    Assert.Same(single, parent.Single);
                    Assert.Same(parent, single.Parent);
                }
            }
        }
        else
        {
            Assert.Null(await parent.LazyLoadSingle(async));
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_many_to_one_reference_to_principal_null_FK_full_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.Attach(new ChildFullLoaderByConstructor { Id = 767, ParentId = null }).Entity;

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            Assert.Null(await child.LazyLoadParent(async));

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
            Assert.Null(child.Parent);
        }
        else
        {
            Assert.Null(await child.LazyLoadParent(async));
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_one_to_one_reference_to_principal_null_FK_full_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Attach(new SingleFullLoaderByConstructor { Id = 767, ParentId = null }).Entity;

        ClearLog();

        SetState(context, single, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            Assert.Null(await single.LazyLoadParent(async));

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());

            Assert.Null(single.Parent);
        }
        else
        {
            Assert.Null(await single.LazyLoadParent(async));
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_collection_not_found_full_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Attach(new ParentFullLoaderByConstructor { Id = 767 }).Entity;

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior, isAttached: true);

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        Assert.False(collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached)
            {
                Assert.Null(await parent.LazyLoadChildren(async)); // Explicitly detached
            }
            else
            {
                Assert.Empty(await parent.LazyLoadChildren(async));
                Assert.False(changeDetector.DetectChangesCalled);
                Assert.True(collectionEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Single(context.ChangeTracker.Entries());
            }
        }
        else
        {
            Assert.Null(await parent.LazyLoadChildren(async));
            Assert.False(collectionEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_many_to_one_reference_to_principal_not_found_full_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.Attach(new ChildFullLoaderByConstructor { Id = 767, ParentId = 787 }).Entity;

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            Assert.Null(await child.LazyLoadParent(async));

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
            Assert.Null(child.Parent);
        }
        else
        {
            Assert.Null(await child.LazyLoadParent(async));
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_one_to_one_reference_to_principal_not_found_full_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Attach(new SingleFullLoaderByConstructor { Id = 767, ParentId = 787 }).Entity;

        ClearLog();

        SetState(context, single, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            Assert.Null(await single.LazyLoadParent(async));

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());

            Assert.Null(single.Parent);
        }
        else
        {
            Assert.Null(await single.LazyLoadParent(async));
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_one_to_one_reference_to_dependent_not_found_full_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Attach(new ParentFullLoaderByConstructor { Id = 767 }).Entity;

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            Assert.Null(await parent.LazyLoadSingle(async));

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Null(parent.Single);

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
        }
        else
        {
            Assert.Null(await parent.LazyLoadSingle(async));
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_collection_already_loaded_full_loader_constructor_injection(
        EntityState state,
        CascadeTiming deleteOrphansTiming,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<ParentFullLoaderByConstructor>().Include(e => e.Children).Single();

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior);

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        Assert.True(collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.NotNull(await parent.LazyLoadChildren(async));

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.True(collectionEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(2, parent.Children.Count());

        if (queryTrackingBehavior == QueryTrackingBehavior.TrackAll
            && state == EntityState.Deleted
            && deleteOrphansTiming != CascadeTiming.Never)
        {
            Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Null(c));
        }
        else
        {
            Assert.All(parent.Children.Select(e => e.Parent), p => Assert.Same(parent, p));
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_many_to_one_reference_to_principal_already_loaded_full_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.Set<ChildFullLoaderByConstructor>().Include(e => e.Parent).Single(e => e.Id == 12);

        ClearLog();

        SetState(context, child.Parent, state, queryTrackingBehavior);
        SetState(context, child, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        if (state == EntityState.Deleted && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
        {
            Assert.False(referenceEntry.IsLoaded);
            Assert.Null(await child.LazyLoadParent(async));
        }
        else
        {
            Assert.True(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.NotNull(await child.LazyLoadParent(async));

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

            Assert.Same(child, child.Parent.Children.Single());

            if (state != EntityState.Detached)
            {
                var parent = context.ChangeTracker.Entries<ParentFullLoaderByConstructor>().Single().Entity;

                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.Children.Single());
            }
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    public virtual async Task Lazy_load_one_to_one_reference_to_principal_already_loaded_full_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Set<SingleFullLoaderByConstructor>().Include(e => e.Parent).Single();

        ClearLog();

        SetState(context, single.Parent, state, queryTrackingBehavior);
        SetState(context, single, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        if (state == EntityState.Deleted && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
        {
            Assert.False(referenceEntry.IsLoaded);
            Assert.Null(await single.LazyLoadParent(async));
        }
        else
        {
            Assert.True(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.NotNull(await single.LazyLoadParent(async));

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

            Assert.Same(single, single.Parent.Single);

            if (state != EntityState.Detached)
            {
                var parent = context.ChangeTracker.Entries<ParentFullLoaderByConstructor>().Single().Entity;

                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.Single);
            }
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, true)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, true)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll, false)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking, false)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
    public virtual async Task Lazy_load_one_to_one_reference_to_dependent_already_loaded_full_loader_constructor_injection(
        EntityState state,
        CascadeTiming deleteOrphansTiming,
        QueryTrackingBehavior queryTrackingBehavior,
        bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<ParentFullLoaderByConstructor>().Include(e => e.Single).Single();

        ClearLog();

        SetState(context, parent.Single, state, queryTrackingBehavior);
        SetState(context, parent, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        Assert.True(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.NotNull(await parent.LazyLoadSingle(async));

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state == EntityState.Deleted
            && deleteOrphansTiming != CascadeTiming.Never)
        {
            Assert.Same(parent, parent.Single.Parent);
        }

        if (state != EntityState.Detached)
        {
            var single = context.ChangeTracker.Entries<SingleFullLoaderByConstructor>().Single().Entity;

            Assert.Same(single, parent.Single);
            Assert.Same(parent, single.Parent);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_collection_already_partially_loaded_full_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;

        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        context.ChangeTracker.LazyLoadingEnabled = false;

        var child = context.Set<ChildFullLoaderByConstructor>().OrderBy(e => e.Id).First();
        var parent = context.Set<ParentFullLoaderByConstructor>().Single();
        if (parent.Children == null)
        {
            parent.Children = new List<ChildFullLoaderByConstructor> { child };
            child.Parent = parent;
        }

        context.ChangeTracker.LazyLoadingEnabled = true;

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior);
        SetState(context, parent, state, queryTrackingBehavior);

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        Assert.False(collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.NotNull(parent.Children);

        Assert.False(changeDetector.DetectChangesCalled);

        RecordLog();

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.False(collectionEntry.IsLoaded); // Explicitly detached
                Assert.Equal(1, parent.Children.Count());

                Assert.All(parent.Children.Select(e => e.Parent), p => Assert.Same(parent, p));
            }
            else
            {
                Assert.True(collectionEntry.IsLoaded);

                context.ChangeTracker.LazyLoadingEnabled = false;

                // Note that when detached there is no identity resolution, so loading results in duplicates
                Assert.Equal(
                    state == EntityState.Detached && queryTrackingBehavior != QueryTrackingBehavior.NoTrackingWithIdentityResolution
                        ? 3
                        : 2, parent.Children.Count());

                Assert.All(parent.Children.Select(e => e.Parent), p => Assert.Same(parent, p));
            }
        }
        else
        {
            Assert.False(collectionEntry.IsLoaded);
            Assert.Equal(1, parent.Children.Count());
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_collection_delegate_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<ParentDelegateLoaderByConstructor>().Single();

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior);

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        Assert.False(collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            Assert.NotNull(parent.Children);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(collectionEntry.IsLoaded);

            Assert.All(parent.Children.Select(e => e.Parent), p => Assert.Same(parent, p));

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, parent.Children.Count());

            Assert.Equal(state == EntityState.Detached ? 0 : 3, context.ChangeTracker.Entries().Count());
        }
        else
        {
            Assert.Null(parent.Children);
            Assert.False(collectionEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_delegate_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.Set<ChildDelegateLoaderByConstructor>().Single(e => e.Id == 12);

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Deleted)
            {
                Assert.Null(child.Parent);
            }
            else
            {
                Assert.NotNull(child.Parent);
            }

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

            if (state != EntityState.Deleted)
            {
                Assert.Same(child, child.Parent!.Children.Single());
            }

            if (state != EntityState.Detached)
            {
                var parent = context.ChangeTracker.Entries<ParentDelegateLoaderByConstructor>().Single().Entity;

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
        else
        {
            Assert.Null(child.Parent);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_delegate_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Set<SingleDelegateLoaderByConstructor>().Single();

        ClearLog();

        SetState(context, single, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Deleted)
            {
                Assert.Null(single.Parent);
            }
            else
            {
                Assert.NotNull(single.Parent);
            }

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

            if (state != EntityState.Deleted)
            {
                Assert.Same(single, single.Parent!.Single);
            }

            if (state != EntityState.Detached)
            {
                var parent = context.ChangeTracker.Entries<ParentDelegateLoaderByConstructor>().Single().Entity;

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
        else
        {
            Assert.Null(single.Parent);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_dependent_delegate_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<ParentDelegateLoaderByConstructor>().Single();

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            Assert.NotNull(parent.Single);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

            if (state != EntityState.Deleted)
            {
                Assert.Same(parent, parent.Single.Parent);
            }

            if (state != EntityState.Detached)
            {
                var single = context.ChangeTracker.Entries<SingleDelegateLoaderByConstructor>().Single().Entity;

                Assert.Same(single, parent.Single);
                Assert.Same(parent, single.Parent);
            }
        }
        else
        {
            Assert.Null(parent.Single);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_null_FK_delegate_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.Attach(new ChildDelegateLoaderByConstructor { Id = 767, ParentId = null }).Entity;

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.Null(child.Parent);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.False(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
        Assert.Null(child.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_null_FK_delegate_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Attach(new SingleDelegateLoaderByConstructor { Id = 767, ParentId = null }).Entity;

        ClearLog();

        SetState(context, single, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.Null(single.Parent);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.False(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());

        Assert.Null(single.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_collection_not_found_delegate_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Attach(new ParentDelegateLoaderByConstructor { Id = 767 }).Entity;

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior, isAttached: true);

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        Assert.False(collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        // Delegate not set because delegate constructor not called
        Assert.Null(parent.Children);
        Assert.False(changeDetector.DetectChangesCalled);
        Assert.False(collectionEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_not_found_delegate_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.Attach(new ChildDelegateLoaderByConstructor { Id = 767, ParentId = 787 }).Entity;

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.Null(child.Parent);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.False(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
        Assert.Null(child.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_not_found_delegate_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Attach(new SingleDelegateLoaderByConstructor { Id = 767, ParentId = 787 }).Entity;

        ClearLog();

        SetState(context, single, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.Null(single.Parent);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.False(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());

        Assert.Null(single.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_dependent_not_found_delegate_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Attach(new ParentDelegateLoaderByConstructor { Id = 767 }).Entity;

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.Null(parent.Single);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.False(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Null(parent.Single);

        Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_collection_already_loaded_delegate_loader_constructor_injection(
        EntityState state,
        CascadeTiming deleteOrphansTiming,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<ParentDelegateLoaderByConstructor>().Include(e => e.Children).Single();

        ClearLog();

        context.ChangeTracker.LazyLoadingEnabled = false;

        foreach (var child in parent.Children)
        {
            SetState(context, child, state, queryTrackingBehavior);
        }

        SetState(context, parent, state, queryTrackingBehavior);

        context.ChangeTracker.LazyLoadingEnabled = true;

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        Assert.Equal(queryTrackingBehavior == QueryTrackingBehavior.TrackAll && state != EntityState.Detached, collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        var originalLoadedState = collectionEntry.IsLoaded;

        Assert.NotNull(parent.Children);

        Assert.False(changeDetector.DetectChangesCalled);

        if (LazyLoadingEnabled)
        {
            Assert.True(collectionEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            // Note that when detached there is no identity resolution, so loading results in duplicates
            Assert.Equal(
                state == EntityState.Detached && queryTrackingBehavior != QueryTrackingBehavior.NoTrackingWithIdentityResolution
                    ? 4
                    : 2, parent.Children.Count());

            Assert.All(parent.Children.Select(e => e.Parent), p => Assert.Same(parent, p));
        }
        else
        {
            Assert.Equal(originalLoadedState, collectionEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_already_loaded_delegate_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.Set<ChildDelegateLoaderByConstructor>().Include(e => e.Parent).Single(e => e.Id == 12);

        ClearLog();

        SetState(context, child.Parent, state, queryTrackingBehavior);
        SetState(context, child, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        if (state == EntityState.Deleted && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
        {
            Assert.False(referenceEntry.IsLoaded);
            Assert.Null(child.Parent);
        }
        else
        {
            // Delegate loader cannot influence IsLoader flag
            Assert.Equal(queryTrackingBehavior == QueryTrackingBehavior.TrackAll && state != EntityState.Detached, referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.NotNull(child.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            // Delegate loader cannot influence IsLoader flag
            Assert.Equal(queryTrackingBehavior == QueryTrackingBehavior.TrackAll && state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

            Assert.Same(child, child.Parent.Children.Single());

            if (state != EntityState.Detached)
            {
                var parent = context.ChangeTracker.Entries<ParentDelegateLoaderByConstructor>().Single().Entity;

                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.Children.Single());
            }
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_already_loaded_delegate_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Set<SingleDelegateLoaderByConstructor>().Include(e => e.Parent).Single();

        ClearLog();

        SetState(context, single.Parent, state, queryTrackingBehavior);
        SetState(context, single, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        if (state == EntityState.Deleted && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
        {
            Assert.False(referenceEntry.IsLoaded);
            Assert.Null(single.Parent);
        }
        else
        {
            Assert.Equal(queryTrackingBehavior == QueryTrackingBehavior.TrackAll && state != EntityState.Detached, referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.NotNull(single.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.Equal(queryTrackingBehavior == QueryTrackingBehavior.TrackAll && state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

            Assert.Same(single, single.Parent.Single);

            if (state != EntityState.Detached)
            {
                var parent = context.ChangeTracker.Entries<ParentDelegateLoaderByConstructor>().Single().Entity;

                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.Single);
            }
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_dependent_already_loaded_delegate_loader_constructor_injection(
        EntityState state,
        CascadeTiming deleteOrphansTiming,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<ParentDelegateLoaderByConstructor>().Include(e => e.Single).Single();

        ClearLog();

        SetState(context, parent.Single, state, queryTrackingBehavior);
        SetState(context, parent, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        Assert.Equal(queryTrackingBehavior == QueryTrackingBehavior.TrackAll && state != EntityState.Detached, referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.NotNull(parent.Single);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.Equal(queryTrackingBehavior == QueryTrackingBehavior.TrackAll && state != EntityState.Detached, referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state == EntityState.Deleted
            && deleteOrphansTiming != CascadeTiming.Never)
        {
            Assert.Same(parent, parent.Single.Parent);
        }

        if (state != EntityState.Detached)
        {
            var single = context.ChangeTracker.Entries<SingleDelegateLoaderByConstructor>().Single().Entity;

            Assert.Same(single, parent.Single);
            Assert.Same(parent, single.Parent);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_collection_already_partially_loaded_delegate_loader_constructor_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;

        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        context.ChangeTracker.LazyLoadingEnabled = false;

        var child = context.Set<ChildDelegateLoaderByConstructor>().OrderBy(e => e.Id).First();
        var parent = context.Set<ParentDelegateLoaderByConstructor>().Single();
        if (parent.Children == null)
        {
            parent.Children = new List<ChildDelegateLoaderByConstructor> { child };
            child.Parent = parent;
        }

        context.ChangeTracker.LazyLoadingEnabled = true;

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior);
        SetState(context, parent, state, queryTrackingBehavior);

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        Assert.False(collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.NotNull(parent.Children);

        Assert.False(changeDetector.DetectChangesCalled);

        RecordLog();

        if (LazyLoadingEnabled)
        {
            Assert.True(collectionEntry.IsLoaded);

            context.ChangeTracker.LazyLoadingEnabled = false;

            // Note that when detached there is no identity resolution, so loading results in duplicates
            Assert.Equal(
                state == EntityState.Detached && queryTrackingBehavior != QueryTrackingBehavior.NoTrackingWithIdentityResolution
                    ? 3
                    : 2, parent.Children.Count());

            Assert.All(parent.Children.Select(e => e.Parent), p => Assert.Same(parent, p));
        }
        else
        {
            Assert.False(collectionEntry.IsLoaded);
            Assert.Single(parent.Children);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_collection_delegate_loader_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<ParentDelegateLoaderByProperty>().Single();

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior);

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        Assert.False(collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(parent.Children); // Explicitly detached
            }
            else
            {
                Assert.NotNull(parent.Children);

                Assert.False(changeDetector.DetectChangesCalled);

                Assert.True(collectionEntry.IsLoaded);

                Assert.All(parent.Children.Select(e => e.Parent), p => Assert.Same(parent, p));

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(2, parent.Children.Count());
            }

            Assert.Equal(state == EntityState.Detached ? 0 : 3, context.ChangeTracker.Entries().Count());
        }
        else
        {
            Assert.Null(parent.Children);
            Assert.False(collectionEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_delegate_loader_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.Set<ChildDelegateLoaderByProperty>().Single(e => e.Id == 12);

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(child.Parent); // Explicitly detached
            }
            else
            {
                if (state == EntityState.Deleted)
                {
                    Assert.Null(child.Parent);
                }
                else
                {
                    Assert.NotNull(child.Parent);
                }

                Assert.False(changeDetector.DetectChangesCalled);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

                if (state != EntityState.Deleted)
                {
                    Assert.Same(child, child.Parent!.Children.Single());
                }

                if (state != EntityState.Detached)
                {
                    var parent = context.ChangeTracker.Entries<ParentDelegateLoaderByProperty>().Single().Entity;

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
        }
        else
        {
            Assert.Null(child.Parent);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_delegate_loader_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Set<SingleDelegateLoaderByProperty>().Single();

        ClearLog();

        SetState(context, single, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(single.Parent); // Explicitly detached
            }
            else
            {
                if (state == EntityState.Deleted)
                {
                    Assert.Null(single.Parent);
                }
                else
                {
                    Assert.NotNull(single.Parent);
                }

                Assert.False(changeDetector.DetectChangesCalled);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

                if (state != EntityState.Deleted)
                {
                    Assert.Same(single, single.Parent!.Single);
                }

                if (state != EntityState.Detached)
                {
                    var parent = context.ChangeTracker.Entries<ParentDelegateLoaderByProperty>().Single().Entity;

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
        }
        else
        {
            Assert.Null(single.Parent);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_dependent_delegate_loader_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<ParentDelegateLoaderByProperty>().Single();

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(parent.Single); // Explicitly detached
            }
            else
            {
                Assert.NotNull(parent.Single);

                Assert.False(changeDetector.DetectChangesCalled);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

                if (state != EntityState.Deleted)
                {
                    Assert.Same(parent, parent.Single.Parent);
                }

                if (state != EntityState.Detached)
                {
                    var single = context.ChangeTracker.Entries<SingleDelegateLoaderByProperty>().Single().Entity;

                    Assert.Same(single, parent.Single);
                    Assert.Same(parent, single.Parent);
                }
            }
        }
        else
        {
            Assert.Null(parent.Single);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_null_FK_delegate_loader_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.Attach(new ChildDelegateLoaderByProperty { Id = 767, ParentId = null }).Entity;

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            Assert.Null(child.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
            Assert.Null(child.Parent);
        }
        else
        {
            Assert.Null(child.Parent);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_null_FK_delegate_loader_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Attach(new SingleDelegateLoaderByProperty { Id = 767, ParentId = null }).Entity;

        ClearLog();

        SetState(context, single, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            Assert.Null(single.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());

            Assert.Null(single.Parent);
        }
        else
        {
            Assert.Null(single.Parent);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_collection_not_found_delegate_loader_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Attach(new ParentDelegateLoaderByProperty { Id = 767 }).Entity;

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior, isAttached: true);

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        Assert.False(collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached)
            {
                Assert.Null(parent.Children); // Explicitly detached
            }
            else
            {
                Assert.Empty(parent.Children);
                Assert.False(changeDetector.DetectChangesCalled);
                Assert.True(collectionEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Single(context.ChangeTracker.Entries());
            }
        }
        else
        {
            Assert.Null(parent.Children);
            Assert.False(collectionEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_not_found_delegate_loader_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.Attach(new ChildDelegateLoaderByProperty { Id = 767, ParentId = 787 }).Entity;

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            Assert.Null(child.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
            Assert.Null(child.Parent);
        }
        else
        {
            Assert.Null(child.Parent);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_not_found_delegate_loader_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Attach(new SingleDelegateLoaderByProperty { Id = 767, ParentId = 787 }).Entity;

        ClearLog();

        SetState(context, single, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            Assert.Null(single.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());

            Assert.Null(single.Parent);
        }
        else
        {
            Assert.Null(single.Parent);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_dependent_not_found_delegate_loader_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Attach(new ParentDelegateLoaderByProperty { Id = 767 }).Entity;

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            Assert.Null(parent.Single);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Null(parent.Single);

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
        }
        else
        {
            Assert.Null(parent.Single);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_collection_already_loaded_delegate_loader_property_injection(
        EntityState state,
        CascadeTiming deleteOrphansTiming,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<ParentDelegateLoaderByProperty>().Include(e => e.Children).Single();

        ClearLog();

        context.ChangeTracker.LazyLoadingEnabled = false;

        foreach (var child in parent.Children)
        {
            SetState(context, child, state, queryTrackingBehavior);
        }

        SetState(context, parent, state, queryTrackingBehavior);

        context.ChangeTracker.LazyLoadingEnabled = true;

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        // Loader delegate has no way of recording loader state for untracked queries or detached entities
        Assert.Equal(queryTrackingBehavior == QueryTrackingBehavior.TrackAll && state != EntityState.Detached, collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        var originalLoadedState = collectionEntry.IsLoaded;

        Assert.NotNull(parent.Children);

        Assert.False(changeDetector.DetectChangesCalled);

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.False(collectionEntry.IsLoaded); // Explicitly detached
                Assert.Equal(2, parent.Children.Count());
            }
            else
            {
                Assert.True(collectionEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                // Note that when detached there is no identity resolution, so loading results in duplicates
                Assert.Equal(
                    state == EntityState.Detached && queryTrackingBehavior != QueryTrackingBehavior.NoTrackingWithIdentityResolution
                        ? 4
                        : 2, parent.Children.Count());
            }

            Assert.All(parent.Children.Select(e => e.Parent), p => Assert.Same(parent, p));
        }
        else
        {
            Assert.Equal(originalLoadedState, collectionEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_already_loaded_delegate_loader_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.Set<ChildDelegateLoaderByProperty>().Include(e => e.Parent).Single(e => e.Id == 12);

        ClearLog();

        SetState(context, child.Parent, state, queryTrackingBehavior);
        SetState(context, child, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        if (state == EntityState.Deleted && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
        {
            Assert.False(referenceEntry.IsLoaded);
            Assert.Null(child.Parent);
        }
        else
        {
            Assert.Equal(queryTrackingBehavior == QueryTrackingBehavior.TrackAll && state != EntityState.Detached, referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.NotNull(child.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.Equal(queryTrackingBehavior == QueryTrackingBehavior.TrackAll && state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

            Assert.Same(child, child.Parent.Children.Single());

            if (state != EntityState.Detached)
            {
                var parent = context.ChangeTracker.Entries<ParentDelegateLoaderByProperty>().Single().Entity;

                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.Children.Single());
            }
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_already_loaded_delegate_loader_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Set<SingleDelegateLoaderByProperty>().Include(e => e.Parent).Single();

        ClearLog();

        SetState(context, single.Parent, state, queryTrackingBehavior);
        SetState(context, single, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        if (state == EntityState.Deleted && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
        {
            Assert.False(referenceEntry.IsLoaded);
            Assert.Null(single.Parent);
        }
        else
        {
            Assert.Equal(queryTrackingBehavior == QueryTrackingBehavior.TrackAll && state != EntityState.Detached, referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.NotNull(single.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.Equal(queryTrackingBehavior == QueryTrackingBehavior.TrackAll && state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

            Assert.Same(single, single.Parent.Single);

            if (state != EntityState.Detached)
            {
                var parent = context.ChangeTracker.Entries<ParentDelegateLoaderByProperty>().Single().Entity;

                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.Single);
            }
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_dependent_already_loaded_delegate_loader_property_injection(
        EntityState state,
        CascadeTiming deleteOrphansTiming,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<ParentDelegateLoaderByProperty>().Include(e => e.Single).Single();

        ClearLog();

        SetState(context, parent.Single, state, queryTrackingBehavior);
        SetState(context, parent, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        Assert.Equal(queryTrackingBehavior == QueryTrackingBehavior.TrackAll && state != EntityState.Detached, referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.NotNull(parent.Single);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.Equal(queryTrackingBehavior == QueryTrackingBehavior.TrackAll && state != EntityState.Detached, referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state == EntityState.Deleted
            && deleteOrphansTiming != CascadeTiming.Never)
        {
            Assert.Same(parent, parent.Single.Parent);
        }

        if (state != EntityState.Detached)
        {
            var single = context.ChangeTracker.Entries<SingleDelegateLoaderByProperty>().Single().Entity;

            Assert.Same(single, parent.Single);
            Assert.Same(parent, single.Parent);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_collection_already_partially_loaded_delegate_loader_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;

        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        context.ChangeTracker.LazyLoadingEnabled = false;

        var child = context.Set<ChildDelegateLoaderByProperty>().OrderBy(e => e.Id).First();
        var parent = context.Set<ParentDelegateLoaderByProperty>().Single();
        if (parent.Children == null)
        {
            parent.Children = new List<ChildDelegateLoaderByProperty> { child };
            child.Parent = parent;
        }

        context.ChangeTracker.LazyLoadingEnabled = true;

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior);
        SetState(context, parent, state, queryTrackingBehavior);

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        Assert.False(collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.NotNull(parent.Children);

        Assert.False(changeDetector.DetectChangesCalled);

        RecordLog();

        if (!LazyLoadingEnabled || (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll))
        {
            Assert.False(collectionEntry.IsLoaded); // Explicitly detached
            Assert.Equal(1, parent.Children.Count());

            Assert.All(parent.Children.Select(e => e.Parent), p => Assert.Same(parent, p));
        }
        else
        {
            Assert.True(collectionEntry.IsLoaded);

            context.ChangeTracker.LazyLoadingEnabled = false;

            // Note that when detached there is no identity resolution, so loading results in duplicates
            Assert.Equal(
                state == EntityState.Detached && queryTrackingBehavior != QueryTrackingBehavior.NoTrackingWithIdentityResolution
                    ? 3
                    : 2, parent.Children.Count());

            Assert.All(parent.Children.Select(e => e.Parent), p => Assert.Same(parent, p));
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_collection_delegate_loader_with_state_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<ParentDelegateLoaderWithStateByProperty>().Single();

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior);

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        Assert.False(collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(parent.Children); // Explicitly detached
            }
            else
            {
                Assert.NotNull(parent.Children);

                Assert.False(changeDetector.DetectChangesCalled);

                Assert.True(collectionEntry.IsLoaded);

                Assert.All(parent.Children.Select(e => e.Parent), p => Assert.Same(parent, p));

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(2, parent.Children.Count());
            }

            Assert.Equal(state == EntityState.Detached ? 0 : 3, context.ChangeTracker.Entries().Count());
        }
        else
        {
            Assert.Null(parent.Children);
            Assert.False(collectionEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_delegate_loader_with_state_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.Set<ChildDelegateLoaderWithStateByProperty>().Single(e => e.Id == 12);

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(child.Parent); // Explicitly detached
            }
            else
            {
                if (state == EntityState.Deleted)
                {
                    Assert.Null(child.Parent);
                }
                else
                {
                    Assert.NotNull(child.Parent);
                }

                Assert.False(changeDetector.DetectChangesCalled);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

                if (state != EntityState.Deleted)
                {
                    Assert.Same(child, child.Parent!.Children.Single());
                }

                if (state != EntityState.Detached)
                {
                    var parent = context.ChangeTracker.Entries<ParentDelegateLoaderWithStateByProperty>().Single().Entity;

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
        }
        else
        {
            Assert.Null(child.Parent);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_delegate_loader_with_state_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Set<SingleDelegateLoaderWithStateByProperty>().Single();

        ClearLog();

        SetState(context, single, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(single.Parent); // Explicitly detached
            }
            else
            {
                if (state == EntityState.Deleted)
                {
                    Assert.Null(single.Parent);
                }
                else
                {
                    Assert.NotNull(single.Parent);
                }

                Assert.False(changeDetector.DetectChangesCalled);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

                if (state != EntityState.Deleted)
                {
                    Assert.Same(single, single.Parent!.Single);
                }

                if (state != EntityState.Detached)
                {
                    var parent = context.ChangeTracker.Entries<ParentDelegateLoaderWithStateByProperty>().Single().Entity;

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
        }
        else
        {
            Assert.Null(single.Parent);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_dependent_delegate_loader_with_state_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<ParentDelegateLoaderWithStateByProperty>().Single();

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.Null(parent.Single); // Explicitly detached
            }
            else
            {
                Assert.NotNull(parent.Single);

                Assert.False(changeDetector.DetectChangesCalled);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

                if (state != EntityState.Deleted)
                {
                    Assert.Same(parent, parent.Single.Parent);
                }

                if (state != EntityState.Detached)
                {
                    var single = context.ChangeTracker.Entries<SingleDelegateLoaderWithStateByProperty>().Single().Entity;

                    Assert.Same(single, parent.Single);
                    Assert.Same(parent, single.Parent);
                }
            }
        }
        else
        {
            Assert.Null(parent.Single);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_null_FK_delegate_loader_with_state_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.Attach(new ChildDelegateLoaderWithStateByProperty { Id = 767, ParentId = null }).Entity;

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            Assert.Null(child.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
            Assert.Null(child.Parent);
        }
        else
        {
            Assert.Null(child.Parent);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_null_FK_delegate_loader_with_state_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Attach(new SingleDelegateLoaderWithStateByProperty { Id = 767, ParentId = null }).Entity;

        ClearLog();

        SetState(context, single, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            Assert.Null(single.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());

            Assert.Null(single.Parent);
        }
        else
        {
            Assert.Null(single.Parent);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_collection_not_found_delegate_loader_with_state_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Attach(new ParentDelegateLoaderWithStateByProperty { Id = 767 }).Entity;

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior, isAttached: true);

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        Assert.False(collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached)
            {
                Assert.Null(parent.Children); // Explicitly detached
            }
            else
            {
                Assert.Empty(parent.Children);
                Assert.False(changeDetector.DetectChangesCalled);
                Assert.True(collectionEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Single(context.ChangeTracker.Entries());
            }
        }
        else
        {
            Assert.Null(parent.Children);
            Assert.False(collectionEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_not_found_delegate_loader_with_state_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.Attach(new ChildDelegateLoaderWithStateByProperty { Id = 767, ParentId = 787 }).Entity;

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            Assert.Null(child.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
            Assert.Null(child.Parent);
        }
        else
        {
            Assert.Null(child.Parent);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_not_found_delegate_loader_with_state_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Attach(new SingleDelegateLoaderWithStateByProperty { Id = 767, ParentId = 787 }).Entity;

        ClearLog();

        SetState(context, single, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            Assert.Null(single.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());

            Assert.Null(single.Parent);
        }
        else
        {
            Assert.Null(single.Parent);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_dependent_not_found_delegate_loader_with_state_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Attach(new ParentDelegateLoaderWithStateByProperty { Id = 767 }).Entity;

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior, isAttached: true);

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        if (LazyLoadingEnabled)
        {
            Assert.Null(parent.Single);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Null(parent.Single);

            Assert.Equal(state == EntityState.Detached ? 0 : 1, context.ChangeTracker.Entries().Count());
        }
        else
        {
            Assert.Null(parent.Single);
            Assert.False(referenceEntry.IsLoaded);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_collection_already_loaded_delegate_loader_with_state_property_injection(
        EntityState state,
        CascadeTiming deleteOrphansTiming,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<ParentDelegateLoaderWithStateByProperty>().Include(e => e.Children).Single();

        ClearLog();

        SetState(context, parent, state, queryTrackingBehavior);

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        Assert.True(collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.NotNull(parent.Children);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.True(collectionEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(2, parent.Children.Count());

        if (queryTrackingBehavior == QueryTrackingBehavior.TrackAll
            && state == EntityState.Deleted
            && deleteOrphansTiming != CascadeTiming.Never)
        {
            Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Null(c));
        }
        else
        {
            Assert.All(parent.Children.Select(e => e.Parent), p => Assert.Same(parent, p));
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_already_loaded_delegate_loader_with_state_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.Set<ChildDelegateLoaderWithStateByProperty>().Include(e => e.Parent).Single(e => e.Id == 12);

        ClearLog();

        SetState(context, child.Parent, state, queryTrackingBehavior);
        SetState(context, child, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        if (state == EntityState.Deleted && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
        {
            Assert.False(referenceEntry.IsLoaded);
            Assert.Null(child.Parent);
        }
        else
        {
            Assert.True(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.NotNull(child.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

            Assert.Same(child, child.Parent.Children.Single());

            if (state != EntityState.Detached)
            {
                var parent = context.ChangeTracker.Entries<ParentDelegateLoaderWithStateByProperty>().Single().Entity;

                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.Children.Single());
            }
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_already_loaded_delegate_loader_with_state_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Set<SingleDelegateLoaderWithStateByProperty>().Include(e => e.Parent).Single();

        ClearLog();

        SetState(context, single.Parent, state, queryTrackingBehavior);
        SetState(context, single, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        if (state == EntityState.Deleted && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
        {
            Assert.False(referenceEntry.IsLoaded);
            Assert.Null(single.Parent);
        }
        else
        {
            Assert.True(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.NotNull(single.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

            Assert.Same(single, single.Parent.Single);

            if (state != EntityState.Detached)
            {
                var parent = context.ChangeTracker.Entries<ParentDelegateLoaderWithStateByProperty>().Single().Entity;

                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.Single);
            }
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_one_to_one_reference_to_dependent_already_loaded_delegate_loader_with_state_property_injection(
        EntityState state,
        CascadeTiming deleteOrphansTiming,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<ParentDelegateLoaderWithStateByProperty>().Include(e => e.Single).Single();

        ClearLog();

        SetState(context, parent.Single, state, queryTrackingBehavior);
        SetState(context, parent, state, queryTrackingBehavior);

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        Assert.True(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.NotNull(parent.Single);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state == EntityState.Deleted
            && deleteOrphansTiming != CascadeTiming.Never)
        {
            Assert.Same(parent, parent.Single.Parent);
        }

        if (state != EntityState.Detached)
        {
            var single = context.ChangeTracker.Entries<SingleDelegateLoaderWithStateByProperty>().Single().Entity;

            Assert.Same(single, parent.Single);
            Assert.Same(parent, single.Parent);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.TrackAll)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTracking)]
    [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Added, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(EntityState.Detached, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_collection_already_partially_loaded_delegate_loader_with_state_property_injection(
        EntityState state,
        QueryTrackingBehavior queryTrackingBehavior)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        context.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;

        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        context.ChangeTracker.LazyLoadingEnabled = false;

        var child = context.Set<ChildDelegateLoaderWithStateByProperty>().OrderBy(e => e.Id).First();
        var parent = context.Set<ParentDelegateLoaderWithStateByProperty>().Single();
        if (parent.Children == null)
        {
            parent.Children = new List<ChildDelegateLoaderWithStateByProperty> { child };
            child.Parent = parent;
        }

        context.ChangeTracker.LazyLoadingEnabled = true;

        ClearLog();

        SetState(context, child, state, queryTrackingBehavior);
        SetState(context, parent, state, queryTrackingBehavior);

        var collectionEntry = context.Entry(parent).Collection(e => e.Children);

        Assert.False(collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.NotNull(parent.Children);

        Assert.False(changeDetector.DetectChangesCalled);

        RecordLog();

        if (LazyLoadingEnabled)
        {
            if (state == EntityState.Detached && queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                Assert.False(collectionEntry.IsLoaded); // Explicitly detached
                Assert.Equal(1, parent.Children.Count());

                Assert.All(parent.Children.Select(e => e.Parent), p => Assert.Same(parent, p));
            }
            else
            {
                Assert.True(collectionEntry.IsLoaded);

                context.ChangeTracker.LazyLoadingEnabled = false;

                // Note that when detached there is no identity resolution, so loading results in duplicates
                Assert.Equal(
                    state == EntityState.Detached && queryTrackingBehavior != QueryTrackingBehavior.NoTrackingWithIdentityResolution
                        ? 3
                        : 2, parent.Children.Count());

                Assert.All(parent.Children.Select(e => e.Parent), p => Assert.Same(parent, p));
            }
        }
        else
        {
            Assert.False(collectionEntry.IsLoaded);
            Assert.Equal(1, parent.Children.Count());
        }
    }

    [ConditionalFact]
    public virtual void Lazy_loading_uses_field_access_when_abstract_base_class_navigation()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        var product = context.Set<SimpleProduct>().Single();
        var deposit = product.Deposit;

        if (LazyLoadingEnabled)
        {
            Assert.NotNull(deposit);
            Assert.Same(deposit, product.Deposit);
        }
        else
        {
            Assert.Null(deposit);
        }
    }

    protected virtual bool LazyLoadingEnabled
        => true;
}
