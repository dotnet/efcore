// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration;

public class AppendTakeExpressionMutator(DbContext context) : ExpressionMutator(context)
{
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
