// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Extensions.Internal;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion.Visitors
{

    public class PendingIncludeFindingVisitor : ExpressionVisitor
    {
        public virtual Dictionary<NavigationTreeNode, SourceMapping> PendingIncludes { get; } = new Dictionary<NavigationTreeNode, SourceMapping>();

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
            Visit(conditionalExpression.IfTrue);
            Visit(conditionalExpression.IfFalse);

            return conditionalExpression;
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            return binaryExpression.NodeType == ExpressionType.Coalesce
                ? base.VisitBinary(binaryExpression)
                : binaryExpression;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            // TODO: what about nested scenarios i.e. NavigationExpansionExpression inside pending selector? - add tests
            if (extensionExpression is NavigationBindingExpression navigationBindingExpression)
            {
                // find all nodes and children UNTIL you find a collection in that subtree
                // collection navigations will be converted to their own NavigationExpansionExpressions and their child includes will be applied when those NavigationExpansionExpressions are processed
                FindPendingReferenceIncludes(navigationBindingExpression.NavigationTreeNode, navigationBindingExpression.SourceMapping);

                return navigationBindingExpression;
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

        private void FindPendingReferenceIncludes(NavigationTreeNode node, SourceMapping sourceMapping)
        {
            if (node.Navigation != null && node.Navigation.IsCollection())
            {
                return;
            }

            if (node.Included == NavigationTreeNodeIncludeMode.ReferencePending && node.ExpansionMode != NavigationTreeNodeExpansionMode.ReferenceComplete)
            {
                PendingIncludes[node] = sourceMapping;
            }

            foreach (var child in node.Children)
            {
                FindPendingReferenceIncludes(child, sourceMapping);
            }
        }
    }
}
