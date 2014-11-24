// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public class ThrowingMonsterStateManager : StateManager
    {
        public ThrowingMonsterStateManager(
            StateEntryFactory factory,
            EntityKeyFactorySource entityKeyFactorySource,
            StateEntrySubscriber subscriber,
            StateEntryNotifier notifier,
            ValueGenerationManager valueGeneration,
            DbContextService<IModel> model,
            DbContextService<DataStore> dataStore)
            : base(factory, entityKeyFactorySource, subscriber, notifier, valueGeneration, model, dataStore)
        {
        }

        protected override async Task<int> SaveChangesAsync(
            IReadOnlyList<StateEntry> entriesToSave, CancellationToken cancellationToken = new CancellationToken())
        {
            await base.SaveChangesAsync(entriesToSave, cancellationToken);

            throw new Exception("Aborting.");
        }
    }
}
