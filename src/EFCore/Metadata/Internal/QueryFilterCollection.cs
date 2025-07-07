// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class QueryFilterCollection : IReadOnlyCollection<IQueryFilter>
{
    private IQueryFilter? anonymousFilter;    
    private Dictionary<string, IQueryFilter>? filters;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public QueryFilterCollection()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public QueryFilterCollection(IEnumerable<IQueryFilter> filters) => SetRange(filters);

    [MemberNotNullWhen(true, nameof(anonymousFilter))]
    private bool HasAnonymousFilter => anonymousFilter != null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public int Count => HasAnonymousFilter
        ? 1
        : filters?.Count
            ?? 0;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IEnumerator<IQueryFilter> GetEnumerator() => HasAnonymousFilter
        ? new AnonymousFilterEnumerator(anonymousFilter)
        : filters?.Values.GetEnumerator()
            ?? Enumerable.Empty<IQueryFilter>().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IQueryFilter? this[string? filterKey]
    {
        get
        {
            if(filterKey == null)
            {
                return anonymousFilter;
            }
            return filters?.GetValueOrDefault(filterKey);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IQueryFilter? Set(IQueryFilter? filter)
    {        
        if (filter == null)
        {
            Remove();
        }
        else if (filter.Expression == null)
        {
            Remove(filter.Key);
        }
        else if (filter.IsAnonymous)
        {
            if(filters?.Count > 0)
            {
                throw new InvalidOperationException(CoreStrings.AnonymousAndNamedFiltersCombined);
            }
            anonymousFilter = filter;
        }
        else
        {
            if (HasAnonymousFilter)
            {
                throw new InvalidOperationException(CoreStrings.AnonymousAndNamedFiltersCombined);
            }
            (filters ??= [])[filter.Key] = filter;
        }

        return filter;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void SetRange(IEnumerable<IQueryFilter> newFilters)
    {
        if(filters == null && newFilters.TryGetNonEnumeratedCount(out var count) && count > 1)
        {
            filters = new(count);
        }

        foreach (var filter in newFilters)
        {
            Set(filter);
        }
    }

    private void Remove(string? key = null)
    {
        if(key == null)
        {
            anonymousFilter = null;
        }
        else
        {
            filters?.Remove(key);
        }
    }

    sealed class AnonymousFilterEnumerator(IQueryFilter filter) : IEnumerator<IQueryFilter>
    {
        private readonly IQueryFilter _filter = filter;

        int position = -1;

        public IQueryFilter Current => position == 0 ? _filter : null!;

        object IEnumerator.Current => Current;

        public void Dispose() { }
        public bool MoveNext()
        {
            position++;
            return position < 1;
        }
        public void Reset()
        {
            position = -1;
        }
    }
}
