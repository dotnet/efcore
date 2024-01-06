// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <inheritdoc />
public class RelationalMethodCallTranslatorProvider : IMethodCallTranslatorProvider
{
    private readonly List<IMethodCallTranslator> _plugins = [];
    private readonly List<IMethodCallTranslator> _translators = [];
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     Creates a new instance of the <see cref="RelationalMethodCallTranslatorProvider" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    public RelationalMethodCallTranslatorProvider(RelationalMethodCallTranslatorProviderDependencies dependencies)
    {
        Dependencies = dependencies;

        _plugins.AddRange(dependencies.Plugins.SelectMany(p => p.Translators));

        var sqlExpressionFactory = dependencies.SqlExpressionFactory;

        _translators.AddRange(
            new IMethodCallTranslator[]
            {
                new EqualsTranslator(sqlExpressionFactory),
                new StringMethodTranslator(sqlExpressionFactory),
                new CollateTranslator(),
                new ContainsTranslator(sqlExpressionFactory),
                new LikeTranslator(sqlExpressionFactory),
                new EnumHasFlagTranslator(sqlExpressionFactory),
                new GetValueOrDefaultTranslator(sqlExpressionFactory),
                new ComparisonTranslator(sqlExpressionFactory),
                new ByteArraySequenceEqualTranslator(sqlExpressionFactory),
                new RandomTranslator(sqlExpressionFactory)
            });
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual RelationalMethodCallTranslatorProviderDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        IModel model,
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        var dbFunction = model.FindDbFunction(method);
        if (dbFunction != null)
        {
            if (dbFunction.Translation != null)
            {
                var translation = dbFunction.Translation.Invoke(
                    arguments.Select(e => _sqlExpressionFactory.ApplyDefaultTypeMapping(e)).ToList());

                if (translation.Type.IsNullableValueType())
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DbFunctionNullableValueReturnType(
                            dbFunction.ModelName, dbFunction.ReturnType.ShortDisplayName()));
                }

                return translation;
            }

            var argumentsPropagateNullability = dbFunction.Parameters.Select(p => p.PropagatesNullability);

            return dbFunction.IsBuiltIn
                ? _sqlExpressionFactory.Function(
                    dbFunction.Name,
                    arguments,
                    dbFunction.IsNullable,
                    argumentsPropagateNullability,
                    method.ReturnType.UnwrapNullableType(),
                    dbFunction.TypeMapping)
                : _sqlExpressionFactory.Function(
                    dbFunction.Schema,
                    dbFunction.Name,
                    arguments,
                    dbFunction.IsNullable,
                    argumentsPropagateNullability,
                    method.ReturnType.UnwrapNullableType(),
                    dbFunction.TypeMapping);
        }

        return _plugins.Concat(_translators)
            .Select(t => t.Translate(instance, method, arguments, logger))
            .FirstOrDefault(t => t != null);
    }

    /// <summary>
    ///     Adds additional translators which will take priority over existing registered translators.
    /// </summary>
    /// <param name="translators">Translators to add.</param>
    protected virtual void AddTranslators(IEnumerable<IMethodCallTranslator> translators)
        => _translators.InsertRange(0, translators);
}
