// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerParameterBasedQueryTranslationPostprocessor : RelationalParameterBasedQueryTranslationPostprocessor
    {
        public SqlServerParameterBasedQueryTranslationPostprocessor(
            [NotNull] RelationalParameterBasedQueryTranslationPostprocessorDependencies dependencies,
            bool useRelationalNulls)
            : base(dependencies, useRelationalNulls)
        {
        }

        public override (SelectExpression, bool) Optimize(
            SelectExpression selectExpression,
            IReadOnlyDictionary<string, object> parametersValues)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));
            Check.NotNull(parametersValues, nameof(parametersValues));

            var (optimizedSelectExpression, canCache) = base.Optimize(selectExpression, parametersValues);

            var searchConditionOptimized = (SelectExpression)new SearchConditionConvertingExpressionVisitor(
                Dependencies.SqlExpressionFactory).Visit(optimizedSelectExpression);

            return (searchConditionOptimized, canCache);
        }
    }
}
