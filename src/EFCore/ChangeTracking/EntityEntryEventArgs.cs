// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     Event arguments for events relating to tracked <see cref="EntityEntry" />s.
    /// </summary>
    public class EntityEntryEventArgs : EventArgs
    {
        private readonly InternalEntityEntry _internalEntityEntry;
        private EntityEntry _entry;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityEntryEventArgs(
            [NotNull] InternalEntityEntry internalEntityEntry)
        {
            _internalEntityEntry = internalEntityEntry;
        }

        /// <summary>
        ///     The <see cref="EntityEntry" /> for the entity.
        /// </summary>
        public virtual EntityEntry Entry => _entry ?? (_entry = new EntityEntry(_internalEntityEntry));
    }
}
