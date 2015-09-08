// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public interface IInternalEntityEntryFactory
    {
        InternalEntityEntry Create(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityType entityType,
            [CanBeNull] object entity);

        InternalEntityEntry Create(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityType entityType,
            [NotNull] object entity,
            ValueBuffer valueBuffer);
    }
}
