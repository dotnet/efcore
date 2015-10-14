// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public interface IQueryFlattenerFactory
    {
        QueryFlattener Create(
            [NotNull] RelationalQueryCompilationContext relationalQueryCompilationContext,
            [NotNull] MethodInfo operatorToFlatten,
            int readerOffset);
    }
}
