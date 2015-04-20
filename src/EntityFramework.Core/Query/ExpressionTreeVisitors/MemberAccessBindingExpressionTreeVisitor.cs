// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Query.ExpressionTreeVisitors
{
    public class MemberAccessBindingExpressionTreeVisitor : ReferenceReplacingExpressionTreeVisitor
    {
        private readonly EntityQueryModelVisitor _queryModelVisitor;
        private readonly bool _inProjection;

        public MemberAccessBindingExpressionTreeVisitor(
            [NotNull] QuerySourceMapping querySourceMapping,
            [NotNull] EntityQueryModelVisitor queryModelVisitor,
            bool inProjection)
            : base(
                Check.NotNull(querySourceMapping, nameof(querySourceMapping)),
                throwOnUnmappedReferences: false)
        {
            Check.NotNull(queryModelVisitor, nameof(queryModelVisitor));

            _queryModelVisitor = queryModelVisitor;
            _inProjection = inProjection;
        }

        protected override Expression VisitSubQueryExpression(SubQueryExpression subQueryExpression)
        {
            subQueryExpression.QueryModel.TransformExpressions(VisitExpression);

            return subQueryExpression;
        }

        protected override Expression VisitQuerySourceReferenceExpression(QuerySourceReferenceExpression expression)
        {
            var newExpression
                = QuerySourceMapping.ContainsMapping(expression.ReferencedQuerySource)
                    ? QuerySourceMapping.GetExpression(expression.ReferencedQuerySource)
                    : base.VisitQuerySourceReferenceExpression(expression);

            if (_inProjection
                && newExpression.Type.IsConstructedGenericType)
            {
                var genericTypeDefinition = newExpression.Type.GetGenericTypeDefinition();

                if (genericTypeDefinition == typeof(IOrderedAsyncEnumerable<>))
                {
                    newExpression = TryUnwrap(newExpression);

                    newExpression
                        = Expression.Call(
                            _queryModelVisitor.LinqOperatorProvider.ToOrdered
                                .MakeGenericMethod(newExpression.Type.GenericTypeArguments[0]),
                            newExpression);
                }
                else if (genericTypeDefinition == typeof(IAsyncEnumerable<>))
                {
                    newExpression = TryUnwrap(newExpression);

                    newExpression
                        = Expression.Call(
                            _queryModelVisitor.LinqOperatorProvider.ToEnumerable
                                .MakeGenericMethod(newExpression.Type.GenericTypeArguments[0]),
                            newExpression);
                }
                else if (genericTypeDefinition == typeof(IEnumerable<>))
                {
                    newExpression = TryUnwrap(newExpression);
                }
            }

            return newExpression;
        }

        private Expression TryUnwrap(Expression expression)
        {
            var sequenceType = expression.Type.GenericTypeArguments[0];

            if (sequenceType.IsConstructedGenericType
                && sequenceType.GetGenericTypeDefinition() == typeof(QuerySourceScope<>))
            {
                expression
                    = Expression.Call(
                        _queryModelVisitor.LinqOperatorProvider.UnwrapQueryResults
                            .MakeGenericMethod(sequenceType.GenericTypeArguments[0]),
                        expression);
            }

            return expression;
        }

        protected override Expression VisitMemberExpression([NotNull] MemberExpression memberExpression)
        {
            var newExpression = VisitExpression(memberExpression.Expression);

            if (newExpression != memberExpression.Expression)
            {
                if (newExpression.Type == typeof(IValueReader))
                {
                    return _queryModelVisitor.BindMemberToValueReader(memberExpression, newExpression)
                           ?? memberExpression;
                }

                var member = memberExpression.Member;
                var typeInfo = newExpression.Type.GetTypeInfo();

                if (typeInfo.IsGenericType)
                {
                    var genericTypeDefinition = typeInfo.GetGenericTypeDefinition();
                    var asyncGrouping = genericTypeDefinition == typeof(IAsyncGrouping<,>);

                    if (genericTypeDefinition == typeof(IGrouping<,>)
                        || asyncGrouping)
                    {
                        newExpression
                            = Expression.Call(
                                _queryModelVisitor.LinqOperatorProvider
                                    .UnwrapGrouping
                                    .MakeGenericMethod(
                                        typeInfo.GenericTypeArguments[0],
                                        typeInfo.GenericTypeArguments[1]
                                            .GetTypeInfo()
                                            .GenericTypeArguments[0]),
                                newExpression);

                        if (asyncGrouping)
                        {
                            member = newExpression.Type.GetTypeInfo().GetDeclaredProperty("Key");
                        }
                    }
                    else if (genericTypeDefinition == typeof(QuerySourceScope<>))
                    {
                        newExpression
                            = Expression.Convert(
                                newExpression,
                                typeInfo.GenericTypeArguments[0]);
                    }
                }

                return Expression.MakeMemberAccess(newExpression, member);
            }

            return memberExpression;
        }

        protected override Expression VisitMethodCallExpression([NotNull] MethodCallExpression methodCallExpression)
        {
            var newExpression
                = (MethodCallExpression)base.VisitMethodCallExpression(methodCallExpression);

            if (newExpression != methodCallExpression
                && newExpression.Arguments.Any()
                && newExpression.Arguments[0].Type == typeof(IValueReader))
            {
                return
                    _queryModelVisitor
                        .BindMethodCallToValueReader(methodCallExpression, newExpression.Arguments[0])
                    ?? newExpression;
            }

            return _queryModelVisitor
                .BindMethodCallExpression(
                    methodCallExpression,
                    (property, _) =>
                        {
                            var querySourceScopeExpression
                                = new QuerySourceScopeFindingExpressionTreeVisitor()
                                    .Find(newExpression.Arguments[0]);

                            return Expression.Call(
                                _getValueMethodInfo.MakeGenericMethod(newExpression.Method.GetGenericArguments()[0]),
                                EntityQueryModelVisitor.QueryContextParameter,
                                newExpression.Arguments[0],
                                querySourceScopeExpression,
                                Expression.Constant(property));
                        })
                   ?? newExpression;
        }

        private class QuerySourceScopeFindingExpressionTreeVisitor : ExpressionTreeVisitor
        {
            private Expression _result;

            public Expression Find(Expression expression)
            {
                _result = null;

                VisitExpression(expression);

                return _result;
            }

            public override Expression VisitExpression(Expression expression)
            {
                if (_result == null
                    && expression != null)
                {
                    if ((expression.Type.IsConstructedGenericType
                         && expression.Type.GetGenericTypeDefinition() == typeof(QuerySourceScope<>))
                        || expression.Type == typeof(QuerySourceScope))
                    {
                        _result = expression;
                    }
                    else
                    {
                        return base.VisitExpression(expression);
                    }
                }

                return expression;
            }
        }

        private static readonly MethodInfo _getValueMethodInfo
            = typeof(MemberAccessBindingExpressionTreeVisitor)
                .GetTypeInfo().GetDeclaredMethod(nameof(GetValue));

        [UsedImplicitly]
        private static T GetValue<T>(
            QueryContext queryContext,
            object entity,
            QuerySourceScope querySourceScope,
            IProperty property)
        {
            return (T)queryContext.GetPropertyValue(entity, querySourceScope, property);
        }
    }
}
