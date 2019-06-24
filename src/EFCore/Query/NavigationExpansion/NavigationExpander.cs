// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.NavigationExpansion.Visitors;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion
{
    public class NavigationExpander
    {
        private readonly QueryCompilationContext _queryCompilationContext;

        public NavigationExpander([NotNull] QueryCompilationContext queryCompilationContext)
        {
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));

            _queryCompilationContext = queryCompilationContext;
        }

        public virtual Expression ExpandNavigations(Expression expression)
        {
            var navigationExpandingVisitor = new NavigationExpandingVisitor(_queryCompilationContext);
            var newExpression = navigationExpandingVisitor.Visit(expression);
            newExpression = new NavigationExpansionReducingVisitor(navigationExpandingVisitor, _queryCompilationContext).Visit(newExpression);

            return newExpression;
        }
    }
}
