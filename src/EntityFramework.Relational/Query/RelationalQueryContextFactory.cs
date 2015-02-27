// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class RelationalQueryContextFactory : QueryContextFactory, IRelationalQueryContextFactory
    {
        private readonly IRelationalConnection _connection;

        public RelationalQueryContextFactory(
            [NotNull] StateManager stateManager,
            [NotNull] EntityKeyFactorySource entityKeyFactorySource,
            [NotNull] ClrCollectionAccessorSource collectionAccessorSource,
            [NotNull] ClrPropertySetterSource propertySetterSource,
            [NotNull] IRelationalConnection connection,
            [NotNull] ILoggerFactory loggerFactory)
            : base(stateManager, entityKeyFactorySource, collectionAccessorSource, propertySetterSource, loggerFactory)
        {
            Check.NotNull(connection, nameof(connection));

            _connection = connection;
        }

        protected virtual RelationalValueReaderFactory ValueReaderFactory => new RelationalTypedValueReaderFactory();

        public override QueryContext CreateQueryContext() => new RelationalQueryContext(Logger, CreateQueryBuffer(), _connection, ValueReaderFactory);
    }
}
