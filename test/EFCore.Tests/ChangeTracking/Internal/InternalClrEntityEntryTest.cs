// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Runtime.CompilerServices;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public class InternalClrEntityEntryTest : InternalEntityEntryTestBase<
    InternalClrEntityEntryTest.SomeEntity,
    InternalClrEntityEntryTest.SomeSimpleEntityBase,
    InternalClrEntityEntryTest.SomeDependentEntity,
    InternalClrEntityEntryTest.SomeMoreDependentEntity,
    InternalClrEntityEntryTest.Root,
    InternalClrEntityEntryTest.FirstDependent,
    InternalClrEntityEntryTest.SecondDependent,
    InternalClrEntityEntryTest.CompositeRoot,
    InternalClrEntityEntryTest.CompositeFirstDependent,
    InternalClrEntityEntryTest.SomeCompositeEntityBase,
    InternalClrEntityEntryTest.CompositeSecondDependent,
    InternalClrEntityEntryTest.KClrContext,
    InternalClrEntityEntryTest.KClrSnapContext>
{
    [ConditionalFact]
    public virtual void All_original_values_can_be_accessed_for_entity_that_does_full_change_tracking_if_eager_values_on()
        => AllOriginalValuesTest(new FullNotificationEntity());

    [ConditionalFact]
    public virtual void Required_original_values_can_be_accessed_for_entity_that_does_full_change_tracking()
        => OriginalValuesTest(new FullNotificationEntity());

    [ConditionalFact]
    public virtual void Required_original_values_can_be_accessed_for_entity_that_does_changed_only_notification()
        => OriginalValuesTest(new ChangedOnlyEntity());

    [ConditionalFact]
    public virtual void Required_original_values_can_be_accessed_generically_for_entity_that_does_full_change_tracking()
        => GenericOriginalValuesTest(new FullNotificationEntity());

    [ConditionalFact]
    public virtual void Required_original_values_can_be_accessed_generically_for_entity_that_does_changed_only_notification()
        => GenericOriginalValuesTest(new ChangedOnlyEntity());

    [ConditionalFact]
    public virtual void Null_original_values_are_handled_for_entity_that_does_full_change_tracking()
        => NullOriginalValuesTest(new FullNotificationEntity());

    [ConditionalFact]
    public virtual void Null_original_values_are_handled_for_entity_that_does_changed_only_notification()
        => NullOriginalValuesTest(new ChangedOnlyEntity());

    [ConditionalFact]
    public virtual void Null_original_values_are_handled_generically_for_entity_that_does_full_change_tracking()
        => GenericNullOriginalValuesTest(new FullNotificationEntity());

    [ConditionalFact]
    public virtual void Null_original_values_are_handled_generically_for_entity_that_does_changed_only_notification()
        => GenericNullOriginalValuesTest(new ChangedOnlyEntity());

    [ConditionalFact]
    public virtual void Setting_property_using_state_entry_always_marks_as_modified_full_notifications()
        => SetPropertyInternalEntityEntryTest(new FullNotificationEntity());

    [ConditionalFact]
    public virtual void Setting_property_using_state_entry_always_marks_as_modified_changed_notifications()
        => SetPropertyInternalEntityEntryTest(new ChangedOnlyEntity());

    [ConditionalFact]
    public void All_original_values_can_be_accessed_for_entity_that_does_changed_only_notifications()
        => AllOriginalValuesTest(new ChangedOnlyEntity());

    [ConditionalFact]
    public virtual void Temporary_values_are_not_reset_when_entity_is_detached()
    {
        using var context = new KClrContext();
        var entity = new SomeEntity();
        var entry = context.Add(entity).GetInfrastructure();
        var keyProperty = entry.EntityType.FindProperty("Id");

        entry.SetEntityState(EntityState.Added);
        entry.SetTemporaryValue(keyProperty, -1);

        Assert.NotNull(entry[keyProperty]);
        Assert.Equal(-1, entity.Id);
        Assert.Equal(-1, entry[keyProperty]);

        entry.SetEntityState(EntityState.Detached);

        Assert.Equal(-1, entity.Id);
        Assert.Equal(-1, entry[keyProperty]);

        entry.SetEntityState(EntityState.Added);

        Assert.Equal(-1, entity.Id);
        Assert.Equal(-1, entry[keyProperty]);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Detached)]
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Deleted)]
    public void AcceptChanges_handles_different_entity_states_for_owned_types(EntityState entityState)
    {
        using var context = new KClrContext();
        var ownerEntry = context.Entry(
            new OwnerClass { Id = 1, Owned = new OwnedClass { Value = "Kool" } }).GetInfrastructure();

        ownerEntry.SetEntityState(EntityState.Unchanged);

        var entry = context.Entry(((OwnerClass)ownerEntry.Entity).Owned).GetInfrastructure();
        var valueProperty = entry.EntityType.FindProperty(nameof(OwnedClass.Value));

        entry.SetEntityState(entityState);

        if (entityState != EntityState.Unchanged)
        {
            entry[valueProperty] = "Pickle";
        }

        entry.SetOriginalValue(valueProperty, "Cheese");

        entry.AcceptChanges();

        Assert.Equal(
            entityState is EntityState.Deleted or EntityState.Detached
                ? EntityState.Detached
                : EntityState.Unchanged,
            entry.EntityState);
        if (entityState == EntityState.Unchanged)
        {
            Assert.Equal("Kool", entry[valueProperty]);
            Assert.Equal("Kool", entry.GetOriginalValue(valueProperty));
        }
        else
        {
            Assert.Equal("Pickle", entry[valueProperty]);
            Assert.Equal(
                entityState is EntityState.Detached or EntityState.Deleted ? "Cheese" : "Pickle",
                entry.GetOriginalValue(valueProperty));
        }
    }

    [ConditionalFact]
    public void Setting_an_explicit_value_on_the_entity_does_not_mark_property_as_temporary()
    {
        using var context = new KClrContext();
        var entry = context.Entry(new SomeEntity()).GetInfrastructure();
        var keyProperty = entry.EntityType.FindProperty("Id");

        var entity = (SomeEntity)entry.Entity;

        entry.SetEntityState(EntityState.Added);
        entry.SetTemporaryValue(keyProperty, -1);

        Assert.True(entry.HasTemporaryValue(keyProperty));

        entity.Id = 77;

        context.GetService<IChangeDetector>().DetectChanges(entry);

        Assert.False(entry.HasTemporaryValue(keyProperty));

        entry.SetEntityState(EntityState.Unchanged); // Does not throw

        var nameProperty = entry.EntityType.FindProperty(nameof(SomeEntity.Name));
        Assert.False(entry.HasExplicitValue(nameProperty));

        entity.Name = "Name";

        Assert.True(entry.HasExplicitValue(nameProperty));
    }

    [ConditionalFact]
    public void Setting_CLR_property_with_snapshot_change_tracking_requires_DetectChanges()
        => SetPropertyClrTest(
            new SomeEntity { Id = 1, Name = "Kool" }, needsDetectChanges: true);

    [ConditionalFact]
    public void Setting_CLR_property_with_changed_only_notifications_does_not_require_DetectChanges()
        => SetPropertyClrTest(
            new ChangedOnlyEntity { Id = 1, Name = "Kool" }, needsDetectChanges: false);

    [ConditionalFact]
    public void Setting_CLR_property_with_full_notifications_does_not_require_DetectChanges()
        => SetPropertyClrTest(
            new FullNotificationEntity { Id = 1, Name = "Kool" }, needsDetectChanges: false);

    private void SetPropertyClrTest<TEntity>(TEntity entity, bool needsDetectChanges)
        where TEntity : class, ISomeEntity
    {
        using var context = new KClrContext();
        var entry = context.Attach(entity).GetInfrastructure();
        var nameProperty = entry.EntityType.FindProperty("Name");

        Assert.False(entry.IsModified(nameProperty));
        Assert.Equal(EntityState.Unchanged, entry.EntityState);

        entity.Name = "Kool";

        Assert.False(entry.IsModified(nameProperty));
        Assert.Equal(EntityState.Unchanged, entry.EntityState);

        entity.Name = "Beans";

        if (needsDetectChanges)
        {
            Assert.False(entry.IsModified(nameProperty));
            Assert.Equal(EntityState.Unchanged, entry.EntityState);

            context.GetService<IChangeDetector>().DetectChanges(entry);
        }

        Assert.True(entry.IsModified(nameProperty));
        Assert.Equal(EntityState.Modified, entry.EntityState);
    }

    public class SomeCompositeEntityBase
    {
        public int Id1 { get; set; }
        public string Id2 { get; set; }
    }

    public class SomeDependentEntity : SomeCompositeEntityBase
    {
        public int SomeEntityId { get; set; }
        public int JustAProperty { get; set; }
    }

    public class SomeMoreDependentEntity : SomeSimpleEntityBase
    {
        public int Fk1 { get; set; }
        public string Fk2 { get; set; }
    }

    public class FullNotificationEntity : INotifyPropertyChanging, INotifyPropertyChanged, ISomeEntity
    {
        private int _id;
        private string _name;

        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    NotifyChanging();
                    _id = value;
                    NotifyChanged();
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    NotifyChanging();
                    _name = value;
                    NotifyChanged();
                }
            }
        }

        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void NotifyChanging([CallerMemberName] string propertyName = "")
            => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
    }

    public class ChangedOnlyEntity : INotifyPropertyChanged, ISomeEntity
    {
        private int _id;
        private string _name;

        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    NotifyChanged();
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class Root : IRoot
    {
        public int Id { get; set; }

        public FirstDependent First { get; set; }

        IFirstDependent IRoot.First
        {
            get => First;
            set => First = (FirstDependent)value;
        }
    }

    public class FirstDependent : IFirstDependent
    {
        public int Id { get; set; }

        public Root Root { get; set; }

        IRoot IFirstDependent.Root
        {
            get => Root;
            set => Root = (Root)value;
        }

        public SecondDependent Second { get; set; }

        ISecondDependent IFirstDependent.Second
        {
            get => Second;
            set => Second = (SecondDependent)value;
        }
    }

    public class SecondDependent : ISecondDependent
    {
        public int Id { get; set; }

        public FirstDependent First { get; set; }

        IFirstDependent ISecondDependent.First
        {
            get => First;
            set => First = (FirstDependent)value;
        }
    }

    public class CompositeRoot : ICompositeRoot
    {
        public int Id1 { get; set; }
        public string Id2 { get; set; }

        public ICompositeFirstDependent First { get; set; }
    }

    public class CompositeFirstDependent : ICompositeFirstDependent
    {
        public int Id1 { get; set; }
        public string Id2 { get; set; }

        public int RootId1 { get; set; }
        public string RootId2 { get; set; }

        public CompositeRoot Root { get; set; }

        ICompositeRoot ICompositeFirstDependent.Root
        {
            get => Root;
            set => Root = (CompositeRoot)value;
        }

        public CompositeSecondDependent Second { get; set; }

        ICompositeSecondDependent ICompositeFirstDependent.Second
        {
            get => Second;
            set => Second = (CompositeSecondDependent)value;
        }
    }

    public class CompositeSecondDependent : ICompositeSecondDependent
    {
        public int Id1 { get; set; }
        public string Id2 { get; set; }

        public int FirstId1 { get; set; }
        public string FirstId2 { get; set; }

        public CompositeFirstDependent First { get; set; }

        ICompositeFirstDependent ICompositeSecondDependent.First
        {
            get => First;
            set => First = (CompositeFirstDependent)value;
        }
    }

    public class OwnerClass
    {
        public int Id { get; set; }
        public virtual OwnedClass Owned { get; set; }
    }

    public class OwnedClass
    {
        public string Value { get; set; }
    }

    public interface ISomeEntity
    {
        int Id { get; set; }
        string Name { get; set; }
    }

    public class SomeSimpleEntityBase
    {
        public int Id { get; set; }
    }

    public class SomeEntity : SomeSimpleEntityBase, ISomeEntity
    {
        public string Name { get; set; }
    }

    public class KClrContext : KContext
    {
        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FullNotificationEntity>(
                b =>
                {
                    b.Property(e => e.Name).IsConcurrencyToken();
                    b.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
                });

            modelBuilder.Entity<ChangedOnlyEntity>(
                b =>
                {
                    b.Property(e => e.Name).IsConcurrencyToken();
                    b.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);
                });

            modelBuilder.Entity<OwnerClass>(
                eb =>
                {
                    eb.HasKey(e => e.Id);
                    var owned = eb.OwnsOne(e => e.Owned);
                    owned.WithOwner().HasForeignKey("Id");
                    owned.HasKey("Id");
                    owned.Property(e => e.Value);
                });
        }
    }

    public class KClrSnapContext : KContext
    {
        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FullNotificationEntity>(
                b =>
                {
                    b.Property(e => e.Name).IsConcurrencyToken();
                    b.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
                });

            modelBuilder.Entity<ChangedOnlyEntity>(
                b =>
                {
                    b.Property(e => e.Name).IsConcurrencyToken();
                    b.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);
                });
        }
    }
}
