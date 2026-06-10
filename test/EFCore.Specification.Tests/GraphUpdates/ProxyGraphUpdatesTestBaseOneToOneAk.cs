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
    public virtual Task Save_changed_optional_one_to_one_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingEntities)
    {
        OptionalSingleAk2 new2 = null;
        OptionalSingleAk2Derived new2d = null;
        OptionalSingleAk2MoreDerived new2dd = null;
        OptionalSingleComposite2 new2c = null;
        OptionalSingleAk1 new1 = null;
        OptionalSingleAk1Derived new1d = null;
        OptionalSingleAk1MoreDerived new1dd = null;
        OptionalSingleAk1 old1 = null;
        OptionalSingleAk1Derived old1d = null;
        OptionalSingleAk1MoreDerived old1dd = null;
        OptionalSingleAk2 old2 = null;
        OptionalSingleComposite2 old2c = null;
        OptionalSingleAk2Derived old2d = null;
        OptionalSingleAk2MoreDerived old2dd = null;

        return ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                new2 = context.CreateProxy<OptionalSingleAk2>(e => e.AlternateId = Guid.NewGuid());
                new2d = context.CreateProxy<OptionalSingleAk2Derived>(e => e.AlternateId = Guid.NewGuid());
                new2dd = context.CreateProxy<OptionalSingleAk2MoreDerived>(e => e.AlternateId = Guid.NewGuid());
                new2c = context.CreateProxy<OptionalSingleComposite2>();
                new1 = context.CreateProxy<OptionalSingleAk1>(
                    e =>
                    {
                        e.AlternateId = Guid.NewGuid();
                        e.Single = new2;
                        e.SingleComposite = new2c;
                    });
                new1d = context.CreateProxy<OptionalSingleAk1Derived>(
                    e =>
                    {
                        e.AlternateId = Guid.NewGuid();
                        e.Single = new2d;
                    });
                new1dd = context.CreateProxy<OptionalSingleAk1MoreDerived>(
                    e =>
                    {
                        e.AlternateId = Guid.NewGuid();
                        e.Single = new2dd;
                    });

                if (useExistingEntities)
                {
                    context.AddRange(new1, new1d, new1dd, new2, new2d, new2dd, new2c);
                    context.SaveChanges();
                }

                return Task.CompletedTask;
            }, async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.OptionalSingleAk).Load();
                    context.Entry(root).Reference(e => e.OptionalSingleAkDerived).Load();
                    context.Entry(root).Reference(e => e.OptionalSingleAkMoreDerived).Load();
                }

                old1 = root.OptionalSingleAk;
                old1d = root.OptionalSingleAkDerived;
                old1dd = root.OptionalSingleAkMoreDerived;

                if (!DoesLazyLoading)
                {
                    context.Entry(old1).Reference(e => e.Single).Load();
                    context.Entry(old1).Reference(e => e.SingleComposite).Load();
                    context.Entry(old1d).Reference(e => e.Single).Load();
                    context.Entry(old1dd).Reference(e => e.Single).Load();
                }

                old2 = root.OptionalSingleAk.Single;
                old2c = root.OptionalSingleAk.SingleComposite;
                old2d = (OptionalSingleAk2Derived)root.OptionalSingleAkDerived.Single;
                old2dd = (OptionalSingleAk2MoreDerived)root.OptionalSingleAkMoreDerived.Single;

                if (useExistingEntities)
                {
                    new1 = context.Set<OptionalSingleAk1>().Single(e => e.Id == new1.Id);
                    new1d = (OptionalSingleAk1Derived)context.Set<OptionalSingleAk1>().Single(e => e.Id == new1d.Id);
                    new1dd = (OptionalSingleAk1MoreDerived)context.Set<OptionalSingleAk1>().Single(e => e.Id == new1dd.Id);
                    new2 = context.Set<OptionalSingleAk2>().Single(e => e.Id == new2.Id);
                    new2c = context.Set<OptionalSingleComposite2>().Single(e => e.Id == new2c.Id);
                    new2d = (OptionalSingleAk2Derived)context.Set<OptionalSingleAk2>().Single(e => e.Id == new2d.Id);
                    new2dd = (OptionalSingleAk2MoreDerived)context.Set<OptionalSingleAk2>().Single(e => e.Id == new2dd.Id);
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

                context.SaveChanges();

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
            }, async context =>
            {
                await LoadRootAsync(context);

                var loaded1 = context.Set<OptionalSingleAk1>().Single(e => e.Id == old1.Id);
                var loaded1d = context.Set<OptionalSingleAk1>().Single(e => e.Id == old1d.Id);
                var loaded1dd = context.Set<OptionalSingleAk1>().Single(e => e.Id == old1dd.Id);
                var loaded2 = context.Set<OptionalSingleAk2>().Single(e => e.Id == old2.Id);
                var loaded2d = context.Set<OptionalSingleAk2>().Single(e => e.Id == old2d.Id);
                var loaded2dd = context.Set<OptionalSingleAk2>().Single(e => e.Id == old2dd.Id);
                var loaded2c = context.Set<OptionalSingleComposite2>().Single(e => e.Id == old2c.Id);

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
        OptionalSingleAk2 new2;
        OptionalSingleAk2Derived new2d;
        OptionalSingleAk2MoreDerived new2dd;
        OptionalSingleComposite2 new2c;
        OptionalSingleAk1 new1;
        OptionalSingleAk1Derived new1d;
        OptionalSingleAk1MoreDerived new1dd;
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
                new2 = context.CreateProxy<OptionalSingleAk2>(e => e.AlternateId = Guid.NewGuid());
                new2d = context.CreateProxy<OptionalSingleAk2Derived>(e => e.AlternateId = Guid.NewGuid());
                new2dd = context.CreateProxy<OptionalSingleAk2MoreDerived>(e => e.AlternateId = Guid.NewGuid());
                new2c = context.CreateProxy<OptionalSingleComposite2>();
                new1 = context.CreateProxy<OptionalSingleAk1>(
                    e =>
                    {
                        e.AlternateId = Guid.NewGuid();
                        e.Single = new2;
                        e.SingleComposite = new2c;
                    });
                new1d = context.CreateProxy<OptionalSingleAk1Derived>(
                    e =>
                    {
                        e.AlternateId = Guid.NewGuid();
                        e.Single = new2d;
                    });
                new1dd = context.CreateProxy<OptionalSingleAk1MoreDerived>(
                    e =>
                    {
                        e.AlternateId = Guid.NewGuid();
                        e.Single = new2dd;
                    });

                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.OptionalSingleAk).Load();
                    context.Entry(root).Reference(e => e.OptionalSingleAkDerived).Load();
                    context.Entry(root).Reference(e => e.OptionalSingleAkMoreDerived).Load();
                }

                old1 = root.OptionalSingleAk;
                old1d = root.OptionalSingleAkDerived;
                old1dd = root.OptionalSingleAkMoreDerived;

                if (!DoesLazyLoading)
                {
                    context.Entry(old1).Reference(e => e.Single).Load();
                    context.Entry(old1).Reference(e => e.SingleComposite).Load();
                    context.Entry(old1d).Reference(e => e.Single).Load();
                    context.Entry(old1dd).Reference(e => e.Single).Load();
                }

                old2 = root.OptionalSingleAk.Single;
                old2c = root.OptionalSingleAk.SingleComposite;
                old2d = (OptionalSingleAk2Derived)root.OptionalSingleAkDerived.Single;
                old2dd = (OptionalSingleAk2MoreDerived)root.OptionalSingleAkMoreDerived.Single;

                using (var context2 = CreateContext())
                {
                    UseTransaction(context2.Database, context.Database.CurrentTransaction);
                    var root2 = context2.Set<Root>()
                        .Include(e => e.OptionalChildrenAk).ThenInclude(e => e.Children)
                        .Include(e => e.OptionalChildrenAk).ThenInclude(e => e.CompositeChildren)
                        .Include(e => e.OptionalSingleAk).ThenInclude(e => e.Single)
                        .Include(e => e.OptionalSingleAk).ThenInclude(e => e.SingleComposite)
                        .Include(e => e.OptionalSingleAkDerived).ThenInclude(e => e.Single)
                        .Include(e => e.OptionalSingleAkMoreDerived).ThenInclude(e => e.Single)
                        .Single(IsTheRoot);

                    context2.AddRange(new1, new1d, new1dd, new2, new2d, new2dd, new2c);
                    root2.OptionalSingleAk = new1;
                    root2.OptionalSingleAkDerived = new1d;
                    root2.OptionalSingleAkMoreDerived = new1dd;

                    context2.SaveChanges();
                }

                new1 = context.Set<OptionalSingleAk1>().Single(e => e.Id == new1.Id);
                new1d = (OptionalSingleAk1Derived)context.Set<OptionalSingleAk1>().Single(e => e.Id == new1d.Id);
                new1dd = (OptionalSingleAk1MoreDerived)context.Set<OptionalSingleAk1>().Single(e => e.Id == new1dd.Id);
                new2 = context.Set<OptionalSingleAk2>().Single(e => e.Id == new2.Id);
                new2c = context.Set<OptionalSingleComposite2>().Single(e => e.Id == new2c.Id);
                new2d = (OptionalSingleAk2Derived)context.Set<OptionalSingleAk2>().Single(e => e.Id == new2d.Id);
                new2dd = (OptionalSingleAk2MoreDerived)context.Set<OptionalSingleAk2>().Single(e => e.Id == new2dd.Id);

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

                context.SaveChanges();

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
            }, async context =>
            {
                await LoadRootAsync(context);

                var loaded1 = context.Set<OptionalSingleAk1>().Single(e => e.Id == old1.Id);
                var loaded1d = context.Set<OptionalSingleAk1>().Single(e => e.Id == old1d.Id);
                var loaded1dd = context.Set<OptionalSingleAk1>().Single(e => e.Id == old1dd.Id);
                var loaded2 = context.Set<OptionalSingleAk2>().Single(e => e.Id == old2.Id);
                var loaded2d = context.Set<OptionalSingleAk2>().Single(e => e.Id == old2d.Id);
                var loaded2dd = context.Set<OptionalSingleAk2>().Single(e => e.Id == old2dd.Id);
                var loaded2c = context.Set<OptionalSingleComposite2>().Single(e => e.Id == old2c.Id);

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
    [InlineData((int)ChangeMechanism.Dependent, false)]
    [InlineData((int)ChangeMechanism.Dependent, true)]
    [InlineData((int)ChangeMechanism.Principal, false)]
    [InlineData((int)ChangeMechanism.Principal, true)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true)]
    public virtual Task Save_required_one_to_one_changed_by_reference_with_alternate_key(
        ChangeMechanism changeMechanism,
        bool useExistingEntities)
    {
        RequiredSingleAk2 new2 = null;
        RequiredSingleComposite2 new2c = null;
        RequiredSingleAk1 new1 = null;
        Root newRoot;
        RequiredSingleAk1 old1 = null;
        RequiredSingleAk2 old2 = null;
        RequiredSingleComposite2 old2c = null;

        return ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                new2 = context.CreateProxy<RequiredSingleAk2>(e => e.AlternateId = Guid.NewGuid());
                new2c = context.CreateProxy<RequiredSingleComposite2>();
                new1 = context.CreateProxy<RequiredSingleAk1>(
                    e =>
                    {
                        e.AlternateId = Guid.NewGuid();
                        e.Single = new2;
                        e.SingleComposite = new2c;
                    });
                newRoot = context.CreateProxy<Root>(
                    e =>
                    {
                        e.AlternateId = Guid.NewGuid();
                        e.RequiredSingleAk = new1;
                    });

                if (useExistingEntities)
                {
                    context.AddRange(newRoot, new1, new2, new2c);
                    context.SaveChanges();
                }

                return Task.CompletedTask;
            }, async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.RequiredSingleAk).Load();
                }

                old1 = root.RequiredSingleAk;

                if (!DoesLazyLoading)
                {
                    context.Entry(old1).Reference(e => e.Single).Load();
                    context.Entry(old1).Reference(e => e.SingleComposite).Load();
                }

                old2 = root.RequiredSingleAk.Single;
                old2c = root.RequiredSingleAk.SingleComposite;

                if (useExistingEntities)
                {
                    new1 = context.Set<RequiredSingleAk1>().Single(e => e.Id == new1.Id);
                    new2 = context.Set<RequiredSingleAk2>().Single(e => e.Id == new2.Id);
                    new2c = context.Set<RequiredSingleComposite2>().Single(e => e.Id == new2c.Id);
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

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Equal(root.AlternateId, new1.RootId);
                Assert.Equal(new1.AlternateId, new2.BackId);
                Assert.Equal(new1.Id, new2c.BackId);
                Assert.Equal(new1.AlternateId, new2c.BackAlternateId);
                Assert.Same(root, new1.Root);
                Assert.Same(new1, new2.Back);
                Assert.Same(new1, new2c.Back);

                Assert.Null(old1.Root);
                Assert.Null(old2.Back);
                Assert.Null(old2c.Back);
                Assert.Equal(old1.AlternateId, old2.BackId);
                Assert.Equal(old1.Id, old2c.BackId);
                Assert.Equal(old1.AlternateId, old2c.BackAlternateId);
            }, async context =>
            {
                await LoadRootAsync(context);

                Assert.False(context.Set<RequiredSingleAk1>().Any(e => e.Id == old1.Id));
                Assert.False(context.Set<RequiredSingleAk2>().Any(e => e.Id == old2.Id));
                Assert.False(context.Set<RequiredSingleComposite2>().Any(e => e.Id == old2c.Id));
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
    public virtual Task Save_required_non_PK_one_to_one_changed_by_reference_with_alternate_key(
        ChangeMechanism changeMechanism,
        bool useExistingEntities)
    {
        RequiredNonPkSingleAk2 new2 = null;
        RequiredNonPkSingleAk2Derived new2d = null;
        RequiredNonPkSingleAk2MoreDerived new2dd = null;
        RequiredNonPkSingleAk1 new1 = null;
        RequiredNonPkSingleAk1Derived new1d = null;
        RequiredNonPkSingleAk1MoreDerived new1dd = null;
        Root newRoot;
        RequiredNonPkSingleAk1 old1 = null;
        RequiredNonPkSingleAk1Derived old1d = null;
        RequiredNonPkSingleAk1MoreDerived old1dd = null;
        RequiredNonPkSingleAk2 old2 = null;
        RequiredNonPkSingleAk2Derived old2d = null;
        RequiredNonPkSingleAk2MoreDerived old2dd = null;

        return ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                new2 = context.CreateProxy<RequiredNonPkSingleAk2>(e => e.AlternateId = Guid.NewGuid());
                new2d = context.CreateProxy<RequiredNonPkSingleAk2Derived>(e => e.AlternateId = Guid.NewGuid());
                new2dd = context.CreateProxy<RequiredNonPkSingleAk2MoreDerived>(e => e.AlternateId = Guid.NewGuid());
                new1 = context.CreateProxy<RequiredNonPkSingleAk1>(
                    e =>
                    {
                        e.AlternateId = Guid.NewGuid();
                        e.Single = new2;
                    });
                new1d = context.CreateProxy<RequiredNonPkSingleAk1Derived>(
                    e =>
                    {
                        e.AlternateId = Guid.NewGuid();
                        e.Single = new2d;
                        e.Root = context.CreateProxy<Root>();
                    });
                new1dd = context.CreateProxy<RequiredNonPkSingleAk1MoreDerived>(
                    e =>
                    {
                        e.AlternateId = Guid.NewGuid();
                        e.Single = new2dd;
                        e.Root = context.CreateProxy<Root>();
                        e.DerivedRoot = context.CreateProxy<Root>();
                    });
                newRoot = context.CreateProxy<Root>(
                    e =>
                    {
                        e.AlternateId = Guid.NewGuid();
                        e.RequiredNonPkSingleAk = new1;
                        e.RequiredNonPkSingleAkDerived = new1d;
                        e.RequiredNonPkSingleAkMoreDerived = new1dd;
                    });

                if (useExistingEntities)
                {
                    context.AddRange(newRoot, new1, new1d, new1dd, new2, new2d, new2dd);
                    context.SaveChanges();
                }

                return Task.CompletedTask;
            }, async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.RequiredNonPkSingleAk).Load();
                    context.Entry(root).Reference(e => e.RequiredNonPkSingleAkDerived).Load();
                    context.Entry(root).Reference(e => e.RequiredNonPkSingleAkMoreDerived).Load();
                }

                old1 = root.RequiredNonPkSingleAk;
                old1d = root.RequiredNonPkSingleAkDerived;
                old1dd = root.RequiredNonPkSingleAkMoreDerived;

                if (!DoesLazyLoading)
                {
                    context.Entry(old1).Reference(e => e.Single).Load();
                    context.Entry(old1d).Reference(e => e.Single).Load();
                    context.Entry(old1dd).Reference(e => e.Single).Load();
                    context.Entry(old1d).Reference(e => e.Root).Load();
                    context.Entry(old1dd).Reference(e => e.Root).Load();
                    context.Entry(old1dd).Reference(e => e.DerivedRoot).Load();
                }

                old2 = root.RequiredNonPkSingleAk.Single;
                old2d = (RequiredNonPkSingleAk2Derived)root.RequiredNonPkSingleAkDerived.Single;
                old2dd = (RequiredNonPkSingleAk2MoreDerived)root.RequiredNonPkSingleAkMoreDerived.Single;

                context.Set<RequiredNonPkSingleAk1>().Remove(old1d);
                context.Set<RequiredNonPkSingleAk1>().Remove(old1dd);

                if (useExistingEntities)
                {
                    new1 = context.Set<RequiredNonPkSingleAk1>().Single(e => e.Id == new1.Id);
                    new1d = (RequiredNonPkSingleAk1Derived)context.Set<RequiredNonPkSingleAk1>().Single(e => e.Id == new1d.Id);
                    new1dd = (RequiredNonPkSingleAk1MoreDerived)context.Set<RequiredNonPkSingleAk1>().Single(e => e.Id == new1dd.Id);
                    new2 = context.Set<RequiredNonPkSingleAk2>().Single(e => e.Id == new2.Id);
                    new2d = (RequiredNonPkSingleAk2Derived)context.Set<RequiredNonPkSingleAk2>().Single(e => e.Id == new2d.Id);
                    new2dd = (RequiredNonPkSingleAk2MoreDerived)context.Set<RequiredNonPkSingleAk2>().Single(e => e.Id == new2dd.Id);

                    new1d.RootId = old1d.RootId;
                    new1dd.RootId = old1dd.RootId;
                    new1dd.DerivedRootId = old1dd.DerivedRootId;
                }
                else
                {
                    new1d.RootId = old1d.RootId;
                    new1dd.RootId = old1dd.RootId;
                    new1dd.DerivedRootId = old1dd.DerivedRootId;
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

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

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
            }, async context =>
            {
                var loadedRoot = await LoadRootAsync(context);

                Assert.False(context.Set<RequiredNonPkSingleAk1>().Any(e => e.Id == old1.Id));
                Assert.False(context.Set<RequiredNonPkSingleAk1>().Any(e => e.Id == old1d.Id));
                Assert.False(context.Set<RequiredNonPkSingleAk1>().Any(e => e.Id == old1dd.Id));
                Assert.False(context.Set<RequiredNonPkSingleAk2>().Any(e => e.Id == old2.Id));
                Assert.False(context.Set<RequiredNonPkSingleAk2>().Any(e => e.Id == old2d.Id));
                Assert.False(context.Set<RequiredNonPkSingleAk2>().Any(e => e.Id == old2dd.Id));
            });
    }

    [ConditionalTheory]
    [InlineData((int)ChangeMechanism.Dependent)]
    [InlineData((int)ChangeMechanism.Principal)]
    [InlineData((int)ChangeMechanism.Fk)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk))]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent))]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk))]
    public virtual Task Sever_optional_one_to_one_with_alternate_key(ChangeMechanism changeMechanism)
    {
        Root root = null;
        OptionalSingleAk1 old1 = null;
        OptionalSingleAk2 old2 = null;
        OptionalSingleComposite2 old2c = null;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.OptionalSingleAk).Load();
                }

                old1 = root.OptionalSingleAk;

                if (!DoesLazyLoading)
                {
                    context.Entry(old1).Reference(e => e.Single).Load();
                    context.Entry(old1).Reference(e => e.SingleComposite).Load();
                }

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

                context.SaveChanges();

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
                    var loadedRoot = await LoadRootAsync(context);

                    var loaded1 = context.Set<OptionalSingleAk1>().Single(e => e.Id == old1.Id);
                    var loaded2 = context.Set<OptionalSingleAk2>().Single(e => e.Id == old2.Id);
                    var loaded2c = context.Set<OptionalSingleComposite2>().Single(e => e.Id == old2c.Id);

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
    [InlineData((int)ChangeMechanism.Dependent)]
    [InlineData((int)ChangeMechanism.Principal)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
    public virtual Task Sever_required_one_to_one_with_alternate_key(ChangeMechanism changeMechanism)
    {
        Root root = null;
        RequiredSingleAk1 old1 = null;
        RequiredSingleAk2 old2 = null;
        RequiredSingleComposite2 old2c = null;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.RequiredSingleAk).Load();
                }

                old1 = root.RequiredSingleAk;

                if (!DoesLazyLoading)
                {
                    context.Entry(old1).Reference(e => e.Single).Load();
                    context.Entry(old1).Reference(e => e.SingleComposite).Load();
                }

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

                Assert.False(context.Entry(root).Reference(e => e.RequiredSingleAk).IsLoaded);
                Assert.False(context.Entry(old1).Reference(e => e.Root).IsLoaded);
                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Null(old1.Root);
                Assert.Null(old2.Back);
                Assert.Null(old2c.Back);
                Assert.Equal(old1.AlternateId, old2.BackId);
                Assert.Equal(old1.Id, old2c.BackId);
                Assert.Equal(old1.AlternateId, old2c.BackAlternateId);
            }, async context =>
            {
                var loadedRoot = await LoadRootAsync(context);

                Assert.False(context.Set<RequiredSingleAk1>().Any(e => e.Id == old1.Id));
                Assert.False(context.Set<RequiredSingleAk2>().Any(e => e.Id == old2.Id));
                Assert.False(context.Set<RequiredSingleComposite2>().Any(e => e.Id == old2c.Id));
            });
    }

    [ConditionalTheory]
    [InlineData((int)ChangeMechanism.Dependent)]
    [InlineData((int)ChangeMechanism.Principal)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
    public virtual Task Sever_required_non_PK_one_to_one_with_alternate_key(ChangeMechanism changeMechanism)
    {
        Root root = null;
        RequiredNonPkSingleAk1 old1 = null;
        RequiredNonPkSingleAk2 old2 = null;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.RequiredNonPkSingleAk).Load();
                }

                old1 = root.RequiredNonPkSingleAk;

                if (!DoesLazyLoading)
                {
                    context.Entry(old1).Reference(e => e.Single).Load();
                }

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

                if (!DoesChangeTracking)
                {
                    context.ChangeTracker.DetectChanges();
                    context.ChangeTracker.DetectChanges();
                }

                Assert.False(context.Entry(root).Reference(e => e.RequiredNonPkSingleAk).IsLoaded);
                Assert.False(context.Entry(old1).Reference(e => e.Root).IsLoaded);
                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Null(old1.Root);
                Assert.Null(old2.Back);
                Assert.Equal(old1.AlternateId, old2.BackId);
            }, async context =>
            {
                var loadedRoot = await LoadRootAsync(context);

                Assert.False(context.Set<RequiredNonPkSingleAk1>().Any(e => e.Id == old1.Id));
                Assert.False(context.Set<RequiredNonPkSingleAk2>().Any(e => e.Id == old2.Id));
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
    public virtual Task Reparent_optional_one_to_one_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingRoot)
    {
        Root newRoot = null;
        Root root;
        OptionalSingleAk1 old1 = null;
        OptionalSingleAk2 old2 = null;
        OptionalSingleComposite2 old2c = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                newRoot = context.CreateProxy<Root>(e => e.AlternateId = Guid.NewGuid());

                if (useExistingRoot)
                {
                    context.Add(newRoot);
                    await context.SaveChangesAsync();
                }
            }, async context =>
            {
                root = await LoadRootAsync(context);

                context.Entry(newRoot).State = useExistingRoot ? EntityState.Unchanged : EntityState.Added;

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.OptionalSingleAk).Load();
                }

                old1 = root.OptionalSingleAk;

                if (!DoesLazyLoading)
                {
                    context.Entry(old1).Reference(e => e.Single).Load();
                    context.Entry(old1).Reference(e => e.SingleComposite).Load();
                }

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

                context.SaveChanges();

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
                await LoadRootAsync(context);

                newRoot = context.Set<Root>().Single(e => e.Id == newRoot.Id);
                var loaded1 = context.Set<OptionalSingleAk1>().Single(e => e.Id == old1.Id);
                var loaded2 = context.Set<OptionalSingleAk2>().Single(e => e.Id == old2.Id);
                var loaded2c = context.Set<OptionalSingleComposite2>().Single(e => e.Id == old2c.Id);

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
    public virtual Task Reparent_required_one_to_one_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingRoot)
    {
        Root newRoot = null;
        Root root;
        RequiredSingleAk1 old1 = null;
        RequiredSingleAk2 old2 = null;
        RequiredSingleComposite2 old2c = null;

        return ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                newRoot = context.CreateProxy<Root>(e => e.AlternateId = Guid.NewGuid());

                if (useExistingRoot)
                {
                    context.Add(newRoot);
                    context.SaveChanges();
                }

                return Task.CompletedTask;
            }, async context =>
            {
                root = await LoadRootAsync(context);

                context.Entry(newRoot).State = useExistingRoot ? EntityState.Unchanged : EntityState.Added;

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.RequiredSingleAk).Load();
                }

                old1 = root.RequiredSingleAk;

                if (!DoesLazyLoading)
                {
                    context.Entry(old1).Reference(e => e.Single).Load();
                    context.Entry(old1).Reference(e => e.SingleComposite).Load();
                }

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

                context.SaveChanges();

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
                var loadedRoot = await LoadRootAsync(context);

                newRoot = context.Set<Root>().Single(e => e.Id == newRoot.Id);
                var loaded1 = context.Set<RequiredSingleAk1>().Single(e => e.Id == old1.Id);
                var loaded2 = context.Set<RequiredSingleAk2>().Single(e => e.Id == old2.Id);
                var loaded2c = context.Set<RequiredSingleComposite2>().Single(e => e.Id == old2c.Id);

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
    public virtual Task Reparent_required_non_PK_one_to_one_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingRoot)
    {
        Root newRoot = null;
        Root root;
        RequiredNonPkSingleAk1 old1 = null;
        RequiredNonPkSingleAk2 old2 = null;

        return ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                newRoot = context.CreateProxy<Root>(e => e.AlternateId = Guid.NewGuid());

                if (useExistingRoot)
                {
                    context.Add(newRoot);
                    context.SaveChanges();
                }

                return Task.CompletedTask;
            }, async context =>
            {
                root = await LoadRootAsync(context);

                context.Entry(newRoot).State = useExistingRoot ? EntityState.Unchanged : EntityState.Added;

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.RequiredNonPkSingleAk).Load();
                }

                old1 = root.RequiredNonPkSingleAk;

                if (!DoesLazyLoading)
                {
                    context.Entry(old1).Reference(e => e.Single).Load();
                }

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

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Null(root.RequiredNonPkSingleAk);

                Assert.Same(newRoot, old1.Root);
                Assert.Same(old1, old2.Back);
                Assert.Equal(newRoot.AlternateId, old1.RootId);
                Assert.Equal(old1.AlternateId, old2.BackId);
            }, async context =>
            {
                var loadedRoot = await LoadRootAsync(context);

                newRoot = context.Set<Root>().Single(e => e.Id == newRoot.Id);
                var loaded1 = context.Set<RequiredNonPkSingleAk1>().Single(e => e.Id == old1.Id);
                var loaded2 = context.Set<RequiredNonPkSingleAk2>().Single(e => e.Id == old2.Id);

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
    public virtual Task Optional_one_to_one_with_alternate_key_are_orphaned(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;
        var orphanedIdC = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.OptionalSingleAk).Load();
                }

                var removed = root.OptionalSingleAk;

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Reference(e => e.Single).Load();
                    context.Entry(removed).Reference(e => e.SingleComposite).Load();
                }

                removedId = removed.Id;
                var orphaned = removed.Single;
                var orphanedC = removed.SingleComposite;
                orphanedId = orphaned.Id;
                orphanedIdC = orphanedC.Id;

                context.Remove(removed);

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

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
            }, async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.OptionalSingleAk).Load();
                }

                Assert.Null(root.OptionalSingleAk);

                Assert.Empty(context.Set<OptionalSingleAk1>().Where(e => e.Id == removedId));
                Assert.Equal(1, context.Set<OptionalSingleAk2>().Count(e => e.Id == orphanedId));
                Assert.Equal(1, context.Set<OptionalSingleComposite2>().Count(e => e.Id == orphanedIdC));
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
    public virtual Task Required_one_to_one_with_alternate_key_are_cascade_deleted(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;
        var orphanedIdC = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.RequiredSingleAk).Load();
                }

                var removed = root.RequiredSingleAk;

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Reference(e => e.Single).Load();
                    context.Entry(removed).Reference(e => e.SingleComposite).Load();
                }

                removedId = removed.Id;
                var orphaned = removed.Single;
                var orphanedC = removed.SingleComposite;
                orphanedId = orphaned.Id;
                orphanedIdC = orphanedC.Id;

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
                    Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);
                    Assert.Equal(EntityState.Detached, context.Entry(orphanedC).State);

                    Assert.Null(root.RequiredSingleAk);

                    Assert.Empty(context.Set<RequiredSingleAk1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<RequiredSingleAk2>().Where(e => e.Id == orphanedId));
                    Assert.Empty(context.Set<RequiredSingleComposite2>().Where(e => e.Id == orphanedIdC));

                    Assert.Same(root, removed.Root);
                    Assert.Same(orphaned, removed.Single);
                }
            }, async context =>
            {
                if (cascadeDeleteTiming != CascadeTiming.Never)
                {
                    var root = await LoadRootAsync(context);

                    if (!DoesLazyLoading)
                    {
                        context.Entry(root).Reference(e => e.RequiredSingleAk).Load();
                    }

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
    public virtual Task Required_non_PK_one_to_one_with_alternate_key_are_cascade_deleted(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.RequiredNonPkSingleAk).Load();
                }

                var removed = root.RequiredNonPkSingleAk;

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Reference(e => e.Single).Load();
                }

                removedId = removed.Id;
                var orphaned = removed.Single;
                orphanedId = orphaned.Id;

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
                    Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);

                    Assert.Null(root.RequiredNonPkSingleAk);

                    Assert.Empty(context.Set<RequiredNonPkSingleAk1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<RequiredNonPkSingleAk2>().Where(e => e.Id == orphanedId));

                    Assert.Same(root, removed.Root);
                    Assert.Same(orphaned, removed.Single);
                }
            }, async context =>
            {
                if (cascadeDeleteTiming != CascadeTiming.Never)
                {
                    var root = await LoadRootAsync(context);

                    if (!DoesLazyLoading)
                    {
                        context.Entry(root).Reference(e => e.RequiredNonPkSingleAk).Load();
                    }

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
    public virtual Task Required_one_to_one_with_alternate_key_are_cascade_deleted_in_store(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;
        var orphanedIdC = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.RequiredSingleAk).Load();
                }

                var removed = root.RequiredSingleAk;

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Reference(e => e.Single).Load();
                    context.Entry(removed).Reference(e => e.SingleComposite).Load();
                }

                removedId = removed.Id;
                orphanedId = removed.Single.Id;
                orphanedIdC = removed.SingleComposite.Id;
            }, async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

                var root = await context.Set<Root>().Include(e => e.RequiredSingleAk).SingleAsync(IsTheRoot);

                var removed = root.RequiredSingleAk;

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Reference(e => e.Single).Load();
                }

                var orphaned = removed.Single;

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

                    Assert.Null(root.RequiredSingleAk);

                    Assert.Empty(context.Set<RequiredSingleAk1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<RequiredSingleAk2>().Where(e => e.Id == orphanedId));
                    Assert.Empty(context.Set<RequiredSingleComposite2>().Where(e => e.Id == orphanedIdC));

                    Assert.Same(root, removed.Root);
                    Assert.Same(orphaned, removed.Single);
                }
            }, async context =>
            {
                if (cascadeDeleteTiming != CascadeTiming.Never)
                {
                    var root = await LoadRootAsync(context);

                    if (!DoesLazyLoading)
                    {
                        context.Entry(root).Reference(e => e.RequiredSingleAk).Load();
                    }

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
    public virtual Task Required_non_PK_one_to_one_with_alternate_key_are_cascade_deleted_in_store(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.RequiredNonPkSingleAk).Load();
                }

                var removed = root.RequiredNonPkSingleAk;

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Reference(e => e.Single).Load();
                }

                removedId = removed.Id;
                orphanedId = removed.Single.Id;
            },
            context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

                var root = context.Set<Root>().Include(e => e.RequiredNonPkSingleAk).Single(IsTheRoot);

                var removed = root.RequiredNonPkSingleAk;

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Reference(e => e.Single).Load();
                }

                var orphaned = removed.Single;

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

                    Assert.Null(root.RequiredNonPkSingleAk);

                    Assert.Empty(context.Set<RequiredNonPkSingleAk1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<RequiredNonPkSingleAk2>().Where(e => e.Id == orphanedId));

                    Assert.Same(root, removed.Root);
                    Assert.Same(orphaned, removed.Single);
                }

                return Task.CompletedTask;
            }, async context =>
            {
                if (cascadeDeleteTiming != CascadeTiming.Never)
                {
                    var root = await LoadRootAsync(context);

                    if (!DoesLazyLoading)
                    {
                        context.Entry(root).Reference(e => e.RequiredNonPkSingleAk).Load();
                    }

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
    public virtual Task Optional_one_to_one_with_alternate_key_are_orphaned_in_store(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;
        var orphanedIdC = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.OptionalSingleAk).Load();
                }

                var removed = root.OptionalSingleAk;

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Reference(e => e.Single).Load();
                    context.Entry(removed).Reference(e => e.SingleComposite).Load();
                }

                removedId = removed.Id;
                orphanedId = removed.Single.Id;
                orphanedIdC = removed.SingleComposite.Id;
            },
            context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

                var root = context.Set<Root>().Include(e => e.OptionalSingleAk).Single(IsTheRoot);

                var removed = root.OptionalSingleAk;

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Reference(e => e.Single).Load();
                }

                var orphaned = removed.Single;

                context.Remove(removed);

                // Cannot have SET NULL action in the store because one of the FK columns
                // is not nullable, so need to do this on the EF side.
                context.Set<OptionalSingleComposite2>().Single(e => e.Id == orphanedIdC).BackId = null;

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Null(root.OptionalSingleAk);

                Assert.Empty(context.Set<OptionalSingleAk1>().Where(e => e.Id == removedId));
                Assert.Null(context.Set<OptionalSingleAk2>().Single(e => e.Id == orphanedId).BackId);
                Assert.Null(context.Set<OptionalSingleComposite2>().Single(e => e.Id == orphanedIdC).BackId);

                Assert.Same(root, removed.Root);
                Assert.Same(orphaned, removed.Single);
                return Task.CompletedTask;
            }, async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.OptionalSingleAk).Load();
                }

                Assert.Null(root.OptionalSingleAk);

                Assert.Empty(context.Set<OptionalSingleAk1>().Where(e => e.Id == removedId));
                Assert.Null(context.Set<OptionalSingleAk2>().Single(e => e.Id == orphanedId).BackId);
                Assert.Null(context.Set<OptionalSingleComposite2>().Single(e => e.Id == orphanedIdC).BackId);
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
    public virtual Task Optional_one_to_one_with_alternate_key_are_orphaned_starting_detached(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;
        var orphanedIdC = 0;
        Root root = null;
        OptionalSingleAk1 removed = null;
        OptionalSingleAk2 orphaned = null;
        OptionalSingleComposite2 orphanedC = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.OptionalSingleAk).Load();
                }

                removed = root.OptionalSingleAk;

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Reference(e => e.Single).Load();
                    context.Entry(removed).Reference(e => e.SingleComposite).Load();
                }

                orphaned = removed.Single;
                orphanedC = removed.SingleComposite;
            },
            context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

                removedId = removed.Id;
                orphanedId = orphaned.Id;
                orphanedIdC = orphanedC.Id;

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);

                var expectedState = cascadeDeleteTiming == CascadeTiming.Immediate
                    ? EntityState.Modified
                    : EntityState.Unchanged;

                Assert.Equal(expectedState, context.Entry(orphaned).State);
                Assert.Equal(expectedState, context.Entry(orphanedC).State);

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(orphanedC).State);

                Assert.Same(root, removed.Root);
                Assert.Same(orphaned, removed.Single);
                return Task.CompletedTask;
            }, async context =>
            {
                root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.OptionalSingleAk).Load();
                }

                Assert.Null(root.OptionalSingleAk);

                Assert.Empty(context.Set<OptionalSingleAk1>().Where(e => e.Id == removedId));
                Assert.Equal(1, context.Set<OptionalSingleAk2>().Count(e => e.Id == orphanedId));
                Assert.Equal(1, context.Set<OptionalSingleComposite2>().Count(e => e.Id == orphanedIdC));
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
    public virtual Task Required_one_to_one_with_alternate_key_are_cascade_deleted_starting_detached(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;
        var orphanedIdC = 0;
        Root root = null;
        RequiredSingleAk1 removed = null;
        RequiredSingleAk2 orphaned = null;
        RequiredSingleComposite2 orphanedC = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.RequiredSingleAk).Load();
                }

                removed = root.RequiredSingleAk;

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Reference(e => e.Single).Load();
                    context.Entry(removed).Reference(e => e.SingleComposite).Load();
                }

                orphaned = removed.Single;
                orphanedC = removed.SingleComposite;
            },
            context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

                removedId = removed.Id;
                orphanedId = orphaned.Id;
                orphanedIdC = orphanedC.Id;

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);

                var expectedState = cascadeDeleteTiming == CascadeTiming.Immediate
                    ? EntityState.Deleted
                    : EntityState.Unchanged;

                Assert.Equal(expectedState, context.Entry(orphaned).State);
                Assert.Equal(expectedState, context.Entry(orphanedC).State);

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
                    Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);
                    Assert.Equal(EntityState.Detached, context.Entry(orphanedC).State);

                    Assert.Same(root, removed.Root);
                    Assert.Same(orphaned, removed.Single);
                }

                return Task.CompletedTask;
            }, async context =>
            {
                if (cascadeDeleteTiming != CascadeTiming.Never)
                {
                    root = await LoadRootAsync(context);

                    if (!DoesLazyLoading)
                    {
                        context.Entry(root).Reference(e => e.RequiredSingleAk).Load();
                    }

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
    public virtual Task Required_non_PK_one_to_one_with_alternate_key_are_cascade_deleted_starting_detached(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;
        Root root = null;
        RequiredNonPkSingleAk1 removed = null;
        RequiredNonPkSingleAk2 orphaned = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.RequiredNonPkSingleAk).Load();
                }

                removed = root.RequiredNonPkSingleAk;

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Reference(e => e.Single).Load();
                }

                orphaned = removed.Single;
            },
            context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

                removedId = removed.Id;
                orphanedId = orphaned.Id;

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);

                var expectedState = cascadeDeleteTiming == CascadeTiming.Immediate
                    ? EntityState.Deleted
                    : EntityState.Unchanged;

                Assert.Equal(expectedState, context.Entry(orphaned).State);

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
                    Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);

                    Assert.Same(root, removed.Root);
                    Assert.Same(orphaned, removed.Single);
                }

                return Task.CompletedTask;
            }, async context =>
            {
                if (cascadeDeleteTiming != CascadeTiming.Never)
                {
                    root = await LoadRootAsync(context);

                    if (!DoesLazyLoading)
                    {
                        context.Entry(root).Reference(e => e.RequiredNonPkSingleAk).Load();
                    }

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
    public virtual Task Required_one_to_one_with_alternate_key_are_cascade_detached_when_Added(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;
        var orphanedIdC = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.RequiredSingleAk).Load();
                }

                var removed = root.RequiredSingleAk;

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Reference(e => e.Single).Load();
                    context.Entry(removed).Reference(e => e.SingleComposite).Load();
                }

                removedId = removed.Id;
                var orphaned = removed.Single;
                var orphanedC = removed.SingleComposite;
                orphanedId = orphaned.Id;
                orphanedIdC = orphanedC.Id;

                context.Entry(orphaned).State = EntityState.Added;
                context.Entry(orphanedC).State = EntityState.Added;

                Assert.Equal(EntityState.Unchanged, context.Entry(removed).State);
                Assert.Equal(EntityState.Added, context.Entry(orphaned).State);
                Assert.Equal(EntityState.Added, context.Entry(orphanedC).State);

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);

                var expectedState = cascadeDeleteTiming == CascadeTiming.Immediate
                    ? EntityState.Detached
                    : EntityState.Added;

                Assert.Equal(expectedState, context.Entry(orphaned).State);
                Assert.Equal(expectedState, context.Entry(orphanedC).State);

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
                    Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);
                    Assert.Equal(EntityState.Detached, context.Entry(orphanedC).State);

                    Assert.Same(root, removed.Root);
                    Assert.Same(orphaned, removed.Single);
                }
            }, async context =>
            {
                if (cascadeDeleteTiming != CascadeTiming.Never)
                {
                    var root = await LoadRootAsync(context);

                    if (!DoesLazyLoading)
                    {
                        context.Entry(root).Reference(e => e.RequiredSingleAk).Load();
                    }

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
    public virtual Task Required_non_PK_one_to_one_with_alternate_key_are_cascade_detached_when_Added(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.RequiredNonPkSingleAk).Load();
                }

                var removed = root.RequiredNonPkSingleAk;

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Reference(e => e.Single).Load();
                }

                removedId = removed.Id;
                var orphaned = removed.Single;
                orphanedId = orphaned.Id;

                context.Entry(orphaned).State = EntityState.Added;

                Assert.Equal(EntityState.Unchanged, context.Entry(removed).State);
                Assert.Equal(EntityState.Added, context.Entry(orphaned).State);

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);

                var expectedState = cascadeDeleteTiming == CascadeTiming.Immediate
                    ? EntityState.Detached
                    : EntityState.Added;

                Assert.Equal(expectedState, context.Entry(orphaned).State);

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
                    Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);

                    Assert.Same(root, removed.Root);
                    Assert.Same(orphaned, removed.Single);
                }
            }, async context =>
            {
                if (cascadeDeleteTiming != CascadeTiming.Never)
                {
                    var root = await LoadRootAsync(context);

                    if (!DoesLazyLoading)
                    {
                        context.Entry(root).Reference(e => e.RequiredNonPkSingleAk).Load();
                    }

                    Assert.Null(root.RequiredNonPkSingleAk);

                    Assert.Empty(context.Set<RequiredNonPkSingleAk1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<RequiredNonPkSingleAk2>().Where(e => e.Id == orphanedId));
                }
            });
    }
}
