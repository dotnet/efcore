// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents a constant in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class SqlConstantExpression : SqlExpression
    {
        private readonly ConstantExpression _constantExpression;

        /// <summary>
        ///     Creates a new instance of the <see cref="SqlConstantExpression" /> class.
        /// </summary>
        /// <param name="constantExpression"> A <see cref="ConstantExpression" />. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        public SqlConstantExpression([NotNull] ConstantExpression constantExpression, [CanBeNull] RelationalTypeMapping typeMapping)
            : base(Check.NotNull(constantExpression, nameof(constantExpression)).Type.UnwrapNullableType(), typeMapping)
        {
            _constantExpression = constantExpression;
        }

        /// <summary>
        ///     The constant value.
        /// </summary>
        public virtual object Value
            => _constantExpression.Value;

        /// <summary>
        ///     Applies supplied type mapping to this expression.
        /// </summary>
        /// <param name="typeMapping"> A relational type mapping to apply. </param>
        /// <returns> A new expression which has supplied type mapping. </returns>
        public virtual SqlExpression ApplyTypeMapping([CanBeNull] RelationalTypeMapping typeMapping)
            => new SqlConstantExpression(_constantExpression, typeMapping);

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return this;
        }

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            Print(Value, expressionPrinter);
        }

        private void Print(object value, ExpressionPrinter expressionPrinter)
            => expressionPrinter.Append(TypeMapping?.GenerateSqlLiteral(value) ?? Value?.ToString() ?? "NULL");

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is SqlConstantExpression sqlConstantExpression
                    && Equals(sqlConstantExpression));

        private bool Equals(SqlConstantExpression sqlConstantExpression)
            => base.Equals(sqlConstantExpression)
                && ValueEquals(Value, sqlConstantExpression.Value);

        private bool ValueEquals(object value1, object value2)
        {
            if (value1 == null)
            {
                return value2 == null;
            }

            if (value1 is IList list1
                && value2 is IList list2)
            {
                if (list1.Count != list2.Count)
                {
                    return false;
                }

                for (var i = 0; i < list1.Count; i++)
                {
                    if (!ValueEquals(list1[i], list2[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return value1.Equals(value2);
        }

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), Value);
    }
}
