// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
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
                        ".Reference().TargetEntry",
                        ".Collection().GetTargetEntry()"),
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
                        CoreResources.LogDuplicateDependentEntityTypeInstance(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                            typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.Child2) + "#" + typeof(ChildPN).ShortDisplayName(),
                            typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.Child1) + "#" + typeof(ChildPN).ShortDisplayName()),
                        "CoreEventId.DuplicateDependentEntityTypeInstanceWarning"),
                    Assert.Throws<InvalidOperationException>(() => context.Entry(principal).Reference(p => p.Child2).TargetEntry).Message);
            }
        }

        [Theory]
        [InlineData(EntityState.Added, true)]
        [InlineData(EntityState.Added, false)]
        [InlineData(EntityState.Added, null)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Modified, null)]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Unchanged, null)]
        public void Add_principal_with_dependent_unidirectional_nav(EntityState entityState, bool? useTrackGraph)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentPN
                {
                    Id = 77
                };
                if (useTrackGraph == null)
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

                if (useTrackGraph == null)
                {
                    context.ChangeTracker.DetectChanges();
                }
                else if (useTrackGraph == true)
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
                        Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, dependentEntry.State);
                        Assert.Equal(nameof(ParentPN.Child1), dependentEntry.Metadata.DefiningNavigationName);

                        Assert.Same(subDependent, dependent.SubChild);
                        var subDependentEntry = context.Entry(subDependent);
                        Assert.Equal(principal.Id, subDependentEntry.Property("ParentId").CurrentValue);
                        Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, subDependentEntry.State);
                        Assert.Equal(nameof(ChildPN.SubChild), subDependentEntry.Metadata.DefiningNavigationName);
                    });
            }
        }

        [Theory]
        [InlineData(EntityState.Added, true)]
        [InlineData(EntityState.Added, false)]
        [InlineData(EntityState.Added, null)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Modified, null)]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Unchanged, null)]
        public void Add_principal_with_dependent_both_navs(EntityState entityState, bool? useTrackGraph)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent
                {
                    Id = 77
                };
                if (useTrackGraph == null)
                {
                    context.Entry(principal).State = entityState;
                }

                var dependent = new Child
                {
                    Name = "1",
                    Parent = principal
                };
                principal.Child1 = dependent;

                var subDependent = new SubChild
                {
                    Name = "1S",
                    Parent = dependent
                };
                dependent.SubChild = subDependent;

                if (useTrackGraph == null)
                {
                    context.ChangeTracker.DetectChanges();
                }
                else if (useTrackGraph == true)
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
                        Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                        Assert.Same(dependent, principal.Child1);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Equal(entityState, context.Entry(principal).State);
                        Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, context.Entry(dependent).State);

                        Assert.Same(subDependent, dependent.SubChild);
                        Assert.Same(dependent, subDependent.Parent);
                        var subDependentEntry = context.Entry(subDependent);
                        Assert.Equal(principal.Id, subDependentEntry.Property("ParentId").CurrentValue);
                        Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, subDependentEntry.State);
                        Assert.Equal(nameof(ChildPN.SubChild), subDependentEntry.Metadata.DefiningNavigationName);
                    });
            }
        }

        [Theory]
        [InlineData(EntityState.Added, true)]
        [InlineData(EntityState.Added, false)]
        [InlineData(EntityState.Added, null)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Modified, null)]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Unchanged, null)]
        public void Add_principal_with_dependent_principal_nav(EntityState entityState, bool? useTrackGraph)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent
                {
                    Id = 77
                };
                if (useTrackGraph == null)
                {
                    context.Entry(principal).State = entityState;
                }

                var dependent = new Child
                {
                    Name = "1"
                };
                principal.Child1 = dependent;

                var subDependent = new SubChild
                {
                    Name = "1S"
                };
                dependent.SubChild = subDependent;

                if (useTrackGraph == null)
                {
                    context.ChangeTracker.DetectChanges();
                }
                else if (useTrackGraph == true)
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
                        Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                        Assert.Same(dependent, principal.Child1);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Equal(entityState, context.Entry(principal).State);
                        Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, context.Entry(dependent).State);

                        Assert.Same(subDependent, dependent.SubChild);
                        Assert.Same(dependent, subDependent.Parent);
                        var subDependentEntry = context.Entry(subDependent);
                        Assert.Equal(principal.Id, subDependentEntry.Property("ParentId").CurrentValue);
                        Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, subDependentEntry.State);
                        Assert.Equal(nameof(ChildPN.SubChild), subDependentEntry.Metadata.DefiningNavigationName);
                    });
            }
        }

        [Theory]
        [InlineData(EntityState.Added, true)]
        [InlineData(EntityState.Added, false)]
        [InlineData(EntityState.Added, null)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Modified, null)]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Unchanged, null)]
        public void Add_principal_with_dependent_unidirectional_nav_collection(EntityState entityState, bool? useTrackGraph)
        {
            using (var context = new FixupContext())
            {
                var principal = new ParentPN
                {
                    Id = 77
                };
                if (useTrackGraph == null)
                {
                    context.Entry(principal).State = entityState;
                }

                var dependent = new ChildPN
                {
                    Name = "1"
                };
                principal.ChildCollection1 = new HashSet<ChildPN>(ReferenceEqualityComparer.Instance) { dependent};

                var subDependent = new SubChildPN
                {
                    Name = "1S"
                };
                dependent.SubChildCollection = new HashSet<SubChildPN>(ReferenceEqualityComparer.Instance) { subDependent };

                if (useTrackGraph == null)
                {
                    context.ChangeTracker.DetectChanges();
                }
                else if (useTrackGraph == true)
                {
                    context.ChangeTracker.TrackGraph(principal, e =>
                    {
                        if (entityState != EntityState.Added)
                        {
                            if (ReferenceEquals(e.Entry.Entity, dependent))
                            {
                                e.Entry.Property("Id").CurrentValue = 10;
                            }
                            else if (ReferenceEquals(e.Entry.Entity, subDependent))
                            {
                                e.Entry.Property("Id").CurrentValue = 100;
                            }
                        }
                        e.Entry.State = entityState;
                    });
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

                        Assert.Contains(dependent, principal.ChildCollection1);
                        Assert.Null(principal.ChildCollection2);
                        var dependentEntry = context.Entry(dependent);
                        Assert.Equal(principal.Id, dependentEntry.Property("ParentId").CurrentValue);
                        Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, dependentEntry.State);
                        Assert.Equal(nameof(ParentPN.ChildCollection1), dependentEntry.Metadata.DefiningNavigationName);

                        Assert.Contains(subDependent, dependent.SubChildCollection);
                        var subDependentEntry = context.Entry(subDependent);
                        Assert.Equal(principal.Id, subDependentEntry.Property("ParentId").CurrentValue);
                        Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, subDependentEntry.State);
                        Assert.Equal(nameof(ChildPN.SubChildCollection), subDependentEntry.Metadata.DefiningNavigationName);
                    });
            }
        }

        [Theory]
        [InlineData(EntityState.Added, true)]
        [InlineData(EntityState.Added, false)]
        [InlineData(EntityState.Added, null)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Modified, null)]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Unchanged, null)]
        public void Add_principal_with_dependent_both_navs_collection(EntityState entityState, bool? useTrackGraph)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent
                {
                    Id = 77
                };
                if (useTrackGraph == null)
                {
                    context.Entry(principal).State = entityState;
                }

                var dependent = new Child
                {
                    Name = "1",
                    Parent = principal
                };
                principal.ChildCollection1 = new HashSet<Child>(ReferenceEqualityComparer.Instance) { dependent };

                var subDependent = new SubChild
                {
                    Name = "1S",
                    Parent = dependent
                };
                dependent.SubChildCollection = new HashSet<SubChild>(ReferenceEqualityComparer.Instance) { subDependent };

                if (useTrackGraph == null)
                {
                    context.ChangeTracker.DetectChanges();
                }
                else if (useTrackGraph == true)
                {
                    context.ChangeTracker.TrackGraph(principal, e =>
                    {
                        if (entityState != EntityState.Added)
                        {
                            if (ReferenceEquals(e.Entry.Entity, dependent))
                            {
                                e.Entry.Property("Id").CurrentValue = 10;
                            }
                            else if (ReferenceEquals(e.Entry.Entity, subDependent))
                            {
                                e.Entry.Property("Id").CurrentValue = 100;
                            }
                        }
                        e.Entry.State = entityState;
                    });
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
                        Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                        Assert.Contains(dependent, principal.ChildCollection1);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Null(principal.ChildCollection2);
                        Assert.Equal(entityState, context.Entry(principal).State);
                        Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, context.Entry(dependent).State);

                        Assert.Contains(subDependent, dependent.SubChildCollection);
                        Assert.Same(dependent, subDependent.Parent);
                        var subDependentEntry = context.Entry(subDependent);
                        Assert.Equal(principal.Id, subDependentEntry.Property("ParentId").CurrentValue);
                        Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, subDependentEntry.State);
                        Assert.Equal(nameof(ChildPN.SubChildCollection), subDependentEntry.Metadata.DefiningNavigationName);
                    });
            }
        }

        [Theory]
        [InlineData(EntityState.Added, true)]
        [InlineData(EntityState.Added, false)]
        [InlineData(EntityState.Added, null)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Modified, null)]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Unchanged, null)]
        public void Add_principal_with_dependent_principal_nav_collection(EntityState entityState, bool? useTrackGraph)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent
                {
                    Id = 77
                };
                if (useTrackGraph == null)
                {
                    context.Entry(principal).State = entityState;
                }

                var dependent = new Child
                {
                    Name = "1"
                };
                principal.ChildCollection1 = new HashSet<Child>(ReferenceEqualityComparer.Instance) { dependent };

                var subDependent = new SubChild
                {
                    Name = "1S"
                };
                dependent.SubChildCollection = new HashSet<SubChild>(ReferenceEqualityComparer.Instance) { subDependent };

                if (useTrackGraph == null)
                {
                    context.ChangeTracker.DetectChanges();
                }
                else if (useTrackGraph == true)
                {
                    context.ChangeTracker.TrackGraph(principal, e =>
                    {
                        if (entityState != EntityState.Added)
                        {
                            if (ReferenceEquals(e.Entry.Entity, dependent))
                            {
                                e.Entry.Property("Id").CurrentValue = 10;
                            }
                            else if (ReferenceEquals(e.Entry.Entity, subDependent))
                            {
                                e.Entry.Property("Id").CurrentValue = 100;
                            }
                        }
                        e.Entry.State = entityState;
                    });
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
                        Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                        Assert.Contains(dependent, principal.ChildCollection1);
                        Assert.Null(principal.ChildCollection2);
                        Assert.Same(principal, dependent.Parent);
                        Assert.Equal(entityState, context.Entry(principal).State);
                        Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, context.Entry(dependent).State);

                        Assert.Contains(subDependent, dependent.SubChildCollection);
                        Assert.Same(dependent, subDependent.Parent);
                        var subDependentEntry = context.Entry(subDependent);
                        Assert.Equal(principal.Id, subDependentEntry.Property("ParentId").CurrentValue);
                        Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, subDependentEntry.State);
                        Assert.Equal(nameof(ChildPN.SubChildCollection), subDependentEntry.Metadata.DefiningNavigationName);
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
        public void Instance_changed_unidirectional(EntityState entityState)
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
                principal.Child2 = dependent1;

                var subDependent1 = new SubChildPN
                {
                    Name = "1S"
                };
                dependent1.SubChild = subDependent1;

                context.ChangeTracker.TrackGraph(principal, e => e.Entry.State = entityState);

                var dependentEntry1 = context.Entry(principal).Reference(p => p.Child2).TargetEntry;

                var dependent2 = new ChildPN
                {
                    Name = "2"
                };
                principal.Child2 = dependent2;

                var subDependent2 = new SubChildPN
                {
                    Name = "2S"
                };
                dependent2.SubChild = subDependent2;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
                Assert.Null(principal.Child1);
                Assert.Same(dependent2, principal.Child2);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
                var dependentEntry2 = context.Entry(principal).Reference(p => p.Child2).TargetEntry;
                Assert.Equal(principal.Id, dependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, dependentEntry2.State);
                Assert.Equal(nameof(ParentPN.Child2), dependentEntry2.Metadata.DefiningNavigationName);

                Assert.Same(subDependent2, dependent2.SubChild);
                var subDependentEntry = dependentEntry2.Reference(p => p.SubChild).TargetEntry;
                Assert.Equal(principal.Id, subDependentEntry.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, subDependentEntry.State);
                Assert.Equal(nameof(ChildPN.SubChild), subDependentEntry.Metadata.DefiningNavigationName);

                context.ChangeTracker.CascadeChanges();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());

                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
                Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                Assert.Null(principal.Child1);
                Assert.Same(dependent2, principal.Child2);
                Assert.Same(subDependent2, dependent2.SubChild);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Instance_changed_bidirectional(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent
                {
                    Id = 77
                };

                var dependent1 = new Child
                {
                    Name = "1"
                };
                principal.Child1 = dependent1;

                var subDependent1 = new SubChild
                {
                    Name = "1S"
                };
                dependent1.SubChild = subDependent1;

                context.ChangeTracker.TrackGraph(principal, e => e.Entry.State = entityState);

                var dependentEntry1 = context.Entry(principal).Reference(p => p.Child1).TargetEntry;

                var dependent2 = new Child
                {
                    Name = "2"
                };
                principal.Child1 = dependent2;

                var subDependent2 = new SubChild
                {
                    Name = "2S"
                };
                dependent2.SubChild = subDependent2;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
                Assert.Null(principal.Child2);
                Assert.Same(principal, dependent2.Parent);
                Assert.Same(dependent2, principal.Child1);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
                var dependentEntry2 = context.Entry(principal).Reference(p => p.Child1).TargetEntry;
                Assert.Equal(principal.Id, dependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, dependentEntry2.State);
                Assert.Equal(nameof(Parent.Child1), dependentEntry2.Metadata.DefiningNavigationName);

                Assert.Same(subDependent2, dependent2.SubChild);
                Assert.Same(dependent2, subDependent2.Parent);
                var subDependentEntry = dependentEntry2.Reference(p => p.SubChild).TargetEntry;
                Assert.Equal(principal.Id, subDependentEntry.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, subDependentEntry.State);
                Assert.Equal(nameof(Child.SubChild), subDependentEntry.Metadata.DefiningNavigationName);

                context.ChangeTracker.CascadeChanges();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());

                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
                Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                Assert.Null(principal.Child2);
                Assert.Same(dependent2, principal.Child1);
                Assert.Same(dependent2, subDependent2.Parent);
                Assert.Same(subDependent2, dependent2.SubChild);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Instance_changed_unidirectional_collection(EntityState entityState)
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
                principal.ChildCollection2 = new HashSet<ChildPN>(ReferenceEqualityComparer.Instance) { dependent1 };

                var subDependent1 = new SubChildPN
                {
                    Name = "1S"
                };
                dependent1.SubChildCollection = new HashSet<SubChildPN>(ReferenceEqualityComparer.Instance) { subDependent1 };

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

                var dependentEntry1 = context.Entry(dependent1);
                var subDependentEntry1 = context.Entry(subDependent1);

                var dependent2 = new ChildPN
                {
                    Name = "2"
                };
                principal.ChildCollection2 = new HashSet<ChildPN>(ReferenceEqualityComparer.Instance) { dependent2 };

                var subDependent2 = new SubChildPN
                {
                    Name = "2S"
                };
                dependent2.SubChildCollection = new HashSet<SubChildPN>(ReferenceEqualityComparer.Instance) { subDependent2 };

                var dependentEntry2 = context.Entry(principal).Collection(p => p.ChildCollection2)
                    .GetTargetEntry(dependent2);
                dependentEntry2.Property<int>("Id").CurrentValue = dependentEntry1.Property<int>("Id").CurrentValue;

                var subDependentEntry2 = dependentEntry2.Collection(p => p.SubChildCollection)
                    .GetTargetEntry(subDependent2);
                subDependentEntry2.Property<int>("Id").CurrentValue = subDependentEntry1.Property<int>("Id").CurrentValue;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
                Assert.Null(principal.ChildCollection1);
                Assert.Contains(dependent2, principal.ChildCollection2);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
                Assert.Equal(principal.Id, dependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, dependentEntry2.State);
                Assert.Equal(nameof(ParentPN.ChildCollection2), dependentEntry2.Metadata.DefiningNavigationName);

                Assert.Contains(subDependent2, dependent2.SubChildCollection);
                Assert.Equal(principal.Id, subDependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, subDependentEntry2.State);
                Assert.Equal(nameof(ChildPN.SubChildCollection), subDependentEntry2.Metadata.DefiningNavigationName);

                context.ChangeTracker.CascadeChanges();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());

                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
                Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                Assert.Null(principal.ChildCollection1);
                Assert.Contains(dependent2, principal.ChildCollection2);
                Assert.Contains(subDependent2, dependent2.SubChildCollection);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Instance_changed_bidirectional_collection(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent
                {
                    Id = 77
                };

                var dependent1 = new Child
                {
                    Name = "1"
                };
                principal.ChildCollection1 = new HashSet<Child>(ReferenceEqualityComparer.Instance) {dependent1};

                var subDependent1 = new SubChild
                {
                    Name = "1S"
                };
                dependent1.SubChildCollection = new HashSet<SubChild>(ReferenceEqualityComparer.Instance) {subDependent1};

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

                var dependentEntry1 = context.Entry(dependent1);
                var subDependentEntry1 = context.Entry(subDependent1);

                var dependent2 = new Child
                {
                    Name = "2"
                };
                principal.ChildCollection1 = new HashSet<Child>(ReferenceEqualityComparer.Instance) {dependent2};

                var subDependent2 = new SubChild
                {
                    Name = "2S"
                };
                dependent2.SubChildCollection = new HashSet<SubChild>(ReferenceEqualityComparer.Instance) {subDependent2};

                var dependentEntry2 = context.Entry(principal).Collection(p => p.ChildCollection1)
                    .GetTargetEntry(dependent2);
                dependentEntry2.Property<int>("Id").CurrentValue = dependentEntry1.Property<int>("Id").CurrentValue;

                var subDependentEntry2 = dependentEntry2.Collection(p => p.SubChildCollection)
                    .GetTargetEntry(subDependent2);
                subDependentEntry2.Property<int>("Id").CurrentValue = subDependentEntry1.Property<int>("Id").CurrentValue;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
                Assert.Null(principal.ChildCollection2);
                Assert.Same(principal, dependent2.Parent);
                Assert.Contains(dependent2, principal.ChildCollection1);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
                Assert.Equal(principal.Id, dependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, dependentEntry2.State);
                Assert.Equal(nameof(Parent.ChildCollection1), dependentEntry2.Metadata.DefiningNavigationName);

                Assert.Contains(subDependent2, dependent2.SubChildCollection);
                Assert.Same(dependent2, subDependent2.Parent);
                Assert.Equal(principal.Id, subDependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, subDependentEntry2.State);
                Assert.Equal(nameof(Child.SubChildCollection), subDependentEntry2.Metadata.DefiningNavigationName);

                context.ChangeTracker.CascadeChanges();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());

                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
                Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                Assert.Null(principal.ChildCollection2);
                Assert.Contains(dependent2, principal.ChildCollection1);
                Assert.Same(dependent2, subDependent2.Parent);
                Assert.Contains(subDependent2, dependent2.SubChildCollection);
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

                var subDependent = new SubChildPN
                {
                    Name = "1S"
                };
                dependent.SubChild = subDependent;

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

                var dependentEntry1 = context.Entry(principal).Reference(p => p.Child1).TargetEntry;

                principal.Child1 = null;
                principal.Child2 = dependent;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(entityState == EntityState.Added ? 3 : 5, context.ChangeTracker.Entries().Count());
                Assert.Null(principal.Child1);
                Assert.Same(dependent, principal.Child2);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
                var dependentEntry2 = context.Entry(principal).Reference(p => p.Child2).TargetEntry;
                Assert.Equal(principal.Id, dependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, dependentEntry2.State);
                Assert.Equal(nameof(ParentPN.Child2), dependentEntry2.Metadata.DefiningNavigationName);

                Assert.Same(subDependent, dependent.SubChild);
                var subDependentEntry = dependentEntry2.Reference(p => p.SubChild).TargetEntry;
                Assert.Equal(principal.Id, subDependentEntry.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, subDependentEntry.State);
                Assert.Equal(nameof(ChildPN.SubChild), subDependentEntry.Metadata.DefiningNavigationName);

                context.ChangeTracker.CascadeChanges();

                Assert.Equal(entityState == EntityState.Added ? 3 : 5, context.ChangeTracker.Entries().Count());

                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
                Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                Assert.Null(principal.Child1);
                Assert.Same(dependent, principal.Child2);
                Assert.Same(subDependent, dependent.SubChild);
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
                    Name = "1"
                };
                principal.Child2 = dependent;

                var subDependent = new SubChild
                {
                    Name = "1S"
                };
                dependent.SubChild = subDependent;

                context.ChangeTracker.TrackGraph(principal, e => e.Entry.State = entityState);

                var dependentEntry1 = context.Entry(principal).Reference(p => p.Child2).TargetEntry;

                principal.Child1 = dependent;
                principal.Child2 = null;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(entityState == EntityState.Added ? 3 : 5, context.ChangeTracker.Entries().Count());
                Assert.Null(principal.Child2);
                Assert.Same(principal, dependent.Parent);
                Assert.Same(dependent, principal.Child1);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
                var dependentEntry2 = context.Entry(principal).Reference(p => p.Child1).TargetEntry;
                Assert.Equal(principal.Id, dependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, dependentEntry2.State);
                Assert.Equal(nameof(Parent.Child1), dependentEntry2.Metadata.DefiningNavigationName);

                Assert.Same(subDependent, dependent.SubChild);
                Assert.Same(dependent, subDependent.Parent);
                var subDependentEntry = dependentEntry2.Reference(p => p.SubChild).TargetEntry;
                Assert.Equal(principal.Id, subDependentEntry.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, subDependentEntry.State);
                Assert.Equal(nameof(Child.SubChild), subDependentEntry.Metadata.DefiningNavigationName);

                context.ChangeTracker.CascadeChanges();

                Assert.Equal(entityState == EntityState.Added ? 3 : 5, context.ChangeTracker.Entries().Count());

                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
                Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                Assert.Null(principal.Child2);
                Assert.Same(dependent, principal.Child1);
                Assert.Same(subDependent, dependent.SubChild);
                Assert.Same(dependent, subDependent.Parent);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Identity_changed_unidirectional_collection(EntityState entityState)
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
                principal.ChildCollection1 = new HashSet<ChildPN>(ReferenceEqualityComparer.Instance) { dependent };

                var subDependent = new SubChildPN
                {
                    Name = "1S"
                };
                dependent.SubChildCollection = new HashSet<SubChildPN>(ReferenceEqualityComparer.Instance) { subDependent };

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

                var dependentEntry1 = context.Entry(principal).Collection(p => p.ChildCollection1).GetTargetEntry(dependent);

                principal.ChildCollection2 = principal.ChildCollection1;
                principal.ChildCollection1 = null;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(entityState == EntityState.Added ? 3 : 5, context.ChangeTracker.Entries().Count());
                Assert.Null(principal.ChildCollection1);
                Assert.Contains(dependent, principal.ChildCollection2);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
                var dependentEntry2 = context.Entry(principal).Collection(p => p.ChildCollection2).GetTargetEntry(dependent);
                Assert.Equal(principal.Id, dependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, dependentEntry2.State);
                Assert.Equal(nameof(ParentPN.ChildCollection2), dependentEntry2.Metadata.DefiningNavigationName);

                Assert.Contains(subDependent, dependent.SubChildCollection);
                var subDependentEntry = dependentEntry2.Collection(p => p.SubChildCollection).GetTargetEntry(subDependent);
                Assert.Equal(principal.Id, subDependentEntry.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, subDependentEntry.State);
                Assert.Equal(nameof(ChildPN.SubChildCollection), subDependentEntry.Metadata.DefiningNavigationName);

                context.ChangeTracker.CascadeChanges();

                Assert.Equal(entityState == EntityState.Added ? 3 : 5, context.ChangeTracker.Entries().Count());

                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
                Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                Assert.Null(principal.ChildCollection1);
                Assert.Contains(dependent, principal.ChildCollection2);
                Assert.Contains(subDependent, dependent.SubChildCollection);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Identity_changed_bidirectional_collection(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent
                {
                    Id = 77
                };

                var dependent = new Child
                {
                    Name = "1"
                };
                principal.ChildCollection2 = new HashSet<Child>(ReferenceEqualityComparer.Instance) { dependent };

                var subDependent = new SubChild
                {
                    Name = "1S"
                };
                dependent.SubChildCollection = new HashSet<SubChild>(ReferenceEqualityComparer.Instance) { subDependent };

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

                var dependentEntry1 = context.Entry(principal).Collection(p => p.ChildCollection2).GetTargetEntry(dependent);

                principal.ChildCollection1 = principal.ChildCollection2;
                principal.ChildCollection2 = null;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(entityState == EntityState.Added ? 3 : 5, context.ChangeTracker.Entries().Count());
                Assert.Null(principal.ChildCollection2);
                Assert.Same(principal, dependent.Parent);
                Assert.Contains(dependent, principal.ChildCollection1);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
                var dependentEntry2 = context.Entry(principal).Collection(p => p.ChildCollection1).GetTargetEntry(dependent);
                Assert.Equal(principal.Id, dependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, dependentEntry2.State);
                Assert.Equal(nameof(Parent.ChildCollection1), dependentEntry2.Metadata.DefiningNavigationName);

                Assert.Contains(subDependent, dependent.SubChildCollection);
                Assert.Same(dependent, subDependent.Parent);
                var subDependentEntry = dependentEntry2.Collection(p => p.SubChildCollection).GetTargetEntry(subDependent);
                Assert.Equal(principal.Id, subDependentEntry.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, subDependentEntry.State);
                Assert.Equal(nameof(Child.SubChildCollection), subDependentEntry.Metadata.DefiningNavigationName);

                context.ChangeTracker.CascadeChanges();

                Assert.Equal(entityState == EntityState.Added ? 3 : 5, context.ChangeTracker.Entries().Count());

                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
                Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                Assert.Null(principal.Child2);
                Assert.Contains(dependent, principal.ChildCollection1);
                Assert.Contains(subDependent, dependent.SubChildCollection);
                Assert.Same(dependent, subDependent.Parent);
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

                var subDependent1 = new SubChildPN
                {
                    Name = "1S"
                };
                dependent1.SubChild = subDependent1;

                var dependent2 = new ChildPN
                {
                    Name = "2"
                };
                principal.Child2 = dependent2;

                var subDependent2 = new SubChildPN
                {
                    Name = "2S"
                };
                dependent2.SubChild = subDependent2;

                context.ChangeTracker.TrackGraph(principal, e => e.Entry.State = entityState);

                principal.Child2 = dependent1;
                principal.Child1 = dependent2;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(5, context.ChangeTracker.Entries().Count());
                Assert.Same(dependent1, principal.Child2);
                Assert.Same(dependent2, principal.Child1);
                Assert.Equal(entityState, context.Entry(principal).State);

                var dependent1Entry = context.Entry(principal).Reference(p => p.Child1).TargetEntry;
                Assert.Equal(principal.Id, dependent1Entry.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, dependent1Entry.State);
                Assert.Equal(nameof(ParentPN.Child1), dependent1Entry.Metadata.DefiningNavigationName);
                Assert.Equal(
                    entityState == EntityState.Added ? null : (EntityState?)EntityState.Deleted,
                    dependent1Entry.GetInfrastructure().SharedIdentityEntry?.EntityState);

                var dependent2Entry = context.Entry(principal).Reference(p => p.Child2).TargetEntry;
                Assert.Equal(principal.Id, dependent2Entry.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, dependent2Entry.State);
                Assert.Equal(nameof(ParentPN.Child2), dependent2Entry.Metadata.DefiningNavigationName);
                Assert.Equal(
                    entityState == EntityState.Added ? null : (EntityState?)EntityState.Deleted,
                    dependent2Entry.GetInfrastructure().SharedIdentityEntry?.EntityState);

                Assert.Same(subDependent1, dependent1.SubChild);
                var subDependentEntry1 = dependent1Entry.Reference(p => p.SubChild).TargetEntry;
                Assert.Equal(principal.Id, subDependentEntry1.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, subDependentEntry1.State);
                Assert.Equal(nameof(ChildPN.SubChild), subDependentEntry1.Metadata.DefiningNavigationName);

                Assert.Same(subDependent2, dependent2.SubChild);
                var subDependentEntry2 = dependent2Entry.Reference(p => p.SubChild).TargetEntry;
                Assert.Equal(principal.Id, subDependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, subDependentEntry2.State);
                Assert.Equal(nameof(ChildPN.SubChild), subDependentEntry2.Metadata.DefiningNavigationName);

                context.ChangeTracker.CascadeChanges();

                Assert.Equal(5, context.ChangeTracker.Entries().Count());

                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(5, context.ChangeTracker.Entries().Count());
                Assert.Null(dependent1Entry.GetInfrastructure().SharedIdentityEntry);
                Assert.Null(dependent2Entry.GetInfrastructure().SharedIdentityEntry);
                Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                Assert.Same(dependent1, principal.Child2);
                Assert.Same(dependent2, principal.Child1);
                Assert.Same(subDependent1, dependent1.SubChild);
                Assert.Same(subDependent2, dependent2.SubChild);
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
                    Name = "1"
                };
                principal.Child1 = dependent1;

                var subDependent1 = new SubChild
                {
                    Name = "1S"
                };
                dependent1.SubChild = subDependent1;

                var dependent2 = new Child
                {
                    Name = "2"
                };
                principal.Child2 = dependent2;

                var subDependent2 = new SubChild
                {
                    Name = "2S"
                };
                dependent2.SubChild = subDependent2;

                context.ChangeTracker.TrackGraph(principal, e => e.Entry.State = entityState);

                principal.Child2 = dependent1;
                principal.Child1 = dependent2;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(5, context.ChangeTracker.Entries().Count());
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

                Assert.Same(subDependent1, dependent1.SubChild);
                Assert.Same(dependent1, subDependent1.Parent);
                var subDependentEntry1 = dependent1Entry.Reference(p => p.SubChild).TargetEntry;
                Assert.Equal(principal.Id, subDependentEntry1.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, subDependentEntry1.State);
                Assert.Equal(nameof(Child.SubChild), subDependentEntry1.Metadata.DefiningNavigationName);

                Assert.Same(subDependent2, dependent2.SubChild);
                Assert.Same(dependent2, subDependent2.Parent);
                var subDependentEntry2 = dependent1Entry.Reference(p => p.SubChild).TargetEntry;
                Assert.Equal(principal.Id, subDependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, subDependentEntry2.State);
                Assert.Equal(nameof(Child.SubChild), subDependentEntry2.Metadata.DefiningNavigationName);

                context.ChangeTracker.CascadeChanges();

                Assert.Equal(5, context.ChangeTracker.Entries().Count());

                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(5, context.ChangeTracker.Entries().Count());
                Assert.Null(dependent1Entry.GetInfrastructure().SharedIdentityEntry);
                Assert.Null(dependent2Entry.GetInfrastructure().SharedIdentityEntry);
                Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                Assert.Same(dependent1, principal.Child2);
                Assert.Same(dependent2, principal.Child1);
                Assert.Same(subDependent1, dependent1.SubChild);
                Assert.Same(subDependent2, dependent2.SubChild);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Identity_swapped_unidirectional_collection(EntityState entityState)
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
                principal.ChildCollection1 = new HashSet<ChildPN>(ReferenceEqualityComparer.Instance) { dependent1 };

                var subDependent1 = new SubChildPN
                {
                    Name = "1S"
                };
                dependent1.SubChildCollection = new HashSet<SubChildPN>(ReferenceEqualityComparer.Instance) { subDependent1 };

                var dependent2 = new ChildPN
                {
                    Name = "2"
                };
                principal.ChildCollection2 = new HashSet<ChildPN>(ReferenceEqualityComparer.Instance) { dependent2 };

                var subDependent2 = new SubChildPN
                {
                    Name = "2S"
                };
                dependent2.SubChildCollection = new HashSet<SubChildPN>(ReferenceEqualityComparer.Instance) { subDependent2 };

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

                var dependentEntry1 = context.Entry(dependent1);
                var dependentEntry2 = context.Entry(dependent2);
                var subDependentEntry1 = context.Entry(subDependent1);
                var subDependentEntry2 = context.Entry(subDependent2);

                var tempCollection = principal.ChildCollection2;
                principal.ChildCollection2 = principal.ChildCollection1;
                principal.ChildCollection1 = tempCollection;

                var newDependentEntry1 = context.Entry(principal).Collection(p => p.ChildCollection2)
                    .GetTargetEntry(dependent1);
                newDependentEntry1.Property<int>("Id").CurrentValue = dependentEntry1.Property<int>("Id").CurrentValue;

                var newDependentEntry2 = context.Entry(principal).Collection(p => p.ChildCollection1)
                    .GetTargetEntry(dependent2);
                newDependentEntry2.Property<int>("Id").CurrentValue = dependentEntry2.Property<int>("Id").CurrentValue;

                var newSubDependentEntry1 = newDependentEntry1.Collection(p => p.SubChildCollection)
                    .GetTargetEntry(subDependent1);
                newSubDependentEntry1.Property<int>("Id").CurrentValue = subDependentEntry1.Property<int>("Id").CurrentValue;

                var newSubDependentEntry2 = newDependentEntry2.Collection(p => p.SubChildCollection)
                    .GetTargetEntry(subDependent2);
                newSubDependentEntry2.Property<int>("Id").CurrentValue = subDependentEntry2.Property<int>("Id").CurrentValue;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(5, context.ChangeTracker.Entries().Count());
                Assert.Contains(dependent1, principal.ChildCollection2);
                Assert.Contains(dependent2, principal.ChildCollection1);
                Assert.Equal(entityState, context.Entry(principal).State);

                Assert.Equal(principal.Id, newDependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, newDependentEntry2.State);
                Assert.Equal(nameof(ParentPN.ChildCollection1), newDependentEntry2.Metadata.DefiningNavigationName);
                Assert.Equal(
                    entityState == EntityState.Added ? null : (EntityState?)EntityState.Deleted,
                    newDependentEntry2.GetInfrastructure().SharedIdentityEntry?.EntityState);

                Assert.Equal(principal.Id, newDependentEntry1.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, newDependentEntry1.State);
                Assert.Equal(nameof(ParentPN.ChildCollection2), newDependentEntry1.Metadata.DefiningNavigationName);
                Assert.Equal(
                    entityState == EntityState.Added ? null : (EntityState?)EntityState.Deleted,
                    newDependentEntry1.GetInfrastructure().SharedIdentityEntry?.EntityState);

                Assert.Contains(subDependent1, dependent1.SubChildCollection);
                Assert.Equal(principal.Id, newSubDependentEntry1.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, newSubDependentEntry1.State);
                Assert.Equal(nameof(ChildPN.SubChildCollection), newSubDependentEntry1.Metadata.DefiningNavigationName);

                Assert.Contains(subDependent2, dependent2.SubChildCollection);
                Assert.Equal(principal.Id, newSubDependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, newSubDependentEntry2.State);
                Assert.Equal(nameof(ChildPN.SubChildCollection), newSubDependentEntry2.Metadata.DefiningNavigationName);

                context.ChangeTracker.CascadeChanges();

                Assert.Equal(5, context.ChangeTracker.Entries().Count());

                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(5, context.ChangeTracker.Entries().Count());
                Assert.Null(newDependentEntry2.GetInfrastructure().SharedIdentityEntry);
                Assert.Null(newDependentEntry1.GetInfrastructure().SharedIdentityEntry);
                Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                Assert.Contains(dependent1, principal.ChildCollection2);
                Assert.Contains(dependent2, principal.ChildCollection1);
                Assert.Contains(subDependent1, dependent1.SubChildCollection);
                Assert.Contains(subDependent2, dependent2.SubChildCollection);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Identity_swapped_bidirectional_collection(EntityState entityState)
        {
            using (var context = new FixupContext())
            {
                var principal = new Parent
                {
                    Id = 77
                };

                var dependent1 = new Child
                {
                    Name = "1"
                };
                principal.ChildCollection1 = new HashSet<Child>(ReferenceEqualityComparer.Instance) { dependent1 };

                var subDependent1 = new SubChild
                {
                    Name = "1S"
                };
                dependent1.SubChildCollection = new HashSet<SubChild>(ReferenceEqualityComparer.Instance) { subDependent1 };

                var dependent2 = new Child
                {
                    Name = "2"
                };
                principal.ChildCollection2 = new HashSet<Child>(ReferenceEqualityComparer.Instance) { dependent2 };

                var subDependent2 = new SubChild
                {
                    Name = "2S"
                };
                dependent2.SubChildCollection = new HashSet<SubChild>(ReferenceEqualityComparer.Instance) { subDependent2 };

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

                var dependentEntry1 = context.Entry(dependent1);
                var dependentEntry2 = context.Entry(dependent2);
                var subDependentEntry1 = context.Entry(subDependent1);
                var subDependentEntry2 = context.Entry(subDependent2);

                var tempCollection = principal.ChildCollection2;
                principal.ChildCollection2 = principal.ChildCollection1;
                principal.ChildCollection1 = tempCollection;

                var newDependentEntry1 = context.Entry(principal).Collection(p => p.ChildCollection2)
                    .GetTargetEntry(dependent1);
                newDependentEntry1.Property<int>("Id").CurrentValue = dependentEntry1.Property<int>("Id").CurrentValue;

                var newDependentEntry2 = context.Entry(principal).Collection(p => p.ChildCollection1)
                    .GetTargetEntry(dependent2);
                newDependentEntry2.Property<int>("Id").CurrentValue = dependentEntry2.Property<int>("Id").CurrentValue;

                var newSubDependentEntry1 = newDependentEntry1.Collection(p => p.SubChildCollection)
                    .GetTargetEntry(subDependent1);
                newSubDependentEntry1.Property<int>("Id").CurrentValue = subDependentEntry1.Property<int>("Id").CurrentValue;

                var newSubDependentEntry2 = newDependentEntry2.Collection(p => p.SubChildCollection)
                    .GetTargetEntry(subDependent2);
                newSubDependentEntry2.Property<int>("Id").CurrentValue = subDependentEntry2.Property<int>("Id").CurrentValue;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(5, context.ChangeTracker.Entries().Count());
                Assert.Same(principal, dependent1.Parent);
                Assert.Contains(dependent1, principal.ChildCollection2);
                Assert.Same(principal, dependent2.Parent);
                Assert.Contains(dependent2, principal.ChildCollection1);
                Assert.Equal(entityState, context.Entry(principal).State);

                Assert.Equal(principal.Id, newDependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, newDependentEntry2.State);
                Assert.Equal(nameof(Parent.ChildCollection1), newDependentEntry2.Metadata.DefiningNavigationName);
                Assert.Equal(
                    entityState == EntityState.Added ? null : (EntityState?)EntityState.Deleted,
                    newDependentEntry2.GetInfrastructure().SharedIdentityEntry?.EntityState);

                Assert.Equal(principal.Id, newDependentEntry1.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, newDependentEntry1.State);
                Assert.Equal(nameof(Parent.ChildCollection2), newDependentEntry1.Metadata.DefiningNavigationName);
                Assert.Equal(
                    entityState == EntityState.Added ? null : (EntityState?)EntityState.Deleted,
                    newDependentEntry1.GetInfrastructure().SharedIdentityEntry?.EntityState);

                Assert.Contains(subDependent1, dependent1.SubChildCollection);
                Assert.Same(dependent1, subDependent1.Parent);
                Assert.Equal(principal.Id, newSubDependentEntry1.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, newSubDependentEntry1.State);
                Assert.Equal(nameof(Child.SubChildCollection), newSubDependentEntry1.Metadata.DefiningNavigationName);

                Assert.Contains(subDependent2, dependent2.SubChildCollection);
                Assert.Same(dependent2, subDependent2.Parent);
                Assert.Equal(principal.Id, newSubDependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, newSubDependentEntry2.State);
                Assert.Equal(nameof(Child.SubChildCollection), newSubDependentEntry2.Metadata.DefiningNavigationName);

                context.ChangeTracker.CascadeChanges();

                Assert.Equal(5, context.ChangeTracker.Entries().Count());

                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(5, context.ChangeTracker.Entries().Count());
                Assert.Null(newDependentEntry2.GetInfrastructure().SharedIdentityEntry);
                Assert.Null(newDependentEntry1.GetInfrastructure().SharedIdentityEntry);
                Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                Assert.Contains(dependent1, principal.ChildCollection2);
                Assert.Contains(dependent2, principal.ChildCollection1);
                Assert.Contains(subDependent1, dependent1.SubChildCollection);
                Assert.Contains(subDependent2, dependent2.SubChildCollection);
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

                var subDependent = new SubChildPN
                {
                    Name = "1S"
                };
                dependent.SubChild = subDependent;

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

                    Assert.Equal(4, context.ChangeTracker.Entries().Count());
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
                    Assert.Equal(nameof(ParentPN.Child1), dependentEntry2.Metadata.DefiningNavigationName);

                    Assert.Same(subDependent, dependent.SubChild);
                    var subDependentEntry = dependentEntry2.Reference(p => p.SubChild).TargetEntry;
                    Assert.Equal(principal2.Id, subDependentEntry.Property("ParentId").CurrentValue);
                    Assert.Equal(EntityState.Added, subDependentEntry.State);
                    Assert.Equal(nameof(ChildPN.SubChild), subDependentEntry.Metadata.DefiningNavigationName);

                    context.ChangeTracker.CascadeChanges();

                    Assert.Equal(4, context.ChangeTracker.Entries().Count());

                    context.ChangeTracker.AcceptAllChanges();

                    Assert.Equal(4, context.ChangeTracker.Entries().Count());
                    Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                    Assert.Null(principal1.Child1);
                    Assert.Null(principal1.Child2);
                    Assert.Same(dependent, principal2.Child1);
                    Assert.Null(principal2.Child2);
                    Assert.Same(subDependent, dependent.SubChild);
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

                var subDependent = new SubChild
                {
                    Name = "1S"
                };
                dependent.SubChild = subDependent;

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

                    Assert.Equal(4, context.ChangeTracker.Entries().Count());
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

                    Assert.Same(subDependent, dependent.SubChild);
                    Assert.Same(dependent, subDependent.Parent);
                    var subDependentEntry = dependentEntry2.Reference(p => p.SubChild).TargetEntry;
                    Assert.Equal(principal2.Id, subDependentEntry.Property("ParentId").CurrentValue);
                    Assert.Equal(EntityState.Added, subDependentEntry.State);
                    Assert.Equal(nameof(Child.SubChild), subDependentEntry.Metadata.DefiningNavigationName);

                    context.ChangeTracker.CascadeChanges();

                    Assert.Equal(4, context.ChangeTracker.Entries().Count());

                    context.ChangeTracker.AcceptAllChanges();

                    Assert.Equal(4, context.ChangeTracker.Entries().Count());
                    Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                    Assert.Null(principal1.Child1);
                    Assert.Null(principal1.Child2);
                    Assert.Same(dependent, principal2.Child1);
                    Assert.Null(principal2.Child2);
                    Assert.Same(subDependent, dependent.SubChild);
                    Assert.Same(dependent, subDependent.Parent);
                }
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Parent_changed_unidirectional_collection(EntityState entityState)
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
                principal1.ChildCollection1 = new HashSet<ChildPN>(ReferenceEqualityComparer.Instance) { dependent };

                var subDependent = new SubChildPN
                {
                    Name = "1S"
                };
                dependent.SubChildCollection = new HashSet<SubChildPN>(ReferenceEqualityComparer.Instance) { subDependent };

                switch (entityState)
                {
                    case EntityState.Added:
                        context.Add(principal1);
                        context.Add(principal2);
                        break;
                    case EntityState.Unchanged:
                        context.Attach(principal1);
                        context.Attach(principal2);
                        break;
                    case EntityState.Modified:
                        context.Update(principal1);
                        context.Update(principal2);
                        break;
                }

                var dependentEntry1 = context.Entry(dependent);
                var subDependentEntry1 = context.Entry(subDependent);

                principal2.ChildCollection1 = principal1.ChildCollection1;
                principal1.ChildCollection1 = null;

                if (entityState != EntityState.Added)
                {
                    Assert.Equal(
                        CoreStrings.KeyReadOnly("ParentId", dependentEntry1.Metadata.DisplayName()),
                        Assert.Throws<InvalidOperationException>(() => context.ChangeTracker.DetectChanges()).Message);
                }
                else
                {
                    context.ChangeTracker.DetectChanges();

                    Assert.Equal(4, context.ChangeTracker.Entries().Count());
                    Assert.Null(principal1.ChildCollection1);
                    Assert.Null(principal1.ChildCollection2);
                    Assert.Contains(dependent, principal2.ChildCollection1);
                    Assert.Null(principal2.ChildCollection2);
                    Assert.Equal(entityState, context.Entry(principal1).State);
                    Assert.Equal(entityState, context.Entry(principal2).State);
                    Assert.Equal(EntityState.Detached, dependentEntry1.State);

                    var dependentEntry2 = context.Entry(principal2).Collection(p => p.ChildCollection1)
                        .GetTargetEntry(dependent);
                    Assert.Equal(principal2.Id, dependentEntry2.Property("ParentId").CurrentValue);
                    Assert.Equal(EntityState.Added, dependentEntry2.State);
                    Assert.Equal(nameof(ParentPN.ChildCollection1), dependentEntry2.Metadata.DefiningNavigationName);

                    Assert.Contains(subDependent, dependent.SubChildCollection);
                    var subDependentEntry2 = dependentEntry2.Collection(p => p.SubChildCollection)
                        .GetTargetEntry(subDependent);
                    Assert.Equal(principal2.Id, subDependentEntry2.Property("ParentId").CurrentValue);
                    Assert.Equal(EntityState.Added, subDependentEntry2.State);
                    Assert.Equal(nameof(ChildPN.SubChildCollection), subDependentEntry2.Metadata.DefiningNavigationName);

                    context.ChangeTracker.CascadeChanges();

                    Assert.Equal(4, context.ChangeTracker.Entries().Count());

                    context.ChangeTracker.AcceptAllChanges();

                    Assert.Equal(4, context.ChangeTracker.Entries().Count());
                    Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                    Assert.Null(principal1.ChildCollection1);
                    Assert.Null(principal1.ChildCollection2);
                    Assert.Contains(dependent, principal2.ChildCollection1);
                    Assert.Null(principal2.ChildCollection2);
                    Assert.Contains(subDependent, dependent.SubChildCollection);
                }
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Parent_changed_bidirectional_collection(EntityState entityState)
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
                principal1.ChildCollection1 = new HashSet<Child>(ReferenceEqualityComparer.Instance) { dependent };

                var subDependent = new SubChild
                {
                    Name = "1S"
                };
                dependent.SubChildCollection = new HashSet<SubChild>(ReferenceEqualityComparer.Instance) { subDependent };

                switch (entityState)
                {
                    case EntityState.Added:
                        context.Add(principal1);
                        context.Add(principal2);
                        break;
                    case EntityState.Unchanged:
                        context.Attach(principal1);
                        context.Attach(principal2);
                        break;
                    case EntityState.Modified:
                        context.Update(principal1);
                        context.Update(principal2);
                        break;
                }

                var dependentEntry1 = context.Entry(dependent);
                var subDependentEntry1 = context.Entry(subDependent);

                principal2.ChildCollection1 = principal1.ChildCollection1;
                principal1.ChildCollection1 = null;

                if (entityState != EntityState.Added)
                {
                    Assert.Equal(
                        CoreStrings.KeyReadOnly("ParentId", dependentEntry1.Metadata.DisplayName()),
                        Assert.Throws<InvalidOperationException>(() => context.ChangeTracker.DetectChanges()).Message);
                }
                else
                {
                    context.ChangeTracker.DetectChanges();

                    Assert.Equal(4, context.ChangeTracker.Entries().Count());
                    Assert.Empty(principal1.ChildCollection1);
                    Assert.Null(principal1.ChildCollection2);
                    Assert.Contains(dependent, principal2.ChildCollection1);
                    Assert.Null(principal2.ChildCollection2);
                    Assert.Same(principal2, dependent.Parent);
                    Assert.Equal(entityState, context.Entry(principal1).State);
                    Assert.Equal(entityState, context.Entry(principal2).State);
                    Assert.Equal(EntityState.Detached, dependentEntry1.State);

                    var dependentEntry2 = context.Entry(principal2).Collection(p => p.ChildCollection1)
                        .GetTargetEntry(dependent);
                    Assert.Equal(EntityState.Added, dependentEntry2.State);
                    Assert.Equal(principal2.Id, dependentEntry2.Property("ParentId").CurrentValue);
                    Assert.Equal(nameof(Parent.ChildCollection1), dependentEntry2.Metadata.DefiningNavigationName);

                    Assert.Contains(subDependent, dependent.SubChildCollection);
                    Assert.Same(dependent, subDependent.Parent);
                    var subDependentEntry2 = dependentEntry2.Collection(p => p.SubChildCollection)
                        .GetTargetEntry(subDependent);
                    Assert.Equal(principal2.Id, subDependentEntry2.Property("ParentId").CurrentValue);
                    Assert.Equal(EntityState.Added, subDependentEntry2.State);
                    Assert.Equal(nameof(Child.SubChildCollection), subDependentEntry2.Metadata.DefiningNavigationName);

                    context.ChangeTracker.CascadeChanges();

                    Assert.Equal(4, context.ChangeTracker.Entries().Count());

                    context.ChangeTracker.AcceptAllChanges();

                    Assert.Equal(4, context.ChangeTracker.Entries().Count());
                    Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                    Assert.Empty(principal1.ChildCollection1);
                    Assert.Null(principal1.ChildCollection2);
                    Assert.Contains(dependent, principal2.ChildCollection1);
                    Assert.Null(principal2.ChildCollection2);
                    Assert.Contains(subDependent, dependent.SubChildCollection);
                    Assert.Same(dependent, subDependent.Parent);
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

                var subDependent1 = new SubChildPN
                {
                    Name = "1S"
                };
                dependent1.SubChild = subDependent1;

                var dependent2 = new ChildPN
                {
                    Name = "2"
                };
                principal2.Child1 = dependent2;

                var subDependent2 = new SubChildPN
                {
                    Name = "2S"
                };
                dependent2.SubChild = subDependent2;

                context.ChangeTracker.TrackGraph(principal1, e => e.Entry.State = entityState);
                context.ChangeTracker.TrackGraph(principal2, e => e.Entry.State = entityState);

                principal1.Child1 = dependent2;
                principal2.Child1 = dependent1;

                if (entityState != EntityState.Added)
                {
                    Assert.Equal(
                        CoreStrings.KeyReadOnly(
                            "ParentId",
                            "ParentPN.Child1#ChildPN"),
                        Assert.Throws<InvalidOperationException>(() => context.ChangeTracker.DetectChanges()).Message);
                }
                else
                {
                    context.ChangeTracker.DetectChanges();

                    Assert.Equal(6, context.ChangeTracker.Entries().Count());
                    Assert.Same(dependent2, principal1.Child1);
                    Assert.Null(principal1.Child2);
                    Assert.Same(dependent1, principal2.Child1);
                    Assert.Null(principal2.Child2);
                    Assert.Equal(entityState, context.Entry(principal1).State);
                    Assert.Equal(entityState, context.Entry(principal2).State);

                    var dependent1Entry = context.Entry(principal1).Reference(p => p.Child1).TargetEntry;
                    Assert.Equal(principal1.Id, dependent1Entry.Property("ParentId").CurrentValue);
                    Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, dependent1Entry.State);
                    Assert.Equal(nameof(ParentPN.Child1), dependent1Entry.Metadata.DefiningNavigationName);

                    var dependent2Entry = context.Entry(principal2).Reference(p => p.Child1).TargetEntry;
                    Assert.Equal(principal2.Id, dependent2Entry.Property("ParentId").CurrentValue);
                    Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, dependent2Entry.State);
                    Assert.Equal(nameof(ParentPN.Child1), dependent2Entry.Metadata.DefiningNavigationName);

                    Assert.Same(subDependent1, dependent1.SubChild);
                    var subDependentEntry1 = dependent1Entry.Reference(p => p.SubChild).TargetEntry;
                    Assert.Equal(principal1.Id, subDependentEntry1.Property("ParentId").CurrentValue);
                    Assert.Equal(EntityState.Added, subDependentEntry1.State);
                    Assert.Equal(nameof(ChildPN.SubChild), subDependentEntry1.Metadata.DefiningNavigationName);

                    Assert.Same(subDependent2, dependent2.SubChild);
                    var subDependentEntry2 = dependent2Entry.Reference(p => p.SubChild).TargetEntry;
                    Assert.Equal(principal2.Id, subDependentEntry2.Property("ParentId").CurrentValue);
                    Assert.Equal(EntityState.Added, subDependentEntry2.State);
                    Assert.Equal(nameof(ChildPN.SubChild), subDependentEntry2.Metadata.DefiningNavigationName);

                    context.ChangeTracker.CascadeChanges();

                    Assert.Equal(6, context.ChangeTracker.Entries().Count());

                    context.ChangeTracker.AcceptAllChanges();

                    Assert.Equal(6, context.ChangeTracker.Entries().Count());
                    Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                    Assert.Same(dependent2, principal1.Child1);
                    Assert.Null(principal1.Child2);
                    Assert.Same(dependent1, principal2.Child1);
                    Assert.Null(principal2.Child2);
                    Assert.Same(subDependent1, dependent1.SubChild);
                    Assert.Same(subDependent2, dependent2.SubChild);
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

                var subDependent1 = new SubChild
                {
                    Name = "1S"
                };
                dependent1.SubChild = subDependent1;

                var dependent2 = new Child
                {
                    Name = "2"
                };
                principal2.Child1 = dependent2;

                var subDependent2 = new SubChild
                {
                    Name = "2S"
                };
                dependent2.SubChild = subDependent2;

                context.ChangeTracker.TrackGraph(principal1, e => e.Entry.State = entityState);
                context.ChangeTracker.TrackGraph(principal2, e => e.Entry.State = entityState);

                principal1.Child1 = dependent2;
                principal2.Child1 = dependent1;

                if (entityState != EntityState.Added)
                {
                    Assert.Equal(
                        CoreStrings.KeyReadOnly(
                            "ParentId",
                            "Parent.Child1#Child"),
                        Assert.Throws<InvalidOperationException>(() => context.ChangeTracker.DetectChanges()).Message);
                }
                else
                {
                    context.ChangeTracker.DetectChanges();

                    Assert.Equal(6, context.ChangeTracker.Entries().Count());
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

                    Assert.Same(subDependent1, dependent1.SubChild);
                    Assert.Same(dependent1, subDependent1.Parent);
                    var subDependentEntry1 = dependent1Entry.Reference(p => p.SubChild).TargetEntry;
                    Assert.Equal(principal1.Id, subDependentEntry1.Property("ParentId").CurrentValue);
                    Assert.Equal(EntityState.Added, subDependentEntry1.State);
                    Assert.Equal(nameof(Child.SubChild), subDependentEntry1.Metadata.DefiningNavigationName);

                    Assert.Same(subDependent2, dependent2.SubChild);
                    Assert.Same(dependent2, subDependent2.Parent);
                    var subDependentEntry2 = dependent2Entry.Reference(p => p.SubChild).TargetEntry;
                    Assert.Equal(principal2.Id, subDependentEntry2.Property("ParentId").CurrentValue);
                    Assert.Equal(EntityState.Added, subDependentEntry2.State);
                    Assert.Equal(nameof(Child.SubChild), subDependentEntry2.Metadata.DefiningNavigationName);

                    context.ChangeTracker.CascadeChanges();

                    Assert.Equal(6, context.ChangeTracker.Entries().Count());

                    context.ChangeTracker.AcceptAllChanges();

                    Assert.Equal(6, context.ChangeTracker.Entries().Count());
                    Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                    Assert.Same(dependent2, principal1.Child1);
                    Assert.Null(principal1.Child2);
                    Assert.Same(dependent1, principal2.Child1);
                    Assert.Null(principal2.Child2);
                    Assert.Same(subDependent1, dependent1.SubChild);
                    Assert.Same(subDependent2, dependent2.SubChild);
                    Assert.Same(dependent1, subDependent1.Parent);
                    Assert.Same(dependent2, subDependent2.Parent);
                }
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Parent_swapped_unidirectional_collection(EntityState entityState)
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
                principal1.ChildCollection1 = new HashSet<ChildPN>(ReferenceEqualityComparer.Instance) { dependent1 };

                var subDependent1 = new SubChildPN
                {
                    Name = "1S"
                };
                dependent1.SubChildCollection = new HashSet<SubChildPN>(ReferenceEqualityComparer.Instance) { subDependent1 };

                var dependent2 = new ChildPN
                {
                    Name = "2"
                };
                principal2.ChildCollection1 = new HashSet<ChildPN>(ReferenceEqualityComparer.Instance) { dependent2 };

                var subDependent2 = new SubChildPN
                {
                    Name = "2S"
                };
                dependent2.SubChildCollection = new HashSet<SubChildPN>(ReferenceEqualityComparer.Instance) { subDependent2 };

                switch (entityState)
                {
                    case EntityState.Added:
                        context.Add(principal1);
                        context.Add(principal2);
                        break;
                    case EntityState.Unchanged:
                        context.Attach(principal1);
                        context.Attach(principal2);
                        break;
                    case EntityState.Modified:
                        context.Update(principal1);
                        context.Update(principal2);
                        break;
                }

                var dependentEntry1 = context.Entry(dependent1);
                var subDependentEntry1 = context.Entry(subDependent1);

                var tempCollection = principal1.ChildCollection1;
                principal1.ChildCollection1 = principal2.ChildCollection1;
                principal2.ChildCollection1 = tempCollection;

                if (entityState != EntityState.Added)
                {
                    Assert.Equal(
                        CoreStrings.KeyReadOnly(
                            "ParentId",
                            "ParentPN.ChildCollection1#ChildPN"),
                        Assert.Throws<InvalidOperationException>(() => context.ChangeTracker.DetectChanges()).Message);
                }
                else
                {
                    context.ChangeTracker.DetectChanges();

                    Assert.Equal(6, context.ChangeTracker.Entries().Count());
                    Assert.Contains(dependent2, principal1.ChildCollection1);
                    Assert.Null(principal1.Child1);
                    Assert.Contains(dependent1, principal2.ChildCollection1);
                    Assert.Null(principal2.Child1);
                    Assert.Equal(entityState, context.Entry(principal1).State);
                    Assert.Equal(entityState, context.Entry(principal2).State);

                    var newDependentEntry2 = context.Entry(principal1).Collection(p => p.ChildCollection1)
                        .GetTargetEntry(dependent2);
                    Assert.Equal(principal1.Id, newDependentEntry2.Property("ParentId").CurrentValue);
                    Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, newDependentEntry2.State);
                    Assert.Equal(nameof(ParentPN.ChildCollection1), newDependentEntry2.Metadata.DefiningNavigationName);

                    var newDependentEntry1 = context.Entry(principal2).Collection(p => p.ChildCollection1)
                        .GetTargetEntry(dependent1);
                    Assert.Equal(principal2.Id, newDependentEntry1.Property("ParentId").CurrentValue);
                    Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, newDependentEntry1.State);
                    Assert.Equal(nameof(ParentPN.ChildCollection1), newDependentEntry1.Metadata.DefiningNavigationName);

                    Assert.Contains(subDependent1, dependent1.SubChildCollection);
                    var newSubDependentEntry1 = newDependentEntry1.Collection(p => p.SubChildCollection)
                        .GetTargetEntry(subDependent1);
                    Assert.Equal(principal2.Id, newSubDependentEntry1.Property("ParentId").CurrentValue);
                    Assert.Equal(EntityState.Added, newSubDependentEntry1.State);
                    Assert.Equal(nameof(ChildPN.SubChildCollection), newSubDependentEntry1.Metadata.DefiningNavigationName);

                    Assert.Contains(subDependent2, dependent2.SubChildCollection);
                    var newSubDependentEntry2 = newDependentEntry2.Collection(p => p.SubChildCollection)
                        .GetTargetEntry(subDependent2);
                    Assert.Equal(principal1.Id, newSubDependentEntry2.Property("ParentId").CurrentValue);
                    Assert.Equal(EntityState.Added, newSubDependentEntry2.State);
                    Assert.Equal(nameof(ChildPN.SubChildCollection), newSubDependentEntry2.Metadata.DefiningNavigationName);

                    context.ChangeTracker.CascadeChanges();

                    Assert.Equal(6, context.ChangeTracker.Entries().Count());

                    context.ChangeTracker.AcceptAllChanges();

                    Assert.Equal(6, context.ChangeTracker.Entries().Count());
                    Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                    Assert.Contains(dependent2, principal1.ChildCollection1);
                    Assert.Null(principal1.Child1);
                    Assert.Contains(dependent1, principal2.ChildCollection1);
                    Assert.Null(principal2.Child1);
                    Assert.Contains(subDependent1, dependent1.SubChildCollection);
                    Assert.Contains(subDependent2, dependent2.SubChildCollection);
                }
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Parent_swapped_bidirectional_collection(EntityState entityState)
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
                principal1.ChildCollection1 = new HashSet<Child>(ReferenceEqualityComparer.Instance) { dependent1 };

                var subDependent1 = new SubChild
                {
                    Name = "1S"
                };
                dependent1.SubChildCollection = new HashSet<SubChild>(ReferenceEqualityComparer.Instance) { subDependent1 };

                var dependent2 = new Child
                {
                    Name = "2"
                };
                principal2.ChildCollection1 = new HashSet<Child>(ReferenceEqualityComparer.Instance) { dependent2 };

                var subDependent2 = new SubChild
                {
                    Name = "2S"
                };
                dependent2.SubChildCollection = new HashSet<SubChild>(ReferenceEqualityComparer.Instance) { subDependent2 };

                switch (entityState)
                {
                    case EntityState.Added:
                        context.Add(principal1);
                        context.Add(principal2);
                        break;
                    case EntityState.Unchanged:
                        context.Attach(principal1);
                        context.Attach(principal2);
                        break;
                    case EntityState.Modified:
                        context.Update(principal1);
                        context.Update(principal2);
                        break;
                }

                var dependentEntry1 = context.Entry(dependent1);
                var subDependentEntry1 = context.Entry(subDependent1);

                var tempCollection = principal1.ChildCollection1;
                principal1.ChildCollection1 = principal2.ChildCollection1;
                principal2.ChildCollection1 = tempCollection;

                if (entityState != EntityState.Added)
                {
                    Assert.Equal(
                        CoreStrings.KeyReadOnly(
                            "ParentId",
                            "Parent.ChildCollection1#Child"),
                        Assert.Throws<InvalidOperationException>(() => context.ChangeTracker.DetectChanges()).Message);
                }
                else
                {
                    context.ChangeTracker.DetectChanges();

                    Assert.Equal(6, context.ChangeTracker.Entries().Count());
                    Assert.Contains(dependent2, principal1.ChildCollection1);
                    Assert.Null(principal1.Child1);
                    Assert.Contains(dependent1, principal2.ChildCollection1);
                    Assert.Null(principal2.Child1);
                    Assert.Same(principal2, dependent1.Parent);
                    Assert.Same(principal1, dependent2.Parent);
                    Assert.Equal(entityState, context.Entry(principal1).State);
                    Assert.Equal(entityState, context.Entry(principal2).State);

                    var newDependentEntry2 = context.Entry(principal1).Collection(p => p.ChildCollection1)
                        .GetTargetEntry(dependent2);
                    Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, newDependentEntry2.State);
                    Assert.Equal(principal1.Id, newDependentEntry2.Property("ParentId").CurrentValue);
                    Assert.Equal(nameof(Parent.ChildCollection1), newDependentEntry2.Metadata.DefiningNavigationName);

                    var newDependentEntry1 = context.Entry(principal2).Collection(p => p.ChildCollection1)
                        .GetTargetEntry(dependent1);
                    Assert.Equal(principal2.Id, newDependentEntry1.Property("ParentId").CurrentValue);
                    Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, newDependentEntry1.State);
                    Assert.Equal(nameof(Parent.ChildCollection1), newDependentEntry1.Metadata.DefiningNavigationName);

                    Assert.Contains(subDependent1, dependent1.SubChildCollection);
                    Assert.Same(dependent1, subDependent1.Parent);
                    var newSubDependentEntry1 = newDependentEntry1.Collection(p => p.SubChildCollection)
                        .GetTargetEntry(subDependent1);
                    Assert.Equal(principal2.Id, newSubDependentEntry1.Property("ParentId").CurrentValue);
                    Assert.Equal(EntityState.Added, newSubDependentEntry1.State);
                    Assert.Equal(nameof(Child.SubChildCollection), newSubDependentEntry1.Metadata.DefiningNavigationName);

                    Assert.Contains(subDependent2, dependent2.SubChildCollection);
                    Assert.Same(dependent2, subDependent2.Parent);
                    var newSubDependentEntry2 = newDependentEntry2.Collection(p => p.SubChildCollection)
                        .GetTargetEntry(subDependent2);
                    Assert.Equal(principal1.Id, newSubDependentEntry2.Property("ParentId").CurrentValue);
                    Assert.Equal(EntityState.Added, newSubDependentEntry2.State);
                    Assert.Equal(nameof(Child.SubChildCollection), newSubDependentEntry2.Metadata.DefiningNavigationName);

                    context.ChangeTracker.CascadeChanges();

                    Assert.Equal(6, context.ChangeTracker.Entries().Count());

                    context.ChangeTracker.AcceptAllChanges();

                    Assert.Equal(6, context.ChangeTracker.Entries().Count());
                    Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                    Assert.Contains(dependent2, principal1.ChildCollection1);
                    Assert.Null(principal1.Child1);
                    Assert.Contains(dependent1, principal2.ChildCollection1);
                    Assert.Null(principal2.Child1);
                    Assert.Contains(subDependent1, dependent1.SubChildCollection);
                    Assert.Contains(subDependent2, dependent2.SubChildCollection);
                    Assert.Same(dependent1, subDependent1.Parent);
                    Assert.Same(dependent2, subDependent2.Parent);
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

                var subDependent = new SubChildPN
                {
                    Name = "1S"
                };
                dependent.SubChild = subDependent;

                context.ChangeTracker.TrackGraph(principal1, e => e.Entry.State = entityState);
                context.ChangeTracker.TrackGraph(principal2, e => e.Entry.State = entityState);

                var dependentEntry1 = context.Entry(principal1).Reference(p => p.Child2).TargetEntry;

                principal2.Child1 = dependent;
                principal1.Child2 = null;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(entityState == EntityState.Added ? 4 : 6, context.ChangeTracker.Entries().Count());
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
                Assert.Equal(nameof(ParentPN.Child1), dependentEntry2.Metadata.DefiningNavigationName);

                Assert.Same(subDependent, dependent.SubChild);
                var subDependentEntry = dependentEntry2.Reference(p => p.SubChild).TargetEntry;
                Assert.Equal(principal2.Id, subDependentEntry.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, subDependentEntry.State);
                Assert.Equal(nameof(ChildPN.SubChild), subDependentEntry.Metadata.DefiningNavigationName);

                context.ChangeTracker.CascadeChanges();

                Assert.Equal(entityState == EntityState.Added ? 4 : 6, context.ChangeTracker.Entries().Count());

                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(4, context.ChangeTracker.Entries().Count());
                Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                Assert.Null(principal1.Child1);
                Assert.Null(principal1.Child2);
                Assert.Same(dependent, principal2.Child1);
                Assert.Null(principal2.Child2);
                Assert.Same(subDependent, dependent.SubChild);
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
                    Name = "1"
                };
                principal1.Child2 = dependent;

                var subDependent = new SubChild
                {
                    Name = "1S"
                };
                dependent.SubChild = subDependent;

                context.ChangeTracker.TrackGraph(principal1, e => e.Entry.State = entityState);
                context.ChangeTracker.TrackGraph(principal2, e => e.Entry.State = entityState);

                var dependentEntry1 = context.Entry(principal1).Reference(p => p.Child2).TargetEntry;

                principal2.Child1 = dependent;
                principal1.Child2 = null;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(entityState == EntityState.Added ? 4 : 6, context.ChangeTracker.Entries().Count());
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

                Assert.Same(subDependent, dependent.SubChild);
                Assert.Same(dependent, subDependent.Parent);
                var subDependentEntry = dependentEntry2.Reference(p => p.SubChild).TargetEntry;
                Assert.Equal(principal2.Id, subDependentEntry.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, subDependentEntry.State);
                Assert.Equal(nameof(Child.SubChild), subDependentEntry.Metadata.DefiningNavigationName);

                context.ChangeTracker.CascadeChanges();

                Assert.Equal(entityState == EntityState.Added ? 4 : 6, context.ChangeTracker.Entries().Count());

                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(4, context.ChangeTracker.Entries().Count());
                Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                Assert.Null(principal1.Child1);
                Assert.Null(principal1.Child2);
                Assert.Same(dependent, principal2.Child1);
                Assert.Null(principal2.Child2);
                Assert.Same(subDependent, dependent.SubChild);
                Assert.Same(dependent, subDependent.Parent);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Parent_and_identity_changed_unidirectional_collection(EntityState entityState)
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
                principal1.ChildCollection2 = new HashSet<ChildPN>(ReferenceEqualityComparer.Instance) { dependent };

                var subDependent = new SubChildPN
                {
                    Name = "1S"
                };
                dependent.SubChildCollection = new HashSet<SubChildPN>(ReferenceEqualityComparer.Instance) { subDependent };

                switch (entityState)
                {
                    case EntityState.Added:
                        context.Add(principal1);
                        context.Add(principal2);
                        break;
                    case EntityState.Unchanged:
                        context.Attach(principal1);
                        context.Attach(principal2);
                        break;
                    case EntityState.Modified:
                        context.Update(principal1);
                        context.Update(principal2);
                        break;
                }

                var dependentEntry1 = context.Entry(principal1).Collection(p => p.ChildCollection2).GetTargetEntry(dependent);

                principal2.ChildCollection1 = principal1.ChildCollection2;
                principal1.ChildCollection2 = null;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(entityState == EntityState.Added ? 4 : 6, context.ChangeTracker.Entries().Count());
                Assert.Null(principal1.ChildCollection1);
                Assert.Null(principal1.ChildCollection2);
                Assert.Contains(dependent, principal2.ChildCollection1);
                Assert.Null(principal2.ChildCollection2);
                Assert.Equal(entityState, context.Entry(principal1).State);
                Assert.Equal(entityState, context.Entry(principal2).State);
                Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
                var dependentEntry2 = context.Entry(principal2).Collection(p => p.ChildCollection1).GetTargetEntry(dependent);
                Assert.Equal(principal2.Id, dependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, dependentEntry2.State);
                Assert.Equal(nameof(ParentPN.ChildCollection1), dependentEntry2.Metadata.DefiningNavigationName);

                Assert.Contains(subDependent, dependent.SubChildCollection);
                var subDependentEntry = dependentEntry2.Collection(p => p.SubChildCollection).GetTargetEntry(subDependent);
                Assert.Equal(principal2.Id, subDependentEntry.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, subDependentEntry.State);
                Assert.Equal(nameof(ChildPN.SubChildCollection), subDependentEntry.Metadata.DefiningNavigationName);

                context.ChangeTracker.CascadeChanges();

                Assert.Equal(entityState == EntityState.Added ? 4 : 6, context.ChangeTracker.Entries().Count());

                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(4, context.ChangeTracker.Entries().Count());
                Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                Assert.Null(principal1.ChildCollection1);
                Assert.Null(principal1.ChildCollection2);
                Assert.Contains(dependent, principal2.ChildCollection1);
                Assert.Null(principal2.ChildCollection2);
                Assert.Contains(subDependent, dependent.SubChildCollection);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Parent_and_identity_changed_bidirectional_collection(EntityState entityState)
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
                principal1.ChildCollection2 = new HashSet<Child>(ReferenceEqualityComparer.Instance) { dependent };

                var subDependent = new SubChild
                {
                    Name = "1S"
                };
                dependent.SubChildCollection = new HashSet<SubChild>(ReferenceEqualityComparer.Instance) { subDependent };

                switch (entityState)
                {
                    case EntityState.Added:
                        context.Add(principal1);
                        context.Add(principal2);
                        break;
                    case EntityState.Unchanged:
                        context.Attach(principal1);
                        context.Attach(principal2);
                        break;
                    case EntityState.Modified:
                        context.Update(principal1);
                        context.Update(principal2);
                        break;
                }

                var dependentEntry1 = context.Entry(principal1).Collection(p => p.ChildCollection2).GetTargetEntry(dependent);

                principal2.ChildCollection1 = principal1.ChildCollection2;
                principal1.ChildCollection2 = null;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(entityState == EntityState.Added ? 4 : 6, context.ChangeTracker.Entries().Count());
                Assert.Empty(principal1.ChildCollection1);
                Assert.Null(principal1.ChildCollection2);
                Assert.Contains(dependent, principal2.ChildCollection1);
                Assert.Null(principal2.ChildCollection2);
                Assert.Same(principal2, dependent.Parent);
                Assert.Equal(entityState, context.Entry(principal1).State);
                Assert.Equal(entityState, context.Entry(principal2).State);
                Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
                var dependentEntry2 = context.Entry(principal2).Collection(p => p.ChildCollection1).GetTargetEntry(dependent);
                Assert.Equal(principal2.Id, dependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, dependentEntry2.State);
                Assert.Equal(nameof(Parent.ChildCollection1), dependentEntry2.Metadata.DefiningNavigationName);

                Assert.Contains(subDependent, dependent.SubChildCollection);
                Assert.Same(dependent, subDependent.Parent);
                var subDependentEntry = dependentEntry2.Collection(p => p.SubChildCollection).GetTargetEntry(subDependent);
                Assert.Equal(principal2.Id, subDependentEntry.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, subDependentEntry.State);
                Assert.Equal(nameof(Child.SubChildCollection), subDependentEntry.Metadata.DefiningNavigationName);

                context.ChangeTracker.CascadeChanges();

                Assert.Equal(entityState == EntityState.Added ? 4 : 6, context.ChangeTracker.Entries().Count());

                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(4, context.ChangeTracker.Entries().Count());
                Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                Assert.Empty(principal1.ChildCollection1);
                Assert.Null(principal1.ChildCollection2);
                Assert.Contains(dependent, principal2.ChildCollection1);
                Assert.Null(principal2.ChildCollection2);
                Assert.Contains(subDependent, dependent.SubChildCollection);
                Assert.Same(dependent, subDependent.Parent);
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

                var subDependent1 = new SubChildPN
                {
                    Name = "1S"
                };
                dependent1.SubChild = subDependent1;

                var dependent2 = new ChildPN
                {
                    Name = "2"
                };
                principal2.Child1 = dependent2;

                var subDependent2 = new SubChildPN
                {
                    Name = "2S"
                };
                dependent2.SubChild = subDependent2;

                context.ChangeTracker.TrackGraph(principal1, e => e.Entry.State = entityState);
                context.ChangeTracker.TrackGraph(principal2, e => e.Entry.State = entityState);

                principal2.Child1 = dependent1;
                principal1.Child2 = dependent2;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(6, context.ChangeTracker.Entries().Count());
                Assert.Null(principal1.Child1);
                Assert.Same(dependent2, principal1.Child2);
                Assert.Same(dependent1, principal2.Child1);
                Assert.Null(principal2.Child2);
                Assert.Equal(entityState, context.Entry(principal1).State);
                Assert.Equal(entityState, context.Entry(principal2).State);

                var dependent1Entry = context.Entry(principal1).Reference(p => p.Child2).TargetEntry;
                Assert.Equal(EntityState.Added, dependent1Entry.State);
                Assert.Equal(principal1.Id, dependent1Entry.Property("ParentId").CurrentValue);
                Assert.Equal(nameof(ParentPN.Child2), dependent1Entry.Metadata.DefiningNavigationName);
                Assert.Equal(
                    entityState == EntityState.Added ? null : (EntityState?)EntityState.Deleted,
                    dependent1Entry.GetInfrastructure().SharedIdentityEntry?.EntityState);

                var dependent2Entry = context.Entry(principal2).Reference(p => p.Child1).TargetEntry;
                Assert.Equal(principal2.Id, dependent2Entry.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, dependent2Entry.State);
                Assert.Equal(nameof(ParentPN.Child1), dependent2Entry.Metadata.DefiningNavigationName);
                Assert.Equal(
                    entityState == EntityState.Added ? null : (EntityState?)EntityState.Deleted,
                    dependent2Entry.GetInfrastructure().SharedIdentityEntry?.EntityState);

                Assert.Same(subDependent1, dependent1.SubChild);
                var subDependentEntry1 = dependent1Entry.Reference(p => p.SubChild).TargetEntry;
                Assert.Equal(principal1.Id, subDependentEntry1.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, subDependentEntry1.State);
                Assert.Equal(nameof(ChildPN.SubChild), subDependentEntry1.Metadata.DefiningNavigationName);

                Assert.Same(subDependent2, dependent2.SubChild);
                var subDependentEntry2 = dependent2Entry.Reference(p => p.SubChild).TargetEntry;
                Assert.Equal(principal2.Id, subDependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, subDependentEntry2.State);
                Assert.Equal(nameof(ChildPN.SubChild), subDependentEntry2.Metadata.DefiningNavigationName);

                context.ChangeTracker.CascadeChanges();

                Assert.Equal(6, context.ChangeTracker.Entries().Count());

                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(6, context.ChangeTracker.Entries().Count());
                Assert.Null(dependent1Entry.GetInfrastructure().SharedIdentityEntry);
                Assert.Null(dependent2Entry.GetInfrastructure().SharedIdentityEntry);
                Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                Assert.Null(principal1.Child1);
                Assert.Same(dependent2, principal1.Child2);
                Assert.Same(dependent1, principal2.Child1);
                Assert.Null(principal2.Child2);
                Assert.Same(subDependent1, dependent1.SubChild);
                Assert.Same(subDependent2, dependent2.SubChild);
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
                    Name = "1"
                };
                principal1.Child2 = dependent1;

                var subDependent1 = new SubChild
                {
                    Name = "1S"
                };
                dependent1.SubChild = subDependent1;

                var dependent2 = new Child
                {
                    Name = "2"
                };
                principal2.Child1 = dependent2;

                var subDependent2 = new SubChild
                {
                    Name = "2S"
                };
                dependent2.SubChild = subDependent2;

                context.ChangeTracker.TrackGraph(principal1, e => e.Entry.State = entityState);
                context.ChangeTracker.TrackGraph(principal2, e => e.Entry.State = entityState);

                principal2.Child1 = dependent1;
                principal1.Child2 = dependent2;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(6, context.ChangeTracker.Entries().Count());
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

                Assert.Same(subDependent1, dependent1.SubChild);
                Assert.Same(dependent1, subDependent1.Parent);
                var subDependentEntry1 = dependent1Entry.Reference(p => p.SubChild).TargetEntry;
                Assert.Equal(principal1.Id, subDependentEntry1.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, subDependentEntry1.State);
                Assert.Equal(nameof(Child.SubChild), subDependentEntry1.Metadata.DefiningNavigationName);

                Assert.Same(subDependent2, dependent2.SubChild);
                Assert.Same(dependent2, subDependent2.Parent);
                var subDependentEntry2 = dependent2Entry.Reference(p => p.SubChild).TargetEntry;
                Assert.Equal(principal2.Id, subDependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, subDependentEntry2.State);
                Assert.Equal(nameof(Child.SubChild), subDependentEntry2.Metadata.DefiningNavigationName);

                context.ChangeTracker.CascadeChanges();

                Assert.Equal(6, context.ChangeTracker.Entries().Count());

                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(6, context.ChangeTracker.Entries().Count());
                Assert.Null(dependent1Entry.GetInfrastructure().SharedIdentityEntry);
                Assert.Null(dependent2Entry.GetInfrastructure().SharedIdentityEntry);
                Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                Assert.Null(principal1.Child1);
                Assert.Same(dependent2, principal1.Child2);
                Assert.Same(dependent1, principal2.Child1);
                Assert.Null(principal2.Child2);
                Assert.Same(subDependent1, dependent1.SubChild);
                Assert.Same(subDependent2, dependent2.SubChild);
                Assert.Same(dependent1, subDependent1.Parent);
                Assert.Same(dependent2, subDependent2.Parent);
            }
        }
        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Parent_and_identity_swapped_unidirectional_collection(EntityState entityState)
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
                principal1.ChildCollection2 = new HashSet<ChildPN>(ReferenceEqualityComparer.Instance) { dependent1 };

                var subDependent1 = new SubChildPN
                {
                    Name = "1S"
                };
                dependent1.SubChildCollection = new HashSet<SubChildPN>(ReferenceEqualityComparer.Instance) { subDependent1 };

                var dependent2 = new ChildPN
                {
                    Name = "2"
                };
                principal2.ChildCollection1 = new HashSet<ChildPN>(ReferenceEqualityComparer.Instance) { dependent2 };

                var subDependent2 = new SubChildPN
                {
                    Name = "2S"
                };
                dependent2.SubChildCollection = new HashSet<SubChildPN>(ReferenceEqualityComparer.Instance) { subDependent2 };

                switch (entityState)
                {
                    case EntityState.Added:
                        context.Add(principal1);
                        context.Add(principal2);
                        break;
                    case EntityState.Unchanged:
                        context.Attach(principal1);
                        context.Attach(principal2);
                        break;
                    case EntityState.Modified:
                        context.Update(principal1);
                        context.Update(principal2);
                        break;
                }

                var dependentEntry1 = context.Entry(dependent1);
                var dependentEntry2 = context.Entry(dependent2);
                var subDependentEntry1 = context.Entry(subDependent1);
                var subDependentEntry2 = context.Entry(subDependent2);

                var tempCollection = principal2.ChildCollection1;
                principal2.ChildCollection1 = principal1.ChildCollection2;
                principal1.ChildCollection2 = tempCollection;

                var newDependentEntry1 = context.Entry(principal2).Collection(p => p.ChildCollection1)
                    .GetTargetEntry(dependent1);
                newDependentEntry1.Property<int>("Id").CurrentValue = dependentEntry1.Property<int>("Id").CurrentValue;

                var newDependentEntry2 = context.Entry(principal1).Collection(p => p.ChildCollection2)
                    .GetTargetEntry(dependent2);
                newDependentEntry2.Property<int>("Id").CurrentValue = dependentEntry2.Property<int>("Id").CurrentValue;

                var newSubDependentEntry1 = newDependentEntry1.Collection(p => p.SubChildCollection)
                    .GetTargetEntry(subDependent1);
                newSubDependentEntry1.Property<int>("Id").CurrentValue = subDependentEntry1.Property<int>("Id").CurrentValue;

                var newSubDependentEntry2 = newDependentEntry2.Collection(p => p.SubChildCollection)
                    .GetTargetEntry(subDependent2);
                newSubDependentEntry2.Property<int>("Id").CurrentValue = subDependentEntry2.Property<int>("Id").CurrentValue;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(6, context.ChangeTracker.Entries().Count());
                Assert.Null(principal1.ChildCollection1);
                Assert.Contains(dependent2, principal1.ChildCollection2);
                Assert.Contains(dependent1, principal2.ChildCollection1);
                Assert.Null(principal2.ChildCollection2);
                Assert.Equal(entityState, context.Entry(principal1).State);
                Assert.Equal(entityState, context.Entry(principal2).State);

                Assert.Equal(EntityState.Added, newDependentEntry2.State);
                Assert.Equal(principal1.Id, newDependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(nameof(ParentPN.ChildCollection2), newDependentEntry2.Metadata.DefiningNavigationName);
                Assert.Equal(
                    entityState == EntityState.Added ? null : (EntityState?)EntityState.Deleted,
                    newDependentEntry2.GetInfrastructure().SharedIdentityEntry?.EntityState);

                Assert.Equal(principal2.Id, newDependentEntry1.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, newDependentEntry1.State);
                Assert.Equal(nameof(ParentPN.ChildCollection1), newDependentEntry1.Metadata.DefiningNavigationName);
                Assert.Equal(
                    entityState == EntityState.Added ? null : (EntityState?)EntityState.Deleted,
                    newDependentEntry1.GetInfrastructure().SharedIdentityEntry?.EntityState);

                Assert.Contains(subDependent1, dependent1.SubChildCollection);
                Assert.Equal(principal1.Id, newSubDependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, newSubDependentEntry2.State);
                Assert.Equal(nameof(ChildPN.SubChildCollection), newSubDependentEntry2.Metadata.DefiningNavigationName);

                Assert.Contains(subDependent2, dependent2.SubChildCollection);
                Assert.Equal(principal2.Id, newSubDependentEntry1.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, newSubDependentEntry1.State);
                Assert.Equal(nameof(ChildPN.SubChildCollection), newSubDependentEntry1.Metadata.DefiningNavigationName);

                context.ChangeTracker.CascadeChanges();

                Assert.Equal(6, context.ChangeTracker.Entries().Count());

                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(6, context.ChangeTracker.Entries().Count());
                Assert.Null(newDependentEntry2.GetInfrastructure().SharedIdentityEntry);
                Assert.Null(newDependentEntry1.GetInfrastructure().SharedIdentityEntry);
                Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                Assert.Null(principal1.ChildCollection1);
                Assert.Contains(dependent2, principal1.ChildCollection2);
                Assert.Contains(dependent1, principal2.ChildCollection1);
                Assert.Null(principal2.ChildCollection2);
                Assert.Contains(subDependent1, dependent1.SubChildCollection);
                Assert.Contains(subDependent2, dependent2.SubChildCollection);
            }
        }

        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Unchanged)]
        public void Parent_and_identity_swapped_bidirectional_collection(EntityState entityState)
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
                principal1.ChildCollection2 = new HashSet<Child>(ReferenceEqualityComparer.Instance) { dependent1 };

                var subDependent1 = new SubChild
                {
                    Name = "1S"
                };
                dependent1.SubChildCollection = new HashSet<SubChild>(ReferenceEqualityComparer.Instance) { subDependent1 };

                var dependent2 = new Child
                {
                    Name = "2"
                };
                principal2.ChildCollection1 = new HashSet<Child>(ReferenceEqualityComparer.Instance) { dependent2 };

                var subDependent2 = new SubChild
                {
                    Name = "2S"
                };
                dependent2.SubChildCollection = new HashSet<SubChild>(ReferenceEqualityComparer.Instance) { subDependent2 };

                switch (entityState)
                {
                    case EntityState.Added:
                        context.Add(principal1);
                        context.Add(principal2);
                        break;
                    case EntityState.Unchanged:
                        context.Attach(principal1);
                        context.Attach(principal2);
                        break;
                    case EntityState.Modified:
                        context.Update(principal1);
                        context.Update(principal2);
                        break;
                }

                var dependentEntry1 = context.Entry(dependent1);
                var dependentEntry2 = context.Entry(dependent2);
                var subDependentEntry1 = context.Entry(subDependent1);
                var subDependentEntry2 = context.Entry(subDependent2);

                var tempCollection = principal2.ChildCollection1;
                principal2.ChildCollection1 = principal1.ChildCollection2;
                principal1.ChildCollection2 = tempCollection;

                var newDependentEntry1 = context.Entry(principal2).Collection(p => p.ChildCollection1)
                    .GetTargetEntry(dependent1);
                newDependentEntry1.Property<int>("Id").CurrentValue = dependentEntry1.Property<int>("Id").CurrentValue;

                var newDependentEntry2 = context.Entry(principal1).Collection(p => p.ChildCollection2)
                    .GetTargetEntry(dependent2);
                newDependentEntry2.Property<int>("Id").CurrentValue = dependentEntry2.Property<int>("Id").CurrentValue;

                var newSubDependentEntry1 = newDependentEntry1.Collection(p => p.SubChildCollection)
                    .GetTargetEntry(subDependent1);
                newSubDependentEntry1.Property<int>("Id").CurrentValue = subDependentEntry1.Property<int>("Id").CurrentValue;

                var newSubDependentEntry2 = newDependentEntry2.Collection(p => p.SubChildCollection)
                    .GetTargetEntry(subDependent2);
                newSubDependentEntry2.Property<int>("Id").CurrentValue = subDependentEntry2.Property<int>("Id").CurrentValue;

                context.ChangeTracker.DetectChanges();

                Assert.Equal(6, context.ChangeTracker.Entries().Count());
                Assert.Empty(principal1.ChildCollection1);
                Assert.Contains(dependent2, principal1.ChildCollection2);
                Assert.Contains(dependent1, principal2.ChildCollection1);
                Assert.Empty(principal2.ChildCollection2);
                Assert.Same(principal2, dependent1.Parent);
                Assert.Same(principal1, dependent2.Parent);
                Assert.Equal(entityState, context.Entry(principal1).State);
                Assert.Equal(entityState, context.Entry(principal2).State);

                Assert.Equal(EntityState.Added, newDependentEntry2.State);
                Assert.Equal(principal1.Id, newDependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(nameof(Parent.ChildCollection2), newDependentEntry2.Metadata.DefiningNavigationName);
                Assert.Equal(
                    entityState == EntityState.Added ? null : (EntityState?)EntityState.Deleted,
                    newDependentEntry2.GetInfrastructure().SharedIdentityEntry?.EntityState);

                Assert.Equal(principal2.Id, newDependentEntry1.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, newDependentEntry1.State);
                Assert.Equal(nameof(Parent.ChildCollection1), newDependentEntry1.Metadata.DefiningNavigationName);
                Assert.Equal(
                    entityState == EntityState.Added ? null : (EntityState?)EntityState.Deleted,
                    newDependentEntry2.GetInfrastructure().SharedIdentityEntry?.EntityState);

                Assert.Contains(subDependent1, dependent1.SubChildCollection);
                Assert.Same(dependent1, subDependent1.Parent);
                Assert.Equal(principal1.Id, newSubDependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, newSubDependentEntry2.State);
                Assert.Equal(nameof(Child.SubChildCollection), newSubDependentEntry2.Metadata.DefiningNavigationName);

                Assert.Contains(subDependent2, dependent2.SubChildCollection);
                Assert.Same(dependent2, subDependent2.Parent);
                Assert.Equal(principal2.Id, newSubDependentEntry1.Property("ParentId").CurrentValue);
                Assert.Equal(EntityState.Added, newSubDependentEntry1.State);
                Assert.Equal(nameof(Child.SubChildCollection), newSubDependentEntry1.Metadata.DefiningNavigationName);

                context.ChangeTracker.CascadeChanges();

                Assert.Equal(6, context.ChangeTracker.Entries().Count());

                context.ChangeTracker.AcceptAllChanges();

                Assert.Equal(6, context.ChangeTracker.Entries().Count());
                Assert.Null(newDependentEntry2.GetInfrastructure().SharedIdentityEntry);
                Assert.Null(newDependentEntry1.GetInfrastructure().SharedIdentityEntry);
                Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
                Assert.Empty(principal1.ChildCollection1);
                Assert.Contains(dependent2, principal1.ChildCollection2);
                Assert.Contains(dependent1, principal2.ChildCollection1);
                Assert.Empty(principal2.ChildCollection2);
                Assert.Contains(subDependent1, dependent1.SubChildCollection);
                Assert.Contains(subDependent2, dependent2.SubChildCollection);
                Assert.Same(dependent1, subDependent1.Parent);
                Assert.Same(dependent2, subDependent2.Parent);
            }
        }

        private class Parent
        {
            public int Id { get; set; }

            public Child Child1 { get; set; }
            public Child Child2 { get; set; }
            public ICollection<Child> ChildCollection1 { get; set; }
            public ICollection<Child> ChildCollection2 { get; set; }

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
            public ICollection<SubChild> SubChildCollection { get; set; }

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

            public Child Parent { get; set; }

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
            public ICollection<ChildPN> ChildCollection1 { get; set; }
            public ICollection<ChildPN> ChildCollection2 { get; set; }
        }

        private class ChildPN
        {
            public string Name { get; set; }

            // ReSharper disable once MemberHidesStaticFromOuterClass
            public SubChildPN SubChild { get; set; }
            public ICollection<SubChildPN> SubChildCollection { get; set; }
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

                                cb.OwnsOne(c => c.SubChild, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.WithOwner(c => c.Parent)
                                        .HasForeignKey("ParentId");
                                });

                                cb.OwnsMany(c => c.SubChildCollection, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.WithOwner(c => c.Parent)
                                        .HasForeignKey("ParentId");
                                });
                            });

                        pb.OwnsOne(
                            p => p.Child2, cb =>
                            {
                                cb.Property<int?>("ParentId");
                                cb.WithOwner(c => c.Parent)
                                    .HasForeignKey("ParentId");

                                cb.OwnsOne(c => c.SubChild, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.WithOwner(c => c.Parent)
                                        .HasForeignKey("ParentId");
                                });

                                cb.OwnsMany(c => c.SubChildCollection, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.WithOwner(c => c.Parent)
                                        .HasForeignKey("ParentId");
                                });
                            });

                        pb.OwnsMany(
                            p => p.ChildCollection1, cb =>
                            {
                                cb.Property<int?>("ParentId");
                                cb.WithOwner(c => c.Parent)
                                    .HasForeignKey("ParentId");

                                cb.OwnsOne(c => c.SubChild, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.Property<int>("ChildId");
                                    sb.WithOwner(c => c.Parent)
                                        .HasForeignKey("ParentId", "ChildId");
                                });

                                cb.OwnsMany(c => c.SubChildCollection, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.Property<int>("ChildId");
                                    sb.WithOwner(c => c.Parent)
                                        .HasForeignKey("ParentId", "ChildId");
                                });
                            });

                        pb.OwnsMany(
                            p => p.ChildCollection2, cb =>
                            {
                                cb.Property<int?>("ParentId");
                                cb.WithOwner(c => c.Parent)
                                    .HasForeignKey("ParentId");

                                cb.OwnsOne(c => c.SubChild, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.Property<int>("ChildId");
                                    sb.WithOwner(c => c.Parent)
                                        .HasForeignKey("ParentId", "ChildId");
                                });

                                cb.OwnsMany(c => c.SubChildCollection, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.Property<int>("ChildId");
                                    sb.WithOwner(c => c.Parent)
                                        .HasForeignKey("ParentId", "ChildId");
                                });
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

                                cb.OwnsOne(c => c.SubChild, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.WithOwner()
                                        .HasForeignKey("ParentId");
                                });

                                cb.OwnsMany(c => c.SubChildCollection, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.WithOwner()
                                        .HasForeignKey("ParentId");
                                });
                            });

                        pb.OwnsOne(
                            p => p.Child2, cb =>
                            {
                                cb.Property<int?>("ParentId");
                                cb.WithOwner()
                                  .HasForeignKey("ParentId");

                                cb.OwnsOne(c => c.SubChild, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.WithOwner()
                                        .HasForeignKey("ParentId");
                                });

                                cb.OwnsMany(c => c.SubChildCollection, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.WithOwner()
                                        .HasForeignKey("ParentId");
                                });
                            });

                        pb.OwnsMany(
                            p => p.ChildCollection1, cb =>
                            {
                                cb.Property<int?>("ParentId");
                                cb.WithOwner()
                                    .HasForeignKey("ParentId");

                                cb.OwnsOne(c => c.SubChild, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.Property<int>("ChildId");
                                    sb.WithOwner()
                                        .HasForeignKey("ParentId", "ChildId");
                                });

                                cb.OwnsMany(c => c.SubChildCollection, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.Property<int>("ChildId");
                                    sb.WithOwner()
                                        .HasForeignKey("ParentId", "ChildId");
                                });
                            });

                        pb.OwnsMany(
                            p => p.ChildCollection2, cb =>
                            {
                                cb.Property<int?>("ParentId");
                                cb.WithOwner()
                                    .HasForeignKey("ParentId");

                                cb.OwnsOne(c => c.SubChild, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.Property<int>("ChildId");
                                    sb.WithOwner()
                                        .HasForeignKey("ParentId", "ChildId");
                                });

                                cb.OwnsMany(c => c.SubChildCollection, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.Property<int>("ChildId");
                                    sb.WithOwner()
                                        .HasForeignKey("ParentId", "ChildId");
                                });
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
