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
    ///         An expression that represents an unary operation in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class SqlUnaryExpression : SqlExpression
    {
        private static readonly ISet<ExpressionType> _allowedOperators = new HashSet<ExpressionType>
        {
            ExpressionType.Equal,
            ExpressionType.NotEqual,
            ExpressionType.Convert,
            ExpressionType.Not,
            ExpressionType.Negate
        };

        internal static bool IsValidOperator(ExpressionType operatorType)
            => _allowedOperators.Contains(operatorType);

        /// <summary>
        ///     Creates a new instance of the <see cref="SqlUnaryExpression" /> class.
        /// </summary>
        /// <param name="operatorType"> The operator to apply. </param>
        /// <param name="operand"> An expression on which operator is applied. </param>
        /// <param name="type"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        public SqlUnaryExpression(
            ExpressionType operatorType,
            [NotNull] SqlExpression operand,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : base(type, typeMapping)
        {
            Check.NotNull(operand, nameof(operand));
            Check.NotNull(type, nameof(type));

            if (!IsValidOperator(operatorType))
            {
                throw new InvalidOperationException(
                    RelationalStrings.UnsupportedOperatorForSqlExpression(
                        operatorType, typeof(SqlUnaryExpression).ShortDisplayName()));
            }

            OperatorType = operatorType;
            Operand = operand;
        }

        /// <summary>
        ///     The operator of this SQL unary operation.
        /// </summary>
        public virtual ExpressionType OperatorType { get; }

        /// <summary>
        ///     The operand of this SQL unary operation.
        /// </summary>
        public virtual SqlExpression Operand { get; }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return Update((SqlExpression)visitor.Visit(Operand));
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="operand"> The <see cref="Operand" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual SqlUnaryExpression Update([NotNull] SqlExpression operand)
        {
            Check.NotNull(operand, nameof(operand));

            return operand != Operand
                ? new SqlUnaryExpression(OperatorType, operand, Type, TypeMapping)
                : this;
        }

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            if (OperatorType == ExpressionType.Convert)
            {
                expressionPrinter.Append("CAST(");
                expressionPrinter.Visit(Operand);
                expressionPrinter.Append(")");
                expressionPrinter.Append(" AS ");
                expressionPrinter.Append(TypeMapping.StoreType);
                expressionPrinter.Append(")");
            }
            else
            {
                expressionPrinter.Append(OperatorType.ToString());
                expressionPrinter.Append("(");
                expressionPrinter.Visit(Operand);
                expressionPrinter.Append(")");
            }
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is SqlUnaryExpression sqlUnaryExpression
                    && Equals(sqlUnaryExpression));

        private bool Equals(SqlUnaryExpression sqlUnaryExpression)
            => base.Equals(sqlUnaryExpression)
                && OperatorType == sqlUnaryExpression.OperatorType
                && Operand.Equals(sqlUnaryExpression.Operand);

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), OperatorType, Operand);
    }
}
