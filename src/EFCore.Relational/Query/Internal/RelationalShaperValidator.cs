// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

#pragma warning disable EF1001 // Internal EF Core API usage

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
[EntityFrameworkInternal]
public sealed class RelationalShaperValidator : ShaperValidator
{
    private readonly SelectExpression _selectExpression;
    private readonly QueryTrackingBehavior _queryTrackingBehavior;
    private readonly QuerySplittingBehavior? _querySplittingBehavior;
    private readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;

    private int _collectionCount;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <param name="typeMappingSource">The type mapping source.</param>
    /// <param name="selectExpression">The select expression for the shaper.</param>
    /// <param name="queryTrackingBehavior">The query tracking behavior.</param>
    /// <param name="querySplittingBehavior">The query splitting behavior.</param>
    /// <param name="logger">The query logger.</param>
    public RelationalShaperValidator(
        ITypeMappingSource typeMappingSource,
        SelectExpression selectExpression,
        QueryTrackingBehavior queryTrackingBehavior,
        QuerySplittingBehavior? querySplittingBehavior,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        : base(typeMappingSource, queryTrackingBehavior)
    {
        _selectExpression = selectExpression;
        _queryTrackingBehavior = queryTrackingBehavior;
        _querySplittingBehavior = querySplittingBehavior;
        _logger = logger;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void Validate(Expression shaperExpression)
    {
        _collectionCount = 0;

        base.Validate(shaperExpression);

        if (_querySplittingBehavior is null
            && _collectionCount > 1)
        {
            _logger.MultipleCollectionIncludeWarning();
        }

        if (_queryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution)
        {
            new RelationalShapedQueryCompilingExpressionVisitor.ShaperProcessingExpressionVisitor
                .JsonCorrectOrderOfEntitiesForChangeTrackerValidator(_selectExpression).Validate(shaperExpression);
        }
    }

    /// <inheritdoc />
    protected override Expression VisitExtension(Expression node)
    {
        if (node is RelationalCollectionShaperExpression or RelationalSplitCollectionShaperExpression)
        {
            _collectionCount++;
        }

        return base.VisitExtension(node);
    }
}