// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
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

        [ConditionalFact]
        public virtual void Optional_One_to_one_relationships_are_one_to_one()
        {
            using (var context = CreateContext())
            {
                var root = context.Roots.Single(IsTheRoot);

                root.OptionalSingle = new OptionalSingle1();

                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
            }
        }

        [ConditionalFact]
        public virtual void Required_One_to_one_relationships_are_one_to_one()
        {
            using (var context = CreateContext())
            {
                var root = context.Roots.Single(IsTheRoot);

                root.RequiredSingle = new RequiredSingle1();

                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
            }
        }

        [ConditionalFact]
        public virtual void Optional_One_to_one_with_AK_relationships_are_one_to_one()
        {
            using (var context = CreateContext())
            {
                var root = context.Roots.Single(IsTheRoot);

                root.OptionalSingleAk = new OptionalSingleAk1();

                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
            }
        }

        [ConditionalFact]
        public virtual void Required_One_to_one_with_AK_relationships_are_one_to_one()
        {
            using (var context = CreateContext())
            {
                var root = context.Roots.Single(IsTheRoot);

                root.RequiredSingleAk = new RequiredSingleAk1();

                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
            }
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
        public virtual void Save_optional_many_to_one_dependents(ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            var new1 = new Optional1();
            var new1d = new Optional1Derived();
            var new1dd = new Optional1MoreDerived();
            var new2a = new Optional2();
            var new2b = new Optional2();
            var new2d = new Optional2Derived();
            var new2dd = new Optional2MoreDerived();

            if (useExistingEntities)
            {
                using (var context = CreateContext())
                {
                    context.AddRange(new1, new1d, new1dd, new2a, new2d, new2dd, new2b);
                    context.SaveChanges();
                }
            }

            Root root;
            IReadOnlyList<EntityEntry> entries;
            using (var context = CreateContext())
            {
                root = LoadOptionalGraph(context);
                var existing = root.OptionalChildren.OrderBy(e => e.Id).First();

                if (useExistingEntities)
                {
                    new1 = context.Optional1s.Single(e => e.Id == new1.Id);
                    new1d = (Optional1Derived)context.Optional1s.Single(e => e.Id == new1d.Id);
                    new1dd = (Optional1MoreDerived)context.Optional1s.Single(e => e.Id == new1dd.Id);
                    new2a = context.Optional2s.Single(e => e.Id == new2a.Id);
                    new2b = context.Optional2s.Single(e => e.Id == new2b.Id);
                    new2d = (Optional2Derived)context.Optional2s.Single(e => e.Id == new2d.Id);
                    new2dd = (Optional2MoreDerived)context.Optional2s.Single(e => e.Id == new2dd.Id);
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
                    new2a.ParentId = existing.Id;
                    new2b.ParentId = existing.Id;
                    new2d.ParentId = new1d.Id;
                    new2dd.ParentId = new1dd.Id;
                    new1.ParentId = root.Id;
                    new1d.ParentId = root.Id;
                    new1dd.ParentId = root.Id;
                }

                context.SaveChanges();

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
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadOptionalGraph(context);

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);
            }
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
        public virtual void Save_required_many_to_one_dependents(ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            var newRoot = new Root();
            var new1 = new Required1 { Parent = newRoot };
            var new1d = new Required1Derived { Parent = newRoot };
            var new1dd = new Required1MoreDerived { Parent = newRoot };
            var new2a = new Required2 { Parent = new1 };
            var new2b = new Required2 { Parent = new1 };
            var new2d = new Required2Derived { Parent = new1 };
            var new2dd = new Required2MoreDerived { Parent = new1 };

            if (useExistingEntities)
            {
                using (var context = CreateContext())
                {
                    context.AddRange(newRoot, new1, new1d, new1dd, new2a, new2d, new2dd, new2b);
                    context.SaveChanges();
                }
            }

            Root root;
            IReadOnlyList<EntityEntry> entries;
            using (var context = CreateContext())
            {
                root = LoadRequiredGraph(context);
                var existing = root.RequiredChildren.OrderBy(e => e.Id).First();

                if (useExistingEntities)
                {
                    new1 = context.Required1s.Single(e => e.Id == new1.Id);
                    new1d = (Required1Derived)context.Required1s.Single(e => e.Id == new1d.Id);
                    new1dd = (Required1MoreDerived)context.Required1s.Single(e => e.Id == new1dd.Id);
                    new2a = context.Required2s.Single(e => e.Id == new2a.Id);
                    new2b = context.Required2s.Single(e => e.Id == new2b.Id);
                    new2d = (Required2Derived)context.Required2s.Single(e => e.Id == new2d.Id);
                    new2dd = (Required2MoreDerived)context.Required2s.Single(e => e.Id == new2dd.Id);
                }
                else
                {
                    context.AddRange(new1, new1d, new1dd, new2a, new2d, new2dd, new2b);
                    context.Entry(newRoot).State = EntityState.Detached;
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
                    new2a.ParentId = existing.Id;
                    new2b.ParentId = existing.Id;
                    new2d.ParentId = new1d.Id;
                    new2dd.ParentId = new1dd.Id;
                    new1.ParentId = root.Id;
                    new1d.ParentId = root.Id;
                    new1dd.ParentId = root.Id;
                }

                context.SaveChanges();

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
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadRequiredGraph(context);

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.Fk)]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk))]
        [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent))]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk))]
        public virtual void Save_removed_optional_many_to_one_dependents(ChangeMechanism changeMechanism)
        {
            Root root;
            using (var context = CreateContext())
            {
                root = LoadOptionalGraph(context);

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
                var loadedRoot = LoadOptionalGraph(context);

                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);

                Assert.Equal(1, loadedRoot.OptionalChildren.Count());
                Assert.Equal(1, loadedRoot.OptionalChildren.First().Children.Count());
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.Fk)]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk))]
        [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent))]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk))]
        public virtual void Save_removed_required_many_to_one_dependents(ChangeMechanism changeMechanism)
        {
            int removed1Id;
            int removed2Id;
            List<int> removed1ChildrenIds;

            using (var context = CreateContext())
            {
                var root = LoadRequiredGraph(context);

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

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                var root = LoadRequiredGraph(context);

                AssertNavigations(root);

                Assert.Equal(1, root.RequiredChildren.Count());
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
        public virtual void Save_changed_optional_one_to_one(ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            var new2 = new OptionalSingle2();
            var new2d = new OptionalSingle2Derived();
            var new2dd = new OptionalSingle2MoreDerived();
            var new1 = new OptionalSingle1 { Single = new2 };
            var new1d = new OptionalSingle1Derived { Single = new2d };
            var new1dd = new OptionalSingle1MoreDerived { Single = new2dd };

            if (useExistingEntities)
            {
                using (var context = CreateContext())
                {
                    context.AddRange(new1, new1d, new1dd, new2, new2d, new2dd);
                    context.SaveChanges();
                }
            }

            Root root;
            IReadOnlyList<EntityEntry> entries;
            OptionalSingle1 old1;
            OptionalSingle1Derived old1d;
            OptionalSingle1MoreDerived old1dd;
            OptionalSingle2 old2;
            OptionalSingle2Derived old2d;
            OptionalSingle2MoreDerived old2dd;
            using (var context = CreateContext())
            {
                root = LoadOptionalGraph(context);

                old1 = root.OptionalSingle;
                old1d = root.OptionalSingleDerived;
                old1dd = root.OptionalSingleMoreDerived;
                old2 = root.OptionalSingle.Single;
                old2d = (OptionalSingle2Derived)root.OptionalSingleDerived.Single;
                old2dd = (OptionalSingle2MoreDerived)root.OptionalSingleMoreDerived.Single;

                if (useExistingEntities)
                {
                    new1 = context.OptionalSingle1s.Single(e => e.Id == new1.Id);
                    new1d = (OptionalSingle1Derived)context.OptionalSingle1s.Single(e => e.Id == new1d.Id);
                    new1dd = (OptionalSingle1MoreDerived)context.OptionalSingle1s.Single(e => e.Id == new1dd.Id);
                    new2 = context.OptionalSingle2s.Single(e => e.Id == new2.Id);
                    new2d = (OptionalSingle2Derived)context.OptionalSingle2s.Single(e => e.Id == new2d.Id);
                    new2dd = (OptionalSingle2MoreDerived)context.OptionalSingle2s.Single(e => e.Id == new2dd.Id);
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

                context.SaveChanges();

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
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadOptionalGraph(context);

                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);

                var loaded1 = context.OptionalSingle1s.Single(e => e.Id == old1.Id);
                var loaded1d = context.OptionalSingle1s.Single(e => e.Id == old1d.Id);
                var loaded1dd = context.OptionalSingle1s.Single(e => e.Id == old1dd.Id);
                var loaded2 = context.OptionalSingle2s.Single(e => e.Id == old2.Id);
                var loaded2d = context.OptionalSingle2s.Single(e => e.Id == old2d.Id);
                var loaded2dd = context.OptionalSingle2s.Single(e => e.Id == old2dd.Id);

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
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)ChangeMechanism.Fk)]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk))]
        [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent))]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk))]
        public virtual void Save_required_one_to_one_changed_by_reference(ChangeMechanism changeMechanism)
        {
            // This test is a bit strange because the relationships are PK<->PK, which means
            // that an existing entity has to be deleted and then a new entity created that has
            // the same key as the existing entry. In other words it is a new incarnation of the same
            // entity. EF7 can't track two different instances of the same entity, so this has to be
            // done in two steps.

            Root oldRoot;
            IReadOnlyList<EntityEntry> entries;
            RequiredSingle1 old1;
            RequiredSingle2 old2;
            using (var context = CreateContext())
            {
                oldRoot = LoadRequiredGraph(context);

                old1 = oldRoot.RequiredSingle;
                old2 = oldRoot.RequiredSingle.Single;
            }

            using (var context = CreateContext())
            {
                var root = LoadRequiredGraph(context);

                root.RequiredSingle = null;

                context.SaveChanges();
            }

            var new2 = new RequiredSingle2();
            var new1 = new RequiredSingle1 { Single = new2 };

            using (var context = CreateContext())
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

                context.SaveChanges();

                Assert.Equal(root.Id, new1.Id);
                Assert.Equal(new1.Id, new2.Id);
                Assert.Same(root, new1.Root);
                Assert.Same(new1, new2.Back);

                Assert.Same(oldRoot, old1.Root);
                Assert.Same(old1, old2.Back);
                Assert.Equal(old1.Id, old2.Id);

                entries = context.ChangeTracker.Entries().ToList();
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadRequiredGraph(context);

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertKeys(oldRoot, loadedRoot);
                AssertNavigations(loadedRoot);
            }
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
        public virtual void Save_required_non_PK_one_to_one_changed_by_reference(ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            var new2 = new RequiredNonPkSingle2();
            var new2d = new RequiredNonPkSingle2Derived();
            var new2dd = new RequiredNonPkSingle2MoreDerived();
            var new1 = new RequiredNonPkSingle1 { Single = new2 };
            var new1d = new RequiredNonPkSingle1Derived { Single = new2d, Root = new Root() };
            var new1dd = new RequiredNonPkSingle1MoreDerived { Single = new2dd, Root = new Root(), DerivedRoot = new Root() };
            var newRoot = new Root { RequiredNonPkSingle = new1, RequiredNonPkSingleDerived = new1d, RequiredNonPkSingleMoreDerived = new1dd };

            if (useExistingEntities)
            {
                using (var context = CreateContext())
                {
                    context.AddRange(newRoot, new1, new1d, new1dd, new2, new2d, new2dd);
                    context.SaveChanges();
                }
            }

            Root root;
            IReadOnlyList<EntityEntry> entries;
            RequiredNonPkSingle1 old1;
            RequiredNonPkSingle1Derived old1d;
            RequiredNonPkSingle1MoreDerived old1dd;
            RequiredNonPkSingle2 old2;
            RequiredNonPkSingle2Derived old2d;
            RequiredNonPkSingle2MoreDerived old2dd;
            using (var context = CreateContext())
            {
                root = LoadRequiredNonPkGraph(context);

                old1 = root.RequiredNonPkSingle;
                old1d = root.RequiredNonPkSingleDerived;
                old1dd = root.RequiredNonPkSingleMoreDerived;
                old2 = root.RequiredNonPkSingle.Single;
                old2d = (RequiredNonPkSingle2Derived)root.RequiredNonPkSingleDerived.Single;
                old2dd = (RequiredNonPkSingle2MoreDerived)root.RequiredNonPkSingleMoreDerived.Single;

                context.RequiredNonPkSingle1s.Remove(old1d);
                context.RequiredNonPkSingle1s.Remove(old1dd);

                if (useExistingEntities)
                {
                    new1 = context.RequiredNonPkSingle1s.Single(e => e.Id == new1.Id);
                    new1d = (RequiredNonPkSingle1Derived)context.RequiredNonPkSingle1s.Single(e => e.Id == new1d.Id);
                    new1dd = (RequiredNonPkSingle1MoreDerived)context.RequiredNonPkSingle1s.Single(e => e.Id == new1dd.Id);
                    new2 = context.RequiredNonPkSingle2s.Single(e => e.Id == new2.Id);
                    new2d = (RequiredNonPkSingle2Derived)context.RequiredNonPkSingle2s.Single(e => e.Id == new2d.Id);
                    new2dd = (RequiredNonPkSingle2MoreDerived)context.RequiredNonPkSingle2s.Single(e => e.Id == new2dd.Id);

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

                context.SaveChanges();

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

            using (var context = CreateContext())
            {
                var loadedRoot = LoadRequiredNonPkGraph(context);

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);

                Assert.False(context.RequiredNonPkSingle1s.Any(e => e.Id == old1.Id));
                Assert.False(context.RequiredNonPkSingle1s.Any(e => e.Id == old1d.Id));
                Assert.False(context.RequiredNonPkSingle1s.Any(e => e.Id == old1dd.Id));
                Assert.False(context.RequiredNonPkSingle2s.Any(e => e.Id == old2.Id));
                Assert.False(context.RequiredNonPkSingle2s.Any(e => e.Id == old2d.Id));
                Assert.False(context.RequiredNonPkSingle2s.Any(e => e.Id == old2dd.Id));
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)ChangeMechanism.Fk)]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk))]
        [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent))]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk))]
        public virtual void Sever_optional_one_to_one(ChangeMechanism changeMechanism)
        {
            Root root;
            OptionalSingle1 old1;
            OptionalSingle2 old2;
            using (var context = CreateContext())
            {
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

                context.SaveChanges();

                Assert.Null(old1.Root);
                Assert.Same(old1, old2.Back);
                Assert.Null(old1.RootId);
                Assert.Equal(old1.Id, old2.BackId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadOptionalGraph(context);

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
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
        public virtual void Sever_required_one_to_one(ChangeMechanism changeMechanism)
        {
            Root root;
            RequiredSingle1 old1;
            RequiredSingle2 old2;
            using (var context = CreateContext())
            {
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

                context.SaveChanges();

                Assert.Null(old1.Root);
                Assert.Null(old2.Back);
                Assert.Equal(old1.Id, old2.Id);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadRequiredGraph(context);

                AssertKeys(root, loadedRoot);
                AssertPossiblyNullNavigations(loadedRoot);

                Assert.False(context.RequiredSingle1s.Any(e => e.Id == old1.Id));
                Assert.False(context.RequiredSingle2s.Any(e => e.Id == old2.Id));
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
        public virtual void Sever_required_non_PK_one_to_one(ChangeMechanism changeMechanism)
        {
            Root root;
            RequiredNonPkSingle1 old1;
            RequiredNonPkSingle2 old2;
            using (var context = CreateContext())
            {
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

                Assert.False(context.Entry(root).Reference(e => e.RequiredNonPkSingle).IsLoaded);
                Assert.False(context.Entry(old1).Reference(e => e.Root).IsLoaded);

                context.SaveChanges();

                Assert.Null(old1.Root);
                Assert.Null(old2.Back);
                Assert.Equal(old1.Id, old2.BackId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadRequiredNonPkGraph(context);

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
        public virtual void Reparent_optional_one_to_one(ChangeMechanism changeMechanism, bool useExistingRoot)
        {
            var newRoot = new Root();

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
                    old1.RootId = newRoot.Id;
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
                var loadedRoot = LoadOptionalGraph(context);

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
        public virtual void Reparent_required_one_to_one(ChangeMechanism changeMechanism, bool useExistingRoot)
        {
            var newRoot = new Root();

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
                var root = LoadRequiredGraph(context);

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
            }
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
        public virtual void Reparent_required_non_PK_one_to_one(ChangeMechanism changeMechanism, bool useExistingRoot)
        {
            var newRoot = new Root();

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
                    old1.RootId = newRoot.Id;
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
                var loadedRoot = LoadRequiredNonPkGraph(context);

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
        public virtual void Save_optional_many_to_one_dependents_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingEntities)
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

            if (useExistingEntities)
            {
                using (var context = CreateContext())
                {
                    context.AddRange(new1, new1d, new1dd, new2a, new2d, new2dd, new2b, new2ca, new2cb);
                    context.SaveChanges();
                }
            }

            Root root;
            IReadOnlyList<EntityEntry> entries;
            using (var context = CreateContext())
            {
                root = LoadOptionalAkGraph(context);
                var existing = root.OptionalChildrenAk.OrderBy(e => e.Id).First();

                if (useExistingEntities)
                {
                    new1 = context.OptionalAk1s.Single(e => e.Id == new1.Id);
                    new1d = (OptionalAk1Derived)context.OptionalAk1s.Single(e => e.Id == new1d.Id);
                    new1dd = (OptionalAk1MoreDerived)context.OptionalAk1s.Single(e => e.Id == new1dd.Id);
                    new2a = context.OptionalAk2s.Single(e => e.Id == new2a.Id);
                    new2b = context.OptionalAk2s.Single(e => e.Id == new2b.Id);
                    new2ca = context.OptionalComposite2s.Single(e => e.Id == new2ca.Id);
                    new2cb = context.OptionalComposite2s.Single(e => e.Id == new2cb.Id);
                    new2d = (OptionalAk2Derived)context.OptionalAk2s.Single(e => e.Id == new2d.Id);
                    new2dd = (OptionalAk2MoreDerived)context.OptionalAk2s.Single(e => e.Id == new2dd.Id);
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

                context.SaveChanges();

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
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadOptionalAkGraph(context);

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);
            }
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
        public virtual void Save_required_many_to_one_dependents_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingEntities)
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

            if (useExistingEntities)
            {
                using (var context = CreateContext())
                {
                    context.AddRange(newRoot, new1, new1d, new1dd, new2a, new2d, new2dd, new2b, new2ca, new2cb);
                    context.SaveChanges();
                }
            }

            Root root;
            IReadOnlyList<EntityEntry> entries;
            using (var context = CreateContext())
            {
                root = LoadRequiredAkGraph(context);
                var existing = root.RequiredChildrenAk.OrderBy(e => e.Id).First();

                if (useExistingEntities)
                {
                    new1 = context.RequiredAk1s.Single(e => e.Id == new1.Id);
                    new1d = (RequiredAk1Derived)context.RequiredAk1s.Single(e => e.Id == new1d.Id);
                    new1dd = (RequiredAk1MoreDerived)context.RequiredAk1s.Single(e => e.Id == new1dd.Id);
                    new2a = context.RequiredAk2s.Single(e => e.Id == new2a.Id);
                    new2b = context.RequiredAk2s.Single(e => e.Id == new2b.Id);
                    new2ca = context.RequiredComposite2s.Single(e => e.Id == new2ca.Id);
                    new2cb = context.RequiredComposite2s.Single(e => e.Id == new2cb.Id);
                    new2d = (RequiredAk2Derived)context.RequiredAk2s.Single(e => e.Id == new2d.Id);
                    new2dd = (RequiredAk2MoreDerived)context.RequiredAk2s.Single(e => e.Id == new2dd.Id);
                }
                else
                {
                    context.AddRange(new1, new1d, new1dd, new2a, new2d, new2dd, new2b, new2ca, new2cb);
                    context.Entry(newRoot).State = EntityState.Detached;
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

                context.SaveChanges();

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
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadRequiredAkGraph(context);

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.Fk)]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk))]
        [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent))]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk))]
        public virtual void Save_removed_optional_many_to_one_dependents_with_alternate_key(ChangeMechanism changeMechanism)
        {
            Root root;
            using (var context = CreateContext())
            {
                root = LoadOptionalAkGraph(context);

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

                context.SaveChanges();

                Assert.DoesNotContain(removed1, root.OptionalChildrenAk);
                Assert.DoesNotContain(removed2, childCollection);
                Assert.DoesNotContain(removed2c, childCompositeCollection);

                Assert.Null(removed1.Parent);
                Assert.Null(removed2.Parent);
                Assert.Null(removed2c.Parent);

                Assert.Null(removed1.ParentId);
                Assert.Null(removed2.ParentId);
                Assert.Null(removed2c.ParentId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadOptionalAkGraph(context);

                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);

                Assert.Equal(1, loadedRoot.OptionalChildrenAk.Count());
                Assert.Equal(1, loadedRoot.OptionalChildrenAk.First().Children.Count());
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
        public virtual void Save_removed_required_many_to_one_dependents_with_alternate_key(ChangeMechanism changeMechanism)
        {
            Root root;
            RequiredAk2 removed2;
            RequiredComposite2 removed2c;
            RequiredAk1 removed1;

            using (var context = CreateContext())
            {
                root = LoadRequiredAkGraph(context);

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

                context.SaveChanges();

                Assert.DoesNotContain(removed1, root.RequiredChildrenAk);
                Assert.DoesNotContain(removed2, childCollection);
                Assert.DoesNotContain(removed2c, childCompositeCollection);

                Assert.Null(removed1.Parent);
                Assert.Null(removed2.Parent);
                Assert.Null(removed2c.Parent);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadRequiredAkGraph(context);

                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);

                Assert.False(context.RequiredAk1s.Any(e => e.Id == removed1.Id));
                Assert.False(context.RequiredAk2s.Any(e => e.Id == removed2.Id));
                Assert.False(context.RequiredComposite2s.Any(e => e.Id == removed2c.Id));

                Assert.Equal(1, loadedRoot.RequiredChildrenAk.Count());
                Assert.Equal(1, loadedRoot.RequiredChildrenAk.First().Children.Count());
                Assert.Equal(1, loadedRoot.RequiredChildrenAk.First().CompositeChildren.Count());
            }
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
        public virtual void Save_changed_optional_one_to_one_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            var new2 = new OptionalSingleAk2 { AlternateId = Guid.NewGuid() };
            var new2d = new OptionalSingleAk2Derived { AlternateId = Guid.NewGuid() };
            var new2dd = new OptionalSingleAk2MoreDerived { AlternateId = Guid.NewGuid() };
            var new2c = new OptionalSingleComposite2();
            var new1 = new OptionalSingleAk1 { AlternateId = Guid.NewGuid(), Single = new2, SingleComposite = new2c };
            var new1d = new OptionalSingleAk1Derived { AlternateId = Guid.NewGuid(), Single = new2d };
            var new1dd = new OptionalSingleAk1MoreDerived { AlternateId = Guid.NewGuid(), Single = new2dd };

            if (useExistingEntities)
            {
                using (var context = CreateContext())
                {
                    context.AddRange(new1, new1d, new1dd, new2, new2d, new2dd, new2c);
                    context.SaveChanges();
                }
            }

            Root root;
            IReadOnlyList<EntityEntry> entries;
            OptionalSingleAk1 old1;
            OptionalSingleAk1Derived old1d;
            OptionalSingleAk1MoreDerived old1dd;
            OptionalSingleAk2 old2;
            OptionalSingleComposite2 old2c;
            OptionalSingleAk2Derived old2d;
            OptionalSingleAk2MoreDerived old2dd;
            using (var context = CreateContext())
            {
                root = LoadOptionalAkGraph(context);

                old1 = root.OptionalSingleAk;
                old1d = root.OptionalSingleAkDerived;
                old1dd = root.OptionalSingleAkMoreDerived;
                old2 = root.OptionalSingleAk.Single;
                old2c = root.OptionalSingleAk.SingleComposite;
                old2d = (OptionalSingleAk2Derived)root.OptionalSingleAkDerived.Single;
                old2dd = (OptionalSingleAk2MoreDerived)root.OptionalSingleAkMoreDerived.Single;

                if (useExistingEntities)
                {
                    new1 = context.OptionalSingleAk1s.Single(e => e.Id == new1.Id);
                    new1d = (OptionalSingleAk1Derived)context.OptionalSingleAk1s.Single(e => e.Id == new1d.Id);
                    new1dd = (OptionalSingleAk1MoreDerived)context.OptionalSingleAk1s.Single(e => e.Id == new1dd.Id);
                    new2 = context.OptionalSingleAk2s.Single(e => e.Id == new2.Id);
                    new2c = context.OptionalSingleComposite2s.Single(e => e.Id == new2c.Id);
                    new2d = (OptionalSingleAk2Derived)context.OptionalSingleAk2s.Single(e => e.Id == new2d.Id);
                    new2dd = (OptionalSingleAk2MoreDerived)context.OptionalSingleAk2s.Single(e => e.Id == new2dd.Id);
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

                entries = context.ChangeTracker.Entries().ToList();
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadOptionalAkGraph(context);

                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);

                var loaded1 = context.OptionalSingleAk1s.Single(e => e.Id == old1.Id);
                var loaded1d = context.OptionalSingleAk1s.Single(e => e.Id == old1d.Id);
                var loaded1dd = context.OptionalSingleAk1s.Single(e => e.Id == old1dd.Id);
                var loaded2 = context.OptionalSingleAk2s.Single(e => e.Id == old2.Id);
                var loaded2d = context.OptionalSingleAk2s.Single(e => e.Id == old2d.Id);
                var loaded2dd = context.OptionalSingleAk2s.Single(e => e.Id == old2dd.Id);
                var loaded2c = context.OptionalSingleComposite2s.Single(e => e.Id == old2c.Id);

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
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent, false)]
        [InlineData((int)ChangeMechanism.Dependent, true)]
        [InlineData((int)ChangeMechanism.Principal, false)]
        [InlineData((int)ChangeMechanism.Principal, true)]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false)]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true)]
        public virtual void Save_required_one_to_one_changed_by_reference_with_alternate_key(
            ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            var new2 = new RequiredSingleAk2 { AlternateId = Guid.NewGuid() };
            var new2c = new RequiredSingleComposite2();
            var new1 = new RequiredSingleAk1 { AlternateId = Guid.NewGuid(), Single = new2, SingleComposite = new2c };
            var newRoot = new Root { AlternateId = Guid.NewGuid(), RequiredSingleAk = new1 };

            if (useExistingEntities)
            {
                using (var context = CreateContext())
                {
                    context.AddRange(newRoot, new1, new2, new2c);
                    context.SaveChanges();
                }
            }

            Root root;
            IReadOnlyList<EntityEntry> entries;
            RequiredSingleAk1 old1;
            RequiredSingleAk2 old2;
            RequiredSingleComposite2 old2c;
            using (var context = CreateContext())
            {
                root = LoadRequiredAkGraph(context);

                old1 = root.RequiredSingleAk;
                old2 = root.RequiredSingleAk.Single;
                old2c = root.RequiredSingleAk.SingleComposite;

                if (useExistingEntities)
                {
                    new1 = context.RequiredSingleAk1s.Single(e => e.Id == new1.Id);
                    new2 = context.RequiredSingleAk2s.Single(e => e.Id == new2.Id);
                    new2c = context.RequiredSingleComposite2s.Single(e => e.Id == new2c.Id);
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

                context.SaveChanges();

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

                entries = context.ChangeTracker.Entries().ToList();
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadRequiredAkGraph(context);

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);

                Assert.False(context.RequiredSingleAk1s.Any(e => e.Id == old1.Id));
                Assert.False(context.RequiredSingleAk2s.Any(e => e.Id == old2.Id));
                Assert.False(context.RequiredSingleComposite2s.Any(e => e.Id == old2c.Id));
            }
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
        public virtual void Save_required_non_PK_one_to_one_changed_by_reference_with_alternate_key(
            ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            var new2 = new RequiredNonPkSingleAk2 { AlternateId = Guid.NewGuid() };
            var new2d = new RequiredNonPkSingleAk2Derived { AlternateId = Guid.NewGuid() };
            var new2dd = new RequiredNonPkSingleAk2MoreDerived { AlternateId = Guid.NewGuid() };
            var new1 = new RequiredNonPkSingleAk1 { AlternateId = Guid.NewGuid(), Single = new2 };
            var new1d = new RequiredNonPkSingleAk1Derived { AlternateId = Guid.NewGuid(), Single = new2d, Root = new Root() };
            var new1dd = new RequiredNonPkSingleAk1MoreDerived { AlternateId = Guid.NewGuid(), Single = new2dd, Root = new Root(), DerivedRoot = new Root() };
            var newRoot = new Root { AlternateId = Guid.NewGuid(), RequiredNonPkSingleAk = new1, RequiredNonPkSingleAkDerived = new1d, RequiredNonPkSingleAkMoreDerived = new1dd };

            if (useExistingEntities)
            {
                using (var context = CreateContext())
                {
                    context.AddRange(newRoot, new1, new1d, new1dd, new2, new2d, new2dd);
                    context.SaveChanges();
                }
            }

            Root root;
            IReadOnlyList<EntityEntry> entries;
            RequiredNonPkSingleAk1 old1;
            RequiredNonPkSingleAk1Derived old1d;
            RequiredNonPkSingleAk1MoreDerived old1dd;
            RequiredNonPkSingleAk2 old2;
            RequiredNonPkSingleAk2Derived old2d;
            RequiredNonPkSingleAk2MoreDerived old2dd;
            using (var context = CreateContext())
            {
                root = LoadRequiredNonPkAkGraph(context);

                old1 = root.RequiredNonPkSingleAk;
                old1d = root.RequiredNonPkSingleAkDerived;
                old1dd = root.RequiredNonPkSingleAkMoreDerived;
                old2 = root.RequiredNonPkSingleAk.Single;
                old2d = (RequiredNonPkSingleAk2Derived)root.RequiredNonPkSingleAkDerived.Single;
                old2dd = (RequiredNonPkSingleAk2MoreDerived)root.RequiredNonPkSingleAkMoreDerived.Single;

                context.RequiredNonPkSingleAk1s.Remove(old1d);
                context.RequiredNonPkSingleAk1s.Remove(old1dd);

                if (useExistingEntities)
                {
                    new1 = context.RequiredNonPkSingleAk1s.Single(e => e.Id == new1.Id);
                    new1d = (RequiredNonPkSingleAk1Derived)context.RequiredNonPkSingleAk1s.Single(e => e.Id == new1d.Id);
                    new1dd = (RequiredNonPkSingleAk1MoreDerived)context.RequiredNonPkSingleAk1s.Single(e => e.Id == new1dd.Id);
                    new2 = context.RequiredNonPkSingleAk2s.Single(e => e.Id == new2.Id);
                    new2d = (RequiredNonPkSingleAk2Derived)context.RequiredNonPkSingleAk2s.Single(e => e.Id == new2d.Id);
                    new2dd = (RequiredNonPkSingleAk2MoreDerived)context.RequiredNonPkSingleAk2s.Single(e => e.Id == new2dd.Id);

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

                context.SaveChanges();

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

            using (var context = CreateContext())
            {
                var loadedRoot = LoadRequiredNonPkAkGraph(context);

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                AssertKeys(root, loadedRoot);
                AssertNavigations(loadedRoot);

                Assert.False(context.RequiredNonPkSingleAk1s.Any(e => e.Id == old1.Id));
                Assert.False(context.RequiredNonPkSingleAk1s.Any(e => e.Id == old1d.Id));
                Assert.False(context.RequiredNonPkSingleAk1s.Any(e => e.Id == old1dd.Id));
                Assert.False(context.RequiredNonPkSingleAk2s.Any(e => e.Id == old2.Id));
                Assert.False(context.RequiredNonPkSingleAk2s.Any(e => e.Id == old2d.Id));
                Assert.False(context.RequiredNonPkSingleAk2s.Any(e => e.Id == old2dd.Id));
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)ChangeMechanism.Fk)]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk))]
        [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent))]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk))]
        public virtual void Sever_optional_one_to_one_with_alternate_key(ChangeMechanism changeMechanism)
        {
            Root root;
            OptionalSingleAk1 old1;
            OptionalSingleAk2 old2;
            OptionalSingleComposite2 old2c;
            using (var context = CreateContext())
            {
                root = LoadOptionalAkGraph(context);

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

                context.SaveChanges();

                Assert.Null(old1.Root);
                Assert.Same(old1, old2.Back);
                Assert.Same(old1, old2c.Back);
                Assert.Null(old1.RootId);
                Assert.Equal(old1.AlternateId, old2.BackId);
                Assert.Equal(old1.Id, old2c.BackId);
                Assert.Equal(old1.AlternateId, old2c.ParentAlternateId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadOptionalAkGraph(context);

                AssertKeys(root, loadedRoot);
                AssertPossiblyNullNavigations(loadedRoot);

                var loaded1 = context.OptionalSingleAk1s.Single(e => e.Id == old1.Id);
                var loaded2 = context.OptionalSingleAk2s.Single(e => e.Id == old2.Id);
                var loaded2c = context.OptionalSingleComposite2s.Single(e => e.Id == old2c.Id);

                Assert.Null(loaded1.Root);
                Assert.Same(loaded1, loaded2.Back);
                Assert.Same(loaded1, loaded2c.Back);
                Assert.Null(loaded1.RootId);
                Assert.Equal(loaded1.AlternateId, loaded2.BackId);
                Assert.Equal(loaded1.Id, loaded2c.BackId);
                Assert.Equal(loaded1.AlternateId, loaded2c.ParentAlternateId);
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
        public virtual void Sever_required_one_to_one_with_alternate_key(ChangeMechanism changeMechanism)
        {
            Root root;
            RequiredSingleAk1 old1;
            RequiredSingleAk2 old2;
            RequiredSingleComposite2 old2c;
            using (var context = CreateContext())
            {
                root = LoadRequiredAkGraph(context);

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

                Assert.False(context.Entry(root).Reference(e => e.RequiredSingleAk).IsLoaded);
                Assert.False(context.Entry(old1).Reference(e => e.Root).IsLoaded);

                context.SaveChanges();

                Assert.Null(old1.Root);
                Assert.Null(old2.Back);
                Assert.Null(old2c.Back);
                Assert.Equal(old1.AlternateId, old2.BackId);
                Assert.Equal(old1.Id, old2c.BackId);
                Assert.Equal(old1.AlternateId, old2c.BackAlternateId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadRequiredAkGraph(context);

                AssertKeys(root, loadedRoot);
                AssertPossiblyNullNavigations(loadedRoot);

                Assert.False(context.RequiredSingleAk1s.Any(e => e.Id == old1.Id));
                Assert.False(context.RequiredSingleAk2s.Any(e => e.Id == old2.Id));
                Assert.False(context.RequiredSingleComposite2s.Any(e => e.Id == old2c.Id));
            }
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
        public virtual void Sever_required_non_PK_one_to_one_with_alternate_key(ChangeMechanism changeMechanism)
        {
            Root root;
            RequiredNonPkSingleAk1 old1;
            RequiredNonPkSingleAk2 old2;
            using (var context = CreateContext())
            {
                root = LoadRequiredNonPkAkGraph(context);

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

                Assert.False(context.Entry(root).Reference(e => e.RequiredNonPkSingleAk).IsLoaded);
                Assert.False(context.Entry(old1).Reference(e => e.Root).IsLoaded);

                context.SaveChanges();

                Assert.Null(old1.Root);
                Assert.Null(old2.Back);
                Assert.Equal(old1.AlternateId, old2.BackId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadRequiredNonPkAkGraph(context);

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
        public virtual void Reparent_optional_one_to_one_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingRoot)
        {
            var newRoot = new Root { AlternateId = Guid.NewGuid() };

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
            OptionalSingleComposite2 old2c;
            using (var context = CreateContext())
            {
                root = LoadOptionalAkGraph(context);

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

                context.SaveChanges();

                Assert.Null(root.OptionalSingleAk);

                Assert.Same(newRoot, old1.Root);
                Assert.Same(old1, old2.Back);
                Assert.Same(old1, old2c.Back);
                Assert.Equal(newRoot.AlternateId, old1.RootId);
                Assert.Equal(old1.AlternateId, old2.BackId);
                Assert.Equal(old1.Id, old2c.BackId);
                Assert.Equal(old1.AlternateId, old2c.ParentAlternateId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadOptionalAkGraph(context);

                AssertKeys(root, loadedRoot);
                AssertPossiblyNullNavigations(loadedRoot);

                newRoot = context.Roots.Single(e => e.Id == newRoot.Id);
                var loaded1 = context.OptionalSingleAk1s.Single(e => e.Id == old1.Id);
                var loaded2 = context.OptionalSingleAk2s.Single(e => e.Id == old2.Id);
                var loaded2c = context.OptionalSingleComposite2s.Single(e => e.Id == old2c.Id);

                Assert.Same(newRoot, loaded1.Root);
                Assert.Same(loaded1, loaded2.Back);
                Assert.Same(loaded1, loaded2c.Back);
                Assert.Equal(newRoot.AlternateId, loaded1.RootId);
                Assert.Equal(loaded1.AlternateId, loaded2.BackId);
                Assert.Equal(loaded1.Id, loaded2c.BackId);
                Assert.Equal(loaded1.AlternateId, loaded2c.ParentAlternateId);
            }
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
        public virtual void Reparent_required_one_to_one_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingRoot)
        {
            var newRoot = new Root { AlternateId = Guid.NewGuid() };

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
            RequiredSingleComposite2 old2c;
            using (var context = CreateContext())
            {
                root = LoadRequiredAkGraph(context);

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

                context.SaveChanges();

                Assert.Null(root.RequiredSingleAk);

                Assert.Same(newRoot, old1.Root);
                Assert.Same(old1, old2.Back);
                Assert.Same(old1, old2c.Back);
                Assert.Equal(newRoot.AlternateId, old1.RootId);
                Assert.Equal(old1.AlternateId, old2.BackId);
                Assert.Equal(old1.Id, old2c.BackId);
                Assert.Equal(old1.AlternateId, old2c.BackAlternateId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadRequiredAkGraph(context);

                AssertKeys(root, loadedRoot);
                AssertPossiblyNullNavigations(loadedRoot);

                newRoot = context.Roots.Single(e => e.Id == newRoot.Id);
                var loaded1 = context.RequiredSingleAk1s.Single(e => e.Id == old1.Id);
                var loaded2 = context.RequiredSingleAk2s.Single(e => e.Id == old2.Id);
                var loaded2c = context.RequiredSingleComposite2s.Single(e => e.Id == old2c.Id);

                Assert.Same(newRoot, loaded1.Root);
                Assert.Same(loaded1, loaded2.Back);
                Assert.Same(loaded1, loaded2c.Back);
                Assert.Equal(newRoot.AlternateId, loaded1.RootId);
                Assert.Equal(loaded1.AlternateId, loaded2.BackId);
                Assert.Equal(loaded1.Id, loaded2c.BackId);
                Assert.Equal(loaded1.AlternateId, loaded2c.BackAlternateId);
            }
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
        public virtual void Reparent_required_non_PK_one_to_one_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingRoot)
        {
            var newRoot = new Root { AlternateId = Guid.NewGuid() };

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
                root = LoadRequiredNonPkAkGraph(context);

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

                context.SaveChanges();

                Assert.Null(root.RequiredNonPkSingleAk);

                Assert.Same(newRoot, old1.Root);
                Assert.Same(old1, old2.Back);
                Assert.Equal(newRoot.AlternateId, old1.RootId);
                Assert.Equal(old1.AlternateId, old2.BackId);
            }

            using (var context = CreateContext())
            {
                var loadedRoot = LoadRequiredNonPkAkGraph(context);

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
                var root = LoadRequiredGraph(context);

                Assert.Equal(2, root.RequiredChildren.Count());

                var removed = root.RequiredChildren.First();

                removedId = removed.Id;
                var cascadeRemoved = removed.Children.ToList();
                orphanedIds = cascadeRemoved.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Detached));

                Assert.Equal(1, root.RequiredChildren.Count());
                Assert.DoesNotContain(removedId, root.RequiredChildren.Select(e => e.Id));

                Assert.Empty(context.Required1s.Where(e => e.Id == removedId));
                Assert.Empty(context.Required2s.Where(e => orphanedIds.Contains(e.Id)));
            }

            using (var context = CreateContext())
            {
                var root = LoadRequiredGraph(context);

                Assert.Equal(1, root.RequiredChildren.Count());
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
                var root = LoadOptionalGraph(context);

                Assert.Equal(2, root.OptionalChildren.Count());

                var removed = root.OptionalChildren.First();

                removedId = removed.Id;
                var orphaned = removed.Children.ToList();
                orphanedIds = orphaned.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));

                Assert.Equal(1, root.OptionalChildren.Count());
                Assert.DoesNotContain(removedId, root.OptionalChildren.Select(e => e.Id));

                Assert.Empty(context.Optional1s.Where(e => e.Id == removedId));
                Assert.Equal(orphanedIds.Count, context.Optional2s.Count(e => orphanedIds.Contains(e.Id)));
            }

            using (var context = CreateContext())
            {
                var root = LoadOptionalGraph(context);

                Assert.Equal(1, root.OptionalChildren.Count());
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
                var root = LoadOptionalGraph(context);

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
                var root = LoadOptionalGraph(context);

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
                var root = LoadRequiredGraph(context);

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
                var root = LoadRequiredGraph(context);

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
                var root = LoadRequiredNonPkGraph(context);

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
                var root = LoadRequiredNonPkGraph(context);

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
                var root = LoadOptionalAkGraph(context);

                Assert.Equal(2, root.OptionalChildrenAk.Count());

                var removed = root.OptionalChildrenAk.First();

                removedId = removed.Id;
                var orphaned = removed.Children.ToList();
                orphanedIds = orphaned.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));

                Assert.Equal(1, root.OptionalChildrenAk.Count());
                Assert.DoesNotContain(removedId, root.OptionalChildrenAk.Select(e => e.Id));

                Assert.Empty(context.OptionalAk1s.Where(e => e.Id == removedId));
                Assert.Equal(orphanedIds.Count, context.OptionalAk2s.Count(e => orphanedIds.Contains(e.Id)));
            }

            using (var context = CreateContext())
            {
                var root = LoadOptionalAkGraph(context);

                Assert.Equal(1, root.OptionalChildrenAk.Count());
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
            List<int> orphanedIdCs;

            using (var context = CreateContext())
            {
                var root = LoadRequiredAkGraph(context);

                Assert.Equal(2, root.RequiredChildrenAk.Count());

                var removed = root.RequiredChildrenAk.First();

                removedId = removed.Id;
                var cascadeRemoved = removed.Children.ToList();
                var cascadeRemovedC = removed.CompositeChildren.ToList();
                orphanedIds = cascadeRemoved.Select(e => e.Id).ToList();
                orphanedIdCs = cascadeRemovedC.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
                Assert.Equal(2, orphanedIdCs.Count);

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Detached));
                Assert.True(cascadeRemovedC.All(e => context.Entry(e).State == EntityState.Detached));

                Assert.Equal(1, root.RequiredChildrenAk.Count());
                Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                Assert.Empty(context.RequiredAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredAk2s.Where(e => orphanedIds.Contains(e.Id)));
            }

            using (var context = CreateContext())
            {
                var root = LoadRequiredAkGraph(context);

                Assert.Equal(1, root.RequiredChildrenAk.Count());
                Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                Assert.Empty(context.RequiredAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredAk2s.Where(e => orphanedIds.Contains(e.Id)));
                Assert.Empty(context.RequiredComposite2s.Where(e => orphanedIdCs.Contains(e.Id)));
            }
        }

        [ConditionalFact]
        public virtual void Optional_one_to_one_with_alternate_key_are_orphaned()
        {
            int removedId;
            int orphanedId;
            int orphanedIdC;

            using (var context = CreateContext())
            {
                var root = LoadOptionalAkGraph(context);

                var removed = root.OptionalSingleAk;

                removedId = removed.Id;
                var orphaned = removed.Single;
                var orphanedC = removed.SingleComposite;
                orphanedId = orphaned.Id;
                orphanedIdC = orphanedC.Id;

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(orphanedC).State);

                Assert.Null(root.OptionalSingleAk);

                Assert.Empty(context.OptionalSingleAk1s.Where(e => e.Id == removedId));
                Assert.Equal(1, context.OptionalSingleAk2s.Count(e => e.Id == orphanedId));
                Assert.Equal(1, context.OptionalSingleComposite2s.Count(e => e.Id == orphanedIdC));
            }

            using (var context = CreateContext())
            {
                var root = LoadOptionalAkGraph(context);

                Assert.Null(root.OptionalSingleAk);

                Assert.Empty(context.OptionalSingleAk1s.Where(e => e.Id == removedId));
                Assert.Equal(1, context.OptionalSingleAk2s.Count(e => e.Id == orphanedId));
                Assert.Equal(1, context.OptionalSingleComposite2s.Count(e => e.Id == orphanedIdC));
            }
        }

        [ConditionalFact]
        public virtual void Required_one_to_one_with_alternate_key_are_cascade_deleted()
        {
            int removedId;
            int orphanedId;
            int orphanedIdC;

            using (var context = CreateContext())
            {
                var root = LoadRequiredAkGraph(context);

                var removed = root.RequiredSingleAk;

                removedId = removed.Id;
                var orphaned = removed.Single;
                var orphanedC = removed.SingleComposite;
                orphanedId = orphaned.Id;
                orphanedIdC = orphanedC.Id;

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);
                Assert.Equal(EntityState.Detached, context.Entry(orphanedC).State);

                Assert.Null(root.RequiredSingleAk);

                Assert.Empty(context.RequiredSingleAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredSingleAk2s.Where(e => e.Id == orphanedId));
                Assert.Empty(context.RequiredSingleComposite2s.Where(e => e.Id == orphanedIdC));
            }

            using (var context = CreateContext())
            {
                var root = LoadRequiredAkGraph(context);

                Assert.Null(root.RequiredSingleAk);

                Assert.Empty(context.RequiredSingleAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredSingleAk2s.Where(e => e.Id == orphanedId));
                Assert.Empty(context.RequiredSingleComposite2s.Where(e => e.Id == orphanedIdC));
            }
        }

        [ConditionalFact]
        public virtual void Required_non_PK_one_to_one_with_alternate_key_are_cascade_deleted()
        {
            int removedId;
            int orphanedId;

            using (var context = CreateContext())
            {
                var root = LoadRequiredNonPkAkGraph(context);

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
                var root = LoadRequiredNonPkAkGraph(context);

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
                var removed = LoadRequiredGraph(context).RequiredChildren.First();

                removedId = removed.Id;
                orphanedIds = removed.Children.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
            }

            using (var context = CreateContext())
            {
                var root = context.Roots.Include(e => e.RequiredChildren).Single(IsTheRoot);

                var removed = root.RequiredChildren.Single(e => e.Id == removedId);

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Equal(1, root.RequiredChildren.Count());
                Assert.DoesNotContain(removedId, root.RequiredChildren.Select(e => e.Id));

                Assert.Empty(context.Required1s.Where(e => e.Id == removedId));
                Assert.Empty(context.Required2s.Where(e => orphanedIds.Contains(e.Id)));
            }

            using (var context = CreateContext())
            {
                var root = LoadRequiredGraph(context);

                Assert.Equal(1, root.RequiredChildren.Count());
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
                var removed = LoadRequiredGraph(context).RequiredSingle;

                removedId = removed.Id;
                orphanedId = removed.Single.Id;
            }

            using (var context = CreateContext())
            {
                var root = context.Roots.Include(e => e.RequiredSingle).Single(IsTheRoot);

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
                var root = LoadRequiredGraph(context);

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
                var removed = LoadRequiredNonPkGraph(context).RequiredNonPkSingle;

                removedId = removed.Id;
                orphanedId = removed.Single.Id;
            }

            using (var context = CreateContext())
            {
                var root = context.Roots.Include(e => e.RequiredNonPkSingle).Single(IsTheRoot);

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
                var root = LoadRequiredNonPkGraph(context);

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
            List<int> orphanedIdCs;

            using (var context = CreateContext())
            {
                var removed = LoadRequiredAkGraph(context).RequiredChildrenAk.First();

                removedId = removed.Id;
                orphanedIds = removed.Children.Select(e => e.Id).ToList();
                orphanedIdCs = removed.CompositeChildren.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
                Assert.Equal(2, orphanedIdCs.Count);
            }

            using (var context = CreateContext())
            {
                var root = context.Roots.Include(e => e.RequiredChildrenAk).Single(IsTheRoot);

                var removed = root.RequiredChildrenAk.Single(e => e.Id == removedId);

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Equal(1, root.RequiredChildrenAk.Count());
                Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                Assert.Empty(context.RequiredAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredAk2s.Where(e => orphanedIds.Contains(e.Id)));
                Assert.Empty(context.RequiredComposite2s.Where(e => orphanedIdCs.Contains(e.Id)));
            }

            using (var context = CreateContext())
            {
                var root = LoadRequiredAkGraph(context);

                Assert.Equal(1, root.RequiredChildrenAk.Count());
                Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                Assert.Empty(context.RequiredAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredAk2s.Where(e => orphanedIds.Contains(e.Id)));
                Assert.Empty(context.RequiredComposite2s.Where(e => orphanedIdCs.Contains(e.Id)));
            }
        }

        [ConditionalFact]
        public virtual void Required_one_to_one_with_alternate_key_are_cascade_deleted_in_store()
        {
            int removedId;
            int orphanedId;
            int orphanedIdC;

            using (var context = CreateContext())
            {
                var removed = LoadRequiredAkGraph(context).RequiredSingleAk;

                removedId = removed.Id;
                orphanedId = removed.Single.Id;
                orphanedIdC = removed.SingleComposite.Id;
            }

            using (var context = CreateContext())
            {
                var root = context.Roots.Include(e => e.RequiredSingleAk).Single(IsTheRoot);

                var removed = root.RequiredSingleAk;

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Null(root.RequiredSingleAk);

                Assert.Empty(context.RequiredSingleAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredSingleAk2s.Where(e => e.Id == orphanedId));
                Assert.Empty(context.RequiredSingleComposite2s.Where(e => e.Id == orphanedIdC));
            }

            using (var context = CreateContext())
            {
                var root = LoadRequiredAkGraph(context);

                Assert.Null(root.RequiredSingleAk);

                Assert.Empty(context.RequiredSingleAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredSingleAk2s.Where(e => e.Id == orphanedId));
                Assert.Empty(context.RequiredSingleComposite2s.Where(e => e.Id == orphanedIdC));
            }
        }

        [ConditionalFact]
        public virtual void Required_non_PK_one_to_one_with_alternate_key_are_cascade_deleted_in_store()
        {
            int removedId;
            int orphanedId;

            using (var context = CreateContext())
            {
                var removed = LoadRequiredNonPkAkGraph(context).RequiredNonPkSingleAk;

                removedId = removed.Id;
                orphanedId = removed.Single.Id;
            }

            using (var context = CreateContext())
            {
                var root = context.Roots.Include(e => e.RequiredNonPkSingleAk).Single(IsTheRoot);

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
                var root = LoadRequiredNonPkAkGraph(context);

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
                var removed = LoadOptionalGraph(context).OptionalChildren.First();

                removedId = removed.Id;
                orphanedIds = removed.Children.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
            }

            using (var context = CreateContext())
            {
                var root = context.Roots.Include(e => e.OptionalChildren).Single(IsTheRoot);

                var removed = root.OptionalChildren.First(e => e.Id == removedId);

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Equal(1, root.OptionalChildren.Count());
                Assert.DoesNotContain(removedId, root.OptionalChildren.Select(e => e.Id));

                Assert.Empty(context.Optional1s.Where(e => e.Id == removedId));

                var orphaned = context.Optional2s.Where(e => orphanedIds.Contains(e.Id)).ToList();
                Assert.Equal(orphanedIds.Count, orphaned.Count);
                Assert.True(orphaned.All(e => e.ParentId == null));
            }

            using (var context = CreateContext())
            {
                var root = LoadOptionalGraph(context);

                Assert.Equal(1, root.OptionalChildren.Count());
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
                var removed = LoadOptionalGraph(context).OptionalSingle;

                removedId = removed.Id;
                orphanedId = removed.Single.Id;
            }

            using (var context = CreateContext())
            {
                var root = context.Roots.Include(e => e.OptionalSingle).Single(IsTheRoot);

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
                var root = LoadOptionalGraph(context);

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
            List<int> orphanedIdCs;

            using (var context = CreateContext())
            {
                var removed = LoadOptionalAkGraph(context).OptionalChildrenAk.First();

                removedId = removed.Id;
                orphanedIds = removed.Children.Select(e => e.Id).ToList();
                orphanedIdCs = removed.CompositeChildren.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
                Assert.Equal(2, orphanedIdCs.Count);
            }

            using (var context = CreateContext())
            {
                var root = context.Roots.Include(e => e.OptionalChildrenAk).Single(IsTheRoot);

                var removed = root.OptionalChildrenAk.First(e => e.Id == removedId);

                context.Remove(removed);

                // Cannot have SET NULL action in the store because one of the FK columns
                // is not nullable, so need to do this on the EF side.
                foreach (var toOrphan in context.OptionalComposite2s.Where(e => orphanedIdCs.Contains(e.Id)).ToList())
                {
                    toOrphan.ParentId = null;
                }

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Equal(1, root.OptionalChildrenAk.Count());
                Assert.DoesNotContain(removedId, root.OptionalChildrenAk.Select(e => e.Id));

                Assert.Empty(context.OptionalAk1s.Where(e => e.Id == removedId));

                var orphaned = context.OptionalAk2s.Where(e => orphanedIds.Contains(e.Id)).ToList();
                Assert.Equal(orphanedIds.Count, orphaned.Count);
                Assert.True(orphaned.All(e => e.ParentId == null));

                var orphanedC = context.OptionalComposite2s.Where(e => orphanedIdCs.Contains(e.Id)).ToList();
                Assert.Equal(orphanedIdCs.Count, orphanedC.Count);
                Assert.True(orphanedC.All(e => e.ParentId == null));
            }

            using (var context = CreateContext())
            {
                var root = LoadOptionalAkGraph(context);

                Assert.Equal(1, root.OptionalChildrenAk.Count());
                Assert.DoesNotContain(removedId, root.OptionalChildrenAk.Select(e => e.Id));

                Assert.Empty(context.OptionalAk1s.Where(e => e.Id == removedId));

                var orphaned = context.OptionalAk2s.Where(e => orphanedIds.Contains(e.Id)).ToList();
                Assert.Equal(orphanedIds.Count, orphaned.Count);
                Assert.True(orphaned.All(e => e.ParentId == null));

                var orphanedC = context.OptionalComposite2s.Where(e => orphanedIdCs.Contains(e.Id)).ToList();
                Assert.Equal(orphanedIdCs.Count, orphanedC.Count);
                Assert.True(orphanedC.All(e => e.ParentId == null));
            }
        }

        [ConditionalFact]
        public virtual void Optional_one_to_one_with_alternate_key_are_orphaned_in_store()
        {
            int removedId;
            int orphanedId;
            int orphanedIdC;

            using (var context = CreateContext())
            {
                var removed = LoadOptionalAkGraph(context).OptionalSingleAk;

                removedId = removed.Id;
                orphanedId = removed.Single.Id;
                orphanedIdC = removed.SingleComposite.Id;
            }

            using (var context = CreateContext())
            {
                var root = context.Roots.Include(e => e.OptionalSingleAk).Single(IsTheRoot);

                var removed = root.OptionalSingleAk;

                context.Remove(removed);

                // Cannot have SET NULL action in the store because one of the FK columns
                // is not nullable, so need to do this on the EF side.
                context.OptionalSingleComposite2s.Single(e => e.Id == orphanedIdC).BackId = null;

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                Assert.Null(root.OptionalSingleAk);

                Assert.Empty(context.OptionalSingleAk1s.Where(e => e.Id == removedId));
                Assert.Null(context.OptionalSingleAk2s.Single(e => e.Id == orphanedId).BackId);
                Assert.Null(context.OptionalSingleComposite2s.Single(e => e.Id == orphanedIdC).BackId);
            }

            using (var context = CreateContext())
            {
                var root = LoadOptionalAkGraph(context);

                Assert.Null(root.OptionalSingleAk);

                Assert.Empty(context.OptionalSingleAk1s.Where(e => e.Id == removedId));
                Assert.Null(context.OptionalSingleAk2s.Single(e => e.Id == orphanedId).BackId);
                Assert.Null(context.OptionalSingleComposite2s.Single(e => e.Id == orphanedIdC).BackId);
            }
        }

        [ConditionalFact]
        public virtual void Required_many_to_one_dependents_are_cascade_deleted_starting_detached()
        {
            int removedId;
            List<int> orphanedIds;
            Root root;

            using (var context = CreateContext())
            {
                root = LoadRequiredGraph(context);

                Assert.Equal(2, root.RequiredChildren.Count());
            }

            using (var context = CreateContext())
            {
                var removed = root.RequiredChildren.First();

                removedId = removed.Id;
                var cascadeRemoved = removed.Children.ToList();
                orphanedIds = cascadeRemoved.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);
                Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Unchanged));

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Detached));
            }

            using (var context = CreateContext())
            {
                root = LoadRequiredGraph(context);

                Assert.Equal(1, root.RequiredChildren.Count());
                Assert.DoesNotContain(removedId, root.RequiredChildren.Select(e => e.Id));

                Assert.Empty(context.Required1s.Where(e => e.Id == removedId));
                Assert.Empty(context.Required2s.Where(e => orphanedIds.Contains(e.Id)));
            }
        }

        [ConditionalFact]
        public virtual void Optional_many_to_one_dependents_are_orphaned_starting_detached()
        {
            int removedId;
            List<int> orphanedIds;
            Root root;

            using (var context = CreateContext())
            {
                root = LoadOptionalGraph(context);

                Assert.Equal(2, root.OptionalChildren.Count());
            }

            using (var context = CreateContext())
            {
                var removed = root.OptionalChildren.First();

                removedId = removed.Id;
                var orphaned = removed.Children.ToList();
                orphanedIds = orphaned.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);
                Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));
            }

            using (var context = CreateContext())
            {
                root = LoadOptionalGraph(context);

                Assert.Equal(1, root.OptionalChildren.Count());
                Assert.DoesNotContain(removedId, root.OptionalChildren.Select(e => e.Id));

                Assert.Empty(context.Optional1s.Where(e => e.Id == removedId));
                Assert.Equal(orphanedIds.Count, context.Optional2s.Count(e => orphanedIds.Contains(e.Id)));
            }
        }

        [ConditionalFact]
        public virtual void Optional_one_to_one_are_orphaned_starting_detached()
        {
            int removedId;
            int orphanedId;
            Root root;

            using (var context = CreateContext())
            {
                root = LoadOptionalGraph(context);
            }

            using (var context = CreateContext())
            {
                var removed = root.OptionalSingle;

                removedId = removed.Id;
                var orphaned = removed.Single;
                orphanedId = orphaned.Id;

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);
            }

            using (var context = CreateContext())
            {
                root = LoadOptionalGraph(context);

                Assert.Null(root.OptionalSingle);

                Assert.Empty(context.OptionalSingle1s.Where(e => e.Id == removedId));
                Assert.Equal(1, context.OptionalSingle2s.Count(e => e.Id == orphanedId));
            }
        }

        [ConditionalFact]
        public virtual void Required_one_to_one_are_cascade_deleted_starting_detached()
        {
            int removedId;
            int orphanedId;
            Root root;

            using (var context = CreateContext())
            {
                root = LoadRequiredGraph(context);
            }

            using (var context = CreateContext())
            {
                var removed = root.RequiredSingle;

                removedId = removed.Id;
                var orphaned = removed.Single;
                orphanedId = orphaned.Id;

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);
            }

            using (var context = CreateContext())
            {
                root = LoadRequiredGraph(context);
            }

            using (var context = CreateContext())
            {
                Assert.Null(root.RequiredSingle);

                Assert.Empty(context.RequiredSingle1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredSingle2s.Where(e => e.Id == orphanedId));
            }
        }

        [ConditionalFact]
        public virtual void Required_non_PK_one_to_one_are_cascade_deleted_starting_detached()
        {
            int removedId;
            int orphanedId;
            Root root;

            using (var context = CreateContext())
            {
                root = LoadRequiredNonPkGraph(context);
            }

            using (var context = CreateContext())
            {
                var removed = root.RequiredNonPkSingle;

                removedId = removed.Id;
                var orphaned = removed.Single;
                orphanedId = orphaned.Id;

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);
            }

            using (var context = CreateContext())
            {
                root = LoadRequiredNonPkGraph(context);

                Assert.Null(root.RequiredNonPkSingle);

                Assert.Empty(context.RequiredNonPkSingle1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredNonPkSingle2s.Where(e => e.Id == orphanedId));
            }
        }

        [ConditionalFact]
        public virtual void Optional_many_to_one_dependents_with_alternate_key_are_orphaned_starting_detached()
        {
            int removedId;
            List<int> orphanedIds;
            List<int> orphanedIdCs;
            Root root;

            using (var context = CreateContext())
            {
                root = LoadOptionalAkGraph(context);

                Assert.Equal(2, root.OptionalChildrenAk.Count());
            }

            using (var context = CreateContext())
            {
                var removed = root.OptionalChildrenAk.First();

                removedId = removed.Id;
                var orphaned = removed.Children.ToList();
                var orphanedC = removed.CompositeChildren.ToList();
                orphanedIds = orphaned.Select(e => e.Id).ToList();
                orphanedIdCs = orphanedC.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);
                Assert.Equal(2, orphanedIdCs.Count);

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);
                Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));
                Assert.True(orphanedC.All(e => context.Entry(e).State == EntityState.Unchanged));

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));
                Assert.True(orphanedC.All(e => context.Entry(e).State == EntityState.Unchanged));
            }

            using (var context = CreateContext())
            {
                root = LoadOptionalAkGraph(context);

                Assert.Equal(1, root.OptionalChildrenAk.Count());
                Assert.DoesNotContain(removedId, root.OptionalChildrenAk.Select(e => e.Id));

                Assert.Empty(context.OptionalAk1s.Where(e => e.Id == removedId));
                Assert.Equal(orphanedIds.Count, context.OptionalAk2s.Count(e => orphanedIds.Contains(e.Id)));
                Assert.Equal(orphanedIdCs.Count, context.OptionalComposite2s.Count(e => orphanedIdCs.Contains(e.Id)));
            }
        }

        [ConditionalFact]
        public virtual void Required_many_to_one_dependents_with_alternate_key_are_cascade_deleted_starting_detached()
        {
            int removedId;
            List<int> orphanedIds;
            List<int> orphanedIdCs;
            Root root;

            using (var context = CreateContext())
            {
                root = LoadRequiredAkGraph(context);

                Assert.Equal(2, root.RequiredChildrenAk.Count());
            }

            using (var context = CreateContext())
            {
                var removed = root.RequiredChildrenAk.First();

                removedId = removed.Id;
                var cascadeRemoved = removed.Children.ToList();
                var cascadeRemovedC = removed.CompositeChildren.ToList();
                orphanedIds = cascadeRemoved.Select(e => e.Id).ToList();
                orphanedIdCs = cascadeRemovedC.Select(e => e.Id).ToList();

                Assert.Equal(2, orphanedIds.Count);

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);
                Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Unchanged));
                Assert.True(cascadeRemovedC.All(e => context.Entry(e).State == EntityState.Unchanged));

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Detached));
                Assert.True(cascadeRemovedC.All(e => context.Entry(e).State == EntityState.Detached));
            }

            using (var context = CreateContext())
            {
                root = LoadRequiredAkGraph(context);

                Assert.Equal(1, root.RequiredChildrenAk.Count());
                Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                Assert.Empty(context.RequiredAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredAk2s.Where(e => orphanedIds.Contains(e.Id)));
                Assert.Empty(context.RequiredComposite2s.Where(e => orphanedIdCs.Contains(e.Id)));
            }
        }

        [ConditionalFact]
        public virtual void Optional_one_to_one_with_alternate_key_are_orphaned_starting_detached()
        {
            int removedId;
            int orphanedId;
            int orphanedIdC;
            Root root;

            using (var context = CreateContext())
            {
                root = LoadOptionalAkGraph(context);
            }

            using (var context = CreateContext())
            {
                var removed = root.OptionalSingleAk;

                removedId = removed.Id;
                var orphaned = removed.Single;
                var orphanedC = removed.SingleComposite;
                orphanedId = orphaned.Id;
                orphanedIdC = orphanedC.Id;

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(orphanedC).State);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(orphanedC).State);
            }

            using (var context = CreateContext())
            {
                root = LoadOptionalAkGraph(context);

                Assert.Null(root.OptionalSingleAk);

                Assert.Empty(context.OptionalSingleAk1s.Where(e => e.Id == removedId));
                Assert.Equal(1, context.OptionalSingleAk2s.Count(e => e.Id == orphanedId));
                Assert.Equal(1, context.OptionalSingleComposite2s.Count(e => e.Id == orphanedIdC));
            }
        }

        [ConditionalFact]
        public virtual void Required_one_to_one_with_alternate_key_are_cascade_deleted_starting_detached()
        {
            int removedId;
            int orphanedId;
            int orphanedIdC;
            Root root;

            using (var context = CreateContext())
            {
                root = LoadRequiredAkGraph(context);
            }

            using (var context = CreateContext())
            {
                var removed = root.RequiredSingleAk;

                removedId = removed.Id;
                var orphaned = removed.Single;
                var orphanedC = removed.SingleComposite;
                orphanedId = orphaned.Id;
                orphanedIdC = orphanedC.Id;

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(orphanedC).State);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);
                Assert.Equal(EntityState.Detached, context.Entry(orphanedC).State);
            }

            using (var context = CreateContext())
            {
                root = LoadRequiredAkGraph(context);

                Assert.Null(root.RequiredSingleAk);

                Assert.Empty(context.RequiredSingleAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredSingleAk2s.Where(e => e.Id == orphanedId));
                Assert.Empty(context.RequiredSingleComposite2s.Where(e => e.Id == orphanedIdC));
            }
        }

        [ConditionalFact]
        public virtual void Required_non_PK_one_to_one_with_alternate_key_are_cascade_deleted_starting_detached()
        {
            int removedId;
            int orphanedId;
            Root root;

            using (var context = CreateContext())
            {
                root = LoadRequiredNonPkAkGraph(context);
            }

            using (var context = CreateContext())
            {
                var removed = root.RequiredNonPkSingleAk;

                removedId = removed.Id;
                var orphaned = removed.Single;
                orphanedId = orphaned.Id;

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);
            }

            using (var context = CreateContext())
            {
                root = LoadRequiredNonPkAkGraph(context);

                Assert.Null(root.RequiredNonPkSingleAk);

                Assert.Empty(context.RequiredNonPkSingleAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredNonPkSingleAk2s.Where(e => e.Id == orphanedId));
            }
        }

        [ConditionalFact]
        public virtual void Required_many_to_one_dependents_are_cascade_detached_when_Added()
        {
            int removedId;
            List<int> orphanedIds;

            using (var context = CreateContext())
            {
                var root = LoadRequiredGraph(context);

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
                Assert.Equal(EntityState.Added, context.Entry(added).State);
                Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Unchanged));

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.Equal(EntityState.Detached, context.Entry(added).State);
                Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Detached));
            }

            using (var context = CreateContext())
            {
                var root = LoadRequiredGraph(context);

                Assert.Equal(1, root.RequiredChildren.Count());
                Assert.DoesNotContain(removedId, root.RequiredChildren.Select(e => e.Id));

                Assert.Empty(context.Required1s.Where(e => e.Id == removedId));
                Assert.Empty(context.Required2s.Where(e => orphanedIds.Contains(e.Id)));
            }
        }

        [ConditionalFact]
        public virtual void Required_one_to_one_are_cascade_detached_when_Added()
        {
            int removedId;
            int orphanedId;

            using (var context = CreateContext())
            {
                var root = LoadRequiredGraph(context);

                var removed = root.RequiredSingle;

                removedId = removed.Id;
                var orphaned = removed.Single;
                orphanedId = orphaned.Id;

                context.Entry(orphaned).State = EntityState.Added;

                Assert.Equal(EntityState.Unchanged, context.Entry(removed).State);
                Assert.Equal(EntityState.Added, context.Entry(orphaned).State);

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);
                Assert.Equal(EntityState.Added, context.Entry(orphaned).State);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);
            }

            using (var context = CreateContext())
            {
                var root = LoadRequiredGraph(context);

                Assert.Null(root.RequiredSingle);

                Assert.Empty(context.RequiredSingle1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredSingle2s.Where(e => e.Id == orphanedId));
            }
        }

        [ConditionalFact]
        public virtual void Required_non_PK_one_to_one_are_cascade_detached_when_Added()
        {
            int removedId;
            int orphanedId;

            using (var context = CreateContext())
            {
                var root = LoadRequiredNonPkGraph(context);

                var removed = root.RequiredNonPkSingle;

                removedId = removed.Id;
                var orphaned = removed.Single;
                orphanedId = orphaned.Id;

                context.Entry(orphaned).State = EntityState.Added;

                Assert.Equal(EntityState.Unchanged, context.Entry(removed).State);
                Assert.Equal(EntityState.Added, context.Entry(orphaned).State);

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);
                Assert.Equal(EntityState.Added, context.Entry(orphaned).State);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);
            }

            using (var context = CreateContext())
            {
                var root = LoadRequiredNonPkGraph(context);

                Assert.Null(root.RequiredNonPkSingle);

                Assert.Empty(context.RequiredNonPkSingle1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredNonPkSingle2s.Where(e => e.Id == orphanedId));
            }
        }

        [ConditionalFact]
        public virtual void Required_many_to_one_dependents_with_alternate_key_are_cascade_detached_when_Added()
        {
            int removedId;
            List<int> orphanedIds;
            List<int> orphanedIdCs;

            using (var context = CreateContext())
            {
                var root = LoadRequiredAkGraph(context);

                Assert.Equal(2, root.RequiredChildrenAk.Count());

                var removed = root.RequiredChildrenAk.First();

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
                Assert.Equal(EntityState.Added, context.Entry(added).State);
                Assert.Equal(EntityState.Added, context.Entry(addedC).State);
                Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Unchanged));
                Assert.True(cascadeRemovedC.All(e => context.Entry(e).State == EntityState.Unchanged));

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.Equal(EntityState.Detached, context.Entry(added).State);
                Assert.Equal(EntityState.Detached, context.Entry(addedC).State);
                Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Detached));
                Assert.True(cascadeRemovedC.All(e => context.Entry(e).State == EntityState.Detached));
            }

            using (var context = CreateContext())
            {
                var root = LoadRequiredAkGraph(context);

                Assert.Equal(1, root.RequiredChildrenAk.Count());
                Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                Assert.Empty(context.RequiredAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredAk2s.Where(e => orphanedIds.Contains(e.Id)));
                Assert.Empty(context.RequiredComposite2s.Where(e => orphanedIdCs.Contains(e.Id)));
            }
        }

        [ConditionalFact]
        public virtual void Required_one_to_one_with_alternate_key_are_cascade_detached_when_Added()
        {
            int removedId;
            int orphanedId;
            int orphanedIdC;

            using (var context = CreateContext())
            {
                var root = LoadRequiredAkGraph(context);

                var removed = root.RequiredSingleAk;

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
                Assert.Equal(EntityState.Added, context.Entry(orphaned).State);
                Assert.Equal(EntityState.Added, context.Entry(orphanedC).State);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);
                Assert.Equal(EntityState.Detached, context.Entry(orphanedC).State);
            }

            using (var context = CreateContext())
            {
                var root = LoadRequiredAkGraph(context);

                Assert.Null(root.RequiredSingleAk);

                Assert.Empty(context.RequiredSingleAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredSingleAk2s.Where(e => e.Id == orphanedId));
                Assert.Empty(context.RequiredSingleComposite2s.Where(e => e.Id == orphanedIdC));
            }
        }

        [ConditionalFact]
        public virtual void Required_non_PK_one_to_one_with_alternate_key_are_cascade_detached_when_Added()
        {
            int removedId;
            int orphanedId;

            using (var context = CreateContext())
            {
                var root = LoadRequiredNonPkAkGraph(context);

                var removed = root.RequiredNonPkSingleAk;

                removedId = removed.Id;
                var orphaned = removed.Single;
                orphanedId = orphaned.Id;

                context.Entry(orphaned).State = EntityState.Added;

                Assert.Equal(EntityState.Unchanged, context.Entry(removed).State);
                Assert.Equal(EntityState.Added, context.Entry(orphaned).State);

                context.Remove(removed);

                Assert.Equal(EntityState.Deleted, context.Entry(removed).State);
                Assert.Equal(EntityState.Added, context.Entry(orphaned).State);

                context.SaveChanges();

                Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                Assert.Equal(EntityState.Detached, context.Entry(orphaned).State);
            }

            using (var context = CreateContext())
            {
                var root = LoadRequiredNonPkAkGraph(context);

                Assert.Null(root.RequiredNonPkSingleAk);

                Assert.Empty(context.RequiredNonPkSingleAk1s.Where(e => e.Id == removedId));
                Assert.Empty(context.RequiredNonPkSingleAk2s.Where(e => e.Id == orphanedId));
            }
        }

        private void Add<T>(IEnumerable<T> collection, T item) => ((ICollection<T>)collection).Add(item);

        private void Remove<T>(IEnumerable<T> collection, T item) => ((ICollection<T>)collection).Remove(item);

        [Flags]
        public enum ChangeMechanism
        {
            Dependent = 1,
            Principal = 2,
            Fk = 4
        }

        protected Expression<Func<Root, bool>> IsTheRoot => r => r.AlternateId == Fixture.RootAK;

        protected Root LoadRequiredGraph(GraphUpdatesContext context)
            => context.Roots
                .Include(e => e.RequiredChildren).ThenInclude(e => e.Children)
                .Include(e => e.RequiredSingle).ThenInclude(e => e.Single)
                .Single(IsTheRoot);

        protected Root LoadOptionalGraph(GraphUpdatesContext context)
            => context.Roots
                .Include(e => e.OptionalChildren).ThenInclude(e => e.Children)
                .Include(e => e.OptionalSingle).ThenInclude(e => e.Single)
                .Include(e => e.OptionalSingleDerived).ThenInclude(e => e.Single)
                .Include(e => e.OptionalSingleMoreDerived).ThenInclude(e => e.Single)
                .Single(IsTheRoot);

        protected Root LoadRequiredNonPkGraph(GraphUpdatesContext context)
            => context.Roots
                .Include(e => e.RequiredNonPkSingle).ThenInclude(e => e.Single)
                .Include(e => e.RequiredNonPkSingleDerived).ThenInclude(e => e.Single)
                .Include(e => e.RequiredNonPkSingleDerived).ThenInclude(e => e.Root)
                .Include(e => e.RequiredNonPkSingleMoreDerived).ThenInclude(e => e.Single)
                .Include(e => e.RequiredNonPkSingleMoreDerived).ThenInclude(e => e.Root)
                .Include(e => e.RequiredNonPkSingleMoreDerived).ThenInclude(e => e.DerivedRoot)
                .Single(IsTheRoot);

        protected Root LoadRequiredAkGraph(GraphUpdatesContext context)
            => context.Roots
                .Include(e => e.RequiredChildrenAk).ThenInclude(e => e.Children)
                .Include(e => e.RequiredChildrenAk).ThenInclude(e => e.CompositeChildren)
                .Include(e => e.RequiredSingleAk).ThenInclude(e => e.Single)
                .Include(e => e.RequiredSingleAk).ThenInclude(e => e.SingleComposite)
                .Single(IsTheRoot);

        protected Root LoadOptionalAkGraph(GraphUpdatesContext context)
            => context.Roots
                .Include(e => e.OptionalChildrenAk).ThenInclude(e => e.Children)
                .Include(e => e.OptionalChildrenAk).ThenInclude(e => e.CompositeChildren)
                .Include(e => e.OptionalSingleAk).ThenInclude(e => e.Single)
                .Include(e => e.OptionalSingleAk).ThenInclude(e => e.SingleComposite)
                .Include(e => e.OptionalSingleAkDerived).ThenInclude(e => e.Single)
                .Include(e => e.OptionalSingleAkMoreDerived).ThenInclude(e => e.Single)
                .Single(IsTheRoot);

        protected Root LoadRequiredNonPkAkGraph(GraphUpdatesContext context)
            => context.Roots
                .Include(e => e.RequiredNonPkSingleAk).ThenInclude(e => e.Single)
                .Include(e => e.RequiredNonPkSingleAkDerived).ThenInclude(e => e.Single)
                .Include(e => e.RequiredNonPkSingleAkDerived).ThenInclude(e => e.Root)
                .Include(e => e.RequiredNonPkSingleAkMoreDerived).ThenInclude(e => e.Single)
                .Include(e => e.RequiredNonPkSingleAkMoreDerived).ThenInclude(e => e.Root)
                .Include(e => e.RequiredNonPkSingleAkMoreDerived).ThenInclude(e => e.DerivedRoot)
                .Single(IsTheRoot);

        private static void AssertEntries(IReadOnlyList<EntityEntry> expectedEntries, IReadOnlyList<EntityEntry> actualEntries)
        {
            var newEntities = new HashSet<object>(actualEntries.Select(ne => ne.Entity));
            var missingEntities = expectedEntries.Select(e => e.Entity).Where(e => !newEntities.Contains(e)).ToList();
            Assert.Equal(new object[0], missingEntities);
            Assert.Equal(expectedEntries.Count, actualEntries.Count);
        }

        private static void AssertKeys(Root expected, Root actual)
        {
            Assert.Equal(expected.Id, actual.Id);

            Assert.Equal(
                expected.RequiredChildren.OrderBy(e => e.Id).Select(e => e.Id),
                actual.RequiredChildren.OrderBy(e => e.Id).Select(e => e.Id));

            Assert.Equal(
                expected.RequiredChildren.OrderBy(e => e.Id).Select(e => e.Children.Count()),
                actual.RequiredChildren.OrderBy(e => e.Id).Select(e => e.Children.Count()));

            Assert.Equal(
                expected.RequiredChildren.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.Id),
                actual.RequiredChildren.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.Id));

            Assert.Equal(
                expected.OptionalChildren.OrderBy(e => e.Id).Select(e => e.Id),
                actual.OptionalChildren.OrderBy(e => e.Id).Select(e => e.Id));

            Assert.Equal(
                expected.OptionalChildren.OrderBy(e => e.Id).Select(e => e.Children.Count()),
                actual.OptionalChildren.OrderBy(e => e.Id).Select(e => e.Children.Count()));

            Assert.Equal(
                expected.OptionalChildren.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.Id),
                actual.OptionalChildren.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.Id));

            Assert.Equal(expected.RequiredSingle?.Id, actual.RequiredSingle?.Id);
            Assert.Equal(expected.OptionalSingle?.Id, actual.OptionalSingle?.Id);
            Assert.Equal(expected.OptionalSingleDerived?.Id, actual.OptionalSingleDerived?.Id);
            Assert.Equal(expected.OptionalSingleMoreDerived?.Id, actual.OptionalSingleMoreDerived?.Id);
            Assert.Equal(expected.RequiredNonPkSingle?.Id, actual.RequiredNonPkSingle?.Id);
            Assert.Equal(expected.RequiredNonPkSingleDerived?.Id, actual.RequiredNonPkSingleDerived?.Id);
            Assert.Equal(expected.RequiredNonPkSingleMoreDerived?.Id, actual.RequiredNonPkSingleMoreDerived?.Id);

            Assert.Equal(expected.RequiredSingle?.Single?.Id, actual.RequiredSingle?.Single?.Id);
            Assert.Equal(expected.OptionalSingle?.Single?.Id, actual.OptionalSingle?.Single?.Id);
            Assert.Equal(expected.OptionalSingleDerived?.Single?.Id, actual.OptionalSingleDerived?.Single?.Id);
            Assert.Equal(expected.OptionalSingleMoreDerived?.Single?.Id, actual.OptionalSingleMoreDerived?.Single?.Id);
            Assert.Equal(expected.RequiredNonPkSingle?.Single?.Id, actual.RequiredNonPkSingle?.Single?.Id);
            Assert.Equal(expected.RequiredNonPkSingleDerived?.Single?.Id, actual.RequiredNonPkSingleDerived?.Single?.Id);
            Assert.Equal(expected.RequiredNonPkSingleMoreDerived?.Single?.Id, actual.RequiredNonPkSingleMoreDerived?.Single?.Id);

            Assert.Equal(expected.AlternateId, actual.AlternateId);

            Assert.Equal(
                expected.RequiredChildrenAk.OrderBy(e => e.Id).Select(e => e.AlternateId),
                actual.RequiredChildrenAk.OrderBy(e => e.Id).Select(e => e.AlternateId));

            Assert.Equal(
                expected.RequiredChildrenAk.OrderBy(e => e.Id).Select(e => e.Children.Count()),
                actual.RequiredChildrenAk.OrderBy(e => e.Id).Select(e => e.Children.Count()));

            Assert.Equal(
                expected.RequiredChildrenAk.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.AlternateId),
                actual.RequiredChildrenAk.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.AlternateId));

            Assert.Equal(
                expected.RequiredChildrenAk.OrderBy(e => e.Id).SelectMany(e => e.CompositeChildren).OrderBy(e => e.Id).Select(e => e.Id),
                actual.RequiredChildrenAk.OrderBy(e => e.Id).SelectMany(e => e.CompositeChildren).OrderBy(e => e.Id).Select(e => e.Id));

            Assert.Equal(
                expected.OptionalChildrenAk.OrderBy(e => e.Id).Select(e => e.AlternateId),
                actual.OptionalChildrenAk.OrderBy(e => e.Id).Select(e => e.AlternateId));

            Assert.Equal(
                expected.OptionalChildrenAk.OrderBy(e => e.Id).Select(e => e.Children.Count()),
                actual.OptionalChildrenAk.OrderBy(e => e.Id).Select(e => e.Children.Count()));

            Assert.Equal(
                expected.OptionalChildrenAk.OrderBy(e => e.Id).Select(e => e.CompositeChildren.Count()),
                actual.OptionalChildrenAk.OrderBy(e => e.Id).Select(e => e.CompositeChildren.Count()));

            Assert.Equal(
                expected.OptionalChildrenAk.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.AlternateId),
                actual.OptionalChildrenAk.OrderBy(e => e.Id).SelectMany(e => e.Children).OrderBy(e => e.Id).Select(e => e.AlternateId));

            Assert.Equal(
                expected.OptionalChildrenAk.OrderBy(e => e.Id).SelectMany(e => e.CompositeChildren).OrderBy(e => e.Id).Select(e => e.Id),
                actual.OptionalChildrenAk.OrderBy(e => e.Id).SelectMany(e => e.CompositeChildren).OrderBy(e => e.Id).Select(e => e.Id));

            Assert.Equal(expected.RequiredSingleAk?.AlternateId, actual.RequiredSingleAk?.AlternateId);
            Assert.Equal(expected.OptionalSingleAk?.AlternateId, actual.OptionalSingleAk?.AlternateId);
            Assert.Equal(expected.OptionalSingleAkDerived?.AlternateId, actual.OptionalSingleAkDerived?.AlternateId);
            Assert.Equal(expected.OptionalSingleAkMoreDerived?.AlternateId, actual.OptionalSingleAkMoreDerived?.AlternateId);
            Assert.Equal(expected.RequiredNonPkSingleAk?.AlternateId, actual.RequiredNonPkSingleAk?.AlternateId);
            Assert.Equal(expected.RequiredNonPkSingleAkDerived?.AlternateId, actual.RequiredNonPkSingleAkDerived?.AlternateId);
            Assert.Equal(expected.RequiredNonPkSingleAkMoreDerived?.AlternateId, actual.RequiredNonPkSingleAkMoreDerived?.AlternateId);

            Assert.Equal(expected.RequiredSingleAk?.Single?.AlternateId, actual.RequiredSingleAk?.Single?.AlternateId);
            Assert.Equal(expected.RequiredSingleAk?.SingleComposite?.Id, actual.RequiredSingleAk?.SingleComposite?.Id);
            Assert.Equal(expected.OptionalSingleAk?.Single?.AlternateId, actual.OptionalSingleAk?.Single?.AlternateId);
            Assert.Equal(expected.OptionalSingleAk?.SingleComposite?.Id, actual.OptionalSingleAk?.SingleComposite?.Id);
            Assert.Equal(expected.OptionalSingleAkDerived?.Single?.AlternateId, actual.OptionalSingleAkDerived?.Single?.AlternateId);
            Assert.Equal(expected.OptionalSingleAkMoreDerived?.Single?.AlternateId, actual.OptionalSingleAkMoreDerived?.Single?.AlternateId);
            Assert.Equal(expected.RequiredNonPkSingleAk?.Single?.AlternateId, actual.RequiredNonPkSingleAk?.Single?.AlternateId);
            Assert.Equal(expected.RequiredNonPkSingleAkDerived?.Single?.AlternateId, actual.RequiredNonPkSingleAkDerived?.Single?.AlternateId);
            Assert.Equal(expected.RequiredNonPkSingleAkMoreDerived?.Single?.AlternateId, actual.RequiredNonPkSingleAkMoreDerived?.Single?.AlternateId);
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

            if (root.RequiredSingle != null)
            {
                Assert.Same(root, root.RequiredSingle.Root);
                Assert.Same(root.RequiredSingle, root.RequiredSingle.Single.Back);
            }

            if (root.OptionalSingle != null)
            {
                Assert.Same(root, root.OptionalSingle.Root);
                Assert.Same(root, root.OptionalSingleDerived.DerivedRoot);
                Assert.Same(root, root.OptionalSingleMoreDerived.MoreDerivedRoot);
                Assert.Same(root.OptionalSingle, root.OptionalSingle.Single.Back);
                Assert.Same(root.OptionalSingleDerived, root.OptionalSingleDerived.Single.Back);
                Assert.Same(root.OptionalSingleMoreDerived, root.OptionalSingleMoreDerived.Single.Back);
            }

            if (root.RequiredNonPkSingle != null)
            {
                Assert.Same(root, root.RequiredNonPkSingle.Root);
                Assert.Same(root, root.RequiredNonPkSingleDerived.DerivedRoot);
                Assert.Same(root, root.RequiredNonPkSingleMoreDerived.MoreDerivedRoot);
                Assert.Same(root.RequiredNonPkSingle, root.RequiredNonPkSingle.Single.Back);
                Assert.Same(root.RequiredNonPkSingleDerived, root.RequiredNonPkSingleDerived.Single.Back);
                Assert.Same(root.RequiredNonPkSingleMoreDerived, root.RequiredNonPkSingleMoreDerived.Single.Back);
            }

            foreach (var child in root.RequiredChildrenAk)
            {
                Assert.Same(root, child.Parent);
                Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
                Assert.All(child.CompositeChildren.Select(e => e.Parent), e => Assert.Same(child, e));
            }

            foreach (var child in root.OptionalChildrenAk)
            {
                Assert.Same(root, child.Parent);
                Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
                Assert.All(child.CompositeChildren.Select(e => e.Parent), e => Assert.Same(child, e));
            }

            if (root.RequiredSingleAk != null)
            {
                Assert.Same(root, root.RequiredSingleAk.Root);
                Assert.Same(root.RequiredSingleAk, root.RequiredSingleAk.Single.Back);
                Assert.Same(root.RequiredSingleAk, root.RequiredSingleAk.SingleComposite.Back);
            }

            if (root.OptionalSingleAk != null)
            {
                Assert.Same(root, root.OptionalSingleAk.Root);
                Assert.Same(root, root.OptionalSingleAkDerived.DerivedRoot);
                Assert.Same(root, root.OptionalSingleAkMoreDerived.MoreDerivedRoot);
                Assert.Same(root.OptionalSingleAk, root.OptionalSingleAk.Single.Back);
                Assert.Same(root.OptionalSingleAk, root.OptionalSingleAk.SingleComposite.Back);
                Assert.Same(root.OptionalSingleAkDerived, root.OptionalSingleAkDerived.Single.Back);
                Assert.Same(root.OptionalSingleAkMoreDerived, root.OptionalSingleAkMoreDerived.Single.Back);
            }

            if (root.RequiredNonPkSingleAk != null)
            {
                Assert.Same(root, root.RequiredNonPkSingleAk.Root);
                Assert.Same(root, root.RequiredNonPkSingleAkDerived.DerivedRoot);
                Assert.Same(root, root.RequiredNonPkSingleAkMoreDerived.MoreDerivedRoot);
                Assert.Same(root.RequiredNonPkSingleAk, root.RequiredNonPkSingleAk.Single.Back);
                Assert.Same(root.RequiredNonPkSingleAkDerived, root.RequiredNonPkSingleAkDerived.Single.Back);
                Assert.Same(root.RequiredNonPkSingleAkMoreDerived, root.RequiredNonPkSingleAkMoreDerived.Single.Back);
            }
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
                Assert.All(child.CompositeChildren.Select(e => e.Parent), e => Assert.Same(child, e));
            }

            foreach (var child in root.OptionalChildrenAk)
            {
                Assert.Same(root, child.Parent);
                Assert.All(child.Children.Select(e => e.Parent), e => Assert.Same(child, e));
                Assert.All(child.CompositeChildren.Select(e => e.Parent), e => Assert.Same(child, e));
            }

            if (root.RequiredSingleAk != null)
            {
                Assert.Same(root, root.RequiredSingleAk.Root);
                Assert.Same(root.RequiredSingleAk, root.RequiredSingleAk.Single.Back);
                Assert.Same(root.RequiredSingleAk, root.RequiredSingleAk.SingleComposite.Back);
            }

            if (root.OptionalSingleAk != null)
            {
                Assert.Same(root, root.OptionalSingleAk.Root);
                Assert.Same(root.OptionalSingleAk, root.OptionalSingleAk.Single.Back);
                Assert.Same(root.OptionalSingleAk, root.OptionalSingleAk.SingleComposite.Back);
            }

            if (root.RequiredNonPkSingleAk != null)
            {
                Assert.Same(root, root.RequiredNonPkSingleAk.Root);
                Assert.Same(root.RequiredNonPkSingleAk, root.RequiredNonPkSingleAk.Single.Back);
            }
        }

        protected class Root : NotifyingEntity
        {
            private int _id;
            private Guid _alternateId;
            private IEnumerable<Required1> _requiredChildren = new ObservableHashSet<Required1>();
            private IEnumerable<Optional1> _optionalChildren = new ObservableHashSet<Optional1>();
            private RequiredSingle1 _requiredSingle;
            private RequiredNonPkSingle1 _requiredNonPkSingle;
            private RequiredNonPkSingle1Derived _requiredNonPkSingleDerived;
            private RequiredNonPkSingle1MoreDerived _requiredNonPkSingleMoreDerived;
            private OptionalSingle1 _optionalSingle;
            private OptionalSingle1Derived _optionalSingleDerived;
            private OptionalSingle1MoreDerived _optionalSingleMoreDerived;
            private IEnumerable<RequiredAk1> _requiredChildrenAk = new ObservableHashSet<RequiredAk1>();
            private IEnumerable<OptionalAk1> _optionalChildrenAk = new ObservableHashSet<OptionalAk1>();
            private RequiredSingleAk1 _requiredSingleAk;
            private RequiredNonPkSingleAk1 _requiredNonPkSingleAk;
            private RequiredNonPkSingleAk1Derived _requiredNonPkSingleAkDerived;
            private RequiredNonPkSingleAk1MoreDerived _requiredNonPkSingleAkMoreDerived;
            private OptionalSingleAk1 _optionalSingleAk;
            private OptionalSingleAk1Derived _optionalSingleAkDerived;
            private OptionalSingleAk1MoreDerived _optionalSingleAkMoreDerived;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public Guid AlternateId
            {
                get { return _alternateId; }
                set { SetWithNotify(value, ref _alternateId); }
            }

            public IEnumerable<Required1> RequiredChildren
            {
                get { return _requiredChildren; }
                set { SetWithNotify(value, ref _requiredChildren); }
            }

            public IEnumerable<Optional1> OptionalChildren
            {
                get { return _optionalChildren; }
                set { SetWithNotify(value, ref _optionalChildren); }
            }

            public RequiredSingle1 RequiredSingle
            {
                get { return _requiredSingle; }
                set { SetWithNotify(value, ref _requiredSingle); }
            }

            public RequiredNonPkSingle1 RequiredNonPkSingle
            {
                get { return _requiredNonPkSingle; }
                set { SetWithNotify(value, ref _requiredNonPkSingle); }
            }

            public RequiredNonPkSingle1Derived RequiredNonPkSingleDerived
            {
                get { return _requiredNonPkSingleDerived; }
                set { SetWithNotify(value, ref _requiredNonPkSingleDerived); }
            }

            public RequiredNonPkSingle1MoreDerived RequiredNonPkSingleMoreDerived
            {
                get { return _requiredNonPkSingleMoreDerived; }
                set { SetWithNotify(value, ref _requiredNonPkSingleMoreDerived); }
            }

            public OptionalSingle1 OptionalSingle
            {
                get { return _optionalSingle; }
                set { SetWithNotify(value, ref _optionalSingle); }
            }

            public OptionalSingle1Derived OptionalSingleDerived
            {
                get { return _optionalSingleDerived; }
                set { SetWithNotify(value, ref _optionalSingleDerived); }
            }

            public OptionalSingle1MoreDerived OptionalSingleMoreDerived
            {
                get { return _optionalSingleMoreDerived; }
                set { SetWithNotify(value, ref _optionalSingleMoreDerived); }
            }

            public IEnumerable<RequiredAk1> RequiredChildrenAk
            {
                get { return _requiredChildrenAk; }
                set { SetWithNotify(value, ref _requiredChildrenAk); }
            }

            public IEnumerable<OptionalAk1> OptionalChildrenAk
            {
                get { return _optionalChildrenAk; }
                set { SetWithNotify(value, ref _optionalChildrenAk); }
            }

            public RequiredSingleAk1 RequiredSingleAk
            {
                get { return _requiredSingleAk; }
                set { SetWithNotify(value, ref _requiredSingleAk); }
            }

            public RequiredNonPkSingleAk1 RequiredNonPkSingleAk
            {
                get { return _requiredNonPkSingleAk; }
                set { SetWithNotify(value, ref _requiredNonPkSingleAk); }
            }

            public RequiredNonPkSingleAk1Derived RequiredNonPkSingleAkDerived
            {
                get { return _requiredNonPkSingleAkDerived; }
                set { SetWithNotify(value, ref _requiredNonPkSingleAkDerived); }
            }

            public RequiredNonPkSingleAk1MoreDerived RequiredNonPkSingleAkMoreDerived
            {
                get { return _requiredNonPkSingleAkMoreDerived; }
                set { SetWithNotify(value, ref _requiredNonPkSingleAkMoreDerived); }
            }

            public OptionalSingleAk1 OptionalSingleAk
            {
                get { return _optionalSingleAk; }
                set { SetWithNotify(value, ref _optionalSingleAk); }
            }

            public OptionalSingleAk1Derived OptionalSingleAkDerived
            {
                get { return _optionalSingleAkDerived; }
                set { SetWithNotify(value, ref _optionalSingleAkDerived); }
            }

            public OptionalSingleAk1MoreDerived OptionalSingleAkMoreDerived
            {
                get { return _optionalSingleAkMoreDerived; }
                set { SetWithNotify(value, ref _optionalSingleAkMoreDerived); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as Root;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class Required1 : NotifyingEntity
        {
            private int _id;
            private int _parentId;
            private Root _parent;
            private IEnumerable<Required2> _children = new ObservableHashSet<Required2>();

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public int ParentId
            {
                get { return _parentId; }
                set { SetWithNotify(value, ref _parentId); }
            }

            public Root Parent
            {
                get { return _parent; }
                set { SetWithNotify(value, ref _parent); }
            }

            public IEnumerable<Required2> Children
            {
                get { return _children; }
                set { SetWithNotify(value, ref _children); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as Required1;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class Required1Derived : Required1
        {
            public override bool Equals(object obj) => base.Equals(obj as Required1Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class Required1MoreDerived : Required1Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as Required1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class Required2 : NotifyingEntity
        {
            private int _id;
            private int _parentId;
            private Required1 _parent;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public int ParentId
            {
                get { return _parentId; }
                set { SetWithNotify(value, ref _parentId); }
            }

            public Required1 Parent
            {
                get { return _parent; }
                set { SetWithNotify(value, ref _parent); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as Required2;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class Required2Derived : Required2
        {
            public override bool Equals(object obj) => base.Equals(obj as Required2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class Required2MoreDerived : Required2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as Required2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class Optional1 : NotifyingEntity
        {
            private int _id;
            private int? _parentId;
            private Root _parent;
            private IEnumerable<Optional2> _children = new ObservableHashSet<Optional2>();

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public int? ParentId
            {
                get { return _parentId; }
                set { SetWithNotify(value, ref _parentId); }
            }

            public Root Parent
            {
                get { return _parent; }
                set { SetWithNotify(value, ref _parent); }
            }

            public IEnumerable<Optional2> Children
            {
                get { return _children; }
                set { SetWithNotify(value, ref _children); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as Optional1;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class Optional1Derived : Optional1
        {
            public override bool Equals(object obj) => base.Equals(obj as Optional1Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class Optional1MoreDerived : Optional1Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as Optional1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class Optional2 : NotifyingEntity
        {
            private int _id;
            private int? _parentId;
            private Optional1 _parent;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public int? ParentId
            {
                get { return _parentId; }
                set { SetWithNotify(value, ref _parentId); }
            }

            public Optional1 Parent
            {
                get { return _parent; }
                set { SetWithNotify(value, ref _parent); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as Optional2;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class Optional2Derived : Optional2
        {
            public override bool Equals(object obj) => base.Equals(obj as Optional2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class Optional2MoreDerived : Optional2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as Optional2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class RequiredSingle1 : NotifyingEntity
        {
            private int _id;
            private Root _root;
            private RequiredSingle2 _single;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public Root Root
            {
                get { return _root; }
                set { SetWithNotify(value, ref _root); }
            }

            public RequiredSingle2 Single
            {
                get { return _single; }
                set { SetWithNotify(value, ref _single); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as RequiredSingle1;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class RequiredSingle2 : NotifyingEntity
        {
            private int _id;
            private RequiredSingle1 _back;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public RequiredSingle1 Back
            {
                get { return _back; }
                set { SetWithNotify(value, ref _back); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as RequiredSingle2;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class RequiredNonPkSingle1 : NotifyingEntity
        {
            private int _id;
            private int _rootId;
            private Root _root;
            private RequiredNonPkSingle2 _single;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public int RootId
            {
                get { return _rootId; }
                set { SetWithNotify(value, ref _rootId); }
            }

            public Root Root
            {
                get { return _root; }
                set { SetWithNotify(value, ref _root); }
            }

            public RequiredNonPkSingle2 Single
            {
                get { return _single; }
                set { SetWithNotify(value, ref _single); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as RequiredNonPkSingle1;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class RequiredNonPkSingle1Derived : RequiredNonPkSingle1
        {
            private int _derivedRootId;
            private Root _derivedRoot;

            public int DerivedRootId
            {
                get { return _derivedRootId; }
                set { SetWithNotify(value, ref _derivedRootId); }
            }

            public Root DerivedRoot
            {
                get { return _derivedRoot; }
                set { SetWithNotify(value, ref _derivedRoot); }
            }

            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingle1Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class RequiredNonPkSingle1MoreDerived : RequiredNonPkSingle1Derived
        {
            private int _moreDerivedRootId;
            private Root _moreDerivedRoot;

            public int MoreDerivedRootId
            {
                get { return _moreDerivedRootId; }
                set { SetWithNotify(value, ref _moreDerivedRootId); }
            }

            public Root MoreDerivedRoot
            {
                get { return _moreDerivedRoot; }
                set { SetWithNotify(value, ref _moreDerivedRoot); }
            }

            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingle1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class RequiredNonPkSingle2 : NotifyingEntity
        {
            private int _id;
            private int _backId;
            private RequiredNonPkSingle1 _back;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public int BackId
            {
                get { return _backId; }
                set { SetWithNotify(value, ref _backId); }
            }

            public RequiredNonPkSingle1 Back
            {
                get { return _back; }
                set { SetWithNotify(value, ref _back); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as RequiredNonPkSingle2;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class RequiredNonPkSingle2Derived : RequiredNonPkSingle2
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingle2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class RequiredNonPkSingle2MoreDerived : RequiredNonPkSingle2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingle2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class OptionalSingle1 : NotifyingEntity
        {
            private int _id;
            private int? _rootId;
            private Root _root;
            private OptionalSingle2 _single;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public int? RootId
            {
                get { return _rootId; }
                set { SetWithNotify(value, ref _rootId); }
            }

            public Root Root
            {
                get { return _root; }
                set { SetWithNotify(value, ref _root); }
            }

            public OptionalSingle2 Single
            {
                get { return _single; }
                set { SetWithNotify(value, ref _single); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as OptionalSingle1;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class OptionalSingle1Derived : OptionalSingle1
        {
            private int? _derivedRootId;
            private Root _derivedRoot;

            public int? DerivedRootId
            {
                get { return _derivedRootId; }
                set { SetWithNotify(value, ref _derivedRootId); }
            }

            public Root DerivedRoot
            {
                get { return _derivedRoot; }
                set { SetWithNotify(value, ref _derivedRoot); }
            }

            public override bool Equals(object obj) => base.Equals(obj as OptionalSingle1Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class OptionalSingle1MoreDerived : OptionalSingle1Derived
        {
            private Root _moreDerivedRoot;
            private int? _moreDerivedRootId;

            public int? MoreDerivedRootId
            {
                get { return _moreDerivedRootId; }
                set { SetWithNotify(value, ref _moreDerivedRootId); }
            }

            public Root MoreDerivedRoot
            {
                get { return _moreDerivedRoot; }
                set { SetWithNotify(value, ref _moreDerivedRoot); }
            }

            public override bool Equals(object obj) => base.Equals(obj as OptionalSingle1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class OptionalSingle2 : NotifyingEntity
        {
            private int _id;
            private int? _backId;
            private OptionalSingle1 _back;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public int? BackId
            {
                get { return _backId; }
                set { SetWithNotify(value, ref _backId); }
            }

            public OptionalSingle1 Back
            {
                get { return _back; }
                set { SetWithNotify(value, ref _back); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as OptionalSingle2;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class OptionalSingle2Derived : OptionalSingle2
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalSingle2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class OptionalSingle2MoreDerived : OptionalSingle2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalSingle2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class RequiredAk1 : NotifyingEntity
        {
            private int _id;
            private Guid _alternateId;
            private Guid _parentId;
            private Root _parent;
            private IEnumerable<RequiredAk2> _children = new ObservableHashSet<RequiredAk2>();
            private IEnumerable<RequiredComposite2> _compositeChildren = new ObservableHashSet<RequiredComposite2>();

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public Guid AlternateId
            {
                get { return _alternateId; }
                set { SetWithNotify(value, ref _alternateId); }
            }

            public Guid ParentId
            {
                get { return _parentId; }
                set { SetWithNotify(value, ref _parentId); }
            }

            public Root Parent
            {
                get { return _parent; }
                set { SetWithNotify(value, ref _parent); }
            }

            public IEnumerable<RequiredAk2> Children
            {
                get { return _children; }
                set { SetWithNotify(value, ref _children); }
            }

            public IEnumerable<RequiredComposite2> CompositeChildren
            {
                get { return _compositeChildren; }
                set { SetWithNotify(value, ref _compositeChildren); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as RequiredAk1;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class RequiredAk1Derived : RequiredAk1
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredAk1Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class RequiredAk1MoreDerived : RequiredAk1Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredAk1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class RequiredAk2 : NotifyingEntity
        {
            private int _id;
            private Guid _alternateId;
            private Guid _parentId;
            private RequiredAk1 _parent;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public Guid AlternateId
            {
                get { return _alternateId; }
                set { SetWithNotify(value, ref _alternateId); }
            }

            public Guid ParentId
            {
                get { return _parentId; }
                set { SetWithNotify(value, ref _parentId); }
            }

            public RequiredAk1 Parent
            {
                get { return _parent; }
                set { SetWithNotify(value, ref _parent); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as RequiredAk2;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class RequiredComposite2 : NotifyingEntity
        {
            private int _id;
            private Guid _alternateId;
            private int _parentId;
            private RequiredAk1 _parent;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public Guid ParentAlternateId
            {
                get { return _alternateId; }
                set { SetWithNotify(value, ref _alternateId); }
            }

            public int ParentId
            {
                get { return _parentId; }
                set { SetWithNotify(value, ref _parentId); }
            }

            public RequiredAk1 Parent
            {
                get { return _parent; }
                set { SetWithNotify(value, ref _parent); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as RequiredComposite2;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class RequiredAk2Derived : RequiredAk2
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredAk2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class RequiredAk2MoreDerived : RequiredAk2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredAk2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class OptionalAk1 : NotifyingEntity
        {
            private int _id;
            private Guid _alternateId;
            private Guid? _parentId;
            private Root _parent;
            private IEnumerable<OptionalAk2> _children = new ObservableHashSet<OptionalAk2>();
            private IEnumerable<OptionalComposite2> _compositeChildren = new ObservableHashSet<OptionalComposite2>();

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public Guid AlternateId
            {
                get { return _alternateId; }
                set { SetWithNotify(value, ref _alternateId); }
            }

            public Guid? ParentId
            {
                get { return _parentId; }
                set { SetWithNotify(value, ref _parentId); }
            }

            public Root Parent
            {
                get { return _parent; }
                set { SetWithNotify(value, ref _parent); }
            }

            public IEnumerable<OptionalAk2> Children
            {
                get { return _children; }
                set { SetWithNotify(value, ref _children); }
            }

            public IEnumerable<OptionalComposite2> CompositeChildren
            {
                get { return _compositeChildren; }
                set { SetWithNotify(value, ref _compositeChildren); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as OptionalAk1;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class OptionalAk1Derived : OptionalAk1
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalAk1Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class OptionalAk1MoreDerived : OptionalAk1Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalAk1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class OptionalAk2 : NotifyingEntity
        {
            private int _id;
            private Guid _alternateId;
            private Guid? _parentId;
            private OptionalAk1 _parent;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public Guid AlternateId
            {
                get { return _alternateId; }
                set { SetWithNotify(value, ref _alternateId); }
            }

            public Guid? ParentId
            {
                get { return _parentId; }
                set { SetWithNotify(value, ref _parentId); }
            }

            public OptionalAk1 Parent
            {
                get { return _parent; }
                set { SetWithNotify(value, ref _parent); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as OptionalAk2;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class OptionalComposite2 : NotifyingEntity
        {
            private int _id;
            private Guid _alternateId;
            private int? _parentId;
            private OptionalAk1 _parent;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public Guid ParentAlternateId
            {
                get { return _alternateId; }
                set { SetWithNotify(value, ref _alternateId); }
            }

            public int? ParentId
            {
                get { return _parentId; }
                set { SetWithNotify(value, ref _parentId); }
            }

            public OptionalAk1 Parent
            {
                get { return _parent; }
                set { SetWithNotify(value, ref _parent); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as OptionalComposite2;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class OptionalAk2Derived : OptionalAk2
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalAk2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class OptionalAk2MoreDerived : OptionalAk2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalAk2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class RequiredSingleAk1 : NotifyingEntity
        {
            private int _id;
            private Guid _alternateId;
            private Guid _rootId;
            private Root _root;
            private RequiredSingleAk2 _single;
            private RequiredSingleComposite2 _singleComposite;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public Guid AlternateId
            {
                get { return _alternateId; }
                set { SetWithNotify(value, ref _alternateId); }
            }

            public Guid RootId
            {
                get { return _rootId; }
                set { SetWithNotify(value, ref _rootId); }
            }

            public Root Root
            {
                get { return _root; }
                set { SetWithNotify(value, ref _root); }
            }

            public RequiredSingleAk2 Single
            {
                get { return _single; }
                set { SetWithNotify(value, ref _single); }
            }

            public RequiredSingleComposite2 SingleComposite
            {
                get { return _singleComposite; }
                set { SetWithNotify(value, ref _singleComposite); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as RequiredSingleAk1;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class RequiredSingleAk2 : NotifyingEntity
        {
            private int _id;
            private Guid _alternateId;
            private Guid _backId;
            private RequiredSingleAk1 _back;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public Guid AlternateId
            {
                get { return _alternateId; }
                set { SetWithNotify(value, ref _alternateId); }
            }

            public Guid BackId
            {
                get { return _backId; }
                set { SetWithNotify(value, ref _backId); }
            }

            public RequiredSingleAk1 Back
            {
                get { return _back; }
                set { SetWithNotify(value, ref _back); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as RequiredSingleAk2;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class RequiredSingleComposite2 : NotifyingEntity
        {
            private int _id;
            private Guid _alternateId;
            private int _backId;
            private RequiredSingleAk1 _back;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public Guid BackAlternateId
            {
                get { return _alternateId; }
                set { SetWithNotify(value, ref _alternateId); }
            }

            public int BackId
            {
                get { return _backId; }
                set { SetWithNotify(value, ref _backId); }
            }

            public RequiredSingleAk1 Back
            {
                get { return _back; }
                set { SetWithNotify(value, ref _back); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as RequiredSingleComposite2;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class RequiredNonPkSingleAk1 : NotifyingEntity
        {
            private int _id;
            private Guid _alternateId;
            private Guid _rootId;
            private Root _root;
            private RequiredNonPkSingleAk2 _single;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public Guid AlternateId
            {
                get { return _alternateId; }
                set { SetWithNotify(value, ref _alternateId); }
            }

            public Guid RootId
            {
                get { return _rootId; }
                set { SetWithNotify(value, ref _rootId); }
            }

            public Root Root
            {
                get { return _root; }
                set { SetWithNotify(value, ref _root); }
            }

            public RequiredNonPkSingleAk2 Single
            {
                get { return _single; }
                set { SetWithNotify(value, ref _single); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as RequiredNonPkSingleAk1;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class RequiredNonPkSingleAk1Derived : RequiredNonPkSingleAk1
        {
            private Guid _derivedRootId;
            private Root _derivedRoot;

            public Guid DerivedRootId
            {
                get { return _derivedRootId; }
                set { SetWithNotify(value, ref _derivedRootId); }
            }

            public Root DerivedRoot
            {
                get { return _derivedRoot; }
                set { SetWithNotify(value, ref _derivedRoot); }
            }

            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingleAk1Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class RequiredNonPkSingleAk1MoreDerived : RequiredNonPkSingleAk1Derived
        {
            private Guid _moreDerivedRootId;
            private Root _moreDerivedRoot;

            public Guid MoreDerivedRootId
            {
                get { return _moreDerivedRootId; }
                set { SetWithNotify(value, ref _moreDerivedRootId); }
            }

            public Root MoreDerivedRoot
            {
                get { return _moreDerivedRoot; }
                set { SetWithNotify(value, ref _moreDerivedRoot); }
            }

            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingleAk1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class RequiredNonPkSingleAk2 : NotifyingEntity
        {
            private int _id;
            private Guid _alternateId;
            private Guid _backId;
            private RequiredNonPkSingleAk1 _back;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public Guid AlternateId
            {
                get { return _alternateId; }
                set { SetWithNotify(value, ref _alternateId); }
            }

            public Guid BackId
            {
                get { return _backId; }
                set { SetWithNotify(value, ref _backId); }
            }

            public RequiredNonPkSingleAk1 Back
            {
                get { return _back; }
                set { SetWithNotify(value, ref _back); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as RequiredNonPkSingleAk2;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class RequiredNonPkSingleAk2Derived : RequiredNonPkSingleAk2
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingleAk2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class RequiredNonPkSingleAk2MoreDerived : RequiredNonPkSingleAk2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as RequiredNonPkSingleAk2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class OptionalSingleAk1 : NotifyingEntity
        {
            private int _id;
            private Guid _alternateId;
            private Guid? _rootId;
            private Root _root;
            private OptionalSingleAk2 _single;
            private OptionalSingleComposite2 _singleComposite;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public Guid AlternateId
            {
                get { return _alternateId; }
                set { SetWithNotify(value, ref _alternateId); }
            }

            public Guid? RootId
            {
                get { return _rootId; }
                set { SetWithNotify(value, ref _rootId); }
            }

            public Root Root
            {
                get { return _root; }
                set { SetWithNotify(value, ref _root); }
            }

            public OptionalSingleComposite2 SingleComposite
            {
                get { return _singleComposite; }
                set { SetWithNotify(value, ref _singleComposite); }
            }

            public OptionalSingleAk2 Single
            {
                get { return _single; }
                set { SetWithNotify(value, ref _single); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as OptionalSingleAk1;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class OptionalSingleAk1Derived : OptionalSingleAk1
        {
            private Guid? _derivedRootId;
            private Root _derivedRoot;

            public Guid? DerivedRootId
            {
                get { return _derivedRootId; }
                set { SetWithNotify(value, ref _derivedRootId); }
            }

            public Root DerivedRoot
            {
                get { return _derivedRoot; }
                set { SetWithNotify(value, ref _derivedRoot); }
            }

            public override bool Equals(object obj) => base.Equals(obj as OptionalSingleAk1Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class OptionalSingleAk1MoreDerived : OptionalSingleAk1Derived
        {
            private Guid? _moreDerivedRootId;
            private Root _moreDerivedRoot;

            public Guid? MoreDerivedRootId
            {
                get { return _moreDerivedRootId; }
                set { SetWithNotify(value, ref _moreDerivedRootId); }
            }

            public Root MoreDerivedRoot
            {
                get { return _moreDerivedRoot; }
                set { SetWithNotify(value, ref _moreDerivedRoot); }
            }

            public override bool Equals(object obj) => base.Equals(obj as OptionalSingleAk1MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class OptionalSingleAk2 : NotifyingEntity
        {
            private int _id;
            private Guid _alternateId;
            private Guid? _backId;
            private OptionalSingleAk1 _back;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public Guid AlternateId
            {
                get { return _alternateId; }
                set { SetWithNotify(value, ref _alternateId); }
            }

            public Guid? BackId
            {
                get { return _backId; }
                set { SetWithNotify(value, ref _backId); }
            }

            public OptionalSingleAk1 Back
            {
                get { return _back; }
                set { SetWithNotify(value, ref _back); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as OptionalSingleAk2;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class OptionalSingleComposite2 : NotifyingEntity
        {
            private int _id;
            private Guid _alternateId;
            private int? _backId;
            private OptionalSingleAk1 _back;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public Guid ParentAlternateId
            {
                get { return _alternateId; }
                set { SetWithNotify(value, ref _alternateId); }
            }

            public int? BackId
            {
                get { return _backId; }
                set { SetWithNotify(value, ref _backId); }
            }

            public OptionalSingleAk1 Back
            {
                get { return _back; }
                set { SetWithNotify(value, ref _back); }
            }

            public override bool Equals(object obj)
            {
                var other = obj as OptionalSingleComposite2;
                return _id == other?.Id;
            }

            public override int GetHashCode() => _id;
        }

        protected class OptionalSingleAk2Derived : OptionalSingleAk2
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalSingleAk2Derived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class OptionalSingleAk2MoreDerived : OptionalSingleAk2Derived
        {
            public override bool Equals(object obj) => base.Equals(obj as OptionalSingleAk2MoreDerived);

            public override int GetHashCode() => base.GetHashCode();
        }

        protected class NotifyingEntity : INotifyPropertyChanging, INotifyPropertyChanged
        {
            protected void SetWithNotify<T>(T value, ref T field, [CallerMemberName] string propertyName = "")
            {
                NotifyChanging(propertyName);
                field = value;
                NotifyChanged(propertyName);
            }

            public event PropertyChangingEventHandler PropertyChanging;
            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyChanged(string propertyName)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            private void NotifyChanging(string propertyName)
                => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        protected class GraphUpdatesContext : DbContext
        {
            public GraphUpdatesContext(DbContextOptions options)
                : base(options)
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
            public DbSet<RequiredSingleComposite2> RequiredSingleComposite2s { get; set; }
            public DbSet<RequiredNonPkSingleAk1> RequiredNonPkSingleAk1s { get; set; }
            public DbSet<RequiredNonPkSingleAk2> RequiredNonPkSingleAk2s { get; set; }
            public DbSet<OptionalSingleAk1> OptionalSingleAk1s { get; set; }
            public DbSet<OptionalSingleAk2> OptionalSingleAk2s { get; set; }
            public DbSet<OptionalSingleComposite2> OptionalSingleComposite2s { get; set; }
            public DbSet<RequiredAk1> RequiredAk1s { get; set; }
            public DbSet<OptionalAk1> OptionalAk1s { get; set; }
            public DbSet<RequiredAk2> RequiredAk2s { get; set; }
            public DbSet<RequiredComposite2> RequiredComposite2s { get; set; }
            public DbSet<OptionalAk2> OptionalAk2s { get; set; }
            public DbSet<OptionalComposite2> OptionalComposite2s { get; set; }
        }

        protected GraphUpdatesContext CreateContext()
            => (GraphUpdatesContext)Fixture.CreateContext(TestStore);

        public void Dispose()
            => TestStore.Dispose();

        protected TFixture Fixture { get; }

        protected TTestStore TestStore { get; }

        public abstract class GraphUpdatesFixtureBase
        {
            public readonly Guid RootAK = Guid.NewGuid();

            public abstract TTestStore CreateTestStore();

            public abstract DbContext CreateContext(TTestStore testStore);

            protected virtual void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Root>(b =>
                {
                    b.Property(e => e.AlternateId).ValueGeneratedOnAdd();

                    b.HasMany(e => e.RequiredChildren)
                        .WithOne(e => e.Parent)
                        .HasForeignKey(e => e.ParentId);

                    b.HasMany(e => e.OptionalChildren)
                        .WithOne(e => e.Parent)
                        .HasForeignKey(e => e.ParentId)
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne(e => e.RequiredSingle)
                        .WithOne(e => e.Root)
                        .HasForeignKey<RequiredSingle1>(e => e.Id);

                    b.HasOne(e => e.OptionalSingle)
                        .WithOne(e => e.Root)
                        .HasForeignKey<OptionalSingle1>(e => e.RootId)
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne(e => e.OptionalSingleDerived)
                        .WithOne(e => e.DerivedRoot)
                        .HasForeignKey<OptionalSingle1Derived>(e => e.DerivedRootId)
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne(e => e.OptionalSingleMoreDerived)
                        .WithOne(e => e.MoreDerivedRoot)
                        .HasForeignKey<OptionalSingle1MoreDerived>(e => e.MoreDerivedRootId)
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne(e => e.RequiredNonPkSingle)
                        .WithOne(e => e.Root)
                        .HasForeignKey<RequiredNonPkSingle1>(e => e.RootId);

                    b.HasOne(e => e.RequiredNonPkSingleDerived)
                        .WithOne(e => e.DerivedRoot)
                        .HasForeignKey<RequiredNonPkSingle1Derived>(e => e.DerivedRootId)
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne(e => e.RequiredNonPkSingleMoreDerived)
                        .WithOne(e => e.MoreDerivedRoot)
                        .HasForeignKey<RequiredNonPkSingle1MoreDerived>(e => e.MoreDerivedRootId)
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasMany(e => e.RequiredChildrenAk)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(e => e.AlternateId)
                        .HasForeignKey(e => e.ParentId);

                    b.HasMany(e => e.OptionalChildrenAk)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(e => e.AlternateId)
                        .HasForeignKey(e => e.ParentId)
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne(e => e.RequiredSingleAk)
                        .WithOne(e => e.Root)
                        .HasPrincipalKey<Root>(e => e.AlternateId)
                        .HasForeignKey<RequiredSingleAk1>(e => e.RootId);

                    b.HasOne(e => e.OptionalSingleAk)
                        .WithOne(e => e.Root)
                        .HasPrincipalKey<Root>(e => e.AlternateId)
                        .HasForeignKey<OptionalSingleAk1>(e => e.RootId)
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne(e => e.OptionalSingleAkDerived)
                        .WithOne(e => e.DerivedRoot)
                        .HasPrincipalKey<Root>(e => e.AlternateId)
                        .HasForeignKey<OptionalSingleAk1Derived>(e => e.DerivedRootId)
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne(e => e.OptionalSingleAkMoreDerived)
                        .WithOne(e => e.MoreDerivedRoot)
                        .HasPrincipalKey<Root>(e => e.AlternateId)
                        .HasForeignKey<OptionalSingleAk1MoreDerived>(e => e.MoreDerivedRootId)
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne(e => e.RequiredNonPkSingleAk)
                        .WithOne(e => e.Root)
                        .HasPrincipalKey<Root>(e => e.AlternateId)
                        .HasForeignKey<RequiredNonPkSingleAk1>(e => e.RootId);

                    b.HasOne(e => e.RequiredNonPkSingleAkDerived)
                        .WithOne(e => e.DerivedRoot)
                        .HasPrincipalKey<Root>(e => e.AlternateId)
                        .HasForeignKey<RequiredNonPkSingleAk1Derived>(e => e.DerivedRootId)
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne(e => e.RequiredNonPkSingleAkMoreDerived)
                        .WithOne(e => e.MoreDerivedRoot)
                        .HasPrincipalKey<Root>(e => e.AlternateId)
                        .HasForeignKey<RequiredNonPkSingleAk1MoreDerived>(e => e.MoreDerivedRootId)
                        .OnDelete(DeleteBehavior.Restrict);
                });

                modelBuilder.Entity<Required1>()
                    .HasMany(e => e.Children)
                    .WithOne(e => e.Parent)
                    .HasForeignKey(e => e.ParentId);

                modelBuilder.Entity<Required1Derived>();
                modelBuilder.Entity<Required1MoreDerived>();
                modelBuilder.Entity<Required2Derived>();
                modelBuilder.Entity<Required2MoreDerived>();

                modelBuilder.Entity<Optional1>()
                    .HasMany(e => e.Children)
                    .WithOne(e => e.Parent)
                    .HasForeignKey(e => e.ParentId)
                    .OnDelete(DeleteBehavior.SetNull);

                modelBuilder.Entity<Optional1Derived>();
                modelBuilder.Entity<Optional1MoreDerived>();
                modelBuilder.Entity<Optional2Derived>();
                modelBuilder.Entity<Optional2MoreDerived>();

                modelBuilder.Entity<RequiredSingle1>()
                    .HasOne(e => e.Single)
                    .WithOne(e => e.Back)
                    .HasForeignKey<RequiredSingle2>(e => e.Id);

                modelBuilder.Entity<OptionalSingle1>()
                    .HasOne(e => e.Single)
                    .WithOne(e => e.Back)
                    .HasForeignKey<OptionalSingle2>(e => e.BackId)
                    .OnDelete(DeleteBehavior.SetNull);

                modelBuilder.Entity<OptionalSingle2Derived>();
                modelBuilder.Entity<OptionalSingle2MoreDerived>();

                modelBuilder.Entity<RequiredNonPkSingle1>()
                    .HasOne(e => e.Single)
                    .WithOne(e => e.Back)
                    .HasForeignKey<RequiredNonPkSingle2>(e => e.BackId);

                modelBuilder.Entity<RequiredNonPkSingle2Derived>();
                modelBuilder.Entity<RequiredNonPkSingle2MoreDerived>();

                modelBuilder.Entity<RequiredAk1>(b =>
                {
                    b.Property(e => e.AlternateId)
                        .ValueGeneratedOnAdd();

                    b.HasMany(e => e.Children)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(e => e.AlternateId)
                        .HasForeignKey(e => e.ParentId);

                    b.HasMany(e => e.CompositeChildren)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(e => new { e.Id, e.AlternateId })
                        .HasForeignKey(e => new { e.ParentId, e.ParentAlternateId });
                });

                modelBuilder.Entity<RequiredAk1Derived>();
                modelBuilder.Entity<RequiredAk1MoreDerived>();

                modelBuilder.Entity<OptionalAk1>(b =>
                {
                    b.Property(e => e.AlternateId)
                        .ValueGeneratedOnAdd();

                    b.HasMany(e => e.Children)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(e => e.AlternateId)
                        .HasForeignKey(e => e.ParentId)
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasMany(e => e.CompositeChildren)
                        .WithOne(e => e.Parent)
                        .HasPrincipalKey(e => new { e.Id, e.AlternateId })
                        .HasForeignKey(e => new { e.ParentId, e.ParentAlternateId });
                });

                modelBuilder.Entity<OptionalAk1Derived>();
                modelBuilder.Entity<OptionalAk1MoreDerived>();

                modelBuilder.Entity<RequiredSingleAk1>(b =>
                {
                    b.Property(e => e.AlternateId)
                        .ValueGeneratedOnAdd();

                    b.HasOne(e => e.Single)
                        .WithOne(e => e.Back)
                        .HasForeignKey<RequiredSingleAk2>(e => e.BackId)
                        .HasPrincipalKey<RequiredSingleAk1>(e => e.AlternateId);

                    b.HasOne(e => e.SingleComposite)
                        .WithOne(e => e.Back)
                        .HasForeignKey<RequiredSingleComposite2>(e => new { e.BackId, e.BackAlternateId })
                        .HasPrincipalKey<RequiredSingleAk1>(e => new { e.Id, e.AlternateId });
                });

                modelBuilder.Entity<OptionalSingleAk1>(b =>
                {
                    b.Property(e => e.AlternateId)
                        .ValueGeneratedOnAdd();

                    b.HasOne(e => e.Single)
                        .WithOne(e => e.Back)
                        .HasForeignKey<OptionalSingleAk2>(e => e.BackId)
                        .HasPrincipalKey<OptionalSingleAk1>(e => e.AlternateId)
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne(e => e.SingleComposite)
                        .WithOne(e => e.Back)
                        .HasForeignKey<OptionalSingleComposite2>(e => new { e.BackId, e.ParentAlternateId })
                        .HasPrincipalKey<OptionalSingleAk1>(e => new { e.Id, e.AlternateId });
                });

                modelBuilder.Entity<OptionalSingleAk2Derived>();
                modelBuilder.Entity<OptionalSingleAk2MoreDerived>();

                modelBuilder.Entity<RequiredNonPkSingleAk1>(b =>
                {
                    b.Property(e => e.AlternateId)
                        .ValueGeneratedOnAdd();

                    b.HasOne(e => e.Single)
                        .WithOne(e => e.Back)
                        .HasForeignKey<RequiredNonPkSingleAk2>(e => e.BackId)
                        .HasPrincipalKey<RequiredNonPkSingleAk1>(e => e.AlternateId);
                });

                modelBuilder.Entity<RequiredAk2>()
                    .Property(e => e.AlternateId)
                    .ValueGeneratedOnAdd();

                modelBuilder.Entity<RequiredAk2Derived>();
                modelBuilder.Entity<RequiredAk2MoreDerived>();

                modelBuilder.Entity<OptionalAk2>()
                    .Property(e => e.AlternateId)
                    .ValueGeneratedOnAdd();

                modelBuilder.Entity<OptionalAk2Derived>();
                modelBuilder.Entity<OptionalAk2MoreDerived>();

                modelBuilder.Entity<RequiredSingleAk2>()
                    .Property(e => e.AlternateId)
                    .ValueGeneratedOnAdd();

                modelBuilder.Entity<RequiredNonPkSingleAk2>()
                    .Property(e => e.AlternateId)
                    .ValueGeneratedOnAdd();

                modelBuilder.Entity<RequiredNonPkSingleAk2Derived>();
                modelBuilder.Entity<RequiredNonPkSingleAk2MoreDerived>();

                modelBuilder.Entity<OptionalSingleAk2>()
                    .Property(e => e.AlternateId)
                    .ValueGeneratedOnAdd();
            }

            protected virtual object CreateFullGraph()
                => new Root
                {
                    AlternateId = RootAK,
                    RequiredChildren = new ObservableHashSet<Required1>(ReferenceEqualityComparer.Instance)
                    {
                        new Required1
                        {
                            Children = new ObservableHashSet<Required2>(ReferenceEqualityComparer.Instance)
                            {
                                new Required2(),
                                new Required2()
                            }
                        },
                        new Required1
                        {
                            Children = new ObservableHashSet<Required2>(ReferenceEqualityComparer.Instance)
                            {
                                new Required2(),
                                new Required2()
                            }
                        }
                    },
                    OptionalChildren = new ObservableHashSet<Optional1>(ReferenceEqualityComparer.Instance)
                    {
                        new Optional1
                        {
                            Children = new ObservableHashSet<Optional2>(ReferenceEqualityComparer.Instance)
                            {
                                new Optional2(),
                                new Optional2()
                            }
                        },
                        new Optional1
                        {
                            Children = new ObservableHashSet<Optional2>(ReferenceEqualityComparer.Instance)
                            {
                                new Optional2(),
                                new Optional2()
                            }
                        }
                    },
                    RequiredSingle = new RequiredSingle1
                    {
                        Single = new RequiredSingle2()
                    },
                    OptionalSingle = new OptionalSingle1
                    {
                        Single = new OptionalSingle2()
                    },
                    OptionalSingleDerived = new OptionalSingle1Derived
                    {
                        Single = new OptionalSingle2Derived()
                    },
                    OptionalSingleMoreDerived = new OptionalSingle1MoreDerived
                    {
                        Single = new OptionalSingle2MoreDerived()
                    },
                    RequiredNonPkSingle = new RequiredNonPkSingle1
                    {
                        Single = new RequiredNonPkSingle2()
                    },
                    RequiredNonPkSingleDerived = new RequiredNonPkSingle1Derived
                    {
                        Single = new RequiredNonPkSingle2Derived(),
                        Root = new Root()
                    },
                    RequiredNonPkSingleMoreDerived = new RequiredNonPkSingle1MoreDerived
                    {
                        Single = new RequiredNonPkSingle2MoreDerived(),
                        Root = new Root(),
                        DerivedRoot = new Root()
                    },
                    RequiredChildrenAk = new ObservableHashSet<RequiredAk1>(ReferenceEqualityComparer.Instance)
                    {
                        new RequiredAk1
                        {
                            AlternateId = Guid.NewGuid(),
                            Children = new ObservableHashSet<RequiredAk2>(ReferenceEqualityComparer.Instance)
                            {
                                new RequiredAk2 { AlternateId = Guid.NewGuid() },
                                new RequiredAk2 { AlternateId = Guid.NewGuid() }
                            },
                            CompositeChildren = new ObservableHashSet<RequiredComposite2>(ReferenceEqualityComparer.Instance)
                            {
                                new RequiredComposite2(),
                                new RequiredComposite2()
                            }
                        },
                        new RequiredAk1
                        {
                            AlternateId = Guid.NewGuid(),
                            Children = new ObservableHashSet<RequiredAk2>(ReferenceEqualityComparer.Instance)
                            {
                                new RequiredAk2 { AlternateId = Guid.NewGuid() },
                                new RequiredAk2 { AlternateId = Guid.NewGuid() }
                            },
                            CompositeChildren = new ObservableHashSet<RequiredComposite2>(ReferenceEqualityComparer.Instance)
                            {
                                new RequiredComposite2(),
                                new RequiredComposite2()
                            }
                        }
                    },
                    OptionalChildrenAk = new ObservableHashSet<OptionalAk1>(ReferenceEqualityComparer.Instance)
                    {
                        new OptionalAk1
                        {
                            AlternateId = Guid.NewGuid(),
                            Children = new ObservableHashSet<OptionalAk2>(ReferenceEqualityComparer.Instance)
                            {
                                new OptionalAk2 { AlternateId = Guid.NewGuid() },
                                new OptionalAk2 { AlternateId = Guid.NewGuid() }
                            },
                            CompositeChildren = new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance)
                            {
                                new OptionalComposite2(),
                                new OptionalComposite2()
                            }
                        },
                        new OptionalAk1
                        {
                            AlternateId = Guid.NewGuid(),
                            Children = new ObservableHashSet<OptionalAk2>(ReferenceEqualityComparer.Instance)
                            {
                                new OptionalAk2 { AlternateId = Guid.NewGuid() },
                                new OptionalAk2 { AlternateId = Guid.NewGuid() }
                            },
                            CompositeChildren = new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance)
                            {
                                new OptionalComposite2(),
                                new OptionalComposite2()
                            }
                        }
                    },
                    RequiredSingleAk = new RequiredSingleAk1
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new RequiredSingleAk2 { AlternateId = Guid.NewGuid() },
                        SingleComposite = new RequiredSingleComposite2()
                    },
                    OptionalSingleAk = new OptionalSingleAk1
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new OptionalSingleAk2 { AlternateId = Guid.NewGuid() },
                        SingleComposite = new OptionalSingleComposite2()
                    },
                    OptionalSingleAkDerived = new OptionalSingleAk1Derived
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new OptionalSingleAk2Derived { AlternateId = Guid.NewGuid() }
                    },
                    OptionalSingleAkMoreDerived = new OptionalSingleAk1MoreDerived
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new OptionalSingleAk2MoreDerived { AlternateId = Guid.NewGuid() }
                    },
                    RequiredNonPkSingleAk = new RequiredNonPkSingleAk1
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new RequiredNonPkSingleAk2 { AlternateId = Guid.NewGuid() }
                    },
                    RequiredNonPkSingleAkDerived = new RequiredNonPkSingleAk1Derived
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new RequiredNonPkSingleAk2Derived { AlternateId = Guid.NewGuid() },
                        Root = new Root()
                    },
                    RequiredNonPkSingleAkMoreDerived = new RequiredNonPkSingleAk1MoreDerived
                    {
                        AlternateId = Guid.NewGuid(),
                        Single = new RequiredNonPkSingleAk2MoreDerived { AlternateId = Guid.NewGuid() },
                        Root = new Root(),
                        DerivedRoot = new Root()
                    }
                };

            protected virtual void Seed(DbContext context)
            {
                var tracker = new KeyValueEntityTracker();

                context.ChangeTracker.TrackGraph(CreateFullGraph(), e => tracker.TrackEntity(e.Entry));
                context.SaveChanges();
            }

            public class KeyValueEntityTracker
            {
                public virtual void TrackEntity(EntityEntry entry)
                    => entry.GetInfrastructure()
                        .SetEntityState(DetermineState(entry), acceptChanges: true);

                public virtual EntityState DetermineState(EntityEntry entry)
                    => entry.IsKeySet ? EntityState.Unchanged : EntityState.Added;
            }
        }
    }
}
