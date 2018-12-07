// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Newtonsoft.Json.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.ExpressionVisitors.Internal
{
    public class CosmosMemberAccessBindingExpressionVisitor : RelinqExpressionVisitor
    {
        private static readonly MethodInfo _getItemMethodInfo
            = typeof(JObject).GetTypeInfo().GetRuntimeProperties()
                .Single(pi => pi.Name == "Item" && pi.GetIndexParameters()[0].ParameterType == typeof(string))
                .GetMethod;

        private readonly QuerySourceMapping _querySourceMapping;
        private readonly CosmosQueryModelVisitor _queryModelVisitor;
        private readonly bool _inProjection;

        public CosmosMemberAccessBindingExpressionVisitor(
            QuerySourceMapping querySourceMapping,
            EntityQueryModelVisitor queryModelVisitor,
            bool inProjection)
        {
            _querySourceMapping = querySourceMapping;
            _queryModelVisitor = (CosmosQueryModelVisitor)queryModelVisitor;
            _inProjection = inProjection;
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var newExpression = Visit(memberExpression.Expression);

            if (newExpression != memberExpression.Expression)
            {
                if (_queryModelVisitor.CurrentParameter?.Type == typeof(JObject)
                    || newExpression.Type == typeof(JObject))
                {
                    if (_queryModelVisitor.AllMembersBoundToJObject)
                    {
                        var properties = MemberAccessBindingExpressionVisitor.GetPropertyPath(
                            memberExpression, _queryModelVisitor.QueryCompilationContext, out var qsre);

                        if (qsre != null)
                        {
                            foreach (var property in properties)
                            {
                                if (property is INavigation)
                                {
                                    _queryModelVisitor.AllMembersBoundToJObject = false;

                                    return memberExpression;
                                }

                                newExpression = CreateGetValueExpression(newExpression, property);
                            }

                            return newExpression;
                        }
                    }

                    _queryModelVisitor.AllMembersBoundToJObject = false;

                    return memberExpression;
                }

                return Expression.MakeMemberAccess(newExpression, memberExpression.Member);
            }

            return memberExpression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (_queryModelVisitor.CurrentParameter?.Type == typeof(JObject))
            {
                if (methodCallExpression.Method.IsEFPropertyMethod())
                {
                    var source = methodCallExpression.Arguments[0];
                    var newSource = Visit(source);

                    if (source != newSource
                        && _queryModelVisitor.AllMembersBoundToJObject
                        && newSource.Type == typeof(JObject))
                    {
                        var properties = MemberAccessBindingExpressionVisitor.GetPropertyPath(
                            methodCallExpression, _queryModelVisitor.QueryCompilationContext, out var qsre);

                        if (qsre != null)
                        {
                            foreach (var property in properties)
                            {
                                newSource = CreateGetValueExpression(newSource, property);
                            }

                            return newSource;
                        }
                    }
                }

                _queryModelVisitor.AllMembersBoundToJObject = false;
                return methodCallExpression;
            }

            var newExpression = (MethodCallExpression)base.VisitMethodCall(methodCallExpression);

            return _queryModelVisitor.BindMethodCallToEntity(methodCallExpression, newExpression) ?? newExpression;
        }

        protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression querySourceReferenceExpression)
        {
            Expression newExpression = querySourceReferenceExpression;
            if (_querySourceMapping.ContainsMapping(querySourceReferenceExpression.ReferencedQuerySource))
            {
                var mappedExpression = _querySourceMapping.GetExpression(querySourceReferenceExpression.ReferencedQuerySource);
                if (!(mappedExpression is ParameterExpression mappedParameter)
                    || mappedParameter != _queryModelVisitor.CurrentParameter)
                {
                    _queryModelVisitor.AllMembersBoundToJObject = false;
                }

                newExpression = mappedExpression;
            }

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

        private static Expression CreateGetValueExpression(
            Expression jObjectExpression,
            IPropertyBase property)
            => Expression.Convert(
                Expression.Call(
                    jObjectExpression,
                    _getItemMethodInfo,
                    Expression.Constant((property as IProperty)?.Cosmos().PropertyName ?? property.Name)),
                property.ClrType);
    }
}
