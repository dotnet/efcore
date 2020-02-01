// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

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
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class InternalEntityEntryNotifier : IInternalEntityEntryNotifier
    {
        private readonly ILocalViewListener _localViewListener;
        private readonly IChangeDetector _changeDetector;
        private readonly INavigationFixer _navigationFixer;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalEntityEntryNotifier(
            [CanBeNull] ILocalViewListener localViewListener,
            [CanBeNull] IChangeDetector changeDetector,
            [CanBeNull] INavigationFixer navigationFixer)
        {
            _localViewListener = localViewListener;
            _changeDetector = changeDetector;
            _navigationFixer = navigationFixer;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void StateChanging(InternalEntityEntry entry, EntityState newState)
        {
            _navigationFixer.StateChanging(entry, newState);
            _localViewListener.StateChanging(entry, newState);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void StateChanged(InternalEntityEntry entry, EntityState oldState, bool fromQuery)
        {
            _navigationFixer.StateChanged(entry, oldState, fromQuery);
            _localViewListener.StateChanged(entry, oldState, fromQuery);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void TrackedFromQuery(InternalEntityEntry entry)
            => _navigationFixer.TrackedFromQuery(entry);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void NavigationReferenceChanged(
            InternalEntityEntry entry,
            INavigation navigation,
            object oldValue,
            object newValue)
            => _navigationFixer.NavigationReferenceChanged(entry, navigation, oldValue, newValue);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void NavigationCollectionChanged(
            InternalEntityEntry entry,
            INavigation navigation,
            IEnumerable<object> added,
            IEnumerable<object> removed)
            => _navigationFixer.NavigationCollectionChanged(entry, navigation, added, removed);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void KeyPropertyChanged(
            InternalEntityEntry entry,
            IProperty property,
            IReadOnlyList<IKey> keys,
            IReadOnlyList<IForeignKey> foreignKeys,
            object oldValue,
            object newValue)
            => _navigationFixer.KeyPropertyChanged(entry, property, keys, foreignKeys, oldValue, newValue);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void PropertyChanged(InternalEntityEntry entry, IPropertyBase property, bool setModified)
            => _changeDetector.PropertyChanged(entry, property, setModified);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void PropertyChanging(InternalEntityEntry entry, IPropertyBase property)
            => _changeDetector.PropertyChanging(entry, property);
    }
}
