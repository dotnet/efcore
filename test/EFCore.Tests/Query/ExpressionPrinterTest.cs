// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ExpressionPrinterTest
    {
        private readonly ExpressionPrinter _expressionPrinter = new ExpressionPrinter();

        [ConditionalFact]
        public void UnaryExpression_printed_correctly()
        {
            Assert.Equal("(decimal)42", _expressionPrinter.Print(Expression.Convert(Expression.Constant(42), typeof(decimal))));
            Assert.Equal("throw \"Some exception\"", _expressionPrinter.Print(Expression.Throw(Expression.Constant("Some exception"))));
            Assert.Equal("!(True)", _expressionPrinter.Print(Expression.Not(Expression.Constant(true))));
            Assert.Equal(
                "(BaseClass as DerivedClass)",
                _expressionPrinter.Print(Expression.TypeAs(Expression.Constant(new BaseClass()), typeof(DerivedClass))));
        }

        private class BaseClass
        {
        }

        private class DerivedClass : BaseClass
        {
        }

        [ConditionalFact]
        public void BinaryExpression_printed_correctly()
        {
            Assert.Equal(
                "7 == 42",
                _expressionPrinter.Print(Expression.MakeBinary(ExpressionType.Equal, Expression.Constant(7), Expression.Constant(42))));
            Assert.Equal(
                "7 != 42",
                _expressionPrinter.Print(Expression.MakeBinary(ExpressionType.NotEqual, Expression.Constant(7), Expression.Constant(42))));
            Assert.Equal(
                "7 > 42",
                _expressionPrinter.Print(
                    Expression.MakeBinary(ExpressionType.GreaterThan, Expression.Constant(7), Expression.Constant(42))));
            Assert.Equal(
                "7 >= 42",
                _expressionPrinter.Print(
                    Expression.MakeBinary(ExpressionType.GreaterThanOrEqual, Expression.Constant(7), Expression.Constant(42))));
            Assert.Equal(
                "7 < 42",
                _expressionPrinter.Print(Expression.MakeBinary(ExpressionType.LessThan, Expression.Constant(7), Expression.Constant(42))));
            Assert.Equal(
                "7 <= 42",
                _expressionPrinter.Print(
                    Expression.MakeBinary(ExpressionType.LessThanOrEqual, Expression.Constant(7), Expression.Constant(42))));
            Assert.Equal(
                "True && True",
                _expressionPrinter.Print(
                    Expression.MakeBinary(ExpressionType.AndAlso, Expression.Constant(true), Expression.Constant(true))));
            Assert.Equal(
                "True || True",
                _expressionPrinter.Print(
                    Expression.MakeBinary(ExpressionType.OrElse, Expression.Constant(true), Expression.Constant(true))));
            Assert.Equal(
                "7 & 42",
                _expressionPrinter.Print(Expression.MakeBinary(ExpressionType.And, Expression.Constant(7), Expression.Constant(42))));
            Assert.Equal(
                "7 | 42",
                _expressionPrinter.Print(Expression.MakeBinary(ExpressionType.Or, Expression.Constant(7), Expression.Constant(42))));
            Assert.Equal(
                "7 ^ 42",
                _expressionPrinter.Print(
                    Expression.MakeBinary(ExpressionType.ExclusiveOr, Expression.Constant(7), Expression.Constant(42))));
            Assert.Equal(
                "7 + 42",
                _expressionPrinter.Print(Expression.MakeBinary(ExpressionType.Add, Expression.Constant(7), Expression.Constant(42))));
            Assert.Equal(
                "7 - 42",
                _expressionPrinter.Print(Expression.MakeBinary(ExpressionType.Subtract, Expression.Constant(7), Expression.Constant(42))));
            Assert.Equal(
                "7 * 42",
                _expressionPrinter.Print(Expression.MakeBinary(ExpressionType.Multiply, Expression.Constant(7), Expression.Constant(42))));
            Assert.Equal(
                "7 / 42",
                _expressionPrinter.Print(Expression.MakeBinary(ExpressionType.Divide, Expression.Constant(7), Expression.Constant(42))));
            Assert.Equal(
                "7 % 42",
                _expressionPrinter.Print(Expression.MakeBinary(ExpressionType.Modulo, Expression.Constant(7), Expression.Constant(42))));
        }

        [ConditionalFact]
        public void ConditionalExpression_printed_correctly()
        {
            Assert.Equal(
                "True ? \"Foo\" : \"Bar\"",
                _expressionPrinter.Print(
                    Expression.Condition(
                        Expression.Constant(true),
                        Expression.Constant("Foo"),
                        Expression.Constant("Bar"))));
        }

        [ConditionalFact]
        public void Simple_lambda_printed_correctly()
        {
            Assert.Equal(
                "prm => 42",
                _expressionPrinter.Print(
                    Expression.Lambda(
                        Expression.Constant(42),
                        Expression.Parameter(typeof(int), "prm"))));
        }

        [ConditionalFact]
        public void Multi_parameter_lambda_printed_correctly()
        {
            Assert.Equal(
                "(prm1, prm2) => 42",
                _expressionPrinter.Print(
                    Expression.Lambda(
                        Expression.Constant(42),
                        Expression.Parameter(typeof(int), "prm1"),
                        Expression.Parameter(typeof(int), "prm2"))));
        }

        [ConditionalFact]
        public void Unhandled_parameter_in_lambda_detected()
        {
            Assert.Equal(
                "prm1{0} => (Unhandled parameter: prm2){1}",
                _expressionPrinter.PrintDebug(
                    Expression.Lambda(
                        Expression.Parameter(typeof(int), "prm2"),
                        Expression.Parameter(typeof(int), "prm1"))));
        }

        [ConditionalFact]
        public void MemberAccess_after_BinaryExpression_adds_parentheses()
        {
            Assert.Equal(
                @"(7 + 42).Value",
                _expressionPrinter.Print(
                    Expression.Property(
                        Expression.Add(
                            Expression.Constant(7, typeof(int?)),
                            Expression.Constant(42, typeof(int?))),
                        "Value")));
        }

        [ConditionalFact]
        public void Simple_MethodCall_printed_correctly()
        {
            Assert.Equal(
                @"""Foo"".ToUpper()",
                _expressionPrinter.Print(
                    Expression.Call(
                        Expression.Constant("Foo"),
                        typeof(string).GetMethods().Single(m => m.Name == nameof(string.ToUpper) && m.GetParameters().Count() == 0))));
        }

        [ConditionalFact]
        public void Complex_MethodCall_printed_correctly()
        {
            Assert.Equal(
                "\"Foobar\""
+ @".Substring(
    startIndex: 0, 
    length: 4)",
                _expressionPrinter.Print(
                    Expression.Call(
                        Expression.Constant("Foobar"),
                        typeof(string).GetMethods().Single(m => m.Name == nameof(string.Substring) && m.GetParameters().Count() == 2),
                        Expression.Constant(0),
                        Expression.Constant(4))));
        }

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
                _expressionPrinter.Print(expr.Body));
        }
    }
}
