// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public interface IQueryFlatteningExpressionVisitorFactory
    {
        ExpressionVisitor Create(
            [NotNull] IQuerySource outerQuerySource,
            [NotNull] IQuerySource innerQuerySource,
            [NotNull] RelationalQueryCompilationContext relationalQueryCompilationContext,
            int readerOffset,
            [NotNull] MethodInfo operatorToFlatten);
    }
}
