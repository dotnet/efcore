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
    [InlineData(CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.Never)]
    [InlineData(null)]
    public virtual Task Optional_one_to_one_with_AK_relationships_are_one_to_one(
        CascadeTiming? deleteOrphansTiming)
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await context.Set<Root>().SingleAsync(IsTheRoot);

                Assert.False(context.ChangeTracker.HasChanges());

                root.OptionalSingleAk = new OptionalSingleAk1();

                Assert.True(context.ChangeTracker.HasChanges());

                await Assert.ThrowsAsync<DbUpdateException>(async () => await context.SaveChangesAsync());
            });

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
    public virtual Task Save_changed_optional_one_to_one_with_alternate_key(
        ChangeMechanism changeMechanism,
        bool useExistingEntities,
        CascadeTiming? deleteOrphansTiming)
    {
        var new2 = new OptionalSingleAk2 { AlternateId = Guid.NewGuid() };
        var new2d = new OptionalSingleAk2Derived { AlternateId = Guid.NewGuid() };
        var new2dd = new OptionalSingleAk2MoreDerived { AlternateId = Guid.NewGuid() };
        var new2c = new OptionalSingleComposite2();
        var new1 = new OptionalSingleAk1
        {
            AlternateId = Guid.NewGuid(),
            Single = new2,
            SingleComposite = new2c
        };
        var new1d = new OptionalSingleAk1Derived { AlternateId = Guid.NewGuid(), Single = new2d };
        var new1dd = new OptionalSingleAk1MoreDerived { AlternateId = Guid.NewGuid(), Single = new2dd };
        Root root = null;
        IReadOnlyList<EntityEntry> entries = null;
        OptionalSingleAk1 old1 = null;
        OptionalSingleAk1Derived old1d = null;
        OptionalSingleAk1MoreDerived old1dd = null;
        OptionalSingleAk2 old2 = null;
        OptionalSingleComposite2 old2c = null;
        OptionalSingleAk2Derived old2d = null;
        OptionalSingleAk2MoreDerived old2dd = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                if (useExistingEntities)
                {
                    context.AddRange(new1, new1d, new1dd, new2, new2d, new2dd, new2c);
                    await context.SaveChangesAsync();
                }
            }, async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                root = await LoadOptionalAkGraphAsync(context);

                old1 = root.OptionalSingleAk;
                old1d = root.OptionalSingleAkDerived;
                old1dd = root.OptionalSingleAkMoreDerived;
                old2 = root.OptionalSingleAk.Single;
                old2c = root.OptionalSingleAk.SingleComposite;
                old2d = (OptionalSingleAk2Derived)root.OptionalSingleAkDerived.Single;
                old2dd = (OptionalSingleAk2MoreDerived)root.OptionalSingleAkMoreDerived.Single;

                if (useExistingEntities)
                {
                    new1 = await context.Set<OptionalSingleAk1>().SingleAsync(e => e.Id == new1.Id);
                    new1d = (OptionalSingleAk1Derived)await context.Set<OptionalSingleAk1>().SingleAsync(e => e.Id == new1d.Id);
                    new1dd = (OptionalSingleAk1MoreDerived)await context.Set<OptionalSingleAk1>().SingleAsync(e => e.Id == new1dd.Id);
                    new2 = await context.Set<OptionalSingleAk2>().SingleAsync(e => e.Id == new2.Id);
                    new2c = await context.Set<OptionalSingleComposite2>().SingleAsync(e => e.Id == new2c.Id);
                    new2d = (OptionalSingleAk2Derived)await context.Set<OptionalSingleAk2>().SingleAsync(e => e.Id == new2d.Id);
                    new2dd = (OptionalSingleAk2MoreDerived)await context.Set<OptionalSingleAk2>().SingleAsync(e => e.Id == new2dd.Id);
                }
                else
                {
                    context.AddRange(new1, new1d, new1dd, new2, new2d, new2dd, new2c);
                }

                if ((changeMechanism & ChangeMechanism.Principal) != 0)
                {
                    root.OptionalSingleAk = new1;
                    root.OptionalSingleAkDerived = new1d;
                    root.OptionalSingleAkMoreDerived = new1dd;
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    new1.Root = root;
                    new1d.DerivedRoot = root;
                    new1dd.MoreDerivedRoot = root;
                }

                if ((changeMechanism & ChangeMechanism.Fk) != 0)
                {
                    new1.RootId = root.AlternateId;
                    new1d.DerivedRootId = root.AlternateId;
                    new1dd.MoreDerivedRootId = root.AlternateId;
                }

                Assert.True(context.ChangeTracker.HasChanges());

                await context.SaveChangesAsync();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Equal(root.AlternateId, new1.RootId);
                Assert.Equal(root.AlternateId, new1d.DerivedRootId);
                Assert.Equal(root.AlternateId, new1dd.MoreDerivedRootId);
                Assert.Equal(new1.AlternateId, new2.BackId);
                Assert.Equal(new1.Id, new2c.BackId);
                Assert.Equal(new1.AlternateId, new2c.ParentAlternateId);
                Assert.Equal(new1d.AlternateId, new2d.BackId);
                Assert.Equal(new1dd.AlternateId, new2dd.BackId);
                Assert.Same(root, new1.Root);
                Assert.Same(root, new1d.DerivedRoot);
                Assert.Same(root, new1dd.MoreDerivedRoot);
                Assert.Same(new1, new2.Back);
                Assert.Same(new1, new2c.Back);
                Assert.Same(new1d, new2d.Back);
                Assert.Same(new1dd, new2dd.Back);

                Assert.Null(old1.Root);
                Assert.Null(old1d.DerivedRoot);
                Assert.Null(old1dd.MoreDerivedRoot);
                Assert.Same(old1, old2.Back);
                Assert.Same(old1, old2c.Back);
                Assert.Equal(old1d, old2d.Back);
                Assert.Equal(old1dd, old2dd.Back);
                Assert.Null(old1.RootId);
                Assert.Null(old1d.DerivedRootId);
                Assert.Null(old1dd.MoreDerivedRootId);
                Assert.Equal(old1.AlternateId, old2.BackId);
                Assert.Equal(old1.Id, old2c.BackId);
                Assert.Equal(old1.AlternateId, old2c.ParentAlternateId);
                Assert.Equal(old1d.AlternateId, old2d.BackId);
                Assert.Equal(old1dd.AlternateId, old2dd.BackId);

                entries = context.ChangeTracker.Entries().ToList();
            }, async context =>
            {
                var loadedRoot = await LoadOptionalAkGraphAsync(context);

                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);

                var loaded1 = await context.Set<OptionalSingleAk1>().SingleAsync(e => e.Id == old1.Id);
                var loaded1d = await context.Set<OptionalSingleAk1>().SingleAsync(e => e.Id == old1d.Id);
                var loaded1dd = await context.Set<OptionalSingleAk1>().SingleAsync(e => e.Id == old1dd.Id);
                var loaded2 = await context.Set<OptionalSingleAk2>().SingleAsync(e => e.Id == old2.Id);
                var loaded2d = await context.Set<OptionalSingleAk2>().SingleAsync(e => e.Id == old2d.Id);
                var loaded2dd = await context.Set<OptionalSingleAk2>().SingleAsync(e => e.Id == old2dd.Id);
                var loaded2c = await context.Set<OptionalSingleComposite2>().SingleAsync(e => e.Id == old2c.Id);

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());

                Assert.Null(loaded1.Root);
                Assert.Null(loaded1d.Root);
                Assert.Null(loaded1dd.Root);
                Assert.Same(loaded1, loaded2.Back);
                Assert.Same(loaded1, loaded2c.Back);
                Assert.Same(loaded1d, loaded2d.Back);
                Assert.Same(loaded1dd, loaded2dd.Back);
                Assert.Null(loaded1.RootId);
                Assert.Null(loaded1d.RootId);
                Assert.Null(loaded1dd.RootId);
                Assert.Equal(loaded1.AlternateId, loaded2.BackId);
                Assert.Equal(loaded1.Id, loaded2c.BackId);
                Assert.Equal(loaded1.AlternateId, loaded2c.ParentAlternateId);
                Assert.Equal(loaded1d.AlternateId, loaded2d.BackId);
                Assert.Equal(loaded1dd.AlternateId, loaded2dd.BackId);
            });
    }

    [ConditionalFact]
    public virtual Task Save_changed_optional_one_to_one_with_alternate_key_in_store()
    {
        var new2 = new OptionalSingleAk2 { AlternateId = Guid.NewGuid() };
        var new2d = new OptionalSingleAk2Derived { AlternateId = Guid.NewGuid() };
        var new2dd = new OptionalSingleAk2MoreDerived { AlternateId = Guid.NewGuid() };
        var new2c = new OptionalSingleComposite2();
        var new1 = new OptionalSingleAk1
        {
            AlternateId = Guid.NewGuid(),
            Single = new2,
            SingleComposite = new2c
        };
        var new1d = new OptionalSingleAk1Derived { AlternateId = Guid.NewGuid(), Single = new2d };
        var new1dd = new OptionalSingleAk1MoreDerived { AlternateId = Guid.NewGuid(), Single = new2dd };
        Root root = null;
        IReadOnlyList<EntityEntry> entries = null;
        OptionalSingleAk1 old1 = null;
        OptionalSingleAk1Derived old1d = null;
        OptionalSingleAk1MoreDerived old1dd = null;
        OptionalSingleAk2 old2 = null;
        OptionalSingleComposite2 old2c = null;
        OptionalSingleAk2Derived old2d = null;
        OptionalSingleAk2MoreDerived old2dd = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadOptionalAkGraphAsync(context);

                old1 = root.OptionalSingleAk;
                old1d = root.OptionalSingleAkDerived;
                old1dd = root.OptionalSingleAkMoreDerived;
                old2 = root.OptionalSingleAk.Single;
                old2c = root.OptionalSingleAk.SingleComposite;
                old2d = (OptionalSingleAk2Derived)root.OptionalSingleAkDerived.Single;
                old2dd = (OptionalSingleAk2MoreDerived)root.OptionalSingleAkMoreDerived.Single;

                using (var context2 = CreateContext())
                {
                    UseTransaction(context2.Database, context.Database.CurrentTransaction);
                    var root2 = await LoadOptionalAkGraphAsync(context2);

                    context2.AddRange(new1, new1d, new1dd, new2, new2d, new2dd, new2c);
                    root2.OptionalSingleAk = new1;
                    root2.OptionalSingleAkDerived = new1d;
                    root2.OptionalSingleAkMoreDerived = new1dd;

                    Assert.True(context2.ChangeTracker.HasChanges());

                    await context2.SaveChangesAsync();

                    Assert.False(context2.ChangeTracker.HasChanges());
                }

                new1 = await context.Set<OptionalSingleAk1>().SingleAsync(e => e.Id == new1.Id);
                new1d = (OptionalSingleAk1Derived)await context.Set<OptionalSingleAk1>().SingleAsync(e => e.Id == new1d.Id);
                new1dd = (OptionalSingleAk1MoreDerived)await context.Set<OptionalSingleAk1>().SingleAsync(e => e.Id == new1dd.Id);
                new2 = await context.Set<OptionalSingleAk2>().SingleAsync(e => e.Id == new2.Id);
                new2c = await context.Set<OptionalSingleComposite2>().SingleAsync(e => e.Id == new2c.Id);
                new2d = (OptionalSingleAk2Derived)await context.Set<OptionalSingleAk2>().SingleAsync(e => e.Id == new2d.Id);
                new2dd = (OptionalSingleAk2MoreDerived)await context.Set<OptionalSingleAk2>().SingleAsync(e => e.Id == new2dd.Id);

                Assert.Equal(root.AlternateId, new1.RootId);
                Assert.Equal(root.AlternateId, new1d.DerivedRootId);
                Assert.Equal(root.AlternateId, new1dd.MoreDerivedRootId);
                Assert.Equal(new1.AlternateId, new2.BackId);
                Assert.Equal(new1.Id, new2c.BackId);
                Assert.Equal(new1.AlternateId, new2c.ParentAlternateId);
                Assert.Equal(new1d.AlternateId, new2d.BackId);
                Assert.Equal(new1dd.AlternateId, new2dd.BackId);
                Assert.Same(root, new1.Root);
                Assert.Same(root, new1d.DerivedRoot);
                Assert.Same(root, new1dd.MoreDerivedRoot);
                Assert.Same(new1, new2.Back);
                Assert.Same(new1, new2c.Back);
                Assert.Same(new1d, new2d.Back);
                Assert.Same(new1dd, new2dd.Back);

                Assert.Null(old1.Root);
                Assert.Null(old1d.DerivedRoot);
                Assert.Null(old1dd.MoreDerivedRoot);
                Assert.Same(old1, old2.Back);
                Assert.Same(old1, old2c.Back);
                Assert.Equal(old1d, old2d.Back);
                Assert.Equal(old1dd, old2dd.Back);
                Assert.Null(old1.RootId);
                Assert.Null(old1d.DerivedRootId);
                Assert.Null(old1dd.MoreDerivedRootId);
                Assert.Equal(old1.AlternateId, old2.BackId);
                Assert.Equal(old1.Id, old2c.BackId);
                Assert.Equal(old1.AlternateId, old2c.ParentAlternateId);
                Assert.Equal(old1d.AlternateId, old2d.BackId);
                Assert.Equal(old1dd.AlternateId, old2dd.BackId);

                Assert.True(context.ChangeTracker.HasChanges());

                await context.SaveChangesAsync();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Equal(root.AlternateId, new1.RootId);
                Assert.Equal(root.AlternateId, new1d.DerivedRootId);
                Assert.Equal(root.AlternateId, new1dd.MoreDerivedRootId);
                Assert.Equal(new1.AlternateId, new2.BackId);
                Assert.Equal(new1.Id, new2c.BackId);
                Assert.Equal(new1.AlternateId, new2c.ParentAlternateId);
                Assert.Equal(new1d.AlternateId, new2d.BackId);
                Assert.Equal(new1dd.AlternateId, new2dd.BackId);
                Assert.Same(root, new1.Root);
                Assert.Same(root, new1d.DerivedRoot);
                Assert.Same(root, new1dd.MoreDerivedRoot);
                Assert.Same(new1, new2.Back);
                Assert.Same(new1, new2c.Back);
                Assert.Same(new1d, new2d.Back);
                Assert.Same(new1dd, new2dd.Back);

                Assert.Null(old1.Root);
                Assert.Null(old1d.DerivedRoot);
                Assert.Null(old1dd.MoreDerivedRoot);
                Assert.Same(old1, old2.Back);
                Assert.Same(old1, old2c.Back);
                Assert.Equal(old1d, old2d.Back);
                Assert.Equal(old1dd, old2dd.Back);
                Assert.Null(old1.RootId);
                Assert.Null(old1d.DerivedRootId);
                Assert.Null(old1dd.MoreDerivedRootId);
                Assert.Equal(old1.AlternateId, old2.BackId);
                Assert.Equal(old1.Id, old2c.BackId);
                Assert.Equal(old1.AlternateId, old2c.ParentAlternateId);
                Assert.Equal(old1d.AlternateId, old2d.BackId);
                Assert.Equal(old1dd.AlternateId, old2dd.BackId);

                entries = context.ChangeTracker.Entries().ToList();
            }, async context =>
            {
                var loadedRoot = await LoadOptionalAkGraphAsync(context);

                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);

                var loaded1 = await context.Set<OptionalSingleAk1>().SingleAsync(e => e.Id == old1.Id);
                var loaded1d = await context.Set<OptionalSingleAk1>().SingleAsync(e => e.Id == old1d.Id);
                var loaded1dd = await context.Set<OptionalSingleAk1>().SingleAsync(e => e.Id == old1dd.Id);
                var loaded2 = await context.Set<OptionalSingleAk2>().SingleAsync(e => e.Id == old2.Id);
                var loaded2d = await context.Set<OptionalSingleAk2>().SingleAsync(e => e.Id == old2d.Id);
                var loaded2dd = await context.Set<OptionalSingleAk2>().SingleAsync(e => e.Id == old2dd.Id);
                var loaded2c = await context.Set<OptionalSingleComposite2>().SingleAsync(e => e.Id == old2c.Id);

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());

                Assert.Null(loaded1.Root);
                Assert.Null(loaded1d.Root);
                Assert.Null(loaded1dd.Root);
                Assert.Same(loaded1, loaded2.Back);
                Assert.Same(loaded1, loaded2c.Back);
                Assert.Same(loaded1d, loaded2d.Back);
                Assert.Same(loaded1dd, loaded2dd.Back);
                Assert.Null(loaded1.RootId);
                Assert.Null(loaded1d.RootId);
                Assert.Null(loaded1dd.RootId);
                Assert.Equal(loaded1.AlternateId, loaded2.BackId);
                Assert.Equal(loaded1.Id, loaded2c.BackId);
                Assert.Equal(loaded1.AlternateId, loaded2c.ParentAlternateId);
                Assert.Equal(loaded1d.AlternateId, loaded2d.BackId);
                Assert.Equal(loaded1dd.AlternateId, loaded2dd.BackId);
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
    public virtual Task Sever_optional_one_to_one_with_alternate_key(
        ChangeMechanism changeMechanism,
        CascadeTiming? deleteOrphansTiming)
    {
        Root root = null;
        OptionalSingleAk1 old1 = null;
        OptionalSingleAk2 old2 = null;
        OptionalSingleComposite2 old2c = null;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                root = await LoadOptionalAkGraphAsync(context);

                old1 = root.OptionalSingleAk;
                old2 = root.OptionalSingleAk.Single;
                old2c = root.OptionalSingleAk.SingleComposite;

                if ((changeMechanism & ChangeMechanism.Principal) != 0)
                {
                    root.OptionalSingleAk = null;
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    old1.Root = null;
                }

                if ((changeMechanism & ChangeMechanism.Fk) != 0)
                {
                    old1.RootId = null;
                }

                Assert.False(context.Entry(root).Reference(e => e.OptionalSingleAk).IsLoaded);
                Assert.False(context.Entry(old1).Reference(e => e.Root).IsLoaded);
                Assert.True(context.ChangeTracker.HasChanges());

                await context.SaveChangesAsync();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Null(old1.Root);
                Assert.Same(old1, old2.Back);
                Assert.Same(old1, old2c.Back);
                Assert.Null(old1.RootId);
                Assert.Equal(old1.AlternateId, old2.BackId);
                Assert.Equal(old1.Id, old2c.BackId);
                Assert.Equal(old1.AlternateId, old2c.ParentAlternateId);
            }, async context =>
            {
                if ((changeMechanism & ChangeMechanism.Fk) == 0)
                {
                    var loadedRoot = await LoadOptionalAkGraphAsync(context);

                    AssertKeys(root, loadedRoot);
                    AssertPossiblyNullNavigations(loadedRoot);

                    var loaded1 = await context.Set<OptionalSingleAk1>().SingleAsync(e => e.Id == old1.Id);
                    var loaded2 = await context.Set<OptionalSingleAk2>().SingleAsync(e => e.Id == old2.Id);
                    var loaded2c = await context.Set<OptionalSingleComposite2>().SingleAsync(e => e.Id == old2c.Id);

                    Assert.Null(loaded1.Root);
                    Assert.Same(loaded1, loaded2.Back);
                    Assert.Same(loaded1, loaded2c.Back);
                    Assert.Null(loaded1.RootId);
                    Assert.Equal(loaded1.AlternateId, loaded2.BackId);
                    Assert.Equal(loaded1.Id, loaded2c.BackId);
                    Assert.Equal(loaded1.AlternateId, loaded2c.ParentAlternateId);
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
    public virtual Task Reparent_optional_one_to_one_with_alternate_key(
        ChangeMechanism changeMechanism,
        bool useExistingRoot,
        CascadeTiming? deleteOrphansTiming)
    {
        var newRoot = new Root { AlternateId = Guid.NewGuid() };
        Root root = null;
        OptionalSingleAk1 old1 = null;
        OptionalSingleAk2 old2 = null;
        OptionalSingleComposite2 old2c = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                if (useExistingRoot)
                {
                    context.Add(newRoot);
                    await context.SaveChangesAsync();
                }
            }, async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                root = await LoadOptionalAkGraphAsync(context);

                context.Entry(newRoot).State = useExistingRoot ? EntityState.Unchanged : EntityState.Added;

                old1 = root.OptionalSingleAk;
                old2 = root.OptionalSingleAk.Single;
                old2c = root.OptionalSingleAk.SingleComposite;

                if ((changeMechanism & ChangeMechanism.Principal) != 0)
                {
                    newRoot.OptionalSingleAk = old1;
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    old1.Root = newRoot;
                }

                if ((changeMechanism & ChangeMechanism.Fk) != 0)
                {
                    old1.RootId = newRoot.AlternateId;
                }

                Assert.True(context.ChangeTracker.HasChanges());

                await context.SaveChangesAsync();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Null(root.OptionalSingleAk);

                Assert.Same(newRoot, old1.Root);
                Assert.Same(old1, old2.Back);
                Assert.Same(old1, old2c.Back);
                Assert.Equal(newRoot.AlternateId, old1.RootId);
                Assert.Equal(old1.AlternateId, old2.BackId);
                Assert.Equal(old1.Id, old2c.BackId);
                Assert.Equal(old1.AlternateId, old2c.ParentAlternateId);
            }, async context =>
            {
                var loadedRoot = await LoadOptionalAkGraphAsync(context);

                AssertKeys(root, loadedRoot);
                AssertPossiblyNullNavigations(loadedRoot);

                newRoot = await context.Set<Root>().SingleAsync(e => e.Id == newRoot.Id);
                var loaded1 = await context.Set<OptionalSingleAk1>().SingleAsync(e => e.Id == old1.Id);
                var loaded2 = await context.Set<OptionalSingleAk2>().SingleAsync(e => e.Id == old2.Id);
                var loaded2c = await context.Set<OptionalSingleComposite2>().SingleAsync(e => e.Id == old2c.Id);

                Assert.Same(newRoot, loaded1.Root);
                Assert.Same(loaded1, loaded2.Back);
                Assert.Same(loaded1, loaded2c.Back);
                Assert.Equal(newRoot.AlternateId, loaded1.RootId);
                Assert.Equal(loaded1.AlternateId, loaded2.BackId);
                Assert.Equal(loaded1.Id, loaded2c.BackId);
                Assert.Equal(loaded1.AlternateId, loaded2c.ParentAlternateId);
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
    public virtual Task Optional_one_to_one_with_alternate_key_are_orphaned(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;
        var orphanedIdC = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await LoadOptionalAkGraphAsync(context);

                var removed = root.OptionalSingleAk;

                removedId = removed.Id;
                var orphaned = removed.Single;
                var orphanedC = removed.SingleComposite;
                orphanedId = orphaned.Id;
                orphanedIdC = orphanedC.Id;

                context.Remove(removed);

                Assert.True(context.ChangeTracker.HasChanges());

                if (cascadeDeleteTiming == null)
                {
                    Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(orphanedC).State);

                    context.ChangeTracker.CascadeChanges();

                    if (Fixture.ForceClientNoAction)
                    {
                        Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(orphanedC).State);
                    }
                    else
                    {
                        Assert.Equal(EntityState.Modified, context.Entry(orphaned).State);
                        Assert.Equal(EntityState.Modified, context.Entry(orphanedC).State);
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
                    Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(orphanedC).State);

                    Assert.Null(root.OptionalSingleAk);

                    Assert.Empty(context.Set<OptionalSingleAk1>().Where(e => e.Id == removedId));
                    Assert.Equal(1, context.Set<OptionalSingleAk2>().Count(e => e.Id == orphanedId));
                    Assert.Equal(1, context.Set<OptionalSingleComposite2>().Count(e => e.Id == orphanedIdC));

                    Assert.Same(root, removed.Root);
                    Assert.Same(orphaned, removed.Single);
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction)
                {
                    var root = await LoadOptionalAkGraphAsync(context);

                    Assert.Null(root.OptionalSingleAk);

                    Assert.Empty(context.Set<OptionalSingleAk1>().Where(e => e.Id == removedId));
                    Assert.Equal(1, context.Set<OptionalSingleAk2>().Count(e => e.Id == orphanedId));
                    Assert.Equal(1, context.Set<OptionalSingleComposite2>().Count(e => e.Id == orphanedIdC));
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
    public virtual Task Optional_one_to_one_with_alternate_key_are_orphaned_in_store(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;
        var orphanedIdC = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var removed = (await LoadOptionalAkGraphAsync(context)).OptionalSingleAk;

                removedId = removed.Id;
                orphanedId = removed.Single.Id;
                orphanedIdC = removed.SingleComposite.Id;
            }, async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await context.Set<Root>().Include(e => e.OptionalSingleAk).SingleAsync(IsTheRoot);

                var removed = root.OptionalSingleAk;
                var orphaned = removed.Single;

                context.Remove(removed);

                // Cannot have SET NULL action in the store because one of the FK columns
                // is not nullable, so need to do this on the EF side.
                (await context.Set<OptionalSingleComposite2>().SingleAsync(e => e.Id == orphanedIdC)).BackId = null;

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

                    Assert.Null(root.OptionalSingleAk);

                    Assert.Empty(await context.Set<OptionalSingleAk1>().Where(e => e.Id == removedId).ToListAsync());
                    Assert.Null((await context.Set<OptionalSingleAk2>().SingleAsync(e => e.Id == orphanedId)).BackId);
                    Assert.Null((await context.Set<OptionalSingleComposite2>().SingleAsync(e => e.Id == orphanedIdC)).BackId);

                    Assert.Same(root, removed.Root);
                    Assert.Same(orphaned, removed.Single);
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction)
                {
                    var root = await LoadOptionalAkGraphAsync(context);

                    Assert.Null(root.OptionalSingleAk);

                    Assert.Empty(await context.Set<OptionalSingleAk1>().Where(e => e.Id == removedId).ToListAsync());
                    Assert.Null((await context.Set<OptionalSingleAk2>().SingleAsync(e => e.Id == orphanedId)).BackId);
                    Assert.Null((await context.Set<OptionalSingleComposite2>().SingleAsync(e => e.Id == orphanedIdC)).BackId);
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
    public virtual Task Optional_one_to_one_with_alternate_key_are_orphaned_starting_detached(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;
        var orphanedIdC = 0;
        Root root = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context => root = await LoadOptionalAkGraphAsync(context), async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var removed = root.OptionalSingleAk;

                removedId = removed.Id;
                var orphaned = removed.Single;
                var orphanedC = removed.SingleComposite;
                orphanedId = orphaned.Id;
                orphanedIdC = orphanedC.Id;

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);

                if (cascadeDeleteTiming == null)
                {
                    Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(orphanedC).State);

                    context.ChangeTracker.CascadeChanges();
                }

                var expectedState = cascadeDeleteTiming is CascadeTiming.Immediate or null
                    && !Fixture.ForceClientNoAction
                        ? EntityState.Modified
                        : EntityState.Unchanged;

                Assert.Equal(expectedState, context.Entry(orphaned).State);
                Assert.Equal(expectedState, context.Entry(orphanedC).State);

                Assert.True(context.ChangeTracker.HasChanges());

                if (Fixture.ForceClientNoAction)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(async () => await context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(orphanedC).State);

                    Assert.Same(root, removed.Root);
                    Assert.Same(orphaned, removed.Single);
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction)
                {
                    root = await LoadOptionalAkGraphAsync(context);

                    Assert.Null(root.OptionalSingleAk);

                    Assert.Empty(context.Set<OptionalSingleAk1>().Where(e => e.Id == removedId));
                    Assert.Equal(1, context.Set<OptionalSingleAk2>().Count(e => e.Id == orphanedId));
                    Assert.Equal(1, context.Set<OptionalSingleComposite2>().Count(e => e.Id == orphanedIdC));
                }
            });
    }

    [ConditionalTheory]
    [InlineData(CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.Never)]
    [InlineData(null)]
    public virtual Task Required_one_to_one_with_AK_relationships_are_one_to_one(
        CascadeTiming? deleteOrphansTiming)
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await context.Set<Root>().SingleAsync(IsTheRoot);

                Assert.False(context.ChangeTracker.HasChanges());

                root.RequiredSingleAk = new RequiredSingleAk1();

                Assert.True(context.ChangeTracker.HasChanges());

                await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            });

    [ConditionalTheory]
    [InlineData((int)ChangeMechanism.Principal, false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Principal, true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Dependent, false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Dependent, true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false, CascadeTiming.OnSaveChanges)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true, CascadeTiming.OnSaveChanges)]
    [InlineData((int)ChangeMechanism.Principal, false, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Principal, true, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Dependent, false, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Dependent, true, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false, CascadeTiming.Immediate)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true, CascadeTiming.Immediate)]
    [InlineData((int)ChangeMechanism.Principal, false, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Principal, true, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Dependent, false, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Dependent, true, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false, CascadeTiming.Never)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true, CascadeTiming.Never)]
    [InlineData((int)ChangeMechanism.Principal, false, null)]
    [InlineData((int)ChangeMechanism.Principal, true, null)]
    [InlineData((int)ChangeMechanism.Dependent, false, null)]
    [InlineData((int)ChangeMechanism.Dependent, true, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false, null)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true, null)]
    public virtual Task Save_required_one_to_one_changed_by_reference_with_alternate_key(
        ChangeMechanism changeMechanism,
        bool useExistingEntities,
        CascadeTiming? deleteOrphansTiming)
    {
        var new2 = new RequiredSingleAk2 { AlternateId = Guid.NewGuid() };
        var new2c = new RequiredSingleComposite2();
        var new1 = new RequiredSingleAk1
        {
            AlternateId = Guid.NewGuid(),
            Single = new2,
            SingleComposite = new2c
        };
        var newRoot = new Root { AlternateId = Guid.NewGuid(), RequiredSingleAk = new1 };
        Root root = null;
        IReadOnlyList<EntityEntry> entries = null;
        RequiredSingleAk1 old1 = null;
        RequiredSingleAk2 old2 = null;
        RequiredSingleComposite2 old2c = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                if (useExistingEntities)
                {
                    context.AddRange(newRoot, new1, new2, new2c);
                    await context.SaveChangesAsync();
                }
            }, async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                root = await LoadRequiredAkGraphAsync(context);

                old1 = root.RequiredSingleAk;
                old2 = root.RequiredSingleAk.Single;
                old2c = root.RequiredSingleAk.SingleComposite;

                if (useExistingEntities)
                {
                    newRoot = await context.Set<Root>()
                        .Include(e => e.RequiredSingleAk).ThenInclude(e => e.Single)
                        .Include(e => e.RequiredSingleAk).ThenInclude(e => e.SingleComposite)
                        .OrderBy(e => e.Id)
                        .SingleAsync(e => e.Id == newRoot.Id);

                    new1 = newRoot.RequiredSingleAk;
                    new2 = new1.Single;
                    new2c = new1.SingleComposite;
                }
                else
                {
                    context.AddRange(new1, new2, new2c);
                }

                if ((changeMechanism & ChangeMechanism.Principal) != 0)
                {
                    root.RequiredSingleAk = new1;
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    new1.Root = root;
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

                    Assert.Equal(
                        message,
                        CoreStrings.RelationshipConceptualNullSensitive(
                            nameof(Root), nameof(RequiredSingleAk1), "{RootId: " + old1.RootId + "}"));
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

                    Assert.Equal(root.AlternateId, new1.RootId);
                    Assert.Equal(new1.AlternateId, new2.BackId);
                    Assert.Equal(new1.Id, new2c.BackId);
                    Assert.Equal(new1.AlternateId, new2c.BackAlternateId);
                    Assert.Same(root, new1.Root);
                    Assert.Same(new1, new2.Back);
                    Assert.Same(new1, new2c.Back);

                    Assert.Equal(EntityState.Detached, context.Entry(old1).State);
                    Assert.Equal(EntityState.Detached, context.Entry(old2).State);
                    Assert.Equal(EntityState.Detached, context.Entry(old2c).State);
                    Assert.Null(old1.Root);
                    if (!context.Entry(old2).Metadata.IsOwned())
                    {
                        // Navigations to owners are preserved when these are owned
                        Assert.Null(old2.Back);
                        Assert.Null(old2c.Back);
                    }

                    Assert.NotEqual(new1.Id, old1.Id);
                    Assert.NotEqual(new2.Id, old2.Id);
                    Assert.NotEqual(new2c.Id, old2c.Id);
                    Assert.Equal(old1.AlternateId, old2.BackId);
                    Assert.Equal(old1.Id, old2c.BackId);
                    Assert.Equal(old1.AlternateId, old2c.BackAlternateId);

                    context.Entry(newRoot).State = EntityState.Detached;
                    entries = context.ChangeTracker.Entries().ToList();
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction
                    && !Fixture.NoStoreCascades
                    && deleteOrphansTiming != CascadeTiming.Never)
                {
                    var loadedRoot = await LoadRequiredAkGraphAsync(context);

                    AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                    AssertKeys(root, loadedRoot);
                    AssertNavigations(loadedRoot);

                    var removedCount = context.Set<Root>().Select(r => r.RequiredSingleAk).Count(e => e.Id == old1.Id);
                    Assert.Equal(0, removedCount);

                    Assert.False(context.Set<Root>().Any(r => r.RequiredSingleAk != null && r.RequiredSingleAk.Id == old1.Id));

                    var orphanedCount = context.Set<Root>().Select(r => r.RequiredSingleAk).Select(r => r.Single)
                        .Count(e => e.Id == old2.Id);
                    Assert.Equal(0, orphanedCount);

                    Assert.False(
                        context.Set<Root>().Select(r => r.RequiredSingleAk)
                            .Any(r => r.Single != null && r.Single.Id == old2.Id));

                    var orphanedCCount = context.Set<Root>().Select(r => r.RequiredSingleAk).Select(r => r.SingleComposite)
                        .Count(e => e.Id == old2c.Id);
                    Assert.Equal(0, orphanedCCount);

                    Assert.False(
                        context.Set<Root>().Select(r => r.RequiredSingleAk)
                            .Any(r => r.SingleComposite != null && r.SingleComposite.Id == old2c.Id));
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
    public virtual Task Save_required_non_PK_one_to_one_changed_by_reference_with_alternate_key(
        ChangeMechanism changeMechanism,
        bool useExistingEntities,
        CascadeTiming? deleteOrphansTiming)
    {
        var new2 = new RequiredNonPkSingleAk2 { AlternateId = Guid.NewGuid() };
        var new2d = new RequiredNonPkSingleAk2Derived { AlternateId = Guid.NewGuid() };
        var new2dd = new RequiredNonPkSingleAk2MoreDerived { AlternateId = Guid.NewGuid() };
        var new1 = new RequiredNonPkSingleAk1 { AlternateId = Guid.NewGuid(), Single = new2 };
        var new1d = new RequiredNonPkSingleAk1Derived
        {
            AlternateId = Guid.NewGuid(),
            Single = new2d,
            Root = new Root()
        };
        var new1dd = new RequiredNonPkSingleAk1MoreDerived
        {
            AlternateId = Guid.NewGuid(),
            Single = new2dd,
            Root = new Root(),
            DerivedRoot = new Root()
        };
        var newRoot = new Root
        {
            AlternateId = Guid.NewGuid(),
            RequiredNonPkSingleAk = new1,
            RequiredNonPkSingleAkDerived = new1d,
            RequiredNonPkSingleAkMoreDerived = new1dd
        };
        Root root = null;
        IReadOnlyList<EntityEntry> entries = null;
        RequiredNonPkSingleAk1 old1 = null;
        RequiredNonPkSingleAk1Derived old1d = null;
        RequiredNonPkSingleAk1MoreDerived old1dd = null;
        RequiredNonPkSingleAk2 old2 = null;
        RequiredNonPkSingleAk2Derived old2d = null;
        RequiredNonPkSingleAk2MoreDerived old2dd = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                if (useExistingEntities)
                {
                    context.AddRange(newRoot, new1, new1d, new1dd, new2, new2d, new2dd);
                    await context.SaveChangesAsync();
                }
            }, async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                root = await LoadRequiredNonPkAkGraphAsync(context);

                old1 = root.RequiredNonPkSingleAk;
                old1d = root.RequiredNonPkSingleAkDerived;
                old1dd = root.RequiredNonPkSingleAkMoreDerived;
                old2 = root.RequiredNonPkSingleAk.Single;
                old2d = (RequiredNonPkSingleAk2Derived)root.RequiredNonPkSingleAkDerived.Single;
                old2dd = (RequiredNonPkSingleAk2MoreDerived)root.RequiredNonPkSingleAkMoreDerived.Single;

                context.Set<RequiredNonPkSingleAk1>().Remove(old1d);
                context.Set<RequiredNonPkSingleAk1>().Remove(old1dd);

                if (useExistingEntities)
                {
                    new1 = await context.Set<RequiredNonPkSingleAk1>().SingleAsync(e => e.Id == new1.Id);
                    new1d = (RequiredNonPkSingleAk1Derived)await context.Set<RequiredNonPkSingleAk1>().SingleAsync(e => e.Id == new1d.Id);
                    new1dd = (RequiredNonPkSingleAk1MoreDerived)await context.Set<RequiredNonPkSingleAk1>()
                        .SingleAsync(e => e.Id == new1dd.Id);
                    new2 = await context.Set<RequiredNonPkSingleAk2>().SingleAsync(e => e.Id == new2.Id);
                    new2d = (RequiredNonPkSingleAk2Derived)await context.Set<RequiredNonPkSingleAk2>().SingleAsync(e => e.Id == new2d.Id);
                    new2dd = (RequiredNonPkSingleAk2MoreDerived)await context.Set<RequiredNonPkSingleAk2>()
                        .SingleAsync(e => e.Id == new2dd.Id);

                    new1d.RootId = old1d.RootId;
                    new1dd.RootId = old1dd.RootId;
                    new1dd.DerivedRootId = old1dd.DerivedRootId;
                }
                else
                {
                    new1d.Root = old1d.Root;
                    new1dd.Root = old1dd.Root;
                    new1dd.DerivedRoot = old1dd.DerivedRoot;
                    context.AddRange(new1, new1d, new1dd, new2, new2d, new2dd);
                }

                if ((changeMechanism & ChangeMechanism.Principal) != 0)
                {
                    root.RequiredNonPkSingleAk = new1;
                    root.RequiredNonPkSingleAkDerived = new1d;
                    root.RequiredNonPkSingleAkMoreDerived = new1dd;
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    new1.Root = root;
                    new1d.DerivedRoot = root;
                    new1dd.MoreDerivedRoot = root;
                }

                if ((changeMechanism & ChangeMechanism.Fk) != 0)
                {
                    new1.RootId = root.AlternateId;
                    new1d.DerivedRootId = root.AlternateId;
                    new1dd.MoreDerivedRootId = root.AlternateId;
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

                    Assert.Equal(
                        message,
                        CoreStrings.RelationshipConceptualNullSensitive(
                            nameof(Root), nameof(RequiredNonPkSingleAk1), "{RootId: " + old1.RootId + "}"));
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

                    Assert.Equal(root.AlternateId, new1.RootId);
                    Assert.Equal(root.AlternateId, new1d.DerivedRootId);
                    Assert.Equal(root.AlternateId, new1dd.MoreDerivedRootId);
                    Assert.Equal(new1.AlternateId, new2.BackId);
                    Assert.Equal(new1d.AlternateId, new2d.BackId);
                    Assert.Equal(new1dd.AlternateId, new2dd.BackId);
                    Assert.Same(root, new1.Root);
                    Assert.Same(root, new1d.DerivedRoot);
                    Assert.Same(root, new1dd.MoreDerivedRoot);
                    Assert.Same(new1, new2.Back);
                    Assert.Same(new1d, new2d.Back);
                    Assert.Same(new1dd, new2dd.Back);

                    Assert.Null(old1.Root);
                    Assert.Null(old1d.DerivedRoot);
                    Assert.Null(old1dd.MoreDerivedRoot);
                    Assert.Null(old2.Back);
                    Assert.Null(old2d.Back);
                    Assert.Null(old2dd.Back);
                    Assert.Equal(old1.AlternateId, old2.BackId);
                    Assert.Equal(old1d.AlternateId, old2d.BackId);
                    Assert.Equal(old1dd.AlternateId, old2dd.BackId);

                    entries = context.ChangeTracker.Entries().ToList();
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction
                    && !Fixture.NoStoreCascades
                    && deleteOrphansTiming != CascadeTiming.Never)
                {
                    var loadedRoot = await LoadRequiredNonPkAkGraphAsync(context);

                    AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                    AssertKeys(root, loadedRoot);
                    AssertNavigations(loadedRoot);

                    Assert.False(context.Set<RequiredNonPkSingleAk1>().Any(e => e.Id == old1.Id));
                    Assert.False(context.Set<RequiredNonPkSingleAk1>().Any(e => e.Id == old1d.Id));
                    Assert.False(context.Set<RequiredNonPkSingleAk1>().Any(e => e.Id == old1dd.Id));
                    Assert.False(context.Set<RequiredNonPkSingleAk2>().Any(e => e.Id == old2.Id));
                    Assert.False(context.Set<RequiredNonPkSingleAk2>().Any(e => e.Id == old2d.Id));
                    Assert.False(context.Set<RequiredNonPkSingleAk2>().Any(e => e.Id == old2dd.Id));
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
    public virtual Task Sever_required_one_to_one_with_alternate_key(
        ChangeMechanism changeMechanism,
        CascadeTiming? deleteOrphansTiming)
    {
        Root root = null;
        RequiredSingleAk1 old1 = null;
        RequiredSingleAk2 old2 = null;
        RequiredSingleComposite2 old2c = null;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                root = await LoadRequiredAkGraphAsync(context);

                old1 = root.RequiredSingleAk;
                old2 = root.RequiredSingleAk.Single;
                old2c = root.RequiredSingleAk.SingleComposite;

                if ((changeMechanism & ChangeMechanism.Principal) != 0)
                {
                    root.RequiredSingleAk = null;
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    old1.Root = null;
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

                    Assert.Equal(
                        message,
                        CoreStrings.RelationshipConceptualNullSensitive(
                            nameof(Root), nameof(RequiredSingleAk1), "{RootId: " + old1.RootId + "}"));
                }
                else
                {
                    Assert.False(context.Entry(root).Reference(e => e.RequiredSingleAk).IsLoaded);
                    Assert.False(context.Entry(old1).Reference(e => e.Root).IsLoaded);
                    Assert.True(context.ChangeTracker.HasChanges());

                    if (deleteOrphansTiming == null)
                    {
                        context.ChangeTracker.CascadeChanges();
                    }

                    await context.SaveChangesAsync();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Null(old1.Root);
                    if (!context.Entry(old2).Metadata.IsOwned())
                    {
                        // Navigations to owners are preserved when these are owned
                        Assert.Null(old2.Back);
                        Assert.Null(old2c.Back);
                    }

                    Assert.Equal(old1.AlternateId, old2.BackId);
                    Assert.Equal(old1.Id, old2c.BackId);
                    Assert.Equal(old1.AlternateId, old2c.BackAlternateId);
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction
                    && !Fixture.NoStoreCascades
                    && deleteOrphansTiming != CascadeTiming.Never)
                {
                    var loadedRoot = await LoadRequiredAkGraphAsync(context);

                    AssertKeys(root, loadedRoot);
                    AssertPossiblyNullNavigations(loadedRoot);

                    var removedCount = context.Set<Root>().Select(r => r.RequiredSingleAk).Count(e => e.Id == old1.Id);
                    Assert.Equal(0, removedCount);

                    Assert.False(context.Set<Root>().Any(r => r.RequiredSingleAk != null && r.RequiredSingleAk.Id == old1.Id));

                    var orphanedCount = context.Set<Root>().Select(r => r.RequiredSingleAk).Select(r => r.Single)
                        .Count(e => e.Id == old2.Id);
                    Assert.Equal(0, orphanedCount);

                    Assert.False(
                        context.Set<Root>().Select(r => r.RequiredSingleAk)
                            .Any(r => r.Single != null && r.Single.Id == old2.Id));

                    var orphanedCCount = context.Set<Root>().Select(r => r.RequiredSingleAk).Select(r => r.SingleComposite)
                        .Count(e => e.Id == old2c.Id);
                    Assert.Equal(0, orphanedCCount);

                    Assert.False(
                        context.Set<Root>().Select(r => r.RequiredSingleAk)
                            .Any(r => r.SingleComposite != null && r.SingleComposite.Id == old2c.Id));
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
    public virtual Task Sever_required_non_PK_one_to_one_with_alternate_key(
        ChangeMechanism changeMechanism,
        CascadeTiming? deleteOrphansTiming)
    {
        Root root = null;
        RequiredNonPkSingleAk1 old1 = null;
        RequiredNonPkSingleAk2 old2 = null;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                root = await LoadRequiredNonPkAkGraphAsync(context);

                old1 = root.RequiredNonPkSingleAk;
                old2 = root.RequiredNonPkSingleAk.Single;

                if ((changeMechanism & ChangeMechanism.Principal) != 0)
                {
                    root.RequiredNonPkSingleAk = null;
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    old1.Root = null;
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

                    Assert.Equal(
                        message,
                        CoreStrings.RelationshipConceptualNullSensitive(
                            nameof(Root), nameof(RequiredNonPkSingleAk1), "{RootId: " + old1.RootId + "}"));
                }
                else
                {
                    context.ChangeTracker.DetectChanges();
                    context.ChangeTracker.DetectChanges();
                    Assert.False(context.Entry(root).Reference(e => e.RequiredNonPkSingleAk).IsLoaded);
                    Assert.False(context.Entry(old1).Reference(e => e.Root).IsLoaded);
                    Assert.True(context.ChangeTracker.HasChanges());

                    if (deleteOrphansTiming == null)
                    {
                        context.ChangeTracker.CascadeChanges();
                    }

                    await context.SaveChangesAsync();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Null(old1.Root);
                    Assert.Null(old2.Back);
                    Assert.Equal(old1.AlternateId, old2.BackId);
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction
                    && !Fixture.NoStoreCascades
                    && deleteOrphansTiming != CascadeTiming.Never)
                {
                    var loadedRoot = await LoadRequiredNonPkAkGraphAsync(context);

                    AssertKeys(root, loadedRoot);
                    AssertPossiblyNullNavigations(loadedRoot);

                    Assert.False(context.Set<RequiredNonPkSingleAk1>().Any(e => e.Id == old1.Id));
                    Assert.False(context.Set<RequiredNonPkSingleAk2>().Any(e => e.Id == old2.Id));
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
    public virtual Task Reparent_required_one_to_one_with_alternate_key(
        ChangeMechanism changeMechanism,
        bool useExistingRoot,
        CascadeTiming? deleteOrphansTiming)
    {
        var newRoot = new Root { AlternateId = Guid.NewGuid() };
        Root root = null;
        RequiredSingleAk1 old1 = null;
        RequiredSingleAk2 old2 = null;
        RequiredSingleComposite2 old2c = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                if (useExistingRoot)
                {
                    context.Add(newRoot);
                    await context.SaveChangesAsync();
                }
            }, async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                root = await LoadRequiredAkGraphAsync(context);

                context.Entry(newRoot).State = useExistingRoot ? EntityState.Unchanged : EntityState.Added;

                old1 = root.RequiredSingleAk;
                old2 = root.RequiredSingleAk.Single;
                old2c = root.RequiredSingleAk.SingleComposite;

                if ((changeMechanism & ChangeMechanism.Principal) != 0)
                {
                    newRoot.RequiredSingleAk = old1;
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    old1.Root = newRoot;
                }

                if ((changeMechanism & ChangeMechanism.Fk) != 0)
                {
                    old1.RootId = newRoot.AlternateId;
                }

                Assert.True(context.ChangeTracker.HasChanges());

                await context.SaveChangesAsync();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Null(root.RequiredSingleAk);

                Assert.Same(newRoot, old1.Root);
                Assert.Same(old1, old2.Back);
                Assert.Same(old1, old2c.Back);
                Assert.Equal(newRoot.AlternateId, old1.RootId);
                Assert.Equal(old1.AlternateId, old2.BackId);
                Assert.Equal(old1.Id, old2c.BackId);
                Assert.Equal(old1.AlternateId, old2c.BackAlternateId);
            }, async context =>
            {
                var loadedRoot = await LoadRequiredAkGraphAsync(context);

                AssertKeys(root, loadedRoot);
                AssertPossiblyNullNavigations(loadedRoot);

                newRoot = await context.Set<Root>()
                    .Include(r => r.RequiredSingleAk.Single)
                    .Include(r => r.RequiredSingleAk.SingleComposite)
                    .SingleAsync(e => e.Id == newRoot.Id);

                var loaded1 = newRoot.RequiredSingleAk;
                var loaded2 = loaded1.Single;
                var loaded2c = loaded1.SingleComposite;

                Assert.Equal(old1.Id, loaded1.Id);
                Assert.Equal(old2.Id, loaded2.Id);
                Assert.Equal(old2c.Id, loaded2c.Id);

                Assert.Same(newRoot, loaded1.Root);
                Assert.Same(loaded1, loaded2.Back);
                Assert.Same(loaded1, loaded2c.Back);
                Assert.Equal(newRoot.AlternateId, loaded1.RootId);
                Assert.Equal(loaded1.AlternateId, loaded2.BackId);
                Assert.Equal(loaded1.Id, loaded2c.BackId);
                Assert.Equal(loaded1.AlternateId, loaded2c.BackAlternateId);
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
    public virtual Task Reparent_required_non_PK_one_to_one_with_alternate_key(
        ChangeMechanism changeMechanism,
        bool useExistingRoot,
        CascadeTiming? deleteOrphansTiming)
    {
        var newRoot = new Root { AlternateId = Guid.NewGuid() };
        Root root = null;
        RequiredNonPkSingleAk1 old1 = null;
        RequiredNonPkSingleAk2 old2 = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                if (useExistingRoot)
                {
                    context.Add(newRoot);
                    await context.SaveChangesAsync();
                }
            }, async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                root = await LoadRequiredNonPkAkGraphAsync(context);

                context.Entry(newRoot).State = useExistingRoot ? EntityState.Unchanged : EntityState.Added;

                old1 = root.RequiredNonPkSingleAk;
                old2 = root.RequiredNonPkSingleAk.Single;

                if ((changeMechanism & ChangeMechanism.Principal) != 0)
                {
                    newRoot.RequiredNonPkSingleAk = old1;
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    old1.Root = newRoot;
                }

                if ((changeMechanism & ChangeMechanism.Fk) != 0)
                {
                    old1.RootId = newRoot.AlternateId;
                }

                Assert.True(context.ChangeTracker.HasChanges());

                await context.SaveChangesAsync();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Null(root.RequiredNonPkSingleAk);

                Assert.Same(newRoot, old1.Root);
                Assert.Same(old1, old2.Back);
                Assert.Equal(newRoot.AlternateId, old1.RootId);
                Assert.Equal(old1.AlternateId, old2.BackId);
            }, async context =>
            {
                var loadedRoot = await LoadRequiredNonPkAkGraphAsync(context);

                AssertKeys(root, loadedRoot);
                AssertPossiblyNullNavigations(loadedRoot);

                newRoot = await context.Set<Root>().SingleAsync(e => e.Id == newRoot.Id);
                var loaded1 = await context.Set<RequiredNonPkSingleAk1>().SingleAsync(e => e.Id == old1.Id);
                var loaded2 = await context.Set<RequiredNonPkSingleAk2>().SingleAsync(e => e.Id == old2.Id);

                Assert.Same(newRoot, loaded1.Root);
                Assert.Same(loaded1, loaded2.Back);
                Assert.Equal(newRoot.AlternateId, loaded1.RootId);
                Assert.Equal(loaded1.AlternateId, loaded2.BackId);
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
    public virtual Task Required_one_to_one_with_alternate_key_are_cascade_deleted(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;
        var orphanedIdC = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await LoadRequiredAkGraphAsync(context);

                var removed = root.RequiredSingleAk;

                removedId = removed.Id;
                var orphaned = removed.Single;
                var orphanedC = removed.SingleComposite;
                orphanedId = orphaned.Id;
                orphanedIdC = orphanedC.Id;

                context.Remove(removed);

                Assert.True(context.ChangeTracker.HasChanges());

                if (cascadeDeleteTiming == null)
                {
                    Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(orphanedC).State);

                    context.ChangeTracker.CascadeChanges();

                    if (Fixture.ForceClientNoAction)
                    {
                        Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(orphanedC).State);
                    }
                    else
                    {
                        Assert.Equal(EntityState.Deleted, context.Entry(orphaned).State);
                        Assert.Equal(EntityState.Deleted, context.Entry(orphanedC).State);
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
                    Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);
                    Assert.Equal(EntityState.Detached, context.Entry(orphanedC).State);

                    Assert.Null(root.RequiredSingleAk);

                    Assert.Same(root, removed.Root);
                    Assert.Same(orphaned, removed.Single);
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction
                    && cascadeDeleteTiming != CascadeTiming.Never)
                {
                    var root = await LoadRequiredAkGraphAsync(context);

                    Assert.Null(root.RequiredSingleAk);

                    var removedCount = context.Set<Root>().Select(r => r.RequiredSingleAk).Count(e => e.Id == removedId);
                    Assert.Equal(0, removedCount);

                    Assert.False(context.Set<Root>().Any(r => r.RequiredSingleAk != null));

                    var orphanedCount = context.Set<Root>().Select(r => r.RequiredSingleAk).Select(r => r.Single)
                        .Count(e => e.Id == orphanedId);
                    Assert.Equal(0, orphanedCount);

                    Assert.False(context.Set<Root>().Select(r => r.RequiredSingleAk).Any(r => r.Single != null));

                    var orphanedCCount = context.Set<Root>().Select(r => r.RequiredSingleAk).Select(r => r.SingleComposite)
                        .Count(e => e.Id == orphanedId);
                    Assert.Equal(0, orphanedCCount);

                    Assert.False(context.Set<Root>().Select(r => r.RequiredSingleAk).Any(r => r.SingleComposite != null));
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
    public virtual Task Required_non_PK_one_to_one_with_alternate_key_are_cascade_deleted(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await LoadRequiredNonPkAkGraphAsync(context);

                var removed = root.RequiredNonPkSingleAk;

                removedId = removed.Id;
                var orphaned = removed.Single;
                orphanedId = orphaned.Id;

                context.Remove(removed);

                Assert.True(context.ChangeTracker.HasChanges());

                if (cascadeDeleteTiming == null)
                {
                    Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);

                    context.ChangeTracker.CascadeChanges();

                    Assert.Equal(
                        Fixture.ForceClientNoAction ? EntityState.Unchanged : EntityState.Deleted, context.Entry(orphaned).State);
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
                    Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);

                    Assert.Null(root.RequiredNonPkSingleAk);

                    Assert.Empty(context.Set<RequiredNonPkSingleAk1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<RequiredNonPkSingleAk2>().Where(e => e.Id == orphanedId));

                    Assert.Same(root, removed.Root);
                    Assert.Same(orphaned, removed.Single);
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction
                    && cascadeDeleteTiming != CascadeTiming.Never)
                {
                    var root = await LoadRequiredNonPkAkGraphAsync(context);

                    Assert.Null(root.RequiredNonPkSingleAk);

                    Assert.Empty(context.Set<RequiredNonPkSingleAk1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<RequiredNonPkSingleAk2>().Where(e => e.Id == orphanedId));
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
    public virtual Task Required_one_to_one_with_alternate_key_are_cascade_deleted_in_store(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;
        var orphanedIdC = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var removed = (await LoadRequiredAkGraphAsync(context)).RequiredSingleAk;

                removedId = removed.Id;
                orphanedId = removed.Single.Id;
                orphanedIdC = removed.SingleComposite.Id;
            }, async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await context.Set<Root>().Include(e => e.RequiredSingleAk).SingleAsync(IsTheRoot);

                var removed = root.RequiredSingleAk;
                var orphaned = removed.Single;

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

                    Assert.Null(root.RequiredSingleAk);

                    Assert.Same(root, removed.Root);
                    Assert.Same(orphaned, removed.Single);
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction
                    && !Fixture.NoStoreCascades)
                {
                    var root = await LoadRequiredAkGraphAsync(context);

                    Assert.Null(root.RequiredSingleAk);

                    Assert.Empty(context.Set<RequiredSingleAk1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<RequiredSingleAk2>().Where(e => e.Id == orphanedId));
                    Assert.Empty(context.Set<RequiredSingleComposite2>().Where(e => e.Id == orphanedIdC));
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
    public virtual Task Required_non_PK_one_to_one_with_alternate_key_are_cascade_deleted_in_store(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var removed = (await LoadRequiredNonPkAkGraphAsync(context)).RequiredNonPkSingleAk;

                removedId = removed.Id;
                orphanedId = removed.Single.Id;
            }, async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await context.Set<Root>().Include(e => e.RequiredNonPkSingleAk).SingleAsync(IsTheRoot);

                var removed = root.RequiredNonPkSingleAk;
                var orphaned = removed.Single;

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

                    Assert.Null(root.RequiredNonPkSingleAk);

                    Assert.Empty(context.Set<RequiredNonPkSingleAk1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<RequiredNonPkSingleAk2>().Where(e => e.Id == orphanedId));

                    Assert.Same(root, removed.Root);
                    Assert.Same(orphaned, removed.Single);
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction
                    && !Fixture.NoStoreCascades)
                {
                    var root = await LoadRequiredNonPkAkGraphAsync(context);

                    Assert.Null(root.RequiredNonPkSingleAk);

                    Assert.Empty(context.Set<RequiredNonPkSingleAk1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<RequiredNonPkSingleAk2>().Where(e => e.Id == orphanedId));
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
    public virtual Task Required_one_to_one_with_alternate_key_are_cascade_deleted_starting_detached(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;
        var orphanedIdC = 0;
        Root root = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context => root = await LoadRequiredAkGraphAsync(context), async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var removed = root.RequiredSingleAk;

                removedId = removed.Id;
                var orphaned = removed.Single;
                var orphanedC = removed.SingleComposite;
                orphanedId = orphaned.Id;
                orphanedIdC = orphanedC.Id;

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);

                if (cascadeDeleteTiming == null)
                {
                    Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(orphanedC).State);

                    context.ChangeTracker.CascadeChanges();
                }

                var expectedState = cascadeDeleteTiming is CascadeTiming.Immediate or null
                    && !Fixture.ForceClientNoAction
                        ? EntityState.Deleted
                        : EntityState.Unchanged;

                Assert.Equal(expectedState, context.Entry(orphaned).State);
                Assert.Equal(expectedState, context.Entry(orphanedC).State);

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
                    Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);
                    Assert.Equal(EntityState.Detached, context.Entry(orphanedC).State);

                    Assert.Same(root, removed.Root);
                    Assert.Same(orphaned, removed.Single);
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction
                    && cascadeDeleteTiming != CascadeTiming.Never)
                {
                    root = await LoadRequiredAkGraphAsync(context);

                    Assert.Null(root.RequiredSingleAk);

                    var removedCount = context.Set<Root>().Select(r => r.RequiredSingleAk).Count(e => e.Id == removedId);
                    Assert.Equal(0, removedCount);

                    Assert.False(context.Set<Root>().Any(r => r.RequiredSingleAk != null));

                    var orphanedCount = context.Set<Root>().Select(r => r.RequiredSingleAk).Select(r => r.Single)
                        .Count(e => e.Id == orphanedId);
                    Assert.Equal(0, orphanedCount);

                    Assert.False(context.Set<Root>().Select(r => r.RequiredSingleAk).Any(r => r.Single != null));

                    var orphanedCCount = context.Set<Root>().Select(r => r.RequiredSingleAk).Select(r => r.SingleComposite)
                        .Count(e => e.Id == orphanedId);
                    Assert.Equal(0, orphanedCCount);

                    Assert.False(context.Set<Root>().Select(r => r.RequiredSingleAk).Any(r => r.SingleComposite != null));
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
    public virtual Task Required_non_PK_one_to_one_with_alternate_key_are_cascade_deleted_starting_detached(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;
        Root root = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context => root = await LoadRequiredNonPkAkGraphAsync(context), async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var removed = root.RequiredNonPkSingleAk;

                removedId = removed.Id;
                var orphaned = removed.Single;
                orphanedId = orphaned.Id;

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);

                if (cascadeDeleteTiming == null)
                {
                    Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);

                    context.ChangeTracker.CascadeChanges();
                }

                var expectedState = cascadeDeleteTiming is CascadeTiming.Immediate or null
                    && !Fixture.ForceClientNoAction
                        ? EntityState.Deleted
                        : EntityState.Unchanged;

                Assert.Equal(expectedState, context.Entry(orphaned).State);

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
                    Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);

                    Assert.Same(root, removed.Root);
                    Assert.Same(orphaned, removed.Single);
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction
                    && cascadeDeleteTiming != CascadeTiming.Never)
                {
                    root = await LoadRequiredNonPkAkGraphAsync(context);

                    Assert.Null(root.RequiredNonPkSingleAk);

                    Assert.Empty(context.Set<RequiredNonPkSingleAk1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<RequiredNonPkSingleAk2>().Where(e => e.Id == orphanedId));
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
    public virtual Task Required_one_to_one_with_alternate_key_are_cascade_detached_when_Added(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;
        var orphanedIdC = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await LoadRequiredAkGraphAsync(context);

                var removed = root.RequiredSingleAk;
                removedId = removed.Id;

                var orphaned = removed.Single;
                var orphanedC = removed.SingleComposite;

                // Since we're pretending these aren't in the database, make them really not in the database
                context.Entry(orphaned).State = EntityState.Deleted;
                context.Entry(orphanedC).State = EntityState.Deleted;
                await context.SaveChangesAsync();

                Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);
                Assert.Equal(EntityState.Detached, context.Entry(orphanedC).State);

                removed.Single = orphaned;
                removed.SingleComposite = orphanedC;
                context.ChangeTracker.DetectChanges();
                context.Entry(orphaned).State = EntityState.Added;
                context.Entry(orphanedC).State = EntityState.Added;
                orphanedId = orphaned.Id;
                orphanedIdC = orphanedC.Id;

                Assert.Equal(EntityState.Unchanged, context.Entry(removed).State);
                Assert.Equal(EntityState.Added, context.Entry(orphaned).State);
                Assert.Equal(EntityState.Added, context.Entry(orphanedC).State);

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);

                if (cascadeDeleteTiming == null)
                {
                    Assert.Equal(EntityState.Added, context.Entry(orphaned).State);
                    Assert.Equal(EntityState.Added, context.Entry(orphanedC).State);

                    context.ChangeTracker.CascadeChanges();
                }

                var expectedState = cascadeDeleteTiming is CascadeTiming.Immediate or null
                    && !Fixture.ForceClientNoAction
                        ? EntityState.Detached
                        : EntityState.Added;

                Assert.Equal(expectedState, context.Entry(orphaned).State);
                Assert.Equal(expectedState, context.Entry(orphanedC).State);

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
                    Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);
                    Assert.Equal(EntityState.Detached, context.Entry(orphanedC).State);

                    Assert.Same(root, removed.Root);
                    Assert.Same(orphaned, removed.Single);
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction
                    && cascadeDeleteTiming != CascadeTiming.Never)
                {
                    var root = await LoadRequiredAkGraphAsync(context);

                    Assert.Null(root.RequiredSingleAk);

                    var removedCount = context.Set<Root>().Select(r => r.RequiredSingleAk).Count(e => e.Id == removedId);
                    Assert.Equal(0, removedCount);

                    Assert.False(context.Set<Root>().Any(r => r.RequiredSingleAk != null));

                    var orphanedCount = context.Set<Root>().Select(r => r.RequiredSingleAk).Select(r => r.Single)
                        .Count(e => e.Id == orphanedId);
                    Assert.Equal(0, orphanedCount);

                    Assert.False(context.Set<Root>().Select(r => r.RequiredSingleAk).Any(r => r.Single != null));

                    var orphanedCCount = context.Set<Root>().Select(r => r.RequiredSingleAk).Select(r => r.SingleComposite)
                        .Count(e => e.Id == orphanedId);
                    Assert.Equal(0, orphanedCCount);

                    Assert.False(context.Set<Root>().Select(r => r.RequiredSingleAk).Any(r => r.SingleComposite != null));
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
    public virtual Task Required_non_PK_one_to_one_with_alternate_key_are_cascade_detached_when_Added(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var root = await LoadRequiredNonPkAkGraphAsync(context);

                var removed = root.RequiredNonPkSingleAk;

                removedId = removed.Id;
                var orphaned = removed.Single;

                // Since we're pretending this isn't in the database, make it really not in the database
                context.Entry(orphaned).State = EntityState.Deleted;
                await context.SaveChangesAsync();

                Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);

                removed.Single = orphaned;
                context.ChangeTracker.DetectChanges();
                context.Entry(orphaned).State = EntityState.Added;
                orphanedId = orphaned.Id;

                Assert.Equal(EntityState.Unchanged, context.Entry(removed).State);
                Assert.Equal(EntityState.Added, context.Entry(orphaned).State);

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);

                if (cascadeDeleteTiming == null)
                {
                    Assert.Equal(EntityState.Added, context.Entry(orphaned).State);

                    context.ChangeTracker.CascadeChanges();
                }

                var expectedState = cascadeDeleteTiming is CascadeTiming.Immediate or null
                    && !Fixture.ForceClientNoAction
                        ? EntityState.Detached
                        : EntityState.Added;

                Assert.Equal(expectedState, context.Entry(orphaned).State);

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
                    Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);

                    Assert.Same(root, removed.Root);
                    Assert.Same(orphaned, removed.Single);
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction
                    && cascadeDeleteTiming != CascadeTiming.Never)
                {
                    var root = await LoadRequiredNonPkAkGraphAsync(context);

                    Assert.Null(root.RequiredNonPkSingleAk);

                    Assert.Empty(context.Set<RequiredNonPkSingleAk1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<RequiredNonPkSingleAk2>().Where(e => e.Id == orphanedId));
                }
            });
    }
}
