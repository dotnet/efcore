// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class CaseWhenFlatteningExpressionVisitor : ExpressionVisitor
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public CaseWhenFlatteningExpressionVisitor(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        protected override Expression VisitExtension(Expression node)
        {
            // Only applies to 'CASE WHEN condition...' not 'CASE operand WHEN...'
            if (node is CaseExpression caseExpr && caseExpr.Operand == null)
            {
                if (caseExpr.ElseResult is CaseExpression nestedCaseExpr && nestedCaseExpr.Operand == null)
                {
                    return VisitExtension(_sqlExpressionFactory.Case(
                        caseExpr.WhenClauses.Union(nestedCaseExpr.WhenClauses).ToList(),
                        nestedCaseExpr.ElseResult));
                }
            }
            return base.VisitExtension(node);
        }

    }
}
