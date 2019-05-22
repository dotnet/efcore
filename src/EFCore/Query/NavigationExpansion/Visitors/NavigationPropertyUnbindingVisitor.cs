// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion.Visitors
{
    public class NavigationPropertyUnbindingVisitor : ExpressionVisitor
    {
        private ParameterExpression _rootParameter;

        public NavigationPropertyUnbindingVisitor(ParameterExpression rootParameter)
        {
            _rootParameter = rootParameter;
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
                || extensionExpression is NavigationExpansionExpression
                || extensionExpression is IncludeExpression)
            {
                var result = new NavigationExpansionReducingVisitor().Visit(extensionExpression);

                return Visit(result);
            }

            return base.VisitExtension(extensionExpression);
        }
    }
}
