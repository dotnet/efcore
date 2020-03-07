// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
    public class SqliteSqlTranslatingExpressionVisitor : RelationalSqlTranslatingExpressionVisitor
    {
        private static readonly IReadOnlyDictionary<ExpressionType, IReadOnlyCollection<Type>> _restrictedBinaryExpressions
            = new Dictionary<ExpressionType, IReadOnlyCollection<Type>>
            {
                [ExpressionType.Add] = new HashSet<Type>
                {
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(TimeSpan)
                },
                [ExpressionType.Divide] = new HashSet<Type> { typeof(TimeSpan), typeof(ulong) },
                [ExpressionType.GreaterThan] = new HashSet<Type>
                {
                    typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(ulong)
                },
                [ExpressionType.GreaterThanOrEqual] = new HashSet<Type>
                {
                    typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(ulong)
                },
                [ExpressionType.LessThan] = new HashSet<Type>
                {
                    typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(ulong)
                },
                [ExpressionType.LessThanOrEqual] = new HashSet<Type>
                {
                    typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(ulong)
                },
                [ExpressionType.Modulo] = new HashSet<Type> { typeof(ulong) },
                [ExpressionType.Multiply] = new HashSet<Type> { typeof(TimeSpan), typeof(ulong) },
                [ExpressionType.Subtract] = new HashSet<Type>
                {
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(TimeSpan)
                }
            };

        private static readonly IReadOnlyCollection<Type> _functionModuloTypes = new HashSet<Type>
        {
            typeof(decimal),
            typeof(double),
            typeof(float)
        };

        public SqliteSqlTranslatingExpressionVisitor(
            [NotNull] RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
            [NotNull] IModel model,
            [NotNull] QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
            : base(dependencies, model, queryableMethodTranslatingExpressionVisitor)
        {
        }

        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            Check.NotNull(unaryExpression, nameof(unaryExpression));

            if (unaryExpression.NodeType == ExpressionType.ArrayLength
                && unaryExpression.Operand.Type == typeof(byte[]))
            {
                return base.Visit(unaryExpression.Operand) is SqlExpression sqlExpression
                    ? SqlExpressionFactory.Function(
                        "length",
                        new[] { sqlExpression },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        typeof(int))
                    : null;
            }

            var visitedExpression = base.VisitUnary(unaryExpression);
            if (visitedExpression == null)
            {
                return null;
            }

            if (visitedExpression is SqlUnaryExpression sqlUnary
                && sqlUnary.OperatorType == ExpressionType.Negate)
            {
                var operandType = GetProviderType(sqlUnary.Operand);
                if (operandType == typeof(decimal))
                {
                    return SqlExpressionFactory.Function(
                        name: "ef_negate",
                        new[] { sqlUnary.Operand },
                        nullable: true,
                        new[] { true },
                        visitedExpression.Type);
                }

                if (operandType == typeof(TimeSpan))
                {
                    return null;
                }
            }

            return visitedExpression;
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            var visitedExpression = (SqlExpression)base.VisitBinary(binaryExpression);

            if (visitedExpression == null)
            {
                return null;
            }

            if (visitedExpression is SqlBinaryExpression sqlBinary)
            {
                if (sqlBinary.OperatorType == ExpressionType.Modulo
                    && (_functionModuloTypes.Contains(GetProviderType(sqlBinary.Left))
                        || _functionModuloTypes.Contains(GetProviderType(sqlBinary.Right))))
                {
                    return SqlExpressionFactory.Function(
                        "ef_mod",
                        new[] { sqlBinary.Left, sqlBinary.Right },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true, true },
                        visitedExpression.Type,
                        visitedExpression.TypeMapping);
                }

                if (AttemptDecimalCompare(sqlBinary))
                {
                    return DoDecimalCompare(visitedExpression, sqlBinary.OperatorType, sqlBinary.Left, sqlBinary.Right);
                }

                if (AttemptDecimalArithmetic(sqlBinary))
                {
                    return DoDecimalArithmetics(visitedExpression, sqlBinary.OperatorType, sqlBinary.Left, sqlBinary.Right);
                }

                if (_restrictedBinaryExpressions.TryGetValue(sqlBinary.OperatorType, out var restrictedTypes)
                    && (restrictedTypes.Contains(GetProviderType(sqlBinary.Left))
                        || restrictedTypes.Contains(GetProviderType(sqlBinary.Right))))
                {
                    return null;
                }
            }

            return visitedExpression;
        }

        public override SqlExpression TranslateAverage(Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var visitedExpression = base.TranslateAverage(expression);
            if (GetProviderType(visitedExpression) == typeof(decimal))
            {
                return null;
            }

            return visitedExpression;
        }

        public override SqlExpression TranslateMax(Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var visitedExpression = base.TranslateMax(expression);
            var argumentType = GetProviderType(visitedExpression);
            if (argumentType == typeof(DateTimeOffset)
                || argumentType == typeof(decimal)
                || argumentType == typeof(TimeSpan)
                || argumentType == typeof(ulong))
            {
                return null;
            }

            return visitedExpression;
        }

        public override SqlExpression TranslateMin(Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var visitedExpression = base.TranslateMin(expression);
            var argumentType = GetProviderType(visitedExpression);
            if (argumentType == typeof(DateTimeOffset)
                || argumentType == typeof(decimal)
                || argumentType == typeof(TimeSpan)
                || argumentType == typeof(ulong))
            {
                return null;
            }

            return visitedExpression;
        }

        public override SqlExpression TranslateSum(Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var visitedExpression = base.TranslateSum(expression);
            if (GetProviderType(visitedExpression) == typeof(decimal))
            {
                return null;
            }

            return visitedExpression;
        }

        private static Type GetProviderType(SqlExpression expression)
            => expression == null
                ? null
                : (expression.TypeMapping?.Converter?.ProviderClrType
                    ?? expression.TypeMapping?.ClrType
                    ?? expression.Type).UnwrapNullableType();

        private static bool AreOperandsDecimals(SqlBinaryExpression sqlExpression) => GetProviderType(sqlExpression.Left) == typeof(decimal)
            && GetProviderType(sqlExpression.Right) == typeof(decimal);

        private static bool AttemptDecimalCompare(SqlBinaryExpression sqlBinary) =>
            AreOperandsDecimals(sqlBinary)
            && new[]
            {
                ExpressionType.GreaterThan, ExpressionType.GreaterThanOrEqual, ExpressionType.LessThan, ExpressionType.LessThanOrEqual
            }.Contains(sqlBinary.OperatorType);

        private Expression DoDecimalCompare(SqlExpression visitedExpression, ExpressionType op, SqlExpression left, SqlExpression right)
        {
            var actual = SqlExpressionFactory.Function(
                name: "ef_compare",
                new[] { left, right },
                nullable: true,
                new[] { true, true },
                typeof(int));
            var oracle = SqlExpressionFactory.Constant(value: 0);

            return op switch
            {
                ExpressionType.GreaterThan => SqlExpressionFactory.GreaterThan(left: actual, right: oracle),
                ExpressionType.GreaterThanOrEqual => SqlExpressionFactory.GreaterThanOrEqual(left: actual, right: oracle),
                ExpressionType.LessThan => SqlExpressionFactory.LessThan(left: actual, right: oracle),
                ExpressionType.LessThanOrEqual => SqlExpressionFactory.LessThanOrEqual(left: actual, right: oracle),
                _ => visitedExpression
            };
        }

        private static bool AttemptDecimalArithmetic(SqlBinaryExpression sqlBinary) =>
            AreOperandsDecimals(sqlBinary)
            && new[] { ExpressionType.Add, ExpressionType.Subtract, ExpressionType.Multiply, ExpressionType.Divide }.Contains(
                sqlBinary.OperatorType);

        private Expression DoDecimalArithmetics(SqlExpression visitedExpression, ExpressionType op, SqlExpression left, SqlExpression right)
        {
            return op switch
            {
                ExpressionType.Add => ArithmeticExpressionFactoryFunction("ef_add", left, right),
                ExpressionType.Divide => ArithmeticExpressionFactoryFunction("ef_divide", left, right),
                ExpressionType.Multiply => ArithmeticExpressionFactoryFunction("ef_multiply", left, right),
                ExpressionType.Subtract => SubtractExpressionFactoryFunction(left, right),
                _ => visitedExpression
            };

            Expression ArithmeticExpressionFactoryFunction(string name, SqlExpression left, SqlExpression right) =>
                SqlExpressionFactory.Function(
                    name,
                    new[] { left, right },
                    nullable: true,
                    new[] { true, true },
                    visitedExpression.Type);

            Expression SubtractExpressionFactoryFunction(SqlExpression left, SqlExpression right)
            {
                var subtrahend = SqlExpressionFactory.Function(
                    "ef_negate",
                    new[] { right },
                    nullable: true,
                    new[] { true },
                    visitedExpression.Type);

                return ArithmeticExpressionFactoryFunction("ef_add", left, subtrahend);
            }
        }
    }
}
