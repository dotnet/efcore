// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
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
                var methodProvider =
                    ((RedisQueryCompilationContext)_parentVisitor.QueryCompilationContext).QueryMethodProvider;
                var methodInfo = methodProvider.ProjectionQueryMethod;
                if (_parentVisitor.QuerySourceRequiresMaterialization(_querySource))
                {
                    methodInfo = methodProvider.MaterializationQueryMethod
                                    .MakeGenericMethod(elementType);
                }

                return Expression.Call(
                    methodInfo,
                    Expression.Constant(_querySource),
                    QueryContextParameter,
                    Expression.Constant(_parentVisitor));
            }
        }
    }
}
