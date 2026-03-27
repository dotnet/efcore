// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class CosmosShapedQueryExpressionExtensions
{
    /// <summary>
    ///     If the given <paramref name="source" /> represents wraps an array-returning expression without any additional clauses
    ///     (e.g. filter, ordering...), returns that expression. Otherwise, converts it to an ARRAY() subquery that returns the results
    ///     of the subquery as an array.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public static bool TryConvertToArray(
        this ShapedQueryExpression source,
        ITypeMappingSource typeMappingSource,
        [NotNullWhen(true)] out Expression? array,
        bool ignoreOrderings = false)
        => TryConvertToArray(source, typeMappingSource, out array, out _, ignoreOrderings);

    /// <summary>
    ///     If the given <paramref name="source" /> represents wraps an array-returning expression without any additional clauses
    ///     (e.g. filter, ordering...), returns that expression. Otherwise, converts it to an ARRAY() subquery that returns the results
    ///     of the subquery as an array.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public static bool TryConvertToArray(
        this ShapedQueryExpression source,
        ITypeMappingSource typeMappingSource,
        [NotNullWhen(true)] out Expression? array,
        [NotNullWhen(true)] out Expression? projection,
        bool ignoreOrderings = false)
    {
        if (TryExtractArray(source, out array, out var projectedScalar, ignoreOrderings))
        {
            projection = projectedScalar;
            return true;
        }

        // Otherwise, wrap the subquery with an ARRAY() operator, converting the subquery to an array first.
        if (source.QueryExpression is SelectExpression subquery
            && TryGetProjection(source, out projection))
        {
            subquery.ApplyProjection();

            var arrayClrType = typeof(IEnumerable<>).MakeGenericType(projection.Type);

            switch (projection)
            {
                case StructuralTypeShaperExpression:
                    array = new ObjectArrayExpression(subquery, arrayClrType);
                    return true;

                // TODO: Temporary hack - need to perform proper derivation of the array type mapping from the element (e.g. for
                // value conversion). #34026.
                case SqlExpression sqlExpression:
                    var arrayTypeMapping = typeMappingSource.FindMapping(arrayClrType);

                    array = new ScalarArrayExpression(subquery, arrayClrType, arrayTypeMapping);
                    return true;
            }
        }

        array = null;
        projection = null;
        return false;
    }

    /// <summary>
    ///     If the given <paramref name="source" /> represents wraps an array-returning expression without any additional clauses
    ///     (e.g. filter, ordering...), returns that expression.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public static bool TryExtractArray(
        this ShapedQueryExpression source,
        [NotNullWhen(true)] out Expression? array,
        bool ignoreOrderings = false)
        => TryExtractArray(source, out array, out _, ignoreOrderings);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool TryExtractArray(
        this ShapedQueryExpression source,
        [NotNullWhen(true)] out Expression? array,
        [NotNullWhen(true)] out Expression? projection,
        bool ignoreOrderings = false)
        => TryExtractArray(source, out array, out projection, out _, out var boundMember, ignoreOrderings)
            && boundMember is null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool TryExtractArray(
        this ShapedQueryExpression source,
        [NotNullWhen(true)] out Expression? array,
        [NotNullWhen(true)] out Expression? projection,
        out StructuralTypeShaperExpression? projectedStructuralTypeShaper,
        // On this, see comment in CosmosQueryableMethodTranslatingEV.VisitMethod()
        out Expression? boundMember,
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
            || !TryGetProjection(source, out projection))
        {
            goto ExitFailed;
        }

        // On this, see comment in CosmosQueryableMethodTranslatingEV.VisitMethod()
        switch (projection)
        {
            case ScalarAccessExpression { Object: ObjectReferenceExpression objectRef } scalarAccess:
                projection = objectRef;
                boundMember = scalarAccess;
                break;

            case ObjectAccessExpression { Object: ObjectReferenceExpression objectRef } objectAccess:
                projection = objectRef;
                boundMember = objectAccess;
                break;

            default:
                boundMember = null;
                break;
        }

        if (projection is StructuralTypeShaperExpression shaper)
        {
            projectedStructuralTypeShaper = shaper;
            projection = shaper.ValueBufferExpression;
            if (projection is ProjectionBindingExpression { ProjectionMember: ProjectionMember projectionMember }
                && select.GetMappedProjection(projectionMember) is EntityProjectionExpression entityProjection)
            {
                projection = entityProjection.Object;
            }
        }
        else
        {
            projectedStructuralTypeShaper = null;
        }

        var projectedReferenceName = projection switch
        {
            ScalarReferenceExpression { Name: var name } => name,
            ObjectReferenceExpression { Name: var name } => name,

            _ => null
        };

        if (projectedReferenceName is null)
        {
            goto ExitFailed;
        }

        switch (select)
        {
            // SelectExpressions representing bare arrays are of the form SELECT VALUE i FROM i IN x.
            // Unfortunately, Cosmos doesn't support x being anything but a root container or a property access
            // (e.g. SELECT VALUE i FROM i IN c.SomeArray).
            // For example, x cannot be a function invocation (SELECT VALUE i FROM i IN SetUnion(...)) or an array constant
            // (SELECT VALUE i FROM i IN [1,2,3]).
            // So we wrap any non-property in a subquery as follows: SELECT i FROM i IN (SELECT VALUE [1,2,3]), and that needs to be
            // match here.
            case
            {
                Sources:
                [
                    {
                        WithIn: true,
                        Alias: var sourceAlias,
                        Expression: SelectExpression
                        {
                            Sources: [],
                            Predicate: null,
                            Offset: null,
                            Limit: null,
                            Orderings: [],
                            IsDistinct: false,
                            Projection: [{ Expression: var a }]
                        },
                    }
                ]
            } when projectedReferenceName == sourceAlias:
            {
                array = a;
                return true;
            }

            // For properties: SELECT i FROM i IN c.SomeArray
            // So just match any SelectExpression with IN.
            case { Sources: [{ WithIn: true, Expression: var a, Alias: var sourceAlias }] }
                when projectedReferenceName == sourceAlias:
            {
                array = a;
                return true;
            }
        }

        ExitFailed:
        array = null;
        projection = null;
        projectedStructuralTypeShaper = null;
        boundMember = null;
        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private static bool TryGetProjection(ShapedQueryExpression shapedQueryExpression, [NotNullWhen(true)] out Expression? projection)
    {
        var shaperExpression = shapedQueryExpression.ShaperExpression;
        // No need to check ConvertChecked since this is convert node which we may have added during projection
        if (shaperExpression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression
            && unaryExpression.Operand.Type.IsNullableType()
            && unaryExpression.Operand.Type.UnwrapNullableType() == unaryExpression.Type)
        {
            shaperExpression = unaryExpression.Operand;
        }

        switch (shaperExpression)
        {
            case ProjectionBindingExpression { ProjectionMember: ProjectionMember projectionMember }
                when shapedQueryExpression.QueryExpression is SelectExpression selectExpression:
            {
                projection = selectExpression.GetMappedProjection(projectionMember);
                return true;
            }

            case StructuralTypeShaperExpression shaper:
            {
                projection = shaper;
                return true;
            }
        }

        projection = null;
        return false;
    }
}
