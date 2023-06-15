// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace System.Linq.Expressions;

[DebuggerStepThrough]
internal static class ExpressionExtensions
{
    public static bool IsNullConstantExpression(this Expression expression)
        => RemoveConvert(expression) is ConstantExpression { Value: null };

    public static LambdaExpression UnwrapLambdaFromQuote(this Expression expression)
        => (LambdaExpression)(expression is UnaryExpression unary && expression.NodeType == ExpressionType.Quote
            ? unary.Operand
            : expression);

    [return: NotNullIfNotNull("expression")]
    public static Expression? UnwrapTypeConversion(this Expression? expression, out Type? convertedType)
    {
        convertedType = null;
        while (expression is UnaryExpression
               {
                   NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked or ExpressionType.TypeAs
               } unaryExpression)
        {
            expression = unaryExpression.Operand;
            if (unaryExpression.Type != typeof(object) // Ignore object conversion
                && !unaryExpression.Type.IsAssignableFrom(expression.Type)) // Ignore casting to base type/interface
            {
                convertedType = unaryExpression.Type;
            }
        }

        return expression;
    }

    private static Expression RemoveConvert(Expression expression)
        => expression is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unaryExpression
            ? RemoveConvert(unaryExpression.Operand)
            : expression;

    public static T GetConstantValue<T>(this Expression expression)
        => expression is ConstantExpression constantExpression
            ? (T)constantExpression.Value!
            : throw new InvalidOperationException();
}
