// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosQueryTranslationPostprocessor(
    QueryTranslationPostprocessorDependencies dependencies,
    ISqlExpressionFactory sqlExpressionFactory,
    CosmosQueryCompilationContext queryCompilationContext)
    : QueryTranslationPostprocessor(dependencies, queryCompilationContext)
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Expression Process(Expression query)
    {
        query = base.Process(query);

        if (query is ShapedQueryExpression { QueryExpression: SelectExpression selectExpression })
        {
            selectExpression.ApplyProjection();
        }

        var afterValueConverterCompensation = new CosmosValueConverterCompensatingExpressionVisitor(sqlExpressionFactory).Visit(query);
        var afterAliases = queryCompilationContext.AliasManager.PostprocessAliases(afterValueConverterCompensation);
        var afterKeyProjectionOrdering = MoveRootKeyProjectionsFirst(afterAliases);

        ValidateNoTrackingIdentityResolutionOwnedEntityProjection(afterKeyProjectionOrdering);

        var afterExtraction = new CosmosReadItemAndPartitionKeysExtractor().ExtractPartitionKeysAndId(
            queryCompilationContext, sqlExpressionFactory, afterKeyProjectionOrdering);

        return afterExtraction;
    }

    private Expression MoveRootKeyProjectionsFirst(Expression query)
    {
        if (queryCompilationContext.RootEntityType is not { } rootEntityType
            || query is not ShapedQueryExpression
            {
                QueryExpression: SelectExpression selectExpression,
                ShaperExpression: var shaperExpression
            }
            || rootEntityType.FindPrimaryKey()?.Properties
                .ToArray() is not { Length: > 0 } primaryKeyProperties
            || selectExpression.MoveRootKeyProjectionsFirst(rootEntityType, primaryKeyProperties) is not { } oldIndexToNewIndex)
        {
            return query;
        }

        var remappedShaperExpression = new ProjectionBindingIndexRemappingExpressionVisitor(selectExpression, oldIndexToNewIndex)
            .Visit(shaperExpression);

        return ((ShapedQueryExpression)query).UpdateShaperExpression(remappedShaperExpression);
    }

    private void ValidateNoTrackingIdentityResolutionOwnedEntityProjection(Expression query)
    {
        if (queryCompilationContext.QueryTrackingBehavior != QueryTrackingBehavior.NoTrackingWithIdentityResolution
          || queryCompilationContext.RootEntityType is not { } rootEntityType
          || query is not ShapedQueryExpression
              {
                  QueryExpression: SelectExpression selectExpression,
                  ShaperExpression: var shaperExpression
              }
          || rootEntityType.FindPrimaryKey()?.Properties
                .ToArray() is not { Length: > 0 } primaryKeyProperties)
        {
            return;
        }

        var visitor = new NoTrackingIdentityResolutionProjectionVisitor(selectExpression, rootEntityType, primaryKeyProperties);
        visitor.Visit(shaperExpression);

        if (!visitor.ContainsOwnedEntityShaper)
        {
            return;
        }

        var missingKeyProperties = primaryKeyProperties
            .Where(property => !visitor.ProjectedKeyProperties.Contains(property))
            .ToArray();

        if (missingKeyProperties.Length > 0)
        {
            throw new InvalidOperationException(
                CosmosStrings.NoTrackingIdentityResolutionOwnedEntityProjectionMissingOwnerKey(
                    string.Join(", ", missingKeyProperties.Select(property => property.Name)),
                    rootEntityType.DisplayName()));
        }
    }

    private sealed class NoTrackingIdentityResolutionProjectionVisitor(
        SelectExpression selectExpression,
        IEntityType rootEntityType,
        IReadOnlyList<IProperty> primaryKeyProperties)
        : ExpressionVisitor
    {
        public bool ContainsOwnedEntityShaper { get; private set; }

        public HashSet<IProperty> ProjectedKeyProperties { get; } = [];

        public override Expression? Visit(Expression? node)
            => ContainsOwnedEntityShaper
                && ProjectedKeyProperties.Count == primaryKeyProperties.Count
                    ? node
                    : base.Visit(node);

        protected override Expression VisitExtension(Expression node)
        {
            switch (node)
            {
                case StructuralTypeShaperExpression { StructuralType: IEntityType entityType }:
                    if (entityType.IsOwned())
                    {
                        ContainsOwnedEntityShaper = true;
                    }

                    return node;

                case ProjectionBindingExpression projectionBindingExpression:
                    AddProjectedKeyProperty(projectionBindingExpression);
                    break;
            }

            return base.VisitExtension(node);
        }

        private void AddProjectedKeyProperty(ProjectionBindingExpression projectionBindingExpression)
        {
            if (projectionBindingExpression.QueryExpression != selectExpression)
            {
                return;
            }

            var projection = selectExpression.Projection[GetProjectionIndex(projectionBindingExpression)].Expression;

            if (projection is not ScalarAccessExpression
                {
                    Object: ObjectReferenceExpression { StructuralType: IEntityType entityType },
                    PropertyName: var propertyName
                }
                || !IsRootEntityType(entityType))
            {
                return;
            }

            foreach (var primaryKeyProperty in primaryKeyProperties)
            {
                if (string.Equals(primaryKeyProperty.GetJsonPropertyName(), propertyName, StringComparison.Ordinal))
                {
                    ProjectedKeyProperties.Add(primaryKeyProperty);
                    return;
                }
            }
        }

        private int GetProjectionIndex(ProjectionBindingExpression projectionBindingExpression)
            => projectionBindingExpression.ProjectionMember is not null
                ? selectExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember).GetConstantValue<int>()
                : projectionBindingExpression.Index
                    ?? throw new InvalidOperationException(CoreStrings.TranslationFailed(projectionBindingExpression.Print()));

        private bool IsRootEntityType(IEntityType entityType)
            => entityType == rootEntityType
                || entityType.GetRootType() == rootEntityType.GetRootType();
    }

    private sealed class ProjectionBindingIndexRemappingExpressionVisitor(
        SelectExpression selectExpression,
        IReadOnlyList<int> oldIndexToNewIndex)
        : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression node)
        {
            switch (node)
            {
                case ShapedQueryExpression:
                    return node;

                case ProjectionBindingExpression
                {
                    QueryExpression: var queryExpression,
                    ProjectionMember: null,
                    Index: int index
                } projectionBindingExpression
                    when queryExpression == selectExpression:
                    return new ProjectionBindingExpression(
                        selectExpression,
                        oldIndexToNewIndex[index],
                        projectionBindingExpression.Type);
            }

            return base.VisitExtension(node);
        }
    }
}
