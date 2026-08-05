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
    [ConditionalFact]
    public virtual Task Optional_one_to_one_relationships_are_one_to_one()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var root = context.Set<Root>().Single(IsTheRoot);

                root.OptionalSingle = context.CreateProxy<OptionalSingle1>();

                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                return Task.CompletedTask;
            });

    [ConditionalFact]
    public virtual Task Required_one_to_one_relationships_are_one_to_one()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var root = context.Set<Root>().Single(IsTheRoot);

                root.RequiredSingle = context.CreateProxy<RequiredSingle1>();

                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                return Task.CompletedTask;
            });

    [ConditionalFact]
    public virtual Task Optional_one_to_one_with_AK_relationships_are_one_to_one()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var root = context.Set<Root>().Single(IsTheRoot);

                root.OptionalSingleAk = context.CreateProxy<OptionalSingleAk1>();

                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                return Task.CompletedTask;
            });

    [ConditionalFact]
    public virtual Task Required_one_to_one_with_AK_relationships_are_one_to_one()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var root = context.Set<Root>().Single(IsTheRoot);

                root.RequiredSingleAk = context.CreateProxy<RequiredSingleAk1>();

                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                return Task.CompletedTask;
            });

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
    public virtual Task Save_changed_optional_one_to_one(ChangeMechanism changeMechanism, bool useExistingEntities)
    {
        OptionalSingle2 new2 = null;
        OptionalSingle2Derived new2d = null;
        OptionalSingle2MoreDerived new2dd = null;
        OptionalSingle1 new1 = null;
        OptionalSingle1Derived new1d = null;
        OptionalSingle1MoreDerived new1dd = null;
        OptionalSingle1 old1 = null;
        OptionalSingle1Derived old1d = null;
        OptionalSingle1MoreDerived old1dd = null;
        OptionalSingle2 old2 = null;
        OptionalSingle2Derived old2d = null;
        OptionalSingle2MoreDerived old2dd = null;

        return ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                new2 = context.CreateProxy<OptionalSingle2>();
                new2d = context.CreateProxy<OptionalSingle2Derived>();
                new2dd = context.CreateProxy<OptionalSingle2MoreDerived>();
                new1 = context.CreateProxy<OptionalSingle1>(e => e.Single = new2);
                new1d = context.CreateProxy<OptionalSingle1Derived>(e => e.Single = new2d);
                new1dd = context.CreateProxy<OptionalSingle1MoreDerived>(e => e.Single = new2dd);

                if (useExistingEntities)
                {
                    context.AddRange(new1, new1d, new1dd, new2, new2d, new2dd);
                    context.SaveChanges();
                }

                return Task.CompletedTask;
            }, async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.OptionalSingle).Load();
                    context.Entry(root).Reference(e => e.OptionalSingleDerived).Load();
                    context.Entry(root).Reference(e => e.OptionalSingleMoreDerived).Load();
                }

                old1 = root.OptionalSingle;
                old1d = root.OptionalSingleDerived;
                old1dd = root.OptionalSingleMoreDerived;

                if (!DoesLazyLoading)
                {
                    context.Entry(old1).Reference(e => e.Single).Load();
                    context.Entry(old1d).Reference(e => e.Single).Load();
                    context.Entry(old1dd).Reference(e => e.Single).Load();
                }

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
            }, async context =>
            {
                await LoadRootAsync(context);

                var loaded1 = context.Set<OptionalSingle1>().Single(e => e.Id == old1.Id);
                var loaded1d = context.Set<OptionalSingle1>().Single(e => e.Id == old1d.Id);
                var loaded1dd = context.Set<OptionalSingle1>().Single(e => e.Id == old1dd.Id);
                var loaded2 = context.Set<OptionalSingle2>().Single(e => e.Id == old2.Id);
                var loaded2d = context.Set<OptionalSingle2>().Single(e => e.Id == old2d.Id);
                var loaded2dd = context.Set<OptionalSingle2>().Single(e => e.Id == old2dd.Id);

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
    [InlineData((int)ChangeMechanism.Dependent)]
    [InlineData((int)ChangeMechanism.Principal)]
    [InlineData((int)ChangeMechanism.Fk)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk))]
    [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent))]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk))]
    public virtual async Task Save_required_one_to_one_changed_by_reference(ChangeMechanism changeMechanism)
    {
        RequiredSingle1 old1 = null;
        RequiredSingle2 old2 = null;
        Root oldRoot;
        RequiredSingle2 new2 = null;
        RequiredSingle1 new1 = null;
        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                oldRoot = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(oldRoot).Reference(e => e.RequiredSingle).Load();
                }

                old1 = oldRoot.RequiredSingle;

                if (!DoesLazyLoading)
                {
                    context.Entry(old1).Reference(e => e.Single).Load();
                }

                old2 = oldRoot.RequiredSingle.Single;

                context.Entry(oldRoot).State = EntityState.Detached;
                context.Entry(old1).State = EntityState.Detached;
                context.Entry(old2).State = EntityState.Detached;

                new2 = context.CreateProxy<RequiredSingle2>();
                new1 = context.CreateProxy<RequiredSingle1>(e => e.Single = new2);
            });

        await ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var root = context.Set<Root>().Include(e => e.RequiredSingle.Single).Single(IsTheRoot);

                context.Entry(root.RequiredSingle.Single).State = EntityState.Deleted;
                context.Entry(root.RequiredSingle).State = EntityState.Deleted;

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
                    context.Entry(new1).Property(e => e.Id).IsTemporary = false;
                    context.Entry(new2).Property(e => e.Id).IsTemporary = false;
                }

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Equal(root.Id, new1.Id);
                Assert.Equal(new1.Id, new2.Id);
                Assert.Same(root, new1.Root);
                Assert.Same(new1, new2.Back);

                Assert.NotNull(old1.Root);
                Assert.Same(old1, old2.Back);
                Assert.Equal(old1.Id, old2.Id);
                return Task.CompletedTask;
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
    public virtual Task Save_required_non_PK_one_to_one_changed_by_reference(ChangeMechanism changeMechanism, bool useExistingEntities)
    {
        RequiredNonPkSingle2 new2 = null;
        RequiredNonPkSingle2Derived new2d = null;
        RequiredNonPkSingle2MoreDerived new2dd = null;
        RequiredNonPkSingle1 new1 = null;
        RequiredNonPkSingle1Derived new1d = null;
        RequiredNonPkSingle1MoreDerived new1dd = null;
        Root newRoot;
        RequiredNonPkSingle1 old1 = null;
        RequiredNonPkSingle1Derived old1d = null;
        RequiredNonPkSingle1MoreDerived old1dd = null;
        RequiredNonPkSingle2 old2 = null;
        RequiredNonPkSingle2Derived old2d = null;
        RequiredNonPkSingle2MoreDerived old2dd = null;

        return ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                new2 = context.CreateProxy<RequiredNonPkSingle2>();
                new2d = context.CreateProxy<RequiredNonPkSingle2Derived>();
                new2dd = context.CreateProxy<RequiredNonPkSingle2MoreDerived>();
                new1 = context.CreateProxy<RequiredNonPkSingle1>(e => e.Single = new2);
                new1d = context.CreateProxy<RequiredNonPkSingle1Derived>(
                    e =>
                    {
                        e.Single = new2d;
                        e.Root = context.CreateProxy<Root>();
                    });
                new1dd = context.CreateProxy<RequiredNonPkSingle1MoreDerived>(
                    e =>
                    {
                        e.Single = new2dd;
                        e.Root = context.CreateProxy<Root>();
                        e.DerivedRoot = context.CreateProxy<Root>();
                    });
                newRoot = context.CreateProxy<Root>(
                    e =>
                    {
                        e.RequiredNonPkSingle = new1;
                        e.RequiredNonPkSingleDerived = new1d;
                        e.RequiredNonPkSingleMoreDerived = new1dd;
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
                    context.Entry(root).Reference(e => e.RequiredNonPkSingle).Load();
                    context.Entry(root).Reference(e => e.RequiredNonPkSingleDerived).Load();
                    context.Entry(root).Reference(e => e.RequiredNonPkSingleMoreDerived).Load();
                }

                old1 = root.RequiredNonPkSingle;
                old1d = root.RequiredNonPkSingleDerived;
                old1dd = root.RequiredNonPkSingleMoreDerived;

                if (!DoesLazyLoading)
                {
                    context.Entry(old1).Reference(e => e.Single).Load();
                    context.Entry(old1d).Reference(e => e.Single).Load();
                    context.Entry(old1dd).Reference(e => e.Single).Load();
                    context.Entry(old1d).Reference(e => e.Root).Load();
                    context.Entry(old1dd).Reference(e => e.Root).Load();
                    context.Entry(old1dd).Reference(e => e.DerivedRoot).Load();
                }

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
                    new1d.RootId = old1d.RootId;
                    new1dd.RootId = old1dd.RootId;
                    new1dd.DerivedRootId = old1dd.DerivedRootId;
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
                Assert.Null(old2.Back);
                Assert.Null(old2d.Back);
                Assert.Null(old2dd.Back);
                Assert.Equal(old1.Id, old2.BackId);
                Assert.Equal(old1d.Id, old2d.BackId);
                Assert.Equal(old1dd.Id, old2dd.BackId);
            }, async context =>
            {
                var loadedRoot = await LoadRootAsync(context);

                Assert.False(context.Set<RequiredNonPkSingle1>().Any(e => e.Id == old1.Id));
                Assert.False(context.Set<RequiredNonPkSingle1>().Any(e => e.Id == old1d.Id));
                Assert.False(context.Set<RequiredNonPkSingle1>().Any(e => e.Id == old1dd.Id));
                Assert.False(context.Set<RequiredNonPkSingle2>().Any(e => e.Id == old2.Id));
                Assert.False(context.Set<RequiredNonPkSingle2>().Any(e => e.Id == old2d.Id));
                Assert.False(context.Set<RequiredNonPkSingle2>().Any(e => e.Id == old2dd.Id));
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
    public virtual Task Sever_optional_one_to_one(ChangeMechanism changeMechanism)
    {
        Root root;
        OptionalSingle1 old1 = null;
        OptionalSingle2 old2 = null;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.OptionalSingle).Load();
                }

                old1 = root.OptionalSingle;

                if (!DoesLazyLoading)
                {
                    context.Entry(old1).Reference(e => e.Single).Load();
                }

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
            }, async context =>
            {
                if ((changeMechanism & ChangeMechanism.Fk) == 0)
                {
                    await LoadRootAsync(context);

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
    [InlineData((int)ChangeMechanism.Dependent)]
    [InlineData((int)ChangeMechanism.Principal)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
    public virtual Task Sever_required_one_to_one(ChangeMechanism changeMechanism)
    {
        Root root = null;
        RequiredSingle1 old1 = null;
        RequiredSingle2 old2 = null;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.RequiredSingle).Load();
                }

                old1 = root.RequiredSingle;

                if (!DoesLazyLoading)
                {
                    context.Entry(old1).Reference(e => e.Single).Load();
                }

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

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Null(old1.Root);
                Assert.Null(old2.Back);
                Assert.Equal(old1.Id, old2.Id);
            }, async context =>
            {
                await LoadRootAsync(context);

                Assert.False(context.Set<RequiredSingle1>().Any(e => e.Id == old1.Id));
                Assert.False(context.Set<RequiredSingle2>().Any(e => e.Id == old2.Id));
            });
    }

    [ConditionalTheory]
    [InlineData((int)ChangeMechanism.Dependent)]
    [InlineData((int)ChangeMechanism.Principal)]
    [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
    public virtual Task Sever_required_non_PK_one_to_one(ChangeMechanism changeMechanism)
    {
        Root root;
        RequiredNonPkSingle1 old1 = null;
        RequiredNonPkSingle2 old2 = null;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.RequiredNonPkSingle).Load();
                }

                old1 = root.RequiredNonPkSingle;

                if (!DoesLazyLoading)
                {
                    context.Entry(old1).Reference(e => e.Single).Load();
                }

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

                Assert.False(context.Entry(root).Reference(e => e.RequiredNonPkSingle).IsLoaded);
                Assert.False(context.Entry(old1).Reference(e => e.Root).IsLoaded);
                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Null(old1.Root);
                Assert.Null(old2.Back);
                Assert.Equal(old1.Id, old2.BackId);
            }, async context =>
            {
                await LoadRootAsync(context);

                Assert.False(context.Set<RequiredNonPkSingle1>().Any(e => e.Id == old1.Id));
                Assert.False(context.Set<RequiredNonPkSingle2>().Any(e => e.Id == old2.Id));
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
    public virtual Task Reparent_optional_one_to_one(ChangeMechanism changeMechanism, bool useExistingRoot)
    {
        Root newRoot = null;
        Root root;
        OptionalSingle1 old1 = null;
        OptionalSingle2 old2 = null;

        return ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                newRoot = context.CreateProxy<Root>();

                if (useExistingRoot)
                {
                    context.AddRange(newRoot);
                    context.SaveChanges();
                }

                return Task.CompletedTask;
            }, async context =>
            {
                root = await LoadRootAsync(context);

                context.Entry(newRoot).State = useExistingRoot ? EntityState.Unchanged : EntityState.Added;

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.OptionalSingle).Load();
                }

                old1 = root.OptionalSingle;

                if (!DoesLazyLoading)
                {
                    context.Entry(old1).Reference(e => e.Single).Load();
                }

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
            }, async context =>
            {
                var loadedRoot = await LoadRootAsync(context);

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
    public virtual Task Reparent_required_one_to_one(ChangeMechanism changeMechanism, bool useExistingRoot)
    {
        Root newRoot = null;

        return ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                newRoot = context.CreateProxy<Root>();

                if (useExistingRoot)
                {
                    context.AddRange(newRoot);
                    context.SaveChanges();
                }

                return Task.CompletedTask;
            }, async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.RequiredSingle).Load();
                }

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
    public virtual Task Reparent_required_non_PK_one_to_one(ChangeMechanism changeMechanism, bool useExistingRoot)
    {
        Root newRoot = null;
        Root root;
        RequiredNonPkSingle1 old1 = null;
        RequiredNonPkSingle2 old2 = null;

        return ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                newRoot = context.CreateProxy<Root>();

                if (useExistingRoot)
                {
                    context.AddRange(newRoot);
                    context.SaveChanges();
                }

                return Task.CompletedTask;
            }, async context =>
            {
                root = await LoadRootAsync(context);

                context.Entry(newRoot).State = useExistingRoot ? EntityState.Unchanged : EntityState.Added;

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.RequiredNonPkSingle).Load();
                }

                old1 = root.RequiredNonPkSingle;

                if (!DoesLazyLoading)
                {
                    context.Entry(old1).Reference(e => e.Single).Load();
                }

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
            }, async context =>
            {
                var loadedRoot = await LoadRootAsync(context);

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
    public virtual Task Optional_one_to_one_are_orphaned(
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
                    context.Entry(root).Reference(e => e.OptionalSingle).Load();
                }

                var removed = root.OptionalSingle;

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Reference(e => e.Single).Load();
                }

                removedId = removed.Id;
                var orphaned = removed.Single;
                orphanedId = orphaned.Id;

                context.Remove(removed);

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);

                Assert.Null(root.OptionalSingle);

                Assert.Empty(context.Set<OptionalSingle1>().Where(e => e.Id == removedId));
                Assert.Equal(1, context.Set<OptionalSingle2>().Count(e => e.Id == orphanedId));

                Assert.Same(root, removed.Root);
                Assert.Same(orphaned, removed.Single);
            }, async context =>
            {
                var root = await LoadRootAsync(context);

                Assert.Null(root.OptionalSingle);

                Assert.Empty(context.Set<OptionalSingle1>().Where(e => e.Id == removedId));
                Assert.Equal(1, context.Set<OptionalSingle2>().Count(e => e.Id == orphanedId));
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
    public virtual Task Required_one_to_one_are_cascade_deleted(
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
                    context.Entry(root).Reference(e => e.RequiredSingle).Load();
                }

                var removed = root.RequiredSingle;

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

                    Assert.Null(root.RequiredSingle);

                    Assert.Empty(context.Set<RequiredSingle1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<RequiredSingle2>().Where(e => e.Id == orphanedId));

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
                        context.Entry(root).Reference(e => e.RequiredSingle).Load();
                    }

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
    public virtual Task Required_non_PK_one_to_one_are_cascade_deleted(
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
                    context.Entry(root).Reference(e => e.RequiredNonPkSingle).Load();
                }

                var removed = root.RequiredNonPkSingle;

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

                    Assert.Null(root.RequiredNonPkSingle);

                    Assert.Empty(context.Set<RequiredNonPkSingle1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<RequiredNonPkSingle2>().Where(e => e.Id == orphanedId));

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
                        context.Entry(root).Reference(e => e.RequiredNonPkSingle).Load();
                    }

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
    public virtual Task Required_one_to_one_are_cascade_deleted_in_store(
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
                    context.Entry(root).Reference(e => e.RequiredSingle).Load();
                }

                var removed = root.RequiredSingle;

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

                var root = context.Set<Root>().Include(e => e.RequiredSingle).Single(IsTheRoot);

                var removed = root.RequiredSingle;

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

                    if (!DoesLazyLoading)
                    {
                        context.Entry(root).Reference(e => e.RequiredSingle).Load();
                    }

                    Assert.Null(root.RequiredSingle);

                    Assert.Empty(context.Set<RequiredSingle1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<RequiredSingle2>().Where(e => e.Id == orphanedId));

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
                        context.Entry(root).Reference(e => e.RequiredSingle).Load();
                    }

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
    public virtual Task Required_non_PK_one_to_one_are_cascade_deleted_in_store(
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
                    context.Entry(root).Reference(e => e.RequiredNonPkSingle).Load();
                }

                var removed = root.RequiredNonPkSingle;

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

                var root = context.Set<Root>().Include(e => e.RequiredNonPkSingle).Single(IsTheRoot);

                var removed = root.RequiredNonPkSingle;

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

                    if (!DoesLazyLoading)
                    {
                        context.Entry(root).Reference(e => e.RequiredNonPkSingle).Load();
                    }

                    Assert.Null(root.RequiredNonPkSingle);

                    Assert.Empty(context.Set<RequiredNonPkSingle1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<RequiredNonPkSingle2>().Where(e => e.Id == orphanedId));

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
                        context.Entry(root).Reference(e => e.RequiredNonPkSingle).Load();
                    }

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
    public virtual Task Optional_one_to_one_are_orphaned_in_store(
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
                    context.Entry(root).Reference(e => e.OptionalSingle).Load();
                }

                var removed = root.OptionalSingle;

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

                var root = context.Set<Root>().Include(e => e.OptionalSingle).Single(IsTheRoot);

                var removed = root.OptionalSingle;

                if (!DoesLazyLoading)
                {
                    context.Entry(removed).Reference(e => e.Single).Load();
                }

                var orphaned = removed.Single;

                context.Remove(removed);

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Null(root.OptionalSingle);

                Assert.Empty(context.Set<OptionalSingle1>().Where(e => e.Id == removedId));
                Assert.Null(context.Set<OptionalSingle2>().Single(e => e.Id == orphanedId).BackId);

                Assert.Same(root, removed.Root);
                Assert.Same(orphaned, removed.Single);
                return Task.CompletedTask;
            }, async context =>
            {
                var root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.OptionalSingle).Load();
                }

                Assert.Null(root.OptionalSingle);

                Assert.Empty(context.Set<OptionalSingle1>().Where(e => e.Id == removedId));
                Assert.Null(context.Set<OptionalSingle2>().Single(e => e.Id == orphanedId).BackId);
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
    public virtual Task Optional_one_to_one_are_orphaned_starting_detached(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;
        Root root = null;
        OptionalSingle1 removed = null;
        OptionalSingle2 orphaned = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.OptionalSingle).Load();
                }

                removed = root.OptionalSingle;

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
                    ? EntityState.Modified
                    : EntityState.Unchanged;

                Assert.Equal(expectedState, context.Entry(orphaned).State);

                Assert.True(context.ChangeTracker.HasChanges());

                context.SaveChanges();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);

                Assert.Same(root, removed.Root);
                Assert.Same(orphaned, removed.Single);
                return Task.CompletedTask;
            }, async context =>
            {
                root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.OptionalSingle).Load();
                }

                Assert.Null(root.OptionalSingle);

                Assert.Empty(context.Set<OptionalSingle1>().Where(e => e.Id == removedId));
                Assert.Equal(1, context.Set<OptionalSingle2>().Count(e => e.Id == orphanedId));
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
    public virtual Task Required_one_to_one_are_cascade_deleted_starting_detached(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;
        Root root = null;
        RequiredSingle1 removed = null;
        RequiredSingle2 orphaned = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.RequiredSingle).Load();
                }

                removed = root.RequiredSingle;

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
                        context.Entry(root).Reference(e => e.RequiredSingle).Load();
                    }

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
    public virtual Task Required_non_PK_one_to_one_are_cascade_deleted_starting_detached(
        CascadeTiming cascadeDeleteTiming,
        CascadeTiming deleteOrphansTiming)
    {
        var removedId = 0;
        var orphanedId = 0;
        Root root = null;
        RequiredNonPkSingle1 removed = null;
        RequiredNonPkSingle2 orphaned = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                root = await LoadRootAsync(context);

                if (!DoesLazyLoading)
                {
                    context.Entry(root).Reference(e => e.RequiredNonPkSingle).Load();
                }

                removed = root.RequiredNonPkSingle;

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
                        context.Entry(root).Reference(e => e.RequiredNonPkSingle).Load();
                    }

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
    public virtual Task Required_one_to_one_are_cascade_detached_when_Added(
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
                    context.Entry(root).Reference(e => e.RequiredSingle).Load();
                }

                var removed = root.RequiredSingle;

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
                        context.Entry(root).Reference(e => e.RequiredSingle).Load();
                    }

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
    public virtual Task Required_non_PK_one_to_one_are_cascade_detached_when_Added(
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
                    context.Entry(root).Reference(e => e.RequiredNonPkSingle).Load();
                }

                var removed = root.RequiredNonPkSingle;

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
                        context.Entry(root).Reference(e => e.RequiredNonPkSingle).Load();
                    }

                    Assert.Null(root.RequiredNonPkSingle);

                    Assert.Empty(context.Set<RequiredNonPkSingle1>().Where(e => e.Id == removedId));
                    Assert.Empty(context.Set<RequiredNonPkSingle2>().Where(e => e.Id == orphanedId));
                }
            });
    }
}
