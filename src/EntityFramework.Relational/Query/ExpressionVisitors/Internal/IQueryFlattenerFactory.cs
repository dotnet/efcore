// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors.Internal
{
    public interface IQueryFlattenerFactory
    {
        QueryFlattener Create(
            [NotNull] IQuerySource querySource,
            [NotNull] RelationalQueryCompilationContext relationalQueryCompilationContext,
            [NotNull] MethodInfo operatorToFlatten,
            int readerOffset);
    }
}
