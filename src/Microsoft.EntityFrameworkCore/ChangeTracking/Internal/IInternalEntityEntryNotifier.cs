// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public interface IInternalEntityEntryNotifier
    {
        void StateChanging([NotNull] InternalEntityEntry entry, EntityState newState);

        void StateChanged([NotNull] InternalEntityEntry entry, EntityState oldState, bool skipInitialFixup, bool fromQuery);

        void NavigationReferenceChanged(
            [NotNull] InternalEntityEntry entry,
            [NotNull] INavigation navigation,
            [CanBeNull] object oldValue,
            [CanBeNull] object newValue);

        void NavigationCollectionChanged(
            [NotNull] InternalEntityEntry entry,
            [NotNull] INavigation navigation,
            [NotNull] IEnumerable<object> added,
            [NotNull] IEnumerable<object> removed);

        void KeyPropertyChanged(
            [NotNull] InternalEntityEntry entry,
            [NotNull] IProperty property,
            [NotNull] IReadOnlyList<IKey> keys,
            [NotNull] IReadOnlyList<IForeignKey> foreignKeys,
            [CanBeNull] object oldValue,
            [CanBeNull] object newValue);

        void PropertyChanged([NotNull] InternalEntityEntry entry, [NotNull] IPropertyBase property, bool setModified);

        void PropertyChanging([NotNull] InternalEntityEntry entry, [NotNull] IPropertyBase property);
    }
}
