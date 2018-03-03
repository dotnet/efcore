// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


namespace Microsoft.EntityFrameworkCore.Query
{
    public class SqlTranslatingExpressionVisitorFactory : ISqlTranslatingExpressionVisitorFactory
    {
        public SqlTranslatingExpressionVisitor Create(
            DocumentDbQueryModelVisitor queryModelVisitor, SelectExpression selectExpression)
        {
            return new SqlTranslatingExpressionVisitor(queryModelVisitor, selectExpression);
        }
    }
}
