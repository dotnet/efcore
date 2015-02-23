// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Query;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer.Query
{
    public class SqlServerQueryContextFactory : RelationalQueryContextFactory
    {
        public SqlServerQueryContextFactory(
            [NotNull] StateManager stateManager,
            [NotNull] EntityKeyFactorySource entityKeyFactorySource,
            [NotNull] ClrCollectionAccessorSource collectionAccessorSource,
            [NotNull] ClrPropertySetterSource propertySetterSource,
            [NotNull] SqlServerConnection connection,
            [NotNull] ILoggerFactory loggerFactory)
            : base(stateManager, entityKeyFactorySource, collectionAccessorSource, propertySetterSource, connection, loggerFactory)
        {
        }

        protected override RelationalValueReaderFactory ValueReaderFactory => new RelationalObjectArrayValueReaderFactory();
    }
}
