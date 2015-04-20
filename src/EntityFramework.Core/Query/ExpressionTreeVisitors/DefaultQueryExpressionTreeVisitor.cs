// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionTreeVisitors
{
    public class DefaultQueryExpressionTreeVisitor : ExpressionTreeVisitorBase
    {
        private readonly EntityQueryModelVisitor _entityQueryModelVisitor;
        private readonly IQuerySource _querySource;

        public DefaultQueryExpressionTreeVisitor(
            [NotNull] EntityQueryModelVisitor entityQueryModelVisitor,
            [NotNull] IQuerySource querySource)
        {
            Check.NotNull(entityQueryModelVisitor, nameof(entityQueryModelVisitor));
            Check.NotNull(querySource, nameof(querySource));

            _entityQueryModelVisitor = entityQueryModelVisitor;
            _querySource = querySource;
        }

        public virtual EntityQueryModelVisitor QueryModelVisitor => _entityQueryModelVisitor;

        public virtual IQuerySource QuerySource => _querySource;

        protected override Expression VisitMemberExpression(MemberExpression memberExpression)
        {
            var newExpression = VisitExpression(memberExpression.Expression);

            if (newExpression != memberExpression.Expression)
            {
                var newExpressionTypeInfo = newExpression.Type.GetTypeInfo();

                if (newExpressionTypeInfo.IsGenericType
                    && newExpressionTypeInfo.GetGenericTypeDefinition() == typeof(QuerySourceScope<>))
                {
                    newExpression
                        = Expression.Convert(newExpression, memberExpression.Expression.Type);
                }

                return Expression.MakeMemberAccess(newExpression, memberExpression.Member);
            }

            return memberExpression;
        }

        protected override Expression VisitMethodCallExpression(MethodCallExpression methodCallExpression)
        {
            var newObject = VisitExpression(methodCallExpression.Object);
            IList<Expression> newArguments = VisitAndConvert(methodCallExpression.Arguments, "VisitMethodCallExpression");

            if (newObject != methodCallExpression.Object
                || !ReferenceEquals(newArguments, methodCallExpression.Arguments))
            {
                if (newArguments.Any()
                    && newArguments[0] != methodCallExpression.Arguments[0])
                {
                    var newArgumentTypeInfo = newArguments[0].Type.GetTypeInfo();

                    if (newArgumentTypeInfo.IsGenericType
                        && newArgumentTypeInfo.GetGenericTypeDefinition() == typeof(QuerySourceScope<>))
                    {
                        newArguments = new List<Expression>(newArguments);

                        newArguments[0]
                            = Expression.Convert(newArguments[0], methodCallExpression.Arguments[0].Type);
                    }
                }

                return Expression.Call(newObject, methodCallExpression.Method, newArguments);
            }

            return methodCallExpression;
        }

        protected override Expression VisitSubQueryExpression(SubQueryExpression subQueryExpression)
        {
            Check.NotNull(subQueryExpression, nameof(subQueryExpression));

            var queryModelVisitor = CreateQueryModelVisitor();

            queryModelVisitor.VisitQueryModel(subQueryExpression.QueryModel);

            var resultItemTypeInfo
                = (queryModelVisitor.StreamedSequenceInfo?.ResultItemType
                   ?? queryModelVisitor.Expression.Type)
                    .GetTypeInfo();

            if (resultItemTypeInfo.IsGenericType
                && resultItemTypeInfo.GetGenericTypeDefinition() == typeof(QuerySourceScope<>))
            {
                return
                    Expression.Call(
                        (queryModelVisitor.StreamedSequenceInfo != null
                            ? QueryModelVisitor.QueryCompilationContext.LinqOperatorProvider
                                .RewrapQueryResults
                            : _rewrapSingleResult)
                            .MakeGenericMethod(resultItemTypeInfo.GenericTypeArguments[0]),
                        queryModelVisitor.Expression,
                        Expression.Constant(_querySource));
            }

            return queryModelVisitor.Expression;
        }

        private static readonly MethodInfo _rewrapSingleResult
            = typeof(DefaultQueryExpressionTreeVisitor)
                .GetTypeInfo().GetDeclaredMethod("RewrapSingleResult");

        [UsedImplicitly]
        private static QuerySourceScope<TResult> RewrapSingleResult<TResult>(
            QuerySourceScope<TResult> querySourceScope,
            IQuerySource querySource)
            where TResult : class
        {
            return
                new QuerySourceScope<TResult>(
                    querySource,
                    // ReSharper disable once MergeConditionalExpression
                    querySourceScope != null ? querySourceScope.Result : default(TResult),
                    querySourceScope,
                    null);
        }

        protected EntityQueryModelVisitor CreateQueryModelVisitor()
        {
            return QueryModelVisitor.QueryCompilationContext
                .CreateQueryModelVisitor(_entityQueryModelVisitor);
        }

        protected override Expression VisitParameterExpression(ParameterExpression parameterExpression)
        {
            if (parameterExpression.Name != null
                && parameterExpression.Name
                    .StartsWith(CompiledQueryCache.CompiledQueryParameterPrefix))
            {
                return Expression.Call(
                    _getParameterValueMethodInfo.MakeGenericMethod(parameterExpression.Type),
                    EntityQueryModelVisitor.QueryContextParameter,
                    Expression.Constant(parameterExpression.Name));
            }

            return parameterExpression;
        }

        private static readonly MethodInfo _getParameterValueMethodInfo
            = typeof(DefaultQueryExpressionTreeVisitor)
                .GetTypeInfo().GetDeclaredMethod("GetParameterValue");

        [UsedImplicitly]
        private static T GetParameterValue<T>(QueryContext queryContext, string parameterName)
        {
            return (T)queryContext.ParameterValues[parameterName];
        }
    }
}
