// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors.Internal
{
    public class QueryFlattenerFactory : IQueryFlattenerFactory
    {
        public virtual QueryFlattener Create(
            IQuerySource querySource,
            RelationalQueryCompilationContext relationalQueryCompilationContext,
            MethodInfo operatorToFlatten,
            int readerOffset)
            => new QueryFlattener(
                querySource,
                relationalQueryCompilationContext,
                operatorToFlatten,
                readerOffset);
    }
}
