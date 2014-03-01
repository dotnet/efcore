// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.ChangeTracking
{
    // TODO: Consider which of listerners/events/interceptors/etc is better here
    public interface IEntityStateListener
    {
        void StateChanging([NotNull] StateEntry entry, EntityState newState);
        void StateChanged([NotNull] StateEntry entry, EntityState oldState);
    }
}
