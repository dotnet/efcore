// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration;

public class AppendSelectConstantExpressionMutator(DbContext context) : ExpressionMutator(context)
{
    private readonly List<(Type Type, Expression Expression)> _expressions =
    [
        (Type: typeof(int), Expression: Expression.Constant(42, typeof(int))),
        (Type: typeof(int?), Expression: Expression.Constant(7, typeof(int?))),
        (Type: typeof(int?), Expression: Expression.Constant(null, typeof(int?))),
        (Type: typeof(string), Expression: Expression.Constant("Foo", typeof(string))),
        (Type: typeof(string), Expression: Expression.Constant(null, typeof(string)))
    ];

    public override bool IsValid(Expression expression)
        => IsQueryableResult(expression);

    public override Expression Apply(Expression expression, Random random)
    {
        var i = random.Next(_expressions.Count);

        var typeArgument = expression.Type.GetGenericArguments()[0];
        var select = QueryableMethods.Select.MakeGenericMethod(typeArgument, _expressions[i].Type);
        var lambda = Expression.Lambda(_expressions[i].Expression, Expression.Parameter(typeArgument, "prm"));
        var resultExpression = Expression.Call(select, expression, lambda);

        return resultExpression;
    }
}
