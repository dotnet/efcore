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
    ///     An expression that represents accessing a property on a query parameter.
    /// </summary>
    public class PropertyParameterExpression : Expression
    {
        /// <summary>
        ///     Creates a new instance of a PropertyParameterExpression.
        /// </summary>
        /// <param name="name"> The parameter name. </param>
        /// <param name="property"> The property to access. </param>
        public PropertyParameterExpression([NotNull] string name, [NotNull] IProperty property)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(property, nameof(property));

            Name = name;
            Property = property;
        }

        /// <summary>
        ///     Gets the parameter name.
        /// </summary>
        /// <value>
        ///     The parameter name.
        /// </value>
        public virtual string Name { get; }

#pragma warning disable 108

        /// <summary>
        ///     Gets the property.
        /// </summary>
        /// <value>
        ///     The property.
        /// </value>
        public virtual IProperty Property { get; }
#pragma warning restore 108

        /// <summary>
        ///     Name of the property parameter when used in DbCommands.
        /// </summary>
        public virtual string PropertyParameterName => $"{Name}_{Property.Name}"; // Interpolation okay; strings.

        /// <summary>
        ///     Returns the node type of this <see cref="Expression" />. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="ExpressionType" /> that represents this expression.</returns>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        ///     Gets the static type of the expression that this <see cref="Expression" /> represents. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="Type" /> that represents the static type of the expression.</returns>
        public override Type Type => Property.ClrType;

        /// <summary>
        ///     Creates a <see cref="string" /> representation of the Expression.
        /// </summary>
        /// <returns>A <see cref="string" /> representation of the Expression.</returns>
        public override string ToString() => Name;

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

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return visitor is ISqlExpressionVisitor specificVisitor
                ? specificVisitor.VisitPropertyParameter(this)
                : base.Accept(visitor);
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

            return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((PropertyParameterExpression)obj);
        }

        private bool Equals(PropertyParameterExpression other)
            => string.Equals(Name, other.Name) && Equals(Property, other.Property);

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
                return (Name.GetHashCode() * 397) ^ Property.GetHashCode();
            }
        }
    }
}
