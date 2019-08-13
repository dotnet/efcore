// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    public partial class CosmosShapedQueryCompilingExpressionVisitor
    {
        private class InExpressionValuesExpandingExpressionVisitor : ExpressionVisitor
        {
            private readonly ISqlExpressionFactory _sqlExpressionFactory;
            private readonly IReadOnlyDictionary<string, object> _parametersValues;

            public InExpressionValuesExpandingExpressionVisitor(
                ISqlExpressionFactory sqlExpressionFactory, IReadOnlyDictionary<string, object> parametersValues)
            {
                _sqlExpressionFactory = sqlExpressionFactory;
                _parametersValues = parametersValues;
            }

            public override Expression Visit(Expression expression)
            {
                if (expression is InExpression inExpression)
                {
                    var inValues = new List<object>();
                    var hasNullValue = false;
                    CoreTypeMapping typeMapping = null;

                    switch (inExpression.Values)
                    {
                        case SqlConstantExpression sqlConstant:
                        {
                            typeMapping = sqlConstant.TypeMapping;
                            var values = (IEnumerable)sqlConstant.Value;
                            foreach (var value in values)
                            {
                                if (value == null)
                                {
                                    hasNullValue = true;
                                    continue;
                                }

                                inValues.Add(value);
                            }
                        }
                        break;

                        case SqlParameterExpression sqlParameter:
                        {
                            typeMapping = sqlParameter.TypeMapping;
                            var values = (IEnumerable)_parametersValues[sqlParameter.Name];
                            foreach (var value in values)
                            {
                                if (value == null)
                                {
                                    hasNullValue = true;
                                    continue;
                                }

                                inValues.Add(value);
                            }
                        }
                        break;
                   }

                    var updatedInExpression = inValues.Count > 0
                        ? _sqlExpressionFactory.In(
                            (SqlExpression)Visit(inExpression.Item),
                            _sqlExpressionFactory.Constant(inValues, typeMapping),
                            inExpression.IsNegated)
                        : null;

                    var nullCheckExpression = hasNullValue
                        ? _sqlExpressionFactory.IsNull(inExpression.Item)
                        : null;

                    if (updatedInExpression != null && nullCheckExpression != null)
                    {
                        return _sqlExpressionFactory.OrElse(updatedInExpression, nullCheckExpression);
                    }

                    if (updatedInExpression == null && nullCheckExpression == null)
                    {
                        return _sqlExpressionFactory.Equal(_sqlExpressionFactory.Constant(true), _sqlExpressionFactory.Constant(false));
                    }

                    return (SqlExpression)updatedInExpression ?? nullCheckExpression;
                }

                return base.Visit(expression);
            }
        }
    }
}
