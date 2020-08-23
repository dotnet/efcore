// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents an IN operation in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class InExpression : SqlExpression
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="InExpression" /> class which represents a <paramref name="item" /> IN subquery expression.
        /// </summary>
        /// <param name="item"> An item to look into values. </param>
        /// <param name="negated"> A value indicating if the item should be present in the values or absent. </param>
        /// <param name="subquery"> A subquery in which item is searched. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        [Obsolete("Use overload which passes negated argument after subquery argument.")]
        public InExpression(
            [NotNull] SqlExpression item,
            bool negated,
            [NotNull] SelectExpression subquery,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : this(Check.NotNull(item, nameof(item)), null, Check.NotNull(subquery, nameof(subquery)), negated, typeMapping)
        {
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="InExpression" /> class which represents a <paramref name="item" /> IN values expression.
        /// </summary>
        /// <param name="item"> An item to look into values. </param>
        /// <param name="negated"> A value indicating if the item should be present in the values or absent. </param>
        /// <param name="values"> A list of values in which item is searched. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        [Obsolete("Use overload which passes negated argument after values argument.")]
        public InExpression(
            [NotNull] SqlExpression item,
            bool negated,
            [NotNull] SqlExpression values,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : this(Check.NotNull(item, nameof(item)), Check.NotNull(values, nameof(values)), null, negated, typeMapping)
        {
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="InExpression" /> class which represents a <paramref name="item" /> IN subquery expression.
        /// </summary>
        /// <param name="item"> An item to look into values. </param>
        /// <param name="subquery"> A subquery in which item is searched. </param>
        /// <param name="negated"> A value indicating if the item should be present in the values or absent. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        public InExpression(
            [NotNull] SqlExpression item,
            [NotNull] SelectExpression subquery,
            bool negated,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : this(Check.NotNull(item, nameof(item)), null, Check.NotNull(subquery, nameof(subquery)), negated, typeMapping)
        {
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="InExpression" /> class which represents a <paramref name="item" /> IN values expression.
        /// </summary>
        /// <param name="item"> An item to look into values. </param>
        /// <param name="values"> A list of values in which item is searched. </param>
        /// <param name="negated"> A value indicating if the item should be present in the values or absent. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        public InExpression(
            [NotNull] SqlExpression item,
            [NotNull] SqlExpression values,
            bool negated,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : this(Check.NotNull(item, nameof(item)), Check.NotNull(values, nameof(values)), null, negated, typeMapping)
        {
        }

        private InExpression(
            SqlExpression item,
            SqlExpression values,
            SelectExpression subquery,
            bool negated,
            RelationalTypeMapping typeMapping)
            : base(typeof(bool), typeMapping)
        {
            Item = item;
            Subquery = subquery;
            Values = values;
            IsNegated = negated;
        }

        /// <summary>
        ///     The item to look into values.
        /// </summary>
        public virtual SqlExpression Item { get; }

        /// <summary>
        ///     The value indicating if item should be present in the values or absent.
        /// </summary>
        public virtual bool IsNegated { get; }

        /// <summary>
        ///     The list of values to search item in.
        /// </summary>
        public virtual SqlExpression Values { get; }

        /// <summary>
        ///     The subquery to search item in.
        /// </summary>
        public virtual SelectExpression Subquery { get; }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var item = (SqlExpression)visitor.Visit(Item);
            var subquery = (SelectExpression)visitor.Visit(Subquery);
            var values = (SqlExpression)visitor.Visit(Values);

            return Update(item, values, subquery);
        }

        /// <summary>
        ///     Negates this expression by changing presence/absence state indicated by <see cref="IsNegated" />.
        /// </summary>
        /// <returns> An expression which is negated form of this expression. </returns>
        public virtual InExpression Negate()
            => new InExpression(Item, Values, Subquery, !IsNegated, TypeMapping);

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="item"> The <see cref="Item" /> property of the result. </param>
        /// <param name="values"> The <see cref="Values" /> property of the result. </param>
        /// <param name="subquery"> The <see cref="Subquery" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual InExpression Update(
            [NotNull] SqlExpression item,
            [CanBeNull] SqlExpression values,
            [CanBeNull] SelectExpression subquery)
        {
            Check.NotNull(item, nameof(item));

            if (values != null
                && subquery != null)
            {
                throw new ArgumentException(RelationalStrings.EitherOfTwoValuesMustBeNull(nameof(values), nameof(subquery)));
            }

            return item != Item || subquery != Subquery || values != Values
                ? new InExpression(item, values, subquery, IsNegated, TypeMapping)
                : this;
        }

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Visit(Item);
            expressionPrinter.Append(IsNegated ? " NOT IN " : " IN ");
            expressionPrinter.Append("(");

            if (Subquery != null)
            {
                using (expressionPrinter.Indent())
                {
                    expressionPrinter.Visit(Subquery);
                }
            }
            else if (Values is SqlConstantExpression constantValuesExpression
                && constantValuesExpression.Value is IEnumerable constantValues)
            {
                var first = true;
                foreach (var item in constantValues)
                {
                    if (!first)
                    {
                        expressionPrinter.Append(", ");
                    }

                    first = false;
                    expressionPrinter.Append(constantValuesExpression.TypeMapping?.GenerateSqlLiteral(item) ?? item?.ToString() ?? "NULL");
                }
            }
            else
            {
                expressionPrinter.Visit(Values);
            }

            expressionPrinter.Append(")");
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is InExpression inExpression
                    && Equals(inExpression));

        private bool Equals(InExpression inExpression)
            => base.Equals(inExpression)
                && Item.Equals(inExpression.Item)
                && IsNegated.Equals(inExpression.IsNegated)
                && (Values == null ? inExpression.Values == null : Values.Equals(inExpression.Values))
                && (Subquery == null ? inExpression.Subquery == null : Subquery.Equals(inExpression.Subquery));

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), Item, IsNegated, Values, Subquery);
    }
}
