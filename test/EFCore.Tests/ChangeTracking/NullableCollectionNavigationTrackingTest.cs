// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable EF1001 // Internal EF Core API usage.
using System.ComponentModel;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

public class NullableCollectionNavigationTrackingTest
{
    [Fact]
    public void Changing_nullable_collection_navigation_from_empty_to_null_is_detected()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = [] };
        context.Attach(parent);
        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);

        parent.Children = null;
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Changing_nullable_collection_navigation_from_null_to_empty_is_detected()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = null };
        context.Attach(parent);

        parent.Children = [];
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Changing_nullable_collection_navigation_with_elements_to_null_is_detected()
    {
        using var context = new NullableContext();
        var child = new Child { Id = 2 };
        var parent = new Parent { Id = 1, Children = [child] };
        context.Attach(parent);
        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);

        parent.Children = null;
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Changing_nullable_collection_navigation_to_another_non_null_collection_is_not_a_nullness_change()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = [] };
        context.Attach(parent);

        parent.Children = [new Child { Id = 2 }];
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Changing_collection_navigation_that_is_not_nullable_from_empty_to_null_is_not_detected()
    {
        using var context = new NonNullableContext();
        var parent = new Parent { Id = 1, Children = [] };
        context.Attach(parent);

        parent.Children = null;
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Reverting_a_nullness_change_before_saving_keeps_the_modification_like_for_a_property()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = [] };
        context.Attach(parent);

        parent.Children = null;
        context.ChangeTracker.DetectChanges();
        Assert.Equal(EntityState.Modified, context.Entry(parent).State);

        parent.Children = [];
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Explicitly_unmodifying_a_nullable_collection_navigation_returns_the_entry_to_Unchanged()
    {
        using var context = new NullableContext();
        var children = new List<Child>();
        var parent = new Parent { Id = 1, Children = children };
        context.Attach(parent);
        parent.Children = null;
        context.ChangeTracker.DetectChanges();
        Assert.Equal(EntityState.Modified, context.Entry(parent).State);

        var entry = context.Entry(parent);
        entry.GetInfrastructure().SetPropertyModified((INavigation)entry.Metadata.FindNavigation(nameof(Parent.Children))!, false);

        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
        Assert.Same(children, parent.Children);
    }

    [Fact]
    public void Unmodifying_a_nullable_collection_navigation_reverts_a_change_from_null_to_non_null()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = null };
        context.Attach(parent);
        parent.Children = [];
        context.ChangeTracker.DetectChanges();
        Assert.True(IsChildrenModified(context, parent));

        context.Entry(parent).Collection(p => p.Children!).IsModified = false;
        context.ChangeTracker.DetectChanges();

        Assert.Null(parent.Children);
        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Unmodifying_a_nullable_collection_navigation_can_accept_the_current_value_instead_of_reverting()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = [] };
        context.Attach(parent);
        parent.Children = null;
        context.ChangeTracker.DetectChanges();

        var entry = context.Entry(parent);
        entry.GetInfrastructure().SetPropertyModified(
            (INavigation)entry.Metadata.FindNavigation(nameof(Parent.Children))!, false, acceptChanges: true);
        context.ChangeTracker.DetectChanges();

        Assert.Null(parent.Children);
        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Unmodifying_an_unrelated_property_does_not_revert_a_nullness_change()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Name = "A", Children = [] };
        context.Attach(parent);

        parent.Name = "B";
        parent.Children = null;
        context.ChangeTracker.DetectChanges();
        Assert.Equal(EntityState.Modified, context.Entry(parent).State);

        context.Entry(parent).Property(p => p.Name).IsModified = false;

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Detecting_changes_repeatedly_keeps_the_same_result()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = [] };
        context.Attach(parent);

        parent.Children = null;
        context.ChangeTracker.DetectChanges();
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Saving_a_nullness_change_resets_the_baseline()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = [] };
        context.Add(parent);
        context.SaveChanges();
        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);

        parent.Children = null;
        context.ChangeTracker.DetectChanges();
        Assert.True(IsChildrenModified(context, parent));
        context.SaveChanges();

        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));

        context.ChangeTracker.DetectChanges();
        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);

        parent.Children = [];
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Setting_state_to_Modified_flags_a_nullable_collection_navigation(bool empty)
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = empty ? [] : null };
        context.Attach(parent);
        Assert.False(IsChildrenModified(context, parent));

        context.Entry(parent).State = EntityState.Modified;

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Update_flags_a_nullable_collection_navigation_of_a_disconnected_entity()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = null };

        context.Update(parent);

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Flag_set_by_an_explicit_Modified_state_survives_DetectChanges()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = null };
        context.Update(parent);

        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Setting_state_back_to_Unchanged_clears_the_flag_of_a_nullable_collection_navigation()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = null };
        context.Update(parent);
        Assert.True(IsChildrenModified(context, parent));

        context.Entry(parent).State = EntityState.Unchanged;

        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Setting_state_to_Modified_does_not_flag_a_collection_navigation_that_is_not_nullable()
    {
        using var context = new NonNullableContext();
        var parent = new Parent { Id = 1, Children = null };

        context.Update(parent);

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Saving_an_explicitly_modified_entity_clears_the_flag_of_a_nullable_collection_navigation()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = [] };
        context.Add(parent);
        context.SaveChanges();

        context.Entry(parent).State = EntityState.Modified;
        Assert.True(IsChildrenModified(context, parent));

        context.SaveChanges();

        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Setting_the_original_value_of_a_nullable_collection_navigation_to_null_detects_a_nullness_change()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = [] };
        context.Attach(parent);
        Assert.False(IsChildrenModified(context, parent));

        var entry = context.Entry(parent);
        entry.GetInfrastructure().SetOriginalValue(entry.Metadata.FindNavigation(nameof(Parent.Children))!, null);

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Setting_the_original_value_of_a_nullable_collection_navigation_to_match_the_current_value_is_not_a_change()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = null };
        context.Attach(parent);

        var entry = context.Entry(parent);
        entry.GetInfrastructure().SetOriginalValue(entry.Metadata.FindNavigation(nameof(Parent.Children))!, null);

        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Setting_the_original_value_of_a_nullable_collection_navigation_does_not_unmodify_a_detected_change()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = [] };
        context.Attach(parent);
        parent.Children = null;
        context.ChangeTracker.DetectChanges();
        Assert.True(IsChildrenModified(context, parent));

        var entry = context.Entry(parent);
        entry.GetInfrastructure().SetOriginalValue(entry.Metadata.FindNavigation(nameof(Parent.Children))!, null);

        // Like for properties, changing the original value does not unmodify what was already modified.
        Assert.True(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Setting_state_to_Unchanged_after_a_detected_nullness_change_is_not_detected_again()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = [new Child { Id = 2 }] };
        context.Attach(parent);
        parent.Children = null;
        context.ChangeTracker.DetectChanges();
        Assert.True(IsChildrenModified(context, parent));

        context.Entry(parent).State = EntityState.Unchanged;
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Unmodifying_a_nullable_collection_navigation_through_CollectionEntry_reverts_and_is_not_detected_again()
    {
        using var context = new NullableContext();
        var children = new List<Child>();
        var parent = new Parent { Id = 1, Children = children };
        context.Attach(parent);
        parent.Children = null;
        context.ChangeTracker.DetectChanges();
        Assert.True(IsChildrenModified(context, parent));

        context.Entry(parent).Collection(p => p.Children!).IsModified = false;
        context.ChangeTracker.DetectChanges();

        Assert.Same(children, parent.Children);
        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Unmodifying_a_nullable_collection_navigation_through_the_internal_entry_reverts_and_is_not_detected_again()
    {
        using var context = new NullableContext();
        var children = new List<Child>();
        var parent = new Parent { Id = 1, Children = children };
        context.Attach(parent);
        parent.Children = null;
        context.ChangeTracker.DetectChanges();

        var entry = context.Entry(parent);
        entry.GetInfrastructure().SetPropertyModified((INavigation)entry.Metadata.FindNavigation(nameof(Parent.Children))!, false);
        context.ChangeTracker.DetectChanges();

        Assert.Same(children, parent.Children);
        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Original_value_of_a_nullable_collection_navigation_is_not_a_copy_of_the_collection()
    {
        using var context = new NullableContext();
        var children = new List<Child> { new() { Id = 2 } };
        var parent = new Parent { Id = 1, Children = children };
        context.Attach(parent);

        var entry = context.Entry(parent);
        var original = entry.GetInfrastructure().GetOriginalValue(entry.Metadata.FindNavigation(nameof(Parent.Children))!);

        Assert.Same(children, original);
    }

    [Fact]
    public void Setting_state_to_Added_clears_the_flag_of_a_nullable_collection_navigation()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = null };
        context.Update(parent);
        Assert.True(IsChildrenModified(context, parent));

        context.Entry(parent).State = EntityState.Added;

        Assert.Equal(EntityState.Added, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
        Assert.False(context.Entry(parent).Collection(p => p.Children!).IsModified);
    }

    [Fact]
    public void Nullness_change_of_a_navigation_declared_on_a_base_type_is_detected_for_a_derived_entity()
    {
        using var context = new InheritedNullableContext();
        var parent = new DerivedParent { Id = 1, Children = [], Extra = "X" };
        context.Attach(parent);
        Assert.False(IsChildrenModified(context, parent));

        parent.Children = null;
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Nullness_change_of_a_derived_entity_does_not_affect_its_other_properties()
    {
        using var context = new InheritedNullableContext();
        var parent = new DerivedParent { Id = 1, Children = [], Extra = "X" };
        context.Attach(parent);

        parent.Children = null;
        context.ChangeTracker.DetectChanges();

        var entry = context.Entry(parent);
        Assert.False(entry.Property(p => p.Extra).IsModified);
        Assert.False(entry.Property(p => p.Name).IsModified);
    }

    [Fact]
    public void AcceptAllChanges_resets_the_nullness_baseline()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = [] };
        context.Attach(parent);
        parent.Children = null;
        context.ChangeTracker.DetectChanges();
        Assert.True(IsChildrenModified(context, parent));

        context.ChangeTracker.AcceptAllChanges();

        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));

        context.ChangeTracker.DetectChanges();
        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);

        parent.Children = [];
        context.ChangeTracker.DetectChanges();
        Assert.True(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Changing_the_nullness_of_an_Added_entity_does_not_flag_the_navigation()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = null };
        context.Add(parent);

        parent.Children = [];
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Added, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Deleting_an_entity_with_a_detected_nullness_change_saves_without_error()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = [] };
        context.Add(parent);
        context.SaveChanges();
        parent.Children = null;
        context.ChangeTracker.DetectChanges();
        Assert.True(IsChildrenModified(context, parent));

        context.Remove(parent);
        context.SaveChanges();

        Assert.Equal(EntityState.Detached, context.Entry(parent).State);
    }

    [Fact]
    public void Nullness_change_on_a_Deleted_entity_is_not_detected()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = [] };
        context.Attach(parent);
        context.Entry(parent).State = EntityState.Deleted;

        parent.Children = null;
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Deleted, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Pending_change_from_null_to_empty_is_kept_by_a_second_DetectChanges()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = null };
        context.Attach(parent);
        parent.Children = [];
        context.ChangeTracker.DetectChanges();
        Assert.True(IsChildrenModified(context, parent));

        parent.Children = [new Child { Id = 5 }];
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Public_CollectionEntry_IsModified_reflects_a_nullness_change()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = [] };
        context.Attach(parent);
        Assert.False(context.Entry(parent).Collection(p => p.Children!).IsModified);

        parent.Children = null;
        context.ChangeTracker.DetectChanges();

        Assert.True(context.Entry(parent).Collection(p => p.Children!).IsModified);
    }

    [Fact]
    public void Public_CollectionEntry_IsModified_can_be_set_to_flag_and_unflag_a_nullable_collection()
    {
        using var context = new NullableContext();
        var parent = new Parent { Id = 1, Children = [] };
        context.Attach(parent);

        context.Entry(parent).Collection(p => p.Children!).IsModified = true;

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));

        context.Entry(parent).Collection(p => p.Children!).IsModified = false;

        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
        Assert.False(context.Entry(parent).Collection(p => p.Children!).IsModified);
    }

    [Fact]
    public void Public_CollectionEntry_IsModified_of_a_collection_that_is_not_nullable_ignores_nullness()
    {
        using var context = new NonNullableContext();
        var parent = new Parent { Id = 1, Children = [] };
        context.Attach(parent);

        context.Entry(parent).Collection(p => p.Children!).IsModified = true;

        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(context.Entry(parent).Collection(p => p.Children!).IsModified);
    }

    [Theory]
    [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
    [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
    public void Changing_notifying_nullable_collection_navigation_from_empty_to_null_is_detected(ChangeTrackingStrategy strategy)
    {
        using var context = NotifyingContext.Create(strategy);
        var parent = new NotifyingParent { Id = 1, Children = [] };
        context.Attach(parent);
        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);

        parent.Children = null;

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Theory]
    [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
    [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
    public void Changing_notifying_nullable_collection_navigation_from_null_to_empty_is_detected(ChangeTrackingStrategy strategy)
    {
        using var context = NotifyingContext.Create(strategy);
        var parent = new NotifyingParent { Id = 1, Children = null };
        context.Attach(parent);

        parent.Children = [];

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Replacing_a_notifying_nullable_collection_navigation_with_another_collection_is_not_a_nullness_change()
    {
        using var context = NotifyingContext.Create(ChangeTrackingStrategy.ChangedNotifications);
        var parent = new NotifyingParent { Id = 1, Children = [] };
        context.Attach(parent);

        parent.Children = [new NotifyingChild { Id = 2 }];

        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
    }

    [Theory]
    [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
    [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
    public void Update_flags_a_notifying_nullable_collection_navigation_of_a_disconnected_entity(ChangeTrackingStrategy strategy)
    {
        using var context = NotifyingContext.Create(strategy);
        var parent = new NotifyingParent { Id = 1, Children = null };

        context.Update(parent);

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Theory]
    [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
    [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
    public void Saving_a_notifying_nullness_change_resets_the_baseline(ChangeTrackingStrategy strategy)
    {
        using var context = NotifyingContext.Create(strategy);
        var parent = new NotifyingParent { Id = 1, Children = [] };
        context.Add(parent);
        context.SaveChanges();

        parent.Children = null;
        Assert.True(IsChildrenModified(context, parent));
        context.SaveChanges();

        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));

        parent.Children = [];
        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    private static bool IsChildrenModified(DbContext context, object parent)
    {
        var entry = context.Entry(parent);
        return ((IUpdateEntry)entry.GetInfrastructure()).IsModified(
            (INavigation)entry.Metadata.FindNavigation("Children")!);
    }

    private abstract class NotifyingContext : DbContext
    {
        // The model is cached per context type, so each strategy needs its own type.
        public static NotifyingContext Create(ChangeTrackingStrategy strategy)
            => strategy switch
            {
                ChangeTrackingStrategy.ChangedNotifications => new ChangedNotificationsContext(),
                ChangeTrackingStrategy.ChangingAndChangedNotifications => new ChangingAndChangedNotificationsContext(),
                _ => throw new ArgumentOutOfRangeException(nameof(strategy))
            };

        protected abstract ChangeTrackingStrategy Strategy { get; }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                // The in-memory provider rejects nullable collection navigations, but change tracking is what is tested here.
                .ReplaceService<IModelValidator, ModelValidator>();

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasChangeTrackingStrategy(Strategy);
            modelBuilder.Entity<NotifyingParent>().HasMany(p => p.Children).WithOne();

            modelBuilder.Model.FindEntityType(typeof(NotifyingParent))!.FindNavigation(nameof(NotifyingParent.Children))!
                .SetIsNullable(true);
        }
    }

    private class ChangedNotificationsContext : NotifyingContext
    {
        protected override ChangeTrackingStrategy Strategy
            => ChangeTrackingStrategy.ChangedNotifications;
    }

    private class ChangingAndChangedNotificationsContext : NotifyingContext
    {
        protected override ChangeTrackingStrategy Strategy
            => ChangeTrackingStrategy.ChangingAndChangedNotifications;
    }

    private class NotifyingParent: INotifyPropertyChanged, INotifyPropertyChanging
    {
        private int _id;
        private ObservableHashSet<NotifyingChild>? _children;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event PropertyChangingEventHandler? PropertyChanging;

        public int Id
        {
            get => _id;
            set
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Id)));
                _id = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Id)));
            }
        }

        public ObservableHashSet<NotifyingChild>? Children
        {
            get => _children;
            set
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Children)));
                _children = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Children)));
            }
        }
    }

    private class NotifyingChild : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private int _id;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event PropertyChangingEventHandler? PropertyChanging;

        public int Id
        {
            get => _id;
            set
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Id)));
                _id = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Id)));
            }
        }
    }

    private class NullableContext : TestContext
    {
        protected override bool Nullable
            => true;
    }

    private class NonNullableContext : TestContext
    {
        protected override bool Nullable
            => false;
    }

    private class InheritedNullableContext : NullableContext
    {
        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DerivedParent>();
        }
    }

    private abstract class TestContext : DbContext
    {
        protected TestContext()
            // Entry() and SaveChanges() would otherwise run DetectChanges implicitly and mask what these tests observe.
            => ChangeTracker.AutoDetectChangesEnabled = false;

        protected abstract bool Nullable { get; }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                // The in-memory provider rejects nullable collection navigations, but change tracking is what is tested here.
                .ReplaceService<IModelValidator, ModelValidator>();

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>().HasMany(p => p.Children).WithOne();

            if (Nullable)
            {
                modelBuilder.Model.FindEntityType(typeof(Parent))!.FindNavigation(nameof(Parent.Children))!.SetIsNullable(true);
            }
        }
    }

    private class Parent
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<Child>? Children { get; set; }
    }

    private class DerivedParent : Parent
    {
        public string? Extra { get; set; }
    }

    private class Child
    {
        public int Id { get; set; }
    }
}
