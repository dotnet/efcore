// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration
{
    public class AppendSelectIdentityExpressionMutator : ExpressionMutator
    {
        public AppendSelectIdentityExpressionMutator(DbContext context)
            : base(context)
        {
        }

        public override bool IsValid(Expression expression) => IsQueryableResult(expression);

        public override Expression Apply(Expression expression, Random random)
        {
            var typeArgument = expression.Type.GetGenericArguments()[0];
            var select = SelectMethodInfo.MakeGenericMethod(typeArgument, typeArgument);
            var prm = Expression.Parameter(typeArgument, "prm");
            var lambda = Expression.Lambda(prm, prm);
            var resultExpression = Expression.Call(select, expression, lambda);

            return resultExpression;
        }
    }
}
