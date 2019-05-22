// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SqlServerQueryableMethodTranslatingExpressionVisitorFactory : IQueryableMethodTranslatingExpressionVisitorFactory
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IRelationalSqlTranslatingExpressionVisitorFactory _relationalSqlTranslatingExpressionVisitorFactory;

        public SqlServerQueryableMethodTranslatingExpressionVisitorFactory(
            IRelationalSqlTranslatingExpressionVisitorFactory relationalSqlTranslatingExpressionVisitorFactory,
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _relationalSqlTranslatingExpressionVisitorFactory = relationalSqlTranslatingExpressionVisitorFactory;
        }

        public QueryableMethodTranslatingExpressionVisitor Create(IModel model)
        {
            return new SqlServerQueryableMethodTranslatingExpressionVisitor(
                model,
                this,
                _relationalSqlTranslatingExpressionVisitorFactory,
                _sqlExpressionFactory);
        }
    }
}
