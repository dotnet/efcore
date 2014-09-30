// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Redis.Query
{
    public partial class RedisQueryModelVisitor
    {
        protected class RedisQueryingExpressionTreeVisitor : QueryingExpressionTreeVisitor
        {
            private readonly RedisQueryModelVisitor _parentVisitor;
            private readonly IQuerySource _querySource;

            public RedisQueryingExpressionTreeVisitor(RedisQueryModelVisitor parentVisitor, IQuerySource querySource)
                : base(parentVisitor)
            {
                _parentVisitor = parentVisitor;
                _querySource = querySource;
            }

            protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
            {
                var visitor = new RedisQueryModelVisitor(_parentVisitor);

                visitor.VisitQueryModel(expression.QueryModel);

                return visitor.Expression;
            }

            protected override Expression VisitEntityQueryable(Type elementType)
            {
                var isAsync =
                    ((RedisQueryCompilationContext)_parentVisitor.QueryCompilationContext).IsAsync;
                MethodInfo methodInfo;
                if (_parentVisitor.QuerySourceRequiresMaterialization(_querySource))
                {
                    var entityType = _parentVisitor.QueryCompilationContext.Model.GetEntityType(elementType);

                    methodInfo = (isAsync
                                    ? _executeMaterializedQueryExpressionMethodInfoAsync
                                    : _executeMaterializedQueryExpressionMethodInfo)
                                .MakeGenericMethod(elementType);
                }
                else
                {
                    methodInfo = isAsync
                        ? _executeNonMaterializedQueryExpressionMethodInfoAsync
                        : _executeNonMaterializedQueryExpressionMethodInfo;
                }

                return Expression.Call(
                    Expression.Constant(_parentVisitor),
                    methodInfo,
                    Expression.Constant(_querySource),
                    QueryContextParameter);
            }
        }
    }
}
