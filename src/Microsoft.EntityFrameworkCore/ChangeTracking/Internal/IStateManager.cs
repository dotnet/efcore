// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public interface IStateManager
    {
        InternalEntityEntry GetOrCreateEntry([NotNull] object entity);

        InternalEntityEntry StartTrackingFromQuery([NotNull] IEntityType baseEntityType, [NotNull] object entity, ValueBuffer valueBuffer);

        void BeginTrackingQuery();

        InternalEntityEntry TryGetEntry([NotNull] IKey key, ValueBuffer valueBuffer, bool throwOnNullKey);

        InternalEntityEntry TryGetEntry([NotNull] object entity);

        IEnumerable<InternalEntityEntry> Entries { get; }

        IInternalEntityEntryNotifier Notify { get; }

        IValueGenerationManager ValueGeneration { get; }

        InternalEntityEntry StartTracking([NotNull] InternalEntityEntry entry);

        void StopTracking([NotNull] InternalEntityEntry entry);

        void RecordReferencedUntrackedEntity([NotNull] object referencedEntity, [NotNull] INavigation navigation, [NotNull] InternalEntityEntry referencedFromEntry);

        IEnumerable<Tuple<INavigation, InternalEntityEntry>> GetRecordedReferers([NotNull] object referencedEntity, bool clear);

        InternalEntityEntry GetPrincipal([NotNull] InternalEntityEntry dependentEntry, [NotNull] IForeignKey foreignKey);

        InternalEntityEntry GetPrincipalUsingRelationshipSnapshot([NotNull] InternalEntityEntry dependentEntry, [NotNull] IForeignKey foreignKey);

        void UpdateIdentityMap([NotNull] InternalEntityEntry entry, [NotNull] IKey principalKey);

        void UpdateDependentMap([NotNull] InternalEntityEntry entry, [NotNull] IForeignKey foreignKey);

        IEnumerable<InternalEntityEntry> GetDependentsFromNavigation([NotNull] InternalEntityEntry principalEntry, [NotNull] IForeignKey foreignKey);

        IEnumerable<InternalEntityEntry> GetDependents([NotNull] InternalEntityEntry principalEntry, [NotNull] IForeignKey foreignKey);

        IEnumerable<InternalEntityEntry> GetDependentsUsingRelationshipSnapshot(
            [NotNull] InternalEntityEntry principalEntry, [NotNull] IForeignKey foreignKey);

        int SaveChanges(bool acceptAllChangesOnSuccess);

        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken));

        void AcceptAllChanges();

        DbContext Context { get; }

        bool? SingleQueryMode { get; set; }

        void Unsubscribe();
    }
}
