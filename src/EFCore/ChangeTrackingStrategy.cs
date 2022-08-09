// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Indicates how the context detects changes to properties for an instance of the entity type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-change-detection">Change detection and notifications</see> for more information and examples.
/// </remarks>
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
    ///     To use this strategy, the entity class must implement <see cref="INotifyPropertyChanged" /> and
    ///     <see cref="INotifyPropertyChanging" />.
    ///     Original values are recorded when the entity raises the <see cref="INotifyPropertyChanging.PropertyChanging" /> event.
    ///     Properties are marked as modified when the entity raises the <see cref="INotifyPropertyChanged.PropertyChanged" /> event.
    /// </summary>
    /// <remarks>
    ///     Original values are only recorded when they are required to save changes to the entity. For example, properties that are
    ///     configured as concurrency tokens.
    /// </remarks>
    ChangingAndChangedNotifications,

    /// <summary>
    ///     To use this strategy, the entity class must implement <see cref="INotifyPropertyChanged" /> and
    ///     <see cref="INotifyPropertyChanging" />.
    ///     Original values are recorded when the entity raises the <see cref="INotifyPropertyChanging.PropertyChanging" />.
    ///     Properties are marked as modified when the entity raises the <see cref="INotifyPropertyChanged.PropertyChanged" /> event.
    /// </summary>
    /// <remarks>
    ///     Original values are recorded for all properties, regardless of whether they are required to save changes to the entity.
    /// </remarks>
    ChangingAndChangedNotificationsWithOriginalValues
}
