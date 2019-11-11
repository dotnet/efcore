// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ParameterValueBasedSelectExpressionOptimizer
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

        public virtual (SelectExpression selectExpression, bool canCache) Optimize(
            SelectExpression selectExpression, IReadOnlyDictionary<string, object> parametersValues)
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

        private class ParameterNullabilityBasedSqlExpressionOptimizingExpressionVisitor : SqlExpressionOptimizingExpressionVisitor
        {
            private readonly IReadOnlyDictionary<string, object> _parametersValues;

            public ParameterNullabilityBasedSqlExpressionOptimizingExpressionVisitor(
                ISqlExpressionFactory sqlExpressionFactory,
                bool useRelationalNulls,
                IReadOnlyDictionary<string, object> parametersValues)
                : base(sqlExpressionFactory, useRelationalNulls)
            {
                _parametersValues = parametersValues;
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                // workaround for issue #18492
                var newExpression = base.VisitExtension(extensionExpression);
                if (newExpression is SelectExpression newSelectExpression)
                {
                    var changed = false;
                    var newPredicate = newSelectExpression.Predicate;
                    var newHaving = newSelectExpression.Having;
                    if (newSelectExpression.Predicate is SqlConstantExpression predicateConstantExpression
                        && predicateConstantExpression.Value is bool predicateBoolValue
                        && !predicateBoolValue)
                    {
                        changed = true;
                        newPredicate = SqlExpressionFactory.Equal(
                            predicateConstantExpression,
                            SqlExpressionFactory.Constant(true, predicateConstantExpression.TypeMapping));
                    }

                    if (newSelectExpression.Having is SqlConstantExpression havingConstantExpression
                        && havingConstantExpression.Value is bool havingBoolValue
                        && !havingBoolValue)
                    {
                        changed = true;
                        newHaving = SqlExpressionFactory.Equal(
                            havingConstantExpression,
                            SqlExpressionFactory.Constant(true, havingConstantExpression.TypeMapping));
                    }

                    return changed
                        ? newSelectExpression.Update(
                            newSelectExpression.Projection.ToList(),
                            newSelectExpression.Tables.ToList(),
                            newPredicate,
                            newSelectExpression.GroupBy.ToList(),
                            newHaving,
                            newSelectExpression.Orderings.ToList(),
                            newSelectExpression.Limit,
                            newSelectExpression.Offset,
                            newSelectExpression.IsDistinct,
                            newSelectExpression.Alias)
                        : newSelectExpression;
                }

                return newExpression;
            }

            protected override Expression VisitSqlUnaryExpression(SqlUnaryExpression sqlUnaryExpression)
            {
                var result = base.VisitSqlUnaryExpression(sqlUnaryExpression);
                if (result is SqlUnaryExpression newUnaryExpression
                    && newUnaryExpression.Operand is SqlParameterExpression parameterOperand)
                {
                    var parameterValue = _parametersValues[parameterOperand.Name];
                    if (sqlUnaryExpression.OperatorType == ExpressionType.Equal)
                    {
                        return SqlExpressionFactory.Constant(parameterValue == null, sqlUnaryExpression.TypeMapping);
                    }

                    if (sqlUnaryExpression.OperatorType == ExpressionType.NotEqual)
                    {
                        return SqlExpressionFactory.Constant(parameterValue != null, sqlUnaryExpression.TypeMapping);
                    }
                }

                return result;
            }

            protected override Expression VisitSqlBinaryExpression(SqlBinaryExpression sqlBinaryExpression)
            {
                var result = base.VisitSqlBinaryExpression(sqlBinaryExpression);
                if (result is SqlBinaryExpression sqlBinaryResult)
                {
                    var leftNullParameter = sqlBinaryResult.Left is SqlParameterExpression leftParameter
                        && _parametersValues[leftParameter.Name] == null;

                    var rightNullParameter = sqlBinaryResult.Right is SqlParameterExpression rightParameter
                        && _parametersValues[rightParameter.Name] == null;

                    if ((sqlBinaryResult.OperatorType == ExpressionType.Equal || sqlBinaryResult.OperatorType == ExpressionType.NotEqual)
                        && (leftNullParameter || rightNullParameter))
                    {
                        return SimplifyNullComparisonExpression(
                            sqlBinaryResult.OperatorType,
                            sqlBinaryResult.Left,
                            sqlBinaryResult.Right,
                            leftNullParameter,
                            rightNullParameter,
                            sqlBinaryResult.TypeMapping);
                    }
                }

                return result;
            }
        }

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
                if (expression is InExpression inExpression
                    && inExpression.Values != null)
                {
                    var inValues = new List<object>();
                    var hasNullValue = false;
                    RelationalTypeMapping typeMapping = null;

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

                            break;
                        }

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

                            break;
                        }
                    }

                    var updatedInExpression = inValues.Count > 0
                        ? _sqlExpressionFactory.In(
                            (SqlExpression)Visit(inExpression.Item),
                            _sqlExpressionFactory.Constant(inValues, typeMapping),
                            inExpression.IsNegated)
                        : null;

                    var nullCheckExpression = hasNullValue
                        ? inExpression.IsNegated
                            ? _sqlExpressionFactory.IsNotNull(inExpression.Item)
                            : _sqlExpressionFactory.IsNull(inExpression.Item)
                        : null;

                    if (updatedInExpression != null
                        && nullCheckExpression != null)
                    {
                        return inExpression.IsNegated
                            ? _sqlExpressionFactory.AndAlso(updatedInExpression, nullCheckExpression)
                            : _sqlExpressionFactory.OrElse(updatedInExpression, nullCheckExpression);
                    }

                    if (updatedInExpression == null
                        && nullCheckExpression == null)
                    {
                        return _sqlExpressionFactory.Equal(
                            _sqlExpressionFactory.Constant(true), _sqlExpressionFactory.Constant(inExpression.IsNegated));
                    }

                    return (SqlExpression)updatedInExpression ?? nullCheckExpression;
                }

                return base.Visit(expression);
            }
        }

        private class FromSqlParameterApplyingExpressionVisitor : ExpressionVisitor
        {
            private readonly IDictionary<FromSqlExpression, Expression> _visitedFromSqlExpressions
                = new Dictionary<FromSqlExpression, Expression>(ReferenceEqualityComparer.Instance);

            private readonly ISqlExpressionFactory _sqlExpressionFactory;
            private readonly ParameterNameGenerator _parameterNameGenerator;
            private readonly IReadOnlyDictionary<string, object> _parametersValues;

            public FromSqlParameterApplyingExpressionVisitor(
                ISqlExpressionFactory sqlExpressionFactory,
                ParameterNameGenerator parameterNameGenerator,
                IReadOnlyDictionary<string, object> parametersValues)
            {
                _sqlExpressionFactory = sqlExpressionFactory;
                _parameterNameGenerator = parameterNameGenerator;
                _parametersValues = parametersValues;
            }

            public override Expression Visit(Expression expression)
            {
                if (expression is FromSqlExpression fromSql)
                {
                    if (!_visitedFromSqlExpressions.TryGetValue(fromSql, out var updatedFromSql))
                    {
                        switch (fromSql.Arguments)
                        {
                            case ParameterExpression parameterExpression:
                                var parameterValues = (object[])_parametersValues[parameterExpression.Name];

                                var subParameters = new List<IRelationalParameter>(parameterValues.Length);
                                // ReSharper disable once ForCanBeConvertedToForeach
                                for (var i = 0; i < parameterValues.Length; i++)
                                {
                                    var parameterName = _parameterNameGenerator.GenerateNext();
                                    if (parameterValues[i] is DbParameter dbParameter)
                                    {
                                        if (string.IsNullOrEmpty(dbParameter.ParameterName))
                                        {
                                            dbParameter.ParameterName = parameterName;
                                        }
                                        else
                                        {
                                            parameterName = dbParameter.ParameterName;
                                        }

                                        subParameters.Add(new RawRelationalParameter(parameterName, dbParameter));
                                    }
                                    else
                                    {
                                        subParameters.Add(
                                            new TypeMappedRelationalParameter(
                                                parameterName,
                                                parameterName,
                                                _sqlExpressionFactory.GetTypeMappingForValue(parameterValues[i]),
                                                parameterValues[i]?.GetType().IsNullableType()));
                                    }
                                }

                                updatedFromSql = new FromSqlExpression(
                                    fromSql.Sql,
                                    Expression.Constant(
                                        new CompositeRelationalParameter(
                                            parameterExpression.Name,
                                            subParameters)),
                                    fromSql.Alias);

                                _visitedFromSqlExpressions[fromSql] = updatedFromSql;
                                break;

                            case ConstantExpression constantExpression:
                                var existingValues = (object[])constantExpression.Value;
                                var constantValues = new object[existingValues.Length];
                                for (var i = 0; i < existingValues.Length; i++)
                                {
                                    var value = existingValues[i];
                                    if (value is DbParameter dbParameter)
                                    {
                                        var parameterName = _parameterNameGenerator.GenerateNext();
                                        if (string.IsNullOrEmpty(dbParameter.ParameterName))
                                        {
                                            dbParameter.ParameterName = parameterName;
                                        }
                                        else
                                        {
                                            parameterName = dbParameter.ParameterName;
                                        }

                                        constantValues[i] = new RawRelationalParameter(parameterName, dbParameter);
                                    }
                                    else
                                    {
                                        constantValues[i] = _sqlExpressionFactory.Constant(
                                            value, _sqlExpressionFactory.GetTypeMappingForValue(value));
                                    }
                                }

                                updatedFromSql = new FromSqlExpression(
                                    fromSql.Sql,
                                    Expression.Constant(constantValues, typeof(object[])),
                                    fromSql.Alias);

                                _visitedFromSqlExpressions[fromSql] = updatedFromSql;
                                break;
                        }
                    }

                    return updatedFromSql;
                }

                return base.Visit(expression);
            }
        }
    }
}
