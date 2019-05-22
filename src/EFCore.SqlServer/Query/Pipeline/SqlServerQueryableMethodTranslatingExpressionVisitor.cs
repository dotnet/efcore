// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SqlServerQueryableMethodTranslatingExpressionVisitor : RelationalQueryableMethodTranslatingExpressionVisitor
    {
        public SqlServerQueryableMethodTranslatingExpressionVisitor(
            IModel model,
            IQueryableMethodTranslatingExpressionVisitorFactory queryableMethodTranslatingExpressionVisitorFactory,
            IRelationalSqlTranslatingExpressionVisitorFactory relationalSqlTranslatingExpressionVisitorFactory,
            ISqlExpressionFactory sqlExpressionFactory)
            : base(
                model,
                queryableMethodTranslatingExpressionVisitorFactory,
                relationalSqlTranslatingExpressionVisitorFactory,
                sqlExpressionFactory)
        {
        }

        protected override SqlExpression GenerateLongCountExpression()
            => SqlExpressionFactory.ApplyDefaultTypeMapping(
                SqlExpressionFactory.Function("COUNT_BIG", new[] { SqlExpressionFactory.Fragment("*") }, typeof(long)));
    }
}
