// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class SqlBinaryExpression : SqlExpression
    {
        private static ISet<ExpressionType> _allowedOperators = new HashSet<ExpressionType>
        {
            ExpressionType.Add,
            //ExpressionType.AddChecked,
            ExpressionType.Subtract,
            //ExpressionType.SubtractChecked,
            ExpressionType.Multiply,
            //ExpressionType.MultiplyChecked,
            ExpressionType.Divide,
            ExpressionType.Modulo,
            //ExpressionType.Power,
            ExpressionType.And,
            ExpressionType.AndAlso,
            ExpressionType.Or,
            ExpressionType.OrElse,
            ExpressionType.LessThan,
            ExpressionType.LessThanOrEqual,
            ExpressionType.GreaterThan,
            ExpressionType.GreaterThanOrEqual,
            ExpressionType.Equal,
            ExpressionType.NotEqual,
            //ExpressionType.ExclusiveOr,
            ExpressionType.Coalesce,
            //ExpressionType.ArrayIndex,
            //ExpressionType.RightShift,
            //ExpressionType.LeftShift,
        };

        public SqlBinaryExpression(
            ExpressionType operatorType,
            SqlExpression left,
            SqlExpression right,
            Type type,
            RelationalTypeMapping typeMapping)
            : base(type, typeMapping, IsConditionFromOperator(VerifyOperator(operatorType)), !IsConditionFromOperator(operatorType))
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            OperatorType = operatorType;

            var conditionArgument = operatorType == ExpressionType.AndAlso || operatorType == ExpressionType.OrElse;

            Left = left.ConvertToValue(!conditionArgument);
            Right = right.ConvertToValue(!conditionArgument);
        }

        private SqlBinaryExpression(
            ExpressionType operatorType,
            SqlExpression left,
            SqlExpression right,
            Type type,
            RelationalTypeMapping typeMapping,
            bool treatAsValue)
            : base(type, typeMapping, IsConditionFromOperator(operatorType), treatAsValue)
        {
            OperatorType = operatorType;
            Left = left;
            Right = right;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var left = (SqlExpression)visitor.Visit(Left);
            var right = (SqlExpression)visitor.Visit(Right);

            return left != Left || right != Right
                ? new SqlBinaryExpression(OperatorType, left, right, Type, TypeMapping, ShouldBeValue)
                : this;
        }

        private static bool IsConditionFromOperator(ExpressionType operatorType)
        {
            return operatorType == ExpressionType.AndAlso
                || operatorType == ExpressionType.OrElse
                || operatorType == ExpressionType.LessThan
                || operatorType == ExpressionType.LessThanOrEqual
                || operatorType == ExpressionType.GreaterThan
                || operatorType == ExpressionType.GreaterThanOrEqual
                || operatorType == ExpressionType.Equal
                || operatorType == ExpressionType.NotEqual;
        }

        private static ExpressionType VerifyOperator(ExpressionType operatorType)
        {
            return _allowedOperators.Contains(operatorType)
                ? operatorType
                : throw new InvalidOperationException("Unsupported Binary operator type specified.");

        }

        public override SqlExpression ConvertToValue(bool treatAsValue)
        {
            return new SqlBinaryExpression(OperatorType, Left, Right, Type, TypeMapping, treatAsValue);
        }

        public ExpressionType OperatorType { get; }
        public SqlExpression Left { get; }
        public SqlExpression Right { get; }

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlBinaryExpression sqlBinaryExpression
                    && Equals(sqlBinaryExpression));

        private bool Equals(SqlBinaryExpression sqlBinaryExpression)
            => base.Equals(sqlBinaryExpression)
            && OperatorType == sqlBinaryExpression.OperatorType
            && Left.Equals(sqlBinaryExpression.Left)
            && Right.Equals(sqlBinaryExpression.Right);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ OperatorType.GetHashCode();
                hashCode = (hashCode * 397) ^ Left.GetHashCode();
                hashCode = (hashCode * 397) ^ Right.GetHashCode();

                return hashCode;
            }
        }
    }
}
