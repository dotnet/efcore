// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.Internal;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Newtonsoft.Json.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.ExpressionVisitors.Internal
{
    public class CosmosSqlMemberAccessBindingExpressionVisitor : RelinqExpressionVisitor
    {
        private readonly QuerySourceMapping _querySourceMapping;
        private readonly EntityQueryModelVisitor _queryModelVisitor;
        private readonly bool _inProjection;
        private static readonly MethodInfo _getItemMethodInfo
            = typeof(JObject).GetTypeInfo().GetRuntimeProperties()
                .Single(pi => pi.Name == "Item" && pi.GetIndexParameters()[0].ParameterType == typeof(string))
                .GetMethod;

        public CosmosSqlMemberAccessBindingExpressionVisitor(
            QuerySourceMapping querySourceMapping,
            EntityQueryModelVisitor queryModelVisitor,
            bool inProjection)
        {
            _querySourceMapping = querySourceMapping;
            _queryModelVisitor = queryModelVisitor;
            _inProjection = inProjection;
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var newExpression = Visit(memberExpression.Expression);

            if (newExpression != memberExpression.Expression)
            {
                if (newExpression.Type == typeof(JObject))
                {
                    var properties = MemberAccessBindingExpressionVisitor.GetPropertyPath(
                        memberExpression, _queryModelVisitor.QueryCompilationContext, out var qsre);

                    if (qsre != null)
                    {
                        foreach (var property in properties)
                        {
                            newExpression = CreateGetValueExpression(
                                newExpression, property);
                        }

                        return newExpression;
                    }
                    else
                    {
                        var modelVisitor = (CosmosSqlQueryModelVisitor)_queryModelVisitor;
                        modelVisitor.AllMembersBoundToJObject = false;

                        return memberExpression;
                    }
                }

                return Expression.MakeMemberAccess(newExpression, memberExpression.Member);
            }

            return memberExpression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.IsEFPropertyMethod())
            {
                var source = methodCallExpression.Arguments[0];
                var newSource = Visit(source);

                if (source != newSource)
                {
                    if (newSource.Type == typeof(JObject))
                    {
                        var properties = MemberAccessBindingExpressionVisitor.GetPropertyPath(
                        methodCallExpression, _queryModelVisitor.QueryCompilationContext, out var qsre);

                        if (qsre != null)
                        {
                            foreach (var property in properties)
                            {
                                newSource = CreateGetValueExpression(
                                    newSource, property);
                            }

                            return newSource;
                        }
                    }
                }
            }

            var newExpression = (MethodCallExpression)base.VisitMethodCall(methodCallExpression);

            var firstArgument = newExpression.Arguments.FirstOrDefault();
            var isJobject = newExpression != methodCallExpression
                && firstArgument?.Type == typeof(JObject);

            return newExpression;
        }

        protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression querySourceReferenceExpression)
        {
            return _querySourceMapping.ContainsMapping(querySourceReferenceExpression.ReferencedQuerySource)
                ? _querySourceMapping.GetExpression(querySourceReferenceExpression.ReferencedQuerySource)
                : querySourceReferenceExpression;
        }

        private static Expression CreateGetValueExpression(
            Expression jObjectExpression,
            IPropertyBase property)
        {
            // TODO : Converters
            // TODO : TryCatch for invalid values

            return Expression.Convert(
                Expression.Call(
                    jObjectExpression,
                    _getItemMethodInfo,
                    Expression.Constant(property.Name)),
                property.ClrType);
        }
    }
}
