﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class FromSqlParameterExpandingExpressionVisitor : ExpressionVisitor
    {
        private readonly IDictionary<FromSqlExpression, Expression> _visitedFromSqlExpressions
            = new Dictionary<FromSqlExpression, Expression>(LegacyReferenceEqualityComparer.Instance);

        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;

        private IReadOnlyDictionary<string, object?> _parametersValues;
        private ParameterNameGenerator _parameterNameGenerator;
        private bool _canCache;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public FromSqlParameterExpandingExpressionVisitor(
            RelationalParameterBasedSqlProcessorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            _sqlExpressionFactory = dependencies.SqlExpressionFactory;
            _typeMappingSource = dependencies.TypeMappingSource;
            _parameterNameGeneratorFactory = dependencies.ParameterNameGeneratorFactory;
            _parametersValues = default!;
            _parameterNameGenerator = default!;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SelectExpression Expand(
            SelectExpression selectExpression,
            IReadOnlyDictionary<string, object?> parameterValues,
            out bool canCache)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));
            Check.NotNull(parameterValues, nameof(parameterValues));

            _visitedFromSqlExpressions.Clear();
            _parameterNameGenerator = _parameterNameGeneratorFactory.Create();
            _parametersValues = parameterValues;
            _canCache = true;

            var result = (SelectExpression)Visit(selectExpression);
            canCache = _canCache;

            return result;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            if (expression is FromSqlExpression fromSql)
            {
                if (!_visitedFromSqlExpressions.TryGetValue(fromSql, out var updatedFromSql))
                {
                    switch (fromSql.Arguments)
                    {
                        case ParameterExpression parameterExpression:
                            // parameter value will never be null. It could be empty object[]
                            var parameterValues = (object[])_parametersValues[parameterExpression.Name!]!;
                            _canCache = false;

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
                                            _typeMappingSource.GetMappingForValue(parameterValues[i]),
                                            parameterValues[i]?.GetType().IsNullableType()));
                                }
                            }

                            updatedFromSql = fromSql.Update(
                                Expression.Constant(new CompositeRelationalParameter(parameterExpression.Name!, subParameters)));

                            _visitedFromSqlExpressions[fromSql] = updatedFromSql;
                            break;

                        case ConstantExpression constantExpression:
                            var existingValues = constantExpression.GetConstantValue<object?[]>();
                            var constantValues = new object?[existingValues.Length];
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
                                        value, _typeMappingSource.GetMappingForValue(value));
                                }
                            }

                            updatedFromSql = fromSql.Update(Expression.Constant(constantValues, typeof(object?[])));

                            _visitedFromSqlExpressions[fromSql] = updatedFromSql;
                            break;

                        default:
                            Check.DebugAssert(false, "FromSql.Arguments must be Constant/ParameterExpression");
                            break;
                    }
                }

                return updatedFromSql;
            }

            return base.Visit(expression);
        }
    }
}
