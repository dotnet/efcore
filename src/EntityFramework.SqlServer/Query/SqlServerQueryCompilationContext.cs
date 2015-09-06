// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.ExpressionVisitors;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Query
{
    public class SqlServerQueryCompilationContext : RelationalQueryCompilationContext
    {
        public SqlServerQueryCompilationContext(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IEntityQueryModelVisitorFactory entityQueryModelVisitorFactory,
            [NotNull] IRequiresMaterializationExpressionVisitorFactory requiresMaterializationExpressionVisitorFactory,
            [NotNull] IDatabase database,
            [NotNull] ILinqOperatorProvider linqOpeartorProvider,
            [NotNull] IQueryMethodProvider queryMethodProvider)
            : base(
                Check.NotNull(loggerFactory, nameof(loggerFactory)),
                Check.NotNull(entityQueryModelVisitorFactory, nameof(entityQueryModelVisitorFactory)),
                Check.NotNull(requiresMaterializationExpressionVisitorFactory, nameof(requiresMaterializationExpressionVisitorFactory)),
                Check.NotNull(database, nameof(database)),
                Check.NotNull(linqOpeartorProvider, nameof(linqOpeartorProvider)),
                Check.NotNull(queryMethodProvider, nameof(queryMethodProvider)))
        {
        }

        public override bool IsCrossApplySupported => true;
    }
}