// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosQueryRootProcessor : QueryRootProcessor
{
    private readonly IModel _model;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosQueryRootProcessor(QueryTranslationPreprocessorDependencies dependencies, QueryCompilationContext queryCompilationContext)
        : base(dependencies, queryCompilationContext)
        => _model = queryCompilationContext.Model;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool ShouldConvertToInlineQueryRoot(Expression expression)
        => true;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override bool ShouldConvertToParameterQueryRoot(ParameterExpression parameterExpression)
        => true;

    /// <inheritdoc />
    protected override Expression VisitExtension(Expression node)
        => node switch
        {
            // We skip FromSqlQueryRootExpression, since that contains the arguments as an object array parameter, and don't want to convert
            // that to a query root
            FromSqlQueryRootExpression e => e,

            _ => base.VisitExtension(node)
        };
}
