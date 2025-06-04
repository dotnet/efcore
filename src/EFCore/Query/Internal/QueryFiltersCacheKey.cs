// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class QueryFiltersCacheKey(IEntityType entityType, IReadOnlyCollection<IQueryFilter> queryFilters) : IEquatable<QueryFiltersCacheKey>
{
    private readonly IEntityType _entityType = entityType;
    private readonly int _hashCode = CalculateHashCode(entityType, queryFilters);

    private HashSet<string?> Keys => field ??= [.. queryFilters.Select(x => x.Key)];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool Equals(QueryFiltersCacheKey? other) =>
        ReferenceEquals(_entityType, other?._entityType)
            && Keys.Count == other.Keys.Count
            && Keys.All(other.Keys.Contains);

    private static int CalculateHashCode(IEntityType entityType, IReadOnlyCollection<IQueryFilter> queryFilters)
    {
        var hashCode = new HashCode();
        hashCode.Add(entityType);
        foreach (var filter in queryFilters)
        {
            hashCode.Add(filter.Key);
        }
        return hashCode.ToHashCode();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool Equals(object? obj) => obj is QueryFiltersCacheKey other && Equals(other);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override int GetHashCode() => _hashCode;
}
