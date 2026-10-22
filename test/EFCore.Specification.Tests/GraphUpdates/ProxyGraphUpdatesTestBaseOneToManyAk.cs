// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming
// ReSharper disable AccessToModifiedClosure
// ReSharper disable PossibleMultipleEnumeration

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract partial class ProxyGraphUpdatesTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : ProxyGraphUpdatesTestBase<TFixture>.ProxyGraphUpdatesFixtureBase, new()
{
    [ConditionalTheory]
    [InlineData((int)ChangeMechanism.Principal, false)]
    [InlineData((int)ChangeMechanism.Principal, true)]
    [InlineData((int)ChangeMechanism.Dependent, false)]
    [InlineData((int)ChangeMechanism.Dependent, true)]
    [InlineData((int)ChangeMechanism.Fk, false)]
    [InlineData((int)ChangeMechanism.Fk, true)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), false)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), true)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), false)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), true)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), false)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), true)]
    public virtual Task Save_optional_many_to_one_dependents_with_alternate_key(
        ChangeMechanism changeMechanism,
        bool useExistingEntities)
    {
        OptionalAk1 new1 = null;
        OptionalAk1Derived new1d = null;
        OptionalAk1MoreDerived new1dd = null;
        OptionalAk2 new2a = null;
        OptionalAk2 new2b = null;
        OptionalComposite2 new2ca = null;
        OptionalComposite2 new2cb = null;
        OptionalAk2Derived new2d = null;
        OptionalAk2MoreDerived new2dd = null;

        return ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                new1 = context.CreateProxy<OptionalAk1>(e => e.AlternateId = Guid.NewGuid());
                new1d = context.CreateProxy<OptionalAk1Derived>(e => e.AlternateId = Guid.NewGuid());
                new1dd = context.CreateProxy<OptionalAk1MoreDerived>(e => e.AlternateId = Guid.NewGuid());
                new2a = context.CreateProxy<OptionalAk2>(e => e.AlternateId = Guid.NewGuid());
                new2b = context.CreateProxy<OptionalAk2>(e => e.AlternateId = Guid.NewGuid());
                new2ca = context.CreateProxy<OptionalComposite2>();
                new2cb = context.CreateProxy<OptionalComposite2>();
                new2d = context.CreateProxy<OptionalAk2Derived>(e => e.AlternateId = Guid.NewGuid());
                new2dd = context.CreateProxy<OptionalAk2MoreDerived>(e => e.AlternateId = Guid.NewGuid());

                if (useExistingEntities)
                {
                    context.AddRange(new1, new1d, new1dd, new2a, new2d, new2dd, new2b, new2ca, new2cb);
                    context.SaveChanges();
                }

                return Task.CompletedTask;
            }, async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.OptionalChildrenAk).Load();
                }

                var existing = root.OptionalChildrenAk.OrderBy(e => e.Id).First();

                if (useExistingEntities)
                {
                    new1 = context.Set<OptionalAk1>().Single(e => e.Id == new1.Id);
                    new1d = (OptionalAk1Derived)context.Set<OptionalAk1>().Single(e => e.Id == new1d.Id);
                    new1dd = (OptionalAk1MoreDerived)context.Set<OptionalAk1>().Single(e => e.Id == new1dd.Id);
                    new2a = context.Set<OptionalAk2>().Single(e => e.Id == new2a.Id);
                    new2b = context.Set<OptionalAk2>().Single(e => e.Id == new2b.Id);
                    new2ca = context.Set<OptionalComposite2>().Single(e => e.Id == new2ca.Id);
                    new2cb = context.Set<OptionalComposite2>().Single(e => e.Id == new2cb.Id);
                    new2d = (OptionalAk2Derived)context.Set<OptionalAk2>().Single(e => e.Id == new2d.Id);
                    new2dd = (OptionalAk2MoreDerived)context.Set<OptionalAk2>().Single(e => e.Id == new2dd.Id);
                }
                else
                {
                    context.AddRange(new1, new1d, new1dd, new2a, new2d, new2dd, new2b, new2ca, new2cb);
                }

                if ((changeMechanism & ChangeMechanism.Principal) != 0)
                {
                    Add(existing.Children, new2a);
                    Add(existing.Children, new2b);
                    Add(existing.CompositeChildren, new2ca);
                    Add(existing.CompositeChildren, new2cb);
                    Add(new1d.Children, new2d);
                    Add(new1dd.Children, new2dd);
                    Add(root.OptionalChildrenAk, new1);
                    Add(root.OptionalChildrenAk, new1d);
                    Add(root.OptionalChildrenAk, new1dd);
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    new2a.Parent = existing;
                    new2b.Parent = existing;
                    new2ca.Parent = existing;
                    new2cb.Parent = existing;
                    new2d.Parent = new1d;
                    new2dd.Parent = new1dd;
                    new1.Parent = root;
                    new1d.Parent = root;
                    new1dd.Parent = root;
                }

                if ((changeMechanism & ChangeMechanism.Fk) != 0)
                {
                    new2a.ParentId = existing.AlternateId;
                    new2b.ParentId = existing.AlternateId;
                    new2ca.ParentId = existing.Id;
                    new2ca.ParentAlternateId = existing.AlternateId;
                    new2cb.ParentId = existing.Id;
                    new2cb.ParentAlternateId = existing.AlternateId;
                    new2d.ParentId = new1d.AlternateId;
                    new2dd.ParentId = new1dd.AlternateId;
                    new1.ParentId = root.AlternateId;
                    new1d.ParentId = root.AlternateId;
                    new1dd.ParentId = root.AlternateId;
                }

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Contains(new2a, existing.Children);
                Assert.Contains(new2b, existing.Children);
                Assert.Contains(new2ca, existing.CompositeChildren);
                Assert.Contains(new2cb, existing.CompositeChildren);
                Assert.Contains(new2d, new1d.Children);
                Assert.Contains(new2dd, new1dd.Children);
                Assert.Contains(new1, root.OptionalChildrenAk);
                Assert.Contains(new1d, root.OptionalChildrenAk);
                Assert.Contains(new1dd, root.OptionalChildrenAk);

                Assert.Same(existing, new2a.Parent);
                Assert.Same(existing, new2b.Parent);
                Assert.Same(existing, new2ca.Parent);
                Assert.Same(existing, new2cb.Parent);
                Assert.Same(new1d, new2d.Parent);
                Assert.Same(new1dd, new2dd.Parent);
                Assert.Same(root, existing.Parent);
                Assert.Same(root, new1d.Parent);
                Assert.Same(root, new1dd.Parent);

                Assert.Equal(existing.AlternateId, new2a.ParentId);
                Assert.Equal(existing.AlternateId, new2b.ParentId);
                Assert.Equal(existing.Id, new2ca.ParentId);
                Assert.Equal(existing.Id, new2cb.ParentId);
                Assert.Equal(existing.AlternateId, new2ca.ParentAlternateId);
                Assert.Equal(existing.AlternateId, new2cb.ParentAlternateId);
                Assert.Equal(new1d.AlternateId, new2d.ParentId);
                Assert.Equal(new1dd.AlternateId, new2dd.ParentId);
                Assert.Equal(root.AlternateId, existing.ParentId);
                Assert.Equal(root.AlternateId, new1d.ParentId);
                Assert.Equal(root.AlternateId, new1dd.ParentId);
            });
    }

    [ConditionalTheory]
    [InlineData((int)ChangeMechanism.Principal, false)]
    [InlineData((int)ChangeMechanism.Principal, true)]
    [InlineData((int)ChangeMechanism.Dependent, false)]
    [InlineData((int)ChangeMechanism.Dependent, true)]
    [InlineData((int)ChangeMechanism.Fk, false)]
    [InlineData((int)ChangeMechanism.Fk, true)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), false)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), true)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), false)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), true)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), false)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), true)]
    public virtual Task Save_required_many_to_one_dependents_with_alternate_key(
        ChangeMechanism changeMechanism,
        bool useExistingEntities)
    {
        Root newRoot;
        RequiredAk1 new1 = null;
        RequiredAk1Derived new1d = null;
        RequiredAk1MoreDerived new1dd = null;
        RequiredAk2 new2a = null;
        RequiredAk2 new2b = null;
        RequiredComposite2 new2ca = null;
        RequiredComposite2 new2cb = null;
        RequiredAk2Derived new2d = null;
        RequiredAk2MoreDerived new2dd = null;

        return ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                newRoot = context.CreateProxy<Root>(e => e.AlternateId = Guid.NewGuid());
                new1 = context.CreateProxy<RequiredAk1>(
                    e =>
                    {
                        e.AlternateId = Guid.NewGuid();
                        e.Parent = newRoot;
                    });
                new1d = context.CreateProxy<RequiredAk1Derived>(
                    e =>
                    {
                        e.AlternateId = Guid.NewGuid();
                        e.Parent = newRoot;
                    });
                new1dd = context.CreateProxy<RequiredAk1MoreDerived>(
                    e =>
                    {
                        e.AlternateId = Guid.NewGuid();
                        e.Parent = newRoot;
                    });
                new2a = context.CreateProxy<RequiredAk2>(
                    e =>
                    {
                        e.AlternateId = Guid.NewGuid();
                        e.Parent = new1;
                    });
                new2b = context.CreateProxy<RequiredAk2>(
                    e =>
                    {
                        e.AlternateId = Guid.NewGuid();
                        e.Parent = new1;
                    });
                new2ca = context.CreateProxy<RequiredComposite2>(e => e.Parent = new1);
                new2cb = context.CreateProxy<RequiredComposite2>(e => e.Parent = new1);
                new2d = context.CreateProxy<RequiredAk2Derived>(
                    e =>
                    {
                        e.AlternateId = Guid.NewGuid();
                        e.Parent = new1;
                    });
                new2dd = context.CreateProxy<RequiredAk2MoreDerived>(
                    e =>
                    {
                        e.AlternateId = Guid.NewGuid();
                        e.Parent = new1;
                    });

                if (useExistingEntities)
                {
                    context.AddRange(newRoot, new1, new1d, new1dd, new2a, new2d, new2dd, new2b, new2ca, new2cb);
                    context.SaveChanges();
                }

                return Task.CompletedTask;
            }, async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.RequiredChildrenAk).Load();
                }

                var existing = root.RequiredChildrenAk.OrderBy(e => e.Id).First();

                if (useExistingEntities)
                {
                    new1 = context.Set<RequiredAk1>().Single(e => e.Id == new1.Id);
                    new1d = (RequiredAk1Derived)context.Set<RequiredAk1>().Single(e => e.Id == new1d.Id);
                    new1dd = (RequiredAk1MoreDerived)context.Set<RequiredAk1>().Single(e => e.Id == new1dd.Id);
                    new2a = context.Set<RequiredAk2>().Single(e => e.Id == new2a.Id);
                    new2b = context.Set<RequiredAk2>().Single(e => e.Id == new2b.Id);
                    new2ca = context.Set<RequiredComposite2>().Single(e => e.Id == new2ca.Id);
                    new2cb = context.Set<RequiredComposite2>().Single(e => e.Id == new2cb.Id);
                    new2d = (RequiredAk2Derived)context.Set<RequiredAk2>().Single(e => e.Id == new2d.Id);
                    new2dd = (RequiredAk2MoreDerived)context.Set<RequiredAk2>().Single(e => e.Id == new2dd.Id);
                }
                else
                {
                    new1.Parent = null;
                    new1d.Parent = null;
                    new1dd.Parent = null;

                    context.AddRange(new1, new1d, new1dd, new2a, new2d, new2dd, new2b, new2ca, new2cb);
                }

                if ((changeMechanism & ChangeMechanism.Principal) != 0)
                {
                    Add(existing.Children, new2a);
                    Add(existing.Children, new2b);
                    Add(existing.CompositeChildren, new2ca);
                    Add(existing.CompositeChildren, new2cb);
                    Add(new1d.Children, new2d);
                    Add(new1dd.Children, new2dd);
                    Add(root.RequiredChildrenAk, new1);
                    Add(root.RequiredChildrenAk, new1d);
                    Add(root.RequiredChildrenAk, new1dd);
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    new2a.Parent = existing;
                    new2b.Parent = existing;
                    new2ca.Parent = existing;
                    new2cb.Parent = existing;
                    new2d.Parent = new1d;
                    new2dd.Parent = new1dd;
                    new1.Parent = root;
                    new1d.Parent = root;
                    new1dd.Parent = root;
                }

                if ((changeMechanism & ChangeMechanism.Fk) != 0)
                {
                    new2a.ParentId = existing.AlternateId;
                    new2b.ParentId = existing.AlternateId;
                    new2ca.ParentId = existing.Id;
                    new2cb.ParentId = existing.Id;
                    new2ca.ParentAlternateId = existing.AlternateId;
                    new2cb.ParentAlternateId = existing.AlternateId;
                    new2d.ParentId = new1d.AlternateId;
                    new2dd.ParentId = new1dd.AlternateId;
                    new1.ParentId = root.AlternateId;
                    new1d.ParentId = root.AlternateId;
                    new1dd.ParentId = root.AlternateId;
                }

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Contains(new2a, existing.Children);
                Assert.Contains(new2b, existing.Children);
                Assert.Contains(new2ca, existing.CompositeChildren);
                Assert.Contains(new2cb, existing.CompositeChildren);
                Assert.Contains(new2d, new1d.Children);
                Assert.Contains(new2dd, new1dd.Children);
                Assert.Contains(new1, root.RequiredChildrenAk);
                Assert.Contains(new1d, root.RequiredChildrenAk);
                Assert.Contains(new1dd, root.RequiredChildrenAk);

                Assert.Same(existing, new2a.Parent);
                Assert.Same(existing, new2b.Parent);
                Assert.Same(existing, new2ca.Parent);
                Assert.Same(existing, new2cb.Parent);
                Assert.Same(new1d, new2d.Parent);
                Assert.Same(new1dd, new2dd.Parent);
                Assert.Same(root, existing.Parent);
                Assert.Same(root, new1d.Parent);
                Assert.Same(root, new1dd.Parent);

                Assert.Equal(existing.AlternateId, new2a.ParentId);
                Assert.Equal(existing.AlternateId, new2b.ParentId);
                Assert.Equal(existing.Id, new2ca.ParentId);
                Assert.Equal(existing.Id, new2cb.ParentId);
                Assert.Equal(existing.AlternateId, new2ca.ParentAlternateId);
                Assert.Equal(existing.AlternateId, new2cb.ParentAlternateId);
                Assert.Equal(new1d.AlternateId, new2d.ParentId);
                Assert.Equal(new1dd.AlternateId, new2dd.ParentId);
                Assert.Equal(root.AlternateId, existing.ParentId);
                Assert.Equal(root.AlternateId, new1d.ParentId);
                Assert.Equal(root.AlternateId, new1dd.ParentId);
            });
    }

    [ConditionalTheory]
    [InlineData((int)ChangeMechanism.Principal)]
    [InlineData((int)ChangeMechanism.Dependent)]
    [InlineData((int)ChangeMechanism.Fk)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk))]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent))]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk))]
    public virtual Task Save_removed_optional_many_to_one_dependents_with_alternate_key(ChangeMechanism changeMechanism)
    {
        Root root;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.OptionalChildrenAk).Load();
                    context.Entry(root.OptionalChildrenAk.First()).Collection(e => e.Children).Load();
                    context.Entry(root.OptionalChildrenAk.First()).Collection(e => e.CompositeChildren).Load();
                }

                var childCollection = root.OptionalChildrenAk.First().Children;
                var childCompositeCollection = root.OptionalChildrenAk.First().CompositeChildren;
                var removed2 = childCollection.First();
                var removed1 = root.OptionalChildrenAk.Skip(1).First();
                var removed2c = childCompositeCollection.First();

                if ((changeMechanism & ChangeMechanism.Principal) != 0)
                {
                    Remove(childCollection, removed2);
                    Remove(childCompositeCollection, removed2c);
                    Remove(root.OptionalChildrenAk, removed1);
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    removed2.Parent = null;
                    removed2c.Parent = null;
                    removed1.Parent = null;
                }

                if ((changeMechanism & ChangeMechanism.Fk) != 0)
                {
                    removed2.ParentId = null;
                    removed2c.ParentId = null;
                    removed1.ParentId = null;
                }

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.DoesNotContain(removed1, root.OptionalChildrenAk);
                Assert.DoesNotContain(removed2, childCollection);
                Assert.DoesNotContain(removed2c, childCompositeCollection);

                Assert.Null(removed1.Parent);
                Assert.Null(removed2.Parent);
                Assert.Null(removed2c.Parent);

                Assert.Null(removed1.ParentId);
                Assert.Null(removed2.ParentId);
                Assert.Null(removed2c.ParentId);
            }, async context =>
            {
                if ((changeMechanism & ChangeMechanism.Fk) == 0)
                {
                    var loadedRoot = await LoadRootAsync(context);

                    if (!DoesLazyLoading)
                    {
                        context.Entry(loadedRoot).Collection(e => e.OptionalChildrenAk).Load();
                        context.Entry(loadedRoot.OptionalChildrenAk.First()).Collection(e => e.Children).Load();
                    }

                    Assert.Single(loadedRoot.OptionalChildrenAk);
                    Assert.Single(loadedRoot.OptionalChildrenAk.First().Children);
                }
            });
    }

    [ConditionalTheory]
    [InlineData((int)ChangeMechanism.Principal)]
    [InlineData((int)ChangeMechanism.Dependent)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
    public virtual Task Save_removed_required_many_to_one_dependents_with_alternate_key(ChangeMechanism changeMechanism)
    {
        Root root = null;
        RequiredAk2 removed2 = null;
        RequiredComposite2 removed2c = null;
        RequiredAk1 removed1 = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.RequiredChildrenAk).Load();
                    context.Entry(root.RequiredChildrenAk.First()).Collection(e => e.Children).Load();
                    context.Entry(root.RequiredChildrenAk.First()).Collection(e => e.CompositeChildren).Load();
                }

                var childCollection = root.RequiredChildrenAk.First().Children;
                var childCompositeCollection = root.RequiredChildrenAk.First().CompositeChildren;
                removed2 = childCollection.First();
                removed2c = childCompositeCollection.First();
                removed1 = root.RequiredChildrenAk.Skip(1).First();

                if ((changeMechanism & ChangeMechanism.Principal) != 0)
                {
                    Remove(childCollection, removed2);
                    Remove(childCompositeCollection, removed2c);
                    Remove(root.RequiredChildrenAk, removed1);
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    removed2.Parent = null;
                    removed2c.Parent = null;
                    removed1.Parent = null;
                }

                if ((changeMechanism & ChangeMechanism.Fk) != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.DoesNotContain(removed1, root.RequiredChildrenAk);
                Assert.DoesNotContain(removed2, childCollection);
                Assert.DoesNotContain(removed2c, childCompositeCollection);

                Assert.Null(removed1.Parent);
                Assert.Null(removed2.Parent);
                Assert.Null(removed2c.Parent);
            }, async context =>
            {
                var loadedRoot = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(loadedRoot).Collection(e => e.RequiredChildrenAk).Load();
                    context.Entry(loadedRoot.RequiredChildrenAk.First()).Collection(e => e.Children).Load();
                    context.Entry(loadedRoot.RequiredChildrenAk.First()).Collection(e => e.CompositeChildren).Load();
                }

                Assert.False(context.Set<RequiredAk1>().Any(e => e.Id == removed1.Id));
                Assert.False(context.Set<RequiredAk2>().Any(e => e.Id == removed2.Id));
                Assert.False(context.Set<RequiredComposite2>().Any(e => e.Id == removed2c.Id));

                Assert.Single(loadedRoot.RequiredChildrenAk);
                Assert.Single(loadedRoot.RequiredChildrenAk.First().Children);
                Assert.Single(loadedRoot.RequiredChildrenAk.First().CompositeChildren);
            });
    }

    [ConditionalTheory]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.Never)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.Never)]
    [InlineData(CascadeTiming.Never, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.Never, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.Never, CascadeTiming.Never)]
    public virtual Task Optional_many_to_one_dependents_with_alternate_key_are_orphaned(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.OptionalChildrenAk).Load();
                }

                Assert.Equal(2, root.OptionalChildrenAk.Count());

                var removed = root.OptionalChildrenAk.First();
                context.Entry(removed).Collection(e => e.CompositeChildren).Load();

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Collection(e => e.Children).Load();
                }

                removedId = removed.Id;
                var orphaned = removed.Children.ToList();
                orphanedIds = orphaned.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));

                Assert.Single(root.OptionalChildrenAk);
                Assert.DoesNotContain(removedId, root.OptionalChildrenAk.Select(e => e.Id));

                Assert.Empty(context.Set<OptionalAk1>().Where(e => e.Id == removedId));
                Assert.Equal(orphanedIds.Count, context.Set<OptionalAk2>().Count(e => orphanedIds.Contains(e.Id)));

                Assert.Same(root, removed.Parent);
                Assert.Equal(2, removed.Children.Count());
            }, async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.OptionalChildrenAk).Load();
                }

                Assert.Single(root.OptionalChildrenAk);
                Assert.DoesNotContain(removedId, root.OptionalChildrenAk.Select(e => e.Id));

                Assert.Empty(context.Set<OptionalAk1>().Where(e => e.Id == removedId));
                Assert.Equal(orphanedIds.Count, context.Set<OptionalAk2>().Count(e => orphanedIds.Contains(e.Id)));
            });
    }

    [ConditionalTheory]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.Never)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.Never)]
    [InlineData(CascadeTiming.Never, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.Never, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.Never, CascadeTiming.Never)]
    public virtual Task Required_many_to_one_dependents_with_alternate_key_are_cascade_deleted(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;
        List<int> orphanedIdCs = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.RequiredChildrenAk).Load();
                }

                Assert.Equal(2, root.RequiredChildrenAk.Count());

                var removed = root.RequiredChildrenAk.First();

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Collection(e => e.Children).Load();
                    context.Entry(removed).Collection(e => e.CompositeChildren).Load();
                }

                removedId = removed.Id;
                var cascadeRemoved = removed.Children.ToList();
                var cascadeRemovedC = removed.CompositeChildren.ToList();
                orphanedIds = cascadeRemoved.Select(e => e.Id).ToList();
                orphanedIdCs = cascadeRemovedC.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
                Assert.Equal(2, orphanedIdCs.Count);

                context.Remove(removed);

                Assert.True(context.ChangeTracker.HasChanges());

                if (cascadeDeleteTiming == CascadeTiming.Never)
                {
                    Assert.Throws<InvalidOperationException>(() => context.SaveChanges());
                }
                else
                {
                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                    Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Detached));
                    Assert.True(cascadeRemovedC.All(e => context.Entry(e).State == EntityState.Detached));

                    Assert.Single(root.RequiredChildrenAk);
                    Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                    Assert.Empty(context.Set<RequiredAk1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<RequiredAk2>().Where(e => orphanedIds.Contains(e.Id)));

                    Assert.Same(root, removed.Parent);
                    Assert.Equal(2, removed.Children.Count());
                }
            }, async context =>
            {
                if (cascadeDeleteTiming != CascadeTiming.Never)
                {
                    var root = await LoadRootAsync(context);

                    if (!DoesLazyLoading)
                    {
                        context.Entry(root).Collection(e => e.RequiredChildrenAk).Load();
                    }

                    Assert.Single(root.RequiredChildrenAk);
                    Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                    Assert.Empty(context.Set<RequiredAk1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<RequiredAk2>().Where(e => orphanedIds.Contains(e.Id)));
                    Assert.Empty(context.Set<RequiredComposite2>().Where(e => orphanedIdCs.Contains(e.Id)));
                }
            });
    }

    [ConditionalTheory]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.Never)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.Never)]
    [InlineData(CascadeTiming.Never, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.Never, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.Never, CascadeTiming.Never)]
    public virtual Task Required_many_to_one_dependents_with_alternate_key_are_cascade_deleted_in_store(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;
        List<int> orphanedIdCs = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.RequiredChildrenAk).Load();
                }

                var removed = root.RequiredChildrenAk.First();

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Collection(e => e.Children).Load();
                    context.Entry(removed).Collection(e => e.CompositeChildren).Load();
                }

                removedId = removed.Id;
                orphanedIds = removed.Children.Select(e => e.Id).ToList();
                orphanedIdCs = removed.CompositeChildren.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
                Assert.Equal(2, orphanedIdCs.Count);
            },
            context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

                var root = context.Set<Root>().Include(e => e.RequiredChildrenAk).Single(IsTheRoot);

                var removed = root.RequiredChildrenAk.Single(e => e.Id == removedId);

                context.Remove(removed);

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Single(root.RequiredChildrenAk);
                Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                Assert.Empty(context.Set<RequiredAk1>().Where(e => e.Id == removedId));
                Assert.Empty(context.Set<RequiredAk2>().Where(e => orphanedIds.Contains(e.Id)));
                Assert.Empty(context.Set<RequiredComposite2>().Where(e => orphanedIdCs.Contains(e.Id)));

                Assert.Same(root, removed.Parent);
                Assert.Empty(removed.Children); // Never loaded
                return Task.CompletedTask;
            }, async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.RequiredChildrenAk).Load();
                }

                Assert.Single(root.RequiredChildrenAk);
                Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                Assert.Empty(context.Set<RequiredAk1>().Where(e => e.Id == removedId));
                Assert.Empty(context.Set<RequiredAk2>().Where(e => orphanedIds.Contains(e.Id)));
                Assert.Empty(context.Set<RequiredComposite2>().Where(e => orphanedIdCs.Contains(e.Id)));
            });
    }

    [ConditionalTheory]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.Never)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.Never)]
    [InlineData(CascadeTiming.Never, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.Never, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.Never, CascadeTiming.Never)]
    public virtual Task Optional_many_to_one_dependents_with_alternate_key_are_orphaned_in_store(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;
        List<int> orphanedIdCs = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.OptionalChildrenAk).Load();
                }

                var removed = root.OptionalChildrenAk.First();

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Collection(e => e.Children).Load();
                    context.Entry(removed).Collection(e => e.CompositeChildren).Load();
                }

                removedId = removed.Id;
                orphanedIds = removed.Children.Select(e => e.Id).ToList();
                orphanedIdCs = removed.CompositeChildren.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
                Assert.Equal(2, orphanedIdCs.Count);
            },
            context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

                var root = context.Set<Root>().Include(e => e.OptionalChildrenAk).Single(IsTheRoot);

                var removed = root.OptionalChildrenAk.First(e => e.Id == removedId);

                context.Remove(removed);

                foreach (var toOrphan in context.Set<OptionalComposite2>().Where(e => orphanedIdCs.Contains(e.Id)).ToList())
                {
                    toOrphan.ParentId = null;
                }

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Single(root.OptionalChildrenAk);
                Assert.DoesNotContain(removedId, root.OptionalChildrenAk.Select(e => e.Id));

                Assert.Empty(context.Set<OptionalAk1>().Where(e => e.Id == removedId));

                var orphaned = context.Set<OptionalAk2>().Where(e => orphanedIds.Contains(e.Id)).ToList();
                Assert.Equal(orphanedIds.Count, orphaned.Count);
                Assert.True(orphaned.All(e => e.ParentId == null));

                var orphanedC = context.Set<OptionalComposite2>().Where(e => orphanedIdCs.Contains(e.Id)).ToList();
                Assert.Equal(orphanedIdCs.Count, orphanedC.Count);
                Assert.True(orphanedC.All(e => e.ParentId == null));

                Assert.Same(root, removed.Parent);
                Assert.Empty(removed.Children); // Never loaded
                return Task.CompletedTask;
            }, async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.OptionalChildrenAk).Load();
                }

                Assert.Single(root.OptionalChildrenAk);
                Assert.DoesNotContain(removedId, root.OptionalChildrenAk.Select(e => e.Id));

                Assert.Empty(context.Set<OptionalAk1>().Where(e => e.Id == removedId));

                var orphaned = context.Set<OptionalAk2>().Where(e => orphanedIds.Contains(e.Id)).ToList();
                Assert.Equal(orphanedIds.Count, orphaned.Count);
                Assert.True(orphaned.All(e => e.ParentId == null));

                var orphanedC = context.Set<OptionalComposite2>().Where(e => orphanedIdCs.Contains(e.Id)).ToList();
                Assert.Equal(orphanedIdCs.Count, orphanedC.Count);
                Assert.True(orphanedC.All(e => e.ParentId == null));
            });
    }

    [ConditionalTheory]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.Never)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.Never)]
    [InlineData(CascadeTiming.Never, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.Never, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.Never, CascadeTiming.Never)]
    public virtual Task Optional_many_to_one_dependents_with_alternate_key_are_orphaned_starting_detached(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;
        List<int> orphanedIdCs = null;
        Root root = null;
        OptionalAk1 removed = null;
        List<OptionalAk2> orphaned = null;
        List<OptionalComposite2> orphanedC = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.OptionalChildrenAk).Load();
                }

                removed = root.OptionalChildrenAk.First();

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Collection(e => e.Children).Load();
                    context.Entry(removed).Collection(e => e.CompositeChildren).Load();
                }

                orphaned = removed.Children.ToList();
                orphanedC = removed.CompositeChildren.ToList();

                Assert.Equal(2, root.OptionalChildrenAk.Count());
            },
            context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

                removedId = removed.Id;
                orphanedIds = orphaned.Select(e => e.Id).ToList();
                orphanedIdCs = orphanedC.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
                Assert.Equal(2, orphanedIdCs.Count);

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);

                var expectedState = cascadeDeleteTiming == CascadeTiming.Immediate
                    ? EntityState.Modified
                    : EntityState.Unchanged;

                Assert.True(orphaned.All(e => context.Entry(e).State == expectedState));
                Assert.True(orphanedC.All(e => context.Entry(e).State == expectedState));

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));
                Assert.True(orphanedC.All(e => context.Entry(e).State == EntityState.Unchanged));

                Assert.Same(root, removed.Parent);
                Assert.Equal(2, removed.Children.Count());
                return Task.CompletedTask;
            }, async context =>
            {
                root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.OptionalChildrenAk).Load();
                }

                Assert.Single(root.OptionalChildrenAk);
                Assert.DoesNotContain(removedId, root.OptionalChildrenAk.Select(e => e.Id));

                Assert.Empty(context.Set<OptionalAk1>().Where(e => e.Id == removedId));
                Assert.Equal(orphanedIds.Count, context.Set<OptionalAk2>().Count(e => orphanedIds.Contains(e.Id)));
                Assert.Equal(orphanedIdCs.Count, context.Set<OptionalComposite2>().Count(e => orphanedIdCs.Contains(e.Id)));
            });
    }

    [ConditionalTheory]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.Never)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.Never)]
    [InlineData(CascadeTiming.Never, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.Never, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.Never, CascadeTiming.Never)]
    public virtual Task Required_many_to_one_dependents_with_alternate_key_are_cascade_deleted_starting_detached(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;
        List<int> orphanedIdCs = null;
        Root root = null;
        RequiredAk1 removed = null;
        List<RequiredAk2> cascadeRemoved = null;
        List<RequiredComposite2> cascadeRemovedC = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.RequiredChildrenAk).Load();
                }

                removed = root.RequiredChildrenAk.First();

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Collection(e => e.Children).Load();
                    context.Entry(removed).Collection(e => e.CompositeChildren).Load();
                }

                cascadeRemoved = removed.Children.ToList();
                cascadeRemovedC = removed.CompositeChildren.ToList();

                Assert.Equal(2, root.RequiredChildrenAk.Count());
            },
            context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

                removedId = removed.Id;
                orphanedIds = cascadeRemoved.Select(e => e.Id).ToList();
                orphanedIdCs = cascadeRemovedC.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);

                var expectedState = cascadeDeleteTiming == CascadeTiming.Immediate
                    ? EntityState.Deleted
                    : EntityState.Unchanged;

                Assert.True(cascadeRemoved.All(e => context.Entry(e).State == expectedState));
                Assert.True(cascadeRemovedC.All(e => context.Entry(e).State == expectedState));

                Assert.True(context.ChangeTracker.HasChanges());

                if (cascadeDeleteTiming == CascadeTiming.Never)
                {
                    Assert.Throws<InvalidOperationException>(() => context.SaveChanges());
                }
                else
                {
                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                    Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Detached));
                    Assert.True(cascadeRemovedC.All(e => context.Entry(e).State == EntityState.Detached));

                    Assert.Same(root, removed.Parent);
                    Assert.Equal(2, removed.Children.Count());
                }

                return Task.CompletedTask;
            }, async context =>
            {
                if (cascadeDeleteTiming != CascadeTiming.Never)
                {
                    root = await LoadRootAsync(context);

                    if (!DoesLazyLoading)
                    {
                        context.Entry(root).Collection(e => e.RequiredChildrenAk).Load();
                    }

                    Assert.Single(root.RequiredChildrenAk);
                    Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                    Assert.Empty(context.Set<RequiredAk1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<RequiredAk2>().Where(e => orphanedIds.Contains(e.Id)));
                    Assert.Empty(context.Set<RequiredComposite2>().Where(e => orphanedIdCs.Contains(e.Id)));
                }
            });
    }

    [ConditionalTheory]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.Never)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.Never)]
    [InlineData(CascadeTiming.Never, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.Never, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.Never, CascadeTiming.Never)]
    public virtual Task Required_many_to_one_dependents_with_alternate_key_are_cascade_detached_when_Added(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;
        List<int> orphanedIdCs = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.RequiredChildrenAk).Load();
                }

                Assert.Equal(2, root.RequiredChildrenAk.Count());

                var removed = root.RequiredChildrenAk.First();

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Collection(e => e.Children).Load();
                    context.Entry(removed).Collection(e => e.CompositeChildren).Load();
                }

                removedId = removed.Id;
                var cascadeRemoved = removed.Children.ToList();
                var cascadeRemovedC = removed.CompositeChildren.ToList();
                orphanedIds = cascadeRemoved.Select(e => e.Id).ToList();
                orphanedIdCs = cascadeRemovedC.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
                Assert.Equal(2, orphanedIdCs.Count);

                var added = context.CreateProxy<RequiredAk2>();
                var addedC = context.CreateProxy<RequiredComposite2>();
                Add(removed.Children, added);
                Add(removed.CompositeChildren, addedC);

                if (context.ChangeTracker.AutoDetectChangesEnabled
                    && !DoesChangeTracking)
                {
                    context.ChangeTracker.DetectChanges();
                }

                Assert.Equal(EntityState.Unchanged, context.Entry(removed).State);
                Assert.Equal(EntityState.Added, context.Entry(added).State);
                Assert.Equal(EntityState.Added, context.Entry(addedC).State);
                Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Unchanged));
                Assert.True(cascadeRemovedC.All(e => context.Entry(e).State == EntityState.Unchanged));

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);

                if (cascadeDeleteTiming == CascadeTiming.Immediate)
                {
                    Assert.Equal(EntityState.Detached, context.Entry(added).State);
                    Assert.Equal(EntityState.Detached, context.Entry(addedC).State);
                    Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Deleted));
                    Assert.True(cascadeRemovedC.All(e => context.Entry(e).State == EntityState.Deleted));
                }
                else
                {
                    Assert.Equal(EntityState.Added, context.Entry(added).State);
                    Assert.Equal(EntityState.Added, context.Entry(addedC).State);
                    Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Unchanged));
                    Assert.True(cascadeRemovedC.All(e => context.Entry(e).State == EntityState.Unchanged));
                }

                Assert.True(context.ChangeTracker.HasChanges());

                if (cascadeDeleteTiming == CascadeTiming.Never)
                {
                    Assert.Throws<InvalidOperationException>(() => context.SaveChanges());
                }
                else
                {
                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                    Assert.Equal(EntityState.Detached, context.Entry(added).State);
                    Assert.Equal(EntityState.Detached, context.Entry(addedC).State);
                    Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Detached));
                    Assert.True(cascadeRemovedC.All(e => context.Entry(e).State == EntityState.Detached));

                    Assert.Same(root, removed.Parent);
                    Assert.Equal(3, removed.Children.Count());
                }
            }, async context =>
            {
                if (cascadeDeleteTiming != CascadeTiming.Never)
                {
                    var root = await LoadRootAsync(context);

                    if (!DoesLazyLoading)
                    {
                        context.Entry(root).Collection(e => e.RequiredChildrenAk).Load();
                    }

                    Assert.Single(root.RequiredChildrenAk);
                    Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                    Assert.Empty(context.Set<RequiredAk1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<RequiredAk2>().Where(e => orphanedIds.Contains(e.Id)));
                    Assert.Empty(context.Set<RequiredComposite2>().Where(e => orphanedIdCs.Contains(e.Id)));
                }
            });
    }
}
