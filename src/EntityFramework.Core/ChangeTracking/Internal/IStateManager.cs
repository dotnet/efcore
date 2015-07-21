// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public interface IStateManager
    {
        InternalEntityEntry CreateNewEntry([NotNull] IEntityType entityType);

        InternalEntityEntry GetOrCreateEntry([NotNull] object entity);

        InternalEntityEntry StartTracking(
            [NotNull] IEntityType entityType,
            [NotNull] EntityKey entityKey,
            [NotNull] object entity,
            ValueBuffer valueBuffer);

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

        int SaveChanges(bool acceptAllChangesOnSuccess);

        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken));

        void AcceptAllChanges();
    }
}
