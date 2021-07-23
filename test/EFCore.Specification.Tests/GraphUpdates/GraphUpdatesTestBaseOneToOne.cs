// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable AccessToModifiedClosure
// ReSharper disable PossibleMultipleEnumeration
namespace Microsoft.EntityFrameworkCore
{
    public abstract partial class GraphUpdatesTestBase<TFixture>
        where TFixture : GraphUpdatesTestBase<TFixture>.GraphUpdatesFixtureBase, new()
    {
        [ConditionalTheory]
        [InlineData(CascadeTiming.OnSaveChanges)]
        [InlineData(CascadeTiming.Immediate)]
        [InlineData(CascadeTiming.Never)]
        [InlineData(null)]
        public virtual void Optional_one_to_one_relationships_are_one_to_one(
            CascadeTiming? deleteOrphansTiming)
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    var root = context.Set<Root>().Single(IsTheRoot);

                    Assert.False(context.ChangeTracker.HasChanges());

                    root.OptionalSingle = new OptionalSingle1();

                    Assert.True(context.ChangeTracker.HasChanges());

                    Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
        public virtual void Save_changed_optional_one_to_one(
            ChangeMechanism changeMechanism,
            bool useExistingEntities,
            CascadeTiming? deleteOrphansTiming)
        {
            var new2 = new OptionalSingle2();
            var new2d = new OptionalSingle2Derived();
            var new2dd = new OptionalSingle2MoreDerived();
            var new1 = new OptionalSingle1 { Single = new2 };
            var new1d = new OptionalSingle1Derived { Single = new2d };
            var new1dd = new OptionalSingle1MoreDerived { Single = new2dd };
            Root root = null;
            IReadOnlyList<EntityEntry> entries = null;
            OptionalSingle1 old1 = null;
            OptionalSingle1Derived old1d = null;
            OptionalSingle1MoreDerived old1dd = null;
            OptionalSingle2 old2 = null;
            OptionalSingle2Derived old2d = null;
            OptionalSingle2MoreDerived old2dd = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    if (useExistingEntities)
                    {
                        context.AddRange(new1, new1d, new1dd, new2, new2d, new2dd);
                        context.SaveChanges();
                    }
                },
                context =>
                {
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    root = LoadOptionalGraph(context);

                    old1 = root.OptionalSingle;
                    old1d = root.OptionalSingleDerived;
                    old1dd = root.OptionalSingleMoreDerived;
                    old2 = root.OptionalSingle.Single;
                    old2d = (OptionalSingle2Derived)root.OptionalSingleDerived.Single;
                    old2dd = (OptionalSingle2MoreDerived)root.OptionalSingleMoreDerived.Single;

                    if (useExistingEntities)
                    {
                        new1 = context.Set<OptionalSingle1>().Single(e => e.Id == new1.Id);
                        new1d = (OptionalSingle1Derived)context.Set<OptionalSingle1>().Single(e => e.Id == new1d.Id);
                        new1dd = (OptionalSingle1MoreDerived)context.Set<OptionalSingle1>().Single(e => e.Id == new1dd.Id);
                        new2 = context.Set<OptionalSingle2>().Single(e => e.Id == new2.Id);
                        new2d = (OptionalSingle2Derived)context.Set<OptionalSingle2>().Single(e => e.Id == new2d.Id);
                        new2dd = (OptionalSingle2MoreDerived)context.Set<OptionalSingle2>().Single(e => e.Id == new2dd.Id);
                    }
                    else
                    {
                        context.AddRange(new1, new1d, new1dd, new2, new2d, new2dd);
                    }

                    if ((changeMechanism & ChangeMechanism.Principal) != 0)
                    {
                        root.OptionalSingle = new1;
                        root.OptionalSingleDerived = new1d;
                        root.OptionalSingleMoreDerived = new1dd;
                    }

                    if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                    {
                        new1.Root = root;
                        new1d.DerivedRoot = root;
                        new1dd.MoreDerivedRoot = root;
                    }

                    if ((changeMechanism & ChangeMechanism.Fk) != 0)
                    {
                        new1.RootId = root.Id;
                        new1d.DerivedRootId = root.Id;
                        new1dd.MoreDerivedRootId = root.Id;
                    }

                    Assert.True(context.ChangeTracker.HasChanges());

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Equal(root.Id, new1.RootId);
                    Assert.Equal(root.Id, new1d.DerivedRootId);
                    Assert.Equal(root.Id, new1dd.MoreDerivedRootId);
                    Assert.Equal(new1.Id, new2.BackId);
                    Assert.Equal(new1d.Id, new2d.BackId);
                    Assert.Equal(new1dd.Id, new2dd.BackId);
                    Assert.Same(root, new1.Root);
                    Assert.Same(root, new1d.DerivedRoot);
                    Assert.Same(root, new1dd.MoreDerivedRoot);
                    Assert.Same(new1, new2.Back);
                    Assert.Same(new1d, new2d.Back);
                    Assert.Same(new1dd, new2dd.Back);

                    Assert.Null(old1.Root);
                    Assert.Null(old1d.DerivedRoot);
                    Assert.Null(old1dd.MoreDerivedRoot);
                    Assert.Equal(old1, old2.Back);
                    Assert.Equal(old1d, old2d.Back);
                    Assert.Equal(old1dd, old2dd.Back);
                    Assert.Null(old1.RootId);
                    Assert.Null(old1d.DerivedRootId);
                    Assert.Null(old1dd.MoreDerivedRootId);
                    Assert.Equal(old1.Id, old2.BackId);
                    Assert.Equal(old1d.Id, old2d.BackId);
                    Assert.Equal(old1dd.Id, old2dd.BackId);

                    entries = context.ChangeTracker.Entries().ToList();
                },
                context =>
                {
                    var loadedRoot = LoadOptionalGraph(context);

                    AssertKeys(root, loadedRoot);
                    AssertNavigations(loadedRoot);

                    var loaded1 = context.Set<OptionalSingle1>().Single(e => e.Id == old1.Id);
                    var loaded1d = context.Set<OptionalSingle1>().Single(e => e.Id == old1d.Id);
                    var loaded1dd = context.Set<OptionalSingle1>().Single(e => e.Id == old1dd.Id);
                    var loaded2 = context.Set<OptionalSingle2>().Single(e => e.Id == old2.Id);
                    var loaded2d = context.Set<OptionalSingle2>().Single(e => e.Id == old2d.Id);
                    var loaded2dd = context.Set<OptionalSingle2>().Single(e => e.Id == old2dd.Id);

                    AssertEntries(entries, context.ChangeTracker.Entries().ToList());

                    Assert.Null(loaded1.Root);
                    Assert.Null(loaded1d.Root);
                    Assert.Null(loaded1dd.Root);
                    Assert.Same(loaded1, loaded2.Back);
                    Assert.Same(loaded1d, loaded2d.Back);
                    Assert.Same(loaded1dd, loaded2dd.Back);
                    Assert.Null(loaded1.RootId);
                    Assert.Null(loaded1d.RootId);
                    Assert.Null(loaded1dd.RootId);
                    Assert.Equal(loaded1.Id, loaded2.BackId);
                    Assert.Equal(loaded1d.Id, loaded2d.BackId);
                    Assert.Equal(loaded1dd.Id, loaded2dd.BackId);
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
        public virtual void Sever_optional_one_to_one(
            ChangeMechanism changeMechanism,
            CascadeTiming? deleteOrphansTiming)
        {
            Root root = null;
            OptionalSingle1 old1 = null;
            OptionalSingle2 old2 = null;
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    root = LoadOptionalGraph(context);

                    old1 = root.OptionalSingle;
                    old2 = root.OptionalSingle.Single;

                    if ((changeMechanism & ChangeMechanism.Principal) != 0)
                    {
                        root.OptionalSingle = null;
                    }

                    if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                    {
                        old1.Root = null;
                    }

                    if ((changeMechanism & ChangeMechanism.Fk) != 0)
                    {
                        old1.RootId = null;
                    }

                    Assert.False(context.Entry(root).Reference(e => e.OptionalSingle).IsLoaded);
                    Assert.False(context.Entry(old1).Reference(e => e.Root).IsLoaded);
                    Assert.True(context.ChangeTracker.HasChanges());

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Null(old1.Root);
                    Assert.Same(old1, old2.Back);
                    Assert.Null(old1.RootId);
                    Assert.Equal(old1.Id, old2.BackId);
                },
                context =>
                {
                    if ((changeMechanism & ChangeMechanism.Fk) == 0)
                    {
                        var loadedRoot = LoadOptionalGraph(context);

                        AssertKeys(root, loadedRoot);
                        AssertPossiblyNullNavigations(loadedRoot);

                        var loaded1 = context.Set<OptionalSingle1>().Single(e => e.Id == old1.Id);
                        var loaded2 = context.Set<OptionalSingle2>().Single(e => e.Id == old2.Id);

                        Assert.Null(loaded1.Root);
                        Assert.Same(loaded1, loaded2.Back);
                        Assert.Null(loaded1.RootId);
                        Assert.Equal(loaded1.Id, loaded2.BackId);
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
        public virtual void Reparent_optional_one_to_one(
            ChangeMechanism changeMechanism,
            bool useExistingRoot,
            CascadeTiming? deleteOrphansTiming)
        {
            var newRoot = new Root();
            Root root = null;
            OptionalSingle1 old1 = null;
            OptionalSingle2 old2 = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    if (useExistingRoot)
                    {
                        context.AddRange(newRoot);
                        context.SaveChanges();
                    }
                },
                context =>
                {
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    root = LoadOptionalGraph(context);

                    context.Entry(newRoot).State = useExistingRoot ? EntityState.Unchanged : EntityState.Added;

                    old1 = root.OptionalSingle;
                    old2 = root.OptionalSingle.Single;

                    if ((changeMechanism & ChangeMechanism.Principal) != 0)
                    {
                        newRoot.OptionalSingle = old1;
                    }

                    if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                    {
                        old1.Root = newRoot;
                    }

                    if ((changeMechanism & ChangeMechanism.Fk) != 0)
                    {
                        old1.RootId = context.Entry(newRoot).Property(e => e.Id).CurrentValue;
                    }

                    Assert.True(context.ChangeTracker.HasChanges());

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Null(root.OptionalSingle);

                    Assert.Same(newRoot, old1.Root);
                    Assert.Same(old1, old2.Back);
                    Assert.Equal(newRoot.Id, old1.RootId);
                    Assert.Equal(old1.Id, old2.BackId);
                },
                context =>
                {
                    var loadedRoot = LoadOptionalGraph(context);

                    AssertKeys(root, loadedRoot);
                    AssertPossiblyNullNavigations(loadedRoot);

                    newRoot = context.Set<Root>().Single(e => e.Id == newRoot.Id);
                    var loaded1 = context.Set<OptionalSingle1>().Single(e => e.Id == old1.Id);
                    var loaded2 = context.Set<OptionalSingle2>().Single(e => e.Id == old2.Id);

                    Assert.Same(newRoot, loaded1.Root);
                    Assert.Same(loaded1, loaded2.Back);
                    Assert.Equal(newRoot.Id, loaded1.RootId);
                    Assert.Equal(loaded1.Id, loaded2.BackId);
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
        public virtual void Optional_one_to_one_are_orphaned(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            var removedId = 0;
            var orphanedId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    var root = LoadOptionalGraph(context);

                    var removed = root.OptionalSingle;

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
                            Fixture.ForceClientNoAction ? EntityState.Unchanged : EntityState.Modified, context.Entry(orphaned).State);
                    }

                    if (Fixture.ForceClientNoAction)
                    {
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);

                        Assert.Null(root.OptionalSingle);

                        Assert.Empty(context.Set<OptionalSingle1>().Where(e => e.Id == removedId));
                        Assert.Equal(1, context.Set<OptionalSingle2>().Count(e => e.Id == orphanedId));

                        Assert.Same(root, removed.Root);
                        Assert.Same(orphaned, removed.Single);
                    }
                },
                context =>
                {
                    if (!Fixture.ForceClientNoAction)
                    {
                        var root = LoadOptionalGraph(context);

                        Assert.Null(root.OptionalSingle);

                        Assert.Empty(context.Set<OptionalSingle1>().Where(e => e.Id == removedId));
                        Assert.Equal(1, context.Set<OptionalSingle2>().Count(e => e.Id == orphanedId));
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
        public virtual void Optional_one_to_one_leaf_can_be_deleted(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            var removedId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    var root = LoadOptionalGraph(context);
                    var parent = root.OptionalSingle;

                    var removed = parent.Single;

                    removedId = removed.Id;

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (cascadeDeleteTiming == null)
                    {
                        context.ChangeTracker.CascadeChanges();
                    }

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                    Assert.Null(parent.Single);
                    Assert.Empty(context.Set<OptionalSingle2>().Where(e => e.Id == removedId));
                    Assert.Same(parent, removed.Back);
                },
                context =>
                {
                    var root = LoadOptionalGraph(context);
                    var parent = root.OptionalSingle;

                    Assert.Null(parent.Single);
                    Assert.Empty(context.Set<OptionalSingle2>().Where(e => e.Id == removedId));
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
        public virtual void Optional_one_to_one_are_orphaned_in_store(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            var removedId = 0;
            var orphanedId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var removed = LoadOptionalGraph(context).OptionalSingle;

                    removedId = removed.Id;
                    orphanedId = removed.Single.Id;
                },
                context =>
                {
                    context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    var root = context.Set<Root>().Include(e => e.OptionalSingle).Single(IsTheRoot);

                    var removed = root.OptionalSingle;
                    var orphaned = removed.Single;

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (cascadeDeleteTiming == null)
                    {
                        context.ChangeTracker.CascadeChanges();
                    }

                    if (Fixture.ForceClientNoAction)
                    {
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                        Assert.Null(root.OptionalSingle);

                        Assert.Empty(context.Set<OptionalSingle1>().Where(e => e.Id == removedId));
                        Assert.Null(context.Set<OptionalSingle2>().Single(e => e.Id == orphanedId).BackId);

                        Assert.Same(root, removed.Root);
                        Assert.Same(orphaned, removed.Single);
                    }
                },
                context =>
                {
                    if (!Fixture.ForceClientNoAction)
                    {
                        var root = LoadOptionalGraph(context);

                        Assert.Null(root.OptionalSingle);

                        Assert.Empty(context.Set<OptionalSingle1>().Where(e => e.Id == removedId));
                        Assert.Null(context.Set<OptionalSingle2>().Single(e => e.Id == orphanedId).BackId);
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
        public virtual void Optional_one_to_one_are_orphaned_starting_detached(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            var removedId = 0;
            var orphanedId = 0;
            Root root = null;

            ExecuteWithStrategyInTransaction(
                context => root = LoadOptionalGraph(context),
                context =>
                {
                    context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    var removed = root.OptionalSingle;

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

                    var expectedState = (cascadeDeleteTiming == CascadeTiming.Immediate
                            || cascadeDeleteTiming == null)
                        && !Fixture.ForceClientNoAction
                            ? EntityState.Modified
                            : EntityState.Unchanged;

                    Assert.Equal(expectedState, context.Entry(orphaned).State);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceClientNoAction)
                    {
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);

                        Assert.Same(root, removed.Root);
                        Assert.Same(orphaned, removed.Single);
                    }
                },
                context =>
                {
                    if (!Fixture.ForceClientNoAction)
                    {
                        root = LoadOptionalGraph(context);

                        Assert.Null(root.OptionalSingle);

                        Assert.Empty(context.Set<OptionalSingle1>().Where(e => e.Id == removedId));
                        Assert.Equal(1, context.Set<OptionalSingle2>().Count(e => e.Id == orphanedId));
                    }
                });
        }

        [ConditionalTheory]
        [InlineData(CascadeTiming.OnSaveChanges)]
        [InlineData(CascadeTiming.Immediate)]
        [InlineData(CascadeTiming.Never)]
        [InlineData(null)]
        public virtual void Required_one_to_one_relationships_are_one_to_one(
            CascadeTiming? deleteOrphansTiming)
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    var root = context.Set<Root>().Single(IsTheRoot);

                    Assert.False(context.ChangeTracker.HasChanges());

                    root.RequiredSingle = new RequiredSingle1();

                    Assert.True(context.ChangeTracker.HasChanges());

                    Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
        public virtual void Save_required_one_to_one_changed_by_reference(
            ChangeMechanism changeMechanism,
            CascadeTiming? deleteOrphansTiming)
        {
            // This test is a bit strange because the relationships are PK<->PK, which means
            // that an existing entity has to be deleted and then a new entity created that has
            // the same key as the existing entry. In other words it is a new incarnation of the same
            // entity. EF7 can't track two different instances of the same entity, so this has to be
            // done in two steps.

            Root oldRoot = null;
            IReadOnlyList<EntityEntry> entries = null;
            RequiredSingle1 old1 = null;
            RequiredSingle2 old2 = null;
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    oldRoot = LoadRequiredGraph(context);

                    old1 = oldRoot.RequiredSingle;
                    old2 = oldRoot.RequiredSingle.Single;
                });

            var new2 = new RequiredSingle2();
            var new1 = new RequiredSingle1 { Single = new2 };

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    var root = LoadRequiredGraph(context);

                    root.RequiredSingle = null;

                    if (Fixture.ForceClientNoAction)
                    {
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();
                    }
                },
                context =>
                {
                    if (!Fixture.ForceClientNoAction
                        && !Fixture.NoStoreCascades)
                    {
                        var root = LoadRequiredGraph(context);

                        if ((changeMechanism & ChangeMechanism.Principal) != 0)
                        {
                            root.RequiredSingle = new1;
                        }

                        if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                        {
                            context.Add(new1);
                            new1.Root = root;
                        }

                        if ((changeMechanism & ChangeMechanism.Fk) != 0)
                        {
                            context.Add(new1);
                            new1.Id = root.Id;
                        }

                        Assert.True(context.ChangeTracker.HasChanges());

                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(root.Id, new1.Id);
                        Assert.Equal(new1.Id, new2.Id);
                        Assert.Same(root, new1.Root);
                        Assert.Same(new1, new2.Back);

                        Assert.Same(oldRoot, old1.Root);
                        Assert.Same(old1, old2.Back);
                        Assert.Equal(old1.Id, old2.Id);

                        entries = context.ChangeTracker.Entries().ToList();
                    }
                },
                context =>
                {
                    if (!Fixture.ForceClientNoAction
                        && !Fixture.NoStoreCascades)
                    {
                        var loadedRoot = LoadRequiredGraph(context);

                        AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                        AssertKeys(oldRoot, loadedRoot);
                        AssertNavigations(loadedRoot);
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
        public virtual void Save_required_non_PK_one_to_one_changed_by_reference(
            ChangeMechanism changeMechanism,
            bool useExistingEntities,
            CascadeTiming? deleteOrphansTiming)
        {
            var new2 = new RequiredNonPkSingle2();
            var new2d = new RequiredNonPkSingle2Derived();
            var new2dd = new RequiredNonPkSingle2MoreDerived();
            var new1 = new RequiredNonPkSingle1 { Single = new2 };
            var new1d = new RequiredNonPkSingle1Derived { Single = new2d, Root = new Root() };
            var new1dd = new RequiredNonPkSingle1MoreDerived
            {
                Single = new2dd,
                Root = new Root(),
                DerivedRoot = new Root()
            };
            var newRoot = new Root
            {
                RequiredNonPkSingle = new1,
                RequiredNonPkSingleDerived = new1d,
                RequiredNonPkSingleMoreDerived = new1dd
            };
            Root root = null;
            IReadOnlyList<EntityEntry> entries = null;
            RequiredNonPkSingle1 old1 = null;
            RequiredNonPkSingle1Derived old1d = null;
            RequiredNonPkSingle1MoreDerived old1dd = null;
            RequiredNonPkSingle2 old2 = null;
            RequiredNonPkSingle2Derived old2d = null;
            RequiredNonPkSingle2MoreDerived old2dd = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    if (useExistingEntities)
                    {
                        context.AddRange(newRoot, new1, new1d, new1dd, new2, new2d, new2dd);
                        context.SaveChanges();
                    }
                },
                context =>
                {
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    root = LoadRequiredNonPkGraph(context);

                    old1 = root.RequiredNonPkSingle;
                    old1d = root.RequiredNonPkSingleDerived;
                    old1dd = root.RequiredNonPkSingleMoreDerived;
                    old2 = root.RequiredNonPkSingle.Single;
                    old2d = (RequiredNonPkSingle2Derived)root.RequiredNonPkSingleDerived.Single;
                    old2dd = (RequiredNonPkSingle2MoreDerived)root.RequiredNonPkSingleMoreDerived.Single;

                    context.Set<RequiredNonPkSingle1>().Remove(old1d);
                    context.Set<RequiredNonPkSingle1>().Remove(old1dd);

                    if (useExistingEntities)
                    {
                        new1 = context.Set<RequiredNonPkSingle1>().Single(e => e.Id == new1.Id);
                        new1d = (RequiredNonPkSingle1Derived)context.Set<RequiredNonPkSingle1>().Single(e => e.Id == new1d.Id);
                        new1dd = (RequiredNonPkSingle1MoreDerived)context.Set<RequiredNonPkSingle1>().Single(e => e.Id == new1dd.Id);
                        new2 = context.Set<RequiredNonPkSingle2>().Single(e => e.Id == new2.Id);
                        new2d = (RequiredNonPkSingle2Derived)context.Set<RequiredNonPkSingle2>().Single(e => e.Id == new2d.Id);
                        new2dd = (RequiredNonPkSingle2MoreDerived)context.Set<RequiredNonPkSingle2>().Single(e => e.Id == new2dd.Id);

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
                        root.RequiredNonPkSingle = new1;
                        root.RequiredNonPkSingleDerived = new1d;
                        root.RequiredNonPkSingleMoreDerived = new1dd;
                    }

                    if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                    {
                        new1.Root = root;
                        new1d.DerivedRoot = root;
                        new1dd.MoreDerivedRoot = root;
                    }

                    if ((changeMechanism & ChangeMechanism.Fk) != 0)
                    {
                        new1.RootId = root.Id;
                        new1d.DerivedRootId = root.Id;
                        new1dd.MoreDerivedRootId = root.Id;
                    }

                    if (Fixture.ForceClientNoAction
                        || deleteOrphansTiming == CascadeTiming.Never)
                    {
                        var testCode = deleteOrphansTiming == CascadeTiming.Immediate
                            ? () => context.ChangeTracker.DetectChanges()
                            : deleteOrphansTiming == null
                                ? () => context.ChangeTracker.CascadeChanges()
                                : (Action)(() => context.SaveChanges());

                        var message = Assert.Throws<InvalidOperationException>(testCode).Message;

                        Assert.Equal(
                            message,
                            CoreStrings.RelationshipConceptualNullSensitive(
                                nameof(Root), nameof(RequiredNonPkSingle1), "{RootId: " + old1.RootId + "}"));
                    }
                    else
                    {
                        Assert.True(context.ChangeTracker.HasChanges());

                        if (deleteOrphansTiming == null)
                        {
                            context.ChangeTracker.CascadeChanges();
                        }

                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(root.Id, new1.RootId);
                        Assert.Equal(root.Id, new1d.DerivedRootId);
                        Assert.Equal(root.Id, new1dd.MoreDerivedRootId);
                        Assert.Equal(new1.Id, new2.BackId);
                        Assert.Equal(new1d.Id, new2d.BackId);
                        Assert.Equal(new1dd.Id, new2dd.BackId);
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
                        Assert.Equal(old1.Id, old2.BackId);
                        Assert.Equal(old1d.Id, old2d.BackId);
                        Assert.Equal(old1dd.Id, old2dd.BackId);

                        entries = context.ChangeTracker.Entries().ToList();
                    }
                },
                context =>
                {
                    if (!Fixture.ForceClientNoAction
                        && !Fixture.NoStoreCascades
                        && deleteOrphansTiming != CascadeTiming.Never)
                    {
                        var loadedRoot = LoadRequiredNonPkGraph(context);

                        AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                        AssertKeys(root, loadedRoot);
                        AssertNavigations(loadedRoot);

                        Assert.False(context.Set<RequiredNonPkSingle1>().Any(e => e.Id == old1.Id));
                        Assert.False(context.Set<RequiredNonPkSingle1>().Any(e => e.Id == old1d.Id));
                        Assert.False(context.Set<RequiredNonPkSingle1>().Any(e => e.Id == old1dd.Id));
                        Assert.False(context.Set<RequiredNonPkSingle2>().Any(e => e.Id == old2.Id));
                        Assert.False(context.Set<RequiredNonPkSingle2>().Any(e => e.Id == old2d.Id));
                        Assert.False(context.Set<RequiredNonPkSingle2>().Any(e => e.Id == old2dd.Id));
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
        public virtual void Sever_required_one_to_one(
            ChangeMechanism changeMechanism,
            CascadeTiming? deleteOrphansTiming)
        {
            Root root = null;
            RequiredSingle1 old1 = null;
            RequiredSingle2 old2 = null;
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    root = LoadRequiredGraph(context);

                    old1 = root.RequiredSingle;
                    old2 = root.RequiredSingle.Single;

                    if ((changeMechanism & ChangeMechanism.Principal) != 0)
                    {
                        root.RequiredSingle = null;
                    }

                    if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                    {
                        old1.Root = null;
                    }

                    if ((changeMechanism & ChangeMechanism.Fk) != 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                    }

                    Assert.False(context.Entry(root).Reference(e => e.RequiredSingle).IsLoaded);
                    Assert.False(context.Entry(old1).Reference(e => e.Root).IsLoaded);
                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceClientNoAction)
                    {
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Null(old1.Root);
                        if (!context.Entry(old2).Metadata.IsOwned())
                        {
                            // Navigations to owners are preserved when these are owned
                            Assert.Null(old2.Back);
                        }
                        Assert.Equal(old1.Id, old2.Id);
                    }
                },
                context =>
                {
                    if (!Fixture.ForceClientNoAction
                        && !Fixture.NoStoreCascades)
                    {
                        var loadedRoot = LoadRequiredGraph(context);

                        AssertKeys(root, loadedRoot);
                        AssertPossiblyNullNavigations(loadedRoot);

                        //TODO: Aggregate on optional dependent #23230
                        //var removedCount = context.Set<Root>().Select(r => r.RequiredSingle).Count(e => e.Id == old1.Id);
                        //Assert.Equal(0, removedCount);

                        Assert.False(context.Set<Root>().Any(r => r.RequiredSingle != null));

                        //var orphanedCount = context.Set<Root>().Select(r => r.RequiredSingle).Select(r => r.Single)
                        //    .Count(e => e.Id == old2.Id);
                        //Assert.Equal(0, orphanedCount);

                        Assert.False(context.Set<Root>().Select(r => r.RequiredSingle).Any(r => r.Single != null));
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
        public virtual void Sever_required_non_PK_one_to_one(
            ChangeMechanism changeMechanism,
            CascadeTiming? deleteOrphansTiming)
        {
            Root root = null;
            RequiredNonPkSingle1 old1 = null;
            RequiredNonPkSingle2 old2 = null;
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    root = LoadRequiredNonPkGraph(context);

                    old1 = root.RequiredNonPkSingle;
                    old2 = root.RequiredNonPkSingle.Single;

                    if ((changeMechanism & ChangeMechanism.Principal) != 0)
                    {
                        root.RequiredNonPkSingle = null;
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
                            ? () => context.ChangeTracker.DetectChanges()
                            : deleteOrphansTiming == null
                                ? () => context.ChangeTracker.CascadeChanges()
                                : (Action)(() => context.SaveChanges());

                        var message = Assert.Throws<InvalidOperationException>(testCode).Message;

                        Assert.Equal(
                            message,
                            CoreStrings.RelationshipConceptualNullSensitive(
                                nameof(Root), nameof(RequiredNonPkSingle1), "{RootId: " + old1.RootId + "}"));
                    }
                    else
                    {
                        Assert.False(context.Entry(root).Reference(e => e.RequiredNonPkSingle).IsLoaded);
                        Assert.False(context.Entry(old1).Reference(e => e.Root).IsLoaded);
                        Assert.True(context.ChangeTracker.HasChanges());

                        if (deleteOrphansTiming == null)
                        {
                            context.ChangeTracker.CascadeChanges();
                        }

                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Null(old1.Root);
                        Assert.Null(old2.Back);
                        Assert.Equal(old1.Id, old2.BackId);
                    }
                },
                context =>
                {
                    if (!Fixture.ForceClientNoAction
                        && !Fixture.NoStoreCascades
                        && deleteOrphansTiming != CascadeTiming.Never)
                    {
                        var loadedRoot = LoadRequiredNonPkGraph(context);

                        AssertKeys(root, loadedRoot);
                        AssertPossiblyNullNavigations(loadedRoot);

                        Assert.False(context.Set<RequiredNonPkSingle1>().Any(e => e.Id == old1.Id));
                        Assert.False(context.Set<RequiredNonPkSingle2>().Any(e => e.Id == old2.Id));
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
        public virtual void Reparent_required_one_to_one(
            ChangeMechanism changeMechanism,
            bool useExistingRoot,
            CascadeTiming? deleteOrphansTiming)
        {
            var newRoot = new Root();

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    if (useExistingRoot)
                    {
                        context.AddRange(newRoot);
                        context.SaveChanges();
                    }
                },
                context =>
                {
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    var root = LoadRequiredGraph(context);

                    Assert.False(context.ChangeTracker.HasChanges());

                    context.Entry(newRoot).State = useExistingRoot ? EntityState.Unchanged : EntityState.Added;

                    Assert.Equal(
                        CoreStrings.KeyReadOnly("Id", typeof(RequiredSingle1).Name),
                        Assert.Throws<InvalidOperationException>(
                            () =>
                            {
                                if ((changeMechanism & ChangeMechanism.Principal) != 0)
                                {
                                    newRoot.RequiredSingle = root.RequiredSingle;
                                }

                                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                                {
                                    root.RequiredSingle.Root = newRoot;
                                }

                                if ((changeMechanism & ChangeMechanism.Fk) != 0)
                                {
                                    root.RequiredSingle.Id = newRoot.Id;
                                }

                                newRoot.RequiredSingle = root.RequiredSingle;

                                context.SaveChanges();
                            }).Message);
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
        public virtual void Reparent_required_non_PK_one_to_one(
            ChangeMechanism changeMechanism,
            bool useExistingRoot,
            CascadeTiming? deleteOrphansTiming)
        {
            var newRoot = new Root();
            Root root = null;
            RequiredNonPkSingle1 old1 = null;
            RequiredNonPkSingle2 old2 = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    if (useExistingRoot)
                    {
                        context.AddRange(newRoot);
                        context.SaveChanges();
                    }
                },
                context =>
                {
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    root = LoadRequiredNonPkGraph(context);

                    context.Entry(newRoot).State = useExistingRoot ? EntityState.Unchanged : EntityState.Added;

                    old1 = root.RequiredNonPkSingle;
                    old2 = root.RequiredNonPkSingle.Single;

                    if ((changeMechanism & ChangeMechanism.Principal) != 0)
                    {
                        newRoot.RequiredNonPkSingle = old1;
                    }

                    if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                    {
                        old1.Root = newRoot;
                    }

                    if ((changeMechanism & ChangeMechanism.Fk) != 0)
                    {
                        old1.RootId = context.Entry(newRoot).Property(e => e.Id).CurrentValue;
                    }

                    Assert.True(context.ChangeTracker.HasChanges());

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Null(root.RequiredNonPkSingle);

                    Assert.Same(newRoot, old1.Root);
                    Assert.Same(old1, old2.Back);
                    Assert.Equal(newRoot.Id, old1.RootId);
                    Assert.Equal(old1.Id, old2.BackId);
                },
                context =>
                {
                    var loadedRoot = LoadRequiredNonPkGraph(context);

                    AssertKeys(root, loadedRoot);
                    AssertPossiblyNullNavigations(loadedRoot);

                    newRoot = context.Set<Root>().Single(e => e.Id == newRoot.Id);
                    var loaded1 = context.Set<RequiredNonPkSingle1>().Single(e => e.Id == old1.Id);
                    var loaded2 = context.Set<RequiredNonPkSingle2>().Single(e => e.Id == old2.Id);

                    Assert.Same(newRoot, loaded1.Root);
                    Assert.Same(loaded1, loaded2.Back);
                    Assert.Equal(newRoot.Id, loaded1.RootId);
                    Assert.Equal(loaded1.Id, loaded2.BackId);
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
        public virtual void Required_one_to_one_are_cascade_deleted(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            var removedId = 0;
            var orphanedId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    var root = LoadRequiredGraph(context);

                    var removed = root.RequiredSingle;

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
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else if (cascadeDeleteTiming == CascadeTiming.Never)
                    {
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                        Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);

                        Assert.Null(root.RequiredSingle);

                        Assert.Same(root, removed.Root);
                        Assert.Same(orphaned, removed.Single);
                    }
                },
                context =>
                {
                    if (!Fixture.ForceClientNoAction
                        && cascadeDeleteTiming != CascadeTiming.Never)
                    {
                        var root = LoadRequiredGraph(context);

                        Assert.Null(root.RequiredSingle);

                        //TODO: Aggregate on optional dependent #23230
                        //var removedCount = context.Set<Root>().Select(r => r.RequiredSingle).Count(e => e.Id == removedId);
                        //Assert.Equal(0, removedCount);

                        Assert.False(context.Set<Root>().Any(r => r.RequiredSingle != null));

                        //var orphanedCount = context.Set<Root>().Select(r => r.RequiredSingle).Select(r => r.Single)
                        //    .Count(e => e.Id == orphanedId);
                        //Assert.Equal(0, orphanedCount);

                        Assert.False(context.Set<Root>().Select(r => r.RequiredSingle).Any(r => r.Single != null));
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
        public virtual void Required_one_to_one_leaf_can_be_deleted(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            var removedId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    var root = LoadRequiredGraph(context);
                    var parent = root.RequiredSingle;

                    var removed = parent.Single;

                    removedId = removed.Id;

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (cascadeDeleteTiming == null)
                    {
                        context.ChangeTracker.CascadeChanges();
                    }

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                    Assert.Null(parent.Single);
                    Assert.Same(parent, removed.Back);
                },
                context =>
                {
                    var root = LoadRequiredGraph(context);
                    var parent = root.RequiredSingle;

                    Assert.Null(parent.Single);

                    //TODO: Aggregate on optional dependent #23230
                    //var removedCount = context.Set<Root>().Select(r => r.RequiredSingle).Select(r => r.Single)
                    //    .Count(e => e.Id == removedId);
                    //Assert.Equal(0, removedCount);

                    Assert.False(context.Set<Root>().Select(r => r.RequiredSingle).Any(r => r.Single != null));
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
        public virtual void Required_non_PK_one_to_one_are_cascade_deleted(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            var removedId = 0;
            var orphanedId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    var root = LoadRequiredNonPkGraph(context);

                    var removed = root.RequiredNonPkSingle;

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
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else if (cascadeDeleteTiming == CascadeTiming.Never)
                    {
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                        Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);

                        Assert.Null(root.RequiredNonPkSingle);

                        Assert.Empty(context.Set<RequiredNonPkSingle1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredNonPkSingle2>().Where(e => e.Id == orphanedId));

                        Assert.Same(root, removed.Root);
                        Assert.Same(orphaned, removed.Single);
                    }
                },
                context =>
                {
                    if (!Fixture.ForceClientNoAction
                        && cascadeDeleteTiming != CascadeTiming.Never)
                    {
                        var root = LoadRequiredNonPkGraph(context);

                        Assert.Null(root.RequiredNonPkSingle);

                        Assert.Empty(context.Set<RequiredNonPkSingle1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredNonPkSingle2>().Where(e => e.Id == orphanedId));
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
        public virtual void Required_non_PK_one_to_one_leaf_can_be_deleted(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            var removedId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    var root = LoadRequiredNonPkGraph(context);
                    var parent = root.RequiredNonPkSingle;

                    var removed = parent.Single;

                    removedId = removed.Id;

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (cascadeDeleteTiming == null)
                    {
                        context.ChangeTracker.CascadeChanges();
                    }

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                    Assert.Null(parent.Single);
                    Assert.Same(parent, removed.Back);
                },
                context =>
                {
                    var root = LoadRequiredNonPkGraph(context);
                    var parent = root.RequiredNonPkSingle;

                    Assert.Null(parent.Single);
                    Assert.Empty(context.Set<RequiredNonPkSingle2>().Where(e => e.Id == removedId));
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
        public virtual void Required_one_to_one_are_cascade_deleted_in_store(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            var removedId = 0;
            var orphanedId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var removed = LoadRequiredGraph(context).RequiredSingle;

                    removedId = removed.Id;
                    orphanedId = removed.Single.Id;
                },
                context =>
                {
                    context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    var root = context.Set<Root>().Include(e => e.RequiredSingle).Single(IsTheRoot);

                    var removed = root.RequiredSingle;
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
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                        Assert.Null(root.RequiredSingle);

                        Assert.Same(root, removed.Root);
                        Assert.Same(orphaned, removed.Single);
                    }
                },
                context =>
                {
                    if (!Fixture.ForceClientNoAction
                        && !Fixture.NoStoreCascades)
                    {
                        var root = LoadRequiredGraph(context);

                        Assert.Null(root.RequiredSingle);

                        Assert.Empty(context.Set<RequiredSingle1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredSingle2>().Where(e => e.Id == orphanedId));
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
        public virtual void Required_non_PK_one_to_one_are_cascade_deleted_in_store(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            var removedId = 0;
            var orphanedId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var removed = LoadRequiredNonPkGraph(context).RequiredNonPkSingle;

                    removedId = removed.Id;
                    orphanedId = removed.Single.Id;
                },
                context =>
                {
                    context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    var root = context.Set<Root>().Include(e => e.RequiredNonPkSingle).Single(IsTheRoot);

                    var removed = root.RequiredNonPkSingle;
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
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                        Assert.Null(root.RequiredNonPkSingle);

                        Assert.Empty(context.Set<RequiredNonPkSingle1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredNonPkSingle2>().Where(e => e.Id == orphanedId));

                        Assert.Same(root, removed.Root);
                        Assert.Same(orphaned, removed.Single);
                    }
                },
                context =>
                {
                    if (!Fixture.ForceClientNoAction
                        && !Fixture.NoStoreCascades)
                    {
                        var root = LoadRequiredNonPkGraph(context);

                        Assert.Null(root.RequiredNonPkSingle);

                        Assert.Empty(context.Set<RequiredNonPkSingle1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredNonPkSingle2>().Where(e => e.Id == orphanedId));
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
        public virtual void Required_one_to_one_are_cascade_deleted_starting_detached(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            var removedId = 0;
            var orphanedId = 0;
            Root root = null;

            ExecuteWithStrategyInTransaction(
                context => root = LoadRequiredGraph(context),
                context =>
                {
                    context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    var removed = root.RequiredSingle;

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

                    var expectedState = (cascadeDeleteTiming == CascadeTiming.Immediate
                            || cascadeDeleteTiming == null)
                        && !Fixture.ForceClientNoAction
                            ? EntityState.Deleted
                            : EntityState.Unchanged;

                    Assert.Equal(expectedState, context.Entry(orphaned).State);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceClientNoAction)
                    {
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else if (cascadeDeleteTiming == CascadeTiming.Never)
                    {
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                        Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);

                        Assert.Same(root, removed.Root);
                        Assert.Same(orphaned, removed.Single);
                    }
                },
                context => root = LoadRequiredGraph(context),
                context =>
                {
                    if (!Fixture.ForceClientNoAction
                        && cascadeDeleteTiming != CascadeTiming.Never)
                    {
                        Assert.Null(root.RequiredSingle);

                        //TODO: Aggregate on optional dependent #23230
                        //var removedCount = context.Set<Root>().Select(r => r.RequiredSingle).Count(e => e.Id == removedId);
                        //Assert.Equal(0, removedCount);

                        Assert.False(context.Set<Root>().Any(r => r.RequiredSingle != null));

                        //var orphanedCount = context.Set<Root>().Select(r => r.RequiredSingle).Select(r => r.Single)
                        //    .Count(e => e.Id == orphanedId);
                        //Assert.Equal(0, orphanedCount);

                        Assert.False(context.Set<Root>().Select(r => r.RequiredSingle).Any(r => r.Single != null));
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
        public virtual void Required_non_PK_one_to_one_are_cascade_deleted_starting_detached(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            var removedId = 0;
            var orphanedId = 0;
            Root root = null;

            ExecuteWithStrategyInTransaction(
                context => root = LoadRequiredNonPkGraph(context),
                context =>
                {
                    context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    var removed = root.RequiredNonPkSingle;

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

                    var expectedState = (cascadeDeleteTiming == CascadeTiming.Immediate
                            || cascadeDeleteTiming == null)
                        && !Fixture.ForceClientNoAction
                            ? EntityState.Deleted
                            : EntityState.Unchanged;

                    Assert.Equal(expectedState, context.Entry(orphaned).State);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceClientNoAction)
                    {
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else if (cascadeDeleteTiming == CascadeTiming.Never)
                    {
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                        Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);

                        Assert.Same(root, removed.Root);
                        Assert.Same(orphaned, removed.Single);
                    }
                },
                context =>
                {
                    if (!Fixture.ForceClientNoAction
                        && cascadeDeleteTiming != CascadeTiming.Never)
                    {
                        root = LoadRequiredNonPkGraph(context);

                        Assert.Null(root.RequiredNonPkSingle);

                        Assert.Empty(context.Set<RequiredNonPkSingle1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredNonPkSingle2>().Where(e => e.Id == orphanedId));
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
        public virtual void Required_one_to_one_are_cascade_detached_when_Added(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            var removedId = 0;
            var orphanedId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    var root = LoadRequiredGraph(context);

                    var removed = root.RequiredSingle;

                    removedId = removed.Id;
                    var orphaned = removed.Single;

                    // Since we're pretending this isn't in the database, make it really not in the database
                    context.Entry(orphaned).State = EntityState.Deleted;
                    context.SaveChanges();

                    Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);

                    removed.Single = orphaned;
                    context.ChangeTracker.DetectChanges();
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

                    var expectedState = (cascadeDeleteTiming == CascadeTiming.Immediate
                            || cascadeDeleteTiming == null)
                        && !Fixture.ForceClientNoAction
                            ? EntityState.Detached
                            : EntityState.Added;

                    Assert.Equal(expectedState, context.Entry(orphaned).State);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceClientNoAction)
                    {
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else if (cascadeDeleteTiming == CascadeTiming.Never)
                    {
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                        Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);

                        Assert.Same(root, removed.Root);
                        Assert.Same(orphaned, removed.Single);
                    }
                },
                context =>
                {
                    if (!Fixture.ForceClientNoAction
                        && cascadeDeleteTiming != CascadeTiming.Never)
                    {
                        var root = LoadRequiredGraph(context);

                        Assert.Null(root.RequiredSingle);

                        //TODO: Aggregate on optional dependent #23230
                        //var removedCount = context.Set<Root>().Select(r => r.RequiredSingle).Count(e => e.Id == removedId);
                        //Assert.Equal(0, removedCount);

                        Assert.False(context.Set<Root>().Any(r => r.RequiredSingle != null));

                        //var orphanedCount = context.Set<Root>().Select(r => r.RequiredSingle).Select(r => r.Single)
                        //    .Count(e => e.Id == orphanedId);
                        //Assert.Equal(0, orphanedCount);

                        Assert.False(context.Set<Root>().Select(r => r.RequiredSingle).Any(r => r.Single != null));
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
        public virtual void Required_non_PK_one_to_one_are_cascade_detached_when_Added(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            var removedId = 0;
            var orphanedId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    var root = LoadRequiredNonPkGraph(context);

                    var removed = root.RequiredNonPkSingle;

                    removedId = removed.Id;
                    var orphaned = removed.Single;

                    // Since we're pretending this isn't in the database, make it really not in the database
                    context.Entry(orphaned).State = EntityState.Deleted;
                    context.SaveChanges();

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

                    var expectedState = (cascadeDeleteTiming == CascadeTiming.Immediate
                            || cascadeDeleteTiming == null)
                        && !Fixture.ForceClientNoAction
                            ? EntityState.Detached
                            : EntityState.Added;

                    Assert.Equal(expectedState, context.Entry(orphaned).State);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceClientNoAction)
                    {
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else if (cascadeDeleteTiming == CascadeTiming.Never)
                    {
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                        Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);

                        Assert.Same(root, removed.Root);
                        Assert.Same(orphaned, removed.Single);
                    }
                },
                context =>
                {
                    if (!Fixture.ForceClientNoAction
                        && cascadeDeleteTiming != CascadeTiming.Never)
                    {
                        var root = LoadRequiredNonPkGraph(context);

                        Assert.Null(root.RequiredNonPkSingle);

                        Assert.Empty(context.Set<RequiredNonPkSingle1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredNonPkSingle2>().Where(e => e.Id == orphanedId));
                    }
                });
        }
    }
}
