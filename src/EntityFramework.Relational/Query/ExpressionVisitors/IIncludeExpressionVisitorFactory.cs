// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public interface IIncludeExpressionVisitorFactory
    {
        ExpressionVisitor Create(
            [NotNull] IQuerySource querySource,
            [NotNull] IReadOnlyList<INavigation> navigationPath,
            [NotNull] RelationalQueryCompilationContext queryCompilationContext,
            [NotNull] IReadOnlyList<int> readerIndexes,
            bool querySourceRequiresTracking);
    }
}
