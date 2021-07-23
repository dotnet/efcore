﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

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
                ? QueryableMethods.ThenByDescending.MakeGenericMethod(typeArgument, typeArgument)
                : QueryableMethods.ThenBy.MakeGenericMethod(typeArgument, typeArgument);

            var prm = Expression.Parameter(typeArgument, "prm");
            var lambda = Expression.Lambda(prm, prm);
            var resultExpression = Expression.Call(thenBy, expression, lambda);

            return resultExpression;
        }
    }
}
