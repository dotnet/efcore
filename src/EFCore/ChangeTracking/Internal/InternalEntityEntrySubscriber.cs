// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InternalEntityEntrySubscriber : IInternalEntityEntrySubscriber
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool SnapshotAndSubscribe(InternalEntityEntry entry)
    {
        var entityType = entry.EntityType;

        if (entityType.UseEagerSnapshots())
        {
            entry.EnsureOriginalValues();
            entry.EnsureRelationshipSnapshot();
        }

        var changeTrackingStrategy = entityType.GetChangeTrackingStrategy();

        if (changeTrackingStrategy == ChangeTrackingStrategy.Snapshot)
        {
            return false;
        }

        foreach (var navigation in entityType
                     .GetNavigations()
                     .Concat<INavigationBase>(entityType.GetSkipNavigations())
                     .Where(n => n.IsCollection))
        {
            SubscribeCollectionChanged(entry, navigation);
        }

        if (changeTrackingStrategy != ChangeTrackingStrategy.ChangedNotifications)
        {
            AsINotifyPropertyChanging(entry, entityType, changeTrackingStrategy).PropertyChanging
                += entry.HandleINotifyPropertyChanging;
        }

        AsINotifyPropertyChanged(entry, entityType, changeTrackingStrategy).PropertyChanged
            += entry.HandleINotifyPropertyChanged;

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SubscribeCollectionChanged(InternalEntityEntry entry, INavigationBase navigation)
        => AsINotifyCollectionChanged(entry, navigation, entry.EntityType, entry.EntityType.GetChangeTrackingStrategy()).CollectionChanged
            += entry.HandleINotifyCollectionChanged;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Unsubscribe(InternalEntityEntry entry)
    {
        var entityType = entry.EntityType;
        var changeTrackingStrategy = entityType.GetChangeTrackingStrategy();

        if (changeTrackingStrategy != ChangeTrackingStrategy.Snapshot)
        {
            foreach (var navigation in entityType.GetNavigations()
                         .Concat<INavigationBase>(entityType.GetSkipNavigations())
                         .Where(n => n.IsCollection))
            {
                UnsubscribeCollectionChanged(entry, navigation);
            }

            if (changeTrackingStrategy != ChangeTrackingStrategy.ChangedNotifications)
            {
                AsINotifyPropertyChanging(entry, entityType, changeTrackingStrategy).PropertyChanging
                    -= entry.HandleINotifyPropertyChanging;
            }

            AsINotifyPropertyChanged(entry, entityType, changeTrackingStrategy).PropertyChanged
                -= entry.HandleINotifyPropertyChanged;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void UnsubscribeCollectionChanged(
        InternalEntityEntry entry,
        INavigationBase navigation)
        => AsINotifyCollectionChanged(entry, navigation, entry.EntityType, entry.EntityType.GetChangeTrackingStrategy()).CollectionChanged
            -= entry.HandleINotifyCollectionChanged;

    private static INotifyCollectionChanged AsINotifyCollectionChanged(
        InternalEntityEntry entry,
        INavigationBase navigation,
        IEntityType entityType,
        ChangeTrackingStrategy changeTrackingStrategy)
    {
        var collection = entry.GetOrCreateCollection(navigation, forMaterialization: false);
        if (collection is not INotifyCollectionChanged notifyingCollection)
        {
            var collectionType = collection.GetType().DisplayName(fullName: false);
            throw new InvalidOperationException(
                CoreStrings.NonNotifyingCollection(navigation.Name, entityType.DisplayName(), collectionType, changeTrackingStrategy));
        }

        return notifyingCollection;
    }

    private static INotifyPropertyChanged AsINotifyPropertyChanged(
        InternalEntityEntry entry,
        IEntityType entityType,
        ChangeTrackingStrategy changeTrackingStrategy)
    {
        if (entry.Entity is not INotifyPropertyChanged changed)
        {
            throw new InvalidOperationException(
                CoreStrings.ChangeTrackingInterfaceMissing(
                    entityType.DisplayName(), changeTrackingStrategy, nameof(INotifyPropertyChanged)));
        }

        return changed;
    }

    private static INotifyPropertyChanging AsINotifyPropertyChanging(
        InternalEntityEntry entry,
        IEntityType entityType,
        ChangeTrackingStrategy changeTrackingStrategy)
    {
        if (entry.Entity is not INotifyPropertyChanging changing)
        {
            throw new InvalidOperationException(
                CoreStrings.ChangeTrackingInterfaceMissing(
                    entityType.DisplayName(), changeTrackingStrategy, nameof(INotifyPropertyChanging)));
        }

        return changing;
    }
}
