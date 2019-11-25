// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
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
            var negated = expressionType switch
            {
                ExpressionType.AndAlso => ExpressionType.OrElse,
                ExpressionType.OrElse => ExpressionType.AndAlso,
                ExpressionType.Equal => ExpressionType.NotEqual,
                ExpressionType.NotEqual => ExpressionType.Equal,
                ExpressionType.GreaterThan => ExpressionType.LessThanOrEqual,
                ExpressionType.GreaterThanOrEqual => ExpressionType.LessThan,
                ExpressionType.LessThan => ExpressionType.GreaterThanOrEqual,
                ExpressionType.LessThanOrEqual => ExpressionType.GreaterThan,
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
                SelectExpression selectExpression => VisitSelectExpression(selectExpression),
                _ => base.VisitExtension(extensionExpression),
            };

        private Expression VisitSelectExpression(SelectExpression selectExpression)
        {
            var newExpression = base.VisitExtension(selectExpression);

            // if predicate is optimized to true, we can simply remove it
            if (newExpression is SelectExpression newSelectExpression)
            {
                var changed = false;
                var newPredicate = newSelectExpression.Predicate;
                var newHaving = newSelectExpression.Having;
                if (newSelectExpression.Predicate is SqlConstantExpression predicateConstantExpression
                    && predicateConstantExpression.Value is bool predicateBoolValue
                    && predicateBoolValue)
                {
                    newPredicate = null;
                    changed = true;
                }

                if (newSelectExpression.Having is SqlConstantExpression havingConstantExpression
                    && havingConstantExpression.Value is bool havingBoolValue
                    && havingBoolValue)
                {
                    newHaving = null;
                    changed = true;
                }

                return changed
                    ? newSelectExpression.Update(
                        newSelectExpression.Projection.ToList(),
                        newSelectExpression.Tables.ToList(),
                        newPredicate,
                        newSelectExpression.GroupBy.ToList(),
                        newHaving,
                        newSelectExpression.Orderings.ToList(),
                        newSelectExpression.Limit,
                        newSelectExpression.Offset,
                        newSelectExpression.IsDistinct,
                        newSelectExpression.Alias)
                    : newSelectExpression;
            }

            return newExpression;
        }

        protected virtual Expression VisitSqlUnaryExpression(SqlUnaryExpression sqlUnaryExpression)
        {
            var newOperand = (SqlExpression)Visit(sqlUnaryExpression.Operand);

            return SimplifyUnaryExpression(
                sqlUnaryExpression.OperatorType,
                newOperand,
                sqlUnaryExpression.Type,
                sqlUnaryExpression.TypeMapping);
        }

        private SqlExpression SimplifyUnaryExpression(
            ExpressionType operatorType,
            SqlExpression operand,
            Type type,
            RelationalTypeMapping typeMapping)
        {
            switch (operatorType)
            {
                case ExpressionType.Not:
                    switch (operand)
                    {
                        // !(true) -> false
                        // !(false) -> true
                        case SqlConstantExpression constantOperand
                        when constantOperand.Value is bool value:
                        {
                            return SqlExpressionFactory.Constant(!value, typeMapping);
                        }

                        case InExpression inOperand:
                            return inOperand.Negate();

                        case SqlUnaryExpression unaryOperand:
                            switch (unaryOperand.OperatorType)
                            {
                                // !(!a) -> a
                                case ExpressionType.Not:
                                    return unaryOperand.Operand;

                                //!(a IS NULL) -> a IS NOT NULL
                                case ExpressionType.Equal:
                                    return SqlExpressionFactory.IsNotNull(unaryOperand.Operand);

                                //!(a IS NOT NULL) -> a IS NULL
                                case ExpressionType.NotEqual:
                                    return SqlExpressionFactory.IsNull(unaryOperand.Operand);
                            }
                            break;

                        case SqlBinaryExpression binaryOperand:
                        {
                            // De Morgan's
                            if (binaryOperand.OperatorType == ExpressionType.AndAlso
                                || binaryOperand.OperatorType == ExpressionType.OrElse)
                            {
                                var newLeft = SimplifyUnaryExpression(ExpressionType.Not, binaryOperand.Left, type, typeMapping);
                                var newRight = SimplifyUnaryExpression(ExpressionType.Not, binaryOperand.Right, type, typeMapping);

                                return SimplifyLogicalSqlBinaryExpression(
                                    binaryOperand.OperatorType == ExpressionType.AndAlso
                                        ? ExpressionType.OrElse
                                        : ExpressionType.AndAlso,
                                    newLeft,
                                    newRight,
                                    binaryOperand.TypeMapping);
                            }

                            // those optimizations are only valid in 2-value logic
                            // they are safe to do here because if we apply null semantics
                            // because null semantics removes possibility of nulls in the tree when the comparison is wrapped around NOT
                            if (!_useRelationalNulls && TryNegate(binaryOperand.OperatorType, out var negated))
                            {
                                return SimplifyBinaryExpression(
                                    negated,
                                    binaryOperand.Left,
                                    binaryOperand.Right,
                                    binaryOperand.TypeMapping);
                            }
                        }
                        break;
                    }
                    break;

                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    return SimplifyNullNotNullExpression(
                        operatorType,
                        operand,
                        type,
                        typeMapping);
            }

            return SqlExpressionFactory.MakeUnary(operatorType, operand, type, typeMapping);
        }

        private SqlExpression SimplifyNullNotNullExpression(
            ExpressionType operatorType,
            SqlExpression operand,
            Type type,
            RelationalTypeMapping typeMapping)
        {
            switch (operatorType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    switch (operand)
                    {
                        case SqlConstantExpression constantOperand:
                            return SqlExpressionFactory.Constant(
                                operatorType == ExpressionType.Equal
                                    ? constantOperand.Value == null
                                    : constantOperand.Value != null,
                                typeMapping);

                        case ColumnExpression columnOperand
                            when !columnOperand.IsNullable:
                            return SqlExpressionFactory.Constant(operatorType == ExpressionType.NotEqual, typeMapping);

                        case SqlUnaryExpression sqlUnaryOperand:
                            if (sqlUnaryOperand.OperatorType == ExpressionType.Convert
                                || sqlUnaryOperand.OperatorType == ExpressionType.Not
                                || sqlUnaryOperand.OperatorType == ExpressionType.Negate)
                            {
                                // op(a) is null -> a is null
                                // op(a) is not null -> a is not null
                                return SimplifyNullNotNullExpression(operatorType, sqlUnaryOperand.Operand, type, typeMapping);
                            }

                            if (sqlUnaryOperand.OperatorType == ExpressionType.Equal
                                || sqlUnaryOperand.OperatorType == ExpressionType.NotEqual)
                            {
                                // (a is null) is null -> false
                                // (a is not null) is null -> false
                                // (a is null) is not null -> true
                                // (a is not null) is not null -> true
                                return SqlExpressionFactory.Constant(operatorType == ExpressionType.NotEqual, typeMapping);
                            }
                            break;

                        case SqlBinaryExpression sqlBinaryOperand:
                            // in general:
                            // binaryOp(a, b) == null -> a == null || b == null
                            // binaryOp(a, b) != null -> a != null && b != null
                            // for coalesce:
                            // (a ?? b) == null -> a == null && b == null
                            // (a ?? b) != null -> a != null || b != null
                            // for AndAlso, OrElse, And, Or we can't do this optimization
                            // we could do something like this, but it seems too complicated:
                            // (a && b) == null -> a == null && b != 0 || a != 0 && b == null
                            if (sqlBinaryOperand.OperatorType != ExpressionType.AndAlso
                                && sqlBinaryOperand.OperatorType != ExpressionType.OrElse
                                && sqlBinaryOperand.OperatorType != ExpressionType.And
                                && sqlBinaryOperand.OperatorType != ExpressionType.Or)
                            {
                                var newLeft = SimplifyNullNotNullExpression(operatorType, sqlBinaryOperand.Left, typeof(bool), typeMapping);
                                var newRight = SimplifyNullNotNullExpression(operatorType, sqlBinaryOperand.Right, typeof(bool), typeMapping);

                                return sqlBinaryOperand.OperatorType == ExpressionType.Coalesce
                                    ? SimplifyLogicalSqlBinaryExpression(
                                        operatorType == ExpressionType.Equal
                                            ? ExpressionType.AndAlso
                                            : ExpressionType.OrElse,
                                        newLeft,
                                        newRight,
                                        typeMapping)
                                    : SimplifyLogicalSqlBinaryExpression(
                                        operatorType == ExpressionType.Equal
                                            ? ExpressionType.OrElse
                                            : ExpressionType.AndAlso,
                                        newLeft,
                                        newRight,
                                        typeMapping);
                            }
                            break;
                    }
                    break;
            }

            return SqlExpressionFactory.MakeUnary(operatorType, operand, type, typeMapping);
        }

        protected virtual Expression VisitSqlBinaryExpression(SqlBinaryExpression sqlBinaryExpression)
        {
            var newLeft = (SqlExpression)Visit(sqlBinaryExpression.Left);
            var newRight = (SqlExpression)Visit(sqlBinaryExpression.Right);

            return SimplifyBinaryExpression(
                sqlBinaryExpression.OperatorType,
                newLeft,
                newRight,
                sqlBinaryExpression.TypeMapping);
        }

        private SqlExpression SimplifyBinaryExpression(
            ExpressionType operatorType,
            SqlExpression left,
            SqlExpression right,
            RelationalTypeMapping typeMapping)
        {
            switch (operatorType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    var leftUnary = left as SqlUnaryExpression;
                    var rightUnary = right as SqlUnaryExpression;
                    if (leftUnary != null
                        && rightUnary != null
                        && (leftUnary.OperatorType == ExpressionType.Equal || leftUnary.OperatorType == ExpressionType.NotEqual)
                        && (rightUnary.OperatorType == ExpressionType.Equal || rightUnary.OperatorType == ExpressionType.NotEqual)
                        && leftUnary.Operand.Equals(rightUnary.Operand))
                    {
                        // a is null || a is null -> a is null
                        // a is not null || a is not null -> a is not null
                        // a is null && a is null -> a is null
                        // a is not null && a is not null -> a is not null
                        // a is null || a is not null -> true
                        // a is null && a is not null -> false
                        return leftUnary.OperatorType == rightUnary.OperatorType
                            ? (SqlExpression)leftUnary
                            : SqlExpressionFactory.Constant(operatorType == ExpressionType.OrElse, typeMapping);
                    }

                    return SimplifyLogicalSqlBinaryExpression(
                        operatorType,
                        left,
                        right,
                        typeMapping);

                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    var leftConstant = left as SqlConstantExpression;
                    var rightConstant = right as SqlConstantExpression;
                    var leftNullConstant = leftConstant != null && leftConstant.Value == null;
                    var rightNullConstant = rightConstant != null && rightConstant.Value == null;
                    if (leftNullConstant || rightNullConstant)
                    {
                        return SimplifyNullComparisonExpression(
                            operatorType,
                            left,
                            right,
                            leftNullConstant,
                            rightNullConstant,
                            typeMapping);
                    }

                    var leftBoolConstant = left.Type == typeof(bool) ? leftConstant : null;
                    var rightBoolConstant = right.Type == typeof(bool) ? rightConstant : null;
                    if (leftBoolConstant != null || rightBoolConstant != null)
                    {
                        return SimplifyBoolConstantComparisonExpression(
                            operatorType,
                            left,
                            right,
                            leftBoolConstant,
                            rightBoolConstant,
                            typeMapping);
                    }

                    // only works when a is not nullable
                    // a == a -> true
                    // a != a -> false
                    if ((left is LikeExpression
                        || left is ColumnExpression columnExpression && !columnExpression.IsNullable)
                        && left.Equals(right))
                    {
                        return SqlExpressionFactory.Constant(operatorType == ExpressionType.Equal, typeMapping);
                    }

                    break;
            }

            return SqlExpressionFactory.MakeBinary(operatorType, left, right, typeMapping);
        }

        protected virtual SqlExpression SimplifyNullComparisonExpression(
            ExpressionType operatorType,
            SqlExpression left,
            SqlExpression right,
            bool leftNull,
            bool rightNull,
            RelationalTypeMapping typeMapping)
        {
            if ((operatorType == ExpressionType.Equal || operatorType == ExpressionType.NotEqual)
                && (leftNull || rightNull))
            {
                if (leftNull && rightNull)
                {
                    return SqlExpressionFactory.Constant(operatorType == ExpressionType.Equal, typeMapping);
                }

                if (leftNull)
                {
                    return SimplifyNullNotNullExpression(operatorType, right, typeof(bool), typeMapping);
                }

                if (rightNull)
                {
                    return SimplifyNullNotNullExpression(operatorType, left, typeof(bool), typeMapping);
                }
            }

            return SqlExpressionFactory.MakeBinary(operatorType, left, right, typeMapping);
        }

        private SqlExpression SimplifyBoolConstantComparisonExpression(
            ExpressionType operatorType,
            SqlExpression left,
            SqlExpression right,
            SqlConstantExpression leftBoolConstant,
            SqlConstantExpression rightBoolConstant,
            RelationalTypeMapping typeMapping)
        {
            if (leftBoolConstant != null && rightBoolConstant != null)
            {
                // potential optimization:
                // we can't do it on SqlServer because it reverts search conditions back to values
                // and we run this visitor after search condition visitor
                //return operatorType == ExpressionType.Equal
                //    ? SqlExpressionFactory.Constant((bool)leftBoolConstant.Value == (bool)rightBoolConstant.Value, typeMapping)
                //    : SqlExpressionFactory.Constant((bool)leftBoolConstant.Value != (bool)rightBoolConstant.Value, typeMapping);
                return SqlExpressionFactory.MakeBinary(operatorType, left, right, typeMapping);
            }

            if (rightBoolConstant != null
                && CanOptimize(left))
            {
                // a == true -> a
                // a == false -> !a
                // a != true -> !a
                // a != false -> a
                // only correct when f(x) can't be null
                return operatorType == ExpressionType.Equal
                    ? (bool)rightBoolConstant.Value
                        ? left
                        : SimplifyUnaryExpression(ExpressionType.Not, left, typeof(bool), typeMapping)
                    : (bool)rightBoolConstant.Value
                        ? SimplifyUnaryExpression(ExpressionType.Not, left, typeof(bool), typeMapping)
                        : left;
            }

            if (leftBoolConstant != null
                && CanOptimize(right))
            {
                // true == a -> a
                // false == a -> !a
                // true != a -> !a
                // false != a -> a
                // only correct when a can't be null
                return operatorType == ExpressionType.Equal
                    ? (bool)leftBoolConstant.Value
                        ? right
                        : SimplifyUnaryExpression(ExpressionType.Not, right, typeof(bool), typeMapping)
                    : (bool)leftBoolConstant.Value
                        ? SimplifyUnaryExpression(ExpressionType.Not, right, typeof(bool), typeMapping)
                        : right;
            }

            return SqlExpressionFactory.MakeBinary(operatorType, left, right, typeMapping);

            static bool CanOptimize(SqlExpression operand)
                => operand is LikeExpression
                || (operand is SqlUnaryExpression sqlUnary
                    && (sqlUnary.OperatorType == ExpressionType.Equal
                        || sqlUnary.OperatorType == ExpressionType.NotEqual
                        // TODO: #18689
                        /*|| sqlUnary.OperatorType == ExpressionType.Not*/));
        }

        private SqlExpression SimplifyLogicalSqlBinaryExpression(
            ExpressionType operatorType,
            SqlExpression left,
            SqlExpression right,
            RelationalTypeMapping typeMapping)
        {
            // true && a -> a
            // true || a -> true
            // false && a -> false
            // false || a -> a
            if (left is SqlConstantExpression newLeftConstant)
            {
                return operatorType == ExpressionType.AndAlso
                    ? (bool)newLeftConstant.Value
                        ? right
                        : newLeftConstant
                    : (bool)newLeftConstant.Value
                        ? newLeftConstant
                        : right;
            }
            else if (right is SqlConstantExpression newRightConstant)
            {
                // a && true -> a
                // a || true -> true
                // a && false -> false
                // a || false -> a
                return operatorType == ExpressionType.AndAlso
                    ? (bool)newRightConstant.Value
                        ? left
                        : newRightConstant
                    : (bool)newRightConstant.Value
                        ? newRightConstant
                        : left;
            }

            return SqlExpressionFactory.MakeBinary(operatorType, left, right, typeMapping);
        }
    }
}
