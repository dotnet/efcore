// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable AccessToDisposedClosure
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public class OwnedFixupTest
    {
        [Fact]
        public void Can_get_owned_entity_entry()
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentPN
                {
                    Id = 77
                };
                var dependent = new ChildPN
                {
                    Name = "1"
                };
                principal.Child1 = dependent;
                principal.Child2 = dependent;

                Assert.Equal(
                    CoreStrings.UntrackedDependentEntity(
                        typeof(ChildPN).ShortDisplayName(),
                        "." + nameof(EntityEntry.Reference) + "()." + nameof(ReferenceEntry.TargetEntry)),
                    Assert.Throws<InvalidOperationException>(() => context.Entry(dependent)).Message);

                var dependentEntry1 = context.Entry(principal).Reference(p => p.Child1).TargetEntry;

                Assert.Same(dependentEntry1.GetInfrastructure(), context.Entry(dependent).GetInfrastructure());

                var dependentEntry2 = context.Entry(principal).Reference(p => p.Child2).TargetEntry;

                Assert.NotNull(dependentEntry2);
                Assert.Equal(
                    CoreStrings.AmbiguousDependentEntity(
                        typeof(ChildPN).ShortDisplayName(),
                        "." + nameof(EntityEntry.Reference) + "()." + nameof(ReferenceEntry.TargetEntry)),
                    Assert.Throws<InvalidOperationException>(() => context.Entry(dependent)).Message);
            }
        }

        [Fact]
        public void Adding_duplicate_owned_entity_throws_by_default()
        {
            using (var context = new FixupContext(false))
            {
                var principal = new ParentPN
                {
                    Id = 77
                };
                var dependent = new ChildPN
                {
                    Name = "1"
                };
                principal.Child1 = dependent;
                principal.Child2 = dependent;

                var dependentEntry1 = context.Entry(principal).Reference(p => p.Child1).TargetEntry;

                Assert.Same(dependentEntry1.GetInfrastructure(), context.Entry(dependent).GetInfrastructure());

                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        CoreEventId.DuplicateDependentEntityTypeInstanceWarning.ToString(),
                        CoreStrings.LogDuplicateDependentEntityTypeInstance.GenerateMessage(
                            typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.Child2) + "#" + typeof(ChildPN).ShortDisplayName(),
                            typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.Child1) + "#" + typeof(ChildPN).ShortDisplayName()),
                        "CoreEventId.DuplicateDependentEntityTypeInstanceWarning"),
                    Assert.Throws<InvalidOperationException>(() => context.Entry(principal).Reference(p => p.Child2).TargetEntry).Message);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Principal_nav_set_unidirectional(EntityState entityState)
        {
            Principal_nav_set_unidirectional_impl(entityState, true);
            Principal_nav_set_unidirectional_impl(entityState, false);
            Principal_nav_set_unidirectional_impl(entityState, null);
        }

        private void Principal_nav_set_unidirectional_impl(EntityState entityState, bool? graph)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentPN
                {
                    Id = 77
                };
                if (graph == null)
                {
                    context.Entry(principal).State = entityState;
                }

                var dependent = new ChildPN
                {
                    Name = "1"
                };
                principal.Child1 = dependent;
                var subDependent = new SubChildPN
                {
                    Name = "1S"
                };
                dependent.SubChild = subDependent;

                if (graph == null)
                {
                    context.ChangeTracker.DetectChanges();
                }
                else if (graph == true)
                {
                    context.ChangeTracker.TrackGraph(principal, e => e.Entry.State = entityState);
                }
                else
                {
                    switch (entityState)
                    {
                        case EntityState.Added:
                            context.Add(principal);
                            break;
                        case EntityState.Unchanged:
                            context.Attach(principal);
                            break;
                        case EntityState.Modified:
                            context.Update(principal);
                            break;
                    }
                }

                Assert.Equal(3, context.ChangeTracker.Entries().Count());

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(entityState, context.Entry(principal).State);

                        Assert.Same(dependent, principal.Child1);
                        Assert.Null(principal.Child2);
                        var dependentEntry = context.Entry(dependent);
                        Assert.Equal(principal.Id, dependentEntry.Property("ParentId").CurrentValue);
                        Assert.Equal(graph == null ? EntityState.Added : entityState, dependentEntry.State);
                        Assert.Equal(nameof(ParentPN.Child1), dependentEntry.Metadata.DefiningNavigationName);

                        Assert.Same(subDependent, dependent.SubChild);
                        var subDependentEntry = context.Entry(subDependent);
                        Assert.Equal(principal.Id, subDependentEntry.Property("ChildId").CurrentValue);
                        Assert.Equal(graph == null ? EntityState.Added : entityState, subDependentEntry.State);
                        Assert.Equal(nameof(ChildPN.SubChild), subDependentEntry.Metadata.DefiningNavigationName);
                    });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Both_navs_set(EntityState entityState)
        {
            Both_navs_set_impl(entityState, true);
            Both_navs_set_impl(entityState, false);
            Both_navs_set_impl(entityState, null);
        }

        private void Both_navs_set_impl(EntityState entityState, bool? graph)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent
                {
                    Id = 77
                };
                if (graph == null)
                {
                    context.Entry(principal).State = entityState;
                }

                var dependent = new Child
                {
                    Name = "1",
                    Parent = principal
                };
                principal.Child1 = dependent;

                if (graph == null)
                {
                    context.ChangeTracker.DetectChanges();
                }
                else if (graph == true)
                {
                    context.ChangeTracker.TrackGraph(principal, e => e.Entry.State = entityState);
                }
                else
                {
                    switch (entityState)
                    {
                        case EntityState.Added:
                            context.Add(principal);
                            break;
                        case EntityState.Unchanged:
                            context.Attach(principal);
                            break;
                        case EntityState.Modified:
                            context.Update(principal);
                            break;
                    }
                }

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Same(dependent, principal.Child1);
                        Assert.Equal(entityState, context.Entry(principal).State);
                        Assert.Equal(graph == null ? EntityState.Added : entityState, context.Entry(dependent).State);
                    });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Principal_nav_set_bidirectional(EntityState entityState)
        {
            Principal_nav_set_bidirectional_impl(entityState, true);
            Principal_nav_set_bidirectional_impl(entityState, false);
            Principal_nav_set_bidirectional_impl(entityState, null);
        }

        private void Principal_nav_set_bidirectional_impl(EntityState entityState, bool? graph)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent
                {
                    Id = 77
                };
                if (graph == null)
                {
                    context.Entry(principal).State = entityState;
                }

                var dependent = new Child
                {
                    Name = "1"
                };
                principal.Child1 = dependent;

                if (graph == null)
                {
                    context.ChangeTracker.DetectChanges();
                }
                else if (graph == true)
                {
                    context.ChangeTracker.TrackGraph(principal, e => e.Entry.State = entityState);
                }
                else
                {
                    switch (entityState)
                    {
                        case EntityState.Added:
                            context.Add(principal);
                            break;
                        case EntityState.Unchanged:
                            context.Attach(principal);
                            break;
                        case EntityState.Modified:
                            context.Update(principal);
                            break;
                    }
                }

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                        Assert.Same(dependent, principal.Child1);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Equal(entityState, context.Entry(principal).State);
                        Assert.Equal(graph == null ? EntityState.Added : entityState, context.Entry(dependent).State);
                    });
            }
        }

        [Fact]
        public async Task Principal_nav_set_unidirectional_AddAsync()
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentPN
                {
                    Id = 77
                };
                var dependent = new ChildPN
                {
                    Name = "1"
                };
                principal.Child1 = dependent;

                await context.AddAsync(principal);
                var entityState = EntityState.Added;

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                AssertFixup(
                    context,
                    () =>
                    {
                        Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                        Assert.Same(dependent, principal.Child1);
                        Assert.Equal(entityState, context.Entry(principal).State);
                        Assert.Equal(entityState, context.Entry(dependent).State);
                    });
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Identity_changed_unidirectional(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentPN
                {
                    Id = 77
                };
                var dependent = new ChildPN
                {
                    Name = "1"
                };
                principal.Child1 = dependent;

                context.ChangeTracker.TrackGraph(principal, e => e.Entry.State = entityState);

                var dependentEntry1 = context.Entry(principal).Reference(p => p.Child1).TargetEntry;

                principal.Child1 = null;
                principal.Child2 = dependent;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(entityState == EntityState.Added ? 2 : 3, context.ChangeTracker.Entries().Count());
                Assert.Null(principal.Child1);
                Assert.Same(dependent, principal.Child2);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
                var dependentEntry2 = context.Entry(principal).Reference(p => p.Child2).TargetEntry;
                Assert.Equal(principal.Id, dependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, dependentEntry2.State);
                Assert.Equal(nameof(ParentPN.Child2), dependentEntry2.Metadata.DefiningNavigationName);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Identity_changed_bidirectional(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent
                {
                    Id = 77
                };
                var dependent = new Child
                {
                    Name = "1",
                    Parent = principal
                };
                principal.Child2 = dependent;

                context.ChangeTracker.TrackGraph(principal, e => e.Entry.State = entityState);

                var dependentEntry1 = context.Entry(principal).Reference(p => p.Child2).TargetEntry;

                principal.Child1 = dependent;
                principal.Child2 = null;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(entityState == EntityState.Added ? 2 : 3, context.ChangeTracker.Entries().Count());
                Assert.Null(principal.Child2);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child1);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
                var dependentEntry = context.Entry(principal).Reference(p => p.Child1).TargetEntry;
                Assert.Equal(principal.Id, dependentEntry.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, dependentEntry.State);
                Assert.Equal(nameof(Parent.Child1), dependentEntry.Metadata.DefiningNavigationName);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Identity_swapped_unidirectional(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentPN
                {
                    Id = 77
                };
                var dependent1 = new ChildPN
                {
                    Name = "1"
                };
                principal.Child1 = dependent1;
                var dependent2 = new ChildPN
                {
                    Name = "2"
                };
                principal.Child2 = dependent2;

                context.ChangeTracker.TrackGraph(principal, e => e.Entry.State = entityState);

                principal.Child2 = dependent1;
                principal.Child1 = dependent2;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
                Assert.Same(dependent1, principal.Child2);
                Assert.Same(dependent2, principal.Child1);
                Assert.Equal(entityState, context.Entry(principal).State);

                var dependent1Entry = context.Entry(principal).Reference(p => p.Child1).TargetEntry;
                Assert.Equal(principal.Id, dependent1Entry.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, dependent1Entry.State);
                Assert.Equal(nameof(Parent.Child1), dependent1Entry.Metadata.DefiningNavigationName);
                Assert.Equal(
                    entityState == EntityState.Added ? null : (EntityState?)EntityState.Deleted,
                    dependent1Entry.GetInfrastructure().SharedIdentityEntry?.EntityState);

                var dependent2Entry = context.Entry(principal).Reference(p => p.Child2).TargetEntry;
                Assert.Equal(principal.Id, dependent2Entry.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, dependent2Entry.State);
                Assert.Equal(nameof(Parent.Child2), dependent2Entry.Metadata.DefiningNavigationName);
                Assert.Equal(
                    entityState == EntityState.Added ? null : (EntityState?)EntityState.Deleted,
                    dependent2Entry.GetInfrastructure().SharedIdentityEntry?.EntityState);

                dependent1Entry.GetInfrastructure().AcceptChanges();
                dependent2Entry.GetInfrastructure().AcceptChanges();
                Assert.Null(dependent1Entry.GetInfrastructure().SharedIdentityEntry);
                Assert.Null(dependent2Entry.GetInfrastructure().SharedIdentityEntry);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Identity_swapped_bidirectional(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent
                {
                    Id = 77
                };
                var dependent1 = new Child
                {
                    Name = "1",
                    Parent = principal
                };
                principal.Child1 = dependent1;
                var dependent2 = new Child
                {
                    Name = "2",
                    Parent = principal
                };
                principal.Child2 = dependent2;

                context.ChangeTracker.TrackGraph(principal, e => e.Entry.State = entityState);

                principal.Child2 = dependent1;
                principal.Child1 = dependent2;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
                Assert.Same(principal, dependent1.Parent);
                Assert.Same(dependent1, principal.Child2);
                Assert.Same(principal, dependent2.Parent);
                Assert.Same(dependent2, principal.Child1);
                Assert.Equal(entityState, context.Entry(principal).State);

                var dependent1Entry = context.Entry(principal).Reference(p => p.Child1).TargetEntry;
                Assert.Equal(principal.Id, dependent1Entry.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, dependent1Entry.State);
                Assert.Equal(nameof(Parent.Child1), dependent1Entry.Metadata.DefiningNavigationName);
                Assert.Equal(
                    entityState == EntityState.Added ? null : (EntityState?)EntityState.Deleted,
                    dependent1Entry.GetInfrastructure().SharedIdentityEntry?.EntityState);

                var dependent2Entry = context.Entry(principal).Reference(p => p.Child2).TargetEntry;
                Assert.Equal(principal.Id, dependent2Entry.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, dependent2Entry.State);
                Assert.Equal(nameof(Parent.Child2), dependent2Entry.Metadata.DefiningNavigationName);
                Assert.Equal(
                    entityState == EntityState.Added ? null : (EntityState?)EntityState.Deleted,
                    dependent2Entry.GetInfrastructure().SharedIdentityEntry?.EntityState);

                dependent1Entry.GetInfrastructure().AcceptChanges();
                dependent2Entry.GetInfrastructure().AcceptChanges();
                Assert.Null(dependent1Entry.GetInfrastructure().SharedIdentityEntry);
                Assert.Null(dependent2Entry.GetInfrastructure().SharedIdentityEntry);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Parent_changed_unidirectional(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal1 = new ParentPN
                {
                    Id = 77
                };
                var principal2 = new ParentPN
                {
                    Id = 78
                };
                var dependent = new ChildPN
                {
                    Name = "1"
                };
                principal1.Child1 = dependent;

                context.ChangeTracker.TrackGraph(principal1, e => e.Entry.State = entityState);
                context.ChangeTracker.TrackGraph(principal2, e => e.Entry.State = entityState);

                var dependentEntry1 = context.Entry(principal1).Reference(p => p.Child1).TargetEntry;

                principal2.Child1 = dependent;
                principal1.Child1 = null;

                if (entityState != EntityState.Added)
                {
                    Assert.Equal(
                        CoreStrings.KeyReadOnly("ParentId", dependentEntry1.Metadata.DisplayName()),
                        Assert.Throws<InvalidOperationException>(() => context.ChangeTracker.DetectChanges()).Message);
                }
                else
                {
                    context.ChangeTracker.DetectChanges();

                    Assert.Equal(3, context.ChangeTracker.Entries().Count());
                    Assert.Null(principal1.Child1);
                    Assert.Null(principal1.Child2);
                    Assert.Same(dependent, principal2.Child1);
                    Assert.Null(principal2.Child2);
                    Assert.Equal(entityState, context.Entry(principal1).State);
                    Assert.Equal(entityState, context.Entry(principal2).State);
                    Assert.Equal(EntityState.Detached, dependentEntry1.State);
                    var dependentEntry2 = context.Entry(principal2).Reference(p => p.Child1).TargetEntry;
                    Assert.Equal(principal2.Id, dependentEntry2.Property("ParentId").CurrentValue);
                    Assert.Equal(EntityState.Added, dependentEntry2.State);
                    Assert.Equal(nameof(Parent.Child1), dependentEntry2.Metadata.DefiningNavigationName);
                }
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Parent_changed_bidirectional(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal1 = new Parent
                {
                    Id = 77
                };
                var principal2 = new Parent
                {
                    Id = 78
                };
                var dependent = new Child
                {
                    Name = "1"
                };
                principal1.Child1 = dependent;

                context.ChangeTracker.TrackGraph(principal1, e => e.Entry.State = entityState);
                context.ChangeTracker.TrackGraph(principal2, e => e.Entry.State = entityState);

                var dependentEntry1 = context.Entry(principal1).Reference(p => p.Child1).TargetEntry;

                principal2.Child1 = dependent;
                principal1.Child1 = null;

                if (entityState != EntityState.Added)
                {
                    Assert.Equal(
                        CoreStrings.KeyReadOnly("ParentId", dependentEntry1.Metadata.DisplayName()),
                        Assert.Throws<InvalidOperationException>(() => context.ChangeTracker.DetectChanges()).Message);
                }
                else
                {
                    context.ChangeTracker.DetectChanges();

                    Assert.Equal(3, context.ChangeTracker.Entries().Count());
                    Assert.Null(principal1.Child1);
                    Assert.Null(principal1.Child2);
                    Assert.Same(dependent, principal2.Child1);
                    Assert.Null(principal2.Child2);
                    Assert.Same(principal2, dependent.Parent);
                    Assert.Equal(entityState, context.Entry(principal1).State);
                    Assert.Equal(entityState, context.Entry(principal2).State);
                    Assert.Equal(EntityState.Detached, dependentEntry1.State);
                    var dependentEntry2 = context.Entry(principal2).Reference(p => p.Child1).TargetEntry;
                    Assert.Equal(EntityState.Added, dependentEntry2.State);
                    Assert.Equal(principal2.Id, dependentEntry2.Property("ParentId").CurrentValue);
                    Assert.Equal(nameof(Parent.Child1), dependentEntry2.Metadata.DefiningNavigationName);
                }
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Parent_swapped_unidirectional(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal1 = new ParentPN
                {
                    Id = 77
                };
                var principal2 = new ParentPN
                {
                    Id = 78
                };
                var dependent1 = new ChildPN
                {
                    Name = "1"
                };
                principal1.Child1 = dependent1;
                var dependent2 = new ChildPN
                {
                    Name = "2"
                };
                principal2.Child1 = dependent2;

                context.ChangeTracker.TrackGraph(principal1, e => e.Entry.State = entityState);
                context.ChangeTracker.TrackGraph(principal2, e => e.Entry.State = entityState);

                principal1.Child1 = dependent2;
                principal2.Child1 = dependent1;

                if (entityState != EntityState.Added)
                {
                    Assert.Equal(
                        CoreStrings.KeyReadOnly(
                            "ParentId",
                            context.Entry(principal1).Reference(p => p.Child1).Metadata.GetTargetType().DisplayName()),
                        Assert.Throws<InvalidOperationException>(() => context.ChangeTracker.DetectChanges()).Message);
                }
                else
                {
                    context.ChangeTracker.DetectChanges();

                    Assert.Equal(4, context.ChangeTracker.Entries().Count());
                    Assert.Same(dependent2, principal1.Child1);
                    Assert.Null(principal1.Child2);
                    Assert.Same(dependent1, principal2.Child1);
                    Assert.Null(principal2.Child2);
                    Assert.Equal(entityState, context.Entry(principal1).State);
                    Assert.Equal(entityState, context.Entry(principal2).State);
                    var dependent1Entry = context.Entry(principal1).Reference(p => p.Child1).TargetEntry;
                    Assert.Equal(principal1.Id, dependent1Entry.Property("ParentId").CurrentValue);
                    Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, dependent1Entry.State);
                    Assert.Equal(nameof(Parent.Child1), dependent1Entry.Metadata.DefiningNavigationName);
                    var dependent2Entry = context.Entry(principal2).Reference(p => p.Child1).TargetEntry;
                    Assert.Equal(principal2.Id, dependent2Entry.Property("ParentId").CurrentValue);
                    Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, dependent2Entry.State);
                    Assert.Equal(nameof(Parent.Child1), dependent2Entry.Metadata.DefiningNavigationName);
                }
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Parent_swapped_bidirectional(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal1 = new Parent
                {
                    Id = 77
                };
                var principal2 = new Parent
                {
                    Id = 78
                };
                var dependent1 = new Child
                {
                    Name = "1"
                };
                principal1.Child1 = dependent1;
                var dependent2 = new Child
                {
                    Name = "2"
                };
                principal2.Child1 = dependent2;

                context.ChangeTracker.TrackGraph(principal1, e => e.Entry.State = entityState);
                context.ChangeTracker.TrackGraph(principal2, e => e.Entry.State = entityState);

                principal1.Child1 = dependent2;
                principal2.Child1 = dependent1;

                if (entityState != EntityState.Added)
                {
                    Assert.Equal(
                        CoreStrings.KeyReadOnly(
                            "ParentId",
                            context.Entry(principal1).Reference(p => p.Child1).Metadata.GetTargetType().DisplayName()),
                        Assert.Throws<InvalidOperationException>(() => context.ChangeTracker.DetectChanges()).Message);
                }
                else
                {
                    context.ChangeTracker.DetectChanges();

                    Assert.Equal(4, context.ChangeTracker.Entries().Count());
                    Assert.Same(dependent2, principal1.Child1);
                    Assert.Null(principal1.Child2);
                    Assert.Same(dependent1, principal2.Child1);
                    Assert.Null(principal2.Child2);
                    Assert.Same(principal2, dependent1.Parent);
                    Assert.Same(principal1, dependent2.Parent);
                    Assert.Equal(entityState, context.Entry(principal1).State);
                    Assert.Equal(entityState, context.Entry(principal2).State);
                    var dependent1Entry = context.Entry(principal1).Reference(p => p.Child1).TargetEntry;
                    Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, dependent1Entry.State);
                    Assert.Equal(principal1.Id, dependent1Entry.Property("ParentId").CurrentValue);
                    Assert.Equal(nameof(Parent.Child1), dependent1Entry.Metadata.DefiningNavigationName);
                    var dependent2Entry = context.Entry(principal2).Reference(p => p.Child1).TargetEntry;
                    Assert.Equal(principal2.Id, dependent2Entry.Property("ParentId").CurrentValue);
                    Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, dependent2Entry.State);
                    Assert.Equal(nameof(Parent.Child1), dependent2Entry.Metadata.DefiningNavigationName);
                }
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Parent_and_identity_changed_unidirectional(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal1 = new ParentPN
                {
                    Id = 77
                };
                var principal2 = new ParentPN
                {
                    Id = 78
                };
                var dependent = new ChildPN
                {
                    Name = "1"
                };
                principal1.Child2 = dependent;

                context.ChangeTracker.TrackGraph(principal1, e => e.Entry.State = entityState);
                context.ChangeTracker.TrackGraph(principal2, e => e.Entry.State = entityState);

                var dependentEntry1 = context.Entry(principal1).Reference(p => p.Child2).TargetEntry;

                principal2.Child1 = dependent;
                principal1.Child2 = null;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(entityState == EntityState.Added ? 3 : 4, context.ChangeTracker.Entries().Count());
                Assert.Null(principal1.Child1);
                Assert.Null(principal1.Child2);
                Assert.Same(dependent, principal2.Child1);
                Assert.Null(principal2.Child2);
                Assert.Equal(entityState, context.Entry(principal1).State);
                Assert.Equal(entityState, context.Entry(principal2).State);
                Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
                var dependentEntry2 = context.Entry(principal2).Reference(p => p.Child1).TargetEntry;
                Assert.Equal(principal2.Id, dependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, dependentEntry2.State);
                Assert.Equal(nameof(Parent.Child1), dependentEntry2.Metadata.DefiningNavigationName);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Parent_and_identity_changed_bidirectional(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal1 = new Parent
                {
                    Id = 77
                };
                var principal2 = new Parent
                {
                    Id = 78
                };
                var dependent = new Child
                {
                    Name = "1",
                    Parent = principal1
                };
                principal1.Child2 = dependent;

                context.ChangeTracker.TrackGraph(principal1, e => e.Entry.State = entityState);
                context.ChangeTracker.TrackGraph(principal2, e => e.Entry.State = entityState);

                var dependentEntry1 = context.Entry(principal1).Reference(p => p.Child2).TargetEntry;

                principal2.Child1 = dependent;
                principal1.Child2 = null;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(entityState == EntityState.Added ? 3 : 4, context.ChangeTracker.Entries().Count());
                Assert.Null(principal1.Child1);
                Assert.Null(principal1.Child2);
                Assert.Same(dependent, principal2.Child1);
                Assert.Null(principal2.Child2);
                Assert.Same(principal2, dependent.Parent);
                Assert.Equal(entityState, context.Entry(principal1).State);
                Assert.Equal(entityState, context.Entry(principal2).State);
                Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
                var dependentEntry2 = context.Entry(principal2).Reference(p => p.Child1).TargetEntry;
                Assert.Equal(principal2.Id, dependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, dependentEntry2.State);
                Assert.Equal(nameof(Parent.Child1), dependentEntry2.Metadata.DefiningNavigationName);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Parent_and_identity_swapped_unidirectional(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal1 = new ParentPN
                {
                    Id = 77
                };
                var principal2 = new ParentPN
                {
                    Id = 78
                };
                var dependent1 = new ChildPN
                {
                    Name = "1"
                };
                principal1.Child2 = dependent1;
                var dependent2 = new ChildPN
                {
                    Name = "2"
                };
                principal2.Child1 = dependent2;

                context.ChangeTracker.TrackGraph(principal1, e => e.Entry.State = entityState);
                context.ChangeTracker.TrackGraph(principal2, e => e.Entry.State = entityState);

                principal2.Child1 = dependent1;
                principal1.Child2 = dependent2;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(4, context.ChangeTracker.Entries().Count());
                Assert.Null(principal1.Child1);
                Assert.Same(dependent2, principal1.Child2);
                Assert.Same(dependent1, principal2.Child1);
                Assert.Null(principal2.Child2);
                Assert.Equal(entityState, context.Entry(principal1).State);
                Assert.Equal(entityState, context.Entry(principal2).State);

                var dependent1Entry = context.Entry(principal1).Reference(p => p.Child2).TargetEntry;
                Assert.Equal(EntityState.Added, dependent1Entry.State);
                Assert.Equal(principal1.Id, dependent1Entry.Property("ParentId").CurrentValue);
                Assert.Equal(nameof(Parent.Child2), dependent1Entry.Metadata.DefiningNavigationName);
                Assert.Equal(
                    entityState == EntityState.Added ? null : (EntityState?)EntityState.Deleted,
                    dependent1Entry.GetInfrastructure().SharedIdentityEntry?.EntityState);

                var dependent2Entry = context.Entry(principal2).Reference(p => p.Child1).TargetEntry;
                Assert.Equal(principal2.Id, dependent2Entry.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, dependent2Entry.State);
                Assert.Equal(nameof(Parent.Child1), dependent2Entry.Metadata.DefiningNavigationName);
                Assert.Equal(
                    entityState == EntityState.Added ? null : (EntityState?)EntityState.Deleted,
                    dependent2Entry.GetInfrastructure().SharedIdentityEntry?.EntityState);

                dependent1Entry.GetInfrastructure().AcceptChanges();
                dependent2Entry.GetInfrastructure().AcceptChanges();
                Assert.Null(dependent1Entry.GetInfrastructure().SharedIdentityEntry);
                Assert.Null(dependent2Entry.GetInfrastructure().SharedIdentityEntry);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Parent_and_identity_swapped_bidirectional(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal1 = new Parent
                {
                    Id = 77
                };
                var principal2 = new Parent
                {
                    Id = 78
                };
                var dependent1 = new Child
                {
                    Name = "1",
                    Parent = principal1
                };
                principal1.Child2 = dependent1;
                var dependent2 = new Child
                {
                    Name = "2"
                };
                principal2.Child1 = dependent2;

                context.ChangeTracker.TrackGraph(principal1, e => e.Entry.State = entityState);
                context.ChangeTracker.TrackGraph(principal2, e => e.Entry.State = entityState);

                principal2.Child1 = dependent1;
                principal1.Child2 = dependent2;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(4, context.ChangeTracker.Entries().Count());
                Assert.Null(principal1.Child1);
                Assert.Same(dependent2, principal1.Child2);
                Assert.Same(dependent1, principal2.Child1);
                Assert.Null(principal2.Child2);
                Assert.Same(principal2, dependent1.Parent);
                Assert.Same(principal1, dependent2.Parent);
                Assert.Equal(entityState, context.Entry(principal1).State);
                Assert.Equal(entityState, context.Entry(principal2).State);

                var dependent1Entry = context.Entry(principal1).Reference(p => p.Child2).TargetEntry;
                Assert.Equal(EntityState.Added, dependent1Entry.State);
                Assert.Equal(principal1.Id, dependent1Entry.Property("ParentId").CurrentValue);
                Assert.Equal(nameof(Parent.Child2), dependent1Entry.Metadata.DefiningNavigationName);
                Assert.Equal(
                    entityState == EntityState.Added ? null : (EntityState?)EntityState.Deleted,
                    dependent1Entry.GetInfrastructure().SharedIdentityEntry?.EntityState);

                var dependent2Entry = context.Entry(principal2).Reference(p => p.Child1).TargetEntry;
                Assert.Equal(principal2.Id, dependent2Entry.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, dependent2Entry.State);
                Assert.Equal(nameof(Parent.Child1), dependent2Entry.Metadata.DefiningNavigationName);
                Assert.Equal(
                    entityState == EntityState.Added ? null : (EntityState?)EntityState.Deleted,
                    dependent1Entry.GetInfrastructure().SharedIdentityEntry?.EntityState);

                dependent1Entry.GetInfrastructure().AcceptChanges();
                dependent2Entry.GetInfrastructure().AcceptChanges();
                Assert.Null(dependent1Entry.GetInfrastructure().SharedIdentityEntry);
                Assert.Null(dependent2Entry.GetInfrastructure().SharedIdentityEntry);
            }
        }

        private class Parent
        {
            public int Id { get; set; }

            public Child Child1 { get; set; }
            public Child Child2 { get; set; }

            public override bool Equals(object obj)
            {
                Assert.False(true);
                return false;
            }

            public override int GetHashCode()
            {
                Assert.False(true);
                return base.GetHashCode();
            }

            public static bool operator ==(Parent _, Parent __)
            {
                Assert.False(true);
                return false;
            }

            public static bool operator !=(Parent _, Parent __)
            {
                Assert.False(true);
                return true;
            }
        }

        private class Child
        {
            public string Name { get; set; }

            public Parent Parent { get; set; }
            public SubChild SubChild { get; set; }

            public override bool Equals(object obj)
            {
                Assert.False(true);
                return false;
            }

            public override int GetHashCode()
            {
                Assert.False(true);
                return base.GetHashCode();
            }

            public static bool operator ==(Child _, Child __)
            {
                Assert.False(true);
                return false;
            }

            public static bool operator !=(Child _, Child __)
            {
                Assert.False(true);
                return true;
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class SubChild
        {
            // ReSharper disable once UnusedMember.Local
            public string Name { get; set; }

            public Child Child { get; set; }

            public override bool Equals(object obj)
            {
                Assert.False(true);
                return false;
            }

            public override int GetHashCode()
            {
                Assert.False(true);
                return base.GetHashCode();
            }

            public static bool operator ==(SubChild _, SubChild __)
            {
                Assert.False(true);
                return false;
            }

            public static bool operator !=(SubChild _, SubChild __)
            {
                Assert.False(true);
                return true;
            }
        }

        private class ParentPN
        {
            public int Id { get; set; }

            public ChildPN Child1 { get; set; }
            public ChildPN Child2 { get; set; }
        }

        private class ChildPN
        {
            public string Name { get; set; }

            // ReSharper disable once MemberHidesStaticFromOuterClass
            public SubChildPN SubChild { get; set; }
        }

        private class SubChildPN
        {
            public string Name { get; set; }
        }

        private class FixupContext : DbContext
        {
            private readonly bool _ignoreDuplicates;

            public FixupContext(bool ignoreDuplicates = true)
            {
                _ignoreDuplicates = ignoreDuplicates;

                // ReSharper disable once VirtualMemberCallInConstructor
                ChangeTracker.AutoDetectChangesEnabled = false;
            }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Parent>(
                    pb =>
                    {
                        pb.Property(p => p.Id).ValueGeneratedNever();
                        pb.OwnsOne(
                            p => p.Child1, cb =>
                            {
                                cb.Property<int?>("ParentId");
                                cb.WithOwner(c => c.Parent)
                                  .HasForeignKey("ParentId");

                                cb.OwnsOne(c => c.SubChild)
                                  .WithOwner(c => c.Child)
                                  .HasForeignKey("ChildId");
                            });
                        pb.OwnsOne(
                            p => p.Child2, cb =>
                            {
                                cb.Property<int?>("ParentId");
                                cb.WithOwner(c => c.Parent)
                                  .HasForeignKey("ParentId");

                                cb.OwnsOne(c => c.SubChild)
                                  .WithOwner(c => c.Child)
                                  .HasForeignKey("ChildId");
                            });
                    });

                modelBuilder.Entity<ParentPN>(
                    pb =>
                    {
                        pb.Property(p => p.Id).ValueGeneratedNever();
                        pb.OwnsOne(
                            p => p.Child1, cb =>
                            {
                                cb.Property<int?>("ParentId");
                                cb.WithOwner()
                                  .HasForeignKey("ParentId");

                                cb.OwnsOne(c => c.SubChild)
                                  .WithOwner()
                                  .HasForeignKey("ChildId");
                            });
                        pb.OwnsOne(
                            p => p.Child2, cb =>
                            {
                                cb.Property<int?>("ParentId");
                                cb.WithOwner()
                                  .HasForeignKey("ParentId");

                                cb.OwnsOne(c => c.SubChild)
                                  .WithOwner()
                                  .HasForeignKey("ChildId");
                            });
                    });
            }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseInMemoryDatabase(nameof(FixupContext))
                    .UseInternalServiceProvider(InMemoryFixture.BuildServiceProvider());

                if (!_ignoreDuplicates)
                {
                    optionsBuilder.ConfigureWarnings(
                        w => w.Default(WarningBehavior.Throw).Log(CoreEventId.ManyServiceProvidersCreatedWarning));
                }
            }
        }

        private class SensitiveFixupContext : FixupContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.EnableSensitiveDataLogging();

                base.OnConfiguring(optionsBuilder);
            }
        }

        private void AssertFixup(DbContext context, Action asserts)
        {
            asserts();
            context.ChangeTracker.DetectChanges();
            asserts();
            context.ChangeTracker.DetectChanges();
            asserts();
            context.ChangeTracker.DetectChanges();
            asserts();
        }
    }
}
