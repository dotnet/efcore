// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Query
{
    public class RelationalQueryContextFactory : QueryContextFactory
    {
        private readonly IRelationalConnection _connection;
        private readonly DbContext _context;

        public RelationalQueryContextFactory(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource,
            [NotNull] IClrCollectionAccessorSource collectionAccessorSource,
            [NotNull] IClrAccessorSource<IClrPropertySetter> propertySetterSource,
            [NotNull] IRelationalConnection connection,
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] DbContext context)
            : base(stateManager, entityKeyFactorySource, collectionAccessorSource, propertySetterSource, loggerFactory)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(context, nameof(context));

            _connection = connection;
            _context = context;
        }

        public override QueryContext Create()
            => new RelationalQueryContext(Logger, CreateQueryBuffer(), _connection)
            {
                ContextType = _context.GetType()
            };
    }
}
