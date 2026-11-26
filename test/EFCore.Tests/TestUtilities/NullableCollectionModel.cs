// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

// A small model with a collection navigation that can be null as distinct from empty, used by the change tracking tests.
// The in-memory provider rejects nullable collection navigations, but change tracking is what these tests exercise.

internal static class NullableCollectionTestHelpers
{
    public static bool IsChildrenModified(DbContext context, object parent)
    {
        var entry = context.Entry(parent);
        return ((IUpdateEntry)entry.GetInfrastructure()).IsModified(
            (INavigation)entry.Metadata.FindNavigation("Children")!);
    }
}

internal class NullableCollectionParent
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public List<NullableCollectionChild>? Children { get; set; }
}

internal class DerivedNullableCollectionParent : NullableCollectionParent
{
    public string? Extra { get; set; }
}

internal class NullableCollectionChild
{
    public int Id { get; set; }
}

internal abstract class NullableCollectionContext : DbContext
{
    protected NullableCollectionContext()
        // Entry() and SaveChanges() would otherwise run DetectChanges implicitly and mask what the tests observe.
        => ChangeTracker.AutoDetectChangesEnabled = false;

    protected abstract bool Nullable { get; }

    protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ReplaceService<IModelValidator, ModelValidator>();

    protected internal override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NullableCollectionParent>().OwnsMany(p => p.Children);
        modelBuilder.Entity<NullableCollectionParent>().Navigation(p => p.Children).IsRequired(!Nullable);
    }
}

internal class NullableCollectionNavigationContext : NullableCollectionContext
{
    protected override bool Nullable
        => true;
}

internal class NotNullableCollectionNavigationContext : NullableCollectionContext
{
    protected override bool Nullable
        => false;
}

internal class DerivedNullableCollectionNavigationContext : NullableCollectionNavigationContext
{
    protected internal override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<DerivedNullableCollectionParent>();
    }
}

internal abstract class NotifyingNullableCollectionEntity : INotifyPropertyChanged, INotifyPropertyChanging
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event PropertyChangingEventHandler? PropertyChanging;

    protected void SetWithNotify<T>(T value, ref T field, [CallerMemberName] string propertyName = "")
    {
        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

internal class NotifyingNullableCollectionParent : NotifyingNullableCollectionEntity
{
    private int _id;
    private ObservableHashSet<NotifyingNullableCollectionChild>? _children;

    public int Id
    {
        get => _id;
        set => SetWithNotify(value, ref _id);
    }

    public ObservableHashSet<NotifyingNullableCollectionChild>? Children
    {
        get => _children;
        set => SetWithNotify(value, ref _children);
    }
}

internal class NotifyingNullableCollectionChild : NotifyingNullableCollectionEntity
{
    private int _id;

    public int Id
    {
        get => _id;
        set => SetWithNotify(value, ref _id);
    }
}

internal abstract class NotifyingNullableCollectionContext : DbContext
{
    // The model is cached per context type, so each strategy needs its own type.
    public static NotifyingNullableCollectionContext Create(ChangeTrackingStrategy strategy)
        => strategy switch
        {
            ChangeTrackingStrategy.ChangedNotifications => new ChangedNotificationsNullableCollectionContext(),
            ChangeTrackingStrategy.ChangingAndChangedNotifications => new ChangingAndChangedNotificationsNullableCollectionContext(),
            ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues =>
                new ChangingAndChangedNotificationsWithOriginalValuesNullableCollectionContext(),
            _ => throw new ArgumentOutOfRangeException(nameof(strategy))
        };

    protected abstract ChangeTrackingStrategy Strategy { get; }

    protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ReplaceService<IModelValidator, ModelValidator>();

    protected internal override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasChangeTrackingStrategy(Strategy);
        modelBuilder.Entity<NotifyingNullableCollectionParent>().OwnsMany(p => p.Children);
        modelBuilder.Entity<NotifyingNullableCollectionParent>().Navigation(p => p.Children).IsRequired(false);
    }
}

internal class ChangedNotificationsNullableCollectionContext : NotifyingNullableCollectionContext
{
    protected override ChangeTrackingStrategy Strategy
        => ChangeTrackingStrategy.ChangedNotifications;
}

internal class ChangingAndChangedNotificationsNullableCollectionContext : NotifyingNullableCollectionContext
{
    protected override ChangeTrackingStrategy Strategy
        => ChangeTrackingStrategy.ChangingAndChangedNotifications;
}

internal class ChangingAndChangedNotificationsWithOriginalValuesNullableCollectionContext : NotifyingNullableCollectionContext
{
    protected override ChangeTrackingStrategy Strategy
        => ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues;
}
