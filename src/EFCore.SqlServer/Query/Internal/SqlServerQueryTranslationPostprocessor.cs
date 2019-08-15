// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerQueryTranslationPostprocessor : RelationalQueryTranslationPostprocessor
    {
        public SqlServerQueryTranslationPostprocessor(
            QueryTranslationPostprocessorDependencies dependencies,
            RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
            QueryCompilationContext queryCompilationContext)
            : base(dependencies, relationalDependencies, queryCompilationContext)
        {
        }

        public override Expression Process(Expression query)
        {
            query = base.Process(query);
            query = new SearchConditionConvertingExpressionVisitor(SqlExpressionFactory).Visit(query);
            query = OptimizeSqlExpression(query);

            return query;
        }
    }
}
