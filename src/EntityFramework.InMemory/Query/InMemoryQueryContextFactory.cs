// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.InMemory;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Query
{
    public class InMemoryQueryContextFactory : QueryContextFactory
    {
        private readonly IInMemoryDatabase _database;
        private readonly DbContext _context;

        public InMemoryQueryContextFactory(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource,
            [NotNull] IClrCollectionAccessorSource collectionAccessorSource,
            [NotNull] IClrAccessorSource<IClrPropertySetter> propertySetterSource,
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IInMemoryDatabase database,
            [NotNull] DbContext context)
            : base(stateManager, entityKeyFactorySource, collectionAccessorSource, propertySetterSource, loggerFactory)
        {
            Check.NotNull(database, nameof(database));
            Check.NotNull(context, nameof(context));

            _database = database;
            _context = context;
        }

        public override QueryContext Create()
            => new InMemoryQueryContext(Logger, CreateQueryBuffer(), _database.Store)
            {
                ContextType = _context.GetType()
            };
    }
}
