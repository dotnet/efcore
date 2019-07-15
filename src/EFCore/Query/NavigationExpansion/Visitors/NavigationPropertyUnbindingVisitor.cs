// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion.Visitors
{
    public class NavigationPropertyUnbindingVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _rootParameter;

        public NavigationPropertyUnbindingVisitor(ParameterExpression rootParameter)
        {
            _rootParameter = rootParameter;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case NavigationBindingExpression navigationBindingExpression
                    when navigationBindingExpression.RootParameter == _rootParameter:
                {
                    var node = navigationBindingExpression.NavigationTreeNode;
                    var navigations = new List<INavigation>();
                    while (node != null)
                    {
                        if (node.Navigation != null)
                        {
                            navigations.Add(node.Navigation);
                        }
                        node = node.Parent;
                    }

                    var result = navigationBindingExpression.RootParameter.BuildPropertyAccess(
                        navigationBindingExpression.NavigationTreeNode.ToMapping,
                        navigations.Count == navigationBindingExpression.NavigationTreeNode.ToMapping.Count ? navigations : null);

                    return result.Type != navigationBindingExpression.Type
                        ? Expression.Convert(result, navigationBindingExpression.Type)
                        : result;
                }

                case CustomRootExpression customRootExpression
                    when customRootExpression.RootParameter == _rootParameter:
                {
                    var result = _rootParameter.BuildPropertyAccess(customRootExpression.Mapping);

                    return result.Type != customRootExpression.Type
                        ? Expression.Convert(result, customRootExpression.Type)
                        : result;
                }

                case NavigationExpansionRootExpression _:
                case NavigationExpansionExpression _:
                {
                    var result = new NavigationExpansionReducingVisitor().Visit(extensionExpression);

                    return Visit(result);
                }

                default:
                    return base.VisitExtension(extensionExpression);
            }
        }
    }
}
