// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerSqlTranslatingExpressionVisitor : RelationalSqlTranslatingExpressionVisitor
    {
        private static readonly HashSet<string> _dateTimeDataTypes
            = new HashSet<string>
            {
                "time",
                "date",
                "datetime",
                "datetime2",
                "datetimeoffset"
            };

        private static readonly HashSet<ExpressionType> _arithmeticOperatorTypes
            = new HashSet<ExpressionType>
            {
                ExpressionType.Add,
                ExpressionType.Subtract,
                ExpressionType.Multiply,
                ExpressionType.Divide,
                ExpressionType.Modulo
            };

        public SqlServerSqlTranslatingExpressionVisitor(
            [NotNull] RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
            [NotNull] IModel model,
            [NotNull] QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
            : base(dependencies, model, queryableMethodTranslatingExpressionVisitor)
        {
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            var visitedExpression = (SqlExpression)base.VisitBinary(binaryExpression);

            if (visitedExpression == null)
            {
                return null;
            }

            return visitedExpression is SqlBinaryExpression sqlBinary
                && _arithmeticOperatorTypes.Contains(sqlBinary.OperatorType)
                && (_dateTimeDataTypes.Contains(GetProviderType(sqlBinary.Left))
                    || _dateTimeDataTypes.Contains(GetProviderType(sqlBinary.Right)))
                    ? null
                    : visitedExpression;
        }

        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            if (unaryExpression.NodeType == ExpressionType.ArrayLength
                && unaryExpression.Operand.Type == typeof(byte[]))
            {
                var sqlExpression = base.Visit(unaryExpression.Operand) as SqlExpression;

                if (sqlExpression == null)
                {
                    return null;
                }

                var isBinaryMaxDataType = GetProviderType(sqlExpression) == "varbinary(max)" || sqlExpression is SqlParameterExpression;
                var dataLengthSqlFunction = SqlExpressionFactory.Function(
                    "DATALENGTH",
                    new[] { sqlExpression },
                    nullResultAllowed: true,
                    argumentsPropagateNullability: new bool[] { true },
                    isBinaryMaxDataType ? typeof(long) : typeof(int));

                return isBinaryMaxDataType
                    ? (Expression)SqlExpressionFactory.Convert(dataLengthSqlFunction, typeof(int))
                    : dataLengthSqlFunction;
            }

            return base.VisitUnary(unaryExpression);
        }

        public override SqlExpression TranslateLongCount(Expression expression = null)
        {
            if (expression != null)
            {
                // TODO: Translate Count with predicate for GroupBy
                return null;
            }

            return SqlExpressionFactory.ApplyDefaultTypeMapping(
                SqlExpressionFactory.Function(
                    "COUNT_BIG",
                    new[] { SqlExpressionFactory.Fragment("*") },
                    nullResultAllowed: true,
                    argumentsPropagateNullability: new[] { false },
                    typeof(long)));
        }

        private static string GetProviderType(SqlExpression expression)
        {
            return expression.TypeMapping?.StoreType;
        }
    }
}
