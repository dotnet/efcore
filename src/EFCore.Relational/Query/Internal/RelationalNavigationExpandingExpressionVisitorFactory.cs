// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class RelationalNavigationExpandingExpressionVisitorFactory : INavigationExpandingExpressionVisitorFactory
    {
        public virtual NavigationExpandingExpressionVisitor Create(
            QueryTranslationPreprocessor queryTranslationPreprocessor,
            QueryCompilationContext queryCompilationContext, IEvaluatableExpressionFilter evaluatableExpressionFilter)
        {
            return new RelationalNavigationExpandingExpressionVisitor(queryTranslationPreprocessor, queryCompilationContext, evaluatableExpressionFilter);
        }
    }
}
