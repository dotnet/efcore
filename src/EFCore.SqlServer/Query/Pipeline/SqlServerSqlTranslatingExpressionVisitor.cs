// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
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
                ExpressionType.Modulo,
            };
        // TODO: Possibly make this protected in base
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerSqlTranslatingExpressionVisitor(
            IModel model,
            QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor,
            ISqlExpressionFactory sqlExpressionFactory,
            IMemberTranslatorProvider memberTranslatorProvider,
            IMethodCallTranslatorProvider methodCallTranslatorProvider)
            : base(model, queryableMethodTranslatingExpressionVisitor, sqlExpressionFactory, memberTranslatorProvider, methodCallTranslatorProvider)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
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

        public override SqlExpression TranslateLongCount(Expression expression = null)
        {
            // TODO: Translate Count with predicate for GroupBy
            return _sqlExpressionFactory.ApplyDefaultTypeMapping(
                _sqlExpressionFactory.Function("COUNT_BIG", new[] { _sqlExpressionFactory.Fragment("*") }, typeof(long)));
        }

        private static string GetProviderType(SqlExpression expression)
        {
            return expression.TypeMapping?.StoreType;
        }
    }
}
