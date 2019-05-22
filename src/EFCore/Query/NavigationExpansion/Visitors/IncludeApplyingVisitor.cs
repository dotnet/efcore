// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion.Visitors
{
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.EntityFrameworkCore.Extensions.Internal;

    public class PendingSelectorIncludeRewriter : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression memberExpression) => memberExpression;
        protected override Expression VisitInvocation(InvocationExpression invocationExpression) => invocationExpression;
        protected override Expression VisitLambda<T>(Expression<T> lambdaExpression) => lambdaExpression;
        protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinaryExpression) => typeBinaryExpression;

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            => methodCallExpression.Method.IsEFPropertyMethod()
            ? methodCallExpression
            : base.VisitMethodCall(methodCallExpression);

        protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            var newIfTrue = Visit(conditionalExpression.IfTrue);
            var newIfFalse = Visit(conditionalExpression.IfFalse);

            return newIfTrue != conditionalExpression.IfTrue || newIfFalse != conditionalExpression.IfFalse
                ? conditionalExpression.Update(conditionalExpression.Test, newIfTrue, newIfFalse)
                : conditionalExpression;
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            return binaryExpression.NodeType == ExpressionType.Coalesce
                ? base.VisitBinary(binaryExpression)
                : binaryExpression;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is NavigationBindingExpression navigationBindingExpression)
            {
                var result = (Expression)navigationBindingExpression;

                foreach (var child in navigationBindingExpression.NavigationTreeNode.Children.Where(n => n.Included == NavigationTreeNodeIncludeMode.ReferencePending || n.Included == NavigationTreeNodeIncludeMode.Collection))
                {
                    result = CreateIncludeCall(result, child, navigationBindingExpression.RootParameter, navigationBindingExpression.SourceMapping);
                }

                return result;
            }

            if (extensionExpression is CustomRootExpression customRootExpression)
            {
                return customRootExpression;
            }

            if (extensionExpression is NavigationExpansionRootExpression expansionRootExpression)
            {
                return expansionRootExpression;
            }

            if (extensionExpression is NavigationExpansionExpression navigationExpansionExpression)
            {
                return navigationExpansionExpression;
            }

            return base.VisitExtension(extensionExpression);
        }

        private IncludeExpression CreateIncludeCall(Expression caller, NavigationTreeNode node, ParameterExpression rootParameter, SourceMapping sourceMapping)
            => node.Navigation.IsCollection()
            ? CreateIncludeCollectionCall(caller, node, rootParameter, sourceMapping)
            : CreateIncludeReferenceCall(caller, node, rootParameter, sourceMapping);

        private IncludeExpression CreateIncludeReferenceCall(Expression caller, NavigationTreeNode node, ParameterExpression rootParameter, SourceMapping sourceMapping)
        {
            var entityType = node.Navigation.GetTargetType();
            var included = (Expression)new NavigationBindingExpression(rootParameter, node, entityType, sourceMapping, entityType.ClrType);

            foreach (var child in node.Children.Where(n => n.Included == NavigationTreeNodeIncludeMode.ReferencePending || n.Included == NavigationTreeNodeIncludeMode.Collection))
            {
                included = CreateIncludeCall(included, child, rootParameter, sourceMapping);
            }

            return new IncludeExpression(caller, included, node.Navigation);
        }

        private IncludeExpression CreateIncludeCollectionCall(Expression caller, NavigationTreeNode node, ParameterExpression rootParameter, SourceMapping sourceMapping)
        {
            var included = CollectionNavigationRewritingVisitor.CreateCollectionNavigationExpression(node, rootParameter, sourceMapping);

            return new IncludeExpression(caller, included, node.Navigation);
        }
    }
}
