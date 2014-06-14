// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking
{
    // TODO: Consider which of listerners/events/interceptors/etc is better here
    public interface IEntityStateListener
    {
        void StateChanging([NotNull] StateEntry entry, EntityState newState);
        void StateChanged([NotNull] StateEntry entry, EntityState oldState);
        void ForeignKeyPropertyChanged([NotNull] StateEntry entry, [NotNull] IProperty property, [CanBeNull] object oldValue, [CanBeNull] object newValue);
        void NavigationReferenceChanged([NotNull] StateEntry entry, [NotNull] INavigation property, [CanBeNull] object oldValue, [CanBeNull] object newValue);
    }
}
