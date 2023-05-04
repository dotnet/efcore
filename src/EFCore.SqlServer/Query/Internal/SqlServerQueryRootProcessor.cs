// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerQueryRootProcessor : RelationalQueryRootProcessor
{
    private readonly bool _supportsOpenJson;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerQueryRootProcessor(
        QueryTranslationPreprocessorDependencies dependencies,
        RelationalQueryTranslationPreprocessorDependencies relationalDependencies,
        QueryCompilationContext queryCompilationContext,
        ISqlServerSingletonOptions sqlServerSingletonOptions)
        : base(dependencies, relationalDependencies, queryCompilationContext)
        => _supportsOpenJson = sqlServerSingletonOptions.CompatibilityLevel >= 130;

    /// <summary>
    ///     Indicates that a <see cref="ParameterExpression" /> can be converted to a <see cref="ParameterQueryRootExpression" />, if the
    ///     configured SQL Server version supports OPENJSON.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    protected override bool ShouldConvertToParameterQueryRoot(ParameterExpression parameterExpression)
        => _supportsOpenJson;
}
