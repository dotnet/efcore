// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class FromSqlParameterApplyingExpressionVisitor : ExpressionVisitor
    {
        private readonly IDictionary<FromSqlExpression, Expression> _visitedFromSqlExpressions
            = new Dictionary<FromSqlExpression, Expression>(ReferenceEqualityComparer.Instance);

        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly ParameterNameGenerator _parameterNameGenerator;
        private readonly IReadOnlyDictionary<string, object> _parametersValues;

        public FromSqlParameterApplyingExpressionVisitor(
            [NotNull] ISqlExpressionFactory sqlExpressionFactory,
            [NotNull] ParameterNameGenerator parameterNameGenerator,
            [NotNull] IReadOnlyDictionary<string, object> parametersValues)
        {
            Check.NotNull(sqlExpressionFactory, nameof(sqlExpressionFactory));
            Check.NotNull(parameterNameGenerator, nameof(parameterNameGenerator));
            Check.NotNull(parametersValues, nameof(parametersValues));

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
