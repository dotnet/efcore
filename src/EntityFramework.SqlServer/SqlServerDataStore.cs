// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Query;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Data.Entity.SqlServer.Query;
using Microsoft.Data.Entity.SqlServer.Update;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerDataStore : RelationalDataStore
    {
        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected SqlServerDataStore()
        {
        }

        public SqlServerDataStore(
            [NotNull] StateManager stateManager,
            [NotNull] DbContextService<IModel> model,
            [NotNull] EntityKeyFactorySource entityKeyFactorySource,
            [NotNull] EntityMaterializerSource entityMaterializerSource,
            [NotNull] ClrCollectionAccessorSource collectionAccessorSource,
            [NotNull] ClrPropertySetterSource propertySetterSource,
            [NotNull] SqlServerConnection connection,
            [NotNull] SqlServerCommandBatchPreparer batchPreparer,
            [NotNull] SqlServerBatchExecutor batchExecutor,
            [NotNull] DbContextService<IDbContextOptions> options,
            [NotNull] ILoggerFactory loggerFactory)
            : base(
                Check.NotNull(stateManager, nameof(stateManager)),
                Check.NotNull(model, nameof(model)),
                Check.NotNull(entityKeyFactorySource, nameof(entityKeyFactorySource)),
                Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource)),
                Check.NotNull(collectionAccessorSource, nameof(collectionAccessorSource)),
                Check.NotNull(propertySetterSource, nameof(propertySetterSource)),
                Check.NotNull(connection, nameof(connection)),
                Check.NotNull(batchPreparer, nameof(batchPreparer)),
                Check.NotNull(batchExecutor, nameof(batchExecutor)),
                Check.NotNull(options, nameof(options)),
                Check.NotNull(loggerFactory, nameof(loggerFactory)))
        {
        }

        protected override RelationalValueReaderFactory ValueReaderFactory => new RelationalObjectArrayValueReaderFactory();

        protected override RelationalQueryCompilationContext CreateQueryCompilationContext(
            ILinqOperatorProvider linqOperatorProvider,
            IResultOperatorHandler resultOperatorHandler,
            IQueryMethodProvider enumerableMethodProvider,
            IMethodCallTranslator methodCallTranslator)
        {
            Check.NotNull(linqOperatorProvider, nameof(linqOperatorProvider));
            Check.NotNull(resultOperatorHandler, nameof(resultOperatorHandler));
            Check.NotNull(enumerableMethodProvider, nameof(enumerableMethodProvider));
            Check.NotNull(methodCallTranslator, nameof(methodCallTranslator));

            return new SqlServerQueryCompilationContext(
                Model,
                Logger,
                linqOperatorProvider,
                resultOperatorHandler,
                EntityMaterializerSource,
                EntityKeyFactorySource,
                enumerableMethodProvider,
                methodCallTranslator);
        }
    }
}
