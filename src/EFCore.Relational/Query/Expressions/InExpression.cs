// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     Represents a SQL IN expression.
    /// </summary>
    public class InExpression : Expression
    {
        /// <summary>
        ///     Creates a new instance of InExpression.
        /// </summary>
        /// <param name="operand"> The operand. </param>
        /// <param name="values"> The values. </param>
        public InExpression(
            [NotNull] Expression operand,
            [NotNull] IReadOnlyList<Expression> values)
        {
            Check.NotNull(operand, nameof(operand));
            Check.NotNull(values, nameof(values));

            Operand = operand;
            Values = values;
        }

        /// <summary>
        ///     Creates a new instance of InExpression.
        /// </summary>
        /// <param name="operand"> The operand. </param>
        /// <param name="subQuery"> The sub query. </param>
        public InExpression(
            [NotNull] Expression operand,
            [NotNull] SelectExpression subQuery)
        {
            Check.NotNull(operand, nameof(operand));
            Check.NotNull(subQuery, nameof(subQuery));

            Operand = operand;
            SubQuery = subQuery;
        }

        private InExpression(
            Expression operand,
            IReadOnlyList<Expression> values,
            SelectExpression subQuery)
        {
            Operand = operand;
            Values = values;
            SubQuery = subQuery;
        }

        /// <summary>
        ///     Gets the operand.
        /// </summary>
        /// <value>
        ///     The operand.
        /// </value>
        public virtual Expression Operand { get; }

        /// <summary>
        ///     Gets the values.
        /// </summary>
        /// <value>
        ///     The values.
        /// </value>
        public virtual IReadOnlyList<Expression> Values { get; }

        /// <summary>
        ///     Gets the sub query.
        /// </summary>
        /// <value>
        ///     The sub query.
        /// </value>
        public virtual SelectExpression SubQuery { get; }

        /// <summary>
        ///     Returns the node type of this <see cref="Expression" />. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="ExpressionType" /> that represents this expression.</returns>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        ///     Gets the static type of the expression that this <see cref="Expression" /> represents. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="Type" /> that represents the static type of the expression.</returns>
        public override Type Type => typeof(bool);

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return visitor is ISqlExpressionVisitor specificVisitor
                ? specificVisitor.VisitIn(this)
                : base.Accept(visitor);
        }

        /// <summary>
        ///     Reduces the node and then calls the <see cref="ExpressionVisitor.Visit(Expression)" /> method passing the
        ///     reduced expression.
        ///     Throws an exception if the node isn't reducible.
        /// </summary>
        /// <param name="visitor"> An instance of <see cref="ExpressionVisitor" />. </param>
        /// <returns> The expression being visited, or an expression which should replace it in the tree. </returns>
        /// <remarks>
        ///     Override this method to provide logic to walk the node's children.
        ///     A typical implementation will call visitor.Visit on each of its
        ///     children, and if any of them change, should return a new copy of
        ///     itself with the modified children.
        /// </remarks>
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newOperand = visitor.Visit(Operand);
            var newSubQuery = (SelectExpression)visitor.Visit(SubQuery);

            var valuesChanged = false;
            var newValues = new List<Expression>();
            if (Values != null)
            {
                foreach (var value in Values)
                {
                    var newValue = visitor.Visit(value);
                    if (newValue is BlockExpression
                        && value is ListInitExpression)
                    {
                        newValues.Add(value);
                    }
                    else
                    {
                        newValues.Add(newValue);
                        valuesChanged |= newValue != value;
                    }
                }
            }

            return valuesChanged || newOperand != Operand || newSubQuery != SubQuery
                ? new InExpression(newOperand, Values == null ? null : newValues.AsReadOnly(), newSubQuery)
                : this;
        }

        /// <summary>
        ///     Tests if this object is considered equal to another.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns>
        ///     true if the objects are considered equal, false if they are not.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((InExpression)obj);
        }

        private bool Equals(InExpression other)
            => Operand.Equals(other.Operand)
               && (Values == null
                   ? other.Values == null
                    : ExpressionEqualityComparer.Instance.SequenceEquals(Values, other.Values))
               && (SubQuery == null
                   ? other.SubQuery == null
                   : SubQuery.Equals(other.SubQuery));

        /// <summary>
        ///     Returns a hash code for this object.
        /// </summary>
        /// <returns>
        ///     A hash code for this object.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Operand.GetHashCode();
                hashCode = (hashCode * 397) ^ (Values != null
                               ? Values.Aggregate(0, (current, value) => current + ((current * 397) ^ value.GetHashCode()))
                               : 0);
                hashCode = (hashCode * 397) ^ (SubQuery?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        /// <summary>
        ///     Creates a <see cref="string" /> representation of the Expression.
        /// </summary>
        /// <returns>A <see cref="string" /> representation of the Expression.</returns>
        public override string ToString()
            => Operand + " IN (" + (Values != null ? string.Join(", ", Values) : SubQuery.ToString()) + ")";
    }
}
