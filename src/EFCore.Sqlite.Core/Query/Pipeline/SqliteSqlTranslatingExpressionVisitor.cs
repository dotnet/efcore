// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Pipeline
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
                    typeof(decimal),
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

        public SqliteSqlTranslatingExpressionVisitor(
            IModel model,
            ISqlExpressionFactory sqlExpressionFactory,
            IMemberTranslatorProvider memberTranslatorProvider,
            IMethodCallTranslatorProvider methodCallTranslatorProvider)
            : base(model, sqlExpressionFactory, memberTranslatorProvider, methodCallTranslatorProvider)
        {
        }

        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            var visitedExpression = (SqlExpression)base.VisitUnary(unaryExpression);

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
            var visitedExpression = (SqlExpression)base.VisitBinary(binaryExpression);

            if (visitedExpression == null)
            {
                return null;
            }

            return visitedExpression is SqlBinaryExpression sqlBinary
                && _restrictedBinaryExpressions.TryGetValue(sqlBinary.OperatorType, out var restrictedTypes)
                && (restrictedTypes.Contains(GetProviderType(sqlBinary.Left))
                    || restrictedTypes.Contains(GetProviderType(sqlBinary.Right)))
                ? null
                : visitedExpression;
        }

        private static Type GetProviderType(SqlExpression expression)
            => (expression.TypeMapping?.Converter.ProviderClrType ?? expression.TypeMapping?.ClrType).UnwrapNullableType();
    }
}
