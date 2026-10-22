// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming
// ReSharper disable AccessToModifiedClosure
// ReSharper disable PossibleMultipleEnumeration

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract partial class GraphUpdatesTestBase<TFixture>
    where TFixture : GraphUpdatesTestBase<TFixture>.GraphUpdatesFixtureBase, new()
{
    [ConditionalTheory]
    [InlineData((int)ChangeMechanism.Principal, false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Principal, true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Dependent, false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Dependent, true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Fk, false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Fk, true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), true, CascadeTiming.OnSaveChanges)]
    [InlineData(
        (int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), false, CascadeTiming.OnSaveChanges)]
    [InlineData(
        (int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Principal, false, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Principal, true, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Dependent, false, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Dependent, true, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Fk, false, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Fk, true, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), false, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), true, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), false, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), true, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), false, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), true, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Principal, false, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Principal, true, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Dependent, false, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Dependent, true, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Fk, false, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Fk, true, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), false, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), true, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), false, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), true, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), false, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), true, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Principal, false, null)]
    [InlineData((int)ChangeMechanism.Principal, true, null)]
    [InlineData((int)ChangeMechanism.Dependent, false, null)]
    [InlineData((int)ChangeMechanism.Dependent, true, null)]
    [InlineData((int)ChangeMechanism.Fk, false, null)]
    [InlineData((int)ChangeMechanism.Fk, true, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), false, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), true, null)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), false, null)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), true, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), false, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), true, null)]
    public virtual Task Save_optional_many_to_one_dependents(
        ChangeMechanism changeMechanism,
        bool useExistingEntities,
        CascadeTiming? deleteOrphansTiming)
    {
        var new1 = new Optional1();
        var new1d = new Optional1Derived();
        var new1dd = new Optional1MoreDerived();
        var new2a = new Optional2();
        var new2b = new Optional2();
        var new2d = new Optional2Derived();
        var new2dd = new Optional2MoreDerived();
        Root root = null;
        IReadOnlyList<EntityEntry> entries = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                if (useExistingEntities)
                {
                    context.AddRange(new1, new1d, new1dd, new2a, new2d, new2dd, new2b);
                    await context.SaveChangesAsync();
                }
            }, async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                root = await LoadOptionalGraphAsync(context);
                var existing = root.OptionalChildren.OrderBy(e => e.Id).First();

                if (useExistingEntities)
                {
                    new1 = await context.Set<Optional1>().SingleAsync(e => e.Id == new1.Id);
                    new1d = (Optional1Derived)await context.Set<Optional1>().SingleAsync(e => e.Id == new1d.Id);
                    new1dd = (Optional1MoreDerived)await context.Set<Optional1>().SingleAsync(e => e.Id == new1dd.Id);
                    new2a = await context.Set<Optional2>().SingleAsync(e => e.Id == new2a.Id);
                    new2b = await context.Set<Optional2>().SingleAsync(e => e.Id == new2b.Id);
                    new2d = (Optional2Derived)await context.Set<Optional2>().SingleAsync(e => e.Id == new2d.Id);
                    new2dd = (Optional2MoreDerived)await context.Set<Optional2>().SingleAsync(e => e.Id == new2dd.Id);
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

                await context.SaveChangesAsync();

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

                entries = context.ChangeTracker.Entries().ToList();
            }, async context =>
            {
                var loadedRoot = await LoadOptionalGraphAsync(context);

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);
            });
    }

    [ConditionalTheory]
    [InlineData((int)ChangeMechanism.Principal, false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Principal, true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Dependent, false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Dependent, true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Fk, false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Fk, true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), true, CascadeTiming.OnSaveChanges)]
    [InlineData(
        (int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), false, CascadeTiming.OnSaveChanges)]
    [InlineData(
        (int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Principal, false, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Principal, true, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Dependent, false, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Dependent, true, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Fk, false, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Fk, true, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), false, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), true, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), false, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), true, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), false, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), true, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Principal, false, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Principal, true, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Dependent, false, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Dependent, true, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Fk, false, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Fk, true, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), false, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), true, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), false, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), true, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), false, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), true, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Principal, false, null)]
    [InlineData((int)ChangeMechanism.Principal, true, null)]
    [InlineData((int)ChangeMechanism.Dependent, false, null)]
    [InlineData((int)ChangeMechanism.Dependent, true, null)]
    [InlineData((int)ChangeMechanism.Fk, false, null)]
    [InlineData((int)ChangeMechanism.Fk, true, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), false, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), true, null)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), false, null)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), true, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), false, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), true, null)]
    public virtual Task Save_required_many_to_one_dependents(
        ChangeMechanism changeMechanism,
        bool useExistingEntities,
        CascadeTiming? deleteOrphansTiming)
    {
        var newRoot = new Root();
        var new1 = new Required1 { Parent = newRoot };
        var new1d = new Required1Derived { Parent = newRoot };
        var new1dd = new Required1MoreDerived { Parent = newRoot };
        var new2a = new Required2 { Parent = new1 };
        var new2b = new Required2 { Parent = new1 };
        var new2d = new Required2Derived { Parent = new1 };
        var new2dd = new Required2MoreDerived { Parent = new1 };
        Root root = null;
        IReadOnlyList<EntityEntry> entries = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                if (useExistingEntities)
                {
                    context.AddRange(newRoot, new1, new1d, new1dd, new2a, new2d, new2dd, new2b);
                    await context.SaveChangesAsync();
                }
            }, async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                root = await LoadRequiredGraphAsync(context);
                var existing = root.RequiredChildren.OrderBy(e => e.Id).First();

                if (useExistingEntities)
                {
                    new1 = await context.Set<Required1>().SingleAsync(e => e.Id == new1.Id);
                    new1d = (Required1Derived)await context.Set<Required1>().SingleAsync(e => e.Id == new1d.Id);
                    new1dd = (Required1MoreDerived)await context.Set<Required1>().SingleAsync(e => e.Id == new1dd.Id);
                    new2a = await context.Set<Required2>().SingleAsync(e => e.Id == new2a.Id);
                    new2b = await context.Set<Required2>().SingleAsync(e => e.Id == new2b.Id);
                    new2d = (Required2Derived)await context.Set<Required2>().SingleAsync(e => e.Id == new2d.Id);
                    new2dd = (Required2MoreDerived)await context.Set<Required2>().SingleAsync(e => e.Id == new2dd.Id);
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

                await context.SaveChangesAsync();

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

                entries = context.ChangeTracker.Entries().ToList();
            }, async context =>
            {
                var loadedRoot = await LoadRequiredGraphAsync(context);

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);
            });
    }

    [ConditionalTheory]
    [InlineData((int)ChangeMechanism.Principal, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Dependent, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Fk, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Principal, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Dependent, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Fk, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Principal, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Dependent, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Fk, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Principal, null)]
    [InlineData((int)ChangeMechanism.Dependent, null)]
    [InlineData((int)ChangeMechanism.Fk, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), null)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), null)]
    public virtual Task Save_removed_optional_many_to_one_dependents(
        ChangeMechanism changeMechanism,
        CascadeTiming? deleteOrphansTiming)
    {
        Root root = null;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                root = await LoadOptionalGraphAsync(context);

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

                await context.SaveChangesAsync();

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
                    var loadedRoot = await LoadOptionalGraphAsync(context);

                    AssertKeys(root, loadedRoot);
                    AssertNavigations(loadedRoot);

                    Assert.Single(loadedRoot.OptionalChildren);
                    Assert.Single(loadedRoot.OptionalChildren.First().Children);
                }
            });
    }

    [ConditionalTheory]
    [InlineData((int)ChangeMechanism.Principal, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Dependent, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Fk, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Principal, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Dependent, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Fk, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Principal, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Dependent, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Fk, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Principal, null)]
    [InlineData((int)ChangeMechanism.Dependent, null)]
    [InlineData((int)ChangeMechanism.Fk, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), null)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), null)]
    public virtual Task Save_removed_required_many_to_one_dependents(
        ChangeMechanism changeMechanism,
        CascadeTiming? deleteOrphansTiming)
    {
        var removed1Id = 0;
        var removed2Id = 0;
        List<int> removed1ChildrenIds = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await LoadRequiredGraphAsync(context);

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

                if (Fixture.ForceClientNoAction
                    || deleteOrphansTiming == CascadeTiming.Never)
                {
                    Func<Task> testCode;

                    if ((changeMechanism & ChangeMechanism.Fk) != 0
                        && deleteOrphansTiming == CascadeTiming.Immediate)
                    {
                        testCode = () =>
                        {
                            context.Entry(removed2).GetInfrastructure()
                                [context.Entry(removed2).Property(e => e.ParentId).Metadata] = null;
                            return Task.CompletedTask;
                        };
                    }
                    else
                    {
                        if ((changeMechanism & ChangeMechanism.Fk) != 0)
                        {
                            context.Entry(removed2).GetInfrastructure()
                                [context.Entry(removed2).Property(e => e.ParentId).Metadata] = null;
                            context.Entry(removed1).GetInfrastructure()
                                [context.Entry(removed1).Property(e => e.ParentId).Metadata] = null;
                        }

                        testCode = deleteOrphansTiming == CascadeTiming.Immediate
                            ? () =>
                            {
                                context.ChangeTracker.DetectChanges();
                                return Task.CompletedTask;
                            }
                            : deleteOrphansTiming == null
                                ? () =>
                                {
                                    context.ChangeTracker.CascadeChanges();
                                    return Task.CompletedTask;
                                }
                                : () => context.SaveChangesAsync();
                    }

                    var message = (await Assert.ThrowsAsync<InvalidOperationException>(testCode)).Message;

                    Assert.True(
                        message
                        == CoreStrings.RelationshipConceptualNullSensitive(
                            nameof(Root), nameof(Required1), "{ParentId: " + removed1.ParentId + "}")
                        || message
                        == CoreStrings.RelationshipConceptualNullSensitive(
                            nameof(Required1), nameof(Required2), "{ParentId: " + removed2.ParentId + "}"));
                }
                else
                {
                    if ((changeMechanism & ChangeMechanism.Fk) != 0)
                    {
                        context.Entry(removed2).GetInfrastructure()[context.Entry(removed2).Property(e => e.ParentId).Metadata] =
                            null;
                        context.Entry(removed1).GetInfrastructure()[context.Entry(removed1).Property(e => e.ParentId).Metadata] =
                            null;
                    }

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (deleteOrphansTiming == null)
                    {
                        context.ChangeTracker.CascadeChanges();
                    }

                    await context.SaveChangesAsync();

                    Assert.False(context.ChangeTracker.HasChanges());
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction
                    && !Fixture.NoStoreCascades
                    && deleteOrphansTiming != CascadeTiming.Never)
                {
                    var root = await LoadRequiredGraphAsync(context);

                    AssertNavigations(root);

                    Assert.Single(root.RequiredChildren);
                    Assert.DoesNotContain(removed1Id, root.RequiredChildren.Select(e => e.Id));

                    Assert.Empty(context.Set<Required1>().Where(e => e.Id == removed1Id));
                    Assert.Empty(context.Set<Required2>().Where(e => e.Id == removed2Id));
                    Assert.Empty(context.Set<Required2>().Where(e => removed1ChildrenIds.Contains(e.Id)));
                }
            });
    }

    [ConditionalTheory]
    [InlineData((int)ChangeMechanism.Principal, false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Principal, true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Dependent, false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Dependent, true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Fk, false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Fk, true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), true, CascadeTiming.OnSaveChanges)]
    [InlineData(
        (int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), false, CascadeTiming.OnSaveChanges)]
    [InlineData(
        (int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Principal, false, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Principal, true, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Dependent, false, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Dependent, true, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Fk, false, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Fk, true, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), false, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), true, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), false, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), true, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), false, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), true, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Principal, false, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Principal, true, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Dependent, false, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Dependent, true, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Fk, false, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Fk, true, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), false, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), true, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), false, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), true, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), false, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), true, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Principal, false, null)]
    [InlineData((int)ChangeMechanism.Principal, true, null)]
    [InlineData((int)ChangeMechanism.Dependent, false, null)]
    [InlineData((int)ChangeMechanism.Dependent, true, null)]
    [InlineData((int)ChangeMechanism.Fk, false, null)]
    [InlineData((int)ChangeMechanism.Fk, true, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), false, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), true, null)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), false, null)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), true, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), false, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), true, null)]
    public virtual Task Reparent_to_different_one_to_many(
        ChangeMechanism changeMechanism,
        bool useExistingParent,
        CascadeTiming? deleteOrphansTiming)
    {
        Root root = null;
        IReadOnlyList<EntityEntry> entries = null;
        var compositeCount = 0;
        OptionalAk1 oldParent = null;
        OptionalComposite2 oldComposite1 = null;
        OptionalComposite2 oldComposite2 = null;
        Optional1 newParent = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                if (!useExistingParent)
                {
                    newParent = new Optional1
                    {
                        CompositeChildren = new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance)
                    };

                    context.Set<Optional1>().Add(newParent);
                    await context.SaveChangesAsync();
                }
            }, async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                root = await LoadOptionalOneToManyGraphAsync(context);

                compositeCount = context.Set<OptionalComposite2>().Count();

                oldParent = root.OptionalChildrenAk.OrderBy(e => e.Id).First();

                oldComposite1 = oldParent.CompositeChildren.OrderBy(e => e.Id).First();
                oldComposite2 = oldParent.CompositeChildren.OrderBy(e => e.Id).Last();

                if (useExistingParent)
                {
                    newParent = root.OptionalChildren.OrderBy(e => e.Id).Last();
                }
                else
                {
                    newParent = await context.Set<Optional1>().SingleAsync(e => e.Id == newParent.Id);
                    newParent.Parent = root;
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

                await context.SaveChangesAsync();

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

                entries = context.ChangeTracker.Entries().ToList();

                Assert.Equal(compositeCount, context.Set<OptionalComposite2>().Count());
            }, async context =>
            {
                if ((changeMechanism & ChangeMechanism.Fk) == 0)
                {
                    var loadedRoot = await LoadOptionalOneToManyGraphAsync(context);

                    Assert.False(context.ChangeTracker.HasChanges());

                    AssertKeys(root, loadedRoot);
                    AssertNavigations(loadedRoot);

                    oldParent = await context.Set<OptionalAk1>().SingleAsync(e => e.Id == oldParent.Id);
                    newParent = await context.Set<Optional1>().SingleAsync(e => e.Id == newParent.Id);

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

                    AssertEntries(entries, context.ChangeTracker.Entries().ToList());

                    Assert.Equal(compositeCount, context.Set<OptionalComposite2>().Count());
                }
            });
    }

    [ConditionalTheory]
    [InlineData((int)ChangeMechanism.Principal, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Dependent, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Fk, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Principal, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Dependent, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Fk, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Principal, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Dependent, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Fk, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Principal, null)]
    [InlineData((int)ChangeMechanism.Dependent, null)]
    [InlineData((int)ChangeMechanism.Fk, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), null)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), null)]
    public virtual Task Reparent_dependent_one_to_many(
        ChangeMechanism changeMechanism,
        CascadeTiming? deleteOrphansTiming)
    {
        Required1 oldParent = null;
        Required1 newParent = null;
        Required2 child = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await LoadRequiredGraphAsync(context);

                oldParent = root.RequiredChildren.OrderBy(e => e.Id).First();
                newParent = root.RequiredChildren.OrderBy(e => e.Id).Last();
                child = oldParent.Children.OrderBy(e => e.Id).Skip(1).First();

                if ((changeMechanism & ChangeMechanism.Principal) != 0)
                {
                    ((ICollection<Required2>)oldParent.Children).Remove(child);
                    ((ICollection<Required2>)newParent.Children).Add(child);
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    child.Parent = newParent;
                }

                if ((changeMechanism & ChangeMechanism.Fk) != 0)
                {
                    child.ParentId = newParent.Id;
                }

                if (!Fixture.ForceClientNoAction
                    || deleteOrphansTiming != CascadeTiming.Immediate
                    || (changeMechanism & ChangeMechanism.Fk) != 0
                    || changeMechanism == ChangeMechanism.Dependent)
                {
                    Assert.True(context.ChangeTracker.HasChanges());

                    Assert.DoesNotContain(child, oldParent.Children);
                    Assert.Contains(child, newParent.Children);
                    Assert.Equal(newParent.Id, child.ParentId);
                    Assert.Equal(EntityState.Modified, context.Entry(child).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(oldParent).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(newParent).State);

                    await context.SaveChangesAsync();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.DoesNotContain(child, oldParent.Children);
                    Assert.Contains(child, newParent.Children);
                    Assert.Equal(newParent.Id, child.ParentId);
                    Assert.Equal(EntityState.Unchanged, context.Entry(child).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(oldParent).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(newParent).State);
                }
                else
                {
                    Assert.Throws<InvalidOperationException>(() => context.ChangeTracker.DetectChanges());
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction
                    || deleteOrphansTiming != CascadeTiming.Immediate
                    || (changeMechanism & ChangeMechanism.Fk) != 0
                    || changeMechanism == ChangeMechanism.Dependent)
                {
                    var root = await LoadRequiredGraphAsync(context);

                    Assert.False(context.ChangeTracker.HasChanges());

                    oldParent = root.RequiredChildren.First(e => e.Id == oldParent.Id);
                    newParent = root.RequiredChildren.First(e => e.Id == newParent.Id);
                    child = newParent.Children.First(e => e.Id == child.Id);

                    Assert.DoesNotContain(child, oldParent.Children);
                    Assert.Contains(child, newParent.Children);
                    Assert.Equal(newParent.Id, child.ParentId);
                    Assert.Equal(EntityState.Unchanged, context.Entry(child).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(oldParent).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(newParent).State);
                }
            });
    }

    [ConditionalTheory]
    [InlineData((int)ChangeMechanism.Principal, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Dependent, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Fk, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Principal, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Dependent, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Fk, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Principal, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Dependent, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Fk, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Principal, null)]
    [InlineData((int)ChangeMechanism.Dependent, null)]
    [InlineData((int)ChangeMechanism.Fk, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), null)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), null)]
    public virtual Task Reparent_dependent_one_to_many_ak(
        ChangeMechanism changeMechanism,
        CascadeTiming? deleteOrphansTiming)
    {
        RequiredAk1 oldParent = null;
        RequiredAk1 newParent = null;
        RequiredAk2 child = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await LoadRequiredAkGraphAsync(context);

                oldParent = root.RequiredChildrenAk.OrderBy(e => e.Id).First();
                newParent = root.RequiredChildrenAk.OrderBy(e => e.Id).Last();
                child = oldParent.Children.OrderBy(e => e.Id).Skip(1).First();

                if ((changeMechanism & ChangeMechanism.Principal) != 0)
                {
                    ((ICollection<RequiredAk2>)oldParent.Children).Remove(child);
                    ((ICollection<RequiredAk2>)newParent.Children).Add(child);
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    child.Parent = newParent;
                }

                if ((changeMechanism & ChangeMechanism.Fk) != 0)
                {
                    child.ParentId = newParent.AlternateId;
                }

                if (!Fixture.ForceClientNoAction
                    || deleteOrphansTiming != CascadeTiming.Immediate
                    || (changeMechanism & ChangeMechanism.Fk) != 0
                    || changeMechanism == ChangeMechanism.Dependent)
                {
                    Assert.True(context.ChangeTracker.HasChanges());

                    Assert.DoesNotContain(child, oldParent.Children);
                    Assert.Contains(child, newParent.Children);
                    Assert.Equal(newParent.AlternateId, child.ParentId);
                    Assert.Equal(EntityState.Modified, context.Entry(child).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(oldParent).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(newParent).State);

                    await context.SaveChangesAsync();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.DoesNotContain(child, oldParent.Children);
                    Assert.Contains(child, newParent.Children);
                    Assert.Equal(newParent.AlternateId, child.ParentId);
                    Assert.Equal(EntityState.Unchanged, context.Entry(child).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(oldParent).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(newParent).State);
                }
                else
                {
                    Assert.Throws<InvalidOperationException>(() => context.ChangeTracker.DetectChanges());
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction
                    || deleteOrphansTiming != CascadeTiming.Immediate
                    || (changeMechanism & ChangeMechanism.Fk) != 0
                    || changeMechanism == ChangeMechanism.Dependent)
                {
                    var root = await LoadRequiredAkGraphAsync(context);

                    Assert.False(context.ChangeTracker.HasChanges());

                    oldParent = root.RequiredChildrenAk.First(e => e.Id == oldParent.Id);
                    newParent = root.RequiredChildrenAk.First(e => e.Id == newParent.Id);
                    child = newParent.Children.First(e => e.Id == child.Id);

                    Assert.DoesNotContain(child, oldParent.Children);
                    Assert.Contains(child, newParent.Children);
                    Assert.Equal(newParent.AlternateId, child.ParentId);
                    Assert.Equal(EntityState.Unchanged, context.Entry(child).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(oldParent).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(newParent).State);
                }
            });
    }

    [ConditionalTheory]
    [InlineData((int)ChangeMechanism.Principal, false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Principal, true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Dependent, false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Dependent, true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Fk, false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Fk, true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), true, CascadeTiming.OnSaveChanges)]
    [InlineData(
        (int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), false, CascadeTiming.OnSaveChanges)]
    [InlineData(
        (int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Principal, false, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Principal, true, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Dependent, false, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Dependent, true, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Fk, false, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Fk, true, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), false, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), true, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), false, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), true, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), false, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), true, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Principal, false, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Principal, true, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Dependent, false, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Dependent, true, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Fk, false, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Fk, true, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), false, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), true, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), false, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), true, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), false, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), true, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Principal, false, null)]
    [InlineData((int)ChangeMechanism.Principal, true, null)]
    [InlineData((int)ChangeMechanism.Dependent, false, null)]
    [InlineData((int)ChangeMechanism.Dependent, true, null)]
    [InlineData((int)ChangeMechanism.Fk, false, null)]
    [InlineData((int)ChangeMechanism.Fk, true, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), false, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk), true, null)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), false, null)]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent), true, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), false, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk), true, null)]
    public virtual Task Reparent_one_to_many_overlapping(
        ChangeMechanism changeMechanism,
        bool useExistingParent,
        CascadeTiming? deleteOrphansTiming)
    {
        Root root = null;
        IReadOnlyList<EntityEntry> entries = null;
        var childCount = 0;
        RequiredComposite1 oldParent = null;
        OptionalOverlapping2 oldChild1 = null;
        OptionalOverlapping2 oldChild2 = null;
        RequiredComposite1 newParent = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                if (!useExistingParent)
                {
                    newParent = new RequiredComposite1
                    {
                        Id = 3,
                        Parent = await context.Set<Root>().SingleAsync(IsTheRoot),
                        CompositeChildren = new ObservableHashSet<OptionalOverlapping2>(ReferenceEqualityComparer.Instance)
                        {
                            new() { Id = 5 }, new() { Id = 6 }
                        }
                    };

                    context.Set<RequiredComposite1>().Add(newParent);
                    await context.SaveChangesAsync();
                }
            }, async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                root = await LoadRequiredCompositeGraphAsync(context);

                childCount = context.Set<OptionalOverlapping2>().Count();

                oldParent = root.RequiredCompositeChildren.OrderBy(e => e.Id).First();

                oldChild1 = oldParent.CompositeChildren.OrderBy(e => e.Id).First();
                oldChild2 = oldParent.CompositeChildren.OrderBy(e => e.Id).Last();

                Assert.Equal(useExistingParent ? 2 : 3, root.RequiredCompositeChildren.Count());

                if (useExistingParent)
                {
                    newParent = root.RequiredCompositeChildren.OrderBy(e => e.Id).Last();
                }
                else
                {
                    newParent = await context.Set<RequiredComposite1>().SingleAsync(e => e.Id == newParent.Id);
                    newParent.Parent = root;
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

                await context.SaveChangesAsync();

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

                entries = context.ChangeTracker.Entries().ToList();

                Assert.Equal(childCount, context.Set<OptionalOverlapping2>().Count());
            }, async context =>
            {
                var loadedRoot = await LoadRequiredCompositeGraphAsync(context);

                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);

                oldParent = await context.Set<RequiredComposite1>().SingleAsync(e => e.Id == oldParent.Id);
                newParent = await context.Set<RequiredComposite1>().SingleAsync(e => e.Id == newParent.Id);

                oldChild1 = await context.Set<OptionalOverlapping2>().SingleAsync(e => e.Id == oldChild1.Id);
                oldChild2 = await context.Set<OptionalOverlapping2>().SingleAsync(e => e.Id == oldChild2.Id);

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

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());

                Assert.Equal(childCount, context.Set<OptionalOverlapping2>().Count());
            });
    }

    [ConditionalTheory]
    [InlineData((int)ChangeMechanism.Principal, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Dependent, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Fk, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Principal, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Dependent, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Fk, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Principal, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Dependent, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Fk, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Principal, null)]
    [InlineData((int)ChangeMechanism.Dependent, null)]
    [InlineData((int)ChangeMechanism.Fk, null)]
    public virtual Task Mark_modified_one_to_many_overlapping(
        ChangeMechanism changeMechanism,
        CascadeTiming? deleteOrphansTiming)
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await LoadRequiredCompositeGraphAsync(context);
                var parent = root.RequiredCompositeChildren.OrderBy(e => e.Id).First();
                var child = parent.CompositeChildren.OrderBy(e => e.Id).First();

                var childCount = context.Set<OptionalOverlapping2>().Count();

                if ((changeMechanism & ChangeMechanism.Principal) != 0)
                {
                    context.Entry(parent).Collection(p => p.CompositeChildren).IsModified = true;
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    context.Entry(child).Reference(c => c.Parent).IsModified = true;
                }

                if ((changeMechanism & ChangeMechanism.Fk) != 0)
                {
                    context.Entry(child).Property(c => c.ParentId).IsModified = true;
                }

                Assert.True(context.ChangeTracker.HasChanges());

                await context.SaveChangesAsync();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Same(child, parent.CompositeChildren.OrderBy(e => e.Id).First());
                Assert.Same(parent, child.Parent);
                Assert.Equal(parent.Id, child.ParentId);
                Assert.Equal(parent.ParentAlternateId, child.ParentAlternateId);
                Assert.Equal(root.AlternateId, child.ParentAlternateId);
                Assert.Same(root, child.Root);

                Assert.Equal(childCount, context.Set<OptionalOverlapping2>().Count());
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
    [InlineData(null, null)]
    public virtual Task Required_many_to_one_dependents_are_cascade_deleted(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await LoadRequiredGraphAsync(context);

                Assert.Equal(2, root.RequiredChildren.Count());

                var removed = root.RequiredChildren.First();

                removedId = removed.Id;
                var cascadeRemoved = removed.Children.ToList();
                orphanedIds = cascadeRemoved.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                Assert.True(context.ChangeTracker.HasChanges());

                if (cascadeDeleteTiming == null)
                {
                    Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Unchanged));

                    context.ChangeTracker.CascadeChanges();

                    Assert.True(
                        cascadeRemoved.All(
                            e => context.Entry(e).State
                                == (Fixture.ForceClientNoAction ? EntityState.Unchanged : EntityState.Deleted)));
                }

                if (Fixture.ForceClientNoAction)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else if (cascadeDeleteTiming == CascadeTiming.Never)
                {
                    await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();

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
                if (!Fixture.ForceClientNoAction
                    && cascadeDeleteTiming != CascadeTiming.Never)
                {
                    var root = await LoadRequiredGraphAsync(context);

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
    [InlineData(null, null)]
    public virtual Task Required_many_to_one_dependent_leaves_can_be_deleted(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await LoadRequiredGraphAsync(context);
                var parent = root.RequiredChildren.First();

                Assert.Equal(2, parent.Children.Count());
                var removed = parent.Children.First();

                removedId = removed.Id;

                context.Remove(removed);

                Assert.True(context.ChangeTracker.HasChanges());

                if (cascadeDeleteTiming == null)
                {
                    context.ChangeTracker.CascadeChanges();
                }

                await context.SaveChangesAsync();

                Assert.False(context.ChangeTracker.HasChanges());
                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Single(parent.Children);
                Assert.DoesNotContain(removedId, parent.Children.Select(e => e.Id));

                Assert.Empty(context.Set<Required2>().Where(e => e.Id == removedId));

                Assert.Same(parent, removed.Parent);
            }, async context =>
            {
                var root = await LoadRequiredGraphAsync(context);
                var parent = root.RequiredChildren.First();

                Assert.Single(parent.Children);
                Assert.DoesNotContain(removedId, parent.Children.Select(e => e.Id));

                Assert.Empty(context.Set<Required2>().Where(e => e.Id == removedId));
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
    public virtual Task Optional_many_to_one_dependents_are_orphaned(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await LoadOptionalGraphAsync(context);

                Assert.Equal(2, root.OptionalChildren.Count());

                var removed = root.OptionalChildren.First();

                removedId = removed.Id;
                var orphaned = removed.Children.ToList();
                orphanedIds = orphaned.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                Assert.True(context.ChangeTracker.HasChanges());

                if (cascadeDeleteTiming == null)
                {
                    Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));

                    context.ChangeTracker.CascadeChanges();
                }

                var expectedState = cascadeDeleteTiming is CascadeTiming.Immediate or null
                    && !Fixture.ForceClientNoAction
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

                if (Fixture.ForceClientNoAction)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                    Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));

                    Assert.Single(root.OptionalChildren);
                    Assert.DoesNotContain(removedId, root.OptionalChildren.Select(e => e.Id));

                    Assert.Empty(context.Set<Optional1>().Where(e => e.Id == removedId));
                    Assert.Equal(orphanedIds.Count, context.Set<Optional2>().Count(e => orphanedIds.Contains(e.Id)));

                    Assert.Same(root, removed.Parent);
                    Assert.Equal(2, removed.Children.Count());
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction)
                {
                    var root = await LoadOptionalGraphAsync(context);

                    Assert.Single(root.OptionalChildren);
                    Assert.DoesNotContain(removedId, root.OptionalChildren.Select(e => e.Id));

                    Assert.Empty(context.Set<Optional1>().Where(e => e.Id == removedId));
                    Assert.Equal(orphanedIds.Count, context.Set<Optional2>().Count(e => orphanedIds.Contains(e.Id)));
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
    [InlineData(null, null)]
    public virtual Task Optional_many_to_one_dependents_are_orphaned_with_Added_graph(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming) // Issue #29318
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = new Root { AlternateId = Guid.NewGuid() };
                var removed = new Optional1 { Parent = root };
                var orphaned = new List<Optional2> { new() { Parent = removed }, new() { Parent = removed } };

                context.AddRange(orphaned);
                var removedId = context.Entry(removed).Property(e => e.Id).CurrentValue;
                context.Remove(removed);

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                if (cascadeDeleteTiming == null)
                {
                    Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Added));

                    context.ChangeTracker.CascadeChanges();
                }

                if (Fixture.ForceClientNoAction)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    foreach (var orphanEntry in orphaned.Select(context.Entry))
                    {
                        Assert.Equal(EntityState.Added, orphanEntry.State);
                        Assert.Null(orphanEntry.Entity.ParentId);
                        Assert.Null(orphanEntry.Property(e => e.ParentId).CurrentValue);
                    }

                    await context.SaveChangesAsync();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                    Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));

                    Assert.Empty(root.OptionalChildren);
                    Assert.Same(root, removed.Parent);
                    Assert.Equal(2, removed.Children.Count());
                }
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
    [InlineData(null, null)]
    public virtual Task Optional_many_to_one_dependent_leaves_can_be_deleted(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await LoadOptionalGraphAsync(context);
                var parent = root.OptionalChildren.First();

                Assert.Equal(2, parent.Children.Count());

                var removed = parent.Children.First();
                removedId = removed.Id;

                context.Remove(removed);

                Assert.True(context.ChangeTracker.HasChanges());

                if (cascadeDeleteTiming == null)
                {
                    context.ChangeTracker.CascadeChanges();
                }

                await context.SaveChangesAsync();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Single(parent.Children);
                Assert.DoesNotContain(removedId, parent.Children.Select(e => e.Id));

                Assert.Empty(context.Set<Optional2>().Where(e => e.Id == removedId));

                Assert.Same(parent, removed.Parent);
            }, async context =>
            {
                var root = await LoadOptionalGraphAsync(context);
                var parent = root.OptionalChildren.First();

                Assert.Single(parent.Children);
                Assert.DoesNotContain(removedId, parent.Children.Select(e => e.Id));

                Assert.Empty(context.Set<Optional2>().Where(e => e.Id == removedId));
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
    public virtual Task Required_many_to_one_dependents_are_cascade_deleted_in_store(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var removed = (await LoadRequiredGraphAsync(context)).RequiredChildren.First();

                removedId = removed.Id;
                orphanedIds = removed.Children.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
            }, async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await context.Set<Root>().Include(e => e.RequiredChildren).SingleAsync(IsTheRoot);
                context.Set<Required1>().Load();

                var removed = root.RequiredChildren.Single(e => e.Id == removedId);

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                Assert.True(context.ChangeTracker.HasChanges());

                if (cascadeDeleteTiming == null)
                {
                    context.ChangeTracker.CascadeChanges();
                }

                if (Fixture.ForceClientNoAction
                    || Fixture.NoStoreCascades)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                    Assert.Single(root.RequiredChildren);
                    Assert.DoesNotContain(removedId, root.RequiredChildren.Select(e => e.Id));

                    Assert.Empty(context.Set<Required1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<Required2>().Where(e => orphanedIds.Contains(e.Id)));

                    Assert.Same(root, removed.Parent);
                    Assert.Empty(removed.Children);
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction
                    && !Fixture.NoStoreCascades)
                {
                    var root = await LoadRequiredGraphAsync(context);

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
    [InlineData(null, null)]
    public virtual Task Optional_many_to_one_dependents_are_orphaned_in_store(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var removed = (await LoadOptionalGraphAsync(context)).OptionalChildren.First();

                removedId = removed.Id;
                orphanedIds = removed.Children.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
            }, async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await context.Set<Root>().Include(e => e.OptionalChildren).SingleAsync(IsTheRoot);
                context.Entry(root).Collection(e => e.OptionalChildren).Load();

                var removed = root.OptionalChildren.First(e => e.Id == removedId);

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                Assert.True(context.ChangeTracker.HasChanges());

                if (cascadeDeleteTiming == null)
                {
                    context.ChangeTracker.CascadeChanges();
                }

                if (Fixture.ForceClientNoAction)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();

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
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction)
                {
                    var root = await LoadOptionalGraphAsync(context);

                    Assert.Single(root.OptionalChildren);
                    Assert.DoesNotContain(removedId, root.OptionalChildren.Select(e => e.Id));

                    Assert.Empty(context.Set<Optional1>().Where(e => e.Id == removedId));

                    var orphaned = context.Set<Optional2>().Where(e => orphanedIds.Contains(e.Id)).ToList();
                    Assert.Equal(orphanedIds.Count, orphaned.Count);
                    Assert.True(orphaned.All(e => e.ParentId == null));
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
    [InlineData(null, null)]
    public virtual Task Required_many_to_one_dependents_are_cascade_deleted_starting_detached(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;
        Root root = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadRequiredGraphAsync(context);

                Assert.Equal(2, root.RequiredChildren.Count());
            }, async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var removed = root.RequiredChildren.First();

                removedId = removed.Id;
                var cascadeRemoved = removed.Children.ToList();
                orphanedIds = cascadeRemoved.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);

                if (cascadeDeleteTiming == null)
                {
                    Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Unchanged));

                    context.ChangeTracker.CascadeChanges();
                }

                var expectedState = cascadeDeleteTiming is CascadeTiming.Immediate or null
                    && !Fixture.ForceClientNoAction
                        ? EntityState.Deleted
                        : EntityState.Unchanged;

                Assert.True(cascadeRemoved.All(e => context.Entry(e).State == expectedState));

                Assert.True(context.ChangeTracker.HasChanges());

                if (Fixture.ForceClientNoAction)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else if (cascadeDeleteTiming == CascadeTiming.Never)
                {
                    await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                    Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Detached));

                    Assert.Same(root, removed.Parent);
                    Assert.Equal(2, removed.Children.Count());
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction
                    && cascadeDeleteTiming != CascadeTiming.Never)
                {
                    root = await LoadRequiredGraphAsync(context);

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
    [InlineData(null, null)]
    public virtual Task Optional_many_to_one_dependents_are_orphaned_starting_detached(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;
        Root root = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadOptionalGraphAsync(context);

                Assert.Equal(2, root.OptionalChildren.Count());
            }, async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var removed = root.OptionalChildren.First();

                removedId = removed.Id;
                var orphaned = removed.Children.ToList();
                orphanedIds = orphaned.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);

                if (cascadeDeleteTiming == null)
                {
                    Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));

                    context.ChangeTracker.CascadeChanges();
                }

                var expectedState = cascadeDeleteTiming is CascadeTiming.Immediate or null
                    && !Fixture.ForceClientNoAction
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

                if (Fixture.ForceClientNoAction)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                    Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));

                    Assert.Same(root, removed.Parent);
                    Assert.Equal(2, removed.Children.Count());
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction)
                {
                    root = await LoadOptionalGraphAsync(context);

                    Assert.Single(root.OptionalChildren);
                    Assert.DoesNotContain(removedId, root.OptionalChildren.Select(e => e.Id));

                    Assert.Empty(context.Set<Optional1>().Where(e => e.Id == removedId));
                    Assert.Equal(orphanedIds.Count, context.Set<Optional2>().Count(e => orphanedIds.Contains(e.Id)));
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
    [InlineData(null, null)]
    public virtual Task Required_many_to_one_dependents_are_cascade_detached_when_Added(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await LoadRequiredGraphAsync(context);

                Assert.Equal(2, root.RequiredChildren.Count());

                var removed = root.RequiredChildren.First();

                removedId = removed.Id;
                var cascadeRemoved = removed.Children.ToList();
                orphanedIds = cascadeRemoved.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);

                var added = new Required2();
                Add(removed.Children, added);

                if (context.ChangeTracker.AutoDetectChangesEnabled)
                {
                    context.ChangeTracker.DetectChanges();
                }

                Assert.Equal(EntityState.Unchanged, context.Entry(removed).State);
                Assert.Equal(EntityState.Added, context.Entry(added).State);
                Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Unchanged));

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);

                if (cascadeDeleteTiming == null)
                {
                    Assert.Equal(EntityState.Added, context.Entry(added).State);
                    Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Unchanged));

                    context.ChangeTracker.CascadeChanges();
                }

                if (cascadeDeleteTiming is CascadeTiming.Immediate or null
                    && !Fixture.ForceClientNoAction)
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

                if (Fixture.ForceClientNoAction)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else if (cascadeDeleteTiming == CascadeTiming.Never)
                {
                    await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                    Assert.Equal(EntityState.Detached, context.Entry(added).State);
                    Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Detached));

                    Assert.Same(root, removed.Parent);
                    Assert.Equal(3, removed.Children.Count());
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction
                    && cascadeDeleteTiming != CascadeTiming.Never)
                {
                    var root = await LoadRequiredGraphAsync(context);

                    Assert.Single(root.RequiredChildren);
                    Assert.DoesNotContain(removedId, root.RequiredChildren.Select(e => e.Id));

                    Assert.Empty(context.Set<Required1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<Required2>().Where(e => orphanedIds.Contains(e.Id)));
                }
            });
    }
}
