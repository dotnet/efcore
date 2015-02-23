// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.InMemory.Query
{
    public class InMemoryQueryContextFactory : QueryContextFactory
    {
        private readonly InMemoryDataStore _dataStore;

        public InMemoryQueryContextFactory(
            [NotNull] StateManager stateManager,
            [NotNull] EntityKeyFactorySource entityKeyFactorySource,
            [NotNull] ClrCollectionAccessorSource collectionAccessorSource,
            [NotNull] ClrPropertySetterSource propertySetterSource,
            [NotNull] InMemoryDataStore dataStore,
            [NotNull] ILoggerFactory loggerFactory)
            : base(stateManager, entityKeyFactorySource, collectionAccessorSource, propertySetterSource, loggerFactory)
        {
            Check.NotNull(dataStore, nameof(dataStore));

            _dataStore = dataStore;
        }

        public override QueryContext CreateQueryContext() => new InMemoryQueryContext(Logger, CreateQueryBuffer(), _dataStore.Database);
    }
}
