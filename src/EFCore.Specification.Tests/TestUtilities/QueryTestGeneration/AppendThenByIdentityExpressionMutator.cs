// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration
{
    public class AppendThenByIdentityExpressionMutator : ExpressionMutator
    {
        public AppendThenByIdentityExpressionMutator(DbContext context)
            : base(context)
        {
        }

        public override bool IsValid(Expression expression)
            => IsOrderedQueryableResult(expression)
            && IsOrderedableType(expression.Type.GetGenericArguments()[0]);

        public override Expression Apply(Expression expression, Random random)
        {
            var typeArgument = expression.Type.GetGenericArguments()[0];

            var isDescending = random.Next(3) == 0;
            var thenBy = isDescending
                ? ThenByDescendingMethodInfo.MakeGenericMethod(typeArgument, typeArgument)
                : ThenByMethodInfo.MakeGenericMethod(typeArgument, typeArgument);

            var prm = Expression.Parameter(typeArgument, "prm");
            var lambda = Expression.Lambda(prm, prm);
            var resultExpression = Expression.Call(thenBy, expression, lambda);

            return resultExpression;
        }
    }
}
