// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
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

            foreach (var navigation in entityType.GetNavigations().Where(n => n.IsCollection()))
            {
                AsINotifyCollectionChanged(entry, navigation, entityType, changeTrackingStrategy).CollectionChanged
                    += entry.HandleINotifyCollectionChanged;
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
        public virtual void Unsubscribe(InternalEntityEntry entry)
        {
            var entityType = entry.EntityType;
            var changeTrackingStrategy = entityType.GetChangeTrackingStrategy();

            if (changeTrackingStrategy != ChangeTrackingStrategy.Snapshot)
            {
                foreach (var navigation in entityType.GetNavigations().Where(n => n.IsCollection()))
                {
                    AsINotifyCollectionChanged(entry, navigation, entityType, changeTrackingStrategy).CollectionChanged
                        -= entry.HandleINotifyCollectionChanged;
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

        private static INotifyCollectionChanged AsINotifyCollectionChanged(
            InternalEntityEntry entry,
            INavigation navigation,
            IEntityType entityType,
            ChangeTrackingStrategy changeTrackingStrategy)
        {
            if (!(navigation.GetCollectionAccessor()
                ?.GetOrCreate(entry.Entity, forMaterialization: false) is INotifyCollectionChanged notifyingCollection))
            {
                throw new InvalidOperationException(
                    CoreStrings.NonNotifyingCollection(navigation.Name, entityType.DisplayName(), changeTrackingStrategy));
            }

            return notifyingCollection;
        }

        private static INotifyPropertyChanged AsINotifyPropertyChanged(
            InternalEntityEntry entry,
            IEntityType entityType,
            ChangeTrackingStrategy changeTrackingStrategy)
        {
            if (!(entry.Entity is INotifyPropertyChanged changed))
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
            if (!(entry.Entity is INotifyPropertyChanging changing))
            {
                throw new InvalidOperationException(
                    CoreStrings.ChangeTrackingInterfaceMissing(
                        entityType.DisplayName(), changeTrackingStrategy, nameof(INotifyPropertyChanging)));
            }

            return changing;
        }
    }
}
