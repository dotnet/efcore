using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     An expression that represents passing an entity field collection (rather than an entity field) reference into a database function.  For example,
    ///     
    ///     <code>
    ///         var result = await context.Employees
    ///             .Where(c => EF.Functions.Contains(new { c.Name, c.City }, "Representative"))
    ///             .ToListAsync();
    ///     </code>
    /// </summary>
    public class PropertyListParameterExpression : Expression
    {
        /// <summary>
        ///     Create a new instance of PropertyListParameterExpression.
        /// </summary>
        /// <param name="properties">The collection of properties</param>
        public PropertyListParameterExpression(IEnumerable<Expression> properties)
        {
            Check.NotNull(properties, nameof(properties));
            Properties = properties
                .OfType<MemberExpression>()
                .ToArray();
        }

        /// <summary>
        ///     Get the properties.
        /// </summary>
        public virtual MemberExpression[] Properties { get; }

        /// <summary>
        ///     Returns the node type of this <see cref="Expression" />. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="ExpressionType" /> that represents this expression.</returns>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        ///     Gets the static type of the expression that this <see cref="Expression" /> represents. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="Type" /> that represents the static type of the expression.</returns>
        public override Type Type => typeof(object);

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
            foreach (var property in Properties)
            {
                visitor.Visit(property);
            }

            return this;
        }

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return visitor is ISqlExpressionVisitor specificVisitor
                ? specificVisitor.VisitPropertyListParameter(this)
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

            return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((PropertyListParameterExpression)obj);
        }

        private bool Equals(PropertyListParameterExpression other)
            => Equals(Properties, other.Properties);

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
                return Properties.GetHashCode();
            }
        }
    }
}
