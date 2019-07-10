// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion.Visitors
{
    public class PendingSelectorIncludeVisitor : ExpressionVisitor
    {
        private readonly bool _skipCollectionNavigations;
        private readonly bool _rewriteIncludes;

        public PendingSelectorIncludeVisitor(bool skipCollectionNavigations = true, bool rewriteIncludes = true)
        {
            _skipCollectionNavigations = skipCollectionNavigations;
            _rewriteIncludes = rewriteIncludes;
        }

        public virtual List<(NavigationTreeNode NavTreeNode, SourceMapping SourceMapping)> PendingIncludes { get; } =
            new List<(NavigationTreeNode, SourceMapping)>();

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            if (memberExpression.Expression is NavigationBindingExpression navigationBindingExpression
                && navigationBindingExpression.EntityType.FindProperty(memberExpression.Member) != null)
            {
                return memberExpression;
            }

            var newExpression = Visit(memberExpression.Expression);

            return newExpression != memberExpression.Expression
               ? Expression.MakeMemberAccess(newExpression, memberExpression.Member)
               : memberExpression;
        }

        protected override Expression VisitInvocation(InvocationExpression invocationExpression) => invocationExpression;
        protected override Expression VisitLambda<T>(Expression<T> lambdaExpression) => lambdaExpression;
        protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinaryExpression) => typeBinaryExpression;

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            => methodCallExpression.IsEFProperty()
            ? methodCallExpression
            : base.VisitMethodCall(methodCallExpression);

        protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            var newIfTrue = Visit(conditionalExpression.IfTrue);
            var newIfFalse = Visit(conditionalExpression.IfFalse);

            return conditionalExpression.Update(conditionalExpression.Test, newIfTrue, newIfFalse);
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
            => binaryExpression.NodeType == ExpressionType.Coalesce
                ? base.VisitBinary(binaryExpression)
                : binaryExpression;

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            // TODO: what about nested scenarios i.e. NavigationExpansionExpression inside pending selector? - add tests
            switch (extensionExpression)
            {
                case NavigationBindingExpression navigationBindingExpression:
                    ProcessEagerLoadedNavigations(
                        navigationBindingExpression.NavigationTreeNode,
                        navigationBindingExpression.EntityType,
                        navigationBindingExpression.SourceMapping);

                    // find all nodes and children UNTIL you find a collection in that subtree
                    // collection navigations will be converted to their own NavigationExpansionExpressions and their child includes will be applied when those NavigationExpansionExpressions are processed
                    return ProcessIncludes(
                        navigationBindingExpression,
                        navigationBindingExpression.NavigationTreeNode,
                        navigationBindingExpression.RootParameter,
                        navigationBindingExpression.SourceMapping,
                        rewrite: false);
                case CustomRootExpression _:
                case NavigationExpansionRootExpression _:
                case NavigationExpansionExpression _:
                    return extensionExpression;
            }

            return base.VisitExtension(extensionExpression);
        }

        private void ProcessEagerLoadedNavigations(NavigationTreeNode node, IEntityType entityType, SourceMapping sourceMapping)
        {
            foreach (var child in node.Children)
            {
                ProcessEagerLoadedNavigations(child, child.Navigation.GetTargetType(), sourceMapping);
            }

            var outboundNavigations
                = entityType.GetNavigations()
                    .Concat(entityType.GetDerivedNavigations())
                    .Where(n => n.IsEagerLoaded());

            foreach (var navigation in outboundNavigations)
            {
                var newNode = NavigationTreeNode.Create(sourceMapping, navigation, node, include: true);
                ProcessEagerLoadedNavigations(newNode, navigation.GetTargetType(), sourceMapping);
            }
        }

        private Expression ProcessIncludes(
            Expression caller,
            NavigationTreeNode node,
            ParameterExpression rootParameter,
            SourceMapping sourceMapping,
            bool rewrite)
        {
            if (node.ExpansionState != NavigationState.Complete
                && node.IncludeState == NavigationState.Pending
                && (!_skipCollectionNavigations || !node.IsCollection))
            {
                PendingIncludes.Add((node, sourceMapping));
            }

            var skipChildren = false;
            if (node.IsCollection)
            {
                skipChildren = _skipCollectionNavigations;
            }

            var included = caller;
            if (rewrite)
            {
                if (node.IsCollection)
                {
                    included = CollectionNavigationRewritingVisitor.CreateCollectionNavigationExpression(node, rootParameter, sourceMapping);
                }
                else
                {
                    var entityType = node.Navigation.GetTargetType();
                    included = new NavigationBindingExpression(rootParameter, node, entityType, sourceMapping, entityType.ClrType);
                }
            }

            if (!skipChildren)
            {
                foreach (var child in node.Children)
                {
                    if (child.IncludeState != NavigationState.Pending
                        && child.IncludeState != NavigationState.Delayed)
                    {
                        continue;
                    }

                    included = ProcessIncludes(included, child, rootParameter, sourceMapping, _rewriteIncludes);
                }
            }

            return rewrite
                ? new IncludeExpression(caller, included, node.Navigation)
                : included;
        }
    }
}
