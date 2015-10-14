// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class QueryFlattenerFactory : IQueryFlattenerFactory
    {
        public virtual QueryFlattener Create(
            RelationalQueryCompilationContext relationalQueryCompilationContext,
            MethodInfo operatorToFlatten,
            int readerOffset)
            => new QueryFlattener(
                relationalQueryCompilationContext,
                operatorToFlatten,
                readerOffset);
    }
}
