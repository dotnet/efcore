// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Query;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Data.Entity.Sqlite.Query;
using Microsoft.Data.Entity.Sqlite.Utilities;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteDataStore : RelationalDataStore
    {
        public SqliteDataStore(
            [NotNull] StateManager stateManager,
            [NotNull] ContextService<IModel> model,
            [NotNull] EntityKeyFactorySource entityKeyFactorySource,
            [NotNull] EntityMaterializerSource entityMaterializerSource,
            [NotNull] ClrCollectionAccessorSource collectionAccessorSource,
            [NotNull] ClrPropertySetterSource propertySetterSource,
            [NotNull] SqliteConnection connection,
            [NotNull] SqliteCommandBatchPreparer batchPreparer,
            [NotNull] SqliteBatchExecutor batchExecutor,
            [NotNull] ILoggerFactory loggerFactory)
            : base(stateManager, model, entityKeyFactorySource, entityMaterializerSource,
                collectionAccessorSource, propertySetterSource, connection, batchPreparer, batchExecutor, loggerFactory)
        {
        }

        protected override RelationalValueReaderFactory ValueReaderFactory
        {
            get { return new RelationalObjectArrayValueReaderFactory(); }
        }

        protected override RelationalQueryCompilationContext CreateQueryCompilationContext(
            ILinqOperatorProvider linqOperatorProvider,
            IResultOperatorHandler resultOperatorHandler,
            IQueryMethodProvider queryMethodProvider,
            IMethodCallTranslator methodCallTranslator)
        {
            Check.NotNull(linqOperatorProvider, "linqOperatorProvider");
            Check.NotNull(resultOperatorHandler, "resultOperatorHandler");
            Check.NotNull(queryMethodProvider, "queryMethodProvider");

            return new SqliteQueryCompilationContext(
                Model,
                Logger,
                linqOperatorProvider,
                resultOperatorHandler,
                EntityMaterializerSource,
                queryMethodProvider,
                methodCallTranslator);
        }
    }
}
