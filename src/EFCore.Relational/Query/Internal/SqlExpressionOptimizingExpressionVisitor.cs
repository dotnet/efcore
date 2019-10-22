// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class SqlExpressionOptimizingExpressionVisitor : ExpressionVisitor
    {
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
            SqlExpressionFactory = sqlExpressionFactory;
            _useRelationalNulls = useRelationalNulls;
        }

        protected virtual ISqlExpressionFactory SqlExpressionFactory { get; }

        protected override Expression VisitExtension(Expression extensionExpression)
            => extensionExpression switch
                {
                    SqlUnaryExpression sqlUnaryExpression => VisitSqlUnaryExpression(sqlUnaryExpression),
                    SqlBinaryExpression sqlBinaryExpression => VisitSqlBinaryExpression(sqlBinaryExpression),
                    _ => base.VisitExtension(extensionExpression),
                };

        protected virtual Expression VisitSqlUnaryExpression(SqlUnaryExpression sqlUnaryExpression)
        {
            switch (sqlUnaryExpression.OperatorType)
            {
                case ExpressionType.Not:
                    return VisitNot(sqlUnaryExpression);

                case ExpressionType.Equal:
                    switch (sqlUnaryExpression.Operand)
                    {
                        case SqlConstantExpression constantOperand:
                            return SqlExpressionFactory.Constant(constantOperand.Value == null, sqlUnaryExpression.TypeMapping);

                        case ColumnExpression columnOperand
                        when !columnOperand.IsNullable:
                            return SqlExpressionFactory.Constant(false, sqlUnaryExpression.TypeMapping);

                        case SqlUnaryExpression sqlUnaryOperand
                        when sqlUnaryOperand.OperatorType == ExpressionType.Convert
                            || sqlUnaryOperand.OperatorType == ExpressionType.Not
                            || sqlUnaryOperand.OperatorType == ExpressionType.Negate:
                            return (SqlExpression)Visit(SqlExpressionFactory.IsNull(sqlUnaryOperand.Operand));

                        case SqlBinaryExpression sqlBinaryOperand:
                            var newLeft = (SqlExpression)Visit(SqlExpressionFactory.IsNull(sqlBinaryOperand.Left));
                            var newRight = (SqlExpression)Visit(SqlExpressionFactory.IsNull(sqlBinaryOperand.Right));

                            return sqlBinaryOperand.OperatorType == ExpressionType.Coalesce
                                ? SimplifyLogicalSqlBinaryExpression(ExpressionType.AndAlso, newLeft, newRight, sqlBinaryOperand.TypeMapping)
                                : SimplifyLogicalSqlBinaryExpression(ExpressionType.OrElse, newLeft, newRight, sqlBinaryOperand.TypeMapping);
                    }
                    break;

                case ExpressionType.NotEqual:
                    switch (sqlUnaryExpression.Operand)
                    {
                        case SqlConstantExpression constantOperand:
                            return SqlExpressionFactory.Constant(constantOperand.Value != null, sqlUnaryExpression.TypeMapping);

                        case ColumnExpression columnOperand
                        when !columnOperand.IsNullable:
                            return SqlExpressionFactory.Constant(true, sqlUnaryExpression.TypeMapping);

                        case SqlUnaryExpression sqlUnaryOperand
                        when sqlUnaryOperand.OperatorType == ExpressionType.Convert
                            || sqlUnaryOperand.OperatorType == ExpressionType.Not
                            || sqlUnaryOperand.OperatorType == ExpressionType.Negate:
                            return (SqlExpression)Visit(SqlExpressionFactory.IsNotNull(sqlUnaryOperand.Operand));

                        case SqlBinaryExpression sqlBinaryOperand:
                            var newLeft = (SqlExpression)Visit(SqlExpressionFactory.IsNotNull(sqlBinaryOperand.Left));
                            var newRight = (SqlExpression)Visit(SqlExpressionFactory.IsNotNull(sqlBinaryOperand.Right));

                            return sqlBinaryOperand.OperatorType == ExpressionType.Coalesce
                                ? SimplifyLogicalSqlBinaryExpression(ExpressionType.OrElse, newLeft, newRight, sqlBinaryOperand.TypeMapping)
                                : SimplifyLogicalSqlBinaryExpression(ExpressionType.AndAlso, newLeft, newRight, sqlBinaryOperand.TypeMapping);
                    }
                    break;
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
                return SqlExpressionFactory.Constant(!value, sqlUnaryExpression.TypeMapping);
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
                    return Visit(SqlExpressionFactory.IsNotNull(innerUnary.Operand));
                }

                //!(a IS NOT NULL) -> a IS NULL
                if (innerUnary.OperatorType == ExpressionType.NotEqual)
                {
                    return Visit(SqlExpressionFactory.IsNull(innerUnary.Operand));
                }
            }

            if (sqlUnaryExpression.Operand is SqlBinaryExpression innerBinary)
            {
                // De Morgan's
                if (innerBinary.OperatorType == ExpressionType.AndAlso
                    || innerBinary.OperatorType == ExpressionType.OrElse)
                {
                    var newLeft = (SqlExpression)Visit(SqlExpressionFactory.Not(innerBinary.Left));
                    var newRight = (SqlExpression)Visit(SqlExpressionFactory.Not(innerBinary.Right));

                    return SimplifyLogicalSqlBinaryExpression(
                        innerBinary.OperatorType == ExpressionType.AndAlso
                            ? ExpressionType.OrElse
                            : ExpressionType.AndAlso,
                        newLeft,
                        newRight,
                        innerBinary.TypeMapping);
                }

                // those optimizations are only valid in 2-value logic
                // they are safe to do here because null semantics removes possibility of nulls in the tree
                // however if we decide to do "partial" null semantics (that doesn't distinguish between NULL and FALSE, e.g. for predicates)
                // we need to be extra careful here
                if (!_useRelationalNulls && TryNegate(innerBinary.OperatorType, out var negated))
                {
                    return Visit(
                        SqlExpressionFactory.MakeBinary(
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
                return SimplifyLogicalSqlBinaryExpression(
                    sqlBinaryExpression.OperatorType,
                    newLeft,
                    newRight,
                    sqlBinaryExpression.TypeMapping);
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
                        : SqlExpressionFactory.MakeBinary(
                            negated,
                            sqlBinaryExpression.Left,
                            sqlBinaryExpression.Right,
                            sqlBinaryExpression.TypeMapping);
                }
            }

            return sqlBinaryExpression.Update(newLeft, newRight);
        }

        private SqlExpression SimplifyLogicalSqlBinaryExpression(
            ExpressionType operatorType,
            SqlExpression newLeft,
            SqlExpression newRight,
            RelationalTypeMapping typeMapping)
        {
            // true && a -> a
            // true || a -> true
            // false && a -> false
            // false || a -> a
            if (newLeft is SqlConstantExpression newLeftConstant)
            {
                return operatorType == ExpressionType.AndAlso
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
                return operatorType == ExpressionType.AndAlso
                    ? (bool)newRightConstant.Value
                        ? newLeft
                        : newRightConstant
                    : (bool)newRightConstant.Value
                        ? newRightConstant
                        : newLeft;
            }

            return SqlExpressionFactory.MakeBinary(operatorType, newLeft, newRight, typeMapping);
        }
    }
}
