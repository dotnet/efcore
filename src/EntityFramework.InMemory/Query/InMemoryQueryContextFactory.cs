// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.InMemory.Query
{
    public class InMemoryQueryContextFactory : QueryContextFactory
    {
        private readonly IInMemoryDatabase _database;

        public InMemoryQueryContextFactory(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource,
            [NotNull] IClrCollectionAccessorSource collectionAccessorSource,
            [NotNull] IClrAccessorSource<IClrPropertySetter> propertySetterSource,
            [NotNull] IInMemoryDatabase database,
            [NotNull] ILoggerFactory loggerFactory)
            : base(stateManager, entityKeyFactorySource, collectionAccessorSource, propertySetterSource, loggerFactory)
        {
            Check.NotNull(database, nameof(database));

            _database = database;
        }

        public override QueryContext Create() => new InMemoryQueryContext(Logger, CreateQueryBuffer(), _database.Store);
    }
}
