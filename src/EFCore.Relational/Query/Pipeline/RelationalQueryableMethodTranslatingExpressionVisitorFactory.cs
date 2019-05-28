// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalQueryableMethodTranslatingExpressionVisitorFactory : IQueryableMethodTranslatingExpressionVisitorFactory
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IRelationalSqlTranslatingExpressionVisitorFactory _relationalSqlTranslatingExpressionVisitorFactory;

        public RelationalQueryableMethodTranslatingExpressionVisitorFactory(
            IRelationalSqlTranslatingExpressionVisitorFactory relationalSqlTranslatingExpressionVisitorFactory,
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _relationalSqlTranslatingExpressionVisitorFactory = relationalSqlTranslatingExpressionVisitorFactory;
        }

        public QueryableMethodTranslatingExpressionVisitor Create(IModel model)
        {
            return new RelationalQueryableMethodTranslatingExpressionVisitor(
                model,
                _relationalSqlTranslatingExpressionVisitorFactory,
                _sqlExpressionFactory);
        }
    }
}
