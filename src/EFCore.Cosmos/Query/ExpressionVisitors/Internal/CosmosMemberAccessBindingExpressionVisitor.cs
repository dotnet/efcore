// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
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

                                newExpression = CreateGetValueExpression(newExpression, (IProperty)property);
                            }

                            if (newExpression != null)
                            {
                                return newExpression;
                            }
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
                if (methodCallExpression.TryGetEFPropertyArguments(out var source, out _))
                {
                    var newSource = Visit(source);

                    if (source != newSource
                        && _queryModelVisitor.AllMembersBoundToJObject
                        && newSource.Type == typeof(JObject))
                    {
                        var properties = MemberAccessBindingExpressionVisitor.GetPropertyPath(
                            methodCallExpression, _queryModelVisitor.QueryCompilationContext, out var qsre);

                        if (qsre != null)
                        {
                            Debug.Assert(properties.Count == 1);

                            newSource = CreateGetValueExpression(newSource, (IProperty)properties[0]);

                            if (newSource != null)
                            {
                                return newSource;
                            }
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
            IProperty property)
        {
            var storeName = property.GetCosmosPropertyName();
            if (storeName.Length == 0)
            {
                return null;
            }

            return ValueBufferFactoryFactory.CreateGetStoreValueExpression(jObjectExpression, property, storeName);
        }
    }
}
