// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public readonly struct QueryableJsonProjectionInfo
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public QueryableJsonProjectionInfo(
        Dictionary<IProperty, int> propertyIndexMap,
        List<(JsonProjectionInfo, INavigation)> childrenProjectionInfo)
    {
        PropertyIndexMap = propertyIndexMap;
        ChildrenProjectionInfo = childrenProjectionInfo;
    }

    /// <summary>
    ///     Map between entity properties and corresponding column indexes.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public IDictionary<IProperty, int> PropertyIndexMap { get; }

    /// <summary>
    ///     Information needed to construct each child JSON entity.
    ///     - JsonProjection info (same one we use for simple JSON projection),
    ///     - navigation between parent and the child JSON entity.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public IList<(JsonProjectionInfo JsonProjectionInfo, INavigation Navigation)> ChildrenProjectionInfo { get; }
}
