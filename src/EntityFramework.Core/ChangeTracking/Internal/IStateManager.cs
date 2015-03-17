// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public interface IStateManager
    {
        InternalEntityEntry CreateNewEntry([NotNull] IEntityType entityType);

        InternalEntityEntry GetOrCreateEntry([NotNull] object entity);

        InternalEntityEntry StartTracking(
            [NotNull] IEntityType entityType, [NotNull] object entity, [NotNull] IValueReader valueReader);

        InternalEntityEntry TryGetEntry([NotNull] EntityKey keyValue);

        InternalEntityEntry TryGetEntry([NotNull] object entity);

        IEnumerable<InternalEntityEntry> Entries { get; }

        IInternalEntityEntryNotifier Notify { get; }

        IValueGenerationManager ValueGeneration { get; }

        InternalEntityEntry StartTracking([NotNull] InternalEntityEntry entry);

        void StopTracking([NotNull] InternalEntityEntry entry);

        InternalEntityEntry GetPrincipal([NotNull] IPropertyAccessor dependentEntry, [NotNull] IForeignKey foreignKey);

        void UpdateIdentityMap([NotNull] InternalEntityEntry entry, [NotNull] EntityKey oldKey);

        IEnumerable<InternalEntityEntry> GetDependents([NotNull] InternalEntityEntry principalEntry, [NotNull] IForeignKey foreignKey);

        int SaveChanges();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
