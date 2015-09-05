// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors.Internal
{
    public class MemberAccessBindingExpressionVisitor : RelinqExpressionVisitor
    {
        private readonly QuerySourceMapping _querySourceMapping;
        private readonly EntityQueryModelVisitor _queryModelVisitor;
        private readonly bool _inProjection;

        public MemberAccessBindingExpressionVisitor(
            [NotNull] QuerySourceMapping querySourceMapping,
            [NotNull] EntityQueryModelVisitor queryModelVisitor,
            bool inProjection)
        {
            _querySourceMapping = querySourceMapping;
            _queryModelVisitor = queryModelVisitor;
            _inProjection = inProjection;
        }

        protected override Expression VisitNew(NewExpression newExpression)
        {
            var newArguments = Visit(newExpression.Arguments).ToList();

            for (var i = 0; i < newArguments.Count; i++)
            {
                if (newArguments[i].Type == typeof(ValueBuffer))
                {
                    newArguments[i]
                        = _queryModelVisitor
                            .BindReadValueMethod(newExpression.Arguments[i].Type, newArguments[i], 0);
                }
            }

            return newExpression.Update(newArguments);
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            var newLeft = Visit(binaryExpression.Left);

            if (newLeft.Type == typeof(ValueBuffer))
            {
                newLeft = _queryModelVisitor.BindReadValueMethod(binaryExpression.Left.Type, newLeft, 0);
            }

            var newRight = Visit(binaryExpression.Right);

            if (newRight.Type == typeof(ValueBuffer))
            {
                newRight = _queryModelVisitor.BindReadValueMethod(binaryExpression.Right.Type, newRight, 0);
            }

            var newConversion = VisitAndConvert(binaryExpression.Conversion, "VisitBinary");

            return binaryExpression.Update(newLeft, newConversion, newRight);
        }

        protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression querySourceReferenceExpression)
        {
            var newExpression
                = _querySourceMapping.ContainsMapping(querySourceReferenceExpression.ReferencedQuerySource)
                    ? _querySourceMapping.GetExpression(querySourceReferenceExpression.ReferencedQuerySource)
                    : querySourceReferenceExpression;

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

        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            expression.QueryModel.TransformExpressions(Visit);
            return expression;
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var newExpression = Visit(memberExpression.Expression);

            if (newExpression != memberExpression.Expression)
            {
                if (newExpression.Type == typeof(ValueBuffer))
                {
                    return _queryModelVisitor
                        .BindMemberToValueBuffer(memberExpression, newExpression)
                           ?? memberExpression;
                }

                var member = memberExpression.Member;
                var typeInfo = newExpression.Type.GetTypeInfo();

                if (typeInfo.IsGenericType
                    && (typeInfo.GetGenericTypeDefinition() == typeof(IGrouping<,>)
                        || typeInfo.GetGenericTypeDefinition() == typeof(IAsyncGrouping<,>)))
                {
                    member = typeInfo.GetDeclaredProperty("Key");
                }

                return Expression.MakeMemberAccess(newExpression, member);
            }

            return memberExpression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            MethodCallExpression newExpression = null;
            Expression firstArgument = null;

            if (methodCallExpression.Method.IsGenericMethod
                && ReferenceEquals(
                    methodCallExpression.Method.GetGenericMethodDefinition(),
                    EntityQueryModelVisitor.PropertyMethodInfo))
            {
                var newArguments
                    = VisitAndConvert(methodCallExpression.Arguments, "VisitMethodCall");

                if (newArguments[0].Type == typeof(ValueBuffer))
                {
                    firstArgument = newArguments[0];

                    // Compensate for ValueBuffer being a struct, and hence not compatible with Object method
                    newExpression
                        = Expression.Call(
                            methodCallExpression.Method,
                            Expression.Convert(newArguments[0], typeof(object)),
                            newArguments[1]);
                }
            }

            if (newExpression == null)
            {
                newExpression
                    = (MethodCallExpression)base.VisitMethodCall(methodCallExpression);
            }

            firstArgument = firstArgument ?? newExpression.Arguments.FirstOrDefault();

            if (newExpression != methodCallExpression
                && firstArgument?.Type == typeof(ValueBuffer))
            {
                return
                    _queryModelVisitor
                        .BindMethodCallToValueBuffer(methodCallExpression, firstArgument)
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
            = typeof(MemberAccessBindingExpressionVisitor)
                .GetTypeInfo().GetDeclaredMethod(nameof(GetValue));

        [UsedImplicitly]
        private static T GetValue<T>(QueryContext queryContext, object entity, IProperty property)
            => (T)queryContext.QueryBuffer.GetPropertyValue(entity, property);
    }
}
