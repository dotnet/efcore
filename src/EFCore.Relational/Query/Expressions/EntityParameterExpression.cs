using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     An expression that represents passing an entity (rather than an entity field) reference into a database function.  For example,
    ///     
    ///     <code>
    ///         var result = await context.Employees
    ///             .Where(c => EF.Functions.Contains(c, "Representative"))
    ///             .ToListAsync();
    ///     </code>
    ///
    ///     versus
    ///     
    ///     <code>
    ///         var result = await context.Employees
    ///             .Where(c => EF.Functions.Contains(c.Name, "Representative"))
    ///             .ToListAsync();
    ///     </code>
    /// </summary>
    public class EntityParameterExpression : Expression
    {
        /// <summary>
        ///     Create a new instance of an EntityParameterExpression.
        /// </summary>
        /// <param name="name">Name of the table (alias)</param>
        /// <param name="entityType">The type of the entity</param>
        public EntityParameterExpression([NotNull] string name, [NotNull] Type entityType)
        {
            Name = Check.NotNull(name, nameof(name));
            EntityType = Check.NotNull(entityType, nameof(entityType));
        }

        /// <summary>
        ///     Get the property Name
        /// </summary>
        public virtual string Name { get; }

        /// <summary>
        ///     Get the property EntityType
        /// </summary>
        public virtual Type EntityType { get; }

        /// <summary>
        ///     Returns the node type of this <see cref="Expression" />. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="ExpressionType" /> that represents this expression.</returns>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        ///     Gets the static type of the expression that this <see cref="Expression" /> represents. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="Type" /> that represents the static type of the expression.</returns>
        public override Type Type => EntityType;

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
                ? specificVisitor.VisitEntityParameter(this)
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

            return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((EntityParameterExpression)obj);
        }

        private bool Equals(EntityParameterExpression other)
            => string.Equals(Name, other.Name) && Equals(EntityType, other.EntityType);

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
                return (Name.GetHashCode() * 397) ^ EntityType.GetHashCode();
            }
        }
    }
}
