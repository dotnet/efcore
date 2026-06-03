// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration;

public class AppendSelectConstantExpressionMutator(DbContext context) : ExpressionMutator(context)
{
    private readonly List<(Type type, Expression expression)> _expressions =
    [
        (type: typeof(int), expression: Expression.Constant(42, typeof(int))),
        (type: typeof(int?), expression: Expression.Constant(7, typeof(int?))),
        (type: typeof(int?), expression: Expression.Constant(null, typeof(int?))),
        (type: typeof(string), expression: Expression.Constant("Foo", typeof(string))),
        (type: typeof(string), expression: Expression.Constant(null, typeof(string)))
    ];

    public override bool IsValid(Expression expression)
        => IsQueryableResult(expression);

    public override Expression Apply(Expression expression, Random random)
    {
        var i = random.Next(_expressions.Count);

        var typeArgument = expression.Type.GetGenericArguments()[0];
        var select = QueryableMethods.Select.MakeGenericMethod(typeArgument, _expressions[i].type);
        var lambda = Expression.Lambda(_expressions[i].expression, Expression.Parameter(typeArgument, "prm"));
        var resultExpression = Expression.Call(select, expression, lambda);

        return resultExpression;
    }
}
