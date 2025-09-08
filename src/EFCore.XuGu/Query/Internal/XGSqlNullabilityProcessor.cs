// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.XuGu.Query.Expressions.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    /// <inheritdoc />
    public class XGSqlNullabilityProcessor : SqlNullabilityProcessor
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        /// Creates a new instance of the <see cref="XGSqlNullabilityProcessor" /> class.
        /// </summary>
        /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
        /// <param name="parameters">Parameter object containing parameters for this class.</param>
        public XGSqlNullabilityProcessor(
            [NotNull] RelationalParameterBasedSqlProcessorDependencies dependencies,
            RelationalParameterBasedSqlProcessorParameters parameters)
            : base(dependencies, parameters)
            => _sqlExpressionFactory = dependencies.SqlExpressionFactory;

        /// <inheritdoc />
        protected override SqlExpression VisitCustomSqlExpression(
            SqlExpression sqlExpression, bool allowOptimizedExpansion, out bool nullable)
            => sqlExpression switch
            {
                XGBinaryExpression binaryExpression => VisitBinary(binaryExpression, allowOptimizedExpansion, out nullable),
                XGCollateExpression collateExpression => VisitCollate(collateExpression, allowOptimizedExpansion, out nullable),
                XGComplexFunctionArgumentExpression complexFunctionArgumentExpression => VisitComplexFunctionArgument(complexFunctionArgumentExpression, allowOptimizedExpansion, out nullable),
                XGMatchExpression matchExpression => VisitMatch(matchExpression, allowOptimizedExpansion, out nullable),
                XGJsonArrayIndexExpression arrayIndexExpression => VisitJsonArrayIndex(arrayIndexExpression, allowOptimizedExpansion, out nullable),
                XGJsonTraversalExpression jsonTraversalExpression => VisitJsonTraversal(jsonTraversalExpression, allowOptimizedExpansion, out nullable),
                XGRegexpExpression regexpExpression => VisitRegexp(regexpExpression, allowOptimizedExpansion, out nullable),
                XGColumnAliasReferenceExpression columnAliasReferenceExpression => VisitColumnAliasReference(columnAliasReferenceExpression, allowOptimizedExpansion, out nullable),
                _ => base.VisitCustomSqlExpression(sqlExpression, allowOptimizedExpansion, out nullable)
            };

        private SqlExpression VisitColumnAliasReference(XGColumnAliasReferenceExpression columnAliasReferenceExpression, bool allowOptimizedExpansion, out bool nullable)
        {
            Check.NotNull(columnAliasReferenceExpression, nameof(columnAliasReferenceExpression));

            var expression = Visit(columnAliasReferenceExpression.Expression, allowOptimizedExpansion, out nullable);

            return columnAliasReferenceExpression.Update(columnAliasReferenceExpression.Alias, expression);
        }

        /// <summary>
        /// Visits a <see cref="XGBinaryExpression" /> and computes its nullability.
        /// </summary>
        /// <param name="binaryExpression">A <see cref="XGBinaryExpression" /> expression to visit.</param>
        /// <param name="allowOptimizedExpansion">A bool value indicating if optimized expansion which considers null value as false value is allowed.</param>
        /// <param name="nullable">A bool value indicating whether the sql expression is nullable.</param>
        /// <returns>An optimized sql expression.</returns>
        protected virtual SqlExpression VisitBinary(
            [NotNull] XGBinaryExpression binaryExpression, bool allowOptimizedExpansion, out bool nullable)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            var left = Visit(binaryExpression.Left, allowOptimizedExpansion, out var leftNullable);
            var right = Visit(binaryExpression.Right, allowOptimizedExpansion, out var rightNullable);

            nullable = leftNullable || rightNullable;

            return binaryExpression.Update(left, right);
        }

        /// <summary>
        /// Visits a <see cref="XGCollateExpression" /> and computes its nullability.
        /// </summary>
        /// <param name="collateExpression">A <see cref="XGCollateExpression" /> expression to visit.</param>
        /// <param name="allowOptimizedExpansion">A bool value indicating if optimized expansion which considers null value as false value is allowed.</param>
        /// <param name="nullable">A bool value indicating whether the sql expression is nullable.</param>
        /// <returns>An optimized sql expression.</returns>
        protected virtual SqlExpression VisitCollate(
            [NotNull] XGCollateExpression collateExpression, bool allowOptimizedExpansion, out bool nullable)
        {
            Check.NotNull(collateExpression, nameof(collateExpression));

            var valueExpression = Visit(collateExpression.ValueExpression, allowOptimizedExpansion, out nullable);

            return collateExpression.Update(valueExpression);
        }

        /// <summary>
        /// Visits a <see cref="XGComplexFunctionArgumentExpression" /> and computes its nullability.
        /// </summary>
        /// <param name="complexFunctionArgumentExpression">A <see cref="XGComplexFunctionArgumentExpression" /> expression to visit.</param>
        /// <param name="allowOptimizedExpansion">A bool value indicating if optimized expansion which considers null value as false value is allowed.</param>
        /// <param name="nullable">A bool value indicating whether the sql expression is nullable.</param>
        /// <returns>An optimized sql expression.</returns>
        protected virtual SqlExpression VisitComplexFunctionArgument(
            [NotNull] XGComplexFunctionArgumentExpression complexFunctionArgumentExpression, bool allowOptimizedExpansion, out bool nullable)
        {
            Check.NotNull(complexFunctionArgumentExpression, nameof(complexFunctionArgumentExpression));

            nullable = false;

            var argumentParts = new SqlExpression[complexFunctionArgumentExpression.ArgumentParts.Count];

            for (var i = 0; i < argumentParts.Length; i++)
            {
                argumentParts[i] = Visit(complexFunctionArgumentExpression.ArgumentParts[i], allowOptimizedExpansion, out var argumentPartNullable);
                nullable |= argumentPartNullable;
            }

            return complexFunctionArgumentExpression.Update(argumentParts, complexFunctionArgumentExpression.Delimiter);
        }

        /// <summary>
        /// Visits a <see cref="XGMatchExpression" /> and computes its nullability.
        /// </summary>
        /// <param name="matchExpression">A <see cref="XGMatchExpression" /> expression to visit.</param>
        /// <param name="allowOptimizedExpansion">A bool value indicating if optimized expansion which considers null value as false value is allowed.</param>
        /// <param name="nullable">A bool value indicating whether the sql expression is nullable.</param>
        /// <returns>An optimized sql expression.</returns>
        protected virtual SqlExpression VisitMatch(
            [NotNull] XGMatchExpression matchExpression, bool allowOptimizedExpansion, out bool nullable)
        {
            Check.NotNull(matchExpression, nameof(matchExpression));

            var match = Visit(matchExpression.Match, allowOptimizedExpansion, out var matchNullable);
            var pattern = Visit(matchExpression.Against, allowOptimizedExpansion, out var patternNullable);

            nullable = matchNullable || patternNullable;

            return matchExpression.Update(match, pattern);
        }

        /// <summary>
        /// Visits an <see cref="XGJsonArrayIndexExpression" /> and computes its nullability.
        /// </summary>
        /// <param name="jsonArrayIndexExpression">A <see cref="XGJsonArrayIndexExpression" /> expression to visit.</param>
        /// <param name="allowOptimizedExpansion">A bool value indicating if optimized expansion which considers null value as false value is allowed.</param>
        /// <param name="nullable">A bool value indicating whether the sql expression is nullable.</param>
        /// <returns>An optimized sql expression.</returns>
        protected virtual SqlExpression VisitJsonArrayIndex(
            [NotNull] XGJsonArrayIndexExpression jsonArrayIndexExpression, bool allowOptimizedExpansion, out bool nullable)
        {
            Check.NotNull(jsonArrayIndexExpression, nameof(jsonArrayIndexExpression));

            var index = Visit(jsonArrayIndexExpression.Expression, allowOptimizedExpansion, out nullable);

            return jsonArrayIndexExpression.Update(index);
        }

        /// <summary>
        /// Visits a <see cref="XGJsonTraversalExpression" /> and computes its nullability.
        /// </summary>
        /// <param name="jsonTraversalExpression">A <see cref="XGJsonTraversalExpression" /> expression to visit.</param>
        /// <param name="allowOptimizedExpansion">A bool value indicating if optimized expansion which considers null value as false value is allowed.</param>
        /// <param name="nullable">A bool value indicating whether the sql expression is nullable.</param>
        /// <returns>An optimized sql expression.</returns>
        protected virtual SqlExpression VisitJsonTraversal(
            [NotNull] XGJsonTraversalExpression jsonTraversalExpression, bool allowOptimizedExpansion, out bool nullable)
        {
            Check.NotNull(jsonTraversalExpression, nameof(jsonTraversalExpression));

            var expression = Visit(jsonTraversalExpression.Expression, out nullable);

            List<SqlExpression> newPath = null;
            for (var i = 0; i < jsonTraversalExpression.Path.Count; i++)
            {
                var pathComponent = jsonTraversalExpression.Path[i];
                var newPathComponent = Visit(pathComponent, allowOptimizedExpansion, out var nullablePathComponent);
                nullable |= nullablePathComponent;
                if (newPathComponent != pathComponent && newPath is null)
                {
                    newPath = new List<SqlExpression>();
                    for (var j = 0; j < i; j++)
                    {
                        newPath.Add(newPathComponent);
                    }
                }

                newPath?.Add(newPathComponent);
            }

            nullable = false;

            return jsonTraversalExpression.Update(
                expression,
                newPath is null
                    ? jsonTraversalExpression.Path
                    : newPath.ToArray());
        }

        /// <summary>
        /// Visits a <see cref="XGRegexpExpression" /> and computes its nullability.
        /// </summary>
        /// <param name="regexpExpression">A <see cref="XGRegexpExpression" /> expression to visit.</param>
        /// <param name="allowOptimizedExpansion">A bool value indicating if optimized expansion which considers null value as false value is allowed.</param>
        /// <param name="nullable">A bool value indicating whether the sql expression is nullable.</param>
        /// <returns>An optimized sql expression.</returns>
        protected virtual SqlExpression VisitRegexp(
            [NotNull] XGRegexpExpression regexpExpression, bool allowOptimizedExpansion, out bool nullable)
        {
            Check.NotNull(regexpExpression, nameof(regexpExpression));

            var match = Visit(regexpExpression.Match, out var matchNullable);
            var pattern = Visit(regexpExpression.Pattern, out var patternNullable);

            nullable = matchNullable || patternNullable;

            return regexpExpression.Update(match, pattern);
        }
    }
}
