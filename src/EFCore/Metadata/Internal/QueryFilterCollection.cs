// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class QueryFilterCollection : IReadOnlyCollection<IQueryFilter>
{
    private IQueryFilter? _anonymousFilter;
    private Dictionary<string, IQueryFilter>? _filters;

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
    public QueryFilterCollection(IEnumerable<IQueryFilter> filters)
        => SetRange(filters);

    [MemberNotNullWhen(true, nameof(_anonymousFilter))]
    private bool HasAnonymousFilter
        => _anonymousFilter != null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public int Count
        => HasAnonymousFilter
            ? 1
            : _filters?.Count
            ?? 0;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IEnumerator<IQueryFilter> GetEnumerator()
        => HasAnonymousFilter
            ? new AnonymousFilterEnumerator(_anonymousFilter)
            : _filters?.Values.GetEnumerator()
            ?? Enumerable.Empty<IQueryFilter>().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IQueryFilter? this[string? filterKey]
        => filterKey == null ? _anonymousFilter : _filters?.GetValueOrDefault(filterKey);

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
            if (_filters?.Count > 0)
            {
                throw new InvalidOperationException(CoreStrings.AnonymousAndNamedFiltersCombined);
            }

            _anonymousFilter = filter;
        }
        else
        {
            if (HasAnonymousFilter)
            {
                throw new InvalidOperationException(CoreStrings.AnonymousAndNamedFiltersCombined);
            }

            (_filters ??= [])[filter.Key] = filter;
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
        if (_filters == null && newFilters.TryGetNonEnumeratedCount(out var count) && count > 1)
        {
            _filters = new Dictionary<string, IQueryFilter>(count);
        }

        foreach (var filter in newFilters)
        {
            Set(filter);
        }
    }

    private void Remove(string? key = null)
    {
        if (key == null)
        {
            _anonymousFilter = null;
        }
        else
        {
            _filters?.Remove(key);
        }
    }

    private sealed class AnonymousFilterEnumerator(IQueryFilter filter) : IEnumerator<IQueryFilter>
    {
        private int _position = -1;

        public IQueryFilter Current
            => _position == 0 ? filter : null!;

        object IEnumerator.Current
            => Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            _position++;
            return _position < 1;
        }

        public void Reset()
            => _position = -1;
    }
}
