// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public interface IInternalEntityEntryNotifier
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        void StateChanging([NotNull] InternalEntityEntry entry, EntityState newState);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        void StateChanged([NotNull] InternalEntityEntry entry, EntityState oldState, bool fromQuery);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        void TrackedFromQuery(
            [NotNull] InternalEntityEntry entry,
            [CanBeNull] ISet<IForeignKey> handledForeignKeys);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        void NavigationReferenceChanged(
            [NotNull] InternalEntityEntry entry,
            [NotNull] INavigation navigation,
            [CanBeNull] object oldValue,
            [CanBeNull] object newValue);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        void NavigationCollectionChanged(
            [NotNull] InternalEntityEntry entry,
            [NotNull] INavigation navigation,
            [NotNull] IEnumerable<object> added,
            [NotNull] IEnumerable<object> removed);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        void KeyPropertyChanged(
            [NotNull] InternalEntityEntry entry,
            [NotNull] IProperty property,
            [NotNull] IReadOnlyList<IKey> keys,
            [NotNull] IReadOnlyList<IForeignKey> foreignKeys,
            [CanBeNull] object oldValue,
            [CanBeNull] object newValue);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        void PropertyChanged([NotNull] InternalEntityEntry entry, [NotNull] IPropertyBase property, bool setModified);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        void PropertyChanging([NotNull] InternalEntityEntry entry, [NotNull] IPropertyBase property);
    }
}
