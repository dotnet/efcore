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

            public ParameterValueBasedSelectExpressionOptimizer(
                ISqlExpressionFactory sqlExpressionFactory,
                IParameterNameGeneratorFactory parameterNameGeneratorFactory)
            {
                _sqlExpressionFactory = sqlExpressionFactory;
                _parameterNameGeneratorFactory = parameterNameGeneratorFactory;
            }

            public SelectExpression Optimize(SelectExpression selectExpression, IReadOnlyDictionary<string, object> parametersValues)
            {
                var query = new InExpressionValuesExpandingExpressionVisitor(
                    _sqlExpressionFactory, parametersValues).Visit(selectExpression);

                query = new FromSqlParameterApplyingExpressionVisitor(
                    _sqlExpressionFactory,
                    _parameterNameGeneratorFactory.Create(),
                    parametersValues).Visit(query);

                return (SelectExpression)query;
            }
        }
    }
}
