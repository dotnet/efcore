// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <inheritdoc />
public class RelationalAggregateMethodCallTranslatorProvider : IAggregateMethodCallTranslatorProvider
{
    private readonly List<IAggregateMethodCallTranslator> _plugins = [];
    private readonly List<IAggregateMethodCallTranslator> _translators = [];

    /// <summary>
    ///     Creates a new instance of the <see cref="RelationalAggregateMethodCallTranslatorProvider" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    public RelationalAggregateMethodCallTranslatorProvider(RelationalAggregateMethodCallTranslatorProviderDependencies dependencies)
    {
        Dependencies = dependencies;

        _plugins.AddRange(dependencies.Plugins.SelectMany(p => p.Translators));

        var sqlExpressionFactory = dependencies.SqlExpressionFactory;

        _translators.AddRange(
            new IAggregateMethodCallTranslator[] { new QueryableAggregateMethodTranslator(sqlExpressionFactory) });
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual RelationalAggregateMethodCallTranslatorProviderDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
            IModel model,
            MethodInfo method,
            EnumerableExpression source,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        // TODO: Add support for user defined aggregate functions
        //var dbFunction = model.FindDbFunction(method);
        //if (dbFunction != null)
        //{
        //    if (dbFunction.Translation != null)
        //    {
        //        return dbFunction.Translation.Invoke(
        //            arguments.Select(e => _sqlExpressionFactory.ApplyDefaultTypeMapping(e)).ToList());
        //    }
        //    var argumentsPropagateNullability = dbFunction.Parameters.Select(p => p.PropagatesNullability);
        //    return dbFunction.IsBuiltIn
        //        ? _sqlExpressionFactory.Function(
        //            dbFunction.Name,
        //            arguments,
        //            dbFunction.IsNullable,
        //            argumentsPropagateNullability,
        //            method.ReturnType.UnwrapNullableType(),
        //            dbFunction.TypeMapping)
        //        : _sqlExpressionFactory.Function(
        //            dbFunction.Schema,
        //            dbFunction.Name,
        //            arguments,
        //            dbFunction.IsNullable,
        //            argumentsPropagateNullability,
        //            method.ReturnType.UnwrapNullableType(),
        //            dbFunction.TypeMapping);
        //}
        => _plugins.Concat(_translators)
            .Select(t => t.Translate(method, source, arguments, logger))
            .FirstOrDefault(t => t != null);

    /// <summary>
    ///     Adds additional translators which will take priority over existing registered translators.
    /// </summary>
    /// <param name="translators">Translators to add.</param>
    protected virtual void AddTranslators(IEnumerable<IAggregateMethodCallTranslator> translators)
        => _translators.InsertRange(0, translators);
}
