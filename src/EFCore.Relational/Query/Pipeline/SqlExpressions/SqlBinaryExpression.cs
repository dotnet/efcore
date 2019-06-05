// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class SqlBinaryExpression : SqlExpression
    {
        private static ISet<ExpressionType> _allowedOperators = new HashSet<ExpressionType>
        {
            ExpressionType.Add,
            ExpressionType.Subtract,
            ExpressionType.Multiply,
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

        private static ExpressionType VerifyOperator(ExpressionType operatorType)
            => _allowedOperators.Contains(operatorType)
                ? operatorType
                : throw new InvalidOperationException("Unsupported Binary operator type specified.");

        public SqlBinaryExpression(
            ExpressionType operatorType,
            SqlExpression left,
            SqlExpression right,
            Type type,
            RelationalTypeMapping typeMapping)
            : base(type, typeMapping)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            OperatorType = VerifyOperator(operatorType);

            Left = left;
            Right = right;
        }

        public ExpressionType OperatorType { get; }
        public SqlExpression Left { get; }
        public SqlExpression Right { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var left = (SqlExpression)visitor.Visit(Left);
            var right = (SqlExpression)visitor.Visit(Right);

            return Update(left, right);
        }

        public SqlBinaryExpression Update(SqlExpression left, SqlExpression right)
            => left != Left || right != Right
                ? new SqlBinaryExpression(OperatorType, left, right, Type, TypeMapping)
                : this;

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            var requiresBrackets = RequiresBrackets(Left);

            if (requiresBrackets)
            {
                expressionPrinter.StringBuilder.Append("(");
            }

            expressionPrinter.Visit(Left);

            if (requiresBrackets)
            {
                expressionPrinter.StringBuilder.Append(")");
            }

            expressionPrinter.StringBuilder.Append(expressionPrinter.GenerateBinaryOperator(OperatorType));

            requiresBrackets = RequiresBrackets(Right);

            if (requiresBrackets)
            {
                expressionPrinter.StringBuilder.Append("(");
            }

            expressionPrinter.Visit(Right);

            if (requiresBrackets)
            {
                expressionPrinter.StringBuilder.Append(")");
            }
        }

        private bool RequiresBrackets(SqlExpression expression)
        {
            return expression is SqlBinaryExpression sqlBinary
                && sqlBinary.OperatorType != ExpressionType.Coalesce
                || expression is LikeExpression;
        }

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
