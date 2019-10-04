// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteSqlTranslatingExpressionVisitor : SqlTranslatingExpressionVisitor
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqliteSqlTranslatingExpressionVisitor(
            [NotNull] SqlTranslatingExpressionVisitorDependencies dependencies,
            [NotNull] RelationalQueryModelVisitor queryModelVisitor,
            [NotNull] SelectExpression targetSelectExpression = null,
            [NotNull] Expression topLevelPredicate = null,
            bool inProjection = false)
            : base(dependencies, queryModelVisitor, targetSelectExpression, topLevelPredicate, inProjection)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitUnary(UnaryExpression expression)
        {
            var visitedExpression = base.VisitUnary(expression);

            if (visitedExpression == null)
            {
                return null;
            }

            if (visitedExpression.NodeType == ExpressionType.Negate
                && visitedExpression is UnaryExpression visitedUnaryExpression)
            {
                var operandType = GetProviderType(visitedUnaryExpression.Operand);
                if (operandType == typeof(decimal)
                    || operandType == typeof(TimeSpan))
                {
                    return null;
                }
            }

            return visitedExpression;
        }

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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            var visitedExpression = base.VisitBinary(binaryExpression);

            if (visitedExpression == null)
            {
                return null;
            }

            return _restrictedBinaryExpressions.TryGetValue(visitedExpression.NodeType, out var restrictedTypes)
                && visitedExpression is BinaryExpression visitedBinaryExpression
                && (restrictedTypes.Contains(GetProviderType(visitedBinaryExpression.Left))
                    || restrictedTypes.Contains(GetProviderType(visitedBinaryExpression.Right)))
                ? null
                : visitedExpression;
        }

        private static Type GetProviderType(Expression expression)
            => (expression.FindProperty(expression.Type)?.FindRelationalMapping().ClrType ?? expression.Type)
                .UnwrapNullableType();
    }
}
