// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion.Visitors
{
    public class NavigationPropertyUnbindingVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _rootParameter;
        private readonly NavigationExpandingVisitor _navigationExpandingVisitor;
        private readonly QueryCompilationContext _queryCompilationContext;

        public NavigationPropertyUnbindingVisitor(
            ParameterExpression rootParameter,
            NavigationExpandingVisitor navigationExpandingVisitor,
            QueryCompilationContext queryCompilationContext)
        {
            _rootParameter = rootParameter;
            _navigationExpandingVisitor = navigationExpandingVisitor;
            _queryCompilationContext = queryCompilationContext;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is NavigationBindingExpression navigationBindingExpression
                && navigationBindingExpression.RootParameter == _rootParameter)
            {
                var result = navigationBindingExpression.RootParameter.BuildPropertyAccess(navigationBindingExpression.NavigationTreeNode.ToMapping);

                return result.Type != navigationBindingExpression.Type
                    ? Expression.Convert(result, navigationBindingExpression.Type)
                    : result;
            }

            if (extensionExpression is CustomRootExpression customRootExpression
                && customRootExpression.RootParameter == _rootParameter)
            {
                var result = _rootParameter.BuildPropertyAccess(customRootExpression.Mapping);

                return result.Type != customRootExpression.Type
                    ? Expression.Convert(result, customRootExpression.Type)
                    : result;
            }

            if (extensionExpression is NavigationExpansionRootExpression
                || extensionExpression is NavigationExpansionExpression)
            {
                var result = new NavigationExpansionReducingVisitor(_navigationExpandingVisitor, _queryCompilationContext).Visit(extensionExpression);

                return Visit(result);
            }

            return base.VisitExtension(extensionExpression);
        }
    }
}
