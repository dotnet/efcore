// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class SelectExpressionTableAliasUniquifyingExpressionVisitor : ExpressionVisitor
    {
        private readonly ISet<string> _usedAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly ISet<TableExpressionBase> _visitedTableExpressionBases
            = new HashSet<TableExpressionBase>(ReferenceEqualityComparer.Instance);

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case TableExpressionBase tableExpressionBase
                    when !_visitedTableExpressionBases.Contains(tableExpressionBase)
                        && !string.IsNullOrEmpty(tableExpressionBase.Alias):
                    tableExpressionBase.Alias = GenerateUniqueAlias(tableExpressionBase.Alias);
                    _visitedTableExpressionBases.Add(tableExpressionBase);
                    return tableExpressionBase;

                default:
                    return base.VisitExtension(extensionExpression);
            }
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
