// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
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
            ICurrentDbContext currentContext,
            ILoggingOptions loggingOptions)
            : base(factory, subscriber, notifier, valueGeneration, model, database, concurrencyDetector, currentContext, loggingOptions)
        {
        }

        protected override int SaveChanges(IReadOnlyList<InternalEntityEntry> entriesToSave)
        {
            throw new Exception("Aborting.");
        }

        protected override Task<int> SaveChangesAsync(
            IReadOnlyList<InternalEntityEntry> entriesToSave, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new Exception("Aborting.");
        }
    }
}
