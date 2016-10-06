// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     A column expression.
    /// </summary>
    public class ColumnExpression : Expression
    {
        private readonly IProperty _property;
        private readonly TableExpressionBase _tableExpression;

        /// <summary>
        ///     Creates a new instance of a ColumnExpression.
        /// </summary>
        /// <param name="name"> The column name. </param>
        /// <param name="property"> The corresponding property. </param>
        /// <param name="tableExpression"> The target table expression. </param>
        public ColumnExpression(
            [NotNull] string name,
            [NotNull] IProperty property,
            [NotNull] TableExpressionBase tableExpression)
            : this(name, Check.NotNull(property, nameof(property)).ClrType, tableExpression)
        {
            _property = property;
            IsNullable = _property.IsNullable;
        }

        /// <summary>
        ///     Creates a new instance of a ColumnExpression.
        /// </summary>
        /// <param name="name"> The column name. </param>
        /// <param name="type"> The column type. </param>
        /// <param name="tableExpression"> The target table expression. </param>
        public ColumnExpression(
            [NotNull] string name,
            [NotNull] Type type,
            [NotNull] TableExpressionBase tableExpression)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(type, nameof(type));
            Check.NotNull(tableExpression, nameof(tableExpression));

            Name = name;
            Type = type;
            _tableExpression = tableExpression;
        }

        /// <summary>
        ///     The target table.
        /// </summary>
        public virtual TableExpressionBase Table => _tableExpression;

        /// <summary>
        ///     The target table alias.
        /// </summary>
        public virtual string TableAlias => _tableExpression.Alias;

#pragma warning disable 108

        /// <summary>
        ///     The corresponding property.
        /// </summary>
        public virtual IProperty Property => _property;
#pragma warning restore 108

        /// <summary>
        ///     Gets the column name.
        /// </summary>
        /// <value>
        ///     The column name.
        /// </value>
        public virtual string Name { get; }

        /// <summary>
        ///     Returns the node type of this <see cref="Expression" />. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns> The <see cref="ExpressionType" /> that represents this expression. </returns>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        ///     Gets the static type of the expression that this <see cref="Expression" /> represents. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns> The <see cref="Type" /> that represents the static type of the expression. </returns>
        public override Type Type { get; }

        /// <summary>
        ///     Gets a value indicating whether this column expression can contain null.
        /// </summary>
        public virtual bool IsNullable { get; set; }

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitColumn(this)
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
        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        private bool Equals([NotNull] ColumnExpression other)
            => ((_property == null && other._property == null && Name == other.Name)
                || (_property != null && _property.Equals(other._property)))
               && Type == other.Type
               && _tableExpression.Equals(other._tableExpression);

        /// <summary>
        ///     Tests if this object is considered equal to another.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns>
        ///     true if the objects are considered equal, false if they are not.
        /// </returns>
        public override bool Equals([CanBeNull] object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return (obj.GetType() == GetType())
                   && Equals((ColumnExpression)obj);
        }

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
                return (_property.GetHashCode() * 397)
                       ^ _tableExpression.GetHashCode();
            }
        }

        /// <summary>
        ///     Creates a <see cref="String" /> representation of the Expression.
        /// </summary>
        /// <returns>A <see cref="String" /> representation of the Expression.</returns>
        public override string ToString() => _tableExpression.Alias + "." + Name;
    }
}
