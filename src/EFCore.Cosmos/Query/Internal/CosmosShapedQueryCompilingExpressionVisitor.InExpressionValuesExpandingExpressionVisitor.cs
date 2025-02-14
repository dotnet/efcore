// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Collections;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

public partial class CosmosShapedQueryCompilingExpressionVisitor
{
    private sealed class InExpressionValuesExpandingExpressionVisitor(
        ISqlExpressionFactory sqlExpressionFactory,
        IReadOnlyDictionary<string, object> parametersValues)
        : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression expression)
        {
            if (expression is InExpression inExpression)
            {
                IReadOnlyList<SqlExpression> values;

                switch (inExpression)
                {
                    case { Values: IReadOnlyList<SqlExpression> values2 }:
                        values = values2;
                        break;

                    // TODO: IN with subquery (return immediately, nothing to do here)

                    case { ValuesParameter: SqlParameterExpression valuesParameter }:
                    {
                        var typeMapping = valuesParameter.TypeMapping;
                        var mutableValues = new List<SqlExpression>();
                        foreach (var value in (IEnumerable)parametersValues[valuesParameter.Name])
                        {
                            mutableValues.Add(sqlExpressionFactory.Constant(value, value?.GetType() ?? typeof(object), typeMapping));
                        }

                        values = mutableValues;
                        break;
                    }

                    default:
                        throw new UnreachableException();
                }

                return values.Count == 0
                    ? sqlExpressionFactory.ApplyDefaultTypeMapping(sqlExpressionFactory.Constant(false))
                    : sqlExpressionFactory.In((SqlExpression)Visit(inExpression.Item), values);
            }

            return base.VisitExtension(expression);
        }
    }
}
