// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

// ReSharper disable AccessToDisposedClosure
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable InconsistentNaming
// ReSharper disable AccessToModifiedClosure
namespace Microsoft.EntityFrameworkCore
{
    public abstract partial class GraphUpdatesTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : GraphUpdatesTestBase<TFixture>.GraphUpdatesFixtureBase, new()
    {
        protected GraphUpdatesTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual DbUpdateException New_FK_is_not_cleared_on_old_dependent_delete(bool loadNewParent)
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            var childId = 0;
            int? newFk = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var removed = context.Set<Optional1>().OrderBy(e => e.Id).First();
                    var child = context.Set<Optional2>().OrderBy(e => e.Id).First(e => e.ParentId == removed.Id);

                    removedId = removed.Id;
                    childId = child.Id;

                    newFk = context.Set<Optional1>().AsNoTracking().Single(e => e.Id != removed.Id).Id;

                    var newParent = loadNewParent ? context.Set<Optional1>().Find(newFk) : null;

                    child.ParentId = newFk;

                    context.Remove(removed);

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                        Assert.Equal(newFk, child.ParentId);

                        if (loadNewParent)
                        {
                            Assert.Same(newParent, child.Parent);
                            Assert.Contains(child, newParent.Children);
                        }
                        else
                        {
                            Assert.Null((child.Parent));
                        }
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        Assert.Null(context.Set<Optional1>().Find(removedId));

                        var child = context.Set<Optional2>().Find(childId);
                        var newParent = loadNewParent ? context.Set<Optional1>().Find(newFk) : null;

                        Assert.Equal(newFk, child.ParentId);

                        if (loadNewParent)
                        {
                            Assert.Same(newParent, child.Parent);
                            Assert.Contains(child, newParent.Children);
                        }
                        else
                        {
                            Assert.Null((child.Parent));
                        }
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Optional_One_to_one_relationships_are_one_to_one()
        {
            DbUpdateException updateException = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = context.Set<Root>().Single(IsTheRoot);

                    root.OptionalSingle = new OptionalSingle1();

                    updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_One_to_one_relationships_are_one_to_one()
        {
            DbUpdateException updateException = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = context.Set<Root>().Single(IsTheRoot);

                    root.RequiredSingle = new RequiredSingle1();

                    updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Optional_One_to_one_with_AK_relationships_are_one_to_one()
        {
            DbUpdateException updateException = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = context.Set<Root>().Single(IsTheRoot);

                    root.OptionalSingleAk = new OptionalSingleAk1();

                    updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_One_to_one_with_AK_relationships_are_one_to_one()
        {
            DbUpdateException updateException = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = context.Set<Root>().Single(IsTheRoot);

                    root.RequiredSingleAk = new RequiredSingleAk1();

                    updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                });

            return updateException;
        }

        [Fact]
        public virtual void No_fixup_to_Deleted_entities()
        {
            using (var context = CreateContext())
            {
                var root = LoadOptionalGraph(context);
                var existing = root.OptionalChildren.OrderBy(e => e.Id).First();

                existing.Parent = null;
                existing.ParentId = null;
                ((ICollection<Optional1>)root.OptionalChildren).Remove(existing);

                context.Entry(existing).State = EntityState.Deleted;

                var queried = context.Set<Optional1>().ToList();

                Assert.Null(existing.Parent);
                Assert.Null(existing.ParentId);
                Assert.Equal(1, root.OptionalChildren.Count());
                Assert.DoesNotContain(existing, root.OptionalChildren);

                Assert.Equal(2, queried.Count);
                Assert.Contains(existing, queried);
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
            Root root = null;
            IReadOnlyList<EntityEntry> entries = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    if (useExistingEntities)
                    {
                        context.AddRange(new1, new1d, new1dd, new2a, new2d, new2dd, new2b);
                        context.SaveChanges();
                    }
                },
                context =>
                {
                    root = LoadOptionalGraph(context);
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

                    entries = context.ChangeTracker.Entries().ToList();
                },
                context =>
                {
                    var loadedRoot = LoadOptionalGraph(context);

                    AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                    AssertKeys(root, loadedRoot);
                    AssertNavigations(loadedRoot);
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
        public virtual void Save_required_many_to_one_dependents(ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            var newRoot = new Root();
            var new1 = new Required1
            {
                Parent = newRoot
            };
            var new1d = new Required1Derived
            {
                Parent = newRoot
            };
            var new1dd = new Required1MoreDerived
            {
                Parent = newRoot
            };
            var new2a = new Required2
            {
                Parent = new1
            };
            var new2b = new Required2
            {
                Parent = new1
            };
            var new2d = new Required2Derived
            {
                Parent = new1
            };
            var new2dd = new Required2MoreDerived
            {
                Parent = new1
            };
            Root root = null;
            IReadOnlyList<EntityEntry> entries = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    if (useExistingEntities)
                    {
                        context.AddRange(newRoot, new1, new1d, new1dd, new2a, new2d, new2dd, new2b);
                        context.SaveChanges();
                    }
                },
                context =>
                {
                    root = LoadRequiredGraph(context);
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

                    entries = context.ChangeTracker.Entries().ToList();
                },
                context =>
                {
                    var loadedRoot = LoadRequiredGraph(context);

                    AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                    AssertKeys(root, loadedRoot);
                    AssertNavigations(loadedRoot);
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
        public virtual void Save_removed_optional_many_to_one_dependents(ChangeMechanism changeMechanism)
        {
            Root root = null;
            ExecuteWithStrategyInTransaction(
                context =>
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

                    Assert.True(context.ChangeTracker.HasChanges());

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.DoesNotContain(removed1, root.OptionalChildren);
                    Assert.DoesNotContain(removed2, childCollection);

                    Assert.Null(removed1.Parent);
                    Assert.Null(removed2.Parent);
                    Assert.Null(removed1.ParentId);
                    Assert.Null(removed2.ParentId);
                },
                context =>
                {
                    if ((changeMechanism & ChangeMechanism.Fk) == 0)
                    {
                        var loadedRoot = LoadOptionalGraph(context);

                        AssertKeys(root, loadedRoot);
                        AssertNavigations(loadedRoot);

                        Assert.Equal(1, loadedRoot.OptionalChildren.Count());
                        Assert.Equal(1, loadedRoot.OptionalChildren.First().Children.Count());
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
        public virtual void Save_removed_required_many_to_one_dependents(ChangeMechanism changeMechanism)
        {
            var removed1Id = 0;
            var removed2Id = 0;
            List<int> removed1ChildrenIds = null;

            ExecuteWithStrategyInTransaction(
                context =>
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

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        Assert.Equal(
                            CoreStrings.RelationshipConceptualNullSensitive(nameof(Required1), nameof(Required2), "{Id: 2}"),
                            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadRequiredGraph(context);

                        AssertNavigations(root);

                        Assert.Equal(1, root.RequiredChildren.Count());
                        Assert.DoesNotContain(removed1Id, root.RequiredChildren.Select(e => e.Id));

                        Assert.Empty(context.Set<Required1>().Where(e => e.Id == removed1Id));
                        Assert.Empty(context.Set<Required2>().Where(e => e.Id == removed2Id));
                        Assert.Empty(context.Set<Required2>().Where(e => removed1ChildrenIds.Contains(e.Id)));
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
        public virtual void Save_changed_optional_one_to_one(ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            var new2 = new OptionalSingle2();
            var new2d = new OptionalSingle2Derived();
            var new2dd = new OptionalSingle2MoreDerived();
            var new1 = new OptionalSingle1
            {
                Single = new2
            };
            var new1d = new OptionalSingle1Derived
            {
                Single = new2d
            };
            var new1dd = new OptionalSingle1MoreDerived
            {
                Single = new2dd
            };
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
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)ChangeMechanism.Fk)]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk))]
        [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent))]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk))]
        public virtual DbUpdateException Save_required_one_to_one_changed_by_reference(ChangeMechanism changeMechanism)
        {
            DbUpdateException updateException = null;

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
            var new1 = new RequiredSingle1
            {
                Single = new2
            };

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = LoadRequiredGraph(context);

                    root.RequiredSingle = null;

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
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
                    if (!Fixture.ForceRestrict)
                    {
                        var loadedRoot = LoadRequiredGraph(context);

                        AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                        AssertKeys(oldRoot, loadedRoot);
                        AssertNavigations(loadedRoot);
                    }
                });

