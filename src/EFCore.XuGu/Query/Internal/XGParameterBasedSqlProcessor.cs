// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    public class XGParametersBasedSqlProcessor : RelationalParameterBasedSqlProcessor
    {
        private readonly IXGOptions _options;

        public XGParametersBasedSqlProcessor(
            RelationalParameterBasedSqlProcessorDependencies dependencies,
            RelationalParameterBasedSqlProcessorParameters parameters,
            IXGOptions options)
            : base(dependencies, parameters)
        {
            _options = options;
        }

        public override Expression Optimize(
            Expression queryExpression,
            IReadOnlyDictionary<string, object?> parametersValues,
            out bool canCache)
        {
            queryExpression = base.Optimize(queryExpression, parametersValues, out canCache);

            if (_options.ServerVersion.Supports.XGBugLimit0Offset0ExistsWorkaround)
            {
                queryExpression = new SkipTakeCollapsingExpressionVisitor(Dependencies.SqlExpressionFactory)
                    .Process(queryExpression, parametersValues, out var canCache2);

                canCache &= canCache2;
            }

            if (_options.IndexOptimizedBooleanColumns)
            {
                queryExpression = new XGBoolOptimizingExpressionVisitor(Dependencies.SqlExpressionFactory)
                    .Visit(queryExpression);
            }

            queryExpression = new XGParametersInliningExpressionVisitor(
                Dependencies.TypeMappingSource,
                Dependencies.SqlExpressionFactory,
                _options).Process(queryExpression, parametersValues, out var canCache3);

            canCache &= canCache3;

            // Run the compatibility checks as late in the query pipeline (before the actual SQL translation happens) as reasonable.
            queryExpression = new XGCompatibilityExpressionVisitor(_options).Visit(queryExpression);

            return queryExpression;
        }

        /// <inheritdoc />
        protected override Expression ProcessSqlNullability(
            Expression queryExpression,
            IReadOnlyDictionary<string, object?> parametersValues,
            out bool canCache)
        {
            Check.NotNull(queryExpression, nameof(queryExpression));
            Check.NotNull(parametersValues, nameof(parametersValues));

            queryExpression = new XGSqlNullabilityProcessor(Dependencies, Parameters)
                .Process(queryExpression, parametersValues, out canCache);

            return queryExpression;
        }
    }
}
