// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public abstract class LazyLoadProxyTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : LazyLoadProxyTestBase<TFixture>.LoadFixtureBase
{
    protected LazyLoadProxyTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

    [ConditionalTheory] // Issue #32390
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_use_proxies_from_multiple_threads_when_navigations_already_loaded(bool noTracking)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);

        IQueryable<Parent> query = context.Set<Parent>();

        if (noTracking)
        {
            query = query.AsNoTracking();
        }

        var parent = query.Single();

        var children = parent.Children!.ToList();
        var singlePkToPk = parent.SinglePkToPk;
        var single = parent.Single;
        var childrenAk = parent.ChildrenAk!.ToList();
        var singleAk = parent.SingleAk;
        var childrenShadowFk = parent.ChildrenShadowFk!.ToList();
        var singleShadowFk = parent.SingleShadowFk;
        var childrenCompositeKey = parent.ChildrenCompositeKey!.ToList();
        var singleCompositeKey = parent.SingleCompositeKey;
        var withRecursiveProperty = parent.WithRecursiveProperty;
        var manyChildren = parent.ManyChildren!.ToList();

        var tests = new Action[20];
        for (var i = 0; i < 20; i++)
        {
            tests[i] = () =>
            {
                Assert.Equal(children, parent.Children);
                Assert.Equal(singlePkToPk, parent.SinglePkToPk);
                Assert.Equal(single, parent.Single);
                Assert.Equal(childrenAk, parent.ChildrenAk!);
                Assert.Equal(singleAk, parent.SingleAk);
                Assert.Equal(childrenShadowFk, parent.ChildrenShadowFk);
                Assert.Equal(singleShadowFk, parent.SingleShadowFk);
                Assert.Equal(childrenCompositeKey, parent.ChildrenCompositeKey);
                Assert.Equal(singleCompositeKey, parent.SingleCompositeKey);
                Assert.Equal(withRecursiveProperty, parent.WithRecursiveProperty);
                Assert.Equal(manyChildren, parent.ManyChildren);
            };
        }

        Task.WaitAll(tests.Select(Task.Run).ToArray());
    }

    [ConditionalFact]
    public virtual void Detected_principal_reference_navigation_changes_are_detected_and_marked_loaded()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);

        var parent = context.Set<Parent>().Single();

        var single = context.CreateProxy<Single>();
        parent.Single = single;

        Assert.Same(single, parent.Single);
        Assert.True(context.Entry(parent).Reference(e => e.Single).IsLoaded);
    }

    [ConditionalFact]
    public virtual void Detected_dependent_reference_navigation_changes_are_detected_and_marked_loaded()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);

        var single = context.Set<Single>().Single();

        var parent = context.CreateProxy<Parent>();
        single.Parent = parent;

        Assert.Same(parent, single.Parent);
        Assert.True(context.Entry(single).Reference(e => e.Parent).IsLoaded);
    }

    [ConditionalTheory] // Issue #13138
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_one_to_one_reference_with_recursive_property(EntityState state)
    {
        using (var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached))
        {
            var child = context.Set<WithRecursiveProperty>().Single();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            if (state != EntityState.Detached)
            {
                context.Entry(child).State = state;
            }

            Assert.Equal(state != EntityState.Detached, referenceEntry.IsLoaded);

            Assert.NotNull(child.Parent);

            Assert.True(referenceEntry.IsLoaded);

            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

            if (state != EntityState.Detached)
            {
                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Equal(parent.Id, child.IdLoadedFromParent);

                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.WithRecursiveProperty);
            }
        }
    }

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
        var parent = context.CreateProxy<Parent>();
        parent.Id = 707;
        parent.AlternateId = "Root";

        var singlePkToPk = context.CreateProxy<SinglePkToPk>();
        singlePkToPk.Id = 707;
        parent.SinglePkToPk = singlePkToPk;

        var single = context.CreateProxy<Single>();
        single.Id = 21;
        parent.Single = single;

        var singleAk = context.CreateProxy<SingleAk>();
        singleAk.Id = 42;
        parent.SingleAk = singleAk;

        var singleShadowFk = context.CreateProxy<SingleShadowFk>();
        singleShadowFk.Id = 62;
        parent.SingleShadowFk = singleShadowFk;

        var singleCompositeKey = context.CreateProxy<SingleCompositeKey>();
        singleCompositeKey.Id = 62;
        parent.SingleCompositeKey = singleCompositeKey;

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
        var parent = context.CreateProxy<Parent>();
        parent.Id = 707;
        parent.AlternateId = "Root";

        var singlePkToPk = context.CreateProxy<SinglePkToPk>();
        singlePkToPk.Id = 707;
        parent.SinglePkToPk = singlePkToPk;

        var single = context.CreateProxy<Single>();
        single.Id = 21;
        parent.Single = single;

        var singleAk = context.CreateProxy<SingleAk>();
        singleAk.Id = 42;
        parent.SingleAk = singleAk;

        var singleShadowFk = context.CreateProxy<SingleShadowFk>();
        singleShadowFk.Id = 62;
        parent.SingleShadowFk = singleShadowFk;

        var singleCompositeKey = context.CreateProxy<SingleCompositeKey>();
        singleCompositeKey.Id = 62;
        parent.SingleCompositeKey = singleCompositeKey;

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
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Added, true)]
    public virtual void Attached_collections_are_not_marked_as_loaded(EntityState state, bool lazy)
    {
        using var context = CreateContext(lazy);
        var parent = context.CreateProxy<Parent>();
        parent.Id = 707;
        parent.AlternateId = "Root";

        var child1 = context.CreateProxy<Child>();
        child1.Id = 11;
        var child2 = context.CreateProxy<Child>();
        child2.Id = 12;
        parent.Children = new List<Child> { child1, child2 };

        var childAk1 = context.CreateProxy<ChildAk>();
        childAk1.Id = 31;
        var childAk2 = context.CreateProxy<ChildAk>();
        childAk2.Id = 32;
        parent.ChildrenAk = new List<ChildAk> { childAk1, childAk2 };

        var childShadowFk1 = context.CreateProxy<ChildShadowFk>();
        childShadowFk1.Id = 51;
        var childShadowFk2 = context.CreateProxy<ChildShadowFk>();
        childShadowFk2.Id = 52;
        parent.ChildrenShadowFk = new List<ChildShadowFk> { childShadowFk1, childShadowFk2 };

        var childCompositeKey1 = context.CreateProxy<ChildCompositeKey>();
        childCompositeKey1.Id = 51;
        var childCompositeKey2 = context.CreateProxy<ChildCompositeKey>();
        childCompositeKey2.Id = 52;
        parent.ChildrenCompositeKey = new List<ChildCompositeKey> { childCompositeKey1, childCompositeKey2 };

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

        Assert.False(context.Entry(parent).Collection(e => e.Children!).IsLoaded);
        Assert.False(context.Entry(parent).Collection(e => e.ChildrenAk!).IsLoaded);
        Assert.False(context.Entry(parent).Collection(e => e.ChildrenShadowFk!).IsLoaded);
        Assert.False(context.Entry(parent).Collection(e => e.ChildrenCompositeKey!).IsLoaded);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, false, false)]
    [InlineData(EntityState.Added, false, false)]
    [InlineData(EntityState.Modified, false, false)]
    [InlineData(EntityState.Deleted, false, false)]
    [InlineData(EntityState.Detached, false, false)]
    [InlineData(EntityState.Unchanged, true, false)]
    [InlineData(EntityState.Added, true, false)]
    [InlineData(EntityState.Modified, true, false)]
    [InlineData(EntityState.Deleted, true, false)]
    [InlineData(EntityState.Detached, true, false)]
    [InlineData(EntityState.Unchanged, true, true)]
    [InlineData(EntityState.Added, true, true)]
    [InlineData(EntityState.Modified, true, true)]
    [InlineData(EntityState.Deleted, true, true)]
    [InlineData(EntityState.Detached, true, true)]
    public virtual void Lazy_load_collection(EntityState state, bool useAttach, bool useDetach)
    {
        Parent? parent = null;

        if (useAttach)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached))
            {
                parent = context.Set<Parent>().Single();

                if (useDetach)
                {
                    context.Entry(parent).State = EntityState.Detached;
                }
            }

            if (state != EntityState.Detached && useDetach)
            {
                Assert.Null(parent.Children);
            }
            else
            {
                AssertDisposed(() => parent.Children, "MotherProxy", "Children");
            }
        }

        using (var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached))
        {
            var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

            parent ??= context.Set<Parent>().Single();

            ClearLog();

            var collectionEntry = context.Entry(parent).Collection(e => e.Children!);

            if (state != EntityState.Detached)
            {
                context.Entry(parent).State = state;
            }

            Assert.False(collectionEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            if (state == EntityState.Detached && useAttach)
            {
                AssertDisposed(() => parent.Children, "MotherProxy", "Children");
            }
            else
            {
                Assert.NotNull(parent.Children);

                Assert.False(changeDetector.DetectChangesCalled);

                //Assert.True(collectionEntry.IsLoaded);

                Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(2, parent.Children.Count());
                Assert.Equal(state == EntityState.Detached ? 0 : 3, context.ChangeTracker.Entries().Count());
            }
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, false, false)]
    [InlineData(EntityState.Added, false, false)]
    [InlineData(EntityState.Modified, false, false)]
    [InlineData(EntityState.Deleted, false, false)]
    [InlineData(EntityState.Detached, false, false)]
    [InlineData(EntityState.Unchanged, true, false)]
    [InlineData(EntityState.Added, true, false)]
    [InlineData(EntityState.Modified, true, false)]
    [InlineData(EntityState.Deleted, true, false)]
    [InlineData(EntityState.Detached, true, false)]
    [InlineData(EntityState.Unchanged, true, true)]
    [InlineData(EntityState.Added, true, true)]
    [InlineData(EntityState.Modified, true, true)]
    [InlineData(EntityState.Deleted, true, true)]
    [InlineData(EntityState.Detached, true, true)]
    public virtual void Lazy_load_many_to_one_reference_to_principal(EntityState state, bool useAttach, bool useDetach)
    {
        Child? child = null;

        if (useAttach)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached))
            {
                child = context.Set<Child>().Single(e => e.Id == 12);

                if (useDetach)
                {
                    context.Entry(child).State = EntityState.Detached;
                }
            }

            if (state != EntityState.Detached && useDetach)
            {
                Assert.Null(child.Parent);
            }
            else
            {
                AssertDisposed(() => child.Parent, "ChildProxy", "Parent");
            }
        }

        using (var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached))
        {
            var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

            child ??= context.Set<Child>().Single(e => e.Id == 12);

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            if (state != EntityState.Detached)
            {
                context.Entry(child).State = state;
            }

            Assert.False(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            if (state == EntityState.Detached && useAttach)
            {
                AssertDisposed(() => child.Parent, "ChildProxy", "Parent");
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
                    Assert.Contains(child, child.Parent!.Children!);
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
                        Assert.Same(child, parent.Children!.Single());
                    }
                }
            }
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, false, false)]
    [InlineData(EntityState.Added, false, false)]
    [InlineData(EntityState.Modified, false, false)]
    [InlineData(EntityState.Deleted, false, false)]
    [InlineData(EntityState.Detached, false, false)]
    [InlineData(EntityState.Unchanged, true, false)]
    [InlineData(EntityState.Added, true, false)]
    [InlineData(EntityState.Modified, true, false)]
    [InlineData(EntityState.Deleted, true, false)]
    [InlineData(EntityState.Detached, true, false)]
    [InlineData(EntityState.Unchanged, true, true)]
    [InlineData(EntityState.Added, true, true)]
    [InlineData(EntityState.Modified, true, true)]
    [InlineData(EntityState.Deleted, true, true)]
    [InlineData(EntityState.Detached, true, true)]
    public virtual void Lazy_load_one_to_one_reference_to_principal(EntityState state, bool useAttach, bool useDetach)
    {
        Single? single = null;

        if (useAttach)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached))
            {
                single = context.Set<Single>().Single();

                if (useDetach)
                {
                    context.Entry(single).State = EntityState.Detached;
                }
            }

            if (state != EntityState.Detached && useDetach)
            {
                Assert.Null(single.Parent);
            }
            else
            {
                AssertDisposed(() => single.Parent, "SingleProxy", "Parent");
            }
        }

        using (var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached))
        {
            var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

            single ??= context.Set<Single>().Single();

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            if (state != EntityState.Detached)
            {
                context.Entry(single).State = state;
            }

            Assert.False(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            if (state == EntityState.Detached && useAttach)
            {
                AssertDisposed(() => single.Parent, "SingleProxy", "Parent");
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

                Assert.False(changeDetector.DetectChangesCalled);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                if (state != EntityState.Deleted)
                {
                    Assert.Same(single, single.Parent!.Single);
                }

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
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, false, false)]
    [InlineData(EntityState.Added, false, false)]
    [InlineData(EntityState.Modified, false, false)]
    [InlineData(EntityState.Deleted, false, false)]
    [InlineData(EntityState.Detached, false, false)]
    [InlineData(EntityState.Unchanged, true, false)]
    [InlineData(EntityState.Added, true, false)]
    [InlineData(EntityState.Modified, true, false)]
    [InlineData(EntityState.Deleted, true, false)]
    [InlineData(EntityState.Detached, true, false)]
    [InlineData(EntityState.Unchanged, true, true)]
    [InlineData(EntityState.Added, true, true)]
    [InlineData(EntityState.Modified, true, true)]
    [InlineData(EntityState.Deleted, true, true)]
    [InlineData(EntityState.Detached, true, true)]
    public virtual void Lazy_load_one_to_one_reference_to_dependent(EntityState state, bool useAttach, bool useDetach)
    {
        Parent? parent = null;

        if (useAttach)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached))
            {
                parent = context.Set<Parent>().Single();

                if (useDetach)
                {
                    context.Entry(parent).State = EntityState.Detached;
                }
            }

            if (state != EntityState.Detached && useDetach)
            {
                Assert.Null(parent.Single);
            }
            else
            {
                AssertDisposed(() => parent.Single, "MotherProxy", "Single");
            }
        }

        using (var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached))
        {
            var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

            parent ??= context.Set<Parent>().Single();

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.Single);

            if (state != EntityState.Detached)
            {
                context.Entry(parent).State = state;
            }

            Assert.False(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            if (state == EntityState.Detached && useAttach)
            {
                AssertDisposed(() => parent.Single, "MotherProxy", "Single");
            }
            else
            {
                Assert.NotNull(parent.Single);
                Assert.Same(parent, parent.Single.Parent);

                Assert.False(changeDetector.DetectChangesCalled);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

                if (state != EntityState.Detached)
                {
                    var single = context.ChangeTracker.Entries<Single>().Single().Entity;

                    Assert.Same(single, parent.Single);
                    Assert.Same(parent, single.Parent);
                }
            }
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_one_to_one_PK_to_PK_reference_to_principal(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Set<SinglePkToPk>().Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        if (state != EntityState.Detached)
        {
            context.Entry(single).State = state;
        }

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

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

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_one_to_one_PK_to_PK_reference_to_dependent(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<Parent>().Single();

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.SinglePkToPk);

        if (state != EntityState.Detached)
        {
            context.Entry(parent).State = state;
        }

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.NotNull(parent.SinglePkToPk);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Deleted)
        {
            Assert.Same(parent, parent.SinglePkToPk.Parent);
        }

        if (state != EntityState.Detached)
        {
            var single = context.ChangeTracker.Entries<SinglePkToPk>().Single().Entity;

            Assert.Same(single, parent.SinglePkToPk);
            Assert.Same(parent, single.Parent);
        }
    }

    [ConditionalFact]
    public virtual void Eager_load_one_to_one_non_virtual_reference_to_owned_type()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);

        var owner = context.Set<NonVirtualOneToOneOwner>().Single();
        var addressReferenceEntry = context.Entry(owner).References.First();

        Assert.Equal("Address", addressReferenceEntry.Metadata.Name);
        Assert.True(addressReferenceEntry.IsLoaded);
        Assert.Equal("Paradise Alley", owner.Address!.Street);
    }

    [ConditionalFact]
    public virtual void Eager_load_one_to_one_virtual_reference_to_owned_type()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);

        var owner = context.Set<VirtualOneToOneOwner>().Single();
        var addressReferenceEntry = context.Entry(owner).References.First();

        Assert.Equal("Address", addressReferenceEntry.Metadata.Name);
        Assert.True(addressReferenceEntry.IsLoaded);
        Assert.Equal("Dead End", owner.Address!.Street);
    }

    [ConditionalFact]
    public virtual void Eager_load_one_to_many_non_virtual_collection_of_owned_types()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);

        var owner = context.Set<NonVirtualOneToManyOwner>().Single();
        var addressesCollectionEntry = context.Entry(owner).Collections.First();

        Assert.Equal("Addresses", addressesCollectionEntry.Metadata.Name);
        Assert.True(addressesCollectionEntry.IsLoaded);
        Assert.Single(owner.Addresses!);
    }

    [ConditionalFact]
    public virtual void Eager_load_one_to_many_virtual_collection_of_owned_types()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);

        var owner = context.Set<VirtualOneToManyOwner>().Single();
        var addressesCollectionEntry = context.Entry(owner).Collections.First();

        Assert.Equal("Addresses", addressesCollectionEntry.Metadata.Name);
        Assert.True(addressesCollectionEntry.IsLoaded);
        Assert.Equal(3, owner.Addresses!.Count);
    }

    [ConditionalFact]
    public virtual void Eager_load_one_to_many_non_virtual_collection_of_owned_types_with_explicit_lazy_load()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);

        var owner = context.Set<ExplicitLazyLoadNonVirtualOneToManyOwner>().Single();
        var addressesCollectionEntry = context.Entry(owner).Collections.First();

        Assert.Equal("Addresses", addressesCollectionEntry.Metadata.Name);
        Assert.True(addressesCollectionEntry.IsLoaded);
        Assert.Single(owner.Addresses);
    }

    [ConditionalFact]
    public virtual void Eager_load_one_to_many_virtual_collection_of_owned_types_with_explicit_lazy_load()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);

        var owner = context.Set<ExplicitLazyLoadVirtualOneToManyOwner>().Single();
        var addressesCollectionEntry = context.Entry(owner).Collections.First();

        Assert.Equal("Addresses", addressesCollectionEntry.Metadata.Name);
        Assert.True(addressesCollectionEntry.IsLoaded);
        Assert.Single(owner.Addresses);
    }

    // Tests issue https://github.com/dotnet/efcore/issues/19847 (non-virtual)
    [ConditionalFact]
    public virtual void Setting_reference_to_owned_type_to_null_is_allowed_on_non_virtual_navigation()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);

        var owner = context.Set<NonVirtualOneToOneOwner>().Single();
        owner.Address = null;
        context.Attach(owner);
        context.Update(owner);

        Assert.Null(owner.Address);

        context.ChangeTracker.DetectChanges();

        Assert.Null(owner.Address);
    }

    // Tests issue https://github.com/dotnet/efcore/issues/19847 (virtual)
    [ConditionalFact]
    public virtual void Setting_reference_to_owned_type_to_null_is_allowed_on_virtual_navigation()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);

        var owner = context.Set<VirtualOneToOneOwner>().Single();
        owner.Address = null;
        context.Attach(owner);
        context.Update(owner);

        Assert.Null(owner.Address);

        context.ChangeTracker.DetectChanges();

        Assert.Null(owner.Address);
    }

    [ConditionalFact]
    public virtual void Non_virtual_one_to_one_reference_to_principal_is_not_lazy_loaded()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);

        var child = context.Set<NonVirtualChild>().Single(e => e.SingleParent != null);

        Assert.Null(child.SingleParent);
        context.Entry(child).Reference(e => e.SingleParent).Load();
        Assert.NotNull(child.SingleParent);

        child.SingleParent = null;
        Assert.Null(child.SingleParent);
        context.ChangeTracker.DetectChanges();
        Assert.Null(child.SingleParent);
    }

    [ConditionalFact]
    public virtual void Non_virtual_one_to_many_reference_to_principal_is_not_lazy_loaded()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);

        var child = context.Set<NonVirtualChild>().Single(e => e.CollectionParent != null);

        Assert.Null(child.CollectionParent);
        context.Entry(child).Reference(e => e.CollectionParent).Load();
        Assert.NotNull(child.CollectionParent);

        child.CollectionParent = null;
        Assert.Null(child.CollectionParent);
        context.ChangeTracker.DetectChanges();
        Assert.Null(child.CollectionParent);
    }

    [ConditionalFact]
    public virtual void Non_virtual_reference_to_dependent_is_not_lazy_loaded()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);

        var parent = context.Set<NonVirtualParent>().Single();

        Assert.Null(parent.Child);
        context.Entry(parent).Reference(e => e.Child).Load();
        Assert.NotNull(parent.Child);

        parent.Child = null;
        Assert.Null(parent.Child);
        context.ChangeTracker.DetectChanges();
        Assert.Null(parent.Child);
    }

    [ConditionalFact]
    public virtual void Non_virtual_collection_is_not_lazy_loaded()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);

        var parent = context.Set<NonVirtualParent>().Single();

        Assert.Null(parent.Children);
        context.Entry(parent).Collection(e => e.Children!).Load();
        Assert.Single(parent.Children!);

        parent.Children!.Clear();
        Assert.Empty(parent.Children);
        context.ChangeTracker.DetectChanges();
        Assert.Empty(parent.Children);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_null_FK(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.CreateProxy<Child>();
        child.Id = 767;

        context.Attach(child);

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        if (state != EntityState.Detached)
        {
            context.Entry(child).State = state;
        }

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.Null(child.Parent);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Single(context.ChangeTracker.Entries());
        Assert.Null(child.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_null_FK(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.CreateProxy<Single>();
        single.Id = 767;

        context.Attach(single);

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        if (state != EntityState.Detached)
        {
            context.Entry(single).State = state;
        }

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.Null(single.Parent);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Single(context.ChangeTracker.Entries());

        Assert.Null(single.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_changed_non_found_FK(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.CreateProxy<Child>();
        child.Id = 767;
        child.ParentId = 797;

        if (state != EntityState.Detached)
        {
            context.Attach(child);
        }

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        if (state != EntityState.Detached)
        {
            context.Entry(child).State = state;
        }

        referenceEntry.IsLoaded = true;

        changeDetector.DetectChangesCalled = false;

        child.ParentId = 707;

        if (state != EntityState.Detached)
        {
            context.ChangeTracker.DetectChanges();
        }
        else
        {
            referenceEntry.IsLoaded = false;
        }

        if (state == EntityState.Deleted)
        {
            Assert.Null(child.Parent);
        }
        else
        {
            Assert.NotNull(child.Parent);
        }

        if (state != EntityState.Detached)
        {
            Assert.True(changeDetector.DetectChangesCalled);
        }

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Deleted)
        {
            Assert.Contains(child, child.Parent!.Children!);
        }

        if (state != EntityState.Detached)
        {
            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            if (state == EntityState.Deleted)
            {
                Assert.Null(parent.Children);
                Assert.Null(child.Parent);
            }
            else
            {
                Assert.Same(child, parent.Children!.Single());
                Assert.Same(parent, child.Parent);
            }
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_changed_found_FK(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.CreateProxy<Parent>();
        parent.Id = 797;
        parent.AlternateId = "X";

        var child = context.CreateProxy<Child>();
        child.Id = 767;

        child.ParentId = 797;
        child.Parent = parent;
        parent.Children = new List<Child> { child };

        if (state != EntityState.Detached)
        {
            context.Attach(child);
            context.Attach(parent);
        }

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        if (state != EntityState.Detached)
        {
            context.Entry(child).State = state;
        }

        referenceEntry.IsLoaded = true;

        changeDetector.DetectChangesCalled = false;

        child.ParentId = 707;

        context.ChangeTracker.DetectChanges();

        if (state == EntityState.Deleted)
        {
            Assert.Null(child.Parent);
        }
        else
        {
            Assert.NotNull(child.Parent);
        }

        Assert.True(changeDetector.DetectChangesCalled);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 3, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Deleted)
        {
            Assert.Contains(child, child.Parent!.Children!);
        }

        if (state != EntityState.Detached)
        {
            var newParent = context.ChangeTracker.Entries<Parent>().Single(e => e.Entity.Id != parent.Id).Entity;

            if (state == EntityState.Deleted)
            {
                Assert.Empty(parent.Children);
                Assert.Null(child.Parent);
            }
            else
            {
                Assert.Same(child, newParent.Children!.Single());
                Assert.Same(newParent, child.Parent);
            }
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_collection_not_found(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.CreateProxy<Parent>();
        parent.Id = 767;
        parent.AlternateId = "NewRoot";

        context.Attach(parent);

        ClearLog();

        var collectionEntry = context.Entry(parent).Collection(e => e.Children!);

        if (state != EntityState.Detached)
        {
            context.Entry(parent).State = state;
        }

        Assert.False(collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.Empty(parent.Children!);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.True(collectionEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Empty(parent.Children!);
        Assert.Single(context.ChangeTracker.Entries());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_not_found(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.CreateProxy<Child>();
        child.Id = 767;
        child.ParentId = 787;

        context.Attach(child);

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        if (state != EntityState.Detached)
        {
            context.Entry(child).State = state;
        }

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.Null(child.Parent);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Single(context.ChangeTracker.Entries());
        Assert.Null(child.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_not_found(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.CreateProxy<Single>();
        single.Id = 767;
        single.ParentId = 787;

        context.Attach(single);

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        if (state != EntityState.Detached)
        {
            context.Entry(single).State = state;
        }

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.Null(single.Parent);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Single(context.ChangeTracker.Entries());

        Assert.Null(single.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_one_to_one_reference_to_dependent_not_found(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.CreateProxy<Parent>();
        parent.Id = 767;
        parent.AlternateId = "NewRoot";

        context.Attach(parent);

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        if (state != EntityState.Detached)
        {
            context.Entry(parent).State = state;
        }

        Assert.False(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.Null(parent.Single);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Single(context.ChangeTracker.Entries());

        Assert.Null(parent.Single);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Detached, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate)]
    [InlineData(EntityState.Detached, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Never)]
    [InlineData(EntityState.Added, CascadeTiming.Never)]
    [InlineData(EntityState.Modified, CascadeTiming.Never)]
    [InlineData(EntityState.Deleted, CascadeTiming.Never)]
    [InlineData(EntityState.Detached, CascadeTiming.Never)]
    public virtual void Lazy_load_collection_already_loaded(EntityState state, CascadeTiming cascadeDeleteTiming)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;

        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<Parent>().Include(e => e.Children).Single();

        ClearLog();

        var collectionEntry = context.Entry(parent).Collection(e => e.Children!);

        if (state != EntityState.Detached)
        {
            context.Entry(parent).State = state;
        }

        Assert.True(collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.NotNull(parent.Children);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.True(collectionEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(2, parent.Children.Count());

        if (state == EntityState.Deleted
            && cascadeDeleteTiming == CascadeTiming.Immediate)
        {
            Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Null(c));
        }
        else
        {
            Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));
        }

        Assert.Equal(state == EntityState.Detached ? 0 : 3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_collection_already_partially_loaded(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
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

        var collectionEntry = context.Entry(parent).Collection(e => e.Children!);

        if (state != EntityState.Unchanged)
        {
            context.Entry(child).State = state;
            context.Entry(parent).State = state;
        }

        Assert.False(collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.NotNull(parent.Children);

        Assert.False(changeDetector.DetectChangesCalled);
        Assert.Equal(state != EntityState.Detached, collectionEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 1 : 2, parent.Children.Count());
        Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));
        Assert.Equal(state == EntityState.Detached ? 0 : 3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public virtual void Lazy_load_collection_already_partially_loaded_no_tracking(QueryTrackingBehavior queryTrackingBehavior)
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

        var collectionEntry = context.Entry(parent).Collection(e => e.Children!);

        Assert.False(collectionEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.NotNull(parent.Children);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.True(collectionEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(queryTrackingBehavior == QueryTrackingBehavior.NoTracking ? 3 : 2, parent.Children.Count());

        Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));

        Assert.Empty(context.ChangeTracker.Entries());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Never)]
    [InlineData(EntityState.Added, CascadeTiming.Never)]
    [InlineData(EntityState.Modified, CascadeTiming.Never)]
    [InlineData(EntityState.Deleted, CascadeTiming.Never)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_already_loaded(
        EntityState state,
        CascadeTiming cascadeDeleteTiming)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;

        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var child = context.Set<Child>().Include(e => e.Parent).Single(e => e.Id == 12);

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        if (state != EntityState.Detached)
        {
            context.Entry(child).State = state;
        }

        Assert.True(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.NotNull(child.Parent);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Deleted)
        {
            Assert.Contains(child, child.Parent!.Children!);
        }

        if (state != EntityState.Detached)
        {
            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, child.Parent);
            Assert.Same(child, parent.Children!.Single());
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_already_loaded(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Set<Single>().Include(e => e.Parent).Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        if (state != EntityState.Detached)
        {
            context.Entry(single).State = state;
        }

        Assert.True(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.NotNull(single.Parent);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Deleted)
        {
            Assert.Same(single, single.Parent.Single);
        }

        if (state != EntityState.Detached)
        {
            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.Single);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Added, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Immediate)]
    [InlineData(EntityState.Added, CascadeTiming.Immediate)]
    [InlineData(EntityState.Modified, CascadeTiming.Immediate)]
    [InlineData(EntityState.Deleted, CascadeTiming.Immediate)]
    [InlineData(EntityState.Unchanged, CascadeTiming.Never)]
    [InlineData(EntityState.Added, CascadeTiming.Never)]
    [InlineData(EntityState.Modified, CascadeTiming.Never)]
    [InlineData(EntityState.Deleted, CascadeTiming.Never)]
    public virtual void Lazy_load_one_to_one_reference_to_dependent_already_loaded(
        EntityState state,
        CascadeTiming cascadeDeleteTiming)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;

        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<Parent>().Include(e => e.Single).Single();

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.Single);

        if (state != EntityState.Detached)
        {
            context.Entry(parent).State = state;
        }

        Assert.True(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

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
            var single = context.ChangeTracker.Entries<Single>().Single().Entity;

            Assert.Same(single, parent.Single);

            if (cascadeDeleteTiming == CascadeTiming.Immediate
                && state == EntityState.Deleted)
            {
                // No fixup to Deleted entity.
                Assert.Null(single.Parent);
            }
            else
            {
                Assert.Same(parent, single.Parent);
            }
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_one_to_one_PK_to_PK_reference_to_principal_already_loaded(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var single = context.Set<SinglePkToPk>().Include(e => e.Parent).Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        if (state != EntityState.Detached)
        {
            context.Entry(single).State = state;
        }

        Assert.True(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.NotNull(single.Parent);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Deleted)
        {
            Assert.Same(single, single.Parent.SinglePkToPk);
        }

        if (state != EntityState.Detached)
        {
            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.SinglePkToPk);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_one_to_one_PK_to_PK_reference_to_dependent_already_loaded(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var parent = context.Set<Parent>().Include(e => e.SinglePkToPk).Single();

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.SinglePkToPk);

        if (state != EntityState.Detached)
        {
            context.Entry(parent).State = state;
        }

        Assert.True(referenceEntry.IsLoaded);

        changeDetector.DetectChangesCalled = false;

        Assert.NotNull(parent.SinglePkToPk);

        Assert.False(changeDetector.DetectChangesCalled);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        if (state != EntityState.Deleted)
        {
            Assert.Same(parent, parent.SinglePkToPk.Parent);
        }

        if (state != EntityState.Detached)
        {
            Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

            var single = context.ChangeTracker.Entries<SinglePkToPk>().Single().Entity;

            Assert.Same(single, parent.SinglePkToPk);
            Assert.Same(parent, single.Parent);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_alternate_key(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var child = context.Set<ChildAk>().Single(e => e.Id == 32);

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        if (state != EntityState.Detached)
        {
            context.Entry(child).State = state;
        }

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

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Deleted)
        {
            Assert.Same(child, child.Parent!.ChildrenAk!.Single());
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
                Assert.Same(child, parent.ChildrenAk!.Single());
            }
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_alternate_key(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var single = context.Set<SingleAk>().Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        if (state != EntityState.Detached)
        {
            context.Entry(single).State = state;
        }

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

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_one_to_one_reference_to_dependent_alternate_key(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.SingleAk);

        if (state != EntityState.Detached)
        {
            context.Entry(parent).State = state;
        }

        Assert.False(referenceEntry.IsLoaded);

        Assert.NotNull(parent.SingleAk);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Deleted)
        {
            Assert.Same(parent, parent.SingleAk.Parent);
        }

        if (state != EntityState.Detached)
        {
            var single = context.ChangeTracker.Entries<SingleAk>().Single().Entity;

            Assert.Same(single, parent.SingleAk);
            Assert.Same(parent, single.Parent);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_null_FK_alternate_key(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var child = context.CreateProxy<ChildAk>();
        child.Id = 767;

        context.Attach(child);

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        if (state != EntityState.Detached)
        {
            context.Entry(child).State = state;
        }

        Assert.False(referenceEntry.IsLoaded);

        Assert.Null(child.Parent);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Single(context.ChangeTracker.Entries());
        Assert.Null(child.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_null_FK_alternate_key(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var single = context.CreateProxy<SingleAk>();
        single.Id = 767;

        context.Attach(single);

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        if (state != EntityState.Detached)
        {
            context.Entry(single).State = state;
        }

        Assert.False(referenceEntry.IsLoaded);

        Assert.Null(single.Parent);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Single(context.ChangeTracker.Entries());

        Assert.Null(single.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_collection_shadow_fk(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var collectionEntry = context.Entry(parent).Collection(e => e.ChildrenShadowFk!);

        if (state != EntityState.Detached)
        {
            context.Entry(parent).State = state;
        }

        Assert.False(collectionEntry.IsLoaded);

        Assert.NotNull(parent.ChildrenShadowFk);

        Assert.True(collectionEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(2, parent.ChildrenShadowFk.Count());
        Assert.All(parent.ChildrenShadowFk.Select(e => e.Parent), c => Assert.Same(parent, c));

        Assert.Equal(state == EntityState.Detached ? 0 : 3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_shadow_fk(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var child = context.Set<ChildShadowFk>().Single(e => e.Id == 52);

        ClearLog();

        if (state == EntityState.Detached)
        {
            Assert.Equal(
                CoreStrings.CannotLoadDetachedShadow("Parent", "ChildShadowFk"),
                Assert.Throws<InvalidOperationException>(() => child.Parent).Message);
        }
        else
        {
            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

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
                Assert.Same(child, child.Parent!.ChildrenShadowFk!.Single());
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
                Assert.Same(child, parent.ChildrenShadowFk!.Single());
            }
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_shadow_fk(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var single = context.Set<SingleShadowFk>().Single();

        ClearLog();

        if (state == EntityState.Detached)
        {
            Assert.Equal(
                CoreStrings.CannotLoadDetachedShadow("Parent", "SingleShadowFk"),
                Assert.Throws<InvalidOperationException>(() => single.Parent).Message);
        }
        else
        {
            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

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

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_one_to_one_reference_to_dependent_shadow_fk(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.SingleShadowFk);

        if (state != EntityState.Detached)
        {
            context.Entry(parent).State = state;
        }

        Assert.False(referenceEntry.IsLoaded);

        Assert.NotNull(parent.SingleShadowFk);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Deleted)
        {
            Assert.Same(parent, parent.SingleShadowFk!.Parent);
        }

        if (state != EntityState.Detached)
        {
            var single = context.ChangeTracker.Entries<SingleShadowFk>().Single().Entity;

            Assert.Same(single, parent.SingleShadowFk);
            Assert.Same(parent, single.Parent);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_null_FK_shadow_fk(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var child = context.CreateProxy<ChildShadowFk>();
        child.Id = 767;

        context.Attach(child);
        context.Entry(child).Property("ParentId").CurrentValue = null;

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        if (state != EntityState.Detached)
        {
            context.Entry(child).State = state;
        }

        Assert.False(referenceEntry.IsLoaded);

        Assert.Null(child.Parent);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Single(context.ChangeTracker.Entries());
        Assert.Null(child.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_null_FK_shadow_fk(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var single = context.CreateProxy<SingleShadowFk>();
        single.Id = 767;

        context.Attach(single);
        context.Entry(single).Property("ParentId").CurrentValue = null;

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        if (state != EntityState.Detached)
        {
            context.Entry(single).State = state;
        }

        Assert.False(referenceEntry.IsLoaded);

        Assert.Null(single.Parent);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Single(context.ChangeTracker.Entries());

        Assert.Null(single.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_collection_composite_key(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var collectionEntry = context.Entry(parent).Collection(e => e.ChildrenCompositeKey!);

        if (state != EntityState.Detached)
        {
            context.Entry(parent).State = state;
        }

        Assert.False(collectionEntry.IsLoaded);

        Assert.NotNull(parent.ChildrenCompositeKey);

        Assert.True(collectionEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(2, parent.ChildrenCompositeKey.Count());
        Assert.All(parent.ChildrenCompositeKey.Select(e => e.Parent), c => Assert.Same(parent, c));

        Assert.Equal(state == EntityState.Detached ? 0 : 3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_composite_key(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var child = context.Set<ChildCompositeKey>().Single(e => e.Id == 52);

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        if (state != EntityState.Detached)
        {
            context.Entry(child).State = state;
        }

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

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Deleted)
        {
            Assert.Same(child, child.Parent!.ChildrenCompositeKey!.Single());
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
                Assert.Same(child, parent.ChildrenCompositeKey!.Single());
            }
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_composite_key(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var single = context.Set<SingleCompositeKey>().Single();

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        if (state != EntityState.Detached)
        {
            context.Entry(single).State = state;
        }

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

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_one_to_one_reference_to_dependent_composite_key(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var referenceEntry = context.Entry(parent).Reference(e => e.SingleCompositeKey);

        if (state != EntityState.Detached)
        {
            context.Entry(parent).State = state;
        }

        Assert.False(referenceEntry.IsLoaded);

        Assert.NotNull(parent.SingleCompositeKey);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Equal(state == EntityState.Detached ? 0 : 2, context.ChangeTracker.Entries().Count());

        if (state != EntityState.Deleted)
        {
            Assert.Same(parent, parent.SingleCompositeKey.Parent);
        }

        if (state != EntityState.Detached)
        {
            var single = context.ChangeTracker.Entries<SingleCompositeKey>().Single().Entity;

            Assert.Same(single, parent.SingleCompositeKey);
            Assert.Same(parent, single.Parent);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_many_to_one_reference_to_principal_null_FK_composite_key(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var child = context.CreateProxy<ChildCompositeKey>();
        child.Id = 767;
        child.ParentId = 567;

        context.Attach(child);

        ClearLog();

        var referenceEntry = context.Entry(child).Reference(e => e.Parent);

        if (state != EntityState.Detached)
        {
            context.Entry(child).State = state;
        }

        Assert.False(referenceEntry.IsLoaded);

        Assert.Null(child.Parent);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Single(context.ChangeTracker.Entries());
        Assert.Null(child.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Detached)]
    public virtual void Lazy_load_one_to_one_reference_to_principal_null_FK_composite_key(EntityState state)
    {
        using var context = CreateContext(lazyLoadingEnabled: true, tracking: state != EntityState.Detached);
        var single = context.CreateProxy<SingleCompositeKey>();
        single.Id = 767;
        single.ParentAlternateId = "Boot";

        context.Attach(single);

        ClearLog();

        var referenceEntry = context.Entry(single).Reference(e => e.Parent);

        if (state != EntityState.Detached)
        {
            context.Entry(single).State = state;
        }

        Assert.False(referenceEntry.IsLoaded);

        Assert.Null(single.Parent);

        Assert.True(referenceEntry.IsLoaded);

        RecordLog();
        context.ChangeTracker.LazyLoadingEnabled = false;

        Assert.Single(context.ChangeTracker.Entries());

        Assert.Null(single.Parent);
    }

    [ConditionalFact]
    public virtual void Lazy_load_collection_for_detached_is_no_op()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        var parent = context.Set<Parent>().Single();

        context.Entry(parent).State = EntityState.Detached;

        Assert.Null(parent.Children);
    }

    [ConditionalFact]
    public virtual void Lazy_load_reference_to_principal_for_detached_is_no_op()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        var child = context.Set<Child>().Single(e => e.Id == 12);

        context.Entry(child).State = EntityState.Detached;

        Assert.Null(child.Parent);
    }

    [ConditionalFact]
    public virtual void Lazy_load_reference_to_dependent_for_detached_is_no_op()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        var parent = context.Set<Parent>().Single();

        context.Entry(parent).State = EntityState.Detached;

        Assert.Null(parent.Single);
    }

    [ConditionalFact]
    public virtual void Lazy_load_collection_for_no_tracking_does_not_throw_if_populated()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        var parent = context.Set<Parent>().Include(e => e.Children).AsNoTracking().Single();

        Assert.Same(parent, parent.Children!.First().Parent);

        ((ICollection<Child>)parent.Children!).Clear();

        Assert.Empty(parent.Children);
    }

    [ConditionalFact]
    public virtual void Lazy_load_reference_to_principal_for_no_tracking_does_not_throw_if_populated()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        var child = context.Set<Child>().Include(e => e.Parent).AsNoTracking().Single(e => e.Id == 12);

        Assert.NotNull(child.Parent);

        child.Parent = null;

        Assert.Null(child.Parent);
    }

    [ConditionalFact]
    public virtual void Lazy_load_reference_to_dependent_for_no_tracking_does_not_throw_if_populated()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        var parent = context.Set<Parent>().Include(e => e.Single).AsNoTracking().Single();

        Assert.Same(parent, parent.Single!.Parent);

        parent.Single = null;

        Assert.Null(parent.Single);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Detached, true)]
    [InlineData(EntityState.Detached, false)]
    public virtual async Task Load_collection(EntityState state, bool async)
    {
        using var context = CreateContext(tracking: state != EntityState.Detached);
        var parent = context.Set<Parent>().Single();

        ClearLog();

        var collectionEntry = context.Entry(parent).Collection(e => e.Children!);

        if (state != EntityState.Detached)
        {
            context.Entry(parent).State = state;
        }

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

        Assert.Equal(2, parent.Children!.Count());
        Assert.All(parent.Children!.Select(e => e.Parent), c => Assert.Same(parent, c));

        Assert.Equal(state == EntityState.Detached ? 0 : 3, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public virtual void Can_serialize_proxies_to_JSON()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);

        var blogs = context.Set<Blog>().OrderBy(e => e.Host!.HostName).ToList();

        VerifyBlogs(blogs);
        foreach (var blog in blogs)
        {
            Assert.IsNotType<Blog>(blog);
        }

        var serialized = JsonConvert.SerializeObject(
            blogs,
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Formatting = Formatting.Indented });

        Assert.Equal(SerializedBlogs1, serialized, ignoreLineEndingDifferences: true);

        var newBlogs = JsonConvert.DeserializeObject<List<Blog>>(serialized)!;

        VerifyBlogs(newBlogs);
        foreach (var blog in newBlogs)
        {
            Assert.IsType<Blog>(blog);
        }

        var options = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve, WriteIndented = true };

        serialized = JsonSerializer.Serialize(blogs, options);

        Assert.Equal(SerializedBlogs2, serialized, ignoreLineEndingDifferences: true);

        newBlogs = JsonSerializer.Deserialize<List<Blog>>(serialized, options);
        Assert.IsType<List<Blog>>(newBlogs);

        foreach (var blog in newBlogs)
        {
            Assert.IsType<Blog>(blog);
        }

        VerifyBlogs(newBlogs);
    }

    protected virtual string SerializedBlogs2
        => """
{
  "$id": "1",
  "$values": [
    {
      "$id": "2",
      "Id": 1,
      "Writer": {
        "$id": "3",
        "FirstName": "firstNameWriter0",
        "LastName": "lastNameWriter0",
        "Alive": false,
        "Culture": {
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "4",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "5",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.00,
            "Tag": {
              "$id": "6",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        },
        "Milk": {
          "$id": "7",
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "8",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "9",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.00,
            "Tag": {
              "$id": "10",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        }
      },
      "Reader": {
        "$id": "11",
        "FirstName": "firstNameReader0",
        "LastName": "lastNameReader0",
        "Alive": false,
        "Culture": {
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "12",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "13",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.00,
            "Tag": {
              "$id": "14",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        },
        "Milk": {
          "$id": "15",
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "16",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "17",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.00,
            "Tag": {
              "$id": "18",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        }
      },
      "Host": {
        "$id": "19",
        "HostName": "127.0.0.1",
        "Rating": 0,
        "Culture": {
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "20",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "21",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.00,
            "Tag": {
              "$id": "22",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        },
        "Milk": {
          "$id": "23",
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "24",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "25",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.00,
            "Tag": {
              "$id": "26",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        }
      },
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "$id": "27",
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "$id": "28",
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "$id": "29",
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "$id": "30",
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "$id": "31",
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "$id": "32",
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "$id": "33",
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      }
    },
    {
      "$id": "34",
      "Id": 2,
      "Writer": {
        "$id": "35",
        "FirstName": "firstNameWriter1",
        "LastName": "lastNameWriter1",
        "Alive": false,
        "Culture": {
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "36",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "37",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.00,
            "Tag": {
              "$id": "38",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        },
        "Milk": {
          "$id": "39",
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "40",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "41",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.00,
            "Tag": {
              "$id": "42",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        }
      },
      "Reader": {
        "$id": "43",
        "FirstName": "firstNameReader1",
        "LastName": "lastNameReader1",
        "Alive": false,
        "Culture": {
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "44",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "45",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.00,
            "Tag": {
              "$id": "46",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        },
        "Milk": {
          "$id": "47",
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "48",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "49",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.00,
            "Tag": {
              "$id": "50",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        }
      },
      "Host": {
        "$id": "51",
        "HostName": "127.0.0.2",
        "Rating": 0,
        "Culture": {
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "52",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "53",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.00,
            "Tag": {
              "$id": "54",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        },
        "Milk": {
          "$id": "55",
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "56",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "57",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.00,
            "Tag": {
              "$id": "58",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        }
      },
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "$id": "59",
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "$id": "60",
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "$id": "61",
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "$id": "62",
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "$id": "63",
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "$id": "64",
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "$id": "65",
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      }
    },
    {
      "$id": "66",
      "Id": 3,
      "Writer": {
        "$id": "67",
        "FirstName": "firstNameWriter2",
        "LastName": "lastNameWriter2",
        "Alive": false,
        "Culture": {
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "68",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "69",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.00,
            "Tag": {
              "$id": "70",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        },
        "Milk": {
          "$id": "71",
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "72",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "73",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.00,
            "Tag": {
              "$id": "74",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        }
      },
      "Reader": {
        "$id": "75",
        "FirstName": "firstNameReader2",
        "LastName": "lastNameReader2",
        "Alive": false,
        "Culture": {
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "76",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "77",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.00,
            "Tag": {
              "$id": "78",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        },
        "Milk": {
          "$id": "79",
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "80",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "81",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.00,
            "Tag": {
              "$id": "82",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        }
      },
      "Host": {
        "$id": "83",
        "HostName": "127.0.0.3",
        "Rating": 0,
        "Culture": {
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "84",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "85",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.00,
            "Tag": {
              "$id": "86",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        },
        "Milk": {
          "$id": "87",
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "88",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "89",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.00,
            "Tag": {
              "$id": "90",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        }
      },
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "$id": "91",
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "$id": "92",
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "$id": "93",
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "$id": "94",
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "$id": "95",
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "$id": "96",
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "$id": "97",
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      }
    }
  ]
}
""";

    protected virtual string SerializedBlogs1
        => """
[
  {
    "Writer": {
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "FirstName": "firstNameWriter0",
      "LastName": "lastNameWriter0",
      "Alive": false
    },
    "Reader": {
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "FirstName": "firstNameReader0",
      "LastName": "lastNameReader0",
      "Alive": false
    },
    "Host": {
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "HostName": "127.0.0.1",
      "Rating": 0.0
    },
    "Culture": {
      "Species": "S1",
      "Subspecies": null,
      "Rating": 8,
      "Validation": false,
      "Manufacturer": {
        "Name": "M1",
        "Rating": 7,
        "Tag": {
          "Text": "Ta2"
        },
        "Tog": {
          "Text": "To2"
        }
      },
      "License": {
        "Title": "Ti1",
        "Charge": 1.00,
        "Tag": {
          "Text": "Ta1"
        },
        "Tog": {
          "Text": "To1"
        }
      }
    },
    "Milk": {
      "Species": "S1",
      "Subspecies": null,
      "Rating": 8,
      "Validation": false,
      "Manufacturer": {
        "Name": "M1",
        "Rating": 7,
        "Tag": {
          "Text": "Ta2"
        },
        "Tog": {
          "Text": "To2"
        }
      },
      "License": {
        "Title": "Ti1",
        "Charge": 1.00,
        "Tag": {
          "Text": "Ta1"
        },
        "Tog": {
          "Text": "To1"
        }
      }
    },
    "Id": 1
  },
  {
    "Writer": {
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "FirstName": "firstNameWriter1",
      "LastName": "lastNameWriter1",
      "Alive": false
    },
    "Reader": {
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "FirstName": "firstNameReader1",
      "LastName": "lastNameReader1",
      "Alive": false
    },
    "Host": {
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "HostName": "127.0.0.2",
      "Rating": 0.0
    },
    "Culture": {
      "Species": "S1",
      "Subspecies": null,
      "Rating": 8,
      "Validation": false,
      "Manufacturer": {
        "Name": "M1",
        "Rating": 7,
        "Tag": {
          "Text": "Ta2"
        },
        "Tog": {
          "Text": "To2"
        }
      },
      "License": {
        "Title": "Ti1",
        "Charge": 1.00,
        "Tag": {
          "Text": "Ta1"
        },
        "Tog": {
          "Text": "To1"
        }
      }
    },
    "Milk": {
      "Species": "S1",
      "Subspecies": null,
      "Rating": 8,
      "Validation": false,
      "Manufacturer": {
        "Name": "M1",
        "Rating": 7,
        "Tag": {
          "Text": "Ta2"
        },
        "Tog": {
          "Text": "To2"
        }
      },
      "License": {
        "Title": "Ti1",
        "Charge": 1.00,
        "Tag": {
          "Text": "Ta1"
        },
        "Tog": {
          "Text": "To1"
        }
      }
    },
    "Id": 2
  },
  {
    "Writer": {
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "FirstName": "firstNameWriter2",
      "LastName": "lastNameWriter2",
      "Alive": false
    },
    "Reader": {
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "FirstName": "firstNameReader2",
      "LastName": "lastNameReader2",
      "Alive": false
    },
    "Host": {
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.00,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "HostName": "127.0.0.3",
      "Rating": 0.0
    },
    "Culture": {
      "Species": "S1",
      "Subspecies": null,
      "Rating": 8,
      "Validation": false,
      "Manufacturer": {
        "Name": "M1",
        "Rating": 7,
        "Tag": {
          "Text": "Ta2"
        },
        "Tog": {
          "Text": "To2"
        }
      },
      "License": {
        "Title": "Ti1",
        "Charge": 1.00,
        "Tag": {
          "Text": "Ta1"
        },
        "Tog": {
          "Text": "To1"
        }
      }
    },
    "Milk": {
      "Species": "S1",
      "Subspecies": null,
      "Rating": 8,
      "Validation": false,
      "Manufacturer": {
        "Name": "M1",
        "Rating": 7,
        "Tag": {
          "Text": "Ta2"
        },
        "Tog": {
          "Text": "To2"
        }
      },
      "License": {
        "Title": "Ti1",
        "Charge": 1.00,
        "Tag": {
          "Text": "Ta1"
        },
        "Tog": {
          "Text": "To1"
        }
      }
    },
    "Id": 3
  }
]
""";

    [ConditionalFact]
    public virtual void Lazy_loading_finds_correct_entity_type_with_already_loaded_owned_types()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        var blogs = context.Set<Blog>().OrderBy(e => e.Host!.HostName).ToList();

        VerifyBlogs(blogs);
    }

    private static void VerifyBlogs(List<Blog> blogs)
    {
        Assert.Equal(3, blogs.Count);

        for (var i = 0; i < 3; i++)
        {
            Assert.Equal($"firstNameReader{i}", blogs[i].Reader!.FirstName);
            Assert.Equal($"lastNameReader{i}", blogs[i].Reader!.LastName);

            Assert.Equal($"firstNameWriter{i}", blogs[i].Writer!.FirstName);
            Assert.Equal($"lastNameWriter{i}", blogs[i].Writer!.LastName);

            Assert.Equal($"127.0.0.{i + 1}", blogs[i].Host!.HostName);
        }
    }

    [ConditionalFact]
    public virtual void Lazy_loading_finds_correct_entity_type_with_multiple_queries()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        var blogs = context.Set<Blog>().Where(_ => true);

        VerifyBlogs(blogs.ToList().OrderBy(e => e.Host!.HostName).ToList());
        Assert.Equal(12, context.ChangeTracker.Entries().Count());

        VerifyBlogs(blogs.ToList().OrderBy(e => e.Host!.HostName).ToList());
        Assert.Equal(12, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public virtual void Lazy_loading_finds_correct_entity_type_with_opaque_predicate_and_multiple_queries()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);

        // ReSharper disable once ConvertToLocalFunction
        bool opaquePredicate(Blog _)
            => true;

        var blogs = context.Set<Blog>().Where(opaquePredicate);

        VerifyBlogs(blogs.ToList().OrderBy(e => e.Host!.HostName).ToList());
        Assert.Equal(12, context.ChangeTracker.Entries().Count());

        VerifyBlogs(blogs.ToList().OrderBy(e => e.Host!.HostName).ToList());
        Assert.Equal(12, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public virtual void Lazy_loading_finds_correct_entity_type_with_multiple_queries_using_Count()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        var blogs = context.Set<Blog>().Where(_ => true);

        Assert.Equal(3, blogs.Count());
        Assert.Empty(context.ChangeTracker.Entries());

        Assert.Equal(3, blogs.Count());
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [ConditionalFact]
    public virtual void Lazy_loading_shares_service__property_on_derived_types()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        var parson = context.Set<Parson>().Single();

        Assert.Equal(2, parson.ParsonNoses!.Count);
        Assert.Equal(
            new[] { "Large", "Medium" },
            parson.ParsonNoses.Select(b => b.Size).OrderBy(h => h));

        var company = context.Set<Company>().Single();

        Assert.Equal(2, company.CompanyNoses!.Count);
        Assert.Equal(
            new[] { "Large", "Small" },
            company.CompanyNoses.Select(b => b.Size).OrderBy(h => h));

        var entity = context.Set<Entity>().ToList().Except(new Entity[] { parson, company }).Single();

        Assert.Equal(3, entity.BaseNoses!.Count);
        Assert.Equal(
            new[] { "Large", "Medium", "Small" },
            entity.BaseNoses.Select(b => b.Size).OrderBy(h => h));
    }

    [ConditionalFact]
    public virtual void Lazy_loading_handles_shadow_nullable_GUID_FK_in_TPH_model()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);

        var tribes = context.Set<Tribe>().ToList();

        Assert.Single(tribes);
        Assert.IsAssignableFrom<Quest>(tribes[0]);
        Assert.Equal(new DateTime(1973, 9, 3), ((Quest)tribes[0]).Birthday);
    }

    [ConditionalFact]
    public virtual void Lazy_loading_finds_correct_entity_type_with_alternate_model()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        var person = context.Set<Pyrson>().Single();

        Assert.NotNull(person.Name!.FirstName);
        Assert.NotNull(person.Name.LastName);
        Assert.NotNull(person.Address);

        var applicant = context.Set<Applicant>().Single();

        Assert.NotNull(applicant.Name);

        var address = context.Set<Address>().Single();

        Assert.NotNull(address.Line1);
        Assert.NotNull(address.Line2);

        Assert.Same(address, person.Address);
    }

    [ConditionalFact]
    public virtual void Top_level_projection_track_entities_before_passing_to_client_method()
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        var query = (from p in context.Set<Parent>()
                     orderby p.Id
                     select DtoFactory.CreateDto(p)).FirstOrDefault();

        RecordLog();

        Assert.NotNull(((dynamic)query!).Single);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Entity_equality_with_proxy_parameter(bool async)
    {
        using var context = CreateContext(lazyLoadingEnabled: true);
        var called = context.Set<Parent>().OrderBy(e => e.Id).FirstOrDefault();
        ClearLog();

        var query = from Child q in context.Set<Child>()
                    where q.Parent == called
                    select q;

        var result = async ? await query.ToListAsync() : query.ToList();

        RecordLog();
    }

    private static class DtoFactory
    {
        public static object CreateDto(Parent parent)
            => new
            {
                parent.Id,
                parent.Single,
                parent.Single!.ParentId
            };
    }

    public struct Culture
    {
        public string Species { get; set; }
        public string? Subspecies { get; set; }
        public int Rating { get; set; }
        public bool? Validation { get; set; }
        public Manufacturer Manufacturer { get; set; }
        public License License { get; set; }
    }

    public class Milk
    {
        public string Species { get; set; } = null!;
        public string? Subspecies { get; set; }
        public int Rating { get; set; }
        public bool? Validation { get; set; }
        public Manufacturer Manufacturer { get; set; } = null!;
        public License License { get; set; }
    }

    public class Manufacturer
    {
        public string? Name { get; set; }
        public int Rating { get; set; }
        public Tag Tag { get; set; } = null!;
        public Tog Tog { get; set; }
    }

    public struct License
    {
        public string Title { get; set; }
        public decimal Charge { get; set; }
        public Tag Tag { get; set; }
        public Tog Tog { get; set; }
    }

    public class Tag
    {
        public string? Text { get; set; }
    }

    public struct Tog
    {
        public string? Text { get; set; }
    }

    public class Address
    {
        public int AddressId { get; set; }
        public string? Line1 { get; set; }
        public string? Line2 { get; set; }
        public int PyrsonId { get; set; }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class Applicant
    {
        public int ApplicantId { get; set; }
        public virtual FullName? Name { get; set; }

        protected Applicant()
        {
        }

        public Applicant(FullName name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }

    public class FirstName
    {
        private readonly string? _value;

        protected FirstName()
        {
        }

        private FirstName(string value)
        {
            _value = value;
        }

        public static FirstName Create(string firstName)
            => new(firstName);
    }

    public class LastName
    {
        private readonly string? _value;

        protected LastName()
        {
        }

        private LastName(string value)
        {
            _value = value;
        }

        public static LastName Create(string lastName)
            => new(lastName);
    }

    public class Pyrson
    {
        public int PyrsonId { get; set; }
        public virtual FullName? Name { get; set; }
        public virtual Address? Address { get; set; }

        protected Pyrson()
        {
        }

        public Pyrson(FullName name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class FullName
    {
        public virtual bool Exists { get; set; }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public virtual FirstName? FirstName { get; private set; }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public virtual LastName? LastName { get; private set; }

        protected FullName()
        {
        }

        public FullName(FirstName firstName, LastName lastName)
        {
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            Exists = true;
        }
    }

    public abstract class Parent
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string? AlternateId { get; set; }

        public virtual IEnumerable<Child>? Children { get; set; }
        public virtual SinglePkToPk? SinglePkToPk { get; set; }
        public virtual Single? Single { get; set; }
        public virtual IEnumerable<ChildAk>? ChildrenAk { get; set; }
        public virtual SingleAk? SingleAk { get; set; }
        public virtual IEnumerable<ChildShadowFk>? ChildrenShadowFk { get; set; }
        public virtual SingleShadowFk? SingleShadowFk { get; set; }
        public virtual IEnumerable<ChildCompositeKey>? ChildrenCompositeKey { get; set; }
        public virtual SingleCompositeKey? SingleCompositeKey { get; set; }
        public virtual WithRecursiveProperty? WithRecursiveProperty { get; set; }
        public virtual IEnumerable<Child>? ManyChildren { get; set; }
    }

    public class Mother : Parent
    {
        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class Father : Parent
    {
        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class WithRecursiveProperty
    {
        private int _backing;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int? ParentId { get; set; }
        public virtual Parent? Parent { get; set; }

        public int IdLoadedFromParent
        {
            get
            {
                if (Parent != null)
                {
                    _backing = Parent.Id;
                }

                return _backing;
            }
            set => _backing = value;
        }

        public override bool Equals(object? obj)
            => throw new InvalidOperationException();

        public override int GetHashCode()
            => throw new InvalidOperationException();
    }

    public class Child
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int? ParentId { get; set; }

        public virtual Parent? Parent { get; set; }

        public virtual IEnumerable<Parent>? ManyParents { get; set; }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class SinglePkToPk
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public virtual Parent? Parent { get; set; }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class Single
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int? ParentId { get; set; }

        public virtual Parent? Parent { get; set; }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class ChildAk
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string? ParentId { get; set; }

        public virtual Parent? Parent { get; set; }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class SingleAk
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string? ParentId { get; set; }

        public virtual Parent? Parent { get; set; }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class ChildShadowFk
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public virtual Parent? Parent { get; set; }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class SingleShadowFk
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public virtual Parent? Parent { get; set; }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class ChildCompositeKey
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int? ParentId { get; set; }
        public string? ParentAlternateId { get; set; }

        public virtual Parent? Parent { get; set; }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class SingleCompositeKey
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int? ParentId { get; set; }
        public string? ParentAlternateId { get; set; }

        public virtual Parent? Parent { get; set; }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class Blog
    {
        public int Id { get; set; }
        public virtual Person? Writer { get; set; }
        public virtual Person? Reader { get; set; }
        public virtual Host? Host { get; set; }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class Nose
    {
        public int Id { get; set; }
        public string? Size { get; set; }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class Person
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool Alive { get; set; }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class Entity
    {
        public int Id { get; set; }

        public virtual ICollection<Nose>? BaseNoses { get; set; }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class Company : Entity
    {
        public virtual ICollection<Nose>? CompanyNoses { get; set; }
    }

    public class Parson : Entity
    {
        public DateTime Birthday { set; get; }

        public virtual ICollection<Nose>? ParsonNoses { get; set; }
    }

    public class Host
    {
        public string? HostName { get; set; }
        public double Rating { get; set; }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public abstract class Tribe
    {
        public Guid Id { get; set; }
    }

    public class Called : Tribe
    {
        public string? Name { set; get; }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class Quest : Tribe
    {
        public DateTime Birthday { set; get; }
        public virtual Called? Called { set; get; }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class NonVirtualParent
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public NonVirtualChild? Child { get; set; }
        public List<NonVirtualChild>? Children { get; set; }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class NonVirtualChild
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public NonVirtualParent? SingleParent { get; set; }
        public NonVirtualParent? CollectionParent { get; set; }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class NonVirtualOneToOneOwner
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        // note: _not_ virtual
        public OwnedAddress? Address { get; set; }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class VirtualOneToOneOwner
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public virtual OwnedAddress? Address { get; set; }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class NonVirtualOneToManyOwner
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        // note: _not_ virtual
        public List<OwnedAddress>? Addresses { get; set; }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class VirtualOneToManyOwner
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public virtual List<OwnedAddress>? Addresses { get; set; }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class ExplicitLazyLoadNonVirtualOneToManyOwner
    {
        private ICollection<OwnedAddress>? _addresses;
        private ILazyLoader? LazyLoader { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        // note: _not_ virtual
        public ICollection<OwnedAddress> Addresses
        {
            get => LazyLoader.Load(this, ref _addresses)!;
            set => _addresses = value;
        }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public class ExplicitLazyLoadVirtualOneToManyOwner
    {
        private ICollection<OwnedAddress>? _addresses;
        private ILazyLoader? LazyLoader { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public virtual ICollection<OwnedAddress> Addresses
        {
            get => LazyLoader.Load(this, ref _addresses)!;
            set => _addresses = value;
        }

        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    [Owned]
    public class OwnedAddress
    {
        public string? Street { get; set; }
        public string? PostalCode { get; set; }
        public int CountryCode { get; set; }
    }

    private static void AssertDisposed(Func<object?> testCode, string entityTypeName, string navigationName)
        => Assert.Equal(
            CoreStrings.WarningAsErrorTemplate(
                CoreEventId.LazyLoadOnDisposedContextWarning.ToString(),
                CoreResources.LogLazyLoadOnDisposedContext(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(entityTypeName, navigationName),
                "CoreEventId.LazyLoadOnDisposedContextWarning"),
            Assert.Throws<InvalidOperationException>(testCode).Message);

    protected DbContext CreateContext(bool lazyLoadingEnabled = false, bool tracking = true)
        => CreateContext(lazyLoadingEnabled, tracking ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTracking);

    protected DbContext CreateContext(bool lazyLoadingEnabled, QueryTrackingBehavior trackingBehavior)
    {
        var context = Fixture.CreateContext();
        context.ChangeTracker.LazyLoadingEnabled = lazyLoadingEnabled;
        context.ChangeTracker.QueryTrackingBehavior = trackingBehavior;

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

    public abstract class LoadFixtureBase : SharedStoreFixtureBase<DbContext>
    {
        protected override string StoreName
            => "LazyLoadProxyTest";

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder.UseLazyLoadingProxies(b => b.IgnoreNonVirtualNavigations()));

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(
                serviceCollection
                    .AddScoped<IChangeDetector, ChangeDetectorProxy>()
                    .AddEntityFrameworkProxies());

        // By-design. Lazy loaders are not disposed when using pooling
        protected override bool UsePooling
            => false;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<Tribe>();
            modelBuilder.Entity<Called>();
            modelBuilder.Entity<Quest>();

            modelBuilder.Entity<Entity>();
            modelBuilder.Entity<Company>();
            modelBuilder.Entity<Parson>();

            modelBuilder.Entity<SingleShadowFk>()
                .Property<int?>("ParentId");

            modelBuilder.Entity<Parent>(
                b =>
                {
                    b.Property(e => e.AlternateId).ValueGeneratedOnAdd();

                    b.HasMany(e => e.Children)
                        .WithOne(e => e.Parent)
                        .HasForeignKey(e => e.ParentId);

                    b.HasOne(e => e.SinglePkToPk)
                        .WithOne(e => e.Parent)
                        .HasForeignKey<SinglePkToPk>(e => e.Id)
                        .IsRequired();

                    b.HasOne(e => e.Single)
                        .WithOne(e => e.Parent)
                        .HasForeignKey<Single>(e => e.ParentId);

                    b.HasMany(e => e.ChildrenAk)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(e => e.AlternateId)
                        .HasForeignKey(e => e.ParentId);

                    b.HasOne(e => e.SingleAk)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey<Parent>(e => e.AlternateId)
                        .HasForeignKey<SingleAk>(e => e.ParentId);

                    b.HasMany(e => e.ChildrenShadowFk)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(e => e.Id)
                        .HasForeignKey("ParentId");

                    b.HasOne(e => e.SingleShadowFk)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey<Parent>(e => e.Id)
                        .HasForeignKey<SingleShadowFk>("ParentId");

                    b.HasMany(e => e.ChildrenCompositeKey)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(
                            e => new { e.AlternateId, e.Id })
                        .HasForeignKey(
                            e => new { e.ParentAlternateId, e.ParentId });

                    b.HasOne(e => e.SingleCompositeKey)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey<Parent>(
                            e => new { e.AlternateId, e.Id })
                        .HasForeignKey<SingleCompositeKey>(
                            e => new { e.ParentAlternateId, e.ParentId });

                    b.HasMany(e => e.ManyChildren).WithMany(e => e.ManyParents);
                });

            modelBuilder.Entity<Mother>();
            modelBuilder.Entity<Father>();

            modelBuilder.Entity<Blog>(
                e =>
                {
                    e.OwnsOne(x => x.Writer);
                    e.OwnsOne(x => x.Reader);
                    e.OwnsOne(x => x.Host);
                });

            modelBuilder.Entity<Address>(
                builder =>
                {
                    builder.HasKey(prop => prop.AddressId);

                    builder.Property(prop => prop.Line1)
                        .IsRequired()
                        .HasMaxLength(50);

                    builder.Property(prop => prop.Line2)
                        .IsRequired(false)
                        .HasMaxLength(50);
                });

            modelBuilder.Entity<Applicant>(
                builder =>
                {
                    builder.HasKey(prop => prop.ApplicantId);

                    builder.OwnsOne(
                        prop => prop.Name, name =>
                        {
                            name
                                .OwnsOne(prop => prop.FirstName)
                                .Property("_value")
                                .HasMaxLength(50)
                                .IsRequired();

                            name
                                .OwnsOne(prop => prop.LastName)
                                .Property("_value")
                                .HasMaxLength(50)
                                .IsRequired();
                        });
                });

            modelBuilder.Entity<Pyrson>(
                builder =>
                {
                    builder.HasKey(prop => prop.PyrsonId);

                    builder.OwnsOne(
                        prop => prop.Name, name =>
                        {
                            name
                                .OwnsOne(prop => prop.FirstName)
                                .Property("_value")
                                .HasMaxLength(50)
                                .IsRequired();

                            name
                                .OwnsOne(prop => prop.LastName)
                                .Property("_value")
                                .HasMaxLength(50)
                                .IsRequired();
                        });

                    builder.HasOne(prop => prop.Address)
                        .WithOne()
                        .HasForeignKey<Address>(prop => prop.PyrsonId);
                });

            modelBuilder.Entity<NonVirtualOneToOneOwner>();
            modelBuilder.Entity<VirtualOneToOneOwner>();

            // Note: Sqlite does not support auto-increment on composite keys
            // so have to redefine the key for this to work in Sqlite
            modelBuilder.Entity<NonVirtualOneToManyOwner>()
                .OwnsMany(o => o.Addresses, a => a.HasKey("Id"));
            modelBuilder.Entity<VirtualOneToManyOwner>()
                .OwnsMany(o => o.Addresses, a => a.HasKey("Id"));
            modelBuilder.Entity<ExplicitLazyLoadNonVirtualOneToManyOwner>()
                .OwnsMany(o => o.Addresses, a => a.HasKey("Id"));
            modelBuilder.Entity<ExplicitLazyLoadVirtualOneToManyOwner>()
                .OwnsMany(o => o.Addresses, a => a.HasKey("Id"));

            modelBuilder.Entity<NonVirtualParent>(
                b =>
                {
                    b.HasOne(e => e.Child).WithOne(e => e.SingleParent).HasPrincipalKey<NonVirtualChild>();
                    b.HasMany(e => e.Children).WithOne(e => e.CollectionParent);
                });
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.ComplexProperties<Culture>();
            configurationBuilder.ComplexProperties<Milk>();
            configurationBuilder.ComplexProperties<Manufacturer>();
            configurationBuilder.ComplexProperties<License>();
            configurationBuilder.ComplexProperties<Tag>();
            configurationBuilder.ComplexProperties<Tog>();
        }

        protected override Task SeedAsync(DbContext context)
        {
            context.Add(
                new Quest
                {
                    Birthday = new DateTime(1973, 9, 3),
                    Culture = CreateCulture(),
                    Milk = CreateMilk()
                });

            context.Add(
                new Mother
                {
                    Id = 707,
                    AlternateId = "Root",
                    Children = new List<Child>
                    {
                        new()
                        {
                            Id = 11,
                            Culture = CreateCulture(),
                            Milk = CreateMilk()
                        },
                        new()
                        {
                            Id = 12,
                            Culture = CreateCulture(),
                            Milk = CreateMilk()
                        }
                    },
                    SinglePkToPk = new SinglePkToPk
                    {
                        Id = 707,
                        Culture = CreateCulture(),
                        Milk = CreateMilk()
                    },
                    Single = new Single
                    {
                        Id = 21,
                        Culture = CreateCulture(),
                        Milk = CreateMilk()
                    },
                    ChildrenAk = new List<ChildAk>
                    {
                        new()
                        {
                            Id = 31,
                            Culture = CreateCulture(),
                            Milk = CreateMilk()
                        },
                        new()
                        {
                            Id = 32,
                            Culture = CreateCulture(),
                            Milk = CreateMilk()
                        }
                    },
                    SingleAk = new SingleAk
                    {
                        Id = 42,
                        Culture = CreateCulture(),
                        Milk = CreateMilk()
                    },
                    ChildrenShadowFk = new List<ChildShadowFk>
                    {
                        new()
                        {
                            Id = 51,
                            Culture = CreateCulture(),
                            Milk = CreateMilk()
                        },
                        new()
                        {
                            Id = 52,
                            Culture = CreateCulture(),
                            Milk = CreateMilk()
                        }
                    },
                    SingleShadowFk = new SingleShadowFk
                    {
                        Id = 62,
                        Culture = CreateCulture(),
                        Milk = CreateMilk()
                    },
                    ChildrenCompositeKey =
                        new List<ChildCompositeKey>
                        {
                            new()
                            {
                                Id = 51,
                                Culture = CreateCulture(),
                                Milk = CreateMilk()
                            },
                            new()
                            {
                                Id = 52,
                                Culture = CreateCulture(),
                                Milk = CreateMilk()
                            }
                        },
                    SingleCompositeKey = new SingleCompositeKey
                    {
                        Id = 62,
                        Culture = CreateCulture(),
                        Milk = CreateMilk()
                    },
                    WithRecursiveProperty = new WithRecursiveProperty { Id = 8086 },
                    ManyChildren = new List<Child>
                    {
                        new()
                        {
                            Id = 999,
                            Culture = CreateCulture(),
                            Milk = CreateMilk()
                        }
                    },
                    Culture = CreateCulture(),
                    Milk = CreateMilk()
                });

            context.Add(
                new Blog
                {
                    Writer = new Person
                    {
                        FirstName = "firstNameWriter0",
                        LastName = "lastNameWriter0",
                        Culture = CreateCulture(),
                        Milk = CreateMilk()
                    },
                    Reader = new Person
                    {
                        FirstName = "firstNameReader0",
                        LastName = "lastNameReader0",
                        Culture = CreateCulture(),
                        Milk = CreateMilk()
                    },
                    Host = new Host
                    {
                        HostName = "127.0.0.1",
                        Culture = CreateCulture(),
                        Milk = CreateMilk()
                    },
                    Culture = CreateCulture(),
                    Milk = CreateMilk()
                });

            context.Add(
                new Blog
                {
                    Writer = new Person
                    {
                        FirstName = "firstNameWriter1",
                        LastName = "lastNameWriter1",
                        Culture = CreateCulture(),
                        Milk = CreateMilk()
                    },
                    Reader = new Person
                    {
                        FirstName = "firstNameReader1",
                        LastName = "lastNameReader1",
                        Culture = CreateCulture(),
                        Milk = CreateMilk()
                    },
                    Host = new Host
                    {
                        HostName = "127.0.0.2",
                        Culture = CreateCulture(),
                        Milk = CreateMilk()
                    },
                    Culture = CreateCulture(),
                    Milk = CreateMilk()
                });

            context.Add(
                new Blog
                {
                    Writer = new Person
                    {
                        FirstName = "firstNameWriter2",
                        LastName = "lastNameWriter2",
                        Culture = CreateCulture(),
                        Milk = CreateMilk()
                    },
                    Reader = new Person
                    {
                        FirstName = "firstNameReader2",
                        LastName = "lastNameReader2",
                        Culture = CreateCulture(),
                        Milk = CreateMilk()
                    },
                    Host = new Host
                    {
                        HostName = "127.0.0.3",
                        Culture = CreateCulture(),
                        Milk = CreateMilk()
                    },
                    Culture = CreateCulture(),
                    Milk = CreateMilk()
                });

            var nose1 = new Nose
            {
                Size = "Small",
                Culture = CreateCulture(),
                Milk = CreateMilk()
            };

            var nose2 = new Nose
            {
                Size = "Medium",
                Culture = CreateCulture(),
                Milk = CreateMilk()
            };

            var nose3 = new Nose
            {
                Size = "Large",
                Culture = CreateCulture(),
                Milk = CreateMilk()
            };

            context.Add(
                new Entity
                {
                    BaseNoses = new List<Nose>
                    {
                        nose1,
                        nose2,
                        nose3
                    },
                    Culture = CreateCulture(),
                    Milk = CreateMilk()
                });

            context.Add(
                new Parson
                {
                    ParsonNoses = new List<Nose> { nose2, nose3 },
                    Culture = CreateCulture(),
                    Milk = CreateMilk()
                });

            context.Add(
                new Company
                {
                    CompanyNoses = new List<Nose> { nose1, nose3 },
                    Culture = CreateCulture(),
                    Milk = CreateMilk()
                });

            context.Add(
                new Applicant(
                    new FullName(FirstName.Create("Amila"), LastName.Create("Udayanga"))));

            context.Add(
                new Pyrson(new FullName(FirstName.Create("Amila"), LastName.Create("Udayanga")))
                {
                    Address = new Address
                    {
                        Line1 = "Line1",
                        Line2 = "Line2",
                        Culture = CreateCulture(),
                        Milk = CreateMilk()
                    },
                    Culture = CreateCulture(),
                    Milk = CreateMilk()
                });

            context.Add(
                new NonVirtualOneToOneOwner
                {
                    Id = 100,
                    Address = new OwnedAddress { Street = "Paradise Alley", PostalCode = "WEEEEEE" },
                    Culture = CreateCulture(),
                    Milk = CreateMilk()
                });

            context.Add(
                new VirtualOneToOneOwner
                {
                    Id = 200,
                    Address = new OwnedAddress { Street = "Dead End", PostalCode = "N0 WA1R" },
                    Culture = CreateCulture(),
                    Milk = CreateMilk()
                });

            context.Add(
                new NonVirtualOneToManyOwner
                {
                    Id = 300,
                    Addresses = new List<OwnedAddress> { new() { Street = "4 Privet Drive", PostalCode = "SURREY" } },
                    Culture = CreateCulture(),
                    Milk = CreateMilk()
                });

            context.Add(
                new VirtualOneToManyOwner
                {
                    Id = 400,
                    Addresses = new List<OwnedAddress>
                    {
                        new() { Street = "The Ministry", PostalCode = "MAG1C" },
                        new() { Street = "Diagon Alley", PostalCode = "WC2H 0AW" },
                        new() { Street = "Shell Cottage", PostalCode = "THE SEA" }
                    },
                    Culture = CreateCulture(),
                    Milk = CreateMilk()
                });

            context.Add(
                new ExplicitLazyLoadNonVirtualOneToManyOwner
                {
                    Id = 500,
                    Addresses = new List<OwnedAddress> { new() { Street = "Spinner's End", PostalCode = "BE WA1R" } },
                    Culture = CreateCulture(),
                    Milk = CreateMilk()
                });

            context.Add(
                new ExplicitLazyLoadVirtualOneToManyOwner
                {
                    Id = 600,
                    Addresses = new List<OwnedAddress> { new() { Street = "12 Grimmauld Place", PostalCode = "L0N D0N" } },
                    Culture = CreateCulture(),
                    Milk = CreateMilk()
                });

            context.Add(
                new NonVirtualParent
                {
                    Id = 100,
                    Child = new NonVirtualChild
                    {
                        Id = 100,
                        Culture = CreateCulture(),
                        Milk = CreateMilk()
                    },
                    Children = new List<NonVirtualChild>
                    {
                        new()
                        {
                            Id = 101,
                            Culture = CreateCulture(),
                            Milk = CreateMilk()
                        }
                    },
                    Culture = CreateCulture(),
                    Milk = CreateMilk()
                });

            return context.SaveChangesAsync();
        }

        protected static Culture CreateCulture()
            => new()
            {
                License = new License
                {
                    Charge = 1.0m,
                    Tag = new Tag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new Tog { Text = "To1" }
                },
                Manufacturer = new Manufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new Tag { Text = "Ta2" },
                    Tog = new Tog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            };

        protected static Milk CreateMilk()
            => new()
            {
                License = new License
                {
                    Charge = 1.0m,
                    Tag = new Tag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new Tog { Text = "To1" }
                },
                Manufacturer = new Manufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new Tag { Text = "Ta2" },
                    Tog = new Tog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            };
    }
}
