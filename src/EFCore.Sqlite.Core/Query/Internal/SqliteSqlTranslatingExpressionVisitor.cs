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
                    typeof(decimal),
                    typeof(TimeSpan)
                },
                [ExpressionType.Divide] = new HashSet<Type>
                {
                    typeof(decimal),
                    typeof(TimeSpan),
                    typeof(ulong)
                },
                [ExpressionType.GreaterThan] = new HashSet<Type>
                {
                    typeof(DateTimeOffset),
                    typeof(decimal),
                    typeof(TimeSpan),
                    typeof(ulong)
                },
                [ExpressionType.GreaterThanOrEqual] = new HashSet<Type>
                {
                    typeof(DateTimeOffset),
                    typeof(decimal),
                    typeof(TimeSpan),
                    typeof(ulong)
                },
                [ExpressionType.LessThan] = new HashSet<Type>
                {
                    typeof(DateTimeOffset),
                    typeof(decimal),
                    typeof(TimeSpan),
                    typeof(ulong)
                },
                [ExpressionType.LessThanOrEqual] = new HashSet<Type>
                {
                    typeof(DateTimeOffset),
                    typeof(decimal),
                    typeof(TimeSpan),
                    typeof(ulong)
                },
                [ExpressionType.Modulo] = new HashSet<Type>
                {
                    typeof(ulong)
                },
                [ExpressionType.Multiply] = new HashSet<Type>
                {
                    typeof(decimal),
                    typeof(TimeSpan),
                    typeof(ulong)
                },
                [ExpressionType.Subtract] = new HashSet<Type>
                {
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(decimal),
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

            if (unaryExpression.NodeType == ExpressionType.Negate
                && unaryExpression.Operand.Type == typeof(TimeSpan))
            {
                return Visit(unaryExpression.Operand) is SqlExpression sqlExpression
                    ? SqlExpressionFactory.Function(
                        "ef_timespan",
                        new[]
                        {
                            SqlExpressionFactory.Negate(
                                SqliteExpression.Days(
                                    SqlExpressionFactory,
                                    sqlExpression))
                        },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        unaryExpression.Type)
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
                if (operandType == typeof(decimal)
                    || operandType == typeof(TimeSpan))
                {
                    return null;
                }
            }

            return visitedExpression;
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            if (binaryExpression.NodeType == ExpressionType.Add)
            {
                if (binaryExpression.Right.Type == typeof(TimeSpan))
                {
                    if (binaryExpression.Left.Type == typeof(TimeSpan))
                    {
                        return Visit(binaryExpression.Left) is SqlExpression sqlLeft
                                && Visit(binaryExpression.Right) is SqlExpression sqlRight
                            ? SqlExpressionFactory.Function(
                                "ef_timespan",
                                new[]
                                {
                                    SqlExpressionFactory.Add(
                                        SqliteExpression.Days(
                                            SqlExpressionFactory,
                                            sqlLeft),
                                        SqliteExpression.Days(
                                            SqlExpressionFactory,
                                            sqlRight))
                                },
                                nullable: true,
                                argumentsPropagateNullability: new[] { true },
                                binaryExpression.Type)
                            : null;
                    }
                    else if (binaryExpression.Left.Type == typeof(DateTime))
                    {
                        return Visit(binaryExpression.Left) is SqlExpression sqlLeft
                                && Visit(binaryExpression.Right) is SqlExpression sqlRight
                            ? SqliteExpression.DateTime(
                                SqlExpressionFactory,
                                binaryExpression.Type,
                                SqlExpressionFactory.Add(
                                    SqliteExpression.JulianDay(
                                        SqlExpressionFactory,
                                        sqlLeft),
                                    SqliteExpression.Days(
                                        SqlExpressionFactory,
                                        sqlRight)))
                            : null;
                    }
                }
            }
            else if (binaryExpression.NodeType == ExpressionType.Divide)
            {
                if (binaryExpression.Left.Type == typeof(TimeSpan))
                {
                    if (binaryExpression.Right.Type == typeof(double))
                    {
                        return Visit(binaryExpression.Left) is SqlExpression sqlLeft
                                && Visit(binaryExpression.Right) is SqlExpression sqlRight
                            ? SqlExpressionFactory.Function(
                                "ef_timespan",
                                new[]
                                {
                                    SqlExpressionFactory.Divide(
                                        SqliteExpression.Days(
                                            SqlExpressionFactory,
                                            sqlLeft),
                                        sqlRight)
                                },
                                nullable: true,
                                argumentsPropagateNullability: new[] { true },
                                binaryExpression.Type)
                            : null;
                    }
                    else if (binaryExpression.Right.Type == typeof(TimeSpan))
                    {
                        return Visit(binaryExpression.Left) is SqlExpression sqlLeft
                                && Visit(binaryExpression.Right) is SqlExpression sqlRight
                            ? SqlExpressionFactory.Function(
                                "ef_timespan",
                                new[]
                                {
                                    SqlExpressionFactory.Divide(
                                        SqliteExpression.Days(
                                            SqlExpressionFactory,
                                            sqlLeft),
                                        SqliteExpression.Days(
                                            SqlExpressionFactory,
                                            sqlRight))
                                },
                                nullable: true,
                                argumentsPropagateNullability: new[] { true },
                                binaryExpression.Type)
                            : null;
                    }
                }
            }
            else if (binaryExpression.NodeType == ExpressionType.GreaterThan)
            {
                if (binaryExpression.Left.Type == typeof(TimeSpan)
                    && binaryExpression.Right.Type == typeof(TimeSpan))
                {
                    return Visit(binaryExpression.Left) is SqlExpression sqlLeft
                                && Visit(binaryExpression.Right) is SqlExpression sqlRight
                            ? SqlExpressionFactory.GreaterThan(
                                SqliteExpression.Days(
                                    SqlExpressionFactory,
                                    sqlLeft),
                                SqliteExpression.Days(
                                    SqlExpressionFactory,
                                    sqlRight))
                            : null;
                }
            }
            else if (binaryExpression.NodeType == ExpressionType.GreaterThanOrEqual)
            {
                if (binaryExpression.Left.Type == typeof(TimeSpan)
                    && binaryExpression.Right.Type == typeof(TimeSpan))
                {
                    return Visit(binaryExpression.Left) is SqlExpression sqlLeft
                                && Visit(binaryExpression.Right) is SqlExpression sqlRight
                            ? SqlExpressionFactory.GreaterThanOrEqual(
                                SqliteExpression.Days(
                                    SqlExpressionFactory,
                                    sqlLeft),
                                SqliteExpression.Days(
                                    SqlExpressionFactory,
                                    sqlRight))
                            : null;
                }
            }
            else if (binaryExpression.NodeType == ExpressionType.LessThan)
            {
                if (binaryExpression.Left.Type == typeof(TimeSpan)
                    && binaryExpression.Right.Type == typeof(TimeSpan))
                {
                    return Visit(binaryExpression.Left) is SqlExpression sqlLeft
                                && Visit(binaryExpression.Right) is SqlExpression sqlRight
                            ? SqlExpressionFactory.LessThan(
                                SqliteExpression.Days(
                                    SqlExpressionFactory,
                                    sqlLeft),
                                SqliteExpression.Days(
                                    SqlExpressionFactory,
                                    sqlRight))
                            : null;
                }
            }
            else if (binaryExpression.NodeType == ExpressionType.LessThanOrEqual)
            {
                if (binaryExpression.Left.Type == typeof(TimeSpan)
                    && binaryExpression.Right.Type == typeof(TimeSpan))
                {
                    return Visit(binaryExpression.Left) is SqlExpression sqlLeft
                                && Visit(binaryExpression.Right) is SqlExpression sqlRight
                            ? SqlExpressionFactory.LessThanOrEqual(
                                SqliteExpression.Days(
                                    SqlExpressionFactory,
                                    sqlLeft),
                                SqliteExpression.Days(
                                    SqlExpressionFactory,
                                    sqlRight))
                            : null;
                }
            }
            else if (binaryExpression.NodeType == ExpressionType.Multiply)
            {
                if (binaryExpression.Left.Type == typeof(TimeSpan)
                    && binaryExpression.Right.Type == typeof(double))
                {
                    return Visit(binaryExpression.Left) is SqlExpression sqlLeft
                                && Visit(binaryExpression.Right) is SqlExpression sqlRight
                            ? SqlExpressionFactory.Function(
                                "ef_timespan",
                                new[]
                                {
                                    SqlExpressionFactory.Multiply(
                                        SqliteExpression.Days(
                                            SqlExpressionFactory,
                                            sqlLeft),
                                        sqlRight)
                                },
                                nullable: true,
                                argumentsPropagateNullability: new[] { true },
                                typeof(TimeSpan))
                            : null;
                }
                else if (binaryExpression.Left.Type == typeof(double)
                    && binaryExpression.Right.Type == typeof(TimeSpan))
                {
                    return Visit(binaryExpression.Left) is SqlExpression sqlLeft
                                && Visit(binaryExpression.Right) is SqlExpression sqlRight
                            ? SqlExpressionFactory.Function(
                                "ef_timespan",
                                new[]
                                {
                                    SqlExpressionFactory.Multiply(
                                        sqlLeft,
                                        SqliteExpression.Days(
                                            SqlExpressionFactory,
                                            sqlRight))
                                },
                                nullable: true,
                                argumentsPropagateNullability: new[] { true },
                                typeof(TimeSpan))
                            : null;
                }
            }
            else if (binaryExpression.NodeType == ExpressionType.Subtract)
            {
                if (binaryExpression.Left.Type == typeof(DateTime))
                {
                    if (binaryExpression.Right.Type == typeof(DateTime))
                    {
                        return Visit(binaryExpression.Left) is SqlExpression sqlLeft
                                && Visit(binaryExpression.Right) is SqlExpression sqlRight
                            ? SqlExpressionFactory.Function(
                                "ef_timespan",
                                new[]
                                {
                                    SqlExpressionFactory.Subtract(
                                        SqliteExpression.JulianDay(
                                            SqlExpressionFactory,
                                            sqlLeft),
                                        SqliteExpression.JulianDay(
                                            SqlExpressionFactory,
                                            sqlRight))
                                },
                                nullable: true,
                                argumentsPropagateNullability: new[] { true },
                                typeof(TimeSpan))
                            : null;
                    }
                    else if (binaryExpression.Right.Type == typeof(TimeSpan))
                    {
                        return Visit(binaryExpression.Left) is SqlExpression sqlLeft
                                && Visit(binaryExpression.Right) is SqlExpression sqlRight
                            ? SqliteExpression.DateTime(
                                SqlExpressionFactory,
                                typeof(DateTime),
                                SqlExpressionFactory.Subtract(
                                    SqliteExpression.JulianDay(
                                        SqlExpressionFactory,
                                        sqlLeft),
                                    SqliteExpression.Days(
                                        SqlExpressionFactory,
                                        sqlRight)))
                            : null;
                    }
                }
                else if (binaryExpression.Left.Type == typeof(TimeSpan)
                    && binaryExpression.Right.Type == typeof(TimeSpan))
                {
                    return Visit(binaryExpression.Left) is SqlExpression sqlLeft
                                && Visit(binaryExpression.Right) is SqlExpression sqlRight
                            ? SqlExpressionFactory.Function(
                                "ef_timespan",
                                new[]
                                {
                                    SqlExpressionFactory.Subtract(
                                        SqliteExpression.Days(
                                            SqlExpressionFactory,
                                            sqlLeft),
                                        SqliteExpression.Days(
                                            SqlExpressionFactory,
                                            sqlRight))
                                },
                                nullable: true,
                                argumentsPropagateNullability: new[] { true },
                                typeof(TimeSpan))
                            : null;
                }
            }

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

            if (expression.Type == typeof(TimeSpan))
            {
                return Translate(expression) is SqlExpression sqlExpression
                    ? SqlExpressionFactory.Function(
                        "ef_timespan",
                        new[]
                        {
                            base.TranslateMax(
                                SqliteExpression.Days(
                                    SqlExpressionFactory,
                                    sqlExpression))
                        },
                                nullable: true,
                                argumentsPropagateNullability: new[] { true },
                        typeof(TimeSpan))
                    : null;
            }

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

            if (expression.Type == typeof(TimeSpan))
            {
                return Translate(expression) is SqlExpression sqlExpression
                    ? SqlExpressionFactory.Function(
                        "ef_timespan",
                        new[]
                        {
                            base.TranslateMin(
                                SqliteExpression.Days(
                                    SqlExpressionFactory,
                                    sqlExpression))
                        },
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        typeof(TimeSpan))
                    : null;
            }

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
    }
}
