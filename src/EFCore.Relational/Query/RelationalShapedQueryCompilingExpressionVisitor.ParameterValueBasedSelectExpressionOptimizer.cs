// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class RelationalShapedQueryCompilingExpressionVisitor
    {
        private class ParameterValueBasedSelectExpressionOptimizer
        {
            private readonly ISqlExpressionFactory _sqlExpressionFactory;
            private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;
            private readonly bool _useRelationalNulls;

            public ParameterValueBasedSelectExpressionOptimizer(
                ISqlExpressionFactory sqlExpressionFactory,
                IParameterNameGeneratorFactory parameterNameGeneratorFactory,
                bool useRelationalNulls)
            {
                _sqlExpressionFactory = sqlExpressionFactory;
                _parameterNameGeneratorFactory = parameterNameGeneratorFactory;
                _useRelationalNulls = useRelationalNulls;
            }

            public (SelectExpression selectExpression, bool canCache) Optimize(SelectExpression selectExpression, IReadOnlyDictionary<string, object> parametersValues)
            {
                var canCache = true;

                var inExpressionOptimized = new InExpressionValuesExpandingExpressionVisitor(
                    _sqlExpressionFactory, parametersValues).Visit(selectExpression);

                if (!ReferenceEquals(selectExpression, inExpressionOptimized))
                {
                    canCache = false;
                }

                var nullParametersOptimized = new ParameterNullabilityBasedSqlExpressionOptimizingExpressionVisitor(
                    _sqlExpressionFactory, _useRelationalNulls, parametersValues).Visit(inExpressionOptimized);

                var fromSqlParameterOptimized = new FromSqlParameterApplyingExpressionVisitor(
                    _sqlExpressionFactory,
                    _parameterNameGeneratorFactory.Create(),
                    parametersValues).Visit(nullParametersOptimized);

                if (!ReferenceEquals(nullParametersOptimized, fromSqlParameterOptimized))
                {
                    canCache = false;
                }

                return (selectExpression: (SelectExpression)fromSqlParameterOptimized, canCache);
            }
        }
    }
}
