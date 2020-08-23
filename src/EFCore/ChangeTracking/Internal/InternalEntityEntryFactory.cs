// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
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
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class InternalEntityEntryFactory : IInternalEntityEntryFactory
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityEntry Create(IStateManager stateManager, IEntityType entityType, object entity)
            => NewInternalEntityEntry(stateManager, entityType, entity);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityEntry Create(
            IStateManager stateManager,
            IEntityType entityType,
            object entity,
            in ValueBuffer valueBuffer)
            => NewInternalEntityEntry(stateManager, entityType, entity, valueBuffer);

        private static InternalEntityEntry NewInternalEntityEntry(IStateManager stateManager, IEntityType entityType, object entity)
        {
            if (!entityType.HasClrType())
            {
                return new InternalShadowEntityEntry(stateManager, entityType);
            }

            Check.DebugAssert(entity != null, "entity is null");

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
