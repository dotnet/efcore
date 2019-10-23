// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     Represents a SQL table valued function expression.
    /// </summary>
    public class TableValuedFunctionExpression : TableExpressionBase
    {
        /// <summary>
        ///     Initializes a new instance of the Microsoft.EntityFrameworkCore.Query.Expressions.TableValuedFunctionExpression class.
        /// </summary>
        /// <param name="function"> The function name. </param>
        /// <param name="parameters"> The parameters. </param>
        /// <param name="schema"> The schema name. </param>
        /// <param name="alias"> The alias. </param>
        /// <param name="querySource"> The query source. </param>
        public TableValuedFunctionExpression(
            [NotNull] string function,
            [NotNull] Expression[] parameters,
            [CanBeNull] string schema,
            [NotNull] string alias,
            [CanBeNull] IQuerySource querySource)
            : base(
                querySource,
                Check.NotEmpty(alias, nameof(alias)))
        {
            Check.NotEmpty(function , nameof(function));
            
            Function = function;
            Parameters = parameters;
            Schema = schema;
        }

        /// <summary>
        ///     Gets the function name.
        /// </summary>
        /// <value>
        ///     The function name.
        /// </value>
        public virtual string Function { get; }

        /// <summary>
        ///     Gets the parameters.
        /// </summary>
        /// <value>
        ///     The parameters.
        /// </value>
        public virtual Expression[] Parameters { get; }

        /// <summary>
        ///     Gets the schema name.
        /// </summary>
        /// <value>
        ///     The schema name.
        /// </value>
        public virtual string Schema { get; }

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return visitor is ISqlExpressionVisitor specificVisitor
                ? specificVisitor.VisitTableValuedFunction(this)
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

            return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((TableExpression)obj);
        }

        private bool Equals(TableValuedFunctionExpression other)
            => string.Equals(Function, other.Function)
               && Equals(Parameters, other.Parameters)
               && string.Equals(Schema, other.Schema)
               && string.Equals(Alias, other.Alias)
               && Equals(QuerySource, other.QuerySource);

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
                var hashCode = Alias?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (QuerySource?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Function.GetHashCode();
                hashCode = (hashCode * 397) ^ (Schema?.GetHashCode() ?? 0);

                return hashCode;
            }
        }

        /// <summary>
        ///     Creates a <see cref="string" /> representation of the Expression.
        /// </summary>
        /// <returns>A <see cref="string" /> representation of the Expression.</returns>
        public override string ToString() => Function + " " + Alias;
    }
}
