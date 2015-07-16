// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public interface IInternalEntityEntryNotifier
    {
        void StateChanging([NotNull] InternalEntityEntry entry, EntityState newState);

        void StateChanged([NotNull] InternalEntityEntry entry, EntityState oldState);

        void ForeignKeyPropertyChanged(
            [NotNull] InternalEntityEntry entry,
            [NotNull] IProperty property,
            [CanBeNull] object oldValue,
            [CanBeNull] object newValue);

        void NavigationReferenceChanged(
            [NotNull] InternalEntityEntry entry,
            [NotNull] INavigation navigation,
            [CanBeNull] object oldValue,
            [CanBeNull] object newValue);

        void NavigationCollectionChanged(
            [NotNull] InternalEntityEntry entry,
            [NotNull] INavigation navigation,
            [NotNull] ISet<object> added,
            [NotNull] ISet<object> removed);

        void PrincipalKeyPropertyChanged(
            [NotNull] InternalEntityEntry entry,
            [NotNull] IProperty property,
            [CanBeNull] object oldValue,
            [CanBeNull] object newValue);

        void PropertyChanged([NotNull] InternalEntityEntry entry, [NotNull] IPropertyBase property);

        void PropertyChanging([NotNull] InternalEntityEntry entry, [NotNull] IPropertyBase property);
    }
}