            return updateException;
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
            var new1 = new RequiredNonPkSingle1
            {
                Single = new2
            };
            var new1d = new RequiredNonPkSingle1Derived
            {
                Single = new2d,
                Root = new Root()
            };
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

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        Assert.Equal(
                            CoreStrings.RelationshipConceptualNullSensitive(nameof(Root), nameof(RequiredNonPkSingle1), "{Id: 1}"),
                            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                    }
                    else
                    {
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
                    if (!Fixture.ForceRestrict)
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
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)ChangeMechanism.Fk)]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk))]
        [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent))]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk))]
        public virtual void Sever_optional_one_to_one(ChangeMechanism changeMechanism)
        {
            Root root = null;
            OptionalSingle1 old1 = null;
            OptionalSingle2 old2 = null;
            ExecuteWithStrategyInTransaction(
                context =>
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
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
        public virtual DbUpdateException Sever_required_one_to_one(ChangeMechanism changeMechanism)
        {
            DbUpdateException updateException = null;

            Root root = null;
            RequiredSingle1 old1 = null;
            RequiredSingle2 old2 = null;
            ExecuteWithStrategyInTransaction(
                context =>
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
                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Null(old1.Root);
                        Assert.Null(old2.Back);
                        Assert.Equal(old1.Id, old2.Id);
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var loadedRoot = LoadRequiredGraph(context);

                        AssertKeys(root, loadedRoot);
                        AssertPossiblyNullNavigations(loadedRoot);

                        Assert.False(context.Set<RequiredSingle1>().Any(e => e.Id == old1.Id));
                        Assert.False(context.Set<RequiredSingle2>().Any(e => e.Id == old2.Id));
                    }
                });

            return updateException;
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
        public virtual void Sever_required_non_PK_one_to_one(ChangeMechanism changeMechanism)
        {
            Root root = null;
            RequiredNonPkSingle1 old1 = null;
            RequiredNonPkSingle2 old2 = null;
            ExecuteWithStrategyInTransaction(
                context =>
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
                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        Assert.Equal(
                            CoreStrings.RelationshipConceptualNullSensitive(nameof(Root), nameof(RequiredNonPkSingle1), "{Id: 1}"),
                            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Null(old1.Root);
                        Assert.Null(old2.Back);
                        Assert.Equal(old1.Id, old2.BackId);
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
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
        public virtual void Reparent_required_non_PK_one_to_one(ChangeMechanism changeMechanism, bool useExistingRoot)
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
        public virtual void Reparent_to_different_one_to_many(ChangeMechanism changeMechanism, bool useExistingParent)
        {
            Root root = null;
            IReadOnlyList<EntityEntry> entries = null;
            var compositeCount = 0;
            OptionalAk1 oldParent = null;
            OptionalComposite2 oldComposite1 = null;
            OptionalComposite2 oldComposite2 = null;
            Optional1 newParent = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    if (!useExistingParent)
                    {
                        newParent = new Optional1
                        {
                            CompositeChildren = new ObservableHashSet<OptionalComposite2>(ReferenceEqualityComparer.Instance)
                        };

                        context.Set<Optional1>().Add(newParent);
                        context.SaveChanges();
                    }
                },
                context =>
                {
                    root = LoadOptionalOneToManyGraph(context);

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
                        newParent = context.Set<Optional1>().Single(e => e.Id == newParent.Id);
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

                    entries = context.ChangeTracker.Entries().ToList();

                    Assert.Equal(compositeCount, context.Set<OptionalComposite2>().Count());
                },
                context =>
                {
                    if ((changeMechanism & ChangeMechanism.Fk) == 0)
                    {
                        var loadedRoot = LoadOptionalOneToManyGraph(context);

                        AssertKeys(root, loadedRoot);
                        AssertNavigations(loadedRoot);

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

                        AssertEntries(entries, context.ChangeTracker.Entries().ToList());

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
        public virtual void Reparent_one_to_many_overlapping(ChangeMechanism changeMechanism, bool useExistingParent)
        {
            Root root = null;
            IReadOnlyList<EntityEntry> entries = null;
            var childCount = 0;
            RequiredComposite1 oldParent = null;
            OptionalOverlaping2 oldChild1 = null;
            OptionalOverlaping2 oldChild2 = null;
            RequiredComposite1 newParent = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    if (!useExistingParent)
                    {
                        newParent = new RequiredComposite1
                        {
                            Id = 3,
                            Parent = context.Set<Root>().Single(IsTheRoot),
                            CompositeChildren = new ObservableHashSet<OptionalOverlaping2>(ReferenceEqualityComparer.Instance)
                            {
                                new OptionalOverlaping2
                                {
                                    Id = 5
                                },
                                new OptionalOverlaping2
                                {
                                    Id = 6
                                }
                            }
                        };

                        context.Set<RequiredComposite1>().Add(newParent);
                        context.SaveChanges();
                    }
                },
                context =>
                {
                    root = LoadRequiredCompositeGraph(context);

                    childCount = context.Set<OptionalOverlaping2>().Count();

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
                        newParent = context.Set<RequiredComposite1>().Single(e => e.Id == newParent.Id);
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

                    entries = context.ChangeTracker.Entries().ToList();

                    Assert.Equal(childCount, context.Set<OptionalOverlaping2>().Count());
                },
                context =>
                {
                    var loadedRoot = LoadRequiredCompositeGraph(context);

                    AssertKeys(root, loadedRoot);
                    AssertNavigations(loadedRoot);

                    oldParent = context.Set<RequiredComposite1>().Single(e => e.Id == oldParent.Id);
                    newParent = context.Set<RequiredComposite1>().Single(e => e.Id == newParent.Id);

                    oldChild1 = context.Set<OptionalOverlaping2>().Single(e => e.Id == oldChild1.Id);
                    oldChild2 = context.Set<OptionalOverlaping2>().Single(e => e.Id == oldChild2.Id);

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

                    Assert.Equal(childCount, context.Set<OptionalOverlaping2>().Count());
                });
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)ChangeMechanism.Fk)]
        public virtual void Mark_modified_one_to_many_overlapping(ChangeMechanism changeMechanism)
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = LoadRequiredCompositeGraph(context);
                    var parent = root.RequiredCompositeChildren.OrderBy(e => e.Id).First();
                    var child = parent.CompositeChildren.OrderBy(e => e.Id).First();

                    var childCount = context.Set<OptionalOverlaping2>().Count();

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

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Same(child, parent.CompositeChildren.OrderBy(e => e.Id).First());
                    Assert.Same(parent, child.Parent);
                    Assert.Equal(parent.Id, child.ParentId);
                    Assert.Equal(parent.ParentAlternateId, child.ParentAlternateId);
                    Assert.Equal(root.AlternateId, child.ParentAlternateId);
                    Assert.Same(root, child.Root);

                    Assert.Equal(childCount, context.Set<OptionalOverlaping2>().Count());
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
        public virtual void Save_optional_many_to_one_dependents_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            var new1 = new OptionalAk1
            {
                AlternateId = Guid.NewGuid()
            };
            var new1d = new OptionalAk1Derived
            {
                AlternateId = Guid.NewGuid()
            };
            var new1dd = new OptionalAk1MoreDerived
            {
                AlternateId = Guid.NewGuid()
            };
            var new2a = new OptionalAk2
            {
                AlternateId = Guid.NewGuid()
            };
            var new2b = new OptionalAk2
            {
                AlternateId = Guid.NewGuid()
            };
            var new2ca = new OptionalComposite2();
            var new2cb = new OptionalComposite2();
            var new2d = new OptionalAk2Derived
            {
                AlternateId = Guid.NewGuid()
            };
            var new2dd = new OptionalAk2MoreDerived
            {
                AlternateId = Guid.NewGuid()
            };
            Root root = null;
            IReadOnlyList<EntityEntry> entries = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    if (useExistingEntities)
                    {
                        context.AddRange(new1, new1d, new1dd, new2a, new2d, new2dd, new2b, new2ca, new2cb);
                        context.SaveChanges();
                    }
                },
                context =>
                {
                    root = LoadOptionalAkGraph(context);
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

                    entries = context.ChangeTracker.Entries().ToList();
                },
                context =>
                {
                    var loadedRoot = LoadOptionalAkGraph(context);

                    AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                    AssertKeys(root, loadedRoot);
                    AssertNavigations(loadedRoot);
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
        public virtual void Save_required_many_to_one_dependents_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            var newRoot = new Root
            {
                AlternateId = Guid.NewGuid()
            };
            var new1 = new RequiredAk1
            {
                AlternateId = Guid.NewGuid(),
                Parent = newRoot
            };
            var new1d = new RequiredAk1Derived
            {
                AlternateId = Guid.NewGuid(),
                Parent = newRoot
            };
            var new1dd = new RequiredAk1MoreDerived
            {
                AlternateId = Guid.NewGuid(),
                Parent = newRoot
            };
            var new2a = new RequiredAk2
            {
                AlternateId = Guid.NewGuid(),
                Parent = new1
            };
            var new2b = new RequiredAk2
            {
                AlternateId = Guid.NewGuid(),
                Parent = new1
            };
            var new2ca = new RequiredComposite2
            {
                Parent = new1
            };
            var new2cb = new RequiredComposite2
            {
                Parent = new1
            };
            var new2d = new RequiredAk2Derived
            {
                AlternateId = Guid.NewGuid(),
                Parent = new1
            };
            var new2dd = new RequiredAk2MoreDerived
            {
                AlternateId = Guid.NewGuid(),
                Parent = new1
            };
            Root root = null;
            IReadOnlyList<EntityEntry> entries = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    if (useExistingEntities)
                    {
                        context.AddRange(newRoot, new1, new1d, new1dd, new2a, new2d, new2dd, new2b, new2ca, new2cb);
                        context.SaveChanges();
                    }
                },
                context =>
                {
                    root = LoadRequiredAkGraph(context);
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

                    entries = context.ChangeTracker.Entries().ToList();
                },
                context =>
                {
                    var loadedRoot = LoadRequiredAkGraph(context);

                    AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                    AssertKeys(root, loadedRoot);
                    AssertNavigations(loadedRoot);
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
        public virtual void Save_removed_optional_many_to_one_dependents_with_alternate_key(ChangeMechanism changeMechanism)
        {
            Root root = null;
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    root = LoadOptionalAkGraph(context);

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
                },
                context =>
                {
                    if ((changeMechanism & ChangeMechanism.Fk) == 0)
                    {
                        var loadedRoot = LoadOptionalAkGraph(context);

                        AssertKeys(root, loadedRoot);
                        AssertNavigations(loadedRoot);

                        Assert.Equal(1, loadedRoot.OptionalChildrenAk.Count());
                        Assert.Equal(1, loadedRoot.OptionalChildrenAk.OrderBy(c => c.Id).First().Children.Count());
                    }
                });
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
        public virtual void Save_removed_required_many_to_one_dependents_with_alternate_key(ChangeMechanism changeMechanism)
        {
            Root root = null;
            RequiredAk2 removed2 = null;
            RequiredComposite2 removed2c = null;
            RequiredAk1 removed1 = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    root = LoadRequiredAkGraph(context);

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

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        Add(root.RequiredChildrenAk, removed1);
                        Assert.Equal(
                            CoreStrings.RelationshipConceptualNullSensitive(nameof(RequiredAk1), nameof(RequiredAk2), "{Id: 1}"),
                            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.DoesNotContain(removed1, root.RequiredChildrenAk);
                        Assert.DoesNotContain(removed2, childCollection);
                        Assert.DoesNotContain(removed2c, childCompositeCollection);

                        Assert.Null(removed1.Parent);
                        Assert.Null(removed2.Parent);
                        Assert.Null(removed2c.Parent);
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var loadedRoot = LoadRequiredAkGraph(context);

                        AssertKeys(root, loadedRoot);
                        AssertNavigations(loadedRoot);

                        Assert.False(context.Set<RequiredAk1>().Any(e => e.Id == removed1.Id));
                        Assert.False(context.Set<RequiredAk2>().Any(e => e.Id == removed2.Id));
                        Assert.False(context.Set<RequiredComposite2>().Any(e => e.Id == removed2c.Id));

                        Assert.Equal(1, loadedRoot.RequiredChildrenAk.Count());
                        Assert.Equal(1, loadedRoot.RequiredChildrenAk.OrderBy(c => c.Id).First().Children.Count());
                        Assert.Equal(1, loadedRoot.RequiredChildrenAk.OrderBy(c => c.Id).First().CompositeChildren.Count());
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
        public virtual void Save_changed_optional_one_to_one_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            var new2 = new OptionalSingleAk2
            {
                AlternateId = Guid.NewGuid()
            };
            var new2d = new OptionalSingleAk2Derived
            {
                AlternateId = Guid.NewGuid()
            };
            var new2dd = new OptionalSingleAk2MoreDerived
            {
                AlternateId = Guid.NewGuid()
            };
            var new2c = new OptionalSingleComposite2();
            var new1 = new OptionalSingleAk1
            {
                AlternateId = Guid.NewGuid(),
                Single = new2,
                SingleComposite = new2c
            };
            var new1d = new OptionalSingleAk1Derived
            {
                AlternateId = Guid.NewGuid(),
                Single = new2d
            };
            var new1dd = new OptionalSingleAk1MoreDerived
            {
                AlternateId = Guid.NewGuid(),
                Single = new2dd
            };
            Root root = null;
            IReadOnlyList<EntityEntry> entries = null;
            OptionalSingleAk1 old1 = null;
            OptionalSingleAk1Derived old1d = null;
            OptionalSingleAk1MoreDerived old1dd = null;
            OptionalSingleAk2 old2 = null;
            OptionalSingleComposite2 old2c = null;
            OptionalSingleAk2Derived old2d = null;
            OptionalSingleAk2MoreDerived old2dd = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    if (useExistingEntities)
                    {
                        context.AddRange(new1, new1d, new1dd, new2, new2d, new2dd, new2c);
                        context.SaveChanges();
                    }
                },
                context =>
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

                    entries = context.ChangeTracker.Entries().ToList();
                },
                context =>
                {
                    var loadedRoot = LoadOptionalAkGraph(context);

                    AssertKeys(root, loadedRoot);
                    AssertNavigations(loadedRoot);

                    var loaded1 = context.Set<OptionalSingleAk1>().Single(e => e.Id == old1.Id);
                    var loaded1d = context.Set<OptionalSingleAk1>().Single(e => e.Id == old1d.Id);
                    var loaded1dd = context.Set<OptionalSingleAk1>().Single(e => e.Id == old1dd.Id);
                    var loaded2 = context.Set<OptionalSingleAk2>().Single(e => e.Id == old2.Id);
                    var loaded2d = context.Set<OptionalSingleAk2>().Single(e => e.Id == old2d.Id);
                    var loaded2dd = context.Set<OptionalSingleAk2>().Single(e => e.Id == old2dd.Id);
                    var loaded2c = context.Set<OptionalSingleComposite2>().Single(e => e.Id == old2c.Id);

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
        public virtual void Save_changed_optional_one_to_one_with_alternate_key_in_store()
        {
            var new2 = new OptionalSingleAk2
            {
                AlternateId = Guid.NewGuid()
            };
            var new2d = new OptionalSingleAk2Derived
            {
                AlternateId = Guid.NewGuid()
            };
            var new2dd = new OptionalSingleAk2MoreDerived
            {
                AlternateId = Guid.NewGuid()
            };
            var new2c = new OptionalSingleComposite2();
            var new1 = new OptionalSingleAk1
            {
                AlternateId = Guid.NewGuid(),
                Single = new2,
                SingleComposite = new2c
            };
            var new1d = new OptionalSingleAk1Derived
            {
                AlternateId = Guid.NewGuid(),
                Single = new2d
            };
            var new1dd = new OptionalSingleAk1MoreDerived
            {
                AlternateId = Guid.NewGuid(),
                Single = new2dd
            };
            Root root = null;
            IReadOnlyList<EntityEntry> entries = null;
            OptionalSingleAk1 old1 = null;
            OptionalSingleAk1Derived old1d = null;
            OptionalSingleAk1MoreDerived old1dd = null;
            OptionalSingleAk2 old2 = null;
            OptionalSingleComposite2 old2c = null;
            OptionalSingleAk2Derived old2d = null;
            OptionalSingleAk2MoreDerived old2dd = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    root = LoadOptionalAkGraph(context);

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
                        var root2 = LoadOptionalAkGraph(context2);

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

                    entries = context.ChangeTracker.Entries().ToList();
                },
                context =>
                {
                    var loadedRoot = LoadOptionalAkGraph(context);

                    AssertKeys(root, loadedRoot);
                    AssertNavigations(loadedRoot);

                    var loaded1 = context.Set<OptionalSingleAk1>().Single(e => e.Id == old1.Id);
                    var loaded1d = context.Set<OptionalSingleAk1>().Single(e => e.Id == old1d.Id);
                    var loaded1dd = context.Set<OptionalSingleAk1>().Single(e => e.Id == old1dd.Id);
                    var loaded2 = context.Set<OptionalSingleAk2>().Single(e => e.Id == old2.Id);
                    var loaded2d = context.Set<OptionalSingleAk2>().Single(e => e.Id == old2d.Id);
                    var loaded2dd = context.Set<OptionalSingleAk2>().Single(e => e.Id == old2dd.Id);
                    var loaded2c = context.Set<OptionalSingleComposite2>().Single(e => e.Id == old2c.Id);

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
        [InlineData((int)ChangeMechanism.Dependent, false)]
        [InlineData((int)ChangeMechanism.Dependent, true)]
        [InlineData((int)ChangeMechanism.Principal, false)]
        [InlineData((int)ChangeMechanism.Principal, true)]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), false)]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent), true)]
        public virtual void Save_required_one_to_one_changed_by_reference_with_alternate_key(
            ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            var new2 = new RequiredSingleAk2
            {
                AlternateId = Guid.NewGuid()
            };
            var new2c = new RequiredSingleComposite2();
            var new1 = new RequiredSingleAk1
            {
                AlternateId = Guid.NewGuid(),
                Single = new2,
                SingleComposite = new2c
            };
            var newRoot = new Root
            {
                AlternateId = Guid.NewGuid(),
                RequiredSingleAk = new1
            };
            Root root = null;
            IReadOnlyList<EntityEntry> entries = null;
            RequiredSingleAk1 old1 = null;
            RequiredSingleAk2 old2 = null;
            RequiredSingleComposite2 old2c = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    if (useExistingEntities)
                    {
                        context.AddRange(newRoot, new1, new2, new2c);
                        context.SaveChanges();
                    }
                },
                context =>
                {
                    root = LoadRequiredAkGraph(context);

                    old1 = root.RequiredSingleAk;
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

                    if (Fixture.ForceRestrict)
                    {
                        Assert.Equal(
                            CoreStrings.RelationshipConceptualNullSensitive(nameof(Root), nameof(RequiredSingleAk1), "{Id: 1}"),
                            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                    }
                    else
                    {
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

                        entries = context.ChangeTracker.Entries().ToList();
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var loadedRoot = LoadRequiredAkGraph(context);

                        AssertEntries(entries, context.ChangeTracker.Entries().ToList());
                        AssertKeys(root, loadedRoot);
                        AssertNavigations(loadedRoot);

                        Assert.False(context.Set<RequiredSingleAk1>().Any(e => e.Id == old1.Id));
                        Assert.False(context.Set<RequiredSingleAk2>().Any(e => e.Id == old2.Id));
                        Assert.False(context.Set<RequiredSingleComposite2>().Any(e => e.Id == old2c.Id));
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
        public virtual void Save_required_non_PK_one_to_one_changed_by_reference_with_alternate_key(
            ChangeMechanism changeMechanism, bool useExistingEntities)
        {
            var new2 = new RequiredNonPkSingleAk2
            {
                AlternateId = Guid.NewGuid()
            };
            var new2d = new RequiredNonPkSingleAk2Derived
            {
                AlternateId = Guid.NewGuid()
            };
            var new2dd = new RequiredNonPkSingleAk2MoreDerived
            {
                AlternateId = Guid.NewGuid()
            };
            var new1 = new RequiredNonPkSingleAk1
            {
                AlternateId = Guid.NewGuid(),
                Single = new2
            };
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
                    root = LoadRequiredNonPkAkGraph(context);

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

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        Assert.Equal(
                            CoreStrings.RelationshipConceptualNullSensitive(nameof(Root), nameof(RequiredNonPkSingleAk1), "{Id: 1}"),
                            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                    }
                    else
                    {
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

                        entries = context.ChangeTracker.Entries().ToList();
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var loadedRoot = LoadRequiredNonPkAkGraph(context);

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
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)ChangeMechanism.Fk)]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Fk))]
        [InlineData((int)(ChangeMechanism.Fk | ChangeMechanism.Dependent))]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent | ChangeMechanism.Fk))]
        public virtual void Sever_optional_one_to_one_with_alternate_key(ChangeMechanism changeMechanism)
        {
            Root root = null;
            OptionalSingleAk1 old1 = null;
            OptionalSingleAk2 old2 = null;
            OptionalSingleComposite2 old2c = null;
            ExecuteWithStrategyInTransaction(
                context =>
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
                },
                context =>
                {
                    if ((changeMechanism & ChangeMechanism.Fk) == 0)
                    {
                        var loadedRoot = LoadOptionalAkGraph(context);

                        AssertKeys(root, loadedRoot);
                        AssertPossiblyNullNavigations(loadedRoot);

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
        public virtual void Sever_required_one_to_one_with_alternate_key(ChangeMechanism changeMechanism)
        {
            Root root = null;
            RequiredSingleAk1 old1 = null;
            RequiredSingleAk2 old2 = null;
            RequiredSingleComposite2 old2c = null;
            ExecuteWithStrategyInTransaction(
                context =>
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
                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        Assert.Equal(
                            CoreStrings.RelationshipConceptualNullSensitive(nameof(Root), nameof(RequiredSingleAk1), "{Id: 1}"),
                            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Null(old1.Root);
                        Assert.Null(old2.Back);
                        Assert.Null(old2c.Back);
                        Assert.Equal(old1.AlternateId, old2.BackId);
                        Assert.Equal(old1.Id, old2c.BackId);
                        Assert.Equal(old1.AlternateId, old2c.BackAlternateId);
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var loadedRoot = LoadRequiredAkGraph(context);

                        AssertKeys(root, loadedRoot);
                        AssertPossiblyNullNavigations(loadedRoot);

                        Assert.False(context.Set<RequiredSingleAk1>().Any(e => e.Id == old1.Id));
                        Assert.False(context.Set<RequiredSingleAk2>().Any(e => e.Id == old2.Id));
                        Assert.False(context.Set<RequiredSingleComposite2>().Any(e => e.Id == old2c.Id));
                    }
                });
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)ChangeMechanism.Principal)]
        [InlineData((int)(ChangeMechanism.Principal | ChangeMechanism.Dependent))]
        public virtual void Sever_required_non_PK_one_to_one_with_alternate_key(ChangeMechanism changeMechanism)
        {
            Root root = null;
            RequiredNonPkSingleAk1 old1 = null;
            RequiredNonPkSingleAk2 old2 = null;
            ExecuteWithStrategyInTransaction(
                context =>
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

                    context.ChangeTracker.DetectChanges();
                    context.ChangeTracker.DetectChanges();
                    Assert.False(context.Entry(root).Reference(e => e.RequiredNonPkSingleAk).IsLoaded);
                    Assert.False(context.Entry(old1).Reference(e => e.Root).IsLoaded);
                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        Assert.Equal(
                            CoreStrings.RelationshipConceptualNullSensitive(nameof(Root), nameof(RequiredNonPkSingleAk1), "{Id: 1}"),
                            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Null(old1.Root);
                        Assert.Null(old2.Back);
                        Assert.Equal(old1.AlternateId, old2.BackId);
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var loadedRoot = LoadRequiredNonPkAkGraph(context);

                        AssertKeys(root, loadedRoot);
                        AssertPossiblyNullNavigations(loadedRoot);

                        Assert.False(context.Set<RequiredNonPkSingleAk1>().Any(e => e.Id == old1.Id));
                        Assert.False(context.Set<RequiredNonPkSingleAk2>().Any(e => e.Id == old2.Id));
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
        public virtual void Reparent_optional_one_to_one_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingRoot)
        {
            var newRoot = new Root
            {
                AlternateId = Guid.NewGuid()
            };
            Root root = null;
            OptionalSingleAk1 old1 = null;
            OptionalSingleAk2 old2 = null;
            OptionalSingleComposite2 old2c = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    if (useExistingRoot)
                    {
                        context.Add(newRoot);
                        context.SaveChanges();
                    }
                },
                context =>
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
                },
                context =>
                {
                    var loadedRoot = LoadOptionalAkGraph(context);

                    AssertKeys(root, loadedRoot);
                    AssertPossiblyNullNavigations(loadedRoot);

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
        public virtual void Reparent_required_one_to_one_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingRoot)
        {
            var newRoot = new Root
            {
                AlternateId = Guid.NewGuid()
            };
            Root root = null;
            RequiredSingleAk1 old1 = null;
            RequiredSingleAk2 old2 = null;
            RequiredSingleComposite2 old2c = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    if (useExistingRoot)
                    {
                        context.Add(newRoot);
                        context.SaveChanges();
                    }
                },
                context =>
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
                },
                context =>
                {
                    var loadedRoot = LoadRequiredAkGraph(context);

                    AssertKeys(root, loadedRoot);
                    AssertPossiblyNullNavigations(loadedRoot);

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
        public virtual void Reparent_required_non_PK_one_to_one_with_alternate_key(ChangeMechanism changeMechanism, bool useExistingRoot)
        {
            var newRoot = new Root
            {
                AlternateId = Guid.NewGuid()
            };
            Root root = null;
            RequiredNonPkSingleAk1 old1 = null;
            RequiredNonPkSingleAk2 old2 = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    if (useExistingRoot)
                    {
                        context.Add(newRoot);
                        context.SaveChanges();
                    }
                },
                context =>
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

                    Assert.True(context.ChangeTracker.HasChanges());

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Null(root.RequiredNonPkSingleAk);

                    Assert.Same(newRoot, old1.Root);
                    Assert.Same(old1, old2.Back);
                    Assert.Equal(newRoot.AlternateId, old1.RootId);
                    Assert.Equal(old1.AlternateId, old2.BackId);
                },
                context =>
                {
                    var loadedRoot = LoadRequiredNonPkAkGraph(context);

                    AssertKeys(root, loadedRoot);
                    AssertPossiblyNullNavigations(loadedRoot);

                    newRoot = context.Set<Root>().Single(e => e.Id == newRoot.Id);
                    var loaded1 = context.Set<RequiredNonPkSingleAk1>().Single(e => e.Id == old1.Id);
                    var loaded2 = context.Set<RequiredNonPkSingleAk2>().Single(e => e.Id == old2.Id);

                    Assert.Same(newRoot, loaded1.Root);
                    Assert.Same(loaded1, loaded2.Back);
                    Assert.Equal(newRoot.AlternateId, loaded1.RootId);
                    Assert.Equal(loaded1.AlternateId, loaded2.BackId);
                });
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_many_to_one_dependents_are_cascade_deleted()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            List<int> orphanedIds = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = LoadRequiredGraph(context);

                    Assert.Equal(2, root.RequiredChildren.Count());

                    var removed = root.RequiredChildren.First();

                    removedId = removed.Id;
                    var cascadeRemoved = removed.Children.ToList();
                    orphanedIds = cascadeRemoved.Select(e => e.Id).ToList();

                    Assert.Equal(2, orphanedIds.Count);

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                        Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Detached));

                        Assert.Equal(1, root.RequiredChildren.Count());
                        Assert.DoesNotContain(removedId, root.RequiredChildren.Select(e => e.Id));

                        Assert.Empty(context.Set<Required1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<Required2>().Where(e => orphanedIds.Contains(e.Id)));

                        Assert.Same(root, removed.Parent);
                        Assert.Equal(2, removed.Children.Count());
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadRequiredGraph(context);

                        Assert.Equal(1, root.RequiredChildren.Count());
                        Assert.DoesNotContain(removedId, root.RequiredChildren.Select(e => e.Id));

                        Assert.Empty(context.Set<Required1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<Required2>().Where(e => orphanedIds.Contains(e.Id)));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual void Required_many_to_one_dependent_leaves_can_be_deleted()
        {
            var removedId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = LoadRequiredGraph(context);
                    var parent = root.RequiredChildren.First();

                    Assert.Equal(2, parent.Children.Count());
                    var removed = parent.Children.First();

                    removedId = removed.Id;

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());
                    Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                    Assert.Equal(1, parent.Children.Count());
                    Assert.DoesNotContain(removedId, parent.Children.Select(e => e.Id));

                    Assert.Empty(context.Set<Required2>().Where(e => e.Id == removedId));

                    Assert.Same(parent, removed.Parent);
                },
                context =>
                {
                    var root = LoadRequiredGraph(context);
                    var parent = root.RequiredChildren.First();

                    Assert.Equal(1, parent.Children.Count());
                    Assert.DoesNotContain(removedId, parent.Children.Select(e => e.Id));

                    Assert.Empty(context.Set<Required2>().Where(e => e.Id == removedId));
                });
        }

        [ConditionalFact]
        public virtual DbUpdateException Optional_many_to_one_dependents_are_orphaned()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            List<int> orphanedIds = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = LoadOptionalGraph(context);

                    Assert.Equal(2, root.OptionalChildren.Count());

                    var removed = root.OptionalChildren.First();

                    removedId = removed.Id;
                    var orphaned = removed.Children.ToList();
                    orphanedIds = orphaned.Select(e => e.Id).ToList();

                    Assert.Equal(2, orphanedIds.Count);

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                        Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));

                        Assert.Equal(1, root.OptionalChildren.Count());
                        Assert.DoesNotContain(removedId, root.OptionalChildren.Select(e => e.Id));

                        Assert.Empty(context.Set<Optional1>().Where(e => e.Id == removedId));
                        Assert.Equal(orphanedIds.Count, context.Set<Optional2>().Count(e => orphanedIds.Contains(e.Id)));

                        Assert.Same(root, removed.Parent);
                        Assert.Equal(2, removed.Children.Count());
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadOptionalGraph(context);

                        Assert.Equal(1, root.OptionalChildren.Count());
                        Assert.DoesNotContain(removedId, root.OptionalChildren.Select(e => e.Id));

                        Assert.Empty(context.Set<Optional1>().Where(e => e.Id == removedId));
                        Assert.Equal(orphanedIds.Count, context.Set<Optional2>().Count(e => orphanedIds.Contains(e.Id)));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual void Optional_many_to_one_dependent_leaves_can_be_deleted()
        {
            var removedId = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = LoadOptionalGraph(context);
                    var parent = root.OptionalChildren.First();

                    Assert.Equal(2, parent.Children.Count());

                    var removed = parent.Children.First();
                    removedId = removed.Id;

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                    Assert.Equal(1, parent.Children.Count());
                    Assert.DoesNotContain(removedId, parent.Children.Select(e => e.Id));

                    Assert.Empty(context.Set<Optional2>().Where(e => e.Id == removedId));

                    Assert.Same(parent, removed.Parent);
                },
                context =>
                {
                    var root = LoadOptionalGraph(context);
                    var parent = root.OptionalChildren.First();

                    Assert.Equal(1, parent.Children.Count());
                    Assert.DoesNotContain(removedId, parent.Children.Select(e => e.Id));

                    Assert.Empty(context.Set<Optional2>().Where(e => e.Id == removedId));
                });
        }

        [ConditionalFact]
        public virtual DbUpdateException Optional_one_to_one_are_orphaned()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            var orphanedId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = LoadOptionalGraph(context);

                    var removed = root.OptionalSingle;

                    removedId = removed.Id;
                    var orphaned = removed.Single;
                    orphanedId = orphaned.Id;

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadOptionalGraph(context);

                        Assert.Null(root.OptionalSingle);

                        Assert.Empty(context.Set<OptionalSingle1>().Where(e => e.Id == removedId));
                        Assert.Equal(1, context.Set<OptionalSingle2>().Count(e => e.Id == orphanedId));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual void Optional_one_to_one_leaf_can_be_deleted()
        {
            var removedId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = LoadOptionalGraph(context);
                    var parent = root.OptionalSingle;

                    var removed = parent.Single;

                    removedId = removed.Id;

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

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

        [ConditionalFact]
        public virtual DbUpdateException Required_one_to_one_are_cascade_deleted()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            var orphanedId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = LoadRequiredGraph(context);

                    var removed = root.RequiredSingle;

                    removedId = removed.Id;
                    var orphaned = removed.Single;
                    orphanedId = orphaned.Id;

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadRequiredGraph(context);

                        Assert.Null(root.RequiredSingle);

                        Assert.Empty(context.Set<RequiredSingle1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredSingle2>().Where(e => e.Id == orphanedId));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual void Required_one_to_one_leaf_can_be_deleted()
        {
            var removedId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = LoadRequiredGraph(context);
                    var parent = root.RequiredSingle;

                    var removed = parent.Single;

                    removedId = removed.Id;

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                    Assert.Null(parent.Single);
                    Assert.Empty(context.Set<RequiredSingle2>().Where(e => e.Id == removedId));
                    Assert.Same(parent, removed.Back);
                },
                context =>
                {
                    var root = LoadRequiredGraph(context);
                    var parent = root.RequiredSingle;

                    Assert.Null(parent.Single);
                    Assert.Empty(context.Set<RequiredSingle2>().Where(e => e.Id == removedId));
                });
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_non_PK_one_to_one_are_cascade_deleted()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            var orphanedId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = LoadRequiredNonPkGraph(context);

                    var removed = root.RequiredNonPkSingle;

                    removedId = removed.Id;
                    var orphaned = removed.Single;
                    orphanedId = orphaned.Id;

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadRequiredNonPkGraph(context);

                        Assert.Null(root.RequiredNonPkSingle);

                        Assert.Empty(context.Set<RequiredNonPkSingle1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredNonPkSingle2>().Where(e => e.Id == orphanedId));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual void Required_non_PK_one_to_one_leaf_can_be_deleted()
        {
            var removedId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = LoadRequiredNonPkGraph(context);
                    var parent = root.RequiredNonPkSingle;

                    var removed = parent.Single;

                    removedId = removed.Id;

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                    Assert.Null(parent.Single);
                    Assert.Empty(context.Set<RequiredNonPkSingle2>().Where(e => e.Id == removedId));
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

        [ConditionalFact]
        public virtual DbUpdateException Optional_many_to_one_dependents_with_alternate_key_are_orphaned()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            List<int> orphanedIds = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = LoadOptionalAkGraph(context);

                    Assert.Equal(2, root.OptionalChildrenAk.Count());

                    var removed = root.OptionalChildrenAk.OrderBy(c => c.Id).First();

                    removedId = removed.Id;
                    var orphaned = removed.Children.ToList();
                    orphanedIds = orphaned.Select(e => e.Id).ToList();

                    Assert.Equal(2, orphanedIds.Count);

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                        Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));

                        Assert.Equal(1, root.OptionalChildrenAk.Count());
                        Assert.DoesNotContain(removedId, root.OptionalChildrenAk.Select(e => e.Id));

                        Assert.Empty(context.Set<OptionalAk1>().Where(e => e.Id == removedId));
                        Assert.Equal(orphanedIds.Count, context.Set<OptionalAk2>().Count(e => orphanedIds.Contains(e.Id)));

                        Assert.Same(root, removed.Parent);
                        Assert.Equal(2, removed.Children.Count());
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadOptionalAkGraph(context);

                        Assert.Equal(1, root.OptionalChildrenAk.Count());
                        Assert.DoesNotContain(removedId, root.OptionalChildrenAk.Select(e => e.Id));

                        Assert.Empty(context.Set<OptionalAk1>().Where(e => e.Id == removedId));
                        Assert.Equal(orphanedIds.Count, context.Set<OptionalAk2>().Count(e => orphanedIds.Contains(e.Id)));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_many_to_one_dependents_with_alternate_key_are_cascade_deleted()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            List<int> orphanedIds = null;
            List<int> orphanedIdCs = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = LoadRequiredAkGraph(context);

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

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                        Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Detached));
                        Assert.True(cascadeRemovedC.All(e => context.Entry(e).State == EntityState.Detached));

                        Assert.Equal(1, root.RequiredChildrenAk.Count());
                        Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                        Assert.Empty(context.Set<RequiredAk1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredAk2>().Where(e => orphanedIds.Contains(e.Id)));

                        Assert.Same(root, removed.Parent);
                        Assert.Equal(2, removed.Children.Count());
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadRequiredAkGraph(context);

                        Assert.Equal(1, root.RequiredChildrenAk.Count());
                        Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                        Assert.Empty(context.Set<RequiredAk1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredAk2>().Where(e => orphanedIds.Contains(e.Id)));
                        Assert.Empty(context.Set<RequiredComposite2>().Where(e => orphanedIdCs.Contains(e.Id)));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Optional_one_to_one_with_alternate_key_are_orphaned()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            var orphanedId = 0;
            var orphanedIdC = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = LoadOptionalAkGraph(context);

                    var removed = root.OptionalSingleAk;

                    removedId = removed.Id;
                    var orphaned = removed.Single;
                    var orphanedC = removed.SingleComposite;
                    orphanedId = orphaned.Id;
                    orphanedIdC = orphanedC.Id;

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
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
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadOptionalAkGraph(context);

                        Assert.Null(root.OptionalSingleAk);

                        Assert.Empty(context.Set<OptionalSingleAk1>().Where(e => e.Id == removedId));
                        Assert.Equal(1, context.Set<OptionalSingleAk2>().Count(e => e.Id == orphanedId));
                        Assert.Equal(1, context.Set<OptionalSingleComposite2>().Count(e => e.Id == orphanedIdC));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_one_to_one_with_alternate_key_are_cascade_deleted()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            var orphanedId = 0;
            var orphanedIdC = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = LoadRequiredAkGraph(context);

                    var removed = root.RequiredSingleAk;

                    removedId = removed.Id;
                    var orphaned = removed.Single;
                    var orphanedC = removed.SingleComposite;
                    orphanedId = orphaned.Id;
                    orphanedIdC = orphanedC.Id;

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadRequiredAkGraph(context);

                        Assert.Null(root.RequiredSingleAk);

                        Assert.Empty(context.Set<RequiredSingleAk1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredSingleAk2>().Where(e => e.Id == orphanedId));
                        Assert.Empty(context.Set<RequiredSingleComposite2>().Where(e => e.Id == orphanedIdC));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_non_PK_one_to_one_with_alternate_key_are_cascade_deleted()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            var orphanedId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = LoadRequiredNonPkAkGraph(context);

                    var removed = root.RequiredNonPkSingleAk;

                    removedId = removed.Id;
                    var orphaned = removed.Single;
                    orphanedId = orphaned.Id;

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadRequiredNonPkAkGraph(context);

                        Assert.Null(root.RequiredNonPkSingleAk);

                        Assert.Empty(context.Set<RequiredNonPkSingleAk1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredNonPkSingleAk2>().Where(e => e.Id == orphanedId));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_many_to_one_dependents_are_cascade_deleted_in_store()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            List<int> orphanedIds = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var removed = LoadRequiredGraph(context).RequiredChildren.First();

                    removedId = removed.Id;
                    orphanedIds = removed.Children.Select(e => e.Id).ToList();

                    Assert.Equal(2, orphanedIds.Count);
                },
                context =>
                {
                    var root = context.Set<Root>().Include(e => e.RequiredChildren).Single(IsTheRoot);

                    var removed = root.RequiredChildren.Single(e => e.Id == removedId);

                    Assert.Equal(2, orphanedIds.Count);

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                        Assert.Equal(1, root.RequiredChildren.Count());
                        Assert.DoesNotContain(removedId, root.RequiredChildren.Select(e => e.Id));

                        Assert.Empty(context.Set<Required1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<Required2>().Where(e => orphanedIds.Contains(e.Id)));

                        Assert.Same(root, removed.Parent);
                        Assert.Equal(0, removed.Children.Count());
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadRequiredGraph(context);

                        Assert.Equal(1, root.RequiredChildren.Count());
                        Assert.DoesNotContain(removedId, root.RequiredChildren.Select(e => e.Id));

                        Assert.Empty(context.Set<Required1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<Required2>().Where(e => orphanedIds.Contains(e.Id)));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_one_to_one_are_cascade_deleted_in_store()
        {
            DbUpdateException updateException = null;

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
                    var root = context.Set<Root>().Include(e => e.RequiredSingle).Single(IsTheRoot);

                    var removed = root.RequiredSingle;
                    var orphaned = removed.Single;

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                        Assert.Null(root.RequiredSingle);

                        Assert.Empty(context.Set<RequiredSingle1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredSingle2>().Where(e => e.Id == orphanedId));

                        Assert.Same(root, removed.Root);
                        Assert.Same(orphaned, removed.Single);
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadRequiredGraph(context);

                        Assert.Null(root.RequiredSingle);

                        Assert.Empty(context.Set<RequiredSingle1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredSingle2>().Where(e => e.Id == orphanedId));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_non_PK_one_to_one_are_cascade_deleted_in_store()
        {
            DbUpdateException updateException = null;

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
                    var root = context.Set<Root>().Include(e => e.RequiredNonPkSingle).Single(IsTheRoot);

                    var removed = root.RequiredNonPkSingle;
                    var orphaned = removed.Single;

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadRequiredNonPkGraph(context);

                        Assert.Null(root.RequiredNonPkSingle);

                        Assert.Empty(context.Set<RequiredNonPkSingle1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredNonPkSingle2>().Where(e => e.Id == orphanedId));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_many_to_one_dependents_with_alternate_key_are_cascade_deleted_in_store()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            List<int> orphanedIds = null;
            List<int> orphanedIdCs = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var removed = LoadRequiredAkGraph(context).RequiredChildrenAk.First();

                    removedId = removed.Id;
                    orphanedIds = removed.Children.Select(e => e.Id).ToList();
                    orphanedIdCs = removed.CompositeChildren.Select(e => e.Id).ToList();

                    Assert.Equal(2, orphanedIds.Count);
                    Assert.Equal(2, orphanedIdCs.Count);
                },
                context =>
                {
                    var root = context.Set<Root>().Include(e => e.RequiredChildrenAk).Single(IsTheRoot);

                    var removed = root.RequiredChildrenAk.Single(e => e.Id == removedId);

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                        Assert.Equal(1, root.RequiredChildrenAk.Count());
                        Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                        Assert.Empty(context.Set<RequiredAk1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredAk2>().Where(e => orphanedIds.Contains(e.Id)));
                        Assert.Empty(context.Set<RequiredComposite2>().Where(e => orphanedIdCs.Contains(e.Id)));

                        Assert.Same(root, removed.Parent);
                        Assert.Equal(0, removed.Children.Count()); // Never loaded
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadRequiredAkGraph(context);

                        Assert.Equal(1, root.RequiredChildrenAk.Count());
                        Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                        Assert.Empty(context.Set<RequiredAk1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredAk2>().Where(e => orphanedIds.Contains(e.Id)));
                        Assert.Empty(context.Set<RequiredComposite2>().Where(e => orphanedIdCs.Contains(e.Id)));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_one_to_one_with_alternate_key_are_cascade_deleted_in_store()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            var orphanedId = 0;
            var orphanedIdC = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var removed = LoadRequiredAkGraph(context).RequiredSingleAk;

                    removedId = removed.Id;
                    orphanedId = removed.Single.Id;
                    orphanedIdC = removed.SingleComposite.Id;
                },
                context =>
                {
                    var root = context.Set<Root>().Include(e => e.RequiredSingleAk).Single(IsTheRoot);

                    var removed = root.RequiredSingleAk;
                    var orphaned = removed.Single;

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadRequiredAkGraph(context);

                        Assert.Null(root.RequiredSingleAk);

                        Assert.Empty(context.Set<RequiredSingleAk1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredSingleAk2>().Where(e => e.Id == orphanedId));
                        Assert.Empty(context.Set<RequiredSingleComposite2>().Where(e => e.Id == orphanedIdC));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_non_PK_one_to_one_with_alternate_key_are_cascade_deleted_in_store()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            var orphanedId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var removed = LoadRequiredNonPkAkGraph(context).RequiredNonPkSingleAk;

                    removedId = removed.Id;
                    orphanedId = removed.Single.Id;
                },
                context =>
                {
                    var root = context.Set<Root>().Include(e => e.RequiredNonPkSingleAk).Single(IsTheRoot);

                    var removed = root.RequiredNonPkSingleAk;
                    var orphaned = removed.Single;

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadRequiredNonPkAkGraph(context);

                        Assert.Null(root.RequiredNonPkSingleAk);

                        Assert.Empty(context.Set<RequiredNonPkSingleAk1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredNonPkSingleAk2>().Where(e => e.Id == orphanedId));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Optional_many_to_one_dependents_are_orphaned_in_store()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            List<int> orphanedIds = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var removed = LoadOptionalGraph(context).OptionalChildren.First();

                    removedId = removed.Id;
                    orphanedIds = removed.Children.Select(e => e.Id).ToList();

                    Assert.Equal(2, orphanedIds.Count);
                },
                context =>
                {
                    var root = context.Set<Root>().Include(e => e.OptionalChildren).Single(IsTheRoot);

                    var removed = root.OptionalChildren.First(e => e.Id == removedId);

                    Assert.Equal(2, orphanedIds.Count);

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                        Assert.Equal(1, root.OptionalChildren.Count());
                        Assert.DoesNotContain(removedId, root.OptionalChildren.Select(e => e.Id));

                        Assert.Empty(context.Set<Optional1>().Where(e => e.Id == removedId));

                        var orphaned = context.Set<Optional2>().Where(e => orphanedIds.Contains(e.Id)).ToList();
                        Assert.Equal(orphanedIds.Count, orphaned.Count);
                        Assert.True(orphaned.All(e => e.ParentId == null));

                        Assert.Same(root, removed.Parent);
                        Assert.Equal(0, removed.Children.Count()); // Never loaded
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadOptionalGraph(context);

                        Assert.Equal(1, root.OptionalChildren.Count());
                        Assert.DoesNotContain(removedId, root.OptionalChildren.Select(e => e.Id));

                        Assert.Empty(context.Set<Optional1>().Where(e => e.Id == removedId));

                        var orphaned = context.Set<Optional2>().Where(e => orphanedIds.Contains(e.Id)).ToList();
                        Assert.Equal(orphanedIds.Count, orphaned.Count);
                        Assert.True(orphaned.All(e => e.ParentId == null));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Optional_one_to_one_are_orphaned_in_store()
        {
            DbUpdateException updateException = null;

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
                    var root = context.Set<Root>().Include(e => e.OptionalSingle).Single(IsTheRoot);

                    var removed = root.OptionalSingle;
                    var orphaned = removed.Single;

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadOptionalGraph(context);

                        Assert.Null(root.OptionalSingle);

                        Assert.Empty(context.Set<OptionalSingle1>().Where(e => e.Id == removedId));
                        Assert.Null(context.Set<OptionalSingle2>().Single(e => e.Id == orphanedId).BackId);
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Optional_many_to_one_dependents_with_alternate_key_are_orphaned_in_store()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            List<int> orphanedIds = null;
            List<int> orphanedIdCs = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var removed = LoadOptionalAkGraph(context).OptionalChildrenAk.OrderBy(c => c.Id).First();

                    removedId = removed.Id;
                    orphanedIds = removed.Children.Select(e => e.Id).ToList();
                    orphanedIdCs = removed.CompositeChildren.Select(e => e.Id).ToList();

                    Assert.Equal(2, orphanedIds.Count);
                    Assert.Equal(2, orphanedIdCs.Count);
                },
                context =>
                {
                    var root = context.Set<Root>().Include(e => e.OptionalChildrenAk).Single(IsTheRoot);

                    var removed = root.OptionalChildrenAk.First(e => e.Id == removedId);

                    context.Remove(removed);

                    foreach (var toOrphan in context.Set<OptionalComposite2>().Where(e => orphanedIdCs.Contains(e.Id)).ToList())
                    {
                        toOrphan.ParentId = null;
                    }

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                        Assert.Equal(1, root.OptionalChildrenAk.Count());
                        Assert.DoesNotContain(removedId, root.OptionalChildrenAk.Select(e => e.Id));

                        Assert.Empty(context.Set<OptionalAk1>().Where(e => e.Id == removedId));

                        var orphaned = context.Set<OptionalAk2>().Where(e => orphanedIds.Contains(e.Id)).ToList();
                        Assert.Equal(orphanedIds.Count, orphaned.Count);
                        Assert.True(orphaned.All(e => e.ParentId == null));

                        var orphanedC = context.Set<OptionalComposite2>().Where(e => orphanedIdCs.Contains(e.Id)).ToList();
                        Assert.Equal(orphanedIdCs.Count, orphanedC.Count);
                        Assert.True(orphanedC.All(e => e.ParentId == null));

                        Assert.Same(root, removed.Parent);
                        Assert.Equal(0, removed.Children.Count()); // Never loaded
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadOptionalAkGraph(context);

                        Assert.Equal(1, root.OptionalChildrenAk.Count());
                        Assert.DoesNotContain(removedId, root.OptionalChildrenAk.Select(e => e.Id));

                        Assert.Empty(context.Set<OptionalAk1>().Where(e => e.Id == removedId));

                        var orphaned = context.Set<OptionalAk2>().Where(e => orphanedIds.Contains(e.Id)).ToList();
                        Assert.Equal(orphanedIds.Count, orphaned.Count);
                        Assert.True(orphaned.All(e => e.ParentId == null));

                        var orphanedC = context.Set<OptionalComposite2>().Where(e => orphanedIdCs.Contains(e.Id)).ToList();
                        Assert.Equal(orphanedIdCs.Count, orphanedC.Count);
                        Assert.True(orphanedC.All(e => e.ParentId == null));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Optional_one_to_one_with_alternate_key_are_orphaned_in_store()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            var orphanedId = 0;
            var orphanedIdC = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var removed = LoadOptionalAkGraph(context).OptionalSingleAk;

                    removedId = removed.Id;
                    orphanedId = removed.Single.Id;
                    orphanedIdC = removed.SingleComposite.Id;
                },
                context =>
                {
                    var root = context.Set<Root>().Include(e => e.OptionalSingleAk).Single(IsTheRoot);

                    var removed = root.OptionalSingleAk;
                    var orphaned = removed.Single;

                    context.Remove(removed);

                    // Cannot have SET NULL action in the store because one of the FK columns
                    // is not nullable, so need to do this on the EF side.
                    context.Set<OptionalSingleComposite2>().Single(e => e.Id == orphanedIdC).BackId = null;

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);

                        Assert.Null(root.OptionalSingleAk);

                        Assert.Empty(context.Set<OptionalSingleAk1>().Where(e => e.Id == removedId));
                        Assert.Null(context.Set<OptionalSingleAk2>().Single(e => e.Id == orphanedId).BackId);
                        Assert.Null(context.Set<OptionalSingleComposite2>().Single(e => e.Id == orphanedIdC).BackId);

                        Assert.Same(root, removed.Root);
                        Assert.Same(orphaned, removed.Single);
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadOptionalAkGraph(context);

                        Assert.Null(root.OptionalSingleAk);

                        Assert.Empty(context.Set<OptionalSingleAk1>().Where(e => e.Id == removedId));
                        Assert.Null(context.Set<OptionalSingleAk2>().Single(e => e.Id == orphanedId).BackId);
                        Assert.Null(context.Set<OptionalSingleComposite2>().Single(e => e.Id == orphanedIdC).BackId);
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_many_to_one_dependents_are_cascade_deleted_starting_detached()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            List<int> orphanedIds = null;
            Root root = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    root = LoadRequiredGraph(context);

                    Assert.Equal(2, root.RequiredChildren.Count());
                },
                context =>
                {
                    var removed = root.RequiredChildren.First();

                    removedId = removed.Id;
                    var cascadeRemoved = removed.Children.ToList();
                    orphanedIds = cascadeRemoved.Select(e => e.Id).ToList();

                    Assert.Equal(2, orphanedIds.Count);

                    context.Remove(removed);

                    Assert.Equal(EntityState.Deleted, context.Entry(removed).State);
                    Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Unchanged));

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        root = LoadRequiredGraph(context);

                        Assert.Equal(1, root.RequiredChildren.Count());
                        Assert.DoesNotContain(removedId, root.RequiredChildren.Select(e => e.Id));

                        Assert.Empty(context.Set<Required1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<Required2>().Where(e => orphanedIds.Contains(e.Id)));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Optional_many_to_one_dependents_are_orphaned_starting_detached()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            List<int> orphanedIds = null;
            Root root = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    root = LoadOptionalGraph(context);

                    Assert.Equal(2, root.OptionalChildren.Count());
                },
                context =>
                {
                    var removed = root.OptionalChildren.First();

                    removedId = removed.Id;
                    var orphaned = removed.Children.ToList();
                    orphanedIds = orphaned.Select(e => e.Id).ToList();

                    Assert.Equal(2, orphanedIds.Count);

                    context.Remove(removed);

                    Assert.Equal(EntityState.Deleted, context.Entry(removed).State);
                    Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                        Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));

                        Assert.Same(root, removed.Parent);
                        Assert.Equal(2, removed.Children.Count());
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        root = LoadOptionalGraph(context);

                        Assert.Equal(1, root.OptionalChildren.Count());
                        Assert.DoesNotContain(removedId, root.OptionalChildren.Select(e => e.Id));

                        Assert.Empty(context.Set<Optional1>().Where(e => e.Id == removedId));
                        Assert.Equal(orphanedIds.Count, context.Set<Optional2>().Count(e => orphanedIds.Contains(e.Id)));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Optional_one_to_one_are_orphaned_starting_detached()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            var orphanedId = 0;
            Root root = null;

            ExecuteWithStrategyInTransaction(
                context => root = LoadOptionalGraph(context),
                context =>
                {
                    var removed = root.OptionalSingle;

                    removedId = removed.Id;
                    var orphaned = removed.Single;
                    orphanedId = orphaned.Id;

                    context.Remove(removed);

                    Assert.Equal(EntityState.Deleted, context.Entry(removed).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
                    if (!Fixture.ForceRestrict)
                    {
                        root = LoadOptionalGraph(context);

                        Assert.Null(root.OptionalSingle);

                        Assert.Empty(context.Set<OptionalSingle1>().Where(e => e.Id == removedId));
                        Assert.Equal(1, context.Set<OptionalSingle2>().Count(e => e.Id == orphanedId));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_one_to_one_are_cascade_deleted_starting_detached()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            var orphanedId = 0;
            Root root = null;

            ExecuteWithStrategyInTransaction(
                context => root = LoadRequiredGraph(context),
                context =>
                {
                    var removed = root.RequiredSingle;

                    removedId = removed.Id;
                    var orphaned = removed.Single;
                    orphanedId = orphaned.Id;

                    context.Remove(removed);

                    Assert.Equal(EntityState.Deleted, context.Entry(removed).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
                    if (!Fixture.ForceRestrict)
                    {
                        Assert.Null(root.RequiredSingle);

                        Assert.Empty(context.Set<RequiredSingle1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredSingle2>().Where(e => e.Id == orphanedId));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_non_PK_one_to_one_are_cascade_deleted_starting_detached()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            var orphanedId = 0;
            Root root = null;

            ExecuteWithStrategyInTransaction(
                context => root = LoadRequiredNonPkGraph(context),
                context =>
                {
                    var removed = root.RequiredNonPkSingle;

                    removedId = removed.Id;
                    var orphaned = removed.Single;
                    orphanedId = orphaned.Id;

                    context.Remove(removed);

                    Assert.Equal(EntityState.Deleted, context.Entry(removed).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
                    if (!Fixture.ForceRestrict)
                    {
                        root = LoadRequiredNonPkGraph(context);

                        Assert.Null(root.RequiredNonPkSingle);

                        Assert.Empty(context.Set<RequiredNonPkSingle1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredNonPkSingle2>().Where(e => e.Id == orphanedId));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Optional_many_to_one_dependents_with_alternate_key_are_orphaned_starting_detached()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            List<int> orphanedIds = null;
            List<int> orphanedIdCs = null;
            Root root = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    root = LoadOptionalAkGraph(context);

                    Assert.Equal(2, root.OptionalChildrenAk.Count());
                },
                context =>
                {
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
                    Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));
                    Assert.True(orphanedC.All(e => context.Entry(e).State == EntityState.Unchanged));

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                        Assert.True(orphaned.All(e => context.Entry(e).State == EntityState.Unchanged));
                        Assert.True(orphanedC.All(e => context.Entry(e).State == EntityState.Unchanged));

                        Assert.Same(root, removed.Parent);
                        Assert.Equal(2, removed.Children.Count());
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        root = LoadOptionalAkGraph(context);

                        Assert.Equal(1, root.OptionalChildrenAk.Count());
                        Assert.DoesNotContain(removedId, root.OptionalChildrenAk.Select(e => e.Id));

                        Assert.Empty(context.Set<OptionalAk1>().Where(e => e.Id == removedId));
                        Assert.Equal(orphanedIds.Count, context.Set<OptionalAk2>().Count(e => orphanedIds.Contains(e.Id)));
                        Assert.Equal(orphanedIdCs.Count, context.Set<OptionalComposite2>().Count(e => orphanedIdCs.Contains(e.Id)));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_many_to_one_dependents_with_alternate_key_are_cascade_deleted_starting_detached()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            List<int> orphanedIds = null;
            List<int> orphanedIdCs = null;
            Root root = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    root = LoadRequiredAkGraph(context);

                    Assert.Equal(2, root.RequiredChildrenAk.Count());
                },
                context =>
                {
                    var removed = root.RequiredChildrenAk.OrderBy(c => c.Id).First();

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

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        root = LoadRequiredAkGraph(context);

                        Assert.Equal(1, root.RequiredChildrenAk.Count());
                        Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                        Assert.Empty(context.Set<RequiredAk1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredAk2>().Where(e => orphanedIds.Contains(e.Id)));
                        Assert.Empty(context.Set<RequiredComposite2>().Where(e => orphanedIdCs.Contains(e.Id)));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Optional_one_to_one_with_alternate_key_are_orphaned_starting_detached()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            var orphanedId = 0;
            var orphanedIdC = 0;
            Root root = null;

            ExecuteWithStrategyInTransaction(
                context => root = LoadOptionalAkGraph(context),
                context =>
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

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(orphanedC).State);

                        Assert.Same(root, removed.Root);
                        Assert.Same(orphaned, removed.Single);
                    }
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        root = LoadOptionalAkGraph(context);

                        Assert.Null(root.OptionalSingleAk);

                        Assert.Empty(context.Set<OptionalSingleAk1>().Where(e => e.Id == removedId));
                        Assert.Equal(1, context.Set<OptionalSingleAk2>().Count(e => e.Id == orphanedId));
                        Assert.Equal(1, context.Set<OptionalSingleComposite2>().Count(e => e.Id == orphanedIdC));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_one_to_one_with_alternate_key_are_cascade_deleted_starting_detached()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            var orphanedId = 0;
            var orphanedIdC = 0;
            Root root = null;

            ExecuteWithStrategyInTransaction(
                context => root = LoadRequiredAkGraph(context),
                context =>
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

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        root = LoadRequiredAkGraph(context);

                        Assert.Null(root.RequiredSingleAk);

                        Assert.Empty(context.Set<RequiredSingleAk1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredSingleAk2>().Where(e => e.Id == orphanedId));
                        Assert.Empty(context.Set<RequiredSingleComposite2>().Where(e => e.Id == orphanedIdC));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_non_PK_one_to_one_with_alternate_key_are_cascade_deleted_starting_detached()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            var orphanedId = 0;
            Root root = null;

            ExecuteWithStrategyInTransaction(
                context => root = LoadRequiredNonPkAkGraph(context),
                context =>
                {
                    var removed = root.RequiredNonPkSingleAk;

                    removedId = removed.Id;
                    var orphaned = removed.Single;
                    orphanedId = orphaned.Id;

                    context.Remove(removed);

                    Assert.Equal(EntityState.Deleted, context.Entry(removed).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(orphaned).State);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
                    if (!Fixture.ForceRestrict)
                    {
                        root = LoadRequiredNonPkAkGraph(context);

                        Assert.Null(root.RequiredNonPkSingleAk);

                        Assert.Empty(context.Set<RequiredNonPkSingleAk1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredNonPkSingleAk2>().Where(e => e.Id == orphanedId));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_many_to_one_dependents_are_cascade_detached_when_Added()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            List<int> orphanedIds = null;

            ExecuteWithStrategyInTransaction(
                context =>
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

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadRequiredGraph(context);

                        Assert.Equal(1, root.RequiredChildren.Count());
                        Assert.DoesNotContain(removedId, root.RequiredChildren.Select(e => e.Id));

                        Assert.Empty(context.Set<Required1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<Required2>().Where(e => orphanedIds.Contains(e.Id)));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_one_to_one_are_cascade_detached_when_Added()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            var orphanedId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
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

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadRequiredGraph(context);

                        Assert.Null(root.RequiredSingle);

                        Assert.Empty(context.Set<RequiredSingle1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredSingle2>().Where(e => e.Id == orphanedId));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_non_PK_one_to_one_are_cascade_detached_when_Added()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            var orphanedId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
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

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadRequiredNonPkGraph(context);

                        Assert.Null(root.RequiredNonPkSingle);

                        Assert.Empty(context.Set<RequiredNonPkSingle1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredNonPkSingle2>().Where(e => e.Id == orphanedId));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_many_to_one_dependents_with_alternate_key_are_cascade_detached_when_Added()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            List<int> orphanedIds = null;
            List<int> orphanedIdCs = null;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = LoadRequiredAkGraph(context);

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
                    Assert.Equal(EntityState.Added, context.Entry(added).State);
                    Assert.Equal(EntityState.Added, context.Entry(addedC).State);
                    Assert.True(cascadeRemoved.All(e => context.Entry(e).State == EntityState.Unchanged));
                    Assert.True(cascadeRemovedC.All(e => context.Entry(e).State == EntityState.Unchanged));

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadRequiredAkGraph(context);

                        Assert.Equal(1, root.RequiredChildrenAk.Count());
                        Assert.DoesNotContain(removedId, root.RequiredChildrenAk.Select(e => e.Id));

                        Assert.Empty(context.Set<RequiredAk1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredAk2>().Where(e => orphanedIds.Contains(e.Id)));
                        Assert.Empty(context.Set<RequiredComposite2>().Where(e => orphanedIdCs.Contains(e.Id)));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_one_to_one_with_alternate_key_are_cascade_detached_when_Added()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            var orphanedId = 0;
            var orphanedIdC = 0;

            ExecuteWithStrategyInTransaction(
                context =>
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

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
                },
                context =>
                {
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadRequiredAkGraph(context);

                        Assert.Null(root.RequiredSingleAk);

                        Assert.Empty(context.Set<RequiredSingleAk1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredSingleAk2>().Where(e => e.Id == orphanedId));
                        Assert.Empty(context.Set<RequiredSingleComposite2>().Where(e => e.Id == orphanedIdC));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual DbUpdateException Required_non_PK_one_to_one_with_alternate_key_are_cascade_detached_when_Added()
        {
            DbUpdateException updateException = null;

            var removedId = 0;
            var orphanedId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
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

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceRestrict)
                    {
                        updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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
                    if (!Fixture.ForceRestrict)
                    {
                        var root = LoadRequiredNonPkAkGraph(context);

                        Assert.Null(root.RequiredNonPkSingleAk);

                        Assert.Empty(context.Set<RequiredNonPkSingleAk1>().Where(e => e.Id == removedId));
                        Assert.Empty(context.Set<RequiredNonPkSingleAk2>().Where(e => e.Id == orphanedId));
                    }
                });

            return updateException;
        }

        [ConditionalFact]
        public virtual void Re_childing_parent_to_new_child_with_delete()
        {
            var oldId = 0;
            var newId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var parent = context.Set<ParentAsAChild>().Include(p => p.ChildAsAParent).Single();

                    var oldChild = parent.ChildAsAParent;
                    oldId = oldChild.Id;

                    context.Remove(oldChild);

                    var newChild = new ChildAsAParent();
                    parent.ChildAsAParent = newChild;

                    context.SaveChanges();

                    newId = newChild.Id;
                    Assert.NotEqual(newId, oldId);

                    Assert.Equal(newId, parent.ChildAsAParentId);
                    Assert.Same(newChild, parent.ChildAsAParent);

                    Assert.Equal(EntityState.Detached, context.Entry(oldChild).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(newChild).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
                },
                context =>
                {
                    var parent = context.Set<ParentAsAChild>().Include(p => p.ChildAsAParent).Single();
                    Assert.Equal(newId, parent.ChildAsAParentId);
                    Assert.Equal(newId, parent.ChildAsAParent.Id);
                    Assert.Null(context.Set<ChildAsAParent>().Find(oldId));
                });
        }

        [ConditionalFact]
        public virtual void Sometimes_not_calling_DetectChanges_when_required_does_not_throw_for_null_ref()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var dependent = context.Set<BadOrder>().Single();

                    dependent.BadCustomerId = null;

                    var principal = context.Set<BadCustomer>().Single();

                    principal.Status++;

                    Assert.Null(dependent.BadCustomerId);
                    Assert.Null(dependent.BadCustomer);
                    Assert.Empty(principal.BadOrders);

                    context.SaveChanges();

                    Assert.Null(dependent.BadCustomerId);
                    Assert.Null(dependent.BadCustomer);
                    Assert.Empty(principal.BadOrders);
                },
                context =>
                {
                    var dependent = context.Set<BadOrder>().Single();
                    var principal = context.Set<BadCustomer>().Single();

                    Assert.Null(dependent.BadCustomerId);
                    Assert.Null(dependent.BadCustomer);
                    Assert.Empty(principal.BadOrders);
                });
        }

        [ConditionalFact]
        public virtual void Can_add_valid_first_dependent_when_multiple_possible_principal_sides()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var quizTask = new QuizTask();
                    quizTask.Choices.Add(new TaskChoice());

                    context.Add(quizTask);

                    context.SaveChanges();
                },
                context =>
                {
                    var quizTask = context.Set<QuizTask>().Include(e => e.Choices).Single();

                    Assert.Equal(quizTask.Id, quizTask.Choices.Single().QuestTaskId);

                    Assert.Same(quizTask.Choices.Single(), context.Set<TaskChoice>().Single());

                    Assert.Empty(context.Set<HiddenAreaTask>().Include(e => e.Choices));
                });
        }

        [ConditionalFact]
        public virtual void Can_add_valid_second_dependent_when_multiple_possible_principal_sides()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var hiddenAreaTask = new HiddenAreaTask();
                    hiddenAreaTask.Choices.Add(new TaskChoice());

                    context.Add(hiddenAreaTask);

                    context.SaveChanges();
                },
                context =>
                {
                    var hiddenAreaTask = context.Set<HiddenAreaTask>().Include(e => e.Choices).Single();

                    Assert.Equal(hiddenAreaTask.Id, hiddenAreaTask.Choices.Single().QuestTaskId);

                    Assert.Same(hiddenAreaTask.Choices.Single(), context.Set<TaskChoice>().Single());

                    Assert.Empty(context.Set<QuizTask>().Include(e => e.Choices));
                });
        }

        [ConditionalFact]
        public virtual void Can_add_multiple_dependents_when_multiple_possible_principal_sides()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var quizTask = new QuizTask();
                    quizTask.Choices.Add(new TaskChoice());
                    quizTask.Choices.Add(new TaskChoice());

                    context.Add(quizTask);

                    var hiddenAreaTask = new HiddenAreaTask();
                    hiddenAreaTask.Choices.Add(new TaskChoice());
                    hiddenAreaTask.Choices.Add(new TaskChoice());

                    context.Add(hiddenAreaTask);

                    context.SaveChanges();
                },
                context =>
                {
                    var quizTask = context.Set<QuizTask>().Include(e => e.Choices).Single();
                    var hiddenAreaTask = context.Set<HiddenAreaTask>().Include(e => e.Choices).Single();

                    Assert.Equal(2, quizTask.Choices.Count);
                    foreach (var quizTaskChoice in quizTask.Choices)
                    {
                        Assert.Equal(quizTask.Id, quizTaskChoice.QuestTaskId);
                    }

                    Assert.Equal(2, hiddenAreaTask.Choices.Count);
                    foreach (var hiddenAreaTaskChoice in hiddenAreaTask.Choices)
                    {
                        Assert.Equal(hiddenAreaTask.Id, hiddenAreaTaskChoice.QuestTaskId);
                    }

                    foreach (var taskChoice in context.Set<TaskChoice>())
                    {
                        Assert.Equal(
                            1,
                            quizTask.Choices.Count(e => e.Id == taskChoice.Id)
                            + hiddenAreaTask.Choices.Count(e => e.Id == taskChoice.Id));
                    }
                });
        }
    }
}
