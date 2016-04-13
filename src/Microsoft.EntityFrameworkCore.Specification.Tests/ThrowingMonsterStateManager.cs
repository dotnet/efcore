// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.FunctionalTests
{
    public class ThrowingMonsterStateManager : StateManager
    {
        public ThrowingMonsterStateManager(
            IInternalEntityEntryFactory factory,
            IInternalEntityEntrySubscriber subscriber,
            IInternalEntityEntryNotifier notifier,
            IValueGenerationManager valueGeneration,
            IModel model,
            IDatabase database,
            IConcurrencyDetector concurrencyDetector,
            ICurrentDbContext currentContext)
            : base(factory, subscriber, notifier, valueGeneration, model, database, concurrencyDetector, currentContext)
        {
        }

        protected override async Task<int> SaveChangesAsync(
            IReadOnlyList<InternalEntityEntry> entriesToSave, CancellationToken cancellationToken = new CancellationToken())
        {
            await base.SaveChangesAsync(entriesToSave, cancellationToken);

            throw new Exception("Aborting.");
        }
    }
}
