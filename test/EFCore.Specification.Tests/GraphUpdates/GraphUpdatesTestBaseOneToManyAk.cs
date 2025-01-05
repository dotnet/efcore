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
    public virtual Task Save_optional_many_to_one_dependents_with_alternate_key(
        ChangeMechanism changeMechanism,
        bool useExistingEntities,
        CascadeTiming? deleteOrphansTiming)
    {
        var new1 = new OptionalAk1 { AlternateId = Guid.NewGuid() };
        var new1d = new OptionalAk1Derived { AlternateId = Guid.NewGuid() };
        var new1dd = new OptionalAk1MoreDerived { AlternateId = Guid.NewGuid() };
        var new2a = new OptionalAk2 { AlternateId = Guid.NewGuid() };
        var new2b = new OptionalAk2 { AlternateId = Guid.NewGuid() };
        var new2ca = new OptionalComposite2();
        var new2cb = new OptionalComposite2();
        var new2d = new OptionalAk2Derived { AlternateId = Guid.NewGuid() };
        var new2dd = new OptionalAk2MoreDerived { AlternateId = Guid.NewGuid() };
        Root root = null;
        IReadOnlyList<EntityEntry> entries = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                if (useExistingEntities)
                {
                    context.AddRange(new1, new1d, new1dd, new2a, new2d, new2dd, new2b, new2ca, new2cb);
                    await context.SaveChangesAsync();
                }
            }, async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                root = await LoadOptionalAkGraphAsync(context);
                var existing = root.OptionalChildrenAk.OrderBy(e => e.Id).First();

                if (useExistingEntities)
                {
                    new1 = await context.Set<OptionalAk1>().SingleAsync(e => e.Id == new1.Id);
                    new1d = (OptionalAk1Derived)await context.Set<OptionalAk1>().SingleAsync(e => e.Id == new1d.Id);
                    new1dd = (OptionalAk1MoreDerived)await context.Set<OptionalAk1>().SingleAsync(e => e.Id == new1dd.Id);
                    new2a = await context.Set<OptionalAk2>().SingleAsync(e => e.Id == new2a.Id);
                    new2b = await context.Set<OptionalAk2>().SingleAsync(e => e.Id == new2b.Id);
                    new2ca = await context.Set<OptionalComposite2>().SingleAsync(e => e.Id == new2ca.Id);
                    new2cb = await context.Set<OptionalComposite2>().SingleAsync(e => e.Id == new2cb.Id);
                    new2d = (OptionalAk2Derived)await context.Set<OptionalAk2>().SingleAsync(e => e.Id == new2d.Id);
                    new2dd = (OptionalAk2MoreDerived)await context.Set<OptionalAk2>().SingleAsync(e => e.Id == new2dd.Id);
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

                await context.SaveChangesAsync();

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

                entries = context.ChangeTracker.Entries().ToList();
            }, async context =>
            {
                var loadedRoot = await LoadOptionalAkGraphAsync(context);

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
    public virtual Task Save_required_many_to_one_dependents_with_alternate_key(
        ChangeMechanism changeMechanism,
        bool useExistingEntities,
        CascadeTiming? deleteOrphansTiming)
    {
        var newRoot = new Root { AlternateId = Guid.NewGuid() };
        var new1 = new RequiredAk1 { AlternateId = Guid.NewGuid(), Parent = newRoot };
        var new1d = new RequiredAk1Derived { AlternateId = Guid.NewGuid(), Parent = newRoot };
        var new1dd = new RequiredAk1MoreDerived { AlternateId = Guid.NewGuid(), Parent = newRoot };
        var new2a = new RequiredAk2 { AlternateId = Guid.NewGuid(), Parent = new1 };
        var new2b = new RequiredAk2 { AlternateId = Guid.NewGuid(), Parent = new1 };
        var new2ca = new RequiredComposite2 { Parent = new1 };
        var new2cb = new RequiredComposite2 { Parent = new1 };
        var new2d = new RequiredAk2Derived { AlternateId = Guid.NewGuid(), Parent = new1 };
        var new2dd = new RequiredAk2MoreDerived { AlternateId = Guid.NewGuid(), Parent = new1 };
        Root root = null;
        IReadOnlyList<EntityEntry> entries = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                if (useExistingEntities)
                {
                    context.AddRange(newRoot, new1, new1d, new1dd, new2a, new2d, new2dd, new2b, new2ca, new2cb);
                    await context.SaveChangesAsync();
                }
            }, async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                root = await LoadRequiredAkGraphAsync(context);
                var existing = root.RequiredChildrenAk.OrderBy(e => e.Id).First();

                if (useExistingEntities)
                {
                    new1 = await context.Set<RequiredAk1>().SingleAsync(e => e.Id == new1.Id);
                    new1d = (RequiredAk1Derived)await context.Set<RequiredAk1>().SingleAsync(e => e.Id == new1d.Id);
                    new1dd = (RequiredAk1MoreDerived)await context.Set<RequiredAk1>().SingleAsync(e => e.Id == new1dd.Id);
                    new2a = await context.Set<RequiredAk2>().SingleAsync(e => e.Id == new2a.Id);
                    new2b = await context.Set<RequiredAk2>().SingleAsync(e => e.Id == new2b.Id);
                    new2ca = await context.Set<RequiredComposite2>().SingleAsync(e => e.Id == new2ca.Id);
                    new2cb = await context.Set<RequiredComposite2>().SingleAsync(e => e.Id == new2cb.Id);
                    new2d = (RequiredAk2Derived)await context.Set<RequiredAk2>().SingleAsync(e => e.Id == new2d.Id);
                    new2dd = (RequiredAk2MoreDerived)await context.Set<RequiredAk2>().SingleAsync(e => e.Id == new2dd.Id);
                }
                else
                {
                    new1.Parent = null;
                    new1d.Parent = null;
                    new1dd.Parent = null;

                    var old1 = root.RequiredChildrenAk.OrderBy(c => c.Id).Last();

                    // Test replacing entity with the same AK, but different PK
                    new1.AlternateId = old1.AlternateId;
                    context.Remove(old1);
                    context.RemoveRange(old1.Children);
                    context.RemoveRange(old1.CompositeChildren);
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

                await context.SaveChangesAsync();

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

                entries = context.ChangeTracker.Entries().ToList();
            }, async context =>
            {
                var loadedRoot = await LoadRequiredAkGraphAsync(context);

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
    public virtual Task Save_removed_optional_many_to_one_dependents_with_alternate_key(
        ChangeMechanism changeMechanism,
        CascadeTiming? deleteOrphansTiming)
    {
        Root root = null;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                root = await LoadOptionalAkGraphAsync(context);

                var firstChild = root.OptionalChildrenAk.OrderByDescending(c => c.Id).First();
                var childCollection = firstChild.Children;
                var childCompositeCollection = firstChild.CompositeChildren;
                var removed2 = childCollection.OrderByDescending(c => c.Id).First();
                var removed1 = root.OptionalChildrenAk.OrderByDescending(c => c.Id).Skip(1).First();
                var removed2c = childCompositeCollection.OrderByDescending(c => c.Id).First();

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

                await context.SaveChangesAsync();

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
                    var loadedRoot = await LoadOptionalAkGraphAsync(context);

                    AssertKeys(root, loadedRoot);
                    AssertNavigations(loadedRoot);

                    Assert.Single(loadedRoot.OptionalChildrenAk);
                    Assert.Single(loadedRoot.OptionalChildrenAk.OrderBy(c => c.Id).First().Children);
                }
            });
    }

    [ConditionalTheory]
    [InlineData((int)ChangeMechanism.Principal, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Dependent, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Principal, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Dependent, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Principal, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Dependent, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Principal, null)]
    [InlineData((int)ChangeMechanism.Dependent, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), null)]
    public virtual Task Save_removed_required_many_to_one_dependents_with_alternate_key(
        ChangeMechanism changeMechanism,
        CascadeTiming? deleteOrphansTiming)
    {
        Root root = null;
        RequiredAk2 removed2 = null;
        RequiredComposite2 removed2c = null;
        RequiredAk1 removed1 = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                root = await LoadRequiredAkGraphAsync(context);

                var firstChild = root.RequiredChildrenAk.OrderByDescending(c => c.Id).First();
                var childCollection = firstChild.Children;
                var childCompositeCollection = firstChild.CompositeChildren;
                removed2 = childCollection.OrderBy(c => c.Id).First();
                removed2c = childCompositeCollection.OrderBy(c => c.Id).First();
                removed1 = root.RequiredChildrenAk.OrderByDescending(c => c.Id).Skip(1).First();

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

                if (Fixture.ForceClientNoAction
                    || deleteOrphansTiming == CascadeTiming.Never)
                {
                    var testCode = deleteOrphansTiming == CascadeTiming.Immediate
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
                            : (Func<Task>)(() => context.SaveChangesAsync());

                    var message = (await Assert.ThrowsAsync<InvalidOperationException>(testCode)).Message;

                    Assert.True(
                        message
                        == CoreStrings.RelationshipConceptualNullSensitive(
                            nameof(Root), nameof(RequiredAk1), "{ParentId: " + removed1.ParentId + "}")
                        || message
                        == CoreStrings.RelationshipConceptualNullSensitive(
                            nameof(RequiredAk1), nameof(RequiredAk2), "{ParentId: " + removed2.ParentId + "}"));
                }
                else
                {
                    Assert.True(context.ChangeTracker.HasChanges());

                    if (deleteOrphansTiming == null)
                    {
                        context.ChangeTracker.CascadeChanges();
                    }

                    await context.SaveChangesAsync();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.DoesNotContain(removed1, root.RequiredChildrenAk);
                    Assert.DoesNotContain(removed2, childCollection);
                    Assert.DoesNotContain(removed2c, childCompositeCollection);

                    Assert.Null(removed1.Parent);
                    Assert.Null(removed2.Parent);
                    Assert.Null(removed2c.Parent);
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction
                    && !Fixture.NoStoreCascades
                    && deleteOrphansTiming != CascadeTiming.Never)
                {
                    var loadedRoot = await LoadRequiredAkGraphAsync(context);

                    AssertKeys(root, loadedRoot);
                    AssertNavigations(loadedRoot);

                    Assert.False(context.Set<RequiredAk1>().Any(e => e.Id == removed1.Id));
                    Assert.False(context.Set<RequiredAk2>().Any(e => e.Id == removed2.Id));
                    Assert.False(context.Set<RequiredComposite2>().Any(e => e.Id == removed2c.Id));

                    Assert.Single(loadedRoot.RequiredChildrenAk);
                    Assert.Single(loadedRoot.RequiredChildrenAk.OrderBy(c => c.Id).First().Children);
                    Assert.Single(loadedRoot.RequiredChildrenAk.OrderBy(c => c.Id).First().CompositeChildren);
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
    public virtual Task Optional_many_to_one_dependents_with_alternate_key_are_orphaned(
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

                var root = await LoadOptionalAkGraphAsync(context);

                Assert.Equal(2, root.OptionalChildrenAk.Count());

                var removed = root.OptionalChildrenAk.OrderBy(c => c.Id).First();

                removedId = removed.Id;
                var orphaned = removed.Children.ToList();
                orphanedIds = orphaned.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                if (cascadeDeleteTiming == null)
                {
                    Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));

                    context.ChangeTracker.CascadeChanges();

                    Assert.True(
                        orphaned.All(
                            e => context.Entry(e).State
                                == (Fixture.ForceClientNoAction ? EntityState.Unchanged : EntityState.Modified)));
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

                    Assert.Single(root.OptionalChildrenAk);
                    Assert.DoesNotContain(removedId, root.OptionalChildrenAk.Select(e => e.Id));

                    Assert.Empty(context.Set<OptionalAk1>().Where(e => e.Id == removedId));
                    Assert.Equal(orphanedIds.Count, context.Set<OptionalAk2>().Count(e => orphanedIds.Contains(e.Id)));

                    Assert.Same(root, removed.Parent);
                    Assert.Equal(2, removed.Children.Count());
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction)
                {
                    var root = await LoadOptionalAkGraphAsync(context);

                    Assert.Single(root.OptionalChildrenAk);
                    Assert.DoesNotContain(removedId, root.OptionalChildrenAk.Select(e => e.Id));

                    Assert.Empty(context.Set<OptionalAk1>().Where(e => e.Id == removedId));
                    Assert.Equal(orphanedIds.Count, context.Set<OptionalAk2>().Count(e => orphanedIds.Contains(e.Id)));
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
    public virtual Task Required_many_to_one_dependents_with_alternate_key_are_cascade_deleted(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;
        List<int> orphanedIdCs = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await LoadRequiredAkGraphAsync(context);

                Assert.Equal(2, root.RequiredChildrenAk.Count());

                var removed = root.RequiredChildrenAk.OrderBy(c => c.Id).First();

                removedId = removed.Id;
                var cascadeRemoved = removed.Children.ToList();
                var cascadeRemovedC = removed.CompositeChildren.ToList();
                orphanedIds = cascadeRemoved.Select(e => e.Id).ToList();
                orphanedIdCs = cascadeRemovedC.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
                Assert.Equal(2, orphanedIdCs.Count);

                context.Remove(removed);

                Assert.True(context.ChangeTracker.HasChanges());

                if (cascadeDeleteTiming == null)
                {
                    Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Unchanged));
                    Assert.True(cascadeRemovedC.All(e => context.Entry(e).State == EntityState.Unchanged));

                    context.ChangeTracker.CascadeChanges();

                    if (Fixture.ForceClientNoAction)
                    {
                        Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Unchanged));
                        Assert.True(cascadeRemovedC.All(e => context.Entry(e).State == EntityState.Unchanged));
                    }
                    else
                    {
                        Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Deleted));
                        Assert.True(cascadeRemovedC.All(e => context.Entry(e).State == EntityState.Deleted));
                    }
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
                if (!Fixture.ForceClientNoAction
                    && cascadeDeleteTiming != CascadeTiming.Never)
                {
                    var root = await LoadRequiredAkGraphAsync(context);

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
    [InlineData(null, null)]
    public virtual Task Required_many_to_one_dependents_with_alternate_key_are_cascade_deleted_in_store(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;
        List<int> orphanedIdCs = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var removed = (await LoadRequiredAkGraphAsync(context)).RequiredChildrenAk.First();

                removedId = removed.Id;
                orphanedIds = removed.Children.Select(e => e.Id).ToList();
                orphanedIdCs = removed.CompositeChildren.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
                Assert.Equal(2, orphanedIdCs.Count);
            }, async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await context.Set<Root>().Include(e => e.RequiredChildrenAk).SingleAsync(IsTheRoot);
                context.Set<RequiredAk1>().Load();

                var removed = root.RequiredChildrenAk.Single(e => e.Id == removedId);

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

                    Assert.Single(root.RequiredChildrenAk);
                    Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                    Assert.Empty(context.Set<RequiredAk1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<RequiredAk2>().Where(e => orphanedIds.Contains(e.Id)));
                    Assert.Empty(context.Set<RequiredComposite2>().Where(e => orphanedIdCs.Contains(e.Id)));

                    Assert.Same(root, removed.Parent);
                    Assert.Empty(removed.Children); // Never loaded
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction
                    && !Fixture.NoStoreCascades)
                {
                    var root = await LoadRequiredAkGraphAsync(context);

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
    [InlineData(null, null)]
    public virtual Task Optional_many_to_one_dependents_with_alternate_key_are_orphaned_in_store(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;
        List<int> orphanedIdCs = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var removed = (await LoadOptionalAkGraphAsync(context)).OptionalChildrenAk.OrderBy(c => c.Id).First();

                removedId = removed.Id;
                orphanedIds = removed.Children.Select(e => e.Id).ToList();
                orphanedIdCs = removed.CompositeChildren.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
                Assert.Equal(2, orphanedIdCs.Count);
            }, async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await context.Set<Root>().Include(e => e.OptionalChildrenAk).SingleAsync(IsTheRoot);
                context.Entry(root).Collection(e => e.OptionalChildrenAk).Load();

                var removed = root.OptionalChildrenAk.First(e => e.Id == removedId);

                context.Remove(removed);

                foreach (var toOrphan in await context.Set<OptionalComposite2>().Where(e => orphanedIdCs.Contains(e.Id)).ToListAsync())
                {
                    toOrphan.ParentId = null;
                }

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

                    Assert.Single(root.OptionalChildrenAk);
                    Assert.DoesNotContain(removedId, root.OptionalChildrenAk.Select(e => e.Id));

                    Assert.Empty(context.Set<OptionalAk1>().Where(e => e.Id == removedId));

                    var orphaned = await context.Set<OptionalAk2>().Where(e => orphanedIds.Contains(e.Id)).ToListAsync();
                    Assert.Equal(orphanedIds.Count, orphaned.Count);
                    Assert.True(orphaned.All(e => e.ParentId == null));

                    var orphanedC = await context.Set<OptionalComposite2>().Where(e => orphanedIdCs.Contains(e.Id)).ToListAsync();
                    Assert.Equal(orphanedIdCs.Count, orphanedC.Count);
                    Assert.True(orphanedC.All(e => e.ParentId == null));

                    Assert.Same(root, removed.Parent);
                    Assert.Empty(removed.Children); // Never loaded
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction)
                {
                    var root = await LoadOptionalAkGraphAsync(context);

                    Assert.Single(root.OptionalChildrenAk);
                    Assert.DoesNotContain(removedId, root.OptionalChildrenAk.Select(e => e.Id));

                    Assert.Empty(context.Set<OptionalAk1>().Where(e => e.Id == removedId));

                    var orphaned = await context.Set<OptionalAk2>().Where(e => orphanedIds.Contains(e.Id)).ToListAsync();
                    Assert.Equal(orphanedIds.Count, orphaned.Count);
                    Assert.True(orphaned.All(e => e.ParentId == null));

                    var orphanedC = await context.Set<OptionalComposite2>().Where(e => orphanedIdCs.Contains(e.Id)).ToListAsync();
                    Assert.Equal(orphanedIdCs.Count, orphanedC.Count);
                    Assert.True(orphanedC.All(e => e.ParentId == null));
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
    public virtual Task Optional_many_to_one_dependents_with_alternate_key_are_orphaned_starting_detached(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;
        List<int> orphanedIdCs = null;
        Root root = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadOptionalAkGraphAsync(context);

                Assert.Equal(2, root.OptionalChildrenAk.Count());
            }, async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var removed = root.OptionalChildrenAk.OrderBy(c => c.Id).First();

                removedId = removed.Id;
                var orphaned = removed.Children.ToList();
                var orphanedC = removed.CompositeChildren.ToList();
                orphanedIds = orphaned.Select(e => e.Id).ToList();
                orphanedIdCs = orphanedC.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
                Assert.Equal(2, orphanedIdCs.Count);

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);

                if (cascadeDeleteTiming == null)
                {
                    Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));
                    Assert.True(orphanedC.All(e => context.Entry(e).State == EntityState.Unchanged));

                    context.ChangeTracker.CascadeChanges();
                }

                var expectedState = cascadeDeleteTiming is CascadeTiming.Immediate or null
                    && !Fixture.ForceClientNoAction
                        ? EntityState.Modified
                        : EntityState.Unchanged;

                Assert.True(orphaned.All(e => context.Entry(e).State == expectedState));
                Assert.True(orphanedC.All(e => context.Entry(e).State == expectedState));

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
                    Assert.True(orphanedC.All(e => context.Entry(e).State == EntityState.Unchanged));

                    Assert.Same(root, removed.Parent);
                    Assert.Equal(2, removed.Children.Count());
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction)
                {
                    root = await LoadOptionalAkGraphAsync(context);

                    Assert.Single(root.OptionalChildrenAk);
                    Assert.DoesNotContain(removedId, root.OptionalChildrenAk.Select(e => e.Id));

                    Assert.Empty(context.Set<OptionalAk1>().Where(e => e.Id == removedId));
                    Assert.Equal(orphanedIds.Count, context.Set<OptionalAk2>().Count(e => orphanedIds.Contains(e.Id)));
                    Assert.Equal(orphanedIdCs.Count, context.Set<OptionalComposite2>().Count(e => orphanedIdCs.Contains(e.Id)));
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
    public virtual Task Required_many_to_one_dependents_with_alternate_key_are_cascade_deleted_starting_detached(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;
        List<int> orphanedIdCs = null;
        Root root = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadRequiredAkGraphAsync(context);

                Assert.Equal(2, root.RequiredChildrenAk.Count());
            }, async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var removed = root.RequiredChildrenAk.OrderBy(c => c.Id).First();

                removedId = removed.Id;
                var cascadeRemoved = removed.Children.ToList();
                var cascadeRemovedC = removed.CompositeChildren.ToList();
                orphanedIds = cascadeRemoved.Select(e => e.Id).ToList();
                orphanedIdCs = cascadeRemovedC.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);

                if (cascadeDeleteTiming == null)
                {
                    Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Unchanged));
                    Assert.True(cascadeRemovedC.All(e => context.Entry(e).State == EntityState.Unchanged));

                    context.ChangeTracker.CascadeChanges();
                }

                var expectedState = cascadeDeleteTiming is CascadeTiming.Immediate or null
                    && !Fixture.ForceClientNoAction
                        ? EntityState.Deleted
                        : EntityState.Unchanged;

                Assert.True(cascadeRemoved.All(e => context.Entry(e).State == expectedState));
                Assert.True(cascadeRemovedC.All(e => context.Entry(e).State == expectedState));

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
                    Assert.True(cascadeRemovedC.All(e => context.Entry(e).State == EntityState.Detached));

                    Assert.Same(root, removed.Parent);
                    Assert.Equal(2, removed.Children.Count());
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction
                    && cascadeDeleteTiming != CascadeTiming.Never)
                {
                    root = await LoadRequiredAkGraphAsync(context);

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
    [InlineData(null, null)]
    public virtual Task Required_many_to_one_dependents_with_alternate_key_are_cascade_detached_when_Added(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        List<int> orphanedIds = null;
        List<int> orphanedIdCs = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await LoadRequiredAkGraphAsync(context);

                Assert.Equal(2, root.RequiredChildrenAk.Count());

                var removed = root.RequiredChildrenAk.OrderBy(c => c.Id).First();

                removedId = removed.Id;
                var cascadeRemoved = removed.Children.ToList();
                var cascadeRemovedC = removed.CompositeChildren.ToList();
                orphanedIds = cascadeRemoved.Select(e => e.Id).ToList();
                orphanedIdCs = cascadeRemovedC.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
                Assert.Equal(2, orphanedIdCs.Count);

                var added = new RequiredAk2();
                var addedC = new RequiredComposite2();
                Add(removed.Children, added);
                Add(removed.CompositeChildren, addedC);

                if (context.ChangeTracker.AutoDetectChangesEnabled)
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

                if (cascadeDeleteTiming == null)
                {
                    Assert.Equal(EntityState.Added, context.Entry(added).State);
                    Assert.Equal(EntityState.Added, context.Entry(addedC).State);
                    Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Unchanged));
                    Assert.True(cascadeRemovedC.All(e => context.Entry(e).State == EntityState.Unchanged));

                    context.ChangeTracker.CascadeChanges();
                }

                if (cascadeDeleteTiming is CascadeTiming.Immediate or null
                    && !Fixture.ForceClientNoAction)
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
                    Assert.Equal(EntityState.Detached, context.Entry(addedC).State);
                    Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Detached));
                    Assert.True(cascadeRemovedC.All(e => context.Entry(e).State == EntityState.Detached));

                    Assert.Same(root, removed.Parent);
                    Assert.Equal(3, removed.Children.Count());
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction
                    && cascadeDeleteTiming != CascadeTiming.Never)
                {
                    var root = await LoadRequiredAkGraphAsync(context);

                    Assert.Single(root.RequiredChildrenAk);
                    Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                    Assert.Empty(context.Set<RequiredAk1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<RequiredAk2>().Where(e => orphanedIds.Contains(e.Id)));
                    Assert.Empty(context.Set<RequiredComposite2>().Where(e => orphanedIdCs.Contains(e.Id)));
                }
            });
    }
}
