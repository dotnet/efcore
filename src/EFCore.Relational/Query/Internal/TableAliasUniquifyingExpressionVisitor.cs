// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class TableAliasUniquifyingExpressionVisitor : ExpressionVisitor
    {
        private readonly ISet<string> _usedAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly ISet<TableExpressionBase> _visitedTableExpressionBases
            = new HashSet<TableExpressionBase>(ReferenceEqualityComparer.Instance);

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            var visitedExpression = base.VisitExtension(extensionExpression);
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
