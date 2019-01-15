// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration
{
    public class AppendSelectConstantExpressionMutator : ExpressionMutator
    {
        public AppendSelectConstantExpressionMutator(DbContext context)
            : base(context)
        {
        }

        private readonly List<(Type type, Expression expression)> _expressions = new List<(Type type, Expression expression)>
        {
            (type: typeof(int), expression: Expression.Constant(42, typeof(int))),
            (type: typeof(int?), expression: Expression.Constant(7, typeof(int?))),
            (type: typeof(int?), expression: Expression.Constant(null, typeof(int?))),
            (type: typeof(string), expression: Expression.Constant("Foo", typeof(string))),
            (type: typeof(string), expression: Expression.Constant(null, typeof(string)))
        };

        public override bool IsValid(Expression expression) => IsQueryableResult(expression);

        public override Expression Apply(Expression expression, Random random)
        {
            var i = random.Next(_expressions.Count);

            var typeArgument = expression.Type.GetGenericArguments()[0];
            var select = SelectMethodInfo.MakeGenericMethod(typeArgument, _expressions[i].type);
            var lambda = Expression.Lambda(_expressions[i].expression, Expression.Parameter(typeArgument, "prm"));
            var resultExpression = Expression.Call(select, expression, lambda);

            return resultExpression;
        }
    }
}
