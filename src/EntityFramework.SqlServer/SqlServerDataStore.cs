// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Query;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Data.Entity.SqlServer.Query;
using Microsoft.Data.Entity.SqlServer.Update;
using Microsoft.Data.Entity.SqlServer.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerDataStore : RelationalDataStore
    {
        public SqlServerDataStore(
            [NotNull] DbContextConfiguration configuration,
            [NotNull] SqlServerConnection connection,
            [NotNull] SqlServerCommandBatchPreparer batchPreparer,
            [NotNull] SqlServerBatchExecutor batchExecutor, 
            [NotNull] ILoggerFactory loggerFactory)
            : base(configuration, connection, batchPreparer, batchExecutor, loggerFactory)
        {
        }

        protected override RelationalValueReaderFactory ValueReaderFactory
        {
            get { return new RelationalObjectArrayValueReaderFactory(); }
        }

        protected override RelationalQueryCompilationContext CreateQueryCompilationContext(
            ILinqOperatorProvider linqOperatorProvider,
            IResultOperatorHandler resultOperatorHandler,
            IQueryMethodProvider enumerableMethodProvider,
            IMethodCallTranslator methodCallTranslator)
        {
            Check.NotNull(linqOperatorProvider, "linqOperatorProvider");
            Check.NotNull(resultOperatorHandler, "resultOperatorHandler");
            Check.NotNull(enumerableMethodProvider, "enumerableMethodProvider");
            Check.NotNull(methodCallTranslator, "methodCallTranslator");

            return new SqlServerQueryCompilationContext(
                Model, 
                Logger, 
                linqOperatorProvider, 
                resultOperatorHandler, 
                EntityMaterializerSource,
                enumerableMethodProvider,
                methodCallTranslator);
        }
    }
}
