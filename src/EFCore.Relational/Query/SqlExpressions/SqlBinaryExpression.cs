// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents a binary operation in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class SqlBinaryExpression : SqlExpression
    {
        private static readonly ISet<ExpressionType> _allowedOperators = new HashSet<ExpressionType>
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
            //ExpressionType.ArrayIndex,
            //ExpressionType.RightShift,
            //ExpressionType.LeftShift,
        };

        internal static bool IsValidOperator(ExpressionType operatorType)
            => _allowedOperators.Contains(operatorType);

        /// <summary>
        ///     Creates a new instance of the <see cref="SqlBinaryExpression" /> class.
        /// </summary>
        /// <param name="operatorType"> The operator to apply. </param>
        /// <param name="left"> An expression which is left operand. </param>
        /// <param name="right"> An expression which is right operand. </param>
        /// <param name="type"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        public SqlBinaryExpression(
            ExpressionType operatorType,
            [NotNull] SqlExpression left,
            [NotNull] SqlExpression right,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : base(type, typeMapping)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            if (!IsValidOperator(operatorType))
            {
                throw new InvalidOperationException(
                    RelationalStrings.UnsupportedOperatorForSqlExpression(
                        operatorType, typeof(SqlBinaryExpression).ShortDisplayName()));
            }

            OperatorType = operatorType;
            Left = left;
            Right = right;
        }

        /// <summary>
        ///     The operator of this SQL binary operation.
        /// </summary>
        public virtual ExpressionType OperatorType { get; }

        /// <summary>
        ///     The left operand.
        /// </summary>
        public virtual SqlExpression Left { get; }

        /// <summary>
        ///     The right operand.
        /// </summary>
        public virtual SqlExpression Right { get; }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var left = (SqlExpression)visitor.Visit(Left);
            var right = (SqlExpression)visitor.Visit(Right);

            return Update(left, right);
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="left"> The <see cref="Left" /> property of the result. </param>
        /// <param name="right"> The <see cref="Right" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual SqlBinaryExpression Update([NotNull] SqlExpression left, [NotNull] SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return left != Left || right != Right
                ? new SqlBinaryExpression(OperatorType, left, right, Type, TypeMapping)
                : this;
        }

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            var requiresBrackets = RequiresBrackets(Left);

            if (requiresBrackets)
            {
                expressionPrinter.Append("(");
            }

            expressionPrinter.Visit(Left);

            if (requiresBrackets)
            {
                expressionPrinter.Append(")");
            }

            expressionPrinter.Append(expressionPrinter.GenerateBinaryOperator(OperatorType));

            requiresBrackets = RequiresBrackets(Right);

            if (requiresBrackets)
            {
                expressionPrinter.Append("(");
            }

            expressionPrinter.Visit(Right);

            if (requiresBrackets)
            {
                expressionPrinter.Append(")");
            }

            static bool RequiresBrackets(SqlExpression expression)
                => expression is SqlBinaryExpression || expression is LikeExpression;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), OperatorType, Left, Right);
    }
}
