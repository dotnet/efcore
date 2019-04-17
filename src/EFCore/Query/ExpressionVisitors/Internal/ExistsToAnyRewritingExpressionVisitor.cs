// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing.ExpressionVisitors;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ExistsToAnyRewritingExpressionVisitor : ExpressionVisitorBase
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Arguments.Count == 1
                && methodCallExpression.Method.Name == nameof(List<int>.Exists)
                && methodCallExpression.Method.DeclaringType.IsGenericType
                && methodCallExpression.Method.DeclaringType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var genericTypeParameters = methodCallExpression.Method.DeclaringType.GetGenericArguments();
                if (genericTypeParameters.Length == 1
                    && methodCallExpression.Arguments[0] is LambdaExpression lambdaExpression)
                {
                    var mainFromClause = new MainFromClause(
                        "<generated>_",
                        genericTypeParameters[0],
                        methodCallExpression.Object);

                    var qsre = new QuerySourceReferenceExpression(mainFromClause);
                    var queryModel = new QueryModel(
                        mainFromClause,
                        new SelectClause(qsre));

                    mainFromClause.ItemName = queryModel.GetNewName(mainFromClause.ItemName);

                    var predicateExpression
                        = ReplacingExpressionVisitor.Replace(
                            lambdaExpression.Parameters[0],
                            qsre,
                            lambdaExpression.Body);

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
