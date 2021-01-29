// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

#nullable enable

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlServerSqlTranslatingExpressionVisitor : RelationalSqlTranslatingExpressionVisitor
    {
        private static readonly HashSet<string?> _dateTimeDataTypes
            = new()
            {
                "time",
                "date",
                "datetime",
                "datetime2",
                "datetimeoffset"
            };

        private static readonly HashSet<ExpressionType> _arithmeticOperatorTypes
            = new()
            {
                ExpressionType.Add,
                ExpressionType.Subtract,
                ExpressionType.Multiply,
                ExpressionType.Divide,
                ExpressionType.Modulo
            };

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerSqlTranslatingExpressionVisitor(
            [NotNull] RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
            [NotNull] QueryCompilationContext queryCompilationContext,
            [NotNull] QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
            : base(dependencies, queryCompilationContext, queryableMethodTranslatingExpressionVisitor)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            if (binaryExpression.NodeType == ExpressionType.ArrayIndex
                && binaryExpression.Left.Type == typeof(byte[]))
            {
                var left = Visit(binaryExpression.Left);
                var right = Visit(binaryExpression.Right);

                if (left is SqlExpression leftSql
                    && right is SqlExpression rightSql)
                {
                    return Dependencies.SqlExpressionFactory.Convert(
                        Dependencies.SqlExpressionFactory.Function(
                            "SUBSTRING",
                            new SqlExpression[]
                            {
                                leftSql,
                                Dependencies.SqlExpressionFactory.Add(
                                    Dependencies.SqlExpressionFactory.ApplyDefaultTypeMapping(rightSql),
                                    Dependencies.SqlExpressionFactory.Constant(1)),
                                Dependencies.SqlExpressionFactory.Constant(1)
                            },
                            nullable: true,
                            argumentsPropagateNullability: new[] { true, true, true },
                            typeof(byte[])),
                        binaryExpression.Type);
                }
            }

            return !(base.VisitBinary(binaryExpression) is SqlExpression visitedExpression)
                ? QueryCompilationContext.NotTranslatedExpression
                : visitedExpression is SqlBinaryExpression sqlBinary
                    && _arithmeticOperatorTypes.Contains(sqlBinary.OperatorType)
                    && (_dateTimeDataTypes.Contains(GetProviderType(sqlBinary.Left))
                        || _dateTimeDataTypes.Contains(GetProviderType(sqlBinary.Right)))
                        ? QueryCompilationContext.NotTranslatedExpression
                        : visitedExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            if (unaryExpression.NodeType == ExpressionType.ArrayLength
                && unaryExpression.Operand.Type == typeof(byte[]))
            {
                if (!(base.Visit(unaryExpression.Operand) is SqlExpression sqlExpression))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                var isBinaryMaxDataType = GetProviderType(sqlExpression) == "varbinary(max)" || sqlExpression is SqlParameterExpression;
                var dataLengthSqlFunction = Dependencies.SqlExpressionFactory.Function(
                    "DATALENGTH",
                    new[] { sqlExpression },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    isBinaryMaxDataType ? typeof(long) : typeof(int));

                return isBinaryMaxDataType
                    ? (Expression)Dependencies.SqlExpressionFactory.Convert(dataLengthSqlFunction, typeof(int))
                    : dataLengthSqlFunction;
            }

            return base.VisitUnary(unaryExpression);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override SqlExpression? TranslateLongCount(SqlExpression sqlExpression, [NotNull] string functionName = "COUNT_BIG")
        {
            return base.TranslateLongCount(sqlExpression, functionName);
        }

        private static string? GetProviderType(SqlExpression expression)
            => expression.TypeMapping?.StoreType;
    }
}
