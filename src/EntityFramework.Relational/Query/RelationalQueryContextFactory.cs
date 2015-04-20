// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class RelationalQueryContextFactory : QueryContextFactory, IRelationalQueryContextFactory
    {
        private readonly IRelationalValueReaderFactory _valueReaderFactory;
        private readonly IRelationalConnection _connection;

        public RelationalQueryContextFactory(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource,
            [NotNull] IClrCollectionAccessorSource collectionAccessorSource,
            [NotNull] IClrAccessorSource<IClrPropertySetter> propertySetterSource,
            [NotNull] IRelationalValueReaderFactory valueReaderFactory,
            [NotNull] IRelationalConnection connection,
            [NotNull] ILoggerFactory loggerFactory)
            : base(stateManager, entityKeyFactorySource, collectionAccessorSource, propertySetterSource, loggerFactory)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(valueReaderFactory, nameof(valueReaderFactory));

            _valueReaderFactory = valueReaderFactory;
            _connection = connection;
        }

        public override QueryContext CreateQueryContext()
            => new RelationalQueryContext(Logger, CreateQueryBuffer(), StateManager, _connection, _valueReaderFactory);
    }
}
