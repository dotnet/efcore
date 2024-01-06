// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <inheritdoc />
public class RelationalMemberTranslatorProvider : IMemberTranslatorProvider
{
    private readonly List<IMemberTranslator> _plugins = [];
    private readonly List<IMemberTranslator> _translators = [];

    /// <summary>
    ///     Creates a new instance of the <see cref="RelationalMemberTranslatorProvider" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    public RelationalMemberTranslatorProvider(RelationalMemberTranslatorProviderDependencies dependencies)
    {
        Dependencies = dependencies;

        _plugins.AddRange(dependencies.Plugins.SelectMany(p => p.Translators));
        _translators
            .AddRange(
                new[] { new NullableMemberTranslator(dependencies.SqlExpressionFactory) });
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual RelationalMemberTranslatorProviderDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        => _plugins.Concat(_translators)
            .Select(t => t.Translate(instance, member, returnType, logger)).FirstOrDefault(t => t != null);

    /// <summary>
    ///     Adds additional translators which will take priority over existing registered translators.
    /// </summary>
    /// <param name="translators">Translators to add.</param>
    protected virtual void AddTranslators(IEnumerable<IMemberTranslator> translators)
        => _translators.InsertRange(0, translators);
}
