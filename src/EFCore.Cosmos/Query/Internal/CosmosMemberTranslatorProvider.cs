// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosMemberTranslatorProvider : IMemberTranslatorProvider
{
    private readonly List<IMemberTranslator> _plugins = [];
    private readonly List<IMemberTranslator> _translators = [];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosMemberTranslatorProvider(
        ISqlExpressionFactory sqlExpressionFactory,
        IEnumerable<IMemberTranslatorPlugin> plugins)
    {
        _plugins.AddRange(plugins.SelectMany(p => p.Translators));
        _translators
            .AddRange(
                new IMemberTranslator[]
                {
                    new CosmosStringMemberTranslator(sqlExpressionFactory), new CosmosDateTimeMemberTranslator(sqlExpressionFactory)
                });
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        => _plugins.Concat(_translators)
            .Select(t => t.Translate(instance, member, returnType, logger)).FirstOrDefault(t => t != null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual void AddTranslators(IEnumerable<IMemberTranslator> translators)
        => _translators.InsertRange(0, translators);
}
