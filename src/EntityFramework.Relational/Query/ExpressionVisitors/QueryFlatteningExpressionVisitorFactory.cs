// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class QueryFlatteningExpressionVisitorFactory : IQueryFlatteningExpressionVisitorFactory
    {
        public virtual ExpressionVisitor Create(
            [NotNull] IQuerySource outerQuerySource,
            [NotNull] IQuerySource innerQuerySource,
            [NotNull] RelationalQueryCompilationContext relationalQueryCompilationContext,
            int readerOffset,
            [NotNull] MethodInfo operatorToFlatten)
            => new QueryFlatteningExpressionVisitor(
                outerQuerySource,
                innerQuerySource,
                relationalQueryCompilationContext,
                readerOffset,
                operatorToFlatten);
    }
}
