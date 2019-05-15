// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class SelectExpressionTableAliasUniquifyingExpressionVisitor : ExpressionVisitor
    {
        private readonly ISet<string> _usedAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case SelectExpression selectExpression:
                    foreach (var table in selectExpression.Tables)
                    {
                        Visit(table);
                    }
                    return selectExpression;

                case JoinExpressionBase joinExpressionBase:
                    Visit(joinExpressionBase.Table);
                    return joinExpressionBase;

                case TableExpressionBase tableExpressionBase
                    when !string.IsNullOrEmpty(tableExpressionBase.Alias):
                    tableExpressionBase.Alias = GenerateUniqueAlias(tableExpressionBase.Alias);
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
