// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Collections;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

public partial class CosmosShapedQueryCompilingExpressionVisitor
{
    private sealed class InExpressionValuesExpandingExpressionVisitor : ExpressionVisitor
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IReadOnlyDictionary<string, object> _parametersValues;

        public InExpressionValuesExpandingExpressionVisitor(
            ISqlExpressionFactory sqlExpressionFactory,
            IReadOnlyDictionary<string, object> parametersValues)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _parametersValues = parametersValues;
        }

        public override Expression Visit(Expression expression)
        {
            if (expression is InExpression inExpression)
            {
                var inValues = new List<SqlExpression>();
                var hasNullValue = false;

                switch (inExpression)
                {
                    case { ValuesParameter: SqlParameterExpression valuesParameter }:
                    {
                        var typeMapping = valuesParameter.TypeMapping;

                        foreach (var value in (IEnumerable)_parametersValues[valuesParameter.Name])
                        {
                            if (value is null)
                            {
                                hasNullValue = true;
                                continue;
                            }

                            inValues.Add(_sqlExpressionFactory.Constant(value, typeMapping));
                        }

                        break;
                    }

                    case { Values: IReadOnlyList<SqlExpression> values }:
                    {
                        foreach (var value in values)
                        {
                            if (value is not (SqlConstantExpression or SqlParameterExpression))
                            {
                                throw new InvalidOperationException(CosmosStrings.OnlyConstantsAndParametersAllowedInContains);
                            }

                            if (IsNull(value))
                            {
                                hasNullValue = true;
                                continue;
                            }

                            inValues.Add(value);
                        }

                        break;
                    }

                    default:
                        throw new UnreachableException();
                }

                var updatedInExpression = inValues.Count > 0
                    ? _sqlExpressionFactory.In((SqlExpression)Visit(inExpression.Item), inValues)
                    : null;

                var nullCheckExpression = hasNullValue
                    ? _sqlExpressionFactory.IsNull(inExpression.Item)
                    : null;

                if (updatedInExpression != null
                    && nullCheckExpression != null)
                {
                    return _sqlExpressionFactory.OrElse(updatedInExpression, nullCheckExpression);
                }

                if (updatedInExpression == null
                    && nullCheckExpression == null)
                {
                    return _sqlExpressionFactory.Equal(_sqlExpressionFactory.Constant(true), _sqlExpressionFactory.Constant(false));
                }

                return (SqlExpression)updatedInExpression ?? nullCheckExpression;
            }

            return base.Visit(expression);
        }

        private bool IsNull(SqlExpression expression)
            => expression is SqlConstantExpression { Value: null }
                || expression is SqlParameterExpression { Name: string parameterName } && _parametersValues[parameterName] is null;
    }
}
