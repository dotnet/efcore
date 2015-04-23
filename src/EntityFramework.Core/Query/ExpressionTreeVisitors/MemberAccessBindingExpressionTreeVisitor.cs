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
                    newExpression
                        = Expression.Call(
                            _queryModelVisitor.LinqOperatorProvider.ToOrdered
                                .MakeGenericMethod(newExpression.Type.GenericTypeArguments[0]),
                            newExpression);
                }
                else if (genericTypeDefinition == typeof(IAsyncEnumerable<>))
                {
                    newExpression
                        = Expression.Call(
                            _queryModelVisitor.LinqOperatorProvider.ToEnumerable
                                .MakeGenericMethod(newExpression.Type.GenericTypeArguments[0]),
                            newExpression);
                }
            }

            return newExpression;
        }

        protected override Expression VisitMemberExpression(MemberExpression memberExpression)
        {
            var newExpression = VisitExpression(memberExpression.Expression);

            if (newExpression != memberExpression.Expression)
            {
                if (newExpression.Type == typeof(ValueBuffer))
                {
                    return _queryModelVisitor.BindMemberToValueBuffer(memberExpression, newExpression)
                           ?? memberExpression;
                }

                var member = memberExpression.Member;
                var typeInfo = newExpression.Type.GetTypeInfo();

                if (typeInfo.IsGenericType
                    && typeInfo.GetGenericTypeDefinition() == typeof(IAsyncGrouping<,>))
                {
                    member = typeInfo.GetDeclaredProperty("Key");
                }

                return Expression.MakeMemberAccess(newExpression, member);
            }

            return memberExpression;
        }

        protected override Expression VisitMethodCallExpression(MethodCallExpression methodCallExpression)
        {
            MethodCallExpression newExpression = null;

            if (methodCallExpression.Method.IsGenericMethod
                && ReferenceEquals(
                    methodCallExpression.Method.GetGenericMethodDefinition(),
                    QueryExtensions.PropertyMethodInfo))
            {
                var newArguments
                    = VisitAndConvert(methodCallExpression.Arguments, "VisitMethodCallExpression");

                if (newArguments[0].Type == typeof(ValueBuffer))
                {
                    // Compensate for ValueBuffer being a struct, and hence not compatible with Object method
                    newExpression
                        = Expression.Call(
                            QueryExtensions.ValueBufferPropertyMethodInfo
                                .MakeGenericMethod(methodCallExpression.Method.GetGenericArguments()),
                            newArguments);
                }
            }

            if (newExpression == null)
            {
                newExpression 
                    = (MethodCallExpression)base.VisitMethodCallExpression(methodCallExpression);
            }

            if (newExpression != methodCallExpression
                && newExpression.Arguments.Any()
                && newExpression.Arguments[0].Type == typeof(ValueBuffer))
            {
                return
                    _queryModelVisitor
                        .BindMethodCallToValueBuffer(methodCallExpression, newExpression.Arguments[0])
                    ?? newExpression;
            }

            return _queryModelVisitor
                .BindMethodCallExpression(
                    methodCallExpression,
                    (property, _) => Expression.Call(
                        _getValueMethodInfo.MakeGenericMethod(newExpression.Method.GetGenericArguments()[0]),
                        EntityQueryModelVisitor.QueryContextParameter,
                        newExpression.Arguments[0],
                        Expression.Constant(property)))
                   ?? newExpression;
        }

        private static readonly MethodInfo _getValueMethodInfo
            = typeof(MemberAccessBindingExpressionTreeVisitor)
                .GetTypeInfo().GetDeclaredMethod(nameof(GetValue));

        [UsedImplicitly]
        private static T GetValue<T>(QueryContext queryContext, object entity, IProperty property)
            => (T)queryContext.QueryBuffer.GetPropertyValue(entity, property);
    }
}
