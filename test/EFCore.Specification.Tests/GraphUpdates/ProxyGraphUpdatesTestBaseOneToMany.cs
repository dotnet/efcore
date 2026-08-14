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
    public virtual Task Save_optional_many_to_one_dependents(ChangeMechanism changeMechanism, bool useExistingEntities)
    {
        Optional1 new1 = null;
        Optional1Derived new1d = null;
        Optional1MoreDerived new1dd = null;
        Optional2 new2a = null;
        Optional2 new2b = null;
        Optional2Derived new2d = null;
        Optional2MoreDerived new2dd = null;

        return ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                new1 = context.CreateProxy<Optional1>();
                new1d = context.CreateProxy<Optional1Derived>();
                new1dd = context.CreateProxy<Optional1MoreDerived>();
                new2a = context.CreateProxy<Optional2>();
                new2b = context.CreateProxy<Optional2>();
                new2d = context.CreateProxy<Optional2Derived>();
                new2dd = context.CreateProxy<Optional2MoreDerived>();

                if (useExistingEntities)
                {
                    context.AddRange(new1, new1d, new1dd, new2a, new2d, new2dd, new2b);
                    context.SaveChanges();
                }

                return Task.CompletedTask;
            }, async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.OptionalChildren).Load();
                }

                var existing = root.OptionalChildren.OrderBy(e => e.Id).First();

                if (useExistingEntities)
                {
                    new1 = context.Set<Optional1>().Single(e => e.Id == new1.Id);
                    new1d = (Optional1Derived)context.Set<Optional1>().Single(e => e.Id == new1d.Id);
                    new1dd = (Optional1MoreDerived)context.Set<Optional1>().Single(e => e.Id == new1dd.Id);
                    new2a = context.Set<Optional2>().Single(e => e.Id == new2a.Id);
                    new2b = context.Set<Optional2>().Single(e => e.Id == new2b.Id);
                    new2d = (Optional2Derived)context.Set<Optional2>().Single(e => e.Id == new2d.Id);
                    new2dd = (Optional2MoreDerived)context.Set<Optional2>().Single(e => e.Id == new2dd.Id);
                }
                else
                {
                    context.AddRange(new1, new1d, new1dd, new2a, new2d, new2dd, new2b);
                }

                if ((changeMechanism & ChangeMechanism.Principal) != 0)
                {
                    Add(existing.Children, new2a);
                    Add(existing.Children, new2b);
                    Add(new1d.Children, new2d);
                    Add(new1dd.Children, new2dd);
                    Add(root.OptionalChildren, new1);
                    Add(root.OptionalChildren, new1d);
                    Add(root.OptionalChildren, new1dd);
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    new2a.Parent = existing;
                    new2b.Parent = existing;
                    new2d.Parent = new1d;
                    new2dd.Parent = new1dd;
                    new1.Parent = root;
                    new1d.Parent = root;
                    new1dd.Parent = root;
                }

                if ((changeMechanism & ChangeMechanism.Fk) != 0)
                {
                    new2a.ParentId = context.Entry(existing).Property(e => e.Id).CurrentValue;
                    new2b.ParentId = context.Entry(existing).Property(e => e.Id).CurrentValue;
                    new2d.ParentId = context.Entry(new1d).Property(e => e.Id).CurrentValue;
                    new2dd.ParentId = context.Entry(new1dd).Property(e => e.Id).CurrentValue;
                    new1.ParentId = context.Entry(root).Property(e => e.Id).CurrentValue;
                    new1d.ParentId = context.Entry(root).Property(e => e.Id).CurrentValue;
                    new1dd.ParentId = context.Entry(root).Property(e => e.Id).CurrentValue;
                }

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Contains(new2a, existing.Children);
                Assert.Contains(new2b, existing.Children);
                Assert.Contains(new2d, new1d.Children);
                Assert.Contains(new2dd, new1dd.Children);
                Assert.Contains(new1, root.OptionalChildren);
                Assert.Contains(new1d, root.OptionalChildren);
                Assert.Contains(new1dd, root.OptionalChildren);

                Assert.Same(existing, new2a.Parent);
                Assert.Same(existing, new2b.Parent);
                Assert.Same(new1d, new2d.Parent);
                Assert.Same(new1dd, new2dd.Parent);
                Assert.Same(root, existing.Parent);
                Assert.Same(root, new1d.Parent);
                Assert.Same(root, new1dd.Parent);

                Assert.Equal(existing.Id, new2a.ParentId);
                Assert.Equal(existing.Id, new2b.ParentId);
                Assert.Equal(new1d.Id, new2d.ParentId);
                Assert.Equal(new1dd.Id, new2dd.ParentId);
                Assert.Equal(root.Id, existing.ParentId);
                Assert.Equal(root.Id, new1d.ParentId);
                Assert.Equal(root.Id, new1dd.ParentId);
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
    public virtual Task Save_required_many_to_one_dependents(ChangeMechanism changeMechanism, bool useExistingEntities)
    {
        Root newRoot;
        Required1 new1 = null;
        Required1Derived new1d = null;
        Required1MoreDerived new1dd = null;
        Required2 new2a = null;
        Required2 new2b = null;
        Required2Derived new2d = null;
        Required2MoreDerived new2dd = null;

        return ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                newRoot = context.CreateProxy<Root>();
                new1 = context.CreateProxy<Required1>(e => e.Parent = newRoot);
                new1d = context.CreateProxy<Required1Derived>(e => e.Parent = newRoot);
                new1dd = context.CreateProxy<Required1MoreDerived>(e => e.Parent = newRoot);
                new2a = context.CreateProxy<Required2>(e => e.Parent = new1);
                new2b = context.CreateProxy<Required2>(e => e.Parent = new1);
                new2d = context.CreateProxy<Required2Derived>(e => e.Parent = new1);
                new2dd = context.CreateProxy<Required2MoreDerived>(e => e.Parent = new1);

                if (useExistingEntities)
                {
                    context.AddRange(newRoot, new1, new1d, new1dd, new2a, new2d, new2dd, new2b);
                    context.SaveChanges();
                }

                return Task.CompletedTask;
            }, async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.RequiredChildren).Load();
                }

                var existing = root.RequiredChildren.OrderBy(e => e.Id).First();

                if (useExistingEntities)
                {
                    new1 = context.Set<Required1>().Single(e => e.Id == new1.Id);
                    new1d = (Required1Derived)context.Set<Required1>().Single(e => e.Id == new1d.Id);
                    new1dd = (Required1MoreDerived)context.Set<Required1>().Single(e => e.Id == new1dd.Id);
                    new2a = context.Set<Required2>().Single(e => e.Id == new2a.Id);
                    new2b = context.Set<Required2>().Single(e => e.Id == new2b.Id);
                    new2d = (Required2Derived)context.Set<Required2>().Single(e => e.Id == new2d.Id);
                    new2dd = (Required2MoreDerived)context.Set<Required2>().Single(e => e.Id == new2dd.Id);
                }
                else
                {
                    new1.Parent = null;
                    new1d.Parent = null;
                    new1dd.Parent = null;

                    context.AddRange(new1, new1d, new1dd, new2a, new2d, new2dd, new2b);
                }

                if ((changeMechanism & ChangeMechanism.Principal) != 0)
                {
                    Add(existing.Children, new2a);
                    Add(existing.Children, new2b);
                    Add(new1d.Children, new2d);
                    Add(new1dd.Children, new2dd);
                    Add(root.RequiredChildren, new1);
                    Add(root.RequiredChildren, new1d);
                    Add(root.RequiredChildren, new1dd);
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    new2a.Parent = existing;
                    new2b.Parent = existing;
                    new2d.Parent = new1d;
                    new2dd.Parent = new1dd;
                    new1.Parent = root;
                    new1d.Parent = root;
                    new1dd.Parent = root;
                }

                if ((changeMechanism & ChangeMechanism.Fk) != 0)
                {
                    new2a.ParentId = context.Entry(existing).Property(e => e.Id).CurrentValue;
                    new2b.ParentId = context.Entry(existing).Property(e => e.Id).CurrentValue;
                    new2d.ParentId = context.Entry(new1d).Property(e => e.Id).CurrentValue;
                    new2dd.ParentId = context.Entry(new1dd).Property(e => e.Id).CurrentValue;
                    new1.ParentId = context.Entry(root).Property(e => e.Id).CurrentValue;
                    new1d.ParentId = context.Entry(root).Property(e => e.Id).CurrentValue;
                    new1dd.ParentId = context.Entry(root).Property(e => e.Id).CurrentValue;
                }

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Contains(new2a, existing.Children);
                Assert.Contains(new2b, existing.Children);
                Assert.Contains(new2d, new1d.Children);
                Assert.Contains(new2dd, new1dd.Children);
                Assert.Contains(new1, root.RequiredChildren);
                Assert.Contains(new1d, root.RequiredChildren);
                Assert.Contains(new1dd, root.RequiredChildren);

                Assert.Same(existing, new2a.Parent);
                Assert.Same(existing, new2b.Parent);
                Assert.Same(new1d, new2d.Parent);
                Assert.Same(new1dd, new2dd.Parent);
                Assert.Same(root, existing.Parent);
                Assert.Same(root, new1d.Parent);
                Assert.Same(root, new1dd.Parent);

                Assert.Equal(existing.Id, new2a.ParentId);
                Assert.Equal(existing.Id, new2b.ParentId);
                Assert.Equal(new1d.Id, new2d.ParentId);
                Assert.Equal(new1dd.Id, new2dd.ParentId);
                Assert.Equal(root.Id, existing.ParentId);
                Assert.Equal(root.Id, new1d.ParentId);
                Assert.Equal(root.Id, new1dd.ParentId);
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
    public virtual Task Save_removed_optional_many_to_one_dependents(ChangeMechanism changeMechanism)
    {
        Root root;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.OptionalChildren).Load();
                    context.Entry(root.OptionalChildren.First()).Collection(e => e.Children).Load();
                }

                var childCollection = root.OptionalChildren.First().Children;
                var removed2 = childCollection.First();
                var removed1 = root.OptionalChildren.Skip(1).First();

                if ((changeMechanism & ChangeMechanism.Principal) != 0)
                {
                    Remove(childCollection, removed2);
                    Remove(root.OptionalChildren, removed1);
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    removed2.Parent = null;
                    removed1.Parent = null;
                }

                if ((changeMechanism & ChangeMechanism.Fk) != 0)
                {
                    removed2.ParentId = null;
                    removed1.ParentId = null;
                }

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.DoesNotContain(removed1, root.OptionalChildren);
                Assert.DoesNotContain(removed2, childCollection);

                Assert.Null(removed1.Parent);
                Assert.Null(removed2.Parent);
                Assert.Null(removed1.ParentId);
                Assert.Null(removed2.ParentId);
            }, async context =>
            {
                if ((changeMechanism & ChangeMechanism.Fk) == 0)
                {
                    var loadedRoot = await LoadRootAsync(context);

                    if (!DoesLazyLoading)
                    {
                        context.Entry(loadedRoot).Collection(e => e.OptionalChildren).Load();
                        context.Entry(loadedRoot.OptionalChildren.First()).Collection(e => e.Children).Load();
                    }

                    Assert.Single(loadedRoot.OptionalChildren);
                    Assert.Single(loadedRoot.OptionalChildren.First().Children);
                }
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
    public virtual Task Save_removed_required_many_to_one_dependents(ChangeMechanism changeMechanism)
    {
        var removed1Id = 0;
        var removed2Id = 0;
        List<int> removed1ChildrenIds = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.RequiredChildren).Load();
                    context.Entry(root.RequiredChildren.First()).Collection(e => e.Children).Load();
                }

                var childCollection = root.RequiredChildren.First().Children;
                var removed2 = childCollection.First();
                var removed1 = root.RequiredChildren.Skip(1).First();

                removed1Id = removed1.Id;
                removed2Id = removed2.Id;
                removed1ChildrenIds = removed1.Children.Select(e => e.Id).ToList();

                if ((changeMechanism & ChangeMechanism.Principal) != 0)
                {
                    Remove(childCollection, removed2);
                    Remove(root.RequiredChildren, removed1);
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    removed2.Parent = null;
                    removed1.Parent = null;
                }

                if ((changeMechanism & ChangeMechanism.Fk) != 0)
                {
                    context.Entry(removed2).GetInfrastructure()[context.Entry(removed2).Property(e => e.ParentId).Metadata] = null;
                    context.Entry(removed1).GetInfrastructure()[context.Entry(removed1).Property(e => e.ParentId).Metadata] = null;
                }

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());
            }, async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.RequiredChildren).Load();
                }

                Assert.Single(root.RequiredChildren);
                Assert.DoesNotContain(removed1Id, root.RequiredChildren.Select(e => e.Id));

                Assert.Empty(context.Set<Required1>().Where(e => e.Id == removed1Id));
                Assert.Empty(context.Set<Required2>().Where(e => e.Id == removed2Id));
                Assert.Empty(context.Set<Required2>().Where(e => removed1ChildrenIds.Contains(e.Id)));
            });
    }

    [ConditionalTheory]
    [InlineData((int)ChangeMechanism.Dependent, false)]
    [InlineData((int)ChangeMechanism.Dependent, true)]
    [InlineData((int)ChangeMechanism.Principal, false)]
    [InlineData((int)ChangeMechanism.Principal, true)]
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
    public virtual Task Reparent_to_different_one_to_many(ChangeMechanism changeMechanism, bool useExistingParent)
    {
        var compositeCount = 0;
        OptionalAk1 oldParent = null;
        OptionalComposite2 oldComposite1 = null;
        OptionalComposite2 oldComposite2 = null;
        Optional1 newParent = null;

        return ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                if (!useExistingParent)
                {
                    newParent = context.CreateProxy<Optional1>(
                        e => e.CompositeChildren = new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance));

                    context.Set<Optional1>().Add(newParent);
                    context.SaveChanges();
                }

                return Task.CompletedTask;
            }, async context =>
            {
                var root = await LoadRootAsync(context);

                compositeCount = context.Set<OptionalComposite2>().Count();

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.OptionalChildren).Load();
                    context.Entry(root).Collection(e => e.OptionalChildrenAk).Load();
                }

                oldParent = root.OptionalChildrenAk.OrderBy(e => e.Id).First();

                if (!DoesLazyLoading)
                {
                    context.Entry(oldParent).Collection(e => e.CompositeChildren).Load();
                }

                oldComposite1 = oldParent.CompositeChildren.OrderBy(e => e.Id).First();
                oldComposite2 = oldParent.CompositeChildren.OrderBy(e => e.Id).Last();

                if (useExistingParent)
                {
                    newParent = root.OptionalChildren.OrderBy(e => e.Id).Last();
                }
                else
                {
                    newParent = context.Set<Optional1>().Single(e => e.Id == newParent.Id);
                    newParent.Parent = root;
                }

                if (!DoesLazyLoading)
                {
                    context.Entry(newParent).Collection(e => e.CompositeChildren).Load();
                }

                if ((changeMechanism & ChangeMechanism.Principal) != 0)
                {
                    oldParent.CompositeChildren.Remove(oldComposite1);
                    newParent.CompositeChildren.Add(oldComposite1);
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    oldComposite1.Parent = null;
                    oldComposite1.Parent2 = newParent;
                }

                if ((changeMechanism & ChangeMechanism.Fk) != 0)
                {
                    oldComposite1.ParentId = null;
                    oldComposite1.Parent2Id = newParent.Id;
                }

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Same(oldComposite2, oldParent.CompositeChildren.Single());
                Assert.Same(oldParent, oldComposite2.Parent);
                Assert.Equal(oldParent.Id, oldComposite2.ParentId);
                Assert.Null(oldComposite2.Parent2);
                Assert.Null(oldComposite2.Parent2Id);

                Assert.Same(oldComposite1, newParent.CompositeChildren.Single());
                Assert.Same(newParent, oldComposite1.Parent2);
                Assert.Equal(newParent.Id, oldComposite1.Parent2Id);
                Assert.Null(oldComposite1.Parent);
                Assert.Null(oldComposite1.ParentId);

                Assert.Equal(compositeCount, context.Set<OptionalComposite2>().Count());
            }, async context =>
            {
                if ((changeMechanism & ChangeMechanism.Fk) == 0)
                {
                    var loadedRoot = await LoadRootAsync(context);

                    oldParent = context.Set<OptionalAk1>().Single(e => e.Id == oldParent.Id);
                    newParent = context.Set<Optional1>().Single(e => e.Id == newParent.Id);

                    oldComposite1 = context.Set<OptionalComposite2>().Single(e => e.Id == oldComposite1.Id);
                    oldComposite2 = context.Set<OptionalComposite2>().Single(e => e.Id == oldComposite2.Id);

                    Assert.Same(oldComposite2, oldParent.CompositeChildren.Single());
                    Assert.Same(oldParent, oldComposite2.Parent);
                    Assert.Equal(oldParent.Id, oldComposite2.ParentId);
                    Assert.Null(oldComposite2.Parent2);
                    Assert.Null(oldComposite2.Parent2Id);

                    Assert.Same(oldComposite1, newParent.CompositeChildren.Single());
                    Assert.Same(newParent, oldComposite1.Parent2);
                    Assert.Equal(newParent.Id, oldComposite1.Parent2Id);
                    Assert.Null(oldComposite1.Parent);
                    Assert.Null(oldComposite1.ParentId);

                    Assert.Equal(compositeCount, context.Set<OptionalComposite2>().Count());
                }
            });
    }

    [ConditionalTheory]
    [InlineData((int)ChangeMechanism.Dependent, false)]
    [InlineData((int)ChangeMechanism.Dependent, true)]
    [InlineData((int)ChangeMechanism.Principal, false)]
    [InlineData((int)ChangeMechanism.Principal, true)]
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
    public virtual Task Reparent_one_to_many_overlapping(ChangeMechanism changeMechanism, bool useExistingParent)
    {
        Root root = null;
        var childCount = 0;
        RequiredComposite1 oldParent = null;
        OptionalOverlapping2 oldChild1 = null;
        OptionalOverlapping2 oldChild2 = null;
        RequiredComposite1 newParent = null;

        return ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                if (!useExistingParent)
                {
                    newParent = context.CreateProxy<RequiredComposite1>(
                        e =>
                        {
                            e.Id = 3;
                            e.Parent = context.Set<Root>().Single(IsTheRoot);
                            e.CompositeChildren = new ObservableHashSet<OptionalOverlapping2>(ReferenceEqualityComparer.Instance)
                            {
                                context.CreateProxy<OptionalOverlapping2>(e => e.Id = 5),
                                context.CreateProxy<OptionalOverlapping2>(e => e.Id = 6)
                            };
                        });

                    context.Set<RequiredComposite1>().Add(newParent);
                    context.SaveChanges();
                }

                return Task.CompletedTask;
            }, async context =>
            {
                root = await LoadRootAsync(context);

                childCount = context.Set<OptionalOverlapping2>().Count();

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.RequiredCompositeChildren).Load();
                }

                oldParent = root.RequiredCompositeChildren.OrderBy(e => e.Id).First();

                if (!DoesLazyLoading)
                {
                    context.Entry(oldParent).Collection(e => e.CompositeChildren).Load();
                }

                oldChild1 = oldParent.CompositeChildren.OrderBy(e => e.Id).First();
                oldChild2 = oldParent.CompositeChildren.OrderBy(e => e.Id).Last();

                Assert.Equal(useExistingParent ? 2 : 3, root.RequiredCompositeChildren.Count());

                if (useExistingParent)
                {
                    newParent = root.RequiredCompositeChildren.OrderBy(e => e.Id).Last();
                }
                else
                {
                    newParent = context.Set<RequiredComposite1>().Single(e => e.Id == newParent.Id);
                    newParent.Parent = root;
                }

                if (!DoesLazyLoading)
                {
                    context.Entry(newParent).Collection(e => e.CompositeChildren).Load();
                }

                if ((changeMechanism & ChangeMechanism.Principal) != 0)
                {
                    oldParent.CompositeChildren.Remove(oldChild1);
                    newParent.CompositeChildren.Add(oldChild1);
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    oldChild1.Parent = newParent;
                }

                if ((changeMechanism & ChangeMechanism.Fk) != 0)
                {
                    oldChild1.ParentId = newParent.Id;
                }

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Same(oldChild2, oldParent.CompositeChildren.Single());
                Assert.Same(oldParent, oldChild2.Parent);
                Assert.Equal(oldParent.Id, oldChild2.ParentId);
                Assert.Equal(oldParent.ParentAlternateId, oldChild2.ParentAlternateId);
                Assert.Equal(root.AlternateId, oldChild2.ParentAlternateId);
                Assert.Same(root, oldChild2.Root);

                Assert.Equal(3, newParent.CompositeChildren.Count);
                Assert.Same(oldChild1, newParent.CompositeChildren.Single(e => e.Id == oldChild1.Id));
                Assert.Same(newParent, oldChild1.Parent);
                Assert.Equal(newParent.Id, oldChild1.ParentId);
                Assert.Equal(oldParent.ParentAlternateId, oldChild1.ParentAlternateId);
                Assert.Equal(root.AlternateId, oldChild1.ParentAlternateId);
                Assert.Same(root, oldChild1.Root);

                Assert.Equal(childCount, context.Set<OptionalOverlapping2>().Count());
            }, async context =>
            {
                var loadedRoot = await LoadRootAsync(context);

                oldParent = context.Set<RequiredComposite1>().Single(e => e.Id == oldParent.Id);
                newParent = context.Set<RequiredComposite1>().Single(e => e.Id == newParent.Id);

                oldChild1 = context.Set<OptionalOverlapping2>().Single(e => e.Id == oldChild1.Id);
                oldChild2 = context.Set<OptionalOverlapping2>().Single(e => e.Id == oldChild2.Id);

                if (!DoesLazyLoading)
                {
                    context.Entry(oldParent).Collection(e => e.CompositeChildren).Load();
                    context.Entry(newParent).Collection(e => e.CompositeChildren).Load();
                }

                Assert.Same(oldChild2, oldParent.CompositeChildren.Single());
                Assert.Same(oldParent, oldChild2.Parent);
                Assert.Equal(oldParent.Id, oldChild2.ParentId);
                Assert.Equal(oldParent.ParentAlternateId, oldChild2.ParentAlternateId);
                Assert.Equal(root.AlternateId, oldChild2.ParentAlternateId);

                Assert.Same(oldChild1, newParent.CompositeChildren.Single(e => e.Id == oldChild1.Id));
                Assert.Same(newParent, oldChild1.Parent);
                Assert.Equal(newParent.Id, oldChild1.ParentId);
                Assert.Equal(oldParent.ParentAlternateId, oldChild1.ParentAlternateId);
                Assert.Equal(root.AlternateId, oldChild1.ParentAlternateId);

                Assert.Equal(childCount, context.Set<OptionalOverlapping2>().Count());
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
    public virtual Task Required_many_to_one_dependents_are_cascade_deleted(
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
                    context.Entry(root).Collection(e => e.RequiredChildren).Load();
                }

                Assert.Equal(2, root.RequiredChildren.Count());

                var removed = root.RequiredChildren.First();

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Collection(e => e.Children).Load();
                }

                removedId = removed.Id;
                var cascadeRemoved = removed.Children.ToList();
                orphanedIds = cascadeRemoved.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);

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

                    Assert.Single(root.RequiredChildren);
                    Assert.DoesNotContain(removedId, root.RequiredChildren.Select(e => e.Id));

                    Assert.Empty(context.Set<Required1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<Required2>().Where(e => orphanedIds.Contains(e.Id)));

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
                        context.Entry(root).Collection(e => e.RequiredChildren).Load();
                    }

                    Assert.Single(root.RequiredChildren);
                    Assert.DoesNotContain(removedId, root.RequiredChildren.Select(e => e.Id));

                    Assert.Empty(context.Set<Required1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<Required2>().Where(e => orphanedIds.Contains(e.Id)));
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
    public virtual Task Optional_many_to_one_dependents_are_orphaned(
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
                    context.Entry(root).Collection(e => e.OptionalChildren).Load();
                }

                Assert.Equal(2, root.OptionalChildren.Count());

                var removed = root.OptionalChildren.First();

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

                var expectedState = (cascadeDeleteTiming == CascadeTiming.Immediate)
                    ? EntityState.Modified
                    : EntityState.Unchanged;

                foreach (var orphanEntry in orphaned.Select(context.Entry))
                {
                    Assert.Equal(expectedState, orphanEntry.State);
                    if (expectedState == EntityState.Unchanged)
                    {
                        Assert.Equal(removed.Id, orphanEntry.Entity.ParentId);
                        Assert.Equal(
                            context.Entry(removed).Property(e => e.Id).CurrentValue,
                            orphanEntry.Property(e => e.ParentId).CurrentValue);
                    }
                    else
                    {
                        Assert.Null(orphanEntry.Entity.ParentId);
                        Assert.Null(orphanEntry.Property(e => e.ParentId).CurrentValue);
                    }
                }

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));

                Assert.Single(root.OptionalChildren);
                Assert.DoesNotContain(removedId, root.OptionalChildren.Select(e => e.Id));

                Assert.Empty(context.Set<Optional1>().Where(e => e.Id == removedId));
                Assert.Equal(orphanedIds.Count, context.Set<Optional2>().Count(e => orphanedIds.Contains(e.Id)));

                Assert.Same(root, removed.Parent);
                Assert.Equal(2, removed.Children.Count());
            }, async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.OptionalChildren).Load();
                }

                Assert.Single(root.OptionalChildren);
                Assert.DoesNotContain(removedId, root.OptionalChildren.Select(e => e.Id));

                Assert.Empty(context.Set<Optional1>().Where(e => e.Id == removedId));
                Assert.Equal(orphanedIds.Count, context.Set<Optional2>().Count(e => orphanedIds.Contains(e.Id)));
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
    [InlineData(null, null)]
    public virtual Task Optional_many_to_one_dependents_are_orphaned_with_Added_graph(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming) // Issue #29318
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = context.CreateProxy<Root>(e => e.AlternateId = Guid.NewGuid());
                var removed = context.CreateProxy<Optional1>(e => e.Parent = root);
                var orphaned = new List<Optional2>
                {
                    context.CreateProxy<Optional2>(e => e.Parent = removed), context.CreateProxy<Optional2>(e => e.Parent = removed)
                };

                context.AddRange(orphaned);
                var removedId = context.Entry(removed).Property(e => e.Id).CurrentValue;
                context.Remove(removed);

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                if (cascadeDeleteTiming == null)
                {
                    Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Added));

                    context.ChangeTracker.CascadeChanges();
                }

                foreach (var orphanEntry in orphaned.Select(context.Entry))
                {
                    Assert.Equal(EntityState.Added, orphanEntry.State);
                    Assert.Null(orphanEntry.Entity.ParentId);
                    Assert.Null(orphanEntry.Property(e => e.ParentId).CurrentValue);
                }

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));

                Assert.Empty(root.OptionalChildren);
                Assert.Same(root, removed.Parent);
                Assert.Equal(2, removed.Children.Count());
                return Task.CompletedTask;
            });

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
    public virtual Task Required_many_to_one_dependents_are_cascade_deleted_in_store(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.RequiredChildren).Load();
                }

                var removed = root.RequiredChildren.First();

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Collection(e => e.Children).Load();
                }

                removedId = removed.Id;
                orphanedIds = removed.Children.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
            },
            context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

                var root = context.Set<Root>().Include(e => e.RequiredChildren).Single(IsTheRoot);

                var removed = root.RequiredChildren.Single(e => e.Id == removedId);

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Single(root.RequiredChildren);
                Assert.DoesNotContain(removedId, root.RequiredChildren.Select(e => e.Id));

                Assert.Empty(context.Set<Required1>().Where(e => e.Id == removedId));
                Assert.Empty(context.Set<Required2>().Where(e => orphanedIds.Contains(e.Id)));

                Assert.Same(root, removed.Parent);
                Assert.Empty(removed.Children);
                return Task.CompletedTask;
            }, async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.RequiredChildren).Load();
                }

                Assert.Single(root.RequiredChildren);
                Assert.DoesNotContain(removedId, root.RequiredChildren.Select(e => e.Id));

                Assert.Empty(context.Set<Required1>().Where(e => e.Id == removedId));
                Assert.Empty(context.Set<Required2>().Where(e => orphanedIds.Contains(e.Id)));
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
    public virtual Task Optional_many_to_one_dependents_are_orphaned_in_store(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.OptionalChildren).Load();
                }

                var removed = root.OptionalChildren.First();

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Collection(e => e.Children).Load();
                }

                removedId = removed.Id;
                orphanedIds = removed.Children.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
            },
            context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

                var root = context.Set<Root>().Include(e => e.OptionalChildren).Single(IsTheRoot);

                var removed = root.OptionalChildren.First(e => e.Id == removedId);

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Single(root.OptionalChildren);
                Assert.DoesNotContain(removedId, root.OptionalChildren.Select(e => e.Id));

                Assert.Empty(context.Set<Optional1>().Where(e => e.Id == removedId));

                var orphaned = context.Set<Optional2>().Where(e => orphanedIds.Contains(e.Id)).ToList();
                Assert.Equal(orphanedIds.Count, orphaned.Count);
                Assert.True(orphaned.All(e => e.ParentId == null));

                Assert.Same(root, removed.Parent);
                Assert.Empty(removed.Children); // Never loaded
                return Task.CompletedTask;
            }, async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.OptionalChildren).Load();
                }

                Assert.Single(root.OptionalChildren);
                Assert.DoesNotContain(removedId, root.OptionalChildren.Select(e => e.Id));

                Assert.Empty(context.Set<Optional1>().Where(e => e.Id == removedId));

                var orphaned = context.Set<Optional2>().Where(e => orphanedIds.Contains(e.Id)).ToList();
                Assert.Equal(orphanedIds.Count, orphaned.Count);
                Assert.True(orphaned.All(e => e.ParentId == null));
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
    public virtual Task Required_many_to_one_dependents_are_cascade_deleted_starting_detached(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;
        Root root = null;
        Required1 removed = null;
        List<Required2> cascadeRemoved = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.RequiredChildren).Load();
                }

                removed = root.RequiredChildren.First();

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Collection(e => e.Children).Load();
                }

                cascadeRemoved = removed.Children.ToList();

                Assert.Equal(2, root.RequiredChildren.Count());
            },
            context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

                removedId = removed.Id;
                orphanedIds = cascadeRemoved.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);

                var expectedState = cascadeDeleteTiming == CascadeTiming.Immediate
                    ? EntityState.Deleted
                    : EntityState.Unchanged;

                Assert.True(cascadeRemoved.All(e => context.Entry(e).State == expectedState));

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
                        context.Entry(root).Collection(e => e.RequiredChildren).Load();
                    }

                    Assert.Single(root.RequiredChildren);
                    Assert.DoesNotContain(removedId, root.RequiredChildren.Select(e => e.Id));

                    Assert.Empty(context.Set<Required1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<Required2>().Where(e => orphanedIds.Contains(e.Id)));
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
    public virtual Task Optional_many_to_one_dependents_are_orphaned_starting_detached(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;
        Root root = null;
        Optional1 removed = null;
        List<Optional2> orphaned = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.OptionalChildren).Load();
                }

                removed = root.OptionalChildren.First();

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Collection(e => e.Children).Load();
                }

                orphaned = removed.Children.ToList();

                Assert.Equal(2, root.OptionalChildren.Count());
            },
            context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

                removedId = removed.Id;
                orphanedIds = orphaned.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);

                var expectedState = (cascadeDeleteTiming == CascadeTiming.Immediate)
                    ? EntityState.Modified
                    : EntityState.Unchanged;

                foreach (var orphanEntry in orphaned.Select(context.Entry))
                {
                    Assert.Equal(expectedState, orphanEntry.State);
                    if (expectedState == EntityState.Unchanged)
                    {
                        Assert.Equal(removed.Id, orphanEntry.Entity.ParentId);
                        Assert.Equal(
                            context.Entry(removed).Property(e => e.Id).CurrentValue,
                            orphanEntry.Property(e => e.ParentId).CurrentValue);
                    }
                    else
                    {
                        Assert.Null(orphanEntry.Entity.ParentId);
                        Assert.Null(orphanEntry.Property(e => e.ParentId).CurrentValue);
                    }
                }

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));

                Assert.Same(root, removed.Parent);
                Assert.Equal(2, removed.Children.Count());
                return Task.CompletedTask;
            }, async context =>
            {
                root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.OptionalChildren).Load();
                }

                Assert.Single(root.OptionalChildren);
                Assert.DoesNotContain(removedId, root.OptionalChildren.Select(e => e.Id));

                Assert.Empty(context.Set<Optional1>().Where(e => e.Id == removedId));
                Assert.Equal(orphanedIds.Count, context.Set<Optional2>().Count(e => orphanedIds.Contains(e.Id)));
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
    public virtual Task Required_many_to_one_dependents_are_cascade_detached_when_Added(
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
                    context.Entry(root).Collection(e => e.RequiredChildren).Load();
                }

                Assert.Equal(2, root.RequiredChildren.Count());

                var removed = root.RequiredChildren.First();

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Collection(e => e.Children).Load();
                }

                removedId = removed.Id;
                var cascadeRemoved = removed.Children.ToList();
                orphanedIds = cascadeRemoved.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);

                var added = context.CreateProxy<Required2>();
                Add(removed.Children, added);

                if (context.ChangeTracker.AutoDetectChangesEnabled
                    && !DoesChangeTracking)
                {
                    context.ChangeTracker.DetectChanges();
                }

                Assert.Equal(EntityState.Unchanged, context.Entry(removed).State);

                Assert.Equal(EntityState.Added, context.Entry(added).State);
                Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Unchanged));

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);

                if (cascadeDeleteTiming == CascadeTiming.Immediate)
                {
                    Assert.Equal(EntityState.Detached, context.Entry(added).State);
                    Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Deleted));
                }
                else
                {
                    Assert.Equal(EntityState.Added, context.Entry(added).State);
                    Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Unchanged));
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
                    Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Detached));

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
                        context.Entry(root).Collection(e => e.RequiredChildren).Load();
                    }

                    Assert.Single(root.RequiredChildren);
                    Assert.DoesNotContain(removedId, root.RequiredChildren.Select(e => e.Id));

                    Assert.Empty(context.Set<Required1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<Required2>().Where(e => orphanedIds.Contains(e.Id)));
                }
            });
    }
}
