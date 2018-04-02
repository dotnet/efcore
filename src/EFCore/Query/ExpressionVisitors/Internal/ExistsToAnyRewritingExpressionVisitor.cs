// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing.ExpressionVisitors;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ExistsToAnyRewritingExpressionVisitor : ExpressionVisitorBase
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Arguments.Count == 1 &&
                methodCallExpression.Method.Name == nameof(List<int>.Exists) &&
                methodCallExpression.Method.DeclaringType.IsGenericType &&
                methodCallExpression.Method.DeclaringType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var genericTypeParameters = methodCallExpression.Method.DeclaringType.GetGenericArguments();
                if (genericTypeParameters.Length == 1 && methodCallExpression.Arguments[0] is LambdaExpression actualLambdaExpression)
                {
                    var innerListType = methodCallExpression.Method.DeclaringType.GetGenericArguments()[0];

                    var mainFromClause = new MainFromClause(
                        "<generated>_",
                        innerListType,
                        methodCallExpression.Object);

                    var qsre = new QuerySourceReferenceExpression(mainFromClause);
                    var queryModel = new QueryModel(
                        mainFromClause,
                        new SelectClause(qsre));

                    mainFromClause.ItemName = queryModel.GetNewName(mainFromClause.ItemName);

                    var predicateExpression
                        = ReplacingExpressionVisitor
                        .Replace(
                            actualLambdaExpression.Parameters[0],
                            qsre,
                            actualLambdaExpression.Body);

                    queryModel.BodyClauses.Add(new WhereClause(predicateExpression));
                    queryModel.ResultOperators.Add(new AnyResultOperator());
                    queryModel.ResultTypeOverride = typeof(bool);

                    return new SubQueryExpression(queryModel);
                }
            }

            return methodCallExpression;
        }
    }
}
