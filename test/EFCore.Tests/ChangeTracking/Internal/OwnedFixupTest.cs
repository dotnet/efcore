// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;

// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable AccessToDisposedClosure
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public class OwnedFixupTest
{
    private class Thing
    {
        public Guid ThingId { get; set; }
        public List<OwnedByThing> OwnedByThings { get; set; } = [];
    }

    private class OwnedByThing
    {
        public Guid OwnedByThingId { get; set; }
        public Guid ThingId { get; set; }
        public Thing Thing { get; set; }
    }

    [ConditionalTheory] // Issue #18982
    [InlineData(false)]
    [InlineData(true)]
    public void Detaching_owner_does_not_delete_owned_entities(bool delayCascade)
    {
        using var context = new FixupContext();

        var thing = new Thing
        {
            ThingId = Guid.NewGuid(),
            OwnedByThings = [new() { OwnedByThingId = Guid.NewGuid() }, new() { OwnedByThingId = Guid.NewGuid() }]
        };

        context.Attach(thing);

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.Equal(EntityState.Unchanged, context.Entry(thing).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(thing.OwnedByThings[0]).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(thing.OwnedByThings[1]).State);

        if (delayCascade)
        {
            context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;
        }

        context.Entry(thing).State = EntityState.Detached;

        Assert.False(context.ChangeTracker.HasChanges());

        if (delayCascade)
        {
            Assert.Equal(2, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Detached, context.Entry(thing).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(thing.OwnedByThings[0]).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(thing.OwnedByThings[1]).State);
        }
        else
        {
            Assert.Empty(context.ChangeTracker.Entries());
            Assert.Equal(EntityState.Detached, context.Entry(thing).State);
            Assert.Equal(EntityState.Detached, context.Entry(thing.OwnedByThings[0]).State);
            Assert.Equal(EntityState.Detached, context.Entry(thing.OwnedByThings[1]).State);
        }
    }

    [ConditionalFact]
    public void Can_detach_Added_owner_referencing_detached_weak_owned_entity()
    {
        using var context = new FixupContext();
        var owner = new Parent { Child1 = new Child() };

        context.Entry(owner).State = EntityState.Added;

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(EntityState.Added, context.Entry(owner).State);
        Assert.Equal(EntityState.Detached, context.Entry(owner).Reference(e => e.Child1).TargetEntry.State);

        context.Entry(owner).State = EntityState.Detached;

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(EntityState.Detached, context.Entry(owner).State);
        Assert.Equal(EntityState.Detached, context.Entry(owner).Reference(e => e.Child1).TargetEntry.State);
    }

    [ConditionalFact]
    public void Can_get_owned_entity_entry()
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id = 77 };

        var dependent = new ChildPN { Name = "1" };
        principal.Child1 = dependent;
        principal.Child2 = dependent;

        Assert.Equal(
            CoreStrings.UntrackedDependentEntity(
                nameof(ChildPN),
                ".Reference().TargetEntry",
                ".Collection().FindEntry()"),
            Assert.Throws<InvalidOperationException>(() => context.Entry(dependent)).Message);

        var dependentEntry1 = context.Entry(principal).Reference(p => p.Child1).TargetEntry;

        Assert.Same(dependentEntry1.GetInfrastructure(), context.Entry(dependent).GetInfrastructure());

        var dependentEntry2 = context.Entry(principal).Reference(p => p.Child2).TargetEntry;

        Assert.NotNull(dependentEntry2);
        Assert.Equal(
            CoreStrings.AmbiguousDependentEntity(
                nameof(ChildPN),
                "." + nameof(EntityEntry.Reference) + "()." + nameof(ReferenceEntry.TargetEntry)),
            Assert.Throws<InvalidOperationException>(() => context.Entry(dependent)).Message);
    }

    [ConditionalFact]
    public void Adding_duplicate_owned_entity_throws_by_default()
    {
        using var context = new FixupContext(false);
        var principal = new ParentPN { Id = 77 };

        var dependent = new ChildPN { Name = "1" };
        principal.Child1 = dependent;
        principal.Child2 = dependent;

        var dependentEntry1 = context.Entry(principal).Reference(p => p.Child1).TargetEntry;

        Assert.Same(dependentEntry1.GetInfrastructure(), context.Entry(dependent).GetInfrastructure());

        Assert.Equal(
            CoreStrings.WarningAsErrorTemplate(
                CoreEventId.DuplicateDependentEntityTypeInstanceWarning.ToString(),
                CoreResources.LogDuplicateDependentEntityTypeInstance(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                    typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.Child2) + "#" + nameof(ChildPN),
                    typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.Child1) + "#" + nameof(ChildPN)),
                "CoreEventId.DuplicateDependentEntityTypeInstanceWarning"),
            Assert.Throws<InvalidOperationException>(() => context.Entry(principal).Reference(p => p.Child2).TargetEntry).Message);
    }

    [ConditionalTheory]
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
        using var context = new FixupContext();
        var principal = new ParentPN { Id = 77 };
        if (useTrackGraph == null)
        {
            context.Entry(principal).State = entityState;
        }

        var dependent = new ChildPN { Name = "1" };
        principal.Child1 = dependent;

        var subDependent = new SubChildPN { Name = "1S" };
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

        Assert.Equal(
            entityState != EntityState.Unchanged
            || useTrackGraph == null,
            context.ChangeTracker.HasChanges());

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
                Assert.Equal(
                    typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.Child1) + "#" + nameof(ChildPN),
                    dependentEntry.Metadata.DisplayName());

                Assert.Same(subDependent, dependent.SubChild);
                var subDependentEntry = context.Entry(subDependent);
                Assert.Equal(principal.Id, subDependentEntry.Property("ParentId").CurrentValue);
                Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, subDependentEntry.State);
                Assert.Equal(
                    typeof(ParentPN).ShortDisplayName()
                    + "."
                    + nameof(ParentPN.Child1)
                    + "#"
                    + nameof(ChildPN)
                    + "."
                    + nameof(ChildPN.SubChild)
                    + "#"
                    + nameof(SubChildPN), subDependentEntry.Metadata.DisplayName());
            });
    }

    [ConditionalTheory]
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
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };
        if (useTrackGraph == null)
        {
            context.Entry(principal).State = entityState;
        }

        var dependent = new Child { Name = "1", Parent = principal };
        principal.Child1 = dependent;

        var subDependent1 = new SubChild { Name = "1S1", Parent = dependent };
        dependent.SubChild1 = subDependent1;

        var subDependent2 = new SubChild { Name = "1S2", Parent = dependent };
        dependent.SubChild2 = subDependent2;

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

        Assert.Equal(
            entityState != EntityState.Unchanged
            || useTrackGraph == null,
            context.ChangeTracker.HasChanges());

        Assert.Equal(4, context.ChangeTracker.Entries().Count());

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(dependent, principal.Child1);
                Assert.Same(principal, dependent.Parent);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, context.Entry(dependent).State);

                Assert.Same(subDependent1, dependent.SubChild1);
                Assert.Same(dependent, subDependent1.Parent);
                var subDependentEntry1 = context.Entry(subDependent1);
                Assert.Equal(principal.Id, subDependentEntry1.Property("ParentId").CurrentValue);
                Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, subDependentEntry1.State);
                Assert.Equal(
                    typeof(Parent).ShortDisplayName()
                    + "."
                    + nameof(Parent.Child1)
                    + "#"
                    + nameof(Child)
                    + "."
                    + nameof(Child.SubChild1)
                    + "#"
                    + nameof(SubChild),
                    subDependentEntry1.Metadata.DisplayName());

                Assert.Same(subDependent2, dependent.SubChild2);
                Assert.Same(dependent, subDependent2.Parent);
                var subDependentEntry2 = context.Entry(subDependent1);
                Assert.Equal(principal.Id, subDependentEntry2.Property("ParentId").CurrentValue);
                Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, subDependentEntry2.State);
                Assert.Equal(
                    typeof(Parent).ShortDisplayName()
                    + "."
                    + nameof(Parent.Child1)
                    + "#"
                    + nameof(Child)
                    + "."
                    + nameof(Child.SubChild1)
                    + "#"
                    + nameof(SubChild),
                    subDependentEntry2.Metadata.DisplayName());
            });
    }

    [ConditionalTheory]
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
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };
        if (useTrackGraph == null)
        {
            context.Entry(principal).State = entityState;
        }

        var dependent = new Child { Name = "1" };
        principal.Child1 = dependent;

        var subDependent1 = new SubChild { Name = "1S1" };
        dependent.SubChild1 = subDependent1;
        var subDependent2 = new SubChild { Name = "1S2" };
        dependent.SubChild2 = subDependent2;

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

        Assert.Equal(
            entityState != EntityState.Unchanged
            || useTrackGraph == null,
            context.ChangeTracker.HasChanges());

        Assert.Equal(4, context.ChangeTracker.Entries().Count());

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Same(dependent, principal.Child1);
                Assert.Same(principal, dependent.Parent);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, context.Entry(dependent).State);

                Assert.Same(subDependent1, dependent.SubChild1);
                Assert.Same(dependent, subDependent1.Parent);
                var subDependentEntry1 = context.Entry(subDependent1);
                Assert.Equal(principal.Id, subDependentEntry1.Property("ParentId").CurrentValue);
                Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, subDependentEntry1.State);
                Assert.Equal(
                    typeof(Parent).ShortDisplayName()
                    + "."
                    + nameof(Parent.Child1)
                    + "#"
                    + nameof(Child)
                    + "."
                    + nameof(Child.SubChild1)
                    + "#"
                    + nameof(SubChild),
                    subDependentEntry1.Metadata.DisplayName());

                Assert.Same(subDependent2, dependent.SubChild2);
                Assert.Same(dependent, subDependent2.Parent);
                var subDependentEntry2 = context.Entry(subDependent2);
                Assert.Equal(principal.Id, subDependentEntry1.Property("ParentId").CurrentValue);
                Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, subDependentEntry2.State);
                Assert.Equal(
                    typeof(Parent).ShortDisplayName()
                    + "."
                    + nameof(Parent.Child1)
                    + "#"
                    + nameof(Child)
                    + "."
                    + nameof(Child.SubChild2)
                    + "#"
                    + nameof(SubChild),
                    subDependentEntry2.Metadata.DisplayName());
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added, true, CollectionType.HashSet)]
    [InlineData(EntityState.Added, false, CollectionType.HashSet)]
    [InlineData(EntityState.Added, null, CollectionType.HashSet)]
    [InlineData(EntityState.Modified, true, CollectionType.HashSet)]
    [InlineData(EntityState.Modified, false, CollectionType.HashSet)]
    [InlineData(EntityState.Modified, null, CollectionType.HashSet)]
    [InlineData(EntityState.Unchanged, true, CollectionType.HashSet)]
    [InlineData(EntityState.Unchanged, false, CollectionType.HashSet)]
    [InlineData(EntityState.Unchanged, null, CollectionType.HashSet)]
    [InlineData(EntityState.Added, true, CollectionType.List)]
    [InlineData(EntityState.Added, false, CollectionType.List)]
    [InlineData(EntityState.Added, null, CollectionType.List)]
    [InlineData(EntityState.Modified, true, CollectionType.List)]
    [InlineData(EntityState.Modified, false, CollectionType.List)]
    [InlineData(EntityState.Modified, null, CollectionType.List)]
    [InlineData(EntityState.Unchanged, true, CollectionType.List)]
    [InlineData(EntityState.Unchanged, false, CollectionType.List)]
    [InlineData(EntityState.Unchanged, null, CollectionType.List)]
    [InlineData(EntityState.Added, true, CollectionType.SortedSet)]
    [InlineData(EntityState.Added, false, CollectionType.SortedSet)]
    [InlineData(EntityState.Added, null, CollectionType.SortedSet)]
    [InlineData(EntityState.Modified, true, CollectionType.SortedSet)]
    [InlineData(EntityState.Modified, false, CollectionType.SortedSet)]
    [InlineData(EntityState.Modified, null, CollectionType.SortedSet)]
    [InlineData(EntityState.Unchanged, true, CollectionType.SortedSet)]
    [InlineData(EntityState.Unchanged, false, CollectionType.SortedSet)]
    [InlineData(EntityState.Unchanged, null, CollectionType.SortedSet)]
    [InlineData(EntityState.Added, true, CollectionType.Collection)]
    [InlineData(EntityState.Added, false, CollectionType.Collection)]
    [InlineData(EntityState.Added, null, CollectionType.Collection)]
    [InlineData(EntityState.Modified, true, CollectionType.Collection)]
    [InlineData(EntityState.Modified, false, CollectionType.Collection)]
    [InlineData(EntityState.Modified, null, CollectionType.Collection)]
    [InlineData(EntityState.Unchanged, true, CollectionType.Collection)]
    [InlineData(EntityState.Unchanged, false, CollectionType.Collection)]
    [InlineData(EntityState.Unchanged, null, CollectionType.Collection)]
    [InlineData(EntityState.Added, true, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Added, false, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Added, null, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Modified, true, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Modified, false, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Modified, null, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Unchanged, true, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Unchanged, false, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Unchanged, null, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Added, true, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Added, false, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Added, null, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Modified, true, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Modified, false, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Modified, null, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Unchanged, true, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Unchanged, false, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Unchanged, null, CollectionType.ObservableHashSet)]
    public void Add_principal_with_dependent_unidirectional_nav_collection(
        EntityState entityState,
        bool? useTrackGraph,
        CollectionType collectionType)
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id = 77 };
        if (useTrackGraph == null)
        {
            context.Entry(principal).State = entityState;
        }

        var dependent = new ChildPN { Name = "1" };
        principal.ChildCollection1 = CreateChildCollection(collectionType, dependent);

        var subDependent = new SubChildPN { Name = "1S" };
        dependent.SubChildCollection = CreateChildCollection(collectionType, subDependent);

        if (useTrackGraph == null)
        {
            context.ChangeTracker.DetectChanges();
        }
        else if (useTrackGraph == true)
        {
            context.ChangeTracker.TrackGraph(
                principal, e =>
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

        Assert.Equal(
            entityState != EntityState.Unchanged
            || useTrackGraph == null,
            context.ChangeTracker.HasChanges());

        Assert.Equal(3, context.ChangeTracker.Entries().Count());

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(entityState, context.Entry(principal).State);

                Assert.Contains(principal.ChildCollection1, e => ReferenceEquals(e, dependent));
                Assert.Null(principal.ChildCollection2);
                var dependentEntry = context.Entry(dependent);
                Assert.Equal(principal.Id, dependentEntry.Property("ParentId").CurrentValue);
                Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, dependentEntry.State);
                Assert.Equal(
                    typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.ChildCollection1) + "#" + nameof(ChildPN),
                    dependentEntry.Metadata.DisplayName());

                Assert.Contains(dependent.SubChildCollection, e => ReferenceEquals(e, subDependent));
                var subDependentEntry = context.Entry(subDependent);
                Assert.Equal(principal.Id, subDependentEntry.Property("ParentId").CurrentValue);
                Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, subDependentEntry.State);
                Assert.Equal(
                    typeof(ParentPN).ShortDisplayName()
                    + "."
                    + nameof(ParentPN.ChildCollection1)
                    + "#"
                    + nameof(ChildPN)
                    + "."
                    + nameof(ChildPN.SubChildCollection)
                    + "#"
                    + nameof(SubChildPN),
                    subDependentEntry.Metadata.DisplayName());
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added, true, CollectionType.HashSet)]
    [InlineData(EntityState.Added, false, CollectionType.HashSet)]
    [InlineData(EntityState.Added, null, CollectionType.HashSet)]
    [InlineData(EntityState.Modified, true, CollectionType.HashSet)]
    [InlineData(EntityState.Modified, false, CollectionType.HashSet)]
    [InlineData(EntityState.Modified, null, CollectionType.HashSet)]
    [InlineData(EntityState.Unchanged, true, CollectionType.HashSet)]
    [InlineData(EntityState.Unchanged, false, CollectionType.HashSet)]
    [InlineData(EntityState.Unchanged, null, CollectionType.HashSet)]
    [InlineData(EntityState.Added, true, CollectionType.List)]
    [InlineData(EntityState.Added, false, CollectionType.List)]
    [InlineData(EntityState.Added, null, CollectionType.List)]
    [InlineData(EntityState.Modified, true, CollectionType.List)]
    [InlineData(EntityState.Modified, false, CollectionType.List)]
    [InlineData(EntityState.Modified, null, CollectionType.List)]
    [InlineData(EntityState.Unchanged, true, CollectionType.List)]
    [InlineData(EntityState.Unchanged, false, CollectionType.List)]
    [InlineData(EntityState.Unchanged, null, CollectionType.List)]
    [InlineData(EntityState.Added, true, CollectionType.SortedSet)]
    [InlineData(EntityState.Added, false, CollectionType.SortedSet)]
    [InlineData(EntityState.Added, null, CollectionType.SortedSet)]
    [InlineData(EntityState.Modified, true, CollectionType.SortedSet)]
    [InlineData(EntityState.Modified, false, CollectionType.SortedSet)]
    [InlineData(EntityState.Modified, null, CollectionType.SortedSet)]
    [InlineData(EntityState.Unchanged, true, CollectionType.SortedSet)]
    [InlineData(EntityState.Unchanged, false, CollectionType.SortedSet)]
    [InlineData(EntityState.Unchanged, null, CollectionType.SortedSet)]
    [InlineData(EntityState.Added, true, CollectionType.Collection)]
    [InlineData(EntityState.Added, false, CollectionType.Collection)]
    [InlineData(EntityState.Added, null, CollectionType.Collection)]
    [InlineData(EntityState.Modified, true, CollectionType.Collection)]
    [InlineData(EntityState.Modified, false, CollectionType.Collection)]
    [InlineData(EntityState.Modified, null, CollectionType.Collection)]
    [InlineData(EntityState.Unchanged, true, CollectionType.Collection)]
    [InlineData(EntityState.Unchanged, false, CollectionType.Collection)]
    [InlineData(EntityState.Unchanged, null, CollectionType.Collection)]
    [InlineData(EntityState.Added, true, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Added, false, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Added, null, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Modified, true, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Modified, false, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Modified, null, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Unchanged, true, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Unchanged, false, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Unchanged, null, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Added, true, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Added, false, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Added, null, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Modified, true, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Modified, false, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Modified, null, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Unchanged, true, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Unchanged, false, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Unchanged, null, CollectionType.ObservableHashSet)]
    public void Add_principal_with_dependent_both_navs_collection(
        EntityState entityState,
        bool? useTrackGraph,
        CollectionType collectionType)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };
        if (useTrackGraph == null)
        {
            context.Entry(principal).State = entityState;
        }

        var dependent = new Child { Name = "1", Parent = principal };
        principal.ChildCollection1 = CreateChildCollection(collectionType, dependent);

        var subDependent = new SubChild { Name = "1S", Parent = dependent };
        dependent.SubChildCollection = CreateChildCollection(collectionType, subDependent);

        if (useTrackGraph == null)
        {
            context.ChangeTracker.DetectChanges();
        }
        else if (useTrackGraph == true)
        {
            context.ChangeTracker.TrackGraph(
                principal, e =>
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

        Assert.Equal(
            entityState != EntityState.Unchanged
            || useTrackGraph == null,
            context.ChangeTracker.HasChanges());

        Assert.Equal(3, context.ChangeTracker.Entries().Count());

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Contains(principal.ChildCollection1, e => ReferenceEquals(e, dependent));
                Assert.Same(principal, dependent.Parent);
                Assert.Null(principal.ChildCollection2);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, context.Entry(dependent).State);

                Assert.Contains(dependent.SubChildCollection, e => ReferenceEquals(e, subDependent));
                Assert.Same(dependent, subDependent.Parent);
                var subDependentEntry = context.Entry(subDependent);
                Assert.Equal(principal.Id, subDependentEntry.Property("ParentId").CurrentValue);
                Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, subDependentEntry.State);
                Assert.Equal(
                    typeof(Parent).ShortDisplayName()
                    + "."
                    + nameof(ParentPN.ChildCollection1)
                    + "#"
                    + nameof(Child)
                    + "."
                    + nameof(Child.SubChildCollection)
                    + "#"
                    + nameof(SubChild),
                    subDependentEntry.Metadata.DisplayName());
            });
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added, true, CollectionType.HashSet)]
    [InlineData(EntityState.Added, false, CollectionType.HashSet)]
    [InlineData(EntityState.Added, null, CollectionType.HashSet)]
    [InlineData(EntityState.Modified, true, CollectionType.HashSet)]
    [InlineData(EntityState.Modified, false, CollectionType.HashSet)]
    [InlineData(EntityState.Modified, null, CollectionType.HashSet)]
    [InlineData(EntityState.Unchanged, true, CollectionType.HashSet)]
    [InlineData(EntityState.Unchanged, false, CollectionType.HashSet)]
    [InlineData(EntityState.Unchanged, null, CollectionType.HashSet)]
    [InlineData(EntityState.Added, true, CollectionType.List)]
    [InlineData(EntityState.Added, false, CollectionType.List)]
    [InlineData(EntityState.Added, null, CollectionType.List)]
    [InlineData(EntityState.Modified, true, CollectionType.List)]
    [InlineData(EntityState.Modified, false, CollectionType.List)]
    [InlineData(EntityState.Modified, null, CollectionType.List)]
    [InlineData(EntityState.Unchanged, true, CollectionType.List)]
    [InlineData(EntityState.Unchanged, false, CollectionType.List)]
    [InlineData(EntityState.Unchanged, null, CollectionType.List)]
    [InlineData(EntityState.Added, true, CollectionType.SortedSet)]
    [InlineData(EntityState.Added, false, CollectionType.SortedSet)]
    [InlineData(EntityState.Added, null, CollectionType.SortedSet)]
    [InlineData(EntityState.Modified, true, CollectionType.SortedSet)]
    [InlineData(EntityState.Modified, false, CollectionType.SortedSet)]
    [InlineData(EntityState.Modified, null, CollectionType.SortedSet)]
    [InlineData(EntityState.Unchanged, true, CollectionType.SortedSet)]
    [InlineData(EntityState.Unchanged, false, CollectionType.SortedSet)]
    [InlineData(EntityState.Unchanged, null, CollectionType.SortedSet)]
    [InlineData(EntityState.Added, true, CollectionType.Collection)]
    [InlineData(EntityState.Added, false, CollectionType.Collection)]
    [InlineData(EntityState.Added, null, CollectionType.Collection)]
    [InlineData(EntityState.Modified, true, CollectionType.Collection)]
    [InlineData(EntityState.Modified, false, CollectionType.Collection)]
    [InlineData(EntityState.Modified, null, CollectionType.Collection)]
    [InlineData(EntityState.Unchanged, true, CollectionType.Collection)]
    [InlineData(EntityState.Unchanged, false, CollectionType.Collection)]
    [InlineData(EntityState.Unchanged, null, CollectionType.Collection)]
    [InlineData(EntityState.Added, true, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Added, false, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Added, null, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Modified, true, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Modified, false, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Modified, null, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Unchanged, true, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Unchanged, false, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Unchanged, null, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Added, true, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Added, false, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Added, null, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Modified, true, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Modified, false, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Modified, null, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Unchanged, true, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Unchanged, false, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Unchanged, null, CollectionType.ObservableHashSet)]
    public void Add_principal_with_dependent_principal_nav_collection(
        EntityState entityState,
        bool? useTrackGraph,
        CollectionType collectionType)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };
        if (useTrackGraph == null)
        {
            context.Entry(principal).State = entityState;
        }

        var dependent = new Child { Name = "1" };
        principal.ChildCollection1 = CreateChildCollection(collectionType, dependent);

        var subDependent = new SubChild { Name = "1S" };
        dependent.SubChildCollection = CreateChildCollection(collectionType, subDependent);

        if (useTrackGraph == null)
        {
            context.ChangeTracker.DetectChanges();
        }
        else if (useTrackGraph == true)
        {
            context.ChangeTracker.TrackGraph(
                principal, e =>
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

        Assert.Equal(
            entityState != EntityState.Unchanged
            || useTrackGraph == null,
            context.ChangeTracker.HasChanges());

        Assert.Equal(3, context.ChangeTracker.Entries().Count());

        AssertFixup(
            context,
            () =>
            {
                Assert.Equal(principal.Id, context.Entry(dependent).Property("ParentId").CurrentValue);
                Assert.Contains(principal.ChildCollection1, e => ReferenceEquals(e, dependent));
                Assert.Null(principal.ChildCollection2);
                Assert.Same(principal, dependent.Parent);
                Assert.Equal(entityState, context.Entry(principal).State);
                Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, context.Entry(dependent).State);

                Assert.Contains(dependent.SubChildCollection, e => ReferenceEquals(e, subDependent));
                Assert.Same(dependent, subDependent.Parent);
                var subDependentEntry = context.Entry(subDependent);
                Assert.Equal(principal.Id, subDependentEntry.Property("ParentId").CurrentValue);
                Assert.Equal(useTrackGraph == null ? EntityState.Added : entityState, subDependentEntry.State);
                Assert.Equal(
                    typeof(Parent).ShortDisplayName()
                    + "."
                    + nameof(Parent.ChildCollection1)
                    + "#"
                    + nameof(Child)
                    + "."
                    + nameof(Child.SubChildCollection)
                    + "#"
                    + nameof(SubChild),
                    subDependentEntry.Metadata.DisplayName());
            });
    }

    [ConditionalFact]
    public async Task Principal_nav_set_unidirectional_AddAsync()
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id = 77 };

        var dependent = new ChildPN { Name = "1" };
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

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Instance_changed_unidirectional(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id = 77 };

        var dependent1 = new ChildPN { Name = "1" };
        principal.Child2 = dependent1;

        var subDependent1 = new SubChildPN { Name = "1S" };
        dependent1.SubChild = subDependent1;

        context.ChangeTracker.TrackGraph(principal, e => e.Entry.State = entityState);

        var dependentEntry1 = context.Entry(principal).Reference(p => p.Child2).TargetEntry;

        var dependent2 = new ChildPN { Name = "2" };
        principal.Child2 = dependent2;

        var subDependent2 = new SubChildPN { Name = "2S" };
        dependent2.SubChild = subDependent2;

        context.ChangeTracker.DetectChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 3 : 5, context.ChangeTracker.Entries().Count());
        Assert.Null(principal.Child1);
        Assert.Same(dependent2, principal.Child2);
        Assert.Equal(entityState, context.Entry(principal).State);
        Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
        var dependentEntry2 = context.Entry(principal).Reference(p => p.Child2).TargetEntry;
        Assert.Equal(principal.Id, dependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, dependentEntry2.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.Child2) + "#" + nameof(ChildPN),
            dependentEntry2.Metadata.DisplayName());

        Assert.Same(subDependent2, dependent2.SubChild);
        var subDependentEntry = dependentEntry2.Reference(p => p.SubChild).TargetEntry;
        Assert.Equal(principal.Id, subDependentEntry.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName()
            + "."
            + nameof(ParentPN.Child2)
            + "#"
            + nameof(ChildPN)
            + "."
            + nameof(ChildPN.SubChild)
            + "#"
            + nameof(SubChildPN), subDependentEntry.Metadata.DisplayName());

        context.ChangeTracker.CascadeChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 3 : 5, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.AcceptAllChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
        Assert.Null(principal.Child1);
        Assert.Same(dependent2, principal.Child2);
        Assert.Same(subDependent2, dependent2.SubChild);
        Assert.False(context.ChangeTracker.HasChanges());
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Instance_changed_bidirectional(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };

        var dependent1 = new Child { Name = "1" };
        principal.Child1 = dependent1;

        var subDependent11 = new SubChild { Name = "1S1" };
        dependent1.SubChild1 = subDependent11;
        var subDependent21 = new SubChild { Name = "1S2" };
        dependent1.SubChild2 = subDependent21;

        context.ChangeTracker.TrackGraph(principal, e => e.Entry.State = entityState);

        var dependentEntry1 = context.Entry(principal).Reference(p => p.Child1).TargetEntry;

        var dependent2 = new Child { Name = "2" };
        principal.Child1 = dependent2;

        var subDependent12 = new SubChild { Name = "2S1" };
        dependent2.SubChild1 = subDependent12;
        var subDependent22 = new SubChild { Name = "2S2" };
        dependent2.SubChild2 = subDependent22;

        context.ChangeTracker.DetectChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 4 : 7, context.ChangeTracker.Entries().Count());
        Assert.Null(principal.Child2);
        Assert.Same(principal, dependent2.Parent);
        Assert.Same(dependent2, principal.Child1);
        Assert.Equal(entityState, context.Entry(principal).State);
        Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
        var dependentEntry2 = context.Entry(principal).Reference(p => p.Child1).TargetEntry;
        Assert.Equal(principal.Id, dependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, dependentEntry2.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName() + "." + nameof(Parent.Child1) + "#" + nameof(Child),
            dependentEntry2.Metadata.DisplayName());

        Assert.Same(subDependent12, dependent2.SubChild1);
        Assert.Same(dependent2, subDependent12.Parent);
        var subDependentEntry1 = dependentEntry2.Reference(p => p.SubChild1).TargetEntry!;
        Assert.Equal(principal.Id, subDependentEntry1.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry1.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName()
            + "."
            + nameof(Parent.Child1)
            + "#"
            + nameof(Child)
            + "."
            + nameof(Child.SubChild1)
            + "#"
            + nameof(SubChild), subDependentEntry1.Metadata.DisplayName());

        Assert.Same(subDependent22, dependent2.SubChild2);
        Assert.Same(dependent2, subDependent22.Parent);
        var subDependentEntry2 = dependentEntry2.Reference(p => p.SubChild2).TargetEntry!;
        Assert.Equal(principal.Id, subDependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry2.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName()
            + "."
            + nameof(Parent.Child1)
            + "#"
            + nameof(Child)
            + "."
            + nameof(Child.SubChild2)
            + "#"
            + nameof(SubChild), subDependentEntry2.Metadata.DisplayName());

        context.ChangeTracker.CascadeChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 4 : 7, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.AcceptAllChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(4, context.ChangeTracker.Entries().Count());
        Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
        Assert.Null(principal.Child2);
        Assert.Same(dependent2, principal.Child1);
        Assert.Same(dependent2, subDependent12.Parent);
        Assert.Same(subDependent12, dependent2.SubChild1);
        Assert.Same(dependent2, subDependent22.Parent);
        Assert.Same(subDependent22, dependent2.SubChild2);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added, CollectionType.HashSet)]
    [InlineData(EntityState.Modified, CollectionType.HashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.HashSet)]
    [InlineData(EntityState.Added, CollectionType.List)]
    [InlineData(EntityState.Modified, CollectionType.List)]
    [InlineData(EntityState.Unchanged, CollectionType.List)]
    [InlineData(EntityState.Added, CollectionType.SortedSet)]
    [InlineData(EntityState.Modified, CollectionType.SortedSet)]
    [InlineData(EntityState.Unchanged, CollectionType.SortedSet)]
    [InlineData(EntityState.Added, CollectionType.Collection)]
    [InlineData(EntityState.Modified, CollectionType.Collection)]
    [InlineData(EntityState.Unchanged, CollectionType.Collection)]
    [InlineData(EntityState.Added, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Modified, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Added, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Modified, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableHashSet)]
    public void Instance_changed_unidirectional_collection(EntityState entityState, CollectionType collectionType)
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id = 77 };

        var dependent1 = new ChildPN { Name = "1" };
        principal.ChildCollection2 = CreateChildCollection(collectionType, dependent1);

        var subDependent1 = new SubChildPN { Name = "1S" };
        dependent1.SubChildCollection = CreateChildCollection(collectionType, subDependent1);

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

        var dependent2 = new ChildPN { Name = "2" };
        principal.ChildCollection2 = CreateChildCollection(collectionType, dependent2);

        var subDependent2 = new SubChildPN { Name = "2S" };
        dependent2.SubChildCollection = CreateChildCollection(collectionType, subDependent2);

        var dependentEntry2 = context.Entry(principal).Collection(p => p.ChildCollection2)
            .FindEntry(dependent2);
        dependentEntry2.Property<int>("Id").CurrentValue = dependentEntry1.Property<int>("Id").CurrentValue;

        var subDependentEntry2 = dependentEntry2.Collection(p => p.SubChildCollection)
            .FindEntry(subDependent2);
        subDependentEntry2.Property<int>("Id").CurrentValue = subDependentEntry1.Property<int>("Id").CurrentValue;

        context.ChangeTracker.DetectChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 3 : 5, context.ChangeTracker.Entries().Count());
        Assert.Null(principal.ChildCollection1);
        Assert.Contains(principal.ChildCollection2, e => ReferenceEquals(e, dependent2));
        Assert.Equal(entityState, context.Entry(principal).State);
        Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
        Assert.Equal(principal.Id, dependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, dependentEntry2.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.ChildCollection2) + "#" + nameof(ChildPN),
            dependentEntry2.Metadata.DisplayName());

        Assert.Contains(dependent2.SubChildCollection, e => ReferenceEquals(e, subDependent2));
        Assert.Equal(principal.Id, subDependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry2.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName()
            + "."
            + nameof(ParentPN.ChildCollection2)
            + "#"
            + nameof(ChildPN)
            + "."
            + nameof(ChildPN.SubChildCollection)
            + "#"
            + nameof(SubChildPN), subDependentEntry2.Metadata.DisplayName());

        context.ChangeTracker.CascadeChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 3 : 5, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.AcceptAllChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
        Assert.Null(principal.ChildCollection1);
        Assert.Contains(principal.ChildCollection2, e => ReferenceEquals(e, dependent2));
        Assert.Contains(subDependent2, dependent2.SubChildCollection);
        Assert.Contains(dependent2.SubChildCollection, e => ReferenceEquals(e, subDependent2));
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added, CollectionType.HashSet)]
    [InlineData(EntityState.Modified, CollectionType.HashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.HashSet)]
    [InlineData(EntityState.Added, CollectionType.List)]
    [InlineData(EntityState.Modified, CollectionType.List)]
    [InlineData(EntityState.Unchanged, CollectionType.List)]
    [InlineData(EntityState.Added, CollectionType.SortedSet)]
    [InlineData(EntityState.Modified, CollectionType.SortedSet)]
    [InlineData(EntityState.Unchanged, CollectionType.SortedSet)]
    [InlineData(EntityState.Added, CollectionType.Collection)]
    [InlineData(EntityState.Modified, CollectionType.Collection)]
    [InlineData(EntityState.Unchanged, CollectionType.Collection)]
    [InlineData(EntityState.Added, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Modified, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Added, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Modified, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableHashSet)]
    public void Instance_changed_bidirectional_collection(EntityState entityState, CollectionType collectionType)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };

        var dependent1 = new Child { Name = "1" };
        principal.ChildCollection1 = CreateChildCollection(collectionType, dependent1);

        var subDependent1 = new SubChild { Name = "1S" };
        dependent1.SubChildCollection = CreateChildCollection(collectionType, subDependent1);

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

        var dependent2 = new Child { Name = "2" };
        principal.ChildCollection1 = CreateChildCollection(collectionType, dependent2);

        var subDependent2 = new SubChild { Name = "2S" };
        dependent2.SubChildCollection = CreateChildCollection(collectionType, subDependent2);

        var dependentEntry2 = context.Entry(principal).Collection(p => p.ChildCollection1)
            .FindEntry(dependent2);
        dependentEntry2.Property<int>("Id").CurrentValue = dependentEntry1.Property<int>("Id").CurrentValue;

        var subDependentEntry2 = dependentEntry2.Collection(p => p.SubChildCollection)
            .FindEntry(subDependent2);
        subDependentEntry2.Property<int>("Id").CurrentValue = subDependentEntry1.Property<int>("Id").CurrentValue;

        context.ChangeTracker.DetectChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 3 : 5, context.ChangeTracker.Entries().Count());
        Assert.Null(principal.ChildCollection2);
        Assert.Same(principal, dependent2.Parent);
        Assert.Contains(principal.ChildCollection1, e => ReferenceEquals(e, dependent2));
        Assert.Equal(entityState, context.Entry(principal).State);
        Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
        Assert.Equal(principal.Id, dependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, dependentEntry2.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName() + "." + nameof(Parent.ChildCollection1) + "#" + nameof(Child),
            dependentEntry2.Metadata.DisplayName());

        Assert.Contains(dependent2.SubChildCollection, e => ReferenceEquals(e, subDependent2));
        Assert.Same(dependent2, subDependent2.Parent);
        Assert.Equal(principal.Id, subDependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry2.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName()
            + "."
            + nameof(Parent.ChildCollection1)
            + "#"
            + nameof(Child)
            + "."
            + nameof(Child.SubChildCollection)
            + "#"
            + nameof(SubChild), subDependentEntry2.Metadata.DisplayName());

        context.ChangeTracker.CascadeChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 3 : 5, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.AcceptAllChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
        Assert.Null(principal.ChildCollection2);
        Assert.Contains(principal.ChildCollection1, e => ReferenceEquals(e, dependent2));
        Assert.Same(dependent2, subDependent2.Parent);
        Assert.Contains(dependent2.SubChildCollection, e => ReferenceEquals(e, subDependent2));
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Identity_changed_unidirectional(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id = 77 };

        var dependent = new ChildPN { Name = "1" };
        principal.Child1 = dependent;

        var subDependent = new SubChildPN { Name = "1S" };
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

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 3 : 5, context.ChangeTracker.Entries().Count());
        Assert.Null(principal.Child1);
        Assert.Same(dependent, principal.Child2);
        Assert.Equal(entityState, context.Entry(principal).State);
        Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
        var dependentEntry2 = context.Entry(principal).Reference(p => p.Child2).TargetEntry;
        Assert.Equal(principal.Id, dependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, dependentEntry2.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.Child2) + "#" + nameof(ChildPN),
            dependentEntry2.Metadata.DisplayName());

        Assert.Same(subDependent, dependent.SubChild);
        var subDependentEntry = dependentEntry2.Reference(p => p.SubChild).TargetEntry;
        Assert.Equal(principal.Id, subDependentEntry.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName()
            + "."
            + nameof(ParentPN.Child2)
            + "#"
            + nameof(ChildPN)
            + "."
            + nameof(ChildPN.SubChild)
            + "#"
            + nameof(SubChildPN),
            subDependentEntry.Metadata.DisplayName());

        context.ChangeTracker.CascadeChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 3 : 5, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.AcceptAllChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
        Assert.Null(principal.Child1);
        Assert.Same(dependent, principal.Child2);
        Assert.Same(subDependent, dependent.SubChild);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Identity_changed_bidirectional(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };

        var dependent = new Child { Name = "1" };
        principal.Child2 = dependent;

        var subDependent1 = new SubChild { Name = "1S1" };
        dependent.SubChild1 = subDependent1;
        var subDependent2 = new SubChild { Name = "1S2" };
        dependent.SubChild2 = subDependent2;

        context.ChangeTracker.TrackGraph(principal, e => e.Entry.State = entityState);

        var dependentEntry1 = context.Entry(principal).Reference(p => p.Child2).TargetEntry;

        principal.Child1 = dependent;
        principal.Child2 = null;

        context.ChangeTracker.DetectChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 4 : 7, context.ChangeTracker.Entries().Count());
        Assert.Null(principal.Child2);
        Assert.Same(principal, dependent.Parent);
        Assert.Same(dependent, principal.Child1);
        Assert.Equal(entityState, context.Entry(principal).State);
        Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
        var dependentEntry2 = context.Entry(principal).Reference(p => p.Child1).TargetEntry;
        Assert.Equal(principal.Id, dependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, dependentEntry2.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName() + "." + nameof(Parent.Child1) + "#" + nameof(Child),
            dependentEntry2.Metadata.DisplayName());

        Assert.Same(subDependent1, dependent.SubChild1);
        Assert.Same(dependent, subDependent1.Parent);
        var subDependentEntry1 = dependentEntry2.Reference(p => p.SubChild1).TargetEntry!;
        Assert.Equal(principal.Id, subDependentEntry1.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry1.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName()
            + "."
            + nameof(Parent.Child1)
            + "#"
            + nameof(Child)
            + "."
            + nameof(Child.SubChild1)
            + "#"
            + nameof(SubChild), subDependentEntry1.Metadata.DisplayName());

        Assert.Same(subDependent2, dependent.SubChild2);
        Assert.Same(dependent, subDependent1.Parent);
        var subDependentEntry2 = dependentEntry2.Reference(p => p.SubChild2).TargetEntry!;
        Assert.Equal(principal.Id, subDependentEntry1.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry1.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName()
            + "."
            + nameof(Parent.Child1)
            + "#"
            + nameof(Child)
            + "."
            + nameof(Child.SubChild2)
            + "#"
            + nameof(SubChild), subDependentEntry2.Metadata.DisplayName());

        context.ChangeTracker.CascadeChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 4 : 7, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.AcceptAllChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(4, context.ChangeTracker.Entries().Count());
        Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
        Assert.Null(principal.Child2);
        Assert.Same(dependent, principal.Child1);
        Assert.Same(subDependent1, dependent.SubChild1);
        Assert.Same(dependent, subDependent1.Parent);
        Assert.Same(subDependent2, dependent.SubChild2);
        Assert.Same(dependent, subDependent2.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added, CollectionType.HashSet)]
    [InlineData(EntityState.Modified, CollectionType.HashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.HashSet)]
    [InlineData(EntityState.Added, CollectionType.List)]
    [InlineData(EntityState.Modified, CollectionType.List)]
    [InlineData(EntityState.Unchanged, CollectionType.List)]
    [InlineData(EntityState.Added, CollectionType.SortedSet)]
    [InlineData(EntityState.Modified, CollectionType.SortedSet)]
    [InlineData(EntityState.Unchanged, CollectionType.SortedSet)]
    [InlineData(EntityState.Added, CollectionType.Collection)]
    [InlineData(EntityState.Modified, CollectionType.Collection)]
    [InlineData(EntityState.Unchanged, CollectionType.Collection)]
    [InlineData(EntityState.Added, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Modified, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Added, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Modified, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableHashSet)]
    public void Identity_changed_unidirectional_collection(EntityState entityState, CollectionType collectionType)
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id = 77 };

        var dependent = new ChildPN { Name = "1" };
        principal.ChildCollection1 = CreateChildCollection(collectionType, dependent);

        var subDependent = new SubChildPN { Name = "1S" };
        dependent.SubChildCollection = CreateChildCollection(collectionType, subDependent);

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

        var dependentEntry1 = context.Entry(principal).Collection(p => p.ChildCollection1).FindEntry(dependent);

        principal.ChildCollection2 = principal.ChildCollection1;
        principal.ChildCollection1 = null;

        context.ChangeTracker.DetectChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 3 : 5, context.ChangeTracker.Entries().Count());
        Assert.Null(principal.ChildCollection1);
        Assert.Contains(principal.ChildCollection2, e => ReferenceEquals(e, dependent));
        Assert.Equal(entityState, context.Entry(principal).State);
        Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
        var dependentEntry2 = context.Entry(principal).Collection(p => p.ChildCollection2).FindEntry(dependent);
        Assert.Equal(principal.Id, dependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, dependentEntry2.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.ChildCollection2) + "#" + nameof(ChildPN),
            dependentEntry2.Metadata.DisplayName());

        Assert.Contains(dependent.SubChildCollection, e => ReferenceEquals(e, subDependent));
        var subDependentEntry = dependentEntry2.Collection(p => p.SubChildCollection).FindEntry(subDependent);
        Assert.Equal(principal.Id, subDependentEntry.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName()
            + "."
            + nameof(ParentPN.ChildCollection2)
            + "#"
            + nameof(ChildPN)
            + "."
            + nameof(ChildPN.SubChildCollection)
            + "#"
            + nameof(SubChildPN),
            subDependentEntry.Metadata.DisplayName());

        context.ChangeTracker.CascadeChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 3 : 5, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.AcceptAllChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
        Assert.Null(principal.ChildCollection1);
        Assert.Contains(principal.ChildCollection2, e => ReferenceEquals(e, dependent));
        Assert.Contains(dependent.SubChildCollection, e => ReferenceEquals(e, subDependent));
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added, CollectionType.HashSet)]
    [InlineData(EntityState.Modified, CollectionType.HashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.HashSet)]
    [InlineData(EntityState.Added, CollectionType.List)]
    [InlineData(EntityState.Modified, CollectionType.List)]
    [InlineData(EntityState.Unchanged, CollectionType.List)]
    [InlineData(EntityState.Added, CollectionType.SortedSet)]
    [InlineData(EntityState.Modified, CollectionType.SortedSet)]
    [InlineData(EntityState.Unchanged, CollectionType.SortedSet)]
    [InlineData(EntityState.Added, CollectionType.Collection)]
    [InlineData(EntityState.Modified, CollectionType.Collection)]
    [InlineData(EntityState.Unchanged, CollectionType.Collection)]
    [InlineData(EntityState.Added, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Modified, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Added, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Modified, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableHashSet)]
    public void Identity_changed_bidirectional_collection(EntityState entityState, CollectionType collectionType)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };

        var dependent = new Child { Name = "1" };
        principal.ChildCollection2 = CreateChildCollection(collectionType, dependent);

        var subDependent = new SubChild { Name = "1S" };
        dependent.SubChildCollection = CreateChildCollection(collectionType, subDependent);

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

        var dependentEntry1 = context.Entry(principal).Collection(p => p.ChildCollection2).FindEntry(dependent);

        principal.ChildCollection1 = principal.ChildCollection2;
        principal.ChildCollection2 = null;

        context.ChangeTracker.DetectChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 3 : 5, context.ChangeTracker.Entries().Count());
        Assert.Null(principal.ChildCollection2);
        Assert.Same(principal, dependent.Parent);
        Assert.Contains(principal.ChildCollection1, e => ReferenceEquals(e, dependent));
        Assert.Equal(entityState, context.Entry(principal).State);
        Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
        var dependentEntry2 = context.Entry(principal).Collection(p => p.ChildCollection1).FindEntry(dependent);
        Assert.Equal(principal.Id, dependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, dependentEntry2.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName() + "." + nameof(Parent.ChildCollection1) + "#" + nameof(Child),
            dependentEntry2.Metadata.DisplayName());

        Assert.Contains(dependent.SubChildCollection, e => ReferenceEquals(e, subDependent));
        Assert.Same(dependent, subDependent.Parent);
        var subDependentEntry = dependentEntry2.Collection(p => p.SubChildCollection).FindEntry(subDependent);
        Assert.Equal(principal.Id, subDependentEntry.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName()
            + "."
            + nameof(Parent.ChildCollection1)
            + "#"
            + nameof(Child)
            + "."
            + nameof(Child.SubChildCollection)
            + "#"
            + nameof(SubChild), subDependentEntry.Metadata.DisplayName());

        context.ChangeTracker.CascadeChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 3 : 5, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.AcceptAllChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
        Assert.Null(principal.Child2);
        Assert.Contains(principal.ChildCollection1, e => ReferenceEquals(e, dependent));
        Assert.Contains(dependent.SubChildCollection, e => ReferenceEquals(e, subDependent));
        Assert.Same(dependent, subDependent.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Identity_swapped_unidirectional(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id = 77 };

        var dependent1 = new ChildPN { Name = "1" };
        principal.Child1 = dependent1;

        var subDependent1 = new SubChildPN { Name = "1S" };
        dependent1.SubChild = subDependent1;

        var dependent2 = new ChildPN { Name = "2" };
        principal.Child2 = dependent2;

        var subDependent2 = new SubChildPN { Name = "2S" };
        dependent2.SubChild = subDependent2;

        context.ChangeTracker.TrackGraph(principal, e => e.Entry.State = entityState);

        Assert.Equal(entityState != EntityState.Unchanged, context.ChangeTracker.HasChanges());

        principal.Child2 = dependent1;
        principal.Child1 = dependent2;

        context.ChangeTracker.DetectChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 5 : 9, context.ChangeTracker.Entries().Count());
        Assert.Same(dependent1, principal.Child2);
        Assert.Same(dependent2, principal.Child1);
        Assert.Equal(entityState, context.Entry(principal).State);

        var dependent1Entry = context.Entry(principal).Reference(p => p.Child1).TargetEntry;
        Assert.Equal(principal.Id, dependent1Entry.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, dependent1Entry.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.Child1) + "#" + nameof(ChildPN),
            dependent1Entry.Metadata.DisplayName());
        Assert.Equal(
            entityState == EntityState.Added ? null : EntityState.Deleted,
            dependent1Entry.GetInfrastructure().SharedIdentityEntry?.EntityState);

        var dependent2Entry = context.Entry(principal).Reference(p => p.Child2).TargetEntry;
        Assert.Equal(principal.Id, dependent2Entry.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, dependent2Entry.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.Child2) + "#" + nameof(ChildPN),
            dependent2Entry.Metadata.DisplayName());
        Assert.Equal(
            entityState == EntityState.Added ? null : EntityState.Deleted,
            dependent2Entry.GetInfrastructure().SharedIdentityEntry?.EntityState);

        Assert.Same(subDependent1, dependent1.SubChild);
        var subDependentEntry1 = dependent1Entry.Reference(p => p.SubChild).TargetEntry;
        Assert.Equal(principal.Id, subDependentEntry1.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry1.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName()
            + "."
            + nameof(ParentPN.Child1)
            + "#"
            + nameof(ChildPN)
            + "."
            + nameof(ChildPN.SubChild)
            + "#"
            + nameof(SubChildPN), subDependentEntry1.Metadata.DisplayName());

        Assert.Same(subDependent2, dependent2.SubChild);
        var subDependentEntry2 = dependent2Entry.Reference(p => p.SubChild).TargetEntry;
        Assert.Equal(principal.Id, subDependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry2.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName()
            + "."
            + nameof(ParentPN.Child2)
            + "#"
            + nameof(ChildPN)
            + "."
            + nameof(ChildPN.SubChild)
            + "#"
            + nameof(SubChildPN), subDependentEntry2.Metadata.DisplayName());

        context.ChangeTracker.CascadeChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 5 : 9, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.AcceptAllChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(5, context.ChangeTracker.Entries().Count());
        Assert.Null(dependent1Entry.GetInfrastructure().SharedIdentityEntry);
        Assert.Null(dependent2Entry.GetInfrastructure().SharedIdentityEntry);
        Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
        Assert.Same(dependent1, principal.Child2);
        Assert.Same(dependent2, principal.Child1);
        Assert.Same(subDependent1, dependent1.SubChild);
        Assert.Same(subDependent2, dependent2.SubChild);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Identity_swapped_bidirectional(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };

        var dependent1 = new Child { Name = "1" };
        principal.Child1 = dependent1;

        var subDependent11 = new SubChild { Name = "1S1" };
        dependent1.SubChild1 = subDependent11;
        var subDependent21 = new SubChild { Name = "1S2" };
        dependent1.SubChild2 = subDependent21;

        var dependent2 = new Child { Name = "2" };
        principal.Child2 = dependent2;

        var subDependent12 = new SubChild { Name = "2S1" };
        dependent2.SubChild1 = subDependent12;
        var subDependent22 = new SubChild { Name = "2S2" };
        dependent2.SubChild2 = subDependent22;

        context.ChangeTracker.TrackGraph(principal, e => e.Entry.State = entityState);

        Assert.Equal(entityState != EntityState.Unchanged, context.ChangeTracker.HasChanges());

        principal.Child2 = dependent1;
        principal.Child1 = dependent2;

        context.ChangeTracker.DetectChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 7 : 13, context.ChangeTracker.Entries().Count());
        Assert.Same(principal, dependent1.Parent);
        Assert.Same(dependent1, principal.Child2);
        Assert.Same(principal, dependent2.Parent);
        Assert.Same(dependent2, principal.Child1);
        Assert.Equal(entityState, context.Entry(principal).State);

        var dependent1Entry = context.Entry(principal).Reference(p => p.Child1).TargetEntry;
        Assert.Equal(principal.Id, dependent1Entry.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, dependent1Entry.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName() + "." + nameof(Parent.Child1) + "#" + nameof(Child),
            dependent1Entry.Metadata.DisplayName());
        Assert.Equal(
            entityState == EntityState.Added ? null : EntityState.Deleted,
            dependent1Entry.GetInfrastructure().SharedIdentityEntry?.EntityState);

        var dependent2Entry = context.Entry(principal).Reference(p => p.Child2).TargetEntry;
        Assert.Equal(principal.Id, dependent2Entry.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, dependent2Entry.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName() + "." + nameof(Parent.Child2) + "#" + nameof(Child),
            dependent2Entry.Metadata.DisplayName());
        Assert.Equal(
            entityState == EntityState.Added ? null : EntityState.Deleted,
            dependent2Entry.GetInfrastructure().SharedIdentityEntry?.EntityState);

        Assert.Same(subDependent11, dependent1.SubChild1);
        Assert.Same(dependent1, subDependent11.Parent);
        var subDependentEntry11 = dependent1Entry.Reference(p => p.SubChild1).TargetEntry!;
        Assert.Equal(principal.Id, subDependentEntry11.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry11.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName()
            + "."
            + nameof(Parent.Child1)
            + "#"
            + nameof(Child)
            + "."
            + nameof(Child.SubChild1)
            + "#"
            + nameof(SubChild), subDependentEntry11.Metadata.DisplayName());

        Assert.Same(subDependent21, dependent1.SubChild2);
        Assert.Same(dependent1, subDependent21.Parent);
        var subDependentEntry21 = dependent1Entry.Reference(p => p.SubChild2).TargetEntry!;
        Assert.Equal(principal.Id, subDependentEntry21.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry11.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName()
            + "."
            + nameof(Parent.Child1)
            + "#"
            + nameof(Child)
            + "."
            + nameof(Child.SubChild2)
            + "#"
            + nameof(SubChild), subDependentEntry21.Metadata.DisplayName());

        Assert.Same(subDependent12, dependent2.SubChild1);
        Assert.Same(dependent2, subDependent12.Parent);
        var subDependentEntry12 = dependent1Entry.Reference(p => p.SubChild1).TargetEntry!;
        Assert.Equal(principal.Id, subDependentEntry12.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry12.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName()
            + "."
            + nameof(Parent.Child1)
            + "#"
            + nameof(Child)
            + "."
            + nameof(Child.SubChild1)
            + "#"
            + nameof(SubChild), subDependentEntry12.Metadata.DisplayName());

        Assert.Same(subDependent22, dependent2.SubChild2);
        Assert.Same(dependent2, subDependent22.Parent);
        var subDependentEntry22 = dependent1Entry.Reference(p => p.SubChild2).TargetEntry!;
        Assert.Equal(principal.Id, subDependentEntry12.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry22.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName()
            + "."
            + nameof(Parent.Child1)
            + "#"
            + nameof(Child)
            + "."
            + nameof(Child.SubChild2)
            + "#"
            + nameof(SubChild), subDependentEntry22.Metadata.DisplayName());

        context.ChangeTracker.CascadeChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 7 : 13, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.AcceptAllChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(7, context.ChangeTracker.Entries().Count());
        Assert.Null(dependent1Entry.GetInfrastructure().SharedIdentityEntry);
        Assert.Null(dependent2Entry.GetInfrastructure().SharedIdentityEntry);
        Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
        Assert.Same(dependent1, principal.Child2);
        Assert.Same(dependent2, principal.Child1);
        Assert.Same(subDependent11, dependent1.SubChild1);
        Assert.Same(subDependent12, dependent2.SubChild1);
        Assert.Same(subDependent21, dependent1.SubChild2);
        Assert.Same(subDependent22, dependent2.SubChild2);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added, CollectionType.HashSet)]
    [InlineData(EntityState.Modified, CollectionType.HashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.HashSet)]
    [InlineData(EntityState.Added, CollectionType.List)]
    [InlineData(EntityState.Modified, CollectionType.List)]
    [InlineData(EntityState.Unchanged, CollectionType.List)]
    [InlineData(EntityState.Added, CollectionType.SortedSet)]
    [InlineData(EntityState.Modified, CollectionType.SortedSet)]
    [InlineData(EntityState.Unchanged, CollectionType.SortedSet)]
    [InlineData(EntityState.Added, CollectionType.Collection)]
    [InlineData(EntityState.Modified, CollectionType.Collection)]
    [InlineData(EntityState.Unchanged, CollectionType.Collection)]
    [InlineData(EntityState.Added, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Modified, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Added, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Modified, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableHashSet)]
    public void Identity_swapped_unidirectional_collection(EntityState entityState, CollectionType collectionType)
    {
        using var context = new FixupContext();
        var principal = new ParentPN { Id = 77 };

        var dependent1 = new ChildPN { Name = "1" };
        principal.ChildCollection1 = CreateChildCollection(collectionType, dependent1);

        var subDependent1 = new SubChildPN { Name = "1S" };
        dependent1.SubChildCollection = CreateChildCollection(collectionType, subDependent1);

        var dependent2 = new ChildPN { Name = "2" };
        principal.ChildCollection2 = CreateChildCollection(collectionType, dependent2);

        var subDependent2 = new SubChildPN { Name = "2S" };
        dependent2.SubChildCollection = CreateChildCollection(collectionType, subDependent2);

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

        Assert.Equal(entityState != EntityState.Unchanged, context.ChangeTracker.HasChanges());

        var dependentEntry1 = context.Entry(dependent1);
        var dependentEntry2 = context.Entry(dependent2);
        var subDependentEntry1 = context.Entry(subDependent1);
        var subDependentEntry2 = context.Entry(subDependent2);

        var tempCollection = principal.ChildCollection2;
        principal.ChildCollection2 = principal.ChildCollection1;
        principal.ChildCollection1 = tempCollection;

        var newDependentEntry1 = context.Entry(principal).Collection(p => p.ChildCollection1)
            .FindEntry(dependent2);
        newDependentEntry1.Property<int>("Id").CurrentValue = dependentEntry2.Property<int>("Id").CurrentValue;

        var newDependentEntry2 = context.Entry(principal).Collection(p => p.ChildCollection2)
            .FindEntry(dependent1);
        newDependentEntry2.Property<int>("Id").CurrentValue = dependentEntry1.Property<int>("Id").CurrentValue;

        var newSubDependentEntry1 = newDependentEntry1.Collection(p => p.SubChildCollection)
            .FindEntry(subDependent2);
        newSubDependentEntry1.Property<int>("Id").CurrentValue = subDependentEntry2.Property<int>("Id").CurrentValue;

        var newSubDependentEntry2 = newDependentEntry2.Collection(p => p.SubChildCollection)
            .FindEntry(subDependent1);
        newSubDependentEntry2.Property<int>("Id").CurrentValue = subDependentEntry1.Property<int>("Id").CurrentValue;

        context.ChangeTracker.DetectChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 5 : 9, context.ChangeTracker.Entries().Count());
        Assert.Contains(principal.ChildCollection2, e => ReferenceEquals(e, dependent1));
        Assert.Contains(principal.ChildCollection1, e => ReferenceEquals(e, dependent2));
        Assert.Equal(entityState, context.Entry(principal).State);

        Assert.Equal(principal.Id, newDependentEntry1.Property("ParentId").CurrentValue);
        Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, newDependentEntry1.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.ChildCollection1) + "#" + nameof(ChildPN),
            newDependentEntry1.Metadata.DisplayName());
        Assert.Equal(
            entityState == EntityState.Added ? null : EntityState.Deleted,
            newDependentEntry1.GetInfrastructure().SharedIdentityEntry?.EntityState);

        Assert.Equal(principal.Id, newDependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, newDependentEntry2.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.ChildCollection2) + "#" + nameof(ChildPN),
            newDependentEntry2.Metadata.DisplayName());
        Assert.Equal(
            entityState == EntityState.Added ? null : EntityState.Deleted,
            newDependentEntry2.GetInfrastructure().SharedIdentityEntry?.EntityState);

        Assert.Contains(dependent1.SubChildCollection, e => ReferenceEquals(e, subDependent1));
        Assert.Equal(principal.Id, newSubDependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, newSubDependentEntry2.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName()
            + "."
            + nameof(ParentPN.ChildCollection2)
            + "#"
            + nameof(ChildPN)
            + "."
            + nameof(ChildPN.SubChildCollection)
            + "#"
            + nameof(SubChildPN), newSubDependentEntry2.Metadata.DisplayName());

        Assert.Contains(dependent2.SubChildCollection, e => ReferenceEquals(e, subDependent2));
        Assert.Equal(principal.Id, newSubDependentEntry1.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, newSubDependentEntry1.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName()
            + "."
            + nameof(ParentPN.ChildCollection1)
            + "#"
            + nameof(ChildPN)
            + "."
            + nameof(ChildPN.SubChildCollection)
            + "#"
            + nameof(SubChildPN), newSubDependentEntry1.Metadata.DisplayName());

        context.ChangeTracker.CascadeChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 5 : 9, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.AcceptAllChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(5, context.ChangeTracker.Entries().Count());
        Assert.Null(newDependentEntry1.GetInfrastructure().SharedIdentityEntry);
        Assert.Null(newDependentEntry2.GetInfrastructure().SharedIdentityEntry);
        Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
        Assert.Contains(principal.ChildCollection2, e => ReferenceEquals(e, dependent1));
        Assert.Contains(principal.ChildCollection1, e => ReferenceEquals(e, dependent2));
        Assert.Contains(dependent1.SubChildCollection, e => ReferenceEquals(e, subDependent1));
        Assert.Contains(dependent2.SubChildCollection, e => ReferenceEquals(e, subDependent2));
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added, CollectionType.HashSet)]
    [InlineData(EntityState.Modified, CollectionType.HashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.HashSet)]
    [InlineData(EntityState.Added, CollectionType.List)]
    [InlineData(EntityState.Modified, CollectionType.List)]
    [InlineData(EntityState.Unchanged, CollectionType.List)]
    [InlineData(EntityState.Added, CollectionType.SortedSet)]
    [InlineData(EntityState.Modified, CollectionType.SortedSet)]
    [InlineData(EntityState.Unchanged, CollectionType.SortedSet)]
    [InlineData(EntityState.Added, CollectionType.Collection)]
    [InlineData(EntityState.Modified, CollectionType.Collection)]
    [InlineData(EntityState.Unchanged, CollectionType.Collection)]
    [InlineData(EntityState.Added, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Modified, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Added, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Modified, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableHashSet)]
    public void Identity_swapped_bidirectional_collection(EntityState entityState, CollectionType collectionType)
    {
        using var context = new FixupContext();
        var principal = new Parent { Id = 77 };

        var dependent1 = new Child { Name = "1" };
        principal.ChildCollection1 = CreateChildCollection(collectionType, dependent1);

        var subDependent1 = new SubChild { Name = "1S" };
        dependent1.SubChildCollection = CreateChildCollection(collectionType, subDependent1);

        var dependent2 = new Child { Name = "2" };
        principal.ChildCollection2 = CreateChildCollection(collectionType, dependent2);

        var subDependent2 = new SubChild { Name = "2S" };
        dependent2.SubChildCollection = CreateChildCollection(collectionType, subDependent2);

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

        Assert.Equal(entityState != EntityState.Unchanged, context.ChangeTracker.HasChanges());

        var dependentEntry1 = context.Entry(dependent1);
        var dependentEntry2 = context.Entry(dependent2);
        var subDependentEntry1 = context.Entry(subDependent1);
        var subDependentEntry2 = context.Entry(subDependent2);

        var tempCollection = principal.ChildCollection2;
        principal.ChildCollection2 = principal.ChildCollection1;
        principal.ChildCollection1 = tempCollection;

        var newDependentEntry1 = context.Entry(principal).Collection(p => p.ChildCollection1)
            .FindEntry(dependent2);
        newDependentEntry1.Property<int>("Id").CurrentValue = dependentEntry2.Property<int>("Id").CurrentValue;

        var newDependentEntry2 = context.Entry(principal).Collection(p => p.ChildCollection2)
            .FindEntry(dependent1);
        newDependentEntry2.Property<int>("Id").CurrentValue = dependentEntry1.Property<int>("Id").CurrentValue;

        var newSubDependentEntry1 = newDependentEntry2.Collection(p => p.SubChildCollection)
            .FindEntry(subDependent1);
        newSubDependentEntry1.Property<int>("Id").CurrentValue = subDependentEntry1.Property<int>("Id").CurrentValue;

        var newSubDependentEntry2 = newDependentEntry1.Collection(p => p.SubChildCollection)
            .FindEntry(subDependent2);
        newSubDependentEntry2.Property<int>("Id").CurrentValue = subDependentEntry2.Property<int>("Id").CurrentValue;

        context.ChangeTracker.DetectChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 5 : 9, context.ChangeTracker.Entries().Count());
        Assert.Same(principal, dependent1.Parent);
        Assert.Contains(principal.ChildCollection2, e => ReferenceEquals(e, dependent1));
        Assert.Same(principal, dependent2.Parent);
        Assert.Contains(principal.ChildCollection1, e => ReferenceEquals(e, dependent2));
        Assert.Equal(entityState, context.Entry(principal).State);

        Assert.Equal(principal.Id, newDependentEntry1.Property("ParentId").CurrentValue);
        Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, newDependentEntry1.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName() + "." + nameof(Parent.ChildCollection1) + "#" + nameof(Child),
            newDependentEntry1.Metadata.DisplayName());
        Assert.Equal(
            entityState == EntityState.Added ? null : EntityState.Deleted,
            newDependentEntry1.GetInfrastructure().SharedIdentityEntry?.EntityState);

        Assert.Equal(principal.Id, newDependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, newDependentEntry2.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName() + "." + nameof(Parent.ChildCollection2) + "#" + nameof(Child),
            newDependentEntry2.Metadata.DisplayName());
        Assert.Equal(
            entityState == EntityState.Added ? null : EntityState.Deleted,
            newDependentEntry2.GetInfrastructure().SharedIdentityEntry?.EntityState);

        Assert.Contains(dependent1.SubChildCollection, e => ReferenceEquals(e, subDependent1));
        Assert.Same(dependent1, subDependent1.Parent);
        Assert.Equal(principal.Id, newSubDependentEntry1.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, newSubDependentEntry1.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName()
            + "."
            + nameof(Parent.ChildCollection2)
            + "#"
            + nameof(Child)
            + "."
            + nameof(Child.SubChildCollection)
            + "#"
            + nameof(SubChild), newSubDependentEntry1.Metadata.DisplayName());

        Assert.Contains(dependent2.SubChildCollection, e => ReferenceEquals(e, subDependent2));
        Assert.Same(dependent2, subDependent2.Parent);
        Assert.Equal(principal.Id, newSubDependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, newSubDependentEntry2.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName()
            + "."
            + nameof(Parent.ChildCollection1)
            + "#"
            + nameof(Child)
            + "."
            + nameof(Child.SubChildCollection)
            + "#"
            + nameof(SubChild), newSubDependentEntry2.Metadata.DisplayName());

        context.ChangeTracker.CascadeChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 5 : 9, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.AcceptAllChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(5, context.ChangeTracker.Entries().Count());
        Assert.Null(newDependentEntry1.GetInfrastructure().SharedIdentityEntry);
        Assert.Null(newDependentEntry2.GetInfrastructure().SharedIdentityEntry);
        Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
        Assert.Contains(principal.ChildCollection2, e => ReferenceEquals(e, dependent1));
        Assert.Contains(principal.ChildCollection1, e => ReferenceEquals(e, dependent2));
        Assert.Contains(dependent1.SubChildCollection, e => ReferenceEquals(e, subDependent1));
        Assert.Contains(dependent2.SubChildCollection, e => ReferenceEquals(e, subDependent2));
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Parent_changed_unidirectional(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal1 = new ParentPN { Id = 77 };

        var principal2 = new ParentPN { Id = 78 };

        var dependent = new ChildPN { Name = "1" };
        principal1.Child1 = dependent;

        var subDependent = new SubChildPN { Name = "1S" };
        dependent.SubChild = subDependent;

        context.ChangeTracker.TrackGraph(principal1, e => e.Entry.State = entityState);
        context.ChangeTracker.TrackGraph(principal2, e => e.Entry.State = entityState);

        Assert.Equal(entityState != EntityState.Unchanged, context.ChangeTracker.HasChanges());

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
            Assert.True(context.ChangeTracker.HasChanges());

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
            Assert.Equal(
                typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.Child1) + "#" + nameof(ChildPN),
                dependentEntry2.Metadata.DisplayName());

            Assert.Same(subDependent, dependent.SubChild);
            var subDependentEntry = dependentEntry2.Reference(p => p.SubChild).TargetEntry;
            Assert.Equal(principal2.Id, subDependentEntry.Property("ParentId").CurrentValue);
            Assert.Equal(EntityState.Added, subDependentEntry.State);
            Assert.Equal(
                typeof(ParentPN).ShortDisplayName()
                + "."
                + nameof(ParentPN.Child1)
                + "#"
                + nameof(ChildPN)
                + "."
                + nameof(ChildPN.SubChild)
                + "#"
                + nameof(SubChildPN),
                subDependentEntry.Metadata.DisplayName());

            context.ChangeTracker.CascadeChanges();

            Assert.True(context.ChangeTracker.HasChanges());

            Assert.Equal(4, context.ChangeTracker.Entries().Count());

            context.ChangeTracker.AcceptAllChanges();

            Assert.False(context.ChangeTracker.HasChanges());

            Assert.Equal(4, context.ChangeTracker.Entries().Count());
            Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
            Assert.Null(principal1.Child1);
            Assert.Null(principal1.Child2);
            Assert.Same(dependent, principal2.Child1);
            Assert.Null(principal2.Child2);
            Assert.Same(subDependent, dependent.SubChild);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Parent_changed_bidirectional(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal1 = new Parent { Id = 77 };

        var principal2 = new Parent { Id = 78 };

        var dependent = new Child { Name = "1" };
        principal1.Child1 = dependent;

        var subDependent1 = new SubChild { Name = "1S1" };
        dependent.SubChild1 = subDependent1;
        var subDependent2 = new SubChild { Name = "1S2" };
        dependent.SubChild2 = subDependent2;

        context.ChangeTracker.TrackGraph(principal1, e => e.Entry.State = entityState);
        context.ChangeTracker.TrackGraph(principal2, e => e.Entry.State = entityState);

        Assert.Equal(entityState != EntityState.Unchanged, context.ChangeTracker.HasChanges());

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

            Assert.True(context.ChangeTracker.HasChanges());

            Assert.Equal(5, context.ChangeTracker.Entries().Count());
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
            Assert.Equal(
                typeof(Parent).ShortDisplayName() + "." + nameof(Parent.Child1) + "#" + nameof(Child),
                dependentEntry2.Metadata.DisplayName());

            Assert.Same(subDependent1, dependent.SubChild1);
            Assert.Same(dependent, subDependent1.Parent);
            var subDependentEntry1 = dependentEntry2.Reference(p => p.SubChild1).TargetEntry!;
            Assert.Equal(principal2.Id, subDependentEntry1.Property("ParentId").CurrentValue);
            Assert.Equal(EntityState.Added, subDependentEntry1.State);
            Assert.Equal(
                typeof(Parent).ShortDisplayName()
                + "."
                + nameof(Parent.Child1)
                + "#"
                + nameof(Child)
                + "."
                + nameof(Child.SubChild1)
                + "#"
                + nameof(SubChild),
                subDependentEntry1.Metadata.DisplayName());

            Assert.Same(subDependent2, dependent.SubChild2);
            Assert.Same(dependent, subDependent1.Parent);
            var subDependentEntry2 = dependentEntry2.Reference(p => p.SubChild2).TargetEntry!;
            Assert.Equal(principal2.Id, subDependentEntry2.Property("ParentId").CurrentValue);
            Assert.Equal(EntityState.Added, subDependentEntry2.State);
            Assert.Equal(
                typeof(Parent).ShortDisplayName()
                + "."
                + nameof(Parent.Child1)
                + "#"
                + nameof(Child)
                + "."
                + nameof(Child.SubChild2)
                + "#"
                + nameof(SubChild),
                subDependentEntry2.Metadata.DisplayName());

            context.ChangeTracker.CascadeChanges();

            Assert.True(context.ChangeTracker.HasChanges());

            Assert.Equal(5, context.ChangeTracker.Entries().Count());

            context.ChangeTracker.AcceptAllChanges();

            Assert.False(context.ChangeTracker.HasChanges());

            Assert.Equal(5, context.ChangeTracker.Entries().Count());
            Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
            Assert.Null(principal1.Child1);
            Assert.Null(principal1.Child2);
            Assert.Same(dependent, principal2.Child1);
            Assert.Null(principal2.Child2);
            Assert.Same(subDependent1, dependent.SubChild1);
            Assert.Same(dependent, subDependent1.Parent);
            Assert.Same(subDependent2, dependent.SubChild2);
            Assert.Same(dependent, subDependent2.Parent);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added, CollectionType.HashSet)]
    [InlineData(EntityState.Modified, CollectionType.HashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.HashSet)]
    [InlineData(EntityState.Added, CollectionType.List)]
    [InlineData(EntityState.Modified, CollectionType.List)]
    [InlineData(EntityState.Unchanged, CollectionType.List)]
    [InlineData(EntityState.Added, CollectionType.SortedSet)]
    [InlineData(EntityState.Modified, CollectionType.SortedSet)]
    [InlineData(EntityState.Unchanged, CollectionType.SortedSet)]
    [InlineData(EntityState.Added, CollectionType.Collection)]
    [InlineData(EntityState.Modified, CollectionType.Collection)]
    [InlineData(EntityState.Unchanged, CollectionType.Collection)]
    [InlineData(EntityState.Added, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Modified, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Added, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Modified, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableHashSet)]
    public void Parent_changed_unidirectional_collection(EntityState entityState, CollectionType collectionType)
    {
        using var context = new FixupContext();
        var principal1 = new ParentPN { Id = 77 };

        var principal2 = new ParentPN { Id = 78 };

        var dependent = new ChildPN { Name = "1" };
        principal1.ChildCollection1 = CreateChildCollection(collectionType, dependent);

        var subDependent = new SubChildPN { Name = "1S" };
        dependent.SubChildCollection = CreateChildCollection(collectionType, subDependent);

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

        Assert.Equal(entityState != EntityState.Unchanged, context.ChangeTracker.HasChanges());

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

            Assert.True(context.ChangeTracker.HasChanges());

            Assert.Equal(4, context.ChangeTracker.Entries().Count());
            Assert.Null(principal1.ChildCollection1);
            Assert.Null(principal1.ChildCollection2);
            Assert.Contains(principal2.ChildCollection1, e => ReferenceEquals(e, dependent));
            Assert.Null(principal2.ChildCollection2);
            Assert.Equal(entityState, context.Entry(principal1).State);
            Assert.Equal(entityState, context.Entry(principal2).State);
            Assert.Equal(EntityState.Detached, dependentEntry1.State);

            var dependentEntry2 = context.Entry(principal2).Collection(p => p.ChildCollection1)
                .FindEntry(dependent);
            Assert.Equal(principal2.Id, dependentEntry2.Property("ParentId").CurrentValue);
            Assert.Equal(EntityState.Added, dependentEntry2.State);
            Assert.Equal(
                typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.ChildCollection1) + "#" + nameof(ChildPN),
                dependentEntry2.Metadata.DisplayName());

            Assert.Contains(dependent.SubChildCollection, e => ReferenceEquals(e, subDependent));
            var subDependentEntry2 = dependentEntry2.Collection(p => p.SubChildCollection)
                .FindEntry(subDependent);
            Assert.Equal(principal2.Id, subDependentEntry2.Property("ParentId").CurrentValue);
            Assert.Equal(EntityState.Added, subDependentEntry2.State);
            Assert.Equal(
                typeof(ParentPN).ShortDisplayName()
                + "."
                + nameof(ParentPN.ChildCollection1)
                + "#"
                + nameof(ChildPN)
                + "."
                + nameof(ChildPN.SubChildCollection)
                + "#"
                + nameof(SubChildPN),
                subDependentEntry2.Metadata.DisplayName());

            context.ChangeTracker.CascadeChanges();

            Assert.True(context.ChangeTracker.HasChanges());

            Assert.Equal(4, context.ChangeTracker.Entries().Count());

            context.ChangeTracker.AcceptAllChanges();

            Assert.False(context.ChangeTracker.HasChanges());

            Assert.Equal(4, context.ChangeTracker.Entries().Count());
            Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
            Assert.Null(principal1.ChildCollection1);
            Assert.Null(principal1.ChildCollection2);
            Assert.Contains(principal2.ChildCollection1, e => ReferenceEquals(e, dependent));
            Assert.Null(principal2.ChildCollection2);
            Assert.Contains(dependent.SubChildCollection, e => ReferenceEquals(e, subDependent));
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added, CollectionType.HashSet)]
    [InlineData(EntityState.Modified, CollectionType.HashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.HashSet)]
    [InlineData(EntityState.Added, CollectionType.List)]
    [InlineData(EntityState.Modified, CollectionType.List)]
    [InlineData(EntityState.Unchanged, CollectionType.List)]
    [InlineData(EntityState.Added, CollectionType.SortedSet)]
    [InlineData(EntityState.Modified, CollectionType.SortedSet)]
    [InlineData(EntityState.Unchanged, CollectionType.SortedSet)]
    [InlineData(EntityState.Added, CollectionType.Collection)]
    [InlineData(EntityState.Modified, CollectionType.Collection)]
    [InlineData(EntityState.Unchanged, CollectionType.Collection)]
    [InlineData(EntityState.Added, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Modified, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Added, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Modified, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableHashSet)]
    public void Parent_changed_bidirectional_collection(EntityState entityState, CollectionType collectionType)
    {
        using var context = new FixupContext();
        var principal1 = new Parent { Id = 77 };

        var principal2 = new Parent { Id = 78 };

        var dependent = new Child { Name = "1" };
        principal1.ChildCollection1 = CreateChildCollection(collectionType, dependent);

        var subDependent = new SubChild { Name = "1S" };
        dependent.SubChildCollection = CreateChildCollection(collectionType, subDependent);

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

        Assert.Equal(entityState != EntityState.Unchanged, context.ChangeTracker.HasChanges());

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

            Assert.True(context.ChangeTracker.HasChanges());

            Assert.Equal(4, context.ChangeTracker.Entries().Count());
            Assert.Empty(principal1.ChildCollection1);
            Assert.Null(principal1.ChildCollection2);
            Assert.Contains(principal2.ChildCollection1, e => ReferenceEquals(e, dependent));
            Assert.Null(principal2.ChildCollection2);
            Assert.Same(principal2, dependent.Parent);
            Assert.Equal(entityState, context.Entry(principal1).State);
            Assert.Equal(entityState, context.Entry(principal2).State);
            Assert.Equal(EntityState.Detached, dependentEntry1.State);

            var dependentEntry2 = context.Entry(principal2).Collection(p => p.ChildCollection1)
                .FindEntry(dependent);
            Assert.Equal(EntityState.Added, dependentEntry2.State);
            Assert.Equal(principal2.Id, dependentEntry2.Property("ParentId").CurrentValue);
            Assert.Equal(
                typeof(Parent).ShortDisplayName() + "." + nameof(Parent.ChildCollection1) + "#" + nameof(Child),
                dependentEntry2.Metadata.DisplayName());

            Assert.Contains(dependent.SubChildCollection, e => ReferenceEquals(e, subDependent));
            Assert.Same(dependent, subDependent.Parent);
            var subDependentEntry2 = dependentEntry2.Collection(p => p.SubChildCollection)
                .FindEntry(subDependent);
            Assert.Equal(principal2.Id, subDependentEntry2.Property("ParentId").CurrentValue);
            Assert.Equal(EntityState.Added, subDependentEntry2.State);
            Assert.Equal(
                typeof(Parent).ShortDisplayName()
                + "."
                + nameof(Parent.ChildCollection1)
                + "#"
                + nameof(Child)
                + "."
                + nameof(Child.SubChildCollection)
                + "#"
                + nameof(SubChild), subDependentEntry2.Metadata.DisplayName());

            context.ChangeTracker.CascadeChanges();

            Assert.True(context.ChangeTracker.HasChanges());

            Assert.Equal(4, context.ChangeTracker.Entries().Count());

            context.ChangeTracker.AcceptAllChanges();

            Assert.False(context.ChangeTracker.HasChanges());

            Assert.Equal(4, context.ChangeTracker.Entries().Count());
            Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
            Assert.Empty(principal1.ChildCollection1);
            Assert.Null(principal1.ChildCollection2);
            Assert.Contains(principal2.ChildCollection1, e => ReferenceEquals(e, dependent));
            Assert.Null(principal2.ChildCollection2);
            Assert.Contains(dependent.SubChildCollection, e => ReferenceEquals(e, subDependent));
            Assert.Same(dependent, subDependent.Parent);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Parent_swapped_unidirectional(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal1 = new ParentPN { Id = 77 };

        var principal2 = new ParentPN { Id = 78 };

        var dependent1 = new ChildPN { Name = "1" };
        principal1.Child1 = dependent1;

        var subDependent1 = new SubChildPN { Name = "1S" };
        dependent1.SubChild = subDependent1;

        var dependent2 = new ChildPN { Name = "2" };
        principal2.Child1 = dependent2;

        var subDependent2 = new SubChildPN { Name = "2S" };
        dependent2.SubChild = subDependent2;

        context.ChangeTracker.TrackGraph(principal1, e => e.Entry.State = entityState);
        context.ChangeTracker.TrackGraph(principal2, e => e.Entry.State = entityState);

        Assert.Equal(entityState != EntityState.Unchanged, context.ChangeTracker.HasChanges());

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

            Assert.True(context.ChangeTracker.HasChanges());

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
            Assert.Equal(
                typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.Child1) + "#" + nameof(ChildPN),
                dependent1Entry.Metadata.DisplayName());

            var dependent2Entry = context.Entry(principal2).Reference(p => p.Child1).TargetEntry;
            Assert.Equal(principal2.Id, dependent2Entry.Property("ParentId").CurrentValue);
            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, dependent2Entry.State);
            Assert.Equal(
                typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.Child1) + "#" + nameof(ChildPN),
                dependent2Entry.Metadata.DisplayName());

            Assert.Same(subDependent1, dependent1.SubChild);
            var subDependentEntry1 = dependent1Entry.Reference(p => p.SubChild).TargetEntry;
            Assert.Equal(principal1.Id, subDependentEntry1.Property("ParentId").CurrentValue);
            Assert.Equal(EntityState.Added, subDependentEntry1.State);
            Assert.Equal(
                typeof(ParentPN).ShortDisplayName()
                + "."
                + nameof(ParentPN.Child1)
                + "#"
                + nameof(ChildPN)
                + "."
                + nameof(ChildPN.SubChild)
                + "#"
                + nameof(SubChildPN),
                subDependentEntry1.Metadata.DisplayName());

            Assert.Same(subDependent2, dependent2.SubChild);
            var subDependentEntry2 = dependent2Entry.Reference(p => p.SubChild).TargetEntry;
            Assert.Equal(principal2.Id, subDependentEntry2.Property("ParentId").CurrentValue);
            Assert.Equal(EntityState.Added, subDependentEntry2.State);
            Assert.Equal(
                typeof(ParentPN).ShortDisplayName()
                + "."
                + nameof(ParentPN.Child1)
                + "#"
                + nameof(ChildPN)
                + "."
                + nameof(ChildPN.SubChild)
                + "#"
                + nameof(SubChildPN),
                subDependentEntry2.Metadata.DisplayName());

            context.ChangeTracker.CascadeChanges();

            Assert.True(context.ChangeTracker.HasChanges());

            Assert.Equal(6, context.ChangeTracker.Entries().Count());

            context.ChangeTracker.AcceptAllChanges();

            Assert.False(context.ChangeTracker.HasChanges());

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

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Parent_swapped_bidirectional(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal1 = new Parent { Id = 77 };

        var principal2 = new Parent { Id = 78 };

        var dependent1 = new Child { Name = "1" };
        principal1.Child1 = dependent1;

        var subDependent11 = new SubChild { Name = "1S11" };
        dependent1.SubChild1 = subDependent11;
        var subDependent21 = new SubChild { Name = "1S21" };
        dependent1.SubChild2 = subDependent21;

        var dependent2 = new Child { Name = "2" };
        principal2.Child1 = dependent2;

        var subDependent12 = new SubChild { Name = "2S12" };
        dependent2.SubChild1 = subDependent12;
        var subDependent22 = new SubChild { Name = "2S22" };
        dependent2.SubChild2 = subDependent22;

        context.ChangeTracker.TrackGraph(principal1, e => e.Entry.State = entityState);
        context.ChangeTracker.TrackGraph(principal2, e => e.Entry.State = entityState);

        Assert.Equal(entityState != EntityState.Unchanged, context.ChangeTracker.HasChanges());

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

            Assert.True(context.ChangeTracker.HasChanges());

            Assert.Equal(8, context.ChangeTracker.Entries().Count());
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
            Assert.Equal(
                typeof(Parent).ShortDisplayName() + "." + nameof(Parent.Child1) + "#" + nameof(Child),
                dependent1Entry.Metadata.DisplayName());

            var dependent2Entry = context.Entry(principal2).Reference(p => p.Child1).TargetEntry;
            Assert.Equal(principal2.Id, dependent2Entry.Property("ParentId").CurrentValue);
            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, dependent2Entry.State);
            Assert.Equal(
                typeof(Parent).ShortDisplayName() + "." + nameof(Parent.Child1) + "#" + nameof(Child),
                dependent2Entry.Metadata.DisplayName());

            Assert.Same(subDependent11, dependent1.SubChild1);
            Assert.Same(dependent1, subDependent11.Parent);
            var subDependentEntry11 = dependent1Entry.Reference(p => p.SubChild1).TargetEntry!;
            Assert.Equal(principal1.Id, subDependentEntry11.Property("ParentId").CurrentValue);
            Assert.Equal(EntityState.Added, subDependentEntry11.State);
            Assert.Equal(
                typeof(Parent).ShortDisplayName()
                + "."
                + nameof(Parent.Child1)
                + "#"
                + nameof(Child)
                + "."
                + nameof(Child.SubChild1)
                + "#"
                + nameof(SubChild),
                subDependentEntry11.Metadata.DisplayName());

            Assert.Same(subDependent21, dependent1.SubChild2);
            Assert.Same(dependent1, subDependent21.Parent);
            var subDependentEntry21 = dependent1Entry.Reference(p => p.SubChild2).TargetEntry!;
            Assert.Equal(principal1.Id, subDependentEntry21.Property("ParentId").CurrentValue);
            Assert.Equal(EntityState.Added, subDependentEntry21.State);
            Assert.Equal(
                typeof(Parent).ShortDisplayName()
                + "."
                + nameof(Parent.Child1)
                + "#"
                + nameof(Child)
                + "."
                + nameof(Child.SubChild2)
                + "#"
                + nameof(SubChild),
                subDependentEntry21.Metadata.DisplayName());

            Assert.Same(subDependent12, dependent2.SubChild1);
            Assert.Same(dependent2, subDependent12.Parent);
            var subDependentEntry12 = dependent2Entry.Reference(p => p.SubChild1).TargetEntry!;
            Assert.Equal(principal2.Id, subDependentEntry12.Property("ParentId").CurrentValue);
            Assert.Equal(EntityState.Added, subDependentEntry12.State);
            Assert.Equal(
                typeof(Parent).ShortDisplayName()
                + "."
                + nameof(Parent.Child1)
                + "#"
                + nameof(Child)
                + "."
                + nameof(Child.SubChild1)
                + "#"
                + nameof(SubChild),
                subDependentEntry12.Metadata.DisplayName());

            Assert.Same(subDependent22, dependent2.SubChild2);
            Assert.Same(dependent2, subDependent22.Parent);
            var subDependentEntry22 = dependent2Entry.Reference(p => p.SubChild2).TargetEntry!;
            Assert.Equal(principal2.Id, subDependentEntry22.Property("ParentId").CurrentValue);
            Assert.Equal(EntityState.Added, subDependentEntry22.State);
            Assert.Equal(
                typeof(Parent).ShortDisplayName()
                + "."
                + nameof(Parent.Child1)
                + "#"
                + nameof(Child)
                + "."
                + nameof(Child.SubChild2)
                + "#"
                + nameof(SubChild),
                subDependentEntry22.Metadata.DisplayName());

            context.ChangeTracker.CascadeChanges();

            Assert.True(context.ChangeTracker.HasChanges());

            Assert.Equal(8, context.ChangeTracker.Entries().Count());

            context.ChangeTracker.AcceptAllChanges();

            Assert.False(context.ChangeTracker.HasChanges());

            Assert.Equal(8, context.ChangeTracker.Entries().Count());
            Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
            Assert.Same(dependent2, principal1.Child1);
            Assert.Null(principal1.Child2);
            Assert.Same(dependent1, principal2.Child1);
            Assert.Null(principal2.Child2);
            Assert.Same(subDependent11, dependent1.SubChild1);
            Assert.Same(subDependent12, dependent2.SubChild1);
            Assert.Same(dependent1, subDependent11.Parent);
            Assert.Same(dependent2, subDependent12.Parent);
            Assert.Same(subDependent21, dependent1.SubChild2);
            Assert.Same(subDependent22, dependent2.SubChild2);
            Assert.Same(dependent1, subDependent21.Parent);
            Assert.Same(dependent2, subDependent22.Parent);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added, CollectionType.HashSet)]
    [InlineData(EntityState.Modified, CollectionType.HashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.HashSet)]
    [InlineData(EntityState.Added, CollectionType.List)]
    [InlineData(EntityState.Modified, CollectionType.List)]
    [InlineData(EntityState.Unchanged, CollectionType.List)]
    [InlineData(EntityState.Added, CollectionType.SortedSet)]
    [InlineData(EntityState.Modified, CollectionType.SortedSet)]
    [InlineData(EntityState.Unchanged, CollectionType.SortedSet)]
    [InlineData(EntityState.Added, CollectionType.Collection)]
    [InlineData(EntityState.Modified, CollectionType.Collection)]
    [InlineData(EntityState.Unchanged, CollectionType.Collection)]
    [InlineData(EntityState.Added, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Modified, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Added, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Modified, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableHashSet)]
    public void Parent_swapped_unidirectional_collection(EntityState entityState, CollectionType collectionType)
    {
        using var context = new FixupContext();
        var principal1 = new ParentPN { Id = 77 };

        var principal2 = new ParentPN { Id = 78 };

        var dependent1 = new ChildPN { Name = "1" };
        principal1.ChildCollection1 = CreateChildCollection(collectionType, dependent1);

        var subDependent1 = new SubChildPN { Name = "1S" };
        dependent1.SubChildCollection = CreateChildCollection(collectionType, subDependent1);

        var dependent2 = new ChildPN { Name = "2" };
        principal2.ChildCollection1 = CreateChildCollection(collectionType, dependent2);

        var subDependent2 = new SubChildPN { Name = "2S" };
        dependent2.SubChildCollection = CreateChildCollection(collectionType, subDependent2);

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

        Assert.Equal(entityState != EntityState.Unchanged, context.ChangeTracker.HasChanges());

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

            Assert.True(context.ChangeTracker.HasChanges());

            Assert.Equal(6, context.ChangeTracker.Entries().Count());
            Assert.Contains(principal1.ChildCollection1, e => ReferenceEquals(e, dependent2));
            Assert.Null(principal1.Child1);
            Assert.Contains(principal2.ChildCollection1, e => ReferenceEquals(e, dependent1));
            Assert.Null(principal2.Child1);
            Assert.Equal(entityState, context.Entry(principal1).State);
            Assert.Equal(entityState, context.Entry(principal2).State);

            var newDependentEntry2 = context.Entry(principal1).Collection(p => p.ChildCollection1)
                .FindEntry(dependent2);
            Assert.Equal(principal1.Id, newDependentEntry2.Property("ParentId").CurrentValue);
            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, newDependentEntry2.State);
            Assert.Equal(
                typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.ChildCollection1) + "#" + nameof(ChildPN),
                newDependentEntry2.Metadata.DisplayName());

            var newDependentEntry1 = context.Entry(principal2).Collection(p => p.ChildCollection1)
                .FindEntry(dependent1);
            Assert.Equal(principal2.Id, newDependentEntry1.Property("ParentId").CurrentValue);
            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, newDependentEntry1.State);
            Assert.Equal(
                typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.ChildCollection1) + "#" + nameof(ChildPN),
                newDependentEntry1.Metadata.DisplayName());

            Assert.Contains(dependent1.SubChildCollection, e => ReferenceEquals(e, subDependent1));
            var newSubDependentEntry1 = newDependentEntry1.Collection(p => p.SubChildCollection)
                .FindEntry(subDependent1);
            Assert.Equal(principal2.Id, newSubDependentEntry1.Property("ParentId").CurrentValue);
            Assert.Equal(EntityState.Added, newSubDependentEntry1.State);
            Assert.Equal(
                typeof(ParentPN).ShortDisplayName()
                + "."
                + nameof(ParentPN.ChildCollection1)
                + "#"
                + nameof(ChildPN)
                + "."
                + nameof(ChildPN.SubChildCollection)
                + "#"
                + nameof(SubChildPN), newSubDependentEntry1.Metadata.DisplayName());

            Assert.Contains(dependent2.SubChildCollection, e => ReferenceEquals(e, subDependent2));
            var newSubDependentEntry2 = newDependentEntry2.Collection(p => p.SubChildCollection)
                .FindEntry(subDependent2);
            Assert.Equal(principal1.Id, newSubDependentEntry2.Property("ParentId").CurrentValue);
            Assert.Equal(EntityState.Added, newSubDependentEntry2.State);
            Assert.Equal(
                typeof(ParentPN).ShortDisplayName()
                + "."
                + nameof(ParentPN.ChildCollection1)
                + "#"
                + nameof(ChildPN)
                + "."
                + nameof(ChildPN.SubChildCollection)
                + "#"
                + nameof(SubChildPN), newSubDependentEntry2.Metadata.DisplayName());

            context.ChangeTracker.CascadeChanges();

            Assert.True(context.ChangeTracker.HasChanges());

            Assert.Equal(6, context.ChangeTracker.Entries().Count());

            context.ChangeTracker.AcceptAllChanges();

            Assert.False(context.ChangeTracker.HasChanges());

            Assert.Equal(6, context.ChangeTracker.Entries().Count());
            Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
            Assert.Contains(principal1.ChildCollection1, e => ReferenceEquals(e, dependent2));
            Assert.Null(principal1.Child1);
            Assert.Contains(principal2.ChildCollection1, e => ReferenceEquals(e, dependent1));
            Assert.Null(principal2.Child1);
            Assert.Contains(dependent1.SubChildCollection, e => ReferenceEquals(e, subDependent1));
            Assert.Contains(dependent2.SubChildCollection, e => ReferenceEquals(e, subDependent2));
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added, CollectionType.HashSet)]
    [InlineData(EntityState.Modified, CollectionType.HashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.HashSet)]
    [InlineData(EntityState.Added, CollectionType.List)]
    [InlineData(EntityState.Modified, CollectionType.List)]
    [InlineData(EntityState.Unchanged, CollectionType.List)]
    [InlineData(EntityState.Added, CollectionType.SortedSet)]
    [InlineData(EntityState.Modified, CollectionType.SortedSet)]
    [InlineData(EntityState.Unchanged, CollectionType.SortedSet)]
    [InlineData(EntityState.Added, CollectionType.Collection)]
    [InlineData(EntityState.Modified, CollectionType.Collection)]
    [InlineData(EntityState.Unchanged, CollectionType.Collection)]
    [InlineData(EntityState.Added, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Modified, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Added, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Modified, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableHashSet)]
    public void Parent_swapped_bidirectional_collection(EntityState entityState, CollectionType collectionType)
    {
        using var context = new FixupContext();
        var principal1 = new Parent { Id = 77 };

        var principal2 = new Parent { Id = 78 };

        var dependent1 = new Child { Name = "1" };
        principal1.ChildCollection1 = CreateChildCollection(collectionType, dependent1);

        var subDependent1 = new SubChild { Name = "1S" };
        dependent1.SubChildCollection = CreateChildCollection(collectionType, subDependent1);

        var dependent2 = new Child { Name = "2" };
        principal2.ChildCollection1 = CreateChildCollection(collectionType, dependent2);

        var subDependent2 = new SubChild { Name = "2S" };
        dependent2.SubChildCollection = CreateChildCollection(collectionType, subDependent2);

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

        Assert.Equal(entityState != EntityState.Unchanged, context.ChangeTracker.HasChanges());

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

            Assert.True(context.ChangeTracker.HasChanges());

            Assert.Equal(6, context.ChangeTracker.Entries().Count());
            Assert.Contains(principal1.ChildCollection1, e => ReferenceEquals(e, dependent2));
            Assert.Null(principal1.Child1);
            Assert.Contains(principal2.ChildCollection1, e => ReferenceEquals(e, dependent1));
            Assert.Null(principal2.Child1);
            Assert.Same(principal2, dependent1.Parent);
            Assert.Same(principal1, dependent2.Parent);
            Assert.Equal(entityState, context.Entry(principal1).State);
            Assert.Equal(entityState, context.Entry(principal2).State);

            var newDependentEntry2 = context.Entry(principal1).Collection(p => p.ChildCollection1)
                .FindEntry(dependent2);
            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, newDependentEntry2.State);
            Assert.Equal(principal1.Id, newDependentEntry2.Property("ParentId").CurrentValue);
            Assert.Equal(
                typeof(Parent).ShortDisplayName() + "." + nameof(Parent.ChildCollection1) + "#" + nameof(Child),
                newDependentEntry2.Metadata.DisplayName());

            var newDependentEntry1 = context.Entry(principal2).Collection(p => p.ChildCollection1)
                .FindEntry(dependent1);
            Assert.Equal(principal2.Id, newDependentEntry1.Property("ParentId").CurrentValue);
            Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, newDependentEntry1.State);
            Assert.Equal(
                typeof(Parent).ShortDisplayName() + "." + nameof(Parent.ChildCollection1) + "#" + nameof(Child),
                newDependentEntry1.Metadata.DisplayName());

            Assert.Contains(dependent1.SubChildCollection, e => ReferenceEquals(e, subDependent1));
            Assert.Same(dependent1, subDependent1.Parent);
            var newSubDependentEntry1 = newDependentEntry1.Collection(p => p.SubChildCollection)
                .FindEntry(subDependent1);
            Assert.Equal(principal2.Id, newSubDependentEntry1.Property("ParentId").CurrentValue);
            Assert.Equal(EntityState.Added, newSubDependentEntry1.State);
            Assert.Equal(
                typeof(Parent).ShortDisplayName()
                + "."
                + nameof(Parent.ChildCollection1)
                + "#"
                + nameof(Child)
                + "."
                + nameof(Child.SubChildCollection)
                + "#"
                + nameof(SubChild),
                newSubDependentEntry1.Metadata.DisplayName());

            Assert.Contains(dependent2.SubChildCollection, e => ReferenceEquals(e, subDependent2));
            Assert.Same(dependent2, subDependent2.Parent);
            var newSubDependentEntry2 = newDependentEntry2.Collection(p => p.SubChildCollection)
                .FindEntry(subDependent2);
            Assert.Equal(principal1.Id, newSubDependentEntry2.Property("ParentId").CurrentValue);
            Assert.Equal(EntityState.Added, newSubDependentEntry2.State);
            Assert.Equal(
                typeof(Parent).ShortDisplayName()
                + "."
                + nameof(Parent.ChildCollection1)
                + "#"
                + nameof(Child)
                + "."
                + nameof(Child.SubChildCollection)
                + "#"
                + nameof(SubChild),
                newSubDependentEntry2.Metadata.DisplayName());

            context.ChangeTracker.CascadeChanges();

            Assert.True(context.ChangeTracker.HasChanges());

            Assert.Equal(6, context.ChangeTracker.Entries().Count());

            context.ChangeTracker.AcceptAllChanges();

            Assert.False(context.ChangeTracker.HasChanges());

            Assert.Equal(6, context.ChangeTracker.Entries().Count());
            Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
            Assert.Contains(principal1.ChildCollection1, e => ReferenceEquals(e, dependent2));
            Assert.Null(principal1.Child1);
            Assert.Contains(principal2.ChildCollection1, e => ReferenceEquals(e, dependent1));
            Assert.Null(principal2.Child1);
            Assert.Contains(dependent1.SubChildCollection, e => ReferenceEquals(e, subDependent1));
            Assert.Contains(dependent2.SubChildCollection, e => ReferenceEquals(e, subDependent2));
            Assert.Same(dependent1, subDependent1.Parent);
            Assert.Same(dependent2, subDependent2.Parent);
        }
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Parent_and_identity_changed_unidirectional(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal1 = new ParentPN { Id = 77 };

        var principal2 = new ParentPN { Id = 78 };

        var dependent = new ChildPN { Name = "1" };
        principal1.Child2 = dependent;

        var subDependent = new SubChildPN { Name = "1S" };
        dependent.SubChild = subDependent;

        context.ChangeTracker.TrackGraph(principal1, e => e.Entry.State = entityState);
        context.ChangeTracker.TrackGraph(principal2, e => e.Entry.State = entityState);

        Assert.Equal(entityState != EntityState.Unchanged, context.ChangeTracker.HasChanges());

        var dependentEntry1 = context.Entry(principal1).Reference(p => p.Child2).TargetEntry;

        principal2.Child1 = dependent;
        principal1.Child2 = null;

        context.ChangeTracker.DetectChanges();

        Assert.True(context.ChangeTracker.HasChanges());

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
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.Child1) + "#" + nameof(ChildPN),
            dependentEntry2.Metadata.DisplayName());

        Assert.Same(subDependent, dependent.SubChild);
        var subDependentEntry = dependentEntry2.Reference(p => p.SubChild).TargetEntry;
        Assert.Equal(principal2.Id, subDependentEntry.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName()
            + "."
            + nameof(ParentPN.Child1)
            + "#"
            + nameof(ChildPN)
            + "."
            + nameof(ChildPN.SubChild)
            + "#"
            + nameof(SubChildPN),
            subDependentEntry.Metadata.DisplayName());

        context.ChangeTracker.CascadeChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 4 : 6, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.AcceptAllChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(4, context.ChangeTracker.Entries().Count());
        Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
        Assert.Null(principal1.Child1);
        Assert.Null(principal1.Child2);
        Assert.Same(dependent, principal2.Child1);
        Assert.Null(principal2.Child2);
        Assert.Same(subDependent, dependent.SubChild);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Parent_and_identity_changed_bidirectional(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal1 = new Parent { Id = 77 };

        var principal2 = new Parent { Id = 78 };

        var dependent = new Child { Name = "1" };
        principal1.Child2 = dependent;

        var subDependent1 = new SubChild { Name = "1S1" };
        dependent.SubChild1 = subDependent1;

        var subDependent2 = new SubChild { Name = "1S2" };
        dependent.SubChild2 = subDependent2;

        context.ChangeTracker.TrackGraph(principal1, e => e.Entry.State = entityState);
        context.ChangeTracker.TrackGraph(principal2, e => e.Entry.State = entityState);

        Assert.Equal(entityState != EntityState.Unchanged, context.ChangeTracker.HasChanges());

        var dependentEntry1 = context.Entry(principal1).Reference(p => p.Child2).TargetEntry;

        principal2.Child1 = dependent;
        principal1.Child2 = null;

        if (entityState != EntityState.Added)
        {
            Assert.Equal(
                CoreStrings.KeyReadOnly("ParentId", "Parent.Child2#Child"),
                Assert.Throws<InvalidOperationException>(() => context.ChangeTracker.DetectChanges()).Message);
            return;
        }

        context.ChangeTracker.DetectChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(5, context.ChangeTracker.Entries().Count());
        Assert.Null(principal1.Child1);
        Assert.Null(principal1.Child2);
        Assert.Same(dependent, principal2.Child1);
        Assert.Null(principal2.Child2);
        Assert.Same(principal2, dependent.Parent);
        Assert.Equal(entityState, context.Entry(principal1).State);
        Assert.Equal(entityState, context.Entry(principal2).State);
        Assert.Equal(EntityState.Detached, dependentEntry1.State);
        var dependentEntry2 = context.Entry(principal2).Reference(p => p.Child1).TargetEntry;
        Assert.Equal(principal2.Id, dependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, dependentEntry2.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName() + "." + nameof(Parent.Child1) + "#" + nameof(Child),
            dependentEntry2.Metadata.DisplayName());

        Assert.Same(subDependent1, dependent.SubChild1);
        Assert.Same(dependent, subDependent1.Parent);
        var subDependentEntry1 = dependentEntry2.Reference(p => p.SubChild1).TargetEntry!;
        Assert.Equal(principal2.Id, subDependentEntry1.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry1.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName()
            + "."
            + nameof(Parent.Child1)
            + "#"
            + nameof(Child)
            + "."
            + nameof(Child.SubChild1)
            + "#"
            + nameof(SubChild),
            subDependentEntry1.Metadata.DisplayName());

        Assert.Same(subDependent2, dependent.SubChild2);
        Assert.Same(dependent, subDependent1.Parent);
        var subDependentEntry2 = dependentEntry2.Reference(p => p.SubChild2).TargetEntry!;
        Assert.Equal(principal2.Id, subDependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry2.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName()
            + "."
            + nameof(Parent.Child1)
            + "#"
            + nameof(Child)
            + "."
            + nameof(Child.SubChild2)
            + "#"
            + nameof(SubChild),
            subDependentEntry2.Metadata.DisplayName());

        context.ChangeTracker.CascadeChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(5, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.AcceptAllChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(5, context.ChangeTracker.Entries().Count());
        Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
        Assert.Null(principal1.Child1);
        Assert.Null(principal1.Child2);
        Assert.Same(dependent, principal2.Child1);
        Assert.Null(principal2.Child2);
        Assert.Same(subDependent1, dependent.SubChild1);
        Assert.Same(dependent, subDependent1.Parent);
        Assert.Same(subDependent2, dependent.SubChild2);
        Assert.Same(dependent, subDependent2.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added, CollectionType.HashSet)]
    [InlineData(EntityState.Modified, CollectionType.HashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.HashSet)]
    [InlineData(EntityState.Added, CollectionType.List)]
    [InlineData(EntityState.Modified, CollectionType.List)]
    [InlineData(EntityState.Unchanged, CollectionType.List)]
    [InlineData(EntityState.Added, CollectionType.SortedSet)]
    [InlineData(EntityState.Modified, CollectionType.SortedSet)]
    [InlineData(EntityState.Unchanged, CollectionType.SortedSet)]
    [InlineData(EntityState.Added, CollectionType.Collection)]
    [InlineData(EntityState.Modified, CollectionType.Collection)]
    [InlineData(EntityState.Unchanged, CollectionType.Collection)]
    [InlineData(EntityState.Added, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Modified, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Added, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Modified, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableHashSet)]
    public void Parent_and_identity_changed_unidirectional_collection(EntityState entityState, CollectionType collectionType)
    {
        using var context = new FixupContext();
        var principal1 = new ParentPN { Id = 77 };

        var principal2 = new ParentPN { Id = 78 };

        var dependent = new ChildPN { Name = "1" };
        principal1.ChildCollection2 = CreateChildCollection(collectionType, dependent);

        var subDependent = new SubChildPN { Name = "1S" };
        dependent.SubChildCollection = CreateChildCollection(collectionType, subDependent);

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

        Assert.Equal(entityState != EntityState.Unchanged, context.ChangeTracker.HasChanges());

        var dependentEntry1 = context.Entry(principal1).Collection(p => p.ChildCollection2).FindEntry(dependent);

        principal2.ChildCollection1 = principal1.ChildCollection2;
        principal1.ChildCollection2 = null;

        context.ChangeTracker.DetectChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 4 : 6, context.ChangeTracker.Entries().Count());
        Assert.Null(principal1.ChildCollection1);
        Assert.Null(principal1.ChildCollection2);
        Assert.Contains(principal2.ChildCollection1, e => ReferenceEquals(e, dependent));
        Assert.Null(principal2.ChildCollection2);
        Assert.Equal(entityState, context.Entry(principal1).State);
        Assert.Equal(entityState, context.Entry(principal2).State);
        Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
        var dependentEntry2 = context.Entry(principal2).Collection(p => p.ChildCollection1).FindEntry(dependent);
        Assert.Equal(principal2.Id, dependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, dependentEntry2.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.ChildCollection1) + "#" + nameof(ChildPN),
            dependentEntry2.Metadata.DisplayName());

        Assert.Contains(dependent.SubChildCollection, e => ReferenceEquals(e, subDependent));
        var subDependentEntry = dependentEntry2.Collection(p => p.SubChildCollection).FindEntry(subDependent);
        Assert.Equal(principal2.Id, subDependentEntry.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName()
            + "."
            + nameof(ParentPN.ChildCollection1)
            + "#"
            + nameof(ChildPN)
            + "."
            + nameof(ChildPN.SubChildCollection)
            + "#"
            + nameof(SubChildPN),
            subDependentEntry.Metadata.DisplayName());

        context.ChangeTracker.CascadeChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 4 : 6, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.AcceptAllChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(4, context.ChangeTracker.Entries().Count());
        Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
        Assert.Null(principal1.ChildCollection1);
        Assert.Null(principal1.ChildCollection2);
        Assert.Contains(principal2.ChildCollection1, e => ReferenceEquals(e, dependent));
        Assert.Null(principal2.ChildCollection2);
        Assert.Contains(dependent.SubChildCollection, e => ReferenceEquals(e, subDependent));
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added, CollectionType.HashSet)]
    [InlineData(EntityState.Modified, CollectionType.HashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.HashSet)]
    [InlineData(EntityState.Added, CollectionType.List)]
    [InlineData(EntityState.Modified, CollectionType.List)]
    [InlineData(EntityState.Unchanged, CollectionType.List)]
    [InlineData(EntityState.Added, CollectionType.SortedSet)]
    [InlineData(EntityState.Modified, CollectionType.SortedSet)]
    [InlineData(EntityState.Unchanged, CollectionType.SortedSet)]
    [InlineData(EntityState.Added, CollectionType.Collection)]
    [InlineData(EntityState.Modified, CollectionType.Collection)]
    [InlineData(EntityState.Unchanged, CollectionType.Collection)]
    [InlineData(EntityState.Added, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Modified, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Added, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Modified, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableHashSet)]
    public void Parent_and_identity_changed_bidirectional_collection(EntityState entityState, CollectionType collectionType)
    {
        using var context = new FixupContext();
        var principal1 = new Parent { Id = 77 };

        var principal2 = new Parent { Id = 78 };

        var dependent = new Child { Name = "1" };
        principal1.ChildCollection2 = CreateChildCollection(collectionType, dependent);

        var subDependent = new SubChild { Name = "1S" };
        dependent.SubChildCollection = CreateChildCollection(collectionType, subDependent);

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

        Assert.Equal(entityState != EntityState.Unchanged, context.ChangeTracker.HasChanges());

        var dependentEntry1 = context.Entry(principal1).Collection(p => p.ChildCollection2).FindEntry(dependent);

        principal2.ChildCollection1 = principal1.ChildCollection2;
        principal1.ChildCollection2 = null;

        if (entityState != EntityState.Added)
        {
            Assert.Equal(
                CoreStrings.KeyReadOnly("ParentId", "Parent.ChildCollection2#Child"),
                Assert.Throws<InvalidOperationException>(() => context.ChangeTracker.DetectChanges()).Message);
            return;
        }

        context.ChangeTracker.DetectChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 4 : 6, context.ChangeTracker.Entries().Count());
        Assert.Empty(principal1.ChildCollection1);
        Assert.Null(principal1.ChildCollection2);
        Assert.Contains(principal2.ChildCollection1, e => ReferenceEquals(e, dependent));
        Assert.Null(principal2.ChildCollection2);
        Assert.Same(principal2, dependent.Parent);
        Assert.Equal(entityState, context.Entry(principal1).State);
        Assert.Equal(entityState, context.Entry(principal2).State);
        Assert.Equal(entityState == EntityState.Added ? EntityState.Detached : EntityState.Deleted, dependentEntry1.State);
        var dependentEntry2 = context.Entry(principal2).Collection(p => p.ChildCollection1).FindEntry(dependent);
        Assert.Equal(principal2.Id, dependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, dependentEntry2.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName() + "." + nameof(Parent.ChildCollection1) + "#" + nameof(Child),
            dependentEntry2.Metadata.DisplayName());

        Assert.Contains(dependent.SubChildCollection, e => ReferenceEquals(e, subDependent));
        Assert.Same(dependent, subDependent.Parent);
        var subDependentEntry = dependentEntry2.Collection(p => p.SubChildCollection).FindEntry(subDependent);
        Assert.Equal(principal2.Id, subDependentEntry.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName()
            + "."
            + nameof(Parent.ChildCollection1)
            + "#"
            + nameof(Child)
            + "."
            + nameof(Child.SubChildCollection)
            + "#"
            + nameof(SubChild), subDependentEntry.Metadata.DisplayName());

        context.ChangeTracker.CascadeChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 4 : 6, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.AcceptAllChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(4, context.ChangeTracker.Entries().Count());
        Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
        Assert.Empty(principal1.ChildCollection1);
        Assert.Null(principal1.ChildCollection2);
        Assert.Contains(principal2.ChildCollection1, e => ReferenceEquals(e, dependent));
        Assert.Null(principal2.ChildCollection2);
        Assert.Contains(dependent.SubChildCollection, e => ReferenceEquals(e, subDependent));
        Assert.Same(dependent, subDependent.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Parent_and_identity_swapped_unidirectional(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal1 = new ParentPN { Id = 77 };

        var principal2 = new ParentPN { Id = 78 };

        var dependent1 = new ChildPN { Name = "1" };
        principal1.Child2 = dependent1;

        var subDependent1 = new SubChildPN { Name = "1S" };
        dependent1.SubChild = subDependent1;

        var dependent2 = new ChildPN { Name = "2" };
        principal2.Child1 = dependent2;

        var subDependent2 = new SubChildPN { Name = "2S" };
        dependent2.SubChild = subDependent2;

        context.ChangeTracker.TrackGraph(principal1, e => e.Entry.State = entityState);
        context.ChangeTracker.TrackGraph(principal2, e => e.Entry.State = entityState);

        Assert.Equal(entityState != EntityState.Unchanged, context.ChangeTracker.HasChanges());

        principal2.Child1 = dependent1;
        principal1.Child2 = dependent2;

        context.ChangeTracker.DetectChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 6 : 10, context.ChangeTracker.Entries().Count());
        Assert.Null(principal1.Child1);
        Assert.Same(dependent2, principal1.Child2);
        Assert.Same(dependent1, principal2.Child1);
        Assert.Null(principal2.Child2);
        Assert.Equal(entityState, context.Entry(principal1).State);
        Assert.Equal(entityState, context.Entry(principal2).State);

        var dependent1Entry = context.Entry(principal1).Reference(p => p.Child2).TargetEntry;
        Assert.Equal(EntityState.Added, dependent1Entry.State);
        Assert.Equal(principal1.Id, dependent1Entry.Property("ParentId").CurrentValue);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.Child2) + "#" + nameof(ChildPN),
            dependent1Entry.Metadata.DisplayName());
        Assert.Equal(
            entityState == EntityState.Added ? null : EntityState.Deleted,
            dependent1Entry.GetInfrastructure().SharedIdentityEntry?.EntityState);

        var dependent2Entry = context.Entry(principal2).Reference(p => p.Child1).TargetEntry;
        Assert.Equal(principal2.Id, dependent2Entry.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, dependent2Entry.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.Child1) + "#" + nameof(ChildPN),
            dependent2Entry.Metadata.DisplayName());
        Assert.Equal(
            entityState == EntityState.Added ? null : EntityState.Deleted,
            dependent2Entry.GetInfrastructure().SharedIdentityEntry?.EntityState);

        Assert.Same(subDependent1, dependent1.SubChild);
        var subDependentEntry1 = dependent1Entry.Reference(p => p.SubChild).TargetEntry;
        Assert.Equal(principal1.Id, subDependentEntry1.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry1.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName()
            + "."
            + nameof(ParentPN.Child2)
            + "#"
            + nameof(ChildPN)
            + "."
            + nameof(ChildPN.SubChild)
            + "#"
            + nameof(SubChildPN), subDependentEntry1.Metadata.DisplayName());

        Assert.Same(subDependent2, dependent2.SubChild);
        var subDependentEntry2 = dependent2Entry.Reference(p => p.SubChild).TargetEntry;
        Assert.Equal(principal2.Id, subDependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry2.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName()
            + "."
            + nameof(ParentPN.Child1)
            + "#"
            + nameof(ChildPN)
            + "."
            + nameof(ChildPN.SubChild)
            + "#"
            + nameof(SubChildPN), subDependentEntry2.Metadata.DisplayName());

        context.ChangeTracker.CascadeChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 6 : 10, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.AcceptAllChanges();

        Assert.False(context.ChangeTracker.HasChanges());

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

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Unchanged)]
    public void Parent_and_identity_swapped_bidirectional(EntityState entityState)
    {
        using var context = new FixupContext();
        var principal1 = new Parent { Id = 77 };

        var principal2 = new Parent { Id = 78 };

        var dependent1 = new Child { Name = "1" };
        principal1.Child2 = dependent1;

        var subDependent1 = new SubChild { Name = "1S" };
        dependent1.SubChild1 = subDependent1;

        var dependent2 = new Child { Name = "2" };
        principal2.Child1 = dependent2;

        var subDependent2 = new SubChild { Name = "2S" };
        dependent2.SubChild1 = subDependent2;

        context.ChangeTracker.TrackGraph(principal1, e => e.Entry.State = entityState);
        context.ChangeTracker.TrackGraph(principal2, e => e.Entry.State = entityState);

        Assert.Equal(entityState != EntityState.Unchanged, context.ChangeTracker.HasChanges());

        principal2.Child1 = dependent1;
        principal1.Child2 = dependent2;

        context.ChangeTracker.DetectChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 6 : 10, context.ChangeTracker.Entries().Count());
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
        Assert.Equal(
            typeof(Parent).ShortDisplayName() + "." + nameof(Parent.Child2) + "#" + nameof(Child),
            dependent1Entry.Metadata.DisplayName());
        Assert.Equal(
            entityState == EntityState.Added ? null : EntityState.Deleted,
            dependent1Entry.GetInfrastructure().SharedIdentityEntry?.EntityState);

        var dependent2Entry = context.Entry(principal2).Reference(p => p.Child1).TargetEntry;
        Assert.Equal(principal2.Id, dependent2Entry.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, dependent2Entry.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName() + "." + nameof(Parent.Child1) + "#" + nameof(Child),
            dependent2Entry.Metadata.DisplayName());
        Assert.Equal(
            entityState == EntityState.Added ? null : EntityState.Deleted,
            dependent1Entry.GetInfrastructure().SharedIdentityEntry?.EntityState);

        Assert.Same(subDependent1, dependent1.SubChild1);
        Assert.Same(dependent1, subDependent1.Parent);
        var subDependentEntry1 = dependent1Entry.Reference(p => p.SubChild1).TargetEntry;
        Assert.Equal(principal1.Id, subDependentEntry1.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry1.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName()
            + "."
            + nameof(Parent.Child2)
            + "#"
            + nameof(Child)
            + "."
            + nameof(Child.SubChild1)
            + "#"
            + nameof(SubChild), subDependentEntry1.Metadata.DisplayName());

        Assert.Same(subDependent2, dependent2.SubChild1);
        Assert.Same(dependent2, subDependent2.Parent);
        var subDependentEntry2 = dependent2Entry.Reference(p => p.SubChild1).TargetEntry;
        Assert.Equal(principal2.Id, subDependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, subDependentEntry2.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName()
            + "."
            + nameof(Parent.Child1)
            + "#"
            + nameof(Child)
            + "."
            + nameof(Child.SubChild1)
            + "#"
            + nameof(SubChild), subDependentEntry2.Metadata.DisplayName());

        context.ChangeTracker.CascadeChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 6 : 10, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.AcceptAllChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(6, context.ChangeTracker.Entries().Count());
        Assert.Null(dependent1Entry.GetInfrastructure().SharedIdentityEntry);
        Assert.Null(dependent2Entry.GetInfrastructure().SharedIdentityEntry);
        Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
        Assert.Null(principal1.Child1);
        Assert.Same(dependent2, principal1.Child2);
        Assert.Same(dependent1, principal2.Child1);
        Assert.Null(principal2.Child2);
        Assert.Same(subDependent1, dependent1.SubChild1);
        Assert.Same(subDependent2, dependent2.SubChild1);
        Assert.Same(dependent1, subDependent1.Parent);
        Assert.Same(dependent2, subDependent2.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added, CollectionType.HashSet)]
    [InlineData(EntityState.Modified, CollectionType.HashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.HashSet)]
    [InlineData(EntityState.Added, CollectionType.List)]
    [InlineData(EntityState.Modified, CollectionType.List)]
    [InlineData(EntityState.Unchanged, CollectionType.List)]
    [InlineData(EntityState.Added, CollectionType.SortedSet)]
    [InlineData(EntityState.Modified, CollectionType.SortedSet)]
    [InlineData(EntityState.Unchanged, CollectionType.SortedSet)]
    [InlineData(EntityState.Added, CollectionType.Collection)]
    [InlineData(EntityState.Modified, CollectionType.Collection)]
    [InlineData(EntityState.Unchanged, CollectionType.Collection)]
    [InlineData(EntityState.Added, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Modified, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Added, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Modified, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableHashSet)]
    public void Parent_and_identity_swapped_unidirectional_collection(EntityState entityState, CollectionType collectionType)
    {
        using var context = new FixupContext();
        var principal1 = new ParentPN { Id = 77 };

        var principal2 = new ParentPN { Id = 78 };

        var dependent1 = new ChildPN { Name = "1" };
        principal1.ChildCollection2 = CreateChildCollection(collectionType, dependent1);

        var subDependent1 = new SubChildPN { Name = "1S" };
        dependent1.SubChildCollection = CreateChildCollection(collectionType, subDependent1);

        var dependent2 = new ChildPN { Name = "2" };
        principal2.ChildCollection1 = CreateChildCollection(collectionType, dependent2);

        var subDependent2 = new SubChildPN { Name = "2S" };
        dependent2.SubChildCollection = CreateChildCollection(collectionType, subDependent2);

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

        Assert.Equal(entityState != EntityState.Unchanged, context.ChangeTracker.HasChanges());

        var dependentEntry1 = context.Entry(dependent1);
        var dependentEntry2 = context.Entry(dependent2);
        var subDependentEntry1 = context.Entry(subDependent1);
        var subDependentEntry2 = context.Entry(subDependent2);

        var tempCollection = principal2.ChildCollection1;
        principal2.ChildCollection1 = principal1.ChildCollection2;
        principal1.ChildCollection2 = tempCollection;

        var newDependentEntry1 = context.Entry(principal2).Collection(p => p.ChildCollection1)
            .FindEntry(dependent1);
        newDependentEntry1.Property<int>("Id").CurrentValue = dependentEntry1.Property<int>("Id").CurrentValue;

        var newDependentEntry2 = context.Entry(principal1).Collection(p => p.ChildCollection2)
            .FindEntry(dependent2);
        newDependentEntry2.Property<int>("Id").CurrentValue = dependentEntry2.Property<int>("Id").CurrentValue;

        var newSubDependentEntry1 = newDependentEntry1.Collection(p => p.SubChildCollection)
            .FindEntry(subDependent1);
        newSubDependentEntry1.Property<int>("Id").CurrentValue = subDependentEntry1.Property<int>("Id").CurrentValue;

        var newSubDependentEntry2 = newDependentEntry2.Collection(p => p.SubChildCollection)
            .FindEntry(subDependent2);
        newSubDependentEntry2.Property<int>("Id").CurrentValue = subDependentEntry2.Property<int>("Id").CurrentValue;

        Assert.Equal(entityState != EntityState.Unchanged, context.ChangeTracker.HasChanges());

        context.ChangeTracker.DetectChanges();

        Assert.Equal(entityState == EntityState.Added ? 6 : 10, context.ChangeTracker.Entries().Count());
        Assert.Null(principal1.ChildCollection1);
        Assert.Contains(principal1.ChildCollection2, e => ReferenceEquals(e, dependent2));
        Assert.Contains(principal2.ChildCollection1, e => ReferenceEquals(e, dependent1));
        Assert.Null(principal2.ChildCollection2);
        Assert.Equal(entityState, context.Entry(principal1).State);
        Assert.Equal(entityState, context.Entry(principal2).State);

        Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, newDependentEntry2.State);
        Assert.Equal(principal1.Id, newDependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.ChildCollection2) + "#" + nameof(ChildPN),
            newDependentEntry2.Metadata.DisplayName());
        Assert.Equal(
            entityState == EntityState.Added ? null : EntityState.Deleted,
            newDependentEntry2.GetInfrastructure().SharedIdentityEntry?.EntityState);

        Assert.Equal(principal2.Id, newDependentEntry1.Property("ParentId").CurrentValue);
        Assert.Equal(entityState == EntityState.Added ? EntityState.Added : EntityState.Modified, newDependentEntry1.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName() + "." + nameof(ParentPN.ChildCollection1) + "#" + nameof(ChildPN),
            newDependentEntry1.Metadata.DisplayName());
        Assert.Equal(
            entityState == EntityState.Added ? null : EntityState.Deleted,
            newDependentEntry1.GetInfrastructure().SharedIdentityEntry?.EntityState);

        Assert.Contains(dependent1.SubChildCollection, e => ReferenceEquals(e, subDependent1));
        Assert.Equal(principal1.Id, newSubDependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, newSubDependentEntry2.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName()
            + "."
            + nameof(ParentPN.ChildCollection2)
            + "#"
            + nameof(ChildPN)
            + "."
            + nameof(ChildPN.SubChildCollection)
            + "#"
            + nameof(SubChildPN),
            newSubDependentEntry2.Metadata.DisplayName());

        Assert.Contains(dependent2.SubChildCollection, e => ReferenceEquals(e, subDependent2));
        Assert.Equal(principal2.Id, newSubDependentEntry1.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, newSubDependentEntry1.State);
        Assert.Equal(
            typeof(ParentPN).ShortDisplayName()
            + "."
            + nameof(ParentPN.ChildCollection1)
            + "#"
            + nameof(ChildPN)
            + "."
            + nameof(ChildPN.SubChildCollection)
            + "#"
            + nameof(SubChildPN),
            newSubDependentEntry1.Metadata.DisplayName());

        context.ChangeTracker.CascadeChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(entityState == EntityState.Added ? 6 : 10, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.AcceptAllChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(6, context.ChangeTracker.Entries().Count());
        Assert.Null(newDependentEntry2.GetInfrastructure().SharedIdentityEntry);
        Assert.Null(newDependentEntry1.GetInfrastructure().SharedIdentityEntry);
        Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
        Assert.Null(principal1.ChildCollection1);
        Assert.Contains(principal1.ChildCollection2, e => ReferenceEquals(e, dependent2));
        Assert.Contains(principal2.ChildCollection1, e => ReferenceEquals(e, dependent1));
        Assert.Null(principal2.ChildCollection2);
        Assert.Contains(dependent1.SubChildCollection, e => ReferenceEquals(e, subDependent1));
        Assert.Contains(dependent2.SubChildCollection, e => ReferenceEquals(e, subDependent2));
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added, CollectionType.HashSet)]
    [InlineData(EntityState.Modified, CollectionType.HashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.HashSet)]
    [InlineData(EntityState.Added, CollectionType.List)]
    [InlineData(EntityState.Modified, CollectionType.List)]
    [InlineData(EntityState.Unchanged, CollectionType.List)]
    [InlineData(EntityState.Added, CollectionType.SortedSet)]
    [InlineData(EntityState.Modified, CollectionType.SortedSet)]
    [InlineData(EntityState.Unchanged, CollectionType.SortedSet)]
    [InlineData(EntityState.Added, CollectionType.Collection)]
    [InlineData(EntityState.Modified, CollectionType.Collection)]
    [InlineData(EntityState.Unchanged, CollectionType.Collection)]
    [InlineData(EntityState.Added, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Modified, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableCollection)]
    [InlineData(EntityState.Added, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Modified, CollectionType.ObservableHashSet)]
    [InlineData(EntityState.Unchanged, CollectionType.ObservableHashSet)]
    public void Parent_and_identity_swapped_bidirectional_collection(EntityState entityState, CollectionType collectionType)
    {
        using var context = new FixupContext();
        var principal1 = new Parent { Id = 77 };

        var principal2 = new Parent { Id = 78 };

        var dependent1 = new Child { Name = "1" };
        principal1.ChildCollection2 = CreateChildCollection(collectionType, dependent1);

        var subDependent1 = new SubChild { Name = "1S" };
        dependent1.SubChildCollection = CreateChildCollection(collectionType, subDependent1);

        var dependent2 = new Child { Name = "2" };
        principal2.ChildCollection1 = CreateChildCollection(collectionType, dependent2);

        var subDependent2 = new SubChild { Name = "2S" };
        dependent2.SubChildCollection = CreateChildCollection(collectionType, subDependent2);

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

        Assert.Equal(entityState != EntityState.Unchanged, context.ChangeTracker.HasChanges());

        var dependentEntry1 = context.Entry(dependent1);
        var dependentEntry2 = context.Entry(dependent2);
        var subDependentEntry1 = context.Entry(subDependent1);
        var subDependentEntry2 = context.Entry(subDependent2);

        var tempCollection = principal2.ChildCollection1;
        principal2.ChildCollection1 = principal1.ChildCollection2;
        principal1.ChildCollection2 = tempCollection;

        var newDependentEntry1 = context.Entry(principal2).Collection(p => p.ChildCollection1)
            .FindEntry(dependent1);
        newDependentEntry1.Property<int>("Id").CurrentValue = dependentEntry1.Property<int>("Id").CurrentValue;

        var newDependentEntry2 = context.Entry(principal1).Collection(p => p.ChildCollection2)
            .FindEntry(dependent2);
        newDependentEntry2.Property<int>("Id").CurrentValue = dependentEntry2.Property<int>("Id").CurrentValue;

        var newSubDependentEntry1 = newDependentEntry1.Collection(p => p.SubChildCollection)
            .FindEntry(subDependent1);
        newSubDependentEntry1.Property<int>("Id").CurrentValue = subDependentEntry1.Property<int>("Id").CurrentValue;

        var newSubDependentEntry2 = newDependentEntry2.Collection(p => p.SubChildCollection)
            .FindEntry(subDependent2);
        newSubDependentEntry2.Property<int>("Id").CurrentValue = subDependentEntry2.Property<int>("Id").CurrentValue;

        if (entityState != EntityState.Added)
        {
            Assert.Equal(
                CoreStrings.KeyReadOnly("ParentId", "Parent.ChildCollection2#Child"),
                Assert.Throws<InvalidOperationException>(() => context.ChangeTracker.DetectChanges()).Message);
            return;
        }

        context.ChangeTracker.DetectChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(6, context.ChangeTracker.Entries().Count());
        Assert.Empty(principal1.ChildCollection1);
        Assert.Contains(principal1.ChildCollection2, e => ReferenceEquals(e, dependent2));
        Assert.Contains(principal2.ChildCollection1, e => ReferenceEquals(e, dependent1));
        Assert.Empty(principal2.ChildCollection2);
        Assert.Same(principal2, dependent1.Parent);
        Assert.Same(principal1, dependent2.Parent);
        Assert.Equal(entityState, context.Entry(principal1).State);
        Assert.Equal(entityState, context.Entry(principal2).State);

        Assert.Equal(EntityState.Added, newDependentEntry2.State);
        Assert.Equal(principal1.Id, newDependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(
            typeof(Parent).ShortDisplayName() + "." + nameof(Parent.ChildCollection2) + "#" + nameof(Child),
            newDependentEntry2.Metadata.DisplayName());
        Assert.Equal(
            entityState == EntityState.Added ? null : EntityState.Deleted,
            newDependentEntry2.GetInfrastructure().SharedIdentityEntry?.EntityState);

        Assert.Equal(principal2.Id, newDependentEntry1.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, newDependentEntry1.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName() + "." + nameof(Parent.ChildCollection1) + "#" + nameof(Child),
            newDependentEntry1.Metadata.DisplayName());
        Assert.Equal(
            entityState == EntityState.Added ? null : EntityState.Deleted,
            newDependentEntry2.GetInfrastructure().SharedIdentityEntry?.EntityState);

        Assert.Contains(dependent1.SubChildCollection, e => ReferenceEquals(e, subDependent1));
        Assert.Same(dependent1, subDependent1.Parent);
        Assert.Equal(principal1.Id, newSubDependentEntry2.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, newSubDependentEntry2.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName()
            + "."
            + nameof(Parent.ChildCollection2)
            + "#"
            + nameof(Child)
            + "."
            + nameof(Child.SubChildCollection)
            + "#"
            + nameof(SubChild), newSubDependentEntry2.Metadata.DisplayName());

        Assert.Contains(dependent2.SubChildCollection, e => ReferenceEquals(e, subDependent2));
        Assert.Same(dependent2, subDependent2.Parent);
        Assert.Equal(principal2.Id, newSubDependentEntry1.Property("ParentId").CurrentValue);
        Assert.Equal(EntityState.Added, newSubDependentEntry1.State);
        Assert.Equal(
            typeof(Parent).ShortDisplayName()
            + "."
            + nameof(Parent.ChildCollection1)
            + "#"
            + nameof(Child)
            + "."
            + nameof(Child.SubChildCollection)
            + "#"
            + nameof(SubChild), newSubDependentEntry1.Metadata.DisplayName());

        context.ChangeTracker.CascadeChanges();

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(6, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.AcceptAllChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(6, context.ChangeTracker.Entries().Count());
        Assert.Null(newDependentEntry2.GetInfrastructure().SharedIdentityEntry);
        Assert.Null(newDependentEntry1.GetInfrastructure().SharedIdentityEntry);
        Assert.True(context.ChangeTracker.Entries().All(e => e.State == EntityState.Unchanged));
        Assert.Empty(principal1.ChildCollection1);
        Assert.Contains(principal1.ChildCollection2, e => ReferenceEquals(e, dependent2));
        Assert.Contains(principal2.ChildCollection1, e => ReferenceEquals(e, dependent1));
        Assert.Empty(principal2.ChildCollection2);
        Assert.Contains(dependent1.SubChildCollection, e => ReferenceEquals(e, subDependent1));
        Assert.Contains(dependent2.SubChildCollection, e => ReferenceEquals(e, subDependent2));
        Assert.Same(dependent1, subDependent1.Parent);
        Assert.Same(dependent2, subDependent2.Parent);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added, true, true, true)]
    [InlineData(EntityState.Modified, true, true, true)]
    [InlineData(EntityState.Unchanged, true, true, true)]
    [InlineData(EntityState.Added, true, true, false)]
    [InlineData(EntityState.Modified, true, true, false)]
    [InlineData(EntityState.Unchanged, true, true, false)]
    [InlineData(EntityState.Added, true, false, true)]
    [InlineData(EntityState.Modified, true, false, true)]
    [InlineData(EntityState.Unchanged, true, false, true)]
    [InlineData(EntityState.Added, true, false, false)]
    [InlineData(EntityState.Modified, true, false, false)]
    [InlineData(EntityState.Unchanged, true, false, false)]
    [InlineData(EntityState.Added, false, true, true)]
    [InlineData(EntityState.Modified, false, true, true)]
    [InlineData(EntityState.Unchanged, false, true, true)]
    [InlineData(EntityState.Added, false, true, false)]
    [InlineData(EntityState.Modified, false, true, false)]
    [InlineData(EntityState.Unchanged, false, true, false)]
    [InlineData(EntityState.Added, false, false, true)]
    [InlineData(EntityState.Modified, false, false, true)]
    [InlineData(EntityState.Unchanged, false, false, true)]
    [InlineData(EntityState.Added, false, false, false)]
    [InlineData(EntityState.Modified, false, false, false)]
    [InlineData(EntityState.Unchanged, false, false, false)]
    public void Can_set_nested_dependent_to_null(EntityState entityState, bool null1, bool null2, bool nullC)
    {
        using var context = new FixupContext();

        var subChild1 = new SubChild { Name = "1S1" };
        var subChild2 = new SubChild { Name = "1S2" };
        var subChild3 = new SubChild { Name = "1S3" };
        var subChildCollection = new List<SubChild> { subChild3 };
        var child = new Child
        {
            Name = "1",
            SubChild1 = subChild1,
            SubChild2 = subChild2,
            SubChildCollection = subChildCollection
        };
        var principal = new Parent { Id = 77, Child1 = child };

        _ = entityState switch
        {
            EntityState.Added => context.Add(principal),
            EntityState.Unchanged => context.Attach(principal),
            EntityState.Modified => context.Update(principal),
            _ => throw new ArgumentOutOfRangeException()
        };

        var newSubChild1 = new SubChild { Name = "n1S1" };
        var newSubChild2 = new SubChild { Name = "n1S2" };
        var newSubChild3 = new SubChild { Name = "n1S3" };
        var newSubChildCollection = new List<SubChild> { newSubChild3 };
        var newChild = new Child
        {
            Name = "n1",
            SubChild1 = null1 ? null : newSubChild1,
            SubChild2 = null2 ? null : newSubChild2,
            SubChildCollection = nullC ? null : newSubChildCollection
        };

        principal.Child1 = newChild;

        context.ChangeTracker.DetectChanges();
        AssertChildren();

        context.ChangeTracker.DetectChanges();
        AssertChildren();

        void AssertChildren()
        {
            Assert.Equal("n1", newChild.Name);
            Assert.Equal("n1S1", newSubChild1.Name);
            Assert.Equal("n1S2", newSubChild2.Name);
            Assert.Equal("n1S3", newSubChild3.Name);
            Assert.Single(newSubChildCollection);

            Assert.Same(newChild, principal.Child1);
            Assert.Same(null1 ? null : newSubChild1, newChild.SubChild1);
            Assert.Same(null2 ? null : newSubChild2, newChild.SubChild2);
            Assert.Same(nullC ? null : newSubChildCollection, newChild.SubChildCollection);
            Assert.Same(nullC ? null : newSubChildCollection.Single(), newChild.SubChildCollection?.Single());
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Fixup_works_when_changing_state_from_Detached_to_Modified(bool detachDependent)
    {
        using var context = new OwnedModifiedContext(Guid.NewGuid().ToString());

        var details = new ProductDetails { Color = "C1", Size = "S1" };
        var product = new Product { Name = "Product1", Details = details };

        context.Add(product);

        Assert.True(context.ChangeTracker.HasChanges());

        context.SaveChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(details).State);

        context.Entry(product).State = EntityState.Detached;
        if (detachDependent)
        {
            context.Entry(details).State = EntityState.Detached;
        }

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Empty(context.ChangeTracker.Entries());
        Assert.Equal(EntityState.Detached, context.Entry(details).State);

        var newDetails = new ProductDetails { Color = "C2", Size = "S2" };

        var newProduct = new Product
        {
            Id = product.Id,
            Name = "Product1NewName",
            Details = newDetails
        };

        context.Update(newProduct);

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(2, context.ChangeTracker.Entries().Count());

        Assert.Equal(EntityState.Modified, context.Entry(newProduct).State);
        Assert.Equal(EntityState.Modified, context.Entry(newDetails).State);

        Assert.Same(details, product.Details);
        Assert.Equal("C1", product.Details.Color);
        Assert.Same(newDetails, newProduct.Details);
        Assert.Equal("C2", newProduct.Details.Color);

        context.SaveChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(2, context.ChangeTracker.Entries().Count());
        Assert.Equal(EntityState.Unchanged, context.Entry(newProduct).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(newDetails).State);
    }

    private class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ProductDetails Details { get; set; }
    }

    private class ProductDetails
    {
        public string Color { get; set; }
        public string Size { get; set; }
    }

    private class OwnedModifiedContext(string databaseName) : DbContext
    {
        private readonly string _databaseName = databaseName;

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(_databaseName);

        protected internal override void OnModelCreating(ModelBuilder builder)
            => builder.Entity<Product>().OwnsOne(x => x.Details);
    }

    [ConditionalFact]
    public void Can_save_multiple_deep_owned_entities()
    {
        using var context = new StreetContext(nameof(StreetContext));
        var address1 = new StreetAddress { Street = "1", City = "City" };

        var address2 = new StreetAddress { Street = "2", City = "City" };

        var distributor = new Distributor { ShippingCenters = new List<StreetAddress> { address1, address2 } };

        context.Add(distributor);

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.Equal(EntityState.Added, context.Entry(distributor).State);
        Assert.Equal(EntityState.Added, context.Entry(address1).State);
        Assert.Equal(EntityState.Added, context.Entry(address2).State);

        Assert.Equal(2, distributor.ShippingCenters.Count);
        Assert.Contains(address1, distributor.ShippingCenters);
        Assert.Contains(address2, distributor.ShippingCenters);

        context.SaveChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.Equal(EntityState.Unchanged, context.Entry(distributor).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(address1).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(address2).State);

        Assert.Equal(2, distributor.ShippingCenters.Count);
        Assert.Contains(address1, distributor.ShippingCenters);
        Assert.Contains(address2, distributor.ShippingCenters);
    }

    private class StreetAddress
    {
        public string Street { get; set; }
        public string City { get; set; }
    }

    private class Distributor
    {
        public int Id { get; set; }
        public ICollection<StreetAddress> ShippingCenters { get; set; }
    }

    private class StreetContext(string databaseName) : DbContext
    {
        private readonly string _databaseName = databaseName;

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(_databaseName);

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Distributor>().OwnsMany(
                rt => rt.ShippingCenters, image =>
                {
                    image.WithOwner().HasForeignKey("DistributorId");
                    image.Property<int>("Id");
                    image.HasKey("DistributorId", "Id");
                });
    }

    [ConditionalFact]
    public void Can_replace_owned_entity_after_deleting()
    {
        const long MyBookId = 1234;

        var info = new Info { Title = "MyBook" };

        var book = new Book
        {
            BookId = MyBookId,
            Pages = 99,
            EnglishInfo = info
        };

        using (var context = new BooksContext(nameof(BooksContext)))
        {
            context.Books.Add(book);

            Assert.True(context.ChangeTracker.HasChanges());

            context.SaveChanges();

            Assert.False(context.ChangeTracker.HasChanges());

            Assert.Same(info, book.EnglishInfo);
            Assert.Equal("MyBook", book.EnglishInfo.Title);
        }

        using (var context = new BooksContext(nameof(BooksContext)))
        {
            context.Attach(book);

            Assert.False(context.ChangeTracker.HasChanges());

            Assert.Same(info, book.EnglishInfo);
            Assert.Equal("MyBook", book.EnglishInfo.Title);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Unchanged, context.Entry(book).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(info).State);

            var newInfo = new Info { Title = "MyBook Rev 2" };

            var newBook = new Book
            {
                BookId = MyBookId,
                Pages = 100,
                EnglishInfo = newInfo
            };

            context.Remove(book);

            Assert.True(context.ChangeTracker.HasChanges());

            context.Add(newBook);

            Assert.True(context.ChangeTracker.HasChanges());

            Assert.Equal(4, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Deleted, context.Entry(book).State);
            Assert.Equal(EntityState.Deleted, context.Entry(info).State);
            Assert.Equal(EntityState.Added, context.Entry(newBook).State);
            Assert.Equal(EntityState.Added, context.Entry(newInfo).State);

            Assert.Same(info, book.EnglishInfo);
            Assert.Equal("MyBook", book.EnglishInfo.Title);
            Assert.Same(newInfo, newBook.EnglishInfo);
            Assert.Equal("MyBook Rev 2", newBook.EnglishInfo.Title);

            context.SaveChanges();

            Assert.False(context.ChangeTracker.HasChanges());

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Unchanged, context.Entry(newBook).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(newInfo).State);

            Assert.Same(info, book.EnglishInfo);
            Assert.Equal("MyBook", book.EnglishInfo.Title);
            Assert.Same(newInfo, newBook.EnglishInfo);
            Assert.Equal("MyBook Rev 2", newBook.EnglishInfo.Title);
        }
    }

    [ConditionalFact]
    public void Can_replace_owned_entity_with_unchanged_entity_after_deleting()
    {
        const long MyBookId = 1534;

        var info = new Info { Title = "MyBook" };

        var book = new Book
        {
            BookId = MyBookId,
            Pages = 99,
            EnglishInfo = info
        };

        using (var context = new BooksContext(nameof(BooksContext)))
        {
            context.Books.Add(book);

            Assert.True(context.ChangeTracker.HasChanges());

            context.SaveChanges();

            Assert.False(context.ChangeTracker.HasChanges());

            Assert.Same(info, book.EnglishInfo);
            Assert.Equal("MyBook", book.EnglishInfo.Title);
        }

        using (var context = new BooksContext(nameof(BooksContext)))
        {
            context.Attach(book);

            Assert.False(context.ChangeTracker.HasChanges());

            Assert.Same(info, book.EnglishInfo);
            Assert.Equal("MyBook", book.EnglishInfo.Title);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Unchanged, context.Entry(book).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(info).State);

            var newInfo = new Info { Title = "MyBook Rev 2" };

            var newBook = new Book
            {
                BookId = MyBookId,
                Pages = 100,
                EnglishInfo = newInfo
            };

            context.Remove(book);

            Assert.True(context.ChangeTracker.HasChanges());

            context.Attach(newBook);

            Assert.False(context.ChangeTracker.HasChanges());

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Unchanged, context.Entry(newBook).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(newInfo).State);

            Assert.Same(info, book.EnglishInfo);
            Assert.Equal("MyBook", book.EnglishInfo.Title);
            Assert.Same(newInfo, newBook.EnglishInfo);
            Assert.Equal("MyBook Rev 2", newBook.EnglishInfo.Title);

            context.SaveChanges();

            Assert.False(context.ChangeTracker.HasChanges());

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Unchanged, context.Entry(newBook).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(newInfo).State);

            Assert.Same(info, book.EnglishInfo);
            Assert.Equal("MyBook", book.EnglishInfo.Title);
            Assert.Same(newInfo, newBook.EnglishInfo);
            Assert.Equal("MyBook Rev 2", newBook.EnglishInfo.Title);
        }
    }

    private class Book
    {
        public long BookId { get; set; }
        public int Pages { get; set; }
        public Info EnglishInfo { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            var e_tb = modelBuilder.Entity<Book>();
            e_tb.Property(e => e.BookId);
            e_tb.Property(e => e.Pages);
            e_tb.HasKey(e => e.BookId);
            e_tb.OwnsOne(e => e.EnglishInfo, rob => Info.OnModelCreating(rob));
        }
    }

    private class Info
    {
        public string Title { get; set; }

        public static void OnModelCreating<T>(OwnedNavigationBuilder<T, Info> rob)
            where T : class
            => rob.Property(e => e.Title);
    }

    private class BooksContext(string databaseName) : DbContext
    {
        private readonly string _databaseName = databaseName;

        public DbSet<Book> Books { get; set; }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(_databaseName);

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => Book.OnModelCreating(modelBuilder);
    }

    [ConditionalFact]
    public void Entities_with_owned_custom_enum_pattern_are_tracked_correctly_if_not_shared()
    {
        using var context = new TestCurrencyContext(nameof(TestCurrencyContext));
        var items = new List<TestOrderItem>
        {
            new() { ProductName = "Test Product 1", Price = new TestMoney { Amount = 99.99, Currency = TestCurrency.EUR } },
            new() { ProductName = "Test Product 3", Price = new TestMoney { Amount = 8.95, Currency = TestCurrency.USD } }
        };

        var order = new TestOrder { CustomerName = "Test Customer", TestOrderItems = items };

        Assert.Equal(2, order.TestOrderItems.Count);
        Assert.Equal("EUR", order.TestOrderItems.Single(e => e.ProductName == "Test Product 1").Price.Currency.Code);
        Assert.Equal("USD", order.TestOrderItems.Single(e => e.ProductName == "Test Product 3").Price.Currency.Code);

        context.TestOrders.Add(order);

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(2, order.TestOrderItems.Count);
        Assert.Equal("EUR", order.TestOrderItems.Single(e => e.ProductName == "Test Product 1").Price.Currency.Code);
        Assert.Equal("USD", order.TestOrderItems.Single(e => e.ProductName == "Test Product 3").Price.Currency.Code);

        context.SaveChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(2, order.TestOrderItems.Count);
        Assert.Equal("EUR", order.TestOrderItems.Single(e => e.ProductName == "Test Product 1").Price.Currency.Code);
        Assert.Equal("USD", order.TestOrderItems.Single(e => e.ProductName == "Test Product 3").Price.Currency.Code);
    }

    private class TestOrder
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }

        public IList<TestOrderItem> TestOrderItems { get; set; }
    }

    private class TestOrderItem
    {
        public int Id { get; set; }
        public int TestOrderId { get; set; }
        public string ProductName { get; set; }
        public TestMoney Price { get; set; }
    }

    private class TestMoney
    {
        public double Amount { get; set; }
        public TestCurrency Currency { get; set; }
    }

    private class TestCurrency
    {
        public static readonly TestCurrency EUR = new(49, "EUR", 978, "Euro");
        public static readonly TestCurrency USD = new(148, "USD", 840, "United States dollar");

        private TestCurrency()
        {
        }

        public TestCurrency(int id, string code, int numericCode, string name)
        {
            Id = id;
            Name = name;
            Code = code;
            NumericCode = numericCode;
        }

        public int Id { get; }
        public string Name { get; }
        public string Code { get; }
        public int NumericCode { get; }
    }

    private class TestCurrencyContext(string databaseName) : DbContext
    {
        private readonly string _databaseName = databaseName;

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(_databaseName);

        public DbSet<TestOrder> TestOrders { get; set; }

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestOrder>().HasKey(o => o.Id);

            modelBuilder.Entity<TestOrderItem>()
                .OwnsOne(
                    oi => oi.Price, ip =>
                    {
                        ip.Property(p => p.Amount).IsRequired();
                        ip.OwnsOne(
                            p => p.Currency, pc =>
                            {
                                pc.Property(c => c.Code).IsRequired();
                                pc.Ignore(c => c.Id);
                                pc.Ignore(c => c.Name);
                                pc.Ignore(c => c.NumericCode);
                            });
                    }).HasKey(oi => oi.Id);
        }
    }

    [ConditionalFact]
    public void Entities_with_owned_custom_enum_pattern_using_ValueConverter_are_tracked_correctly()
    {
        using var context = new TestCurrencyContextRevisited(nameof(TestCurrencyContextRevisited));
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var items = new List<TestOrderItem>
        {
            new() { ProductName = "Test Product 1", Price = new TestMoney { Amount = 99.99, Currency = TestCurrency.EUR } },
            new() { ProductName = "Test Product 2", Price = new TestMoney { Amount = 10, Currency = TestCurrency.EUR } },
            new() { ProductName = "Test Product 3", Price = new TestMoney { Amount = 8.95, Currency = TestCurrency.USD } },
            new() { ProductName = "Test Product 4", Price = new TestMoney { Amount = 2.99, Currency = TestCurrency.USD } }
        };

        var order = new TestOrder { CustomerName = "Test Customer", TestOrderItems = items };

        Assert.Equal(4, order.TestOrderItems.Count);
        Assert.Equal("EUR", order.TestOrderItems.Single(e => e.ProductName == "Test Product 1").Price.Currency.Code);
        Assert.Equal("EUR", order.TestOrderItems.Single(e => e.ProductName == "Test Product 2").Price.Currency.Code);
        Assert.Equal("USD", order.TestOrderItems.Single(e => e.ProductName == "Test Product 3").Price.Currency.Code);
        Assert.Equal("USD", order.TestOrderItems.Single(e => e.ProductName == "Test Product 4").Price.Currency.Code);

        context.Add(order);

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(4, order.TestOrderItems.Count);
        Assert.Equal("EUR", order.TestOrderItems.Single(e => e.ProductName == "Test Product 1").Price.Currency.Code);
        Assert.Equal("EUR", order.TestOrderItems.Single(e => e.ProductName == "Test Product 2").Price.Currency.Code);
        Assert.Equal("USD", order.TestOrderItems.Single(e => e.ProductName == "Test Product 3").Price.Currency.Code);
        Assert.Equal("USD", order.TestOrderItems.Single(e => e.ProductName == "Test Product 4").Price.Currency.Code);

        context.SaveChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(4, order.TestOrderItems.Count);
        Assert.Equal("EUR", order.TestOrderItems.Single(e => e.ProductName == "Test Product 1").Price.Currency.Code);
        Assert.Equal("EUR", order.TestOrderItems.Single(e => e.ProductName == "Test Product 2").Price.Currency.Code);
        Assert.Equal("USD", order.TestOrderItems.Single(e => e.ProductName == "Test Product 3").Price.Currency.Code);
        Assert.Equal("USD", order.TestOrderItems.Single(e => e.ProductName == "Test Product 4").Price.Currency.Code);
    }

    private class TestCurrencyContextRevisited(string databaseName) : DbContext
    {
        private readonly string _databaseName = databaseName;

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(_databaseName);

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestOrder>().HasKey(o => o.Id);

            modelBuilder.Entity<TestOrderItem>()
                .OwnsOne(
                    oi => oi.Price, ip =>
                    {
                        ip.Property(p => p.Amount).IsRequired();
                        ip.Property(p => p.Currency).HasConversion(
                            v => v.Code,
                            v => v == "EUR" ? TestCurrency.EUR : v == "USD" ? TestCurrency.USD : null);
                    }).HasKey(oi => oi.Id);
        }
    }

    [ConditionalFact]
    public void Equatable_entities_that_comply_are_tracked_correctly()
    {
        EntityState GetEntryState<TEntity>(EquatableEntitiesContext context, string role = null)
            where TEntity : class
            => context
                .ChangeTracker
                .Entries<TEntity>()
                .Single(e => role == null || e.Property("Value").CurrentValue.Equals(role))
                .State;

        using (var context = new EquatableEntitiesContext("EquatableEntities"))
        {
            var user = new User();

            user.SetRoles(
                new[] { new Role { Value = "Pascal" }, new Role { Value = "Smalltalk" }, new Role { Value = "COBOL" } });

            context.Add(user);

            Assert.True(context.ChangeTracker.HasChanges());

            context.SaveChanges();

            Assert.False(context.ChangeTracker.HasChanges());

            Assert.Equal(4, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Unchanged, GetEntryState<User>(context));
            Assert.Equal(EntityState.Unchanged, GetEntryState<Role>(context, "Pascal"));
            Assert.Equal(EntityState.Unchanged, GetEntryState<Role>(context, "Smalltalk"));
            Assert.Equal(EntityState.Unchanged, GetEntryState<Role>(context, "COBOL"));

            Assert.Equal(3, user.Roles.Count);
            var roles = user.Roles.Select(e => e.Value).ToList();
            Assert.Contains("Pascal", roles);
            Assert.Contains("Smalltalk", roles);
            Assert.Contains("COBOL", roles);

            user.SetRoles(
                new List<Role> { new() { Value = "BASIC" } });

            Assert.Equal(5, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Unchanged, GetEntryState<User>(context));
            Assert.Equal(EntityState.Deleted, GetEntryState<Role>(context, "Pascal"));
            Assert.Equal(EntityState.Deleted, GetEntryState<Role>(context, "Smalltalk"));
            Assert.Equal(EntityState.Deleted, GetEntryState<Role>(context, "COBOL"));
            Assert.Equal(EntityState.Added, GetEntryState<Role>(context, "BASIC"));

            Assert.Equal(1, user.Roles.Count);
            Assert.Equal("BASIC", user.Roles.Select(e => e.Value).Single());

            Assert.True(context.ChangeTracker.HasChanges());

            context.SaveChanges();

            Assert.False(context.ChangeTracker.HasChanges());

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Unchanged, GetEntryState<User>(context));
            Assert.Equal(EntityState.Unchanged, GetEntryState<Role>(context, "BASIC"));

            Assert.Equal(1, user.Roles.Count);
            Assert.Equal("BASIC", user.Roles.Select(e => e.Value).Single());
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveChanges_when_owner_has_PK_with_default_values(bool async)
    {
        using (var context = new OneRowContext(async))
        {
            var blog = new Blog { Type = new OwnedType { Value = "A" } };

            _ = async
                ? await context.AddAsync(blog)
                : context.Add(blog);

            Assert.Equal(EntityState.Added, context.Entry(blog).State);
            Assert.Equal(EntityState.Added, context.Entry(blog.Type).State);
            Assert.Equal(0, blog.Id);
            Assert.Equal(0, context.Entry(blog.Type).Property<int>("BlogId").CurrentValue);

            _ = async
                ? await context.SaveChangesAsync()
                : context.SaveChanges();

            Assert.Equal(EntityState.Unchanged, context.Entry(blog).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(blog.Type).State);
            Assert.Equal(0, blog.Id);
            Assert.Equal(0, context.Entry(blog.Type).Property<int>("BlogId").CurrentValue);
        }

        using (var context = new OneRowContext(async))
        {
            // Trying to do the same thing again will throw since only one row can have ID zero.

            await context.AddAsync(new Blog { Type = new OwnedType { Value = "A" } });
            Assert.Throws<ArgumentException>(() => context.SaveChanges());
        }
    }

    private class OneRowContext(bool async) : DbContext
    {
        private readonly bool _async = async;

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(nameof(OneRowContext) + _async);

        public DbSet<Blog> Blogs { get; set; }
    }

    public class Blog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public OwnedType Type { get; set; }
    }

    [Owned]
    public class OwnedType
    {
        public string Value { get; set; }
    }

    private class User
    {
        public Guid UserId { get; set; }

        public IReadOnlyList<Role> Roles
            => _roles.AsReadOnly();

        private readonly List<Role> _roles = [];

        public void SetRoles(IList<Role> roles)
        {
            if (_roles.Count == roles.Count
                && !_roles.Except(roles).Any())
            {
                return;
            }

            _roles.Clear();
            _roles.AddRange(roles.Where(x => x != null).Distinct());
        }
    }

    private class Role : IEquatable<Role>
    {
        private Guid RoleAssignmentId { get; set; }
        public string Value { get; set; }

        public bool Equals(Role other)
            => Value == other.Value;
    }

    private class EquatableEntitiesContext(string databaseName) : DbContext
    {
        private readonly string _databaseName = databaseName;

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(_databaseName);

        protected internal override void OnModelCreating(ModelBuilder builder)
            => builder.Entity<User>(
                m =>
                {
                    m.HasKey(x => x.UserId);
                    m.OwnsMany(
                        x => x.Roles,
                        b =>
                        {
                            b.Property<Guid>("RoleAssignmentId");
                            b.HasKey("RoleAssignmentId");
                            b.Property(x => x.Value);
                            b.Property<Guid>("UserId");
                            b.WithOwner().HasForeignKey("UserId");
                        }).UsePropertyAccessMode(PropertyAccessMode.Field);
                });
    }

    private class Parent : IComparable<Parent>
    {
        public int Id { get; set; }

        public Child Child1 { get; set; }
        public Child Child2 { get; set; }
        public ICollection<Child> ChildCollection1 { get; set; }
        public ICollection<Child> ChildCollection2 { get; set; }

        public int CompareTo(Parent other)
            => Id - other.Id;

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

    private class Child : IComparable<Child>
    {
        public string Name { get; set; }

        public Parent Parent { get; set; }
        public SubChild SubChild1 { get; set; }
        public SubChild SubChild2 { get; set; }
        public ICollection<SubChild> SubChildCollection { get; set; }

        public int CompareTo(Child other)
            => StringComparer.InvariantCulture.Compare(Name, other.Name);

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
    private class SubChild : IComparable<SubChild>
    {
        // ReSharper disable once UnusedMember.Local
        public string Name { get; set; }

        public Child Parent { get; set; }

        public int CompareTo(SubChild other)
            => StringComparer.InvariantCulture.Compare(Name, other.Name);

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

    private class ParentPN : IComparable<ParentPN>
    {
        public int Id { get; set; }

        public ChildPN Child1 { get; set; }
        public ChildPN Child2 { get; set; }
        public ICollection<ChildPN> ChildCollection1 { get; set; }
        public ICollection<ChildPN> ChildCollection2 { get; set; }

        public int CompareTo(ParentPN other)
            => Id - other.Id;
    }

    private class ChildPN : IComparable<ChildPN>
    {
        public string Name { get; set; }

        // ReSharper disable once MemberHidesStaticFromOuterClass
        public SubChildPN SubChild { get; set; }
        public ICollection<SubChildPN> SubChildCollection { get; set; }

        public int CompareTo(ChildPN other)
            => StringComparer.InvariantCulture.Compare(Name, other.Name);
    }

    private class SubChildPN : IComparable<SubChildPN>
    {
        public string Name { get; set; }

        public int CompareTo(SubChildPN other)
            => StringComparer.InvariantCulture.Compare(Name, other.Name);
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

                            cb.OwnsOne(
                                c => c.SubChild1, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.WithOwner(c => c.Parent)
                                        .HasForeignKey("ParentId");
                                });

                            cb.OwnsOne(
                                c => c.SubChild2, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.WithOwner(c => c.Parent)
                                        .HasForeignKey("ParentId");
                                });

                            cb.OwnsMany(
                                c => c.SubChildCollection, sb =>
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

                            cb.OwnsOne(
                                c => c.SubChild1, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.WithOwner(c => c.Parent)
                                        .HasForeignKey("ParentId");
                                });

                            cb.OwnsOne(
                                c => c.SubChild2, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.WithOwner(c => c.Parent)
                                        .HasForeignKey("ParentId");
                                });

                            cb.OwnsMany(
                                c => c.SubChildCollection, sb =>
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

                            cb.OwnsOne(
                                c => c.SubChild1, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.Property<int>("ChildId");
                                    sb.WithOwner(c => c.Parent)
                                        .HasForeignKey("ParentId", "ChildId");
                                });

                            cb.OwnsOne(
                                c => c.SubChild2, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.Property<int>("ChildId");
                                    sb.WithOwner(c => c.Parent)
                                        .HasForeignKey("ParentId", "ChildId");
                                });

                            cb.OwnsMany(
                                c => c.SubChildCollection, sb =>
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

                            cb.OwnsOne(
                                c => c.SubChild1, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.Property<int>("ChildId");
                                    sb.WithOwner(c => c.Parent)
                                        .HasForeignKey("ParentId", "ChildId");
                                });

                            cb.OwnsOne(
                                c => c.SubChild2, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.Property<int>("ChildId");
                                    sb.WithOwner(c => c.Parent)
                                        .HasForeignKey("ParentId", "ChildId");
                                });

                            cb.OwnsMany(
                                c => c.SubChildCollection, sb =>
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

                            cb.OwnsOne(
                                c => c.SubChild, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.WithOwner()
                                        .HasForeignKey("ParentId");
                                });

                            cb.OwnsMany(
                                c => c.SubChildCollection, sb =>
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

                            cb.OwnsOne(
                                c => c.SubChild, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.WithOwner()
                                        .HasForeignKey("ParentId");
                                });

                            cb.OwnsMany(
                                c => c.SubChildCollection, sb =>
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

                            cb.OwnsOne(
                                c => c.SubChild, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.Property<int>("ChildId");
                                    sb.WithOwner()
                                        .HasForeignKey("ParentId", "ChildId");
                                });

                            cb.OwnsMany(
                                c => c.SubChildCollection, sb =>
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

                            cb.OwnsOne(
                                c => c.SubChild, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.Property<int>("ChildId");
                                    sb.WithOwner()
                                        .HasForeignKey("ParentId", "ChildId");
                                });

                            cb.OwnsMany(
                                c => c.SubChildCollection, sb =>
                                {
                                    sb.Property<int>("ParentId");
                                    sb.Property<int>("ChildId");
                                    sb.WithOwner()
                                        .HasForeignKey("ParentId", "ChildId");
                                });
                        });
                });

            modelBuilder.Entity<Thing>().OwnsMany(
                p => p.OwnedByThings, a =>
                {
                    a.WithOwner(e => e.Thing).HasForeignKey(e => e.ThingId);
                    a.HasKey(e => e.OwnedByThingId);
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

    public enum CollectionType
    {
        Default,
        HashSet = Default,
        List,
        SortedSet,
        Collection,
        ObservableCollection,
        ObservableHashSet
    }

    private static ICollection<T> CreateChildCollection<T>(CollectionType collectionType, T dependent)
        where T : class
        => collectionType switch
        {
            CollectionType.List => new List<T> { dependent },
            CollectionType.SortedSet => new SortedSet<T> { dependent },
            CollectionType.Collection => new Collection<T> { dependent },
            CollectionType.ObservableCollection => new ObservableCollection<T> { dependent },
            CollectionType.ObservableHashSet => new ObservableHashSet<T>(ReferenceEqualityComparer.Instance) { dependent },
            _ => new HashSet<T>(ReferenceEqualityComparer.Instance) { dependent }
        };

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
