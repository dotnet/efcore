// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         An expression that combines a query expression and shaper expression.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class ShapedQueryExpression : Expression, IPrintableExpression
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="ShapedQueryExpression" /> class with associated query provider.
        /// </summary>
        /// <param name="queryExpression"> The query expression to get results from server. </param>
        /// <param name="shaperExpression"> The shaper expression to create result objects from server results. </param>
        public ShapedQueryExpression(Expression queryExpression, Expression shaperExpression)
            : this(
                Check.NotNull(queryExpression, nameof(queryExpression)),
                Check.NotNull(shaperExpression, nameof(shaperExpression)),
                ResultCardinality.Enumerable)
        {
        }

        private ShapedQueryExpression(
            Expression queryExpression,
            Expression shaperExpression,
            ResultCardinality resultCardinality)
        {
            QueryExpression = queryExpression;
            ShaperExpression = shaperExpression;
            ResultCardinality = resultCardinality;
        }

        /// <summary>
        ///     An expression representing the query to be run against server to retrieve the data.
        /// </summary>
        public virtual Expression QueryExpression { get; }

        /// <summary>
        ///     The cardinality of the results generated.
        /// </summary>
        public virtual ResultCardinality ResultCardinality { get; }

        /// <summary>
        ///     An expression representing the shaper to be run on the results fetched from the server.
        /// </summary>
        public virtual Expression ShaperExpression { get; }

        /// <inheritdoc />
        public override Type Type
            => ResultCardinality == ResultCardinality.Enumerable
                ? typeof(IQueryable<>).MakeGenericType(ShaperExpression.Type)
                : ShaperExpression.Type;

        /// <inheritdoc />
        public sealed override ExpressionType NodeType
            => ExpressionType.Extension;

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => throw new InvalidOperationException(
                CoreStrings.VisitIsNotAllowed($"{nameof(ShapedQueryExpression)}.{nameof(VisitChildren)}"));

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="queryExpression"> The <see cref="QueryExpression" /> property of the result. </param>
        /// <param name="shaperExpression"> The <see cref="ShaperExpression" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual ShapedQueryExpression Update(Expression queryExpression, Expression shaperExpression)
        {
            Check.NotNull(queryExpression, nameof(queryExpression));
            Check.NotNull(shaperExpression, nameof(shaperExpression));

            return queryExpression != QueryExpression || shaperExpression != ShaperExpression
                ? new ShapedQueryExpression(queryExpression, shaperExpression, ResultCardinality)
                : this;
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied shaper expression. If shaper expression is the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="shaperExpression"> The <see cref="ShaperExpression" /> property of the result. </param>
        /// <returns> This expression if shaper expression did not change, or an expression with the updated shaper expression. </returns>
        public virtual ShapedQueryExpression UpdateShaperExpression(Expression shaperExpression)
        {
            Check.NotNull(shaperExpression, nameof(shaperExpression));

            return shaperExpression != ShaperExpression
                ? new ShapedQueryExpression(QueryExpression, shaperExpression, ResultCardinality)
                : this;
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but with supplied result cardinality.
        /// </summary>
        /// <param name="resultCardinality"> The <see cref="ResultCardinality" /> property of the result. </param>
        /// <returns> An expression with the updated result cardinality. </returns>
        public virtual ShapedQueryExpression UpdateResultCardinality(ResultCardinality resultCardinality)
            => new(QueryExpression, ShaperExpression, resultCardinality);

        /// <inheritdoc />
        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.AppendLine(nameof(ShapedQueryExpression) + ": ");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.AppendLine(nameof(QueryExpression) + ": ");
                using (expressionPrinter.Indent())
                {
                    expressionPrinter.Visit(QueryExpression);
                }

                expressionPrinter.AppendLine().Append(nameof(ShaperExpression) + ": ");
                using (expressionPrinter.Indent())
                {
                    expressionPrinter.Visit(ShaperExpression);
                }

                expressionPrinter.AppendLine();
            }
        }
    }
}
