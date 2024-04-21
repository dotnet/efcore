// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query;

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

    [return: NotNullIfNotNull(nameof(expression))]
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
        => expression switch
        {
            ConstantExpression constantExpression => (T)constantExpression.Value!,
#pragma warning disable EF9100 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            LiftableConstantExpression liftableConstantExpression => (T)liftableConstantExpression.OriginalExpression.Value!,
#pragma warning restore EF9100 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            _ => throw new InvalidOperationException()
        };

    public static bool TryGetNonNullConstantValue<T>(this Expression expression, [NotNullWhen(true)][MaybeNullWhen(false)]out T value)
    {
        switch (expression)
        {
            case ConstantExpression constant when constant.Value is T typedValue:
                value = typedValue;
                return true;
#pragma warning disable EF9100 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            case LiftableConstantExpression liftableConstant when liftableConstant.OriginalExpression.Value is T typedValue:
#pragma warning restore EF9100 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                value = typedValue;
                return true;
            default:
                value = default;
                return false;
        }
    }
}
