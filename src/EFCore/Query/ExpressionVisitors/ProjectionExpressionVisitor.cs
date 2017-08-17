// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    /// <summary>
    ///     A projection expression visitor.
    /// </summary>
    public class ProjectionExpressionVisitor : DefaultQueryExpressionVisitor
    {
        /// <summary>
        ///     Initializes a new instance of the Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.ProjectionExpressionVisitor class.
        /// </summary>
        /// <param name="entityQueryModelVisitor"> The entity query model visitor. </param>
        public ProjectionExpressionVisitor([NotNull] EntityQueryModelVisitor entityQueryModelVisitor)
            : base(Check.NotNull(entityQueryModelVisitor, nameof(entityQueryModelVisitor)))
        {
        }       
        
        /// <summary>
        ///     Visits the children of the <see cref="T:System.Linq.Expressions.MethodCallExpression"></see>.
        /// </summary>
        /// <param name="methodCallExpression">The expression to visit.</param>
        /// <returns>
        ///     The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var newExpression = base.VisitMethodCall(methodCallExpression);

            switch (newExpression)
            {
                case MethodCallExpression newMethodCallExpression
                when newMethodCallExpression.Arguments.Count > 0
                     && newMethodCallExpression.Arguments[0] is MethodCallExpression innerMethodCallExpression
                     && innerMethodCallExpression.Method.MethodIsClosedFormOf(QueryModelVisitor.LinqOperatorProvider.ToEnumerable)
                     && TryFindAsyncMethod(newMethodCallExpression.Method.Name, out var asyncMethodInfo):
                {
                    // Transforms a sync method inside an async projector into the corresponding async version (if one exists).
                    // We call .Result here so that the types still line up (we remove the .Result in TaskLiftingExpressionVisitor).

                    return
                        Expression.Property(
                            ResultOperatorHandler.CallWithPossibleCancellationToken(
                                asyncMethodInfo.MakeGenericMethod(
                                    newMethodCallExpression.Method.GetGenericArguments()),
                                innerMethodCallExpression.Arguments.ToArray()),
                            nameof(Task<object>.Result));
                }
                case MethodCallExpression newMethodCallExpression2
                when newMethodCallExpression2.Method
                         .MethodIsClosedFormOf(CollectionNavigationSubqueryInjector.MaterializeCollectionNavigationMethodInfo)
                     && newMethodCallExpression2.Arguments[1] is MethodCallExpression innerMethodCallExpression2
                     && innerMethodCallExpression2.Method.MethodIsClosedFormOf(QueryModelVisitor.LinqOperatorProvider.ToQueryable):
                {
                    // Transforms a sync method inside a MaterializeCollectionNavigation call into a corresponding async version.
                    // We call .Result here so that the types still line up (we remove the .Result in TaskLiftingExpressionVisitor).

                    return
                        innerMethodCallExpression2.Arguments[0].Type.IsGenericType
                        && innerMethodCallExpression2.Arguments[0].Type.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>)
                            ? (Expression) Expression.Property(
                                ResultOperatorHandler.CallWithPossibleCancellationToken(
                                    MaterializeCollectionNavigationAsyncMethodInfo.MakeGenericMethod(
                                        newMethodCallExpression2.Method.GetGenericArguments()[0]),
                                    newMethodCallExpression2.Arguments[0],
                                    innerMethodCallExpression2.Arguments[0]),
                                nameof(Task<object>.Result))
                            : Expression.Call(
                                newMethodCallExpression2.Method,
                                newMethodCallExpression2.Arguments[0],
                                innerMethodCallExpression2.Arguments[0]);
                }
            }

            return newExpression;
        }

        private static readonly MethodInfo MaterializeCollectionNavigationAsyncMethodInfo
            = typeof(ProjectionExpressionVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(MaterializeCollectionNavigationAsync));

        [UsedImplicitly]
        private static async Task<ICollection<TEntity>> MaterializeCollectionNavigationAsync<TEntity>(
            INavigation navigation,
            IAsyncEnumerable<object> elements)
        {
            var collection = (ICollection<TEntity>) navigation.GetCollectionAccessor().Create();

            await elements.ForEachAsync(e => collection.Add((TEntity) e));

            return collection;
        }

        private static bool TryFindAsyncMethod(string methodName, out MethodInfo asyncMethodInfo)
        {
            var candidateMethods
                = typeof(AsyncEnumerable).GetTypeInfo().GetDeclaredMethods(methodName)
                    .Where(mi =>
                        mi.ReturnType.IsConstructedGenericType
                        && mi.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)
                        && mi.GetParameters().Length > 0
                        && mi.GetParameters().Last().ParameterType == typeof(CancellationToken))
                    .ToList();

            asyncMethodInfo = candidateMethods.Count == 1 ? candidateMethods[0] : null;

            return asyncMethodInfo != null;
        }
        
        /// <summary>
        ///     Visit a subquery.
        /// </summary>
        /// <param name="expression"> The subquery expression. </param>
        /// <returns>
        ///     A compiled query expression fragment representing the input subquery expression.
        /// </returns>
        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            var queryModelVisitor = CreateQueryModelVisitor();

            queryModelVisitor.VisitQueryModel(expression.QueryModel);

            var subExpression = queryModelVisitor.Expression;

            if (subExpression.Type != expression.Type)
            {
                var subQueryExpressionTypeInfo = expression.Type.GetTypeInfo();

                if (typeof(IQueryable).GetTypeInfo().IsAssignableFrom(expression.Type.GetTypeInfo()))
                {
                    subExpression
                        = Expression.Call(
                            QueryModelVisitor.LinqOperatorProvider.ToQueryable
                                .MakeGenericMethod(expression.Type.GetSequenceType()),
                            subExpression,
                            EntityQueryModelVisitor.QueryContextParameter);
                }
                else if (subQueryExpressionTypeInfo.IsGenericType)
                {
                    var genericTypeDefinition = subQueryExpressionTypeInfo.GetGenericTypeDefinition();

                    if (genericTypeDefinition == typeof(IOrderedEnumerable<>))
                    {
                        subExpression
                            = Expression.Call(
                                QueryModelVisitor.LinqOperatorProvider.ToOrdered
                                    .MakeGenericMethod(expression.Type.GetSequenceType()),
                                subExpression);
                    }
                    else if (genericTypeDefinition == typeof(IEnumerable<>))
                    {
                        subExpression
                            = Expression.Call(
                                QueryModelVisitor.LinqOperatorProvider.ToEnumerable
                                    .MakeGenericMethod(expression.Type.GetSequenceType()),
                                subExpression);
                    }
                }
            }

            return subExpression;
        }
    }
}
