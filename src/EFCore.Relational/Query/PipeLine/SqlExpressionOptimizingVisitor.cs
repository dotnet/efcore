// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public class SqlExpressionOptimizingVisitor : ExpressionVisitor
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        private readonly Dictionary<ExpressionType, ExpressionType> _expressionTypesNegationMap
            = new Dictionary<ExpressionType, ExpressionType>
            {
                { ExpressionType.AndAlso, ExpressionType.OrElse },
                { ExpressionType.OrElse, ExpressionType.AndAlso },
                { ExpressionType.Equal, ExpressionType.NotEqual },
                { ExpressionType.NotEqual, ExpressionType.Equal },
                { ExpressionType.GreaterThan, ExpressionType.LessThanOrEqual },
                { ExpressionType.GreaterThanOrEqual, ExpressionType.LessThan },
                { ExpressionType.LessThan, ExpressionType.GreaterThanOrEqual },
                { ExpressionType.LessThanOrEqual, ExpressionType.GreaterThan },
            };

        public SqlExpressionOptimizingVisitor(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is SqlUnaryExpression sqlUnaryExpression)
            {
                return VisitSqlUnaryExpression(sqlUnaryExpression);
            }

            if (extensionExpression is SqlBinaryExpression sqlBinaryExpression)
            {
                return VisitSqlBinaryExpression(sqlBinaryExpression);
            }

            return base.VisitExtension(extensionExpression);
        }

        private Expression VisitSqlUnaryExpression(SqlUnaryExpression sqlUnaryExpression)
        {
            // !(true) -> false
            // !(false) -> true
            if (sqlUnaryExpression.OperatorType == ExpressionType.Not
                && sqlUnaryExpression.Operand is SqlConstantExpression innerConstantBool
                && innerConstantBool.Value is bool value)
            {
                return value
                    ? _sqlExpressionFactory.Constant(false, sqlUnaryExpression.TypeMapping)
                    : _sqlExpressionFactory.Constant(true, sqlUnaryExpression.TypeMapping);
            }

            // NULL IS NULL -> true
            // non_nullablee_constant IS NULL -> false
            if (sqlUnaryExpression.OperatorType == ExpressionType.Equal
                && sqlUnaryExpression.Operand is SqlConstantExpression innerConstantNull1)
            {
                return _sqlExpressionFactory.Constant(innerConstantNull1.Value == null, sqlUnaryExpression.TypeMapping);
            }

            // NULL IS NOT NULL -> false
            // non_nullablee_constant IS NOT NULL -> true
            if (sqlUnaryExpression.OperatorType == ExpressionType.NotEqual
                && sqlUnaryExpression.Operand is SqlConstantExpression innerConstantNull2)
            {
                return _sqlExpressionFactory.Constant(innerConstantNull2.Value != null, sqlUnaryExpression.TypeMapping);
            }

            if (sqlUnaryExpression.Operand is SqlUnaryExpression innerUnary)
            {
                if (sqlUnaryExpression.OperatorType == ExpressionType.Not)
                {
                    // !(!a) -> a
                    if (innerUnary.OperatorType == ExpressionType.Not)
                    {
                        return Visit(innerUnary.Operand);
                    }

                    if (innerUnary.OperatorType == ExpressionType.Equal)
                    {
                        //!(a IS NULL) -> a IS NOT NULL
                        return Visit(_sqlExpressionFactory.IsNotNull(innerUnary.Operand));
                    }

                    //!(a IS NOT NULL) -> a IS NULL
                    if (innerUnary.OperatorType == ExpressionType.NotEqual)
                    {
                        return Visit(_sqlExpressionFactory.IsNull(innerUnary.Operand));
                    }
                }

                // (!a) IS NULL <==> a IS NULL
                if (sqlUnaryExpression.OperatorType == ExpressionType.Equal
                    && innerUnary.OperatorType == ExpressionType.Not)
                {
                    return Visit(_sqlExpressionFactory.IsNull(innerUnary.Operand));
                }

                // (!a) IS NOT NULL <==> a IS NOT NULL
                if (sqlUnaryExpression.OperatorType == ExpressionType.NotEqual
                    && innerUnary.OperatorType == ExpressionType.Not)
                {
                    return Visit(_sqlExpressionFactory.IsNotNull(innerUnary.Operand));
                }
            }

            if (sqlUnaryExpression.Operand is SqlBinaryExpression innerBinary)
            {
                // De Morgan's
                if (innerBinary.OperatorType == ExpressionType.AndAlso
                    || innerBinary.OperatorType == ExpressionType.OrElse)
                {
                    var newLeft = (SqlExpression)Visit(_sqlExpressionFactory.Not(innerBinary.Left));
                    var newRight = (SqlExpression)Visit(_sqlExpressionFactory.Not(innerBinary.Right));

                    return innerBinary.OperatorType == ExpressionType.AndAlso
                        ? _sqlExpressionFactory.OrElse(newLeft, newRight)
                        : _sqlExpressionFactory.AndAlso(newLeft, newRight);
                }

                // note that those optimizations are only valid in 2-value logic
                // they are safe to do here because null semantics removes possibility of nulls in the tree
                // however if we decide to do "partial" null semantics (that doesn't distinguish between NULL and FALSE, e.g. for predicates)
                // we need to be extra careful here
                if (_expressionTypesNegationMap.ContainsKey(innerBinary.OperatorType))
                {
                    return Visit(
                        _sqlExpressionFactory.MakeBinary(
                            _expressionTypesNegationMap[innerBinary.OperatorType],
                            innerBinary.Left,
                            innerBinary.Right,
                            innerBinary.TypeMapping));
                }
            }

            var newOperand = (SqlExpression)Visit(sqlUnaryExpression.Operand);

            return sqlUnaryExpression.Update(newOperand);
        }

        private Expression VisitSqlBinaryExpression(SqlBinaryExpression sqlBinaryExpression)
        {
            var newLeft = (SqlExpression)Visit(sqlBinaryExpression.Left);
            var newRight = (SqlExpression)Visit(sqlBinaryExpression.Right);

            if (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso
                || sqlBinaryExpression.OperatorType == ExpressionType.OrElse)
            {

                var newLeftConstant = newLeft as SqlConstantExpression;
                var newRightConstant = newRight as SqlConstantExpression;

                // true && a -> a
                // true || a -> true
                // false && a -> false
                // false || a -> a 
                if (newLeftConstant != null)
                {
                    return sqlBinaryExpression.OperatorType == ExpressionType.AndAlso
                        ? (bool)newLeftConstant.Value
                            ? newRight
                            : newLeftConstant
                        : (bool)newLeftConstant.Value
                            ? newLeftConstant
                            : newRight;
                }
                else if (newRightConstant != null)
                {
                    // a && true -> a
                    // a || true -> true 
                    // a && false -> false
                    // a || false -> a
                    return sqlBinaryExpression.OperatorType == ExpressionType.AndAlso
                        ? (bool)newRightConstant.Value
                            ? newLeft
                            : newRightConstant
                        : (bool)newRightConstant.Value
                            ? newRightConstant
                            : newLeft;
                }

                return sqlBinaryExpression.Update(newLeft, newRight);
            }

            return sqlBinaryExpression.Update(newLeft, newRight);
        }
    }
}
