// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SearchConditionConvertingExpressionVisitor : ExpressionVisitor
    {
        private RelationalTypeMapping _boolTypeMapping;

        public SearchConditionConvertingExpressionVisitor(IRelationalTypeMappingSource typeMappingSource)
        {
            _boolTypeMapping = typeMappingSource.FindMapping(typeof(bool));
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is SqlExpression sqlExpression)
            {
                if (sqlExpression.IsCondition && sqlExpression.ShouldBeValue)
                {
                    return new CaseExpression(
                        new[] {
                            new CaseWhenClause(
                                (SqlExpression)base.VisitExtension(sqlExpression),
                                new SqlCastExpression(
                                    new SqlConstantExpression(Expression.Constant(true), _boolTypeMapping),
                                    typeof(bool),
                                    _boolTypeMapping))
                        },
                        new SqlCastExpression(
                            new SqlConstantExpression(Expression.Constant(false), _boolTypeMapping),
                            typeof(bool),
                            _boolTypeMapping));
                }

                if (!sqlExpression.IsCondition && !sqlExpression.ShouldBeValue)
                {
                    return new SqlBinaryExpression(
                        ExpressionType.Equal,
                        (SqlExpression)base.VisitExtension(sqlExpression),
                        new SqlConstantExpression(Expression.Constant(true), _boolTypeMapping),
                        typeof(bool),
                        _boolTypeMapping);
                }
            }

            return base.VisitExtension(extensionExpression);
        }
    }
}
