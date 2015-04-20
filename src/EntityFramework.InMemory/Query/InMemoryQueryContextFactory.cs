// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.InMemory.Query
{
    public class InMemoryQueryContextFactory : QueryContextFactory, IInMemoryQueryContextFactory
    {
        private readonly IInMemoryDataStore _dataStore;

        public InMemoryQueryContextFactory(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource,
            [NotNull] IClrCollectionAccessorSource collectionAccessorSource,
            [NotNull] IClrAccessorSource<IClrPropertySetter> propertySetterSource,
            [NotNull] IInMemoryDataStore dataStore,
            [NotNull] ILoggerFactory loggerFactory)
            : base(stateManager, entityKeyFactorySource, collectionAccessorSource, propertySetterSource, loggerFactory)
        {
            Check.NotNull(dataStore, nameof(dataStore));

            _dataStore = dataStore;
        }

        public override QueryContext CreateQueryContext()
            => new InMemoryQueryContext(Logger, CreateQueryBuffer(), StateManager, _dataStore.Database);
    }
}
