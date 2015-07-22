// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    // TODO: Consider which of listerners/events/interceptors/etc is better here
    // See issue #737
    public interface IEntityStateListener
    {
        void StateChanging([NotNull] InternalEntityEntry entry, EntityState newState);
        void StateChanged([NotNull] InternalEntityEntry entry, EntityState oldState);
    }
}
