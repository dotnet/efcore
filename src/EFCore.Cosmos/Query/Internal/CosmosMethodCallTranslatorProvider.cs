// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosMethodCallTranslatorProvider : IMethodCallTranslatorProvider
{
    private readonly List<IMethodCallTranslator> _plugins = [];
    private readonly List<IMethodCallTranslator> _translators = [];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosMethodCallTranslatorProvider(
        ISqlExpressionFactory sqlExpressionFactory,
        IEnumerable<IMethodCallTranslatorPlugin> plugins)
    {
        _plugins.AddRange(plugins.SelectMany(p => p.Translators));

        _translators.AddRange(
            new IMethodCallTranslator[]
            {
                new CosmosEqualsTranslator(sqlExpressionFactory),
                new CosmosStringMethodTranslator(sqlExpressionFactory),
                new CosmosRandomTranslator(sqlExpressionFactory),
                new CosmosMathTranslator(sqlExpressionFactory),
                new CosmosRegexTranslator(sqlExpressionFactory)
                //new LikeTranslator(sqlExpressionFactory),
                //new EnumHasFlagTranslator(sqlExpressionFactory),
                //new GetValueOrDefaultTranslator(sqlExpressionFactory),
                //new ComparisonTranslator(sqlExpressionFactory),
            });
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        IModel model,
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        => _plugins.Concat(_translators)
            .Select(t => t.Translate(instance, method, arguments, logger))
            .FirstOrDefault(t => t != null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual void AddTranslators(IEnumerable<IMethodCallTranslator> translators)
        => _translators.InsertRange(0, translators);
}
