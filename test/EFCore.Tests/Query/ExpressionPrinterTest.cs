// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class ExpressionPrinterTest
{
    private readonly ExpressionPrinter _expressionPrinter = new();

    [ConditionalFact]
    public void UnaryExpression_printed_correctly()
    {
        Assert.Equal("(decimal)42", _expressionPrinter.PrintExpression(Expression.Convert(Expression.Constant(42), typeof(decimal))));
        Assert.Equal(
            "throw \"Some exception\"", _expressionPrinter.PrintExpression(Expression.Throw(Expression.Constant("Some exception"))));
        Assert.Equal("!(True)", _expressionPrinter.PrintExpression(Expression.Not(Expression.Constant(true))));
        Assert.Equal(
            "(BaseClass as DerivedClass)",
            _expressionPrinter.PrintExpression(Expression.TypeAs(Expression.Constant(new BaseClass()), typeof(DerivedClass))));
    }

    private class BaseClass;

    private class DerivedClass : BaseClass;

    [ConditionalFact]
    public void BinaryExpression_printed_correctly()
    {
        Assert.Equal(
            "7 == 42",
            _expressionPrinter.PrintExpression(
                Expression.MakeBinary(ExpressionType.Equal, Expression.Constant(7), Expression.Constant(42))));
        Assert.Equal(
            "7 != 42",
            _expressionPrinter.PrintExpression(
                Expression.MakeBinary(ExpressionType.NotEqual, Expression.Constant(7), Expression.Constant(42))));
        Assert.Equal(
            "7 > 42",
            _expressionPrinter.PrintExpression(
                Expression.MakeBinary(ExpressionType.GreaterThan, Expression.Constant(7), Expression.Constant(42))));
        Assert.Equal(
            "7 >= 42",
            _expressionPrinter.PrintExpression(
                Expression.MakeBinary(ExpressionType.GreaterThanOrEqual, Expression.Constant(7), Expression.Constant(42))));
        Assert.Equal(
            "7 < 42",
            _expressionPrinter.PrintExpression(
                Expression.MakeBinary(ExpressionType.LessThan, Expression.Constant(7), Expression.Constant(42))));
        Assert.Equal(
            "7 <= 42",
            _expressionPrinter.PrintExpression(
                Expression.MakeBinary(ExpressionType.LessThanOrEqual, Expression.Constant(7), Expression.Constant(42))));
        Assert.Equal(
            "True && True",
            _expressionPrinter.PrintExpression(
                Expression.MakeBinary(ExpressionType.AndAlso, Expression.Constant(true), Expression.Constant(true))));
        Assert.Equal(
            "True || True",
            _expressionPrinter.PrintExpression(
                Expression.MakeBinary(ExpressionType.OrElse, Expression.Constant(true), Expression.Constant(true))));
        Assert.Equal(
            "7 & 42",
            _expressionPrinter.PrintExpression(Expression.MakeBinary(ExpressionType.And, Expression.Constant(7), Expression.Constant(42))));
        Assert.Equal(
            "7 | 42",
            _expressionPrinter.PrintExpression(Expression.MakeBinary(ExpressionType.Or, Expression.Constant(7), Expression.Constant(42))));
        Assert.Equal(
            "7 ^ 42",
            _expressionPrinter.PrintExpression(
                Expression.MakeBinary(ExpressionType.ExclusiveOr, Expression.Constant(7), Expression.Constant(42))));
        Assert.Equal(
            "7 + 42",
            _expressionPrinter.PrintExpression(Expression.MakeBinary(ExpressionType.Add, Expression.Constant(7), Expression.Constant(42))));
        Assert.Equal(
            "7 - 42",
            _expressionPrinter.PrintExpression(
                Expression.MakeBinary(ExpressionType.Subtract, Expression.Constant(7), Expression.Constant(42))));
        Assert.Equal(
            "7 * 42",
            _expressionPrinter.PrintExpression(
                Expression.MakeBinary(ExpressionType.Multiply, Expression.Constant(7), Expression.Constant(42))));
        Assert.Equal(
            "7 / 42",
            _expressionPrinter.PrintExpression(
                Expression.MakeBinary(ExpressionType.Divide, Expression.Constant(7), Expression.Constant(42))));
        Assert.Equal(
            "7 % 42",
            _expressionPrinter.PrintExpression(
                Expression.MakeBinary(ExpressionType.Modulo, Expression.Constant(7), Expression.Constant(42))));
    }

    [ConditionalFact]
    public void ConditionalExpression_printed_correctly()
        => Assert.Equal(
            "True ? \"Foo\" : \"Bar\"",
            _expressionPrinter.PrintExpression(
                Expression.Condition(
                    Expression.Constant(true),
                    Expression.Constant("Foo"),
                    Expression.Constant("Bar"))));

    [ConditionalFact]
    public void Simple_lambda_printed_correctly()
        => Assert.Equal(
            "prm => 42",
            _expressionPrinter.PrintExpression(
                Expression.Lambda(
                    Expression.Constant(42),
                    Expression.Parameter(typeof(int), "prm"))));

    [ConditionalFact]
    public void Multi_parameter_lambda_printed_correctly()
        => Assert.Equal(
            "(prm1, prm2) => 42",
            _expressionPrinter.PrintExpression(
                Expression.Lambda(
                    Expression.Constant(42),
                    Expression.Parameter(typeof(int), "prm1"),
                    Expression.Parameter(typeof(int), "prm2"))));

    [ConditionalFact]
    public void Unhandled_parameter_in_lambda_detected()
        => Assert.Equal(
            "prm1{0} => (Unhandled parameter: prm2){1}",
            _expressionPrinter.PrintExpressionDebug(
                Expression.Lambda(
                    Expression.Parameter(typeof(int), "prm2"),
                    Expression.Parameter(typeof(int), "prm1"))));

    [ConditionalFact]
    public void MemberAccess_after_BinaryExpression_adds_parentheses()
        => Assert.Equal(
            @"(7 + 42).Value",
            _expressionPrinter.PrintExpression(
                Expression.Property(
                    Expression.Add(
                        Expression.Constant(7, typeof(int?)),
                        Expression.Constant(42, typeof(int?))),
                    "Value")));

    [ConditionalFact]
    public void Simple_MethodCall_printed_correctly()
        => Assert.Equal(
            @"""Foo"".ToUpper()",
            _expressionPrinter.PrintExpression(
                Expression.Call(
                    Expression.Constant("Foo"),
                    typeof(string).GetMethods().Single(m => m.Name == nameof(string.ToUpper) && m.GetParameters().Count() == 0))));

    [ConditionalFact]
    public void Complex_MethodCall_printed_correctly()
        => Assert.Equal(
            "\"Foobar\""
            + @".Substring(
    startIndex: 0, 
    length: 4)",
            _expressionPrinter.PrintExpression(
                Expression.Call(
                    Expression.Constant("Foobar"),
                    typeof(string).GetMethods().Single(m => m.Name == nameof(string.Substring) && m.GetParameters().Count() == 2),
                    Expression.Constant(0),
                    Expression.Constant(4))),
            ignoreLineEndingDifferences: true);

    [ConditionalFact]
    public void Linq_methods_printed_as_extensions()
    {
        Expression<Func<object, object>> expr =
            _ => new[] { 1, 2, 3 }.AsQueryable().Select(x => x.ToString()).AsEnumerable().Where(x => x.Length > 1);

        Assert.Equal(
            @"new int[]
{ 
    1, 
    2, 
    3 
}
    .AsQueryable()
    .Select(x => x.ToString())
    .AsEnumerable()
    .Where(x => x.Length > 1)",
            _expressionPrinter.PrintExpression(expr.Body),
            ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void Enumerable_Constant_printed_correctly()
        => Assert.Equal(
            @"int[] { 1, 2, 3 }",
            _expressionPrinter.PrintExpression(
                Expression.Constant(
                    new[] { 1, 2, 3 })));
}
