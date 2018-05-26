using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     Represents a SQL Table Valued Fuction in the sql generation tree.
    /// </summary>
    public class QuerableSqlFunctionExpression : TableExpressionBase
    {
        private readonly SqlFunctionExpression _sqlFunctionExpression;

        /// <summary>
        /// The sql function expression representing the database function
        /// </summary>
        public virtual SqlFunctionExpression SqlFunctionExpression => _sqlFunctionExpression;

        /// <summary>
        /// Creates a new instance of a QuerableSqlFunctionExpression.
        /// </summary>
        /// <param name="sqlFunction"> The sqlFunctionExprssion representing the database function. </param>
        /// <param name="querySource">  The query source. </param>
        /// <param name="alias"> The alias. </param>
        public QuerableSqlFunctionExpression([NotNull] SqlFunctionExpression sqlFunction, [NotNull] IQuerySource querySource, [CanBeNull] string alias)
             : this(sqlFunction.FunctionName, sqlFunction.Type, sqlFunction.Schema, sqlFunction.Arguments, querySource, alias)
        {
        }

        /// <summary>
        /// Creates a new instance of a QuerableSqlFunctionExpression.
        /// </summary>
        /// <param name="functionName"> The db function name. </param>
        /// <param name="returnType"> The db function return type. </param>
        /// <param name="schema"> The schema. </param>
        /// <param name="arguments"> The arguemnts to the db function. </param>
        /// <param name="querySource"> The query source. </param>
        /// <param name="alias"> The alias. </param>
        public QuerableSqlFunctionExpression([NotNull] string functionName,
                [NotNull] Type returnType,
                [CanBeNull] string schema,
                [NotNull] IEnumerable<Expression> arguments,
                [NotNull] IQuerySource querySource,
                [CanBeNull] string alias)
            : base(Check.NotNull(querySource, nameof(querySource)), alias)
        {
            Check.NotNull(functionName, nameof(functionName));
            Check.NotNull(returnType, nameof(returnType));
            Check.NotNull(arguments, nameof(arguments));

            _sqlFunctionExpression = new SqlFunctionExpression(functionName, returnType, schema, arguments);
        }

        /// <summary>
        ///   Convert this object into a string representation.
        /// </summary>
        /// <returns> A string that represents this object. </returns>
        public override string ToString()
        {
            return _sqlFunctionExpression.ToString();
        }

        /// <summary>
        /// Dispatches to the specific visit method for this node type.
        /// </summary>
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return visitor is ISqlExpressionVisitor specificVisitor
                ? specificVisitor.VisitQueryableSqlFunctionExpression(this)
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
            Check.NotNull(visitor, nameof(visitor));

            var newArguments = visitor.Visit(new ReadOnlyCollection<Expression>(_sqlFunctionExpression.Arguments.ToList()));

            return !Equals(newArguments, _sqlFunctionExpression.Arguments)
                ? new QuerableSqlFunctionExpression(new SqlFunctionExpression(_sqlFunctionExpression.FunctionName, Type, _sqlFunctionExpression.Schema, newArguments), QuerySource, Alias)
                : this;
        }
    }
}
