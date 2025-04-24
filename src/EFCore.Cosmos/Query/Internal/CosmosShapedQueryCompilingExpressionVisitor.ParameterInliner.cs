// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Collections;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

public partial class CosmosShapedQueryCompilingExpressionVisitor
{
    private sealed class ParameterInliner(
        ISqlExpressionFactory sqlExpressionFactory,
        IReadOnlyDictionary<string, object> parametersValues)
        : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression expression)
        {
            expression = base.VisitExtension(expression);

            switch (expression)
            {
                // Inlines array parameter of InExpression, transforming: 'item IN (@valuesArray)' to: 'item IN (value1, value2)'
                case InExpression inExpression:
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

                // Converts Offset and Limit parameters to constants when ORDER BY RANK is detected in the SelectExpression (i.e. we order by scoring function)
                // Cosmos only supports constants in Offset and Limit for this scenario currently (ORDER BY RANK limitation)
                case SelectExpression { Orderings: [{ Expression: SqlFunctionExpression { IsScoringFunction: true } }], Limit: var limit, Offset: var offset } hybridSearch
                    when limit is SqlParameterExpression || offset is SqlParameterExpression:
                {
                    if (hybridSearch.Limit is SqlParameterExpression limitPrm)
                    {
                        hybridSearch.ApplyLimit(
                            sqlExpressionFactory.Constant(
                                parametersValues[limitPrm.Name],
                                limitPrm.TypeMapping));
                    }

                    if (hybridSearch.Offset is SqlParameterExpression offsetPrm)
                    {
                        hybridSearch.ApplyOffset(
                            sqlExpressionFactory.Constant(
                                parametersValues[offsetPrm.Name],
                                offsetPrm.TypeMapping));
                    }

                    return base.VisitExtension(expression);
                }

                // Inlines array parameter of full-text functions, transforming FullTextContainsAll(x, @keywordsArray) to FullTextContainsAll(x, keyword1, keyword2)) 
                case SqlFunctionExpression
                {
                    Name: "FullTextContainsAny" or "FullTextContainsAll",
                    Arguments: [var property, SqlParameterExpression { TypeMapping: { ElementTypeMapping: var elementTypeMapping }, Type: Type type } keywords]
                } fullTextContainsAllAnyFunction
                when type == typeof(string[]):
                {
                    var keywordValues = new List<SqlExpression>();
                    foreach (var value in (IEnumerable)parametersValues[keywords.Name])
                    {
                        keywordValues.Add(sqlExpressionFactory.Constant(value, typeof(string), elementTypeMapping));
                    }

                    return sqlExpressionFactory.Function(
                        fullTextContainsAllAnyFunction.Name,
                        [property, .. keywordValues],
                        fullTextContainsAllAnyFunction.Type,
                        fullTextContainsAllAnyFunction.TypeMapping);
                }

                // Inlines array parameter of full-text score, transforming FullTextScore(x, @keywordsArray) to FullTextScore(x, [keyword1, keyword2])) 
                case SqlFunctionExpression
                {
                    Name: "FullTextScore",
                    IsScoringFunction: true,
                    Arguments: [var property, SqlParameterExpression { TypeMapping: { ElementTypeMapping: not null } typeMapping } keywords]
                } fullTextScoreFunction:
                {
                    var keywordValues = new List<string>();
                    foreach (var value in (IEnumerable)parametersValues[keywords.Name])
                    {
                        keywordValues.Add((string)value);
                    }

                    return new SqlFunctionExpression(
                        fullTextScoreFunction.Name,
                        scoringFunction: true,
                        [property, sqlExpressionFactory.Constant(keywordValues, typeMapping)],
                        fullTextScoreFunction.Type,
                        fullTextScoreFunction.TypeMapping);
                }

                default:
                    return expression;
            }
        }
    }
}
