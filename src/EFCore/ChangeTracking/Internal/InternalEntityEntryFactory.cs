// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InternalEntityEntryFactory : IInternalEntityEntryFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityEntry Create(IStateManager stateManager, IEntityType entityType, object entity)
            => NewInternalEntityEntry(stateManager, entityType, entity);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityEntry Create(IStateManager stateManager, IEntityType entityType, object entity, in ValueBuffer valueBuffer)
            => NewInternalEntityEntry(stateManager, entityType, entity, valueBuffer);

        private static InternalEntityEntry NewInternalEntityEntry(IStateManager stateManager, IEntityType entityType, object entity)
        {
            if (!entityType.HasClrType())
            {
                return new InternalShadowEntityEntry(stateManager, entityType);
            }

            Debug.Assert(entity != null);

            return entityType.ShadowPropertyCount() > 0
                ? (InternalEntityEntry)new InternalMixedEntityEntry(stateManager, entityType, entity)
                : new InternalClrEntityEntry(stateManager, entityType, entity);
        }

        private static InternalEntityEntry NewInternalEntityEntry(
            IStateManager stateManager,
            IEntityType entityType,
            object entity,
            in ValueBuffer valueBuffer)
        {
            return !entityType.HasClrType()
                ? new InternalShadowEntityEntry(stateManager, entityType, valueBuffer)
                : entityType.ShadowPropertyCount() > 0
                ? (InternalEntityEntry)new InternalMixedEntityEntry(stateManager, entityType, entity, valueBuffer)
                : new InternalClrEntityEntry(stateManager, entityType, entity);
        }
    }
}
