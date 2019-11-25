// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerParameterBasedQueryTranslationPostprocessor : RelationalParameterBasedQueryTranslationPostprocessor
    {
        public SqlServerParameterBasedQueryTranslationPostprocessor(
            RelationalParameterBasedQueryTranslationPostprocessorDependencies dependencies,
            bool useRelationalNulls)
            : base(dependencies, useRelationalNulls)
        {
        }

        public override (SelectExpression selectExpression, bool canCache) Optimize(
            SelectExpression selectExpression,
            IReadOnlyDictionary<string, object> parametersValues)
        {
            var (optimizedSelectExpression, canCache) = base.Optimize(selectExpression, parametersValues);

            var searchConditionOptimized = (SelectExpression)new SearchConditionConvertingExpressionVisitor(Dependencies.SqlExpressionFactory)
                .Visit(optimizedSelectExpression);

            return (searchConditionOptimized, canCache);
        }
    }
}
