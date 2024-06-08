// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class CosmosQueryUtils
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool TryConvertToArray(
        ShapedQueryExpression source,
        ITypeMappingSource typeMappingSource,
        [NotNullWhen(true)] out SqlExpression? array,
        bool ignoreOrderings = false)
        => TryConvertToArray(source, typeMappingSource, out array, out _, ignoreOrderings);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool TryConvertToArray(
        ShapedQueryExpression source,
        ITypeMappingSource typeMappingSource,
        [NotNullWhen(true)] out SqlExpression? array,
        [NotNullWhen(true)] out SqlExpression? projection,
        bool ignoreOrderings = false)
    {
        if (TryExtractBareArray(source, out array, out var projectedScalar, ignoreOrderings))
        {
            projection = projectedScalar;
            return true;
        }

        // Otherwise, wrap the subquery with an ARRAY() operator, converting the subquery to an array first.
        if (source.QueryExpression is SelectExpression subquery
            && TryGetProjection(source, out projection))
        {
            subquery.ApplyProjection();

            // TODO: Should the type be an array, or enumerable/queryable?
            var arrayClrType = projection.Type.MakeArrayType();
            // TODO: Temporary hack - need to perform proper derivation of the array type mapping from the element (e.g. for
            // value conversion).
            var arrayTypeMapping = typeMappingSource.FindMapping(arrayClrType);

            array = new ArrayExpression(subquery, arrayClrType, arrayTypeMapping);
            return true;
        }

        array = null;
        projection = null;
        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool TryExtractBareArray(
        ShapedQueryExpression source,
        [NotNullWhen(true)] out SqlExpression? array,
        bool ignoreOrderings = false)
        => TryExtractBareArray(source, out array, out _, ignoreOrderings);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool TryExtractBareArray(
        ShapedQueryExpression source,
        [NotNullWhen(true)] out SqlExpression? array,
        [NotNullWhen(true)] out SqlExpression? projectedScalarReference,
        bool ignoreOrderings = false)
    {
        if (source.QueryExpression is not SelectExpression
            {
                Predicate: null,
                IsDistinct: false,
                Limit: null,
                Offset: null
            } select
            || (!ignoreOrderings && select.Orderings.Count > 0)
            || !TryGetProjection(source, out var projection)
            || projection is not ScalarReferenceExpression scalarReferenceProjection)
        {
            array = null;
            projectedScalarReference = null;
            return false;
        }

        switch (source.QueryExpression)
        {
            // For properties: SELECT i FROM i IN c.SomeArray
            // So just match any SelectExpression with IN.
            case SelectExpression {
                Sources: [{ WithIn: true, ContainerExpression: SqlExpression a } arraySource],
            } when scalarReferenceProjection.Name == arraySource.Alias:
            {
                array = a;
                projectedScalarReference = scalarReferenceProjection;
                return true;
            }

            // For inline and parameter arrays the case is unfortunately more difficult; Cosmos doesn't allow SELECT i FROM i IN [1,2,3]
            // or SELECT i FROM i IN @p.
            // So we instead generate SELECT i FROM i IN (SELECT VALUE [1,2,3]), which needs to be match here.
            case SelectExpression
            {
                Sources:
                [
                    {
                        WithIn: true,
                        ContainerExpression: SelectExpression
                        {
                            Sources: [],
                            Predicate: null,
                            Offset: null,
                            Limit: null,
                            Orderings: [],
                            IsDistinct: false,
                            UsesSingleValueProjection: true,
                            Projection: [{Expression: SqlExpression a}]
                        },
                    } arraySource
                ]
            } when scalarReferenceProjection.Name == arraySource.Alias:
            {
                array = a;
                projectedScalarReference = scalarReferenceProjection;
                return true;
            }

            default:
                array = null;
                projectedScalarReference = null;
                return false;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool TryGetProjection(
        ShapedQueryExpression shapedQueryExpression,
        [NotNullWhen(true)] out SqlExpression? projectedScalarReference)
    {
        var shaperExpression = shapedQueryExpression.ShaperExpression;
        // No need to check ConvertChecked since this is convert node which we may have added during projection
        if (shaperExpression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression
            && unaryExpression.Operand.Type.IsNullableType()
            && unaryExpression.Operand.Type.UnwrapNullableType() == unaryExpression.Type)
        {
            shaperExpression = unaryExpression.Operand;
        }

        if (shapedQueryExpression.QueryExpression is SelectExpression selectExpression
            && shaperExpression is ProjectionBindingExpression { ProjectionMember: ProjectionMember projectionMember }
            && selectExpression.GetMappedProjection(projectionMember) is SqlExpression projection)
        {
            projectedScalarReference = projection;
            return true;
        }

        projectedScalarReference = null;
        return false;
    }
}
