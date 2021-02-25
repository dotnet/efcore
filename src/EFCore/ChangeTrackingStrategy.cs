// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Indicates how the context detects changes to properties for an instance of the entity type.
    /// </summary>
    public enum ChangeTrackingStrategy
    {
        /// <summary>
        ///     Original values are recorded when an entity is queried from the database. Changes are detected by scanning the
        ///     current property values and comparing them to the recorded values. This scanning takes place when
        ///     <see cref="ChangeTracker.DetectChanges" /> is called, or when another API call (such as <see cref="DbContext.SaveChanges()" />)
        ///     triggers the change detection process.
        /// </summary>
        Snapshot,

        /// <summary>
        ///     To use this strategy, the entity class must implement <see cref="INotifyPropertyChanged" />.
        ///     Original values are recorded when an entity is queried from the database. Properties are marked as modified when the
        ///     entity raises the <see cref="INotifyPropertyChanged.PropertyChanged" /> event.
        /// </summary>
        ChangedNotifications,

        /// <summary>
        ///     <para>
        ///         To use this strategy, the entity class must implement <see cref="INotifyPropertyChanged" /> and
        ///         <see cref="INotifyPropertyChanging" />.
        ///         Original values are recorded when the entity raises the <see cref="INotifyPropertyChanging.PropertyChanging" /> event.
        ///         Properties are marked as modified when the entity raises the <see cref="INotifyPropertyChanged.PropertyChanged" /> event.
        ///     </para>
        ///     <para>
        ///         Original values are only recorded when they are required to save changes to the entity. For example, properties that are
        ///         configured as concurrency tokens.
        ///     </para>
        /// </summary>
        ChangingAndChangedNotifications,

        /// <summary>
        ///     <para>
        ///         To use this strategy, the entity class must implement <see cref="INotifyPropertyChanged" /> and
        ///         <see cref="INotifyPropertyChanging" />.
        ///         Original values are recorded when the entity raises the <see cref="INotifyPropertyChanging.PropertyChanging" />.
        ///         Properties are marked as modified when the entity raises the <see cref="INotifyPropertyChanged.PropertyChanged" /> event.
        ///     </para>
        ///     <para>
        ///         Original values are recorded for all properties, regardless of whether they are required to save changes to the entity.
        ///     </para>
        /// </summary>
        ChangingAndChangedNotificationsWithOriginalValues
    }
}
