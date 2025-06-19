// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This visitor processes the parameters of <see cref="FromSqlExpression" />, expanding them and creating the appropriate
///     <see cref="IRelationalParameter" /> for them, and ensures parameter names are unique across the SQL tree.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public class RelationalParameterProcessor : ExpressionVisitor
{
    private readonly IDictionary<FromSqlExpression, Expression> _visitedFromSqlExpressions
        = new Dictionary<FromSqlExpression, Expression>(ReferenceEqualityComparer.Instance);

    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly ISqlGenerationHelper _sqlGenerationHelper;
    private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;

    /// <summary>
    ///     Contains parameter names seen so far, for uniquification. These parameter names have already gone through
    ///     <see cref="ISqlGenerationHelper.GenerateParameterName(string)"/> (i.e. they're prefixed), since
    ///     <see cref="DbParameter.ParameterName" /> can be prefixed or not.
    /// </summary>
    private readonly HashSet<string> _prefixedParameterNames = new();

    private readonly Dictionary<string, SqlParameterExpression> _sqlParameters = new();

    private CacheSafeParameterFacade _parametersFacade;
    private ParameterNameGenerator _parameterNameGenerator;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RelationalParameterProcessor(
        RelationalParameterBasedSqlProcessorDependencies dependencies)
    {
        Dependencies = dependencies;

        _sqlExpressionFactory = dependencies.SqlExpressionFactory;
        _typeMappingSource = dependencies.TypeMappingSource;
        _parameterNameGeneratorFactory = dependencies.ParameterNameGeneratorFactory;
        _sqlGenerationHelper = dependencies.SqlGenerationHelper;
        _parametersFacade = default!;
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
    public virtual Expression Expand(Expression queryExpression, CacheSafeParameterFacade parametersFacade)
    {
        _visitedFromSqlExpressions.Clear();
        _prefixedParameterNames.Clear();
        _sqlParameters.Clear();
        _parameterNameGenerator = _parameterNameGeneratorFactory.Create();
        _parametersFacade = parametersFacade;

        var result = Visit(queryExpression);

        return result;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression expression)
        => expression switch
        {
            FromSqlExpression fromSql
                => _visitedFromSqlExpressions.TryGetValue(fromSql, out var visitedFromSql)
                    ? visitedFromSql
                    : _visitedFromSqlExpressions[fromSql] = VisitFromSql(fromSql),

            SqlParameterExpression parameter => VisitSqlParameter(parameter),

            _ => base.VisitExtension(expression)
        };

    private SqlParameterExpression VisitSqlParameter(SqlParameterExpression parameter)
    {
        var typeMapping = parameter.TypeMapping!;

        // Try to see if a parameter already exists - if so, just integrate the same placeholder into the SQL instead of sending the same
        // data twice.
        // Note that if the type mapping differs, we do send the same data twice (e.g. the same string may be sent once as Unicode, once as
        // non-Unicode).
        // TODO: Note that we perform Equals comparison on the value converter. We should be able to do reference comparison, but for
        // that we need to ensure that there's only ever one type mapping instance (i.e. no type mappings are ever instantiated out of the
        // type mapping source). See #30677.
        if (_sqlParameters.TryGetValue(parameter.InvariantName, out var existingParameter)
            && existingParameter is { TypeMapping: RelationalTypeMapping existingTypeMapping }
            && string.Equals(existingTypeMapping.StoreType, typeMapping.StoreType, StringComparison.OrdinalIgnoreCase)
            && (existingTypeMapping.Converter is null && typeMapping.Converter is null
                || existingTypeMapping.Converter is not null && existingTypeMapping.Converter.Equals(typeMapping.Converter)))
        {
            return parameter;
        }

        var uniquifiedName = UniquifyParameterName(parameter.Name);
        var newParameter = uniquifiedName == parameter.Name
            ? parameter
            : new SqlParameterExpression(
                parameter.InvariantName,
                uniquifiedName,
                parameter.Type,
                parameter.IsNullable,
                parameter.ShouldBeConstantized,
                parameter.TypeMapping);

        return _sqlParameters[newParameter.InvariantName] = newParameter;
    }

    private FromSqlExpression VisitFromSql(FromSqlExpression fromSql)
    {
        switch (fromSql.Arguments)
        {
            case QueryParameterExpression queryParameter:
                // parameter value will never be null. It could be empty object?[]
                var parameters = _parametersFacade.GetParametersAndDisableSqlCaching();
                var parameterValues = (object?[])parameters[queryParameter.Name]!;

                var subParameters = new List<IRelationalParameter>(parameterValues.Length);
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < parameterValues.Length; i++)
                {
                    if (parameterValues[i] is DbParameter dbParameter)
                    {
                        ProcessDbParameter(dbParameter);
                        subParameters.Add(new RawRelationalParameter(dbParameter.ParameterName, dbParameter));
                    }
                    else
                    {
                        var parameterName = GenerateNewParameterName();
                        subParameters.Add(
                            new TypeMappedRelationalParameter(
                                parameterName,
                                parameterName,
                                _typeMappingSource.GetMappingForValue(parameterValues[i]),
                                parameterValues[i]?.GetType().IsNullableType()));
                    }
                }

                return fromSql.Update(Expression.Constant(new CompositeRelationalParameter(queryParameter.Name, subParameters)));

            case ConstantExpression { Value: object?[] existingValues }:
            {
                var constantValues = new object?[existingValues.Length];

                for (var i = 0; i < existingValues.Length; i++)
                {
                    constantValues[i] = ProcessConstantValue(existingValues[i]);
                }

                return fromSql.Update(Expression.Constant(constantValues, typeof(object[])));
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

                return fromSql.Update(Expression.Constant(constantValues, typeof(object[])));
            }

            default:
                throw new UnreachableException("FromSql.Arguments must be Constant/QueryParameterExpression");
        }

        object ProcessConstantValue(object? existingConstantValue)
        {
            if (existingConstantValue is DbParameter dbParameter)
            {
                ProcessDbParameter(dbParameter);
                return new RawRelationalParameter(dbParameter.ParameterName, dbParameter);
            }

            return _sqlExpressionFactory.Constant(
                existingConstantValue,
                existingConstantValue?.GetType() ?? typeof(object),
                _typeMappingSource.GetMappingForValue(existingConstantValue));
        }

        void ProcessDbParameter(DbParameter dbParameter)
        {
            dbParameter.ParameterName = string.IsNullOrEmpty(dbParameter.ParameterName)
                ? GenerateNewParameterName()
                : UniquifyParameterName(dbParameter.ParameterName);
        }
    }

    private string GenerateNewParameterName()
    {
        string name, prefixedName;
        do
        {
            name = _parameterNameGenerator.GenerateNext();
            prefixedName = _sqlGenerationHelper.GenerateParameterName(name);
        }
        while (_prefixedParameterNames.Contains(prefixedName));

        _prefixedParameterNames.Add(prefixedName);
        return name;
    }

    private string UniquifyParameterName(string originalName)
    {
        var parameterName = originalName;
        var prefixedName = _sqlGenerationHelper.GenerateParameterName(originalName);
        for (var j = 0; _prefixedParameterNames.Contains(prefixedName); j++)
        {
            parameterName = originalName + j;
            prefixedName = _sqlGenerationHelper.GenerateParameterName(parameterName);
        }

        _prefixedParameterNames.Add(prefixedName);
        return parameterName;
    }
}
