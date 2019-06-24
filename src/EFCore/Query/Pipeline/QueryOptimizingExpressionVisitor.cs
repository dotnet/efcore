// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.NavigationExpansion;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public class QueryOptimizer
    {
        private readonly QueryCompilationContext _queryCompilationContext;

        public QueryOptimizer(QueryCompilationContext queryCompilationContext)
        {
            _queryCompilationContext = queryCompilationContext;
        }

        public Expression Visit(Expression query)
        {
            query = new QueryMetadataExtractingExpressionVisitor(_queryCompilationContext).Visit(query);
            query = new AllAnyToContainsRewritingExpressionVisitor().Visit(query);
            query = new GroupJoinFlatteningExpressionVisitor().Visit(query);
            query = new NullCheckRemovingExpressionVisitor().Visit(query);
            query = new EntityEqualityRewritingExpressionVisitor(_queryCompilationContext).Rewrite(query);
            query = new NavigationExpander(_queryCompilationContext).ExpandNavigations(query);
            query = new EnumerableToQueryableReMappingExpressionVisitor().Visit(query);
            query = new NullCheckRemovingExpressionVisitor().Visit(query);
            query = new FunctionPreprocessingVisitor().Visit(query);
            new EnumerableVerifyingExpressionVisitor().Visit(query);

            return query;
        }

        // TODO: For debugging
        private class EnumerableVerifyingExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.DeclaringType == typeof(Enumerable)
                    && node.Arguments[0].Type.IsGenericType
                    && node.Arguments[0].Type.GetGenericTypeDefinition() == typeof(IQueryable<>)
                    && !string.Equals(node.Method.Name, nameof(Enumerable.ToList))
                    && !string.Equals(node.Method.Name, nameof(Enumerable.ToArray)))
                {
                    throw new InvalidFilterCriteriaException();
                }

                return base.VisitMethodCall(node);
            }
        }
    }
}
