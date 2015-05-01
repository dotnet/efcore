// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Relational.Query;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Sqlite.Query
{
    public class SqliteQueryContextFactory : RelationalQueryContextFactory, ISqliteQueryContextFactory
    {
        public SqliteQueryContextFactory(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource,
            [NotNull] IClrCollectionAccessorSource collectionAccessorSource,
            [NotNull] IClrAccessorSource<IClrPropertySetter> propertySetterSource,
            [NotNull] ISqliteConnection connection,
            [NotNull] ILoggerFactory loggerFactory)
            : base(
                stateManager,
                entityKeyFactorySource,
                collectionAccessorSource,
                propertySetterSource,
                connection,
                loggerFactory)
        {
        }
    }
}
