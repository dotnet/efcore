// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration;

public class AppendSelectIdentityExpressionMutator(DbContext context) : ExpressionMutator(context)
{
    public override bool IsValid(Expression expression)
        => IsQueryableResult(expression);

    public override Expression Apply(Expression expression, Random random)
    {
        var typeArgument = expression.Type.GetGenericArguments()[0];
        var select = QueryableMethods.Select.MakeGenericMethod(typeArgument, typeArgument);
        var prm = Expression.Parameter(typeArgument, "prm");
        var lambda = Expression.Lambda(prm, prm);
        var resultExpression = Expression.Call(select, expression, lambda);

        return resultExpression;
    }
}
