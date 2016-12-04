// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     Represents a SQL function call expression.
    /// </summary>
    [DebuggerDisplay("{this.FunctionName}({string.Join(\", \", this.Arguments)})")]
    public class SqlFunctionExpression : Expression
    {
        private readonly ReadOnlyCollection<Expression> _arguments;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlFunctionExpression" /> class.
        /// </summary>
        /// <param name="functionName"> Name of the function. </param>
        /// <param name="returnType"> The return type. </param>
        public SqlFunctionExpression(
            [NotNull] string functionName,
            [NotNull] Type returnType)
            : this(functionName, returnType, Enumerable.Empty<Expression>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlFunctionExpression" /> class.
        /// </summary>
        /// <param name="functionName"> Name of the function. </param>
        /// <param name="returnType"> The return type. </param>
        /// <param name="arguments"> The arguments. </param>
        public SqlFunctionExpression(
            [NotNull] string functionName,
            [NotNull] Type returnType,
            [NotNull] IEnumerable<Expression> arguments)
        {
            FunctionName = functionName;
            Type = returnType;

            _arguments = arguments.ToList().AsReadOnly();
        }

        /// <summary>
        ///     Gets the name of the function.
        /// </summary>
        /// <value>
        ///     The name of the function.
        /// </value>
        public virtual string FunctionName { get; }

        /// <summary>
        ///     The arguments.
        /// </summary>
        public virtual IReadOnlyList<Expression> Arguments => _arguments;

        /// <summary>
        ///     Returns the node type of this <see cref="Expression" />. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="ExpressionType" /> that represents this expression.</returns>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <summary>
        ///     Gets the static type of the expression that this <see cref="Expression" /> represents. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="Type" /> that represents the static type of the expression.</returns>
        public override Type Type { get; }

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitSqlFunction(this)
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
            var newArguments = visitor.VisitAndConvert(_arguments, "VisitChildren");

            return newArguments != _arguments
                ? new SqlFunctionExpression(FunctionName, Type, newArguments)
                : this;
        }
    }
}
