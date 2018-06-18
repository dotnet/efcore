// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     A column reference expression.
    /// </summary>
    public class ColumnReferenceExpression : Expression
    {
        private readonly Expression _expression;
        private readonly TableExpressionBase _tableExpression;

        /// <summary>
        ///     Creates a new instance of a ColumnReferenceExpression.
        /// </summary>
        /// <param name="aliasExpression"> The referenced AliasExpression. </param>
        /// <param name="tableExpression"> The target table expression. </param>
        public ColumnReferenceExpression(
            [NotNull] AliasExpression aliasExpression,
            [NotNull] TableExpressionBase tableExpression)
            : this(
                Check.NotNull(aliasExpression, nameof(aliasExpression)).Alias,
                aliasExpression,
                Check.NotNull(tableExpression, nameof(tableExpression)))
        {
        }

        /// <summary>
        ///     Creates a new instance of a ColumnReferenceExpression.
        /// </summary>
        /// <param name="columnExpression"> The referenced ColumnExpression. </param>
        /// <param name="tableExpression"> The target table expression. </param>
        public ColumnReferenceExpression(
            [NotNull] ColumnExpression columnExpression,
            [NotNull] TableExpressionBase tableExpression)
            : this(
                Check.NotNull(columnExpression, nameof(columnExpression)).Name,
                columnExpression,
                Check.NotNull(tableExpression, nameof(tableExpression)))
        {
        }

        /// <summary>
        ///     Creates a new instance of a ColumnReferenceExpression.
        /// </summary>
        /// <param name="columnReferenceExpression"> The referenced ColumnReferenceExpression. </param>
        /// <param name="tableExpression"> The target table expression. </param>
        public ColumnReferenceExpression(
            [NotNull] ColumnReferenceExpression columnReferenceExpression,
            [NotNull] TableExpressionBase tableExpression)
            : this(
                Check.NotNull(columnReferenceExpression, nameof(columnReferenceExpression)).Name,
                columnReferenceExpression,
                Check.NotNull(tableExpression, nameof(tableExpression)))
        {
        }

        private ColumnReferenceExpression(string name, Expression expression, TableExpressionBase tableExpression)
        {
            Name = name;
            _expression = expression;
            _tableExpression = tableExpression;
        }

        /// <summary>
        ///     The target table.
        /// </summary>
        public virtual TableExpressionBase Table => _tableExpression;

        /// <summary>
        ///     The referenced expression.
        /// </summary>
        public virtual Expression Expression => _expression;

        /// <summary>
        ///     Gets the column name.
        /// </summary>
        /// <value>
        ///     The column name.
        /// </value>
        public virtual string Name { get; }

        /// <summary>
        ///     Gets the static type of the expression that this <see cref="Expression" /> represents. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns> The <see cref="Type" /> that represents the static type of the expression. </returns>
        public override Type Type => _expression.Type;

        /// <summary>
        ///     Returns the node type of this <see cref="Expression" />. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns> The <see cref="ExpressionType" /> that represents this expression. </returns>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return visitor is ISqlExpressionVisitor specificVisitor
                ? specificVisitor.VisitColumnReference(this)
                : base.Accept(visitor);
        }

        /// <summary>
        ///     Reduces the node and then calls the <see cref="ExpressionVisitor.Visit(System.Linq.Expressions.Expression)" /> method passing the
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
        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

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

            return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((ColumnReferenceExpression)obj);
        }

        private bool Equals([NotNull] ColumnReferenceExpression other)
            => Equals(_expression, other._expression)
               && Equals(_tableExpression, other._tableExpression);

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
                var hashCode = _expression.GetHashCode();
                hashCode = (hashCode * 397) ^ _tableExpression.GetHashCode();

                return hashCode;
            }
        }

        /// <summary>
        ///     Creates a <see cref="string" /> representation of the Expression.
        /// </summary>
        /// <returns>A <see cref="string" /> representation of the Expression.</returns>
        public override string ToString() => _tableExpression.Alias + "." + Name;
    }
}
