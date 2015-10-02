// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.FunctionalTests.TestUtilities.Xunit;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class GraphUpdatesTestBase<TTestStore, TFixture> : IClassFixture<TFixture>, IDisposable
        where TTestStore : TestStore
        where TFixture : GraphUpdatesTestBase<TTestStore, TFixture>.GraphUpdatesFixtureBase, new()
    {
        protected GraphUpdatesTestBase(TFixture fixture)
        {
            Fixture = fixture;
            TestStore = Fixture.CreateTestStore();
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Principal, false)]
        [InlineData((int)ChangeMechanism.Principal, true)]
        [InlineData((int)ChangeMechanism.Dependent, false)]
        [InlineData((int)ChangeMechanism.Dependent, true)]
        [InlineData((int)ChangeMechanism.FK, false)]
        [InlineData((int)ChangeMechanism.FK, true)]
        public virtual void Save_optional_many_to_one_dependents(ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            var new2a = new Optional2 { Id = Fixture.IntSentinel };
            var new2b = new Optional2 { Id = Fixture.IntSentinel };
            var new1 = new Optional1 { Id = Fixture.IntSentinel };

            if (useExistingEntities)
            {
                using (var context = CreateContext())
                {
                    context.AddRange(new1, new2a, new2b);
                    context.SaveChanges();
                }
            }

            Root root;
            int entityCount;
            using (var context = CreateContext())
            {
                root = LoadFullGraph(context);
                var existing = root.OptionalChildren.OrderBy(e => e.Id).First();

                if (useExistingEntities)
                {
                    new1 = context.Optional1s.Single(e => e.Id == new1.Id);
                    new2a = context.Optional2s.Single(e => e.Id == new2a.Id);
                    new2b = context.Optional2s.Single(e => e.Id == new2b.Id);
                }
                else
                {
                    context.AddRange(new1, new2a, new2b);
                }

                switch (changeMechanism)
                {
                    case ChangeMechanism.Principal:
                        existing.Children.Add(new2a);
                        existing.Children.Add(new2b);
                        root.OptionalChildren.Add(new1);
                        break;
                    case ChangeMechanism.Dependent:
                        new2a.Parent = existing;
                        new2b.Parent = existing;
                        new1.Parent = root;
                        break;
                    case ChangeMechanism.FK:
                        new2a.ParentId = existing.Id;
                        new2b.ParentId = existing.Id;
                        new1.ParentId = root.Id;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.Contains(new2a, existing.Children);
                Assert.Contains(new2b, existing.Children);
                Assert.Contains(new1, root.OptionalChildren);

                Assert.Same(existing, new2a.Parent);
                Assert.Same(existing, new2b.Parent);
                Assert.Same(root, existing.Parent);

                Assert.Equal(existing.Id, new2a.ParentId);
                Assert.Equal(existing.Id, new2b.ParentId);
                Assert.Equal(root.Id, existing.ParentId);

                entityCount = context.ChangeTracker.Entries().Count();
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context);

                Assert.Equal(entityCount, context.ChangeTracker.Entries().Count());
                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Principal, false)]
        [InlineData((int)ChangeMechanism.Principal, true)]
        [InlineData((int)ChangeMechanism.Dependent, false)]
        [InlineData((int)ChangeMechanism.Dependent, true)]
        [InlineData((int)ChangeMechanism.FK, false)]
        [InlineData((int)ChangeMechanism.FK, true)]
        public virtual void Save_required_many_to_one_dependents(ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            var newRoot = new Root { Id = Fixture.IntSentinel };
            var new1 = new Required1 { Id = Fixture.IntSentinel, ParentId = Fixture.IntSentinel, Parent = newRoot };
            var new2a = new Required2 { Id = Fixture.IntSentinel, ParentId = Fixture.IntSentinel, Parent = new1 };
            var new2b = new Required2 { Id = Fixture.IntSentinel, ParentId = Fixture.IntSentinel, Parent = new1 };

            if (useExistingEntities)
            {
                using (var context = CreateContext())
                {
                    context.AddRange(newRoot, new1, new2a, new2b);
                    context.SaveChanges();
                }
            }

            Root root;
            int entityCount;
            using (var context = CreateContext())
            {
                root = LoadFullGraph(context, e => e.Id != newRoot.Id);
                var existing = root.RequiredChildren.OrderBy(e => e.Id).First();

                if (useExistingEntities)
                {
                    new1 = context.Required1s.Single(e => e.Id == new1.Id);
                    new2a = context.Required2s.Single(e => e.Id == new2a.Id);
                    new2b = context.Required2s.Single(e => e.Id == new2b.Id);
                }
                else
                {
                    context.AddRange(new1, new2a, new2b);
                }

                switch (changeMechanism)
                {
                    case ChangeMechanism.Principal:
                        existing.Children.Add(new2a);
                        existing.Children.Add(new2b);
                        root.RequiredChildren.Add(new1);
                        break;
                    case ChangeMechanism.Dependent:
                        new2a.Parent = existing;
                        new2b.Parent = existing;
                        new1.Parent = root;
                        break;
                    case ChangeMechanism.FK:
                        new2a.ParentId = existing.Id;
                        new2b.ParentId = existing.Id;
                        new1.ParentId = root.Id;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.Contains(new2a, existing.Children);
                Assert.Contains(new2b, existing.Children);
                Assert.Contains(new1, root.RequiredChildren);

                Assert.Same(existing, new2a.Parent);
                Assert.Same(existing, new2b.Parent);
                Assert.Same(root, existing.Parent);

                Assert.Equal(existing.Id, new2a.ParentId);
                Assert.Equal(existing.Id, new2b.ParentId);
                Assert.Equal(root.Id, existing.ParentId);

                entityCount = context.ChangeTracker.Entries().Count();
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context, e => e.Id != newRoot.Id);

                Assert.Equal(entityCount, context.ChangeTracker.Entries().Count());
                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.FK)]
        public virtual void Save_removed_optional_many_to_one_dependents(ChangeMechanism changeMechanism)
        {
            Root root;
            using (var context = CreateContext())
            {
                root = LoadFullGraph(context);

                var childCollection = root.OptionalChildren.First().Children;
                var removed2 = childCollection.First();
                var removed1 = root.OptionalChildren.Skip(1).First();

                switch (changeMechanism)
                {
                    case ChangeMechanism.Dependent:
                        removed2.Parent = null;
                        removed1.Parent = null;
                        break;
                    case ChangeMechanism.Principal:
                        childCollection.Remove(removed2);
                        root.OptionalChildren.Remove(removed1);
                        break;
                    case ChangeMechanism.FK:
                        removed2.ParentId = null;
                        removed1.ParentId = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.DoesNotContain(removed1, root.OptionalChildren);
                Assert.DoesNotContain(removed2, childCollection);

                Assert.Null(removed1.Parent);
                Assert.Null(removed2.Parent);
                Assert.Null(removed1.ParentId);
                Assert.Null(removed2.ParentId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context);

                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);

                Assert.Equal(2, loadedRoot.RequiredChildren.Count);
                Assert.Equal(2, loadedRoot.RequiredChildren.First().Children.Count);

                Assert.Equal(1, loadedRoot.OptionalChildren.Count);
                Assert.Equal(1, loadedRoot.OptionalChildren.First().Children.Count);
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.FK)]
        public virtual void Save_removed_required_many_to_one_dependents(ChangeMechanism changeMechanism)
        {
            int removed1Id;
            int removed2Id;
            List<int> removed1ChildrenIds;

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                var childCollection = root.RequiredChildren.First().Children;
                var removed2 = childCollection.First();
                var removed1 = root.RequiredChildren.Skip(1).First();

                removed1Id = removed1.Id;
                removed2Id = removed2.Id;
                removed1ChildrenIds = removed1.Children.Select(e => e.Id).ToList();

                switch (changeMechanism)
                {
                    case ChangeMechanism.Dependent:
                        removed2.Parent = null;
                        removed1.Parent = null;
                        break;
                    case ChangeMechanism.Principal:
                        childCollection.Remove(removed2);
                        root.RequiredChildren.Remove(removed1);
                        break;
                    case ChangeMechanism.FK:
                        context.Entry(removed2).GetService()[context.Entry(removed2).Property(e => e.ParentId).Metadata] = null;
                        context.Entry(removed1).GetService()[context.Entry(removed1).Property(e => e.ParentId).Metadata] = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                AssertNavigations(root);

                Assert.Equal(1, root.RequiredChildren.Count);
                Assert.DoesNotContain(removed1Id, root.RequiredChildren.Select(e => e.Id));

                Assert.Empty(context.Required1s.Where(e => e.Id == removed1Id));
                Assert.Empty(context.Required2s.Where(e => e.Id == removed2Id));
                Assert.Empty(context.Required2s.Where(e => removed1ChildrenIds.Contains(e.Id)));
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent, false)]
        [InlineData((int)ChangeMechanism.Dependent, true)]
        [InlineData((int)ChangeMechanism.Principal, false)]
        [InlineData((int)ChangeMechanism.Principal, true)]
        [InlineData((int)ChangeMechanism.FK, false)]
        [InlineData((int)ChangeMechanism.FK, true)]
        public virtual void Save_changed_optional_one_to_one(ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            var new2 = new OptionalSingle2 { Id = Fixture.IntSentinel };
            var new1 = new OptionalSingle1 { Id = Fixture.IntSentinel, Single = new2 };

            if (useExistingEntities)
            {
                using (var context = CreateContext())
                {
                    context.AddRange(new1, new2);
                    context.SaveChanges();
                }
            }

            Root root;
            OptionalSingle1 old1;
            OptionalSingle2 old2;
            using (var context = CreateContext())
            {
                root = LoadFullGraph(context);

                old1 = root.OptionalSingle;
                old2 = root.OptionalSingle.Single;

                if (useExistingEntities)
                {
                    new1 = context.OptionalSingle1s.Single(e => e.Id == new1.Id);
                    new2 = context.OptionalSingle2s.Single(e => e.Id == new2.Id);
                }
                else
                {
                    context.AddRange(new1, new2);
                }

                switch (changeMechanism)
                {
                    case ChangeMechanism.Dependent:
                        new1.Root = root;
                        break;
                    case ChangeMechanism.Principal:
                        root.OptionalSingle = new1;
                        break;
                    case ChangeMechanism.FK:
                        new1.RootId = root.Id;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.Equal(root.Id, new1.RootId);
                Assert.Equal(new1.Id, new2.BackId);
                Assert.Same(root, new1.Root);
                Assert.Same(new1, new2.Back);

                Assert.Null(old1.Root);
                Assert.Same(old1, old2.Back);
                Assert.Null(old1.RootId);
                Assert.Equal(old1.Id, old2.BackId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context);

                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);

                var loaded1 = context.OptionalSingle1s.Single(e => e.Id == old1.Id);
                var loaded2 = context.OptionalSingle2s.Single(e => e.Id == old2.Id);

                Assert.Null(loaded1.Root);
                Assert.Same(loaded1, loaded2.Back);
                Assert.Null(loaded1.RootId);
                Assert.Equal(loaded1.Id, loaded2.BackId);
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)ChangeMechanism.FK)]
        public virtual void Save_required_one_to_one_changed_by_reference(ChangeMechanism changeMechanism)
        {
            // This test is a bit strange because the relationships are PK<->PK, which means
            // that an existing entity has to be deleted and then a new entity created that has
            // the same key as the existing entry. In other words it is a new incarnation of the same
            // entity. EF7 can't track two different instances of the same entity, so this has to be
            // done in two steps.

            Root oldRoot;
            RequiredSingle1 old1;
            RequiredSingle2 old2;
            using (var context = CreateContext())
            {
                oldRoot = LoadFullGraph(context);

                old1 = oldRoot.RequiredSingle;
                old2 = oldRoot.RequiredSingle.Single;
            }

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                root.RequiredSingle = null;

                context.SaveChanges();
            }

            var new2 = new RequiredSingle2 { Id = Fixture.IntSentinel };
            var new1 = new RequiredSingle1 { Id = Fixture.IntSentinel, Single = new2 };

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                switch (changeMechanism)
                {
                    case ChangeMechanism.Dependent:
                        context.Add(new1);
                        new1.Root = root;
                        break;
                    case ChangeMechanism.Principal:
                        root.RequiredSingle = new1;
                        break;
                    case ChangeMechanism.FK:
                        context.Add(new1);
                        new1.Id = root.Id;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.Equal(root.Id, new1.Id);
                Assert.Equal(new1.Id, new2.Id);
                Assert.Same(root, new1.Root);
                Assert.Same(new1, new2.Back);

                Assert.Same(oldRoot, old1.Root);
                Assert.Same(old1, old2.Back);
                Assert.Equal(oldRoot.Id, old1.Id);
                Assert.Equal(old1.Id, old2.Id);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context);

                AssertKeys(oldRoot, loadedRoot);
                AssertNavigations(loadedRoot);
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent, false)]
        [InlineData((int)ChangeMechanism.Dependent, true)]
        [InlineData((int)ChangeMechanism.Principal, false)]
        [InlineData((int)ChangeMechanism.Principal, true)]
        // TODO: Not working yet
        //[InlineData((int)ChangeMechanism.FK, false)]
        //[InlineData((int)ChangeMechanism.FK, true)]
        public virtual void Save_required_non_PK_one_to_one_changed_by_reference(ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            var new2 = new RequiredNonPkSingle2 { Id = Fixture.IntSentinel, BackId = Fixture.IntSentinel };
            var new1 = new RequiredNonPkSingle1 { Id = Fixture.IntSentinel, RootId = Fixture.IntSentinel, Single = new2 };
            var newRoot = new Root { Id = Fixture.IntSentinel, RequiredNonPkSingle = new1 };

            if (useExistingEntities)
            {
                using (var context = CreateContext())
                {
                    context.AddRange(newRoot, new1, new2);
                    context.SaveChanges();
                }
            }

            Root root;
            RequiredNonPkSingle1 old1;
            RequiredNonPkSingle2 old2;
            using (var context = CreateContext())
            {
                root = LoadFullGraph(context, e => e.Id != newRoot.Id);

                old1 = root.RequiredNonPkSingle;
                old2 = root.RequiredNonPkSingle.Single;

                if (useExistingEntities)
                {
                    new1 = context.RequiredNonPkSingle1s.Single(e => e.Id == new1.Id);
                    new2 = context.RequiredNonPkSingle2s.Single(e => e.Id == new2.Id);
                }
                else
                {
                    context.AddRange(newRoot, new1, new2);
                }

                switch (changeMechanism)
                {
                    case ChangeMechanism.Dependent:
                        new1.Root = root;
                        break;
                    case ChangeMechanism.Principal:
                        root.RequiredNonPkSingle = new1;
                        break;
                    case ChangeMechanism.FK:
                        new1.RootId = root.Id;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.Equal(root.Id, new1.RootId);
                Assert.Equal(new1.Id, new2.BackId);
                Assert.Same(root, new1.Root);
                Assert.Same(new1, new2.Back);

                Assert.Null(old1.Root);
                Assert.Null(old2.Back);
                Assert.Equal(old1.Id, old2.BackId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context, e => e.Id == root.Id);

                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);

                Assert.False(context.RequiredNonPkSingle1s.Any(e => e.Id == old1.Id));
                Assert.False(context.RequiredNonPkSingle2s.Any(e => e.Id == old2.Id));
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)ChangeMechanism.FK)]
        public virtual void Sever_optional_one_to_one(ChangeMechanism changeMechanism)
        {
            Root root;
            OptionalSingle1 old1;
            OptionalSingle2 old2;
            using (var context = CreateContext())
            {
                root = LoadFullGraph(context);

                old1 = root.OptionalSingle;
                old2 = root.OptionalSingle.Single;

                switch (changeMechanism)
                {
                    case ChangeMechanism.Dependent:
                        old1.Root = null;
                        break;
                    case ChangeMechanism.Principal:
                        root.OptionalSingle = null;
                        break;
                    case ChangeMechanism.FK:
                        old1.RootId = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.Null(old1.Root);
                Assert.Same(old1, old2.Back);
                Assert.Null(old1.RootId);
                Assert.Equal(old1.Id, old2.BackId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context);

                AssertKeys(root, loadedRoot);
                AssertPossiblyNullNavigations(loadedRoot);

                var loaded1 = context.OptionalSingle1s.Single(e => e.Id == old1.Id);
                var loaded2 = context.OptionalSingle2s.Single(e => e.Id == old2.Id);

                Assert.Null(loaded1.Root);
                Assert.Same(loaded1, loaded2.Back);
                Assert.Null(loaded1.RootId);
                Assert.Equal(loaded1.Id, loaded2.BackId);
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.Principal)]
        public virtual void Sever_required_one_to_one(ChangeMechanism changeMechanism)
        {
            Root root;
            RequiredSingle1 old1;
            RequiredSingle2 old2;
            using (var context = CreateContext())
            {
                root = LoadFullGraph(context);

                old1 = root.RequiredSingle;
                old2 = root.RequiredSingle.Single;

                switch (changeMechanism)
                {
                    case ChangeMechanism.Dependent:
                        old1.Root = null;
                        break;
                    case ChangeMechanism.Principal:
                        root.RequiredSingle = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.Null(old1.Root);
                Assert.Null(old2.Back);
                Assert.Equal(old1.Id, old2.Id);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context);

                AssertKeys(root, loadedRoot);
                AssertPossiblyNullNavigations(loadedRoot);

                Assert.False(context.RequiredSingle1s.Any(e => e.Id == old1.Id));
                Assert.False(context.RequiredSingle2s.Any(e => e.Id == old2.Id));
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.Principal)]
        public virtual void Sever_required_non_PK_one_to_one(ChangeMechanism changeMechanism)
        {
            Root root;
            RequiredNonPkSingle1 old1;
            RequiredNonPkSingle2 old2;
            using (var context = CreateContext())
            {
                root = LoadFullGraph(context);

                old1 = root.RequiredNonPkSingle;
                old2 = root.RequiredNonPkSingle.Single;

                switch (changeMechanism)
                {
                    case ChangeMechanism.Dependent:
                        old1.Root = null;
                        break;
                    case ChangeMechanism.Principal:
                        root.RequiredNonPkSingle = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.Null(old1.Root);
                Assert.Null(old2.Back);
                Assert.Equal(old1.Id, old2.BackId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context);

                AssertKeys(root, loadedRoot);
                AssertPossiblyNullNavigations(loadedRoot);

                Assert.False(context.RequiredNonPkSingle1s.Any(e => e.Id == old1.Id));
                Assert.False(context.RequiredNonPkSingle2s.Any(e => e.Id == old2.Id));
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent, false)]
        [InlineData((int)ChangeMechanism.Dependent, true)]
        [InlineData((int)ChangeMechanism.Principal, false)]
        [InlineData((int)ChangeMechanism.Principal, true)]
        [InlineData((int)ChangeMechanism.FK, false)]
        [InlineData((int)ChangeMechanism.FK, true)]
        public virtual void Reparent_optional_one_to_one(ChangeMechanism changeMechanism, bool useExistingRoot)
        {
            var newRoot = new Root { Id = Fixture.IntSentinel };

            if (useExistingRoot)
            {
                using (var context = CreateContext())
                {
                    context.AddRange(newRoot);
                    context.SaveChanges();
                }
            }

            Root root;
            OptionalSingle1 old1;
            OptionalSingle2 old2;
            using (var context = CreateContext())
            {
                root = LoadFullGraph(context, e => e.Id != newRoot.Id);

                context.Entry(newRoot).State = useExistingRoot ? EntityState.Unchanged : EntityState.Added;

                old1 = root.OptionalSingle;
                old2 = root.OptionalSingle.Single;

                switch (changeMechanism)
                {
                    case ChangeMechanism.Dependent:
                        old1.Root = newRoot;
                        break;
                    case ChangeMechanism.Principal:
                        newRoot.OptionalSingle = old1;
                        break;
                    case ChangeMechanism.FK:
                        old1.RootId = newRoot.Id;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.Null(root.OptionalSingle);

                Assert.Same(newRoot, old1.Root);
                Assert.Same(old1, old2.Back);
                Assert.Equal(newRoot.Id, old1.RootId);
                Assert.Equal(old1.Id, old2.BackId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context, e => e.Id == root.Id);

                AssertKeys(root, loadedRoot);
                AssertPossiblyNullNavigations(loadedRoot);

                newRoot = context.Roots.Single(e => e.Id == newRoot.Id);
                var loaded1 = context.OptionalSingle1s.Single(e => e.Id == old1.Id);
                var loaded2 = context.OptionalSingle2s.Single(e => e.Id == old2.Id);

                Assert.Same(newRoot, loaded1.Root);
                Assert.Same(loaded1, loaded2.Back);
                Assert.Equal(newRoot.Id, loaded1.RootId);
                Assert.Equal(loaded1.Id, loaded2.BackId);
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent, false)]
        [InlineData((int)ChangeMechanism.Dependent, true)]
        [InlineData((int)ChangeMechanism.Principal, false)]
        [InlineData((int)ChangeMechanism.Principal, true)]
        [InlineData((int)ChangeMechanism.FK, false)]
        [InlineData((int)ChangeMechanism.FK, true)]
        public virtual void Reparent_required_one_to_one(ChangeMechanism changeMechanism, bool useExistingRoot)
        {
            var newRoot = new Root { Id = Fixture.IntSentinel };

            if (useExistingRoot)
            {
                using (var context = CreateContext())
                {
                    context.AddRange(newRoot);
                    context.SaveChanges();
                }
            }

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context, e => e.Id != newRoot.Id);

                context.Entry(newRoot).State = useExistingRoot ? EntityState.Unchanged : EntityState.Added;

                switch (changeMechanism)
                {
                    case ChangeMechanism.Dependent:
                        root.RequiredSingle.Root = newRoot;
                        break;
                    case ChangeMechanism.Principal:
                        newRoot.RequiredSingle = root.RequiredSingle;
                        break;
                    case ChangeMechanism.FK:
                        root.RequiredSingle.Id = newRoot.Id;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                newRoot.RequiredSingle = root.RequiredSingle;

                Assert.Equal(
                    CoreStrings.KeyReadOnly("Id", typeof(RequiredSingle1).Name),
                    Assert.Throws<NotSupportedException>(() => context.SaveChanges()).Message);
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent, false)]
        [InlineData((int)ChangeMechanism.Dependent, true)]
        [InlineData((int)ChangeMechanism.Principal, false)]
        [InlineData((int)ChangeMechanism.Principal, true)]
        [InlineData((int)ChangeMechanism.FK, false)]
        [InlineData((int)ChangeMechanism.FK, true)]
        public virtual void Reparent_required_non_PK_one_to_one(ChangeMechanism changeMechanism, bool useExistingRoot)
        {
            var newRoot = new Root { Id = Fixture.IntSentinel };

            if (useExistingRoot)
            {
                using (var context = CreateContext())
                {
                    context.AddRange(newRoot);
                    context.SaveChanges();
                }
            }

            Root root;
            RequiredNonPkSingle1 old1;
            RequiredNonPkSingle2 old2;
            using (var context = CreateContext())
            {
                root = LoadFullGraph(context, e => e.Id != newRoot.Id);

                context.Entry(newRoot).State = useExistingRoot ? EntityState.Unchanged : EntityState.Added;

                old1 = root.RequiredNonPkSingle;
                old2 = root.RequiredNonPkSingle.Single;

                switch (changeMechanism)
                {
                    case ChangeMechanism.Dependent:
                        old1.Root = newRoot;
                        break;
                    case ChangeMechanism.Principal:
                        newRoot.RequiredNonPkSingle = old1;
                        break;
                    case ChangeMechanism.FK:
                        old1.RootId = newRoot.Id;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.Null(root.RequiredNonPkSingle);

                Assert.Same(newRoot, old1.Root);
                Assert.Same(old1, old2.Back);
                Assert.Equal(newRoot.Id, old1.RootId);
                Assert.Equal(old1.Id, old2.BackId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context, e => e.Id == root.Id);

                AssertKeys(root, loadedRoot);
                AssertPossiblyNullNavigations(loadedRoot);

                newRoot = context.Roots.Single(e => e.Id == newRoot.Id);
                var loaded1 = context.RequiredNonPkSingle1s.Single(e => e.Id == old1.Id);
                var loaded2 = context.RequiredNonPkSingle2s.Single(e => e.Id == old2.Id);

                Assert.Same(newRoot, loaded1.Root);
                Assert.Same(loaded1, loaded2.Back);
                Assert.Equal(newRoot.Id, loaded1.RootId);
                Assert.Equal(loaded1.Id, loaded2.BackId);
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Principal, false)]
        [InlineData((int)ChangeMechanism.Principal, true)]
        [InlineData((int)ChangeMechanism.Dependent, false)]
        [InlineData((int)ChangeMechanism.Dependent, true)]
        [InlineData((int)ChangeMechanism.FK, false)]
        [InlineData((int)ChangeMechanism.FK, true)]
        public virtual void Save_optional_many_to_one_dependents_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            var new2a = new OptionalAk2 { Id = Fixture.IntSentinel, AlternateId = Guid.NewGuid() };
            var new2b = new OptionalAk2 { Id = Fixture.IntSentinel, AlternateId = Guid.NewGuid() };
            var new1 = new OptionalAk1 { Id = Fixture.IntSentinel, AlternateId = Guid.NewGuid() };

            if (useExistingEntities)
            {
                using (var context = CreateContext())
                {
                    context.AddRange(new1, new2a, new2b);
                    context.SaveChanges();
                }
            }

            Root root;
            int entityCount;
            using (var context = CreateContext())
            {
                root = LoadFullGraph(context);
                var existing = root.OptionalChildrenAk.OrderBy(e => e.Id).First();

                if (useExistingEntities)
                {
                    new1 = context.OptionalAk1s.Single(e => e.Id == new1.Id);
                    new2a = context.OptionalAk2s.Single(e => e.Id == new2a.Id);
                    new2b = context.OptionalAk2s.Single(e => e.Id == new2b.Id);
                }
                else
                {
                    context.AddRange(new1, new2a, new2b);
                }

                switch (changeMechanism)
                {
                    case ChangeMechanism.Principal:
                        existing.Children.Add(new2a);
                        existing.Children.Add(new2b);
                        root.OptionalChildrenAk.Add(new1);
                        break;
                    case ChangeMechanism.Dependent:
                        new2a.Parent = existing;
                        new2b.Parent = existing;
                        new1.Parent = root;
                        break;
                    case ChangeMechanism.FK:
                        new2a.ParentId = existing.AlternateId;
                        new2b.ParentId = existing.AlternateId;
                        new1.ParentId = root.AlternateId;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.Contains(new2a, existing.Children);
                Assert.Contains(new2b, existing.Children);
                Assert.Contains(new1, root.OptionalChildrenAk);

                Assert.Same(existing, new2a.Parent);
                Assert.Same(existing, new2b.Parent);
                Assert.Same(root, existing.Parent);

                Assert.Equal(existing.AlternateId, new2a.ParentId);
                Assert.Equal(existing.AlternateId, new2b.ParentId);
                Assert.Equal(root.AlternateId, existing.ParentId);

                entityCount = context.ChangeTracker.Entries().Count();
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context);

                Assert.Equal(entityCount, context.ChangeTracker.Entries().Count());
                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Principal, false)]
        [InlineData((int)ChangeMechanism.Principal, true)]
        [InlineData((int)ChangeMechanism.Dependent, false)]
        [InlineData((int)ChangeMechanism.Dependent, true)]
        [InlineData((int)ChangeMechanism.FK, false)]
        [InlineData((int)ChangeMechanism.FK, true)]
        public virtual void Save_required_many_to_one_dependents_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            var newRoot = new Root { Id = Fixture.IntSentinel, AlternateId = Guid.NewGuid() };
            var new1 = new RequiredAk1 { Id = Fixture.IntSentinel, AlternateId = Guid.NewGuid(), Parent = newRoot };
            var new2a = new RequiredAk2 { Id = Fixture.IntSentinel, AlternateId = Guid.NewGuid(), Parent = new1 };
            var new2b = new RequiredAk2 { Id = Fixture.IntSentinel, AlternateId = Guid.NewGuid(), Parent = new1 };

            if (useExistingEntities)
            {
                using (var context = CreateContext())
                {
                    context.AddRange(newRoot, new1, new2a, new2b);
                    context.SaveChanges();
                }
            }

            Root root;
            int entityCount;
            using (var context = CreateContext())
            {
                root = LoadFullGraph(context, e => e.Id != newRoot.Id);
                var existing = root.RequiredChildrenAk.OrderBy(e => e.Id).First();

                if (useExistingEntities)
                {
                    new1 = context.RequiredAk1s.Single(e => e.Id == new1.Id);
                    new2a = context.RequiredAk2s.Single(e => e.Id == new2a.Id);
                    new2b = context.RequiredAk2s.Single(e => e.Id == new2b.Id);
                }
                else
                {
                    context.AddRange(new1, new2a, new2b);
                }

                switch (changeMechanism)
                {
                    case ChangeMechanism.Principal:
                        existing.Children.Add(new2a);
                        existing.Children.Add(new2b);
                        root.RequiredChildrenAk.Add(new1);
                        break;
                    case ChangeMechanism.Dependent:
                        new2a.Parent = existing;
                        new2b.Parent = existing;
                        new1.Parent = root;
                        break;
                    case ChangeMechanism.FK:
                        new2a.ParentId = existing.AlternateId;
                        new2b.ParentId = existing.AlternateId;
                        new1.ParentId = root.AlternateId;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.Contains(new2a, existing.Children);
                Assert.Contains(new2b, existing.Children);
                Assert.Contains(new1, root.RequiredChildrenAk);

                Assert.Same(existing, new2a.Parent);
                Assert.Same(existing, new2b.Parent);
                Assert.Same(root, existing.Parent);

                Assert.Equal(existing.AlternateId, new2a.ParentId);
                Assert.Equal(existing.AlternateId, new2b.ParentId);
                Assert.Equal(root.AlternateId, existing.ParentId);

                entityCount = context.ChangeTracker.Entries().Count();
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context, e => e.Id != newRoot.Id);

                Assert.Equal(entityCount, context.ChangeTracker.Entries().Count());
                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.FK)]
        public virtual void Save_removed_optional_many_to_one_dependents_with_alternate_key(ChangeMechanism changeMechanism)
        {
            Root root;
            using (var context = CreateContext())
            {
                root = LoadFullGraph(context);

                var childCollection = root.OptionalChildrenAk.First().Children;
                var removed2 = childCollection.First();
                var removed1 = root.OptionalChildrenAk.Skip(1).First();

                switch (changeMechanism)
                {
                    case ChangeMechanism.Dependent:
                        removed2.Parent = null;
                        removed1.Parent = null;
                        break;
                    case ChangeMechanism.Principal:
                        childCollection.Remove(removed2);
                        root.OptionalChildrenAk.Remove(removed1);
                        break;
                    case ChangeMechanism.FK:
                        removed2.ParentId = null;
                        removed1.ParentId = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.DoesNotContain(removed1, root.OptionalChildrenAk);
                Assert.DoesNotContain(removed2, childCollection);

                Assert.Null(removed1.Parent);
                Assert.Null(removed2.Parent);
                Assert.Null(removed1.ParentId);
                Assert.Null(removed2.ParentId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context);

                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);

                Assert.Equal(2, loadedRoot.RequiredChildrenAk.Count);
                Assert.Equal(2, loadedRoot.RequiredChildrenAk.First().Children.Count);

                Assert.Equal(1, loadedRoot.OptionalChildrenAk.Count);
                Assert.Equal(1, loadedRoot.OptionalChildrenAk.First().Children.Count);
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)ChangeMechanism.Dependent)]
        public virtual void Save_removed_required_many_to_one_dependents_with_alternate_key(ChangeMechanism changeMechanism)
        {
            Root root;
            RequiredAk2 removed2;
            RequiredAk1 removed1;

            using (var context = CreateContext())
            {
                root = LoadFullGraph(context);

                var childCollection = root.RequiredChildrenAk.First().Children;
                removed2 = childCollection.First();
                removed1 = root.RequiredChildrenAk.Skip(1).First();

                switch (changeMechanism)
                {
                    case ChangeMechanism.Dependent:
                        removed2.Parent = null;
                        removed1.Parent = null;
                        break;
                    case ChangeMechanism.Principal:
                        childCollection.Remove(removed2);
                        root.RequiredChildrenAk.Remove(removed1);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.DoesNotContain(removed1, root.RequiredChildrenAk);
                Assert.DoesNotContain(removed2, childCollection);

                Assert.Null(removed1.Parent);
                Assert.Null(removed2.Parent);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context);

                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);

                Assert.False(context.RequiredAk1s.Any(e => e.Id == removed1.Id));
                Assert.False(context.RequiredAk2s.Any(e => e.Id == removed2.Id));

                Assert.Equal(1, loadedRoot.RequiredChildrenAk.Count);
                Assert.Equal(1, loadedRoot.RequiredChildrenAk.First().Children.Count);

                Assert.Equal(2, loadedRoot.OptionalChildrenAk.Count);
                Assert.Equal(2, loadedRoot.OptionalChildrenAk.First().Children.Count);
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent, false)]
        [InlineData((int)ChangeMechanism.Dependent, true)]
        [InlineData((int)ChangeMechanism.Principal, false)]
        [InlineData((int)ChangeMechanism.Principal, true)]
        [InlineData((int)ChangeMechanism.FK, false)]
        [InlineData((int)ChangeMechanism.FK, true)]
        public virtual void Save_changed_optional_one_to_one_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            var new2 = new OptionalSingleAk2 { Id = Fixture.IntSentinel, AlternateId = Guid.NewGuid() };
            var new1 = new OptionalSingleAk1 { Id = Fixture.IntSentinel, AlternateId = Guid.NewGuid(), Single = new2 };

            if (useExistingEntities)
            {
                using (var context = CreateContext())
                {
                    context.AddRange(new1, new2);
                    context.SaveChanges();
                }
            }

            Root root;
            OptionalSingleAk1 old1;
            OptionalSingleAk2 old2;
            using (var context = CreateContext())
            {
                root = LoadFullGraph(context);

                old1 = root.OptionalSingleAk;
                old2 = root.OptionalSingleAk.Single;

                if (useExistingEntities)
                {
                    new1 = context.OptionalSingleAk1s.Single(e => e.Id == new1.Id);
                    new2 = context.OptionalSingleAk2s.Single(e => e.Id == new2.Id);
                }
                else
                {
                    context.AddRange(new1, new2);
                }

                switch (changeMechanism)
                {
                    case ChangeMechanism.Dependent:
                        new1.Root = root;
                        break;
                    case ChangeMechanism.Principal:
                        root.OptionalSingleAk = new1;
                        break;
                    case ChangeMechanism.FK:
                        new1.RootId = root.AlternateId;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.Equal(root.AlternateId, new1.RootId);
                Assert.Equal(new1.AlternateId, new2.BackId);
                Assert.Same(root, new1.Root);
                Assert.Same(new1, new2.Back);

                Assert.Null(old1.Root);
                Assert.Same(old1, old2.Back);
                Assert.Null(old1.RootId);
                Assert.Equal(old1.AlternateId, old2.BackId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context);

                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);

                var loaded1 = context.OptionalSingleAk1s.Single(e => e.Id == old1.Id);
                var loaded2 = context.OptionalSingleAk2s.Single(e => e.Id == old2.Id);

                Assert.Null(loaded1.Root);
                Assert.Same(loaded1, loaded2.Back);
                Assert.Null(loaded1.RootId);
                Assert.Equal(loaded1.AlternateId, loaded2.BackId);
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent, false)]
        [InlineData((int)ChangeMechanism.Dependent, true)]
        [InlineData((int)ChangeMechanism.Principal, false)]
        [InlineData((int)ChangeMechanism.Principal, true)]
        public virtual void Save_required_one_to_one_changed_by_reference_with_alternate_key(
            ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            var new2 = new RequiredSingleAk2 { Id = Fixture.IntSentinel, AlternateId = Guid.NewGuid() };
            var new1 = new RequiredSingleAk1 { Id = Fixture.IntSentinel, AlternateId = Guid.NewGuid(), Single = new2 };
            var newRoot = new Root { Id = Fixture.IntSentinel, AlternateId = Guid.NewGuid(), RequiredSingleAk = new1 };

            if (useExistingEntities)
            {
                using (var context = CreateContext())
                {
                    context.AddRange(newRoot, new1, new2);
                    context.SaveChanges();
                }
            }

            Root root;
            RequiredSingleAk1 old1;
            RequiredSingleAk2 old2;
            using (var context = CreateContext())
            {
                root = LoadFullGraph(context, e => e.Id != newRoot.Id);

                old1 = root.RequiredSingleAk;
                old2 = root.RequiredSingleAk.Single;

                if (useExistingEntities)
                {
                    new1 = context.RequiredSingleAk1s.Single(e => e.Id == new1.Id);
                    new2 = context.RequiredSingleAk2s.Single(e => e.Id == new2.Id);
                }
                else
                {
                    context.AddRange(newRoot, new1, new2);
                }

                switch (changeMechanism)
                {
                    case ChangeMechanism.Dependent:
                        new1.Root = root;
                        break;
                    case ChangeMechanism.Principal:
                        root.RequiredSingleAk = new1;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.Equal(root.AlternateId, new1.RootId);
                Assert.Equal(new1.AlternateId, new2.BackId);
                Assert.Same(root, new1.Root);
                Assert.Same(new1, new2.Back);

                Assert.Null(old1.Root);
                Assert.Null(old2.Back);
                Assert.Equal(old1.AlternateId, old2.BackId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context, e => e.Id != newRoot.Id);

                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);

                Assert.False(context.RequiredSingleAk1s.Any(e => e.Id == old1.Id));
                Assert.False(context.RequiredSingleAk2s.Any(e => e.Id == old2.Id));
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent, false)]
        [InlineData((int)ChangeMechanism.Dependent, true)]
        [InlineData((int)ChangeMechanism.Principal, false)]
        [InlineData((int)ChangeMechanism.Principal, true)]
        // TODO: Not working yet
        //[InlineData((int)ChangeMechanism.FK, false)]
        //[InlineData((int)ChangeMechanism.FK, true)]
        public virtual void Save_required_non_PK_one_to_one_changed_by_reference_with_alternate_key(
            ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            var new2 = new RequiredNonPkSingleAk2 { Id = Fixture.IntSentinel, AlternateId = Guid.NewGuid() };
            var new1 = new RequiredNonPkSingleAk1 { Id = Fixture.IntSentinel, AlternateId = Guid.NewGuid(), Single = new2 };
            var newRoot = new Root { Id = Fixture.IntSentinel, AlternateId = Guid.NewGuid(), RequiredNonPkSingleAk = new1 };

            if (useExistingEntities)
            {
                using (var context = CreateContext())
                {
                    context.AddRange(newRoot, new1, new2);
                    context.SaveChanges();
                }
            }

            Root root;
            RequiredNonPkSingleAk1 old1;
            RequiredNonPkSingleAk2 old2;
            using (var context = CreateContext())
            {
                root = LoadFullGraph(context, e => e.Id != newRoot.Id);

                old1 = root.RequiredNonPkSingleAk;
                old2 = root.RequiredNonPkSingleAk.Single;

                if (useExistingEntities)
                {
                    new1 = context.RequiredNonPkSingleAk1s.Single(e => e.Id == new1.Id);
                    new2 = context.RequiredNonPkSingleAk2s.Single(e => e.Id == new2.Id);
                }
                else
                {
                    context.AddRange(new1, new2);
                }

                switch (changeMechanism)
                {
                    case ChangeMechanism.Dependent:
                        new1.Root = root;
                        break;
                    case ChangeMechanism.Principal:
                        root.RequiredNonPkSingleAk = new1;
                        break;
                    case ChangeMechanism.FK:
                        new1.RootId = root.AlternateId;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.Equal(root.AlternateId, new1.RootId);
                Assert.Equal(new1.AlternateId, new2.BackId);
                Assert.Same(root, new1.Root);
                Assert.Same(new1, new2.Back);

                Assert.Null(old1.Root);
                Assert.Null(old2.Back);
                Assert.Equal(old1.AlternateId, old2.BackId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context, e => e.Id != newRoot.Id);

                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);

                Assert.False(context.RequiredNonPkSingleAk1s.Any(e => e.Id == old1.Id));
                Assert.False(context.RequiredNonPkSingleAk2s.Any(e => e.Id == old2.Id));
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)ChangeMechanism.FK)]
        public virtual void Sever_optional_one_to_one_with_alternate_key(ChangeMechanism changeMechanism)
        {
            Root root;
            OptionalSingleAk1 old1;
            OptionalSingleAk2 old2;
            using (var context = CreateContext())
            {
                root = LoadFullGraph(context);

                old1 = root.OptionalSingleAk;
                old2 = root.OptionalSingleAk.Single;

                switch (changeMechanism)
                {
                    case ChangeMechanism.Dependent:
                        old1.Root = null;
                        break;
                    case ChangeMechanism.Principal:
                        root.OptionalSingleAk = null;
                        break;
                    case ChangeMechanism.FK:
                        old1.RootId = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.Null(old1.Root);
                Assert.Same(old1, old2.Back);
                Assert.Null(old1.RootId);
                Assert.Equal(old1.AlternateId, old2.BackId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context);

                AssertKeys(root, loadedRoot);
                AssertPossiblyNullNavigations(loadedRoot);

                var loaded1 = context.OptionalSingleAk1s.Single(e => e.Id == old1.Id);
                var loaded2 = context.OptionalSingleAk2s.Single(e => e.Id == old2.Id);

                Assert.Null(loaded1.Root);
                Assert.Same(loaded1, loaded2.Back);
                Assert.Null(loaded1.RootId);
                Assert.Equal(loaded1.AlternateId, loaded2.BackId);
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.Principal)]
        public virtual void Sever_required_one_to_one_with_alternate_key(ChangeMechanism changeMechanism)
        {
            Root root;
            RequiredSingleAk1 old1;
            RequiredSingleAk2 old2;
            using (var context = CreateContext())
            {
                root = LoadFullGraph(context);

                old1 = root.RequiredSingleAk;
                old2 = root.RequiredSingleAk.Single;

                switch (changeMechanism)
                {
                    case ChangeMechanism.Dependent:
                        old1.Root = null;
                        break;
                    case ChangeMechanism.Principal:
                        root.RequiredSingleAk = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.Null(old1.Root);
                Assert.Null(old2.Back);
                Assert.Equal(old1.AlternateId, old2.BackId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context);

                AssertKeys(root, loadedRoot);
                AssertPossiblyNullNavigations(loadedRoot);

                Assert.False(context.RequiredSingleAk1s.Any(e => e.Id == old1.Id));
                Assert.False(context.RequiredSingleAk2s.Any(e => e.Id == old2.Id));
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.Principal)]
        public virtual void Sever_required_non_PK_one_to_one_with_alternate_key(ChangeMechanism changeMechanism)
        {
            Root root;
            RequiredNonPkSingleAk1 old1;
            RequiredNonPkSingleAk2 old2;
            using (var context = CreateContext())
            {
                root = LoadFullGraph(context);

                old1 = root.RequiredNonPkSingleAk;
                old2 = root.RequiredNonPkSingleAk.Single;

                switch (changeMechanism)
                {
                    case ChangeMechanism.Dependent:
                        old1.Root = null;
                        break;
                    case ChangeMechanism.Principal:
                        root.RequiredNonPkSingleAk = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.Null(old1.Root);
                Assert.Null(old2.Back);
                Assert.Equal(old1.AlternateId, old2.BackId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context);

                AssertKeys(root, loadedRoot);
                AssertPossiblyNullNavigations(loadedRoot);

                Assert.False(context.RequiredNonPkSingleAk1s.Any(e => e.Id == old1.Id));
                Assert.False(context.RequiredNonPkSingleAk2s.Any(e => e.Id == old2.Id));
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent, false)]
        [InlineData((int)ChangeMechanism.Dependent, true)]
        [InlineData((int)ChangeMechanism.Principal, false)]
        [InlineData((int)ChangeMechanism.Principal, true)]
        [InlineData((int)ChangeMechanism.FK, false)]
        [InlineData((int)ChangeMechanism.FK, true)]
        public virtual void Reparent_optional_one_to_one_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingRoot)
        {
            var newRoot = new Root { Id = Fixture.IntSentinel, AlternateId = Guid.NewGuid() };

            if (useExistingRoot)
            {
                using (var context = CreateContext())
                {
                    context.Add(newRoot);
                    context.SaveChanges();
                }
            }

            Root root;
            OptionalSingleAk1 old1;
            OptionalSingleAk2 old2;
            using (var context = CreateContext())
            {
                root = LoadFullGraph(context, e => e.Id != newRoot.Id);

                context.Entry(newRoot).State = useExistingRoot ? EntityState.Unchanged : EntityState.Added;

                old1 = root.OptionalSingleAk;
                old2 = root.OptionalSingleAk.Single;

                switch (changeMechanism)
                {
                    case ChangeMechanism.Dependent:
                        old1.Root = newRoot;
                        break;
                    case ChangeMechanism.Principal:
                        newRoot.OptionalSingleAk = old1;
                        break;
                    case ChangeMechanism.FK:
                        old1.RootId = newRoot.AlternateId;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.Null(root.OptionalSingleAk);

                Assert.Same(newRoot, old1.Root);
                Assert.Same(old1, old2.Back);
                Assert.Equal(newRoot.AlternateId, old1.RootId);
                Assert.Equal(old1.AlternateId, old2.BackId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context, e => e.Id == root.Id);

                AssertKeys(root, loadedRoot);
                AssertPossiblyNullNavigations(loadedRoot);

                newRoot = context.Roots.Single(e => e.Id == newRoot.Id);
                var loaded1 = context.OptionalSingleAk1s.Single(e => e.Id == old1.Id);
                var loaded2 = context.OptionalSingleAk2s.Single(e => e.Id == old2.Id);

                Assert.Same(newRoot, loaded1.Root);
                Assert.Same(loaded1, loaded2.Back);
                Assert.Equal(newRoot.AlternateId, loaded1.RootId);
                Assert.Equal(loaded1.AlternateId, loaded2.BackId);
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent, false)]
        [InlineData((int)ChangeMechanism.Dependent, true)]
        [InlineData((int)ChangeMechanism.Principal, false)]
        [InlineData((int)ChangeMechanism.Principal, true)]
        [InlineData((int)ChangeMechanism.FK, false)]
        [InlineData((int)ChangeMechanism.FK, true)]
        public virtual void Reparent_required_one_to_one_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingRoot)
        {
            var newRoot = new Root { Id = Fixture.IntSentinel, AlternateId = Guid.NewGuid() };

            if (useExistingRoot)
            {
                using (var context = CreateContext())
                {
                    context.Add(newRoot);
                    context.SaveChanges();
                }
            }

            Root root;
            RequiredSingleAk1 old1;
            RequiredSingleAk2 old2;
            using (var context = CreateContext())
            {
                root = LoadFullGraph(context, e => e.Id != newRoot.Id);

                context.Entry(newRoot).State = useExistingRoot ? EntityState.Unchanged : EntityState.Added;

                old1 = root.RequiredSingleAk;
                old2 = root.RequiredSingleAk.Single;

                switch (changeMechanism)
                {
                    case ChangeMechanism.Dependent:
                        old1.Root = newRoot;
                        break;
                    case ChangeMechanism.Principal:
                        newRoot.RequiredSingleAk = old1;
                        break;
                    case ChangeMechanism.FK:
                        old1.RootId = newRoot.AlternateId;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.Null(root.RequiredSingleAk);

                Assert.Same(newRoot, old1.Root);
                Assert.Same(old1, old2.Back);
                Assert.Equal(newRoot.AlternateId, old1.RootId);
                Assert.Equal(old1.AlternateId, old2.BackId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context, e => e.Id == root.Id);

                AssertKeys(root, loadedRoot);
                AssertPossiblyNullNavigations(loadedRoot);

                newRoot = context.Roots.Single(e => e.Id == newRoot.Id);
                var loaded1 = context.RequiredSingleAk1s.Single(e => e.Id == old1.Id);
                var loaded2 = context.RequiredSingleAk2s.Single(e => e.Id == old2.Id);

                Assert.Same(newRoot, loaded1.Root);
                Assert.Same(loaded1, loaded2.Back);
                Assert.Equal(newRoot.AlternateId, loaded1.RootId);
                Assert.Equal(loaded1.AlternateId, loaded2.BackId);
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent, false)]
        [InlineData((int)ChangeMechanism.Dependent, true)]
        [InlineData((int)ChangeMechanism.Principal, false)]
        [InlineData((int)ChangeMechanism.Principal, true)]
        [InlineData((int)ChangeMechanism.FK, false)]
        [InlineData((int)ChangeMechanism.FK, true)]
        public virtual void Reparent_required_non_PK_one_to_one_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingRoot)
        {
            var newRoot = new Root { Id = Fixture.IntSentinel, AlternateId = Guid.NewGuid() };

            if (useExistingRoot)
            {
                using (var context = CreateContext())
                {
                    context.Add(newRoot);
                    context.SaveChanges();
                }
            }

            Root root;
            RequiredNonPkSingleAk1 old1;
            RequiredNonPkSingleAk2 old2;
            using (var context = CreateContext())
            {
                root = LoadFullGraph(context, e => e.Id != newRoot.Id);

                context.Entry(newRoot).State = useExistingRoot ? EntityState.Unchanged : EntityState.Added;

                old1 = root.RequiredNonPkSingleAk;
                old2 = root.RequiredNonPkSingleAk.Single;

                switch (changeMechanism)
                {
                    case ChangeMechanism.Dependent:
                        old1.Root = newRoot;
                        break;
                    case ChangeMechanism.Principal:
                        newRoot.RequiredNonPkSingleAk = old1;
                        break;
                    case ChangeMechanism.FK:
                        old1.RootId = newRoot.AlternateId;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(changeMechanism));
                }

                context.SaveChanges();

                Assert.Null(root.RequiredNonPkSingleAk);

                Assert.Same(newRoot, old1.Root);
                Assert.Same(old1, old2.Back);
                Assert.Equal(newRoot.AlternateId, old1.RootId);
                Assert.Equal(old1.AlternateId, old2.BackId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadFullGraph(context, e => e.Id == root.Id);

                AssertKeys(root, loadedRoot);
                AssertPossiblyNullNavigations(loadedRoot);

                newRoot = context.Roots.Single(e => e.Id == newRoot.Id);
                var loaded1 = context.RequiredNonPkSingleAk1s.Single(e => e.Id == old1.Id);
                var loaded2 = context.RequiredNonPkSingleAk2s.Single(e => e.Id == old2.Id);

                Assert.Same(newRoot, loaded1.Root);
                Assert.Same(loaded1, loaded2.Back);
                Assert.Equal(newRoot.AlternateId, loaded1.RootId);
                Assert.Equal(loaded1.AlternateId, loaded2.BackId);
            }
        }

        [ConditionalFact]
        public virtual void Required_many_to_one_dependents_are_cascade_deleted()
        {
            int removedId;
            List<int> orphanedIds;

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Equal(2, root.RequiredChildren.Count);

                var removed = root.RequiredChildren.First();

                removedId = removed.Id;
                var cascadeRemoved = removed.Children.ToList();
                orphanedIds = cascadeRemoved.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Detached));

                Assert.Equal(1, root.RequiredChildren.Count);
                Assert.DoesNotContain(removedId, root.RequiredChildren.Select(e => e.Id));

                Assert.Empty(context.Required1s.Where(e => e.Id == removedId));
                Assert.Empty(context.Required2s.Where(e => orphanedIds.Contains(e.Id)));
            }

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Equal(1, root.RequiredChildren.Count);
                Assert.DoesNotContain(removedId, root.RequiredChildren.Select(e => e.Id));

                Assert.Empty(context.Required1s.Where(e => e.Id == removedId));
                Assert.Empty(context.Required2s.Where(e => orphanedIds.Contains(e.Id)));
            }
        }

        [ConditionalFact]
        public virtual void Optional_many_to_one_dependents_are_orphaned()
        {
            int removedId;
            List<int> orphanedIds;

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Equal(2, root.OptionalChildren.Count);

                var removed = root.OptionalChildren.First();

                removedId = removed.Id;
                var orphaned = removed.Children.ToList();
                orphanedIds = orphaned.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));

                Assert.Equal(1, root.OptionalChildren.Count);
                Assert.DoesNotContain(removedId, root.OptionalChildren.Select(e => e.Id));

                Assert.Empty(context.Optional1s.Where(e => e.Id == removedId));
                Assert.Equal(orphanedIds.Count, context.Optional2s.Count(e => orphanedIds.Contains(e.Id)));
            }

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Equal(1, root.OptionalChildren.Count);
                Assert.DoesNotContain(removedId, root.OptionalChildren.Select(e => e.Id));

                Assert.Empty(context.Optional1s.Where(e => e.Id == removedId));
                Assert.Equal(orphanedIds.Count, context.Optional2s.Count(e => orphanedIds.Contains(e.Id)));
            }
        }

        [ConditionalFact]
        public virtual void Optional_one_to_one_are_orphaned()
        {
            int removedId;
            int orphanedId;

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                var removed = root.OptionalSingle;

                removedId = removed.Id;
                var orphaned = removed.Single;
                orphanedId = orphaned.Id;

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);

                Assert.Null(root.OptionalSingle);

                Assert.Empty(context.OptionalSingle1s.Where(e => e.Id == removedId));
                Assert.Equal(1, context.OptionalSingle2s.Count(e => e.Id == orphanedId));
            }

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Null(root.OptionalSingle);

                Assert.Empty(context.OptionalSingle1s.Where(e => e.Id == removedId));
                Assert.Equal(1, context.OptionalSingle2s.Count(e => e.Id == orphanedId));
            }
        }

        [ConditionalFact]
        public virtual void Required_one_to_one_are_cascade_deleted()
        {
            int removedId;
            int orphanedId;

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                var removed = root.RequiredSingle;

                removedId = removed.Id;
                var orphaned = removed.Single;
                orphanedId = orphaned.Id;

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);

                Assert.Null(root.RequiredSingle);

                Assert.Empty(context.RequiredSingle1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredSingle2s.Where(e => e.Id == orphanedId));
            }

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Null(root.RequiredSingle);

                Assert.Empty(context.RequiredSingle1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredSingle2s.Where(e => e.Id == orphanedId));
            }
        }

        [ConditionalFact]
        public virtual void Required_non_PK_one_to_one_are_cascade_deleted()
        {
            int removedId;
            int orphanedId;

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                var removed = root.RequiredNonPkSingle;

                removedId = removed.Id;
                var orphaned = removed.Single;
                orphanedId = orphaned.Id;

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);

                Assert.Null(root.RequiredNonPkSingle);

                Assert.Empty(context.RequiredNonPkSingle1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredNonPkSingle2s.Where(e => e.Id == orphanedId));
            }

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Null(root.RequiredNonPkSingle);

                Assert.Empty(context.RequiredNonPkSingle1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredNonPkSingle2s.Where(e => e.Id == orphanedId));
            }
        }

        [ConditionalFact]
        public virtual void Optional_many_to_one_dependents_with_alternate_key_are_orphaned()
        {
            int removedId;
            List<int> orphanedIds;

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Equal(2, root.OptionalChildrenAk.Count);

                var removed = root.OptionalChildrenAk.First();

                removedId = removed.Id;
                var orphaned = removed.Children.ToList();
                orphanedIds = orphaned.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));

                Assert.Equal(1, root.OptionalChildrenAk.Count);
                Assert.DoesNotContain(removedId, root.OptionalChildrenAk.Select(e => e.Id));

                Assert.Empty(context.OptionalAk1s.Where(e => e.Id == removedId));
                Assert.Equal(orphanedIds.Count, context.OptionalAk2s.Count(e => orphanedIds.Contains(e.Id)));
            }

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Equal(1, root.OptionalChildrenAk.Count);
                Assert.DoesNotContain(removedId, root.OptionalChildrenAk.Select(e => e.Id));

                Assert.Empty(context.OptionalAk1s.Where(e => e.Id == removedId));
                Assert.Equal(orphanedIds.Count, context.OptionalAk2s.Count(e => orphanedIds.Contains(e.Id)));
            }
        }

        [ConditionalFact]
        public virtual void Required_many_to_one_dependents_with_alternate_key_are_cascade_deleted()
        {
            int removedId;
            List<int> orphanedIds;

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Equal(2, root.RequiredChildrenAk.Count);

                var removed = root.RequiredChildrenAk.First();

                removedId = removed.Id;
                var cascadeRemoved = removed.Children.ToList();
                orphanedIds = cascadeRemoved.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Detached));

                Assert.Equal(1, root.RequiredChildrenAk.Count);
                Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                Assert.Empty(context.RequiredAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredAk2s.Where(e => orphanedIds.Contains(e.Id)));
            }

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Equal(1, root.RequiredChildrenAk.Count);
                Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                Assert.Empty(context.RequiredAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredAk2s.Where(e => orphanedIds.Contains(e.Id)));
            }
        }

        [ConditionalFact]
        public virtual void Optional_one_to_one_with_alternate_key_are_orphaned()
        {
            int removedId;
            int orphanedId;

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                var removed = root.OptionalSingleAk;

                removedId = removed.Id;
                var orphaned = removed.Single;
                orphanedId = orphaned.Id;

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);

                Assert.Null(root.OptionalSingleAk);

                Assert.Empty(context.OptionalSingleAk1s.Where(e => e.Id == removedId));
                Assert.Equal(1, context.OptionalSingleAk2s.Count(e => e.Id == orphanedId));
            }

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Null(root.OptionalSingleAk);

                Assert.Empty(context.OptionalSingleAk1s.Where(e => e.Id == removedId));
                Assert.Equal(1, context.OptionalSingleAk2s.Count(e => e.Id == orphanedId));
            }
        }

        [ConditionalFact]
        public virtual void Required_one_to_one_with_alternate_key_are_cascade_deleted()
        {
            int removedId;
            int orphanedId;

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                var removed = root.RequiredSingleAk;

                removedId = removed.Id;
                var orphaned = removed.Single;
                orphanedId = orphaned.Id;

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);

                Assert.Null(root.RequiredSingleAk);

                Assert.Empty(context.RequiredSingleAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredSingleAk2s.Where(e => e.Id == orphanedId));
            }

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Null(root.RequiredSingleAk);

                Assert.Empty(context.RequiredSingleAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredSingleAk2s.Where(e => e.Id == orphanedId));
            }
        }

        [ConditionalFact]
        public virtual void Required_non_PK_one_to_one_with_alternate_key_are_cascade_deleted()
        {
            int removedId;
            int orphanedId;

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                var removed = root.RequiredNonPkSingleAk;

                removedId = removed.Id;
                var orphaned = removed.Single;
                orphanedId = orphaned.Id;

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);

                Assert.Null(root.RequiredNonPkSingleAk);

                Assert.Empty(context.RequiredNonPkSingleAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredNonPkSingleAk2s.Where(e => e.Id == orphanedId));
            }

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Null(root.RequiredNonPkSingleAk);

                Assert.Empty(context.RequiredNonPkSingleAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredNonPkSingleAk2s.Where(e => e.Id == orphanedId));
            }
        }

        [ConditionalFact]
        public virtual void Required_many_to_one_dependents_are_cascade_deleted_in_store()
        {
            int removedId;
            List<int> orphanedIds;

            using (var context = CreateContext())
            {
                var removed = LoadFullGraph(context).RequiredChildren.First();

                removedId = removed.Id;
                orphanedIds = removed.Children.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
            }

            using (var context = CreateContext())
            {
                var root = context.Roots.Include(e => e.RequiredChildren).Single();

                var removed = root.RequiredChildren.Single(e => e.Id == removedId);

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Equal(1, root.RequiredChildren.Count);
                Assert.DoesNotContain(removedId, root.RequiredChildren.Select(e => e.Id));

                Assert.Empty(context.Required1s.Where(e => e.Id == removedId));
                Assert.Empty(context.Required2s.Where(e => orphanedIds.Contains(e.Id)));
            }

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Equal(1, root.RequiredChildren.Count);
                Assert.DoesNotContain(removedId, root.RequiredChildren.Select(e => e.Id));

                Assert.Empty(context.Required1s.Where(e => e.Id == removedId));
                Assert.Empty(context.Required2s.Where(e => orphanedIds.Contains(e.Id)));
            }
        }

        [ConditionalFact]
        public virtual void Required_one_to_one_are_cascade_deleted_in_store()
        {
            int removedId;
            int orphanedId;

            using (var context = CreateContext())
            {
                var removed = LoadFullGraph(context).RequiredSingle;

                removedId = removed.Id;
                orphanedId = removed.Single.Id;
            }

            using (var context = CreateContext())
            {
                var root = context.Roots.Include(e => e.RequiredSingle).Single();

                var removed = root.RequiredSingle;

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Null(root.RequiredSingle);

                Assert.Empty(context.RequiredSingle1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredSingle2s.Where(e => e.Id == orphanedId));
            }

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Null(root.RequiredSingle);

                Assert.Empty(context.RequiredSingle1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredSingle2s.Where(e => e.Id == orphanedId));
            }
        }

        [ConditionalFact]
        public virtual void Required_non_PK_one_to_one_are_cascade_deleted_in_store()
        {
            int removedId;
            int orphanedId;

            using (var context = CreateContext())
            {
                var removed = LoadFullGraph(context).RequiredNonPkSingle;

                removedId = removed.Id;
                orphanedId = removed.Single.Id;
            }

            using (var context = CreateContext())
            {
                var root = context.Roots.Include(e => e.RequiredNonPkSingle).Single();

                var removed = root.RequiredNonPkSingle;

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Null(root.RequiredNonPkSingle);

                Assert.Empty(context.RequiredNonPkSingle1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredNonPkSingle2s.Where(e => e.Id == orphanedId));
            }

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Null(root.RequiredNonPkSingle);

                Assert.Empty(context.RequiredNonPkSingle1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredNonPkSingle2s.Where(e => e.Id == orphanedId));
            }
        }


        [ConditionalFact]
        public virtual void Required_many_to_one_dependents_with_alternate_key_are_cascade_deleted_in_store()
        {
            int removedId;
            List<int> orphanedIds;

            using (var context = CreateContext())
            {
                var removed = LoadFullGraph(context).RequiredChildrenAk.First();

                removedId = removed.Id;
                orphanedIds = removed.Children.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
            }

            using (var context = CreateContext())
            {
                var root = context.Roots.Include(e => e.RequiredChildrenAk).Single();

                var removed = root.RequiredChildrenAk.Single(e => e.Id == removedId);

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Equal(1, root.RequiredChildrenAk.Count);
                Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                Assert.Empty(context.RequiredAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredAk2s.Where(e => orphanedIds.Contains(e.Id)));
            }

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Equal(1, root.RequiredChildrenAk.Count);
                Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                Assert.Empty(context.RequiredAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredAk2s.Where(e => orphanedIds.Contains(e.Id)));
            }
        }

        [ConditionalFact]
        public virtual void Required_one_to_one_with_alternate_key_are_cascade_deleted_in_store()
        {
            int removedId;
            int orphanedId;

            using (var context = CreateContext())
            {
                var removed = LoadFullGraph(context).RequiredSingleAk;

                removedId = removed.Id;
                orphanedId = removed.Single.Id;
            }

            using (var context = CreateContext())
            {
                var root = context.Roots.Include(e => e.RequiredSingleAk).Single();

                var removed = root.RequiredSingleAk;

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Null(root.RequiredSingleAk);

                Assert.Empty(context.RequiredSingleAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredSingleAk2s.Where(e => e.Id == orphanedId));
            }

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Null(root.RequiredSingleAk);

                Assert.Empty(context.RequiredSingleAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredSingleAk2s.Where(e => e.Id == orphanedId));
            }
        }

        [ConditionalFact]
        public virtual void Required_non_PK_one_to_one_with_alternate_key_are_cascade_deleted_in_store()
        {
            int removedId;
            int orphanedId;

            using (var context = CreateContext())
            {
                var removed = LoadFullGraph(context).RequiredNonPkSingleAk;

                removedId = removed.Id;
                orphanedId = removed.Single.Id;
            }

            using (var context = CreateContext())
            {
                var root = context.Roots.Include(e => e.RequiredNonPkSingleAk).Single();

                var removed = root.RequiredNonPkSingleAk;

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Null(root.RequiredNonPkSingleAk);

                Assert.Empty(context.RequiredNonPkSingleAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredNonPkSingleAk2s.Where(e => e.Id == orphanedId));
            }

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Null(root.RequiredNonPkSingleAk);

                Assert.Empty(context.RequiredNonPkSingleAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredNonPkSingleAk2s.Where(e => e.Id == orphanedId));
            }
        }

        [ConditionalFact]
        public virtual void Optional_many_to_one_dependents_are_orphaned_in_store()
        {
            int removedId;
            List<int> orphanedIds;

            using (var context = CreateContext())
            {
                var removed = LoadFullGraph(context).OptionalChildren.First();

                removedId = removed.Id;
                orphanedIds = removed.Children.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
            }

            using (var context = CreateContext())
            {
                var root = context.Roots.Include(e => e.OptionalChildren).Single();

                var removed = root.OptionalChildren.First(e => e.Id == removedId);

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Equal(1, root.OptionalChildren.Count);
                Assert.DoesNotContain(removedId, root.OptionalChildren.Select(e => e.Id));

                Assert.Empty(context.Optional1s.Where(e => e.Id == removedId));

                var orphaned = context.Optional2s.Where(e => orphanedIds.Contains(e.Id)).ToList();
                Assert.Equal(orphanedIds.Count, orphaned.Count);
                Assert.True(orphaned.All(e => e.ParentId == null));
            }

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Equal(1, root.OptionalChildren.Count);
                Assert.DoesNotContain(removedId, root.OptionalChildren.Select(e => e.Id));

                Assert.Empty(context.Optional1s.Where(e => e.Id == removedId));

                var orphaned = context.Optional2s.Where(e => orphanedIds.Contains(e.Id)).ToList();
                Assert.Equal(orphanedIds.Count, orphaned.Count);
                Assert.True(orphaned.All(e => e.ParentId == null));
            }
        }

        [ConditionalFact]
        public virtual void Optional_one_to_one_are_orphaned_in_store()
        {
            int removedId;
            int orphanedId;

            using (var context = CreateContext())
            {
                var removed = LoadFullGraph(context).OptionalSingle;

                removedId = removed.Id;
                orphanedId = removed.Single.Id;
            }

            using (var context = CreateContext())
            {
                var root = context.Roots.Include(e => e.OptionalSingle).Single();

                var removed = root.OptionalSingle;

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Null(root.OptionalSingle);

                Assert.Empty(context.OptionalSingle1s.Where(e => e.Id == removedId));
                Assert.Null(context.OptionalSingle2s.Single(e => e.Id == orphanedId).BackId);
            }

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Null(root.OptionalSingle);

                Assert.Empty(context.OptionalSingle1s.Where(e => e.Id == removedId));
                Assert.Null(context.OptionalSingle2s.Single(e => e.Id == orphanedId).BackId);
            }
        }

        [ConditionalFact]
        public virtual void Optional_many_to_one_dependents_with_alternate_key_are_orphaned_in_store()
        {
            int removedId;
            List<int> orphanedIds;

            using (var context = CreateContext())
            {
                var removed = LoadFullGraph(context).OptionalChildrenAk.First();

                removedId = removed.Id;
                orphanedIds = removed.Children.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
            }

            using (var context = CreateContext())
            {
                var root = context.Roots.Include(e => e.OptionalChildrenAk).Single();

                var removed = root.OptionalChildrenAk.First(e => e.Id == removedId);

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Equal(1, root.OptionalChildrenAk.Count);
                Assert.DoesNotContain(removedId, root.OptionalChildrenAk.Select(e => e.Id));

                Assert.Empty(context.OptionalAk1s.Where(e => e.Id == removedId));

                var orphaned = context.OptionalAk2s.Where(e => orphanedIds.Contains(e.Id)).ToList();
                Assert.Equal(orphanedIds.Count, orphaned.Count);
                Assert.True(orphaned.All(e => e.ParentId == null));
            }

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Equal(1, root.OptionalChildrenAk.Count);
                Assert.DoesNotContain(removedId, root.OptionalChildrenAk.Select(e => e.Id));

                Assert.Empty(context.OptionalAk1s.Where(e => e.Id == removedId));

                var orphaned = context.OptionalAk2s.Where(e => orphanedIds.Contains(e.Id)).ToList();
                Assert.Equal(orphanedIds.Count, orphaned.Count);
                Assert.True(orphaned.All(e => e.ParentId == null));
            }
        }

        [ConditionalFact]
        public virtual void Optional_one_to_one_with_alternate_key_are_orphaned_in_store()
        {
            int removedId;
            int orphanedId;

            using (var context = CreateContext())
            {
                var removed = LoadFullGraph(context).OptionalSingleAk;

                removedId = removed.Id;
                orphanedId = removed.Single.Id;
            }

            using (var context = CreateContext())
            {
                var root = context.Roots.Include(e => e.OptionalSingleAk).Single();

                var removed = root.OptionalSingleAk;

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Null(root.OptionalSingleAk);

                Assert.Empty(context.OptionalSingleAk1s.Where(e => e.Id == removedId));
                Assert.Null(context.OptionalSingleAk2s.Single(e => e.Id == orphanedId).BackId);
            }

            using (var context = CreateContext())
            {
                var root = LoadFullGraph(context);

                Assert.Null(root.OptionalSingleAk);

                Assert.Empty(context.OptionalSingleAk1s.Where(e => e.Id == removedId));
                Assert.Null(context.OptionalSingleAk2s.Single(e => e.Id == orphanedId).BackId);
            }
        }

        public enum ChangeMechanism
        {
            Dependent,
            Principal,
            FK
        }

        protected static Root LoadFullGraph(GraphUpdatesContext context, Expression<Func<Root, bool>> predicate = null)
        {
            var query = context.Roots
                .Include(e => e.RequiredChildren)
                .Include(e => e.OptionalChildren)
                .Include(e => e.RequiredSingle)
                .Include(e => e.RequiredNonPkSingle)
                .Include(e => e.OptionalSingle);

            var loadedGraph = predicate == null
                ? query.Single()
                : query.Single(predicate);

            // TODO: Use Include when supported
            context.RequiredAk1s.Load();
            context.OptionalAk1s.Load();
            context.RequiredSingleAk1s.Load();
            context.OptionalSingleAk1s.Load();
            context.RequiredNonPkSingleAk1s.Load();

            context.RequiredSingle2s.Load();
            context.RequiredNonPkSingle2s.Load();
            context.OptionalSingle2s.Load();
            context.Required2s.Load();
            context.Optional2s.Load();

            context.RequiredSingleAk2s.Load();
            context.RequiredNonPkSingleAk2s.Load();
            context.OptionalSingleAk2s.Load();
            context.RequiredAk2s.Load();
            context.OptionalAk2s.Load();

            return loadedGraph;
        }

        private static void AssertKeys(Root expected, Root actual)
        {
            Assert.Equal(expected.Id, actual.Id);

            Assert.Equal(
                expected.RequiredChildren.OrderBy(e => e.Id).Select(e => e.Id),
                actual.RequiredChildren.OrderBy(e => e.Id).Select(e => e.Id));

            Assert.Equal(
                expected.RequiredChildren.OrderBy(e => e.Id).Select(e => e.Children.Count),
                actual.RequiredChildren.OrderBy(e => e.Id).Select(e => e.Children.Count));

            Assert.Equal(
                expected.RequiredChildren.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.Id),
                actual.RequiredChildren.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.Id));

            Assert.Equal(
                expected.OptionalChildren.OrderBy(e => e.Id).Select(e => e.Id),
                actual.OptionalChildren.OrderBy(e => e.Id).Select(e => e.Id));

            Assert.Equal(
                expected.OptionalChildren.OrderBy(e => e.Id).Select(e => e.Children.Count),
                actual.OptionalChildren.OrderBy(e => e.Id).Select(e => e.Children.Count));

            Assert.Equal(
                expected.OptionalChildren.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.Id),
                actual.OptionalChildren.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.Id));

            Assert.Equal(expected.RequiredSingle?.Id, actual.RequiredSingle?.Id);
            Assert.Equal(expected.OptionalSingle?.Id, actual.OptionalSingle?.Id);
            Assert.Equal(expected.RequiredNonPkSingle?.Id, actual.RequiredNonPkSingle?.Id);

            Assert.Equal(expected.RequiredSingle?.Single?.Id, actual.RequiredSingle?.Single?.Id);
            Assert.Equal(expected.OptionalSingle?.Single?.Id, actual.OptionalSingle?.Single?.Id);
            Assert.Equal(expected.RequiredNonPkSingle?.Single?.Id, actual.RequiredNonPkSingle?.Single?.Id);

            Assert.Equal(expected.AlternateId, actual.AlternateId);

            Assert.Equal(
                expected.RequiredChildrenAk.OrderBy(e => e.Id).Select(e => e.AlternateId),
                actual.RequiredChildrenAk.OrderBy(e => e.Id).Select(e => e.AlternateId));

            Assert.Equal(
                expected.RequiredChildrenAk.OrderBy(e => e.Id).Select(e => e.Children.Count),
                actual.RequiredChildrenAk.OrderBy(e => e.Id).Select(e => e.Children.Count));

            Assert.Equal(
                expected.RequiredChildrenAk.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.AlternateId),
                actual.RequiredChildrenAk.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.AlternateId));

            Assert.Equal(
                expected.OptionalChildrenAk.OrderBy(e => e.Id).Select(e => e.AlternateId),
                actual.OptionalChildrenAk.OrderBy(e => e.Id).Select(e => e.AlternateId));

            Assert.Equal(
                expected.OptionalChildrenAk.OrderBy(e => e.Id).Select(e => e.Children.Count),
                actual.OptionalChildrenAk.OrderBy(e => e.Id).Select(e => e.Children.Count));

            Assert.Equal(
                expected.OptionalChildrenAk.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.AlternateId),
                actual.OptionalChildrenAk.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.AlternateId));

            Assert.Equal(expected.RequiredSingleAk?.AlternateId, actual.RequiredSingleAk?.AlternateId);
            Assert.Equal(expected.OptionalSingleAk?.AlternateId, actual.OptionalSingleAk?.AlternateId);
            Assert.Equal(expected.RequiredNonPkSingleAk?.AlternateId, actual.RequiredNonPkSingleAk?.AlternateId);

            Assert.Equal(expected.RequiredSingleAk?.Single?.AlternateId, actual.RequiredSingleAk?.Single?.AlternateId);
            Assert.Equal(expected.OptionalSingleAk?.Single?.AlternateId, actual.OptionalSingleAk?.Single?.AlternateId);
            Assert.Equal(expected.RequiredNonPkSingleAk?.Single?.AlternateId, actual.RequiredNonPkSingleAk?.Single?.AlternateId);
        }

        private static void AssertNavigations(Root root)
        {
            foreach (var child in root.RequiredChildren)
            {
                Assert.Same(root, child.Parent);
                Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
            }

            foreach (var child in root.OptionalChildren)
            {
                Assert.Same(root, child.Parent);
                Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
            }

            foreach (var child in root.OptionalChildren)
            {
                Assert.Same(root, child.Parent);
                Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
            }

            Assert.Same(root, root.RequiredSingle.Root);
            Assert.Same(root, root.OptionalSingle.Root);
            Assert.Same(root, root.RequiredNonPkSingle.Root);

            Assert.Same(root.RequiredSingle, root.RequiredSingle.Single.Back);
            Assert.Same(root.OptionalSingle, root.OptionalSingle.Single.Back);
            Assert.Same(root.RequiredNonPkSingle, root.RequiredNonPkSingle.Single.Back);

            foreach (var child in root.RequiredChildrenAk)
            {
                Assert.Same(root, child.Parent);
                Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
            }

            foreach (var child in root.OptionalChildrenAk)
            {
                Assert.Same(root, child.Parent);
                Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
            }

            foreach (var child in root.OptionalChildrenAk)
            {
                Assert.Same(root, child.Parent);
                Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
            }

            Assert.Same(root, root.RequiredSingleAk.Root);
            Assert.Same(root, root.OptionalSingleAk.Root);
            Assert.Same(root, root.RequiredNonPkSingleAk.Root);

            Assert.Same(root.RequiredSingleAk, root.RequiredSingleAk.Single.Back);
            Assert.Same(root.OptionalSingleAk, root.OptionalSingleAk.Single.Back);
            Assert.Same(root.RequiredNonPkSingleAk, root.RequiredNonPkSingleAk.Single.Back);
        }

        private static void AssertPossiblyNullNavigations(Root root)
        {
            foreach (var child in root.RequiredChildren)
            {
                Assert.Same(root, child.Parent);
                Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
            }

            foreach (var child in root.OptionalChildren)
            {
                Assert.Same(root, child.Parent);
                Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
            }

            foreach (var child in root.OptionalChildren)
            {
                Assert.Same(root, child.Parent);
                Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
            }

            if (root.RequiredSingle != null)
            {
                Assert.Same(root, root.RequiredSingle.Root);
                Assert.Same(root.RequiredSingle, root.RequiredSingle.Single.Back);
            }

            if (root.OptionalSingle != null)
            {
                Assert.Same(root, root.OptionalSingle.Root);
                Assert.Same(root.OptionalSingle, root.OptionalSingle.Single.Back);
            }

            if (root.RequiredNonPkSingle != null)
            {
                Assert.Same(root, root.RequiredNonPkSingle.Root);
                Assert.Same(root.RequiredNonPkSingle, root.RequiredNonPkSingle.Single.Back);
            }

            foreach (var child in root.RequiredChildrenAk)
            {
                Assert.Same(root, child.Parent);
                Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
            }

            foreach (var child in root.OptionalChildrenAk)
            {
                Assert.Same(root, child.Parent);
                Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
            }

            foreach (var child in root.OptionalChildrenAk)
            {
                Assert.Same(root, child.Parent);
                Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
            }

            if (root.RequiredSingleAk != null)
            {
                Assert.Same(root, root.RequiredSingleAk.Root);
                Assert.Same(root.RequiredSingleAk, root.RequiredSingleAk.Single.Back);
            }

            if (root.OptionalSingleAk != null)
            {
                Assert.Same(root, root.OptionalSingleAk.Root);
                Assert.Same(root.OptionalSingleAk, root.OptionalSingleAk.Single.Back);
            }

            if (root.RequiredNonPkSingleAk != null)
            {
                Assert.Same(root, root.RequiredNonPkSingleAk.Root);
                Assert.Same(root.RequiredNonPkSingleAk, root.RequiredNonPkSingleAk.Single.Back);
            }
        }

        protected class Root
        {
            public int Id { get; set; }
            public Guid AlternateId { get; set; }

            public ICollection<Required1> RequiredChildren { get; set; } = new List<Required1>();
            public ICollection<Optional1> OptionalChildren { get; set; } = new List<Optional1>();
            public RequiredSingle1 RequiredSingle { get; set; }
            public RequiredNonPkSingle1 RequiredNonPkSingle { get; set; }
            public OptionalSingle1 OptionalSingle { get; set; }

            public ICollection<RequiredAk1> RequiredChildrenAk { get; set; } = new List<RequiredAk1>();
            public ICollection<OptionalAk1> OptionalChildrenAk { get; set; } = new List<OptionalAk1>();
            public RequiredSingleAk1 RequiredSingleAk { get; set; }
            public RequiredNonPkSingleAk1 RequiredNonPkSingleAk { get; set; }
            public OptionalSingleAk1 OptionalSingleAk { get; set; }
        }

        protected class Required1
        {
            public int Id { get; set; }

            public int ParentId { get; set; }
            public Root Parent { get; set; }

            public ICollection<Required2> Children { get; set; } = new List<Required2>();
        }

        protected class Required2
        {
            public int Id { get; set; }

            public int ParentId { get; set; }
            public Required1 Parent { get; set; }
        }

        protected class Optional1
        {
            public int Id { get; set; }

            public int? ParentId { get; set; }
            public Root Parent { get; set; }

            public ICollection<Optional2> Children { get; set; } = new List<Optional2>();
        }

        protected class Optional2
        {
            public int Id { get; set; }

            public int? ParentId { get; set; }
            public Optional1 Parent { get; set; }
        }

        protected class RequiredSingle1
        {
            public int Id { get; set; }

            public Root Root { get; set; }
            public RequiredSingle2 Single { get; set; }
        }

        protected class RequiredSingle2
        {
            public int Id { get; set; }

            public RequiredSingle1 Back { get; set; }
        }

        protected class RequiredNonPkSingle1
        {
            public int Id { get; set; }

            public int RootId { get; set; }
            public Root Root { get; set; }

            public RequiredNonPkSingle2 Single { get; set; }
        }

        protected class RequiredNonPkSingle2
        {
            public int Id { get; set; }

            public int BackId { get; set; }
            public RequiredNonPkSingle1 Back { get; set; }
        }

        protected class OptionalSingle1
        {
            public int Id { get; set; }

            public int? RootId { get; set; }
            public Root Root { get; set; }

            public OptionalSingle2 Single { get; set; }
        }

        protected class OptionalSingle2
        {
            public int Id { get; set; }

            public int? BackId { get; set; }
            public OptionalSingle1 Back { get; set; }
        }

        protected class RequiredAk1
        {
            public int Id { get; set; }
            public Guid AlternateId { get; set; }

            public Guid ParentId { get; set; }
            public Root Parent { get; set; }

            public ICollection<RequiredAk2> Children { get; set; } = new List<RequiredAk2>();
        }

        protected class RequiredAk2
        {
            public int Id { get; set; }
            public Guid AlternateId { get; set; }

            public Guid ParentId { get; set; }
            public RequiredAk1 Parent { get; set; }
        }

        protected class OptionalAk1
        {
            public int Id { get; set; }
            public Guid AlternateId { get; set; }

            public Guid? ParentId { get; set; }
            public Root Parent { get; set; }

            public ICollection<OptionalAk2> Children { get; set; } = new List<OptionalAk2>();
        }

        protected class OptionalAk2
        {
            public int Id { get; set; }
            public Guid AlternateId { get; set; }

            public Guid? ParentId { get; set; }
            public OptionalAk1 Parent { get; set; }
        }

        protected class RequiredSingleAk1
        {
            public int Id { get; set; }
            public Guid AlternateId { get; set; }

            public Guid RootId { get; set; }
            public Root Root { get; set; }

            public RequiredSingleAk2 Single { get; set; }
        }

        protected class RequiredSingleAk2
        {
            public int Id { get; set; }
            public Guid AlternateId { get; set; }

            public Guid BackId { get; set; }
            public RequiredSingleAk1 Back { get; set; }
        }

        protected class RequiredNonPkSingleAk1
        {
            public int Id { get; set; }
            public Guid AlternateId { get; set; }

            public Guid RootId { get; set; }
            public Root Root { get; set; }

            public RequiredNonPkSingleAk2 Single { get; set; }
        }

        protected class RequiredNonPkSingleAk2
        {
            public int Id { get; set; }
            public Guid AlternateId { get; set; }

            public Guid BackId { get; set; }
            public RequiredNonPkSingleAk1 Back { get; set; }
        }

        protected class OptionalSingleAk1
        {
            public int Id { get; set; }
            public Guid AlternateId { get; set; }

            public Guid? RootId { get; set; }
            public Root Root { get; set; }

            public OptionalSingleAk2 Single { get; set; }
        }

        protected class OptionalSingleAk2
        {
            public int Id { get; set; }
            public Guid AlternateId { get; set; }

            public Guid? BackId { get; set; }
            public OptionalSingleAk1 Back { get; set; }
        }

        protected class GraphUpdatesContext : DbContext
        {
            public GraphUpdatesContext(IServiceProvider serviceProvider, DbContextOptions options)
                : base(serviceProvider, options)
            {
            }

            public DbSet<Root> Roots { get; set; }
            public DbSet<RequiredSingle1> RequiredSingle1s { get; set; }
            public DbSet<RequiredSingle2> RequiredSingle2s { get; set; }
            public DbSet<RequiredNonPkSingle1> RequiredNonPkSingle1s { get; set; }
            public DbSet<RequiredNonPkSingle2> RequiredNonPkSingle2s { get; set; }
            public DbSet<OptionalSingle1> OptionalSingle1s { get; set; }
            public DbSet<OptionalSingle2> OptionalSingle2s { get; set; }
            public DbSet<Required1> Required1s { get; set; }
            public DbSet<Optional1> Optional1s { get; set; }
            public DbSet<Required2> Required2s { get; set; }
            public DbSet<Optional2> Optional2s { get; set; }

            public DbSet<RequiredSingleAk1> RequiredSingleAk1s { get; set; }
            public DbSet<RequiredSingleAk2> RequiredSingleAk2s { get; set; }
            public DbSet<RequiredNonPkSingleAk1> RequiredNonPkSingleAk1s { get; set; }
            public DbSet<RequiredNonPkSingleAk2> RequiredNonPkSingleAk2s { get; set; }
            public DbSet<OptionalSingleAk1> OptionalSingleAk1s { get; set; }
            public DbSet<OptionalSingleAk2> OptionalSingleAk2s { get; set; }
            public DbSet<RequiredAk1> RequiredAk1s { get; set; }
            public DbSet<OptionalAk1> OptionalAk1s { get; set; }
            public DbSet<RequiredAk2> RequiredAk2s { get; set; }
            public DbSet<OptionalAk2> OptionalAk2s { get; set; }
        }

        protected GraphUpdatesContext CreateContext()
        {
            return (GraphUpdatesContext)Fixture.CreateContext(TestStore);
        }

        public void Dispose()
        {
            TestStore.Dispose();
        }

        protected TFixture Fixture { get; }

        protected TTestStore TestStore { get; }

        public abstract class GraphUpdatesFixtureBase
        {
            public abstract TTestStore CreateTestStore();

            public abstract DbContext CreateContext(TTestStore testStore);

            protected virtual void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Root>(b =>
                    {
                        b.Property(e => e.AlternateId).ValueGeneratedOnAdd();

                        b.HasMany(e => e.RequiredChildren)
                            .WithOne(e => e.Parent)
                            .ForeignKey(e => e.ParentId);

                        b.HasMany(e => e.OptionalChildren)
                            .WithOne(e => e.Parent)
                            .ForeignKey(e => e.ParentId)
                            .OnDelete(DeleteBehavior.SetNull);

                        b.HasOne(e => e.RequiredSingle)
                            .WithOne(e => e.Root)
                            .ForeignKey<RequiredSingle1>(e => e.Id);

                        b.HasOne(e => e.OptionalSingle)
                            .WithOne(e => e.Root)
                            .ForeignKey<OptionalSingle1>(e => e.RootId)
                            .OnDelete(DeleteBehavior.SetNull);

                        b.HasOne(e => e.RequiredNonPkSingle)
                            .WithOne(e => e.Root)
                            .ForeignKey<RequiredNonPkSingle1>(e => e.RootId);

                        b.HasMany(e => e.RequiredChildrenAk)
                            .WithOne(e => e.Parent)
                            .PrincipalKey(e => e.AlternateId)
                            .ForeignKey(e => e.ParentId);

                        b.HasMany(e => e.OptionalChildrenAk)
                            .WithOne(e => e.Parent)
                            .PrincipalKey(e => e.AlternateId)
                            .ForeignKey(e => e.ParentId)
                            .OnDelete(DeleteBehavior.SetNull);

                        b.HasOne(e => e.RequiredSingleAk)
                            .WithOne(e => e.Root)
                            .PrincipalKey<Root>(e => e.AlternateId)
                            .ForeignKey<RequiredSingleAk1>(e => e.RootId);

                        b.HasOne(e => e.OptionalSingleAk)
                            .WithOne(e => e.Root)
                            .PrincipalKey<Root>(e => e.AlternateId)
                            .ForeignKey<OptionalSingleAk1>(e => e.RootId)
                            .OnDelete(DeleteBehavior.SetNull);

                        b.HasOne(e => e.RequiredNonPkSingleAk)
                            .WithOne(e => e.Root)
                            .PrincipalKey<Root>(e => e.AlternateId)
                            .ForeignKey<RequiredNonPkSingleAk1>(e => e.RootId);
                    });

                modelBuilder.Entity<Required1>()
                    .HasMany(e => e.Children)
                    .WithOne(e => e.Parent)
                    .ForeignKey(e => e.ParentId);

                modelBuilder.Entity<Optional1>()
                    .HasMany(e => e.Children)
                    .WithOne(e => e.Parent)
                    .ForeignKey(e => e.ParentId)
                    .OnDelete(DeleteBehavior.SetNull);

                modelBuilder.Entity<RequiredSingle1>()
                    .HasOne(e => e.Single)
                    .WithOne(e => e.Back)
                    .ForeignKey<RequiredSingle2>(e => e.Id);

                modelBuilder.Entity<OptionalSingle1>()
                    .HasOne(e => e.Single)
                    .WithOne(e => e.Back)
                    .ForeignKey<OptionalSingle2>(e => e.BackId)
                    .OnDelete(DeleteBehavior.SetNull);

                modelBuilder.Entity<RequiredNonPkSingle1>()
                    .HasOne(e => e.Single)
                    .WithOne(e => e.Back)
                    .ForeignKey<RequiredNonPkSingle2>(e => e.BackId);

                modelBuilder.Entity<RequiredAk1>(b =>
                    {
                        b.Property(e => e.AlternateId)
                            .ValueGeneratedOnAdd();

                        b.HasMany(e => e.Children)
                            .WithOne(e => e.Parent)
                            .PrincipalKey(e => e.AlternateId)
                            .ForeignKey(e => e.ParentId);
                    });

                modelBuilder.Entity<OptionalAk1>(b =>
                    {
                        b.Property(e => e.AlternateId)
                            .ValueGeneratedOnAdd();

                        b.HasMany(e => e.Children)
                            .WithOne(e => e.Parent)
                            .PrincipalKey(e => e.AlternateId)
                            .ForeignKey(e => e.ParentId)
                            .OnDelete(DeleteBehavior.SetNull);
                    });

                modelBuilder.Entity<RequiredSingleAk1>(b =>
                    {
                        b.Property(e => e.AlternateId)
                            .ValueGeneratedOnAdd();

                        b.HasOne(e => e.Single)
                            .WithOne(e => e.Back)
                            .ForeignKey<RequiredSingleAk2>(e => e.BackId)
                            .PrincipalKey<RequiredSingleAk1>(e => e.AlternateId);
                    });

                modelBuilder.Entity<OptionalSingleAk1>(b =>
                    {
                        b.Property(e => e.AlternateId)
                            .ValueGeneratedOnAdd();

                        b.HasOne(e => e.Single)
                            .WithOne(e => e.Back)
                            .ForeignKey<OptionalSingleAk2>(e => e.BackId)
                            .PrincipalKey<OptionalSingleAk1>(e => e.AlternateId)
                            .OnDelete(DeleteBehavior.SetNull);
                    });

                modelBuilder.Entity<RequiredNonPkSingleAk1>(b =>
                    {
                        b.Property(e => e.AlternateId)
                            .ValueGeneratedOnAdd();

                        b.HasOne(e => e.Single)
                            .WithOne(e => e.Back)
                            .ForeignKey<RequiredNonPkSingleAk2>(e => e.BackId)
                            .PrincipalKey<RequiredNonPkSingleAk1>(e => e.AlternateId);
                    });

                modelBuilder.Entity<RequiredAk2>()
                    .Property(e => e.AlternateId)
                    .ValueGeneratedOnAdd();

                modelBuilder.Entity<OptionalAk2>()
                    .Property(e => e.AlternateId)
                    .ValueGeneratedOnAdd();

                modelBuilder.Entity<RequiredSingleAk2>()
                    .Property(e => e.AlternateId)
                    .ValueGeneratedOnAdd();

                modelBuilder.Entity<RequiredNonPkSingleAk2>()
                    .Property(e => e.AlternateId)
                    .ValueGeneratedOnAdd();

                modelBuilder.Entity<OptionalSingleAk2>()
                    .Property(e => e.AlternateId)
                    .ValueGeneratedOnAdd();
            }

            public abstract int IntSentinel { get; }

            protected virtual object CreateFullGraph()
            {
                return new Root
                {
                    Id = IntSentinel,
                    AlternateId = Guid.NewGuid(),
                    RequiredChildren = new List<Required1>
                    {
                        new Required1
                        {
                            Id = IntSentinel,
                            ParentId = IntSentinel,
                            Children = new List<Required2>
                            {
                                new Required2 { ParentId = IntSentinel, Id = IntSentinel },
                                new Required2 { ParentId = IntSentinel, Id = IntSentinel }
                            }
                        },
                        new Required1
                        {
                            Id = IntSentinel,
                            ParentId = IntSentinel,
                            Children = new List<Required2>
                            {
                                new Required2 { ParentId = IntSentinel, Id = IntSentinel },
                                new Required2 { ParentId = IntSentinel, Id = IntSentinel }
                            }
                        }
                    },
                    OptionalChildren = new List<Optional1>
                    {
                        new Optional1
                        {
                            Id = IntSentinel,
                            Children = new List<Optional2>
                            {
                                new Optional2 { Id = IntSentinel },
                                new Optional2 { Id = IntSentinel }
                            }
                        },
                        new Optional1
                        {
                            Id = IntSentinel,
                            Children = new List<Optional2>
                            {
                                new Optional2 { Id = IntSentinel },
                                new Optional2 { Id = IntSentinel }
                            }
                        }
                    },
                    RequiredSingle = new RequiredSingle1
                    {
                        Id = IntSentinel,
                        Single = new RequiredSingle2 { Id = IntSentinel }
                    },
                    OptionalSingle = new OptionalSingle1
                    {
                        Id = IntSentinel,
                        Single = new OptionalSingle2 { Id = IntSentinel }
                    },
                    RequiredNonPkSingle = new RequiredNonPkSingle1
                    {
                        Id = IntSentinel,
                        RootId = IntSentinel,
                        Single = new RequiredNonPkSingle2 { BackId = IntSentinel, Id = IntSentinel }
                    },
                    RequiredChildrenAk = new List<RequiredAk1>
                    {
                        new RequiredAk1
                        {
                            Id = IntSentinel,
                            AlternateId = Guid.NewGuid(),
                            Children = new List<RequiredAk2>
                            {
                                new RequiredAk2 { Id = IntSentinel, AlternateId = Guid.NewGuid() },
                                new RequiredAk2 { Id = IntSentinel, AlternateId = Guid.NewGuid() }
                            }
                        },
                        new RequiredAk1
                        {
                            Id = IntSentinel,
                            AlternateId = Guid.NewGuid(),
                            Children = new List<RequiredAk2>
                            {
                                new RequiredAk2 { Id = IntSentinel, AlternateId = Guid.NewGuid() },
                                new RequiredAk2 { Id = IntSentinel, AlternateId = Guid.NewGuid() }
                            }
                        }
                    },
                    OptionalChildrenAk = new List<OptionalAk1>
                    {
                        new OptionalAk1
                        {
                            Id = IntSentinel,
                            AlternateId = Guid.NewGuid(),
                            Children = new List<OptionalAk2>
                            {
                                new OptionalAk2 { Id = IntSentinel, AlternateId = Guid.NewGuid() },
                                new OptionalAk2 { Id = IntSentinel, AlternateId = Guid.NewGuid() }
                            }
                        },
                        new OptionalAk1
                        {
                            Id = IntSentinel,
                            AlternateId = Guid.NewGuid(),
                            Children = new List<OptionalAk2>
                            {
                                new OptionalAk2 { Id = IntSentinel, AlternateId = Guid.NewGuid() },
                                new OptionalAk2 { Id = IntSentinel, AlternateId = Guid.NewGuid() }
                            }
                        }
                    },
                    RequiredSingleAk = new RequiredSingleAk1
                    {
                        Id = IntSentinel,
                        AlternateId = Guid.NewGuid(),
                        Single = new RequiredSingleAk2 { Id = IntSentinel, AlternateId = Guid.NewGuid() }
                    },
                    OptionalSingleAk = new OptionalSingleAk1
                    {
                        Id = IntSentinel,
                        AlternateId = Guid.NewGuid(),
                        Single = new OptionalSingleAk2 { Id = IntSentinel, AlternateId = Guid.NewGuid() }
                    },
                    RequiredNonPkSingleAk = new RequiredNonPkSingleAk1
                    {
                        Id = IntSentinel,
                        AlternateId = Guid.NewGuid(),
                        Single = new RequiredNonPkSingleAk2 { Id = IntSentinel, AlternateId = Guid.NewGuid() }
                    }
                };
            }

            protected static void SetSentinelValues(ModelBuilder modelBuilder, int intSentinel)
            {
                foreach (var property in modelBuilder.Model.EntityTypes.SelectMany(e => e.Properties)
                    .Where(p => ((IProperty)p).ClrType == typeof(int) || ((IProperty)p).ClrType == typeof(int?)))
                {
                    property.SentinelValue = intSentinel;
                }

                var sentinelGuid = new Guid("{71334AF2-51DE-4015-9CC1-10CE02D151BB}");

                foreach (var property in modelBuilder.Model.EntityTypes.SelectMany(e => e.Properties)
                    .Where(p => ((IProperty)p).ClrType == typeof(Guid) || ((IProperty)p).ClrType == typeof(Guid?)))
                {
                    property.SentinelValue = sentinelGuid;
                }
            }

            protected virtual void Seed(DbContext context)
            {
                var tracker = new KeyValueEntityTracker();

                context.ChangeTracker.TrackGraph(CreateFullGraph(), e => tracker.TrackEntity(e.Entry));
                context.SaveChanges();
            }

            public class KeyValueEntityTracker
            {
                public virtual void TrackEntity(EntityEntry entry)
                    => entry.GetService()
                        .SetEntityState(DetermineState(entry), acceptChanges: true);

                public virtual EntityState DetermineState(EntityEntry entry)
                    => entry.IsKeySet ? EntityState.Unchanged : EntityState.Added;
            }
        }
    }
}
