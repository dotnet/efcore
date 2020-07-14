// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class TableAliasUniquifyingExpressionVisitor : ExpressionVisitor
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Expression Visit(Expression expression)
        {
            switch (expression)
            {
                case ShapedQueryExpression shapedQueryExpression:
                    return shapedQueryExpression.Update(
                        UniquifyAliasInSelectExpression((SelectExpression)shapedQueryExpression.QueryExpression),
                        Visit(shapedQueryExpression.ShaperExpression));

                case RelationalSplitCollectionShaperExpression relationalSplitCollectionShaperExpression:
                    return relationalSplitCollectionShaperExpression.Update(
                        relationalSplitCollectionShaperExpression.ParentIdentifier,
                        relationalSplitCollectionShaperExpression.ChildIdentifier,
                        UniquifyAliasInSelectExpression(relationalSplitCollectionShaperExpression.SelectExpression),
                        Visit(relationalSplitCollectionShaperExpression.InnerShaper));

                default:
                    return base.Visit(expression);
            }
        }

        private SelectExpression UniquifyAliasInSelectExpression(SelectExpression selectExpression)
            => (SelectExpression)new ScopedVisitor().Visit(selectExpression);

        private sealed class ScopedVisitor : ExpressionVisitor
        {
            private readonly ISet<string> _usedAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            private readonly ISet<TableExpressionBase> _visitedTableExpressionBases
                = new HashSet<TableExpressionBase>(LegacyReferenceEqualityComparer.Instance);

            public override Expression Visit(Expression expression)
            {
                var visitedExpression = base.Visit(expression);
                if (visitedExpression is TableExpressionBase tableExpressionBase
                    && !_visitedTableExpressionBases.Contains(tableExpressionBase)
                    && tableExpressionBase.Alias != null)
                {
                    tableExpressionBase.Alias = GenerateUniqueAlias(tableExpressionBase.Alias);
                    _visitedTableExpressionBases.Add(tableExpressionBase);
                }

                return visitedExpression;
            }

            private string GenerateUniqueAlias(string currentAlias)
            {
                if (!_usedAliases.Contains(currentAlias))
                {
                    _usedAliases.Add(currentAlias);
                    return currentAlias;
                }

                var counter = 0;
                var uniqueAlias = currentAlias;

                while (_usedAliases.Contains(uniqueAlias))
                {
                    uniqueAlias = currentAlias + counter++;
                }

                _usedAliases.Add(uniqueAlias);

                return uniqueAlias;
            }
        }
    }
}
