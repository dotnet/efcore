// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class TypedToListToArrayRewritingExpressionVisitor : ExpressionVisitorBase
    {
        private static readonly MethodInfo _toListMethodInfo
            = typeof(Enumerable).GetTypeInfo().GetDeclaredMethod(nameof(Enumerable.ToList));

        private static readonly MethodInfo _toArrayMethodInfo
            = typeof(Enumerable).GetTypeInfo().GetDeclaredMethod(nameof(Enumerable.ToArray));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if ((methodCallExpression.Method.MethodIsClosedFormOf(_toListMethodInfo) || methodCallExpression.Method.MethodIsClosedFormOf(_toArrayMethodInfo))
                && methodCallExpression.Arguments[0] is SubQueryExpression subQuery
                && subQuery.Type.TryGetSequenceType() != methodCallExpression.Type.TryGetSequenceType())
            {
                var sequenceType = methodCallExpression.Type.GetSequenceType();
                subQuery.QueryModel.ResultOperators.Add(new CastResultOperator(sequenceType));
                subQuery.QueryModel.ResultTypeOverride = typeof(IEnumerable<>).MakeGenericType(sequenceType);

                var newSubQueryExpression = new SubQueryExpression(subQuery.QueryModel);

                return methodCallExpression.Update(methodCallExpression.Object, new[] { newSubQueryExpression });
            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }
}
