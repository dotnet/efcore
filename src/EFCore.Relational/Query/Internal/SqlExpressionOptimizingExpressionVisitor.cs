// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class SqlExpressionOptimizingExpressionVisitor : ExpressionVisitor
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly bool _useRelationalNulls;

        private static bool TryNegate(ExpressionType expressionType, out ExpressionType result)
        {
            var negated = expressionType switch {
                ExpressionType.AndAlso            => ExpressionType.OrElse,
                ExpressionType.OrElse             => ExpressionType.AndAlso,
                ExpressionType.Equal              => ExpressionType.NotEqual,
                ExpressionType.NotEqual           => ExpressionType.Equal,
                ExpressionType.GreaterThan        => ExpressionType.LessThanOrEqual,
                ExpressionType.GreaterThanOrEqual => ExpressionType.LessThan,
                ExpressionType.LessThan           => ExpressionType.GreaterThanOrEqual,
                ExpressionType.LessThanOrEqual    => ExpressionType.GreaterThan,
                _ => (ExpressionType?)null
            };

            result = negated ?? default;
            return negated.HasValue;
        }

        public SqlExpressionOptimizingExpressionVisitor(ISqlExpressionFactory sqlExpressionFactory, bool useRelationalNulls)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _useRelationalNulls = useRelationalNulls;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
            => extensionExpression switch
                {
                    SqlUnaryExpression sqlUnaryExpression => VisitSqlUnaryExpression(sqlUnaryExpression),
                    SqlBinaryExpression sqlBinaryExpression => VisitSqlBinaryExpression(sqlBinaryExpression),
                    _ => base.VisitExtension(extensionExpression),
                };

        private Expression VisitSqlUnaryExpression(SqlUnaryExpression sqlUnaryExpression)
        {
            if (sqlUnaryExpression.OperatorType == ExpressionType.Not)
            {
                return VisitNot(sqlUnaryExpression);
            }

            // NULL IS NULL -> true
            // non_nullable_constant IS NULL -> false
            if (sqlUnaryExpression.OperatorType == ExpressionType.Equal
                && sqlUnaryExpression.Operand is SqlConstantExpression innerConstantNull1)
            {
                return _sqlExpressionFactory.Constant(innerConstantNull1.Value == null, sqlUnaryExpression.TypeMapping);
            }

            // NULL IS NOT NULL -> false
            // non_nullable_constant IS NOT NULL -> true
            if (sqlUnaryExpression.OperatorType == ExpressionType.NotEqual
                && sqlUnaryExpression.Operand is SqlConstantExpression innerConstantNull2)
            {
                return _sqlExpressionFactory.Constant(innerConstantNull2.Value != null, sqlUnaryExpression.TypeMapping);
            }

            if (sqlUnaryExpression.Operand is SqlUnaryExpression innerUnary)
            {
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

            var newOperand = (SqlExpression)Visit(sqlUnaryExpression.Operand);

            return sqlUnaryExpression.Update(newOperand);
        }

        private Expression VisitNot(SqlUnaryExpression sqlUnaryExpression)
        {
            // !(true) -> false
            // !(false) -> true
            if (sqlUnaryExpression.Operand is SqlConstantExpression innerConstantBool
                && innerConstantBool.Value is bool value)
            {
                return _sqlExpressionFactory.Constant(!value, sqlUnaryExpression.TypeMapping);
            }

            if (sqlUnaryExpression.Operand is InExpression inExpression)
            {
                return Visit(inExpression.Negate());
            }

            if (sqlUnaryExpression.Operand is SqlUnaryExpression innerUnary)
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

                // those optimizations are only valid in 2-value logic
                // they are safe to do here because null semantics removes possibility of nulls in the tree
                // however if we decide to do "partial" null semantics (that doesn't distinguish between NULL and FALSE, e.g. for predicates)
                // we need to be extra careful here
                if (!_useRelationalNulls && TryNegate(innerBinary.OperatorType, out var negated))
                {
                    return Visit(
                        _sqlExpressionFactory.MakeBinary(
                            negated,
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
                // true && a -> a
                // true || a -> true
                // false && a -> false
                // false || a -> a
                if (newLeft is SqlConstantExpression newLeftConstant)
                {
                    return sqlBinaryExpression.OperatorType == ExpressionType.AndAlso
                        ? (bool)newLeftConstant.Value
                            ? newRight
                            : newLeftConstant
                        : (bool)newLeftConstant.Value
                            ? newLeftConstant
                            : newRight;
                }
                else if (newRight is SqlConstantExpression newRightConstant)
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

            // those optimizations are only valid in 2-value logic
            // they are safe to do here because null semantics removes possibility of nulls in the tree
            // however if we decide to do "partial" null semantics (that doesn't distinguish between NULL and FALSE, e.g. for predicates)
            // we need to be extra careful here
            if (!_useRelationalNulls
                && (sqlBinaryExpression.OperatorType == ExpressionType.Equal || sqlBinaryExpression.OperatorType == ExpressionType.NotEqual))
            {
                // op(a, b) == true -> op(a, b)
                // op(a, b) != false -> op(a, b)
                // op(a, b) == false -> !op(a, b)
                // op(a, b) != true -> !op(a, b)
                var constant = sqlBinaryExpression.Left as SqlConstantExpression ?? sqlBinaryExpression.Right as SqlConstantExpression;
                var binary = sqlBinaryExpression.Left as SqlBinaryExpression ?? sqlBinaryExpression.Right as SqlBinaryExpression;
                if (constant != null && binary != null && TryNegate(binary.OperatorType, out var negated))
                {
                    return (bool)constant.Value == (sqlBinaryExpression.OperatorType == ExpressionType.Equal)
                        ? binary
                        : _sqlExpressionFactory.MakeBinary(
                            negated,
                            sqlBinaryExpression.Left,
                            sqlBinaryExpression.Right,
                            sqlBinaryExpression.TypeMapping);
                }
            }

            return sqlBinaryExpression.Update(newLeft, newRight);
        }
    }
}
