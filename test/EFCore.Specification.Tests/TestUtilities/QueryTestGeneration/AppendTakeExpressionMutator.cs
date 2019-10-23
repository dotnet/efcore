// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration
{
    public class AppendTakeExpressionMutator : ExpressionMutator
    {
        public AppendTakeExpressionMutator(DbContext context)
            : base(context)
        {
        }

        public override bool IsValid(Expression expression)
            => IsOrderedQueryableResult(expression);

        public override Expression Apply(Expression expression, Random random)
        {
            var typeArgument = expression.Type.GetGenericArguments()[0];
            var take = QueryableMethods.Take.MakeGenericMethod(typeArgument);
            var count = random.Next(20);
            var resultExpression = Expression.Call(take, expression, Expression.Constant(count));

            return resultExpression;
        }
    }
}
