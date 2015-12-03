// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public interface IIdentityMap
    {
        IKey Key { get; }

        bool Contains(ValueBuffer valueBuffer);

        bool Contains([NotNull] IForeignKey foreignKey, ValueBuffer valueBuffer);

        InternalEntityEntry TryGetEntry(ValueBuffer valueBuffer, bool throwOnNullKey);

        InternalEntityEntry TryGetEntry([NotNull] IForeignKey foreignKey, [NotNull] InternalEntityEntry dependentEntry);

        InternalEntityEntry TryGetEntryUsingRelationshipSnapshot([NotNull] IForeignKey foreignKey, [NotNull] InternalEntityEntry dependentEntry);

        void AddOrUpdate([NotNull] InternalEntityEntry entry);

        void Add([NotNull] InternalEntityEntry entry);

        void Remove([NotNull] InternalEntityEntry entry);

        void RemoveUsingRelationshipSnapshot([NotNull] InternalEntityEntry entry);
    }
}
