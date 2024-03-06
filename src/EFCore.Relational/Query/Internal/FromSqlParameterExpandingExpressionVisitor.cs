// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class FromSqlParameterExpandingExpressionVisitor : ExpressionVisitor
{
    private readonly IDictionary<FromSqlExpression, Expression> _visitedFromSqlExpressions
        = new Dictionary<FromSqlExpression, Expression>(ReferenceEqualityComparer.Instance);

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
        Dependencies = dependencies;

        _sqlExpressionFactory = dependencies.SqlExpressionFactory;
        _typeMappingSource = dependencies.TypeMappingSource;
        _parameterNameGeneratorFactory = dependencies.ParameterNameGeneratorFactory;
        _parametersValues = default!;
        _parameterNameGenerator = default!;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalParameterBasedSqlProcessorDependencies Dependencies { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression Expand(
        Expression queryExpression,
        IReadOnlyDictionary<string, object?> parameterValues,
        out bool canCache)
    {
        _visitedFromSqlExpressions.Clear();
        _parameterNameGenerator = _parameterNameGeneratorFactory.Create();
        _parametersValues = parameterValues;
        _canCache = true;

        var result = Visit(queryExpression);
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
        if (expression is not FromSqlExpression fromSql)
        {
            return base.Visit(expression);
        }

        if (_visitedFromSqlExpressions.TryGetValue(fromSql, out var visitedFromSql))
        {
            return visitedFromSql;
        }

        switch (fromSql.Arguments)
        {
            case ParameterExpression parameterExpression:
                // parameter value will never be null. It could be empty object?[]
                var parameterValues = (object?[])_parametersValues[parameterExpression.Name!]!;
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

                return _visitedFromSqlExpressions[fromSql] = fromSql.Update(
                    Expression.Constant(new CompositeRelationalParameter(parameterExpression.Name!, subParameters)));

            case ConstantExpression { Value: object?[] existingValues }:
            {
                var constantValues = new object?[existingValues.Length];
                for (var i = 0; i < existingValues.Length; i++)
                {
                    constantValues[i] = ProcessConstantValue(existingValues[i]);
                }

                return _visitedFromSqlExpressions[fromSql] = fromSql.Update(Expression.Constant(constantValues, typeof(object[])));
            }

            case NewArrayExpression { Expressions: var expressions }:
            {
                var constantValues = new object?[expressions.Count];
                for (var i = 0; i < constantValues.Length; i++)
                {
                    if (expressions[i] is not SqlConstantExpression { Value: var existingValue })
                    {
                        Check.DebugFail("FromSql.Arguments must be Constant/ParameterExpression");
                        throw new InvalidOperationException();
                    }

                    constantValues[i] = ProcessConstantValue(existingValue);
                }

                return _visitedFromSqlExpressions[fromSql] = fromSql.Update(Expression.Constant(constantValues, typeof(object[])));
            }

            default:
                Check.DebugFail("FromSql.Arguments must be Constant/ParameterExpression");
                return null;
        }

        object ProcessConstantValue(object? existingConstantValue)
        {
            if (existingConstantValue is DbParameter dbParameter)
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

                return new RawRelationalParameter(parameterName, dbParameter);
            }

            return _sqlExpressionFactory.Constant(
                existingConstantValue,
                existingConstantValue?.GetType() ?? typeof(object),
                _typeMappingSource.GetMappingForValue(existingConstantValue));
        }
    }
}
