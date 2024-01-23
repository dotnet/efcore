// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RawSqlCommandBuilder : IRawSqlCommandBuilder
{
    private readonly IRelationalCommandBuilderFactory _relationalCommandBuilderFactory;
    private readonly ISqlGenerationHelper _sqlGenerationHelper;
    private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RawSqlCommandBuilder(
        IRelationalCommandBuilderFactory relationalCommandBuilderFactory,
        ISqlGenerationHelper sqlGenerationHelper,
        IParameterNameGeneratorFactory parameterNameGeneratorFactory,
        IRelationalTypeMappingSource typeMappingSource)
    {
        _relationalCommandBuilderFactory = relationalCommandBuilderFactory;
        _sqlGenerationHelper = sqlGenerationHelper;
        _parameterNameGeneratorFactory = parameterNameGeneratorFactory;
        _typeMappingSource = typeMappingSource;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IRelationalCommand Build(string sql)
        => _relationalCommandBuilderFactory
            .Create()
            .Append(sql)
            .Build();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual RawSqlCommand Build(string sql, IEnumerable<object> parameters)
        => Build(sql, parameters, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual RawSqlCommand Build(string sql, IEnumerable<object> parameters, IModel? model)
    {
        var relationalCommandBuilder = _relationalCommandBuilderFactory.Create();

        var substitutions = new List<string>();

        var parameterNameGenerator = _parameterNameGeneratorFactory.Create();

        var parameterValues = new Dictionary<string, object?>();

        foreach (var parameter in parameters)
        {
            if (parameter is DbParameter dbParameter)
            {
                if (string.IsNullOrEmpty(dbParameter.ParameterName))
                {
                    dbParameter.ParameterName = _sqlGenerationHelper.GenerateParameterName(parameterNameGenerator.GenerateNext());
                }

                substitutions.Add(_sqlGenerationHelper.GenerateParameterName(dbParameter.ParameterName));
                relationalCommandBuilder.AddRawParameter(dbParameter.ParameterName, dbParameter);
            }
            else
            {
                var parameterName = parameterNameGenerator.GenerateNext();
                var substitutedName = _sqlGenerationHelper.GenerateParameterName(parameterName);

                substitutions.Add(substitutedName);
                var typeMapping = parameter == null
                    ? model == null
                        ? _typeMappingSource.GetMappingForValue(null)
                        : _typeMappingSource.GetMappingForValue(null, model)
                    : model == null
                        ? _typeMappingSource.GetMapping(parameter.GetType())
                        : _typeMappingSource.GetMapping(parameter.GetType(), model);
                var nullable = parameter == null || parameter.GetType().IsNullableType();

                relationalCommandBuilder.AddParameter(parameterName, substitutedName, typeMapping, nullable);
                parameterValues.Add(parameterName, parameter);
            }
        }

        // ReSharper disable once CoVariantArrayConversion
        sql = string.Format(sql, substitutions.ToArray());

        return new RawSqlCommand(
            relationalCommandBuilder.Append(sql).Build(),
            parameterValues);
    }
}
