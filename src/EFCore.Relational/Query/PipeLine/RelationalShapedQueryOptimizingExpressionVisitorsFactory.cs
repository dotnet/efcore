// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalShapedQueryOptimizingExpressionVisitorsFactory : ShapedQueryOptimizingExpressionVisitorsFactory
    {
        public override ShapedQueryOptimizingExpressionVisitors Create(QueryCompilationContext2 queryCompilationContext)
        {
            return new RelationalShapedQueryOptimizingExpressionVisitors(queryCompilationContext);
        }
    }
}
